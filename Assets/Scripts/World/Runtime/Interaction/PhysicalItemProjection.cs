using System;
using System.Linq;
using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.World.Interaction
{
    public sealed class PhysicalItemIdScope : IStableIdScope
    {
    }

    public enum PhysicalItemOwnership
    {
        World = 0,
        PlayerHands = 1,
        Recovery = 2
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PhysicalItemProjection : MonoBehaviour
    {
        [SerializeField] private string itemId = "prototype.item";
        [SerializeField] private string displayName = "Package";
        [SerializeField] private Rigidbody body;
        [SerializeField] private Vector3 carryHalfExtents = new Vector3(0.275f, 0.25f, 0.275f);
        [SerializeField] private Vector3 carryLocalPosition;
        [SerializeField] private Vector3 carryLocalEulerAngles;
        [SerializeField] private float recoveryFloorY = -20f;

        private Transform _worldParent;
        private int _worldLayer;
        private bool _worldUseGravity;
        private bool _worldIsKinematic;
        private bool _worldDetectCollisions;
        private CollisionDetectionMode _worldCollisionMode;
        private RigidbodyInterpolation _worldInterpolation;
        private Collider[] _colliders = Array.Empty<Collider>();
        private bool[] _colliderEnabled = Array.Empty<bool>();
        private int[] _colliderLayers = Array.Empty<int>();
        private bool _hasCarrySnapshot;
        private Vector3 _lastSafePosition;
        private Quaternion _lastSafeRotation = Quaternion.identity;

        public StableId<PhysicalItemIdScope> ItemId => StableId<PhysicalItemIdScope>.Parse(itemId);

        public string ItemIdValue => itemId;

        public string DisplayName => displayName;

        public Rigidbody Body => body;

        public Vector3 CarryHalfExtents => carryHalfExtents;

        public Vector3 DropHalfExtents => Vector3.Scale(carryHalfExtents, Abs(transform.lossyScale));

        public PhysicalItemOwnership Ownership { get; private set; } = PhysicalItemOwnership.World;

        public bool IsCarried => Ownership == PhysicalItemOwnership.PlayerHands;

        public Vector3 LastSafePosition => _lastSafePosition;

        public Quaternion LastSafeRotation => _lastSafeRotation;

        public void Configure(
            string stableItemId,
            string playerFacingName,
            Rigidbody rigidbody,
            Vector3 halfExtents,
            Vector3 localCarryPosition,
            Vector3 localCarryEulerAngles)
        {
            itemId = StableId<PhysicalItemIdScope>.Parse(stableItemId).Value;
            displayName = string.IsNullOrWhiteSpace(playerFacingName)
                ? throw new ArgumentException("A display name is required.", nameof(playerFacingName))
                : playerFacingName;
            body = rigidbody != null ? rigidbody : throw new ArgumentNullException(nameof(rigidbody));
            carryHalfExtents = ClampHalfExtents(halfExtents);
            carryLocalPosition = localCarryPosition;
            carryLocalEulerAngles = localCarryEulerAngles;
            CacheColliders();
            RecordSafePose();
        }

        public OperationResult BeginCarry(Transform carryAnchor, int heldLayer)
        {
            if (carryAnchor == null)
            {
                return OperationResult.Fail(Failure.FromCode("pickup.anchor-missing"));
            }

            if (Ownership != PhysicalItemOwnership.World || _hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("pickup.target-unavailable"));
            }

            EnsureRuntimeReferences();
            OperationResult contract = ValidatePickupContract();
            if (contract.IsFailure)
            {
                return contract;
            }

            CaptureWorldState();
            Ownership = PhysicalItemOwnership.PlayerHands;

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
            SetColliderState(false, heldLayer);
            transform.SetParent(carryAnchor, false);
            transform.localPosition = carryLocalPosition;
            transform.localRotation = Quaternion.Euler(carryLocalEulerAngles);
            return OperationResult.Success();
        }

        public OperationResult ReleaseTo(Pose worldPose)
        {
            if (Ownership != PhysicalItemOwnership.PlayerHands || !_hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("drop.item-not-held"));
            }

            transform.SetParent(_worldParent, true);
            transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            RestoreWorldState();
            Ownership = PhysicalItemOwnership.World;
            _hasCarrySnapshot = false;
            RecordSafePose();
            return OperationResult.Success();
        }

        public OperationResult RecoverToLastSafePose()
        {
            if (Ownership == PhysicalItemOwnership.PlayerHands && !_hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("carry.item-not-held"));
            }

            Ownership = PhysicalItemOwnership.Recovery;
            if (_hasCarrySnapshot)
            {
                transform.SetParent(_worldParent, true);
            }

            transform.SetPositionAndRotation(_lastSafePosition, _lastSafeRotation);
            if (_hasCarrySnapshot)
            {
                RestoreWorldState();
            }
            else
            {
                EnsureRuntimeReferences();
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.WakeUp();
            }

            Physics.SyncTransforms();
            Ownership = PhysicalItemOwnership.World;
            _hasCarrySnapshot = false;
            return OperationResult.Success();
        }

        public void RecordSafePose()
        {
            if (Ownership == PhysicalItemOwnership.World)
            {
                _lastSafePosition = transform.position;
                _lastSafeRotation = transform.rotation;
            }
        }

        private void Awake()
        {
            EnsureRuntimeReferences();
            StableId<PhysicalItemIdScope>.Parse(itemId);
            carryHalfExtents = ClampHalfExtents(carryHalfExtents);
            CacheColliders();
            RecordSafePose();
        }

        private void FixedUpdate()
        {
            if (Ownership != PhysicalItemOwnership.World || body == null)
            {
                return;
            }

            if (!IsFinite(transform.position) || transform.position.y < recoveryFloorY)
            {
                RecoverToLastSafePose();
                return;
            }

            if (body.isKinematic || body.IsSleeping())
            {
                RecordSafePose();
            }
        }

        private void CaptureWorldState()
        {
            _worldParent = transform.parent;
            _worldLayer = gameObject.layer;
            _worldUseGravity = body.useGravity;
            _worldIsKinematic = body.isKinematic;
            _worldDetectCollisions = body.detectCollisions;
            _worldCollisionMode = body.collisionDetectionMode;
            _worldInterpolation = body.interpolation;
            CacheColliders();
            _colliderEnabled = _colliders.Select(collider => collider.enabled).ToArray();
            _colliderLayers = _colliders.Select(collider => collider.gameObject.layer).ToArray();
            _hasCarrySnapshot = true;
        }

        private void RestoreWorldState()
        {
            gameObject.layer = _worldLayer;
            for (int index = 0; index < _colliders.Length; index++)
            {
                Collider collider = _colliders[index];
                if (collider == null)
                {
                    continue;
                }

                collider.gameObject.layer = _colliderLayers[index];
                collider.enabled = _colliderEnabled[index];
            }

            body.isKinematic = _worldIsKinematic;
            body.useGravity = _worldUseGravity;
            body.detectCollisions = _worldDetectCollisions;
            body.collisionDetectionMode = _worldCollisionMode;
            body.interpolation = _worldInterpolation;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.WakeUp();
        }

        private void SetColliderState(bool enabled, int layer)
        {
            gameObject.layer = layer;
            foreach (Collider collider in _colliders)
            {
                if (collider != null)
                {
                    collider.gameObject.layer = layer;
                    collider.enabled = enabled;
                }
            }
        }

        private void CacheColliders()
        {
            _colliders = GetComponentsInChildren<Collider>(true);
        }

        private void EnsureRuntimeReferences()
        {
            body ??= GetComponent<Rigidbody>();
            if (body == null)
            {
                throw new InvalidOperationException("A physical item requires a Rigidbody.");
            }
        }

        private OperationResult ValidatePickupContract()
        {
            Vector3 scale = transform.lossyScale;
            if (!ApproximatelyOne(scale.x) || !ApproximatelyOne(scale.y) || !ApproximatelyOne(scale.z))
            {
                return OperationResult.Fail(Failure.FromCode("pickup.invalid-scale"));
            }

            CacheColliders();
            if (_colliders.Length == 0 || !_colliders.Any(collider => collider != null && collider.enabled))
            {
                return OperationResult.Fail(Failure.FromCode("pickup.missing-collider"));
            }

            if (_colliders.Any(collider =>
                    collider == null || collider.isTrigger || collider.attachedRigidbody != body))
            {
                return OperationResult.Fail(Failure.FromCode("pickup.invalid-collider-ownership"));
            }

            return OperationResult.Success();
        }

        private void OnValidate()
        {
            body ??= GetComponent<Rigidbody>();
            carryHalfExtents = ClampHalfExtents(carryHalfExtents);
        }

        private static Vector3 ClampHalfExtents(Vector3 value)
        {
            return new Vector3(
                Mathf.Max(0.01f, value.x),
                Mathf.Max(0.01f, value.y),
                Mathf.Max(0.01f, value.z));
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(Mathf.Abs(value.x), Mathf.Abs(value.y), Mathf.Abs(value.z));
        }

        private static bool ApproximatelyOne(float value)
        {
            return Mathf.Abs(value - 1f) <= 0.001f;
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }
    }
}
