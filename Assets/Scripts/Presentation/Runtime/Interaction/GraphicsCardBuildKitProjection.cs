using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed class GraphicsCardBuildKitProjectionIdScope : IStableIdScope
    {
    }

    public enum GraphicsCardBuildKitStatus
    {
        Uninitialized = 0,
        Valid = 1,
        ContextMissing = 2,
        Paused = 3,
        AuthorityBlocked = 4,
        PrerequisiteMissing = 5,
        AlreadyStaged = 6,
        OutOfRange = 7,
        NotFocused = 8,
        LineOfSightBlocked = 9,
        Unsupported = 10,
        OutsideSurface = 11,
        Obstructed = 12
    }

    public readonly struct GraphicsCardBuildKitEvaluation
    {
        public GraphicsCardBuildKitEvaluation(
            GraphicsCardBuildKitStatus status,
            Pose pose,
            bool hasPose)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
        }

        public GraphicsCardBuildKitStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public bool IsValid => Status == GraphicsCardBuildKitStatus.Valid && HasPose;

        public string FailureCode => Status switch
        {
            GraphicsCardBuildKitStatus.ContextMissing =>
                "custom-pc-graphics-card-build-kit.context-missing",
            GraphicsCardBuildKitStatus.Paused =>
                "custom-pc-graphics-card-build-kit.paused",
            GraphicsCardBuildKitStatus.AuthorityBlocked =>
                "custom-pc-graphics-card-build-kit.authority-blocked",
            GraphicsCardBuildKitStatus.PrerequisiteMissing =>
                "custom-pc-graphics-card-build-kit.prerequisite-missing",
            GraphicsCardBuildKitStatus.AlreadyStaged =>
                "custom-pc-graphics-card-build-kit.already-staged",
            GraphicsCardBuildKitStatus.OutOfRange =>
                "custom-pc-graphics-card-build-kit.out-of-range",
            GraphicsCardBuildKitStatus.NotFocused =>
                "custom-pc-graphics-card-build-kit.focus-missing",
            GraphicsCardBuildKitStatus.LineOfSightBlocked =>
                "custom-pc-graphics-card-build-kit.line-of-sight-blocked",
            GraphicsCardBuildKitStatus.Unsupported =>
                "custom-pc-graphics-card-build-kit.unsupported",
            GraphicsCardBuildKitStatus.OutsideSurface =>
                "custom-pc-graphics-card-build-kit.outside-surface",
            GraphicsCardBuildKitStatus.Obstructed =>
                "custom-pc-graphics-card-build-kit.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Dedicated fail-closed physical projection for the reserved custom-PC graphics card.
    /// Inventory and CustomPcBuildKitAuthority own custody; this component owns only
    /// focus, keyed 180-degree staging geometry and visible 5/10 to 6/10 progress.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(65)]
    public sealed class GraphicsCardBuildKitProjection : MonoBehaviour
    {
        public const string PrototypeProjectionIdValue =
            "world.graphics-card-build-kit.garage-001";
        public const string PrototypeSurfaceIdValue =
            "world.graphics-card-build-kit-surface.garage-001";
        public const int PrototypeTotalComponentCount = 10;
        public const float DefaultInteractionRange = 2.25f;
        public const float DefaultMinimumFocusDot = 0.92f;

        private const int HitCapacity = 32;
        private const float RotationStepDegrees = 180f;
        private const float SupportProbeHeight = 0.04f;
        private const float SupportProbeDistance = 0.08f;
        private const float FootprintInsetX = 0.132f;
        private const float FootprintInsetZ = 0.052f;
        private static readonly RaycastHit[] LineHits = new RaycastHit[HitCapacity];
        private static readonly Collider[] Overlaps = new Collider[HitCapacity];

        [SerializeField] private string projectionId = PrototypeProjectionIdValue;
        [SerializeField] private GarageStockFlowRuntime runtime;
        [SerializeField] private PlacementSurface surface;
        [SerializeField] private Transform snapAnchor;
        [SerializeField] private TextMesh progressText;
        [SerializeField, Min(0.1f)] private float maximumRange =
            DefaultInteractionRange;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot =
            DefaultMinimumFocusDot;

        public StableId<GraphicsCardBuildKitProjectionIdScope> ProjectionId =>
            StableId<GraphicsCardBuildKitProjectionIdScope>.Parse(projectionId);

        public string ProjectionIdValue => projectionId;

        public GarageStockFlowRuntime Runtime => runtime;

        public PlacementSurface Surface => surface;

        public Transform SnapAnchor => snapAnchor;

        public TextMesh ProgressText => progressText;

        public Collider SupportCollider => surface != null
            ? surface.SurfaceCollider
            : null;

        public bool IsFocused { get; private set; }

        public bool HasContextualAttention { get; private set; }

        public GraphicsCardBuildKitEvaluation LastEvaluation { get; private set; }

        public GarageStockFlowSession Session => runtime != null
            ? runtime.EnsureInitialized()
            : null;

        public int StagedComponentCount =>
            Session?.CustomPcBuildKit?.StagedComponentCount ?? 0;

        public bool HasMotherboardPrerequisite
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeCustomPcBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.MotherboardStaged;
            }
        }

        public bool HasProcessorPrerequisite
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeProcessorBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.ProcessorStaged;
            }
        }

        public bool HasMemoryModulePrerequisite
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeMemoryModuleBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.MemoryModuleStaged;
            }
        }

        public bool HasStoragePrerequisite
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeStorageBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.StorageStaged;
            }
        }

        public bool HasProcessorCoolerPrerequisite
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeProcessorCoolerBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.ProcessorCoolerStaged;
            }
        }

        public bool IsStaged
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeGraphicsCardBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.GraphicsCardStaged;
            }
        }

        public bool HasPickupReceipt
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeGraphicsCardBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.GraphicsCardInHands;
            }
        }

        public bool IsConfigured => runtime != null &&
                                    surface != null &&
                                    surface.SurfaceCollider != null &&
                                    surface.SurfaceCollider.enabled &&
                                    snapAnchor != null &&
                                    progressText != null;

        public bool IsCanonical => IsConfigured &&
                                   projectionId == PrototypeProjectionIdValue &&
                                   surface.SurfaceId == PrototypeSurfaceIdValue &&
                                   surface.SurfaceCollider.gameObject.layer == 0 &&
                                   progressText.gameObject.layer ==
                                       LayerMask.NameToLayer("Ignore Raycast");

        public void Configure(
            GarageStockFlowRuntime stockFlowRuntime,
            PlacementSurface placementSurface,
            Transform exactSnapAnchor,
            TextMesh authoredProgressText,
            string stableProjectionId = PrototypeProjectionIdValue,
            float interactionRange = DefaultInteractionRange,
            float focusDot = DefaultMinimumFocusDot)
        {
            runtime = stockFlowRuntime != null
                ? stockFlowRuntime
                : throw new ArgumentNullException(nameof(stockFlowRuntime));
            surface = placementSurface != null
                ? placementSurface
                : throw new ArgumentNullException(nameof(placementSurface));
            snapAnchor = exactSnapAnchor != null
                ? exactSnapAnchor
                : throw new ArgumentNullException(nameof(exactSnapAnchor));
            progressText = authoredProgressText != null
                ? authoredProgressText
                : throw new ArgumentNullException(nameof(authoredProgressText));
            projectionId = StableId<GraphicsCardBuildKitProjectionIdScope>.Parse(
                stableProjectionId).Value;
            maximumRange = Mathf.Max(0.1f, interactionRange);
            minimumFocusDot = Mathf.Clamp01(focusDot);

            if (surface.SurfaceId != PrototypeSurfaceIdValue)
            {
                throw new ArgumentException(
                    "The graphics-card Build Kit must own the canonical placement surface.",
                    nameof(placementSurface));
            }

            ResetFeedback();
            RefreshPresentation();
        }

        public GraphicsCardBuildKitEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection graphicsCard,
            LayerMask obstructionMask,
            int clockwiseHalfTurns,
            bool paused,
            bool authorityAvailable)
        {
            Pose candidate = ResolveSnapPose(clockwiseHalfTurns);
            HasContextualAttention = false;
            IsFocused = false;

            if (!IsConfigured || interactionOrigin == null || graphicsCard == null)
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.ContextMissing,
                    candidate,
                    snapAnchor != null));
            }

            if (paused)
            {
                return Remember(Invalid(GraphicsCardBuildKitStatus.Paused, candidate));
            }

            if (!HasMotherboardPrerequisite ||
                !HasProcessorPrerequisite ||
                !HasMemoryModulePrerequisite ||
                !HasStoragePrerequisite ||
                !HasProcessorCoolerPrerequisite)
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.PrerequisiteMissing,
                    candidate));
            }

            if (!authorityAvailable)
            {
                return Remember(Invalid(
                    IsStaged
                        ? GraphicsCardBuildKitStatus.AlreadyStaged
                        : GraphicsCardBuildKitStatus.AuthorityBlocked,
                    candidate));
            }

            if (IsStaged)
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.AlreadyStaged,
                    candidate));
            }

            Collider support = surface.SurfaceCollider;
            Vector3 focusPoint = new Vector3(
                support.bounds.center.x,
                support.bounds.max.y,
                support.bounds.center.z);
            Vector3 toFocus = focusPoint - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon || distance > maximumRange)
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.OutOfRange,
                    candidate));
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) < minimumFocusDot)
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.NotFocused,
                    candidate));
            }

            HasContextualAttention = true;
            if (!HasLineOfSight(
                    interactionOrigin.position,
                    direction,
                    distance,
                    support,
                    graphicsCard,
                    playerRoot,
                    obstructionMask))
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.LineOfSightBlocked,
                    candidate));
            }

            IsFocused = true;
            if (!HasExactFullSupport(candidate))
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.OutsideSurface,
                    candidate));
            }

            if (HasObstruction(
                    candidate,
                    graphicsCard,
                    playerRoot,
                    obstructionMask))
            {
                return Remember(Invalid(
                    GraphicsCardBuildKitStatus.Obstructed,
                    candidate));
            }

            return Remember(new GraphicsCardBuildKitEvaluation(
                GraphicsCardBuildKitStatus.Valid,
                candidate,
                true));
        }

        public Pose ResolveSnapPose(int clockwiseHalfTurns)
        {
            if (snapAnchor == null)
            {
                return default;
            }

            int normalizedTurns = NormalizeHalfTurns(clockwiseHalfTurns);
            Quaternion rotation = snapAnchor.rotation *
                                  Quaternion.AngleAxis(
                                      normalizedTurns * RotationStepDegrees,
                                      Vector3.forward);
            return new Pose(snapAnchor.position, rotation);
        }

        public bool MatchesCommittedPlacement(PhysicalItemProjection graphicsCard)
        {
            if (!IsConfigured || graphicsCard == null ||
                graphicsCard.Ownership != PhysicalItemOwnership.World ||
                !graphicsCard.IsStablePlacement ||
                Vector3.Distance(graphicsCard.transform.position, snapAnchor.position) > 0.003f)
            {
                return false;
            }

            for (int turns = 0; turns < 2; turns++)
            {
                if (Quaternion.Angle(
                        graphicsCard.transform.rotation,
                        ResolveSnapPose(turns).rotation) <= 0.25f)
                {
                    return true;
                }
            }

            return false;
        }

        public void RefreshPresentation()
        {
            if (progressText == null)
            {
                return;
            }

            GarageStockFlowSession session = Session;
            if (session == null || !session.TryGetPrototypeCustomPcBuildOrder(out _))
            {
                progressText.text = "EKRAN KARTI BUILD KIT\nİŞ EMRİ BEKLİYOR";
                return;
            }

            if (!HasMotherboardPrerequisite ||
                !HasProcessorPrerequisite ||
                !HasMemoryModulePrerequisite ||
                !HasStoragePrerequisite ||
                !HasProcessorCoolerPrerequisite)
            {
                string prerequisite = !HasMotherboardPrerequisite
                    ? "ÖNCE ANAKART"
                    : !HasProcessorPrerequisite
                        ? "ÖNCE İŞLEMCİ"
                        : !HasMemoryModulePrerequisite
                            ? "ÖNCE BELLEK"
                            : !HasStoragePrerequisite
                                ? "ÖNCE NVMe"
                                : "ÖNCE SOĞUTUCU";
                progressText.text =
                    $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\n" +
                    prerequisite;
                return;
            }

            progressText.text = IsStaged
                ? $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nGPU HAZIR"
                : $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nGPU BEKLİYOR";
        }

        public void ResetFeedback()
        {
            IsFocused = false;
            HasContextualAttention = false;
            LastEvaluation = new GraphicsCardBuildKitEvaluation(
                GraphicsCardBuildKitStatus.Uninitialized,
                ResolveSnapPose(0),
                snapAnchor != null);
        }

        private bool HasExactFullSupport(Pose candidate)
        {
            Collider support = surface.SurfaceCollider;
            Quaternion footprintRotation = Quaternion.Euler(
                0f,
                candidate.rotation.eulerAngles.y,
                0f);
            Vector3 right = footprintRotation * Vector3.right * FootprintInsetX;
            Vector3 forward = footprintRotation * Vector3.forward * FootprintInsetZ;
            Vector3[] offsets =
            {
                Vector3.zero,
                right + forward,
                right - forward,
                -right + forward,
                -right - forward
            };

            foreach (Vector3 offset in offsets)
            {
                Ray ray = new Ray(
                    candidate.position + offset + (Vector3.up * SupportProbeHeight),
                    Vector3.down);
                if (!support.Raycast(ray, out _, SupportProbeDistance))
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasObstruction(
            Pose candidate,
            PhysicalItemProjection graphicsCard,
            Transform playerRoot,
            LayerMask obstructionMask)
        {
            Collider support = surface.SurfaceCollider;
            const float halfHeight = 0.032f;
            Vector3 center = new Vector3(
                candidate.position.x,
                support.bounds.max.y + halfHeight,
                candidate.position.z);
            Quaternion footprintRotation = Quaternion.Euler(
                0f,
                candidate.rotation.eulerAngles.y,
                0f);
            int count = Physics.OverlapBoxNonAlloc(
                center,
                new Vector3(0.145f, halfHeight, 0.065f),
                Overlaps,
                footprintRotation,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (count >= HitCapacity)
            {
                return true;
            }

            for (int index = 0; index < count; index++)
            {
                Collider overlap = Overlaps[index];
                if (overlap == null || overlap == support ||
                    IsChildOf(overlap.transform, graphicsCard.transform) ||
                    IsChildOf(overlap.transform, playerRoot))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private static bool HasLineOfSight(
            Vector3 origin,
            Vector3 direction,
            float distance,
            Collider expectedSupport,
            PhysicalItemProjection graphicsCard,
            Transform playerRoot,
            LayerMask obstructionMask)
        {
            int supportLayerMask = 1 << expectedSupport.gameObject.layer;
            int count = Physics.RaycastNonAlloc(
                origin,
                direction,
                LineHits,
                distance + 0.04f,
                obstructionMask | supportLayerMask,
                QueryTriggerInteraction.Ignore);
            if (count <= 0 || count >= HitCapacity)
            {
                return false;
            }

            Collider nearest = null;
            float nearestDistance = float.PositiveInfinity;
            for (int index = 0; index < count; index++)
            {
                RaycastHit hit = LineHits[index];
                if (hit.collider == null ||
                    IsChildOf(hit.collider.transform, graphicsCard.transform) ||
                    IsChildOf(hit.collider.transform, playerRoot))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearest = hit.collider;
                    nearestDistance = hit.distance;
                }
            }

            return nearest == expectedSupport;
        }

        private GraphicsCardBuildKitEvaluation Remember(
            GraphicsCardBuildKitEvaluation evaluation)
        {
            LastEvaluation = evaluation;
            return evaluation;
        }

        private static GraphicsCardBuildKitEvaluation Invalid(
            GraphicsCardBuildKitStatus status,
            Pose pose,
            bool hasPose = true)
        {
            return new GraphicsCardBuildKitEvaluation(status, pose, hasPose);
        }

        private static int NormalizeHalfTurns(int turns)
        {
            return ((turns % 4) + 4) % 4;
        }

        private static bool IsChildOf(Transform candidate, Transform root)
        {
            return candidate != null && root != null && candidate.IsChildOf(root);
        }

        private void LateUpdate()
        {
            RefreshPresentation();
        }

        private void OnValidate()
        {
            maximumRange = Mathf.Max(0.1f, maximumRange);
            minimumFocusDot = Mathf.Clamp01(minimumFocusDot);
        }
    }
}
