using System.Collections;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        private IEnumerator RunGraphicsCardSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (playerMotor == null ||
                session == null ||
                playerCarry == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                motherboardBinding == null ||
                graphicsCardSlot == null ||
                graphicsCardBinding == null ||
                graphicsCard == null ||
                !HasGraphicsCardR28Runtime)
            {
                LogGraphicsCardSmokeFailure("smoke.context-missing");
                yield break;
            }

            Pose initialGraphicsCardPose = new Pose(
                graphicsCard.transform.position,
                graphicsCard.transform.rotation);
            Transform initialGraphicsCardParent = graphicsCard.transform.parent;
            int physicalIdentity = graphicsCard.GetInstanceID();
            bool pcieInterface = graphicsCardSlot.IsConfigured &&
                                 graphicsCardSlot.SlotIdValue ==
                                     session.GraphicsCardSlotId.Value &&
                                 graphicsCardSlot.LatchIdValue ==
                                     session.GraphicsCardLatchId.Value &&
                                 graphicsCardSlot.RearBracketIdValue ==
                                     GarageStockFlowSession
                                         .GraphicsCardRearBracketIdValue &&
                                 graphicsCardSlot.RearBracketFastenerIdValue ==
                                     session.GraphicsCardBracketFastenerId.Value &&
                                 graphicsCardSlot.SlotInterface ==
                                     GraphicsCardPcieInterface.PcieX16 &&
                                 graphicsCardBinding.CardInterface ==
                                     GraphicsCardPcieInterface.PcieX16 &&
                                 graphicsCardBinding.Slot == graphicsCardSlot &&
                                 graphicsCardBinding.PhysicalItem == graphicsCard;
            bool preflight = pcieInterface &&
                             session.AssemblyBuild.HasGraphicsCardSlot &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.Empty &&
                             session.AssemblyBuild.GraphicsCardSlotState ==
                                 GraphicsCardSlotState.EmptyOpen &&
                             session.GraphicsCardAssemblyItemId != session.ItemId &&
                             session.TryGetGraphicsCardAssemblyItem(
                                 out InventoryItemRecord looseGraphicsCard) &&
                             looseGraphicsCard.Id ==
                                 session.GraphicsCardAssemblyItemId &&
                             looseGraphicsCard.ProductId == session.ProductId &&
                             looseGraphicsCard.ContainerId ==
                                 session.WorldFloorContainerId &&
                             graphicsCardSlot.ChassisClearanceBlockers.Length == 5 &&
                             graphicsCardSlot.CoolerClearanceBlockers.Length == 1 &&
                             graphicsCardBinding.HasChassisClearance &&
                             graphicsCardBinding.HasCoolerClearance &&
                             CountCanonicalGraphicsCardProjections(
                                 session.GraphicsCardAssemblyItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             session.Inventory.GetContainerQuantity(
                                 session.HandsContainerId).Value == 0 &&
                             session.Inventory.GetContainerQuantity(
                                 session.GraphicsCardSlotContainerId).Value == 0 &&
                             graphicsCardBinding.ValidateProjectionInvariant()
                                 .IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            if (!preflight)
            {
                LogGraphicsCardSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long settlementRevision = session.CheckoutSettlements.Revision;
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;

            OperationResult pickupMotherboard =
                playerCarry.TryPickup(motherboardBinding.PhysicalItem);
            MovePlayerToMotherboardSeat();
            OperationResult enterMotherboardMode = pickupMotherboard.IsSuccess
                ? playerCarry.TrySetMotherboardSeatMode(true)
                : OperationResult.Fail(pickupMotherboard.Error);
            OperationResult attach = enterMotherboardMode.IsSuccess
                ? playerCarry.TryConfirmMotherboardSeat()
                : OperationResult.Fail(enterMotherboardMode.Error);
            MovePlayerToMotherboardFastener();
            OperationResult secure = attach.IsSuccess
                ? playerCarry.TryOperateMotherboardFastener()
                : OperationResult.Fail(attach.Error);
            if (pickupMotherboard.IsFailure ||
                enterMotherboardMode.IsFailure ||
                attach.IsFailure ||
                secure.IsFailure ||
                session.AssemblyBuild.MotherboardSeatState !=
                    AssemblySeatState.SeatedSecured ||
                motherboardBinding.ValidateProjectionInvariant().IsFailure ||
                session.ValidateInvariants().IsFailure)
            {
                LogGraphicsCardSmokeFailure(
                    pickupMotherboard.IsFailure
                        ? pickupMotherboard.Error.Code
                        : enterMotherboardMode.IsFailure
                            ? enterMotherboardMode.Error.Code
                            : attach.IsFailure
                                ? attach.Error.Code
                                : secure.IsFailure
                                    ? secure.Error.Code
                                    : "smoke.host-preflight-failed");
                yield break;
            }

            OperationResult pickup = playerCarry.TryPickup(graphicsCard);
            MovePlayerToGraphicsCardSlot();
            OperationResult enterMode = pickup.IsSuccess
                ? playerCarry.TrySetGraphicsCardSeatMode(true)
                : OperationResult.Fail(pickup.Error);
            OperationResult rotateInvalid = enterMode.IsSuccess
                ? playerCarry.TryRotateGraphicsCardSeatPreview()
                : OperationResult.Fail(enterMode.Error);
            long invalidAssemblyRevision = session.AssemblyBuild.Revision;
            long invalidInventoryRevision = session.Inventory.Revision;
            int invalidReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult invalidOrientationConfirm = rotateInvalid.IsSuccess
                ? playerCarry.TryConfirmGraphicsCardSeat()
                : OperationResult.Fail(rotateInvalid.Error);
            bool invalidOrientationBlocked = rotateInvalid.IsSuccess &&
                                             invalidOrientationConfirm.IsFailure &&
                                             invalidOrientationConfirm.Error.Code ==
                                                 "assembly-graphics-card.orientation-mismatch" &&
                                             !playerCarry.PlacementValid &&
                                             playerCarry.CurrentGraphicsCardSlotStatus ==
                                                 GraphicsCardSlotStatus
                                                     .OrientationInvalid &&
                                             playerCarry.LastFailureCode ==
                                                 "assembly-graphics-card.orientation-mismatch" &&
                                             playerCarry.HeldItem == graphicsCard &&
                                             session.AssemblyBuild.Revision ==
                                                 invalidAssemblyRevision &&
                                             session.Inventory.Revision ==
                                                 invalidInventoryRevision &&
                                             session.AssemblyBuild.ReceiptCount ==
                                                 invalidReceiptCount;
            OperationResult rotatePrimary = rotateInvalid.IsSuccess
                ? playerCarry.TryRotateGraphicsCardSeatPreview()
                : OperationResult.Fail(rotateInvalid.Error);
            bool primaryOrientationReady = rotatePrimary.IsSuccess &&
                                           playerCarry.PlacementValid &&
                                           playerCarry.CurrentGraphicsCardSlotStatus ==
                                               GraphicsCardSlotStatus.ValidSeat;
            long seatAssemblyRevision = session.AssemblyBuild.Revision;
            long seatInventoryRevision = session.Inventory.Revision;
            int seatReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult seat = primaryOrientationReady
                ? playerCarry.TryConfirmGraphicsCardSeat()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.gpu-primary-orientation-not-ready"));
            AssemblyOperationReceipt seatReceipt = seat.IsSuccess
                ? session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1]
                : null;
            AssemblyBuildSnapshot seatedSnapshot =
                session.AssemblyBuild.GetSnapshot();
            bool seatingStable = seat.IsSuccess &&
                                 seatReceipt != null &&
                                 seatReceipt.OperationKind ==
                                     AssemblyOperationKind.SeatGraphicsCard &&
                                 seatReceipt.ItemId ==
                                     session.GraphicsCardAssemblyItemId &&
                                 seatReceipt.ProductId == session.ProductId &&
                                 seatReceipt.SlotId ==
                                     session.GraphicsCardSlotId &&
                                 seatReceipt.GraphicsCardMountOrientation ==
                                     GraphicsCardMountOrientation.Primary &&
                                 seatReceipt.SourceAttachOperationId ==
                                     seatedSnapshot.InstalledByOperationId &&
                                 seatReceipt.SourceSecureOperationId ==
                                     seatedSnapshot.SecuredByOperationId &&
                                 session.AssemblyBuild.Revision ==
                                     seatAssemblyRevision + 1 &&
                                 session.Inventory.Revision ==
                                     seatInventoryRevision + 1 &&
                                 session.AssemblyBuild.ReceiptCount ==
                                     seatReceiptCount + 1 &&
                                 playerCarry.HeldItem == null &&
                                 graphicsCard.GetInstanceID() == physicalIdentity &&
                                 graphicsCardBinding.IsSeated &&
                                 !graphicsCardBinding.IsRetained &&
                                 seatedSnapshot.GraphicsCardSlotState ==
                                     GraphicsCardSlotState
                                         .GraphicsCardSeatedUnsecured &&
                                 graphicsCardBinding.ValidateProjectionInvariant()
                                     .IsSuccess;
            if (!invalidOrientationBlocked ||
                !primaryOrientationReady ||
                !seatingStable)
            {
                LogGraphicsCardSmokeFailure(
                    seat.IsFailure
                        ? seat.Error.Code
                        : !invalidOrientationBlocked
                            ? "smoke.gpu-rotation-gate-failed"
                            : "smoke.gpu-seat-failed");
                yield break;
            }

            long duplicateAssemblyRevision = session.AssemblyBuild.Revision;
            long duplicateInventoryRevision = session.Inventory.Revision;
            int duplicateReceiptCount = session.AssemblyBuild.ReceiptCount;
            Pose seatedPose = new Pose(
                graphicsCard.transform.position,
                graphicsCard.transform.rotation);
            OperationResult duplicateSeat = graphicsCardBinding.TryAttachAt(
                seatedPose,
                GraphicsCardSeatOrientation.Primary,
                null,
                graphicsCard.gameObject.layer);
            bool duplicateSeatBlocked = duplicateSeat.IsFailure &&
                                        duplicateSeat.Error.Code ==
                                            "assembly-graphics-card.attach-authority-mismatch" &&
                                        session.AssemblyBuild.Revision ==
                                            duplicateAssemblyRevision &&
                                        session.Inventory.Revision ==
                                            duplicateInventoryRevision &&
                                        session.AssemblyBuild.ReceiptCount ==
                                            duplicateReceiptCount &&
                                        graphicsCard.GetInstanceID() ==
                                            physicalIdentity &&
                                        graphicsCardBinding
                                            .ValidateProjectionInvariant().IsSuccess;

            MovePlayerToGraphicsCardSlot();
            long retainAssemblyRevision = session.AssemblyBuild.Revision;
            long retainInventoryRevision = session.Inventory.Revision;
            int retainReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult retain = playerCarry.TryOperateGraphicsCardRetention();
            AssemblyOperationReceipt retainReceipt = retain.IsSuccess
                ? session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1]
                : null;
            AssemblyBuildSnapshot retainedSnapshot =
                session.AssemblyBuild.GetSnapshot();
            bool retentionStable = retain.IsSuccess &&
                                   retainReceipt != null &&
                                   retainReceipt.OperationKind ==
                                       AssemblyOperationKind.RetainGraphicsCard &&
                                   retainReceipt.ItemId ==
                                       session.GraphicsCardAssemblyItemId &&
                                   retainReceipt.ProductId == session.ProductId &&
                                   retainReceipt.SlotId ==
                                       session.GraphicsCardSlotId &&
                                   retainReceipt.SourceGraphicsCardSeatOperationId ==
                                       seatReceipt.OperationId &&
                                   retainReceipt.GraphicsCardSlotDefinition
                                       .RetentionTopology.LatchId ==
                                       session.GraphicsCardLatchId &&
                                   retainReceipt.GraphicsCardSlotDefinition
                                       .RetentionTopology.BracketFastenerId ==
                                       session.GraphicsCardBracketFastenerId &&
                                   session.AssemblyBuild.Revision ==
                                       retainAssemblyRevision + 1 &&
                                   session.Inventory.Revision ==
                                       retainInventoryRevision &&
                                   session.AssemblyBuild.ReceiptCount ==
                                       retainReceiptCount + 1;
            long retainedGateAssemblyRevision = session.AssemblyBuild.Revision;
            long retainedGateInventoryRevision = session.Inventory.Revision;
            int retainedGateReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult retainedRemoval = playerCarry.TryPickup(graphicsCard);
            bool retainedGate = retentionStable &&
                                graphicsCardBinding.IsRetained &&
                                retainedSnapshot.GraphicsCardSlotState ==
                                    GraphicsCardSlotState.GraphicsCardRetained &&
                                !retainedSnapshot
                                    .GraphicsCardRetainedByOperationId.IsEmpty &&
                                retainedRemoval.IsFailure &&
                                retainedRemoval.Error ==
                                    AssemblyFailures.GraphicsCardRetained &&
                                session.AssemblyBuild.Revision ==
                                    retainedGateAssemblyRevision &&
                                session.Inventory.Revision ==
                                    retainedGateInventoryRevision &&
                                session.AssemblyBuild.ReceiptCount ==
                                    retainedGateReceiptCount &&
                                graphicsCardBinding.ValidateProjectionInvariant()
                                    .IsSuccess;
            if (!duplicateSeatBlocked || !retainedGate)
            {
                LogGraphicsCardSmokeFailure(
                    !duplicateSeatBlocked
                        ? "smoke.gpu-duplicate-seat-not-blocked"
                        : retain.IsFailure
                        ? retain.Error.Code
                        : "smoke.gpu-retention-gate-failed");
                yield break;
            }

            OperationResult motherboardUnsecure =
                motherboardBinding.TryOperateFastener();
            if (motherboardUnsecure.IsSuccess)
            {
                graphicsCardBinding.SyncProjectionToAuthority();
            }
            long hostGateAssemblyRevision = session.AssemblyBuild.Revision;
            long hostGateInventoryRevision = session.Inventory.Revision;
            int hostGateReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult<AssemblyOperationReceipt> hostDetach =
                session.DetachMotherboard(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.smoke.gpu-host-detach"));
            bool hostDetachGate = motherboardUnsecure.IsSuccess &&
                                  hostDetach.IsFailure &&
                                  hostDetach.Error ==
                                      AssemblyFailures.GraphicsCardInstalled &&
                                  playerCarry.HeldItem == null &&
                                  session.AssemblyBuild.MotherboardSeatState ==
                                      AssemblySeatState.SeatedUnsecured &&
                                  session.AssemblyBuild.GraphicsCardSlotState ==
                                      GraphicsCardSlotState.GraphicsCardRetained &&
                                  session.AssemblyBuild.Revision ==
                                      hostGateAssemblyRevision &&
                                  session.Inventory.Revision ==
                                      hostGateInventoryRevision &&
                                  session.AssemblyBuild.ReceiptCount ==
                                      hostGateReceiptCount;
            if (!hostDetachGate)
            {
                LogGraphicsCardSmokeFailure(
                    motherboardUnsecure.IsFailure
                        ? motherboardUnsecure.Error.Code
                        : hostDetach.IsSuccess
                            ? "smoke.gpu-host-detach-unexpected-success"
                            : hostDetach.Error !=
                              AssemblyFailures.GraphicsCardInstalled
                                ? hostDetach.Error.Code
                                : playerCarry.HeldItem != null
                                    ? "smoke.gpu-host-detach-held-item-mutated"
                                    : session.AssemblyBuild.MotherboardSeatState !=
                                      AssemblySeatState.SeatedUnsecured
                                        ? "smoke.gpu-host-state-mismatch"
                                        : session.AssemblyBuild
                                              .GraphicsCardSlotState !=
                                          GraphicsCardSlotState
                                              .GraphicsCardRetained
                                            ? "smoke.gpu-host-gpu-state-mismatch"
                                            : session.AssemblyBuild.Revision !=
                                              hostGateAssemblyRevision
                                                ? "smoke.gpu-host-assembly-revision-mutated"
                                                : session.Inventory.Revision !=
                                                  hostGateInventoryRevision
                                                    ? "smoke.gpu-host-inventory-revision-mutated"
                                                    : "smoke.gpu-host-receipt-count-mutated");
                yield break;
            }

            MovePlayerToGraphicsCardSlot();
            OperationResult unretain =
                playerCarry.TryOperateGraphicsCardRetention();
            AssemblyOperationReceipt unretainReceipt = unretain.IsSuccess
                ? session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1]
                : null;
            OperationResult remove = unretain.IsSuccess
                ? playerCarry.TryPickup(graphicsCard)
                : OperationResult.Fail(unretain.Error);
            AssemblyOperationReceipt removeReceipt = remove.IsSuccess
                ? session.AssemblyBuild.GetReceipts()[
                    session.AssemblyBuild.ReceiptCount - 1]
                : null;
            bool removalStable = unretain.IsSuccess &&
                                 unretainReceipt != null &&
                                 unretainReceipt.OperationKind ==
                                     AssemblyOperationKind.UnretainGraphicsCard &&
                                 unretainReceipt.SourceGraphicsCardSeatOperationId ==
                                     seatReceipt.OperationId &&
                                 unretainReceipt
                                     .SourceGraphicsCardRetentionOperationId ==
                                     retainReceipt.OperationId &&
                                 remove.IsSuccess &&
                                 removeReceipt != null &&
                                 removeReceipt.OperationKind ==
                                     AssemblyOperationKind.RemoveGraphicsCard &&
                                 removeReceipt.SourceGraphicsCardSeatOperationId ==
                                     seatReceipt.OperationId &&
                                 playerCarry.HeldItem == graphicsCard &&
                                 graphicsCard.GetInstanceID() == physicalIdentity &&
                                 session.AssemblyBuild.GraphicsCardSlotState ==
                                     GraphicsCardSlotState.EmptyOpen &&
                                 graphicsCardBinding.IsAuthorityInHands;
            if (!removalStable)
            {
                LogGraphicsCardSmokeFailure(
                    unretain.IsFailure
                        ? unretain.Error.Code
                        : remove.IsFailure
                            ? remove.Error.Code
                            : "smoke.gpu-remove-failed");
                yield break;
            }

            OperationResult recovery = playerCarry.TryRecoverHeldItem();
            long finalAssemblyRevision = session.AssemblyBuild.Revision;
            int finalReceiptCount = session.AssemblyBuild.ReceiptCount;
            long finalInventoryRevision = session.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> delayedSeatReplay =
                session.SeatGraphicsCard(
                    seatReceipt.OperationId,
                    seatReceipt.GraphicsCardMountOrientation,
                    seatReceipt.SourceAttachOperationId,
                    seatReceipt.SourceSecureOperationId,
                    seatReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedRetainReplay =
                session.RetainGraphicsCard(
                    retainReceipt.OperationId,
                    retainReceipt.SourceGraphicsCardSeatOperationId,
                    retainReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedUnretainReplay =
                session.UnretainGraphicsCard(
                    unretainReceipt.OperationId,
                    unretainReceipt.SourceGraphicsCardSeatOperationId,
                    unretainReceipt.SourceGraphicsCardRetentionOperationId,
                    unretainReceipt.ExpectedAssemblyRevision);
            OperationResult<AssemblyOperationReceipt> delayedRemoveReplay =
                session.RemoveGraphicsCard(
                    removeReceipt.OperationId,
                    removeReceipt.SourceGraphicsCardSeatOperationId,
                    removeReceipt.ExpectedAssemblyRevision);
            bool replayStable = delayedSeatReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedSeatReplay.Value,
                                    seatReceipt) &&
                                delayedRetainReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedRetainReplay.Value,
                                    retainReceipt) &&
                                delayedUnretainReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedUnretainReplay.Value,
                                    unretainReceipt) &&
                                delayedRemoveReplay.IsSuccess &&
                                ReferenceEquals(
                                    delayedRemoveReplay.Value,
                                    removeReceipt) &&
                                session.AssemblyBuild.Revision ==
                                    finalAssemblyRevision &&
                                session.AssemblyBuild.ReceiptCount ==
                                    finalReceiptCount &&
                                session.Inventory.Revision ==
                                    finalInventoryRevision;
            bool recovered = recovery.IsSuccess &&
                             playerCarry.HeldItem == null &&
                             graphicsCard.GetInstanceID() == physicalIdentity &&
                             graphicsCard.transform.parent ==
                                 initialGraphicsCardParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     graphicsCard.transform.position,
                                     graphicsCard.transform.rotation),
                                 initialGraphicsCardPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     graphicsCard.Body.position,
                                     graphicsCard.Body.rotation),
                                 initialGraphicsCardPose) &&
                             ApproximatelySamePose(
                                 new Pose(
                                     graphicsCard.LastSafePosition,
                                     graphicsCard.LastSafeRotation),
                                 initialGraphicsCardPose) &&
                             graphicsCard.Ownership ==
                                 PhysicalItemOwnership.World &&
                             graphicsCard.IsStablePlacement &&
                             graphicsCardBinding.IsAuthorityLooseWorld &&
                             session.TryGetGraphicsCardAssemblyItem(
                                 out InventoryItemRecord recoveredItem) &&
                             recoveredItem.Id ==
                                 session.GraphicsCardAssemblyItemId &&
                             recoveredItem.ProductId == session.ProductId &&
                             recoveredItem.ContainerId ==
                                 session.WorldFloorContainerId &&
                             CountCanonicalGraphicsCardProjections(
                                 session.GraphicsCardAssemblyItemId.Value) == 1 &&
                             session.Inventory.SerializedItemCount == 7 &&
                             session.Inventory.GetContainerQuantity(
                                 session.HandsContainerId).Value == 0 &&
                             session.Inventory.GetContainerQuantity(
                                 session.GraphicsCardSlotContainerId).Value == 0 &&
                             session.ValidateInvariants().IsSuccess &&
                             graphicsCardBinding.ValidateProjectionInvariant()
                                 .IsSuccess;
            bool authorityIsolated = session.Orders.Revision == orderRevision &&
                                     session.RetailOffers.Revision == offerRevision &&
                                     session.RetailBaskets.Revision == basketRevision &&
                                     session.RetailCheckouts.Revision ==
                                         checkoutRevision &&
                                     session.CheckoutSettlements.Revision ==
                                         settlementRevision &&
                                     session.CustomerVisits.Revision == visitRevision &&
                                     session.CustomerConsultations.Revision ==
                                         consultationRevision &&
                                     session.CustomerOfferActions.Revision ==
                                         actionRevision &&
                                     session.AssemblyBuild.ProcessorSocketState ==
                                         ProcessorSocketState.EmptyOpen &&
                                     session.AssemblyBuild.MemorySlotState ==
                                         MemorySlotState.EmptyOpen &&
                                     session.AssemblyBuild.StorageSlotState ==
                                         StorageSlotState.EmptyOpen &&
                                     session.AssemblyBuild.ProcessorCoolerSlotState ==
                                         ProcessorCoolerSlotState.EmptyOpen;
            if (!recovered ||
                !replayStable ||
                !authorityIsolated)
            {
                LogGraphicsCardSmokeFailure(
                    remove.IsFailure
                        ? remove.Error.Code
                        : recovery.IsFailure
                            ? recovery.Error.Code
                            : !recovered
                                ? "smoke.gpu-recovery-failed"
                                : !replayStable
                                    ? "smoke.gpu-delayed-replay-failed"
                                    : "smoke.gpu-authority-isolation-failed");
                yield break;
            }

            Debug.Log(
                "GARAGE_GPU_RUNTIME_SMOKE gpu-flow=ok preflight=ok " +
                "pcie-interface=ok keyed-orientation=ok clearance=ok " +
                "slot-latch=ok rear-bracket=ok duplicate-seat-blocked=ok " +
                "retained-remove-gate=ok host-detach-gate=ok replay=ok " +
                "authority-isolated=ok identity=stable recovery=ok");
            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToGraphicsCardSlot()
        {
            Vector3 target = graphicsCardSlot.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));

            Camera playerCamera = playerMotor.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.transform.rotation = Quaternion.LookRotation(
                    target - playerCamera.transform.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private static int CountCanonicalGraphicsCardProjections(
            string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in
                     FindObjectsByType<PhysicalItemProjection>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (item != null && item.ItemIdValue == canonicalItemId)
                {
                    count++;
                }
            }

            return count;
        }

        private static void LogGraphicsCardSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_GPU_RUNTIME_SMOKE gpu-flow=failed code={code}");
        }
    }
}
