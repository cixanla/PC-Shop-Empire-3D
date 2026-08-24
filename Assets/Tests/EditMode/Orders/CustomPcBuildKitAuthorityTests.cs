using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed class CustomPcBuildKitAuthorityTests
    {
        private const string BuildKitContainerIdValue =
            "inventory.container.custom-pc-build-kit-test";
        private const string OperationIdValue =
            "orders.custom-pc-build-kit-operation.test-motherboard";

        [Test]
        public void PickupSelectsCanonicalMotherboardByKindAndMovesExactReservedItemToHands()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildOrderLineSnapshot motherboard = CanonicalMotherboard(
                fixture.WorkOrder);
            Assert.That(fixture.WorkOrder.Lines[0].ComponentKind,
                Is.Not.EqualTo(PcComponentKind.Motherboard),
                "Ordinal line zero must never act as the motherboard selector.");
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long assemblyRevision = fixture.Session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitReceipt> result =
                fixture.BuildKit.PickupCanonicalMotherboard(
                    OperationId(),
                    fixture.WorkOrder);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Line, Is.SameAs(motherboard));
            Assert.That(result.Value.Line.LineId, Is.EqualTo(motherboard.LineId));
            Assert.That(result.Value.Line.ProductId, Is.EqualTo(motherboard.ProductId));
            Assert.That(result.Value.Line.ItemId, Is.EqualTo(motherboard.ItemId));
            Assert.That(result.Value.Line.ReservationId,
                Is.EqualTo(motherboard.ReservationId));
            Assert.That(result.Value.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardInHands));
            Assert.That(fixture.Session.Inventory.TryGetSerializedItem(
                motherboard.ItemId,
                out InventoryItemRecord item), Is.True);
            Assert.That(item.ContainerId, Is.EqualTo(fixture.Session.HandsContainerId));
            Assert.That(fixture.Session.Inventory.TryGetReservation(
                motherboard.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(motherboard.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(fixture.WorkOrder.InventoryClaimId));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(1));
            Assert.That(fixture.BuildKit.StagedComponentCount, Is.Zero);
            AssertInvariants(fixture);
        }

        [Test]
        public void ExactPickupReplayReturnsSameReceiptWithoutAnyRevisionChange()
        {
            Fixture fixture = CreateFixture();
            OperationResult<CustomPcBuildKitReceipt> first =
                fixture.BuildKit.PickupCanonicalMotherboard(
                    OperationId(),
                    fixture.WorkOrder);
            Assert.That(first.IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> replay =
                fixture.BuildKit.PickupCanonicalMotherboard(
                    OperationId(),
                    fixture.WorkOrder);

            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(first.Value));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            AssertInvariants(fixture);
        }

        [Test]
        public void SecondOperationForSameWorkOrderFailsBeforeMutation()
        {
            Fixture fixture = CreateFixture();
            Assert.That(fixture.BuildKit.PickupCanonicalMotherboard(
                OperationId(),
                fixture.WorkOrder).IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> conflict =
                fixture.BuildKit.PickupCanonicalMotherboard(
                    StableId<CustomPcBuildKitOperationIdScope>.Parse(
                        OperationIdValue + ".conflict"),
                    fixture.WorkOrder);

            Assert.That(conflict.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitIdentityConflict));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            AssertInvariants(fixture);
        }

        [Test]
        public void FullHandsFailsPickupWithoutAnyMutation()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildOrderLineSnapshot other = fixture.WorkOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            Assert.That(fixture.Session.Inventory.TransferSerializedItem(
                other.ItemId,
                fixture.Session.HandsContainerId).IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> result =
                fixture.BuildKit.PickupCanonicalMotherboard(
                    OperationId(),
                    fixture.WorkOrder);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(fixture.BuildKit.ActiveKitCount, Is.Zero);
            AssertInvariants(fixture);
        }

        [Test]
        public void PickupRejectsReservedMotherboardOutsideExactWorldFloorSource()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildOrderLineSnapshot motherboard = CanonicalMotherboard(
                fixture.WorkOrder);
            Assert.That(fixture.Session.Inventory.TransferSerializedItem(
                motherboard.ItemId,
                fixture.Session.ShelfContainerId).IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> result =
                fixture.BuildKit.PickupCanonicalMotherboard(
                    OperationId(),
                    fixture.WorkOrder);

            Assert.That(result.Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitStageInvalid));
            Assert.That(fixture.Session.Inventory.TryGetSerializedItem(
                motherboard.ItemId,
                out InventoryItemRecord item), Is.True);
            Assert.That(item.ContainerId, Is.EqualTo(fixture.Session.ShelfContainerId));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(fixture.BuildKit.ActiveKitCount, Is.Zero);
            AssertInvariants(fixture);
        }

        [Test]
        public void GenericWorldDropCannotEscapeActiveBuildKitPickup()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildOrderLineSnapshot motherboard = CanonicalMotherboard(
                fixture.WorkOrder);
            Assert.That(fixture.BuildKit.PickupCanonicalMotherboard(
                OperationId(),
                fixture.WorkOrder).IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            OperationResult result = fixture.Session.DropHeldMotherboardToWorld();

            Assert.That(result.Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
            Assert.That(fixture.Session.Inventory.TryGetSerializedItem(
                motherboard.ItemId,
                out InventoryItemRecord item), Is.True);
            Assert.That(item.ContainerId, Is.EqualTo(fixture.Session.HandsContainerId));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            AssertInvariants(fixture);
        }

        [Test]
        public void PlaceMovesMotherboardToManagedBuildKitAndPreservesLiveReservation()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildOrderLineSnapshot motherboard = CanonicalMotherboard(
                fixture.WorkOrder);
            OperationResult<CustomPcBuildKitReceipt> pickup =
                fixture.BuildKit.PickupCanonicalMotherboard(
                    OperationId(),
                    fixture.WorkOrder);
            Assert.That(pickup.IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long assemblyRevision = fixture.Session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitReceipt> placement =
                fixture.BuildKit.PlaceCanonicalMotherboard(pickup.Value);

            Assert.That(placement.IsSuccess, Is.True);
            Assert.That(placement.Value.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardStaged));
            Assert.That(placement.Value.StagedComponentCount, Is.EqualTo(1));
            Assert.That(fixture.BuildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(fixture.Session.Inventory.TryGetSerializedItem(
                motherboard.ItemId,
                out InventoryItemRecord item), Is.True);
            Assert.That(item.ContainerId, Is.EqualTo(fixture.BuildKitContainerId));
            Assert.That(fixture.Session.Inventory.TryGetReservation(
                motherboard.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(motherboard.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(fixture.WorkOrder.InventoryClaimId));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(2));
            AssertInvariants(fixture);
        }

        [Test]
        public void ExactPlacementReplayReturnsSameReceiptWithoutAnyRevisionChange()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildKitReceipt pickup = fixture.BuildKit.PickupCanonicalMotherboard(
                OperationId(),
                fixture.WorkOrder).Value;
            OperationResult<CustomPcBuildKitReceipt> first =
                fixture.BuildKit.PlaceCanonicalMotherboard(pickup);
            Assert.That(first.IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> replay =
                fixture.BuildKit.PlaceCanonicalMotherboard(pickup);

            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(first.Value));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            AssertInvariants(fixture);
        }

        [Test]
        public void GenericTransferCannotBypassManagedBuildKitCustody()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildOrderLineSnapshot motherboard = CanonicalMotherboard(
                fixture.WorkOrder);
            CustomPcBuildKitReceipt pickup = fixture.BuildKit.PickupCanonicalMotherboard(
                OperationId(),
                fixture.WorkOrder).Value;
            Assert.That(fixture.BuildKit.PlaceCanonicalMotherboard(pickup).IsSuccess,
                Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;

            OperationResult transfer = fixture.Session.Inventory.TransferSerializedItem(
                motherboard.ItemId,
                fixture.Session.WorldFloorContainerId);

            Assert.That(transfer.Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            AssertInvariants(fixture);
        }

        [Test]
        public void ForeignPickupReceiptCannotBePlacedByAnotherAuthority()
        {
            Fixture owned = CreateFixture();
            Fixture foreign = CreateFixture();
            CustomPcBuildKitReceipt foreignPickup =
                foreign.BuildKit.PickupCanonicalMotherboard(
                    OperationId(),
                    foreign.WorkOrder).Value;
            long inventoryRevision = owned.Session.Inventory.Revision;
            long buildKitRevision = owned.BuildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> result =
                owned.BuildKit.PlaceCanonicalMotherboard(foreignPickup);

            Assert.That(result.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitReceiptInvalid));
            Assert.That(owned.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(owned.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            AssertInvariants(owned);
            AssertInvariants(foreign);
        }

        private static Fixture CreateFixture()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            Assert.That(session.StartPrototypeCustomerVisit(Time(10)).IsSuccess, Is.True);
            Assert.That(session.MarkPrototypeCustomerBrowseArrival(Time(11)).IsSuccess,
                Is.True);
            Assert.That(session.ConsultPrototypeCustomer(Time(12)).IsSuccess, Is.True);
            Assert.That(session.AcceptPrototypeCustomPcRequest(Time(13)).IsSuccess, Is.True);
            Assert.That(session.CreatePrototypeCustomPcQuote(Time(14)).IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            OperationResult<CustomPcWorkOrderIssueResult> issued =
                session.CustomPcWorkOrders.Issue(
                    IssueAccess(session.CustomPcWorkOrders),
                    session.PrototypeCustomPcBuildOrderId,
                    session.PrototypeCustomPcWorkTicketId,
                    session.PrototypeCustomPcWorkOrderOperationId,
                    quote,
                    Time(15));
            Assert.That(issued.IsSuccess, Is.True);

            StableId<ContainerIdScope> buildKitContainerId =
                StableId<ContainerIdScope>.Parse(BuildKitContainerIdValue);
            Assert.That(session.Inventory.RegisterContainer(
                InventoryContainerDefinition.Create(
                    buildKitContainerId,
                    InventoryContainerKind.BuildKit,
                    1).Value).IsSuccess, Is.True);
            OperationResult<CustomPcBuildKitAuthority> buildKit =
                CustomPcBuildKitAuthority.Create(
                    session.CustomPcWorkOrders,
                    session.WorldFloorContainerId,
                    session.HandsContainerId,
                    buildKitContainerId);
            Assert.That(buildKit.IsSuccess, Is.True);

            return new Fixture(
                session,
                issued.Value.BuildOrder,
                buildKit.Value,
                buildKitContainerId);
        }

        private static CustomPcBuildOrderLineSnapshot CanonicalMotherboard(
            CustomPcBuildOrderRecord workOrder)
        {
            return workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
        }

        private static void AssertInvariants(Fixture fixture)
        {
            Assert.That(fixture.Session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Session.CustomPcWorkOrders.ValidateInvariants().IsSuccess,
                Is.True);
            Assert.That(fixture.BuildKit.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static StableId<CustomPcBuildKitOperationIdScope> OperationId()
        {
            return StableId<CustomPcBuildKitOperationIdScope>.Parse(OperationIdValue);
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
            Assert.That(field, Is.Not.Null);
            return (CustomPcWorkOrderIssueAccess)field.GetValue(authority);
        }

        private sealed class Fixture
        {
            internal Fixture(
                GarageStockFlowSession session,
                CustomPcBuildOrderRecord workOrder,
                CustomPcBuildKitAuthority buildKit,
                StableId<ContainerIdScope> buildKitContainerId)
            {
                Session = session;
                WorkOrder = workOrder;
                BuildKit = buildKit;
                BuildKitContainerId = buildKitContainerId;
            }

            internal GarageStockFlowSession Session { get; }

            internal CustomPcBuildOrderRecord WorkOrder { get; }

            internal CustomPcBuildKitAuthority BuildKit { get; }

            internal StableId<ContainerIdScope> BuildKitContainerId { get; }
        }
    }
}
