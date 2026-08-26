using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string GraphicsCardAssemblyItemInstanceIdValue =
            "inventory.item.northstar-a60-assembly-001";
        public const string GraphicsCardSlotContainerIdValue =
            "inventory.container.assembly-graphics-card-x16";
        public const string GraphicsCardSlotIdValue =
            "assembly.slot.graphics-card-pcie-x16";
        public const string GraphicsCardLatchIdValue =
            "assembly.latch.graphics-card-pcie-x16";
        public const string GraphicsCardRearBracketIdValue =
            "assembly.bracket.graphics-card-rear";
        public const string GraphicsCardBracketFastenerIdValue =
            "assembly.fastener.graphics-card-rear-01";
        public const long GraphicsCardAssemblyUnitCostMinorUnits =
            PrototypeUnitCostMinorUnits;

        public StableId<ItemInstanceIdScope> GraphicsCardAssemblyItemId =>
            StableId<ItemInstanceIdScope>.Parse(
                GraphicsCardAssemblyItemInstanceIdValue);

        public StableId<ContainerIdScope> GraphicsCardSlotContainerId =>
            StableId<ContainerIdScope>.Parse(GraphicsCardSlotContainerIdValue);

        public StableId<AssemblySlotIdScope> GraphicsCardSlotId =>
            StableId<AssemblySlotIdScope>.Parse(GraphicsCardSlotIdValue);

        public StableId<AssemblyGraphicsCardLatchIdScope> GraphicsCardLatchId =>
            StableId<AssemblyGraphicsCardLatchIdScope>.Parse(
                GraphicsCardLatchIdValue);

        public StableId<AssemblyFastenerIdScope> GraphicsCardBracketFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                GraphicsCardBracketFastenerIdValue);

        public bool TryGetGraphicsCardAssemblyItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(GraphicsCardAssemblyItemId, out item);
        }

        public OperationResult PickupLooseGraphicsCardToHands()
        {
            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalGraphicsCard(
                        PrototypeGraphicsCardBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            if (!AssemblyBuild.HasGraphicsCardSlot ||
                AssemblyBuild.GraphicsCardSlotState != GraphicsCardSlotState.EmptyOpen ||
                !TryGetGraphicsCardAssemblyItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(
                GraphicsCardAssemblyItemId,
                HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldGraphicsCardInCustomPcBuildKit()
        {
            return PlaceHeldGraphicsCardInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldGraphicsCardInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeGraphicsCardBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.GraphicsCardInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalGraphicsCard(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedGraphicsCardForAssembly()
        {
            return PickupStagedGraphicsCardForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedGraphicsCardForAssembly(
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
                snapshot.GraphicsCardSlotState != GraphicsCardSlotState.EmptyOpen ||
                snapshot.MotherboardItemId != MotherboardItemId ||
                snapshot.ProcessorItemId != ProcessorItemId ||
                snapshot.MemoryItemId != MemoryItemId ||
                snapshot.StorageItemId != StorageItemId ||
                snapshot.ProcessorCoolerItemId != ProcessorCoolerItemId ||
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
                graphicsCard.Id != GraphicsCardAssemblyItemId ||
                graphicsCard.ProductId != ProductId ||
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
                    workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            return CustomPcBuildKit.ReleaseCanonicalGraphicsCardForAssembly(
                PrototypeGraphicsCardAssemblyHandoffOperationId,
                workOrder,
                GraphicsCardSlotContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult DropHeldGraphicsCardToWorld()
        {
            if (CustomPcBuildKit != null &&
                CustomPcBuildKit.TryGetReceipt(
                    PrototypeGraphicsCardBuildKitOperationId,
                    out CustomPcBuildKitReceipt buildKitReceipt) &&
                buildKitReceipt.Stage == CustomPcBuildKitStage.GraphicsCardInHands)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            if (!AssemblyBuild.HasGraphicsCardSlot ||
                AssemblyBuild.GraphicsCardSlotState != GraphicsCardSlotState.EmptyOpen ||
                !TryGetGraphicsCardAssemblyItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                GraphicsCardAssemblyItemId,
                WorldFloorContainerId);
        }

        public OperationResult<AssemblyOperationReceipt> SeatGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            GraphicsCardMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SeatGraphicsCard(
                operationId,
                GraphicsCardAssemblyItemId,
                GraphicsCardSlotId,
                orientation,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RetainGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RetainGraphicsCard(
                operationId,
                GraphicsCardAssemblyItemId,
                GraphicsCardSlotId,
                GraphicsCardLatchId,
                GraphicsCardBracketFastenerId,
                sourceGraphicsCardSeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> UnretainGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.UnretainGraphicsCard(
                operationId,
                GraphicsCardAssemblyItemId,
                GraphicsCardSlotId,
                GraphicsCardLatchId,
                GraphicsCardBracketFastenerId,
                sourceGraphicsCardSeatOperationId,
                sourceGraphicsCardRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RemoveGraphicsCard(
                operationId,
                GraphicsCardAssemblyItemId,
                GraphicsCardSlotId,
                sourceGraphicsCardSeatOperationId,
                expectedAssemblyRevision);
        }

        private bool HasLiveRetainedProcessorCoolerAssemblyPrerequisite(
            AssemblyBuildSnapshot snapshot,
            CustomPcBuildOrderRecord workOrder)
        {
            if (workOrder == null ||
                snapshot.BuildId != AssemblyBuild.BuildId ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.ProcessorCoolerSeatedByOperationId,
                    out AssemblyOperationReceipt seat) ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.ProcessorCoolerRetainedByOperationId,
                    out AssemblyOperationReceipt retain))
            {
                return false;
            }

            CustomPcBuildOrderLineSnapshot canonicalProcessorCooler = null;
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.ProcessorCooler)
                {
                    if (canonicalProcessorCooler != null)
                    {
                        return false;
                    }

                    canonicalProcessorCooler = line;
                }
            }

            return canonicalProcessorCooler != null &&
                   canonicalProcessorCooler.ItemId ==
                       snapshot.ProcessorCoolerItemId &&
                   canonicalProcessorCooler.ProductId ==
                       snapshot.ProcessorCoolerProductId &&
                   seat.OperationId == snapshot.ProcessorCoolerSeatedByOperationId &&
                   seat.OperationKind == AssemblyOperationKind.SeatProcessorCooler &&
                   seat.BuildId == snapshot.BuildId &&
                   seat.ChassisId == snapshot.ChassisId &&
                   seat.SlotId == snapshot.ProcessorCoolerSlotId &&
                   seat.ItemId == snapshot.ProcessorCoolerItemId &&
                   seat.ProductId == snapshot.ProcessorCoolerProductId &&
                   seat.SourceContainerId == HandsContainerId &&
                   seat.TargetContainerId == ProcessorCoolerSlotContainerId &&
                   seat.SourceAttachOperationId == snapshot.InstalledByOperationId &&
                   seat.SourceSecureOperationId == snapshot.SecuredByOperationId &&
                   seat.SourceProcessorSeatOperationId ==
                       snapshot.ProcessorSeatedByOperationId &&
                   seat.SourceProcessorRetentionOperationId ==
                       snapshot.ProcessorRetainedByOperationId &&
                   seat.PreviousProcessorCoolerSlotState ==
                       ProcessorCoolerSlotState.EmptyOpen &&
                   seat.ResultingProcessorCoolerSlotState ==
                       ProcessorCoolerSlotState.CoolerSeatedUnsecured &&
                   seat.PreviousProcessorCoolerTimState ==
                       ProcessorCoolerTimState.PreAppliedUnused &&
                   seat.ResultingProcessorCoolerTimState ==
                       ProcessorCoolerTimState.AppliedConsumed &&
                   retain.OperationId ==
                       snapshot.ProcessorCoolerRetainedByOperationId &&
                   retain.OperationKind ==
                       AssemblyOperationKind.RetainProcessorCooler &&
                   retain.BuildId == snapshot.BuildId &&
                   retain.ChassisId == snapshot.ChassisId &&
                   retain.SlotId == snapshot.ProcessorCoolerSlotId &&
                   retain.ItemId == snapshot.ProcessorCoolerItemId &&
                   retain.ProductId == snapshot.ProcessorCoolerProductId &&
                   retain.SourceProcessorCoolerSeatOperationId == seat.OperationId &&
                   retain.PreviousProcessorCoolerSlotState ==
                       ProcessorCoolerSlotState.CoolerSeatedUnsecured &&
                   retain.ResultingProcessorCoolerSlotState ==
                       ProcessorCoolerSlotState.CoolerRetained &&
                   retain.AssemblyRevision == snapshot.Revision;
        }
    }
}
