using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
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
        public void OccupiedBuildKitContainerRejectsAuthorityClaimWithoutAnyMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StableId<ContainerIdScope> buildKitContainerId =
                StableId<ContainerIdScope>.Parse(BuildKitContainerIdValue);
            Assert.That(session.Inventory.RegisterContainer(
                InventoryContainerDefinition.Create(
                    buildKitContainerId,
                    InventoryContainerKind.BuildKit,
                    1).Value).IsSuccess, Is.True);

            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            Assert.That(session.Inventory.TryGetSerializedItem(
                processor.ItemId,
                out InventoryItemRecord processorItem), Is.True);
            StableId<ItemInstanceIdScope> foreignItemId =
                StableId<ItemInstanceIdScope>.Parse(
                    "inventory.item.custom-pc-build-kit-occupied-test");
            Assert.That(session.Inventory.ReceiveSerializedItem(
                foreignItemId,
                processor.ProductId,
                buildKitContainerId,
                InventoryCondition.New,
                processorItem.UnitCost).IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;

            OperationResult<CustomPcBuildKitAuthority> result =
                CustomPcBuildKitAuthority.Create(
                    session.CustomPcWorkOrders,
                    session.WorldFloorContainerId,
                    session.HandsContainerId,
                    buildKitContainerId);

            Assert.That(result.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitContainerInvalid));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.Inventory.TryGetSerializedItem(
                foreignItemId,
                out InventoryItemRecord foreignItem), Is.True);
            Assert.That(foreignItem.ProductId, Is.EqualTo(processor.ProductId));
            Assert.That(foreignItem.ContainerId, Is.EqualTo(buildKitContainerId));
            Assert.That(foreignItem.UnitCost, Is.EqualTo(processorItem.UnitCost));
            Assert.That(session.Inventory.GetContainerQuantity(buildKitContainerId).Value,
                Is.EqualTo(1));
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.CustomPcWorkOrders.ValidateInvariants().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
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
        public void StalePlacementRevisionFailsBeforeAnyCustodyMutation()
        {
            Fixture fixture = CreateFixture();
            CustomPcBuildKitReceipt pickup = fixture.BuildKit.PickupCanonicalMotherboard(
                OperationId(),
                fixture.WorkOrder).Value;
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> staleBuildKit =
                fixture.BuildKit.PlaceCanonicalMotherboard(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision);
            Assert.That(staleBuildKit.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            AssertMotherboardStillInHands(fixture, pickup);

            OperationResult<CustomPcBuildKitReceipt> staleInventory =
                fixture.BuildKit.PlaceCanonicalMotherboard(
                    pickup,
                    buildKitRevision,
                    inventoryRevision - 1);
            Assert.That(staleInventory.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
            AssertMotherboardStillInHands(fixture, pickup);
            AssertInvariants(fixture);
        }

        [Test]
        public void ForgedPickupReceiptIdentityMatrixFailsBeforeAnyMutation()
        {
            Fixture fixture = CreateFixture();
            Fixture foreign = CreateFixture();
            CustomPcBuildKitReceipt pickup = fixture.BuildKit.PickupCanonicalMotherboard(
                OperationId(),
                fixture.WorkOrder).Value;
            CustomPcBuildOrderLineSnapshot motherboard = pickup.Line;
            CustomPcBuildOrderLineSnapshot processor = fixture.WorkOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long buildKitRevision = fixture.BuildKit.Revision;

            var valueEqualReceipt = new CustomPcBuildKitReceipt(
                pickup.OperationId,
                pickup.BuildOrder,
                pickup.Line,
                pickup.SourceContainerId,
                pickup.HandsContainerId,
                pickup.BuildKitContainerId,
                pickup.Stage,
                pickup.InventoryAppliedRevision);
            var wrongLineReceipt = CloneReceipt(
                pickup,
                CloneLine(
                    motherboard,
                    StableId<CustomPcBomLineIdScope>.Parse(
                        motherboard.LineId.Value + ".wrong"),
                    motherboard.ProductId,
                    motherboard.ItemId,
                    motherboard.ReservationId,
                    motherboard.ComponentKind));
            var wrongProductReceipt = CloneReceipt(
                pickup,
                CloneLine(
                    motherboard,
                    motherboard.LineId,
                    processor.ProductId,
                    motherboard.ItemId,
                    motherboard.ReservationId,
                    motherboard.ComponentKind));
            var wrongItemReceipt = CloneReceipt(
                pickup,
                CloneLine(
                    motherboard,
                    motherboard.LineId,
                    motherboard.ProductId,
                    processor.ItemId,
                    motherboard.ReservationId,
                    motherboard.ComponentKind));
            var wrongReservationReceipt = CloneReceipt(
                pickup,
                CloneLine(
                    motherboard,
                    motherboard.LineId,
                    motherboard.ProductId,
                    motherboard.ItemId,
                    processor.ReservationId,
                    motherboard.ComponentKind));
            var wrongKindReceipt = CloneReceipt(
                pickup,
                CloneLine(
                    motherboard,
                    motherboard.LineId,
                    motherboard.ProductId,
                    motherboard.ItemId,
                    motherboard.ReservationId,
                    PcComponentKind.Processor));
            var wrongWorkOrderReceipt = new CustomPcBuildKitReceipt(
                pickup.OperationId,
                foreign.WorkOrder,
                pickup.Line,
                pickup.SourceContainerId,
                pickup.HandsContainerId,
                pickup.BuildKitContainerId,
                pickup.Stage,
                pickup.InventoryAppliedRevision);
            var wrongStageReceipt = new CustomPcBuildKitReceipt(
                pickup.OperationId,
                pickup.BuildOrder,
                pickup.Line,
                pickup.SourceContainerId,
                pickup.HandsContainerId,
                pickup.BuildKitContainerId,
                CustomPcBuildKitStage.MotherboardStaged,
                pickup.InventoryAppliedRevision);

            CustomPcBuildKitReceipt[] forgeries =
            {
                valueEqualReceipt,
                wrongLineReceipt,
                wrongProductReceipt,
                wrongItemReceipt,
                wrongReservationReceipt,
                wrongKindReceipt,
                wrongWorkOrderReceipt,
                wrongStageReceipt
            };
            foreach (CustomPcBuildKitReceipt forgery in forgeries)
            {
                OperationResult<CustomPcBuildKitReceipt> result =
                    fixture.BuildKit.PlaceCanonicalMotherboard(forgery);
                Assert.That(result.Error,
                    Is.EqualTo(CustomPcWorkOrderFailures.BuildKitReceiptInvalid));
                Assert.That(fixture.Session.Inventory.Revision,
                    Is.EqualTo(inventoryRevision));
                Assert.That(fixture.BuildKit.Revision, Is.EqualTo(buildKitRevision));
                Assert.That(fixture.BuildKit.StagedComponentCount, Is.Zero);
            }

            Assert.That(fixture.Session.Inventory.TryGetSerializedItem(
                motherboard.ItemId,
                out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(fixture.Session.HandsContainerId));
            AssertInvariants(fixture);
            AssertInvariants(foreign);
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

        [Test]
        public void ProcessorBuildKitRequiresStagedMotherboardThenPreservesExactIdentity()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildOrderLineSnapshot motherboard = CanonicalMotherboard(workOrder);
            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            long initialInventoryRevision = session.Inventory.Revision;
            long initialBuildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitReceipt> blocked =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder);

            Assert.That(blocked.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(initialInventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(initialBuildKitRevision));
            Assert.That(session.TryGetProcessorItem(out InventoryItemRecord looseProcessor),
                Is.True);
            Assert.That(looseProcessor.Id, Is.EqualTo(processor.ItemId));
            Assert.That(looseProcessor.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));

            CustomPcBuildKitReceipt motherboardPickup =
                buildKit.PickupCanonicalMotherboard(
                    session.PrototypeCustomPcBuildKitOperationId,
                    workOrder).Value;
            CustomPcBuildKitReceipt motherboardPlacement =
                buildKit.PlaceCanonicalMotherboard(motherboardPickup).Value;
            Assert.That(motherboardPlacement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardStaged));
            Assert.That(motherboardPlacement.Line, Is.SameAs(motherboard));

            CustomPcBuildKitReceipt processorPickup =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder).Value;
            Assert.That(processorPickup.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorInHands));
            Assert.That(processorPickup.Line, Is.SameAs(processor));
            Assert.That(processorPickup.Line.LineId, Is.EqualTo(processor.LineId));
            Assert.That(processorPickup.Line.ProductId, Is.EqualTo(processor.ProductId));
            Assert.That(processorPickup.Line.ItemId, Is.EqualTo(processor.ItemId));
            Assert.That(processorPickup.Line.ReservationId,
                Is.EqualTo(processor.ReservationId));
            Assert.That(session.TryGetProcessorItem(out InventoryItemRecord heldProcessor),
                Is.True);
            Assert.That(heldProcessor.ContainerId, Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt processorPlacement =
                buildKit.PlaceCanonicalProcessor(processorPickup).Value;

            Assert.That(processorPlacement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorStaged));
            Assert.That(processorPlacement.Line, Is.SameAs(processor));
            Assert.That(session.TryGetProcessorItem(out InventoryItemRecord stagedProcessor),
                Is.True);
            Assert.That(stagedProcessor.Id, Is.EqualTo(processor.ItemId));
            Assert.That(stagedProcessor.ProductId, Is.EqualTo(processor.ProductId));
            Assert.That(stagedProcessor.ContainerId,
                Is.EqualTo(session.ProcessorBuildKitContainerId));
            Assert.That(session.TryGetMotherboardItem(
                out InventoryItemRecord stagedMotherboard), Is.True);
            Assert.That(stagedMotherboard.ContainerId,
                Is.EqualTo(session.CustomPcBuildKitContainerId));
            Assert.That(buildKit.ActiveKitCount, Is.EqualTo(2));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorPlacementRejectsStaleRevisionAndExactReplayIsNoMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildKitReceipt motherboardPickup =
                buildKit.PickupCanonicalMotherboard(
                    session.PrototypeCustomPcBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalMotherboard(motherboardPickup).IsSuccess,
                Is.True);
            CustomPcBuildKitReceipt processorPickup =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = buildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> stale =
                buildKit.PlaceCanonicalProcessor(
                    processorPickup,
                    buildKitRevision - 1,
                    inventoryRevision);

            Assert.That(stale.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.TryGetProcessorItem(out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt placed =
                buildKit.PlaceCanonicalProcessor(processorPickup).Value;
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = buildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> placementReplay =
                buildKit.PlaceCanonicalProcessor(
                    processorPickup,
                    buildKitRevision - 1,
                    inventoryRevision - 1);
            OperationResult<CustomPcBuildKitReceipt> pickupReplay =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder);

            Assert.That(placementReplay.IsSuccess, Is.True);
            Assert.That(placementReplay.Value, Is.SameAs(placed));
            Assert.That(pickupReplay.IsSuccess, Is.True);
            Assert.That(pickupReplay.Value, Is.SameAs(processorPickup));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MemoryModuleBuildKitRequiresMotherboardAndProcessorThenPreservesIdentity()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildOrderLineSnapshot memory = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.MemoryModule);
            long initialInventoryRevision = session.Inventory.Revision;
            long initialBuildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            MemorySlotState memorySlotState = session.AssemblyBuild.MemorySlotState;

            OperationResult<CustomPcBuildKitReceipt> beforeMotherboard =
                buildKit.PickupCanonicalMemoryModule(
                    session.PrototypeMemoryModuleBuildKitOperationId,
                    workOrder);

            Assert.That(beforeMotherboard.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(initialInventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(initialBuildKitRevision));

            CustomPcBuildKitReceipt motherboardPickup =
                buildKit.PickupCanonicalMotherboard(
                    session.PrototypeCustomPcBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalMotherboard(motherboardPickup).IsSuccess,
                Is.True);
            long afterMotherboardInventoryRevision = session.Inventory.Revision;
            long afterMotherboardBuildKitRevision = buildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> beforeProcessor =
                buildKit.PickupCanonicalMemoryModule(
                    session.PrototypeMemoryModuleBuildKitOperationId,
                    workOrder);

            Assert.That(beforeProcessor.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(afterMotherboardInventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(afterMotherboardBuildKitRevision));

            CustomPcBuildKitReceipt processorPickup =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalProcessor(processorPickup).IsSuccess,
                Is.True);

            CustomPcBuildKitReceipt memoryPickup =
                buildKit.PickupCanonicalMemoryModule(
                    session.PrototypeMemoryModuleBuildKitOperationId,
                    workOrder).Value;

            Assert.That(memoryPickup.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MemoryModuleInHands));
            Assert.That(memoryPickup.Line, Is.SameAs(memory));
            Assert.That(memoryPickup.Line.LineId, Is.EqualTo(memory.LineId));
            Assert.That(memoryPickup.Line.ProductId, Is.EqualTo(memory.ProductId));
            Assert.That(memoryPickup.Line.ItemId, Is.EqualTo(memory.ItemId));
            Assert.That(memoryPickup.Line.ReservationId, Is.EqualTo(memory.ReservationId));
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord heldMemory),
                Is.True);
            Assert.That(heldMemory.ContainerId, Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt memoryPlacement =
                buildKit.PlaceCanonicalMemoryModule(memoryPickup).Value;

            Assert.That(memoryPlacement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MemoryModuleStaged));
            Assert.That(memoryPlacement.Line, Is.SameAs(memory));
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord stagedMemory),
                Is.True);
            Assert.That(stagedMemory.Id, Is.EqualTo(memory.ItemId));
            Assert.That(stagedMemory.ProductId, Is.EqualTo(memory.ProductId));
            Assert.That(stagedMemory.ContainerId,
                Is.EqualTo(session.MemoryModuleBuildKitContainerId));
            Assert.That(buildKit.MemoryModuleBuildKitContainerId,
                Is.EqualTo(session.MemoryModuleBuildKitContainerId));
            Assert.That(buildKit.ActiveKitCount, Is.EqualTo(3));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.MemorySlotState, Is.EqualTo(memorySlotState));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MemoryModulePlacementRejectsStaleRevisionAndReplayIsNoMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            StageMotherboardAndProcessor(session, workOrder);
            CustomPcBuildKitReceipt pickup = buildKit.PickupCanonicalMemoryModule(
                session.PrototypeMemoryModuleBuildKitOperationId,
                workOrder).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitReceipt> stale =
                buildKit.PlaceCanonicalMemoryModule(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision);

            Assert.That(stale.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt placed =
                buildKit.PlaceCanonicalMemoryModule(pickup).Value;
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = buildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> placementReplay =
                buildKit.PlaceCanonicalMemoryModule(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision - 1);
            OperationResult<CustomPcBuildKitReceipt> pickupReplay =
                buildKit.PickupCanonicalMemoryModule(
                    session.PrototypeMemoryModuleBuildKitOperationId,
                    workOrder);

            Assert.That(placementReplay.IsSuccess, Is.True);
            Assert.That(placementReplay.Value, Is.SameAs(placed));
            Assert.That(pickupReplay.IsSuccess, Is.True);
            Assert.That(pickupReplay.Value, Is.SameAs(pickup));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SessionMemoryPickupUsesBuildKitCustodyAndGenericDropCannotBypassIt()
        {
            GarageStockFlowSession session = CreateIssuedSession(out CustomPcBuildOrderRecord workOrder);
            StageMotherboardAndProcessor(session, workOrder);
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult pickup = session.PickupLooseMemoryToHands();

            Assert.That(pickup.IsSuccess, Is.True);
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));
            Assert.That(session.DropHeldMemoryToWorld().IsFailure, Is.True);
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord stillHeld), Is.True);
            Assert.That(stillHeld.ContainerId, Is.EqualTo(session.HandsContainerId));

            OperationResult<CustomPcBuildKitReceipt> placed =
                session.PlaceHeldMemoryModuleInCustomPcBuildKit();

            Assert.That(placed.IsSuccess, Is.True);
            Assert.That(placed.Value.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MemoryModuleStaged));
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord staged), Is.True);
            Assert.That(staged.ContainerId,
                Is.EqualTo(session.MemoryModuleBuildKitContainerId));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StorageBuildKitRequiresFirstThreeComponentsThenPreservesIdentity()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildOrderLineSnapshot storage = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.StorageDevice);
            long initialInventoryRevision = session.Inventory.Revision;
            long initialBuildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            StorageSlotState storageSlotState = session.AssemblyBuild.StorageSlotState;

            OperationResult<CustomPcBuildKitReceipt> beforePrerequisites =
                buildKit.PickupCanonicalStorage(
                    session.PrototypeStorageBuildKitOperationId,
                    workOrder);

            Assert.That(beforePrerequisites.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(initialInventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(initialBuildKitRevision));

            StageMotherboardProcessorAndMemory(session, workOrder);

            CustomPcBuildKitReceipt storagePickup =
                buildKit.PickupCanonicalStorage(
                    session.PrototypeStorageBuildKitOperationId,
                    workOrder).Value;

            Assert.That(storagePickup.Stage,
                Is.EqualTo(CustomPcBuildKitStage.StorageInHands));
            Assert.That(storagePickup.Line, Is.SameAs(storage));
            Assert.That(storagePickup.Line.LineId, Is.EqualTo(storage.LineId));
            Assert.That(storagePickup.Line.ProductId, Is.EqualTo(storage.ProductId));
            Assert.That(storagePickup.Line.ItemId, Is.EqualTo(storage.ItemId));
            Assert.That(storagePickup.Line.ReservationId,
                Is.EqualTo(storage.ReservationId));
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord heldStorage),
                Is.True);
            Assert.That(heldStorage.ContainerId, Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt storagePlacement =
                buildKit.PlaceCanonicalStorage(storagePickup).Value;

            Assert.That(storagePlacement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.StorageStaged));
            Assert.That(storagePlacement.Line, Is.SameAs(storage));
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord stagedStorage),
                Is.True);
            Assert.That(stagedStorage.Id, Is.EqualTo(storage.ItemId));
            Assert.That(stagedStorage.ProductId, Is.EqualTo(storage.ProductId));
            Assert.That(stagedStorage.ContainerId,
                Is.EqualTo(session.StorageBuildKitContainerId));
            Assert.That(buildKit.StorageBuildKitContainerId,
                Is.EqualTo(session.StorageBuildKitContainerId));
            Assert.That(buildKit.ActiveKitCount, Is.EqualTo(4));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(storageSlotState));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StoragePlacementRejectsStaleRevisionAndReplayIsNoMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            StageMotherboardProcessorAndMemory(session, workOrder);
            CustomPcBuildKitReceipt pickup = buildKit.PickupCanonicalStorage(
                session.PrototypeStorageBuildKitOperationId,
                workOrder).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitReceipt> stale =
                buildKit.PlaceCanonicalStorage(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision);

            Assert.That(stale.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt placed =
                buildKit.PlaceCanonicalStorage(pickup).Value;
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = buildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> placementReplay =
                buildKit.PlaceCanonicalStorage(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision - 1);
            OperationResult<CustomPcBuildKitReceipt> pickupReplay =
                buildKit.PickupCanonicalStorage(
                    session.PrototypeStorageBuildKitOperationId,
                    workOrder);

            Assert.That(placementReplay.IsSuccess, Is.True);
            Assert.That(placementReplay.Value, Is.SameAs(placed));
            Assert.That(pickupReplay.IsSuccess, Is.True);
            Assert.That(pickupReplay.Value, Is.SameAs(pickup));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SessionStoragePickupUsesBuildKitCustodyAndGenericDropCannotBypassIt()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageMotherboardProcessorAndMemory(session, workOrder);
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult pickup = session.PickupLooseStorageToHands();

            Assert.That(pickup.IsSuccess, Is.True);
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));
            Assert.That(session.DropHeldStorageToWorld().IsFailure, Is.True);
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord stillHeld), Is.True);
            Assert.That(stillHeld.ContainerId, Is.EqualTo(session.HandsContainerId));

            OperationResult<CustomPcBuildKitReceipt> placed =
                session.PlaceHeldStorageInCustomPcBuildKit();

            Assert.That(placed.IsSuccess, Is.True);
            Assert.That(placed.Value.Stage,
                Is.EqualTo(CustomPcBuildKitStage.StorageStaged));
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord staged), Is.True);
            Assert.That(staged.ContainerId,
                Is.EqualTo(session.StorageBuildKitContainerId));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorCoolerBuildKitRequiresFirstFourComponentsAndPreservesExactIdentity()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildOrderLineSnapshot cooler = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.ProcessorCooler);
            long initialInventoryRevision = session.Inventory.Revision;
            long initialBuildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            ProcessorCoolerSlotState coolerSlotState =
                session.AssemblyBuild.ProcessorCoolerSlotState;
            ProcessorCoolerTimState coolerTimState =
                session.AssemblyBuild.ProcessorCoolerTimState;
            StableId<AssemblyOperationIdScope> seatedByOperationId =
                session.AssemblyBuild.ProcessorCoolerSeatedByOperationId;
            StableId<AssemblyOperationIdScope> retainedByOperationId =
                session.AssemblyBuild.ProcessorCoolerRetainedByOperationId;
            StableId<ItemInstanceIdScope> assemblyCoolerItemId =
                session.AssemblyBuild.ProcessorCoolerItemId;
            StableId<ProductDefinitionIdScope> assemblyCoolerProductId =
                session.AssemblyBuild.ProcessorCoolerProductId;

            AssertProcessorCoolerPrerequisiteFailure(
                session,
                workOrder,
                initialInventoryRevision,
                initialBuildKitRevision,
                assemblyRevision,
                assemblyReceiptCount);

            CustomPcBuildKitReceipt motherboardPickup =
                buildKit.PickupCanonicalMotherboard(
                    session.PrototypeCustomPcBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalMotherboard(motherboardPickup).IsSuccess,
                Is.True);
            AssertProcessorCoolerPrerequisiteFailure(
                session,
                workOrder,
                session.Inventory.Revision,
                buildKit.Revision,
                assemblyRevision,
                assemblyReceiptCount);

            CustomPcBuildKitReceipt processorPickup =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalProcessor(processorPickup).IsSuccess,
                Is.True);
            AssertProcessorCoolerPrerequisiteFailure(
                session,
                workOrder,
                session.Inventory.Revision,
                buildKit.Revision,
                assemblyRevision,
                assemblyReceiptCount);

            CustomPcBuildKitReceipt memoryPickup =
                buildKit.PickupCanonicalMemoryModule(
                    session.PrototypeMemoryModuleBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalMemoryModule(memoryPickup).IsSuccess,
                Is.True);
            AssertProcessorCoolerPrerequisiteFailure(
                session,
                workOrder,
                session.Inventory.Revision,
                buildKit.Revision,
                assemblyRevision,
                assemblyReceiptCount);

            CustomPcBuildKitReceipt storagePickup =
                buildKit.PickupCanonicalStorage(
                    session.PrototypeStorageBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalStorage(storagePickup).IsSuccess,
                Is.True);

            CustomPcBuildKitReceipt pickup =
                buildKit.PickupCanonicalProcessorCooler(
                    session.PrototypeProcessorCoolerBuildKitOperationId,
                    workOrder).Value;

            Assert.That(pickup.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorCoolerInHands));
            Assert.That(pickup.Line, Is.SameAs(cooler));
            Assert.That(pickup.Line.LineId, Is.EqualTo(cooler.LineId));
            Assert.That(pickup.Line.ProductId, Is.EqualTo(cooler.ProductId));
            Assert.That(pickup.Line.ItemId, Is.EqualTo(cooler.ItemId));
            Assert.That(pickup.Line.ReservationId, Is.EqualTo(cooler.ReservationId));
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord heldCooler), Is.True);
            Assert.That(heldCooler.ContainerId, Is.EqualTo(session.HandsContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                cooler.ReservationId,
                out InventoryReservation pickupReservation), Is.True);
            Assert.That(pickupReservation.ItemId, Is.EqualTo(cooler.ItemId));
            Assert.That(pickupReservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));

            CustomPcBuildKitReceipt placement =
                buildKit.PlaceCanonicalProcessorCooler(pickup).Value;

            Assert.That(placement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorCoolerStaged));
            Assert.That(placement.Line, Is.SameAs(cooler));
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord stagedCooler), Is.True);
            Assert.That(stagedCooler.Id, Is.EqualTo(cooler.ItemId));
            Assert.That(stagedCooler.ProductId, Is.EqualTo(cooler.ProductId));
            Assert.That(stagedCooler.ContainerId,
                Is.EqualTo(session.ProcessorCoolerBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                cooler.ReservationId,
                out InventoryReservation placementReservation), Is.True);
            Assert.That(placementReservation.ItemId, Is.EqualTo(cooler.ItemId));
            Assert.That(placementReservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));
            Assert.That(buildKit.ProcessorCoolerBuildKitContainerId,
                Is.EqualTo(session.ProcessorCoolerBuildKitContainerId));
            Assert.That(buildKit.ActiveKitCount, Is.EqualTo(5));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(coolerSlotState));
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(coolerTimState));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSeatedByOperationId,
                Is.EqualTo(seatedByOperationId));
            Assert.That(session.AssemblyBuild.ProcessorCoolerRetainedByOperationId,
                Is.EqualTo(retainedByOperationId));
            Assert.That(session.AssemblyBuild.ProcessorCoolerItemId,
                Is.EqualTo(assemblyCoolerItemId));
            Assert.That(session.AssemblyBuild.ProcessorCoolerProductId,
                Is.EqualTo(assemblyCoolerProductId));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorCoolerPlacementRejectsForgeryAndStaleRevisionThenReplaysWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            StageMotherboardProcessorMemoryAndStorage(session, workOrder);
            CustomPcBuildKitReceipt pickup =
                buildKit.PickupCanonicalProcessorCooler(
                    session.PrototypeProcessorCoolerBuildKitOperationId,
                    workOrder).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            ProcessorCoolerSlotState coolerSlotState =
                session.AssemblyBuild.ProcessorCoolerSlotState;
            ProcessorCoolerTimState coolerTimState =
                session.AssemblyBuild.ProcessorCoolerTimState;
            StableId<AssemblyOperationIdScope> seatedByOperationId =
                session.AssemblyBuild.ProcessorCoolerSeatedByOperationId;
            StableId<AssemblyOperationIdScope> retainedByOperationId =
                session.AssemblyBuild.ProcessorCoolerRetainedByOperationId;
            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            GarageStockFlowSession foreignSession = CreateIssuedSession(
                out CustomPcBuildOrderRecord foreignWorkOrder);
            StageMotherboardProcessorMemoryAndStorage(foreignSession, foreignWorkOrder);
            CustomPcBuildKitReceipt foreignPickup =
                foreignSession.CustomPcBuildKit.PickupCanonicalProcessorCooler(
                    foreignSession.PrototypeProcessorCoolerBuildKitOperationId,
                    foreignWorkOrder).Value;

            CustomPcBuildKitReceipt[] forgeries =
            {
                CloneReceipt(pickup, pickup.Line),
                CloneReceipt(
                    pickup,
                    CloneLine(
                        pickup.Line,
                        StableId<CustomPcBomLineIdScope>.Parse(
                            pickup.Line.LineId.Value + ".wrong"),
                        pickup.Line.ProductId,
                        pickup.Line.ItemId,
                        pickup.Line.ReservationId,
                        pickup.Line.ComponentKind)),
                CloneReceipt(
                    pickup,
                    CloneLine(
                        pickup.Line,
                        pickup.Line.LineId,
                        processor.ProductId,
                        pickup.Line.ItemId,
                        pickup.Line.ReservationId,
                        pickup.Line.ComponentKind)),
                CloneReceipt(
                    pickup,
                    CloneLine(
                        pickup.Line,
                        pickup.Line.LineId,
                        pickup.Line.ProductId,
                        processor.ItemId,
                        pickup.Line.ReservationId,
                        pickup.Line.ComponentKind)),
                CloneReceipt(
                    pickup,
                    CloneLine(
                        pickup.Line,
                        pickup.Line.LineId,
                        pickup.Line.ProductId,
                        pickup.Line.ItemId,
                        processor.ReservationId,
                        pickup.Line.ComponentKind)),
                CloneReceipt(
                    pickup,
                    CloneLine(
                        pickup.Line,
                        pickup.Line.LineId,
                        pickup.Line.ProductId,
                        pickup.Line.ItemId,
                        pickup.Line.ReservationId,
                        PcComponentKind.StorageDevice)),
                new CustomPcBuildKitReceipt(
                    pickup.OperationId,
                    pickup.BuildOrder,
                    pickup.Line,
                    pickup.SourceContainerId,
                    pickup.HandsContainerId,
                    pickup.BuildKitContainerId,
                    CustomPcBuildKitStage.ProcessorCoolerStaged,
                    pickup.InventoryAppliedRevision),
                new CustomPcBuildKitReceipt(
                    pickup.OperationId,
                    pickup.BuildOrder,
                    pickup.Line,
                    pickup.SourceContainerId,
                    pickup.HandsContainerId,
                    session.StorageBuildKitContainerId,
                    pickup.Stage,
                    pickup.InventoryAppliedRevision),
                foreignPickup
            };

            foreach (CustomPcBuildKitReceipt forgery in forgeries)
            {
                OperationResult<CustomPcBuildKitReceipt> result =
                    buildKit.PlaceCanonicalProcessorCooler(forgery);
                Assert.That(result.Error,
                    Is.EqualTo(CustomPcWorkOrderFailures.BuildKitReceiptInvalid));
                Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
                Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
                Assert.That(session.AssemblyBuild.ReceiptCount,
                    Is.EqualTo(assemblyReceiptCount));
                Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                    Is.EqualTo(coolerSlotState));
                Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                    Is.EqualTo(coolerTimState));
                Assert.That(session.AssemblyBuild.ProcessorCoolerSeatedByOperationId,
                    Is.EqualTo(seatedByOperationId));
                Assert.That(session.AssemblyBuild.ProcessorCoolerRetainedByOperationId,
                    Is.EqualTo(retainedByOperationId));
                Assert.That(session.TryGetProcessorCoolerItem(
                    out InventoryItemRecord forgedHeld), Is.True);
                Assert.That(forgedHeld.ContainerId,
                    Is.EqualTo(session.HandsContainerId));
            }

            OperationResult<CustomPcBuildKitReceipt> stale =
                buildKit.PlaceCanonicalProcessorCooler(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision);

            Assert.That(stale.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.TryGetProcessorCoolerItem(out InventoryItemRecord held),
                Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt placed =
                buildKit.PlaceCanonicalProcessorCooler(pickup).Value;
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = buildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> placementReplay =
                buildKit.PlaceCanonicalProcessorCooler(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision - 1);
            OperationResult<CustomPcBuildKitReceipt> pickupReplay =
                buildKit.PickupCanonicalProcessorCooler(
                    session.PrototypeProcessorCoolerBuildKitOperationId,
                    workOrder);
            OperationResult<CustomPcBuildKitReceipt> secondOperation =
                buildKit.PickupCanonicalProcessorCooler(
                    StableId<CustomPcBuildKitOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-operation.second-cooler"),
                    workOrder);

            Assert.That(placementReplay.IsSuccess, Is.True);
            Assert.That(placementReplay.Value, Is.SameAs(placed));
            Assert.That(pickupReplay.IsSuccess, Is.True);
            Assert.That(pickupReplay.Value, Is.SameAs(pickup));
            Assert.That(secondOperation.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitIdentityConflict));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(foreignSession.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SessionProcessorCoolerPickupUsesBuildKitCustodyAndGenericDropCannotBypassIt()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageMotherboardProcessorMemoryAndStorage(session, workOrder);
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            ProcessorCoolerSlotState coolerSlotState =
                session.AssemblyBuild.ProcessorCoolerSlotState;
            ProcessorCoolerTimState coolerTimState =
                session.AssemblyBuild.ProcessorCoolerTimState;
            System.Reflection.FieldInfo coolerSlotStateField =
                typeof(AssemblyBuildAuthority).GetField(
                    "_processorCoolerSlotState",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(coolerSlotStateField, Is.Not.Null);
            coolerSlotStateField.SetValue(
                session.AssemblyBuild,
                ProcessorCoolerSlotState.Unsupported);
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(coolerTimState),
                "Changing only the legacy assembly slot gate must not alter TIM state.");

            OperationResult pickup = session.PickupLooseProcessorCoolerToHands();
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(coolerTimState),
                "BuildKit pickup must not alter legacy assembly TIM state.");
            coolerSlotStateField.SetValue(session.AssemblyBuild, coolerSlotState);
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(coolerTimState),
                "Restoring the legacy slot gate must preserve TIM state.");

            Assert.That(pickup.IsSuccess, Is.True);
            Assert.That(session.TryGetProcessorCoolerItem(out InventoryItemRecord held),
                Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));
            Assert.That(session.DropHeldProcessorCoolerToWorld().IsFailure, Is.True);
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord stillHeld), Is.True);
            Assert.That(stillHeld.ContainerId, Is.EqualTo(session.HandsContainerId));

            OperationResult<CustomPcBuildKitReceipt> placed =
                session.PlaceHeldProcessorCoolerInCustomPcBuildKit();

            Assert.That(placed.IsSuccess, Is.True);
            Assert.That(placed.Value.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorCoolerStaged));
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord staged), Is.True);
            Assert.That(staged.ContainerId,
                Is.EqualTo(session.ProcessorCoolerBuildKitContainerId));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(coolerSlotState));
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(coolerTimState));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void GraphicsCardBuildKitRequiresFirstFiveComponentsAndPreservesAssemblyAndPcieState()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildOrderLineSnapshot graphicsCard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.GraphicsCard);
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            GraphicsCardSlotState graphicsCardSlotState =
                session.AssemblyBuild.GraphicsCardSlotState;
            StableId<AssemblyOperationIdScope> seatedByOperationId =
                session.AssemblyBuild.GraphicsCardSeatedByOperationId;
            StableId<AssemblyOperationIdScope> retainedByOperationId =
                session.AssemblyBuild.GraphicsCardRetainedByOperationId;
            StableId<ItemInstanceIdScope> assemblyGraphicsCardItemId =
                session.AssemblyBuild.GraphicsCardItemId;
            StableId<ProductDefinitionIdScope> assemblyGraphicsCardProductId =
                session.AssemblyBuild.GraphicsCardProductId;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            StableId<AssemblyOperationIdScope> pcieRoutedByOperationId =
                session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId;
            int pcieReceiptCount = session.AssemblyBuild.PcieGpuPowerCableReceiptCount;

            AssertGraphicsCardPrerequisiteFailure(session, workOrder);

            CustomPcBuildKitReceipt motherboardPickup =
                buildKit.PickupCanonicalMotherboard(
                    session.PrototypeCustomPcBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalMotherboard(motherboardPickup).IsSuccess,
                Is.True);
            AssertGraphicsCardPrerequisiteFailure(session, workOrder);

            CustomPcBuildKitReceipt processorPickup =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalProcessor(processorPickup).IsSuccess,
                Is.True);
            AssertGraphicsCardPrerequisiteFailure(session, workOrder);

            CustomPcBuildKitReceipt memoryPickup =
                buildKit.PickupCanonicalMemoryModule(
                    session.PrototypeMemoryModuleBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalMemoryModule(memoryPickup).IsSuccess,
                Is.True);
            AssertGraphicsCardPrerequisiteFailure(session, workOrder);

            CustomPcBuildKitReceipt storagePickup =
                buildKit.PickupCanonicalStorage(
                    session.PrototypeStorageBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalStorage(storagePickup).IsSuccess,
                Is.True);
            AssertGraphicsCardPrerequisiteFailure(session, workOrder);

            CustomPcBuildKitReceipt coolerPickup =
                buildKit.PickupCanonicalProcessorCooler(
                    session.PrototypeProcessorCoolerBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalProcessorCooler(coolerPickup).IsSuccess,
                Is.True);

            CustomPcBuildKitReceipt pickup =
                buildKit.PickupCanonicalGraphicsCard(
                    session.PrototypeGraphicsCardBuildKitOperationId,
                    workOrder).Value;

            Assert.That(pickup.Stage,
                Is.EqualTo(CustomPcBuildKitStage.GraphicsCardInHands));
            Assert.That(pickup.Line, Is.SameAs(graphicsCard));
            Assert.That(pickup.Line.LineId, Is.EqualTo(graphicsCard.LineId));
            Assert.That(pickup.Line.ProductId, Is.EqualTo(graphicsCard.ProductId));
            Assert.That(pickup.Line.ItemId, Is.EqualTo(graphicsCard.ItemId));
            Assert.That(pickup.Line.ReservationId,
                Is.EqualTo(graphicsCard.ReservationId));
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord heldGraphicsCard), Is.True);
            Assert.That(heldGraphicsCard.ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                graphicsCard.ReservationId,
                out InventoryReservation pickupReservation), Is.True);
            Assert.That(pickupReservation.ItemId, Is.EqualTo(graphicsCard.ItemId));
            Assert.That(pickupReservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));

            CustomPcBuildKitReceipt placement =
                buildKit.PlaceCanonicalGraphicsCard(pickup).Value;

            Assert.That(placement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.GraphicsCardStaged));
            Assert.That(placement.Line, Is.SameAs(graphicsCard));
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord stagedGraphicsCard), Is.True);
            Assert.That(stagedGraphicsCard.Id, Is.EqualTo(graphicsCard.ItemId));
            Assert.That(stagedGraphicsCard.ProductId,
                Is.EqualTo(graphicsCard.ProductId));
            Assert.That(stagedGraphicsCard.ContainerId,
                Is.EqualTo(session.GraphicsCardBuildKitContainerId));
            Assert.That(buildKit.GraphicsCardBuildKitContainerId,
                Is.EqualTo(session.GraphicsCardBuildKitContainerId));
            Assert.That(buildKit.ActiveKitCount, Is.EqualTo(6));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(graphicsCardSlotState));
            Assert.That(session.AssemblyBuild.GraphicsCardSeatedByOperationId,
                Is.EqualTo(seatedByOperationId));
            Assert.That(session.AssemblyBuild.GraphicsCardRetainedByOperationId,
                Is.EqualTo(retainedByOperationId));
            Assert.That(session.AssemblyBuild.GraphicsCardItemId,
                Is.EqualTo(assemblyGraphicsCardItemId));
            Assert.That(session.AssemblyBuild.GraphicsCardProductId,
                Is.EqualTo(assemblyGraphicsCardProductId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(pcieState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId,
                Is.EqualTo(pcieRoutedByOperationId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(pcieReceiptCount));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void GraphicsCardPlacementRejectsForgeryAndStaleRevisionThenReplaysWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstFiveBuildKitComponents(session, workOrder);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildKitReceipt pickup =
                buildKit.PickupCanonicalGraphicsCard(
                    session.PrototypeGraphicsCardBuildKitOperationId,
                    workOrder).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            int pcieReceiptCount = session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            GarageStockFlowSession foreignSession = CreateIssuedSession(
                out CustomPcBuildOrderRecord foreignWorkOrder);
            StageFirstFiveBuildKitComponents(foreignSession, foreignWorkOrder);
            CustomPcBuildKitReceipt foreignPickup =
                foreignSession.CustomPcBuildKit.PickupCanonicalGraphicsCard(
                    foreignSession.PrototypeGraphicsCardBuildKitOperationId,
                    foreignWorkOrder).Value;

            CustomPcBuildKitReceipt[] forgeries =
            {
                CloneReceipt(pickup, pickup.Line),
                new CustomPcBuildKitReceipt(
                    pickup.OperationId,
                    pickup.BuildOrder,
                    pickup.Line,
                    pickup.SourceContainerId,
                    pickup.HandsContainerId,
                    pickup.BuildKitContainerId,
                    CustomPcBuildKitStage.GraphicsCardStaged,
                    pickup.InventoryAppliedRevision),
                new CustomPcBuildKitReceipt(
                    pickup.OperationId,
                    pickup.BuildOrder,
                    pickup.Line,
                    pickup.SourceContainerId,
                    pickup.HandsContainerId,
                    session.ProcessorCoolerBuildKitContainerId,
                    pickup.Stage,
                    pickup.InventoryAppliedRevision),
                foreignPickup
            };

            foreach (CustomPcBuildKitReceipt forgery in forgeries)
            {
                OperationResult<CustomPcBuildKitReceipt> result =
                    buildKit.PlaceCanonicalGraphicsCard(forgery);
                Assert.That(result.Error,
                    Is.EqualTo(CustomPcWorkOrderFailures.BuildKitReceiptInvalid));
                Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
                Assert.That(session.AssemblyBuild.Revision,
                    Is.EqualTo(assemblyRevision));
                Assert.That(session.AssemblyBuild.ReceiptCount,
                    Is.EqualTo(assemblyReceiptCount));
                Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                    Is.EqualTo(pcieState));
                Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                    Is.EqualTo(pcieReceiptCount));
                Assert.That(session.TryGetGraphicsCardAssemblyItem(
                    out InventoryItemRecord stillHeld), Is.True);
                Assert.That(stillHeld.ContainerId,
                    Is.EqualTo(session.HandsContainerId));
            }

            OperationResult<CustomPcBuildKitReceipt> stale =
                buildKit.PlaceCanonicalGraphicsCard(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision);
            Assert.That(stale.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));

            CustomPcBuildKitReceipt placed =
                buildKit.PlaceCanonicalGraphicsCard(pickup).Value;
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = buildKit.Revision;

            OperationResult<CustomPcBuildKitReceipt> placementReplay =
                buildKit.PlaceCanonicalGraphicsCard(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision - 1);
            OperationResult<CustomPcBuildKitReceipt> pickupReplay =
                buildKit.PickupCanonicalGraphicsCard(
                    session.PrototypeGraphicsCardBuildKitOperationId,
                    workOrder);
            OperationResult<CustomPcBuildKitReceipt> secondOperation =
                buildKit.PickupCanonicalGraphicsCard(
                    StableId<CustomPcBuildKitOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-operation.second-graphics-card"),
                    workOrder);

            Assert.That(placementReplay.IsSuccess, Is.True);
            Assert.That(placementReplay.Value, Is.SameAs(placed));
            Assert.That(pickupReplay.IsSuccess, Is.True);
            Assert.That(pickupReplay.Value, Is.SameAs(pickup));
            Assert.That(secondOperation.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitIdentityConflict));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(pcieState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(pcieReceiptCount));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(foreignSession.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SessionGraphicsCardPickupUsesBuildKitCustodyAndGenericDropCannotBypassIt()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstFiveBuildKitComponents(session, workOrder);
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            GraphicsCardSlotState slotState =
                session.AssemblyBuild.GraphicsCardSlotState;
            System.Reflection.FieldInfo slotStateField =
                typeof(AssemblyBuildAuthority).GetField(
                    "_graphicsCardSlotState",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(slotStateField, Is.Not.Null);
            slotStateField.SetValue(
                session.AssemblyBuild,
                GraphicsCardSlotState.Unsupported);

            OperationResult pickup = session.PickupLooseGraphicsCardToHands();

            slotStateField.SetValue(session.AssemblyBuild, slotState);
            Assert.That(pickup.IsSuccess, Is.True);
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));
            Assert.That(session.DropHeldGraphicsCardToWorld().IsFailure, Is.True);
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord stillHeld), Is.True);
            Assert.That(stillHeld.ContainerId, Is.EqualTo(session.HandsContainerId));

            OperationResult<CustomPcBuildKitReceipt> placed =
                session.PlaceHeldGraphicsCardInCustomPcBuildKit();

            Assert.That(placed.IsSuccess, Is.True);
            Assert.That(placed.Value.Stage,
                Is.EqualTo(CustomPcBuildKitStage.GraphicsCardStaged));
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord staged), Is.True);
            Assert.That(staged.ContainerId,
                Is.EqualTo(session.GraphicsCardBuildKitContainerId));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(slotState));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void GraphicsCardSextupleContainerClaimIsAllOrNone()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out _);
            StableId<ContainerIdScope>[] containerIds = Enumerable.Range(1, 6)
                .Select(index => StableId<ContainerIdScope>.Parse(
                    $"inventory.container.graphics-card-build-kit-claim-{index}"))
                .ToArray();
            foreach (StableId<ContainerIdScope> containerId in containerIds)
            {
                RegisterBuildKitContainer(session, containerId);
            }
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAuthority> collision =
                CustomPcBuildKitAuthority.Create(
                    session.CustomPcWorkOrders,
                    session.WorldFloorContainerId,
                    session.HandsContainerId,
                    containerIds[0],
                    containerIds[1],
                    containerIds[2],
                    containerIds[3],
                    containerIds[4],
                    containerIds[4]);

            Assert.That(collision.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitContainerInvalid));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));

            OperationResult<CustomPcBuildKitAuthority> valid =
                CustomPcBuildKitAuthority.Create(
                    session.CustomPcWorkOrders,
                    session.WorldFloorContainerId,
                    session.HandsContainerId,
                    containerIds[0],
                    containerIds[1],
                    containerIds[2],
                    containerIds[3],
                    containerIds[4],
                    containerIds[5]);

            Assert.That(valid.IsSuccess, Is.True,
                "A rejected sextuple topology must not partially claim containers.");
            Assert.That(valid.Value.GraphicsCardBuildKitContainerId,
                Is.EqualTo(containerIds[5]));
            Assert.That(valid.Value.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static Fixture CreateFixture()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
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
                workOrder,
                buildKit.Value,
                buildKitContainerId);
        }

        private static GarageStockFlowSession CreateIssuedSession(
            out CustomPcBuildOrderRecord workOrder)
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
            workOrder = issued.Value.BuildOrder;
            return session;
        }

        private static void StageMotherboardAndProcessor(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildKitReceipt motherboardPickup =
                buildKit.PickupCanonicalMotherboard(
                    session.PrototypeCustomPcBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalMotherboard(motherboardPickup).IsSuccess,
                Is.True);
            CustomPcBuildKitReceipt processorPickup =
                buildKit.PickupCanonicalProcessor(
                    session.PrototypeProcessorBuildKitOperationId,
                    workOrder).Value;
            Assert.That(buildKit.PlaceCanonicalProcessor(processorPickup).IsSuccess,
                Is.True);
        }

        private static void StageMotherboardProcessorAndMemory(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            StageMotherboardAndProcessor(session, workOrder);
            CustomPcBuildKitReceipt memoryPickup =
                session.CustomPcBuildKit.PickupCanonicalMemoryModule(
                    session.PrototypeMemoryModuleBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalMemoryModule(memoryPickup).IsSuccess, Is.True);
        }

        private static void StageMotherboardProcessorMemoryAndStorage(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            StageMotherboardProcessorAndMemory(session, workOrder);
            CustomPcBuildKitReceipt storagePickup =
                session.CustomPcBuildKit.PickupCanonicalStorage(
                    session.PrototypeStorageBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalStorage(storagePickup).IsSuccess, Is.True);
        }

        private static void StageFirstFiveBuildKitComponents(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            StageMotherboardProcessorMemoryAndStorage(session, workOrder);
            CustomPcBuildKitReceipt coolerPickup =
                session.CustomPcBuildKit.PickupCanonicalProcessorCooler(
                    session.PrototypeProcessorCoolerBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalProcessorCooler(coolerPickup).IsSuccess, Is.True);
        }

        private static void AssertGraphicsCardPrerequisiteFailure(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            int pcieReceiptCount = session.AssemblyBuild.PcieGpuPowerCableReceiptCount;

            OperationResult<CustomPcBuildKitReceipt> result =
                session.CustomPcBuildKit.PickupCanonicalGraphicsCard(
                    session.PrototypeGraphicsCardBuildKitOperationId,
                    workOrder);

            Assert.That(result.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(pcieState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(pcieReceiptCount));
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord untouched), Is.True);
            Assert.That(untouched.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));
        }

        private static void RegisterBuildKitContainer(
            GarageStockFlowSession session,
            StableId<ContainerIdScope> containerId)
        {
            Assert.That(session.Inventory.RegisterContainer(
                InventoryContainerDefinition.Create(
                    containerId,
                    InventoryContainerKind.BuildKit,
                    1).Value).IsSuccess, Is.True);
        }

        private static void AssertProcessorCoolerPrerequisiteFailure(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            long expectedInventoryRevision,
            long expectedBuildKitRevision,
            long expectedAssemblyRevision,
            int expectedAssemblyReceiptCount)
        {
            OperationResult<CustomPcBuildKitReceipt> result =
                session.CustomPcBuildKit.PickupCanonicalProcessorCooler(
                    session.PrototypeProcessorCoolerBuildKitOperationId,
                    workOrder);

            Assert.That(result.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(expectedInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(expectedBuildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(expectedAssemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(expectedAssemblyReceiptCount));
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord untouched), Is.True);
            Assert.That(untouched.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));
        }

        private static CustomPcBuildOrderLineSnapshot CanonicalMotherboard(
            CustomPcBuildOrderRecord workOrder)
        {
            return workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
        }

        private static CustomPcBuildKitReceipt CloneReceipt(
            CustomPcBuildKitReceipt source,
            CustomPcBuildOrderLineSnapshot line)
        {
            return new CustomPcBuildKitReceipt(
                source.OperationId,
                source.BuildOrder,
                line,
                source.SourceContainerId,
                source.HandsContainerId,
                source.BuildKitContainerId,
                source.Stage,
                source.InventoryAppliedRevision);
        }

        private static CustomPcBuildOrderLineSnapshot CloneLine(
            CustomPcBuildOrderLineSnapshot source,
            StableId<CustomPcBomLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind)
        {
            return new CustomPcBuildOrderLineSnapshot(
                new CustomPcQuoteLineSnapshot(
                    lineId,
                    productId,
                    itemId,
                    reservationId,
                    componentKind,
                    source.PowerCableType,
                    source.UnitCost,
                    source.UnitPrice));
        }

        private static void AssertInvariants(Fixture fixture)
        {
            Assert.That(fixture.Session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Session.CustomPcWorkOrders.ValidateInvariants().IsSuccess,
                Is.True);
            Assert.That(fixture.BuildKit.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertMotherboardStillInHands(
            Fixture fixture,
            CustomPcBuildKitReceipt pickup)
        {
            Assert.That(fixture.Session.Inventory.TryGetSerializedItem(
                pickup.Line.ItemId,
                out InventoryItemRecord item), Is.True);
            Assert.That(item.ContainerId, Is.EqualTo(fixture.Session.HandsContainerId));
            Assert.That(fixture.BuildKit.StagedComponentCount, Is.Zero);
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
