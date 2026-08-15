using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Inventory
{
    /// <summary>
    /// The single authoritative owner of logical stock. Unity world objects are projections and never mutate
    /// quantities directly. Every failed command leaves state and Revision unchanged.
    /// </summary>
    public sealed class InventoryAuthority
    {
        private readonly ProductCatalog _catalog;
        private readonly Dictionary<StableId<ContainerIdScope>, InventoryContainerDefinition> _containers =
            new Dictionary<StableId<ContainerIdScope>, InventoryContainerDefinition>();
        private readonly Dictionary<StableId<ItemInstanceIdScope>, InventoryItemRecord> _items =
            new Dictionary<StableId<ItemInstanceIdScope>, InventoryItemRecord>();
        private readonly Dictionary<StableId<BatchIdScope>, InventoryBatchRecord> _batches =
            new Dictionary<StableId<BatchIdScope>, InventoryBatchRecord>();
        private readonly Dictionary<BatchPositionKey, int> _batchQuantities =
            new Dictionary<BatchPositionKey, int>();
        private readonly Dictionary<StableId<ReservationIdScope>, InventoryReservation> _reservations =
            new Dictionary<StableId<ReservationIdScope>, InventoryReservation>();

        private InventoryAuthority(ProductCatalog catalog)
        {
            _catalog = catalog;
        }

        public long Revision { get; private set; }

        public int ContainerCount => _containers.Count;

        public int SerializedItemCount => _items.Count;

        public int BatchCount => _batches.Count;

        public int ReservationCount => _reservations.Count;

        public static OperationResult<InventoryAuthority> Create(ProductCatalog catalog)
        {
            return catalog == null
                ? OperationResult<InventoryAuthority>.Fail(InventoryFailures.MissingCatalog)
                : OperationResult<InventoryAuthority>.Success(new InventoryAuthority(catalog));
        }

        public OperationResult RegisterContainer(InventoryContainerDefinition definition)
        {
            if (definition == null || definition.Id.IsEmpty)
            {
                return OperationResult.Fail(InventoryFailures.InvalidContainerId);
            }

            if (!InventoryValidation.IsValidContainerKind(definition.Kind))
            {
                return OperationResult.Fail(InventoryFailures.InvalidContainerKind);
            }

            if (definition.UnitCapacity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidContainerCapacity);
            }

            if (_containers.ContainsKey(definition.Id))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateContainer);
            }

            _containers.Add(definition.Id, definition);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReceiveSerializedItem(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> containerId,
            InventoryCondition condition)
        {
            if (itemId.IsEmpty)
            {
                return OperationResult.Fail(InventoryFailures.InvalidItemId);
            }

            Failure productFailure = ValidateProduct(productId, ProductTrackingPolicy.SerializedInstance);
            if (!productFailure.IsNone)
            {
                return OperationResult.Fail(productFailure);
            }

            if (!InventoryValidation.IsValidCondition(condition))
            {
                return OperationResult.Fail(InventoryFailures.InvalidCondition);
            }

            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (_items.ContainsKey(itemId))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateItem);
            }

            Failure capacityFailure = ValidateCapacity(containerId, 1);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            _items.Add(itemId, new InventoryItemRecord(itemId, productId, containerId, condition));
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReceiveBatch(
            StableId<BatchIdScope> batchId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> containerId,
            InventoryCondition condition,
            int quantity)
        {
            if (batchId.IsEmpty)
            {
                return OperationResult.Fail(InventoryFailures.InvalidBatchId);
            }

            if (quantity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidQuantity);
            }

            Failure productFailure = ValidateProduct(productId, ProductTrackingPolicy.BatchQuantity);
            if (!productFailure.IsNone)
            {
                return OperationResult.Fail(productFailure);
            }

            if (!InventoryValidation.IsValidCondition(condition))
            {
                return OperationResult.Fail(InventoryFailures.InvalidCondition);
            }

            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (_batches.ContainsKey(batchId))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateBatch);
            }

            Failure capacityFailure = ValidateCapacity(containerId, quantity);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            var position = new BatchPositionKey(batchId, containerId);
            _batches.Add(batchId, new InventoryBatchRecord(batchId, productId, condition));
            _batchQuantities.Add(position, quantity);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult TransferSerializedItem(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> targetContainerId)
        {
            if (!_items.TryGetValue(itemId, out InventoryItemRecord item))
            {
                return OperationResult.Fail(InventoryFailures.UnknownItem);
            }

            if (!_containers.ContainsKey(targetContainerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (item.ContainerId == targetContainerId)
            {
                return OperationResult.Fail(InventoryFailures.SameContainer);
            }

            Failure capacityFailure = ValidateCapacity(targetContainerId, 1);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            _items[itemId] = new InventoryItemRecord(item.Id, item.ProductId, targetContainerId, item.Condition);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult TransferBatch(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            int quantity)
        {
            if (quantity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidQuantity);
            }

            if (!_batches.ContainsKey(batchId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatch);
            }

            if (!_containers.ContainsKey(sourceContainerId) || !_containers.ContainsKey(targetContainerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (sourceContainerId == targetContainerId)
            {
                return OperationResult.Fail(InventoryFailures.SameContainer);
            }

            var source = new BatchPositionKey(batchId, sourceContainerId);
            if (!_batchQuantities.TryGetValue(source, out int sourceQuantity))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatchPosition);
            }

            if (sourceQuantity < quantity)
            {
                return OperationResult.Fail(InventoryFailures.InsufficientAvailable);
            }

            int reservedAtSource = GetReservedBatchQuantityUnsafe(batchId, sourceContainerId);
            if ((long)sourceQuantity - reservedAtSource < quantity)
            {
                return OperationResult.Fail(InventoryFailures.ReservedQuantity);
            }

            Failure capacityFailure = ValidateCapacity(targetContainerId, quantity);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            var target = new BatchPositionKey(batchId, targetContainerId);
            _batchQuantities.TryGetValue(target, out int targetQuantity);
            if ((long)targetQuantity + quantity > int.MaxValue)
            {
                return OperationResult.Fail(InventoryFailures.QuantityOverflow);
            }

            int remaining = sourceQuantity - quantity;
            if (remaining == 0)
            {
                _batchQuantities.Remove(source);
            }
            else
            {
                _batchQuantities[source] = remaining;
            }

            _batchQuantities[target] = targetQuantity + quantity;
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReserveSerializedItem(
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId,
            StableId<ItemInstanceIdScope> itemId)
        {
            Failure identityFailure = ValidateReservationIdentity(reservationId, claimId);
            if (!identityFailure.IsNone)
            {
                return OperationResult.Fail(identityFailure);
            }

            if (_reservations.ContainsKey(reservationId))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateReservation);
            }

            if (!_items.ContainsKey(itemId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownItem);
            }

            foreach (InventoryReservation existing in _reservations.Values)
            {
                if (existing.TargetKind == InventoryReservationTargetKind.SerializedItem &&
                    existing.ItemId == itemId)
                {
                    return OperationResult.Fail(InventoryFailures.ItemAlreadyReserved);
                }
            }

            _reservations.Add(
                reservationId,
                new InventoryReservation(
                    reservationId,
                    claimId,
                    InventoryReservationTargetKind.SerializedItem,
                    itemId,
                    default,
                    default,
                    1));
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReserveBatch(
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId,
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId,
            int quantity)
        {
            Failure identityFailure = ValidateReservationIdentity(reservationId, claimId);
            if (!identityFailure.IsNone)
            {
                return OperationResult.Fail(identityFailure);
            }

            if (quantity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidQuantity);
            }

            if (_reservations.ContainsKey(reservationId))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateReservation);
            }

            if (!_batches.ContainsKey(batchId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatch);
            }

            var position = new BatchPositionKey(batchId, containerId);
            if (!_batchQuantities.TryGetValue(position, out int storedQuantity))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatchPosition);
            }

            int reservedQuantity = GetReservedBatchQuantityUnsafe(batchId, containerId);
            if ((long)storedQuantity - reservedQuantity < quantity)
            {
                return OperationResult.Fail(InventoryFailures.InsufficientAvailable);
            }

            _reservations.Add(
                reservationId,
                new InventoryReservation(
                    reservationId,
                    claimId,
                    InventoryReservationTargetKind.BatchPosition,
                    default,
                    batchId,
                    containerId,
                    quantity));
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReleaseReservation(StableId<ReservationIdScope> reservationId)
        {
            if (!_reservations.ContainsKey(reservationId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownReservation);
            }

            _reservations.Remove(reservationId);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ConsumeReservation(StableId<ReservationIdScope> reservationId)
        {
            if (!_reservations.TryGetValue(reservationId, out InventoryReservation reservation))
            {
                return OperationResult.Fail(InventoryFailures.UnknownReservation);
            }

            if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem)
            {
                if (!_items.ContainsKey(reservation.ItemId))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                _items.Remove(reservation.ItemId);
                _reservations.Remove(reservationId);
                AdvanceRevision();
                return OperationResult.Success();
            }

            if (reservation.TargetKind != InventoryReservationTargetKind.BatchPosition)
            {
                return OperationResult.Fail(InventoryFailures.InvariantViolation);
            }

            var position = new BatchPositionKey(reservation.BatchId, reservation.ContainerId);
            if (!_batchQuantities.TryGetValue(position, out int storedQuantity) ||
                storedQuantity < reservation.Quantity)
            {
                return OperationResult.Fail(InventoryFailures.InvariantViolation);
            }

            int remaining = storedQuantity - reservation.Quantity;
            if (remaining == 0)
            {
                _batchQuantities.Remove(position);
            }
            else
            {
                _batchQuantities[position] = remaining;
            }

            _reservations.Remove(reservationId);
            if (!HasBatchPositionsUnsafe(reservation.BatchId))
            {
                _batches.Remove(reservation.BatchId);
            }

            AdvanceRevision();
            return OperationResult.Success();
        }

        public bool TryGetSerializedItem(
            StableId<ItemInstanceIdScope> itemId,
            out InventoryItemRecord item)
        {
            return _items.TryGetValue(itemId, out item);
        }

        public bool TryGetReservation(
            StableId<ReservationIdScope> reservationId,
            out InventoryReservation reservation)
        {
            return _reservations.TryGetValue(reservationId, out reservation);
        }

        public OperationResult<int> GetBatchQuantity(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId)
        {
            if (!_batches.ContainsKey(batchId))
            {
                return OperationResult<int>.Fail(InventoryFailures.UnknownBatch);
            }

            var key = new BatchPositionKey(batchId, containerId);
            return _batchQuantities.TryGetValue(key, out int quantity)
                ? OperationResult<int>.Success(quantity)
                : OperationResult<int>.Fail(InventoryFailures.UnknownBatchPosition);
        }

        public OperationResult<long> GetTotalQuantity(StableId<ProductDefinitionIdScope> productId)
        {
            Failure productFailure = ValidateKnownProduct(productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<long>.Fail(productFailure);
            }

            long total = 0;
            foreach (InventoryItemRecord item in _items.Values)
            {
                if (item.ProductId == productId)
                {
                    total++;
                }
            }

            foreach (KeyValuePair<BatchPositionKey, int> position in _batchQuantities)
            {
                if (_batches[position.Key.BatchId].ProductId == productId)
                {
                    total += position.Value;
                }
            }

            return OperationResult<long>.Success(total);
        }

        public OperationResult<long> GetAvailableQuantity(StableId<ProductDefinitionIdScope> productId)
        {
            OperationResult<long> totalResult = GetTotalQuantity(productId);
            if (totalResult.IsFailure)
            {
                return totalResult;
            }

            long reserved = 0;
            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem)
                {
                    if (_items.TryGetValue(reservation.ItemId, out InventoryItemRecord item) &&
                        item.ProductId == productId)
                    {
                        reserved++;
                    }
                }
                else if (_batches.TryGetValue(reservation.BatchId, out InventoryBatchRecord batch) &&
                         batch.ProductId == productId)
                {
                    reserved += reservation.Quantity;
                }
            }

            return OperationResult<long>.Success(totalResult.Value - reserved);
        }

        public OperationResult<long> GetContainerQuantity(StableId<ContainerIdScope> containerId)
        {
            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult<long>.Fail(InventoryFailures.UnknownContainer);
            }

            return OperationResult<long>.Success(GetContainerLoadUnsafe(containerId));
        }

        public IReadOnlyList<InventoryContainerDefinition> GetContainers()
        {
            var values = new List<InventoryContainerDefinition>(_containers.Values);
            values.Sort((left, right) => string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public IReadOnlyList<InventoryItemRecord> GetSerializedItems()
        {
            var values = new List<InventoryItemRecord>(_items.Values);
            values.Sort((left, right) => string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public IReadOnlyList<InventoryBatchPosition> GetBatchPositions()
        {
            var values = new List<InventoryBatchPosition>();
            foreach (KeyValuePair<BatchPositionKey, int> position in _batchQuantities)
            {
                values.Add(new InventoryBatchPosition(
                    position.Key.BatchId,
                    position.Key.ContainerId,
                    position.Value));
            }

            values.Sort(CompareBatchPositions);
            return Array.AsReadOnly(values.ToArray());
        }

        public IReadOnlyList<InventoryReservation> GetReservations()
        {
            var values = new List<InventoryReservation>(_reservations.Values);
            values.Sort((left, right) => string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            var containerLoads = new Dictionary<StableId<ContainerIdScope>, long>();
            foreach (KeyValuePair<StableId<ContainerIdScope>, InventoryContainerDefinition> entry in _containers)
            {
                InventoryContainerDefinition definition = entry.Value;
                if (definition == null ||
                    entry.Key != definition.Id ||
                    definition.Id.IsEmpty ||
                    !InventoryValidation.IsValidContainerKind(definition.Kind) ||
                    definition.UnitCapacity <= 0)
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                containerLoads.Add(entry.Key, 0);
            }

            var reservedItems = new HashSet<StableId<ItemInstanceIdScope>>();
            var reservedBatches = new Dictionary<BatchPositionKey, long>();

            foreach (KeyValuePair<StableId<ItemInstanceIdScope>, InventoryItemRecord> entry in _items)
            {
                InventoryItemRecord item = entry.Value;
                if (item == null ||
                    entry.Key != item.Id ||
                    item.Id.IsEmpty ||
                    !_containers.ContainsKey(item.ContainerId) ||
                    !InventoryValidation.IsValidCondition(item.Condition) ||
                    !ProductHasTrackingPolicy(item.ProductId, ProductTrackingPolicy.SerializedInstance))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                containerLoads[item.ContainerId]++;
            }

            foreach (KeyValuePair<StableId<BatchIdScope>, InventoryBatchRecord> entry in _batches)
            {
                InventoryBatchRecord batch = entry.Value;
                if (batch == null ||
                    entry.Key != batch.Id ||
                    batch.Id.IsEmpty ||
                    !InventoryValidation.IsValidCondition(batch.Condition) ||
                    !ProductHasTrackingPolicy(batch.ProductId, ProductTrackingPolicy.BatchQuantity) ||
                    !HasBatchPositionsUnsafe(batch.Id))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<BatchPositionKey, int> entry in _batchQuantities)
            {
                if (entry.Key.BatchId.IsEmpty ||
                    entry.Key.ContainerId.IsEmpty ||
                    entry.Value <= 0 ||
                    !_batches.ContainsKey(entry.Key.BatchId) ||
                    !containerLoads.ContainsKey(entry.Key.ContainerId))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                containerLoads[entry.Key.ContainerId] += entry.Value;
            }

            foreach (KeyValuePair<StableId<ReservationIdScope>, InventoryReservation> entry in _reservations)
            {
                InventoryReservation reservation = entry.Value;
                if (reservation == null ||
                    entry.Key != reservation.Id ||
                    reservation.Id.IsEmpty ||
                    reservation.ClaimId.IsEmpty)
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem)
                {
                    if (reservation.Quantity != 1 ||
                        !_items.ContainsKey(reservation.ItemId) ||
                        !reservedItems.Add(reservation.ItemId))
                    {
                        return OperationResult.Fail(InventoryFailures.InvariantViolation);
                    }
                }
                else if (reservation.TargetKind == InventoryReservationTargetKind.BatchPosition)
                {
                    var key = new BatchPositionKey(reservation.BatchId, reservation.ContainerId);
                    if (reservation.Quantity <= 0 || !_batchQuantities.ContainsKey(key))
                    {
                        return OperationResult.Fail(InventoryFailures.InvariantViolation);
                    }

                    reservedBatches.TryGetValue(key, out long quantity);
                    reservedBatches[key] = quantity + reservation.Quantity;
                }
                else
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<BatchPositionKey, long> reserved in reservedBatches)
            {
                if (reserved.Value > _batchQuantities[reserved.Key])
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<StableId<ContainerIdScope>, long> load in containerLoads)
            {
                if (load.Value < 0 || load.Value > _containers[load.Key].UnitCapacity)
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private Failure ValidateProduct(
            StableId<ProductDefinitionIdScope> productId,
            ProductTrackingPolicy expectedTrackingPolicy)
        {
            Failure knownFailure = ValidateKnownProduct(productId);
            if (!knownFailure.IsNone)
            {
                return knownFailure;
            }

            _catalog.TryGet(productId, out ProductDefinition definition);
            return definition.TrackingPolicy == expectedTrackingPolicy
                ? Failure.None
                : InventoryFailures.TrackingMismatch;
        }

        private Failure ValidateKnownProduct(StableId<ProductDefinitionIdScope> productId)
        {
            if (productId.IsEmpty)
            {
                return InventoryFailures.InvalidProductId;
            }

            return _catalog.TryGet(productId, out _)
                ? Failure.None
                : InventoryFailures.UnknownProduct;
        }

        private bool ProductHasTrackingPolicy(
            StableId<ProductDefinitionIdScope> productId,
            ProductTrackingPolicy policy)
        {
            return _catalog.TryGet(productId, out ProductDefinition definition) &&
                   definition.TrackingPolicy == policy;
        }

        private Failure ValidateCapacity(StableId<ContainerIdScope> containerId, int addedQuantity)
        {
            if (!_containers.TryGetValue(containerId, out InventoryContainerDefinition container))
            {
                return InventoryFailures.UnknownContainer;
            }

            long load = GetContainerLoadUnsafe(containerId);
            return load > container.UnitCapacity - (long)addedQuantity
                ? InventoryFailures.ContainerCapacityExceeded
                : Failure.None;
        }

        private Failure ValidateReservationIdentity(
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId)
        {
            if (reservationId.IsEmpty)
            {
                return InventoryFailures.InvalidReservationId;
            }

            return claimId.IsEmpty
                ? InventoryFailures.InvalidClaimId
                : Failure.None;
        }

        private long GetContainerLoadUnsafe(StableId<ContainerIdScope> containerId)
        {
            long total = 0;
            foreach (InventoryItemRecord item in _items.Values)
            {
                if (item.ContainerId == containerId)
                {
                    total++;
                }
            }

            foreach (KeyValuePair<BatchPositionKey, int> position in _batchQuantities)
            {
                if (position.Key.ContainerId == containerId)
                {
                    total += position.Value;
                }
            }

            return total;
        }

        private int GetReservedBatchQuantityUnsafe(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId)
        {
            long total = 0;
            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.TargetKind == InventoryReservationTargetKind.BatchPosition &&
                    reservation.BatchId == batchId &&
                    reservation.ContainerId == containerId)
                {
                    total += reservation.Quantity;
                }
            }

            return total > int.MaxValue ? int.MaxValue : (int)total;
        }

        private bool HasBatchPositionsUnsafe(StableId<BatchIdScope> batchId)
        {
            foreach (BatchPositionKey position in _batchQuantities.Keys)
            {
                if (position.BatchId == batchId)
                {
                    return true;
                }
            }

            return false;
        }

        private void AdvanceRevision()
        {
            if (Revision < long.MaxValue)
            {
                Revision++;
            }
        }

        private static int CompareBatchPositions(InventoryBatchPosition left, InventoryBatchPosition right)
        {
            int batchComparison = string.Compare(left.BatchId.Value, right.BatchId.Value, StringComparison.Ordinal);
            return batchComparison != 0
                ? batchComparison
                : string.Compare(left.ContainerId.Value, right.ContainerId.Value, StringComparison.Ordinal);
        }
    }
}
