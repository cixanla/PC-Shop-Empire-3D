using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
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
        private const string MotherboardPhysicalItemId =
            GarageStockFlowSession.MotherboardItemInstanceIdValue;
        private const string ProcessorPhysicalItemId =
            GarageStockFlowSession.ProcessorItemInstanceIdValue;
        private const string MemoryPhysicalItemId =
            GarageStockFlowSession.MemoryItemInstanceIdValue;
        private const string StoragePhysicalItemId =
            GarageStockFlowSession.StorageItemInstanceIdValue;

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
            DefaultExecutionOrder carryExecutionOrder =
                (DefaultExecutionOrder)System.Attribute.GetCustomAttribute(
                    typeof(PlayerCarryController),
                    typeof(DefaultExecutionOrder));
            Assert.That(carryExecutionOrder, Is.Not.Null);
            Assert.That(executionOrder.order, Is.LessThan(carryExecutionOrder.order));

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
            const float SimulatedFrameDeltaTime = 1f / 60f;
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
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null);
            controller.enabled = false;
            player.SetPositionAndRotation(
                FindOpenMovementTestPosition(controller),
                Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();

            Vector3 keyboardStart = player.position;
            Vector3 forward = player.forward;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W, Key.LeftShift));
            InputSystem.Update();
            Assert.That(keyboard.wKey.isPressed, Is.True);
            Assert.That(marker.PlayerInput.Move.y, Is.GreaterThan(0.9f));
            Assert.That(marker.PlayerInput.SprintHeld, Is.True);
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(
                Vector3.Dot(player.position - keyboardStart, forward),
                Is.GreaterThan(0.001f),
                "W must move the live CharacterController forward.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            keyboardStart = player.position;
            forward = player.forward;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.S));
            InputSystem.Update();
            Assert.That(keyboard.sKey.isPressed, Is.True);
            Assert.That(marker.PlayerInput.Move.y, Is.LessThan(-0.9f));
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(
                Vector3.Dot(player.position - keyboardStart, forward),
                Is.LessThan(-0.001f),
                "S must move the live CharacterController backward.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            keyboardStart = player.position;
            Vector3 right = player.right;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.A));
            InputSystem.Update();
            Assert.That(keyboard.aKey.isPressed, Is.True);
            Assert.That(marker.PlayerInput.Move.x, Is.LessThan(-0.9f));
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(
                Vector3.Dot(player.position - keyboardStart, right),
                Is.LessThan(-0.001f),
                "A must move the live CharacterController left.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            keyboardStart = player.position;
            right = player.right;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D));
            InputSystem.Update();
            Assert.That(keyboard.dKey.isPressed, Is.True);
            Assert.That(marker.PlayerInput.Move.x, Is.GreaterThan(0.9f));
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(
                Vector3.Dot(player.position - keyboardStart, right),
                Is.GreaterThan(0.001f),
                "D must move the live CharacterController right.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            Assert.That(marker.PlayerInput.Move, Is.EqualTo(Vector2.zero));

            keyboardStart = player.position;
            forward = player.forward;
            right = player.right;
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.W, Key.D));
            InputSystem.Update();
            Assert.That(marker.PlayerInput.Move.x, Is.GreaterThan(0f));
            Assert.That(marker.PlayerInput.Move.y, Is.GreaterThan(0f));
            Assert.That(marker.PlayerInput.Move.magnitude,
                Is.LessThanOrEqualTo(1.001f));
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Vector3 diagonalDelta = player.position - keyboardStart;
            Assert.That(Vector3.Dot(diagonalDelta, forward), Is.GreaterThan(0.001f));
            Assert.That(Vector3.Dot(diagonalDelta, right), Is.GreaterThan(0.001f));

            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.W, Key.S, Key.A, Key.D));
            InputSystem.Update();
            Assert.That(
                marker.PlayerInput.Move.sqrMagnitude,
                Is.LessThan(0.000001f),
                "Opposite movement keys must resolve to a neutral input vector.");
            keyboardStart = player.position;
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(
                Vector3.ProjectOnPlane(
                    player.position - keyboardStart,
                    Vector3.up).magnitude,
                Is.LessThan(0.001f),
                "Opposite movement keys must cancel without horizontal drift.");

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            InputSystem.Update();
            keyboardStart = player.position;
            marker.PlayerMotor.SetPaused(true);
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(
                Vector3.ProjectOnPlane(
                    player.position - keyboardStart,
                    Vector3.up).magnitude,
                Is.LessThan(0.001f),
                "Paused gameplay must not consume movement input.");
            marker.PlayerMotor.SetPaused(false);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            float yawBeforeMouse = player.eulerAngles.y;
            float pitchBeforeMouse = cameraPivot.localEulerAngles.x;
            InputSystem.QueueStateEvent(mouse, new MouseState { delta = new Vector2(30f, -12f) });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.Look.sqrMagnitude, Is.GreaterThan(0f));
            marker.PlayerMotor.SetPaused(false);
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(Mathf.DeltaAngle(yawBeforeMouse, player.eulerAngles.y), Is.Not.EqualTo(0f).Within(0.001f));
            Assert.That(Mathf.DeltaAngle(pitchBeforeMouse, cameraPivot.localEulerAngles.x), Is.Not.EqualTo(0f).Within(0.001f));

            Vector3 gamepadStart = player.position;
            forward = player.forward;
            right = player.right;
            float yawBeforeGamepad = player.eulerAngles.y;
            InputSystem.QueueStateEvent(gamepad, new GamepadState
            {
                leftStick = new Vector2(-0.8f, -0.8f),
                rightStick = new Vector2(0.75f, -0.65f)
            });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.Move.x, Is.LessThan(0f));
            Assert.That(marker.PlayerInput.Move.y, Is.LessThan(0f));
            Assert.That(marker.PlayerInput.Move.magnitude,
                Is.LessThanOrEqualTo(1.001f));
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Vector3 gamepadHorizontalDelta = Vector3.ProjectOnPlane(
                player.position - gamepadStart,
                Vector3.up);
            Assert.That(Vector3.Dot(gamepadHorizontalDelta, forward),
                Is.LessThan(-0.001f));
            Assert.That(Vector3.Dot(gamepadHorizontalDelta, right),
                Is.LessThan(-0.001f));
            Assert.That(Mathf.DeltaAngle(yawBeforeGamepad, player.eulerAngles.y), Is.Not.EqualTo(0f).Within(0.001f));
        }

        [UnityTest]
        public IEnumerator PauseToggleInputFramesNeverLurchOrLook()
        {
            const float SimulatedFrameDeltaTime = 1f / 60f;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            Transform player = marker.PlayerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            Assert.That(cameraPivot, Is.Not.Null);
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null);
            controller.enabled = false;
            player.SetPositionAndRotation(
                FindOpenMovementTestPosition(controller),
                Quaternion.identity);
            controller.enabled = true;
            Physics.SyncTransforms();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();

            marker.PlayerMotor.SetPaused(true);
            Vector3 start = player.position;
            float yaw = player.eulerAngles.y;
            float pitch = cameraPivot.localEulerAngles.x;
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.Escape, Key.W));
            InputSystem.QueueStateEvent(
                mouse,
                new MouseState { delta = new Vector2(80f, -40f) });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.PausePressedThisFrame, Is.True);
            Assert.That(marker.PlayerInput.Move.y, Is.GreaterThan(0.9f));
            Assert.That(marker.PlayerInput.Look.sqrMagnitude, Is.GreaterThan(0f));
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude,
                Is.LessThan(0.001f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw, player.eulerAngles.y)),
                Is.LessThan(0.001f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(
                    pitch,
                    cameraPivot.localEulerAngles.x)),
                Is.LessThan(0.001f));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();

            start = player.position;
            Vector3 forward = player.forward;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            InputSystem.Update();
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(Vector3.Dot(player.position - start, forward),
                Is.GreaterThan(0.001f));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            start = player.position;
            yaw = player.eulerAngles.y;
            pitch = cameraPivot.localEulerAngles.x;
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.Start,
                    leftStick = Vector2.up,
                    rightStick = new Vector2(0.75f, -0.65f)
                });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.PausePressedThisFrame, Is.True);
            Assert.That(marker.PlayerInput.Move.y, Is.GreaterThan(0.9f));
            Assert.That(marker.PlayerInput.Look.sqrMagnitude, Is.GreaterThan(0f));
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(marker.PlayerMotor.IsPaused, Is.True);
            Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude,
                Is.LessThan(0.001f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw, player.eulerAngles.y)),
                Is.LessThan(0.001f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(
                    pitch,
                    cameraPivot.localEulerAngles.x)),
                Is.LessThan(0.001f));
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();

            start = player.position;
            yaw = player.eulerAngles.y;
            pitch = cameraPivot.localEulerAngles.x;
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.Start,
                    leftStick = Vector2.up,
                    rightStick = new Vector2(0.75f, -0.65f)
                });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.PausePressedThisFrame, Is.True);
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(Vector3.ProjectOnPlane(player.position - start, Vector3.up).magnitude,
                Is.LessThan(0.001f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(yaw, player.eulerAngles.y)),
                Is.LessThan(0.001f));
            Assert.That(Mathf.Abs(Mathf.DeltaAngle(
                    pitch,
                    cameraPivot.localEulerAngles.x)),
                Is.LessThan(0.001f));
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
        public IEnumerator KeyboardMouseSeatsDetachesAndRecoversSameMotherboardIdentity()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            int physicalIdentity = motherboard.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);
            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            AssertMotherboardIsResolvable(marker, motherboard);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(motherboard));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("HASSAS PARÇA"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToMotherboardSeat(marker);
            long assemblyRevisionBeforeCoEdge = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeCoEdge = session.Inventory.Revision;
            int receiptCountBeforeCoEdge = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsMotherboardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionBeforeCoEdge));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCoEdge));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountBeforeCoEdge));
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid), marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsShowingValidPose, Is.True);
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.False);
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("E"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("G"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt, Is.EqualTo("R"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("LMB"));
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo("[OK] HİZALI • G: oturt • R: döndür • LMB: çık"));
            Assert.That(Vector3.Distance(
                marker.PlayerCarry.PlacementPreview.transform.localScale,
                new Vector3(0.244f, 0.244f, 0.012f)), Is.LessThan(0.0001f));
            Assert.That(Vector3.Distance(
                marker.PlayerCarry.PlacementPreview.CurrentPose.position,
                marker.MotherboardSeat.SnapPose.position), Is.LessThan(0.0001f));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(session.AssemblyBuild.MotherboardItemId,
                Is.EqualTo(session.MotherboardItemId));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);

            yield return new WaitForFixedUpdate();
            yield return null;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            AssertMotherboardIsResolvable(marker, motherboard);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(motherboard));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            Assert.That(recovery.IsSuccess, Is.True,
                recovery.IsFailure ? recovery.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadRotatesSeatsDetachesAndSafelyDropsSameMotherboard()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            string inventoryIdentity = marker.MotherboardBinding.InventoryItemIdValue;
            marker.PlayerMotor.SetPaused(false);
            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt, Is.EqualTo("RB"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
            InputSystem.Update();
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.False);
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("G"));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToMotherboardSeat(marker);
            long assemblyRevisionBeforeCoEdge = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeCoEdge = session.Inventory.Revision;
            int receiptCountBeforeCoEdge = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons = 1u << (int)GamepadButton.East
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsMotherboardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionBeforeCoEdge));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCoEdge));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountBeforeCoEdge));
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid), marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo("[OK] HİZALI • B: oturt • RB: döndür • RT: çık"));
            Assert.That(Vector3.Distance(
                marker.PlayerCarry.PlacementPreview.transform.localScale,
                new Vector3(0.244f, 0.244f, 0.012f)), Is.LessThan(0.0001f));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons = 1u << (int)GamepadButton.East
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsMotherboardSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionBeforeCoEdge));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCoEdge));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountBeforeCoEdge));
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsMotherboardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid), marker.PlayerCarry.LastFailureCode);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.RightShoulder
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.OrientationInvalid));

            for (int turn = 0; turn < 3; turn++)
            {
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.RightShoulder
                    });
                InputSystem.Update();
                marker.PlayerCarry.ProcessInputFrame();
            }

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid), marker.PlayerCarry.LastFailureCode);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));

            yield return new WaitForFixedUpdate();
            yield return null;

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            AssertMotherboardIsResolvable(marker, motherboard);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToOpenDropArea(marker);
            Vector3 safeSeatPosition = motherboard.LastSafePosition;
            Quaternion safeSeatRotation = motherboard.LastSafeRotation;
            StableId<ItemInstanceIdScope> firstBlocker = default;
            Assert.That(session.Inventory.TryGetContainer(
                session.WorldFloorContainerId,
                out InventoryContainerDefinition worldFloor), Is.True);
            int blockersToFill = worldFloor.UnitCapacity - (int)session.Inventory
                .GetContainerQuantity(session.WorldFloorContainerId).Value;
            Assert.That(blockersToFill, Is.GreaterThan(0));
            for (int index = 0; index < blockersToFill; index++)
            {
                StableId<ItemInstanceIdScope> blocker =
                    StableId<ItemInstanceIdScope>.Parse(
                        $"inventory.item.motherboard-drop-blocker-{index}");
                if (index == 0)
                {
                    firstBlocker = blocker;
                }

                Assert.That(session.Inventory.ReceiveSerializedItem(
                    blocker,
                    session.ProductId,
                    session.WorldFloorContainerId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create(
                        GarageStockFlowSession.PrototypeCurrencyCode,
                        GarageStockFlowSession.PrototypeUnitCostMinorUnits).Value).IsSuccess,
                    Is.True);
            }

            long blockedAssemblyRevision = session.AssemblyBuild.Revision;
            long blockedInventoryRevision = session.Inventory.Revision;
            int blockedReceiptCount = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(motherboard.Ownership, Is.EqualTo(PhysicalItemOwnership.PlayerHands));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(Vector3.Distance(
                motherboard.LastSafePosition,
                safeSeatPosition), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(
                motherboard.LastSafeRotation,
                safeSeatRotation), Is.LessThan(0.0001f));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(blockedAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(blockedInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(blockedReceiptCount));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            Assert.That(session.Inventory.TransferSerializedItem(
                firstBlocker,
                session.ShelfContainerId).IsSuccess, Is.True);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(marker.MotherboardBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(marker.MotherboardBinding.InventoryItemIdValue,
                Is.EqualTo(inventoryIdentity));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseSeatsRetainsGatesRemovesAndRecoversSameProcessor()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GaragePrototypeHud hud =
                Object.FindFirstObjectByType<GaragePrototypeHud>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            PhysicalItemProjection processor = FindPhysicalItem(
                ProcessorPhysicalItemId);
            int physicalIdentity = processor.GetInstanceID();
            Pose initialProcessorPose = new Pose(
                processor.transform.position,
                processor.transform.rotation);
            Transform initialProcessorParent = processor.transform.parent;
            marker.PlayerMotor.SetPaused(false);
            PrepareSecuredMotherboardForProcessor(marker, motherboard);

            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(processor));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("HASSAS PARÇA"));
            Assert.That(hud.UsesCompactAssemblyUi, Is.True);
            Assert.That(hud.EffectivePromptText,
                Is.EqualTo(marker.PlayerCarry.PromptText));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ContextMissing));
            long assemblyRevisionBeforeCoEdge = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeCoEdge = session.Inventory.Revision;
            int receiptCountBeforeCoEdge = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsShowingValidPose, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo("[OK] CPU ANAHTARI HİZALI • G: oturt • R: döndür • LMB: çık"));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionBeforeCoEdge));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeCoEdge));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountBeforeCoEdge));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            AssertProcessorAtSocket(marker, processor, "keyboard-seat");
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsProcessorSocketFocused, Is.True,
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("LMB: kolu kapat"));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(marker.ProcessorBinding.IsRetained, Is.True);
            Assert.That(marker.ProcessorSocket.MatchesAuthorityState(
                AssemblySeatState.SeatedSecured,
                ProcessorSocketState.ProcessorRetained), Is.True);

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ProcessorRetained.Code));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            MovePlayerToMotherboardFastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            AssertProcessorAtSocket(marker, processor, "keyboard-unsecure-retained");

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            long blockedDetachAssemblyRevision = session.AssemblyBuild.Revision;
            long blockedDetachInventoryRevision = session.Inventory.Revision;
            int blockedDetachReceiptCount = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ProcessorInstalled.Code));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(blockedDetachAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(blockedDetachInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(blockedDetachReceiptCount));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            MovePlayerToProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsProcessorSocketFocused, Is.True,
                marker.PlayerCarry.LastFailureCode);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            Assert.That(marker.ProcessorBinding.IsRetained, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null,
                "Primary+Interact co-edge must only open retention.");

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("ANAKART SABİT DEĞİL"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Not.Contain("kolu kapat"));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            Assert.That(recovery.IsSuccess, Is.True,
                recovery.IsFailure ? recovery.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(processor.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(processor.transform.parent, Is.SameAs(initialProcessorParent));
            Assert.That(Vector3.Distance(
                processor.transform.position,
                initialProcessorPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                processor.transform.rotation,
                initialProcessorPose.rotation), Is.LessThan(0.05f));
            Assert.That(Vector3.Distance(
                processor.Body.position,
                initialProcessorPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                processor.Body.rotation,
                initialProcessorPose.rotation), Is.LessThan(0.05f));
            Assert.That(Vector3.Distance(
                processor.LastSafePosition,
                initialProcessorPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                processor.LastSafeRotation,
                initialProcessorPose.rotation), Is.LessThan(0.05f));
            Assert.That(marker.ProcessorBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadKeyedRotationConsumesCoEdgesAndCompletesProcessorCycle()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GaragePrototypeHud hud =
                Object.FindFirstObjectByType<GaragePrototypeHud>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            PhysicalItemProjection processor = FindPhysicalItem(
                ProcessorPhysicalItemId);
            marker.PlayerMotor.SetPaused(false);
            PrepareSecuredMotherboardForProcessor(marker, motherboard);

            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt, Is.EqualTo("RB"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));
            Assert.That(hud.UsesCompactAssemblyUi, Is.True);
            Assert.That(hud.EffectivePromptText,
                Is.EqualTo(marker.PlayerCarry.PromptText));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ContextMissing));
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);

            long assemblyRevision = session.AssemblyBuild.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int receiptCount = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.RightShoulder) |
                              (1u << (int)GamepadButton.East)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo("[X] ANAHTAR YÖNÜ • RB: döndür • RT: çık"));

            for (int turn = 0; turn < 3; turn++)
            {
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
                InputSystem.Update();
                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.RightShoulder
                    });
                InputSystem.Update();
                marker.PlayerCarry.ProcessInputFrame();
            }

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            AssertProcessorAtSocket(marker, processor, "gamepad-seat");

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ProcessorRetained.Code));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseSeatsDualLatchGatesRemovesAndRecoversSameDimm()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            PhysicalItemProjection memory = FindPhysicalItem(MemoryPhysicalItemId);
            int physicalIdentity = memory.GetInstanceID();
            Pose initialPose = new Pose(memory.transform.position, memory.transform.rotation);
            Transform initialParent = memory.transform.parent;
            marker.PlayerMotor.SetPaused(false);
            PrepareSecuredMotherboardForProcessor(marker, motherboard);

            AimPlayerAtItem(marker, memory, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(memory));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("ANAHTARLI DDR5"));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToDimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            long assemblyBeforeMode = session.AssemblyBuild.Revision;
            long inventoryBeforeMode = session.Inventory.Revision;
            int receiptsBeforeMode = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsDimmSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.ValidSeat),
                $"{marker.PlayerCarry.LastFailureCode} overlaps=" +
                DescribeDimmSeatOverlaps(marker, memory));
            Assert.That(marker.PlayerCarry.PlacementPreview.IsShowingValidPose, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeMode));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeMode));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeMode));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(2));
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeMode));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeMode));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeMode));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.ValidSeat), marker.PlayerCarry.LastFailureCode);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeMode + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeMode + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeMode + 1));
            AssertMemoryAtDimmSlot(marker, memory, "keyboard-seat");
            AssemblyBuildSnapshot seatedSnapshot = session.AssemblyBuild.GetSnapshot();

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToDimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsDimmSlotFocused, Is.True,
                marker.PlayerCarry.LastFailureCode);
            long assemblyBeforeClose = session.AssemblyBuild.Revision;
            long inventoryBeforeClose = session.Inventory.Revision;
            int receiptsBeforeClose = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeClose + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeClose));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeClose + 1));
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.ClosingLeft));
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.ClosingRight));
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);
            Assert.That(marker.DimmSlot.MatchesAuthorityState(
                AssemblySeatState.SeatedSecured,
                MemorySlotState.MemoryModuleRetained), Is.True);
            AssemblyBuildSnapshot retainedSnapshot = session.AssemblyBuild.GetSnapshot();

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.MemoryModuleRetained.Code));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeClose + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeClose));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeClose + 1));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToMotherboardFastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            long assemblyBeforeHostDetach = session.AssemblyBuild.Revision;
            long inventoryBeforeHostDetach = session.Inventory.Revision;
            int receiptsBeforeHostDetach = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.MemoryModuleInstalled.Code));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeHostDetach));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeHostDetach));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptsBeforeHostDetach));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToDimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            long assemblyBeforeOpen = session.AssemblyBuild.Revision;
            long inventoryBeforeOpen = session.Inventory.Revision;
            int receiptsBeforeOpen = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null,
                "Primary+Interact co-edge must only open the aggregate retention.");
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeOpen + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeOpen));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeOpen + 1));
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.OpeningRight));
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.OpeningLeft));
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);
            Assert.That(marker.DimmSlot.MatchesAuthorityState(
                AssemblySeatState.SeatedUnsecured,
                MemorySlotState.MemoryModuleSeatedOpen), Is.True);
            AssemblyOperationReceipt openReceipt = session.AssemblyBuild.GetReceipts()
                .Single(receipt => receipt.OperationKind ==
                    AssemblyOperationKind.OpenMemoryRetention);

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            long assemblyBeforeRemove = session.AssemblyBuild.Revision;
            long inventoryBeforeRemove = session.Inventory.Revision;
            int receiptsBeforeRemove = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeRemove + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeRemove + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeRemove + 1));
            AssemblyOperationReceipt removeReceipt = session.AssemblyBuild.GetReceipts()
                .Single(receipt => receipt.AssemblyRevision == assemblyBeforeRemove + 1);
            Assert.That(removeReceipt.OperationKind,
                Is.EqualTo(AssemblyOperationKind.RemoveMemoryModule));
            Assert.That(removeReceipt.ItemId, Is.EqualTo(session.MemoryItemId));
            Assert.That(removeReceipt.ProductId, Is.EqualTo(session.MemoryProductId));
            Assert.That(removeReceipt.SlotId, Is.EqualTo(session.MemorySlotId));
            Assert.That(removeReceipt.SourceContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            Assert.That(removeReceipt.TargetContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(removeReceipt.SourceMemorySeatOperationId,
                Is.EqualTo(seatedSnapshot.MemorySeatedByOperationId));
            Assert.That(removeReceipt.ExpectedAssemblyRevision,
                Is.EqualTo(assemblyBeforeRemove));
            Assert.That(removeReceipt.PreviousMemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(removeReceipt.ResultingMemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(removeReceipt.InventoryRevision,
                Is.EqualTo(inventoryBeforeRemove + 1));

            long assemblyBeforeReplay = session.AssemblyBuild.Revision;
            long inventoryBeforeReplay = session.Inventory.Revision;
            int receiptsBeforeReplay = session.AssemblyBuild.ReceiptCount;
            OperationResult<AssemblyOperationReceipt> replay = session.OpenMemoryRetention(
                openReceipt.OperationId,
                seatedSnapshot.MemorySeatedByOperationId,
                retainedSnapshot.MemoryRetainedByOperationId,
                assemblyBeforeOpen);
            Assert.That(replay.IsSuccess, Is.True,
                replay.IsFailure ? replay.Error.Code : string.Empty);
            Assert.That(replay.Value, Is.SameAs(openReceipt));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeReplay));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeReplay));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeReplay));

            long assemblyBeforeRecovery = session.AssemblyBuild.Revision;
            long inventoryBeforeRecovery = session.Inventory.Revision;
            int receiptsBeforeRecovery = session.AssemblyBuild.ReceiptCount;
            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            Assert.That(recovery.IsSuccess, Is.True,
                recovery.IsFailure ? recovery.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(memory.transform.parent, Is.SameAs(initialParent));
            Assert.That(Vector3.Distance(memory.transform.position, initialPose.position),
                Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(memory.transform.rotation, initialPose.rotation),
                Is.LessThan(0.05f));
            Assert.That(marker.DimmBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeRecovery));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeRecovery + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeRecovery));
            Assert.That(session.Inventory.SerializedItemCount, Is.EqualTo(10));
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord recovered), Is.True);
            Assert.That(recovered.Id, Is.EqualTo(session.MemoryItemId));
            Assert.That(recovered.ProductId, Is.EqualTo(session.MemoryProductId));
            Assert.That(recovered.ContainerId, Is.EqualTo(session.WorldFloorContainerId));
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadKeyedCoEdgesCompleteDualLatchDimmCycle()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            PhysicalItemProjection memory = FindPhysicalItem(MemoryPhysicalItemId);
            int physicalIdentity = memory.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);
            PrepareSecuredMotherboardForProcessor(marker, motherboard);

            AimPlayerAtItem(marker, memory, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt, Is.EqualTo("RB"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToDimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsDimmSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.ValidSeat), marker.PlayerCarry.LastFailureCode);

            long assemblyBeforeRotation = session.AssemblyBuild.Revision;
            long inventoryBeforeRotation = session.Inventory.Revision;
            int receiptsBeforeRotation = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.RightShoulder) |
                              (1u << (int)GamepadButton.East)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(2));
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeRotation));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeRotation));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptsBeforeRotation));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.RightShoulder
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.ValidSeat), marker.PlayerCarry.LastFailureCode);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            AssertMemoryAtDimmSlot(marker, memory, "gamepad-seat");

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToDimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.ClosingLeft));
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.ClosingRight));
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.MemoryModuleRetained.Code));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.OpeningRight));
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.OpeningLeft));
            marker.DimmSlot.AdvanceLatchAnimation(0.10f);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            AssemblyBuildSnapshot beforeRecovery = session.AssemblyBuild.GetSnapshot();
            long assemblyBeforeRecovery = session.AssemblyBuild.Revision;
            long inventoryBeforeRecovery = session.Inventory.Revision;
            int receiptsBeforeRecovery = session.AssemblyBuild.ReceiptCount;
            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            Assert.That(recovery.IsSuccess, Is.True,
                recovery.IsFailure ? recovery.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyBeforeRecovery + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeRecovery + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptsBeforeRecovery + 1));
            Assert.That(session.TryGetMemoryItem(out InventoryItemRecord recovered), Is.True);
            Assert.That(recovered.Id, Is.EqualTo(session.MemoryItemId));
            Assert.That(recovered.ProductId, Is.EqualTo(session.MemoryProductId));
            Assert.That(recovered.ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            AssemblyOperationReceipt recoveryReceipt = session.AssemblyBuild.GetReceipts()
                .Single(receipt => receipt.AssemblyRevision == assemblyBeforeRecovery + 1);
            Assert.That(recoveryReceipt.OperationKind,
                Is.EqualTo(AssemblyOperationKind.SeatMemoryModule));
            Assert.That(recoveryReceipt.ItemId, Is.EqualTo(session.MemoryItemId));
            Assert.That(recoveryReceipt.ProductId, Is.EqualTo(session.MemoryProductId));
            Assert.That(recoveryReceipt.SlotId, Is.EqualTo(session.MemorySlotId));
            Assert.That(recoveryReceipt.SourceContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(recoveryReceipt.TargetContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            Assert.That(recoveryReceipt.SourceAttachOperationId,
                Is.EqualTo(beforeRecovery.InstalledByOperationId));
            Assert.That(recoveryReceipt.SourceSecureOperationId,
                Is.EqualTo(beforeRecovery.SecuredByOperationId));
            Assert.That(recoveryReceipt.ExpectedAssemblyRevision,
                Is.EqualTo(assemblyBeforeRecovery));
            Assert.That(recoveryReceipt.PreviousMemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(recoveryReceipt.ResultingMemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(recoveryReceipt.DimmKeyOrientation,
                Is.EqualTo(DimmKeyOrientation.NotchAligned));
            AssertMemoryAtDimmSlot(marker, memory, "gamepad-recovery");
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseFastenerGatesPhysicsAndConsumesOneSameFrameAction()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            marker.PlayerMotor.SetPaused(false);
            SeatMotherboardForFastener(marker, motherboard);
            long inventoryRevision = session.Inventory.Revision;
            long seatedRevision = session.AssemblyBuild.Revision;
            int seatedReceiptCount = session.AssemblyBuild.ReceiptCount;

            MovePlayerToMotherboardFastener(marker);
            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);
            AssertMotherboardAtSeat(marker, motherboard, "paused");
            marker.PlayerMotor.SetPaused(false);
            marker.PlayerCarry.ProcessInputFrame();
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);
            AssertMotherboardAtSeat(marker, motherboard, "pause-edge-drained");
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();

            MovePlayerOutOfFastenerRange(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardFastenerStatus,
                Is.EqualTo(MotherboardFastenerStatus.OutOfRange));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);
            AssertMotherboardAtSeat(marker, motherboard, "out-of-range");
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();

            MovePlayerToMotherboardFastener(marker);
            AimPlayerAwayFromMotherboardFastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardFastenerStatus,
                Is.EqualTo(MotherboardFastenerStatus.NotFocused));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);
            AssertMotherboardAtSeat(marker, motherboard, "not-focused");
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();

            MovePlayerToMotherboardFastener(marker);
            GameObject blocker = CreateMotherboardFastenerBlocker(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardFastenerStatus,
                Is.EqualTo(MotherboardFastenerStatus.Obstructed));
            Assert.That(marker.PlayerCarry.HasMotherboardFastenerContext, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("[X]"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("önünü aç"));
            Assert.That(marker.MotherboardFastener.StatusText.text,
                Is.EqualTo("[X] ÖNÜNÜ AÇ"));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E, Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);
            AssertMotherboardAtSeat(marker, motherboard, "obstructed");
            blocker.SetActive(false);
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            AssertMotherboardAtSeat(marker, motherboard, "obstructed-edge-drained");
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            Object.Destroy(blocker);
            AssertMotherboardAtSeat(marker, motherboard, "post-blocker");

            MovePlayerToMotherboardFastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardFastenerStatus,
                Is.EqualTo(MotherboardFastenerStatus.ValidUnsecured));
            Assert.That(marker.MotherboardFastener.FocusCollider.enabled, Is.True);

            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E, Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.MotherboardFastener.IsShowingSecured, Is.True);
            AssertMotherboardAtSeat(marker, motherboard, "secured");
            Vector3 securedScrewPosition = marker.MotherboardFastener.ScrewHead.localPosition;
            marker.MotherboardFastener.ScrewHead.localPosition -= Vector3.forward * 0.004f;
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsFailure,
                Is.True);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().Error.Code,
                Is.EqualTo("assembly-seat.projection-invariant"));
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedSecured,
                seatedRevision + 1,
                inventoryRevision,
                seatedReceiptCount + 1);
            marker.MotherboardFastener.ApplyAuthoritativeState(
                AssemblySeatState.SeatedSecured);
            Assert.That(Vector3.Distance(
                marker.MotherboardFastener.ScrewHead.localPosition,
                securedScrewPosition), Is.LessThan(0.00001f));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.False);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("LMB: gevşet"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("E: sökme kilitli"));

            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 1));
            AssertMotherboardAtSeat(marker, motherboard, "held-primary");

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ComponentSecured.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            AssertMotherboardAtSeat(marker, motherboard, "secured-detach-blocked");

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 2));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 2));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(marker.MotherboardFastener.IsShowingSecured, Is.False);
            Assert.That(marker.MotherboardBinding.IsSeated, Is.True);
            Assert.That(motherboard.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(motherboard.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                motherboard.transform.position,
                marker.MotherboardSeat.SnapPose.position), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(
                motherboard.transform.rotation,
                marker.MotherboardSeat.SnapPose.rotation), Is.LessThan(0.01f));
            Assert.That(marker.MotherboardFastener.MatchesAuthorityState(
                AssemblySeatState.SeatedUnsecured), Is.True);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator GamepadFastenerUsesDynamicPromptsAndOneConsumerPerEdge()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            marker.PlayerMotor.SetPaused(false);
            SeatMotherboardForFastener(marker, motherboard);
            MovePlayerToMotherboardFastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long seatedRevision = session.AssemblyBuild.Revision;
            int seatedReceiptCount = session.AssemblyBuild.ReceiptCount;

            uint coEdgeButtons =
                (1u << (int)GamepadButton.South) |
                (1u << (int)GamepadButton.East);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons = coEdgeButtons
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("RT: gevşet"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("A: sökme kilitli"));

            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 1));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ComponentSecured.Code));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(seatedRevision + 2));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 2));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(marker.MotherboardFastener.MatchesAuthorityState(
                AssemblySeatState.SeatedUnsecured), Is.True);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator GamepadFastenerPauseCoEdgeRequiresReleaseRepressInProductionLifecycle()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            marker.PlayerMotor.SetPaused(false);
            SeatMotherboardForFastener(marker, motherboard);
            MovePlayerToMotherboardFastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long seatedRevision = session.AssemblyBuild.Revision;
            int seatedReceiptCount = session.AssemblyBuild.ReceiptCount;

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons = 1u << (int)GamepadButton.Start
                });
            yield return null;
            yield return null;

            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);
            AssertMotherboardAtSeat(marker, motherboard, "resume-co-edge-drained");

            yield return null;
            AssertFastenerAuthoritiesUnchanged(
                session,
                AssemblySeatState.SeatedUnsecured,
                seatedRevision,
                inventoryRevision,
                seatedReceiptCount);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();

            MovePlayerToMotherboardFastener(marker);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(seatedRevision + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            AssertMotherboardAtSeat(marker, motherboard, "fresh-primary-after-resume");

            yield return null;
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(seatedRevision + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 1));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
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
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
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
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(2));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);
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
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(true);
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.False);
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.Escape, Key.E));
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
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
            yield return null;
            Assert.That(customerFlow.ConsultationCompleted, Is.True);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
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
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(2));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);
            Assert.That(stockFlow.ShelfOfferText.text,
                Does.Contain("MÜŞTERİ: 1 ÜRÜN • AYRILDI"));
            Assert.That(stockFlow.StatusText, Does.Contain("SEPET: 1 ÜRÜN • AYRILDI"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("SATIN ALMA ONAYLANDI"));
            Assert.That(marker.PlayerCarry.HasCompetingWorldInteractOwner, Is.False,
                "A customer-reserved shelf item rejects pickup and must not steal " +
                "Interact from a later valid world station.");

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
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(2));
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
            Assert.That(stockFlow.Session.Inventory.SerializedItemCount, Is.EqualTo(10));
            Assert.That(stockFlow.Session.TryGetMotherboardItem(
                out InventoryItemRecord remainingMotherboard), Is.True);
            Assert.That(remainingMotherboard.ContainerId,
                Is.EqualTo(stockFlow.Session.WorldFloorContainerId));
            Assert.That(stockFlow.Session.TryGetProcessorItem(
                out InventoryItemRecord remainingProcessor), Is.True);
            Assert.That(remainingProcessor.Id,
                Is.EqualTo(stockFlow.Session.ProcessorItemId));
            Assert.That(remainingProcessor.ProductId,
                Is.EqualTo(stockFlow.Session.ProcessorProductId));
            Assert.That(remainingProcessor.ContainerId,
                Is.EqualTo(stockFlow.Session.WorldFloorContainerId));
            Assert.That(stockFlow.Session.TryGetMemoryItem(
                out InventoryItemRecord remainingMemory), Is.True);
            Assert.That(remainingMemory.Id,
                Is.EqualTo(stockFlow.Session.MemoryItemId));
            Assert.That(remainingMemory.ProductId,
                Is.EqualTo(stockFlow.Session.MemoryProductId));
            Assert.That(remainingMemory.ContainerId,
                Is.EqualTo(stockFlow.Session.WorldFloorContainerId));
            Assert.That(stockFlow.Session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);
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

            Assert.That(
                marker.PlayerCarry.HeldItem,
                Is.Null,
                marker.PlayerCarry.LastFailureCode);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.ItemIdValue, Is.EqualTo(GarageStockFlowSession.ItemInstanceIdValue));
            AssertInventoryLocation(stockFlow, stockFlow.Session.WorldFloorContainerId);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(2));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);

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
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);
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
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);

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
            Assert.That(stockFlow.Session.Inventory.SerializedItemCount, Is.EqualTo(10));
            Assert.That(stockFlow.Session.TryGetMotherboardItem(
                out InventoryItemRecord remainingMotherboard), Is.True);
            Assert.That(remainingMotherboard.ContainerId,
                Is.EqualTo(stockFlow.Session.WorldFloorContainerId));
            Assert.That(stockFlow.Session.TryGetProcessorItem(
                out InventoryItemRecord remainingProcessor), Is.True);
            Assert.That(remainingProcessor.Id,
                Is.EqualTo(stockFlow.Session.ProcessorItemId));
            Assert.That(remainingProcessor.ProductId,
                Is.EqualTo(stockFlow.Session.ProcessorProductId));
            Assert.That(remainingProcessor.ContainerId,
                Is.EqualTo(stockFlow.Session.WorldFloorContainerId));
            Assert.That(stockFlow.Session.TryGetMemoryItem(
                out InventoryItemRecord remainingMemory), Is.True);
            Assert.That(remainingMemory.Id,
                Is.EqualTo(stockFlow.Session.MemoryItemId));
            Assert.That(remainingMemory.ProductId,
                Is.EqualTo(stockFlow.Session.MemoryProductId));
            Assert.That(remainingMemory.ContainerId,
                Is.EqualTo(stockFlow.Session.WorldFloorContainerId));
            Assert.That(stockFlow.Session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(stockFlow.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(stockFlow.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(stockFlow.Session.Inventory.GetTotalQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(1));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);
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
                stockFlow.Session.ProductId).Value, Is.EqualTo(2));
            Assert.That(stockFlow.Session.Inventory.GetAvailableQuantity(
                stockFlow.Session.ProductId).Value, Is.EqualTo(2));
            AssertAssemblyGraphicsCardIsolated(stockFlow.Session);
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

        [UnityTest]
        public IEnumerator KeyboardMouseCompletesKeyedM2CaptiveScrewCycle()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.Session;
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            PhysicalItemProjection storage = FindPhysicalItem(StoragePhysicalItemId);
            int instanceId = storage.GetInstanceID();
            PrepareSecuredMotherboardForProcessor(marker, motherboard);

            AimPlayerAtItem(marker, storage, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToM2StorageSlot(marker);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsM2StorageSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentM2StorageSlotStatus,
                Is.EqualTo(M2StorageSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("18°"));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentM2StorageSlotStatus,
                Is.EqualTo(M2StorageSlotStatus.OrientationInvalid));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            AssertStorageAtM2Slot(marker, storage, "keyboard-seat");

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            MovePlayerToM2StorageSlot(marker);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
            Assert.That(marker.StorageBinding.IsSecured, Is.True);

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.StorageDeviceSecured.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(storage.GetInstanceID(), Is.EqualTo(instanceId));
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadCoEdgesCompleteM2SeatAndRetentionCycle()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.Session;
            PhysicalItemProjection motherboard = FindPhysicalItem(
                MotherboardPhysicalItemId);
            PhysicalItemProjection storage = FindPhysicalItem(StoragePhysicalItemId);
            PrepareSecuredMotherboardForProcessor(marker, motherboard);

            AimPlayerAtItem(marker, storage, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToM2StorageSlot(marker);
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsM2StorageSeatMode, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.RightShoulder) |
                              (1u << (int)GamepadButton.East)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(marker.PlayerCarry.CurrentM2StorageSlotStatus,
                Is.EqualTo(M2StorageSlotStatus.OrientationInvalid));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.RightShoulder
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            AssertStorageAtM2Slot(marker, storage, "gamepad-seat");

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToM2StorageSlot(marker);
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.StorageDeviceSecured.Code));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(gamepad, new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
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

        private static void MovePlayerToMotherboardSeat(GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.MotherboardSeat.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private static void SeatMotherboardForFastener(
            GaragePrototypeMarker marker,
            PhysicalItemProjection motherboard)
        {
            OperationResult pickup = marker.PlayerCarry.TryPickup(motherboard);
            Assert.That(pickup.IsSuccess, Is.True,
                pickup.IsFailure ? pickup.Error.Code : string.Empty);
            MovePlayerToMotherboardSeat(marker);
            OperationResult begin = marker.PlayerCarry.TrySetMotherboardSeatMode(true);
            Assert.That(begin.IsSuccess, Is.True,
                begin.IsFailure ? begin.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid), marker.PlayerCarry.LastFailureCode);
            OperationResult attach = marker.PlayerCarry.TryConfirmMotherboardSeat();
            Assert.That(attach.IsSuccess, Is.True,
                attach.IsFailure ? attach.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.MotherboardBinding.IsSeated, Is.True);
            Assert.That(marker.MotherboardBinding.IsSecured, Is.False);
            Assert.That(marker.MotherboardFastener.FocusCollider.enabled, Is.True);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
        }

        private static void PrepareSecuredMotherboardForProcessor(
            GaragePrototypeMarker marker,
            PhysicalItemProjection motherboard)
        {
            SeatMotherboardForFastener(marker, motherboard);
            MovePlayerToMotherboardFastener(marker);
            OperationResult secure = marker.PlayerCarry.TryOperateMotherboardFastener();
            Assert.That(secure.IsSuccess, Is.True,
                secure.IsFailure
                    ? $"{secure.Error.Code} {DescribeFastenerLine(marker)}"
                    : string.Empty);
            Assert.That(marker.StockFlow.Session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(marker.MotherboardBinding.IsSecured, Is.True);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
        }

        private static string DescribeFastenerLine(GaragePrototypeMarker marker)
        {
            Camera camera = marker.PlayerMotor.GetComponentInChildren<Camera>(true);
            Vector3 target = marker.MotherboardFastener.FocusCollider.bounds.center;
            Vector3 direction = (target - camera.transform.position).normalized;
            float distance = Vector3.Distance(camera.transform.position, target) + 0.03f;
            return string.Join(
                ",",
                Physics.RaycastAll(
                        camera.transform.position,
                        direction,
                        distance,
                        ~0,
                        QueryTriggerInteraction.Ignore)
                    .OrderBy(hit => hit.distance)
                    .Select(hit => $"{hit.collider.name}@{hit.distance:0.000}"));
        }

        private static void MovePlayerToProcessorSocket(GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.ProcessorSocket.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private static void MovePlayerToDimmSlot(GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.DimmSlot.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private static void MovePlayerToM2StorageSlot(GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.StorageSlot.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private static void AssertStorageAtM2Slot(
            GaragePrototypeMarker marker,
            PhysicalItemProjection storage,
            string stage)
        {
            Assert.That(storage.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World), stage);
            Assert.That(storage.IsStablePlacement, Is.True, stage);
            Assert.That(storage.Body.isKinematic, Is.True, stage);
            Assert.That(storage.Body.useGravity, Is.False, stage);
            Assert.That(Vector3.Distance(
                storage.transform.position,
                marker.StorageSlot.SeatedPose.position), Is.LessThan(0.0001f), stage);
            Assert.That(Quaternion.Angle(
                storage.transform.rotation,
                marker.StorageSlot.SeatedPose.rotation), Is.LessThan(0.01f), stage);
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True, stage);
        }

        private static void AssertMemoryAtDimmSlot(
            GaragePrototypeMarker marker,
            PhysicalItemProjection memory,
            string stage)
        {
            Assert.That(memory.Ownership, Is.EqualTo(PhysicalItemOwnership.World), stage);
            Assert.That(memory.IsStablePlacement, Is.True, stage);
            Assert.That(memory.Body.isKinematic, Is.True, stage);
            Assert.That(memory.Body.useGravity, Is.False, stage);
            Assert.That(Vector3.Distance(
                memory.transform.position,
                marker.DimmSlot.SnapPose.position), Is.LessThan(0.0001f), stage);
            Assert.That(Quaternion.Angle(
                memory.transform.rotation,
                marker.DimmSlot.SnapPose.rotation), Is.LessThan(0.01f), stage);
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True, stage);
        }

        private static string DescribeDimmSeatOverlaps(
            GaragePrototypeMarker marker,
            PhysicalItemProjection memory)
        {
            Pose pose = marker.DimmSlot.SnapPose;
            return string.Join(
                ",",
                Physics.OverlapBox(
                        pose.position,
                        memory.DropHalfExtents,
                        pose.rotation,
                        ~0,
                        QueryTriggerInteraction.Ignore)
                    .Select(collider => collider.name)
                    .OrderBy(name => name));
        }

        private static void AssertProcessorAtSocket(
            GaragePrototypeMarker marker,
            PhysicalItemProjection processor,
            string stage)
        {
            Assert.That(processor.Ownership, Is.EqualTo(PhysicalItemOwnership.World), stage);
            Assert.That(processor.IsStablePlacement, Is.True, stage);
            Assert.That(processor.Body.isKinematic, Is.True, stage);
            Assert.That(processor.Body.useGravity, Is.False, stage);
            Assert.That(Vector3.Distance(
                processor.transform.position,
                marker.ProcessorSocket.SnapPose.position), Is.LessThan(0.0001f), stage);
            Assert.That(Quaternion.Angle(
                processor.transform.rotation,
                marker.ProcessorSocket.SnapPose.rotation), Is.LessThan(0.01f), stage);
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True, stage);
        }

        private static void MovePlayerToMotherboardFastener(GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.MotherboardFastener.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
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

        private static void MovePlayerOutOfFastenerRange(GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.MotherboardFastener.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 1.35f);
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

        private static void AimPlayerAwayFromMotherboardFastener(
            GaragePrototypeMarker marker)
        {
            marker.PlayerMotor.transform.rotation = Quaternion.LookRotation(
                Vector3.back,
                Vector3.up);
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            Physics.SyncTransforms();
        }

        private static GameObject CreateMotherboardFastenerBlocker(
            GaragePrototypeMarker marker)
        {
            Camera camera = marker.PlayerMotor.GetComponentInChildren<Camera>(true);
            Vector3 target = marker.MotherboardFastener.FocusCollider.bounds.center;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "MotherboardFastenerPlayModeBlocker";
            blocker.layer = LayerMask.NameToLayer("Default");
            blocker.transform.position = Vector3.Lerp(camera.transform.position, target, 0.52f);
            blocker.transform.localScale = new Vector3(0.12f, 0.12f, 0.08f);
            Physics.SyncTransforms();
            return blocker;
        }

        private static void AssertFastenerAuthoritiesUnchanged(
            GarageStockFlowSession session,
            AssemblySeatState expectedState,
            long expectedAssemblyRevision,
            long expectedInventoryRevision,
            int expectedReceiptCount)
        {
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(expectedState));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(expectedAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(expectedInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(expectedReceiptCount));
        }

        private static void AssertMotherboardAtSeat(
            GaragePrototypeMarker marker,
            PhysicalItemProjection motherboard,
            string stage)
        {
            Assert.That(motherboard.Ownership, Is.EqualTo(PhysicalItemOwnership.World), stage);
            Assert.That(motherboard.IsStablePlacement, Is.True, stage);
            Assert.That(motherboard.Body.isKinematic, Is.True, stage);
            Assert.That(motherboard.Body.useGravity, Is.False, stage);
            Assert.That(motherboard.Body.interpolation,
                Is.EqualTo(RigidbodyInterpolation.None), stage);
            Assert.That(Vector3.Distance(
                motherboard.transform.position,
                marker.MotherboardSeat.SnapPose.position), Is.LessThan(0.0001f),
                $"{stage} actual={motherboard.transform.position:F6} " +
                $"snap={marker.MotherboardSeat.SnapPose.position:F6} " +
                $"safe={motherboard.LastSafePosition:F6}");
            Assert.That(Quaternion.Angle(
                motherboard.transform.rotation,
                marker.MotherboardSeat.SnapPose.rotation), Is.LessThan(0.01f), stage);
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
            Vector3 target = item.InteractionCenter;
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

        private static void AssertMotherboardIsResolvable(
            GaragePrototypeMarker marker,
            PhysicalItemProjection motherboard)
        {
            PhysicalInteractionResolver resolver =
                marker.PlayerMotor.GetComponent<PhysicalInteractionResolver>();
            OperationResult<PhysicalItemProjection> resolved = resolver.Resolve();
            string hits = string.Join(
                ",",
                Physics.RaycastAll(
                        resolver.Origin.position,
                        resolver.Origin.forward,
                        resolver.MaximumRange + 0.10f,
                        resolver.QueryMask,
                        QueryTriggerInteraction.Ignore)
                    .OrderBy(hit => hit.distance)
                    .Select(hit => $"{hit.collider.name}@{hit.distance:0.000}"));
            Assert.That(
                resolved.IsSuccess && resolved.Value == motherboard,
                Is.True,
                $"code={(resolved.IsFailure ? resolved.Error.Code : "wrong-target")} " +
                $"hits={hits}");
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

        private static void AssertAssemblyGraphicsCardIsolated(
            GarageStockFlowSession session)
        {
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord graphicsCard), Is.True);
            Assert.That(graphicsCard.Id,
                Is.EqualTo(session.GraphicsCardAssemblyItemId));
            Assert.That(graphicsCard.Id, Is.Not.EqualTo(session.ItemId));
            Assert.That(graphicsCard.ProductId, Is.EqualTo(session.ProductId));
            Assert.That(graphicsCard.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));
            if (session.Inventory.TryGetReservation(
                    session.PrototypeReservationId,
                    out InventoryReservation reservation))
            {
                Assert.That(reservation.ItemId, Is.EqualTo(session.ItemId));
                Assert.That(reservation.ItemId,
                    Is.Not.EqualTo(session.GraphicsCardAssemblyItemId));
            }
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

        private static Vector3 FindOpenMovementTestPosition(
            CharacterController controller)
        {
            Vector3[] clearanceOffsets =
            {
                Vector3.zero,
                Vector3.forward * 0.6f,
                Vector3.back * 0.6f,
                Vector3.left * 0.6f,
                Vector3.right * 0.6f
            };
            for (float z = -3.5f; z <= 3.5f; z += 0.5f)
            {
                for (float x = -3f; x <= 3f; x += 0.5f)
                {
                    Vector3 candidate = new Vector3(x, 0.05f, z);
                    if (clearanceOffsets.All(offset =>
                            IsPlayerCapsuleClear(controller, candidate + offset)))
                    {
                        return candidate;
                    }
                }
            }

            Assert.Fail("Garage must contain an open movement-test area.");
            return default;
        }

        private static bool IsPlayerCapsuleClear(
            CharacterController controller,
            Vector3 position)
        {
            float radius = controller.radius + 0.04f;
            float halfHeight = Mathf.Max(controller.height * 0.5f, radius);
            Vector3 center = position + controller.center;
            Vector3 segment = Vector3.up * (halfHeight - radius);
            return Physics.OverlapCapsule(
                    center - segment,
                    center + segment,
                    radius,
                    ~0,
                    QueryTriggerInteraction.Ignore)
                .All(collider =>
                    collider == controller ||
                    collider.transform.IsChildOf(controller.transform));
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
