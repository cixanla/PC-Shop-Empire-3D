using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Economy;
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
            Assert.That(
                marker.CustomerFlow.CustomerVisualRoot
                    .GetComponent<CapsuleCollider>().isTrigger,
                Is.True);

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
        public IEnumerator RuntimeInputReconfigurationOwnsOnlyRuntimeClones()
        {
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            InputActionAsset firstSource = Object.Instantiate(marker.PlayerInput.Actions);
            InputActionAsset secondSource = Object.Instantiate(marker.PlayerInput.Actions);
            firstSource.name = "InputReconfigureSourceA";
            secondSource.name = "InputReconfigureSourceB";
            firstSource.Disable();
            secondSource.Disable();

            GameObject adapterObject = new GameObject("RuntimeInputReconfigureProbe");
            PlayerInputAdapter adapter = adapterObject.AddComponent<PlayerInputAdapter>();
            adapter.Configure(firstSource);
            InputActionAsset firstRuntimeClone = adapter.Actions;
            Assert.That(firstRuntimeClone, Is.Not.SameAs(firstSource));
            Assert.That(
                firstRuntimeClone.FindActionMap(PlayerInputContract.PlayerMap, true).enabled,
                Is.True);
            Assert.That(
                firstSource.FindActionMap(PlayerInputContract.PlayerMap, true).enabled,
                Is.False);

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            Assert.That(adapter.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(adapter.TryConsumePrimaryActionPressThisFrame(), Is.True);
            Assert.That(adapter.PrimaryActionPressedThisFrame, Is.False);
            Assert.That(adapter.TryConsumePrimaryActionPressThisFrame(), Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();

            adapter.Configure(secondSource);
            InputActionAsset secondRuntimeClone = adapter.Actions;
            Assert.That(secondRuntimeClone, Is.Not.SameAs(secondSource));
            Assert.That(secondRuntimeClone, Is.Not.SameAs(firstRuntimeClone));
            yield return null;
            Assert.That(firstRuntimeClone == null, Is.True);
            Assert.That(firstSource, Is.Not.Null);
            Assert.That(secondSource, Is.Not.Null);

            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            Assert.That(adapter.TryConsumePrimaryActionPressThisFrame(), Is.True);
            Assert.That(adapter.TryConsumePrimaryActionPressThisFrame(), Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();

            adapterObject.SetActive(false);
            Assert.That(
                secondRuntimeClone.FindActionMap(PlayerInputContract.PlayerMap, true).enabled,
                Is.False);
            adapterObject.SetActive(true);
            Assert.That(
                secondRuntimeClone.FindActionMap(PlayerInputContract.PlayerMap, true).enabled,
                Is.True);

            DefaultExecutionOrder executionOrder =
                (DefaultExecutionOrder)System.Attribute.GetCustomAttribute(
                    typeof(GarageCustomerFlowRuntime),
                    typeof(DefaultExecutionOrder));
            Assert.That(executionOrder, Is.Not.Null);
            Assert.That(executionOrder.order, Is.GreaterThan(0));

            Object.Destroy(adapterObject);
            yield return null;
            Assert.That(secondRuntimeClone == null, Is.True);
            Assert.That(firstSource, Is.Not.Null);
            Assert.That(secondSource, Is.Not.Null);
            Object.Destroy(firstSource);
            Object.Destroy(secondSource);
        }

        [UnityTest]
        public IEnumerator CheckoutStationRejectsInvalidPhysicalAccessWithoutCommerceMutation()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            CheckoutStationProjection checkoutStation = marker.CheckoutStation;
            Assert.That(checkoutStation, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            long[] authorityRevisions = CaptureAuthorityRevisions(session);

            MovePlayerToCheckoutStation(
                marker,
                checkoutStation.InteractionRange + 0.75f);
            OperationResult outOfRange = checkoutStation.TryOperate();
            Assert.That(outOfRange.Error, Is.EqualTo(CheckoutStationFailures.OutOfRange));
            Assert.That(CaptureAuthorityRevisions(session), Is.EqualTo(authorityRevisions));

            MovePlayerToCheckoutStation(marker);
            marker.PlayerMotor.SetPaused(true);
            OperationResult paused = checkoutStation.TryOperate();
            Assert.That(paused.Error, Is.EqualTo(CheckoutStationFailures.Paused));
            Assert.That(CaptureAuthorityRevisions(session), Is.EqualTo(authorityRevisions));
            marker.PlayerMotor.SetPaused(false);
            checkoutStation.RefreshPresentation();

            OperationResult wrongState = checkoutStation.TryOperate();
            Assert.That(
                wrongState.Error,
                Is.EqualTo(CheckoutStationFailures.CustomerNotAwaitingCheckout));
            Assert.That(CaptureAuthorityRevisions(session), Is.EqualTo(authorityRevisions));

            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.rotation *= Quaternion.Euler(0f, 180f, 0f);
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.localRotation = Quaternion.identity;
            controller.enabled = true;
            Physics.SyncTransforms();
            OperationResult focusMissing = checkoutStation.TryOperate();
            Assert.That(focusMissing.Error, Is.EqualTo(CheckoutStationFailures.FocusMissing));
            Assert.That(CaptureAuthorityRevisions(session), Is.EqualTo(authorityRevisions));

            MovePlayerToCheckoutStation(marker);
            Vector3 cameraPosition = checkoutStation.PlayerCamera.transform.position;
            Vector3 target = checkoutStation.InteractionCollider.bounds.center;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "CheckoutStationLosBlocker";
            blocker.transform.position = Vector3.Lerp(cameraPosition, target, 0.5f);
            blocker.transform.localScale = Vector3.one * 0.38f;
            Physics.SyncTransforms();
            OperationResult lineOfSight = checkoutStation.TryOperate();
            Assert.That(
                lineOfSight.Error,
                Is.EqualTo(CheckoutStationFailures.LineOfSightBlocked));
            Assert.That(CaptureAuthorityRevisions(session), Is.EqualTo(authorityRevisions));
            Object.Destroy(blocker);
            yield return null;
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
        public IEnumerator KeyboardMouseCompletesPhysicalFlowAndSettlesExactCash()
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
            CheckoutStationProjection checkoutStation = marker.CheckoutStation;
            marker.PlayerMotor.SetPaused(false);
            MovePlayerToAuthoritativeDelivery(marker);
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(stockFlow, Is.Not.Null);
            Assert.That(checkoutStation, Is.Not.Null);
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
                Does.Contain("RAF A • STOK 1"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Not.Contain(marker.PlayerInput.DropBindingPrompt));

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
            Assert.That(customerFlow.StateText, Does.Contain("YARDIM BEKLİYOR"));
            Assert.That(customerFlow.CurrentOfferDecision, Is.Null);
            Assert.That(stockFlow.Session.CustomerConsultations.Revision, Is.Zero);
            MovePlayerToOpenDropArea(marker);
            customerFlow.RefreshPresentation();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            customerFlow.ProcessInputFrame();
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            MovePlayerToCustomer(marker, customerFlow);
            customerFlow.RefreshPresentation();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.True);
            Vector3 focusTarget = customerFlow.CustomerVisualRoot.transform.position +
                                  (Vector3.up * 1.35f);
            Vector3 cameraPosition = customerFlow.PlayerCamera.transform.position;
            GameObject consultationBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            consultationBlocker.name = "CustomerConsultationLosBlocker";
            consultationBlocker.transform.position = Vector3.Lerp(
                cameraPosition,
                focusTarget,
                0.50f);
            consultationBlocker.transform.localScale = new Vector3(0.55f, 0.75f, 0.18f);
            consultationBlocker.transform.rotation = Quaternion.LookRotation(
                focusTarget - cameraPosition,
                Vector3.up);
            Physics.SyncTransforms();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.False);
            Object.DestroyImmediate(consultationBlocker);
            Physics.SyncTransforms();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.True);
            marker.PlayerMotor.SetPaused(true);
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.False);
            customerFlow.ProcessInputFrame();
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            marker.PlayerMotor.SetPaused(false);
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.True);
            Assert.That(customerFlow.ContextualPromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            Vector3 speechTowardCamera = customerFlow.PlayerCamera.transform.position -
                                         customerFlow.CustomerSpeechText.transform.position;
            speechTowardCamera.y = 0f;
            Assert.That(Vector3.Dot(
                    -customerFlow.CustomerSpeechText.transform.forward,
                    speechTowardCamera.normalized),
                Is.GreaterThan(0.995f));
            Assert.That(customerFlow.CustomerSpeechText.text,
                Does.Contain("İHTİYACI SOR"));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            PhysicalItemProjection overlapItem = CreateConsultationOverlapItem(
                marker,
                customerFlow);
            customerFlow.RefreshPresentation();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.True);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(overlapItem));
            string carryFailureBeforeConsultation = marker.PlayerCarry.LastFailureCode;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            customerFlow.ProcessInputFrame();
            Assert.That(customerFlow.ConsultationCompleted, Is.True);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(overlapItem.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(carryFailureBeforeConsultation));
            Object.DestroyImmediate(overlapItem.gameObject);
            Physics.SyncTransforms();
            Assert.That(stockFlow.Session.CustomerConsultations.Revision, Is.EqualTo(1));
            Assert.That(stockFlow.Session.TryGetPrototypeCustomerConsultation(
                out CustomerConsultationRecord consultation), Is.True);
            Assert.That(consultation.VisitId, Is.EqualTo(customerFlow.CurrentVisit.Id));
            Assert.That(consultation.Need, Is.EqualTo(CustomerNeedKind.GraphicsUpgrade));
            Assert.That(customerFlow.CustomerSpeechText.text,
                Does.Contain("YÜKSELTMEK İSTİYORUM"));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            long actorRevisionBeforeDecision = stockFlow.Session.CustomerVisits.Revision;
            long consultationRevisionBeforeDecision =
                stockFlow.Session.CustomerConsultations.Revision;
            long inventoryRevisionBeforeDecision = stockFlow.Session.Inventory.Revision;
            long orderRevisionBeforeDecision = stockFlow.Session.Orders.Revision;
            long offerRevisionBeforeDecision = stockFlow.Session.RetailOffers.Revision;
            long basketRevisionBeforeDecision = stockFlow.Session.RetailBaskets.Revision;
            long checkoutRevisionBeforeDecision = stockFlow.Session.RetailCheckouts.Revision;
            CustomerOfferDecision decision = customerFlow.CurrentOfferDecision;
            customerFlow.RefreshPresentation();
            Assert.That(decision, Is.Not.Null);
            Assert.That(decision.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Buy));
            Assert.That(decision.ReasonCode,
                Is.EqualTo(CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit));
            Assert.That(decision.MaximumAcceptedPrice.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits));
            Assert.That(customerFlow.StateText, Does.Contain("KARAR: SATIN AL"));
            Assert.That(customerFlow.StatusText, Does.Contain(decision.ReasonCode));
            Assert.That(customerFlow.CustomerStatusText.text, Does.Contain("KARAR: SATIN AL"));
            Assert.That(stockFlow.Session.CustomerVisits.Revision,
                Is.EqualTo(actorRevisionBeforeDecision));
            Assert.That(stockFlow.Session.CustomerConsultations.Revision,
                Is.EqualTo(consultationRevisionBeforeDecision));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeDecision));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(offerRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.RetailCheckouts.Count, Is.Zero);
            Assert.That(customerFlow.CurrentVisit.State, Is.EqualTo(CustomerVisitState.Browsing));
            SimulationTimestamp offerActionTime = customerFlow.CurrentOfferActionTime;
            Assert.That(offerActionTime.IsAtOrAfter(customerFlow.CurrentVisit.LastUpdatedAt),
                Is.True);
            Assert.That(offerActionTime, Is.Not.EqualTo(customerFlow.CurrentVisit.LastUpdatedAt));
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("müşterinin satın almasını onayla"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.DropBindingPrompt));
            long inventoryRevisionBeforeReservation = stockFlow.Session.Inventory.Revision;
            long basketRevisionBeforeReservation = stockFlow.Session.RetailBaskets.Revision;
            long retailOfferRevisionBeforeReservation = stockFlow.Session.RetailOffers.Revision;
            long orderRevisionBeforeReservation = stockFlow.Session.Orders.Revision;
            long actionRevisionBeforeReservation =
                stockFlow.Session.CustomerOfferActions.Revision;
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
            Assert.That(stockFlow.Session.CustomerVisits.Revision,
                Is.EqualTo(actorRevisionBeforeDecision + 1));
            Assert.That(stockFlow.Session.CustomerOfferActions.Revision,
                Is.EqualTo(actionRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.TryGetPrototypeCustomerBuyAction(out _), Is.True);
            Assert.That(binding.IsCustomerReservationActionOwned, Is.True);
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
                Does.Contain("SATIN ALMA ONAYLANDI"));

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

            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(StockProjectionFailures.CustomerReservationActionOwned.Code));
            Assert.That(binding.IsCustomerReserved, Is.True);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain("MÜŞTERİ: 1 ÜRÜN • AYRILDI"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            Assert.That(binding.RequiresCheckoutStart, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("KASA İSTASYONUNA GİT"));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return WaitForCustomerState(
                customerFlow,
                CustomerVisitState.AwaitingCheckout);
            Assert.That(customerFlow.StateText, Does.Contain("KASADA BEKLİYOR"));
            MovePlayerToCheckoutStation(
                marker,
                checkoutStation.InteractionRange + 0.75f);
            checkoutStation.RefreshPresentation();
            Assert.That(checkoutStation.IsFocused, Is.False);
            Assert.That(checkoutStation.HasContextualAttention, Is.True);
            Assert.That(checkoutStation.PromptText,
                Does.Contain(CheckoutStationFailures.OutOfRange.Code));

            MovePlayerToCheckoutStation(marker);
            Vector3 stationCameraPosition = checkoutStation.PlayerCamera.transform.position;
            Vector3 stationTarget = checkoutStation.InteractionCollider.bounds.center;
            GameObject stationBlocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stationBlocker.name = "CheckoutStationVisibleFailureBlocker";
            stationBlocker.transform.position = Vector3.Lerp(
                stationCameraPosition,
                stationTarget,
                0.5f);
            stationBlocker.transform.localScale = Vector3.one * 0.38f;
            Physics.SyncTransforms();
            checkoutStation.RefreshPresentation();
            Assert.That(checkoutStation.IsFocused, Is.False);
            Assert.That(checkoutStation.HasContextualAttention, Is.True);
            Assert.That(checkoutStation.PromptText,
                Does.Contain(CheckoutStationFailures.LineOfSightBlocked.Code));
            Object.Destroy(stationBlocker);
            yield return null;

            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            long inventoryRevisionBeforeCheckout = stockFlow.Session.Inventory.Revision;
            long basketRevisionBeforeCheckout = stockFlow.Session.RetailBaskets.Revision;
            long offerRevisionBeforeCheckout = stockFlow.Session.RetailOffers.Revision;
            long orderRevisionBeforeCheckout = stockFlow.Session.Orders.Revision;
            long economyRevisionBeforeCheckout = stockFlow.Session.CheckoutSettlements.Revision;
            long checkoutRevisionBeforeCheckout = stockFlow.Session.RetailCheckouts.Revision;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(binding.IsCheckoutStarted, Is.False);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("KASA İSTASYONUNA GİT"));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            MovePlayerToCheckoutStation(marker);
            checkoutStation.RefreshPresentation();
            Assert.That(checkoutStation.IsFocused, Is.True);
            Assert.That(checkoutStation.PromptText, Does.Contain("KASAYI BAŞLAT"));
            Assert.That(checkoutStation.PromptText,
                Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            checkoutStation.ProcessInputFrame();
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            checkoutStation.RefreshPresentation();

            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            checkoutStation.ProcessInputFrame();

            Assert.That(binding.IsCheckoutStarted, Is.True);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout + 1));
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
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.CheckoutSettlements.SettlementCount, Is.Zero);
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain($"KASA: {GarageStockFlowRuntime.PrototypePriceText} • ÖDEME BEKLİYOR"));
            Assert.That(checkoutStation.PromptText,
                Does.Contain("NAKİT ÖDEMEYİ AL"));
            Assert.That(checkoutStation.PromptText,
                Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));

            OperationResult sameFrameReplay = checkoutStation.TryOperate();
            Assert.That(sameFrameReplay.Error,
                Is.EqualTo(CheckoutStationFailures.InputReplay));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));

            checkoutStation.ProcessInputFrame();
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));
            InputSystem.Update();
            checkoutStation.ProcessInputFrame();
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));

            yield return null;
            MovePlayerToShelfItem(marker, item);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(StockProjectionFailures.CheckoutActive.Code));
            Assert.That(binding.IsCustomerReserved, Is.True);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout + 1));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToCheckoutStation(marker);
            checkoutStation.RefreshPresentation();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            checkoutStation.ProcessInputFrame();

            Assert.That(binding.IsCheckoutCompleted, Is.True);
            Assert.That(binding.IsCheckoutSettled, Is.True);
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
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.CheckoutSettlements.SettlementCount, Is.EqualTo(1));
            Assert.That(stockFlow.Session.CheckoutSettlements.TransactionCount, Is.EqualTo(1));
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
            AssertPrototypeCashSettlement(stockFlow.Session);
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain($"KASA: {GarageStockFlowRuntime.PrototypePriceText} • NAKİT ALINDI"));
            Assert.That(stockFlow.StatusText,
                Does.Contain("MÜŞTERİYE TESLİM EDİLDİ • STOK 0"));
            Assert.That(stockFlow.StatusText, Does.Contain("MUHASEBE: NAKİT +"));
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
        public IEnumerator GamepadCompletesPhysicalFlowAndSettlesExactCash()
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
            CheckoutStationProjection checkoutStation = marker.CheckoutStation;
            Assert.That(checkoutStation, Is.Not.Null);
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
            Assert.That(gamepadCustomerFlow.CurrentOfferDecision, Is.Null);
            MovePlayerToCustomer(marker, gamepadCustomerFlow);
            gamepadCustomerFlow.RefreshPresentation();
            Assert.That(gamepadCustomerFlow.CanConsultCurrentCustomer, Is.True);
            Assert.That(gamepadCustomerFlow.ContextualPromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            string carryFailureBeforeConsultation = marker.PlayerCarry.LastFailureCode;
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            gamepadCustomerFlow.ProcessInputFrame();
            Assert.That(gamepadCustomerFlow.ConsultationCompleted, Is.True);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(carryFailureBeforeConsultation));
            Assert.That(stockFlow.Session.CustomerConsultations.Revision, Is.EqualTo(1));
            Assert.That(gamepadCustomerFlow.CustomerSpeechText.text,
                Does.Contain("YÜKSELTMEK İSTİYORUM"));
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            long actorRevisionBeforeDecision = stockFlow.Session.CustomerVisits.Revision;
            long consultationRevisionBeforeDecision =
                stockFlow.Session.CustomerConsultations.Revision;
            long inventoryRevisionBeforeDecision = stockFlow.Session.Inventory.Revision;
            long orderRevisionBeforeDecision = stockFlow.Session.Orders.Revision;
            long offerRevisionBeforeDecision = stockFlow.Session.RetailOffers.Revision;
            long basketRevisionBeforeDecision = stockFlow.Session.RetailBaskets.Revision;
            long checkoutRevisionBeforeDecision = stockFlow.Session.RetailCheckouts.Revision;
            CustomerOfferDecision decision = gamepadCustomerFlow.CurrentOfferDecision;
            gamepadCustomerFlow.RefreshPresentation();
            Assert.That(decision, Is.Not.Null);
            Assert.That(decision.DecisionKind, Is.EqualTo(CustomerOfferDecisionKind.Buy));
            Assert.That(decision.ReasonCode,
                Is.EqualTo(CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit));
            Assert.That(gamepadCustomerFlow.StateText, Does.Contain("KARAR: SATIN AL"));
            Assert.That(gamepadCustomerFlow.CustomerStatusText.text,
                Does.Contain(decision.ReasonCode));
            Assert.That(stockFlow.Session.CustomerVisits.Revision,
                Is.EqualTo(actorRevisionBeforeDecision));
            Assert.That(stockFlow.Session.CustomerConsultations.Revision,
                Is.EqualTo(consultationRevisionBeforeDecision));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeDecision));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(offerRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeDecision));
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.RetailCheckouts.Count, Is.Zero);
            Assert.That(gamepadCustomerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Browsing));
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            long inventoryRevisionBeforeReservation = stockFlow.Session.Inventory.Revision;
            long basketRevisionBeforeReservation = stockFlow.Session.RetailBaskets.Revision;
            long actionRevisionBeforeReservation =
                stockFlow.Session.CustomerOfferActions.Revision;
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
            Assert.That(stockFlow.Session.CustomerVisits.Revision,
                Is.EqualTo(actorRevisionBeforeDecision + 1));
            Assert.That(stockFlow.Session.CustomerOfferActions.Revision,
                Is.EqualTo(actionRevisionBeforeReservation + 1));
            Assert.That(stockFlow.ItemBinding.IsCustomerReservationActionOwned, Is.True);
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("SATIN ALMA ONAYLANDI"));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(StockProjectionFailures.CustomerReservationActionOwned.Code));
            Assert.That(stockFlow.ItemBinding.IsCustomerReserved, Is.True);
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeReservation + 1));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.Zero);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            Assert.That(stockFlow.ItemBinding.RequiresCheckoutStart, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("KASA İSTASYONUNA GİT"));
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
            long economyRevisionBeforeCheckout = stockFlow.Session.CheckoutSettlements.Revision;
            long checkoutRevisionBeforeCheckout = stockFlow.Session.RetailCheckouts.Revision;

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(stockFlow.ItemBinding.IsCheckoutStarted, Is.False);
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToCheckoutStation(marker);
            checkoutStation.RefreshPresentation();
            Assert.That(checkoutStation.IsFocused, Is.True);
            Assert.That(checkoutStation.PromptText, Does.Contain("KASAYI BAŞLAT"));
            Assert.That(checkoutStation.PromptText,
                Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));

            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            checkoutStation.ProcessInputFrame();

            Assert.That(stockFlow.ItemBinding.IsCheckoutStarted, Is.True);
            Assert.That(stockFlow.Session.TryGetPrototypeCheckout(out var checkout), Is.True);
            Assert.That(checkout.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.RetailOffers.Revision,
                Is.EqualTo(offerRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.Orders.Revision,
                Is.EqualTo(orderRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));
            Assert.That(stockFlow.Session.CheckoutSettlements.SettlementCount, Is.Zero);
            Assert.That(checkoutStation.PromptText,
                Does.Contain("NAKİT ÖDEMEYİ AL"));
            Assert.That(checkoutStation.PromptText,
                Does.Contain(marker.PlayerInput.PrimaryBindingPrompt));

            checkoutStation.ProcessInputFrame();
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));
            InputSystem.Update();
            checkoutStation.ProcessInputFrame();
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout));

            yield return null;
            MovePlayerToShelfItem(marker, item);
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
            MovePlayerToCheckoutStation(marker);
            checkoutStation.RefreshPresentation();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            checkoutStation.ProcessInputFrame();

            Assert.That(stockFlow.ItemBinding.IsCheckoutCompleted, Is.True);
            Assert.That(stockFlow.ItemBinding.IsCheckoutSettled, Is.True);
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
            Assert.That(stockFlow.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevisionBeforeCheckout + 1));
            Assert.That(stockFlow.Session.CheckoutSettlements.SettlementCount, Is.EqualTo(1));
            Assert.That(stockFlow.Session.CheckoutSettlements.TransactionCount, Is.EqualTo(1));
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
            AssertPrototypeCashSettlement(stockFlow.Session);
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain("NAKİT ALINDI"));
            Assert.That(stockFlow.StatusText,
                Does.Contain("MÜŞTERİYE TESLİM EDİLDİ • STOK 0"));
            Assert.That(stockFlow.StatusText, Does.Contain("MUHASEBE: NAKİT +"));
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
        public IEnumerator KeyboardDisplayedBuyRejectsStaleOfferWithoutAuthorityMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            GarageStockFlowRuntime stockFlow = marker.StockFlow;
            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            InventoryItemWorldBinding binding = stockFlow.ItemBinding;
            PhysicalItemProjection item = FindPhysicalItem(DeliveryItemId);
            marker.PlayerMotor.SetPaused(false);

            Assert.That(stockFlow.Session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(stockFlow.Session.TransferItem(
                stockFlow.Session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(stockFlow.Session.PublishShelfOffer().IsSuccess, Is.True);
            stockFlow.RefreshPresentation();
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            ConsultCurrentCustomerDirectly(customerFlow);

            CustomerOfferDecision displayedDecision = customerFlow.CurrentOfferDecision;
            Assert.That(displayedDecision, Is.Not.Null);
            Assert.That(displayedDecision.DecisionKind,
                Is.EqualTo(CustomerOfferDecisionKind.Buy));
            Assert.That(stockFlow.Session.RetailOffers.SetOffer(
                stockFlow.Session.ShelfOfferId,
                stockFlow.Session.ProductId,
                stockFlow.Session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits + 1).IsSuccess, Is.True);

            long actorRevision = stockFlow.Session.CustomerVisits.Revision;
            long inventoryRevision = stockFlow.Session.Inventory.Revision;
            long basketRevision = stockFlow.Session.RetailBaskets.Revision;
            long actionRevision = stockFlow.Session.CustomerOfferActions.Revision;
            long offerRevision = stockFlow.Session.RetailOffers.Revision;
            long checkoutRevision = stockFlow.Session.RetailCheckouts.Revision;
            long orderRevision = stockFlow.Session.Orders.Revision;
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            Assert.That(binding.RequiresCustomerReservation, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(CustomerOfferDecisionActionFailures.DecisionStale.Code));
            Assert.That(binding.IsCustomerReserved, Is.False);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.CustomerOfferActions.Count, Is.Zero);
            Assert.That(stockFlow.Session.CustomerVisits.Revision, Is.EqualTo(actorRevision));
            Assert.That(stockFlow.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(stockFlow.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(stockFlow.Session.CustomerOfferActions.Revision,
                Is.EqualTo(actionRevision));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevision));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(customerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Browsing));
            Assert.That(customerFlow.StatusText,
                Does.Contain(CustomerOfferDecisionActionFailures.DecisionStale.Code));
            Assert.That(customerFlow.CustomerStatusText.text,
                Does.Contain("SATIN ALMA ENGELLİ"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(CustomerOfferDecisionActionFailures.DecisionStale.Code));
            Assert.That(stockFlow.Session.ValidateInvariants().IsSuccess, Is.True);

            CustomerVisitRecord staleVisit = customerFlow.CurrentVisit;
            customerFlow.enabled = false;
            Assert.That(stockFlow.Session.AdvanceCustomerTime(
                staleVisit.StateDeadline).IsSuccess, Is.True);
            customerFlow.RefreshPresentation();
            Assert.That(customerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Exiting));
            Assert.That(customerFlow.LastOfferActionFailureCode, Is.Empty);
            Assert.That(customerFlow.StatusText,
                Does.Not.Contain("SATIN ALMA ENGELLİ"));
        }

        [UnityTest]
        public IEnumerator KeyboardDisplayedLeaveSendsCustomerOutWithoutCommerceMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            GarageStockFlowRuntime stockFlow = marker.StockFlow;
            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            InventoryItemWorldBinding binding = stockFlow.ItemBinding;
            PhysicalItemProjection item = FindPhysicalItem(DeliveryItemId);
            marker.PlayerMotor.SetPaused(false);

            Assert.That(stockFlow.Session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(stockFlow.Session.TransferItem(
                stockFlow.Session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(stockFlow.Session.PublishShelfOffer().IsSuccess, Is.True);
            Assert.That(stockFlow.Session.RetailOffers.SetOffer(
                stockFlow.Session.ShelfOfferId,
                stockFlow.Session.ProductId,
                stockFlow.Session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1).IsSuccess,
                Is.True);
            stockFlow.RefreshPresentation();
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            ConsultCurrentCustomerDirectly(customerFlow);

            CustomerOfferDecision displayedDecision = customerFlow.CurrentOfferDecision;
            customerFlow.RefreshPresentation();
            Assert.That(displayedDecision, Is.Not.Null);
            Assert.That(displayedDecision.DecisionKind,
                Is.EqualTo(CustomerOfferDecisionKind.Leave));
            Assert.That(customerFlow.StateText, Does.Contain("KARAR: AYRIL"));

            long actorRevision = stockFlow.Session.CustomerVisits.Revision;
            long inventoryRevision = stockFlow.Session.Inventory.Revision;
            long basketRevision = stockFlow.Session.RetailBaskets.Revision;
            long actionRevision = stockFlow.Session.CustomerOfferActions.Revision;
            long offerRevision = stockFlow.Session.RetailOffers.Revision;
            long checkoutRevision = stockFlow.Session.RetailCheckouts.Revision;
            long orderRevision = stockFlow.Session.Orders.Revision;
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(item));
            Assert.That(binding.RequiresCustomerDeparture, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("müşteriyi teklifi reddederek uğurla"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.DropBindingPrompt));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode, Is.Empty);
            Assert.That(binding.IsCustomerReserved, Is.False);
            Assert.That(stockFlow.Session.CustomerOfferActions.Revision,
                Is.EqualTo(actionRevision + 1));
            Assert.That(stockFlow.Session.CustomerVisits.Revision,
                Is.EqualTo(actorRevision + 1));
            Assert.That(stockFlow.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(stockFlow.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevision));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.TryGetPrototypeCustomerLeaveAction(
                out CustomerOfferDecisionActionRecord action), Is.True);
            Assert.That(action.IsLeave, Is.True);
            Assert.That(action.HasReservation, Is.False);
            Assert.That(customerFlow.CurrentVisit.State, Is.EqualTo(CustomerVisitState.Exiting));
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.OfferDeclined));
            Assert.That(customerFlow.StatusText, Does.Contain("TEKLİF REDDEDİLDİ"));
            Assert.That(customerFlow.CustomerStatusText.text,
                Does.Contain("TEKLİF REDDEDİLDİ"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Exited);
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.OfferDeclined));
            Assert.That(customerFlow.CustomerVisible, Is.False);
            Assert.That(stockFlow.Session.CustomerOfferActions.Revision,
                Is.EqualTo(actionRevision + 1));
            Assert.That(stockFlow.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(stockFlow.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevision));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(stockFlow.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadDisplayedLeaveUsesEastAndPreservesShelfStock()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            GarageStockFlowRuntime stockFlow = marker.StockFlow;
            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            PhysicalItemProjection item = FindPhysicalItem(DeliveryItemId);
            marker.PlayerMotor.SetPaused(false);

            Assert.That(stockFlow.Session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(stockFlow.Session.TransferItem(
                stockFlow.Session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(stockFlow.Session.PublishShelfOffer().IsSuccess, Is.True);
            Assert.That(stockFlow.Session.RetailOffers.SetOffer(
                stockFlow.Session.ShelfOfferId,
                stockFlow.Session.ProductId,
                stockFlow.Session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1).IsSuccess,
                Is.True);
            stockFlow.RefreshPresentation();
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            ConsultCurrentCustomerDirectly(customerFlow);
            Assert.That(customerFlow.CurrentOfferDecision.DecisionKind,
                Is.EqualTo(CustomerOfferDecisionKind.Leave));

            long actorRevision = stockFlow.Session.CustomerVisits.Revision;
            long inventoryRevision = stockFlow.Session.Inventory.Revision;
            long basketRevision = stockFlow.Session.RetailBaskets.Revision;
            long actionRevision = stockFlow.Session.CustomerOfferActions.Revision;
            long offerRevision = stockFlow.Session.RetailOffers.Revision;
            long checkoutRevision = stockFlow.Session.RetailCheckouts.Revision;
            long orderRevision = stockFlow.Session.Orders.Revision;
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.DropBindingPrompt));

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode, Is.Empty);
            Assert.That(stockFlow.Session.CustomerOfferActions.Revision,
                Is.EqualTo(actionRevision + 1));
            Assert.That(stockFlow.Session.CustomerVisits.Revision,
                Is.EqualTo(actorRevision + 1));
            Assert.That(stockFlow.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(stockFlow.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevision));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.TryGetPrototypeCustomerLeaveAction(out _), Is.True);
            Assert.That(customerFlow.CurrentVisit.State, Is.EqualTo(CustomerVisitState.Exiting));
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.OfferDeclined));
            Assert.That(stockFlow.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardDisplayedLeaveRejectsStaleOfferWithoutAuthorityMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync("GarageGraybox", LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            GarageStockFlowRuntime stockFlow = marker.StockFlow;
            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            InventoryItemWorldBinding binding = stockFlow.ItemBinding;
            PhysicalItemProjection item = FindPhysicalItem(DeliveryItemId);
            marker.PlayerMotor.SetPaused(false);

            Assert.That(stockFlow.Session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(stockFlow.Session.TransferItem(
                stockFlow.Session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(stockFlow.Session.PublishShelfOffer().IsSuccess, Is.True);
            Assert.That(stockFlow.Session.RetailOffers.SetOffer(
                stockFlow.Session.ShelfOfferId,
                stockFlow.Session.ProductId,
                stockFlow.Session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeMaximumAcceptedPriceMinorUnits + 1).IsSuccess,
                Is.True);
            stockFlow.RefreshPresentation();
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            ConsultCurrentCustomerDirectly(customerFlow);

            CustomerOfferDecision displayedDecision = customerFlow.CurrentOfferDecision;
            Assert.That(displayedDecision, Is.Not.Null);
            Assert.That(displayedDecision.DecisionKind,
                Is.EqualTo(CustomerOfferDecisionKind.Leave));
            Assert.That(stockFlow.Session.RetailOffers.SetOffer(
                stockFlow.Session.ShelfOfferId,
                stockFlow.Session.ProductId,
                stockFlow.Session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits).IsSuccess,
                Is.True);

            long actorRevision = stockFlow.Session.CustomerVisits.Revision;
            long inventoryRevision = stockFlow.Session.Inventory.Revision;
            long basketRevision = stockFlow.Session.RetailBaskets.Revision;
            long actionRevision = stockFlow.Session.CustomerOfferActions.Revision;
            long offerRevision = stockFlow.Session.RetailOffers.Revision;
            long checkoutRevision = stockFlow.Session.RetailCheckouts.Revision;
            long orderRevision = stockFlow.Session.Orders.Revision;
            MovePlayerToShelfItem(marker, item);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(binding.RequiresCustomerDeparture, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(CustomerOfferDecisionActionFailures.DecisionStale.Code));
            Assert.That(stockFlow.Session.CustomerOfferActions.Count, Is.Zero);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.CustomerOfferActions.Revision,
                Is.EqualTo(actionRevision));
            Assert.That(stockFlow.Session.CustomerVisits.Revision, Is.EqualTo(actorRevision));
            Assert.That(stockFlow.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(stockFlow.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(stockFlow.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(stockFlow.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevision));
            Assert.That(stockFlow.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(customerFlow.CurrentVisit.State, Is.EqualTo(CustomerVisitState.Browsing));
            Assert.That(customerFlow.LastOfferActionFailureCode,
                Is.EqualTo(CustomerOfferDecisionActionFailures.DecisionStale.Code));
            Assert.That(customerFlow.CustomerStatusText.text,
                Does.Contain("AYRILMA ENGELLİ"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("AYRILMA ENGELLİ"));
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
                InventoryCondition.New,
                InventoryUnitCost.Create(
                    GarageStockFlowSession.PrototypeCurrencyCode,
                    GarageStockFlowSession.PrototypeUnitCostMinorUnits).Value).IsSuccess,
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

        private static void MovePlayerToCheckoutStation(
            GaragePrototypeMarker marker,
            float distance = 1.45f)
        {
            CheckoutStationProjection checkoutStation = marker.CheckoutStation;
            Collider targetCollider = checkoutStation.InteractionCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 approach = -targetCollider.transform.forward;
            approach.y = 0f;
            approach.Normalize();
            Vector3 playerPosition = target + (approach * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;

            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
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

        private static long[] CaptureAuthorityRevisions(GarageStockFlowSession session)
        {
            return new[]
            {
                session.CustomerVisits.Revision,
                session.CustomerConsultations.Revision,
                session.CustomerOfferActions.Revision,
                session.Inventory.Revision,
                session.Orders.Revision,
                session.RetailOffers.Revision,
                session.RetailBaskets.Revision,
                session.RetailCheckouts.Revision,
                session.CheckoutSettlements.Revision
            };
        }

        private static void MovePlayerToCustomer(
            GaragePrototypeMarker marker,
            GarageCustomerFlowRuntime customerFlow)
        {
            CharacterController controller = marker.PlayerMotor.GetComponent<CharacterController>();
            Vector3 target = customerFlow.CustomerVisualRoot.transform.position +
                             (Vector3.up * 1.35f);
            Vector3 playerPosition = target - (Vector3.right * 1.55f);
            playerPosition.y = 0.05f;
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(Vector3.right, Vector3.up));
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static PhysicalItemProjection CreateConsultationOverlapItem(
            GaragePrototypeMarker marker,
            GarageCustomerFlowRuntime customerFlow)
        {
            Vector3 target = customerFlow.CustomerVisualRoot.transform.position +
                             (Vector3.up * 1.35f);
            Vector3 cameraPosition = customerFlow.PlayerCamera.transform.position;
            Vector3 direct = (target - cameraPosition).normalized;
            Vector3 itemDirection = Quaternion.AngleAxis(15f, Vector3.up) * direct;
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(itemDirection, Vector3.up);

            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.name = "ConsultationInteractConsumptionItem";
            itemObject.layer = LayerMask.NameToLayer("Interactable");
            itemObject.transform.position = cameraPosition + (itemDirection * 1.05f);
            itemObject.transform.localScale = Vector3.one * 0.18f;
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "physical-item.consultation-consumption-test",
                "Consultation Consumption Test Item",
                body,
                Vector3.one * 0.09f,
                Vector3.zero,
                Vector3.zero);
            Physics.SyncTransforms();
            return item;
        }

        private static void ConsultCurrentCustomerDirectly(
            GarageCustomerFlowRuntime customerFlow)
        {
            GarageStockFlowSession session = customerFlow.StockFlow.EnsureInitialized();
            OperationResult result = session.ConsultPrototypeCustomer(
                customerFlow.CurrentConsultationTime);
            Assert.That(result.IsSuccess, Is.True,
                result.IsFailure ? result.Error.Code : string.Empty);
            Assert.That(session.TryGetPrototypeCustomerConsultation(out _), Is.True);
            customerFlow.RefreshPresentation();
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

        private static void AssertPrototypeCashSettlement(GarageStockFlowSession session)
        {
            Assert.That(session.TryGetPrototypeCheckoutSettlement(
                out CheckoutSettlementReceipt receipt), Is.True);
            Assert.That(receipt.Id, Is.EqualTo(session.PrototypeCheckoutSettlementId));
            Assert.That(receipt.TransactionId, Is.EqualTo(session.PrototypeLedgerTransactionId));
            Assert.That(receipt.CompletionId, Is.EqualTo(session.PrototypeCheckoutCompletionId));
            Assert.That(receipt.CheckoutId, Is.EqualTo(session.PrototypeCheckoutId));
            Assert.That(receipt.CustomerId, Is.EqualTo(session.PrototypeCustomerId));
            Assert.That(receipt.PaymentMethod, Is.EqualTo(CheckoutPaymentMethod.Cash));
            Assert.That(receipt.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(receipt.GrossMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(receipt.CostOfGoodsSoldMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(receipt.GrossMarginMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits -
                           GarageStockFlowSession.PrototypeUnitCostMinorUnits));

            Assert.That(session.TryGetPrototypeLedgerTransaction(
                out EconomyLedgerTransactionRecord transaction), Is.True);
            Assert.That(transaction.Id, Is.EqualTo(session.PrototypeLedgerTransactionId));
            Assert.That(transaction.SettlementId, Is.EqualTo(receipt.Id));
            Assert.That(transaction.PostedAt, Is.EqualTo(receipt.PaidAt));
            Assert.That(transaction.Entries.Count, Is.EqualTo(4));
            AssertEconomyEntry(
                transaction.Entries[0],
                EconomyAccountKind.Cash,
                EconomyEntryDirection.Debit,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertEconomyEntry(
                transaction.Entries[1],
                EconomyAccountKind.SalesRevenue,
                EconomyEntryDirection.Credit,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertEconomyEntry(
                transaction.Entries[2],
                EconomyAccountKind.CostOfGoodsSold,
                EconomyEntryDirection.Debit,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits);
            AssertEconomyEntry(
                transaction.Entries[3],
                EconomyAccountKind.InventoryAsset,
                EconomyEntryDirection.Credit,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits);
            Assert.That(
                transaction.Entries[0].MinorUnits + transaction.Entries[2].MinorUnits,
                Is.EqualTo(
                    transaction.Entries[1].MinorUnits + transaction.Entries[3].MinorUnits));

            CurrencyCode currency = CurrencyCode.Create(
                GarageStockFlowSession.PrototypeCurrencyCode).Value;
            OperationResult<long> cashDelta = session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.Cash,
                currency);
            OperationResult<long> revenueDelta = session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.SalesRevenue,
                currency);
            OperationResult<long> cogsDelta = session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.CostOfGoodsSold,
                currency);
            OperationResult<long> inventoryDelta = session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.InventoryAsset,
                currency);
            Assert.That(cashDelta.IsSuccess, Is.True);
            Assert.That(cashDelta.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(revenueDelta.IsSuccess, Is.True);
            Assert.That(revenueDelta.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(cogsDelta.IsSuccess, Is.True);
            Assert.That(cogsDelta.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(inventoryDelta.IsSuccess, Is.True);
            Assert.That(inventoryDelta.Value,
                Is.EqualTo(-GarageStockFlowSession.PrototypeUnitCostMinorUnits));

            long inventoryRevision = session.Inventory.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long orderRevision = session.Orders.Revision;
            long economyRevision = session.CheckoutSettlements.Revision;
            long customerRevision = session.CustomerVisits.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;
            OperationResult replay = session.SettlePrototypeCashCheckout();
            OperationResult conflict = session.CheckoutSettlements.SettleCashCheckout(
                session.PrototypeCheckoutSettlementId,
                StableId<EconomyLedgerTransactionIdScope>.Parse(
                    "economy.ledger-transaction.playmode-conflict"),
                session.PrototypeCheckoutCompletionId,
                session.PrototypeCheckoutId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits,
                receipt.PaidAt);

            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(conflict.Error,
                Is.EqualTo(CheckoutSettlementFailures.SettlementIdentityConflict));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(session.RetailCheckouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(session.CheckoutSettlements.Revision, Is.EqualTo(economyRevision));
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(customerRevision));
            Assert.That(session.CustomerOfferActions.Revision, Is.EqualTo(actionRevision));
            Assert.That(session.RetailCheckouts.CompletionCount, Is.EqualTo(1));
            Assert.That(session.CheckoutSettlements.SettlementCount, Is.EqualTo(1));
            Assert.That(session.CheckoutSettlements.TransactionCount, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertEconomyEntry(
            EconomyLedgerEntryRecord entry,
            EconomyAccountKind account,
            EconomyEntryDirection direction,
            long minorUnits)
        {
            Assert.That(entry.Account, Is.EqualTo(account));
            Assert.That(entry.Direction, Is.EqualTo(direction));
            Assert.That(entry.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(entry.MinorUnits, Is.EqualTo(minorUnits));
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
