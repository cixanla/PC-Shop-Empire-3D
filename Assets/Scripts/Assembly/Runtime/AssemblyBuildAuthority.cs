using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Authoritative single-chassis assembly aggregate. Presentation may project motherboard
    /// and processor state but cannot own component identity, custody or operation chronology.
    /// </summary>
    public sealed partial class AssemblyBuildAuthority
    {
        private readonly PcComponentCatalog _componentCatalog;
        private readonly InventoryAuthority _inventory;
        private readonly StableId<ContainerIdScope> _handsContainerId;
        private readonly StableId<ContainerIdScope> _workbenchContainerId;
        private readonly StableId<AssemblyFastenerIdScope> _motherboardFastenerId;
        private readonly InventorySerializedTransferAccess _inventoryTransferAccess;
        private readonly MotherboardFormFactor _supportedMotherboardFormFactor;
        private readonly StableId<AssemblySlotIdScope> _processorSlotId;
        private readonly StableId<AssemblyRetentionIdScope> _processorRetentionId;
        private readonly StableId<ContainerIdScope> _processorSocketContainerId;
        private readonly InventorySerializedTransferAccess _processorInventoryTransferAccess;
        private readonly CpuSocketFamily _supportedCpuSocketFamily;
        private readonly Dictionary<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt> _receipts =
            new Dictionary<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt>();

        private AssemblySeatState _motherboardSeatState = AssemblySeatState.Empty;
        private StableId<ItemInstanceIdScope> _motherboardItemId;
        private StableId<ProductDefinitionIdScope> _motherboardProductId;
        private StableId<AssemblyOperationIdScope> _installedByOperationId;
        private StableId<AssemblyOperationIdScope> _securedByOperationId;
        private ProcessorSocketState _processorSocketState = ProcessorSocketState.Unsupported;
        private StableId<ItemInstanceIdScope> _processorItemId;
        private StableId<ProductDefinitionIdScope> _processorProductId;
        private StableId<AssemblyOperationIdScope> _processorSeatedByOperationId;
        private StableId<AssemblyOperationIdScope> _processorRetainedByOperationId;

        private AssemblyBuildAuthority(
            PcComponentCatalog componentCatalog,
            InventoryAuthority inventory,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
            StableId<AssemblyFastenerIdScope> motherboardFastenerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> workbenchContainerId,
            MotherboardFormFactor supportedMotherboardFormFactor,
            InventorySerializedTransferAccess inventoryTransferAccess)
            : this(
                componentCatalog,
                inventory,
                buildId,
                chassisId,
                motherboardSlotId,
                motherboardFastenerId,
                handsContainerId,
                workbenchContainerId,
                supportedMotherboardFormFactor,
                inventoryTransferAccess,
                default,
                default,
                default,
                default,
                default)
        {
        }

        private AssemblyBuildAuthority(
            PcComponentCatalog componentCatalog,
            InventoryAuthority inventory,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
            StableId<AssemblyFastenerIdScope> motherboardFastenerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> workbenchContainerId,
            MotherboardFormFactor supportedMotherboardFormFactor,
            InventorySerializedTransferAccess inventoryTransferAccess,
            StableId<AssemblySlotIdScope> processorSlotId,
            StableId<AssemblyRetentionIdScope> processorRetentionId,
            StableId<ContainerIdScope> processorSocketContainerId,
            CpuSocketFamily supportedCpuSocketFamily,
            InventorySerializedTransferAccess processorInventoryTransferAccess,
            DimmSlotDefinition memorySlotDefinition = default,
            InventorySerializedTransferAccess memoryInventoryTransferAccess = null)
        {
            _componentCatalog = componentCatalog;
            _inventory = inventory;
            BuildId = buildId;
            ChassisId = chassisId;
            MotherboardSlotId = motherboardSlotId;
            _motherboardFastenerId = motherboardFastenerId;
            _handsContainerId = handsContainerId;
            _workbenchContainerId = workbenchContainerId;
            _supportedMotherboardFormFactor = supportedMotherboardFormFactor;
            _inventoryTransferAccess = inventoryTransferAccess;
            _processorSlotId = processorSlotId;
            _processorRetentionId = processorRetentionId;
            _processorSocketContainerId = processorSocketContainerId;
            _supportedCpuSocketFamily = supportedCpuSocketFamily;
            _processorInventoryTransferAccess = processorInventoryTransferAccess;
            _memorySlotDefinition = memorySlotDefinition;
            _memoryInventoryTransferAccess = memoryInventoryTransferAccess;
            if (!processorSlotId.IsEmpty)
            {
                _processorSocketState = ProcessorSocketState.EmptyOpen;
            }

            if (memorySlotDefinition.IsValid)
            {
                _memorySlotState = MemorySlotState.EmptyOpen;
            }
        }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<AssemblySlotIdScope> MotherboardSlotId { get; }

        public StableId<AssemblyFastenerIdScope> MotherboardFastenerId =>
            _motherboardFastenerId;

        public StableId<ContainerIdScope> HandsContainerId => _handsContainerId;

        public StableId<ContainerIdScope> WorkbenchContainerId => _workbenchContainerId;

        public MotherboardFormFactor SupportedMotherboardFormFactor =>
            _supportedMotherboardFormFactor;

        public AssemblySeatState MotherboardSeatState => _motherboardSeatState;

        public StableId<ItemInstanceIdScope> MotherboardItemId => _motherboardItemId;

        public StableId<ProductDefinitionIdScope> MotherboardProductId => _motherboardProductId;

        public StableId<AssemblyOperationIdScope> InstalledByOperationId => _installedByOperationId;

        public StableId<AssemblyOperationIdScope> SecuredByOperationId => _securedByOperationId;

        public bool HasProcessorSocket => !_processorSlotId.IsEmpty;

        public StableId<AssemblySlotIdScope> ProcessorSlotId => _processorSlotId;

        public StableId<AssemblyRetentionIdScope> ProcessorRetentionId => _processorRetentionId;

        public StableId<ContainerIdScope> ProcessorSocketContainerId =>
            _processorSocketContainerId;

        public CpuSocketFamily SupportedCpuSocketFamily => _supportedCpuSocketFamily;

        public ProcessorSocketState ProcessorSocketState => _processorSocketState;

        public StableId<ItemInstanceIdScope> ProcessorItemId => _processorItemId;

        public StableId<ProductDefinitionIdScope> ProcessorProductId => _processorProductId;

        public StableId<AssemblyOperationIdScope> ProcessorSeatedByOperationId =>
            _processorSeatedByOperationId;

        public StableId<AssemblyOperationIdScope> ProcessorRetainedByOperationId =>
            _processorRetainedByOperationId;

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public static OperationResult<AssemblyBuildAuthority> Create(
            PcComponentCatalog componentCatalog,
            InventoryAuthority inventory,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
            StableId<AssemblyFastenerIdScope> motherboardFastenerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> workbenchContainerId,
            MotherboardFormFactor supportedMotherboardFormFactor)
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
                return OperationResult<AssemblyBuildAuthority>.Fail(AssemblyFailures.InvalidBuildId);
            }

            if (chassisId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(AssemblyFailures.InvalidChassisId);
            }

            if (motherboardSlotId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(AssemblyFailures.InvalidSlotId);
            }

            if (motherboardFastenerId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidFastener);
            }

            if (handsContainerId.IsEmpty ||
                !inventory.TryGetContainer(handsContainerId, out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidHandsContainer);
            }

            if (workbenchContainerId.IsEmpty ||
                !inventory.TryGetContainer(workbenchContainerId, out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidWorkbenchContainer);
            }

            if (handsContainerId == workbenchContainerId)
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

            OperationResult<InventorySerializedTransferAccess> access =
                inventory.ClaimManagedSerializedTransferContainer(workbenchContainerId);
            if (access.IsFailure)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? AssemblyFailures.RevisionOverflow
                        : access.Error == InventoryFailures.SerializedTransferContainerOccupied
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
                    access.Value));
        }

        public static OperationResult<AssemblyBuildAuthority> CreateWithProcessorSocket(
            PcComponentCatalog componentCatalog,
            InventoryAuthority inventory,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
            StableId<AssemblyFastenerIdScope> motherboardFastenerId,
            StableId<AssemblySlotIdScope> processorSlotId,
            StableId<AssemblyRetentionIdScope> processorRetentionId,
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

            if (motherboardSlotId.IsEmpty ||
                processorSlotId.IsEmpty ||
                motherboardSlotId == processorSlotId)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (motherboardFastenerId.IsEmpty)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    AssemblyFailures.InvalidFastener);
            }

            if (processorRetentionId.IsEmpty)
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

            if (handsContainerId == workbenchContainerId ||
                handsContainerId == processorSocketContainerId ||
                workbenchContainerId == processorSocketContainerId)
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

            OperationResult<InventorySerializedTransferAccessPair> accessPair =
                inventory.ClaimManagedSerializedTransferContainers(
                    workbenchContainerId,
                    processorSocketContainerId);
            if (accessPair.IsFailure)
            {
                return OperationResult<AssemblyBuildAuthority>.Fail(
                    accessPair.Error == InventoryFailures.RevisionOverflow
                        ? AssemblyFailures.RevisionOverflow
                        : accessPair.Error ==
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
                    accessPair.Value.First,
                    processorSlotId,
                    processorRetentionId,
                    processorSocketContainerId,
                    supportedCpuSocketFamily,
                    accessPair.Value.Second));
        }

        public OperationResult<AssemblyOperationReceipt> AttachMotherboard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesAttach(itemId, slotId)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateAttach(itemId, slotId);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            long expectedAssemblyRevision = Revision;
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _workbenchContainerId,
                    _inventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapInventoryFailure(prepared.Error, attaching: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapInventoryFailure(committed.Error, attaching: true));
            }

            _motherboardSeatState = AssemblySeatState.SeatedUnsecured;
            _motherboardItemId = item.Id;
            _motherboardProductId = item.ProductId;
            _installedByOperationId = operationId;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.AttachMotherboard,
                BuildId,
                ChassisId,
                MotherboardSlotId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _workbenchContainerId,
                default,
                default,
                default,
                -1,
                expectedAssemblyRevision,
                AssemblySeatState.Empty,
                _motherboardSeatState,
                Revision,
                _inventory.Revision,
                _processorSocketState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> DetachMotherboard(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesDetach(itemId, slotId)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateDetach(itemId, slotId);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            long expectedAssemblyRevision = Revision;
            StableId<AssemblyOperationIdScope> sourceAttachOperationId =
                _installedByOperationId;
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    item.Id,
                    _handsContainerId,
                    _inventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapInventoryFailure(prepared.Error, attaching: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapInventoryFailure(committed.Error, attaching: false));
            }

            _motherboardSeatState = AssemblySeatState.Empty;
            _motherboardItemId = default;
            _motherboardProductId = default;
            _installedByOperationId = default;
            _securedByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.DetachMotherboard,
                BuildId,
                ChassisId,
                MotherboardSlotId,
                item.Id,
                item.ProductId,
                _workbenchContainerId,
                _handsContainerId,
                sourceAttachOperationId,
                default,
                default,
                -1,
                expectedAssemblyRevision,
                AssemblySeatState.SeatedUnsecured,
                _motherboardSeatState,
                Revision,
                _inventory.Revision,
                _processorSocketState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> SecureMotherboardFastener(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesSecure(
                        itemId,
                        slotId,
                        fastenerId,
                        sourceAttachOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateFastenerOperation(
                itemId,
                slotId,
                fastenerId,
                sourceAttachOperationId,
                default,
                expectedAssemblyRevision,
                securing: true);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            _motherboardSeatState = AssemblySeatState.SeatedSecured;
            _securedByOperationId = operationId;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.SecureMotherboardFastener,
                BuildId,
                ChassisId,
                MotherboardSlotId,
                item.Id,
                item.ProductId,
                default,
                default,
                sourceAttachOperationId,
                default,
                fastenerId,
                0,
                expectedAssemblyRevision,
                AssemblySeatState.SeatedUnsecured,
                _motherboardSeatState,
                Revision,
                _inventory.Revision,
                _processorSocketState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> UnsecureMotherboardFastener(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesUnsecure(
                        itemId,
                        slotId,
                        fastenerId,
                        sourceAttachOperationId,
                        sourceSecureOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateFastenerOperation(
                itemId,
                slotId,
                fastenerId,
                sourceAttachOperationId,
                sourceSecureOperationId,
                expectedAssemblyRevision,
                securing: false);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            _motherboardSeatState = AssemblySeatState.SeatedUnsecured;
            _securedByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.UnsecureMotherboardFastener,
                BuildId,
                ChassisId,
                MotherboardSlotId,
                item.Id,
                item.ProductId,
                default,
                default,
                sourceAttachOperationId,
                sourceSecureOperationId,
                fastenerId,
                0,
                expectedAssemblyRevision,
                AssemblySeatState.SeatedSecured,
                _motherboardSeatState,
                Revision,
                _inventory.Revision,
                _processorSocketState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> SeatProcessor(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
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
                return replay.MatchesSeatProcessor(
                        itemId,
                        slotId,
                        sourceMotherboardAttachOperationId,
                        sourceMotherboardSecureOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateSeatProcessor(
                itemId,
                slotId,
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
                    _processorSocketContainerId,
                    _processorInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorInventoryFailure(prepared.Error, seating: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorInventoryFailure(committed.Error, seating: true));
            }

            _processorSocketState = ProcessorSocketState.ProcessorSeatedOpen;
            _processorItemId = item.Id;
            _processorProductId = item.ProductId;
            _processorSeatedByOperationId = operationId;
            _processorRetainedByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.SeatProcessor,
                BuildId,
                ChassisId,
                _processorSlotId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _processorSocketContainerId,
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
                ProcessorSocketState.EmptyOpen,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                _memorySlotState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> CloseProcessorRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesCloseProcessorRetention(
                        itemId,
                        slotId,
                        retentionId,
                        sourceProcessorSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateProcessorRetention(
                itemId,
                slotId,
                retentionId,
                sourceProcessorSeatOperationId,
                default,
                expectedAssemblyRevision,
                closing: true);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceProcessorSeatOperationId];
            _processorSocketState = ProcessorSocketState.ProcessorRetained;
            _processorRetainedByOperationId = operationId;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.CloseProcessorRetention,
                BuildId,
                ChassisId,
                _processorSlotId,
                _processorItemId,
                _processorProductId,
                default,
                default,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                default,
                retentionId,
                sourceProcessorSeatOperationId,
                default,
                0,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                ProcessorSocketState.ProcessorSeatedOpen,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                _memorySlotState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> OpenProcessorRetention(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
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
                return replay.MatchesOpenProcessorRetention(
                        itemId,
                        slotId,
                        retentionId,
                        sourceProcessorSeatOperationId,
                        sourceProcessorRetentionOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateProcessorRetention(
                itemId,
                slotId,
                retentionId,
                sourceProcessorSeatOperationId,
                sourceProcessorRetentionOperationId,
                expectedAssemblyRevision,
                closing: false);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceProcessorSeatOperationId];
            _processorSocketState = ProcessorSocketState.ProcessorSeatedOpen;
            _processorRetainedByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.OpenProcessorRetention,
                BuildId,
                ChassisId,
                _processorSlotId,
                _processorItemId,
                _processorProductId,
                default,
                default,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                default,
                retentionId,
                sourceProcessorSeatOperationId,
                sourceProcessorRetentionOperationId,
                0,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                ProcessorSocketState.ProcessorRetained,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                _memorySlotState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult<AssemblyOperationReceipt> RemoveProcessor(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            long expectedAssemblyRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(operationId, out AssemblyOperationReceipt replay))
            {
                return replay.MatchesRemoveProcessor(
                        itemId,
                        slotId,
                        sourceProcessorSeatOperationId,
                        expectedAssemblyRevision)
                    ? OperationResult<AssemblyOperationReceipt>.Success(replay)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflightFailure = ValidateRemoveProcessor(
                itemId,
                slotId,
                sourceProcessorSeatOperationId,
                expectedAssemblyRevision);
            if (!preflightFailure.IsNone)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(preflightFailure);
            }

            InventoryItemRecord item = GetItem(itemId);
            AssemblyOperationReceipt seatReceipt =
                _receipts[sourceProcessorSeatOperationId];
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _processorInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorInventoryFailure(prepared.Error, seating: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<AssemblyOperationReceipt>.Fail(
                    MapProcessorInventoryFailure(committed.Error, seating: false));
            }

            _processorSocketState = ProcessorSocketState.EmptyOpen;
            _processorItemId = default;
            _processorProductId = default;
            _processorSeatedByOperationId = default;
            _processorRetainedByOperationId = default;
            Revision++;

            var receipt = new AssemblyOperationReceipt(
                operationId,
                AssemblyOperationKind.RemoveProcessor,
                BuildId,
                ChassisId,
                _processorSlotId,
                item.Id,
                item.ProductId,
                _processorSocketContainerId,
                _handsContainerId,
                seatReceipt.SourceAttachOperationId,
                seatReceipt.SourceSecureOperationId,
                default,
                default,
                sourceProcessorSeatOperationId,
                default,
                -1,
                expectedAssemblyRevision,
                _motherboardSeatState,
                _motherboardSeatState,
                ProcessorSocketState.ProcessorSeatedOpen,
                _processorSocketState,
                Revision,
                _inventory.Revision,
                _memorySlotState,
                _memorySlotState);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult EvaluateBenchmarkReadiness()
        {
            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return OperationResult.Fail(AssemblyFailures.MotherboardMissing);
            }

            if (_motherboardSeatState == AssemblySeatState.SeatedUnsecured)
            {
                return OperationResult.Fail(AssemblyFailures.MotherboardUnsecured);
            }

            if (!HasProcessorSocket)
            {
                return OperationResult.Fail(AssemblyFailures.BuildIncomplete);
            }

            if (_processorSocketState == ProcessorSocketState.EmptyOpen)
            {
                return OperationResult.Fail(AssemblyFailures.ProcessorMissing);
            }

            if (_processorSocketState == ProcessorSocketState.ProcessorSeatedOpen)
            {
                return OperationResult.Fail(AssemblyFailures.ProcessorUnretained);
            }

            if (!HasMemorySlot)
            {
                return OperationResult.Fail(AssemblyFailures.BuildIncomplete);
            }

            if (_memorySlotState == MemorySlotState.EmptyOpen)
            {
                return OperationResult.Fail(AssemblyFailures.MemoryMissing);
            }

            return _memorySlotState == MemorySlotState.MemoryModuleSeatedOpen
                ? OperationResult.Fail(AssemblyFailures.MemoryUnretained)
                : OperationResult.Fail(AssemblyFailures.BuildIncomplete);
        }

        public AssemblyBuildSnapshot GetSnapshot()
        {
            return new AssemblyBuildSnapshot(
                BuildId,
                ChassisId,
                MotherboardSlotId,
                _motherboardFastenerId,
                _handsContainerId,
                _workbenchContainerId,
                _processorSlotId,
                _processorRetentionId,
                _processorSocketContainerId,
                _supportedMotherboardFormFactor,
                _supportedCpuSocketFamily,
                _motherboardSeatState,
                _motherboardItemId,
                _motherboardProductId,
                _installedByOperationId,
                _securedByOperationId,
                _processorSocketState,
                _processorItemId,
                _processorProductId,
                _processorSeatedByOperationId,
                _processorRetainedByOperationId,
                Revision,
                _memorySlotDefinition,
                _memorySlotState,
                _memoryItemId,
                _memoryProductId,
                _memorySeatedByOperationId,
                _memoryRetainedByOperationId);
        }

        public bool TryGetReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            out AssemblyOperationReceipt receipt)
        {
            return _receipts.TryGetValue(operationId, out receipt);
        }

        public IReadOnlyList<AssemblyOperationReceipt> GetReceipts()
        {
            var receipts = new List<AssemblyOperationReceipt>(_receipts.Values);
            receipts.Sort((left, right) =>
            {
                int revisionComparison = left.AssemblyRevision.CompareTo(right.AssemblyRevision);
                return revisionComparison != 0
                    ? revisionComparison
                    : string.Compare(
                        left.OperationId.Value,
                        right.OperationId.Value,
                        StringComparison.Ordinal);
            });
            return Array.AsReadOnly(receipts.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            if (_componentCatalog == null ||
                _inventory == null ||
                BuildId.IsEmpty ||
                ChassisId.IsEmpty ||
                MotherboardSlotId.IsEmpty ||
                _motherboardFastenerId.IsEmpty ||
                _handsContainerId.IsEmpty ||
                _workbenchContainerId.IsEmpty ||
                _handsContainerId == _workbenchContainerId ||
                !PcComponentSpecification.IsValidMotherboardFormFactor(
                    _supportedMotherboardFormFactor))
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            if (!_inventory.TryGetContainer(
                    _handsContainerId,
                    out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands ||
                !_inventory.TryGetContainer(
                    _workbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench)
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            if (HasProcessorSocket)
            {
                if (_processorRetentionId.IsEmpty ||
                    _processorSocketContainerId.IsEmpty ||
                    _processorInventoryTransferAccess == null ||
                    _processorSlotId == MotherboardSlotId ||
                    _processorSocketContainerId == _handsContainerId ||
                    _processorSocketContainerId == _workbenchContainerId ||
                    !PcComponentSpecification.IsValidCpuSocketFamily(
                        _supportedCpuSocketFamily) ||
                    !_inventory.TryGetContainer(
                        _processorSocketContainerId,
                        out InventoryContainerDefinition processorSocket) ||
                    processorSocket.Kind != InventoryContainerKind.Workbench ||
                    processorSocket.UnitCapacity != 1)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }
            else if (!_processorRetentionId.IsEmpty ||
                     !_processorSocketContainerId.IsEmpty ||
                     _processorInventoryTransferAccess != null ||
                     _supportedCpuSocketFamily != default ||
                     _processorSocketState != ProcessorSocketState.Unsupported)
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                if (!_motherboardItemId.IsEmpty ||
                    !_motherboardProductId.IsEmpty ||
                    !_installedByOperationId.IsEmpty ||
                    !_securedByOperationId.IsEmpty)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                if (HasProcessorSocket &&
                    _processorSocketState != ProcessorSocketState.EmptyOpen)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }
            else if (_motherboardSeatState == AssemblySeatState.SeatedUnsecured ||
                     _motherboardSeatState == AssemblySeatState.SeatedSecured)
            {
                if (_motherboardItemId.IsEmpty ||
                    _motherboardProductId.IsEmpty ||
                    _installedByOperationId.IsEmpty ||
                    !_inventory.TryGetSerializedItem(
                        _motherboardItemId,
                        out InventoryItemRecord item) ||
                    item.ProductId != _motherboardProductId ||
                    item.ContainerId != _workbenchContainerId ||
                    !_componentCatalog.TryGet(
                        item.ProductId,
                        out PcComponentSpecification specification) ||
                    !AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                        specification,
                        _supportedMotherboardFormFactor).IsCompatible)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                if (_motherboardSeatState == AssemblySeatState.SeatedUnsecured)
                {
                    if (!_securedByOperationId.IsEmpty)
                    {
                        return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                    }
                }
                else if (_securedByOperationId.IsEmpty ||
                         !_receipts.TryGetValue(
                             _securedByOperationId,
                             out AssemblyOperationReceipt secureReceipt) ||
                         secureReceipt.OperationKind !=
                             AssemblyOperationKind.SecureMotherboardFastener ||
                         secureReceipt.ItemId != _motherboardItemId ||
                         secureReceipt.FastenerId != _motherboardFastenerId ||
                         secureReceipt.SourceAttachOperationId != _installedByOperationId)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }
            else
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            if (!HasProcessorSocket)
            {
                if (!_processorItemId.IsEmpty ||
                    !_processorProductId.IsEmpty ||
                    !_processorSeatedByOperationId.IsEmpty ||
                    !_processorRetainedByOperationId.IsEmpty)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }
            else if (_processorSocketState == ProcessorSocketState.EmptyOpen)
            {
                if (!_processorItemId.IsEmpty ||
                    !_processorProductId.IsEmpty ||
                    !_processorSeatedByOperationId.IsEmpty ||
                    !_processorRetainedByOperationId.IsEmpty ||
                    _inventory.GetContainerQuantity(
                        _processorSocketContainerId).Value != 0)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }
            else if (_processorSocketState ==
                         ProcessorSocketState.ProcessorSeatedOpen ||
                     _processorSocketState == ProcessorSocketState.ProcessorRetained)
            {
                if (_motherboardSeatState == AssemblySeatState.Empty ||
                    _processorItemId.IsEmpty ||
                    _processorProductId.IsEmpty ||
                    _processorSeatedByOperationId.IsEmpty ||
                    !_inventory.TryGetSerializedItem(
                        _processorItemId,
                        out InventoryItemRecord processorItem) ||
                    processorItem.ProductId != _processorProductId ||
                    processorItem.ContainerId != _processorSocketContainerId ||
                    !_componentCatalog.TryGet(
                        processorItem.ProductId,
                        out PcComponentSpecification processorSpecification) ||
                    !_componentCatalog.TryGet(
                        _motherboardProductId,
                        out PcComponentSpecification motherboardSpecification) ||
                    !AssemblyCompatibilityEvaluator.EvaluateProcessorSeat(
                        processorSpecification,
                        motherboardSpecification,
                        _supportedCpuSocketFamily).IsCompatible ||
                    !_receipts.TryGetValue(
                        _processorSeatedByOperationId,
                        out AssemblyOperationReceipt seatReceipt) ||
                    seatReceipt.OperationKind != AssemblyOperationKind.SeatProcessor ||
                    seatReceipt.ItemId != _processorItemId ||
                    seatReceipt.SlotId != _processorSlotId)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                if (_processorSocketState == ProcessorSocketState.ProcessorSeatedOpen)
                {
                    if (!_processorRetainedByOperationId.IsEmpty)
                    {
                        return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                    }
                }
                else if (_processorRetainedByOperationId.IsEmpty ||
                         !_receipts.TryGetValue(
                             _processorRetainedByOperationId,
                             out AssemblyOperationReceipt retentionReceipt) ||
                         retentionReceipt.OperationKind !=
                             AssemblyOperationKind.CloseProcessorRetention ||
                         retentionReceipt.ItemId != _processorItemId ||
                         retentionReceipt.RetentionId != _processorRetentionId ||
                         retentionReceipt.SourceProcessorSeatOperationId !=
                             _processorSeatedByOperationId)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }
            else
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            if (!ValidateMemoryStateInvariants())
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            if (Revision != _receipts.Count)
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            var receiptsByRevision = new AssemblyOperationReceipt[_receipts.Count];
            foreach (KeyValuePair<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt> entry in _receipts)
            {
                AssemblyOperationReceipt receipt = entry.Value;
                bool processorOperation = receipt != null &&
                    IsProcessorOperation(receipt.OperationKind);
                bool memoryOperation = receipt != null &&
                    IsMemoryOperation(receipt.OperationKind);
                StableId<AssemblySlotIdScope> expectedSlotId = memoryOperation
                    ? _memorySlotDefinition.SlotId
                    : processorOperation
                        ? _processorSlotId
                        : MotherboardSlotId;
                if (receipt == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != receipt.OperationId ||
                    receipt.BuildId != BuildId ||
                    receipt.ChassisId != ChassisId ||
                    receipt.SlotId != expectedSlotId ||
                    receipt.ItemId.IsEmpty ||
                    receipt.ProductId.IsEmpty ||
                    receipt.AssemblyRevision <= 0 ||
                    receipt.AssemblyRevision > Revision ||
                    receipt.ExpectedAssemblyRevision != receipt.AssemblyRevision - 1L ||
                    receipt.InventoryRevision <= 0 ||
                    receipt.InventoryRevision > _inventory.Revision ||
                    !IsKnownOperationKind(receipt.OperationKind))
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                int revisionIndex = checked((int)receipt.AssemblyRevision - 1);
                if (receiptsByRevision[revisionIndex] != null)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                receiptsByRevision[revisionIndex] = receipt;

                if (!ValidateReceiptShape(receipt))
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                if (!ValidateReceiptTransition(receipt))
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }

            if (!ValidateReceiptHistory(receiptsByRevision))
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            return _inventory.ValidateInvariants().IsSuccess
                ? OperationResult.Success()
                : OperationResult.Fail(AssemblyFailures.InvariantViolation);
        }

        private Failure ValidateAttach(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId)
        {
            if (slotId != MotherboardSlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (_motherboardSeatState != AssemblySeatState.Empty)
            {
                return AssemblyFailures.SlotOccupied;
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
                    out PcComponentSpecification specification))
            {
                return AssemblyFailures.UnknownComponentSpecification;
            }

            AssemblyCompatibilityResult compatibility =
                AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    specification,
                    _supportedMotherboardFormFactor);
            return compatibility.IsCompatible ? Failure.None : compatibility.Reason;
        }

        private Failure ValidateDetach(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId)
        {
            if (slotId != MotherboardSlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (HasProcessorSocket &&
                _processorSocketState != ProcessorSocketState.EmptyOpen)
            {
                return AssemblyFailures.ProcessorInstalled;
            }

            if (HasMemorySlot && _memorySlotState != MemorySlotState.EmptyOpen)
            {
                return AssemblyFailures.MemoryModuleInstalled;
            }

            if (_motherboardSeatState == AssemblySeatState.SeatedSecured)
            {
                return AssemblyFailures.ComponentSecured;
            }

            if (_motherboardSeatState != AssemblySeatState.SeatedUnsecured)
            {
                return AssemblyFailures.SlotEmpty;
            }

            if (itemId != _motherboardItemId)
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (!_inventory.TryGetSerializedItem(
                    _motherboardItemId,
                    out InventoryItemRecord item) ||
                item.ProductId != _motherboardProductId)
            {
                return AssemblyFailures.InvariantViolation;
            }

            return item.ContainerId == _workbenchContainerId
                ? Failure.None
                : AssemblyFailures.ItemNotOnWorkbench;
        }

        private Failure ValidateFastenerOperation(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyFastenerIdScope> fastenerId,
            StableId<AssemblyOperationIdScope> sourceAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceSecureOperationId,
            long expectedAssemblyRevision,
            bool securing)
        {
            if (slotId != MotherboardSlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (fastenerId != _motherboardFastenerId)
            {
                return AssemblyFailures.InvalidFastener;
            }

            if (itemId.IsEmpty ||
                (!_motherboardItemId.IsEmpty && itemId != _motherboardItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceAttachOperationId.IsEmpty ||
                sourceAttachOperationId != _installedByOperationId ||
                !_receipts.TryGetValue(
                    sourceAttachOperationId,
                    out AssemblyOperationReceipt attachReceipt) ||
                attachReceipt.OperationKind != AssemblyOperationKind.AttachMotherboard ||
                attachReceipt.ItemId != itemId ||
                attachReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            if (securing)
            {
                if (!sourceSecureOperationId.IsEmpty ||
                    _motherboardSeatState != AssemblySeatState.SeatedUnsecured ||
                    !_securedByOperationId.IsEmpty)
                {
                    return AssemblyFailures.FastenerOutOfOrder;
                }
            }
            else if (sourceSecureOperationId.IsEmpty ||
                     sourceSecureOperationId != _securedByOperationId ||
                     _motherboardSeatState != AssemblySeatState.SeatedSecured ||
                     !_receipts.TryGetValue(
                         sourceSecureOperationId,
                         out AssemblyOperationReceipt secureReceipt) ||
                     secureReceipt.OperationKind !=
                         AssemblyOperationKind.SecureMotherboardFastener ||
                     secureReceipt.ItemId != itemId ||
                     secureReceipt.FastenerId != fastenerId ||
                     secureReceipt.SourceAttachOperationId != sourceAttachOperationId)
            {
                return AssemblyFailures.FastenerOutOfOrder;
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item) ||
                item.ProductId != _motherboardProductId ||
                item.ContainerId != _workbenchContainerId)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            return Failure.None;
        }

        private Failure ValidateSeatProcessor(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceMotherboardAttachOperationId,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            long expectedAssemblyRevision)
        {
            if (!HasProcessorSocket)
            {
                return AssemblyFailures.ProcessorSocketUnavailable;
            }

            if (slotId != _processorSlotId)
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
                sourceMotherboardAttachOperationId != _installedByOperationId ||
                sourceMotherboardSecureOperationId.IsEmpty ||
                sourceMotherboardSecureOperationId != _securedByOperationId ||
                !_receipts.TryGetValue(
                    sourceMotherboardAttachOperationId,
                    out AssemblyOperationReceipt attachReceipt) ||
                attachReceipt.OperationKind != AssemblyOperationKind.AttachMotherboard ||
                !_receipts.TryGetValue(
                    sourceMotherboardSecureOperationId,
                    out AssemblyOperationReceipt secureReceipt) ||
                secureReceipt.OperationKind !=
                    AssemblyOperationKind.SecureMotherboardFastener ||
                secureReceipt.SourceAttachOperationId !=
                    sourceMotherboardAttachOperationId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_processorSocketState != ProcessorSocketState.EmptyOpen)
            {
                return AssemblyFailures.ProcessorSocketOccupied;
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
                    out PcComponentSpecification processorSpecification) ||
                !_componentCatalog.TryGet(
                    _motherboardProductId,
                    out PcComponentSpecification motherboardSpecification))
            {
                return AssemblyFailures.UnknownComponentSpecification;
            }

            AssemblyCompatibilityResult compatibility =
                AssemblyCompatibilityEvaluator.EvaluateProcessorSeat(
                    processorSpecification,
                    motherboardSpecification,
                    _supportedCpuSocketFamily);
            return compatibility.IsCompatible ? Failure.None : compatibility.Reason;
        }

        private Failure ValidateProcessorRetention(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedAssemblyRevision,
            bool closing)
        {
            if (!HasProcessorSocket)
            {
                return AssemblyFailures.ProcessorSocketUnavailable;
            }

            if (slotId != _processorSlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (retentionId != _processorRetentionId)
            {
                return AssemblyFailures.InvalidRetention;
            }

            if (itemId.IsEmpty ||
                (!_processorItemId.IsEmpty && itemId != _processorItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceProcessorSeatOperationId.IsEmpty ||
                sourceProcessorSeatOperationId != _processorSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceProcessorSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatProcessor ||
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
                if (!sourceProcessorRetentionOperationId.IsEmpty ||
                    _processorSocketState !=
                        ProcessorSocketState.ProcessorSeatedOpen ||
                    !_processorRetainedByOperationId.IsEmpty)
                {
                    return AssemblyFailures.ProcessorRetentionOutOfOrder;
                }
            }
            else if (sourceProcessorRetentionOperationId.IsEmpty ||
                     sourceProcessorRetentionOperationId !=
                         _processorRetainedByOperationId ||
                     _processorSocketState != ProcessorSocketState.ProcessorRetained ||
                     !_receipts.TryGetValue(
                         sourceProcessorRetentionOperationId,
                         out AssemblyOperationReceipt retentionReceipt) ||
                     retentionReceipt.OperationKind !=
                         AssemblyOperationKind.CloseProcessorRetention ||
                     retentionReceipt.ItemId != itemId ||
                     retentionReceipt.RetentionId != retentionId ||
                     retentionReceipt.SourceProcessorSeatOperationId !=
                         sourceProcessorSeatOperationId)
            {
                return AssemblyFailures.ProcessorRetentionOutOfOrder;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _processorProductId &&
                   item.ContainerId == _processorSocketContainerId
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private Failure ValidateRemoveProcessor(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyOperationIdScope> sourceProcessorSeatOperationId,
            long expectedAssemblyRevision)
        {
            if (!HasProcessorSocket)
            {
                return AssemblyFailures.ProcessorSocketUnavailable;
            }

            if (slotId != _processorSlotId)
            {
                return AssemblyFailures.UnknownSlot;
            }

            if (itemId.IsEmpty ||
                (!_processorItemId.IsEmpty && itemId != _processorItemId))
            {
                return AssemblyFailures.IdentityConflict;
            }

            if (Revision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (expectedAssemblyRevision != Revision ||
                sourceProcessorSeatOperationId.IsEmpty ||
                sourceProcessorSeatOperationId != _processorSeatedByOperationId ||
                !_receipts.TryGetValue(
                    sourceProcessorSeatOperationId,
                    out AssemblyOperationReceipt seatReceipt) ||
                seatReceipt.OperationKind != AssemblyOperationKind.SeatProcessor ||
                seatReceipt.ItemId != itemId ||
                seatReceipt.SlotId != slotId)
            {
                return AssemblyFailures.PlanStale;
            }

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return AssemblyFailures.MotherboardMissing;
            }

            if (_processorSocketState == ProcessorSocketState.ProcessorRetained)
            {
                return AssemblyFailures.ProcessorRetained;
            }

            if (_processorSocketState != ProcessorSocketState.ProcessorSeatedOpen)
            {
                return AssemblyFailures.ComponentNotSeated;
            }

            return _inventory.TryGetSerializedItem(
                       itemId,
                       out InventoryItemRecord item) &&
                   item.ProductId == _processorProductId &&
                   item.ContainerId == _processorSocketContainerId
                ? Failure.None
                : AssemblyFailures.ComponentNotSeated;
        }

        private bool ValidateReceiptShape(AssemblyOperationReceipt receipt)
        {
            if (!IsMemoryOperation(receipt.OperationKind) &&
                (!receipt.SourceMemorySeatOperationId.IsEmpty ||
                 !receipt.SourceMemoryRetentionOperationId.IsEmpty ||
                 receipt.DimmKeyOrientation != default))
            {
                return false;
            }

            switch (receipt.OperationKind)
            {
                case AssemblyOperationKind.AttachMotherboard:
                    return receipt.SourceContainerId == _handsContainerId &&
                           receipt.TargetContainerId == _workbenchContainerId &&
                           receipt.SourceAttachOperationId.IsEmpty &&
                           receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId.IsEmpty &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == -1;

                case AssemblyOperationKind.DetachMotherboard:
                    return receipt.SourceContainerId == _workbenchContainerId &&
                           receipt.TargetContainerId == _handsContainerId &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId.IsEmpty &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == -1;

                case AssemblyOperationKind.SecureMotherboardFastener:
                    return receipt.SourceContainerId.IsEmpty &&
                           receipt.TargetContainerId.IsEmpty &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId == _motherboardFastenerId &&
                           receipt.RetentionId.IsEmpty &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == 0;

                case AssemblyOperationKind.UnsecureMotherboardFastener:
                    return receipt.SourceContainerId.IsEmpty &&
                           receipt.TargetContainerId.IsEmpty &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId == _motherboardFastenerId &&
                           receipt.RetentionId.IsEmpty &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == 0;

                case AssemblyOperationKind.SeatProcessor:
                    return HasProcessorSocket &&
                           receipt.SourceContainerId == _handsContainerId &&
                           receipt.TargetContainerId == _processorSocketContainerId &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId.IsEmpty &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == -1;

                case AssemblyOperationKind.RemoveProcessor:
                    return HasProcessorSocket &&
                           receipt.SourceContainerId == _processorSocketContainerId &&
                           receipt.TargetContainerId == _handsContainerId &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId.IsEmpty &&
                           !receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == -1;

                case AssemblyOperationKind.CloseProcessorRetention:
                    return HasProcessorSocket &&
                           receipt.SourceContainerId.IsEmpty &&
                           receipt.TargetContainerId.IsEmpty &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId == _processorRetentionId &&
                           !receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == 0;

                case AssemblyOperationKind.OpenProcessorRetention:
                    return HasProcessorSocket &&
                           receipt.SourceContainerId.IsEmpty &&
                           receipt.TargetContainerId.IsEmpty &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId == _processorRetentionId &&
                           !receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           !receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SequenceIndex == 0;

                case AssemblyOperationKind.SeatMemoryModule:
                    return HasMemorySlot &&
                           receipt.SourceContainerId == _handsContainerId &&
                           receipt.TargetContainerId ==
                               _memorySlotDefinition.ContainerId &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId.IsEmpty &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           receipt.SourceMemorySeatOperationId.IsEmpty &&
                           receipt.SourceMemoryRetentionOperationId.IsEmpty &&
                           receipt.DimmKeyOrientation ==
                               DimmKeyOrientation.NotchAligned &&
                           receipt.SequenceIndex == -1;

                case AssemblyOperationKind.RemoveMemoryModule:
                    return HasMemorySlot &&
                           receipt.SourceContainerId ==
                               _memorySlotDefinition.ContainerId &&
                           receipt.TargetContainerId == _handsContainerId &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId.IsEmpty &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           !receipt.SourceMemorySeatOperationId.IsEmpty &&
                           receipt.SourceMemoryRetentionOperationId.IsEmpty &&
                           receipt.DimmKeyOrientation == default &&
                           receipt.SequenceIndex == -1;

                case AssemblyOperationKind.CloseMemoryRetention:
                    return HasMemorySlot &&
                           receipt.SourceContainerId.IsEmpty &&
                           receipt.TargetContainerId.IsEmpty &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId ==
                               _memorySlotDefinition.RetentionId &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           !receipt.SourceMemorySeatOperationId.IsEmpty &&
                           receipt.SourceMemoryRetentionOperationId.IsEmpty &&
                           receipt.DimmKeyOrientation == default &&
                           receipt.SequenceIndex == 0;

                case AssemblyOperationKind.OpenMemoryRetention:
                    return HasMemorySlot &&
                           receipt.SourceContainerId.IsEmpty &&
                           receipt.TargetContainerId.IsEmpty &&
                           !receipt.SourceAttachOperationId.IsEmpty &&
                           !receipt.SourceSecureOperationId.IsEmpty &&
                           receipt.FastenerId.IsEmpty &&
                           receipt.RetentionId ==
                               _memorySlotDefinition.RetentionId &&
                           receipt.SourceProcessorSeatOperationId.IsEmpty &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           !receipt.SourceMemorySeatOperationId.IsEmpty &&
                           !receipt.SourceMemoryRetentionOperationId.IsEmpty &&
                           receipt.DimmKeyOrientation == default &&
                           receipt.SequenceIndex == 0;

                default:
                    return false;
            }
        }

        private static bool IsKnownOperationKind(AssemblyOperationKind operationKind)
        {
            return operationKind == AssemblyOperationKind.AttachMotherboard ||
                   operationKind == AssemblyOperationKind.DetachMotherboard ||
                   operationKind == AssemblyOperationKind.SecureMotherboardFastener ||
                   operationKind == AssemblyOperationKind.UnsecureMotherboardFastener ||
                   operationKind == AssemblyOperationKind.SeatProcessor ||
                   operationKind == AssemblyOperationKind.RemoveProcessor ||
                   operationKind == AssemblyOperationKind.CloseProcessorRetention ||
                   operationKind == AssemblyOperationKind.OpenProcessorRetention ||
                   operationKind == AssemblyOperationKind.SeatMemoryModule ||
                   operationKind == AssemblyOperationKind.RemoveMemoryModule ||
                   operationKind == AssemblyOperationKind.CloseMemoryRetention ||
                   operationKind == AssemblyOperationKind.OpenMemoryRetention;
        }

        private static bool IsProcessorOperation(AssemblyOperationKind operationKind)
        {
            return operationKind == AssemblyOperationKind.SeatProcessor ||
                   operationKind == AssemblyOperationKind.RemoveProcessor ||
                   operationKind == AssemblyOperationKind.CloseProcessorRetention ||
                   operationKind == AssemblyOperationKind.OpenProcessorRetention;
        }

        private static bool IsMemoryOperation(AssemblyOperationKind operationKind)
        {
            return operationKind == AssemblyOperationKind.SeatMemoryModule ||
                   operationKind == AssemblyOperationKind.RemoveMemoryModule ||
                   operationKind == AssemblyOperationKind.CloseMemoryRetention ||
                   operationKind == AssemblyOperationKind.OpenMemoryRetention;
        }

        private bool ValidateReceiptTransition(AssemblyOperationReceipt receipt)
        {
            if (!IsMemoryOperation(receipt.OperationKind) &&
                receipt.PreviousMemorySlotState != receipt.ResultingMemorySlotState)
            {
                return false;
            }

            switch (receipt.OperationKind)
            {
                case AssemblyOperationKind.AttachMotherboard:
                    return receipt.SourceContainerId == _handsContainerId &&
                           receipt.TargetContainerId == _workbenchContainerId &&
                           receipt.SourceAttachOperationId.IsEmpty &&
                           receipt.PreviousSeatState == AssemblySeatState.Empty &&
                           receipt.ResultingSeatState ==
                               AssemblySeatState.SeatedUnsecured &&
                           receipt.PreviousProcessorSocketState ==
                               receipt.ResultingProcessorSocketState;

                case AssemblyOperationKind.DetachMotherboard:
                    return receipt.SourceContainerId == _workbenchContainerId &&
                           receipt.TargetContainerId == _handsContainerId &&
                           receipt.PreviousSeatState ==
                               AssemblySeatState.SeatedUnsecured &&
                           receipt.ResultingSeatState == AssemblySeatState.Empty &&
                           receipt.PreviousProcessorSocketState ==
                               receipt.ResultingProcessorSocketState &&
                           IsMatchingAttachReceipt(
                               receipt.SourceAttachOperationId,
                               receipt);

                case AssemblyOperationKind.SecureMotherboardFastener:
                    return receipt.PreviousSeatState ==
                               AssemblySeatState.SeatedUnsecured &&
                           receipt.ResultingSeatState ==
                               AssemblySeatState.SeatedSecured &&
                           receipt.PreviousProcessorSocketState ==
                               receipt.ResultingProcessorSocketState &&
                           receipt.SourceSecureOperationId.IsEmpty &&
                           IsMatchingAttachReceipt(
                               receipt.SourceAttachOperationId,
                               receipt);

                case AssemblyOperationKind.UnsecureMotherboardFastener:
                    if (receipt.PreviousSeatState != AssemblySeatState.SeatedSecured ||
                        receipt.ResultingSeatState != AssemblySeatState.SeatedUnsecured ||
                        receipt.PreviousProcessorSocketState !=
                            receipt.ResultingProcessorSocketState ||
                        !IsMatchingAttachReceipt(
                            receipt.SourceAttachOperationId,
                            receipt) ||
                        receipt.SourceSecureOperationId.IsEmpty ||
                        !_receipts.TryGetValue(
                            receipt.SourceSecureOperationId,
                            out AssemblyOperationReceipt secureReceipt))
                    {
                        return false;
                    }

                    return secureReceipt.OperationKind ==
                               AssemblyOperationKind.SecureMotherboardFastener &&
                           secureReceipt.AssemblyRevision < receipt.AssemblyRevision &&
                           secureReceipt.ItemId == receipt.ItemId &&
                           secureReceipt.SlotId == receipt.SlotId &&
                           secureReceipt.FastenerId == receipt.FastenerId &&
                           secureReceipt.SourceAttachOperationId ==
                               receipt.SourceAttachOperationId;

                case AssemblyOperationKind.SeatProcessor:
                    return receipt.PreviousSeatState == AssemblySeatState.SeatedSecured &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               ProcessorSocketState.EmptyOpen &&
                           receipt.ResultingProcessorSocketState ==
                               ProcessorSocketState.ProcessorSeatedOpen &&
                           IsMatchingMotherboardSecureLineage(receipt);

                case AssemblyOperationKind.RemoveProcessor:
                    return receipt.PreviousSeatState != AssemblySeatState.Empty &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               ProcessorSocketState.ProcessorSeatedOpen &&
                           receipt.ResultingProcessorSocketState ==
                               ProcessorSocketState.EmptyOpen &&
                           IsMatchingProcessorSeatReceipt(
                               receipt.SourceProcessorSeatOperationId,
                               receipt);

                case AssemblyOperationKind.CloseProcessorRetention:
                    return receipt.PreviousSeatState == AssemblySeatState.SeatedSecured &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               ProcessorSocketState.ProcessorSeatedOpen &&
                           receipt.ResultingProcessorSocketState ==
                               ProcessorSocketState.ProcessorRetained &&
                           receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                           IsMatchingProcessorSeatReceipt(
                               receipt.SourceProcessorSeatOperationId,
                               receipt);

                case AssemblyOperationKind.OpenProcessorRetention:
                    return receipt.PreviousSeatState != AssemblySeatState.Empty &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               ProcessorSocketState.ProcessorRetained &&
                           receipt.ResultingProcessorSocketState ==
                               ProcessorSocketState.ProcessorSeatedOpen &&
                           IsMatchingProcessorSeatReceipt(
                               receipt.SourceProcessorSeatOperationId,
                               receipt) &&
                           IsMatchingProcessorRetentionReceipt(
                               receipt.SourceProcessorRetentionOperationId,
                               receipt);

                case AssemblyOperationKind.SeatMemoryModule:
                    return receipt.PreviousSeatState == AssemblySeatState.SeatedSecured &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               receipt.ResultingProcessorSocketState &&
                           receipt.PreviousMemorySlotState == MemorySlotState.EmptyOpen &&
                           receipt.ResultingMemorySlotState ==
                               MemorySlotState.MemoryModuleSeatedOpen &&
                           receipt.DimmKeyOrientation ==
                               DimmKeyOrientation.NotchAligned &&
                           IsMatchingMotherboardSecureLineage(receipt);

                case AssemblyOperationKind.RemoveMemoryModule:
                    return receipt.PreviousSeatState != AssemblySeatState.Empty &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               receipt.ResultingProcessorSocketState &&
                           receipt.PreviousMemorySlotState ==
                               MemorySlotState.MemoryModuleSeatedOpen &&
                           receipt.ResultingMemorySlotState == MemorySlotState.EmptyOpen &&
                           IsMatchingMemorySeatReceipt(
                               receipt.SourceMemorySeatOperationId,
                               receipt);

                case AssemblyOperationKind.CloseMemoryRetention:
                    return receipt.PreviousSeatState == AssemblySeatState.SeatedSecured &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               receipt.ResultingProcessorSocketState &&
                           receipt.PreviousMemorySlotState ==
                               MemorySlotState.MemoryModuleSeatedOpen &&
                           receipt.ResultingMemorySlotState ==
                               MemorySlotState.MemoryModuleRetained &&
                           receipt.SourceMemoryRetentionOperationId.IsEmpty &&
                           IsMatchingMemorySeatReceipt(
                               receipt.SourceMemorySeatOperationId,
                               receipt);

                case AssemblyOperationKind.OpenMemoryRetention:
                    return receipt.PreviousSeatState != AssemblySeatState.Empty &&
                           receipt.ResultingSeatState == receipt.PreviousSeatState &&
                           receipt.PreviousProcessorSocketState ==
                               receipt.ResultingProcessorSocketState &&
                           receipt.PreviousMemorySlotState ==
                               MemorySlotState.MemoryModuleRetained &&
                           receipt.ResultingMemorySlotState ==
                               MemorySlotState.MemoryModuleSeatedOpen &&
                           IsMatchingMemorySeatReceipt(
                               receipt.SourceMemorySeatOperationId,
                               receipt) &&
                           IsMatchingMemoryRetentionReceipt(
                               receipt.SourceMemoryRetentionOperationId,
                               receipt);

                default:
                    return false;
            }
        }

        private bool ValidateReceiptHistory(AssemblyOperationReceipt[] receiptsByRevision)
        {
            AssemblySeatState foldedState = AssemblySeatState.Empty;
            StableId<ItemInstanceIdScope> foldedItemId = default;
            StableId<ProductDefinitionIdScope> foldedProductId = default;
            StableId<AssemblyOperationIdScope> foldedAttachOperationId = default;
            StableId<AssemblyOperationIdScope> foldedSecureOperationId = default;
            ProcessorSocketState foldedProcessorState = HasProcessorSocket
                ? ProcessorSocketState.EmptyOpen
                : ProcessorSocketState.Unsupported;
            StableId<ItemInstanceIdScope> foldedProcessorItemId = default;
            StableId<ProductDefinitionIdScope> foldedProcessorProductId = default;
            StableId<AssemblyOperationIdScope> foldedProcessorSeatOperationId = default;
            StableId<AssemblyOperationIdScope> foldedProcessorRetentionOperationId = default;
            MemorySlotState foldedMemoryState = HasMemorySlot
                ? MemorySlotState.EmptyOpen
                : MemorySlotState.Unsupported;
            StableId<ItemInstanceIdScope> foldedMemoryItemId = default;
            StableId<ProductDefinitionIdScope> foldedMemoryProductId = default;
            StableId<AssemblyOperationIdScope> foldedMemorySeatOperationId = default;
            StableId<AssemblyOperationIdScope> foldedMemoryRetentionOperationId = default;
            long foldedInventoryRevision = 0;

            for (int index = 0; index < receiptsByRevision.Length; index++)
            {
                AssemblyOperationReceipt receipt = receiptsByRevision[index];
                if (receipt == null ||
                    receipt.AssemblyRevision != index + 1L ||
                    receipt.PreviousSeatState != foldedState ||
                    receipt.PreviousProcessorSocketState != foldedProcessorState ||
                    receipt.PreviousMemorySlotState != foldedMemoryState ||
                    receipt.InventoryRevision < foldedInventoryRevision)
                {
                    return false;
                }

                bool inventoryTransfer =
                    receipt.OperationKind == AssemblyOperationKind.AttachMotherboard ||
                    receipt.OperationKind == AssemblyOperationKind.DetachMotherboard ||
                    receipt.OperationKind == AssemblyOperationKind.SeatProcessor ||
                    receipt.OperationKind == AssemblyOperationKind.RemoveProcessor ||
                    receipt.OperationKind == AssemblyOperationKind.SeatMemoryModule ||
                    receipt.OperationKind == AssemblyOperationKind.RemoveMemoryModule;
                if (inventoryTransfer &&
                    receipt.InventoryRevision <= foldedInventoryRevision)
                {
                    return false;
                }

                switch (receipt.OperationKind)
                {
                    case AssemblyOperationKind.AttachMotherboard:
                        if (foldedState != AssemblySeatState.Empty ||
                            !foldedItemId.IsEmpty ||
                            !foldedProductId.IsEmpty ||
                            !foldedAttachOperationId.IsEmpty ||
                            !foldedSecureOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedItemId = receipt.ItemId;
                        foldedProductId = receipt.ProductId;
                        foldedAttachOperationId = receipt.OperationId;
                        break;

                    case AssemblyOperationKind.DetachMotherboard:
                        if (foldedState != AssemblySeatState.SeatedUnsecured ||
                            foldedProcessorState != (HasProcessorSocket
                                ? ProcessorSocketState.EmptyOpen
                                : ProcessorSocketState.Unsupported) ||
                            foldedMemoryState != (HasMemorySlot
                                ? MemorySlotState.EmptyOpen
                                : MemorySlotState.Unsupported) ||
                            receipt.ItemId != foldedItemId ||
                            receipt.ProductId != foldedProductId ||
                            receipt.SourceAttachOperationId != foldedAttachOperationId ||
                            !receipt.SourceSecureOperationId.IsEmpty ||
                            !foldedSecureOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedItemId = default;
                        foldedProductId = default;
                        foldedAttachOperationId = default;
                        foldedSecureOperationId = default;
                        break;

                    case AssemblyOperationKind.SecureMotherboardFastener:
                        if (foldedState != AssemblySeatState.SeatedUnsecured ||
                            receipt.ItemId != foldedItemId ||
                            receipt.ProductId != foldedProductId ||
                            receipt.SourceAttachOperationId != foldedAttachOperationId ||
                            !receipt.SourceSecureOperationId.IsEmpty ||
                            !foldedSecureOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedSecureOperationId = receipt.OperationId;
                        break;

                    case AssemblyOperationKind.UnsecureMotherboardFastener:
                        if (foldedState != AssemblySeatState.SeatedSecured ||
                            receipt.ItemId != foldedItemId ||
                            receipt.ProductId != foldedProductId ||
                            receipt.SourceAttachOperationId != foldedAttachOperationId ||
                            receipt.SourceSecureOperationId != foldedSecureOperationId ||
                            foldedSecureOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedSecureOperationId = default;
                        break;

                    case AssemblyOperationKind.SeatProcessor:
                        if (foldedState != AssemblySeatState.SeatedSecured ||
                            foldedProcessorState != ProcessorSocketState.EmptyOpen ||
                            !foldedProcessorItemId.IsEmpty ||
                            !foldedProcessorProductId.IsEmpty ||
                            !foldedProcessorSeatOperationId.IsEmpty ||
                            !foldedProcessorRetentionOperationId.IsEmpty ||
                            receipt.SourceAttachOperationId !=
                                foldedAttachOperationId ||
                            receipt.SourceSecureOperationId !=
                                foldedSecureOperationId)
                        {
                            return false;
                        }

                        foldedProcessorItemId = receipt.ItemId;
                        foldedProcessorProductId = receipt.ProductId;
                        foldedProcessorSeatOperationId = receipt.OperationId;
                        break;

                    case AssemblyOperationKind.CloseProcessorRetention:
                        if (foldedState != AssemblySeatState.SeatedSecured ||
                            foldedProcessorState !=
                                ProcessorSocketState.ProcessorSeatedOpen ||
                            receipt.ItemId != foldedProcessorItemId ||
                            receipt.ProductId != foldedProcessorProductId ||
                            receipt.SourceProcessorSeatOperationId !=
                                foldedProcessorSeatOperationId ||
                            !foldedProcessorRetentionOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedProcessorRetentionOperationId = receipt.OperationId;
                        break;

                    case AssemblyOperationKind.OpenProcessorRetention:
                        if (foldedState == AssemblySeatState.Empty ||
                            foldedProcessorState != ProcessorSocketState.ProcessorRetained ||
                            receipt.ItemId != foldedProcessorItemId ||
                            receipt.ProductId != foldedProcessorProductId ||
                            receipt.SourceProcessorSeatOperationId !=
                                foldedProcessorSeatOperationId ||
                            receipt.SourceProcessorRetentionOperationId !=
                                foldedProcessorRetentionOperationId ||
                            foldedProcessorRetentionOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedProcessorRetentionOperationId = default;
                        break;

                    case AssemblyOperationKind.RemoveProcessor:
                        if (foldedState == AssemblySeatState.Empty ||
                            foldedProcessorState !=
                                ProcessorSocketState.ProcessorSeatedOpen ||
                            receipt.ItemId != foldedProcessorItemId ||
                            receipt.ProductId != foldedProcessorProductId ||
                            receipt.SourceProcessorSeatOperationId !=
                                foldedProcessorSeatOperationId ||
                            !receipt.SourceProcessorRetentionOperationId.IsEmpty ||
                            !foldedProcessorRetentionOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedProcessorItemId = default;
                        foldedProcessorProductId = default;
                        foldedProcessorSeatOperationId = default;
                        foldedProcessorRetentionOperationId = default;
                        break;

                    case AssemblyOperationKind.SeatMemoryModule:
                        if (foldedState != AssemblySeatState.SeatedSecured ||
                            foldedMemoryState != MemorySlotState.EmptyOpen ||
                            !foldedMemoryItemId.IsEmpty ||
                            !foldedMemoryProductId.IsEmpty ||
                            !foldedMemorySeatOperationId.IsEmpty ||
                            !foldedMemoryRetentionOperationId.IsEmpty ||
                            receipt.SourceAttachOperationId !=
                                foldedAttachOperationId ||
                            receipt.SourceSecureOperationId !=
                                foldedSecureOperationId ||
                            receipt.DimmKeyOrientation !=
                                DimmKeyOrientation.NotchAligned)
                        {
                            return false;
                        }

                        foldedMemoryItemId = receipt.ItemId;
                        foldedMemoryProductId = receipt.ProductId;
                        foldedMemorySeatOperationId = receipt.OperationId;
                        break;

                    case AssemblyOperationKind.CloseMemoryRetention:
                        if (foldedState != AssemblySeatState.SeatedSecured ||
                            foldedMemoryState !=
                                MemorySlotState.MemoryModuleSeatedOpen ||
                            receipt.ItemId != foldedMemoryItemId ||
                            receipt.ProductId != foldedMemoryProductId ||
                            receipt.SourceMemorySeatOperationId !=
                                foldedMemorySeatOperationId ||
                            !foldedMemoryRetentionOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedMemoryRetentionOperationId = receipt.OperationId;
                        break;

                    case AssemblyOperationKind.OpenMemoryRetention:
                        if (foldedState == AssemblySeatState.Empty ||
                            foldedMemoryState != MemorySlotState.MemoryModuleRetained ||
                            receipt.ItemId != foldedMemoryItemId ||
                            receipt.ProductId != foldedMemoryProductId ||
                            receipt.SourceMemorySeatOperationId !=
                                foldedMemorySeatOperationId ||
                            receipt.SourceMemoryRetentionOperationId !=
                                foldedMemoryRetentionOperationId ||
                            foldedMemoryRetentionOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedMemoryRetentionOperationId = default;
                        break;

                    case AssemblyOperationKind.RemoveMemoryModule:
                        if (foldedState == AssemblySeatState.Empty ||
                            foldedMemoryState !=
                                MemorySlotState.MemoryModuleSeatedOpen ||
                            receipt.ItemId != foldedMemoryItemId ||
                            receipt.ProductId != foldedMemoryProductId ||
                            receipt.SourceMemorySeatOperationId !=
                                foldedMemorySeatOperationId ||
                            !receipt.SourceMemoryRetentionOperationId.IsEmpty ||
                            !foldedMemoryRetentionOperationId.IsEmpty)
                        {
                            return false;
                        }

                        foldedMemoryItemId = default;
                        foldedMemoryProductId = default;
                        foldedMemorySeatOperationId = default;
                        foldedMemoryRetentionOperationId = default;
                        break;

                    default:
                        return false;
                }

                foldedState = receipt.ResultingSeatState;
                foldedProcessorState = receipt.ResultingProcessorSocketState;
                foldedMemoryState = receipt.ResultingMemorySlotState;
                foldedInventoryRevision = receipt.InventoryRevision;
            }

            return foldedState == _motherboardSeatState &&
                   foldedItemId == _motherboardItemId &&
                   foldedProductId == _motherboardProductId &&
                   foldedAttachOperationId == _installedByOperationId &&
                   foldedSecureOperationId == _securedByOperationId &&
                   foldedProcessorState == _processorSocketState &&
                   foldedProcessorItemId == _processorItemId &&
                   foldedProcessorProductId == _processorProductId &&
                   foldedProcessorSeatOperationId ==
                       _processorSeatedByOperationId &&
                   foldedProcessorRetentionOperationId ==
                       _processorRetainedByOperationId &&
                   foldedMemoryState == _memorySlotState &&
                   foldedMemoryItemId == _memoryItemId &&
                   foldedMemoryProductId == _memoryProductId &&
                   foldedMemorySeatOperationId == _memorySeatedByOperationId &&
                   foldedMemoryRetentionOperationId ==
                       _memoryRetainedByOperationId;
        }

        private bool IsMatchingAttachReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt attachReceipt) &&
                   attachReceipt.OperationKind == AssemblyOperationKind.AttachMotherboard &&
                   attachReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   attachReceipt.ItemId == descendant.ItemId &&
                   attachReceipt.SlotId == descendant.SlotId;
        }

        private bool IsMatchingMotherboardSecureLineage(
            AssemblyOperationReceipt descendant)
        {
            return !descendant.SourceAttachOperationId.IsEmpty &&
                   !descendant.SourceSecureOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       descendant.SourceAttachOperationId,
                       out AssemblyOperationReceipt attachReceipt) &&
                   _receipts.TryGetValue(
                       descendant.SourceSecureOperationId,
                       out AssemblyOperationReceipt secureReceipt) &&
                   attachReceipt.OperationKind ==
                       AssemblyOperationKind.AttachMotherboard &&
                   secureReceipt.OperationKind ==
                       AssemblyOperationKind.SecureMotherboardFastener &&
                   attachReceipt.AssemblyRevision < secureReceipt.AssemblyRevision &&
                   secureReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   attachReceipt.ItemId == secureReceipt.ItemId &&
                   attachReceipt.SlotId == MotherboardSlotId &&
                   secureReceipt.SlotId == MotherboardSlotId &&
                   secureReceipt.SourceAttachOperationId ==
                       attachReceipt.OperationId;
        }

        private bool IsMatchingProcessorSeatReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt seatReceipt) &&
                   seatReceipt.OperationKind == AssemblyOperationKind.SeatProcessor &&
                   seatReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   seatReceipt.ItemId == descendant.ItemId &&
                   seatReceipt.ProductId == descendant.ProductId &&
                   seatReceipt.SlotId == descendant.SlotId &&
                   seatReceipt.SourceAttachOperationId ==
                       descendant.SourceAttachOperationId &&
                   seatReceipt.SourceSecureOperationId ==
                       descendant.SourceSecureOperationId;
        }

        private bool IsMatchingProcessorRetentionReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationReceipt descendant)
        {
            return !operationId.IsEmpty &&
                   _receipts.TryGetValue(
                       operationId,
                       out AssemblyOperationReceipt retentionReceipt) &&
                   retentionReceipt.OperationKind ==
                       AssemblyOperationKind.CloseProcessorRetention &&
                   retentionReceipt.AssemblyRevision < descendant.AssemblyRevision &&
                   retentionReceipt.ItemId == descendant.ItemId &&
                   retentionReceipt.ProductId == descendant.ProductId &&
                   retentionReceipt.SlotId == descendant.SlotId &&
                   retentionReceipt.RetentionId == descendant.RetentionId &&
                   retentionReceipt.SourceProcessorSeatOperationId ==
                       descendant.SourceProcessorSeatOperationId;
        }

        private InventoryItemRecord GetItem(StableId<ItemInstanceIdScope> itemId)
        {
            _inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item);
            return item;
        }

        private static Failure MapInventoryFailure(Failure failure, bool attaching)
        {
            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return attaching
                    ? AssemblyFailures.WorkbenchCapacityExceeded
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
                return AssemblyFailures.ItemNotInActorHands;
            }

            return AssemblyFailures.InventoryTransferRejected;
        }

        private static Failure MapProcessorInventoryFailure(Failure failure, bool seating)
        {
            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return seating
                    ? AssemblyFailures.ProcessorSocketCapacityExceeded
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
