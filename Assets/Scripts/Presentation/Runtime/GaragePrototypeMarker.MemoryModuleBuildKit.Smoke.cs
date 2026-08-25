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
        public const string MemoryModuleBuildKitSmokeSuccessMarker =
            "GARAGE_MEMORY_MODULE_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=motherboard+processor-staged " +
            "memory-pickup=exact physical-identity=stable carry=ok " +
            "input=keyboard+mouse custody-guards=ok rotation=180 " +
            "placement=ok progress=3/10 reservation=alive " +
            "custody=memory-module-build-kit receipts=ok revisions=ok " +
            "assembly=untouched dimm-a2=untouched no-duplicate-loss=ok " +
            "replay=ok invariants=ok";

        private IEnumerator RunMemoryModuleBuildKitSmoke()
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
            PhysicalItemProjection physicalMemory =
                dimmBinding != null
                    ? dimmBinding.PhysicalItem
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
                dimmBinding == null ||
                physicalMemory == null ||
                memoryModuleBuildKit == null ||
                dimmSlot == null ||
                !HasMotherboardBuildKitR35Runtime ||
                !HasProcessorBuildKitR36Runtime ||
                !HasMemoryModuleBuildKitR37Runtime)
            {
                LogMemoryModuleBuildKitSmokeFailure("smoke.context-missing");
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
                    LogMemoryModuleBuildKitSmokeFailure(code);
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
                        LogMemoryModuleBuildKitSmokeFailure(
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
                    LogMemoryModuleBuildKitSmokeFailure(
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
                    LogMemoryModuleBuildKitSmokeFailure(code);
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
                    LogMemoryModuleBuildKitSmokeFailure(
                        string.IsNullOrEmpty(workTicketInputFailure)
                            ? "smoke.work-ticket-missing"
                            : workTicketInputFailure);
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
                CustomPcBuildOrderLineSnapshot memoryLine =
                    workOrder.Lines.SingleOrDefault(
                        line => line.ComponentKind ==
                            PcComponentKind.MemoryModule);
                if (!MemoryModuleBuildKitSmokeReservationIsExact(
                        session,
                        workOrder,
                        motherboardLine,
                        processorLine,
                        memoryLine))
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.reservation-mismatch");
                    yield break;
                }

                long assemblyRevision = session.AssemblyBuild.Revision;
                int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
                MemorySlotState memorySlotState =
                    session.AssemblyBuild.MemorySlotState;
                DimmLatchVisualPhase latchPhase = dimmSlot.LatchVisualPhase;

                OperationResult motherboardPickup =
                    playerCarry.TryPickup(physicalMotherboard);
                MoveMotherboardBuildKitSmokePlayerToKit(motherboardBuildKit);
                OperationResult motherboardMode =
                    playerCarry.TrySetMotherboardBuildKitMode(true);
                OperationResult motherboardPlacement =
                    playerCarry.TryConfirmMotherboardBuildKit();
                if (motherboardPickup.IsFailure ||
                    motherboardMode.IsFailure ||
                    motherboardPlacement.IsFailure ||
                    playerCarry.HeldItem != null ||
                    !motherboardBuildKit.IsStaged ||
                    motherboardBuildKit.StagedComponentCount != 1 ||
                    !motherboardBinding.IsAuthorityInBuildKit)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.motherboard-prerequisite-mismatch");
                    yield break;
                }

                OperationResult processorPickup =
                    playerCarry.TryPickup(physicalProcessor);
                MoveProcessorBuildKitSmokePlayerToKit(processorBuildKit);
                OperationResult processorMode =
                    playerCarry.TrySetProcessorBuildKitMode(true);
                OperationResult processorPlacement =
                    playerCarry.TryConfirmProcessorBuildKit();
                if (processorPickup.IsFailure ||
                    processorMode.IsFailure ||
                    processorPlacement.IsFailure ||
                    playerCarry.HeldItem != null ||
                    !processorBuildKit.IsStaged ||
                    processorBuildKit.StagedComponentCount != 2 ||
                    !processorBinding.IsAuthorityInBuildKit ||
                    !memoryModuleBuildKit.HasMotherboardPrerequisite ||
                    !memoryModuleBuildKit.HasProcessorPrerequisite ||
                    memoryModuleBuildKit.StagedComponentCount != 2 ||
                    session.AssemblyBuild.Revision != assemblyRevision ||
                    session.AssemblyBuild.ReceiptCount != assemblyReceiptCount)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.processor-prerequisite-mismatch");
                    yield break;
                }

                int memoryIdentity = physicalMemory.GetInstanceID();
                int serializedItemCount = session.Inventory.SerializedItemCount;
                long inventoryRevisionBeforePickup = session.Inventory.Revision;
                long buildKitRevisionBeforePickup =
                    session.CustomPcBuildKit.Revision;

                AimMotherboardBuildKitSmokeAtItem(
                    physicalMemory,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalMemory)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.memory-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalMemory &&
                    physicalMemory.GetInstanceID() == memoryIdentity &&
                    dimmBinding.IsAuthorityInHands &&
                    memoryModuleBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeMemoryModuleBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.MemoryModuleInHands &&
                    ReferenceEquals(pickupReceipt.Line, memoryLine) &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.MemorySlotState == memorySlotState &&
                    dimmSlot.LatchVisualPhase == latchPhase &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.memory-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool custodyGuard =
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code &&
                    MemoryModuleBuildKitSmokeHeldStateUnchanged(
                        session,
                        physicalMemory,
                        inventoryRevisionInHands,
                        buildKitRevisionInHands,
                        assemblyRevision,
                        assemblyReceiptCount,
                        memorySlotState,
                        latchPhase);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!custodyGuard)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.memory-custody-guard-mismatch");
                    yield break;
                }

                MoveMemoryModuleBuildKitSmokePlayerToKit(memoryModuleBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool memoryModeValid =
                    playerCarry.IsMemoryModuleBuildKitMode &&
                    !playerCarry.IsDimmSeatMode &&
                    playerCarry.CurrentMemoryModuleBuildKitStatus ==
                        MemoryModuleBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("2/10 → 3/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!memoryModeValid)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.memory-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentMemoryModuleBuildKitStatus ==
                        MemoryModuleBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    memoryModuleBuildKit.IsStaged &&
                    memoryModuleBuildKit.StagedComponentCount == 3 &&
                    memoryModuleBuildKit.ProgressText.text.Contains("3/10") &&
                    memoryModuleBuildKit.ProgressText.text.Contains(
                        "BELLEK HAZIR") &&
                    dimmBinding.IsAuthorityInBuildKit &&
                    physicalMemory.GetInstanceID() == memoryIdentity &&
                    physicalMemory.Ownership == PhysicalItemOwnership.World &&
                    physicalMemory.IsStablePlacement &&
                    memoryModuleBuildKit.MatchesCommittedPlacement(
                        physicalMemory) &&
                    Quaternion.Angle(
                        physicalMemory.transform.rotation,
                        memoryModuleBuildKit.ResolveSnapPose(1).rotation) <= 0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeMemoryModuleBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.MemoryModuleStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, memoryLine) &&
                    session.TryGetMemoryItem(out InventoryItemRecord stagedMemory) &&
                    stagedMemory.ContainerId ==
                        session.MemoryModuleBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        memoryLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedMemory.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision == inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.MemorySlotState == memorySlotState &&
                    dimmSlot.LatchVisualPhase == latchPhase &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    !playerCarry.IsDimmSeatMode &&
                    CountCanonicalMemoryProjections(
                        session.MemoryItemId.Value) == 1 &&
                    session.Inventory.SerializedItemCount ==
                        serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.MemoryModuleBuildKitContainerId).Value == 1 &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!exactPlacement)
                {
                    LogMemoryModuleBuildKitSmokeFailure(
                        "smoke.memory-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalMemoryModule(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    memoryModuleBuildKit.StagedComponentCount == 3 &&
                    physicalMemory.GetInstanceID() == memoryIdentity &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.MemorySlotState == memorySlotState &&
                    dimmSlot.LatchVisualPhase == latchPhase &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    CountCanonicalMemoryProjections(
                        session.MemoryItemId.Value) == 1 &&
                    session.Inventory.SerializedItemCount ==
                        serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.MemoryModuleBuildKitContainerId).Value == 1 &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogMemoryModuleBuildKitSmokeFailure("smoke.replay-mismatch");
                    yield break;
                }

                Debug.Log(MemoryModuleBuildKitSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(
                    smokeKeyboard,
                    smokeMouse);
            }
        }

        private static bool MemoryModuleBuildKitSmokeReservationIsExact(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            CustomPcBuildOrderLineSnapshot motherboardLine,
            CustomPcBuildOrderLineSnapshot processorLine,
            CustomPcBuildOrderLineSnapshot memoryLine)
        {
            return motherboardLine != null &&
                   motherboardLine.ItemId == session.MotherboardItemId &&
                   processorLine != null &&
                   processorLine.ItemId == session.ProcessorItemId &&
                   memoryLine != null &&
                   memoryLine.ItemId == session.MemoryItemId &&
                   session.Inventory.TryGetReservation(
                       motherboardLine.ReservationId,
                       out InventoryReservation motherboardReservation) &&
                   motherboardReservation.ItemId == motherboardLine.ItemId &&
                   motherboardReservation.ClaimId == workOrder.InventoryClaimId &&
                   session.Inventory.TryGetReservation(
                       processorLine.ReservationId,
                       out InventoryReservation processorReservation) &&
                   processorReservation.ItemId == processorLine.ItemId &&
                   processorReservation.ClaimId == workOrder.InventoryClaimId &&
                   session.Inventory.TryGetReservation(
                       memoryLine.ReservationId,
                       out InventoryReservation memoryReservation) &&
                   memoryReservation.ItemId == memoryLine.ItemId &&
                   memoryReservation.ClaimId == workOrder.InventoryClaimId;
        }

        private bool MemoryModuleBuildKitSmokeHeldStateUnchanged(
            GarageStockFlowSession session,
            PhysicalItemProjection physicalMemory,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount,
            MemorySlotState memorySlotState,
            DimmLatchVisualPhase latchPhase)
        {
            return playerCarry.HeldItem == physicalMemory &&
                   physicalMemory.IsCarried &&
                   dimmBinding.IsAuthorityInHands &&
                   memoryModuleBuildKit.StagedComponentCount == 2 &&
                   session.Inventory.Revision == inventoryRevision &&
                   session.CustomPcBuildKit.Revision == buildKitRevision &&
                   session.AssemblyBuild.Revision == assemblyRevision &&
                   session.AssemblyBuild.MemorySlotState == memorySlotState &&
                   dimmSlot.LatchVisualPhase == latchPhase &&
                   session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
        }

        private void MoveMemoryModuleBuildKitSmokePlayerToKit(
            MemoryModuleBuildKitProjection buildKit)
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

        private static void LogMemoryModuleBuildKitSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_MEMORY_MODULE_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
