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
        public void SecuredMotherboardAllowsCanonicalProcessorSocketRetentionRoundTrip()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareSecuredMotherboardForProcessorAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId);

            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            CustomPcBuildOrderLineSnapshot motherboard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetItem(session, line.ItemId).ContainerId);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorBuildKitOperationId,
                out CustomPcBuildKitReceipt originalProcessorStaging), Is.True);
            Assert.That(originalProcessorStaging.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorStaged));

            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedProcessorForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    assemblyRevision);

            Assert.That(handoff.IsSuccess, Is.True);
            Assert.That(handoff.Value.ComponentKind, Is.EqualTo(PcComponentKind.Processor));
            Assert.That(handoff.Value.Line, Is.SameAs(processor));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(originalProcessorStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(2));
            Assert.That(GetItem(session, processor.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            AssertReservationStillLive(session, processor);
            AssertUntouchedContainers(session, untouchedContainers);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedProcessorForAssembly();
            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));

            OperationResult blockedDrop = session.DropHeldProcessorToWorld();
            Assert.That(blockedDrop.IsFailure, Is.True);
            Assert.That(blockedDrop.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> seatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue91.seat-processor");
            Assert.That(session.SeatProcessor(
                seatId,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            Assert.That(GetItem(session, processor.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            AssertReservationStillLive(session, processor);

            StableId<AssemblyOperationIdScope> retainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue91.retain-processor");
            Assert.That(session.CloseProcessorRetention(
                retainId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));

            StableId<AssemblyOperationIdScope> openId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue91.open-processor-retention");
            Assert.That(session.OpenProcessorRetention(
                openId,
                seatId,
                retainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> removeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue91.remove-processor");
            Assert.That(session.RemoveProcessor(
                removeId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(GetItem(session, processor.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, processor);

            StableId<AssemblyOperationIdScope> reseatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue91.reseat-processor");
            Assert.That(session.SeatProcessor(
                reseatId,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(GetItem(session, processor.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorBuildKitOperationId,
                out CustomPcBuildKitReceipt preservedProcessorStaging), Is.True);
            Assert.That(preservedProcessorStaging, Is.SameAs(originalProcessorStaging));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(2));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            AssertUntouchedContainers(session, untouchedContainers);

            long delayedReplayBuildKitRevision =
                session.CustomPcBuildKit.Revision;
            long delayedReplayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
                delayedReplay = session.CustomPcBuildKit
                    .ReleaseCanonicalProcessorForAssembly(
                        session.PrototypeProcessorAssemblyHandoffOperationId,
                        workOrder,
                        session.ProcessorSocketContainerId,
                        expectedBuildKitRevision: -1L,
                        expectedInventoryRevision: -1L);
            Assert.That(delayedReplay.IsSuccess, Is.True);
            Assert.That(delayedReplay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(delayedReplayBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(delayedReplayInventoryRevision));
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorAssemblyHandoffFailsClosedUntilExactMotherboardIsSecured()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            long stagedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long stagedInventoryRevision = session.Inventory.Revision;
            long stagedAssemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> motherboardMissing =
                session.PickupStagedProcessorForAssembly();
            Assert.That(motherboardMissing.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(stagedBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(stagedInventoryRevision));

            Assert.That(session.PickupStagedMotherboardForAssembly().IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> attachId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue91.precondition-attach");
            Assert.That(session.AttachMotherboard(attachId).IsSuccess, Is.True);
            long unsecuredBuildKitRevision = session.CustomPcBuildKit.Revision;
            long unsecuredInventoryRevision = session.Inventory.Revision;
            long unsecuredAssemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> unsecured =
                session.PickupStagedProcessorForAssembly();
            Assert.That(unsecured.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(unsecuredBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(unsecuredInventoryRevision));

            StableId<AssemblyOperationIdScope> secureId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue91.precondition-secure");
            Assert.That(session.SecureMotherboardFastener(
                secureId,
                attachId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> staleAssembly =
                session.PickupStagedProcessorForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    unsecuredAssemblyRevision);
            Assert.That(staleAssembly.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignTarget =
                session.CustomPcBuildKit.ReleaseCanonicalProcessorForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue91.foreign-target"),
                    workOrder,
                    session.WorkbenchContainerId,
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision);
            Assert.That(foreignTarget.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedProcessorForAssembly();
            Assert.That(accepted.IsSuccess, Is.True);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalProcessorForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue91.second"),
                    workOrder,
                    session.ProcessorSocketContainerId,
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
        public void ProcessorAssemblyHandoffRetryPublishesAfterInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareSecuredMotherboardForProcessorAssembly(session, out _, out _);
            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryProcessorAssemblyHandoffForRecovery(
                        session.PrototypeProcessorAssemblyHandoffOperationId,
                        workOrder,
                        session.ProcessorSocketContainerId,
                        buildKitRevision,
                        inventoryRevision);

            Assert.That(inventoryCommit.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(1));
            Assert.That(GetItem(session, processor.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, processor);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedProcessorForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision);

            Assert.That(retry.IsSuccess, Is.True);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(2));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(2));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareSecuredMotherboardForProcessorAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> attachId,
            out StableId<AssemblyOperationIdScope> secureId)
        {
            Assert.That(session.PickupStagedMotherboardForAssembly().IsSuccess, Is.True);
            attachId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue91.attach-motherboard");
            Assert.That(session.AttachMotherboard(attachId).IsSuccess, Is.True);
            secureId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue91.secure-motherboard");
            Assert.That(session.SecureMotherboardFastener(
                secureId,
                attachId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
        }
    }
}
