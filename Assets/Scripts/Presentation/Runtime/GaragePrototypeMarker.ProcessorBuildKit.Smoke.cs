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
        public const string ProcessorBuildKitSmokeSuccessMarker =
            "GARAGE_PROCESSOR_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisite=motherboard-staged " +
            "processor-pickup=exact " +
            "physical-identity=stable carry=ok input=keyboard+mouse " +
            "custody-guards=ok rotation=ok placement=ok progress=2/10 " +
            "reservation=alive custody=processor-build-kit receipts=ok " +
            "revisions=ok assembly=untouched processor-socket=untouched " +
            "no-duplicate-loss=ok replay=ok invariants=ok";

        private IEnumerator RunProcessorBuildKitSmoke()
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
            PhysicalItemProjection physicalProcessor =
                processorBinding != null
                    ? processorBinding.PhysicalItem
                    : null;
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
                motherboardBuildKit == null ||
                processorBinding == null ||
                physicalProcessor == null ||
                processorBuildKit == null ||
                processorSocket == null ||
                !HasMotherboardBuildKitR35Runtime ||
                !HasProcessorBuildKitR36Runtime)
            {
                LogProcessorBuildKitSmokeFailure("smoke.context-missing");
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
                    LogProcessorBuildKitSmokeFailure(code);
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
                        LogProcessorBuildKitSmokeFailure(
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
                    LogProcessorBuildKitSmokeFailure(
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
                    LogProcessorBuildKitSmokeFailure(code);
                    yield break;
                }

                MovePlayerToCustomPcWorkTicketStation(1.35f);
                customPcWorkTicketStation.RefreshPresentation();
                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                InputSystem.Update();
                customPcWorkTicketStation.ProcessInputFrame();
                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState());
                InputSystem.Update();
                customPcWorkTicketStation.ProcessInputFrame();

                if (!session.TryGetPrototypeCustomPcBuildOrder(
                        out CustomPcBuildOrderRecord workOrder) ||
                    !session.TryGetPrototypeCustomPcWorkTicket(out _))
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.work-ticket-missing");
                    yield break;
                }

                CustomPcBuildOrderLineSnapshot motherboardLine =
                    workOrder.Lines.SingleOrDefault(
                        line => line.ComponentKind ==
                            PcComponentKind.Motherboard);
                CustomPcBuildOrderLineSnapshot processorLine =
                    workOrder.Lines.SingleOrDefault(
                        line => line.ComponentKind ==
                            PcComponentKind.Processor);
                if (motherboardLine == null ||
                    motherboardLine.ItemId != session.MotherboardItemId ||
                    processorLine == null ||
                    processorLine.ItemId != session.ProcessorItemId ||
                    !session.Inventory.TryGetReservation(
                        motherboardLine.ReservationId,
                        out InventoryReservation motherboardReservation) ||
                    motherboardReservation.ItemId != motherboardLine.ItemId ||
                    !session.Inventory.TryGetReservation(
                        processorLine.ReservationId,
                        out InventoryReservation processorReservation) ||
                    processorReservation.ItemId != processorLine.ItemId)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.reservation-mismatch");
                    yield break;
                }

                long assemblyRevision = session.AssemblyBuild.Revision;
                int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
                int motherboardIdentity = physicalMotherboard.GetInstanceID();

                AimMotherboardBuildKitSmokeAtItem(
                    physicalMotherboard,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool motherboardPickupValid =
                    playerCarry.HeldItem == physicalMotherboard &&
                    motherboardBinding.IsAuthorityInHands &&
                    motherboardBuildKit.HasPickupReceipt;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!motherboardPickupValid)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.motherboard-pickup-mismatch");
                    yield break;
                }

                MoveMotherboardBuildKitSmokePlayerToKit(motherboardBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool motherboardModeValid =
                    playerCarry.IsMotherboardBuildKitMode &&
                    playerCarry.CurrentMotherboardBuildKitStatus ==
                        MotherboardBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!motherboardModeValid)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.motherboard-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool motherboardStaged =
                    playerCarry.HeldItem == null &&
                    motherboardBuildKit.IsStaged &&
                    motherboardBuildKit.StagedComponentCount == 1 &&
                    motherboardBuildKit.ProgressText.text.Contains("1/10") &&
                    motherboardBinding.IsAuthorityInBuildKit &&
                    physicalMotherboard.GetInstanceID() == motherboardIdentity &&
                    physicalMotherboard.IsStablePlacement &&
                    motherboardBuildKit.MatchesCommittedPlacement(
                        physicalMotherboard) &&
                    processorBuildKit.HasMotherboardPrerequisite &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!motherboardStaged)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.motherboard-placement-mismatch");
                    yield break;
                }

                int processorIdentity = physicalProcessor.GetInstanceID();
                int serializedItemCount = session.Inventory.SerializedItemCount;
                long inventoryRevisionBeforePickup = session.Inventory.Revision;
                long buildKitRevisionBeforePickup =
                    session.CustomPcBuildKit.Revision;

                AimMotherboardBuildKitSmokeAtItem(
                    physicalProcessor,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalProcessor)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.processor-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalProcessor &&
                    physicalProcessor.GetInstanceID() == processorIdentity &&
                    processorBinding.IsAuthorityInHands &&
                    processorBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeProcessorBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.ProcessorInHands &&
                    ReferenceEquals(pickupReceipt.Line, processorLine) &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.EmptyOpen &&
                    processorSocket.MatchesAuthorityState(
                        AssemblySeatState.Empty,
                        ProcessorSocketState.EmptyOpen) &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.processor-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool custodyGuard =
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code &&
                    ProcessorBuildKitSmokeHeldStateUnchanged(
                        session,
                        physicalProcessor,
                        inventoryRevisionInHands,
                        buildKitRevisionInHands,
                        assemblyRevision,
                        assemblyReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!custodyGuard)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.processor-custody-guard-mismatch");
                    yield break;
                }

                MoveProcessorBuildKitSmokePlayerToKit(processorBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool processorModeValid =
                    playerCarry.IsProcessorBuildKitMode &&
                    !playerCarry.IsProcessorSeatMode &&
                    playerCarry.CurrentProcessorBuildKitStatus ==
                        ProcessorBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("1/10 → 2/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!processorModeValid)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.processor-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentProcessorBuildKitStatus ==
                        ProcessorBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    processorBuildKit.IsStaged &&
                    processorBuildKit.StagedComponentCount == 2 &&
                    processorBuildKit.ProgressText.text.Contains("2/10") &&
                    processorBuildKit.ProgressText.text.Contains(
                        "İŞLEMCİ HAZIR") &&
                    processorBinding.IsAuthorityInBuildKit &&
                    physicalProcessor.GetInstanceID() == processorIdentity &&
                    physicalProcessor.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalProcessor.IsStablePlacement &&
                    processorBuildKit.MatchesCommittedPlacement(
                        physicalProcessor) &&
                    Quaternion.Angle(
                        physicalProcessor.transform.rotation,
                        processorBuildKit.ResolveSnapPose(1).rotation) <= 0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeProcessorBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.ProcessorStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, processorLine) &&
                    session.Inventory.TryGetSerializedItem(
                        processorLine.ItemId,
                        out InventoryItemRecord stagedProcessor) &&
                    stagedProcessor.ContainerId ==
                        session.ProcessorBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        processorLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedProcessor.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision == inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.EmptyOpen &&
                    processorSocket.MatchesAuthorityState(
                        AssemblySeatState.Empty,
                        ProcessorSocketState.EmptyOpen) &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    !playerCarry.IsProcessorSeatMode &&
                    CountCanonicalProcessorProjections(
                        session.ProcessorItemId.Value) == 1 &&
                    session.Inventory.SerializedItemCount ==
                        serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.ProcessorBuildKitContainerId).Value == 1 &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!exactPlacement)
                {
                    LogProcessorBuildKitSmokeFailure(
                        "smoke.processor-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision = session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalProcessor(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    processorBuildKit.StagedComponentCount == 2 &&
                    physicalProcessor.GetInstanceID() == processorIdentity &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.EmptyOpen &&
                    processorSocket.MatchesAuthorityState(
                        AssemblySeatState.Empty,
                        ProcessorSocketState.EmptyOpen) &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    CountCanonicalProcessorProjections(
                        session.ProcessorItemId.Value) == 1 &&
                    session.Inventory.SerializedItemCount ==
                        serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.ProcessorBuildKitContainerId).Value == 1 &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogProcessorBuildKitSmokeFailure("smoke.replay-mismatch");
                    yield break;
                }

                Debug.Log(ProcessorBuildKitSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(
                    smokeKeyboard,
                    smokeMouse);
            }
        }

        private bool ProcessorBuildKitSmokeHeldStateUnchanged(
            GarageStockFlowSession session,
            PhysicalItemProjection physicalProcessor,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount)
        {
            return playerCarry.HeldItem == physicalProcessor &&
                   physicalProcessor.IsCarried &&
                   processorBinding.IsAuthorityInHands &&
                   processorBuildKit.StagedComponentCount == 1 &&
                   session.Inventory.Revision == inventoryRevision &&
                   session.CustomPcBuildKit.Revision == buildKitRevision &&
                   session.AssemblyBuild.Revision == assemblyRevision &&
                   session.AssemblyBuild.ProcessorSocketState ==
                       ProcessorSocketState.EmptyOpen &&
                   session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
        }

        private void MoveProcessorBuildKitSmokePlayerToKit(
            ProcessorBuildKitProjection buildKit)
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

        private static void LogProcessorBuildKitSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_PROCESSOR_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
