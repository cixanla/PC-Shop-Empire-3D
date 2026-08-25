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
        public IEnumerator MemoryPickupRequiresBothStagedPrerequisitesWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardForProcessorBuildKit(marker);

            PhysicalItemProjection memory = marker.MemoryModule;
            MemoryModuleBuildKitProjection buildKit = marker.MemoryModuleBuildKit;
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord itemBefore),
                Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int physicalIdentity = memory.GetInstanceID();
            Transform worldParent = memory.transform.parent;
            Vector3 worldPosition = memory.transform.position;
            Quaternion worldRotation = memory.transform.rotation;

            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));

            AimPlayerAtItem(marker, memory, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(memory));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing.Code));
            Assert.That(marker.DimmBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.False);
            Assert.That(marker.DimmBinding.IsAuthorityInBuildKit, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.False);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord itemAfter),
                Is.True);
            Assert.That(itemAfter.Id, Is.EqualTo(itemBefore.Id));
            Assert.That(itemAfter.ProductId, Is.EqualTo(itemBefore.ProductId));
            Assert.That(itemAfter.ContainerId, Is.EqualTo(itemBefore.ContainerId));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(memory.transform.parent, Is.SameAs(worldParent));
            Assert.That(memory.transform.position, Is.EqualTo(worldPosition));
            Assert.That(memory.transform.rotation, Is.EqualTo(worldRotation));
            Assert.That(memory.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseMovesExactReservedMemoryFromTwoToThree()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardAndProcessorForMemoryBuildKit(marker);

            Assert.That(marker.HasMemoryModuleBuildKitR37Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot memoryLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.MemoryModule);
            Assert.That(memoryLine.ItemId, Is.EqualTo(session.MemoryItemId));
            Assert.That(session.Inventory.TryGetReservation(
                memoryLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(memoryLine.ItemId));

            PhysicalItemProjection memory = marker.MemoryModule;
            MemoryModuleBuildKitProjection buildKit = marker.MemoryModuleBuildKit;
            int physicalIdentity = memory.GetInstanceID();
            string itemIdentity = memory.ItemIdValue;
            int worldLayer = memory.gameObject.layer;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            MemorySlotState memorySlotState = session.AssemblyBuild.MemorySlotState;
            DimmLatchVisualPhase latchPhase = marker.DimmSlot.LatchVisualPhase;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup = session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(buildKit.ProgressText.text, Does.Contain("2/10"));

            AimPlayerAtItem(marker, memory, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(memory));
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeMemoryModuleBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MemoryModuleInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(memoryLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsMemoryModuleBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsDimmSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentMemoryModuleBuildKitStatus,
                Is.EqualTo(MemoryModuleBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("2/10 → 3/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentMemoryModuleBuildKitStatus,
                Is.EqualTo(MemoryModuleBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(buildKit.ProgressText.text, Does.Contain("3/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("BELLEK HAZIR"));
            Assert.That(marker.DimmBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(memory.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(memory.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(memory.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(memory.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(memory), Is.True);
            Assert.That(Quaternion.Angle(
                memory.transform.rotation,
                buildKit.ResolveSnapPose(1).rotation), Is.LessThanOrEqualTo(0.25f));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeMemoryModuleBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MemoryModuleStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(memoryLine));
            Assert.That(session.TryGetMemoryItem(
                out InventoryItemRecord stagedMemory), Is.True);
            Assert.That(stagedMemory.ContainerId,
                Is.EqualTo(session.MemoryModuleBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                memoryLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId, Is.EqualTo(stagedMemory.Id));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 2));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(memorySlotState));
            Assert.That(marker.DimmSlot.LatchVisualPhase, Is.EqualTo(latchPhase));
            Assert.That(marker.MotherboardBuildKit.IsStaged, Is.True);
            Assert.That(marker.ProcessorBuildKit.IsStaged, Is.True);
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator MemoryBuildKitSameFramePrimaryRotateDropHasOneConsumer()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardAndProcessorForMemoryBuildKit(marker);

            PhysicalItemProjection memory = marker.MemoryModule;
            MemoryModuleBuildKitProjection buildKit = marker.MemoryModuleBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(memory));
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

            Assert.That(marker.PlayerCarry.IsMemoryModuleBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsDimmSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.G);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.DimmBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator GamepadMemoryPauseCoEdgeRequiresReleaseRepress()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardAndProcessorForMemoryBuildKit(marker);

            PhysicalItemProjection memory = marker.MemoryModule;
            MemoryModuleBuildKitProjection buildKit = marker.MemoryModuleBuildKit;
            int physicalIdentity = memory.GetInstanceID();
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            MemorySlotState memorySlotState = session.AssemblyBuild.MemorySlotState;
            DimmLatchVisualPhase latchPhase = marker.DimmSlot.LatchVisualPhase;
            AimPlayerAtItem(marker, memory, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
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
            Assert.That(marker.PlayerCarry.IsMemoryModuleBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsDimmSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(marker.PlayerCarry.CurrentMemoryModuleBuildKitStatus,
                Is.EqualTo(MemoryModuleBuildKitStatus.Valid),
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
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));
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
            Assert.That(marker.DimmBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(memorySlotState));
            Assert.That(marker.DimmSlot.LatchVisualPhase, Is.EqualTo(latchPhase));
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator MemoryPlacementFailureRecoversExactSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardAndProcessorForMemoryBuildKit(marker);

            PhysicalItemProjection memory = marker.MemoryModule;
            MemoryModuleBuildKitProjection buildKit = marker.MemoryModuleBuildKit;
            int physicalIdentity = memory.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(memory));

            long inventoryInHands = session.Inventory.Revision;
            long buildKitInHands = session.CustomPcBuildKit.Revision;
            OperationResult genericDrop = marker.PlayerCarry.TryDrop();
            Assert.That(genericDrop.IsFailure, Is.True);
            Assert.That(genericDrop.Error.Code,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands));

            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetMemoryModuleBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentMemoryModuleBuildKitStatus,
                Is.EqualTo(MemoryModuleBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            FailNextStablePlacement(memory);
            OperationResult placement =
                marker.PlayerCarry.TryConfirmMemoryModuleBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.DimmBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(memory.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(memory.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                memory.transform.position,
                expectedPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                memory.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.05f));
            Assert.That(buildKit.MatchesCommittedPlacement(memory), Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryInHands + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageMotherboardAndProcessorForMemoryBuildKit(
            GaragePrototypeMarker marker)
        {
            StageMotherboardForProcessorBuildKit(marker);
            ProcessorBuildKitProjection processorBuildKit = marker.ProcessorBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.Processor));
            MovePlayerToBuildKit(marker, processorBuildKit);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentProcessorBuildKitStatus,
                Is.EqualTo(ProcessorBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(processorBuildKit.IsStaged, Is.True);
            Assert.That(processorBuildKit.StagedComponentCount, Is.EqualTo(2));
            Assert.That(marker.ProcessorBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
        }

        private static void MovePlayerToBuildKit(
            GaragePrototypeMarker marker,
            MemoryModuleBuildKitProjection buildKit)
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
    }
}
