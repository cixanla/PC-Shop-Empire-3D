using System.Collections;
using System.Linq;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string MotherboardBuildKitSmokeSuccessMarker =
            "GARAGE_MOTHERBOARD_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok pickup=exact physical-identity=stable carry=ok " +
            "input=keyboard+mouse custody-guards=ok rotation=ok placement=ok " +
            "progress=1/10 reservation=alive custody=build-kit " +
            "assembly=untouched replay=ok invariants=ok";

        private IEnumerator RunMotherboardBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection physicalMotherboard =
                motherboardBinding != null
                    ? motherboardBinding.PhysicalItem
                    : null;
            MotherboardBuildKitProjection buildKit = motherboardBuildKit;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                customerFlow == null ||
                customerFlow.CustomerAgent == null ||
                customerFlow.CustomerVisualRoot == null ||
                customPcWorkTicketStation == null ||
                motherboardBinding == null ||
                physicalMotherboard == null ||
                buildKit == null ||
                motherboardSeat == null ||
                !HasMotherboardBuildKitR35Runtime)
            {
                LogMotherboardBuildKitSmokeFailure("smoke.context-missing");
                yield break;
            }

            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeMouse = InputSystem.AddDevice<Mouse>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.Update();

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
                    LogMotherboardBuildKitSmokeFailure(code);
                    yield break;
                }

                const int MaximumBrowseSteps = 650;
                int browseSteps = 0;
                while (browseSteps < MaximumBrowseSteps &&
                       customerFlow.CurrentVisit?.State !=
                           CustomerVisitState.Browsing)
                {
                    if (customerFlow.CurrentVisit?.State ==
                        CustomerVisitState.Exited)
                    {
                        LogMotherboardBuildKitSmokeFailure(
                            "smoke.customer-exited-before-browse");
                        yield break;
                    }

                    browseSteps++;
                    playerMotor.SetPaused(false);
                    yield return new WaitForFixedUpdate();
                }

                if (customerFlow.CurrentVisit?.State !=
                    CustomerVisitState.Browsing)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.browse-route-mismatch");
                    yield break;
                }

                MovePlayerToCustomPcCustomer();
                customerFlow.RefreshPresentation();
                OperationResult consultation =
                    customerFlow.TryConsultCurrentCustomer();
                OperationResult acceptRequest =
                    customerFlow.TryProgressCurrentCustomPc();
                OperationResult createQuote =
                    customerFlow.TryProgressCurrentCustomPc();
                if (consultation.IsFailure ||
                    acceptRequest.IsFailure ||
                    createQuote.IsFailure ||
                    !session.TryGetPrototypeCustomPcQuote(out _))
                {
                    string code = consultation.IsFailure
                        ? consultation.Error.Code
                        : acceptRequest.IsFailure
                            ? acceptRequest.Error.Code
                            : createQuote.IsFailure
                                ? createQuote.Error.Code
                                : "smoke.quote-missing";
                    LogMotherboardBuildKitSmokeFailure(code);
                    yield break;
                }

                string workTicketInputFailure = null;
                yield return RunBuildKitWorkTicketPhysicalInput(
                    smokeKeyboard,
                    session,
                    code => workTicketInputFailure = code);

                if (!session.TryGetPrototypeCustomPcBuildOrder(
                        out CustomPcBuildOrderRecord workOrder) ||
                    !session.TryGetPrototypeCustomPcWorkTicket(out _))
                {
                    LogMotherboardBuildKitSmokeFailure(
                        string.IsNullOrEmpty(workTicketInputFailure)
                            ? "smoke.work-ticket-missing"
                            : workTicketInputFailure);
                    yield break;
                }

                CustomPcBuildOrderLineSnapshot motherboardLine =
                    workOrder.Lines.SingleOrDefault(
                        line => line.ComponentKind ==
                            PcComponentKind.Motherboard);
                if (motherboardLine == null ||
                    motherboardLine.ItemId != session.MotherboardItemId ||
                    !session.Inventory.TryGetReservation(
                        motherboardLine.ReservationId,
                        out InventoryReservation initialReservation) ||
                    initialReservation.ItemId != motherboardLine.ItemId)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.motherboard-reservation-mismatch");
                    yield break;
                }

                int physicalIdentity = physicalMotherboard.GetInstanceID();
                long assemblyRevision = session.AssemblyBuild.Revision;
                int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
                long inventoryRevisionBeforePickup = session.Inventory.Revision;

                AimMotherboardBuildKitSmokeAtItem(
                    physicalMotherboard,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalMotherboard)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.motherboard-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalMotherboard &&
                    physicalMotherboard.GetInstanceID() == physicalIdentity &&
                    motherboardBinding.IsAuthorityInHands &&
                    buildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeCustomPcBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.MotherboardInHands &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.exact-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool genericDropGuard =
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code &&
                    MotherboardBuildKitSmokeHeldStateUnchanged(
                        session,
                        physicalMotherboard,
                        inventoryRevisionInHands,
                        buildKitRevisionInHands,
                        assemblyRevision,
                        assemblyReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!genericDropGuard)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.generic-drop-guard-mismatch");
                    yield break;
                }

                MoveMotherboardBuildKitSmokePlayerToSeat();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool seatModeValid = playerCarry.IsMotherboardSeatMode &&
                                     playerCarry.CurrentMotherboardSeatStatus ==
                                         MotherboardSeatStatus.Valid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!seatModeValid)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.seat-bypass-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool seatBypassGuard =
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.InventoryTransferRejected.Code &&
                    MotherboardBuildKitSmokeHeldStateUnchanged(
                        session,
                        physicalMotherboard,
                        inventoryRevisionInHands,
                        buildKitRevisionInHands,
                        assemblyRevision,
                        assemblyReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!seatBypassGuard)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.seat-bypass-guard-mismatch");
                    yield break;
                }

                MoveMotherboardBuildKitSmokePlayerToKit(buildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool buildKitModeValid =
                    playerCarry.IsMotherboardBuildKitMode &&
                    playerCarry.CurrentMotherboardBuildKitStatus ==
                        MotherboardBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!buildKitModeValid)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.build-kit-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentMotherboardBuildKitStatus ==
                        MotherboardBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    buildKit.IsStaged &&
                    buildKit.StagedComponentCount == 1 &&
                    buildKit.ProgressText.text.Contains("1/10") &&
                    buildKit.ProgressText.text.Contains("ANAKART HAZIR") &&
                    motherboardBinding.IsAuthorityInBuildKit &&
                    physicalMotherboard.GetInstanceID() == physicalIdentity &&
                    physicalMotherboard.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalMotherboard.IsStablePlacement &&
                    buildKit.MatchesCommittedPlacement(physicalMotherboard) &&
                    Quaternion.Angle(
                        physicalMotherboard.transform.rotation,
                        buildKit.ResolveSnapPose(1).rotation) <= 0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeCustomPcBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.MotherboardStaged &&
                    session.Inventory.TryGetSerializedItem(
                        motherboardLine.ItemId,
                        out InventoryItemRecord stagedItem) &&
                    stagedItem.ContainerId ==
                        session.CustomPcBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        motherboardLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedItem.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision == inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.Empty &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!exactPlacement)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.exact-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision = session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalMotherboard(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    buildKit.StagedComponentCount == 1 &&
                    physicalMotherboard.GetInstanceID() == physicalIdentity &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogMotherboardBuildKitSmokeFailure(
                        "smoke.replay-mismatch");
                    yield break;
                }

                Debug.Log(MotherboardBuildKitSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(
                    smokeKeyboard,
                    smokeMouse);
            }
        }

        private bool MotherboardBuildKitSmokeHeldStateUnchanged(
            GarageStockFlowSession session,
            PhysicalItemProjection physicalMotherboard,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount)
        {
            return playerCarry.HeldItem == physicalMotherboard &&
                   physicalMotherboard.IsCarried &&
                   motherboardBinding.IsAuthorityInHands &&
                   motherboardBuildKit.StagedComponentCount == 0 &&
                   session.Inventory.Revision == inventoryRevision &&
                   session.CustomPcBuildKit.Revision == buildKitRevision &&
                   session.AssemblyBuild.Revision == assemblyRevision &&
                   session.AssemblyBuild.MotherboardSeatState ==
                       AssemblySeatState.Empty &&
                   session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
        }

        private void PressMotherboardBuildKitSmokeKey(
            Keyboard keyboard,
            Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
            playerCarry.ProcessInputFrame();
        }

        private void ReleaseMotherboardBuildKitSmokeKeyboard(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            playerCarry.ProcessInputFrame();
        }

        private void PressMotherboardBuildKitSmokeMouse(Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            playerCarry.ProcessInputFrame();
        }

        private void ReleaseMotherboardBuildKitSmokeMouse(Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            playerCarry.ProcessInputFrame();
        }

        private void AimMotherboardBuildKitSmokeAtItem(
            PhysicalItemProjection item,
            Vector3 approachDirection)
        {
            Vector3 target = item.InteractionCenter;
            Vector3 playerPosition = target +
                                     (approachDirection.normalized * 1.25f);
            playerPosition.y = 0.05f;
            SetMotherboardBuildKitSmokePlayerLook(playerPosition, target);
        }

        private void MoveMotherboardBuildKitSmokePlayerToKit(
            MotherboardBuildKitProjection buildKit)
        {
            Collider support = buildKit.SupportCollider;
            Vector3 target = new Vector3(
                support.bounds.center.x,
                support.bounds.max.y,
                support.bounds.center.z);
            Vector3 playerPosition = target + (Vector3.back * 0.95f);
            playerPosition.y = 0.05f;
            SetMotherboardBuildKitSmokePlayerLook(playerPosition, target);
        }

        private void MoveMotherboardBuildKitSmokePlayerToSeat()
        {
            Vector3 target = motherboardSeat.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetMotherboardBuildKitSmokePlayerLook(playerPosition, target);
        }

        private void SetMotherboardBuildKitSmokePlayerLook(
            Vector3 playerPosition,
            Vector3 target)
        {
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            CharacterController controller =
                playerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            playerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Camera playerCamera =
                playerMotor.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.identity;
            }

            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static void RemoveMotherboardBuildKitSmokeDevices(
            Keyboard keyboard,
            Mouse mouse)
        {
            if (keyboard != null)
            {
                InputSystem.RemoveDevice(keyboard);
            }

            if (mouse != null)
            {
                InputSystem.RemoveDevice(mouse);
            }
        }

        private static void LogMotherboardBuildKitSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_MOTHERBOARD_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
