using System.Collections.Generic;
using PCShopEmpire3D.World.Interaction;
using PCShopEmpire3D.Assembly;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum ProcessorCoolerSlotStatus
    {
        Uninitialized, ValidSeat, ValidSeatedUnsecured, ValidRetained,
        ContextMissing, Paused, AuthorityBlocked, OutOfRange, NotFocused,
        LineOfSightBlocked, OrientationInvalid, Obstructed,
        ValidSeatedUnsecuredRetentionBlocked
    }

    public readonly struct ProcessorCoolerSlotEvaluation
    {
        public ProcessorCoolerSlotEvaluation(ProcessorCoolerSlotStatus status, Pose pose,
            bool hasPose, ProcessorCoolerMountOrientation orientation)
        {
            Status = status; Pose = pose; HasPose = hasPose; Orientation = orientation;
        }
        public ProcessorCoolerSlotStatus Status { get; }
        public Pose Pose { get; }
        public bool HasPose { get; }
        public ProcessorCoolerMountOrientation Orientation { get; }
        public bool CanSeat => Status == ProcessorCoolerSlotStatus.ValidSeat && HasPose &&
                               (Orientation == ProcessorCoolerMountOrientation.Primary ||
                                Orientation == ProcessorCoolerMountOrientation.Rotated180);
        public bool CanOperateRetention => Status == ProcessorCoolerSlotStatus.ValidSeatedUnsecured ||
                                            Status == ProcessorCoolerSlotStatus.ValidRetained;
        public bool CanRemove => Status == ProcessorCoolerSlotStatus.ValidSeatedUnsecured ||
                                 Status == ProcessorCoolerSlotStatus.ValidSeatedUnsecuredRetentionBlocked;
        public bool HasOwnedContext => CanSeat || CanOperateRetention || CanRemove ||
            Status == ProcessorCoolerSlotStatus.LineOfSightBlocked ||
            Status == ProcessorCoolerSlotStatus.Obstructed ||
            Status == ProcessorCoolerSlotStatus.OrientationInvalid;
        public string FailureCode => Status switch
        {
            ProcessorCoolerSlotStatus.ContextMissing => "assembly-cooler.context-missing",
            ProcessorCoolerSlotStatus.Paused => "assembly-cooler.paused",
            ProcessorCoolerSlotStatus.AuthorityBlocked => "assembly-cooler.authority-blocked",
            ProcessorCoolerSlotStatus.OutOfRange => "assembly-cooler.out-of-range",
            ProcessorCoolerSlotStatus.NotFocused => "assembly-cooler.focus-missing",
            ProcessorCoolerSlotStatus.LineOfSightBlocked => "assembly-cooler.line-of-sight-blocked",
            ProcessorCoolerSlotStatus.OrientationInvalid => "assembly-cooler.orientation-mismatch",
            ProcessorCoolerSlotStatus.Obstructed => "assembly-cooler.obstructed",
            ProcessorCoolerSlotStatus.ValidSeatedUnsecuredRetentionBlocked =>
                "assembly-cooler.host-unsecured",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Deterministic, allocation-free top-down cooler seat gate. It delegates all physics
    /// queries to AssemblySeatPhysics, whose overlap path is NonAlloc and fail-closed.
    /// </summary>
    public static class ProcessorCoolerSlotSolver
    {
        private const float RotationStepDegrees = 180f;
        private const float InsertionDistance = 0.075f;

        public static ProcessorCoolerSlotEvaluation EvaluateSeat(Transform interactionOrigin,
            Transform playerRoot, PhysicalItemProjection cooler, Transform snapAnchor,
            Collider focusCollider, Transform assemblyRoot, LayerMask obstructionMask,
            float maximumRange, float minimumFocusDot, int halfTurns, bool paused,
            bool authorityAvailable,
            IReadOnlyList<Collider> explicitClearanceBlockers = null)
        {
            if (interactionOrigin == null || cooler == null || snapAnchor == null ||
                focusCollider == null || assemblyRoot == null)
                return Invalid(ProcessorCoolerSlotStatus.ContextMissing, default, false, default);

            int turns = ((halfTurns % 2) + 2) % 2;
            ProcessorCoolerMountOrientation orientation = turns == 0
                ? ProcessorCoolerMountOrientation.Primary
                : ProcessorCoolerMountOrientation.Rotated180;
            Pose pose = ResolveSeatPose(snapAnchor, orientation);
            ProcessorCoolerSlotStatus focus = Map(AssemblySeatPhysics.EvaluateFocus(
                interactionOrigin, playerRoot, cooler.transform, focusCollider, assemblyRoot,
                obstructionMask, maximumRange, minimumFocusDot, paused, authorityAvailable));
            if (focus != ProcessorCoolerSlotStatus.ValidSeat) return Invalid(focus, pose, true, orientation);
            if (AssemblySeatPhysics.IsPoseObstructed(cooler, pose, snapAnchor.forward,
                    InsertionDistance, focusCollider, playerRoot, assemblyRoot,
                    obstructionMask, explicitClearanceBlockers))
                return Invalid(ProcessorCoolerSlotStatus.Obstructed, pose, true, orientation);
            return new ProcessorCoolerSlotEvaluation(ProcessorCoolerSlotStatus.ValidSeat, pose, true, orientation);
        }

        internal static Pose ResolveSeatPose(
            Transform snapAnchor,
            ProcessorCoolerMountOrientation orientation)
        {
            float rotation = orientation == ProcessorCoolerMountOrientation.Rotated180
                ? RotationStepDegrees
                : 0f;
            return new Pose(
                snapAnchor.position,
                snapAnchor.rotation *
                Quaternion.AngleAxis(rotation, Vector3.forward));
        }

        public static ProcessorCoolerSlotEvaluation EvaluateInteraction(Transform interactionOrigin,
            Transform playerRoot, Transform seatedCooler, Collider focusCollider,
            Transform assemblyRoot, LayerMask obstructionMask, float maximumRange,
            float minimumFocusDot, bool paused, ProcessorCoolerSlotState state,
            bool authorityAvailable, bool retentionAvailable)
        {
            if (interactionOrigin == null || focusCollider == null || assemblyRoot == null)
                return Invalid(ProcessorCoolerSlotStatus.ContextMissing, default, false, default);
            bool seated = state == ProcessorCoolerSlotState.CoolerSeatedUnsecured ||
                          state == ProcessorCoolerSlotState.CoolerRetained;
            ProcessorCoolerSlotStatus focus = Map(AssemblySeatPhysics.EvaluateFocus(
                interactionOrigin, playerRoot, seatedCooler, focusCollider, assemblyRoot,
                obstructionMask, maximumRange, minimumFocusDot, paused, authorityAvailable && seated));
            if (focus != ProcessorCoolerSlotStatus.ValidSeat) return Invalid(focus, default, false, default);
            ProcessorCoolerSlotStatus status = state == ProcessorCoolerSlotState.CoolerRetained
                ? ProcessorCoolerSlotStatus.ValidRetained : retentionAvailable
                    ? ProcessorCoolerSlotStatus.ValidSeatedUnsecured
                    : ProcessorCoolerSlotStatus.ValidSeatedUnsecuredRetentionBlocked;
            return Invalid(status, default, false, default);
        }

        private static ProcessorCoolerSlotStatus Map(AssemblySeatPhysicsStatus status) => status switch
        {
            AssemblySeatPhysicsStatus.Valid => ProcessorCoolerSlotStatus.ValidSeat,
            AssemblySeatPhysicsStatus.ContextMissing => ProcessorCoolerSlotStatus.ContextMissing,
            AssemblySeatPhysicsStatus.Paused => ProcessorCoolerSlotStatus.Paused,
            AssemblySeatPhysicsStatus.AuthorityBlocked => ProcessorCoolerSlotStatus.AuthorityBlocked,
            AssemblySeatPhysicsStatus.OutOfRange => ProcessorCoolerSlotStatus.OutOfRange,
            AssemblySeatPhysicsStatus.NotFocused => ProcessorCoolerSlotStatus.NotFocused,
            AssemblySeatPhysicsStatus.LineOfSightBlocked => ProcessorCoolerSlotStatus.LineOfSightBlocked,
            _ => ProcessorCoolerSlotStatus.Obstructed
        };
        private static ProcessorCoolerSlotEvaluation Invalid(ProcessorCoolerSlotStatus status,
            Pose pose, bool hasPose, ProcessorCoolerMountOrientation orientation) =>
            new ProcessorCoolerSlotEvaluation(status, pose, hasPose, orientation);
    }
}
