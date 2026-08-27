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
        public void Eps12vAssemblyHandoffRequiresExactRoutedAtx24WithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedPowerSupplyForAtx24Assembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot eps12v = CanonicalEps12vLine(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> blocked =
                session.PickupStagedEps12vPowerCableForAssembly();

            Assert.That(blocked.Error, Is.EqualTo(
                CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision, Is.Zero);
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision, Is.Zero);
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(session.Eps12vPowerCableBuildKitContainerId));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Eps12vAssemblyHandoffRejectsEveryStaleRevisionWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24ForEps12vAssembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot eps12v = CanonicalEps12vLine(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long cableRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            int handoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;

            Assert.That(session.PickupStagedEps12vPowerCableForAssembly(
                buildKitRevision - 1L,
                inventoryRevision,
                assemblyRevision,
                cableRevision,
                atx24Revision).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.PickupStagedEps12vPowerCableForAssembly(
                buildKitRevision,
                inventoryRevision,
                assemblyRevision - 1L,
                cableRevision,
                atx24Revision).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedEps12vPowerCableForAssembly(
                buildKitRevision,
                inventoryRevision,
                assemblyRevision,
                cableRevision + 1L,
                atx24Revision).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedEps12vPowerCableForAssembly(
                buildKitRevision,
                inventoryRevision,
                assemblyRevision,
                cableRevision,
                atx24Revision + 1L).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                staleInventory = session.CustomPcBuildKit
                    .PrepareInventoryEps12vPowerCableAssemblyHandoffForRecovery(
                        session.PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                        workOrder,
                        session.Eps12vPowerCableRouteContainerId,
                        buildKitRevision,
                        inventoryRevision - 1L);
            Assert.That(staleInventory.Error, Is.EqualTo(
                InventoryFailures.SerializedTransferPlanStale));

            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(cableRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(handoffCount));
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(session.Eps12vPowerCableBuildKitContainerId));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Eps12vBuildKitRouteCyclePreservesAtx24PcieAndExactLineage()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24ForEps12vAssembly(
                session,
                out StableId<AssemblyOperationIdScope> powerSupplySeatId,
                out StableId<AssemblyOperationIdScope> powerSupplyRetainId,
                out StableId<AssemblyOperationIdScope> atx24RouteId);
            CustomPcBuildOrderLineSnapshot eps12v = CanonicalEps12vLine(workOrder);
            CustomPcBuildOrderLineSnapshot atx24 = CanonicalAtx24Line(workOrder);
            CustomPcBuildOrderLineSnapshot pcie = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            StableId<ContainerIdScope> pcieContainer =
                GetItem(session, pcie.ItemId).ContainerId;
            long protectedAtx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int protectedAtx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeEps12vPowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt historicalStaging), Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedEps12vPowerCableForAssembly();

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind,
                Is.EqualTo(PcComponentKind.PowerCable));
            Assert.That(handoff.Value.Line, Is.SameAs(eps12v));
            Assert.That(handoff.Value.Line.PowerCableType, Is.EqualTo(
                PowerCableType.ModularEps12v8PinPsuToMotherboard));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(historicalStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(9));
            AssertReservationStillLive(session, eps12v);
            AssertAtx24AndPcieProtected(
                session,
                atx24,
                atx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                pcie,
                pcieContainer);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedEps12vPowerCableForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));
            Assert.That(session.DropHeldEps12vPowerCableToWorld().Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> routeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue107.route-eps12v");
            OperationResult<Eps12vPowerCableOperationReceipt> route =
                session.RouteEps12vPowerCable(
                    routeId,
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.Eps12vPowerCableRevision);
            Assert.That(route.IsSuccess, Is.True, route.Error.Code);
            Assert.That(route.Value.ItemId, Is.EqualTo(eps12v.ItemId));
            Assert.That(route.Value.ProductId, Is.EqualTo(eps12v.ProductId));
            Assert.That(route.Value.SourcePowerSupplyRetentionOperationId,
                Is.EqualTo(powerSupplyRetainId));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Routed));
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(session.UnretainPowerSupply(
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue107.blocked-unretain-psu"),
                powerSupplySeatId,
                powerSupplyRetainId,
                session.AssemblyBuild.Revision).Error,
                Is.EqualTo(AssemblyFailures.PowerCableDependentComponentLocked));

            StableId<AssemblyOperationIdScope> unrouteId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue107.unroute-eps12v");
            OperationResult<Eps12vPowerCableOperationReceipt> unroute =
                session.UnrouteEps12vPowerCable(
                    unrouteId,
                    routeId,
                    session.AssemblyBuild.Eps12vPowerCableRevision);
            Assert.That(unroute.IsSuccess, Is.True, unroute.Error.Code);
            Assert.That(unroute.Value.ItemId, Is.EqualTo(route.Value.ItemId));
            Assert.That(unroute.Value.SourceRouteOperationId, Is.EqualTo(routeId));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Loose));
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, eps12v);
            AssertAtx24AndPcieProtected(
                session,
                atx24,
                atx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                pcie,
                pcieContainer);
            Assert.That(session.AssemblyBuild.ValidateEps12vPowerCableReceiptHistory()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Eps12vAssemblyHandoffRetryPublishesInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24ForEps12vAssembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot eps12v = CanonicalEps12vLine(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryEps12vPowerCableAssemblyHandoffForRecovery(
                        session.PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                        workOrder,
                        session.Eps12vPowerCableRouteContainerId,
                        buildKitRevision,
                        inventoryRevision);
            Assert.That(inventoryCommit.IsSuccess, Is.True,
                inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(8));
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, eps12v);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedEps12vPowerCableForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision,
                    session.AssemblyBuild.Eps12vPowerCableRevision,
                    session.AssemblyBuild.Atx24PowerCableRevision);
            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(9));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(9));

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedEps12vPowerCableForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(retry.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareRoutedAtx24ForEps12vAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> powerSupplySeatId,
            out StableId<AssemblyOperationIdScope> powerSupplyRetainId,
            out StableId<AssemblyOperationIdScope> atx24RouteId)
        {
            PrepareRetainedPowerSupplyForAtx24Assembly(
                session,
                out powerSupplySeatId,
                out powerSupplyRetainId,
                out _);
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedAtx24PowerCableForAssembly();
            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            atx24RouteId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue107.prepare-route-atx24");
            OperationResult<Atx24PowerCableOperationReceipt> route =
                session.RouteAtx24PowerCable(
                    atx24RouteId,
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.Atx24PowerCableRevision);
            Assert.That(route.IsSuccess, Is.True, route.Error.Code);
            Assert.That(session.AssemblyBuild.IsAtx24PowerCableRouted, Is.True);
            Assert.That(GetItem(session, session.Atx24PowerCableItemId).ContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
        }

        private static CustomPcBuildOrderLineSnapshot CanonicalEps12vLine(
            CustomPcBuildOrderRecord workOrder)
        {
            return workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularEps12v8PinPsuToMotherboard);
        }

        private static void AssertAtx24AndPcieProtected(
            GarageStockFlowSession session,
            CustomPcBuildOrderLineSnapshot atx24,
            StableId<AssemblyOperationIdScope> atx24RouteId,
            long atx24Revision,
            int atx24ReceiptCount,
            CustomPcBuildOrderLineSnapshot pcie,
            StableId<ContainerIdScope> pcieContainer)
        {
            Assert.That(GetItem(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(Atx24PowerCableState.Routed));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRoutedByOperationId,
                Is.EqualTo(atx24RouteId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(atx24ReceiptCount));
            Assert.That(GetItem(session, pcie.ItemId).ContainerId,
                Is.EqualTo(pcieContainer));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(PcieGpuPowerCableState.Loose));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision, Is.Zero);
        }
    }
}
