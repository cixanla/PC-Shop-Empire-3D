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
    public sealed class ProcessorCoolerAssemblyItemBinding : MonoBehaviour
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
        [SerializeField] private ProcessorCoolerSlotProjection slot;
        [SerializeField] private ProcessorCoolerBuildKitProjection buildKit;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue;

        private CarryOrigin _carryOrigin;
        private Pose _initialLoosePose;
        private bool _hasInitialLoosePose;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public ProcessorCoolerSlotProjection Slot => slot;

        public ProcessorCoolerBuildKitProjection BuildKit => buildKit;

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

                ProcessorCoolerSlotState state =
                    session.AssemblyBuild.ProcessorCoolerSlotState;
                return (state == ProcessorCoolerSlotState.CoolerSeatedUnsecured ||
                        state == ProcessorCoolerSlotState.CoolerRetained) &&
                       session.AssemblyBuild.ProcessorCoolerItemId ==
                           session.ProcessorCoolerItemId;
            }
        }

        public bool IsRetained
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.ProcessorCoolerSlotState ==
                           ProcessorCoolerSlotState.CoolerRetained &&
                       session.AssemblyBuild.ProcessorCoolerItemId ==
                           session.ProcessorCoolerItemId;
            }
        }

        public bool IsHostReady
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.MotherboardSeatState ==
                           AssemblySeatState.SeatedSecured &&
                       session.AssemblyBuild.ProcessorSocketState ==
                           ProcessorSocketState.ProcessorRetained;
            }
        }

        public bool IsAuthorityInHands => IsInContainer(
            Session?.HandsContainerId ?? default);

        public bool IsAuthorityLooseWorld => IsInContainer(
            Session?.WorldFloorContainerId ?? default);

        public bool IsAuthorityInBuildKit => IsInContainer(
            Session?.ProcessorCoolerBuildKitContainerId ?? default);

        private void Awake()
        {
            physicalItem ??= GetComponent<PhysicalItemProjection>();
            CaptureInitialLoosePose();
        }

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            ProcessorCoolerSlotProjection slotProjection,
            string stableInventoryItemId,
            ProcessorCoolerBuildKitProjection buildKitProjection = null)
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
                GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype cooler binding must use the canonical inventory identity.",
                    nameof(stableInventoryItemId));
            }

            CaptureInitialLoosePose();
            SyncProjectionToAuthority();
        }

        public bool MatchesBuildKitConfiguration(
            ProcessorCoolerBuildKitProjection buildKitProjection)
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
                    Failure.FromCode("assembly-cooler.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLooseProcessorCoolerToHands();
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

            if (physicalItem.Ownership != PhysicalItemOwnership.PlayerHands ||
                !physicalItem.IsCarried ||
                IsSeated ||
                !IsAuthorityInBuildKit ||
                buildKit == null ||
                !buildKit.IsStaged)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-assembly." +
                        "pickup-authority-mismatch"));
            }

            OperationResult headroom = ValidateBuildKitAssemblyPickupHeadroom();
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                Session.PickupStagedProcessorCoolerForAssembly();
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
                    Failure.FromCode("assembly-cooler.detach-authority-mismatch"));
            }

            if (IsRetained)
            {
                return OperationResult.Fail(AssemblyFailures.ProcessorCoolerRetained);
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<AssemblyOperationReceipt> remove =
                Session.RemoveProcessorCooler(
                    CreateOperationId("remove"),
                    snapshot.ProcessorCoolerSeatedByOperationId,
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
            ProcessorCoolerMountOrientation orientation,
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
                    Failure.FromCode("assembly-cooler.attach-authority-mismatch"));
            }

            if (!slot.LastEvaluation.CanSeat ||
                slot.LastEvaluation.Orientation != orientation ||
                !ApproximatelySamePose(exactSeatPose, slot.LastEvaluation.Pose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.preview-commit-pose-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom();
            if (headroom.IsFailure)
            {
                return headroom;
            }

            // Commit the reversible physical projection first. If the domain rejects the
            // seat, returning to carry leaves the single-use TIM untouched.
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
                Session.SeatProcessorCooler(
                    CreateOperationId("seat"),
                    orientation,
                    snapshot.InstalledByOperationId,
                    snapshot.SecuredByOperationId,
                    snapshot.ProcessorSeatedByOperationId,
                    snapshot.ProcessorRetainedByOperationId,
                    snapshot.Revision);
            if (seat.IsFailure)
            {
                OperationResult safePoseRestore =
                    physicalItem.RestoreLastSafePoseSnapshot(previousSafePose);
                if (safePoseRestore.IsFailure)
                {
                    return OperationResult.Fail(
                        Failure.FromCode("assembly-cooler.safe-pose-rollback-failed"));
                }

                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                if (rollback.IsFailure)
                {
                    OperationResult physicalRecovery =
                        physicalItem.RecoverToLastSafePose();
                    OperationResult authorityRecovery = physicalRecovery.IsSuccess
                        ? Session.DropHeldProcessorCoolerToWorld()
                        : OperationResult.Fail(
                            Failure.FromCode("assembly-cooler.recovery-unavailable"));
                    _carryOrigin = CarryOrigin.None;
                    return physicalRecovery.IsFailure || authorityRecovery.IsFailure
                        ? OperationResult.Fail(
                            Failure.FromCode(
                                "assembly-cooler.physical-rollback-compensation-failed"))
                        : OperationResult.Fail(
                            Failure.FromCode("assembly-cooler.physical-rollback-failed"));
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
            if (snapshot.ProcessorCoolerSlotState ==
                ProcessorCoolerSlotState.CoolerSeatedUnsecured)
            {
                operation = Session.RetainProcessorCooler(
                    CreateOperationId("retain-1-3-2-4"),
                    snapshot.ProcessorCoolerSeatedByOperationId,
                    snapshot.Revision);
            }
            else if (snapshot.ProcessorCoolerSlotState ==
                ProcessorCoolerSlotState.CoolerRetained)
            {
                operation = Session.UnretainProcessorCooler(
                    CreateOperationId("unretain-4-2-3-1"),
                    snapshot.ProcessorCoolerSeatedByOperationId,
                    snapshot.ProcessorCoolerRetainedByOperationId,
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
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.context-missing"));
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsSeated ||
                buildKit.IsStaged)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.authority-mismatch"));
            }

            if (carryAnchor == null || Session.CustomPcBuildKit == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        carryAnchor == null
                            ? "custom-pc-processor-cooler-build-kit.carry-anchor-missing"
                            : "custom-pc-processor-cooler-build-kit.context-missing"));
            }

            if (Session.CustomPcBuildKit.Revision == long.MaxValue ||
                Session.Inventory.Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            long expectedBuildKitRevision = Session.CustomPcBuildKit.Revision;
            long expectedInventoryRevision = Session.Inventory.Revision;
            ProcessorCoolerBuildKitEvaluation evaluation = buildKit.Evaluate(
                interactionOrigin,
                playerRoot,
                physicalItem,
                obstructionMask,
                clockwiseQuarterTurns,
                paused,
                IsAuthorityInHands && !IsSeated && buildKit.HasPickupReceipt);
            if (!evaluation.IsValid)
            {
                return OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode));
            }

            OperationResult<CustomPcBuildKitReceipt> domain =
                Session.PlaceHeldProcessorCoolerInCustomPcBuildKit(
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
                            "custom-pc-processor-cooler-build-kit.projection-recovery-failed"));
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
                    Failure.FromCode("assembly-cooler.drop-authority-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom(
                requiresAssemblyRevision: false);
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult transfer = Session.DropHeldProcessorCoolerToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation =
                    Session.PickupLooseProcessorCoolerToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-cooler.drop-compensation-failed"))
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
                    Session.AssemblyBuild.ProcessorCoolerMountOrientation);
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
                    Failure.FromCode("assembly-cooler.recovery-authority-mismatch"));
            }

            if (_carryOrigin == CarryOrigin.BuildKit)
            {
                AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
                if (snapshot.MotherboardSeatState == AssemblySeatState.SeatedSecured &&
                    snapshot.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained &&
                    snapshot.MemorySlotState == MemorySlotState.MemoryModuleRetained &&
                    snapshot.StorageSlotState == StorageSlotState.StorageDeviceSecured &&
                    snapshot.ProcessorCoolerSlotState ==
                        ProcessorCoolerSlotState.EmptyOpen &&
                    snapshot.ProcessorCoolerTimState ==
                        ProcessorCoolerTimState.Unsupported &&
                    Session.TryGetProcessorCoolerItem(
                        out InventoryItemRecord processorCooler) &&
                    (processorCooler.StateFlags &
                     InventorySerializedItemStateFlags
                         .PreAppliedConsumableConsumed) == 0)
                {
                    OperationResult headroom = ValidateCompensationHeadroom();
                    if (headroom.IsFailure)
                    {
                        return headroom;
                    }

                    OperationResult<Pose> seatPose = slot.ResolveSeatPose(
                        ProcessorCoolerMountOrientation.Primary);
                    if (seatPose.IsFailure)
                    {
                        return OperationResult.Fail(seatPose.Error);
                    }

                    var previousSafePose = new Pose(
                        physicalItem.LastSafePosition,
                        physicalItem.LastSafeRotation);
                    OperationResult physicalSeat = physicalItem.PlaceAt(seatPose.Value);
                    if (physicalSeat.IsFailure)
                    {
                        return physicalSeat;
                    }

                    OperationResult<AssemblyOperationReceipt> seat =
                        Session.SeatProcessorCooler(
                            CreateOperationId("recovery-build-kit-seat"),
                            ProcessorCoolerMountOrientation.Primary,
                            snapshot.InstalledByOperationId,
                            snapshot.SecuredByOperationId,
                            snapshot.ProcessorSeatedByOperationId,
                            snapshot.ProcessorRetainedByOperationId,
                            snapshot.Revision);
                    if (seat.IsSuccess)
                    {
                        _carryOrigin = CarryOrigin.None;
                        slot.ResetFeedback();
                        SyncProjectionToAuthority();
                        return OperationResult.Success();
                    }

                    OperationResult safePoseRestore =
                        physicalItem.RestoreLastSafePoseSnapshot(previousSafePose);
                    OperationResult carryRestore = safePoseRestore.IsSuccess
                        ? physicalItem.BeginCarry(carryAnchor, heldLayer)
                        : OperationResult.Fail(
                            Failure.FromCode(
                                "assembly-cooler.recovery-safe-pose-restore-failed"));
                    return safePoseRestore.IsFailure || carryRestore.IsFailure
                        ? OperationResult.Fail(
                            Failure.FromCode(
                                "assembly-cooler.recovery-build-kit-rollback-failed"))
                        : OperationResult.Fail(seat.Error);
                }
            }

            OperationResult physicalRecovery = physicalItem.RecoverToLastSafePose();
            if (physicalRecovery.IsFailure)
            {
                return physicalRecovery;
            }

            OperationResult transfer = Session.DropHeldProcessorCoolerToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(carryAnchor, heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(
                        Failure.FromCode("assembly-cooler.recovery-rollback-failed"))
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
                    Failure.FromCode("assembly-cooler.context-missing"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            slot.ApplyAuthoritativeState(
                snapshot.MotherboardSeatState,
                snapshot.ProcessorSocketState,
                snapshot.ProcessorCoolerSlotState);
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
                    Failure.FromCode("assembly-cooler.identity-mismatch"));
            }

            AssemblyBuildSnapshot snapshot = Session.AssemblyBuild.GetSnapshot();
            OperationResult<Pose> authoritativeSeatPose = slot.ResolveSeatPose(
                snapshot.ProcessorCoolerMountOrientation);
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
                slot.SlotIdValue == Session.ProcessorCoolerSlotId.Value &&
                slot.BracketIdValue == Session.ProcessorCoolerBracketId.Value &&
                slot.MatchesLogicalAuthorityState(
                    snapshot.MotherboardSeatState,
                    snapshot.ProcessorSocketState,
                    snapshot.ProcessorCoolerSlotState);
            return physicalMatches && slotMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.projection-invariant"));
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
                   !Session.AssemblyBuild.HasProcessorCoolerSlot ||
                   inventoryItemId !=
                       GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue ||
                   slot.SlotIdValue !=
                       GarageStockFlowSession.ProcessorCoolerSlotIdValue ||
                   slot.BracketIdValue !=
                       GarageStockFlowSession.ProcessorCoolerBracketIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId
                ? OperationResult.Fail(Failure.FromCode(
                    physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                        ? "assembly-cooler.identity-mismatch"
                        : "assembly-cooler.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetProcessorCoolerItem(out InventoryItemRecord item) &&
                   item.Id == session.ProcessorCoolerItemId &&
                   item.ProductId == session.ProcessorCoolerProductId &&
                   item.ContainerId == containerId;
        }

        private OperationResult ValidateCompensationHeadroom(
            bool requiresAssemblyRevision = true)
        {
            GarageStockFlowSession session = Session;
            if (session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.context-missing"));
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

        private OperationResult ValidateBuildKitAssemblyPickupHeadroom()
        {
            GarageStockFlowSession session = Session;
            if (session == null || session.CustomPcBuildKit == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.context-missing"));
            }

            if (session.CustomPcBuildKit.Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            if (session.AssemblyBuild.Revision > long.MaxValue - 2L)
            {
                return OperationResult.Fail(AssemblyFailures.RevisionOverflow);
            }

            return session.Inventory.Revision > long.MaxValue - 3L
                ? OperationResult.Fail(AssemblyFailures.InventoryRevisionOverflow)
                : OperationResult.Success();
        }

        private StableId<AssemblyOperationIdScope> CreateOperationId(string action)
        {
            long nextRevision = Session.AssemblyBuild.Revision + 1L;
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.cooler-{action}.r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }
    }
}
