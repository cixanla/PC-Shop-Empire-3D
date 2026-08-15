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

        internal ShelfOfferAuthority OfferAuthority => _offers;

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
            OperationResult<RetailBasketReservationPlan> prepared =
                PrepareSerializedOfferReservation(
                    lineId,
                    basketId,
                    customerId,
                    offerId,
                    itemId,
                    inventoryReservationId,
                    inventoryClaimId);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedSerializedOfferReservation(prepared.Value);
        }

        public OperationResult<RetailBasketReservationPlan>
            PrepareSerializedOfferReservation(
                StableId<RetailBasketLineIdScope> lineId,
                StableId<RetailBasketIdScope> basketId,
                StableId<RetailCustomerIdScope> customerId,
                StableId<ShelfOfferIdScope> offerId,
                StableId<ItemInstanceIdScope> itemId,
                StableId<ReservationIdScope> inventoryReservationId,
                StableId<InventoryClaimIdScope> inventoryClaimId)
        {
            return PrepareSerializedOfferReservationCore(
                lineId,
                basketId,
                customerId,
                offerId,
                itemId,
                inventoryReservationId,
                inventoryClaimId,
                default,
                false);
        }

        internal OperationResult<RetailBasketReservationPlan>
            PrepareActionOwnedSerializedOfferReservation(
                StableId<RetailBasketLineIdScope> lineId,
                StableId<RetailBasketIdScope> basketId,
                StableId<RetailCustomerIdScope> customerId,
                StableId<ShelfOfferIdScope> offerId,
                StableId<ItemInstanceIdScope> itemId,
                StableId<ReservationIdScope> inventoryReservationId,
                StableId<InventoryClaimIdScope> inventoryClaimId,
                StableId<CustomerOfferDecisionActionIdScope> ownerActionId)
        {
            if (ownerActionId.IsEmpty)
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    RetailBasketFailures.ReservationPlanInvalid);
            }

            return PrepareSerializedOfferReservationCore(
                lineId,
                basketId,
                customerId,
                offerId,
                itemId,
                inventoryReservationId,
                inventoryClaimId,
                ownerActionId,
                true);
        }

        private OperationResult<RetailBasketReservationPlan>
            PrepareSerializedOfferReservationCore(
                StableId<RetailBasketLineIdScope> lineId,
                StableId<RetailBasketIdScope> basketId,
                StableId<RetailCustomerIdScope> customerId,
                StableId<ShelfOfferIdScope> offerId,
                StableId<ItemInstanceIdScope> itemId,
                StableId<ReservationIdScope> inventoryReservationId,
                StableId<InventoryClaimIdScope> inventoryClaimId,
                StableId<CustomerOfferDecisionActionIdScope> ownerActionId,
                bool consumeOnly)
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
                return OperationResult<RetailBasketReservationPlan>.Fail(identityFailure);
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
                        inventoryClaimId,
                        ownerActionId))
                {
                    return OperationResult<RetailBasketReservationPlan>.Fail(
                        RetailBasketFailures.LineIdentityConflict);
                }

                if (!IsConsistent(existing))
                {
                    return OperationResult<RetailBasketReservationPlan>.Fail(
                        RetailBasketFailures.InventoryReservationDrift);
                }

                return OperationResult<RetailBasketReservationPlan>.Success(
                    new RetailBasketReservationPlan(
                        this,
                        Revision,
                        null,
                        existing,
                        null,
                        true));
            }

            foreach (RetailBasketLineRecord candidate in _lines.Values)
            {
                if (candidate.BasketId == basketId && candidate.CustomerId != customerId)
                {
                    return OperationResult<RetailBasketReservationPlan>.Fail(
                        RetailBasketFailures.BasketCustomerConflict);
                }

                if (candidate.ItemId == itemId)
                {
                    return OperationResult<RetailBasketReservationPlan>.Fail(
                        RetailBasketFailures.ItemAlreadyInBasket);
                }

                if (candidate.InventoryReservationId == inventoryReservationId)
                {
                    return OperationResult<RetailBasketReservationPlan>.Fail(
                        RetailBasketFailures.ReservationIdentityConflict);
                }
            }

            if (!_offers.TryGetOffer(offerId, out ShelfOfferRecord offer))
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    RetailBasketFailures.UnknownOffer);
            }

            if (!_inventory.TryGetSerializedItem(itemId, out InventoryItemRecord item))
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    RetailBasketFailures.UnknownItem);
            }

            if (item.ProductId != offer.ProductId)
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    RetailBasketFailures.OfferProductMismatch);
            }

            if (item.ContainerId != offer.ShelfContainerId)
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    RetailBasketFailures.ItemNotOnOfferShelf);
            }

            if (_inventory.TryGetReservation(inventoryReservationId, out _))
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    RetailBasketFailures.ReservationIdentityConflict);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    RetailBasketFailures.RevisionOverflow);
            }

            OperationResult<InventorySerializedReservationPlan> inventoryPlan = consumeOnly
                ? _inventory.PrepareSerializedItemReservationForConsumption(
                    inventoryReservationId,
                    inventoryClaimId,
                    itemId)
                : _inventory.PrepareSerializedItemReservation(
                    inventoryReservationId,
                    inventoryClaimId,
                    itemId);
            if (inventoryPlan.IsFailure)
            {
                return OperationResult<RetailBasketReservationPlan>.Fail(
                    inventoryPlan.Error);
            }

            var preparedLine = new RetailBasketLineRecord(
                lineId,
                basketId,
                customerId,
                offerId,
                itemId,
                inventoryReservationId,
                inventoryClaimId,
                ownerActionId);
            return OperationResult<RetailBasketReservationPlan>.Success(
                new RetailBasketReservationPlan(
                    this,
                    Revision,
                    offer,
                    preparedLine,
                    inventoryPlan.Value,
                    false));
        }

        public OperationResult CommitPreparedSerializedOfferReservation(
            RetailBasketReservationPlan plan)
        {
            if (plan == null || !ReferenceEquals(plan.Owner, this))
            {
                return OperationResult.Fail(RetailBasketFailures.ReservationPlanInvalid);
            }

            if (_lines.TryGetValue(plan.Line.Id, out RetailBasketLineRecord existing) &&
                ReferenceEquals(existing, plan.Line) &&
                IsConsistent(existing))
            {
                return OperationResult.Success();
            }

            if (plan.IsReplay)
            {
                return OperationResult.Fail(RetailBasketFailures.ReservationPlanStale);
            }

            if (Revision != plan.ExpectedRevision ||
                !_offers.TryGetOffer(plan.Line.OfferId, out ShelfOfferRecord currentOffer) ||
                !ReferenceEquals(currentOffer, plan.ExpectedOffer) ||
                plan.InventoryPlan == null)
            {
                return OperationResult.Fail(RetailBasketFailures.ReservationPlanStale);
            }

            OperationResult reservation =
                _inventory.CommitPreparedSerializedItemReservation(plan.InventoryPlan);
            if (reservation.IsFailure)
            {
                return reservation;
            }

            _lines.Add(plan.Line.Id, plan.Line);
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

            if (line.IsActionOwned)
            {
                return OperationResult.Fail(RetailBasketFailures.ActionOwnedLine);
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

        internal OperationResult<RetailCheckoutConsumptionPlan> PrepareCheckoutConsumption(
            RetailCheckoutRecord checkout)
        {
            if (checkout == null || checkout.Lines == null || checkout.Lines.Count == 0)
            {
                return OperationResult<RetailCheckoutConsumptionPlan>.Fail(
                    RetailBasketFailures.CheckoutSnapshotMismatch);
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
                return OperationResult<RetailCheckoutConsumptionPlan>.Fail(
                    RetailBasketFailures.CheckoutSnapshotMismatch);
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
                    item.UnitCost != snapshot.UnitCost ||
                    !_inventory.TryGetReservation(
                        snapshot.InventoryReservationId,
                        out InventoryReservation reservation) ||
                    !Matches(reservation, line))
                {
                    return OperationResult<RetailCheckoutConsumptionPlan>.Fail(
                        RetailBasketFailures.CheckoutSnapshotMismatch);
                }

                reservations.Add(snapshot.InventoryReservationId);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<RetailCheckoutConsumptionPlan>.Fail(
                    RetailBasketFailures.RevisionOverflow);
            }

            OperationResult<InventoryCheckoutConsumptionPlan> inventoryPlan =
                _inventory.PrepareCheckoutReservationConsumption(reservations);
            if (inventoryPlan.IsFailure)
            {
                return OperationResult<RetailCheckoutConsumptionPlan>.Fail(
                    inventoryPlan.Error);
            }

            var orderedLineIds = new List<StableId<RetailBasketLineIdScope>>(lineIds);
            orderedLineIds.Sort((left, right) => string.Compare(
                left.Value,
                right.Value,
                StringComparison.Ordinal));
            return OperationResult<RetailCheckoutConsumptionPlan>.Success(
                new RetailCheckoutConsumptionPlan(
                    this,
                    Revision,
                    checkout.Id,
                    checkout.BasketId,
                    Array.AsReadOnly(orderedLineIds.ToArray()),
                    inventoryPlan.Value));
        }

        internal OperationResult CommitPreparedCheckoutConsumption(
            RetailCheckoutConsumptionPlan plan)
        {
            if (plan == null ||
                !ReferenceEquals(plan.Owner, this) ||
                plan.CheckoutId.IsEmpty ||
                plan.BasketId.IsEmpty ||
                plan.LineIds == null ||
                plan.LineIds.Count == 0 ||
                plan.InventoryPlan == null)
            {
                return OperationResult.Fail(RetailBasketFailures.CheckoutPlanInvalid);
            }

            if (plan.ExpectedRevision != Revision)
            {
                return OperationResult.Fail(RetailBasketFailures.CheckoutPlanStale);
            }

            OperationResult consume =
                _inventory.CommitPreparedCheckoutReservationConsumption(plan.InventoryPlan);
            if (consume.IsFailure)
            {
                return consume;
            }

            for (int index = 0; index < plan.LineIds.Count; index++)
            {
                _lines.Remove(plan.LineIds[index]);
            }

            Revision++;
            return OperationResult.Success();
        }

        /// <summary>
        /// Internal compatibility wrapper. Production checkout settlement prepares all
        /// participating authorities before calling the commit method directly.
        /// </summary>
        internal OperationResult ConsumeCheckoutLines(RetailCheckoutRecord checkout)
        {
            OperationResult<RetailCheckoutConsumptionPlan> prepared =
                PrepareCheckoutConsumption(checkout);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedCheckoutConsumption(prepared.Value);
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
            StableId<InventoryClaimIdScope> inventoryClaimId,
            StableId<CustomerOfferDecisionActionIdScope> ownerActionId)
        {
            return line.BasketId == basketId &&
                   line.CustomerId == customerId &&
                   line.OfferId == offerId &&
                   line.ItemId == itemId &&
                   line.InventoryReservationId == inventoryReservationId &&
                   line.InventoryClaimId == inventoryClaimId &&
                   line.OwnerActionId == ownerActionId;
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
        public static readonly Failure ReservationPlanInvalid =
            Failure.FromCode("retail.basket.reservation-plan-invalid");
        public static readonly Failure ReservationPlanStale =
            Failure.FromCode("retail.basket.reservation-plan-stale");
        public static readonly Failure CheckoutPlanInvalid =
            Failure.FromCode("retail.basket.checkout-plan-invalid");
        public static readonly Failure CheckoutPlanStale =
            Failure.FromCode("retail.basket.checkout-plan-stale");
        public static readonly Failure ActionOwnedLine =
            Failure.FromCode("retail.basket.line-action-owned");
        public static readonly Failure InvariantViolation = Failure.FromCode("retail.basket.invariant");
    }
}
