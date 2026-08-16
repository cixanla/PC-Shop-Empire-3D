using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum DimmSlotStatus
    {
        Uninitialized = 0,
        ValidSeat = 1,
        ValidSeatedOpen = 2,
        ValidRetained = 3,
        ContextMissing = 4,
        Paused = 5,
        AuthorityBlocked = 6,
        OutOfRange = 7,
        NotFocused = 8,
        LineOfSightBlocked = 9,
        OrientationInvalid = 10,
        Obstructed = 11,
        ValidSeatedOpenRetentionBlocked = 12
    }

    public readonly struct DimmSlotEvaluation
    {
        public DimmSlotEvaluation(
            DimmSlotStatus status,
            Pose pose,
            bool hasPose,
            DimmKeyOrientation orientation)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
            Orientation = orientation;
        }

        public DimmSlotStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public DimmKeyOrientation Orientation { get; }

        public bool CanSeat => Status == DimmSlotStatus.ValidSeat &&
                               HasPose &&
                               Orientation == DimmKeyOrientation.NotchAligned;

        public bool CanOperateRetention =>
            Status == DimmSlotStatus.ValidSeatedOpen ||
            Status == DimmSlotStatus.ValidRetained;

        public bool CanRemove => Status == DimmSlotStatus.ValidSeatedOpen ||
                                 Status ==
                                     DimmSlotStatus.ValidSeatedOpenRetentionBlocked;

        public bool HasOwnedContext => CanSeat ||
                                       CanOperateRetention ||
                                       CanRemove ||
                                       Status == DimmSlotStatus.LineOfSightBlocked ||
                                       Status == DimmSlotStatus.Obstructed ||
                                       Status == DimmSlotStatus.OrientationInvalid;

        public string FailureCode => Status switch
        {
            DimmSlotStatus.ContextMissing => "assembly-memory.context-missing",
            DimmSlotStatus.Paused => "assembly-memory.paused",
            DimmSlotStatus.AuthorityBlocked => "assembly-memory.authority-blocked",
            DimmSlotStatus.ValidSeatedOpenRetentionBlocked =>
                AssemblyFailures.MotherboardUnsecured.Code,
            DimmSlotStatus.OutOfRange => "assembly-memory.out-of-range",
            DimmSlotStatus.NotFocused => "assembly-memory.focus-missing",
            DimmSlotStatus.LineOfSightBlocked =>
                "assembly-memory.line-of-sight-blocked",
            DimmSlotStatus.OrientationInvalid =>
                Orientation == DimmKeyOrientation.Reversed
                    ? AssemblyFailures.DimmOrientationMismatch.Code
                    : AssemblyFailures.InvalidDimmOrientation.Code,
            DimmSlotStatus.Obstructed => "assembly-memory.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Allocation-free DDR5 UDIMM A2 slot gate. A quarter-turn preview is presentation
    /// state; only the canonical notch-aligned orientation can reach domain authority.
    /// </summary>
    public static class DimmSlotSolver
    {
        private const float RotationStepDegrees = 90f;
        private const float InsertionDistance = 0.10f;

        public static DimmSlotEvaluation EvaluateSeat(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection memoryModule,
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
                memoryModule == null ||
                snapAnchor == null ||
                focusCollider == null ||
                assemblyRoot == null)
            {
                return Invalid(DimmSlotStatus.ContextMissing, default, false, default);
            }

            int normalizedTurns = ((clockwiseQuarterTurns % 4) + 4) % 4;
            DimmKeyOrientation orientation = normalizedTurns switch
            {
                0 => DimmKeyOrientation.NotchAligned,
                2 => DimmKeyOrientation.Reversed,
                _ => default
            };
            Pose candidatePose = new Pose(
                snapAnchor.position,
                snapAnchor.rotation * Quaternion.AngleAxis(
                    normalizedTurns * RotationStepDegrees,
                    Vector3.forward));
            DimmSlotStatus focusStatus = MapFocusStatus(
                AssemblySeatPhysics.EvaluateFocus(
                    interactionOrigin,
                    playerRoot,
                    memoryModule.transform,
                    focusCollider,
                    assemblyRoot,
                    obstructionMask,
                    maximumRange,
                    minimumFocusDot,
                    paused,
                    authorityAvailable));
            if (focusStatus != DimmSlotStatus.ValidSeat)
            {
                return Invalid(focusStatus, candidatePose, true, orientation);
            }

            if (orientation != DimmKeyOrientation.NotchAligned)
            {
                return Invalid(
                    DimmSlotStatus.OrientationInvalid,
                    candidatePose,
                    true,
                    orientation);
            }

            if (AssemblySeatPhysics.IsPoseObstructed(
                memoryModule,
                candidatePose,
                snapAnchor.forward,
                InsertionDistance,
                focusCollider,
                playerRoot,
                assemblyRoot,
                obstructionMask))
            {
                return Invalid(
                    DimmSlotStatus.Obstructed,
                    candidatePose,
                    true,
                    orientation);
            }

            return new DimmSlotEvaluation(
                DimmSlotStatus.ValidSeat,
                candidatePose,
                true,
                orientation);
        }

        public static DimmSlotEvaluation EvaluateInteraction(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedMemoryRoot,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            MemorySlotState state,
            bool authorityAvailable,
            bool retentionCloseAvailable)
        {
            if (interactionOrigin == null || focusCollider == null || assemblyRoot == null)
            {
                return Invalid(DimmSlotStatus.ContextMissing, default, false, default);
            }

            bool stateCanOperate = state == MemorySlotState.MemoryModuleSeatedOpen ||
                                   state == MemorySlotState.MemoryModuleRetained;
            DimmSlotStatus focusStatus = MapFocusStatus(
                AssemblySeatPhysics.EvaluateFocus(
                    interactionOrigin,
                    playerRoot,
                    seatedMemoryRoot,
                    focusCollider,
                    assemblyRoot,
                    obstructionMask,
                    maximumRange,
                    minimumFocusDot,
                    paused,
                    authorityAvailable && stateCanOperate));
            if (focusStatus != DimmSlotStatus.ValidSeat)
            {
                return Invalid(focusStatus, default, false, default);
            }

            DimmSlotStatus status = state == MemorySlotState.MemoryModuleRetained
                ? DimmSlotStatus.ValidRetained
                : retentionCloseAvailable
                    ? DimmSlotStatus.ValidSeatedOpen
                    : DimmSlotStatus.ValidSeatedOpenRetentionBlocked;
            return new DimmSlotEvaluation(status, default, false, default);
        }

        private static DimmSlotStatus MapFocusStatus(AssemblySeatPhysicsStatus status)
        {
            return status switch
            {
                AssemblySeatPhysicsStatus.Valid => DimmSlotStatus.ValidSeat,
                AssemblySeatPhysicsStatus.ContextMissing => DimmSlotStatus.ContextMissing,
                AssemblySeatPhysicsStatus.Paused => DimmSlotStatus.Paused,
                AssemblySeatPhysicsStatus.AuthorityBlocked =>
                    DimmSlotStatus.AuthorityBlocked,
                AssemblySeatPhysicsStatus.OutOfRange => DimmSlotStatus.OutOfRange,
                AssemblySeatPhysicsStatus.NotFocused => DimmSlotStatus.NotFocused,
                AssemblySeatPhysicsStatus.LineOfSightBlocked =>
                    DimmSlotStatus.LineOfSightBlocked,
                _ => DimmSlotStatus.Obstructed
            };
        }

        private static DimmSlotEvaluation Invalid(
            DimmSlotStatus status,
            Pose pose,
            bool hasPose,
            DimmKeyOrientation orientation)
        {
            return new DimmSlotEvaluation(status, pose, hasPose, orientation);
        }
    }
}
