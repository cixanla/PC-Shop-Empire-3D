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
        public const string Atx24PowerCableBuildKitSmokeSuccessMarker =
            "GARAGE_ATX24_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok " +
            "prerequisites=motherboard+processor+memory+storage+processor-cooler+graphics-card+power-supply-staged " +
            "atx24-pickup=exact cable-family=modular-atx24-split " +
            "physical-identity=stable carry=ok input=keyboard+mouse " +
            "prerequisite-positioning=teleport-assisted " +
            "custody-guards=ok route-consumer=blocked rotation=180 " +
            "placement=ok progress=8/10 reservation=alive " +
            "custody=atx24-build-kit receipts=ok revisions=ok " +
            "assembly=untouched atx24-route=untouched " +
            "eps12v-route=untouched pcie-route=untouched " +
            "no-duplicate-loss=ok replay=ok invariants=ok";

        private bool _suppressAtx24PowerCableBuildKitSmokeSuccessMarker;
        private string _nestedAtx24PowerCableBuildKitSmokeFailureCode;

        private IEnumerator RunAtx24PowerCableBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedPowerSupplyBuildKitSmokeFailureCode = null;
            _suppressPowerSupplyBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunPowerSupplyBuildKitSmoke();
            }
            finally
            {
                _suppressPowerSupplyBuildKitSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedPowerSupplyBuildKitSmokeFailureCode;
            _nestedPowerSupplyBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    System.StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogAtx24PowerCableBuildKitSmokeFailure(
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
                atx24PowerCableBinding == null ||
                physicalCable == null ||
                atx24PowerCableBuildKit == null ||
                atx24PowerCableRoute == null ||
                !HasAtx24PowerCableBuildKitR42Runtime ||
                !motherboardBuildKit.IsStaged ||
                !processorBuildKit.IsStaged ||
                !memoryModuleBuildKit.IsStaged ||
                !storageBuildKit.IsStaged ||
                !processorCoolerBuildKit.IsStaged ||
                !graphicsCardBuildKit.IsStaged ||
                !powerSupplyBuildKit.IsStaged ||
                !atx24PowerCableBuildKit.HasMotherboardPrerequisite ||
                !atx24PowerCableBuildKit.HasProcessorPrerequisite ||
                !atx24PowerCableBuildKit.HasMemoryModulePrerequisite ||
                !atx24PowerCableBuildKit.HasStoragePrerequisite ||
                !atx24PowerCableBuildKit.HasProcessorCoolerPrerequisite ||
                !atx24PowerCableBuildKit.HasGraphicsCardPrerequisite ||
                !atx24PowerCableBuildKit.HasPowerSupplyPrerequisite ||
                atx24PowerCableBuildKit.StagedComponentCount != 7 ||
                atx24PowerCableBinding.IsRouted ||
                !atx24PowerCableBinding.IsAuthorityLooseWorld)
            {
                LogAtx24PowerCableBuildKitSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogAtx24PowerCableBuildKitSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot cableLine =
                workOrder.Lines.SingleOrDefault(
                    line => line.ComponentKind == PcComponentKind.PowerCable &&
                            line.PowerCableType ==
                                PowerCableType.ModularAtx24SplitPsuToMotherboard);
            if (cableLine == null ||
                cableLine.ItemId != session.Atx24PowerCableItemId ||
                !session.Inventory.TryGetReservation(
                    cableLine.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.ItemId != cableLine.ItemId ||
                reservation.ClaimId != workOrder.InventoryClaimId)
            {
                LogAtx24PowerCableBuildKitSmokeFailure(
                    "smoke.reservation-mismatch");
                yield break;
            }

            long assemblyRevision = session.AssemblyBuild.Revision;
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
                    LogAtx24PowerCableBuildKitSmokeFailure(
                        "smoke.atx24-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalCable &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    atx24PowerCableBinding.IsAuthorityInHands &&
                    !atx24PowerCableBinding.IsRouted &&
                    atx24PowerCableBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeAtx24PowerCableBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.Atx24PowerCableInHands &&
                    ReferenceEquals(pickupReceipt.Line, cableLine) &&
                    pickupReceipt.Line.PowerCableType ==
                        PowerCableType.ModularAtx24SplitPsuToMotherboard &&
                    pickupReceipt.Line.ItemId == cableLine.ItemId &&
                    pickupReceipt.Line.ReservationId ==
                        cableLine.ReservationId &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    Atx24PowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
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
                    LogAtx24PowerCableBuildKitSmokeFailure(
                        "smoke.atx24-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands =
                    session.CustomPcBuildKit.Revision;
                bool custodyGuard =
                    playerCarry.TryDrop().IsFailure &&
                    playerCarry.HeldItem == physicalCable &&
                    physicalCable.IsCarried &&
                    atx24PowerCableBinding.IsAuthorityInHands &&
                    !atx24PowerCableBinding.IsRouted &&
                    atx24PowerCableBuildKit.StagedComponentCount == 7 &&
                    session.Inventory.Revision == inventoryRevisionInHands &&
                    session.CustomPcBuildKit.Revision == buildKitRevisionInHands &&
                    Atx24PowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
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
                    LogAtx24PowerCableBuildKitSmokeFailure(
                        "smoke.atx24-custody-guard-mismatch");
                    yield break;
                }

                MoveAtx24PowerCableBuildKitSmokePlayerToKit(
                    atx24PowerCableBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool modeValid =
                    playerCarry.IsAtx24PowerCableBuildKitMode &&
                    !playerCarry.IsAtx24PowerCableRouteMode &&
                    playerCarry.CurrentAtx24PowerCableBuildKitStatus ==
                        Atx24PowerCableBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("7/10 → 8/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!modeValid)
                {
                    LogAtx24PowerCableBuildKitSmokeFailure(
                        "smoke.atx24-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentAtx24PowerCableBuildKitStatus ==
                        Atx24PowerCableBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogAtx24PowerCableBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    atx24PowerCableBuildKit.IsStaged &&
                    atx24PowerCableBuildKit.StagedComponentCount == 8 &&
                    atx24PowerCableBuildKit.ProgressText.text.Contains("8/10") &&
                    atx24PowerCableBuildKit.ProgressText.text.Contains(
                        "ATX24 HAZIR") &&
                    atx24PowerCableBinding.IsAuthorityInBuildKit &&
                    !atx24PowerCableBinding.IsRouted &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    physicalCable.ItemIdValue == stableItemId &&
                    physicalCable.Ownership == PhysicalItemOwnership.World &&
                    physicalCable.IsStablePlacement &&
                    atx24PowerCableBuildKit.MatchesCommittedPlacement(
                        physicalCable) &&
                    Quaternion.Angle(
                        physicalCable.transform.rotation,
                        atx24PowerCableBuildKit.ResolveSnapPose(1).rotation) <=
                            0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeAtx24PowerCableBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.Atx24PowerCableStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, cableLine) &&
                    session.TryGetAtx24PowerCableItem(
                        out InventoryItemRecord stagedCable) &&
                    stagedCable.ContainerId ==
                        session.Atx24PowerCableBuildKitContainerId &&
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
                        session.Atx24PowerCableBuildKitContainerId).Value == 1 &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    CountCanonicalAtx24PowerCableProjections(stableItemId) == 1 &&
                    Atx24PowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
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
                    LogAtx24PowerCableBuildKitSmokeFailure(
                        "smoke.atx24-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalAtx24PowerCable(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    atx24PowerCableBuildKit.StagedComponentCount == 8 &&
                    physicalCable.GetInstanceID() == physicalIdentity &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.Atx24PowerCableBuildKitContainerId).Value == 1 &&
                    Atx24PowerCableBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyRevision,
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
                    LogAtx24PowerCableBuildKitSmokeFailure(
                        "smoke.replay-mismatch");
                    yield break;
                }

                if (!_suppressAtx24PowerCableBuildKitSmokeSuccessMarker)
                {
                    Debug.Log(Atx24PowerCableBuildKitSmokeSuccessMarker);
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

        private static bool Atx24PowerCableBuildKitSmokeAssemblyUnchanged(
            GarageStockFlowSession session,
            long expectedAssemblyRevision,
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
            return session.AssemblyBuild.Revision == expectedAssemblyRevision &&
                   session.AssemblyBuild.ReceiptCount ==
                       expectedAssemblyReceiptCount &&
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

        private void MoveAtx24PowerCableBuildKitSmokePlayerToKit(
            Atx24PowerCableBuildKitProjection buildKit)
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

        private void LogAtx24PowerCableBuildKitSmokeFailure(string code)
        {
            if (_suppressAtx24PowerCableBuildKitSmokeSuccessMarker)
            {
                _nestedAtx24PowerCableBuildKitSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_ATX24_POWER_CABLE_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
