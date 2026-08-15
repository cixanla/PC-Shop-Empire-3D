using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Tests.EditMode.Actors
{
    public sealed class CustomerVisitAuthorityTests
    {
        private static readonly StableId<ProductDefinitionIdScope> ProductId =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.test-a60");
        private static readonly StableId<CustomerVisitIdScope> VisitId =
            StableId<CustomerVisitIdScope>.Parse("actors.visit.customer-001");
        private static readonly StableId<CustomerIntentIdScope> IntentId =
            StableId<CustomerIntentIdScope>.Parse("actors.intent.customer-001");
        private static readonly StableId<CustomerIdScope> CustomerId =
            StableId<CustomerIdScope>.Parse("actors.customer.001");

        [Test]
        public void ExactStartIsIdempotentAndCreatesImmutableIntent()
        {
            CustomerVisitAuthority authority = CreateAuthority();

            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);

            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.Count, Is.EqualTo(1));
            Assert.That(authority.ActiveCount, Is.EqualTo(1));
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Entering));
            Assert.That(visit.Intent.Id, Is.EqualTo(IntentId));
            Assert.That(visit.Intent.CustomerId, Is.EqualTo(CustomerId));
            Assert.That(visit.Intent.ProductId, Is.EqualTo(ProductId));
            Assert.That(visit.Intent.Need, Is.EqualTo(CustomerNeedKind.GraphicsUpgrade));
            Assert.That(visit.StateDeadline.ElapsedMilliseconds, Is.EqualTo(6_000));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ValidLifecycleAdvancesExactlyOncePerStateAndQueriesByStableId()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            StableId<CustomerVisitIdScope> laterVisit =
                StableId<CustomerVisitIdScope>.Parse("actors.visit.z-customer");
            StableId<CustomerIntentIdScope> laterIntent =
                StableId<CustomerIntentIdScope>.Parse("actors.intent.z-customer");
            StableId<CustomerIdScope> laterCustomer =
                StableId<CustomerIdScope>.Parse("actors.customer.z-customer");
            Assert.That(Start(authority, laterVisit, laterIntent, laterCustomer, Time(1)).IsSuccess, Is.True);
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);

            Assert.That(authority.MarkBrowseArrival(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.BeginCheckoutNavigation(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.MarkCheckoutArrival(VisitId, Time(4)).IsSuccess, Is.True);
            Assert.That(authority.BeginExit(
                VisitId,
                CustomerVisitExitReason.Fulfilled,
                Time(5)).IsSuccess, Is.True);
            Assert.That(authority.MarkExitArrival(VisitId, Time(6)).IsSuccess, Is.True);
            Assert.That(authority.MarkExitArrival(VisitId, Time(6)).IsSuccess, Is.True);

            Assert.That(authority.Revision, Is.EqualTo(7));
            Assert.That(authority.ActiveCount, Is.EqualTo(1));
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Exited));
            Assert.That(visit.ExitReason, Is.EqualTo(CustomerVisitExitReason.Fulfilled));
            Assert.That(visit.RouteFallbackUsed, Is.False);
            Assert.That(authority.GetVisits().Select(record => record.Id.Value),
                Is.EqualTo(new[] { VisitId.Value, laterVisit.Value }));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void DelayedExactLifecycleCommandsReplayAfterTerminalAndWatermarkAdvance()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.BeginCheckoutNavigation(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.MarkCheckoutArrival(VisitId, Time(4)).IsSuccess, Is.True);
            Assert.That(authority.BeginExit(
                VisitId,
                CustomerVisitExitReason.Fulfilled,
                Time(5)).IsSuccess, Is.True);
            Assert.That(authority.MarkExitArrival(VisitId, Time(6)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord terminal), Is.True);
            long terminalRevision = authority.Revision;

            Assert.That(authority.AdvanceTime(Time(20)).IsSuccess, Is.True);
            Assert.That(authority.MarkExitArrival(VisitId, Time(6)).IsSuccess, Is.True);
            Assert.That(authority.BeginExit(
                VisitId,
                CustomerVisitExitReason.Fulfilled,
                Time(5)).IsSuccess, Is.True);
            Assert.That(authority.MarkCheckoutArrival(VisitId, Time(4)).IsSuccess, Is.True);
            Assert.That(authority.BeginCheckoutNavigation(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(19)).Error,
                Is.EqualTo(CustomerVisitFailures.NonMonotonicTimestamp));
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord replayed), Is.True);
            Assert.That(replayed, Is.SameAs(terminal));
            Assert.That(authority.Revision, Is.EqualTo(terminalRevision));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MaximumEightReceiptsRemainBoundedAndInvariantSafe()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.BeginCheckoutNavigation(VisitId, Time(4)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(5)).IsSuccess, Is.True);
            Assert.That(authority.MarkCheckoutArrival(VisitId, Time(6)).IsSuccess, Is.True);
            Assert.That(authority.BeginExit(
                VisitId,
                CustomerVisitExitReason.Fulfilled,
                Time(7)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(8)).IsSuccess, Is.True);
            Assert.That(authority.MarkExitArrival(VisitId, Time(9)).IsSuccess, Is.True);

            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Exited));
            Assert.That(visit.TotalRouteFailureCount, Is.EqualTo(3));
            Assert.That(visit.RouteFallbackUsed, Is.False);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InvalidIdentityProductNeedAndConflictsLeaveAuthorityUntouched()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            long revision = authority.Revision;

            Assert.That(authority.StartVisit(
                default,
                IntentId,
                CustomerId,
                ProductId,
                CustomerNeedKind.GraphicsUpgrade,
                Time(1)).Error, Is.EqualTo(CustomerVisitFailures.InvalidVisitId));
            Assert.That(authority.StartVisit(
                VisitId,
                IntentId,
                CustomerId,
                StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.unknown"),
                CustomerNeedKind.GraphicsUpgrade,
                Time(1)).Error, Is.EqualTo(CustomerVisitFailures.UnknownProduct));
            Assert.That(authority.StartVisit(
                VisitId,
                IntentId,
                CustomerId,
                ProductId,
                (CustomerNeedKind)999,
                Time(1)).Error, Is.EqualTo(CustomerVisitFailures.InvalidNeed));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.Count, Is.Zero);

            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            revision = authority.Revision;
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(2)).Error,
                Is.EqualTo(CustomerVisitFailures.VisitIdentityConflict));
            Assert.That(Start(
                authority,
                StableId<CustomerVisitIdScope>.Parse("actors.visit.duplicate-intent"),
                IntentId,
                StableId<CustomerIdScope>.Parse("actors.customer.other"),
                Time(2)).Error, Is.EqualTo(CustomerVisitFailures.DuplicateIntent));
            Assert.That(Start(
                authority,
                StableId<CustomerVisitIdScope>.Parse("actors.visit.duplicate-customer"),
                StableId<CustomerIntentIdScope>.Parse("actors.intent.other"),
                CustomerId,
                Time(2)).Error, Is.EqualTo(CustomerVisitFailures.CustomerAlreadyVisiting));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.Count, Is.EqualTo(1));
        }

        [Test]
        public void SkippedBackwardAndNonMonotonicTransitionsDoNotMutate()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            long revision = authority.Revision;

            Assert.That(authority.BeginCheckoutNavigation(VisitId, Time(100)).Error,
                Is.EqualTo(CustomerVisitFailures.InvalidTransition));
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(1)).Error,
                Is.EqualTo(CustomerVisitFailures.NonMonotonicTimestamp));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord entering), Is.True);
            Assert.That(entering.State, Is.EqualTo(CustomerVisitState.Entering));

            Assert.That(authority.MarkBrowseArrival(VisitId, Time(2)).IsSuccess, Is.True);
            revision = authority.Revision;
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(3)).Error,
                Is.EqualTo(CustomerVisitFailures.InvalidTransition));
            Assert.That(authority.ReportRouteFailure(VisitId, Time(3)).Error,
                Is.EqualTo(CustomerVisitFailures.RouteFailureNotAllowed));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RouteFailuresRetryOnceThenUseExitAndTerminalFallbacks()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);

            Assert.That(authority.ReportRouteFailure(VisitId, Time(2)).IsSuccess, Is.True);
            long retryRevision = authority.Revision;
            Assert.That(authority.ReportRouteFailure(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord retry), Is.True);
            Assert.That(retry.State, Is.EqualTo(CustomerVisitState.Entering));
            Assert.That(retry.RouteFailureCount, Is.EqualTo(1));
            Assert.That(retry.TotalRouteFailureCount, Is.EqualTo(1));
            Assert.That(retry.HasRouteFailureReport, Is.True);
            Assert.That(retry.LastRouteFailureAt, Is.EqualTo(Time(2)));
            Assert.That(retry.StateDeadline.Tick, Is.EqualTo(Time(2).Tick));
            Assert.That(retry.StateDeadline.ElapsedMilliseconds, Is.EqualTo(7_000));
            Assert.That(authority.Revision, Is.EqualTo(retryRevision));

            Assert.That(authority.ReportRouteFailure(VisitId, Time(3)).IsSuccess, Is.True);
            long exitRevision = authority.Revision;
            Assert.That(authority.ReportRouteFailure(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord exiting), Is.True);
            Assert.That(exiting.State, Is.EqualTo(CustomerVisitState.Exiting));
            Assert.That(exiting.ExitReason, Is.EqualTo(CustomerVisitExitReason.RouteUnavailable));
            Assert.That(exiting.RouteFailureCount, Is.Zero);
            Assert.That(exiting.TotalRouteFailureCount, Is.EqualTo(2));
            Assert.That(authority.Revision, Is.EqualTo(exitRevision));

            Assert.That(authority.ReportRouteFailure(VisitId, Time(4)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(5)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord exited), Is.True);
            long terminalRevision = authority.Revision;
            Assert.That(authority.MarkExitArrival(VisitId, Time(5)).Error,
                Is.EqualTo(CustomerVisitFailures.InvalidTransition));
            Assert.That(authority.ReportRouteFailure(VisitId, Time(5)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(4)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord replayedExit), Is.True);
            Assert.That(replayedExit, Is.SameAs(exited));
            Assert.That(authority.Revision, Is.EqualTo(terminalRevision));
            Assert.That(exited.State, Is.EqualTo(CustomerVisitState.Exited));
            Assert.That(exited.ExitReason, Is.EqualTo(CustomerVisitExitReason.RouteUnavailable));
            Assert.That(exited.RouteFallbackUsed, Is.True);
            Assert.That(exited.TotalRouteFailureCount, Is.EqualTo(4));
            Assert.That(authority.ActiveCount, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FulfilledExitRequiresCheckoutWaitAndBackwardAdvanceDoesNotMutate()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            long revision = authority.Revision;

            Assert.That(authority.BeginExit(
                VisitId,
                CustomerVisitExitReason.Fulfilled,
                Time(2)).Error, Is.EqualTo(CustomerVisitFailures.InvalidTransition));
            Assert.That(authority.BeginExit(
                VisitId,
                CustomerVisitExitReason.PatienceExpired,
                Time(2)).Error, Is.EqualTo(CustomerVisitFailures.InvalidExitReason));
            Assert.That(authority.AdvanceTime(Time(3)).IsSuccess, Is.True);
            Assert.That(authority.AdvanceTime(Time(2)).Error,
                Is.EqualTo(CustomerVisitFailures.NonMonotonicTimestamp));
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(2)).Error,
                Is.EqualTo(CustomerVisitFailures.NonMonotonicTimestamp));
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Entering));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PatienceAndExitTimeoutsMutateAllExpiredVisitsOncePerAdvance()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            StableId<CustomerVisitIdScope> secondVisit =
                StableId<CustomerVisitIdScope>.Parse("actors.visit.customer-002");
            StableId<CustomerIntentIdScope> secondIntent =
                StableId<CustomerIntentIdScope>.Parse("actors.intent.customer-002");
            StableId<CustomerIdScope> secondCustomer =
                StableId<CustomerIdScope>.Parse("actors.customer.002");
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            Assert.That(Start(authority, secondVisit, secondIntent, secondCustomer, Time(1)).IsSuccess, Is.True);
            long revision = authority.Revision;

            Assert.That(authority.AdvanceTime(Time(5)).IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.AdvanceTime(Time(6)).IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.GetVisits().All(visit =>
                visit.State == CustomerVisitState.Exiting &&
                visit.ExitReason == CustomerVisitExitReason.PatienceExpired), Is.True);

            Assert.That(authority.AdvanceTime(Time(11)).IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 2));
            Assert.That(authority.GetVisits().All(visit =>
                visit.State == CustomerVisitState.Exited && visit.RouteFallbackUsed), Is.True);
            long terminalRevision = authority.Revision;
            Assert.That(authority.MarkExitArrival(VisitId, Time(11)).Error,
                Is.EqualTo(CustomerVisitFailures.InvalidTransition));
            Assert.That(authority.AdvanceTime(Time(11)).IsSuccess, Is.True);
            Assert.That(authority.AdvanceTime(Time(10)).Error,
                Is.EqualTo(CustomerVisitFailures.NonMonotonicTimestamp));
            Assert.That(authority.Revision, Is.EqualTo(terminalRevision));
            Assert.That(authority.ActiveCount, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void NonEmptyReceiptLedgerSurvivesPatienceAndExitTimeouts()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(3)).IsSuccess, Is.True);

            Assert.That(authority.AdvanceTime(Time(8)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord exiting), Is.True);
            Assert.That(exiting.State, Is.EqualTo(CustomerVisitState.Exiting));
            Assert.That(exiting.ExitReason, Is.EqualTo(CustomerVisitExitReason.PatienceExpired));
            Assert.That(exiting.TotalRouteFailureCount, Is.EqualTo(1));

            Assert.That(authority.AdvanceTime(Time(13)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord terminal), Is.True);
            Assert.That(terminal.State, Is.EqualTo(CustomerVisitState.Exited));
            Assert.That(terminal.RouteFallbackUsed, Is.True);
            long terminalRevision = authority.Revision;

            Assert.That(authority.AdvanceTime(Time(20)).IsSuccess, Is.True);
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord replayed), Is.True);
            Assert.That(replayed, Is.SameAs(terminal));
            Assert.That(authority.Revision, Is.EqualTo(terminalRevision));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FulfilledReasonSurvivesAFailedExitRoute()
        {
            CustomerVisitAuthority authority = CreateAuthority();
            Assert.That(Start(authority, VisitId, IntentId, CustomerId, Time(1)).IsSuccess, Is.True);
            Assert.That(authority.MarkBrowseArrival(VisitId, Time(2)).IsSuccess, Is.True);
            Assert.That(authority.BeginCheckoutNavigation(VisitId, Time(3)).IsSuccess, Is.True);
            Assert.That(authority.MarkCheckoutArrival(VisitId, Time(4)).IsSuccess, Is.True);
            Assert.That(authority.BeginExit(
                VisitId,
                CustomerVisitExitReason.Fulfilled,
                Time(5)).IsSuccess, Is.True);

            Assert.That(authority.ReportRouteFailure(VisitId, Time(6)).IsSuccess, Is.True);
            Assert.That(authority.ReportRouteFailure(VisitId, Time(7)).IsSuccess, Is.True);
            Assert.That(authority.TryGetVisit(VisitId, out CustomerVisitRecord visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Exited));
            Assert.That(visit.ExitReason, Is.EqualTo(CustomerVisitExitReason.Fulfilled));
            Assert.That(visit.RouteFallbackUsed, Is.True);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static CustomerVisitAuthority CreateAuthority()
        {
            ProductDefinition product = ProductDefinition.Create(
                ProductId,
                StableId<ProductCategoryIdScope>.Parse("catalog.category.gpu"),
                "Test Graphics Card",
                ProductTrackingPolicy.SerializedInstance,
                365).Value;
            ProductCatalog catalog = ProductCatalog.Create(new[] { product }).Value;
            return CustomerVisitAuthority.Create(
                catalog,
                SimulationDuration.FromMilliseconds(5_000),
                2).Value;
        }

        private static OperationResult Start(
            CustomerVisitAuthority authority,
            StableId<CustomerVisitIdScope> visitId,
            StableId<CustomerIntentIdScope> intentId,
            StableId<CustomerIdScope> customerId,
            SimulationTimestamp at)
        {
            return authority.StartVisit(
                visitId,
                intentId,
                customerId,
                ProductId,
                CustomerNeedKind.GraphicsUpgrade,
                at);
        }

        private static SimulationTimestamp Time(long second)
        {
            return SimulationTimestamp.Create(second, second * 1_000L);
        }
    }
}
