using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Retail
{
    public sealed class CustomPcQuoteAuthorityTests
    {
        [Test]
        public void AcceptedRequestCreatesImmutableTenLineQuoteAndAtomicReservations()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcRequest(
                out CustomPcRequestRecord request), Is.True);
            Assert.That(request.CustomerBinding, Is.EqualTo(session.PrototypeCustomerBinding));
            Assert.That(request.Profile,
                Is.EqualTo(CustomPcBuildProfile.GraphicsFirstGaming));
            Assert.That(request.MaximumBudget.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeCustomPcMaximumBudgetMinorUnits));
            Assert.That(request.MaximumBudget.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(1));
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            Assert.That(session.CreatePrototypeCustomPcQuote(Time(14)).IsSuccess, Is.True);

            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(2));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.Inventory.ReservationCount,
                Is.EqualTo(reservationCount + CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(quote.Request, Is.SameAs(request));
            Assert.That(quote.InventoryClaimId, Is.EqualTo(session.PrototypeCustomPcClaimId));
            Assert.That(quote.TotalPrice.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeCustomPcTotalPriceMinorUnits));
            Assert.That(quote.TotalPrice.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(quote.ReservedSerializedItemCount,
                Is.EqualTo(CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(quote.Lines.Select(line => line.LineId.Value),
                Is.Ordered.Using<string>(System.StringComparer.Ordinal));
            Assert.That(quote.Lines.Select(line => line.ItemId).Distinct().Count(),
                Is.EqualTo(quote.Lines.Count));
            Assert.That(quote.Lines.Select(line => line.ReservationId).Distinct().Count(),
                Is.EqualTo(quote.Lines.Count));

            foreach (CustomPcQuoteLineSnapshot line in quote.Lines)
            {
                Assert.That(session.Inventory.TryGetSerializedItem(
                    line.ItemId, out InventoryItemRecord item), Is.True);
                Assert.That(item.ProductId, Is.EqualTo(line.ProductId));
                Assert.That(item.UnitCost, Is.EqualTo(line.UnitCost));
                Assert.That(session.Inventory.TryGetReservation(
                    line.ReservationId, out InventoryReservation reservation), Is.True);
                Assert.That(reservation.ClaimId, Is.EqualTo(quote.InventoryClaimId));
                Assert.That(reservation.ItemId, Is.EqualTo(line.ItemId));
            }

            Assert.That(session.CustomPcQuotes.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ExactRequestAndQuoteReplayDoNotAdvanceAnyRevision()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.CreatePrototypeCustomPcQuote(Time(14)).IsSuccess, Is.True);
            long quoteRevision = session.CustomPcQuotes.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.CreatePrototypeCustomPcQuote(Time(14)).IsSuccess, Is.True);

            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.CustomPcQuotes.RequestCount, Is.EqualTo(1));
            Assert.That(session.CustomPcQuotes.QuoteCount, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BudgetExceededFailsBeforeAnyReservationOrQuoteMutation()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            CustomPcQuoteLineDraft[] lines =
                session.CreatePrototypeCustomPcQuoteLines().ToArray();
            CustomPcQuoteLineDraft first = lines[0];
            lines[0] = CustomPcQuoteLineDraft.Create(
                first.LineId,
                first.ProductId,
                first.ItemId,
                first.ReservationId,
                ShelfPrice.Create(
                    GarageStockFlowSession.PrototypeCurrencyCode,
                    100_000).Value).Value;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult result = session.CustomPcQuotes.CreateQuoteAndReserve(
                session.PrototypeCustomPcQuoteId,
                session.PrototypeCustomPcRequestId,
                session.PrototypeCustomPcClaimId,
                lines,
                Time(14));

            Assert.That(result.Error, Is.EqualTo(CustomPcQuoteFailures.BudgetExceeded));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.CustomPcQuotes.QuoteCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.TryGetPrototypeCustomPcQuote(out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MissingRequiredBomRoleFailsBeforeAnyReservationOrQuoteMutation()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            CustomPcQuoteLineDraft[] incompleteLines =
                session.CreatePrototypeCustomPcQuoteLines()
                    .Where(line => line.ProductId != session.ProcessorProductId)
                    .ToArray();
            Assert.That(incompleteLines.Length,
                Is.EqualTo(CustomPcQuoteAuthority.GraphicsFirstGamingLineCount - 1));
            long quoteRevision = session.CustomPcQuotes.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult result = session.CustomPcQuotes.CreateQuoteAndReserve(
                session.PrototypeCustomPcQuoteId,
                session.PrototypeCustomPcRequestId,
                session.PrototypeCustomPcClaimId,
                incompleteLines,
                Time(14));

            Assert.That(result.Error,
                Is.EqualTo(CustomPcQuoteFailures.ComponentSetInvalid));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.CustomPcQuotes.QuoteCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.TryGetPrototypeCustomPcQuote(out _), Is.False);
            Assert.That(session.CustomPcQuotes.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void IncompatibleProcessorSocketFailsBeforeAnyReservationOrQuoteMutation()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            PcComponentSpecification incompatibleProcessor =
                PcComponentSpecification.CreateProcessor(
                    session.Catalog,
                    session.ProcessorProductId,
                    CpuSocketFamily.Am5).Value;
            PcComponentCatalog incompatibleComponents = PcComponentCatalog.Create(
                session.Catalog,
                session.Components.Specifications.Select(specification =>
                    specification.ProductId == session.ProcessorProductId
                        ? incompatibleProcessor
                        : specification)).Value;
            CustomPcQuoteAuthority quotes = CustomPcQuoteAuthority.Create(
                session.Catalog,
                incompatibleComponents,
                session.Inventory,
                session.CustomerConsultations).Value;
            CustomerConsultationRecord consultation =
                session.CustomerConsultations.GetConsultations().Single();
            Assert.That(quotes.AcceptRequest(
                session.PrototypeCustomPcRequestId,
                session.PrototypeCustomerBinding,
                consultation,
                CustomPcBuildProfile.GraphicsFirstGaming,
                ShelfPrice.Create(
                    GarageStockFlowSession.PrototypeCurrencyCode,
                    GarageStockFlowSession.PrototypeCustomPcMaximumBudgetMinorUnits).Value,
                Time(13)).IsSuccess, Is.True);
            long quoteRevision = quotes.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult result = quotes.CreateQuoteAndReserve(
                session.PrototypeCustomPcQuoteId,
                session.PrototypeCustomPcRequestId,
                session.PrototypeCustomPcClaimId,
                session.CreatePrototypeCustomPcQuoteLines(),
                Time(14));

            Assert.That(result.Error,
                Is.EqualTo(CustomPcQuoteFailures.ComponentIncompatible));
            Assert.That(quotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(quotes.QuoteCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(quotes.TryGetQuote(session.PrototypeCustomPcQuoteId, out _), Is.False);
            Assert.That(quotes.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void OneConflictingComponentFailsWithoutPartialBomReservation()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.Inventory.ReserveSerializedItem(
                StableId<ReservationIdScope>.Parse(
                    "inventory.reservation.custom-pc-test.external"),
                StableId<InventoryClaimIdScope>.Parse(
                    "inventory.claim.custom-pc-test.external"),
                session.MotherboardItemId).IsSuccess, Is.True);
            long quoteRevision = session.CustomPcQuotes.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult result = session.CreatePrototypeCustomPcQuote(Time(14));

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ItemAlreadyReserved));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.CustomPcQuotes.QuoteCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            foreach (CustomPcQuoteLineDraft line in session.CreatePrototypeCustomPcQuoteLines())
            {
                Assert.That(session.Inventory.TryGetReservation(
                    line.ReservationId, out _), Is.False);
            }

            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RequestIdentityConflictFailsWithoutReplacingAcceptedProvenance()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            long revision = session.CustomPcQuotes.Revision;

            OperationResult result = session.AcceptPrototypeCustomPcRequest(Time(14));

            Assert.That(result.Error,
                Is.EqualTo(CustomPcQuoteFailures.RequestIdentityConflict));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(revision));
            Assert.That(session.CustomPcQuotes.RequestCount, Is.EqualTo(1));
            Assert.That(session.TryGetPrototypeCustomPcRequest(
                out CustomPcRequestRecord request), Is.True);
            Assert.That(request.AcceptedAt, Is.EqualTo(Time(13)));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FirstQuoteCannotAdoptExternallyReservedExactSetWithoutOwnedReceipt()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            CustomPcQuoteLineDraft[] lines =
                session.CreatePrototypeCustomPcQuoteLines().ToArray();
            InventorySerializedReservationRequest[] reservations = lines
                .Select(line => InventorySerializedReservationRequest.Create(
                    line.ReservationId,
                    session.PrototypeCustomPcClaimId,
                    line.ItemId).Value)
                .ToArray();
            Assert.That(session.Inventory.ReserveSerializedItems(reservations).IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;

            OperationResult result = session.CustomPcQuotes.CreateQuoteAndReserve(
                session.PrototypeCustomPcQuoteId,
                session.PrototypeCustomPcRequestId,
                session.PrototypeCustomPcClaimId,
                lines,
                Time(14));

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationClaimOccupied));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.CustomPcQuotes.QuoteCount, Is.Zero);
            Assert.That(session.CustomPcQuotes.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void QuoteClaimRejectsEleventhReservationWithoutMutation()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.CreatePrototypeCustomPcQuote(Time(14)).IsSuccess, Is.True);
            long revision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult result = session.Inventory.ReserveSerializedItem(
                StableId<ReservationIdScope>.Parse(
                    "inventory.reservation.custom-pc.demo-gaming-001.eleventh"),
                session.PrototypeCustomPcClaimId,
                session.ItemId);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationClaimManaged));
            Assert.That(session.Inventory.Revision, Is.EqualTo(revision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void DirectReleaseOrConsumeCannotBreakOwnedQuoteReservationSet()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.CreatePrototypeCustomPcQuote(Time(14)).IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            StableId<ReservationIdScope> reservationId = quote.Lines[0].ReservationId;
            long revision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult release = session.Inventory.ReleaseReservation(reservationId);
            OperationResult consume = session.Inventory.ConsumeReservation(reservationId);

            Assert.That(release.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationClaimManaged));
            Assert.That(consume.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationClaimManaged));
            Assert.That(session.Inventory.Revision, Is.EqualTo(revision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.Inventory.TryGetReservation(reservationId, out _), Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void QuoteRetryAfterInventoryManagedCommitBeforeQuotePublicationRecoversExactOwnedAccess()
        {
            GarageStockFlowSession session = CreateConsultedSession();
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            CustomPcQuoteLineDraft[] lines =
                session.CreatePrototypeCustomPcQuoteLines().ToArray();
            InventorySerializedReservationRequest[] reservations = lines
                .Select(line => InventorySerializedReservationRequest.Create(
                    line.ReservationId,
                    session.PrototypeCustomPcClaimId,
                    line.ItemId).Value)
                .ToArray();
            StableId<InventorySerializedReservationSetOperationIdScope> operationId =
                CustomPcQuoteAuthority.CreateInventoryReservationSetOperationId(
                    session.PrototypeCustomPcQuoteId);
            OperationResult<InventorySerializedReservationSetAccess> inventoryCommit =
                session.Inventory.ReserveManagedSerializedItems(
                    operationId,
                    reservations);
            Assert.That(inventoryCommit.IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;
            long quoteRevision = session.CustomPcQuotes.Revision;

            OperationResult retry = session.CustomPcQuotes.CreateQuoteAndReserve(
                session.PrototypeCustomPcQuoteId,
                session.PrototypeCustomPcRequestId,
                session.PrototypeCustomPcClaimId,
                lines,
                Time(14));

            Assert.That(retry.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision + 1));
            Assert.That(session.CustomPcQuotes.QuoteCount, Is.EqualTo(1));
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(quote.InventoryClaimId, Is.EqualTo(session.PrototypeCustomPcClaimId));
            Assert.That(session.CustomPcQuotes.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static GarageStockFlowSession CreateConsultedSession()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            Assert.That(session.StartPrototypeCustomerVisit(Time(10)).IsSuccess, Is.True);
            Assert.That(session.MarkPrototypeCustomerBrowseArrival(Time(11)).IsSuccess, Is.True);
            Assert.That(session.ConsultPrototypeCustomer(Time(12)).IsSuccess, Is.True);
            return session;
        }

        private static SimulationTimestamp Time(long tick)
        {
            return SimulationTimestamp.Create(tick, tick * 1000L);
        }
    }
}
