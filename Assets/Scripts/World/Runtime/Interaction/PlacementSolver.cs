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
        Blocked = 6
    }

    public readonly struct PlacementEvaluation
    {
        public PlacementEvaluation(PlacementStatus status, Pose pose, bool hasPose)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
        }

        public PlacementStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public bool IsValid => Status == PlacementStatus.Valid;

        public string FailureCode => Status switch
        {
            PlacementStatus.ContextMissing => "placement.context-missing",
            PlacementStatus.NoSupport => "placement.no-support",
            PlacementStatus.SurfaceNotAllowed => "placement.surface-not-allowed",
            PlacementStatus.SurfaceTooSteep => "placement.surface-too-steep",
            PlacementStatus.OutsideSurface => "placement.outside-surface",
            PlacementStatus.Blocked => "placement.blocked",
            _ => string.Empty
        };
    }

    public static class PlacementSolver
    {
        private const float SurfaceClearance = 0.025f;
        private const float MinimumUpDot = 0.92f;
        private const float ProbeHeight = 0.45f;
        private const float ProbeDistance = 3f;
        private const float ClockwiseRotationStepDegrees = 90f;
        private static readonly float[] CandidateDistances = { 1.15f, 0.9f, 0.7f };

        public static PlacementEvaluation Evaluate(
            Transform origin,
            PhysicalItemProjection item,
            LayerMask supportMask,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns = 0)
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

            foreach (float distance in CandidateDistances)
            {
                Vector3 rayStart = origin.position + (forward * distance) + (Vector3.up * ProbeHeight);
                if (!Physics.Raycast(
                        rayStart,
                        Vector3.down,
                        out RaycastHit support,
                        ProbeDistance,
                        supportMask,
                        QueryTriggerInteraction.Ignore))
                {
                    continue;
                }

                PlacementSurface surface = support.collider.GetComponentInParent<PlacementSurface>();
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

                if (surface == null || surface.SurfaceCollider == null || !surface.SurfaceCollider.enabled)
                {
                    firstInvalid ??= new PlacementEvaluation(
                        PlacementStatus.SurfaceNotAllowed,
                        unsnappedPose,
                        true);
                    continue;
                }

                Vector3 snappedSurfacePoint = surface.SnapPoint(support.point);
                Quaternion snappedRotation = surface.SnapRotation(requestedRotation);
                Pose snappedPose = new Pose(
                    snappedSurfacePoint + (Vector3.up * (halfExtents.y + SurfaceClearance)),
                    snappedRotation);

                if (!HasFullSupport(snappedPose, halfExtents, surface, supportMask))
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

                return new PlacementEvaluation(PlacementStatus.Valid, snappedPose, true);
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
            int clockwiseQuarterTurns = 0)
        {
            PlacementEvaluation evaluation = Evaluate(
                origin,
                item,
                supportMask,
                obstructionMask,
                clockwiseQuarterTurns);
            return evaluation.IsValid
                ? OperationResult<Pose>.Success(evaluation.Pose)
                : OperationResult<Pose>.Fail(Failure.FromCode(evaluation.FailureCode));
        }

        private static bool HasFullSupport(
            Pose pose,
            Vector3 halfExtents,
            PlacementSurface expectedSurface,
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
                    support.collider.GetComponentInParent<PlacementSurface>() != expectedSurface)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
