using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum Atx24PowerCableRouteStatus
    {
        Uninitialized = 0,
        ModeDisabled = 1,
        ValidRoute = 2,
        ContextMissing = 3,
        Paused = 4,
        AuthorityBlocked = 5,
        HostMotherboardUnsecured = 6,
        HostPowerSupplyUnretained = 7,
        OutOfRange = 8,
        NotFocused = 9,
        LineOfSightBlocked = 10,
        OrientationInvalid = 11,
        RouteObstructed = 12,
        QuerySaturated = 13
    }

    public readonly struct Atx24PowerCableRouteEvaluation
    {
        public Atx24PowerCableRouteEvaluation(
            Atx24PowerCableRouteStatus status,
            Pose pose,
            bool hasPose,
            PowerCableKeyOrientation orientation)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
            Orientation = orientation;
        }

        public Atx24PowerCableRouteStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public PowerCableKeyOrientation Orientation { get; }

        public bool CanRoute =>
            Status == Atx24PowerCableRouteStatus.ValidRoute &&
            HasPose &&
            Orientation == PowerCableKeyOrientation.Keyed;

        public bool HasPreview =>
            HasPose &&
            Status != Atx24PowerCableRouteStatus.ModeDisabled &&
            Status != Atx24PowerCableRouteStatus.ContextMissing;

        public string FailureCode => Status switch
        {
            Atx24PowerCableRouteStatus.ModeDisabled =>
                "assembly-power-cable.mode-disabled",
            Atx24PowerCableRouteStatus.ContextMissing =>
                "assembly-power-cable.context-missing",
            Atx24PowerCableRouteStatus.Paused =>
                "assembly-power-cable.paused",
            Atx24PowerCableRouteStatus.AuthorityBlocked =>
                "assembly-power-cable.authority-blocked",
            Atx24PowerCableRouteStatus.HostMotherboardUnsecured =>
                "assembly-power-cable.host-motherboard-unsecured",
            Atx24PowerCableRouteStatus.HostPowerSupplyUnretained =>
                "assembly-power-cable.host-power-supply-unretained",
            Atx24PowerCableRouteStatus.OutOfRange =>
                "assembly-power-cable.out-of-range",
            Atx24PowerCableRouteStatus.NotFocused =>
                "assembly-power-cable.focus-missing",
            Atx24PowerCableRouteStatus.LineOfSightBlocked =>
                "assembly-power-cable.line-of-sight-blocked",
            Atx24PowerCableRouteStatus.OrientationInvalid =>
                "assembly-power-cable.orientation-mismatch",
            Atx24PowerCableRouteStatus.RouteObstructed =>
                "assembly-power-cable.route-obstructed",
            Atx24PowerCableRouteStatus.QuerySaturated =>
                "assembly-power-cable.query-saturated",
            _ => string.Empty
        };
    }

    internal readonly struct Atx24PowerCablePhysicsHit
    {
        internal Atx24PowerCablePhysicsHit(Collider collider, float distance)
        {
            Collider = collider;
            Distance = distance;
        }

        internal Collider Collider { get; }

        internal float Distance { get; }
    }

    internal interface IAtx24PowerCableRoutePhysics
    {
        int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            Atx24PowerCablePhysicsHit[] results,
            float maximumDistance,
            int layerMask);

        int OverlapCapsuleNonAlloc(
            Vector3 point0,
            Vector3 point1,
            float radius,
            Collider[] results,
            int layerMask);
    }

    /// <summary>
    /// Deterministic, allocation-free gate for the authored ATX24 route. The disabled
    /// branch is intentionally first so an inactive guided mode performs zero physics
    /// queries. Every saturated query fails closed instead of accepting a partial view.
    /// </summary>
    public static class Atx24PowerCableRouteSolver
    {
        internal const int HitCapacity = 32;
        internal const float DistanceTieEpsilon = 0.0001f;

        private static readonly Atx24PowerCablePhysicsHit[] LineHits =
            new Atx24PowerCablePhysicsHit[HitCapacity];
        private static readonly Collider[] RouteOverlaps =
            new Collider[HitCapacity];

        public static Atx24PowerCableRouteStatus EvaluateRoutedFocus(
            bool authoritativeRouted,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform powerSupplyHostRoot,
            Transform motherboardHostRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused)
        {
            return EvaluateRoutedFocus(
                authoritativeRouted,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                powerSupplyHostRoot,
                motherboardHostRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                UnityAtx24PowerCableRoutePhysics.Instance);
        }

        internal static Atx24PowerCableRouteStatus EvaluateRoutedFocus(
            bool authoritativeRouted,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform powerSupplyHostRoot,
            Transform motherboardHostRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            IAtx24PowerCableRoutePhysics physics)
        {
            if (!authoritativeRouted)
            {
                return Atx24PowerCableRouteStatus.ModeDisabled;
            }

            if (interactionOrigin == null ||
                cable == null ||
                focusCollider == null ||
                routeRoot == null ||
                powerSupplyHostRoot == null ||
                motherboardHostRoot == null ||
                physics == null ||
                !focusCollider.enabled ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return Atx24PowerCableRouteStatus.ContextMissing;
            }

            if (paused)
            {
                return Atx24PowerCableRouteStatus.Paused;
            }

            return EvaluateFocus(
                interactionOrigin,
                playerRoot,
                cable.transform,
                focusCollider,
                routeRoot,
                powerSupplyHostRoot,
                motherboardHostRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics);
        }

        public static Atx24PowerCableRouteEvaluation Evaluate(
            bool routeModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform psuPrimaryEndpoint,
            Transform psuSenseEndpoint,
            Transform motherboardEndpoint,
            Transform firstWaypoint,
            Transform secondWaypoint,
            Transform thirdWaypoint,
            Transform powerSupplyHostRoot,
            Transform motherboardHostRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            float routeRadius,
            bool paused,
            bool authorityAvailable,
            bool motherboardSecured,
            bool powerSupplyRetained,
            PowerCableKeyOrientation orientation)
        {
            return Evaluate(
                routeModeEnabled,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                psuPrimaryEndpoint,
                psuSenseEndpoint,
                motherboardEndpoint,
                firstWaypoint,
                secondWaypoint,
                thirdWaypoint,
                powerSupplyHostRoot,
                motherboardHostRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                routeRadius,
                paused,
                authorityAvailable,
                motherboardSecured,
                powerSupplyRetained,
                orientation,
                UnityAtx24PowerCableRoutePhysics.Instance);
        }

        internal static Atx24PowerCableRouteEvaluation Evaluate(
            bool routeModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform psuPrimaryEndpoint,
            Transform psuSenseEndpoint,
            Transform motherboardEndpoint,
            Transform firstWaypoint,
            Transform secondWaypoint,
            Transform thirdWaypoint,
            Transform powerSupplyHostRoot,
            Transform motherboardHostRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            float routeRadius,
            bool paused,
            bool authorityAvailable,
            bool motherboardSecured,
            bool powerSupplyRetained,
            PowerCableKeyOrientation orientation,
            IAtx24PowerCableRoutePhysics physics)
        {
            if (!routeModeEnabled)
            {
                return Invalid(Atx24PowerCableRouteStatus.ModeDisabled);
            }

            if (interactionOrigin == null ||
                cable == null ||
                focusCollider == null ||
                routeRoot == null ||
                psuPrimaryEndpoint == null ||
                psuSenseEndpoint == null ||
                motherboardEndpoint == null ||
                firstWaypoint == null ||
                secondWaypoint == null ||
                thirdWaypoint == null ||
                powerSupplyHostRoot == null ||
                motherboardHostRoot == null ||
                physics == null ||
                !focusCollider.enabled ||
                !focusCollider.gameObject.activeInHierarchy)
            {
                return Invalid(Atx24PowerCableRouteStatus.ContextMissing);
            }

            Pose routedPose = new Pose(
                motherboardEndpoint.position,
                motherboardEndpoint.rotation);
            if (paused)
            {
                return Invalid(
                    Atx24PowerCableRouteStatus.Paused,
                    routedPose,
                    orientation);
            }

            if (!authorityAvailable)
            {
                return Invalid(
                    Atx24PowerCableRouteStatus.AuthorityBlocked,
                    routedPose,
                    orientation);
            }

            if (!motherboardSecured)
            {
                return Invalid(
                    Atx24PowerCableRouteStatus.HostMotherboardUnsecured,
                    routedPose,
                    orientation);
            }

            if (!powerSupplyRetained)
            {
                return Invalid(
                    Atx24PowerCableRouteStatus.HostPowerSupplyUnretained,
                    routedPose,
                    orientation);
            }

            if (orientation != PowerCableKeyOrientation.Keyed)
            {
                return Invalid(
                    Atx24PowerCableRouteStatus.OrientationInvalid,
                    routedPose,
                    orientation);
            }

            Atx24PowerCableRouteStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                cable.transform,
                focusCollider,
                routeRoot,
                powerSupplyHostRoot,
                motherboardHostRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics);
            if (focusStatus != Atx24PowerCableRouteStatus.ValidRoute)
            {
                return Invalid(focusStatus, routedPose, orientation);
            }

            float radius = Mathf.Max(0.001f, routeRadius);
            Atx24PowerCableRouteStatus routeStatus = EvaluateRouteSegment(
                psuPrimaryEndpoint.position,
                firstWaypoint.position,
                radius,
                focusCollider,
                cable.transform,
                routeRoot,
                powerSupplyHostRoot,
                motherboardHostRoot,
                obstructionMask,
                physics);
            if (routeStatus == Atx24PowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    psuSenseEndpoint.position,
                    firstWaypoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    powerSupplyHostRoot,
                    motherboardHostRoot,
                    obstructionMask,
                    physics);
            }

            if (routeStatus == Atx24PowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    firstWaypoint.position,
                    secondWaypoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    powerSupplyHostRoot,
                    motherboardHostRoot,
                    obstructionMask,
                    physics);
            }

            if (routeStatus == Atx24PowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    secondWaypoint.position,
                    thirdWaypoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    powerSupplyHostRoot,
                    motherboardHostRoot,
                    obstructionMask,
                    physics);
            }

            if (routeStatus == Atx24PowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    thirdWaypoint.position,
                    motherboardEndpoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    powerSupplyHostRoot,
                    motherboardHostRoot,
                    obstructionMask,
                    physics);
            }

            if (routeStatus != Atx24PowerCableRouteStatus.ValidRoute)
            {
                return Invalid(routeStatus, routedPose, orientation);
            }

            return new Atx24PowerCableRouteEvaluation(
                Atx24PowerCableRouteStatus.ValidRoute,
                routedPose,
                true,
                orientation);
        }

        private static Atx24PowerCableRouteStatus EvaluateRouteSegment(
            Vector3 start,
            Vector3 end,
            float radius,
            Collider focusCollider,
            Transform cableRoot,
            Transform routeRoot,
            Transform powerSupplyHostRoot,
            Transform motherboardHostRoot,
            LayerMask obstructionMask,
            IAtx24PowerCableRoutePhysics physics)
        {
            int overlapCount = physics.OverlapCapsuleNonAlloc(
                start,
                end,
                radius,
                RouteOverlaps,
                obstructionMask);
            if (overlapCount >= HitCapacity)
            {
                return Atx24PowerCableRouteStatus.QuerySaturated;
            }

            for (int index = 0; index < overlapCount; index++)
            {
                if (!ShouldIgnoreRouteOverlap(
                        RouteOverlaps[index],
                        focusCollider,
                        cableRoot,
                        routeRoot,
                        powerSupplyHostRoot,
                        motherboardHostRoot))
                {
                    return Atx24PowerCableRouteStatus.RouteObstructed;
                }
            }

            return Atx24PowerCableRouteStatus.ValidRoute;
        }

        private static Atx24PowerCableRouteStatus EvaluateFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform cableRoot,
            Collider focusCollider,
            Transform routeRoot,
            Transform powerSupplyHostRoot,
            Transform motherboardHostRoot,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            IAtx24PowerCableRoutePhysics physics)
        {
            Vector3 toFocus = focusCollider.bounds.center - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon ||
                distance > Mathf.Max(0.1f, maximumRange))
            {
                return Atx24PowerCableRouteStatus.OutOfRange;
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return Atx24PowerCableRouteStatus.NotFocused;
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
                return Atx24PowerCableRouteStatus.LineOfSightBlocked;
            }

            if (hitCount >= HitCapacity)
            {
                return Atx24PowerCableRouteStatus.QuerySaturated;
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                Atx24PowerCablePhysicsHit hit = LineHits[index];
                Collider collider = hit.Collider;
                if (collider == null ||
                    IsChildOf(collider.transform, playerRoot) ||
                    IsChildOf(collider.transform, cableRoot))
                {
                    continue;
                }

                if (collider == focusCollider)
                {
                    targetDistance = Mathf.Min(targetDistance, hit.Distance);
                    continue;
                }

                if (collider.isTrigger ||
                    IsChildOf(collider.transform, routeRoot) ||
                    IsChildOf(collider.transform, powerSupplyHostRoot) ||
                    IsChildOf(collider.transform, motherboardHostRoot))
                {
                    continue;
                }

                obstructionDistance = Mathf.Min(obstructionDistance, hit.Distance);
            }

            if (float.IsPositiveInfinity(targetDistance))
            {
                return Atx24PowerCableRouteStatus.LineOfSightBlocked;
            }

            return obstructionDistance <= targetDistance + DistanceTieEpsilon
                ? Atx24PowerCableRouteStatus.LineOfSightBlocked
                : Atx24PowerCableRouteStatus.ValidRoute;
        }

        private static bool ShouldIgnoreRouteOverlap(
            Collider collider,
            Collider focusCollider,
            Transform cableRoot,
            Transform routeRoot,
            Transform powerSupplyHostRoot,
            Transform motherboardHostRoot)
        {
            return collider == null ||
                   collider == focusCollider ||
                   collider.isTrigger ||
                   IsChildOf(collider.transform, cableRoot) ||
                   IsChildOf(collider.transform, routeRoot) ||
                   IsChildOf(collider.transform, powerSupplyHostRoot) ||
                   IsChildOf(collider.transform, motherboardHostRoot);
        }

        private static bool IsChildOf(Transform candidate, Transform root)
        {
            return candidate != null && root != null && candidate.IsChildOf(root);
        }

        private static Atx24PowerCableRouteEvaluation Invalid(
            Atx24PowerCableRouteStatus status,
            Pose pose = default,
            PowerCableKeyOrientation orientation = default)
        {
            bool hasPose = status != Atx24PowerCableRouteStatus.Uninitialized &&
                           status != Atx24PowerCableRouteStatus.ModeDisabled &&
                           status != Atx24PowerCableRouteStatus.ContextMissing;
            return new Atx24PowerCableRouteEvaluation(
                status,
                pose,
                hasPose,
                orientation);
        }

        private sealed class UnityAtx24PowerCableRoutePhysics :
            IAtx24PowerCableRoutePhysics
        {
            internal static readonly UnityAtx24PowerCableRoutePhysics Instance = new();

            private readonly RaycastHit[] _raycastHits =
                new RaycastHit[HitCapacity];

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                Atx24PowerCablePhysicsHit[] results,
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
                int copyCount = Mathf.Min(count, results.Length);
                for (int index = 0; index < copyCount; index++)
                {
                    results[index] = new Atx24PowerCablePhysicsHit(
                        _raycastHits[index].collider,
                        _raycastHits[index].distance);
                }

                return count;
            }

            public int OverlapCapsuleNonAlloc(
                Vector3 point0,
                Vector3 point1,
                float radius,
                Collider[] results,
                int layerMask)
            {
                return Physics.OverlapCapsuleNonAlloc(
                    point0,
                    point1,
                    radius,
                    results,
                    layerMask,
                    QueryTriggerInteraction.Ignore);
            }
        }
    }
}
