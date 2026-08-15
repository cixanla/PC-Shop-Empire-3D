using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Economy;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Small deterministic composition root for the first visible order-to-shelf gameplay slice.
    /// Domain authorities remain the only source of stock and order truth; Unity objects only project it.
    /// </summary>
    public sealed class GarageStockFlowSession
    {
        public const string ProductIdValue = "catalog.gpu.northstar-a60";
        public const string ProductCategoryIdValue = "catalog.category.graphics-cards";
        public const string ItemInstanceIdValue = "inventory.item.northstar-a60-001";
        public const string PurchaseOrderIdValue = "purchase-order.garage-demo-001";
        public const string SupplierIdValue = "supplier.northstar-distribution";
        public const string DeliveryIdValue = "delivery.garage-demo-001";
        public const string ReceivingContainerIdValue = "inventory.container.receiving-bay-a";
        public const string HandsContainerIdValue = "inventory.container.player-hands";
        public const string ShelfContainerIdValue = "inventory.container.retail-shelf-a";
        public const string WorldFloorContainerIdValue = "inventory.container.world-floor";
        public const string ShelfOfferIdValue = "retail.offer.garage-shelf-a-northstar-a60";
        public const string PrototypeCustomerIdValue = "retail.customer.demo-walk-in-001";
        public const string PrototypeActorCustomerIdValue = "actors.customer.demo-walk-in-001";
        public const string PrototypeCustomerIntentIdValue = "actors.intent.demo-a60-001";
        public const string PrototypeCustomerVisitIdValue = "actors.visit.demo-walk-in-001";
        public const string PrototypeCustomerConsultationIdValue =
            "actors.consultation.demo-walk-in-001";
        public const string PrototypeCustomerBindingIdValue =
            "retail.customer-binding.demo-walk-in-001";
        public const string PrototypeCustomerBuyActionIdValue =
            "retail.offer-action.demo-walk-in-001";
        public const string PrototypeCustomerLeaveActionIdValue =
            "retail.offer-action.demo-walk-in-leave-001";
        public const string PrototypeBasketIdValue = "retail.basket.demo-customer-001";
        public const string PrototypeBasketLineIdValue = "retail.basket-line.demo-a60-001";
        public const string PrototypeCheckoutIdValue = "retail.checkout.demo-customer-001";
        public const string PrototypeCheckoutCompletionIdValue =
            "retail.checkout-completion.demo-customer-001";
        public const string PrototypeCheckoutSettlementIdValue =
            "economy.checkout-settlement.demo-customer-001";
        public const string PrototypeLedgerTransactionIdValue =
            "economy.ledger-transaction.demo-customer-001";
        public const string PrototypeReservationIdValue =
            "inventory.reservation.demo-basket-a60-001";
        public const string PrototypeClaimIdValue =
            "inventory.claim.retail-basket-demo-001";
        public const string PrototypeCurrencyCode = "EUR";
        public const long PrototypePriceMinorUnits = 54_999;
        public const long PrototypeUnitCostMinorUnits = 42_000;
        public const long PrototypeMaximumAcceptedPriceMinorUnits = 60_000;
        public const string ProductDisplayName = "Northstar A60 Ekran Kartı";

        private GarageStockFlowSession(
            ProductCatalog catalog,
            InventoryAuthority inventory,
            PurchaseOrderAuthority orders,
            ShelfOfferAuthority retailOffers,
            RetailBasketAuthority retailBaskets,
            RetailCheckoutAuthority retailCheckouts,
            CheckoutSettlementAuthority checkoutSettlements,
            CustomerVisitAuthority customerVisits,
            CustomerConsultationAuthority customerConsultations,
            CustomerOfferDecisionActionAuthority customerOfferActions,
            CustomerRetailIdentityBinding prototypeCustomerBinding)
        {
            Catalog = catalog;
            Inventory = inventory;
            Orders = orders;
            RetailOffers = retailOffers;
            RetailBaskets = retailBaskets;
            RetailCheckouts = retailCheckouts;
            CheckoutSettlements = checkoutSettlements;
            CustomerVisits = customerVisits;
            CustomerConsultations = customerConsultations;
            CustomerOfferActions = customerOfferActions;
            PrototypeCustomerBinding = prototypeCustomerBinding;
        }

        public ProductCatalog Catalog { get; }

        public InventoryAuthority Inventory { get; }

        public PurchaseOrderAuthority Orders { get; }

        public ShelfOfferAuthority RetailOffers { get; }

        public RetailBasketAuthority RetailBaskets { get; }

        public RetailCheckoutAuthority RetailCheckouts { get; }

        public CheckoutSettlementAuthority CheckoutSettlements { get; }

        public CustomerVisitAuthority CustomerVisits { get; }

        public CustomerConsultationAuthority CustomerConsultations { get; }

        public CustomerOfferDecisionActionAuthority CustomerOfferActions { get; }

        public CustomerRetailIdentityBinding PrototypeCustomerBinding { get; }

        public StableId<ProductDefinitionIdScope> ProductId =>
            StableId<ProductDefinitionIdScope>.Parse(ProductIdValue);

        public StableId<ItemInstanceIdScope> ItemId =>
            StableId<ItemInstanceIdScope>.Parse(ItemInstanceIdValue);

        public StableId<PurchaseOrderIdScope> OrderId =>
            StableId<PurchaseOrderIdScope>.Parse(PurchaseOrderIdValue);

        public StableId<ContainerIdScope> ReceivingContainerId =>
            StableId<ContainerIdScope>.Parse(ReceivingContainerIdValue);

        public StableId<ContainerIdScope> HandsContainerId =>
            StableId<ContainerIdScope>.Parse(HandsContainerIdValue);

        public StableId<ContainerIdScope> ShelfContainerId =>
            StableId<ContainerIdScope>.Parse(ShelfContainerIdValue);

        public StableId<ContainerIdScope> WorldFloorContainerId =>
            StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue);

        public StableId<ShelfOfferIdScope> ShelfOfferId =>
            StableId<ShelfOfferIdScope>.Parse(ShelfOfferIdValue);

        public StableId<RetailCustomerIdScope> PrototypeCustomerId =>
            StableId<RetailCustomerIdScope>.Parse(PrototypeCustomerIdValue);

        public StableId<CustomerIdScope> PrototypeActorCustomerId =>
            StableId<CustomerIdScope>.Parse(PrototypeActorCustomerIdValue);

        public StableId<CustomerIntentIdScope> PrototypeCustomerIntentId =>
            StableId<CustomerIntentIdScope>.Parse(PrototypeCustomerIntentIdValue);

        public StableId<CustomerVisitIdScope> PrototypeCustomerVisitId =>
            StableId<CustomerVisitIdScope>.Parse(PrototypeCustomerVisitIdValue);

        public StableId<CustomerConsultationIdScope> PrototypeCustomerConsultationId =>
            StableId<CustomerConsultationIdScope>.Parse(
                PrototypeCustomerConsultationIdValue);

        public StableId<CustomerRetailIdentityBindingIdScope> PrototypeCustomerBindingId =>
            StableId<CustomerRetailIdentityBindingIdScope>.Parse(
                PrototypeCustomerBindingIdValue);

        public StableId<CustomerOfferDecisionActionIdScope> PrototypeCustomerBuyActionId =>
            StableId<CustomerOfferDecisionActionIdScope>.Parse(
                PrototypeCustomerBuyActionIdValue);

        public StableId<CustomerOfferDecisionActionIdScope> PrototypeCustomerLeaveActionId =>
            StableId<CustomerOfferDecisionActionIdScope>.Parse(
                PrototypeCustomerLeaveActionIdValue);

        public StableId<RetailBasketIdScope> PrototypeBasketId =>
            StableId<RetailBasketIdScope>.Parse(PrototypeBasketIdValue);

        public StableId<RetailBasketLineIdScope> PrototypeBasketLineId =>
            StableId<RetailBasketLineIdScope>.Parse(PrototypeBasketLineIdValue);

        public StableId<RetailCheckoutIdScope> PrototypeCheckoutId =>
            StableId<RetailCheckoutIdScope>.Parse(PrototypeCheckoutIdValue);

        public StableId<RetailCheckoutCompletionIdScope> PrototypeCheckoutCompletionId =>
            StableId<RetailCheckoutCompletionIdScope>.Parse(
                PrototypeCheckoutCompletionIdValue);

        public StableId<EconomyCheckoutSettlementIdScope> PrototypeCheckoutSettlementId =>
            StableId<EconomyCheckoutSettlementIdScope>.Parse(
                PrototypeCheckoutSettlementIdValue);

        public StableId<EconomyLedgerTransactionIdScope> PrototypeLedgerTransactionId =>
            StableId<EconomyLedgerTransactionIdScope>.Parse(
                PrototypeLedgerTransactionIdValue);

        public StableId<ReservationIdScope> PrototypeReservationId =>
            StableId<ReservationIdScope>.Parse(PrototypeReservationIdValue);

        public StableId<InventoryClaimIdScope> PrototypeClaimId =>
            StableId<InventoryClaimIdScope>.Parse(PrototypeClaimIdValue);

        public PurchaseOrderRecord Order
        {
            get
            {
                if (!Orders.TryGetOrder(OrderId, out PurchaseOrderRecord order))
                {
                    throw new System.InvalidOperationException("The prototype purchase order is missing.");
                }

                return order;
            }
        }

        public static GarageStockFlowSession CreateArrived()
        {
            ProductDefinition product = ProductDefinition.Create(
                StableId<ProductDefinitionIdScope>.Parse(ProductIdValue),
                StableId<ProductCategoryIdScope>.Parse(ProductCategoryIdValue),
                ProductDisplayName,
                ProductTrackingPolicy.SerializedInstance,
                1095).Value;
            ProductCatalog catalog = ProductCatalog.Create(new[] { product }).Value;
            InventoryAuthority inventory = InventoryAuthority.Create(catalog).Value;
            RegisterContainer(
                inventory,
                ReceivingContainerIdValue,
                InventoryContainerKind.Receiving,
                8);
            RegisterContainer(
                inventory,
                HandsContainerIdValue,
                InventoryContainerKind.ActorHands,
                1);
            RegisterContainer(
                inventory,
                ShelfContainerIdValue,
                InventoryContainerKind.Shelf,
                8);
            RegisterContainer(
                inventory,
                WorldFloorContainerIdValue,
                InventoryContainerKind.WorldFloor,
                8);

            PurchaseOrderAuthority orders = PurchaseOrderAuthority.Create(catalog).Value;
            ShelfOfferAuthority retailOffers = ShelfOfferAuthority.Create(catalog, inventory).Value;
            RetailBasketAuthority retailBaskets =
                RetailBasketAuthority.Create(retailOffers, inventory).Value;
            RetailCheckoutAuthority retailCheckouts =
                RetailCheckoutAuthority.Create(retailOffers, retailBaskets, inventory).Value;
            CheckoutSettlementAuthority checkoutSettlements =
                CheckoutSettlementAuthority.Create(retailCheckouts).Value;
            CustomerVisitAuthority customerVisits = CustomerVisitAuthority.Create(
                catalog,
                SimulationDuration.FromMilliseconds(60_000),
                CustomerVisitAuthority.RequiredRouteAttemptLimit).Value;
            CustomerConsultationAuthority customerConsultations =
                CustomerConsultationAuthority.Create(customerVisits).Value;
            CustomerOfferDecisionActionAuthority customerOfferActions =
                CustomerOfferDecisionActionAuthority.Create(
                    retailOffers,
                    retailBaskets,
                    customerVisits,
                    customerConsultations).Value;
            CustomerRetailIdentityBinding prototypeCustomerBinding =
                CustomerRetailIdentityBinding.Create(
                    StableId<CustomerRetailIdentityBindingIdScope>.Parse(
                        PrototypeCustomerBindingIdValue),
                    StableId<CustomerIdScope>.Parse(PrototypeActorCustomerIdValue),
                    StableId<RetailCustomerIdScope>.Parse(
                        PrototypeCustomerIdValue)).Value;
            StableId<PurchaseOrderIdScope> orderId =
                StableId<PurchaseOrderIdScope>.Parse(PurchaseOrderIdValue);
            StableId<DeliveryIdScope> deliveryId =
                StableId<DeliveryIdScope>.Parse(DeliveryIdValue);
            StableId<ProductDefinitionIdScope> productId =
                StableId<ProductDefinitionIdScope>.Parse(ProductIdValue);
            InventoryUnitCost unitCost = InventoryUnitCost.Create(
                PrototypeCurrencyCode,
                PrototypeUnitCostMinorUnits).Value;
            PurchaseOrderLine line = PurchaseOrderLine.Create(productId, 1, unitCost).Value;
            InventorySerializedIntake serialized = InventorySerializedIntake.Create(
                StableId<ItemInstanceIdScope>.Parse(ItemInstanceIdValue),
                productId,
                InventoryCondition.New,
                unitCost).Value;
            InventoryIntake intake = InventoryIntake.Create(
                new[] { serialized },
                System.Array.Empty<InventoryBatchIntake>()).Value;
            DeliveryManifest manifest = DeliveryManifest.Create(deliveryId, intake).Value;

            RequireSuccess(orders.PlaceOrder(
                orderId,
                StableId<SupplierIdScope>.Parse(SupplierIdValue),
                new[] { line },
                Time(1)));
            RequireSuccess(orders.ConfirmOrder(orderId, deliveryId, Time(2), Time(3), Time(5)));
            RequireSuccess(orders.DispatchOrder(orderId, Time(3)));
            RequireSuccess(orders.RegisterArrival(orderId, manifest, Time(4)));

            var session = new GarageStockFlowSession(
                catalog,
                inventory,
                orders,
                retailOffers,
                retailBaskets,
                retailCheckouts,
                checkoutSettlements,
                customerVisits,
                customerConsultations,
                customerOfferActions,
                prototypeCustomerBinding);
            RequireSuccess(session.ValidateInvariants());
            return session;
        }

        public OperationResult AcceptArrivedDelivery()
        {
            return Orders.AcceptDelivery(OrderId, ReceivingContainerId, Inventory, Time(5));
        }

        public OperationResult TransferItem(StableId<ContainerIdScope> targetContainerId)
        {
            return Inventory.TransferSerializedItem(ItemId, targetContainerId);
        }

        public bool TryGetItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(ItemId, out item);
        }

        public OperationResult PublishShelfOffer()
        {
            return RetailOffers.SetOffer(
                ShelfOfferId,
                ProductId,
                ShelfContainerId,
                PrototypeCurrencyCode,
                PrototypePriceMinorUnits);
        }

        public bool TryGetShelfOffer(out ShelfOfferRecord offer)
        {
            return RetailOffers.TryGetOfferForShelfProduct(
                ShelfContainerId,
                ProductId,
                out offer);
        }

        public OperationResult<CustomerOfferDecision> EvaluatePrototypeCustomerOffer()
        {
            if (!TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit) ||
                !TryGetShelfOffer(out ShelfOfferRecord offer))
            {
                return OperationResult<CustomerOfferDecision>.Fail(
                    CustomerOfferDecisionFailures.InputInvalid);
            }

            OperationResult<ShelfPrice> maximumAcceptedPrice = ShelfPrice.Create(
                PrototypeCurrencyCode,
                PrototypeMaximumAcceptedPriceMinorUnits);
            TryGetPrototypeCustomerConsultation(
                out CustomerConsultationRecord consultation);
            return maximumAcceptedPrice.IsFailure
                ? OperationResult<CustomerOfferDecision>.Fail(
                    CustomerOfferDecisionFailures.InputInvalid)
                : CustomerOfferDecisionEvaluator.Evaluate(
                    visit,
                    consultation,
                    offer,
                    maximumAcceptedPrice.Value);
        }

        public OperationResult ConsultPrototypeCustomer(SimulationTimestamp at)
        {
            if (!TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit))
            {
                return OperationResult.Fail(
                    CustomerConsultationFailures.InputInvalid);
            }

            return CustomerConsultations.RecordConsultation(
                PrototypeCustomerConsultationId,
                visit,
                at);
        }

        public bool TryGetPrototypeCustomerConsultation(
            out CustomerConsultationRecord consultation)
        {
            return CustomerConsultations.TryGetConsultation(
                PrototypeCustomerConsultationId,
                out consultation);
        }

        public OperationResult ReservePrototypeCustomerBasket()
        {
            return RetailBaskets.ReserveSerializedOffer(
                PrototypeBasketLineId,
                PrototypeBasketId,
                PrototypeCustomerId,
                ShelfOfferId,
                ItemId,
                PrototypeReservationId,
                PrototypeClaimId);
        }

        public OperationResult ApplyPrototypeCustomerBuy(
            CustomerOfferDecision sourceDecision,
            SimulationTimestamp at)
        {
            return CustomerOfferActions.ApplyBuy(
                PrototypeCustomerBuyActionId,
                PrototypeCustomerBinding,
                sourceDecision,
                PrototypeBasketLineId,
                PrototypeBasketId,
                ItemId,
                PrototypeReservationId,
                PrototypeClaimId,
                at);
        }

        public bool TryGetPrototypeCustomerBuyAction(
            out CustomerOfferDecisionActionRecord action)
        {
            return CustomerOfferActions.TryGetAction(
                PrototypeCustomerBuyActionId,
                out action);
        }

        public OperationResult ApplyPrototypeCustomerLeave(
            CustomerOfferDecision sourceDecision,
            SimulationTimestamp at)
        {
            return CustomerOfferActions.ApplyLeave(
                PrototypeCustomerLeaveActionId,
                PrototypeCustomerBinding,
                sourceDecision,
                at);
        }

        public bool TryGetPrototypeCustomerLeaveAction(
            out CustomerOfferDecisionActionRecord action)
        {
            return CustomerOfferActions.TryGetAction(
                PrototypeCustomerLeaveActionId,
                out action);
        }

        public OperationResult ReleasePrototypeCustomerBasket()
        {
            if (TryGetPrototypeCheckout(out _))
            {
                return OperationResult.Fail(StockProjectionFailures.CheckoutActive);
            }

            return RetailBaskets.ReleaseLine(PrototypeBasketLineId);
        }

        public bool TryGetPrototypeBasketLine(out RetailBasketLineRecord line)
        {
            return RetailBaskets.TryGetLine(PrototypeBasketLineId, out line);
        }

        public OperationResult BeginPrototypeCheckout()
        {
            return RetailCheckouts.BeginCheckout(
                PrototypeCheckoutId,
                PrototypeBasketId,
                PrototypeCustomerId,
                Time(6));
        }

        public bool TryGetPrototypeCheckout(out RetailCheckoutRecord checkout)
        {
            return RetailCheckouts.TryGetCheckout(PrototypeCheckoutId, out checkout);
        }

        public OperationResult CompletePrototypeCheckout()
        {
            return SettlePrototypeCashCheckout();
        }

        public OperationResult SettlePrototypeCashCheckout()
        {
            return CheckoutSettlements.SettleCashCheckout(
                PrototypeCheckoutSettlementId,
                PrototypeLedgerTransactionId,
                PrototypeCheckoutCompletionId,
                PrototypeCheckoutId,
                PrototypeCurrencyCode,
                PrototypePriceMinorUnits,
                Time(7));
        }

        public bool TryGetPrototypeCheckoutCompletion(
            out RetailCheckoutCompletionRecord completion)
        {
            return RetailCheckouts.TryGetCompletion(
                PrototypeCheckoutCompletionId,
                out completion);
        }

        public bool TryGetPrototypeCheckoutSettlement(
            out CheckoutSettlementReceipt receipt)
        {
            return CheckoutSettlements.TryGetSettlement(
                PrototypeCheckoutSettlementId,
                out receipt);
        }

        public bool TryGetPrototypeLedgerTransaction(
            out EconomyLedgerTransactionRecord transaction)
        {
            return CheckoutSettlements.TryGetTransaction(
                PrototypeLedgerTransactionId,
                out transaction);
        }

        public OperationResult StartPrototypeCustomerVisit(SimulationTimestamp at)
        {
            return CustomerVisits.StartVisit(
                PrototypeCustomerVisitId,
                PrototypeCustomerIntentId,
                PrototypeActorCustomerId,
                ProductId,
                CustomerNeedKind.GraphicsUpgrade,
                at);
        }

        public OperationResult MarkPrototypeCustomerBrowseArrival(SimulationTimestamp at)
        {
            return CustomerVisits.MarkBrowseArrival(PrototypeCustomerVisitId, at);
        }

        public OperationResult BeginPrototypeCustomerCheckoutNavigation(SimulationTimestamp at)
        {
            return CustomerVisits.BeginCheckoutNavigation(PrototypeCustomerVisitId, at);
        }

        public OperationResult MarkPrototypeCustomerCheckoutArrival(SimulationTimestamp at)
        {
            return CustomerVisits.MarkCheckoutArrival(PrototypeCustomerVisitId, at);
        }

        public OperationResult BeginPrototypeCustomerExit(
            CustomerVisitExitReason reason,
            SimulationTimestamp at)
        {
            return CustomerVisits.BeginExit(PrototypeCustomerVisitId, reason, at);
        }

        public OperationResult MarkPrototypeCustomerExitArrival(SimulationTimestamp at)
        {
            return CustomerVisits.MarkExitArrival(PrototypeCustomerVisitId, at);
        }

        public OperationResult ReportPrototypeCustomerRouteFailure(SimulationTimestamp at)
        {
            return CustomerVisits.ReportRouteFailure(PrototypeCustomerVisitId, at);
        }

        public OperationResult AdvanceCustomerTime(SimulationTimestamp now)
        {
            return CustomerVisits.AdvanceTime(now);
        }

        public bool TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit)
        {
            return CustomerVisits.TryGetVisit(PrototypeCustomerVisitId, out visit);
        }

        public OperationResult ValidateInvariants()
        {
            OperationResult orderResult = Orders.ValidateInvariants();
            if (orderResult.IsFailure)
            {
                return orderResult;
            }

            OperationResult inventoryResult = Inventory.ValidateInvariants();
            if (inventoryResult.IsFailure)
            {
                return inventoryResult;
            }

            OperationResult offerResult = RetailOffers.ValidateInvariants();
            if (offerResult.IsFailure)
            {
                return offerResult;
            }

            OperationResult basketResult = RetailBaskets.ValidateInvariants();
            if (basketResult.IsFailure)
            {
                return basketResult;
            }

            OperationResult checkoutResult = RetailCheckouts.ValidateInvariants();
            if (checkoutResult.IsFailure)
            {
                return checkoutResult;
            }

            OperationResult settlementResult = CheckoutSettlements.ValidateInvariants();
            if (settlementResult.IsFailure)
            {
                return settlementResult;
            }

            OperationResult visitResult = CustomerVisits.ValidateInvariants();
            if (visitResult.IsFailure)
            {
                return visitResult;
            }

            OperationResult consultationResult =
                CustomerConsultations.ValidateInvariants();
            return consultationResult.IsFailure
                ? consultationResult
                : CustomerOfferActions.ValidateInvariants();
        }

        private static void RegisterContainer(
            InventoryAuthority inventory,
            string id,
            InventoryContainerKind kind,
            int capacity)
        {
            InventoryContainerDefinition definition = InventoryContainerDefinition.Create(
                StableId<ContainerIdScope>.Parse(id),
                kind,
                capacity).Value;
            RequireSuccess(inventory.RegisterContainer(definition));
        }

        private static SimulationTimestamp Time(long tick)
        {
            return SimulationTimestamp.Create(tick, tick * 1000L);
        }

        private static void RequireSuccess(OperationResult result)
        {
            if (result.IsFailure)
            {
                throw new System.InvalidOperationException(
                    $"Prototype stock-flow composition failed: {result.Error.Code}");
            }
        }
    }
}
