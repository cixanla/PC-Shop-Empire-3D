using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PSE.Retail")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PCShopEmpire3D.EditModeTests")]

namespace PCShopEmpire3D.Actors
{
    /// <summary>
    /// Owns deterministic customer intent and visit lifecycle state. Navigation only reports
    /// arrival/failure; this authority never mutates stock, checkout, money, or Unity objects.
    /// </summary>
    public sealed class CustomerVisitAuthority
    {
        public const int RequiredRouteAttemptLimit = 2;

        private readonly ProductCatalog _catalog;
        private readonly SimulationDuration _stateTimeout;
        private readonly int _routeAttemptLimit;
        private readonly Dictionary<StableId<CustomerVisitIdScope>, CustomerVisitRecord> _visits =
            new Dictionary<StableId<CustomerVisitIdScope>, CustomerVisitRecord>();
        private CustomerConsultationAuthority _consultationAuthority;
        private SimulationTimestamp _lastObservedAt;
        private bool _hasObservedTime;

        private CustomerVisitAuthority(
            ProductCatalog catalog,
            SimulationDuration stateTimeout,
            int routeAttemptLimit)
        {
            _catalog = catalog;
            _stateTimeout = stateTimeout;
            _routeAttemptLimit = routeAttemptLimit;
        }

        public long Revision { get; private set; }

        public int Count => _visits.Count;

        public int ActiveCount
        {
            get
            {
                int count = 0;
                foreach (CustomerVisitRecord visit in _visits.Values)
                {
                    if (visit.IsActive)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public SimulationDuration StateTimeout => _stateTimeout;

        public int RouteAttemptLimit => _routeAttemptLimit;

        internal bool IsAtOrAfterObservedTime(SimulationTimestamp at)
        {
            return !_hasObservedTime || at.IsAtOrAfter(_lastObservedAt);
        }

        internal bool TryAttachConsultationAuthority(
            CustomerConsultationAuthority consultationAuthority)
        {
            if (consultationAuthority == null)
            {
                return false;
            }

            if (_consultationAuthority == null)
            {
                _consultationAuthority = consultationAuthority;
                return true;
            }

            return ReferenceEquals(_consultationAuthority, consultationAuthority);
        }

        internal bool OwnsConsultationAuthority(
            CustomerConsultationAuthority consultationAuthority)
        {
            return consultationAuthority != null &&
                   ReferenceEquals(_consultationAuthority, consultationAuthority);
        }

        public static OperationResult<CustomerVisitAuthority> Create(
            ProductCatalog catalog,
            SimulationDuration stateTimeout,
            int routeAttemptLimit)
        {
            if (catalog == null)
            {
                return OperationResult<CustomerVisitAuthority>.Fail(
                    CustomerVisitFailures.MissingCatalog);
            }

            if (stateTimeout.IsZero)
            {
                return OperationResult<CustomerVisitAuthority>.Fail(
                    CustomerVisitFailures.InvalidStateTimeout);
            }

            if (routeAttemptLimit != RequiredRouteAttemptLimit)
            {
                return OperationResult<CustomerVisitAuthority>.Fail(
                    CustomerVisitFailures.InvalidRouteAttemptLimit);
            }

            return OperationResult<CustomerVisitAuthority>.Success(
                new CustomerVisitAuthority(catalog, stateTimeout, routeAttemptLimit));
        }

        public OperationResult StartVisit(
            StableId<CustomerVisitIdScope> visitId,
            StableId<CustomerIntentIdScope> intentId,
            StableId<CustomerIdScope> customerId,
            StableId<ProductDefinitionIdScope> productId,
            CustomerNeedKind need,
            SimulationTimestamp startedAt)
        {
            Failure validation = ValidateStartIdentity(
                visitId,
                intentId,
                customerId,
                productId,
                need);
            if (!validation.IsNone)
            {
                return OperationResult.Fail(validation);
            }

            if (_visits.TryGetValue(visitId, out CustomerVisitRecord existing))
            {
                bool exact = existing.Intent.Id == intentId &&
                             existing.Intent.CustomerId == customerId &&
                             existing.Intent.ProductId == productId &&
                             existing.Intent.Need == need &&
                             existing.StartedAt == startedAt;
                return exact
                    ? OperationResult.Success()
                    : OperationResult.Fail(CustomerVisitFailures.VisitIdentityConflict);
            }

            if (IsBeforeObservedTime(startedAt))
            {
                return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
            }

            foreach (CustomerVisitRecord visit in _visits.Values)
            {
                if (visit.Intent.Id == intentId)
                {
                    return OperationResult.Fail(CustomerVisitFailures.DuplicateIntent);
                }

                if (visit.IsActive && visit.Intent.CustomerId == customerId)
                {
                    return OperationResult.Fail(CustomerVisitFailures.CustomerAlreadyVisiting);
                }
            }

            Failure deadlineFailure = TryCreateDeadline(startedAt, out SimulationTimestamp deadline);
            if (!deadlineFailure.IsNone)
            {
                return OperationResult.Fail(deadlineFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(CustomerVisitFailures.RevisionOverflow);
            }

            var intent = new CustomerIntentRecord(intentId, customerId, productId, need);
            _visits.Add(
                visitId,
                new CustomerVisitRecord(
                    visitId,
                    intent,
                    CustomerVisitState.Entering,
                    startedAt,
                    startedAt,
                    startedAt,
                    deadline,
                    0,
                    0,
                    default,
                    false,
                    CustomerVisitExitReason.None,
                    false,
                    CustomerVisitCommandReceipts.Empty));
            Revision++;
            ObserveTime(startedAt);
            return OperationResult.Success();
        }

        public OperationResult MarkBrowseArrival(
            StableId<CustomerVisitIdScope> visitId,
            SimulationTimestamp at)
        {
            return Transition(
                visitId,
                CustomerVisitState.Entering,
                CustomerVisitState.Browsing,
                CustomerVisitExitReason.None,
                CustomerVisitCommandKind.MarkBrowseArrival,
                at);
        }

        public OperationResult BeginCheckoutNavigation(
            StableId<CustomerVisitIdScope> visitId,
            SimulationTimestamp at)
        {
            OperationResult<CustomerVisitCheckoutNavigationPlan> prepared =
                PrepareCheckoutNavigation(visitId, at);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedCheckoutNavigation(prepared.Value);
        }

        public OperationResult<CustomerVisitCheckoutNavigationPlan>
            PrepareCheckoutNavigation(
                StableId<CustomerVisitIdScope> visitId,
                SimulationTimestamp at)
        {
            if (!_visits.TryGetValue(visitId, out CustomerVisitRecord visit))
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Fail(
                    CustomerVisitFailures.UnknownVisit);
            }

            if (visit.CommandReceipts.ContainsExact(
                CustomerVisitCommandKind.BeginCheckoutNavigation,
                at,
                CustomerVisitExitReason.None))
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Success(
                    new CustomerVisitCheckoutNavigationPlan(
                        this,
                        Revision,
                        visit,
                        null,
                        at,
                        _hasObservedTime,
                        _lastObservedAt,
                        true));
            }

            if (IsBeforeObservedTime(at))
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Fail(
                    CustomerVisitFailures.NonMonotonicTimestamp);
            }

            if (visit.State != CustomerVisitState.Browsing)
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Fail(
                    CustomerVisitFailures.InvalidTransition);
            }

            if (!IsStrictlyAfter(at, visit.LastUpdatedAt))
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Fail(
                    CustomerVisitFailures.NonMonotonicTimestamp);
            }

