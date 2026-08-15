using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Authoritative single-chassis, single-motherboard-seat aggregate for the first bounded
    /// physical PC assembly slice. Presentation may project it but cannot own component state.
    /// </summary>
    public sealed class AssemblyBuildAuthority
    {
        private readonly PcComponentCatalog _componentCatalog;
        private readonly InventoryAuthority _inventory;
        private readonly StableId<ContainerIdScope> _handsContainerId;
        private readonly StableId<ContainerIdScope> _workbenchContainerId;
        private readonly StableId<AssemblyFastenerIdScope> _motherboardFastenerId;
        private readonly InventorySerializedTransferAccess _inventoryTransferAccess;
        private readonly MotherboardFormFactor _supportedMotherboardFormFactor;
        private readonly Dictionary<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt> _receipts =
            new Dictionary<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt>();

        private AssemblySeatState _motherboardSeatState = AssemblySeatState.Empty;
        private StableId<ItemInstanceIdScope> _motherboardItemId;
        private StableId<ProductDefinitionIdScope> _motherboardProductId;
        private StableId<AssemblyOperationIdScope> _installedByOperationId;
        private StableId<AssemblyOperationIdScope> _securedByOperationId;

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
                _inventory.Revision);
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
                _inventory.Revision);
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
                _inventory.Revision);
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
                _inventory.Revision);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult EvaluateBenchmarkReadiness()
        {
            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                return OperationResult.Fail(AssemblyFailures.MotherboardMissing);
            }

            return _motherboardSeatState == AssemblySeatState.SeatedUnsecured
                ? OperationResult.Fail(AssemblyFailures.MotherboardUnsecured)
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
                _supportedMotherboardFormFactor,
                _motherboardSeatState,
                _motherboardItemId,
                _motherboardProductId,
                _installedByOperationId,
                _securedByOperationId,
                Revision);
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

            if (_motherboardSeatState == AssemblySeatState.Empty)
            {
                if (!_motherboardItemId.IsEmpty ||
                    !_motherboardProductId.IsEmpty ||
                    !_installedByOperationId.IsEmpty ||
                    !_securedByOperationId.IsEmpty)
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

            if (Revision != _receipts.Count)
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

            var receiptsByRevision = new AssemblyOperationReceipt[_receipts.Count];
            foreach (KeyValuePair<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt> entry in _receipts)
            {
                AssemblyOperationReceipt receipt = entry.Value;
                if (receipt == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != receipt.OperationId ||
                    receipt.BuildId != BuildId ||
                    receipt.ChassisId != ChassisId ||
                    receipt.SlotId != MotherboardSlotId ||
                    receipt.ItemId.IsEmpty ||
                    receipt.ProductId.IsEmpty ||
                    receipt.AssemblyRevision <= 0 ||
                    receipt.AssemblyRevision > Revision ||
                    receipt.ExpectedAssemblyRevision != receipt.AssemblyRevision - 1L ||
                    receipt.InventoryRevision <= 0 ||
                    receipt.InventoryRevision > _inventory.Revision ||
                    (receipt.OperationKind != AssemblyOperationKind.AttachMotherboard &&
                     receipt.OperationKind != AssemblyOperationKind.DetachMotherboard &&
                     receipt.OperationKind !=
                         AssemblyOperationKind.SecureMotherboardFastener &&
                     receipt.OperationKind !=
                         AssemblyOperationKind.UnsecureMotherboardFastener))
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                int revisionIndex = checked((int)receipt.AssemblyRevision - 1);
                if (receiptsByRevision[revisionIndex] != null)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                receiptsByRevision[revisionIndex] = receipt;

                bool isInventoryTransfer =
                    receipt.OperationKind == AssemblyOperationKind.AttachMotherboard ||
                    receipt.OperationKind == AssemblyOperationKind.DetachMotherboard;
                if (isInventoryTransfer)
                {
                    if (receipt.SourceContainerId.IsEmpty ||
                        receipt.TargetContainerId.IsEmpty ||
                        receipt.SourceContainerId == receipt.TargetContainerId ||
                        !receipt.FastenerId.IsEmpty ||
                        !receipt.SourceSecureOperationId.IsEmpty ||
                        receipt.SequenceIndex != -1)
                    {
                        return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                    }
                }
                else if (!receipt.SourceContainerId.IsEmpty ||
                         !receipt.TargetContainerId.IsEmpty ||
                         receipt.FastenerId != _motherboardFastenerId ||
                         receipt.SequenceIndex != 0 ||
                         receipt.SourceAttachOperationId.IsEmpty)
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

        private bool ValidateReceiptTransition(AssemblyOperationReceipt receipt)
        {
            switch (receipt.OperationKind)
            {
                case AssemblyOperationKind.AttachMotherboard:
                    return receipt.SourceContainerId == _handsContainerId &&
                           receipt.TargetContainerId == _workbenchContainerId &&
                           receipt.SourceAttachOperationId.IsEmpty &&
                           receipt.PreviousSeatState == AssemblySeatState.Empty &&
                           receipt.ResultingSeatState ==
                               AssemblySeatState.SeatedUnsecured;

                case AssemblyOperationKind.DetachMotherboard:
                    return receipt.SourceContainerId == _workbenchContainerId &&
                           receipt.TargetContainerId == _handsContainerId &&
                           receipt.PreviousSeatState ==
                               AssemblySeatState.SeatedUnsecured &&
                           receipt.ResultingSeatState == AssemblySeatState.Empty &&
                           IsMatchingAttachReceipt(
                               receipt.SourceAttachOperationId,
                               receipt);

                case AssemblyOperationKind.SecureMotherboardFastener:
                    return receipt.PreviousSeatState ==
                               AssemblySeatState.SeatedUnsecured &&
                           receipt.ResultingSeatState ==
                               AssemblySeatState.SeatedSecured &&
                           receipt.SourceSecureOperationId.IsEmpty &&
                           IsMatchingAttachReceipt(
                               receipt.SourceAttachOperationId,
                               receipt);

                case AssemblyOperationKind.UnsecureMotherboardFastener:
                    if (receipt.PreviousSeatState != AssemblySeatState.SeatedSecured ||
                        receipt.ResultingSeatState != AssemblySeatState.SeatedUnsecured ||
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
            long foldedInventoryRevision = 0;

            for (int index = 0; index < receiptsByRevision.Length; index++)
            {
                AssemblyOperationReceipt receipt = receiptsByRevision[index];
                if (receipt == null ||
                    receipt.AssemblyRevision != index + 1L ||
                    receipt.PreviousSeatState != foldedState ||
                    receipt.InventoryRevision < foldedInventoryRevision)
                {
                    return false;
                }

                bool inventoryTransfer =
                    receipt.OperationKind == AssemblyOperationKind.AttachMotherboard ||
                    receipt.OperationKind == AssemblyOperationKind.DetachMotherboard;
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

                    default:
                        return false;
                }

                foldedState = receipt.ResultingSeatState;
                foldedInventoryRevision = receipt.InventoryRevision;
            }

            return foldedState == _motherboardSeatState &&
                   foldedItemId == _motherboardItemId &&
                   foldedProductId == _motherboardProductId &&
                   foldedAttachOperationId == _installedByOperationId &&
                   foldedSecureOperationId == _securedByOperationId;
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
    }
}
