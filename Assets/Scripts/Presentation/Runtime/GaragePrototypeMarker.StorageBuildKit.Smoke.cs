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
        public const string StorageBuildKitSmokeSuccessMarker =
            "GARAGE_STORAGE_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=motherboard+processor+memory-staged " +
            "storage-pickup=exact physical-identity=stable carry=ok " +
            "input=keyboard+mouse custody-guards=ok rotation=180 " +
            "placement=ok progress=4/10 reservation=alive " +
            "custody=storage-build-kit receipts=ok revisions=ok " +
            "assembly=untouched m2-slot=untouched no-duplicate-loss=ok " +
            "replay=ok invariants=ok";

        private IEnumerator RunStorageBuildKitSmoke()
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
            PhysicalItemProjection physicalMemoryPrerequisite =
                dimmBinding != null
                    ? dimmBinding.PhysicalItem
                    : null;
            PhysicalItemProjection physicalStorage =
                storageBinding != null
                    ? storageBinding.PhysicalItem
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
                physicalMemoryPrerequisite == null ||
                memoryModuleBuildKit == null ||
                storageBinding == null ||
                physicalStorage == null ||
                storageBuildKit == null ||
                storageSlot == null ||
                !HasMotherboardBuildKitR35Runtime ||
                !HasProcessorBuildKitR36Runtime ||
                !HasMemoryModuleBuildKitR37Runtime ||
                !HasStorageBuildKitR38Runtime)
            {
                LogStorageBuildKitSmokeFailure("smoke.context-missing");
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
                    LogStorageBuildKitSmokeFailure(code);
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
                        LogStorageBuildKitSmokeFailure(
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
                    LogStorageBuildKitSmokeFailure(
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
                    LogStorageBuildKitSmokeFailure(code);
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
                    LogStorageBuildKitSmokeFailure(
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
                CustomPcBuildOrderLineSnapshot memoryLine =
                    workOrder.Lines.SingleOrDefault(
                        line => line.ComponentKind ==
                            PcComponentKind.MemoryModule);
                CustomPcBuildOrderLineSnapshot storageLine =
                    workOrder.Lines.SingleOrDefault(
                        line => line.ComponentKind ==
                            PcComponentKind.StorageDevice);
                if (!StorageBuildKitSmokeReservationIsExact(
                        session,
                        workOrder,
                        motherboardLine,
                        processorLine,
                        memoryLine,
                        storageLine))
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.reservation-mismatch");
                    yield break;
                }

                long assemblyRevision = session.AssemblyBuild.Revision;
                int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
                StorageSlotState storageSlotState =
                    session.AssemblyBuild.StorageSlotState;
                bool storageFocusEnabled = storageSlot.FocusCollider.enabled;

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
                    LogStorageBuildKitSmokeFailure(
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
                    LogStorageBuildKitSmokeFailure(
                        "smoke.processor-prerequisite-mismatch");
                    yield break;
                }

                OperationResult memoryPickup =
                    playerCarry.TryPickup(physicalMemoryPrerequisite);
                MoveMemoryModuleBuildKitSmokePlayerToKit(memoryModuleBuildKit);
                OperationResult memoryMode =
                    playerCarry.TrySetMemoryModuleBuildKitMode(true);
                OperationResult memoryPlacement =
                    playerCarry.TryConfirmMemoryModuleBuildKit();
                if (memoryPickup.IsFailure ||
                    memoryMode.IsFailure ||
                    memoryPlacement.IsFailure ||
                    playerCarry.HeldItem != null ||
                    !memoryModuleBuildKit.IsStaged ||
                    memoryModuleBuildKit.StagedComponentCount != 3 ||
                    !dimmBinding.IsAuthorityInBuildKit ||
                    !storageBuildKit.HasMotherboardPrerequisite ||
                    !storageBuildKit.HasProcessorPrerequisite ||
                    !storageBuildKit.HasMemoryModulePrerequisite ||
                    storageBuildKit.StagedComponentCount != 3 ||
                    session.AssemblyBuild.Revision != assemblyRevision ||
                    session.AssemblyBuild.ReceiptCount != assemblyReceiptCount)
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.memory-prerequisite-mismatch");
                    yield break;
                }

                int storageIdentity = physicalStorage.GetInstanceID();
                int serializedItemCount = session.Inventory.SerializedItemCount;
                long inventoryRevisionBeforePickup = session.Inventory.Revision;
                long buildKitRevisionBeforePickup =
                    session.CustomPcBuildKit.Revision;

                AimMotherboardBuildKitSmokeAtItem(
                    physicalStorage,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalStorage)
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.storage-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalStorage &&
                    physicalStorage.GetInstanceID() == storageIdentity &&
                    storageBinding.IsAuthorityInHands &&
                    storageBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeStorageBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.StorageInHands &&
                    ReferenceEquals(pickupReceipt.Line, storageLine) &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.StorageSlotState == storageSlotState &&
                    storageSlot.FocusCollider.enabled == storageFocusEnabled &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.storage-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool custodyGuard =
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code &&
                    StorageBuildKitSmokeHeldStateUnchanged(
                        session,
                        physicalStorage,
                        inventoryRevisionInHands,
                        buildKitRevisionInHands,
                        assemblyRevision,
                        assemblyReceiptCount,
                        storageSlotState,
                        storageFocusEnabled);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!custodyGuard)
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.storage-custody-guard-mismatch");
                    yield break;
                }

                MoveStorageBuildKitSmokePlayerToKit(storageBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool storageModeValid =
                    playerCarry.IsStorageBuildKitMode &&
                    !playerCarry.IsM2StorageSeatMode &&
                    playerCarry.CurrentStorageBuildKitStatus ==
                        StorageBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("3/10 → 4/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!storageModeValid)
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.storage-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentStorageBuildKitStatus ==
                        StorageBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    storageBuildKit.IsStaged &&
                    storageBuildKit.StagedComponentCount == 4 &&
                    storageBuildKit.ProgressText.text.Contains("4/10") &&
                    storageBuildKit.ProgressText.text.Contains(
                        "NVMe HAZIR") &&
                    storageBinding.IsAuthorityInBuildKit &&
                    physicalStorage.GetInstanceID() == storageIdentity &&
                    physicalStorage.Ownership == PhysicalItemOwnership.World &&
                    physicalStorage.IsStablePlacement &&
                    storageBuildKit.MatchesCommittedPlacement(
                        physicalStorage) &&
                    Quaternion.Angle(
                        physicalStorage.transform.rotation,
                        storageBuildKit.ResolveSnapPose(1).rotation) <= 0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeStorageBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.StorageStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, storageLine) &&
                    session.TryGetStorageItem(out InventoryItemRecord stagedStorage) &&
                    stagedStorage.ContainerId ==
                        session.StorageBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        storageLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedStorage.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision == inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.StorageSlotState == storageSlotState &&
                    storageSlot.FocusCollider.enabled == storageFocusEnabled &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    !playerCarry.IsM2StorageSeatMode &&
                    CountCanonicalStorageProjections(
                        session.StorageItemId.Value) == 1 &&
                    session.Inventory.SerializedItemCount ==
                        serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.StorageBuildKitContainerId).Value == 1 &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!exactPlacement)
                {
                    LogStorageBuildKitSmokeFailure(
                        "smoke.storage-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalStorage(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    storageBuildKit.StagedComponentCount == 4 &&
                    physicalStorage.GetInstanceID() == storageIdentity &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.StorageSlotState == storageSlotState &&
                    storageSlot.FocusCollider.enabled == storageFocusEnabled &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    CountCanonicalStorageProjections(
                        session.StorageItemId.Value) == 1 &&
                    session.Inventory.SerializedItemCount ==
                        serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.StorageBuildKitContainerId).Value == 1 &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogStorageBuildKitSmokeFailure("smoke.replay-mismatch");
                    yield break;
                }

                Debug.Log(StorageBuildKitSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(
                    smokeKeyboard,
                    smokeMouse);
            }
        }

        private static bool StorageBuildKitSmokeReservationIsExact(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            CustomPcBuildOrderLineSnapshot motherboardLine,
            CustomPcBuildOrderLineSnapshot processorLine,
            CustomPcBuildOrderLineSnapshot memoryLine,
            CustomPcBuildOrderLineSnapshot storageLine)
        {
            return motherboardLine != null &&
                   motherboardLine.ItemId == session.MotherboardItemId &&
                   processorLine != null &&
                   processorLine.ItemId == session.ProcessorItemId &&
                   memoryLine != null &&
                   memoryLine.ItemId == session.MemoryItemId &&
                   storageLine != null &&
                   storageLine.ItemId == session.StorageItemId &&
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
                   memoryReservation.ClaimId == workOrder.InventoryClaimId &&
                   session.Inventory.TryGetReservation(
                       storageLine.ReservationId,
                       out InventoryReservation storageReservation) &&
                   storageReservation.ItemId == storageLine.ItemId &&
                   storageReservation.ClaimId == workOrder.InventoryClaimId;
        }

        private bool StorageBuildKitSmokeHeldStateUnchanged(
            GarageStockFlowSession session,
            PhysicalItemProjection physicalStorage,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount,
            StorageSlotState storageSlotState,
            bool storageFocusEnabled)
        {
            return playerCarry.HeldItem == physicalStorage &&
                   physicalStorage.IsCarried &&
                   storageBinding.IsAuthorityInHands &&
                   storageBuildKit.StagedComponentCount == 3 &&
                   session.Inventory.Revision == inventoryRevision &&
                   session.CustomPcBuildKit.Revision == buildKitRevision &&
                   session.AssemblyBuild.Revision == assemblyRevision &&
                   session.AssemblyBuild.StorageSlotState == storageSlotState &&
                   storageSlot.FocusCollider.enabled == storageFocusEnabled &&
                   session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
        }

        private void MoveStorageBuildKitSmokePlayerToKit(
            StorageBuildKitProjection buildKit)
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

        private static void LogStorageBuildKitSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_STORAGE_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
