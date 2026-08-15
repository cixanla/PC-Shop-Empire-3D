using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Retail
{
    public sealed class CustomerOfferDecisionActionAuthorityTests
    {
        [Test]
        public void CustomerBindingIsTypedAndValueEqual()
        {
            StableId<CustomerRetailIdentityBindingIdScope> bindingId =
                StableId<CustomerRetailIdentityBindingIdScope>.Parse(
                    GarageStockFlowSession.PrototypeCustomerBindingIdValue);
            StableId<CustomerIdScope> actorId =
                StableId<CustomerIdScope>.Parse(
                    GarageStockFlowSession.PrototypeActorCustomerIdValue);
            StableId<RetailCustomerIdScope> retailId =
                StableId<RetailCustomerIdScope>.Parse(
                    GarageStockFlowSession.PrototypeCustomerIdValue);

            CustomerRetailIdentityBinding first =
                CustomerRetailIdentityBinding.Create(bindingId, actorId, retailId).Value;
            CustomerRetailIdentityBinding second =
                CustomerRetailIdentityBinding.Create(bindingId, actorId, retailId).Value;

            Assert.That(first, Is.EqualTo(second));
            Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
            Assert.That(first.ActorCustomerId, Is.EqualTo(actorId));
            Assert.That(first.RetailCustomerId, Is.EqualTo(retailId));
            Assert.That(CustomerRetailIdentityBinding.Create(default, actorId, retailId).Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.InputInvalid));
        }

        [Test]
        public void CreateRejectsBasketBackedByDifferentOfferAuthority()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();
            ShelfOfferAuthority otherOffers =
                ShelfOfferAuthority.Create(session.Catalog, session.Inventory).Value;
            RetailBasketAuthority otherBaskets =
                RetailBasketAuthority.Create(otherOffers, session.Inventory).Value;

            OperationResult<CustomerOfferDecisionActionAuthority> result =
                CustomerOfferDecisionActionAuthority.Create(
                    session.RetailOffers,
                    otherBaskets,
                    session.CustomerVisits);

            Assert.That(result.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.InputInvalid));
            Assert.That(session.RetailOffers.Revision, Is.Zero);
            Assert.That(otherOffers.Revision, Is.Zero);
            Assert.That(otherBaskets.Revision, Is.Zero);
            Assert.That(session.Inventory.ReservationCount, Is.Zero);
        }

        [Test]
        public void CurrentBuyAtomicallyReservesAndNavigatesExactlyOnce()
        {
            Fixture fixture = CreateBrowsingFixture();
            AuthoritySnapshot before = Snapshot(fixture.Session);

            OperationResult result = ApplyDefault(fixture);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(fixture.Session.CustomerOfferActions.Revision,
                Is.EqualTo(before.ActionRevision + 1));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(before.InventoryRevision + 1));
            Assert.That(fixture.Session.RetailBaskets.Revision,
                Is.EqualTo(before.BasketRevision + 1));
            Assert.That(fixture.Session.CustomerVisits.Revision,
                Is.EqualTo(before.VisitRevision + 1));
            Assert.That(fixture.Session.RetailOffers.Revision,
                Is.EqualTo(before.OfferRevision));
            Assert.That(fixture.Session.RetailCheckouts.Revision,
                Is.EqualTo(before.CheckoutRevision));
            Assert.That(fixture.Session.Orders.Revision,
                Is.EqualTo(before.OrderRevision));
            Assert.That(fixture.Session.TryGetPrototypeBasketLine(out var line), Is.True);
            Assert.That(line.IsActionOwned, Is.True);
            Assert.That(line.OwnerActionId,
                Is.EqualTo(fixture.Session.PrototypeCustomerBuyActionId));
            Assert.That(fixture.Session.TryGetPrototypeCustomerVisit(out var visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.NavigatingToCheckout));
            Assert.That(fixture.Session.TryGetPrototypeCheckout(out _), Is.False);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StaleOfferDecisionFailsWithoutAnyFurtherMutation()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(fixture.Session.RetailOffers.SetOffer(
                fixture.Session.ShelfOfferId,
                fixture.Session.ProductId,
                fixture.Session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits + 1).IsSuccess, Is.True);
            AuthoritySnapshot before = Snapshot(fixture.Session);

            OperationResult result = ApplyDefault(fixture);

            Assert.That(result.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.DecisionStale));
            AssertSnapshot(fixture.Session, before);
            Assert.That(fixture.Session.TryGetPrototypeBasketLine(out _), Is.False);
            Assert.That(fixture.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(fixture.Session.TryGetPrototypeCustomerVisit(out var visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Browsing));
        }

        [Test]
        public void StaleVisitDecisionFailsWithoutAnyFurtherMutation()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(fixture.Session.AdvanceCustomerTime(Time(100)).IsSuccess, Is.True);
            AuthoritySnapshot before = Snapshot(fixture.Session);

            OperationResult result = ApplyDefault(fixture, Time(101));

            Assert.That(result.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.DecisionStale));
            AssertSnapshot(fixture.Session, before);
            Assert.That(fixture.Session.TryGetPrototypeBasketLine(out _), Is.False);
            Assert.That(fixture.Session.Inventory.ReservationCount, Is.Zero);
        }

        [Test]
        public void ValidLeaveIsRejectedAsKindNotBuyWithoutAnyMutation()
        {
            GarageStockFlowSession session = CreateBrowsingSession();
            Assert.That(session.RetailOffers.SetOffer(
                session.ShelfOfferId,
                session.ProductId,
                session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1).IsSuccess,
                Is.True);
            CustomerOfferDecision leave = session.EvaluatePrototypeCustomerOffer().Value;
            Assert.That(leave.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Leave));
            var fixture = new Fixture(session, leave);
            AuthoritySnapshot before = Snapshot(session);

            OperationResult result = ApplyDefault(fixture);

            Assert.That(result.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.KindNotBuy));
            AssertSnapshot(session, before);
            Assert.That(session.CustomerOfferActions.Count, Is.Zero);
            Assert.That(session.RetailBaskets.Count, Is.Zero);
        }

        [Test]
        public void ActorRetailBindingMismatchFailsWithoutMutation()
        {
            Fixture fixture = CreateBrowsingFixture();
            CustomerRetailIdentityBinding wrongBinding =
                CustomerRetailIdentityBinding.Create(
                    fixture.Session.PrototypeCustomerBindingId,
                    StableId<CustomerIdScope>.Parse("actors.customer.other-walk-in"),
                    fixture.Session.PrototypeCustomerId).Value;
            AuthoritySnapshot before = Snapshot(fixture.Session);

            OperationResult result = fixture.Session.CustomerOfferActions.ApplyBuy(
                fixture.Session.PrototypeCustomerBuyActionId,
                wrongBinding,
                fixture.Decision,
                fixture.Session.PrototypeBasketLineId,
                fixture.Session.PrototypeBasketId,
                fixture.Session.ItemId,
                fixture.Session.PrototypeReservationId,
                fixture.Session.PrototypeClaimId,
                Time(12));

            Assert.That(result.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.CustomerBindingMismatch));
            AssertSnapshot(fixture.Session, before);
        }

        [Test]
        public void ReservationPreflightFailureDoesNotNavigateCustomer()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(fixture.Session.Inventory.ReserveSerializedItem(
                StableId<ReservationIdScope>.Parse("inventory.reservation.external-buy-test"),
                StableId<InventoryClaimIdScope>.Parse("inventory.claim.external-buy-test"),
                fixture.Session.ItemId).IsSuccess, Is.True);
            AuthoritySnapshot before = Snapshot(fixture.Session);

            OperationResult result = ApplyDefault(fixture);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ItemAlreadyReserved));
            AssertSnapshot(fixture.Session, before);
            Assert.That(fixture.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(fixture.Session.TryGetPrototypeCustomerVisit(out var visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Browsing));
        }

        [Test]
        public void NavigationPreflightFailureDoesNotReserveItem()
        {
            Fixture fixture = CreateBrowsingFixture();
            AuthoritySnapshot before = Snapshot(fixture.Session);

            OperationResult result = ApplyDefault(
                fixture,
                fixture.Decision.VisitLastUpdatedAt);

            Assert.That(result.Error,
                Is.EqualTo(CustomerVisitFailures.NonMonotonicTimestamp));
            AssertSnapshot(fixture.Session, before);
            Assert.That(fixture.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(fixture.Session.Inventory.ReservationCount, Is.Zero);
        }

        [Test]
        public void ExactReplayIsIdempotentAndConflictingReplayFailsClosed()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(ApplyDefault(fixture).IsSuccess, Is.True);
            AuthoritySnapshot afterFirst = Snapshot(fixture.Session);

            OperationResult exact = ApplyDefault(fixture);
            OperationResult conflict = fixture.Session.CustomerOfferActions.ApplyBuy(
                fixture.Session.PrototypeCustomerBuyActionId,
                fixture.Session.PrototypeCustomerBinding,
                fixture.Decision,
                fixture.Session.PrototypeBasketLineId,
                fixture.Session.PrototypeBasketId,
                fixture.Session.ItemId,
                fixture.Session.PrototypeReservationId,
                StableId<InventoryClaimIdScope>.Parse("inventory.claim.conflicting-buy"),
                Time(12));

            Assert.That(exact.IsSuccess, Is.True);
            Assert.That(conflict.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.ActionIdentityConflict));
            AssertSnapshot(fixture.Session, afterFirst);
            Assert.That(fixture.Session.CustomerOfferActions.Count, Is.EqualTo(1));
        }

        [Test]
        public void SecondActionIdForSameVisitFailsBeforeCurrentStateRevalidation()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(ApplyDefault(fixture).IsSuccess, Is.True);
            AuthoritySnapshot afterFirst = Snapshot(fixture.Session);

            OperationResult second = fixture.Session.CustomerOfferActions.ApplyBuy(
                StableId<CustomerOfferDecisionActionIdScope>.Parse(
                    "retail.offer-action.demo-walk-in-002"),
                fixture.Session.PrototypeCustomerBinding,
                fixture.Decision,
                fixture.Session.PrototypeBasketLineId,
                fixture.Session.PrototypeBasketId,
                fixture.Session.ItemId,
                fixture.Session.PrototypeReservationId,
                fixture.Session.PrototypeClaimId,
                Time(12));

            Assert.That(second.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.VisitAlreadyActioned));
            AssertSnapshot(fixture.Session, afterFirst);
        }

        [Test]
        public void ActionOwnedReservationCannotBeReleasedByLegacyBasketCommand()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(ApplyDefault(fixture).IsSuccess, Is.True);
            AuthoritySnapshot afterAction = Snapshot(fixture.Session);

            OperationResult release = fixture.Session.RetailBaskets.ReleaseLine(
                fixture.Session.PrototypeBasketLineId);

            Assert.That(release.Error, Is.EqualTo(RetailBasketFailures.ActionOwnedLine));
            AssertSnapshot(fixture.Session, afterAction);
            Assert.That(fixture.Session.TryGetPrototypeBasketLine(out _), Is.True);
            Assert.That(fixture.Session.Inventory.ReservationCount, Is.EqualTo(1));
        }

        [Test]
        public void ActionOwnedReservationCannotBeReleasedOrConsumedThroughPublicInventoryApi()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(ApplyDefault(fixture).IsSuccess, Is.True);
            Assert.That(fixture.Session.Inventory.TryGetReservation(
                fixture.Session.PrototypeReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ReleasePolicy,
                Is.EqualTo(InventoryReservationReleasePolicy.ConsumeOnly));
            AuthoritySnapshot afterAction = Snapshot(fixture.Session);

            OperationResult release = fixture.Session.Inventory.ReleaseReservation(
                fixture.Session.PrototypeReservationId);
            OperationResult consume = fixture.Session.Inventory.ConsumeReservation(
                fixture.Session.PrototypeReservationId);
            OperationResult consumeSet = fixture.Session.Inventory.ConsumeReservations(
                new[] { fixture.Session.PrototypeReservationId });

            Assert.That(release.Error,
                Is.EqualTo(InventoryFailures.ReservationReleaseRestricted));
            Assert.That(consume.Error,
                Is.EqualTo(InventoryFailures.ReservationConsumptionRestricted));
            Assert.That(consumeSet.Error,
                Is.EqualTo(InventoryFailures.ReservationConsumptionRestricted));
            AssertSnapshot(fixture.Session, afterAction);
            Assert.That(fixture.Session.Inventory.ReservationCount, Is.EqualTo(1));
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ActionRecordRemainsInvariantSafeAfterFulfillmentAndExit()
        {
            Fixture fixture = CreateBrowsingFixture();
            Assert.That(ApplyDefault(fixture).IsSuccess, Is.True);
            Assert.That(fixture.Session.MarkPrototypeCustomerCheckoutArrival(Time(13)).IsSuccess,
                Is.True);
            Assert.That(fixture.Session.BeginPrototypeCheckout().IsSuccess, Is.True);
            Assert.That(fixture.Session.CompletePrototypeCheckout().IsSuccess, Is.True);
            Assert.That(fixture.Session.BeginPrototypeCustomerExit(
                CustomerVisitExitReason.Fulfilled,
                Time(14)).IsSuccess, Is.True);
            Assert.That(fixture.Session.MarkPrototypeCustomerExitArrival(Time(15)).IsSuccess,
                Is.True);

            Assert.That(fixture.Session.TryGetPrototypeBasketLine(out _), Is.False);
            Assert.That(fixture.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(fixture.Session.TryGetPrototypeCustomerBuyAction(out var action), Is.True);
            Assert.That(action.SourceDecision, Is.EqualTo(fixture.Decision));
            Assert.That(fixture.Session.CustomerOfferActions.ValidateInvariants().IsSuccess,
                Is.True);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);

            AuthoritySnapshot afterFulfillment = Snapshot(fixture.Session);
            Assert.That(ApplyDefault(fixture).IsSuccess, Is.True);
            AssertSnapshot(fixture.Session, afterFulfillment);
        }

        private static Fixture CreateBrowsingFixture()
        {
            GarageStockFlowSession session = CreateBrowsingSession();
            return new Fixture(session, session.EvaluatePrototypeCustomerOffer().Value);
        }

        private static GarageStockFlowSession CreateBrowsingSession()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            Assert.That(session.StartPrototypeCustomerVisit(Time(10)).IsSuccess, Is.True);
            Assert.That(session.MarkPrototypeCustomerBrowseArrival(Time(11)).IsSuccess, Is.True);
            return session;
        }

        private static OperationResult ApplyDefault(
            Fixture fixture,
            SimulationTimestamp? at = null)
        {
            return fixture.Session.ApplyPrototypeCustomerBuy(
                fixture.Decision,
                at ?? Time(12));
        }

        private static AuthoritySnapshot Snapshot(GarageStockFlowSession session)
        {
            return new AuthoritySnapshot(
                session.CustomerOfferActions.Revision,
                session.Inventory.Revision,
                session.RetailBaskets.Revision,
                session.CustomerVisits.Revision,
                session.RetailOffers.Revision,
                session.RetailCheckouts.Revision,
                session.Orders.Revision,
                session.CustomerOfferActions.Count,
                session.Inventory.ReservationCount,
                session.RetailBaskets.Count,
                session.RetailCheckouts.Count);
        }

        private static void AssertSnapshot(
            GarageStockFlowSession session,
            AuthoritySnapshot expected)
        {
            Assert.That(Snapshot(session), Is.EqualTo(expected));
        }

        private static SimulationTimestamp Time(long tick)
        {
            return SimulationTimestamp.Create(tick, tick * 1000L);
        }

        private readonly struct Fixture
        {
            public Fixture(
                GarageStockFlowSession session,
                CustomerOfferDecision decision)
            {
                Session = session;
                Decision = decision;
            }

            public GarageStockFlowSession Session { get; }

            public CustomerOfferDecision Decision { get; }
        }

        private readonly struct AuthoritySnapshot
        {
            public AuthoritySnapshot(
                long actionRevision,
                long inventoryRevision,
                long basketRevision,
                long visitRevision,
                long offerRevision,
                long checkoutRevision,
                long orderRevision,
                int actionCount,
                int reservationCount,
                int basketCount,
                int checkoutCount)
            {
                ActionRevision = actionRevision;
                InventoryRevision = inventoryRevision;
                BasketRevision = basketRevision;
                VisitRevision = visitRevision;
                OfferRevision = offerRevision;
                CheckoutRevision = checkoutRevision;
                OrderRevision = orderRevision;
                ActionCount = actionCount;
                ReservationCount = reservationCount;
                BasketCount = basketCount;
                CheckoutCount = checkoutCount;
            }

            public long ActionRevision { get; }
            public long InventoryRevision { get; }
            public long BasketRevision { get; }
            public long VisitRevision { get; }
            public long OfferRevision { get; }
            public long CheckoutRevision { get; }
            public long OrderRevision { get; }
            public int ActionCount { get; }
            public int ReservationCount { get; }
            public int BasketCount { get; }
            public int CheckoutCount { get; }
        }
    }
}
