using System;
using System.Collections.Generic;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    public sealed class CustomerRetailIdentityBindingIdScope : IStableIdScope
    {
    }

    public sealed class CustomerOfferDecisionActionIdScope : IStableIdScope
    {
    }

    /// <summary>
    /// Explicit typed bridge between the Actors customer identity and its Retail identity.
    /// Neither side is converted to a string or cast into the other scope.
    /// </summary>
    public sealed class CustomerRetailIdentityBinding : IEquatable<CustomerRetailIdentityBinding>
    {
        private CustomerRetailIdentityBinding(
            StableId<CustomerRetailIdentityBindingIdScope> id,
            StableId<CustomerIdScope> actorCustomerId,
            StableId<RetailCustomerIdScope> retailCustomerId)
        {
            Id = id;
            ActorCustomerId = actorCustomerId;
            RetailCustomerId = retailCustomerId;
        }

        public StableId<CustomerRetailIdentityBindingIdScope> Id { get; }

        public StableId<CustomerIdScope> ActorCustomerId { get; }

        public StableId<RetailCustomerIdScope> RetailCustomerId { get; }

        public static OperationResult<CustomerRetailIdentityBinding> Create(
            StableId<CustomerRetailIdentityBindingIdScope> id,
            StableId<CustomerIdScope> actorCustomerId,
            StableId<RetailCustomerIdScope> retailCustomerId)
        {
            return id.IsEmpty || actorCustomerId.IsEmpty || retailCustomerId.IsEmpty
                ? OperationResult<CustomerRetailIdentityBinding>.Fail(
                    CustomerOfferDecisionActionFailures.InputInvalid)
                : OperationResult<CustomerRetailIdentityBinding>.Success(
                    new CustomerRetailIdentityBinding(
                        id,
                        actorCustomerId,
                        retailCustomerId));
        }

        public bool Equals(CustomerRetailIdentityBinding other)
        {
            return !ReferenceEquals(other, null) &&
                   Id == other.Id &&
                   ActorCustomerId == other.ActorCustomerId &&
                   RetailCustomerId == other.RetailCustomerId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CustomerRetailIdentityBinding);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Id.GetHashCode();
                hash = (hash * 397) ^ ActorCustomerId.GetHashCode();
                return (hash * 397) ^ RetailCustomerId.GetHashCode();
            }
        }
    }

    /// <summary>
    /// Immutable historical receipt for one accepted Buy or Leave action. It deliberately keeps
    /// no requirement that its reservation or visit state must still be live after fulfillment
    /// or exit. Leave receipts carry empty reservation identities by contract.
    /// </summary>
    public sealed class CustomerOfferDecisionActionRecord
    {
        internal CustomerOfferDecisionActionRecord(
            StableId<CustomerOfferDecisionActionIdScope> id,
            CustomerRetailIdentityBinding customerBinding,
            CustomerOfferDecision sourceDecision,
            StableId<RetailBasketLineIdScope> lineId,
            StableId<RetailBasketIdScope> basketId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId,
            SimulationTimestamp appliedAt)
        {
            Id = id;
            CustomerBinding = customerBinding;
            SourceDecision = sourceDecision;
            LineId = lineId;
            BasketId = basketId;
            ItemId = itemId;
            ReservationId = reservationId;
            ClaimId = claimId;
            AppliedAt = appliedAt;
        }

        public StableId<CustomerOfferDecisionActionIdScope> Id { get; }

        public CustomerRetailIdentityBinding CustomerBinding { get; }

        public CustomerOfferDecision SourceDecision { get; }

        public StableId<RetailBasketLineIdScope> LineId { get; }

        public StableId<RetailBasketIdScope> BasketId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ReservationIdScope> ReservationId { get; }

        public StableId<InventoryClaimIdScope> ClaimId { get; }

        public SimulationTimestamp AppliedAt { get; }

        public bool IsBuy => SourceDecision.DecisionKind == CustomerOfferDecisionKind.Buy;

        public bool IsLeave => SourceDecision.DecisionKind == CustomerOfferDecisionKind.Leave;

        public bool HasReservation =>
            !LineId.IsEmpty &&
            !BasketId.IsEmpty &&
            !ItemId.IsEmpty &&
            !ReservationId.IsEmpty &&
            !ClaimId.IsEmpty;

        internal bool Matches(
            CustomerRetailIdentityBinding customerBinding,
            CustomerOfferDecision sourceDecision,
            StableId<RetailBasketLineIdScope> lineId,
            StableId<RetailBasketIdScope> basketId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId,
            SimulationTimestamp appliedAt)
        {
            return IsBuy &&
                   HasReservation &&
                   CustomerBinding.Equals(customerBinding) &&
                   SourceDecision.Equals(sourceDecision) &&
                   LineId == lineId &&
                   BasketId == basketId &&
                   ItemId == itemId &&
                   ReservationId == reservationId &&
                   ClaimId == claimId &&
                   AppliedAt == appliedAt;
        }

        internal bool MatchesLeave(
            CustomerRetailIdentityBinding customerBinding,
            CustomerOfferDecision sourceDecision,
            SimulationTimestamp appliedAt)
        {
            return IsLeave &&
                   CustomerBinding.Equals(customerBinding) &&
                   SourceDecision.Equals(sourceDecision) &&
                   LineId.IsEmpty &&
                   BasketId.IsEmpty &&
                   ItemId.IsEmpty &&
                   ReservationId.IsEmpty &&
                   ClaimId.IsEmpty &&
                   AppliedAt == appliedAt;
        }
    }

    /// <summary>
    /// Revalidates one immutable Buy/Leave decision against current Actors and ShelfOffer state.
    /// Buy commits an exact reservation plus checkout navigation; Leave commits only the
    /// prepared OfferDeclined customer exit while all commerce authorities remain untouched.
    /// </summary>
    public sealed class CustomerOfferDecisionActionAuthority
    {
        private readonly ShelfOfferAuthority _offers;
        private readonly RetailBasketAuthority _baskets;
        private readonly CustomerVisitAuthority _visits;
        private readonly Dictionary<StableId<CustomerOfferDecisionActionIdScope>,
            CustomerOfferDecisionActionRecord> _actions =
                new Dictionary<StableId<CustomerOfferDecisionActionIdScope>,
                    CustomerOfferDecisionActionRecord>();
        private readonly Dictionary<StableId<CustomerVisitIdScope>,
            CustomerOfferDecisionActionRecord> _actionsByVisit =
                new Dictionary<StableId<CustomerVisitIdScope>,
                    CustomerOfferDecisionActionRecord>();

        private CustomerOfferDecisionActionAuthority(
            ShelfOfferAuthority offers,
            RetailBasketAuthority baskets,
            CustomerVisitAuthority visits)
        {
            _offers = offers;
            _baskets = baskets;
            _visits = visits;
        }

        public long Revision { get; private set; }

        public int Count => _actions.Count;

        public static OperationResult<CustomerOfferDecisionActionAuthority> Create(
            ShelfOfferAuthority offers,
            RetailBasketAuthority baskets,
            CustomerVisitAuthority visits)
        {
            return offers == null ||
                   baskets == null ||
                   visits == null ||
                   !ReferenceEquals(offers, baskets.OfferAuthority)
                ? OperationResult<CustomerOfferDecisionActionAuthority>.Fail(
                    CustomerOfferDecisionActionFailures.InputInvalid)
                : OperationResult<CustomerOfferDecisionActionAuthority>.Success(
                    new CustomerOfferDecisionActionAuthority(offers, baskets, visits));
        }

        public OperationResult ApplyBuy(
            StableId<CustomerOfferDecisionActionIdScope> actionId,
            CustomerRetailIdentityBinding customerBinding,
            CustomerOfferDecision sourceDecision,
            StableId<RetailBasketLineIdScope> lineId,
            StableId<RetailBasketIdScope> basketId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId,
            SimulationTimestamp appliedAt)
        {
            if (actionId.IsEmpty)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.InputInvalid);
            }

            if (_actions.TryGetValue(
                actionId,
                out CustomerOfferDecisionActionRecord existingAction))
            {
                return existingAction.Matches(
                    customerBinding,
                    sourceDecision,
                    lineId,
                    basketId,
                    itemId,
                    reservationId,
                    claimId,
                    appliedAt)
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        CustomerOfferDecisionActionFailures.ActionIdentityConflict);
            }

            if (!HasValidInput(
                customerBinding,
                sourceDecision,
                lineId,
                basketId,
                itemId,
                reservationId,
                claimId))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.InputInvalid);
            }

            if (sourceDecision.DecisionKind != CustomerOfferDecisionKind.Buy)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.KindNotBuy);
            }

            if (sourceDecision.CustomerId != customerBinding.ActorCustomerId)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.CustomerBindingMismatch);
            }

            if (_actionsByVisit.ContainsKey(sourceDecision.VisitId))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.VisitAlreadyActioned);
            }

            if (!_visits.TryGetVisit(
                sourceDecision.VisitId,
                out CustomerVisitRecord currentVisit))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.DecisionStale);
            }

            if (currentVisit.Intent.CustomerId != customerBinding.ActorCustomerId)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.CustomerBindingMismatch);
            }

            if (!_offers.TryGetOffer(
                sourceDecision.OfferId,
                out ShelfOfferRecord currentOffer))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.DecisionStale);
            }

            OperationResult<CustomerOfferDecision> currentDecision =
                CustomerOfferDecisionEvaluator.Evaluate(
                    currentVisit,
                    currentOffer,
                    sourceDecision.MaximumAcceptedPrice);
            if (currentDecision.IsFailure ||
                !currentDecision.Value.Equals(sourceDecision) ||
                currentDecision.Value.DecisionKind != CustomerOfferDecisionKind.Buy)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.DecisionStale);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.RevisionOverflow);
            }

            OperationResult<RetailBasketReservationPlan> basketPlan =
                _baskets.PrepareActionOwnedSerializedOfferReservation(
                    lineId,
                    basketId,
                    customerBinding.RetailCustomerId,
                    sourceDecision.OfferId,
                    itemId,
                    reservationId,
                    claimId,
                    actionId);
            if (basketPlan.IsFailure)
            {
                return OperationResult.Fail(basketPlan.Error);
            }

            OperationResult<CustomerVisitCheckoutNavigationPlan> visitPlan =
                _visits.PrepareCheckoutNavigation(
                    sourceDecision.VisitId,
                    appliedAt);
            if (visitPlan.IsFailure)
            {
                return OperationResult.Fail(visitPlan.Error);
            }

            OperationResult basketCommit =
                _baskets.CommitPreparedSerializedOfferReservation(basketPlan.Value);
            if (basketCommit.IsFailure)
            {
                return basketCommit;
            }

            OperationResult visitCommit =
                _visits.CommitPreparedCheckoutNavigation(visitPlan.Value);
            if (visitCommit.IsFailure)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.InvariantViolation);
            }

            var record = new CustomerOfferDecisionActionRecord(
                actionId,
                customerBinding,
                sourceDecision,
                lineId,
                basketId,
                itemId,
                reservationId,
                claimId,
                appliedAt);
            _actions.Add(actionId, record);
            _actionsByVisit.Add(sourceDecision.VisitId, record);
            Revision++;
            return OperationResult.Success();
        }

        public OperationResult ApplyLeave(
            StableId<CustomerOfferDecisionActionIdScope> actionId,
            CustomerRetailIdentityBinding customerBinding,
            CustomerOfferDecision sourceDecision,
            SimulationTimestamp appliedAt)
        {
            if (actionId.IsEmpty)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.InputInvalid);
            }

            if (_actions.TryGetValue(
                actionId,
                out CustomerOfferDecisionActionRecord existingAction))
            {
                return existingAction.MatchesLeave(
                    customerBinding,
                    sourceDecision,
                    appliedAt)
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        CustomerOfferDecisionActionFailures.ActionIdentityConflict);
            }

            if (!HasValidCommonInput(customerBinding, sourceDecision))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.InputInvalid);
            }

            if (sourceDecision.DecisionKind != CustomerOfferDecisionKind.Leave)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.KindNotLeave);
            }

            if (sourceDecision.CustomerId != customerBinding.ActorCustomerId)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.CustomerBindingMismatch);
            }

            if (_actionsByVisit.ContainsKey(sourceDecision.VisitId))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.VisitAlreadyActioned);
            }

            if (!_visits.TryGetVisit(
                sourceDecision.VisitId,
                out CustomerVisitRecord currentVisit))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.DecisionStale);
            }

            if (currentVisit.Intent.CustomerId != customerBinding.ActorCustomerId)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.CustomerBindingMismatch);
            }

            if (!_offers.TryGetOffer(
                sourceDecision.OfferId,
                out ShelfOfferRecord currentOffer))
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.DecisionStale);
            }

            OperationResult<CustomerOfferDecision> currentDecision =
                CustomerOfferDecisionEvaluator.Evaluate(
                    currentVisit,
                    currentOffer,
                    sourceDecision.MaximumAcceptedPrice);
            if (currentDecision.IsFailure ||
                !currentDecision.Value.Equals(sourceDecision) ||
                currentDecision.Value.DecisionKind != CustomerOfferDecisionKind.Leave)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.DecisionStale);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.RevisionOverflow);
            }

            OperationResult<CustomerVisitOfferDeclinedExitPlan> visitPlan =
                _visits.PrepareOfferDeclinedExit(
                    sourceDecision.VisitId,
                    appliedAt);
            if (visitPlan.IsFailure)
            {
                return OperationResult.Fail(visitPlan.Error);
            }

            OperationResult visitCommit =
                _visits.CommitPreparedOfferDeclinedExit(visitPlan.Value);
            if (visitCommit.IsFailure)
            {
                return visitCommit;
            }

            var record = new CustomerOfferDecisionActionRecord(
                actionId,
                customerBinding,
                sourceDecision,
                default,
                default,
                default,
                default,
                default,
                appliedAt);
            _actions.Add(actionId, record);
            _actionsByVisit.Add(sourceDecision.VisitId, record);
            Revision++;
            return OperationResult.Success();
        }

        public bool TryGetAction(
            StableId<CustomerOfferDecisionActionIdScope> actionId,
            out CustomerOfferDecisionActionRecord action)
        {
            return _actions.TryGetValue(actionId, out action);
        }

        public bool TryGetActionForVisit(
            StableId<CustomerVisitIdScope> visitId,
            out CustomerOfferDecisionActionRecord action)
        {
            return _actionsByVisit.TryGetValue(visitId, out action);
        }

        public OperationResult ValidateInvariants()
        {
            if (_actions.Count != _actionsByVisit.Count)
            {
                return OperationResult.Fail(
                    CustomerOfferDecisionActionFailures.InvariantViolation);
            }

            foreach (KeyValuePair<StableId<CustomerOfferDecisionActionIdScope>,
                CustomerOfferDecisionActionRecord> entry in _actions)
            {
                CustomerOfferDecisionActionRecord action = entry.Value;
                if (action == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != action.Id ||
                    !HasValidActionPayload(action) ||
                    action.SourceDecision.CustomerId !=
                        action.CustomerBinding.ActorCustomerId ||
                    !IsStrictlyAfter(
                        action.AppliedAt,
                        action.SourceDecision.VisitLastUpdatedAt) ||
                    !_actionsByVisit.TryGetValue(
                        action.SourceDecision.VisitId,
                        out CustomerOfferDecisionActionRecord byVisit) ||
                    !ReferenceEquals(action, byVisit))
                {
                    return OperationResult.Fail(
                        CustomerOfferDecisionActionFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private static bool HasValidActionPayload(CustomerOfferDecisionActionRecord action)
        {
            if (!HasValidCommonInput(action.CustomerBinding, action.SourceDecision))
            {
                return false;
            }

            if (action.SourceDecision.DecisionKind == CustomerOfferDecisionKind.Buy)
            {
                return HasValidInput(
                    action.CustomerBinding,
                    action.SourceDecision,
                    action.LineId,
                    action.BasketId,
                    action.ItemId,
                    action.ReservationId,
                    action.ClaimId);
            }

            return action.SourceDecision.DecisionKind == CustomerOfferDecisionKind.Leave &&
                   action.LineId.IsEmpty &&
                   action.BasketId.IsEmpty &&
                   action.ItemId.IsEmpty &&
                   action.ReservationId.IsEmpty &&
                   action.ClaimId.IsEmpty;
        }

        private static bool HasValidInput(
            CustomerRetailIdentityBinding customerBinding,
            CustomerOfferDecision sourceDecision,
            StableId<RetailBasketLineIdScope> lineId,
            StableId<RetailBasketIdScope> basketId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId)
        {
            return HasValidCommonInput(customerBinding, sourceDecision) &&
                   !lineId.IsEmpty &&
                   !basketId.IsEmpty &&
                   !itemId.IsEmpty &&
                   !reservationId.IsEmpty &&
                   !claimId.IsEmpty;
        }

        private static bool HasValidCommonInput(
            CustomerRetailIdentityBinding customerBinding,
            CustomerOfferDecision sourceDecision)
        {
            return customerBinding != null &&
                   !customerBinding.Id.IsEmpty &&
                   !customerBinding.ActorCustomerId.IsEmpty &&
                   !customerBinding.RetailCustomerId.IsEmpty &&
                   sourceDecision != null &&
                   !sourceDecision.CustomerId.IsEmpty &&
                   !sourceDecision.VisitId.IsEmpty &&
                   !sourceDecision.IntentId.IsEmpty &&
                   !sourceDecision.OfferId.IsEmpty;
        }

        private static bool IsStrictlyAfter(
            SimulationTimestamp candidate,
            SimulationTimestamp previous)
        {
            return candidate.IsAtOrAfter(previous) && candidate != previous;
        }
    }

    public static class CustomerOfferDecisionActionFailures
    {
        public static readonly Failure InputInvalid =
            Failure.FromCode("retail.offer-action.input-invalid");
        public static readonly Failure KindNotBuy =
            Failure.FromCode("retail.offer-action.kind-not-buy");
        public static readonly Failure KindNotLeave =
            Failure.FromCode("retail.offer-action.kind-not-leave");
        public static readonly Failure CustomerBindingMismatch =
            Failure.FromCode("retail.offer-action.customer-binding-mismatch");
        public static readonly Failure DecisionStale =
            Failure.FromCode("retail.offer-action.decision-stale");
        public static readonly Failure ActionIdentityConflict =
            Failure.FromCode("retail.offer-action.action-identity-conflict");
        public static readonly Failure VisitAlreadyActioned =
            Failure.FromCode("retail.offer-action.visit-already-actioned");
        public static readonly Failure RevisionOverflow =
            Failure.FromCode("retail.offer-action.revision-overflow");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("retail.offer-action.invariant");
    }
}
