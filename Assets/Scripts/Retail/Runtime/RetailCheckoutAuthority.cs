using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// Freezes active basket offers into immutable checkout snapshots. Beginning checkout
    /// never consumes a reservation or mutates Basket, Inventory or ShelfOffer authorities.
    /// </summary>
    public sealed class RetailCheckoutAuthority
    {
        private readonly ShelfOfferAuthority _offers;
        private readonly RetailBasketAuthority _baskets;
        private readonly InventoryAuthority _inventory;
        private readonly Dictionary<StableId<RetailCheckoutIdScope>, RetailCheckoutRecord>
            _checkouts = new Dictionary<StableId<RetailCheckoutIdScope>, RetailCheckoutRecord>();

        private RetailCheckoutAuthority(
            ShelfOfferAuthority offers,
            RetailBasketAuthority baskets,
            InventoryAuthority inventory)
        {
            _offers = offers;
            _baskets = baskets;
            _inventory = inventory;
        }

        public long Revision { get; private set; }

        public int Count => _checkouts.Count;

        public static OperationResult<RetailCheckoutAuthority> Create(
            ShelfOfferAuthority offers,
            RetailBasketAuthority baskets,
            InventoryAuthority inventory)
        {
            if (offers == null)
            {
                return OperationResult<RetailCheckoutAuthority>.Fail(
                    RetailCheckoutFailures.MissingOfferAuthority);
            }

            if (baskets == null)
            {
                return OperationResult<RetailCheckoutAuthority>.Fail(
                    RetailCheckoutFailures.MissingBasketAuthority);
            }

            return inventory == null
                ? OperationResult<RetailCheckoutAuthority>.Fail(
                    RetailCheckoutFailures.MissingInventory)
                : OperationResult<RetailCheckoutAuthority>.Success(
                    new RetailCheckoutAuthority(offers, baskets, inventory));
        }

        public OperationResult BeginCheckout(
            StableId<RetailCheckoutIdScope> checkoutId,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            SimulationTimestamp startedAt)
        {
            if (checkoutId.IsEmpty)
            {
                return OperationResult.Fail(RetailCheckoutFailures.InvalidCheckoutId);
            }

            if (basketId.IsEmpty)
            {
                return OperationResult.Fail(RetailCheckoutFailures.InvalidBasketId);
            }

            if (customerId.IsEmpty)
            {
                return OperationResult.Fail(RetailCheckoutFailures.InvalidCustomerId);
            }

            if (_checkouts.TryGetValue(checkoutId, out RetailCheckoutRecord existing))
            {
                if (existing.BasketId != basketId ||
                    existing.CustomerId != customerId ||
                    existing.StartedAt != startedAt)
                {
                    return OperationResult.Fail(
                        RetailCheckoutFailures.CheckoutIdentityConflict);
                }

                Failure consistency = ValidateRecord(existing);
                return consistency.IsNone
                    ? OperationResult.Success()
                    : OperationResult.Fail(consistency);
            }

            foreach (RetailCheckoutRecord checkout in _checkouts.Values)
            {
                if (checkout.BasketId == basketId)
                {
                    return OperationResult.Fail(
                        RetailCheckoutFailures.BasketAlreadyInCheckout);
                }
            }

            Failure snapshotFailure = TryBuildSnapshot(
                basketId,
                customerId,
                out CurrencyCode currency,
                out long totalMinorUnits,
                out IReadOnlyList<RetailCheckoutLineSnapshot> snapshots);
            if (!snapshotFailure.IsNone)
            {
                return OperationResult.Fail(snapshotFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(RetailCheckoutFailures.RevisionOverflow);
            }

            _checkouts.Add(
                checkoutId,
                new RetailCheckoutRecord(
                    checkoutId,
                    basketId,
                    customerId,
                    startedAt,
                    currency,
                    totalMinorUnits,
                    snapshots));
            Revision++;
            return OperationResult.Success();
        }

        public bool TryGetCheckout(
            StableId<RetailCheckoutIdScope> checkoutId,
            out RetailCheckoutRecord checkout)
        {
            return _checkouts.TryGetValue(checkoutId, out checkout);
        }

        public bool TryGetCheckoutForBasket(
            StableId<RetailBasketIdScope> basketId,
            out RetailCheckoutRecord checkout)
        {
            foreach (RetailCheckoutRecord candidate in _checkouts.Values)
            {
                if (candidate.BasketId == basketId)
                {
                    checkout = candidate;
                    return true;
                }
            }

            checkout = null;
            return false;
        }

        public IReadOnlyList<RetailCheckoutRecord> GetCheckouts()
        {
            var ordered = new List<RetailCheckoutRecord>(_checkouts.Values);
            ordered.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(ordered.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            var baskets = new HashSet<StableId<RetailBasketIdScope>>();
            foreach (KeyValuePair<StableId<RetailCheckoutIdScope>, RetailCheckoutRecord> entry
                     in _checkouts)
            {
                RetailCheckoutRecord checkout = entry.Value;
                if (checkout == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != checkout.Id ||
                    checkout.BasketId.IsEmpty ||
                    checkout.CustomerId.IsEmpty ||
                    !baskets.Add(checkout.BasketId) ||
                    !ValidateRecord(checkout).IsNone)
                {
                    return OperationResult.Fail(RetailCheckoutFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private Failure TryBuildSnapshot(
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            out CurrencyCode currency,
            out long totalMinorUnits,
            out IReadOnlyList<RetailCheckoutLineSnapshot> snapshots)
        {
            currency = default;
            totalMinorUnits = 0;
            snapshots = null;
            var selected = new List<RetailCheckoutLineSnapshot>();
            foreach (RetailBasketLineRecord line in _baskets.GetLines())
            {
                if (line.BasketId != basketId)
                {
                    continue;
                }

                if (line.CustomerId != customerId)
                {
                    return RetailCheckoutFailures.CustomerMismatch;
                }

                Failure lineFailure = TrySnapshotLine(line, out RetailCheckoutLineSnapshot snapshot);
                if (!lineFailure.IsNone)
                {
                    return lineFailure;
                }

                if (selected.Count == 0)
                {
                    currency = snapshot.UnitPrice.Currency;
                }
                else if (currency != snapshot.UnitPrice.Currency)
                {
                    return RetailCheckoutFailures.MixedCurrency;
                }

                if (totalMinorUnits > long.MaxValue - snapshot.UnitPrice.MinorUnits)
                {
                    return RetailCheckoutFailures.TotalOverflow;
                }

                totalMinorUnits += snapshot.UnitPrice.MinorUnits;
                selected.Add(snapshot);
            }

            if (selected.Count == 0)
            {
                return RetailCheckoutFailures.UnknownOrEmptyBasket;
            }

            snapshots = Array.AsReadOnly(selected.ToArray());
            return Failure.None;
        }

        private Failure TrySnapshotLine(
            RetailBasketLineRecord line,
            out RetailCheckoutLineSnapshot snapshot)
        {
            snapshot = null;
            if (!_offers.TryGetOffer(line.OfferId, out ShelfOfferRecord offer))
            {
                return RetailCheckoutFailures.UnknownOffer;
            }

            if (!_inventory.TryGetSerializedItem(line.ItemId, out InventoryItemRecord item))
            {
                return RetailCheckoutFailures.UnknownItem;
            }

            if (item.ProductId != offer.ProductId)
            {
                return RetailCheckoutFailures.OfferProductMismatch;
            }

            if (item.ContainerId != offer.ShelfContainerId)
            {
                return RetailCheckoutFailures.ItemNotOnOfferShelf;
            }

            if (!_inventory.TryGetReservation(
                    line.InventoryReservationId,
                    out InventoryReservation reservation) ||
                !Matches(reservation, line))
            {
                return RetailCheckoutFailures.InventoryReservationDrift;
            }

            snapshot = new RetailCheckoutLineSnapshot(
                line.Id,
                line.OfferId,
                line.ItemId,
                line.InventoryReservationId,
                line.InventoryClaimId,
                offer.ProductId,
                offer.ShelfContainerId,
                offer.Price,
                offer.OfferRevision);
            return Failure.None;
        }

        private Failure ValidateRecord(RetailCheckoutRecord checkout)
        {
            if (checkout.Lines == null || checkout.Lines.Count == 0)
            {
                return RetailCheckoutFailures.CheckoutBasketDrift;
            }

            int currentBasketLineCount = 0;
            foreach (RetailBasketLineRecord currentLine in _baskets.GetLines())
            {
                if (currentLine.BasketId == checkout.BasketId)
                {
                    currentBasketLineCount++;
                }
            }

            if (currentBasketLineCount != checkout.Lines.Count)
            {
                return RetailCheckoutFailures.CheckoutBasketDrift;
            }

            var lineIds = new HashSet<StableId<RetailBasketLineIdScope>>();
            var itemIds = new HashSet<StableId<ItemInstanceIdScope>>();
            var reservationIds = new HashSet<StableId<ReservationIdScope>>();
            string previousLineId = null;
            long total = 0;
            for (int index = 0; index < checkout.Lines.Count; index++)
            {
                RetailCheckoutLineSnapshot snapshot = checkout.Lines[index];
                if (snapshot == null ||
                    snapshot.BasketLineId.IsEmpty ||
                    snapshot.OfferId.IsEmpty ||
                    snapshot.ItemId.IsEmpty ||
                    snapshot.InventoryReservationId.IsEmpty ||
                    snapshot.InventoryClaimId.IsEmpty ||
                    snapshot.ProductId.IsEmpty ||
                    snapshot.ShelfContainerId.IsEmpty ||
                    snapshot.SourceOfferRevision <= 0 ||
                    !lineIds.Add(snapshot.BasketLineId) ||
                    !itemIds.Add(snapshot.ItemId) ||
                    !reservationIds.Add(snapshot.InventoryReservationId) ||
                    ShelfPrice.Create(
                        snapshot.UnitPrice.Currency.Value,
                        snapshot.UnitPrice.MinorUnits).IsFailure ||
                    snapshot.UnitPrice.Currency != checkout.Currency)
                {
                    return RetailCheckoutFailures.CheckoutBasketDrift;
                }

                if (previousLineId != null &&
                    string.Compare(previousLineId, snapshot.BasketLineId.Value,
                        StringComparison.Ordinal) >= 0)
                {
                    return RetailCheckoutFailures.CheckoutBasketDrift;
                }

                previousLineId = snapshot.BasketLineId.Value;
                if (!_baskets.TryGetLine(
                        snapshot.BasketLineId,
                        out RetailBasketLineRecord currentLine) ||
                    currentLine.BasketId != checkout.BasketId ||
                    currentLine.CustomerId != checkout.CustomerId ||
                    currentLine.OfferId != snapshot.OfferId ||
                    currentLine.ItemId != snapshot.ItemId ||
                    currentLine.InventoryReservationId != snapshot.InventoryReservationId ||
                    currentLine.InventoryClaimId != snapshot.InventoryClaimId)
                {
                    return RetailCheckoutFailures.CheckoutBasketDrift;
                }

                if (!_offers.TryGetOffer(snapshot.OfferId, out ShelfOfferRecord offer) ||
                    offer.ProductId != snapshot.ProductId ||
                    offer.ShelfContainerId != snapshot.ShelfContainerId ||
                    offer.OfferRevision < snapshot.SourceOfferRevision)
                {
                    return RetailCheckoutFailures.CheckoutOfferDrift;
                }

                if (!_inventory.TryGetSerializedItem(
                        snapshot.ItemId,
                        out InventoryItemRecord item) ||
                    item.ProductId != snapshot.ProductId ||
                    item.ContainerId != snapshot.ShelfContainerId)
                {
                    return RetailCheckoutFailures.CheckoutItemDrift;
                }

                if (!_inventory.TryGetReservation(
                        snapshot.InventoryReservationId,
                        out InventoryReservation reservation) ||
                    !Matches(reservation, currentLine))
                {
                    return RetailCheckoutFailures.InventoryReservationDrift;
                }

                if (total > long.MaxValue - snapshot.UnitPrice.MinorUnits)
                {
                    return RetailCheckoutFailures.TotalOverflow;
                }

                total += snapshot.UnitPrice.MinorUnits;
            }

            return total == checkout.TotalMinorUnits && total > 0
                ? Failure.None
                : RetailCheckoutFailures.CheckoutBasketDrift;
        }

        private static bool Matches(
            InventoryReservation reservation,
            RetailBasketLineRecord line)
        {
            return reservation.TargetKind == InventoryReservationTargetKind.SerializedItem &&
                   reservation.Id == line.InventoryReservationId &&
                   reservation.ClaimId == line.InventoryClaimId &&
                   reservation.ItemId == line.ItemId &&
                   reservation.Quantity == 1;
        }
    }

    public static class RetailCheckoutFailures
    {
        public static readonly Failure MissingOfferAuthority = Failure.FromCode("retail.checkout.offers-missing");
        public static readonly Failure MissingBasketAuthority = Failure.FromCode("retail.checkout.baskets-missing");
        public static readonly Failure MissingInventory = Failure.FromCode("retail.checkout.inventory-missing");
        public static readonly Failure InvalidCheckoutId = Failure.FromCode("retail.checkout.id-invalid");
        public static readonly Failure InvalidBasketId = Failure.FromCode("retail.checkout.basket-id-invalid");
        public static readonly Failure InvalidCustomerId = Failure.FromCode("retail.checkout.customer-id-invalid");
        public static readonly Failure UnknownOrEmptyBasket = Failure.FromCode("retail.checkout.basket-empty");
        public static readonly Failure CustomerMismatch = Failure.FromCode("retail.checkout.customer-mismatch");
        public static readonly Failure CheckoutIdentityConflict = Failure.FromCode("retail.checkout.identity-conflict");
        public static readonly Failure BasketAlreadyInCheckout = Failure.FromCode("retail.checkout.basket-active");
        public static readonly Failure UnknownOffer = Failure.FromCode("retail.checkout.offer-unknown");
        public static readonly Failure UnknownItem = Failure.FromCode("retail.checkout.item-unknown");
        public static readonly Failure OfferProductMismatch = Failure.FromCode("retail.checkout.product-mismatch");
        public static readonly Failure ItemNotOnOfferShelf = Failure.FromCode("retail.checkout.shelf-mismatch");
        public static readonly Failure InventoryReservationDrift = Failure.FromCode("retail.checkout.reservation-drift");
        public static readonly Failure MixedCurrency = Failure.FromCode("retail.checkout.currency-mixed");
        public static readonly Failure TotalOverflow = Failure.FromCode("retail.checkout.total-overflow");
        public static readonly Failure CheckoutBasketDrift = Failure.FromCode("retail.checkout.basket-drift");
        public static readonly Failure CheckoutOfferDrift = Failure.FromCode("retail.checkout.offer-drift");
        public static readonly Failure CheckoutItemDrift = Failure.FromCode("retail.checkout.item-drift");
        public static readonly Failure RevisionOverflow = Failure.FromCode("retail.checkout.revision-overflow");
        public static readonly Failure InvariantViolation = Failure.FromCode("retail.checkout.invariant");
    }
}
