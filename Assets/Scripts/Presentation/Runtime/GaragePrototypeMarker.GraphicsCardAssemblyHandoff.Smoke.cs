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
        public const string GraphicsCardAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_GRAPHICS_CARD_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 motherboard=secured " +
            "processor=retained memory=retained storage=secured cooler=retained " +
            "prerequisite-setup=assisted pickup=exact " +
            "custody=build-kit-to-hands-to-pcie-x16 reservation=alive " +
            "physical-identity=stable gpu-input=keyboard+mouse " +
            "orientation-invalid=blocked seat=ok slot-latch=retained " +
            "rear-bracket=secured retained-remove-blocked=ok unretain=ok " +
            "detach=ok reseat=ok history=10/10-preserved other-four=untouched " +
            "pcie-power-cable=untouched receipts=ok revisions=ok " +
            "no-duplicate-loss=ok invariants=ok";

        private bool _suppressGraphicsCardAssemblyHandoffSmokeSuccessMarker;
        private string _nestedGraphicsCardAssemblyHandoffSmokeFailureCode;

        public bool HasGraphicsCardAssemblyHandoffR50Runtime =>
            HasProcessorCoolerAssemblyHandoffR49Runtime &&
            HasGraphicsCardR28Runtime &&
            HasGraphicsCardBuildKitR40Runtime &&
            graphicsCardBinding != null &&
            graphicsCardBinding.BuildKit == graphicsCardBuildKit &&
            graphicsCardBinding.PhysicalItem == graphicsCard &&
            graphicsCardBinding.Slot == graphicsCardSlot &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypeGraphicsCardAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeGraphicsCardBuildKitOperationId.Value &&
            stockFlow.Session.PrototypeGraphicsCardAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeProcessorCoolerAssemblyHandoffOperationId.Value &&
            stockFlow.Session.PrototypeGraphicsCardAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeStorageAssemblyHandoffOperationId.Value &&
            stockFlow.Session.GraphicsCardSlotContainerId !=
                stockFlow.Session.GraphicsCardBuildKitContainerId;

        private IEnumerator RunGraphicsCardAssemblyHandoffSmoke()
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
                LogGraphicsCardAssemblyHandoffSmokeFailure(
                    $"smoke.storage-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection physicalGraphicsCard =
                graphicsCardBinding != null
                    ? graphicsCardBinding.PhysicalItem
                    : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                physicalGraphicsCard == null ||
                processorCooler == null ||
                !HasGraphicsCardAssemblyHandoffR50Runtime ||
                !processorCoolerBuildKit.IsStaged ||
                !processorCoolerBinding.IsAuthorityInBuildKit ||
                !graphicsCardBuildKit.IsStaged ||
                !graphicsCardBinding.IsAuthorityInBuildKit ||
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
                    ProcessorCoolerSlotState.EmptyOpen ||
                session.AssemblyBuild.GraphicsCardSlotState !=
                    GraphicsCardSlotState.EmptyOpen)
            {
                LogGraphicsCardAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogGraphicsCardAssemblyHandoffSmokeFailure(
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
            CustomPcBuildOrderLineSnapshot graphicsCardLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.GraphicsCard);
            if (motherboardLine == null ||
                processorLine == null ||
                memoryLine == null ||
                storageLine == null ||
                coolerLine == null ||
                graphicsCardLine == null ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                !TryCaptureGraphicsCardAssemblyHandoffOtherContainers(
                    session,
                    workOrder,
                    out Dictionary<StableId<ItemInstanceIdScope>,
                        StableId<ContainerIdScope>> otherContainers) ||
                !GraphicsCardAssemblyHandoffReservationsAreLive(
                    session,
                    workOrder,
                    motherboardLine,
                    processorLine,
                    memoryLine,
                    storageLine,
                    coolerLine,
                    graphicsCardLine))
            {
                LogGraphicsCardAssemblyHandoffSmokeFailure(
                    "smoke.reservation-or-history-mismatch");
                yield break;
            }

            OperationResult coolerPickup = playerCarry.TryPickup(processorCooler);
            if (coolerPickup.IsFailure ||
                playerCarry.HeldItem != processorCooler ||
                !processorCoolerBinding.IsAuthorityInHands ||
                !processorCoolerBuildKit.IsReleasedForAssembly ||
                !session.CustomPcBuildKit.TryGetAssemblyHandoff(
                    session.PrototypeProcessorCoolerAssemblyHandoffOperationId,
                    out CustomPcBuildKitAssemblyHandoffReceipt coolerHandoff) ||
                !ReferenceEquals(coolerHandoff.Line, coolerLine) ||
                !ReferenceEquals(coolerHandoff.StagingReceipt, historicalReceipts[4]))
            {
                LogGraphicsCardAssemblyHandoffSmokeFailure(
                    "smoke.cooler-assisted-pickup-mismatch");
                yield break;
            }

            MovePlayerToProcessorCoolerSlot();
            OperationResult coolerSeatMode =
                playerCarry.TrySetProcessorCoolerSeatMode(true);
            OperationResult coolerSeat = coolerSeatMode.IsSuccess
                ? playerCarry.TryConfirmProcessorCoolerSeat()
                : OperationResult.Fail(coolerSeatMode.Error);
            MovePlayerToProcessorCoolerSlot();
            OperationResult coolerRetain = coolerSeat.IsSuccess
                ? playerCarry.TryOperateProcessorCoolerRetention()
                : OperationResult.Fail(coolerSeat.Error);
            if (coolerRetain.IsFailure ||
                playerCarry.HeldItem != null ||
                session.AssemblyBuild.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.CoolerRetained ||
                session.AssemblyBuild.ProcessorCoolerTimState !=
                    ProcessorCoolerTimState.AppliedConsumed ||
                !processorCoolerBinding.IsRetained ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 5 ||
                !processorCoolerBinding.ValidateProjectionInvariant().IsSuccess ||
                !session.ValidateInvariants().IsSuccess)
            {
                LogGraphicsCardAssemblyHandoffSmokeFailure(
                    "smoke.cooler-assisted-retention-mismatch");
                yield break;
            }

            int physicalIdentity = physicalGraphicsCard.GetInstanceID();
            string stableItemId = physicalGraphicsCard.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            StableId<ItemInstanceIdScope> pcieItemId =
                session.AssemblyBuild.PcieGpuPowerCableItemId;
            StableId<ProductDefinitionIdScope> pcieProductId =
                session.AssemblyBuild.PcieGpuPowerCableProductId;
            StableId<AssemblyOperationIdScope> pcieRoutedByOperationId =
                session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId;
            StableId<ContainerIdScope> pcieRouteContainerId =
                session.AssemblyBuild.PcieGpuPowerCableRouteContainerId;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            if (!session.TryGetPcieGpuPowerCableItem(
                    out InventoryItemRecord pcieInventoryItem))
            {
                LogGraphicsCardAssemblyHandoffSmokeFailure(
                    "smoke.pcie-power-cable-authority-missing");
                yield break;
            }

            StableId<ContainerIdScope> pcieInventoryContainerId =
                pcieInventoryItem.ContainerId;
            InventorySerializedItemStateFlags pcieInventoryStateFlags =
                pcieInventoryItem.StateFlags;
            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeMouse = InputSystem.AddDevice<Mouse>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.Update();

                graphicsCardBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(
                    physicalGraphicsCard,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalGraphicsCard ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains("PCIe x16 MONTAJINA AL"))
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitAssemblyHandoffReceipt graphicsCardHandoff = null;
                bool pickedUp =
                    playerCarry.HeldItem == physicalGraphicsCard &&
                    graphicsCardBinding.IsAuthorityInHands &&
                    graphicsCardBuildKit.IsReleasedForAssembly &&
                    graphicsCardBuildKit.StagedComponentCount == 10 &&
                    graphicsCardBuildKit.ProgressText.text.Contains("GPU MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                        out graphicsCardHandoff) &&
                    graphicsCardHandoff.ComponentKind ==
                        PcComponentKind.GraphicsCard &&
                    ReferenceEquals(graphicsCardHandoff.Line, graphicsCardLine) &&
                    ReferenceEquals(
                        graphicsCardHandoff.StagingReceipt,
                        historicalReceipts[5]) &&
                    physicalGraphicsCard.GetInstanceID() == physicalIdentity &&
                    physicalGraphicsCard.ItemIdValue == stableItemId &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-build-kit-pickup-mismatch");
                    yield break;
                }

                long replayInventoryRevision = session.Inventory.Revision;
                long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
                long replayAssemblyRevision = session.AssemblyBuild.Revision;
                int replayAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                    session.PickupStagedGraphicsCardForAssembly();
                if (replay.IsFailure ||
                    !ReferenceEquals(replay.Value, graphicsCardHandoff) ||
                    session.Inventory.Revision != replayInventoryRevision ||
                    session.CustomPcBuildKit.Revision != replayBuildKitRevision ||
                    session.AssemblyBuild.Revision != replayAssemblyRevision ||
                    session.AssemblyBuild.ReceiptCount !=
                        replayAssemblyReceiptCount)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.immediate-replay-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool worldDropBlocked =
                    playerCarry.HeldItem == physicalGraphicsCard &&
                    graphicsCardBinding.IsAuthorityInHands &&
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!worldDropBlocked)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.reserved-world-drop-not-blocked");
                    yield break;
                }

                MovePlayerToGraphicsCardSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool seatReady =
                    playerCarry.IsGraphicsCardSeatMode &&
                    playerCarry.CurrentGraphicsCardSlotStatus ==
                        GraphicsCardSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!seatReady)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool invalidOrientationReady =
                    playerCarry.IsGraphicsCardSeatMode &&
                    playerCarry.CurrentGraphicsCardSlotStatus ==
                        GraphicsCardSlotStatus.OrientationInvalid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!invalidOrientationReady)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-invalid-orientation-mismatch");
                    yield break;
                }

                long invalidInventoryRevision = session.Inventory.Revision;
                long invalidAssemblyRevision = session.AssemblyBuild.Revision;
                int invalidAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool invalidOrientationBlocked =
                    playerCarry.HeldItem == physicalGraphicsCard &&
                    graphicsCardBinding.IsAuthorityInHands &&
                    playerCarry.LastFailureCode ==
                        "assembly-graphics-card.orientation-mismatch" &&
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.EmptyOpen &&
                    session.Inventory.Revision == invalidInventoryRevision &&
                    session.AssemblyBuild.Revision == invalidAssemblyRevision &&
                    session.AssemblyBuild.ReceiptCount ==
                        invalidAssemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!invalidOrientationBlocked)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.invalid-orientation-not-fail-closed");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool primaryOrientationReady =
                    playerCarry.CurrentGraphicsCardSlotStatus ==
                        GraphicsCardSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!primaryOrientationReady)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-primary-orientation-mismatch");
                    yield break;
                }

                OperationResult<Pose> primarySeatPose =
                    graphicsCardSlot.ResolveSeatPose(0);
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool seated =
                    primarySeatPose.IsSuccess &&
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.GraphicsCardSeatedUnsecured &&
                    session.AssemblyBuild.GraphicsCardMountOrientation ==
                        GraphicsCardMountOrientation.Primary &&
                    session.TryGetGraphicsCardAssemblyItem(
                        out InventoryItemRecord seatedGraphicsCard) &&
                    seatedGraphicsCard.ContainerId ==
                        session.GraphicsCardSlotContainerId &&
                    physicalGraphicsCard.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalGraphicsCard.IsStablePlacement &&
                    physicalGraphicsCard.Body.isKinematic &&
                    !physicalGraphicsCard.Body.useGravity &&
                    Vector3.Distance(
                        physicalGraphicsCard.transform.position,
                        primarySeatPose.Value.position) <= 0.0005f &&
                    Quaternion.Angle(
                        physicalGraphicsCard.transform.rotation,
                        primarySeatPose.Value.rotation) <= 0.05f;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!seated)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-seat-mismatch");
                    yield break;
                }

                MovePlayerToGraphicsCardSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool retained =
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.GraphicsCardRetained &&
                    graphicsCardBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!retained)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-retain-mismatch");
                    yield break;
                }

                long retainedInventoryRevision = session.Inventory.Revision;
                long retainedAssemblyRevision = session.AssemblyBuild.Revision;
                int retainedAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool retainedRemoveBlocked =
                    playerCarry.HeldItem == null &&
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.GraphicsCardRetained.Code &&
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.GraphicsCardRetained &&
                    session.Inventory.Revision == retainedInventoryRevision &&
                    session.AssemblyBuild.Revision == retainedAssemblyRevision &&
                    session.AssemblyBuild.ReceiptCount ==
                        retainedAssemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!retainedRemoveBlocked)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.retained-detach-not-blocked");
                    yield break;
                }

                MovePlayerToGraphicsCardSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool unretained =
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.GraphicsCardSeatedUnsecured &&
                    !graphicsCardBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!unretained)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-unretain-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool detached =
                    playerCarry.HeldItem == physicalGraphicsCard &&
                    graphicsCardBinding.IsAuthorityInHands &&
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.EmptyOpen;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!detached)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-detach-mismatch");
                    yield break;
                }

                MovePlayerToGraphicsCardSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool reseatReady =
                    playerCarry.IsGraphicsCardSeatMode &&
                    playerCarry.CurrentGraphicsCardSlotStatus ==
                        GraphicsCardSlotStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!reseatReady)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-reseat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool reseated =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.GraphicsCardSeatedUnsecured;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!reseated)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.graphics-card-reseat-mismatch");
                    yield break;
                }

                MovePlayerToGraphicsCardSlot();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool finalRetention =
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.GraphicsCardRetained &&
                    graphicsCardBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);

                bool pcieAuthorityUnchanged =
                    GraphicsCardAssemblyHandoffPcieAuthorityIsUnchanged(
                        session,
                        pcieState,
                        pcieItemId,
                        pcieProductId,
                        pcieRoutedByOperationId,
                        pcieRouteContainerId,
                        pcieRevision,
                        pcieReceiptCount,
                        pcieInventoryItem.Id,
                        pcieInventoryItem.ProductId,
                        pcieInventoryContainerId,
                        pcieInventoryStateFlags);
                if (!pcieAuthorityUnchanged)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.pcie-power-cable-authority-changed");
                    yield break;
                }

                bool finalState =
                    finalRetention &&
                    session.Inventory.Revision == inventoryRevision + 4 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 6 &&
                    session.AssemblyBuild.ReceiptCount ==
                        assemblyReceiptCount + 6 &&
                    session.CustomPcBuildKit.StagedComponentCount == 10 &&
                    session.CustomPcBuildKit.AssemblyHandoffCount == 6 &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedSecured &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained &&
                    session.AssemblyBuild.MemorySlotState ==
                        MemorySlotState.MemoryModuleRetained &&
                    session.AssemblyBuild.StorageSlotState ==
                        StorageSlotState.StorageDeviceSecured &&
                    session.AssemblyBuild.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.CoolerRetained &&
                    processorCoolerBinding.IsRetained &&
                    physicalGraphicsCard.GetInstanceID() == physicalIdentity &&
                    physicalGraphicsCard.ItemIdValue == stableItemId &&
                    physicalGraphicsCard.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalGraphicsCard.IsStablePlacement &&
                    primarySeatPose.IsSuccess &&
                    Vector3.Distance(
                        physicalGraphicsCard.transform.position,
                        primarySeatPose.Value.position) <= 0.0005f &&
                    Quaternion.Angle(
                        physicalGraphicsCard.transform.rotation,
                        primarySeatPose.Value.rotation) <= 0.05f &&
                    GraphicsCardAssemblyHandoffReservationsAreLive(
                        session,
                        workOrder,
                        motherboardLine,
                        processorLine,
                        memoryLine,
                        storageLine,
                        coolerLine,
                        graphicsCardLine) &&
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        otherContainers) &&
                    CountCanonicalGraphicsCardProjections(
                        GarageStockFlowSession
                            .GraphicsCardAssemblyItemInstanceIdValue) == 1 &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorCoolerBinding.ValidateProjectionInvariant().IsSuccess &&
                    graphicsCardBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!finalState)
                {
                    LogGraphicsCardAssemblyHandoffSmokeFailure(
                        "smoke.final-state-or-invariant-mismatch");
                    yield break;
                }

                if (!_suppressGraphicsCardAssemblyHandoffSmokeSuccessMarker)
                {
                    Debug.Log(GraphicsCardAssemblyHandoffSmokeSuccessMarker);
                }

                yield return new WaitForEndOfFrame();
                if (!Application.isEditor &&
                    !_suppressGraphicsCardAssemblyHandoffSmokeSuccessMarker)
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

        private static bool GraphicsCardAssemblyHandoffReservationsAreLive(
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

        private static bool TryCaptureGraphicsCardAssemblyHandoffOtherContainers(
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
                    line.ComponentKind == PcComponentKind.ProcessorCooler ||
                    line.ComponentKind == PcComponentKind.GraphicsCard)
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

            return containers.Count == 4;
        }

        private static bool GraphicsCardAssemblyHandoffPcieAuthorityIsUnchanged(
            GarageStockFlowSession session,
            PcieGpuPowerCableState expectedState,
            StableId<ItemInstanceIdScope> expectedItemId,
            StableId<ProductDefinitionIdScope> expectedProductId,
            StableId<AssemblyOperationIdScope> expectedRoutedByOperationId,
            StableId<ContainerIdScope> expectedRouteContainerId,
            long expectedRevision,
            int expectedReceiptCount,
            StableId<ItemInstanceIdScope> expectedInventoryItemId,
            StableId<ProductDefinitionIdScope> expectedInventoryProductId,
            StableId<ContainerIdScope> expectedInventoryContainerId,
            InventorySerializedItemStateFlags expectedInventoryStateFlags)
        {
            return session.AssemblyBuild.PcieGpuPowerCableState ==
                       expectedState &&
                   session.AssemblyBuild.PcieGpuPowerCableItemId ==
                       expectedItemId &&
                   session.AssemblyBuild.PcieGpuPowerCableProductId ==
                       expectedProductId &&
                   session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId ==
                       expectedRoutedByOperationId &&
                   session.AssemblyBuild.PcieGpuPowerCableRouteContainerId ==
                       expectedRouteContainerId &&
                   session.AssemblyBuild.PcieGpuPowerCableRevision ==
                       expectedRevision &&
                   session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                       expectedReceiptCount &&
                   session.TryGetPcieGpuPowerCableItem(
                       out InventoryItemRecord currentItem) &&
                   currentItem.Id == expectedInventoryItemId &&
                   currentItem.ProductId == expectedInventoryProductId &&
                   currentItem.ContainerId == expectedInventoryContainerId &&
                   currentItem.StateFlags == expectedInventoryStateFlags;
        }

        private void LogGraphicsCardAssemblyHandoffSmokeFailure(string code)
        {
            if (_suppressGraphicsCardAssemblyHandoffSmokeSuccessMarker)
            {
                _nestedGraphicsCardAssemblyHandoffSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_GRAPHICS_CARD_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
