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
        public const string StorageAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_STORAGE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 motherboard=secured processor=retained " +
            "memory=retained pickup=exact custody=build-kit-to-hands-to-primary-m2 " +
            "reservation=alive physical-identity=stable input=keyboard+mouse " +
            "m-key=aligned guided-angle=18 seat=ok captive-screw=tightened " +
            "secured-remove-blocked=ok loosen=ok detach=ok reseat=ok " +
            "history=10/10-preserved other-six=untouched receipts=ok " +
            "revisions=ok no-duplicate-loss=ok invariants=ok";

        private bool _suppressStorageAssemblyHandoffSmokeSuccessMarker;
        private string _nestedStorageAssemblyHandoffSmokeFailureCode;

        public bool HasStorageAssemblyHandoffR48Runtime =>
            HasMemoryModuleAssemblyHandoffR47Runtime &&
            storageDevice != null &&
            storageBinding != null &&
            storageSlot != null &&
            storageBuildKit != null &&
            storageBinding.PhysicalItem == storageDevice &&
            storageBinding.BuildKit == storageBuildKit &&
            storageBinding.Slot == storageSlot &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypeStorageAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeStorageBuildKitOperationId.Value &&
            stockFlow.Session.StorageSlotContainerId !=
                stockFlow.Session.StorageBuildKitContainerId;

        private IEnumerator RunStorageAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedMemoryModuleAssemblyHandoffSmokeFailureCode = null;
            _suppressMemoryModuleAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunMemoryModuleAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressMemoryModuleAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedMemoryModuleAssemblyHandoffSmokeFailureCode;
            _nestedMemoryModuleAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogStorageAssemblyHandoffSmokeFailure(
                    $"smoke.memory-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection storage = storageBinding != null
                ? storageBinding.PhysicalItem
                : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                storage == null ||
                !HasStorageAssemblyHandoffR48Runtime ||
                !storageBuildKit.IsStaged ||
                !storageBinding.IsAuthorityInBuildKit ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 3 ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.ProcessorRetained ||
                session.AssemblyBuild.MemorySlotState !=
                    MemorySlotState.MemoryModuleRetained ||
                session.AssemblyBuild.StorageSlotState != StorageSlotState.EmptyOpen)
            {
                LogStorageAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogStorageAssemblyHandoffSmokeFailure(
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
            if (motherboardLine == null ||
                processorLine == null ||
                memoryLine == null ||
                storageLine == null ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                !TryCaptureStorageAssemblyHandoffOtherContainers(
                    session,
                    workOrder,
                    out Dictionary<StableId<ItemInstanceIdScope>,
                        StableId<ContainerIdScope>> otherContainers) ||
                !MotherboardAssemblyHandoffSmokeReservationIsLive(
                    session,
                    workOrder,
                    motherboardLine) ||
                !MotherboardAssemblyHandoffSmokeReservationIsLive(
                    session,
                    workOrder,
                    processorLine) ||
                !MotherboardAssemblyHandoffSmokeReservationIsLive(
                    session,
                    workOrder,
                    memoryLine) ||
                !MotherboardAssemblyHandoffSmokeReservationIsLive(
                    session,
                    workOrder,
                    storageLine))
            {
                LogStorageAssemblyHandoffSmokeFailure(
                    "smoke.reservation-or-history-mismatch");
                yield break;
            }

            int storagePhysicalIdentity = storage.GetInstanceID();
            string storageItemIdentity = storage.ItemIdValue;
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

                storageBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(storage, -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != storage ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains("PRIMARY SLOT MONTAJINA AL"))
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool pickedUp =
                    playerCarry.HeldItem == storage &&
                    storageBinding.IsAuthorityInHands &&
                    storageBuildKit.IsReleasedForAssembly &&
                    storageBuildKit.StagedComponentCount == 10 &&
                    storageBuildKit.ProgressText.text.Contains("M.2 MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeStorageAssemblyHandoffOperationId,
                        out CustomPcBuildKitAssemblyHandoffReceipt storageHandoff) &&
                    storageHandoff.ComponentKind == PcComponentKind.StorageDevice &&
                    ReferenceEquals(storageHandoff.Line, storageLine) &&
                    ReferenceEquals(storageHandoff.StagingReceipt,
                        historicalReceipts[3]);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-build-kit-pickup-mismatch");
                    yield break;
                }

                MovePlayerToM2StorageSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool seatReady =
                    playerCarry.IsM2StorageSeatMode &&
                    playerCarry.CurrentM2StorageSlotStatus ==
                        M2StorageSlotStatus.ValidSeat &&
                    playerCarry.PromptText.Contains("18°");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!seatReady)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool wrongOrientation =
                    playerCarry.CurrentM2StorageSlotStatus ==
                        M2StorageSlotStatus.OrientationInvalid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool wrongSeatBlocked =
                    playerCarry.HeldItem == storage &&
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.EmptyOpen &&
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.M2OrientationMismatch.Code;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!wrongOrientation || !wrongSeatBlocked)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-key-orientation-gate-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool seated =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.StorageDeviceSeatedUnsecured &&
                    storageBinding.IsSeated &&
                    Vector3.Distance(
                        storage.transform.position,
                        storageSlot.SeatedPose.position) <= 0.0005f &&
                    Quaternion.Angle(
                        storage.transform.rotation,
                        storageSlot.SeatedPose.rotation) <= 0.05f;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!seated)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-seat-mismatch");
                    yield break;
                }

                MovePlayerToM2StorageSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool secured =
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.StorageDeviceSecured &&
                    storageBinding.IsSecured;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!secured)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-captive-screw-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool securedRemoveBlocked =
                    playerCarry.HeldItem == null &&
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.StorageDeviceSecured.Code &&
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.StorageDeviceSecured;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!securedRemoveBlocked)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.secured-detach-not-blocked");
                    yield break;
                }

                MovePlayerToM2StorageSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool loosened =
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.StorageDeviceSeatedUnsecured &&
                    !storageBinding.IsSecured;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!loosened)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-loosen-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool detached =
                    playerCarry.HeldItem == storage &&
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.EmptyOpen &&
                    storageBinding.IsAuthorityInHands;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!detached)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-detach-mismatch");
                    yield break;
                }

                MovePlayerToM2StorageSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool reseatReady =
                    playerCarry.IsM2StorageSeatMode &&
                    playerCarry.CurrentM2StorageSlotStatus ==
                        M2StorageSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!reseatReady)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.storage-reseat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                MovePlayerToM2StorageSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);

                bool finalState =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedSecured &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained &&
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleRetained &&
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.StorageDeviceSecured &&
                    session.Inventory.Revision == inventoryRevision + 4 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 6 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 6 &&
                    session.CustomPcBuildKit.StagedComponentCount == 10 &&
                    session.CustomPcBuildKit.AssemblyHandoffCount == 4 &&
                    storage.GetInstanceID() == storagePhysicalIdentity &&
                    storage.ItemIdValue == storageItemIdentity &&
                    Vector3.Distance(
                        storage.transform.position,
                        storageSlot.SeatedPose.position) <= 0.0005f &&
                    Quaternion.Angle(
                        storage.transform.rotation,
                        storageSlot.SeatedPose.rotation) <= 0.05f &&
                    MotherboardAssemblyHandoffSmokeReservationIsLive(
                        session,
                        workOrder,
                        motherboardLine) &&
                    MotherboardAssemblyHandoffSmokeReservationIsLive(
                        session,
                        workOrder,
                        processorLine) &&
                    MotherboardAssemblyHandoffSmokeReservationIsLive(
                        session,
                        workOrder,
                        memoryLine) &&
                    MotherboardAssemblyHandoffSmokeReservationIsLive(
                        session,
                        workOrder,
                        storageLine) &&
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        otherContainers) &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!finalState)
                {
                    LogStorageAssemblyHandoffSmokeFailure(
                        "smoke.final-state-or-invariant-mismatch");
                    yield break;
                }

                if (!_suppressStorageAssemblyHandoffSmokeSuccessMarker)
                {
                    Debug.Log(StorageAssemblyHandoffSmokeSuccessMarker);
                }

                yield return new WaitForEndOfFrame();
                if (!Application.isEditor &&
                    !_suppressStorageAssemblyHandoffSmokeSuccessMarker)
                {
                    Application.Quit(0);
                }
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(smokeKeyboard, smokeMouse);
            }
        }

        private static bool TryCaptureStorageAssemblyHandoffOtherContainers(
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
                    line.ComponentKind == PcComponentKind.StorageDevice)
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

            return containers.Count == 6;
        }

        private void LogStorageAssemblyHandoffSmokeFailure(string code)
        {
            if (_suppressStorageAssemblyHandoffSmokeSuccessMarker)
            {
                _nestedStorageAssemblyHandoffSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_STORAGE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
