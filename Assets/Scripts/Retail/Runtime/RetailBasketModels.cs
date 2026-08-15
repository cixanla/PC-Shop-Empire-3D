using System.Collections.Generic;
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
            StableId<InventoryClaimIdScope> inventoryClaimId,
            StableId<CustomerOfferDecisionActionIdScope> ownerActionId = default)
        {
            Id = id;
            BasketId = basketId;
            CustomerId = customerId;
            OfferId = offerId;
            ItemId = itemId;
            InventoryReservationId = inventoryReservationId;
            InventoryClaimId = inventoryClaimId;
            OwnerActionId = ownerActionId;
        }

        public StableId<RetailBasketLineIdScope> Id { get; }

        public StableId<RetailBasketIdScope> BasketId { get; }

        public StableId<RetailCustomerIdScope> CustomerId { get; }

        public StableId<ShelfOfferIdScope> OfferId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ReservationIdScope> InventoryReservationId { get; }

        public StableId<InventoryClaimIdScope> InventoryClaimId { get; }

        public StableId<CustomerOfferDecisionActionIdScope> OwnerActionId { get; }

        public bool IsActionOwned => !OwnerActionId.IsEmpty;
    }

    /// <summary>
    /// Immutable Basket + Inventory reservation plan. It is bound to exact authority revisions,
    /// offer identity and one preflighted Inventory reservation plan.
    /// </summary>
    public sealed class RetailBasketReservationPlan
    {
        internal RetailBasketReservationPlan(
            RetailBasketAuthority owner,
            long expectedRevision,
            ShelfOfferRecord expectedOffer,
            RetailBasketLineRecord line,
            InventorySerializedReservationPlan inventoryPlan,
            bool isReplay)
        {
            Owner = owner;
            ExpectedRevision = expectedRevision;
            ExpectedOffer = expectedOffer;
            Line = line;
            InventoryPlan = inventoryPlan;
            IsReplay = isReplay;
        }

        internal RetailBasketAuthority Owner { get; }

        internal ShelfOfferRecord ExpectedOffer { get; }

        internal InventorySerializedReservationPlan InventoryPlan { get; }

        public long ExpectedRevision { get; }

        public RetailBasketLineRecord Line { get; }

        public bool IsReplay { get; }
    }

    /// <summary>
    /// Internal, side-effect-free permission to consume the exact basket lines captured by one
    /// checkout. Both Basket and Inventory revisions must still match when it is committed.
    /// </summary>
    internal sealed class RetailCheckoutConsumptionPlan
    {
        internal RetailCheckoutConsumptionPlan(
            RetailBasketAuthority owner,
            long expectedRevision,
            StableId<RetailCheckoutIdScope> checkoutId,
            StableId<RetailBasketIdScope> basketId,
            IReadOnlyList<StableId<RetailBasketLineIdScope>> lineIds,
            InventoryCheckoutConsumptionPlan inventoryPlan)
        {
            Owner = owner;
            ExpectedRevision = expectedRevision;
            CheckoutId = checkoutId;
            BasketId = basketId;
            LineIds = lineIds;
            InventoryPlan = inventoryPlan;
        }

        internal RetailBasketAuthority Owner { get; }

        internal IReadOnlyList<StableId<RetailBasketLineIdScope>> LineIds { get; }

        internal InventoryCheckoutConsumptionPlan InventoryPlan { get; }

        public long ExpectedRevision { get; }

        public StableId<RetailCheckoutIdScope> CheckoutId { get; }

        public StableId<RetailBasketIdScope> BasketId { get; }
    }
}
