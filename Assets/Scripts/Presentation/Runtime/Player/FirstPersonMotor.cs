using System;
using PCShopEmpire3D.Presentation.Input;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Player
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class FirstPersonMotor : MonoBehaviour
    {
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

        private float _verticalVelocity;
        private float _pitch;

        public bool IsPaused { get; private set; }

        public FirstPersonViewSettings ViewSettings => viewSettings;

        public float WalkSpeed => walkSpeed;

        public float SprintSpeed => sprintSpeed;

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
                playerCamera.fieldOfView = viewSettings.FieldOfView;
            }
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
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
            ApplyViewSettings();
        }

        private void Start()
        {
            SetPaused(false);
        }

        private void Update()
        {
            if (input == null || characterController == null || cameraPivot == null)
            {
                return;
            }

            if (input.PausePressedThisFrame)
            {
                SetPaused(!IsPaused);
            }

            if (IsPaused)
            {
                return;
            }

            UpdateLook(Time.unscaledDeltaTime);
            UpdateMovement(Time.deltaTime);
        }

        private void UpdateLook(float unscaledDeltaTime)
        {
            Vector2 lookDegrees = FirstPersonMath.CalculateLookDegrees(
                input.Look,
                input.IsPointerLook,
                unscaledDeltaTime,
                viewSettings);
            transform.Rotate(0f, lookDegrees.x, 0f, Space.Self);
            _pitch = FirstPersonMath.ClampPitch(_pitch - lookDegrees.y);
            cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void UpdateMovement(float deltaTime)
        {
            Vector2 inputVector = FirstPersonMath.ClampMoveInput(input.Move);
            Vector3 horizontal = (transform.right * inputVector.x) + (transform.forward * inputVector.y);
            float speed = input.SprintHeld ? sprintSpeed : walkSpeed;

            if (characterController.isGrounded && _verticalVelocity < 0f)
            {
                _verticalVelocity = -2f;
            }

            _verticalVelocity += gravity * deltaTime;
            Vector3 velocity = (horizontal * speed) + (Vector3.up * _verticalVelocity);
            characterController.Move(velocity * deltaTime);
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
        }

        private void OnValidate()
        {
            viewSettings ??= new FirstPersonViewSettings();
            walkSpeed = Mathf.Max(0.1f, walkSpeed);
            sprintSpeed = Mathf.Max(walkSpeed, sprintSpeed);
            gravity = Mathf.Min(-0.1f, gravity);
            ApplyViewSettings();
        }
    }
}
