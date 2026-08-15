using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.World.Interaction
{
    public enum PlacementStatus
    {
        Valid = 0,
        ContextMissing = 1,
        NoSupport = 2,
        SurfaceNotAllowed = 3,
        SurfaceTooSteep = 4,
        OutsideSurface = 5,
        Blocked = 6,
        StackSupportUnavailable = 7
    }

    public readonly struct PlacementEvaluation
    {
        public PlacementEvaluation(
            PlacementStatus status,
            Pose pose,
            bool hasPose,
            PhysicalItemProjection stackSupport = null)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
            StackSupport = stackSupport;
        }

        public PlacementStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public PhysicalItemProjection StackSupport { get; }

        public bool IsValid => Status == PlacementStatus.Valid;

        public string FailureCode => Status switch
        {
            PlacementStatus.ContextMissing => "placement.context-missing",
            PlacementStatus.NoSupport => "placement.no-support",
            PlacementStatus.SurfaceNotAllowed => "placement.surface-not-allowed",
            PlacementStatus.SurfaceTooSteep => "placement.surface-too-steep",
            PlacementStatus.OutsideSurface => "placement.outside-surface",
            PlacementStatus.Blocked => "placement.blocked",
            PlacementStatus.StackSupportUnavailable => "placement.stack-support-unavailable",
            _ => string.Empty
        };
    }

    public static class PlacementSolver
    {
        private const float SurfaceClearance = 0.025f;
        private const float MinimumUpDot = 0.92f;
        private const float ProbeHeight = 1.2f;
        private const float ProbeDistance = 3f;
        private const float ClockwiseRotationStepDegrees = 90f;
        private const int SupportHitCapacity = 16;
        private static readonly float[] CandidateDistances = { 1.15f, 0.9f, 0.7f };
        private static readonly RaycastHit[] SupportHits = new RaycastHit[SupportHitCapacity];

        public static PlacementEvaluation Evaluate(
            Transform origin,
            PhysicalItemProjection item,
            LayerMask supportMask,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns = 0,
            LayerMask stackSupportMask = default)
        {
            if (origin == null || item == null)
            {
                return new PlacementEvaluation(PlacementStatus.ContextMissing, default, false);
            }

            Vector3 forward = Vector3.ProjectOnPlane(origin.forward, Vector3.up).normalized;
            if (forward.sqrMagnitude < 0.99f)
            {
                forward = Vector3.forward;
            }

            Vector3 halfExtents = item.DropHalfExtents;
            int normalizedQuarterTurns = ((clockwiseQuarterTurns % 4) + 4) % 4;
            float requestedYaw = origin.eulerAngles.y +
                                 (normalizedQuarterTurns * ClockwiseRotationStepDegrees);
            Quaternion requestedRotation = Quaternion.Euler(0f, requestedYaw, 0f);
            Pose fallbackPose = new Pose(
                origin.position + (forward * CandidateDistances[0]) + (Vector3.up * halfExtents.y),
                requestedRotation);
            PlacementEvaluation? firstInvalid = null;
            LayerMask placementSupportMask = supportMask | stackSupportMask;

            foreach (float distance in CandidateDistances)
            {
                Vector3 rayStart = origin.position + (forward * distance) + (Vector3.up * ProbeHeight);
                if (!TryFindPlacementSupport(
                        rayStart,
                        supportMask,
                        stackSupportMask,
                        out RaycastHit support))
                {
                    continue;
                }

                PlacementSurface surface = support.collider.GetComponentInParent<PlacementSurface>();
                PhysicalItemProjection stackSupport =
                    support.collider.GetComponentInParent<PhysicalItemProjection>();
                Pose unsnappedPose = new Pose(
                    support.point + (Vector3.up * (halfExtents.y + SurfaceClearance)),
                    requestedRotation);

                if (Vector3.Dot(support.normal, Vector3.up) < MinimumUpDot)
                {
                    firstInvalid ??= new PlacementEvaluation(
                        PlacementStatus.SurfaceTooSteep,
                        unsnappedPose,
                        true);
                    continue;
                }

                bool hasSurface = surface != null &&
                                  surface.SurfaceCollider != null &&
                                  surface.SurfaceCollider.enabled;
                bool hasStackSupport = stackSupport != null &&
                                       IsInLayerMask(stackSupport.gameObject.layer, stackSupportMask);
                if (!hasSurface && !hasStackSupport)
                {
                    firstInvalid ??= new PlacementEvaluation(
                        PlacementStatus.SurfaceNotAllowed,
                        unsnappedPose,
                        true);
                    continue;
                }

                if (hasStackSupport && !stackSupport.CanAcceptStackedItem(item))
                {
                    firstInvalid ??= new PlacementEvaluation(
                        PlacementStatus.StackSupportUnavailable,
                        unsnappedPose,
                        true,
                        stackSupport);
                    continue;
                }

                Vector3 snappedSurfacePoint = hasSurface
                    ? surface.SnapPoint(support.point)
                    : new Vector3(
                        stackSupport.transform.position.x,
                        support.point.y,
                        stackSupport.transform.position.z);
                Quaternion snappedRotation = hasSurface
                    ? surface.SnapRotation(requestedRotation)
                    : SnapStackRotation(stackSupport.transform.rotation, requestedRotation);
                Pose snappedPose = new Pose(
                    snappedSurfacePoint + (Vector3.up * (halfExtents.y + SurfaceClearance)),
                    snappedRotation);

                if (!HasFullSupport(
                        snappedPose,
                        halfExtents,
                        hasSurface ? surface : null,
                        hasStackSupport ? stackSupport : null,
                        placementSupportMask))
                {
                    firstInvalid ??= new PlacementEvaluation(
                        PlacementStatus.OutsideSurface,
                        snappedPose,
                        true);
                    continue;
                }

                if (Physics.CheckBox(
                        snappedPose.position,
                        halfExtents * 0.94f,
                        snappedPose.rotation,
                        obstructionMask,
                        QueryTriggerInteraction.Ignore))
                {
                    firstInvalid ??= new PlacementEvaluation(
                        PlacementStatus.Blocked,
                        snappedPose,
                        true);
                    continue;
                }

                return new PlacementEvaluation(
                    PlacementStatus.Valid,
                    snappedPose,
                    true,
                    hasStackSupport ? stackSupport : null);
            }

            return firstInvalid ?? new PlacementEvaluation(
                PlacementStatus.NoSupport,
                fallbackPose,
                true);
        }

        public static OperationResult<Pose> FindPose(
            Transform origin,
            PhysicalItemProjection item,
            LayerMask supportMask,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns = 0,
            LayerMask stackSupportMask = default)
        {
            PlacementEvaluation evaluation = Evaluate(
                origin,
                item,
                supportMask,
                obstructionMask,
                clockwiseQuarterTurns,
                stackSupportMask);
            return evaluation.IsValid
                ? OperationResult<Pose>.Success(evaluation.Pose)
                : OperationResult<Pose>.Fail(Failure.FromCode(evaluation.FailureCode));
        }

        private static bool HasFullSupport(
            Pose pose,
            Vector3 halfExtents,
            PlacementSurface expectedSurface,
            PhysicalItemProjection expectedStackSupport,
            LayerMask supportMask)
        {
            const float inset = 0.92f;
            Vector3 right = pose.rotation * Vector3.right * (halfExtents.x * inset);
            Vector3 forward = pose.rotation * Vector3.forward * (halfExtents.z * inset);
            Vector3[] offsets =
            {
                Vector3.zero,
                right + forward,
                right - forward,
                -right + forward,
                -right - forward
            };

            float rayDistance = halfExtents.y + SurfaceClearance + 0.2f;
            foreach (Vector3 offset in offsets)
            {
                Vector3 rayStart = pose.position + offset + (Vector3.up * 0.1f);
                if (!Physics.Raycast(
                        rayStart,
                        Vector3.down,
                        out RaycastHit support,
                        rayDistance,
                        supportMask,
                        QueryTriggerInteraction.Ignore) ||
                    Vector3.Dot(support.normal, Vector3.up) < MinimumUpDot)
                {
                    return false;
                }

                bool matchesSurface = expectedSurface != null &&
                                      support.collider.GetComponentInParent<PlacementSurface>() ==
                                      expectedSurface;
                bool matchesStackSupport = expectedStackSupport != null &&
                                           support.collider.GetComponentInParent<PhysicalItemProjection>() ==
                                           expectedStackSupport;
                if (!matchesSurface && !matchesStackSupport)
                {
                    return false;
                }
            }

            return true;
        }

        private static Quaternion SnapStackRotation(
            Quaternion supportRotation,
            Quaternion requestedRotation)
        {
            float supportYaw = supportRotation.eulerAngles.y;
            float relativeYaw = Mathf.DeltaAngle(supportYaw, requestedRotation.eulerAngles.y);
            float snappedRelativeYaw = Mathf.Round(relativeYaw / ClockwiseRotationStepDegrees) *
                                       ClockwiseRotationStepDegrees;
            return Quaternion.Euler(0f, supportYaw + snappedRelativeYaw, 0f);
        }

        private static bool TryFindPlacementSupport(
            Vector3 rayStart,
            LayerMask surfaceMask,
            LayerMask stackMask,
            out RaycastHit bestHit)
        {
            int count = Physics.RaycastNonAlloc(
                rayStart,
                Vector3.down,
                SupportHits,
                ProbeDistance,
                surfaceMask | stackMask,
                QueryTriggerInteraction.Ignore);
            bestHit = default;
            if (count >= SupportHitCapacity)
            {
                return false;
            }

            float nearestDistance = float.PositiveInfinity;
            bool found = false;
            for (int index = 0; index < count; index++)
            {
                RaycastHit candidate = SupportHits[index];
                if (candidate.collider == null)
                {
                    continue;
                }

                bool isSurfaceLayer = IsInLayerMask(candidate.collider.gameObject.layer, surfaceMask);
                PhysicalItemProjection itemSupport =
                    candidate.collider.GetComponentInParent<PhysicalItemProjection>();
                bool isStackItem = itemSupport != null &&
                                   IsInLayerMask(itemSupport.gameObject.layer, stackMask);
                if ((!isSurfaceLayer && !isStackItem) || candidate.distance >= nearestDistance)
                {
                    continue;
                }

                bestHit = candidate;
                nearestDistance = candidate.distance;
                found = true;
            }

            return found;
        }

        private static bool IsInLayerMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
