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
    public sealed class DimmAssemblyItemBinding : MonoBehaviour
    {
        private enum CarryOrigin
        {
            None = 0,
            LooseWorld = 1,
            Seated = 2
        }

        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection physicalItem;
        [SerializeField] private DimmSlotProjection slot;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.MemoryItemInstanceIdValue;

        private CarryOrigin _carryOrigin;
        private Pose _initialLoosePose;
        private bool _hasInitialLoosePose;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public DimmSlotProjection Slot => slot;

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

                MemorySlotState state = session.AssemblyBuild.MemorySlotState;
                return (state == MemorySlotState.MemoryModuleSeatedOpen ||
                        state == MemorySlotState.MemoryModuleRetained) &&
                       session.AssemblyBuild.MemoryItemId == session.MemoryItemId;
            }
        }

        public bool IsRetained
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.MemorySlotState ==
                           MemorySlotState.MemoryModuleRetained &&
                       session.AssemblyBuild.MemoryItemId == session.MemoryItemId;
            }
        }

        public bool IsAuthorityInHands => IsInContainer(
            Session?.HandsContainerId ?? default);

        public bool IsAuthorityLooseWorld => IsInContainer(
            Session?.WorldFloorContainerId ?? default);

        private void Awake()
        {
            physicalItem ??= GetComponent<PhysicalItemProjection>();
            CaptureInitialLoosePose();
        }

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            DimmSlotProjection slotProjection,
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
            if (inventoryItemId != GarageStockFlowSession.MemoryItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype DIMM binding must use the canonical inventory identity.",
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
                    Failure.FromCode("assembly-memory.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLooseMemoryToHands();
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
                    Failure.FromCode("assembly-memory.detach-authority-mismatch"));
            }

            if (IsRetained)
            {
                return OperationResult.Fail(AssemblyFailures.MemoryModuleRetained);
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> remove = Session.RemoveMemoryModule(
                CreateOperationId("remove"),
                snapshot.MemorySeatedByOperationId,
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
            DimmKeyOrientation orientation)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsSeated)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-memory.attach-authority-mismatch"));
            }

            if (!ApproximatelySamePose(exactSeatPose, slot.SnapPose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-memory.preview-commit-pose-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom(
                requiresAssemblyRevision: true);
            if (headroom.IsFailure)
            {
                return headroom;
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> seat = Session.SeatMemoryModule(
                CreateOperationId("seat"),
                orientation,
                snapshot.InstalledByOperationId,
                snapshot.SecuredByOperationId,
                snapshot.Revision);
            if (seat.IsFailure)
            {
                return OperationResult.Fail(seat.Error);
            }

            OperationResult physicalCommit = physicalItem.PlaceAt(exactSeatPose);
            if (physicalCommit.IsFailure)
            {
                OperationResult<AssemblyOperationReceipt> compensation =
                    Session.RemoveMemoryModule(
                        CreateOperationId("seat-compensation"),
                        seat.Value.OperationId,
                        Session.AssemblyBuild.Revision);
                SyncProjectionToAuthority();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-memory.compensation-failed"))
                    : physicalCommit;
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
            if (snapshot.MemorySlotState == MemorySlotState.MemoryModuleSeatedOpen)
            {
                operation = Session.CloseMemoryRetention(
                    CreateOperationId("close-retention"),
                    snapshot.MemorySeatedByOperationId,
                    snapshot.Revision);
            }
            else if (snapshot.MemorySlotState == MemorySlotState.MemoryModuleRetained)
            {
                operation = Session.OpenMemoryRetention(
                    CreateOperationId("open-retention"),
                    snapshot.MemorySeatedByOperationId,
                    snapshot.MemoryRetainedByOperationId,
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
                    Failure.FromCode("assembly-memory.drop-authority-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom(
                requiresAssemblyRevision: false);
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult transfer = Session.DropHeldMemoryToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation = Session.PickupLooseMemoryToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-memory.drop-compensation-failed"))
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

            if (!physicalItem.IsCarried || !IsAuthorityInHands)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-memory.recovery-authority-mismatch"));
            }

            if (_carryOrigin == CarryOrigin.Seated)
            {
                AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
                if (snapshot.MotherboardSeatState == AssemblySeatState.SeatedSecured)
                {
                    OperationResult headroom = ValidateCompensationHeadroom(
                        requiresAssemblyRevision: true);
                    if (headroom.IsFailure)
                    {
                        return headroom;
                    }

                    OperationResult<AssemblyOperationReceipt> reseat =
                        Session.SeatMemoryModule(
                            CreateOperationId("recovery-seat"),
                            DimmKeyOrientation.NotchAligned,
                            snapshot.InstalledByOperationId,
                            snapshot.SecuredByOperationId,
                            snapshot.Revision);
                    if (reseat.IsSuccess)
                    {
                        OperationResult physicalRecovery =
                            physicalItem.RecoverToLastSafePose();
                        if (physicalRecovery.IsFailure)
                        {
                            OperationResult<AssemblyOperationReceipt> compensation =
                                Session.RemoveMemoryModule(
                                    CreateOperationId("recovery-compensation"),
                                    reseat.Value.OperationId,
                                    Session.AssemblyBuild.Revision);
                            return compensation.IsFailure
                                ? OperationResult.Fail(Failure.FromCode(
                                    "assembly-memory.recovery-compensation-failed"))
                                : physicalRecovery;
                        }

                        _carryOrigin = CarryOrigin.None;
                        SyncProjectionToAuthority();
                        return OperationResult.Success();
                    }
                }
            }

            OperationResult looseRecovery = physicalItem.RecoverToLastSafePose();
            if (looseRecovery.IsFailure)
            {
                return looseRecovery;
            }

            OperationResult transfer = Session.DropHeldMemoryToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(Failure.FromCode(
                        "assembly-memory.recovery-rollback-failed"))
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
                    Failure.FromCode("assembly-memory.context-missing"));
            }

            slot.ApplyAuthoritativeState(
                Session.AssemblyBuild.MotherboardSeatState,
                Session.AssemblyBuild.MemorySlotState);
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
                    Failure.FromCode("assembly-memory.identity-mismatch"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            bool physicalMatches = IsSeated
                ? physicalItem.Ownership == PhysicalItemOwnership.World &&
                  physicalItem.IsStablePlacement &&
                  ApproximatelySamePose(
                      new Pose(
                          physicalItem.transform.position,
                          physicalItem.transform.rotation),
                      slot.SnapPose)
                : IsAuthorityInHands
                    ? physicalItem.IsCarried
                    : IsAuthorityLooseWorld &&
                      physicalItem.Ownership == PhysicalItemOwnership.World;
            bool slotMatches = slot.SlotIdValue == Session.MemorySlotId.Value &&
                               slot.RetentionIdValue == Session.MemoryRetentionId.Value &&
                               slot.ChannelIdValue == Session.MemoryChannelId.Value &&
                               slot.BankIdValue == Session.MemoryBankId.Value &&
                               slot.MatchesLogicalAuthorityState(
                                   snapshot.MotherboardSeatState,
                                   snapshot.MemorySlotState);
            return physicalMatches && slotMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-memory.projection-invariant"));
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
                   !Session.AssemblyBuild.HasMemorySlot ||
                   inventoryItemId != GarageStockFlowSession.MemoryItemInstanceIdValue ||
                   slot.SlotIdValue != GarageStockFlowSession.MemorySlotIdValue ||
                   slot.RetentionIdValue != GarageStockFlowSession.MemoryRetentionIdValue ||
                   slot.ChannelIdValue != GarageStockFlowSession.MemoryChannelIdValue ||
                   slot.BankIdValue != GarageStockFlowSession.MemoryBankIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId
                ? OperationResult.Fail(Failure.FromCode(
                    physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                        ? "assembly-memory.identity-mismatch"
                        : "assembly-memory.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetMemoryItem(out InventoryItemRecord item) &&
                   item.Id == session.MemoryItemId &&
                   item.ProductId == session.MemoryProductId &&
                   item.ContainerId == containerId;
        }

        private OperationResult ValidateCompensationHeadroom(
            bool requiresAssemblyRevision)
        {
            GarageStockFlowSession session = Session;
            if (session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-memory.context-missing"));
            }

            if (requiresAssemblyRevision &&
                session.AssemblyBuild.Revision > long.MaxValue - 2L)
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
                $"assembly.operation.prototype-001.memory-{action}.r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }
    }
}
