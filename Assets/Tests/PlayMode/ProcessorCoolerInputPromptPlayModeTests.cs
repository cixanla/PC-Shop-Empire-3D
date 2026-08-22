using System.Collections;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    /// <summary>
    /// Real Input System coverage for the r27 cooler flow and active-device prompts.
    /// </summary>
    public sealed class ProcessorCoolerInputPromptPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator PromptFamilyTracksTheLastRealInputDevice()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();

            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.False);
            AssertKeyboardPrompts(marker);

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            AssertGamepadPrompts(marker);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            InputSystem.Update();
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.False);
            AssertKeyboardPrompts(marker);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseCompletesCoolerRetentionRemovalAndRecovery()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection cooler = marker.ProcessorCooler;
            Pose initialPose = new Pose(
                cooler.transform.position,
                cooler.transform.rotation);
            int physicalIdentity = cooler.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);
            PrepareRetainedProcessorHost(marker);

            AimPlayerAtItem(marker, cooler, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem,
                Is.SameAs(cooler),
                $"focused={marker.PlayerCarry.FocusedItem?.name ?? "<none>"} " +
                $"failure={marker.PlayerCarry.LastFailureCode}");
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsProcessorCoolerSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerSlotStatus,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            AssertKeyboardPrompts(marker);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] 4 NOKTA HİZALI • YÖN 0° • G: oturt • R: 180° döndür • LMB: çık"));
            ReleaseMouse(marker, mouse);

            long rotationAssemblyRevision = session.AssemblyBuild.Revision;
            long rotationInventoryRevision = session.Inventory.Revision;
            int rotationReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(1));
            Assert.That(marker.ProcessorCoolerSlot.LastEvaluation.Orientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Rotated180));
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerSlotStatus,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(rotationAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(rotationInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(rotationReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            Assert.That(session.AssemblyBuild.ProcessorCoolerMountOrientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Rotated180));
            AssertConsumedTim(session);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("1→3→2→4"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerRetained.Code));
            ReleaseKeyboard(marker, keyboard);

            PressMouse(marker, mouse);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            ReleaseMouse(marker, mouse);
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecoveredCooler(
                marker,
                session,
                cooler,
                initialPose,
                physicalIdentity,
                recovery);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesCoolerRotationRetentionAndRecovery()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection cooler = marker.ProcessorCooler;
            Pose initialPose = new Pose(
                cooler.transform.position,
                cooler.transform.rotation);
            int physicalIdentity = cooler.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);
            PrepareRetainedProcessorHost(marker);

            AimPlayerAtItem(marker, cooler, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem,
                Is.SameAs(cooler),
                $"focused={marker.PlayerCarry.FocusedItem?.name ?? "<none>"} " +
                $"failure={marker.PlayerCarry.LastFailureCode}");
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            AssertGamepadPrompts(marker);
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadTrigger(marker, gamepad);
            Assert.That(marker.PlayerCarry.IsProcessorCoolerSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerSlotStatus,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] 4 NOKTA HİZALI • YÖN 0° • B: oturt • RB: 180° döndür • RT: çık"));
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.RightShoulder);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(1));
            Assert.That(marker.ProcessorCoolerSlot.LastEvaluation.Orientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Rotated180));
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.East);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            AssertConsumedTim(session);
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadTrigger(marker, gamepad);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerRetained.Code));
            ReleaseGamepad(marker, gamepad);

            PressGamepadTrigger(marker, gamepad);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecoveredCooler(
                marker,
                session,
                cooler,
                initialPose,
                physicalIdentity,
                recovery);
        }

        private static IEnumerator LoadGarage()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(Object.FindFirstObjectByType<GaragePrototypeMarker>(), Is.Not.Null);
        }

        private static void PrepareRetainedProcessorHost(GaragePrototypeMarker marker)
        {
            AssertSuccess(marker.PlayerCarry.TryPickup(
                marker.MotherboardBinding.PhysicalItem));
            MovePlayerToFocus(marker, marker.MotherboardSeat.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetMotherboardSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmMotherboardSeat());
            MovePlayerToFocus(marker, marker.MotherboardFastener.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateMotherboardFastener());

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.Processor));
            MovePlayerToFocus(marker, marker.ProcessorSocket.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorSeat());
            MovePlayerToFocus(marker, marker.ProcessorSocket.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorRetention());
            Assert.That(marker.StockFlow.Session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(marker.StockFlow.Session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
        }

        private static void AssertSuccess(OperationResult result)
        {
            Assert.That(result.IsSuccess, Is.True,
                result.IsFailure ? result.Error.Code : string.Empty);
        }

        private static void AssertKeyboardPrompts(GaragePrototypeMarker marker)
        {
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("E"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("G"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt, Is.EqualTo("R"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("LMB"));
        }

        private static void AssertGamepadPrompts(GaragePrototypeMarker marker)
        {
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt, Is.EqualTo("RB"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));
        }

        private static void AssertConsumedTim(GarageStockFlowSession session)
        {
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.AppliedConsumed));
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord item), Is.True);
            Assert.That(item.StateFlags,
                Is.EqualTo(
                    InventorySerializedItemStateFlags
                        .PreAppliedConsumableConsumed));
        }

        private static void AssertRecoveredCooler(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection cooler,
            Pose initialPose,
            int physicalIdentity,
            OperationResult recovery)
        {
            AssertSuccess(recovery);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(Vector3.Distance(
                cooler.transform.position,
                initialPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                cooler.transform.rotation,
                initialPose.rotation), Is.LessThan(0.05f));
            Assert.That(cooler.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cooler.IsStablePlacement, Is.True);
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.Unsupported));
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord recoveredItem), Is.True);
            Assert.That(recoveredItem.StateFlags,
                Is.EqualTo(
                    InventorySerializedItemStateFlags
                        .PreAppliedConsumableConsumed));
            Assert.That(marker.ProcessorCoolerBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PressKeyboard(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseKeyboard(
            GaragePrototypeMarker marker,
            Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void PressMouse(
            GaragePrototypeMarker marker,
            Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseMouse(
            GaragePrototypeMarker marker,
            Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void PressGamepadButton(
            GaragePrototypeMarker marker,
            Gamepad gamepad,
            GamepadButton button)
        {
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)button });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void PressGamepadTrigger(
            GaragePrototypeMarker marker,
            Gamepad gamepad)
        {
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseGamepad(
            GaragePrototypeMarker marker,
            Gamepad gamepad)
        {
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void MovePlayerToFocus(
            GaragePrototypeMarker marker,
            Collider focus)
        {
            Vector3 target = focus.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void AimPlayerAtItem(
            GaragePrototypeMarker marker,
            PhysicalItemProjection item,
            Vector3 approachDirection)
        {
            Vector3 target = item.InteractionCenter;
            Vector3 playerPosition = target +
                                     (approachDirection.normalized * 1.25f);
            playerPosition.y = 0.05f;
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void SetPlayerLook(
            GaragePrototypeMarker marker,
            Vector3 playerPosition,
            Vector3 target)
        {
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }
    }
}
