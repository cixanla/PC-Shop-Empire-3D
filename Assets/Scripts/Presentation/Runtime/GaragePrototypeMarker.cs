using System;
using System.Collections;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Economy;
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
        public const string Version = "garage-motherboard-fastener-r23-v1";

        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private PlayerCarryController playerCarry;
        [SerializeField] private TransportCartProjection transportCart;
        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private GarageCustomerFlowRuntime customerFlow;
        [SerializeField] private CheckoutStationProjection checkoutStation;
        [SerializeField] private MotherboardSeatProjection motherboardSeat;
        [SerializeField] private MotherboardFastenerProjection motherboardFastener;
        [SerializeField] private MotherboardAssemblyItemBinding motherboardBinding;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public PlayerInputAdapter PlayerInput => playerInput;

        public PlayerCarryController PlayerCarry => playerCarry;

        public TransportCartProjection TransportCart => transportCart;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public GarageCustomerFlowRuntime CustomerFlow => customerFlow;

        public CheckoutStationProjection CheckoutStation => checkoutStation;

        public MotherboardSeatProjection MotherboardSeat => motherboardSeat;

        public MotherboardFastenerProjection MotherboardFastener => motherboardFastener;

        public MotherboardAssemblyItemBinding MotherboardBinding => motherboardBinding;

        public void Configure(
            FirstPersonMotor motor,
            PlayerInputAdapter input,
            PlayerCarryController carry,
            TransportCartProjection cart,
            GarageStockFlowRuntime garageStockFlow = null,
            GarageCustomerFlowRuntime garageCustomerFlow = null,
            CheckoutStationProjection physicalCheckoutStation = null,
            MotherboardSeatProjection physicalMotherboardSeat = null,
            MotherboardFastenerProjection physicalMotherboardFastener = null,
            MotherboardAssemblyItemBinding physicalMotherboardBinding = null)
        {
            playerMotor = motor;
            playerInput = input;
            playerCarry = carry;
            transportCart = cart;
            stockFlow = garageStockFlow;
            customerFlow = garageCustomerFlow;
            checkoutStation = physicalCheckoutStation;
            motherboardSeat = physicalMotherboardSeat;
            motherboardFastener = physicalMotherboardFastener;
            motherboardBinding = physicalMotherboardBinding;
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
            bool hasEconomySettlementAuthority = hasCheckoutAuthority &&
                                                 stockFlow.Session.CheckoutSettlements != null &&
                                                 stockFlow.Session.CheckoutSettlements.SettlementCount == 0;
            bool hasCashLedgerAuthority = hasEconomySettlementAuthority &&
                                          stockFlow.Session.CheckoutSettlements.TransactionCount == 0;
            bool hasCustomerVisitAuthority = hasArrivedStockFlow &&
                                             stockFlow.Session.CustomerVisits != null &&
                                             stockFlow.Session.CustomerVisits.Count == 0;
            bool hasCustomerConsultationAuthority = hasCustomerVisitAuthority &&
                                                    stockFlow.Session.CustomerConsultations != null &&
                                                    !stockFlow.Session.PrototypeCustomerConsultationId.IsEmpty;
            bool hasCustomerBuyActionAuthority = hasArrivedStockFlow &&
                                                 stockFlow.Session.CustomerOfferActions != null &&
                                                 stockFlow.Session.CustomerOfferActions.Count == 0;
            bool hasCustomerLeaveActionAuthority = hasCustomerBuyActionAuthority &&
                                                   !stockFlow.Session.PrototypeCustomerLeaveActionId.IsEmpty &&
                                                   stockFlow.Session.PrototypeCustomerLeaveActionId !=
                                                   stockFlow.Session.PrototypeCustomerBuyActionId;
            bool hasCustomerNavigation = customerFlow != null &&
                                         customerFlow.NavigationReady &&
                                         customerFlow.CustomerAgent != null;
            bool hasPhysicalCheckoutStation = checkoutStation != null &&
                                              checkoutStation.InteractionCollider != null &&
                                              checkoutStation.StationStatusText != null &&
                                              checkoutStation.StationIdValue ==
                                                  CheckoutStationProjection.PrototypeStationIdValue;
            GarageStockFlowSession assemblySession = stockFlow?.Session;
            bool hasMotherboardSeat = motherboardSeat != null &&
                                      motherboardSeat.IsConfigured;
            bool hasMotherboardFastener = motherboardFastener != null &&
                                          motherboardFastener.IsConfigured &&
                                          motherboardFastener.FastenerIdValue ==
                                              GarageStockFlowSession.MotherboardFastenerIdValue &&
                                          motherboardFastener.Screwdriver != null &&
                                          motherboardFastener.StatusText != null &&
                                          motherboardFastener.MatchesAuthorityState(
                                              AssemblySeatState.Empty);
            bool hasMotherboardIdentity = assemblySession != null &&
                                          motherboardBinding != null &&
                                          motherboardBinding.PhysicalItem != null &&
                                          motherboardBinding.InventoryItemIdValue ==
                                              assemblySession.MotherboardItemId.Value &&
                                          motherboardBinding.PhysicalItem.ItemIdValue ==
                                              assemblySession.MotherboardItemId.Value &&
                                          assemblySession.Inventory.SerializedItemCount == 1 &&
                                          assemblySession.TryGetMotherboardItem(
                                              out InventoryItemRecord motherboardItem) &&
                                          motherboardItem.Id == assemblySession.MotherboardItemId &&
                                          motherboardItem.ProductId ==
                                              assemblySession.MotherboardProductId &&
                                          motherboardItem.ContainerId ==
                                              assemblySession.WorldFloorContainerId &&
                                          CountCanonicalMotherboardProjections(
                                              assemblySession.MotherboardItemId.Value) == 1 &&
                                          motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            bool hasMotherboardAssembly = hasMotherboardSeat &&
                                          hasMotherboardFastener &&
                                          motherboardBinding != null &&
                                          motherboardBinding.Runtime == stockFlow &&
                                          motherboardBinding.Seat == motherboardSeat &&
                                          motherboardBinding.Fastener == motherboardFastener &&
                                          motherboardBinding.PhysicalItem != null &&
                                          motherboardBinding.PhysicalItem.CarryProfile ==
                                              PhysicalCarryProfile.PcComponent &&
                                          assemblySession != null &&
                                          assemblySession.AssemblyBuild.MotherboardSeatState ==
                                              AssemblySeatState.Empty &&
                                          assemblySession.AssemblyBuild.Revision == 0 &&
                                          assemblySession.AssemblyBuild.ReceiptCount == 0 &&
                                          assemblySession.AssemblyBuild.ValidateInvariants().IsSuccess &&
                                          hasMotherboardIdentity;

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
                $"cash-payment={(hasEconomySettlementAuthority ? "ready" : "missing")} " +
                $"payment-receipt={(hasEconomySettlementAuthority ? "ready" : "missing")} " +
                $"economy-settlement={(hasEconomySettlementAuthority ? "ready" : "missing")} " +
                $"cash-ledger={(hasCashLedgerAuthority ? "ready" : "missing")} " +
                $"customer-visit={(hasCustomerVisitAuthority ? "ready" : "missing")} " +
                $"customer-consultation={(hasCustomerConsultationAuthority ? "ready" : "missing")} " +
                $"consultation-decision-gate={(hasCustomerConsultationAuthority ? "ready" : "missing")} " +
                $"customer-buy-action={(hasCustomerBuyActionAuthority ? "ready" : "missing")} " +
                $"customer-leave-action={(hasCustomerLeaveActionAuthority ? "ready" : "missing")} " +
                $"customer-navmesh={(hasCustomerNavigation ? "ready" : "missing")} " +
                $"checkout-station={(hasPhysicalCheckoutStation ? "ready" : "missing")} " +
                $"assembly={(hasMotherboardAssembly ? "ready" : "missing")} " +
                $"motherboard-seat={(hasMotherboardSeat ? "ready" : "missing")} " +
                $"motherboard-fastener={(hasMotherboardFastener ? "ready" : "missing")} " +
                $"screwdriver={(hasMotherboardFastener ? "ready" : "missing")} " +
                $"motherboard-identity={(hasMotherboardIdentity ? "stable" : "missing")} " +
                $"lookdev={(hasLookdevCorner && hasLookdevVolume && hasTaskLight ? "ok" : "missing")}");

            bool cartSmokeRequested = HasCommandLineArgument("-pse-cart-smoke");
            bool runStockFlowSmoke = HasCommandLineArgument("-pse-stock-flow-smoke");
            bool runCustomerFlowSmoke = HasCommandLineArgument("-pse-customer-flow-smoke");
            bool runAssemblySmoke = HasCommandLineArgument("-pse-assembly-smoke");
            int smokeCount = (cartSmokeRequested ? 1 : 0) +
                             (runStockFlowSmoke ? 1 : 0) +
                             (runCustomerFlowSmoke ? 1 : 0) +
                             (runAssemblySmoke ? 1 : 0);
            if (smokeCount > 1)
            {
                Debug.LogError("GARAGE_RUNTIME_SMOKE smoke=failed code=smoke.conflicting-flags");
                return;
            }

            if (cartSmokeRequested && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_RUNTIME_SMOKE smoke=failed code=smoke.cart-requires-development-build");
                return;
            }

            if (runAssemblySmoke && !Debug.isDebugBuild)
            {
                Debug.LogError(
                    "GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE " +
                    "assembly-flow=failed code=smoke.assembly-requires-development-build");
                return;
            }

            if (cartSmokeRequested)
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

            if (runAssemblySmoke)
            {
                Application.runInBackground = true;
                StartCoroutine(RunMotherboardAssemblySmoke());
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

            long settlementInventoryBefore = checkoutSession.Inventory.Revision;
            long settlementBasketBefore = checkoutSession.RetailBaskets.Revision;
            long settlementCheckoutBefore = checkoutSession.RetailCheckouts.Revision;
            long settlementOfferBefore = checkoutSession.RetailOffers.Revision;
            long settlementOrdersBefore = checkoutSession.Orders.Revision;
            long settlementEconomyBefore = checkoutSession.CheckoutSettlements.Revision;
            OperationResult settleCash = checkoutSession.SettlePrototypeCashCheckout();
            OperationResult repeatedSettlement = checkoutSession.SettlePrototypeCashCheckout();
            OperationResult conflictingSettlement =
                checkoutSession.CheckoutSettlements.SettleCashCheckout(
                    checkoutSession.PrototypeCheckoutSettlementId,
                    StableId<EconomyLedgerTransactionIdScope>.Parse(
                        "economy.ledger-transaction.smoke-conflict"),
                    checkoutSession.PrototypeCheckoutCompletionId,
                    checkoutSession.PrototypeCheckoutId,
                    GarageStockFlowSession.PrototypeCurrencyCode,
                    GarageStockFlowSession.PrototypePriceMinorUnits,
                    SimulationTimestamp.Create(7, 7_000L));
            OperationResult repeatedBeginAfterCompletion = checkoutSession.BeginPrototypeCheckout();
            CurrencyCode settlementCurrency = CurrencyCode.Create(
                GarageStockFlowSession.PrototypeCurrencyCode).Value;
            OperationResult<long> cashDelta = checkoutSession.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.Cash,
                settlementCurrency);
            OperationResult<long> revenueDelta = checkoutSession.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.SalesRevenue,
                settlementCurrency);
            OperationResult<long> cogsDelta = checkoutSession.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.CostOfGoodsSold,
                settlementCurrency);
            OperationResult<long> inventoryAssetDelta =
                checkoutSession.CheckoutSettlements.GetAccountDelta(
                    EconomyAccountKind.InventoryAsset,
                    settlementCurrency);
            bool saleSettled =
                settleCash.IsSuccess &&
                repeatedSettlement.IsSuccess &&
                conflictingSettlement.Error ==
                    CheckoutSettlementFailures.SettlementIdentityConflict &&
                repeatedBeginAfterCompletion.IsSuccess &&
                checkoutSession.TryGetPrototypeCheckoutCompletion(
                    out RetailCheckoutCompletionRecord completionRecord) &&
                checkoutSession.CheckoutSettlements.TryGetSettlement(
                    checkoutSession.PrototypeCheckoutSettlementId,
                    out CheckoutSettlementReceipt settlementReceipt) &&
                checkoutSession.TryGetPrototypeLedgerTransaction(
                    out EconomyLedgerTransactionRecord ledgerTransaction) &&
                completionRecord.CheckoutId == checkoutSession.PrototypeCheckoutId &&
                completionRecord.BasketId == checkoutSession.PrototypeBasketId &&
                completionRecord.CustomerId == checkoutSession.PrototypeCustomerId &&
                completionRecord.Currency.Value ==
                    GarageStockFlowSession.PrototypeCurrencyCode &&
                completionRecord.TotalMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                completionRecord.Lines.Count == 1 &&
                completionRecord.Lines[0].ItemId == checkoutSession.ItemId &&
                settlementReceipt.Id == checkoutSession.PrototypeCheckoutSettlementId &&
                settlementReceipt.TransactionId == checkoutSession.PrototypeLedgerTransactionId &&
                settlementReceipt.CompletionId == completionRecord.Id &&
                settlementReceipt.CheckoutId == completionRecord.CheckoutId &&
                settlementReceipt.CustomerId == completionRecord.CustomerId &&
                settlementReceipt.PaymentMethod == CheckoutPaymentMethod.Cash &&
                settlementReceipt.Currency == completionRecord.Currency &&
                settlementReceipt.GrossMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                settlementReceipt.CostOfGoodsSoldMinorUnits ==
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                settlementReceipt.GrossMarginMinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits -
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                ledgerTransaction.Id == checkoutSession.PrototypeLedgerTransactionId &&
                ledgerTransaction.SettlementId == settlementReceipt.Id &&
                ledgerTransaction.Entries.Count == 4 &&
                ledgerTransaction.Entries[0].Account == EconomyAccountKind.Cash &&
                ledgerTransaction.Entries[0].Direction == EconomyEntryDirection.Debit &&
                ledgerTransaction.Entries[0].MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                ledgerTransaction.Entries[1].Account == EconomyAccountKind.SalesRevenue &&
                ledgerTransaction.Entries[1].Direction == EconomyEntryDirection.Credit &&
                ledgerTransaction.Entries[1].MinorUnits ==
                    GarageStockFlowSession.PrototypePriceMinorUnits &&
                ledgerTransaction.Entries[2].Account == EconomyAccountKind.CostOfGoodsSold &&
                ledgerTransaction.Entries[2].Direction == EconomyEntryDirection.Debit &&
                ledgerTransaction.Entries[2].MinorUnits ==
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                ledgerTransaction.Entries[3].Account == EconomyAccountKind.InventoryAsset &&
                ledgerTransaction.Entries[3].Direction == EconomyEntryDirection.Credit &&
                ledgerTransaction.Entries[3].MinorUnits ==
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                ledgerTransaction.Entries[0].MinorUnits +
                    ledgerTransaction.Entries[2].MinorUnits ==
                    ledgerTransaction.Entries[1].MinorUnits +
                    ledgerTransaction.Entries[3].MinorUnits &&
                cashDelta.IsSuccess &&
                cashDelta.Value == GarageStockFlowSession.PrototypePriceMinorUnits &&
                revenueDelta.IsSuccess &&
                revenueDelta.Value == GarageStockFlowSession.PrototypePriceMinorUnits &&
                cogsDelta.IsSuccess &&
                cogsDelta.Value == GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                inventoryAssetDelta.IsSuccess &&
                inventoryAssetDelta.Value == -GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                !checkoutSession.TryGetItem(out _) &&
                checkoutSession.Inventory.GetTotalQuantity(
                    checkoutSession.ProductId).Value == 0 &&
                checkoutSession.Inventory.GetAvailableQuantity(
                    checkoutSession.ProductId).Value == 0 &&
                checkoutSession.Inventory.ReservationCount == 0 &&
                checkoutSession.RetailBaskets.Count == 0 &&
                checkoutSession.RetailCheckouts.Count == 1 &&
                checkoutSession.RetailCheckouts.CompletionCount == 1 &&
                checkoutSession.CheckoutSettlements.SettlementCount == 1 &&
                checkoutSession.CheckoutSettlements.TransactionCount == 1 &&
                checkoutSession.Inventory.Revision == settlementInventoryBefore + 1 &&
                checkoutSession.RetailBaskets.Revision == settlementBasketBefore + 1 &&
                checkoutSession.RetailCheckouts.Revision == settlementCheckoutBefore + 1 &&
                checkoutSession.RetailOffers.Revision == settlementOfferBefore &&
                checkoutSession.Orders.Revision == settlementOrdersBefore &&
                checkoutSession.CheckoutSettlements.Revision == settlementEconomyBefore + 1 &&
                checkoutSession.ValidateInvariants().IsSuccess;
            if (!saleSettled)
            {
                string settlementFailureCode = settleCash.IsFailure
                    ? settleCash.Error.Code
                    : repeatedSettlement.IsFailure
                        ? repeatedSettlement.Error.Code
                        : repeatedBeginAfterCompletion.IsFailure
                            ? repeatedBeginAfterCompletion.Error.Code
                            : "smoke.cash-settlement-contract";
                Debug.LogError(
                    $"GARAGE_STOCK_FLOW_RUNTIME_SMOKE stock-flow=failed " +
                    $"code={settlementFailureCode}");
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
                "cash-payment=ok payment-receipt=ok economy-settlement=ok " +
                "cash-ledger=ok revenue=ok cogs=ok inventory-asset=ok ledger-balanced=ok " +
                "payment-replay=ok payment-conflict-blocked=ok stock-consumed=ok " +
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
            if (playerMotor == null || playerCarry == null || session == null ||
                customerFlow == null || checkoutStation == null ||
                liveBinding == null || liveBinding.Projection == null ||
                checkoutStation.InteractionCollider == null ||
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
            long isolatedEconomyRevision = session.CheckoutSettlements.Revision;
            long isolatedConsultationRevision = session.CustomerConsultations.Revision;
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
                                 session.RetailCheckouts.Revision == isolatedCheckoutRevision &&
                                 session.CheckoutSettlements.Revision == isolatedEconomyRevision;
            browseReached = browseReached &&
                            session.CustomerConsultations.Revision ==
                                isolatedConsultationRevision;
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
            long decisionEconomyRevision = session.CheckoutSettlements.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            OperationResult<CustomerOfferDecision> gatedDecision =
                session.EvaluatePrototypeCustomerOffer();
            bool decisionGated = gatedDecision.Error ==
                                 CustomerOfferDecisionFailures.ConsultationRequired &&
                                 customerFlow.CurrentOfferDecision == null &&
                                 session.CustomerConsultations.Revision == consultationRevision &&
                                 session.CustomerVisits.Revision == decisionCustomerRevision &&
                                 session.Inventory.Revision == decisionInventoryRevision &&
                                 session.Orders.Revision == decisionOrderRevision &&
                                 session.RetailOffers.Revision == decisionOfferRevision &&
                                 session.RetailBaskets.Revision == decisionBasketRevision &&
                                 session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                                 session.CheckoutSettlements.Revision == decisionEconomyRevision;
            if (!decisionGated)
            {
                LogCustomerFlowSmokeFailure("smoke.consultation-decision-gate-mismatch");
                yield break;
            }

            OperationResult consultation = session.ConsultPrototypeCustomer(
                customerFlow.CurrentConsultationTime);
            OperationResult consultationReplay = session.ConsultPrototypeCustomer(
                customerFlow.CurrentConsultationTime);
            CustomerConsultationRecord consultationRecord = null;
            bool consultationRecorded = consultation.IsSuccess &&
                                        consultationReplay.IsSuccess &&
                                        session.CustomerConsultations.Revision ==
                                            consultationRevision + 1 &&
                                        session.TryGetPrototypeCustomerConsultation(
                                            out consultationRecord) &&
                                        consultationRecord.VisitId == browsingVisit.Id &&
                                        consultationRecord.IntentId == browsingVisit.Intent.Id &&
                                        consultationRecord.Need == browsingVisit.Intent.Need &&
                                        consultationRecord.ProductId == browsingVisit.Intent.ProductId &&
                                        session.CustomerVisits.Revision == decisionCustomerRevision &&
                                        session.Inventory.Revision == decisionInventoryRevision &&
                                        session.Orders.Revision == decisionOrderRevision &&
                                        session.RetailOffers.Revision == decisionOfferRevision &&
                                        session.RetailBaskets.Revision == decisionBasketRevision &&
                                        session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                                        session.CheckoutSettlements.Revision == decisionEconomyRevision;
            if (!consultationRecorded)
            {
                LogCustomerFlowSmokeFailure(
                    consultation.IsFailure
                        ? consultation.Error.Code
                        : consultationReplay.IsFailure
                            ? consultationReplay.Error.Code
                            : "smoke.consultation-provenance-mismatch");
                yield break;
            }

            OperationResult<CustomerOfferDecision> offerDecisionResult =
                session.EvaluatePrototypeCustomerOffer();
            CustomerOfferDecision displayedDecision = customerFlow.CurrentOfferDecision;
            bool offerDecision = offerDecisionResult.IsSuccess &&
                                 displayedDecision != null &&
                                 displayedDecision.Equals(offerDecisionResult.Value) &&
                                 offerDecisionResult.Value.DecisionKind ==
                                 CustomerOfferDecisionKind.Buy &&
                                 offerDecisionResult.Value.ReasonCode ==
                                 CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit &&
                                 offerDecisionResult.Value.VisitId == browsingVisit.Id &&
                                 offerDecisionResult.Value.Consultation.Id ==
                                     consultationRecord.Id &&
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
                                 session.CheckoutSettlements.Revision == decisionEconomyRevision &&
                                 session.CustomerConsultations.Revision ==
                                     consultationRevision + 1 &&
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

            long actionRevision = session.CustomerOfferActions.Revision;
            OperationResult buyAction = session.ApplyPrototypeCustomerBuy(
                displayedDecision,
                customerFlow.CurrentOfferActionTime);
            bool buyApplied = buyAction.IsSuccess &&
                              session.CustomerOfferActions.Revision == actionRevision + 1 &&
                              session.CustomerVisits.Revision == decisionCustomerRevision + 1 &&
                              session.Inventory.Revision == decisionInventoryRevision + 1 &&
                              session.RetailBaskets.Revision == decisionBasketRevision + 1 &&
                              session.Orders.Revision == decisionOrderRevision &&
                              session.RetailOffers.Revision == decisionOfferRevision &&
                              session.RetailCheckouts.Revision == decisionCheckoutRevision &&
                              session.CustomerConsultations.Revision ==
                                  consultationRevision + 1 &&
                              session.TryGetPrototypeCustomerBuyAction(out _) &&
                              session.TryGetPrototypeBasketLine(out RetailBasketLineRecord actionLine) &&
                              actionLine.IsActionOwned;
            if (!buyApplied)
            {
                LogCustomerFlowSmokeFailure(
                    buyAction.IsFailure
                        ? buyAction.Error.Code
                        : "smoke.buy-action-mismatch");
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

            long shelfCheckoutRevision = session.RetailCheckouts.Revision;
            long shelfEconomyRevision = session.CheckoutSettlements.Revision;
            MovePlayerToPhysicalItem(liveBinding.Projection, -Vector3.right, 1.25f);
            playerCarry.ProcessInputFrame();
            bool shelfBypassBlocked = playerCarry.FocusedItem == liveBinding.Projection &&
                                      liveBinding.RequiresCheckoutStart &&
                                      playerCarry.PromptText.Contains("KASA İSTASYONUNA GİT") &&
                                      session.RetailCheckouts.Revision == shelfCheckoutRevision &&
                                      session.CheckoutSettlements.Revision == shelfEconomyRevision;
            if (!shelfBypassBlocked)
            {
                LogCustomerFlowSmokeFailure("smoke.shelf-checkout-bypass");
                yield break;
            }

            MovePlayerToCheckoutStation(1.45f);
            checkoutStation.RefreshPresentation();
            if (!checkoutStation.IsFocused ||
                !checkoutStation.PromptText.Contains("KASAYI BAŞLAT"))
            {
                LogCustomerFlowSmokeFailure(
                    string.IsNullOrEmpty(checkoutStation.LastFailureCode)
                        ? "smoke.checkout-station-focus-missing"
                        : checkoutStation.LastFailureCode);
                yield break;
            }

            OperationResult beginCheckout = checkoutStation.TryOperate();
            bool checkoutStartedAtStation = beginCheckout.IsSuccess &&
                                            liveBinding.RequiresCheckoutCompletion &&
                                            session.RetailCheckouts.Revision ==
                                                shelfCheckoutRevision + 1 &&
                                            session.CheckoutSettlements.Revision ==
                                                shelfEconomyRevision &&
                                            checkoutStation.PromptText.Contains(
                                                "NAKİT ÖDEMEYİ AL");
            if (checkoutStartedAtStation)
            {
                yield return null;
                checkoutStation.RefreshPresentation();
            }

            OperationResult settleCash = checkoutStartedAtStation
                ? checkoutStation.TryOperate()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.checkout-station-start-mismatch"));
            if (beginCheckout.IsFailure || settleCash.IsFailure)
            {
                LogCustomerFlowSmokeFailure(
                    beginCheckout.IsFailure ? beginCheckout.Error.Code : settleCash.Error.Code);
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
            bool hasExitedVisit = session.TryGetPrototypeCustomerVisit(
                out CustomerVisitRecord exitedVisit);
            bool hasFulfilledReceipt = session.TryGetPrototypeCheckoutSettlement(
                out CheckoutSettlementReceipt fulfilledReceipt);
            bool hasFulfilledTransaction = session.TryGetPrototypeLedgerTransaction(
                out EconomyLedgerTransactionRecord fulfilledTransaction);
            bool invariantsValid = session.ValidateInvariants().IsSuccess;
            bool hasRemainingMotherboard = session.TryGetMotherboardItem(
                out InventoryItemRecord remainingMotherboard);
            bool motherboardProjectionValid = motherboardBinding != null &&
                                                motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            bool motherboardIsolated = session.Inventory.SerializedItemCount == 1 &&
                                       hasRemainingMotherboard &&
                                       remainingMotherboard.Id == session.MotherboardItemId &&
                                       remainingMotherboard.ProductId == session.MotherboardProductId &&
                                       remainingMotherboard.ContainerId == session.WorldFloorContainerId &&
                                       session.AssemblyBuild.Revision == 0 &&
                                       motherboardProjectionValid;
            bool fulfilled = hasExitedVisit &&
                             exitedVisit.State == CustomerVisitState.Exited &&
                             exitedVisit.ExitReason == CustomerVisitExitReason.Fulfilled &&
                             !exitedVisit.RouteFallbackUsed &&
                             exitedVisit.TotalRouteFailureCount == 0 &&
                             !customerFlow.CustomerVisible &&
                             session.Inventory.GetTotalQuantity(session.ProductId).Value == 0 &&
                             session.RetailBaskets.Count == 0 &&
                             session.RetailCheckouts.CompletionCount == 1 &&
                             session.CheckoutSettlements.SettlementCount == 1 &&
                             session.CheckoutSettlements.TransactionCount == 1 &&
                             hasFulfilledReceipt &&
                             fulfilledReceipt.PaymentMethod == CheckoutPaymentMethod.Cash &&
                             fulfilledReceipt.GrossMinorUnits ==
                                 GarageStockFlowSession.PrototypePriceMinorUnits &&
                             fulfilledReceipt.CostOfGoodsSoldMinorUnits ==
                                 GarageStockFlowSession.PrototypeUnitCostMinorUnits &&
                             hasFulfilledTransaction &&
                             fulfilledTransaction.Entries.Count == 4 &&
                             fulfilledTransaction.Entries[0].MinorUnits +
                                 fulfilledTransaction.Entries[2].MinorUnits ==
                                 fulfilledTransaction.Entries[1].MinorUnits +
                             fulfilledTransaction.Entries[3].MinorUnits &&
                             !liveBinding.Projection.gameObject.activeSelf &&
                             motherboardIsolated &&
                             invariantsValid;
            if (!fulfilled)
            {
                LogCustomerFlowSmokeFailure(
                    "smoke.fulfilled-exit-mismatch " +
                    $"visit={(hasExitedVisit ? exitedVisit.State.ToString() : "missing")} " +
                    $"reason={(hasExitedVisit ? exitedVisit.ExitReason.ToString() : "missing")} " +
                    $"fallback={(hasExitedVisit && exitedVisit.RouteFallbackUsed ? "yes" : "no")} " +
                    $"route-failures={(hasExitedVisit ? exitedVisit.TotalRouteFailureCount.ToString() : "missing")} " +
                    $"visible={(customerFlow.CustomerVisible ? "yes" : "no")} " +
                    $"stock={session.Inventory.GetTotalQuantity(session.ProductId).Value} " +
                    $"basket={session.RetailBaskets.Count} " +
                    $"completion={session.RetailCheckouts.CompletionCount} " +
                    $"settlement={session.CheckoutSettlements.SettlementCount} " +
                    $"transaction={session.CheckoutSettlements.TransactionCount} " +
                    $"receipt={(hasFulfilledReceipt ? "ok" : "missing")} " +
                    $"ledger={(hasFulfilledTransaction ? "ok" : "missing")} " +
                    $"projection={(liveBinding.Projection.gameObject.activeSelf ? "visible" : "hidden")} " +
                    $"global-items={session.Inventory.SerializedItemCount} " +
                    $"motherboard-id={(hasRemainingMotherboard && remainingMotherboard.Id == session.MotherboardItemId ? "ok" : "mismatch")} " +
                    $"motherboard-product={(hasRemainingMotherboard && remainingMotherboard.ProductId == session.MotherboardProductId ? "ok" : "mismatch")} " +
                    $"motherboard-container={(hasRemainingMotherboard ? remainingMotherboard.ContainerId.Value : "missing")} " +
                    $"assembly-revision={session.AssemblyBuild.Revision} " +
                    $"motherboard-projection={(motherboardProjectionValid ? "ok" : "failed")} " +
                    $"invariants={(invariantsValid ? "ok" : "failed")}");
                yield break;
            }

            GarageStockFlowSession staleSession = GarageStockFlowSession.CreateArrived();
            OperationResult staleAccept = staleSession.AcceptArrivedDelivery();
            OperationResult staleShelf = staleSession.TransferItem(staleSession.ShelfContainerId);
            OperationResult stalePublish = staleSession.PublishShelfOffer();
            OperationResult staleStart = staleSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult staleBrowse = staleSession.MarkPrototypeCustomerBrowseArrival(
                SimulationTimestamp.Create(2, 40));
            OperationResult staleConsultation = staleSession.ConsultPrototypeCustomer(
                SimulationTimestamp.Create(3, 60));
            OperationResult<CustomerOfferDecision> staleDecisionResult =
                staleSession.EvaluatePrototypeCustomerOffer();
            OperationResult staleOfferDrift = staleSession.RetailOffers.SetOffer(
                staleSession.ShelfOfferId,
                staleSession.ProductId,
                staleSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits + 1);
            long staleActionRevision = staleSession.CustomerOfferActions.Revision;
            long staleActorRevision = staleSession.CustomerVisits.Revision;
            long staleInventoryRevision = staleSession.Inventory.Revision;
            long staleBasketRevision = staleSession.RetailBaskets.Revision;
            long staleOfferRevision = staleSession.RetailOffers.Revision;
            long staleCheckoutRevision = staleSession.RetailCheckouts.Revision;
            long staleOrderRevision = staleSession.Orders.Revision;
            long staleConsultationRevision = staleSession.CustomerConsultations.Revision;
            OperationResult staleApply = staleDecisionResult.IsSuccess
                ? staleSession.ApplyPrototypeCustomerBuy(
                    staleDecisionResult.Value,
                    SimulationTimestamp.Create(4, 80))
                : OperationResult.Fail(staleDecisionResult.Error);
            bool staleBlocked = staleAccept.IsSuccess &&
                                staleShelf.IsSuccess &&
                                stalePublish.IsSuccess &&
                                staleStart.IsSuccess &&
                                staleBrowse.IsSuccess &&
                                staleConsultation.IsSuccess &&
                                staleDecisionResult.IsSuccess &&
                                staleOfferDrift.IsSuccess &&
                                staleApply.Error ==
                                    CustomerOfferDecisionActionFailures.DecisionStale &&
                                staleSession.CustomerOfferActions.Revision == staleActionRevision &&
                                staleSession.CustomerVisits.Revision == staleActorRevision &&
                                staleSession.Inventory.Revision == staleInventoryRevision &&
                                staleSession.RetailBaskets.Revision == staleBasketRevision &&
                                staleSession.RetailOffers.Revision == staleOfferRevision &&
                                staleSession.RetailCheckouts.Revision == staleCheckoutRevision &&
                                staleSession.Orders.Revision == staleOrderRevision &&
                                staleSession.CustomerConsultations.Revision ==
                                    staleConsultationRevision &&
                                staleSession.CustomerOfferActions.Count == 0 &&
                                staleSession.RetailBaskets.Count == 0 &&
                                staleSession.Inventory.ReservationCount == 0 &&
                                staleSession.ValidateInvariants().IsSuccess;
            if (!staleBlocked)
            {
                LogCustomerFlowSmokeFailure(
                    staleApply.IsFailure
                        ? staleApply.Error.Code
                        : "smoke.stale-decision-mutated-authority");
                yield break;
            }

            GarageStockFlowSession foreignReceiptSession =
                GarageStockFlowSession.CreateArrived();
            OperationResult foreignReceiptAccept =
                foreignReceiptSession.AcceptArrivedDelivery();
            OperationResult foreignReceiptShelf = foreignReceiptSession.TransferItem(
                foreignReceiptSession.ShelfContainerId);
            OperationResult foreignReceiptPublish =
                foreignReceiptSession.PublishShelfOffer();
            OperationResult foreignReceiptStart =
                foreignReceiptSession.StartPrototypeCustomerVisit(
                    SimulationTimestamp.Create(1, 20));
            OperationResult foreignReceiptBrowse =
                foreignReceiptSession.MarkPrototypeCustomerBrowseArrival(
                    SimulationTimestamp.Create(2, 40));
            OperationResult foreignReceiptConsult =
                foreignReceiptSession.ConsultPrototypeCustomer(
                    SimulationTimestamp.Create(3, 60));
            bool hasForeignReceipt =
                foreignReceiptSession.TryGetPrototypeCustomerConsultation(
                    out CustomerConsultationRecord foreignReceipt);

            GarageStockFlowSession receiptOwnerSession =
                GarageStockFlowSession.CreateArrived();
            OperationResult receiptOwnerAccept = receiptOwnerSession.AcceptArrivedDelivery();
            OperationResult receiptOwnerShelf = receiptOwnerSession.TransferItem(
                receiptOwnerSession.ShelfContainerId);
            OperationResult receiptOwnerPublish = receiptOwnerSession.PublishShelfOffer();
            OperationResult receiptOwnerStart = receiptOwnerSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult receiptOwnerBrowse =
                receiptOwnerSession.MarkPrototypeCustomerBrowseArrival(
                    SimulationTimestamp.Create(2, 40));
            bool hasReceiptOwnerVisit = receiptOwnerSession.TryGetPrototypeCustomerVisit(
                out CustomerVisitRecord receiptOwnerVisit);
            bool hasReceiptOwnerOffer = receiptOwnerSession.TryGetShelfOffer(
                out ShelfOfferRecord receiptOwnerOffer);
            OperationResult<CustomerOfferDecision> foreignReceiptDecision =
                hasForeignReceipt && hasReceiptOwnerVisit && hasReceiptOwnerOffer
                    ? CustomerOfferDecisionEvaluator.Evaluate(
                        receiptOwnerVisit,
                        foreignReceipt,
                        receiptOwnerOffer,
                        ShelfPrice.Create(
                            GarageStockFlowSession.PrototypeCurrencyCode,
                            GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits).Value)
                    : OperationResult<CustomerOfferDecision>.Fail(
                        CustomerOfferDecisionFailures.InputInvalid);
            long receiptOwnerActionRevision = receiptOwnerSession.CustomerOfferActions.Revision;
            long receiptOwnerVisitRevision = receiptOwnerSession.CustomerVisits.Revision;
            long receiptOwnerInventoryRevision = receiptOwnerSession.Inventory.Revision;
            long receiptOwnerBasketRevision = receiptOwnerSession.RetailBaskets.Revision;
            long receiptOwnerOfferRevision = receiptOwnerSession.RetailOffers.Revision;
            long receiptOwnerCheckoutRevision = receiptOwnerSession.RetailCheckouts.Revision;
            long receiptOwnerOrderRevision = receiptOwnerSession.Orders.Revision;
            long receiptOwnerConsultationRevision =
                receiptOwnerSession.CustomerConsultations.Revision;
            bool staleConsultationBlocked = foreignReceiptAccept.IsSuccess &&
                                            foreignReceiptShelf.IsSuccess &&
                                            foreignReceiptPublish.IsSuccess &&
                                            foreignReceiptStart.IsSuccess &&
                                            foreignReceiptBrowse.IsSuccess &&
                                            foreignReceiptConsult.IsSuccess &&
                                            hasForeignReceipt &&
                                            receiptOwnerAccept.IsSuccess &&
                                            receiptOwnerShelf.IsSuccess &&
                                            receiptOwnerPublish.IsSuccess &&
                                            receiptOwnerStart.IsSuccess &&
                                            receiptOwnerBrowse.IsSuccess &&
                                            foreignReceiptDecision.Error ==
                                                CustomerOfferDecisionFailures.ConsultationMismatch &&
                                            !receiptOwnerSession.CustomerConsultations.Owns(
                                                foreignReceipt) &&
                                            receiptOwnerSession.CustomerOfferActions.Revision ==
                                                receiptOwnerActionRevision &&
                                            receiptOwnerSession.CustomerVisits.Revision ==
                                                receiptOwnerVisitRevision &&
                                            receiptOwnerSession.Inventory.Revision ==
                                                receiptOwnerInventoryRevision &&
                                            receiptOwnerSession.RetailBaskets.Revision ==
                                                receiptOwnerBasketRevision &&
                                            receiptOwnerSession.RetailOffers.Revision ==
                                                receiptOwnerOfferRevision &&
                                            receiptOwnerSession.RetailCheckouts.Revision ==
                                                receiptOwnerCheckoutRevision &&
                                            receiptOwnerSession.Orders.Revision ==
                                                receiptOwnerOrderRevision &&
                                            receiptOwnerSession.CustomerConsultations.Revision ==
                                                receiptOwnerConsultationRevision &&
                                            receiptOwnerSession.CustomerOfferActions.Count == 0 &&
                                            receiptOwnerSession.RetailBaskets.Count == 0 &&
                                            receiptOwnerSession.Inventory.ReservationCount == 0 &&
                                            receiptOwnerSession.ValidateInvariants().IsSuccess &&
                                            foreignReceiptSession.ValidateInvariants().IsSuccess;
            if (!staleConsultationBlocked)
            {
                LogCustomerFlowSmokeFailure(
                    foreignReceiptDecision.IsFailure
                        ? foreignReceiptDecision.Error.Code
                        : "smoke.foreign-consultation-not-blocked");
                yield break;
            }

            GarageStockFlowSession leaveSession = GarageStockFlowSession.CreateArrived();
            OperationResult leaveAccept = leaveSession.AcceptArrivedDelivery();
            OperationResult leaveShelf = leaveSession.TransferItem(
                leaveSession.ShelfContainerId);
            OperationResult leavePublish = leaveSession.PublishShelfOffer();
            OperationResult leavePrice = leaveSession.RetailOffers.SetOffer(
                leaveSession.ShelfOfferId,
                leaveSession.ProductId,
                leaveSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1);
            OperationResult leaveStart = leaveSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult leaveBrowse = leaveSession.MarkPrototypeCustomerBrowseArrival(
                SimulationTimestamp.Create(2, 40));
            OperationResult leaveConsultation = leaveSession.ConsultPrototypeCustomer(
                SimulationTimestamp.Create(3, 60));
            OperationResult<CustomerOfferDecision> leaveDecision =
                leaveSession.EvaluatePrototypeCustomerOffer();
            long leaveActionRevision = leaveSession.CustomerOfferActions.Revision;
            long leaveActorRevision = leaveSession.CustomerVisits.Revision;
            long leaveInventoryRevision = leaveSession.Inventory.Revision;
            long leaveBasketRevision = leaveSession.RetailBaskets.Revision;
            long leaveOfferRevision = leaveSession.RetailOffers.Revision;
            long leaveCheckoutRevision = leaveSession.RetailCheckouts.Revision;
            long leaveOrderRevision = leaveSession.Orders.Revision;
            long leaveConsultationRevision = leaveSession.CustomerConsultations.Revision;
            OperationResult leaveApply = leaveDecision.IsSuccess
                ? leaveSession.ApplyPrototypeCustomerLeave(
                    leaveDecision.Value,
                    SimulationTimestamp.Create(4, 80))
                : OperationResult.Fail(leaveDecision.Error);
            OperationResult leaveExit = leaveApply.IsSuccess
                ? leaveSession.MarkPrototypeCustomerExitArrival(
                    SimulationTimestamp.Create(5, 100))
                : OperationResult.Fail(leaveApply.Error);
            bool leaveAction = leaveAccept.IsSuccess &&
                               leaveShelf.IsSuccess &&
                               leavePublish.IsSuccess &&
                               leavePrice.IsSuccess &&
                               leaveStart.IsSuccess &&
                               leaveBrowse.IsSuccess &&
                               leaveConsultation.IsSuccess &&
                               leaveDecision.IsSuccess &&
                               leaveDecision.Value.DecisionKind ==
                                   CustomerOfferDecisionKind.Leave &&
                               leaveApply.IsSuccess &&
                               leaveExit.IsSuccess &&
                               leaveSession.CustomerOfferActions.Revision ==
                                   leaveActionRevision + 1 &&
                               leaveSession.CustomerVisits.Revision ==
                                   leaveActorRevision + 2 &&
                               leaveSession.Inventory.Revision == leaveInventoryRevision &&
                               leaveSession.RetailBaskets.Revision == leaveBasketRevision &&
                               leaveSession.RetailOffers.Revision == leaveOfferRevision &&
                               leaveSession.RetailCheckouts.Revision == leaveCheckoutRevision &&
                               leaveSession.Orders.Revision == leaveOrderRevision &&
                               leaveSession.CustomerConsultations.Revision ==
                                   leaveConsultationRevision &&
                               leaveSession.TryGetPrototypeCustomerLeaveAction(
                                   out CustomerOfferDecisionActionRecord leaveRecord) &&
                               leaveRecord.IsLeave &&
                               !leaveRecord.HasReservation &&
                               leaveSession.TryGetPrototypeCustomerVisit(
                                   out CustomerVisitRecord declinedVisit) &&
                               declinedVisit.State == CustomerVisitState.Exited &&
                               declinedVisit.ExitReason ==
                                   CustomerVisitExitReason.OfferDeclined &&
                               leaveSession.RetailBaskets.Count == 0 &&
                               leaveSession.Inventory.ReservationCount == 0 &&
                               leaveSession.ValidateInvariants().IsSuccess;
            if (!leaveAction)
            {
                LogCustomerFlowSmokeFailure(
                    leaveApply.IsFailure
                        ? leaveApply.Error.Code
                        : leaveExit.IsFailure
                            ? leaveExit.Error.Code
                            : "smoke.leave-action-mismatch");
                yield break;
            }

            GarageStockFlowSession staleLeaveSession = GarageStockFlowSession.CreateArrived();
            OperationResult staleLeaveAccept = staleLeaveSession.AcceptArrivedDelivery();
            OperationResult staleLeaveShelf = staleLeaveSession.TransferItem(
                staleLeaveSession.ShelfContainerId);
            OperationResult staleLeavePublish = staleLeaveSession.PublishShelfOffer();
            OperationResult staleLeavePrice = staleLeaveSession.RetailOffers.SetOffer(
                staleLeaveSession.ShelfOfferId,
                staleLeaveSession.ProductId,
                staleLeaveSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1);
            OperationResult staleLeaveStart = staleLeaveSession.StartPrototypeCustomerVisit(
                SimulationTimestamp.Create(1, 20));
            OperationResult staleLeaveBrowse =
                staleLeaveSession.MarkPrototypeCustomerBrowseArrival(
                    SimulationTimestamp.Create(2, 40));
            OperationResult staleLeaveConsultation =
                staleLeaveSession.ConsultPrototypeCustomer(
                    SimulationTimestamp.Create(3, 60));
            OperationResult<CustomerOfferDecision> staleLeaveDecision =
                staleLeaveSession.EvaluatePrototypeCustomerOffer();
            OperationResult staleLeaveDrift = staleLeaveSession.RetailOffers.SetOffer(
                staleLeaveSession.ShelfOfferId,
                staleLeaveSession.ProductId,
                staleLeaveSession.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            long staleLeaveActionRevision = staleLeaveSession.CustomerOfferActions.Revision;
            long staleLeaveActorRevision = staleLeaveSession.CustomerVisits.Revision;
            long staleLeaveInventoryRevision = staleLeaveSession.Inventory.Revision;
            long staleLeaveBasketRevision = staleLeaveSession.RetailBaskets.Revision;
            long staleLeaveOfferRevision = staleLeaveSession.RetailOffers.Revision;
            long staleLeaveCheckoutRevision = staleLeaveSession.RetailCheckouts.Revision;
            long staleLeaveOrderRevision = staleLeaveSession.Orders.Revision;
            long staleLeaveConsultationRevision =
                staleLeaveSession.CustomerConsultations.Revision;
            OperationResult staleLeaveApply = staleLeaveDecision.IsSuccess
                ? staleLeaveSession.ApplyPrototypeCustomerLeave(
                    staleLeaveDecision.Value,
                    SimulationTimestamp.Create(4, 80))
                : OperationResult.Fail(staleLeaveDecision.Error);
            bool staleLeaveBlocked = staleLeaveAccept.IsSuccess &&
                                     staleLeaveShelf.IsSuccess &&
                                     staleLeavePublish.IsSuccess &&
                                     staleLeavePrice.IsSuccess &&
                                     staleLeaveStart.IsSuccess &&
                                     staleLeaveBrowse.IsSuccess &&
                                     staleLeaveConsultation.IsSuccess &&
                                     staleLeaveDecision.IsSuccess &&
                                     staleLeaveDecision.Value.DecisionKind ==
                                         CustomerOfferDecisionKind.Leave &&
                                     staleLeaveDrift.IsSuccess &&
                                     staleLeaveApply.Error ==
                                         CustomerOfferDecisionActionFailures.DecisionStale &&
                                     staleLeaveSession.CustomerOfferActions.Revision ==
                                         staleLeaveActionRevision &&
                                     staleLeaveSession.CustomerVisits.Revision ==
                                         staleLeaveActorRevision &&
                                     staleLeaveSession.Inventory.Revision ==
                                         staleLeaveInventoryRevision &&
                                     staleLeaveSession.RetailBaskets.Revision ==
                                         staleLeaveBasketRevision &&
                                     staleLeaveSession.RetailOffers.Revision ==
                                         staleLeaveOfferRevision &&
                                     staleLeaveSession.RetailCheckouts.Revision ==
                                         staleLeaveCheckoutRevision &&
                                     staleLeaveSession.Orders.Revision == staleLeaveOrderRevision &&
                                     staleLeaveSession.CustomerConsultations.Revision ==
                                         staleLeaveConsultationRevision &&
                                     staleLeaveSession.CustomerOfferActions.Count == 0 &&
                                     staleLeaveSession.RetailBaskets.Count == 0 &&
                                     staleLeaveSession.Inventory.ReservationCount == 0 &&
                                     staleLeaveSession.TryGetPrototypeCustomerVisit(
                                         out CustomerVisitRecord staleLeaveVisit) &&
                                     staleLeaveVisit.State == CustomerVisitState.Browsing &&
                                     staleLeaveSession.ValidateInvariants().IsSuccess;
            if (!staleLeaveBlocked)
            {
                LogCustomerFlowSmokeFailure(
                    staleLeaveApply.IsFailure
                        ? staleLeaveApply.Error.Code
                        : "smoke.stale-leave-mutated-authority");
                yield break;
            }

            GarageStockFlowSession routeFallbackSession = GarageStockFlowSession.CreateArrived();
            long fallbackInventoryRevision = routeFallbackSession.Inventory.Revision;
            long fallbackOrderRevision = routeFallbackSession.Orders.Revision;
            long fallbackOfferRevision = routeFallbackSession.RetailOffers.Revision;
            long fallbackBasketRevision = routeFallbackSession.RetailBaskets.Revision;
            long fallbackCheckoutRevision = routeFallbackSession.RetailCheckouts.Revision;
            long fallbackConsultationRevision =
                routeFallbackSession.CustomerConsultations.Revision;
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
                                 routeFallbackSession.CustomerConsultations.Revision ==
                                     fallbackConsultationRevision &&
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
            long timeoutConsultationRevision =
                timeoutSession.CustomerConsultations.Revision;
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
                                   timeoutSession.CustomerConsultations.Revision ==
                                       timeoutConsultationRevision &&
                                   timeoutSession.ValidateInvariants().IsSuccess;
            if (!timeoutFallback)
            {
                LogCustomerFlowSmokeFailure("smoke.timeout-fallback-mismatch");
                yield break;
            }

            Debug.Log(
                "GARAGE_CUSTOMER_VISIT_RUNTIME_SMOKE customer-visit=ok runtime-route=ok " +
                "pause=ok consultation=ok consultation-replay=ok decision-gated=ok " +
                "stale-consultation-blocked=ok offer-decision=ok buy-action=ok " +
                "stale-blocked=ok awaiting-checkout-gate=ok fulfilled=ok " +
                "checkout-station=ok station-focus=ok station-los=ok " +
                "shelf-bypass-blocked=ok checkout-start=ok " +
                "cash-payment=ok payment-receipt=ok economy-settlement=ok cash-ledger=ok " +
                "leave-action=ok stale-leave-blocked=ok " +
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

        private IEnumerator RunMotherboardAssemblySmoke()
        {
            yield return null;
            yield return new WaitForFixedUpdate();

            if (playerMotor == null ||
                playerCarry == null ||
                stockFlow == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                motherboardBinding == null ||
                motherboardBinding.PhysicalItem == null)
            {
                LogMotherboardAssemblySmokeFailure("smoke.context-missing");
                yield break;
            }

            playerMotor.SetPaused(false);
            GarageStockFlowSession session = stockFlow.EnsureInitialized();
            PhysicalItemProjection motherboard = motherboardBinding.PhysicalItem;
            int physicalInstanceId = motherboard.GetInstanceID();
            int physicalMotherboardCount = CountCanonicalMotherboardProjections(
                session.MotherboardItemId.Value);

            if (motherboardBinding.Runtime != stockFlow ||
                motherboardBinding.Seat != motherboardSeat ||
                motherboardBinding.Fastener != motherboardFastener ||
                motherboardBinding.InventoryItemIdValue !=
                    session.MotherboardItemId.Value ||
                motherboard.ItemIdValue != session.MotherboardItemId.Value ||
                physicalMotherboardCount != 1 ||
                session.Inventory.SerializedItemCount != 1 ||
                !session.TryGetMotherboardItem(out InventoryItemRecord looseItem) ||
                looseItem.Id != session.MotherboardItemId ||
                looseItem.ProductId != session.MotherboardProductId ||
                looseItem.ContainerId != session.WorldFloorContainerId ||
                motherboardFastener.FastenerIdValue !=
                    session.MotherboardFastenerId.Value ||
                motherboardFastener.FocusCollider == null ||
                motherboardFastener.FocusCollider.enabled ||
                !motherboardFastener.MatchesAuthorityState(AssemblySeatState.Empty) ||
                session.AssemblyBuild.MotherboardSeatState != AssemblySeatState.Empty ||
                session.AssemblyBuild.Revision != 0 ||
                session.AssemblyBuild.ReceiptCount != 0 ||
                session.AssemblyBuild.ValidateInvariants().IsFailure ||
                motherboardBinding.ValidateProjectionInvariant().IsFailure)
            {
                LogMotherboardAssemblySmokeFailure("smoke.authority-identity-mismatch");
                yield break;
            }

            long initialAssemblyRevision = session.AssemblyBuild.Revision;
            long initialInventoryRevision = session.Inventory.Revision;
            int initialReceiptCount = session.AssemblyBuild.ReceiptCount;

            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long settlementRevision = session.CheckoutSettlements.Revision;
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;

            OperationResult<PcComponentSpecification> specification =
                session.Components.Get(session.MotherboardProductId);
            AssemblyCompatibilityResult compatibility = specification.IsSuccess
                ? AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    specification.Value,
                    MotherboardFormFactor.MicroAtx)
                : AssemblyCompatibilityResult.Incompatible(specification.Error);
            OperationResult<PcComponentSpecification> mismatchSpecification =
                PcComponentSpecification.Create(
                    session.Catalog,
                    session.MotherboardProductId,
                    PcComponentKind.Motherboard,
                    MotherboardFormFactor.Atx);
            AssemblyCompatibilityResult mismatch = mismatchSpecification.IsSuccess
                ? AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    mismatchSpecification.Value,
                    MotherboardFormFactor.MicroAtx)
                : AssemblyCompatibilityResult.Incompatible(mismatchSpecification.Error);
            bool compatible = compatibility.IsCompatible &&
                              compatibility.Reason.IsNone;
            bool mismatchBlocked = !mismatch.IsCompatible &&
                                   mismatch.Reason ==
                                       AssemblyFailures.MotherboardFormFactorMismatch &&
                                   session.AssemblyBuild.Revision == 0 &&
                                   session.AssemblyBuild.ReceiptCount == 0;
            if (!compatible || !mismatchBlocked)
            {
                LogMotherboardAssemblySmokeFailure(
                    compatible ? "smoke.mismatch-not-blocked" : "smoke.compatibility-mismatch");
                yield break;
            }

            OperationResult pickup = playerCarry.TryPickup(motherboard);
            if (pickup.IsFailure ||
                playerCarry.HeldItem != motherboard ||
                !motherboardBinding.IsAuthorityInHands ||
                session.AssemblyBuild.Revision != initialAssemblyRevision ||
                session.Inventory.Revision != initialInventoryRevision + 1 ||
                session.AssemblyBuild.ReceiptCount != initialReceiptCount)
            {
                LogMotherboardAssemblySmokeFailure(
                    pickup.IsFailure ? pickup.Error.Code : "smoke.pickup-projection-mismatch");
                yield break;
            }

            MovePlayerToMotherboardSeat();
            OperationResult beginGuidedSeat = playerCarry.TrySetMotherboardSeatMode(true);
            bool previewReady = beginGuidedSeat.IsSuccess &&
                                playerCarry.IsMotherboardSeatMode &&
                                playerCarry.PlacementValid &&
                                playerCarry.CurrentMotherboardSeatStatus ==
                                    MotherboardSeatStatus.Valid &&
                                playerCarry.PlacementPreview != null &&
                                playerCarry.PlacementPreview.IsVisible &&
                                playerCarry.PlacementPreview.IsShowingValidPose &&
                                ApproximatelySamePose(
                                    playerCarry.PlacementPreview.CurrentPose,
                                    motherboardSeat.SnapPose);
            if (!previewReady)
            {
                LogMotherboardAssemblySmokeFailure(
                    beginGuidedSeat.IsFailure
                        ? beginGuidedSeat.Error.Code
                        : string.IsNullOrEmpty(playerCarry.LastFailureCode)
                            ? "smoke.preview-invalid"
                            : playerCarry.LastFailureCode);
                yield break;
            }

            OperationResult attach = playerCarry.TryConfirmMotherboardSeat();
            AssemblyBuildSnapshot attachedSnapshot = session.AssemblyBuild.GetSnapshot();
            bool attached = attach.IsSuccess &&
                            playerCarry.HeldItem == null &&
                            attachedSnapshot.MotherboardSeatState ==
                                AssemblySeatState.SeatedUnsecured &&
                            attachedSnapshot.MotherboardItemId == session.MotherboardItemId &&
                            session.TryGetMotherboardItem(out InventoryItemRecord seatedItem) &&
                            seatedItem.ContainerId == session.WorkbenchContainerId &&
                            session.AssemblyBuild.Revision == initialAssemblyRevision + 1 &&
                            session.Inventory.Revision == initialInventoryRevision + 2 &&
                            session.AssemblyBuild.ReceiptCount == initialReceiptCount + 1 &&
                            motherboardFastener.FocusCollider.enabled &&
                            motherboardFastener.MatchesAuthorityState(
                                AssemblySeatState.SeatedUnsecured) &&
                            motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                            ApproximatelySamePose(
                                new Pose(
                                    motherboard.transform.position,
                                    motherboard.transform.rotation),
                                motherboardSeat.SnapPose);
            if (!attached)
            {
                LogMotherboardAssemblySmokeFailure(
                    attach.IsFailure
                        ? attach.Error.Code
                        : "smoke.attach-projection-mismatch");
                yield break;
            }

            var attachReceipts = session.AssemblyBuild.GetReceipts();
            if (attachReceipts.Count != 1)
            {
                LogMotherboardAssemblySmokeFailure("smoke.attach-receipt-mismatch");
                yield break;
            }

            AssemblyOperationReceipt attachReceipt = attachReceipts[0];
            long attachedAssemblyRevision = session.AssemblyBuild.Revision;
            long attachedInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> attachReplay =
                session.AttachMotherboard(attachReceipt.OperationId);
            bool attachReplayed = attachReplay.IsSuccess &&
                                  ReferenceEquals(attachReplay.Value, attachReceipt) &&
                                  session.AssemblyBuild.Revision == attachedAssemblyRevision &&
                                  session.Inventory.Revision == attachedInventoryRevision &&
                                  session.AssemblyBuild.ReceiptCount == 1;
            if (!attachReplayed)
            {
                LogMotherboardAssemblySmokeFailure("smoke.attach-replay-mismatch");
                yield break;
            }

            OperationResult duplicateConfirm = playerCarry.TryConfirmMotherboardSeat();
            bool duplicateSeatConfirmBlocked = duplicateConfirm.IsFailure &&
                                               session.AssemblyBuild.Revision ==
                                                   attachedAssemblyRevision &&
                                               session.Inventory.Revision ==
                                                   attachedInventoryRevision &&
                                               session.AssemblyBuild.ReceiptCount == 1;
            if (!duplicateSeatConfirmBlocked)
            {
                LogMotherboardAssemblySmokeFailure("smoke.input-double-consumed");
                yield break;
            }

            MovePlayerToMotherboardFastener();
            long fastenerInventoryRevision = session.Inventory.Revision;
            OperationResult secure = playerCarry.TryOperateMotherboardFastener();
            AssemblyBuildSnapshot securedSnapshot = session.AssemblyBuild.GetSnapshot();
            var securedReceipts = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt secureReceipt = securedReceipts.Count > 1
                ? securedReceipts[1]
                : null;
            bool secured = secure.IsSuccess &&
                           secureReceipt != null &&
                           secureReceipt.OperationKind ==
                               AssemblyOperationKind.SecureMotherboardFastener &&
                           secureReceipt.SourceAttachOperationId == attachReceipt.OperationId &&
                           secureReceipt.SourceSecureOperationId.IsEmpty &&
                           secureReceipt.FastenerId == session.MotherboardFastenerId &&
                           securedSnapshot.MotherboardSeatState ==
                               AssemblySeatState.SeatedSecured &&
                           securedSnapshot.SecuredByOperationId == secureReceipt.OperationId &&
                           session.AssemblyBuild.Revision == initialAssemblyRevision + 2 &&
                           session.AssemblyBuild.ReceiptCount == initialReceiptCount + 2 &&
                           session.Inventory.Revision == fastenerInventoryRevision &&
                           motherboardFastener.MatchesAuthorityState(
                               AssemblySeatState.SeatedSecured) &&
                           motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            if (!secured)
            {
                LogMotherboardAssemblySmokeFailure(
                    secure.IsFailure ? secure.Error.Code : "smoke.secure-mismatch");
                yield break;
            }

            OperationResult<AssemblyOperationReceipt> secureReplay =
                session.SecureMotherboardFastener(
                    secureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.ExpectedAssemblyRevision);
            bool secureReplayed = secureReplay.IsSuccess &&
                                  ReferenceEquals(secureReplay.Value, secureReceipt) &&
                                  session.AssemblyBuild.Revision ==
                                      initialAssemblyRevision + 2 &&
                                  session.AssemblyBuild.ReceiptCount ==
                                      initialReceiptCount + 2 &&
                                  session.Inventory.Revision == fastenerInventoryRevision;
            if (!secureReplayed)
            {
                LogMotherboardAssemblySmokeFailure("smoke.secure-replay-mismatch");
                yield break;
            }

            Pose securedPose = new Pose(
                motherboard.transform.position,
                motherboard.transform.rotation);
            Transform securedParent = motherboard.transform.parent;
            OperationResult blockedDetach = playerCarry.TryPickup(motherboard);
            bool detachBlocked = blockedDetach.IsFailure &&
                                 blockedDetach.Error == AssemblyFailures.ComponentSecured &&
                                 playerCarry.HeldItem == null &&
                                 motherboard.transform.parent == securedParent &&
                                 ApproximatelySamePose(
                                     new Pose(
                                         motherboard.transform.position,
                                         motherboard.transform.rotation),
                                     securedPose) &&
                                 session.AssemblyBuild.MotherboardSeatState ==
                                     AssemblySeatState.SeatedSecured &&
                                 session.AssemblyBuild.Revision ==
                                     initialAssemblyRevision + 2 &&
                                 session.AssemblyBuild.ReceiptCount ==
                                     initialReceiptCount + 2 &&
                                 session.Inventory.Revision == fastenerInventoryRevision;
            if (!detachBlocked)
            {
                LogMotherboardAssemblySmokeFailure("smoke.secured-detach-not-blocked");
                yield break;
            }

            StableId<AssemblyOperationIdScope> directDetachOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.smoke-secured-detach.r000003");
            OperationResult<AssemblyOperationReceipt> authorityBlockedDetach =
                session.DetachMotherboard(directDetachOperationId);
            AssemblyBuildSnapshot authorityBlockedSnapshot =
                session.AssemblyBuild.GetSnapshot();
            bool authorityDetachBlocked = authorityBlockedDetach.IsFailure &&
                                          authorityBlockedDetach.Error ==
                                              AssemblyFailures.ComponentSecured &&
                                          authorityBlockedSnapshot.MotherboardSeatState ==
                                              securedSnapshot.MotherboardSeatState &&
                                          authorityBlockedSnapshot.MotherboardItemId ==
                                              securedSnapshot.MotherboardItemId &&
                                          authorityBlockedSnapshot.InstalledByOperationId ==
                                              securedSnapshot.InstalledByOperationId &&
                                          authorityBlockedSnapshot.SecuredByOperationId ==
                                              securedSnapshot.SecuredByOperationId &&
                                          session.AssemblyBuild.Revision ==
                                              initialAssemblyRevision + 2 &&
                                          session.AssemblyBuild.ReceiptCount ==
                                              initialReceiptCount + 2 &&
                                          session.Inventory.Revision ==
                                              fastenerInventoryRevision &&
                                          playerCarry.HeldItem == null &&
                                          motherboard.transform.parent == securedParent &&
                                          ApproximatelySamePose(
                                              new Pose(
                                                  motherboard.transform.position,
                                                  motherboard.transform.rotation),
                                              securedPose);
            if (!authorityDetachBlocked)
            {
                LogMotherboardAssemblySmokeFailure(
                    "smoke.secured-detach-authority-not-blocked");
                yield break;
            }

            OperationResult unsecure = playerCarry.TryOperateMotherboardFastener();
            AssemblyBuildSnapshot unsecuredSnapshot = session.AssemblyBuild.GetSnapshot();
            var unsecuredReceipts = session.AssemblyBuild.GetReceipts();
            AssemblyOperationReceipt unsecureReceipt = unsecuredReceipts.Count > 2
                ? unsecuredReceipts[2]
                : null;
            bool unsecured = unsecure.IsSuccess &&
                              unsecureReceipt != null &&
                              unsecureReceipt.OperationKind ==
                                  AssemblyOperationKind.UnsecureMotherboardFastener &&
                              unsecureReceipt.SourceAttachOperationId == attachReceipt.OperationId &&
                              unsecureReceipt.SourceSecureOperationId == secureReceipt.OperationId &&
                              unsecuredSnapshot.MotherboardSeatState ==
                                  AssemblySeatState.SeatedUnsecured &&
                              unsecuredSnapshot.SecuredByOperationId.IsEmpty &&
                              session.AssemblyBuild.Revision == initialAssemblyRevision + 3 &&
                              session.AssemblyBuild.ReceiptCount == initialReceiptCount + 3 &&
                              session.Inventory.Revision == fastenerInventoryRevision &&
                              motherboardFastener.MatchesAuthorityState(
                                  AssemblySeatState.SeatedUnsecured) &&
                              motherboardBinding.ValidateProjectionInvariant().IsSuccess;
            if (!unsecured)
            {
                LogMotherboardAssemblySmokeFailure(
                    unsecure.IsFailure ? unsecure.Error.Code : "smoke.unsecure-mismatch");
                yield break;
            }

            OperationResult<AssemblyOperationReceipt> unsecureReplay =
                session.UnsecureMotherboardFastener(
                    unsecureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.OperationId,
                    unsecureReceipt.ExpectedAssemblyRevision);
            bool unsecureReplayed = unsecureReplay.IsSuccess &&
                                    ReferenceEquals(unsecureReplay.Value, unsecureReceipt) &&
                                    session.AssemblyBuild.Revision ==
                                        initialAssemblyRevision + 3 &&
                                    session.AssemblyBuild.ReceiptCount ==
                                        initialReceiptCount + 3 &&
                                    session.Inventory.Revision == fastenerInventoryRevision;
            if (!unsecureReplayed)
            {
                LogMotherboardAssemblySmokeFailure("smoke.unsecure-replay-mismatch");
                yield break;
            }

            OperationResult detach = playerCarry.TryPickup(motherboard);
            AssemblyBuildSnapshot detachedSnapshot = session.AssemblyBuild.GetSnapshot();
            bool detached = detach.IsSuccess &&
                            playerCarry.HeldItem == motherboard &&
                            detachedSnapshot.MotherboardSeatState == AssemblySeatState.Empty &&
                            detachedSnapshot.MotherboardItemId.IsEmpty &&
                            session.AssemblyBuild.Revision == initialAssemblyRevision + 4 &&
                            session.Inventory.Revision == initialInventoryRevision + 3 &&
                            session.AssemblyBuild.ReceiptCount == initialReceiptCount + 4 &&
                            motherboardBinding.IsAuthorityInHands;
            if (!detached)
            {
                LogMotherboardAssemblySmokeFailure(
                    detach.IsFailure ? detach.Error.Code : "smoke.detach-projection-mismatch");
                yield break;
            }

            OperationResult recovery = playerCarry.TryRecoverHeldItem();
            AssemblyBuildSnapshot recoveredSnapshot = session.AssemblyBuild.GetSnapshot();
            AssemblyOperationReceipt detachReceipt = null;
            AssemblyOperationReceipt recoveryAttachReceipt = null;
            var finalReceipts = session.AssemblyBuild.GetReceipts();
            for (int index = 0; index < finalReceipts.Count; index++)
            {
                AssemblyOperationReceipt receipt = finalReceipts[index];
                if (receipt.OperationKind == AssemblyOperationKind.DetachMotherboard)
                {
                    detachReceipt = receipt;
                }
                else if (receipt.OperationKind == AssemblyOperationKind.AttachMotherboard &&
                         receipt.OperationId != attachReceipt.OperationId)
                {
                    recoveryAttachReceipt = receipt;
                }
            }

            bool receiptLineage = detachReceipt != null &&
                                  recoveryAttachReceipt != null &&
                                  detachReceipt.SourceAttachOperationId ==
                                      attachReceipt.OperationId &&
                                  detachReceipt.AssemblyRevision ==
                                      initialAssemblyRevision + 4 &&
                                  detachReceipt.InventoryRevision ==
                                      initialInventoryRevision + 3 &&
                                  recoveryAttachReceipt.AssemblyRevision ==
                                      initialAssemblyRevision + 5 &&
                                  recoveryAttachReceipt.InventoryRevision ==
                                      initialInventoryRevision + 4 &&
                                  recoveredSnapshot.InstalledByOperationId ==
                                      recoveryAttachReceipt.OperationId;
            bool identityStable = motherboard.GetInstanceID() == physicalInstanceId &&
                                  motherboardBinding.PhysicalItem == motherboard &&
                                  motherboard.ItemIdValue == session.MotherboardItemId.Value &&
                                  motherboardBinding.InventoryItemIdValue ==
                                      session.MotherboardItemId.Value &&
                                  CountCanonicalMotherboardProjections(
                                      session.MotherboardItemId.Value) == 1;
            bool recovered = recovery.IsSuccess &&
                             playerCarry.HeldItem == null &&
                             recoveredSnapshot.MotherboardSeatState ==
                                 AssemblySeatState.SeatedUnsecured &&
                             recoveredSnapshot.MotherboardItemId == session.MotherboardItemId &&
                             session.TryGetMotherboardItem(out InventoryItemRecord recoveredItem) &&
                             recoveredItem.Id == session.MotherboardItemId &&
                             recoveredItem.ProductId == session.MotherboardProductId &&
                             recoveredItem.ContainerId == session.WorkbenchContainerId &&
                             session.Inventory.SerializedItemCount == 1 &&
                             session.AssemblyBuild.Revision == initialAssemblyRevision + 5 &&
                             session.Inventory.Revision == initialInventoryRevision + 4 &&
                             session.AssemblyBuild.ReceiptCount == initialReceiptCount + 5 &&
                             motherboardFastener.MatchesAuthorityState(
                                 AssemblySeatState.SeatedUnsecured) &&
                             motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            bool authorityIsolated = session.Orders.Revision == orderRevision &&
                                     session.RetailOffers.Revision == offerRevision &&
                                     session.RetailBaskets.Revision == basketRevision &&
                                     session.RetailCheckouts.Revision == checkoutRevision &&
                                     session.CheckoutSettlements.Revision == settlementRevision &&
                                     session.CustomerVisits.Revision == visitRevision &&
                                     session.CustomerConsultations.Revision ==
                                         consultationRevision &&
                                     session.CustomerOfferActions.Revision == actionRevision;
            if (!recovered || !identityStable || !authorityIsolated || !receiptLineage)
            {
                LogMotherboardAssemblySmokeFailure(
                    recovery.IsFailure
                        ? recovery.Error.Code
                        : !identityStable
                            ? "smoke.identity-mismatch"
                            : !authorityIsolated
                                ? "smoke.authority-isolation-mismatch"
                                : !receiptLineage
                                    ? "smoke.receipt-lineage-mismatch"
                                    : "smoke.recovery-projection-mismatch");
                yield break;
            }

            long recoveredAssemblyRevision = session.AssemblyBuild.Revision;
            long recoveredInventoryRevision = session.Inventory.Revision;
            int recoveredReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose recoveredPose = new Pose(
                motherboard.transform.position,
                motherboard.transform.rotation);
            Transform recoveredParent = motherboard.transform.parent;
            OperationResult<AssemblyOperationReceipt> delayedSecureReplay =
                session.SecureMotherboardFastener(
                    secureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.ExpectedAssemblyRevision);
            bool secureDelayedReplayed = delayedSecureReplay.IsSuccess &&
                                         ReferenceEquals(
                                             delayedSecureReplay.Value,
                                             secureReceipt) &&
                                         session.AssemblyBuild.Revision ==
                                             recoveredAssemblyRevision &&
                                         session.AssemblyBuild.ReceiptCount ==
                                             recoveredReceiptCount &&
                                         session.Inventory.Revision ==
                                             recoveredInventoryRevision &&
                                         session.AssemblyBuild.MotherboardSeatState ==
                                             AssemblySeatState.SeatedUnsecured &&
                                         motherboard.transform.parent == recoveredParent &&
                                         ApproximatelySamePose(
                                             new Pose(
                                                 motherboard.transform.position,
                                                 motherboard.transform.rotation),
                                             recoveredPose);
            if (!secureDelayedReplayed)
            {
                LogMotherboardAssemblySmokeFailure(
                    "smoke.secure-delayed-replay-mismatch");
                yield break;
            }

            OperationResult<AssemblyOperationReceipt> delayedUnsecureReplay =
                session.UnsecureMotherboardFastener(
                    unsecureReceipt.OperationId,
                    attachReceipt.OperationId,
                    secureReceipt.OperationId,
                    unsecureReceipt.ExpectedAssemblyRevision);
            bool unsecureDelayedReplayed = delayedUnsecureReplay.IsSuccess &&
                                           ReferenceEquals(
                                               delayedUnsecureReplay.Value,
                                               unsecureReceipt) &&
                                           session.AssemblyBuild.Revision ==
                                               recoveredAssemblyRevision &&
                                           session.AssemblyBuild.ReceiptCount ==
                                               recoveredReceiptCount &&
                                           session.Inventory.Revision ==
                                               recoveredInventoryRevision &&
                                           session.AssemblyBuild.MotherboardSeatState ==
                                               AssemblySeatState.SeatedUnsecured &&
                                           motherboard.transform.parent == recoveredParent &&
                                           ApproximatelySamePose(
                                               new Pose(
                                                   motherboard.transform.position,
                                                   motherboard.transform.rotation),
                                               recoveredPose) &&
                                           motherboardFastener.MatchesAuthorityState(
                                               AssemblySeatState.SeatedUnsecured) &&
                                           motherboardBinding.ValidateProjectionInvariant()
                                               .IsSuccess &&
                                           session.ValidateInvariants().IsSuccess;
            if (!unsecureDelayedReplayed)
            {
                LogMotherboardAssemblySmokeFailure(
                    "smoke.unsecure-delayed-replay-mismatch");
                yield break;
            }

            Debug.Log(
                "GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE assembly-flow=ok " +
                "compatible=ok mismatch-blocked=ok attach=ok attach-replay=ok " +
                "fastener=ok secure=ok secure-replay=ok secure-delayed-replay=ok " +
                "detach-blocked=ok detach-authority-blocked=ok " +
                "unsecure=ok unsecure-replay=ok unsecure-delayed-replay=ok detach=ok " +
                "duplicate-seat-confirm-blocked=ok authority-isolated=ok " +
                "identity-stable=ok recovery=ok");
            yield return new WaitForEndOfFrame();
        }

        private static void LogMotherboardAssemblySmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_MOTHERBOARD_ASSEMBLY_RUNTIME_SMOKE assembly-flow=failed code={code}");
        }

        private static int CountCanonicalMotherboardProjections(string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in FindObjectsByType<PhysicalItemProjection>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (item != null && item.ItemIdValue == canonicalItemId)
                {
                    count++;
                }
            }

            return count;
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

        private void MovePlayerToCheckoutStation(float distance)
        {
            Collider targetCollider = checkoutStation.InteractionCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 approach = -targetCollider.transform.forward;
            approach.y = 0f;
            approach.Normalize();
            Vector3 playerPosition = target + (approach * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private void MovePlayerToMotherboardSeat()
        {
            Vector3 target = motherboardSeat.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));

            Camera playerCamera = playerMotor.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.transform.rotation = Quaternion.LookRotation(
                    target - playerCamera.transform.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private void MovePlayerToMotherboardFastener()
        {
            Vector3 target = motherboardFastener.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));

            Camera playerCamera = playerMotor.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.transform.rotation = Quaternion.LookRotation(
                    target - playerCamera.transform.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private void MovePlayerToPhysicalItem(
            PhysicalItemProjection item,
            Vector3 approachDirection,
            float distance)
        {
            Vector3 target = item.Body != null
                ? item.Body.worldCenterOfMass
                : item.transform.position;
            Vector3 approach = approachDirection.normalized;
            Vector3 playerPosition = target + (approach * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
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

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.SqrMagnitude(left.position - right.position) <= 0.000001f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.1f;
        }

    }
}
