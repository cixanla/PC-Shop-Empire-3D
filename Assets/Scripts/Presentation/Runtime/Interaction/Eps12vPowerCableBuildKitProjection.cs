using System;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed class Eps12vPowerCableBuildKitProjectionIdScope : IStableIdScope
    {
    }

    public enum Eps12vPowerCableBuildKitStatus
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

    public readonly struct Eps12vPowerCableBuildKitEvaluation
    {
        public Eps12vPowerCableBuildKitEvaluation(
            Eps12vPowerCableBuildKitStatus status,
            Pose pose,
            bool hasPose)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
        }

        public Eps12vPowerCableBuildKitStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public bool IsValid => Status == Eps12vPowerCableBuildKitStatus.Valid && HasPose;

        public string FailureCode => Status switch
        {
            Eps12vPowerCableBuildKitStatus.ContextMissing =>
                "custom-pc-eps12v-power-cable-build-kit.context-missing",
            Eps12vPowerCableBuildKitStatus.Paused =>
                "custom-pc-eps12v-power-cable-build-kit.paused",
            Eps12vPowerCableBuildKitStatus.AuthorityBlocked =>
                "custom-pc-eps12v-power-cable-build-kit.authority-blocked",
            Eps12vPowerCableBuildKitStatus.PrerequisiteMissing =>
                "custom-pc-eps12v-power-cable-build-kit.prerequisite-missing",
            Eps12vPowerCableBuildKitStatus.AlreadyStaged =>
                "custom-pc-eps12v-power-cable-build-kit.already-staged",
            Eps12vPowerCableBuildKitStatus.OutOfRange =>
                "custom-pc-eps12v-power-cable-build-kit.out-of-range",
            Eps12vPowerCableBuildKitStatus.NotFocused =>
                "custom-pc-eps12v-power-cable-build-kit.focus-missing",
            Eps12vPowerCableBuildKitStatus.LineOfSightBlocked =>
                "custom-pc-eps12v-power-cable-build-kit.line-of-sight-blocked",
            Eps12vPowerCableBuildKitStatus.Unsupported =>
                "custom-pc-eps12v-power-cable-build-kit.unsupported",
            Eps12vPowerCableBuildKitStatus.OutsideSurface =>
                "custom-pc-eps12v-power-cable-build-kit.outside-surface",
            Eps12vPowerCableBuildKitStatus.Obstructed =>
                "custom-pc-eps12v-power-cable-build-kit.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Fail-closed physical projection for the reserved EPS12V CPU power cable.
    /// Domain authorities own custody; this projection owns only focus, keyed
    /// 180-degree staging geometry and visible 8/10 to 9/10 progress.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(66)]
    public sealed class Eps12vPowerCableBuildKitProjection : MonoBehaviour
    {
        public const string PrototypeProjectionIdValue =
            "world.eps12v-power-cable-build-kit.garage-001";
        public const string PrototypeSurfaceIdValue =
            "world.eps12v-power-cable-build-kit-surface.garage-001";
        public const int PrototypeTotalComponentCount = 10;
        public const float DefaultInteractionRange = 2.25f;
        public const float DefaultMinimumFocusDot = 0.92f;

        private const int HitCapacity = 32;
        private const float RotationStepDegrees = 180f;
        private const float SupportProbeHeight = 0.04f;
        private const float SupportProbeDistance = 0.08f;
        private const float FootprintInsetX = 0.0675f;
        private const float FootprintInsetZ = 0.0625f;
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

        public StableId<Eps12vPowerCableBuildKitProjectionIdScope> ProjectionId =>
            StableId<Eps12vPowerCableBuildKitProjectionIdScope>.Parse(projectionId);

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

        public Eps12vPowerCableBuildKitEvaluation LastEvaluation { get; private set; }

        public GarageStockFlowSession Session => runtime != null
            ? runtime.EnsureInitialized()
            : null;

        public int StagedComponentCount =>
            Session?.CustomPcBuildKit?.StagedComponentCount ?? 0;

        public bool HasMotherboardPrerequisite => HasStage(
            Session?.PrototypeCustomPcBuildKitOperationId ?? default,
            CustomPcBuildKitStage.MotherboardStaged);

        public bool HasProcessorPrerequisite => HasStage(
            Session?.PrototypeProcessorBuildKitOperationId ?? default,
            CustomPcBuildKitStage.ProcessorStaged);

        public bool HasMemoryModulePrerequisite => HasStage(
            Session?.PrototypeMemoryModuleBuildKitOperationId ?? default,
            CustomPcBuildKitStage.MemoryModuleStaged);

        public bool HasStoragePrerequisite => HasStage(
            Session?.PrototypeStorageBuildKitOperationId ?? default,
            CustomPcBuildKitStage.StorageStaged);

        public bool HasProcessorCoolerPrerequisite => HasStage(
            Session?.PrototypeProcessorCoolerBuildKitOperationId ?? default,
            CustomPcBuildKitStage.ProcessorCoolerStaged);

        public bool HasGraphicsCardPrerequisite => HasStage(
            Session?.PrototypeGraphicsCardBuildKitOperationId ?? default,
            CustomPcBuildKitStage.GraphicsCardStaged);

        public bool HasPowerSupplyPrerequisite => HasStage(
            Session?.PrototypePowerSupplyBuildKitOperationId ?? default,
            CustomPcBuildKitStage.PowerSupplyStaged);

        public bool HasAtx24PowerCablePrerequisite => HasStage(
            Session?.PrototypeAtx24PowerCableBuildKitOperationId ?? default,
            CustomPcBuildKitStage.Atx24PowerCableStaged);

        public bool IsStaged => HasStage(
            Session?.PrototypeEps12vPowerCableBuildKitOperationId ?? default,
            CustomPcBuildKitStage.Eps12vPowerCableStaged);

        public bool HasPickupReceipt => HasStage(
            Session?.PrototypeEps12vPowerCableBuildKitOperationId ?? default,
            CustomPcBuildKitStage.Eps12vPowerCableInHands);

        public bool IsReleasedForAssembly
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetAssemblyHandoff(
                           session.PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                           out CustomPcBuildKitAssemblyHandoffReceipt receipt) &&
                       receipt.ComponentKind == PcComponentKind.PowerCable &&
                       receipt.Line.PowerCableType ==
                           PowerCableType.ModularEps12v8PinPsuToMotherboard &&
                       receipt.WorkbenchContainerId ==
                           session.Eps12vPowerCableRouteContainerId;
            }
        }

        public bool HasAllPrerequisites =>
            HasMotherboardPrerequisite &&
            HasProcessorPrerequisite &&
            HasMemoryModulePrerequisite &&
            HasStoragePrerequisite &&
            HasProcessorCoolerPrerequisite &&
            HasGraphicsCardPrerequisite &&
            HasPowerSupplyPrerequisite &&
            HasAtx24PowerCablePrerequisite;

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
            projectionId = StableId<Eps12vPowerCableBuildKitProjectionIdScope>.Parse(
                stableProjectionId).Value;
            maximumRange = Mathf.Max(0.1f, interactionRange);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            if (surface.SurfaceId != PrototypeSurfaceIdValue)
            {
                throw new ArgumentException(
                    "The EPS12V Build Kit must own the canonical placement surface.",
                    nameof(placementSurface));
            }

            ResetFeedback();
            RefreshPresentation();
        }

        public Eps12vPowerCableBuildKitEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            LayerMask obstructionMask,
            int clockwiseHalfTurns,
            bool paused,
            bool authorityAvailable)
        {
            Pose candidate = ResolveSnapPose(clockwiseHalfTurns);
            HasContextualAttention = false;
            IsFocused = false;
            if (!IsConfigured || interactionOrigin == null || cable == null)
            {
                return Remember(Invalid(
                    Eps12vPowerCableBuildKitStatus.ContextMissing,
                    candidate,
                    snapAnchor != null));
            }

            if (paused)
            {
                return Remember(Invalid(
                    Eps12vPowerCableBuildKitStatus.Paused,
                    candidate));
            }

            if (!HasAllPrerequisites)
            {
                return Remember(Invalid(
                    Eps12vPowerCableBuildKitStatus.PrerequisiteMissing,
                    candidate));
            }

            if (!authorityAvailable || IsStaged)
            {
                return Remember(Invalid(
                    IsStaged
                        ? Eps12vPowerCableBuildKitStatus.AlreadyStaged
                        : Eps12vPowerCableBuildKitStatus.AuthorityBlocked,
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
                    Eps12vPowerCableBuildKitStatus.OutOfRange,
                    candidate));
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) < minimumFocusDot)
            {
                return Remember(Invalid(
                    Eps12vPowerCableBuildKitStatus.NotFocused,
                    candidate));
            }

            HasContextualAttention = true;
            if (!HasLineOfSight(
                    interactionOrigin.position,
                    direction,
                    distance,
                    support,
                    cable,
                    playerRoot,
                    obstructionMask))
            {
                return Remember(Invalid(
                    Eps12vPowerCableBuildKitStatus.LineOfSightBlocked,
                    candidate));
            }

            IsFocused = true;
            if (!HasExactFullSupport(candidate))
            {
                return Remember(Invalid(
                    Eps12vPowerCableBuildKitStatus.OutsideSurface,
                    candidate));
            }

            if (HasObstruction(candidate, cable, playerRoot, obstructionMask))
            {
                return Remember(Invalid(
                    Eps12vPowerCableBuildKitStatus.Obstructed,
                    candidate));
            }

            return Remember(new Eps12vPowerCableBuildKitEvaluation(
                Eps12vPowerCableBuildKitStatus.Valid,
                candidate,
                true));
        }

        public Pose ResolveSnapPose(int clockwiseHalfTurns)
        {
            if (snapAnchor == null)
            {
                return default;
            }

            int normalizedTurns = ((clockwiseHalfTurns % 2) + 2) % 2;
            return new Pose(
                snapAnchor.position,
                snapAnchor.rotation * Quaternion.AngleAxis(
                    normalizedTurns * RotationStepDegrees,
                    Vector3.forward));
        }

        public bool MatchesCommittedPlacement(PhysicalItemProjection cable)
        {
            if (!IsConfigured || cable == null ||
                cable.Ownership != PhysicalItemOwnership.World ||
                !cable.IsStablePlacement ||
                Vector3.Distance(cable.transform.position, snapAnchor.position) > 0.003f)
            {
                return false;
            }

            for (int turns = 0; turns < 2; turns++)
            {
                if (Quaternion.Angle(
                        cable.transform.rotation,
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
                progressText.text = "EPS12V BUILD KIT\nİŞ EMRİ BEKLİYOR";
                return;
            }

            if (!HasAllPrerequisites)
            {
                string prerequisite = !HasMotherboardPrerequisite
                    ? "ÖNCE ANAKART"
                    : !HasProcessorPrerequisite
                        ? "ÖNCE İŞLEMCİ"
                        : !HasMemoryModulePrerequisite
                            ? "ÖNCE BELLEK"
                            : !HasStoragePrerequisite
                                ? "ÖNCE NVMe"
                                : !HasProcessorCoolerPrerequisite
                                    ? "ÖNCE SOĞUTUCU"
                                    : !HasGraphicsCardPrerequisite
                                        ? "ÖNCE GPU"
                                        : !HasPowerSupplyPrerequisite
                                            ? "ÖNCE PSU"
                                            : "ÖNCE ATX24";
                progressText.text =
                    $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\n" +
                    prerequisite;
                return;
            }

            progressText.text = IsReleasedForAssembly
                ? $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nEPS12V MONTAJDA"
                : IsStaged
                    ? $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nEPS12V HAZIR"
                    : $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nEPS12V BEKLİYOR";
        }

        public void ResetFeedback()
        {
            IsFocused = false;
            HasContextualAttention = false;
            LastEvaluation = new Eps12vPowerCableBuildKitEvaluation(
                Eps12vPowerCableBuildKitStatus.Uninitialized,
                ResolveSnapPose(0),
                snapAnchor != null);
        }

        private bool HasStage(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildKitStage stage)
        {
            GarageStockFlowSession session = Session;
            return session?.CustomPcBuildKit != null &&
                   !operationId.IsEmpty &&
                   session.CustomPcBuildKit.TryGetReceipt(
                       operationId,
                       out CustomPcBuildKitReceipt receipt) &&
                   receipt.Stage == stage;
        }

        private bool HasExactFullSupport(Pose candidate)
        {
            Collider support = surface.SurfaceCollider;
            Quaternion rotation = Quaternion.Euler(
                0f,
                candidate.rotation.eulerAngles.y,
                0f);
            Vector3 right = rotation * Vector3.right * FootprintInsetX;
            Vector3 forward = rotation * Vector3.forward * FootprintInsetZ;
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
                    candidate.position + offset + Vector3.up * SupportProbeHeight,
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
            PhysicalItemProjection cable,
            Transform playerRoot,
            LayerMask obstructionMask)
        {
            Collider support = surface.SurfaceCollider;
            const float halfHeight = 0.041f;
            Vector3 center = new Vector3(
                candidate.position.x,
                support.bounds.max.y + halfHeight,
                candidate.position.z);
            Quaternion rotation = Quaternion.Euler(
                0f,
                candidate.rotation.eulerAngles.y,
                0f);
            int count = Physics.OverlapBoxNonAlloc(
                center,
                new Vector3(0.073f, halfHeight, 0.068f),
                Overlaps,
                rotation,
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
                    IsChildOf(overlap.transform, cable.transform) ||
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
            PhysicalItemProjection cable,
            Transform playerRoot,
            LayerMask obstructionMask)
        {
            int count = Physics.RaycastNonAlloc(
                origin,
                direction,
                LineHits,
                distance + 0.04f,
                obstructionMask | (1 << expectedSupport.gameObject.layer),
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
                    IsChildOf(hit.collider.transform, cable.transform) ||
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

        private Eps12vPowerCableBuildKitEvaluation Remember(
            Eps12vPowerCableBuildKitEvaluation evaluation)
        {
            LastEvaluation = evaluation;
            return evaluation;
        }

        private static Eps12vPowerCableBuildKitEvaluation Invalid(
            Eps12vPowerCableBuildKitStatus status,
            Pose pose,
            bool hasPose = true)
        {
            return new Eps12vPowerCableBuildKitEvaluation(status, pose, hasPose);
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
