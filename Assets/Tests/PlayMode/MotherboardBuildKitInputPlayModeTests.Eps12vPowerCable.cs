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
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class MotherboardBuildKitInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator Eps12vPickupRequiresAllEightStagedPrerequisitesWithoutMutation()
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
                marker.Eps12vPowerCableBinding.PhysicalItem;
            Eps12vPowerCableBuildKitProjection buildKit =
                marker.Eps12vPowerCableBuildKit;
            Assert.That(session.TryGetEps12vPowerCableItem(
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
            Assert.That(buildKit.HasPowerSupplyPrerequisite, Is.True);
            Assert.That(buildKit.HasAtx24PowerCablePrerequisite, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(7));

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing.Code));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityLooseWorld,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands,
                Is.False);
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInBuildKit,
                Is.False);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.False);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.TryGetEps12vPowerCableItem(
                out InventoryItemRecord itemAfter), Is.True);
            Assert.That(itemAfter.Id, Is.EqualTo(itemBefore.Id));
            Assert.That(itemAfter.ProductId, Is.EqualTo(itemBefore.ProductId));
            Assert.That(itemAfter.ContainerId, Is.EqualTo(itemBefore.ContainerId));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.transform.parent, Is.SameAs(worldParent));
            Assert.That(cable.transform.position, Is.EqualTo(worldPosition));
            Assert.That(cable.transform.rotation, Is.EqualTo(worldRotation));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseMovesExactReservedEps12vCableFromEightToNine()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstEightForEps12vBuildKit(marker);

            Assert.That(marker.HasEps12vPowerCableBuildKitR43Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot cableLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularEps12v8PinPsuToMotherboard);
            Assert.That(cableLine.ItemId, Is.EqualTo(session.Eps12vPowerCableItemId));
            Assert.That(session.Inventory.TryGetReservation(
                cableLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(cableLine.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));

            PhysicalItemProjection cable =
                marker.Eps12vPowerCableBinding.PhysicalItem;
            Eps12vPowerCableBuildKitProjection buildKit =
                marker.Eps12vPowerCableBuildKit;
            int physicalIdentity = cable.GetInstanceID();
            string itemIdentity = cable.ItemIdValue;
            int worldLayer = cable.gameObject.layer;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Eps12vPowerCableState routeState =
                session.AssemblyBuild.Eps12vPowerCableState;
            long routeRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int routeReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup =
                session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasPowerSupplyPrerequisite, Is.True);
            Assert.That(buildKit.HasAtx24PowerCablePrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(8));
            Assert.That(buildKit.ProgressText.text, Does.Contain("8/10"));

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeEps12vPowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.Eps12vPowerCableInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(cableLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            ReleaseKeyboard(marker, keyboard);

            long inventoryRevisionInHands = session.Inventory.Revision;
            long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
            OperationResult routeBypass =
                marker.PlayerCarry.TrySetEps12vPowerCableRouteMode(true);
            Assert.That(routeBypass.IsFailure, Is.True);
            Assert.That(routeBypass.Error.Code,
                Is.EqualTo(
                    "custom-pc-eps12v-power-cable-build-kit.authority-blocked"));
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.False);
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionInHands));

            MovePlayerToEps12vBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("8/10 → 9/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(1).rotation),
                Is.LessThanOrEqualTo(0.25f));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(9));
            Assert.That(buildKit.ProgressText.text, Does.Contain("9/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("EPS12V HAZIR"));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(cable.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(cable.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cable.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(cable), Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeEps12vPowerCableBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.Eps12vPowerCableStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(cableLine));
            Assert.That(session.TryGetEps12vPowerCableItem(
                out InventoryItemRecord stagedCable), Is.True);
            Assert.That(stagedCable.Id, Is.EqualTo(cableLine.ItemId));
            Assert.That(stagedCable.ProductId, Is.EqualTo(cableLine.ProductId));
            Assert.That(stagedCable.ContainerId,
                Is.EqualTo(session.Eps12vPowerCableBuildKitContainerId));
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
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(routeState));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(routeRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(routeReceiptCount));
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator Eps12vBuildKitRejectsObstructionAndMissingSupportWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstEightForEps12vBuildKit(marker);

            PhysicalItemProjection cable =
                marker.Eps12vPowerCableBinding.PhysicalItem;
            Eps12vPowerCableBuildKitProjection buildKit =
                marker.Eps12vPowerCableBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));
            MovePlayerToEps12vBuildKit(marker, buildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetEps12vPowerCableBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            int physicalIdentity = cable.GetInstanceID();
            GameObject obstruction = CreateEps12vBuildKitObstruction(buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.Obstructed),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.TryConfirmEps12vPowerCableBuildKit()
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
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.OutsideSurface),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.TryConfirmEps12vPowerCableBuildKit()
                .IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            buildKit.SnapAnchor.position = authoredSnapPosition;
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadEps12vCoEdgesPauseAndReleaseRepressAreDeterministic()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstEightForEps12vBuildKit(marker);

            PhysicalItemProjection cable =
                marker.Eps12vPowerCableBinding.PhysicalItem;
            Eps12vPowerCableBuildKitProjection buildKit =
                marker.Eps12vPowerCableBuildKit;
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
            MovePlayerToEps12vBuildKit(marker, buildKit);
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
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(8));
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
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(8));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToEps12vBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerInput.RotatePlacementPressedThisFrame,
                Is.False);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.RightShoulder
                });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.RotatePlacementPressedThisFrame,
                Is.True);
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
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(9));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator Eps12vPlacementFailureRecoversSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstEightForEps12vBuildKit(marker);

            PhysicalItemProjection cable =
                marker.Eps12vPowerCableBinding.PhysicalItem;
            Eps12vPowerCableBuildKitProjection buildKit =
                marker.Eps12vPowerCableBuildKit;
            int physicalIdentity = cable.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(cable));

            long inventoryInHands = session.Inventory.Revision;
            long buildKitInHands = session.CustomPcBuildKit.Revision;
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands));

            MovePlayerToEps12vBuildKit(marker, buildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetEps12vPowerCableBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableBuildKitStatus,
                Is.EqualTo(Eps12vPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);

            FailNextStablePlacement(cable);
            var placement =
                marker.PlayerCarry.TryConfirmEps12vPowerCableBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(9));
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
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageFirstEightForEps12vBuildKit(
            GaragePrototypeMarker marker)
        {
            StageFirstSevenForAtx24BuildKit(marker);
            Atx24PowerCableBuildKitProjection atx24BuildKit =
                marker.Atx24PowerCableBuildKit;
            AssertSuccess(
                marker.PlayerCarry.TryPickup(
                    marker.Atx24PowerCableBinding.PhysicalItem));
            MovePlayerToAtx24BuildKit(marker, atx24BuildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetAtx24PowerCableBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentAtx24PowerCableBuildKitStatus,
                Is.EqualTo(Atx24PowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(
                marker.PlayerCarry.TryConfirmAtx24PowerCableBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(atx24BuildKit.IsStaged, Is.True);
            Assert.That(atx24BuildKit.StagedComponentCount,
                Is.EqualTo(8));
            Assert.That(marker.Atx24PowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.Atx24PowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
        }

        private static void MovePlayerToEps12vBuildKit(
            GaragePrototypeMarker marker,
            Eps12vPowerCableBuildKitProjection buildKit)
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

        private static GameObject CreateEps12vBuildKitObstruction(
            Eps12vPowerCableBuildKitProjection buildKit)
        {
            Pose pose = buildKit.ResolveSnapPose(0);
            Collider support = buildKit.SupportCollider;
            GameObject obstruction =
                GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Eps12vBuildKitFootprintObstruction";
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
