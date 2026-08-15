using System;
using System.Collections;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.Retail;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed class GaragePrototypeMarker : MonoBehaviour
    {
        public const string ScenePath = "Assets/Scenes/Prototypes/GarageGraybox.unity";
        public const string Version = "garage-offer-decision-r16-v1";

        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private PlayerCarryController playerCarry;
        [SerializeField] private TransportCartProjection transportCart;
        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private GarageCustomerFlowRuntime customerFlow;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public PlayerInputAdapter PlayerInput => playerInput;

        public PlayerCarryController PlayerCarry => playerCarry;

        public TransportCartProjection TransportCart => transportCart;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public GarageCustomerFlowRuntime CustomerFlow => customerFlow;

        public void Configure(
            FirstPersonMotor motor,
            PlayerInputAdapter input,
            PlayerCarryController carry,
            TransportCartProjection cart,
            GarageStockFlowRuntime garageStockFlow = null,
            GarageCustomerFlowRuntime garageCustomerFlow = null)
        {
            playerMotor = motor;
            playerInput = input;
            playerCarry = carry;
            transportCart = cart;
            stockFlow = garageStockFlow;
            customerFlow = garageCustomerFlow;
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
            bool hasBasketAuthority = hasArrivedStockFlow &&
                                      stockFlow.Session.RetailBaskets != null &&
                                      stockFlow.Session.RetailBaskets.Count == 0;
            bool hasCheckoutAuthority = hasArrivedStockFlow &&
                                        stockFlow.Session.RetailCheckouts != null &&
                                        stockFlow.Session.RetailCheckouts.Count == 0;
            bool hasCheckoutCompletionAuthority = hasCheckoutAuthority &&
                                                  stockFlow.Session.RetailCheckouts.CompletionCount == 0;
            bool hasCustomerVisitAuthority = hasArrivedStockFlow &&
                                             stockFlow.Session.CustomerVisits != null &&
                                             stockFlow.Session.CustomerVisits.Count == 0;
            bool hasCustomerNavigation = customerFlow != null &&
                                         customerFlow.NavigationReady &&
                                         customerFlow.CustomerAgent != null;

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
                $"basket-reservation={(hasBasketAuthority ? "ready" : "missing")} " +
                $"checkout-snapshot={(hasCheckoutAuthority ? "ready" : "missing")} " +
                $"checkout-completion={(hasCheckoutCompletionAuthority ? "ready" : "missing")} " +
                $"customer-visit={(hasCustomerVisitAuthority ? "ready" : "missing")} " +
                $"customer-navmesh={(hasCustomerNavigation ? "ready" : "missing")} " +
                $"lookdev={(hasLookdevCorner && hasLookdevVolume && hasTaskLight ? "ok" : "missing")}");

            bool runCartSmoke = Debug.isDebugBuild && HasCommandLineArgument("-pse-cart-smoke");
            bool runStockFlowSmoke = HasCommandLineArgument("-pse-stock-flow-smoke");
            bool runCustomerFlowSmoke = HasCommandLineArgument("-pse-customer-flow-smoke");
            int smokeCount = (runCartSmoke ? 1 : 0) +
                             (runStockFlowSmoke ? 1 : 0) +
                             (runCustomerFlowSmoke ? 1 : 0);
            if (smokeCount > 1)
            {
                Debug.LogError("GARAGE_RUNTIME_SMOKE smoke=failed code=smoke.conflicting-flags");
                return;
            }

            if (runCartSmoke)
            {
                StartCoroutine(RunTransportCartSmoke());
            }

            if (runStockFlowSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunStockFlowSmoke());
            }

            if (runCustomerFlowSmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunCustomerFlowSmoke());
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

            GarageStockFlowSession basketSession = GarageStockFlowSession.CreateArrived();
            OperationResult basketAccept = basketSession.AcceptArrivedDelivery();
            OperationResult basketShelfTransfer = basketSession.TransferItem(
                basketSession.ShelfContainerId);
            OperationResult basketOffer = basketSession.PublishShelfOffer();
            long basketInventoryBefore = basketSession.Inventory.Revision;
            long basketRetailBefore = basketSession.RetailBaskets.Revision;
            long basketOffersBefore = basketSession.RetailOffers.Revision;
            long basketOrdersBefore = basketSession.Orders.Revision;
            OperationResult basketReserve = basketSession.ReservePrototypeCustomerBasket();
            OperationResult basketRepeat = basketSession.ReservePrototypeCustomerBasket();
            bool basketReserved =
                basketAccept.IsSuccess &&
                basketShelfTransfer.IsSuccess &&
                basketOffer.IsSuccess &&
                basketReserve.IsSuccess &&
                basketRepeat.IsSuccess &&
                basketSession.TryGetPrototypeBasketLine(out var basketLine) &&
                basketLine.ItemId == basketSession.ItemId &&
                basketLine.OfferId == basketSession.ShelfOfferId &&
                basketLine.CustomerId == basketSession.PrototypeCustomerId &&
                basketSession.Inventory.TryGetReservation(
                    basketSession.PrototypeReservationId,
                    out InventoryReservation reservation) &&
                reservation.ItemId == basketSession.ItemId &&
                reservation.ClaimId == basketSession.PrototypeClaimId &&
                basketSession.Inventory.GetAvailableQuantity(basketSession.ProductId).Value == 0 &&
                basketSession.Inventory.GetTotalQuantity(basketSession.ProductId).Value == 1 &&
                basketSession.Inventory.Revision == basketInventoryBefore + 1 &&
                basketSession.RetailBaskets.Revision == basketRetailBefore + 1 &&
                basketSession.RetailOffers.Revision == basketOffersBefore &&
                basketSession.Orders.Revision == basketOrdersBefore &&
                basketSession.ValidateInvariants().IsSuccess;
            if (!basketReserved)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(basketReserve.IsFailure ? basketReserve.Error.Code : "smoke.basket-reservation-contract")}");
                yield break;
            }

            long inventoryBeforeRelease = basketSession.Inventory.Revision;
            long retailBeforeRelease = basketSession.RetailBaskets.Revision;
            OperationResult basketRelease = basketSession.ReleasePrototypeCustomerBasket();
            bool basketReleased = basketRelease.IsSuccess &&
                                  basketSession.RetailBaskets.Count == 0 &&
                                  basketSession.Inventory.ReservationCount == 0 &&
                                  basketSession.Inventory.GetAvailableQuantity(
                                      basketSession.ProductId).Value == 1 &&
                                  basketSession.Inventory.GetTotalQuantity(
                                      basketSession.ProductId).Value == 1 &&
                                  basketSession.Inventory.Revision == inventoryBeforeRelease + 1 &&
                                  basketSession.RetailBaskets.Revision == retailBeforeRelease + 1 &&
                                  basketSession.ValidateInvariants().IsSuccess;
            if (!basketReleased)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(basketRelease.IsFailure ? basketRelease.Error.Code : "smoke.basket-release-contract")}");
                yield break;
            }

            GarageStockFlowSession checkoutSession = GarageStockFlowSession.CreateArrived();
            OperationResult checkoutAccept = checkoutSession.AcceptArrivedDelivery();
            OperationResult checkoutShelfTransfer = checkoutSession.TransferItem(
                checkoutSession.ShelfContainerId);
            OperationResult checkoutOffer = checkoutSession.PublishShelfOffer();
            OperationResult checkoutReserve = checkoutSession.ReservePrototypeCustomerBasket();
            long checkoutInventoryBefore = checkoutSession.Inventory.Revision;
            long checkoutBasketBefore = checkoutSession.RetailBaskets.Revision;
            long checkoutOffersBefore = checkoutSession.RetailOffers.Revision;
            long checkoutOrdersBefore = checkoutSession.Orders.Revision;
            long checkoutRevisionBefore = checkoutSession.RetailCheckouts.Revision;
            OperationResult checkoutBegin = checkoutSession.BeginPrototypeCheckout();
            OperationResult checkoutRepeat = checkoutSession.BeginPrototypeCheckout();
            bool snapshotCreated =
                checkoutAccept.IsSuccess &&
                checkoutShelfTransfer.IsSuccess &&
                checkoutOffer.IsSuccess &&
                checkoutReserve.IsSuccess &&
                checkoutBegin.IsSuccess &&
                checkoutRepeat.IsSuccess &&
                checkoutSession.TryGetPrototypeCheckout(out var checkoutRecord) &&
                checkoutRecord.BasketId == checkoutSession.PrototypeBasketId &&
                checkoutRecord.CustomerId == checkoutSession.PrototypeCustomerId &&
                checkoutRecord.Currency.Value == GarageStockFlowSession.PrototypeCurrencyCode &&
                checkoutRecord.TotalMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines.Count == 1 &&
                checkoutRecord.Lines[0].ItemId == checkoutSession.ItemId &&
                checkoutRecord.Lines[0].OfferId == checkoutSession.ShelfOfferId &&
                checkoutRecord.Lines[0].UnitPrice.MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines[0].SourceOfferRevision == 1 &&
                checkoutSession.RetailCheckouts.Revision == checkoutRevisionBefore + 1 &&
                checkoutSession.Inventory.Revision == checkoutInventoryBefore &&
                checkoutSession.RetailBaskets.Revision == checkoutBasketBefore &&
                checkoutSession.RetailOffers.Revision == checkoutOffersBefore &&
                checkoutSession.Orders.Revision == checkoutOrdersBefore;
            if (!snapshotCreated)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(checkoutBegin.IsFailure ? checkoutBegin.Error.Code : "smoke.checkout-contract")}");
                yield break;
            }

            const long updatedPriceMinorUnits = 59_999;
            OperationResult updatePrice = checkoutSession.RetailOffers.SetOffer(
                checkoutSession.ShelfOfferId,
                checkoutSession.ProductId,
                checkoutSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                updatedPriceMinorUnits);
            OperationResult repeatAfterPriceChange = checkoutSession.BeginPrototypeCheckout();
            bool priceFrozen =
                updatePrice.IsSuccess &&
                repeatAfterPriceChange.IsSuccess &&
                checkoutSession.TryGetPrototypeCheckout(out checkoutRecord) &&
                checkoutSession.TryGetShelfOffer(out var updatedOffer) &&
                checkoutRecord.TotalMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines[0].UnitPrice.MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                checkoutRecord.Lines[0].SourceOfferRevision == 1 &&
                updatedOffer.Price.MinorUnits == updatedPriceMinorUnits &&
                updatedOffer.OfferRevision == 2 &&
                checkoutSession.RetailCheckouts.Revision == checkoutRevisionBefore + 1 &&
                checkoutSession.Inventory.Revision == checkoutInventoryBefore &&
                checkoutSession.RetailBaskets.Revision == checkoutBasketBefore &&
                checkoutSession.RetailOffers.Revision == checkoutOffersBefore + 1 &&
                checkoutSession.Orders.Revision == checkoutOrdersBefore &&
                checkoutSession.ValidateInvariants().IsSuccess;
            if (!priceFrozen)
            {
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={(repeatAfterPriceChange.IsFailure ? repeatAfterPriceChange.Error.Code : "smoke.checkout-price-drift")}");
                yield break;
            }

            long completionInventoryBefore = checkoutSession.Inventory.Revision;
            long completionBasketBefore = checkoutSession.RetailBaskets.Revision;
            long completionCheckoutBefore = checkoutSession.RetailCheckouts.Revision;
            long completionOfferBefore = checkoutSession.RetailOffers.Revision;
            long completionOrdersBefore = checkoutSession.Orders.Revision;
            OperationResult completeSale = checkoutSession.CompletePrototypeCheckout();
            OperationResult repeatedCompletion = checkoutSession.CompletePrototypeCheckout();
            OperationResult repeatedBeginAfterCompletion = checkoutSession.BeginPrototypeCheckout();
            bool saleCompleted =
                completeSale.IsSuccess &&
                repeatedCompletion.IsSuccess &&
                repeatedBeginAfterCompletion.IsSuccess &&
                checkoutSession.TryGetPrototypeCheckoutCompletion(
                    out RetailCheckoutCompletionRecord completionRecord) &&
                completionRecord.CheckoutId == checkoutSession.PrototypeCheckoutId &&
                completionRecord.BasketId == checkoutSession.PrototypeBasketId &&
                completionRecord.CustomerId == checkoutSession.PrototypeCustomerId &&
                completionRecord.Currency.Value ==
                    GarageStockFlowSession.PrototypeCurrencyCode &&
                completionRecord.TotalMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                completionRecord.Lines.Count == 1 &&
                completionRecord.Lines[0].ItemId == checkoutSession.ItemId &&
                !checkoutSession.TryGetItem(out _) &&
                checkoutSession.Inventory.GetTotalQuantity(
                    checkoutSession.ProductId).Value == 0 &&
                checkoutSession.Inventory.GetAvailableQuantity(
                    checkoutSession.ProductId).Value == 0 &&
                checkoutSession.Inventory.ReservationCount == 0 &&
                checkoutSession.RetailBaskets.Count == 0 &&
                checkoutSession.RetailCheckouts.Count == 1 &&
                checkoutSession.RetailCheckouts.CompletionCount == 1 &&
                checkoutSession.Inventory.Revision == completionInventoryBefore + 1 &&
                checkoutSession.RetailBaskets.Revision == completionBasketBefore + 1 &&
                checkoutSession.RetailCheckouts.Revision == completionCheckoutBefore + 1 &&
                checkoutSession.RetailOffers.Revision == completionOfferBefore &&
                checkoutSession.Orders.Revision == completionOrdersBefore &&
                checkoutSession.ValidateInvariants().IsSuccess;
            if (!saleCompleted)
            {
                string completionFailureCode = completeSale.IsFailure
                    ? completeSale.Error.Code
                    : repeatedCompletion.IsFailure
                        ? repeatedCompletion.Error.Code
                        : repeatedBeginAfterCompletion.IsFailure
                            ? repeatedBeginAfterCompletion.Error.Code
                            : "smoke.sale-completion-contract";
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={completionFailureCode}");
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
                "basket-reservation=ok release=ok " +
                "checkout-snapshot=ok price-frozen=ok " +
                "sale-completion=ok stock-consumed=ok " +
                $"stable={(item.ItemIdValue == session.ItemId.Value ? "ok" : "missing")} " +
                $"completed-quantity={checkoutSession.Inventory.GetTotalQuantity(checkoutSession.ProductId).Value} " +
                $"projection-quantity={session.Inventory.GetTotalQuantity(session.ProductId).Value}");
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator RunCustomerFlowSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(true);
            yield return new WaitForFixedUpdate();

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            InventoryItemWorldBinding liveBinding = stockFlow != null
                ? stockFlow.ItemBinding
                : null;
            if (playerMotor == null || session == null || customerFlow == null ||
                liveBinding == null || liveBinding.Projection == null ||
                !customerFlow.NavigationReady || customerFlow.CustomerAgent == null)
            {
                playerMotor?.SetPaused(false);
                LogCustomerFlowSmokeFailure("smoke.context-missing");
                yield break;
            }

            OperationResult accept = session.AcceptArrivedDelivery();
            OperationResult shelfTransfer = session.TransferItem(session.ShelfContainerId);
            OperationResult publishOffer = session.PublishShelfOffer();
            stockFlow.RefreshPresentation();
            if (accept.IsFailure || shelfTransfer.IsFailure || publishOffer.IsFailure ||
                session.Inventory.GetTotalQuantity(session.ProductId).Value != 1 ||
                !session.TryGetShelfOffer(out _))
            {
                playerMotor.SetPaused(false);
                string code = accept.IsFailure
                    ? accept.Error.Code
                    : shelfTransfer.IsFailure
                        ? shelfTransfer.Error.Code
                        : publishOffer.IsFailure
                            ? publishOffer.Error.Code
                            : "smoke.stock-setup-mismatch";
                LogCustomerFlowSmokeFailure(code);
                yield break;
            }

            long isolatedInventoryRevision = session.Inventory.Revision;
            long isolatedOrderRevision = session.Orders.Revision;
            long isolatedOfferRevision = session.RetailOffers.Revision;
            long isolatedBasketRevision = session.RetailBaskets.Revision;
            long isolatedCheckoutRevision = session.RetailCheckouts.Revision;
            float customerAgentSpeed = customerFlow.CustomerAgent.speed;
            customerFlow.CustomerAgent.speed = Mathf.Min(customerAgentSpeed, 0.10f);
            playerMotor.SetPaused(false);

            const int routeStepLimit = 900;
            int waitSteps = 0;
            while (!customerFlow.VisitStarted && waitSteps < 100)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            if (!customerFlow.VisitStarted || !customerFlow.CustomerVisible ||
                !session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord enteringVisit) ||
                enteringVisit.State != CustomerVisitState.Entering)
            {
                customerFlow.CustomerAgent.speed = customerAgentSpeed;
                playerMotor.SetPaused(false);
                LogCustomerFlowSmokeFailure("smoke.visit-start-mismatch");
                yield break;
            }

            waitSteps = 0;
            while (!customerFlow.HasAssignedRoute && waitSteps < 100)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            if (!customerFlow.HasAssignedRoute ||
                customerFlow.CustomerAgent.remainingDistance <=
                customerFlow.CustomerAgent.stoppingDistance + 0.10f)
            {
                customerFlow.CustomerAgent.speed = customerAgentSpeed;
                playerMotor.SetPaused(false);
                LogCustomerFlowSmokeFailure("smoke.moving-route-missing");
                yield break;
            }

            playerMotor.SetPaused(true);
            yield return new WaitForFixedUpdate();
            yield return null;
            Vector3 pausedPosition = customerFlow.CustomerAgent.transform.position;
            SimulationTimestamp pausedTime = customerFlow.CurrentSimulationTime;
            for (int step = 0; step < 5; step++)
            {
                yield return new WaitForFixedUpdate();
            }

            bool pauseFrozen = customerFlow.CurrentSimulationTime == pausedTime &&
                               Vector3.Distance(
                                   customerFlow.CustomerAgent.transform.position,
                                   pausedPosition) < 0.001f;
            customerFlow.CustomerAgent.speed = customerAgentSpeed;
            playerMotor.SetPaused(false);
            yield return new WaitForFixedUpdate();
            if (!pauseFrozen)
            {
                LogCustomerFlowSmokeFailure("smoke.pause-drift");
                yield break;
            }

            waitSteps = 0;
            while (waitSteps < routeStepLimit &&
                   session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord browseCandidate) &&
                   browseCandidate.State != CustomerVisitState.Browsing)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            bool browseReached = session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord browsingVisit) &&
                                 browsingVisit.State == CustomerVisitState.Browsing &&
                                 browsingVisit.TotalRouteFailureCount == 0 &&
                                 session.Inventory.Revision == isolatedInventoryRevision &&
                                 session.Orders.Revision == isolatedOrderRevision &&
                                 session.RetailOffers.Revision == isolatedOfferRevision &&
                                 session.RetailBaskets.Revision == isolatedBasketRevision &&
                                 session.RetailCheckouts.Revision == isolatedCheckoutRevision;
            if (!browseReached)
            {
                LogCustomerFlowSmokeFailure("smoke.browse-route-or-authority-drift");
                yield break;
            }

            long decisionCustomerRevision = session.CustomerVisits.Revision;
            long decisionInventoryRevision = session.Inventory.Revision;
            long decisionOrderRevision = session.Orders.Revision;
            long decisionOfferRevision = session.RetailOffers.Revision;
            long decisionBasketRevision = session.RetailBaskets.Revision;
            long decisionCheckoutRevision = session.RetailCheckouts.Revision;
            OperationResult<CustomerOfferDecision> offerDecisionResult =
                session.EvaluatePrototypeCustomerOffer();
            bool offerDecision = offerDecisionResult.IsSuccess &&
                                 offerDecisionResult.Value.DecisionKind ==
                                 CustomerOfferDecisionKind.Buy &&
                                 offerDecisionResult.Value.ReasonCode ==
                                 CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit &&
                                 offerDecisionResult.Value.VisitId == browsingVisit.Id &&
                                 offerDecisionResult.Value.OfferRevision == 1 &&
                                 offerDecisionResult.Value.OfferPrice.MinorUnits ==
                                 GarageStockFlowSession.PrototypePriceMinorUnits &&
                                 offerDecisionResult.Value.MaximumAcceptedPrice.MinorUnits ==
                                 GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits &&
                                 customerFlow.StateText.Contains("KARAR: SATIN AL") &&
                                 customerFlow.OfferDecisionReasonCode ==
                                 CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit &&
                                 session.TryGetPrototypeCustomerVisit(
                                     out CustomerVisitRecord decisionVisit) &&
                                 decisionVisit.State == CustomerVisitState.Browsing &&
                                 session.CustomerVisits.Revision == decisionCustomerRevision &&
                                 session.Inventory.Revision == decisionInventoryRevision &&
                                 session.Orders.Revision == decisionOrderRevision &&
                                 session.RetailOffers.Revision == decisionOfferRevision &&
                                 session.RetailBaskets.Revision == decisionBasketRevision &&
                                 session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                                 session.RetailBaskets.Count == 0 &&
                                 session.RetailCheckouts.Count == 0;
            if (!offerDecision)
            {
                LogCustomerFlowSmokeFailure(
                    offerDecisionResult.IsFailure
                        ? offerDecisionResult.Error.Code
                        : "smoke.offer-decision-mismatch");
                yield break;
            }

            OperationResult reserve = session.ReservePrototypeCustomerBasket();
            if (reserve.IsFailure)
            {
                LogCustomerFlowSmokeFailure(reserve.Error.Code);
                yield break;
            }

            waitSteps = 0;
            while (waitSteps < routeStepLimit &&
                   session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord checkoutCandidate) &&
                   checkoutCandidate.State != CustomerVisitState.AwaitingCheckout)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            if (!session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord awaitingVisit) ||
                awaitingVisit.State != CustomerVisitState.AwaitingCheckout ||
                awaitingVisit.TotalRouteFailureCount != 0)
            {
                LogCustomerFlowSmokeFailure("smoke.checkout-route-mismatch");
                yield break;
            }

            OperationResult beginCheckout = liveBinding.TryBeginCheckout();
            OperationResult completeCheckout = liveBinding.TryCompleteCheckout();
            if (beginCheckout.IsFailure || completeCheckout.IsFailure)
            {
                LogCustomerFlowSmokeFailure(
                    beginCheckout.IsFailure ? beginCheckout.Error.Code : completeCheckout.Error.Code);
                yield break;
            }

            waitSteps = 0;
            while (waitSteps < routeStepLimit &&
                   session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord exitCandidate) &&
                   exitCandidate.State != CustomerVisitState.Exited)
            {
                waitSteps++;
                yield return new WaitForFixedUpdate();
            }

            stockFlow.RefreshPresentation();
            bool fulfilled = session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord exitedVisit) &&
                             exitedVisit.State == CustomerVisitState.Exited &&
                             exitedVisit.ExitReason == CustomerVisitExitReason.Fulfilled &&
                             !exitedVisit.RouteFallbackUsed &&
                             exitedVisit.TotalRouteFailureCount == 0 &&
                             !customerFlow.CustomerVisible &&
                             session.Inventory.GetTotalQuantity(session.ProductId).Value == 0 &&
                             session.RetailBaskets.Count == 0 &&
                             session.RetailCheckouts.CompletionCount == 1 &&
                             !liveBinding.Projection.gameObject.activeSelf &&
                             session.ValidateInvariants().IsSuccess;
            if (!fulfilled)
            {
                LogCustomerFlowSmokeFailure("smoke.fulfilled-exit-mismatch");
                yield break;
            }

            GarageStockFlowSession routeFallbackSession = GarageStockFlowSession.CreateArrived();
            long fallbackInventoryRevision = routeFallbackSession.Inventory.Revision;
            long fallbackOrderRevision = routeFallbackSession.Orders.Revision;
            long fallbackOfferRevision = routeFallbackSession.RetailOffers.Revision;
            long fallbackBasketRevision = routeFallbackSession.RetailBaskets.Revision;
            long fallbackCheckoutRevision = routeFallbackSession.RetailCheckouts.Revision;
            OperationResult fallbackStart = routeFallbackSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult routeFailureOne = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(2, 40));
            OperationResult routeFailureTwo = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(3, 60));
            OperationResult exitFailureOne = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(4, 80));
            OperationResult exitFailureTwo = routeFallbackSession.ReportPrototypeCustomerRouteFailure(
                SimulationTimestamp.Create(5, 100));
            bool routeFallback = fallbackStart.IsSuccess &&
                                 routeFailureOne.IsSuccess &&
                                 routeFailureTwo.IsSuccess &&
                                 exitFailureOne.IsSuccess &&
                                 exitFailureTwo.IsSuccess &&
                                 routeFallbackSession.TryGetPrototypeCustomerVisit(
                                     out CustomerVisitRecord fallbackVisit) &&
                                 fallbackVisit.State == CustomerVisitState.Exited &&
                                 fallbackVisit.ExitReason == CustomerVisitExitReason.RouteUnavailable &&
                                 fallbackVisit.RouteFallbackUsed &&
                                 fallbackVisit.TotalRouteFailureCount == 4 &&
                                 routeFallbackSession.Inventory.Revision == fallbackInventoryRevision &&
                                 routeFallbackSession.Orders.Revision == fallbackOrderRevision &&
                                 routeFallbackSession.RetailOffers.Revision == fallbackOfferRevision &&
                                 routeFallbackSession.RetailBaskets.Revision == fallbackBasketRevision &&
                                 routeFallbackSession.RetailCheckouts.Revision == fallbackCheckoutRevision &&
                                 routeFallbackSession.ValidateInvariants().IsSuccess;
            if (!routeFallback)
            {
                LogCustomerFlowSmokeFailure("smoke.route-fallback-mismatch");
                yield break;
            }

            GarageStockFlowSession timeoutSession = GarageStockFlowSession.CreateArrived();
            long timeoutInventoryRevision = timeoutSession.Inventory.Revision;
            long timeoutOrderRevision = timeoutSession.Orders.Revision;
            long timeoutOfferRevision = timeoutSession.RetailOffers.Revision;
            long timeoutBasketRevision = timeoutSession.RetailBaskets.Revision;
            long timeoutCheckoutRevision = timeoutSession.RetailCheckouts.Revision;
            OperationResult timeoutStart = timeoutSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult patienceTimeout = timeoutSession.AdvanceCustomerTime(
                SimulationTimestamp.Create(3001, 60_020));
            OperationResult exitTimeout = timeoutSession.AdvanceCustomerTime(
                SimulationTimestamp.Create(6001, 120_020));
            bool timeoutFallback = timeoutStart.IsSuccess &&
                                   patienceTimeout.IsSuccess &&
                                   exitTimeout.IsSuccess &&
                                   timeoutSession.TryGetPrototypeCustomerVisit(
                                       out CustomerVisitRecord timeoutVisit) &&
                                   timeoutVisit.State == CustomerVisitState.Exited &&
                                   timeoutVisit.ExitReason == CustomerVisitExitReason.PatienceExpired &&
                                   timeoutVisit.RouteFallbackUsed &&
                                   timeoutSession.Inventory.Revision == timeoutInventoryRevision &&
                                   timeoutSession.Orders.Revision == timeoutOrderRevision &&
                                   timeoutSession.RetailOffers.Revision == timeoutOfferRevision &&
                                   timeoutSession.RetailBaskets.Revision == timeoutBasketRevision &&
                                   timeoutSession.RetailCheckouts.Revision == timeoutCheckoutRevision &&
                                   timeoutSession.ValidateInvariants().IsSuccess;
            if (!timeoutFallback)
            {
                LogCustomerFlowSmokeFailure("smoke.timeout-fallback-mismatch");
                yield break;
            }

            Debug.Log(
                "GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok " +
                "pause=ok offer-decision=ok fulfilled=ok " +
                "domain-route-fallback=ok domain-timeout-fallback=ok " +
                "authority-isolated=ok stock-consumed=ok stock-projection-hidden=ok " +
                "customer-hidden=ok");
            yield return new WaitForEndOfFrame();
        }

        private static void LogCustomerFlowSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=failed code={code}");
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
