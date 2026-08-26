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
        public IEnumerator KeyboardMouseMovesExactReservedGraphicsCardFromFiveToSix()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFiveForGraphicsCardBuildKit(marker);

            Assert.That(marker.HasGraphicsCardBuildKitR40Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot graphicsCardLine =
                workOrder.Lines.Single(
                    line => line.ComponentKind == PcComponentKind.GraphicsCard);
            Assert.That(graphicsCardLine.ItemId,
                Is.EqualTo(session.GraphicsCardAssemblyItemId));
            Assert.That(session.Inventory.TryGetReservation(
                graphicsCardLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(graphicsCardLine.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));

            PhysicalItemProjection graphicsCard =
                marker.GraphicsCardBinding.PhysicalItem;
            GraphicsCardBuildKitProjection buildKit =
                marker.GraphicsCardBuildKit;
            int physicalIdentity = graphicsCard.GetInstanceID();
            string itemIdentity = graphicsCard.ItemIdValue;
            int worldLayer = graphicsCard.gameObject.layer;
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup =
                session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.HasMemoryModulePrerequisite, Is.True);
            Assert.That(buildKit.HasStoragePrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorCoolerPrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(buildKit.ProgressText.text, Does.Contain("5/10"));

            AimPlayerAtItem(marker, graphicsCard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem,
                Is.SameAs(graphicsCard));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeGraphicsCardBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.GraphicsCardInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(graphicsCardLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            ReleaseKeyboard(marker, keyboard);

            long inventoryRevisionInHands = session.Inventory.Revision;
            long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionInHands));

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsGraphicsCardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardBuildKitStatus,
                Is.EqualTo(GraphicsCardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("5/10 → 6/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardBuildKitStatus,
                Is.EqualTo(GraphicsCardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(1).rotation),
                Is.LessThanOrEqualTo(0.25f));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(buildKit.ProgressText.text, Does.Contain("6/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("GPU HAZIR"));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(graphicsCard.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(graphicsCard.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(graphicsCard.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(graphicsCard), Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeGraphicsCardBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.GraphicsCardStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(graphicsCardLine));
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord stagedGraphicsCard), Is.True);
            Assert.That(stagedGraphicsCard.Id,
                Is.EqualTo(graphicsCardLine.ItemId));
            Assert.That(stagedGraphicsCard.ProductId,
                Is.EqualTo(graphicsCardLine.ProductId));
            Assert.That(stagedGraphicsCard.ContainerId,
                Is.EqualTo(session.GraphicsCardBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                graphicsCardLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId,
                Is.EqualTo(stagedGraphicsCard.Id));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 2));
            AssertGraphicsCardAndPcieUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount,
                pcieState,
                pcieRevision,
                pcieReceiptCount);
            Assert.That(marker.GraphicsCardBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator GraphicsCardBuildKitRotationCyclesTwoPosesAndResets()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFiveForGraphicsCardBuildKit(marker);

            PhysicalItemProjection graphicsCard =
                marker.GraphicsCardBinding.PhysicalItem;
            GraphicsCardBuildKitProjection buildKit =
                marker.GraphicsCardBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(graphicsCard));
            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardBuildKitMode(true));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;

            for (int expectedTurn = 1; expectedTurn <= 2; expectedTurn++)
            {
                PressKeyboard(marker, keyboard, Key.R);
                int normalizedTurn = expectedTurn % 2;
                Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                    Is.EqualTo(normalizedTurn));
                Assert.That(marker.PlayerCarry.CurrentGraphicsCardBuildKitStatus,
                    Is.EqualTo(GraphicsCardBuildKitStatus.Valid),
                    marker.PlayerCarry.LastFailureCode);
                Assert.That(Quaternion.Angle(
                    marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                    buildKit.ResolveSnapPose(normalizedTurn).rotation),
                    Is.LessThanOrEqualTo(0.25f));
                ReleaseKeyboard(marker, keyboard);
            }

            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardBuildKitMode(false));
            Assert.That(marker.PlayerCarry.IsGraphicsCardBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardBuildKitMode(true));
            Assert.That(marker.PlayerCarry.IsGraphicsCardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(marker.GraphicsCardBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadGraphicsCardCoEdgesPauseAndReleaseRepressAreDeterministic()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFiveForGraphicsCardBuildKit(marker);

            PhysicalItemProjection graphicsCard =
                marker.GraphicsCardBinding.PhysicalItem;
            GraphicsCardBuildKitProjection buildKit =
                marker.GraphicsCardBuildKit;
            int physicalIdentity = graphicsCard.GetInstanceID();
            AimPlayerAtItem(marker, graphicsCard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(graphicsCard));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
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
            Assert.That(marker.PlayerCarry.IsGraphicsCardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
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
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
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
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
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
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(marker.GraphicsCardBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator GraphicsCardPlacementFailureRecoversSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFiveForGraphicsCardBuildKit(marker);

            PhysicalItemProjection graphicsCard =
                marker.GraphicsCardBinding.PhysicalItem;
            GraphicsCardBuildKitProjection buildKit =
                marker.GraphicsCardBuildKit;
            int physicalIdentity = graphicsCard.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(graphicsCard));

            long inventoryInHands = session.Inventory.Revision;
            long buildKitInHands = session.CustomPcBuildKit.Revision;
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands));

            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardBuildKitStatus,
                Is.EqualTo(GraphicsCardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);

            FailNextStablePlacement(graphicsCard);
            var placement = marker.PlayerCarry.TryConfirmGraphicsCardBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(graphicsCard.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                graphicsCard.transform.position,
                expectedPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                graphicsCard.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.05f));
            Assert.That(buildKit.MatchesCommittedPlacement(graphicsCard), Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryInHands + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands + 1));
            Assert.That(marker.GraphicsCardBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageFirstFiveForGraphicsCardBuildKit(
            GaragePrototypeMarker marker)
        {
            StageFirstFourForProcessorCoolerBuildKit(marker);
            ProcessorCoolerBuildKitProjection processorCoolerBuildKit =
                marker.ProcessorCoolerBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.ProcessorCooler));
            MovePlayerToBuildKit(marker, processorCoolerBuildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetProcessorCoolerBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerBuildKitStatus,
                Is.EqualTo(ProcessorCoolerBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(
                marker.PlayerCarry.TryConfirmProcessorCoolerBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(processorCoolerBuildKit.IsStaged, Is.True);
            Assert.That(processorCoolerBuildKit.StagedComponentCount,
                Is.EqualTo(5));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.ProcessorCoolerBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
        }

        private static void MovePlayerToBuildKit(
            GaragePrototypeMarker marker,
            GraphicsCardBuildKitProjection buildKit)
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

        private static void AssertGraphicsCardAndPcieUnchanged(
            GarageStockFlowSession session,
            AssemblyBuildSnapshot expected,
            int expectedAssemblyReceiptCount,
            PcieGpuPowerCableState expectedPcieState,
            long expectedPcieRevision,
            int expectedPcieReceiptCount)
        {
            AssemblyBuildSnapshot actual = session.AssemblyBuild.GetSnapshot();
            Assert.That(actual.Revision, Is.EqualTo(expected.Revision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(expectedAssemblyReceiptCount));
            Assert.That(actual.GraphicsCardSlotState,
                Is.EqualTo(expected.GraphicsCardSlotState));
            Assert.That(actual.GraphicsCardItemId,
                Is.EqualTo(expected.GraphicsCardItemId));
            Assert.That(actual.GraphicsCardProductId,
                Is.EqualTo(expected.GraphicsCardProductId));
            Assert.That(actual.GraphicsCardSeatedByOperationId,
                Is.EqualTo(expected.GraphicsCardSeatedByOperationId));
            Assert.That(actual.GraphicsCardRetainedByOperationId,
                Is.EqualTo(expected.GraphicsCardRetainedByOperationId));
            Assert.That(actual.GraphicsCardMountOrientation,
                Is.EqualTo(expected.GraphicsCardMountOrientation));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(expectedPcieState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(expectedPcieRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(expectedPcieReceiptCount));
        }
    }
}
