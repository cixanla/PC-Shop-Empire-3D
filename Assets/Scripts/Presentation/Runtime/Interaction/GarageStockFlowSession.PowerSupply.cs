using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PowerSupplyProductIdValue =
            "catalog.psu.northstar-p01-atx";
        public const string PowerSupplyCategoryIdValue =
            "catalog.category.power-supplies";
        public const string PowerSupplyItemInstanceIdValue =
            "inventory.item.northstar-p01-atx-001";
        public const string PowerSupplyBayContainerIdValue =
            "inventory.container.assembly-power-supply-bay";
        public const string PowerSupplyBaySlotIdValue =
            "assembly.slot.power-supply-bottom-rear";
        public const string PowerSupplyRearMountIdValue =
            "assembly.mount.power-supply-rear";
        public const string PowerSupplyTopLeftFastenerIdValue =
            "assembly.fastener.power-supply-rear-01";
        public const string PowerSupplyTopRightFastenerIdValue =
            "assembly.fastener.power-supply-rear-02";
        public const string PowerSupplyBottomLeftFastenerIdValue =
            "assembly.fastener.power-supply-rear-03";
        public const string PowerSupplyBottomRightFastenerIdValue =
            "assembly.fastener.power-supply-rear-04";
        public const string PowerSupplyDisplayName =
            "Northstar P-01 ATX Güç Kaynağı";
        public const long PowerSupplyUnitCostMinorUnits = 7_900;

        public StableId<ProductDefinitionIdScope> PowerSupplyProductId =>
            StableId<ProductDefinitionIdScope>.Parse(PowerSupplyProductIdValue);

        public StableId<ItemInstanceIdScope> PowerSupplyItemId =>
            StableId<ItemInstanceIdScope>.Parse(PowerSupplyItemInstanceIdValue);

        public StableId<ContainerIdScope> PowerSupplyBayContainerId =>
            StableId<ContainerIdScope>.Parse(PowerSupplyBayContainerIdValue);

        public StableId<AssemblySlotIdScope> PowerSupplyBaySlotId =>
            StableId<AssemblySlotIdScope>.Parse(PowerSupplyBaySlotIdValue);

        public StableId<AssemblyPowerSupplyRearMountIdScope> PowerSupplyRearMountId =>
            StableId<AssemblyPowerSupplyRearMountIdScope>.Parse(
                PowerSupplyRearMountIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyTopLeftFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyTopLeftFastenerIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyTopRightFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyTopRightFastenerIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyBottomLeftFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyBottomLeftFastenerIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyBottomRightFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyBottomRightFastenerIdValue);

        public bool TryGetPowerSupplyItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(PowerSupplyItemId, out item);
        }

        public OperationResult PickupLoosePowerSupplyToHands()
        {
            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalPowerSupply(
                        PrototypePowerSupplyBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            if (!AssemblyBuild.HasPowerSupplyBay ||
                AssemblyBuild.PowerSupplyBayState != PowerSupplyBayState.EmptyOpen ||
                !TryGetPowerSupplyItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(
                PowerSupplyItemId,
                HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldPowerSupplyInCustomPcBuildKit()
        {
            return PlaceHeldPowerSupplyInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldPowerSupplyInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypePowerSupplyBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.PowerSupplyInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalPowerSupply(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedPowerSupplyForAssembly()
        {
            return PickupStagedPowerSupplyForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedPowerSupplyForAssembly(
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
                snapshot.StorageSlotState != StorageSlotState.StorageDeviceSecured ||
                snapshot.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.CoolerRetained ||
                snapshot.ProcessorCoolerTimState !=
                    ProcessorCoolerTimState.AppliedConsumed ||
                snapshot.GraphicsCardSlotState !=
                    GraphicsCardSlotState.GraphicsCardRetained ||
                snapshot.PowerSupplyBayState != PowerSupplyBayState.EmptyOpen ||
                snapshot.MotherboardItemId != MotherboardItemId ||
                snapshot.ProcessorItemId != ProcessorItemId ||
                snapshot.MemoryItemId != MemoryItemId ||
                snapshot.StorageItemId != StorageItemId ||
                snapshot.ProcessorCoolerItemId != ProcessorCoolerItemId ||
                snapshot.GraphicsCardItemId != GraphicsCardAssemblyItemId ||
                snapshot.InstalledByOperationId.IsEmpty ||
                snapshot.SecuredByOperationId.IsEmpty ||
                snapshot.ProcessorSeatedByOperationId.IsEmpty ||
                snapshot.ProcessorRetainedByOperationId.IsEmpty ||
                snapshot.MemorySeatedByOperationId.IsEmpty ||
                snapshot.MemoryRetainedByOperationId.IsEmpty ||
                snapshot.StorageSeatedByOperationId.IsEmpty ||
                snapshot.StorageSecuredByOperationId.IsEmpty ||
                snapshot.ProcessorCoolerSeatedByOperationId.IsEmpty ||
                snapshot.ProcessorCoolerRetainedByOperationId.IsEmpty ||
                snapshot.GraphicsCardSeatedByOperationId.IsEmpty ||
                snapshot.GraphicsCardRetainedByOperationId.IsEmpty ||
                !TryGetMotherboardItem(out InventoryItemRecord motherboard) ||
                motherboard.ContainerId != WorkbenchContainerId ||
                !TryGetProcessorItem(out InventoryItemRecord processor) ||
                processor.ContainerId != ProcessorSocketContainerId ||
                !TryGetMemoryItem(out InventoryItemRecord memoryModule) ||
                memoryModule.ContainerId != MemorySlotContainerId ||
                !TryGetStorageItem(out InventoryItemRecord storage) ||
                storage.ContainerId != StorageSlotContainerId ||
                !TryGetProcessorCoolerItem(out InventoryItemRecord processorCooler) ||
                processorCooler.ContainerId != ProcessorCoolerSlotContainerId ||
                (processorCooler.StateFlags &
                 InventorySerializedItemStateFlags.PreAppliedConsumableConsumed) == 0 ||
                !TryGetGraphicsCardAssemblyItem(out InventoryItemRecord graphicsCard) ||
                graphicsCard.ContainerId != GraphicsCardSlotContainerId ||
                !TryGetPowerSupplyItem(out InventoryItemRecord powerSupply) ||
                powerSupply.Id != PowerSupplyItemId ||
                powerSupply.ProductId != PowerSupplyProductId ||
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
                !HasLiveRetainedMemoryModuleAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveSecuredStorageAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedProcessorCoolerAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedGraphicsCardAssemblyPrerequisite(
                    snapshot,
                    workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            return CustomPcBuildKit.ReleaseCanonicalPowerSupplyForAssembly(
                PrototypePowerSupplyAssemblyHandoffOperationId,
                workOrder,
                PowerSupplyBayContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult DropHeldPowerSupplyToWorld()
        {
            if (CustomPcBuildKit != null &&
                CustomPcBuildKit.TryGetReceipt(
                    PrototypePowerSupplyBuildKitOperationId,
                    out CustomPcBuildKitReceipt buildKitReceipt) &&
                buildKitReceipt.Stage == CustomPcBuildKitStage.PowerSupplyInHands)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            if (!AssemblyBuild.HasPowerSupplyBay ||
                AssemblyBuild.PowerSupplyBayState != PowerSupplyBayState.EmptyOpen ||
                !TryGetPowerSupplyItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                PowerSupplyItemId,
                WorldFloorContainerId);
        }

        public OperationResult<AssemblyOperationReceipt> SeatPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            PowerSupplyMountOrientation orientation,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SeatPowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                orientation,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RetainPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RetainPowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                PowerSupplyRearMountId,
                PowerSupplyTopLeftFastenerId,
                PowerSupplyTopRightFastenerId,
                PowerSupplyBottomLeftFastenerId,
                PowerSupplyBottomRightFastenerId,
                sourcePowerSupplySeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> UnretainPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.UnretainPowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                PowerSupplyRearMountId,
                PowerSupplyTopLeftFastenerId,
                PowerSupplyTopRightFastenerId,
                PowerSupplyBottomLeftFastenerId,
                PowerSupplyBottomRightFastenerId,
                sourcePowerSupplySeatOperationId,
                sourcePowerSupplyRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RemovePowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RemovePowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                sourcePowerSupplySeatOperationId,
                expectedAssemblyRevision);
        }

        private bool HasLiveRetainedGraphicsCardAssemblyPrerequisite(
            AssemblyBuildSnapshot snapshot,
            CustomPcBuildOrderRecord workOrder,
            bool requireCurrentRevision = true)
        {
            if (workOrder == null ||
                snapshot.BuildId != AssemblyBuild.BuildId ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.GraphicsCardSeatedByOperationId,
                    out AssemblyOperationReceipt seat) ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.GraphicsCardRetainedByOperationId,
                    out AssemblyOperationReceipt retain))
            {
                return false;
            }

            CustomPcBuildOrderLineSnapshot canonicalGraphicsCard = null;
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.GraphicsCard)
                {
                    if (canonicalGraphicsCard != null)
                    {
                        return false;
                    }

                    canonicalGraphicsCard = line;
                }
            }

            return canonicalGraphicsCard != null &&
                   canonicalGraphicsCard.ItemId == snapshot.GraphicsCardItemId &&
                   canonicalGraphicsCard.ProductId == snapshot.GraphicsCardProductId &&
                   seat.OperationId == snapshot.GraphicsCardSeatedByOperationId &&
                   seat.OperationKind == AssemblyOperationKind.SeatGraphicsCard &&
                   seat.BuildId == snapshot.BuildId &&
                   seat.ChassisId == snapshot.ChassisId &&
                   seat.SlotId == snapshot.GraphicsCardSlotId &&
                   seat.ItemId == snapshot.GraphicsCardItemId &&
                   seat.ProductId == snapshot.GraphicsCardProductId &&
                   seat.SourceContainerId == HandsContainerId &&
                   seat.TargetContainerId == GraphicsCardSlotContainerId &&
                   seat.SourceAttachOperationId == snapshot.InstalledByOperationId &&
                   seat.SourceSecureOperationId == snapshot.SecuredByOperationId &&
                   seat.PreviousGraphicsCardSlotState ==
                       GraphicsCardSlotState.EmptyOpen &&
                   seat.ResultingGraphicsCardSlotState ==
                       GraphicsCardSlotState.GraphicsCardSeatedUnsecured &&
                   retain.OperationId == snapshot.GraphicsCardRetainedByOperationId &&
                   retain.OperationKind == AssemblyOperationKind.RetainGraphicsCard &&
                   retain.BuildId == snapshot.BuildId &&
                   retain.ChassisId == snapshot.ChassisId &&
                   retain.SlotId == snapshot.GraphicsCardSlotId &&
                   retain.ItemId == snapshot.GraphicsCardItemId &&
                   retain.ProductId == snapshot.GraphicsCardProductId &&
                   retain.SourceGraphicsCardSeatOperationId == seat.OperationId &&
                   retain.PreviousGraphicsCardSlotState ==
                       GraphicsCardSlotState.GraphicsCardSeatedUnsecured &&
                   retain.ResultingGraphicsCardSlotState ==
                       GraphicsCardSlotState.GraphicsCardRetained &&
                   (requireCurrentRevision
                       ? retain.AssemblyRevision == snapshot.Revision
                       : retain.AssemblyRevision > 0 &&
                         retain.AssemblyRevision < snapshot.Revision);
        }
    }
}
