using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PhysicalItemProjection))]
    public sealed class GraphicsCardAssemblyItemBinding : MonoBehaviour
    {
        private enum CarryOrigin
        {
            None = 0,
            LooseWorld = 1,
            Seated = 2
        }

        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection physicalItem;
        [SerializeField] private GraphicsCardSlotProjection slot;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.GraphicsCardAssemblyItemInstanceIdValue;

        private CarryOrigin _carryOrigin;
        private Pose _initialLoosePose;
        private bool _hasInitialLoosePose;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public GraphicsCardSlotProjection Slot => slot;

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

                GraphicsCardSlotState state =
                    session.AssemblyBuild.GraphicsCardSlotState;
                return (state == GraphicsCardSlotState.GraphicsCardSeatedUnsecured ||
                        state == GraphicsCardSlotState.GraphicsCardRetained) &&
                       session.AssemblyBuild.GraphicsCardItemId ==
                           session.GraphicsCardAssemblyItemId;
            }
        }

        public bool IsRetained
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.GraphicsCardSlotState ==
                           GraphicsCardSlotState.GraphicsCardRetained &&
                       session.AssemblyBuild.GraphicsCardItemId ==
                           session.GraphicsCardAssemblyItemId;
            }
        }

        public bool IsHostReady
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.MotherboardSeatState ==
                           AssemblySeatState.SeatedSecured;
            }
        }

        public bool IsAuthorityInHands => IsInContainer(
            Session?.HandsContainerId ?? default);

        public bool IsAuthorityLooseWorld => IsInContainer(
            Session?.WorldFloorContainerId ?? default);

        public GraphicsCardPcieInterface CardInterface =>
            GraphicsCardPcieInterface.PcieX16;

        public bool HasChassisClearance => true;

        public bool HasCoolerClearance => true;

        private void Awake()
        {
            physicalItem ??= GetComponent<PhysicalItemProjection>();
            CaptureInitialLoosePose();
        }

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            GraphicsCardSlotProjection slotProjection,
            string stableInventoryItemId)
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
            inventoryItemId = StableId<ItemInstanceIdScope>.Parse(
                stableInventoryItemId).Value;
            if (inventoryItemId !=
                GarageStockFlowSession.GraphicsCardAssemblyItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype graphics-card binding must use the canonical assembly identity.",
                    nameof(stableInventoryItemId));
            }

            CaptureInitialLoosePose();
            SyncProjectionToAuthority();
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
                    Failure.FromCode("assembly-graphics-card.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLooseGraphicsCardToHands();
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
                    Failure.FromCode("assembly-graphics-card.detach-authority-mismatch"));
            }

            if (IsRetained)
            {
                return OperationResult.Fail(AssemblyFailures.GraphicsCardRetained);
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> remove =
                Session.RemoveGraphicsCard(
                    CreateOperationId("remove"),
                    snapshot.GraphicsCardSeatedByOperationId,
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
            GraphicsCardSeatOrientation orientation,
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
                    Failure.FromCode("assembly-graphics-card.attach-authority-mismatch"));
            }

            if (!slot.LastEvaluation.CanSeat ||
                slot.LastEvaluation.Orientation != orientation ||
                !ApproximatelySamePose(exactSeatPose, slot.LastEvaluation.Pose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.preview-commit-pose-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom();
            if (headroom.IsFailure)
            {
                return headroom;
            }

            // Commit the reversible physical projection first. If the domain rejects the
            // seat, returning to carry preserves the same physical graphics-card instance.
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
                Session.SeatGraphicsCard(
                    CreateOperationId("seat"),
                    ToMountOrientation(orientation),
                    snapshot.InstalledByOperationId,
                    snapshot.SecuredByOperationId,
                    snapshot.Revision);
            if (seat.IsFailure)
            {
                OperationResult safePoseRestore =
                    physicalItem.RestoreLastSafePoseSnapshot(previousSafePose);
                if (safePoseRestore.IsFailure)
                {
                    return OperationResult.Fail(
                        Failure.FromCode("assembly-graphics-card.safe-pose-rollback-failed"));
                }

                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                if (rollback.IsFailure)
                {
                    OperationResult physicalRecovery =
                        physicalItem.RecoverToLastSafePose();
                    OperationResult authorityRecovery = physicalRecovery.IsSuccess
                        ? Session.DropHeldGraphicsCardToWorld()
                        : OperationResult.Fail(
                            Failure.FromCode("assembly-graphics-card.recovery-unavailable"));
                    _carryOrigin = CarryOrigin.None;
                    return physicalRecovery.IsFailure || authorityRecovery.IsFailure
                        ? OperationResult.Fail(
                            Failure.FromCode(
                                "assembly-graphics-card.physical-rollback-compensation-failed"))
                        : OperationResult.Fail(
                            Failure.FromCode("assembly-graphics-card.physical-rollback-failed"));
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
            if (snapshot.GraphicsCardSlotState ==
                GraphicsCardSlotState.GraphicsCardSeatedUnsecured)
            {
                operation = Session.RetainGraphicsCard(
                    CreateOperationId("retain-latch-bracket"),
                    snapshot.GraphicsCardSeatedByOperationId,
                    snapshot.Revision);
            }
            else if (snapshot.GraphicsCardSlotState ==
                GraphicsCardSlotState.GraphicsCardRetained)
            {
                operation = Session.UnretainGraphicsCard(
                    CreateOperationId("unretain-bracket-latch"),
                    snapshot.GraphicsCardSeatedByOperationId,
                    snapshot.GraphicsCardRetainedByOperationId,
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
                    Failure.FromCode("assembly-graphics-card.drop-authority-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom(
                requiresAssemblyRevision: false);
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult transfer = Session.DropHeldGraphicsCardToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation =
                    Session.PickupLooseGraphicsCardToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-graphics-card.drop-compensation-failed"))
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
                    ToHalfTurns(Session.AssemblyBuild.GraphicsCardMountOrientation));
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
                    Failure.FromCode("assembly-graphics-card.recovery-authority-mismatch"));
            }

            OperationResult physicalRecovery = physicalItem.RecoverToLastSafePose();
            if (physicalRecovery.IsFailure)
            {
                return physicalRecovery;
            }

            OperationResult transfer = Session.DropHeldGraphicsCardToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-graphics-card.recovery-rollback-failed"))
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
                    Failure.FromCode("assembly-graphics-card.context-missing"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            slot.ApplyAuthoritativeState(
                snapshot.MotherboardSeatState == AssemblySeatState.SeatedSecured,
                ToProjectionState(snapshot.GraphicsCardSlotState));
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
                    Failure.FromCode("assembly-graphics-card.identity-mismatch"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<Pose> authoritativeSeatPose = slot.ResolveSeatPose(
                ToHalfTurns(snapshot.GraphicsCardMountOrientation));
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
                    : IsAuthorityLooseWorld &&
                      physicalItem.Ownership == PhysicalItemOwnership.World;
            bool slotMatches =
                slot.SlotIdValue == Session.GraphicsCardSlotId.Value &&
                slot.LatchIdValue == Session.GraphicsCardLatchId.Value &&
                slot.RearBracketIdValue ==
                    GarageStockFlowSession.GraphicsCardRearBracketIdValue &&
                slot.RearBracketFastenerIdValue ==
                    Session.GraphicsCardBracketFastenerId.Value &&
                slot.MatchesLogicalAuthorityState(
                    snapshot.MotherboardSeatState == AssemblySeatState.SeatedSecured,
                    ToProjectionState(snapshot.GraphicsCardSlotState));
            return physicalMatches && slotMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.projection-invariant"));
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
                   !Session.AssemblyBuild.HasGraphicsCardSlot ||
                   inventoryItemId !=
                       GarageStockFlowSession.GraphicsCardAssemblyItemInstanceIdValue ||
                   slot.SlotIdValue !=
                       GarageStockFlowSession.GraphicsCardSlotIdValue ||
                   slot.LatchIdValue !=
                       GarageStockFlowSession.GraphicsCardLatchIdValue ||
                   slot.RearBracketIdValue !=
                       GarageStockFlowSession.GraphicsCardRearBracketIdValue ||
                   slot.RearBracketFastenerIdValue !=
                       GarageStockFlowSession.GraphicsCardBracketFastenerIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId
                ? OperationResult.Fail(Failure.FromCode(
                    physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                        ? "assembly-graphics-card.identity-mismatch"
                        : "assembly-graphics-card.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetGraphicsCardAssemblyItem(out InventoryItemRecord item) &&
                   item.Id == session.GraphicsCardAssemblyItemId &&
                   item.ProductId == session.ProductId &&
                   item.ContainerId == containerId;
        }

        private OperationResult ValidateCompensationHeadroom(
            bool requiresAssemblyRevision = true)
        {
            GarageStockFlowSession session = Session;
            if (session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.context-missing"));
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
                $"assembly.operation.prototype-001.graphics-card-{action}.r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }

        private static GraphicsCardMountOrientation ToMountOrientation(
            GraphicsCardSeatOrientation orientation)
        {
            return orientation == GraphicsCardSeatOrientation.Primary
                ? GraphicsCardMountOrientation.Primary
                : GraphicsCardMountOrientation.Rotated180;
        }

        private static int ToHalfTurns(GraphicsCardMountOrientation orientation)
        {
            return orientation == GraphicsCardMountOrientation.Primary ? 0 : 1;
        }

        private static GraphicsCardSlotProjectionState ToProjectionState(
            GraphicsCardSlotState state)
        {
            return state switch
            {
                GraphicsCardSlotState.EmptyOpen =>
                    GraphicsCardSlotProjectionState.EmptyOpen,
                GraphicsCardSlotState.GraphicsCardSeatedUnsecured =>
                    GraphicsCardSlotProjectionState.GraphicsCardSeatedUnsecured,
                GraphicsCardSlotState.GraphicsCardRetained =>
                    GraphicsCardSlotProjectionState.GraphicsCardRetained,
                _ => GraphicsCardSlotProjectionState.Unsupported
            };
        }
    }
}
