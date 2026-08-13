using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed class GarageGrayboxPlayModeTests : InputTestFixture
    {
        private const string SmallBoxId = "prototype.garage-box-001";
        private const string LargeBoxId = "prototype.garage-large-box-001";

        [UnityTest]
        public IEnumerator GarageLoadsWithOnePlayableRigAndPauseStateTransitions()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(Object.FindObjectsByType<GaragePrototypeMarker>(FindObjectsSortMode.None).Length, Is.EqualTo(1));
            Assert.That(marker.PlayerMotor, Is.Not.Null);
            Assert.That(marker.PlayerInput, Is.Not.Null);
            Assert.That(marker.PlayerInput.Actions, Is.Not.Null);

            // Batch-mode players report no window focus and intentionally pause the motor.
            // Resume explicitly and probe the authored spawn against the live floor collider.
            marker.PlayerMotor.SetPaused(false);
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            CollisionFlags spawnContact = controller.Move(Vector3.down * 0.10f);
            Assert.That(spawnContact.HasFlag(CollisionFlags.Below), Is.True);
            Assert.That(controller.isGrounded, Is.True);

            marker.PlayerMotor.SetPaused(true);
            Assert.That(marker.PlayerMotor.IsPaused, Is.True);
            marker.PlayerMotor.SetPaused(false);
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseAndGamepadDriveTheLiveMotor()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();

            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            Assert.That(
                marker.PlayerInput.Actions.FindActionMap(PlayerInputContract.PlayerMap, true).enabled,
                Is.True);
            Transform player = marker.PlayerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            Assert.That(cameraPivot, Is.Not.Null);

            Vector3 keyboardStart = player.position;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.LeftShift));
            InputSystem.Update();
            Assert.That(keyboard.wKey.isPressed, Is.True);
            Assert.That(marker.PlayerInput.Move.y, Is.GreaterThan(0.9f));
            Assert.That(marker.PlayerInput.SprintHeld, Is.True);
            yield return null;
            Assert.That(player.position.z, Is.GreaterThan(keyboardStart.z));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            float yawBeforeMouse = player.eulerAngles.y;
            float pitchBeforeMouse = cameraPivot.localEulerAngles.x;
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(30f, -12f) });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.Look.sqrMagnitude, Is.GreaterThan(0f));
            marker.PlayerMotor.SendMessage("Update");
            Assert.That(Mathf.DeltaAngle(yawBeforeMouse, player.eulerAngles.y), Is.Not.EqualTo(0f).Within(0.001f));
            Assert.That(Mathf.DeltaAngle(pitchBeforeMouse, cameraPivot.localEulerAngles.x), Is.Not.EqualTo(0f).Within(0.001f));

            Vector3 gamepadStart = player.position;
            float yawBeforeGamepad = player.eulerAngles.y;
            InputSystem.QueueStateEvent(gamepad, new GamepadState
            {
                leftStick = Vector2.up,
                rightStick = Vector2.right
            });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.Move.y, Is.GreaterThan(0.9f));
            yield return null;
            Assert.That(Vector3.Distance(gamepadStart, player.position), Is.GreaterThan(0.001f));
            Assert.That(Mathf.DeltaAngle(yawBeforeGamepad, player.eulerAngles.y), Is.Not.EqualTo(0f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator KeyboardInteractAndDropCarryTheSamePhysicalItem()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();

            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(SmallBoxId);
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.PlayerCarry, Is.Not.Null);
            Assert.That(item, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            Physics.SyncTransforms();

            string itemIdentity = item.ItemIdValue;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(item.IsCarried, Is.True);
            Assert.That(item.Body.isKinematic, Is.True);
            Assert.That(item.Body.detectCollisions, Is.False);
            Assert.That(item.GetComponentsInChildren<Collider>(true).All(collider => !collider.enabled), Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.IsCarried, Is.False);
            Assert.That(item.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(item.Body.isKinematic, Is.False);
            Assert.That(item.Body.detectCollisions, Is.True);
            Assert.That(item.GetComponentsInChildren<Collider>(true).Any(collider => collider.enabled), Is.True);
        }

        [UnityTest]
        public IEnumerator PauseBlocksPickupAndGamepadCanPickupAndDrop()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(gamepad, new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            InputSystem.QueueStateEvent(gamepad, new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Not.Null);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
        }

        [UnityTest]
        public IEnumerator KeyboardMousePlacementShowsBlockedGhostThenPlacesSameItem()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(SmallBoxId);
            marker.PlayerMotor.SetPaused(false);
            string identity = item.ItemIdValue;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToStockSurface(marker);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsPlacementMode, Is.True);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsShowingValidPose, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("GEÇERLİ"));
            Pose validPose = marker.PlayerCarry.PlacementPreview.CurrentPose;

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "PlacementTestBlocker";
            blocker.layer = LayerMask.NameToLayer("Interactable");
            blocker.transform.SetPositionAndRotation(validPose.position, validPose.rotation);
            blocker.transform.localScale = Vector3.one * 0.75f;
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            Assert.That(marker.PlayerCarry.CurrentPlacementStatus, Is.EqualTo(PlacementStatus.Blocked));
            Assert.That(marker.PlayerCarry.PlacementPreview.IsShowingValidPose, Is.False);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("ENGELLİ"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(marker.PlayerCarry.LastFailureCode, Is.EqualTo("placement.blocked"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            Object.Destroy(blocker);
            yield return null;
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);

            validPose = marker.PlayerCarry.PlacementPreview.CurrentPose;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.IsCarried, Is.False);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(Vector3.Distance(item.transform.position, validPose.position), Is.LessThan(0.001f));
            Assert.That(Mathf.DeltaAngle(item.transform.eulerAngles.y, 0f), Is.EqualTo(0f).Within(0.01f));
            Assert.That(item.Body.isKinematic, Is.True);
            Assert.That(item.Body.useGravity, Is.False);
            Vector3 stablePosition = item.transform.position;
            yield return new WaitForFixedUpdate();
            Assert.That(Vector3.Distance(item.transform.position, stablePosition), Is.LessThan(0.001f));
        }

        [UnityTest]
        public IEnumerator GamepadTriggerPlacementAndEastConfirmUseLiveInputActions()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(SmallBoxId);
            marker.PlayerMotor.SetPaused(false);
            string identity = item.ItemIdValue;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToStockSurface(marker);
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsPlacementMode, Is.True);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.IsCarried, Is.False);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.Body.isKinematic, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardLargeBoxCarryAppliesLoadAndBlockedDropFailsClosed()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(LargeBoxId);
            VisibleHandsPresenter hands = marker.PlayerMotor.GetComponentInChildren<VisibleHandsPresenter>(true);
            Camera camera = marker.PlayerMotor.GetComponentInChildren<Camera>(true);
            marker.PlayerMotor.SetPaused(false);
            marker.PlayerMotor.ViewSettings.Set(72f, 0.08f, 160f, false, false);
            marker.PlayerMotor.ApplyViewSettings();
            MovePlayerToLargeBox(marker);

            string identity = item.ItemIdValue;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(item.IsCarried, Is.True);
            Assert.That(item.CarryProfile, Is.EqualTo(PhysicalCarryProfile.LargeBox));
            Assert.That(marker.PlayerMotor.ActiveCarryProfile, Is.EqualTo(PhysicalCarryProfile.LargeBox));
            Assert.That(marker.PlayerMotor.CarryAllowsSprint, Is.False);
            Assert.That(marker.PlayerMotor.TargetFieldOfView, Is.EqualTo(66f).Within(0.001f));
            Assert.That(hands.State, Is.EqualTo(VisibleHandsState.CarryingLargeItem));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain(marker.PlayerInput.DropBindingPrompt));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("AĞIR YÜK"));
            Assert.That(marker.PlayerCarry.IsPlacementMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);
            Assert.That(
                GeometryUtility.TestPlanesAABB(
                    GeometryUtility.CalculateFrustumPlanes(camera),
                    item.GetComponentInChildren<Renderer>().bounds),
                Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.LeftShift));
            InputSystem.Update();
            Assert.That(marker.PlayerInput.SprintHeld, Is.True);
            Assert.That(marker.PlayerMotor.CurrentHorizontalSpeed, Is.EqualTo(2.275f).Within(0.001f));
            Vector3 movementStart = marker.PlayerMotor.transform.position;
            yield return null;
            Assert.That(
                Vector3.Distance(movementStart, marker.PlayerMotor.transform.position),
                Is.GreaterThan(0.001f));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return new WaitForSecondsRealtime(0.30f);
            Assert.That(camera.fieldOfView, Is.EqualTo(66f).Within(0.25f));

            GameObject blocker = CreateLargeDropBlocker(marker);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(marker.PlayerCarry.LastFailureCode, Is.EqualTo("drop.blocked"));
            Assert.That(hands.State, Is.EqualTo(VisibleHandsState.LargeDropBlocked));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("BIRAKMA ENGELLİ"));
            Assert.That(marker.PlayerMotor.ActiveCarryProfile, Is.EqualTo(PhysicalCarryProfile.LargeBox));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            Object.Destroy(blocker);
            yield return null;
            Physics.SyncTransforms();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.IsCarried, Is.False);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.Body.isKinematic, Is.False);
            Assert.That(item.Body.detectCollisions, Is.True);
            Assert.That(marker.PlayerMotor.ActiveCarryProfile, Is.Null);
            Assert.That(marker.PlayerMotor.TargetFieldOfView, Is.EqualTo(72f).Within(0.001f));
            yield return new WaitForSecondsRealtime(0.30f);
            Assert.That(camera.fieldOfView, Is.EqualTo(72f).Within(0.25f));
        }

        [UnityTest]
        public IEnumerator GamepadLargeBoxUsesEffectivePromptAndCannotEnterSmallPlacementMode()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(LargeBoxId);
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToLargeBox(marker);
            string identity = item.ItemIdValue;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain(marker.PlayerInput.DropBindingPrompt));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("Büyük Kargo Kutusu"));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsPlacementMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.IsCarried, Is.False);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(marker.PlayerMotor.ActiveCarryProfile, Is.Null);
        }

        [UnityTest]
        public IEnumerator DisablingCarryControllerRecoversHeldItemToItsSafeWorldPose()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(LargeBoxId);
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToLargeBox(marker);
            Vector3 safePosition = item.LastSafePosition;
            string identity = item.ItemIdValue;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(item.IsCarried, Is.True);

            marker.PlayerCarry.enabled = false;
            Assert.That(item.IsCarried, Is.False);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(Vector3.Distance(item.transform.position, safePosition), Is.LessThan(0.001f));
            Assert.That(item.Body.detectCollisions, Is.True);
            Assert.That(marker.PlayerMotor.ActiveCarryProfile, Is.Null);
        }

        [UnityTest]
        public IEnumerator WorldItemBelowRecoveryFloorReturnsWithSameIdentity()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();

            PhysicalItemProjection item = FindPhysicalItem(SmallBoxId);
            string identity = item.ItemIdValue;
            Vector3 safePosition = item.LastSafePosition;
            item.transform.position = new Vector3(0f, -30f, 0f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.transform.position.y, Is.GreaterThan(-20f));
            Assert.That(Vector3.Distance(item.transform.position, safePosition), Is.LessThan(0.05f));
        }

        private static void MovePlayerToStockSurface(GaragePrototypeMarker marker)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                new Vector3(2.15f, 0.05f, -2.55f),
                Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void MovePlayerToLargeBox(GaragePrototypeMarker marker)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                new Vector3(-1.5f, 0.05f, -2.5f),
                Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static PhysicalItemProjection FindPhysicalItem(string itemId)
        {
            return Object.FindObjectsByType<PhysicalItemProjection>(FindObjectsSortMode.None)
                .Single(item => item.ItemIdValue == itemId);
        }

        private static GameObject CreateLargeDropBlocker(GaragePrototypeMarker marker)
        {
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "LargeDropTestBlocker";
            blocker.layer = LayerMask.NameToLayer("Interactable");
            blocker.transform.position = marker.PlayerMotor.transform.position +
                                         (marker.PlayerMotor.transform.forward * 0.92f) +
                                         (Vector3.up * 0.55f);
            blocker.transform.localScale = new Vector3(1.6f, 1.1f, 1.8f);
            Physics.SyncTransforms();
            return blocker;
        }
    }
}
