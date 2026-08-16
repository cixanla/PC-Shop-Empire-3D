using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private DimmSlotDefinition _memorySlotDefinition;
        private InventorySerializedTransferAccess _memoryInventoryTransferAccess;
        private MemorySlotState _memorySlotState = MemorySlotState.Unsupported;
        private StableId<ItemInstanceIdScope> _memoryItemId;
        private StableId<ProductDefinitionIdScope> _memoryProductId;
        private StableId<AssemblyOperationIdScope> _memorySeatedByOperationId;
        private StableId<AssemblyOperationIdScope> _memoryRetainedByOperationId;

        public bool HasMemorySlot => _memorySlotDefinition.IsValid;

        public DimmSlotDefinition MemorySlotDefinition => _memorySlotDefinition;

        public StableId<AssemblySlotIdScope> MemorySlotId => _memorySlotDefinition.SlotId;

        public StableId<AssemblyRetentionIdScope> MemoryRetentionId =>
            _memorySlotDefinition.RetentionId;

        public StableId<ContainerIdScope> MemorySlotContainerId =>
            _memorySlotDefinition.ContainerId;

        public StableId<AssemblyMemoryChannelIdScope> MemoryChannelId =>
            _memorySlotDefinition.ChannelId;

        public StableId<AssemblyMemoryBankIdScope> MemoryBankId =>
            _memorySlotDefinition.BankId;

        public int MemoryPopulationPriority => _memorySlotDefinition.PopulationPriority;

        public DimmType SupportedDimmType => _memorySlotDefinition.SupportedDimmType;

        public MemorySlotState MemorySlotState => _memorySlotState;

        public StableId<ItemInstanceIdScope> MemoryItemId => _memoryItemId;

        public StableId<ProductDefinitionIdScope> MemoryProductId => _memoryProductId;

        public StableId<AssemblyOperationIdScope> MemorySeatedByOperationId =>
            _memorySeatedByOperationId;

        public StableId<AssemblyOperationIdScope> MemoryRetainedByOperationId =>
            _memoryRetainedByOperationId;

        public static OperationResult<AssemblyBuildAuthority>
            CreateWithProcessorSocketAndMemorySlot(
                PcComponentCatalog componentCatalog,
                InventoryAuthority inventory,
                StableId<PcBuildIdScope> buildId,
                StableId<ChassisIdScope> chassisId,
                StableId<AssemblySlotIdScope> motherboardSlotId,
                StableId<AssemblyFastenerIdScope> motherboardFastenerId,
                StableId<AssemblySlotIdScope> processorSlotId,
                StableId<AssemblyRetentionIdScope> processorRetentionId,
                DimmSlotDefinition memorySlotDefinition,
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

            if (motherboardSlotId.IsEmpty ||
                processorSlotId.IsEmpty ||
                motherboardSlotId == processorSlotId ||
                motherboardSlotId == memorySlotDefinition.SlotId ||
                processorSlotId == memorySlotDefinition.SlotId)
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
                processorRetentionId == memorySlotDefinition.RetentionId)
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

            if (handsContainerId == workbenchContainerId ||
                handsContainerId == processorSocketContainerId ||
                handsContainerId == memorySlotDefinition.ContainerId ||
                workbenchContainerId == processorSocketContainerId ||
                workbenchContainerId == memorySlotDefinition.ContainerId ||
                processorSocketContainerId == memorySlotDefinition.ContainerId)
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

            OperationResult<InventorySerializedTransferAccessTriple> accessTriple =
                inventory.ClaimManagedSerializedTransferContainers(
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId);
            if (accessTriple.IsFailure)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    accessTriple.Error == InventoryFailures.RevisionOverflow
                        ? AssemblyFailures.RevisionOverflow
                        : accessTriple.Error ==
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
                    accessTriple.Value.First,
                    processorSlotId,
                    processorRetentionId,
                    processorSocketContainerId,
                    supportedCpuSocketFamily,
                    accessTriple.Value.Second,
                    memorySlotDefinition,
                    accessTriple.Value.Third));
        }

        public OperationResult<AssemblyOperationReceipt> SeatMemoryModule(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            DimmKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesSeatMemoryModule(
                        itemId,
                        slotId,
                        orientation,
                        sourceMotherboardAttachOperationId,
                        sourceMotherboardSecureOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateSeatMemoryModule(
                itemId,
                slotId,
                orientation,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _memorySlotDefinition.ContainerId,
                    _memoryInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapMemoryInventoryFailure(prepared.Error, seating: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapMemoryInventoryFailure(committed.Error, seating: true));
            }

            _memorySlotState = MemorySlotState.MemoryModuleSeatedOpen;
            _memoryItemId = item.Id;
            _memoryProductId = item.ProductId;
            _memorySeatedByOperationId = operationId;
            _memoryRetainedByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.SeatMemoryModule,
                BuildId,
                ChassisId,
                _memorySlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _memorySlotDefinition.ContainerId,
                sourceMotherboardAttachOperationId,
                sourceMotherboardSecureOperationId,
                default,
                default,
                default,
                default,
                -1,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                _processorSocketState,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                MemorySlotState.EmptyOpen,
                _memorySlotState,
                default,
                default,
                orientation);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> CloseMemoryRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesCloseMemoryRetention(
                        itemId,
                        slotId,
                        retentionId,
                        sourceMemorySeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateMemoryRetention(
                itemId,
                slotId,
                retentionId,
                sourceMemorySeatOperationId,
                default,
                expectedAssemblyRevision,
                closing: true);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt = _receipts[sourceMemorySeatOperationId];
            _memorySlotState = MemorySlotState.MemoryModuleRetained;
            _memoryRetainedByOperationId = operationId;
            Revision++;

            var receipt = CreateMemoryRetentionReceipt(
                operationId,
                AssemblyOperationKind.CloseMemoryRetention,
                seatReceipt,
                retentionId,
                sourceMemorySeatOperationId,
                default,
                expectedAssemblyRevision,
                MemorySlotState.MemoryModuleSeatedOpen,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> OpenMemoryRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            StableId<AssemblyOperationIdScope> sourceMemoryRetentionOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesOpenMemoryRetention(
                        itemId,
                        slotId,
                        retentionId,
                        sourceMemorySeatOperationId,
                        sourceMemoryRetentionOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateMemoryRetention(
                itemId,
                slotId,
                retentionId,
                sourceMemorySeatOperationId,
                sourceMemoryRetentionOperationId,
                expectedAssemblyRevision,
                closing: false);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt = _receipts[sourceMemorySeatOperationId];
            _memorySlotState = MemorySlotState.MemoryModuleSeatedOpen;
            _memoryRetainedByOperationId = default;
            Revision++;

            var receipt = CreateMemoryRetentionReceipt(
                operationId,
                AssemblyOperationKind.OpenMemoryRetention,
                seatReceipt,
                retentionId,
                sourceMemorySeatOperationId,
                sourceMemoryRetentionOperationId,
                expectedAssemblyRevision,
                MemorySlotState.MemoryModuleRetained,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveMemoryModule(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesRemoveMemoryModule(
                        itemId,
                        slotId,
                        sourceMemorySeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateRemoveMemoryModule(
                itemId,
                slotId,
                sourceMemorySeatOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            AssemblyOperationReceipt seatReceipt = _receipts[sourceMemorySeatOperationId];
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _memoryInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapMemoryInventoryFailure(prepared.Error, seating: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapMemoryInventoryFailure(committed.Error, seating: false));
            }

            _memorySlotState = MemorySlotState.EmptyOpen;
            _memoryItemId = default;
            _memoryProductId = default;
            _memorySeatedByOperationId = default;
            _memoryRetainedByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.RemoveMemoryModule,
                BuildId,
                ChassisId,
                _memorySlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _memorySlotDefinition.ContainerId,
                _handsContainerId,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                default,
                default,
                default,
                default,
                -1,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                _processorSocketState,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                MemorySlotState.MemoryModuleSeatedOpen,
                _memorySlotState,
                sourceMemorySeatOperationId);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        private AssemblyOperationReceipt CreateMemoryRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationKind operationKind,
            AssemblyOperationReceipt seatReceipt,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            StableId<AssemblyOperationIdScope> sourceMemoryRetentionOperationId,
            long expectedAssemblyRevision,
            MemorySlotState previousMemorySlotState,
            MemorySlotState resultingMemorySlotState)
        {
            return new AssemblyOperationReceipt(
                operationId,
                operationKind,
                BuildId,
                ChassisId,
                _memorySlotDefinition.SlotId,
                _memoryItemId,
                _memoryProductId,
                default,
                default,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                default,
                retentionId,
                default,
                default,
                0,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                _processorSocketState,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                previousMemorySlotState,
                resultingMemorySlotState,
                sourceMemorySeatOperationId,
                sourceMemoryRetentionOperationId);
        }

        private Failure ValidateSeatMemoryModule(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            DimmKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            if (!HasMemorySlot)
            {
                return AssemblyFailures.InvalidMemorySlotDefinition;
            }

            if (slotId != _memorySlotDefinition.SlotId)
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

            if (_memorySlotState != MemorySlotState.EmptyOpen)
            {
                return AssemblyFailures.MemorySlotOccupied;
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
                    out PcComponentSpecification memorySpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification))
            {
                return AssemblyFailures.UnknownComponentSpecification;
            }

            AssemblyCompatibilityResult compatibility =
                AssemblyCompatibilityEvaluator.EvaluateMemoryModuleSeat(
                    memorySpecification,
                    motherboardSpecification,
                    _memorySlotDefinition.SupportedDimmType,
                    orientation);
            return compatibility.IsCompatible ? Failure.None : compatibility.Reason;
        }

        private Failure ValidateMemoryRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            StableId<AssemblyOperationIdScope> sourceMemoryRetentionOperationId,
            long expectedAssemblyRevision,
            bool closing)
        {
            if (!HasMemorySlot || slotId != _memorySlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (retentionId != _memorySlotDefinition.RetentionId)
            {
                return AssemblyFailures.InvalidRetention;
            }

            if (itemId.IsEmpty || (!_memoryItemId.IsEmpty && itemId != _memoryItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceMemorySeatOperationId.IsEmpty ||
                sourceMemorySeatOperationId != _memorySeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceMemorySeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatMemoryModule ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (closing && _motherboardSeatState != AssemblySeatState.SeatedSecured)
            {
                return AssemblyFailures.MotherboardUnsecured;
            }

            if (closing)
            {
                if (!sourceMemoryRetentionOperationId.IsEmpty ||
                    _memorySlotState != MemorySlotState.MemoryModuleSeatedOpen ||
                    !_memoryRetainedByOperationId.IsEmpty)
                {
                    return AssemblyFailures.MemoryRetentionOutOfOrder;
                }
            }
            else if (sourceMemoryRetentionOperationId.IsEmpty ||
                     sourceMemoryRetentionOperationId != _memoryRetainedByOperationId ||
                     _memorySlotState != MemorySlotState.MemoryModuleRetained ||
                     !_receipts.TryGetValue(
                         sourceMemoryRetentionOperationId,
                         out AssemblyOperationReceipt retentionReceipt) ||
                     retentionReceipt.OperationKind !=
                         AssemblyOperationKind.CloseMemoryRetention ||
                     retentionReceipt.ItemId != itemId ||
                     retentionReceipt.RetentionId != retentionId ||
                     retentionReceipt.SourceMemorySeatOperationId !=
                         sourceMemorySeatOperationId)
            {
                return AssemblyFailures.MemoryRetentionOutOfOrder;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _memoryProductId &&
                   item.ContainerId == _memorySlotDefinition.ContainerId
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private Failure ValidateRemoveMemoryModule(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceMemorySeatOperationId,
            long expectedAssemblyRevision)
        {
            if (!HasMemorySlot || slotId != _memorySlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceMemorySeatOperationId.IsEmpty ||
                sourceMemorySeatOperationId != _memorySeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceMemorySeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatMemoryModule ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (_memorySlotState == MemorySlotState.MemoryModuleRetained)
            {
                return AssemblyFailures.MemoryModuleRetained;
            }

            if (_memorySlotState != MemorySlotState.MemoryModuleSeatedOpen ||
                itemId != _memoryItemId)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _memoryProductId &&
                   item.ContainerId == _memorySlotDefinition.ContainerId
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private bool ValidateMemoryStateInvariants()
        {
            if (!HasMemorySlot)
            {
                return _memoryInventoryTransferAccess == null &&
                       _memorySlotState == MemorySlotState.Unsupported &&
                       _memorySlotDefinition.SlotId.IsEmpty &&
                       _memorySlotDefinition.RetentionId.IsEmpty &&
                       _memorySlotDefinition.ContainerId.IsEmpty &&
                       _memorySlotDefinition.ChannelId.IsEmpty &&
                       _memorySlotDefinition.BankId.IsEmpty &&
                       _memorySlotDefinition.PopulationPriority == 0 &&
                       _memorySlotDefinition.SupportedDimmType == default &&
                       _memoryItemId.IsEmpty &&
                       _memoryProductId.IsEmpty &&
                       _memorySeatedByOperationId.IsEmpty &&
                       _memoryRetainedByOperationId.IsEmpty;
            }

            if (_memoryInventoryTransferAccess == null ||
                _memorySlotDefinition.SlotId == MotherboardSlotId ||
                _memorySlotDefinition.SlotId == _processorSlotId ||
                _memorySlotDefinition.RetentionId == _processorRetentionId ||
                _memorySlotDefinition.ContainerId == _handsContainerId ||
                _memorySlotDefinition.ContainerId == _workbenchContainerId ||
                _memorySlotDefinition.ContainerId == _processorSocketContainerId ||
                !_inventory.TryGetContainer(
                    _memorySlotDefinition.ContainerId,
                    out InventoryContainerDefinition memorySlot) ||
                memorySlot.Kind != InventoryContainerKind.Workbench ||
                memorySlot.UnitCapacity != 1)
            {
                return false;
            }

            if (_memorySlotState == MemorySlotState.EmptyOpen)
            {
                return _memoryItemId.IsEmpty &&
                       _memoryProductId.IsEmpty &&
                       _memorySeatedByOperationId.IsEmpty &&
                       _memoryRetainedByOperationId.IsEmpty &&
                       _inventory.GetContainerQuantity(
                           _memorySlotDefinition.ContainerId).Value == 0;
            }

            if (_memorySlotState != MemorySlotState.MemoryModuleSeatedOpen &&
                _memorySlotState != MemorySlotState.MemoryModuleRetained)
            {
                return false;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty ||
                _memoryItemId.IsEmpty ||
                _memoryProductId.IsEmpty ||
                _memorySeatedByOperationId.IsEmpty ||
                !_inventory.TryGetSerializedItem(
                    _memoryItemId,
                    out InventoryItemRecord memoryItem) ||
                memoryItem.ProductId != _memoryProductId ||
                memoryItem.ContainerId != _memorySlotDefinition.ContainerId ||
                !_componentCatalog.TryGet(
                    memoryItem.ProductId,
                    out PcComponentSpecification memorySpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification) ||
                !AssemblyCompatibilityEvaluator.EvaluateMemoryModuleSeat(
                    memorySpecification,
                    motherboardSpecification,
                    _memorySlotDefinition.SupportedDimmType,
                    DimmKeyOrientation.NotchAligned).IsCompatible ||
                !_receipts.TryGetValue(
                    _memorySeatedByOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatMemoryModule ||
                seatReceipt.ItemId != _memoryItemId ||
                seatReceipt.ProductId != _memoryProductId ||
                seatReceipt.SlotId != _memorySlotDefinition.SlotId ||
                seatReceipt.DimmKeyOrientation != DimmKeyOrientation.NotchAligned)
            {
                return false;
            }

            if (_memorySlotState == MemorySlotState.MemoryModuleSeatedOpen)
            {
                return _memoryRetainedByOperationId.IsEmpty;
            }

            return !_memoryRetainedByOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       _memoryRetainedByOperationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.CloseMemoryRetention &&
                   retentionReceipt.ItemId == _memoryItemId &&
                   retentionReceipt.RetentionId == _memorySlotDefinition.RetentionId &&
                   retentionReceipt.SourceMemorySeatOperationId ==
                       _memorySeatedByOperationId;
        }

        private bool IsMatchingMemorySeatReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt seatReceipt) &&
                   seatReceipt.OperationKind == AssemblyOperationKind.SeatMemoryModule &&
                   seatReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   seatReceipt.ItemId == descendant.ItemId &&
                   seatReceipt.ProductId == descendant.ProductId &&
                   seatReceipt.SlotId == descendant.SlotId &&
                   seatReceipt.SourceAttachOperationId ==
                       descendant.SourceAttachOperationId &&
                   seatReceipt.SourceSecureOperationId ==
                       descendant.SourceSecureOperationId &&
                   seatReceipt.DimmKeyOrientation == DimmKeyOrientation.NotchAligned;
        }

        private bool IsMatchingMemoryRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.CloseMemoryRetention &&
                   retentionReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   retentionReceipt.ItemId == descendant.ItemId &&
                   retentionReceipt.ProductId == descendant.ProductId &&
                   retentionReceipt.SlotId == descendant.SlotId &&
                   retentionReceipt.RetentionId == descendant.RetentionId &&
                   retentionReceipt.SourceMemorySeatOperationId ==
                       descendant.SourceMemorySeatOperationId;
        }

        private static Failure MapMemoryInventoryFailure(Failure failure, bool seating)
        {
            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return seating
                    ? AssemblyFailures.MemorySlotCapacityExceeded
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

            return AssemblyFailures.InventoryTransferRejected;
        }
    }
}
