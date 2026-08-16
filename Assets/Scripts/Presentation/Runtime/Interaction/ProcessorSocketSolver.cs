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
        private const float RotationStepDegrees = 90f;
        private const float InsertionDistance = 0.08f;

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
            ProcessorSocketStatus focusStatus = MapFocusStatus(
                AssemblySeatPhysics.EvaluateFocus(
                interactionOrigin,
                playerRoot,
                processor.transform,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                authorityAvailable));
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

            if (AssemblySeatPhysics.IsPoseObstructed(
                processor,
                candidatePose,
                snapAnchor.forward,
                InsertionDistance,
                focusCollider,
                playerRoot,
                assemblyRoot,
                obstructionMask))
            {
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
            ProcessorSocketStatus focusStatus = MapFocusStatus(
                AssemblySeatPhysics.EvaluateFocus(
                interactionOrigin,
                playerRoot,
                seatedProcessorRoot,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                authorityAvailable && stateCanOperate));
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

        private static ProcessorSocketStatus MapFocusStatus(
            AssemblySeatPhysicsStatus status)
        {
            return status switch
            {
                AssemblySeatPhysicsStatus.Valid => ProcessorSocketStatus.ValidSeat,
                AssemblySeatPhysicsStatus.ContextMissing =>
                    ProcessorSocketStatus.ContextMissing,
                AssemblySeatPhysicsStatus.Paused => ProcessorSocketStatus.Paused,
                AssemblySeatPhysicsStatus.AuthorityBlocked =>
                    ProcessorSocketStatus.AuthorityBlocked,
                AssemblySeatPhysicsStatus.OutOfRange => ProcessorSocketStatus.OutOfRange,
                AssemblySeatPhysicsStatus.NotFocused => ProcessorSocketStatus.NotFocused,
                AssemblySeatPhysicsStatus.LineOfSightBlocked =>
                    ProcessorSocketStatus.LineOfSightBlocked,
                _ => ProcessorSocketStatus.Obstructed
            };
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
