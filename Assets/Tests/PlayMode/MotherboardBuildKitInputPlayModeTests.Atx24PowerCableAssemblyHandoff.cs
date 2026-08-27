using System.Collections;
using System.Collections.Generic;
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
        public IEnumerator KeyboardMouseCompletesAtx24BuildKitRouteUnrouteCycle()
        {
            yield return RunIssue105Atx24AssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesAtx24BuildKitRouteUnrouteCycle()
        {
            yield return RunIssue105Atx24AssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator Atx24ForeignRouteObstructionFailsClosedThenClears()
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
            PrepareIssue105RetainedPowerSupply(marker);

            PhysicalItemProjection cable = marker.Atx24PowerCable;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            Assert.That(marker.Atx24PowerCableBuildKit.IsReleasedForAssembly,
                Is.True);

            Vector3 obstructionPoint = Vector3.Lerp(
                marker.Atx24PowerCableRoute.Waypoints[1].position,
                marker.Atx24PowerCableRoute.Waypoints[2].position,
                0.5f);
            GameObject obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Issue105ForeignAtx24RouteObstruction";
            obstruction.layer = 0;
            obstruction.transform.position = obstructionPoint;
            obstruction.transform.localScale = Vector3.one * 0.08f;
            Physics.SyncTransforms();

            MovePlayerToIssue105Atx24Route(marker);
            AssertSuccess(marker.PlayerCarry.TrySetAtx24PowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableRouteStatus,
                Is.EqualTo(Atx24PowerCableRouteStatus.RouteObstructed));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long cableRevision = session.AssemblyBuild.Atx24PowerCableRevision;
            int cableReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;

            OperationResult blocked =
                marker.PlayerCarry.TryConfirmAtx24PowerCableRoute();

            Assert.That(blocked.Error,
                Is.EqualTo(Failure.FromCode("assembly-power-cable.route-obstructed")));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(cableRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(cableReceiptCount));

            Object.Destroy(obstruction);
            yield return null;
            Physics.SyncTransforms();
            AssertSuccess(marker.PlayerCarry.TrySetAtx24PowerCableRouteMode(false));
            MovePlayerToIssue105Atx24Route(marker);
            AssertSuccess(marker.PlayerCarry.TrySetAtx24PowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableRouteStatus,
                Is.EqualTo(Atx24PowerCableRouteStatus.ValidRoute),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator Atx24RouteProjectionFailureRecoversSameInstanceAfterDomainCommit()
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
            PrepareIssue105RetainedPowerSupply(marker);

            PhysicalItemProjection cable = marker.Atx24PowerCable;
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            MovePlayerToIssue105Atx24Route(marker);
            AssertSuccess(marker.PlayerCarry.TrySetAtx24PowerCableRouteMode(true));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableRouteStatus,
                Is.EqualTo(Atx24PowerCableRouteStatus.ValidRoute),
                marker.PlayerCarry.LastFailureCode);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            int cableReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            FailNextStablePlacement(cable);

            OperationResult route =
                marker.PlayerCarry.TryConfirmAtx24PowerCableRoute();

            AssertSuccess(route);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(Atx24PowerCableState.Routed));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(cableReceiptCount + 1));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cable.IsStablePlacement, Is.True);
            OperationResult<Pose> routePose =
                marker.Atx24PowerCableRoute.ResolveRoutedItemPose();
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
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunIssue105Atx24AssemblyHandoffCycle(bool useGamepad)
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = useGamepad ? null : InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = useGamepad ? InputSystem.AddDevice<Gamepad>() : null;
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.HasAtx24PowerCableAssemblyHandoffR52Runtime, Is.True);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue105RetainedPowerSupply(marker);

            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot atx24Line = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularAtx24SplitPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot eps12vLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularEps12v8PinPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot pcieLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            var untouchedCableContainers = new Dictionary<
                StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
            {
                [eps12vLine.ItemId] =
                    GetIssue89Item(session, eps12vLine.ItemId).ContainerId,
                [pcieLine.ItemId] =
                    GetIssue89Item(session, pcieLine.ItemId).ContainerId
            };
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            Eps12vPowerCableState eps12vState =
                session.AssemblyBuild.Eps12vPowerCableState;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;

            PhysicalItemProjection cable = marker.Atx24PowerCable;
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            marker.Atx24PowerCableBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("ATX24'Ü KABLO MONTAJINA AL"));

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.Atx24PowerCableBuildKit.IsReleasedForAssembly,
                Is.True);
            Assert.That(marker.Atx24PowerCableBuildKit.ProgressText.text,
                Does.Contain("ATX24 MONTAJDA"));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt handoff), Is.True);
            Assert.That(handoff.Line, Is.SameAs(atx24Line));
            Assert.That(handoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[7]));
            Assert.That(handoff.WorkbenchContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
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

            MovePlayerToIssue105Atx24Route(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsAtx24PowerCableRouteMode, Is.True);
            Assert.That(marker.PlayerCarry.IsAtx24PowerCableBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableRouteStatus,
                Is.EqualTo(Atx24PowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue105Atx24RouteOverlaps(marker));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.True);
            Assert.That(marker.Atx24PowerCableGeometry.IsRouted, Is.True);
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(Atx24PowerCableState.Routed));
            Assert.That(session.TryGetAtx24PowerCableItem(
                out InventoryItemRecord routed), Is.True);
            Assert.That(routed.Id, Is.EqualTo(atx24Line.ItemId));
            Assert.That(routed.ProductId, Is.EqualTo(atx24Line.ProductId));
            Assert.That(routed.ContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
            StableId<AssemblyOperationIdScope> routeOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.atx24-power-cable-route.r000001");
            Assert.That(session.AssemblyBuild.TryGetAtx24PowerCableReceipt(
                routeOperationId,
                out Atx24PowerCableOperationReceipt routeReceipt), Is.True);
            Assert.That(routeReceipt.SourceContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(routeReceipt.TargetContainerId,
                Is.EqualTo(session.Atx24PowerCableRouteContainerId));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision, Is.EqualTo(1));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(1));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            AssemblyBuildSnapshot routedSnapshot =
                session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> blockedPowerSupplyUnretain =
                session.UnretainPowerSupply(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue105.playmode-blocked-psu-unretain"),
                    routedSnapshot.PowerSupplySeatedByOperationId,
                    routedSnapshot.PowerSupplyRetainedByOperationId,
                    routedSnapshot.Revision);
            Assert.That(blockedPowerSupplyUnretain.Error,
                Is.EqualTo(AssemblyFailures.PowerCableDependentComponentLocked));

            MovePlayerToIssue105Atx24Route(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(Atx24PowerCableState.Loose));
            Assert.That(session.TryGetAtx24PowerCableItem(
                out InventoryItemRecord unrouted), Is.True);
            Assert.That(unrouted.Id, Is.EqualTo(routed.Id));
            Assert.That(unrouted.ProductId, Is.EqualTo(routed.ProductId));
            Assert.That(unrouted.ContainerId,
                Is.EqualTo(session.HandsContainerId));
            StableId<AssemblyOperationIdScope> unrouteOperationId =
                StableId<AssemblyOperationIdScope>.Parse(
                    "assembly.operation.prototype-001.atx24-power-cable-unroute.r000002");
            Assert.That(session.AssemblyBuild.TryGetAtx24PowerCableReceipt(
                unrouteOperationId,
                out Atx24PowerCableOperationReceipt unrouteReceipt), Is.True);
            Assert.That(unrouteReceipt.SourceRouteOperationId,
                Is.EqualTo(routeOperationId));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 3));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision, Is.EqualTo(2));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(2));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.PowerCableMissing));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(8));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(eps12vState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(pcieState));
            AssertIssue89ReservationStillLive(session, workOrder, atx24Line);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                untouchedCableContainers);
            Assert.That(marker.Atx24PowerCableBinding
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

        private static void PrepareIssue105RetainedPowerSupply(
            GaragePrototypeMarker marker)
        {
            PrepareIssue102RetainedGraphicsCard(marker);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.PowerSupply));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            MovePlayerToIssue102PowerSupplyBay(marker);
            AssertSuccess(marker.PlayerCarry.TrySetPowerSupplySeatMode(true));
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmPowerSupplySeat());
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            MovePlayerToIssue102PowerSupplyBay(marker);
            AssertSuccess(marker.PlayerCarry.TryOperatePowerSupplyRetention());
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            Assert.That(marker.PowerSupplyBinding.IsRetained, Is.True);
        }

        private static void MovePlayerToIssue105Atx24Route(
            GaragePrototypeMarker marker)
        {
            Vector3 target =
                marker.Atx24PowerCableRoute.FocusCollider.bounds.center;
            SetPlayerLook(
                marker,
                new Vector3(-0.72f, 0.05f, 3.25f),
                target);
        }

        private static string DescribeIssue105Atx24RouteOverlaps(
            GaragePrototypeMarker marker)
        {
            Atx24PowerCableRouteProjection route =
                marker.Atx24PowerCableRoute;
            Vector3[] points =
            {
                route.PsuPrimaryEndpoint.position,
                route.Waypoints[0].position,
                route.PsuSenseEndpoint.position,
                route.Waypoints[0].position,
                route.Waypoints[1].position,
                route.Waypoints[2].position,
                route.MotherboardEndpoint.position
            };
            int[] pairs = { 0, 1, 2, 3, 1, 4, 4, 5, 5, 6 };
            var descriptions = new List<string>();
            for (int index = 0; index < points.Length; index++)
            {
                descriptions.Add($"p{index}={points[index]:F3}");
            }

            for (int pair = 0; pair < pairs.Length; pair += 2)
            {
                foreach (Collider collider in Physics.OverlapCapsule(
                             points[pairs[pair]],
                             points[pairs[pair + 1]],
                             0.0075f,
                             ~0,
                             QueryTriggerInteraction.Ignore))
                {
                    descriptions.Add(
                        $"s{pair / 2}:{collider.name}@" +
                        $"{collider.transform.position:F3}/" +
                        $"{LayerMask.LayerToName(collider.gameObject.layer)}/" +
                        $"trigger={collider.isTrigger}/" +
                        $"route={collider.transform.IsChildOf(route.RouteRoot)}/" +
                        $"psu={collider.transform.IsChildOf(route.PowerSupplyHostRoot)}/" +
                        $"mb={collider.transform.IsChildOf(route.MotherboardHostRoot)}/" +
                        $"cooler={collider.transform.IsChildOf(marker.ProcessorCooler.transform)}/" +
                        $"gpu={collider.transform.IsChildOf(marker.GraphicsCard.transform)}");
                }
            }

            return string.Join(",", descriptions);
        }
    }
}
