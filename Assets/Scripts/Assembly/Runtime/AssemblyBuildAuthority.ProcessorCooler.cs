using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private ProcessorCoolerSlotDefinition _processorCoolerSlotDefinition;
        private InventorySerializedTransferAccess _processorCoolerInventoryTransferAccess;
        private ProcessorCoolerSlotState _processorCoolerSlotState =
            ProcessorCoolerSlotState.Unsupported;
        private StableId<ItemInstanceIdScope> _processorCoolerItemId;
        private StableId<ProductDefinitionIdScope> _processorCoolerProductId;
        private StableId<AssemblyOperationIdScope> _processorCoolerSeatedByOperationId;
        private StableId<AssemblyOperationIdScope> _processorCoolerRetainedByOperationId;
        private ProcessorCoolerMountOrientation _processorCoolerMountOrientation;
        private ProcessorCoolerTimState _processorCoolerTimState =
            ProcessorCoolerTimState.Unsupported;

        public bool HasProcessorCoolerSlot => _processorCoolerSlotDefinition.IsValid;

        public ProcessorCoolerSlotDefinition ProcessorCoolerSlotDefinition =>
            _processorCoolerSlotDefinition;

        public StableId<AssemblySlotIdScope> ProcessorCoolerSlotId =>
            _processorCoolerSlotDefinition.SlotId;

        public StableId<AssemblyProcessorCoolerBracketIdScope> ProcessorCoolerBracketId =>
            _processorCoolerSlotDefinition.BracketId;

        public StableId<ContainerIdScope> ProcessorCoolerSlotContainerId =>
            _processorCoolerSlotDefinition.ContainerId;

        public ProcessorCoolerRetentionTopology ProcessorCoolerRetentionTopology =>
            _processorCoolerSlotDefinition.RetentionTopology;

        public ProcessorCoolerType SupportedProcessorCoolerType =>
            _processorCoolerSlotDefinition.SupportedCoolerType;

        public ProcessorCoolerSlotState ProcessorCoolerSlotState =>
            _processorCoolerSlotState;

        public StableId<ItemInstanceIdScope> ProcessorCoolerItemId =>
            _processorCoolerItemId;

        public StableId<ProductDefinitionIdScope> ProcessorCoolerProductId =>
            _processorCoolerProductId;

        public StableId<AssemblyOperationIdScope> ProcessorCoolerSeatedByOperationId =>
            _processorCoolerSeatedByOperationId;

        public StableId<AssemblyOperationIdScope> ProcessorCoolerRetainedByOperationId =>
            _processorCoolerRetainedByOperationId;

        public ProcessorCoolerMountOrientation ProcessorCoolerMountOrientation =>
            _processorCoolerMountOrientation;

        public ProcessorCoolerTimState ProcessorCoolerTimState =>
            _processorCoolerTimState;

        public static OperationResult<AssemblyBuildAuthority>
            CreateWithProcessorSocketMemoryStorageAndCoolerSlots(
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
                StableId<ContainerIdScope> handsContainerId,
                StableId<ContainerIdScope> workbenchContainerId,
                StableId<ContainerIdScope> processorSocketContainerId,
                MotherboardFormFactor supportedMotherboardFormFactor,
                CpuSocketFamily supportedCpuSocketFamily)
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

            if (motherboardSlotId.IsEmpty || processorSlotId.IsEmpty ||
                motherboardSlotId == processorSlotId ||
                motherboardSlotId == memorySlotDefinition.SlotId ||
                motherboardSlotId == storageSlotDefinition.SlotId ||
                motherboardSlotId == processorCoolerSlotDefinition.SlotId ||
                processorSlotId == memorySlotDefinition.SlotId ||
                processorSlotId == storageSlotDefinition.SlotId ||
                processorSlotId == processorCoolerSlotDefinition.SlotId ||
                memorySlotDefinition.SlotId == storageSlotDefinition.SlotId ||
                memorySlotDefinition.SlotId == processorCoolerSlotDefinition.SlotId ||
                storageSlotDefinition.SlotId == processorCoolerSlotDefinition.SlotId)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (motherboardFastenerId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidFastener);
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

            if (processorSocketContainerId.IsEmpty ||
                !inventory.TryGetContainer(
                    processorSocketContainerId,
                    out InventoryContainerDefinition processorSocket) ||
                processorSocket.Kind != InventoryContainerKind.Workbench ||
                processorSocket.UnitCapacity != 1)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidProcessorSocketContainer);
            }

            if (!inventory.TryGetContainer(
                    memorySlotDefinition.ContainerId,
                    out InventoryContainerDefinition memorySlot) ||
                memorySlot.Kind != InventoryContainerKind.Workbench ||
                memorySlot.UnitCapacity != 1)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidMemorySlotContainer);
            }

            if (!inventory.TryGetContainer(
                    storageSlotDefinition.ContainerId,
                    out InventoryContainerDefinition storageSlot) ||
                storageSlot.Kind != InventoryContainerKind.Workbench ||
                storageSlot.UnitCapacity != 1)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidStorageSlotContainer);
            }

            if (!inventory.TryGetContainer(
                    processorCoolerSlotDefinition.ContainerId,
                    out InventoryContainerDefinition processorCoolerSlot) ||
                processorCoolerSlot.Kind != InventoryContainerKind.Workbench ||
                processorCoolerSlot.UnitCapacity != 1)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerSlotContainer);
            }

            if (handsContainerId == workbenchContainerId ||
                handsContainerId == processorSocketContainerId ||
                handsContainerId == memorySlotDefinition.ContainerId ||
                handsContainerId == storageSlotDefinition.ContainerId ||
                handsContainerId == processorCoolerSlotDefinition.ContainerId ||
                workbenchContainerId == processorSocketContainerId ||
                workbenchContainerId == memorySlotDefinition.ContainerId ||
                workbenchContainerId == storageSlotDefinition.ContainerId ||
                workbenchContainerId == processorCoolerSlotDefinition.ContainerId ||
                processorSocketContainerId == memorySlotDefinition.ContainerId ||
                processorSocketContainerId == storageSlotDefinition.ContainerId ||
                processorSocketContainerId == processorCoolerSlotDefinition.ContainerId ||
                memorySlotDefinition.ContainerId == storageSlotDefinition.ContainerId ||
                memorySlotDefinition.ContainerId ==
                    processorCoolerSlotDefinition.ContainerId ||
                storageSlotDefinition.ContainerId ==
                    processorCoolerSlotDefinition.ContainerId)
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

            OperationResult<InventorySerializedTransferAccessQuintuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId);
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
                    access.Value.Fifth));
        }

        public OperationResult<AssemblyOperationReceipt> SeatProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            ProcessorCoolerMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesSeatProcessorCooler(
                        itemId,
                        slotId,
                        orientation,
                        sourceMotherboardAttachOperationId,
                        sourceMotherboardSecureOperationId,
                        sourceProcessorSeatOperationId,
                        sourceProcessorRetentionOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateSeatProcessorCooler(
                itemId,
                slotId,
                orientation,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                sourceProcessorSeatOperationId,
                sourceProcessorRetentionOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransferAndConsumePreAppliedState(
                    itemId,
                    _processorCoolerSlotDefinition.ContainerId,
                    _processorCoolerInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorCoolerInventoryFailure(prepared.Error, seating: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorCoolerInventoryFailure(committed.Error, seating: true));
            }

            _processorCoolerSlotState = ProcessorCoolerSlotState.CoolerSeatedUnsecured;
            _processorCoolerItemId = item.Id;
            _processorCoolerProductId = item.ProductId;
            _processorCoolerSeatedByOperationId = operationId;
            _processorCoolerRetainedByOperationId = default;
            _processorCoolerMountOrientation = orientation;
            _processorCoolerTimState = ProcessorCoolerTimState.AppliedConsumed;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.SeatProcessorCooler,
                BuildId,
                ChassisId,
                _processorCoolerSlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _processorCoolerSlotDefinition.ContainerId,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                default,
                default,
                sourceProcessorSeatOperationId,
                sourceProcessorRetentionOperationId,
                -1,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                _processorSocketState,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                _memorySlotState,
                _memorySlotState,
                default,
                default,
                default,
                _storageSlotState,
                _storageSlotState,
                default,
                default,
                default,
                ProcessorCoolerSlotState.EmptyOpen,
                _processorCoolerSlotState,
                default,
                default,
                orientation,
                ProcessorCoolerTimState.PreAppliedUnused,
                _processorCoolerTimState,
                _processorCoolerSlotDefinition);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RetainProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyProcessorCoolerBracketIdScope> bracketId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesRetainProcessorCooler(
                        itemId,
                        slotId,
                        bracketId,
                        sourceProcessorCoolerSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateProcessorCoolerRetention(
                itemId,
                slotId,
                bracketId,
                sourceProcessorCoolerSeatOperationId,
                default,
                expectedAssemblyRevision,
                retaining: true);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceProcessorCoolerSeatOperationId];
            _processorCoolerSlotState = ProcessorCoolerSlotState.CoolerRetained;
            _processorCoolerRetainedByOperationId = operationId;
            Revision++;

            AssemblyOperationReceipt receipt = CreateProcessorCoolerRetentionReceipt(
                operationId,
                AssemblyOperationKind.RetainProcessorCooler,
                seatReceipt,
                sourceProcessorCoolerSeatOperationId,
                default,
                expectedAssemblyRevision,
                ProcessorCoolerSlotState.CoolerSeatedUnsecured,
                _processorCoolerSlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> UnretainProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyProcessorCoolerBracketIdScope> bracketId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceProcessorCoolerRetentionOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesUnretainProcessorCooler(
                        itemId,
                        slotId,
                        bracketId,
                        sourceProcessorCoolerSeatOperationId,
                        sourceProcessorCoolerRetentionOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateProcessorCoolerRetention(
                itemId,
                slotId,
                bracketId,
                sourceProcessorCoolerSeatOperationId,
                sourceProcessorCoolerRetentionOperationId,
                expectedAssemblyRevision,
                retaining: false);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceProcessorCoolerSeatOperationId];
            _processorCoolerSlotState =
                ProcessorCoolerSlotState.CoolerSeatedUnsecured;
            _processorCoolerRetainedByOperationId = default;
            Revision++;

            AssemblyOperationReceipt receipt = CreateProcessorCoolerRetentionReceipt(
                operationId,
                AssemblyOperationKind.UnretainProcessorCooler,
                seatReceipt,
                sourceProcessorCoolerSeatOperationId,
                sourceProcessorCoolerRetentionOperationId,
                expectedAssemblyRevision,
                ProcessorCoolerSlotState.CoolerRetained,
                _processorCoolerSlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveProcessorCooler(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesRemoveProcessorCooler(
                        itemId,
                        slotId,
                        sourceProcessorCoolerSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateRemoveProcessorCooler(
                itemId,
                slotId,
                sourceProcessorCoolerSeatOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceProcessorCoolerSeatOperationId];
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _processorCoolerInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorCoolerInventoryFailure(prepared.Error, seating: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorCoolerInventoryFailure(committed.Error, seating: false));
            }

            ProcessorCoolerMountOrientation removedOrientation =
                _processorCoolerMountOrientation;
            _processorCoolerSlotState = ProcessorCoolerSlotState.EmptyOpen;
            _processorCoolerItemId = default;
            _processorCoolerProductId = default;
            _processorCoolerSeatedByOperationId = default;
            _processorCoolerRetainedByOperationId = default;
            _processorCoolerMountOrientation = default;
            _processorCoolerTimState = ProcessorCoolerTimState.Unsupported;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.RemoveProcessorCooler,
                BuildId,
                ChassisId,
                _processorCoolerSlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _processorCoolerSlotDefinition.ContainerId,
                _handsContainerId,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                default,
                default,
                seatReceipt.SourceProcessorSeatOperationId,
                seatReceipt.SourceProcessorRetentionOperationId,
                -1,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                _processorSocketState,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                _memorySlotState,
                _memorySlotState,
                default,
                default,
                default,
                _storageSlotState,
                _storageSlotState,
                default,
                default,
                default,
                ProcessorCoolerSlotState.CoolerSeatedUnsecured,
                _processorCoolerSlotState,
                sourceProcessorCoolerSeatOperationId,
                default,
                removedOrientation,
                ProcessorCoolerTimState.AppliedConsumed,
                _processorCoolerTimState,
                _processorCoolerSlotDefinition);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        private AssemblyOperationReceipt CreateProcessorCoolerRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationKind operationKind,
            AssemblyOperationReceipt seatReceipt,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceProcessorCoolerRetentionOperationId,
            long expectedAssemblyRevision,
            ProcessorCoolerSlotState previousProcessorCoolerSlotState,
            ProcessorCoolerSlotState resultingProcessorCoolerSlotState)
        {
            return new AssemblyOperationReceipt(
                operationId,
                operationKind,
                BuildId,
                ChassisId,
                _processorCoolerSlotDefinition.SlotId,
                _processorCoolerItemId,
                _processorCoolerProductId,
                default,
                default,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                default,
                default,
                seatReceipt.SourceProcessorSeatOperationId,
                seatReceipt.SourceProcessorRetentionOperationId,
                0,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                _processorSocketState,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                _memorySlotState,
                _memorySlotState,
                default,
                default,
                default,
                _storageSlotState,
                _storageSlotState,
                default,
                default,
                default,
                previousProcessorCoolerSlotState,
                resultingProcessorCoolerSlotState,
                sourceProcessorCoolerSeatOperationId,
                sourceProcessorCoolerRetentionOperationId,
                _processorCoolerMountOrientation,
                ProcessorCoolerTimState.AppliedConsumed,
                ProcessorCoolerTimState.AppliedConsumed,
                _processorCoolerSlotDefinition);
        }

        private Failure ValidateSeatProcessorCooler(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            ProcessorCoolerMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedAssemblyRevision)
        {
            if (!HasProcessorCoolerSlot)
            {
                return AssemblyFailures.InvalidProcessorCoolerSlotDefinition;
            }

            if (slotId != _processorCoolerSlotDefinition.SlotId)
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

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (_motherboardSeatState != AssemblySeatState.SeatedSecured)
            {
                return AssemblyFailures.MotherboardUnsecured;
            }

            if (sourceMotherboardAttachOperationId.IsEmpty ||
                sourceMotherboardSecureOperationId.IsEmpty ||
                sourceMotherboardAttachOperationId != _installedByOperationId ||
                sourceMotherboardSecureOperationId != _securedByOperationId ||
                !_receipts.TryGetValue(
                    sourceMotherboardAttachOperationId,
                    out AssemblyOperationReceipt attachReceipt) ||
                !_receipts.TryGetValue(
                    sourceMotherboardSecureOperationId,
                    out AssemblyOperationReceipt secureReceipt) ||
                attachReceipt.OperationKind != AssemblyOperationKind.AttachMotherboard ||
                secureReceipt.OperationKind !=
                    AssemblyOperationKind.SecureMotherboardFastener ||
                secureReceipt.SourceAttachOperationId != attachReceipt.OperationId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_processorSocketState == ProcessorSocketState.EmptyOpen)
            {
                return AssemblyFailures.ProcessorMissing;
            }

            if (_processorSocketState != ProcessorSocketState.ProcessorRetained)
            {
                return AssemblyFailures.ProcessorUnretained;
            }

            if (sourceProcessorSeatOperationId.IsEmpty ||
                sourceProcessorRetentionOperationId.IsEmpty ||
                sourceProcessorSeatOperationId != _processorSeatedByOperationId ||
                sourceProcessorRetentionOperationId != _processorRetainedByOperationId ||
                !_receipts.TryGetValue(
                    sourceProcessorSeatOperationId,
                    out AssemblyOperationReceipt processorSeatReceipt) ||
                !_receipts.TryGetValue(
                    sourceProcessorRetentionOperationId,
                    out AssemblyOperationReceipt processorRetentionReceipt) ||
                processorSeatReceipt.OperationKind != AssemblyOperationKind.SeatProcessor ||
                processorRetentionReceipt.OperationKind !=
                    AssemblyOperationKind.CloseProcessorRetention ||
                processorRetentionReceipt.SourceProcessorSeatOperationId !=
                    processorSeatReceipt.OperationId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_processorCoolerSlotState != ProcessorCoolerSlotState.EmptyOpen)
            {
                return AssemblyFailures.ProcessorCoolerSlotOccupied;
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item))
            {
                return AssemblyFailures.UnknownItem;
            }

            if (item.ContainerId != _handsContainerId)
            {
                return AssemblyFailures.ItemNotInActorHands;
            }

            if (HasConsumedProcessorCoolerTim(itemId))
            {
                return AssemblyFailures.ProcessorCoolerTimConsumed;
            }

            if (!_componentCatalog.TryGet(
                    item.ProductId,
                    out PcComponentSpecification coolerSpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification) ||
                !_componentCatalog.TryGet(
                    _processorProductId,
                    out PcComponentSpecification processorSpecification))
            {
                return AssemblyFailures.UnknownComponentSpecification;
            }

            AssemblyCompatibilityResult compatibility =
                AssemblyCompatibilityEvaluator.EvaluateProcessorCoolerSeat(
                    coolerSpecification,
                    motherboardSpecification,
                    processorSpecification,
                    _processorCoolerSlotDefinition.SupportedCoolerType,
                    _processorCoolerSlotDefinition.SupportedSocketFamily,
                    orientation);
            return compatibility.IsCompatible ? Failure.None : compatibility.Reason;
        }

        private Failure ValidateProcessorCoolerRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyProcessorCoolerBracketIdScope> bracketId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceProcessorCoolerRetentionOperationId,
            long expectedAssemblyRevision,
            bool retaining)
        {
            if (!HasProcessorCoolerSlot ||
                slotId != _processorCoolerSlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (bracketId != _processorCoolerSlotDefinition.BracketId)
            {
                return AssemblyFailures.InvalidProcessorCoolerBracket;
            }

            if (itemId.IsEmpty ||
                (!_processorCoolerItemId.IsEmpty && itemId != _processorCoolerItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceProcessorCoolerSeatOperationId.IsEmpty ||
                sourceProcessorCoolerSeatOperationId !=
                    _processorCoolerSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceProcessorCoolerSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatProcessorCooler ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (retaining && _motherboardSeatState != AssemblySeatState.SeatedSecured)
            {
                return AssemblyFailures.MotherboardUnsecured;
            }

            if (_processorSocketState != ProcessorSocketState.ProcessorRetained ||
                seatReceipt.SourceProcessorSeatOperationId !=
                    _processorSeatedByOperationId ||
                seatReceipt.SourceProcessorRetentionOperationId !=
                    _processorRetainedByOperationId)
            {
                return AssemblyFailures.ProcessorUnretained;
            }

            if (retaining)
            {
                if (!sourceProcessorCoolerRetentionOperationId.IsEmpty ||
                    _processorCoolerSlotState !=
                        ProcessorCoolerSlotState.CoolerSeatedUnsecured ||
                    !_processorCoolerRetainedByOperationId.IsEmpty)
                {
                    return AssemblyFailures.ProcessorCoolerRetentionOutOfOrder;
                }
            }
            else if (sourceProcessorCoolerRetentionOperationId.IsEmpty ||
                     sourceProcessorCoolerRetentionOperationId !=
                         _processorCoolerRetainedByOperationId ||
                     _processorCoolerSlotState !=
                         ProcessorCoolerSlotState.CoolerRetained ||
                     !_receipts.TryGetValue(
                         sourceProcessorCoolerRetentionOperationId,
                         out AssemblyOperationReceipt retentionReceipt) ||
                     retentionReceipt.OperationKind !=
                         AssemblyOperationKind.RetainProcessorCooler ||
                     retentionReceipt.ItemId != itemId ||
                     retentionReceipt.SourceProcessorCoolerSeatOperationId !=
                         sourceProcessorCoolerSeatOperationId ||
                     !retentionReceipt.ProcessorCoolerSlotDefinition.HasExactIdentity(
                         _processorCoolerSlotDefinition))
            {
                return AssemblyFailures.ProcessorCoolerRetentionOutOfOrder;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _processorCoolerProductId &&
                   item.ContainerId == _processorCoolerSlotDefinition.ContainerId &&
                   _processorCoolerTimState ==
                       ProcessorCoolerTimState.AppliedConsumed
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private Failure ValidateRemoveProcessorCooler(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceProcessorCoolerSeatOperationId,
            long expectedAssemblyRevision)
        {
            if (!HasProcessorCoolerSlot ||
                slotId != _processorCoolerSlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceProcessorCoolerSeatOperationId.IsEmpty ||
                sourceProcessorCoolerSeatOperationId !=
                    _processorCoolerSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceProcessorCoolerSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatProcessorCooler ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (_processorCoolerSlotState == ProcessorCoolerSlotState.CoolerRetained)
            {
                return AssemblyFailures.ProcessorCoolerRetained;
            }

            if (_processorCoolerSlotState !=
                    ProcessorCoolerSlotState.CoolerSeatedUnsecured ||
                itemId != _processorCoolerItemId)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _processorCoolerProductId &&
                   item.ContainerId == _processorCoolerSlotDefinition.ContainerId &&
                   _processorCoolerTimState ==
                       ProcessorCoolerTimState.AppliedConsumed
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private bool ValidateProcessorCoolerStateInvariants()
        {
            if (!HasProcessorCoolerSlot)
            {
                return _processorCoolerInventoryTransferAccess == null &&
                       _processorCoolerSlotState ==
                           ProcessorCoolerSlotState.Unsupported &&
                       _processorCoolerSlotDefinition.SlotId.IsEmpty &&
                       _processorCoolerSlotDefinition.BracketId.IsEmpty &&
                       _processorCoolerSlotDefinition.ContainerId.IsEmpty &&
                       _processorCoolerSlotDefinition.RetentionTopology == null &&
                       _processorCoolerSlotDefinition.SupportedCoolerType == default &&
                       _processorCoolerSlotDefinition.SupportedSocketFamily == default &&
                       _processorCoolerItemId.IsEmpty &&
                       _processorCoolerProductId.IsEmpty &&
                       _processorCoolerSeatedByOperationId.IsEmpty &&
                       _processorCoolerRetainedByOperationId.IsEmpty &&
                       _processorCoolerMountOrientation == default &&
                       _processorCoolerTimState == ProcessorCoolerTimState.Unsupported;
            }

            if (_processorCoolerInventoryTransferAccess == null ||
                _processorCoolerSlotDefinition.SlotId == MotherboardSlotId ||
                _processorCoolerSlotDefinition.SlotId == _processorSlotId ||
                _processorCoolerSlotDefinition.SlotId ==
                    _memorySlotDefinition.SlotId ||
                _processorCoolerSlotDefinition.SlotId ==
                    _storageSlotDefinition.SlotId ||
                _processorCoolerSlotDefinition.ContainerId == _handsContainerId ||
                _processorCoolerSlotDefinition.ContainerId ==
                    _workbenchContainerId ||
                _processorCoolerSlotDefinition.ContainerId ==
                    _processorSocketContainerId ||
                _processorCoolerSlotDefinition.ContainerId ==
                    _memorySlotDefinition.ContainerId ||
                _processorCoolerSlotDefinition.ContainerId ==
                    _storageSlotDefinition.ContainerId ||
                _processorCoolerSlotDefinition.SupportedSocketFamily !=
                    _supportedCpuSocketFamily ||
                !_inventory.TryGetContainer(
                    _processorCoolerSlotDefinition.ContainerId,
                    out InventoryContainerDefinition coolerSlot) ||
                coolerSlot.Kind != InventoryContainerKind.Workbench ||
                coolerSlot.UnitCapacity != 1)
            {
                return false;
            }

            if (_processorCoolerSlotState == ProcessorCoolerSlotState.EmptyOpen)
            {
                return _processorCoolerItemId.IsEmpty &&
                       _processorCoolerProductId.IsEmpty &&
                       _processorCoolerSeatedByOperationId.IsEmpty &&
                       _processorCoolerRetainedByOperationId.IsEmpty &&
                       _processorCoolerMountOrientation == default &&
                       _processorCoolerTimState == ProcessorCoolerTimState.Unsupported &&
                       _inventory.GetContainerQuantity(
                           _processorCoolerSlotDefinition.ContainerId).Value == 0;
            }

            if (_processorCoolerSlotState !=
                    ProcessorCoolerSlotState.CoolerSeatedUnsecured &&
                _processorCoolerSlotState != ProcessorCoolerSlotState.CoolerRetained)
            {
                return false;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty ||
                _processorSocketState != ProcessorSocketState.ProcessorRetained ||
                _processorCoolerItemId.IsEmpty ||
                _processorCoolerProductId.IsEmpty ||
                _processorCoolerSeatedByOperationId.IsEmpty ||
                _processorCoolerTimState !=
                    ProcessorCoolerTimState.AppliedConsumed ||
                !_inventory.TryGetSerializedItem(
                    _processorCoolerItemId,
                    out InventoryItemRecord coolerItem) ||
                coolerItem.ProductId != _processorCoolerProductId ||
                coolerItem.ContainerId !=
                    _processorCoolerSlotDefinition.ContainerId ||
                (coolerItem.StateFlags &
                 InventorySerializedItemStateFlags.PreAppliedConsumableConsumed) == 0 ||
                !_componentCatalog.TryGet(
                    coolerItem.ProductId,
                    out PcComponentSpecification coolerSpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification) ||
                !_componentCatalog.TryGet(
                    _processorProductId,
                    out PcComponentSpecification processorSpecification) ||
                !AssemblyCompatibilityEvaluator.EvaluateProcessorCoolerSeat(
                    coolerSpecification,
                    motherboardSpecification,
                    processorSpecification,
                    _processorCoolerSlotDefinition.SupportedCoolerType,
                    _processorCoolerSlotDefinition.SupportedSocketFamily,
                    _processorCoolerMountOrientation).IsCompatible ||
                !_receipts.TryGetValue(
                    _processorCoolerSeatedByOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind !=
                    AssemblyOperationKind.SeatProcessorCooler ||
                seatReceipt.ItemId != _processorCoolerItemId ||
                seatReceipt.ProductId != _processorCoolerProductId ||
                seatReceipt.SlotId != _processorCoolerSlotDefinition.SlotId ||
                seatReceipt.ProcessorCoolerMountOrientation !=
                    _processorCoolerMountOrientation ||
                seatReceipt.PreviousProcessorCoolerTimState !=
                    ProcessorCoolerTimState.PreAppliedUnused ||
                seatReceipt.ResultingProcessorCoolerTimState !=
                    ProcessorCoolerTimState.AppliedConsumed ||
                !seatReceipt.ProcessorCoolerSlotDefinition.HasExactIdentity(
                    _processorCoolerSlotDefinition))
            {
                return false;
            }

            if (_processorCoolerSlotState ==
                ProcessorCoolerSlotState.CoolerSeatedUnsecured)
            {
                return _processorCoolerRetainedByOperationId.IsEmpty;
            }

            return !_processorCoolerRetainedByOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       _processorCoolerRetainedByOperationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.RetainProcessorCooler &&
                   retentionReceipt.ItemId == _processorCoolerItemId &&
                   retentionReceipt.SourceProcessorCoolerSeatOperationId ==
                       _processorCoolerSeatedByOperationId &&
                   retentionReceipt.ProcessorCoolerSlotDefinition.HasExactIdentity(
                       _processorCoolerSlotDefinition);
        }

        private bool IsMatchingProcessorCoolerSeatReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt seatReceipt) &&
                   seatReceipt.OperationKind ==
                       AssemblyOperationKind.SeatProcessorCooler &&
                   seatReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   seatReceipt.ItemId == descendant.ItemId &&
                   seatReceipt.ProductId == descendant.ProductId &&
                   seatReceipt.SlotId == descendant.SlotId &&
                   seatReceipt.SourceAttachOperationId ==
                       descendant.SourceAttachOperationId &&
                   seatReceipt.SourceSecureOperationId ==
                       descendant.SourceSecureOperationId &&
                   seatReceipt.SourceProcessorSeatOperationId ==
                       descendant.SourceProcessorSeatOperationId &&
                   seatReceipt.SourceProcessorRetentionOperationId ==
                       descendant.SourceProcessorRetentionOperationId &&
                   seatReceipt.ProcessorCoolerMountOrientation ==
                       descendant.ProcessorCoolerMountOrientation &&
                   seatReceipt.ProcessorCoolerSlotDefinition.HasExactIdentity(
                       descendant.ProcessorCoolerSlotDefinition);
        }

        private bool IsMatchingProcessorCoolerRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.RetainProcessorCooler &&
                   retentionReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   retentionReceipt.ItemId == descendant.ItemId &&
                   retentionReceipt.ProductId == descendant.ProductId &&
                   retentionReceipt.SlotId == descendant.SlotId &&
                   retentionReceipt.SourceProcessorCoolerSeatOperationId ==
                       descendant.SourceProcessorCoolerSeatOperationId &&
                   retentionReceipt.ProcessorCoolerSlotDefinition.HasExactIdentity(
                       descendant.ProcessorCoolerSlotDefinition);
        }

        private bool HasConsumedProcessorCoolerTim(
            StableId<ItemInstanceIdScope> itemId)
        {
            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   (item.StateFlags &
                    InventorySerializedItemStateFlags.PreAppliedConsumableConsumed) != 0;
        }

        private static Failure MapProcessorCoolerInventoryFailure(
            Failure failure,
            bool seating)
        {
            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return seating
                    ? AssemblyFailures.ProcessorCoolerSlotCapacityExceeded
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

            if (failure == InventoryFailures.ReservedQuantity)
            {
                return seating
                    ? AssemblyFailures.ItemNotInActorHands
                    : AssemblyFailures.ComponentNotSeated;
            }

            if (failure == InventoryFailures.SerializedItemStateConflict)
            {
                return AssemblyFailures.ProcessorCoolerTimConsumed;
            }

            return AssemblyFailures.InventoryTransferRejected;
        }
    }
}
