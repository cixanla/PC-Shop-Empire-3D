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
    public sealed class MotherboardAssemblyItemBinding : MonoBehaviour
    {
        private enum CarryOrigin
        {
            None = 0,
            LooseWorld = 1,
            Seated = 2
        }

        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection physicalItem;
        [SerializeField] private MotherboardSeatProjection seat;
        [SerializeField] private MotherboardFastenerProjection fastener;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.MotherboardItemInstanceIdValue;

        private CarryOrigin _carryOrigin;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public MotherboardSeatProjection Seat => seat;

        public MotherboardFastenerProjection Fastener => fastener;

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

                return session.AssemblyBuild.MotherboardSeatState !=
                           AssemblySeatState.Empty &&
                       session.AssemblyBuild.MotherboardItemId ==
                           session.MotherboardItemId;
            }
        }

        public bool IsSecured
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.MotherboardSeatState ==
                           AssemblySeatState.SeatedSecured &&
                       session.AssemblyBuild.MotherboardItemId ==
                           session.MotherboardItemId;
            }
        }

        public bool IsAuthorityInHands => IsInContainer(
            Session?.HandsContainerId ?? default);

        public bool IsAuthorityLooseWorld => IsInContainer(
            Session?.WorldFloorContainerId ?? default);

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            MotherboardSeatProjection seatProjection,
            MotherboardFastenerProjection fastenerProjection,
            string stableInventoryItemId)
        {
            runtime = stockFlowRuntime != null
                ? stockFlowRuntime
                : throw new ArgumentNullException(nameof(stockFlowRuntime));
            physicalItem = itemProjection != null
                ? itemProjection
                : throw new ArgumentNullException(nameof(itemProjection));
            seat = seatProjection != null
                ? seatProjection
                : throw new ArgumentNullException(nameof(seatProjection));
            fastener = fastenerProjection != null
                ? fastenerProjection
                : throw new ArgumentNullException(nameof(fastenerProjection));
            inventoryItemId = StableId<ItemInstanceIdScope>.Parse(
                stableInventoryItemId).Value;
            if (inventoryItemId != GarageStockFlowSession.MotherboardItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype motherboard binding must use the canonical inventory identity.",
                    nameof(stableInventoryItemId));
            }
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
                    Failure.FromCode("assembly-seat.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLooseMotherboardToHands();
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
                    Failure.FromCode("assembly-seat.detach-authority-mismatch"));
            }

            if (IsSecured)
            {
                return OperationResult.Fail(AssemblyFailures.ComponentSecured);
            }

            OperationResult<AssemblyOperationReceipt> detach =
                Session.DetachMotherboard(CreateOperationId("detach"));
            if (detach.IsSuccess)
            {
                _carryOrigin = CarryOrigin.Seated;
                fastener.ApplyAuthoritativeState(AssemblySeatState.Empty);
            }

            return detach.IsSuccess
                ? OperationResult.Success()
                : OperationResult.Fail(detach.Error);
        }

        public OperationResult TryAttachAt(Pose exactSeatPose)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsSeated)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-seat.attach-authority-mismatch"));
            }

            if (!ApproximatelySamePose(exactSeatPose, seat.SnapPose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-seat.preview-commit-pose-mismatch"));
            }

            OperationResult<AssemblyOperationReceipt> attach =
                Session.AttachMotherboard(CreateOperationId("attach"));
            if (attach.IsFailure)
            {
                return OperationResult.Fail(attach.Error);
            }

            OperationResult physicalCommit = physicalItem.PlaceAt(exactSeatPose);
            if (physicalCommit.IsFailure)
            {
                OperationResult<AssemblyOperationReceipt> compensation =
                    Session.DetachMotherboard(CreateOperationId("attach-compensation"));
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-seat.compensation-failed"))
                    : physicalCommit;
            }

            _carryOrigin = CarryOrigin.None;
            seat.ResetFeedback();
            fastener.ApplyAuthoritativeState(AssemblySeatState.SeatedUnsecured);
            return OperationResult.Success();
        }

        public OperationResult TryOperateFastener()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            GarageStockFlowSession session = Session;
            AssemblyBuildSnapshot snapshot = session.AssemblyBuild.GetSnapshot();
            if (snapshot.MotherboardItemId != session.MotherboardItemId ||
                snapshot.MotherboardFastenerId != session.MotherboardFastenerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-fastener.authority-mismatch"));
            }

            OperationResult<AssemblyOperationReceipt> operation;
            if (snapshot.MotherboardSeatState == AssemblySeatState.SeatedUnsecured)
            {
                operation = session.SecureMotherboardFastener(
                    CreateOperationId("secure-fastener"),
                    snapshot.InstalledByOperationId,
                    snapshot.Revision);
            }
            else if (snapshot.MotherboardSeatState == AssemblySeatState.SeatedSecured)
            {
                operation = session.UnsecureMotherboardFastener(
                    CreateOperationId("unsecure-fastener"),
                    snapshot.InstalledByOperationId,
                    snapshot.SecuredByOperationId,
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

            fastener.ApplyAuthoritativeState(operation.Value.ResultingSeatState);
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
                    Failure.FromCode("assembly-seat.drop-authority-mismatch"));
            }

            OperationResult transfer = Session.DropHeldMotherboardToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation = Session.PickupLooseMotherboardToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-seat.drop-compensation-failed"))
                    : physicalDrop;
            }

            _carryOrigin = CarryOrigin.None;
            return OperationResult.Success();
        }

        public OperationResult TryRecoverHeld(
            Transform carryAnchor,
            int heldLayer)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-seat.recovery-authority-mismatch"));
            }

            if (_carryOrigin == CarryOrigin.Seated)
            {
                OperationResult<AssemblyOperationReceipt> reattach =
                    Session.AttachMotherboard(CreateOperationId("recovery-attach"));
                if (reattach.IsFailure)
                {
                    return OperationResult.Fail(reattach.Error);
                }

                OperationResult physicalRecovery = physicalItem.RecoverToLastSafePose();
                if (physicalRecovery.IsFailure)
                {
                    OperationResult<AssemblyOperationReceipt> compensation =
                        Session.DetachMotherboard(
                            CreateOperationId("recovery-compensation"));
                    return compensation.IsFailure
                        ? OperationResult.Fail(
                            Failure.FromCode("assembly-seat.recovery-compensation-failed"))
                        : physicalRecovery;
                }

                _carryOrigin = CarryOrigin.None;
                fastener.ApplyAuthoritativeState(AssemblySeatState.SeatedUnsecured);
                return OperationResult.Success();
            }

            OperationResult looseRecovery = physicalItem.RecoverToLastSafePose();
            if (looseRecovery.IsFailure)
            {
                return looseRecovery;
            }

            OperationResult transfer = Session.DropHeldMotherboardToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-seat.recovery-rollback-failed"))
                    : transfer;
            }

            _carryOrigin = CarryOrigin.None;
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
                    Failure.FromCode("assembly-seat.identity-mismatch"));
            }

            bool physicalMatches = IsSeated
                ? physicalItem.Ownership == PhysicalItemOwnership.World &&
                  physicalItem.IsStablePlacement &&
                  ApproximatelySamePose(
                      new Pose(physicalItem.transform.position, physicalItem.transform.rotation),
                      seat.SnapPose)
                : IsAuthorityInHands
                    ? physicalItem.IsCarried
                    : IsAuthorityLooseWorld &&
                      physicalItem.Ownership == PhysicalItemOwnership.World;
            bool fastenerMatches = fastener.FastenerIdValue ==
                                   Session.MotherboardFastenerId.Value &&
                                   fastener.MatchesAuthorityState(
                                       Session.AssemblyBuild.MotherboardSeatState);
            return physicalMatches && fastenerMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-seat.projection-invariant"));
        }

        private OperationResult ValidateContext()
        {
            return runtime == null ||
                   physicalItem == null ||
                   seat == null ||
                   fastener == null ||
                   !seat.IsConfigured ||
                   !fastener.IsConfigured ||
                   Session == null ||
                   inventoryItemId != GarageStockFlowSession.MotherboardItemInstanceIdValue ||
                   fastener.FastenerIdValue !=
                       GarageStockFlowSession.MotherboardFastenerIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId
                ? OperationResult.Fail(
                    Failure.FromCode(
                        physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                            ? "assembly-seat.identity-mismatch"
                            : "assembly-seat.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetMotherboardItem(out InventoryItemRecord item) &&
                   item.Id == session.MotherboardItemId &&
                   item.ProductId == session.MotherboardProductId &&
                   item.ContainerId == containerId;
        }

        private StableId<AssemblyOperationIdScope> CreateOperationId(string action)
        {
            long nextRevision = Session.AssemblyBuild.Revision + 1L;
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.{action}.r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }
    }
}
