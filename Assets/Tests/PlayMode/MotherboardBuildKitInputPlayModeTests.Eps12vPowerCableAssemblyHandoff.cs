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
        public IEnumerator KeyboardMouseCompletesEps12vBuildKitRouteUnrouteCycle()
        {
            yield return RunIssue107Eps12vAssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesEps12vBuildKitRouteUnrouteCycle()
        {
            yield return RunIssue107Eps12vAssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator Eps12vForeignRouteObstructionFailsClosedThenClears()
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
            PrepareIssue107RoutedAtx24(marker);

            PhysicalItemProjection cable = marker.Eps12vPowerCable;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            Assert.That(marker.Eps12vPowerCableBuildKit.IsReleasedForAssembly,
                Is.True);

            Vector3 obstructionPoint = Vector3.Lerp(
                marker.Eps12vPowerCableRoute.Waypoints[1].position,
                marker.Eps12vPowerCableRoute.Waypoints[2].position,
                0.5f);
            GameObject obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Issue107ForeignEps12vRouteObstruction";
            obstruction.layer = 0;
            obstruction.transform.position = obstructionPoint;
            obstruction.transform.localScale = Vector3.one * 0.08f;
            Physics.SyncTransforms();

            MovePlayerToIssue107Eps12vRoute(marker);
            AssertSuccess(marker.PlayerCarry.TrySetEps12vPowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.RouteObstructed));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long cableRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            int cableReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;

            OperationResult blocked =
                marker.PlayerCarry.TryConfirmEps12vPowerCableRoute();

            Assert.That(blocked.Error,
                Is.EqualTo(Failure.FromCode(
                    "assembly-eps12v-cable.route-obstructed")));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(cableRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(cableReceiptCount));

            Object.Destroy(obstruction);
            yield return null;
            Physics.SyncTransforms();
            AssertSuccess(marker.PlayerCarry.TrySetEps12vPowerCableRouteMode(false));
            MovePlayerToIssue107Eps12vRoute(marker);
            AssertSuccess(marker.PlayerCarry.TrySetEps12vPowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue107EpsRouteOverlaps(marker));
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator Eps12vRouteProjectionFailureRecoversSameInstanceAfterDomainCommit()
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
            PrepareIssue107RoutedAtx24(marker);

            PhysicalItemProjection cable = marker.Eps12vPowerCable;
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            MovePlayerToIssue107Eps12vRoute(marker);
            AssertSuccess(marker.PlayerCarry.TrySetEps12vPowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue107EpsRouteOverlaps(marker));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            int cableReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            long protectedAtx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int protectedAtx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            FailNextStablePlacement(cable);

            OperationResult route =
                marker.PlayerCarry.TryConfirmEps12vPowerCableRoute();

            AssertSuccess(route);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Routed));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(cableReceiptCount + 1));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(protectedAtx24Revision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(protectedAtx24ReceiptCount));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cable.IsStablePlacement, Is.True);
            OperationResult<Pose> routePose =
                marker.Eps12vPowerCableRoute.ResolveRoutedItemPose();
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
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunIssue107Eps12vAssemblyHandoffCycle(bool useGamepad)
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = useGamepad ? null : InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = useGamepad ? InputSystem.AddDevice<Gamepad>() : null;
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.HasEps12vPowerCableAssemblyHandoffR53Runtime, Is.True);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue107RoutedAtx24(marker);

            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot eps12vLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularEps12v8PinPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot atx24Line = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularAtx24SplitPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot pcieLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            StableId<ContainerIdScope> pcieContainer =
                GetIssue89Item(session, pcieLine.ItemId).ContainerId;
            long protectedAtx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int protectedAtx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            StableId<AssemblyOperationIdScope> protectedAtx24RouteId =
                session.AssemblyBuild.Atx24PowerCableRoutedByOperationId;

            PhysicalItemProjection cable = marker.Eps12vPowerCable;
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            marker.Eps12vPowerCableBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("EPS12V'Yİ KABLO MONTAJINA AL"));

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.Eps12vPowerCableBuildKit.IsReleasedForAssembly,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBuildKit.ProgressText.text,
                Does.Contain("EPS12V MONTAJDA"));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt handoff), Is.True);
            Assert.That(handoff.Line, Is.SameAs(eps12vLine));
            Assert.That(handoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[8]));
            Assert.That(handoff.WorkbenchContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
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

            MovePlayerToIssue107Eps12vRoute(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.True);
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue107EpsRouteOverlaps(marker));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.True);
            Assert.That(marker.Eps12vPowerCableGeometry.IsRouted, Is.True);
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Routed));
            Assert.That(session.TryGetEps12vPowerCableItem(
                out InventoryItemRecord routed), Is.True);
            Assert.That(routed.Id, Is.EqualTo(eps12vLine.ItemId));
            Assert.That(routed.ProductId, Is.EqualTo(eps12vLine.ProductId));
            Assert.That(routed.ContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
            StableId<AssemblyOperationIdScope> routeOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.eps12v-power-cable-route.r000001");
            Assert.That(session.AssemblyBuild.TryGetEps12vPowerCableReceipt(
                routeOperationId,
                out Eps12vPowerCableOperationReceipt routeReceipt), Is.True);
            Assert.That(routeReceipt.SourceContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(routeReceipt.TargetContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision, Is.EqualTo(1));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(1));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            AssertIssue107ProtectedCables(
                session,
                atx24Line,
                protectedAtx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                pcieLine,
                pcieContainer);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue107Eps12vRoute(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Loose));
            Assert.That(session.TryGetEps12vPowerCableItem(
                out InventoryItemRecord unrouted), Is.True);
            Assert.That(unrouted.Id, Is.EqualTo(routed.Id));
            Assert.That(unrouted.ProductId, Is.EqualTo(routed.ProductId));
            Assert.That(unrouted.ContainerId,
                Is.EqualTo(session.HandsContainerId));
            StableId<AssemblyOperationIdScope> unrouteOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.eps12v-power-cable-unroute.r000002");
            Assert.That(session.AssemblyBuild.TryGetEps12vPowerCableReceipt(
                unrouteOperationId,
                out Eps12vPowerCableOperationReceipt unrouteReceipt), Is.True);
            Assert.That(unrouteReceipt.SourceRouteOperationId,
                Is.EqualTo(routeOperationId));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 3));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision, Is.EqualTo(2));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(2));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(9));
            AssertIssue89ReservationStillLive(session, workOrder, eps12vLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                new System.Collections.Generic.Dictionary<
                    StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                {
                    [pcieLine.ItemId] = pcieContainer
                });
            AssertIssue107ProtectedCables(
                session,
                atx24Line,
                protectedAtx24RouteId,
                protectedAtx24Revision,
                protectedAtx24ReceiptCount,
                pcieLine,
                pcieContainer);
            Assert.That(marker.Eps12vPowerCableBinding
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

        private static void PrepareIssue107RoutedAtx24(GaragePrototypeMarker marker)
        {
            PrepareIssue105RetainedPowerSupply(marker);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.Atx24PowerCable));
            Assert.That(marker.Atx24PowerCableBuildKit.IsReleasedForAssembly,
                Is.True);
            MovePlayerToIssue105Atx24Route(marker);
            AssertSuccess(marker.PlayerCarry.TrySetAtx24PowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableRouteStatus,
                Is.EqualTo(Atx24PowerCableRouteStatus.ValidRoute),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmAtx24PowerCableRoute());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.IsAtx24PowerCableRouted, Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.True);
        }

        private static void MovePlayerToIssue107Eps12vRoute(
            GaragePrototypeMarker marker)
        {
            Vector3 target =
                marker.Eps12vPowerCableRoute.FocusCollider.bounds.center;
            SetPlayerLook(
                marker,
                new Vector3(-0.72f, 0.05f, 3.25f),
                target);
        }

        private static void AssertIssue107ProtectedCables(
            GarageStockFlowSession session,
            CustomPcBuildOrderLineSnapshot atx24,
            StableId<AssemblyOperationIdScope> atx24RouteId,
            long atx24Revision,
            int atx24ReceiptCount,
            CustomPcBuildOrderLineSnapshot pcie,
            StableId<ContainerIdScope> pcieContainer)
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
            Assert.That(GetIssue89Item(session, pcie.ItemId).ContainerId,
                Is.EqualTo(pcieContainer));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(PcieGpuPowerCableState.Loose));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision, Is.Zero);
        }

        private static string DescribeIssue107EpsRouteOverlaps(
            GaragePrototypeMarker marker)
        {
            Eps12vPowerCableRouteProjection route =
                marker.Eps12vPowerCableRoute;
            Vector3[] points =
            {
                route.PsuEndpoint.position,
                route.Waypoints[0].position,
                route.Waypoints[1].position,
                route.Waypoints[2].position,
                route.MotherboardEndpoint.position
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
