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
        public void RetainedProcessorAllowsCanonicalMemoryDualLatchRoundTrip()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedProcessorForMemoryAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out _,
                out _);

            CustomPcBuildOrderLineSnapshot memory = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.MemoryModule);
            CustomPcBuildOrderLineSnapshot motherboard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetItem(session, line.ItemId).ContainerId);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeMemoryModuleBuildKitOperationId,
                out CustomPcBuildKitReceipt originalMemoryStaging), Is.True);
            Assert.That(originalMemoryStaging.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MemoryModuleStaged));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedMemoryModuleForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    session.AssemblyBuild.Revision);

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind,
                Is.EqualTo(PcComponentKind.MemoryModule));
            Assert.That(handoff.Value.Line, Is.SameAs(memory));
            Assert.That(handoff.Value.StagingReceipt,
                Is.SameAs(originalMemoryStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(3));
            Assert.That(GetItem(session, memory.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(GetItem(session, processor.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            AssertReservationStillLive(session, memory);
            AssertReservationStillLive(session, processor);
            AssertUntouchedContainers(session, untouchedContainers);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedMemoryModuleForAssembly();
            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(replayInventoryRevision));

            OperationResult blockedDrop = session.DropHeldMemoryToWorld();
            Assert.That(blockedDrop.IsFailure, Is.True);
            Assert.That(blockedDrop.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> seatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.seat-memory");
            Assert.That(session.SeatMemoryModule(
                seatId,
                DimmKeyOrientation.NotchAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(GetItem(session, memory.ItemId).ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));

            StableId<AssemblyOperationIdScope> retainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.close-dual-latch");
            Assert.That(session.CloseMemoryRetention(
                retainId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));

            StableId<AssemblyOperationIdScope> openId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.open-dual-latch");
            Assert.That(session.OpenMemoryRetention(
                openId,
                seatId,
                retainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> removeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.remove-memory");
            Assert.That(session.RemoveMemoryModule(
                removeId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(GetItem(session, memory.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, memory);

            StableId<AssemblyOperationIdScope> reseatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.reseat-memory");
            Assert.That(session.SeatMemoryModule(
                reseatId,
                DimmKeyOrientation.NotchAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> recloseId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.reclose-dual-latch");
            Assert.That(session.CloseMemoryRetention(
                recloseId,
                reseatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeMemoryModuleBuildKitOperationId,
                out CustomPcBuildKitReceipt preservedMemoryStaging), Is.True);
            Assert.That(preservedMemoryStaging, Is.SameAs(originalMemoryStaging));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(3));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(GetItem(session, memory.ItemId).ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            AssertUntouchedContainers(session, untouchedContainers);

            long delayedReplayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long delayedReplayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> delayedReplay =
                session.CustomPcBuildKit.ReleaseCanonicalMemoryModuleForAssembly(
                    session.PrototypeMemoryModuleAssemblyHandoffOperationId,
                    workOrder,
                    session.MemorySlotContainerId,
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
        public void MemoryAssemblyHandoffFailsClosedUntilExactProcessorIsRetained()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);

            long originalBuildKitRevision = session.CustomPcBuildKit.Revision;
            long originalInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> motherboardMissing =
                session.PickupStagedMemoryModuleForAssembly();
            Assert.That(motherboardMissing.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(originalBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(originalInventoryRevision));

            PrepareSecuredMotherboardForProcessorAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId);
            long processorMissingBuildKitRevision = session.CustomPcBuildKit.Revision;
            long processorMissingInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> processorMissing =
                session.PickupStagedMemoryModuleForAssembly();
            Assert.That(processorMissing.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(processorMissingBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(processorMissingInventoryRevision));

            Assert.That(session.PickupStagedProcessorForAssembly().IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> processorSeatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.precondition-seat-processor");
            Assert.That(session.SeatProcessor(
                processorSeatId,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            long processorOpenBuildKitRevision = session.CustomPcBuildKit.Revision;
            long processorOpenInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> processorOpen =
                session.PickupStagedMemoryModuleForAssembly();
            Assert.That(processorOpen.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(processorOpenBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(processorOpenInventoryRevision));

            long staleAssemblyRevision = session.AssemblyBuild.Revision;
            StableId<AssemblyOperationIdScope> processorRetainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue93.precondition-retain-processor");
            Assert.That(session.CloseProcessorRetention(
                processorRetainId,
                processorSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> staleAssembly =
                session.PickupStagedMemoryModuleForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    staleAssemblyRevision);
            Assert.That(staleAssembly.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignTarget =
                session.CustomPcBuildKit.ReleaseCanonicalMemoryModuleForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue93.foreign-target"),
                    workOrder,
                    session.WorkbenchContainerId,
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision);
            Assert.That(foreignTarget.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedMemoryModuleForAssembly();
            Assert.That(accepted.IsSuccess, Is.True, accepted.Error.Code);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalMemoryModuleForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue93.second"),
                    workOrder,
                    session.MemorySlotContainerId,
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
        public void MemoryAssemblyHandoffRetryPublishesAfterInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedProcessorForMemoryAssembly(
                session,
                out _,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot memory = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.MemoryModule);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryMemoryModuleAssemblyHandoffForRecovery(
                        session.PrototypeMemoryModuleAssemblyHandoffOperationId,
                        workOrder,
                        session.MemorySlotContainerId,
                        buildKitRevision,
                        inventoryRevision);

            Assert.That(inventoryCommit.IsSuccess, Is.True, inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(2));
            Assert.That(GetItem(session, memory.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, memory);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedMemoryModuleForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision);

            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(3));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(3));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareRetainedProcessorForMemoryAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> motherboardAttachId,
            out StableId<AssemblyOperationIdScope> motherboardSecureId,
            out StableId<AssemblyOperationIdScope> processorSeatId,
            out StableId<AssemblyOperationIdScope> processorRetainId)
        {
            PrepareSecuredMotherboardForProcessorAssembly(
                session,
                out motherboardAttachId,
                out motherboardSecureId);
            Assert.That(session.PickupStagedProcessorForAssembly().IsSuccess, Is.True);
            processorSeatId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue93.seat-processor");
            Assert.That(session.SeatProcessor(
                processorSeatId,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            processorRetainId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue93.retain-processor");
            Assert.That(session.CloseProcessorRetention(
                processorRetainId,
                processorSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
        }
    }
}
