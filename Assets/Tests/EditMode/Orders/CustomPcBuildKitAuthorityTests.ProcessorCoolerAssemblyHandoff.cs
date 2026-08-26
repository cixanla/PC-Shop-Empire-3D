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
        public void SecuredStorageAllowsCanonicalCoolerFourPointTimRoundTrip()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareSecuredStorageForProcessorCoolerAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out StableId<AssemblyOperationIdScope> processorSeatId,
                out StableId<AssemblyOperationIdScope> processorRetainId,
                out _,
                out _,
                out _,
                out _);

            CustomPcBuildOrderLineSnapshot cooler = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.ProcessorCooler);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule &&
                                   line.ComponentKind != PcComponentKind.StorageDevice &&
                                   line.ComponentKind != PcComponentKind.ProcessorCooler)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetItem(session, line.ItemId).ContainerId);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorCoolerBuildKitOperationId,
                out CustomPcBuildKitReceipt originalStaging), Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedProcessorCoolerForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    session.AssemblyBuild.Revision);

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind,
                Is.EqualTo(PcComponentKind.ProcessorCooler));
            Assert.That(handoff.Value.Line, Is.SameAs(cooler));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(originalStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.ProcessorCoolerSlotContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(5));
            Assert.That(GetItem(session, cooler.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, cooler);
            AssertUntouchedContainers(session, untouchedContainers);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedProcessorCoolerForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));

            OperationResult blockedDrop = session.DropHeldProcessorCoolerToWorld();
            Assert.That(blockedDrop.Error, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> seatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue97.seat-cooler");
            OperationResult<AssemblyOperationReceipt> seat =
                session.SeatProcessorCooler(
                    seatId,
                    ProcessorCoolerMountOrientation.Primary,
                    motherboardAttachId,
                    motherboardSecureId,
                    processorSeatId,
                    processorRetainId,
                    session.AssemblyBuild.Revision);
            Assert.That(seat.IsSuccess, Is.True, seat.Error.Code);
            Assert.That(seat.Value.PreviousProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.PreAppliedUnused));
            Assert.That(seat.Value.ResultingProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.AppliedConsumed));
            Assert.That(GetItem(session, cooler.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorCoolerSlotContainerId));

            StableId<AssemblyOperationIdScope> retainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue97.retain-cooler-1-3-2-4");
            Assert.That(session.RetainProcessorCooler(
                retainId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));

            StableId<AssemblyOperationIdScope> unretainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue97.unretain-cooler-4-2-3-1");
            Assert.That(session.UnretainProcessorCooler(
                unretainId,
                seatId,
                retainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> removeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue97.remove-cooler");
            Assert.That(session.RemoveProcessorCooler(
                removeId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(GetItem(session, cooler.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.Unsupported));
            Assert.That(
                GetItem(session, cooler.ItemId).StateFlags &
                InventorySerializedItemStateFlags.PreAppliedConsumableConsumed,
                Is.EqualTo(
                    InventorySerializedItemStateFlags.PreAppliedConsumableConsumed));
            AssertReservationStillLive(session, cooler);

            long failedReseatAssemblyRevision = session.AssemblyBuild.Revision;
            long failedReseatInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> failedReseat =
                session.SeatProcessorCooler(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue97.reseat-with-consumed-tim"),
                    ProcessorCoolerMountOrientation.Rotated180,
                    motherboardAttachId,
                    motherboardSecureId,
                    processorSeatId,
                    processorRetainId,
                    failedReseatAssemblyRevision);
            Assert.That(failedReseat.Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerTimConsumed));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(failedReseatAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(failedReseatInventoryRevision));
            Assert.That(GetItem(session, cooler.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertUntouchedContainers(session, untouchedContainers);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> delayedReplay =
                session.CustomPcBuildKit.ReleaseCanonicalProcessorCoolerForAssembly(
                    session.PrototypeProcessorCoolerAssemblyHandoffOperationId,
                    workOrder,
                    session.ProcessorCoolerSlotContainerId,
                    expectedBuildKitRevision: -1L,
                    expectedInventoryRevision: -1L);
            Assert.That(delayedReplay.IsSuccess, Is.True, delayedReplay.Error.Code);
            Assert.That(delayedReplay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CoolerAssemblyHandoffFailsClosedUntilExactStorageIsSecured()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);

            long initialBuildKitRevision = session.CustomPcBuildKit.Revision;
            long initialInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> motherboardMissing =
                session.PickupStagedProcessorCoolerForAssembly();
            Assert.That(motherboardMissing.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(initialBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(initialInventoryRevision));

            PrepareRetainedMemoryForStorageAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out _,
                out _,
                out _,
                out _);
            Assert.That(session.PickupStagedProcessorCoolerForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            Assert.That(session.PickupStagedStorageForAssembly().IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> storageSeatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue97.precondition-seat-storage");
            Assert.That(session.SeatStorageDevice(
                storageSeatId,
                M2KeyOrientation.KeyAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.PickupStagedProcessorCoolerForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            long staleAssemblyRevision = session.AssemblyBuild.Revision;
            StableId<AssemblyOperationIdScope> storageSecureId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue97.precondition-secure-storage");
            Assert.That(session.SecureStorageDevice(
                storageSecureId,
                storageSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.PickupStagedProcessorCoolerForAssembly(
                session.CustomPcBuildKit.Revision,
                session.Inventory.Revision,
                staleAssemblyRevision).Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignTarget =
                session.CustomPcBuildKit.ReleaseCanonicalProcessorCoolerForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue97.foreign"),
                    workOrder,
                    session.WorkbenchContainerId,
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision);
            Assert.That(foreignTarget.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedProcessorCoolerForAssembly();
            Assert.That(accepted.IsSuccess, Is.True, accepted.Error.Code);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalProcessorCoolerForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue97.second"),
                    workOrder,
                    session.ProcessorCoolerSlotContainerId,
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
        public void CoolerAssemblyHandoffRetryPublishesAfterInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareSecuredStorageForProcessorCoolerAssembly(
                session,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot cooler = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.ProcessorCooler);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryProcessorCoolerAssemblyHandoffForRecovery(
                        session.PrototypeProcessorCoolerAssemblyHandoffOperationId,
                        workOrder,
                        session.ProcessorCoolerSlotContainerId,
                        buildKitRevision,
                        inventoryRevision);

            Assert.That(inventoryCommit.IsSuccess, Is.True, inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(4));
            Assert.That(GetItem(session, cooler.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, cooler);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedProcessorCoolerForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision);

            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(5));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(5));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CoolerInventoryReplayRejectsMotherboardFamilyReceipt()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                motherboardCommit = session.CustomPcBuildKit
                    .PrepareInventoryMotherboardAssemblyHandoffForRecovery(
                        session.PrototypeMotherboardAssemblyHandoffOperationId,
                        workOrder,
                        session.WorkbenchContainerId,
                        buildKitRevision,
                        inventoryRevision);
            Assert.That(motherboardCommit.IsSuccess, Is.True,
                motherboardCommit.Error.Code);
            long committedInventoryRevision = session.Inventory.Revision;
            int committedHandoffCount = session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                wrongFamilyReplay = session.Inventory
                    .ReleaseReservedProcessorCoolerForAssembly(
                        motherboardCommit.Value.PlacementReceipt,
                        motherboardCommit.Value.OperationId,
                        session.WorkbenchContainerId,
                        expectedInventoryRevision: -1L);

            Assert.That(wrongFamilyReplay.Error, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitAssemblyConflict));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(committedInventoryRevision));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(committedHandoffCount));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareSecuredStorageForProcessorCoolerAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> motherboardAttachId,
            out StableId<AssemblyOperationIdScope> motherboardSecureId,
            out StableId<AssemblyOperationIdScope> processorSeatId,
            out StableId<AssemblyOperationIdScope> processorRetainId,
            out StableId<AssemblyOperationIdScope> memorySeatId,
            out StableId<AssemblyOperationIdScope> memoryRetainId,
            out StableId<AssemblyOperationIdScope> storageSeatId,
            out StableId<AssemblyOperationIdScope> storageSecureId)
        {
            PrepareRetainedMemoryForStorageAssembly(
                session,
                out motherboardAttachId,
                out motherboardSecureId,
                out processorSeatId,
                out processorRetainId,
                out memorySeatId,
                out memoryRetainId);
            Assert.That(session.PickupStagedStorageForAssembly().IsSuccess, Is.True);
            storageSeatId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue97.seat-storage");
            Assert.That(session.SeatStorageDevice(
                storageSeatId,
                M2KeyOrientation.KeyAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            storageSecureId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue97.secure-storage");
            Assert.That(session.SecureStorageDevice(
                storageSecureId,
                storageSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
        }
    }
}
