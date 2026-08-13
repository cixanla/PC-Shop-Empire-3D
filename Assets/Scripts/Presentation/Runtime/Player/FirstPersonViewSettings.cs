using System;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Player
{
    [Serializable]
    public sealed class FirstPersonViewSettings
    {
        public const float MinimumFieldOfView = 60f;
        public const float MaximumFieldOfView = 100f;
        public const float MinimumMouseSensitivity = 0.01f;
        public const float MaximumMouseSensitivity = 1f;
        public const float MinimumGamepadLookSpeed = 20f;
        public const float MaximumGamepadLookSpeed = 720f;

        [SerializeField, Range(MinimumFieldOfView, MaximumFieldOfView)]
        private float fieldOfView = 72f;

        [SerializeField, Range(MinimumMouseSensitivity, MaximumMouseSensitivity)]
        private float mouseSensitivity = 0.08f;

        [SerializeField, Range(MinimumGamepadLookSpeed, MaximumGamepadLookSpeed)]
        private float gamepadLookSpeed = 160f;

        [SerializeField] private bool invertY;
        [SerializeField] private bool motionReduced = true;

        public float FieldOfView => fieldOfView;

        public float MouseSensitivity => mouseSensitivity;

        public float GamepadLookSpeed => gamepadLookSpeed;

        public bool InvertY => invertY;

        public bool MotionReduced => motionReduced;

        public void Set(
            float requestedFieldOfView,
            float requestedMouseSensitivity,
            float requestedGamepadLookSpeed,
            bool requestedInvertY,
            bool requestedMotionReduced)
        {
            fieldOfView = Mathf.Clamp(
                requestedFieldOfView,
                MinimumFieldOfView,
                MaximumFieldOfView);
            mouseSensitivity = Mathf.Clamp(
                requestedMouseSensitivity,
                MinimumMouseSensitivity,
                MaximumMouseSensitivity);
            gamepadLookSpeed = Mathf.Clamp(
                requestedGamepadLookSpeed,
                MinimumGamepadLookSpeed,
                MaximumGamepadLookSpeed);
            invertY = requestedInvertY;
            motionReduced = requestedMotionReduced;
        }

        public void ClampToSupportedRange()
        {
            Set(fieldOfView, mouseSensitivity, gamepadLookSpeed, invertY, motionReduced);
        }
    }
}
