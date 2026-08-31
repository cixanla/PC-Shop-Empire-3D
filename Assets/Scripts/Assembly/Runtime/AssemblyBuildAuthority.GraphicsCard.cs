using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private GraphicsCardSlotDefinition _graphicsCardSlotDefinition;
        private InventorySerializedTransferAccess _graphicsCardInventoryTransferAccess;
        private GraphicsCardSlotState _graphicsCardSlotState =
            GraphicsCardSlotState.Unsupported;
        private StableId<ItemInstanceIdScope> _graphicsCardItemId;
        private StableId<ProductDefinitionIdScope> _graphicsCardProductId;
        private StableId<AssemblyOperationIdScope> _graphicsCardSeatedByOperationId;
        private StableId<AssemblyOperationIdScope> _graphicsCardRetainedByOperationId;
        private GraphicsCardMountOrientation _graphicsCardMountOrientation;

        public bool HasGraphicsCardSlot => _graphicsCardSlotDefinition.IsValid;

        public GraphicsCardSlotDefinition GraphicsCardSlotDefinition =>
            _graphicsCardSlotDefinition;

        public StableId<AssemblySlotIdScope> GraphicsCardSlotId =>
            _graphicsCardSlotDefinition.SlotId;

        public StableId<ContainerIdScope> GraphicsCardSlotContainerId =>
            _graphicsCardSlotDefinition.ContainerId;

        public GraphicsCardRetentionTopology GraphicsCardRetentionTopology =>
            _graphicsCardSlotDefinition.RetentionTopology;

        public GraphicsCardType SupportedGraphicsCardType =>
            _graphicsCardSlotDefinition.SupportedGraphicsCardType;

        public GraphicsCardSlotState GraphicsCardSlotState =>
            _graphicsCardSlotState;

        public StableId<ItemInstanceIdScope> GraphicsCardItemId =>
            _graphicsCardItemId;

        public StableId<ProductDefinitionIdScope> GraphicsCardProductId =>
            _graphicsCardProductId;

        public StableId<AssemblyOperationIdScope> GraphicsCardSeatedByOperationId =>
            _graphicsCardSeatedByOperationId;

        public StableId<AssemblyOperationIdScope> GraphicsCardRetainedByOperationId =>
            _graphicsCardRetainedByOperationId;

        public GraphicsCardMountOrientation GraphicsCardMountOrientation =>
            _graphicsCardMountOrientation;

        /// <summary>
        /// Creates the canonical aggregate with one capacity-one graphics-card slot.
        /// All six managed assembly containers are claimed in one inventory revision;
        /// any validation or claim failure leaves all six unmanaged.
        /// </summary>
        public static OperationResult<AssemblyBuildAuthority>
            CreateWithProcessorSocketMemoryStorageCoolerAndGraphicsCardSlots(
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

            if (!graphicsCardSlotDefinition.IsValid)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidGraphicsCardSlotDefinition);
            }

            if (HasDuplicateGraphicsCardFactorySlot(
                    motherboardSlotId,
                    processorSlotId,
                    memorySlotDefinition.SlotId,
                    storageSlotDefinition.SlotId,
                    processorCoolerSlotDefinition.SlotId,
                    graphicsCardSlotDefinition.SlotId))
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (motherboardFastenerId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidFastener);
            }

            if (graphicsCardSlotDefinition.RetentionTopology
                    .BracketFastenerId == motherboardFastenerId)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidGraphicsCardBracketFastener);
            }

            if (processorRetentionId.IsEmpty ||
                processorRetentionId == memorySlotDefinition.RetentionId ||
                processorRetentionId == storageSlotDefinition.CaptiveScrewId ||
                memorySlotDefinition.RetentionId ==
                    storageSlotDefinition.CaptiveScrewId)
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
                !IsCapacityOneWorkbenchContainer(
                    inventory,
                    processorSocketContainerId))
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

            if (HasDuplicateGraphicsCardFactoryContainer(
                    handsContainerId,
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId,
                    graphicsCardSlotDefinition.ContainerId))
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

            if (inventory.GetContainerQuantity(
                    memorySlotDefinition.ContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.MemorySlotOccupied);
            }

            if (inventory.GetContainerQuantity(
                    storageSlotDefinition.ContainerId).Value != 0)
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

            OperationResult<InventorySerializedTransferAccessSextuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId,
                    processorCoolerSlotDefinition.ContainerId,
                    graphicsCardSlotDefinition.ContainerId);
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
                    access.Value.Sixth));
        }

        private static bool IsCapacityOneWorkbenchContainer(
            InventoryAuthority inventory,
            StableId<ContainerIdScope> containerId)
        {
            return !containerId.IsEmpty &&
                   inventory.TryGetContainer(
                       containerId,
                       out InventoryContainerDefinition container) &&
                   container.Kind == InventoryContainerKind.Workbench &&
                   container.UnitCapacity == 1;
        }

        private static bool HasDuplicateGraphicsCardFactorySlot(
            StableId<AssemblySlotIdScope> first,
            StableId<AssemblySlotIdScope> second,
            StableId<AssemblySlotIdScope> third,
            StableId<AssemblySlotIdScope> fourth,
            StableId<AssemblySlotIdScope> fifth,
            StableId<AssemblySlotIdScope> sixth)
        {
            if (first.IsEmpty || second.IsEmpty || third.IsEmpty || fourth.IsEmpty ||
                fifth.IsEmpty || sixth.IsEmpty)
            {
                return true;
            }

            return first == second || first == third || first == fourth ||
                   first == fifth || first == sixth || second == third ||
                   second == fourth || second == fifth || second == sixth ||
                   third == fourth || third == fifth || third == sixth ||
                   fourth == fifth || fourth == sixth || fifth == sixth;
        }

        private static bool HasDuplicateGraphicsCardFactoryContainer(
            StableId<ContainerIdScope> first,
            StableId<ContainerIdScope> second,
            StableId<ContainerIdScope> third,
            StableId<ContainerIdScope> fourth,
            StableId<ContainerIdScope> fifth,
            StableId<ContainerIdScope> sixth,
            StableId<ContainerIdScope> seventh)
        {
            return first == second || first == third || first == fourth ||
                   first == fifth || first == sixth || first == seventh ||
                   second == third || second == fourth || second == fifth ||
                   second == sixth || second == seventh || third == fourth ||
                   third == fifth || third == sixth || third == seventh ||
                   fourth == fifth || fourth == sixth || fourth == seventh ||
                   fifth == sixth || fifth == seventh || sixth == seventh;
        }

        public OperationResult<AssemblyOperationReceipt> SeatGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            GraphicsCardMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
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
                return replay.MatchesSeatGraphicsCard(
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

            Failure preflightFailure = ValidateSeatGraphicsCard(
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
                    _graphicsCardSlotDefinition.ContainerId,
                    _graphicsCardInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapGraphicsCardInventoryFailure(prepared.Error, seating: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapGraphicsCardInventoryFailure(committed.Error, seating: true));
            }

            _graphicsCardSlotState =
                GraphicsCardSlotState.GraphicsCardSeatedUnsecured;
            _graphicsCardItemId = item.Id;
            _graphicsCardProductId = item.ProductId;
            _graphicsCardSeatedByOperationId = operationId;
            _graphicsCardRetainedByOperationId = default;
            _graphicsCardMountOrientation = orientation;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.SeatGraphicsCard,
                BuildId,
                ChassisId,
                _graphicsCardSlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _graphicsCardSlotDefinition.ContainerId,
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
                _processorCoolerSlotState,
                _processorCoolerSlotState,
                default,
                default,
                _processorCoolerMountOrientation,
                _processorCoolerTimState,
                _processorCoolerTimState,
                _processorCoolerSlotDefinition,
                GraphicsCardSlotState.EmptyOpen,
                _graphicsCardSlotState,
                default,
                default,
                orientation,
                _graphicsCardSlotDefinition);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RetainGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyGraphicsCardLatchIdScope> latchId,
            StableId<AssemblyFastenerIdScope> bracketFastenerId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
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
                return replay.MatchesRetainGraphicsCard(
                        itemId,
                        slotId,
                        latchId,
                        bracketFastenerId,
                        sourceGraphicsCardSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateGraphicsCardRetention(
                itemId,
                slotId,
                latchId,
                bracketFastenerId,
                sourceGraphicsCardSeatOperationId,
                default,
                expectedAssemblyRevision,
                retaining: true);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceGraphicsCardSeatOperationId];
            _graphicsCardSlotState = GraphicsCardSlotState.GraphicsCardRetained;
            _graphicsCardRetainedByOperationId = operationId;
            Revision++;

            AssemblyOperationReceipt receipt = CreateGraphicsCardRetentionReceipt(
                operationId,
                AssemblyOperationKind.RetainGraphicsCard,
                seatReceipt,
                sourceGraphicsCardSeatOperationId,
                default,
                expectedAssemblyRevision,
                GraphicsCardSlotState.GraphicsCardSeatedUnsecured,
                _graphicsCardSlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> UnretainGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyGraphicsCardLatchIdScope> latchId,
            StableId<AssemblyFastenerIdScope> bracketFastenerId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceGraphicsCardRetentionOperationId,
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
                return replay.MatchesUnretainGraphicsCard(
                        itemId,
                        slotId,
                        latchId,
                        bracketFastenerId,
                        sourceGraphicsCardSeatOperationId,
                        sourceGraphicsCardRetentionOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    maintenanceFailure);
            }

            if (IsPcieGpuPowerCableRouted)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.PowerCableDependentComponentLocked);
            }

            Failure preflightFailure = ValidateGraphicsCardRetention(
                itemId,
                slotId,
                latchId,
                bracketFastenerId,
                sourceGraphicsCardSeatOperationId,
                sourceGraphicsCardRetentionOperationId,
                expectedAssemblyRevision,
                retaining: false);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceGraphicsCardSeatOperationId];
            _graphicsCardSlotState =
                GraphicsCardSlotState.GraphicsCardSeatedUnsecured;
            _graphicsCardRetainedByOperationId = default;
            Revision++;

            AssemblyOperationReceipt receipt = CreateGraphicsCardRetentionReceipt(
                operationId,
                AssemblyOperationKind.UnretainGraphicsCard,
                seatReceipt,
                sourceGraphicsCardSeatOperationId,
                sourceGraphicsCardRetentionOperationId,
                expectedAssemblyRevision,
                GraphicsCardSlotState.GraphicsCardRetained,
                _graphicsCardSlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveGraphicsCard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
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
                return replay.MatchesRemoveGraphicsCard(
                        itemId,
                        slotId,
                        sourceGraphicsCardSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    maintenanceFailure);
            }

            if (IsPcieGpuPowerCableRouted)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.PowerCableDependentComponentLocked);
            }

            Failure preflightFailure = ValidateRemoveGraphicsCard(
                itemId,
                slotId,
                sourceGraphicsCardSeatOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceGraphicsCardSeatOperationId];
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _graphicsCardInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapGraphicsCardInventoryFailure(prepared.Error, seating: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapGraphicsCardInventoryFailure(committed.Error, seating: false));
            }

            GraphicsCardMountOrientation removedOrientation =
                _graphicsCardMountOrientation;
            _graphicsCardSlotState = GraphicsCardSlotState.EmptyOpen;
            _graphicsCardItemId = default;
            _graphicsCardProductId = default;
            _graphicsCardSeatedByOperationId = default;
            _graphicsCardRetainedByOperationId = default;
            _graphicsCardMountOrientation = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.RemoveGraphicsCard,
                BuildId,
                ChassisId,
                _graphicsCardSlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _graphicsCardSlotDefinition.ContainerId,
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
                _processorCoolerSlotState,
                _processorCoolerSlotState,
                default,
                default,
                _processorCoolerMountOrientation,
                _processorCoolerTimState,
                _processorCoolerTimState,
                _processorCoolerSlotDefinition,
                GraphicsCardSlotState.GraphicsCardSeatedUnsecured,
                _graphicsCardSlotState,
                sourceGraphicsCardSeatOperationId,
                default,
                removedOrientation,
                _graphicsCardSlotDefinition);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        private AssemblyOperationReceipt CreateGraphicsCardRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationKind operationKind,
            AssemblyOperationReceipt seatReceipt,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceGraphicsCardRetentionOperationId,
            long expectedAssemblyRevision,
            GraphicsCardSlotState previousGraphicsCardSlotState,
            GraphicsCardSlotState resultingGraphicsCardSlotState)
        {
            return new AssemblyOperationReceipt(
                operationId,
                operationKind,
                BuildId,
                ChassisId,
                _graphicsCardSlotDefinition.SlotId,
                _graphicsCardItemId,
                _graphicsCardProductId,
                default,
                default,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                _graphicsCardSlotDefinition.RetentionTopology.BracketFastenerId,
                default,
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
                _processorCoolerSlotState,
                _processorCoolerSlotState,
                default,
                default,
                _processorCoolerMountOrientation,
                _processorCoolerTimState,
                _processorCoolerTimState,
                _processorCoolerSlotDefinition,
                previousGraphicsCardSlotState,
                resultingGraphicsCardSlotState,
                sourceGraphicsCardSeatOperationId,
                sourceGraphicsCardRetentionOperationId,
                _graphicsCardMountOrientation,
                _graphicsCardSlotDefinition);
        }

        private Failure ValidateSeatGraphicsCard(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            GraphicsCardMountOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasGraphicsCardSlot)
            {
                return AssemblyFailures.InvalidGraphicsCardSlotDefinition;
            }

            if (slotId != _graphicsCardSlotDefinition.SlotId)
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

            if (_graphicsCardSlotState != GraphicsCardSlotState.EmptyOpen)
            {
                return AssemblyFailures.GraphicsCardSlotOccupied;
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
                    out PcComponentSpecification graphicsCardSpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification))
            {
                return AssemblyFailures.UnknownComponentSpecification;
            }

            AssemblyCompatibilityResult compatibility =
                AssemblyCompatibilityEvaluator.EvaluateGraphicsCardSeat(
                    graphicsCardSpecification,
                    motherboardSpecification,
                    _graphicsCardSlotDefinition.SupportedGraphicsCardType,
                    orientation);
            return compatibility.IsCompatible ? Failure.None : compatibility.Reason;
        }

        private Failure ValidateGraphicsCardRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyGraphicsCardLatchIdScope> latchId,
            StableId<AssemblyFastenerIdScope> bracketFastenerId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            StableId<AssemblyOperationIdScope>
                sourceGraphicsCardRetentionOperationId,
            long expectedAssemblyRevision,
            bool retaining)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasGraphicsCardSlot ||
                slotId != _graphicsCardSlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (latchId != _graphicsCardSlotDefinition.RetentionTopology.LatchId)
            {
                return AssemblyFailures.InvalidGraphicsCardSlotLatch;
            }

            if (bracketFastenerId !=
                _graphicsCardSlotDefinition.RetentionTopology.BracketFastenerId)
            {
                return AssemblyFailures.InvalidGraphicsCardBracketFastener;
            }

            if (itemId.IsEmpty ||
                (!_graphicsCardItemId.IsEmpty && itemId != _graphicsCardItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceGraphicsCardSeatOperationId.IsEmpty ||
                sourceGraphicsCardSeatOperationId !=
                    _graphicsCardSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceGraphicsCardSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatGraphicsCard ||
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

            if (retaining)
            {
                if (!sourceGraphicsCardRetentionOperationId.IsEmpty ||
                    _graphicsCardSlotState !=
                        GraphicsCardSlotState.GraphicsCardSeatedUnsecured ||
                    !_graphicsCardRetainedByOperationId.IsEmpty)
                {
                    return AssemblyFailures.GraphicsCardRetentionOutOfOrder;
                }
            }
            else if (sourceGraphicsCardRetentionOperationId.IsEmpty ||
                     sourceGraphicsCardRetentionOperationId !=
                         _graphicsCardRetainedByOperationId ||
                     _graphicsCardSlotState !=
                         GraphicsCardSlotState.GraphicsCardRetained ||
                     !_receipts.TryGetValue(
                         sourceGraphicsCardRetentionOperationId,
                         out AssemblyOperationReceipt retentionReceipt) ||
                     retentionReceipt.OperationKind !=
                         AssemblyOperationKind.RetainGraphicsCard ||
                     retentionReceipt.ItemId != itemId ||
                     retentionReceipt.SourceGraphicsCardSeatOperationId !=
                         sourceGraphicsCardSeatOperationId ||
                     !retentionReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                         _graphicsCardSlotDefinition))
            {
                return AssemblyFailures.GraphicsCardRetentionOutOfOrder;
            }

            return IsGraphicsCardSeatedItem(itemId)
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private Failure ValidateRemoveGraphicsCard(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardSeatOperationId,
            long expectedAssemblyRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasGraphicsCardSlot ||
                slotId != _graphicsCardSlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceGraphicsCardSeatOperationId.IsEmpty ||
                sourceGraphicsCardSeatOperationId !=
                    _graphicsCardSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceGraphicsCardSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatGraphicsCard ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (_graphicsCardSlotState ==
                GraphicsCardSlotState.GraphicsCardRetained)
            {
                return AssemblyFailures.GraphicsCardRetained;
            }

            if (_graphicsCardSlotState !=
                    GraphicsCardSlotState.GraphicsCardSeatedUnsecured ||
                itemId != _graphicsCardItemId)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            return IsGraphicsCardSeatedItem(itemId)
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        /// <summary>
        /// Called by the shared motherboard detach preflight after the existing
        /// processor/memory/storage/cooler gates, preserving their established failure
        /// precedence.
        /// </summary>
        private Failure ValidateGraphicsCardMotherboardDetachGate()
        {
            return HasGraphicsCardSlot &&
                   _graphicsCardSlotState != GraphicsCardSlotState.EmptyOpen
                ? AssemblyFailures.GraphicsCardInstalled
                : Failure.None;
        }

        private bool ValidateGraphicsCardStateInvariants()
        {
            if (!HasGraphicsCardSlot)
            {
                return _graphicsCardInventoryTransferAccess == null &&
                       _graphicsCardSlotState == GraphicsCardSlotState.Unsupported &&
                       _graphicsCardSlotDefinition.SlotId.IsEmpty &&
                       _graphicsCardSlotDefinition.ContainerId.IsEmpty &&
                       _graphicsCardSlotDefinition.RetentionTopology == null &&
                       _graphicsCardSlotDefinition.SupportedGraphicsCardType == default &&
                       _graphicsCardItemId.IsEmpty &&
                       _graphicsCardProductId.IsEmpty &&
                       _graphicsCardSeatedByOperationId.IsEmpty &&
                       _graphicsCardRetainedByOperationId.IsEmpty &&
                       _graphicsCardMountOrientation == default;
            }

            if (_graphicsCardInventoryTransferAccess == null ||
                _graphicsCardSlotDefinition.SlotId == MotherboardSlotId ||
                _graphicsCardSlotDefinition.SlotId == _processorSlotId ||
                _graphicsCardSlotDefinition.SlotId == _memorySlotDefinition.SlotId ||
                _graphicsCardSlotDefinition.SlotId == _storageSlotDefinition.SlotId ||
                _graphicsCardSlotDefinition.SlotId ==
                    _processorCoolerSlotDefinition.SlotId ||
                _graphicsCardSlotDefinition.ContainerId == _handsContainerId ||
                _graphicsCardSlotDefinition.ContainerId == _workbenchContainerId ||
                _graphicsCardSlotDefinition.ContainerId ==
                    _processorSocketContainerId ||
                _graphicsCardSlotDefinition.ContainerId ==
                    _memorySlotDefinition.ContainerId ||
                _graphicsCardSlotDefinition.ContainerId ==
                    _storageSlotDefinition.ContainerId ||
                _graphicsCardSlotDefinition.ContainerId ==
                    _processorCoolerSlotDefinition.ContainerId ||
                _graphicsCardSlotDefinition.RetentionTopology.BracketFastenerId ==
                    _motherboardFastenerId ||
                !IsCapacityOneWorkbenchContainer(
                    _inventory,
                    _graphicsCardSlotDefinition.ContainerId))
            {
                return false;
            }

            if (_graphicsCardSlotState == GraphicsCardSlotState.EmptyOpen)
            {
                return _graphicsCardItemId.IsEmpty &&
                       _graphicsCardProductId.IsEmpty &&
                       _graphicsCardSeatedByOperationId.IsEmpty &&
                       _graphicsCardRetainedByOperationId.IsEmpty &&
                       _graphicsCardMountOrientation == default &&
                       _inventory.GetContainerQuantity(
                           _graphicsCardSlotDefinition.ContainerId).Value == 0;
            }

            if ((_graphicsCardSlotState !=
                    GraphicsCardSlotState.GraphicsCardSeatedUnsecured &&
                 _graphicsCardSlotState !=
                    GraphicsCardSlotState.GraphicsCardRetained) ||
                _motherboardSeatState == AssemblySeatState.Empty ||
                !IsGraphicsCardSeatedItem(_graphicsCardItemId) ||
                !_componentCatalog.TryGet(
                    _graphicsCardProductId,
                    out PcComponentSpecification graphicsCardSpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification) ||
                !AssemblyCompatibilityEvaluator.EvaluateGraphicsCardSeat(
                    graphicsCardSpecification,
                    motherboardSpecification,
                    _graphicsCardSlotDefinition.SupportedGraphicsCardType,
                    _graphicsCardMountOrientation).IsCompatible ||
                !_receipts.TryGetValue(
                    _graphicsCardSeatedByOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatGraphicsCard ||
                seatReceipt.ItemId != _graphicsCardItemId ||
                seatReceipt.ProductId != _graphicsCardProductId ||
                seatReceipt.SlotId != _graphicsCardSlotDefinition.SlotId ||
                seatReceipt.GraphicsCardMountOrientation !=
                    _graphicsCardMountOrientation ||
                !seatReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                    _graphicsCardSlotDefinition))
            {
                return false;
            }

            if (_graphicsCardSlotState ==
                GraphicsCardSlotState.GraphicsCardSeatedUnsecured)
            {
                return _graphicsCardRetainedByOperationId.IsEmpty;
            }

            return !_graphicsCardRetainedByOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       _graphicsCardRetainedByOperationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.RetainGraphicsCard &&
                   retentionReceipt.ItemId == _graphicsCardItemId &&
                   retentionReceipt.SourceGraphicsCardSeatOperationId ==
                       _graphicsCardSeatedByOperationId &&
                   retentionReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                       _graphicsCardSlotDefinition);
        }

        private bool IsMatchingGraphicsCardSeatReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt seatReceipt) &&
                   seatReceipt.OperationKind == AssemblyOperationKind.SeatGraphicsCard &&
                   seatReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   seatReceipt.ItemId == descendant.ItemId &&
                   seatReceipt.ProductId == descendant.ProductId &&
                   seatReceipt.SlotId == descendant.SlotId &&
                   seatReceipt.SourceAttachOperationId ==
                       descendant.SourceAttachOperationId &&
                   seatReceipt.SourceSecureOperationId ==
                       descendant.SourceSecureOperationId &&
                   seatReceipt.GraphicsCardMountOrientation ==
                       descendant.GraphicsCardMountOrientation &&
                   seatReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                       descendant.GraphicsCardSlotDefinition);
        }

        private bool IsMatchingGraphicsCardRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.RetainGraphicsCard &&
                   retentionReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   retentionReceipt.ItemId == descendant.ItemId &&
                   retentionReceipt.ProductId == descendant.ProductId &&
                   retentionReceipt.SlotId == descendant.SlotId &&
                   retentionReceipt.SourceGraphicsCardSeatOperationId ==
                       descendant.SourceGraphicsCardSeatOperationId &&
                   retentionReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                       descendant.GraphicsCardSlotDefinition);
        }

        private bool IsGraphicsCardSeatedItem(
            StableId<ItemInstanceIdScope> itemId)
        {
            return !itemId.IsEmpty &&
                   itemId == _graphicsCardItemId &&
                   !_graphicsCardProductId.IsEmpty &&
                   !_graphicsCardSeatedByOperationId.IsEmpty &&
                   _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _graphicsCardProductId &&
                   item.ContainerId == _graphicsCardSlotDefinition.ContainerId;
        }

        private static Failure MapGraphicsCardInventoryFailure(
            Failure failure,
            bool seating)
        {
            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return seating
                    ? AssemblyFailures.GraphicsCardSlotCapacityExceeded
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
