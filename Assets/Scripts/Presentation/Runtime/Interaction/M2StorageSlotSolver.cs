using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum M2StorageSlotStatus
    {
        Uninitialized = 0,
        ValidSeat = 1,
        ValidSeatedUnsecured = 2,
        ValidSecured = 3,
        ContextMissing = 4,
        Paused = 5,
        AuthorityBlocked = 6,
        OutOfRange = 7,
        NotFocused = 8,
        LineOfSightBlocked = 9,
        OrientationInvalid = 10,
        Obstructed = 11,
        ValidSeatedUnsecuredRetentionBlocked = 12
    }

    public readonly struct M2StorageSlotEvaluation
    {
        public M2StorageSlotEvaluation(
            M2StorageSlotStatus status,
            Pose guidedPose,
            Pose seatedPose,
            bool hasPose,
            M2KeyOrientation orientation)
        {
            Status = status;
            GuidedPose = guidedPose;
            SeatedPose = seatedPose;
            HasPose = hasPose;
            Orientation = orientation;
        }

        public M2StorageSlotStatus Status { get; }

        public Pose GuidedPose { get; }

        public Pose SeatedPose { get; }

        public Pose Pose => GuidedPose;

        public bool HasPose { get; }

        public M2KeyOrientation Orientation { get; }

        public bool CanSeat => Status == M2StorageSlotStatus.ValidSeat &&
                               HasPose &&
                               Orientation == M2KeyOrientation.KeyAligned;

        public bool CanOperateRetention =>
            Status == M2StorageSlotStatus.ValidSeatedUnsecured ||
            Status == M2StorageSlotStatus.ValidSecured;

        public bool CanRemove =>
            Status == M2StorageSlotStatus.ValidSeatedUnsecured ||
            Status == M2StorageSlotStatus.ValidSeatedUnsecuredRetentionBlocked;

        public bool HasOwnedContext => CanSeat ||
                                       CanOperateRetention ||
                                       CanRemove ||
                                       Status == M2StorageSlotStatus.LineOfSightBlocked ||
                                       Status == M2StorageSlotStatus.Obstructed ||
                                       Status == M2StorageSlotStatus.OrientationInvalid;

        public string FailureCode => Status switch
        {
            M2StorageSlotStatus.ContextMissing => "assembly-storage.context-missing",
            M2StorageSlotStatus.Paused => "assembly-storage.paused",
            M2StorageSlotStatus.AuthorityBlocked =>
                "assembly-storage.authority-blocked",
            M2StorageSlotStatus.ValidSeatedUnsecuredRetentionBlocked =>
                AssemblyFailures.MotherboardUnsecured.Code,
            M2StorageSlotStatus.OutOfRange => "assembly-storage.out-of-range",
            M2StorageSlotStatus.NotFocused => "assembly-storage.focus-missing",
            M2StorageSlotStatus.LineOfSightBlocked =>
                "assembly-storage.line-of-sight-blocked",
            M2StorageSlotStatus.OrientationInvalid =>
                Orientation == M2KeyOrientation.Reversed
                    ? AssemblyFailures.M2OrientationMismatch.Code
                    : AssemblyFailures.InvalidM2Orientation.Code,
            M2StorageSlotStatus.Obstructed => "assembly-storage.obstructed",
            _ => string.Empty
        };
    }

    /// <summary>
    /// Allocation-free single M-key 2280 gate. It exposes a raised guided insertion pose
    /// and a separate flat authoritative seated pose; only 0/180 degree input is valid.
    /// </summary>
    public static class M2StorageSlotSolver
    {
        public const float GuidedInsertionAngleDegrees = 18f;
        public const float GuidedLiftMetres = 0.024f;
        private const float RotationStepDegrees = 90f;
        private const float InsertionDistance = 0.10f;

        public static M2StorageSlotEvaluation EvaluateSeat(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection storageDevice,
            Transform seatedAnchor,
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
                storageDevice == null ||
                seatedAnchor == null ||
                focusCollider == null ||
                assemblyRoot == null)
            {
                return Invalid(M2StorageSlotStatus.ContextMissing);
            }

            int normalizedTurns = ((clockwiseQuarterTurns % 4) + 4) % 4;
            M2KeyOrientation orientation = normalizedTurns switch
            {
                0 => M2KeyOrientation.KeyAligned,
                2 => M2KeyOrientation.Reversed,
                _ => default
            };
            Quaternion seatedRotation = seatedAnchor.rotation * Quaternion.AngleAxis(
                normalizedTurns * RotationStepDegrees,
                Vector3.up);
            var seatedPose = new Pose(seatedAnchor.position, seatedRotation);
            var guidedPose = new Pose(
                seatedAnchor.position + seatedAnchor.up * GuidedLiftMetres,
                seatedRotation * Quaternion.AngleAxis(
                    -GuidedInsertionAngleDegrees,
                    Vector3.right));
            M2StorageSlotStatus focusStatus = MapFocusStatus(
                AssemblySeatPhysics.EvaluateFocus(
                    interactionOrigin,
                    playerRoot,
                    storageDevice.transform,
                    focusCollider,
                    assemblyRoot,
                    obstructionMask,
                    maximumRange,
                    minimumFocusDot,
                    paused,
                    authorityAvailable));
            if (focusStatus != M2StorageSlotStatus.ValidSeat)
            {
                return new M2StorageSlotEvaluation(
                    focusStatus,
                    guidedPose,
                    seatedPose,
                    true,
                    orientation);
            }

            if (orientation != M2KeyOrientation.KeyAligned)
            {
                return new M2StorageSlotEvaluation(
                    M2StorageSlotStatus.OrientationInvalid,
                    guidedPose,
                    seatedPose,
                    true,
                    orientation);
            }

            if (AssemblySeatPhysics.IsPoseObstructed(
                storageDevice,
                seatedPose,
                seatedAnchor.forward,
                InsertionDistance,
                focusCollider,
                playerRoot,
                assemblyRoot,
                obstructionMask))
            {
                return new M2StorageSlotEvaluation(
                    M2StorageSlotStatus.Obstructed,
                    guidedPose,
                    seatedPose,
                    true,
                    orientation);
            }

            return new M2StorageSlotEvaluation(
                M2StorageSlotStatus.ValidSeat,
                guidedPose,
                seatedPose,
                true,
                orientation);
        }

        public static M2StorageSlotEvaluation EvaluateInteraction(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedStorageRoot,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            StorageSlotState state,
            bool authorityAvailable,
            bool retentionCloseAvailable)
        {
            if (interactionOrigin == null || focusCollider == null || assemblyRoot == null)
            {
                return Invalid(M2StorageSlotStatus.ContextMissing);
            }

            bool stateCanOperate =
                state == StorageSlotState.StorageDeviceSeatedUnsecured ||
                state == StorageSlotState.StorageDeviceSecured;
            M2StorageSlotStatus focusStatus = MapFocusStatus(
                AssemblySeatPhysics.EvaluateFocus(
                    interactionOrigin,
                    playerRoot,
                    seatedStorageRoot,
                    focusCollider,
                    assemblyRoot,
                    obstructionMask,
                    maximumRange,
                    minimumFocusDot,
                    paused,
                    authorityAvailable && stateCanOperate));
            if (focusStatus != M2StorageSlotStatus.ValidSeat)
            {
                return new M2StorageSlotEvaluation(
                    focusStatus,
                    default,
                    default,
                    false,
                    default);
            }

            M2StorageSlotStatus status = state == StorageSlotState.StorageDeviceSecured
                ? M2StorageSlotStatus.ValidSecured
                : retentionCloseAvailable
                    ? M2StorageSlotStatus.ValidSeatedUnsecured
                    : M2StorageSlotStatus.ValidSeatedUnsecuredRetentionBlocked;
            return new M2StorageSlotEvaluation(
                status,
                default,
                default,
                false,
                default);
        }

        private static M2StorageSlotStatus MapFocusStatus(AssemblySeatPhysicsStatus status)
        {
            return status switch
            {
                AssemblySeatPhysicsStatus.Valid => M2StorageSlotStatus.ValidSeat,
                AssemblySeatPhysicsStatus.ContextMissing =>
                    M2StorageSlotStatus.ContextMissing,
                AssemblySeatPhysicsStatus.Paused => M2StorageSlotStatus.Paused,
                AssemblySeatPhysicsStatus.AuthorityBlocked =>
                    M2StorageSlotStatus.AuthorityBlocked,
                AssemblySeatPhysicsStatus.OutOfRange => M2StorageSlotStatus.OutOfRange,
                AssemblySeatPhysicsStatus.NotFocused => M2StorageSlotStatus.NotFocused,
                AssemblySeatPhysicsStatus.LineOfSightBlocked =>
                    M2StorageSlotStatus.LineOfSightBlocked,
                _ => M2StorageSlotStatus.Obstructed
            };
        }

        private static M2StorageSlotEvaluation Invalid(M2StorageSlotStatus status)
        {
            return new M2StorageSlotEvaluation(
                status,
                default,
                default,
                false,
                default);
        }
    }
}
