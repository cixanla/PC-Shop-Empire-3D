using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Retail
{
    public sealed class CustomerOfferDecisionEvaluatorTests
    {
        private static readonly StableId<ProductDefinitionIdScope> ProductA =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.decision-a60");
        private static readonly StableId<ProductDefinitionIdScope> ProductB =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.decision-b70");
        private static readonly StableId<ContainerIdScope> ShelfA =
            StableId<ContainerIdScope>.Parse("inventory.container.decision-shelf-a");
        private static readonly StableId<ContainerIdScope> ShelfB =
            StableId<ContainerIdScope>.Parse("inventory.container.decision-shelf-b");
        private static readonly StableId<ShelfOfferIdScope> OfferA =
            StableId<ShelfOfferIdScope>.Parse("retail.offer.decision-a60");
        private static readonly StableId<ShelfOfferIdScope> OfferB =
            StableId<ShelfOfferIdScope>.Parse("retail.offer.decision-b70");
        private static readonly StableId<CustomerVisitIdScope> VisitId =
            StableId<CustomerVisitIdScope>.Parse("actors.visit.decision-customer");
        private static readonly StableId<CustomerIntentIdScope> IntentId =
            StableId<CustomerIntentIdScope>.Parse("actors.intent.decision-a60");
        private static readonly StableId<CustomerIdScope> CustomerId =
            StableId<CustomerIdScope>.Parse("actors.customer.decision-customer");

        [Test]
        public void StableCodesAndEnumValuesMatchPublicContract()
        {
            Assert.That((int)CustomerOfferDecisionKind.Buy, Is.EqualTo(1));
            Assert.That((int)CustomerOfferDecisionKind.Leave, Is.EqualTo(2));
            Assert.That(CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit,
                Is.EqualTo("retail.offer-decision.buy.exact-product-within-limit"));
            Assert.That(CustomerOfferDecisionReasonCodes.LeaveProductMismatch,
                Is.EqualTo("retail.offer-decision.leave.product-mismatch"));
            Assert.That(CustomerOfferDecisionReasonCodes.LeavePriceAboveLimit,
                Is.EqualTo("retail.offer-decision.leave.price-above-limit"));
            Assert.That(CustomerOfferDecisionFailures.InputInvalid.Code,
                Is.EqualTo("retail.offer-decision.input-invalid"));
            Assert.That(CustomerOfferDecisionFailures.VisitNotBrowsing.Code,
                Is.EqualTo("retail.offer-decision.visit-not-browsing"));
            Assert.That(CustomerOfferDecisionFailures.NeedUnsupported.Code,
                Is.EqualTo("retail.offer-decision.need-unsupported"));
            Assert.That(CustomerOfferDecisionFailures.CurrencyMismatch.Code,
                Is.EqualTo("retail.offer-decision.currency-mismatch"));
        }

        [TestCase(60_000L)]
        [TestCase(54_999L)]
        public void ExactProductWithinOrAtLimitReturnsBuyWithExactProvenance(long limitMinorUnits)
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord offer = Publish(fixture, OfferA, ProductA, ShelfA, "EUR", 54_999);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            ShelfPrice limit = Price("EUR", limitMinorUnits);
            Snapshot before = Snapshot.Capture(fixture);

            OperationResult<CustomerOfferDecision> result =
                CustomerOfferDecisionEvaluator.Evaluate(visit, offer, limit);

            Assert.That(result.IsSuccess, Is.True);
            CustomerOfferDecision decision = result.Value;
            Assert.That(decision.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Buy));
            Assert.That(decision.ReasonCode,
                Is.EqualTo(CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit));
            Assert.That(decision.CustomerId, Is.EqualTo(CustomerId));
            Assert.That(decision.VisitId, Is.EqualTo(VisitId));
            Assert.That(decision.IntentId, Is.EqualTo(IntentId));
            Assert.That(decision.VisitState, Is.EqualTo(CustomerVisitState.Browsing));
            Assert.That(decision.VisitLastUpdatedAt, Is.EqualTo(visit.LastUpdatedAt));
            Assert.That(decision.Need, Is.EqualTo(CustomerNeedKind.GraphicsUpgrade));
            Assert.That(decision.IntentProductId, Is.EqualTo(ProductA));
            Assert.That(decision.OfferId, Is.EqualTo(OfferA));
            Assert.That(decision.OfferRevision, Is.EqualTo(1));
            Assert.That(decision.ShelfContainerId, Is.EqualTo(ShelfA));
            Assert.That(decision.OfferProductId, Is.EqualTo(ProductA));
            Assert.That(decision.OfferPrice, Is.EqualTo(offer.Price));
            Assert.That(decision.MaximumAcceptedPrice, Is.EqualTo(limit));
            before.AssertUnchanged(fixture, visit, offer);
        }

        [Test]
        public void ProductMismatchReturnsLeaveBeforePriceComparison()
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord offer = Publish(fixture, OfferB, ProductB, ShelfB, "EUR", 90_000);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            Snapshot before = Snapshot.Capture(fixture);

            OperationResult<CustomerOfferDecision> result =
                CustomerOfferDecisionEvaluator.Evaluate(visit, offer, Price("EUR", 60_000));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Leave));
            Assert.That(result.Value.ReasonCode,
                Is.EqualTo(CustomerOfferDecisionReasonCodes.LeaveProductMismatch));
            before.AssertUnchanged(fixture, visit, offer);
        }

        [Test]
        public void CurrencyMismatchPrecedesProductMismatch()
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord offer = Publish(fixture, OfferB, ProductB, ShelfB, "EUR", 54_999);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            Snapshot before = Snapshot.Capture(fixture);

            OperationResult<CustomerOfferDecision> result =
                CustomerOfferDecisionEvaluator.Evaluate(
                    visit,
                    offer,
                    Price("USD", 60_000));

            Assert.That(result.Error, Is.EqualTo(CustomerOfferDecisionFailures.CurrencyMismatch));
            before.AssertUnchanged(fixture, visit, offer);
        }

        [Test]
        public void ExactProductAboveLimitReturnsLeaveWithStableReason()
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord offer = Publish(fixture, OfferA, ProductA, ShelfA, "EUR", 60_001);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            Snapshot before = Snapshot.Capture(fixture);

            OperationResult<CustomerOfferDecision> result =
                CustomerOfferDecisionEvaluator.Evaluate(visit, offer, Price("EUR", 60_000));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Leave));
            Assert.That(result.Value.ReasonCode,
                Is.EqualTo(CustomerOfferDecisionReasonCodes.LeavePriceAboveLimit));
            before.AssertUnchanged(fixture, visit, offer);
        }

        [Test]
        public void StructurallyInvalidInputFailsFirstWithoutMutation()
        {
            Fixture fixture = CreateFixture(browsing: false);
            ShelfOfferRecord offer = Publish(fixture, OfferA, ProductA, ShelfA, "EUR", 54_999);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            Snapshot before = Snapshot.Capture(fixture);

            Assert.That(CustomerOfferDecisionEvaluator.Evaluate(null, offer, default).Error,
                Is.EqualTo(CustomerOfferDecisionFailures.InputInvalid));
            Assert.That(CustomerOfferDecisionEvaluator.Evaluate(visit, null, default).Error,
                Is.EqualTo(CustomerOfferDecisionFailures.InputInvalid));
            Assert.That(CustomerOfferDecisionEvaluator.Evaluate(visit, offer, default).Error,
                Is.EqualTo(CustomerOfferDecisionFailures.InputInvalid));
            before.AssertUnchanged(fixture, visit, offer);
        }

        [Test]
        public void NonBrowsingVisitAndCurrencyMismatchFailInDocumentedOrder()
        {
            Fixture fixture = CreateFixture(browsing: false);
            ShelfOfferRecord offer = Publish(fixture, OfferA, ProductA, ShelfA, "EUR", 54_999);
            CustomerVisitRecord entering = CurrentVisit(fixture);
            Snapshot before = Snapshot.Capture(fixture);

            Assert.That(CustomerOfferDecisionEvaluator.Evaluate(
                    entering,
                    offer,
                    Price("USD", 60_000)).Error,
                Is.EqualTo(CustomerOfferDecisionFailures.VisitNotBrowsing));
            Assert.That(fixture.Visits.MarkBrowseArrival(VisitId, Time(2)).IsSuccess, Is.True);
            CustomerVisitRecord browsing = CurrentVisit(fixture);
            Snapshot afterBrowse = Snapshot.Capture(fixture);
            Assert.That(CustomerOfferDecisionEvaluator.Evaluate(
                    browsing,
                    offer,
                    Price("USD", 60_000)).Error,
                Is.EqualTo(CustomerOfferDecisionFailures.CurrencyMismatch));
            afterBrowse.AssertUnchanged(fixture, browsing, offer);
            Assert.That(before.OfferRevision, Is.EqualTo(afterBrowse.OfferRevision));
            Assert.That(before.InventoryRevision, Is.EqualTo(afterBrowse.InventoryRevision));
        }

        [Test]
        public void ExactReplayReturnsValueEqualDecisionAndPreservesAuthorityState()
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord offer = Publish(fixture, OfferA, ProductA, ShelfA, "EUR", 54_999);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            ShelfPrice limit = Price("EUR", 60_000);
            Snapshot before = Snapshot.Capture(fixture);

            CustomerOfferDecision first =
                CustomerOfferDecisionEvaluator.Evaluate(visit, offer, limit).Value;
            CustomerOfferDecision replay =
                CustomerOfferDecisionEvaluator.Evaluate(visit, offer, limit).Value;

            Assert.That(replay, Is.Not.SameAs(first));
            Assert.That(replay, Is.EqualTo(first));
            Assert.That(replay.GetHashCode(), Is.EqualTo(first.GetHashCode()));
            before.AssertUnchanged(fixture, visit, offer);
        }

        [Test]
        public void DifferentAcceptedLimitProducesUnequalProvenanceForSameBuyOutcome()
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord offer = Publish(fixture, OfferA, ProductA, ShelfA, "EUR", 54_999);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            Snapshot before = Snapshot.Capture(fixture);

            CustomerOfferDecision first = CustomerOfferDecisionEvaluator.Evaluate(
                visit,
                offer,
                Price("EUR", 60_000)).Value;
            CustomerOfferDecision second = CustomerOfferDecisionEvaluator.Evaluate(
                visit,
                offer,
                Price("EUR", 61_000)).Value;

            Assert.That(first.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Buy));
            Assert.That(second.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Buy));
            Assert.That(second.ReasonCode, Is.EqualTo(first.ReasonCode));
            Assert.That(second, Is.Not.EqualTo(first));
            before.AssertUnchanged(fixture, visit, offer);
        }

        [Test]
        public void HistoricalBrowsingSnapshotReplayCannotMutateCurrentVisit()
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord offer = Publish(fixture, OfferA, ProductA, ShelfA, "EUR", 54_999);
            CustomerVisitRecord historicalBrowsing = CurrentVisit(fixture);
            ShelfPrice limit = Price("EUR", 60_000);
            CustomerOfferDecision historicalDecision = CustomerOfferDecisionEvaluator.Evaluate(
                historicalBrowsing,
                offer,
                limit).Value;

            Assert.That(fixture.Visits.BeginCheckoutNavigation(VisitId, Time(3)).IsSuccess, Is.True);
            CustomerVisitRecord currentVisit = CurrentVisit(fixture);
            Assert.That(currentVisit.State, Is.EqualTo(CustomerVisitState.NavigatingToCheckout));
            long currentRevision = fixture.Visits.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            long offerRevision = fixture.Offers.Revision;

            CustomerOfferDecision replay = CustomerOfferDecisionEvaluator.Evaluate(
                historicalBrowsing,
                offer,
                limit).Value;

            Assert.That(replay, Is.EqualTo(historicalDecision));
            Assert.That(fixture.Visits.Revision, Is.EqualTo(currentRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Offers.Revision, Is.EqualTo(offerRevision));
            Assert.That(fixture.Visits.TryGetVisit(VisitId, out CustomerVisitRecord afterReplay),
                Is.True);
            Assert.That(afterReplay, Is.SameAs(currentVisit));
            Assert.That(afterReplay.State,
                Is.EqualTo(CustomerVisitState.NavigatingToCheckout));
        }

        [Test]
        public void HistoricalOfferSnapshotReplayStaysStableAndNewRevisionIsReevaluated()
        {
            Fixture fixture = CreateFixture(browsing: true);
            ShelfOfferRecord historical = Publish(
                fixture,
                OfferA,
                ProductA,
                ShelfA,
                "EUR",
                54_999);
            CustomerVisitRecord visit = CurrentVisit(fixture);
            ShelfPrice limit = Price("EUR", 60_000);
            CustomerOfferDecision historicalDecision =
                CustomerOfferDecisionEvaluator.Evaluate(visit, historical, limit).Value;

            Assert.That(fixture.Offers.SetOffer(
                OfferA,
                ProductA,
                ShelfA,
                "EUR",
                64_999).IsSuccess, Is.True);
            Assert.That(fixture.Offers.TryGetOffer(OfferA, out ShelfOfferRecord current), Is.True);
            Snapshot afterUpdate = Snapshot.Capture(fixture);

            CustomerOfferDecision replay =
                CustomerOfferDecisionEvaluator.Evaluate(visit, historical, limit).Value;
            CustomerOfferDecision reevaluated =
                CustomerOfferDecisionEvaluator.Evaluate(visit, current, limit).Value;

            Assert.That(replay, Is.EqualTo(historicalDecision));
            Assert.That(replay.OfferRevision, Is.EqualTo(1));
            Assert.That(replay.OfferPrice.MinorUnits, Is.EqualTo(54_999));
            Assert.That(reevaluated.OfferRevision, Is.EqualTo(2));
            Assert.That(reevaluated.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Leave));
            Assert.That(reevaluated.ReasonCode,
                Is.EqualTo(CustomerOfferDecisionReasonCodes.LeavePriceAboveLimit));
            afterUpdate.AssertUnchanged(fixture, visit, current);
        }

        private static Fixture CreateFixture(bool browsing)
        {
            ProductDefinition first = CreateProduct(ProductA, "Decision A60");
            ProductDefinition second = CreateProduct(ProductB, "Decision B70");
            ProductCatalog catalog = ProductCatalog.Create(new[] { first, second }).Value;
            InventoryAuthority inventory = InventoryAuthority.Create(catalog).Value;
            RegisterShelf(inventory, ShelfA);
            RegisterShelf(inventory, ShelfB);
            ShelfOfferAuthority offers = ShelfOfferAuthority.Create(catalog, inventory).Value;
            CustomerVisitAuthority visits = CustomerVisitAuthority.Create(
                catalog,
                SimulationDuration.FromMilliseconds(5_000),
                CustomerVisitAuthority.RequiredRouteAttemptLimit).Value;
            Assert.That(visits.StartVisit(
                VisitId,
                IntentId,
                CustomerId,
                ProductA,
                CustomerNeedKind.GraphicsUpgrade,
                Time(1)).IsSuccess, Is.True);
            if (browsing)
            {
                Assert.That(visits.MarkBrowseArrival(VisitId, Time(2)).IsSuccess, Is.True);
            }

            return new Fixture(inventory, offers, visits);
        }

        private static ProductDefinition CreateProduct(
            StableId<ProductDefinitionIdScope> id,
            string name)
        {
            return ProductDefinition.Create(
                id,
                StableId<ProductCategoryIdScope>.Parse("catalog.category.graphics-cards"),
                name,
                ProductTrackingPolicy.SerializedInstance,
                365).Value;
        }

        private static void RegisterShelf(
            InventoryAuthority inventory,
            StableId<ContainerIdScope> id)
        {
            Assert.That(inventory.RegisterContainer(
                InventoryContainerDefinition.Create(
                    id,
                    InventoryContainerKind.Shelf,
                    8).Value).IsSuccess, Is.True);
        }

        private static ShelfOfferRecord Publish(
            Fixture fixture,
            StableId<ShelfOfferIdScope> offerId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> shelfId,
            string currency,
            long minorUnits)
        {
            Assert.That(fixture.Offers.SetOffer(
                offerId,
                productId,
                shelfId,
                currency,
                minorUnits).IsSuccess, Is.True);
            Assert.That(fixture.Offers.TryGetOffer(offerId, out ShelfOfferRecord offer), Is.True);
            return offer;
        }

        private static CustomerVisitRecord CurrentVisit(Fixture fixture)
        {
            Assert.That(fixture.Visits.TryGetVisit(VisitId, out CustomerVisitRecord visit), Is.True);
            return visit;
        }

        private static ShelfPrice Price(string currency, long minorUnits)
        {
            return ShelfPrice.Create(currency, minorUnits).Value;
        }

        private static SimulationTimestamp Time(long second)
        {
            return SimulationTimestamp.Create(second, second * 1_000L);
        }

        private readonly struct Fixture
        {
            public Fixture(
                InventoryAuthority inventory,
                ShelfOfferAuthority offers,
                CustomerVisitAuthority visits)
            {
                Inventory = inventory;
                Offers = offers;
                Visits = visits;
            }

            public InventoryAuthority Inventory { get; }

            public ShelfOfferAuthority Offers { get; }

            public CustomerVisitAuthority Visits { get; }
        }

        private readonly struct Snapshot
        {
            private Snapshot(
                long inventoryRevision,
                long offerRevision,
                long visitRevision,
                int offerCount,
                int visitCount)
            {
                InventoryRevision = inventoryRevision;
                OfferRevision = offerRevision;
                VisitRevision = visitRevision;
                OfferCount = offerCount;
                VisitCount = visitCount;
            }

            public long InventoryRevision { get; }

            public long OfferRevision { get; }

            private long VisitRevision { get; }

            private int OfferCount { get; }

            private int VisitCount { get; }

            public static Snapshot Capture(Fixture fixture)
            {
                return new Snapshot(
                    fixture.Inventory.Revision,
                    fixture.Offers.Revision,
                    fixture.Visits.Revision,
                    fixture.Offers.Count,
                    fixture.Visits.Count);
            }

            public void AssertUnchanged(
                Fixture fixture,
                CustomerVisitRecord visit,
                ShelfOfferRecord offer)
            {
                Assert.That(fixture.Inventory.Revision, Is.EqualTo(InventoryRevision));
                Assert.That(fixture.Offers.Revision, Is.EqualTo(OfferRevision));
                Assert.That(fixture.Visits.Revision, Is.EqualTo(VisitRevision));
                Assert.That(fixture.Offers.Count, Is.EqualTo(OfferCount));
                Assert.That(fixture.Visits.Count, Is.EqualTo(VisitCount));
                Assert.That(fixture.Offers.TryGetOffer(offer.Id, out ShelfOfferRecord currentOffer),
                    Is.True);
                Assert.That(currentOffer, Is.SameAs(offer));
                Assert.That(fixture.Visits.TryGetVisit(visit.Id, out CustomerVisitRecord currentVisit),
                    Is.True);
                Assert.That(currentVisit, Is.SameAs(visit));
                Assert.That(fixture.Offers.ValidateInvariants().IsSuccess, Is.True);
                Assert.That(fixture.Visits.ValidateInvariants().IsSuccess, Is.True);
                Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
            }
        }
    }
}
