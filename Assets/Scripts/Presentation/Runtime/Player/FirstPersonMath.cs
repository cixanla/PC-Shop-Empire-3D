using UnityEngine;

namespace PCShopEmpire3D.Presentation.Player
{
    public static class FirstPersonMath
    {
        public const float MinimumPitch = -85f;
        public const float MaximumPitch = 85f;

        public static float ClampPitch(float pitch)
        {
            return Mathf.Clamp(pitch, MinimumPitch, MaximumPitch);
        }

        public static Vector2 ClampMoveInput(Vector2 input)
        {
            return Vector2.ClampMagnitude(input, 1f);
        }

        public static Vector2 CalculateLookDegrees(
            Vector2 input,
            bool pointerDelta,
            float unscaledDeltaTime,
            FirstPersonViewSettings settings)
        {
            if (settings == null)
            {
                return Vector2.zero;
            }

            float scale = pointerDelta
                ? settings.MouseSensitivity
                : settings.GamepadLookSpeed * Mathf.Max(0f, unscaledDeltaTime);
            float vertical = settings.InvertY ? -input.y : input.y;
            return new Vector2(input.x * scale, vertical * scale);
        }
    }
}
