using System;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private PowerSupplyBayDefinition _powerSupplyBayDefinition;
        private InventorySerializedTransferAccess _powerSupplyInventoryTransferAccess;
        private PowerSupplyBayState _powerSupplyBayState = PowerSupplyBayState.Unsupported;
        private StableId<ItemInstanceIdScope> _powerSupplyItemId;
        private StableId<ProductDefinitionIdScope> _powerSupplyProductId;
        private StableId<AssemblyOperationIdScope> _powerSupplySeatedByOperationId;
        private StableId<AssemblyOperationIdScope> _powerSupplyRetainedByOperationId;
        private PowerSupplyMountOrientation _powerSupplyMountOrientation;

        public bool HasPowerSupplyBay => _powerSupplyBayDefinition.IsValid;

        public PowerSupplyBayDefinition PowerSupplyBayDefinition =>
            _powerSupplyBayDefinition;

        public StableId<AssemblySlotIdScope> PowerSupplyBaySlotId =>
            _powerSupplyBayDefinition.SlotId;

        public StableId<ContainerIdScope> PowerSupplyBayContainerId =>
            _powerSupplyBayDefinition.ContainerId;

        public PowerSupplyRetentionTopology PowerSupplyRetentionTopology =>
            _powerSupplyBayDefinition.RetentionTopology;

        public PowerSupplyType SupportedPowerSupplyType =>
            _powerSupplyBayDefinition.SupportedPowerSupplyType;

        public PowerSupplyBayState PowerSupplyBayState => _powerSupplyBayState;

        public StableId<ItemInstanceIdScope> PowerSupplyItemId => _powerSupplyItemId;

        public StableId<ProductDefinitionIdScope> PowerSupplyProductId =>
            _powerSupplyProductId;

        public StableId<AssemblyOperationIdScope> PowerSupplySeatedByOperationId =>
            _powerSupplySeatedByOperationId;

        public StableId<AssemblyOperationIdScope> PowerSupplyRetainedByOperationId =>
            _powerSupplyRetainedByOperationId;

        public PowerSupplyMountOrientation PowerSupplyMountOrientation =>
            _powerSupplyMountOrientation;

        /// <summary>
        /// Creates the canonical aggregate with one capacity-one chassis-owned PSU bay.
        /// All seven managed assembly containers are claimed in one Inventory revision.
        /// </summary>
        public static OperationResult<AssemblyBuildAuthority>
            CreateWithProcessorSocketMemoryStorageCoolerGraphicsCardAndPowerSupplySlots(
                PcComponentCatalog componentCatalog,
                InventoryAuthority inventory,
                StableId<PcBuildIdScope> buildId,
                StableId<ChassisIdScope> chassisId,
                StableId<AssemblySlotIdScope> motherboardSlotId,
                StableId<AssemblyFastenerIdScope> motherboardFastenerId,
                StableId<AssemblySlotIdScope> processorSlotId,
                StableId<AssemblyRetentionIdScope> processorRetentionId,
                DimmSlotDefinition memorySlotDefinition,
                M2SlotDefinition storageSlotDefinition,
                ProcessorCoolerSlotDefinition processorCoolerSlotDefinition,
                GraphicsCardSlotDefinition graphicsCardSlotDefinition,
                PowerSupplyBayDefinition powerSupplyBayDefinition,
                StableId<ContainerIdScope> handsContainerId,
                StableId<ContainerIdScope> workbenchContainerId,
                StableId<ContainerIdScope> processorSocketContainerId,
                MotherboardFormFactor supportedMotherboardFormFactor,
                CpuSocketFamily supportedCpuSocketFamily,
                Atx24PowerCableDefinition atx24PowerCableDefinition = default,
                Eps12vPowerCableDefinition eps12vPowerCableDefinition = default,
                PcieGpuPowerCableDefinition pcieGpuPowerCableDefinition = default)
        {
            if (componentCatalog == null)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.MissingComponentCatalog);
            }

            if (inventory == null)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.MissingInventoryAuthority);
            }

            if (!inventory.UsesCatalog(componentCatalog.OwnerCatalog))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.CatalogAuthorityMismatch);
            }

            if (buildId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidBuildId);
            }

            if (chassisId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidChassisId);
            }

            if (!memorySlotDefinition.IsValid)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidMemorySlotDefinition);
            }

            if (!storageSlotDefinition.IsValid)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidStorageSlotDefinition);
            }

            if (!processorCoolerSlotDefinition.IsValid)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerSlotDefinition);
            }

            if (!graphicsCardSlotDefinition.IsValid)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidGraphicsCardSlotDefinition);
            }

            if (!powerSupplyBayDefinition.IsValid)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerSupplyBayDefinition);
            }

            bool hasAtx24PowerCable = atx24PowerCableDefinition.IsValid;
            if (!hasAtx24PowerCable && atx24PowerCableDefinition.HasAnyValue)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            if (hasAtx24PowerCable &&
                (!componentCatalog.OwnerCatalog.TryGet(
                    atx24PowerCableDefinition.ProductId,
                    out ProductDefinition cableProduct) ||
                 cableProduct.TrackingPolicy != ProductTrackingPolicy.SerializedInstance))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableProduct);
            }

            if (hasAtx24PowerCable &&
                (!componentCatalog.TryGet(
                    atx24PowerCableDefinition.ProductId,
                    out PcComponentSpecification atxCableSpecification) ||
                 atxCableSpecification.Kind != PcComponentKind.PowerCable ||
                 atxCableSpecification.PowerCableType !=
                     PowerCableType.ModularAtx24SplitPsuToMotherboard))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            bool hasEps12vPowerCable = eps12vPowerCableDefinition.IsValid;
            if (!hasEps12vPowerCable && eps12vPowerCableDefinition.HasAnyValue)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            if (hasEps12vPowerCable &&
                (!componentCatalog.OwnerCatalog.TryGet(
                    eps12vPowerCableDefinition.ProductId,
                    out ProductDefinition epsCableProduct) ||
                 epsCableProduct.TrackingPolicy !=
                    ProductTrackingPolicy.SerializedInstance))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableProduct);
            }

            if (hasEps12vPowerCable &&
                (!componentCatalog.TryGet(
                    eps12vPowerCableDefinition.ProductId,
                    out PcComponentSpecification epsCableSpecification) ||
                 epsCableSpecification.Kind != PcComponentKind.PowerCable ||
                 epsCableSpecification.PowerCableType !=
                     PowerCableType.ModularEps12v8PinPsuToMotherboard))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            bool hasPcieGpuPowerCable = pcieGpuPowerCableDefinition.IsValid;
            if (!hasPcieGpuPowerCable && pcieGpuPowerCableDefinition.HasAnyValue)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            if (hasPcieGpuPowerCable &&
                (!componentCatalog.OwnerCatalog.TryGet(
                    pcieGpuPowerCableDefinition.ProductId,
                    out ProductDefinition pcieCableProduct) ||
                 pcieCableProduct.TrackingPolicy !=
                    ProductTrackingPolicy.SerializedInstance))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableProduct);
            }

            if (hasPcieGpuPowerCable &&
                (!componentCatalog.TryGet(
                    pcieGpuPowerCableDefinition.ProductId,
                    out PcComponentSpecification pcieCableSpecification) ||
                 pcieCableSpecification.Kind != PcComponentKind.PowerCable ||
                 pcieCableSpecification.PowerCableType !=
                     PowerCableType.ModularPcie8PinPsuToGraphicsCard))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            if (hasAtx24PowerCable &&
                hasEps12vPowerCable &&
                HasPowerCableDefinitionIdentityConflict(
                    atx24PowerCableDefinition,
                    eps12vPowerCableDefinition))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            if (hasAtx24PowerCable &&
                hasPcieGpuPowerCable &&
                HasPowerCableDefinitionIdentityConflict(
                    atx24PowerCableDefinition,
                    pcieGpuPowerCableDefinition))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            if (hasEps12vPowerCable &&
                hasPcieGpuPowerCable &&
                HasPowerCableDefinitionIdentityConflict(
                    eps12vPowerCableDefinition,
                    pcieGpuPowerCableDefinition))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableDefinition);
            }

            if (HasDuplicatePowerSupplyFactorySlot(
                    motherboardSlotId,
                    processorSlotId,
                    memorySlotDefinition.SlotId,
                    storageSlotDefinition.SlotId,
                    processorCoolerSlotDefinition.SlotId,
                    graphicsCardSlotDefinition.SlotId,
                    powerSupplyBayDefinition.SlotId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (motherboardFastenerId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidFastener);
            }

            if (graphicsCardSlotDefinition.RetentionTopology.BracketFastenerId ==
                    motherboardFastenerId)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidGraphicsCardBracketFastener);
            }

            if (HasPowerSupplyFastenerConflict(
                    powerSupplyBayDefinition.RetentionTopology,
                    motherboardFastenerId,
                    graphicsCardSlotDefinition.RetentionTopology.BracketFastenerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerSupplyFastenerTopology);
            }

            if (processorRetentionId.IsEmpty ||
                processorRetentionId == memorySlotDefinition.RetentionId ||
                processorRetentionId == storageSlotDefinition.CaptiveScrewId ||
                memorySlotDefinition.RetentionId == storageSlotDefinition.CaptiveScrewId)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidRetention);
            }

            if (handsContainerId.IsEmpty ||
                !inventory.TryGetContainer(
                    handsContainerId,
                    out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidHandsContainer);
            }

            if (workbenchContainerId.IsEmpty ||
                !inventory.TryGetContainer(
                    workbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidWorkbenchContainer);
            }

            if (!IsCapacityOneWorkbenchContainer(inventory, processorSocketContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidProcessorSocketContainer);
            }

            if (!IsCapacityOneWorkbenchContainer(
                    inventory,
                    memorySlotDefinition.ContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidMemorySlotContainer);
            }

            if (!IsCapacityOneWorkbenchContainer(
                    inventory,
                    storageSlotDefinition.ContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidStorageSlotContainer);
            }

            if (!IsCapacityOneWorkbenchContainer(
                    inventory,
                    processorCoolerSlotDefinition.ContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerSlotContainer);
            }

            if (!IsCapacityOneWorkbenchContainer(
                    inventory,
                    graphicsCardSlotDefinition.ContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidGraphicsCardSlotContainer);
            }

            if (!IsCapacityOneWorkbenchContainer(
                    inventory,
                    powerSupplyBayDefinition.ContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerSupplyBayContainer);
            }

            if (hasAtx24PowerCable &&
                !IsCapacityOneWorkbenchContainer(
                    inventory,
                    atx24PowerCableDefinition.RouteContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableRouteContainer);
            }

            if (hasEps12vPowerCable &&
                !IsCapacityOneWorkbenchContainer(
                    inventory,
                    eps12vPowerCableDefinition.RouteContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableRouteContainer);
            }

            if (hasPcieGpuPowerCable &&
                !IsCapacityOneWorkbenchContainer(
                    inventory,
                    pcieGpuPowerCableDefinition.RouteContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidPowerCableRouteContainer);
            }

            if (HasDuplicatePowerSupplyFactoryContainer(
                    handsContainerId,
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId,
                    graphicsCardSlotDefinition.ContainerId,
                    powerSupplyBayDefinition.ContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.SameInventoryContainer);
            }

            if (hasAtx24PowerCable &&
                IsDuplicatePowerCableFactoryContainer(
                    atx24PowerCableDefinition.RouteContainerId,
                    handsContainerId,
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId,
                    graphicsCardSlotDefinition.ContainerId,
                    powerSupplyBayDefinition.ContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.SameInventoryContainer);
            }

            if (hasEps12vPowerCable &&
                IsDuplicatePowerCableFactoryContainer(
                    eps12vPowerCableDefinition.RouteContainerId,
                    handsContainerId,
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId,
                    graphicsCardSlotDefinition.ContainerId,
                    powerSupplyBayDefinition.ContainerId,
                    atx24PowerCableDefinition.RouteContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.SameInventoryContainer);
            }

            if (hasPcieGpuPowerCable &&
                IsDuplicatePowerCableFactoryContainer(
                    pcieGpuPowerCableDefinition.RouteContainerId,
                    handsContainerId,
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId,
                    graphicsCardSlotDefinition.ContainerId,
                    powerSupplyBayDefinition.ContainerId,
                    atx24PowerCableDefinition.RouteContainerId,
                    eps12vPowerCableDefinition.RouteContainerId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.SameInventoryContainer);
            }

            if (!PcComponentSpecification.IsValidMotherboardFormFactor(
                    supportedMotherboardFormFactor))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidMotherboardFormFactor);
            }

            if (!PcComponentSpecification.IsValidCpuSocketFamily(
                    supportedCpuSocketFamily))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidCpuSocketFamily);
            }

            if (processorCoolerSlotDefinition.SupportedSocketFamily !=
                    supportedCpuSocketFamily)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.ProcessorCoolerSocketMismatch);
            }

            if (inventory.GetContainerQuantity(workbenchContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.SlotOccupied);
            }

            if (inventory.GetContainerQuantity(processorSocketContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.ProcessorSocketOccupied);
            }

            if (inventory.GetContainerQuantity(memorySlotDefinition.ContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.MemorySlotOccupied);
            }

            if (inventory.GetContainerQuantity(storageSlotDefinition.ContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.StorageSlotOccupied);
            }

            if (inventory.GetContainerQuantity(
                    processorCoolerSlotDefinition.ContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.ProcessorCoolerSlotOccupied);
            }

            if (inventory.GetContainerQuantity(
                    graphicsCardSlotDefinition.ContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.GraphicsCardSlotOccupied);
            }

            if (inventory.GetContainerQuantity(
                    powerSupplyBayDefinition.ContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.PowerSupplyBayOccupied);
            }

            if (hasAtx24PowerCable &&
                inventory.GetContainerQuantity(
                    atx24PowerCableDefinition.RouteContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.PowerCableAlreadyRouted);
            }

            if (hasEps12vPowerCable &&
                inventory.GetContainerQuantity(
                    eps12vPowerCableDefinition.RouteContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.PowerCableAlreadyRouted);
            }

            if (hasPcieGpuPowerCable &&
                inventory.GetContainerQuantity(
                    pcieGpuPowerCableDefinition.RouteContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.PowerCableAlreadyRouted);
            }

            if (hasAtx24PowerCable &&
                hasEps12vPowerCable &&
                hasPcieGpuPowerCable)
            {
                OperationResult<InventorySerializedTransferAccessDecuple> cableAccess =
                    inventory.ClaimManagedSerializedTransferContainers(
                        workbenchContainerId,
                        processorSocketContainerId,
                        memorySlotDefinition.ContainerId,
                        storageSlotDefinition.ContainerId,
                        processorCoolerSlotDefinition.ContainerId,
                        graphicsCardSlotDefinition.ContainerId,
                        powerSupplyBayDefinition.ContainerId,
                        atx24PowerCableDefinition.RouteContainerId,
                        eps12vPowerCableDefinition.RouteContainerId,
                        pcieGpuPowerCableDefinition.RouteContainerId);
                if (cableAccess.IsFailure)
                {
                    return OperationResult<AssemblyBuildAuthority>.Fail(
                        MapManagedFactoryClaimFailure(cableAccess.Error));
                }

                return OperationResult<AssemblyBuildAuthority>.Success(
                    new AssemblyBuildAuthority(
                        componentCatalog,
                        inventory,
                        buildId,
                        chassisId,
                        motherboardSlotId,
                        motherboardFastenerId,
                        handsContainerId,
                        workbenchContainerId,
                        supportedMotherboardFormFactor,
                        cableAccess.Value.First,
                        processorSlotId,
                        processorRetentionId,
                        processorSocketContainerId,
                        supportedCpuSocketFamily,
                        cableAccess.Value.Second,
                        memorySlotDefinition,
                        cableAccess.Value.Third,
                        storageSlotDefinition,
                        cableAccess.Value.Fourth,
                        processorCoolerSlotDefinition,
                        cableAccess.Value.Fifth,
                        graphicsCardSlotDefinition,
                        cableAccess.Value.Sixth,
                        powerSupplyBayDefinition,
                        cableAccess.Value.Seventh,
                        atx24PowerCableDefinition,
                        cableAccess.Value.Eighth,
                        eps12vPowerCableDefinition,
                        cableAccess.Value.Ninth,
                        pcieGpuPowerCableDefinition,
                        cableAccess.Value.Tenth));
            }

            if (hasAtx24PowerCable && hasEps12vPowerCable)
            {
                OperationResult<InventorySerializedTransferAccessNonuple> cableAccess =
                    inventory.ClaimManagedSerializedTransferContainers(
                        workbenchContainerId,
                        processorSocketContainerId,
                        memorySlotDefinition.ContainerId,
                        storageSlotDefinition.ContainerId,
                        processorCoolerSlotDefinition.ContainerId,
                        graphicsCardSlotDefinition.ContainerId,
                        powerSupplyBayDefinition.ContainerId,
                        atx24PowerCableDefinition.RouteContainerId,
                        eps12vPowerCableDefinition.RouteContainerId);
                if (cableAccess.IsFailure)
                {
                    return OperationResult<AssemblyBuildAuthority>.Fail(
                        MapManagedFactoryClaimFailure(cableAccess.Error));
                }

                return OperationResult<AssemblyBuildAuthority>.Success(
                    new AssemblyBuildAuthority(
                        componentCatalog,
                        inventory,
                        buildId,
                        chassisId,
                        motherboardSlotId,
                        motherboardFastenerId,
                        handsContainerId,
                        workbenchContainerId,
                        supportedMotherboardFormFactor,
                        cableAccess.Value.First,
                        processorSlotId,
                        processorRetentionId,
                        processorSocketContainerId,
                        supportedCpuSocketFamily,
                        cableAccess.Value.Second,
                        memorySlotDefinition,
                        cableAccess.Value.Third,
                        storageSlotDefinition,
                        cableAccess.Value.Fourth,
                        processorCoolerSlotDefinition,
                        cableAccess.Value.Fifth,
                        graphicsCardSlotDefinition,
                        cableAccess.Value.Sixth,
                        powerSupplyBayDefinition,
                        cableAccess.Value.Seventh,
                        atx24PowerCableDefinition,
                        cableAccess.Value.Eighth,
                        eps12vPowerCableDefinition,
                        cableAccess.Value.Ninth));
            }

            if (hasAtx24PowerCable && hasPcieGpuPowerCable)
            {
                OperationResult<InventorySerializedTransferAccessNonuple> cableAccess =
                    inventory.ClaimManagedSerializedTransferContainers(
                        workbenchContainerId,
                        processorSocketContainerId,
                        memorySlotDefinition.ContainerId,
                        storageSlotDefinition.ContainerId,
                        processorCoolerSlotDefinition.ContainerId,
                        graphicsCardSlotDefinition.ContainerId,
                        powerSupplyBayDefinition.ContainerId,
                        atx24PowerCableDefinition.RouteContainerId,
                        pcieGpuPowerCableDefinition.RouteContainerId);
                if (cableAccess.IsFailure)
                {
                    return OperationResult<AssemblyBuildAuthority>.Fail(
                        MapManagedFactoryClaimFailure(cableAccess.Error));
                }

                return OperationResult<AssemblyBuildAuthority>.Success(
                    new AssemblyBuildAuthority(
                        componentCatalog,
                        inventory,
                        buildId,
                        chassisId,
                        motherboardSlotId,
                        motherboardFastenerId,
                        handsContainerId,
                        workbenchContainerId,
                        supportedMotherboardFormFactor,
                        cableAccess.Value.First,
                        processorSlotId,
                        processorRetentionId,
                        processorSocketContainerId,
                        supportedCpuSocketFamily,
                        cableAccess.Value.Second,
                        memorySlotDefinition,
                        cableAccess.Value.Third,
                        storageSlotDefinition,
                        cableAccess.Value.Fourth,
                        processorCoolerSlotDefinition,
                        cableAccess.Value.Fifth,
                        graphicsCardSlotDefinition,
                        cableAccess.Value.Sixth,
                        powerSupplyBayDefinition,
                        cableAccess.Value.Seventh,
                        atx24PowerCableDefinition,
                        cableAccess.Value.Eighth,
                        default,
                        null,
                        pcieGpuPowerCableDefinition,
                        cableAccess.Value.Ninth));
            }

            if (hasEps12vPowerCable && hasPcieGpuPowerCable)
            {
                OperationResult<InventorySerializedTransferAccessNonuple> cableAccess =
                    inventory.ClaimManagedSerializedTransferContainers(
                        workbenchContainerId,
                        processorSocketContainerId,
                        memorySlotDefinition.ContainerId,
                        storageSlotDefinition.ContainerId,
                        processorCoolerSlotDefinition.ContainerId,
                        graphicsCardSlotDefinition.ContainerId,
                        powerSupplyBayDefinition.ContainerId,
                        eps12vPowerCableDefinition.RouteContainerId,
                        pcieGpuPowerCableDefinition.RouteContainerId);
                if (cableAccess.IsFailure)
                {
                    return OperationResult<AssemblyBuildAuthority>.Fail(
                        MapManagedFactoryClaimFailure(cableAccess.Error));
                }

                return OperationResult<AssemblyBuildAuthority>.Success(
                    new AssemblyBuildAuthority(
                        componentCatalog,
                        inventory,
                        buildId,
                        chassisId,
                        motherboardSlotId,
                        motherboardFastenerId,
                        handsContainerId,
                        workbenchContainerId,
                        supportedMotherboardFormFactor,
                        cableAccess.Value.First,
                        processorSlotId,
                        processorRetentionId,
                        processorSocketContainerId,
                        supportedCpuSocketFamily,
                        cableAccess.Value.Second,
                        memorySlotDefinition,
                        cableAccess.Value.Third,
                        storageSlotDefinition,
                        cableAccess.Value.Fourth,
                        processorCoolerSlotDefinition,
                        cableAccess.Value.Fifth,
                        graphicsCardSlotDefinition,
                        cableAccess.Value.Sixth,
                        powerSupplyBayDefinition,
                        cableAccess.Value.Seventh,
                        default,
                        null,
                        eps12vPowerCableDefinition,
                        cableAccess.Value.Eighth,
                        pcieGpuPowerCableDefinition,
                        cableAccess.Value.Ninth));
            }

            if (hasAtx24PowerCable)
            {
                OperationResult<InventorySerializedTransferAccessOctuple> cableAccess =
                    inventory.ClaimManagedSerializedTransferContainers(
                        workbenchContainerId,
                        processorSocketContainerId,
                        memorySlotDefinition.ContainerId,
                        storageSlotDefinition.ContainerId,
                        processorCoolerSlotDefinition.ContainerId,
                        graphicsCardSlotDefinition.ContainerId,
                        powerSupplyBayDefinition.ContainerId,
                        atx24PowerCableDefinition.RouteContainerId);
                if (cableAccess.IsFailure)
                {
                    return OperationResult<AssemblyBuildAuthority>.Fail(
                        MapManagedFactoryClaimFailure(cableAccess.Error));
                }

                return OperationResult<AssemblyBuildAuthority>.Success(
                    new AssemblyBuildAuthority(
                        componentCatalog,
                        inventory,
                        buildId,
                        chassisId,
                        motherboardSlotId,
                        motherboardFastenerId,
                        handsContainerId,
                        workbenchContainerId,
                        supportedMotherboardFormFactor,
                        cableAccess.Value.First,
                        processorSlotId,
                        processorRetentionId,
                        processorSocketContainerId,
                        supportedCpuSocketFamily,
                        cableAccess.Value.Second,
                        memorySlotDefinition,
                        cableAccess.Value.Third,
                        storageSlotDefinition,
                        cableAccess.Value.Fourth,
                        processorCoolerSlotDefinition,
                        cableAccess.Value.Fifth,
                        graphicsCardSlotDefinition,
                        cableAccess.Value.Sixth,
                        powerSupplyBayDefinition,
                        cableAccess.Value.Seventh,
                        atx24PowerCableDefinition,
                        cableAccess.Value.Eighth));
            }

            if (hasEps12vPowerCable)
            {
                OperationResult<InventorySerializedTransferAccessOctuple> cableAccess =
                    inventory.ClaimManagedSerializedTransferContainers(
                        workbenchContainerId,
                        processorSocketContainerId,
                        memorySlotDefinition.ContainerId,
                        storageSlotDefinition.ContainerId,
                        processorCoolerSlotDefinition.ContainerId,
                        graphicsCardSlotDefinition.ContainerId,
                        powerSupplyBayDefinition.ContainerId,
                        eps12vPowerCableDefinition.RouteContainerId);
                if (cableAccess.IsFailure)
                {
                    return OperationResult<AssemblyBuildAuthority>.Fail(
                        MapManagedFactoryClaimFailure(cableAccess.Error));
                }

                return OperationResult<AssemblyBuildAuthority>.Success(
                    new AssemblyBuildAuthority(
                        componentCatalog,
                        inventory,
                        buildId,
                        chassisId,
                        motherboardSlotId,
                        motherboardFastenerId,
                        handsContainerId,
                        workbenchContainerId,
                        supportedMotherboardFormFactor,
                        cableAccess.Value.First,
                        processorSlotId,
                        processorRetentionId,
                        processorSocketContainerId,
                        supportedCpuSocketFamily,
                        cableAccess.Value.Second,
                        memorySlotDefinition,
                        cableAccess.Value.Third,
                        storageSlotDefinition,
                        cableAccess.Value.Fourth,
                        processorCoolerSlotDefinition,
                        cableAccess.Value.Fifth,
                        graphicsCardSlotDefinition,
                        cableAccess.Value.Sixth,
                        powerSupplyBayDefinition,
                        cableAccess.Value.Seventh,
                        default,
                        null,
                        eps12vPowerCableDefinition,
                        cableAccess.Value.Eighth));
            }

            if (hasPcieGpuPowerCable)
            {
                OperationResult<InventorySerializedTransferAccessOctuple> cableAccess =
                    inventory.ClaimManagedSerializedTransferContainers(
                        workbenchContainerId,
                        processorSocketContainerId,
                        memorySlotDefinition.ContainerId,
                        storageSlotDefinition.ContainerId,
                        processorCoolerSlotDefinition.ContainerId,
                        graphicsCardSlotDefinition.ContainerId,
                        powerSupplyBayDefinition.ContainerId,
                        pcieGpuPowerCableDefinition.RouteContainerId);
                if (cableAccess.IsFailure)
                {
                    return OperationResult<AssemblyBuildAuthority>.Fail(
                        MapManagedFactoryClaimFailure(cableAccess.Error));
                }

                return OperationResult<AssemblyBuildAuthority>.Success(
                    new AssemblyBuildAuthority(
                        componentCatalog,
                        inventory,
                        buildId,
                        chassisId,
                        motherboardSlotId,
                        motherboardFastenerId,
                        handsContainerId,
                        workbenchContainerId,
                        supportedMotherboardFormFactor,
                        cableAccess.Value.First,
                        processorSlotId,
                        processorRetentionId,
                        processorSocketContainerId,
                        supportedCpuSocketFamily,
                        cableAccess.Value.Second,
                        memorySlotDefinition,
                        cableAccess.Value.Third,
                        storageSlotDefinition,
                        cableAccess.Value.Fourth,
                        processorCoolerSlotDefinition,
                        cableAccess.Value.Fifth,
                        graphicsCardSlotDefinition,
                        cableAccess.Value.Sixth,
                        powerSupplyBayDefinition,
                        cableAccess.Value.Seventh,
                        default,
                        null,
                        default,
                        null,
                        pcieGpuPowerCableDefinition,
                        cableAccess.Value.Eighth));
            }

            OperationResult<InventorySerializedTransferAccessSeptuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId,
                    graphicsCardSlotDefinition.ContainerId,
                    powerSupplyBayDefinition.ContainerId);
            if (access.IsFailure)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? AssemblyFailures.RevisionOverflow
                        : access.Error ==
                            InventoryFailures.SerializedTransferContainerOccupied
                            ? AssemblyFailures.SlotOccupied
                            : AssemblyFailures.PlanForeign);
            }

            return OperationResult<AssemblyBuildAuthority>.Success(
                new AssemblyBuildAuthority(
                    componentCatalog,
                    inventory,
                    buildId,
                    chassisId,
                    motherboardSlotId,
                    motherboardFastenerId,
                    handsContainerId,
                    workbenchContainerId,
                    supportedMotherboardFormFactor,
                    access.Value.First,
                    processorSlotId,
                    processorRetentionId,
                    processorSocketContainerId,
                    supportedCpuSocketFamily,
                    access.Value.Second,
                    memorySlotDefinition,
                    access.Value.Third,
                    storageSlotDefinition,
                    access.Value.Fourth,
                    processorCoolerSlotDefinition,
                    access.Value.Fifth,
                    graphicsCardSlotDefinition,
                    access.Value.Sixth,
                    powerSupplyBayDefinition,
                    access.Value.Seventh));
        }

        private static bool IsDuplicatePowerCableFactoryContainer(
            StableId<ContainerIdScope> cableRouteContainerId,
            params StableId<ContainerIdScope>[] existingContainerIds)
        {
            for (int index = 0; index < existingContainerIds.Length; index++)
            {
                if (cableRouteContainerId == existingContainerIds[index])
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasPowerCableDefinitionIdentityConflict(
            Atx24PowerCableDefinition atx24,
            Eps12vPowerCableDefinition eps12v)
        {
            if (atx24.ProductId == eps12v.ProductId ||
                atx24.RouteContainerId == eps12v.RouteContainerId ||
                atx24.Topology.RouteId == eps12v.Topology.RouteId)
            {
                return true;
            }

            StableId<AssemblyPowerCableEndpointIdScope> epsPsu =
                eps12v.Topology.PsuEndpoint.EndpointId;
            StableId<AssemblyPowerCableEndpointIdScope> epsBoard =
                eps12v.Topology.MotherboardEndpoint.EndpointId;
            if (epsPsu == atx24.Topology.PsuPrimaryEndpoint.EndpointId ||
                epsPsu == atx24.Topology.PsuSenseEndpoint.EndpointId ||
                epsPsu == atx24.Topology.MotherboardEndpoint.EndpointId ||
                epsBoard == atx24.Topology.PsuPrimaryEndpoint.EndpointId ||
                epsBoard == atx24.Topology.PsuSenseEndpoint.EndpointId ||
                epsBoard == atx24.Topology.MotherboardEndpoint.EndpointId)
            {
                return true;
            }

            for (int epsIndex = 0;
                 epsIndex < eps12v.Topology.OrderedWaypoints.Count;
                 epsIndex++)
            {
                for (int atxIndex = 0;
                     atxIndex < atx24.Topology.OrderedWaypoints.Count;
                     atxIndex++)
                {
                    if (eps12v.Topology.OrderedWaypoints[epsIndex] ==
                        atx24.Topology.OrderedWaypoints[atxIndex])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasPowerCableDefinitionIdentityConflict(
            Atx24PowerCableDefinition atx24,
            PcieGpuPowerCableDefinition pcieGpu)
        {
            if (atx24.ProductId == pcieGpu.ProductId ||
                atx24.RouteContainerId == pcieGpu.RouteContainerId ||
                atx24.Topology.RouteId == pcieGpu.Topology.RouteId)
            {
                return true;
            }

            StableId<AssemblyPowerCableEndpointIdScope> pciePsu =
                pcieGpu.Topology.PsuEndpoint.EndpointId;
            StableId<AssemblyPowerCableEndpointIdScope> pcieGpuEndpoint =
                pcieGpu.Topology.GraphicsCardEndpoint.EndpointId;
            if (pciePsu == atx24.Topology.PsuPrimaryEndpoint.EndpointId ||
                pciePsu == atx24.Topology.PsuSenseEndpoint.EndpointId ||
                pciePsu == atx24.Topology.MotherboardEndpoint.EndpointId ||
                pcieGpuEndpoint == atx24.Topology.PsuPrimaryEndpoint.EndpointId ||
                pcieGpuEndpoint == atx24.Topology.PsuSenseEndpoint.EndpointId ||
                pcieGpuEndpoint == atx24.Topology.MotherboardEndpoint.EndpointId)
            {
                return true;
            }

            for (int pcieIndex = 0;
                 pcieIndex < pcieGpu.Topology.OrderedWaypoints.Count;
                 pcieIndex++)
            {
                for (int atxIndex = 0;
                     atxIndex < atx24.Topology.OrderedWaypoints.Count;
                     atxIndex++)
                {
                    if (pcieGpu.Topology.OrderedWaypoints[pcieIndex] ==
                        atx24.Topology.OrderedWaypoints[atxIndex])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasPowerCableDefinitionIdentityConflict(
            Eps12vPowerCableDefinition eps12v,
            PcieGpuPowerCableDefinition pcieGpu)
        {
            if (eps12v.ProductId == pcieGpu.ProductId ||
                eps12v.RouteContainerId == pcieGpu.RouteContainerId ||
                eps12v.Topology.RouteId == pcieGpu.Topology.RouteId)
            {
                return true;
            }

            if (eps12v.Topology.PsuEndpoint.EndpointId ==
                    pcieGpu.Topology.PsuEndpoint.EndpointId ||
                eps12v.Topology.PsuEndpoint.EndpointId ==
                    pcieGpu.Topology.GraphicsCardEndpoint.EndpointId ||
                eps12v.Topology.MotherboardEndpoint.EndpointId ==
                    pcieGpu.Topology.PsuEndpoint.EndpointId ||
                eps12v.Topology.MotherboardEndpoint.EndpointId ==
                    pcieGpu.Topology.GraphicsCardEndpoint.EndpointId)
            {
                return true;
            }

            for (int epsIndex = 0;
                 epsIndex < eps12v.Topology.OrderedWaypoints.Count;
                 epsIndex++)
            {
                for (int pcieIndex = 0;
                     pcieIndex < pcieGpu.Topology.OrderedWaypoints.Count;
                     pcieIndex++)
                {
                    if (eps12v.Topology.OrderedWaypoints[epsIndex] ==
                        pcieGpu.Topology.OrderedWaypoints[pcieIndex])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static Failure MapManagedFactoryClaimFailure(Failure failure)
        {
            return failure == InventoryFailures.RevisionOverflow
                ? AssemblyFailures.RevisionOverflow
                : failure == InventoryFailures.SerializedTransferContainerOccupied
                    ? AssemblyFailures.SlotOccupied
                    : AssemblyFailures.PlanForeign;
        }

        public OperationResult<AssemblyOperationReceipt> SeatPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            PowerSupplyMountOrientation orientation,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (HasPowerCableOperationReceipt(operationId))
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesSeatPowerSupply(
                        itemId,
                        slotId,
                        orientation,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateSeatPowerSupply(
                itemId,
                slotId,
                orientation,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _powerSupplyBayDefinition.ContainerId,
                    _powerSupplyInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapPowerSupplyInventoryFailure(prepared.Error, seating: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapPowerSupplyInventoryFailure(committed.Error, seating: true));
            }

            _powerSupplyBayState = PowerSupplyBayState.PowerSupplySeatedUnsecured;
            _powerSupplyItemId = item.Id;
            _powerSupplyProductId = item.ProductId;
            _powerSupplySeatedByOperationId = operationId;
            _powerSupplyRetainedByOperationId = default;
            _powerSupplyMountOrientation = orientation;
            Revision++;

            AssemblyOperationReceipt receipt = CreatePowerSupplyReceipt(
                operationId,
                AssemblyOperationKind.SeatPowerSupply,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _powerSupplyBayDefinition.ContainerId,
                default,
                default,
                expectedAssemblyRevision,
                PowerSupplyBayState.EmptyOpen,
                _powerSupplyBayState,
                orientation);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RetainPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyPowerSupplyRearMountIdScope> rearMountId,
            StableId<AssemblyFastenerIdScope> topLeftFastenerId,
            StableId<AssemblyFastenerIdScope> topRightFastenerId,
            StableId<AssemblyFastenerIdScope> bottomLeftFastenerId,
            StableId<AssemblyFastenerIdScope> bottomRightFastenerId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (HasPowerCableOperationReceipt(operationId))
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesRetainPowerSupply(
                        itemId,
                        slotId,
                        rearMountId,
                        topLeftFastenerId,
                        topRightFastenerId,
                        bottomLeftFastenerId,
                        bottomRightFastenerId,
                        sourcePowerSupplySeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidatePowerSupplyRetention(
                itemId,
                slotId,
                rearMountId,
                topLeftFastenerId,
                topRightFastenerId,
                bottomLeftFastenerId,
                bottomRightFastenerId,
                sourcePowerSupplySeatOperationId,
                default,
                expectedAssemblyRevision,
                retaining: true);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            _powerSupplyBayState = PowerSupplyBayState.PowerSupplyRetained;
            _powerSupplyRetainedByOperationId = operationId;
            Revision++;

            AssemblyOperationReceipt receipt = CreatePowerSupplyReceipt(
                operationId,
                AssemblyOperationKind.RetainPowerSupply,
                _powerSupplyItemId,
                _powerSupplyProductId,
                default,
                default,
                sourcePowerSupplySeatOperationId,
                default,
                expectedAssemblyRevision,
                PowerSupplyBayState.PowerSupplySeatedUnsecured,
                _powerSupplyBayState,
                _powerSupplyMountOrientation);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> UnretainPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyPowerSupplyRearMountIdScope> rearMountId,
            StableId<AssemblyFastenerIdScope> topLeftFastenerId,
            StableId<AssemblyFastenerIdScope> topRightFastenerId,
            StableId<AssemblyFastenerIdScope> bottomLeftFastenerId,
            StableId<AssemblyFastenerIdScope> bottomRightFastenerId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourcePowerSupplyRetentionOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (HasPowerCableOperationReceipt(operationId))
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesUnretainPowerSupply(
                        itemId,
                        slotId,
                        rearMountId,
                        topLeftFastenerId,
                        topRightFastenerId,
                        bottomLeftFastenerId,
                        bottomRightFastenerId,
                        sourcePowerSupplySeatOperationId,
                        sourcePowerSupplyRetentionOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            if (IsAtx24PowerCableRouted ||
                IsEps12vPowerCableRouted ||
                IsPcieGpuPowerCableRouted)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.PowerCableDependentComponentLocked);
            }

            Failure preflightFailure = ValidatePowerSupplyRetention(
                itemId,
                slotId,
                rearMountId,
                topLeftFastenerId,
                topRightFastenerId,
                bottomLeftFastenerId,
                bottomRightFastenerId,
                sourcePowerSupplySeatOperationId,
                sourcePowerSupplyRetentionOperationId,
                expectedAssemblyRevision,
                retaining: false);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            _powerSupplyBayState = PowerSupplyBayState.PowerSupplySeatedUnsecured;
            _powerSupplyRetainedByOperationId = default;
            Revision++;

            AssemblyOperationReceipt receipt = CreatePowerSupplyReceipt(
                operationId,
                AssemblyOperationKind.UnretainPowerSupply,
                _powerSupplyItemId,
                _powerSupplyProductId,
                default,
                default,
                sourcePowerSupplySeatOperationId,
                sourcePowerSupplyRetentionOperationId,
                expectedAssemblyRevision,
                PowerSupplyBayState.PowerSupplyRetained,
                _powerSupplyBayState,
                _powerSupplyMountOrientation);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RemovePowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (HasPowerCableOperationReceipt(operationId))
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesRemovePowerSupply(
                        itemId,
                        slotId,
                        sourcePowerSupplySeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            if (IsEps12vPowerCableRouted || IsPcieGpuPowerCableRouted)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.PowerCableDependentComponentLocked);
            }

            Failure preflightFailure = ValidateRemovePowerSupply(
                itemId,
                slotId,
                sourcePowerSupplySeatOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _powerSupplyInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapPowerSupplyInventoryFailure(prepared.Error, seating: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapPowerSupplyInventoryFailure(committed.Error, seating: false));
            }

            PowerSupplyMountOrientation removedOrientation =
                _powerSupplyMountOrientation;
            _powerSupplyBayState = PowerSupplyBayState.EmptyOpen;
            _powerSupplyItemId = default;
            _powerSupplyProductId = default;
            _powerSupplySeatedByOperationId = default;
            _powerSupplyRetainedByOperationId = default;
            _powerSupplyMountOrientation = default;
            Revision++;

            AssemblyOperationReceipt receipt = CreatePowerSupplyReceipt(
                operationId,
                AssemblyOperationKind.RemovePowerSupply,
                item.Id,
                item.ProductId,
                _powerSupplyBayDefinition.ContainerId,
                _handsContainerId,
                sourcePowerSupplySeatOperationId,
                default,
                expectedAssemblyRevision,
                PowerSupplyBayState.PowerSupplySeatedUnsecured,
                _powerSupplyBayState,
                removedOrientation);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        private AssemblyOperationReceipt CreatePowerSupplyReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationKind operationKind,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourcePowerSupplyRetentionOperationId,
            long expectedAssemblyRevision,
            PowerSupplyBayState previousPowerSupplyBayState,
            PowerSupplyBayState resultingPowerSupplyBayState,
            PowerSupplyMountOrientation orientation)
        {
            bool retentionOperation =
                operationKind == AssemblyOperationKind.RetainPowerSupply ||
                operationKind == AssemblyOperationKind.UnretainPowerSupply;
            return new AssemblyOperationReceipt(
                operationId: operationId,
                operationKind: operationKind,
                buildId: BuildId,
                chassisId: ChassisId,
                slotId: _powerSupplyBayDefinition.SlotId,
                itemId: itemId,
                productId: productId,
                sourceContainerId: sourceContainerId,
                targetContainerId: targetContainerId,
                sourceAttachOperationId: default,
                sourceSecureOperationId: default,
                fastenerId: default,
                retentionId: default,
                sourceProcessorSeatOperationId: default,
                sourceProcessorRetentionOperationId: default,
                sequenceIndex: retentionOperation ? 0 : -1,
                expectedAssemblyRevision: expectedAssemblyRevision,
                previousSeatState: _motherboardSeatState,
                resultingSeatState: _motherboardSeatState,
                previousProcessorSocketState: _processorSocketState,
                resultingProcessorSocketState: _processorSocketState,
                assemblyRevision: Revision,
                inventoryRevision: _inventory.Revision,
                previousMemorySlotState: _memorySlotState,
                resultingMemorySlotState: _memorySlotState,
                previousStorageSlotState: _storageSlotState,
                resultingStorageSlotState: _storageSlotState,
                previousProcessorCoolerSlotState: _processorCoolerSlotState,
                resultingProcessorCoolerSlotState: _processorCoolerSlotState,
                processorCoolerMountOrientation: _processorCoolerMountOrientation,
                previousProcessorCoolerTimState: _processorCoolerTimState,
                resultingProcessorCoolerTimState: _processorCoolerTimState,
                processorCoolerSlotDefinition: _processorCoolerSlotDefinition,
                previousGraphicsCardSlotState: _graphicsCardSlotState,
                resultingGraphicsCardSlotState: _graphicsCardSlotState,
                graphicsCardMountOrientation: _graphicsCardMountOrientation,
                graphicsCardSlotDefinition: _graphicsCardSlotDefinition,
                previousPowerSupplyBayState: previousPowerSupplyBayState,
                resultingPowerSupplyBayState: resultingPowerSupplyBayState,
                sourcePowerSupplySeatOperationId: sourcePowerSupplySeatOperationId,
                sourcePowerSupplyRetentionOperationId:
                    sourcePowerSupplyRetentionOperationId,
                powerSupplyMountOrientation: orientation,
                powerSupplyBayDefinition: _powerSupplyBayDefinition);
        }

        private Failure ValidateSeatPowerSupply(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            PowerSupplyMountOrientation orientation,
            long expectedAssemblyRevision)
        {
            if (!HasPowerSupplyBay)
            {
                return AssemblyFailures.InvalidPowerSupplyBayDefinition;
            }

            if (slotId != _powerSupplyBayDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_powerSupplyBayState != PowerSupplyBayState.EmptyOpen)
            {
                return AssemblyFailures.PowerSupplyBayOccupied;
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item))
            {
                return AssemblyFailures.UnknownItem;
            }

            if (item.ContainerId != _handsContainerId)
            {
                return AssemblyFailures.ItemNotInActorHands;
            }

            if (!_componentCatalog.TryGet(
                    item.ProductId,
                    out PcComponentSpecification powerSupplySpecification))
            {
                return AssemblyFailures.UnknownComponentSpecification;
            }

            AssemblyCompatibilityResult compatibility =
                AssemblyCompatibilityEvaluator.EvaluatePowerSupplySeat(
                    powerSupplySpecification,
                    _powerSupplyBayDefinition.SupportedPowerSupplyType,
                    orientation);
            return compatibility.IsCompatible ? Failure.None : compatibility.Reason;
        }

        private Failure ValidatePowerSupplyRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyPowerSupplyRearMountIdScope> rearMountId,
            StableId<AssemblyFastenerIdScope> topLeftFastenerId,
            StableId<AssemblyFastenerIdScope> topRightFastenerId,
            StableId<AssemblyFastenerIdScope> bottomLeftFastenerId,
            StableId<AssemblyFastenerIdScope> bottomRightFastenerId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourcePowerSupplyRetentionOperationId,
            long expectedAssemblyRevision,
            bool retaining)
        {
            if (!HasPowerSupplyBay || slotId != _powerSupplyBayDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            PowerSupplyRetentionTopology topology =
                _powerSupplyBayDefinition.RetentionTopology;
            if (rearMountId != topology.RearMountId)
            {
                return AssemblyFailures.InvalidPowerSupplyRearMount;
            }

            if (topLeftFastenerId != topology.TopLeftFastenerId ||
                topRightFastenerId != topology.TopRightFastenerId ||
                bottomLeftFastenerId != topology.BottomLeftFastenerId ||
                bottomRightFastenerId != topology.BottomRightFastenerId)
            {
                return AssemblyFailures.InvalidPowerSupplyFastenerTopology;
            }

            if (itemId.IsEmpty ||
                (!_powerSupplyItemId.IsEmpty && itemId != _powerSupplyItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourcePowerSupplySeatOperationId.IsEmpty ||
                sourcePowerSupplySeatOperationId != _powerSupplySeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourcePowerSupplySeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatPowerSupply ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (retaining)
            {
                if (!sourcePowerSupplyRetentionOperationId.IsEmpty ||
                    _powerSupplyBayState !=
                        PowerSupplyBayState.PowerSupplySeatedUnsecured ||
                    !_powerSupplyRetainedByOperationId.IsEmpty)
                {
                    return AssemblyFailures.PowerSupplyRetentionOutOfOrder;
                }
            }
            else if (sourcePowerSupplyRetentionOperationId.IsEmpty ||
                     sourcePowerSupplyRetentionOperationId !=
                         _powerSupplyRetainedByOperationId ||
                     _powerSupplyBayState != PowerSupplyBayState.PowerSupplyRetained ||
                     !_receipts.TryGetValue(
                         sourcePowerSupplyRetentionOperationId,
                         out AssemblyOperationReceipt retentionReceipt) ||
                     retentionReceipt.OperationKind !=
                         AssemblyOperationKind.RetainPowerSupply ||
                     retentionReceipt.ItemId != itemId ||
                     retentionReceipt.SourcePowerSupplySeatOperationId !=
                         sourcePowerSupplySeatOperationId ||
                     !retentionReceipt.PowerSupplyBayDefinition.HasExactIdentity(
                         _powerSupplyBayDefinition))
            {
                return AssemblyFailures.PowerSupplyRetentionOutOfOrder;
            }

            return IsPowerSupplySeatedItem(itemId)
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private Failure ValidateRemovePowerSupply(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            if (!HasPowerSupplyBay || slotId != _powerSupplyBayDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourcePowerSupplySeatOperationId.IsEmpty ||
                sourcePowerSupplySeatOperationId != _powerSupplySeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourcePowerSupplySeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatPowerSupply ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_powerSupplyBayState == PowerSupplyBayState.PowerSupplyRetained)
            {
                return AssemblyFailures.PowerSupplyRetained;
            }

            if (_powerSupplyBayState !=
                    PowerSupplyBayState.PowerSupplySeatedUnsecured ||
                itemId != _powerSupplyItemId)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            return IsPowerSupplySeatedItem(itemId)
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private bool ValidatePowerSupplyStateInvariants()
        {
            if (!HasPowerSupplyBay)
            {
                return _powerSupplyInventoryTransferAccess == null &&
                       _powerSupplyBayState == PowerSupplyBayState.Unsupported &&
                       _powerSupplyBayDefinition.SlotId.IsEmpty &&
                       _powerSupplyBayDefinition.ContainerId.IsEmpty &&
                       _powerSupplyBayDefinition.RetentionTopology == null &&
                       _powerSupplyBayDefinition.SupportedPowerSupplyType == default &&
                       _powerSupplyItemId.IsEmpty &&
                       _powerSupplyProductId.IsEmpty &&
                       _powerSupplySeatedByOperationId.IsEmpty &&
                       _powerSupplyRetainedByOperationId.IsEmpty &&
                       _powerSupplyMountOrientation == default;
            }

            if (_powerSupplyInventoryTransferAccess == null ||
                _powerSupplyBayDefinition.SlotId == MotherboardSlotId ||
                _powerSupplyBayDefinition.SlotId == _processorSlotId ||
                _powerSupplyBayDefinition.SlotId == _memorySlotDefinition.SlotId ||
                _powerSupplyBayDefinition.SlotId == _storageSlotDefinition.SlotId ||
                _powerSupplyBayDefinition.SlotId ==
                    _processorCoolerSlotDefinition.SlotId ||
                _powerSupplyBayDefinition.SlotId == _graphicsCardSlotDefinition.SlotId ||
                _powerSupplyBayDefinition.ContainerId == _handsContainerId ||
                _powerSupplyBayDefinition.ContainerId == _workbenchContainerId ||
                _powerSupplyBayDefinition.ContainerId == _processorSocketContainerId ||
                _powerSupplyBayDefinition.ContainerId ==
                    _memorySlotDefinition.ContainerId ||
                _powerSupplyBayDefinition.ContainerId ==
                    _storageSlotDefinition.ContainerId ||
                _powerSupplyBayDefinition.ContainerId ==
                    _processorCoolerSlotDefinition.ContainerId ||
                _powerSupplyBayDefinition.ContainerId ==
                    _graphicsCardSlotDefinition.ContainerId ||
                HasPowerSupplyFastenerConflict(
                    _powerSupplyBayDefinition.RetentionTopology,
                    _motherboardFastenerId,
                    _graphicsCardSlotDefinition.RetentionTopology.BracketFastenerId) ||
                !IsCapacityOneWorkbenchContainer(
                    _inventory,
                    _powerSupplyBayDefinition.ContainerId))
            {
                return false;
            }

            if (_powerSupplyBayState == PowerSupplyBayState.EmptyOpen)
            {
                return _powerSupplyItemId.IsEmpty &&
                       _powerSupplyProductId.IsEmpty &&
                       _powerSupplySeatedByOperationId.IsEmpty &&
                       _powerSupplyRetainedByOperationId.IsEmpty &&
                       _powerSupplyMountOrientation == default &&
                       _inventory.GetContainerQuantity(
                           _powerSupplyBayDefinition.ContainerId).Value == 0;
            }

            if ((_powerSupplyBayState !=
                    PowerSupplyBayState.PowerSupplySeatedUnsecured &&
                 _powerSupplyBayState != PowerSupplyBayState.PowerSupplyRetained) ||
                !IsPowerSupplySeatedItem(_powerSupplyItemId) ||
                !_componentCatalog.TryGet(
                    _powerSupplyProductId,
                    out PcComponentSpecification specification) ||
                !AssemblyCompatibilityEvaluator.EvaluatePowerSupplySeat(
                    specification,
                    _powerSupplyBayDefinition.SupportedPowerSupplyType,
                    _powerSupplyMountOrientation).IsCompatible ||
                !_receipts.TryGetValue(
                    _powerSupplySeatedByOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatPowerSupply ||
                seatReceipt.ItemId != _powerSupplyItemId ||
                seatReceipt.ProductId != _powerSupplyProductId ||
                seatReceipt.SlotId != _powerSupplyBayDefinition.SlotId ||
                seatReceipt.PowerSupplyMountOrientation !=
                    _powerSupplyMountOrientation ||
                !seatReceipt.PowerSupplyBayDefinition.HasExactIdentity(
                    _powerSupplyBayDefinition))
            {
                return false;
            }

            if (_powerSupplyBayState ==
                PowerSupplyBayState.PowerSupplySeatedUnsecured)
            {
                return _powerSupplyRetainedByOperationId.IsEmpty;
            }

            return !_powerSupplyRetainedByOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       _powerSupplyRetainedByOperationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.RetainPowerSupply &&
                   retentionReceipt.ItemId == _powerSupplyItemId &&
                   retentionReceipt.SourcePowerSupplySeatOperationId ==
                       _powerSupplySeatedByOperationId &&
                   retentionReceipt.PowerSupplyBayDefinition.HasExactIdentity(
                       _powerSupplyBayDefinition);
        }

        private bool IsMatchingPowerSupplySeatReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt seatReceipt) &&
                   seatReceipt.OperationKind == AssemblyOperationKind.SeatPowerSupply &&
                   seatReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   seatReceipt.ItemId == descendant.ItemId &&
                   seatReceipt.ProductId == descendant.ProductId &&
                   seatReceipt.SlotId == descendant.SlotId &&
                   seatReceipt.PowerSupplyMountOrientation ==
                       descendant.PowerSupplyMountOrientation &&
                   seatReceipt.PowerSupplyBayDefinition.HasExactIdentity(
                       descendant.PowerSupplyBayDefinition);
        }

        private bool IsMatchingPowerSupplyRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.RetainPowerSupply &&
                   retentionReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   retentionReceipt.ItemId == descendant.ItemId &&
                   retentionReceipt.ProductId == descendant.ProductId &&
                   retentionReceipt.SlotId == descendant.SlotId &&
                   retentionReceipt.SourcePowerSupplySeatOperationId ==
                       descendant.SourcePowerSupplySeatOperationId &&
                   retentionReceipt.PowerSupplyBayDefinition.HasExactIdentity(
                       descendant.PowerSupplyBayDefinition);
        }

        private bool IsPowerSupplySeatedItem(StableId<ItemInstanceIdScope> itemId)
        {
            return !itemId.IsEmpty &&
                   itemId == _powerSupplyItemId &&
                   !_powerSupplyProductId.IsEmpty &&
                   !_powerSupplySeatedByOperationId.IsEmpty &&
                   _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _powerSupplyProductId &&
                   item.ContainerId == _powerSupplyBayDefinition.ContainerId;
        }

        private static Failure MapPowerSupplyInventoryFailure(
            Failure failure,
            bool seating)
        {
            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return seating
                    ? AssemblyFailures.PowerSupplyBayCapacityExceeded
                    : AssemblyFailures.HandsCapacityExceeded;
            }

            if (failure == InventoryFailures.RevisionOverflow)
            {
                return AssemblyFailures.InventoryRevisionOverflow;
            }

            if (failure == InventoryFailures.SerializedTransferPlanStale)
            {
                return AssemblyFailures.InventoryTransferStale;
            }

            if (failure == InventoryFailures.SerializedTransferAccessInvalid ||
                failure == InventoryFailures.SerializedTransferContainerManaged)
            {
                return AssemblyFailures.PlanForeign;
            }

            return AssemblyFailures.InventoryTransferRejected;
        }

        private static bool HasDuplicatePowerSupplyFactorySlot(
            params StableId<AssemblySlotIdScope>[] slotIds)
        {
            for (int left = 0; left < slotIds.Length; left++)
            {
                if (slotIds[left].IsEmpty)
                {
                    return true;
                }

                for (int right = left + 1; right < slotIds.Length; right++)
                {
                    if (slotIds[left] == slotIds[right])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasDuplicatePowerSupplyFactoryContainer(
            params StableId<ContainerIdScope>[] containerIds)
        {
            for (int left = 0; left < containerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < containerIds.Length; right++)
                {
                    if (containerIds[left] == containerIds[right])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool HasPowerSupplyFastenerConflict(
            PowerSupplyRetentionTopology topology,
            StableId<AssemblyFastenerIdScope> motherboardFastenerId,
            StableId<AssemblyFastenerIdScope> graphicsCardFastenerId)
        {
            if (topology == null || !topology.IsValid)
            {
                return true;
            }

            for (int index = 0; index < topology.PhysicalOrder.Count; index++)
            {
                StableId<AssemblyFastenerIdScope> fastenerId =
                    topology.PhysicalOrder[index];
                if (fastenerId == motherboardFastenerId ||
                    fastenerId == graphicsCardFastenerId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
