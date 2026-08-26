using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void CompleteKitMotherboardFlowsThroughExistingAssemblyWithoutLosingJobCustody()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot motherboard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetItem(session, line.ItemId).ContainerId);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeCustomPcBuildKitOperationId,
                out CustomPcBuildKitReceipt originalStagingReceipt), Is.True);
            Assert.That(originalStagingReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardStaged));

            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedMotherboardForAssembly(
                    buildKitRevision,
                    inventoryRevision);

            Assert.That(handoff.IsSuccess, Is.True);
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(originalStagingReceipt));
            Assert.That(handoff.Value.Line, Is.SameAs(motherboard));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(1));
            Assert.That(session.Inventory.SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(1));
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, motherboard);
            AssertUntouchedContainers(session, untouchedContainers);
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedMotherboardForAssembly();
            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));

            OperationResult blockedDrop = session.DropHeldMotherboardToWorld();
            Assert.That(blockedDrop.IsFailure, Is.True);
            Assert.That(blockedDrop.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));

            StableId<AssemblyOperationIdScope> attachId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue89.attach-motherboard");
            OperationResult<AssemblyOperationReceipt> attach =
                session.AttachMotherboard(attachId);
            Assert.That(attach.IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            AssertReservationStillLive(session, motherboard);

            StableId<AssemblyOperationIdScope> secureId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue89.secure-motherboard");
            Assert.That(session.SecureMotherboardFastener(
                secureId,
                attachId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));

            StableId<AssemblyOperationIdScope> unsecureId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue89.unsecure-motherboard");
            Assert.That(session.UnsecureMotherboardFastener(
                unsecureId,
                attachId,
                secureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> detachId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue89.detach-motherboard");
            Assert.That(session.DetachMotherboard(detachId).IsSuccess, Is.True);
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, motherboard);
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> reattachId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue89.reattach-motherboard");
            Assert.That(session.AttachMotherboard(reattachId).IsSuccess, Is.True);
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeCustomPcBuildKitOperationId,
                out CustomPcBuildKitReceipt preservedStagingReceipt), Is.True);
            Assert.That(preservedStagingReceipt, Is.SameAs(originalStagingReceipt));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            AssertUntouchedContainers(session, untouchedContainers);
            Assert.That(session.CustomPcBuildKit.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MotherboardAssemblyHandoffFailsClosedForIncompleteStaleAndSecondIdentity()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstNineBuildKitComponents(session, workOrder);
            long incompleteBuildKitRevision = session.CustomPcBuildKit.Revision;
            long incompleteInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> incomplete =
                session.PickupStagedMotherboardForAssembly();

            Assert.That(incomplete.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(incompleteBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(incompleteInventoryRevision));

            CustomPcBuildKitReceipt pciePickup = session.CustomPcBuildKit
                .PickupCanonicalPcieGpuPowerCable(
                    session.PrototypePcieGpuPowerCableBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalPcieGpuPowerCable(pciePickup).IsSuccess, Is.True);
            long completeBuildKitRevision = session.CustomPcBuildKit.Revision;
            long completeInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> stale =
                session.PickupStagedMotherboardForAssembly(
                    completeBuildKitRevision - 1,
                    completeInventoryRevision);
            Assert.That(stale.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(completeBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(completeInventoryRevision));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> invalidWorkbench =
                session.CustomPcBuildKit.ReleaseCanonicalMotherboardForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue89.invalid-workbench"),
                    workOrder,
                    session.WorldFloorContainerId,
                    completeBuildKitRevision,
                    completeInventoryRevision);
            Assert.That(invalidWorkbench.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));

            var valueEqualForeignWorkOrder = new CustomPcBuildOrderRecord(
                workOrder.Id,
                workOrder.WorkTicketId,
                workOrder.OperationId,
                workOrder.SourceQuote,
                workOrder.WorkbenchContainerId,
                workOrder.IssuedAt,
                workOrder.Lines,
                workOrder.InventoryAllocationRevision);
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignWorkOrder =
                session.CustomPcBuildKit.ReleaseCanonicalMotherboardForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue89.foreign-order"),
                    valueEqualForeignWorkOrder,
                    session.WorkbenchContainerId,
                    completeBuildKitRevision,
                    completeInventoryRevision);
            Assert.That(foreignWorkOrder.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitWorkOrderInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(completeBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(completeInventoryRevision));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedMotherboardForAssembly(
                    completeBuildKitRevision,
                    completeInventoryRevision);
            Assert.That(accepted.IsSuccess, Is.True);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalMotherboardForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue89.second"),
                    workOrder,
                    session.WorkbenchContainerId,
                    acceptedBuildKitRevision,
                    acceptedInventoryRevision);
            Assert.That(secondIdentity.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyIdentityConflict));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(acceptedBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(acceptedInventoryRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MotherboardAssemblyHandoffRetryRecoversAfterInventoryCommitWithoutSecondMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot motherboard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryMotherboardAssemblyHandoffForRecovery(
                        session.PrototypeMotherboardAssemblyHandoffOperationId,
                        workOrder,
                        session.WorkbenchContainerId,
                        buildKitRevision,
                        inventoryRevision);

            Assert.That(inventoryCommit.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.Zero);
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, motherboard);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedMotherboardForAssembly(
                    buildKitRevision,
                    inventoryRevision);

            Assert.That(retry.IsSuccess, Is.True);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(1));
            Assert.That(session.Inventory.SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageAllTenBuildKitComponents(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            StageFirstNineBuildKitComponents(session, workOrder);
            CustomPcBuildKitReceipt pciePickup = session.CustomPcBuildKit
                .PickupCanonicalPcieGpuPowerCable(
                    session.PrototypePcieGpuPowerCableBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalPcieGpuPowerCable(pciePickup).IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
        }

        private static InventoryItemRecord GetItem(
            GarageStockFlowSession session,
            StableId<ItemInstanceIdScope> itemId)
        {
            Assert.That(session.Inventory.TryGetSerializedItem(
                itemId,
                out InventoryItemRecord item), Is.True);
            return item;
        }

        private static void AssertReservationStillLive(
            GarageStockFlowSession session,
            CustomPcBuildOrderLineSnapshot line)
        {
            Assert.That(session.Inventory.TryGetReservation(
                line.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(line.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(session.PrototypeCustomPcClaimId));
        }

        private static void AssertUntouchedContainers(
            GarageStockFlowSession session,
            IReadOnlyDictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                expectedContainers)
        {
            foreach (KeyValuePair<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                     entry in expectedContainers)
            {
                Assert.That(GetItem(session, entry.Key).ContainerId, Is.EqualTo(entry.Value));
            }
        }
    }
}
