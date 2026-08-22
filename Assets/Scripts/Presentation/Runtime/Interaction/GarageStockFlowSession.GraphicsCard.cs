using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

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

        public OperationResult DropHeldGraphicsCardToWorld()
        {
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
    }
}
