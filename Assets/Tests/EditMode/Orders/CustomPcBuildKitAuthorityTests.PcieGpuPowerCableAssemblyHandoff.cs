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
        public void PcieGpuAssemblyHandoffRequiresExactRoutedAtx24AndEps12vWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24ForEps12vAssembly(
                session,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot pcieGpu = CanonicalPcieGpuLine(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> blocked =
                session.PickupStagedPcieGpuPowerCableForAssembly();

            Assert.That(blocked.Error, Is.EqualTo(
                CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(1));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision, Is.Zero);
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision, Is.Zero);
            Assert.That(GetItem(session, pcieGpu.ItemId).ContainerId,
                Is.EqualTo(session.PcieGpuPowerCableBuildKitContainerId));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PcieGpuAssemblyHandoffRejectsEveryStaleRevisionWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24AndEps12vForPcieGpuAssembly(
                session,
                out _,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot pcieGpu = CanonicalPcieGpuLine(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long cableRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            int handoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;

            Assert.That(session.PickupStagedPcieGpuPowerCableForAssembly(
                buildKitRevision - 1L,
                inventoryRevision,
                assemblyRevision,
                cableRevision,
                atx24Revision,
                eps12vRevision).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.PickupStagedPcieGpuPowerCableForAssembly(
                buildKitRevision,
                inventoryRevision,
                assemblyRevision - 1L,
                cableRevision,
                atx24Revision,
                eps12vRevision).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedPcieGpuPowerCableForAssembly(
                buildKitRevision,
                inventoryRevision,
                assemblyRevision,
                cableRevision + 1L,
                atx24Revision,
                eps12vRevision).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedPcieGpuPowerCableForAssembly(
                buildKitRevision,
                inventoryRevision,
                assemblyRevision,
                cableRevision,
                atx24Revision + 1L,
                eps12vRevision).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));
            Assert.That(session.PickupStagedPcieGpuPowerCableForAssembly(
                buildKitRevision,
                inventoryRevision,
                assemblyRevision,
                cableRevision,
                atx24Revision,
                eps12vRevision + 1L).Error, Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid));

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                staleInventory = session.CustomPcBuildKit
                    .PrepareInventoryPcieGpuPowerCableAssemblyHandoffForRecovery(
                        session.PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                        workOrder,
                        session.PcieGpuPowerCableRouteContainerId,
                        buildKitRevision,
                        inventoryRevision - 1L);
            Assert.That(staleInventory.Error, Is.EqualTo(
                InventoryFailures.SerializedTransferPlanStale));

            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(cableRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(handoffCount));
            Assert.That(GetItem(session, pcieGpu.ItemId).ContainerId,
                Is.EqualTo(session.PcieGpuPowerCableBuildKitContainerId));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ElectricalReadinessReportsCableBlockersInCanonicalOrderWithoutMutation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRetainedPowerSupplyForAtx24Assembly(
                session,
                out _,
                out _,
                out _);

            Assert.That(EvaluateElectricalBlockerWithoutMutation(session),
                Is.EqualTo(ElectricalReadinessFailures.Atx24PowerCableMissing));

            Assert.That(session.PickupStagedAtx24PowerCableForAssembly().IsSuccess,
                Is.True);
            StableId<AssemblyOperationIdScope> atx24RouteId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue119.readiness-route-atx24");
            Assert.That(session.RouteAtx24PowerCable(
                atx24RouteId,
                PowerCableKeyOrientation.Keyed,
                session.AssemblyBuild.Atx24PowerCableRevision).IsSuccess, Is.True);
            Assert.That(EvaluateElectricalBlockerWithoutMutation(session),
                Is.EqualTo(ElectricalReadinessFailures.Eps12vPowerCableMissing));

            Assert.That(session.PickupStagedEps12vPowerCableForAssembly().IsSuccess,
                Is.True);
            StableId<AssemblyOperationIdScope> eps12vRouteId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue119.readiness-route-eps12v");
            Assert.That(session.RouteEps12vPowerCable(
                eps12vRouteId,
                PowerCableKeyOrientation.Keyed,
                session.AssemblyBuild.Eps12vPowerCableRevision).IsSuccess, Is.True);
            Assert.That(EvaluateElectricalBlockerWithoutMutation(session),
                Is.EqualTo(ElectricalReadinessFailures.PcieGpuPowerCableMissing));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PcieGpuBuildKitRouteCyclePreservesAtx24Eps12vAndExactLineage()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24AndEps12vForPcieGpuAssembly(
                session,
                out StableId<AssemblyOperationIdScope> powerSupplySeatId,
                out StableId<AssemblyOperationIdScope> powerSupplyRetainId,
                out StableId<AssemblyOperationIdScope> atx24RouteId,
                out StableId<AssemblyOperationIdScope> eps12vRouteId);
            CustomPcBuildOrderLineSnapshot pcieGpu = CanonicalPcieGpuLine(workOrder);
            CustomPcBuildOrderLineSnapshot atx24 = CanonicalAtx24Line(workOrder);
            CustomPcBuildOrderLineSnapshot eps12v = CanonicalEps12vLine(workOrder);
            long protectedAtx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int protectedAtx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            long protectedEps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int protectedEps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            Assert.That(session.AssemblyBuild.EvaluateElectricalReadiness().Error,
                Is.EqualTo(ElectricalReadinessFailures.PcieGpuPowerCableMissing));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypePcieGpuPowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt historicalStaging), Is.True);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedPcieGpuPowerCableForAssembly();

            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            Assert.That(handoff.Value.ComponentKind,
                Is.EqualTo(PcComponentKind.PowerCable));
            Assert.That(handoff.Value.Line, Is.SameAs(pcieGpu));
            Assert.That(handoff.Value.Line.PowerCableType, Is.EqualTo(
                PowerCableType.ModularPcie8PinPsuToGraphicsCard));
            Assert.That(handoff.Value.StagingReceipt, Is.SameAs(historicalStaging));
            Assert.That(handoff.Value.WorkbenchContainerId,
                Is.EqualTo(session.PcieGpuPowerCableRouteContainerId));
            Assert.That(GetItem(session, pcieGpu.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(10));
            AssertReservationStillLive(session, pcieGpu);
            AssertAtx24AndEps12vProtected(
                session,
                atx24,
                atx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                eps12v,
                eps12vRouteId,
                protectedEps12vRevision,
                protectedEps12vReceiptCount);

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedPcieGpuPowerCableForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(handoff.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));
            Assert.That(session.DropHeldPcieGpuPowerCableToWorld().Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<AssemblyOperationIdScope> routeId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue109.route-pcie-gpu");
            OperationResult<PcieGpuPowerCableOperationReceipt> route =
                session.RoutePcieGpuPowerCable(
                    routeId,
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.PcieGpuPowerCableRevision);
            Assert.That(route.IsSuccess, Is.True, route.Error.Code);
            Assert.That(route.Value.ItemId, Is.EqualTo(pcieGpu.ItemId));
            Assert.That(route.Value.ProductId, Is.EqualTo(pcieGpu.ProductId));
            Assert.That(route.Value.SourcePowerSupplyRetentionOperationId,
                Is.EqualTo(powerSupplyRetainId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(PcieGpuPowerCableState.Routed));
            Assert.That(GetItem(session, pcieGpu.ItemId).ContainerId,
                Is.EqualTo(session.PcieGpuPowerCableRouteContainerId));
            long readyAssemblyRevision = session.AssemblyBuild.Revision;
            long readyInventoryRevision = session.Inventory.Revision;
            long readyBuildKitRevision = session.CustomPcBuildKit.Revision;
            long readyAtx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            long readyEps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            long readyPcieGpuRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            OperationResult<ElectricalReadinessSnapshot> readiness =
                session.AssemblyBuild.EvaluateElectricalReadiness();
            OperationResult<ElectricalReadinessSnapshot> readinessReplay =
                session.AssemblyBuild.EvaluateElectricalReadiness();
            Assert.That(readiness.IsSuccess, Is.True, readiness.Error.Code);
            Assert.That(readinessReplay.IsSuccess, Is.True,
                readinessReplay.Error.Code);
            Assert.That(readiness.Value.BuildId,
                Is.EqualTo(session.AssemblyBuild.BuildId));
            Assert.That(readiness.Value.ChassisId,
                Is.EqualTo(session.AssemblyBuild.ChassisId));
            Assert.That(readiness.Value.MotherboardItemId,
                Is.EqualTo(session.MotherboardItemId));
            Assert.That(readiness.Value.ProcessorItemId,
                Is.EqualTo(session.ProcessorItemId));
            Assert.That(readiness.Value.MemoryItemId,
                Is.EqualTo(session.MemoryItemId));
            Assert.That(readiness.Value.StorageItemId,
                Is.EqualTo(session.StorageItemId));
            Assert.That(readiness.Value.ProcessorCoolerItemId,
                Is.EqualTo(session.ProcessorCoolerItemId));
            Assert.That(readiness.Value.GraphicsCardItemId,
                Is.EqualTo(session.GraphicsCardAssemblyItemId));
            Assert.That(readiness.Value.PowerSupplyItemId,
                Is.EqualTo(session.PowerSupplyItemId));
            Assert.That(readiness.Value.Atx24PowerCableItemId,
                Is.EqualTo(session.Atx24PowerCableItemId));
            Assert.That(readiness.Value.Eps12vPowerCableItemId,
                Is.EqualTo(session.Eps12vPowerCableItemId));
            Assert.That(readiness.Value.PcieGpuPowerCableItemId,
                Is.EqualTo(pcieGpu.ItemId));
            Assert.That(readiness.Value.MotherboardSecureOperationId,
                Is.EqualTo(session.AssemblyBuild.SecuredByOperationId));
            Assert.That(readiness.Value.ProcessorRetainOperationId,
                Is.EqualTo(session.AssemblyBuild.ProcessorRetainedByOperationId));
            Assert.That(readiness.Value.MemoryRetainOperationId,
                Is.EqualTo(session.AssemblyBuild.MemoryRetainedByOperationId));
            Assert.That(readiness.Value.StorageSecureOperationId,
                Is.EqualTo(session.AssemblyBuild.StorageSecuredByOperationId));
            Assert.That(readiness.Value.ProcessorCoolerRetainOperationId,
                Is.EqualTo(
                    session.AssemblyBuild.ProcessorCoolerRetainedByOperationId));
            Assert.That(readiness.Value.GraphicsCardRetainOperationId,
                Is.EqualTo(session.AssemblyBuild.GraphicsCardRetainedByOperationId));
            Assert.That(readiness.Value.PowerSupplyRetainOperationId,
                Is.EqualTo(session.AssemblyBuild.PowerSupplyRetainedByOperationId));
            Assert.That(readiness.Value.Atx24RouteOperationId,
                Is.EqualTo(atx24RouteId));
            Assert.That(readiness.Value.Eps12vRouteOperationId,
                Is.EqualTo(eps12vRouteId));
            Assert.That(readiness.Value.PcieGpuRouteOperationId,
                Is.EqualTo(routeId));
            Assert.That(readinessReplay.Value.PcieGpuRouteOperationId,
                Is.EqualTo(readiness.Value.PcieGpuRouteOperationId));
            Assert.That(readinessReplay.Value.AssemblyRevision,
                Is.EqualTo(readiness.Value.AssemblyRevision));
            System.Reflection.FieldInfo pcieRouteLineageField =
                typeof(AssemblyBuildAuthority).GetField(
                    "_pcieGpuPowerCableRoutedByOperationId",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(pcieRouteLineageField, Is.Not.Null);
            pcieRouteLineageField.SetValue(
                session.AssemblyBuild,
                default(StableId<AssemblyOperationIdScope>));
            OperationResult<ElectricalReadinessSnapshot> invalidLineage =
                session.AssemblyBuild.EvaluateElectricalReadiness();
            pcieRouteLineageField.SetValue(session.AssemblyBuild, routeId);
            Assert.That(invalidLineage.Error,
                Is.EqualTo(ElectricalReadinessFailures.InvariantInvalid));
            Assert.That(session.AssemblyBuild.EvaluateElectricalReadiness().IsSuccess,
                Is.True);
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(readyAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(readyInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(readyBuildKitRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(readyAtx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(readyEps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(readyPcieGpuRevision));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess,
                Is.True);
            Assert.That(session.UnretainPowerSupply(
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue109.blocked-unretain-psu"),
                powerSupplySeatId,
                powerSupplyRetainId,
                session.AssemblyBuild.Revision).Error,
                Is.EqualTo(AssemblyFailures.PowerCableDependentComponentLocked));

            StableId<AssemblyOperationIdScope> unrouteId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.issue109.unroute-pcie-gpu");
            OperationResult<PcieGpuPowerCableOperationReceipt> unroute =
                session.UnroutePcieGpuPowerCable(
                    unrouteId,
                    routeId,
                    session.AssemblyBuild.PcieGpuPowerCableRevision);
            Assert.That(unroute.IsSuccess, Is.True, unroute.Error.Code);
            Assert.That(unroute.Value.ItemId, Is.EqualTo(route.Value.ItemId));
            Assert.That(unroute.Value.SourceRouteOperationId, Is.EqualTo(routeId));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(PcieGpuPowerCableState.Loose));
            Assert.That(GetItem(session, pcieGpu.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(session.AssemblyBuild.EvaluateElectricalReadiness().Error,
                Is.EqualTo(ElectricalReadinessFailures.PcieGpuPowerCableMissing));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.PowerCableMissing));
            AssertReservationStillLive(session, pcieGpu);
            AssertAtx24AndEps12vProtected(
                session,
                atx24,
                atx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                eps12v,
                eps12vRouteId,
                protectedEps12vRevision,
                protectedEps12vReceiptCount);
            Assert.That(session.AssemblyBuild.ValidatePcieGpuPowerCableReceiptHistory()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static Failure EvaluateElectricalBlockerWithoutMutation(
            GarageStockFlowSession session)
        {
            long assemblyRevision = session.AssemblyBuild.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieGpuRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;

            OperationResult<ElectricalReadinessSnapshot> result =
                session.AssemblyBuild.EvaluateElectricalReadiness();

            Assert.That(result.IsFailure, Is.True);
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieGpuRevision));
            return result.Error;
        }

        [Test]
        public void PcieGpuAssemblyHandoffRetryPublishesInventoryCommitExactlyOnce()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24AndEps12vForPcieGpuAssembly(
                session,
                out _,
                out _,
                out _,
                out _);
            CustomPcBuildOrderLineSnapshot pcieGpu = CanonicalPcieGpuLine(workOrder);
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                inventoryCommit = session.CustomPcBuildKit
                    .PrepareInventoryPcieGpuPowerCableAssemblyHandoffForRecovery(
                        session.PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                        workOrder,
                        session.PcieGpuPowerCableRouteContainerId,
                        buildKitRevision,
                        inventoryRevision);
            Assert.That(inventoryCommit.IsSuccess, Is.True,
                inventoryCommit.Error.Code);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(9));
            Assert.That(GetItem(session, pcieGpu.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            AssertReservationStillLive(session, pcieGpu);

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> retry =
                session.PickupStagedPcieGpuPowerCableForAssembly(
                    buildKitRevision,
                    inventoryRevision,
                    session.AssemblyBuild.Revision,
                    session.AssemblyBuild.PcieGpuPowerCableRevision,
                    session.AssemblyBuild.Atx24PowerCableRevision,
                    session.AssemblyBuild.Eps12vPowerCableRevision);
            Assert.That(retry.IsSuccess, Is.True, retry.Error.Code);
            Assert.That(retry.Value.InventoryAppliedRevision,
                Is.EqualTo(inventoryCommit.Value.AppliedRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(10));
            Assert.That(session.Inventory
                .SerializedReservationWorkOrderBuildKitAssemblyHandoffCount,
                Is.EqualTo(10));

            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                session.PickupStagedPcieGpuPowerCableForAssembly();
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(replay.Value, Is.SameAs(retry.Value));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(replayInventoryRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareRoutedAtx24AndEps12vForPcieGpuAssembly(
            GarageStockFlowSession session,
            out StableId<AssemblyOperationIdScope> powerSupplySeatId,
            out StableId<AssemblyOperationIdScope> powerSupplyRetainId,
            out StableId<AssemblyOperationIdScope> atx24RouteId,
            out StableId<AssemblyOperationIdScope> eps12vRouteId)
        {
            PrepareRoutedAtx24ForEps12vAssembly(
                session,
                out powerSupplySeatId,
                out powerSupplyRetainId,
                out atx24RouteId);
            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                session.PickupStagedEps12vPowerCableForAssembly();
            Assert.That(handoff.IsSuccess, Is.True, handoff.Error.Code);
            eps12vRouteId = StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue109.prepare-route-eps12v");
            OperationResult<Eps12vPowerCableOperationReceipt> route =
                session.RouteEps12vPowerCable(
                    eps12vRouteId,
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.Eps12vPowerCableRevision);
            Assert.That(route.IsSuccess, Is.True, route.Error.Code);
            Assert.That(session.AssemblyBuild.IsEps12vPowerCableRouted, Is.True);
            Assert.That(GetItem(session, session.Eps12vPowerCableItemId).ContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
        }

        private static CustomPcBuildOrderLineSnapshot CanonicalPcieGpuLine(
            CustomPcBuildOrderRecord workOrder)
        {
            return workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
        }

        private static void AssertAtx24AndEps12vProtected(
            GarageStockFlowSession session,
            CustomPcBuildOrderLineSnapshot atx24,
            StableId<AssemblyOperationIdScope> atx24RouteId,
            long atx24Revision,
            int atx24ReceiptCount,
            CustomPcBuildOrderLineSnapshot eps12v,
            StableId<AssemblyOperationIdScope> eps12vRouteId,
            long eps12vRevision,
            int eps12vReceiptCount)
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
            Assert.That(GetItem(session, eps12v.ItemId).ContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Routed));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRoutedByOperationId,
                Is.EqualTo(eps12vRouteId));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(eps12vReceiptCount));
        }
    }
}
