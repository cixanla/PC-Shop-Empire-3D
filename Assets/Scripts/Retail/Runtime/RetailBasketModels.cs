using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// An exact serialized-item selection. Price is intentionally not copied here: the
    /// transaction boundary will freeze an immutable offer snapshot when checkout begins.
    /// </summary>
    public sealed class RetailBasketLineRecord
    {
        internal RetailBasketLineRecord(
            StableId<RetailBasketLineIdScope> id,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            StableId<ShelfOfferIdScope> offerId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> inventoryReservationId,
            StableId<InventoryClaimIdScope> inventoryClaimId)
        {
            Id = id;
            BasketId = basketId;
            CustomerId = customerId;
            OfferId = offerId;
            ItemId = itemId;
            InventoryReservationId = inventoryReservationId;
            InventoryClaimId = inventoryClaimId;
        }

        public StableId<RetailBasketLineIdScope> Id { get; }

        public StableId<RetailBasketIdScope> BasketId { get; }

        public StableId<RetailCustomerIdScope> CustomerId { get; }

        public StableId<ShelfOfferIdScope> OfferId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ReservationIdScope> InventoryReservationId { get; }

        public StableId<InventoryClaimIdScope> InventoryClaimId { get; }
    }
}
