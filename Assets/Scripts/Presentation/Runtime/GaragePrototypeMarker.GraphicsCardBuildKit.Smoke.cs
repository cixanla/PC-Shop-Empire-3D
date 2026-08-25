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
        public const string GraphicsCardBuildKitSmokeSuccessMarker =
            "GARAGE_GRAPHICS_CARD_BUILD_KIT_RUNTIME_SMOKE " +
            "work-ticket=ok " +
            "prerequisites=motherboard+processor+memory+storage+processor-cooler-staged " +
            "graphics-card-pickup=exact physical-identity=stable " +
            "carry=ok input=keyboard+mouse custody-guards=ok rotation=180 " +
            "placement=ok progress=6/10 reservation=alive " +
            "custody=graphics-card-build-kit receipts=ok revisions=ok " +
            "assembly=untouched graphics-card-slot=untouched " +
            "pcie-route=untouched no-duplicate-loss=ok replay=ok invariants=ok";

        private IEnumerator RunGraphicsCardBuildKitSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedProcessorCoolerBuildKitSmokeFailureCode = null;
            _suppressProcessorCoolerBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunProcessorCoolerBuildKitSmoke();
            }
            finally
            {
                _suppressProcessorCoolerBuildKitSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedProcessorCoolerBuildKitSmokeFailureCode;
            _nestedProcessorCoolerBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    System.StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogGraphicsCardBuildKitSmokeFailure(
                    $"smoke.processor-cooler-prerequisite-{suffix}");
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
                graphicsCardBinding == null ||
                physicalGraphicsCard == null ||
                graphicsCardBuildKit == null ||
                graphicsCardSlot == null ||
                !HasGraphicsCardBuildKitR40Runtime ||
                !motherboardBuildKit.IsStaged ||
                !processorBuildKit.IsStaged ||
                !memoryModuleBuildKit.IsStaged ||
                !storageBuildKit.IsStaged ||
                !processorCoolerBuildKit.IsStaged ||
                processorCoolerBuildKit.StagedComponentCount != 5 ||
                !graphicsCardBuildKit.HasMotherboardPrerequisite ||
                !graphicsCardBuildKit.HasProcessorPrerequisite ||
                !graphicsCardBuildKit.HasMemoryModulePrerequisite ||
                !graphicsCardBuildKit.HasStoragePrerequisite ||
                !graphicsCardBuildKit.HasProcessorCoolerPrerequisite ||
                graphicsCardBuildKit.StagedComponentCount != 5)
            {
                LogGraphicsCardBuildKitSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogGraphicsCardBuildKitSmokeFailure(
                    "smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot graphicsCardLine =
                workOrder.Lines.SingleOrDefault(
                    line => line.ComponentKind == PcComponentKind.GraphicsCard);
            if (graphicsCardLine == null ||
                graphicsCardLine.ItemId != session.GraphicsCardAssemblyItemId ||
                !session.Inventory.TryGetReservation(
                    graphicsCardLine.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.ItemId != graphicsCardLine.ItemId ||
                reservation.ClaimId != workOrder.InventoryClaimId)
            {
                LogGraphicsCardBuildKitSmokeFailure(
                    "smoke.reservation-mismatch");
                yield break;
            }

            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            int physicalIdentity = physicalGraphicsCard.GetInstanceID();
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
                    physicalGraphicsCard,
                    -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != physicalGraphicsCard)
                {
                    LogGraphicsCardBuildKitSmokeFailure(
                        "smoke.graphics-card-focus-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                CustomPcBuildKitReceipt pickupReceipt = null;
                bool exactPickup =
                    playerCarry.HeldItem == physicalGraphicsCard &&
                    physicalGraphicsCard.GetInstanceID() == physicalIdentity &&
                    graphicsCardBinding.IsAuthorityInHands &&
                    graphicsCardBuildKit.HasPickupReceipt &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeGraphicsCardBuildKitOperationId,
                        out pickupReceipt) &&
                    pickupReceipt.Stage ==
                        CustomPcBuildKitStage.GraphicsCardInHands &&
                    ReferenceEquals(pickupReceipt.Line, graphicsCardLine) &&
                    pickupReceipt.Line.LineId == graphicsCardLine.LineId &&
                    pickupReceipt.Line.ProductId == graphicsCardLine.ProductId &&
                    pickupReceipt.Line.ItemId == graphicsCardLine.ItemId &&
                    pickupReceipt.Line.ReservationId ==
                        graphicsCardLine.ReservationId &&
                    session.Inventory.Revision ==
                        inventoryRevisionBeforePickup + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionBeforePickup + 1 &&
                    GraphicsCardBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!exactPickup)
                {
                    LogGraphicsCardBuildKitSmokeFailure(
                        "smoke.graphics-card-pickup-mismatch");
                    yield break;
                }

                long inventoryRevisionInHands = session.Inventory.Revision;
                long buildKitRevisionInHands =
                    session.CustomPcBuildKit.Revision;
                bool custodyGuard =
                    playerCarry.TryDrop().IsFailure &&
                    playerCarry.HeldItem == physicalGraphicsCard &&
                    physicalGraphicsCard.IsCarried &&
                    graphicsCardBinding.IsAuthorityInHands &&
                    graphicsCardBuildKit.StagedComponentCount == 5 &&
                    session.Inventory.Revision == inventoryRevisionInHands &&
                    session.CustomPcBuildKit.Revision == buildKitRevisionInHands &&
                    GraphicsCardBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount);
                if (!custodyGuard)
                {
                    LogGraphicsCardBuildKitSmokeFailure(
                        "smoke.graphics-card-custody-guard-mismatch");
                    yield break;
                }

                MoveGraphicsCardBuildKitSmokePlayerToKit(
                    graphicsCardBuildKit);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool modeValid =
                    playerCarry.IsGraphicsCardBuildKitMode &&
                    !playerCarry.IsGraphicsCardSeatMode &&
                    playerCarry.CurrentGraphicsCardBuildKitStatus ==
                        GraphicsCardBuildKitStatus.Valid &&
                    playerCarry.PlacementValid &&
                    playerCarry.PromptText.Contains("5/10 → 6/10");
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!modeValid)
                {
                    LogGraphicsCardBuildKitSmokeFailure(
                        "smoke.graphics-card-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.R);
                bool rotationValid =
                    playerCarry.PlacementRotationQuarterTurns == 1 &&
                    playerCarry.CurrentGraphicsCardBuildKitStatus ==
                        GraphicsCardBuildKitStatus.Valid &&
                    playerCarry.PlacementValid;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!rotationValid)
                {
                    LogGraphicsCardBuildKitSmokeFailure(
                        "smoke.rotation-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                CustomPcBuildKitReceipt placementReceipt = null;
                bool exactPlacement =
                    playerCarry.HeldItem == null &&
                    graphicsCardBuildKit.IsStaged &&
                    graphicsCardBuildKit.StagedComponentCount == 6 &&
                    graphicsCardBuildKit.ProgressText.text.Contains("6/10") &&
                    graphicsCardBuildKit.ProgressText.text.Contains("GPU HAZIR") &&
                    graphicsCardBinding.IsAuthorityInBuildKit &&
                    physicalGraphicsCard.GetInstanceID() == physicalIdentity &&
                    physicalGraphicsCard.Ownership ==
                        PhysicalItemOwnership.World &&
                    physicalGraphicsCard.IsStablePlacement &&
                    graphicsCardBuildKit.MatchesCommittedPlacement(
                        physicalGraphicsCard) &&
                    Quaternion.Angle(
                        physicalGraphicsCard.transform.rotation,
                        graphicsCardBuildKit.ResolveSnapPose(1).rotation) <=
                            0.25f &&
                    session.CustomPcBuildKit.TryGetReceipt(
                        session.PrototypeGraphicsCardBuildKitOperationId,
                        out placementReceipt) &&
                    placementReceipt.Stage ==
                        CustomPcBuildKitStage.GraphicsCardStaged &&
                    !ReferenceEquals(placementReceipt, pickupReceipt) &&
                    ReferenceEquals(placementReceipt.Line, graphicsCardLine) &&
                    session.TryGetGraphicsCardAssemblyItem(
                        out InventoryItemRecord stagedGraphicsCard) &&
                    stagedGraphicsCard.ContainerId ==
                        session.GraphicsCardBuildKitContainerId &&
                    session.Inventory.TryGetReservation(
                        graphicsCardLine.ReservationId,
                        out InventoryReservation stagedReservation) &&
                    stagedReservation.ItemId == stagedGraphicsCard.Id &&
                    stagedReservation.ClaimId == workOrder.InventoryClaimId &&
                    session.Inventory.Revision == inventoryRevisionInHands + 1 &&
                    session.CustomPcBuildKit.Revision ==
                        buildKitRevisionInHands + 1 &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.GraphicsCardBuildKitContainerId).Value == 1 &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    CountCanonicalGraphicsCardProjections(
                        session.GraphicsCardAssemblyItemId.Value) == 1 &&
                    GraphicsCardBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount) &&
                    graphicsCardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorCoolerBinding.ValidateProjectionInvariant().IsSuccess &&
                    storageBinding.ValidateProjectionInvariant().IsSuccess &&
                    dimmBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!exactPlacement)
                {
                    LogGraphicsCardBuildKitSmokeFailure(
                        "smoke.graphics-card-placement-mismatch");
                    yield break;
                }

                long committedInventoryRevision = session.Inventory.Revision;
                long committedBuildKitRevision =
                    session.CustomPcBuildKit.Revision;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                OperationResult<CustomPcBuildKitReceipt> replay =
                    session.CustomPcBuildKit.PlaceCanonicalGraphicsCard(
                        pickupReceipt);
                bool replaySafe =
                    replay.IsSuccess &&
                    ReferenceEquals(replay.Value, placementReceipt) &&
                    session.Inventory.Revision == committedInventoryRevision &&
                    session.CustomPcBuildKit.Revision ==
                        committedBuildKitRevision &&
                    graphicsCardBuildKit.StagedComponentCount == 6 &&
                    physicalGraphicsCard.GetInstanceID() == physicalIdentity &&
                    session.Inventory.SerializedItemCount == serializedItemCount &&
                    session.Inventory.GetContainerQuantity(
                        session.HandsContainerId).Value == 0 &&
                    session.Inventory.GetContainerQuantity(
                        session.GraphicsCardBuildKitContainerId).Value == 1 &&
                    GraphicsCardBuildKitSmokeAssemblyUnchanged(
                        session,
                        assemblyBefore,
                        assemblyReceiptCount,
                        pcieState,
                        pcieRevision,
                        pcieReceiptCount) &&
                    session.ValidateInvariants().IsSuccess;
                if (!replaySafe)
                {
                    LogGraphicsCardBuildKitSmokeFailure(
                        "smoke.replay-mismatch");
                    yield break;
                }

                Debug.Log(GraphicsCardBuildKitSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(
                    smokeKeyboard,
                    smokeMouse);
            }
        }

        private static bool GraphicsCardBuildKitSmokeAssemblyUnchanged(
            GarageStockFlowSession session,
            AssemblyBuildSnapshot expected,
            int expectedAssemblyReceiptCount,
            PcieGpuPowerCableState expectedPcieState,
            long expectedPcieRevision,
            int expectedPcieReceiptCount)
        {
            AssemblyBuildSnapshot actual = session.AssemblyBuild.GetSnapshot();
            return actual.Revision == expected.Revision &&
                   session.AssemblyBuild.ReceiptCount ==
                       expectedAssemblyReceiptCount &&
                   actual.GraphicsCardSlotState ==
                       expected.GraphicsCardSlotState &&
                   actual.GraphicsCardItemId == expected.GraphicsCardItemId &&
                   actual.GraphicsCardProductId ==
                       expected.GraphicsCardProductId &&
                   actual.GraphicsCardMountOrientation ==
                       expected.GraphicsCardMountOrientation &&
                   actual.GraphicsCardSeatedByOperationId ==
                       expected.GraphicsCardSeatedByOperationId &&
                   actual.GraphicsCardRetainedByOperationId ==
                       expected.GraphicsCardRetainedByOperationId &&
                   session.AssemblyBuild.PcieGpuPowerCableState ==
                       expectedPcieState &&
                   session.AssemblyBuild.PcieGpuPowerCableRevision ==
                       expectedPcieRevision &&
                   session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                       expectedPcieReceiptCount;
        }

        private void MoveGraphicsCardBuildKitSmokePlayerToKit(
            GraphicsCardBuildKitProjection buildKit)
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

        private static void LogGraphicsCardBuildKitSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_GRAPHICS_CARD_BUILD_KIT_RUNTIME_SMOKE " +
                $"build-kit-flow=failed code={code}");
        }
    }
}
