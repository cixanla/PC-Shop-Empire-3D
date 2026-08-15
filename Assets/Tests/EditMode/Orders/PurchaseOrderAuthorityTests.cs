using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed class PurchaseOrderAuthorityTests
    {
        private static readonly StableId<ProductDefinitionIdScope> SerializedProduct = ProductId("orders.graphics-card");
        private static readonly StableId<ProductDefinitionIdScope> BatchProduct = ProductId("orders.cable-tie");
        private static readonly StableId<ContainerIdScope> Receiving = ContainerId("orders.receiving");
        private static readonly StableId<ContainerIdScope> Shelf = ContainerId("orders.shelf");
        private static readonly StableId<PurchaseOrderIdScope> Order = OrderId("purchase-order.001");
        private static readonly StableId<DeliveryIdScope> Delivery = DeliveryId("delivery.001");
        private static readonly InventoryUnitCost SerializedCost = UnitCost("EUR", 42_000);
        private static readonly InventoryUnitCost BatchCost = UnitCost("EUR", 25);

        [Test]
        public void CompleteDeliveryEntersInventoryOnlyAfterPhysicalAcceptance()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            DeliveryManifest manifest = CompleteManifest();

            Assert.That(fixture.Orders.RegisterArrival(Order, manifest, Time(5)).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.GetTotalQuantity(SerializedProduct).Value, Is.Zero);
            Assert.That(fixture.Inventory.GetTotalQuantity(BatchProduct).Value, Is.Zero);

            Assert.That(fixture.Orders.AcceptDelivery(
                Order, Receiving, fixture.Inventory, Time(6)).IsSuccess, Is.True);

            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord accepted), Is.True);
            Assert.That(accepted.Status, Is.EqualTo(PurchaseOrderStatus.Accepted));
            Assert.That(accepted.ReceivingContainerId, Is.EqualTo(Receiving));
            Assert.That(fixture.Inventory.GetTotalQuantity(SerializedProduct).Value, Is.EqualTo(2));
            Assert.That(fixture.Inventory.GetTotalQuantity(BatchProduct).Value, Is.EqualTo(4));
            Assert.That(fixture.Inventory.GetContainerQuantity(Receiving).Value, Is.EqualTo(6));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                ItemId("item.gpu-001"), out InventoryItemRecord serialized), Is.True);
            Assert.That(serialized.UnitCost, Is.EqualTo(SerializedCost));
            Assert.That(fixture.Inventory.TryGetBatch(
                BatchId("batch.ties-001"), out InventoryBatchRecord batch), Is.True);
            Assert.That(batch.UnitCost, Is.EqualTo(BatchCost));
            Assert.That(fixture.Orders.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SuccessfulLifecycleAdvancesOrderRevisionOncePerTransition()
        {
            Fixture fixture = CreateFixture();
            Assert.That(fixture.Orders.Revision, Is.Zero);

            Place(fixture);
            Assert.That(fixture.Orders.Revision, Is.EqualTo(1));
            Confirm(fixture);
            Assert.That(fixture.Orders.Revision, Is.EqualTo(2));
            Assert.That(fixture.Orders.DispatchOrder(Order, Time(4)).IsSuccess, Is.True);
            Assert.That(fixture.Orders.Revision, Is.EqualTo(3));
            Assert.That(fixture.Orders.RegisterArrival(Order, CompleteManifest(), Time(5)).IsSuccess, Is.True);
            Assert.That(fixture.Orders.Revision, Is.EqualTo(4));
            Assert.That(fixture.Orders.AcceptDelivery(Order, Receiving, fixture.Inventory, Time(6)).IsSuccess, Is.True);
            Assert.That(fixture.Orders.Revision, Is.EqualTo(5));
        }

        [Test]
        public void InvalidTransitionDoesNotMutateOrder()
        {
            Fixture fixture = CreateFixture();
            Place(fixture);
            long revision = fixture.Orders.Revision;

            OperationResult result = fixture.Orders.DispatchOrder(Order, Time(2));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.InvalidStateTransition));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord record), Is.True);
            Assert.That(record.Status, Is.EqualTo(PurchaseOrderStatus.Placed));
        }

        [Test]
        public void StaleTransitionTimestampDoesNotMutateOrder()
        {
            Fixture fixture = CreateFixture();
            Place(fixture);
            long revision = fixture.Orders.Revision;

            OperationResult result = fixture.Orders.ConfirmOrder(
                Order, Delivery, Time(0), Time(2), Time(3));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.InvalidTimestamp));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord record), Is.True);
            Assert.That(record.Status, Is.EqualTo(PurchaseOrderStatus.Placed));
        }

        [Test]
        public void MissingManifestQuantityCannotRegisterArrival()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            DeliveryManifest incomplete = Manifest(
                Delivery,
                new[] { Serialized("item.gpu-001", SerializedProduct) },
                new[] { Batch("batch.ties-001", BatchProduct, 4) });
            long revision = fixture.Orders.Revision;

            OperationResult result = fixture.Orders.RegisterArrival(Order, incomplete, Time(5));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.QuantityMismatch));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord record), Is.True);
            Assert.That(record.Status, Is.EqualTo(PurchaseOrderStatus.InTransit));
            Assert.That(fixture.Inventory.GetContainerQuantity(Receiving).Value, Is.Zero);
        }

        [Test]
        public void WrongDeliveryManifestCannotRegisterArrival()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            DeliveryManifest wrongDelivery = Manifest(
                DeliveryId("delivery.wrong"),
                new[]
                {
                    Serialized("item.gpu-001", SerializedProduct),
                    Serialized("item.gpu-002", SerializedProduct)
                },
                new[] { Batch("batch.ties-001", BatchProduct, 4) });
            long revision = fixture.Orders.Revision;

            OperationResult result = fixture.Orders.RegisterArrival(Order, wrongDelivery, Time(5));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.DeliveryMismatch));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Inventory.GetContainerQuantity(Receiving).Value, Is.Zero);
        }

        [Test]
        public void ExtraManifestProductCannotRegisterArrival()
        {
            Fixture fixture = CreateFixture(includeExtraProduct: true);
            PlaceConfirmDispatch(fixture);
            DeliveryManifest extra = Manifest(
                Delivery,
                new[]
                {
                    Serialized("item.gpu-001", SerializedProduct),
                    Serialized("item.gpu-002", SerializedProduct),
                    Serialized("item.extra", ProductId("orders.extra-product"))
                },
                new[] { Batch("batch.ties-001", BatchProduct, 4) });

            OperationResult result = fixture.Orders.RegisterArrival(Order, extra, Time(5));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.QuantityMismatch));
            Assert.That(fixture.Inventory.SerializedItemCount, Is.Zero);
        }

        [Test]
        public void TrackingMismatchCannotRegisterArrival()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            DeliveryManifest wrongTracking = Manifest(
                Delivery,
                new[]
                {
                    Serialized("item.gpu-001", SerializedProduct),
                    Serialized("item.gpu-002", SerializedProduct)
                },
                new[] { Batch("batch.wrong", SerializedProduct, 4) });

            OperationResult result = fixture.Orders.RegisterArrival(Order, wrongTracking, Time(5));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.TrackingMismatch));
            Assert.That(fixture.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(fixture.Inventory.BatchCount, Is.Zero);
        }

        [Test]
        public void SerializedUnitCostMismatchCannotRegisterArrivalOrMutateEitherAuthority()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            InventoryUnitCost mismatchedCost = UnitCost("EUR", SerializedCost.MinorUnits + 1);
            DeliveryManifest manifest = Manifest(
                Delivery,
                new[]
                {
                    Serialized("item.gpu-001", SerializedProduct, mismatchedCost),
                    Serialized("item.gpu-002", SerializedProduct)
                },
                new[] { Batch("batch.ties-001", BatchProduct, 4) });
            long orderRevision = fixture.Orders.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = fixture.Orders.RegisterArrival(Order, manifest, Time(5));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.UnitCostMismatch));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord record), Is.True);
            Assert.That(record.Status, Is.EqualTo(PurchaseOrderStatus.InTransit));
            Assert.That(record.Manifest, Is.Null);
            Assert.That(fixture.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(fixture.Inventory.BatchCount, Is.Zero);
            Assert.That(fixture.Orders.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BatchUnitCostMismatchCannotRegisterArrivalOrMutateEitherAuthority()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            InventoryUnitCost mismatchedCost = UnitCost("USD", BatchCost.MinorUnits);
            DeliveryManifest manifest = Manifest(
                Delivery,
                new[]
                {
                    Serialized("item.gpu-001", SerializedProduct),
                    Serialized("item.gpu-002", SerializedProduct)
                },
                new[] { Batch("batch.ties-001", BatchProduct, 4, mismatchedCost) });
            long orderRevision = fixture.Orders.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = fixture.Orders.RegisterArrival(Order, manifest, Time(5));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.UnitCostMismatch));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord record), Is.True);
            Assert.That(record.Status, Is.EqualTo(PurchaseOrderStatus.InTransit));
            Assert.That(record.Manifest, Is.Null);
            Assert.That(fixture.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(fixture.Inventory.BatchCount, Is.Zero);
            Assert.That(fixture.Orders.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReceivingCapacityFailureLeavesBothAuthoritiesUnchanged()
        {
            Fixture fixture = CreateFixture(receivingCapacity: 5);
            PlaceConfirmDispatch(fixture);
            fixture.Orders.RegisterArrival(Order, CompleteManifest(), Time(5));
            long orderRevision = fixture.Orders.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = fixture.Orders.AcceptDelivery(
                Order, Receiving, fixture.Inventory, Time(6));

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord record), Is.True);
            Assert.That(record.Status, Is.EqualTo(PurchaseOrderStatus.Arrived));
            Assert.That(fixture.Inventory.GetContainerQuantity(Receiving).Value, Is.Zero);
        }

        [Test]
        public void ExistingInventoryIdentityRejectsWholeDeliveryAndLeavesOrderArrived()
        {
            Fixture fixture = CreateFixture();
            fixture.Inventory.ReceiveSerializedItem(
                ItemId("item.gpu-002"),
                SerializedProduct,
                Receiving,
                InventoryCondition.New,
                SerializedCost);
            PlaceConfirmDispatch(fixture);
            fixture.Orders.RegisterArrival(Order, CompleteManifest(), Time(5));
            long orderRevision = fixture.Orders.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = fixture.Orders.AcceptDelivery(
                Order, Receiving, fixture.Inventory, Time(6));

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.DuplicateItem));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Inventory.SerializedItemCount, Is.EqualTo(1));
            Assert.That(fixture.Inventory.BatchCount, Is.Zero);
            Assert.That(fixture.Orders.TryGetOrder(Order, out PurchaseOrderRecord record), Is.True);
            Assert.That(record.Status, Is.EqualTo(PurchaseOrderStatus.Arrived));
        }

        [Test]
        public void NonReceivingContainerCannotAcceptDelivery()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            fixture.Orders.RegisterArrival(Order, CompleteManifest(), Time(5));
            long orderRevision = fixture.Orders.Revision;

            OperationResult result = fixture.Orders.AcceptDelivery(
                Order, Shelf, fixture.Inventory, Time(6));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.InvalidReceivingContainer));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Inventory.GetContainerQuantity(Shelf).Value, Is.Zero);
        }

        [Test]
        public void DuplicateAcceptanceCannotDuplicateStock()
        {
            Fixture fixture = CreateFixture();
            PlaceConfirmDispatch(fixture);
            fixture.Orders.RegisterArrival(Order, CompleteManifest(), Time(5));
            fixture.Orders.AcceptDelivery(Order, Receiving, fixture.Inventory, Time(6));
            long orderRevision = fixture.Orders.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = fixture.Orders.AcceptDelivery(
                Order, Receiving, fixture.Inventory, Time(7));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.InvalidStateTransition));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Inventory.GetContainerQuantity(Receiving).Value, Is.EqualTo(6));
        }

        [Test]
        public void DuplicateOrderAndDeliveryIdsFailWithoutMutation()
        {
            Fixture fixture = CreateFixture();
            Place(fixture);
            long revision = fixture.Orders.Revision;
            Assert.That(fixture.Orders.PlaceOrder(
                Order,
                SupplierId("supplier.second"),
                ValidLines(),
                Time(1)).Error,
                Is.EqualTo(OrderFailures.DuplicateOrder));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(revision));

            Confirm(fixture);
            StableId<PurchaseOrderIdScope> secondOrder = OrderId("purchase-order.002");
            fixture.Orders.PlaceOrder(
                secondOrder,
                SupplierId("supplier.second"),
                ValidLines(),
                Time(2));
            revision = fixture.Orders.Revision;

            Assert.That(fixture.Orders.ConfirmOrder(
                secondOrder, Delivery, Time(3), Time(4), Time(5)).Error,
                Is.EqualTo(OrderFailures.DuplicateDelivery));
            Assert.That(fixture.Orders.Revision, Is.EqualTo(revision));
        }

        [Test]
        public void OrdersAndLinesUseDeterministicStableIdOrder()
        {
            Fixture fixture = CreateFixture();
            fixture.Orders.PlaceOrder(
                OrderId("purchase-order.zeta"),
                SupplierId("supplier.zeta"),
                new[]
                {
                    PurchaseOrderLine.Create(BatchProduct, 2, BatchCost).Value,
                    PurchaseOrderLine.Create(SerializedProduct, 1, SerializedCost).Value
                },
                Time(1));
            fixture.Orders.PlaceOrder(
                OrderId("purchase-order.alpha"),
                SupplierId("supplier.alpha"),
                ValidLines(),
                Time(1));

            Assert.That(fixture.Orders.GetOrders().Select(order => order.Id.Value),
                Is.EqualTo(new[] { "purchase-order.alpha", "purchase-order.zeta" }));
            Assert.That(fixture.Orders.GetOrders()[0].Lines.Select(line => line.ProductId.Value),
                Is.Ordered.Using<string>(System.StringComparer.Ordinal));
        }

        [Test]
        public void DuplicateProductLinesAreRejectedWithoutCreatingOrder()
        {
            Fixture fixture = CreateFixture();
            PurchaseOrderLine line = PurchaseOrderLine.Create(
                SerializedProduct, 1, SerializedCost).Value;

            OperationResult result = fixture.Orders.PlaceOrder(
                Order, SupplierId("supplier.test"), new[] { line, line }, Time(1));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.DuplicateProductLine));
            Assert.That(fixture.Orders.Count, Is.Zero);
            Assert.That(fixture.Orders.Revision, Is.Zero);
        }

        [Test]
        public void UnknownProductLineIsRejectedWithoutCreatingOrder()
        {
            Fixture fixture = CreateFixture();
            PurchaseOrderLine unknown = PurchaseOrderLine.Create(
                ProductId("orders.unknown"), 1, SerializedCost).Value;

            OperationResult result = fixture.Orders.PlaceOrder(
                Order, SupplierId("supplier.test"), new[] { unknown }, Time(1));

            Assert.That(result.Error, Is.EqualTo(OrderFailures.UnknownProduct));
            Assert.That(fixture.Orders.Count, Is.Zero);
            Assert.That(fixture.Orders.Revision, Is.Zero);
        }

        [Test]
        public void PurchaseOrderLineRejectsMissingUnitCost()
        {
            OperationResult<PurchaseOrderLine> result = PurchaseOrderLine.Create(
                SerializedProduct,
                1,
                default);

            Assert.That(result.Error, Is.EqualTo(OrderFailures.InvalidUnitCost));
        }

        private static void PlaceConfirmDispatch(Fixture fixture)
        {
            Place(fixture);
            Confirm(fixture);
            Assert.That(fixture.Orders.DispatchOrder(Order, Time(4)).IsSuccess, Is.True);
        }

        private static void Place(Fixture fixture)
        {
            Assert.That(fixture.Orders.PlaceOrder(
                Order,
                SupplierId("supplier.northstar"),
                ValidLines(),
                Time(1)).IsSuccess, Is.True);
        }

        private static void Confirm(Fixture fixture)
        {
            Assert.That(fixture.Orders.ConfirmOrder(
                Order, Delivery, Time(2), Time(4), Time(8)).IsSuccess, Is.True);
        }

        private static PurchaseOrderLine[] ValidLines()
        {
            return new[]
            {
                PurchaseOrderLine.Create(BatchProduct, 4, BatchCost).Value,
                PurchaseOrderLine.Create(SerializedProduct, 2, SerializedCost).Value
            };
        }

        private static DeliveryManifest CompleteManifest()
        {
            return Manifest(
                Delivery,
                new[]
                {
                    Serialized("item.gpu-001", SerializedProduct),
                    Serialized("item.gpu-002", SerializedProduct)
                },
                new[] { Batch("batch.ties-001", BatchProduct, 4) });
        }

        private static DeliveryManifest Manifest(
            StableId<DeliveryIdScope> deliveryId,
            InventorySerializedIntake[] serialized,
            InventoryBatchIntake[] batches)
        {
            InventoryIntake intake = InventoryIntake.Create(serialized, batches).Value;
            return DeliveryManifest.Create(deliveryId, intake).Value;
        }

        private static InventorySerializedIntake Serialized(
            string itemId,
            StableId<ProductDefinitionIdScope> productId,
            InventoryUnitCost? unitCost = null)
        {
            return InventorySerializedIntake.Create(
                ItemId(itemId),
                productId,
                InventoryCondition.New,
                unitCost ?? SerializedCost).Value;
        }

        private static InventoryBatchIntake Batch(
            string batchId,
            StableId<ProductDefinitionIdScope> productId,
            int quantity,
            InventoryUnitCost? unitCost = null)
        {
            return InventoryBatchIntake.Create(
                BatchId(batchId),
                productId,
                InventoryCondition.New,
                quantity,
                unitCost ?? BatchCost).Value;
        }

        private static Fixture CreateFixture(int receivingCapacity = 50, bool includeExtraProduct = false)
        {
            var definitions = new System.Collections.Generic.List<ProductDefinition>
            {
                ProductDefinition.Create(
                    SerializedProduct,
                    CategoryId("graphics-cards"),
                    "Order Graphics Card",
                    ProductTrackingPolicy.SerializedInstance,
                    1095).Value,
                ProductDefinition.Create(
                    BatchProduct,
                    CategoryId("accessories"),
                    "Order Cable Tie",
                    ProductTrackingPolicy.BatchQuantity,
                    0).Value
            };
            if (includeExtraProduct)
            {
                definitions.Add(ProductDefinition.Create(
                    ProductId("orders.extra-product"),
                    CategoryId("graphics-cards"),
                    "Extra Product",
                    ProductTrackingPolicy.SerializedInstance,
                    365).Value);
            }

            ProductCatalog catalog = ProductCatalog.Create(definitions).Value;
            InventoryAuthority inventory = InventoryAuthority.Create(catalog).Value;
            inventory.RegisterContainer(InventoryContainerDefinition.Create(
                Receiving, InventoryContainerKind.Receiving, receivingCapacity).Value);
            inventory.RegisterContainer(InventoryContainerDefinition.Create(
                Shelf, InventoryContainerKind.Shelf, 50).Value);
            return new Fixture(
                PurchaseOrderAuthority.Create(catalog).Value,
                inventory);
        }

        private static SimulationTimestamp Time(long value) =>
            SimulationTimestamp.Create(value, value * 1000);

        private static StableId<ProductDefinitionIdScope> ProductId(string value) =>
            StableId<ProductDefinitionIdScope>.Parse(value);

        private static StableId<ProductCategoryIdScope> CategoryId(string value) =>
            StableId<ProductCategoryIdScope>.Parse(value);

        private static StableId<ContainerIdScope> ContainerId(string value) =>
            StableId<ContainerIdScope>.Parse(value);

        private static StableId<ItemInstanceIdScope> ItemId(string value) =>
            StableId<ItemInstanceIdScope>.Parse(value);

        private static StableId<BatchIdScope> BatchId(string value) =>
            StableId<BatchIdScope>.Parse(value);

        private static StableId<PurchaseOrderIdScope> OrderId(string value) =>
            StableId<PurchaseOrderIdScope>.Parse(value);

        private static StableId<SupplierIdScope> SupplierId(string value) =>
            StableId<SupplierIdScope>.Parse(value);

        private static StableId<DeliveryIdScope> DeliveryId(string value) =>
            StableId<DeliveryIdScope>.Parse(value);

        private static InventoryUnitCost UnitCost(string currencyCode, long minorUnits) =>
            InventoryUnitCost.Create(currencyCode, minorUnits).Value;

        private sealed class Fixture
        {
            public Fixture(PurchaseOrderAuthority orders, InventoryAuthority inventory)
            {
                Orders = orders;
                Inventory = inventory;
            }

            public PurchaseOrderAuthority Orders { get; }

            public InventoryAuthority Inventory { get; }
        }
    }
}
