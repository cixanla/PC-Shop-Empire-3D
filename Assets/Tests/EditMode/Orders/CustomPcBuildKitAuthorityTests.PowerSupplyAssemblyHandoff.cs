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
        public void PowerSupplyAssemblyHandoffRejectsStaleRevisionsWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedGraphicsCardForPowerSupplyAssembly(
                session,
                out _,
                out _,
                out _);
            AssertRetainedGraphicsCardPrerequisiteEvidence(session, workOrder);

            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int handoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;
            CustomPcBuildOrderLineSnapshot powerSupply = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerSupply);
            StableId<ContainerIdScope> container =
                GetItem(session, powerSupply.ItemId).ContainerId;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> staleBuildKit =
                session.CustomPcBuildKit.ReleaseCanonicalPowerSupplyForAssembly(
                    session.PrototypePowerSupplyAssemblyHandoffOperationId,
                    workOrder,
                    session.PowerSupplyBayContainerId,
                    buildKitRevision - 1L,
                    inventoryRevision);

            Assert.That(staleBuildKit.Error, Is.EqualTo(
                CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(handoffCount));
            Assert.That(GetItem(session, powerSupply.ItemId).ContainerId,
                Is.EqualTo(container));

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                staleInventory = session.CustomPcBuildKit
                    .PrepareInventoryPowerSupplyAssemblyHandoffForRecovery(
                        session.PrototypePowerSupplyAssemblyHandoffOperationId,
                        workOrder,
                        session.PowerSupplyBayContainerId,
                        buildKitRevision,
                        inventoryRevision - 1L);

            Assert.That(staleInventory.Error, Is.EqualTo(
                InventoryFailures.SerializedTransferPlanStale));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(handoffCount));
            Assert.That(GetItem(session, powerSupply.ItemId).ContainerId,
                Is.EqualTo(container));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypePowerSupplyAssemblyHandoffOperationId,
                out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PowerSupplyBuildKitAssemblyCyclePreservesAllCableAuthorities()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedGraphicsCardForPowerSupplyAssembly(
                session,
                out _,
                out _,
                out _);

            CustomPcBuildOrderLineSnapshot powerSupply = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerSupply);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                cableContainers = workOrder.Lines
                    .Where(line =>
                        line.ComponentKind == PcComponentKind.PowerCable)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetItem(session, line.ItemId).ContainerId);
            CableAuthoritySnapshot cables = CaptureCableAuthority(session);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypePowerSupplyBuildKitOperationId,
                out CustomPcBuildKitReceipt originalStaging), Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedPowerSupplyForAssembly(
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision,
                    session.AssemblyBuild.Revision);

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind,
                Is.EqualTo(PcComponentKind.PowerSupply));
            Assert.That(handoff.Value.Line, Is.SameAs(powerSupply));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(originalStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.PowerSupplyBayContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(7));
            Assert.That(GetItem(session, powerSupply.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, powerSupply);
            AssertUntouchedContainers(session, cableContainers);
            AssertCableAuthorityUnchanged(session, cables);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedPowerSupplyForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));

            Assert.That(session.DropHeldPowerSupplyToWorld().Error, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> seatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue102.seat-power-supply");
            OperationResult<AssemblyOperationReceipt> seat =
                session.SeatPowerSupply(
                    seatId,
                    PowerSupplyMountOrientation.FanToFilteredVent,
                    session.AssemblyBuild.Revision);
            Assert.That(seat.IsSuccess, Is.True, seat.Error.Code);
            Assert.That(seat.Value.PreviousPowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(seat.Value.ResultingPowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            Assert.That(GetItem(session, powerSupply.ItemId).ContainerId,
                Is.EqualTo(session.PowerSupplyBayContainerId));
            AssertCableAuthorityUnchanged(session, cables);

            StableId<AssemblyOperationIdScope> retainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue102.retain-four-fasteners");
            Assert.That(session.RetainPowerSupply(
                retainId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            AssertCableAuthorityUnchanged(session, cables);

            StableId<AssemblyOperationIdScope> unretainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue102.unretain-four-fasteners");
            Assert.That(session.UnretainPowerSupply(
                unretainId,
                seatId,
                retainId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            AssertCableAuthorityUnchanged(session, cables);

            StableId<AssemblyOperationIdScope> removeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue102.remove-power-supply");
            Assert.That(session.RemovePowerSupply(
                removeId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(GetItem(session, powerSupply.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, powerSupply);
            AssertUntouchedContainers(session, cableContainers);
            AssertCableAuthorityUnchanged(session, cables);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> delayedReplay =
                session.CustomPcBuildKit.ReleaseCanonicalPowerSupplyForAssembly(
                    session.PrototypePowerSupplyAssemblyHandoffOperationId,
                    workOrder,
                    session.PowerSupplyBayContainerId,
                    expectedBuildKitRevision: -1L,
                    expectedInventoryRevision: -1L);
            Assert.That(delayedReplay.IsSuccess, Is.True, delayedReplay.Error.Code);
            Assert.That(delayedReplay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PowerSupplyAssemblyHandoffFailsClosedUntilExactGraphicsCardIsRetained()
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

            Assert.That(session.PickupStagedPowerSupplyForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedGraphicsCardForAssembly().IsSuccess,
                Is.True);

            StableId<AssemblyOperationIdScope> graphicsCardSeatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue102.precondition-seat-gpu");
            Assert.That(session.SeatGraphicsCard(
                graphicsCardSeatId,
                GraphicsCardMountOrientation.Primary,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.PickupStagedPowerSupplyForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            long staleAssemblyRevision = session.AssemblyBuild.Revision;
            StableId<AssemblyOperationIdScope> graphicsCardRetainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue102.precondition-retain-gpu");
            Assert.That(session.RetainGraphicsCard(
                graphicsCardRetainId,
                graphicsCardSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.PickupStagedPowerSupplyForAssembly(
                session.CustomPcBuildKit.Revision,
                session.Inventory.Revision,
                staleAssemblyRevision).Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignTarget =
                session.CustomPcBuildKit.ReleaseCanonicalPowerSupplyForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue102.foreign"),
                    workOrder,
                    session.WorkbenchContainerId,
                    session.CustomPcBuildKit.Revision,
                    session.Inventory.Revision);
            Assert.That(foreignTarget.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedPowerSupplyForAssembly();
            Assert.That(accepted.IsSuccess, Is.True, accepted.Error.Code);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalPowerSupplyForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue102.second"),
                    workOrder,
                    session.PowerSupplyBayContainerId,
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
        public void PowerSupplyAssemblyHandoffRetryPublishesAfterInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedGraphicsCardForPowerSupplyAssembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot powerSupply = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerSupply);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryPowerSupplyAssemblyHandoffForRecovery(
                        session.PrototypePowerSupplyAssemblyHandoffOperationId,
                        workOrder,
                        session.PowerSupplyBayContainerId,
                        buildKitRevision,
                        inventoryRevision);

            Assert.That(inventoryCommit.IsSuccess, Is.True,
                inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(6));
            Assert.That(GetItem(session, powerSupply.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, powerSupply);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedPowerSupplyForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision);

            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(7));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(7));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareRetainedGraphicsCardForPowerSupplyAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> graphicsCardSeatId,
            out StableId<AssemblyOperationIdScope> graphicsCardRetainId,
            out CustomPcBuildKitAssemblyHandoffReceipt graphicsCardHandoff)
        {
            PrepareRetainedProcessorCoolerForGraphicsCardAssembly(
                session,
                out StableId<AssemblyOperationIdScope> motherboardAttachId,
                out StableId<AssemblyOperationIdScope> motherboardSecureId,
                out _,
                out _);
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedGraphicsCardForAssembly();
            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            graphicsCardHandoff = handoff.Value;
            graphicsCardSeatId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue102.prepare-seat-gpu");
            Assert.That(session.SeatGraphicsCard(
                graphicsCardSeatId,
                GraphicsCardMountOrientation.Primary,
                motherboardAttachId,
                motherboardSecureId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            graphicsCardRetainId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue102.prepare-retain-gpu");
            Assert.That(session.RetainGraphicsCard(
                graphicsCardRetainId,
                graphicsCardSeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
        }

        private static void AssertRetainedGraphicsCardPrerequisiteEvidence(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            AssemblyBuildSnapshot snapshot = session.AssemblyBuild.GetSnapshot();
            CustomPcBuildOrderLineSnapshot line = workOrder.Lines.Single(
                candidate => candidate.ComponentKind == PcComponentKind.GraphicsCard);
            Assert.That(session.AssemblyBuild.TryGetReceipt(
                snapshot.GraphicsCardSeatedByOperationId,
                out AssemblyOperationReceipt seat), Is.True, "seat receipt");
            Assert.That(session.AssemblyBuild.TryGetReceipt(
                snapshot.GraphicsCardRetainedByOperationId,
                out AssemblyOperationReceipt retain), Is.True, "retain receipt");
            Assert.That(line.ItemId, Is.EqualTo(snapshot.GraphicsCardItemId),
                "line item");
            Assert.That(line.ProductId,
                Is.EqualTo(snapshot.GraphicsCardProductId), "line product");
            Assert.That(seat.SourceContainerId,
                Is.EqualTo(session.HandsContainerId), "seat source");
            Assert.That(seat.TargetContainerId,
                Is.EqualTo(session.GraphicsCardSlotContainerId), "seat target");
            Assert.That(seat.SourceAttachOperationId,
                Is.EqualTo(snapshot.InstalledByOperationId), "seat attach");
            Assert.That(seat.SourceSecureOperationId,
                Is.EqualTo(snapshot.SecuredByOperationId), "seat secure");
            Assert.That(retain.SourceGraphicsCardSeatOperationId,
                Is.EqualTo(seat.OperationId), "retain source");
            Assert.That(retain.AssemblyRevision,
                Is.EqualTo(snapshot.Revision), "retain revision");
        }

        private readonly struct CableAuthoritySnapshot
        {
            public CableAuthoritySnapshot(GarageStockFlowSession session)
            {
                Atx24State = session.AssemblyBuild.Atx24PowerCableState;
                Atx24ItemId = session.AssemblyBuild.Atx24PowerCableItemId;
                Atx24OperationId =
                    session.AssemblyBuild.Atx24PowerCableRoutedByOperationId;
                Atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
                Eps12vState = session.AssemblyBuild.Eps12vPowerCableState;
                Eps12vItemId = session.AssemblyBuild.Eps12vPowerCableItemId;
                Eps12vOperationId =
                    session.AssemblyBuild.Eps12vPowerCableRoutedByOperationId;
                Eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
                PcieState = session.AssemblyBuild.PcieGpuPowerCableState;
                PcieItemId = session.AssemblyBuild.PcieGpuPowerCableItemId;
                PcieOperationId =
                    session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId;
                PcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            }

            public Atx24PowerCableState Atx24State { get; }
            public StableId<ItemInstanceIdScope> Atx24ItemId { get; }
            public StableId<AssemblyOperationIdScope> Atx24OperationId { get; }
            public long Atx24Revision { get; }
            public Eps12vPowerCableState Eps12vState { get; }
            public StableId<ItemInstanceIdScope> Eps12vItemId { get; }
            public StableId<AssemblyOperationIdScope> Eps12vOperationId { get; }
            public long Eps12vRevision { get; }
            public PcieGpuPowerCableState PcieState { get; }
            public StableId<ItemInstanceIdScope> PcieItemId { get; }
            public StableId<AssemblyOperationIdScope> PcieOperationId { get; }
            public long PcieRevision { get; }
        }

        private static CableAuthoritySnapshot CaptureCableAuthority(
            GarageStockFlowSession session)
        {
            return new CableAuthoritySnapshot(session);
        }

        private static void AssertCableAuthorityUnchanged(
            GarageStockFlowSession session,
            CableAuthoritySnapshot expected)
        {
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(expected.Atx24State));
            Assert.That(session.AssemblyBuild.Atx24PowerCableItemId,
                Is.EqualTo(expected.Atx24ItemId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRoutedByOperationId,
                Is.EqualTo(expected.Atx24OperationId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(expected.Atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(expected.Eps12vState));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableItemId,
                Is.EqualTo(expected.Eps12vItemId));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRoutedByOperationId,
                Is.EqualTo(expected.Eps12vOperationId));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(expected.Eps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(expected.PcieState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableItemId,
                Is.EqualTo(expected.PcieItemId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId,
                Is.EqualTo(expected.PcieOperationId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(expected.PcieRevision));
        }
    }
}
