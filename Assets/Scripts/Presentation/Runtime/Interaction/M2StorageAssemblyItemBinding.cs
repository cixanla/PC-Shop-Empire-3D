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
    public sealed class M2StorageAssemblyItemBinding : MonoBehaviour
    {
        private enum CarryOrigin
        {
            None = 0,
            LooseWorld = 1,
            Seated = 2
        }

        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection physicalItem;
        [SerializeField] private M2StorageSlotProjection slot;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.StorageItemInstanceIdValue;

        private CarryOrigin _carryOrigin;
        private Pose _initialLoosePose;
        private bool _hasInitialLoosePose;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public M2StorageSlotProjection Slot => slot;

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

                StorageSlotState state = session.AssemblyBuild.StorageSlotState;
                return (state == StorageSlotState.StorageDeviceSeatedUnsecured ||
                        state == StorageSlotState.StorageDeviceSecured) &&
                       session.AssemblyBuild.StorageItemId == session.StorageItemId;
            }
        }

        public bool IsSecured
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.StorageSlotState ==
                           StorageSlotState.StorageDeviceSecured &&
                       session.AssemblyBuild.StorageItemId == session.StorageItemId;
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
            M2StorageSlotProjection slotProjection,
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
            if (inventoryItemId != GarageStockFlowSession.StorageItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype M.2 storage binding must use the canonical inventory identity.",
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
                    Failure.FromCode("assembly-storage.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLooseStorageToHands();
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
                    Failure.FromCode("assembly-storage.detach-authority-mismatch"));
            }

            if (IsSecured)
            {
                return OperationResult.Fail(AssemblyFailures.StorageDeviceSecured);
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> remove = Session.RemoveStorageDevice(
                CreateOperationId("remove"),
                snapshot.StorageSeatedByOperationId,
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
            M2KeyOrientation orientation)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsSeated)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-storage.attach-authority-mismatch"));
            }

            if (!slot.LastEvaluation.CanSeat ||
                !ApproximatelySamePose(exactSeatPose, slot.LastEvaluation.GuidedPose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-storage.preview-commit-pose-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom(
                requiresAssemblyRevision: true);
            if (headroom.IsFailure)
            {
                return headroom;
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> seat = Session.SeatStorageDevice(
                CreateOperationId("seat"),
                orientation,
                snapshot.InstalledByOperationId,
                snapshot.SecuredByOperationId,
                snapshot.Revision);
            if (seat.IsFailure)
            {
                return OperationResult.Fail(seat.Error);
            }

            OperationResult physicalCommit = physicalItem.PlaceAt(
                slot.LastEvaluation.SeatedPose);
            if (physicalCommit.IsFailure)
            {
                OperationResult<AssemblyOperationReceipt> compensation =
                    Session.RemoveStorageDevice(
                        CreateOperationId("seat-compensation"),
                        seat.Value.OperationId,
                        Session.AssemblyBuild.Revision);
                SyncProjectionToAuthority();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-storage.compensation-failed"))
                    : physicalCommit;
            }

            _carryOrigin = CarryOrigin.None;
            slot.ResetFeedback();
            SyncProjectionToAuthority();
            return OperationResult.Success();
        }

        public OperationResult TryOperateCaptiveScrew()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> operation;
            if (snapshot.StorageSlotState == StorageSlotState.StorageDeviceSeatedUnsecured)
            {
                operation = Session.SecureStorageDevice(
                    CreateOperationId("close-retention"),
                    snapshot.StorageSeatedByOperationId,
                    snapshot.Revision);
            }
            else if (snapshot.StorageSlotState == StorageSlotState.StorageDeviceSecured)
            {
                operation = Session.UnsecureStorageDevice(
                    CreateOperationId("open-retention"),
                    snapshot.StorageSeatedByOperationId,
                    snapshot.StorageSecuredByOperationId,
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
                    Failure.FromCode("assembly-storage.drop-authority-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom(
                requiresAssemblyRevision: false);
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult transfer = Session.DropHeldStorageToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation = Session.PickupLooseStorageToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-storage.drop-compensation-failed"))
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
                    Failure.FromCode("assembly-storage.recovery-authority-mismatch"));
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
                        Session.SeatStorageDevice(
                            CreateOperationId("recovery-seat"),
                            M2KeyOrientation.KeyAligned,
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
                                Session.RemoveStorageDevice(
                                    CreateOperationId("recovery-compensation"),
                                    reseat.Value.OperationId,
                                    Session.AssemblyBuild.Revision);
                            return compensation.IsFailure
                                ? OperationResult.Fail(Failure.FromCode(
                                    "assembly-storage.recovery-compensation-failed"))
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

            OperationResult transfer = Session.DropHeldStorageToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(Failure.FromCode(
                        "assembly-storage.recovery-rollback-failed"))
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
                    Failure.FromCode("assembly-storage.context-missing"));
            }

            slot.ApplyAuthoritativeState(
                Session.AssemblyBuild.MotherboardSeatState,
                Session.AssemblyBuild.StorageSlotState);
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
                    Failure.FromCode("assembly-storage.identity-mismatch"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            bool physicalMatches = IsSeated
                ? physicalItem.Ownership == PhysicalItemOwnership.World &&
                  physicalItem.IsStablePlacement &&
                  ApproximatelySamePose(
                      new Pose(
                          physicalItem.transform.position,
                          physicalItem.transform.rotation),
                      slot.SeatedPose)
                : IsAuthorityInHands
                    ? physicalItem.IsCarried
                    : IsAuthorityLooseWorld &&
                      physicalItem.Ownership == PhysicalItemOwnership.World;
            bool slotMatches = slot.SlotIdValue == Session.StorageSlotId.Value &&
                               slot.StandoffIdValue == Session.StorageStandoffId.Value &&
                               slot.CaptiveScrewIdValue == Session.StorageCaptiveScrewId.Value &&
                               slot.MatchesLogicalAuthorityState(
                                   snapshot.MotherboardSeatState,
                                   snapshot.StorageSlotState);
            return physicalMatches && slotMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-storage.projection-invariant"));
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
                   !Session.AssemblyBuild.HasStorageSlot ||
                   inventoryItemId != GarageStockFlowSession.StorageItemInstanceIdValue ||
                   slot.SlotIdValue != GarageStockFlowSession.StorageSlotIdValue ||
                   slot.StandoffIdValue != GarageStockFlowSession.StorageStandoffIdValue ||
                   slot.CaptiveScrewIdValue != GarageStockFlowSession.StorageCaptiveScrewIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId
                ? OperationResult.Fail(Failure.FromCode(
                    physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                        ? "assembly-storage.identity-mismatch"
                        : "assembly-storage.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetStorageItem(out InventoryItemRecord item) &&
                   item.Id == session.StorageItemId &&
                   item.ProductId == session.StorageProductId &&
                   item.ContainerId == containerId;
        }

        private OperationResult ValidateCompensationHeadroom(
            bool requiresAssemblyRevision)
        {
            GarageStockFlowSession session = Session;
            if (session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-storage.context-missing"));
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
                $"assembly.operation.prototype-001.storage-{action}.r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }
    }
}
