using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Orders
{
    public enum PurchaseOrderStatus
    {
        Placed = 1,
        Confirmed = 2,
        InTransit = 3,
        Arrived = 4,
        Accepted = 5
    }

    public sealed class PurchaseOrderLine
    {
        private PurchaseOrderLine(
            StableId<ProductDefinitionIdScope> productId,
            int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public int Quantity { get; }

        public static OperationResult<PurchaseOrderLine> Create(
            StableId<ProductDefinitionIdScope> productId,
            int quantity)
        {
            if (productId.IsEmpty)
            {
                return OperationResult<PurchaseOrderLine>.Fail(OrderFailures.InvalidProductId);
            }

            if (quantity <= 0)
            {
                return OperationResult<PurchaseOrderLine>.Fail(OrderFailures.InvalidQuantity);
            }

            return OperationResult<PurchaseOrderLine>.Success(
                new PurchaseOrderLine(productId, quantity));
        }
    }

    public sealed class DeliveryManifest
    {
        private DeliveryManifest(
            StableId<DeliveryIdScope> id,
            InventoryIntake intake)
        {
            Id = id;
            Intake = intake;
        }

        public StableId<DeliveryIdScope> Id { get; }

        public InventoryIntake Intake { get; }

        public static OperationResult<DeliveryManifest> Create(
            StableId<DeliveryIdScope> id,
            InventoryIntake intake)
        {
            if (id.IsEmpty)
            {
                return OperationResult<DeliveryManifest>.Fail(OrderFailures.InvalidDeliveryId);
            }

            if (intake == null)
            {
                return OperationResult<DeliveryManifest>.Fail(OrderFailures.MissingManifest);
            }

            return OperationResult<DeliveryManifest>.Success(new DeliveryManifest(id, intake));
        }
    }

    public sealed class PurchaseOrderRecord
    {
        internal PurchaseOrderRecord(
            StableId<PurchaseOrderIdScope> id,
            StableId<SupplierIdScope> supplierId,
            PurchaseOrderStatus status,
            IReadOnlyList<PurchaseOrderLine> lines,
            SimulationTimestamp placedAt,
            StableId<DeliveryIdScope> deliveryId,
            SimulationTimestamp confirmedAt,
            SimulationTimestamp etaStart,
            SimulationTimestamp etaEnd,
            SimulationTimestamp dispatchedAt,
            SimulationTimestamp arrivedAt,
            SimulationTimestamp acceptedAt,
            DeliveryManifest manifest,
            StableId<ContainerIdScope> receivingContainerId)
        {
            Id = id;
            SupplierId = supplierId;
            Status = status;
            Lines = lines;
            PlacedAt = placedAt;
            DeliveryId = deliveryId;
            ConfirmedAt = confirmedAt;
            EtaStart = etaStart;
            EtaEnd = etaEnd;
            DispatchedAt = dispatchedAt;
            ArrivedAt = arrivedAt;
            AcceptedAt = acceptedAt;
            Manifest = manifest;
            ReceivingContainerId = receivingContainerId;
        }

        public StableId<PurchaseOrderIdScope> Id { get; }

        public StableId<SupplierIdScope> SupplierId { get; }

        public PurchaseOrderStatus Status { get; }

        public IReadOnlyList<PurchaseOrderLine> Lines { get; }

        public SimulationTimestamp PlacedAt { get; }

        public StableId<DeliveryIdScope> DeliveryId { get; }

        public SimulationTimestamp ConfirmedAt { get; }

        public SimulationTimestamp EtaStart { get; }

        public SimulationTimestamp EtaEnd { get; }

        public SimulationTimestamp DispatchedAt { get; }

        public SimulationTimestamp ArrivedAt { get; }

        public SimulationTimestamp AcceptedAt { get; }

        public DeliveryManifest Manifest { get; }

        public StableId<ContainerIdScope> ReceivingContainerId { get; }
    }

    public static class OrderFailures
    {
        public static readonly Failure MissingCatalog = Failure.FromCode("orders.catalog.missing");
        public static readonly Failure MissingInventory = Failure.FromCode("orders.inventory.missing");
        public static readonly Failure InvalidOrderId = Failure.FromCode("orders.order-id.invalid");
        public static readonly Failure InvalidSupplierId = Failure.FromCode("orders.supplier-id.invalid");
        public static readonly Failure InvalidDeliveryId = Failure.FromCode("orders.delivery-id.invalid");
        public static readonly Failure InvalidProductId = Failure.FromCode("orders.product-id.invalid");
        public static readonly Failure UnknownProduct = Failure.FromCode("orders.product.unknown");
        public static readonly Failure InvalidQuantity = Failure.FromCode("orders.quantity.invalid");
        public static readonly Failure MissingLines = Failure.FromCode("orders.lines.missing");
        public static readonly Failure EmptyLines = Failure.FromCode("orders.lines.empty");
        public static readonly Failure NullLine = Failure.FromCode("orders.line.null");
        public static readonly Failure DuplicateProductLine = Failure.FromCode("orders.line.product-duplicate");
        public static readonly Failure DuplicateOrder = Failure.FromCode("orders.order.duplicate");
        public static readonly Failure UnknownOrder = Failure.FromCode("orders.order.unknown");
        public static readonly Failure InvalidStateTransition = Failure.FromCode("orders.state.invalid-transition");
        public static readonly Failure InvalidTimestamp = Failure.FromCode("orders.timestamp.invalid");
        public static readonly Failure DuplicateDelivery = Failure.FromCode("orders.delivery.duplicate");
        public static readonly Failure MissingManifest = Failure.FromCode("orders.manifest.missing");
        public static readonly Failure DeliveryMismatch = Failure.FromCode("orders.manifest.delivery-mismatch");
        public static readonly Failure QuantityMismatch = Failure.FromCode("orders.manifest.quantity-mismatch");
        public static readonly Failure TrackingMismatch = Failure.FromCode("orders.manifest.tracking-mismatch");
        public static readonly Failure InvalidReceivingContainer = Failure.FromCode("orders.receiving-container.invalid");
        public static readonly Failure InvariantViolation = Failure.FromCode("orders.invariant.failed");
    }
}
