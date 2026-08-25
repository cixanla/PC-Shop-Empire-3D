using System.Collections;
using System.Collections.Generic;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string CustomPcWorkTicketSmokeSuccessMarker =
            "GARAGE_CUSTOM_PC_WORK_TICKET_RUNTIME_SMOKE " +
            "work-order=immutable ticket=visible reservation-set=10 " +
            "allocation=atomic input=keyboard+gamepad fresh-press=ok " +
            "single-consumer=ok range=ok los=ok pause=ok replay=ok " +
            "duplicate=fail-closed items=unchanged assembly=untouched " +
            "presentation=ok invariants=ok";

        private IEnumerator RunCustomPcWorkTicketSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            CustomPcWorkTicketStationProjection station =
                customPcWorkTicketStation;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                customerFlow == null ||
                customerFlow.CustomerAgent == null ||
                customerFlow.CustomerVisualRoot == null ||
                customerFlow.StockFlow != stockFlow ||
                station == null ||
                station.StockFlow != stockFlow ||
                station.PlayerInput != playerInput ||
                station.PlayerMotor != playerMotor ||
                station.PlayerCarry != playerCarry ||
                station.PlayerCamera == null ||
                station.InteractionCollider == null ||
                station.StationStatusText == null)
            {
                LogCustomPcWorkTicketSmokeFailure("smoke.context-missing");
                yield break;
            }

            OperationResult acceptDelivery = session.AcceptArrivedDelivery();
            OperationResult shelfTransfer = session.TransferItem(
                session.ShelfContainerId);
            OperationResult publishOffer = session.PublishShelfOffer();
            stockFlow.RefreshPresentation();
            if (acceptDelivery.IsFailure ||
                shelfTransfer.IsFailure ||
                publishOffer.IsFailure ||
                !session.TryGetShelfOffer(out _))
            {
                string code = acceptDelivery.IsFailure
                    ? acceptDelivery.Error.Code
                    : shelfTransfer.IsFailure
                        ? shelfTransfer.Error.Code
                        : publishOffer.IsFailure
                            ? publishOffer.Error.Code
                            : "smoke.storefront-prerequisite-mismatch";
                LogCustomPcWorkTicketSmokeFailure(code);
                yield break;
            }

            const int MaximumBrowseSteps = 650;
            int browseSteps = 0;
            while (browseSteps < MaximumBrowseSteps)
            {
                CustomerVisitRecord candidate = customerFlow.CurrentVisit;
                if (candidate != null && candidate.State == CustomerVisitState.Browsing)
                {
                    break;
                }

                if (candidate != null && candidate.State == CustomerVisitState.Exited)
                {
                    LogCustomPcWorkTicketSmokeFailure(
                        "smoke.customer-exited-before-browse");
                    yield break;
                }

                browseSteps++;
                playerMotor.SetPaused(false);
                yield return new WaitForFixedUpdate();
            }

            if (customerFlow.CurrentVisit?.State != CustomerVisitState.Browsing)
            {
                LogCustomPcWorkTicketSmokeFailure("smoke.browse-route-mismatch");
                yield break;
            }

            MovePlayerToCustomPcCustomer();
            customerFlow.RefreshPresentation();
            OperationResult consultation = customerFlow.TryConsultCurrentCustomer();
            OperationResult acceptRequest = customerFlow.TryProgressCurrentCustomPc();
            OperationResult createQuote = customerFlow.TryProgressCurrentCustomPc();
            if (consultation.IsFailure ||
                acceptRequest.IsFailure ||
                createQuote.IsFailure ||
                !session.TryGetPrototypeCustomPcQuote(
                    out CustomPcQuoteRecord quote))
            {
                string code = consultation.IsFailure
                    ? consultation.Error.Code
                    : acceptRequest.IsFailure
                        ? acceptRequest.Error.Code
                        : createQuote.IsFailure
                            ? createQuote.Error.Code
                            : "smoke.quote-missing";
                LogCustomPcWorkTicketSmokeFailure(code);
                yield break;
            }

            station.RefreshPresentation();
            if (station.IsIssued ||
                !station.HasPendingAction ||
                !station.StationStatusText.text.Contains("10/10") ||
                !station.StationStatusText.text.Contains("İŞ EMRİNİ ÇIKAR"))
            {
                LogCustomPcWorkTicketSmokeFailure(
                    "smoke.ticket-presentation-prerequisite-mismatch");
                yield break;
            }

            long inventoryRevisionBefore = session.Inventory.Revision;
            long workOrderRevisionBefore = session.CustomPcWorkOrders.Revision;
            long assemblyRevisionBefore = session.AssemblyBuild.Revision;
            int assemblyReceiptCountBefore = session.AssemblyBuild.ReceiptCount;
            int serializedItemCountBefore = session.Inventory.SerializedItemCount;
            int reservationCountBefore = session.Inventory.ReservationCount;
            var originalContainers = new List<StableId<ContainerIdScope>>(
                quote.Lines.Count);
            foreach (CustomPcQuoteLineSnapshot line in quote.Lines)
            {
                if (!session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item))
                {
                    LogCustomPcWorkTicketSmokeFailure(
                        "smoke.reserved-item-missing");
                    yield break;
                }

                originalContainers.Add(item.ContainerId);
            }

            MovePlayerToCustomPcWorkTicketStation(
                station.InteractionRange + 0.65f);
            OperationResult outOfRange =
                station.InspectInteractionGateForTests();
            if (outOfRange.Error !=
                    CustomPcWorkTicketStationFailures.OutOfRange ||
                !CustomPcWorkTicketSmokeStateUnchanged(
                    session,
                    inventoryRevisionBefore,
                    workOrderRevisionBefore,
                    assemblyRevisionBefore,
                    assemblyReceiptCountBefore))
            {
                LogCustomPcWorkTicketSmokeFailure("smoke.range-gate-mismatch");
                yield break;
            }

            MovePlayerToCustomPcWorkTicketStation(1.35f);
            Vector3 cameraPosition = station.PlayerCamera.transform.position;
            Vector3 target = station.InteractionCollider.bounds.center;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "CustomPcWorkTicketNativeSmokeLosBlocker";
            blocker.transform.position = Vector3.Lerp(cameraPosition, target, 0.5f);
            blocker.transform.localScale = new Vector3(0.45f, 0.55f, 0.12f);
            Physics.SyncTransforms();
            OperationResult blocked =
                station.InspectInteractionGateForTests();
            bool losBlocked = blocked.Error ==
                              CustomPcWorkTicketStationFailures.LineOfSightBlocked &&
                              CustomPcWorkTicketSmokeStateUnchanged(
                                  session,
                                  inventoryRevisionBefore,
                                  workOrderRevisionBefore,
                                  assemblyRevisionBefore,
                                  assemblyReceiptCountBefore);
            Object.Destroy(blocker);
            yield return null;
            Physics.SyncTransforms();
            if (!losBlocked)
            {
                LogCustomPcWorkTicketSmokeFailure("smoke.los-gate-mismatch");
                yield break;
            }

            playerMotor.SetPaused(true);
            OperationResult paused =
                station.InspectInteractionGateForTests();
            playerMotor.SetPaused(false);
            if (paused.Error != CustomPcWorkTicketStationFailures.Paused ||
                !CustomPcWorkTicketSmokeStateUnchanged(
                    session,
                    inventoryRevisionBefore,
                    workOrderRevisionBefore,
                    assemblyRevisionBefore,
                    assemblyReceiptCountBefore))
            {
                LogCustomPcWorkTicketSmokeFailure("smoke.pause-gate-mismatch");
                yield break;
            }

            Keyboard smokeKeyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad smokeGamepad = InputSystem.AddDevice<Gamepad>();
            InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
            InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
            InputSystem.Update();

            MovePlayerToCustomPcWorkTicketStation(1.35f);
            Vector3 centeredTarget = station.InteractionCollider.bounds.center;
            Quaternion centeredLook = Quaternion.LookRotation(
                centeredTarget - station.PlayerCamera.transform.position,
                Vector3.up);
            Transform offTargetCameraPivot =
                playerMotor.transform.Find("CameraPivot");
            if (offTargetCameraPivot == null)
            {
                RemoveCustomPcWorkTicketSmokeDevices(smokeKeyboard, smokeGamepad);
                LogCustomPcWorkTicketSmokeFailure(
                    "smoke.camera-pivot-missing");
                yield break;
            }

            offTargetCameraPivot.rotation =
                Quaternion.AngleAxis(20f, Vector3.up) * centeredLook;
            Physics.SyncTransforms();
            station.RefreshPresentation();
            InputSystem.QueueStateEvent(
                smokeKeyboard,
                new KeyboardState(Key.E));
            InputSystem.Update();
            bool offTargetPressRemainedAvailable =
                station.HasContextualAttention &&
                !station.IsFocused &&
                playerInput.InteractPressedThisFrame;
            station.ProcessInputFrame();
            offTargetPressRemainedAvailable =
                offTargetPressRemainedAvailable &&
                playerInput.InteractPressedThisFrame &&
                CustomPcWorkTicketSmokeStateUnchanged(
                    session,
                    inventoryRevisionBefore,
                    workOrderRevisionBefore,
                    assemblyRevisionBefore,
                    assemblyReceiptCountBefore);
            InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
            InputSystem.Update();
            if (!offTargetPressRemainedAvailable)
            {
                RemoveCustomPcWorkTicketSmokeDevices(smokeKeyboard, smokeGamepad);
                LogCustomPcWorkTicketSmokeFailure(
                    "smoke.single-consumer-focus-mismatch");
                yield break;
            }

            MovePlayerToCustomPcWorkTicketStation(1.35f);
            station.RefreshPresentation();
            if (!station.IsFocused)
            {
                RemoveCustomPcWorkTicketSmokeDevices(smokeKeyboard, smokeGamepad);
                LogCustomPcWorkTicketSmokeFailure("smoke.focus-mismatch");
                yield break;
            }

            playerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                smokeKeyboard,
                new KeyboardState(Key.Escape, Key.E));
            yield return null;
            bool keyboardPauseCoedgeSafe =
                !playerMotor.IsPaused &&
                !playerInput.InteractPressedThisFrame &&
                CustomPcWorkTicketSmokeStateUnchanged(
                    session,
                    inventoryRevisionBefore,
                    workOrderRevisionBefore,
                    assemblyRevisionBefore,
                    assemblyReceiptCountBefore);
            InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
            yield return null;

            InputSystem.QueueStateEvent(
                smokeGamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.Start) |
                              (1u << (int)GamepadButton.South)
                });
            yield return null;
            bool gamepadPauseCoedgeSafe =
                playerMotor.IsPaused &&
                !playerInput.InteractPressedThisFrame &&
                CustomPcWorkTicketSmokeStateUnchanged(
                    session,
                    inventoryRevisionBefore,
                    workOrderRevisionBefore,
                    assemblyRevisionBefore,
                    assemblyReceiptCountBefore);
            InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
            yield return null;
            InputSystem.QueueStateEvent(
                smokeGamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.Start });
            yield return null;
            bool gamepadUnpausedWithoutIssue =
                !playerMotor.IsPaused &&
                CustomPcWorkTicketSmokeStateUnchanged(
                    session,
                    inventoryRevisionBefore,
                    workOrderRevisionBefore,
                    assemblyRevisionBefore,
                    assemblyReceiptCountBefore);
            InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
            yield return null;
            InputSystem.QueueStateEvent(
                smokeGamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            yield return null;
            CustomPcBuildOrderRecord buildOrder = null;
            CustomPcWorkTicketRecord workTicket = null;
            bool freshGamepadPressIssued =
                playerInput.UsesGamepadPrompts &&
                !playerInput.InteractPressedThisFrame &&
                session.TryGetPrototypeCustomPcBuildOrder(
                    out buildOrder) &&
                session.TryGetPrototypeCustomPcWorkTicket(
                    out workTicket);
            InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
            yield return null;
            RemoveCustomPcWorkTicketSmokeDevices(smokeKeyboard, smokeGamepad);

            bool inputLifecycle = keyboardPauseCoedgeSafe &&
                                  gamepadPauseCoedgeSafe &&
                                  gamepadUnpausedWithoutIssue &&
                                  freshGamepadPressIssued;
            if (!inputLifecycle)
            {
                LogCustomPcWorkTicketSmokeFailure(
                    "smoke.input-lifecycle-mismatch");
                yield break;
            }

            long committedInventoryRevision = session.Inventory.Revision;
            long committedWorkOrderRevision = session.CustomPcWorkOrders.Revision;
            OperationResult<CustomPcWorkOrderIssueResult> replay =
                session.ReplayIssuedPrototypeCustomPcWorkOrderForVerification(
                    quote.QuotedAt);
            OperationResult duplicate =
                station.InspectInteractionGateForTests();
            bool replaySafe = replay.IsSuccess &&
                              ReferenceEquals(replay.Value.BuildOrder, buildOrder) &&
                              ReferenceEquals(replay.Value.WorkTicket, workTicket) &&
                              session.Inventory.Revision == committedInventoryRevision &&
                              session.CustomPcWorkOrders.Revision ==
                                  committedWorkOrderRevision;
            bool duplicateFailClosed = duplicate.Error ==
                                       CustomPcWorkTicketStationFailures.AlreadyIssued &&
                                       session.Inventory.Revision ==
                                           committedInventoryRevision &&
                                       session.CustomPcWorkOrders.Revision ==
                                           committedWorkOrderRevision;

            bool exactReservationsAndItems =
                quote.Lines.Count == CustomPcQuoteAuthority.GraphicsFirstGamingLineCount &&
                buildOrder.ReservedSerializedItemCount == quote.Lines.Count &&
                workTicket.ReservedSerializedItemCount == quote.Lines.Count &&
                session.Inventory.SerializedItemCount == serializedItemCountBefore &&
                session.Inventory.ReservationCount == reservationCountBefore;
            for (int index = 0; index < quote.Lines.Count; index++)
            {
                CustomPcQuoteLineSnapshot line = quote.Lines[index];
                exactReservationsAndItems = exactReservationsAndItems &&
                    session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item) &&
                    item.ContainerId == originalContainers[index] &&
                    session.Inventory.TryGetReservation(
                        line.ReservationId,
                        out InventoryReservation reservation) &&
                    reservation.ItemId == line.ItemId &&
                    reservation.ClaimId == quote.InventoryClaimId &&
                    reservation.Quantity == 1;
            }

            station.RefreshPresentation();
            bool authorityIsolated =
                session.Inventory.Revision == inventoryRevisionBefore + 1 &&
                session.CustomPcWorkOrders.Revision ==
                    workOrderRevisionBefore + 1 &&
                session.AssemblyBuild.Revision == assemblyRevisionBefore &&
                session.AssemblyBuild.ReceiptCount == assemblyReceiptCountBefore &&
                !playerCarry.IsCarrying &&
                !playerCarry.IsDrivingCart &&
                playerCarry.HeldItem == null;
            bool immutableProjection =
                buildOrder.Status ==
                    CustomPcBuildOrderStatus.ReservationSetAllocated &&
                workTicket.Status ==
                    CustomPcWorkTicketStatus.PostedAtWorkbenchStation &&
                ReferenceEquals(workTicket.BuildOrder, buildOrder) &&
                buildOrder.SourceQuoteId == quote.Id &&
                buildOrder.InventoryClaimId == quote.InventoryClaimId &&
                buildOrder.WorkbenchContainerId == session.WorkbenchContainerId;
            GaragePrototypeHud hud = GetComponent<GaragePrototypeHud>();
            bool presentation = station.IsIssued &&
                                !station.HasPendingAction &&
                                IsCustomPcWorkTicketPlayerVisible(station) &&
                                station.StationStatusText.text.Contains(
                                    "DEMO-GAMING-001") &&
                                station.StationStatusText.text.Contains("10/10") &&
                                station.StationStatusText.text.Contains(
                                    "MONTAJA HAZIR") &&
                                station.StationStatusText.text.Contains(
                                    "HENÜZ BAŞLAMADI") &&
                                hud != null &&
                                hud.CustomPcWorkTicketStation == station &&
                                hud.EffectivePromptText.Contains("MONTAJA HAZIR");
            bool invariants = session.CustomPcWorkOrders
                                  .ValidateInvariants().IsSuccess &&
                              session.ValidateInvariants().IsSuccess;

            if (!inputLifecycle ||
                !replaySafe ||
                !duplicateFailClosed ||
                !exactReservationsAndItems ||
                !authorityIsolated ||
                !immutableProjection ||
                !presentation ||
                !invariants)
            {
                LogCustomPcWorkTicketSmokeFailure(
                    "smoke.final-invariant-mismatch");
                yield break;
            }

            Debug.Log(CustomPcWorkTicketSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
        }

        private static bool IsCustomPcWorkTicketPlayerVisible(
            CustomPcWorkTicketStationProjection station)
        {
            if (station == null ||
                !station.isActiveAndEnabled ||
                station.PlayerCamera == null ||
                station.StationStatusText == null ||
                !station.StationStatusText.gameObject.activeInHierarchy)
            {
                return false;
            }

            Renderer renderer = station.StationStatusText.GetComponent<Renderer>();
            Camera camera = station.PlayerCamera;
            if (!camera.isActiveAndEnabled ||
                renderer == null ||
                !renderer.enabled ||
                !renderer.gameObject.activeInHierarchy ||
                (camera.cullingMask & (1 << renderer.gameObject.layer)) == 0 ||
                renderer.bounds.size.sqrMagnitude <= Mathf.Epsilon)
            {
                return false;
            }

            Vector3 viewport = camera.WorldToViewportPoint(renderer.bounds.center);
            if (viewport.z <= 0f ||
                viewport.x < 0f || viewport.x > 1f ||
                viewport.y < 0f || viewport.y > 1f ||
                !GeometryUtility.TestPlanesAABB(
                    GeometryUtility.CalculateFrustumPlanes(camera),
                    renderer.bounds))
            {
                return false;
            }

            Vector3 textToCamera =
                camera.transform.position - station.StationStatusText.transform.position;
            if (textToCamera.sqrMagnitude <= Mathf.Epsilon ||
                Vector3.Dot(
                    -station.StationStatusText.transform.forward,
                    textToCamera.normalized) <= 0.1f)
            {
                return false;
            }

            Vector3 cameraToText =
                renderer.bounds.center - camera.transform.position;
            float distance = cameraToText.magnitude;
            return distance > Mathf.Epsilon &&
                   Physics.Raycast(
                       camera.transform.position,
                       cameraToText / distance,
                       out RaycastHit hit,
                       distance + 0.1f,
                       Physics.DefaultRaycastLayers,
                       QueryTriggerInteraction.Collide) &&
                   hit.collider == station.InteractionCollider;
        }

        private static void RemoveCustomPcWorkTicketSmokeDevices(
            Keyboard keyboard,
            Gamepad gamepad)
        {
            if (keyboard != null)
            {
                InputSystem.RemoveDevice(keyboard);
            }

            if (gamepad != null)
            {
                InputSystem.RemoveDevice(gamepad);
            }
        }

        /// <summary>
        /// Drives the shared BuildKit prerequisite through the same dynamic-frame input
        /// lifecycle as a native player. Queuing and immediately forcing an Input System
        /// update inside a running player can leave WasPressedThisFrame unavailable to the
        /// station's normal Update pass. A neutral settle frame, one pressed frame, and one
        /// released frame preserve the real single-consumer ordering.
        /// </summary>
        private IEnumerator RunBuildKitWorkTicketPhysicalInput(
            Keyboard keyboard,
            GarageStockFlowSession session,
            System.Action<string> captureFailure)
        {
            MovePlayerToCustomPcWorkTicketStation(1.35f);
            customPcWorkTicketStation.RefreshPresentation();

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            customPcWorkTicketStation.RefreshPresentation();
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.E));
            yield return null;

            captureFailure?.Invoke(
                ResolveBuildKitWorkTicketPhysicalInputFailure(session));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            customPcWorkTicketStation.RefreshPresentation();
        }

        private string ResolveBuildKitWorkTicketPhysicalInputFailure(
            GarageStockFlowSession session)
        {
            if (session != null &&
                session.TryGetPrototypeCustomPcBuildOrder(out _) &&
                session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                return string.Empty;
            }

            if (customPcWorkTicketStation == null)
            {
                return "smoke.work-ticket-station-missing";
            }

            OperationResult gate =
                customPcWorkTicketStation.InspectInteractionGateForTests();
            if (gate.IsFailure)
            {
                return "smoke.work-ticket-input-" + gate.Error.Code;
            }

            if (playerInput != null &&
                playerInput.InteractPressedThisFrame)
            {
                return "smoke.work-ticket-input-unconsumed";
            }

            return !string.IsNullOrEmpty(
                    customPcWorkTicketStation.LastFailureCode)
                ? "smoke.work-ticket-input-" +
                  customPcWorkTicketStation.LastFailureCode
                : "smoke.work-ticket-missing";
        }

        private void MovePlayerToCustomPcWorkTicketStation(float distance)
        {
            Collider targetCollider = customPcWorkTicketStation.InteractionCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 approach = -targetCollider.transform.forward;
            approach.y = 0f;
            if (approach.sqrMagnitude <= Mathf.Epsilon)
            {
                approach = Vector3.back;
            }

            approach.Normalize();
            Vector3 playerPosition = target + (approach * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;

            CharacterController controller =
                playerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            playerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static bool CustomPcWorkTicketSmokeStateUnchanged(
            GarageStockFlowSession session,
            long inventoryRevision,
            long workOrderRevision,
            long assemblyRevision,
            int assemblyReceiptCount)
        {
            return session.Inventory.Revision == inventoryRevision &&
                   session.CustomPcWorkOrders.Revision == workOrderRevision &&
                   session.CustomPcWorkOrders.WorkOrderCount == 0 &&
                   session.CustomPcWorkOrders.WorkTicketCount == 0 &&
                   session.AssemblyBuild.Revision == assemblyRevision &&
                   session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                   !session.TryGetPrototypeCustomPcWorkTicket(out _) &&
                   session.ValidateInvariants().IsSuccess;
        }

        private static void LogCustomPcWorkTicketSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_CUSTOM_PC_WORK_TICKET_RUNTIME_SMOKE " +
                $"work-ticket-flow=failed code={code}");
        }
    }
}
