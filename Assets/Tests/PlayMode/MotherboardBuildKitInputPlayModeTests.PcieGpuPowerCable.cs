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
        public IEnumerator KeyboardMouseMovesExactReservedPcieCableFromNineToTen()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstNineForPcieGpuPowerCableBuildKit(marker);

            Assert.That(marker.HasPcieGpuPowerCableBuildKitR44Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot cableLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            Assert.That(cableLine.ItemId,
                Is.EqualTo(session.PcieGpuPowerCableItemId));
            Assert.That(session.Inventory.TryGetReservation(
                cableLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(cableLine.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));

            PhysicalItemProjection cable =
                marker.PcieGpuPowerCableBinding.PhysicalItem;
            PcieGpuPowerCableBuildKitProjection buildKit =
                marker.PcieGpuPowerCableBuildKit;
            int physicalIdentity = cable.GetInstanceID();
            string itemIdentity = cable.ItemIdValue;
            int worldLayer = cable.gameObject.layer;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            PcieGpuPowerCableState routeState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long routeRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int routeReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup =
                session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasAtx24PowerCablePrerequisite, Is.True);
            Assert.That(buildKit.HasEps12vPowerCablePrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(9));
            Assert.That(buildKit.ProgressText.text, Does.Contain("9/10"));

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PcieGpuPowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(marker.PcieGpuPowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypePcieGpuPowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.PcieGpuPowerCableInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(cableLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            ReleaseKeyboard(marker, keyboard);

            long inventoryRevisionInHands = session.Inventory.Revision;
            long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
            OperationResult routeBypass =
                marker.PlayerCarry.TrySetPcieGpuPowerCableRouteMode(true);
            Assert.That(routeBypass.IsFailure, Is.True);
            Assert.That(routeBypass.Error.Code,
                Is.EqualTo(
                    "custom-pc-pcie-gpu-power-cable-build-kit.authority-blocked"));
            Assert.That(marker.PlayerCarry.IsPcieGpuPowerCableRouteMode, Is.False);
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PcieGpuPowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionInHands));

            MovePlayerToPcieGpuPowerCableBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsPcieGpuPowerCableBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsPcieGpuPowerCableRouteMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentPcieGpuPowerCableBuildKitStatus,
                Is.EqualTo(PcieGpuPowerCableBuildKitStatus.Valid),
                DescribePcieBuildKitOverlaps(buildKit, cable));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("9/10 → 10/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentPcieGpuPowerCableBuildKitStatus,
                Is.EqualTo(PcieGpuPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(1).rotation),
                Is.LessThanOrEqualTo(0.25f));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(buildKit.ProgressText.text, Does.Contain("10/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("PCIe GPU HAZIR"));
            Assert.That(marker.PcieGpuPowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.PcieGpuPowerCableBinding.IsRouted, Is.False);
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(cable.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cable.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(cable), Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypePcieGpuPowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.PcieGpuPowerCableStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(cableLine));
            Assert.That(session.TryGetPcieGpuPowerCableItem(
                out InventoryItemRecord stagedCable), Is.True);
            Assert.That(stagedCable.Id, Is.EqualTo(cableLine.ItemId));
            Assert.That(stagedCable.ProductId, Is.EqualTo(cableLine.ProductId));
            Assert.That(stagedCable.ContainerId,
                Is.EqualTo(session.PcieGpuPowerCableBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                cableLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId, Is.EqualTo(stagedCable.Id));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 2));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(routeState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(routeRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(routeReceiptCount));
            Assert.That(marker.PcieGpuPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        private static void StageFirstNineForPcieGpuPowerCableBuildKit(
            GaragePrototypeMarker marker)
        {
            StageFirstEightForEps12vBuildKit(marker);
            Eps12vPowerCableBuildKitProjection eps12vBuildKit =
                marker.Eps12vPowerCableBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(
                marker.Eps12vPowerCableBinding.PhysicalItem));
            MovePlayerToEps12vBuildKit(marker, eps12vBuildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetEps12vPowerCableBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(
                marker.PlayerCarry.TryConfirmEps12vPowerCableBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(eps12vBuildKit.IsStaged, Is.True);
            Assert.That(eps12vBuildKit.StagedComponentCount, Is.EqualTo(9));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
        }

        private static void MovePlayerToPcieGpuPowerCableBuildKit(
            GaragePrototypeMarker marker,
            PcieGpuPowerCableBuildKitProjection buildKit)
        {
            Collider support = buildKit.SupportCollider;
            Vector3 target = new Vector3(
                support.bounds.center.x,
                support.bounds.max.y,
                support.bounds.center.z);
            Vector3 playerPosition = target + (Vector3.back * 0.95f);
            playerPosition.y = 0.05f;
            SetPlayerLook(marker, playerPosition, target);
        }

        private static string DescribePcieBuildKitOverlaps(
            PcieGpuPowerCableBuildKitProjection buildKit,
            PhysicalItemProjection cable)
        {
            Pose candidate = buildKit.ResolveSnapPose(0);
            Collider support = buildKit.SupportCollider;
            const float halfHeight = 0.041f;
            Vector3 center = new Vector3(
                candidate.position.x,
                support.bounds.max.y + halfHeight,
                candidate.position.z);
            Quaternion rotation = Quaternion.Euler(
                0f,
                candidate.rotation.eulerAngles.y,
                0f);
            Collider[] overlaps = Physics.OverlapBox(
                center,
                new Vector3(0.073f, halfHeight, 0.068f),
                rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            return "PCIe Build Kit overlaps: " + string.Join(
                ", ",
                overlaps.Select(overlap =>
                    $"{overlap.name}[layer={overlap.gameObject.layer}," +
                    $"held={overlap.transform.IsChildOf(cable.transform)}]"));
        }
    }
}
