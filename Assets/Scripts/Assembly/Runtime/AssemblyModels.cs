using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public enum AssemblySeatState
    {
        Empty = 1,
        SeatedUnsecured = 2
    }

    public enum AssemblyOperationKind
    {
        AttachMotherboard = 1,
        DetachMotherboard = 2
    }

    /// <summary>
    /// Immutable result of one exact attach or detach command.
    /// </summary>
    public sealed class AssemblyOperationReceipt
    {
        internal AssemblyOperationReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationKind operationKind,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            AssemblySeatState resultingSeatState,
            long assemblyRevision,
            long inventoryRevision)
        {
            OperationId = operationId;
            OperationKind = operationKind;
            BuildId = buildId;
            ChassisId = chassisId;
            SlotId = slotId;
            ItemId = itemId;
            ProductId = productId;
            SourceContainerId = sourceContainerId;
            TargetContainerId = targetContainerId;
            SourceAttachOperationId = sourceAttachOperationId;
            ResultingSeatState = resultingSeatState;
            AssemblyRevision = assemblyRevision;
            InventoryRevision = inventoryRevision;
        }

        public StableId<AssemblyOperationIdScope> OperationId { get; }

        public AssemblyOperationKind OperationKind { get; }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<AssemblySlotIdScope> SlotId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> SourceContainerId { get; }

        public StableId<ContainerIdScope> TargetContainerId { get; }

        public StableId<AssemblyOperationIdScope> SourceAttachOperationId { get; }

        public AssemblySeatState ResultingSeatState { get; }

        public long AssemblyRevision { get; }

        public long InventoryRevision { get; }

        internal bool MatchesAttach(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId)
        {
            return OperationKind == AssemblyOperationKind.AttachMotherboard &&
                   ItemId == itemId &&
                   SlotId == slotId;
        }

        internal bool MatchesDetach(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId)
        {
            return OperationKind == AssemblyOperationKind.DetachMotherboard &&
                   ItemId == itemId &&
                   SlotId == slotId;
        }
    }

    /// <summary>
    /// Read-only projection of the authoritative single-slot build state.
    /// </summary>
    public sealed class AssemblyBuildSnapshot
    {
        internal AssemblyBuildSnapshot(
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> workbenchContainerId,
            MotherboardFormFactor supportedMotherboardFormFactor,
            AssemblySeatState motherboardSeatState,
            StableId<ItemInstanceIdScope> motherboardItemId,
            StableId<ProductDefinitionIdScope> motherboardProductId,
            StableId<AssemblyOperationIdScope> installedByOperationId,
            long revision)
        {
            BuildId = buildId;
            ChassisId = chassisId;
            MotherboardSlotId = motherboardSlotId;
            HandsContainerId = handsContainerId;
            WorkbenchContainerId = workbenchContainerId;
            SupportedMotherboardFormFactor = supportedMotherboardFormFactor;
            MotherboardSeatState = motherboardSeatState;
            MotherboardItemId = motherboardItemId;
            MotherboardProductId = motherboardProductId;
            InstalledByOperationId = installedByOperationId;
            Revision = revision;
        }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<AssemblySlotIdScope> MotherboardSlotId { get; }

        public StableId<ContainerIdScope> HandsContainerId { get; }

        public StableId<ContainerIdScope> WorkbenchContainerId { get; }

        public MotherboardFormFactor SupportedMotherboardFormFactor { get; }

        public AssemblySeatState MotherboardSeatState { get; }

        public StableId<ItemInstanceIdScope> MotherboardItemId { get; }

        public StableId<ProductDefinitionIdScope> MotherboardProductId { get; }

        public StableId<AssemblyOperationIdScope> InstalledByOperationId { get; }

        public long Revision { get; }
    }

    public static class AssemblyFailures
    {
        public static readonly Failure MissingComponentCatalog =
            Failure.FromCode("assembly.component-catalog.missing");
        public static readonly Failure MissingInventoryAuthority =
            Failure.FromCode("assembly.inventory.missing");
        public static readonly Failure InvalidBuildId = Failure.FromCode("assembly.build-id.invalid");
        public static readonly Failure InvalidChassisId = Failure.FromCode("assembly.chassis-id.invalid");
        public static readonly Failure InvalidSlotId = Failure.FromCode("assembly.slot-id.invalid");
        public static readonly Failure InvalidOperationId = Failure.FromCode("assembly.operation-id.invalid");
        public static readonly Failure InvalidHandsContainer =
            Failure.FromCode("assembly.hands-container.invalid");
        public static readonly Failure InvalidWorkbenchContainer =
            Failure.FromCode("assembly.workbench-container.invalid");
        public static readonly Failure SameInventoryContainer =
            Failure.FromCode("assembly.inventory-container.same");
        public static readonly Failure InvalidMotherboardFormFactor =
            Failure.FromCode("assembly.motherboard-form-factor.invalid");
        public static readonly Failure UnknownSlot = Failure.FromCode("assembly.slot.unknown");
        public static readonly Failure OperationConflict =
            Failure.FromCode("assembly.operation.conflict");
        public static readonly Failure RevisionOverflow = Failure.FromCode("assembly.revision-overflow");
        public static readonly Failure SlotOccupied = Failure.FromCode("assembly.slot.occupied");
        public static readonly Failure SlotEmpty = Failure.FromCode("assembly.slot.empty");
        public static readonly Failure UnknownItem = Failure.FromCode("assembly.item.unknown");
        public static readonly Failure ItemNotInActorHands =
            Failure.FromCode("assembly.item.not-in-actor-hands");
        public static readonly Failure ItemNotOnWorkbench =
            Failure.FromCode("assembly.item.not-on-workbench");
        public static readonly Failure UnknownComponentSpecification =
            Failure.FromCode("assembly.component-specification.unknown");
        public static readonly Failure UnsupportedComponentKind =
            Failure.FromCode("assembly.component-kind.unsupported");
        public static readonly Failure MotherboardFormFactorMismatch =
            Failure.FromCode("assembly.motherboard-form-factor.mismatch");
        public static readonly Failure WorkbenchCapacityExceeded =
            Failure.FromCode("assembly.workbench.capacity");
        public static readonly Failure HandsCapacityExceeded =
            Failure.FromCode("assembly.hands.capacity");
        public static readonly Failure InventoryRevisionOverflow =
            Failure.FromCode("assembly.inventory.revision-overflow");
        public static readonly Failure InventoryTransferStale =
            Failure.FromCode("assembly.inventory-transfer.stale");
        public static readonly Failure InventoryTransferRejected =
            Failure.FromCode("assembly.inventory-transfer.rejected");
        public static readonly Failure MotherboardMissing =
            Failure.FromCode("assembly.benchmark.motherboard-missing");
        public static readonly Failure MotherboardUnsecured =
            Failure.FromCode("assembly.benchmark.motherboard-unsecured");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("assembly.invariant.failed");
    }
}
