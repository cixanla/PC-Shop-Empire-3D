using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Tests.EditMode.Actors
{
    public sealed class CustomerConsultationAuthorityTests
    {
        private static readonly StableId<ProductDefinitionIdScope> ProductId =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.consultation-a60");
        private static readonly StableId<CustomerConsultationIdScope> ConsultationId =
            StableId<CustomerConsultationIdScope>.Parse("actors.consultation.customer-001");

        [Test]
        public void StableFailuresHaveExactCodes()
        {
            Assert.That(CustomerConsultationFailures.InputInvalid.Code,
                Is.EqualTo("actors.customer-consultation.input-invalid"));
            Assert.That(CustomerConsultationFailures.VisitNotBrowsing.Code,
                Is.EqualTo("actors.customer-consultation.visit-not-browsing"));
            Assert.That(CustomerConsultationFailures.VisitStale.Code,
                Is.EqualTo("actors.customer-consultation.visit-stale"));
            Assert.That(CustomerConsultationFailures.NonMonotonicTimestamp.Code,
                Is.EqualTo("actors.customer-consultation.non-monotonic-timestamp"));
            Assert.That(CustomerConsultationFailures.IdentityConflict.Code,
                Is.EqualTo("actors.customer-consultation.identity-conflict"));
            Assert.That(CustomerConsultationFailures.VisitAlreadyConsulted.Code,
                Is.EqualTo("actors.customer-consultation.visit-already-consulted"));
            Assert.That(CustomerConsultationFailures.AuthorityAlreadyAttached.Code,
                Is.EqualTo("actors.customer-consultation.authority-already-attached"));
            Assert.That(CustomerConsultationFailures.RevisionOverflow.Code,
                Is.EqualTo("actors.customer-consultation.revision-overflow"));
            Assert.That(CustomerConsultationFailures.InvariantViolation.Code,
                Is.EqualTo("actors.customer-consultation.invariant"));
            Assert.That(CustomerConsultationAuthority.Create(null).Error,
                Is.EqualTo(CustomerConsultationFailures.InputInvalid));
        }

        [Test]
        public void ValidConsultationPreservesExactImmutableProvenance()
        {
            CustomerVisitAuthority visits = CreateVisitAuthority();
            CustomerVisitRecord visit = CreateBrowsingVisit(visits, "customer-001", 1);
            CustomerConsultationAuthority authority = CreateAuthority(visits);

            Assert.That(authority.RecordConsultation(
                ConsultationId,
                visit,
                Time(3)).IsSuccess, Is.True);

            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.Count, Is.EqualTo(1));
            Assert.That(authority.TryGetConsultation(
                ConsultationId,
                out CustomerConsultationRecord record), Is.True);
            Assert.That(record.Id, Is.EqualTo(ConsultationId));
            Assert.That(record.CustomerId, Is.EqualTo(visit.Intent.CustomerId));
            Assert.That(record.VisitId, Is.EqualTo(visit.Id));
            Assert.That(record.IntentId, Is.EqualTo(visit.Intent.Id));
            Assert.That(record.Need, Is.EqualTo(CustomerNeedKind.GraphicsUpgrade));
            Assert.That(record.ProductId, Is.EqualTo(ProductId));
            Assert.That(record.VisitState, Is.EqualTo(CustomerVisitState.Browsing));
            Assert.That(record.VisitLastUpdatedAt, Is.EqualTo(visit.LastUpdatedAt));
            Assert.That(record.RecordedAt, Is.EqualTo(Time(3)));
            Assert.That(authority.TryGetForVisit(visit.Id, out CustomerConsultationRecord byVisit),
                Is.True);
            Assert.That(byVisit, Is.SameAs(record));

            var equivalent = new CustomerConsultationRecord(
                record.Id,
                record.CustomerId,
                record.VisitId,
                record.IntentId,
                record.Need,
                record.ProductId,
                record.VisitState,
                record.VisitLastUpdatedAt,
                record.RecordedAt);
            Assert.That(record, Is.EqualTo(equivalent));
            Assert.That(record.GetHashCode(), Is.EqualTo(equivalent.GetHashCode()));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SecondAuthorityForSameVisitSourceFailsWithoutMutatingCanonicalAuthority()
        {
            CustomerVisitAuthority visits = CreateVisitAuthority();
            CustomerConsultationAuthority canonical = CreateAuthority(visits);

            OperationResult<CustomerConsultationAuthority> second =
                CustomerConsultationAuthority.Create(visits);

            Assert.That(second.Error,
                Is.EqualTo(CustomerConsultationFailures.AuthorityAlreadyAttached));
            Assert.That(canonical.Revision, Is.Zero);
            Assert.That(canonical.Count, Is.Zero);
            Assert.That(canonical.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(visits.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ExactReplayIsIdempotentAndKeepsOriginalRecord()
        {
            CustomerVisitAuthority visits = CreateVisitAuthority();
            CustomerVisitRecord visit = CreateBrowsingVisit(visits, "customer-001", 1);
            CustomerConsultationAuthority authority = CreateAuthority(visits);
            Assert.That(authority.RecordConsultation(
                ConsultationId,
                visit,
                visit.LastUpdatedAt).IsSuccess, Is.True);
            Assert.That(authority.TryGetConsultation(
                ConsultationId,
                out CustomerConsultationRecord original), Is.True);
            Assert.That(visits.AdvanceTime(Time(8)).IsSuccess, Is.True);

            Assert.That(authority.RecordConsultation(
                ConsultationId,
                visit,
                visit.LastUpdatedAt).IsSuccess, Is.True);

            Assert.That(authority.Revision, Is.EqualTo(1));
            Assert.That(authority.Count, Is.EqualTo(1));
            Assert.That(authority.TryGetConsultation(
                ConsultationId,
                out CustomerConsultationRecord replayed), Is.True);
            Assert.That(replayed, Is.SameAs(original));
        }

        [Test]
        public void IdentityAndVisitConflictsFailWithoutMutation()
        {
            CustomerVisitAuthority visits = CreateVisitAuthority();
            CustomerVisitRecord visit = CreateBrowsingVisit(visits, "customer-001", 1);
            CustomerConsultationAuthority authority = CreateAuthority(visits);
            Assert.That(authority.RecordConsultation(
                ConsultationId,
                visit,
                Time(3)).IsSuccess, Is.True);
            long revision = authority.Revision;

            Assert.That(authority.RecordConsultation(
                ConsultationId,
                visit,
                Time(4)).Error, Is.EqualTo(CustomerConsultationFailures.IdentityConflict));
            Assert.That(authority.RecordConsultation(
                Consultation("customer-002"),
                visit,
                Time(3)).Error,
                Is.EqualTo(CustomerConsultationFailures.VisitAlreadyConsulted));

            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.Count, Is.EqualTo(1));
            Assert.That(authority.TryGetConsultation(
                ConsultationId,
                out CustomerConsultationRecord original), Is.True);
            Assert.That(original.RecordedAt, Is.EqualTo(Time(3)));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InvalidNonBrowsingAndNonMonotonicInputsLeaveAuthorityEmpty()
        {
            CustomerVisitAuthority visits = CreateVisitAuthority();
            CustomerConsultationAuthority authority = CreateAuthority(visits);
            CustomerVisitRecord entering = CreateEnteringVisit(
                visits,
                "customer-entering",
                1);
            CustomerVisitRecord browsing = CreateBrowsingVisit(
                visits,
                "customer-browsing",
                2);

            Assert.That(authority.RecordConsultation(
                default,
                browsing,
                Time(3)).Error, Is.EqualTo(CustomerConsultationFailures.InputInvalid));
            Assert.That(authority.RecordConsultation(
                Consultation("null-visit"),
                null,
                Time(3)).Error, Is.EqualTo(CustomerConsultationFailures.InputInvalid));
            Assert.That(authority.RecordConsultation(
                Consultation("entering"),
                entering,
                Time(3)).Error, Is.EqualTo(CustomerConsultationFailures.VisitNotBrowsing));
            Assert.That(authority.RecordConsultation(
                Consultation("old-time"),
                browsing,
                Time(2)).Error,
                Is.EqualTo(CustomerConsultationFailures.NonMonotonicTimestamp));

            Assert.That(authority.Revision, Is.Zero);
            Assert.That(authority.Count, Is.Zero);
            Assert.That(authority.GetConsultations(), Is.Empty);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ConsultationsAreReturnedInOrdinalStableIdOrder()
        {
            CustomerVisitAuthority visits = CreateVisitAuthority();
            CustomerConsultationAuthority authority = CreateAuthority(visits);
            CustomerVisitRecord zVisit = CreateBrowsingVisit(visits, "z-customer", 1);
            CustomerVisitRecord aVisit = CreateBrowsingVisit(visits, "a-customer", 3);
            StableId<CustomerConsultationIdScope> zId = Consultation("z-consultation");
            StableId<CustomerConsultationIdScope> aId = Consultation("a-consultation");

            Assert.That(authority.RecordConsultation(zId, zVisit, Time(5)).IsSuccess, Is.True);
            Assert.That(authority.RecordConsultation(aId, aVisit, Time(5)).IsSuccess, Is.True);

            Assert.That(authority.GetConsultations().Select(record => record.Id.Value),
                Is.EqualTo(new[] { aId.Value, zId.Value }));
            Assert.That(authority.Revision, Is.EqualTo(2));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RevisionOverflowAndBrokenIndexFailWithoutAdditionalMutation()
        {
            CustomerVisitAuthority overflowVisits = CreateVisitAuthority();
            CustomerConsultationAuthority overflow = CreateAuthority(overflowVisits);
            SetRevision(overflow, long.MaxValue);
            CustomerVisitRecord overflowVisit = CreateBrowsingVisit(
                overflowVisits,
                "overflow",
                1);

            Assert.That(overflow.RecordConsultation(
                Consultation("overflow"),
                overflowVisit,
                Time(3)).Error, Is.EqualTo(CustomerConsultationFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(overflow.Count, Is.Zero);

            CustomerVisitAuthority brokenVisits = CreateVisitAuthority();
            CustomerConsultationAuthority broken = CreateAuthority(brokenVisits);
            CustomerVisitRecord first = CreateBrowsingVisit(brokenVisits, "first", 1);
            Assert.That(broken.RecordConsultation(
                Consultation("first"),
                first,
                Time(3)).IsSuccess, Is.True);
            RemoveVisitIndex(broken, first.Id);
            long brokenRevision = broken.Revision;
            CustomerVisitRecord second = CreateBrowsingVisit(brokenVisits, "second", 3);

            Assert.That(broken.ValidateInvariants().Error,
                Is.EqualTo(CustomerConsultationFailures.InvariantViolation));
            Assert.That(broken.RecordConsultation(
                Consultation("second"),
                second,
                Time(5)).Error, Is.EqualTo(CustomerConsultationFailures.InvariantViolation));
            Assert.That(broken.Revision, Is.EqualTo(brokenRevision));
            Assert.That(broken.Count, Is.EqualTo(1));
            Assert.That(broken.TryGetConsultation(Consultation("second"), out _), Is.False);
        }

        [Test]
        public void ForeignAndHistoricalVisitSnapshotsFailWithoutMutation()
        {
            CustomerVisitAuthority localVisits = CreateVisitAuthority();
            CustomerVisitAuthority foreignVisits = CreateVisitAuthority();
            CustomerVisitRecord local = CreateBrowsingVisit(localVisits, "shared", 1);
            CustomerVisitRecord foreign = CreateBrowsingVisit(foreignVisits, "shared", 1);
            CustomerConsultationAuthority authority = CreateAuthority(localVisits);

            Assert.That(authority.RecordConsultation(
                Consultation("foreign"),
                foreign,
                Time(3)).Error, Is.EqualTo(CustomerConsultationFailures.VisitStale));
            Assert.That(localVisits.AdvanceTime(Time(8)).IsSuccess, Is.True);
            Assert.That(authority.RecordConsultation(
                Consultation("historical"),
                local,
                Time(8)).Error, Is.EqualTo(CustomerConsultationFailures.VisitStale));

            Assert.That(authority.Revision, Is.Zero);
            Assert.That(authority.Count, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ConsultationBeforeVisitAuthorityObservedTimeFailsWithoutMutation()
        {
            CustomerVisitAuthority visits = CreateVisitAuthority();
            CustomerVisitRecord browsing = CreateBrowsingVisit(visits, "observed-time", 1);
            CustomerConsultationAuthority authority = CreateAuthority(visits);
            Assert.That(visits.AdvanceTime(Time(6)).IsSuccess, Is.True);
            Assert.That(visits.TryGetVisit(browsing.Id, out CustomerVisitRecord current), Is.True);
            Assert.That(current, Is.SameAs(browsing));

            Assert.That(authority.RecordConsultation(
                Consultation("observed-time"),
                browsing,
                Time(3)).Error,
                Is.EqualTo(CustomerConsultationFailures.NonMonotonicTimestamp));

            Assert.That(authority.Revision, Is.Zero);
            Assert.That(authority.Count, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static CustomerConsultationAuthority CreateAuthority(
            CustomerVisitAuthority visits)
        {
            return CustomerConsultationAuthority.Create(visits).Value;
        }

        private static CustomerVisitRecord CreateBrowsingVisit(
            CustomerVisitAuthority visits,
            string suffix,
            long startSecond)
        {
            StableId<CustomerVisitIdScope> visitId = Visit(suffix);
            Assert.That(visits.StartVisit(
                visitId,
                Intent(suffix),
                Customer(suffix),
                ProductId,
                CustomerNeedKind.GraphicsUpgrade,
                Time(startSecond)).IsSuccess, Is.True);
            Assert.That(visits.MarkBrowseArrival(
                visitId,
                Time(startSecond + 1)).IsSuccess, Is.True);
            Assert.That(visits.TryGetVisit(visitId, out CustomerVisitRecord visit), Is.True);
            return visit;
        }

        private static CustomerVisitRecord CreateEnteringVisit(
            CustomerVisitAuthority visits,
            string suffix,
            long startSecond)
        {
            StableId<CustomerVisitIdScope> visitId = Visit(suffix);
            Assert.That(visits.StartVisit(
                visitId,
                Intent(suffix),
                Customer(suffix),
                ProductId,
                CustomerNeedKind.GraphicsUpgrade,
                Time(startSecond)).IsSuccess, Is.True);
            Assert.That(visits.TryGetVisit(visitId, out CustomerVisitRecord visit), Is.True);
            return visit;
        }

        private static CustomerVisitAuthority CreateVisitAuthority()
        {
            ProductDefinition product = ProductDefinition.Create(
                ProductId,
                StableId<ProductCategoryIdScope>.Parse("catalog.category.gpu"),
                "Consultation Test GPU",
                ProductTrackingPolicy.SerializedInstance,
                365).Value;
            ProductCatalog catalog = ProductCatalog.Create(new[] { product }).Value;
            return CustomerVisitAuthority.Create(
                catalog,
                SimulationDuration.FromMilliseconds(5_000),
                CustomerVisitAuthority.RequiredRouteAttemptLimit).Value;
        }

        private static StableId<CustomerConsultationIdScope> Consultation(string suffix)
        {
            return StableId<CustomerConsultationIdScope>.Parse(
                $"actors.consultation.{suffix}");
        }

        private static StableId<CustomerVisitIdScope> Visit(string suffix)
        {
            return StableId<CustomerVisitIdScope>.Parse($"actors.visit.{suffix}");
        }

        private static StableId<CustomerIntentIdScope> Intent(string suffix)
        {
            return StableId<CustomerIntentIdScope>.Parse($"actors.intent.{suffix}");
        }

        private static StableId<CustomerIdScope> Customer(string suffix)
        {
            return StableId<CustomerIdScope>.Parse($"actors.customer.{suffix}");
        }

        private static SimulationTimestamp Time(long second)
        {
            return SimulationTimestamp.Create(second, second * 1_000L);
        }

        private static void SetRevision(
            CustomerConsultationAuthority authority,
            long revision)
        {
            FieldInfo field = typeof(CustomerConsultationAuthority).GetField(
                "<Revision>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(authority, revision);
        }

        private static void RemoveVisitIndex(
            CustomerConsultationAuthority authority,
            StableId<CustomerVisitIdScope> visitId)
        {
            FieldInfo field = typeof(CustomerConsultationAuthority).GetField(
                "_consultationsByVisit",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            var index = (Dictionary<StableId<CustomerVisitIdScope>,
                CustomerConsultationRecord>)field.GetValue(authority);
            Assert.That(index.Remove(visitId), Is.True);
        }
    }
}
