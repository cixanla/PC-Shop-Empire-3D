using System.Collections;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        private const string PowerSupplySmokeSuccessMarker =
            "GARAGE_PSU_RUNTIME_SMOKE psu-flow=ok preflight=ok atx-ps2=ok " +
            "keyed-orientation=ok clearance=ok rear-mount=ok four-screw=ok " +
            "duplicate-seat-blocked=ok retained-remove-gate=ok " +
            "alternate-order=ok replay=ok authority-isolated=ok " +
            "identity=stable recovery=ok";

        private IEnumerator RunPowerSupplySmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (session == null ||
                playerCarry == null ||
                powerSupplyBay == null ||
                powerSupplyBinding == null ||
                powerSupply == null ||
                powerSupplyGeometry == null ||
                !HasPowerSupplyR29Runtime)
            {
                LogPowerSupplySmokeFailure("smoke.context-missing");
                yield break;
            }

            Pose initialPose = new Pose(
                powerSupply.transform.position,
                powerSupply.transform.rotation);
            Transform initialParent = powerSupply.transform.parent;
            Rigidbody body = powerSupply.Body;
            string stableItemId = powerSupply.ItemIdValue;
            AssemblyBuildSnapshot initialAssembly =
                session.AssemblyBuild.GetSnapshot();
            long initialInventoryRevision = session.Inventory.Revision;
            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long settlementRevision = session.CheckoutSettlements.Revision;
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;

            bool preflight = session.Inventory.SerializedItemCount == 7 &&
                             session.TryGetPowerSupplyItem(
                                 out InventoryItemRecord loose) &&
                             loose.Id == session.PowerSupplyItemId &&
                             loose.ProductId == session.PowerSupplyProductId &&
                             loose.ContainerId == session.WorldFloorContainerId &&
                             session.AssemblyBuild.HasPowerSupplyBay &&
                             session.AssemblyBuild.PowerSupplyBayState ==
                                 PowerSupplyBayState.EmptyOpen &&
                             session.AssemblyBuild.MotherboardSeatState ==
                                 AssemblySeatState.Empty &&
                             session.ValidateInvariants().IsSuccess;
            bool atxPs2 = powerSupplyBinding.FormFactor ==
                              PowerSupplyFormFactor.AtxPs2 &&
                          powerSupplyBay.BayFormFactor ==
                              PowerSupplyFormFactor.AtxPs2 &&
                          session.AssemblyBuild.SupportedPowerSupplyType ==
                              PowerSupplyType.AtxPs2;
            PowerSupplyRetentionTopology topology =
                session.AssemblyBuild.PowerSupplyRetentionTopology;
            bool rearMount = topology != null &&
                             topology.RearMountId == session.PowerSupplyRearMountId &&
                             powerSupplyBay.RearMountIdValue ==
                                 session.PowerSupplyRearMountId.Value;
            bool fourScrew = topology != null &&
                             topology.PhysicalOrder.Count == 4 &&
                             topology.DeterministicRetentionOrder[0] ==
                                 session.PowerSupplyTopLeftFastenerId &&
                             topology.DeterministicRetentionOrder[1] ==
                                 session.PowerSupplyBottomRightFastenerId &&
                             topology.DeterministicRetentionOrder[2] ==
                                 session.PowerSupplyTopRightFastenerId &&
                             topology.DeterministicRetentionOrder[3] ==
                                 session.PowerSupplyBottomLeftFastenerId &&
                             topology.ReverseRetentionOrder[0] ==
                                 session.PowerSupplyBottomLeftFastenerId &&
                             powerSupplyBay.FastenerPivots.Length == 4;
            bool clearance = powerSupplyBay.SupportCollider != null &&
                             powerSupplyBay.SupportCollider.enabled &&
                             powerSupplyGeometry.FilteredFloorIntake != null &&
                             powerSupplyGeometry.IsCanonical;
            if (!preflight || !atxPs2 || !rearMount || !fourScrew || !clearance)
            {
                LogPowerSupplySmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            OperationResult pickup = playerCarry.TryPickup(powerSupply);
            if (pickup.IsFailure ||
                !playerCarry.IsCarrying ||
                playerCarry.HeldItem != powerSupply ||
                !powerSupplyBinding.IsAuthorityInHands)
            {
                LogPowerSupplySmokeFailure(
                    pickup.IsFailure ? pickup.Error.Code : "smoke.pickup-mismatch");
                yield break;
            }

            MovePlayerToPowerSupplyBay();
            OperationResult mode = playerCarry.TrySetPowerSupplySeatMode(true);
            OperationResult rotateWrong =
                playerCarry.TryRotatePowerSupplySeatPreview();
            OperationResult wrongConfirm = playerCarry.TryConfirmPowerSupplySeat();
            bool keyedOrientation = mode.IsSuccess &&
                                    rotateWrong.IsSuccess &&
                                    wrongConfirm.IsFailure &&
                                    playerCarry.CurrentPowerSupplyBayStatus ==
                                        PowerSupplyBayStatus.OrientationInvalid &&
                                    powerSupplyBinding.IsAuthorityInHands;
            OperationResult rotateCorrect =
                playerCarry.TryRotatePowerSupplySeatPreview();
            OperationResult seat = playerCarry.TryConfirmPowerSupplySeat();
            AssemblyBuildSnapshot seated = session.AssemblyBuild.GetSnapshot();
            bool seatedOk = rotateCorrect.IsSuccess &&
                            seat.IsSuccess &&
                            !playerCarry.IsCarrying &&
                            powerSupplyBinding.IsSeated &&
                            seated.PowerSupplyBayState ==
                                PowerSupplyBayState.PowerSupplySeatedUnsecured &&
                            session.TryGetPowerSupplyItem(
                                out InventoryItemRecord seatedItem) &&
                            seatedItem.ContainerId ==
                                session.PowerSupplyBayContainerId;
            long seatedInventoryRevision = session.Inventory.Revision;
            if (!keyedOrientation || !seatedOk)
            {
                LogPowerSupplySmokeFailure(
                    seat.IsFailure ? seat.Error.Code : "smoke.seat-mismatch");
                yield break;
            }

            StableId<AssemblyOperationIdScope> seatId =
                seated.PowerSupplySeatedByOperationId;
            bool hasSeatReceipt = session.AssemblyBuild.TryGetReceipt(
                seatId,
                out AssemblyOperationReceipt seatReceipt);
            OperationResult<AssemblyOperationReceipt> duplicateSeat =
                session.SeatPowerSupply(
                    PowerSupplySmokeOperationId("duplicate-seat"),
                    PowerSupplyMountOrientation.FanToFilteredVent,
                    seated.Revision);
            bool duplicateSeatBlocked = duplicateSeat.IsFailure &&
                                        session.AssemblyBuild.Revision ==
                                            seated.Revision;

            MovePlayerToPowerSupplyBay();
            OperationResult retain = playerCarry.TryOperatePowerSupplyRetention();
            AssemblyBuildSnapshot retained = session.AssemblyBuild.GetSnapshot();
            bool retainedOk = retain.IsSuccess &&
                              retained.PowerSupplyBayState ==
                                  PowerSupplyBayState.PowerSupplyRetained &&
                              powerSupplyBinding.IsRetained &&
                              session.Inventory.Revision ==
                                  seatedInventoryRevision;
            OperationResult<AssemblyOperationReceipt> blockedRemove =
                session.RemovePowerSupply(
                    PowerSupplySmokeOperationId("retained-remove"),
                    seatId,
                    retained.Revision);
            bool retainedRemoveGate = blockedRemove.Error ==
                                      AssemblyFailures.PowerSupplyRetained;
            bool alternateOrder = retained.PowerSupplyBayState ==
                                      PowerSupplyBayState.PowerSupplyRetained &&
                                  retained.MotherboardSeatState ==
                                      AssemblySeatState.Empty;

            StableId<AssemblyOperationIdScope> retainId =
                retained.PowerSupplyRetainedByOperationId;
            bool hasRetainReceipt = session.AssemblyBuild.TryGetReceipt(
                retainId,
                out AssemblyOperationReceipt retainReceipt);
            OperationResult<AssemblyOperationReceipt> seatReplay =
                hasSeatReceipt
                    ? session.SeatPowerSupply(
                        seatReceipt.OperationId,
                        seatReceipt.PowerSupplyMountOrientation,
                        seatReceipt.ExpectedAssemblyRevision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            OperationResult<AssemblyOperationReceipt> retainReplay =
                hasRetainReceipt
                    ? session.RetainPowerSupply(
                        retainReceipt.OperationId,
                        retainReceipt.SourcePowerSupplySeatOperationId,
                        retainReceipt.ExpectedAssemblyRevision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            bool immediateReplay = seatReplay.IsSuccess &&
                                   retainReplay.IsSuccess &&
                                   ReferenceEquals(seatReplay.Value, seatReceipt) &&
                                   ReferenceEquals(retainReplay.Value, retainReceipt) &&
                                   session.AssemblyBuild.Revision == retained.Revision;

            MovePlayerToPowerSupplyBay();
            long unretainExpectedRevision = retained.Revision;
            StableId<AssemblyOperationIdScope> unretainId =
                PowerSupplyPrototypeOperationId(
                    "unretain-four-screw",
                    unretainExpectedRevision + 1L);
            OperationResult unretain = playerCarry.TryOperatePowerSupplyRetention();
            AssemblyBuildSnapshot unretained = session.AssemblyBuild.GetSnapshot();
            long removeExpectedRevision = unretained.Revision;
            StableId<AssemblyOperationIdScope> removeId =
                PowerSupplyPrototypeOperationId(
                    "remove",
                    removeExpectedRevision + 1L);
            OperationResult remove = playerCarry.TryPickup(powerSupply);
            AssemblyBuildSnapshot removed = session.AssemblyBuild.GetSnapshot();
            bool removedToHands = unretain.IsSuccess &&
                                  remove.IsSuccess &&
                                  playerCarry.HeldItem == powerSupply &&
                                  powerSupplyBinding.IsAuthorityInHands;

            bool hasUnretainReceipt = session.AssemblyBuild.TryGetReceipt(
                unretainId,
                out AssemblyOperationReceipt unretainReceipt);
            bool hasRemoveReceipt = session.AssemblyBuild.TryGetReceipt(
                removeId,
                out AssemblyOperationReceipt removeReceipt);
            long replayAssemblyRevision = session.AssemblyBuild.Revision;
            long replayInventoryRevision = session.Inventory.Revision;
            int replayReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult<AssemblyOperationReceipt> delayedSeatReplay =
                hasSeatReceipt
                    ? session.SeatPowerSupply(
                        seatReceipt.OperationId,
                        seatReceipt.PowerSupplyMountOrientation,
                        seatReceipt.ExpectedAssemblyRevision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            OperationResult<AssemblyOperationReceipt> delayedRetainReplay =
                hasRetainReceipt
                    ? session.RetainPowerSupply(
                        retainReceipt.OperationId,
                        retainReceipt.SourcePowerSupplySeatOperationId,
                        retainReceipt.ExpectedAssemblyRevision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            OperationResult<AssemblyOperationReceipt> delayedUnretainReplay =
                hasUnretainReceipt
                    ? session.UnretainPowerSupply(
                        unretainReceipt.OperationId,
                        unretainReceipt.SourcePowerSupplySeatOperationId,
                        unretainReceipt.SourcePowerSupplyRetentionOperationId,
                        unretainReceipt.ExpectedAssemblyRevision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            OperationResult<AssemblyOperationReceipt> delayedRemoveReplay =
                hasRemoveReceipt
                    ? session.RemovePowerSupply(
                        removeReceipt.OperationId,
                        removeReceipt.SourcePowerSupplySeatOperationId,
                        removeReceipt.ExpectedAssemblyRevision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            bool replay = immediateReplay &&
                          delayedSeatReplay.IsSuccess &&
                          delayedRetainReplay.IsSuccess &&
                          delayedUnretainReplay.IsSuccess &&
                          delayedRemoveReplay.IsSuccess &&
                          ReferenceEquals(delayedSeatReplay.Value, seatReceipt) &&
                          ReferenceEquals(delayedRetainReplay.Value, retainReceipt) &&
                          ReferenceEquals(
                              delayedUnretainReplay.Value,
                              unretainReceipt) &&
                          ReferenceEquals(delayedRemoveReplay.Value, removeReceipt) &&
                          session.AssemblyBuild.Revision == replayAssemblyRevision &&
                          session.Inventory.Revision == replayInventoryRevision &&
                          session.AssemblyBuild.ReceiptCount == replayReceiptCount;

            OperationResult recovery = removedToHands
                ? playerCarry.TryRecoverHeldItem()
                : OperationResult.Fail(Failure.FromCode("smoke.remove-mismatch"));
            AssemblyBuildSnapshot finalAssembly =
                session.AssemblyBuild.GetSnapshot();
            bool authorityIsolated =
                finalAssembly.MotherboardSeatState ==
                    initialAssembly.MotherboardSeatState &&
                finalAssembly.ProcessorSocketState ==
                    initialAssembly.ProcessorSocketState &&
                finalAssembly.MemorySlotState == initialAssembly.MemorySlotState &&
                finalAssembly.StorageSlotState == initialAssembly.StorageSlotState &&
                finalAssembly.ProcessorCoolerSlotState ==
                    initialAssembly.ProcessorCoolerSlotState &&
                finalAssembly.GraphicsCardSlotState ==
                    initialAssembly.GraphicsCardSlotState &&
                finalAssembly.PowerSupplyBayState == PowerSupplyBayState.EmptyOpen &&
                removed.PowerSupplyBayState == PowerSupplyBayState.EmptyOpen &&
                session.Orders.Revision == orderRevision &&
                session.RetailOffers.Revision == offerRevision &&
                session.RetailBaskets.Revision == basketRevision &&
                session.RetailCheckouts.Revision == checkoutRevision &&
                session.CheckoutSettlements.Revision == settlementRevision &&
                session.CustomerVisits.Revision == visitRevision &&
                session.CustomerConsultations.Revision == consultationRevision &&
                session.CustomerOfferActions.Revision == actionRevision &&
                session.Inventory.SerializedItemCount == 7 &&
                initialInventoryRevision < session.Inventory.Revision;
            bool identity = stableItemId == powerSupply.ItemIdValue &&
                            powerSupplyBinding.InventoryItemIdValue == stableItemId &&
                            CountCanonicalPowerSupplyProjections(stableItemId) == 1;
            bool recovered = recovery.IsSuccess &&
                             !playerCarry.IsCarrying &&
                             powerSupply.transform.parent == initialParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     powerSupply.transform.position,
                                     powerSupply.transform.rotation),
                                 initialPose) &&
                             powerSupply.Body == body &&
                             powerSupplyBinding.IsAuthorityLooseWorld &&
                             session.ValidateInvariants().IsSuccess;

            if (!retainedOk ||
                !duplicateSeatBlocked ||
                !retainedRemoveGate ||
                !alternateOrder ||
                !replay ||
                !authorityIsolated ||
                !identity ||
                !recovered)
            {
                LogPowerSupplySmokeFailure("smoke.final-invariant-mismatch");
                yield break;
            }

            Debug.Log(PowerSupplySmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToPowerSupplyBay()
        {
            Vector3 target = powerSupplyBay.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.75f, 0.05f, 3.30f);
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

        private static int CountCanonicalPowerSupplyProjections(
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

        private static StableId<AssemblyOperationIdScope>
            PowerSupplySmokeOperationId(string suffix)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.runtime-smoke.power-supply-{suffix}");
        }

        private static StableId<AssemblyOperationIdScope>
            PowerSupplyPrototypeOperationId(string action, long resultingRevision)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.power-supply-{action}." +
                $"r{resultingRevision:000000}");
        }

        private static void LogPowerSupplySmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_PSU_RUNTIME_SMOKE psu-flow=failed code={code}");
        }
    }
}
