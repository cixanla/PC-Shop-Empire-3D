using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation;
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
    public sealed class CustomPcWorkTicketInputPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardPostsOneVisibleWorkTicketWithoutMovingReservedParts()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);

            CustomPcWorkTicketStationProjection station =
                marker.CustomPcWorkTicketStation;
            Assert.That(station, Is.Not.Null);
            station.RefreshPresentation();
            Assert.That(station.StationStatusText.text, Does.Contain("10/10"));
            Assert.That(station.StationStatusText.text,
                Does.Contain("İŞ EMRİNİ ÇIKAR"));

            string[] originalContainers = quote.Lines
                .Select(line =>
                {
                    Assert.That(session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item), Is.True);
                    return item.ContainerId.Value;
                })
                .ToArray();
            long inventoryRevision = session.Inventory.Revision;
            long quoteRevision = session.CustomPcQuotes.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int itemCount = session.Inventory.SerializedItemCount;
            int reservationCount = session.Inventory.ReservationCount;

            MovePlayerToStation(marker, 1.35f);
            station.RefreshPresentation();
            Assert.That(station.IsFocused, Is.True);
            Assert.That(station.PromptText, Does.Contain("10/10"));
            Assert.That(station.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;

            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord buildOrder), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(
                out CustomPcWorkTicketRecord workTicket), Is.True);
            Assert.That(buildOrder.Status,
                Is.EqualTo(CustomPcBuildOrderStatus.ReservationSetAllocated));
            Assert.That(workTicket.Status,
                Is.EqualTo(CustomPcWorkTicketStatus.PostedAtWorkbenchStation));
            Assert.That(buildOrder.ReservedSerializedItemCount,
                Is.EqualTo(CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(session.CustomPcWorkOrders.Revision, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(quoteRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.SerializedItemCount, Is.EqualTo(itemCount));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);

            for (int index = 0; index < quote.Lines.Count; index++)
            {
                CustomPcQuoteLineSnapshot line = quote.Lines[index];
                Assert.That(session.Inventory.TryGetSerializedItem(
                    line.ItemId,
                    out InventoryItemRecord item), Is.True);
                Assert.That(item.ContainerId.Value,
                    Is.EqualTo(originalContainers[index]));
                Assert.That(session.Inventory.TryGetReservation(
                    line.ReservationId,
                    out InventoryReservation reservation), Is.True);
                Assert.That(reservation.ItemId, Is.EqualTo(line.ItemId));
                Assert.That(reservation.ClaimId, Is.EqualTo(quote.InventoryClaimId));
            }

            station.RefreshPresentation();
            Assert.That(station.StationStatusText.text,
                Does.Contain("MONTAJA HAZIR"));
            Assert.That(station.StationStatusText.text,
                Does.Contain("HENÜZ BAŞLAMADI"));
            Assert.That(marker.GetComponent<GaragePrototypeHud>().EffectivePromptText,
                Does.Contain("MONTAJA HAZIR"));
            long committedInventoryRevision = session.Inventory.Revision;
            long committedWorkOrderRevision = session.CustomPcWorkOrders.Revision;
            yield return null;
            yield return null;
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(committedInventoryRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(committedWorkOrderRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator OffTargetInteractRemainsAvailableAndPauseCoedgesRequireFreshPress()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            CustomPcWorkTicketStationProjection station =
                marker.CustomPcWorkTicketStation;
            long inventoryRevision = session.Inventory.Revision;

            MovePlayerToStation(marker, 1.35f);
            Vector3 target = station.InteractionCollider.bounds.center;
            Quaternion centeredLook = Quaternion.LookRotation(
                target - station.PlayerCamera.transform.position,
                Vector3.up);
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            Assert.That(cameraPivot, Is.Not.Null);
            cameraPivot.rotation =
                Quaternion.AngleAxis(20f, Vector3.up) * centeredLook;
            Physics.SyncTransforms();
            station.RefreshPresentation();
            Assert.That(station.HasContextualAttention, Is.True);
            Assert.That(station.IsFocused, Is.False);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True);
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True,
                "Off-target station must not consume another physical interaction's press.");
            AssertUnissued(session, inventoryRevision);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            MovePlayerToStation(marker, 1.35f);
            station.RefreshPresentation();
            Assert.That(station.IsFocused, Is.True);

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.Escape, Key.E));
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            AssertUnissued(session, inventoryRevision);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.Start) |
                              (1u << (int)GamepadButton.South)
                });
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.True);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            AssertUnissued(session, inventoryRevision);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return null;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.Start });
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            AssertUnissued(session, inventoryRevision);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return null;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            yield return null;
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(out _), Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcWorkOrders.Revision, Is.EqualTo(1));
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator BusyTicketYieldsInteractToFocusedCartInSameFrame()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            CustomPcWorkTicketStationProjection station =
                marker.CustomPcWorkTicketStation;
            long inventoryRevision = session.Inventory.Revision;

            PhysicalItemProjection largeBox = Object
                .FindObjectsByType<PhysicalItemProjection>(FindObjectsSortMode.None)
                .Single(item =>
                    item.CarryProfile == PhysicalCarryProfile.LargeBox);
            Assert.That(marker.PlayerCarry.TryPickup(largeBox).IsSuccess, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(largeBox));

            MovePlayerToStation(marker, 1.35f);
            Vector3 ticketTarget = station.InteractionCollider.bounds.center;
            Vector3 desiredCartHit = ticketTarget +
                                     (station.PlayerCamera.transform.forward * 0.35f);
            GameObject cartObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cartObject.name = "BusyTicketFocusedCart";
            cartObject.SetActive(false);
            cartObject.layer = LayerMask.NameToLayer("Interactable");
            cartObject.transform.position = desiredCartHit;
            cartObject.transform.localScale = Vector3.one * 0.18f;
            Rigidbody cartBody = cartObject.AddComponent<Rigidbody>();
            cartBody.useGravity = false;
            cartBody.isKinematic = true;
            Transform cargoAnchor = new GameObject("CargoAnchor").transform;
            cargoAnchor.SetParent(cartObject.transform, worldPositionStays: false);
            cargoAnchor.localPosition = Vector3.up * 0.25f;
            TransportCartProjection cart =
                cartObject.AddComponent<TransportCartProjection>();
            cart.Configure(
                "test.busy-ticket-focused-cart",
                "Test Arabası",
                cartBody,
                cargoAnchor,
                Vector3.one * 0.2f,
                Vector3.one * 0.25f);
            cartObject.SetActive(true);
            Physics.SyncTransforms();

            station.RefreshPresentation();
            Assert.That(station.IsFocused, Is.True);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedCart, Is.SameAs(cart));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True,
                "A busy ticket must leave Interact for the later carry consumer.");
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(cart.Cargo, Is.SameAs(largeBox));
            Assert.That(largeBox.IsMountedOnTransportCart, Is.True);
            AssertUnissued(session, inventoryRevision);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            station.RefreshPresentation();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.FocusedCart, Is.SameAs(cart));
            Assert.That(station.InspectInteractionGateForTests().Error,
                Is.EqualTo(
                    CustomPcWorkTicketStationFailures.CompetingInteractOwner));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True,
                "A loaded focused cart must own Interact before an overlapping ticket.");
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(cart.Cargo, Is.Null);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(largeBox));
            Assert.That(largeBox.IsMountedOnTransportCart, Is.False);
            AssertUnissued(session, inventoryRevision);
            Object.DestroyImmediate(cartObject);
        }

        [UnityTest]
        public IEnumerator RangeLosAndPauseFailClosedBeforeExactIssue()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            CustomPcWorkTicketStationProjection station =
                marker.CustomPcWorkTicketStation;
            long inventoryRevision = session.Inventory.Revision;

            MovePlayerToStation(marker, station.InteractionRange + 0.65f);
            OperationResult outOfRange =
                station.InspectInteractionGateForTests();
            Assert.That(outOfRange.Error,
                Is.EqualTo(CustomPcWorkTicketStationFailures.OutOfRange));
            AssertUnissued(session, inventoryRevision);

            MovePlayerToStation(marker, 1.35f);
            Vector3 cameraPosition = station.PlayerCamera.transform.position;
            Vector3 target = station.InteractionCollider.bounds.center;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "CustomPcWorkTicketLosBlocker";
            blocker.transform.position = Vector3.Lerp(cameraPosition, target, 0.5f);
            blocker.transform.localScale = new Vector3(0.45f, 0.55f, 0.12f);
            Physics.SyncTransforms();
            OperationResult blocked =
                station.InspectInteractionGateForTests();
            Assert.That(blocked.Error,
                Is.EqualTo(CustomPcWorkTicketStationFailures.LineOfSightBlocked));
            AssertUnissued(session, inventoryRevision);
            Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();

            marker.PlayerMotor.SetPaused(true);
            OperationResult paused =
                station.InspectInteractionGateForTests();
            Assert.That(paused.Error,
                Is.EqualTo(CustomPcWorkTicketStationFailures.Paused));
            AssertUnissued(session, inventoryRevision);

            marker.PlayerMotor.SetPaused(false);
            MovePlayerToStation(marker, 1.35f);
            station.RefreshPresentation();
            Assert.That(station.IsFocused, Is.True);
            Assert.That(station.InspectInteractionGateForTests().IsSuccess, Is.True);
            AssertUnissued(session, inventoryRevision);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(out _), Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcWorkOrders.Revision, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator RuntimeCreatedSecondStationCannotBypassCanonicalPhysicalStation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            CustomPcWorkTicketStationProjection canonical =
                marker.CustomPcWorkTicketStation;
            long inventoryRevision = session.Inventory.Revision;

            MovePlayerToStation(marker, 1.35f);
            Vector3 target = canonical.InteractionCollider.bounds.center;
            Vector3 targetSize = canonical.InteractionCollider.bounds.size;
            canonical.enabled = false;
            canonical.InteractionCollider.enabled = false;

            GameObject fakeObject = new GameObject("RuntimeForgedWorkTicketStation");
            fakeObject.layer = LayerMask.NameToLayer("Interactable");
            fakeObject.transform.SetPositionAndRotation(
                target,
                canonical.InteractionCollider.transform.rotation);
            BoxCollider fakeCollider = fakeObject.AddComponent<BoxCollider>();
            fakeCollider.size = targetSize;
            fakeCollider.isTrigger = true;
            TextMesh fakeStatus = new GameObject("ForgedStationStatus")
                .AddComponent<TextMesh>();
            fakeStatus.transform.SetParent(fakeObject.transform, false);
            fakeStatus.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            CustomPcWorkTicketStationProjection fake =
                fakeObject.AddComponent<CustomPcWorkTicketStationProjection>();
            fake.Configure(
                marker.StockFlow,
                marker.PlayerInput,
                marker.PlayerMotor,
                canonical.PlayerCamera,
                marker.PlayerCarry,
                fakeCollider,
                fakeStatus);
            Physics.SyncTransforms();
            fake.RefreshPresentation();
            Assert.That(fake.IsFocused, Is.True);
            Assert.That(fake.InspectInteractionGateForTests().IsSuccess, Is.True);
            AssertUnissued(session, inventoryRevision);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            Assert.That(fake.LastFailureCode,
                Is.EqualTo(
                    CustomPcWorkTicketStationFailures.CanonicalStationMismatch.Code));
            AssertUnissued(session, inventoryRevision);

            Object.DestroyImmediate(fakeObject);
            canonical.InteractionCollider.enabled = true;
            canonical.enabled = true;
            Physics.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator KeyboardMouseCustomerToWorkTicketRoutePostsTicket()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);

            yield return RunCustomerToTicketRoute(
                marker,
                RouteDevice.KeyboardMouse,
                keyboard,
                mouse,
                null);
        }

        [UnityTest]
        public IEnumerator GamepadCustomerToWorkTicketRoutePostsTicket()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);

            yield return RunCustomerToTicketRoute(
                marker,
                RouteDevice.Gamepad,
                null,
                null,
                gamepad);
        }

        private enum RouteDevice
        {
            KeyboardMouse,
            Gamepad
        }

        private static IEnumerator RunCustomerToTicketRoute(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Mouse mouse,
            Gamepad gamepad)
        {
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PrepareCustomerRoutePrerequisites(marker, session);
            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            CustomPcWorkTicketStationProjection station =
                marker.CustomPcWorkTicketStation;
            long inventoryRevisionBeforeIssue;

            yield return WaitForCustomerState(
                customerFlow,
                PCShopEmpire3D.Actors.CustomerVisitState.Browsing);

            AssertPlayerStartsAtSceneSpawn(marker);
            RunCardinalCalibration(marker, device, keyboard, mouse, gamepad);

            DriveToWorldPoint(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                new Vector3(2.0f, 0.05f, -2.5f),
                65);
            DriveToWorldPoint(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                new Vector3(2.0f, 0.05f, -0.25f),
                70);
            DriveToWorldPoint(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                new Vector3(1.15f, 0.05f, -0.25f),
                40);
            DriveToWorldPoint(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                new Vector3(1.15f, 0.05f, 0.55f),
                40);
            AimAtWorldTarget(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                customerFlow.CustomerVisualRoot.transform.position +
                (Vector3.up * 1.35f));
            Physics.SyncTransforms();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.True);

            PressRouteInteractWithoutPhysicalMutation(
                marker,
                device,
                keyboard,
                gamepad,
                customerFlow,
                null,
                session,
                "customer-consultation");
            Assert.That(customerFlow.ConsultationCompleted, Is.True);
            yield return new WaitForFixedUpdate();

            PressRouteInteractWithoutPhysicalMutation(
                marker,
                device,
                keyboard,
                gamepad,
                customerFlow,
                null,
                session,
                "custom-pc-request");
            Assert.That(customerFlow.CustomPcRequestAccepted, Is.True);
            yield return new WaitForFixedUpdate();

            PressRouteInteractWithoutPhysicalMutation(
                marker,
                device,
                keyboard,
                gamepad,
                customerFlow,
                null,
                session,
                "custom-pc-quote");
            Assert.That(customerFlow.CustomPcQuoteReady, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(out _), Is.True);
            inventoryRevisionBeforeIssue = session.Inventory.Revision;
            Vector3 stationApproach = ResolveStationApproachPoint(
                station,
                1.55f);

            DriveToWorldPoint(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                new Vector3(1.15f, 0.05f, 2.15f),
                55);
            DriveToWorldPoint(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                new Vector3(
                    stationApproach.x,
                    0.05f,
                    2.15f),
                120);
            DriveToWorldPoint(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                stationApproach,
                70);
            AimAtWorldTarget(
                marker,
                device,
                keyboard,
                mouse,
                gamepad,
                station.InteractionCollider.bounds.center);
            Physics.SyncTransforms();
            station.RefreshPresentation();
            Assert.That(station.IsFocused, Is.True);
            Assert.That(station.CanIssue, Is.True);
            AssertUnissued(session, inventoryRevisionBeforeIssue);

            PressRouteInteractWithoutPhysicalMutation(
                marker,
                device,
                keyboard,
                gamepad,
                null,
                station,
                session,
                "physical-work-ticket");

            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.EqualTo(1));
            Assert.That(session.CustomPcWorkOrders.WorkTicketCount, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeIssue + 1));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PrepareCustomerRoutePrerequisites(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session)
        {
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();
        }

        private static void AssertPlayerStartsAtSceneSpawn(GaragePrototypeMarker marker)
        {
            Vector3 planar = Vector3.ProjectOnPlane(
                marker.PlayerMotor.transform.position -
                new Vector3(0f, 0.05f, -2.5f),
                Vector3.up);
            Assert.That(planar.magnitude, Is.LessThanOrEqualTo(0.10f),
                "The route test must start at the authored PlayerSpawn.");
        }

        private static void RunCardinalCalibration(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Mouse mouse,
            Gamepad gamepad)
        {
            const int FramesPerDirection = 3;
            Transform player = marker.PlayerMotor.transform;
            Vector3 forward = player.forward;
            Vector3 right = player.right;

            Vector3 start = player.position;
            StepMovementFrames(
                marker, device, keyboard, mouse, gamepad, Vector2.up,
                FramesPerDirection);
            Assert.That(Vector3.Dot(player.position - start, forward),
                Is.GreaterThan(0.05f), "W / left-stick up must move forward.");

            start = player.position;
            StepMovementFrames(
                marker, device, keyboard, mouse, gamepad, Vector2.down,
                FramesPerDirection);
            Assert.That(Vector3.Dot(player.position - start, forward),
                Is.LessThan(-0.05f), "S / left-stick down must move backward.");

            start = player.position;
            StepMovementFrames(
                marker, device, keyboard, mouse, gamepad, Vector2.left,
                FramesPerDirection);
            Assert.That(Vector3.Dot(player.position - start, right),
                Is.LessThan(-0.05f), "A / left-stick left must move left.");

            start = player.position;
            StepMovementFrames(
                marker, device, keyboard, mouse, gamepad, Vector2.right,
                FramesPerDirection);
            Assert.That(Vector3.Dot(player.position - start, right),
                Is.GreaterThan(0.05f), "D / left-stick right must move right.");

            ReleaseRouteInput(device, keyboard, mouse, gamepad);
        }

        private static void StepMovementFrames(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Mouse mouse,
            Gamepad gamepad,
            Vector2 move,
            int frameCount)
        {
            for (int frame = 0; frame < frameCount; frame++)
            {
                StepRouteFrame(
                    marker,
                    device,
                    keyboard,
                    mouse,
                    gamepad,
                    move,
                    Vector2.zero);
            }
        }

        private static void DriveToWorldPoint(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Mouse mouse,
            Gamepad gamepad,
            Vector3 worldTarget,
            int maximumFrames)
        {
            const float ArrivalTolerance = 0.18f;
            const float MinimumProgress = 0.005f;
            const int MaximumStagnantFrames = 30;
            Transform player = marker.PlayerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            int stagnantFrames = 0;
            float previousDistance = PlanarDistance(player.position, worldTarget);

            int frameBudget = maximumFrames +
                              (device == RouteDevice.Gamepad ? 45 : 0);
            for (int frame = 0; frame < frameBudget; frame++)
            {
                float distance = PlanarDistance(player.position, worldTarget);
                if (distance <= ArrivalTolerance)
                {
                    ReleaseRouteInput(device, keyboard, mouse, gamepad);
                    return;
                }

                Vector3 direction = Vector3.ProjectOnPlane(
                    worldTarget - player.position,
                    Vector3.up).normalized;
                float desiredYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float yawError = Mathf.DeltaAngle(player.eulerAngles.y, desiredYaw);
                float pitchError = -NormalizeAngle(cameraPivot.localEulerAngles.x);
                Vector2 look = ResolveLookInput(
                    marker,
                    device,
                    yawError,
                    pitchError);
                bool aligned = Mathf.Abs(yawError) <= 2f &&
                               Mathf.Abs(pitchError) <= 2f;
                StepRouteFrame(
                    marker,
                    device,
                    keyboard,
                    mouse,
                    gamepad,
                    aligned ? Vector2.up : Vector2.zero,
                    look);

                float currentDistance = PlanarDistance(player.position, worldTarget);
                stagnantFrames = aligned &&
                                 previousDistance - currentDistance < MinimumProgress
                    ? stagnantFrames + 1
                    : 0;
                Assert.That(stagnantFrames, Is.LessThan(MaximumStagnantFrames),
                    $"Real input route became blocked while approaching {worldTarget}.");
                previousDistance = currentDistance;
            }

            Assert.Fail(
                $"Real input route did not reach {worldTarget}; remaining distance " +
                $"{PlanarDistance(player.position, worldTarget):0.000} m after " +
                $"{frameBudget} frames.");
        }

        private static void AimAtWorldTarget(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Mouse mouse,
            Gamepad gamepad,
            Vector3 worldTarget)
        {
            int maximumFrames = device == RouteDevice.KeyboardMouse ? 90 : 150;
            Transform player = marker.PlayerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            Camera camera = marker.CustomPcWorkTicketStation.PlayerCamera;

            for (int frame = 0; frame < maximumFrames; frame++)
            {
                Vector3 direction = (worldTarget - camera.transform.position).normalized;
                Vector3 planarDirection = Vector3.ProjectOnPlane(direction, Vector3.up);
                float desiredYaw = Mathf.Atan2(
                    planarDirection.x,
                    planarDirection.z) * Mathf.Rad2Deg;
                float desiredPitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
                float yawError = Mathf.DeltaAngle(player.eulerAngles.y, desiredYaw);
                float pitchError = Mathf.DeltaAngle(
                    NormalizeAngle(cameraPivot.localEulerAngles.x),
                    desiredPitch);
                if (Mathf.Abs(yawError) <= 0.75f &&
                    Mathf.Abs(pitchError) <= 0.75f)
                {
                    ReleaseRouteInput(device, keyboard, mouse, gamepad);
                    return;
                }

                StepRouteFrame(
                    marker,
                    device,
                    keyboard,
                    mouse,
                    gamepad,
                    Vector2.zero,
                    ResolveLookInput(marker, device, yawError, pitchError));
            }

            Assert.Fail($"Real input look did not acquire target {worldTarget}.");
        }

        private static Vector2 ResolveLookInput(
            GaragePrototypeMarker marker,
            RouteDevice device,
            float yawError,
            float pitchError)
        {
            float scale = device == RouteDevice.KeyboardMouse
                ? marker.PlayerMotor.ViewSettings.MouseSensitivity
                : marker.PlayerMotor.ViewSettings.GamepadLookSpeed / 60f;
            float vertical = marker.PlayerMotor.ViewSettings.InvertY
                ? pitchError / scale
                : -pitchError / scale;
            float horizontal = yawError / scale;
            float limit = device == RouteDevice.KeyboardMouse ? 80f : 1f;
            return new Vector2(
                Mathf.Clamp(horizontal, -limit, limit),
                Mathf.Clamp(vertical, -limit, limit));
        }

        private static void StepRouteFrame(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Mouse mouse,
            Gamepad gamepad,
            Vector2 move,
            Vector2 look)
        {
            const float DeltaTime = 1f / 60f;
            if (device == RouteDevice.KeyboardMouse)
            {
                InputSystem.QueueStateEvent(keyboard, KeyboardStateForMove(move));
                InputSystem.QueueStateEvent(mouse, new MouseState { delta = look });
            }
            else
            {
                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState
                    {
                        leftStick = move,
                        rightStick = look
                    });
            }

            InputSystem.Update();
            marker.PlayerMotor.ProcessInputFrame(DeltaTime, DeltaTime);
        }

        private static KeyboardState KeyboardStateForMove(Vector2 move)
        {
            if (move.y > 0.5f)
            {
                return new KeyboardState(Key.W);
            }

            if (move.y < -0.5f)
            {
                return new KeyboardState(Key.S);
            }

            if (move.x < -0.5f)
            {
                return new KeyboardState(Key.A);
            }

            return move.x > 0.5f
                ? new KeyboardState(Key.D)
                : new KeyboardState();
        }

        private static void PressRouteInteract(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Gamepad gamepad,
            GarageCustomerFlowRuntime customerFlow,
            CustomPcWorkTicketStationProjection station)
        {
            if (device == RouteDevice.KeyboardMouse)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            }
            else
            {
                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.South
                    });
            }

            InputSystem.Update();
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True);
            if (customerFlow != null)
            {
                customerFlow.ProcessInputFrame();
            }
            else
            {
                station.ProcessInputFrame();
            }

            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False,
                "The focused physical interaction must be the single input consumer.");
            ReleaseRouteInput(device, keyboard, null, gamepad);
        }

        private static void PressRouteInteractWithoutPhysicalMutation(
            GaragePrototypeMarker marker,
            RouteDevice device,
            Keyboard keyboard,
            Gamepad gamepad,
            GarageCustomerFlowRuntime customerFlow,
            CustomPcWorkTicketStationProjection station,
            GarageStockFlowSession session,
            string stage)
        {
            Pose playerPose = new Pose(
                marker.PlayerMotor.transform.position,
                marker.PlayerMotor.transform.rotation);
            PhysicalItemPoseSnapshot[] physicalItems =
                CapturePhysicalItemPoses();
            InventoryItemRecord[] inventoryItems =
                session.Inventory.GetSerializedItems().ToArray();
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            PressRouteInteract(
                marker,
                device,
                keyboard,
                gamepad,
                customerFlow,
                station);

            Assert.That(
                Vector3.Distance(
                    marker.PlayerMotor.transform.position,
                    playerPose.position),
                Is.LessThan(0.0001f),
                $"{stage} must not teleport the player.");
            Assert.That(
                Quaternion.Angle(
                    marker.PlayerMotor.transform.rotation,
                    playerPose.rotation),
                Is.LessThan(0.01f),
                $"{stage} must not rotate the player automatically.");
            AssertPhysicalItemPosesUnchanged(physicalItems, stage);
            AssertInventoryItemsUnchanged(session, inventoryItems, stage);
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision),
                $"{stage} must not mutate Assembly revision.");
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount),
                $"{stage} must not publish an Assembly receipt.");
        }

        private static PhysicalItemPoseSnapshot[] CapturePhysicalItemPoses()
        {
            return Object
                .FindObjectsByType<PhysicalItemProjection>(FindObjectsSortMode.None)
                .OrderBy(item => item.ItemIdValue)
                .ThenBy(item => item.GetInstanceID())
                .Select(item => new PhysicalItemPoseSnapshot(item))
                .ToArray();
        }

        private static void AssertPhysicalItemPosesUnchanged(
            PhysicalItemPoseSnapshot[] expected,
            string stage)
        {
            PhysicalItemProjection[] current = Object
                .FindObjectsByType<PhysicalItemProjection>(FindObjectsSortMode.None)
                .OrderBy(item => item.ItemIdValue)
                .ThenBy(item => item.GetInstanceID())
                .ToArray();
            Assert.That(current.Length, Is.EqualTo(expected.Length),
                $"{stage} must not create or remove a physical item projection.");
            for (int index = 0; index < expected.Length; index++)
            {
                PhysicalItemPoseSnapshot snapshot = expected[index];
                PhysicalItemProjection item = current[index];
                Assert.That(item, Is.SameAs(snapshot.Item),
                    $"{stage} changed physical-item identity at index {index}.");
                Assert.That(item.transform.parent, Is.SameAs(snapshot.Parent),
                    $"{stage} reparented {item.ItemIdValue}.");
                Assert.That(
                    Vector3.Distance(item.transform.position, snapshot.Position),
                    Is.LessThan(0.0001f),
                    $"{stage} moved {item.ItemIdValue}.");
                Assert.That(
                    Quaternion.Angle(item.transform.rotation, snapshot.Rotation),
                    Is.LessThan(0.01f),
                    $"{stage} rotated {item.ItemIdValue}.");
                Assert.That(item.gameObject.activeSelf,
                    Is.EqualTo(snapshot.ActiveSelf),
                    $"{stage} changed active state for {item.ItemIdValue}.");
                Assert.That(item.IsCarried, Is.EqualTo(snapshot.IsCarried),
                    $"{stage} changed carry state for {item.ItemIdValue}.");
                Assert.That(item.IsMountedOnTransportCart,
                    Is.EqualTo(snapshot.IsMountedOnTransportCart),
                    $"{stage} changed cart state for {item.ItemIdValue}.");
                Assert.That(item.Ownership, Is.EqualTo(snapshot.Ownership),
                    $"{stage} changed ownership for {item.ItemIdValue}.");
                Assert.That(item.StackSupport, Is.SameAs(snapshot.StackSupport),
                    $"{stage} changed stack support for {item.ItemIdValue}.");
                Assert.That(item.StackedItem, Is.SameAs(snapshot.StackedItem),
                    $"{stage} changed stacked child for {item.ItemIdValue}.");
                Assert.That(item.IsStacked, Is.EqualTo(snapshot.IsStacked),
                    $"{stage} changed stacked state for {item.ItemIdValue}.");
                Assert.That(item.HasStackedItem,
                    Is.EqualTo(snapshot.HasStackedItem),
                    $"{stage} changed stack-child state for {item.ItemIdValue}.");
                Assert.That(item.IsStablePlacement,
                    Is.EqualTo(snapshot.IsStablePlacement),
                    $"{stage} changed stable-placement state for {item.ItemIdValue}.");
                Assert.That(
                    Vector3.Distance(item.LastSafePosition, snapshot.LastSafePosition),
                    Is.LessThan(0.0001f),
                    $"{stage} changed last-safe position for {item.ItemIdValue}.");
                Assert.That(
                    Quaternion.Angle(item.LastSafeRotation, snapshot.LastSafeRotation),
                    Is.LessThan(0.01f),
                    $"{stage} changed last-safe rotation for {item.ItemIdValue}.");
            }
        }

        private static void AssertInventoryItemsUnchanged(
            GarageStockFlowSession session,
            InventoryItemRecord[] expected,
            string stage)
        {
            InventoryItemRecord[] current =
                session.Inventory.GetSerializedItems().ToArray();
            Assert.That(current.Length, Is.EqualTo(expected.Length),
                $"{stage} must not create or remove serialized inventory items.");
            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(current[index].Id, Is.EqualTo(expected[index].Id));
                Assert.That(current[index].ProductId,
                    Is.EqualTo(expected[index].ProductId));
                Assert.That(current[index].ContainerId,
                    Is.EqualTo(expected[index].ContainerId),
                    $"{stage} moved inventory item {expected[index].Id.Value}.");
                Assert.That(current[index].Condition,
                    Is.EqualTo(expected[index].Condition));
                Assert.That(current[index].UnitCost,
                    Is.EqualTo(expected[index].UnitCost));
                Assert.That(current[index].StateFlags,
                    Is.EqualTo(expected[index].StateFlags));
            }
        }

        private sealed class PhysicalItemPoseSnapshot
        {
            public PhysicalItemPoseSnapshot(PhysicalItemProjection item)
            {
                Item = item;
                Parent = item.transform.parent;
                Position = item.transform.position;
                Rotation = item.transform.rotation;
                ActiveSelf = item.gameObject.activeSelf;
                IsCarried = item.IsCarried;
                IsMountedOnTransportCart = item.IsMountedOnTransportCart;
                Ownership = item.Ownership;
                StackSupport = item.StackSupport;
                StackedItem = item.StackedItem;
                IsStacked = item.IsStacked;
                HasStackedItem = item.HasStackedItem;
                IsStablePlacement = item.IsStablePlacement;
                LastSafePosition = item.LastSafePosition;
                LastSafeRotation = item.LastSafeRotation;
            }

            public PhysicalItemProjection Item { get; }

            public Transform Parent { get; }

            public Vector3 Position { get; }

            public Quaternion Rotation { get; }

            public bool ActiveSelf { get; }

            public bool IsCarried { get; }

            public bool IsMountedOnTransportCart { get; }

            public PhysicalItemOwnership Ownership { get; }

            public PhysicalItemProjection StackSupport { get; }

            public PhysicalItemProjection StackedItem { get; }

            public bool IsStacked { get; }

            public bool HasStackedItem { get; }

            public bool IsStablePlacement { get; }

            public Vector3 LastSafePosition { get; }

            public Quaternion LastSafeRotation { get; }
        }

        private static void ReleaseRouteInput(
            RouteDevice device,
            Keyboard keyboard,
            Mouse mouse,
            Gamepad gamepad)
        {
            if (device == RouteDevice.KeyboardMouse)
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                if (mouse != null)
                {
                    InputSystem.QueueStateEvent(mouse, new MouseState());
                }
            }
            else
            {
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
            }

            InputSystem.Update();
        }

        private static float PlanarDistance(Vector3 first, Vector3 second)
        {
            return Vector3.ProjectOnPlane(first - second, Vector3.up).magnitude;
        }

        private static float NormalizeAngle(float angle)
        {
            return Mathf.DeltaAngle(0f, angle);
        }

        private static IEnumerator LoadGarage(
            System.Action<GaragePrototypeMarker> assign)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            assign(marker);
        }

        private static IEnumerator PrepareQuote(
            GaragePrototypeMarker marker,
            Keyboard keyboard)
        {
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            yield return WaitForCustomerState(
                customerFlow,
                PCShopEmpire3D.Actors.CustomerVisitState.Browsing);
            MovePlayerToCustomer(marker, customerFlow);

            PressInteract(keyboard, customerFlow);
            ReleaseKeyboard(keyboard);
            yield return new WaitForFixedUpdate();
            PressInteract(keyboard, customerFlow);
            ReleaseKeyboard(keyboard);
            yield return new WaitForFixedUpdate();
            PressInteract(keyboard, customerFlow);
            ReleaseKeyboard(keyboard);
            yield return null;

            Assert.That(customerFlow.CustomPcQuoteReady, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(out _), Is.True);
        }

        private static void PressInteract(
            Keyboard keyboard,
            GarageCustomerFlowRuntime customerFlow)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            customerFlow.ProcessInputFrame();
        }

        private static void ReleaseKeyboard(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        private static void MovePlayerToCustomer(
            GaragePrototypeMarker marker,
            GarageCustomerFlowRuntime customerFlow)
        {
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
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

        private static void MovePlayerToStation(
            GaragePrototypeMarker marker,
            float distance)
        {
            CustomPcWorkTicketStationProjection station =
                marker.CustomPcWorkTicketStation;
            Vector3 target = station.InteractionCollider.bounds.center;
            Vector3 playerPosition = ResolveStationApproachPoint(
                station,
                distance);
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

        private static Vector3 ResolveStationApproachPoint(
            CustomPcWorkTicketStationProjection station,
            float distance)
        {
            Collider targetCollider = station.InteractionCollider;
            Vector3 approach = -targetCollider.transform.forward;
            approach.y = 0f;
            approach.Normalize();
            Vector3 playerPosition =
                targetCollider.bounds.center + (approach * distance);
            playerPosition.y = 0.05f;
            return playerPosition;
        }

        private static IEnumerator WaitForCustomerState(
            GarageCustomerFlowRuntime customerFlow,
            PCShopEmpire3D.Actors.CustomerVisitState expectedState)
        {
            const int MaximumFixedSteps = 650;
            for (int step = 0; step < MaximumFixedSteps; step++)
            {
                if (customerFlow.CurrentVisit?.State == expectedState)
                {
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            Assert.Fail($"Customer did not reach {expectedState}.");
        }

        private static void AssertUnissued(
            GarageStockFlowSession session,
            long inventoryRevision)
        {
            Assert.That(session.CustomPcWorkOrders.Revision, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkOrderCount, Is.Zero);
            Assert.That(session.CustomPcWorkOrders.WorkTicketCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }
    }
}
