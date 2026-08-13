using PCShopEmpire3D.Presentation.Player;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed class GaragePrototypeHud : MonoBehaviour
    {
        [SerializeField] private FirstPersonMotor motor;

        public void Configure(FirstPersonMotor playerMotor)
        {
            motor = playerMotor;
        }

        private void OnGUI()
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(18f, 14f, 500f, 24f), "PC SHOP EMPIRE 3D — GARAGE PROTOTYPE");
            GUI.Label(new Rect(18f, 36f, 600f, 24f), "WASD / Left Stick: Move   Mouse / Right Stick: Look   Shift: Sprint   Esc: Pause");

            float centerX = Screen.width * 0.5f;
            float centerY = Screen.height * 0.5f;
            GUI.DrawTexture(new Rect(centerX - 7f, centerY - 1f, 14f, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(centerX - 1f, centerY - 7f, 2f, 14f), Texture2D.whiteTexture);

            if (motor != null && motor.IsPaused)
            {
                GUI.Box(
                    new Rect(centerX - 130f, centerY - 45f, 260f, 90f),
                    "PAUSED\nPress Esc / Start to continue");
            }
        }
    }
}
