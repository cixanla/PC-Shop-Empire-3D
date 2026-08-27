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
    public sealed partial class GarageStockFlowSession
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
        public const string CustomPcBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit";
        public const string ProcessorBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.processor";
        public const string MemoryModuleBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.memory-module";
        public const string StorageBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.storage";
        public const string ProcessorCoolerBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.processor-cooler";
        public const string GraphicsCardBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.graphics-card";
        public const string PowerSupplyBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.power-supply";
        public const string Atx24PowerCableBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.atx24-power-cable";
        public const string Eps12vPowerCableBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.eps12v-power-cable";
        public const string PcieGpuPowerCableBuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit.pcie-gpu-power-cable";
        public const string PrototypeCustomPcBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-motherboard";
        public const string PrototypeProcessorBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-processor";
        public const string PrototypeMemoryModuleBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-memory-module";
        public const string PrototypeStorageBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-storage";
        public const string PrototypeProcessorCoolerBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-processor-cooler";
        public const string PrototypeGraphicsCardBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-graphics-card";
        public const string PrototypePowerSupplyBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-power-supply";
        public const string PrototypeAtx24PowerCableBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-atx24-power-cable";
        public const string PrototypeEps12vPowerCableBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-eps12v-power-cable";
        public const string PrototypePcieGpuPowerCableBuildKitOperationIdValue =
            "orders.custom-pc-build-kit-operation.prototype-pcie-gpu-power-cable";
        public const string PrototypeMotherboardAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-motherboard";
        public const string PrototypeProcessorAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-processor";
        public const string PrototypeMemoryModuleAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-memory-module";
        public const string PrototypeStorageAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-storage";
        public const string PrototypeProcessorCoolerAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-processor-cooler";
        public const string PrototypeGraphicsCardAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-graphics-card";
        public const string PrototypePowerSupplyAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-power-supply";
        public const string PrototypeAtx24PowerCableAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-atx24-power-cable";
        public const string PrototypeEps12vPowerCableAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-eps12v-power-cable";
        public const string PrototypePcieGpuPowerCableAssemblyHandoffOperationIdValue =
            "orders.custom-pc-build-kit-assembly-operation.prototype-pcie-gpu-power-cable";
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
        public const string StorageProductIdValue =
            "catalog.storage.northstar-nvme-pcie4-2280";
        public const string StorageCategoryIdValue = "catalog.category.storage";
        public const string StorageItemInstanceIdValue =
            "inventory.item.northstar-nvme-pcie4-2280-001";
        public const string StorageSlotContainerIdValue =
            "inventory.container.assembly-m2-primary";
        public const string StorageSlotIdValue = "assembly.slot.m2-primary";
        public const string StorageStandoffIdValue = "assembly.standoff.m2-2280";
        public const string StorageCaptiveScrewIdValue =
            "assembly.retention.m2-primary-captive-screw";
        public const string StorageDisplayName = "Northstar N4 1 TB NVMe SSD";
        public const long StorageUnitCostMinorUnits = 9_900;

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
            CustomPcQuoteAuthority customPcQuotes,
            CustomPcWorkOrderAuthority customPcWorkOrders,
            CustomPcBuildKitAuthority customPcBuildKit,
            CustomPcWorkOrderIssueAccess customPcWorkOrderIssueAccess,
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
            CustomPcQuotes = customPcQuotes;
            CustomPcWorkOrders = customPcWorkOrders;
            CustomPcBuildKit = customPcBuildKit;
            _customPcWorkOrderIssueAccess = customPcWorkOrderIssueAccess;
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

        public CustomPcQuoteAuthority CustomPcQuotes { get; }

        public CustomPcWorkOrderAuthority CustomPcWorkOrders { get; }

        public CustomPcBuildKitAuthority CustomPcBuildKit { get; }

        private readonly CustomPcWorkOrderIssueAccess _customPcWorkOrderIssueAccess;

        private CustomPcWorkTicketStationProjection _canonicalCustomPcWorkTicketStation;

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

        public StableId<ProductDefinitionIdScope> StorageProductId =>
            StableId<ProductDefinitionIdScope>.Parse(StorageProductIdValue);

        public StableId<ItemInstanceIdScope> StorageItemId =>
            StableId<ItemInstanceIdScope>.Parse(StorageItemInstanceIdValue);

        public StableId<ContainerIdScope> WorkbenchContainerId =>
            StableId<ContainerIdScope>.Parse(WorkbenchContainerIdValue);

        public StableId<ContainerIdScope> CustomPcBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(CustomPcBuildKitContainerIdValue);

        public StableId<ContainerIdScope> ProcessorBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(ProcessorBuildKitContainerIdValue);

        public StableId<ContainerIdScope> MemoryModuleBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(MemoryModuleBuildKitContainerIdValue);

        public StableId<ContainerIdScope> StorageBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(StorageBuildKitContainerIdValue);

        public StableId<ContainerIdScope> ProcessorCoolerBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(ProcessorCoolerBuildKitContainerIdValue);

        public StableId<ContainerIdScope> GraphicsCardBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(GraphicsCardBuildKitContainerIdValue);

        public StableId<ContainerIdScope> PowerSupplyBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(PowerSupplyBuildKitContainerIdValue);

        public StableId<ContainerIdScope> Atx24PowerCableBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(Atx24PowerCableBuildKitContainerIdValue);

        public StableId<ContainerIdScope> Eps12vPowerCableBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(Eps12vPowerCableBuildKitContainerIdValue);

        public StableId<ContainerIdScope> PcieGpuPowerCableBuildKitContainerId =>
            StableId<ContainerIdScope>.Parse(PcieGpuPowerCableBuildKitContainerIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeCustomPcBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeCustomPcBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeProcessorBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeProcessorBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeMemoryModuleBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeMemoryModuleBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeStorageBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeStorageBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeProcessorCoolerBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeProcessorCoolerBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeGraphicsCardBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeGraphicsCardBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypePowerSupplyBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypePowerSupplyBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeAtx24PowerCableBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeAtx24PowerCableBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypeEps12vPowerCableBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypeEps12vPowerCableBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitOperationIdScope>
            PrototypePcieGpuPowerCableBuildKitOperationId =>
                StableId<CustomPcBuildKitOperationIdScope>.Parse(
                    PrototypePcieGpuPowerCableBuildKitOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeMotherboardAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeMotherboardAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeProcessorAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeProcessorAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeMemoryModuleAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeMemoryModuleAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeStorageAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeStorageAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeProcessorCoolerAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeProcessorCoolerAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeGraphicsCardAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeGraphicsCardAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypePowerSupplyAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypePowerSupplyAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeAtx24PowerCableAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeAtx24PowerCableAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypeEps12vPowerCableAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypeEps12vPowerCableAssemblyHandoffOperationIdValue);

        public StableId<CustomPcBuildKitAssemblyOperationIdScope>
            PrototypePcieGpuPowerCableAssemblyHandoffOperationId =>
                StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                    PrototypePcieGpuPowerCableAssemblyHandoffOperationIdValue);

        public StableId<ContainerIdScope> ProcessorSocketContainerId =>
            StableId<ContainerIdScope>.Parse(ProcessorSocketContainerIdValue);

        public StableId<ContainerIdScope> MemorySlotContainerId =>
            StableId<ContainerIdScope>.Parse(MemorySlotContainerIdValue);

        public StableId<ContainerIdScope> StorageSlotContainerId =>
            StableId<ContainerIdScope>.Parse(StorageSlotContainerIdValue);

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

        public StableId<AssemblySlotIdScope> StorageSlotId =>
            StableId<AssemblySlotIdScope>.Parse(StorageSlotIdValue);

        public StableId<AssemblyStorageStandoffIdScope> StorageStandoffId =>
            StableId<AssemblyStorageStandoffIdScope>.Parse(StorageStandoffIdValue);

        public StableId<AssemblyRetentionIdScope> StorageCaptiveScrewId =>
            StableId<AssemblyRetentionIdScope>.Parse(StorageCaptiveScrewIdValue);

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
            ProductDefinition storageProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(StorageProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(StorageCategoryIdValue),
                    StorageDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1095).Value
                : null;
            ProductDefinition processorCoolerProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(
                        ProcessorCoolerProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(
                        ProcessorCoolerCategoryIdValue),
                    ProcessorCoolerDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1095).Value
                : null;
            ProductDefinition powerSupplyProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(
                        PowerSupplyProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(
                        PowerSupplyCategoryIdValue),
                    PowerSupplyDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1095).Value
                : null;
            ProductDefinition atx24PowerCableProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(
                        Atx24PowerCableProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(
                        Atx24PowerCableCategoryIdValue),
                    Atx24PowerCableDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1095).Value
                : null;
            ProductDefinition eps12vPowerCableProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(
                        Eps12vPowerCableProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(
                        Eps12vPowerCableCategoryIdValue),
                    Eps12vPowerCableDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1_500).Value
                : null;
            ProductDefinition pcieGpuPowerCableProduct = includeAssemblyPrototype
                ? ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(
                        PcieGpuPowerCableProductIdValue),
                    StableId<ProductCategoryIdScope>.Parse(
                        PcieGpuPowerCableCategoryIdValue),
                    PcieGpuPowerCableDisplayName,
                    ProductTrackingPolicy.SerializedInstance,
                    1_500).Value
                : null;
            ProductCatalog catalog = ProductCatalog.Create(
                includeAssemblyPrototype
                    ? new[]
                    {
                        product,
                        motherboardProduct,
                        processorProduct,
                        memoryProduct,
                        storageProduct,
                        processorCoolerProduct,
                        powerSupplyProduct,
                        atx24PowerCableProduct,
                        eps12vPowerCableProduct,
                        pcieGpuPowerCableProduct
                    }
                    : new[] { product, motherboardProduct }).Value;
            PcComponentSpecification motherboardSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreateMotherboard(
                        catalog,
                        motherboardProduct.Id,
                        MotherboardFormFactor.MicroAtx,
                        CpuSocketFamily.Lga1700,
                        DimmType.Ddr5Udimm,
                        M2StorageType.NvmePcie4X4_2280,
                        GraphicsCardType.Pcie4X16FullHeightDualSlot).Value
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
            PcComponentSpecification storageSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreateStorageDevice(
                        catalog,
                        storageProduct.Id,
                        M2StorageType.NvmePcie4X4_2280).Value
                    : null;
            PcComponentSpecification processorCoolerSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreateProcessorCooler(
                        catalog,
                        processorCoolerProduct.Id,
                        ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                        CpuSocketFamily.Lga1700).Value
                    : null;
            PcComponentSpecification graphicsCardSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreateGraphicsCard(
                        catalog,
                        product.Id,
                        GraphicsCardType.Pcie4X16FullHeightDualSlot).Value
                    : null;
            PcComponentSpecification powerSupplySpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreatePowerSupply(
                        catalog,
                        powerSupplyProduct.Id,
                        PowerSupplyType.AtxPs2).Value
                    : null;
            PcComponentSpecification atx24PowerCableSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreatePowerCable(
                        catalog,
                        atx24PowerCableProduct.Id,
                        PowerCableType.ModularAtx24SplitPsuToMotherboard).Value
                    : null;
            PcComponentSpecification eps12vPowerCableSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreatePowerCable(
                        catalog,
                        eps12vPowerCableProduct.Id,
                        PowerCableType.ModularEps12v8PinPsuToMotherboard).Value
                    : null;
            PcComponentSpecification pcieGpuPowerCableSpecification =
                includeAssemblyPrototype
                    ? PcComponentSpecification.CreatePowerCable(
                        catalog,
                        pcieGpuPowerCableProduct.Id,
                        PowerCableType.ModularPcie8PinPsuToGraphicsCard).Value
                    : null;
            PcComponentCatalog components = PcComponentCatalog.Create(
                catalog,
                includeAssemblyPrototype
                    ? new[]
                    {
                        motherboardSpecification,
                        processorSpecification,
                        memorySpecification,
                        storageSpecification,
                        processorCoolerSpecification,
                        graphicsCardSpecification,
                        powerSupplySpecification,
                        atx24PowerCableSpecification,
                        eps12vPowerCableSpecification,
                        pcieGpuPowerCableSpecification
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
                11);
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
                RegisterContainer(
                    inventory,
                    StorageSlotContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    ProcessorCoolerSlotContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    GraphicsCardSlotContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    PowerSupplyBayContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    Atx24PowerCableRouteContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    Eps12vPowerCableRouteContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    PcieGpuPowerCableRouteContainerIdValue,
                    InventoryContainerKind.Workbench,
                    1);
                RegisterContainer(
                    inventory,
                    CustomPcBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    ProcessorBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    MemoryModuleBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    StorageBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    ProcessorCoolerBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    GraphicsCardBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    PowerSupplyBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    Atx24PowerCableBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    Eps12vPowerCableBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
                RegisterContainer(
                    inventory,
                    PcieGpuPowerCableBuildKitContainerIdValue,
                    InventoryContainerKind.BuildKit,
                    1);
            }

            AssemblyBuildAuthority assemblyBuild = includeAssemblyPrototype
                ? AssemblyBuildAuthority
                    .CreateWithProcessorSocketMemoryStorageCoolerGraphicsCardAndPowerSupplySlots(
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
                    M2SlotDefinition.Create(
                        StableId<AssemblySlotIdScope>.Parse(StorageSlotIdValue),
                        StableId<AssemblyStorageStandoffIdScope>.Parse(
                            StorageStandoffIdValue),
                        StableId<AssemblyRetentionIdScope>.Parse(
                            StorageCaptiveScrewIdValue),
                        StableId<ContainerIdScope>.Parse(StorageSlotContainerIdValue),
                        M2StorageType.NvmePcie4X4_2280).Value,
                    ProcessorCoolerSlotDefinition.Create(
                        StableId<AssemblySlotIdScope>.Parse(
                            ProcessorCoolerSlotIdValue),
                        StableId<AssemblyProcessorCoolerBracketIdScope>.Parse(
                            ProcessorCoolerBracketIdValue),
                        StableId<ContainerIdScope>.Parse(
                            ProcessorCoolerSlotContainerIdValue),
                        ProcessorCoolerRetentionTopology.Create(
                            StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(
                                ProcessorCoolerRetentionPoint1IdValue),
                            StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(
                                ProcessorCoolerRetentionPoint2IdValue),
                            StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(
                                ProcessorCoolerRetentionPoint3IdValue),
                            StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(
                                ProcessorCoolerRetentionPoint4IdValue)).Value,
                        ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                        CpuSocketFamily.Lga1700).Value,
                    GraphicsCardSlotDefinition.Create(
                        StableId<AssemblySlotIdScope>.Parse(
                            GraphicsCardSlotIdValue),
                        StableId<ContainerIdScope>.Parse(
                            GraphicsCardSlotContainerIdValue),
                        GraphicsCardRetentionTopology.Create(
                            StableId<AssemblyGraphicsCardLatchIdScope>.Parse(
                                GraphicsCardLatchIdValue),
                            StableId<AssemblyFastenerIdScope>.Parse(
                                GraphicsCardBracketFastenerIdValue)).Value,
                        GraphicsCardType.Pcie4X16FullHeightDualSlot).Value,
                    PowerSupplyBayDefinition.Create(
                        StableId<AssemblySlotIdScope>.Parse(
                            PowerSupplyBaySlotIdValue),
                        StableId<ContainerIdScope>.Parse(
                            PowerSupplyBayContainerIdValue),
                        PowerSupplyRetentionTopology.Create(
                            StableId<AssemblyPowerSupplyRearMountIdScope>.Parse(
                                PowerSupplyRearMountIdValue),
                            StableId<AssemblyFastenerIdScope>.Parse(
                                PowerSupplyTopLeftFastenerIdValue),
                            StableId<AssemblyFastenerIdScope>.Parse(
                                PowerSupplyTopRightFastenerIdValue),
                            StableId<AssemblyFastenerIdScope>.Parse(
                                PowerSupplyBottomLeftFastenerIdValue),
                            StableId<AssemblyFastenerIdScope>.Parse(
                                PowerSupplyBottomRightFastenerIdValue)).Value,
                        PowerSupplyType.AtxPs2).Value,
                    StableId<ContainerIdScope>.Parse(HandsContainerIdValue),
                    StableId<ContainerIdScope>.Parse(WorkbenchContainerIdValue),
                    StableId<ContainerIdScope>.Parse(ProcessorSocketContainerIdValue),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    Atx24PowerCableDefinition.Create(
                        atx24PowerCableProduct.Id,
                        StableId<ContainerIdScope>.Parse(
                            Atx24PowerCableRouteContainerIdValue),
                        Atx24PowerCableTopology.Create(
                            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                                Atx24PowerCableRouteIdValue),
                            PowerCableEndpointDefinition.Create(
                                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                                    Atx24PowerCablePsuPrimaryEndpointIdValue),
                                PowerCableConnectorType.PsuModularAtx24Primary18).Value,
                            PowerCableEndpointDefinition.Create(
                                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                                    Atx24PowerCablePsuSenseEndpointIdValue),
                                PowerCableConnectorType.PsuModularAtx24Sense10).Value,
                            PowerCableEndpointDefinition.Create(
                                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                                    Atx24PowerCableMotherboardEndpointIdValue),
                                PowerCableConnectorType.MotherboardAtx24).Value,
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                Atx24PowerCableWaypoint1IdValue),
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                Atx24PowerCableWaypoint2IdValue),
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                Atx24PowerCableWaypoint3IdValue)).Value).Value,
                    Eps12vPowerCableDefinition.Create(
                        eps12vPowerCableProduct.Id,
                        StableId<ContainerIdScope>.Parse(
                            Eps12vPowerCableRouteContainerIdValue),
                        Eps12vPowerCableTopology.Create(
                            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                                Eps12vPowerCableRouteIdValue),
                            PowerCableEndpointDefinition.Create(
                                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                                    Eps12vPowerCablePsuEndpointIdValue),
                                PowerCableConnectorType.PsuModularEps12v8).Value,
                            PowerCableEndpointDefinition.Create(
                                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                                    Eps12vPowerCableMotherboardEndpointIdValue),
                                PowerCableConnectorType.MotherboardEps12v8).Value,
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                Eps12vPowerCableWaypoint1IdValue),
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                Eps12vPowerCableWaypoint2IdValue),
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                Eps12vPowerCableWaypoint3IdValue)).Value).Value,
                    PcieGpuPowerCableDefinition.Create(
                        pcieGpuPowerCableProduct.Id,
                        StableId<ContainerIdScope>.Parse(
                            PcieGpuPowerCableRouteContainerIdValue),
                        PcieGpuPowerCableTopology.Create(
                            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                                PcieGpuPowerCableRouteIdValue),
                            PowerCableEndpointDefinition.Create(
                                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                                    PcieGpuPowerCablePsuEndpointIdValue),
                                PowerCableConnectorType.PsuModularPcie8).Value,
                            PowerCableEndpointDefinition.Create(
                                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                                    PcieGpuPowerCableGraphicsCardEndpointIdValue),
                                PowerCableConnectorType.GraphicsCardPcie8).Value,
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                PcieGpuPowerCableWaypoint1IdValue),
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                PcieGpuPowerCableWaypoint2IdValue),
                            StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                                PcieGpuPowerCableWaypoint3IdValue)).Value).Value).Value
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
            CustomPcQuoteAuthority customPcQuotes = CustomPcQuoteAuthority.Create(
                catalog,
                components,
                inventory,
                customerConsultations).Value;
            CustomPcWorkOrderAuthorityCreation customPcWorkOrderCreation =
                CustomPcWorkOrderAuthority.Create(
                    customPcQuotes,
                    StableId<ContainerIdScope>.Parse(
                        WorkbenchContainerIdValue)).Value;
            CustomPcWorkOrderAuthority customPcWorkOrders =
                customPcWorkOrderCreation.Authority;
            CustomPcBuildKitAuthority customPcBuildKit = includeAssemblyPrototype
                ? CustomPcBuildKitAuthority.Create(
                    customPcWorkOrders,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                        StableId<ContainerIdScope>.Parse(HandsContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            CustomPcBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            ProcessorBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            MemoryModuleBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            StorageBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            ProcessorCoolerBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            GraphicsCardBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            PowerSupplyBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            Atx24PowerCableBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            Eps12vPowerCableBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            PcieGpuPowerCableBuildKitContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            ProcessorSocketContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            MemorySlotContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            StorageSlotContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            ProcessorCoolerSlotContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            GraphicsCardSlotContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            PowerSupplyBayContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            Atx24PowerCableRouteContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            Eps12vPowerCableRouteContainerIdValue),
                        StableId<ContainerIdScope>.Parse(
                            PcieGpuPowerCableRouteContainerIdValue)).Value
                : null;
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
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(StorageItemInstanceIdValue),
                    storageProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        StorageUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(
                        ProcessorCoolerItemInstanceIdValue),
                    processorCoolerProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        ProcessorCoolerUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(
                        GraphicsCardAssemblyItemInstanceIdValue),
                    product.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        GraphicsCardAssemblyUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(
                        PowerSupplyItemInstanceIdValue),
                    powerSupplyProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        PowerSupplyUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(
                        Atx24PowerCableItemInstanceIdValue),
                    atx24PowerCableProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        Atx24PowerCableUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(
                        Eps12vPowerCableItemInstanceIdValue),
                    eps12vPowerCableProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        Eps12vPowerCableUnitCostMinorUnits).Value));
                RequireSuccess(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse(
                        PcieGpuPowerCableItemInstanceIdValue),
                    pcieGpuPowerCableProduct.Id,
                    StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue),
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        PrototypeCurrencyCode,
                        PcieGpuPowerCableUnitCostMinorUnits).Value));
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
                customPcQuotes,
                customPcWorkOrders,
                customPcBuildKit,
                customPcWorkOrderCreation.IssueAccess,
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

        public bool TryGetStorageItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(StorageItemId, out item);
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

            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalMotherboard(
                        PrototypeCustomPcBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            return Inventory.TransferSerializedItem(MotherboardItemId, HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldMotherboardInCustomPcBuildKit()
        {
            return PlaceHeldMotherboardInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldMotherboardInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeCustomPcBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.MotherboardInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalMotherboard(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedMotherboardForAssembly()
        {
            return PickupStagedMotherboardForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedMotherboardForAssembly(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                AssemblyBuild.MotherboardSeatState != AssemblySeatState.Empty ||
                !TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            return CustomPcBuildKit.ReleaseCanonicalMotherboardForAssembly(
                PrototypeMotherboardAssemblyHandoffOperationId,
                workOrder,
                WorkbenchContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedProcessorForAssembly()
        {
            return PickupStagedProcessorForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedProcessorForAssembly(
                long expectedBuildKitRevision,
                long expectedInventoryRevision,
                long expectedAssemblyRevision)
        {
            AssemblyBuildSnapshot snapshot = AssemblyBuild.GetSnapshot();
            if (CustomPcBuildKit == null ||
                snapshot.Revision != expectedAssemblyRevision ||
                snapshot.MotherboardSeatState != AssemblySeatState.SeatedSecured ||
                snapshot.ProcessorSocketState != ProcessorSocketState.EmptyOpen ||
                snapshot.MotherboardItemId != MotherboardItemId ||
                snapshot.InstalledByOperationId.IsEmpty ||
                snapshot.SecuredByOperationId.IsEmpty ||
                !TryGetMotherboardItem(out InventoryItemRecord motherboard) ||
                motherboard.ContainerId != WorkbenchContainerId ||
                !TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !HasLiveSecuredMotherboardAssemblyPrerequisite(
                    snapshot,
                    workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            return CustomPcBuildKit.ReleaseCanonicalProcessorForAssembly(
                PrototypeProcessorAssemblyHandoffOperationId,
                workOrder,
                ProcessorSocketContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedMemoryModuleForAssembly()
        {
            return PickupStagedMemoryModuleForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedMemoryModuleForAssembly(
                long expectedBuildKitRevision,
                long expectedInventoryRevision,
                long expectedAssemblyRevision)
        {
            AssemblyBuildSnapshot snapshot = AssemblyBuild.GetSnapshot();
            if (CustomPcBuildKit == null ||
                snapshot.Revision != expectedAssemblyRevision ||
                snapshot.MotherboardSeatState != AssemblySeatState.SeatedSecured ||
                snapshot.ProcessorSocketState != ProcessorSocketState.ProcessorRetained ||
                snapshot.MemorySlotState != MemorySlotState.EmptyOpen ||
                snapshot.MotherboardItemId != MotherboardItemId ||
                snapshot.ProcessorItemId != ProcessorItemId ||
                snapshot.InstalledByOperationId.IsEmpty ||
                snapshot.SecuredByOperationId.IsEmpty ||
                snapshot.ProcessorSeatedByOperationId.IsEmpty ||
                snapshot.ProcessorRetainedByOperationId.IsEmpty ||
                !TryGetMotherboardItem(out InventoryItemRecord motherboard) ||
                motherboard.ContainerId != WorkbenchContainerId ||
                !TryGetProcessorItem(out InventoryItemRecord processor) ||
                processor.ContainerId != ProcessorSocketContainerId ||
                !TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !HasLiveSecuredMotherboardAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedProcessorAssemblyPrerequisite(snapshot, workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            return CustomPcBuildKit.ReleaseCanonicalMemoryModuleForAssembly(
                PrototypeMemoryModuleAssemblyHandoffOperationId,
                workOrder,
                MemorySlotContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedStorageForAssembly()
        {
            return PickupStagedStorageForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedStorageForAssembly(
                long expectedBuildKitRevision,
                long expectedInventoryRevision,
                long expectedAssemblyRevision)
        {
            AssemblyBuildSnapshot snapshot = AssemblyBuild.GetSnapshot();
            if (CustomPcBuildKit == null ||
                snapshot.Revision != expectedAssemblyRevision ||
                snapshot.MotherboardSeatState != AssemblySeatState.SeatedSecured ||
                snapshot.ProcessorSocketState != ProcessorSocketState.ProcessorRetained ||
                snapshot.MemorySlotState != MemorySlotState.MemoryModuleRetained ||
                snapshot.StorageSlotState != StorageSlotState.EmptyOpen ||
                snapshot.MotherboardItemId != MotherboardItemId ||
                snapshot.ProcessorItemId != ProcessorItemId ||
                snapshot.MemoryItemId != MemoryItemId ||
                snapshot.InstalledByOperationId.IsEmpty ||
                snapshot.SecuredByOperationId.IsEmpty ||
                snapshot.ProcessorSeatedByOperationId.IsEmpty ||
                snapshot.ProcessorRetainedByOperationId.IsEmpty ||
                snapshot.MemorySeatedByOperationId.IsEmpty ||
                snapshot.MemoryRetainedByOperationId.IsEmpty ||
                !TryGetMotherboardItem(out InventoryItemRecord motherboard) ||
                motherboard.ContainerId != WorkbenchContainerId ||
                !TryGetProcessorItem(out InventoryItemRecord processor) ||
                processor.ContainerId != ProcessorSocketContainerId ||
                !TryGetMemoryItem(out InventoryItemRecord memoryModule) ||
                memoryModule.ContainerId != MemorySlotContainerId ||
                !TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !HasLiveSecuredMotherboardAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedProcessorAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedMemoryModuleAssemblyPrerequisite(snapshot, workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            return CustomPcBuildKit.ReleaseCanonicalStorageForAssembly(
                PrototypeStorageAssemblyHandoffOperationId,
                workOrder,
                StorageSlotContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        private bool HasLiveSecuredMotherboardAssemblyPrerequisite(
            AssemblyBuildSnapshot snapshot,
            CustomPcBuildOrderRecord workOrder,
            bool requireCurrentRevision = true)
        {
            if (workOrder == null ||
                snapshot.BuildId != AssemblyBuild.BuildId ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.InstalledByOperationId,
                    out AssemblyOperationReceipt attach) ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.SecuredByOperationId,
                    out AssemblyOperationReceipt secure))
            {
                return false;
            }

            CustomPcBuildOrderLineSnapshot canonicalMotherboard = null;
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.Motherboard)
                {
                    if (canonicalMotherboard != null)
                    {
                        return false;
                    }

                    canonicalMotherboard = line;
                }
            }

            return canonicalMotherboard != null &&
                   canonicalMotherboard.ItemId == snapshot.MotherboardItemId &&
                   canonicalMotherboard.ProductId == snapshot.MotherboardProductId &&
                   attach.OperationId == snapshot.InstalledByOperationId &&
                   attach.OperationKind == AssemblyOperationKind.AttachMotherboard &&
                   attach.BuildId == snapshot.BuildId &&
                   attach.ChassisId == snapshot.ChassisId &&
                   attach.SlotId == snapshot.MotherboardSlotId &&
                   attach.ItemId == snapshot.MotherboardItemId &&
                   attach.ProductId == snapshot.MotherboardProductId &&
                   attach.SourceContainerId == HandsContainerId &&
                   attach.TargetContainerId == WorkbenchContainerId &&
                   attach.PreviousSeatState == AssemblySeatState.Empty &&
                   attach.ResultingSeatState == AssemblySeatState.SeatedUnsecured &&
                   secure.OperationId == snapshot.SecuredByOperationId &&
                   secure.OperationKind ==
                       AssemblyOperationKind.SecureMotherboardFastener &&
                   secure.BuildId == snapshot.BuildId &&
                   secure.ChassisId == snapshot.ChassisId &&
                   secure.SlotId == snapshot.MotherboardSlotId &&
                   secure.ItemId == snapshot.MotherboardItemId &&
                   secure.ProductId == snapshot.MotherboardProductId &&
                   secure.SourceAttachOperationId == attach.OperationId &&
                   secure.FastenerId == AssemblyBuild.MotherboardFastenerId &&
                   secure.ExpectedAssemblyRevision == attach.AssemblyRevision &&
                   secure.PreviousSeatState == AssemblySeatState.SeatedUnsecured &&
                   secure.ResultingSeatState == AssemblySeatState.SeatedSecured &&
                   (requireCurrentRevision
                       ? secure.AssemblyRevision == snapshot.Revision
                       : secure.AssemblyRevision > 0 &&
                         secure.AssemblyRevision < snapshot.Revision);
        }

        private bool HasLiveRetainedProcessorAssemblyPrerequisite(
            AssemblyBuildSnapshot snapshot,
            CustomPcBuildOrderRecord workOrder,
            bool requireCurrentRevision = true)
        {
            if (workOrder == null ||
                snapshot.BuildId != AssemblyBuild.BuildId ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.ProcessorSeatedByOperationId,
                    out AssemblyOperationReceipt seat) ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.ProcessorRetainedByOperationId,
                    out AssemblyOperationReceipt retain))
            {
                return false;
            }

            CustomPcBuildOrderLineSnapshot canonicalProcessor = null;
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.Processor)
                {
                    if (canonicalProcessor != null)
                    {
                        return false;
                    }

                    canonicalProcessor = line;
                }
            }

            return canonicalProcessor != null &&
                   canonicalProcessor.ItemId == snapshot.ProcessorItemId &&
                   canonicalProcessor.ProductId == snapshot.ProcessorProductId &&
                   seat.OperationId == snapshot.ProcessorSeatedByOperationId &&
                   seat.OperationKind == AssemblyOperationKind.SeatProcessor &&
                   seat.BuildId == snapshot.BuildId &&
                   seat.ChassisId == snapshot.ChassisId &&
                   seat.SlotId == snapshot.ProcessorSlotId &&
                   seat.ItemId == snapshot.ProcessorItemId &&
                   seat.ProductId == snapshot.ProcessorProductId &&
                   seat.SourceContainerId == HandsContainerId &&
                   seat.TargetContainerId == ProcessorSocketContainerId &&
                   seat.SourceAttachOperationId == snapshot.InstalledByOperationId &&
                   seat.SourceSecureOperationId == snapshot.SecuredByOperationId &&
                   seat.PreviousProcessorSocketState == ProcessorSocketState.EmptyOpen &&
                   seat.ResultingProcessorSocketState ==
                       ProcessorSocketState.ProcessorSeatedOpen &&
                   retain.OperationId == snapshot.ProcessorRetainedByOperationId &&
                   retain.OperationKind == AssemblyOperationKind.CloseProcessorRetention &&
                   retain.BuildId == snapshot.BuildId &&
                   retain.ChassisId == snapshot.ChassisId &&
                   retain.SlotId == snapshot.ProcessorSlotId &&
                   retain.ItemId == snapshot.ProcessorItemId &&
                   retain.ProductId == snapshot.ProcessorProductId &&
                   retain.RetentionId == AssemblyBuild.ProcessorRetentionId &&
                   retain.SourceProcessorSeatOperationId == seat.OperationId &&
                   retain.PreviousProcessorSocketState ==
                       ProcessorSocketState.ProcessorSeatedOpen &&
                   retain.ResultingProcessorSocketState ==
                       ProcessorSocketState.ProcessorRetained &&
                   (requireCurrentRevision
                       ? retain.AssemblyRevision == snapshot.Revision
                       : retain.AssemblyRevision > 0 &&
                         retain.AssemblyRevision < snapshot.Revision);
        }

        private bool HasLiveRetainedMemoryModuleAssemblyPrerequisite(
            AssemblyBuildSnapshot snapshot,
            CustomPcBuildOrderRecord workOrder,
            bool requireCurrentRevision = true)
        {
            if (workOrder == null ||
                snapshot.BuildId != AssemblyBuild.BuildId ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.MemorySeatedByOperationId,
                    out AssemblyOperationReceipt seat) ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.MemoryRetainedByOperationId,
                    out AssemblyOperationReceipt retain))
            {
                return false;
            }

            CustomPcBuildOrderLineSnapshot canonicalMemoryModule = null;
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.MemoryModule)
                {
                    if (canonicalMemoryModule != null)
                    {
                        return false;
                    }

                    canonicalMemoryModule = line;
                }
            }

            return canonicalMemoryModule != null &&
                   canonicalMemoryModule.ItemId == snapshot.MemoryItemId &&
                   canonicalMemoryModule.ProductId == snapshot.MemoryProductId &&
                   seat.OperationId == snapshot.MemorySeatedByOperationId &&
                   seat.OperationKind == AssemblyOperationKind.SeatMemoryModule &&
                   seat.BuildId == snapshot.BuildId &&
                   seat.ChassisId == snapshot.ChassisId &&
                   seat.SlotId == snapshot.MemorySlotId &&
                   seat.ItemId == snapshot.MemoryItemId &&
                   seat.ProductId == snapshot.MemoryProductId &&
                   seat.SourceContainerId == HandsContainerId &&
                   seat.TargetContainerId == MemorySlotContainerId &&
                   seat.SourceAttachOperationId == snapshot.InstalledByOperationId &&
                   seat.SourceSecureOperationId == snapshot.SecuredByOperationId &&
                   seat.PreviousMemorySlotState == MemorySlotState.EmptyOpen &&
                   seat.ResultingMemorySlotState ==
                       MemorySlotState.MemoryModuleSeatedOpen &&
                   retain.OperationId == snapshot.MemoryRetainedByOperationId &&
                   retain.OperationKind == AssemblyOperationKind.CloseMemoryRetention &&
                   retain.BuildId == snapshot.BuildId &&
                   retain.ChassisId == snapshot.ChassisId &&
                   retain.SlotId == snapshot.MemorySlotId &&
                   retain.ItemId == snapshot.MemoryItemId &&
                   retain.ProductId == snapshot.MemoryProductId &&
                   retain.RetentionId == AssemblyBuild.MemoryRetentionId &&
                   retain.SourceMemorySeatOperationId == seat.OperationId &&
                   retain.PreviousMemorySlotState ==
                       MemorySlotState.MemoryModuleSeatedOpen &&
                   retain.ResultingMemorySlotState ==
                       MemorySlotState.MemoryModuleRetained &&
                   (requireCurrentRevision
                       ? retain.AssemblyRevision == snapshot.Revision
                       : retain.AssemblyRevision > 0 &&
                         retain.AssemblyRevision < snapshot.Revision);
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

            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalProcessor(
                        PrototypeProcessorBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            return Inventory.TransferSerializedItem(ProcessorItemId, HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldProcessorInCustomPcBuildKit()
        {
            return PlaceHeldProcessorInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldProcessorInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeProcessorBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.ProcessorInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalProcessor(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
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

            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalMemoryModule(
                        PrototypeMemoryModuleBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            return Inventory.TransferSerializedItem(MemoryItemId, HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldMemoryModuleInCustomPcBuildKit()
        {
            return PlaceHeldMemoryModuleInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldMemoryModuleInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeMemoryModuleBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.MemoryModuleInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalMemoryModule(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
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

        public OperationResult PickupLooseStorageToHands()
        {
            if (!AssemblyBuild.HasStorageSlot ||
                AssemblyBuild.StorageSlotState != StorageSlotState.EmptyOpen ||
                !TryGetStorageItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-storage.loose-pickup-invalid"));
            }

            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalStorage(
                        PrototypeStorageBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            return Inventory.TransferSerializedItem(StorageItemId, HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldStorageInCustomPcBuildKit()
        {
            return PlaceHeldStorageInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldStorageInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeStorageBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.StorageInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalStorage(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult DropHeldStorageToWorld()
        {
            if (!AssemblyBuild.HasStorageSlot ||
                AssemblyBuild.StorageSlotState != StorageSlotState.EmptyOpen ||
                !TryGetStorageItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-storage.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                StorageItemId,
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

        public OperationResult<AssemblyOperationReceipt> SeatStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            M2KeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SeatStorageDevice(
                operationId,
                StorageItemId,
                StorageSlotId,
                orientation,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> SecureStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SecureStorageDevice(
                operationId,
                StorageItemId,
                StorageSlotId,
                StorageCaptiveScrewId,
                sourceStorageSeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> UnsecureStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceStorageRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.UnsecureStorageDevice(
                operationId,
                StorageItemId,
                StorageSlotId,
                StorageCaptiveScrewId,
                sourceStorageSeatOperationId,
                sourceStorageRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RemoveStorageDevice(
                operationId,
                StorageItemId,
                StorageSlotId,
                sourceStorageSeatOperationId,
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

            OperationResult customPcQuoteResult = CustomPcQuotes.ValidateInvariants();
            if (customPcQuoteResult.IsFailure)
            {
                return customPcQuoteResult;
            }

            OperationResult customPcWorkOrderResult =
                CustomPcWorkOrders.ValidateInvariants();
            if (customPcWorkOrderResult.IsFailure)
            {
                return customPcWorkOrderResult;
            }

            if (CustomPcBuildKit != null)
            {
                OperationResult customPcBuildKitResult =
                    CustomPcBuildKit.ValidateInvariants();
                if (customPcBuildKitResult.IsFailure)
                {
                    return customPcBuildKitResult;
                }
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