            Failure deadlineFailure = TryCreateDeadline(at, out SimulationTimestamp deadline);
            if (!deadlineFailure.IsNone)
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Fail(
                    deadlineFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Fail(
                    CustomerVisitFailures.RevisionOverflow);
            }

            var receipt = new CustomerVisitCommandReceipt(
                CustomerVisitCommandKind.BeginCheckoutNavigation,
                at,
                CustomerVisitExitReason.None,
                visit.State);
            if (!visit.CommandReceipts.TryAppend(
                receipt,
                out CustomerVisitCommandReceipts replacementReceipts))
            {
                return OperationResult<CustomerVisitCheckoutNavigationPlan>.Fail(
                    CustomerVisitFailures.InvariantViolation);
            }

            var replacement = new CustomerVisitRecord(
                visit.Id,
                visit.Intent,
                CustomerVisitState.NavigatingToCheckout,
                visit.StartedAt,
                at,
                at,
                deadline,
                0,
                visit.TotalRouteFailureCount,
                visit.LastRouteFailureAt,
                visit.HasRouteFailureReport,
                CustomerVisitExitReason.None,
                visit.RouteFallbackUsed,
                replacementReceipts);
            return OperationResult<CustomerVisitCheckoutNavigationPlan>.Success(
                new CustomerVisitCheckoutNavigationPlan(
                    this,
                    Revision,
                    visit,
                    replacement,
                    at,
                    _hasObservedTime,
                    _lastObservedAt,
                    false));
        }

        public OperationResult CommitPreparedCheckoutNavigation(
            CustomerVisitCheckoutNavigationPlan plan)
        {
            if (plan == null || !ReferenceEquals(plan.Owner, this))
            {
                return OperationResult.Fail(
                    CustomerVisitFailures.CheckoutNavigationPlanInvalid);
            }

            if (_visits.TryGetValue(plan.VisitId, out CustomerVisitRecord replayVisit) &&
                replayVisit.CommandReceipts.ContainsExact(
                    CustomerVisitCommandKind.BeginCheckoutNavigation,
                    plan.At,
                    CustomerVisitExitReason.None))
            {
                return OperationResult.Success();
            }

            if (plan.IsReplay)
            {
                return OperationResult.Fail(
                    CustomerVisitFailures.CheckoutNavigationPlanStale);
            }

            bool observedTimeMatches =
                _hasObservedTime == plan.ExpectedHasObservedTime &&
                (!_hasObservedTime || _lastObservedAt == plan.ExpectedLastObservedAt);
            if (Revision != plan.ExpectedRevision ||
                !observedTimeMatches ||
                !_visits.TryGetValue(plan.VisitId, out CustomerVisitRecord current) ||
                !ReferenceEquals(current, plan.ExpectedVisit) ||
                plan.ReplacementVisit == null)
            {
                return OperationResult.Fail(
                    CustomerVisitFailures.CheckoutNavigationPlanStale);
            }

            _visits[plan.VisitId] = plan.ReplacementVisit;
            Revision++;
            ObserveTime(plan.At);
            return OperationResult.Success();
        }

