using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;
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
        private const string StackBaseBoxId = "prototype.garage-box-002";
        private const string LargeBoxId = "prototype.garage-large-box-001";
        private const string TransportCartId = "prototype.garage-transport-cart-001";
        private const string DeliveryItemId = GarageStockFlowSession.ItemInstanceIdValue;

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
            Assert.That(
                marker.PlayerCarry.PlacementValid,
                Is.True,
                $"status={marker.PlayerCarry.CurrentPlacementStatus} " +
                $"failure={marker.PlayerCarry.LastFailureCode}");
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsShowingValidPose, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("GEÇERLİ"));
            Pose initialPose = marker.PlayerCarry.PlacementPreview.CurrentPose;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.PlacementRotationDegrees, Is.EqualTo(90f));
            Assert.That(
                marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.RotatePlacementBindingPrompt));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("[90°]"));
            Pose validPose = marker.PlayerCarry.PlacementPreview.CurrentPose;
            Assert.That(
                Mathf.DeltaAngle(initialPose.rotation.eulerAngles.y, validPose.rotation.eulerAngles.y),
                Is.EqualTo(90f).Within(0.01f));
            Assert.That(Vector3.Distance(initialPose.position, validPose.position), Is.LessThan(0.001f));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
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
            Assert.That(Mathf.DeltaAngle(item.transform.eulerAngles.y, 90f), Is.EqualTo(0f).Within(0.01f));
            Assert.That(item.Body.isKinematic, Is.True);
            Assert.That(item.Body.useGravity, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
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

            Pose initialPose = marker.PlayerCarry.PlacementPreview.CurrentPose;
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.RightShoulder });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(1));
            Assert.That(
                marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.RotatePlacementBindingPrompt));
            Pose rotatedPose = marker.PlayerCarry.PlacementPreview.CurrentPose;
            Assert.That(
                Mathf.DeltaAngle(initialPose.rotation.eulerAngles.y, rotatedPose.rotation.eulerAngles.y),
                Is.EqualTo(90f).Within(0.01f));

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
            Assert.That(Mathf.DeltaAngle(item.transform.eulerAngles.y, 90f), Is.EqualTo(0f).Within(0.01f));
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
        }

        [UnityTest]
        public IEnumerator KeyboardStackingRequiresFullSupportAndPreservesBothIdentities()
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
            PhysicalItemProjection support = FindPhysicalItem(StackBaseBoxId);
            marker.PlayerMotor.SetPaused(false);
            string itemIdentity = item.ItemIdValue;
            string supportIdentity = support.ItemIdValue;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToStackSupport(marker);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.CurrentStackSupport, Is.SameAs(support));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("İSTİF GEÇERLİ"));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            Assert.That(marker.PlayerCarry.CurrentPlacementStatus, Is.EqualTo(PlacementStatus.OutsideSurface));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(item.ItemIdValue, Is.EqualTo(itemIdentity));

            for (int turn = 0; turn < 3; turn++)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
                InputSystem.Update();
                marker.PlayerCarry.ProcessInputFrame();
            }

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.CurrentStackSupport, Is.SameAs(support));
            Pose stackPose = marker.PlayerCarry.PlacementPreview.CurrentPose;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(support.ItemIdValue, Is.EqualTo(supportIdentity));
            Assert.That(item.StackSupport, Is.SameAs(support));
            Assert.That(support.StackedItem, Is.SameAs(item));
            Assert.That(Vector3.Distance(item.transform.position, stackPose.position), Is.LessThan(0.001f));
            Assert.That(item.Body.isKinematic, Is.True);
            Vector3 stablePosition = item.transform.position;
            yield return new WaitForFixedUpdate();
            Assert.That(Vector3.Distance(item.transform.position, stablePosition), Is.LessThan(0.001f));

            var blockedBasePickup = marker.PlayerCarry.TryPickup(support);
            Assert.That(blockedBasePickup.IsFailure, Is.True);
            Assert.That(blockedBasePickup.Error.Code, Is.EqualTo("pickup.stack-occupied"));
            Assert.That(support.StackedItem, Is.SameAs(item));
        }

        [UnityTest]
        public IEnumerator GamepadCanPlaceSmallBoxOnStableSmallBoxSupport()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(SmallBoxId);
            PhysicalItemProjection support = FindPhysicalItem(StackBaseBoxId);
            marker.PlayerMotor.SetPaused(false);

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToStackSupport(marker);
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.CurrentStackSupport, Is.SameAs(support));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.StackSupport, Is.SameAs(support));
            Assert.That(support.StackedItem, Is.SameAs(item));
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
                new GamepadState { buttons = 1u << (int)GamepadButton.RightShoulder });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsPlacementMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);

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
        public IEnumerator KeyboardLoadsDrivesBlocksAndUnloadsTheSameLargeBox()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(LargeBoxId);
            TransportCartProjection cart = FindTransportCart();
            VisibleHandsPresenter hands = marker.PlayerMotor.GetComponentInChildren<VisibleHandsPresenter>(true);
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToLargeBox(marker);
            string identity = item.ItemIdValue;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToCartHandle(marker, cart);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedCart, Is.SameAs(cart));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("üzerine yükle"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(cart.Cargo, Is.SameAs(item));
            Assert.That(item.IsMountedOnTransportCart, Is.True);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.Body.isKinematic, Is.True);
            Assert.That(item.Body.detectCollisions, Is.False);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.ActiveCart, Is.SameAs(cart));
            Assert.That(cart.IsDriven, Is.True);
            Assert.That(marker.PlayerMotor.IsDrivingTransportCart, Is.True);
            Assert.That(marker.PlayerMotor.MovementAllowsSprint, Is.False);
            Assert.That(
                marker.PlayerMotor.CurrentHorizontalSpeed,
                Is.EqualTo(3.5f * TransportCartRules.LoadedMovementSpeedMultiplier).Within(0.001f));
            Assert.That(
                marker.PlayerMotor.CurrentHorizontalSpeed,
                Is.GreaterThan(3.5f * PhysicalCarryProfileRules.LargeBoxMovementSpeedMultiplier));
            Assert.That(hands.State, Is.EqualTo(VisibleHandsState.DrivingTransportCart));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("YÜKLÜ"));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.LeftShift));
            InputSystem.Update();
            Vector3 cartStart = cart.transform.position;
            yield return null;
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(cart.transform.position.z, Is.GreaterThan(cartStart.z));
            Assert.That(
                Vector3.Distance(item.transform.position, cart.CargoAnchor.position),
                Is.LessThan(0.001f));
            Vector3 cartSafePosition = cart.transform.position;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "TransportCartPlayModeBlocker";
            blocker.layer = LayerMask.NameToLayer("Interactable");
            blocker.transform.position = cart.transform.position +
                                         (cart.transform.forward * 0.82f) +
                                         (Vector3.up * 0.88f);
            blocker.transform.localScale = new Vector3(1.5f, 1.75f, 0.25f);
            MovePlayerBy(marker, cart.transform.forward * 1.0f);
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.ActiveCart, Is.Null);
            Assert.That(cart.IsDriven, Is.False);
            Assert.That(marker.PlayerMotor.IsDrivingTransportCart, Is.False);
            Assert.That(marker.PlayerCarry.LastFailureCode, Is.EqualTo("cart.drive-blocked"));
            Assert.That(Vector3.Distance(cart.transform.position, cartSafePosition), Is.LessThan(0.001f));
            Assert.That(cart.Cargo, Is.SameAs(item));
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));

            Object.Destroy(blocker);
            yield return null;
            Physics.SyncTransforms();
            MovePlayerToCartHandle(marker, cart);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedCart, Is.SameAs(cart));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("yükünü al"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(cart.HasCargo, Is.False);
            Assert.That(item.IsCarried, Is.True);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(marker.PlayerMotor.ActiveCarryProfile, Is.EqualTo(PhysicalCarryProfile.LargeBox));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerAwayFromCart(marker);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.Body.detectCollisions, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadCartFlowUsesLiveBindingsAndDisabledCartRecoversCargo()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(LargeBoxId);
            TransportCartProjection cart = FindTransportCart();
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToLargeBox(marker);
            string identity = item.ItemIdValue;
            Vector3 itemSafePosition = item.LastSafePosition;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToCartHandle(marker, cart);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain(marker.PlayerInput.InteractBindingPrompt));

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(cart.Cargo, Is.SameAs(item));
            Assert.That(item.IsMountedOnTransportCart, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.ActiveCart, Is.SameAs(cart));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { leftStick = Vector2.up });
            InputSystem.Update();
            Vector3 cartStart = cart.transform.position;
            yield return null;
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(cart.transform.position.z, Is.GreaterThan(cartStart.z));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.ActiveCart, Is.Null);
            Assert.That(cart.IsDriven, Is.False);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToCartHandle(marker, cart);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(cart.HasCargo, Is.False);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(cart.Cargo, Is.SameAs(item));

            cart.gameObject.SetActive(false);
            Assert.That(cart.HasCargo, Is.False);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(Vector3.Distance(item.transform.position, itemSafePosition), Is.LessThan(0.001f));
            Assert.That(item.Body.detectCollisions, Is.True);
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

        [UnityTest]
        public IEnumerator DisabledCustomerAgentUsesExactlyTwoAttemptsPerRouteThenDespawnsSafely()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            Assert.That(customerFlow, Is.Not.Null);
            Assert.That(customerFlow.NavigationReady, Is.True);
            yield return WaitForCustomerRoute(customerFlow);
            long inventoryRevision = session.Inventory.Revision;
            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;

            customerFlow.CustomerAgent.enabled = false;
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Exited);

            CustomerVisitRecord visit = customerFlow.CurrentVisit;
            Assert.That(visit.ExitReason, Is.EqualTo(CustomerVisitExitReason.RouteUnavailable));
            Assert.That(visit.RouteFallbackUsed, Is.True);
            Assert.That(visit.RouteFailureCount, Is.EqualTo(2));
            Assert.That(visit.TotalRouteFailureCount, Is.EqualTo(4));
            Assert.That(customerFlow.CustomerVisible, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(session.RetailCheckouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardAcceptsExactDeliveryThenPlacesSameAuthoritativeItemOnShelf()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection item = FindPhysicalItem(DeliveryItemId);
            GarageStockFlowRuntime stockFlow = marker.StockFlow;
            InventoryItemWorldBinding binding = stockFlow.ItemBinding;
            DeliveryParcelProjection parcel = binding.Parcel;
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToAuthoritativeDelivery(marker);
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(stockFlow, Is.Not.Null);
            Assert.That(stockFlow.Session.Order.Status, Is.EqualTo(PurchaseOrderStatus.Arrived));
            Assert.That(stockFlow.Session.TryGetItem(out _), Is.False);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("teslimatını kabul et"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            Assert.That(parcel.IsSealed, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(stockFlow.Session.Order.Status, Is.EqualTo(PurchaseOrderStatus.Accepted));
            AssertInventoryLocation(stockFlow, stockFlow.Session.ReceivingContainerId);
            Assert.That(binding.LocationLabel, Does.Contain("KOLİ KAPALI"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("kolisini aç"));

            long inventoryRevisionBeforeOpen = stockFlow.Session.Inventory.Revision;
            long orderRevisionBeforeOpen = stockFlow.Session.Orders.Revision;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(parcel.IsOpened, Is.True);
            Assert.That(parcel.OpenTransitionCount, Is.EqualTo(1));
            Assert.That(parcel.ProductVisualRoot.activeSelf, Is.True);
            Assert.That(parcel.OpenedShellVisualRoot.activeSelf, Is.True);
            Assert.That(stockFlow.Session.Inventory.Revision, Is.EqualTo(inventoryRevisionBeforeOpen));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevisionBeforeOpen));
            AssertInventoryLocation(stockFlow, stockFlow.Session.ReceivingContainerId);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain(" al"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            Assert.That(item.IsCarried, Is.True);
            Assert.That(item.ItemIdValue, Is.EqualTo(binding.InventoryItemId.Value));
            AssertInventoryLocation(stockFlow, stockFlow.Session.HandsContainerId);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("OYUNCU ELİNDE"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToAuthoritativeShelf(marker);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsPlacementMode, Is.True);
            Assert.That(
                marker.PlayerCarry.PlacementValid,
                Is.True,
                $"status={marker.PlayerCarry.CurrentPlacementStatus} " +
                $"failure={marker.PlayerCarry.LastFailureCode}");
            Assert.That(marker.PlayerCarry.CurrentStackSupport, Is.Null);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("GEÇERLİ"));

            Pose shelfPose = marker.PlayerCarry.PlacementPreview.CurrentPose;
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.Body.isKinematic, Is.True);
            Assert.That(Vector3.Distance(item.transform.position, shelfPose.position), Is.LessThan(0.001f));
            AssertInventoryLocation(stockFlow, stockFlow.Session.ShelfContainerId);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.StatusText, Does.Contain("RAF A"));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.Zero);
            Assert.That(stockFlow.ShelfOfferText.text,
                Is.EqualTo("RAF A\nFİYAT YOK\nMÜŞTERİ: BOŞ\nKASA: BEKLİYOR"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return new WaitForFixedUpdate();
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("fiyatını yayınla"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(GarageStockFlowRuntime.PrototypePriceText));
            long inventoryRevisionBeforeOffer = stockFlow.Session.Inventory.Revision;
            long orderRevisionBeforeOffer = stockFlow.Session.Orders.Revision;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            AssertInventoryLocation(stockFlow, stockFlow.Session.ShelfContainerId);
            Assert.That(stockFlow.Session.TryGetShelfOffer(out var offer), Is.True);
            Assert.That(offer.Id, Is.EqualTo(stockFlow.Session.ShelfOfferId));
            Assert.That(offer.Price.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeOffer));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevisionBeforeOffer));
            Assert.That(stockFlow.ShelfOfferText.text,
                Is.EqualTo(
                    $"RAF A\n{GarageStockFlowRuntime.PrototypePriceText}\n" +
                    "MÜŞTERİ: BOŞ\nKASA: BEKLİYOR"));
            Assert.That(stockFlow.StatusText,
                Does.Contain($"FİYAT: {GarageStockFlowRuntime.PrototypePriceText}"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("demo müşteri için ayır"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.DropBindingPrompt));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            Assert.That(customerFlow, Is.Not.Null);
            Assert.That(customerFlow.NavigationReady, Is.True);
            yield return new WaitForFixedUpdate();
            Assert.That(customerFlow.VisitStarted, Is.True);
            Assert.That(customerFlow.CustomerVisible, Is.True);
            Assert.That(customerFlow.CurrentVisit.State, Is.EqualTo(CustomerVisitState.Entering));
            yield return WaitForCustomerRoute(customerFlow);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            InputSystem.Update();
            marker.PlayerMotor.ProcessInputFrame();
            Assert.That(marker.PlayerMotor.IsPaused, Is.True);
            yield return new WaitForFixedUpdate();
            Assert.That(customerFlow.CustomerAgent.isStopped, Is.True);
            yield return null;
            Vector3 pausedCustomerPosition = customerFlow.CustomerAgent.transform.position;
            CustomerVisitRecord pausedVisit = customerFlow.CurrentVisit;
            SimulationTimestamp pausedSimulationTime = customerFlow.CurrentSimulationTime;
            for (int step = 0; step < 5; step++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.That(Vector3.Distance(
                customerFlow.CustomerAgent.transform.position,
                pausedCustomerPosition), Is.LessThan(0.001f));
            Assert.That(customerFlow.CurrentVisit.LastUpdatedAt,
                Is.EqualTo(pausedVisit.LastUpdatedAt));
            Assert.That(customerFlow.CurrentSimulationTime,
                Is.EqualTo(pausedSimulationTime));
            Assert.That(customerFlow.CustomerAgent.isStopped, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            InputSystem.Update();
            marker.PlayerMotor.ProcessInputFrame();
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            SimulationTimestamp resumedFrom = customerFlow.CurrentSimulationTime;
            yield return new WaitForFixedUpdate();
            Assert.That(customerFlow.CurrentSimulationTime.Tick,
                Is.EqualTo(resumedFrom.Tick + 1));
            Assert.That(customerFlow.CurrentSimulationTime.ElapsedMilliseconds,
                Is.EqualTo(resumedFrom.ElapsedMilliseconds + 20));
            Assert.That(customerFlow.CustomerAgent.isStopped, Is.False);
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            Assert.That(customerFlow.StateText, Does.Contain("RAF ÜRÜNÜNÜ İNCELİYOR"));
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            long inventoryRevisionBeforeReservation = stockFlow.Session.Inventory.Revision;
            long basketRevisionBeforeReservation = stockFlow.Session.RetailBaskets.Revision;
            long retailOfferRevisionBeforeReservation = stockFlow.Session.RetailOffers.Revision;
            long orderRevisionBeforeReservation = stockFlow.Session.Orders.Revision;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(binding.IsCustomerReserved, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(retailOfferRevisionBeforeReservation));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeReservation));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain("MÜŞTERİ: 1 ÜRÜN • AYRILDI"));
            Assert.That(stockFlow.StatusText, Does.Contain("SEPET: 1 ÜRÜN • AYRILDI"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("müşteri rezervasyonunu kaldır"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(StockProjectionFailures.CustomerReserved.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            AssertInventoryLocation(stockFlow, stockFlow.Session.ShelfContainerId);
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeReservation + 1));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(binding.IsCustomerReserved, Is.False);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeReservation + 2));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeReservation + 2));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.ShelfOfferText.text, Does.Contain("MÜŞTERİ: BOŞ"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(binding.RequiresCheckoutStart, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("kasayı başlat"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return WaitForCustomerState(
                customerFlow,
                CustomerVisitState.AwaitingCheckout);
            Assert.That(customerFlow.StateText, Does.Contain("KASADA BEKLİYOR"));
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            long inventoryRevisionBeforeCheckout = stockFlow.Session.Inventory.Revision;
            long basketRevisionBeforeCheckout = stockFlow.Session.RetailBaskets.Revision;
            long offerRevisionBeforeCheckout = stockFlow.Session.RetailOffers.Revision;
            long orderRevisionBeforeCheckout = stockFlow.Session.Orders.Revision;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(binding.IsCheckoutStarted, Is.True);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision, Is.EqualTo(1));
            Assert.That(stockFlow.Session.TryGetPrototypeCheckout(out var checkout), Is.True);
            Assert.That(checkout.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(checkout.Lines.Count, Is.EqualTo(1));
            Assert.That(checkout.Lines[0].ItemId, Is.EqualTo(stockFlow.Session.ItemId));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(offerRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeCheckout));
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain($"KASA: {GarageStockFlowRuntime.PrototypePriceText} • DONDURULDU"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("satışı tamamla"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(StockProjectionFailures.CheckoutActive.Code));
            Assert.That(binding.IsCustomerReserved, Is.True);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision, Is.EqualTo(1));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(binding.IsCheckoutCompleted, Is.True);
            Assert.That(binding.IsCustomerReserved, Is.False);
            Assert.That(item.gameObject.activeSelf, Is.False);
            Assert.That(stockFlow.Session.TryGetItem(out _), Is.False);
            Assert.That(stockFlow.Session.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision, Is.EqualTo(2));
            Assert.That(stockFlow.Session.RetailCheckouts.CompletionCount, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(offerRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.TryGetPrototypeCheckoutCompletion(
                out RetailCheckoutCompletionRecord completion), Is.True);
            Assert.That(completion.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain($"KASA: {GarageStockFlowRuntime.PrototypePriceText} • TAMAMLANDI"));
            Assert.That(stockFlow.StatusText,
                Does.Contain("MÜŞTERİYE TESLİM EDİLDİ • STOK 0"));
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Exited);
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.Fulfilled));
            Assert.That(customerFlow.CurrentVisit.RouteFailureCount, Is.Zero);
            Assert.That(customerFlow.CurrentVisit.TotalRouteFailureCount, Is.Zero);
            Assert.That(customerFlow.CurrentVisit.RouteFallbackUsed, Is.False);
            Assert.That(customerFlow.CustomerVisible, Is.False);
            Assert.That(customerFlow.StateText, Does.Contain("SATIŞ TAMAMLANDI"));
            Assert.That(stockFlow.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadAcceptsCarriesAndSafelyDropsAuthoritativeItemToWorldFloor()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowRuntime stockFlow = marker.StockFlow;
            PhysicalItemProjection item = FindPhysicalItem(DeliveryItemId);
            DeliveryParcelProjection parcel = stockFlow.Parcel;
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToAuthoritativeDelivery(marker);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            AssertInventoryLocation(stockFlow, stockFlow.Session.ReceivingContainerId);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(parcel.IsOpened, Is.True);
            Assert.That(parcel.OpenTransitionCount, Is.EqualTo(1));
            AssertInventoryLocation(stockFlow, stockFlow.Session.ReceivingContainerId);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            AssertInventoryLocation(stockFlow, stockFlow.Session.HandsContainerId);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain(marker.PlayerInput.DropBindingPrompt));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToOpenDropArea(marker);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.ItemIdValue, Is.EqualTo(GarageStockFlowSession.ItemInstanceIdValue));
            AssertInventoryLocation(stockFlow, stockFlow.Session.WorldFloorContainerId);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(stockFlow.Session.ProductId).Value, Is.EqualTo(1));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToWorldItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(item));
            AssertInventoryLocation(stockFlow, stockFlow.Session.HandsContainerId);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToAuthoritativeShelf(marker);
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsPlacementMode, Is.True);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True,
                marker.PlayerCarry.LastFailureCode);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            AssertInventoryLocation(stockFlow, stockFlow.Session.ShelfContainerId);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            yield return new WaitForFixedUpdate();
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("fiyatını yayınla"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            long inventoryRevisionBeforeOffer = stockFlow.Session.Inventory.Revision;
            long orderRevisionBeforeOffer = stockFlow.Session.Orders.Revision;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(stockFlow.Session.TryGetShelfOffer(out var offer), Is.True);
            Assert.That(offer.Price.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(offer.Price.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeOffer));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevisionBeforeOffer));
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain(GarageStockFlowRuntime.PrototypePriceText));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            GarageCustomerFlowRuntime gamepadCustomerFlow = marker.CustomerFlow;
            Assert.That(gamepadCustomerFlow, Is.Not.Null);
            Assert.That(gamepadCustomerFlow.NavigationReady, Is.True);
            yield return WaitForCustomerState(
                gamepadCustomerFlow,
                CustomerVisitState.Browsing);
            Assert.That(gamepadCustomerFlow.VisitStarted, Is.True);
            Assert.That(gamepadCustomerFlow.CustomerVisible, Is.True);
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            long inventoryRevisionBeforeReservation = stockFlow.Session.Inventory.Revision;
            long basketRevisionBeforeReservation = stockFlow.Session.RetailBaskets.Revision;
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(stockFlow.ItemBinding.IsCustomerReserved, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("müşteri rezervasyonunu kaldır"));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(stockFlow.ItemBinding.IsCustomerReserved, Is.False);
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeReservation + 2));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeReservation + 2));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(stockFlow.ItemBinding.RequiresCheckoutStart, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("kasayı başlat"));
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            yield return WaitForCustomerState(
                gamepadCustomerFlow,
                CustomerVisitState.AwaitingCheckout);
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            long inventoryRevisionBeforeCheckout = stockFlow.Session.Inventory.Revision;
            long basketRevisionBeforeCheckout = stockFlow.Session.RetailBaskets.Revision;
            long offerRevisionBeforeCheckout = stockFlow.Session.RetailOffers.Revision;
            long orderRevisionBeforeCheckout = stockFlow.Session.Orders.Revision;

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(stockFlow.ItemBinding.IsCheckoutStarted, Is.True);
            Assert.That(stockFlow.Session.TryGetPrototypeCheckout(out var checkout), Is.True);
            Assert.That(checkout.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(offerRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeCheckout));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("satışı tamamla"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(StockProjectionFailures.CheckoutActive.Code));
            Assert.That(stockFlow.ItemBinding.IsCustomerReserved, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(stockFlow.ItemBinding.IsCheckoutCompleted, Is.True);
            Assert.That(stockFlow.ItemBinding.IsCustomerReserved, Is.False);
            Assert.That(item.gameObject.activeSelf, Is.False);
            Assert.That(stockFlow.Session.TryGetItem(out _), Is.False);
            Assert.That(stockFlow.Session.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision, Is.EqualTo(2));
            Assert.That(stockFlow.Session.RetailCheckouts.CompletionCount, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(offerRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.TryGetPrototypeCheckoutCompletion(
                out RetailCheckoutCompletionRecord completion), Is.True);
            Assert.That(completion.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain("TAMAMLANDI"));
            Assert.That(stockFlow.StatusText,
                Does.Contain("MÜŞTERİYE TESLİM EDİLDİ • STOK 0"));
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            yield return WaitForCustomerState(
                gamepadCustomerFlow,
                CustomerVisitState.Exited);
            Assert.That(gamepadCustomerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.Fulfilled));
            Assert.That(gamepadCustomerFlow.CurrentVisit.RouteFailureCount, Is.Zero);
            Assert.That(gamepadCustomerFlow.CurrentVisit.TotalRouteFailureCount, Is.Zero);
            Assert.That(gamepadCustomerFlow.CurrentVisit.RouteFallbackUsed, Is.False);
            Assert.That(gamepadCustomerFlow.CustomerVisible, Is.False);
            Assert.That(stockFlow.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FullHandsAuthorityRejectsPickupBeforePhysicalOwnershipChanges()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowRuntime stockFlow = marker.StockFlow;
            PhysicalItemProjection item = FindPhysicalItem(DeliveryItemId);
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToAuthoritativeDelivery(marker);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            AssertInventoryLocation(stockFlow, stockFlow.Session.ReceivingContainerId);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(stockFlow.Parcel.IsOpened, Is.True);
            AssertInventoryLocation(stockFlow, stockFlow.Session.ReceivingContainerId);

            Assert.That(stockFlow.Session.Inventory.ReceiveSerializedItem(
                StableId<ItemInstanceIdScope>.Parse("inventory.item.hands-blocker"),
                stockFlow.Session.ProductId,
                stockFlow.Session.HandsContainerId,
                InventoryCondition.New).IsSuccess,
                Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.transform.parent.name, Is.EqualTo("AuthoritativeReceivingBay"));
            AssertInventoryLocation(stockFlow, stockFlow.Session.ReceivingContainerId);
            Assert.That(stockFlow.Session.ValidateInvariants().IsSuccess, Is.True);
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

        private static void MovePlayerToAuthoritativeDelivery(GaragePrototypeMarker marker)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                new Vector3(2.55f, 0.05f, -1.95f),
                Quaternion.Euler(0f, 180f, 0f));
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void MovePlayerToAuthoritativeShelf(GaragePrototypeMarker marker)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                new Vector3(2.32f, 0.05f, 0.55f),
                Quaternion.Euler(0f, 90f, 0f));
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void MovePlayerToOpenDropArea(GaragePrototypeMarker marker)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                new Vector3(0f, 0.05f, -2.50f),
                Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void MovePlayerToWorldItem(
            GaragePrototypeMarker marker,
            PhysicalItemProjection item)
        {
            AimPlayerAtItem(marker, item, -Vector3.forward);
        }

        private static void MovePlayerToShelfItem(
            GaragePrototypeMarker marker,
            PhysicalItemProjection item)
        {
            AimPlayerAtItem(marker, item, -Vector3.right);
        }

        private static void AimPlayerAtItem(
            GaragePrototypeMarker marker,
            PhysicalItemProjection item,
            Vector3 approachDirection)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            Vector3 target = item.Body != null
                ? item.Body.worldCenterOfMass
                : item.transform.position;
            Vector3 playerPosition = target + (approachDirection.normalized * 1.25f);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(horizontalLook, Vector3.up));
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static IEnumerator WaitForCustomerRoute(
            GarageCustomerFlowRuntime customerFlow)
        {
            const int MaximumFixedSteps = 100;
            for (int step = 0; step < MaximumFixedSteps; step++)
            {
                if (customerFlow.HasAssignedRoute &&
                    customerFlow.CustomerAgent != null &&
                    customerFlow.CustomerAgent.isOnNavMesh &&
                    !customerFlow.CustomerAgent.pathPending &&
                    customerFlow.CustomerAgent.remainingDistance >
                    customerFlow.CustomerAgent.stoppingDistance + 0.10f)
                {
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            Assert.Fail(
                $"Customer did not receive a moving route in {MaximumFixedSteps} fixed steps; " +
                $"assigned={customerFlow.HasAssignedRoute} " +
                $"on-navmesh={customerFlow.CustomerAgent != null && customerFlow.CustomerAgent.isOnNavMesh} " +
                $"remaining={customerFlow.CustomerAgent?.remainingDistance.ToString() ?? "missing"}");
        }

        private static IEnumerator WaitForCustomerState(
            GarageCustomerFlowRuntime customerFlow,
            CustomerVisitState expectedState)
        {
            const int MaximumFixedSteps = 650;
            for (int step = 0; step < MaximumFixedSteps; step++)
            {
                CustomerVisitRecord visit = customerFlow.CurrentVisit;
                if (visit != null && visit.State == expectedState)
                {
                    yield break;
                }

                if (visit != null && visit.State == CustomerVisitState.Exited &&
                    expectedState != CustomerVisitState.Exited)
                {
                    Assert.Fail(
                        $"Customer exited before {expectedState}: reason={visit.ExitReason} " +
                        $"fallback={visit.RouteFallbackUsed}");
                }

                yield return new WaitForFixedUpdate();
            }

            CustomerVisitRecord finalVisit = customerFlow.CurrentVisit;
            Assert.Fail(
                $"Customer did not reach {expectedState} in {MaximumFixedSteps} fixed steps; " +
                $"actual={finalVisit?.State.ToString() ?? "missing"} " +
                $"reason={finalVisit?.ExitReason.ToString() ?? "missing"} " +
                $"route-failures={finalVisit?.RouteFailureCount.ToString() ?? "missing"}/" +
                $"{finalVisit?.TotalRouteFailureCount.ToString() ?? "missing"} " +
                $"time={customerFlow.CurrentSimulationTime.ElapsedMilliseconds} " +
                $"nav-ready={customerFlow.NavigationReady} assigned={customerFlow.HasAssignedRoute} " +
                $"path={customerFlow.CustomerAgent?.pathStatus.ToString() ?? "missing"} " +
                $"remaining={customerFlow.CustomerAgent?.remainingDistance.ToString() ?? "missing"}");
        }

        private static void AssertInventoryLocation(
            GarageStockFlowRuntime stockFlow,
            PCShopEmpire3D.Core.Primitives.StableId<ContainerIdScope> expectedContainer)
        {
            Assert.That(stockFlow.Session.TryGetItem(out InventoryItemRecord record), Is.True);
            Assert.That(record.Id, Is.EqualTo(stockFlow.Session.ItemId));
            Assert.That(record.ContainerId, Is.EqualTo(expectedContainer));
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

        private static void MovePlayerToStackSupport(GaragePrototypeMarker marker)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                new Vector3(1.4f, 0.05f, -2.55f),
                Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static PhysicalItemProjection FindPhysicalItem(string itemId)
        {
            return Object.FindObjectsByType<PhysicalItemProjection>(FindObjectsSortMode.None)
                .Single(item => item.ItemIdValue == itemId);
        }

        private static TransportCartProjection FindTransportCart()
        {
            return Object.FindObjectsByType<TransportCartProjection>(FindObjectsSortMode.None)
                .Single(cart => cart.CartIdValue == TransportCartId);
        }

        private static void MovePlayerToCartHandle(
            GaragePrototypeMarker marker,
            TransportCartProjection cart)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            Vector3 handle = cart.transform.TransformPoint(new Vector3(0f, 0f, -0.60f));
            Vector3 playerPosition = handle - (cart.transform.forward * 1.35f);
            playerPosition.y = 0.05f;
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.Euler(0f, cart.transform.eulerAngles.y, 0f));
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void MovePlayerBy(GaragePrototypeMarker marker, Vector3 delta)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.position += delta;
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void MovePlayerAwayFromCart(GaragePrototypeMarker marker)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                new Vector3(0f, 0.05f, -2.5f),
                Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();
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
