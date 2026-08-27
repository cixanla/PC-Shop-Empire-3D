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
        public void Atx24AssemblyHandoffRejectsStaleRevisionsWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedPowerSupplyForAtx24Assembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot atx24 = CanonicalAtx24Line(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long cableRevision = session.AssemblyBuild.Atx24PowerCableRevision;
            int handoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> staleBuildKit =
                session.PickupStagedAtx24PowerCableForAssembly(
                    buildKitRevision - 1L,
                    inventoryRevision,
                    assemblyRevision,
                    cableRevision);
            Assert.That(staleBuildKit.Error, Is.EqualTo(
                CustomPcWorkOrderFailures.BuildKitRevisionStale));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> staleAssembly =
                session.PickupStagedAtx24PowerCableForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    assemblyRevision - 1L,
                    cableRevision);
            Assert.That(staleAssembly.Error, Is.EqualTo(
                CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> staleCable =
                session.PickupStagedAtx24PowerCableForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    assemblyRevision,
                    cableRevision + 1L);
            Assert.That(staleCable.Error, Is.EqualTo(
                CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                staleInventory = session.CustomPcBuildKit
                    .PrepareInventoryAtx24PowerCableAssemblyHandoffForRecovery(
                        session.PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                        workOrder,
                        session.Atx24PowerCableRouteContainerId,
                        buildKitRevision,
                        inventoryRevision - 1L);
            Assert.That(staleInventory.Error, Is.EqualTo(
                InventoryFailures.SerializedTransferPlanStale));

            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(cableRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(handoffCount));
            Assert.That(GetItem(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.Atx24PowerCableBuildKitContainerId));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Atx24BuildKitRouteCyclePreservesExactLineageAndOtherCables()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedPowerSupplyForAtx24Assembly(
                session,
                out StableId<AssemblyOperationIdScope> powerSupplySeatId,
                out StableId<AssemblyOperationIdScope> powerSupplyRetainId,
                out _);
            CustomPcBuildOrderLineSnapshot atx24 = CanonicalAtx24Line(workOrder);
            CustomPcBuildOrderLineSnapshot eps12v = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularEps12v8PinPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot pcie = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            StableId<ContainerIdScope> eps12vContainer =
                GetItem(session, eps12v.ItemId).ContainerId;
            StableId<ContainerIdScope> pcieContainer =
                GetItem(session, pcie.ItemId).ContainerId;
            Eps12vPowerCableState eps12vState =
                session.AssemblyBuild.Eps12vPowerCableState;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeAtx24PowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt historicalStaging), Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedAtx24PowerCableForAssembly();

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind, Is.EqualTo(PcComponentKind.PowerCable));
            Assert.That(handoff.Value.Line, Is.SameAs(atx24));
            Assert.That(handoff.Value.Line.PowerCableType, Is.EqualTo(
                PowerCableType.ModularAtx24SplitPsuToMotherboard));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(historicalStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
            Assert.That(GetItem(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(8));
            AssertReservationStillLive(session, atx24);
            AssertOtherPowerCablesUntouched(
                session,
                eps12v,
                eps12vContainer,
                eps12vState,
                pcie,
                pcieContainer,
                pcieState);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedAtx24PowerCableForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));

            Assert.That(session.DropHeldAtx24PowerCableToWorld().Error, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> routeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue105.route-atx24");
            OperationResult<Atx24PowerCableOperationReceipt> route =
                session.RouteAtx24PowerCable(
                    routeId,
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.Atx24PowerCableRevision);
            Assert.That(route.IsSuccess, Is.True, route.Error.Code);
            Assert.That(route.Value.ItemId, Is.EqualTo(atx24.ItemId));
            Assert.That(route.Value.ProductId, Is.EqualTo(atx24.ProductId));
            Assert.That(route.Value.SourceMotherboardSecureOperationId,
                Is.EqualTo(session.AssemblyBuild.SecuredByOperationId));
            Assert.That(route.Value.SourcePowerSupplyRetentionOperationId,
                Is.EqualTo(powerSupplyRetainId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(Atx24PowerCableState.Routed));
            Assert.That(GetItem(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
            AssertReservationStillLive(session, atx24);

            long routedReplayBuildKitRevision =
                session.CustomPcBuildKit.Revision;
            long routedReplayInventoryRevision = session.Inventory.Revision;
            long routedReplayAssemblyRevision = session.AssemblyBuild.Revision;
            int routedReplayAssemblyReceiptCount =
                session.AssemblyBuild.ReceiptCount;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
                routedHandoffReplay =
                    session.PickupStagedAtx24PowerCableForAssembly();
            Assert.That(routedHandoffReplay.IsSuccess, Is.True,
                routedHandoffReplay.Error.Code);
            Assert.That(routedHandoffReplay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(routedReplayBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(routedReplayInventoryRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(routedReplayAssemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(routedReplayAssemblyReceiptCount));
            Assert.That(session.UnretainPowerSupply(
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue105.blocked-unretain-psu"),
                powerSupplySeatId,
                powerSupplyRetainId,
                session.AssemblyBuild.Revision).Error,
                Is.EqualTo(AssemblyFailures.PowerCableDependentComponentLocked));

            StableId<AssemblyOperationIdScope> unrouteId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue105.unroute-atx24");
            OperationResult<Atx24PowerCableOperationReceipt> unroute =
                session.UnrouteAtx24PowerCable(
                    unrouteId,
                    routeId,
                    session.AssemblyBuild.Atx24PowerCableRevision);
            Assert.That(unroute.IsSuccess, Is.True, unroute.Error.Code);
            Assert.That(unroute.Value.ItemId, Is.EqualTo(route.Value.ItemId));
            Assert.That(unroute.Value.SourceRouteOperationId, Is.EqualTo(routeId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(Atx24PowerCableState.Loose));
            Assert.That(GetItem(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, atx24);

            long unroutedReplayBuildKitRevision =
                session.CustomPcBuildKit.Revision;
            long unroutedReplayInventoryRevision = session.Inventory.Revision;
            long unroutedReplayAssemblyRevision = session.AssemblyBuild.Revision;
            int unroutedReplayAssemblyReceiptCount =
                session.AssemblyBuild.ReceiptCount;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
                unroutedHandoffReplay =
                    session.PickupStagedAtx24PowerCableForAssembly();
            Assert.That(unroutedHandoffReplay.IsSuccess, Is.True,
                unroutedHandoffReplay.Error.Code);
            Assert.That(unroutedHandoffReplay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(unroutedReplayBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(unroutedReplayInventoryRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(unroutedReplayAssemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(unroutedReplayAssemblyReceiptCount));
            AssertOtherPowerCablesUntouched(
                session,
                eps12v,
                eps12vContainer,
                eps12vState,
                pcie,
                pcieContainer,
                pcieState);
            Assert.That(session.AssemblyBuild.ValidateAtx24PowerCableReceiptHistory()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Atx24AssemblyHandoffFailsClosedUntilExactPowerSupplyIsRetained()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedGraphicsCardForPowerSupplyAssembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot atx24 = CanonicalAtx24Line(workOrder);

            Assert.That(session.PickupStagedAtx24PowerCableForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedPowerSupplyForAssembly().IsSuccess, Is.True);
            Assert.That(session.PickupStagedAtx24PowerCableForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            StableId<AssemblyOperationIdScope> seatId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue105.precondition-seat-psu");
            Assert.That(session.SeatPowerSupply(
                seatId,
                PowerSupplyMountOrientation.FanToFilteredVent,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.PickupStagedAtx24PowerCableForAssembly().Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            StableId<AssemblyOperationIdScope> retainId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue105.precondition-retain-psu");
            Assert.That(session.RetainPowerSupply(
                retainId,
                seatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);

            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignTarget =
                session.CustomPcBuildKit.ReleaseCanonicalAtx24PowerCableForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue105.foreign"),
                    workOrder,
                    session.PowerSupplyBayContainerId,
                    buildKitRevision,
                    inventoryRevision);
            Assert.That(foreignTarget.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyWorkbenchInvalid));
            Assert.That(GetItem(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.Atx24PowerCableBuildKitContainerId));

            GarageStockFlowSession foreignSession = CreateIssuedSession(
                out CustomPcBuildOrderRecord foreignOrder);
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> foreignOrderResult =
                session.CustomPcBuildKit.ReleaseCanonicalAtx24PowerCableForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue105.foreign-order"),
                    foreignOrder,
                    session.Atx24PowerCableRouteContainerId,
                    buildKitRevision,
                    inventoryRevision);
            Assert.That(foreignOrderResult.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitWorkOrderInvalid));
            Assert.That(foreignSession.ValidateInvariants().IsSuccess, Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> accepted =
                session.PickupStagedAtx24PowerCableForAssembly();
            Assert.That(accepted.IsSuccess, Is.True, accepted.Error.Code);
            long acceptedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long acceptedInventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> secondIdentity =
                session.CustomPcBuildKit.ReleaseCanonicalAtx24PowerCableForAssembly(
                    StableId<CustomPcBuildKitAssemblyOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-assembly-operation.issue105.second"),
                    workOrder,
                    session.Atx24PowerCableRouteContainerId,
                    acceptedBuildKitRevision,
                    acceptedInventoryRevision);
            Assert.That(secondIdentity.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitAssemblyIdentityConflict));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(acceptedBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(acceptedInventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Atx24AssemblyHandoffRetryPublishesAfterInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedPowerSupplyForAtx24Assembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot atx24 = CanonicalAtx24Line(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryAtx24PowerCableAssemblyHandoffForRecovery(
                        session.PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                        workOrder,
                        session.Atx24PowerCableRouteContainerId,
                        buildKitRevision,
                        inventoryRevision);
            Assert.That(inventoryCommit.IsSuccess, Is.True,
                inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(7));
            Assert.That(GetItem(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, atx24);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedAtx24PowerCableForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision,
                    session.AssemblyBuild.Atx24PowerCableRevision);
            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(8));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(8));

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedAtx24PowerCableForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(retry.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(replayInventoryRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareRetainedPowerSupplyForAtx24Assembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> powerSupplySeatId,
            out StableId<AssemblyOperationIdScope> powerSupplyRetainId,
            out CustomPcBuildKitAssemblyHandoffReceipt powerSupplyHandoff)
        {
            PrepareRetainedGraphicsCardForPowerSupplyAssembly(
                session,
                out _,
                out _,
                out _);
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedPowerSupplyForAssembly();
            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            powerSupplyHandoff = handoff.Value;
            powerSupplySeatId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue105.prepare-seat-psu");
            Assert.That(session.SeatPowerSupply(
                powerSupplySeatId,
                PowerSupplyMountOrientation.FanToFilteredVent,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            powerSupplyRetainId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue105.prepare-retain-psu");
            Assert.That(session.RetainPowerSupply(
                powerSupplyRetainId,
                powerSupplySeatId,
                session.AssemblyBuild.Revision).IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
        }

        private static CustomPcBuildOrderLineSnapshot CanonicalAtx24Line(
            CustomPcBuildOrderRecord workOrder)
        {
            return workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularAtx24SplitPsuToMotherboard);
        }

        private static void AssertOtherPowerCablesUntouched(
            GarageStockFlowSession session,
            CustomPcBuildOrderLineSnapshot eps12v,
            StableId<ContainerIdScope> eps12vContainer,
            Eps12vPowerCableState eps12vState,
            CustomPcBuildOrderLineSnapshot pcie,
            StableId<ContainerIdScope> pcieContainer,
            PcieGpuPowerCableState pcieState)
        {
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(eps12vContainer));
            Assert.That(GetItem(session, pcie.ItemId).ContainerId,
                Is.EqualTo(pcieContainer));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(eps12vState));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision, Is.Zero);
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(pcieState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision, Is.Zero);
        }
    }
}
