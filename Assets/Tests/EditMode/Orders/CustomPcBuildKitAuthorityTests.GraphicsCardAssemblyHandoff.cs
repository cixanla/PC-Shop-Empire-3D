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
        public void GraphicsCardAssemblyHandoffRejectsStaleBuildKitRevisionWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedProcessorCoolerForGraphicsCardAssembly(
                session,
                out _,
                out _,
                out _,
                out _);

            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int handoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;
            CustomPcBuildOrderLineSnapshot graphicsCard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.GraphicsCard);
            StableId<ContainerIdScope> container =
                GetItem(session, graphicsCard.ItemId).ContainerId;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> result =
                session.CustomPcBuildKit.ReleaseCanonicalGraphicsCardForAssembly(
                    session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                    workOrder,
                    session.GraphicsCardSlotContainerId,
                    buildKitRevision - 1,
                    inventoryRevision);

            Assert.That(result.Error, Is.EqualTo(
                CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(handoffCount));
            Assert.That(GetItem(session, graphicsCard.ItemId).ContainerId,
                Is.EqualTo(container));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void GraphicsCardInventoryAssemblyHandoffRejectsStaleRevisionWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedProcessorCoolerForGraphicsCardAssembly(
                session,
                out _,
                out _,
                out _,
                out _);

            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int handoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;
            CustomPcBuildOrderLineSnapshot graphicsCard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.GraphicsCard);
            StableId<ContainerIdScope> container =
                GetItem(session, graphicsCard.ItemId).ContainerId;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                result = session.CustomPcBuildKit
                    .PrepareInventoryGraphicsCardAssemblyHandoffForRecovery(
                        session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                        workOrder,
                        session.GraphicsCardSlotContainerId,
                        buildKitRevision,
                        inventoryRevision - 1);

            Assert.That(result.Error, Is.EqualTo(
                InventoryFailures.SerializedTransferPlanStale));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(handoffCount));
            Assert.That(GetItem(session, graphicsCard.ItemId).ContainerId,
                Is.EqualTo(container));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void GraphicsCardSeatRetentionUnretentionAndRemovalRejectStaleRevisionWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedProcessorCoolerForGraphicsCardAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out _,
                out _);
            Assert.That(session.PickupStagedGraphicsCardForAssembly().IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> seatId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue99.stale-seat");
            long staleRevision = session.AssemblyBuild.Revision - 1;
            OperationResult<AssemblyOperationReceipt> staleSeat = session.SeatGraphicsCard(
                seatId,
                GraphicsCardMountOrientation.Primary,
                motherboardAttachId,
                motherboardSecureId,
                staleRevision);
            Assert.That(staleSeat.Error, Is.EqualTo(AssemblyFailures.PlanStale));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));

            Assert.That(session.SeatGraphicsCard(
                seatId,
                GraphicsCardMountOrientation.Primary,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> retainId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue99.stale-retain");
            staleRevision = session.AssemblyBuild.Revision - 1;
            Assert.That(session.RetainGraphicsCard(
                retainId,
                seatId,
                staleRevision).Error, Is.EqualTo(AssemblyFailures.PlanStale));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            Assert.That(session.RetainGraphicsCard(
                retainId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> unretainId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue99.stale-unretain");
            staleRevision = session.AssemblyBuild.Revision - 1;
            Assert.That(session.UnretainGraphicsCard(
                unretainId,
                seatId,
                retainId,
                staleRevision).Error, Is.EqualTo(AssemblyFailures.PlanStale));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(session.UnretainGraphicsCard(
                unretainId,
                seatId,
                retainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            StableId<AssemblyOperationIdScope> removeId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue99.stale-remove");
            staleRevision = session.AssemblyBuild.Revision - 1;
            Assert.That(session.RemoveGraphicsCard(
                removeId,
                seatId,
                staleRevision).Error, Is.EqualTo(AssemblyFailures.PlanStale));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            Assert.That(session.RemoveGraphicsCard(
                removeId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void GraphicsCardBuildKitAssemblyCyclePreservesIndependentPcieGpuPowerCableAuthority()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedProcessorCoolerForGraphicsCardAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out _,
                out _);

            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            StableId<ItemInstanceIdScope> pcieItemId =
                session.AssemblyBuild.PcieGpuPowerCableItemId;
            StableId<ProductDefinitionIdScope> pcieProductId =
                session.AssemblyBuild.PcieGpuPowerCableProductId;
            StableId<AssemblyOperationIdScope> pcieOperationId =
                session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId;
            StableId<ContainerIdScope> pcieContainerId =
                session.AssemblyBuild.PcieGpuPowerCableRouteContainerId;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount = session.AssemblyBuild.PcieGpuPowerCableReceiptCount;

            CustomPcBuildOrderLineSnapshot graphicsCard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.GraphicsCard);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule &&
                                   line.ComponentKind != PcComponentKind.StorageDevice &&
                                   line.ComponentKind != PcComponentKind.ProcessorCooler &&
                                   line.ComponentKind != PcComponentKind.GraphicsCard)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetItem(session, line.ItemId).ContainerId);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeGraphicsCardBuildKitOperationId,
                out CustomPcBuildKitReceipt originalStaging), Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedGraphicsCardForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    session.AssemblyBuild.Revision);

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind,
                Is.EqualTo(PcComponentKind.GraphicsCard));
            Assert.That(handoff.Value.Line, Is.SameAs(graphicsCard));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(originalStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.GraphicsCardSlotContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(6));
            Assert.That(GetItem(session, graphicsCard.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, graphicsCard);
            AssertUntouchedContainers(session, untouchedContainers);
            AssertPcieGpuPowerCableAuthorityUnchanged(
                session, pcieState, pcieItemId, pcieProductId, pcieOperationId,
                pcieContainerId, pcieRevision, pcieReceiptCount);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedGraphicsCardForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));

            OperationResult blockedDrop = session.DropHeldGraphicsCardToWorld();
            Assert.That(blockedDrop.Error, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> seatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue99.seat-graphics-card");
            OperationResult<AssemblyOperationReceipt> seat =
                session.SeatGraphicsCard(
                    seatId,
                    GraphicsCardMountOrientation.Primary,
                    motherboardAttachId,
                    motherboardSecureId,
                    session.AssemblyBuild.Revision);
            Assert.That(seat.IsSuccess, Is.True, seat.Error.Code);
            Assert.That(seat.Value.PreviousGraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            Assert.That(seat.Value.ResultingGraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            Assert.That(GetItem(session, graphicsCard.ItemId).ContainerId,
                Is.EqualTo(session.GraphicsCardSlotContainerId));
            AssertPcieGpuPowerCableAuthorityUnchanged(
                session, pcieState, pcieItemId, pcieProductId, pcieOperationId,
                pcieContainerId, pcieRevision, pcieReceiptCount);

            StableId<AssemblyOperationIdScope> retainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue99.retain-latch-bracket");
            Assert.That(session.RetainGraphicsCard(
                retainId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            AssertPcieGpuPowerCableAuthorityUnchanged(
                session, pcieState, pcieItemId, pcieProductId, pcieOperationId,
                pcieContainerId, pcieRevision, pcieReceiptCount);

            StableId<AssemblyOperationIdScope> unretainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue99.unretain-bracket-latch");
            Assert.That(session.UnretainGraphicsCard(
                unretainId,
                seatId,
                retainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            AssertPcieGpuPowerCableAuthorityUnchanged(
                session, pcieState, pcieItemId, pcieProductId, pcieOperationId,
                pcieContainerId, pcieRevision, pcieReceiptCount);

            StableId<AssemblyOperationIdScope> removeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue99.remove-graphics-card");
            Assert.That(session.RemoveGraphicsCard(
                removeId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(GetItem(session, graphicsCard.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            AssertReservationStillLive(session, graphicsCard);
            AssertUntouchedContainers(session, untouchedContainers);
            AssertPcieGpuPowerCableAuthorityUnchanged(
                session, pcieState, pcieItemId, pcieProductId, pcieOperationId,
                pcieContainerId, pcieRevision, pcieReceiptCount);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> delayedReplay =
                session.CustomPcBuildKit.ReleaseCanonicalGraphicsCardForAssembly(
                    session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                    workOrder,
                    session.GraphicsCardSlotContainerId,
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
        public void GraphicsCardAssemblyHandoffFailsClosedUntilExactCoolerIsRetained()
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

            Assert.That(session.PickupStagedGraphicsCardForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedProcessorCoolerForAssembly().IsSuccess,
                Is.True);

            StableId<AssemblyOperationIdScope> coolerSeatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue99.precondition-seat-cooler");
            Assert.That(session.SeatProcessorCooler(
                coolerSeatId,
                ProcessorCoolerMountOrientation.Primary,
                motherboardAttachId,
                motherboardSecureId,
                processorSeatId,
                processorRetainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.PickupStagedGraphicsCardForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            long staleAssemblyRevision = session.AssemblyBuild.Revision;
            StableId<AssemblyOperationIdScope> coolerRetainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue99.precondition-retain-cooler");
            Assert.That(session.RetainProcessorCooler(
                coolerRetainId,
                coolerSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.PickupStagedGraphicsCardForAssembly(
                session.CustomPcBuildKit.Revision,
                session.Inventory.Revision,
                staleAssemblyRevision).Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignTarget =
                session.CustomPcBuildKit.ReleaseCanonicalGraphicsCardForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue99.foreign"),
                    workOrder,
                    session.WorkbenchContainerId,
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision);
            Assert.That(foreignTarget.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedGraphicsCardForAssembly();
            Assert.That(accepted.IsSuccess, Is.True, accepted.Error.Code);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalGraphicsCardForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue99.second"),
                    workOrder,
                    session.GraphicsCardSlotContainerId,
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
        public void GraphicsCardAssemblyHandoffRetryPublishesAfterInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedProcessorCoolerForGraphicsCardAssembly(
                session,
                out _,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot graphicsCard = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.GraphicsCard);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryGraphicsCardAssemblyHandoffForRecovery(
                        session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                        workOrder,
                        session.GraphicsCardSlotContainerId,
                        buildKitRevision,
                        inventoryRevision);

            Assert.That(inventoryCommit.IsSuccess, Is.True, inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(5));
            Assert.That(GetItem(session, graphicsCard.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, graphicsCard);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedGraphicsCardForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision);

            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(6));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(6));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void GraphicsCardInventoryReplayRejectsMotherboardFamilyReceipt()
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
                    .ReleaseReservedGraphicsCardForAssembly(
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

        private static void AssertPcieGpuPowerCableAuthorityUnchanged(
            GarageStockFlowSession session,
            PcieGpuPowerCableState state,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ContainerIdScope> containerId,
            long revision,
            int receiptCount)
        {
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(state));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableItemId,
                Is.EqualTo(itemId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableProductId,
                Is.EqualTo(productId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId,
                Is.EqualTo(operationId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRouteContainerId,
                Is.EqualTo(containerId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(revision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(receiptCount));
        }

        private static void PrepareRetainedProcessorCoolerForGraphicsCardAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> motherboardAttachId,
            out StableId<AssemblyOperationIdScope> motherboardSecureId,
            out StableId<AssemblyOperationIdScope> coolerSeatId,
            out StableId<AssemblyOperationIdScope> coolerRetainId)
        {
            PrepareSecuredStorageForProcessorCoolerAssembly(
                session,
                out motherboardAttachId,
                out motherboardSecureId,
                out StableId<AssemblyOperationIdScope> processorSeatId,
                out StableId<AssemblyOperationIdScope> processorRetainId,
                out _,
                out _,
                out _,
                out _);
            Assert.That(session.PickupStagedProcessorCoolerForAssembly().IsSuccess,
                Is.True);
            coolerSeatId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue99.seat-cooler");
            Assert.That(session.SeatProcessorCooler(
                coolerSeatId,
                ProcessorCoolerMountOrientation.Primary,
                motherboardAttachId,
                motherboardSecureId,
                processorSeatId,
                processorRetainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            coolerRetainId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue99.retain-cooler-1-3-2-4");
            Assert.That(session.RetainProcessorCooler(
                coolerRetainId,
                coolerSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
        }
    }
}
