using System;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed class ProcessorBuildKitProjectionIdScope : IStableIdScope
    {
    }

    public enum ProcessorBuildKitStatus
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

    public readonly struct ProcessorBuildKitEvaluation
    {
        public ProcessorBuildKitEvaluation(
            ProcessorBuildKitStatus status,
            Pose pose,
            bool hasPose)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
        }

        public ProcessorBuildKitStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public bool IsValid => Status == ProcessorBuildKitStatus.Valid && HasPose;

        public string FailureCode => Status switch
        {
            ProcessorBuildKitStatus.ContextMissing =>
                "custom-pc-processor-build-kit.context-missing",
            ProcessorBuildKitStatus.Paused =>
                "custom-pc-processor-build-kit.paused",
            ProcessorBuildKitStatus.AuthorityBlocked =>
                "custom-pc-processor-build-kit.authority-blocked",
            ProcessorBuildKitStatus.PrerequisiteMissing =>
                "custom-pc-processor-build-kit.prerequisite-missing",
            ProcessorBuildKitStatus.AlreadyStaged =>
                "custom-pc-processor-build-kit.already-staged",
            ProcessorBuildKitStatus.OutOfRange =>
                "custom-pc-processor-build-kit.out-of-range",
            ProcessorBuildKitStatus.NotFocused =>
                "custom-pc-processor-build-kit.focus-missing",
            ProcessorBuildKitStatus.LineOfSightBlocked =>
                "custom-pc-processor-build-kit.line-of-sight-blocked",
            ProcessorBuildKitStatus.Unsupported =>
                "custom-pc-processor-build-kit.unsupported",
            ProcessorBuildKitStatus.OutsideSurface =>
                "custom-pc-processor-build-kit.outside-surface",
            ProcessorBuildKitStatus.Obstructed =>
                "custom-pc-processor-build-kit.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Dedicated fail-closed physical projection for the reserved custom-PC processor.
    /// Inventory and CustomPcBuildKitAuthority own custody; this component owns only
    /// focus, exact staging geometry and visible 1/10 to 2/10 progress.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(65)]
    public sealed class ProcessorBuildKitProjection : MonoBehaviour
    {
        public const string PrototypeProjectionIdValue =
            "world.processor-build-kit.garage-001";
        public const string PrototypeSurfaceIdValue =
            "world.processor-build-kit-surface.garage-001";
        public const int PrototypeTotalComponentCount = 10;
        public const float DefaultInteractionRange = 2.25f;
        public const float DefaultMinimumFocusDot = 0.92f;

        private const int HitCapacity = 32;
        private const float RotationStepDegrees = 90f;
        private const float SupportProbeHeight = 0.04f;
        private const float SupportProbeDistance = 0.08f;
        private const float FootprintInsetX = 0.024f;
        private const float FootprintInsetZ = 0.021f;
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

        public StableId<ProcessorBuildKitProjectionIdScope> ProjectionId =>
            StableId<ProcessorBuildKitProjectionIdScope>.Parse(projectionId);

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

        public ProcessorBuildKitEvaluation LastEvaluation { get; private set; }

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

        public bool IsStaged
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

        public bool HasPickupReceipt
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeProcessorBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.ProcessorInHands;
            }
        }

        public bool IsReleasedForAssembly
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetAssemblyHandoff(
                           session.PrototypeProcessorAssemblyHandoffOperationId,
                           out CustomPcBuildKitAssemblyHandoffReceipt receipt) &&
                       receipt.Line.ComponentKind == PcComponentKind.Processor;
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
            projectionId = StableId<ProcessorBuildKitProjectionIdScope>.Parse(
                stableProjectionId).Value;
            maximumRange = Mathf.Max(0.1f, interactionRange);
            minimumFocusDot = Mathf.Clamp01(focusDot);

            if (surface.SurfaceId != PrototypeSurfaceIdValue)
            {
                throw new ArgumentException(
                    "The processor Build Kit must own the canonical placement surface.",
                    nameof(placementSurface));
            }

            ResetFeedback();
            RefreshPresentation();
        }

        public ProcessorBuildKitEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection processor,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns,
            bool paused,
            bool authorityAvailable)
        {
            Pose candidate = ResolveSnapPose(clockwiseQuarterTurns);
            HasContextualAttention = false;
            IsFocused = false;

            if (!IsConfigured || interactionOrigin == null || processor == null)
            {
                return Remember(Invalid(
                    ProcessorBuildKitStatus.ContextMissing,
                    candidate,
                    snapAnchor != null));
            }

            if (paused)
            {
                return Remember(Invalid(ProcessorBuildKitStatus.Paused, candidate));
            }

            if (!HasMotherboardPrerequisite)
            {
                return Remember(Invalid(
                    ProcessorBuildKitStatus.PrerequisiteMissing,
                    candidate));
            }

            if (!authorityAvailable)
            {
                return Remember(Invalid(
                    IsStaged
                        ? ProcessorBuildKitStatus.AlreadyStaged
                        : ProcessorBuildKitStatus.AuthorityBlocked,
                    candidate));
            }

            if (IsStaged)
            {
                return Remember(Invalid(
                    ProcessorBuildKitStatus.AlreadyStaged,
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
                    ProcessorBuildKitStatus.OutOfRange,
                    candidate));
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) < minimumFocusDot)
            {
                return Remember(Invalid(
                    ProcessorBuildKitStatus.NotFocused,
                    candidate));
            }

            HasContextualAttention = true;
            if (!HasLineOfSight(
                    interactionOrigin.position,
                    direction,
                    distance,
                    support,
                    processor,
                    playerRoot,
                    obstructionMask))
            {
                return Remember(Invalid(
                    ProcessorBuildKitStatus.LineOfSightBlocked,
                    candidate));
            }

            IsFocused = true;
            if (!HasExactFullSupport(candidate))
            {
                return Remember(Invalid(
                    ProcessorBuildKitStatus.OutsideSurface,
                    candidate));
            }

            if (HasObstruction(
                    candidate,
                    processor,
                    playerRoot,
                    obstructionMask))
            {
                return Remember(Invalid(
                    ProcessorBuildKitStatus.Obstructed,
                    candidate));
            }

            return Remember(new ProcessorBuildKitEvaluation(
                ProcessorBuildKitStatus.Valid,
                candidate,
                true));
        }

        public Pose ResolveSnapPose(int clockwiseQuarterTurns)
        {
            if (snapAnchor == null)
            {
                return default;
            }

            int normalizedTurns = NormalizeQuarterTurns(clockwiseQuarterTurns);
            Quaternion rotation = snapAnchor.rotation *
                                  Quaternion.AngleAxis(
                                      normalizedTurns * RotationStepDegrees,
                                      Vector3.forward);
            return new Pose(snapAnchor.position, rotation);
        }

        public bool MatchesCommittedPlacement(PhysicalItemProjection processor)
        {
            if (!IsConfigured || processor == null ||
                processor.Ownership != PhysicalItemOwnership.World ||
                !processor.IsStablePlacement ||
                Vector3.Distance(processor.transform.position, snapAnchor.position) > 0.003f)
            {
                return false;
            }

            for (int turns = 0; turns < 4; turns++)
            {
                if (Quaternion.Angle(
                        processor.transform.rotation,
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
                progressText.text = "CPU BUILD KIT\nİŞ EMRİ BEKLİYOR";
                return;
            }

            if (!HasMotherboardPrerequisite)
            {
                progressText.text =
                    $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\n" +
                    "ÖNCE ANAKART";
                return;
            }

            progressText.text = IsReleasedForAssembly
                ? $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nCPU MONTAJDA"
                : IsStaged
                    ? $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nİŞLEMCİ HAZIR"
                    : $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nİŞLEMCİ BEKLİYOR";
        }

        public void ResetFeedback()
        {
            IsFocused = false;
            HasContextualAttention = false;
            LastEvaluation = new ProcessorBuildKitEvaluation(
                ProcessorBuildKitStatus.Uninitialized,
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
            PhysicalItemProjection processor,
            Transform playerRoot,
            LayerMask obstructionMask)
        {
            Collider support = surface.SurfaceCollider;
            const float halfHeight = 0.012f;
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
                new Vector3(0.029f, halfHeight, 0.026f),
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
                    IsChildOf(overlap.transform, processor.transform) ||
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
            PhysicalItemProjection processor,
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
                    IsChildOf(hit.collider.transform, processor.transform) ||
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

        private ProcessorBuildKitEvaluation Remember(
            ProcessorBuildKitEvaluation evaluation)
        {
            LastEvaluation = evaluation;
            return evaluation;
        }

        private static ProcessorBuildKitEvaluation Invalid(
            ProcessorBuildKitStatus status,
            Pose pose,
            bool hasPose = true)
        {
            return new ProcessorBuildKitEvaluation(status, pose, hasPose);
        }

        private static int NormalizeQuarterTurns(int turns)
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
