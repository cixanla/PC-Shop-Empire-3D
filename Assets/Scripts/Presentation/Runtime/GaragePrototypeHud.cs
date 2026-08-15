using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed class GaragePrototypeHud : MonoBehaviour
    {
        [SerializeField] private FirstPersonMotor motor;
        [SerializeField] private PlayerCarryController carryController;
        [SerializeField] private GarageStockFlowRuntime stockFlow;

        public void Configure(
            FirstPersonMotor playerMotor,
            PlayerCarryController playerCarryController,
            GarageStockFlowRuntime garageStockFlow = null)
        {
            motor = playerMotor;
            carryController = playerCarryController;
            stockFlow = garageStockFlow;
        }

        private void OnGUI()
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(18f, 14f, 500f, 24f), "PC SHOP EMPIRE 3D — GARAGE PROTOTYPE");
            GUI.Label(new Rect(18f, 36f, 760f, 24f), "WASD / Left Stick: Move   Mouse / Right Stick: Look   Shift: Sprint   Esc: Pause");

            if (stockFlow != null)
            {
                const float panelWidth = 370f;
                GUI.Box(
                    new Rect(Screen.width - panelWidth - 18f, 14f, panelWidth, 62f),
                    stockFlow.StatusText);
            }

            string prompt = carryController != null ? carryController.PromptText : string.Empty;
            if (!string.IsNullOrEmpty(prompt) && (motor == null || !motor.IsPaused))
            {
                float promptWidth = Mathf.Min(900f, Screen.width - 24f);
                GUI.Box(
                    new Rect(
                        (Screen.width - promptWidth) * 0.5f,
                        (Screen.height * 0.5f) + 34f,
                        promptWidth,
                        34f),
                    prompt);
            }

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
