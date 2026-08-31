using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private PcieGpuPowerCableDefinition _pcieGpuPowerCableDefinition;
        private InventorySerializedTransferAccess _pcieGpuPowerCableInventoryTransferAccess;
        private PcieGpuPowerCableState _pcieGpuPowerCableState =
            PcieGpuPowerCableState.Unsupported;
        private StableId<ItemInstanceIdScope> _pcieGpuPowerCableItemId;
        private StableId<ProductDefinitionIdScope> _pcieGpuPowerCableProductId;
        private StableId<AssemblyOperationIdScope> _pcieGpuPowerCableRoutedByOperationId;
        private readonly Dictionary<StableId<AssemblyOperationIdScope>,
            PcieGpuPowerCableOperationReceipt> _pcieGpuPowerCableReceipts =
                new Dictionary<StableId<AssemblyOperationIdScope>,
                    PcieGpuPowerCableOperationReceipt>();

        public bool HasPcieGpuPowerCableRoute => _pcieGpuPowerCableDefinition.IsValid;

        public PcieGpuPowerCableDefinition PcieGpuPowerCableDefinition =>
            _pcieGpuPowerCableDefinition;

        public PcieGpuPowerCableTopology PcieGpuPowerCableTopology =>
            _pcieGpuPowerCableDefinition.Topology;

        public StableId<ContainerIdScope> PcieGpuPowerCableRouteContainerId =>
            _pcieGpuPowerCableDefinition.RouteContainerId;

        public PcieGpuPowerCableState PcieGpuPowerCableState => _pcieGpuPowerCableState;

        public bool IsPcieGpuPowerCableRouted =>
            _pcieGpuPowerCableState == PcieGpuPowerCableState.Routed;

        public StableId<ItemInstanceIdScope> PcieGpuPowerCableItemId =>
            _pcieGpuPowerCableItemId;

        public StableId<ProductDefinitionIdScope> PcieGpuPowerCableProductId =>
            _pcieGpuPowerCableProductId;

        public StableId<AssemblyOperationIdScope> PcieGpuPowerCableRoutedByOperationId =>
            _pcieGpuPowerCableRoutedByOperationId;

        public long PcieGpuPowerCableRevision { get; private set; }

        public int PcieGpuPowerCableReceiptCount => _pcieGpuPowerCableReceipts.Count;

        public OperationResult<PcieGpuPowerCableOperationReceipt> RoutePcieGpuPowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardRetentionOperationId,
            long expectedCableRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.ContainsKey(operationId) ||
                _atx24PowerCableReceipts.ContainsKey(operationId) ||
                _eps12vPowerCableReceipts.ContainsKey(operationId))
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_pcieGpuPowerCableReceipts.TryGetValue(
                    operationId,
                    out PcieGpuPowerCableOperationReceipt replay))
            {
                return replay.MatchesRoute(
                        operationId,
                        BuildId,
                        ChassisId,
                        itemId,
                        _pcieGpuPowerCableDefinition.ProductId,
                        _handsContainerId,
                        _pcieGpuPowerCableDefinition.RouteContainerId,
                        _pcieGpuPowerCableDefinition,
                        orientation,
                        sourceMotherboardSecureOperationId,
                        sourcePowerSupplyRetentionOperationId,
                        sourceGraphicsCardRetentionOperationId,
                        expectedCableRevision)
                    ? OperationResult<PcieGpuPowerCableOperationReceipt>.Success(replay)
                    : OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflight = ValidatePcieGpuPowerCableRoute(
                itemId,
                orientation,
                sourceMotherboardSecureOperationId,
                sourcePowerSupplyRetentionOperationId,
                sourceGraphicsCardRetentionOperationId,
                expectedCableRevision);
            if (!preflight.IsNone)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(preflight);
            }

            _inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _pcieGpuPowerCableDefinition.RouteContainerId,
                    _pcieGpuPowerCableInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    MapPcieGpuPowerCableInventoryFailure(prepared.Error, routing: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    MapPcieGpuPowerCableInventoryFailure(committed.Error, routing: true));
            }

            _pcieGpuPowerCableState = PcieGpuPowerCableState.Routed;
            _pcieGpuPowerCableItemId = item.Id;
            _pcieGpuPowerCableProductId = item.ProductId;
            _pcieGpuPowerCableRoutedByOperationId = operationId;
            PcieGpuPowerCableRevision++;

            var receipt = new PcieGpuPowerCableOperationReceipt(
                operationId,
                PcieGpuPowerCableOperationKind.Route,
                BuildId,
                ChassisId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _pcieGpuPowerCableDefinition.RouteContainerId,
                _pcieGpuPowerCableDefinition,
                orientation,
                PcieGpuPowerCableState.Loose,
                PcieGpuPowerCableState.Routed,
                sourceMotherboardSecureOperationId,
                sourcePowerSupplyRetentionOperationId,
                sourceGraphicsCardRetentionOperationId,
                default,
                expectedCableRevision,
                PcieGpuPowerCableRevision,
                _inventory.Revision);
            _pcieGpuPowerCableReceipts.Add(operationId, receipt);
            return OperationResult<PcieGpuPowerCableOperationReceipt>.Success(receipt);
        }

        public OperationResult<PcieGpuPowerCableOperationReceipt> UnroutePcieGpuPowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.ContainsKey(operationId) ||
                _atx24PowerCableReceipts.ContainsKey(operationId) ||
                _eps12vPowerCableReceipts.ContainsKey(operationId))
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_pcieGpuPowerCableReceipts.TryGetValue(
                    operationId,
                    out PcieGpuPowerCableOperationReceipt replay))
            {
                if (!_pcieGpuPowerCableReceipts.TryGetValue(
                        sourceRouteOperationId,
                        out PcieGpuPowerCableOperationReceipt sourceRouteReceipt) ||
                    sourceRouteReceipt.OperationKind !=
                        PcieGpuPowerCableOperationKind.Route)
                {
                    return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
                }

                return replay.MatchesUnroute(
                        operationId,
                        BuildId,
                        ChassisId,
                        itemId,
                        _pcieGpuPowerCableDefinition.ProductId,
                        _pcieGpuPowerCableDefinition.RouteContainerId,
                        _handsContainerId,
                        _pcieGpuPowerCableDefinition,
                        sourceRouteReceipt.SourceMotherboardSecureOperationId,
                        sourceRouteReceipt.SourcePowerSupplyRetentionOperationId,
                        sourceRouteReceipt.SourceGraphicsCardRetentionOperationId,
                        sourceRouteOperationId,
                        expectedCableRevision)
                    ? OperationResult<PcieGpuPowerCableOperationReceipt>.Success(replay)
                    : OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflight = ValidatePcieGpuPowerCableUnroute(
                itemId,
                sourceRouteOperationId,
                expectedCableRevision);
            if (!preflight.IsNone)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(preflight);
            }

            InventoryItemRecord item = GetItem(itemId);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _pcieGpuPowerCableInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    MapPcieGpuPowerCableInventoryFailure(prepared.Error, routing: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                    MapPcieGpuPowerCableInventoryFailure(committed.Error, routing: false));
            }

            PcieGpuPowerCableOperationReceipt sourceRoute =
                _pcieGpuPowerCableReceipts[sourceRouteOperationId];
            _pcieGpuPowerCableState = PcieGpuPowerCableState.Loose;
            _pcieGpuPowerCableItemId = default;
            _pcieGpuPowerCableProductId = default;
            _pcieGpuPowerCableRoutedByOperationId = default;
            PcieGpuPowerCableRevision++;

            var receipt = new PcieGpuPowerCableOperationReceipt(
                operationId,
                PcieGpuPowerCableOperationKind.Unroute,
                BuildId,
                ChassisId,
                item.Id,
                item.ProductId,
                _pcieGpuPowerCableDefinition.RouteContainerId,
                _handsContainerId,
                _pcieGpuPowerCableDefinition,
                PowerCableKeyOrientation.Keyed,
                PcieGpuPowerCableState.Routed,
                PcieGpuPowerCableState.Loose,
                sourceRoute.SourceMotherboardSecureOperationId,
                sourceRoute.SourcePowerSupplyRetentionOperationId,
                sourceRoute.SourceGraphicsCardRetentionOperationId,
                sourceRouteOperationId,
                expectedCableRevision,
                PcieGpuPowerCableRevision,
                _inventory.Revision);
            _pcieGpuPowerCableReceipts.Add(operationId, receipt);
            return OperationResult<PcieGpuPowerCableOperationReceipt>.Success(receipt);
        }

        public bool TryGetPcieGpuPowerCableReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            out PcieGpuPowerCableOperationReceipt receipt)
        {
            return _pcieGpuPowerCableReceipts.TryGetValue(operationId, out receipt);
        }

        public IReadOnlyList<PcieGpuPowerCableOperationReceipt>
            GetPcieGpuPowerCableReceipts()
        {
            var receipts = new List<PcieGpuPowerCableOperationReceipt>(
                _pcieGpuPowerCableReceipts.Values);
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

        public OperationResult ValidatePcieGpuPowerCableReceiptHistory()
        {
            if (!HasPcieGpuPowerCableRoute)
            {
                return PcieGpuPowerCableRevision == 0 &&
                       _pcieGpuPowerCableReceipts.Count == 0
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            if (PcieGpuPowerCableRevision != _pcieGpuPowerCableReceipts.Count)
            {
                return OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            IReadOnlyList<PcieGpuPowerCableOperationReceipt> receipts =
                GetPcieGpuPowerCableReceipts();
            PcieGpuPowerCableState foldedState = PcieGpuPowerCableState.Loose;
            StableId<ItemInstanceIdScope> foldedItemId = default;
            StableId<ProductDefinitionIdScope> foldedProductId = default;
            StableId<AssemblyOperationIdScope> foldedRouteOperationId = default;
            StableId<AssemblyOperationIdScope> foldedMotherboardOperationId = default;
            StableId<AssemblyOperationIdScope> foldedPowerSupplyOperationId = default;
            StableId<AssemblyOperationIdScope> foldedGraphicsCardOperationId = default;
            long previousInventoryRevision = -1;

            for (int index = 0; index < receipts.Count; index++)
            {
                PcieGpuPowerCableOperationReceipt receipt = receipts[index];
                if (receipt == null ||
                    receipt.CableRevision != index + 1L ||
                    receipt.ExpectedCableRevision != index ||
                    receipt.BuildId != BuildId ||
                    receipt.ChassisId != ChassisId ||
                    receipt.ItemId.IsEmpty ||
                    receipt.ProductId != _pcieGpuPowerCableDefinition.ProductId ||
                    !receipt.Definition.HasExactIdentity(_pcieGpuPowerCableDefinition) ||
                    receipt.RouteFingerprint !=
                        _pcieGpuPowerCableDefinition.Topology.Fingerprint ||
                    receipt.InventoryRevision <= previousInventoryRevision ||
                    receipt.PreviousState != foldedState ||
                    !HasValidPcieGpuHostLineage(receipt))
                {
                    return OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
                }

                if (receipt.OperationKind == PcieGpuPowerCableOperationKind.Route)
                {
                    if (foldedState != PcieGpuPowerCableState.Loose ||
                        receipt.SourceContainerId != _handsContainerId ||
                        receipt.TargetContainerId !=
                            _pcieGpuPowerCableDefinition.RouteContainerId ||
                        receipt.Orientation != PowerCableKeyOrientation.Keyed ||
                        receipt.ResultingState != PcieGpuPowerCableState.Routed ||
                        !receipt.SourceRouteOperationId.IsEmpty)
                    {
                        return OperationResult.Fail(
                            AssemblyFailures.PowerCableReceiptHistoryInvalid);
                    }

                    foldedItemId = receipt.ItemId;
                    foldedProductId = receipt.ProductId;
                    foldedRouteOperationId = receipt.OperationId;
                    foldedMotherboardOperationId =
                        receipt.SourceMotherboardSecureOperationId;
                    foldedPowerSupplyOperationId =
                        receipt.SourcePowerSupplyRetentionOperationId;
                    foldedGraphicsCardOperationId =
                        receipt.SourceGraphicsCardRetentionOperationId;
                }
                else if (receipt.OperationKind ==
                         PcieGpuPowerCableOperationKind.Unroute)
                {
                    if (foldedState != PcieGpuPowerCableState.Routed ||
                        receipt.ItemId != foldedItemId ||
                        receipt.ProductId != foldedProductId ||
                        receipt.SourceContainerId !=
                            _pcieGpuPowerCableDefinition.RouteContainerId ||
                        receipt.TargetContainerId != _handsContainerId ||
                        receipt.SourceRouteOperationId != foldedRouteOperationId ||
                        receipt.SourceMotherboardSecureOperationId !=
                            foldedMotherboardOperationId ||
                        receipt.SourcePowerSupplyRetentionOperationId !=
                            foldedPowerSupplyOperationId ||
                        receipt.SourceGraphicsCardRetentionOperationId !=
                            foldedGraphicsCardOperationId ||
                        receipt.ResultingState != PcieGpuPowerCableState.Loose)
                    {
                        return OperationResult.Fail(
                            AssemblyFailures.PowerCableReceiptHistoryInvalid);
                    }

                    foldedItemId = default;
                    foldedProductId = default;
                    foldedRouteOperationId = default;
                    foldedMotherboardOperationId = default;
                    foldedPowerSupplyOperationId = default;
                    foldedGraphicsCardOperationId = default;
                }
                else
                {
                    return OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
                }

                foldedState = receipt.ResultingState;
                previousInventoryRevision = receipt.InventoryRevision;
            }

            if (foldedState == PcieGpuPowerCableState.Routed &&
                (foldedMotherboardOperationId != _securedByOperationId ||
                 foldedPowerSupplyOperationId !=
                     _powerSupplyRetainedByOperationId ||
                 foldedGraphicsCardOperationId != _graphicsCardRetainedByOperationId))
            {
                return OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            return foldedState == _pcieGpuPowerCableState &&
                   foldedItemId == _pcieGpuPowerCableItemId &&
                   foldedProductId == _pcieGpuPowerCableProductId &&
                   foldedRouteOperationId == _pcieGpuPowerCableRoutedByOperationId
                ? OperationResult.Success()
                : OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
        }

        private Failure ValidatePcieGpuPowerCableRoute(
            StableId<ItemInstanceIdScope> itemId,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardRetentionOperationId,
            long expectedCableRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasPcieGpuPowerCableRoute)
            {
                return AssemblyFailures.PowerCableUnsupported;
            }

            if (expectedCableRevision != PcieGpuPowerCableRevision)
            {
                return AssemblyFailures.PlanStale;
            }

            if (PcieGpuPowerCableRevision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (_pcieGpuPowerCableState != PcieGpuPowerCableState.Loose)
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
                    AssemblyOperationKind.SecureMotherboardFastener ||
                motherboardReceipt.ItemId != _motherboardItemId ||
                motherboardReceipt.SlotId != MotherboardSlotId)
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
                    AssemblyOperationKind.RetainPowerSupply ||
                powerSupplyReceipt.ItemId != _powerSupplyItemId ||
                powerSupplyReceipt.SlotId != _powerSupplyBayDefinition.SlotId)
            {
                return AssemblyFailures.PowerCableHostPowerSupplyUnretained;
            }

            if (_graphicsCardSlotState != GraphicsCardSlotState.GraphicsCardRetained ||
                sourceGraphicsCardRetentionOperationId.IsEmpty ||
                sourceGraphicsCardRetentionOperationId !=
                    _graphicsCardRetainedByOperationId ||
                !_receipts.TryGetValue(
                    sourceGraphicsCardRetentionOperationId,
                    out AssemblyOperationReceipt graphicsCardReceipt) ||
                graphicsCardReceipt.OperationKind !=
                    AssemblyOperationKind.RetainGraphicsCard ||
                graphicsCardReceipt.ItemId != _graphicsCardItemId ||
                graphicsCardReceipt.SlotId != _graphicsCardSlotDefinition.SlotId ||
                graphicsCardReceipt.SourceGraphicsCardSeatOperationId !=
                    _graphicsCardSeatedByOperationId ||
                !graphicsCardReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                    _graphicsCardSlotDefinition))
            {
                return AssemblyFailures.PowerCableHostGraphicsCardUnretained;
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item))
            {
                return AssemblyFailures.UnknownItem;
            }

            if (item.ContainerId != _handsContainerId)
            {
                return AssemblyFailures.ItemNotInActorHands;
            }

            return item.ProductId == _pcieGpuPowerCableDefinition.ProductId
                ? Failure.None
                : AssemblyFailures.PowerCableProductMismatch;
        }

        private Failure ValidatePcieGpuPowerCableUnroute(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasPcieGpuPowerCableRoute)
            {
                return AssemblyFailures.PowerCableUnsupported;
            }

            if (expectedCableRevision != PcieGpuPowerCableRevision)
            {
                return AssemblyFailures.PlanStale;
            }

            if (PcieGpuPowerCableRevision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (_pcieGpuPowerCableState != PcieGpuPowerCableState.Routed ||
                itemId != _pcieGpuPowerCableItemId)
            {
                return AssemblyFailures.PowerCableNotRouted;
            }

            if (sourceRouteOperationId.IsEmpty ||
                sourceRouteOperationId != _pcieGpuPowerCableRoutedByOperationId ||
                !_pcieGpuPowerCableReceipts.TryGetValue(
                    sourceRouteOperationId,
                    out PcieGpuPowerCableOperationReceipt routeReceipt) ||
                routeReceipt.OperationKind != PcieGpuPowerCableOperationKind.Route ||
                routeReceipt.ItemId != itemId)
            {
                return AssemblyFailures.PlanStale;
            }

            return IsPcieGpuPowerCableRoutedItem(itemId)
                ? Failure.None
                : AssemblyFailures.PowerCableNotRouted;
        }

        private bool HasValidPcieGpuHostLineage(
            PcieGpuPowerCableOperationReceipt receipt)
        {
            return !receipt.SourceMotherboardSecureOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       receipt.SourceMotherboardSecureOperationId,
                       out AssemblyOperationReceipt motherboardReceipt) &&
                   motherboardReceipt.OperationKind ==
                       AssemblyOperationKind.SecureMotherboardFastener &&
                   motherboardReceipt.SlotId == MotherboardSlotId &&
                   !receipt.SourcePowerSupplyRetentionOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       receipt.SourcePowerSupplyRetentionOperationId,
                       out AssemblyOperationReceipt powerSupplyReceipt) &&
                   powerSupplyReceipt.OperationKind ==
                       AssemblyOperationKind.RetainPowerSupply &&
                   powerSupplyReceipt.SlotId == _powerSupplyBayDefinition.SlotId &&
                   !receipt.SourceGraphicsCardRetentionOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       receipt.SourceGraphicsCardRetentionOperationId,
                       out AssemblyOperationReceipt graphicsCardReceipt) &&
                   graphicsCardReceipt.OperationKind ==
                       AssemblyOperationKind.RetainGraphicsCard &&
                   graphicsCardReceipt.SlotId == _graphicsCardSlotDefinition.SlotId &&
                   !graphicsCardReceipt.SourceGraphicsCardSeatOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       graphicsCardReceipt.SourceGraphicsCardSeatOperationId,
                       out AssemblyOperationReceipt graphicsCardSeatReceipt) &&
                   graphicsCardSeatReceipt.OperationKind ==
                       AssemblyOperationKind.SeatGraphicsCard &&
                   graphicsCardSeatReceipt.ItemId == graphicsCardReceipt.ItemId &&
                   graphicsCardSeatReceipt.ProductId == graphicsCardReceipt.ProductId &&
                   graphicsCardSeatReceipt.SlotId == graphicsCardReceipt.SlotId &&
                   graphicsCardReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                       _graphicsCardSlotDefinition) &&
                   graphicsCardSeatReceipt.GraphicsCardSlotDefinition.HasExactIdentity(
                       _graphicsCardSlotDefinition);
        }

        private bool ValidatePcieGpuPowerCableStateInvariants()
        {
            if (!HasPcieGpuPowerCableRoute)
            {
                return _pcieGpuPowerCableInventoryTransferAccess == null &&
                       _pcieGpuPowerCableState == PcieGpuPowerCableState.Unsupported &&
                       !_pcieGpuPowerCableDefinition.HasAnyValue &&
                       _pcieGpuPowerCableItemId.IsEmpty &&
                       _pcieGpuPowerCableProductId.IsEmpty &&
                       _pcieGpuPowerCableRoutedByOperationId.IsEmpty &&
                       PcieGpuPowerCableRevision == 0 &&
                       _pcieGpuPowerCableReceipts.Count == 0;
            }

            if (_pcieGpuPowerCableInventoryTransferAccess == null ||
                _pcieGpuPowerCableDefinition.RouteContainerId == _handsContainerId ||
                _pcieGpuPowerCableDefinition.RouteContainerId == _workbenchContainerId ||
                _pcieGpuPowerCableDefinition.RouteContainerId ==
                    _processorSocketContainerId ||
                _pcieGpuPowerCableDefinition.RouteContainerId ==
                    _memorySlotDefinition.ContainerId ||
                _pcieGpuPowerCableDefinition.RouteContainerId ==
                    _storageSlotDefinition.ContainerId ||
                _pcieGpuPowerCableDefinition.RouteContainerId ==
                    _processorCoolerSlotDefinition.ContainerId ||
                _pcieGpuPowerCableDefinition.RouteContainerId ==
                    _graphicsCardSlotDefinition.ContainerId ||
                _pcieGpuPowerCableDefinition.RouteContainerId ==
                    _powerSupplyBayDefinition.ContainerId ||
                (HasAtx24PowerCableRoute &&
                 _pcieGpuPowerCableDefinition.RouteContainerId ==
                     _atx24PowerCableDefinition.RouteContainerId) ||
                (HasEps12vPowerCableRoute &&
                 _pcieGpuPowerCableDefinition.RouteContainerId ==
                     _eps12vPowerCableDefinition.RouteContainerId) ||
                !_inventory.TryGetContainer(
                    _pcieGpuPowerCableDefinition.RouteContainerId,
                    out InventoryContainerDefinition routeContainer) ||
                routeContainer.Kind != InventoryContainerKind.Workbench ||
                routeContainer.UnitCapacity != 1 ||
                !_componentCatalog.OwnerCatalog.TryGet(
                    _pcieGpuPowerCableDefinition.ProductId,
                    out ProductDefinition cableProduct) ||
                cableProduct.TrackingPolicy != ProductTrackingPolicy.SerializedInstance ||
                !_componentCatalog.TryGet(
                    _pcieGpuPowerCableDefinition.ProductId,
                    out PcComponentSpecification cableSpecification) ||
                cableSpecification.Kind != PcComponentKind.PowerCable ||
                cableSpecification.PowerCableType !=
                    PowerCableType.ModularPcie8PinPsuToGraphicsCard)
            {
                return false;
            }

            if (_pcieGpuPowerCableState == PcieGpuPowerCableState.Loose)
            {
                return _pcieGpuPowerCableItemId.IsEmpty &&
                       _pcieGpuPowerCableProductId.IsEmpty &&
                       _pcieGpuPowerCableRoutedByOperationId.IsEmpty &&
                       _inventory.GetContainerQuantity(
                           _pcieGpuPowerCableDefinition.RouteContainerId).Value == 0 &&
                       ValidatePcieGpuPowerCableReceiptHistory().IsSuccess;
            }

            return _pcieGpuPowerCableState == PcieGpuPowerCableState.Routed &&
                   _pcieGpuPowerCableProductId ==
                       _pcieGpuPowerCableDefinition.ProductId &&
                   !_pcieGpuPowerCableRoutedByOperationId.IsEmpty &&
                   IsPcieGpuPowerCableRoutedItem(_pcieGpuPowerCableItemId) &&
                   ValidatePcieGpuPowerCableReceiptHistory().IsSuccess;
        }

        private bool IsPcieGpuPowerCableRoutedItem(
            StableId<ItemInstanceIdScope> itemId)
        {
            return !itemId.IsEmpty &&
                   itemId == _pcieGpuPowerCableItemId &&
                   _inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item) &&
                   item.ProductId == _pcieGpuPowerCableDefinition.ProductId &&
                   item.ContainerId == _pcieGpuPowerCableDefinition.RouteContainerId;
        }

        private static Failure MapPcieGpuPowerCableInventoryFailure(
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
