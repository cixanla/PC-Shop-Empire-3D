using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    public sealed class RetailCheckoutLineSnapshot
    {
        internal RetailCheckoutLineSnapshot(
            StableId<RetailBasketLineIdScope> basketLineId,
            StableId<ShelfOfferIdScope> offerId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> inventoryReservationId,
            StableId<InventoryClaimIdScope> inventoryClaimId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> shelfContainerId,
            InventoryUnitCost unitCost,
            ShelfPrice unitPrice,
            long sourceOfferRevision)
        {
            BasketLineId = basketLineId;
            OfferId = offerId;
            ItemId = itemId;
            InventoryReservationId = inventoryReservationId;
            InventoryClaimId = inventoryClaimId;
            ProductId = productId;
            ShelfContainerId = shelfContainerId;
            UnitCost = unitCost;
            UnitPrice = unitPrice;
            SourceOfferRevision = sourceOfferRevision;
        }

        public StableId<RetailBasketLineIdScope> BasketLineId { get; }

        public StableId<ShelfOfferIdScope> OfferId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ReservationIdScope> InventoryReservationId { get; }

        public StableId<InventoryClaimIdScope> InventoryClaimId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> ShelfContainerId { get; }

        public InventoryUnitCost UnitCost { get; }

        public ShelfPrice UnitPrice { get; }

        public long SourceOfferRevision { get; }
    }

    public sealed class RetailCheckoutRecord
    {
        internal RetailCheckoutRecord(
            StableId<RetailCheckoutIdScope> id,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            SimulationTimestamp startedAt,
            CurrencyCode currency,
            long totalMinorUnits,
            IReadOnlyList<RetailCheckoutLineSnapshot> lines)
        {
            Id = id;
            BasketId = basketId;
            CustomerId = customerId;
            StartedAt = startedAt;
            Currency = currency;
            TotalMinorUnits = totalMinorUnits;
            Lines = lines;
        }

        public StableId<RetailCheckoutIdScope> Id { get; }

        public StableId<RetailBasketIdScope> BasketId { get; }

        public StableId<RetailCustomerIdScope> CustomerId { get; }

        public SimulationTimestamp StartedAt { get; }

        public CurrencyCode Currency { get; }

        public long TotalMinorUnits { get; }

        public IReadOnlyList<RetailCheckoutLineSnapshot> Lines { get; }
    }

    /// <summary>
    /// Immutable proof that an exact checkout snapshot was fulfilled from authoritative stock.
    /// Payment and Economy settlement deliberately remain outside this record.
    /// </summary>
    public sealed class RetailCheckoutCompletionRecord
    {
        internal RetailCheckoutCompletionRecord(
            StableId<RetailCheckoutCompletionIdScope> id,
            StableId<RetailCheckoutIdScope> checkoutId,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            SimulationTimestamp completedAt,
            CurrencyCode currency,
            long totalMinorUnits,
            IReadOnlyList<RetailCheckoutLineSnapshot> lines)
        {
            Id = id;
            CheckoutId = checkoutId;
            BasketId = basketId;
            CustomerId = customerId;
            CompletedAt = completedAt;
            Currency = currency;
            TotalMinorUnits = totalMinorUnits;
            Lines = lines;
        }

        public StableId<RetailCheckoutCompletionIdScope> Id { get; }

        public StableId<RetailCheckoutIdScope> CheckoutId { get; }

        public StableId<RetailBasketIdScope> BasketId { get; }

        public StableId<RetailCustomerIdScope> CustomerId { get; }

        public SimulationTimestamp CompletedAt { get; }

        public CurrencyCode Currency { get; }

        public long TotalMinorUnits { get; }

        public IReadOnlyList<RetailCheckoutLineSnapshot> Lines { get; }
    }

    /// <summary>
    /// Internal immutable permission to fulfill one checkout after every Basket and Inventory
    /// preflight has succeeded. Economy is the production friend that coordinates its commit.
    /// </summary>
    internal sealed class RetailCheckoutCompletionPlan
    {
        internal RetailCheckoutCompletionPlan(
            RetailCheckoutAuthority owner,
            long expectedRevision,
            RetailCheckoutCompletionRecord completion,
            RetailCheckoutConsumptionPlan basketPlan,
            bool isReplay)
        {
            Owner = owner;
            ExpectedRevision = expectedRevision;
            Completion = completion;
            BasketPlan = basketPlan;
            IsReplay = isReplay;
        }

        internal RetailCheckoutAuthority Owner { get; }

        internal RetailCheckoutConsumptionPlan BasketPlan { get; }

        public long ExpectedRevision { get; }

        public RetailCheckoutCompletionRecord Completion { get; }

        public bool IsReplay { get; }
    }
}
