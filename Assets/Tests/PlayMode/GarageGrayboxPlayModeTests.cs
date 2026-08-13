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
            PhysicalItemProjection item = Object.FindFirstObjectByType<PhysicalItemProjection>();
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
        public IEnumerator DisablingCarryControllerRecoversHeldItemToItsSafeWorldPose()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = Object.FindFirstObjectByType<PhysicalItemProjection>();
            marker.PlayerMotor.SetPaused(false);
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
        }

        [UnityTest]
        public IEnumerator WorldItemBelowRecoveryFloorReturnsWithSameIdentity()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();

            PhysicalItemProjection item = Object.FindFirstObjectByType<PhysicalItemProjection>();
            string identity = item.ItemIdValue;
            Vector3 safePosition = item.LastSafePosition;
            item.transform.position = new Vector3(0f, -30f, 0f);
            Physics.SyncTransforms();
            yield return new WaitForFixedUpdate();

            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.transform.position.y, Is.GreaterThan(-20f));
            Assert.That(Vector3.Distance(item.transform.position, safePosition), Is.LessThan(0.05f));
        }
    }
}
