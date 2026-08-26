using System;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
        private const float ContinuousInputNeutralThresholdSquared = 0.0001f;

        [Header("References")]
        [SerializeField] private CharacterController characterController;
        [SerializeField] private PlayerInputAdapter input;
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private Camera playerCamera;

        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float walkSpeed = 3.5f;
        [SerializeField, Min(0.1f)] private float sprintSpeed = 5.2f;
        [SerializeField] private float gravity = -20f;

        [Header("View")]
        [SerializeField] private FirstPersonViewSettings viewSettings = new FirstPersonViewSettings();
        [SerializeField, Min(1f)] private float fieldOfViewTransitionSpeed = 24f;

        private float _verticalVelocity;
        private float _pitch;
        private PhysicalCarryProfileDefinition _carryProfileDefinition =
            PhysicalCarryProfileRules.Resolve(PhysicalCarryProfile.SmallBox);
        private float _transportCartMovementSpeedMultiplier = 1f;
        private bool _moveRequiresNeutralAfterResume;
        private bool _gamepadLookRequiresNeutralAfterResume;

        public bool IsPaused { get; private set; }

        public FirstPersonViewSettings ViewSettings => viewSettings;

        public float WalkSpeed => walkSpeed;

        public float SprintSpeed => sprintSpeed;

        public PhysicalCarryProfile? ActiveCarryProfile { get; private set; }

        public float CarryMovementSpeedMultiplier =>
            ActiveCarryProfile.HasValue ? _carryProfileDefinition.MovementSpeedMultiplier : 1f;

        public float RequestedCarryFieldOfViewPenalty =>
            ActiveCarryProfile.HasValue ? _carryProfileDefinition.FieldOfViewPenalty : 0f;

        public float AppliedCarryFieldOfViewPenalty => viewSettings.MotionReduced
            ? 0f
            : RequestedCarryFieldOfViewPenalty;

        public bool CarryAllowsSprint =>
            !ActiveCarryProfile.HasValue || _carryProfileDefinition.AllowsSprint;

        public bool IsDrivingTransportCart { get; private set; }

        public float TransportCartMovementSpeedMultiplier =>
            IsDrivingTransportCart ? _transportCartMovementSpeedMultiplier : 1f;

        public bool MovementAllowsSprint => !IsDrivingTransportCart && CarryAllowsSprint;

        public float TargetFieldOfView => Mathf.Clamp(
            viewSettings.FieldOfView - AppliedCarryFieldOfViewPenalty,
            FirstPersonViewSettings.MinimumFieldOfView,
            FirstPersonViewSettings.MaximumFieldOfView);

        public float CurrentHorizontalSpeed => ResolveHorizontalSpeed(input?.SprintHeld ?? false);

        public void Configure(
            CharacterController controller,
            PlayerInputAdapter inputAdapter,
            Transform pivot,
            Camera camera)
        {
            characterController = controller != null
                ? controller
                : throw new ArgumentNullException(nameof(controller));
            input = inputAdapter != null
                ? inputAdapter
                : throw new ArgumentNullException(nameof(inputAdapter));
            cameraPivot = pivot != null
                ? pivot
                : throw new ArgumentNullException(nameof(pivot));
            playerCamera = camera != null
                ? camera
                : throw new ArgumentNullException(nameof(camera));
            ApplyViewSettings();
        }

        public void ApplyViewSettings()
        {
            viewSettings.ClampToSupportedRange();
            if (playerCamera != null)
            {
                playerCamera.fieldOfView = TargetFieldOfView;
            }
        }

        public void ApplyCarryProfile(PhysicalCarryProfile profile)
        {
            _carryProfileDefinition = PhysicalCarryProfileRules.Resolve(profile);
            ActiveCarryProfile = profile;
            if (viewSettings.MotionReduced && playerCamera != null)
            {
                playerCamera.fieldOfView = TargetFieldOfView;
            }
        }

        public void ClearCarryProfile()
        {
            _carryProfileDefinition = PhysicalCarryProfileRules.Resolve(PhysicalCarryProfile.SmallBox);
            ActiveCarryProfile = null;
            if (viewSettings.MotionReduced && playerCamera != null)
            {
                playerCamera.fieldOfView = TargetFieldOfView;
            }
        }

        public void ApplyTransportCartDriveProfile(float movementSpeedMultiplier)
        {
            _transportCartMovementSpeedMultiplier = Mathf.Clamp(
                movementSpeedMultiplier,
                PhysicalCarryProfileRules.MinimumMovementSpeedMultiplier,
                PhysicalCarryProfileRules.MaximumMovementSpeedMultiplier);
            IsDrivingTransportCart = true;
        }

        public void ClearTransportCartDriveProfile()
        {
            _transportCartMovementSpeedMultiplier = 1f;
            IsDrivingTransportCart = false;
        }

        public float ResolveHorizontalSpeed(bool sprintRequested)
        {
            bool sprint = sprintRequested && MovementAllowsSprint;
            float baseSpeed = sprint ? sprintSpeed : walkSpeed;
            return baseSpeed * CarryMovementSpeedMultiplier * TransportCartMovementSpeedMultiplier;
        }

        public void SetPaused(bool paused)
        {
            bool wasPaused = IsPaused;
            IsPaused = paused;
            if (paused)
            {
                _moveRequiresNeutralAfterResume = false;
                _gamepadLookRequiresNeutralAfterResume = false;
            }
            else if (wasPaused && input != null)
            {
                _moveRequiresNeutralAfterResume =
                    input.Move.sqrMagnitude > ContinuousInputNeutralThresholdSquared;
                _gamepadLookRequiresNeutralAfterResume =
                    input.GamepadLook.sqrMagnitude > ContinuousInputNeutralThresholdSquared;
            }

            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
        }

        private void Awake()
        {
            characterController ??= GetComponent<CharacterController>();
            viewSettings ??= new FirstPersonViewSettings();
            walkSpeed = Mathf.Max(0.1f, walkSpeed);
            sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            gravity = Mathf.Min(-0.1f, gravity);
            fieldOfViewTransitionSpeed = Mathf.Max(1f, fieldOfViewTransitionSpeed);
            ApplyViewSettings();
        }

        private void Start()
        {
            SetPaused(false);
        }

        private void Update()
        {
            ProcessInputFrame();
        }

        public void ProcessInputFrame()
        {
            ProcessInputFrame(Time.deltaTime, Time.unscaledDeltaTime);
        }

        internal void ProcessInputFrame(float deltaTime, float unscaledDeltaTime)
        {
            if (input == null || characterController == null || cameraPivot == null)
            {
                return;
            }

            if (input.PausePressedThisFrame)
            {
                SetPaused(!IsPaused);
                return;
            }

            if (IsPaused)
            {
                return;
            }

            UpdateFieldOfView(Mathf.Max(0f, unscaledDeltaTime));
            UpdateLook(
                ReadLookAfterResume(),
                Mathf.Max(0f, unscaledDeltaTime));
            UpdateMovement(
                ReadMoveAfterResume(),
                Mathf.Max(0f, deltaTime));
        }

        private Vector2 ReadMoveAfterResume()
        {
            Vector2 move = input.Move;
            if (!_moveRequiresNeutralAfterResume)
            {
                return move;
            }

            if (move.sqrMagnitude <= ContinuousInputNeutralThresholdSquared)
            {
                _moveRequiresNeutralAfterResume = false;
            }

            return Vector2.zero;
        }

        private Vector2 ReadLookAfterResume()
        {
            Vector2 look = input.Look;
            if (!_gamepadLookRequiresNeutralAfterResume)
            {
                return look;
            }

            if (input.GamepadLook.sqrMagnitude <= ContinuousInputNeutralThresholdSquared)
            {
                _gamepadLookRequiresNeutralAfterResume = false;
                return look;
            }

            return input.IsPointerLook ? look : Vector2.zero;
        }

        private void UpdateLook(Vector2 lookInput, float unscaledDeltaTime)
        {
            Vector2 lookDegrees = FirstPersonMath.CalculateLookDegrees(
                lookInput,
                input.IsPointerLook,
                unscaledDeltaTime,
                viewSettings);
            transform.Rotate(0f, lookDegrees.x, 0f, Space.Self);
            _pitch = FirstPersonMath.ClampPitch(_pitch - lookDegrees.y);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMovement(Vector2 moveInput, float deltaTime)
        {
            Vector2 inputVector = FirstPersonMath.ClampMoveInput(moveInput);
            Vector3 horizontal = (transform.right * inputVector.x) + (transform.forward * inputVector.y);
            float speed = ResolveHorizontalSpeed(input.SprintHeld);

            if (characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += gravity * deltaTime;
            Vector3 velocity = (horizontal * speed) + (Vector3.up * _verticalVelocity);
            characterController.Move(velocity * deltaTime);
        }

        private void UpdateFieldOfView(float unscaledDeltaTime)
        {
            if (playerCamera == null)
            {
                return;
            }

            float target = TargetFieldOfView;
            playerCamera.fieldOfView = viewSettings.MotionReduced
                ? target
                : Mathf.MoveTowards(
                    playerCamera.fieldOfView,
                    target,
                    fieldOfViewTransitionSpeed * Mathf.Max(0f, unscaledDeltaTime));
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && Application.isPlaying)
            {
                SetPaused(true);
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (playerCamera != null)
            {
                playerCamera.fieldOfView = viewSettings.FieldOfView;
            }

            ClearTransportCartDriveProfile();
        }

        private void OnValidate()
        {
            viewSettings ??= new FirstPersonViewSettings();
            walkSpeed = Mathf.Max(0.1f, walkSpeed);
            sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            gravity = Mathf.Min(-0.1f, gravity);
            fieldOfViewTransitionSpeed = Mathf.Max(1f, fieldOfViewTransitionSpeed);
            ApplyViewSettings();
        }
    }
}
