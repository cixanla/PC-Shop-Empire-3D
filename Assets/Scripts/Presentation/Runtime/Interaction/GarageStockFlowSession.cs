using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
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
        public const string MotherboardProductIdValue = "catalog.motherboard.northstar-mb-matx";
        public const string MotherboardCategoryIdValue = "catalog.category.motherboards";
        public const string MotherboardItemInstanceIdValue =
            "inventory.item.northstar-mb-matx-001";
        public const string WorkbenchContainerIdValue =
            "inventory.container.assembly-workbench";
        public const string PrototypeBuildIdValue = "assembly.build.prototype-001";
        public const string PrototypeChassisIdValue = "assembly.chassis.prototype-001";
        public const string MotherboardSlotIdValue = "assembly.slot.motherboard-main";
        public const string MotherboardFastenerIdValue =
            "assembly.fastener.motherboard-main-01";
        public const string MotherboardDisplayName = "Northstar M-ATX Anakart";
        public const long MotherboardUnitCostMinorUnits = 8_500;
        public const string ProcessorProductIdValue = "catalog.cpu.northstar-c01-lga1700";
        public const string ProcessorCategoryIdValue = "catalog.category.processors";
        public const string ProcessorItemInstanceIdValue =
            "inventory.item.northstar-c01-lga1700-001";
        public const string ProcessorSocketContainerIdValue =
            "inventory.container.assembly-processor-socket";
        public const string ProcessorSlotIdValue = "assembly.slot.processor-main";
        public const string ProcessorRetentionIdValue =
            "assembly.retention.processor-main-01";
        public const string ProcessorDisplayName = "Northstar C-01 İşlemci";
        public const long ProcessorUnitCostMinorUnits = 24_900;
        public const string MemoryProductIdValue =
            "catalog.memory.northstar-d5-16-udimm";
        public const string MemoryCategoryIdValue = "catalog.category.memory";
        public const string MemoryItemInstanceIdValue =
            "inventory.item.northstar-d5-16-udimm-001";
        public const string MemorySlotContainerIdValue =
            "inventory.container.assembly-memory-a2";
        public const string MemorySlotIdValue = "assembly.slot.memory-a2";
        public const string MemoryRetentionIdValue =
            "assembly.retention.memory-a2-dual-latch";
        public const string MemoryChannelIdValue = "assembly.memory-channel.a";
        public const string MemoryBankIdValue = "assembly.memory-bank.2";
        public const string MemoryDisplayName = "Northstar D5 16 GB Bellek";
        public const long MemoryUnitCostMinorUnits = 8_900;

        private GarageStockFlowSession(
            ProductCatalog catalog,
            PcComponentCatalog components,
            InventoryAuthority inventory,
            AssemblyBuildAuthority assemblyBuild,
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
            Components = components;
            Inventory = inventory;
            AssemblyBuild = assemblyBuild;
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

        public PcComponentCatalog Components { get; }

        public InventoryAuthority Inventory { get; }

        public AssemblyBuildAuthority AssemblyBuild { get; }

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

        public StableId<ProductDefinitionIdScope> MotherboardProductId =>
            StableId<ProductDefinitionIdScope>.Parse(MotherboardProductIdValue);

        public StableId<ItemInstanceIdScope> MotherboardItemId =>
            StableId<ItemInstanceIdScope>.Parse(MotherboardItemInstanceIdValue);

        public StableId<ProductDefinitionIdScope> ProcessorProductId =>
            StableId<ProductDefinitionIdScope>.Parse(ProcessorProductIdValue);

        public StableId<ItemInstanceIdScope> ProcessorItemId =>
            StableId<ItemInstanceIdScope>.Parse(ProcessorItemInstanceIdValue);

        public StableId<ProductDefinitionIdScope> MemoryProductId =>
            StableId<ProductDefinitionIdScope>.Parse(MemoryProductIdValue);

        public StableId<ItemInstanceIdScope> MemoryItemId =>
            StableId<ItemInstanceIdScope>.Parse(MemoryItemInstanceIdValue);

        public StableId<ContainerIdScope> WorkbenchContainerId =>
            StableId<ContainerIdScope>.Parse(WorkbenchContainerIdValue);

        public StableId<ContainerIdScope> ProcessorSocketContainerId =>
            StableId<ContainerIdScope>.Parse(ProcessorSocketContainerIdValue);

        public StableId<ContainerIdScope> MemorySlotContainerId =>
            StableId<ContainerIdScope>.Parse(MemorySlotContainerIdValue);

        public StableId<PcBuildIdScope> PrototypeBuildId =>
            StableId<PcBuildIdScope>.Parse(PrototypeBuildIdValue);

        public StableId<ChassisIdScope> PrototypeChassisId =>
            StableId<ChassisIdScope>.Parse(PrototypeChassisIdValue);

        public StableId<AssemblySlotIdScope> MotherboardSlotId =>
            StableId<AssemblySlotIdScope>.Parse(MotherboardSlotIdValue);

        public StableId<AssemblyFastenerIdScope> MotherboardFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(MotherboardFastenerIdValue);

        public StableId<AssemblySlotIdScope> ProcessorSlotId =>
            StableId<AssemblySlotIdScope>.Parse(ProcessorSlotIdValue);

        public StableId<AssemblyRetentionIdScope> ProcessorRetentionId =>
            StableId<AssemblyRetentionIdScope>.Parse(ProcessorRetentionIdValue);

        public StableId<AssemblySlotIdScope> MemorySlotId =>
            StableId<AssemblySlotIdScope>.Parse(MemorySlotIdValue);

        public StableId<AssemblyRetentionIdScope> MemoryRetentionId =>
            StableId<AssemblyRetentionIdScope>.Parse(MemoryRetentionIdValue);

        public StableId<AssemblyMemoryChannelIdScope> MemoryChannelId =>
            StableId<AssemblyMemoryChannelIdScope>.Parse(MemoryChannelIdValue);

        public StableId<AssemblyMemoryBankIdScope> MemoryBankId =>
            StableId<AssemblyMemoryBankIdScope>.Parse(MemoryBankIdValue);

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

        public static GarageStockFlowSession CreateArrived(
            bool includeAssemblyPrototype = false)
        {
            ProductDefinition product = ProductDefinition.Create(
                StableId<ProductDefinitionIdScope>.Parse(ProductIdValue),
                StableId<ProductCategoryIdScope>.Parse(ProductCategoryIdValue),
                ProductDisplayName,
                ProductTrackingPolicy.SerializedInstance,
                1095).Value;
            ProductDefinition motherboardProduct = ProductDefinition.Create(
                StableId<ProductDefinitionIdScope>.Parse(MotherboardProductIdValue),
                StableId<ProductCategoryIdScope>.Parse(MotherboardCategoryIdValue),
                MotherboardDisplayName,
                ProductTrackingPolicy.SerializedInstance,
                1095).Value;
            ProductDefinition processorProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(ProcessorProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(ProcessorCategoryIdValue),
                    ProcessorDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1095).Value
                : null;
            ProductDefinition memoryProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(MemoryProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(MemoryCategoryIdValue),
                    MemoryDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1095).Value
                : null;
            ProductCatalog catalog = ProductCatalog.Create(
                includeAssemblyPrototype
                    ? new[] { product, motherboardProduct, processorProduct, memoryProduct }
                    : new[] { product, motherboardProduct }).Value;
            PcComponentSpecification motherboardSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreateMotherboard(
                        catalog,
                        motherboardProduct.Id,
                        MotherboardFormFactor.MicroAtx,
                        CpuSocketFamily.Lga1700,
                        DimmType.Ddr5Udimm).Value
                    : PcComponentSpecification.Create(
                        catalog,
                        motherboardProduct.Id,
                        PcComponentKind.Motherboard,
                        MotherboardFormFactor.MicroAtx).Value;
            PcComponentSpecification processorSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreateProcessor(
                        catalog,
                        processorProduct.Id,
                        CpuSocketFamily.Lga1700).Value
                    : null;
            PcComponentSpecification memorySpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreateMemoryModule(
                        catalog,
                        memoryProduct.Id,
                        DimmType.Ddr5Udimm).Value
                    : null;
            PcComponentCatalog components = PcComponentCatalog.Create(
                catalog,
                includeAssemblyPrototype
                    ? new[]
                    {
                        motherboardSpecification,
                        processorSpecification,
                        memorySpecification
                    }
                    : new[] { motherboardSpecification }).Value;
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
            RegisterContainer(
                inventory,
                WorkbenchContainerIdValue,
                InventoryContainerKind.Workbench,
                1);
            if (includeAssemblyPrototype)
            {
                RegisterContainer(
                    inventory,
                    ProcessorSocketContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    MemorySlotContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
            }

            AssemblyBuildAuthority assemblyBuild = includeAssemblyPrototype
                ? AssemblyBuildAuthority.CreateWithProcessorSocketAndMemorySlot(
                    components,
                    inventory,
                    StableId<PcBuildIdScope>.Parse(PrototypeBuildIdValue),
                    StableId<ChassisIdScope>.Parse(PrototypeChassisIdValue),
                    StableId<AssemblySlotIdScope>.Parse(MotherboardSlotIdValue),
                    StableId<AssemblyFastenerIdScope>.Parse(MotherboardFastenerIdValue),
                    StableId<AssemblySlotIdScope>.Parse(ProcessorSlotIdValue),
                    StableId<AssemblyRetentionIdScope>.Parse(ProcessorRetentionIdValue),
                    DimmSlotDefinition.Create(
                        StableId<AssemblySlotIdScope>.Parse(MemorySlotIdValue),
                        StableId<AssemblyRetentionIdScope>.Parse(MemoryRetentionIdValue),
                        StableId<ContainerIdScope>.Parse(MemorySlotContainerIdValue),
                        StableId<AssemblyMemoryChannelIdScope>.Parse(MemoryChannelIdValue),
                        StableId<AssemblyMemoryBankIdScope>.Parse(MemoryBankIdValue),
                        1,
                        DimmType.Ddr5Udimm).Value,
                    StableId<ContainerIdScope>.Parse(HandsContainerIdValue),
                    StableId<ContainerIdScope>.Parse(WorkbenchContainerIdValue),
                    StableId<ContainerIdScope>.Parse(ProcessorSocketContainerIdValue),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700).Value
                : AssemblyBuildAuthority.Create(
                    components,
                    inventory,
                    StableId<PcBuildIdScope>.Parse(PrototypeBuildIdValue),
                    StableId<ChassisIdScope>.Parse(PrototypeChassisIdValue),
                    StableId<AssemblySlotIdScope>.Parse(MotherboardSlotIdValue),
                    StableId<AssemblyFastenerIdScope>.Parse(MotherboardFastenerIdValue),
                    StableId<ContainerIdScope>.Parse(HandsContainerIdValue),
                    StableId<ContainerIdScope>.Parse(WorkbenchContainerIdValue),
                    MotherboardFormFactor.MicroAtx).Value;

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

            if (includeAssemblyPrototype)
            {
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(MotherboardItemInstanceIdValue),
                    motherboardProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        MotherboardUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(ProcessorItemInstanceIdValue),
                    processorProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        ProcessorUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(MemoryItemInstanceIdValue),
                    memoryProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        MemoryUnitCostMinorUnits).Value));
            }

            var session = new GarageStockFlowSession(
                catalog,
                components,
                inventory,
                assemblyBuild,
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

        public bool TryGetMotherboardItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(MotherboardItemId, out item);
        }

        public bool TryGetProcessorItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(ProcessorItemId, out item);
        }

        public bool TryGetMemoryItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(MemoryItemId, out item);
        }

        public OperationResult PickupLooseMotherboardToHands()
        {
            if (AssemblyBuild.MotherboardSeatState != AssemblySeatState.Empty ||
                !TryGetMotherboardItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-seat.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(MotherboardItemId, HandsContainerId);
        }

        public OperationResult DropHeldMotherboardToWorld()
        {
            if (AssemblyBuild.MotherboardSeatState != AssemblySeatState.Empty ||
                !TryGetMotherboardItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-seat.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                MotherboardItemId,
                WorldFloorContainerId);
        }

        public OperationResult PickupLooseProcessorToHands()
        {
            if (!AssemblyBuild.HasProcessorSocket ||
                AssemblyBuild.ProcessorSocketState != ProcessorSocketState.EmptyOpen ||
                !TryGetProcessorItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-processor.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(ProcessorItemId, HandsContainerId);
        }

        public OperationResult DropHeldProcessorToWorld()
        {
            if (!AssemblyBuild.HasProcessorSocket ||
                AssemblyBuild.ProcessorSocketState != ProcessorSocketState.EmptyOpen ||
                !TryGetProcessorItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-processor.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                ProcessorItemId,
                WorldFloorContainerId);
        }

        public OperationResult PickupLooseMemoryToHands()
        {
            if (!AssemblyBuild.HasMemorySlot ||
                AssemblyBuild.MemorySlotState != MemorySlotState.EmptyOpen ||
                !TryGetMemoryItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-memory.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(MemoryItemId, HandsContainerId);
        }

        public OperationResult DropHeldMemoryToWorld()
        {
            if (!AssemblyBuild.HasMemorySlot ||
                AssemblyBuild.MemorySlotState != MemorySlotState.EmptyOpen ||
                !TryGetMemoryItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-memory.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                MemoryItemId,
                WorldFloorContainerId);
        }

        public OperationResult<AssemblyOperationReceipt> AttachMotherboard(
            StableId<AssemblyOperationIdScope> operationId)
        {
            return AssemblyBuild.AttachMotherboard(
                operationId,
                MotherboardItemId,
                MotherboardSlotId);
        }

        public OperationResult<AssemblyOperationReceipt> DetachMotherboard(
            StableId<AssemblyOperationIdScope> operationId)
        {
            return AssemblyBuild.DetachMotherboard(
                operationId,
                MotherboardItemId,
                MotherboardSlotId);
        }

        public OperationResult<AssemblyOperationReceipt> SecureMotherboardFastener(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SecureMotherboardFastener(
                operationId,
                MotherboardItemId,
                MotherboardSlotId,
                MotherboardFastenerId,
                sourceAttachOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> UnsecureMotherboardFastener(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.UnsecureMotherboardFastener(
                operationId,
                MotherboardItemId,
                MotherboardSlotId,
                MotherboardFastenerId,
                sourceAttachOperationId,
                sourceSecureOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> SeatProcessor(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SeatProcessor(
                operationId,
                ProcessorItemId,
                ProcessorSlotId,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> CloseProcessorRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.CloseProcessorRetention(
                operationId,
                ProcessorItemId,
                ProcessorSlotId,
                ProcessorRetentionId,
                sourceProcessorSeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> OpenProcessorRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.OpenProcessorRetention(
                operationId,
                ProcessorItemId,
                ProcessorSlotId,
                ProcessorRetentionId,
                sourceProcessorSeatOperationId,
                sourceProcessorRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveProcessor(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RemoveProcessor(
                operationId,
                ProcessorItemId,
                ProcessorSlotId,
                sourceProcessorSeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> SeatMemoryModule(
            StableId<AssemblyOperationIdScope> operationId,
            DimmKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SeatMemoryModule(
                operationId,
                MemoryItemId,
                MemorySlotId,
                orientation,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> CloseMemoryRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.CloseMemoryRetention(
                operationId,
                MemoryItemId,
                MemorySlotId,
                MemoryRetentionId,
                sourceMemorySeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> OpenMemoryRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            StableId<AssemblyOperationIdScope> sourceMemoryRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.OpenMemoryRetention(
                operationId,
                MemoryItemId,
                MemorySlotId,
                MemoryRetentionId,
                sourceMemorySeatOperationId,
                sourceMemoryRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveMemoryModule(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RemoveMemoryModule(
                operationId,
                MemoryItemId,
                MemorySlotId,
                sourceMemorySeatOperationId,
                expectedAssemblyRevision);
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

        internal OperationResult BeginPrototypeCheckout()
        {
            return RetailCheckouts.BeginCheckout(
                PrototypeCheckoutId,
                PrototypeBasketId,
                PrototypeCustomerId,
                Time(6));
        }

        internal OperationResult BeginPrototypeCustomerCheckout()
        {
            OperationResult provenance = ValidatePrototypeCustomerCheckoutProvenance();
            return provenance.IsFailure
                ? provenance
                : BeginPrototypeCheckout();
        }

        internal OperationResult ValidatePrototypeCustomerCheckoutProvenance()
        {
            if (!TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit) ||
                visit.Id != PrototypeCustomerVisitId ||
                visit.Intent == null ||
                visit.Intent.Id != PrototypeCustomerIntentId ||
                visit.Intent.CustomerId != PrototypeActorCustomerId ||
                visit.Intent.ProductId != ProductId ||
                visit.State != CustomerVisitState.AwaitingCheckout ||
                !TryGetPrototypeCustomerBuyAction(
                    out CustomerOfferDecisionActionRecord action) ||
                action.Id != PrototypeCustomerBuyActionId ||
                !action.IsBuy ||
                !action.HasReservation ||
                action.CustomerBinding == null ||
                !action.CustomerBinding.Equals(PrototypeCustomerBinding) ||
                action.CustomerBinding.Id != PrototypeCustomerBindingId ||
                action.CustomerBinding.ActorCustomerId != PrototypeActorCustomerId ||
                action.CustomerBinding.RetailCustomerId != PrototypeCustomerId ||
                action.SourceDecision == null ||
                action.SourceDecision.DecisionKind != CustomerOfferDecisionKind.Buy ||
                action.SourceDecision.ReasonCode !=
                    CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit ||
                action.SourceDecision.VisitId != visit.Id ||
                action.SourceDecision.CustomerId != visit.Intent.CustomerId ||
                action.SourceDecision.IntentId != visit.Intent.Id ||
                action.SourceDecision.IntentProductId != ProductId ||
                action.SourceDecision.OfferId != ShelfOfferId ||
                action.SourceDecision.ShelfContainerId != ShelfContainerId ||
                action.SourceDecision.OfferProductId != ProductId ||
                action.SourceDecision.Consultation == null ||
                action.SourceDecision.Consultation.Id !=
                    PrototypeCustomerConsultationId ||
                action.SourceDecision.Consultation.VisitId != visit.Id ||
                action.LineId != PrototypeBasketLineId ||
                action.BasketId != PrototypeBasketId ||
                action.ItemId != ItemId ||
                action.ReservationId != PrototypeReservationId ||
                action.ClaimId != PrototypeClaimId ||
                !TryGetPrototypeBasketLine(out RetailBasketLineRecord line) ||
                line.Id != PrototypeBasketLineId ||
                line.BasketId != PrototypeBasketId ||
                line.CustomerId != PrototypeCustomerId ||
                line.OfferId != ShelfOfferId ||
                line.ItemId != ItemId ||
                line.InventoryReservationId != PrototypeReservationId ||
                line.InventoryClaimId != PrototypeClaimId ||
                line.OwnerActionId != action.Id ||
                !TryGetShelfOffer(out ShelfOfferRecord offer) ||
                offer.Id != ShelfOfferId ||
                offer.ProductId != ProductId ||
                offer.ShelfContainerId != ShelfContainerId ||
                offer.OfferRevision != action.SourceDecision.OfferRevision ||
                offer.Price != action.SourceDecision.OfferPrice ||
                !TryGetItem(out InventoryItemRecord item) ||
                item.Id != ItemId ||
                item.ProductId != ProductId ||
                item.ContainerId != ShelfContainerId ||
                !Inventory.TryGetReservation(
                    PrototypeReservationId,
                    out InventoryReservation reservation) ||
                reservation.Id != PrototypeReservationId ||
                reservation.ClaimId != PrototypeClaimId ||
                reservation.TargetKind != InventoryReservationTargetKind.SerializedItem ||
                reservation.ItemId != ItemId ||
                !reservation.BatchId.IsEmpty ||
                !reservation.ContainerId.IsEmpty ||
                reservation.Quantity != 1 ||
                reservation.ReleasePolicy != InventoryReservationReleasePolicy.ConsumeOnly)
            {
                return OperationResult.Fail(
                    StockProjectionFailures.CheckoutProvenanceMismatch);
            }

            return OperationResult.Success();
        }

        public bool TryGetPrototypeCheckout(out RetailCheckoutRecord checkout)
        {
            return RetailCheckouts.TryGetCheckout(PrototypeCheckoutId, out checkout);
        }

        internal OperationResult CompletePrototypeCheckout()
        {
            return SettlePrototypeCashCheckout();
        }

        internal OperationResult SettlePrototypeCashCheckout()
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
            if (!CheckoutSettlements.TryGetSettlement(
                    PrototypeCheckoutSettlementId,
                    out CheckoutSettlementReceipt candidate) ||
                !IsCanonicalPrototypeSettlement(candidate))
            {
                receipt = null;
                return false;
            }

            receipt = candidate;
            return true;
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
            if (consultationResult.IsFailure)
            {
                return consultationResult;
            }

            OperationResult actionResult = CustomerOfferActions.ValidateInvariants();
            return actionResult.IsFailure
                ? actionResult
                : AssemblyBuild.ValidateInvariants();
        }

        private bool IsCanonicalPrototypeSettlement(CheckoutSettlementReceipt receipt)
        {
            if (receipt == null ||
                receipt.Id != PrototypeCheckoutSettlementId ||
                receipt.TransactionId != PrototypeLedgerTransactionId ||
                receipt.CompletionId != PrototypeCheckoutCompletionId ||
                receipt.CheckoutId != PrototypeCheckoutId ||
                receipt.CustomerId != PrototypeCustomerId ||
                receipt.PaymentMethod != CheckoutPaymentMethod.Cash ||
                receipt.PaidAt != Time(7) ||
                receipt.Currency.Value != PrototypeCurrencyCode ||
                receipt.GrossMinorUnits != PrototypePriceMinorUnits ||
                receipt.CostOfGoodsSoldMinorUnits != PrototypeUnitCostMinorUnits ||
                !TryGetPrototypeCustomerBuyAction(
                    out CustomerOfferDecisionActionRecord action) ||
                !TryGetPrototypeCheckout(out RetailCheckoutRecord checkout) ||
                checkout.Id != PrototypeCheckoutId ||
                checkout.BasketId != PrototypeBasketId ||
                checkout.CustomerId != PrototypeCustomerId ||
                checkout.StartedAt != Time(6) ||
                checkout.Currency.Value != PrototypeCurrencyCode ||
                checkout.TotalMinorUnits != PrototypePriceMinorUnits ||
                checkout.Lines == null ||
                checkout.Lines.Count != 1 ||
                !IsCanonicalPrototypeCheckoutLine(checkout.Lines[0], action) ||
                !TryGetPrototypeCheckoutCompletion(
                    out RetailCheckoutCompletionRecord completion) ||
                completion.Id != PrototypeCheckoutCompletionId ||
                completion.CheckoutId != PrototypeCheckoutId ||
                completion.BasketId != PrototypeBasketId ||
                completion.CustomerId != PrototypeCustomerId ||
                completion.Currency.Value != PrototypeCurrencyCode ||
                completion.TotalMinorUnits != PrototypePriceMinorUnits ||
                completion.Lines == null ||
                completion.Lines.Count != 1 ||
                !IsCanonicalPrototypeCheckoutLine(completion.Lines[0], action) ||
                completion.CompletedAt != receipt.PaidAt ||
                !CheckoutSettlements.TryGetSettlementForCheckout(
                    PrototypeCheckoutId,
                    out CheckoutSettlementReceipt checkoutReceipt) ||
                checkoutReceipt.Id != receipt.Id ||
                checkoutReceipt.TransactionId != receipt.TransactionId ||
                !TryGetPrototypeLedgerTransaction(
                    out EconomyLedgerTransactionRecord transaction) ||
                transaction.Id != PrototypeLedgerTransactionId ||
                transaction.SettlementId != PrototypeCheckoutSettlementId ||
                transaction.PostedAt != receipt.PaidAt ||
                transaction.Entries == null ||
                transaction.Entries.Count != 4)
            {
                return false;
            }

            return IsCanonicalLedgerEntry(
                       transaction.Entries[0],
                       EconomyAccountKind.Cash,
                       EconomyEntryDirection.Debit,
                       PrototypePriceMinorUnits) &&
                   IsCanonicalLedgerEntry(
                       transaction.Entries[1],
                       EconomyAccountKind.SalesRevenue,
                       EconomyEntryDirection.Credit,
                       PrototypePriceMinorUnits) &&
                   IsCanonicalLedgerEntry(
                       transaction.Entries[2],
                       EconomyAccountKind.CostOfGoodsSold,
                       EconomyEntryDirection.Debit,
                       PrototypeUnitCostMinorUnits) &&
                   IsCanonicalLedgerEntry(
                       transaction.Entries[3],
                       EconomyAccountKind.InventoryAsset,
                       EconomyEntryDirection.Credit,
                       PrototypeUnitCostMinorUnits);
        }

        private bool IsCanonicalPrototypeCheckoutLine(
            RetailCheckoutLineSnapshot line,
            CustomerOfferDecisionActionRecord action)
        {
            return line != null &&
                   action != null &&
                   line.BasketLineId == PrototypeBasketLineId &&
                   line.OfferId == ShelfOfferId &&
                   line.ItemId == ItemId &&
                   line.InventoryReservationId == PrototypeReservationId &&
                   line.InventoryClaimId == PrototypeClaimId &&
                   line.ProductId == ProductId &&
                   line.ShelfContainerId == ShelfContainerId &&
                   line.UnitCost.CurrencyCode == PrototypeCurrencyCode &&
                   line.UnitCost.MinorUnits == PrototypeUnitCostMinorUnits &&
                   line.UnitPrice == action.SourceDecision.OfferPrice &&
                   line.SourceOfferRevision == action.SourceDecision.OfferRevision;
        }

        private static bool IsCanonicalLedgerEntry(
            EconomyLedgerEntryRecord entry,
            EconomyAccountKind account,
            EconomyEntryDirection direction,
            long minorUnits)
        {
            return entry != null &&
                   entry.Account == account &&
                   entry.Direction == direction &&
                   entry.Currency.Value == PrototypeCurrencyCode &&
                   entry.MinorUnits == minorUnits;
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
