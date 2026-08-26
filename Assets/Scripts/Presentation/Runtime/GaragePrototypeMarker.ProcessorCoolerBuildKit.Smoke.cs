using System.Collections;
using System.Linq;
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
        public const string ProcessorCoolerBuildKitSmokeSuccessMarker =
            "GARAGE_PROCESSOR_COOLER_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok " +
            "prerequisites=motherboard+processor+memory+storage-staged " +
            "processor-cooler-pickup=exact physical-identity=stable " +
            "carry=ok input=keyboard+mouse custody-guards=ok rotation=90 " +
            "placement=ok progress=5/10 reservation=alive " +
            "custody=processor-cooler-build-kit receipts=ok revisions=ok " +
            "assembly=untouched processor-cooler-slot=untouched " +
            "tim=untouched no-duplicate-loss=ok replay=ok invariants=ok";

        private bool _suppressStorageBuildKitSmokeSuccessMarker;
        private string _nestedStorageBuildKitSmokeFailureCode;
        private bool _suppressProcessorCoolerBuildKitSmokeSuccessMarker;
        private string _nestedProcessorCoolerBuildKitSmokeFailureCode;

        private IEnumerator RunProcessorCoolerBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedStorageBuildKitSmokeFailureCode = null;
            _suppressStorageBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunStorageBuildKitSmoke();
            }
            finally
            {
                _suppressStorageBuildKitSmokeSuccessMarker = false;
            }

            string storageBuildKitFailureCode =
                _nestedStorageBuildKitSmokeFailureCode;
            _nestedStorageBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(storageBuildKitFailureCode))
            {
                const string SmokePrefix = "smoke.";
                string failureSuffix = storageBuildKitFailureCode.StartsWith(
                    SmokePrefix,
                    System.StringComparison.Ordinal)
                        ? storageBuildKitFailureCode.Substring(SmokePrefix.Length)
                        : storageBuildKitFailureCode;
                LogProcessorCoolerBuildKitSmokeFailure(
                    $"smoke.storage-prerequisite-{failureSuffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection physicalCooler =
                processorCoolerBinding != null
                    ? processorCoolerBinding.PhysicalItem
                    : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                processorCoolerBinding == null ||
                physicalCooler == null ||
                processorCoolerBuildKit == null ||
                processorCoolerSlot == null ||
                !HasProcessorCoolerBuildKitR39Runtime ||
                !motherboardBuildKit.IsStaged ||
                !processorBuildKit.IsStaged ||
                !memoryModuleBuildKit.IsStaged ||
                !storageBuildKit.IsStaged ||
                storageBuildKit.StagedComponentCount != 4 ||
                !processorCoolerBuildKit.HasMotherboardPrerequisite ||
                !processorCoolerBuildKit.HasProcessorPrerequisite ||
                !processorCoolerBuildKit.HasMemoryModulePrerequisite ||
                !processorCoolerBuildKit.HasStoragePrerequisite ||
                processorCoolerBuildKit.StagedComponentCount != 4)
            {
                LogProcessorCoolerBuildKitSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogProcessorCoolerBuildKitSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot coolerLine =
                workOrder.Lines.SingleOrDefault(
                    line => line.ComponentKind == PcComponentKind.ProcessorCooler);
            if (coolerLine == null ||
                coolerLine.ItemId != session.ProcessorCoolerItemId ||
                !session.Inventory.TryGetReservation(
                    coolerLine.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.ItemId != coolerLine.ItemId ||
                reservation.ClaimId != workOrder.InventoryClaimId)
            {
                LogProcessorCoolerBuildKitSmokeFailure(
                    "smoke.reservation-mismatch");
                yield break;
            }

            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int physicalIdentity = physicalCooler.GetInstanceID();
            int serializedItemCount = session.Inventory.SerializedItemCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup =
                session.CustomPcBuildKit.Revision;

            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeMouse = InputSystem.AddDevice<Mouse>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.Update();

                AimMotherboardBuildKitSmokeAtItem(
                    physicalCooler,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCooler)
                {
                    LogProcessorCoolerBuildKitSmokeFailure(
                        "smoke.processor-cooler-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalCooler &&
                    physicalCooler.GetInstanceID() == physicalIdentity &&
                    processorCoolerBinding.IsAuthorityInHands &&
                    processorCoolerBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeProcessorCoolerBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.ProcessorCoolerInHands &&
                    ReferenceEquals(pickupReceipt.Line, coolerLine) &&
                    pickupReceipt.Line.LineId == coolerLine.LineId &&
                    pickupReceipt.Line.ProductId == coolerLine.ProductId &&
                    pickupReceipt.Line.ItemId == coolerLine.ItemId &&
                    pickupReceipt.Line.ReservationId == coolerLine.ReservationId &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    ProcessorCoolerBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogProcessorCoolerBuildKitSmokeFailure(
                        "smoke.processor-cooler-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands =
                    session.CustomPcBuildKit.Revision;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool custodyGuard =
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code &&
                    playerCarry.HeldItem == physicalCooler &&
                    physicalCooler.IsCarried &&
                    processorCoolerBinding.IsAuthorityInHands &&
                    processorCoolerBuildKit.StagedComponentCount == 4 &&
                    session.Inventory.Revision == inventoryRevisionInHands &&
                    session.CustomPcBuildKit.Revision == buildKitRevisionInHands &&
                    ProcessorCoolerBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!custodyGuard)
                {
                    LogProcessorCoolerBuildKitSmokeFailure(
                        "smoke.processor-cooler-custody-guard-mismatch");
                    yield break;
                }

                MoveProcessorCoolerBuildKitSmokePlayerToKit(
                    processorCoolerBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool modeValid =
                    playerCarry.IsProcessorCoolerBuildKitMode &&
                    !playerCarry.IsProcessorCoolerSeatMode &&
                    playerCarry.CurrentProcessorCoolerBuildKitStatus ==
                        ProcessorCoolerBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("4/10 → 5/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!modeValid)
                {
                    LogProcessorCoolerBuildKitSmokeFailure(
                        "smoke.processor-cooler-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentProcessorCoolerBuildKitStatus ==
                        ProcessorCoolerBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogProcessorCoolerBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    processorCoolerBuildKit.IsStaged &&
                    processorCoolerBuildKit.StagedComponentCount == 5 &&
                    processorCoolerBuildKit.ProgressText.text.Contains("5/10") &&
                    processorCoolerBuildKit.ProgressText.text.Contains(
                        "SOĞUTUCU HAZIR") &&
                    processorCoolerBinding.IsAuthorityInBuildKit &&
                    physicalCooler.GetInstanceID() == physicalIdentity &&
                    physicalCooler.Ownership == PhysicalItemOwnership.World &&
                    physicalCooler.IsStablePlacement &&
                    processorCoolerBuildKit.MatchesCommittedPlacement(
                        physicalCooler) &&
                    Quaternion.Angle(
                        physicalCooler.transform.rotation,
                        processorCoolerBuildKit.ResolveSnapPose(1).rotation) <= 0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeProcessorCoolerBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.ProcessorCoolerStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, coolerLine) &&
                    session.TryGetProcessorCoolerItem(
                        out InventoryItemRecord stagedCooler) &&
                    stagedCooler.ContainerId ==
                        session.ProcessorCoolerBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        coolerLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedCooler.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision == inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.ProcessorCoolerBuildKitContainerId).Value == 1 &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    CountCanonicalProcessorCoolerProjections(
                        session.ProcessorCoolerItemId.Value) == 1 &&
                    ProcessorCoolerBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount) &&
                    processorCoolerBinding.ValidateProjectionInvariant().IsSuccess &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!exactPlacement)
                {
                    LogProcessorCoolerBuildKitSmokeFailure(
                        "smoke.processor-cooler-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalProcessorCooler(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    processorCoolerBuildKit.StagedComponentCount == 5 &&
                    physicalCooler.GetInstanceID() == physicalIdentity &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.ProcessorCoolerBuildKitContainerId).Value == 1 &&
                    ProcessorCoolerBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount) &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogProcessorCoolerBuildKitSmokeFailure(
                        "smoke.replay-mismatch");
                    yield break;
                }

                if (!_suppressProcessorCoolerBuildKitSmokeSuccessMarker)
                {
                    Debug.Log(ProcessorCoolerBuildKitSmokeSuccessMarker);
                }
                yield return new WaitForEndOfFrame();
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(
                    smokeKeyboard,
                    smokeMouse);
            }
        }

        private static bool ProcessorCoolerBuildKitSmokeAssemblyUnchanged(
            GarageStockFlowSession session,
            AssemblyBuildSnapshot expected,
            int expectedReceiptCount)
        {
            AssemblyBuildSnapshot actual = session.AssemblyBuild.GetSnapshot();
            return actual.Revision == expected.Revision &&
                   session.AssemblyBuild.ReceiptCount == expectedReceiptCount &&
                   actual.ProcessorCoolerSlotState ==
                       expected.ProcessorCoolerSlotState &&
                   actual.ProcessorCoolerTimState ==
                       expected.ProcessorCoolerTimState &&
                   actual.ProcessorCoolerItemId ==
                       expected.ProcessorCoolerItemId &&
                   actual.ProcessorCoolerProductId ==
                       expected.ProcessorCoolerProductId &&
                   actual.ProcessorCoolerMountOrientation ==
                       expected.ProcessorCoolerMountOrientation &&
                   actual.ProcessorCoolerSeatedByOperationId ==
                       expected.ProcessorCoolerSeatedByOperationId &&
                   actual.ProcessorCoolerRetainedByOperationId ==
                       expected.ProcessorCoolerRetainedByOperationId;
        }

        private void MoveProcessorCoolerBuildKitSmokePlayerToKit(
            ProcessorCoolerBuildKitProjection buildKit)
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

        private void LogProcessorCoolerBuildKitSmokeFailure(string code)
        {
            if (_suppressProcessorCoolerBuildKitSmokeSuccessMarker)
            {
                _nestedProcessorCoolerBuildKitSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_PROCESSOR_COOLER_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
