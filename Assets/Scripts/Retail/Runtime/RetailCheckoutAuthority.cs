using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// Freezes active basket offers into immutable checkout snapshots, then fulfills an exact
    /// snapshot through the preflighted Basket/Inventory commit boundary. Beginning checkout
    /// never consumes stock; completion deliberately does not claim payment settlement.
    /// </summary>
    public sealed class RetailCheckoutAuthority
    {
        private readonly ShelfOfferAuthority _offers;
        private readonly RetailBasketAuthority _baskets;
        private readonly InventoryAuthority _inventory;
        private readonly Dictionary<StableId<RetailCheckoutIdScope>, RetailCheckoutRecord>
            _checkouts = new Dictionary<StableId<RetailCheckoutIdScope>, RetailCheckoutRecord>();
        private readonly Dictionary<StableId<RetailCheckoutCompletionIdScope>, RetailCheckoutCompletionRecord>
            _completions =
                new Dictionary<StableId<RetailCheckoutCompletionIdScope>, RetailCheckoutCompletionRecord>();

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

        public int CompletionCount => _completions.Count;

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

                Failure consistency = TryGetCompletionForCheckout(
                    existing.Id,
                    out RetailCheckoutCompletionRecord completion)
                    ? ValidateCompletedRecord(existing, completion)
                    : ValidateActiveRecord(existing);
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

        /// <summary>
        /// Fulfills one immutable checkout by consuming every matching Inventory reservation and
        /// basket line as one preflighted commit. The completion record is stable and does not
        /// claim payment or Economy settlement.
        /// </summary>
        public OperationResult CompleteCheckout(
            StableId<RetailCheckoutCompletionIdScope> completionId,
            StableId<RetailCheckoutIdScope> checkoutId,
            SimulationTimestamp completedAt)
        {
            if (completionId.IsEmpty)
            {
                return OperationResult.Fail(RetailCheckoutFailures.InvalidCompletionId);
            }

            if (checkoutId.IsEmpty)
            {
                return OperationResult.Fail(RetailCheckoutFailures.InvalidCheckoutId);
            }

            if (_completions.TryGetValue(
                    completionId,
                    out RetailCheckoutCompletionRecord existing))
            {
                if (existing.CheckoutId != checkoutId || existing.CompletedAt != completedAt)
                {
                    return OperationResult.Fail(
                        RetailCheckoutFailures.CompletionIdentityConflict);
                }

                if (!_checkouts.TryGetValue(checkoutId, out RetailCheckoutRecord completedCheckout))
                {
                    return OperationResult.Fail(
                        RetailCheckoutFailures.CompletionInvariantViolation);
                }

                Failure consistency = ValidateCompletedRecord(completedCheckout, existing);
                return consistency.IsNone
                    ? OperationResult.Success()
                    : OperationResult.Fail(consistency);
            }

            if (!_checkouts.TryGetValue(checkoutId, out RetailCheckoutRecord checkout))
            {
                return OperationResult.Fail(RetailCheckoutFailures.UnknownCheckout);
            }

            if (TryGetCompletionForCheckout(checkoutId, out _))
            {
                return OperationResult.Fail(RetailCheckoutFailures.CheckoutAlreadyCompleted);
            }

            if (!completedAt.IsAtOrAfter(checkout.StartedAt))
            {
                return OperationResult.Fail(RetailCheckoutFailures.CompletionBeforeCheckout);
            }

            Failure activeFailure = ValidateActiveRecord(checkout);
            if (!activeFailure.IsNone)
            {
                return OperationResult.Fail(activeFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(RetailCheckoutFailures.RevisionOverflow);
            }

            var completion = new RetailCheckoutCompletionRecord(
                completionId,
                checkout.Id,
                checkout.BasketId,
                checkout.CustomerId,
                completedAt,
                checkout.Currency,
                checkout.TotalMinorUnits,
                checkout.Lines);
            OperationResult consume = _baskets.ConsumeCheckoutLines(checkout);
            if (consume.IsFailure)
            {
                return consume;
            }

            _completions.Add(completionId, completion);
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

        public bool TryGetCompletion(
            StableId<RetailCheckoutCompletionIdScope> completionId,
            out RetailCheckoutCompletionRecord completion)
        {
            return _completions.TryGetValue(completionId, out completion);
        }

        public bool TryGetCompletionForCheckout(
            StableId<RetailCheckoutIdScope> checkoutId,
            out RetailCheckoutCompletionRecord completion)
        {
            foreach (RetailCheckoutCompletionRecord candidate in _completions.Values)
            {
                if (candidate.CheckoutId == checkoutId)
                {
                    completion = candidate;
                    return true;
                }
            }

            completion = null;
            return false;
        }

        public IReadOnlyList<RetailCheckoutCompletionRecord> GetCompletions()
        {
            var ordered = new List<RetailCheckoutCompletionRecord>(_completions.Values);
            ordered.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(ordered.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            var baskets = new HashSet<StableId<RetailBasketIdScope>>();
            var completionsByCheckout =
                new Dictionary<StableId<RetailCheckoutIdScope>, RetailCheckoutCompletionRecord>();
            foreach (KeyValuePair<StableId<RetailCheckoutCompletionIdScope>, RetailCheckoutCompletionRecord>
                     entry in _completions)
            {
                RetailCheckoutCompletionRecord completion = entry.Value;
                if (completion == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != completion.Id ||
                    completion.CheckoutId.IsEmpty ||
                    !_checkouts.ContainsKey(completion.CheckoutId) ||
                    completionsByCheckout.ContainsKey(completion.CheckoutId))
                {
                    return OperationResult.Fail(RetailCheckoutFailures.InvariantViolation);
                }

                completionsByCheckout.Add(completion.CheckoutId, completion);
            }

            foreach (KeyValuePair<StableId<RetailCheckoutIdScope>, RetailCheckoutRecord> entry
                     in _checkouts)
            {
                RetailCheckoutRecord checkout = entry.Value;
                if (checkout == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != checkout.Id ||
                    checkout.BasketId.IsEmpty ||
                    checkout.CustomerId.IsEmpty ||
                    !baskets.Add(checkout.BasketId))
                {
                    return OperationResult.Fail(RetailCheckoutFailures.InvariantViolation);
                }

                Failure validation;
                if (completionsByCheckout.TryGetValue(
                        checkout.Id,
                        out RetailCheckoutCompletionRecord completion))
                {
                    validation = ValidateCompletedRecord(checkout, completion);
                }
                else
                {
                    validation = ValidateActiveRecord(checkout);
                }

                if (!validation.IsNone)
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

        private Failure ValidateSnapshotRecord(RetailCheckoutRecord checkout)
        {
            if (checkout == null ||
                checkout.Id.IsEmpty ||
                checkout.BasketId.IsEmpty ||
                checkout.CustomerId.IsEmpty ||
                checkout.Lines == null ||
                checkout.Lines.Count == 0 ||
                CurrencyCode.Create(checkout.Currency.Value).IsFailure)
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

        private Failure ValidateActiveRecord(RetailCheckoutRecord checkout)
        {
            Failure snapshotFailure = ValidateSnapshotRecord(checkout);
            if (!snapshotFailure.IsNone)
            {
                return snapshotFailure;
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

            for (int index = 0; index < checkout.Lines.Count; index++)
            {
                RetailCheckoutLineSnapshot snapshot = checkout.Lines[index];
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

                Failure offerFailure = ValidateCurrentOffer(snapshot);
                if (!offerFailure.IsNone)
                {
                    return offerFailure;
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
            }

            return Failure.None;
        }

        private Failure ValidateCompletedRecord(
            RetailCheckoutRecord checkout,
            RetailCheckoutCompletionRecord completion)
        {
            Failure snapshotFailure = ValidateSnapshotRecord(checkout);
            if (!snapshotFailure.IsNone ||
                completion == null ||
                completion.Id.IsEmpty ||
                completion.CheckoutId != checkout.Id ||
                completion.BasketId != checkout.BasketId ||
                completion.CustomerId != checkout.CustomerId ||
                completion.Currency != checkout.Currency ||
                completion.TotalMinorUnits != checkout.TotalMinorUnits ||
                !completion.CompletedAt.IsAtOrAfter(checkout.StartedAt) ||
                !SnapshotsMatch(checkout.Lines, completion.Lines))
            {
                return RetailCheckoutFailures.CompletionInvariantViolation;
            }

            foreach (RetailBasketLineRecord currentLine in _baskets.GetLines())
            {
                if (currentLine.BasketId == checkout.BasketId)
                {
                    return RetailCheckoutFailures.CompletionInvariantViolation;
                }
            }

            for (int index = 0; index < checkout.Lines.Count; index++)
            {
                RetailCheckoutLineSnapshot snapshot = checkout.Lines[index];
                if (!ValidateCurrentOffer(snapshot).IsNone ||
                    _baskets.TryGetLine(snapshot.BasketLineId, out _) ||
                    _inventory.TryGetSerializedItem(snapshot.ItemId, out _) ||
                    _inventory.TryGetReservation(snapshot.InventoryReservationId, out _))
                {
                    return RetailCheckoutFailures.CompletionInvariantViolation;
                }
            }

            return Failure.None;
        }

        private Failure ValidateCurrentOffer(RetailCheckoutLineSnapshot snapshot)
        {
            return _offers.TryGetOffer(snapshot.OfferId, out ShelfOfferRecord offer) &&
                   offer.ProductId == snapshot.ProductId &&
                   offer.ShelfContainerId == snapshot.ShelfContainerId &&
                   offer.OfferRevision >= snapshot.SourceOfferRevision
                ? Failure.None
                : RetailCheckoutFailures.CheckoutOfferDrift;
        }

        private static bool SnapshotsMatch(
            IReadOnlyList<RetailCheckoutLineSnapshot> checkoutLines,
            IReadOnlyList<RetailCheckoutLineSnapshot> completionLines)
        {
            if (checkoutLines == null || completionLines == null ||
                checkoutLines.Count != completionLines.Count)
            {
                return false;
            }

            for (int index = 0; index < checkoutLines.Count; index++)
            {
                RetailCheckoutLineSnapshot left = checkoutLines[index];
                RetailCheckoutLineSnapshot right = completionLines[index];
                if (left == null || right == null ||
                    left.BasketLineId != right.BasketLineId ||
                    left.OfferId != right.OfferId ||
                    left.ItemId != right.ItemId ||
                    left.InventoryReservationId != right.InventoryReservationId ||
                    left.InventoryClaimId != right.InventoryClaimId ||
                    left.ProductId != right.ProductId ||
                    left.ShelfContainerId != right.ShelfContainerId ||
                    left.UnitPrice != right.UnitPrice ||
                    left.SourceOfferRevision != right.SourceOfferRevision)
                {
                    return false;
                }
            }

            return true;
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
        public static readonly Failure InvalidCompletionId = Failure.FromCode("retail.checkout.completion-id-invalid");
        public static readonly Failure InvalidBasketId = Failure.FromCode("retail.checkout.basket-id-invalid");
        public static readonly Failure InvalidCustomerId = Failure.FromCode("retail.checkout.customer-id-invalid");
        public static readonly Failure UnknownCheckout = Failure.FromCode("retail.checkout.unknown");
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
        public static readonly Failure CompletionIdentityConflict = Failure.FromCode("retail.checkout.completion-identity-conflict");
        public static readonly Failure CheckoutAlreadyCompleted = Failure.FromCode("retail.checkout.already-completed");
        public static readonly Failure CompletionBeforeCheckout = Failure.FromCode("retail.checkout.completion-before-start");
        public static readonly Failure CompletionInvariantViolation = Failure.FromCode("retail.checkout.completion-invariant");
        public static readonly Failure RevisionOverflow = Failure.FromCode("retail.checkout.revision-overflow");
        public static readonly Failure InvariantViolation = Failure.FromCode("retail.checkout.invariant");
    }
}
