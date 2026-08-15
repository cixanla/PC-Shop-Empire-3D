using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Small deterministic composition root for the first visible order-to-shelf gameplay slice.
    /// Domain authorities remain the only source of stock and order truth; Unity objects only project it.
    /// </summary>
    public sealed class GarageStockFlowSession
    {
        public const string ProductIdValue = "catalog.gpu.northstar-a60";
        public const string ProductCategoryIdValue = "catalog.category.graphics-cards";
        public const string ItemInstanceIdValue = "inventory.item.northstar-a60-001";
        public const string PurchaseOrderIdValue = "purchase-order.garage-demo-001";
        public const string SupplierIdValue = "supplier.northstar-distribution";
        public const string DeliveryIdValue = "delivery.garage-demo-001";
        public const string ReceivingContainerIdValue = "inventory.container.receiving-bay-a";
        public const string HandsContainerIdValue = "inventory.container.player-hands";
        public const string ShelfContainerIdValue = "inventory.container.retail-shelf-a";
        public const string WorldFloorContainerIdValue = "inventory.container.world-floor";
        public const string ProductDisplayName = "Northstar A60 Ekran Kartı";

        private GarageStockFlowSession(
            ProductCatalog catalog,
            InventoryAuthority inventory,
            PurchaseOrderAuthority orders)
        {
            Catalog = catalog;
            Inventory = inventory;
            Orders = orders;
        }

        public ProductCatalog Catalog { get; }

        public InventoryAuthority Inventory { get; }

        public PurchaseOrderAuthority Orders { get; }

        public StableId<ProductDefinitionIdScope> ProductId =>
            StableId<ProductDefinitionIdScope>.Parse(ProductIdValue);

        public StableId<ItemInstanceIdScope> ItemId =>
            StableId<ItemInstanceIdScope>.Parse(ItemInstanceIdValue);

        public StableId<PurchaseOrderIdScope> OrderId =>
            StableId<PurchaseOrderIdScope>.Parse(PurchaseOrderIdValue);

        public StableId<ContainerIdScope> ReceivingContainerId =>
            StableId<ContainerIdScope>.Parse(ReceivingContainerIdValue);

        public StableId<ContainerIdScope> HandsContainerId =>
            StableId<ContainerIdScope>.Parse(HandsContainerIdValue);

        public StableId<ContainerIdScope> ShelfContainerId =>
            StableId<ContainerIdScope>.Parse(ShelfContainerIdValue);

        public StableId<ContainerIdScope> WorldFloorContainerId =>
            StableId<ContainerIdScope>.Parse(WorldFloorContainerIdValue);

        public PurchaseOrderRecord Order
        {
            get
            {
                if (!Orders.TryGetOrder(OrderId, out PurchaseOrderRecord order))
                {
                    throw new System.InvalidOperationException("The prototype purchase order is missing.");
                }

                return order;
            }
        }

        public static GarageStockFlowSession CreateArrived()
        {
            ProductDefinition product = ProductDefinition.Create(
                StableId<ProductDefinitionIdScope>.Parse(ProductIdValue),
                StableId<ProductCategoryIdScope>.Parse(ProductCategoryIdValue),
                ProductDisplayName,
                ProductTrackingPolicy.SerializedInstance,
                1095).Value;
            ProductCatalog catalog = ProductCatalog.Create(new[] { product }).Value;
            InventoryAuthority inventory = InventoryAuthority.Create(catalog).Value;
            RegisterContainer(
                inventory,
                ReceivingContainerIdValue,
                InventoryContainerKind.Receiving,
                8);
            RegisterContainer(
                inventory,
                HandsContainerIdValue,
                InventoryContainerKind.ActorHands,
                1);
            RegisterContainer(
                inventory,
                ShelfContainerIdValue,
                InventoryContainerKind.Shelf,
                8);
            RegisterContainer(
                inventory,
                WorldFloorContainerIdValue,
                InventoryContainerKind.WorldFloor,
                8);

            PurchaseOrderAuthority orders = PurchaseOrderAuthority.Create(catalog).Value;
            StableId<PurchaseOrderIdScope> orderId =
                StableId<PurchaseOrderIdScope>.Parse(PurchaseOrderIdValue);
            StableId<DeliveryIdScope> deliveryId =
                StableId<DeliveryIdScope>.Parse(DeliveryIdValue);
            StableId<ProductDefinitionIdScope> productId =
                StableId<ProductDefinitionIdScope>.Parse(ProductIdValue);
            PurchaseOrderLine line = PurchaseOrderLine.Create(productId, 1).Value;
            InventorySerializedIntake serialized = InventorySerializedIntake.Create(
                StableId<ItemInstanceIdScope>.Parse(ItemInstanceIdValue),
                productId,
                InventoryCondition.New).Value;
            InventoryIntake intake = InventoryIntake.Create(
                new[] { serialized },
                System.Array.Empty<InventoryBatchIntake>()).Value;
            DeliveryManifest manifest = DeliveryManifest.Create(deliveryId, intake).Value;

            RequireSuccess(orders.PlaceOrder(
                orderId,
                StableId<SupplierIdScope>.Parse(SupplierIdValue),
                new[] { line },
                Time(1)));
            RequireSuccess(orders.ConfirmOrder(orderId, deliveryId, Time(2), Time(3), Time(5)));
            RequireSuccess(orders.DispatchOrder(orderId, Time(3)));
            RequireSuccess(orders.RegisterArrival(orderId, manifest, Time(4)));

            var session = new GarageStockFlowSession(catalog, inventory, orders);
            RequireSuccess(session.ValidateInvariants());
            return session;
        }

        public OperationResult AcceptArrivedDelivery()
        {
            return Orders.AcceptDelivery(OrderId, ReceivingContainerId, Inventory, Time(5));
        }

        public OperationResult TransferItem(StableId<ContainerIdScope> targetContainerId)
        {
            return Inventory.TransferSerializedItem(ItemId, targetContainerId);
        }

        public bool TryGetItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(ItemId, out item);
        }

        public OperationResult ValidateInvariants()
        {
            OperationResult orderResult = Orders.ValidateInvariants();
            return orderResult.IsFailure ? orderResult : Inventory.ValidateInvariants();
        }

        private static void RegisterContainer(
            InventoryAuthority inventory,
            string id,
            InventoryContainerKind kind,
            int capacity)
        {
            InventoryContainerDefinition definition = InventoryContainerDefinition.Create(
                StableId<ContainerIdScope>.Parse(id),
                kind,
                capacity).Value;
            RequireSuccess(inventory.RegisterContainer(definition));
        }

        private static SimulationTimestamp Time(long tick)
        {
            return SimulationTimestamp.Create(tick, tick * 1000L);
        }

        private static void RequireSuccess(OperationResult result)
        {
            if (result.IsFailure)
            {
                throw new System.InvalidOperationException(
                    $"Prototype stock-flow composition failed: {result.Error.Code}");
            }
        }
    }
}
