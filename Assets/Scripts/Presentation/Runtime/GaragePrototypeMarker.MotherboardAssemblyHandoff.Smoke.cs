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
        public const string MotherboardAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_MOTHERBOARD_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 pickup=exact " +
            "custody=build-kit-to-hands-to-workbench reservation=alive " +
            "physical-identity=stable input=keyboard+mouse guided-seat=ok " +
            "secure=ok unsecure=ok detach=ok reseat=ok " +
            "history=10/10-preserved other-nine=untouched " +
            "receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok";

        public bool HasMotherboardAssemblyHandoffR45Runtime =>
            HasMotherboardBuildKitR35Runtime &&
            HasPcieGpuPowerCableBuildKitR44Runtime &&
            stockFlow != null &&
            stockFlow.Session != null &&
            motherboardBinding != null &&
            motherboardBinding.BuildKit == motherboardBuildKit &&
            motherboardBinding.Seat == motherboardSeat &&
            motherboardBinding.Fastener == motherboardFastener &&
            stockFlow.Session.PrototypeMotherboardAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeCustomPcBuildKitOperationId.Value &&
            stockFlow.Session.WorkbenchContainerId !=
                stockFlow.Session.CustomPcBuildKitContainerId;

        private IEnumerator RunMotherboardAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedPcieGpuPowerCableBuildKitSmokeFailureCode = null;
            _suppressPcieGpuPowerCableBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunPcieGpuPowerCableBuildKitSmoke();
            }
            finally
            {
                _suppressPcieGpuPowerCableBuildKitSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedPcieGpuPowerCableBuildKitSmokeFailureCode;
            _nestedPcieGpuPowerCableBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogMotherboardAssemblyHandoffSmokeFailure(
                    $"smoke.build-kit-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection motherboard = motherboardBinding != null
                ? motherboardBinding.PhysicalItem
                : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                motherboard == null ||
                motherboardBuildKit == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                !HasMotherboardAssemblyHandoffR45Runtime ||
                !motherboardBuildKit.IsStaged ||
                !motherboardBinding.IsAuthorityInBuildKit ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.Empty)
            {
                LogMotherboardAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogMotherboardAssemblyHandoffSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot motherboardLine =
                workOrder.Lines.SingleOrDefault(
                    line => line.ComponentKind == PcComponentKind.Motherboard);
            if (motherboardLine == null ||
                motherboardLine.ItemId != session.MotherboardItemId ||
                !session.Inventory.TryGetReservation(
                    motherboardLine.ReservationId,
                    out InventoryReservation initialReservation) ||
                initialReservation.ItemId != motherboardLine.ItemId ||
                initialReservation.ClaimId != workOrder.InventoryClaimId ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                !TryCaptureMotherboardAssemblyHandoffOtherContainers(
                    session,
                    workOrder,
                    out Dictionary<StableId<ItemInstanceIdScope>,
                        StableId<ContainerIdScope>> otherContainers))
            {
                LogMotherboardAssemblyHandoffSmokeFailure(
                    "smoke.reservation-or-history-mismatch");
                yield break;
            }

            int physicalIdentity = motherboard.GetInstanceID();
            string stableItemId = motherboard.ItemIdValue;
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

                AimMotherboardBuildKitSmokeAtItem(
                    motherboard,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != motherboard)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.motherboard-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool pickedUp =
                    playerCarry.HeldItem == motherboard &&
                    motherboardBinding.IsAuthorityInHands &&
                    motherboardBuildKit.IsReleasedForAssembly &&
                    motherboardBuildKit.StagedComponentCount == 10 &&
                    motherboardBuildKit.ProgressText.text.Contains(
                        "ANAKART MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeMotherboardAssemblyHandoffOperationId,
                        out CustomPcBuildKitAssemblyHandoffReceipt handoff) &&
                    ReferenceEquals(handoff.Line, motherboardLine) &&
                    ReferenceEquals(handoff.StagingReceipt, historicalReceipts[0]) &&
                    motherboard.GetInstanceID() == physicalIdentity &&
                    motherboard.ItemIdValue == stableItemId &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.assembly-pickup-mismatch");
                    yield break;
                }

                MoveMotherboardBuildKitSmokePlayerToSeat();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool seatModeReady =
                    playerCarry.IsMotherboardSeatMode &&
                    !playerCarry.IsMotherboardBuildKitMode &&
                    playerCarry.CurrentMotherboardSeatStatus ==
                        MotherboardSeatStatus.Valid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!seatModeReady)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.guided-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool attached =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedUnsecured &&
                    session.TryGetMotherboardItem(out InventoryItemRecord seatedItem) &&
                    seatedItem.ContainerId == session.WorkbenchContainerId &&
                    session.Inventory.Revision == inventoryRevision + 2 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 1 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 1 &&
                    motherboard.GetInstanceID() == physicalIdentity &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!attached)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.attach-mismatch");
                    yield break;
                }

                MovePlayerToMotherboardFastener();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool secured =
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedSecured &&
                    session.Inventory.Revision == inventoryRevision + 2 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 2 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 2 &&
                    motherboardBinding.IsSecured &&
                    motherboard.GetInstanceID() == physicalIdentity;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!secured)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.secure-mismatch");
                    yield break;
                }

                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool unsecured =
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedUnsecured &&
                    session.Inventory.Revision == inventoryRevision + 2 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 3 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 3 &&
                    !motherboardBinding.IsSecured;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!unsecured)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.unsecure-mismatch");
                    yield break;
                }

                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool detached =
                    playerCarry.HeldItem == motherboard &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.Empty &&
                    session.TryGetMotherboardItem(out InventoryItemRecord heldItem) &&
                    heldItem.ContainerId == session.HandsContainerId &&
                    session.Inventory.Revision == inventoryRevision + 3 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 4 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 4 &&
                    motherboard.GetInstanceID() == physicalIdentity;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!detached)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.detach-mismatch");
                    yield break;
                }

                MoveMotherboardBuildKitSmokePlayerToSeat();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool reseatModeReady =
                    playerCarry.IsMotherboardSeatMode &&
                    playerCarry.CurrentMotherboardSeatStatus ==
                        MotherboardSeatStatus.Valid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!reseatModeReady)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.reseat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool reseated =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedUnsecured &&
                    session.TryGetMotherboardItem(out InventoryItemRecord reseatedItem) &&
                    reseatedItem.ContainerId == session.WorkbenchContainerId &&
                    session.Inventory.Revision == inventoryRevision + 4 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 5 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 5 &&
                    session.CustomPcBuildKit.StagedComponentCount == 10 &&
                    session.CustomPcBuildKit.AssemblyHandoffCount == 1 &&
                    motherboard.GetInstanceID() == physicalIdentity &&
                    motherboard.ItemIdValue == stableItemId &&
                    MotherboardAssemblyHandoffSmokeReservationIsLive(
                        session,
                        workOrder,
                        motherboardLine) &&
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        otherContainers) &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!reseated)
                {
                    LogMotherboardAssemblyHandoffSmokeFailure(
                        "smoke.reseat-or-invariant-mismatch");
                    yield break;
                }

                Debug.Log(MotherboardAssemblyHandoffSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
                if (!Application.isEditor)
                {
                    Application.Quit(0);
                }
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(
                    smokeKeyboard,
                    smokeMouse);
            }
        }

        private static bool TryCaptureMotherboardAssemblyHandoffStagingReceipts(
            GarageStockFlowSession session,
            out CustomPcBuildKitReceipt[] receipts)
        {
            StableId<CustomPcBuildKitOperationIdScope>[] operationIds =
            {
                session.PrototypeCustomPcBuildKitOperationId,
                session.PrototypeProcessorBuildKitOperationId,
                session.PrototypeMemoryModuleBuildKitOperationId,
                session.PrototypeStorageBuildKitOperationId,
                session.PrototypeProcessorCoolerBuildKitOperationId,
                session.PrototypeGraphicsCardBuildKitOperationId,
                session.PrototypePowerSupplyBuildKitOperationId,
                session.PrototypeAtx24PowerCableBuildKitOperationId,
                session.PrototypeEps12vPowerCableBuildKitOperationId,
                session.PrototypePcieGpuPowerCableBuildKitOperationId
            };
            receipts = new CustomPcBuildKitReceipt[operationIds.Length];
            for (int index = 0; index < operationIds.Length; index++)
            {
                if (!session.CustomPcBuildKit.TryGetReceipt(
                        operationIds[index],
                        out receipts[index]) ||
                    receipts[index] == null)
                {
                    receipts = null;
                    return false;
                }
            }

            return true;
        }

        private static bool TryCaptureMotherboardAssemblyHandoffOtherContainers(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            out Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>> containers)
        {
            containers = new Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>>();
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.Motherboard)
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

            return containers.Count == 9;
        }

        private static bool MotherboardAssemblyHandoffSmokeReservationIsLive(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            CustomPcBuildOrderLineSnapshot line)
        {
            return session.Inventory.TryGetReservation(
                       line.ReservationId,
                       out InventoryReservation reservation) &&
                   reservation.ItemId == line.ItemId &&
                   reservation.ClaimId == workOrder.InventoryClaimId;
        }

        private static bool MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
            GarageStockFlowSession session,
            IReadOnlyList<CustomPcBuildKitReceipt> expectedReceipts,
            IReadOnlyDictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>> expectedContainers)
        {
            if (!TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] currentReceipts) ||
                currentReceipts.Length != expectedReceipts.Count)
            {
                return false;
            }

            for (int index = 0; index < currentReceipts.Length; index++)
            {
                if (!ReferenceEquals(currentReceipts[index], expectedReceipts[index]))
                {
                    return false;
                }
            }

            foreach (KeyValuePair<StableId<ItemInstanceIdScope>,
                         StableId<ContainerIdScope>> entry in expectedContainers)
            {
                if (!session.Inventory.TryGetSerializedItem(
                        entry.Key,
                        out InventoryItemRecord item) ||
                    item.ContainerId != entry.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static void LogMotherboardAssemblyHandoffSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_MOTHERBOARD_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
