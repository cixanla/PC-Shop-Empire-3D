using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicalItemProjection))]
    public sealed class PowerSupplyAssemblyItemBinding : MonoBehaviour
    {
        private enum CarryOrigin
        {
            None = 0,
            LooseWorld = 1,
            Seated = 2
        }

        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection physicalItem;
        [SerializeField] private PowerSupplyBayProjection slot;
        [SerializeField] private PowerSupplyBuildKitProjection buildKit;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.PowerSupplyItemInstanceIdValue;

        private CarryOrigin _carryOrigin;
        private Pose _initialLoosePose;
        private bool _hasInitialLoosePose;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public PowerSupplyBayProjection Slot => slot;

        public PowerSupplyBuildKitProjection BuildKit => buildKit;

        public string InventoryItemIdValue => inventoryItemId;

        public GarageStockFlowSession Session => runtime != null
            ? runtime.EnsureInitialized()
            : null;

        public bool IsSeated
        {
            get
            {
                GarageStockFlowSession session = Session;
                if (session == null)
                {
                    return false;
                }

                PowerSupplyBayState state =
                    session.AssemblyBuild.PowerSupplyBayState;
                return (state == PowerSupplyBayState.PowerSupplySeatedUnsecured ||
                        state == PowerSupplyBayState.PowerSupplyRetained) &&
                       session.AssemblyBuild.PowerSupplyItemId ==
                           session.PowerSupplyItemId;
            }
        }

        public bool IsRetained
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.PowerSupplyBayState ==
                           PowerSupplyBayState.PowerSupplyRetained &&
                       session.AssemblyBuild.PowerSupplyItemId ==
                           session.PowerSupplyItemId;
            }
        }

        public bool IsAuthorityInHands => IsInContainer(
            Session?.HandsContainerId ?? default);

        public bool IsAuthorityLooseWorld => IsInContainer(
            Session?.WorldFloorContainerId ?? default);

        public bool IsAuthorityInBuildKit => IsInContainer(
            Session?.PowerSupplyBuildKitContainerId ?? default);

        public PowerSupplyFormFactor FormFactor => PowerSupplyFormFactor.AtxPs2;

        public bool HasChassisClearance => true;

        public bool HasCableClearance => true;

        private void Awake()
        {
            physicalItem ??= GetComponent<PhysicalItemProjection>();
            CaptureInitialLoosePose();
        }

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            PowerSupplyBayProjection slotProjection,
            string stableInventoryItemId,
            PowerSupplyBuildKitProjection buildKitProjection = null)
        {
            runtime = stockFlowRuntime != null
                ? stockFlowRuntime
                : throw new ArgumentNullException(nameof(stockFlowRuntime));
            physicalItem = itemProjection != null
                ? itemProjection
                : throw new ArgumentNullException(nameof(itemProjection));
            slot = slotProjection != null
                ? slotProjection
                : throw new ArgumentNullException(nameof(slotProjection));
            buildKit = buildKitProjection;
            inventoryItemId = StableId<ItemInstanceIdScope>.Parse(
                stableInventoryItemId).Value;
            if (inventoryItemId !=
                GarageStockFlowSession.PowerSupplyItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype power-supply binding must use the canonical assembly identity.",
                    nameof(stableInventoryItemId));
            }

            CaptureInitialLoosePose();
            SyncProjectionToAuthority();
        }

        public bool MatchesBuildKitConfiguration(
            PowerSupplyBuildKitProjection buildKitProjection)
        {
            return buildKitProjection != null &&
                   buildKit == buildKitProjection &&
                   buildKit.Runtime == runtime;
        }

        public OperationResult TryCommitLoosePickup()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || IsSeated || !IsAuthorityLooseWorld)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLoosePowerSupplyToHands();
            if (transfer.IsSuccess)
            {
                _carryOrigin = CarryOrigin.LooseWorld;
            }

            return transfer;
        }

        public OperationResult TryCommitSeatedDetach()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsSeated)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.detach-authority-mismatch"));
            }

            if (IsRetained)
            {
                return OperationResult.Fail(AssemblyFailures.PowerSupplyRetained);
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> remove =
                Session.RemovePowerSupply(
                    CreateOperationId("remove"),
                    snapshot.PowerSupplySeatedByOperationId,
                    snapshot.Revision);
            if (remove.IsSuccess)
            {
                _carryOrigin = CarryOrigin.Seated;
                SyncProjectionToAuthority();
            }

            return remove.IsSuccess
                ? OperationResult.Success()
                : OperationResult.Fail(remove.Error);
        }

        public OperationResult TryAttachAt(
            Pose exactSeatPose,
            PowerSupplySeatOrientation orientation,
            Transform carryAnchor,
            int heldLayer)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsSeated)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.attach-authority-mismatch"));
            }

            if (!slot.LastEvaluation.CanSeat ||
                slot.LastEvaluation.Orientation != orientation ||
                !ApproximatelySamePose(exactSeatPose, slot.LastEvaluation.Pose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.preview-commit-pose-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom();
            if (headroom.IsFailure)
            {
                return headroom;
            }

            // Commit the reversible physical projection first. If the domain rejects the
            // seat, returning to carry preserves the same physical power-supply instance.
            var previousSafePose = new Pose(
                physicalItem.LastSafePosition,
                physicalItem.LastSafeRotation);
            OperationResult physicalCommit = physicalItem.PlaceAt(exactSeatPose);
            if (physicalCommit.IsFailure)
            {
                return physicalCommit;
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> seat =
                Session.SeatPowerSupply(
                    CreateOperationId("seat"),
                    ToMountOrientation(orientation),
                    snapshot.Revision);
            if (seat.IsFailure)
            {
                OperationResult safePoseRestore =
                    physicalItem.RestoreLastSafePoseSnapshot(previousSafePose);
                if (safePoseRestore.IsFailure)
                {
                    return OperationResult.Fail(
                        Failure.FromCode("assembly-power-supply.safe-pose-rollback-failed"));
                }

                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                if (rollback.IsFailure)
                {
                    OperationResult physicalRecovery =
                        physicalItem.RecoverToLastSafePose();
                    OperationResult authorityRecovery = physicalRecovery.IsSuccess
                        ? Session.DropHeldPowerSupplyToWorld()
                        : OperationResult.Fail(
                            Failure.FromCode("assembly-power-supply.recovery-unavailable"));
                    _carryOrigin = CarryOrigin.None;
                    return physicalRecovery.IsFailure || authorityRecovery.IsFailure
                        ? OperationResult.Fail(
                            Failure.FromCode(
                                "assembly-power-supply.physical-rollback-compensation-failed"))
                        : OperationResult.Fail(
                            Failure.FromCode("assembly-power-supply.physical-rollback-failed"));
                }

                return OperationResult.Fail(seat.Error);
            }

            _carryOrigin = CarryOrigin.None;
            slot.ResetFeedback();
            SyncProjectionToAuthority();
            return OperationResult.Success();
        }

        public OperationResult TryOperateRetention()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> operation;
            if (snapshot.PowerSupplyBayState ==
                PowerSupplyBayState.PowerSupplySeatedUnsecured)
            {
                operation = Session.RetainPowerSupply(
                    CreateOperationId("retain-four-screw"),
                    snapshot.PowerSupplySeatedByOperationId,
                    snapshot.Revision);
            }
            else if (snapshot.PowerSupplyBayState ==
                PowerSupplyBayState.PowerSupplyRetained)
            {
                operation = Session.UnretainPowerSupply(
                    CreateOperationId("unretain-four-screw"),
                    snapshot.PowerSupplySeatedByOperationId,
                    snapshot.PowerSupplyRetainedByOperationId,
                    snapshot.Revision);
            }
            else
            {
                return OperationResult.Fail(AssemblyFailures.ComponentNotSeated);
            }

            if (operation.IsFailure)
            {
                return OperationResult.Fail(operation.Error);
            }

            SyncProjectionToAuthority();
            return OperationResult.Success();
        }

        internal OperationResult TryPlaceInBuildKit(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform carryAnchor,
            LayerMask obstructionMask,
            int clockwiseHalfTurns,
            bool paused)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (buildKit == null || !buildKit.IsConfigured ||
                buildKit.Runtime != runtime)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.context-missing"));
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsSeated ||
                buildKit.IsStaged)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.authority-mismatch"));
            }

            if (carryAnchor == null || Session.CustomPcBuildKit == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        carryAnchor == null
                            ? "custom-pc-power-supply-build-kit.carry-anchor-missing"
                            : "custom-pc-power-supply-build-kit.context-missing"));
            }

            if (Session.CustomPcBuildKit.Revision == long.MaxValue ||
                Session.Inventory.Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            long expectedBuildKitRevision = Session.CustomPcBuildKit.Revision;
            long expectedInventoryRevision = Session.Inventory.Revision;
            PowerSupplyBuildKitEvaluation evaluation = buildKit.Evaluate(
                interactionOrigin,
                playerRoot,
                physicalItem,
                obstructionMask,
                clockwiseHalfTurns,
                paused,
                IsAuthorityInHands && !IsSeated && buildKit.HasPickupReceipt);
            if (!evaluation.IsValid)
            {
                return OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode));
            }

            OperationResult<CustomPcBuildKitReceipt> domain =
                Session.PlaceHeldPowerSupplyInCustomPcBuildKit(
                    expectedBuildKitRevision,
                    expectedInventoryRevision);
            if (domain.IsFailure)
            {
                return OperationResult.Fail(domain.Error);
            }

            OperationResult physicalCommit = physicalItem.PlaceAt(evaluation.Pose);
            if (physicalCommit.IsFailure)
            {
                OperationResult recovery =
                    physicalItem.RecoverToStablePlacementAfterAuthority(
                        evaluation.Pose);
                if (recovery.IsFailure ||
                    !physicalItem.IsStablePlacement ||
                    physicalItem.Ownership != PhysicalItemOwnership.World)
                {
                    return OperationResult.Fail(
                        Failure.FromCode(
                            "custom-pc-power-supply-build-kit.projection-recovery-failed"));
                }
            }

            _carryOrigin = CarryOrigin.None;
            buildKit.ResetFeedback();
            buildKit.RefreshPresentation();
            return OperationResult.Success();
        }

        public OperationResult TryDropToWorld(Pose worldPose)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.drop-authority-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom(
                requiresAssemblyRevision: false);
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult transfer = Session.DropHeldPowerSupplyToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation =
                    Session.PickupLoosePowerSupplyToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-power-supply.drop-compensation-failed"))
                    : physicalDrop;
            }

            _carryOrigin = CarryOrigin.None;
            return OperationResult.Success();
        }

        public OperationResult TryRecoverHeld(Transform carryAnchor, int heldLayer)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (IsSeated)
            {
                OperationResult<Pose> authoritativeSeatPose = slot.ResolveSeatPose(
                    ToHalfTurns(Session.AssemblyBuild.PowerSupplyMountOrientation));
                if (authoritativeSeatPose.IsFailure)
                {
                    return OperationResult.Fail(authoritativeSeatPose.Error);
                }

                OperationResult seatedRecovery =
                    physicalItem.SynchronizeStableWorldPose(
                        authoritativeSeatPose.Value);
                if (seatedRecovery.IsSuccess)
                {
                    _carryOrigin = CarryOrigin.None;
                    SyncProjectionToAuthority();
                }

                return seatedRecovery;
            }

            if (!IsAuthorityInHands)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.recovery-authority-mismatch"));
            }

            OperationResult physicalRecovery = physicalItem.RecoverToLastSafePose();
            if (physicalRecovery.IsFailure)
            {
                return physicalRecovery;
            }

            OperationResult transfer = Session.DropHeldPowerSupplyToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-power-supply.recovery-rollback-failed"))
                    : transfer;
            }

            if (_carryOrigin == CarryOrigin.Seated && _hasInitialLoosePose)
            {
                physicalItem.transform.SetPositionAndRotation(
                    _initialLoosePose.position,
                    _initialLoosePose.rotation);
                if (physicalItem.Body != null)
                {
                    physicalItem.Body.position = _initialLoosePose.position;
                    physicalItem.Body.rotation = _initialLoosePose.rotation;
                }

                Physics.SyncTransforms();
                physicalItem.RecordSafePose();
            }

            _carryOrigin = CarryOrigin.None;
            SyncProjectionToAuthority();
            return OperationResult.Success();
        }

        public OperationResult SyncProjectionToAuthority()
        {
            if (slot == null || Session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.context-missing"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            slot.ApplyAuthoritativeState(
                ToProjectionState(snapshot.PowerSupplyBayState));
            buildKit?.RefreshPresentation();
            return OperationResult.Success();
        }

        public OperationResult ValidateProjectionInvariant()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            OperationResult domain = Session.ValidateInvariants();
            if (domain.IsFailure)
            {
                return domain;
            }

            if (physicalItem.ItemIdValue != inventoryItemId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.identity-mismatch"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<Pose> authoritativeSeatPose = slot.ResolveSeatPose(
                ToHalfTurns(snapshot.PowerSupplyMountOrientation));
            bool physicalMatches = IsSeated
                ? physicalItem.Ownership == PhysicalItemOwnership.World &&
                  physicalItem.IsStablePlacement &&
                  authoritativeSeatPose.IsSuccess &&
                  ApproximatelySamePose(
                      new Pose(
                          physicalItem.transform.position,
                          physicalItem.transform.rotation),
                      authoritativeSeatPose.Value)
                : IsAuthorityInHands
                    ? physicalItem.IsCarried
                    : IsAuthorityInBuildKit
                        ? buildKit != null &&
                          buildKit.IsStaged &&
                          buildKit.MatchesCommittedPlacement(physicalItem)
                        : IsAuthorityLooseWorld &&
                          physicalItem.Ownership == PhysicalItemOwnership.World;
            bool slotMatches =
                slot.SlotIdValue == Session.PowerSupplyBaySlotId.Value &&
                slot.RearMountIdValue == Session.PowerSupplyRearMountId.Value &&
                slot.TopLeftFastenerIdValue ==
                    Session.PowerSupplyTopLeftFastenerId.Value &&
                slot.TopRightFastenerIdValue ==
                    Session.PowerSupplyTopRightFastenerId.Value &&
                slot.BottomLeftFastenerIdValue ==
                    Session.PowerSupplyBottomLeftFastenerId.Value &&
                slot.BottomRightFastenerIdValue ==
                    Session.PowerSupplyBottomRightFastenerId.Value &&
                slot.MatchesLogicalAuthorityState(
                    ToProjectionState(snapshot.PowerSupplyBayState));
            return physicalMatches && slotMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.projection-invariant"));
        }

        private void CaptureInitialLoosePose()
        {
            if (_hasInitialLoosePose || physicalItem == null)
            {
                return;
            }

            _initialLoosePose = new Pose(
                physicalItem.transform.position,
                physicalItem.transform.rotation);
            _hasInitialLoosePose = true;
        }

        private OperationResult ValidateContext()
        {
            return runtime == null ||
                   physicalItem == null ||
                   slot == null ||
                   !slot.IsConfigured ||
                   Session == null ||
                   !Session.AssemblyBuild.HasPowerSupplyBay ||
                   inventoryItemId !=
                       GarageStockFlowSession.PowerSupplyItemInstanceIdValue ||
                   slot.SlotIdValue !=
                       GarageStockFlowSession.PowerSupplyBaySlotIdValue ||
                   slot.RearMountIdValue !=
                       GarageStockFlowSession.PowerSupplyRearMountIdValue ||
                   slot.TopLeftFastenerIdValue !=
                       GarageStockFlowSession.PowerSupplyTopLeftFastenerIdValue ||
                   slot.TopRightFastenerIdValue !=
                       GarageStockFlowSession.PowerSupplyTopRightFastenerIdValue ||
                   slot.BottomLeftFastenerIdValue !=
                       GarageStockFlowSession.PowerSupplyBottomLeftFastenerIdValue ||
                   slot.BottomRightFastenerIdValue !=
                       GarageStockFlowSession.PowerSupplyBottomRightFastenerIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId
                ? OperationResult.Fail(Failure.FromCode(
                    physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                        ? "assembly-power-supply.identity-mismatch"
                        : "assembly-power-supply.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetPowerSupplyItem(out InventoryItemRecord item) &&
                   item.Id == session.PowerSupplyItemId &&
                   item.ProductId == session.PowerSupplyProductId &&
                   item.ContainerId == containerId;
        }

        private OperationResult ValidateCompensationHeadroom(
            bool requiresAssemblyRevision = true)
        {
            GarageStockFlowSession session = Session;
            if (session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.context-missing"));
            }

            if (requiresAssemblyRevision &&
                session.AssemblyBuild.Revision > long.MaxValue - 1L)
            {
                return OperationResult.Fail(AssemblyFailures.RevisionOverflow);
            }

            return session.Inventory.Revision > long.MaxValue - 2L
                ? OperationResult.Fail(AssemblyFailures.InventoryRevisionOverflow)
                : OperationResult.Success();
        }

        private StableId<AssemblyOperationIdScope> CreateOperationId(string action)
        {
            long nextRevision = Session.AssemblyBuild.Revision + 1L;
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.power-supply-{action}.r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }

        private static PowerSupplyMountOrientation ToMountOrientation(
            PowerSupplySeatOrientation orientation)
        {
            return orientation == PowerSupplySeatOrientation.FanToFilteredVent
                ? PowerSupplyMountOrientation.FanToFilteredVent
                : PowerSupplyMountOrientation.FanAwayFromFilteredVent;
        }

        private static int ToHalfTurns(PowerSupplyMountOrientation orientation)
        {
            return orientation == PowerSupplyMountOrientation.FanToFilteredVent ? 0 : 1;
        }

        private static PowerSupplyBayProjectionState ToProjectionState(
            PowerSupplyBayState state)
        {
            return state switch
            {
                PowerSupplyBayState.EmptyOpen =>
                    PowerSupplyBayProjectionState.EmptyOpen,
                PowerSupplyBayState.PowerSupplySeatedUnsecured =>
                    PowerSupplyBayProjectionState.PowerSupplySeatedUnsecured,
                PowerSupplyBayState.PowerSupplyRetained =>
                    PowerSupplyBayProjectionState.PowerSupplyRetained,
                _ => PowerSupplyBayProjectionState.Unsupported
            };
        }
    }
}
