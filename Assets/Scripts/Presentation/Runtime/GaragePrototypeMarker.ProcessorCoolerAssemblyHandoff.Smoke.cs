using System;
using System.Collections;
using System.Collections.Generic;
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
        public const string ProcessorCoolerAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_PROCESSOR_COOLER_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 motherboard=secured " +
            "processor=retained memory=retained storage=secured pickup=exact " +
            "custody=build-kit-to-hands-to-cooler-slot reservation=alive " +
            "physical-identity=stable input=keyboard+mouse orientation=180 " +
            "tim=consumed-once four-point=1-3-2-4 reverse=4-2-3-1 " +
            "retained-remove-blocked=ok detach=ok consumed-tim-reseat-blocked=ok " +
            "history=10/10-preserved other-five=untouched receipts=ok " +
            "revisions=ok no-duplicate-loss=ok invariants=ok";

        public bool HasProcessorCoolerAssemblyHandoffR49Runtime =>
            HasStorageAssemblyHandoffR48Runtime &&
            processorCooler != null &&
            processorCoolerBinding != null &&
            processorCoolerBuildKit != null &&
            processorCoolerSlot != null &&
            memoryModule != null &&
            storageDevice != null &&
            processorCoolerBinding.PhysicalItem == processorCooler &&
            processorCoolerBinding.BuildKit == processorCoolerBuildKit &&
            processorCoolerBinding.Slot == processorCoolerSlot &&
            processorCoolerSlot.ClearanceBlockers.Length == 1 &&
            processorCoolerSlot.ClearanceBlockers[0] ==
                memoryModule.GetComponent<Collider>() &&
            HasProcessorCoolerStorageClearanceAtSeat() &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypeProcessorCoolerAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeProcessorCoolerBuildKitOperationId.Value &&
            stockFlow.Session.PrototypeProcessorCoolerAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeStorageAssemblyHandoffOperationId.Value &&
            stockFlow.Session.ProcessorCoolerSlotContainerId !=
                stockFlow.Session.ProcessorCoolerBuildKitContainerId;

        private IEnumerator RunProcessorCoolerAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedStorageAssemblyHandoffSmokeFailureCode = null;
            _suppressStorageAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunStorageAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressStorageAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedStorageAssemblyHandoffSmokeFailureCode;
            _nestedStorageAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogProcessorCoolerAssemblyHandoffSmokeFailure(
                    $"smoke.storage-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection cooler = processorCoolerBinding != null
                ? processorCoolerBinding.PhysicalItem
                : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                cooler == null ||
                !HasProcessorCoolerAssemblyHandoffR49Runtime ||
                !processorCoolerBuildKit.IsStaged ||
                !processorCoolerBinding.IsAuthorityInBuildKit ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 4 ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.ProcessorRetained ||
                session.AssemblyBuild.MemorySlotState !=
                    MemorySlotState.MemoryModuleRetained ||
                session.AssemblyBuild.StorageSlotState !=
                    StorageSlotState.StorageDeviceSecured ||
                session.AssemblyBuild.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.EmptyOpen)
            {
                LogProcessorCoolerAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogProcessorCoolerAssemblyHandoffSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot motherboardLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.Motherboard);
            CustomPcBuildOrderLineSnapshot processorLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.Processor);
            CustomPcBuildOrderLineSnapshot memoryLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.MemoryModule);
            CustomPcBuildOrderLineSnapshot storageLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.StorageDevice);
            CustomPcBuildOrderLineSnapshot coolerLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.ProcessorCooler);
            if (motherboardLine == null ||
                processorLine == null ||
                memoryLine == null ||
                storageLine == null ||
                coolerLine == null ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                !TryCaptureProcessorCoolerAssemblyHandoffOtherContainers(
                    session,
                    workOrder,
                    out Dictionary<StableId<ItemInstanceIdScope>,
                        StableId<ContainerIdScope>> otherContainers) ||
                !ProcessorCoolerAssemblyHandoffReservationsAreLive(
                    session,
                    workOrder,
                    motherboardLine,
                    processorLine,
                    memoryLine,
                    storageLine,
                    coolerLine))
            {
                LogProcessorCoolerAssemblyHandoffSmokeFailure(
                    "smoke.reservation-or-history-mismatch");
                yield break;
            }

            int coolerPhysicalIdentity = cooler.GetInstanceID();
            string coolerItemIdentity = cooler.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeMouse = InputSystem.AddDevice<Mouse>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.Update();

                processorCoolerBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(cooler, -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != cooler ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains("4 NOKTALI MONTAJA AL"))
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitAssemblyHandoffReceipt coolerHandoff = null;
                bool pickedUp =
                    playerCarry.HeldItem == cooler &&
                    processorCoolerBinding.IsAuthorityInHands &&
                    processorCoolerBuildKit.IsReleasedForAssembly &&
                    processorCoolerBuildKit.StagedComponentCount == 10 &&
                    processorCoolerBuildKit.ProgressText.text.Contains(
                        "SOĞUTUCU MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeProcessorCoolerAssemblyHandoffOperationId,
                        out coolerHandoff) &&
                    coolerHandoff.ComponentKind ==
                        PcComponentKind.ProcessorCooler &&
                    ReferenceEquals(coolerHandoff.Line, coolerLine) &&
                    ReferenceEquals(
                        coolerHandoff.StagingReceipt,
                        historicalReceipts[4]) &&
                    cooler.GetInstanceID() == coolerPhysicalIdentity &&
                    cooler.ItemIdValue == coolerItemIdentity;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-build-kit-pickup-mismatch");
                    yield break;
                }

                long immediateReplayInventoryRevision = session.Inventory.Revision;
                long immediateReplayBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
                    immediateReplay =
                        session.PickupStagedProcessorCoolerForAssembly();
                if (immediateReplay.IsFailure ||
                    !ReferenceEquals(immediateReplay.Value, coolerHandoff) ||
                    session.Inventory.Revision !=
                        immediateReplayInventoryRevision ||
                    session.CustomPcBuildKit.Revision !=
                        immediateReplayBuildKitRevision)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.immediate-replay-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool worldDropBlocked =
                    playerCarry.HeldItem == cooler &&
                    processorCoolerBinding.IsAuthorityInHands &&
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!worldDropBlocked)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.reserved-world-drop-not-blocked");
                    yield break;
                }

                MovePlayerToProcessorCoolerSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool seatReady =
                    playerCarry.IsProcessorCoolerSeatMode &&
                    playerCarry.CurrentProcessorCoolerSlotStatus ==
                        ProcessorCoolerSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!seatReady)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotated =
                    playerCarry.CurrentProcessorCoolerSlotStatus ==
                        ProcessorCoolerSlotStatus.ValidSeat &&
                    processorCoolerSlot.LastEvaluation.Orientation ==
                        ProcessorCoolerMountOrientation.Rotated180;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotated)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                Pose expectedSeatPose = processorCoolerSlot.ResolveSeatPose(
                    ProcessorCoolerMountOrientation.Rotated180).Value;
                bool seated =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.CoolerSeatedUnsecured &&
                    session.AssemblyBuild.ProcessorCoolerMountOrientation ==
                        ProcessorCoolerMountOrientation.Rotated180 &&
                    session.AssemblyBuild.ProcessorCoolerTimState ==
                        ProcessorCoolerTimState.AppliedConsumed &&
                    session.TryGetProcessorCoolerItem(
                        out InventoryItemRecord seatedCooler) &&
                    seatedCooler.ContainerId ==
                        session.ProcessorCoolerSlotContainerId &&
                    (seatedCooler.StateFlags &
                     InventorySerializedItemStateFlags
                         .PreAppliedConsumableConsumed) != 0 &&
                    Vector3.Distance(
                        cooler.transform.position,
                        expectedSeatPose.position) <= 0.0005f &&
                    Quaternion.Angle(
                        cooler.transform.rotation,
                        expectedSeatPose.rotation) <= 0.05f;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!seated)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-seat-or-tim-mismatch");
                    yield break;
                }

                MovePlayerToProcessorCoolerSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool retained =
                    session.AssemblyBuild.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.CoolerRetained &&
                    processorCoolerBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!retained)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-retain-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool retainedRemoveBlocked =
                    playerCarry.HeldItem == null &&
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.ProcessorCoolerRetained.Code &&
                    session.AssemblyBuild.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.CoolerRetained;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!retainedRemoveBlocked)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.retained-detach-not-blocked");
                    yield break;
                }

                MovePlayerToProcessorCoolerSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool unretained =
                    session.AssemblyBuild.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.CoolerSeatedUnsecured &&
                    !processorCoolerBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!unretained)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-unretain-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool detached =
                    playerCarry.HeldItem == cooler &&
                    processorCoolerBinding.IsAuthorityInHands &&
                    session.AssemblyBuild.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.EmptyOpen &&
                    session.AssemblyBuild.ProcessorCoolerTimState ==
                        ProcessorCoolerTimState.Unsupported;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!detached)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-detach-mismatch");
                    yield break;
                }

                MovePlayerToProcessorCoolerSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool reseatReady =
                    playerCarry.IsProcessorCoolerSeatMode &&
                    playerCarry.CurrentProcessorCoolerSlotStatus ==
                        ProcessorCoolerSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!reseatReady)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.cooler-reseat-preflight-mismatch");
                    yield break;
                }

                long failedReseatInventoryRevision = session.Inventory.Revision;
                long failedReseatAssemblyRevision = session.AssemblyBuild.Revision;
                int failedReseatReceiptCount = session.AssemblyBuild.ReceiptCount;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool consumedTimBlocked =
                    playerCarry.HeldItem == cooler &&
                    processorCoolerBinding.IsAuthorityInHands &&
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.ProcessorCoolerTimConsumed.Code &&
                    session.Inventory.Revision == failedReseatInventoryRevision &&
                    session.AssemblyBuild.Revision == failedReseatAssemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == failedReseatReceiptCount &&
                    session.AssemblyBuild.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.EmptyOpen;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!consumedTimBlocked)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.consumed-tim-reseat-not-blocked");
                    yield break;
                }

                ProcessorCoolerRetentionTopology topology =
                    session.AssemblyBuild.ProcessorCoolerRetentionTopology;
                bool finalState =
                    session.Inventory.Revision == inventoryRevision + 3 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 4 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 4 &&
                    session.CustomPcBuildKit.StagedComponentCount == 10 &&
                    session.CustomPcBuildKit.AssemblyHandoffCount == 5 &&
                    cooler.GetInstanceID() == coolerPhysicalIdentity &&
                    cooler.ItemIdValue == coolerItemIdentity &&
                    topology != null &&
                    topology.IsValid &&
                    topology.CrossRetentionOrder.Count == 4 &&
                    topology.CrossRetentionOrder[0] == topology.Point1Id &&
                    topology.CrossRetentionOrder[1] == topology.Point3Id &&
                    topology.CrossRetentionOrder[2] == topology.Point2Id &&
                    topology.CrossRetentionOrder[3] == topology.Point4Id &&
                    topology.ReverseCrossRetentionOrder.Count == 4 &&
                    topology.ReverseCrossRetentionOrder[0] == topology.Point4Id &&
                    topology.ReverseCrossRetentionOrder[1] == topology.Point2Id &&
                    topology.ReverseCrossRetentionOrder[2] == topology.Point3Id &&
                    topology.ReverseCrossRetentionOrder[3] == topology.Point1Id &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedSecured &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained &&
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleRetained &&
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.StorageDeviceSecured &&
                    ProcessorCoolerAssemblyHandoffReservationsAreLive(
                        session,
                        workOrder,
                        motherboardLine,
                        processorLine,
                        memoryLine,
                        storageLine,
                        coolerLine) &&
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        otherContainers) &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorCoolerBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!finalState)
                {
                    LogProcessorCoolerAssemblyHandoffSmokeFailure(
                        "smoke.final-state-or-invariant-mismatch");
                    yield break;
                }

                Debug.Log(ProcessorCoolerAssemblyHandoffSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
                if (!Application.isEditor)
                {
                    Application.Quit(0);
                }
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(smokeKeyboard, smokeMouse);
            }
        }

        private static bool ProcessorCoolerAssemblyHandoffReservationsAreLive(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            params CustomPcBuildOrderLineSnapshot[] lines)
        {
            return lines != null &&
                   Array.TrueForAll(
                       lines,
                       line => line != null &&
                               MotherboardAssemblyHandoffSmokeReservationIsLive(
                                   session,
                                   workOrder,
                                   line));
        }

        private bool HasProcessorCoolerStorageClearanceAtSeat()
        {
            BoxCollider storageCollider = storageDevice != null
                ? storageDevice.GetComponent<BoxCollider>()
                : null;
            if (processorCooler == null ||
                processorCoolerSlot == null ||
                storageSlot == null ||
                storageCollider == null)
            {
                return false;
            }

            OperationResult<Pose> primaryPose =
                processorCoolerSlot.ResolveSeatPose(
                    ProcessorCoolerMountOrientation.Primary);
            OperationResult<Pose> rotatedPose =
                processorCoolerSlot.ResolveSeatPose(
                    ProcessorCoolerMountOrientation.Rotated180);
            if (primaryPose.IsFailure || rotatedPose.IsFailure)
            {
                return false;
            }

            Bounds storageBounds = ResolveOrientedBounds(
                storageSlot.SeatedPose.position +
                (storageSlot.SeatedPose.rotation * storageCollider.center),
                storageCollider.size * 0.5f,
                storageSlot.SeatedPose.rotation);
            Bounds primaryCoolerBounds = ResolveOrientedBounds(
                processorCooler.ResolveDropCenter(primaryPose.Value),
                processorCooler.DropHalfExtents,
                primaryPose.Value.rotation);
            Bounds rotatedCoolerBounds = ResolveOrientedBounds(
                processorCooler.ResolveDropCenter(rotatedPose.Value),
                processorCooler.DropHalfExtents,
                rotatedPose.Value.rotation);
            return !primaryCoolerBounds.Intersects(storageBounds) &&
                   !rotatedCoolerBounds.Intersects(storageBounds);
        }

        private static Bounds ResolveOrientedBounds(
            Vector3 center,
            Vector3 halfExtents,
            Quaternion rotation)
        {
            Vector3 axisX = rotation * (Vector3.right * halfExtents.x);
            Vector3 axisY = rotation * (Vector3.up * halfExtents.y);
            Vector3 axisZ = rotation * (Vector3.forward * halfExtents.z);
            Vector3 worldHalfExtents = new Vector3(
                Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x),
                Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y),
                Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z));
            return new Bounds(center, worldHalfExtents * 2f);
        }

        private static bool TryCaptureProcessorCoolerAssemblyHandoffOtherContainers(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            out Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>> containers)
        {
            containers = new Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>>();
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.Motherboard ||
                    line.ComponentKind == PcComponentKind.Processor ||
                    line.ComponentKind == PcComponentKind.MemoryModule ||
                    line.ComponentKind == PcComponentKind.StorageDevice ||
                    line.ComponentKind == PcComponentKind.ProcessorCooler)
                {
                    continue;
                }

                if (!session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item))
                {
                    containers = null;
                    return false;
                }

                containers.Add(line.ItemId, item.ContainerId);
            }

            return containers.Count == 5;
        }

        private void LogProcessorCoolerAssemblyHandoffSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_PROCESSOR_COOLER_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
