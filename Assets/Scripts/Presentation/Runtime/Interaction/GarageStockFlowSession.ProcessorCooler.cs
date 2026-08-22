using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string ProcessorCoolerProductIdValue =
            "catalog.cooler.northstar-topdown-lga1700";
        public const string ProcessorCoolerCategoryIdValue =
            "catalog.category.processor-coolers";
        public const string ProcessorCoolerItemInstanceIdValue =
            "inventory.item.northstar-topdown-lga1700-001";
        public const string ProcessorCoolerSlotContainerIdValue =
            "inventory.container.assembly-processor-cooler";
        public const string ProcessorCoolerSlotIdValue =
            "assembly.slot.processor-cooler-main";
        public const string ProcessorCoolerBracketIdValue =
            "assembly.bracket.processor-cooler-lga1700";
        public const string ProcessorCoolerRetentionPoint1IdValue =
            "assembly.retention.processor-cooler-point-1";
        public const string ProcessorCoolerRetentionPoint2IdValue =
            "assembly.retention.processor-cooler-point-2";
        public const string ProcessorCoolerRetentionPoint3IdValue =
            "assembly.retention.processor-cooler-point-3";
        public const string ProcessorCoolerRetentionPoint4IdValue =
            "assembly.retention.processor-cooler-point-4";
        public const string ProcessorCoolerDisplayName =
            "Northstar T-90 LGA1700 Hava Soğutucu";
        public const long ProcessorCoolerUnitCostMinorUnits = 6_900;

        public StableId<ProductDefinitionIdScope> ProcessorCoolerProductId =>
            StableId<ProductDefinitionIdScope>.Parse(ProcessorCoolerProductIdValue);

        public StableId<ItemInstanceIdScope> ProcessorCoolerItemId =>
            StableId<ItemInstanceIdScope>.Parse(ProcessorCoolerItemInstanceIdValue);

        public StableId<ContainerIdScope> ProcessorCoolerSlotContainerId =>
            StableId<ContainerIdScope>.Parse(ProcessorCoolerSlotContainerIdValue);

        public StableId<AssemblySlotIdScope> ProcessorCoolerSlotId =>
            StableId<AssemblySlotIdScope>.Parse(ProcessorCoolerSlotIdValue);

        public StableId<AssemblyProcessorCoolerBracketIdScope> ProcessorCoolerBracketId =>
            StableId<AssemblyProcessorCoolerBracketIdScope>.Parse(
                ProcessorCoolerBracketIdValue);

        public bool TryGetProcessorCoolerItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(ProcessorCoolerItemId, out item);
        }

        public OperationResult PickupLooseProcessorCoolerToHands()
        {
            if (!AssemblyBuild.HasProcessorCoolerSlot ||
                AssemblyBuild.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.EmptyOpen ||
                !TryGetProcessorCoolerItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(
                ProcessorCoolerItemId,
                HandsContainerId);
        }

        public OperationResult DropHeldProcessorCoolerToWorld()
        {
            if (!AssemblyBuild.HasProcessorCoolerSlot ||
                AssemblyBuild.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.EmptyOpen ||
                !TryGetProcessorCoolerItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                ProcessorCoolerItemId,
                WorldFloorContainerId);
        }

        public OperationResult<AssemblyOperationReceipt> SeatProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            ProcessorCoolerMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SeatProcessorCooler(
                operationId,
                ProcessorCoolerItemId,
                ProcessorCoolerSlotId,
                orientation,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                sourceProcessorSeatOperationId,
                sourceProcessorRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RetainProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RetainProcessorCooler(
                operationId,
                ProcessorCoolerItemId,
                ProcessorCoolerSlotId,
                ProcessorCoolerBracketId,
                sourceProcessorCoolerSeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> UnretainProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.UnretainProcessorCooler(
                operationId,
                ProcessorCoolerItemId,
                ProcessorCoolerSlotId,
                ProcessorCoolerBracketId,
                sourceProcessorCoolerSeatOperationId,
                sourceProcessorCoolerRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RemoveProcessorCooler(
                operationId,
                ProcessorCoolerItemId,
                ProcessorCoolerSlotId,
                sourceProcessorCoolerSeatOperationId,
                expectedAssemblyRevision);
        }
    }
}
