using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum MotherboardSeatStatus
    {
        Uninitialized = 0,
        Valid = 1,
        ContextMissing = 2,
        Paused = 3,
        AuthorityBlocked = 4,
        OutOfRange = 5,
        NotFocused = 6,
        LineOfSightBlocked = 7,
        OrientationInvalid = 8,
        Unsupported = 9,
        Obstructed = 10
    }

    public readonly struct MotherboardSeatEvaluation
    {
        public MotherboardSeatEvaluation(
            MotherboardSeatStatus status,
            Pose pose,
            bool hasPose)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
        }

        public MotherboardSeatStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public bool IsValid => Status == MotherboardSeatStatus.Valid && HasPose;

        public string FailureCode => Status switch
        {
            MotherboardSeatStatus.ContextMissing => "assembly-seat.context-missing",
            MotherboardSeatStatus.Paused => "assembly-seat.paused",
            MotherboardSeatStatus.AuthorityBlocked => "assembly-seat.authority-blocked",
            MotherboardSeatStatus.OutOfRange => "assembly-seat.out-of-range",
            MotherboardSeatStatus.NotFocused => "assembly-seat.focus-missing",
            MotherboardSeatStatus.LineOfSightBlocked =>
                "assembly-seat.line-of-sight-blocked",
            MotherboardSeatStatus.OrientationInvalid =>
                "assembly-seat.orientation-invalid",
            MotherboardSeatStatus.Unsupported => "assembly-seat.unsupported",
            MotherboardSeatStatus.Obstructed => "assembly-seat.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Fail-closed, deterministic geometry gate for the first keyed motherboard slot.
    /// The returned pose is the exact pose used by both preview and commit.
    /// </summary>
    public static class MotherboardSeatSolver
    {
        private const int HitCapacity = 32;
        private const float RotationStepDegrees = 90f;
        private const float SupportProbeOffset = 0.03f;
        private const float SupportProbeDistance = 0.12f;
        private const float InsertionDistance = 0.18f;
        private const float ObstructionInset = 1f;

        private static readonly RaycastHit[] LineHits = new RaycastHit[HitCapacity];
        private static readonly RaycastHit[] InsertionHits = new RaycastHit[HitCapacity];
        private static readonly Collider[] Overlaps = new Collider[HitCapacity];

        public static MotherboardSeatEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection motherboard,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            int clockwiseQuarterTurns,
            bool paused,
            bool authorityAvailable)
        {
            if (interactionOrigin == null ||
                motherboard == null ||
                snapAnchor == null ||
                focusCollider == null ||
                supportCollider == null)
            {
                return new MotherboardSeatEvaluation(
                    MotherboardSeatStatus.ContextMissing,
                    default,
                    false);
            }

            int normalizedTurns = ((clockwiseQuarterTurns % 4) + 4) % 4;
            Quaternion candidateRotation = snapAnchor.rotation *
                                           Quaternion.AngleAxis(
                                               normalizedTurns * RotationStepDegrees,
                                               Vector3.forward);
            Pose candidatePose = new Pose(snapAnchor.position, candidateRotation);

            if (paused)
            {
                return Invalid(MotherboardSeatStatus.Paused, candidatePose);
            }

            if (!authorityAvailable)
            {
                return Invalid(MotherboardSeatStatus.AuthorityBlocked, candidatePose);
            }

            Vector3 focusPoint = focusCollider.bounds.center;
            Vector3 toFocus = focusPoint - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon || distance > Mathf.Max(0.1f, maximumRange))
            {
                return Invalid(MotherboardSeatStatus.OutOfRange, candidatePose);
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp(minimumFocusDot, 0f, 1f))
            {
                return Invalid(MotherboardSeatStatus.NotFocused, candidatePose);
            }

            if (!HasLineOfSight(
                    interactionOrigin.position,
                    direction,
                    distance,
                    focusCollider,
                    motherboard,
                    playerRoot,
                    obstructionMask))
            {
                return Invalid(MotherboardSeatStatus.LineOfSightBlocked, candidatePose);
            }

            if (normalizedTurns != 0)
            {
                return Invalid(MotherboardSeatStatus.OrientationInvalid, candidatePose);
            }

            Vector3 seatForward = snapAnchor.forward.normalized;
            Ray supportRay = new Ray(
                candidatePose.position + (seatForward * SupportProbeOffset),
                -seatForward);
            if (!supportCollider.enabled ||
                !supportCollider.gameObject.activeInHierarchy ||
                !supportCollider.Raycast(
                    supportRay,
                    out _,
                    SupportProbeDistance))
            {
                return Invalid(MotherboardSeatStatus.Unsupported, candidatePose);
            }

            int overlapCount = Physics.OverlapBoxNonAlloc(
                candidatePose.position,
                motherboard.DropHalfExtents * ObstructionInset,
                Overlaps,
                candidatePose.rotation,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (overlapCount >= HitCapacity)
            {
                return Invalid(MotherboardSeatStatus.Obstructed, candidatePose);
            }

            for (int index = 0; index < overlapCount; index++)
            {
                Collider overlap = Overlaps[index];
                if (overlap == null ||
                    overlap == focusCollider ||
                    overlap == supportCollider ||
                    IsChildOf(overlap.transform, motherboard.transform))
                {
                    continue;
                }

                return Invalid(MotherboardSeatStatus.Obstructed, candidatePose);
            }

            Vector3 insertionStart = candidatePose.position +
                                     (seatForward * InsertionDistance);
            int insertionCount = Physics.BoxCastNonAlloc(
                insertionStart,
                motherboard.DropHalfExtents * ObstructionInset,
                -seatForward,
                InsertionHits,
                candidatePose.rotation,
                InsertionDistance,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (insertionCount >= HitCapacity)
            {
                return Invalid(MotherboardSeatStatus.Obstructed, candidatePose);
            }

            for (int index = 0; index < insertionCount; index++)
            {
                Collider hit = InsertionHits[index].collider;
                if (hit == null ||
                    hit == focusCollider ||
                    hit == supportCollider ||
                    IsChildOf(hit.transform, motherboard.transform))
                {
                    continue;
                }

                return Invalid(MotherboardSeatStatus.Obstructed, candidatePose);
            }

            return new MotherboardSeatEvaluation(
                MotherboardSeatStatus.Valid,
                candidatePose,
                true);
        }

        private static bool HasLineOfSight(
            Vector3 origin,
            Vector3 direction,
            float distance,
            Collider focusCollider,
            PhysicalItemProjection motherboard,
            Transform playerRoot,
            LayerMask obstructionMask)
        {
            int focusLayerMask = 1 << focusCollider.gameObject.layer;
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction,
                LineHits,
                distance + 0.03f,
                obstructionMask | focusLayerMask,
                QueryTriggerInteraction.Ignore);
            if (hitCount <= 0 || hitCount >= HitCapacity)
            {
                return false;
            }

            Collider nearest = null;
            float nearestDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = LineHits[index];
                if (hit.collider == null ||
                    IsChildOf(hit.collider.transform, playerRoot) ||
                    IsChildOf(hit.collider.transform, motherboard.transform))
                {
                    continue;
                }

                if (hit.distance < nearestDistance)
                {
                    nearest = hit.collider;
                    nearestDistance = hit.distance;
                }
            }

            return nearest == focusCollider;
        }

        private static bool IsChildOf(Transform candidate, Transform root)
        {
            return candidate != null && root != null && candidate.IsChildOf(root);
        }

        private static MotherboardSeatEvaluation Invalid(
            MotherboardSeatStatus status,
            Pose pose)
        {
            return new MotherboardSeatEvaluation(status, pose, true);
        }
    }
}
