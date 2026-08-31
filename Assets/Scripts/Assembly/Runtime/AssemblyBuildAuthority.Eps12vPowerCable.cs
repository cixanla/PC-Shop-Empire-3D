using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private Eps12vPowerCableDefinition _eps12vPowerCableDefinition;
        private InventorySerializedTransferAccess _eps12vPowerCableInventoryTransferAccess;
        private Eps12vPowerCableState _eps12vPowerCableState =
            Eps12vPowerCableState.Unsupported;
        private StableId<ItemInstanceIdScope> _eps12vPowerCableItemId;
        private StableId<ProductDefinitionIdScope> _eps12vPowerCableProductId;
        private StableId<AssemblyOperationIdScope> _eps12vPowerCableRoutedByOperationId;
        private readonly Dictionary<StableId<AssemblyOperationIdScope>,
            Eps12vPowerCableOperationReceipt> _eps12vPowerCableReceipts =
                new Dictionary<StableId<AssemblyOperationIdScope>,
                    Eps12vPowerCableOperationReceipt>();

        public bool HasEps12vPowerCableRoute => _eps12vPowerCableDefinition.IsValid;

        public Eps12vPowerCableDefinition Eps12vPowerCableDefinition =>
            _eps12vPowerCableDefinition;

        public Eps12vPowerCableTopology Eps12vPowerCableTopology =>
            _eps12vPowerCableDefinition.Topology;

        public StableId<ContainerIdScope> Eps12vPowerCableRouteContainerId =>
            _eps12vPowerCableDefinition.RouteContainerId;

        public Eps12vPowerCableState Eps12vPowerCableState => _eps12vPowerCableState;

        public bool IsEps12vPowerCableRouted =>
            _eps12vPowerCableState == Eps12vPowerCableState.Routed;

        public StableId<ItemInstanceIdScope> Eps12vPowerCableItemId =>
            _eps12vPowerCableItemId;

        public StableId<ProductDefinitionIdScope> Eps12vPowerCableProductId =>
            _eps12vPowerCableProductId;

        public StableId<AssemblyOperationIdScope> Eps12vPowerCableRoutedByOperationId =>
            _eps12vPowerCableRoutedByOperationId;

        public long Eps12vPowerCableRevision { get; private set; }

        public int Eps12vPowerCableReceiptCount => _eps12vPowerCableReceipts.Count;

        public OperationResult<Eps12vPowerCableOperationReceipt> RouteEps12vPowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedCableRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.ContainsKey(operationId) ||
                _atx24PowerCableReceipts.ContainsKey(operationId) ||
                _pcieGpuPowerCableReceipts.ContainsKey(operationId))
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_eps12vPowerCableReceipts.TryGetValue(
                    operationId,
                    out Eps12vPowerCableOperationReceipt replay))
            {
                return replay.MatchesRoute(
                        operationId,
                        BuildId,
                        ChassisId,
                        itemId,
                        _eps12vPowerCableDefinition.ProductId,
                        _handsContainerId,
                        _eps12vPowerCableDefinition.RouteContainerId,
                        _eps12vPowerCableDefinition,
                        orientation,
                        sourceMotherboardSecureOperationId,
                        sourcePowerSupplyRetentionOperationId,
                        sourceProcessorRetentionOperationId,
                        expectedCableRevision)
                    ? OperationResult<Eps12vPowerCableOperationReceipt>.Success(replay)
                    : OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflight = ValidateEps12vPowerCableRoute(
                itemId,
                orientation,
                sourceMotherboardSecureOperationId,
                sourcePowerSupplyRetentionOperationId,
                sourceProcessorRetentionOperationId,
                expectedCableRevision);
            if (!preflight.IsNone)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(preflight);
            }

            _inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _eps12vPowerCableDefinition.RouteContainerId,
                    _eps12vPowerCableInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    MapEps12vPowerCableInventoryFailure(prepared.Error, routing: true));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    MapEps12vPowerCableInventoryFailure(committed.Error, routing: true));
            }

            _eps12vPowerCableState = Eps12vPowerCableState.Routed;
            _eps12vPowerCableItemId = item.Id;
            _eps12vPowerCableProductId = item.ProductId;
            _eps12vPowerCableRoutedByOperationId = operationId;
            Eps12vPowerCableRevision++;

            var receipt = new Eps12vPowerCableOperationReceipt(
                operationId,
                Eps12vPowerCableOperationKind.Route,
                BuildId,
                ChassisId,
                item.Id,
                item.ProductId,
                _handsContainerId,
                _eps12vPowerCableDefinition.RouteContainerId,
                _eps12vPowerCableDefinition,
                orientation,
                Eps12vPowerCableState.Loose,
                Eps12vPowerCableState.Routed,
                sourceMotherboardSecureOperationId,
                sourcePowerSupplyRetentionOperationId,
                sourceProcessorRetentionOperationId,
                default,
                expectedCableRevision,
                Eps12vPowerCableRevision,
                _inventory.Revision);
            _eps12vPowerCableReceipts.Add(operationId, receipt);
            return OperationResult<Eps12vPowerCableOperationReceipt>.Success(receipt);
        }

        public OperationResult<Eps12vPowerCableOperationReceipt> UnrouteEps12vPowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.InvalidOperationId);
            }

            if (_receipts.ContainsKey(operationId) ||
                _atx24PowerCableReceipts.ContainsKey(operationId) ||
                _pcieGpuPowerCableReceipts.ContainsKey(operationId))
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    AssemblyFailures.OperationConflict);
            }

            if (_eps12vPowerCableReceipts.TryGetValue(
                    operationId,
                    out Eps12vPowerCableOperationReceipt replay))
            {
                if (!_eps12vPowerCableReceipts.TryGetValue(
                        sourceRouteOperationId,
                        out Eps12vPowerCableOperationReceipt sourceRouteReceipt) ||
                    sourceRouteReceipt.OperationKind !=
                        Eps12vPowerCableOperationKind.Route)
                {
                    return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
                }

                return replay.MatchesUnroute(
                        operationId,
                        BuildId,
                        ChassisId,
                        itemId,
                        _eps12vPowerCableDefinition.ProductId,
                        _eps12vPowerCableDefinition.RouteContainerId,
                        _handsContainerId,
                        _eps12vPowerCableDefinition,
                        sourceRouteReceipt.SourceMotherboardSecureOperationId,
                        sourceRouteReceipt.SourcePowerSupplyRetentionOperationId,
                        sourceRouteReceipt.SourceProcessorRetentionOperationId,
                        sourceRouteOperationId,
                        expectedCableRevision)
                    ? OperationResult<Eps12vPowerCableOperationReceipt>.Success(replay)
                    : OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                        AssemblyFailures.OperationConflict);
            }

            Failure preflight = ValidateEps12vPowerCableUnroute(
                itemId,
                sourceRouteOperationId,
                expectedCableRevision);
            if (!preflight.IsNone)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(preflight);
            }

            InventoryItemRecord item = GetItem(itemId);
            OperationResult<InventorySerializedTransferPlan> prepared =
                _inventory.PrepareSerializedItemTransfer(
                    itemId,
                    _handsContainerId,
                    _eps12vPowerCableInventoryTransferAccess);
            if (prepared.IsFailure)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    MapEps12vPowerCableInventoryFailure(prepared.Error, routing: false));
            }

            OperationResult committed =
                _inventory.CommitPreparedSerializedItemTransfer(prepared.Value);
            if (committed.IsFailure)
            {
                return OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                    MapEps12vPowerCableInventoryFailure(committed.Error, routing: false));
            }

            Eps12vPowerCableOperationReceipt sourceRoute =
                _eps12vPowerCableReceipts[sourceRouteOperationId];
            _eps12vPowerCableState = Eps12vPowerCableState.Loose;
            _eps12vPowerCableItemId = default;
            _eps12vPowerCableProductId = default;
            _eps12vPowerCableRoutedByOperationId = default;
            Eps12vPowerCableRevision++;

            var receipt = new Eps12vPowerCableOperationReceipt(
                operationId,
                Eps12vPowerCableOperationKind.Unroute,
                BuildId,
                ChassisId,
                item.Id,
                item.ProductId,
                _eps12vPowerCableDefinition.RouteContainerId,
                _handsContainerId,
                _eps12vPowerCableDefinition,
                PowerCableKeyOrientation.Keyed,
                Eps12vPowerCableState.Routed,
                Eps12vPowerCableState.Loose,
                sourceRoute.SourceMotherboardSecureOperationId,
                sourceRoute.SourcePowerSupplyRetentionOperationId,
                sourceRoute.SourceProcessorRetentionOperationId,
                sourceRouteOperationId,
                expectedCableRevision,
                Eps12vPowerCableRevision,
                _inventory.Revision);
            _eps12vPowerCableReceipts.Add(operationId, receipt);
            return OperationResult<Eps12vPowerCableOperationReceipt>.Success(receipt);
        }

        public bool TryGetEps12vPowerCableReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            out Eps12vPowerCableOperationReceipt receipt)
        {
            return _eps12vPowerCableReceipts.TryGetValue(operationId, out receipt);
        }

        public IReadOnlyList<Eps12vPowerCableOperationReceipt>
            GetEps12vPowerCableReceipts()
        {
            var receipts = new List<Eps12vPowerCableOperationReceipt>(
                _eps12vPowerCableReceipts.Values);
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

        public OperationResult ValidateEps12vPowerCableReceiptHistory()
        {
            if (!HasEps12vPowerCableRoute)
            {
                return Eps12vPowerCableRevision == 0 &&
                       _eps12vPowerCableReceipts.Count == 0
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            if (Eps12vPowerCableRevision != _eps12vPowerCableReceipts.Count)
            {
                return OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            IReadOnlyList<Eps12vPowerCableOperationReceipt> receipts =
                GetEps12vPowerCableReceipts();
            Eps12vPowerCableState foldedState = Eps12vPowerCableState.Loose;
            StableId<ItemInstanceIdScope> foldedItemId = default;
            StableId<ProductDefinitionIdScope> foldedProductId = default;
            StableId<AssemblyOperationIdScope> foldedRouteOperationId = default;
            StableId<AssemblyOperationIdScope> foldedMotherboardOperationId = default;
            StableId<AssemblyOperationIdScope> foldedPowerSupplyOperationId = default;
            StableId<AssemblyOperationIdScope> foldedProcessorOperationId = default;
            long previousInventoryRevision = -1;

            for (int index = 0; index < receipts.Count; index++)
            {
                Eps12vPowerCableOperationReceipt receipt = receipts[index];
                if (receipt == null ||
                    receipt.CableRevision != index + 1L ||
                    receipt.ExpectedCableRevision != index ||
                    receipt.BuildId != BuildId ||
                    receipt.ChassisId != ChassisId ||
                    receipt.ItemId.IsEmpty ||
                    receipt.ProductId != _eps12vPowerCableDefinition.ProductId ||
                    !receipt.Definition.HasExactIdentity(_eps12vPowerCableDefinition) ||
                    receipt.RouteFingerprint !=
                        _eps12vPowerCableDefinition.Topology.Fingerprint ||
                    receipt.InventoryRevision <= previousInventoryRevision ||
                    receipt.PreviousState != foldedState ||
                    !HasValidEps12vHostLineage(receipt))
                {
                    return OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
                }

                if (receipt.OperationKind == Eps12vPowerCableOperationKind.Route)
                {
                    if (foldedState != Eps12vPowerCableState.Loose ||
                        receipt.SourceContainerId != _handsContainerId ||
                        receipt.TargetContainerId !=
                            _eps12vPowerCableDefinition.RouteContainerId ||
                        receipt.Orientation != PowerCableKeyOrientation.Keyed ||
                        receipt.ResultingState != Eps12vPowerCableState.Routed ||
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
                    foldedProcessorOperationId =
                        receipt.SourceProcessorRetentionOperationId;
                }
                else if (receipt.OperationKind ==
                         Eps12vPowerCableOperationKind.Unroute)
                {
                    if (foldedState != Eps12vPowerCableState.Routed ||
                        receipt.ItemId != foldedItemId ||
                        receipt.ProductId != foldedProductId ||
                        receipt.SourceContainerId !=
                            _eps12vPowerCableDefinition.RouteContainerId ||
                        receipt.TargetContainerId != _handsContainerId ||
                        receipt.SourceRouteOperationId != foldedRouteOperationId ||
                        receipt.SourceMotherboardSecureOperationId !=
                            foldedMotherboardOperationId ||
                        receipt.SourcePowerSupplyRetentionOperationId !=
                            foldedPowerSupplyOperationId ||
                        receipt.SourceProcessorRetentionOperationId !=
                            foldedProcessorOperationId ||
                        receipt.ResultingState != Eps12vPowerCableState.Loose)
                    {
                        return OperationResult.Fail(
                            AssemblyFailures.PowerCableReceiptHistoryInvalid);
                    }

                    foldedItemId = default;
                    foldedProductId = default;
                    foldedRouteOperationId = default;
                    foldedMotherboardOperationId = default;
                    foldedPowerSupplyOperationId = default;
                    foldedProcessorOperationId = default;
                }
                else
                {
                    return OperationResult.Fail(
                        AssemblyFailures.PowerCableReceiptHistoryInvalid);
                }

                foldedState = receipt.ResultingState;
                previousInventoryRevision = receipt.InventoryRevision;
            }

            if (foldedState == Eps12vPowerCableState.Routed &&
                (foldedMotherboardOperationId != _securedByOperationId ||
                 foldedPowerSupplyOperationId !=
                     _powerSupplyRetainedByOperationId ||
                 foldedProcessorOperationId != _processorRetainedByOperationId))
            {
                return OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
            }

            return foldedState == _eps12vPowerCableState &&
                   foldedItemId == _eps12vPowerCableItemId &&
                   foldedProductId == _eps12vPowerCableProductId &&
                   foldedRouteOperationId == _eps12vPowerCableRoutedByOperationId
                ? OperationResult.Success()
                : OperationResult.Fail(
                    AssemblyFailures.PowerCableReceiptHistoryInvalid);
        }

        private Failure ValidateEps12vPowerCableRoute(
            StableId<ItemInstanceIdScope> itemId,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedCableRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasEps12vPowerCableRoute)
            {
                return AssemblyFailures.PowerCableUnsupported;
            }

            if (expectedCableRevision != Eps12vPowerCableRevision)
            {
                return AssemblyFailures.PlanStale;
            }

            if (Eps12vPowerCableRevision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (_eps12vPowerCableState != Eps12vPowerCableState.Loose)
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

            if (_processorSocketState != ProcessorSocketState.ProcessorRetained ||
                sourceProcessorRetentionOperationId.IsEmpty ||
                sourceProcessorRetentionOperationId !=
                    _processorRetainedByOperationId ||
                !_receipts.TryGetValue(
                    sourceProcessorRetentionOperationId,
                    out AssemblyOperationReceipt processorReceipt) ||
                processorReceipt.OperationKind !=
                    AssemblyOperationKind.CloseProcessorRetention ||
                processorReceipt.ItemId != _processorItemId ||
                processorReceipt.SlotId != _processorSlotId ||
                processorReceipt.RetentionId != _processorRetentionId ||
                processorReceipt.SourceProcessorSeatOperationId !=
                    _processorSeatedByOperationId)
            {
                return AssemblyFailures.PowerCableHostProcessorUnretained;
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item))
            {
                return AssemblyFailures.UnknownItem;
            }

            if (item.ContainerId != _handsContainerId)
            {
                return AssemblyFailures.ItemNotInActorHands;
            }

            return item.ProductId == _eps12vPowerCableDefinition.ProductId
                ? Failure.None
                : AssemblyFailures.PowerCableProductMismatch;
        }

        private Failure ValidateEps12vPowerCableUnroute(
            StableId<ItemInstanceIdScope> itemId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            Failure maintenanceFailure = ValidateElectricalMaintenanceInterlock();
            if (!maintenanceFailure.IsNone)
            {
                return maintenanceFailure;
            }

            if (!HasEps12vPowerCableRoute)
            {
                return AssemblyFailures.PowerCableUnsupported;
            }

            if (expectedCableRevision != Eps12vPowerCableRevision)
            {
                return AssemblyFailures.PlanStale;
            }

            if (Eps12vPowerCableRevision == long.MaxValue)
            {
                return AssemblyFailures.RevisionOverflow;
            }

            if (_eps12vPowerCableState != Eps12vPowerCableState.Routed ||
                itemId != _eps12vPowerCableItemId)
            {
                return AssemblyFailures.PowerCableNotRouted;
            }

            if (sourceRouteOperationId.IsEmpty ||
                sourceRouteOperationId != _eps12vPowerCableRoutedByOperationId ||
                !_eps12vPowerCableReceipts.TryGetValue(
                    sourceRouteOperationId,
                    out Eps12vPowerCableOperationReceipt routeReceipt) ||
                routeReceipt.OperationKind != Eps12vPowerCableOperationKind.Route ||
                routeReceipt.ItemId != itemId)
            {
                return AssemblyFailures.PlanStale;
            }

            return IsEps12vPowerCableRoutedItem(itemId)
                ? Failure.None
                : AssemblyFailures.PowerCableNotRouted;
        }

        private bool HasValidEps12vHostLineage(
            Eps12vPowerCableOperationReceipt receipt)
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
                   !receipt.SourceProcessorRetentionOperationId.IsEmpty &&
                   _receipts.TryGetValue(
                       receipt.SourceProcessorRetentionOperationId,
                       out AssemblyOperationReceipt processorReceipt) &&
                   processorReceipt.OperationKind ==
                       AssemblyOperationKind.CloseProcessorRetention &&
                   processorReceipt.SlotId == _processorSlotId &&
                   processorReceipt.RetentionId == _processorRetentionId;
        }

        private bool ValidateEps12vPowerCableStateInvariants()
        {
            if (!HasEps12vPowerCableRoute)
            {
                return _eps12vPowerCableInventoryTransferAccess == null &&
                       _eps12vPowerCableState == Eps12vPowerCableState.Unsupported &&
                       !_eps12vPowerCableDefinition.HasAnyValue &&
                       _eps12vPowerCableItemId.IsEmpty &&
                       _eps12vPowerCableProductId.IsEmpty &&
                       _eps12vPowerCableRoutedByOperationId.IsEmpty &&
                       Eps12vPowerCableRevision == 0 &&
                       _eps12vPowerCableReceipts.Count == 0;
            }

            if (_eps12vPowerCableInventoryTransferAccess == null ||
                _eps12vPowerCableDefinition.RouteContainerId == _handsContainerId ||
                _eps12vPowerCableDefinition.RouteContainerId == _workbenchContainerId ||
                _eps12vPowerCableDefinition.RouteContainerId ==
                    _processorSocketContainerId ||
                _eps12vPowerCableDefinition.RouteContainerId ==
                    _memorySlotDefinition.ContainerId ||
                _eps12vPowerCableDefinition.RouteContainerId ==
                    _storageSlotDefinition.ContainerId ||
                _eps12vPowerCableDefinition.RouteContainerId ==
                    _processorCoolerSlotDefinition.ContainerId ||
                _eps12vPowerCableDefinition.RouteContainerId ==
                    _graphicsCardSlotDefinition.ContainerId ||
                _eps12vPowerCableDefinition.RouteContainerId ==
                    _powerSupplyBayDefinition.ContainerId ||
                (HasAtx24PowerCableRoute &&
                 _eps12vPowerCableDefinition.RouteContainerId ==
                     _atx24PowerCableDefinition.RouteContainerId) ||
                !_inventory.TryGetContainer(
                    _eps12vPowerCableDefinition.RouteContainerId,
                    out InventoryContainerDefinition routeContainer) ||
                routeContainer.Kind != InventoryContainerKind.Workbench ||
                routeContainer.UnitCapacity != 1 ||
                !_componentCatalog.OwnerCatalog.TryGet(
                    _eps12vPowerCableDefinition.ProductId,
                    out ProductDefinition cableProduct) ||
                cableProduct.TrackingPolicy != ProductTrackingPolicy.SerializedInstance ||
                !_componentCatalog.TryGet(
                    _eps12vPowerCableDefinition.ProductId,
                    out PcComponentSpecification cableSpecification) ||
                cableSpecification.Kind != PcComponentKind.PowerCable ||
                cableSpecification.PowerCableType !=
                    PowerCableType.ModularEps12v8PinPsuToMotherboard)
            {
                return false;
            }

            if (_eps12vPowerCableState == Eps12vPowerCableState.Loose)
            {
                return _eps12vPowerCableItemId.IsEmpty &&
                       _eps12vPowerCableProductId.IsEmpty &&
                       _eps12vPowerCableRoutedByOperationId.IsEmpty &&
                       _inventory.GetContainerQuantity(
                           _eps12vPowerCableDefinition.RouteContainerId).Value == 0 &&
                       ValidateEps12vPowerCableReceiptHistory().IsSuccess;
            }

            return _eps12vPowerCableState == Eps12vPowerCableState.Routed &&
                   _eps12vPowerCableProductId ==
                       _eps12vPowerCableDefinition.ProductId &&
                   !_eps12vPowerCableRoutedByOperationId.IsEmpty &&
                   IsEps12vPowerCableRoutedItem(_eps12vPowerCableItemId) &&
                   ValidateEps12vPowerCableReceiptHistory().IsSuccess;
        }

        private bool IsEps12vPowerCableRoutedItem(
            StableId<ItemInstanceIdScope> itemId)
        {
            return !itemId.IsEmpty &&
                   itemId == _eps12vPowerCableItemId &&
                   _inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item) &&
                   item.ProductId == _eps12vPowerCableDefinition.ProductId &&
                   item.ContainerId == _eps12vPowerCableDefinition.RouteContainerId;
        }

        private static Failure MapEps12vPowerCableInventoryFailure(
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
