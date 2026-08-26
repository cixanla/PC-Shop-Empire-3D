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
        public const string Eps12vPowerCableBuildKitSmokeSuccessMarker =
            "GARAGE_EPS12V_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok " +
            "prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card+power-supply+atx24-staged " +
            "eps12v-pickup=exact cable-family=modular-eps12v-8pin " +
            "physical-identity=stable carry=ok input=keyboard+mouse " +
            "prerequisite-positioning=teleport-assisted " +
            "custody-guards=ok route-consumer=blocked rotation=180 " +
            "placement=ok progress=9/10 reservation=alive " +
            "custody=eps12v-build-kit receipts=ok revisions=ok " +
            "assembly=untouched eps12v-route=untouched " +
            "atx24-route=untouched pcie-route=untouched " +
            "no-duplicate-loss=ok replay=ok invariants=ok";

        private bool _suppressEps12vPowerCableBuildKitSmokeSuccessMarker;
        private string _nestedEps12vPowerCableBuildKitSmokeFailureCode;

        private IEnumerator RunEps12vPowerCableBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedAtx24PowerCableBuildKitSmokeFailureCode = null;
            _suppressAtx24PowerCableBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunAtx24PowerCableBuildKitSmoke();
            }
            finally
            {
                _suppressAtx24PowerCableBuildKitSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedAtx24PowerCableBuildKitSmokeFailureCode;
            _nestedAtx24PowerCableBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    System.StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogEps12vPowerCableBuildKitSmokeFailure(
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
                eps12vPowerCableBinding == null ||
                physicalCable == null ||
                eps12vPowerCableBuildKit == null ||
                eps12vPowerCableRoute == null ||
                !HasEps12vPowerCableBuildKitR43Runtime ||
                !motherboardBuildKit.IsStaged ||
                !processorBuildKit.IsStaged ||
                !memoryModuleBuildKit.IsStaged ||
                !storageBuildKit.IsStaged ||
                !processorCoolerBuildKit.IsStaged ||
                !graphicsCardBuildKit.IsStaged ||
                !powerSupplyBuildKit.IsStaged ||
                !atx24PowerCableBuildKit.IsStaged ||
                !eps12vPowerCableBuildKit.HasMotherboardPrerequisite ||
                !eps12vPowerCableBuildKit.HasProcessorPrerequisite ||
                !eps12vPowerCableBuildKit.HasMemoryModulePrerequisite ||
                !eps12vPowerCableBuildKit.HasStoragePrerequisite ||
                !eps12vPowerCableBuildKit.HasProcessorCoolerPrerequisite ||
                !eps12vPowerCableBuildKit.HasGraphicsCardPrerequisite ||
                !eps12vPowerCableBuildKit.HasPowerSupplyPrerequisite ||
                !eps12vPowerCableBuildKit.HasAtx24PowerCablePrerequisite ||
                eps12vPowerCableBuildKit.StagedComponentCount != 8 ||
                eps12vPowerCableBinding.IsRouted ||
                !eps12vPowerCableBinding.IsAuthorityLooseWorld)
            {
                LogEps12vPowerCableBuildKitSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogEps12vPowerCableBuildKitSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot cableLine =
                workOrder.Lines.SingleOrDefault(
                    line => line.ComponentKind == PcComponentKind.PowerCable &&
                            line.PowerCableType ==
                                PowerCableType.ModularEps12v8PinPsuToMotherboard);
            if (cableLine == null ||
                cableLine.ItemId != session.Eps12vPowerCableItemId ||
                !session.Inventory.TryGetReservation(
                    cableLine.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.ItemId != cableLine.ItemId ||
                reservation.ClaimId != workOrder.InventoryClaimId)
            {
                LogEps12vPowerCableBuildKitSmokeFailure(
                    "smoke.reservation-mismatch");
                yield break;
            }

            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Eps12vPowerCableState eps12vState =
                session.AssemblyBuild.Eps12vPowerCableState;
            long eps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int eps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            Atx24PowerCableState atx24State =
                session.AssemblyBuild.Atx24PowerCableState;
            long atx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int atx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            int physicalIdentity = physicalCable.GetInstanceID();
            string stableItemId = physicalCable.ItemIdValue;
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

                AimMotherboardBuildKitSmokeAtItem(
                    physicalCable,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalCable)
                {
                    LogEps12vPowerCableBuildKitSmokeFailure(
                        "smoke.eps12v-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalCable &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    eps12vPowerCableBinding.IsAuthorityInHands &&
                    !eps12vPowerCableBinding.IsRouted &&
                    eps12vPowerCableBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeEps12vPowerCableBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.Eps12vPowerCableInHands &&
                    ReferenceEquals(pickupReceipt.Line, cableLine) &&
                    pickupReceipt.Line.PowerCableType ==
                        PowerCableType.ModularEps12v8PinPsuToMotherboard &&
                    pickupReceipt.Line.ItemId == cableLine.ItemId &&
                    pickupReceipt.Line.ReservationId ==
                        cableLine.ReservationId &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    Eps12vPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogEps12vPowerCableBuildKitSmokeFailure(
                        "smoke.eps12v-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands =
                    session.CustomPcBuildKit.Revision;
                OperationResult routeBypass =
                    playerCarry.TrySetEps12vPowerCableRouteMode(true);
                bool custodyGuard =
                    routeBypass.IsFailure &&
                    routeBypass.Error.Code ==
                        "custom-pc-eps12v-power-cable-build-kit.authority-blocked" &&
                    !playerCarry.IsEps12vPowerCableRouteMode &&
                    playerCarry.TryDrop().IsFailure &&
                    playerCarry.HeldItem == physicalCable &&
                    physicalCable.IsCarried &&
                    eps12vPowerCableBinding.IsAuthorityInHands &&
                    !eps12vPowerCableBinding.IsRouted &&
                    eps12vPowerCableBuildKit.StagedComponentCount == 8 &&
                    session.Inventory.Revision == inventoryRevisionInHands &&
                    session.CustomPcBuildKit.Revision == buildKitRevisionInHands &&
                    Eps12vPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount);
                if (!custodyGuard)
                {
                    LogEps12vPowerCableBuildKitSmokeFailure(
                        "smoke.eps12v-custody-guard-mismatch");
                    yield break;
                }

                MoveEps12vPowerCableBuildKitSmokePlayerToKit(
                    eps12vPowerCableBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool modeValid =
                    playerCarry.IsEps12vPowerCableBuildKitMode &&
                    !playerCarry.IsEps12vPowerCableRouteMode &&
                    playerCarry.CurrentEps12vPowerCableBuildKitStatus ==
                        Eps12vPowerCableBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("8/10 → 9/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!modeValid)
                {
                    LogEps12vPowerCableBuildKitSmokeFailure(
                        "smoke.eps12v-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentEps12vPowerCableBuildKitStatus ==
                        Eps12vPowerCableBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogEps12vPowerCableBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    eps12vPowerCableBuildKit.IsStaged &&
                    eps12vPowerCableBuildKit.StagedComponentCount == 9 &&
                    eps12vPowerCableBuildKit.ProgressText.text.Contains("9/10") &&
                    eps12vPowerCableBuildKit.ProgressText.text.Contains(
                        "EPS12V HAZIR") &&
                    eps12vPowerCableBinding.IsAuthorityInBuildKit &&
                    !eps12vPowerCableBinding.IsRouted &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    physicalCable.Ownership == PhysicalItemOwnership.World &&
                    physicalCable.IsStablePlacement &&
                    eps12vPowerCableBuildKit.MatchesCommittedPlacement(
                        physicalCable) &&
                    Quaternion.Angle(
                        physicalCable.transform.rotation,
                        eps12vPowerCableBuildKit.ResolveSnapPose(1).rotation) <=
                            0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeEps12vPowerCableBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.Eps12vPowerCableStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, cableLine) &&
                    session.TryGetEps12vPowerCableItem(
                        out InventoryItemRecord stagedCable) &&
                    stagedCable.ContainerId ==
                        session.Eps12vPowerCableBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        cableLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedCable.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision ==
                        inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.Eps12vPowerCableBuildKitContainerId).Value == 1 &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    CountCanonicalEps12vPowerCableProjections(stableItemId) == 1 &&
                    Eps12vPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount) &&
                    eps12vPowerCableBinding
                        .ValidateProjectionInvariant().IsSuccess &&
                    atx24PowerCableBinding
                        .ValidateProjectionInvariant().IsSuccess &&
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
                    LogEps12vPowerCableBuildKitSmokeFailure(
                        "smoke.eps12v-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalEps12vPowerCable(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    eps12vPowerCableBuildKit.StagedComponentCount == 9 &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.Eps12vPowerCableBuildKitContainerId).Value == 1 &&
                    Eps12vPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount) &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogEps12vPowerCableBuildKitSmokeFailure(
                        "smoke.replay-mismatch");
                    yield break;
                }

                if (!_suppressEps12vPowerCableBuildKitSmokeSuccessMarker)
                {
                    Debug.Log(Eps12vPowerCableBuildKitSmokeSuccessMarker);
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

        private static bool Eps12vPowerCableBuildKitSmokeAssemblyUnchanged(
            GarageStockFlowSession session,
            long expectedAssemblyRevision,
            int expectedAssemblyReceiptCount,
            Eps12vPowerCableState expectedEps12vState,
            long expectedEps12vRevision,
            int expectedEps12vReceiptCount,
            Atx24PowerCableState expectedAtx24State,
            long expectedAtx24Revision,
            int expectedAtx24ReceiptCount,
            PcieGpuPowerCableState expectedPcieState,
            long expectedPcieRevision,
            int expectedPcieReceiptCount)
        {
            return session.AssemblyBuild.Revision == expectedAssemblyRevision &&
                   session.AssemblyBuild.ReceiptCount ==
                       expectedAssemblyReceiptCount &&
                   session.AssemblyBuild.Eps12vPowerCableState ==
                       expectedEps12vState &&
                   session.AssemblyBuild.Eps12vPowerCableRevision ==
                       expectedEps12vRevision &&
                   session.AssemblyBuild.Eps12vPowerCableReceiptCount ==
                       expectedEps12vReceiptCount &&
                   session.AssemblyBuild.Atx24PowerCableState ==
                       expectedAtx24State &&
                   session.AssemblyBuild.Atx24PowerCableRevision ==
                       expectedAtx24Revision &&
                   session.AssemblyBuild.Atx24PowerCableReceiptCount ==
                       expectedAtx24ReceiptCount &&
                   session.AssemblyBuild.PcieGpuPowerCableState ==
                       expectedPcieState &&
                   session.AssemblyBuild.PcieGpuPowerCableRevision ==
                       expectedPcieRevision &&
                   session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                       expectedPcieReceiptCount;
        }

        private void MoveEps12vPowerCableBuildKitSmokePlayerToKit(
            Eps12vPowerCableBuildKitProjection buildKit)
        {
            Collider support = buildKit.SupportCollider;
            Vector3 target = new Vector3(
                support.bounds.center.x,
                support.bounds.max.y,
                support.bounds.center.z);
            Vector3 playerPosition = target + (Vector3.back * 0.95f);
            playerPosition.y = 0.05f;
            SetMotherboardBuildKitSmokePlayerLook(playerPosition, target);
        }

        private void LogEps12vPowerCableBuildKitSmokeFailure(string code)
        {
            if (_suppressEps12vPowerCableBuildKitSmokeSuccessMarker)
            {
                _nestedEps12vPowerCableBuildKitSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_EPS12V_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
