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
        public const string PcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_PCIE_GPU_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 assembly-chain=7/7 " +
            "atx24+eps12v=routed pickup=exact " +
            "custody=build-kit-to-hands-to-route-to-hands " +
            "reservation=alive physical-identity=stable input=keyboard+mouse " +
            "generic-drop=blocked route=ok psu-unretain=blocked unroute=ok " +
            "history=10/10-preserved cables=2/2-protected " +
            "replay=immediate+delayed receipts=ok revisions=ok " +
            "electrical-readiness=ready-then-blocked power-budget=380/500/550 " +
            "monitor=ok " +
            "no-duplicate-loss=ok invariants=ok";

        private bool _suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker;
        private string _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode;

        private IEnumerator RunPcieGpuPowerCableAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedEps12vPowerCableAssemblyHandoffSmokeFailureCode = null;
            _suppressEps12vPowerCableAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunEps12vPowerCableAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressEps12vPowerCableAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedEps12vPowerCableAssemblyHandoffSmokeFailureCode;
            _nestedEps12vPowerCableAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                    $"smoke.eps12v-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection physicalCable =
                pcieGpuPowerCableBinding != null
                    ? pcieGpuPowerCableBinding.PhysicalItem
                    : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                physicalCable == null ||
                pcieGpuPowerCableBuildKit == null ||
                pcieGpuPowerCableRoute == null ||
                pcieGpuPowerCableGeometry == null ||
                electricalReadinessWorkbench == null ||
                !HasPowerBudgetWorkbenchR59Runtime ||
                !HasPcieGpuPowerCableAssemblyHandoffR54Runtime ||
                !pcieGpuPowerCableBuildKit.IsStaged ||
                pcieGpuPowerCableBuildKit.IsReleasedForAssembly ||
                !pcieGpuPowerCableBinding.IsAuthorityInBuildKit ||
                playerCarry.HeldItem != eps12vPowerCable ||
                !eps12vPowerCableBinding.IsAuthorityInHands ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 9 ||
                !Atx24PowerCableAssemblyHandoffPrerequisitesAreRetained(session) ||
                session.AssemblyBuild.Atx24PowerCableState !=
                    Atx24PowerCableState.Routed ||
                session.AssemblyBuild.Eps12vPowerCableState !=
                    Eps12vPowerCableState.Loose ||
                session.AssemblyBuild.Eps12vPowerCableRevision != 2 ||
                session.AssemblyBuild.PcieGpuPowerCableState !=
                    PcieGpuPowerCableState.Loose ||
                session.AssemblyBuild.PcieGpuPowerCableRevision != 0 ||
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount != 0)
            {
                LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
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
                LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                    "smoke.work-ticket-or-history-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot pcieGpuLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerCable &&
                    line.PowerCableType ==
                        PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            CustomPcBuildOrderLineSnapshot atx24Line = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerCable &&
                    line.PowerCableType ==
                        PowerCableType.ModularAtx24SplitPsuToMotherboard);
            CustomPcBuildOrderLineSnapshot eps12vLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerCable &&
                    line.PowerCableType ==
                        PowerCableType.ModularEps12v8PinPsuToMotherboard);
            if (pcieGpuLine == null ||
                atx24Line == null ||
                eps12vLine == null ||
                pcieGpuLine.ItemId != session.PcieGpuPowerCableItemId ||
                pcieGpuLine.ProductId != session.PcieGpuPowerCableProductId ||
                !GraphicsCardAssemblyHandoffReservationsAreLive(
                    session,
                    workOrder,
                    workOrder.Lines.ToArray()))
            {
                LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
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

                MovePlayerToEps12vPowerCableRoute();
                OperationResult eps12vMode = playerCarry
                    .TrySetEps12vPowerCableRouteMode(true);
                OperationResult eps12vRoute = eps12vMode.IsSuccess
                    ? playerCarry.TryConfirmEps12vPowerCableRoute()
                    : eps12vMode;
                if (eps12vRoute.IsFailure ||
                    playerCarry.HeldItem != null ||
                    !eps12vPowerCableBinding.IsRouted ||
                    session.AssemblyBuild.Eps12vPowerCableState !=
                        Eps12vPowerCableState.Routed ||
                    !session.TryGetEps12vPowerCableItem(
                        out InventoryItemRecord protectedEps12v) ||
                    protectedEps12v.Id != eps12vLine.ItemId ||
                    protectedEps12v.ProductId != eps12vLine.ProductId ||
                    protectedEps12v.ContainerId !=
                        session.Eps12vPowerCableRouteContainerId ||
                    !session.TryGetAtx24PowerCableItem(
                        out InventoryItemRecord protectedAtx24) ||
                    protectedAtx24.Id != atx24Line.ItemId ||
                    protectedAtx24.ProductId != atx24Line.ProductId ||
                    protectedAtx24.ContainerId !=
                        session.Atx24PowerCableRouteContainerId)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        eps12vRoute.IsFailure
                            ? $"smoke.eps12v-route-{eps12vRoute.Error.Code}"
                            : "smoke.eps12v-route-mismatch");
                    yield break;
                }

                OperationResult electricalBeforePcie =
                    electricalReadinessWorkbench.RefreshPresentation();
                if (electricalBeforePcie.Error !=
                        ElectricalReadinessFailures.PcieGpuPowerCableMissing ||
                    electricalReadinessWorkbench.IsReady ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "PCIe GPU 6+2 KABLOSUNU BAĞLA"))
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.electrical-monitor-pre-route-mismatch");
                    yield break;
                }

                StableId<AssemblyOperationIdScope> protectedAtx24RouteId =
                    session.AssemblyBuild.Atx24PowerCableRoutedByOperationId;
                long protectedAtx24Revision =
                    session.AssemblyBuild.Atx24PowerCableRevision;
                int protectedAtx24ReceiptCount =
                    session.AssemblyBuild.Atx24PowerCableReceiptCount;
                StableId<AssemblyOperationIdScope> protectedEps12vRouteId =
                    session.AssemblyBuild.Eps12vPowerCableRoutedByOperationId;
                long protectedEps12vRevision =
                    session.AssemblyBuild.Eps12vPowerCableRevision;
                int protectedEps12vReceiptCount =
                    session.AssemblyBuild.Eps12vPowerCableReceiptCount;

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

                pcieGpuPowerCableBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(
                    physicalCable,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCable ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains(
                        "PCIe GPU 6+2'Yİ KABLO MONTAJINA AL"))
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.pcieGpu-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitAssemblyHandoffReceipt handoff = null;
                bool pickedUp =
                    playerCarry.HeldItem == physicalCable &&
                    pcieGpuPowerCableBinding.IsAuthorityInHands &&
                    pcieGpuPowerCableBuildKit.IsReleasedForAssembly &&
                    pcieGpuPowerCableBuildKit.ProgressText.text.Contains(
                        "PCIe GPU MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                        out handoff) &&
                    handoff.Line.PowerCableType ==
                        PowerCableType.ModularPcie8PinPsuToGraphicsCard &&
                    ReferenceEquals(handoff.Line, pcieGpuLine) &&
                    ReferenceEquals(handoff.StagingReceipt, historicalReceipts[9]) &&
                    handoff.WorkbenchContainerId ==
                        session.PcieGpuPowerCableRouteContainerId &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.pcieGpu-build-kit-pickup-mismatch");
                    yield break;
                }

                long replayInventoryRevision = session.Inventory.Revision;
                long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                    session.PickupStagedPcieGpuPowerCableForAssembly();
                if (replay.IsFailure ||
                    !ReferenceEquals(replay.Value, handoff) ||
                    session.Inventory.Revision != replayInventoryRevision ||
                    session.CustomPcBuildKit.Revision != replayBuildKitRevision)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
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
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.reserved-world-drop-not-blocked");
                    yield break;
                }

                MovePlayerToPcieGpuPowerCableRoute();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool routeReady =
                    playerCarry.IsPcieGpuPowerCableRouteMode &&
                    !playerCarry.IsPcieGpuPowerCableBuildKitMode &&
                    playerCarry.CurrentPcieGpuPowerCableRouteStatus ==
                        PcieGpuPowerCableRouteStatus.ValidRoute &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!routeReady)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        string.IsNullOrEmpty(playerCarry.LastFailureCode)
                            ? "smoke.pcieGpu-route-preflight-mismatch"
                            : playerCarry.LastFailureCode);
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                StableId<AssemblyOperationIdScope> routeOperationId =
                    PcieGpuPowerCablePrototypeOperationId("route", 1);
                PcieGpuPowerCableOperationReceipt routeReceipt = null;
                OperationResult electricalReady =
                    electricalReadinessWorkbench.RefreshPresentation();
                OperationResult<PcPowerBudgetSnapshot> powerBudget =
                    session.PowerBudget.AssessPowerBudget();
                bool routed =
                    playerCarry.HeldItem == null &&
                    pcieGpuPowerCableBinding.IsRouted &&
                    pcieGpuPowerCableGeometry.IsRouted &&
                    session.AssemblyBuild.PcieGpuPowerCableState ==
                        PcieGpuPowerCableState.Routed &&
                    session.AssemblyBuild.TryGetPcieGpuPowerCableReceipt(
                        routeOperationId,
                        out routeReceipt) &&
                    routeReceipt.ItemId == pcieGpuLine.ItemId &&
                    routeReceipt.ProductId == pcieGpuLine.ProductId &&
                    routeReceipt.SourceContainerId == session.HandsContainerId &&
                    routeReceipt.TargetContainerId ==
                        session.PcieGpuPowerCableRouteContainerId &&
                    session.Inventory.Revision == inventoryRevision + 2 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    session.AssemblyBuild.PcieGpuPowerCableRevision == 1 &&
                    session.AssemblyBuild.PcieGpuPowerCableReceiptCount == 1 &&
                    electricalReady.IsSuccess &&
                    powerBudget.IsSuccess &&
                    powerBudget.Value.IsSufficient &&
                    powerBudget.Value.SystemPowerDrawWatts == 380 &&
                    powerBudget.Value.MinimumRecommendedPsuWatts == 500 &&
                    powerBudget.Value.InstalledPsuWatts == 550 &&
                    electricalReadinessWorkbench.IsReady &&
                    electricalReadinessWorkbench.StatusText.text.Contains(
                        "GÜÇ BÜTÇESİ UYGUN") &&
                    electricalReadinessWorkbench.StatusText.text.Contains(
                        "380W / EN AZ 500W / PSU 550W") &&
                    electricalReadinessWorkbench.StatusText.text.Contains(
                        "GÜÇ TESTİ BEKLİYOR") &&
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().Error ==
                        AssemblyFailures.BuildIncomplete &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!routed)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.pcieGpu-route-mismatch");
                    yield break;
                }

                AssemblyBuildSnapshot routedSnapshot =
                    session.AssemblyBuild.GetSnapshot();
                OperationResult<AssemblyOperationReceipt> blockedPsuUnretain =
                    session.UnretainPowerSupply(
                        StableId<AssemblyOperationIdScope>.Parse(
                            "assembly.operation.runtime-smoke.issue109-" +
                            "blocked-psu-unretain"),
                        routedSnapshot.PowerSupplySeatedByOperationId,
                        routedSnapshot.PowerSupplyRetainedByOperationId,
                        routedSnapshot.Revision);
                if (blockedPsuUnretain.Error !=
                    AssemblyFailures.PowerCableDependentComponentLocked)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.routed-psu-unretain-not-blocked");
                    yield break;
                }

                AimMotherboardBuildKitSmokeAtItem(
                    physicalCable,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCable)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.routed-pcieGpu-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                StableId<AssemblyOperationIdScope> unrouteOperationId =
                    PcieGpuPowerCablePrototypeOperationId("unroute", 2);
                PcieGpuPowerCableOperationReceipt unrouteReceipt = null;
                OperationResult electricalBlockedAgain =
                    electricalReadinessWorkbench.RefreshPresentation();
                bool unrouted =
                    playerCarry.HeldItem == physicalCable &&
                    !pcieGpuPowerCableBinding.IsRouted &&
                    !pcieGpuPowerCableGeometry.IsRouted &&
                    pcieGpuPowerCableBinding.IsAuthorityInHands &&
                    session.AssemblyBuild.PcieGpuPowerCableState ==
                        PcieGpuPowerCableState.Loose &&
                    session.AssemblyBuild.TryGetPcieGpuPowerCableReceipt(
                        unrouteOperationId,
                        out unrouteReceipt) &&
                    unrouteReceipt.SourceRouteOperationId == routeOperationId &&
                    session.Inventory.Revision == inventoryRevision + 3 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.PcieGpuPowerCableRevision == 2 &&
                    session.AssemblyBuild.PcieGpuPowerCableReceiptCount == 2 &&
                    electricalBlockedAgain.Error ==
                        ElectricalReadinessFailures.PcieGpuPowerCableMissing &&
                    !electricalReadinessWorkbench.IsReady &&
                    electricalReadinessWorkbench.StatusText.text.Contains(
                        "GÜÇ HAZIR DEĞİL") &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!unrouted)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.pcieGpu-unroute-mismatch");
                    yield break;
                }

                long delayedInventoryRevision = session.Inventory.Revision;
                long delayedBuildKitRevision = session.CustomPcBuildKit.Revision;
                long delayedAssemblyRevision = session.AssemblyBuild.Revision;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
                    delayedHandoff =
                        session.PickupStagedPcieGpuPowerCableForAssembly();
                OperationResult<PcieGpuPowerCableOperationReceipt> routeReplay =
                    session.RoutePcieGpuPowerCable(
                        routeOperationId,
                        PowerCableKeyOrientation.Keyed,
                        routeReceipt.ExpectedCableRevision);
                OperationResult<PcieGpuPowerCableOperationReceipt> unrouteReplay =
                    session.UnroutePcieGpuPowerCable(
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
                        .ValidatePcieGpuPowerCableReceiptHistory().IsSuccess;
                if (!delayedReplay)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
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
                    session.TryGetEps12vPowerCableItem(
                        out InventoryItemRecord currentEps12v) &&
                    currentEps12v.Id == protectedEps12v.Id &&
                    currentEps12v.ProductId == protectedEps12v.ProductId &&
                    currentEps12v.ContainerId ==
                        session.Eps12vPowerCableRouteContainerId &&
                    session.AssemblyBuild.Eps12vPowerCableState ==
                        Eps12vPowerCableState.Routed &&
                    session.AssemblyBuild.Eps12vPowerCableRoutedByOperationId ==
                        protectedEps12vRouteId &&
                    session.AssemblyBuild.Eps12vPowerCableRevision ==
                        protectedEps12vRevision &&
                    session.AssemblyBuild.Eps12vPowerCableReceiptCount ==
                        protectedEps12vReceiptCount;
                var protectedContainers = new Dictionary<
                    StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>();
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
                    session.CustomPcBuildKit.AssemblyHandoffCount != 10 ||
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().Error !=
                        AssemblyFailures.BuildIncomplete ||
                    electricalReadinessWorkbench.IsReady ||
                    electricalReadinessWorkbench.CurrentFailureCode !=
                        ElectricalReadinessFailures.PcieGpuPowerCableMissing.Code ||
                    !GraphicsCardAssemblyHandoffReservationsAreLive(
                        session,
                        workOrder,
                        workOrder.Lines.ToArray()) ||
                    CountCanonicalPcieGpuPowerCableProjections(stableItemId) != 1 ||
                    physicalCable.GetInstanceID() != physicalIdentity ||
                    physicalCable.ItemIdValue != stableItemId ||
                    !physicalCable.IsCarried ||
                    !pcieGpuPowerCableBuildKit.ProgressText.text.Contains(
                        "PCIe GPU MONTAJDA"))
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
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
                    eps12vPowerCableBinding.ValidateProjectionInvariant(),
                    pcieGpuPowerCableBinding.ValidateProjectionInvariant()
                };
                if (Array.Exists(
                        projectionInvariants,
                        invariant => invariant.IsFailure) ||
                    session.ValidateInvariants().IsFailure)
                {
                    LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(
                        "smoke.final-invariant-mismatch");
                    yield break;
                }

                if (!_suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker)
                {
                    Debug.Log(PcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker);
                }

                yield return new WaitForEndOfFrame();
                if (!Application.isEditor &&
                    !_suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker)
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

        private void LogPcieGpuPowerCableAssemblyHandoffSmokeFailure(string code)
        {
            if (_suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker)
            {
                _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_PCIE_GPU_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
