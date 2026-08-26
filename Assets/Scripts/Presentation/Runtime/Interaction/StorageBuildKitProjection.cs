using System;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed class StorageBuildKitProjectionIdScope : IStableIdScope
    {
    }

    public enum StorageBuildKitStatus
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

    public readonly struct StorageBuildKitEvaluation
    {
        public StorageBuildKitEvaluation(
            StorageBuildKitStatus status,
            Pose pose,
            bool hasPose)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
        }

        public StorageBuildKitStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public bool IsValid => Status == StorageBuildKitStatus.Valid && HasPose;

        public string FailureCode => Status switch
        {
            StorageBuildKitStatus.ContextMissing =>
                "custom-pc-storage-build-kit.context-missing",
            StorageBuildKitStatus.Paused =>
                "custom-pc-storage-build-kit.paused",
            StorageBuildKitStatus.AuthorityBlocked =>
                "custom-pc-storage-build-kit.authority-blocked",
            StorageBuildKitStatus.PrerequisiteMissing =>
                "custom-pc-storage-build-kit.prerequisite-missing",
            StorageBuildKitStatus.AlreadyStaged =>
                "custom-pc-storage-build-kit.already-staged",
            StorageBuildKitStatus.OutOfRange =>
                "custom-pc-storage-build-kit.out-of-range",
            StorageBuildKitStatus.NotFocused =>
                "custom-pc-storage-build-kit.focus-missing",
            StorageBuildKitStatus.LineOfSightBlocked =>
                "custom-pc-storage-build-kit.line-of-sight-blocked",
            StorageBuildKitStatus.Unsupported =>
                "custom-pc-storage-build-kit.unsupported",
            StorageBuildKitStatus.OutsideSurface =>
                "custom-pc-storage-build-kit.outside-surface",
            StorageBuildKitStatus.Obstructed =>
                "custom-pc-storage-build-kit.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Dedicated fail-closed physical projection for the reserved custom-PC M.2 NVMe storage device.
    /// Inventory and CustomPcBuildKitAuthority own custody; this component owns only
    /// focus, keyed 180-degree staging geometry and visible 3/10 to 4/10 progress.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(65)]
    public sealed class StorageBuildKitProjection : MonoBehaviour
    {
        public const string PrototypeProjectionIdValue =
            "world.storage-build-kit.garage-001";
        public const string PrototypeSurfaceIdValue =
            "world.storage-build-kit-surface.garage-001";
        public const int PrototypeTotalComponentCount = 10;
        public const float DefaultInteractionRange = 2.25f;
        public const float DefaultMinimumFocusDot = 0.92f;

        private const int HitCapacity = 32;
        private const float RotationStepDegrees = 180f;
        private const float SupportProbeHeight = 0.04f;
        private const float SupportProbeDistance = 0.08f;
        private const float FootprintInsetX = 0.036f;
        private const float FootprintInsetZ = 0.009f;
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

        public StableId<StorageBuildKitProjectionIdScope> ProjectionId =>
            StableId<StorageBuildKitProjectionIdScope>.Parse(projectionId);

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

        public StorageBuildKitEvaluation LastEvaluation { get; private set; }

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

        public bool IsStaged
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

        public bool HasPickupReceipt
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetReceipt(
                           session.PrototypeStorageBuildKitOperationId,
                           out CustomPcBuildKitReceipt receipt) &&
                       receipt.Stage == CustomPcBuildKitStage.StorageInHands;
            }
        }

        public bool IsReleasedForAssembly
        {
            get
            {
                GarageStockFlowSession session = Session;
                return session?.CustomPcBuildKit != null &&
                       session.CustomPcBuildKit.TryGetAssemblyHandoff(
                           session.PrototypeStorageAssemblyHandoffOperationId,
                           out CustomPcBuildKitAssemblyHandoffReceipt receipt) &&
                       receipt.Line.ComponentKind == PcComponentKind.StorageDevice;
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
            projectionId = StableId<StorageBuildKitProjectionIdScope>.Parse(
                stableProjectionId).Value;
            maximumRange = Mathf.Max(0.1f, interactionRange);
            minimumFocusDot = Mathf.Clamp01(focusDot);

            if (surface.SurfaceId != PrototypeSurfaceIdValue)
            {
                throw new ArgumentException(
                    "The M.2 NVMe storage device Build Kit must own the canonical placement surface.",
                    nameof(placementSurface));
            }

            ResetFeedback();
            RefreshPresentation();
        }

        public StorageBuildKitEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection storageDevice,
            LayerMask obstructionMask,
            int clockwiseHalfTurns,
            bool paused,
            bool authorityAvailable)
        {
            Pose candidate = ResolveSnapPose(clockwiseHalfTurns);
            HasContextualAttention = false;
            IsFocused = false;

            if (!IsConfigured || interactionOrigin == null || storageDevice == null)
            {
                return Remember(Invalid(
                    StorageBuildKitStatus.ContextMissing,
                    candidate,
                    snapAnchor != null));
            }

            if (paused)
            {
                return Remember(Invalid(StorageBuildKitStatus.Paused, candidate));
            }

            if (!HasMotherboardPrerequisite ||
                !HasProcessorPrerequisite ||
                !HasMemoryModulePrerequisite)
            {
                return Remember(Invalid(
                    StorageBuildKitStatus.PrerequisiteMissing,
                    candidate));
            }

            if (!authorityAvailable)
            {
                return Remember(Invalid(
                    IsStaged
                        ? StorageBuildKitStatus.AlreadyStaged
                        : StorageBuildKitStatus.AuthorityBlocked,
                    candidate));
            }

            if (IsStaged)
            {
                return Remember(Invalid(
                    StorageBuildKitStatus.AlreadyStaged,
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
                    StorageBuildKitStatus.OutOfRange,
                    candidate));
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) < minimumFocusDot)
            {
                return Remember(Invalid(
                    StorageBuildKitStatus.NotFocused,
                    candidate));
            }

            HasContextualAttention = true;
            if (!HasLineOfSight(
                    interactionOrigin.position,
                    direction,
                    distance,
                    support,
                    storageDevice,
                    playerRoot,
                    obstructionMask))
            {
                return Remember(Invalid(
                    StorageBuildKitStatus.LineOfSightBlocked,
                    candidate));
            }

            IsFocused = true;
            if (!HasExactFullSupport(candidate))
            {
                return Remember(Invalid(
                    StorageBuildKitStatus.OutsideSurface,
                    candidate));
            }

            if (HasObstruction(
                    candidate,
                    storageDevice,
                    playerRoot,
                    obstructionMask))
            {
                return Remember(Invalid(
                    StorageBuildKitStatus.Obstructed,
                    candidate));
            }

            return Remember(new StorageBuildKitEvaluation(
                StorageBuildKitStatus.Valid,
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

        public bool MatchesCommittedPlacement(PhysicalItemProjection storageDevice)
        {
            if (!IsConfigured || storageDevice == null ||
                storageDevice.Ownership != PhysicalItemOwnership.World ||
                !storageDevice.IsStablePlacement ||
                Vector3.Distance(storageDevice.transform.position, snapAnchor.position) > 0.003f)
            {
                return false;
            }

            for (int turns = 0; turns < 2; turns++)
            {
                if (Quaternion.Angle(
                        storageDevice.transform.rotation,
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
                progressText.text = "M.2 NVMe BUILD KIT\nİŞ EMRİ BEKLİYOR";
                return;
            }

            if (!HasMotherboardPrerequisite ||
                !HasProcessorPrerequisite ||
                !HasMemoryModulePrerequisite)
            {
                string prerequisite = !HasMotherboardPrerequisite
                    ? "ÖNCE ANAKART"
                    : !HasProcessorPrerequisite
                        ? "ÖNCE İŞLEMCİ"
                        : "ÖNCE BELLEK";
                progressText.text =
                    $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\n" +
                    prerequisite;
                return;
            }

            progressText.text = IsReleasedForAssembly
                ? $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nM.2 MONTAJDA"
                : IsStaged
                    ? $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nNVMe HAZIR"
                    : $"BUILD KIT • {StagedComponentCount}/{PrototypeTotalComponentCount}\nNVMe BEKLİYOR";
        }

        public void ResetFeedback()
        {
            IsFocused = false;
            HasContextualAttention = false;
            LastEvaluation = new StorageBuildKitEvaluation(
                StorageBuildKitStatus.Uninitialized,
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
            PhysicalItemProjection storageDevice,
            Transform playerRoot,
            LayerMask obstructionMask)
        {
            Collider support = surface.SurfaceCollider;
            const float halfHeight = 0.006f;
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
                new Vector3(0.043f, halfHeight, 0.014f),
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
                    IsChildOf(overlap.transform, storageDevice.transform) ||
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
            PhysicalItemProjection storageDevice,
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
                    IsChildOf(hit.collider.transform, storageDevice.transform) ||
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

        private StorageBuildKitEvaluation Remember(
            StorageBuildKitEvaluation evaluation)
        {
            LastEvaluation = evaluation;
            return evaluation;
        }

        private static StorageBuildKitEvaluation Invalid(
            StorageBuildKitStatus status,
            Pose pose,
            bool hasPose = true)
        {
            return new StorageBuildKitEvaluation(status, pose, hasPose);
        }

        private static int NormalizeHalfTurns(int turns)
        {
            return ((turns % 2) + 2) % 2;
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
