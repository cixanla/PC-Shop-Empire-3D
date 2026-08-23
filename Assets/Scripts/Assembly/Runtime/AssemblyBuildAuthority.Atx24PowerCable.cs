using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private Atx24PowerCableDefinition _atx24PowerCableDefinition;
        private InventorySerializedTransferAccess _atx24PowerCableInventoryTransferAccess;
        private Atx24PowerCableState _atx24PowerCableState =
            Atx24PowerCableState.Unsupported;
        private StableId<ItemInstanceIdScope> _atx24PowerCableItemId;
        private StableId<ProductDefinitionIdScope> _atx24PowerCableProductId;
        private StableId<AssemblyOperationIdScope> _atx24PowerCableRoutedByOperationId;
        private readonly Dictionary<StableId<AssemblyOperationIdScope>,
            Atx24PowerCableOperationReceipt> _atx24PowerCableReceipts =
                new Dictionary<StableId<AssemblyOperationIdScope>,
                    Atx24PowerCableOperationReceipt>();

        public bool HasAtx24PowerCableRoute => _atx24PowerCableDefinition.IsValid;

        public Atx24PowerCableDefinition Atx24PowerCableDefinition =>
            _atx24PowerCableDefinition;

        public Atx24PowerCableTopology Atx24PowerCableTopology =>
            _atx24PowerCableDefinition.Topology;

        public StableId<ContainerIdScope> Atx24PowerCableRouteContainerId =>
            _atx24PowerCableDefinition.RouteContainerId;

        public Atx24PowerCableState Atx24PowerCableState => _atx24PowerCableState;

        public bool IsAtx24PowerCableRouted =>
            _atx24PowerCableState == Atx24PowerCableState.Routed;

        public StableId<ItemInstanceIdScope> Atx24PowerCableItemId =>
            _atx24PowerCableItemId;

        public StableId<ProductDefinitionIdScope> Atx24PowerCableProductId =>
            _atx24PowerCableProductId;

        public StableId<AssemblyOperationIdScope> Atx24PowerCableRoutedByOperationId =>
            _atx24PowerCableRoutedByOperationId;

        public long Atx24PowerCableRevision { get; private set; }

        public int Atx24PowerCableReceiptCount => _atx24PowerCableReceipts.Count;

        public OperationResult<Atx24PowerCableOperationReceipt> RouteAtx24PowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            long expectedCableRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.ContainsKey(operationId) ||
                _eps12vPowerCableReceipts.ContainsKey(operationId))
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_atx24PowerCableReceipts.TryGetValue(
                    operationId,
                    out Atx24PowerCableOperationReceipt replay))
            {
                return replay.MatchesRoute(
                        operationId,
                        BuildId,
                        ChassisId,
                        itemId,
                        _atx24PowerCableDefinition.ProductId,
                        _handsContainerId,
                        _atx24PowerCableDefinition.RouteContainerId,
                        _atx24PowerCableDefinition,
                        orientation,
                        sourceMotherboardSecureOperationId,
                        sourcePowerSupplyRetentionOperationId,
                        expectedCableRevision)
                    ? OperationResult<Atx24PowerCableOperationReceipt>.Success(replay)
                    : OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflight = ValidateAtx24PowerCableRoute(
                itemId,
                orientation,
                sourceMotherboardSecureOperationId,
                sourcePowerSupplyRetentionOperationId,
                expectedCableRevision);
            if (!preflight.IsNone)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(preflight);
            }

            _inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _atx24PowerCableDefinition.RouteContainerId,
                    _atx24PowerCableInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    MapAtx24PowerCableInventoryFailure(prepared.Error, routing: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    MapAtx24PowerCableInventoryFailure(committed.Error, routing: true));
            }

            _atx24PowerCableState = Atx24PowerCableState.Routed;
            _atx24PowerCableItemId = item.Id;
            _atx24PowerCableProductId = item.ProductId;
            _atx24PowerCableRoutedByOperationId = operationId;
            Atx24PowerCableRevision++;

            var receipt = new Atx24PowerCableOperationReceipt(
                operationId,
                Atx24PowerCableOperationKind.Route,
                BuildId,
                ChassisId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _atx24PowerCableDefinition.RouteContainerId,
                _atx24PowerCableDefinition,
                orientation,
                Atx24PowerCableState.Loose,
                Atx24PowerCableState.Routed,
                sourceMotherboardSecureOperationId,
                sourcePowerSupplyRetentionOperationId,
                default,
                expectedCableRevision,
                Atx24PowerCableRevision,
                _inventory.Revision);
            _atx24PowerCableReceipts.Add(operationId, receipt);
            return OperationResult<Atx24PowerCableOperationReceipt>.Success(receipt);
        }

        public OperationResult<Atx24PowerCableOperationReceipt> UnrouteAtx24PowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.ContainsKey(operationId) ||
                _eps12vPowerCableReceipts.ContainsKey(operationId))
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_atx24PowerCableReceipts.TryGetValue(
                    operationId,
                    out Atx24PowerCableOperationReceipt replay))
            {
                if (!_atx24PowerCableReceipts.TryGetValue(
                        sourceRouteOperationId,
                        out Atx24PowerCableOperationReceipt sourceRouteReceipt) ||
                    sourceRouteReceipt.OperationKind !=
                        Atx24PowerCableOperationKind.Route)
                {
                    return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
                }

                return replay.MatchesUnroute(
                        operationId,
                        BuildId,
                        ChassisId,
                        itemId,
                        _atx24PowerCableDefinition.ProductId,
                        _atx24PowerCableDefinition.RouteContainerId,
                        _handsContainerId,
                        _atx24PowerCableDefinition,
                        sourceRouteReceipt.SourceMotherboardSecureOperationId,
                        sourceRouteReceipt.SourcePowerSupplyRetentionOperationId,
                        sourceRouteOperationId,
                        expectedCableRevision)
                    ? OperationResult<Atx24PowerCableOperationReceipt>.Success(replay)
                    : OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflight = ValidateAtx24PowerCableUnroute(
                itemId,
                sourceRouteOperationId,
                expectedCableRevision);
            if (!preflight.IsNone)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(preflight);
            }

            InventoryItemRecord item = GetItem(itemId);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _atx24PowerCableInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    MapAtx24PowerCableInventoryFailure(prepared.Error, routing: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                    MapAtx24PowerCableInventoryFailure(committed.Error, routing: false));
            }

            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId =
                _atx24PowerCableReceipts[sourceRouteOperationId]
                    .SourceMotherboardSecureOperationId;
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId =
                _atx24PowerCableReceipts[sourceRouteOperationId]
                    .SourcePowerSupplyRetentionOperationId;
            _atx24PowerCableState = Atx24PowerCableState.Loose;
            _atx24PowerCableItemId = default;
            _atx24PowerCableProductId = default;
            _atx24PowerCableRoutedByOperationId = default;
            Atx24PowerCableRevision++;

            var receipt = new Atx24PowerCableOperationReceipt(
                operationId,
                Atx24PowerCableOperationKind.Unroute,
                BuildId,
                ChassisId,
                item.Id,
                item.ProductId,
                _atx24PowerCableDefinition.RouteContainerId,
                _handsContainerId,
                _atx24PowerCableDefinition,
                PowerCableKeyOrientation.Keyed,
                Atx24PowerCableState.Routed,
                Atx24PowerCableState.Loose,
                sourceMotherboardSecureOperationId,
                sourcePowerSupplyRetentionOperationId,
                sourceRouteOperationId,
                expectedCableRevision,
                Atx24PowerCableRevision,
                _inventory.Revision);
            _atx24PowerCableReceipts.Add(operationId, receipt);
            return OperationResult<Atx24PowerCableOperationReceipt>.Success(receipt);
        }

        public bool TryGetAtx24PowerCableReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            out Atx24PowerCableOperationReceipt receipt)
        {
            return _atx24PowerCableReceipts.TryGetValue(operationId, out receipt);
        }

        public IReadOnlyList<Atx24PowerCableOperationReceipt>
            GetAtx24PowerCableReceipts()
        {
            var receipts = new List<Atx24PowerCableOperationReceipt>(
                _atx24PowerCableReceipts.Values);
            receipts.Sort((left, right) =>
            {
                int revision = left.CableRevision.CompareTo(right.CableRevision);
                return revision != 0
                    ? revision
                    : string.Compare(
                        left.OperationId.Value,
                        right.OperationId.Value,
                        StringComparison.Ordinal);
            });
            return Array.AsReadOnly(receipts.ToArray());
        }

        public OperationResult ValidateAtx24PowerCableReceiptHistory()
        {
            if (!HasAtx24PowerCableRoute)
            {
                return Atx24PowerCableRevision == 0 &&
                       _atx24PowerCableReceipts.Count == 0
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            if (Atx24PowerCableRevision != _atx24PowerCableReceipts.Count)
            {
                return OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            IReadOnlyList<Atx24PowerCableOperationReceipt> receipts =
                GetAtx24PowerCableReceipts();
            Atx24PowerCableState foldedState = Atx24PowerCableState.Loose;
            StableId<ItemInstanceIdScope> foldedItemId = default;
            StableId<ProductDefinitionIdScope> foldedProductId = default;
            StableId<AssemblyOperationIdScope> foldedRouteOperationId = default;
            long previousInventoryRevision = -1;

            for (int index = 0; index < receipts.Count; index++)
            {
                Atx24PowerCableOperationReceipt receipt = receipts[index];
                if (receipt == null ||
                    receipt.CableRevision != index + 1L ||
                    receipt.ExpectedCableRevision != index ||
                    receipt.BuildId != BuildId ||
                    receipt.ChassisId != ChassisId ||
                    receipt.ItemId.IsEmpty ||
                    receipt.ProductId != _atx24PowerCableDefinition.ProductId ||
                    !receipt.Definition.HasExactIdentity(_atx24PowerCableDefinition) ||
                    receipt.InventoryRevision <= previousInventoryRevision ||
                    receipt.PreviousState != foldedState)
                {
                    return OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
                }

                if (receipt.OperationKind == Atx24PowerCableOperationKind.Route)
                {
                    if (foldedState != Atx24PowerCableState.Loose ||
                        receipt.SourceContainerId != _handsContainerId ||
                        receipt.TargetContainerId !=
                            _atx24PowerCableDefinition.RouteContainerId ||
                        receipt.Orientation != PowerCableKeyOrientation.Keyed ||
                        receipt.ResultingState != Atx24PowerCableState.Routed ||
                        receipt.SourceMotherboardSecureOperationId.IsEmpty ||
                        receipt.SourcePowerSupplyRetentionOperationId.IsEmpty ||
                        !receipt.SourceRouteOperationId.IsEmpty)
                    {
                        return OperationResult.Fail(
                            AssemblyFailures.PowerCableReceiptHistoryInvalid);
                    }

                    foldedItemId = receipt.ItemId;
                    foldedProductId = receipt.ProductId;
                    foldedRouteOperationId = receipt.OperationId;
                }
                else if (receipt.OperationKind == Atx24PowerCableOperationKind.Unroute)
                {
                    if (foldedState != Atx24PowerCableState.Routed ||
                        receipt.ItemId != foldedItemId ||
                        receipt.ProductId != foldedProductId ||
                        receipt.SourceContainerId !=
                            _atx24PowerCableDefinition.RouteContainerId ||
                        receipt.TargetContainerId != _handsContainerId ||
                        receipt.SourceRouteOperationId != foldedRouteOperationId ||
                        receipt.ResultingState != Atx24PowerCableState.Loose)
                    {
                        return OperationResult.Fail(
                            AssemblyFailures.PowerCableReceiptHistoryInvalid);
                    }

                    foldedItemId = default;
                    foldedProductId = default;
                    foldedRouteOperationId = default;
                }
                else
                {
                    return OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
                }

                foldedState = receipt.ResultingState;
                previousInventoryRevision = receipt.InventoryRevision;
            }

            return foldedState == _atx24PowerCableState &&
                   foldedItemId == _atx24PowerCableItemId &&
                   foldedProductId == _atx24PowerCableProductId &&
                   foldedRouteOperationId == _atx24PowerCableRoutedByOperationId
                ? OperationResult.Success()
                : OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
        }

        private Failure ValidateAtx24PowerCableRoute(
            StableId<ItemInstanceIdScope> itemId,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            long expectedCableRevision)
        {
            if (!HasAtx24PowerCableRoute)
            {
                return AssemblyFailures.PowerCableUnsupported;
            }

            if (expectedCableRevision != Atx24PowerCableRevision)
            {
                return AssemblyFailures.PlanStale;
            }

            if (Atx24PowerCableRevision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (_atx24PowerCableState != Atx24PowerCableState.Loose)
            {
                return AssemblyFailures.PowerCableAlreadyRouted;
            }

            if (orientation != PowerCableKeyOrientation.Keyed)
            {
                return AssemblyFailures.PowerCableOrientationMismatch;
            }

            if (_motherboardSeatState != AssemblySeatState.SeatedSecured ||
                sourceMotherboardSecureOperationId.IsEmpty ||
                sourceMotherboardSecureOperationId != _securedByOperationId ||
                !_receipts.TryGetValue(
                    sourceMotherboardSecureOperationId,
                    out AssemblyOperationReceipt motherboardReceipt) ||
                motherboardReceipt.OperationKind !=
                    AssemblyOperationKind.SecureMotherboardFastener)
            {
                return AssemblyFailures.PowerCableHostMotherboardUnsecured;
            }

            if (_powerSupplyBayState != PowerSupplyBayState.PowerSupplyRetained ||
                sourcePowerSupplyRetentionOperationId.IsEmpty ||
                sourcePowerSupplyRetentionOperationId !=
                    _powerSupplyRetainedByOperationId ||
                !_receipts.TryGetValue(
                    sourcePowerSupplyRetentionOperationId,
                    out AssemblyOperationReceipt powerSupplyReceipt) ||
                powerSupplyReceipt.OperationKind !=
                    AssemblyOperationKind.RetainPowerSupply)
            {
                return AssemblyFailures.PowerCableHostPowerSupplyUnretained;
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item))
            {
                return AssemblyFailures.UnknownItem;
            }

            if (item.ContainerId != _handsContainerId)
            {
                return AssemblyFailures.ItemNotInActorHands;
            }

            return item.ProductId == _atx24PowerCableDefinition.ProductId
                ? Failure.None
                : AssemblyFailures.PowerCableProductMismatch;
        }

        private Failure ValidateAtx24PowerCableUnroute(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            if (!HasAtx24PowerCableRoute)
            {
                return AssemblyFailures.PowerCableUnsupported;
            }

            if (expectedCableRevision != Atx24PowerCableRevision)
            {
                return AssemblyFailures.PlanStale;
            }

            if (Atx24PowerCableRevision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (_atx24PowerCableState != Atx24PowerCableState.Routed ||
                itemId != _atx24PowerCableItemId)
            {
                return AssemblyFailures.PowerCableNotRouted;
            }

            if (sourceRouteOperationId.IsEmpty ||
                sourceRouteOperationId != _atx24PowerCableRoutedByOperationId ||
                !_atx24PowerCableReceipts.TryGetValue(
                    sourceRouteOperationId,
                    out Atx24PowerCableOperationReceipt routeReceipt) ||
                routeReceipt.OperationKind != Atx24PowerCableOperationKind.Route ||
                routeReceipt.ItemId != itemId)
            {
                return AssemblyFailures.PlanStale;
            }

            return IsAtx24PowerCableRoutedItem(itemId)
                ? Failure.None
                : AssemblyFailures.PowerCableNotRouted;
        }

        private bool ValidateAtx24PowerCableStateInvariants()
        {
            if (!HasAtx24PowerCableRoute)
            {
                return _atx24PowerCableInventoryTransferAccess == null &&
                       _atx24PowerCableState == Atx24PowerCableState.Unsupported &&
                       !_atx24PowerCableDefinition.HasAnyValue &&
                       _atx24PowerCableItemId.IsEmpty &&
                       _atx24PowerCableProductId.IsEmpty &&
                       _atx24PowerCableRoutedByOperationId.IsEmpty &&
                       Atx24PowerCableRevision == 0 &&
                       _atx24PowerCableReceipts.Count == 0;
            }

            if (_atx24PowerCableInventoryTransferAccess == null ||
                _atx24PowerCableDefinition.RouteContainerId == _handsContainerId ||
                _atx24PowerCableDefinition.RouteContainerId == _workbenchContainerId ||
                _atx24PowerCableDefinition.RouteContainerId ==
                    _processorSocketContainerId ||
                _atx24PowerCableDefinition.RouteContainerId ==
                    _memorySlotDefinition.ContainerId ||
                _atx24PowerCableDefinition.RouteContainerId ==
                    _storageSlotDefinition.ContainerId ||
                _atx24PowerCableDefinition.RouteContainerId ==
                    _processorCoolerSlotDefinition.ContainerId ||
                _atx24PowerCableDefinition.RouteContainerId ==
                    _graphicsCardSlotDefinition.ContainerId ||
                _atx24PowerCableDefinition.RouteContainerId ==
                    _powerSupplyBayDefinition.ContainerId ||
                !_inventory.TryGetContainer(
                    _atx24PowerCableDefinition.RouteContainerId,
                    out InventoryContainerDefinition routeContainer) ||
                routeContainer.Kind != InventoryContainerKind.Workbench ||
                routeContainer.UnitCapacity != 1 ||
                !_componentCatalog.OwnerCatalog.TryGet(
                    _atx24PowerCableDefinition.ProductId,
                    out ProductDefinition cableProduct) ||
                cableProduct.TrackingPolicy != ProductTrackingPolicy.SerializedInstance ||
                !_componentCatalog.TryGet(
                    _atx24PowerCableDefinition.ProductId,
                    out PcComponentSpecification cableSpecification) ||
                cableSpecification.Kind != PcComponentKind.PowerCable ||
                cableSpecification.PowerCableType !=
                    PowerCableType.ModularAtx24SplitPsuToMotherboard)
            {
                return false;
            }

            if (_atx24PowerCableState == Atx24PowerCableState.Loose)
            {
                return _atx24PowerCableItemId.IsEmpty &&
                       _atx24PowerCableProductId.IsEmpty &&
                       _atx24PowerCableRoutedByOperationId.IsEmpty &&
                       _inventory.GetContainerQuantity(
                           _atx24PowerCableDefinition.RouteContainerId).Value == 0 &&
                       ValidateAtx24PowerCableReceiptHistory().IsSuccess;
            }

            return _atx24PowerCableState == Atx24PowerCableState.Routed &&
                   _atx24PowerCableProductId ==
                       _atx24PowerCableDefinition.ProductId &&
                   !_atx24PowerCableRoutedByOperationId.IsEmpty &&
                   IsAtx24PowerCableRoutedItem(_atx24PowerCableItemId) &&
                   ValidateAtx24PowerCableReceiptHistory().IsSuccess;
        }

        private bool IsAtx24PowerCableRoutedItem(
            StableId<ItemInstanceIdScope> itemId)
        {
            return !itemId.IsEmpty &&
                   itemId == _atx24PowerCableItemId &&
                   _inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item) &&
                   item.ProductId == _atx24PowerCableDefinition.ProductId &&
                   item.ContainerId ==
                       _atx24PowerCableDefinition.RouteContainerId;
        }

        private static Failure MapAtx24PowerCableInventoryFailure(
            Failure failure,
            bool routing)
        {
            if (failure == InventoryFailures.RevisionOverflow)
            {
                return AssemblyFailures.InventoryRevisionOverflow;
            }

            if (failure == InventoryFailures.SerializedTransferPlanStale)
            {
                return AssemblyFailures.InventoryTransferStale;
            }

            if (failure == InventoryFailures.ContainerCapacityExceeded)
            {
                return routing
                    ? AssemblyFailures.PowerCableAlreadyRouted
                    : AssemblyFailures.HandsCapacityExceeded;
            }

            return failure == InventoryFailures.SerializedTransferAccessInvalid ||
                   failure == InventoryFailures.SerializedTransferContainerManaged
                ? AssemblyFailures.PlanForeign
                : AssemblyFailures.InventoryTransferRejected;
        }
    }
}
