using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed class CustomPcWorkOrderAuthorityTests
    {
        [Test]
        public void IssueAllocatesExactQuoteSetWithoutMovingOrConsumingAnyItem()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int itemCount = session.Inventory.SerializedItemCount;
            int reservationCount = session.Inventory.ReservationCount;
            string[] itemContainers = quote.Lines
                .Select(line =>
                {
                    Assert.That(session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item), Is.True);
                    return item.ContainerId.Value;
                })
                .ToArray();

            OperationResult<CustomPcWorkOrderIssueResult> issued =
                Issue(session, Time(15));

            Assert.That(issued.IsSuccess, Is.True);
            Assert.That(session.CustomPcWorkOrders.Revision, Is.EqualTo(1));
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.EqualTo(1));
            Assert.That(session.CustomPcWorkOrders.WorkTicketCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.SerializedItemCount, Is.EqualTo(itemCount));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(issued.Value.BuildOrder.SourceQuote, Is.SameAs(quote));
            Assert.That(issued.Value.BuildOrder.Status,
                Is.EqualTo(CustomPcBuildOrderStatus.ReservationSetAllocated));
            Assert.That(issued.Value.WorkTicket.Status,
                Is.EqualTo(CustomPcWorkTicketStatus.PostedAtWorkbenchStation));
            Assert.That(issued.Value.BuildOrder.ReservedSerializedItemCount,
                Is.EqualTo(CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(issued.Value.BuildOrder.WorkbenchContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(issued.Value.BuildOrder.Lines.Select(line => line.ItemId),
                Is.EqualTo(quote.Lines.Select(line => line.ItemId)));
            Assert.That(issued.Value.BuildOrder.Lines.Select(line => line.ReservationId),
                Is.EqualTo(quote.Lines.Select(line => line.ReservationId)));

            for (int index = 0; index < quote.Lines.Count; index++)
            {
                CustomPcQuoteLineSnapshot source = quote.Lines[index];
                Assert.That(session.Inventory.TryGetSerializedItem(
                    source.ItemId,
                    out InventoryItemRecord item), Is.True);
                Assert.That(item.ContainerId.Value, Is.EqualTo(itemContainers[index]));
                Assert.That(session.Inventory.TryGetReservation(
                    source.ReservationId,
                    out InventoryReservation reservation), Is.True);
                Assert.That(reservation.ClaimId, Is.EqualTo(quote.InventoryClaimId));
                Assert.That(reservation.ItemId, Is.EqualTo(source.ItemId));
            }

            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ExactIssueReplayReturnsSameRecordsWithoutAnyRevisionChange()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            OperationResult<CustomPcWorkOrderIssueResult> first =
                Issue(session, Time(15));
            Assert.That(first.IsSuccess, Is.True);
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcWorkOrderIssueResult> replay =
                Issue(session, Time(15));

            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(first.Value));
            Assert.That(replay.Value.BuildOrder, Is.SameAs(first.Value.BuildOrder));
            Assert.That(replay.Value.WorkTicket, Is.SameAs(first.Value.WorkTicket));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SameOperationWithDifferentPayloadFailsBeforeMutation()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            Assert.That(Issue(session, Time(15)).IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcWorkOrderIssueResult> conflict =
                session.CustomPcWorkOrders.Issue(
                    IssueAccess(session.CustomPcWorkOrders),
                    StableId<CustomPcBuildOrderIdScope>.Parse(
                        "orders.custom-pc-build-order.demo-gaming-conflict"),
                    session.PrototypeCustomPcWorkTicketId,
                    session.PrototypeCustomPcWorkOrderOperationId,
                    quote,
                    Time(15));

            Assert.That(conflict.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.IdentityConflict));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RetryAfterInventoryAllocationBeforeOrderPublicationRecoversExactly()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<InventorySerializedReservationWorkOrderAllocationReceipt>
                prepared = session.CustomPcWorkOrders.PrepareInventoryAllocationForRecovery(
                    IssueAccess(session.CustomPcWorkOrders),
                    session.PrototypeCustomPcBuildOrderId,
                    session.PrototypeCustomPcWorkTicketId,
                    session.PrototypeCustomPcWorkOrderOperationId,
                    quote);
            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcWorkOrders.Revision, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            long committedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcWorkOrderIssueResult> recovered =
                Issue(session, Time(15));

            Assert.That(recovered.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(committedInventoryRevision));
            Assert.That(recovered.Value.BuildOrder.InventoryAllocationRevision,
                Is.EqualTo(prepared.Value.AppliedRevision));
            Assert.That(session.CustomPcWorkOrders.Revision, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MismatchedRetryAfterInventoryAllocationFailsWithoutPublishingOrder()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(session.CustomPcWorkOrders.PrepareInventoryAllocationForRecovery(
                IssueAccess(session.CustomPcWorkOrders),
                session.PrototypeCustomPcBuildOrderId,
                session.PrototypeCustomPcWorkTicketId,
                session.PrototypeCustomPcWorkOrderOperationId,
                quote).IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcWorkOrderIssueResult> conflict =
                session.CustomPcWorkOrders.Issue(
                    IssueAccess(session.CustomPcWorkOrders),
                    session.PrototypeCustomPcBuildOrderId,
                    StableId<CustomPcWorkTicketIdScope>.Parse(
                        "orders.custom-pc-work-ticket.demo-gaming-conflict"),
                    session.PrototypeCustomPcWorkOrderOperationId,
                    quote,
                    Time(15));

            Assert.That(conflict.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationWorkOrderConflict));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcWorkOrders.Revision, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void AllocationKeepsManagedReservationsProtectedFromReleaseAndConsume()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            Assert.That(Issue(session, Time(15)).IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            StableId<ReservationIdScope> reservationId = quote.Lines[0].ReservationId;
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult release = session.Inventory.ReleaseReservation(reservationId);
            OperationResult consume = session.Inventory.ConsumeReservation(reservationId);

            Assert.That(release.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationClaimManaged));
            Assert.That(consume.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationClaimManaged));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.Inventory.TryGetReservation(reservationId, out _), Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ForeignQuoteAndTimestampRegressionFailBeforeAnyMutation()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            GarageStockFlowSession foreign = CreateQuotedSession();
            Assert.That(foreign.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord foreignQuote), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord ownedQuote), Is.True);
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcWorkOrderIssueResult> foreignResult =
                session.CustomPcWorkOrders.Issue(
                    IssueAccess(session.CustomPcWorkOrders),
                    session.PrototypeCustomPcBuildOrderId,
                    session.PrototypeCustomPcWorkTicketId,
                    session.PrototypeCustomPcWorkOrderOperationId,
                    foreignQuote,
                    Time(15));
            OperationResult<CustomPcWorkOrderIssueResult> timestampResult =
                session.CustomPcWorkOrders.Issue(
                    IssueAccess(session.CustomPcWorkOrders),
                    session.PrototypeCustomPcBuildOrderId,
                    session.PrototypeCustomPcWorkTicketId,
                    session.PrototypeCustomPcWorkOrderOperationId,
                    ownedQuote,
                    Time(13));

            Assert.That(foreignResult.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.QuoteReservationDrift));
            Assert.That(timestampResult.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.TimestampInvalid));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void AuthorityCreationRejectsNonWorkbenchTarget()
        {
            GarageStockFlowSession session = CreateQuotedSession();

            OperationResult<CustomPcWorkOrderAuthorityCreation> result =
                CustomPcWorkOrderAuthority.Create(
                    session.CustomPcQuotes,
                    session.ShelfContainerId);

            Assert.That(result.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.WorkbenchInvalid));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void AuthorityCreationRejectsSecondPublisherForSameQuoteAuthorityAndWorkbench()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;

            OperationResult<CustomPcWorkOrderAuthorityCreation> duplicate =
                CustomPcWorkOrderAuthority.Create(
                    session.CustomPcQuotes,
                    session.WorkbenchContainerId);

            Assert.That(duplicate.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.PublisherAlreadyRegistered));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkTicketCount, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RawAuthorityIssueWithoutOpaqueAccessFailsBeforeAnyMutation()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            int itemCount = session.Inventory.SerializedItemCount;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult<CustomPcWorkOrderIssueResult> result =
                session.CustomPcWorkOrders.Issue(
                    null,
                    session.PrototypeCustomPcBuildOrderId,
                    session.PrototypeCustomPcWorkTicketId,
                    session.PrototypeCustomPcWorkOrderOperationId,
                    quote,
                    Time(15));

            Assert.That(result.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.IssueAccessInvalid));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkTicketCount, Is.Zero);
            Assert.That(session.Inventory.SerializedItemCount, Is.EqualTo(itemCount));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RawAuthorityIssueWithForeignOpaqueAccessFailsBeforeAnyMutation()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            GarageStockFlowSession foreign = CreateQuotedSession();
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            int itemCount = session.Inventory.SerializedItemCount;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult<CustomPcWorkOrderIssueResult> result =
                session.CustomPcWorkOrders.Issue(
                    IssueAccess(foreign.CustomPcWorkOrders),
                    session.PrototypeCustomPcBuildOrderId,
                    session.PrototypeCustomPcWorkTicketId,
                    session.PrototypeCustomPcWorkOrderOperationId,
                    quote,
                    Time(15));

            Assert.That(result.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.IssueAccessInvalid));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkTicketCount, Is.Zero);
            Assert.That(session.Inventory.SerializedItemCount, Is.EqualTo(itemCount));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RecoveryPreparationRequiresExactOpaqueAccessBeforeAnyMutation()
        {
            GarageStockFlowSession session = CreateQuotedSession();
            GarageStockFlowSession foreign = CreateQuotedSession();
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            int allocationCount =
                session.Inventory.SerializedReservationWorkOrderAllocationCount;
            int reservationCount = session.Inventory.ReservationCount;

            OperationResult<InventorySerializedReservationWorkOrderAllocationReceipt>
                missing = session.CustomPcWorkOrders
                    .PrepareInventoryAllocationForRecovery(
                        null,
                        session.PrototypeCustomPcBuildOrderId,
                        session.PrototypeCustomPcWorkTicketId,
                        session.PrototypeCustomPcWorkOrderOperationId,
                        quote);
            OperationResult<InventorySerializedReservationWorkOrderAllocationReceipt>
                foreignResult = session.CustomPcWorkOrders
                    .PrepareInventoryAllocationForRecovery(
                        IssueAccess(foreign.CustomPcWorkOrders),
                        session.PrototypeCustomPcBuildOrderId,
                        session.PrototypeCustomPcWorkTicketId,
                        session.PrototypeCustomPcWorkOrderOperationId,
                        quote);

            Assert.That(missing.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.IssueAccessInvalid));
            Assert.That(foreignResult.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.IssueAccessInvalid));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(
                session.Inventory.SerializedReservationWorkOrderAllocationCount,
                Is.EqualTo(allocationCount));
            Assert.That(session.Inventory.ReservationCount,
                Is.EqualTo(reservationCount));
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkTicketCount, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static GarageStockFlowSession CreateQuotedSession()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            Assert.That(session.StartPrototypeCustomerVisit(Time(10)).IsSuccess, Is.True);
            Assert.That(session.MarkPrototypeCustomerBrowseArrival(Time(11)).IsSuccess, Is.True);
            Assert.That(session.ConsultPrototypeCustomer(Time(12)).IsSuccess, Is.True);
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.CreatePrototypeCustomPcQuote(Time(14)).IsSuccess, Is.True);
            return session;
        }

        private static OperationResult<CustomPcWorkOrderIssueResult> Issue(
            GarageStockFlowSession session,
            SimulationTimestamp issuedAt)
        {
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            return session.CustomPcWorkOrders.Issue(
                IssueAccess(session.CustomPcWorkOrders),
                session.PrototypeCustomPcBuildOrderId,
                session.PrototypeCustomPcWorkTicketId,
                session.PrototypeCustomPcWorkOrderOperationId,
                quote,
                issuedAt);
        }

        private static SimulationTimestamp Time(long tick)
        {
            return SimulationTimestamp.Create(tick, tick * 1000L);
        }

        private static CustomPcWorkOrderIssueAccess IssueAccess(
            CustomPcWorkOrderAuthority authority)
        {
            System.Reflection.FieldInfo field =
                typeof(CustomPcWorkOrderAuthority).GetField(
                    "_issueAccess",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null,
                "The test fixture must read the authority-owned capability without " +
                "adding a production accessor.");
            return (CustomPcWorkOrderIssueAccess)field.GetValue(authority);
        }
    }
}
