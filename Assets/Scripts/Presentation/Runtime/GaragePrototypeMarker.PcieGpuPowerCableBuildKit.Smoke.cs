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
        public const string PcieGpuPowerCableBuildKitSmokeSuccessMarker =
            "GARAGE_PCIE_GPU_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok " +
            "prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card+power-supply+atx24+eps12v-staged " +
            "pcie-gpu-pickup=exact cable-family=modular-pcie-8pin " +
            "physical-identity=stable carry=ok input=keyboard+mouse " +
            "prerequisite-positioning=teleport-assisted " +
            "custody-guards=ok route-consumer=blocked rotation=180 " +
            "placement=ok progress=10/10 reservation=alive " +
            "custody=pcie-gpu-build-kit receipts=ok revisions=ok " +
            "assembly=untouched pcie-gpu-route=untouched " +
            "atx24-route=untouched eps12v-route=untouched " +
            "no-duplicate-loss=ok replay=ok invariants=ok";

        private bool _suppressPcieGpuPowerCableBuildKitSmokeSuccessMarker;
        private string _nestedPcieGpuPowerCableBuildKitSmokeFailureCode;

        private IEnumerator RunPcieGpuPowerCableBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedEps12vPowerCableBuildKitSmokeFailureCode = null;
            _suppressEps12vPowerCableBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunEps12vPowerCableBuildKitSmoke();
            }
            finally
            {
                _suppressEps12vPowerCableBuildKitSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedEps12vPowerCableBuildKitSmokeFailureCode;
            _nestedEps12vPowerCableBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    System.StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogPcieGpuPowerCableBuildKitSmokeFailure(
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
                pcieGpuPowerCableBinding == null ||
                physicalCable == null ||
                pcieGpuPowerCableBuildKit == null ||
                pcieGpuPowerCableRoute == null ||
                !HasPcieGpuPowerCableBuildKitR44Runtime ||
                !motherboardBuildKit.IsStaged ||
                !processorBuildKit.IsStaged ||
                !memoryModuleBuildKit.IsStaged ||
                !storageBuildKit.IsStaged ||
                !processorCoolerBuildKit.IsStaged ||
                !graphicsCardBuildKit.IsStaged ||
                !powerSupplyBuildKit.IsStaged ||
                !atx24PowerCableBuildKit.IsStaged ||
                !eps12vPowerCableBuildKit.IsStaged ||
                !pcieGpuPowerCableBuildKit.HasMotherboardPrerequisite ||
                !pcieGpuPowerCableBuildKit.HasProcessorPrerequisite ||
                !pcieGpuPowerCableBuildKit.HasMemoryModulePrerequisite ||
                !pcieGpuPowerCableBuildKit.HasStoragePrerequisite ||
                !pcieGpuPowerCableBuildKit.HasProcessorCoolerPrerequisite ||
                !pcieGpuPowerCableBuildKit.HasGraphicsCardPrerequisite ||
                !pcieGpuPowerCableBuildKit.HasPowerSupplyPrerequisite ||
                !pcieGpuPowerCableBuildKit.HasAtx24PowerCablePrerequisite ||
                !pcieGpuPowerCableBuildKit.HasEps12vPowerCablePrerequisite ||
                pcieGpuPowerCableBuildKit.StagedComponentCount != 9 ||
                pcieGpuPowerCableBinding.IsRouted ||
                !pcieGpuPowerCableBinding.IsAuthorityLooseWorld)
            {
                LogPcieGpuPowerCableBuildKitSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogPcieGpuPowerCableBuildKitSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot cableLine =
                workOrder.Lines.SingleOrDefault(
                    line => line.ComponentKind == PcComponentKind.PowerCable &&
                            line.PowerCableType ==
                                PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            if (cableLine == null ||
                cableLine.ItemId != session.PcieGpuPowerCableItemId ||
                !session.Inventory.TryGetReservation(
                    cableLine.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.ItemId != cableLine.ItemId ||
                reservation.ClaimId != workOrder.InventoryClaimId)
            {
                LogPcieGpuPowerCableBuildKitSmokeFailure(
                    "smoke.reservation-mismatch");
                yield break;
            }

            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
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
                    LogPcieGpuPowerCableBuildKitSmokeFailure(
                        "smoke.pcie-gpu-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalCable &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    pcieGpuPowerCableBinding.IsAuthorityInHands &&
                    !pcieGpuPowerCableBinding.IsRouted &&
                    pcieGpuPowerCableBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypePcieGpuPowerCableBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.PcieGpuPowerCableInHands &&
                    ReferenceEquals(pickupReceipt.Line, cableLine) &&
                    pickupReceipt.Line.PowerCableType ==
                        PowerCableType.ModularPcie8PinPsuToGraphicsCard &&
                    pickupReceipt.Line.ItemId == cableLine.ItemId &&
                    pickupReceipt.Line.ReservationId ==
                        cableLine.ReservationId &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    PcieGpuPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogPcieGpuPowerCableBuildKitSmokeFailure(
                        "smoke.pcie-gpu-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands =
                    session.CustomPcBuildKit.Revision;
                OperationResult routeBypass =
                    playerCarry.TrySetPcieGpuPowerCableRouteMode(true);
                bool custodyGuard =
                    routeBypass.IsFailure &&
                    routeBypass.Error.Code ==
                        "custom-pc-pcie-gpu-power-cable-build-kit.authority-blocked" &&
                    !playerCarry.IsPcieGpuPowerCableRouteMode &&
                    playerCarry.TryDrop().IsFailure &&
                    playerCarry.HeldItem == physicalCable &&
                    physicalCable.IsCarried &&
                    pcieGpuPowerCableBinding.IsAuthorityInHands &&
                    !pcieGpuPowerCableBinding.IsRouted &&
                    pcieGpuPowerCableBuildKit.StagedComponentCount == 9 &&
                    session.Inventory.Revision == inventoryRevisionInHands &&
                    session.CustomPcBuildKit.Revision == buildKitRevisionInHands &&
                    PcieGpuPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount);
                if (!custodyGuard)
                {
                    LogPcieGpuPowerCableBuildKitSmokeFailure(
                        "smoke.pcie-gpu-custody-guard-mismatch");
                    yield break;
                }

                MovePcieGpuPowerCableBuildKitSmokePlayerToKit(
                    pcieGpuPowerCableBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool modeValid =
                    playerCarry.IsPcieGpuPowerCableBuildKitMode &&
                    !playerCarry.IsPcieGpuPowerCableRouteMode &&
                    playerCarry.CurrentPcieGpuPowerCableBuildKitStatus ==
                        PcieGpuPowerCableBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("9/10 → 10/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!modeValid)
                {
                    LogPcieGpuPowerCableBuildKitSmokeFailure(
                        "smoke.pcie-gpu-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentPcieGpuPowerCableBuildKitStatus ==
                        PcieGpuPowerCableBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogPcieGpuPowerCableBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    pcieGpuPowerCableBuildKit.IsStaged &&
                    pcieGpuPowerCableBuildKit.StagedComponentCount == 10 &&
                    pcieGpuPowerCableBuildKit.ProgressText.text.Contains("10/10") &&
                    pcieGpuPowerCableBuildKit.ProgressText.text.Contains(
                        "PCIe GPU HAZIR") &&
                    pcieGpuPowerCableBinding.IsAuthorityInBuildKit &&
                    !pcieGpuPowerCableBinding.IsRouted &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    physicalCable.Ownership == PhysicalItemOwnership.World &&
                    physicalCable.IsStablePlacement &&
                    pcieGpuPowerCableBuildKit.MatchesCommittedPlacement(
                        physicalCable) &&
                    Quaternion.Angle(
                        physicalCable.transform.rotation,
                        pcieGpuPowerCableBuildKit.ResolveSnapPose(1).rotation) <=
                            0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypePcieGpuPowerCableBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.PcieGpuPowerCableStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, cableLine) &&
                    session.TryGetPcieGpuPowerCableItem(
                        out InventoryItemRecord stagedCable) &&
                    stagedCable.ContainerId ==
                        session.PcieGpuPowerCableBuildKitContainerId &&
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
                        session.PcieGpuPowerCableBuildKitContainerId).Value == 1 &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    CountCanonicalPcieGpuPowerCableProjections(stableItemId) == 1 &&
                    PcieGpuPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount) &&
                    pcieGpuPowerCableBinding
                        .ValidateProjectionInvariant().IsSuccess &&
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
                    LogPcieGpuPowerCableBuildKitSmokeFailure(
                        "smoke.pcie-gpu-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalPcieGpuPowerCable(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    pcieGpuPowerCableBuildKit.StagedComponentCount == 10 &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.PcieGpuPowerCableBuildKitContainerId).Value == 1 &&
                    PcieGpuPowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount,
                        atx24State,
                        atx24Revision,
                        atx24ReceiptCount,
                        eps12vState,
                        eps12vRevision,
                        eps12vReceiptCount) &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogPcieGpuPowerCableBuildKitSmokeFailure(
                        "smoke.replay-mismatch");
                    yield break;
                }

                if (!_suppressPcieGpuPowerCableBuildKitSmokeSuccessMarker)
                {
                    Debug.Log(PcieGpuPowerCableBuildKitSmokeSuccessMarker);
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

        private static bool PcieGpuPowerCableBuildKitSmokeAssemblyUnchanged(
            GarageStockFlowSession session,
            long expectedAssemblyRevision,
            int expectedAssemblyReceiptCount,
            PcieGpuPowerCableState expectedPcieGpuState,
            long expectedPcieGpuRevision,
            int expectedPcieGpuReceiptCount,
            Atx24PowerCableState expectedAtx24State,
            long expectedAtx24Revision,
            int expectedAtx24ReceiptCount,
            Eps12vPowerCableState expectedPcieState,
            long expectedPcieRevision,
            int expectedPcieReceiptCount)
        {
            return session.AssemblyBuild.Revision == expectedAssemblyRevision &&
                   session.AssemblyBuild.ReceiptCount ==
                       expectedAssemblyReceiptCount &&
                   session.AssemblyBuild.PcieGpuPowerCableState ==
                       expectedPcieGpuState &&
                   session.AssemblyBuild.PcieGpuPowerCableRevision ==
                       expectedPcieGpuRevision &&
                   session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                       expectedPcieGpuReceiptCount &&
                   session.AssemblyBuild.Atx24PowerCableState ==
                       expectedAtx24State &&
                   session.AssemblyBuild.Atx24PowerCableRevision ==
                       expectedAtx24Revision &&
                   session.AssemblyBuild.Atx24PowerCableReceiptCount ==
                       expectedAtx24ReceiptCount &&
                   session.AssemblyBuild.Eps12vPowerCableState ==
                       expectedPcieState &&
                   session.AssemblyBuild.Eps12vPowerCableRevision ==
                       expectedPcieRevision &&
                   session.AssemblyBuild.Eps12vPowerCableReceiptCount ==
                       expectedPcieReceiptCount;
        }

        private void MovePcieGpuPowerCableBuildKitSmokePlayerToKit(
            PcieGpuPowerCableBuildKitProjection buildKit)
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

        private void LogPcieGpuPowerCableBuildKitSmokeFailure(string code)
        {
            if (_suppressPcieGpuPowerCableBuildKitSmokeSuccessMarker)
            {
                _nestedPcieGpuPowerCableBuildKitSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_PCIE_GPU_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
