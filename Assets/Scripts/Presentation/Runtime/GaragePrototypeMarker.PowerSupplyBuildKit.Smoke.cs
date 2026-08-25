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
            "carry=ok input=keyboard+mouse custody-guards=ok rotation=180 " +
            "placement=ok progress=7/10 reservation=alive " +
            "custody=power-supply-build-kit receipts=ok revisions=ok " +
            "assembly=untouched power-supply-bay=untouched " +
            "atx24-route=untouched eps12v-route=untouched pcie-route=untouched " +
            "no-duplicate-loss=ok replay=ok invariants=ok";

        private IEnumerator RunPowerSupplyBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

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

                AimMotherboardBuildKitSmokeAtItem(
                    physicalPowerSupply,
                    -Vector3.forward);
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

                MovePowerSupplyBuildKitSmokePlayerToKit(
                    powerSupplyBuildKit);
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

                Debug.Log(PowerSupplyBuildKitSmokeSuccessMarker);
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

        private void MovePowerSupplyBuildKitSmokePlayerToKit(
            PowerSupplyBuildKitProjection buildKit)
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

        private static void LogPowerSupplyBuildKitSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_POWER_SUPPLY_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
