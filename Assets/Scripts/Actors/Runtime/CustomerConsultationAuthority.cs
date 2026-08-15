using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Actors
{
    /// <summary>
    /// Immutable receipt proving that the player's consultation captured one exact Browsing
    /// visit snapshot. Later customer and retail transitions never rewrite this provenance.
    /// </summary>
    public sealed class CustomerConsultationRecord : IEquatable<CustomerConsultationRecord>
    {
        private readonly CustomerConsultationAuthority _owner;
        private readonly CustomerVisitRecord _sourceVisit;

        internal CustomerConsultationRecord(
            StableId<CustomerConsultationIdScope> id,
            StableId<CustomerIdScope> customerId,
            StableId<CustomerVisitIdScope> visitId,
            StableId<CustomerIntentIdScope> intentId,
            CustomerNeedKind need,
            StableId<ProductDefinitionIdScope> productId,
            CustomerVisitState visitState,
            SimulationTimestamp visitLastUpdatedAt,
            SimulationTimestamp recordedAt)
        {
            Id = id;
            CustomerId = customerId;
            VisitId = visitId;
            IntentId = intentId;
            Need = need;
            ProductId = productId;
            VisitState = visitState;
            VisitLastUpdatedAt = visitLastUpdatedAt;
            RecordedAt = recordedAt;
        }

        internal CustomerConsultationRecord(
            CustomerConsultationAuthority owner,
            StableId<CustomerConsultationIdScope> id,
            CustomerVisitRecord visit,
            SimulationTimestamp recordedAt)
            : this(
                id,
                visit.Intent.CustomerId,
                visit.Id,
                visit.Intent.Id,
                visit.Intent.Need,
                visit.Intent.ProductId,
                visit.State,
                visit.LastUpdatedAt,
                recordedAt)
        {
            _owner = owner;
            _sourceVisit = visit;
        }

        public StableId<CustomerConsultationIdScope> Id { get; }

        public StableId<CustomerIdScope> CustomerId { get; }

        public StableId<CustomerVisitIdScope> VisitId { get; }

        public StableId<CustomerIntentIdScope> IntentId { get; }

        public CustomerNeedKind Need { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public CustomerVisitState VisitState { get; }

        public SimulationTimestamp VisitLastUpdatedAt { get; }

        public SimulationTimestamp RecordedAt { get; }

        public bool Equals(CustomerConsultationRecord other)
        {
            return !ReferenceEquals(other, null) &&
                   Id == other.Id &&
                   CustomerId == other.CustomerId &&
                   VisitId == other.VisitId &&
                   IntentId == other.IntentId &&
                   Need == other.Need &&
                   ProductId == other.ProductId &&
                   VisitState == other.VisitState &&
                   VisitLastUpdatedAt == other.VisitLastUpdatedAt &&
                   RecordedAt == other.RecordedAt;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CustomerConsultationRecord);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Id.GetHashCode();
                hash = (hash * 397) ^ CustomerId.GetHashCode();
                hash = (hash * 397) ^ VisitId.GetHashCode();
                hash = (hash * 397) ^ IntentId.GetHashCode();
                hash = (hash * 397) ^ (int)Need;
                hash = (hash * 397) ^ ProductId.GetHashCode();
                hash = (hash * 397) ^ (int)VisitState;
                hash = (hash * 397) ^ VisitLastUpdatedAt.GetHashCode();
                return (hash * 397) ^ RecordedAt.GetHashCode();
            }
        }

        public static bool operator ==(
            CustomerConsultationRecord left,
            CustomerConsultationRecord right)
        {
            return ReferenceEquals(left, right) ||
                   (!ReferenceEquals(left, null) && left.Equals(right));
        }

        public static bool operator !=(
            CustomerConsultationRecord left,
            CustomerConsultationRecord right)
        {
            return !(left == right);
        }

        internal bool Matches(CustomerVisitRecord visit, SimulationTimestamp recordedAt)
        {
            return visit != null &&
                   visit.Intent != null &&
                   CustomerId == visit.Intent.CustomerId &&
                   VisitId == visit.Id &&
                   IntentId == visit.Intent.Id &&
                   Need == visit.Intent.Need &&
                   ProductId == visit.Intent.ProductId &&
                   VisitState == visit.State &&
                   VisitLastUpdatedAt == visit.LastUpdatedAt &&
                   RecordedAt == recordedAt;
        }

        internal bool IsOwnedFor(CustomerVisitRecord visit)
        {
            return IsOwnedBy(_owner) &&
                   ReferenceEquals(_sourceVisit, visit);
        }

        internal bool IsOwnedBy(CustomerConsultationAuthority owner)
        {
            return owner != null &&
                   ReferenceEquals(_owner, owner) &&
                   _sourceVisit != null &&
                   owner.Owns(this) &&
                   Matches(_sourceVisit, RecordedAt);
        }
    }

    /// <summary>
    /// Owns one immutable consultation receipt per customer visit. It has no Unity, world,
    /// inventory, checkout, or economy dependency; consumers use the receipt as provenance.
    /// </summary>
    public sealed class CustomerConsultationAuthority
    {
        private readonly CustomerVisitAuthority _visits;
        private readonly Dictionary<StableId<CustomerConsultationIdScope>,
            CustomerConsultationRecord> _consultations =
                new Dictionary<StableId<CustomerConsultationIdScope>,
                    CustomerConsultationRecord>();
        private readonly Dictionary<StableId<CustomerVisitIdScope>,
            CustomerConsultationRecord> _consultationsByVisit =
                new Dictionary<StableId<CustomerVisitIdScope>,
                    CustomerConsultationRecord>();

        private CustomerConsultationAuthority(CustomerVisitAuthority visits)
        {
            _visits = visits;
        }

        public long Revision { get; private set; }

        public int Count => _consultations.Count;

        public CustomerVisitAuthority VisitAuthority => _visits;

        public static OperationResult<CustomerConsultationAuthority> Create(
            CustomerVisitAuthority visits)
        {
            if (visits == null)
            {
                return OperationResult<CustomerConsultationAuthority>.Fail(
                    CustomerConsultationFailures.InputInvalid);
            }

            var authority = new CustomerConsultationAuthority(visits);
            return visits.TryAttachConsultationAuthority(authority)
                ? OperationResult<CustomerConsultationAuthority>.Success(authority)
                : OperationResult<CustomerConsultationAuthority>.Fail(
                    CustomerConsultationFailures.AuthorityAlreadyAttached);
        }

        public OperationResult RecordConsultation(
            StableId<CustomerConsultationIdScope> id,
            CustomerVisitRecord visit,
            SimulationTimestamp at)
        {
            if (id.IsEmpty)
            {
                return OperationResult.Fail(CustomerConsultationFailures.InputInvalid);
            }

            if (ValidateInvariants().IsFailure)
            {
                return OperationResult.Fail(CustomerConsultationFailures.InvariantViolation);
            }

            if (_consultations.TryGetValue(id, out CustomerConsultationRecord existing))
            {
                return existing.Matches(visit, at)
                    ? OperationResult.Success()
                    : OperationResult.Fail(CustomerConsultationFailures.IdentityConflict);
            }

            if (!HasValidVisitStructure(visit))
            {
                return OperationResult.Fail(CustomerConsultationFailures.InputInvalid);
            }

            if (!_visits.TryGetVisit(
                    visit.Id,
                    out CustomerVisitRecord currentVisit) ||
                !ReferenceEquals(currentVisit, visit))
            {
                return OperationResult.Fail(CustomerConsultationFailures.VisitStale);
            }

            if (_consultationsByVisit.ContainsKey(visit.Id))
            {
                return OperationResult.Fail(CustomerConsultationFailures.VisitAlreadyConsulted);
            }

            if (visit.State != CustomerVisitState.Browsing)
            {
                return OperationResult.Fail(CustomerConsultationFailures.VisitNotBrowsing);
            }

            if (!at.IsAtOrAfter(visit.LastUpdatedAt) ||
                !_visits.IsAtOrAfterObservedTime(at))
            {
                return OperationResult.Fail(CustomerConsultationFailures.NonMonotonicTimestamp);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(CustomerConsultationFailures.RevisionOverflow);
            }

            var record = new CustomerConsultationRecord(
                this,
                id,
                visit,
                at);
            _consultations.Add(id, record);
            _consultationsByVisit.Add(visit.Id, record);
            Revision++;
            return OperationResult.Success();
        }

        public bool TryGetConsultation(
            StableId<CustomerConsultationIdScope> id,
            out CustomerConsultationRecord consultation)
        {
            return _consultations.TryGetValue(id, out consultation);
        }

        public bool TryGetForVisit(
            StableId<CustomerVisitIdScope> visitId,
            out CustomerConsultationRecord consultation)
        {
            return _consultationsByVisit.TryGetValue(visitId, out consultation);
        }

        public bool Owns(CustomerConsultationRecord consultation)
        {
            return consultation != null &&
                   _consultations.TryGetValue(
                       consultation.Id,
                       out CustomerConsultationRecord owned) &&
                   ReferenceEquals(owned, consultation);
        }

        public IReadOnlyList<CustomerConsultationRecord> GetConsultations()
        {
            var ordered = new List<CustomerConsultationRecord>(_consultations.Values);
            ordered.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(ordered.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            if (_visits == null ||
                !_visits.OwnsConsultationAuthority(this) ||
                _visits.ValidateInvariants().IsFailure ||
                Revision < 0 ||
                _consultations.Count != _consultationsByVisit.Count)
            {
                return OperationResult.Fail(CustomerConsultationFailures.InvariantViolation);
            }

            foreach (KeyValuePair<StableId<CustomerConsultationIdScope>,
                CustomerConsultationRecord> entry in _consultations)
            {
                CustomerConsultationRecord consultation = entry.Value;
                if (consultation == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != consultation.Id ||
                    !HasValidRecord(consultation) ||
                    !consultation.IsOwnedBy(this) ||
                    !_visits.TryGetVisit(
                        consultation.VisitId,
                        out CustomerVisitRecord currentVisit) ||
                    !HasStableVisitIdentity(currentVisit, consultation) ||
                    !_consultationsByVisit.TryGetValue(
                        consultation.VisitId,
                        out CustomerConsultationRecord byVisit) ||
                    !ReferenceEquals(consultation, byVisit))
                {
                    return OperationResult.Fail(CustomerConsultationFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<StableId<CustomerVisitIdScope>,
                CustomerConsultationRecord> entry in _consultationsByVisit)
            {
                CustomerConsultationRecord consultation = entry.Value;
                if (consultation == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != consultation.VisitId ||
                    !_consultations.TryGetValue(
                        consultation.Id,
                        out CustomerConsultationRecord byId) ||
                    !ReferenceEquals(consultation, byId))
                {
                    return OperationResult.Fail(CustomerConsultationFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private static bool HasValidRecord(CustomerConsultationRecord consultation)
        {
            return !consultation.Id.IsEmpty &&
                   !consultation.CustomerId.IsEmpty &&
                   !consultation.VisitId.IsEmpty &&
                   !consultation.IntentId.IsEmpty &&
                   IsValidNeed(consultation.Need) &&
                   !consultation.ProductId.IsEmpty &&
                   consultation.VisitState == CustomerVisitState.Browsing &&
                   consultation.RecordedAt.IsAtOrAfter(
                       consultation.VisitLastUpdatedAt);
        }

        private static bool HasStableVisitIdentity(
            CustomerVisitRecord visit,
            CustomerConsultationRecord consultation)
        {
            return visit != null &&
                   visit.Intent != null &&
                   visit.Id == consultation.VisitId &&
                   visit.Intent.CustomerId == consultation.CustomerId &&
                   visit.Intent.Id == consultation.IntentId &&
                   visit.Intent.Need == consultation.Need &&
                   visit.Intent.ProductId == consultation.ProductId;
        }

        private static bool HasValidVisitStructure(CustomerVisitRecord visit)
        {
            if (visit == null ||
                visit.Id.IsEmpty ||
                visit.Intent == null ||
                visit.Intent.Id.IsEmpty ||
                visit.Intent.CustomerId.IsEmpty ||
                visit.Intent.ProductId.IsEmpty ||
                !IsValidNeed(visit.Intent.Need) ||
                !IsValidVisitState(visit.State) ||
                !visit.StateEnteredAt.IsAtOrAfter(visit.StartedAt) ||
                !visit.LastUpdatedAt.IsAtOrAfter(visit.StateEnteredAt) ||
                !visit.StateDeadline.IsAtOrAfter(visit.StateEnteredAt) ||
                visit.RouteFailureCount < 0 ||
                visit.RouteFailureCount > CustomerVisitAuthority.RequiredRouteAttemptLimit ||
                visit.TotalRouteFailureCount < visit.RouteFailureCount ||
                visit.CommandReceipts == null ||
                visit.CommandReceipts.Count > CustomerVisitCommandReceipts.MaximumCount)
            {
                return false;
            }

            if (visit.State == CustomerVisitState.Exited)
            {
                if (visit.StateEnteredAt != visit.LastUpdatedAt ||
                    visit.StateDeadline != visit.LastUpdatedAt)
                {
                    return false;
                }
            }
            else if (!IsStrictlyAfter(visit.StateDeadline, visit.LastUpdatedAt))
            {
                return false;
            }

            if (visit.HasRouteFailureReport)
            {
                if (visit.TotalRouteFailureCount <= 0 ||
                    !visit.LastRouteFailureAt.IsAtOrAfter(visit.StartedAt) ||
                    !visit.LastUpdatedAt.IsAtOrAfter(visit.LastRouteFailureAt))
                {
                    return false;
                }
            }
            else if (visit.RouteFailureCount != 0 ||
                     visit.TotalRouteFailureCount != 0 ||
                     visit.LastRouteFailureAt != default)
            {
                return false;
            }

            bool hasExitReason = visit.ExitReason != CustomerVisitExitReason.None;
            if ((visit.State == CustomerVisitState.Exiting ||
                 visit.State == CustomerVisitState.Exited) != hasExitReason ||
                (hasExitReason && !IsValidExitReason(visit.ExitReason)) ||
                (!IsRouteState(visit.State) &&
                 visit.State != CustomerVisitState.Exited &&
                 visit.RouteFailureCount != 0) ||
                (visit.IsActive &&
                 IsRouteState(visit.State) &&
                 visit.RouteFailureCount >= CustomerVisitAuthority.RequiredRouteAttemptLimit) ||
                (visit.RouteFallbackUsed && visit.State != CustomerVisitState.Exited))
            {
                return false;
            }

            return true;
        }

        private static bool IsStrictlyAfter(
            SimulationTimestamp candidate,
            SimulationTimestamp previous)
        {
            return candidate.IsAtOrAfter(previous) && candidate != previous;
        }

        private static bool IsRouteState(CustomerVisitState state)
        {
            return state == CustomerVisitState.Entering ||
                   state == CustomerVisitState.NavigatingToCheckout ||
                   state == CustomerVisitState.Exiting;
        }

        private static bool IsValidNeed(CustomerNeedKind need)
        {
            return need == CustomerNeedKind.GraphicsUpgrade;
        }

        private static bool IsValidVisitState(CustomerVisitState state)
        {
            return state == CustomerVisitState.Entering ||
                   state == CustomerVisitState.Browsing ||
                   state == CustomerVisitState.NavigatingToCheckout ||
                   state == CustomerVisitState.AwaitingCheckout ||
                   state == CustomerVisitState.Exiting ||
                   state == CustomerVisitState.Exited;
        }

        private static bool IsValidExitReason(CustomerVisitExitReason reason)
        {
            return reason == CustomerVisitExitReason.Fulfilled ||
                   reason == CustomerVisitExitReason.PatienceExpired ||
                   reason == CustomerVisitExitReason.RouteUnavailable ||
                   reason == CustomerVisitExitReason.OfferDeclined;
        }
    }

    public static class CustomerConsultationFailures
    {
        public static readonly Failure InputInvalid =
            Failure.FromCode("actors.customer-consultation.input-invalid");
        public static readonly Failure VisitNotBrowsing =
            Failure.FromCode("actors.customer-consultation.visit-not-browsing");
        public static readonly Failure VisitStale =
            Failure.FromCode("actors.customer-consultation.visit-stale");
        public static readonly Failure NonMonotonicTimestamp =
            Failure.FromCode("actors.customer-consultation.non-monotonic-timestamp");
        public static readonly Failure IdentityConflict =
            Failure.FromCode("actors.customer-consultation.identity-conflict");
        public static readonly Failure VisitAlreadyConsulted =
            Failure.FromCode("actors.customer-consultation.visit-already-consulted");
        public static readonly Failure AuthorityAlreadyAttached =
            Failure.FromCode("actors.customer-consultation.authority-already-attached");
        public static readonly Failure RevisionOverflow =
            Failure.FromCode("actors.customer-consultation.revision-overflow");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("actors.customer-consultation.invariant");
    }
}
