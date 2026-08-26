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
        private bool _suppressMemoryModuleAssemblyHandoffSmokeSuccessMarker;
        private string _nestedMemoryModuleAssemblyHandoffSmokeFailureCode;

        public const string MemoryModuleAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_MEMORY_MODULE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 motherboard=secured processor=retained " +
            "pickup=exact custody=build-kit-to-hands-to-a2 " +
            "reservation=alive physical-identity=stable input=keyboard+mouse " +
            "notch=aligned seat=ok dual-latch=closed retained-block=ok " +
            "open=ok detach=ok reseat=ok history=10/10-preserved " +
            "other-seven=untouched receipts=ok revisions=ok " +
            "no-duplicate-loss=ok invariants=ok";

        public bool HasMemoryModuleAssemblyHandoffR47Runtime =>
            HasProcessorAssemblyHandoffR46Runtime &&
            memoryModule != null &&
            dimmBinding != null &&
            dimmSlot != null &&
            memoryModuleBuildKit != null &&
            dimmBinding.PhysicalItem == memoryModule &&
            dimmBinding.BuildKit == memoryModuleBuildKit &&
            dimmBinding.Slot == dimmSlot &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypeMemoryModuleAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeMemoryModuleBuildKitOperationId.Value &&
            stockFlow.Session.MemorySlotContainerId !=
                stockFlow.Session.MemoryModuleBuildKitContainerId;

        private IEnumerator RunMemoryModuleAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedProcessorAssemblyHandoffSmokeFailureCode = null;
            _suppressProcessorAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunProcessorAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressProcessorAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedProcessorAssemblyHandoffSmokeFailureCode;
            _nestedProcessorAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogMemoryModuleAssemblyHandoffSmokeFailure(
                    $"smoke.processor-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection memory = dimmBinding != null
                ? dimmBinding.PhysicalItem
                : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                memory == null ||
                !HasMemoryModuleAssemblyHandoffR47Runtime ||
                !memoryModuleBuildKit.IsStaged ||
                !dimmBinding.IsAuthorityInBuildKit ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 2 ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.ProcessorRetained ||
                session.AssemblyBuild.MemorySlotState != MemorySlotState.EmptyOpen)
            {
                LogMemoryModuleAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogMemoryModuleAssemblyHandoffSmokeFailure(
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
            if (motherboardLine == null ||
                processorLine == null ||
                memoryLine == null ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                !TryCaptureMemoryModuleAssemblyHandoffOtherContainers(
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
                    memoryLine))
            {
                LogMemoryModuleAssemblyHandoffSmokeFailure(
                    "smoke.reservation-or-history-mismatch");
                yield break;
            }

            int memoryPhysicalIdentity = memory.GetInstanceID();
            string memoryItemIdentity = memory.ItemIdValue;
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

                memoryModuleBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(memory, -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != memory ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains("DDR5'İ A2 MONTAJINA AL"))
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool memoryPickedUp =
                    playerCarry.HeldItem == memory &&
                    dimmBinding.IsAuthorityInHands &&
                    memoryModuleBuildKit.IsReleasedForAssembly &&
                    memoryModuleBuildKit.ProgressText.text.Contains("DDR5 MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeMemoryModuleAssemblyHandoffOperationId,
                        out CustomPcBuildKitAssemblyHandoffReceipt handoff) &&
                    handoff.ComponentKind == PcComponentKind.MemoryModule &&
                    ReferenceEquals(handoff.Line, memoryLine) &&
                    ReferenceEquals(handoff.StagingReceipt, historicalReceipts[2]) &&
                    memory.GetInstanceID() == memoryPhysicalIdentity &&
                    memory.ItemIdValue == memoryItemIdentity;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!memoryPickedUp)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-pickup-mismatch");
                    yield break;
                }

                MovePlayerToDimmSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool seatReady =
                    playerCarry.IsDimmSeatMode &&
                    playerCarry.CurrentDimmSlotStatus == DimmSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!seatReady)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool seated =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleSeatedOpen &&
                    dimmBinding.IsSeated;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!seated)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-seat-mismatch");
                    yield break;
                }

                MovePlayerToDimmSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool retained =
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleRetained &&
                    dimmBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!retained)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-retain-mismatch");
                    yield break;
                }

                yield return WaitForMemoryModuleAssemblyHandoffLatches();
                if (dimmSlot.IsLatchAnimating)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-latch-close-timeout");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool retainedBlocked =
                    playerCarry.HeldItem == null &&
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.MemoryModuleRetained.Code &&
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleRetained;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!retainedBlocked)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.retained-detach-not-blocked");
                    yield break;
                }

                MovePlayerToDimmSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool opened =
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleSeatedOpen &&
                    !dimmBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!opened)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-open-mismatch");
                    yield break;
                }

                yield return WaitForMemoryModuleAssemblyHandoffLatches();
                if (dimmSlot.IsLatchAnimating)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-latch-open-timeout");
                    yield break;
                }

                MovePlayerToDimmSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool detached =
                    playerCarry.HeldItem == memory &&
                    session.AssemblyBuild.MemorySlotState == MemorySlotState.EmptyOpen &&
                    dimmBinding.IsAuthorityInHands;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!detached)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-detach-mismatch");
                    yield break;
                }

                MovePlayerToDimmSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool reseatReady =
                    playerCarry.IsDimmSeatMode &&
                    playerCarry.CurrentDimmSlotStatus == DimmSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!reseatReady)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.memory-reseat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                MovePlayerToDimmSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                yield return WaitForMemoryModuleAssemblyHandoffLatches();

                bool finalState =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedSecured &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained &&
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleRetained &&
                    !dimmSlot.IsLatchAnimating &&
                    session.Inventory.Revision == inventoryRevision + 4 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 6 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 6 &&
                    session.CustomPcBuildKit.StagedComponentCount == 10 &&
                    session.CustomPcBuildKit.AssemblyHandoffCount == 3 &&
                    memory.GetInstanceID() == memoryPhysicalIdentity &&
                    memory.ItemIdValue == memoryItemIdentity &&
                    Vector3.Distance(
                        memory.transform.position,
                        dimmSlot.SnapPose.position) <= 0.0005f &&
                    Quaternion.Angle(
                        memory.transform.rotation,
                        dimmSlot.SnapPose.rotation) <= 0.05f &&
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
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        otherContainers) &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!finalState)
                {
                    LogMemoryModuleAssemblyHandoffSmokeFailure(
                        "smoke.final-state-or-invariant-mismatch");
                    yield break;
                }

                if (!_suppressMemoryModuleAssemblyHandoffSmokeSuccessMarker)
                {
                    Debug.Log(MemoryModuleAssemblyHandoffSmokeSuccessMarker);
                    yield return new WaitForEndOfFrame();
                    if (!Application.isEditor)
                    {
                        Application.Quit(0);
                    }
                }
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(smokeKeyboard, smokeMouse);
            }
        }

        private IEnumerator WaitForMemoryModuleAssemblyHandoffLatches()
        {
            int frames = 0;
            while (dimmSlot != null && dimmSlot.IsLatchAnimating && frames < 120)
            {
                frames++;
                yield return null;
            }
        }

        private static bool TryCaptureMemoryModuleAssemblyHandoffOtherContainers(
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
                    line.ComponentKind == PcComponentKind.MemoryModule)
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

            return containers.Count == 7;
        }

        private void LogMemoryModuleAssemblyHandoffSmokeFailure(string code)
        {
            if (_suppressMemoryModuleAssemblyHandoffSmokeSuccessMarker)
            {
                _nestedMemoryModuleAssemblyHandoffSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_MEMORY_MODULE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
