using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum ProcessorSocketStatus
    {
        Uninitialized = 0,
        ValidSeat = 1,
        ValidSeatedOpen = 2,
        ValidRetained = 3,
        ValidSeatedOpenRetentionBlocked = 12,
        ContextMissing = 4,
        Paused = 5,
        AuthorityBlocked = 6,
        OutOfRange = 7,
        NotFocused = 8,
        LineOfSightBlocked = 9,
        OrientationInvalid = 10,
        Obstructed = 11
    }

    public readonly struct ProcessorSocketEvaluation
    {
        public ProcessorSocketEvaluation(
            ProcessorSocketStatus status,
            Pose pose,
            bool hasPose)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
        }

        public ProcessorSocketStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public bool CanSeat => Status == ProcessorSocketStatus.ValidSeat && HasPose;

        public bool CanOperateRetention =>
            Status == ProcessorSocketStatus.ValidSeatedOpen ||
            Status == ProcessorSocketStatus.ValidRetained;

        public bool CanRemove => Status == ProcessorSocketStatus.ValidSeatedOpen ||
                                 Status ==
                                     ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked;

        public bool HasOwnedContext => CanSeat ||
                                       CanOperateRetention ||
                                       CanRemove ||
                                       Status == ProcessorSocketStatus.LineOfSightBlocked ||
                                       Status == ProcessorSocketStatus.Obstructed ||
                                       Status == ProcessorSocketStatus.OrientationInvalid;

        public string FailureCode => Status switch
        {
            ProcessorSocketStatus.ContextMissing =>
                "assembly-processor.context-missing",
            ProcessorSocketStatus.Paused => "assembly-processor.paused",
            ProcessorSocketStatus.AuthorityBlocked =>
                "assembly-processor.authority-blocked",
            ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked =>
                AssemblyFailures.MotherboardUnsecured.Code,
            ProcessorSocketStatus.OutOfRange => "assembly-processor.out-of-range",
            ProcessorSocketStatus.NotFocused => "assembly-processor.focus-missing",
            ProcessorSocketStatus.LineOfSightBlocked =>
                "assembly-processor.line-of-sight-blocked",
            ProcessorSocketStatus.OrientationInvalid =>
                "assembly-processor.orientation-invalid",
            ProcessorSocketStatus.Obstructed => "assembly-processor.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Allocation-free keyed CPU socket gate. Authority state is supplied explicitly and
    /// never inferred from cover, lever or CPU transforms.
    /// </summary>
    public static class ProcessorSocketSolver
    {
        private const int HitCapacity = 32;
        private const float DistanceTieEpsilon = 0.0001f;
        private const float RotationStepDegrees = 90f;
        private const float InsertionDistance = 0.08f;
        private static readonly RaycastHit[] LineHits = new RaycastHit[HitCapacity];
        private static readonly RaycastHit[] InsertionHits = new RaycastHit[HitCapacity];
        private static readonly Collider[] Overlaps = new Collider[HitCapacity];

        public static ProcessorSocketEvaluation EvaluateSeat(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection processor,
            Transform snapAnchor,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            int clockwiseQuarterTurns,
            bool paused,
            bool authorityAvailable)
        {
            if (interactionOrigin == null ||
                processor == null ||
                snapAnchor == null ||
                focusCollider == null ||
                assemblyRoot == null)
            {
                return Invalid(ProcessorSocketStatus.ContextMissing, default, false);
            }

            int normalizedTurns = ((clockwiseQuarterTurns % 4) + 4) % 4;
            Pose candidatePose = new Pose(
                snapAnchor.position,
                snapAnchor.rotation * Quaternion.AngleAxis(
                    normalizedTurns * RotationStepDegrees,
                    Vector3.forward));
            ProcessorSocketStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                processor.transform,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                authorityAvailable);
            if (focusStatus != ProcessorSocketStatus.ValidSeat)
            {
                return Invalid(focusStatus, candidatePose, true);
            }

            if (normalizedTurns != 0)
            {
                return Invalid(
                    ProcessorSocketStatus.OrientationInvalid,
                    candidatePose,
                    true);
            }

            int overlapCount = Physics.OverlapBoxNonAlloc(
                candidatePose.position,
                processor.DropHalfExtents,
                Overlaps,
                candidatePose.rotation,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (overlapCount >= HitCapacity)
            {
                return Invalid(ProcessorSocketStatus.Obstructed, candidatePose, true);
            }

            for (int index = 0; index < overlapCount; index++)
            {
                Collider overlap = Overlaps[index];
                if (overlap == null ||
                    overlap == focusCollider ||
                    IsChildOf(overlap.transform, playerRoot) ||
                    IsChildOf(overlap.transform, processor.transform) ||
                    IsChildOf(overlap.transform, assemblyRoot))
                {
                    continue;
                }

                return Invalid(ProcessorSocketStatus.Obstructed, candidatePose, true);
            }

            Vector3 insertionNormal = snapAnchor.forward.normalized;
            int insertionCount = Physics.BoxCastNonAlloc(
                candidatePose.position + (insertionNormal * InsertionDistance),
                processor.DropHalfExtents,
                -insertionNormal,
                InsertionHits,
                candidatePose.rotation,
                InsertionDistance,
                obstructionMask,
                QueryTriggerInteraction.Ignore);
            if (insertionCount >= HitCapacity)
            {
                return Invalid(ProcessorSocketStatus.Obstructed, candidatePose, true);
            }

            for (int index = 0; index < insertionCount; index++)
            {
                Collider hit = InsertionHits[index].collider;
                if (hit == null ||
                    hit == focusCollider ||
                    IsChildOf(hit.transform, playerRoot) ||
                    IsChildOf(hit.transform, processor.transform) ||
                    IsChildOf(hit.transform, assemblyRoot))
                {
                    continue;
                }

                return Invalid(ProcessorSocketStatus.Obstructed, candidatePose, true);
            }

            return new ProcessorSocketEvaluation(
                ProcessorSocketStatus.ValidSeat,
                candidatePose,
                true);
        }

        public static ProcessorSocketEvaluation EvaluateInteraction(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedProcessorRoot,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            ProcessorSocketState state,
            bool authorityAvailable,
            bool retentionCloseAvailable)
        {
            if (interactionOrigin == null || focusCollider == null || assemblyRoot == null)
            {
                return Invalid(ProcessorSocketStatus.ContextMissing, default, false);
            }

            bool stateCanOperate = state == ProcessorSocketState.ProcessorSeatedOpen ||
                                   state == ProcessorSocketState.ProcessorRetained;
            ProcessorSocketStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                seatedProcessorRoot,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                authorityAvailable && stateCanOperate);
            if (focusStatus != ProcessorSocketStatus.ValidSeat)
            {
                return Invalid(focusStatus, default, false);
            }

            ProcessorSocketStatus status = state == ProcessorSocketState.ProcessorRetained
                ? ProcessorSocketStatus.ValidRetained
                : retentionCloseAvailable
                    ? ProcessorSocketStatus.ValidSeatedOpen
                    : ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked;
            return new ProcessorSocketEvaluation(
                status,
                default,
                false);
        }

        private static ProcessorSocketStatus EvaluateFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform heldProcessorRoot,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            bool authorityAvailable)
        {
            if (!focusCollider.enabled || !focusCollider.gameObject.activeInHierarchy)
            {
                return ProcessorSocketStatus.ContextMissing;
            }

            if (paused)
            {
                return ProcessorSocketStatus.Paused;
            }

            if (!authorityAvailable)
            {
                return ProcessorSocketStatus.AuthorityBlocked;
            }

            Vector3 toFocus = focusCollider.bounds.center - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon || distance > Mathf.Max(0.1f, maximumRange))
            {
                return ProcessorSocketStatus.OutOfRange;
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return ProcessorSocketStatus.NotFocused;
            }

            int targetMask = 1 << focusCollider.gameObject.layer;
            int hitCount = Physics.RaycastNonAlloc(
                interactionOrigin.position,
                direction,
                LineHits,
                distance + 0.03f,
                obstructionMask | targetMask,
                QueryTriggerInteraction.Ignore);
            if (hitCount <= 0)
            {
                return ProcessorSocketStatus.LineOfSightBlocked;
            }

            if (hitCount >= HitCapacity)
            {
                return ProcessorSocketStatus.Obstructed;
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit hit = LineHits[index];
                if (hit.collider == null ||
                    IsChildOf(hit.collider.transform, playerRoot) ||
                    IsChildOf(hit.collider.transform, heldProcessorRoot) ||
                    (hit.collider != focusCollider &&
                     IsChildOf(hit.collider.transform, assemblyRoot)))
                {
                    continue;
                }

                if (hit.collider == focusCollider)
                {
                    targetDistance = Mathf.Min(targetDistance, hit.distance);
                }
                else
                {
                    obstructionDistance = Mathf.Min(
                        obstructionDistance,
                        hit.distance);
                }
            }

            if (float.IsPositiveInfinity(targetDistance))
            {
                return ProcessorSocketStatus.LineOfSightBlocked;
            }

            return obstructionDistance <= targetDistance + DistanceTieEpsilon
                ? ProcessorSocketStatus.Obstructed
                : ProcessorSocketStatus.ValidSeat;
        }

        private static bool IsChildOf(Transform candidate, Transform root)
        {
            return candidate != null && root != null && candidate.IsChildOf(root);
        }

        private static ProcessorSocketEvaluation Invalid(
            ProcessorSocketStatus status,
            Pose pose,
            bool hasPose)
        {
            return new ProcessorSocketEvaluation(status, pose, hasPose);
        }
    }
}
