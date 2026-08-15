using System;
using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.World.Interaction
{
    public sealed class TransportCartIdScope : IStableIdScope
    {
    }

    public enum TransportCartState
    {
        World = 0,
        PlayerGrip = 1,
        Recovery = 2
    }

    public static class TransportCartRules
    {
        public const float UnloadedMovementSpeedMultiplier = 0.90f;
        public const float LoadedMovementSpeedMultiplier = 0.85f;
        public const float MinimumGripDistance = 0.90f;
        public const float MaximumGripDistance = 2.25f;
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class TransportCartProjection : MonoBehaviour
    {
        private const float MotionFloorClearance = 0.04f;

        [SerializeField] private string cartId = "prototype.transport-cart";
        [SerializeField] private string displayName = "Platform Arabası";
        [SerializeField] private Rigidbody body;
        [SerializeField] private Transform cargoAnchor;
        [SerializeField] private Vector3 unloadedMotionHalfExtents = new Vector3(0.62f, 0.72f, 0.78f);
        [SerializeField] private Vector3 loadedMotionHalfExtents = new Vector3(0.62f, 0.82f, 0.78f);
        [SerializeField] private float recoveryFloorY = -20f;

        private Transform _worldParent;
        private Transform _driver;
        private Vector3 _driverLocalPosition;
        private float _driverYawOffset;
        private Vector3 _lastSafePosition;
        private Quaternion _lastSafeRotation = Quaternion.identity;
        private bool _applicationQuitting;
        private string _cargoItemId = string.Empty;

        public StableId<TransportCartIdScope> CartId => StableId<TransportCartIdScope>.Parse(cartId);

        public string CartIdValue => cartId;

        public string DisplayName => displayName;

        public Rigidbody Body => body;

        public Transform CargoAnchor => cargoAnchor;

        public PhysicalItemProjection Cargo { get; private set; }

        public bool HasCargo => Cargo != null;

        public string CargoItemIdValue => _cargoItemId;

        public bool IsDriven => State == TransportCartState.PlayerGrip;

        public TransportCartState State { get; private set; } = TransportCartState.World;

        public Vector3 MotionHalfExtents => HasCargo
            ? loadedMotionHalfExtents
            : unloadedMotionHalfExtents;

        public float MovementSpeedMultiplier => HasCargo
            ? TransportCartRules.LoadedMovementSpeedMultiplier
            : TransportCartRules.UnloadedMovementSpeedMultiplier;

        public Vector3 LastSafePosition => _lastSafePosition;

        public Quaternion LastSafeRotation => _lastSafeRotation;

        public string LastMotionFailureCode { get; private set; } = string.Empty;

        public void Configure(
            string stableCartId,
            string playerFacingName,
            Rigidbody rigidbody,
            Transform loadAnchor,
            Vector3 unloadedHalfExtents,
            Vector3 loadedHalfExtents)
        {
            cartId = StableId<TransportCartIdScope>.Parse(stableCartId).Value;
            displayName = string.IsNullOrWhiteSpace(playerFacingName)
                ? throw new ArgumentException("A display name is required.", nameof(playerFacingName))
                : playerFacingName;
            body = rigidbody != null ? rigidbody : throw new ArgumentNullException(nameof(rigidbody));
            cargoAnchor = loadAnchor != null
                ? loadAnchor
                : throw new ArgumentNullException(nameof(loadAnchor));
            unloadedMotionHalfExtents = ClampHalfExtents(unloadedHalfExtents);
            loadedMotionHalfExtents = ClampHalfExtents(loadedHalfExtents);
            _worldParent = transform.parent;
            RecordSafePose();
        }

        public bool CanLoad(PhysicalItemProjection item)
        {
            return State == TransportCartState.World &&
                   !HasCargo &&
                   string.IsNullOrEmpty(_cargoItemId) &&
                   item != null &&
                   item.IsCarried &&
                   item.CarryProfile == PhysicalCarryProfile.LargeBox;
        }

        public OperationResult TryLoad(PhysicalItemProjection item, int mountedLayer)
        {
            if (State != TransportCartState.World)
            {
                return OperationResult.Fail(Failure.FromCode("cart.load-while-driven"));
            }

            if (HasCargo || !string.IsNullOrEmpty(_cargoItemId))
            {
                return OperationResult.Fail(Failure.FromCode("cart.load-slot-occupied"));
            }

            if (item == null)
            {
                return OperationResult.Fail(Failure.FromCode("cart.load-no-item"));
            }

            if (item.CarryProfile != PhysicalCarryProfile.LargeBox)
            {
                return OperationResult.Fail(Failure.FromCode("cart.load-profile-unsupported"));
            }

            OperationResult result = item.MountOnTransportCart(cargoAnchor, mountedLayer);
            if (result.IsFailure)
            {
                return result;
            }

            Cargo = item;
            _cargoItemId = item.ItemIdValue;
            SyncCargoPose();
            LastMotionFailureCode = string.Empty;
            RecordSafePose();
            return OperationResult.Success();
        }

        public OperationResult<PhysicalItemProjection> TryUnload(
            Transform carryAnchor,
            int heldLayer)
        {
            if (State != TransportCartState.World)
            {
                return OperationResult<PhysicalItemProjection>.Fail(
                    Failure.FromCode("cart.unload-while-driven"));
            }

            if (!HasCargo)
            {
                return OperationResult<PhysicalItemProjection>.Fail(
                    Failure.FromCode("cart.unload-empty"));
            }

            PhysicalItemProjection item = Cargo;
            OperationResult result = item.TransferFromTransportCartToCarry(carryAnchor, heldLayer);
            if (result.IsFailure)
            {
                return OperationResult<PhysicalItemProjection>.Fail(result.Error);
            }

            Cargo = null;
            _cargoItemId = string.Empty;
            LastMotionFailureCode = string.Empty;
            return OperationResult<PhysicalItemProjection>.Success(item);
        }

        public OperationResult BeginDrive(Transform driver)
        {
            if (driver == null)
            {
                return OperationResult.Fail(Failure.FromCode("cart.driver-missing"));
            }

            if (State != TransportCartState.World)
            {
                return OperationResult.Fail(Failure.FromCode("cart.driver-unavailable"));
            }

            Vector3 planarDelta = transform.position - driver.position;
            planarDelta.y = 0f;
            if (planarDelta.magnitude < TransportCartRules.MinimumGripDistance)
            {
                return OperationResult.Fail(Failure.FromCode("cart.driver-too-close"));
            }

            if (planarDelta.magnitude > TransportCartRules.MaximumGripDistance)
            {
                return OperationResult.Fail(Failure.FromCode("cart.driver-too-far"));
            }

            _driver = driver;
            _driverLocalPosition = driver.InverseTransformPoint(transform.position);
            _driverYawOffset = Mathf.DeltaAngle(driver.eulerAngles.y, transform.eulerAngles.y);
            State = TransportCartState.PlayerGrip;
            LastMotionFailureCode = string.Empty;
            return OperationResult.Success();
        }

        public OperationResult EndDrive()
        {
            if (State != TransportCartState.PlayerGrip)
            {
                return OperationResult.Fail(Failure.FromCode("cart.driver-inactive"));
            }

            _driver = null;
            State = TransportCartState.World;
            LastMotionFailureCode = string.Empty;
            RecordSafePose();
            return OperationResult.Success();
        }

        public OperationResult TryFollowDriver(
            LayerMask supportMask,
            LayerMask obstructionMask)
        {
            if (State != TransportCartState.PlayerGrip || _driver == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-inactive")));
            }

            Vector3 desiredPosition = _driver.TransformPoint(_driverLocalPosition);
            Quaternion desiredRotation = Quaternion.Euler(
                0f,
                _driver.eulerAngles.y + _driverYawOffset,
                0f);
            var evaluation = TransportCartMotionSolver.Evaluate(
                this,
                new Pose(desiredPosition, desiredRotation),
                supportMask,
                obstructionMask,
                _driver);
            if (!evaluation.IsValid)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(evaluation.FailureCode)));
            }

            transform.SetPositionAndRotation(evaluation.Pose.position, evaluation.Pose.rotation);
            body.position = evaluation.Pose.position;
            body.rotation = evaluation.Pose.rotation;
            SyncCargoPose();
            Physics.SyncTransforms();
            RecordSafePose();
            LastMotionFailureCode = string.Empty;
            return OperationResult.Success();
        }

        public OperationResult RecoverToLastSafePose()
        {
            State = TransportCartState.Recovery;
            _driver = null;
            transform.SetParent(_worldParent, true);
            transform.SetPositionAndRotation(_lastSafePosition, _lastSafeRotation);
            body.position = _lastSafePosition;
            body.rotation = _lastSafeRotation;
            SyncCargoPose();
            Physics.SyncTransforms();
            State = TransportCartState.World;
            LastMotionFailureCode = string.Empty;
            return OperationResult.Success();
        }

        public OperationResult RecoverCargoToLastSafeWorldPose()
        {
            if (!HasCargo)
            {
                return OperationResult.Fail(Failure.FromCode("cart.recovery-no-cargo"));
            }

            PhysicalItemProjection item = Cargo;
            OperationResult result = item.RecoverToLastSafePose();
            if (result.IsSuccess)
            {
                Cargo = null;
                _cargoItemId = string.Empty;
            }

            return result;
        }

        public Vector3 GetMotionCenter(Pose pose)
        {
            return pose.position +
                   (pose.rotation * new Vector3(0f, MotionHalfExtents.y + MotionFloorClearance, 0f));
        }

        public void RecordSafePose()
        {
            if (State == TransportCartState.World || State == TransportCartState.PlayerGrip)
            {
                _lastSafePosition = transform.position;
                _lastSafeRotation = transform.rotation;
            }
        }

        private void Awake()
        {
            body ??= GetComponent<Rigidbody>();
            if (body == null)
            {
                throw new InvalidOperationException("A transport cart requires a Rigidbody.");
            }

            if (cargoAnchor == null)
            {
                throw new InvalidOperationException("A transport cart requires a cargo anchor.");
            }

            StableId<TransportCartIdScope>.Parse(cartId);
            unloadedMotionHalfExtents = ClampHalfExtents(unloadedMotionHalfExtents);
            loadedMotionHalfExtents = ClampHalfExtents(loadedMotionHalfExtents);
            _worldParent = transform.parent;
            body.useGravity = false;
            body.isKinematic = true;
            RecordSafePose();
        }

        private void FixedUpdate()
        {
            if (!IsFinite(transform.position) || transform.position.y < recoveryFloorY)
            {
                RecoverToLastSafePose();
                return;
            }

            if (Cargo == null && !string.IsNullOrEmpty(_cargoItemId))
            {
                LastMotionFailureCode = "cart.cargo-projection-missing";
                Debug.LogError(
                    $"TRANSPORT_CART_CARGO_RECOVERY_FAILED cart={cartId} cargo={_cargoItemId}");
                _cargoItemId = string.Empty;
                return;
            }

            if (HasCargo && (!Cargo.gameObject.activeSelf || !Cargo.enabled))
            {
                if (!Cargo.gameObject.activeSelf)
                {
                    Cargo.gameObject.SetActive(true);
                }

                if (!Cargo.enabled)
                {
                    Cargo.enabled = true;
                }

                RecoverCargoToLastSafeWorldPose();
                return;
            }

            SyncCargoPose();
        }

        private void OnDisable()
        {
            if (!Application.isPlaying || _applicationQuitting)
            {
                return;
            }

            _driver = null;
            State = TransportCartState.World;
            if (HasCargo)
            {
                RecoverCargoToLastSafeWorldPose();
            }
        }

        private void OnApplicationQuit()
        {
            _applicationQuitting = true;
        }

        private void OnValidate()
        {
            body ??= GetComponent<Rigidbody>();
            unloadedMotionHalfExtents = ClampHalfExtents(unloadedMotionHalfExtents);
            loadedMotionHalfExtents = ClampHalfExtents(loadedMotionHalfExtents);
        }

        private OperationResult Remember(OperationResult result)
        {
            LastMotionFailureCode = result.IsFailure ? result.Error.Code : string.Empty;
            return result;
        }

        private void SyncCargoPose()
        {
            if (!HasCargo || cargoAnchor == null)
            {
                return;
            }

            Cargo.SyncTransportCartPose(new Pose(cargoAnchor.position, cargoAnchor.rotation));
        }

        private static Vector3 ClampHalfExtents(Vector3 value)
        {
            return new Vector3(
                Mathf.Max(0.05f, value.x),
                Mathf.Max(0.05f, value.y),
                Mathf.Max(0.05f, value.z));
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) && float.IsFinite(value.y) && float.IsFinite(value.z);
        }
    }
}
