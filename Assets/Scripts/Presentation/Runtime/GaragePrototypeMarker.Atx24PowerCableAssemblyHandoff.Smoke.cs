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
        public const string Atx24PowerCableAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_ATX24_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 assembly-chain=7/7 " +
            "pickup=exact custody=build-kit-to-hands-to-route-to-hands " +
            "reservation=alive physical-identity=stable input=keyboard+mouse " +
            "generic-drop=blocked route=ok psu-unretain=blocked unroute=ok " +
            "history=10/10-preserved cables=2/2-untouched " +
            "replay=immediate+delayed receipts=ok revisions=ok " +
            "electrical-readiness=blocked no-duplicate-loss=ok invariants=ok";

        private bool _suppressAtx24PowerCableAssemblyHandoffSmokeSuccessMarker;
        private string _nestedAtx24PowerCableAssemblyHandoffSmokeFailureCode;

        public bool HasAtx24PowerCableAssemblyHandoffR52Runtime =>
            HasPowerSupplyAssemblyHandoffR51Runtime &&
            HasAtx24PowerCableR30Runtime &&
            HasAtx24PowerCableBuildKitR42Runtime &&
            atx24PowerCableBinding != null &&
            atx24PowerCableBinding.BuildKit == atx24PowerCableBuildKit &&
            atx24PowerCableBinding.Route == atx24PowerCableRoute &&
            atx24PowerCableBinding.PhysicalItem == atx24PowerCable &&
            atx24PowerCableBinding.Geometry == atx24PowerCableGeometry &&
            processorCooler != null &&
            graphicsCard != null &&
            atx24PowerCableRoute.MatchesInstalledAssemblyHostRoots(
                processorCooler.transform,
                graphicsCard.transform,
                ResolveAtx24ChassisCablePassThroughRoot()) &&
            playerCarry != null &&
            playerCarry.MatchesAtx24PowerCableConfiguration(
                atx24PowerCableRoute,
                atx24PowerCableBinding) &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypeAtx24PowerCableAssemblyHandoffOperationId
                .Value !=
                stockFlow.Session.PrototypeAtx24PowerCableBuildKitOperationId.Value &&
            stockFlow.Session.PrototypeAtx24PowerCableAssemblyHandoffOperationId
                .Value !=
                stockFlow.Session.PrototypePowerSupplyAssemblyHandoffOperationId.Value &&
            stockFlow.Session.Atx24PowerCableRouteContainerId !=
                stockFlow.Session.Atx24PowerCableBuildKitContainerId &&
            FindObjectsByType<Atx24PowerCableAssemblyItemBinding>(
                FindObjectsSortMode.None).Length == 1;

        private IEnumerator RunAtx24PowerCableAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedPowerSupplyAssemblyHandoffSmokeFailureCode = null;
            _suppressPowerSupplyAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunPowerSupplyAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressPowerSupplyAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedPowerSupplyAssemblyHandoffSmokeFailureCode;
            _nestedPowerSupplyAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                    $"smoke.power-supply-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection physicalCable =
                atx24PowerCableBinding != null
                    ? atx24PowerCableBinding.PhysicalItem
                    : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                physicalCable == null ||
                atx24PowerCableBuildKit == null ||
                atx24PowerCableRoute == null ||
                atx24PowerCableGeometry == null ||
                !HasAtx24PowerCableAssemblyHandoffR52Runtime ||
                !atx24PowerCableBuildKit.IsStaged ||
                atx24PowerCableBuildKit.IsReleasedForAssembly ||
                !atx24PowerCableBinding.IsAuthorityInBuildKit ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.CustomPcBuildKit.AssemblyHandoffCount != 7 ||
                !Atx24PowerCableAssemblyHandoffPrerequisitesAreRetained(session) ||
                session.AssemblyBuild.Atx24PowerCableState !=
                    Atx24PowerCableState.Loose ||
                session.AssemblyBuild.Atx24PowerCableRevision != 0 ||
                session.AssemblyBuild.Atx24PowerCableReceiptCount != 0)
            {
                LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot atx24Line = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.PowerCable &&
                    line.PowerCableType ==
                        PowerCableType.ModularAtx24SplitPsuToMotherboard);
            if (atx24Line == null ||
                atx24Line.ItemId != session.Atx24PowerCableItemId ||
                atx24Line.ProductId != session.Atx24PowerCableProductId ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                historicalReceipts.Length != 10 ||
                !TryCaptureAtx24AssemblyHandoffUntouchedCableContainers(
                    session,
                    workOrder,
                    atx24Line,
                    out Dictionary<StableId<ItemInstanceIdScope>,
                        StableId<ContainerIdScope>> untouchedCableContainers) ||
                !GraphicsCardAssemblyHandoffReservationsAreLive(
                    session,
                    workOrder,
                    workOrder.Lines.ToArray()) ||
                !session.TryGetEps12vPowerCableItem(
                    out InventoryItemRecord eps12vItem) ||
                !session.TryGetPcieGpuPowerCableItem(
                    out InventoryItemRecord pcieItem))
            {
                LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                    "smoke.reservation-history-or-cable-mismatch");
                yield break;
            }

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
            long settlementsRevision = session.CheckoutSettlements.Revision;
            long visitsRevision = session.CustomerVisits.Revision;
            long consultationsRevision =
                session.CustomerConsultations.Revision;
            long offerActionsRevision = session.CustomerOfferActions.Revision;

            StableId<ItemInstanceIdScope> eps12vItemId = eps12vItem.Id;
            StableId<ProductDefinitionIdScope> eps12vProductId =
                eps12vItem.ProductId;
            StableId<ContainerIdScope> eps12vContainerId =
                eps12vItem.ContainerId;
            InventorySerializedItemStateFlags eps12vFlags =
                eps12vItem.StateFlags;
            Eps12vPowerCableState eps12vState =
                session.AssemblyBuild.Eps12vPowerCableState;
            long eps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int eps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;

            StableId<ItemInstanceIdScope> pcieItemId = pcieItem.Id;
            StableId<ProductDefinitionIdScope> pcieProductId =
                pcieItem.ProductId;
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

                atx24PowerCableBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(
                    physicalCable,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCable ||
                    !playerCarry.PromptText.Contains("10/10") ||
                    !playerCarry.PromptText.Contains(
                        "ATX24'Ü KABLO MONTAJINA AL"))
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.atx24-focus-or-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitAssemblyHandoffReceipt handoff = null;
                bool pickedUp =
                    playerCarry.HeldItem == physicalCable &&
                    atx24PowerCableBinding.IsAuthorityInHands &&
                    atx24PowerCableBuildKit.IsReleasedForAssembly &&
                    atx24PowerCableBuildKit.IsStaged &&
                    atx24PowerCableBuildKit.StagedComponentCount == 10 &&
                    atx24PowerCableBuildKit.ProgressText.text.Contains(
                        "ATX24 MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                        out handoff) &&
                    handoff.ComponentKind == PcComponentKind.PowerCable &&
                    handoff.Line.PowerCableType ==
                        PowerCableType.ModularAtx24SplitPsuToMotherboard &&
                    ReferenceEquals(handoff.Line, atx24Line) &&
                    ReferenceEquals(handoff.StagingReceipt, historicalReceipts[7]) &&
                    handoff.WorkbenchContainerId ==
                        session.Atx24PowerCableRouteContainerId &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    session.Inventory.Revision == inventoryRevision + 1 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!pickedUp)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.atx24-build-kit-pickup-mismatch");
                    yield break;
                }

                long replayInventoryRevision = session.Inventory.Revision;
                long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
                long replayAssemblyRevision = session.AssemblyBuild.Revision;
                int replayAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> replay =
                    session.PickupStagedAtx24PowerCableForAssembly();
                if (replay.IsFailure ||
                    !ReferenceEquals(replay.Value, handoff) ||
                    session.Inventory.Revision != replayInventoryRevision ||
                    session.CustomPcBuildKit.Revision != replayBuildKitRevision ||
                    session.AssemblyBuild.Revision != replayAssemblyRevision ||
                    session.AssemblyBuild.ReceiptCount !=
                        replayAssemblyReceiptCount)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.immediate-replay-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool worldDropBlocked =
                    playerCarry.HeldItem == physicalCable &&
                    atx24PowerCableBinding.IsAuthorityInHands &&
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
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.reserved-world-drop-not-blocked");
                    yield break;
                }

                MovePlayerToAtx24AssemblyHandoffRoute();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool routeReady =
                    playerCarry.IsAtx24PowerCableRouteMode &&
                    !playerCarry.IsAtx24PowerCableBuildKitMode &&
                    playerCarry.CurrentAtx24PowerCableRouteStatus ==
                        Atx24PowerCableRouteStatus.ValidRoute &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!routeReady)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        string.IsNullOrEmpty(playerCarry.LastFailureCode)
                            ? "smoke.atx24-route-preflight-mismatch"
                            : playerCarry.LastFailureCode);
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                StableId<AssemblyOperationIdScope> routeOperationId =
                    Atx24PowerCablePrototypeOperationId("route", 1);
                Atx24PowerCableOperationReceipt routeReceipt = null;
                InventoryItemRecord routedCable = null;
                bool routed =
                    playerCarry.HeldItem == null &&
                    atx24PowerCableBinding.IsRouted &&
                    atx24PowerCableGeometry.IsRouted &&
                    session.AssemblyBuild.Atx24PowerCableState ==
                        Atx24PowerCableState.Routed &&
                    session.AssemblyBuild.TryGetAtx24PowerCableReceipt(
                        routeOperationId,
                        out routeReceipt) &&
                    session.TryGetAtx24PowerCableItem(
                        out routedCable) &&
                    routedCable.Id == atx24Line.ItemId &&
                    routedCable.ProductId == atx24Line.ProductId &&
                    routedCable.ContainerId ==
                        session.Atx24PowerCableRouteContainerId &&
                    session.Inventory.Revision == inventoryRevision + 2 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    session.AssemblyBuild.Atx24PowerCableRevision == 1 &&
                    session.AssemblyBuild.Atx24PowerCableReceiptCount == 1 &&
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().Error ==
                        AssemblyFailures.BuildIncomplete &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    physicalCable.Ownership == PhysicalItemOwnership.World &&
                    physicalCable.IsStablePlacement;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!routed)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.atx24-route-mismatch");
                    yield break;
                }

                long routedReplayInventoryRevision = session.Inventory.Revision;
                long routedReplayBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                long routedReplayAssemblyRevision =
                    session.AssemblyBuild.Revision;
                int routedReplayAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
                    routedHandoffReplay =
                        session.PickupStagedAtx24PowerCableForAssembly();
                if (routedHandoffReplay.IsFailure ||
                    !ReferenceEquals(routedHandoffReplay.Value, handoff) ||
                    session.Inventory.Revision !=
                        routedReplayInventoryRevision ||
                    session.CustomPcBuildKit.Revision !=
                        routedReplayBuildKitRevision ||
                    session.AssemblyBuild.Revision !=
                        routedReplayAssemblyRevision ||
                    session.AssemblyBuild.ReceiptCount !=
                        routedReplayAssemblyReceiptCount)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.delayed-handoff-replay-mismatch");
                    yield break;
                }

                AssemblyBuildSnapshot routedSnapshot =
                    session.AssemblyBuild.GetSnapshot();
                long blockedInventoryRevision = session.Inventory.Revision;
                long blockedAssemblyRevision = session.AssemblyBuild.Revision;
                int blockedAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                OperationResult<AssemblyOperationReceipt> blockedPsuUnretain =
                    session.UnretainPowerSupply(
                        StableId<AssemblyOperationIdScope>.Parse(
                            "assembly.operation.runtime-smoke.issue105-" +
                            "blocked-psu-unretain"),
                        routedSnapshot.PowerSupplySeatedByOperationId,
                        routedSnapshot.PowerSupplyRetainedByOperationId,
                        routedSnapshot.Revision);
                if (blockedPsuUnretain.Error !=
                        AssemblyFailures.PowerCableDependentComponentLocked ||
                    session.Inventory.Revision != blockedInventoryRevision ||
                    session.AssemblyBuild.Revision != blockedAssemblyRevision ||
                    session.AssemblyBuild.ReceiptCount !=
                        blockedAssemblyReceiptCount)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.routed-psu-unretain-not-blocked");
                    yield break;
                }

                MovePlayerToAtx24AssemblyHandoffRoute();
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCable)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.routed-atx24-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                StableId<AssemblyOperationIdScope> unrouteOperationId =
                    Atx24PowerCablePrototypeOperationId("unroute", 2);
                Atx24PowerCableOperationReceipt unrouteReceipt = null;
                bool unrouted =
                    playerCarry.HeldItem == physicalCable &&
                    !atx24PowerCableBinding.IsRouted &&
                    !atx24PowerCableGeometry.IsRouted &&
                    atx24PowerCableBinding.IsAuthorityInHands &&
                    session.AssemblyBuild.Atx24PowerCableState ==
                        Atx24PowerCableState.Loose &&
                    session.AssemblyBuild.TryGetAtx24PowerCableReceipt(
                        unrouteOperationId,
                        out unrouteReceipt) &&
                    unrouteReceipt.SourceRouteOperationId == routeOperationId &&
                    session.TryGetAtx24PowerCableItem(
                        out InventoryItemRecord unroutedCable) &&
                    unroutedCable.Id == routedCable.Id &&
                    unroutedCable.ProductId == routedCable.ProductId &&
                    unroutedCable.ContainerId == session.HandsContainerId &&
                    session.Inventory.Revision == inventoryRevision + 3 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 1 &&
                    session.AssemblyBuild.Revision == assemblyRevision &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                    session.AssemblyBuild.Atx24PowerCableRevision == 2 &&
                    session.AssemblyBuild.Atx24PowerCableReceiptCount == 2 &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!unrouted)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.atx24-unroute-mismatch");
                    yield break;
                }

                long delayedReplayInventoryRevision = session.Inventory.Revision;
                long delayedReplayAssemblyRevision =
                    session.AssemblyBuild.Revision;
                int delayedReplayAssemblyReceiptCount =
                    session.AssemblyBuild.ReceiptCount;
                OperationResult<Atx24PowerCableOperationReceipt> routeReplay =
                    session.RouteAtx24PowerCable(
                        routeOperationId,
                        PowerCableKeyOrientation.Keyed,
                        routeReceipt.ExpectedCableRevision);
                OperationResult<Atx24PowerCableOperationReceipt> unrouteReplay =
                    session.UnrouteAtx24PowerCable(
                        unrouteOperationId,
                        routeOperationId,
                        unrouteReceipt.ExpectedCableRevision);
                bool delayedReplay =
                    routeReplay.IsSuccess &&
                    unrouteReplay.IsSuccess &&
                    ReferenceEquals(routeReplay.Value, routeReceipt) &&
                    ReferenceEquals(unrouteReplay.Value, unrouteReceipt) &&
                    session.Inventory.Revision ==
                        delayedReplayInventoryRevision &&
                    session.AssemblyBuild.Revision ==
                        delayedReplayAssemblyRevision &&
                    session.AssemblyBuild.ReceiptCount ==
                        delayedReplayAssemblyReceiptCount &&
                    session.AssemblyBuild
                        .ValidateAtx24PowerCableReceiptHistory().IsSuccess;
                if (!delayedReplay)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.delayed-replay-mismatch");
                    yield break;
                }

                bool untouchedCables =
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

                bool isolatedAuthorities =
                    session.Orders.Revision == ordersRevision &&
                    session.RetailOffers.Revision == offersRevision &&
                    session.RetailBaskets.Revision == basketsRevision &&
                    session.RetailCheckouts.Revision == checkoutsRevision &&
                    session.CheckoutSettlements.Revision ==
                        settlementsRevision &&
                    session.CustomerVisits.Revision == visitsRevision &&
                    session.CustomerConsultations.Revision ==
                        consultationsRevision &&
                    session.CustomerOfferActions.Revision ==
                        offerActionsRevision;

                if (!untouchedCables)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.untouched-cables-mismatch");
                    yield break;
                }

                if (!isolatedAuthorities)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.isolated-authorities-mismatch");
                    yield break;
                }

                if (!Atx24PowerCableAssemblyHandoffPrerequisitesAreRetained(session))
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.retained-prerequisites-mismatch");
                    yield break;
                }

                if (session.CustomPcBuildKit.StagedComponentCount != 10 ||
                    session.CustomPcBuildKit.AssemblyHandoffCount != 8 ||
                    session.AssemblyBuild.Atx24PowerCableState !=
                        Atx24PowerCableState.Loose ||
                    session.AssemblyBuild.PowerSupplyBayState !=
                        PowerSupplyBayState.PowerSupplyRetained ||
                    !powerSupplyBinding.IsRetained)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.final-authority-state-mismatch");
                    yield break;
                }

                if (!GraphicsCardAssemblyHandoffReservationsAreLive(
                        session,
                        workOrder,
                        workOrder.Lines.ToArray()))
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.reservations-not-live");
                    yield break;
                }

                if (!MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        untouchedCableContainers))
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.staging-history-mismatch");
                    yield break;
                }

                if (CountCanonicalAtx24PowerCableProjections(stableItemId) != 1 ||
                    physicalCable.GetInstanceID() != physicalIdentity ||
                    physicalCable.ItemIdValue != stableItemId ||
                    !physicalCable.IsCarried)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.final-physical-identity-mismatch");
                    yield break;
                }

                if (!atx24PowerCableBuildKit.ProgressText.text.Contains(
                        "ATX24 MONTAJDA"))
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.final-presentation-mismatch");
                    yield break;
                }

                if (session.AssemblyBuild.EvaluateBenchmarkReadiness().Error !=
                    AssemblyFailures.PowerCableMissing)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        "smoke.electrical-readiness-mismatch");
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
                    atx24PowerCableBinding.ValidateProjectionInvariant()
                };
                string[] projectionNames =
                {
                    "motherboard",
                    "processor",
                    "memory-module",
                    "storage",
                    "processor-cooler",
                    "graphics-card",
                    "power-supply",
                    "atx24-power-cable"
                };
                for (int index = 0; index < projectionInvariants.Length; index++)
                {
                    if (projectionInvariants[index].IsSuccess)
                    {
                        continue;
                    }

                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        $"smoke.{projectionNames[index]}-projection-" +
                        projectionInvariants[index].Error.Code);
                    yield break;
                }

                OperationResult sessionInvariant = session.ValidateInvariants();
                if (sessionInvariant.IsFailure)
                {
                    LogAtx24PowerCableAssemblyHandoffSmokeFailure(
                        $"smoke.session-invariant-{sessionInvariant.Error.Code}");
                    yield break;
                }

                if (!_suppressAtx24PowerCableAssemblyHandoffSmokeSuccessMarker)
                {
                    Debug.Log(
                        Atx24PowerCableAssemblyHandoffSmokeSuccessMarker);
                }

                yield return new WaitForEndOfFrame();
                if (!Application.isEditor &&
                    !_suppressAtx24PowerCableAssemblyHandoffSmokeSuccessMarker)
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

        private static bool Atx24PowerCableAssemblyHandoffPrerequisitesAreRetained(
            GarageStockFlowSession session)
        {
            return session != null &&
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
                   session.AssemblyBuild.PowerSupplyBayState ==
                       PowerSupplyBayState.PowerSupplyRetained;
        }

        private static bool
            TryCaptureAtx24AssemblyHandoffUntouchedCableContainers(
                GarageStockFlowSession session,
                CustomPcBuildOrderRecord workOrder,
                CustomPcBuildOrderLineSnapshot atx24Line,
                out Dictionary<StableId<ItemInstanceIdScope>,
                    StableId<ContainerIdScope>> containers)
        {
            containers = new Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>>();
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind != PcComponentKind.PowerCable ||
                    ReferenceEquals(line, atx24Line))
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

            return containers.Count == 2;
        }

        private void MovePlayerToAtx24AssemblyHandoffRoute()
        {
            Vector3 target =
                atx24PowerCableRoute.FocusCollider.bounds.center;
            SetMotherboardBuildKitSmokePlayerLook(
                new Vector3(-0.72f, 0.05f, 3.25f),
                target);
        }

        private Transform ResolveAtx24ChassisCablePassThroughRoot()
        {
            if (graphicsCardSlot?.ChassisClearanceBlockers == null)
            {
                return null;
            }

            foreach (Collider blocker in graphicsCardSlot.ChassisClearanceBlockers)
            {
                if (blocker != null && blocker.name == "ChassisRightRail")
                {
                    return blocker.transform;
                }
            }

            return null;
        }

        private void LogAtx24PowerCableAssemblyHandoffSmokeFailure(string code)
        {
            if (_suppressAtx24PowerCableAssemblyHandoffSmokeSuccessMarker)
            {
                _nestedAtx24PowerCableAssemblyHandoffSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_ATX24_POWER_CABLE_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
