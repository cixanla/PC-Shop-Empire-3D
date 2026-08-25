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
        public IEnumerator ProcessorPickupRequiresStagedMotherboardWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);

            PhysicalItemProjection processor = marker.Processor;
            ProcessorBuildKitProjection buildKit = marker.ProcessorBuildKit;
            Assert.That(session.TryGetProcessorItem(
                out InventoryItemRecord itemBefore), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int physicalIdentity = processor.GetInstanceID();
            Transform worldParent = processor.transform.parent;
            Vector3 worldPosition = processor.transform.position;
            Quaternion worldRotation = processor.transform.rotation;

            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(processor));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing.Code));
            Assert.That(marker.ProcessorBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.False);
            Assert.That(marker.ProcessorBinding.IsAuthorityInBuildKit, Is.False);
            Assert.That(buildKit.HasMotherboardPrerequisite, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.False);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.TryGetProcessorItem(
                out InventoryItemRecord itemAfter), Is.True);
            Assert.That(itemAfter.Id, Is.EqualTo(itemBefore.Id));
            Assert.That(itemAfter.ProductId, Is.EqualTo(itemBefore.ProductId));
            Assert.That(itemAfter.ContainerId, Is.EqualTo(itemBefore.ContainerId));
            Assert.That(processor.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(processor.transform.parent, Is.SameAs(worldParent));
            Assert.That(processor.transform.position, Is.EqualTo(worldPosition));
            Assert.That(processor.transform.rotation, Is.EqualTo(worldRotation));
            Assert.That(processor.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseMovesExactReservedProcessorFromOneToTwo()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot processorLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            Assert.That(processorLine.ItemId, Is.EqualTo(session.ProcessorItemId));
            Assert.That(session.Inventory.TryGetReservation(
                processorLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(processorLine.ItemId));

            PhysicalItemProjection processor = marker.Processor;
            ProcessorBuildKitProjection buildKit = marker.ProcessorBuildKit;
            int physicalIdentity = processor.GetInstanceID();
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup = session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(buildKit.ProgressText.text, Does.Contain("1/10"));

            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(processor));
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(processorLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentProcessorBuildKitStatus,
                Is.EqualTo(ProcessorBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("1/10 → 2/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentProcessorBuildKitStatus,
                Is.EqualTo(ProcessorBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(buildKit.ProgressText.text, Does.Contain("2/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("İŞLEMCİ HAZIR"));
            Assert.That(marker.ProcessorBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(processor.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(processor.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(processor.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(processor), Is.True);
            Assert.That(Quaternion.Angle(
                processor.transform.rotation,
                buildKit.ResolveSnapPose(1).rotation), Is.LessThanOrEqualTo(0.25f));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(processorLine));
            Assert.That(session.TryGetProcessorItem(
                out InventoryItemRecord stagedProcessor), Is.True);
            Assert.That(stagedProcessor.ContainerId,
                Is.EqualTo(session.ProcessorBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                processorLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId, Is.EqualTo(stagedProcessor.Id));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 2));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator ProcessorBuildKitSameFramePrimaryRotateDropHasOneConsumer()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            PhysicalItemProjection processor = marker.Processor;
            ProcessorBuildKitProjection buildKit = marker.ProcessorBuildKit;
            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            ReleaseKeyboard(marker, keyboard);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.R, Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.G);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.ProcessorBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator ProcessorBuildKitPrimaryExitsAfterLookingAwayWithoutEnteringSocketMode()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            PhysicalItemProjection processor = marker.Processor;
            ProcessorBuildKitProjection buildKit = marker.ProcessorBuildKit;
            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            ReleaseKeyboard(marker, keyboard);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            ReleaseMouse(marker, mouse);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            Vector3 playerPosition = marker.PlayerMotor.transform.position;
            SetPlayerLook(
                marker,
                playerPosition,
                playerPosition + Vector3.back);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(buildKit.HasContextualAttention, Is.False);
            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.True);

            PressMouse(marker, mouse);

            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseMouse(marker, mouse);
        }

        [UnityTest]
        public IEnumerator ProcessorBuildKitReceiptNeverRoutesPrimaryToProcessorSocket()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            PhysicalItemProjection processor = marker.Processor;
            ProcessorBuildKitProjection buildKit = marker.ProcessorBuildKit;
            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            ReleaseKeyboard(marker, keyboard);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(buildKit.HasPickupReceipt, Is.True);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.True);
            ReleaseMouse(marker, mouse);

            Vector3 playerPosition = marker.PlayerMotor.transform.position;
            SetPlayerLook(
                marker,
                playerPosition,
                playerPosition + Vector3.back);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            ReleaseMouse(marker, mouse);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            MovePlayerToProcessorSocketForBuildKit(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("CPU Build Kit"));
            PressMouse(marker, mouse);

            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorInHands));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseMouse(marker, mouse);
        }

        [UnityTest]
        public IEnumerator GamepadProcessorPauseCoEdgeRequiresReleaseRepress()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            PhysicalItemProjection processor = marker.Processor;
            ProcessorBuildKitProjection buildKit = marker.ProcessorBuildKit;
            int physicalIdentity = processor.GetInstanceID();
            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));

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
            Assert.That(marker.PlayerCarry.IsProcessorBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(marker.PlayerCarry.CurrentProcessorBuildKitStatus,
                Is.EqualTo(ProcessorBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.Start) |
                              (1u << (int)GamepadButton.East)
                });
            yield return null;
            yield return null;

            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.ProcessorBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(processor.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator ProcessorPlacementFailureRecoversExactSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            PhysicalItemProjection processor = marker.Processor;
            ProcessorBuildKitProjection buildKit = marker.ProcessorBuildKit;
            int physicalIdentity = processor.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(processor));
            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentProcessorBuildKitStatus,
                Is.EqualTo(ProcessorBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            FailNextStablePlacement(processor);
            OperationResult placement =
                marker.PlayerCarry.TryConfirmProcessorBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.ProcessorBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(processor.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(processor.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(processor.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                processor.transform.position,
                expectedPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                processor.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.05f));
            Assert.That(buildKit.MatchesCommittedPlacement(processor), Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator ProcessorBuildKitGenericPlacementCartAndDropRemainFailClosed()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            PhysicalItemProjection processor = marker.Processor;
            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            ReleaseKeyboard(marker, keyboard);

            int physicalIdentity = processor.GetInstanceID();
            Transform carryParent = processor.transform.parent;
            Vector3 carryLocalPosition = processor.transform.localPosition;
            Quaternion carryLocalRotation = processor.transform.localRotation;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            OperationResult genericPlacement =
                marker.PlayerCarry.TryConfirmPlacement();
            Assert.That(genericPlacement.IsFailure, Is.True);
            Assert.That(genericPlacement.Error.Code,
                Is.EqualTo("placement.profile-unsupported"));
            AssertProcessorBuildKitHeldStateUnchanged(
                marker,
                session,
                processor,
                physicalIdentity,
                carryParent,
                carryLocalPosition,
                carryLocalRotation,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount);

            OperationResult genericCart =
                marker.PlayerCarry.TryLoadHeldItem(marker.TransportCart);
            Assert.That(genericCart.IsFailure, Is.True);
            Assert.That(genericCart.Error.Code,
                Is.EqualTo("cart.load-profile-unsupported"));
            AssertProcessorBuildKitHeldStateUnchanged(
                marker,
                session,
                processor,
                physicalIdentity,
                carryParent,
                carryLocalPosition,
                carryLocalRotation,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(
                    InventoryFailures
                        .SerializedReservationWorkOrderBuildKitConflict.Code));
            AssertProcessorBuildKitHeldStateUnchanged(
                marker,
                session,
                processor,
                physicalIdentity,
                carryParent,
                carryLocalPosition,
                carryLocalRotation,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount);
            ReleaseKeyboard(marker, keyboard);
        }

        private static void AssertProcessorBuildKitHeldStateUnchanged(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection processor,
            int physicalIdentity,
            Transform carryParent,
            Vector3 carryLocalPosition,
            Quaternion carryLocalRotation,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount)
        {
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(processor.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(processor.IsCarried, Is.True);
            Assert.That(processor.transform.parent, Is.SameAs(carryParent));
            Assert.That(processor.transform.localPosition,
                Is.EqualTo(carryLocalPosition));
            Assert.That(processor.transform.localRotation,
                Is.EqualTo(carryLocalRotation));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.ProcessorBinding.IsAuthorityInBuildKit, Is.False);
            Assert.That(marker.ProcessorBuildKit.StagedComponentCount,
                Is.EqualTo(1));
            Assert.That(marker.TransportCart.HasCargo, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorInHands));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageMotherboardForProcessorBuildKit(
            GaragePrototypeMarker marker)
        {
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(
                marker.MotherboardBinding.PhysicalItem));
            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetMotherboardBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmMotherboardBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
        }

        private static void MovePlayerToBuildKit(
            GaragePrototypeMarker marker,
            ProcessorBuildKitProjection buildKit)
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

        private static void MovePlayerToProcessorSocketForBuildKit(
            GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.ProcessorSocket.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void AssertSuccess(OperationResult result)
        {
            Assert.That(result.IsSuccess, Is.True,
                result.IsFailure ? result.Error.Code : string.Empty);
        }
    }
}
