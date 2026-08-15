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
        [SerializeField] private GarageCustomerFlowRuntime customerFlow;
        [SerializeField] private CheckoutStationProjection checkoutStation;

        public CheckoutStationProjection CheckoutStation => checkoutStation;

        public void Configure(
            FirstPersonMotor playerMotor,
            PlayerCarryController playerCarryController,
            GarageStockFlowRuntime garageStockFlow = null,
            GarageCustomerFlowRuntime garageCustomerFlow = null,
            CheckoutStationProjection physicalCheckoutStation = null)
        {
            motor = playerMotor;
            carryController = playerCarryController;
            stockFlow = garageStockFlow;
            customerFlow = garageCustomerFlow;
            checkoutStation = physicalCheckoutStation;
        }

        private void OnGUI()
        {
            GUI.color = Color.white;
            bool compactAssemblyUi = carryController != null &&
                                     (carryController.IsMotherboardSeatMode ||
                                      carryController.HasMotherboardFastenerContext);
            if (!compactAssemblyUi)
            {
                GUI.Label(new Rect(18f, 14f, 500f, 24f), "PC SHOP EMPIRE 3D — GARAGE PROTOTYPE");
                GUI.Label(new Rect(18f, 36f, 760f, 24f), "WASD / Left Stick: Move   Mouse / Right Stick: Look   Shift: Sprint   Esc: Pause");
            }

            if (stockFlow != null && !compactAssemblyUi)
            {
                const float panelWidth = 370f;
                string status = customerFlow != null
                    ? $"{stockFlow.StatusText}\n{customerFlow.StatusText}"
                    : stockFlow.StatusText;
                GUI.Box(
                    new Rect(Screen.width - panelWidth - 18f, 14f, panelWidth, 158f),
                    status);
            }

            string prompt;
            if (compactAssemblyUi)
            {
                prompt = carryController.PromptText;
            }
            else
            {
                prompt = checkoutStation != null
                    ? checkoutStation.PromptText
                    : string.Empty;
                if (string.IsNullOrEmpty(prompt) && customerFlow != null)
                {
                    prompt = customerFlow.ContextualPromptText;
                }

                if (string.IsNullOrEmpty(prompt) && carryController != null)
                {
                    prompt = carryController.PromptText;
                }
            }
            if (!string.IsNullOrEmpty(prompt) && (motor == null || !motor.IsPaused))
            {
                float promptWidth = Mathf.Min(
                    compactAssemblyUi ? 680f : 900f,
                    Screen.width - 24f);
                GUI.Box(
                    new Rect(
                        (Screen.width - promptWidth) * 0.5f,
                        compactAssemblyUi
                            ? Screen.height - 56f
                            : (Screen.height * 0.5f) + 34f,
                        promptWidth,
                        compactAssemblyUi ? 30f : 34f),
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
