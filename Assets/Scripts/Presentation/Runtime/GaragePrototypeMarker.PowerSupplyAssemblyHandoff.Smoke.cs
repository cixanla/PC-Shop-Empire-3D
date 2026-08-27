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
        public const string PowerSupplyAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_POWER_SUPPLY_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 motherboard=secured " +
            "processor=retained memory=retained storage=secured cooler=retained " +
            "graphics-card=retained prerequisite-setup=assisted pickup=exact " +
            "custody=build-kit-to-hands-to-psu-bay reservation=alive " +
            "physical-identity=stable input=keyboard+mouse " +
            "orientation-invalid=blocked seat=ok four-screw=retained " +
            "retained-remove-blocked=ok unretain=ok detach=ok reseat=ok " +
            "history=10/10-preserved cables=3/3-untouched receipts=ok " +
            "revisions=ok no-duplicate-loss=ok invariants=ok";

        private bool _suppressPowerSupplyAssemblyHandoffSmokeSuccessMarker;
        private string _nestedPowerSupplyAssemblyHandoffSmokeFailureCode;

        public bool HasPowerSupplyAssemblyHandoffR51Runtime =>
            HasGraphicsCardAssemblyHandoffR50Runtime &&
            HasPowerSupplyR29Runtime &&
            HasPowerSupplyBuildKitR41Runtime &&
            powerSupplyBinding != null &&
            powerSupplyBinding.BuildKit == powerSupplyBuildKit &&
            powerSupplyBinding.PhysicalItem == powerSupply &&
            powerSupplyBinding.Slot == powerSupplyBay &&
            powerSupplyBay != null &&
            powerSupplyBay.AssemblyRoot != null &&
            Vector3.Dot(
                powerSupplyBay.SnapAnchor.up,
                powerSupplyBay.AssemblyRoot.up) > 0.999f &&
            Vector3.Dot(
                powerSupplyBay.SnapAnchor.forward,
                powerSupplyBay.AssemblyRoot.forward) > 0.999f &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypePowerSupplyAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypePowerSupplyBuildKitOperationId.Value &&
            stockFlow.Session.PrototypePowerSupplyAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeGraphicsCardAssemblyHandoffOperationId.Value &&
            stockFlow.Session.PowerSupplyBayContainerId !=
                stockFlow.Session.PowerSupplyBuildKitContainerId;

        private IEnumerator RunPowerSupplyAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedGraphicsCardAssemblyHandoffSmokeFailureCode = null;
            _suppressGraphicsCardAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunGraphicsCardAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressGraphicsCardAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedGraphicsCardAssemblyHandoffSmokeFailureCode;
            _nestedGraphicsCardAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogPowerSupplyAssemblyHandoffSmokeFailure(
                    $"smoke.graphics-card-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection physicalPowerSupply =
                powerSupplyBinding != null
                    ? powerSupplyBinding.PhysicalItem
                    : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                physicalPowerSupply == null ||
                !HasPowerSupplyAssemblyHandoffR51Runtime ||
                !powerSupplyBuildKit.IsStaged ||
                !powerSupplyBinding.IsAuthorityInBuildKit ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 6 ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.ProcessorRetained ||
                session.AssemblyBuild.MemorySlotState !=
                    MemorySlotState.MemoryModuleRetained ||
                session.AssemblyBuild.StorageSlotState !=
                    StorageSlotState.StorageDeviceSecured ||
                session.AssemblyBuild.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.CoolerRetained ||
                session.AssemblyBuild.GraphicsCardSlotState !=
                    GraphicsCardSlotState.GraphicsCardRetained ||
                session.AssemblyBuild.PowerSupplyBayState !=
                    PowerSupplyBayState.EmptyOpen)
            {
                LogPowerSupplyAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogPowerSupplyAssemblyHandoffSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot powerSupplyLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerSupply);
            if (powerSupplyLine == null ||
                powerSupplyLine.ItemId != session.PowerSupplyItemId ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                !TryCapturePowerSupplyAssemblyHandoffCableContainers(
                    session,
                    workOrder,
                    out Dictionary<StableId<ItemInstanceIdScope>,
                        StableId<ContainerIdScope>> cableContainers) ||
                !GraphicsCardAssemblyHandoffReservationsAreLive(
                    session,
                    workOrder,
                    workOrder.Lines.ToArray()) ||
                !session.TryGetAtx24PowerCableItem(
                    out InventoryItemRecord atx24Item) ||
                !session.TryGetEps12vPowerCableItem(
                    out InventoryItemRecord eps12vItem) ||
                !session.TryGetPcieGpuPowerCableItem(
                    out InventoryItemRecord pcieItem))
            {
                LogPowerSupplyAssemblyHandoffSmokeFailure(
                    "smoke.reservation-history-or-cable-mismatch");
                yield break;
            }

            int physicalIdentity = physicalPowerSupply.GetInstanceID();
            string stableItemId = physicalPowerSupply.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            StableId<ItemInstanceIdScope> atx24ItemId = atx24Item.Id;
            StableId<ProductDefinitionIdScope> atx24ProductId = atx24Item.ProductId;
            StableId<ContainerIdScope> atx24ContainerId = atx24Item.ContainerId;
            InventorySerializedItemStateFlags atx24Flags = atx24Item.StateFlags;
            Atx24PowerCableState atx24State =
                session.AssemblyBuild.Atx24PowerCableState;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            int atx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;

            StableId<ItemInstanceIdScope> eps12vItemId = eps12vItem.Id;
            StableId<ProductDefinitionIdScope> eps12vProductId =
                eps12vItem.ProductId;
            StableId<ContainerIdScope> eps12vContainerId = eps12vItem.ContainerId;
            InventorySerializedItemStateFlags eps12vFlags = eps12vItem.StateFlags;
            Eps12vPowerCableState eps12vState =
                session.AssemblyBuild.Eps12vPowerCableState;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            int eps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;

            StableId<ItemInstanceIdScope> pcieItemId = pcieItem.Id;
            StableId<ProductDefinitionIdScope> pcieProductId = pcieItem.ProductId;
            StableId<ContainerIdScope> pcieContainerId = pcieItem.ContainerId;
            InventorySerializedItemStateFlags pcieFlags = pcieItem.StateFlags;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;

            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeMouse = InputSystem.AddDevice<Mouse>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.Update();

                powerSupplyBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(
                    physicalPowerSupply,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalPowerSupply ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains("PSU'YU BAY MONTAJINA AL"))
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitAssemblyHandoffReceipt handoff = null;
                bool pickedUp =
                    playerCarry.HeldItem == physicalPowerSupply &&
                    powerSupplyBinding.IsAuthorityInHands &&
                    powerSupplyBuildKit.IsReleasedForAssembly &&
                    powerSupplyBuildKit.StagedComponentCount == 10 &&
                    powerSupplyBuildKit.ProgressText.text.Contains("PSU MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypePowerSupplyAssemblyHandoffOperationId,
                        out handoff) &&
                    handoff.ComponentKind == PcComponentKind.PowerSupply &&
                    ReferenceEquals(handoff.Line, powerSupplyLine) &&
                    ReferenceEquals(handoff.StagingReceipt, historicalReceipts[6]) &&
                    physicalPowerSupply.GetInstanceID() == physicalIdentity &&
                    physicalPowerSupply.ItemIdValue == stableItemId &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-build-kit-pickup-mismatch");
                    yield break;
                }

                long replayInventoryRevision = session.Inventory.Revision;
                long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
                long replayAssemblyRevision = session.AssemblyBuild.Revision;
                int replayAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                    session.PickupStagedPowerSupplyForAssembly();
                if (replay.IsFailure ||
                    !ReferenceEquals(replay.Value, handoff) ||
                    session.Inventory.Revision != replayInventoryRevision ||
                    session.CustomPcBuildKit.Revision != replayBuildKitRevision ||
                    session.AssemblyBuild.Revision != replayAssemblyRevision ||
                    session.AssemblyBuild.ReceiptCount != replayAssemblyReceiptCount)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.immediate-replay-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool worldDropBlocked =
                    playerCarry.HeldItem == physicalPowerSupply &&
                    powerSupplyBinding.IsAuthorityInHands &&
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
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.reserved-world-drop-not-blocked");
                    yield break;
                }

                MovePlayerToPowerSupplyBay();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool seatReady =
                    playerCarry.IsPowerSupplySeatMode &&
                    playerCarry.CurrentPowerSupplyBayStatus ==
                        PowerSupplyBayStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!seatReady)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool invalidOrientationReady =
                    playerCarry.IsPowerSupplySeatMode &&
                    playerCarry.CurrentPowerSupplyBayStatus ==
                        PowerSupplyBayStatus.OrientationInvalid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!invalidOrientationReady)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-invalid-orientation-mismatch");
                    yield break;
                }

                long invalidInventoryRevision = session.Inventory.Revision;
                long invalidAssemblyRevision = session.AssemblyBuild.Revision;
                int invalidAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool invalidOrientationBlocked =
                    playerCarry.HeldItem == physicalPowerSupply &&
                    playerCarry.LastFailureCode ==
                        "assembly-power-supply.orientation-mismatch" &&
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.EmptyOpen &&
                    session.Inventory.Revision == invalidInventoryRevision &&
                    session.AssemblyBuild.Revision == invalidAssemblyRevision &&
                    session.AssemblyBuild.ReceiptCount ==
                        invalidAssemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!invalidOrientationBlocked)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.invalid-orientation-not-fail-closed");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool primaryOrientationReady =
                    playerCarry.CurrentPowerSupplyBayStatus ==
                        PowerSupplyBayStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!primaryOrientationReady)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-primary-orientation-mismatch");
                    yield break;
                }

                OperationResult<Pose> primarySeatPose =
                    powerSupplyBay.ResolveSeatPose(0);
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool seated =
                    primarySeatPose.IsSuccess &&
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.PowerSupplySeatedUnsecured &&
                    session.AssemblyBuild.PowerSupplyMountOrientation ==
                        PowerSupplyMountOrientation.FanToFilteredVent &&
                    session.TryGetPowerSupplyItem(
                        out InventoryItemRecord seatedPowerSupply) &&
                    seatedPowerSupply.ContainerId ==
                        session.PowerSupplyBayContainerId &&
                    physicalPowerSupply.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalPowerSupply.IsStablePlacement &&
                    physicalPowerSupply.Body.isKinematic &&
                    !physicalPowerSupply.Body.useGravity &&
                    Vector3.Distance(
                        physicalPowerSupply.transform.position,
                        primarySeatPose.Value.position) <= 0.0005f &&
                    Quaternion.Angle(
                        physicalPowerSupply.transform.rotation,
                        primarySeatPose.Value.rotation) <= 0.05f;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!seated)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-seat-mismatch");
                    yield break;
                }

                MovePlayerToPowerSupplyBay();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool retained =
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.PowerSupplyRetained &&
                    powerSupplyBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!retained)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-retain-mismatch");
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
                        AssemblyFailures.PowerSupplyRetained.Code &&
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.PowerSupplyRetained &&
                    session.Inventory.Revision == retainedInventoryRevision &&
                    session.AssemblyBuild.Revision == retainedAssemblyRevision &&
                    session.AssemblyBuild.ReceiptCount ==
                        retainedAssemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!retainedRemoveBlocked)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.retained-detach-not-blocked");
                    yield break;
                }

                MovePlayerToPowerSupplyBay();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool unretained =
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.PowerSupplySeatedUnsecured &&
                    !powerSupplyBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!unretained)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-unretain-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool detached =
                    playerCarry.HeldItem == physicalPowerSupply &&
                    powerSupplyBinding.IsAuthorityInHands &&
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.EmptyOpen;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!detached)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-detach-mismatch");
                    yield break;
                }

                MovePlayerToPowerSupplyBay();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool reseatReady =
                    playerCarry.IsPowerSupplySeatMode &&
                    playerCarry.CurrentPowerSupplyBayStatus ==
                        PowerSupplyBayStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!reseatReady)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-reseat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool reseated =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.PowerSupplySeatedUnsecured;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!reseated)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.power-supply-reseat-mismatch");
                    yield break;
                }

                MovePlayerToPowerSupplyBay();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool finalRetention =
                    session.AssemblyBuild.PowerSupplyBayState ==
                        PowerSupplyBayState.PowerSupplyRetained &&
                    powerSupplyBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);

                bool cablesUnchanged =
                    session.TryGetAtx24PowerCableItem(
                        out InventoryItemRecord currentAtx24) &&
                    currentAtx24.Id == atx24ItemId &&
                    currentAtx24.ProductId == atx24ProductId &&
                    currentAtx24.ContainerId == atx24ContainerId &&
                    currentAtx24.StateFlags == atx24Flags &&
                    session.AssemblyBuild.Atx24PowerCableState == atx24State &&
                    session.AssemblyBuild.Atx24PowerCableRevision ==
                        atx24Revision &&
                    session.AssemblyBuild.Atx24PowerCableReceiptCount ==
                        atx24ReceiptCount &&
                    session.TryGetEps12vPowerCableItem(
                        out InventoryItemRecord currentEps12v) &&
                    currentEps12v.Id == eps12vItemId &&
                    currentEps12v.ProductId == eps12vProductId &&
                    currentEps12v.ContainerId == eps12vContainerId &&
                    currentEps12v.StateFlags == eps12vFlags &&
                    session.AssemblyBuild.Eps12vPowerCableState == eps12vState &&
                    session.AssemblyBuild.Eps12vPowerCableRevision ==
                        eps12vRevision &&
                    session.AssemblyBuild.Eps12vPowerCableReceiptCount ==
                        eps12vReceiptCount &&
                    session.TryGetPcieGpuPowerCableItem(
                        out InventoryItemRecord currentPcie) &&
                    currentPcie.Id == pcieItemId &&
                    currentPcie.ProductId == pcieProductId &&
                    currentPcie.ContainerId == pcieContainerId &&
                    currentPcie.StateFlags == pcieFlags &&
                    session.AssemblyBuild.PcieGpuPowerCableState == pcieState &&
                    session.AssemblyBuild.PcieGpuPowerCableRevision ==
                        pcieRevision &&
                    session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                        pcieReceiptCount;

                bool finalState =
                    finalRetention &&
                    cablesUnchanged &&
                    session.Inventory.Revision == inventoryRevision + 4 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 6 &&
                    session.AssemblyBuild.ReceiptCount ==
                        assemblyReceiptCount + 6 &&
                    session.CustomPcBuildKit.StagedComponentCount == 10 &&
                    session.CustomPcBuildKit.AssemblyHandoffCount == 7 &&
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
                    session.AssemblyBuild.GraphicsCardSlotState ==
                        GraphicsCardSlotState.GraphicsCardRetained &&
                    graphicsCardBinding.IsRetained &&
                    physicalPowerSupply.GetInstanceID() == physicalIdentity &&
                    physicalPowerSupply.ItemIdValue == stableItemId &&
                    physicalPowerSupply.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalPowerSupply.IsStablePlacement &&
                    primarySeatPose.IsSuccess &&
                    Vector3.Distance(
                        physicalPowerSupply.transform.position,
                        primarySeatPose.Value.position) <= 0.0005f &&
                    Quaternion.Angle(
                        physicalPowerSupply.transform.rotation,
                        primarySeatPose.Value.rotation) <= 0.05f &&
                    GraphicsCardAssemblyHandoffReservationsAreLive(
                        session,
                        workOrder,
                        workOrder.Lines.ToArray()) &&
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        cableContainers) &&
                    CountCanonicalPowerSupplyProjections(
                        GarageStockFlowSession.PowerSupplyItemInstanceIdValue) == 1 &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorCoolerBinding.ValidateProjectionInvariant().IsSuccess &&
                    graphicsCardBinding.ValidateProjectionInvariant().IsSuccess &&
                    powerSupplyBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!finalState)
                {
                    LogPowerSupplyAssemblyHandoffSmokeFailure(
                        "smoke.final-state-or-invariant-mismatch");
                    yield break;
                }

                if (!_suppressPowerSupplyAssemblyHandoffSmokeSuccessMarker)
                {
                    Debug.Log(PowerSupplyAssemblyHandoffSmokeSuccessMarker);
                }

                yield return new WaitForEndOfFrame();
                if (!Application.isEditor &&
                    !_suppressPowerSupplyAssemblyHandoffSmokeSuccessMarker)
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

        private static bool TryCapturePowerSupplyAssemblyHandoffCableContainers(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            out Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>> containers)
        {
            containers = new Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>>();
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind != PcComponentKind.PowerCable)
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

            return containers.Count == 3;
        }

        private void LogPowerSupplyAssemblyHandoffSmokeFailure(string code)
        {
            if (_suppressPowerSupplyAssemblyHandoffSmokeSuccessMarker)
            {
                _nestedPowerSupplyAssemblyHandoffSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_POWER_SUPPLY_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
