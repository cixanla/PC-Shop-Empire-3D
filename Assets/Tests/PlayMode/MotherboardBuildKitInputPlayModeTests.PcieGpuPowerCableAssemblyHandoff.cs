using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class MotherboardBuildKitInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator KeyboardMouseCompletesPcieGpuBuildKitRouteUnrouteCycle()
        {
            yield return RunIssue109PcieGpuAssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesPcieGpuBuildKitRouteUnrouteCycle()
        {
            yield return RunIssue109PcieGpuAssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator PcieGpuForeignRouteObstructionFailsClosedThenClears()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue109RoutedAtx24AndEps12v(marker);

            PhysicalItemProjection cable = marker.PcieGpuPowerCable;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            Assert.That(marker.PcieGpuPowerCableBuildKit.IsReleasedForAssembly,
                Is.True);

            Vector3 obstructionPoint = Vector3.Lerp(
                marker.PcieGpuPowerCableRoute.Waypoints[1].position,
                marker.PcieGpuPowerCableRoute.Waypoints[2].position,
                0.5f);
            GameObject obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Issue109ForeignPcieGpuRouteObstruction";
            obstruction.layer = 0;
            obstruction.transform.position = obstructionPoint;
            obstruction.transform.localScale = Vector3.one * 0.08f;
            Physics.SyncTransforms();

            MovePlayerToIssue109PcieGpuRoute(marker);
            AssertSuccess(marker.PlayerCarry.TrySetPcieGpuPowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentPcieGpuPowerCableRouteStatus,
                Is.EqualTo(PcieGpuPowerCableRouteStatus.RouteObstructed));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long cableRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            int cableReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;

            OperationResult blocked =
                marker.PlayerCarry.TryConfirmPcieGpuPowerCableRoute();

            Assert.That(blocked.Error,
                Is.EqualTo(Failure.FromCode(
                    "assembly-pcie-gpu-cable.route-obstructed")));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(cableRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(cableReceiptCount));

            Object.Destroy(obstruction);
            yield return null;
            Physics.SyncTransforms();
            AssertSuccess(marker.PlayerCarry.TrySetPcieGpuPowerCableRouteMode(false));
            MovePlayerToIssue109PcieGpuRoute(marker);
            AssertSuccess(marker.PlayerCarry.TrySetPcieGpuPowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentPcieGpuPowerCableRouteStatus,
                Is.EqualTo(PcieGpuPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue109PcieGpuRouteOverlaps(marker));
            Assert.That(marker.PcieGpuPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator PcieGpuRouteProjectionFailureRecoversSameInstanceAfterDomainCommit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue109RoutedAtx24AndEps12v(marker);

            PhysicalItemProjection cable = marker.PcieGpuPowerCable;
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            MovePlayerToIssue109PcieGpuRoute(marker);
            AssertSuccess(marker.PlayerCarry.TrySetPcieGpuPowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentPcieGpuPowerCableRouteStatus,
                Is.EqualTo(PcieGpuPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue109PcieGpuRouteOverlaps(marker));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            int cableReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            long protectedAtx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int protectedAtx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            long protectedEps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int protectedEps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            FailNextStablePlacement(cable);

            OperationResult route =
                marker.PlayerCarry.TryConfirmPcieGpuPowerCableRoute();

            AssertSuccess(route);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(PcieGpuPowerCableState.Routed));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(cableReceiptCount + 1));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(protectedAtx24Revision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(protectedAtx24ReceiptCount));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(protectedEps12vRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(protectedEps12vReceiptCount));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cable.IsStablePlacement, Is.True);
            OperationResult<Pose> routePose =
                marker.PcieGpuPowerCableRoute.ResolveRoutedItemPose();
            Assert.That(routePose.IsSuccess, Is.True);
            Assert.That(Vector3.Distance(
                    cable.transform.position,
                    routePose.Value.position),
                Is.LessThanOrEqualTo(0.0005f));
            Assert.That(Quaternion.Angle(
                    cable.transform.rotation,
                    routePose.Value.rotation),
                Is.LessThanOrEqualTo(0.05f));
            Assert.That(Object.FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsSortMode.None).Count(
                    item => item.ItemIdValue == stableItemId),
                Is.EqualTo(1));
            Assert.That(marker.PcieGpuPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunIssue109PcieGpuAssemblyHandoffCycle(bool useGamepad)
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = useGamepad ? null : InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = useGamepad ? InputSystem.AddDevice<Gamepad>() : null;
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.HasPcieGpuPowerCableAssemblyHandoffR54Runtime, Is.True);
            Assert.That(marker.HasElectricalReadinessWorkbenchR58Runtime, Is.True);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue109RoutedAtx24AndEps12v(marker);
            Assert.That(marker.ElectricalReadinessWorkbench.RefreshPresentation().Error,
                Is.EqualTo(ElectricalReadinessFailures.PcieGpuPowerCableMissing));
            Assert.That(marker.ElectricalReadinessWorkbench.IsReady, Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("PCIe GPU 6+2 KABLOSUNU BAĞLA"));

            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot pcieGpuLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            CustomPcBuildOrderLineSnapshot atx24Line = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularAtx24SplitPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot eps12vLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularEps12v8PinPsuToMotherboard);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            long protectedAtx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int protectedAtx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            StableId<AssemblyOperationIdScope> protectedAtx24RouteId =
                session.AssemblyBuild.Atx24PowerCableRoutedByOperationId;
            long protectedEps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int protectedEps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            StableId<AssemblyOperationIdScope> protectedEps12vRouteId =
                session.AssemblyBuild.Eps12vPowerCableRoutedByOperationId;

            PhysicalItemProjection cable = marker.PcieGpuPowerCable;
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            marker.PcieGpuPowerCableBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(
                marker.PlayerCarry.FocusedItem,
                Is.SameAs(cable),
                $"pcie={cable.InteractionCenter:F3} " +
                $"eps={marker.Eps12vPowerCable.InteractionCenter:F3} " +
                $"player={marker.PlayerMotor.transform.position:F3}");
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("PCIe GPU 6+2'Yİ KABLO MONTAJINA AL"));

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PcieGpuPowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PcieGpuPowerCableBuildKit.IsReleasedForAssembly,
                Is.True);
            Assert.That(marker.PcieGpuPowerCableBuildKit.ProgressText.text,
                Does.Contain("PCIe GPU MONTAJDA"));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt handoff), Is.True);
            Assert.That(handoff.Line, Is.SameAs(pcieGpuLine));
            Assert.That(handoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[9]));
            Assert.That(handoff.WorkbenchContainerId,
                Is.EqualTo(session.PcieGpuPowerCableRouteContainerId));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            long blockedInventoryRevision = session.Inventory.Revision;
            long blockedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long blockedAssemblyRevision = session.AssemblyBuild.Revision;
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(InventoryFailures
                    .SerializedReservationWorkOrderBuildKitConflict.Code));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(blockedInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(blockedBuildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(blockedAssemblyRevision));
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue109PcieGpuRoute(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsPcieGpuPowerCableRouteMode, Is.True);
            Assert.That(marker.PlayerCarry.IsPcieGpuPowerCableBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentPcieGpuPowerCableRouteStatus,
                Is.EqualTo(PcieGpuPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue109PcieGpuRouteOverlaps(marker));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PcieGpuPowerCableBinding.IsRouted, Is.True);
            Assert.That(marker.PcieGpuPowerCableGeometry.IsRouted, Is.True);
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(PcieGpuPowerCableState.Routed));
            Assert.That(session.TryGetPcieGpuPowerCableItem(
                out InventoryItemRecord routed), Is.True);
            Assert.That(routed.Id, Is.EqualTo(pcieGpuLine.ItemId));
            Assert.That(routed.ProductId, Is.EqualTo(pcieGpuLine.ProductId));
            Assert.That(routed.ContainerId,
                Is.EqualTo(session.PcieGpuPowerCableRouteContainerId));
            StableId<AssemblyOperationIdScope> routeOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.pcie-gpu-power-cable-route.r000001");
            Assert.That(session.AssemblyBuild.TryGetPcieGpuPowerCableReceipt(
                routeOperationId,
                out PcieGpuPowerCableOperationReceipt routeReceipt), Is.True);
            Assert.That(routeReceipt.SourceContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(routeReceipt.TargetContainerId,
                Is.EqualTo(session.PcieGpuPowerCableRouteContainerId));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision, Is.EqualTo(1));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(1));
            Assert.That(marker.ElectricalReadinessWorkbench.RefreshPresentation()
                .IsSuccess, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.IsReady, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("10/10 PARÇA • 3/3 KABLO")
                    .And.Contain("ELEKTRİK HAZIR")
                    .And.Contain("GÜÇ TESTİ BEKLİYOR"));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            AssertIssue109ProtectedCables(
                session,
                atx24Line,
                protectedAtx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                eps12vLine,
                protectedEps12vRouteId,
                protectedEps12vRevision,
                protectedEps12vReceiptCount);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(
                marker.PlayerCarry.FocusedItem,
                Is.SameAs(cable),
                $"pcie={cable.InteractionCenter:F3} " +
                $"eps={marker.Eps12vPowerCable.InteractionCenter:F3} " +
                $"player={marker.PlayerMotor.transform.position:F3}");
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PcieGpuPowerCableBinding.IsRouted, Is.False);
            Assert.That(marker.PcieGpuPowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(PcieGpuPowerCableState.Loose));
            Assert.That(session.TryGetPcieGpuPowerCableItem(
                out InventoryItemRecord unrouted), Is.True);
            Assert.That(unrouted.Id, Is.EqualTo(routed.Id));
            Assert.That(unrouted.ProductId, Is.EqualTo(routed.ProductId));
            Assert.That(unrouted.ContainerId,
                Is.EqualTo(session.HandsContainerId));
            StableId<AssemblyOperationIdScope> unrouteOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.pcie-gpu-power-cable-unroute.r000002");
            Assert.That(session.AssemblyBuild.TryGetPcieGpuPowerCableReceipt(
                unrouteOperationId,
                out PcieGpuPowerCableOperationReceipt unrouteReceipt), Is.True);
            Assert.That(unrouteReceipt.SourceRouteOperationId,
                Is.EqualTo(routeOperationId));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 3));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision, Is.EqualTo(2));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(2));
            Assert.That(marker.ElectricalReadinessWorkbench.RefreshPresentation().Error,
                Is.EqualTo(ElectricalReadinessFailures.PcieGpuPowerCableMissing));
            Assert.That(marker.ElectricalReadinessWorkbench.IsReady, Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("GÜÇ HAZIR DEĞİL"));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(10));
            AssertIssue89ReservationStillLive(session, workOrder, pcieGpuLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                new System.Collections.Generic.Dictionary<
                    StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>());
            AssertIssue109ProtectedCables(
                session,
                atx24Line,
                protectedAtx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                eps12vLine,
                protectedEps12vRouteId,
                protectedEps12vRevision,
                protectedEps12vReceiptCount);
            Assert.That(marker.PcieGpuPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            if (useGamepad)
            {
                Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
                Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
                Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
                Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));
            }
            else
            {
                Assert.That(mouse, Is.Not.Null);
            }

            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);
        }

        private static void PrepareIssue109RoutedAtx24AndEps12v(
            GaragePrototypeMarker marker)
        {
            PrepareIssue107RoutedAtx24(marker);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.Eps12vPowerCable));
            Assert.That(marker.Eps12vPowerCableBuildKit.IsReleasedForAssembly,
                Is.True);
            MovePlayerToIssue107Eps12vRoute(marker);
            AssertSuccess(marker.PlayerCarry.TrySetEps12vPowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmEps12vPowerCableRoute());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.IsAtx24PowerCableRouted, Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.True);
            Assert.That(session.AssemblyBuild.IsEps12vPowerCableRouted, Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.True);
        }

        private static void MovePlayerToIssue109PcieGpuRoute(
            GaragePrototypeMarker marker)
        {
            Vector3 target =
                marker.PcieGpuPowerCableRoute.FocusCollider.bounds.center;
            SetPlayerLook(
                marker,
                new Vector3(-0.72f, 0.05f, 3.25f),
                target);
        }

        private static void AssertIssue109ProtectedCables(
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
            Assert.That(GetIssue89Item(session, atx24.ItemId).ContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(Atx24PowerCableState.Routed));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRoutedByOperationId,
                Is.EqualTo(atx24RouteId));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(atx24ReceiptCount));
            Assert.That(GetIssue89Item(session, eps12v.ItemId).ContainerId,
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

        private static string DescribeIssue109PcieGpuRouteOverlaps(
            GaragePrototypeMarker marker)
        {
            PcieGpuPowerCableRouteProjection route =
                marker.PcieGpuPowerCableRoute;
            Vector3[] points =
            {
                route.PsuEndpoint.position,
                route.Waypoints[0].position,
                route.Waypoints[1].position,
                route.Waypoints[2].position,
                route.GraphicsCardEndpoint.position
            };
            var descriptions = new System.Collections.Generic.List<string>();
            for (int index = 0; index < points.Length - 1; index++)
            {
                foreach (Collider collider in Physics.OverlapCapsule(
                             points[index],
                             points[index + 1],
                             0.0075f,
                             ~0,
                             QueryTriggerInteraction.Ignore))
                {
                    descriptions.Add(
                        $"s{index}:{collider.name}" +
                        $"@{collider.transform.position:F3}" +
                        $"/root={collider.transform.root.name}");
                }
            }

            return string.Join(",", descriptions);
        }
    }
}
