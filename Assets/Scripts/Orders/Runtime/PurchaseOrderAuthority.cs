using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Orders
{
    /// <summary>
    /// Owns purchase-order lifecycle state. A delivery is not stock until arrival has been registered and the
    /// Inventory authority accepts the complete manifest into a receiving container.
    /// </summary>
    public sealed class PurchaseOrderAuthority
    {
        private readonly ProductCatalog _catalog;
        private readonly Dictionary<StableId<PurchaseOrderIdScope>, PurchaseOrderRecord> _orders =
            new Dictionary<StableId<PurchaseOrderIdScope>, PurchaseOrderRecord>();
        private readonly Dictionary<StableId<DeliveryIdScope>, StableId<PurchaseOrderIdScope>> _deliveryOwners =
            new Dictionary<StableId<DeliveryIdScope>, StableId<PurchaseOrderIdScope>>();

        private PurchaseOrderAuthority(ProductCatalog catalog)
        {
            _catalog = catalog;
        }

        public long Revision { get; private set; }

        public int Count => _orders.Count;

        public static OperationResult<PurchaseOrderAuthority> Create(ProductCatalog catalog)
        {
            return catalog == null
                ? OperationResult<PurchaseOrderAuthority>.Fail(OrderFailures.MissingCatalog)
                : OperationResult<PurchaseOrderAuthority>.Success(new PurchaseOrderAuthority(catalog));
        }

        public OperationResult PlaceOrder(
            StableId<PurchaseOrderIdScope> orderId,
            StableId<SupplierIdScope> supplierId,
            IEnumerable<PurchaseOrderLine> lines,
            SimulationTimestamp placedAt)
        {
            if (orderId.IsEmpty)
            {
                return OperationResult.Fail(OrderFailures.InvalidOrderId);
            }

            if (supplierId.IsEmpty)
            {
                return OperationResult.Fail(OrderFailures.InvalidSupplierId);
            }

            if (lines == null)
            {
                return OperationResult.Fail(OrderFailures.MissingLines);
            }

            if (_orders.ContainsKey(orderId))
            {
                return OperationResult.Fail(OrderFailures.DuplicateOrder);
            }

            var orderedLines = new List<PurchaseOrderLine>();
            var products = new HashSet<StableId<ProductDefinitionIdScope>>();
            foreach (PurchaseOrderLine line in lines)
            {
                if (line == null)
                {
                    return OperationResult.Fail(OrderFailures.NullLine);
                }

                if (line.ProductId.IsEmpty)
                {
                    return OperationResult.Fail(OrderFailures.InvalidProductId);
                }

                if (line.Quantity <= 0)
                {
                    return OperationResult.Fail(OrderFailures.InvalidQuantity);
                }

                if (!_catalog.TryGet(line.ProductId, out _))
                {
                    return OperationResult.Fail(OrderFailures.UnknownProduct);
                }

                if (!products.Add(line.ProductId))
                {
                    return OperationResult.Fail(OrderFailures.DuplicateProductLine);
                }

                orderedLines.Add(line);
            }

            if (orderedLines.Count == 0)
            {
                return OperationResult.Fail(OrderFailures.EmptyLines);
            }

            orderedLines.Sort((left, right) =>
                string.Compare(left.ProductId.Value, right.ProductId.Value, StringComparison.Ordinal));
            IReadOnlyList<PurchaseOrderLine> immutableLines = Array.AsReadOnly(orderedLines.ToArray());
            _orders.Add(
                orderId,
                new PurchaseOrderRecord(
                    orderId,
                    supplierId,
                    PurchaseOrderStatus.Placed,
                    immutableLines,
                    placedAt,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    null,
                    default));
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ConfirmOrder(
            StableId<PurchaseOrderIdScope> orderId,
            StableId<DeliveryIdScope> deliveryId,
            SimulationTimestamp confirmedAt,
            SimulationTimestamp etaStart,
            SimulationTimestamp etaEnd)
        {
            if (!_orders.TryGetValue(orderId, out PurchaseOrderRecord order))
            {
                return OperationResult.Fail(OrderFailures.UnknownOrder);
            }

            if (order.Status != PurchaseOrderStatus.Placed)
            {
                return OperationResult.Fail(OrderFailures.InvalidStateTransition);
            }

            if (deliveryId.IsEmpty)
            {
                return OperationResult.Fail(OrderFailures.InvalidDeliveryId);
            }

            if (_deliveryOwners.ContainsKey(deliveryId))
            {
                return OperationResult.Fail(OrderFailures.DuplicateDelivery);
            }

            if (!confirmedAt.IsAtOrAfter(order.PlacedAt) ||
                !etaStart.IsAtOrAfter(confirmedAt) ||
                !etaEnd.IsAtOrAfter(etaStart))
            {
                return OperationResult.Fail(OrderFailures.InvalidTimestamp);
            }

            _orders[orderId] = Copy(
                order,
                PurchaseOrderStatus.Confirmed,
                deliveryId: deliveryId,
                confirmedAt: confirmedAt,
                etaStart: etaStart,
                etaEnd: etaEnd);
            _deliveryOwners.Add(deliveryId, orderId);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult DispatchOrder(
            StableId<PurchaseOrderIdScope> orderId,
            SimulationTimestamp dispatchedAt)
        {
            if (!_orders.TryGetValue(orderId, out PurchaseOrderRecord order))
            {
                return OperationResult.Fail(OrderFailures.UnknownOrder);
            }

            if (order.Status != PurchaseOrderStatus.Confirmed)
            {
                return OperationResult.Fail(OrderFailures.InvalidStateTransition);
            }

            if (!dispatchedAt.IsAtOrAfter(order.ConfirmedAt))
            {
                return OperationResult.Fail(OrderFailures.InvalidTimestamp);
            }

            _orders[orderId] = Copy(
                order,
                PurchaseOrderStatus.InTransit,
                dispatchedAt: dispatchedAt);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult RegisterArrival(
            StableId<PurchaseOrderIdScope> orderId,
            DeliveryManifest manifest,
            SimulationTimestamp arrivedAt)
        {
            if (!_orders.TryGetValue(orderId, out PurchaseOrderRecord order))
            {
                return OperationResult.Fail(OrderFailures.UnknownOrder);
            }

            if (order.Status != PurchaseOrderStatus.InTransit)
            {
                return OperationResult.Fail(OrderFailures.InvalidStateTransition);
            }

            if (manifest == null)
            {
                return OperationResult.Fail(OrderFailures.MissingManifest);
            }

            if (manifest.Id != order.DeliveryId)
            {
                return OperationResult.Fail(OrderFailures.DeliveryMismatch);
            }

            if (!arrivedAt.IsAtOrAfter(order.DispatchedAt))
            {
                return OperationResult.Fail(OrderFailures.InvalidTimestamp);
            }

            Failure manifestFailure = ValidateManifest(order, manifest);
            if (!manifestFailure.IsNone)
            {
                return OperationResult.Fail(manifestFailure);
            }

            _orders[orderId] = Copy(
                order,
                PurchaseOrderStatus.Arrived,
                arrivedAt: arrivedAt,
                manifest: manifest);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult AcceptDelivery(
            StableId<PurchaseOrderIdScope> orderId,
            StableId<ContainerIdScope> receivingContainerId,
            InventoryAuthority inventory,
            SimulationTimestamp acceptedAt)
        {
            if (!_orders.TryGetValue(orderId, out PurchaseOrderRecord order))
            {
                return OperationResult.Fail(OrderFailures.UnknownOrder);
            }

            if (order.Status != PurchaseOrderStatus.Arrived)
            {
                return OperationResult.Fail(OrderFailures.InvalidStateTransition);
            }

            if (inventory == null)
            {
                return OperationResult.Fail(OrderFailures.MissingInventory);
            }

            if (!acceptedAt.IsAtOrAfter(order.ArrivedAt))
            {
                return OperationResult.Fail(OrderFailures.InvalidTimestamp);
            }

            if (!inventory.TryGetContainer(receivingContainerId, out InventoryContainerDefinition container) ||
                container.Kind != InventoryContainerKind.Receiving)
            {
                return OperationResult.Fail(OrderFailures.InvalidReceivingContainer);
            }

            OperationResult intakeResult = inventory.ReceiveIntake(receivingContainerId, order.Manifest.Intake);
            if (intakeResult.IsFailure)
            {
                return intakeResult;
            }

            _orders[orderId] = Copy(
                order,
                PurchaseOrderStatus.Accepted,
                acceptedAt: acceptedAt,
                receivingContainerId: receivingContainerId);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public bool TryGetOrder(
            StableId<PurchaseOrderIdScope> orderId,
            out PurchaseOrderRecord order)
        {
            return _orders.TryGetValue(orderId, out order);
        }

        public IReadOnlyList<PurchaseOrderRecord> GetOrders()
        {
            var values = new List<PurchaseOrderRecord>(_orders.Values);
            values.Sort((left, right) =>
                string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            var deliveryIds = new HashSet<StableId<DeliveryIdScope>>();
            foreach (KeyValuePair<StableId<PurchaseOrderIdScope>, PurchaseOrderRecord> entry in _orders)
            {
                PurchaseOrderRecord order = entry.Value;
                if (order == null ||
                    entry.Key != order.Id ||
                    order.Id.IsEmpty ||
                    order.SupplierId.IsEmpty ||
                    !IsValidStatus(order.Status) ||
                    order.Lines == null ||
                    order.Lines.Count == 0)
                {
                    return OperationResult.Fail(OrderFailures.InvariantViolation);
                }

                var products = new HashSet<StableId<ProductDefinitionIdScope>>();
                for (int index = 0; index < order.Lines.Count; index++)
                {
                    PurchaseOrderLine line = order.Lines[index];
                    if (line == null ||
                        line.ProductId.IsEmpty ||
                        line.Quantity <= 0 ||
                        !_catalog.TryGet(line.ProductId, out _) ||
                        !products.Add(line.ProductId))
                    {
                        return OperationResult.Fail(OrderFailures.InvariantViolation);
                    }
                }

                if ((int)order.Status >= (int)PurchaseOrderStatus.Confirmed)
                {
                    if (order.DeliveryId.IsEmpty ||
                        !deliveryIds.Add(order.DeliveryId) ||
                        !_deliveryOwners.TryGetValue(order.DeliveryId, out StableId<PurchaseOrderIdScope> owner) ||
                        owner != order.Id ||
                        !order.ConfirmedAt.IsAtOrAfter(order.PlacedAt) ||
                        !order.EtaStart.IsAtOrAfter(order.ConfirmedAt) ||
                        !order.EtaEnd.IsAtOrAfter(order.EtaStart))
                    {
                        return OperationResult.Fail(OrderFailures.InvariantViolation);
                    }
                }

                if ((int)order.Status >= (int)PurchaseOrderStatus.InTransit &&
                    !order.DispatchedAt.IsAtOrAfter(order.ConfirmedAt))
                {
                    return OperationResult.Fail(OrderFailures.InvariantViolation);
                }

                if ((int)order.Status >= (int)PurchaseOrderStatus.Arrived)
                {
                    if (order.Manifest == null ||
                        order.Manifest.Id != order.DeliveryId ||
                        !order.ArrivedAt.IsAtOrAfter(order.DispatchedAt) ||
                        !ValidateManifest(order, order.Manifest).IsNone)
                    {
                        return OperationResult.Fail(OrderFailures.InvariantViolation);
                    }
                }

                if (order.Status == PurchaseOrderStatus.Accepted &&
                    (order.ReceivingContainerId.IsEmpty ||
                     !order.AcceptedAt.IsAtOrAfter(order.ArrivedAt)))
                {
                    return OperationResult.Fail(OrderFailures.InvariantViolation);
                }
            }

            return deliveryIds.Count == _deliveryOwners.Count
                ? OperationResult.Success()
                : OperationResult.Fail(OrderFailures.InvariantViolation);
        }

        private Failure ValidateManifest(PurchaseOrderRecord order, DeliveryManifest manifest)
        {
            var deliveredQuantities = new Dictionary<StableId<ProductDefinitionIdScope>, long>();

            for (int index = 0; index < manifest.Intake.SerializedItems.Count; index++)
            {
                InventorySerializedIntake item = manifest.Intake.SerializedItems[index];
                if (!_catalog.TryGet(item.ProductId, out ProductDefinition definition))
                {
                    return OrderFailures.UnknownProduct;
                }

                if (definition.TrackingPolicy != ProductTrackingPolicy.SerializedInstance)
                {
                    return OrderFailures.TrackingMismatch;
                }

                deliveredQuantities.TryGetValue(item.ProductId, out long quantity);
                deliveredQuantities[item.ProductId] = quantity + 1;
            }

            for (int index = 0; index < manifest.Intake.Batches.Count; index++)
            {
                InventoryBatchIntake batch = manifest.Intake.Batches[index];
                if (!_catalog.TryGet(batch.ProductId, out ProductDefinition definition))
                {
                    return OrderFailures.UnknownProduct;
                }

                if (definition.TrackingPolicy != ProductTrackingPolicy.BatchQuantity)
                {
                    return OrderFailures.TrackingMismatch;
                }

                deliveredQuantities.TryGetValue(batch.ProductId, out long quantity);
                if (long.MaxValue - quantity < batch.Quantity)
                {
                    return OrderFailures.InvalidQuantity;
                }

                deliveredQuantities[batch.ProductId] = quantity + batch.Quantity;
            }

            if (deliveredQuantities.Count != order.Lines.Count)
            {
                return OrderFailures.QuantityMismatch;
            }

            for (int index = 0; index < order.Lines.Count; index++)
            {
                PurchaseOrderLine line = order.Lines[index];
                if (!deliveredQuantities.TryGetValue(line.ProductId, out long delivered) ||
                    delivered != line.Quantity)
                {
                    return OrderFailures.QuantityMismatch;
                }
            }

            return Failure.None;
        }

        private static PurchaseOrderRecord Copy(
            PurchaseOrderRecord source,
            PurchaseOrderStatus status,
            StableId<DeliveryIdScope>? deliveryId = null,
            SimulationTimestamp? confirmedAt = null,
            SimulationTimestamp? etaStart = null,
            SimulationTimestamp? etaEnd = null,
            SimulationTimestamp? dispatchedAt = null,
            SimulationTimestamp? arrivedAt = null,
            SimulationTimestamp? acceptedAt = null,
            DeliveryManifest manifest = null,
            StableId<ContainerIdScope>? receivingContainerId = null)
        {
            return new PurchaseOrderRecord(
                source.Id,
                source.SupplierId,
                status,
                source.Lines,
                source.PlacedAt,
                deliveryId ?? source.DeliveryId,
                confirmedAt ?? source.ConfirmedAt,
                etaStart ?? source.EtaStart,
                etaEnd ?? source.EtaEnd,
                dispatchedAt ?? source.DispatchedAt,
                arrivedAt ?? source.ArrivedAt,
                acceptedAt ?? source.AcceptedAt,
                manifest ?? source.Manifest,
                receivingContainerId ?? source.ReceivingContainerId);
        }

        private static bool IsValidStatus(PurchaseOrderStatus status)
        {
            return status == PurchaseOrderStatus.Placed ||
                   status == PurchaseOrderStatus.Confirmed ||
                   status == PurchaseOrderStatus.InTransit ||
                   status == PurchaseOrderStatus.Arrived ||
                   status == PurchaseOrderStatus.Accepted;
        }

        private void AdvanceRevision()
        {
            if (Revision < long.MaxValue)
            {
                Revision++;
            }
        }
    }
}
