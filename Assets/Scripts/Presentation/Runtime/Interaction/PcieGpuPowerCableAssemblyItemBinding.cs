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
    [RequireComponent(typeof(PcieGpuPowerCableRuntimeGeometry))]
    public sealed class PcieGpuPowerCableAssemblyItemBinding : MonoBehaviour
    {
        private enum CarryOrigin
        {
            None = 0,
            LooseWorld = 1,
            Routed = 2,
            BuildKit = 3
        }

        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PhysicalItemProjection physicalItem;
        [SerializeField] private PcieGpuPowerCableRouteProjection route;
        [SerializeField] private PcieGpuPowerCableRuntimeGeometry geometry;
        [SerializeField] private PcieGpuPowerCableBuildKitProjection buildKit;
        [SerializeField] private string inventoryItemId =
            GarageStockFlowSession.PcieGpuPowerCableItemInstanceIdValue;

        private CarryOrigin _carryOrigin;
        private Pose _initialLoosePose;
        private bool _hasInitialLoosePose;

        public GarageStockFlowRuntime Runtime => runtime;

        public PhysicalItemProjection PhysicalItem => physicalItem;

        public PcieGpuPowerCableRouteProjection Route => route;

        public PcieGpuPowerCableRuntimeGeometry Geometry => geometry;

        public PcieGpuPowerCableBuildKitProjection BuildKit => buildKit;

        public string InventoryItemIdValue => inventoryItemId;

        public GarageStockFlowSession Session => runtime != null
            ? runtime.EnsureInitialized()
            : null;

        public bool IsRouted
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session != null &&
                       session.AssemblyBuild.IsPcieGpuPowerCableRouted &&
                       session.AssemblyBuild.PcieGpuPowerCableItemId ==
                           session.PcieGpuPowerCableItemId;
            }
        }

        public bool IsAuthorityInHands => IsInContainer(
            Session?.HandsContainerId ?? default);

        public bool IsAuthorityLooseWorld => IsInContainer(
            Session?.WorldFloorContainerId ?? default);

        public bool IsAuthorityInBuildKit => IsInContainer(
            Session?.PcieGpuPowerCableBuildKitContainerId ?? default);

        private void Awake()
        {
            physicalItem ??= GetComponent<PhysicalItemProjection>();
            geometry ??= GetComponent<PcieGpuPowerCableRuntimeGeometry>();
            CaptureInitialLoosePose();
        }

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PhysicalItemProjection itemProjection,
            PcieGpuPowerCableRouteProjection routeProjection,
            PcieGpuPowerCableRuntimeGeometry runtimeGeometry,
            string stableInventoryItemId,
            PcieGpuPowerCableBuildKitProjection buildKitProjection = null)
        {
            runtime = stockFlowRuntime != null
                ? stockFlowRuntime
                : throw new ArgumentNullException(nameof(stockFlowRuntime));
            physicalItem = itemProjection != null
                ? itemProjection
                : throw new ArgumentNullException(nameof(itemProjection));
            route = routeProjection != null
                ? routeProjection
                : throw new ArgumentNullException(nameof(routeProjection));
            geometry = runtimeGeometry != null
                ? runtimeGeometry
                : throw new ArgumentNullException(nameof(runtimeGeometry));
            buildKit = buildKitProjection;
            inventoryItemId = StableId<ItemInstanceIdScope>.Parse(
                stableInventoryItemId).Value;
            if (inventoryItemId !=
                GarageStockFlowSession.PcieGpuPowerCableItemInstanceIdValue)
            {
                throw new ArgumentException(
                    "The prototype PCIe GPU binding must use the canonical cable identity.",
                    nameof(stableInventoryItemId));
            }

            CaptureInitialLoosePose();
            SyncProjectionToAuthority();
        }

        public bool MatchesBuildKitConfiguration(
            PcieGpuPowerCableBuildKitProjection buildKitProjection)
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

            if (!physicalItem.IsCarried || IsRouted || !IsAuthorityLooseWorld)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.pickup-authority-mismatch"));
            }

            OperationResult transfer = Session.PickupLoosePcieGpuPowerCableToHands();
            if (transfer.IsSuccess)
            {
                _carryOrigin = CarryOrigin.LooseWorld;
                geometry.SetRouted(routed: false);
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
                IsRouted ||
                !IsAuthorityInBuildKit ||
                buildKit == null ||
                !buildKit.IsStaged ||
                buildKit.IsReleasedForAssembly)
            {
                return OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-pcie-gpu-power-cable-assembly." +
                        "pickup-authority-mismatch"));
            }

            OperationResult headroom = ValidateBuildKitAssemblyPickupHeadroom();
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult<CustomPcBuildKitAssemblyHandoffReceipt> handoff =
                Session.PickupStagedPcieGpuPowerCableForAssembly();
            if (handoff.IsSuccess)
            {
                _carryOrigin = CarryOrigin.BuildKit;
                buildKit.RefreshPresentation();
                geometry.SetRouted(routed: false);
            }

            return handoff.IsSuccess
                ? OperationResult.Success()
                : OperationResult.Fail(handoff.Error);
        }

        public OperationResult TryCommitRoutedUnroute()
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (physicalItem.Ownership != PhysicalItemOwnership.World ||
                physicalItem.IsCarried ||
                !IsRouted)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.unroute-authority-mismatch"));
            }

            StableId<AssemblyOperationIdScope> sourceRouteOperationId =
                Session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId;
            OperationResult<PcieGpuPowerCableOperationReceipt> unroute =
                Session.UnroutePcieGpuPowerCable(
                    CreateOperationId("unroute"),
                    sourceRouteOperationId,
                    Session.AssemblyBuild.PcieGpuPowerCableRevision);
            if (unroute.IsFailure)
            {
                return OperationResult.Fail(unroute.Error);
            }

            _carryOrigin = CarryOrigin.Routed;
            route.ApplyAuthoritativeState(routed: false);
            geometry.SetRouted(routed: false);
            return OperationResult.Success();
        }

        public OperationResult TryRouteAt(
            Pose exactRoutePose,
            PowerCableKeyOrientation orientation,
            Transform carryAnchor,
            int heldLayer)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsRouted ||
                (buildKit != null &&
                 buildKit.HasPickupReceipt &&
                 !buildKit.IsReleasedForAssembly))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.route-authority-mismatch"));
            }

            if (!route.LastEvaluation.CanRoute ||
                route.LastEvaluation.Orientation != orientation ||
                !ApproximatelySamePose(exactRoutePose, route.LastEvaluation.Pose))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.preview-commit-pose-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom();
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult<PcieGpuPowerCableOperationReceipt> domain =
                Session.RoutePcieGpuPowerCable(
                    CreateOperationId("route"),
                    orientation,
                    Session.AssemblyBuild.PcieGpuPowerCableRevision);
            if (domain.IsFailure)
            {
                return OperationResult.Fail(domain.Error);
            }

            OperationResult physicalCommit = physicalItem.PlaceAt(exactRoutePose);
            if (physicalCommit.IsFailure)
            {
                OperationResult recovery =
                    physicalItem.RecoverToStablePlacementAfterAuthority(
                        exactRoutePose);
                if (recovery.IsFailure ||
                    !physicalItem.IsStablePlacement ||
                    physicalItem.Ownership != PhysicalItemOwnership.World)
                {
                    return OperationResult.Fail(
                        Failure.FromCode(
                            "custom-pc-pcie-gpu-power-cable-assembly." +
                            "projection-recovery-failed"));
                }
            }

            _carryOrigin = CarryOrigin.None;
            route.SetRouteModeActive(active: false);
            route.ApplyAuthoritativeState(routed: true);
            geometry.SetRouted(routed: true);
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
                return OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.context-missing"));
            }

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsRouted ||
                buildKit.IsStaged)
            {
                return OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.authority-mismatch"));
            }

            if (carryAnchor == null || Session.CustomPcBuildKit == null)
            {
                return OperationResult.Fail(Failure.FromCode(
                    carryAnchor == null
                        ? "custom-pc-pcie-gpu-power-cable-build-kit.carry-anchor-missing"
                        : "custom-pc-pcie-gpu-power-cable-build-kit.context-missing"));
            }

            if (Session.CustomPcBuildKit.Revision == long.MaxValue ||
                Session.Inventory.Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            long expectedBuildKitRevision = Session.CustomPcBuildKit.Revision;
            long expectedInventoryRevision = Session.Inventory.Revision;
            PcieGpuPowerCableBuildKitEvaluation evaluation = buildKit.Evaluate(
                interactionOrigin,
                playerRoot,
                physicalItem,
                obstructionMask,
                clockwiseHalfTurns,
                paused,
                IsAuthorityInHands &&
                !IsRouted &&
                buildKit.HasPickupReceipt &&
                !buildKit.IsReleasedForAssembly);
            if (!evaluation.IsValid)
            {
                return OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode));
            }

            OperationResult<CustomPcBuildKitReceipt> domain =
                Session.PlaceHeldPcieGpuPowerCableInCustomPcBuildKit(
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
                    return OperationResult.Fail(Failure.FromCode(
                        "custom-pc-pcie-gpu-power-cable-build-kit." +
                        "projection-recovery-failed"));
                }
            }

            _carryOrigin = CarryOrigin.None;
            route.SetRouteModeActive(active: false);
            route.ApplyAuthoritativeState(routed: false);
            geometry.SetRouted(routed: false);
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

            if (!physicalItem.IsCarried || !IsAuthorityInHands || IsRouted ||
                (buildKit != null && buildKit.HasPickupReceipt))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.drop-authority-mismatch"));
            }

            OperationResult headroom = ValidateCompensationHeadroom();
            if (headroom.IsFailure)
            {
                return headroom;
            }

            OperationResult transfer = Session.DropHeldPcieGpuPowerCableToWorld();
            if (transfer.IsFailure)
            {
                return transfer;
            }

            OperationResult physicalDrop = physicalItem.ReleaseTo(worldPose);
            if (physicalDrop.IsFailure)
            {
                OperationResult compensation =
                    Session.PickupLoosePcieGpuPowerCableToHands();
                return compensation.IsFailure
                    ? OperationResult.Fail(Failure.FromCode(
                        "assembly-pcie-gpu-cable.drop-compensation-failed"))
                    : physicalDrop;
            }

            _carryOrigin = CarryOrigin.None;
            geometry.SetRouted(routed: false);
            return OperationResult.Success();
        }

        public OperationResult TryRecoverHeld(Transform carryAnchor, int heldLayer)
        {
            OperationResult context = ValidateContext();
            if (context.IsFailure)
            {
                return context;
            }

            if (IsRouted)
            {
                OperationResult<Pose> routePose = route.ResolveRoutedItemPose();
                if (routePose.IsFailure)
                {
                    return OperationResult.Fail(routePose.Error);
                }

                OperationResult routedRecovery =
                    physicalItem.SynchronizeStableWorldPose(routePose.Value);
                if (routedRecovery.IsSuccess)
                {
                    _carryOrigin = CarryOrigin.None;
                    SyncProjectionToAuthority();
                }

                return routedRecovery;
            }

            if (!IsAuthorityInHands)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.recovery-authority-mismatch"));
            }

            OperationResult physicalRecovery = physicalItem.RecoverToLastSafePose();
            if (physicalRecovery.IsFailure)
            {
                return physicalRecovery;
            }

            OperationResult transfer = Session.DropHeldPcieGpuPowerCableToWorld();
            if (transfer.IsFailure)
            {
                OperationResult rollback = physicalItem.BeginCarry(
                    carryAnchor,
                    heldLayer);
                return rollback.IsFailure
                    ? OperationResult.Fail(Failure.FromCode(
                        "assembly-pcie-gpu-cable.recovery-rollback-failed"))
                    : transfer;
            }

            if (_carryOrigin == CarryOrigin.Routed && _hasInitialLoosePose)
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
            if (route == null || geometry == null || Session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.context-missing"));
            }

            bool routed = IsRouted;
            route.ApplyAuthoritativeState(routed);
            geometry.SetRouted(routed);
            buildKit?.RefreshPresentation();
            if (IsAuthorityInBuildKit &&
                buildKit != null &&
                physicalItem != null &&
                physicalItem.Ownership == PhysicalItemOwnership.World &&
                physicalItem.IsStablePlacement &&
                !buildKit.MatchesCommittedPlacement(physicalItem))
            {
                OperationResult synchronized =
                    physicalItem.SynchronizeStableWorldPose(
                        buildKit.ResolveSnapPose(0));
                if (synchronized.IsFailure)
                {
                    return synchronized;
                }
            }
            else if (routed &&
                physicalItem != null &&
                physicalItem.Ownership == PhysicalItemOwnership.World &&
                physicalItem.IsStablePlacement)
            {
                OperationResult<Pose> routedPose = route.ResolveRoutedItemPose();
                if (routedPose.IsFailure)
                {
                    return OperationResult.Fail(routedPose.Error);
                }

                OperationResult synchronized =
                    physicalItem.SynchronizeStableWorldPose(routedPose.Value);
                if (synchronized.IsFailure)
                {
                    return synchronized;
                }
            }

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

            if (physicalItem.ItemIdValue != inventoryItemId ||
                !geometry.IsCanonical ||
                !route.IsConfigured ||
                geometry.IsRouted != IsRouted ||
                route.IsAuthoritativeRouted != IsRouted ||
                (IsAuthorityInBuildKit &&
                 (buildKit == null || !buildKit.IsCanonical)))
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.identity-mismatch"));
            }

            bool physicalMatches;
            if (IsRouted)
            {
                OperationResult<Pose> routedPose = route.ResolveRoutedItemPose();
                physicalMatches = routedPose.IsSuccess &&
                                  physicalItem.Ownership ==
                                      PhysicalItemOwnership.World &&
                                  physicalItem.IsStablePlacement &&
                                  ApproximatelySamePose(
                                      new Pose(
                                          physicalItem.transform.position,
                                          physicalItem.transform.rotation),
                                      routedPose.Value);
            }
            else
            {
                physicalMatches = IsAuthorityInHands
                    ? physicalItem.IsCarried
                    : IsAuthorityInBuildKit
                        ? buildKit != null &&
                          buildKit.IsStaged &&
                          buildKit.MatchesCommittedPlacement(physicalItem) &&
                          !IsRouted
                        : IsAuthorityLooseWorld &&
                          physicalItem.Ownership == PhysicalItemOwnership.World;
            }

            return physicalMatches
                ? OperationResult.Success()
                : OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.projection-invariant"));
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
                   route == null ||
                   geometry == null ||
                   Session == null ||
                   !Session.AssemblyBuild.HasPcieGpuPowerCableRoute ||
                   !route.IsConfigured ||
                   !geometry.IsCanonical ||
                   inventoryItemId !=
                       GarageStockFlowSession.PcieGpuPowerCableItemInstanceIdValue ||
                   physicalItem.ItemIdValue != inventoryItemId ||
                   route.RouteIdValue !=
                       GarageStockFlowSession.PcieGpuPowerCableRouteIdValue
                ? OperationResult.Fail(Failure.FromCode(
                    physicalItem != null && physicalItem.ItemIdValue != inventoryItemId
                        ? "assembly-pcie-gpu-cable.identity-mismatch"
                        : "assembly-pcie-gpu-cable.context-missing"))
                : OperationResult.Success();
        }

        private bool IsInContainer(StableId<ContainerIdScope> containerId)
        {
            GarageStockFlowSession session = Session;
            return session != null &&
                   !containerId.IsEmpty &&
                   session.TryGetPcieGpuPowerCableItem(out InventoryItemRecord item) &&
                   item.Id == session.PcieGpuPowerCableItemId &&
                   item.ProductId == session.PcieGpuPowerCableProductId &&
                   item.ContainerId == containerId;
        }

        private OperationResult ValidateCompensationHeadroom()
        {
            GarageStockFlowSession session = Session;
            if (session == null)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.context-missing"));
            }

            if (session.AssemblyBuild.PcieGpuPowerCableRevision == long.MaxValue)
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
                    Failure.FromCode(
                        "custom-pc-pcie-gpu-power-cable-assembly.context-missing"));
            }

            if (session.CustomPcBuildKit.Revision == long.MaxValue)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            if (session.AssemblyBuild.Revision == long.MaxValue ||
                session.AssemblyBuild.PcieGpuPowerCableRevision == long.MaxValue ||
                session.AssemblyBuild.Atx24PowerCableRevision == long.MaxValue ||
                session.AssemblyBuild.Eps12vPowerCableRevision == long.MaxValue)
            {
                return OperationResult.Fail(AssemblyFailures.RevisionOverflow);
            }

            return session.Inventory.Revision > long.MaxValue - 2L
                ? OperationResult.Fail(AssemblyFailures.InventoryRevisionOverflow)
                : OperationResult.Success();
        }

        private StableId<AssemblyOperationIdScope> CreateOperationId(string action)
        {
            long nextRevision = Session.AssemblyBuild.PcieGpuPowerCableRevision + 1L;
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.pcie-gpu-power-cable-{action}." +
                $"r{nextRevision:000000}");
        }

        private static bool ApproximatelySamePose(Pose left, Pose right)
        {
            return Vector3.Distance(left.position, right.position) <= 0.0005f &&
                   Quaternion.Angle(left.rotation, right.rotation) <= 0.05f;
        }
    }
}