        internal OperationResult BeginOfferDeclinedExit(
            StableId<CustomerVisitIdScope> visitId,
            SimulationTimestamp at)
        {
            OperationResult<CustomerVisitOfferDeclinedExitPlan> prepared =
                PrepareOfferDeclinedExit(visitId, at);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedOfferDeclinedExit(prepared.Value);
        }

        internal OperationResult<CustomerVisitOfferDeclinedExitPlan>
            PrepareOfferDeclinedExit(
                StableId<CustomerVisitIdScope> visitId,
                SimulationTimestamp at)
        {
            if (!_visits.TryGetValue(visitId, out CustomerVisitRecord visit))
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Fail(
                    CustomerVisitFailures.UnknownVisit);
            }

            if (visit.CommandReceipts.ContainsExact(
                CustomerVisitCommandKind.BeginOfferDeclinedExit,
                at,
                CustomerVisitExitReason.OfferDeclined))
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Success(
                    new CustomerVisitOfferDeclinedExitPlan(
                        this,
                        Revision,
                        visit,
                        null,
                        at,
                        _hasObservedTime,
                        _lastObservedAt,
                        true));
            }

            if (IsBeforeObservedTime(at))
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Fail(
                    CustomerVisitFailures.NonMonotonicTimestamp);
            }

            if (visit.State != CustomerVisitState.Browsing)
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Fail(
                    CustomerVisitFailures.InvalidTransition);
            }

            if (!IsStrictlyAfter(at, visit.LastUpdatedAt))
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Fail(
                    CustomerVisitFailures.NonMonotonicTimestamp);
            }

            Failure deadlineFailure = TryCreateDeadline(at, out SimulationTimestamp deadline);
            if (!deadlineFailure.IsNone)
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Fail(
                    deadlineFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Fail(
                    CustomerVisitFailures.RevisionOverflow);
            }

            var receipt = new CustomerVisitCommandReceipt(
                CustomerVisitCommandKind.BeginOfferDeclinedExit,
                at,
                CustomerVisitExitReason.OfferDeclined,
                visit.State);
            if (!visit.CommandReceipts.TryAppend(
                receipt,
                out CustomerVisitCommandReceipts replacementReceipts))
            {
                return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Fail(
                    CustomerVisitFailures.InvariantViolation);
            }

            var replacement = new CustomerVisitRecord(
                visit.Id,
                visit.Intent,
                CustomerVisitState.Exiting,
                visit.StartedAt,
                at,
                at,
                deadline,
                0,
                visit.TotalRouteFailureCount,
                visit.LastRouteFailureAt,
                visit.HasRouteFailureReport,
                CustomerVisitExitReason.OfferDeclined,
                false,
                replacementReceipts);
            return OperationResult<CustomerVisitOfferDeclinedExitPlan>.Success(
                new CustomerVisitOfferDeclinedExitPlan(
                    this,
                    Revision,
                    visit,
                    replacement,
                    at,
                    _hasObservedTime,
                    _lastObservedAt,
                    false));
        }

        internal OperationResult CommitPreparedOfferDeclinedExit(
            CustomerVisitOfferDeclinedExitPlan plan)
        {
            if (plan == null || !ReferenceEquals(plan.Owner, this))
            {
                return OperationResult.Fail(
                    CustomerVisitFailures.OfferDeclinedExitPlanInvalid);
            }

            if (_visits.TryGetValue(plan.VisitId, out CustomerVisitRecord replayVisit) &&
                replayVisit.CommandReceipts.ContainsExact(
                    CustomerVisitCommandKind.BeginOfferDeclinedExit,
                    plan.At,
                    CustomerVisitExitReason.OfferDeclined))
            {
                return OperationResult.Success();
            }

            if (plan.IsReplay)
            {
                return OperationResult.Fail(
                    CustomerVisitFailures.OfferDeclinedExitPlanStale);
            }

            bool observedTimeMatches =
                _hasObservedTime == plan.ExpectedHasObservedTime &&
                (!_hasObservedTime || _lastObservedAt == plan.ExpectedLastObservedAt);
            if (Revision != plan.ExpectedRevision ||
                !observedTimeMatches ||
                !_visits.TryGetValue(plan.VisitId, out CustomerVisitRecord current) ||
                !ReferenceEquals(current, plan.ExpectedVisit) ||
                plan.ReplacementVisit == null)
            {
                return OperationResult.Fail(
                    CustomerVisitFailures.OfferDeclinedExitPlanStale);
            }

            _visits[plan.VisitId] = plan.ReplacementVisit;
            Revision++;
            ObserveTime(plan.At);
            return OperationResult.Success();
        }

        public OperationResult MarkCheckoutArrival(
            StableId<CustomerVisitIdScope> visitId,
            SimulationTimestamp at)
        {
            return Transition(
                visitId,
                CustomerVisitState.NavigatingToCheckout,
                CustomerVisitState.AwaitingCheckout,
                CustomerVisitExitReason.None,
                CustomerVisitCommandKind.MarkCheckoutArrival,
                at);
        }

        public OperationResult BeginExit(
            StableId<CustomerVisitIdScope> visitId,
            CustomerVisitExitReason reason,
            SimulationTimestamp at)
        {
            if (reason != CustomerVisitExitReason.Fulfilled)
            {
                return OperationResult.Fail(CustomerVisitFailures.InvalidExitReason);
            }

            if (!_visits.TryGetValue(visitId, out CustomerVisitRecord visit))
            {
                return OperationResult.Fail(CustomerVisitFailures.UnknownVisit);
            }

            if (visit.CommandReceipts.ContainsExact(
                CustomerVisitCommandKind.BeginExit,
                at,
                reason))
            {
                return OperationResult.Success();
            }

            if (IsBeforeObservedTime(at))
            {
                return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
            }

            if (visit.State != CustomerVisitState.AwaitingCheckout)
            {
                return OperationResult.Fail(CustomerVisitFailures.InvalidTransition);
            }

            return ReplaceState(
                visit,
                CustomerVisitState.Exiting,
                reason,
                at,
                false,
                CustomerVisitCommandKind.BeginExit,
                reason);
        }

        public OperationResult MarkExitArrival(
            StableId<CustomerVisitIdScope> visitId,
            SimulationTimestamp at)
        {
            return Transition(
                visitId,
                CustomerVisitState.Exiting,
                CustomerVisitState.Exited,
                CustomerVisitExitReason.None,
                CustomerVisitCommandKind.MarkExitArrival,
                at);
        }

        public OperationResult ReportRouteFailure(
            StableId<CustomerVisitIdScope> visitId,
            SimulationTimestamp at)
        {
            if (!_visits.TryGetValue(visitId, out CustomerVisitRecord visit))
            {
                return OperationResult.Fail(CustomerVisitFailures.UnknownVisit);
            }

            if (visit.CommandReceipts.ContainsExact(
                CustomerVisitCommandKind.ReportRouteFailure,
                at,
                CustomerVisitExitReason.None))
            {
                return OperationResult.Success();
            }

            if (IsBeforeObservedTime(at))
            {
                return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
            }

            if (!IsRouteState(visit.State))
            {
                return OperationResult.Fail(CustomerVisitFailures.RouteFailureNotAllowed);
            }

            if (!IsStrictlyAfter(at, visit.LastUpdatedAt))
            {
                return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
            }

            int nextRouteFailures = visit.RouteFailureCount + 1;
            int nextTotalFailures = visit.TotalRouteFailureCount + 1;
            if (nextRouteFailures < _routeAttemptLimit)
            {
                Failure retryDeadlineFailure = TryCreateDeadline(
                    at,
                    out SimulationTimestamp retryDeadline);
                if (!retryDeadlineFailure.IsNone)
                {
                    return OperationResult.Fail(retryDeadlineFailure);
                }

                return ReplaceVisit(
                    visit,
                    visit.State,
                    visit.StateEnteredAt,
                    at,
                    retryDeadline,
                    nextRouteFailures,
                    nextTotalFailures,
                    visit.ExitReason,
                    visit.RouteFallbackUsed,
                    true,
                    at,
                    CustomerVisitCommandKind.ReportRouteFailure,
                    CustomerVisitExitReason.None,
                    visit.State);
            }

            if (visit.State == CustomerVisitState.Exiting)
            {
                return ReplaceVisit(
                    visit,
                    CustomerVisitState.Exited,
                    at,
                    at,
                    at,
                    nextRouteFailures,
                    nextTotalFailures,
                    visit.ExitReason,
                    true,
                    true,
                    at,
                    CustomerVisitCommandKind.ReportRouteFailure,
                    CustomerVisitExitReason.None,
                    visit.State);
            }

            Failure deadlineFailure = TryCreateDeadline(at, out SimulationTimestamp exitDeadline);
            if (!deadlineFailure.IsNone)
            {
                return OperationResult.Fail(deadlineFailure);
            }

            return ReplaceVisit(
                visit,
                CustomerVisitState.Exiting,
                at,
                at,
                exitDeadline,
                0,
                nextTotalFailures,
                CustomerVisitExitReason.RouteUnavailable,
                false,
                true,
                at,
                CustomerVisitCommandKind.ReportRouteFailure,
                CustomerVisitExitReason.None,
                visit.State);
        }

        /// <summary>
        /// Applies all expired visit deadlines in stable ID order and advances authority revision once.
        /// Active-state timeout begins a safe exit; exit timeout produces a terminal despawn-safe result.
        /// </summary>
        public OperationResult AdvanceTime(SimulationTimestamp now)
        {
            if (IsBeforeObservedTime(now))
            {
                return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
            }

            var replacements = new List<CustomerVisitRecord>();
            IReadOnlyList<CustomerVisitRecord> ordered = GetVisits();
            foreach (CustomerVisitRecord visit in ordered)
            {
                if (!visit.IsActive)
                {
                    continue;
                }

                if (!now.IsAtOrAfter(visit.LastUpdatedAt))
                {
                    return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
                }

                if (!now.IsAtOrAfter(visit.StateDeadline))
                {
                    continue;
                }

                if (visit.State == CustomerVisitState.Exiting)
                {
                    replacements.Add(new CustomerVisitRecord(
                        visit.Id,
                        visit.Intent,
                        CustomerVisitState.Exited,
                        visit.StartedAt,
                        now,
                        now,
                        now,
                        visit.RouteFailureCount,
                        visit.TotalRouteFailureCount,
                        visit.LastRouteFailureAt,
                        visit.HasRouteFailureReport,
                        visit.ExitReason,
                        true,
                        visit.CommandReceipts));
                    continue;
                }

                Failure deadlineFailure = TryCreateDeadline(
                    now,
                    out SimulationTimestamp exitDeadline);
                if (!deadlineFailure.IsNone)
                {
                    return OperationResult.Fail(deadlineFailure);
                }

                replacements.Add(new CustomerVisitRecord(
                    visit.Id,
                    visit.Intent,
                    CustomerVisitState.Exiting,
                    visit.StartedAt,
                    now,
                    now,
                    exitDeadline,
                    0,
                    visit.TotalRouteFailureCount,
                    visit.LastRouteFailureAt,
                    visit.HasRouteFailureReport,
                    CustomerVisitExitReason.PatienceExpired,
                    false,
                    visit.CommandReceipts));
            }

            if (replacements.Count == 0)
            {
                ObserveTime(now);
                return OperationResult.Success();
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(CustomerVisitFailures.RevisionOverflow);
            }

            foreach (CustomerVisitRecord replacement in replacements)
            {
                _visits[replacement.Id] = replacement;
            }

            Revision++;
            ObserveTime(now);
            return OperationResult.Success();
        }

        public bool TryGetVisit(
            StableId<CustomerVisitIdScope> visitId,
            out CustomerVisitRecord visit)
        {
            return _visits.TryGetValue(visitId, out visit);
        }

        public IReadOnlyList<CustomerVisitRecord> GetVisits()
        {
            var visits = new List<CustomerVisitRecord>(_visits.Values);
            visits.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(visits.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            if (_visits.Count > 0 && !_hasObservedTime)
            {
                return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
            }

            var intentIds = new HashSet<StableId<CustomerIntentIdScope>>();
            var activeCustomers = new HashSet<StableId<CustomerIdScope>>();
            foreach (KeyValuePair<StableId<CustomerVisitIdScope>, CustomerVisitRecord> pair in _visits)
            {
                CustomerVisitRecord visit = pair.Value;
                if (visit == null ||
                    pair.Key != visit.Id ||
                    visit.Id.IsEmpty ||
                    visit.Intent == null ||
                    visit.Intent.Id.IsEmpty ||
                    visit.Intent.CustomerId.IsEmpty ||
                    visit.Intent.ProductId.IsEmpty ||
                    !IsValidNeed(visit.Intent.Need) ||
                    !_catalog.TryGet(visit.Intent.ProductId, out _) ||
                    !IsValidState(visit.State) ||
                    !visit.StateEnteredAt.IsAtOrAfter(visit.StartedAt) ||
                    !visit.LastUpdatedAt.IsAtOrAfter(visit.StateEnteredAt) ||
                    !visit.StateDeadline.IsAtOrAfter(visit.StateEnteredAt) ||
                    (_hasObservedTime && !_lastObservedAt.IsAtOrAfter(visit.LastUpdatedAt)) ||
                    visit.RouteFailureCount < 0 ||
                    visit.RouteFailureCount > _routeAttemptLimit ||
                    visit.TotalRouteFailureCount < visit.RouteFailureCount ||
                    (visit.HasRouteFailureReport &&
                     (!visit.LastRouteFailureAt.IsAtOrAfter(visit.StartedAt) ||
                      !visit.LastUpdatedAt.IsAtOrAfter(visit.LastRouteFailureAt))) ||
                    !intentIds.Add(visit.Intent.Id))
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (!ValidateCommandReceipts(visit).IsNone)
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (visit.State == CustomerVisitState.Exited)
                {
                    if (visit.StateEnteredAt != visit.LastUpdatedAt ||
                        visit.StateDeadline != visit.LastUpdatedAt)
                    {
                        return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                    }
                }
                else if (!IsStrictlyAfter(visit.StateDeadline, visit.LastUpdatedAt))
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (visit.HasRouteFailureReport)
                {
                    if (visit.TotalRouteFailureCount <= 0)
                    {
                        return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                    }
                }
                else if (visit.RouteFailureCount != 0 ||
                         visit.TotalRouteFailureCount != 0 ||
                         visit.LastRouteFailureAt != default)
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                bool hasExitReason = visit.ExitReason != CustomerVisitExitReason.None;
                if ((visit.State == CustomerVisitState.Exiting ||
                     visit.State == CustomerVisitState.Exited) != hasExitReason)
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (hasExitReason && !IsValidExitReason(visit.ExitReason))
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (!IsRouteState(visit.State) &&
                    visit.State != CustomerVisitState.Exited &&
                    visit.RouteFailureCount != 0)
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (visit.IsActive && IsRouteState(visit.State) &&
                    visit.RouteFailureCount >= _routeAttemptLimit)
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (visit.RouteFallbackUsed && visit.State != CustomerVisitState.Exited)
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                if (visit.IsActive && !activeCustomers.Add(visit.Intent.CustomerId))
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private OperationResult Transition(
            StableId<CustomerVisitIdScope> visitId,
            CustomerVisitState expected,
            CustomerVisitState target,
            CustomerVisitExitReason reason,
            CustomerVisitCommandKind commandKind,
            SimulationTimestamp at)
        {
            if (!_visits.TryGetValue(visitId, out CustomerVisitRecord visit))
            {
                return OperationResult.Fail(CustomerVisitFailures.UnknownVisit);
            }

            CustomerVisitExitReason targetReason = target == CustomerVisitState.Exited
                ? visit.ExitReason
                : reason;
            if (visit.CommandReceipts.ContainsExact(
                commandKind,
                at,
                CustomerVisitExitReason.None))
            {
                return OperationResult.Success();
            }

            if (IsBeforeObservedTime(at))
            {
                return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
            }

            if (visit.State != expected)
            {
                return OperationResult.Fail(CustomerVisitFailures.InvalidTransition);
            }

            return ReplaceState(
                visit,
                target,
                targetReason,
                at,
                visit.RouteFallbackUsed,
                commandKind,
                CustomerVisitExitReason.None);
        }

        private OperationResult ReplaceState(
            CustomerVisitRecord visit,
            CustomerVisitState state,
            CustomerVisitExitReason reason,
            SimulationTimestamp at,
            bool routeFallbackUsed,
            CustomerVisitCommandKind commandKind,
            CustomerVisitExitReason commandExitReason)
        {
            if (!IsStrictlyAfter(at, visit.LastUpdatedAt))
            {
                return OperationResult.Fail(CustomerVisitFailures.NonMonotonicTimestamp);
            }

            SimulationTimestamp deadline = at;
            if (state != CustomerVisitState.Exited)
            {
                Failure deadlineFailure = TryCreateDeadline(at, out deadline);
                if (!deadlineFailure.IsNone)
                {
                    return OperationResult.Fail(deadlineFailure);
                }
            }

            return ReplaceVisit(
                visit,
                state,
                at,
                at,
                deadline,
                0,
                visit.TotalRouteFailureCount,
                reason,
                routeFallbackUsed,
                false,
                default,
                commandKind,
                commandExitReason,
                visit.State);
        }

        private OperationResult ReplaceVisit(
            CustomerVisitRecord visit,
            CustomerVisitState state,
            SimulationTimestamp stateEnteredAt,
            SimulationTimestamp lastUpdatedAt,
            SimulationTimestamp stateDeadline,
            int routeFailureCount,
            int totalRouteFailureCount,
            CustomerVisitExitReason exitReason,
            bool routeFallbackUsed,
            bool updateRouteFailure = false,
            SimulationTimestamp routeFailureAt = default,
            CustomerVisitCommandKind? commandKind = null,
            CustomerVisitExitReason commandExitReason = CustomerVisitExitReason.None,
            CustomerVisitState commandAcceptedFromState = default)
        {
            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(CustomerVisitFailures.RevisionOverflow);
            }

            CustomerVisitCommandReceipts commandReceipts = visit.CommandReceipts;
            if (commandKind.HasValue)
            {
                var receipt = new CustomerVisitCommandReceipt(
                    commandKind.Value,
                    lastUpdatedAt,
                    commandExitReason,
                    commandAcceptedFromState);
                if (!commandReceipts.TryAppend(
                    receipt,
                    out CustomerVisitCommandReceipts replacementReceipts))
                {
                    return OperationResult.Fail(CustomerVisitFailures.InvariantViolation);
                }

                commandReceipts = replacementReceipts;
            }

            _visits[visit.Id] = new CustomerVisitRecord(
                visit.Id,
                visit.Intent,
                state,
                visit.StartedAt,
                stateEnteredAt,
                lastUpdatedAt,
                stateDeadline,
                routeFailureCount,
                totalRouteFailureCount,
                updateRouteFailure ? routeFailureAt : visit.LastRouteFailureAt,
                updateRouteFailure || visit.HasRouteFailureReport,
                exitReason,
                routeFallbackUsed,
                commandReceipts);
            Revision++;
            ObserveTime(lastUpdatedAt);
            return OperationResult.Success();
        }

        private bool IsBeforeObservedTime(SimulationTimestamp at)
        {
            return _hasObservedTime && !at.IsAtOrAfter(_lastObservedAt);
        }

        private void ObserveTime(SimulationTimestamp at)
        {
            _lastObservedAt = at;
            _hasObservedTime = true;
        }

        private Failure ValidateCommandReceipts(CustomerVisitRecord visit)
        {
            CustomerVisitCommandReceipts receipts = visit.CommandReceipts;
            if (receipts == null || receipts.Count > CustomerVisitCommandReceipts.MaximumCount)
            {
                return CustomerVisitFailures.InvariantViolation;
            }

            bool hasBrowseArrival = false;
            bool hasCheckoutNavigation = false;
            bool hasCheckoutArrival = false;
            bool hasBeginExit = false;
            bool hasOfferDeclinedExit = false;
            bool hasExitArrival = false;
            bool hasExitingActivity = false;
            bool hasPrevious = false;
            SimulationTimestamp previousAt = default;
            SimulationTimestamp lastRouteFailureAt = default;
            int routeFailureCount = 0;
            int currentStateRouteFailureCount = 0;
            int enteringRouteFailures = 0;
            int checkoutRouteFailures = 0;
            int exitRouteFailures = 0;

            for (int index = 0; index < receipts.Count; index++)
            {
                CustomerVisitCommandReceipt receipt = receipts[index];
                if (!receipt.At.IsAtOrAfter(visit.StartedAt) ||
                    !visit.LastUpdatedAt.IsAtOrAfter(receipt.At) ||
                    (hasPrevious && !IsStrictlyAfter(receipt.At, previousAt)))
                {
                    return CustomerVisitFailures.InvariantViolation;
                }

                switch (receipt.Kind)
                {
                    case CustomerVisitCommandKind.MarkBrowseArrival:
                        if (hasBrowseArrival ||
                            hasExitingActivity ||
                            enteringRouteFailures >= _routeAttemptLimit ||
                            receipt.ExitReason != CustomerVisitExitReason.None ||
                            receipt.AcceptedFromState != CustomerVisitState.Entering)
                        {
                            return CustomerVisitFailures.InvariantViolation;
                        }

                        hasBrowseArrival = true;
                        break;
                    case CustomerVisitCommandKind.BeginCheckoutNavigation:
                        if (hasCheckoutNavigation || !hasBrowseArrival || hasExitingActivity ||
                            receipt.ExitReason != CustomerVisitExitReason.None ||
                            receipt.AcceptedFromState != CustomerVisitState.Browsing)
                        {
                            return CustomerVisitFailures.InvariantViolation;
                        }

                        hasCheckoutNavigation = true;
                        break;
                    case CustomerVisitCommandKind.MarkCheckoutArrival:
                        if (hasCheckoutArrival || !hasCheckoutNavigation ||
                            hasExitingActivity ||
                            checkoutRouteFailures >= _routeAttemptLimit ||
                            receipt.ExitReason != CustomerVisitExitReason.None ||
                            receipt.AcceptedFromState != CustomerVisitState.NavigatingToCheckout)
                        {
                            return CustomerVisitFailures.InvariantViolation;
                        }

                        hasCheckoutArrival = true;
                        break;
                    case CustomerVisitCommandKind.BeginExit:
                        if (hasBeginExit || !hasCheckoutArrival ||
                            hasExitingActivity ||
                            receipt.ExitReason != CustomerVisitExitReason.Fulfilled ||
                            receipt.AcceptedFromState != CustomerVisitState.AwaitingCheckout)
                        {
                            return CustomerVisitFailures.InvariantViolation;
                        }

                        hasBeginExit = true;
                        hasExitingActivity = true;
                        break;
                    case CustomerVisitCommandKind.BeginOfferDeclinedExit:
                        if (hasOfferDeclinedExit || hasCheckoutNavigation ||
                            !hasBrowseArrival || hasExitingActivity ||
                            receipt.ExitReason != CustomerVisitExitReason.OfferDeclined ||
                            receipt.AcceptedFromState != CustomerVisitState.Browsing)
                        {
                            return CustomerVisitFailures.InvariantViolation;
                        }

                        hasOfferDeclinedExit = true;
                        hasExitingActivity = true;
                        break;
                    case CustomerVisitCommandKind.MarkExitArrival:
                        if (hasExitArrival || exitRouteFailures >= _routeAttemptLimit ||
                            (!hasBeginExit && !hasOfferDeclinedExit &&
                             enteringRouteFailures < _routeAttemptLimit &&
                             checkoutRouteFailures < _routeAttemptLimit &&
                             visit.ExitReason != CustomerVisitExitReason.PatienceExpired) ||
                            receipt.ExitReason != CustomerVisitExitReason.None ||
                            receipt.AcceptedFromState != CustomerVisitState.Exiting)
                        {
                            return CustomerVisitFailures.InvariantViolation;
                        }

                        hasExitArrival = true;
                        hasExitingActivity = true;
                        break;
                    case CustomerVisitCommandKind.ReportRouteFailure:
                        if (receipt.ExitReason != CustomerVisitExitReason.None ||
                            !IsRouteState(receipt.AcceptedFromState))
                        {
                            return CustomerVisitFailures.InvariantViolation;
                        }

                        routeFailureCount++;
                        lastRouteFailureAt = receipt.At;
                        if (receipt.AcceptedFromState == visit.State &&
                            receipt.At.IsAtOrAfter(visit.StateEnteredAt))
                        {
                            currentStateRouteFailureCount++;
                        }

                        if (receipt.AcceptedFromState == CustomerVisitState.Entering)
                        {
                            if (hasBrowseArrival || hasExitingActivity ||
                                enteringRouteFailures >= _routeAttemptLimit)
                            {
                                return CustomerVisitFailures.InvariantViolation;
                            }

                            enteringRouteFailures++;
                        }
                        else if (receipt.AcceptedFromState == CustomerVisitState.NavigatingToCheckout)
                        {
                            if (!hasCheckoutNavigation || hasCheckoutArrival || hasExitingActivity ||
                                checkoutRouteFailures >= _routeAttemptLimit)
                            {
                                return CustomerVisitFailures.InvariantViolation;
                            }

                            checkoutRouteFailures++;
                        }
                        else
                        {
                            if (hasExitArrival || exitRouteFailures >= _routeAttemptLimit ||
                                (!hasBeginExit && !hasOfferDeclinedExit &&
                                 enteringRouteFailures < _routeAttemptLimit &&
                                 checkoutRouteFailures < _routeAttemptLimit &&
                                 visit.ExitReason != CustomerVisitExitReason.PatienceExpired))
                            {
                                return CustomerVisitFailures.InvariantViolation;
                            }

                            exitRouteFailures++;
                            hasExitingActivity = true;
                        }

                        break;
                    default:
                        return CustomerVisitFailures.InvariantViolation;
                }

                previousAt = receipt.At;
                hasPrevious = true;
            }

            if ((hasBeginExit && hasOfferDeclinedExit) ||
                (hasBeginExit && visit.ExitReason != CustomerVisitExitReason.Fulfilled) ||
                (visit.ExitReason == CustomerVisitExitReason.Fulfilled && !hasBeginExit) ||
                (hasOfferDeclinedExit &&
                 visit.ExitReason != CustomerVisitExitReason.OfferDeclined) ||
                (visit.ExitReason == CustomerVisitExitReason.OfferDeclined &&
                 !hasOfferDeclinedExit))
            {
                return CustomerVisitFailures.InvariantViolation;
            }

            if (hasExitArrival &&
                (visit.State != CustomerVisitState.Exited ||
                 visit.RouteFallbackUsed ||
                 receipts[receipts.Count - 1].Kind != CustomerVisitCommandKind.MarkExitArrival ||
                 receipts[receipts.Count - 1].At != visit.StateEnteredAt))
            {
                return CustomerVisitFailures.InvariantViolation;
            }

            if (routeFailureCount != visit.TotalRouteFailureCount ||
                (routeFailureCount == 0 && visit.HasRouteFailureReport) ||
                (routeFailureCount > 0 &&
                 (!visit.HasRouteFailureReport ||
                  lastRouteFailureAt != visit.LastRouteFailureAt)))
            {
                return CustomerVisitFailures.InvariantViolation;
            }

            if (visit.IsActive && IsRouteState(visit.State) &&
                currentStateRouteFailureCount != visit.RouteFailureCount)
            {
                return CustomerVisitFailures.InvariantViolation;
            }

            if (visit.State == CustomerVisitState.Exited &&
                !visit.RouteFallbackUsed && !hasExitArrival)
            {
                return CustomerVisitFailures.InvariantViolation;
            }

            return Failure.None;
        }

        private Failure ValidateStartIdentity(
            StableId<CustomerVisitIdScope> visitId,
            StableId<CustomerIntentIdScope> intentId,
            StableId<CustomerIdScope> customerId,
            StableId<ProductDefinitionIdScope> productId,
            CustomerNeedKind need)
        {
            if (visitId.IsEmpty)
            {
                return CustomerVisitFailures.InvalidVisitId;
            }

            if (intentId.IsEmpty)
            {
                return CustomerVisitFailures.InvalidIntentId;
            }

            if (customerId.IsEmpty)
            {
                return CustomerVisitFailures.InvalidCustomerId;
            }

            if (productId.IsEmpty)
            {
                return CustomerVisitFailures.InvalidProductId;
            }

            if (!_catalog.TryGet(productId, out _))
            {
                return CustomerVisitFailures.UnknownProduct;
            }

            return IsValidNeed(need)
                ? Failure.None
                : CustomerVisitFailures.InvalidNeed;
        }

        private Failure TryCreateDeadline(
            SimulationTimestamp start,
            out SimulationTimestamp deadline)
        {
            if (start.ElapsedMilliseconds > long.MaxValue - _stateTimeout.Milliseconds)
            {
                deadline = default;
                return CustomerVisitFailures.TimestampOverflow;
            }

            deadline = SimulationTimestamp.Create(
                start.Tick,
                start.ElapsedMilliseconds + _stateTimeout.Milliseconds);
            return Failure.None;
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

        private static bool IsValidExitReason(CustomerVisitExitReason reason)
        {
            return reason == CustomerVisitExitReason.Fulfilled ||
                   reason == CustomerVisitExitReason.PatienceExpired ||
                   reason == CustomerVisitExitReason.RouteUnavailable ||
                   reason == CustomerVisitExitReason.OfferDeclined;
        }

        private static bool IsValidState(CustomerVisitState state)
        {
            return state >= CustomerVisitState.Entering && state <= CustomerVisitState.Exited;
        }
    }
}
