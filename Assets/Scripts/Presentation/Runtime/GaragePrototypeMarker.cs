using System;
using System.Collections;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed class GaragePrototypeMarker : MonoBehaviour
    {
        public const string ScenePath = "Assets/Scenes/Prototypes/GarageGraybox.unity";
        public const string Version = "garage-shelf-offer-r11-v1";

        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private PlayerCarryController playerCarry;
        [SerializeField] private TransportCartProjection transportCart;
        [SerializeField] private GarageStockFlowRuntime stockFlow;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public PlayerInputAdapter PlayerInput => playerInput;

        public PlayerCarryController PlayerCarry => playerCarry;

        public TransportCartProjection TransportCart => transportCart;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public void Configure(
            FirstPersonMotor motor,
            PlayerInputAdapter input,
            PlayerCarryController carry,
            TransportCartProjection cart,
            GarageStockFlowRuntime garageStockFlow = null)
        {
            playerMotor = motor;
            playerInput = input;
            playerCarry = carry;
            transportCart = cart;
            stockFlow = garageStockFlow;
        }

        private void Start()
        {
            bool hasLargeBox = false;
            int smallBoxCount = 0;
            PhysicalItemProjection[] items = FindObjectsByType<PhysicalItemProjection>(
                FindObjectsSortMode.None);
            foreach (PhysicalItemProjection item in items)
            {
                if (item.CarryProfile == PhysicalCarryProfile.LargeBox)
                {
                    hasLargeBox = true;
                }
                else if (item.CarryProfile == PhysicalCarryProfile.SmallBox)
                {
                    smallBoxCount++;
                }
            }

            bool hasRotationAction = playerInput?.Actions?.FindActionMap(
                PlayerInputContract.PlayerMap,
                false)?.FindAction(PlayerInputContract.RotatePlacement, false) != null;
            bool hasRotationSurface = false;
            PlacementSurface[] surfaces = FindObjectsByType<PlacementSurface>(FindObjectsSortMode.None);
            foreach (PlacementSurface surface in surfaces)
            {
                if (Mathf.Approximately(surface.YawStepDegrees, 90f))
                {
                    hasRotationSurface = true;
                    break;
                }
            }

            Transform[] sceneTransforms = FindObjectsByType<Transform>(FindObjectsSortMode.None);
            bool hasLookdevCorner = false;
            bool hasLookdevVolume = false;
            bool hasTaskLight = false;
            foreach (Transform sceneTransform in sceneTransforms)
            {
                hasLookdevCorner |= sceneTransform.name == "VisualBenchmarkCorner";
                hasLookdevVolume |= sceneTransform.name == "GlobalLookdevVolume";
                hasTaskLight |= sceneTransform.name == "WorkbenchTaskLight";
            }

            bool hasArrivedStockFlow = stockFlow != null &&
                                       stockFlow.Session != null &&
                                       stockFlow.Session.Order.Status ==
                                       PCShopEmpire3D.Orders.PurchaseOrderStatus.Arrived;
            bool hasShelfOfferAuthority = hasArrivedStockFlow &&
                                          stockFlow.Session.RetailOffers != null &&
                                          stockFlow.Session.RetailOffers.Count == 0;

            Debug.Log(
                $"GARAGE_GRAYBOX_RUNTIME_READY version={Version} " +
                $"scene={gameObject.scene.name} resolution={Screen.width}x{Screen.height} " +
                $"motor={(playerMotor != null ? "ok" : "missing")} " +
                $"input={(playerInput != null && playerInput.Actions != null ? "ok" : "missing")} " +
                $"carry={(playerCarry != null ? "ok" : "missing")} " +
                $"placement={(playerCarry != null && playerCarry.PlacementPreview != null ? "ok" : "missing")} " +
                $"large-carry={(hasLargeBox ? "ok" : "missing")} " +
                $"rotation={(hasRotationAction && hasRotationSurface ? "ok" : "missing")} " +
                $"stacking={(smallBoxCount >= 2 ? "ok" : "missing")} " +
                $"transport-cart={(transportCart != null ? "ok" : "missing")} " +
                $"inventory-flow={(hasArrivedStockFlow ? "arrived" : "missing")} " +
                $"parcel={(stockFlow?.Parcel != null && stockFlow.Parcel.IsSealed ? "sealed" : "missing")} " +
                $"shelf-offer={(hasShelfOfferAuthority ? "ready" : "missing")} " +
                $"lookdev={(hasLookdevCorner && hasLookdevVolume && hasTaskLight ? "ok" : "missing")}");

            if (Debug.isDebugBuild && HasCommandLineArgument("-pse-cart-smoke"))
            {
                StartCoroutine(RunTransportCartSmoke());
            }

            if (HasCommandLineArgument("-pse-stock-flow-smoke"))
            {
                Application.runInBackground = true;
                StartCoroutine(RunStockFlowSmoke());
            }
        }

        private IEnumerator RunStockFlowSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            InventoryItemWorldBinding binding = stockFlow != null
                ? stockFlow.ItemBinding
                : null;
            DeliveryParcelProjection parcel = binding != null ? binding.Parcel : null;
            PhysicalItemProjection item = binding != null ? binding.Projection : null;
            if (playerMotor == null || playerCarry == null || session == null || item == null || parcel == null)
            {
                Debug.LogError(
                    "GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed code=smoke.context-missing");
                yield break;
            }

            if (session.Order.Status != PCShopEmpire3D.Orders.PurchaseOrderStatus.Arrived ||
                session.TryGetItem(out _) ||
                session.Inventory.GetTotalQuantity(session.ProductId).Value != 0 ||
                !parcel.IsSealed)
            {
                Debug.LogError(
                    "GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed code=smoke.arrival-contract");
                yield break;
            }

            OperationResult accept = playerCarry.TryPickup(item);
            if (accept.IsFailure || playerCarry.HeldItem != null)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(accept.IsFailure ? accept.Error.Code : "smoke.accept-carried")}");
                yield break;
            }

            long inventoryRevisionBeforeOpen = session.Inventory.Revision;
            long orderRevisionBeforeOpen = session.Orders.Revision;
            OperationResult open = playerCarry.TryPickup(item);
            OperationResult repeatedOpen = binding.TryOpenParcel();
            if (open.IsFailure || repeatedOpen.IsFailure || playerCarry.HeldItem != null ||
                !parcel.IsOpened || parcel.OpenTransitionCount != 1 ||
                session.Inventory.Revision != inventoryRevisionBeforeOpen ||
                session.Orders.Revision != orderRevisionBeforeOpen)
            {
                string parcelFailureCode = open.IsFailure
                    ? open.Error.Code
                    : repeatedOpen.IsFailure
                        ? repeatedOpen.Error.Code
                        : "smoke.parcel-contract";
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={parcelFailureCode}");
                yield break;
            }

            OperationResult pickup = playerCarry.TryPickup(item);
            if (pickup.IsFailure || playerCarry.HeldItem != item)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(pickup.IsFailure ? pickup.Error.Code : "smoke.pickup-missing")}");
                yield break;
            }

            SetPlayerPose(new Vector3(0f, 0.05f, -2.5f), Quaternion.identity);
            OperationResult drop = playerCarry.TryDrop();
            bool validInventory = session.TryGetItem(out PCShopEmpire3D.Inventory.InventoryItemRecord record) &&
                                  record.Id == session.ItemId &&
                                  record.ContainerId == session.WorldFloorContainerId &&
                                  session.Inventory.GetTotalQuantity(session.ProductId).Value == 1;
            if (drop.IsFailure || playerCarry.HeldItem != null || !validInventory)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(drop.IsFailure ? drop.Error.Code : "smoke.inventory-mismatch")}");
                yield break;
            }

            long inventoryRevisionBeforeOffer = session.Inventory.Revision;
            long orderRevisionBeforeOffer = session.Orders.Revision;
            long retailRevisionBeforeOffer = session.RetailOffers.Revision;
            OperationResult publishOffer = session.PublishShelfOffer();
            OperationResult repeatedOffer = session.PublishShelfOffer();
            stockFlow.RefreshPresentation();
            PCShopEmpire3D.Retail.ShelfOfferRecord offer = null;
            bool validOffer = publishOffer.IsSuccess &&
                              repeatedOffer.IsSuccess &&
                              session.TryGetShelfOffer(out offer) &&
                              offer.Id == session.ShelfOfferId &&
                              offer.Price.MinorUnits == GarageStockFlowSession.PrototypePriceMinorUnits &&
                              offer.Price.Currency.Value == GarageStockFlowSession.PrototypeCurrencyCode &&
                              session.RetailOffers.Revision == retailRevisionBeforeOffer + 1 &&
                              session.Inventory.Revision == inventoryRevisionBeforeOffer &&
                              session.Orders.Revision == orderRevisionBeforeOffer &&
                              stockFlow.ShelfOfferText != null &&
                              stockFlow.ShelfOfferText.text == stockFlow.ShelfOfferLabelText;
            if (!validOffer)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(publishOffer.IsFailure ? publishOffer.Error.Code : "smoke.shelf-offer-contract")}");
                yield break;
            }

            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(12f, 0f, 0f);
            }

            Debug.Log(
                $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=ok accepted=ok parcel-open=ok carry=ok " +
                $"world-floor=ok shelf-offer=ok price-minor={offer.Price.MinorUnits} " +
                $"currency={offer.Price.Currency.Value} " +
                $"stable={(item.ItemIdValue == session.ItemId.Value ? "ok" : "missing")} " +
                $"quantity={session.Inventory.GetTotalQuantity(session.ProductId).Value}");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator RunTransportCartSmoke()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            PhysicalItemProjection largeBox = null;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
                         FindObjectsSortMode.None))
            {
                if (item.CarryProfile == PhysicalCarryProfile.LargeBox)
                {
                    largeBox = item;
                    break;
                }
            }

            if (playerMotor == null || playerCarry == null || transportCart == null || largeBox == null)
            {
                Debug.LogError("GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code=smoke.context-missing");
                yield break;
            }

            playerMotor.SetPaused(false);
            string itemIdentity = largeBox.ItemIdValue;
            OperationResult pickup = playerCarry.TryPickup(largeBox);
            if (pickup.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={pickup.Error.Code}");
                yield break;
            }

            OperationResult load = playerCarry.TryLoadHeldItem(transportCart);
            if (load.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={load.Error.Code}");
                yield break;
            }

            MovePlayerToCartHandle(transportCart, 1.35f);
            OperationResult beginDrive = playerCarry.TryBeginCartDrive(transportCart);
            if (beginDrive.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={beginDrive.Error.Code}");
                yield break;
            }

            MovePlayerBy(transportCart.transform.forward * 0.18f);
            int interactableLayer = LayerMask.NameToLayer("Interactable");
            int playerLayer = LayerMask.NameToLayer("Player");
            OperationResult motion = transportCart.TryFollowDriver(
                1 << 0,
                (1 << 0) | (1 << interactableLayer) | (1 << playerLayer));
            if (motion.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={motion.Error.Code}");
                yield break;
            }

            OperationResult endDrive = playerCarry.TryEndCartDrive();
            if (endDrive.IsFailure)
            {
                Debug.LogError(
                    $"GARAGE_CART_RUNTIME_SMOKE cart-flow=failed code={endDrive.Error.Code}");
                yield break;
            }

            MovePlayerToCartHandle(transportCart, 1.45f);
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(18f, 0f, 0f);
            }

            Physics.SyncTransforms();
            Debug.Log(
                $"GARAGE_CART_RUNTIME_SMOKE cart-flow=ok item={itemIdentity} " +
                $"loaded={(transportCart.Cargo == largeBox ? "ok" : "missing")} " +
                $"stable={(largeBox.IsMountedOnTransportCart ? "ok" : "missing")}");

            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToCartHandle(TransportCartProjection cart, float distance)
        {
            Vector3 handle = cart.transform.TransformPoint(new Vector3(0f, 0f, -0.60f));
            Vector3 playerPosition = handle - (cart.transform.forward * distance);
            playerPosition.y = 0.05f;
            SetPlayerPose(playerPosition, Quaternion.Euler(0f, cart.transform.eulerAngles.y, 0f));
        }

        private void MovePlayerBy(Vector3 delta)
        {
            SetPlayerPose(playerMotor.transform.position + delta, playerMotor.transform.rotation);
        }

        private void SetPlayerPose(Vector3 position, Quaternion rotation)
        {
            CharacterController controller = playerMotor.GetComponent<CharacterController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            playerMotor.transform.SetPositionAndRotation(position, rotation);
            if (controller != null)
            {
                controller.enabled = true;
            }

            Physics.SyncTransforms();
        }

        private static bool HasCommandLineArgument(string argument)
        {
            return Array.Exists(
                Environment.GetCommandLineArgs(),
                candidate => string.Equals(candidate, argument, StringComparison.Ordinal));
        }

    }
}
