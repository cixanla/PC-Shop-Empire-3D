using System.Collections.Generic;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum GraphicsCardPcieInterface
    {
        Unknown = 0,
        PcieX16 = 1
    }

    public enum GraphicsCardSeatOrientation
    {
        Primary = 0,
        Rotated180 = 1
    }

    public enum GraphicsCardSlotProjectionState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        GraphicsCardSeatedUnsecured = 2,
        GraphicsCardRetained = 3
    }

    public enum GraphicsCardSlotStatus
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
        InterfaceInvalid = 11,
        OrientationInvalid = 12,
        Unsupported = 13,
        ChassisClearanceBlocked = 14,
        CoolerClearanceBlocked = 15,
        Obstructed = 16,
        ValidSeatedUnsecuredRetentionBlocked = 17
    }

    public readonly struct GraphicsCardSlotEvaluation
    {
        public GraphicsCardSlotEvaluation(
            GraphicsCardSlotStatus status,
            Pose pose,
            bool hasPose,
            GraphicsCardSeatOrientation orientation)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
            Orientation = orientation;
        }

        public GraphicsCardSlotStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public GraphicsCardSeatOrientation Orientation { get; }

        public bool CanSeat =>
            Status == GraphicsCardSlotStatus.ValidSeat &&
            HasPose &&
            Orientation == GraphicsCardSeatOrientation.Primary;

        public bool CanOperateRetention =>
            Status == GraphicsCardSlotStatus.ValidSeatedUnsecured ||
            Status == GraphicsCardSlotStatus.ValidRetained;

        public bool CanRemove =>
            Status == GraphicsCardSlotStatus.ValidSeatedUnsecured ||
            Status == GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked;

        public bool HasOwnedContext =>
            CanSeat ||
            CanOperateRetention ||
            CanRemove ||
            Status == GraphicsCardSlotStatus.LineOfSightBlocked ||
            Status == GraphicsCardSlotStatus.InterfaceInvalid ||
            Status == GraphicsCardSlotStatus.OrientationInvalid ||
            Status == GraphicsCardSlotStatus.Unsupported ||
            Status == GraphicsCardSlotStatus.ChassisClearanceBlocked ||
            Status == GraphicsCardSlotStatus.CoolerClearanceBlocked ||
            Status == GraphicsCardSlotStatus.Obstructed;

        public string FailureCode => Status switch
        {
            GraphicsCardSlotStatus.ModeDisabled =>
                "assembly-graphics-card.mode-disabled",
            GraphicsCardSlotStatus.ContextMissing =>
                "assembly-graphics-card.context-missing",
            GraphicsCardSlotStatus.Paused =>
                "assembly-graphics-card.paused",
            GraphicsCardSlotStatus.AuthorityBlocked =>
                "assembly-graphics-card.authority-blocked",
            GraphicsCardSlotStatus.OutOfRange =>
                "assembly-graphics-card.out-of-range",
            GraphicsCardSlotStatus.NotFocused =>
                "assembly-graphics-card.focus-missing",
            GraphicsCardSlotStatus.LineOfSightBlocked =>
                "assembly-graphics-card.line-of-sight-blocked",
            GraphicsCardSlotStatus.InterfaceInvalid =>
                "assembly-graphics-card.interface-mismatch",
            GraphicsCardSlotStatus.OrientationInvalid =>
                "assembly-graphics-card.orientation-mismatch",
            GraphicsCardSlotStatus.Unsupported =>
                "assembly-graphics-card.support-missing",
            GraphicsCardSlotStatus.ChassisClearanceBlocked =>
                "assembly-graphics-card.chassis-clearance-blocked",
            GraphicsCardSlotStatus.CoolerClearanceBlocked =>
                "assembly-graphics-card.cooler-clearance-blocked",
            GraphicsCardSlotStatus.Obstructed =>
                "assembly-graphics-card.obstructed",
            GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked =>
                "assembly-graphics-card.host-unsecured",
            _ => string.Empty
        };
    }

    internal readonly struct GraphicsCardPhysicsHit
    {
        public GraphicsCardPhysicsHit(Collider collider, float distance)
        {
            Collider = collider;
            Distance = distance;
        }

        public Collider Collider { get; }

        public float Distance { get; }
    }

    internal interface IGraphicsCardSlotPhysics
    {
        int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            GraphicsCardPhysicsHit[] results,
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
            GraphicsCardPhysicsHit[] results,
            Quaternion orientation,
            float maximumDistance,
            int layerMask);
    }

    /// <summary>
    /// Deterministic single-slot PCIe x16 geometry gate. The returned pose is the exact
    /// pose consumed by both preview and commit. All physics paths are NonAlloc and
    /// fail closed on a tied line-of-sight hit or a saturated query buffer.
    /// </summary>
    public static class GraphicsCardSlotSolver
    {
        internal const int HitCapacity = 32;
        internal const float DistanceTieEpsilon = 0.0001f;

        private const float RotationStepDegrees = 180f;
        private const float SupportProbeOffset = 0.025f;
        private const float SupportProbeDistance = 0.10f;
        private const float InsertionDistance = 0.18f;

        private static readonly GraphicsCardPhysicsHit[] LineHits =
            new GraphicsCardPhysicsHit[HitCapacity];
        private static readonly GraphicsCardPhysicsHit[] InsertionHits =
            new GraphicsCardPhysicsHit[HitCapacity];
        private static readonly Collider[] Overlaps = new Collider[HitCapacity];

        public static GraphicsCardSlotEvaluation EvaluateSeat(
            bool placementModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection graphicsCard,
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
            GraphicsCardPcieInterface graphicsCardInterface,
            GraphicsCardPcieInterface slotInterface,
            bool chassisClearanceAvailable,
            bool coolerClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers = null,
            IReadOnlyList<Collider> coolerClearanceBlockers = null)
        {
            return EvaluateSeat(
                placementModeEnabled,
                interactionOrigin,
                playerRoot,
                graphicsCard,
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
                graphicsCardInterface,
                slotInterface,
                chassisClearanceAvailable,
                coolerClearanceAvailable,
                chassisClearanceBlockers,
                coolerClearanceBlockers,
                UnityGraphicsCardSlotPhysics.Instance);
        }

        public static GraphicsCardSlotEvaluation EvaluateRecoverySeat(
            PhysicalItemProjection graphicsCard,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            int halfTurns,
            bool authorityAvailable,
            GraphicsCardPcieInterface graphicsCardInterface,
            GraphicsCardPcieInterface slotInterface,
            bool chassisClearanceAvailable,
            bool coolerClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers = null,
            IReadOnlyList<Collider> coolerClearanceBlockers = null)
        {
            return EvaluateRecoverySeat(
                graphicsCard,
                snapAnchor,
                focusCollider,
                supportCollider,
                assemblyRoot,
                obstructionMask,
                halfTurns,
                authorityAvailable,
                graphicsCardInterface,
                slotInterface,
                chassisClearanceAvailable,
                coolerClearanceAvailable,
                chassisClearanceBlockers,
                coolerClearanceBlockers,
                UnityGraphicsCardSlotPhysics.Instance);
        }

        internal static GraphicsCardSlotEvaluation EvaluateRecoverySeat(
            PhysicalItemProjection graphicsCard,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            int halfTurns,
            bool authorityAvailable,
            GraphicsCardPcieInterface graphicsCardInterface,
            GraphicsCardPcieInterface slotInterface,
            bool chassisClearanceAvailable,
            bool coolerClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> coolerClearanceBlockers,
            IGraphicsCardSlotPhysics physics)
        {
            if (graphicsCard == null ||
                snapAnchor == null ||
                focusCollider == null ||
                supportCollider == null ||
                assemblyRoot == null ||
                physics == null ||
                !focusCollider.enabled ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return Invalid(GraphicsCardSlotStatus.ContextMissing);
            }

            int normalizedHalfTurns = NormalizeHalfTurns(halfTurns);
            GraphicsCardSeatOrientation orientation = normalizedHalfTurns == 0
                ? GraphicsCardSeatOrientation.Primary
                : GraphicsCardSeatOrientation.Rotated180;
            Pose candidatePose = ResolveSeatPose(snapAnchor, normalizedHalfTurns);
            if (!authorityAvailable)
            {
                return Invalid(
                    GraphicsCardSlotStatus.AuthorityBlocked,
                    candidatePose,
                    orientation);
            }

            return EvaluateSeatGeometry(
                graphicsCard,
                snapAnchor,
                focusCollider,
                supportCollider,
                null,
                assemblyRoot,
                obstructionMask,
                normalizedHalfTurns,
                candidatePose,
                orientation,
                graphicsCardInterface,
                slotInterface,
                chassisClearanceAvailable,
                coolerClearanceAvailable,
                chassisClearanceBlockers,
                coolerClearanceBlockers,
                physics);
        }

        internal static GraphicsCardSlotEvaluation EvaluateSeat(
            bool placementModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection graphicsCard,
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
            GraphicsCardPcieInterface graphicsCardInterface,
            GraphicsCardPcieInterface slotInterface,
            bool chassisClearanceAvailable,
            bool coolerClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> coolerClearanceBlockers,
            IGraphicsCardSlotPhysics physics)
        {
            // This is intentionally the first branch. A disabled guided mode must not
            // touch scene context or issue even one physics query.
            if (!placementModeEnabled)
            {
                return Invalid(GraphicsCardSlotStatus.ModeDisabled);
            }

            if (interactionOrigin == null ||
                graphicsCard == null ||
                snapAnchor == null ||
                focusCollider == null ||
                supportCollider == null ||
                assemblyRoot == null ||
                physics == null ||
                !focusCollider.enabled ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return Invalid(GraphicsCardSlotStatus.ContextMissing);
            }

            int normalizedHalfTurns = NormalizeHalfTurns(halfTurns);
            GraphicsCardSeatOrientation orientation = normalizedHalfTurns == 0
                ? GraphicsCardSeatOrientation.Primary
                : GraphicsCardSeatOrientation.Rotated180;
            Pose candidatePose = ResolveSeatPose(snapAnchor, normalizedHalfTurns);

            if (paused)
            {
                return Invalid(
                    GraphicsCardSlotStatus.Paused,
                    candidatePose,
                    orientation);
            }

            if (!authorityAvailable)
            {
                return Invalid(
                    GraphicsCardSlotStatus.AuthorityBlocked,
                    candidatePose,
                    orientation);
            }

            GraphicsCardSlotStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                graphicsCard.transform,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics);
            if (focusStatus != GraphicsCardSlotStatus.ValidSeat)
            {
                return Invalid(focusStatus, candidatePose, orientation);
            }

            return EvaluateSeatGeometry(
                graphicsCard,
                snapAnchor,
                focusCollider,
                supportCollider,
                playerRoot,
                assemblyRoot,
                obstructionMask,
                normalizedHalfTurns,
                candidatePose,
                orientation,
                graphicsCardInterface,
                slotInterface,
                chassisClearanceAvailable,
                coolerClearanceAvailable,
                chassisClearanceBlockers,
                coolerClearanceBlockers,
                physics);
        }

        private static GraphicsCardSlotEvaluation EvaluateSeatGeometry(
            PhysicalItemProjection graphicsCard,
            Transform snapAnchor,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            int normalizedHalfTurns,
            Pose candidatePose,
            GraphicsCardSeatOrientation orientation,
            GraphicsCardPcieInterface graphicsCardInterface,
            GraphicsCardPcieInterface slotInterface,
            bool chassisClearanceAvailable,
            bool coolerClearanceAvailable,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> coolerClearanceBlockers,
            IGraphicsCardSlotPhysics physics)
        {
            if (graphicsCardInterface != GraphicsCardPcieInterface.PcieX16 ||
                slotInterface != GraphicsCardPcieInterface.PcieX16)
            {
                return Invalid(
                    GraphicsCardSlotStatus.InterfaceInvalid,
                    candidatePose,
                    orientation);
            }

            if (normalizedHalfTurns != 0 ||
                orientation != GraphicsCardSeatOrientation.Primary)
            {
                return Invalid(
                    GraphicsCardSlotStatus.OrientationInvalid,
                    candidatePose,
                    orientation);
            }

            if (!chassisClearanceAvailable)
            {
                return Invalid(
                    GraphicsCardSlotStatus.ChassisClearanceBlocked,
                    candidatePose,
                    orientation);
            }

            if (!coolerClearanceAvailable)
            {
                return Invalid(
                    GraphicsCardSlotStatus.CoolerClearanceBlocked,
                    candidatePose,
                    orientation);
            }

            Vector3 insertionNormal = snapAnchor.forward.sqrMagnitude > Mathf.Epsilon
                ? snapAnchor.forward.normalized
                : Vector3.forward;
            Ray supportRay = new Ray(
                candidatePose.position + insertionNormal * SupportProbeOffset,
                -insertionNormal);
            if (!supportCollider.enabled ||
                !supportCollider.gameObject.activeInHierarchy ||
                !physics.RaycastCollider(
                    supportCollider,
                    supportRay,
                    SupportProbeDistance))
            {
                return Invalid(
                    GraphicsCardSlotStatus.Unsupported,
                    candidatePose,
                    orientation);
            }

            GraphicsCardSlotStatus volumeStatus = EvaluateSeatVolume(
                graphicsCard,
                candidatePose,
                insertionNormal,
                focusCollider,
                supportCollider,
                playerRoot,
                assemblyRoot,
                obstructionMask,
                chassisClearanceBlockers,
                coolerClearanceBlockers,
                physics);
            if (volumeStatus != GraphicsCardSlotStatus.ValidSeat)
            {
                return Invalid(volumeStatus, candidatePose, orientation);
            }

            return new GraphicsCardSlotEvaluation(
                GraphicsCardSlotStatus.ValidSeat,
                candidatePose,
                true,
                orientation);
        }

        public static GraphicsCardSlotEvaluation EvaluateInteraction(
            bool interactionModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedGraphicsCard,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            GraphicsCardSlotProjectionState state,
            bool authorityAvailable,
            bool retentionAvailable)
        {
            return EvaluateInteraction(
                interactionModeEnabled,
                interactionOrigin,
                playerRoot,
                seatedGraphicsCard,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                state,
                authorityAvailable,
                retentionAvailable,
                UnityGraphicsCardSlotPhysics.Instance);
        }

        internal static GraphicsCardSlotEvaluation EvaluateInteraction(
            bool interactionModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedGraphicsCard,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            GraphicsCardSlotProjectionState state,
            bool authorityAvailable,
            bool retentionAvailable,
            IGraphicsCardSlotPhysics physics)
        {
            if (!interactionModeEnabled)
            {
                return Invalid(GraphicsCardSlotStatus.ModeDisabled);
            }

            if (interactionOrigin == null ||
                focusCollider == null ||
                assemblyRoot == null ||
                physics == null)
            {
                return Invalid(GraphicsCardSlotStatus.ContextMissing);
            }

            bool stateCanOperate =
                state == GraphicsCardSlotProjectionState.GraphicsCardSeatedUnsecured ||
                state == GraphicsCardSlotProjectionState.GraphicsCardRetained;
            GraphicsCardSlotStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                seatedGraphicsCard,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics,
                paused,
                authorityAvailable && stateCanOperate);
            if (focusStatus != GraphicsCardSlotStatus.ValidSeat)
            {
                return Invalid(focusStatus);
            }

            GraphicsCardSlotStatus status =
                state == GraphicsCardSlotProjectionState.GraphicsCardRetained
                    ? GraphicsCardSlotStatus.ValidRetained
                    : retentionAvailable
                        ? GraphicsCardSlotStatus.ValidSeatedUnsecured
                        : GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked;
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

        private static GraphicsCardSlotStatus EvaluateFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform itemRoot,
            Collider focusCollider,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            IGraphicsCardSlotPhysics physics,
            bool paused = false,
            bool authorityAvailable = true)
        {
            if (paused)
            {
                return GraphicsCardSlotStatus.Paused;
            }

            if (!authorityAvailable)
            {
                return GraphicsCardSlotStatus.AuthorityBlocked;
            }

            Vector3 toFocus = focusCollider.bounds.center - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon ||
                distance > Mathf.Max(0.1f, maximumRange))
            {
                return GraphicsCardSlotStatus.OutOfRange;
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return GraphicsCardSlotStatus.NotFocused;
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
                return GraphicsCardSlotStatus.LineOfSightBlocked;
            }

            if (hitCount >= HitCapacity)
            {
                return GraphicsCardSlotStatus.Obstructed;
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                GraphicsCardPhysicsHit hit = LineHits[index];
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
                return GraphicsCardSlotStatus.LineOfSightBlocked;
            }

            return obstructionDistance <= targetDistance + DistanceTieEpsilon
                ? GraphicsCardSlotStatus.LineOfSightBlocked
                : GraphicsCardSlotStatus.ValidSeat;
        }

        private static GraphicsCardSlotStatus EvaluateSeatVolume(
            PhysicalItemProjection graphicsCard,
            Pose candidatePose,
            Vector3 insertionNormal,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform assemblyRoot,
            LayerMask obstructionMask,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> coolerClearanceBlockers,
            IGraphicsCardSlotPhysics physics)
        {
            int overlapCount = physics.OverlapBoxNonAlloc(
                graphicsCard.ResolveDropCenter(candidatePose),
                graphicsCard.DropHalfExtents,
                Overlaps,
                candidatePose.rotation,
                obstructionMask);
            if (overlapCount >= HitCapacity)
            {
                return GraphicsCardSlotStatus.Obstructed;
            }

            GraphicsCardSlotStatus status = ClassifyColliders(
                Overlaps,
                overlapCount,
                focusCollider,
                supportCollider,
                playerRoot,
                graphicsCard.transform,
                assemblyRoot,
                chassisClearanceBlockers,
                coolerClearanceBlockers);

            int insertionCount = physics.BoxCastNonAlloc(
                graphicsCard.ResolveDropCenter(candidatePose) +
                insertionNormal * InsertionDistance,
                graphicsCard.DropHalfExtents,
                -insertionNormal,
                InsertionHits,
                candidatePose.rotation,
                InsertionDistance,
                obstructionMask);
            if (insertionCount >= HitCapacity)
            {
                return GraphicsCardSlotStatus.Obstructed;
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
                        graphicsCard.transform,
                        assemblyRoot,
                        chassisClearanceBlockers,
                        coolerClearanceBlockers));
            }

            return status;
        }

        private static GraphicsCardSlotStatus ClassifyColliders(
            Collider[] colliders,
            int count,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform itemRoot,
            Transform assemblyRoot,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> coolerClearanceBlockers)
        {
            GraphicsCardSlotStatus status = GraphicsCardSlotStatus.ValidSeat;
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
                        coolerClearanceBlockers));
            }

            return status;
        }

        private static GraphicsCardSlotStatus ClassifyCollider(
            Collider collider,
            Collider focusCollider,
            Collider supportCollider,
            Transform playerRoot,
            Transform itemRoot,
            Transform assemblyRoot,
            IReadOnlyList<Collider> chassisClearanceBlockers,
            IReadOnlyList<Collider> coolerClearanceBlockers)
        {
            if (collider == null ||
                collider == focusCollider ||
                collider == supportCollider ||
                IsChildOf(collider.transform, playerRoot) ||
                IsChildOf(collider.transform, itemRoot))
            {
                return GraphicsCardSlotStatus.ValidSeat;
            }

            if (Contains(chassisClearanceBlockers, collider))
            {
                return GraphicsCardSlotStatus.ChassisClearanceBlocked;
            }

            if (Contains(coolerClearanceBlockers, collider))
            {
                return GraphicsCardSlotStatus.CoolerClearanceBlocked;
            }

            return IsChildOf(collider.transform, assemblyRoot)
                ? GraphicsCardSlotStatus.ValidSeat
                : GraphicsCardSlotStatus.Obstructed;
        }

        private static GraphicsCardSlotStatus SelectDeterministicObstruction(
            GraphicsCardSlotStatus current,
            GraphicsCardSlotStatus candidate)
        {
            return ObstructionRank(candidate) > ObstructionRank(current)
                ? candidate
                : current;
        }

        private static int ObstructionRank(GraphicsCardSlotStatus status)
        {
            return status switch
            {
                GraphicsCardSlotStatus.ChassisClearanceBlocked => 3,
                GraphicsCardSlotStatus.CoolerClearanceBlocked => 2,
                GraphicsCardSlotStatus.Obstructed => 1,
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

        private static GraphicsCardSlotEvaluation Invalid(
            GraphicsCardSlotStatus status,
            Pose pose = default,
            GraphicsCardSeatOrientation orientation = default)
        {
            bool hasPose = status != GraphicsCardSlotStatus.Uninitialized &&
                           status != GraphicsCardSlotStatus.ModeDisabled &&
                           status != GraphicsCardSlotStatus.ContextMissing &&
                           status != GraphicsCardSlotStatus.ValidSeatedUnsecured &&
                           status != GraphicsCardSlotStatus.ValidRetained &&
                           status != GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked;
            return new GraphicsCardSlotEvaluation(status, pose, hasPose, orientation);
        }

        private sealed class UnityGraphicsCardSlotPhysics : IGraphicsCardSlotPhysics
        {
            internal static readonly UnityGraphicsCardSlotPhysics Instance = new();

            private readonly RaycastHit[] _raycastHits = new RaycastHit[HitCapacity];
            private readonly RaycastHit[] _boxCastHits = new RaycastHit[HitCapacity];

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                GraphicsCardPhysicsHit[] results,
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
                GraphicsCardPhysicsHit[] results,
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
                GraphicsCardPhysicsHit[] destination,
                int count)
            {
                int copyCount = Mathf.Min(count, destination.Length);
                for (int index = 0; index < copyCount; index++)
                {
                    destination[index] = new GraphicsCardPhysicsHit(
                        source[index].collider,
                        source[index].distance);
                }
            }
        }
    }
}
