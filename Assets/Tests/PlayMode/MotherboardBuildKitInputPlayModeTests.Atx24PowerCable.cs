using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class MotherboardBuildKitInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator Atx24PickupRequiresAllSevenStagedPrerequisitesWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSixForPowerSupplyBuildKit(marker);

            PhysicalItemProjection cable =
                marker.Atx24PowerCableBinding.PhysicalItem;
            Atx24PowerCableBuildKitProjection buildKit =
                marker.Atx24PowerCableBuildKit;
            Assert.That(session.TryGetAtx24PowerCableItem(
                out InventoryItemRecord itemBefore), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int physicalIdentity = cable.GetInstanceID();
            Transform worldParent = cable.transform.parent;
            Vector3 worldPosition = cable.transform.position;
            Quaternion worldRotation = cable.transform.rotation;

            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.HasMemoryModulePrerequisite, Is.True);
            Assert.That(buildKit.HasStoragePrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorCoolerPrerequisite, Is.True);
            Assert.That(buildKit.HasGraphicsCardPrerequisite, Is.True);
            Assert.That(buildKit.HasPowerSupplyPrerequisite, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing.Code));
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityLooseWorld,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInHands,
                Is.False);
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInBuildKit,
                Is.False);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.False);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.TryGetAtx24PowerCableItem(
                out InventoryItemRecord itemAfter), Is.True);
            Assert.That(itemAfter.Id, Is.EqualTo(itemBefore.Id));
            Assert.That(itemAfter.ProductId, Is.EqualTo(itemBefore.ProductId));
            Assert.That(itemAfter.ContainerId, Is.EqualTo(itemBefore.ContainerId));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.transform.parent, Is.SameAs(worldParent));
            Assert.That(cable.transform.position, Is.EqualTo(worldPosition));
            Assert.That(cable.transform.rotation, Is.EqualTo(worldRotation));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseMovesExactReservedAtx24CableFromSevenToEight()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSevenForAtx24BuildKit(marker);

            Assert.That(marker.HasAtx24PowerCableBuildKitR42Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot cableLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularAtx24SplitPsuToMotherboard);
            Assert.That(cableLine.ItemId, Is.EqualTo(session.Atx24PowerCableItemId));
            Assert.That(session.Inventory.TryGetReservation(
                cableLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(cableLine.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));

            PhysicalItemProjection cable =
                marker.Atx24PowerCableBinding.PhysicalItem;
            Atx24PowerCableBuildKitProjection buildKit =
                marker.Atx24PowerCableBuildKit;
            int physicalIdentity = cable.GetInstanceID();
            string itemIdentity = cable.ItemIdValue;
            int worldLayer = cable.gameObject.layer;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Atx24PowerCableState routeState =
                session.AssemblyBuild.Atx24PowerCableState;
            long routeRevision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int routeReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup =
                session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasPowerSupplyPrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(7));
            Assert.That(buildKit.ProgressText.text, Does.Contain("7/10"));

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeAtx24PowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.Atx24PowerCableInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(cableLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            ReleaseKeyboard(marker, keyboard);

            long inventoryRevisionInHands = session.Inventory.Revision;
            long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionInHands));

            MovePlayerToAtx24BuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsAtx24PowerCableBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsAtx24PowerCableRouteMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("7/10 → 8/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(1).rotation),
                Is.LessThanOrEqualTo(0.25f));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(8));
            Assert.That(buildKit.ProgressText.text, Does.Contain("8/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("ATX24 HAZIR"));
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(cable.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cable.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(cable), Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeAtx24PowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.Atx24PowerCableStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(cableLine));
            Assert.That(session.TryGetAtx24PowerCableItem(
                out InventoryItemRecord stagedCable), Is.True);
            Assert.That(stagedCable.Id, Is.EqualTo(cableLine.ItemId));
            Assert.That(stagedCable.ProductId, Is.EqualTo(cableLine.ProductId));
            Assert.That(stagedCable.ContainerId,
                Is.EqualTo(session.Atx24PowerCableBuildKitContainerId));
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
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(routeState));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(routeRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(routeReceiptCount));
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator Atx24BuildKitRejectsObstructionAndMissingSupportWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSevenForAtx24BuildKit(marker);

            PhysicalItemProjection cable =
                marker.Atx24PowerCableBinding.PhysicalItem;
            Atx24PowerCableBuildKitProjection buildKit =
                marker.Atx24PowerCableBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            MovePlayerToAtx24BuildKit(marker, buildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetAtx24PowerCableBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            int physicalIdentity = cable.GetInstanceID();
            GameObject obstruction = CreateAtx24BuildKitObstruction(buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.Obstructed),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.TryConfirmAtx24PowerCableBuildKit()
                .IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Object.DestroyImmediate(obstruction);
            Physics.SyncTransforms();

            Vector3 authoredSnapPosition = buildKit.SnapAnchor.position;
            buildKit.SnapAnchor.position += Vector3.right * 0.35f;
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.OutsideSurface),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.TryConfirmAtx24PowerCableBuildKit()
                .IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            buildKit.SnapAnchor.position = authoredSnapPosition;
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadAtx24CoEdgesPauseAndReleaseRepressAreDeterministic()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSevenForAtx24BuildKit(marker);

            PhysicalItemProjection cable =
                marker.Atx24PowerCableBinding.PhysicalItem;
            Atx24PowerCableBuildKitProjection buildKit =
                marker.Atx24PowerCableBuildKit;
            int physicalIdentity = cable.GetInstanceID();
            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToAtx24BuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons =
                        (1u << (int)GamepadButton.RightShoulder) |
                        (1u << (int)GamepadButton.East)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsAtx24PowerCableBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsAtx24PowerCableRouteMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(7));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons =
                        (1u << (int)GamepadButton.Start) |
                        (1u << (int)GamepadButton.RightShoulder) |
                        (1u << (int)GamepadButton.East)
                });
            yield return null;
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(7));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToAtx24BuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.RightShoulder
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.East
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(8));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator Atx24PlacementFailureRecoversSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSevenForAtx24BuildKit(marker);

            PhysicalItemProjection cable =
                marker.Atx24PowerCableBinding.PhysicalItem;
            Atx24PowerCableBuildKitProjection buildKit =
                marker.Atx24PowerCableBuildKit;
            int physicalIdentity = cable.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));

            long inventoryInHands = session.Inventory.Revision;
            long buildKitInHands = session.CustomPcBuildKit.Revision;
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands));

            MovePlayerToAtx24BuildKit(marker, buildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetAtx24PowerCableBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);

            FailNextStablePlacement(cable);
            var placement =
                marker.PlayerCarry.TryConfirmAtx24PowerCableBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(8));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cable.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                cable.transform.position,
                expectedPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                cable.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.05f));
            Assert.That(buildKit.MatchesCommittedPlacement(cable), Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryInHands + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands + 1));
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageFirstSevenForAtx24BuildKit(
            GaragePrototypeMarker marker)
        {
            StageFirstSixForPowerSupplyBuildKit(marker);
            PowerSupplyBuildKitProjection powerSupplyBuildKit =
                marker.PowerSupplyBuildKit;
            AssertSuccess(
                marker.PlayerCarry.TryPickup(
                    marker.PowerSupplyBinding.PhysicalItem));
            MovePlayerToBuildKit(marker, powerSupplyBuildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetPowerSupplyBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBuildKitStatus,
                Is.EqualTo(PowerSupplyBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(
                marker.PlayerCarry.TryConfirmPowerSupplyBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(powerSupplyBuildKit.IsStaged, Is.True);
            Assert.That(powerSupplyBuildKit.StagedComponentCount,
                Is.EqualTo(7));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.PowerSupplyBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
        }

        private static void MovePlayerToAtx24BuildKit(
            GaragePrototypeMarker marker,
            Atx24PowerCableBuildKitProjection buildKit)
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

        private static GameObject CreateAtx24BuildKitObstruction(
            Atx24PowerCableBuildKitProjection buildKit)
        {
            Pose pose = buildKit.ResolveSnapPose(0);
            Collider support = buildKit.SupportCollider;
            GameObject obstruction =
                GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Atx24BuildKitFootprintObstruction";
            obstruction.layer = 0;
            obstruction.transform.position = new Vector3(
                pose.position.x + 0.055f,
                support.bounds.max.y + 0.041f,
                pose.position.z);
            obstruction.transform.localScale =
                new Vector3(0.02f, 0.05f, 0.02f);
            Physics.SyncTransforms();
            return obstruction;
        }
    }
}
