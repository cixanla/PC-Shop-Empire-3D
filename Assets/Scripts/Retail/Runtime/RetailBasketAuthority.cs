using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// Coordinates a retail basket line with the matching Inventory reservation. All Retail
    /// preflight runs before Inventory mutation; a failed Inventory command leaves Retail
    /// untouched. Successful commands advance each participating authority exactly once.
    /// </summary>
    public sealed class RetailBasketAuthority
    {
        private readonly ShelfOfferAuthority _offers;
        private readonly InventoryAuthority _inventory;
        private readonly Dictionary<StableId<RetailBasketLineIdScope>, RetailBasketLineRecord> _lines =
            new Dictionary<StableId<RetailBasketLineIdScope>, RetailBasketLineRecord>();

        private RetailBasketAuthority(
            ShelfOfferAuthority offers,
            InventoryAuthority inventory)
        {
            _offers = offers;
            _inventory = inventory;
        }

        public long Revision { get; private set; }

        public int Count => _lines.Count;

        public static OperationResult<RetailBasketAuthority> Create(
            ShelfOfferAuthority offers,
            InventoryAuthority inventory)
        {
            if (offers == null)
            {
                return OperationResult<RetailBasketAuthority>.Fail(
                    RetailBasketFailures.MissingOfferAuthority);
            }

            return inventory == null
                ? OperationResult<RetailBasketAuthority>.Fail(
                    RetailBasketFailures.MissingInventory)
                : OperationResult<RetailBasketAuthority>.Success(
                    new RetailBasketAuthority(offers, inventory));
        }

        public OperationResult ReserveSerializedOffer(
            StableId<RetailBasketLineIdScope> lineId,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            StableId<ShelfOfferIdScope> offerId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> inventoryReservationId,
            StableId<InventoryClaimIdScope> inventoryClaimId)
        {
            Failure identityFailure = ValidateIdentities(
                lineId,
                basketId,
                customerId,
                offerId,
                itemId,
                inventoryReservationId,
                inventoryClaimId);
            if (!identityFailure.IsNone)
            {
                return OperationResult.Fail(identityFailure);
            }

            if (_lines.TryGetValue(lineId, out RetailBasketLineRecord existing))
            {
                if (!Matches(
                        existing,
                        basketId,
                        customerId,
                        offerId,
                        itemId,
                        inventoryReservationId,
                        inventoryClaimId))
                {
                    return OperationResult.Fail(RetailBasketFailures.LineIdentityConflict);
                }

                return IsConsistent(existing)
                    ? OperationResult.Success()
                    : OperationResult.Fail(RetailBasketFailures.InventoryReservationDrift);
            }

            foreach (RetailBasketLineRecord line in _lines.Values)
            {
                if (line.BasketId == basketId && line.CustomerId != customerId)
                {
                    return OperationResult.Fail(RetailBasketFailures.BasketCustomerConflict);
                }

                if (line.ItemId == itemId)
                {
                    return OperationResult.Fail(RetailBasketFailures.ItemAlreadyInBasket);
                }

                if (line.InventoryReservationId == inventoryReservationId)
                {
                    return OperationResult.Fail(RetailBasketFailures.ReservationIdentityConflict);
                }
            }

            if (!_offers.TryGetOffer(offerId, out ShelfOfferRecord offer))
            {
                return OperationResult.Fail(RetailBasketFailures.UnknownOffer);
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item))
            {
                return OperationResult.Fail(RetailBasketFailures.UnknownItem);
            }

            if (item.ProductId != offer.ProductId)
            {
                return OperationResult.Fail(RetailBasketFailures.OfferProductMismatch);
            }

            if (item.ContainerId != offer.ShelfContainerId)
            {
                return OperationResult.Fail(RetailBasketFailures.ItemNotOnOfferShelf);
            }

            if (_inventory.TryGetReservation(inventoryReservationId, out _))
            {
                return OperationResult.Fail(RetailBasketFailures.ReservationIdentityConflict);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(RetailBasketFailures.RevisionOverflow);
            }

            OperationResult reservation = _inventory.ReserveSerializedItem(
                inventoryReservationId,
                inventoryClaimId,
                itemId);
            if (reservation.IsFailure)
            {
                return reservation;
            }

            _lines.Add(
                lineId,
                new RetailBasketLineRecord(
                    lineId,
                    basketId,
                    customerId,
                    offerId,
                    itemId,
                    inventoryReservationId,
                    inventoryClaimId));
            Revision++;
            return OperationResult.Success();
        }

        public OperationResult ReleaseLine(StableId<RetailBasketLineIdScope> lineId)
        {
            if (lineId.IsEmpty)
            {
                return OperationResult.Fail(RetailBasketFailures.InvalidLineId);
            }

            if (!_lines.TryGetValue(lineId, out RetailBasketLineRecord line))
            {
                return OperationResult.Fail(RetailBasketFailures.UnknownLine);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(RetailBasketFailures.RevisionOverflow);
            }

            if (!_inventory.TryGetReservation(
                    line.InventoryReservationId,
                    out InventoryReservation reservation) ||
                !Matches(reservation, line))
            {
                return OperationResult.Fail(RetailBasketFailures.InventoryReservationDrift);
            }

            OperationResult release = _inventory.ReleaseReservation(line.InventoryReservationId);
            if (release.IsFailure)
            {
                return release;
            }

            _lines.Remove(lineId);
            Revision++;
            return OperationResult.Success();
        }

        /// <summary>
        /// Internal checkout commit boundary. It verifies that the basket still exactly matches
        /// the immutable checkout snapshot before asking Inventory to consume every reservation
        /// atomically. After that successful call, removing the preflighted lines cannot fail.
        /// </summary>
        internal OperationResult ConsumeCheckoutLines(RetailCheckoutRecord checkout)
        {
            if (checkout == null || checkout.Lines == null || checkout.Lines.Count == 0)
            {
                return OperationResult.Fail(RetailBasketFailures.CheckoutSnapshotMismatch);
            }

            int basketLineCount = 0;
            foreach (RetailBasketLineRecord current in _lines.Values)
            {
                if (current.BasketId == checkout.BasketId)
                {
                    basketLineCount++;
                }
            }

            if (basketLineCount != checkout.Lines.Count)
            {
                return OperationResult.Fail(RetailBasketFailures.CheckoutSnapshotMismatch);
            }

            var lineIds = new HashSet<StableId<RetailBasketLineIdScope>>();
            var itemIds = new HashSet<StableId<ItemInstanceIdScope>>();
            var reservationIds = new HashSet<StableId<ReservationIdScope>>();
            var reservations = new List<StableId<ReservationIdScope>>(checkout.Lines.Count);
            for (int index = 0; index < checkout.Lines.Count; index++)
            {
                RetailCheckoutLineSnapshot snapshot = checkout.Lines[index];
                if (snapshot == null ||
                    !lineIds.Add(snapshot.BasketLineId) ||
                    !itemIds.Add(snapshot.ItemId) ||
                    !reservationIds.Add(snapshot.InventoryReservationId) ||
                    !_lines.TryGetValue(
                        snapshot.BasketLineId,
                        out RetailBasketLineRecord line) ||
                    line.BasketId != checkout.BasketId ||
                    line.CustomerId != checkout.CustomerId ||
                    line.OfferId != snapshot.OfferId ||
                    line.ItemId != snapshot.ItemId ||
                    line.InventoryReservationId != snapshot.InventoryReservationId ||
                    line.InventoryClaimId != snapshot.InventoryClaimId ||
                    !_inventory.TryGetSerializedItem(
                        snapshot.ItemId,
                        out InventoryItemRecord item) ||
                    item.ProductId != snapshot.ProductId ||
                    item.ContainerId != snapshot.ShelfContainerId ||
                    !_inventory.TryGetReservation(
                        snapshot.InventoryReservationId,
                        out InventoryReservation reservation) ||
                    !Matches(reservation, line))
                {
                    return OperationResult.Fail(RetailBasketFailures.CheckoutSnapshotMismatch);
                }

                reservations.Add(snapshot.InventoryReservationId);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(RetailBasketFailures.RevisionOverflow);
            }

            OperationResult consume = _inventory.ConsumeReservations(reservations);
            if (consume.IsFailure)
            {
                return consume;
            }

            foreach (StableId<RetailBasketLineIdScope> lineId in lineIds)
            {
                _lines.Remove(lineId);
            }

            Revision++;
            return OperationResult.Success();
        }

        public bool TryGetLine(
            StableId<RetailBasketLineIdScope> lineId,
            out RetailBasketLineRecord line)
        {
            return _lines.TryGetValue(lineId, out line);
        }

        public bool TryGetLineForItem(
            StableId<ItemInstanceIdScope> itemId,
            out RetailBasketLineRecord line)
        {
            foreach (RetailBasketLineRecord candidate in _lines.Values)
            {
                if (candidate.ItemId == itemId)
                {
                    line = candidate;
                    return true;
                }
            }

            line = null;
            return false;
        }

        public IReadOnlyList<RetailBasketLineRecord> GetLines()
        {
            var ordered = new List<RetailBasketLineRecord>(_lines.Values);
            ordered.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(ordered.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            var reservedItems = new HashSet<StableId<ItemInstanceIdScope>>();
            var reservationIds = new HashSet<StableId<ReservationIdScope>>();
            foreach (KeyValuePair<StableId<RetailBasketLineIdScope>, RetailBasketLineRecord> entry in _lines)
            {
                RetailBasketLineRecord line = entry.Value;
                if (line == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != line.Id ||
                    line.BasketId.IsEmpty ||
                    line.CustomerId.IsEmpty ||
                    line.OfferId.IsEmpty ||
                    line.ItemId.IsEmpty ||
                    line.InventoryReservationId.IsEmpty ||
                    line.InventoryClaimId.IsEmpty ||
                    !reservedItems.Add(line.ItemId) ||
                    !reservationIds.Add(line.InventoryReservationId) ||
                    !IsConsistent(line))
                {
                    return OperationResult.Fail(RetailBasketFailures.InvariantViolation);
                }
            }

            foreach (RetailBasketLineRecord left in _lines.Values)
            {
                foreach (RetailBasketLineRecord right in _lines.Values)
                {
                    if (left.Id != right.Id &&
                        left.BasketId == right.BasketId &&
                        left.CustomerId != right.CustomerId)
                    {
                        return OperationResult.Fail(RetailBasketFailures.InvariantViolation);
                    }
                }
            }

            return OperationResult.Success();
        }

        private static Failure ValidateIdentities(
            StableId<RetailBasketLineIdScope> lineId,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            StableId<ShelfOfferIdScope> offerId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> inventoryReservationId,
            StableId<InventoryClaimIdScope> inventoryClaimId)
        {
            if (lineId.IsEmpty)
            {
                return RetailBasketFailures.InvalidLineId;
            }

            if (basketId.IsEmpty)
            {
                return RetailBasketFailures.InvalidBasketId;
            }

            if (customerId.IsEmpty)
            {
                return RetailBasketFailures.InvalidCustomerId;
            }

            if (offerId.IsEmpty)
            {
                return RetailBasketFailures.InvalidOfferId;
            }

            if (itemId.IsEmpty)
            {
                return RetailBasketFailures.InvalidItemId;
            }

            if (inventoryReservationId.IsEmpty)
            {
                return RetailBasketFailures.InvalidReservationId;
            }

            return inventoryClaimId.IsEmpty
                ? RetailBasketFailures.InvalidClaimId
                : Failure.None;
        }

        private static bool Matches(
            RetailBasketLineRecord line,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId,
            StableId<ShelfOfferIdScope> offerId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> inventoryReservationId,
            StableId<InventoryClaimIdScope> inventoryClaimId)
        {
            return line.BasketId == basketId &&
                   line.CustomerId == customerId &&
                   line.OfferId == offerId &&
                   line.ItemId == itemId &&
                   line.InventoryReservationId == inventoryReservationId &&
                   line.InventoryClaimId == inventoryClaimId;
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

        private bool IsConsistent(RetailBasketLineRecord line)
        {
            return _offers.TryGetOffer(line.OfferId, out ShelfOfferRecord offer) &&
                   _inventory.TryGetSerializedItem(line.ItemId, out InventoryItemRecord item) &&
                   item.ProductId == offer.ProductId &&
                   item.ContainerId == offer.ShelfContainerId &&
                   _inventory.TryGetReservation(
                       line.InventoryReservationId,
                       out InventoryReservation reservation) &&
                   Matches(reservation, line);
        }
    }

    public static class RetailBasketFailures
    {
        public static readonly Failure MissingOfferAuthority = Failure.FromCode("retail.basket.offers-missing");
        public static readonly Failure MissingInventory = Failure.FromCode("retail.basket.inventory-missing");
        public static readonly Failure InvalidLineId = Failure.FromCode("retail.basket.line-id-invalid");
        public static readonly Failure InvalidBasketId = Failure.FromCode("retail.basket.id-invalid");
        public static readonly Failure InvalidCustomerId = Failure.FromCode("retail.basket.customer-id-invalid");
        public static readonly Failure InvalidOfferId = Failure.FromCode("retail.basket.offer-id-invalid");
        public static readonly Failure InvalidItemId = Failure.FromCode("retail.basket.item-id-invalid");
        public static readonly Failure InvalidReservationId = Failure.FromCode("retail.basket.reservation-id-invalid");
        public static readonly Failure InvalidClaimId = Failure.FromCode("retail.basket.claim-id-invalid");
        public static readonly Failure UnknownOffer = Failure.FromCode("retail.basket.offer-unknown");
        public static readonly Failure UnknownItem = Failure.FromCode("retail.basket.item-unknown");
        public static readonly Failure UnknownLine = Failure.FromCode("retail.basket.line-unknown");
        public static readonly Failure OfferProductMismatch = Failure.FromCode("retail.basket.product-mismatch");
        public static readonly Failure ItemNotOnOfferShelf = Failure.FromCode("retail.basket.shelf-mismatch");
        public static readonly Failure LineIdentityConflict = Failure.FromCode("retail.basket.line-identity-conflict");
        public static readonly Failure BasketCustomerConflict = Failure.FromCode("retail.basket.customer-conflict");
        public static readonly Failure ItemAlreadyInBasket = Failure.FromCode("retail.basket.item-already-reserved");
        public static readonly Failure ReservationIdentityConflict = Failure.FromCode("retail.basket.reservation-identity-conflict");
        public static readonly Failure InventoryReservationDrift = Failure.FromCode("retail.basket.inventory-drift");
        public static readonly Failure CheckoutSnapshotMismatch = Failure.FromCode("retail.basket.checkout-snapshot-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode("retail.basket.revision-overflow");
        public static readonly Failure InvariantViolation = Failure.FromCode("retail.basket.invariant");
    }
}
