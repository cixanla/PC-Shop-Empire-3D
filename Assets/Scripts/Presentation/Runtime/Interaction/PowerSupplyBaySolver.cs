using System.Collections.Generic;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum PowerSupplyFormFactor
    {
        Unknown = 0,
        AtxPs2 = 1
    }

    public enum PowerSupplySeatOrientation
    {
        FanToFilteredVent = 0,
        FanAwayFromFilteredVent = 1
    }

    public enum PowerSupplyBayProjectionState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        PowerSupplySeatedUnsecured = 2,
        PowerSupplyRetained = 3
    }

    public enum PowerSupplyBayStatus
    {
        Uninitialized = 0,
        ModeDisabled = 1,
        ValidSeat = 2,
        ValidSeatedUnsecured = 3,
        ValidRetained = 4,
        ContextMissing = 5,
        Paused = 6,
        AuthorityBlocked = 7,
        OutOfRange = 8,
        NotFocused = 9,
        LineOfSightBlocked = 10,
        FormFactorInvalid = 11,
        OrientationInvalid = 12,
        Unsupported = 13,
        ChassisClearanceBlocked = 14,
        CableClearanceBlocked = 15,
        Obstructed = 16,
        ValidSeatedUnsecuredRetentionBlocked = 17
    }

    public readonly struct PowerSupplyBayEvaluation
    {
        public PowerSupplyBayEvaluation(
            PowerSupplyBayStatus status,
            Pose pose,
            bool hasPose,
            PowerSupplySeatOrientation orientation)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
            Orientation = orientation;
        }

        public PowerSupplyBayStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public PowerSupplySeatOrientation Orientation { get; }

        public bool CanSeat =>
            Status == PowerSupplyBayStatus.ValidSeat &&
            HasPose &&
            Orientation == PowerSupplySeatOrientation.FanToFilteredVent;

        public bool CanOperateRetention =>
            Status == PowerSupplyBayStatus.ValidSeatedUnsecured ||
            Status == PowerSupplyBayStatus.ValidRetained;

        public bool CanRemove =>
            Status == PowerSupplyBayStatus.ValidSeatedUnsecured ||
            Status == PowerSupplyBayStatus.ValidSeatedUnsecuredRetentionBlocked;

        public bool HasOwnedContext =>
            CanSeat ||
            CanOperateRetention ||
            CanRemove ||
            Status == PowerSupplyBayStatus.LineOfSightBlocked ||
            Status == PowerSupplyBayStatus.FormFactorInvalid ||
            Status == PowerSupplyBayStatus.OrientationInvalid ||
            Status == PowerSupplyBayStatus.Unsupported ||
            Status == PowerSupplyBayStatus.ChassisClearanceBlocked ||
            Status == PowerSupplyBayStatus.CableClearanceBlocked ||
            Status == PowerSupplyBayStatus.Obstructed;

        public string FailureCode => Status switch
        {
            PowerSupplyBayStatus.ModeDisabled =>
                "assembly-power-supply.mode-disabled",
            PowerSupplyBayStatus.ContextMissing =>
                "assembly-power-supply.context-missing",
            PowerSupplyBayStatus.Paused =>
                "assembly-power-supply.paused",
            PowerSupplyBayStatus.AuthorityBlocked =>
                "assembly-power-supply.authority-blocked",
            PowerSupplyBayStatus.OutOfRange =>
                "assembly-power-supply.out-of-range",
            PowerSupplyBayStatus.NotFocused =>
                "assembly-power-supply.focus-missing",
            PowerSupplyBayStatus.LineOfSightBlocked =>
                "assembly-power-supply.line-of-sight-blocked",
            PowerSupplyBayStatus.FormFactorInvalid =>
                "assembly-power-supply.form-factor-mismatch",
            PowerSupplyBayStatus.OrientationInvalid =>
                "assembly-power-supply.orientation-mismatch",
            PowerSupplyBayStatus.Unsupported =>
                "assembly-power-supply.support-missing",
            PowerSupplyBayStatus.ChassisClearanceBlocked =>
                "assembly-power-supply.chassis-clearance-blocked",
            PowerSupplyBayStatus.CableClearanceBlocked =>
                "assembly-power-supply.cable-clearance-blocked",
            PowerSupplyBayStatus.Obstructed =>
                "assembly-power-supply.obstructed",
            PowerSupplyBayStatus.ValidSeatedUnsecuredRetentionBlocked =>
                "assembly-power-supply.retention-blocked",
            _ => string.Empty
        };
    }

    internal readonly struct PowerSupplyPhysicsHit
    {
        public PowerSupplyPhysicsHit(Collider collider, float distance)
        {
            Collider = collider;
            Distance = distance;
        }

        public Collider Collider { get; }

        public float Distance { get; }
    }

    internal interface IPowerSupplyBayPhysics
    {
        int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            PowerSupplyPhysicsHit[] results,
            float maximumDistance,
            int layerMask);

        bool RaycastCollider(Collider collider, Ray ray, float maximumDistance);

        int OverlapBoxNonAlloc(
            Vector3 center,
            Vector3 halfExtents,
            Collider[] results,
            Quaternion orientation,
            int layerMask);

        int BoxCastNonAlloc(
            Vector3 center,
            Vector3 halfExtents,
            Vector3 direction,
            PowerSupplyPhysicsHit[] results,
            Quaternion orientation,
            float maximumDistance,
            int layerMask);
    }

    /// <summary>
    /// Deterministic single-slot ATX PS/2 geometry gate. The returned pose is the exact
    /// pose consumed by both preview and commit. All physics paths are NonAlloc and
    /// fail closed on a tied line-of-sight hit or a saturated query buffer.
    /// </summary>
    public static class PowerSupplyBaySolver
    {
        internal const int HitCapacity = 32;
        internal const float DistanceTieEpsilon = 0.0001f;

        private const float RotationStepDegrees = 180f;
        private const float SupportProbeOffset = 0.025f;
        private const float SupportProbeDistance = 0.10f;
        private const float InsertionDistance = 0.18f;

        private static readonly PowerSupplyPhysicsHit[] LineHits =
            new PowerSupplyPhysicsHit[HitCapacity];
        private static readonly PowerSupplyPhysicsHit[] InsertionHits =
            new PowerSupplyPhysicsHit[HitCapacity];
        private static readonly Collider[] Overlaps = new Collider[HitCapacity];

        public static PowerSupplyBayEvaluation EvaluateSeat(
            bool placementModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection powerSupply,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            int halfTurns,
            bool paused,
            bool authorityAvailable,
            PowerSupplyFormFactor powerSupplyFormFactor,
            PowerSupplyFormFactor bayFormFactor,
            bool chassisClearanceAvailable,
            bool cableClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers = null,
            IReadOnlyList<Collider> cableClearanceBlockers = null)
        {
            return EvaluateSeat(
                placementModeEnabled,
                interactionOrigin,
                playerRoot,
                powerSupply,
                snapAnchor,
                focusCollider,
                supportCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                halfTurns,
                paused,
                authorityAvailable,
                powerSupplyFormFactor,
                bayFormFactor,
                chassisClearanceAvailable,
                cableClearanceAvailable,
                chassisClearanceBlockers,
                cableClearanceBlockers,
                UnityPowerSupplyBayPhysics.Instance);
        }

        public static PowerSupplyBayEvaluation EvaluateRecoverySeat(
            PhysicalItemProjection powerSupply,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            int halfTurns,
            bool authorityAvailable,
            PowerSupplyFormFactor powerSupplyFormFactor,
            PowerSupplyFormFactor bayFormFactor,
            bool chassisClearanceAvailable,
            bool cableClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers = null,
            IReadOnlyList<Collider> cableClearanceBlockers = null)
        {
            return EvaluateRecoverySeat(
                powerSupply,
                snapAnchor,
                focusCollider,
                supportCollider,
                assemblyRoot,
                obstructionMask,
                halfTurns,
                authorityAvailable,
                powerSupplyFormFactor,
                bayFormFactor,
                chassisClearanceAvailable,
                cableClearanceAvailable,
                chassisClearanceBlockers,
                cableClearanceBlockers,
                UnityPowerSupplyBayPhysics.Instance);
        }

        internal static PowerSupplyBayEvaluation EvaluateRecoverySeat(
            PhysicalItemProjection powerSupply,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            int halfTurns,
            bool authorityAvailable,
            PowerSupplyFormFactor powerSupplyFormFactor,
            PowerSupplyFormFactor bayFormFactor,
            bool chassisClearanceAvailable,
            bool cableClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> cableClearanceBlockers,
            IPowerSupplyBayPhysics physics)
        {
            if (powerSupply == null ||
                snapAnchor == null ||
                focusCollider == null ||
                supportCollider == null ||
                assemblyRoot == null ||
                physics == null ||
                !focusCollider.enabled ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return Invalid(PowerSupplyBayStatus.ContextMissing);
            }

            int normalizedHalfTurns = NormalizeHalfTurns(halfTurns);
            PowerSupplySeatOrientation orientation = normalizedHalfTurns == 0
                ? PowerSupplySeatOrientation.FanToFilteredVent
                : PowerSupplySeatOrientation.FanAwayFromFilteredVent;
            Pose candidatePose = ResolveSeatPose(snapAnchor, normalizedHalfTurns);
            if (!authorityAvailable)
            {
                return Invalid(
                    PowerSupplyBayStatus.AuthorityBlocked,
                    candidatePose,
                    orientation);
            }

            return EvaluateSeatGeometry(
                powerSupply,
                snapAnchor,
                focusCollider,
                supportCollider,
                null,
                assemblyRoot,
                obstructionMask,
                normalizedHalfTurns,
                candidatePose,
                orientation,
                powerSupplyFormFactor,
                bayFormFactor,
                chassisClearanceAvailable,
                cableClearanceAvailable,
                chassisClearanceBlockers,
                cableClearanceBlockers,
                physics);
        }

        internal static PowerSupplyBayEvaluation EvaluateSeat(
            bool placementModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection powerSupply,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            int halfTurns,
            bool paused,
            bool authorityAvailable,
            PowerSupplyFormFactor powerSupplyFormFactor,
            PowerSupplyFormFactor bayFormFactor,
            bool chassisClearanceAvailable,
            bool cableClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> cableClearanceBlockers,
            IPowerSupplyBayPhysics physics)
        {
            // This is intentionally the first branch. A disabled guided mode must not
            // touch scene context or issue even one physics query.
            if (!placementModeEnabled)
            {
                return Invalid(PowerSupplyBayStatus.ModeDisabled);
            }

            if (interactionOrigin == null ||
                powerSupply == null ||
                snapAnchor == null ||
                focusCollider == null ||
                supportCollider == null ||
                assemblyRoot == null ||
                physics == null ||
                !focusCollider.enabled ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return Invalid(PowerSupplyBayStatus.ContextMissing);
            }

            int normalizedHalfTurns = NormalizeHalfTurns(halfTurns);
            PowerSupplySeatOrientation orientation = normalizedHalfTurns == 0
                ? PowerSupplySeatOrientation.FanToFilteredVent
                : PowerSupplySeatOrientation.FanAwayFromFilteredVent;
            Pose candidatePose = ResolveSeatPose(snapAnchor, normalizedHalfTurns);

            if (paused)
            {
                return Invalid(
                    PowerSupplyBayStatus.Paused,
                    candidatePose,
                    orientation);
            }

            if (!authorityAvailable)
            {
                return Invalid(
                    PowerSupplyBayStatus.AuthorityBlocked,
                    candidatePose,
                    orientation);
            }

            PowerSupplyBayStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                powerSupply.transform,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics);
            if (focusStatus != PowerSupplyBayStatus.ValidSeat)
            {
                return Invalid(focusStatus, candidatePose, orientation);
            }

            return EvaluateSeatGeometry(
                powerSupply,
                snapAnchor,
                focusCollider,
                supportCollider,
                playerRoot,
                assemblyRoot,
                obstructionMask,
                normalizedHalfTurns,
                candidatePose,
                orientation,
                powerSupplyFormFactor,
                bayFormFactor,
                chassisClearanceAvailable,
                cableClearanceAvailable,
                chassisClearanceBlockers,
                cableClearanceBlockers,
                physics);
        }

        private static PowerSupplyBayEvaluation EvaluateSeatGeometry(
            PhysicalItemProjection powerSupply,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            int normalizedHalfTurns,
            Pose candidatePose,
            PowerSupplySeatOrientation orientation,
            PowerSupplyFormFactor powerSupplyFormFactor,
            PowerSupplyFormFactor bayFormFactor,
            bool chassisClearanceAvailable,
            bool cableClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> cableClearanceBlockers,
            IPowerSupplyBayPhysics physics)
        {
            if (powerSupplyFormFactor != PowerSupplyFormFactor.AtxPs2 ||
                bayFormFactor != PowerSupplyFormFactor.AtxPs2)
            {
                return Invalid(
                    PowerSupplyBayStatus.FormFactorInvalid,
                    candidatePose,
                    orientation);
            }

            if (normalizedHalfTurns != 0 ||
                orientation != PowerSupplySeatOrientation.FanToFilteredVent)
            {
                return Invalid(
                    PowerSupplyBayStatus.OrientationInvalid,
                    candidatePose,
                    orientation);
            }

            if (!chassisClearanceAvailable)
            {
                return Invalid(
                    PowerSupplyBayStatus.ChassisClearanceBlocked,
                    candidatePose,
                    orientation);
            }

            if (!cableClearanceAvailable)
            {
                return Invalid(
                    PowerSupplyBayStatus.CableClearanceBlocked,
                    candidatePose,
                    orientation);
            }

            Vector3 supportNormal = assemblyRoot.up.sqrMagnitude > Mathf.Epsilon
                ? assemblyRoot.up.normalized
                : Vector3.up;
            Vector3 insertionNormal = assemblyRoot.forward.sqrMagnitude > Mathf.Epsilon
                ? -assemblyRoot.forward.normalized
                : supportNormal;
            Ray supportRay = new Ray(
                candidatePose.position + supportNormal * SupportProbeOffset,
                -supportNormal);
            if (!supportCollider.enabled ||
                !supportCollider.gameObject.activeInHierarchy ||
                !physics.RaycastCollider(
                    supportCollider,
                    supportRay,
                    SupportProbeDistance))
            {
                return Invalid(
                    PowerSupplyBayStatus.Unsupported,
                    candidatePose,
                    orientation);
            }

            PowerSupplyBayStatus volumeStatus = EvaluateSeatVolume(
                powerSupply,
                candidatePose,
                insertionNormal,
                focusCollider,
                supportCollider,
                playerRoot,
                assemblyRoot,
                obstructionMask,
                chassisClearanceBlockers,
                cableClearanceBlockers,
                physics);
            if (volumeStatus != PowerSupplyBayStatus.ValidSeat)
            {
                return Invalid(volumeStatus, candidatePose, orientation);
            }

            return new PowerSupplyBayEvaluation(
                PowerSupplyBayStatus.ValidSeat,
                candidatePose,
                true,
                orientation);
        }

        public static PowerSupplyBayEvaluation EvaluateInteraction(
            bool interactionModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedPowerSupply,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            PowerSupplyBayProjectionState state,
            bool authorityAvailable,
            bool retentionAvailable)
        {
            return EvaluateInteraction(
                interactionModeEnabled,
                interactionOrigin,
                playerRoot,
                seatedPowerSupply,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                state,
                authorityAvailable,
                retentionAvailable,
                UnityPowerSupplyBayPhysics.Instance);
        }

        internal static PowerSupplyBayEvaluation EvaluateInteraction(
            bool interactionModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedPowerSupply,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            PowerSupplyBayProjectionState state,
            bool authorityAvailable,
            bool retentionAvailable,
            IPowerSupplyBayPhysics physics)
        {
            if (!interactionModeEnabled)
            {
                return Invalid(PowerSupplyBayStatus.ModeDisabled);
            }

            if (interactionOrigin == null ||
                focusCollider == null ||
                assemblyRoot == null ||
                physics == null)
            {
                return Invalid(PowerSupplyBayStatus.ContextMissing);
            }

            bool stateCanOperate =
                state == PowerSupplyBayProjectionState.PowerSupplySeatedUnsecured ||
                state == PowerSupplyBayProjectionState.PowerSupplyRetained;
            PowerSupplyBayStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                seatedPowerSupply,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics,
                paused,
                authorityAvailable && stateCanOperate);
            if (focusStatus != PowerSupplyBayStatus.ValidSeat)
            {
                return Invalid(focusStatus);
            }

            PowerSupplyBayStatus status =
                state == PowerSupplyBayProjectionState.PowerSupplyRetained
                    ? PowerSupplyBayStatus.ValidRetained
                    : retentionAvailable
                        ? PowerSupplyBayStatus.ValidSeatedUnsecured
                        : PowerSupplyBayStatus.ValidSeatedUnsecuredRetentionBlocked;
            return Invalid(status);
        }

        internal static Pose ResolveSeatPose(Transform snapAnchor, int halfTurns)
        {
            int normalizedHalfTurns = NormalizeHalfTurns(halfTurns);
            return new Pose(
                snapAnchor.position,
                snapAnchor.rotation * Quaternion.AngleAxis(
                    normalizedHalfTurns * RotationStepDegrees,
                    Vector3.forward));
        }

        private static PowerSupplyBayStatus EvaluateFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform itemRoot,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            IPowerSupplyBayPhysics physics,
            bool paused = false,
            bool authorityAvailable = true)
        {
            if (paused)
            {
                return PowerSupplyBayStatus.Paused;
            }

            if (!authorityAvailable)
            {
                return PowerSupplyBayStatus.AuthorityBlocked;
            }

            Vector3 toFocus = focusCollider.bounds.center - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon ||
                distance > Mathf.Max(0.1f, maximumRange))
            {
                return PowerSupplyBayStatus.OutOfRange;
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return PowerSupplyBayStatus.NotFocused;
            }

            int targetMask = 1 << focusCollider.gameObject.layer;
            int hitCount = physics.RaycastNonAlloc(
                interactionOrigin.position,
                direction,
                LineHits,
                distance + 0.03f,
                obstructionMask | targetMask);
            if (hitCount <= 0)
            {
                return PowerSupplyBayStatus.LineOfSightBlocked;
            }

            if (hitCount >= HitCapacity)
            {
                return PowerSupplyBayStatus.Obstructed;
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                PowerSupplyPhysicsHit hit = LineHits[index];
                if (hit.Collider == null ||
                    IsChildOf(hit.Collider.transform, playerRoot) ||
                    IsChildOf(hit.Collider.transform, itemRoot))
                {
                    continue;
                }

                if (hit.Collider == focusCollider)
                {
                    targetDistance = Mathf.Min(targetDistance, hit.Distance);
                    continue;
                }

                if (hit.Collider.isTrigger)
                {
                    continue;
                }

                if (IsChildOf(hit.Collider.transform, assemblyRoot))
                {
                    continue;
                }

                obstructionDistance = Mathf.Min(obstructionDistance, hit.Distance);
            }

            if (float.IsPositiveInfinity(targetDistance))
            {
                return PowerSupplyBayStatus.LineOfSightBlocked;
            }

            return obstructionDistance <= targetDistance + DistanceTieEpsilon
                ? PowerSupplyBayStatus.LineOfSightBlocked
                : PowerSupplyBayStatus.ValidSeat;
        }

        private static PowerSupplyBayStatus EvaluateSeatVolume(
            PhysicalItemProjection powerSupply,
            Pose candidatePose,
            Vector3 insertionNormal,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> cableClearanceBlockers,
            IPowerSupplyBayPhysics physics)
        {
            int overlapCount = physics.OverlapBoxNonAlloc(
                powerSupply.ResolveDropCenter(candidatePose),
                powerSupply.DropHalfExtents,
                Overlaps,
                candidatePose.rotation,
                obstructionMask);
            if (overlapCount >= HitCapacity)
            {
                return PowerSupplyBayStatus.Obstructed;
            }

            PowerSupplyBayStatus status = ClassifyColliders(
                Overlaps,
                overlapCount,
                focusCollider,
                supportCollider,
                playerRoot,
                powerSupply.transform,
                assemblyRoot,
                chassisClearanceBlockers,
                cableClearanceBlockers);

            int insertionCount = physics.BoxCastNonAlloc(
                powerSupply.ResolveDropCenter(candidatePose) +
                insertionNormal * InsertionDistance,
                powerSupply.DropHalfExtents,
                -insertionNormal,
                InsertionHits,
                candidatePose.rotation,
                InsertionDistance,
                obstructionMask);
            if (insertionCount >= HitCapacity)
            {
                return PowerSupplyBayStatus.Obstructed;
            }

            for (int index = 0; index < insertionCount; index++)
            {
                status = SelectDeterministicObstruction(
                    status,
                    ClassifyCollider(
                        InsertionHits[index].Collider,
                        focusCollider,
                        supportCollider,
                        playerRoot,
                        powerSupply.transform,
                        assemblyRoot,
                        chassisClearanceBlockers,
                        cableClearanceBlockers));
            }

            return status;
        }

        private static PowerSupplyBayStatus ClassifyColliders(
            Collider[] colliders,
            int count,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform itemRoot,
            Transform assemblyRoot,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> cableClearanceBlockers)
        {
            PowerSupplyBayStatus status = PowerSupplyBayStatus.ValidSeat;
            for (int index = 0; index < count; index++)
            {
                status = SelectDeterministicObstruction(
                    status,
                    ClassifyCollider(
                        colliders[index],
                        focusCollider,
                        supportCollider,
                        playerRoot,
                        itemRoot,
                        assemblyRoot,
                        chassisClearanceBlockers,
                        cableClearanceBlockers));
            }

            return status;
        }

        private static PowerSupplyBayStatus ClassifyCollider(
            Collider collider,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform itemRoot,
            Transform assemblyRoot,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> cableClearanceBlockers)
        {
            if (collider == null ||
                collider.isTrigger ||
                collider == focusCollider ||
                collider == supportCollider ||
                IsChildOf(collider.transform, playerRoot) ||
                IsChildOf(collider.transform, itemRoot))
            {
                return PowerSupplyBayStatus.ValidSeat;
            }

            if (Contains(chassisClearanceBlockers, collider))
            {
                return PowerSupplyBayStatus.ChassisClearanceBlocked;
            }

            if (Contains(cableClearanceBlockers, collider))
            {
                return PowerSupplyBayStatus.CableClearanceBlocked;
            }

            return IsChildOf(collider.transform, assemblyRoot)
                ? PowerSupplyBayStatus.ValidSeat
                : PowerSupplyBayStatus.Obstructed;
        }

        private static PowerSupplyBayStatus SelectDeterministicObstruction(
            PowerSupplyBayStatus current,
            PowerSupplyBayStatus candidate)
        {
            return ObstructionRank(candidate) > ObstructionRank(current)
                ? candidate
                : current;
        }

        private static int ObstructionRank(PowerSupplyBayStatus status)
        {
            return status switch
            {
                PowerSupplyBayStatus.ChassisClearanceBlocked => 3,
                PowerSupplyBayStatus.CableClearanceBlocked => 2,
                PowerSupplyBayStatus.Obstructed => 1,
                _ => 0
            };
        }

        private static bool Contains(
            IReadOnlyList<Collider> colliders,
            Collider candidate)
        {
            if (colliders == null)
            {
                return false;
            }

            for (int index = 0; index < colliders.Count; index++)
            {
                if (colliders[index] == candidate)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsChildOf(Transform candidate, Transform root)
        {
            return candidate != null && root != null && candidate.IsChildOf(root);
        }

        private static int NormalizeHalfTurns(int halfTurns)
        {
            return ((halfTurns % 2) + 2) % 2;
        }

        private static PowerSupplyBayEvaluation Invalid(
            PowerSupplyBayStatus status,
            Pose pose = default,
            PowerSupplySeatOrientation orientation = default)
        {
            bool hasPose = status != PowerSupplyBayStatus.Uninitialized &&
                           status != PowerSupplyBayStatus.ModeDisabled &&
                           status != PowerSupplyBayStatus.ContextMissing &&
                           status != PowerSupplyBayStatus.ValidSeatedUnsecured &&
                           status != PowerSupplyBayStatus.ValidRetained &&
                           status != PowerSupplyBayStatus.ValidSeatedUnsecuredRetentionBlocked;
            return new PowerSupplyBayEvaluation(status, pose, hasPose, orientation);
        }

        private sealed class UnityPowerSupplyBayPhysics : IPowerSupplyBayPhysics
        {
            internal static readonly UnityPowerSupplyBayPhysics Instance = new();

            private readonly RaycastHit[] _raycastHits = new RaycastHit[HitCapacity];
            private readonly RaycastHit[] _boxCastHits = new RaycastHit[HitCapacity];

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                PowerSupplyPhysicsHit[] results,
                float maximumDistance,
                int layerMask)
            {
                int count = Physics.RaycastNonAlloc(
                    origin,
                    direction,
                    _raycastHits,
                    maximumDistance,
                    layerMask,
                    QueryTriggerInteraction.Collide);
                CopyHits(_raycastHits, results, count);
                return count;
            }

            public bool RaycastCollider(
                Collider collider,
                Ray ray,
                float maximumDistance)
            {
                return collider != null && collider.Raycast(ray, out _, maximumDistance);
            }

            public int OverlapBoxNonAlloc(
                Vector3 center,
                Vector3 halfExtents,
                Collider[] results,
                Quaternion orientation,
                int layerMask)
            {
                return Physics.OverlapBoxNonAlloc(
                    center,
                    halfExtents,
                    results,
                    orientation,
                    layerMask,
                    QueryTriggerInteraction.Ignore);
            }

            public int BoxCastNonAlloc(
                Vector3 center,
                Vector3 halfExtents,
                Vector3 direction,
                PowerSupplyPhysicsHit[] results,
                Quaternion orientation,
                float maximumDistance,
                int layerMask)
            {
                int count = Physics.BoxCastNonAlloc(
                    center,
                    halfExtents,
                    direction,
                    _boxCastHits,
                    orientation,
                    maximumDistance,
                    layerMask,
                    QueryTriggerInteraction.Ignore);
                CopyHits(_boxCastHits, results, count);
                return count;
            }

            private static void CopyHits(
                RaycastHit[] source,
                PowerSupplyPhysicsHit[] destination,
                int count)
            {
                int copyCount = Mathf.Min(count, destination.Length);
                for (int index = 0; index < copyCount; index++)
                {
                    destination[index] = new PowerSupplyPhysicsHit(
                        source[index].collider,
                        source[index].distance);
                }
            }
        }
    }
}
