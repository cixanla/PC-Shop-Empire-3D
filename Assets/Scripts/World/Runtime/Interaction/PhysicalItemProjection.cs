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
        Recovery = 2,
        TransportCart = 3
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PhysicalItemProjection : MonoBehaviour
    {
        [SerializeField] private string itemId = "prototype.item";
        [SerializeField] private string displayName = "Package";
        [SerializeField] private PhysicalCarryProfile carryProfile = PhysicalCarryProfile.SmallBox;
        [SerializeField] private Rigidbody body;
        [SerializeField] private Vector3 carryHalfExtents = new Vector3(0.275f, 0.25f, 0.275f);
        [SerializeField] private Vector3 dropLocalCenter;
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
        private PhysicalItemProjection _stackSupport;
        private PhysicalItemProjection _stackedItem;

        public StableId<PhysicalItemIdScope> ItemId => StableId<PhysicalItemIdScope>.Parse(itemId);

        public string ItemIdValue => itemId;

        public string DisplayName => displayName;

        public PhysicalCarryProfile CarryProfile => carryProfile;

        public PhysicalCarryProfileDefinition CarryProfileDefinition =>
            PhysicalCarryProfileRules.Resolve(carryProfile);

        public bool SupportsPlacement => CarryProfileDefinition.SupportsPlacement;

        public Rigidbody Body => body;

        public Vector3 CarryHalfExtents => carryHalfExtents;

        public Vector3 DropHalfExtents => Vector3.Scale(carryHalfExtents, Abs(transform.lossyScale));

        public Vector3 ResolveDropCenter(Pose worldPose)
        {
            return worldPose.position +
                   (worldPose.rotation * Vector3.Scale(
                       dropLocalCenter,
                       transform.lossyScale));
        }

        public Vector3 InteractionCenter
        {
            get
            {
                EnsureRuntimeReferences();
                bool hasBounds = false;
                Bounds combined = default;
                foreach (Collider itemCollider in _colliders)
                {
                    if (itemCollider == null ||
                        !itemCollider.enabled ||
                        !itemCollider.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (!hasBounds)
                    {
                        combined = itemCollider.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        combined.Encapsulate(itemCollider.bounds);
                    }
                }

                return hasBounds
                    ? combined.center
                    : body != null
                        ? body.position
                        : transform.position;
            }
        }

        public PhysicalItemOwnership Ownership { get; private set; } = PhysicalItemOwnership.World;

        public bool IsCarried => Ownership == PhysicalItemOwnership.PlayerHands;

        public bool IsMountedOnTransportCart => Ownership == PhysicalItemOwnership.TransportCart;

        public Vector3 LastSafePosition => _lastSafePosition;

        public Quaternion LastSafeRotation => _lastSafeRotation;

        public PhysicalItemProjection StackSupport => _stackSupport;

        public PhysicalItemProjection StackedItem => _stackedItem;

        public bool IsStacked => _stackSupport != null;

        public bool HasStackedItem => _stackedItem != null;

        public bool IsStablePlacement => Ownership == PhysicalItemOwnership.World &&
                                         body != null &&
                                         body.isKinematic &&
                                         !body.useGravity;

        public bool CanAcceptStackedItem(PhysicalItemProjection candidate)
        {
            return candidate != null &&
                   candidate != this &&
                   candidate.Ownership == PhysicalItemOwnership.PlayerHands &&
                   candidate.CarryProfile == PhysicalCarryProfile.SmallBox &&
                   !candidate.HasStackedItem &&
                   Ownership == PhysicalItemOwnership.World &&
                   CarryProfile == PhysicalCarryProfile.SmallBox &&
                   IsStablePlacement &&
                   !IsStacked &&
                   !HasStackedItem;
        }

        public void Configure(
            string stableItemId,
            string playerFacingName,
            Rigidbody rigidbody,
            Vector3 halfExtents,
            Vector3 localCarryPosition,
            Vector3 localCarryEulerAngles,
            PhysicalCarryProfile physicalCarryProfile = PhysicalCarryProfile.SmallBox)
        {
            Configure(
                stableItemId,
                playerFacingName,
                rigidbody,
                halfExtents,
                localCarryPosition,
                localCarryEulerAngles,
                physicalCarryProfile,
                Vector3.zero);
        }

        public void Configure(
            string stableItemId,
            string playerFacingName,
            Rigidbody rigidbody,
            Vector3 halfExtents,
            Vector3 localCarryPosition,
            Vector3 localCarryEulerAngles,
            PhysicalCarryProfile physicalCarryProfile,
            Vector3 localDropCenter)
        {
            itemId = StableId<PhysicalItemIdScope>.Parse(stableItemId).Value;
            displayName = string.IsNullOrWhiteSpace(playerFacingName)
                ? throw new ArgumentException("A display name is required.", nameof(playerFacingName))
                : playerFacingName;
            body = rigidbody != null ? rigidbody : throw new ArgumentNullException(nameof(rigidbody));
            PhysicalCarryProfileRules.Resolve(physicalCarryProfile);
            if (!IsFinite(localDropCenter))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(localDropCenter),
                    "Drop center must contain finite values.");
            }

            carryProfile = physicalCarryProfile;
            carryHalfExtents = ClampHalfExtents(halfExtents);
            dropLocalCenter = localDropCenter;
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

            if (HasStackedItem)
            {
                return OperationResult.Fail(Failure.FromCode("pickup.stack-occupied"));
            }

            CaptureWorldState();
            DetachFromStackSupport();
            Ownership = PhysicalItemOwnership.PlayerHands;

            ClearDynamicMotion();
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
            return ReleaseInternal(worldPose, stabilizePlacement: false);
        }

        public OperationResult MountOnTransportCart(Transform loadAnchor, int mountedLayer)
        {
            if (loadAnchor == null)
            {
                return OperationResult.Fail(Failure.FromCode("cart.load-anchor-missing"));
            }

            if (Ownership != PhysicalItemOwnership.PlayerHands || !_hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("cart.load-item-not-held"));
            }

            if (carryProfile != PhysicalCarryProfile.LargeBox)
            {
                return OperationResult.Fail(Failure.FromCode("cart.load-profile-unsupported"));
            }

            Ownership = PhysicalItemOwnership.TransportCart;
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
            SetColliderState(false, mountedLayer);
            transform.SetParent(_worldParent, true);
            transform.SetPositionAndRotation(loadAnchor.position, loadAnchor.rotation);
            return OperationResult.Success();
        }

        public OperationResult SyncTransportCartPose(Pose worldPose)
        {
            if (Ownership != PhysicalItemOwnership.TransportCart || !_hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("cart.cargo-unavailable"));
            }

            transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            return OperationResult.Success();
        }

        public OperationResult TransferFromTransportCartToCarry(
            Transform carryAnchor,
            int heldLayer)
        {
            if (carryAnchor == null)
            {
                return OperationResult.Fail(Failure.FromCode("pickup.anchor-missing"));
            }

            if (Ownership != PhysicalItemOwnership.TransportCart || !_hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("cart.unload-item-unavailable"));
            }

            Ownership = PhysicalItemOwnership.PlayerHands;
            body.useGravity = false;
            body.isKinematic = true;
            body.detectCollisions = false;
            SetColliderState(false, heldLayer);
            transform.SetParent(carryAnchor, false);
            transform.localPosition = carryLocalPosition;
            transform.localRotation = Quaternion.Euler(carryLocalEulerAngles);
            return OperationResult.Success();
        }

        public OperationResult PlaceAt(Pose worldPose)
        {
            return PlaceAt(worldPose, null);
        }

        public OperationResult PlaceAt(
            Pose worldPose,
            PhysicalItemProjection stackSupport)
        {
            return ReleaseInternal(worldPose, stabilizePlacement: true, stackSupport);
        }

        private OperationResult ReleaseInternal(
            Pose worldPose,
            bool stabilizePlacement,
            PhysicalItemProjection stackSupport = null)
        {
            if (Ownership != PhysicalItemOwnership.PlayerHands || !_hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("drop.item-not-held"));
            }

            if (stackSupport != null && !stackSupport.CanAcceptStackedItem(this))
            {
                return OperationResult.Fail(Failure.FromCode("placement.stack-support-unavailable"));
            }

            transform.SetParent(_worldParent, true);
            SetWorldPose(worldPose);
            RestoreWorldState();
            if (stabilizePlacement)
            {
                ClearDynamicMotion();
                body.useGravity = false;
                body.isKinematic = true;
                body.interpolation = RigidbodyInterpolation.None;
                SetWorldPose(worldPose);
            }

            Physics.SyncTransforms();

            Ownership = PhysicalItemOwnership.World;
            _hasCarrySnapshot = false;
            if (stackSupport != null)
            {
                AttachToStackSupport(stackSupport);
            }
            RecordSafePose();
            return OperationResult.Success();
        }

        public OperationResult RecoverToLastSafePose()
        {
            if (Ownership != PhysicalItemOwnership.World && !_hasCarrySnapshot)
            {
                return OperationResult.Fail(Failure.FromCode("carry.item-not-held"));
            }

            Ownership = PhysicalItemOwnership.Recovery;
            if (_hasCarrySnapshot)
            {
                transform.SetParent(_worldParent, true);
            }

            SetWorldPose(new Pose(_lastSafePosition, _lastSafeRotation));
            if (_hasCarrySnapshot)
            {
                RestoreWorldState();
            }
            else
            {
                EnsureRuntimeReferences();
                ClearDynamicMotion();
                if (!body.isKinematic)
                {
                    body.WakeUp();
                }
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

        public OperationResult RestoreLastSafePoseSnapshot(Pose safePose)
        {
            Quaternion rotation = safePose.rotation;
            if (!IsFinite(safePose.position) ||
                !float.IsFinite(rotation.x) ||
                !float.IsFinite(rotation.y) ||
                !float.IsFinite(rotation.z) ||
                !float.IsFinite(rotation.w) ||
                Quaternion.Dot(rotation, rotation) <= Mathf.Epsilon)
            {
                return OperationResult.Fail(
                    Failure.FromCode("recovery.safe-pose-invalid"));
            }

            _lastSafePosition = safePose.position;
            _lastSafeRotation = Quaternion.Normalize(rotation);
            return OperationResult.Success();
        }

        public OperationResult SynchronizeStableWorldPose(Pose worldPose)
        {
            EnsureRuntimeReferences();
            if (Ownership != PhysicalItemOwnership.World || !IsStablePlacement)
            {
                return OperationResult.Fail(
                    Failure.FromCode("recovery.stable-world-pose-unavailable"));
            }

            Quaternion rotation = worldPose.rotation;
            if (!IsFinite(worldPose.position) ||
                !float.IsFinite(rotation.x) ||
                !float.IsFinite(rotation.y) ||
                !float.IsFinite(rotation.z) ||
                !float.IsFinite(rotation.w) ||
                Quaternion.Dot(rotation, rotation) <= Mathf.Epsilon)
            {
                return OperationResult.Fail(
                    Failure.FromCode("recovery.stable-world-pose-invalid"));
            }

            ClearDynamicMotion();
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.None;
            SetWorldPose(new Pose(worldPose.position, Quaternion.Normalize(rotation)));
            Physics.SyncTransforms();
            RecordSafePose();
            return OperationResult.Success();
        }

        private void Awake()
        {
            EnsureRuntimeReferences();
            StableId<PhysicalItemIdScope>.Parse(itemId);
            PhysicalCarryProfileRules.Resolve(carryProfile);
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
            ClearDynamicMotion();
            if (!body.isKinematic)
            {
                body.WakeUp();
            }
        }

        private void SetWorldPose(Pose worldPose)
        {
            transform.SetPositionAndRotation(worldPose.position, worldPose.rotation);
            body.position = worldPose.position;
            body.rotation = worldPose.rotation;
        }

        private void ClearDynamicMotion()
        {
            if (body == null || body.isKinematic)
            {
                return;
            }

            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
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

        private void AttachToStackSupport(PhysicalItemProjection support)
        {
            DetachFromStackSupport();
            _stackSupport = support;
            support._stackedItem = this;
        }

        private void DetachFromStackSupport()
        {
            if (_stackSupport != null && _stackSupport._stackedItem == this)
            {
                _stackSupport._stackedItem = null;
            }

            _stackSupport = null;
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
            Collider[] activeColliders = _colliders.Where(collider =>
                collider != null &&
                collider.enabled &&
                collider.gameObject.activeInHierarchy).ToArray();
            if (activeColliders.Length == 0)
            {
                return OperationResult.Fail(Failure.FromCode("pickup.missing-collider"));
            }

            if (activeColliders.Any(collider =>
                    collider.isTrigger || collider.attachedRigidbody != body))
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
