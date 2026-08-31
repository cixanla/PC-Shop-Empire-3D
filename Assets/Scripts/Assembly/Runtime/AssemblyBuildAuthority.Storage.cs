using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private M2SlotDefinition _storageSlotDefinition;
        private InventorySerializedTransferAccess _storageInventoryTransferAccess;
        private StorageSlotState _storageSlotState = StorageSlotState.Unsupported;
        private StableId<ItemInstanceIdScope> _storageItemId;
        private StableId<ProductDefinitionIdScope> _storageProductId;
        private StableId<AssemblyOperationIdScope> _storageSeatedByOperationId;
        private StableId<AssemblyOperationIdScope> _storageSecuredByOperationId;

        public bool HasStorageSlot => _storageSlotDefinition.IsValid;

        public M2SlotDefinition StorageSlotDefinition => _storageSlotDefinition;

        public StableId<AssemblySlotIdScope> StorageSlotId => _storageSlotDefinition.SlotId;

        public StableId<AssemblyStorageStandoffIdScope> StorageStandoffId =>
            _storageSlotDefinition.StandoffId;

        public StableId<AssemblyRetentionIdScope> StorageCaptiveScrewId =>
            _storageSlotDefinition.CaptiveScrewId;

        public StableId<ContainerIdScope> StorageSlotContainerId =>
            _storageSlotDefinition.ContainerId;

        public M2StorageType SupportedM2StorageType =>
            _storageSlotDefinition.SupportedStorageType;

        public StorageSlotState StorageSlotState => _storageSlotState;

        public StableId<ItemInstanceIdScope> StorageItemId => _storageItemId;

        public StableId<ProductDefinitionIdScope> StorageProductId => _storageProductId;

        public StableId<AssemblyOperationIdScope> StorageSeatedByOperationId =>
            _storageSeatedByOperationId;

        public StableId<AssemblyOperationIdScope> StorageSecuredByOperationId =>
            _storageSecuredByOperationId;

        public static OperationResult<AssemblyBuildAuthority>
            CreateWithProcessorSocketMemorySlotAndStorageSlot(
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

            if (motherboardSlotId.IsEmpty ||
                processorSlotId.IsEmpty ||
                motherboardSlotId == processorSlotId ||
                motherboardSlotId == memorySlotDefinition.SlotId ||
                motherboardSlotId == storageSlotDefinition.SlotId ||
                processorSlotId == memorySlotDefinition.SlotId ||
                processorSlotId == storageSlotDefinition.SlotId ||
                memorySlotDefinition.SlotId == storageSlotDefinition.SlotId)
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

            if (handsContainerId == workbenchContainerId ||
                handsContainerId == processorSocketContainerId ||
                handsContainerId == memorySlotDefinition.ContainerId ||
                handsContainerId == storageSlotDefinition.ContainerId ||
                workbenchContainerId == processorSocketContainerId ||
                workbenchContainerId == memorySlotDefinition.ContainerId ||
                workbenchContainerId == storageSlotDefinition.ContainerId ||
                processorSocketContainerId == memorySlotDefinition.ContainerId ||
                processorSocketContainerId == storageSlotDefinition.ContainerId ||
                memorySlotDefinition.ContainerId == storageSlotDefinition.ContainerId)
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

            if (inventory.GetContainerQuantity(storageSlotDefinition.ContainerId).Value != 0)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.StorageSlotOccupied);
            }

            OperationResult<InventorySerializedTransferAccessQuadruple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    workbenchContainerId,
                    processorSocketContainerId,
                    memorySlotDefinition.ContainerId,
                    storageSlotDefinition.ContainerId);
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
                    access.Value.Fourth));
        }

        public OperationResult<AssemblyOperationReceipt> SeatStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            M2KeyOrientation orientation,
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
                return replay.MatchesSeatStorageDevice(
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

            Failure preflightFailure = ValidateSeatStorageDevice(
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
                    _storageSlotDefinition.ContainerId,
                    _storageInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapStorageInventoryFailure(prepared.Error, seating: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapStorageInventoryFailure(committed.Error, seating: true));
            }

            _storageSlotState = StorageSlotState.StorageDeviceSeatedUnsecured;
            _storageItemId = item.Id;
            _storageProductId = item.ProductId;
            _storageSeatedByOperationId = operationId;
            _storageSecuredByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.SeatStorageDevice,
                BuildId,
                ChassisId,
                _storageSlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _storageSlotDefinition.ContainerId,
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
                StorageSlotState.EmptyOpen,
                _storageSlotState,
                default,
                default,
                orientation,
                _processorCoolerSlotState,
                _processorCoolerSlotState,
                default,
                default,
                default,
                _processorCoolerTimState,
                _processorCoolerTimState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> SecureStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
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
                return replay.MatchesSecureStorageDevice(
                        itemId,
                        slotId,
                        retentionId,
                        sourceStorageSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateStorageRetention(
                itemId,
                slotId,
                retentionId,
                sourceStorageSeatOperationId,
                default,
                expectedAssemblyRevision,
                closing: true);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt = _receipts[sourceStorageSeatOperationId];
            _storageSlotState = StorageSlotState.StorageDeviceSecured;
            _storageSecuredByOperationId = operationId;
            Revision++;

            var receipt = CreateStorageRetentionReceipt(
                operationId,
                AssemblyOperationKind.SecureStorageDevice,
                seatReceipt,
                retentionId,
                sourceStorageSeatOperationId,
                default,
                expectedAssemblyRevision,
                StorageSlotState.StorageDeviceSeatedUnsecured,
                _storageSlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> UnsecureStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceStorageRetentionOperationId,
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
                return replay.MatchesUnsecureStorageDevice(
                        itemId,
                        slotId,
                        retentionId,
                        sourceStorageSeatOperationId,
                        sourceStorageRetentionOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateStorageRetention(
                itemId,
                slotId,
                retentionId,
                sourceStorageSeatOperationId,
                sourceStorageRetentionOperationId,
                expectedAssemblyRevision,
                closing: false);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt = _receipts[sourceStorageSeatOperationId];
            _storageSlotState = StorageSlotState.StorageDeviceSeatedUnsecured;
            _storageSecuredByOperationId = default;
            Revision++;

            var receipt = CreateStorageRetentionReceipt(
                operationId,
                AssemblyOperationKind.UnsecureStorageDevice,
                seatReceipt,
                retentionId,
                sourceStorageSeatOperationId,
                sourceStorageRetentionOperationId,
                expectedAssemblyRevision,
                StorageSlotState.StorageDeviceSecured,
                _storageSlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveStorageDevice(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
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
                return replay.MatchesRemoveStorageDevice(
                        itemId,
                        slotId,
                        sourceStorageSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateRemoveStorageDevice(
                itemId,
                slotId,
                sourceStorageSeatOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            AssemblyOperationReceipt seatReceipt = _receipts[sourceStorageSeatOperationId];
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _storageInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapStorageInventoryFailure(prepared.Error, seating: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapStorageInventoryFailure(committed.Error, seating: false));
            }

            _storageSlotState = StorageSlotState.EmptyOpen;
            _storageItemId = default;
            _storageProductId = default;
            _storageSeatedByOperationId = default;
            _storageSecuredByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.RemoveStorageDevice,
                BuildId,
                ChassisId,
                _storageSlotDefinition.SlotId,
                item.Id,
                item.ProductId,
                _storageSlotDefinition.ContainerId,
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
                StorageSlotState.StorageDeviceSeatedUnsecured,
                _storageSlotState,
                sourceStorageSeatOperationId,
                default,
                default,
                _processorCoolerSlotState,
                _processorCoolerSlotState,
                default,
                default,
                default,
                _processorCoolerTimState,
                _processorCoolerTimState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        private AssemblyOperationReceipt CreateStorageRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationKind operationKind,
            AssemblyOperationReceipt seatReceipt,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceStorageRetentionOperationId,
            long expectedAssemblyRevision,
            StorageSlotState previousStorageSlotState,
            StorageSlotState resultingStorageSlotState)
        {
            return new AssemblyOperationReceipt(
                operationId,
                operationKind,
                BuildId,
                ChassisId,
                _storageSlotDefinition.SlotId,
                _storageItemId,
                _storageProductId,
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
                _memorySlotState,
                _memorySlotState,
                default,
                default,
                default,
                previousStorageSlotState,
                resultingStorageSlotState,
                sourceStorageSeatOperationId,
                sourceStorageRetentionOperationId,
                default,
                _processorCoolerSlotState,
                _processorCoolerSlotState,
                default,
                default,
                default,
                _processorCoolerTimState,
                _processorCoolerTimState);
        }

        private Failure ValidateSeatStorageDevice(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            M2KeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasStorageSlot)
            {
                return AssemblyFailures.InvalidStorageSlotDefinition;
            }

            if (slotId != _storageSlotDefinition.SlotId)
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

            if (_storageSlotState != StorageSlotState.EmptyOpen)
            {
                return AssemblyFailures.StorageSlotOccupied;
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
                    out PcComponentSpecification storageSpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification))
            {
                return AssemblyFailures.UnknownComponentSpecification;
            }

            AssemblyCompatibilityResult compatibility =
                AssemblyCompatibilityEvaluator.EvaluateStorageDeviceSeat(
                    storageSpecification,
                    motherboardSpecification,
                    _storageSlotDefinition.SupportedStorageType,
                    orientation);
            return compatibility.IsCompatible ? Failure.None : compatibility.Reason;
        }

        private Failure ValidateStorageRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceStorageRetentionOperationId,
            long expectedAssemblyRevision,
            bool closing)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasStorageSlot || slotId != _storageSlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (retentionId != _storageSlotDefinition.CaptiveScrewId)
            {
                return AssemblyFailures.InvalidRetention;
            }

            if (itemId.IsEmpty || (!_storageItemId.IsEmpty && itemId != _storageItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceStorageSeatOperationId.IsEmpty ||
                sourceStorageSeatOperationId != _storageSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceStorageSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatStorageDevice ||
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
                if (!sourceStorageRetentionOperationId.IsEmpty ||
                    _storageSlotState != StorageSlotState.StorageDeviceSeatedUnsecured ||
                    !_storageSecuredByOperationId.IsEmpty)
                {
                    return AssemblyFailures.StorageRetentionOutOfOrder;
                }
            }
            else if (sourceStorageRetentionOperationId.IsEmpty ||
                     sourceStorageRetentionOperationId != _storageSecuredByOperationId ||
                     _storageSlotState != StorageSlotState.StorageDeviceSecured ||
                     !_receipts.TryGetValue(
                         sourceStorageRetentionOperationId,
                         out AssemblyOperationReceipt retentionReceipt) ||
                     retentionReceipt.OperationKind !=
                         AssemblyOperationKind.SecureStorageDevice ||
                     retentionReceipt.ItemId != itemId ||
                     retentionReceipt.RetentionId != retentionId ||
                     retentionReceipt.SourceStorageSeatOperationId !=
                         sourceStorageSeatOperationId)
            {
                return AssemblyFailures.StorageRetentionOutOfOrder;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _storageProductId &&
                   item.ContainerId == _storageSlotDefinition.ContainerId
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private Failure ValidateRemoveStorageDevice(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceStorageSeatOperationId,
            long expectedAssemblyRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasStorageSlot || slotId != _storageSlotDefinition.SlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceStorageSeatOperationId.IsEmpty ||
                sourceStorageSeatOperationId != _storageSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceStorageSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatStorageDevice ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (_storageSlotState == StorageSlotState.StorageDeviceSecured)
            {
                return AssemblyFailures.StorageDeviceSecured;
            }

            if (_storageSlotState != StorageSlotState.StorageDeviceSeatedUnsecured ||
                itemId != _storageItemId)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _storageProductId &&
                   item.ContainerId == _storageSlotDefinition.ContainerId
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private bool ValidateStorageStateInvariants()
        {
            if (!HasStorageSlot)
            {
                return _storageInventoryTransferAccess == null &&
                       _storageSlotState == StorageSlotState.Unsupported &&
                       _storageSlotDefinition.SlotId.IsEmpty &&
                       _storageSlotDefinition.StandoffId.IsEmpty &&
                       _storageSlotDefinition.CaptiveScrewId.IsEmpty &&
                       _storageSlotDefinition.ContainerId.IsEmpty &&
                       _storageSlotDefinition.SupportedStorageType == default &&
                       _storageItemId.IsEmpty &&
                       _storageProductId.IsEmpty &&
                       _storageSeatedByOperationId.IsEmpty &&
                       _storageSecuredByOperationId.IsEmpty;
            }

            if (_storageInventoryTransferAccess == null ||
                _storageSlotDefinition.SlotId == MotherboardSlotId ||
                _storageSlotDefinition.SlotId == _processorSlotId ||
                _storageSlotDefinition.SlotId == _memorySlotDefinition.SlotId ||
                _storageSlotDefinition.CaptiveScrewId == _processorRetentionId ||
                _storageSlotDefinition.CaptiveScrewId == _memorySlotDefinition.RetentionId ||
                _storageSlotDefinition.ContainerId == _handsContainerId ||
                _storageSlotDefinition.ContainerId == _workbenchContainerId ||
                _storageSlotDefinition.ContainerId == _processorSocketContainerId ||
                _storageSlotDefinition.ContainerId == _memorySlotDefinition.ContainerId ||
                !_inventory.TryGetContainer(
                    _storageSlotDefinition.ContainerId,
                    out InventoryContainerDefinition storageSlot) ||
                storageSlot.Kind != InventoryContainerKind.Workbench ||
                storageSlot.UnitCapacity != 1)
            {
                return false;
            }

            if (_storageSlotState == StorageSlotState.EmptyOpen)
            {
                return _storageItemId.IsEmpty &&
                       _storageProductId.IsEmpty &&
                       _storageSeatedByOperationId.IsEmpty &&
                       _storageSecuredByOperationId.IsEmpty &&
                       _inventory.GetContainerQuantity(
                           _storageSlotDefinition.ContainerId).Value == 0;
            }

            if (_storageSlotState != StorageSlotState.StorageDeviceSeatedUnsecured &&
                _storageSlotState != StorageSlotState.StorageDeviceSecured)
            {
                return false;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty ||
                _storageItemId.IsEmpty ||
                _storageProductId.IsEmpty ||
                _storageSeatedByOperationId.IsEmpty ||
                !_inventory.TryGetSerializedItem(
                    _storageItemId,
                    out InventoryItemRecord storageItem) ||
                storageItem.ProductId != _storageProductId ||
                storageItem.ContainerId != _storageSlotDefinition.ContainerId ||
                !_componentCatalog.TryGet(
                    storageItem.ProductId,
                    out PcComponentSpecification storageSpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification) ||
                !AssemblyCompatibilityEvaluator.EvaluateStorageDeviceSeat(
                    storageSpecification,
                    motherboardSpecification,
                    _storageSlotDefinition.SupportedStorageType,
                    M2KeyOrientation.KeyAligned).IsCompatible ||
                !_receipts.TryGetValue(
                    _storageSeatedByOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatStorageDevice ||
                seatReceipt.ItemId != _storageItemId ||
                seatReceipt.ProductId != _storageProductId ||
                seatReceipt.SlotId != _storageSlotDefinition.SlotId ||
                seatReceipt.M2KeyOrientation != M2KeyOrientation.KeyAligned)
            {
                return false;
            }

            if (_storageSlotState == StorageSlotState.StorageDeviceSeatedUnsecured)
            {
                return _storageSecuredByOperationId.IsEmpty;
            }

            return !_storageSecuredByOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       _storageSecuredByOperationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.SecureStorageDevice &&
                   retentionReceipt.ItemId == _storageItemId &&
                   retentionReceipt.RetentionId == _storageSlotDefinition.CaptiveScrewId &&
                   retentionReceipt.SourceStorageSeatOperationId ==
                       _storageSeatedByOperationId;
        }

        private bool IsMatchingStorageSeatReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt seatReceipt) &&
                   seatReceipt.OperationKind == AssemblyOperationKind.SeatStorageDevice &&
                   seatReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   seatReceipt.ItemId == descendant.ItemId &&
                   seatReceipt.ProductId == descendant.ProductId &&
                   seatReceipt.SlotId == descendant.SlotId &&
                   seatReceipt.SourceAttachOperationId ==
                       descendant.SourceAttachOperationId &&
                   seatReceipt.SourceSecureOperationId ==
                       descendant.SourceSecureOperationId &&
                   seatReceipt.M2KeyOrientation == M2KeyOrientation.KeyAligned;
        }

        private bool IsMatchingStorageRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.SecureStorageDevice &&
                   retentionReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   retentionReceipt.ItemId == descendant.ItemId &&
                   retentionReceipt.ProductId == descendant.ProductId &&
                   retentionReceipt.SlotId == descendant.SlotId &&
                   retentionReceipt.RetentionId == descendant.RetentionId &&
                   retentionReceipt.SourceStorageSeatOperationId ==
                       descendant.SourceStorageSeatOperationId;
        }

        private static Failure MapStorageInventoryFailure(Failure failure, bool seating)
        {
            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return seating
                    ? AssemblyFailures.StorageSlotCapacityExceeded
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
