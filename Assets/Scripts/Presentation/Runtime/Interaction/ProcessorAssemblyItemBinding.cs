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
    public sealed class ProcessorAssemblyItemBinding : MonoBehaviour
    {
        private enum CarryOrigin
        {
            None = 0,
            LooseWorld = 1,
            Seated = 2,
            BuildKit = 3
        }

        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection physicalItem;
        [SerializeField] private ProcessorSocketProjection socket;
        [SerializeField] private ProcessorBuildKitProjection buildKit;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.ProcessorItemInstanceIdValue;

        private CarryOrigin _carryOrigin;
        private Pose _initialLoosePose;
        private bool _hasInitialLoosePose;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public ProcessorSocketProjection Socket => socket;

        public ProcessorBuildKitProjection BuildKit => buildKit;

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

                ProcessorSocketState state = session.AssemblyBuild.ProcessorSocketState;
                return (state == ProcessorSocketState.ProcessorSeatedOpen ||
                        state == ProcessorSocketState.ProcessorRetained) &&
                       session.AssemblyBuild.ProcessorItemId == session.ProcessorItemId;
            }
        }

        public bool IsRetained
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.ProcessorSocketState ==
                           ProcessorSocketState.ProcessorRetained &&
                       session.AssemblyBuild.ProcessorItemId == session.ProcessorItemId;
            }
        }

        public bool IsAuthorityInHands => IsInContainer(
            Session?.HandsContainerId ?? default);

        public bool IsAuthorityLooseWorld => IsInContainer(
            Session?.WorldFloorContainerId ?? default);

        public bool IsAuthorityInBuildKit => IsInContainer(
            Session?.ProcessorBuildKitContainerId ?? default);

        private void Awake()
        {
            physicalItem ??= GetComponent<PhysicalItemProjection>();
            CaptureInitialLoosePose();
        }

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            ProcessorSocketProjection socketProjection,
            string stableInventoryItemId,
            ProcessorBuildKitProjection buildKitProjection = null)
        {
            runtime = stockFlowRuntime != null
                ? stockFlowRuntime
                : throw new ArgumentNullException(nameof(stockFlowRuntime));
            physicalItem = itemProjection != null
                ? itemProjection
                : throw new ArgumentNullException(nameof(itemProjection));
            socket = socketProjection != null
                ? socketProjection
                : throw new ArgumentNullException(nameof(socketProjection));
            buildKit = buildKitProjection;
            inventoryItemId = StableId<ItemInstanceIdScope>.Parse(
                stableInventoryItemId).Value;
            if (inventoryItemId != GarageStockFlowSession.ProcessorItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype processor binding must use the canonical inventory identity.",
                    nameof(stableInventoryItemId));
            }

            CaptureInitialLoosePose();
            SyncProjectionToAuthority();
        }

        public bool MatchesBuildKitConfiguration(
            ProcessorBuildKitProjection buildKitProjection)
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

            if (physicalItem.Ownership != PhysicalItemOwnership.World ||
                physicalItem.IsCarried ||
                IsSeated ||
                !IsAuthorityLooseWorld)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-processor.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLooseProcessorToHands();
            if (transfer.IsSuccess)
            {
                _carryOrigin = CarryOrigin.LooseWorld;
            }

            return transfer;
        }

        public OperationResult TryCommitBuildKitAssemblyPickup()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (physicalItem.Ownership != PhysicalItemOwnership.World ||
                physicalItem.IsCarried ||
                IsSeated ||
                !IsAuthorityInBuildKit ||
                buildKit == null ||
                !buildKit.IsStaged)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-assembly.pickup-authority-mismatch"));
            }

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                Session.PickupStagedProcessorForAssembly();
            if (handoff.IsSuccess)
            {
                _carryOrigin = CarryOrigin.BuildKit;
                buildKit.RefreshPresentation();
            }

            return handoff.IsSuccess
                ? OperationResult.Success()
                : OperationResult.Fail(handoff.Error);
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
                    Failure.FromCode("assembly-processor.detach-authority-mismatch"));
            }

            if (IsRetained)
            {
                return OperationResult.Fail(AssemblyFailures.ProcessorRetained);
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> remove = Session.RemoveProcessor(
                CreateOperationId("remove"),
                snapshot.ProcessorSeatedByOperationId,
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
                    Failure.FromCode("assembly-processor.attach-authority-mismatch"));
            }

            if (!ApproximatelySamePose(exactSeatPose, socket.SnapPose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-processor.preview-commit-pose-mismatch"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> seat = Session.SeatProcessor(
                CreateOperationId("seat"),
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
                    Session.RemoveProcessor(
                        CreateOperationId("seat-compensation"),
                        seat.Value.OperationId,
                        Session.AssemblyBuild.Revision);
                SyncProjectionToAuthority();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-processor.compensation-failed"))
                    : physicalCommit;
            }

            _carryOrigin = CarryOrigin.None;
            socket.ResetFeedback();
            SyncProjectionToAuthority();
            return OperationResult.Success();
        }

        internal OperationResult TryPlaceInBuildKit(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform carryAnchor,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns,
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
                    Failure.FromCode("custom-pc-processor-build-kit.context-missing"));
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsSeated ||
                buildKit.IsStaged)
            {
                return OperationResult.Fail(
                    Failure.FromCode("custom-pc-processor-build-kit.authority-mismatch"));
            }

            if (carryAnchor == null || Session.CustomPcBuildKit == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        carryAnchor == null
                            ? "custom-pc-processor-build-kit.carry-anchor-missing"
                            : "custom-pc-processor-build-kit.context-missing"));
            }

            if (Session.CustomPcBuildKit.Revision == long.MaxValue ||
                Session.Inventory.Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            long expectedBuildKitRevision = Session.CustomPcBuildKit.Revision;
            long expectedInventoryRevision = Session.Inventory.Revision;
            ProcessorBuildKitEvaluation evaluation = buildKit.Evaluate(
                interactionOrigin,
                playerRoot,
                physicalItem,
                obstructionMask,
                clockwiseQuarterTurns,
                paused,
                IsAuthorityInHands &&
                !IsSeated &&
                buildKit.HasPickupReceipt);
            if (!evaluation.IsValid)
            {
                return OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode));
            }

            OperationResult<CustomPcBuildKitReceipt> domain =
                Session.PlaceHeldProcessorInCustomPcBuildKit(
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
                            "custom-pc-processor-build-kit.projection-recovery-failed"));
                }
            }

            _carryOrigin = CarryOrigin.None;
            buildKit.ResetFeedback();
            buildKit.RefreshPresentation();
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
            if (snapshot.ProcessorSocketState == ProcessorSocketState.ProcessorSeatedOpen)
            {
                operation = Session.CloseProcessorRetention(
                    CreateOperationId("close-retention"),
                    snapshot.ProcessorSeatedByOperationId,
                    snapshot.Revision);
            }
            else if (snapshot.ProcessorSocketState == ProcessorSocketState.ProcessorRetained)
            {
                operation = Session.OpenProcessorRetention(
                    CreateOperationId("open-retention"),
                    snapshot.ProcessorSeatedByOperationId,
                    snapshot.ProcessorRetainedByOperationId,
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
                    Failure.FromCode("assembly-processor.drop-authority-mismatch"));
            }

            OperationResult transfer = Session.DropHeldProcessorToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation = Session.PickupLooseProcessorToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-processor.drop-compensation-failed"))
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
                    Failure.FromCode("assembly-processor.recovery-authority-mismatch"));
            }

            if (_carryOrigin == CarryOrigin.Seated)
            {
                AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
                if (snapshot.MotherboardSeatState == AssemblySeatState.SeatedSecured)
                {
                    OperationResult<AssemblyOperationReceipt> reseat = Session.SeatProcessor(
                        CreateOperationId("recovery-seat"),
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
                                Session.RemoveProcessor(
                                    CreateOperationId("recovery-compensation"),
                                    reseat.Value.OperationId,
                                    Session.AssemblyBuild.Revision);
                            return compensation.IsFailure
                                ? OperationResult.Fail(Failure.FromCode(
                                    "assembly-processor.recovery-compensation-failed"))
                                : physicalRecovery;
                        }

                        _carryOrigin = CarryOrigin.None;
                        SyncProjectionToAuthority();
                        return OperationResult.Success();
                    }
                }
            }

            if (_carryOrigin == CarryOrigin.BuildKit)
            {
                AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
                if (snapshot.MotherboardSeatState == AssemblySeatState.SeatedSecured &&
                    snapshot.ProcessorSocketState == ProcessorSocketState.EmptyOpen)
                {
                    OperationResult<AssemblyOperationReceipt> seat = Session.SeatProcessor(
                        CreateOperationId("recovery-build-kit-seat"),
                        snapshot.InstalledByOperationId,
                        snapshot.SecuredByOperationId,
                        snapshot.Revision);
                    if (seat.IsSuccess)
                    {
                        OperationResult physicalRecovery =
                            physicalItem.PlaceAt(socket.SnapPose);
                        if (physicalRecovery.IsFailure)
                        {
                            OperationResult<AssemblyOperationReceipt> compensation =
                                Session.RemoveProcessor(
                                    CreateOperationId(
                                        "recovery-build-kit-compensation"),
                                    seat.Value.OperationId,
                                    Session.AssemblyBuild.Revision);
                            return compensation.IsFailure
                                ? OperationResult.Fail(Failure.FromCode(
                                    "assembly-processor.recovery-build-kit-" +
                                    "compensation-failed"))
                                : physicalRecovery;
                        }

                        _carryOrigin = CarryOrigin.None;
                        socket.ResetFeedback();
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

            OperationResult transfer = Session.DropHeldProcessorToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(Failure.FromCode(
                        "assembly-processor.recovery-rollback-failed"))
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
            if (socket == null || Session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-processor.context-missing"));
            }

            socket.ApplyAuthoritativeState(
                Session.AssemblyBuild.MotherboardSeatState,
                Session.AssemblyBuild.ProcessorSocketState);
            buildKit?.RefreshPresentation();
            return OperationResult.Success();
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
                    Failure.FromCode("assembly-processor.identity-mismatch"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            bool physicalMatches = IsSeated
                ? physicalItem.Ownership == PhysicalItemOwnership.World &&
                  physicalItem.IsStablePlacement &&
                  ApproximatelySamePose(
                      new Pose(
                          physicalItem.transform.position,
                          physicalItem.transform.rotation),
                      socket.SnapPose)
                : IsAuthorityInHands
                    ? physicalItem.IsCarried
                    : IsAuthorityInBuildKit
                        ? buildKit != null &&
                          buildKit.IsStaged &&
                          buildKit.MatchesCommittedPlacement(physicalItem)
                        : IsAuthorityLooseWorld &&
                          physicalItem.Ownership == PhysicalItemOwnership.World;
            bool socketMatches = socket.SlotIdValue ==
                                 Session.ProcessorSlotId.Value &&
                                 socket.RetentionIdValue ==
                                 Session.ProcessorRetentionId.Value &&
                                 socket.MatchesAuthorityState(
                                     snapshot.MotherboardSeatState,
                                     snapshot.ProcessorSocketState);
            return physicalMatches && socketMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-processor.projection-invariant"));
        }

        private OperationResult ValidateContext()
        {
            return runtime == null ||
                   physicalItem == null ||
                   socket == null ||
                   !socket.IsConfigured ||
                   Session == null ||
                   !Session.AssemblyBuild.HasProcessorSocket ||
                   inventoryItemId != GarageStockFlowSession.ProcessorItemInstanceIdValue ||
                   socket.SlotIdValue != GarageStockFlowSession.ProcessorSlotIdValue ||
                   socket.RetentionIdValue !=
                       GarageStockFlowSession.ProcessorRetentionIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId
                ? OperationResult.Fail(Failure.FromCode(
                    physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                        ? "assembly-processor.identity-mismatch"
                        : "assembly-processor.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetProcessorItem(out InventoryItemRecord item) &&
                   item.Id == session.ProcessorItemId &&
                   item.ProductId == session.ProcessorProductId &&
                   item.ContainerId == containerId;
        }

        private StableId<AssemblyOperationIdScope> CreateOperationId(string action)
        {
            long nextRevision = Session.AssemblyBuild.Revision + 1L;
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.processor-{action}.r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }
    }
}
