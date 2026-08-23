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
        OpenProcessorRetention = 8,
        SeatMemoryModule = 9,
        RemoveMemoryModule = 10,
        CloseMemoryRetention = 11,
        OpenMemoryRetention = 12,
        SeatStorageDevice = 13,
        RemoveStorageDevice = 14,
        SecureStorageDevice = 15,
        UnsecureStorageDevice = 16,
        SeatProcessorCooler = 17,
        RemoveProcessorCooler = 18,
        RetainProcessorCooler = 19,
        UnretainProcessorCooler = 20,
        SeatGraphicsCard = 21,
        RemoveGraphicsCard = 22,
        RetainGraphicsCard = 23,
        UnretainGraphicsCard = 24,
        SeatPowerSupply = 25,
        RemovePowerSupply = 26,
        RetainPowerSupply = 27,
        UnretainPowerSupply = 28
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
            ProcessorSocketState processorSocketState,
            MemorySlotState memorySlotState = MemorySlotState.Unsupported,
            StorageSlotState storageSlotState = StorageSlotState.Unsupported,
            ProcessorCoolerSlotState processorCoolerSlotState =
                ProcessorCoolerSlotState.Unsupported,
            ProcessorCoolerTimState processorCoolerTimState =
                ProcessorCoolerTimState.Unsupported)
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
                inventoryRevision,
                memorySlotState,
                memorySlotState,
                default,
                default,
                default,
                storageSlotState,
                storageSlotState,
                default,
                default,
                default,
                processorCoolerSlotState,
                processorCoolerSlotState,
                default,
                default,
                default,
                processorCoolerTimState,
                processorCoolerTimState)
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
            long inventoryRevision,
            MemorySlotState previousMemorySlotState = MemorySlotState.Unsupported,
            MemorySlotState resultingMemorySlotState = MemorySlotState.Unsupported,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId = default,
            StableId<AssemblyOperationIdScope> sourceMemoryRetentionOperationId = default,
            DimmKeyOrientation dimmKeyOrientation = default,
            StorageSlotState previousStorageSlotState = StorageSlotState.Unsupported,
            StorageSlotState resultingStorageSlotState = StorageSlotState.Unsupported,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId = default,
            StableId<AssemblyOperationIdScope> sourceStorageRetentionOperationId = default,
            M2KeyOrientation m2KeyOrientation = default,
            ProcessorCoolerSlotState previousProcessorCoolerSlotState =
                ProcessorCoolerSlotState.Unsupported,
            ProcessorCoolerSlotState resultingProcessorCoolerSlotState =
                ProcessorCoolerSlotState.Unsupported,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId =
                default,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerRetentionOperationId =
                default,
            ProcessorCoolerMountOrientation processorCoolerMountOrientation = default,
            ProcessorCoolerTimState previousProcessorCoolerTimState =
                ProcessorCoolerTimState.Unsupported,
            ProcessorCoolerTimState resultingProcessorCoolerTimState =
                ProcessorCoolerTimState.Unsupported,
            ProcessorCoolerSlotDefinition processorCoolerSlotDefinition = default,
            GraphicsCardSlotState previousGraphicsCardSlotState =
                GraphicsCardSlotState.Unsupported,
            GraphicsCardSlotState resultingGraphicsCardSlotState =
                GraphicsCardSlotState.Unsupported,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId =
                default,
            StableId<AssemblyOperationIdScope>
                sourceGraphicsCardRetentionOperationId = default,
            GraphicsCardMountOrientation graphicsCardMountOrientation = default,
            GraphicsCardSlotDefinition graphicsCardSlotDefinition = default,
            PowerSupplyBayState previousPowerSupplyBayState =
                PowerSupplyBayState.Unsupported,
            PowerSupplyBayState resultingPowerSupplyBayState =
                PowerSupplyBayState.Unsupported,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId =
                default,
            StableId<AssemblyOperationIdScope>
                sourcePowerSupplyRetentionOperationId = default,
            PowerSupplyMountOrientation powerSupplyMountOrientation = default,
            PowerSupplyBayDefinition powerSupplyBayDefinition = default)
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
            PreviousMemorySlotState = previousMemorySlotState;
            ResultingMemorySlotState = resultingMemorySlotState;
            SourceMemorySeatOperationId = sourceMemorySeatOperationId;
            SourceMemoryRetentionOperationId = sourceMemoryRetentionOperationId;
            DimmKeyOrientation = dimmKeyOrientation;
            PreviousStorageSlotState = previousStorageSlotState;
            ResultingStorageSlotState = resultingStorageSlotState;
            SourceStorageSeatOperationId = sourceStorageSeatOperationId;
            SourceStorageRetentionOperationId = sourceStorageRetentionOperationId;
            M2KeyOrientation = m2KeyOrientation;
            PreviousProcessorCoolerSlotState = previousProcessorCoolerSlotState;
            ResultingProcessorCoolerSlotState = resultingProcessorCoolerSlotState;
            SourceProcessorCoolerSeatOperationId =
                sourceProcessorCoolerSeatOperationId;
            SourceProcessorCoolerRetentionOperationId =
                sourceProcessorCoolerRetentionOperationId;
            ProcessorCoolerMountOrientation = processorCoolerMountOrientation;
            PreviousProcessorCoolerTimState = previousProcessorCoolerTimState;
            ResultingProcessorCoolerTimState = resultingProcessorCoolerTimState;
            ProcessorCoolerSlotDefinition = processorCoolerSlotDefinition;
            PreviousGraphicsCardSlotState = previousGraphicsCardSlotState;
            ResultingGraphicsCardSlotState = resultingGraphicsCardSlotState;
            SourceGraphicsCardSeatOperationId = sourceGraphicsCardSeatOperationId;
            SourceGraphicsCardRetentionOperationId =
                sourceGraphicsCardRetentionOperationId;
            GraphicsCardMountOrientation = graphicsCardMountOrientation;
            GraphicsCardSlotDefinition = graphicsCardSlotDefinition;
            PreviousPowerSupplyBayState = previousPowerSupplyBayState;
            ResultingPowerSupplyBayState = resultingPowerSupplyBayState;
            SourcePowerSupplySeatOperationId = sourcePowerSupplySeatOperationId;
            SourcePowerSupplyRetentionOperationId =
                sourcePowerSupplyRetentionOperationId;
            PowerSupplyMountOrientation = powerSupplyMountOrientation;
            PowerSupplyBayDefinition = powerSupplyBayDefinition;
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

        public MemorySlotState PreviousMemorySlotState { get; }

        public MemorySlotState ResultingMemorySlotState { get; }

        public StableId<AssemblyOperationIdScope> SourceMemorySeatOperationId { get; }

        public StableId<AssemblyOperationIdScope> SourceMemoryRetentionOperationId { get; }

        public DimmKeyOrientation DimmKeyOrientation { get; }

        public StorageSlotState PreviousStorageSlotState { get; }

        public StorageSlotState ResultingStorageSlotState { get; }

        public StableId<AssemblyOperationIdScope> SourceStorageSeatOperationId { get; }

        public StableId<AssemblyOperationIdScope> SourceStorageRetentionOperationId { get; }

        public M2KeyOrientation M2KeyOrientation { get; }

        public ProcessorCoolerSlotState PreviousProcessorCoolerSlotState { get; }

        public ProcessorCoolerSlotState ResultingProcessorCoolerSlotState { get; }

        public StableId<AssemblyOperationIdScope> SourceProcessorCoolerSeatOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope>
            SourceProcessorCoolerRetentionOperationId
        {
            get;
        }

        public ProcessorCoolerMountOrientation ProcessorCoolerMountOrientation { get; }

        public ProcessorCoolerTimState PreviousProcessorCoolerTimState { get; }

        public ProcessorCoolerTimState ResultingProcessorCoolerTimState { get; }

        public ProcessorCoolerSlotDefinition ProcessorCoolerSlotDefinition { get; }

        public GraphicsCardSlotState PreviousGraphicsCardSlotState { get; }

        public GraphicsCardSlotState ResultingGraphicsCardSlotState { get; }

        public StableId<AssemblyOperationIdScope> SourceGraphicsCardSeatOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope>
            SourceGraphicsCardRetentionOperationId
        {
            get;
        }

        public GraphicsCardMountOrientation GraphicsCardMountOrientation { get; }

        public GraphicsCardSlotDefinition GraphicsCardSlotDefinition { get; }

        public PowerSupplyBayState PreviousPowerSupplyBayState { get; }

        public PowerSupplyBayState ResultingPowerSupplyBayState { get; }

        public StableId<AssemblyOperationIdScope> SourcePowerSupplySeatOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope>
            SourcePowerSupplyRetentionOperationId
        {
            get;
        }

        public PowerSupplyMountOrientation PowerSupplyMountOrientation { get; }

        public PowerSupplyBayDefinition PowerSupplyBayDefinition { get; }

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

        internal bool MatchesSeatMemoryModule(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            DimmKeyOrientation dimmKeyOrientation,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SeatMemoryModule &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   DimmKeyOrientation == dimmKeyOrientation &&
                   SourceAttachOperationId == sourceAttachOperationId &&
                   SourceSecureOperationId == sourceSecureOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRemoveMemoryModule(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RemoveMemoryModule &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   SourceMemorySeatOperationId == sourceMemorySeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesCloseMemoryRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.CloseMemoryRetention &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   RetentionId == retentionId &&
                   SourceMemorySeatOperationId == sourceMemorySeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesOpenMemoryRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            StableId<AssemblyOperationIdScope> sourceMemoryRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.OpenMemoryRetention &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   RetentionId == retentionId &&
                   SourceMemorySeatOperationId == sourceMemorySeatOperationId &&
                   SourceMemoryRetentionOperationId ==
                       sourceMemoryRetentionOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesSeatStorageDevice(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            M2KeyOrientation m2KeyOrientation,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SeatStorageDevice &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   M2KeyOrientation == m2KeyOrientation &&
                   SourceAttachOperationId == sourceAttachOperationId &&
                   SourceSecureOperationId == sourceSecureOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRemoveStorageDevice(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RemoveStorageDevice &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   SourceStorageSeatOperationId == sourceStorageSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesSecureStorageDevice(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SecureStorageDevice &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   RetentionId == retentionId &&
                   SourceStorageSeatOperationId == sourceStorageSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesUnsecureStorageDevice(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceStorageRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.UnsecureStorageDevice &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   RetentionId == retentionId &&
                   SourceStorageSeatOperationId == sourceStorageSeatOperationId &&
                   SourceStorageRetentionOperationId ==
                       sourceStorageRetentionOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesSeatProcessorCooler(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            ProcessorCoolerMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SeatProcessorCooler &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   ProcessorCoolerMountOrientation == orientation &&
                   SourceAttachOperationId == sourceAttachOperationId &&
                   SourceSecureOperationId == sourceSecureOperationId &&
                   SourceProcessorSeatOperationId == sourceProcessorSeatOperationId &&
                   SourceProcessorRetentionOperationId ==
                       sourceProcessorRetentionOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRemoveProcessorCooler(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RemoveProcessorCooler &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   SourceProcessorCoolerSeatOperationId ==
                       sourceProcessorCoolerSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRetainProcessorCooler(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyProcessorCoolerBracketIdScope> bracketId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RetainProcessorCooler &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   ProcessorCoolerSlotDefinition.BracketId == bracketId &&
                   SourceProcessorCoolerSeatOperationId ==
                       sourceProcessorCoolerSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesUnretainProcessorCooler(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyProcessorCoolerBracketIdScope> bracketId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceProcessorCoolerRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.UnretainProcessorCooler &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   ProcessorCoolerSlotDefinition.BracketId == bracketId &&
                   SourceProcessorCoolerSeatOperationId ==
                       sourceProcessorCoolerSeatOperationId &&
                   SourceProcessorCoolerRetentionOperationId ==
                       sourceProcessorCoolerRetentionOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesSeatGraphicsCard(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            GraphicsCardMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SeatGraphicsCard &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   GraphicsCardMountOrientation == orientation &&
                   SourceAttachOperationId == sourceAttachOperationId &&
                   SourceSecureOperationId == sourceSecureOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRemoveGraphicsCard(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RemoveGraphicsCard &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   SourceGraphicsCardSeatOperationId ==
                       sourceGraphicsCardSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRetainGraphicsCard(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyGraphicsCardLatchIdScope> latchId,
            StableId<AssemblyFastenerIdScope> bracketFastenerId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RetainGraphicsCard &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   GraphicsCardSlotDefinition.IsValid &&
                   GraphicsCardSlotDefinition.RetentionTopology.LatchId == latchId &&
                   GraphicsCardSlotDefinition.RetentionTopology.BracketFastenerId ==
                       bracketFastenerId &&
                   SourceGraphicsCardSeatOperationId ==
                       sourceGraphicsCardSeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesUnretainGraphicsCard(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyGraphicsCardLatchIdScope> latchId,
            StableId<AssemblyFastenerIdScope> bracketFastenerId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceGraphicsCardRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.UnretainGraphicsCard &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   GraphicsCardSlotDefinition.IsValid &&
                   GraphicsCardSlotDefinition.RetentionTopology.LatchId == latchId &&
                   GraphicsCardSlotDefinition.RetentionTopology.BracketFastenerId ==
                       bracketFastenerId &&
                   SourceGraphicsCardSeatOperationId ==
                       sourceGraphicsCardSeatOperationId &&
                   SourceGraphicsCardRetentionOperationId ==
                       sourceGraphicsCardRetentionOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesSeatPowerSupply(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            PowerSupplyMountOrientation orientation,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.SeatPowerSupply &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   PowerSupplyMountOrientation == orientation &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRemovePowerSupply(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            return OperationKind == AssemblyOperationKind.RemovePowerSupply &&
                   ItemId == itemId &&
                   SlotId == slotId &&
                   SourcePowerSupplySeatOperationId ==
                       sourcePowerSupplySeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesRetainPowerSupply(
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
            return OperationKind == AssemblyOperationKind.RetainPowerSupply &&
                   MatchesPowerSupplyTopology(
                       itemId,
                       slotId,
                       rearMountId,
                       topLeftFastenerId,
                       topRightFastenerId,
                       bottomLeftFastenerId,
                       bottomRightFastenerId) &&
                   SourcePowerSupplySeatOperationId ==
                       sourcePowerSupplySeatOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        internal bool MatchesUnretainPowerSupply(
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
            return OperationKind == AssemblyOperationKind.UnretainPowerSupply &&
                   MatchesPowerSupplyTopology(
                       itemId,
                       slotId,
                       rearMountId,
                       topLeftFastenerId,
                       topRightFastenerId,
                       bottomLeftFastenerId,
                       bottomRightFastenerId) &&
                   SourcePowerSupplySeatOperationId ==
                       sourcePowerSupplySeatOperationId &&
                   SourcePowerSupplyRetentionOperationId ==
                       sourcePowerSupplyRetentionOperationId &&
                   ExpectedAssemblyRevision == expectedAssemblyRevision;
        }

        private bool MatchesPowerSupplyTopology(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyPowerSupplyRearMountIdScope> rearMountId,
            StableId<AssemblyFastenerIdScope> topLeftFastenerId,
            StableId<AssemblyFastenerIdScope> topRightFastenerId,
            StableId<AssemblyFastenerIdScope> bottomLeftFastenerId,
            StableId<AssemblyFastenerIdScope> bottomRightFastenerId)
        {
            PowerSupplyRetentionTopology topology =
                PowerSupplyBayDefinition.RetentionTopology;
            return ItemId == itemId &&
                   SlotId == slotId &&
                   PowerSupplyBayDefinition.IsValid &&
                   topology.RearMountId == rearMountId &&
                   topology.TopLeftFastenerId == topLeftFastenerId &&
                   topology.TopRightFastenerId == topRightFastenerId &&
                   topology.BottomLeftFastenerId == bottomLeftFastenerId &&
                   topology.BottomRightFastenerId == bottomRightFastenerId;
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
            long revision,
            DimmSlotDefinition memorySlotDefinition = default,
            MemorySlotState memorySlotState = MemorySlotState.Unsupported,
            StableId<ItemInstanceIdScope> memoryItemId = default,
            StableId<ProductDefinitionIdScope> memoryProductId = default,
            StableId<AssemblyOperationIdScope> memorySeatedByOperationId = default,
            StableId<AssemblyOperationIdScope> memoryRetainedByOperationId = default,
            M2SlotDefinition storageSlotDefinition = default,
            StorageSlotState storageSlotState = StorageSlotState.Unsupported,
            StableId<ItemInstanceIdScope> storageItemId = default,
            StableId<ProductDefinitionIdScope> storageProductId = default,
            StableId<AssemblyOperationIdScope> storageSeatedByOperationId = default,
            StableId<AssemblyOperationIdScope> storageSecuredByOperationId = default,
            ProcessorCoolerSlotDefinition processorCoolerSlotDefinition = default,
            ProcessorCoolerSlotState processorCoolerSlotState =
                ProcessorCoolerSlotState.Unsupported,
            StableId<ItemInstanceIdScope> processorCoolerItemId = default,
            StableId<ProductDefinitionIdScope> processorCoolerProductId = default,
            StableId<AssemblyOperationIdScope> processorCoolerSeatedByOperationId =
                default,
            StableId<AssemblyOperationIdScope> processorCoolerRetainedByOperationId =
                default,
            ProcessorCoolerMountOrientation processorCoolerMountOrientation = default,
            ProcessorCoolerTimState processorCoolerTimState =
                ProcessorCoolerTimState.Unsupported,
            GraphicsCardSlotDefinition graphicsCardSlotDefinition = default,
            GraphicsCardSlotState graphicsCardSlotState =
                GraphicsCardSlotState.Unsupported,
            StableId<ItemInstanceIdScope> graphicsCardItemId = default,
            StableId<ProductDefinitionIdScope> graphicsCardProductId = default,
            StableId<AssemblyOperationIdScope> graphicsCardSeatedByOperationId =
                default,
            StableId<AssemblyOperationIdScope> graphicsCardRetainedByOperationId =
                default,
            GraphicsCardMountOrientation graphicsCardMountOrientation = default,
            PowerSupplyBayDefinition powerSupplyBayDefinition = default,
            PowerSupplyBayState powerSupplyBayState = PowerSupplyBayState.Unsupported,
            StableId<ItemInstanceIdScope> powerSupplyItemId = default,
            StableId<ProductDefinitionIdScope> powerSupplyProductId = default,
            StableId<AssemblyOperationIdScope> powerSupplySeatedByOperationId =
                default,
            StableId<AssemblyOperationIdScope> powerSupplyRetainedByOperationId =
                default,
            PowerSupplyMountOrientation powerSupplyMountOrientation = default)
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
            MemorySlotDefinition = memorySlotDefinition;
            MemorySlotState = memorySlotState;
            MemoryItemId = memoryItemId;
            MemoryProductId = memoryProductId;
            MemorySeatedByOperationId = memorySeatedByOperationId;
            MemoryRetainedByOperationId = memoryRetainedByOperationId;
            StorageSlotDefinition = storageSlotDefinition;
            StorageSlotState = storageSlotState;
            StorageItemId = storageItemId;
            StorageProductId = storageProductId;
            StorageSeatedByOperationId = storageSeatedByOperationId;
            StorageSecuredByOperationId = storageSecuredByOperationId;
            ProcessorCoolerSlotDefinition = processorCoolerSlotDefinition;
            ProcessorCoolerSlotState = processorCoolerSlotState;
            ProcessorCoolerItemId = processorCoolerItemId;
            ProcessorCoolerProductId = processorCoolerProductId;
            ProcessorCoolerSeatedByOperationId =
                processorCoolerSeatedByOperationId;
            ProcessorCoolerRetainedByOperationId =
                processorCoolerRetainedByOperationId;
            ProcessorCoolerMountOrientation = processorCoolerMountOrientation;
            ProcessorCoolerTimState = processorCoolerTimState;
            GraphicsCardSlotDefinition = graphicsCardSlotDefinition;
            GraphicsCardSlotState = graphicsCardSlotState;
            GraphicsCardItemId = graphicsCardItemId;
            GraphicsCardProductId = graphicsCardProductId;
            GraphicsCardSeatedByOperationId = graphicsCardSeatedByOperationId;
            GraphicsCardRetainedByOperationId = graphicsCardRetainedByOperationId;
            GraphicsCardMountOrientation = graphicsCardMountOrientation;
            PowerSupplyBayDefinition = powerSupplyBayDefinition;
            PowerSupplyBayState = powerSupplyBayState;
            PowerSupplyItemId = powerSupplyItemId;
            PowerSupplyProductId = powerSupplyProductId;
            PowerSupplySeatedByOperationId = powerSupplySeatedByOperationId;
            PowerSupplyRetainedByOperationId = powerSupplyRetainedByOperationId;
            PowerSupplyMountOrientation = powerSupplyMountOrientation;
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

        public DimmSlotDefinition MemorySlotDefinition { get; }

        public bool HasMemorySlot => MemorySlotDefinition.IsValid;

        public StableId<AssemblySlotIdScope> MemorySlotId => MemorySlotDefinition.SlotId;

        public StableId<AssemblyRetentionIdScope> MemoryRetentionId =>
            MemorySlotDefinition.RetentionId;

        public StableId<ContainerIdScope> MemorySlotContainerId =>
            MemorySlotDefinition.ContainerId;

        public StableId<AssemblyMemoryChannelIdScope> MemoryChannelId =>
            MemorySlotDefinition.ChannelId;

        public StableId<AssemblyMemoryBankIdScope> MemoryBankId =>
            MemorySlotDefinition.BankId;

        public int MemoryPopulationPriority => MemorySlotDefinition.PopulationPriority;

        public DimmType SupportedDimmType => MemorySlotDefinition.SupportedDimmType;

        public MemorySlotState MemorySlotState { get; }

        public StableId<ItemInstanceIdScope> MemoryItemId { get; }

        public StableId<ProductDefinitionIdScope> MemoryProductId { get; }

        public StableId<AssemblyOperationIdScope> MemorySeatedByOperationId { get; }

        public StableId<AssemblyOperationIdScope> MemoryRetainedByOperationId { get; }

        public M2SlotDefinition StorageSlotDefinition { get; }

        public bool HasStorageSlot => StorageSlotDefinition.IsValid;

        public StableId<AssemblySlotIdScope> StorageSlotId => StorageSlotDefinition.SlotId;

        public StableId<AssemblyStorageStandoffIdScope> StorageStandoffId =>
            StorageSlotDefinition.StandoffId;

        public StableId<AssemblyRetentionIdScope> StorageCaptiveScrewId =>
            StorageSlotDefinition.CaptiveScrewId;

        public StableId<ContainerIdScope> StorageSlotContainerId =>
            StorageSlotDefinition.ContainerId;

        public M2StorageType SupportedM2StorageType =>
            StorageSlotDefinition.SupportedStorageType;

        public StorageSlotState StorageSlotState { get; }

        public StableId<ItemInstanceIdScope> StorageItemId { get; }

        public StableId<ProductDefinitionIdScope> StorageProductId { get; }

        public StableId<AssemblyOperationIdScope> StorageSeatedByOperationId { get; }

        public StableId<AssemblyOperationIdScope> StorageSecuredByOperationId { get; }

        public ProcessorCoolerSlotDefinition ProcessorCoolerSlotDefinition { get; }

        public bool HasProcessorCoolerSlot => ProcessorCoolerSlotDefinition.IsValid;

        public StableId<AssemblySlotIdScope> ProcessorCoolerSlotId =>
            ProcessorCoolerSlotDefinition.SlotId;

        public StableId<AssemblyProcessorCoolerBracketIdScope> ProcessorCoolerBracketId =>
            ProcessorCoolerSlotDefinition.BracketId;

        public StableId<ContainerIdScope> ProcessorCoolerSlotContainerId =>
            ProcessorCoolerSlotDefinition.ContainerId;

        public ProcessorCoolerRetentionTopology ProcessorCoolerRetentionTopology =>
            ProcessorCoolerSlotDefinition.RetentionTopology;

        public ProcessorCoolerType SupportedProcessorCoolerType =>
            ProcessorCoolerSlotDefinition.SupportedCoolerType;

        public ProcessorCoolerSlotState ProcessorCoolerSlotState { get; }

        public StableId<ItemInstanceIdScope> ProcessorCoolerItemId { get; }

        public StableId<ProductDefinitionIdScope> ProcessorCoolerProductId { get; }

        public StableId<AssemblyOperationIdScope> ProcessorCoolerSeatedByOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> ProcessorCoolerRetainedByOperationId
        {
            get;
        }

        public ProcessorCoolerMountOrientation ProcessorCoolerMountOrientation { get; }

        public ProcessorCoolerTimState ProcessorCoolerTimState { get; }

        public GraphicsCardSlotDefinition GraphicsCardSlotDefinition { get; }

        public bool HasGraphicsCardSlot => GraphicsCardSlotDefinition.IsValid;

        public StableId<AssemblySlotIdScope> GraphicsCardSlotId =>
            GraphicsCardSlotDefinition.SlotId;

        public StableId<ContainerIdScope> GraphicsCardSlotContainerId =>
            GraphicsCardSlotDefinition.ContainerId;

        public GraphicsCardRetentionTopology GraphicsCardRetentionTopology =>
            GraphicsCardSlotDefinition.RetentionTopology;

        public StableId<AssemblyGraphicsCardLatchIdScope> GraphicsCardLatchId =>
            GraphicsCardSlotDefinition.RetentionTopology == null
                ? default
                : GraphicsCardSlotDefinition.RetentionTopology.LatchId;

        public StableId<AssemblyFastenerIdScope> GraphicsCardBracketFastenerId =>
            GraphicsCardSlotDefinition.RetentionTopology == null
                ? default
                : GraphicsCardSlotDefinition.RetentionTopology.BracketFastenerId;

        public GraphicsCardType SupportedGraphicsCardType =>
            GraphicsCardSlotDefinition.SupportedGraphicsCardType;

        public GraphicsCardSlotState GraphicsCardSlotState { get; }

        public StableId<ItemInstanceIdScope> GraphicsCardItemId { get; }

        public StableId<ProductDefinitionIdScope> GraphicsCardProductId { get; }

        public StableId<AssemblyOperationIdScope> GraphicsCardSeatedByOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> GraphicsCardRetainedByOperationId
        {
            get;
        }

        public GraphicsCardMountOrientation GraphicsCardMountOrientation { get; }

        public PowerSupplyBayDefinition PowerSupplyBayDefinition { get; }

        public bool HasPowerSupplyBay => PowerSupplyBayDefinition.IsValid;

        public StableId<AssemblySlotIdScope> PowerSupplyBaySlotId =>
            PowerSupplyBayDefinition.SlotId;

        public StableId<ContainerIdScope> PowerSupplyBayContainerId =>
            PowerSupplyBayDefinition.ContainerId;

        public PowerSupplyRetentionTopology PowerSupplyRetentionTopology =>
            PowerSupplyBayDefinition.RetentionTopology;

        public PowerSupplyType SupportedPowerSupplyType =>
            PowerSupplyBayDefinition.SupportedPowerSupplyType;

        public PowerSupplyBayState PowerSupplyBayState { get; }

        public StableId<ItemInstanceIdScope> PowerSupplyItemId { get; }

        public StableId<ProductDefinitionIdScope> PowerSupplyProductId { get; }

        public StableId<AssemblyOperationIdScope> PowerSupplySeatedByOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> PowerSupplyRetainedByOperationId
        {
            get;
        }

        public PowerSupplyMountOrientation PowerSupplyMountOrientation { get; }

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
        public static readonly Failure InvalidMemorySlotContainer =
            Failure.FromCode("assembly.memory-slot-container.invalid");
        public static readonly Failure InvalidMemorySlotDefinition =
            Failure.FromCode("assembly.memory-slot-definition.invalid");
        public static readonly Failure InvalidStorageSlotContainer =
            Failure.FromCode("assembly.storage-slot-container.invalid");
        public static readonly Failure InvalidStorageSlotDefinition =
            Failure.FromCode("assembly.storage-slot-definition.invalid");
        public static readonly Failure InvalidProcessorCoolerSlotContainer =
            Failure.FromCode("assembly.processor-cooler-slot-container.invalid");
        public static readonly Failure InvalidProcessorCoolerSlotDefinition =
            Failure.FromCode("assembly.processor-cooler-slot-definition.invalid");
        public static readonly Failure InvalidProcessorCoolerBracket =
            Failure.FromCode("assembly.processor-cooler-bracket.invalid");
        public static readonly Failure InvalidProcessorCoolerRetentionTopology =
            Failure.FromCode("assembly.processor-cooler-retention-topology.invalid");
        public static readonly Failure InvalidGraphicsCardSlotContainer =
            Failure.FromCode("assembly.graphics-card-slot-container.invalid");
        public static readonly Failure InvalidGraphicsCardSlotDefinition =
            Failure.FromCode("assembly.graphics-card-slot-definition.invalid");
        public static readonly Failure InvalidGraphicsCardSlotLatch =
            Failure.FromCode("assembly.graphics-card-slot-latch.invalid");
        public static readonly Failure InvalidGraphicsCardBracketFastener =
            Failure.FromCode("assembly.graphics-card-bracket-fastener.invalid");
        public static readonly Failure InvalidPowerSupplyBayContainer =
            Failure.FromCode("assembly.power-supply-bay-container.invalid");
        public static readonly Failure InvalidPowerSupplyBayDefinition =
            Failure.FromCode("assembly.power-supply-bay-definition.invalid");
        public static readonly Failure InvalidPowerSupplyRearMount =
            Failure.FromCode("assembly.power-supply-rear-mount.invalid");
        public static readonly Failure InvalidPowerSupplyFastenerTopology =
            Failure.FromCode("assembly.power-supply-fastener-topology.invalid");
        public static readonly Failure SameInventoryContainer =
            Failure.FromCode("assembly.inventory-container.same");
        public static readonly Failure InvalidMotherboardFormFactor =
            Failure.FromCode("assembly.motherboard-form-factor.invalid");
        public static readonly Failure InvalidCpuSocketFamily =
            Failure.FromCode("assembly.cpu-socket-family.invalid");
        public static readonly Failure InvalidDimmType =
            Failure.FromCode("assembly.dimm-type.invalid");
        public static readonly Failure InvalidM2StorageType =
            Failure.FromCode("assembly.m2-storage-type.invalid");
        public static readonly Failure InvalidProcessorCoolerType =
            Failure.FromCode("assembly.processor-cooler-type.invalid");
        public static readonly Failure InvalidGraphicsCardType =
            Failure.FromCode("assembly.graphics-card-type.invalid");
        public static readonly Failure InvalidPowerSupplyType =
            Failure.FromCode("assembly.power-supply-type.invalid");
        public static readonly Failure InvalidDimmOrientation =
            Failure.FromCode("assembly.dimm-orientation.invalid");
        public static readonly Failure DimmOrientationMismatch =
            Failure.FromCode("assembly.dimm-orientation.mismatch");
        public static readonly Failure InvalidM2Orientation =
            Failure.FromCode("assembly.m2-orientation.invalid");
        public static readonly Failure M2OrientationMismatch =
            Failure.FromCode("assembly.m2-orientation.mismatch");
        public static readonly Failure InvalidProcessorCoolerOrientation =
            Failure.FromCode("assembly.processor-cooler-orientation.invalid");
        public static readonly Failure InvalidGraphicsCardOrientation =
            Failure.FromCode("assembly.graphics-card-orientation.invalid");
        public static readonly Failure GraphicsCardOrientationMismatch =
            Failure.FromCode("assembly.graphics-card-orientation.mismatch");
        public static readonly Failure InvalidPowerSupplyOrientation =
            Failure.FromCode("assembly.power-supply-orientation.invalid");
        public static readonly Failure PowerSupplyOrientationMismatch =
            Failure.FromCode("assembly.power-supply-orientation.mismatch");
        public static readonly Failure InvalidStorageStandoff =
            Failure.FromCode("assembly.storage-standoff.invalid");
        public static readonly Failure InvalidMemoryChannel =
            Failure.FromCode("assembly.memory-channel.invalid");
        public static readonly Failure InvalidMemoryBank =
            Failure.FromCode("assembly.memory-bank.invalid");
        public static readonly Failure InvalidMemoryPopulationPriority =
            Failure.FromCode("assembly.memory-population-priority.invalid");
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
        public static readonly Failure MemorySlotOccupied =
            Failure.FromCode("assembly.memory-slot.occupied");
        public static readonly Failure StorageSlotOccupied =
            Failure.FromCode("assembly.storage-slot.occupied");
        public static readonly Failure ProcessorCoolerSlotOccupied =
            Failure.FromCode("assembly.processor-cooler-slot.occupied");
        public static readonly Failure GraphicsCardSlotOccupied =
            Failure.FromCode("assembly.graphics-card-slot.occupied");
        public static readonly Failure PowerSupplyBayOccupied =
            Failure.FromCode("assembly.power-supply-bay.occupied");
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
        public static readonly Failure MemoryModuleInstalled =
            Failure.FromCode("assembly.motherboard.memory-installed");
        public static readonly Failure StorageDeviceInstalled =
            Failure.FromCode("assembly.motherboard.storage-installed");
        public static readonly Failure ProcessorCoolerInstalled =
            Failure.FromCode("assembly.motherboard.processor-cooler-installed");
        public static readonly Failure GraphicsCardInstalled =
            Failure.FromCode("assembly.motherboard.graphics-card-installed");
        public static readonly Failure MemoryRetentionOutOfOrder =
            Failure.FromCode("assembly.memory-retention.out-of-order");
        public static readonly Failure MemoryModuleRetained =
            Failure.FromCode("assembly.memory-module.retained");
        public static readonly Failure StorageRetentionOutOfOrder =
            Failure.FromCode("assembly.storage-retention.out-of-order");
        public static readonly Failure StorageDeviceSecured =
            Failure.FromCode("assembly.storage-device.secured");
        public static readonly Failure ProcessorCoolerRetentionOutOfOrder =
            Failure.FromCode("assembly.processor-cooler-retention.out-of-order");
        public static readonly Failure ProcessorCoolerRetained =
            Failure.FromCode("assembly.processor-cooler.retained");
        public static readonly Failure ProcessorCoolerTimConsumed =
            Failure.FromCode("assembly.processor-cooler.tim-consumed");
        public static readonly Failure GraphicsCardRetentionOutOfOrder =
            Failure.FromCode("assembly.graphics-card-retention.out-of-order");
        public static readonly Failure GraphicsCardRetained =
            Failure.FromCode("assembly.graphics-card.retained");
        public static readonly Failure PowerSupplyRetentionOutOfOrder =
            Failure.FromCode("assembly.power-supply-retention.out-of-order");
        public static readonly Failure PowerSupplyRetained =
            Failure.FromCode("assembly.power-supply.retained");
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
        public static readonly Failure DimmTypeMismatch =
            Failure.FromCode("assembly.dimm-type.mismatch");
        public static readonly Failure M2StorageTypeMismatch =
            Failure.FromCode("assembly.m2-storage-type.mismatch");
        public static readonly Failure ProcessorCoolerTypeMismatch =
            Failure.FromCode("assembly.processor-cooler-type.mismatch");
        public static readonly Failure ProcessorCoolerSocketMismatch =
            Failure.FromCode("assembly.processor-cooler-socket.mismatch");
        public static readonly Failure GraphicsCardTypeMismatch =
            Failure.FromCode("assembly.graphics-card-type.mismatch");
        public static readonly Failure PowerSupplyTypeMismatch =
            Failure.FromCode("assembly.power-supply-type.mismatch");
        public static readonly Failure WorkbenchCapacityExceeded =
            Failure.FromCode("assembly.workbench.capacity");
        public static readonly Failure ProcessorSocketCapacityExceeded =
            Failure.FromCode("assembly.processor-socket.capacity");
        public static readonly Failure MemorySlotCapacityExceeded =
            Failure.FromCode("assembly.memory-slot.capacity");
        public static readonly Failure StorageSlotCapacityExceeded =
            Failure.FromCode("assembly.storage-slot.capacity");
        public static readonly Failure ProcessorCoolerSlotCapacityExceeded =
            Failure.FromCode("assembly.processor-cooler-slot.capacity");
        public static readonly Failure GraphicsCardSlotCapacityExceeded =
            Failure.FromCode("assembly.graphics-card-slot.capacity");
        public static readonly Failure PowerSupplyBayCapacityExceeded =
            Failure.FromCode("assembly.power-supply-bay.capacity");
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
        public static readonly Failure MemoryMissing =
            Failure.FromCode("assembly.benchmark.memory-missing");
        public static readonly Failure MemoryUnretained =
            Failure.FromCode("assembly.benchmark.memory-unretained");
        public static readonly Failure StorageMissing =
            Failure.FromCode("assembly.benchmark.storage-missing");
        public static readonly Failure StorageUnsecured =
            Failure.FromCode("assembly.benchmark.storage-unsecured");
        public static readonly Failure ProcessorCoolerMissing =
            Failure.FromCode("assembly.benchmark.processor-cooler-missing");
        public static readonly Failure ProcessorCoolerUnretained =
            Failure.FromCode("assembly.benchmark.processor-cooler-unretained");
        public static readonly Failure GraphicsCardMissing =
            Failure.FromCode("assembly.benchmark.graphics-card-missing");
        public static readonly Failure GraphicsCardUnretained =
            Failure.FromCode("assembly.benchmark.graphics-card-unretained");
        public static readonly Failure PowerSupplyMissing =
            Failure.FromCode("assembly.benchmark.power-supply-missing");
        public static readonly Failure PowerSupplyUnretained =
            Failure.FromCode("assembly.benchmark.power-supply-unretained");
        public static readonly Failure BuildIncomplete =
            Failure.FromCode("assembly.benchmark.build-incomplete");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("assembly.invariant.failed");
    }
}
