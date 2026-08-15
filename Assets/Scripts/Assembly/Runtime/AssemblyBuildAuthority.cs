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
        private readonly InventorySerializedTransferAccess _inventoryTransferAccess;
        private readonly MotherboardFormFactor _supportedMotherboardFormFactor;
        private readonly Dictionary<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt> _receipts =
            new Dictionary<StableId<AssemblyOperationIdScope>, AssemblyOperationReceipt>();

        private AssemblySeatState _motherboardSeatState = AssemblySeatState.Empty;
        private StableId<ItemInstanceIdScope> _motherboardItemId;
        private StableId<ProductDefinitionIdScope> _motherboardProductId;
        private StableId<AssemblyOperationIdScope> _installedByOperationId;

        private AssemblyBuildAuthority(
            PcComponentCatalog componentCatalog,
            InventoryAuthority inventory,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
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
            _handsContainerId = handsContainerId;
            _workbenchContainerId = workbenchContainerId;
            _supportedMotherboardFormFactor = supportedMotherboardFormFactor;
            _inventoryTransferAccess = inventoryTransferAccess;
        }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<AssemblySlotIdScope> MotherboardSlotId { get; }

        public StableId<ContainerIdScope> HandsContainerId => _handsContainerId;

        public StableId<ContainerIdScope> WorkbenchContainerId => _workbenchContainerId;

        public MotherboardFormFactor SupportedMotherboardFormFactor =>
            _supportedMotherboardFormFactor;

        public AssemblySeatState MotherboardSeatState => _motherboardSeatState;

        public StableId<ItemInstanceIdScope> MotherboardItemId => _motherboardItemId;

        public StableId<ProductDefinitionIdScope> MotherboardProductId => _motherboardProductId;

        public StableId<AssemblyOperationIdScope> InstalledByOperationId => _installedByOperationId;

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public static OperationResult<AssemblyBuildAuthority> Create(
            PcComponentCatalog componentCatalog,
            InventoryAuthority inventory,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<AssemblySlotIdScope> motherboardSlotId,
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
                _motherboardSeatState,
                Revision,
                _inventory.Revision);
            _receipts.Add(operationId, receipt);
            return OperationResult<AssemblyOperationReceipt>.Success(receipt);
        }

        public OperationResult EvaluateBenchmarkReadiness()
        {
            return _motherboardSeatState == AssemblySeatState.Empty
                ? OperationResult.Fail(AssemblyFailures.MotherboardMissing)
                : OperationResult.Fail(AssemblyFailures.MotherboardUnsecured);
        }

        public AssemblyBuildSnapshot GetSnapshot()
        {
            return new AssemblyBuildSnapshot(
                BuildId,
                ChassisId,
                MotherboardSlotId,
                _handsContainerId,
                _workbenchContainerId,
                _supportedMotherboardFormFactor,
                _motherboardSeatState,
                _motherboardItemId,
                _motherboardProductId,
                _installedByOperationId,
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
                    !_installedByOperationId.IsEmpty)
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
            }
            else if (_motherboardSeatState == AssemblySeatState.SeatedUnsecured)
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
            }
            else
            {
                return OperationResult.Fail(AssemblyFailures.InvariantViolation);
            }

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
                    receipt.SourceContainerId.IsEmpty ||
                    receipt.TargetContainerId.IsEmpty ||
                    receipt.SourceContainerId == receipt.TargetContainerId ||
                    receipt.AssemblyRevision <= 0 ||
                    receipt.AssemblyRevision > Revision ||
                    receipt.InventoryRevision <= 0 ||
                    (receipt.OperationKind != AssemblyOperationKind.AttachMotherboard &&
                     receipt.OperationKind != AssemblyOperationKind.DetachMotherboard))
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }

                if ((receipt.OperationKind == AssemblyOperationKind.AttachMotherboard &&
                     !receipt.SourceAttachOperationId.IsEmpty) ||
                    (receipt.OperationKind == AssemblyOperationKind.DetachMotherboard &&
                     receipt.SourceAttachOperationId.IsEmpty))
                {
                    return OperationResult.Fail(AssemblyFailures.InvariantViolation);
                }
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
