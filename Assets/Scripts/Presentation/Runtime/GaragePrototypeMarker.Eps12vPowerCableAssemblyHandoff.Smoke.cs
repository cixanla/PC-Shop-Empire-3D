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
        public const string Eps12vPowerCableAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_EPS12V_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 assembly-chain=7/7 " +
            "atx24=routed pickup=exact " +
            "custody=build-kit-to-hands-to-route-to-hands " +
            "reservation=alive physical-identity=stable input=keyboard+mouse " +
            "generic-drop=blocked route=ok psu-unretain=blocked unroute=ok " +
            "history=10/10-preserved cables=2/2-protected " +
            "replay=immediate+delayed receipts=ok revisions=ok " +
            "electrical-readiness=blocked no-duplicate-loss=ok invariants=ok";

        private IEnumerator RunEps12vPowerCableAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedAtx24PowerCableAssemblyHandoffSmokeFailureCode = null;
            _suppressAtx24PowerCableAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunAtx24PowerCableAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressAtx24PowerCableAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedAtx24PowerCableAssemblyHandoffSmokeFailureCode;
            _nestedAtx24PowerCableAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                    $"smoke.atx24-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection physicalCable =
                eps12vPowerCableBinding != null
                    ? eps12vPowerCableBinding.PhysicalItem
                    : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                physicalCable == null ||
                eps12vPowerCableBuildKit == null ||
                eps12vPowerCableRoute == null ||
                eps12vPowerCableGeometry == null ||
                !HasEps12vPowerCableAssemblyHandoffR53Runtime ||
                !eps12vPowerCableBuildKit.IsStaged ||
                eps12vPowerCableBuildKit.IsReleasedForAssembly ||
                !eps12vPowerCableBinding.IsAuthorityInBuildKit ||
                playerCarry.HeldItem != atx24PowerCable ||
                !atx24PowerCableBinding.IsAuthorityInHands ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 8 ||
                !Atx24PowerCableAssemblyHandoffPrerequisitesAreRetained(session) ||
                session.AssemblyBuild.Atx24PowerCableState !=
                    Atx24PowerCableState.Loose ||
                session.AssemblyBuild.Atx24PowerCableRevision != 2 ||
                session.AssemblyBuild.Eps12vPowerCableState !=
                    Eps12vPowerCableState.Loose ||
                session.AssemblyBuild.Eps12vPowerCableRevision != 0 ||
                session.AssemblyBuild.Eps12vPowerCableReceiptCount != 0)
            {
                LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _) ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                historicalReceipts.Length != 10)
            {
                LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                    "smoke.work-ticket-or-history-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot eps12vLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerCable &&
                    line.PowerCableType ==
                        PowerCableType.ModularEps12v8PinPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot atx24Line = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerCable &&
                    line.PowerCableType ==
                        PowerCableType.ModularAtx24SplitPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot pcieLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerCable &&
                    line.PowerCableType ==
                        PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            if (eps12vLine == null ||
                atx24Line == null ||
                pcieLine == null ||
                eps12vLine.ItemId != session.Eps12vPowerCableItemId ||
                eps12vLine.ProductId != session.Eps12vPowerCableProductId ||
                !session.TryGetPcieGpuPowerCableItem(
                    out InventoryItemRecord pcieItem) ||
                !GraphicsCardAssemblyHandoffReservationsAreLive(
                    session,
                    workOrder,
                    workOrder.Lines.ToArray()))
            {
                LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                    "smoke.line-reservation-or-pcie-mismatch");
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

                MovePlayerToAtx24AssemblyHandoffRoute();
                OperationResult atxMode = playerCarry
                    .TrySetAtx24PowerCableRouteMode(true);
                OperationResult atxRoute = atxMode.IsSuccess
                    ? playerCarry.TryConfirmAtx24PowerCableRoute()
                    : atxMode;
                if (atxRoute.IsFailure ||
                    playerCarry.HeldItem != null ||
                    !atx24PowerCableBinding.IsRouted ||
                    session.AssemblyBuild.Atx24PowerCableState !=
                        Atx24PowerCableState.Routed ||
                    !session.TryGetAtx24PowerCableItem(
                        out InventoryItemRecord protectedAtx24) ||
                    protectedAtx24.Id != atx24Line.ItemId ||
                    protectedAtx24.ProductId != atx24Line.ProductId ||
                    protectedAtx24.ContainerId !=
                        session.Atx24PowerCableRouteContainerId)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        atxRoute.IsFailure
                            ? $"smoke.atx24-route-{atxRoute.Error.Code}"
                            : "smoke.atx24-route-mismatch");
                    yield break;
                }

                StableId<AssemblyOperationIdScope> protectedAtx24RouteId =
                    session.AssemblyBuild.Atx24PowerCableRoutedByOperationId;
                long protectedAtx24Revision =
                    session.AssemblyBuild.Atx24PowerCableRevision;
                int protectedAtx24ReceiptCount =
                    session.AssemblyBuild.Atx24PowerCableReceiptCount;
                StableId<ContainerIdScope> protectedPcieContainer =
                    pcieItem.ContainerId;
                InventorySerializedItemStateFlags protectedPcieFlags =
                    pcieItem.StateFlags;
                long protectedPcieRevision =
                    session.AssemblyBuild.PcieGpuPowerCableRevision;
                int protectedPcieReceiptCount =
                    session.AssemblyBuild.PcieGpuPowerCableReceiptCount;

                int physicalIdentity = physicalCable.GetInstanceID();
                string stableItemId = physicalCable.ItemIdValue;
                long inventoryRevision = session.Inventory.Revision;
                long buildKitRevision = session.CustomPcBuildKit.Revision;
                long assemblyRevision = session.AssemblyBuild.Revision;
                int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
                long ordersRevision = session.Orders.Revision;
                long offersRevision = session.RetailOffers.Revision;
                long basketsRevision = session.RetailBaskets.Revision;
                long checkoutsRevision = session.RetailCheckouts.Revision;
                long settlementsRevision =
                    session.CheckoutSettlements.Revision;
                long visitsRevision = session.CustomerVisits.Revision;
                long consultationsRevision =
                    session.CustomerConsultations.Revision;
                long offerActionsRevision =
                    session.CustomerOfferActions.Revision;

                eps12vPowerCableBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(
                    physicalCable,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCable ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains(
                        "EPS12V'Yİ KABLO MONTAJINA AL"))
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.eps12v-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitAssemblyHandoffReceipt handoff = null;
                bool pickedUp =
                    playerCarry.HeldItem == physicalCable &&
                    eps12vPowerCableBinding.IsAuthorityInHands &&
                    eps12vPowerCableBuildKit.IsReleasedForAssembly &&
                    eps12vPowerCableBuildKit.ProgressText.text.Contains(
                        "EPS12V MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                        out handoff) &&
                    handoff.Line.PowerCableType ==
                        PowerCableType.ModularEps12v8PinPsuToMotherboard &&
                    ReferenceEquals(handoff.Line, eps12vLine) &&
                    ReferenceEquals(handoff.StagingReceipt, historicalReceipts[8]) &&
                    handoff.WorkbenchContainerId ==
                        session.Eps12vPowerCableRouteContainerId &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.eps12v-build-kit-pickup-mismatch");
                    yield break;
                }

                long replayInventoryRevision = session.Inventory.Revision;
                long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                    session.PickupStagedEps12vPowerCableForAssembly();
                if (replay.IsFailure ||
                    !ReferenceEquals(replay.Value, handoff) ||
                    session.Inventory.Revision != replayInventoryRevision ||
                    session.CustomPcBuildKit.Revision != replayBuildKitRevision)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.immediate-handoff-replay-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool worldDropBlocked =
                    playerCarry.HeldItem == physicalCable &&
                    playerCarry.LastFailureCode ==
                        InventoryFailures
                            .SerializedReservationWorkOrderBuildKitConflict.Code &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!worldDropBlocked)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.reserved-world-drop-not-blocked");
                    yield break;
                }

                MovePlayerToEps12vPowerCableRoute();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool routeReady =
                    playerCarry.IsEps12vPowerCableRouteMode &&
                    !playerCarry.IsEps12vPowerCableBuildKitMode &&
                    playerCarry.CurrentEps12vPowerCableRouteStatus ==
                        Eps12vPowerCableRouteStatus.ValidRoute &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!routeReady)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        string.IsNullOrEmpty(playerCarry.LastFailureCode)
                            ? "smoke.eps12v-route-preflight-mismatch"
                            : playerCarry.LastFailureCode);
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                StableId<AssemblyOperationIdScope> routeOperationId =
                    Eps12vPowerCablePrototypeOperationId("route", 1);
                Eps12vPowerCableOperationReceipt routeReceipt = null;
                bool routed =
                    playerCarry.HeldItem == null &&
                    eps12vPowerCableBinding.IsRouted &&
                    eps12vPowerCableGeometry.IsRouted &&
                    session.AssemblyBuild.Eps12vPowerCableState ==
                        Eps12vPowerCableState.Routed &&
                    session.AssemblyBuild.TryGetEps12vPowerCableReceipt(
                        routeOperationId,
                        out routeReceipt) &&
                    routeReceipt.ItemId == eps12vLine.ItemId &&
                    routeReceipt.ProductId == eps12vLine.ProductId &&
                    routeReceipt.SourceContainerId == session.HandsContainerId &&
                    routeReceipt.TargetContainerId ==
                        session.Eps12vPowerCableRouteContainerId &&
                    session.Inventory.Revision == inventoryRevision + 2 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    session.AssemblyBuild.Eps12vPowerCableRevision == 1 &&
                    session.AssemblyBuild.Eps12vPowerCableReceiptCount == 1 &&
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().Error ==
                        AssemblyFailures.BuildIncomplete &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!routed)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.eps12v-route-mismatch");
                    yield break;
                }

                AssemblyBuildSnapshot routedSnapshot =
                    session.AssemblyBuild.GetSnapshot();
                OperationResult<AssemblyOperationReceipt> blockedPsuUnretain =
                    session.UnretainPowerSupply(
                        StableId<AssemblyOperationIdScope>.Parse(
                            "assembly.operation.runtime-smoke.issue107-" +
                            "blocked-psu-unretain"),
                        routedSnapshot.PowerSupplySeatedByOperationId,
                        routedSnapshot.PowerSupplyRetainedByOperationId,
                        routedSnapshot.Revision);
                if (blockedPsuUnretain.Error !=
                    AssemblyFailures.PowerCableDependentComponentLocked)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.routed-psu-unretain-not-blocked");
                    yield break;
                }

                MovePlayerToEps12vPowerCableRoute();
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCable)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.routed-eps12v-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                StableId<AssemblyOperationIdScope> unrouteOperationId =
                    Eps12vPowerCablePrototypeOperationId("unroute", 2);
                Eps12vPowerCableOperationReceipt unrouteReceipt = null;
                bool unrouted =
                    playerCarry.HeldItem == physicalCable &&
                    !eps12vPowerCableBinding.IsRouted &&
                    !eps12vPowerCableGeometry.IsRouted &&
                    eps12vPowerCableBinding.IsAuthorityInHands &&
                    session.AssemblyBuild.Eps12vPowerCableState ==
                        Eps12vPowerCableState.Loose &&
                    session.AssemblyBuild.TryGetEps12vPowerCableReceipt(
                        unrouteOperationId,
                        out unrouteReceipt) &&
                    unrouteReceipt.SourceRouteOperationId == routeOperationId &&
                    session.Inventory.Revision == inventoryRevision + 3 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Eps12vPowerCableRevision == 2 &&
                    session.AssemblyBuild.Eps12vPowerCableReceiptCount == 2 &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!unrouted)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.eps12v-unroute-mismatch");
                    yield break;
                }

                long delayedInventoryRevision = session.Inventory.Revision;
                long delayedBuildKitRevision = session.CustomPcBuildKit.Revision;
                long delayedAssemblyRevision = session.AssemblyBuild.Revision;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
                    delayedHandoff =
                        session.PickupStagedEps12vPowerCableForAssembly();
                OperationResult<Eps12vPowerCableOperationReceipt> routeReplay =
                    session.RouteEps12vPowerCable(
                        routeOperationId,
                        PowerCableKeyOrientation.Keyed,
                        routeReceipt.ExpectedCableRevision);
                OperationResult<Eps12vPowerCableOperationReceipt> unrouteReplay =
                    session.UnrouteEps12vPowerCable(
                        unrouteOperationId,
                        routeOperationId,
                        unrouteReceipt.ExpectedCableRevision);
                bool delayedReplay =
                    delayedHandoff.IsSuccess &&
                    ReferenceEquals(delayedHandoff.Value, handoff) &&
                    routeReplay.IsSuccess &&
                    ReferenceEquals(routeReplay.Value, routeReceipt) &&
                    unrouteReplay.IsSuccess &&
                    ReferenceEquals(unrouteReplay.Value, unrouteReceipt) &&
                    session.Inventory.Revision == delayedInventoryRevision &&
                    session.CustomPcBuildKit.Revision == delayedBuildKitRevision &&
                    session.AssemblyBuild.Revision == delayedAssemblyRevision &&
                    session.AssemblyBuild
                        .ValidateEps12vPowerCableReceiptHistory().IsSuccess;
                if (!delayedReplay)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.delayed-replay-mismatch");
                    yield break;
                }

                bool protectedCables =
                    session.TryGetAtx24PowerCableItem(
                        out InventoryItemRecord currentAtx24) &&
                    currentAtx24.Id == protectedAtx24.Id &&
                    currentAtx24.ProductId == protectedAtx24.ProductId &&
                    currentAtx24.ContainerId ==
                        session.Atx24PowerCableRouteContainerId &&
                    session.AssemblyBuild.Atx24PowerCableState ==
                        Atx24PowerCableState.Routed &&
                    session.AssemblyBuild.Atx24PowerCableRoutedByOperationId ==
                        protectedAtx24RouteId &&
                    session.AssemblyBuild.Atx24PowerCableRevision ==
                        protectedAtx24Revision &&
                    session.AssemblyBuild.Atx24PowerCableReceiptCount ==
                        protectedAtx24ReceiptCount &&
                    session.TryGetPcieGpuPowerCableItem(
                        out InventoryItemRecord currentPcie) &&
                    currentPcie.Id == pcieItem.Id &&
                    currentPcie.ProductId == pcieItem.ProductId &&
                    currentPcie.ContainerId == protectedPcieContainer &&
                    currentPcie.StateFlags == protectedPcieFlags &&
                    session.AssemblyBuild.PcieGpuPowerCableState ==
                        PcieGpuPowerCableState.Loose &&
                    session.AssemblyBuild.PcieGpuPowerCableRevision ==
                        protectedPcieRevision &&
                    session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                        protectedPcieReceiptCount;
                var protectedContainers = new Dictionary<
                    StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                {
                    [pcieLine.ItemId] = protectedPcieContainer
                };
                bool historyPreserved =
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        protectedContainers);
                bool isolatedAuthorities =
                    session.Orders.Revision == ordersRevision &&
                    session.RetailOffers.Revision == offersRevision &&
                    session.RetailBaskets.Revision == basketsRevision &&
                    session.RetailCheckouts.Revision == checkoutsRevision &&
                    session.CheckoutSettlements.Revision == settlementsRevision &&
                    session.CustomerVisits.Revision == visitsRevision &&
                    session.CustomerConsultations.Revision ==
                        consultationsRevision &&
                    session.CustomerOfferActions.Revision ==
                        offerActionsRevision;

                if (!protectedCables ||
                    !historyPreserved ||
                    !isolatedAuthorities ||
                    session.CustomPcBuildKit.StagedComponentCount != 10 ||
                    session.CustomPcBuildKit.AssemblyHandoffCount != 9 ||
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().Error !=
                        AssemblyFailures.BuildIncomplete ||
                    !GraphicsCardAssemblyHandoffReservationsAreLive(
                        session,
                        workOrder,
                        workOrder.Lines.ToArray()) ||
                    CountCanonicalEps12vPowerCableProjections(stableItemId) != 1 ||
                    physicalCable.GetInstanceID() != physicalIdentity ||
                    physicalCable.ItemIdValue != stableItemId ||
                    !physicalCable.IsCarried ||
                    !eps12vPowerCableBuildKit.ProgressText.text.Contains(
                        "EPS12V MONTAJDA"))
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.final-authority-history-or-identity-mismatch");
                    yield break;
                }

                OperationResult[] projectionInvariants =
                {
                    motherboardBinding.ValidateProjectionInvariant(),
                    processorBinding.ValidateProjectionInvariant(),
                    dimmBinding.ValidateProjectionInvariant(),
                    storageBinding.ValidateProjectionInvariant(),
                    processorCoolerBinding.ValidateProjectionInvariant(),
                    graphicsCardBinding.ValidateProjectionInvariant(),
                    powerSupplyBinding.ValidateProjectionInvariant(),
                    atx24PowerCableBinding.ValidateProjectionInvariant(),
                    eps12vPowerCableBinding.ValidateProjectionInvariant()
                };
                if (Array.Exists(
                        projectionInvariants,
                        invariant => invariant.IsFailure) ||
                    session.ValidateInvariants().IsFailure)
                {
                    LogEps12vPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.final-invariant-mismatch");
                    yield break;
                }

                Debug.Log(Eps12vPowerCableAssemblyHandoffSmokeSuccessMarker);
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

        private void LogEps12vPowerCableAssemblyHandoffSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_EPS12V_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
