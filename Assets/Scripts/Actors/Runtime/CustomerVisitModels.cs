using System;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Actors
{
    public enum CustomerNeedKind
    {
        GraphicsUpgrade = 1
    }

    public enum CustomerVisitState
    {
        Entering = 1,
        Browsing = 2,
        NavigatingToCheckout = 3,
        AwaitingCheckout = 4,
        Exiting = 5,
        Exited = 6
    }

    public enum CustomerVisitExitReason
    {
        None = 0,
        Fulfilled = 1,
        PatienceExpired = 2,
        RouteUnavailable = 3
    }

    internal enum CustomerVisitCommandKind
    {
        MarkBrowseArrival = 1,
        BeginCheckoutNavigation = 2,
        MarkCheckoutArrival = 3,
        BeginExit = 4,
        MarkExitArrival = 5,
        ReportRouteFailure = 6
    }

    internal readonly struct CustomerVisitCommandReceipt
    {
        internal CustomerVisitCommandReceipt(
            CustomerVisitCommandKind kind,
            SimulationTimestamp at,
            CustomerVisitExitReason exitReason,
            CustomerVisitState acceptedFromState)
        {
            Kind = kind;
            At = at;
            ExitReason = exitReason;
            AcceptedFromState = acceptedFromState;
        }

        internal CustomerVisitCommandKind Kind { get; }

        internal SimulationTimestamp At { get; }

        internal CustomerVisitExitReason ExitReason { get; }

        internal CustomerVisitState AcceptedFromState { get; }

        internal bool HasExactKey(
            CustomerVisitCommandKind kind,
            SimulationTimestamp at,
            CustomerVisitExitReason exitReason)
        {
            return Kind == kind && At == at && ExitReason == exitReason;
        }
    }

    internal sealed class CustomerVisitCommandReceipts
    {
        internal const int MaximumCount = 8;

        private readonly CustomerVisitCommandReceipt[] _items;

        private CustomerVisitCommandReceipts(CustomerVisitCommandReceipt[] items)
        {
            _items = items;
        }

        internal static CustomerVisitCommandReceipts Empty { get; } =
            new CustomerVisitCommandReceipts(Array.Empty<CustomerVisitCommandReceipt>());

        internal int Count => _items.Length;

        internal CustomerVisitCommandReceipt this[int index] => _items[index];

        internal bool ContainsExact(
            CustomerVisitCommandKind kind,
            SimulationTimestamp at,
            CustomerVisitExitReason exitReason)
        {
            foreach (CustomerVisitCommandReceipt receipt in _items)
            {
                if (receipt.HasExactKey(kind, at, exitReason))
                {
                    return true;
                }
            }

            return false;
        }

        internal bool TryAppend(
            CustomerVisitCommandReceipt receipt,
            out CustomerVisitCommandReceipts replacement)
        {
            if (_items.Length >= MaximumCount ||
                ContainsExact(receipt.Kind, receipt.At, receipt.ExitReason))
            {
                replacement = this;
                return false;
            }

            var items = new CustomerVisitCommandReceipt[_items.Length + 1];
            Array.Copy(_items, items, _items.Length);
            items[_items.Length] = receipt;
            replacement = new CustomerVisitCommandReceipts(items);
            return true;
        }
    }

    public sealed class CustomerIntentRecord
    {
        internal CustomerIntentRecord(
            StableId<CustomerIntentIdScope> id,
            StableId<CustomerIdScope> customerId,
            StableId<ProductDefinitionIdScope> productId,
            CustomerNeedKind need)
        {
            Id = id;
            CustomerId = customerId;
            ProductId = productId;
            Need = need;
        }

        public StableId<CustomerIntentIdScope> Id { get; }

        public StableId<CustomerIdScope> CustomerId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public CustomerNeedKind Need { get; }
    }

    /// <summary>
    /// Immutable authoritative customer visit. World transforms and NavMeshAgent state are projections.
    /// </summary>
    public sealed class CustomerVisitRecord
    {
        internal CustomerVisitRecord(
            StableId<CustomerVisitIdScope> id,
            CustomerIntentRecord intent,
            CustomerVisitState state,
            SimulationTimestamp startedAt,
            SimulationTimestamp stateEnteredAt,
            SimulationTimestamp lastUpdatedAt,
            SimulationTimestamp stateDeadline,
            int routeFailureCount,
            int totalRouteFailureCount,
            SimulationTimestamp lastRouteFailureAt,
            bool hasRouteFailureReport,
            CustomerVisitExitReason exitReason,
            bool routeFallbackUsed,
            CustomerVisitCommandReceipts commandReceipts)
        {
            Id = id;
            Intent = intent;
            State = state;
            StartedAt = startedAt;
            StateEnteredAt = stateEnteredAt;
            LastUpdatedAt = lastUpdatedAt;
            StateDeadline = stateDeadline;
            RouteFailureCount = routeFailureCount;
            TotalRouteFailureCount = totalRouteFailureCount;
            LastRouteFailureAt = lastRouteFailureAt;
            HasRouteFailureReport = hasRouteFailureReport;
            ExitReason = exitReason;
            RouteFallbackUsed = routeFallbackUsed;
            CommandReceipts = commandReceipts;
        }

        public StableId<CustomerVisitIdScope> Id { get; }

        public CustomerIntentRecord Intent { get; }

        public CustomerVisitState State { get; }

        public SimulationTimestamp StartedAt { get; }

        public SimulationTimestamp StateEnteredAt { get; }

        public SimulationTimestamp LastUpdatedAt { get; }

        public SimulationTimestamp StateDeadline { get; }

        public int RouteFailureCount { get; }

        public int TotalRouteFailureCount { get; }

        public SimulationTimestamp LastRouteFailureAt { get; }

        public bool HasRouteFailureReport { get; }

        public CustomerVisitExitReason ExitReason { get; }

        public bool RouteFallbackUsed { get; }

        internal CustomerVisitCommandReceipts CommandReceipts { get; }

        public bool IsActive => State != CustomerVisitState.Exited;
    }

    public static class CustomerVisitFailures
    {
        public static readonly Failure MissingCatalog =
            Failure.FromCode("actors.customer-visit.missing-catalog");
        public static readonly Failure InvalidStateTimeout =
            Failure.FromCode("actors.customer-visit.invalid-state-timeout");
        public static readonly Failure InvalidRouteAttemptLimit =
            Failure.FromCode("actors.customer-visit.invalid-route-attempt-limit");
        public static readonly Failure InvalidVisitId =
            Failure.FromCode("actors.customer-visit.invalid-visit-id");
        public static readonly Failure InvalidIntentId =
            Failure.FromCode("actors.customer-visit.invalid-intent-id");
        public static readonly Failure InvalidCustomerId =
            Failure.FromCode("actors.customer-visit.invalid-customer-id");
        public static readonly Failure InvalidProductId =
            Failure.FromCode("actors.customer-visit.invalid-product-id");
        public static readonly Failure UnknownProduct =
            Failure.FromCode("actors.customer-visit.unknown-product");
        public static readonly Failure InvalidNeed =
            Failure.FromCode("actors.customer-visit.invalid-need");
        public static readonly Failure VisitIdentityConflict =
            Failure.FromCode("actors.customer-visit.identity-conflict");
        public static readonly Failure DuplicateIntent =
            Failure.FromCode("actors.customer-visit.duplicate-intent");
        public static readonly Failure CustomerAlreadyVisiting =
            Failure.FromCode("actors.customer-visit.customer-already-visiting");
        public static readonly Failure UnknownVisit =
            Failure.FromCode("actors.customer-visit.unknown-visit");
        public static readonly Failure InvalidTransition =
            Failure.FromCode("actors.customer-visit.invalid-transition");
        public static readonly Failure InvalidExitReason =
            Failure.FromCode("actors.customer-visit.invalid-exit-reason");
        public static readonly Failure RouteFailureNotAllowed =
            Failure.FromCode("actors.customer-visit.route-failure-not-allowed");
        public static readonly Failure NonMonotonicTimestamp =
            Failure.FromCode("actors.customer-visit.non-monotonic-timestamp");
        public static readonly Failure TimestampOverflow =
            Failure.FromCode("actors.customer-visit.timestamp-overflow");
        public static readonly Failure RevisionOverflow =
            Failure.FromCode("actors.customer-visit.revision-overflow");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("actors.customer-visit.invariant-violation");
    }
}
