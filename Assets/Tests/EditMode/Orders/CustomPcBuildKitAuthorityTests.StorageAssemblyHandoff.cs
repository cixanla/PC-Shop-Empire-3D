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
        public void RetainedMemoryAllowsCanonicalStorageCaptiveScrewRoundTrip()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedMemoryForStorageAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out _,
                out _,
                out _,
                out _);

            CustomPcBuildOrderLineSnapshot storage = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.StorageDevice);
            CustomPcBuildOrderLineSnapshot motherboard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            CustomPcBuildOrderLineSnapshot memory = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.MemoryModule);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule &&
                                   line.ComponentKind != PcComponentKind.StorageDevice)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetItem(session, line.ItemId).ContainerId);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeStorageBuildKitOperationId,
                out CustomPcBuildKitReceipt originalStorageStaging), Is.True);
            Assert.That(originalStorageStaging.Stage,
                Is.EqualTo(CustomPcBuildKitStage.StorageStaged));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedStorageForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    session.AssemblyBuild.Revision);

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind,
                Is.EqualTo(PcComponentKind.StorageDevice));
            Assert.That(handoff.Value.Line, Is.SameAs(storage));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(originalStorageStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.StorageSlotContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(4));
            Assert.That(GetItem(session, storage.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(GetItem(session, motherboard.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(GetItem(session, processor.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            Assert.That(GetItem(session, memory.ItemId).ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            AssertReservationStillLive(session, storage);
            AssertUntouchedContainers(session, untouchedContainers);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedStorageForAssembly();
            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));

            OperationResult blockedDrop = session.DropHeldStorageToWorld();
            Assert.That(blockedDrop.IsFailure, Is.True);
            Assert.That(blockedDrop.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> seatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.seat-storage");
            Assert.That(session.SeatStorageDevice(
                seatId,
                M2KeyOrientation.KeyAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            Assert.That(GetItem(session, storage.ItemId).ContainerId,
                Is.EqualTo(session.StorageSlotContainerId));

            StableId<AssemblyOperationIdScope> secureId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.tighten-captive-screw");
            Assert.That(session.SecureStorageDevice(
                secureId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));

            StableId<AssemblyOperationIdScope> unsecureId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.loosen-captive-screw");
            Assert.That(session.UnsecureStorageDevice(
                unsecureId,
                seatId,
                secureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> removeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.remove-storage");
            Assert.That(session.RemoveStorageDevice(
                removeId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(GetItem(session, storage.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, storage);

            StableId<AssemblyOperationIdScope> reseatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.reseat-storage");
            Assert.That(session.SeatStorageDevice(
                reseatId,
                M2KeyOrientation.KeyAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> resecureId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.retighten-captive-screw");
            Assert.That(session.SecureStorageDevice(
                resecureId,
                reseatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeStorageBuildKitOperationId,
                out CustomPcBuildKitReceipt preservedStorageStaging), Is.True);
            Assert.That(preservedStorageStaging, Is.SameAs(originalStorageStaging));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(4));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
            Assert.That(GetItem(session, storage.ItemId).ContainerId,
                Is.EqualTo(session.StorageSlotContainerId));
            AssertUntouchedContainers(session, untouchedContainers);

            long delayedReplayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long delayedReplayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> delayedReplay =
                session.CustomPcBuildKit.ReleaseCanonicalStorageForAssembly(
                    session.PrototypeStorageAssemblyHandoffOperationId,
                    workOrder,
                    session.StorageSlotContainerId,
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
        public void StorageAssemblyHandoffFailsClosedUntilExactMemoryIsRetained()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);

            long originalBuildKitRevision = session.CustomPcBuildKit.Revision;
            long originalInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> motherboardMissing =
                session.PickupStagedStorageForAssembly();
            Assert.That(motherboardMissing.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(originalBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(originalInventoryRevision));

            PrepareRetainedProcessorForMemoryAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out _,
                out _);
            long memoryMissingBuildKitRevision = session.CustomPcBuildKit.Revision;
            long memoryMissingInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> memoryMissing =
                session.PickupStagedStorageForAssembly();
            Assert.That(memoryMissing.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(memoryMissingBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(memoryMissingInventoryRevision));

            Assert.That(session.PickupStagedMemoryModuleForAssembly().IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> memorySeatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.precondition-seat-memory");
            Assert.That(session.SeatMemoryModule(
                memorySeatId,
                DimmKeyOrientation.NotchAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> memoryOpen =
                session.PickupStagedStorageForAssembly();
            Assert.That(memoryOpen.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            long staleAssemblyRevision = session.AssemblyBuild.Revision;
            StableId<AssemblyOperationIdScope> memoryRetainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue95.precondition-retain-memory");
            Assert.That(session.CloseMemoryRetention(
                memoryRetainId,
                memorySeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> staleAssembly =
                session.PickupStagedStorageForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    staleAssemblyRevision);
            Assert.That(staleAssembly.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignTarget =
                session.CustomPcBuildKit.ReleaseCanonicalStorageForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue95.foreign"),
                    workOrder,
                    session.WorkbenchContainerId,
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision);
            Assert.That(foreignTarget.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedStorageForAssembly();
            Assert.That(accepted.IsSuccess, Is.True, accepted.Error.Code);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalStorageForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue95.second"),
                    workOrder,
                    session.StorageSlotContainerId,
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
        public void StorageAssemblyHandoffRetryPublishesAfterInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedMemoryForStorageAssembly(
                session,
                out _,
                out _,
                out _,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot storage = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.StorageDevice);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryStorageAssemblyHandoffForRecovery(
                        session.PrototypeStorageAssemblyHandoffOperationId,
                        workOrder,
                        session.StorageSlotContainerId,
                        buildKitRevision,
                        inventoryRevision);

            Assert.That(inventoryCommit.IsSuccess, Is.True, inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(3));
            Assert.That(GetItem(session, storage.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, storage);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedStorageForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision);

            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(4));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(4));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StorageInventoryReplayRejectsMotherboardFamilyReceipt()
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
                    .ReleaseReservedStorageForAssembly(
                        motherboardCommit.Value.PlacementReceipt,
                        motherboardCommit.Value.OperationId,
                        session.WorkbenchContainerId,
                        expectedInventoryRevision: -1L);

            Assert.That(wrongFamilyReplay.Error, Is.EqualTo(
                InventoryFailures
                    .SerializedReservationWorkOrderBuildKitAssemblyConflict));
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

        private static void PrepareRetainedMemoryForStorageAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> motherboardAttachId,
            out StableId<AssemblyOperationIdScope> motherboardSecureId,
            out StableId<AssemblyOperationIdScope> processorSeatId,
            out StableId<AssemblyOperationIdScope> processorRetainId,
            out StableId<AssemblyOperationIdScope> memorySeatId,
            out StableId<AssemblyOperationIdScope> memoryRetainId)
        {
            PrepareRetainedProcessorForMemoryAssembly(
                session,
                out motherboardAttachId,
                out motherboardSecureId,
                out processorSeatId,
                out processorRetainId);
            Assert.That(session.PickupStagedMemoryModuleForAssembly().IsSuccess, Is.True);
            memorySeatId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue95.seat-memory");
            Assert.That(session.SeatMemoryModule(
                memorySeatId,
                DimmKeyOrientation.NotchAligned,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            memoryRetainId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue95.retain-memory");
            Assert.That(session.CloseMemoryRetention(
                memoryRetainId,
                memorySeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
        }
    }
}
