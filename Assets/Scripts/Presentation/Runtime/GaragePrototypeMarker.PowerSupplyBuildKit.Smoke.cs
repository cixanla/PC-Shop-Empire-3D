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
        public const string PowerSupplyBuildKitSmokeSuccessMarker =
            "GARAGE_POWER_SUPPLY_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok " +
            "prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card-staged " +
            "power-supply-pickup=exact physical-identity=stable " +
            "carry=ok prerequisite-positioning=teleport-assisted " +
            "post-prerequisite-input=keyboard+mouse " +
            "post-prerequisite-return=authored-spawn " +
            "post-prerequisite-route=authored-spawn>power-supply>power-supply-build-kit " +
            "movement=character-controller look=mouse-delta " +
            "route-horizontal-step-envelope=bounded player-parent=stable " +
            "post-prerequisite-route-no-transform-snap=ok route-collision=ok " +
            "custody-guards=ok rotation=180 " +
            "placement=ok progress=7/10 reservation=alive " +
            "custody=power-supply-build-kit receipts=ok revisions=ok " +
            "assembly=untouched power-supply-bay=untouched " +
            "atx24-route=untouched eps12v-route=untouched pcie-route=untouched " +
            "no-duplicate-loss=ok replay=ok invariants=ok";

        private bool _suppressPowerSupplyBuildKitSmokeSuccessMarker;
        private string _nestedPowerSupplyBuildKitSmokeFailureCode;

        private IEnumerator RunPowerSupplyBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            if (playerMotor == null)
            {
                LogPowerSupplyBuildKitSmokeFailure(
                    "smoke.authored-player-missing");
                yield break;
            }

            Transform authoredPlayerParent = playerMotor.transform.parent;
            Vector3 authoredPlayerSpawn = playerMotor.transform.position;
            if (Vector3.ProjectOnPlane(
                    authoredPlayerSpawn - new Vector3(0f, 0.05f, -2.5f),
                    Vector3.up).magnitude > 0.10f)
            {
                LogPowerSupplyBuildKitSmokeFailure(
                    "smoke.authored-player-spawn-mismatch");
                yield break;
            }

            _nestedGraphicsCardBuildKitSmokeFailureCode = null;
            _suppressGraphicsCardBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunGraphicsCardBuildKitSmoke();
            }
            finally
            {
                _suppressGraphicsCardBuildKitSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedGraphicsCardBuildKitSmokeFailureCode;
            _nestedGraphicsCardBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    System.StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogPowerSupplyBuildKitSmokeFailure(
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
                powerSupplyBinding == null ||
                physicalPowerSupply == null ||
                powerSupplyBuildKit == null ||
                powerSupplyBay == null ||
                !HasPowerSupplyBuildKitR41Runtime ||
                !motherboardBuildKit.IsStaged ||
                !processorBuildKit.IsStaged ||
                !memoryModuleBuildKit.IsStaged ||
                !storageBuildKit.IsStaged ||
                !processorCoolerBuildKit.IsStaged ||
                !graphicsCardBuildKit.IsStaged ||
                graphicsCardBuildKit.StagedComponentCount != 6 ||
                !powerSupplyBuildKit.HasMotherboardPrerequisite ||
                !powerSupplyBuildKit.HasProcessorPrerequisite ||
                !powerSupplyBuildKit.HasMemoryModulePrerequisite ||
                !powerSupplyBuildKit.HasStoragePrerequisite ||
                !powerSupplyBuildKit.HasProcessorCoolerPrerequisite ||
                !powerSupplyBuildKit.HasGraphicsCardPrerequisite ||
                powerSupplyBuildKit.StagedComponentCount != 6)
            {
                LogPowerSupplyBuildKitSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogPowerSupplyBuildKitSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot powerSupplyLine =
                workOrder.Lines.SingleOrDefault(
                    line => line.ComponentKind == PcComponentKind.PowerSupply);
            if (powerSupplyLine == null ||
                powerSupplyLine.ItemId != session.PowerSupplyItemId ||
                !session.Inventory.TryGetReservation(
                    powerSupplyLine.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.ItemId != powerSupplyLine.ItemId ||
                reservation.ClaimId != workOrder.InventoryClaimId)
            {
                LogPowerSupplyBuildKitSmokeFailure(
                    "smoke.reservation-mismatch");
                yield break;
            }

            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Atx24PowerCableState atx24State =
                session.AssemblyBuild.Atx24PowerCableState;
            long atx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int atx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            Eps12vPowerCableState eps12vState =
                session.AssemblyBuild.Eps12vPowerCableState;
            long eps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int eps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            int physicalIdentity = physicalPowerSupply.GetInstanceID();
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
                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.Update();

                if (!TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(2.0f, 0.05f, 2.15f),
                        90,
                        authoredPlayerParent,
                        "return-from-graphics-kit",
                        out string routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(2.0f, 0.05f, -0.25f),
                        90,
                        authoredPlayerParent,
                        "return-through-east-aisle",
                        out routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(2.0f, 0.05f, -2.5f),
                        90,
                        authoredPlayerParent,
                        "return-to-south-aisle",
                        out routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        authoredPlayerSpawn,
                        65,
                        authoredPlayerParent,
                        "return-to-authored-spawn",
                        out routeFailure) ||
                    Vector3.ProjectOnPlane(
                        playerMotor.transform.position - authoredPlayerSpawn,
                        Vector3.up).magnitude > 0.18f ||
                    !TryRunPowerSupplyBuildKitSmokeCardinalCalibration(
                        smokeKeyboard,
                        smokeMouse,
                        authoredPlayerParent,
                        out routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(2.0f, 0.05f, -2.5f),
                        65,
                        authoredPlayerParent,
                        "spawn-to-east-aisle",
                        out routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(2.0f, 0.05f, -0.25f),
                        90,
                        authoredPlayerParent,
                        "east-aisle-northbound",
                        out routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(1.15f, 0.05f, -0.25f),
                        45,
                        authoredPlayerParent,
                        "east-aisle-crossing",
                        out routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(1.15f, 0.05f, 2.15f),
                        90,
                        authoredPlayerParent,
                        "workbench-front-approach",
                        out routeFailure) ||
                    !TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        new Vector3(-0.17f, 0.05f, 2.15f),
                        55,
                        authoredPlayerParent,
                        "power-supply-front-crossing",
                        out routeFailure))
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        $"smoke.power-supply-human-route-{routeFailure ?? "spawn-return-mismatch"}");
                    yield break;
                }

                Vector3 powerSupplyFocus =
                    physicalPowerSupply.InteractionCenter;
                Vector3 powerSupplyApproach =
                    powerSupplyFocus - (Vector3.forward * 1.60f);
                powerSupplyApproach.y = playerMotor.transform.position.y;
                if (!TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        powerSupplyApproach,
                        90,
                        authoredPlayerParent,
                        "power-supply-pickup-approach",
                        out string pickupRouteFailure))
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        $"smoke.power-supply-human-pickup-route-{pickupRouteFailure}");
                    yield break;
                }
                if (!TryAimPowerSupplyBuildKitSmokeAt(
                        smokeKeyboard,
                        smokeMouse,
                        powerSupplyFocus,
                        authoredPlayerParent,
                        "power-supply-pickup-look",
                        out pickupRouteFailure))
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        $"smoke.power-supply-human-pickup-route-{pickupRouteFailure}");
                    yield break;
                }
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalPowerSupply)
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        "smoke.power-supply-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalPowerSupply &&
                    physicalPowerSupply.GetInstanceID() == physicalIdentity &&
                    powerSupplyBinding.IsAuthorityInHands &&
                    powerSupplyBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypePowerSupplyBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.PowerSupplyInHands &&
                    ReferenceEquals(pickupReceipt.Line, powerSupplyLine) &&
                    pickupReceipt.Line.LineId == powerSupplyLine.LineId &&
                    pickupReceipt.Line.ProductId == powerSupplyLine.ProductId &&
                    pickupReceipt.Line.ItemId == powerSupplyLine.ItemId &&
                    pickupReceipt.Line.ReservationId ==
                        powerSupplyLine.ReservationId &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    PowerSupplyBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        "smoke.power-supply-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands =
                    session.CustomPcBuildKit.Revision;
                bool custodyGuard =
                    playerCarry.TryDrop().IsFailure &&
                    playerCarry.HeldItem == physicalPowerSupply &&
                    physicalPowerSupply.IsCarried &&
                    powerSupplyBinding.IsAuthorityInHands &&
                    powerSupplyBuildKit.StagedComponentCount == 6 &&
                    session.Inventory.Revision == inventoryRevisionInHands &&
                    session.CustomPcBuildKit.Revision == buildKitRevisionInHands &&
                    PowerSupplyBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount);
                if (!custodyGuard)
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        "smoke.power-supply-custody-guard-mismatch");
                    yield break;
                }

                Collider support = powerSupplyBuildKit.SupportCollider;
                Vector3 buildKitFocus = new Vector3(
                    support.bounds.center.x,
                    support.bounds.max.y,
                    support.bounds.center.z);
                Vector3 buildKitApproach =
                    buildKitFocus + (Vector3.back * 0.95f);
                buildKitApproach.y = playerMotor.transform.position.y;
                if (!TryDrivePowerSupplyBuildKitSmokePoint(
                        smokeKeyboard,
                        smokeMouse,
                        buildKitApproach,
                        125,
                        authoredPlayerParent,
                        "power-supply-build-kit-approach",
                        out string buildKitRouteFailure))
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        $"smoke.power-supply-human-build-kit-route-{buildKitRouteFailure}");
                    yield break;
                }
                if (!TryAimPowerSupplyBuildKitSmokeAt(
                        smokeKeyboard,
                        smokeMouse,
                        buildKitFocus,
                        authoredPlayerParent,
                        "power-supply-build-kit-look",
                        out buildKitRouteFailure))
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        $"smoke.power-supply-human-build-kit-route-{buildKitRouteFailure}");
                    yield break;
                }
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool modeValid =
                    playerCarry.IsPowerSupplyBuildKitMode &&
                    !playerCarry.IsPowerSupplySeatMode &&
                    playerCarry.CurrentPowerSupplyBuildKitStatus ==
                        PowerSupplyBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("6/10 → 7/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!modeValid)
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        "smoke.power-supply-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentPowerSupplyBuildKitStatus ==
                        PowerSupplyBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    powerSupplyBuildKit.IsStaged &&
                    powerSupplyBuildKit.StagedComponentCount == 7 &&
                    powerSupplyBuildKit.ProgressText.text.Contains("7/10") &&
                    powerSupplyBuildKit.ProgressText.text.Contains("PSU HAZIR") &&
                    powerSupplyBinding.IsAuthorityInBuildKit &&
                    physicalPowerSupply.GetInstanceID() == physicalIdentity &&
                    physicalPowerSupply.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalPowerSupply.IsStablePlacement &&
                    powerSupplyBuildKit.MatchesCommittedPlacement(
                        physicalPowerSupply) &&
                    Quaternion.Angle(
                        physicalPowerSupply.transform.rotation,
                        powerSupplyBuildKit.ResolveSnapPose(1).rotation) <=
                            0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypePowerSupplyBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.PowerSupplyStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, powerSupplyLine) &&
                    session.TryGetPowerSupplyItem(
                        out InventoryItemRecord stagedPowerSupply) &&
                    stagedPowerSupply.ContainerId ==
                        session.PowerSupplyBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        powerSupplyLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedPowerSupply.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision == inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.PowerSupplyBuildKitContainerId).Value == 1 &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    CountCanonicalPowerSupplyProjections(
                        session.PowerSupplyItemId.Value) == 1 &&
                    PowerSupplyBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount) &&
                    powerSupplyBinding.ValidateProjectionInvariant().IsSuccess &&
                    graphicsCardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorCoolerBinding.ValidateProjectionInvariant().IsSuccess &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!exactPlacement)
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        "smoke.power-supply-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalPowerSupply(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    powerSupplyBuildKit.StagedComponentCount == 7 &&
                    physicalPowerSupply.GetInstanceID() == physicalIdentity &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.PowerSupplyBuildKitContainerId).Value == 1 &&
                    PowerSupplyBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount) &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogPowerSupplyBuildKitSmokeFailure(
                        "smoke.replay-mismatch");
                    yield break;
                }

                if (!_suppressPowerSupplyBuildKitSmokeSuccessMarker)
                {
                    Debug.Log(PowerSupplyBuildKitSmokeSuccessMarker);
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

        private static bool PowerSupplyBuildKitSmokeAssemblyUnchanged(
            GarageStockFlowSession session,
            AssemblyBuildSnapshot expected,
            int expectedAssemblyReceiptCount,
            Atx24PowerCableState expectedAtx24State,
            long expectedAtx24Revision,
            int expectedAtx24ReceiptCount,
            Eps12vPowerCableState expectedEps12vState,
            long expectedEps12vRevision,
            int expectedEps12vReceiptCount,
            PcieGpuPowerCableState expectedPcieState,
            long expectedPcieRevision,
            int expectedPcieReceiptCount)
        {
            AssemblyBuildSnapshot actual = session.AssemblyBuild.GetSnapshot();
            return actual.Revision == expected.Revision &&
                   session.AssemblyBuild.ReceiptCount ==
                       expectedAssemblyReceiptCount &&
                   actual.PowerSupplyBayState ==
                       expected.PowerSupplyBayState &&
                   actual.PowerSupplyItemId == expected.PowerSupplyItemId &&
                   actual.PowerSupplyProductId ==
                       expected.PowerSupplyProductId &&
                   actual.PowerSupplyMountOrientation ==
                       expected.PowerSupplyMountOrientation &&
                   actual.PowerSupplySeatedByOperationId ==
                       expected.PowerSupplySeatedByOperationId &&
                   actual.PowerSupplyRetainedByOperationId ==
                       expected.PowerSupplyRetainedByOperationId &&
                   session.AssemblyBuild.Atx24PowerCableState ==
                       expectedAtx24State &&
                   session.AssemblyBuild.Atx24PowerCableRevision ==
                       expectedAtx24Revision &&
                   session.AssemblyBuild.Atx24PowerCableReceiptCount ==
                       expectedAtx24ReceiptCount &&
                   session.AssemblyBuild.Eps12vPowerCableState ==
                       expectedEps12vState &&
                   session.AssemblyBuild.Eps12vPowerCableRevision ==
                       expectedEps12vRevision &&
                   session.AssemblyBuild.Eps12vPowerCableReceiptCount ==
                       expectedEps12vReceiptCount &&
                   session.AssemblyBuild.PcieGpuPowerCableState ==
                       expectedPcieState &&
                   session.AssemblyBuild.PcieGpuPowerCableRevision ==
                       expectedPcieRevision &&
                   session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                       expectedPcieReceiptCount;
        }

        private bool TryRunPowerSupplyBuildKitSmokeCardinalCalibration(
            Keyboard keyboard,
            Mouse mouse,
            Transform expectedParent,
            out string failure)
        {
            const int FramesPerDirection = 3;
            Transform player = playerMotor.transform;
            CharacterController controller =
                playerMotor.GetComponent<CharacterController>();
            if (controller == null || !controller.enabled)
            {
                failure = "cardinal-controller-unavailable";
                return false;
            }

            Vector3 forward = player.forward;
            Vector3 right = player.right;
            Vector3 start = player.position;
            if (!TryStepPowerSupplyBuildKitSmokeMovement(
                    keyboard,
                    mouse,
                    Vector2.up,
                    FramesPerDirection,
                    expectedParent,
                    "cardinal-w",
                    out failure) ||
                Vector3.Dot(player.position - start, forward) <= 0.05f)
            {
                ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                failure ??= "cardinal-w-mismatch";
                return false;
            }

            start = player.position;
            if (!TryStepPowerSupplyBuildKitSmokeMovement(
                    keyboard,
                    mouse,
                    Vector2.down,
                    FramesPerDirection,
                    expectedParent,
                    "cardinal-s",
                    out failure) ||
                Vector3.Dot(player.position - start, forward) >= -0.05f)
            {
                ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                failure ??= "cardinal-s-mismatch";
                return false;
            }

            start = player.position;
            if (!TryStepPowerSupplyBuildKitSmokeMovement(
                    keyboard,
                    mouse,
                    Vector2.left,
                    FramesPerDirection,
                    expectedParent,
                    "cardinal-a",
                    out failure) ||
                Vector3.Dot(player.position - start, right) >= -0.05f)
            {
                ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                failure ??= "cardinal-a-mismatch";
                return false;
            }

            start = player.position;
            if (!TryStepPowerSupplyBuildKitSmokeMovement(
                    keyboard,
                    mouse,
                    Vector2.right,
                    FramesPerDirection,
                    expectedParent,
                    "cardinal-d",
                    out failure) ||
                Vector3.Dot(player.position - start, right) <= 0.05f)
            {
                ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                failure ??= "cardinal-d-mismatch";
                return false;
            }

            ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
            failure = null;
            return true;
        }

        private bool TryStepPowerSupplyBuildKitSmokeMovement(
            Keyboard keyboard,
            Mouse mouse,
            Vector2 movement,
            int frameCount,
            Transform expectedParent,
            string routeId,
            out string failure)
        {
            for (int frame = 0; frame < frameCount; frame++)
            {
                if (!TryStepPowerSupplyBuildKitSmokeRouteFrame(
                        keyboard,
                        mouse,
                        movement,
                        Vector2.zero,
                        expectedParent,
                        routeId,
                        frame,
                        out failure))
                {
                    return false;
                }
            }

            failure = null;
            return true;
        }

        private bool TryDrivePowerSupplyBuildKitSmokePoint(
            Keyboard keyboard,
            Mouse mouse,
            Vector3 worldTarget,
            int maximumFrames,
            Transform expectedParent,
            string routeId,
            out string failure)
        {
            const float ArrivalTolerance = 0.18f;
            const float MinimumProgress = 0.003f;
            const int MaximumStagnantFrames = 30;
            Transform player = playerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            CharacterController controller =
                playerMotor.GetComponent<CharacterController>();
            if (cameraPivot == null ||
                controller == null ||
                !controller.enabled ||
                player.parent != expectedParent)
            {
                failure = $"{routeId}-route-context-mismatch";
                return false;
            }

            playerMotor.SetPaused(false);
            int stagnantFrames = 0;
            float previousDistance = PowerSupplyBuildKitSmokePlanarDistance(
                player.position,
                worldTarget);
            for (int frame = 0; frame < maximumFrames; frame++)
            {
                float distance = PowerSupplyBuildKitSmokePlanarDistance(
                    player.position,
                    worldTarget);
                if (distance <= ArrivalTolerance)
                {
                    ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                    failure = null;
                    return true;
                }

                Vector3 direction = Vector3.ProjectOnPlane(
                    worldTarget - player.position,
                    Vector3.up).normalized;
                float desiredYaw = Mathf.Atan2(
                    direction.x,
                    direction.z) * Mathf.Rad2Deg;
                float yawError = Mathf.DeltaAngle(
                    player.eulerAngles.y,
                    desiredYaw);
                float pitchError = -NormalizePowerSupplyBuildKitSmokeAngle(
                    cameraPivot.localEulerAngles.x);
                bool aligned = Mathf.Abs(yawError) <= 2f &&
                               Mathf.Abs(pitchError) <= 2f;
                if (!TryStepPowerSupplyBuildKitSmokeRouteFrame(
                        keyboard,
                        mouse,
                        aligned ? Vector2.up : Vector2.zero,
                        ResolvePowerSupplyBuildKitSmokeLook(
                            yawError,
                            pitchError),
                        expectedParent,
                        routeId,
                        frame,
                        out failure))
                {
                    ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                    return false;
                }

                float currentDistance = PowerSupplyBuildKitSmokePlanarDistance(
                    player.position,
                    worldTarget);
                stagnantFrames = aligned &&
                                 previousDistance - currentDistance <
                                 MinimumProgress
                    ? stagnantFrames + 1
                    : 0;
                if (stagnantFrames >= MaximumStagnantFrames)
                {
                    ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                    failure = $"{routeId}-blocked-{currentDistance:0.000}";
                    return false;
                }
                previousDistance = currentDistance;
            }

            ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
            failure =
                $"{routeId}-timeout-" +
                $"{PowerSupplyBuildKitSmokePlanarDistance(player.position, worldTarget):0.000}";
            return false;
        }

        private bool TryAimPowerSupplyBuildKitSmokeAt(
            Keyboard keyboard,
            Mouse mouse,
            Vector3 worldTarget,
            Transform expectedParent,
            string routeId,
            out string failure)
        {
            const int MaximumFrames = 90;
            Transform player = playerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            Camera camera = playerMotor.GetComponentInChildren<Camera>();
            CharacterController controller =
                playerMotor.GetComponent<CharacterController>();
            if (cameraPivot == null ||
                camera == null ||
                controller == null ||
                !controller.enabled ||
                player.parent != expectedParent)
            {
                failure = $"{routeId}-look-context-mismatch";
                return false;
            }

            for (int frame = 0; frame < MaximumFrames; frame++)
            {
                Vector3 direction =
                    (worldTarget - camera.transform.position).normalized;
                Vector3 planarDirection = Vector3.ProjectOnPlane(
                    direction,
                    Vector3.up);
                float desiredYaw = Mathf.Atan2(
                    planarDirection.x,
                    planarDirection.z) * Mathf.Rad2Deg;
                float desiredPitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
                float yawError = Mathf.DeltaAngle(
                    player.eulerAngles.y,
                    desiredYaw);
                float pitchError = Mathf.DeltaAngle(
                    NormalizePowerSupplyBuildKitSmokeAngle(
                        cameraPivot.localEulerAngles.x),
                    desiredPitch);
                if (Mathf.Abs(yawError) <= 0.75f &&
                    Mathf.Abs(pitchError) <= 0.75f)
                {
                    ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                    Vector3 expectedDirection =
                        (worldTarget - camera.transform.position).normalized;
                    if (Vector3.Dot(
                            camera.transform.forward,
                            expectedDirection) <= 0.999f)
                    {
                        failure = $"{routeId}-look-dot-mismatch";
                        return false;
                    }

                    failure = null;
                    return true;
                }

                if (!TryStepPowerSupplyBuildKitSmokeRouteFrame(
                        keyboard,
                        mouse,
                        Vector2.zero,
                        ResolvePowerSupplyBuildKitSmokeLook(
                            yawError,
                            pitchError),
                        expectedParent,
                        routeId,
                        frame,
                        out failure))
                {
                    ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
                    return false;
                }
            }

            ReleasePowerSupplyBuildKitSmokeRouteInput(keyboard, mouse);
            failure = $"{routeId}-look-timeout";
            return false;
        }

        private Vector2 ResolvePowerSupplyBuildKitSmokeLook(
            float yawError,
            float pitchError)
        {
            float sensitivity = playerMotor.ViewSettings.MouseSensitivity;
            float vertical = playerMotor.ViewSettings.InvertY
                ? pitchError / sensitivity
                : -pitchError / sensitivity;
            return new Vector2(
                Mathf.Clamp(yawError / sensitivity, -80f, 80f),
                Mathf.Clamp(vertical, -80f, 80f));
        }

        private bool TryStepPowerSupplyBuildKitSmokeRouteFrame(
            Keyboard keyboard,
            Mouse mouse,
            Vector2 movement,
            Vector2 look,
            Transform expectedParent,
            string routeId,
            int frame,
            out string failure)
        {
            const float SimulatedFrameDeltaTime = 1f / 60f;
            Transform player = playerMotor.transform;
            CharacterController controller =
                playerMotor.GetComponent<CharacterController>();
            if (controller == null ||
                !controller.enabled ||
                player.parent != expectedParent)
            {
                failure = $"{routeId}-controller-or-parent-mismatch";
                return false;
            }

            InputSystem.QueueStateEvent(
                keyboard,
                PowerSupplyBuildKitSmokeKeyboardStateForMove(movement));
            InputSystem.QueueStateEvent(
                mouse,
                new MouseState { delta = look });
            InputSystem.Update();

            Vector3 before = player.position;
            float maximumHorizontalStep =
                playerMotor.ResolveHorizontalSpeed(false) *
                SimulatedFrameDeltaTime + 0.02f;
            playerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Physics.SyncTransforms();

            float horizontalStep = Vector3.ProjectOnPlane(
                player.position - before,
                Vector3.up).magnitude;
            if (horizontalStep > maximumHorizontalStep ||
                !controller.enabled ||
                player.parent != expectedParent)
            {
                failure = $"{routeId}-movement-envelope-frame-{frame}";
                return false;
            }

            failure = null;
            return true;
        }

        private static KeyboardState PowerSupplyBuildKitSmokeKeyboardStateForMove(
            Vector2 movement)
        {
            if (movement.y > 0.5f)
            {
                return new KeyboardState(Key.W);
            }

            if (movement.y < -0.5f)
            {
                return new KeyboardState(Key.S);
            }

            if (movement.x < -0.5f)
            {
                return new KeyboardState(Key.A);
            }

            return movement.x > 0.5f
                ? new KeyboardState(Key.D)
                : new KeyboardState();
        }

        private static void ReleasePowerSupplyBuildKitSmokeRouteInput(
            Keyboard keyboard,
            Mouse mouse)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        private static float PowerSupplyBuildKitSmokePlanarDistance(
            Vector3 first,
            Vector3 second)
        {
            return Vector3.ProjectOnPlane(
                first - second,
                Vector3.up).magnitude;
        }

        private static float NormalizePowerSupplyBuildKitSmokeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private void LogPowerSupplyBuildKitSmokeFailure(string code)
        {
            if (_suppressPowerSupplyBuildKitSmokeSuccessMarker)
            {
                _nestedPowerSupplyBuildKitSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_POWER_SUPPLY_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
