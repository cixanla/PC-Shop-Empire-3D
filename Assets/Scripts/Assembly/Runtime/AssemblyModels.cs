using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public enum AssemblySeatState
    {
        Empty = 1,
        SeatedUnsecured = 2,
        SeatedSecured = 3
    }

    public enum AssemblyOperationKind
    {
        AttachMotherboard = 1,
        DetachMotherboard = 2,
        SecureMotherboardFastener = 3,
        UnsecureMotherboardFastener = 4,
        SeatProcessor = 5,
        RemoveProcessor = 6,
        CloseProcessorRetention = 7,
        OpenProcessorRetention = 8
    }

    public enum ProcessorSocketState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        ProcessorSeatedOpen = 2,
        ProcessorRetained = 3
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
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            int sequenceIndex,
            long expectedAssemblyRevision,
            AssemblySeatState previousSeatState,
            AssemblySeatState resultingSeatState,
            long assemblyRevision,
            long inventoryRevision)
            : this(
                operationId,
                operationKind,
                buildId,
                chassisId,
                slotId,
                itemId,
                productId,
                sourceContainerId,
                targetContainerId,
                sourceAttachOperationId,
                sourceSecureOperationId,
                fastenerId,
                default,
                default,
                default,
                sequenceIndex,
                expectedAssemblyRevision,
                previousSeatState,
                resultingSeatState,
                ProcessorSocketState.Unsupported,
                ProcessorSocketState.Unsupported,
                assemblyRevision,
                inventoryRevision)
        {
        }

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
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            int sequenceIndex,
            long expectedAssemblyRevision,
            AssemblySeatState previousSeatState,
            AssemblySeatState resultingSeatState,
            long assemblyRevision,
            long inventoryRevision,
            ProcessorSocketState processorSocketState)
            : this(
                operationId,
                operationKind,
                buildId,
                chassisId,
                slotId,
                itemId,
                productId,
                sourceContainerId,
                targetContainerId,
                sourceAttachOperationId,
                sourceSecureOperationId,
                fastenerId,
                default,
                default,
                default,
                sequenceIndex,
                expectedAssemblyRevision,
                previousSeatState,
                resultingSeatState,
                processorSocketState,
                processorSocketState,
                assemblyRevision,
                inventoryRevision)
        {
        }

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
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            int sequenceIndex,
            long expectedAssemblyRevision,
            AssemblySeatState previousSeatState,
            AssemblySeatState resultingSeatState,
            ProcessorSocketState previousProcessorSocketState,
            ProcessorSocketState resultingProcessorSocketState,
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
            SourceSecureOperationId = sourceSecureOperationId;
            FastenerId = fastenerId;
            RetentionId = retentionId;
            SourceProcessorSeatOperationId = sourceProcessorSeatOperationId;
            SourceProcessorRetentionOperationId = sourceProcessorRetentionOperationId;
            SequenceIndex = sequenceIndex;
            ExpectedAssemblyRevision = expectedAssemblyRevision;
            PreviousSeatState = previousSeatState;
            ResultingSeatState = resultingSeatState;
            PreviousProcessorSocketState = previousProcessorSocketState;
            ResultingProcessorSocketState = resultingProcessorSocketState;
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

        public StableId<AssemblyOperationIdScope> SourceSecureOperationId { get; }

        public StableId<AssemblyFastenerIdScope> FastenerId { get; }

        public StableId<AssemblyRetentionIdScope> RetentionId { get; }

        public StableId<AssemblyOperationIdScope> SourceProcessorSeatOperationId { get; }

        public StableId<AssemblyOperationIdScope> SourceProcessorRetentionOperationId { get; }

        public int SequenceIndex { get; }

        public long ExpectedAssemblyRevision { get; }

        public AssemblySeatState PreviousSeatState { get; }

        public AssemblySeatState ResultingSeatState { get; }

        public ProcessorSocketState PreviousProcessorSocketState { get; }

        public ProcessorSocketState ResultingProcessorSocketState { get; }

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

        internal bool MatchesSecure(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SecureMotherboardFastener &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   FastenerId == fastenerId &&
                   SourceAttachOperationId == sourceAttachOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesUnsecure(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.UnsecureMotherboardFastener &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   FastenerId == fastenerId &&
                   SourceAttachOperationId == sourceAttachOperationId &&
                   SourceSecureOperationId == sourceSecureOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesSeatProcessor(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SeatProcessor &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   SourceAttachOperationId == sourceAttachOperationId &&
                   SourceSecureOperationId == sourceSecureOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRemoveProcessor(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RemoveProcessor &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   SourceProcessorSeatOperationId == sourceProcessorSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesCloseProcessorRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.CloseProcessorRetention &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   RetentionId == retentionId &&
                   SourceProcessorSeatOperationId == sourceProcessorSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesOpenProcessorRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.OpenProcessorRetention &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   RetentionId == retentionId &&
                   SourceProcessorSeatOperationId == sourceProcessorSeatOperationId &&
                   SourceProcessorRetentionOperationId ==
                       sourceProcessorRetentionOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
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
            StableId<AssemblyFastenerIdScope> motherboardFastenerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> workbenchContainerId,
            MotherboardFormFactor supportedMotherboardFormFactor,
            AssemblySeatState motherboardSeatState,
            StableId<ItemInstanceIdScope> motherboardItemId,
            StableId<ProductDefinitionIdScope> motherboardProductId,
            StableId<AssemblyOperationIdScope> installedByOperationId,
            StableId<AssemblyOperationIdScope> securedByOperationId,
            long revision)
            : this(
                buildId,
                chassisId,
                motherboardSlotId,
                motherboardFastenerId,
                handsContainerId,
                workbenchContainerId,
                default,
                default,
                default,
                supportedMotherboardFormFactor,
                default,
                motherboardSeatState,
                motherboardItemId,
                motherboardProductId,
                installedByOperationId,
                securedByOperationId,
                ProcessorSocketState.Unsupported,
                default,
                default,
                default,
                default,
                revision)
        {
        }

        internal AssemblyBuildSnapshot(
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
            StableId<AssemblyFastenerIdScope> motherboardFastenerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> workbenchContainerId,
            StableId<AssemblySlotIdScope> processorSlotId,
            StableId<AssemblyRetentionIdScope> processorRetentionId,
            StableId<ContainerIdScope> processorSocketContainerId,
            MotherboardFormFactor supportedMotherboardFormFactor,
            CpuSocketFamily supportedCpuSocketFamily,
            AssemblySeatState motherboardSeatState,
            StableId<ItemInstanceIdScope> motherboardItemId,
            StableId<ProductDefinitionIdScope> motherboardProductId,
            StableId<AssemblyOperationIdScope> installedByOperationId,
            StableId<AssemblyOperationIdScope> securedByOperationId,
            ProcessorSocketState processorSocketState,
            StableId<ItemInstanceIdScope> processorItemId,
            StableId<ProductDefinitionIdScope> processorProductId,
            StableId<AssemblyOperationIdScope> processorSeatedByOperationId,
            StableId<AssemblyOperationIdScope> processorRetainedByOperationId,
            long revision)
        {
            BuildId = buildId;
            ChassisId = chassisId;
            MotherboardSlotId = motherboardSlotId;
            MotherboardFastenerId = motherboardFastenerId;
            HandsContainerId = handsContainerId;
            WorkbenchContainerId = workbenchContainerId;
            ProcessorSlotId = processorSlotId;
            ProcessorRetentionId = processorRetentionId;
            ProcessorSocketContainerId = processorSocketContainerId;
            SupportedMotherboardFormFactor = supportedMotherboardFormFactor;
            SupportedCpuSocketFamily = supportedCpuSocketFamily;
            MotherboardSeatState = motherboardSeatState;
            MotherboardItemId = motherboardItemId;
            MotherboardProductId = motherboardProductId;
            InstalledByOperationId = installedByOperationId;
            SecuredByOperationId = securedByOperationId;
            ProcessorSocketState = processorSocketState;
            ProcessorItemId = processorItemId;
            ProcessorProductId = processorProductId;
            ProcessorSeatedByOperationId = processorSeatedByOperationId;
            ProcessorRetainedByOperationId = processorRetainedByOperationId;
            Revision = revision;
        }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<AssemblySlotIdScope> MotherboardSlotId { get; }

        public StableId<AssemblyFastenerIdScope> MotherboardFastenerId { get; }

        public StableId<ContainerIdScope> HandsContainerId { get; }

        public StableId<ContainerIdScope> WorkbenchContainerId { get; }

        public StableId<AssemblySlotIdScope> ProcessorSlotId { get; }

        public StableId<AssemblyRetentionIdScope> ProcessorRetentionId { get; }

        public StableId<ContainerIdScope> ProcessorSocketContainerId { get; }

        public MotherboardFormFactor SupportedMotherboardFormFactor { get; }

        public CpuSocketFamily SupportedCpuSocketFamily { get; }

        public AssemblySeatState MotherboardSeatState { get; }

        public StableId<ItemInstanceIdScope> MotherboardItemId { get; }

        public StableId<ProductDefinitionIdScope> MotherboardProductId { get; }

        public StableId<AssemblyOperationIdScope> InstalledByOperationId { get; }

        public StableId<AssemblyOperationIdScope> SecuredByOperationId { get; }

        public ProcessorSocketState ProcessorSocketState { get; }

        public StableId<ItemInstanceIdScope> ProcessorItemId { get; }

        public StableId<ProductDefinitionIdScope> ProcessorProductId { get; }

        public StableId<AssemblyOperationIdScope> ProcessorSeatedByOperationId { get; }

        public StableId<AssemblyOperationIdScope> ProcessorRetainedByOperationId { get; }

        public bool HasProcessorSocket => !ProcessorSlotId.IsEmpty;

        public long Revision { get; }
    }

    public static class AssemblyFailures
    {
        public static readonly Failure MissingComponentCatalog =
            Failure.FromCode("assembly.component-catalog.missing");
        public static readonly Failure MissingInventoryAuthority =
            Failure.FromCode("assembly.inventory.missing");
        public static readonly Failure CatalogAuthorityMismatch =
            Failure.FromCode("assembly.catalog-authority.mismatch");
        public static readonly Failure InvalidBuildId = Failure.FromCode("assembly.invalid-build");
        public static readonly Failure InvalidChassisId = Failure.FromCode("assembly.invalid-chassis");
        public static readonly Failure InvalidSlotId = Failure.FromCode("assembly.invalid-slot");
        public static readonly Failure InvalidFastener =
            Failure.FromCode("assembly.invalid-fastener");
        public static readonly Failure InvalidRetention =
            Failure.FromCode("assembly.retention.invalid");
        public static readonly Failure InvalidOperationId = Failure.FromCode("assembly.operation-id.invalid");
        public static readonly Failure InvalidHandsContainer =
            Failure.FromCode("assembly.hands-container.invalid");
        public static readonly Failure InvalidWorkbenchContainer =
            Failure.FromCode("assembly.workbench-container.invalid");
        public static readonly Failure InvalidProcessorSocketContainer =
            Failure.FromCode("assembly.processor-socket-container.invalid");
        public static readonly Failure SameInventoryContainer =
            Failure.FromCode("assembly.inventory-container.same");
        public static readonly Failure InvalidMotherboardFormFactor =
            Failure.FromCode("assembly.motherboard-form-factor.invalid");
        public static readonly Failure InvalidCpuSocketFamily =
            Failure.FromCode("assembly.cpu-socket-family.invalid");
        public static readonly Failure ProcessorSocketUnavailable =
            Failure.FromCode("assembly.processor-socket.unavailable");
        public static readonly Failure UnknownSlot = InvalidSlotId;
        public static readonly Failure IdentityConflict =
            Failure.FromCode("assembly.identity-conflict");
        public static readonly Failure OperationConflict = IdentityConflict;
        public static readonly Failure RevisionOverflow = Failure.FromCode("assembly.revision-overflow");
        public static readonly Failure SlotOccupied = Failure.FromCode("assembly.slot-occupied");
        public static readonly Failure ProcessorSocketOccupied =
            Failure.FromCode("assembly.processor-socket.occupied");
        public static readonly Failure InvalidComponent = Failure.FromCode("assembly.invalid-component");
        public static readonly Failure UnknownItem = InvalidComponent;
        public static readonly Failure ComponentNotInActorHands =
            Failure.FromCode("assembly.component-not-in-hands");
        public static readonly Failure ItemNotInActorHands = ComponentNotInActorHands;
        public static readonly Failure ComponentNotSeated =
            Failure.FromCode("assembly.component-not-seated");
        public static readonly Failure ComponentSecured =
            Failure.FromCode("assembly.component-secured");
        public static readonly Failure FastenerOutOfOrder =
            Failure.FromCode("assembly.fastener-out-of-order");
        public static readonly Failure ProcessorRetentionOutOfOrder =
            Failure.FromCode("assembly.processor-retention.out-of-order");
        public static readonly Failure ProcessorRetained =
            Failure.FromCode("assembly.processor-retained");
        public static readonly Failure ProcessorInstalled =
            Failure.FromCode("assembly.motherboard.processor-installed");
        public static readonly Failure SlotEmpty = ComponentNotSeated;
        public static readonly Failure ItemNotOnWorkbench = ComponentNotSeated;
        public static readonly Failure UnknownComponentSpecification = InvalidComponent;
        public static readonly Failure ComponentKindMismatch =
            Failure.FromCode("assembly.component-kind-mismatch");
        public static readonly Failure UnsupportedComponentKind = ComponentKindMismatch;
        public static readonly Failure FormFactorMismatch =
            Failure.FromCode("assembly.form-factor-mismatch");
        public static readonly Failure MotherboardFormFactorMismatch = FormFactorMismatch;
        public static readonly Failure CpuSocketFamilyMismatch =
            Failure.FromCode("assembly.cpu-socket-family.mismatch");
        public static readonly Failure WorkbenchCapacityExceeded =
            Failure.FromCode("assembly.workbench.capacity");
        public static readonly Failure ProcessorSocketCapacityExceeded =
            Failure.FromCode("assembly.processor-socket.capacity");
        public static readonly Failure HandsCapacityExceeded =
            Failure.FromCode("assembly.hands.capacity");
        public static readonly Failure InventoryRevisionOverflow = RevisionOverflow;
        public static readonly Failure PlanStale = Failure.FromCode("assembly.plan-stale");
        public static readonly Failure InventoryTransferStale = PlanStale;
        public static readonly Failure InventoryTransferRejected =
            Failure.FromCode("assembly.inventory-transfer.rejected");
        public static readonly Failure PlanForeign = Failure.FromCode("assembly.plan-foreign");
        public static readonly Failure MotherboardMissing =
            Failure.FromCode("assembly.benchmark.motherboard-missing");
        public static readonly Failure MotherboardUnsecured =
            Failure.FromCode("assembly.benchmark.motherboard-unsecured");
        public static readonly Failure ProcessorMissing =
            Failure.FromCode("assembly.benchmark.processor-missing");
        public static readonly Failure ProcessorUnretained =
            Failure.FromCode("assembly.benchmark.processor-unretained");
        public static readonly Failure BuildIncomplete =
            Failure.FromCode("assembly.benchmark.build-incomplete");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("assembly.invariant.failed");
    }
}
