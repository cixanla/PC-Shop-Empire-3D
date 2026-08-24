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
