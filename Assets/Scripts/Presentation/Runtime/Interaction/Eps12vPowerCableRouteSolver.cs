using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum Eps12vPowerCableRouteStatus
    {
        Uninitialized = 0,
        ModeDisabled = 1,
        ValidRoute = 2,
        ContextMissing = 3,
        Paused = 4,
        AuthorityBlocked = 5,
        HostMotherboardUnsecured = 6,
        HostPowerSupplyUnretained = 7,
        HostProcessorUnretained = 8,
        OutOfRange = 9,
        NotFocused = 10,
        LineOfSightBlocked = 11,
        OrientationInvalid = 12,
        RouteObstructed = 13,
        QuerySaturated = 14
    }

    public readonly struct Eps12vPowerCableRouteEvaluation
    {
        public Eps12vPowerCableRouteEvaluation(
            Eps12vPowerCableRouteStatus status,
            Pose pose,
            bool hasPose,
            PowerCableKeyOrientation orientation)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
            Orientation = orientation;
        }

        public Eps12vPowerCableRouteStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public PowerCableKeyOrientation Orientation { get; }

        public bool CanRoute =>
            Status == Eps12vPowerCableRouteStatus.ValidRoute &&
            HasPose &&
            Orientation == PowerCableKeyOrientation.Keyed;

        public bool HasPreview =>
            HasPose &&
            Status != Eps12vPowerCableRouteStatus.ModeDisabled &&
            Status != Eps12vPowerCableRouteStatus.ContextMissing;

        public string FailureCode => Status switch
        {
            Eps12vPowerCableRouteStatus.ModeDisabled =>
                "assembly-eps12v-cable.mode-disabled",
            Eps12vPowerCableRouteStatus.ContextMissing =>
                "assembly-eps12v-cable.context-missing",
            Eps12vPowerCableRouteStatus.Paused =>
                "assembly-eps12v-cable.paused",
            Eps12vPowerCableRouteStatus.AuthorityBlocked =>
                "assembly-eps12v-cable.authority-blocked",
            Eps12vPowerCableRouteStatus.HostMotherboardUnsecured =>
                "assembly-eps12v-cable.host-motherboard-unsecured",
            Eps12vPowerCableRouteStatus.HostPowerSupplyUnretained =>
                "assembly-eps12v-cable.host-power-supply-unretained",
            Eps12vPowerCableRouteStatus.HostProcessorUnretained =>
                "assembly-eps12v-cable.host-processor-unretained",
            Eps12vPowerCableRouteStatus.OutOfRange =>
                "assembly-eps12v-cable.out-of-range",
            Eps12vPowerCableRouteStatus.NotFocused =>
                "assembly-eps12v-cable.focus-missing",
            Eps12vPowerCableRouteStatus.LineOfSightBlocked =>
                "assembly-eps12v-cable.line-of-sight-blocked",
            Eps12vPowerCableRouteStatus.OrientationInvalid =>
                "assembly-eps12v-cable.orientation-mismatch",
            Eps12vPowerCableRouteStatus.RouteObstructed =>
                "assembly-eps12v-cable.route-obstructed",
            Eps12vPowerCableRouteStatus.QuerySaturated =>
                "assembly-eps12v-cable.query-saturated",
            _ => string.Empty
        };
    }

    internal readonly struct Eps12vPowerCablePhysicsHit
    {
        internal Eps12vPowerCablePhysicsHit(Collider collider, float distance)
        {
            Collider = collider;
            Distance = distance;
        }

        internal Collider Collider { get; }

        internal float Distance { get; }
    }

    internal interface IEps12vPowerCableRoutePhysics
    {
        int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            Eps12vPowerCablePhysicsHit[] results,
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
    /// Deterministic fail-closed gate for the authored EPS12V route. Only an
    /// explicit collider allowlist may intersect the cable path; whole PSU or
    /// motherboard hierarchies are never ignored.
    /// </summary>
    public static class Eps12vPowerCableRouteSolver
    {
        internal const int HitCapacity = 32;
        internal const float DistanceTieEpsilon = 0.0001f;

        private static readonly Eps12vPowerCablePhysicsHit[] LineHits =
            new Eps12vPowerCablePhysicsHit[HitCapacity];
        private static readonly Collider[] RouteOverlaps =
            new Collider[HitCapacity];

        public static Eps12vPowerCableRouteStatus EvaluateRoutedFocus(
            bool authoritativeRouted,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Collider[] allowedRouteColliders,
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
                allowedRouteColliders,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                UnityEps12vPowerCableRoutePhysics.Instance);
        }

        internal static Eps12vPowerCableRouteStatus EvaluateRoutedFocus(
            bool authoritativeRouted,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Collider[] allowedRouteColliders,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            bool paused,
            IEps12vPowerCableRoutePhysics physics)
        {
            if (!authoritativeRouted)
            {
                return Eps12vPowerCableRouteStatus.ModeDisabled;
            }

            if (!HasFocusContext(
                    interactionOrigin,
                    cable,
                    focusCollider,
                    routeRoot,
                    allowedRouteColliders,
                    physics))
            {
                return Eps12vPowerCableRouteStatus.ContextMissing;
            }

            if (paused)
            {
                return Eps12vPowerCableRouteStatus.Paused;
            }

            return EvaluateFocus(
                interactionOrigin,
                playerRoot,
                cable.transform,
                focusCollider,
                routeRoot,
                allowedRouteColliders,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics);
        }

        public static Eps12vPowerCableRouteEvaluation Evaluate(
            bool routeModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform psuEndpoint,
            Transform motherboardEndpoint,
            Transform firstWaypoint,
            Transform secondWaypoint,
            Transform thirdWaypoint,
            Collider[] allowedRouteColliders,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            float routeRadius,
            bool paused,
            bool authorityAvailable,
            bool motherboardSecured,
            bool powerSupplyRetained,
            bool processorRetained,
            PowerCableKeyOrientation orientation)
        {
            return Evaluate(
                routeModeEnabled,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                psuEndpoint,
                motherboardEndpoint,
                firstWaypoint,
                secondWaypoint,
                thirdWaypoint,
                allowedRouteColliders,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                routeRadius,
                paused,
                authorityAvailable,
                motherboardSecured,
                powerSupplyRetained,
                processorRetained,
                orientation,
                UnityEps12vPowerCableRoutePhysics.Instance);
        }

        internal static Eps12vPowerCableRouteEvaluation Evaluate(
            bool routeModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform psuEndpoint,
            Transform motherboardEndpoint,
            Transform firstWaypoint,
            Transform secondWaypoint,
            Transform thirdWaypoint,
            Collider[] allowedRouteColliders,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            float routeRadius,
            bool paused,
            bool authorityAvailable,
            bool motherboardSecured,
            bool powerSupplyRetained,
            bool processorRetained,
            PowerCableKeyOrientation orientation,
            IEps12vPowerCableRoutePhysics physics)
        {
            if (!routeModeEnabled)
            {
                return Invalid(Eps12vPowerCableRouteStatus.ModeDisabled);
            }

            if (!HasFocusContext(
                    interactionOrigin,
                    cable,
                    focusCollider,
                    routeRoot,
                    allowedRouteColliders,
                    physics) ||
                psuEndpoint == null ||
                motherboardEndpoint == null ||
                firstWaypoint == null ||
                secondWaypoint == null ||
                thirdWaypoint == null)
            {
                return Invalid(Eps12vPowerCableRouteStatus.ContextMissing);
            }

            Pose routedPose = new Pose(
                motherboardEndpoint.position,
                motherboardEndpoint.rotation);
            if (paused)
            {
                return Invalid(
                    Eps12vPowerCableRouteStatus.Paused,
                    routedPose,
                    orientation);
            }

            if (!authorityAvailable)
            {
                return Invalid(
                    Eps12vPowerCableRouteStatus.AuthorityBlocked,
                    routedPose,
                    orientation);
            }

            if (!motherboardSecured)
            {
                return Invalid(
                    Eps12vPowerCableRouteStatus.HostMotherboardUnsecured,
                    routedPose,
                    orientation);
            }

            if (!powerSupplyRetained)
            {
                return Invalid(
                    Eps12vPowerCableRouteStatus.HostPowerSupplyUnretained,
                    routedPose,
                    orientation);
            }

            if (!processorRetained)
            {
                return Invalid(
                    Eps12vPowerCableRouteStatus.HostProcessorUnretained,
                    routedPose,
                    orientation);
            }

            if (orientation != PowerCableKeyOrientation.Keyed)
            {
                return Invalid(
                    Eps12vPowerCableRouteStatus.OrientationInvalid,
                    routedPose,
                    orientation);
            }

            Eps12vPowerCableRouteStatus focusStatus = EvaluateFocus(
                interactionOrigin,
                playerRoot,
                cable.transform,
                focusCollider,
                routeRoot,
                allowedRouteColliders,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                physics);
            if (focusStatus != Eps12vPowerCableRouteStatus.ValidRoute)
            {
                return Invalid(focusStatus, routedPose, orientation);
            }

            float radius = Mathf.Max(0.001f, routeRadius);
            Eps12vPowerCableRouteStatus routeStatus = EvaluateRouteSegment(
                psuEndpoint.position,
                firstWaypoint.position,
                radius,
                focusCollider,
                cable.transform,
                routeRoot,
                allowedRouteColliders,
                obstructionMask,
                physics);
            if (routeStatus == Eps12vPowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    firstWaypoint.position,
                    secondWaypoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    allowedRouteColliders,
                    obstructionMask,
                    physics);
            }

            if (routeStatus == Eps12vPowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    secondWaypoint.position,
                    thirdWaypoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    allowedRouteColliders,
                    obstructionMask,
                    physics);
            }

            if (routeStatus == Eps12vPowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    thirdWaypoint.position,
                    motherboardEndpoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    allowedRouteColliders,
                    obstructionMask,
                    physics);
            }

            if (routeStatus != Eps12vPowerCableRouteStatus.ValidRoute)
            {
                return Invalid(routeStatus, routedPose, orientation);
            }

            return new Eps12vPowerCableRouteEvaluation(
                Eps12vPowerCableRouteStatus.ValidRoute,
                routedPose,
                true,
                orientation);
        }

        private static bool HasFocusContext(
            Transform interactionOrigin,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Collider[] allowedRouteColliders,
            IEps12vPowerCableRoutePhysics physics)
        {
            return interactionOrigin != null &&
                   cable != null &&
                   focusCollider != null &&
                   routeRoot != null &&
                   allowedRouteColliders != null &&
                   physics != null &&
                   focusCollider.enabled &&
                   focusCollider.gameObject.activeInHierarchy;
        }

        private static Eps12vPowerCableRouteStatus EvaluateRouteSegment(
            Vector3 start,
            Vector3 end,
            float radius,
            Collider focusCollider,
            Transform cableRoot,
            Transform routeRoot,
            Collider[] allowedRouteColliders,
            LayerMask obstructionMask,
            IEps12vPowerCableRoutePhysics physics)
        {
            int overlapCount = physics.OverlapCapsuleNonAlloc(
                start,
                end,
                radius,
                RouteOverlaps,
                obstructionMask);
            if (overlapCount >= HitCapacity)
            {
                return Eps12vPowerCableRouteStatus.QuerySaturated;
            }

            for (int index = 0; index < overlapCount; index++)
            {
                if (!ShouldIgnore(
                        RouteOverlaps[index],
                        focusCollider,
                        cableRoot,
                        routeRoot,
                        allowedRouteColliders))
                {
                    return Eps12vPowerCableRouteStatus.RouteObstructed;
                }
            }

            return Eps12vPowerCableRouteStatus.ValidRoute;
        }

        private static Eps12vPowerCableRouteStatus EvaluateFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform cableRoot,
            Collider focusCollider,
            Transform routeRoot,
            Collider[] allowedRouteColliders,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            IEps12vPowerCableRoutePhysics physics)
        {
            Vector3 toFocus = focusCollider.bounds.center - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon ||
                distance > Mathf.Max(0.1f, maximumRange))
            {
                return Eps12vPowerCableRouteStatus.OutOfRange;
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return Eps12vPowerCableRouteStatus.NotFocused;
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
                return Eps12vPowerCableRouteStatus.LineOfSightBlocked;
            }

            if (hitCount >= HitCapacity)
            {
                return Eps12vPowerCableRouteStatus.QuerySaturated;
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                Eps12vPowerCablePhysicsHit hit = LineHits[index];
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

                if (ShouldIgnore(
                        collider,
                        focusCollider,
                        cableRoot,
                        routeRoot,
                        allowedRouteColliders))
                {
                    continue;
                }

                obstructionDistance = Mathf.Min(obstructionDistance, hit.Distance);
            }

            if (float.IsPositiveInfinity(targetDistance))
            {
                return Eps12vPowerCableRouteStatus.LineOfSightBlocked;
            }

            return obstructionDistance <= targetDistance + DistanceTieEpsilon
                ? Eps12vPowerCableRouteStatus.LineOfSightBlocked
                : Eps12vPowerCableRouteStatus.ValidRoute;
        }

        private static bool ShouldIgnore(
            Collider collider,
            Collider focusCollider,
            Transform cableRoot,
            Transform routeRoot,
            Collider[] allowedRouteColliders)
        {
            if (collider == null ||
                collider == focusCollider ||
                collider.isTrigger ||
                IsChildOf(collider.transform, cableRoot) ||
                IsChildOf(collider.transform, routeRoot))
            {
                return true;
            }

            for (int index = 0; index < allowedRouteColliders.Length; index++)
            {
                if (collider == allowedRouteColliders[index])
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

        private static Eps12vPowerCableRouteEvaluation Invalid(
            Eps12vPowerCableRouteStatus status,
            Pose pose = default,
            PowerCableKeyOrientation orientation = default)
        {
            bool hasPose = status != Eps12vPowerCableRouteStatus.Uninitialized &&
                           status != Eps12vPowerCableRouteStatus.ModeDisabled &&
                           status != Eps12vPowerCableRouteStatus.ContextMissing;
            return new Eps12vPowerCableRouteEvaluation(
                status,
                pose,
                hasPose,
                orientation);
        }

        private sealed class UnityEps12vPowerCableRoutePhysics :
            IEps12vPowerCableRoutePhysics
        {
            internal static readonly UnityEps12vPowerCableRoutePhysics Instance =
                new();

            private readonly RaycastHit[] _raycastHits =
                new RaycastHit[HitCapacity];

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                Eps12vPowerCablePhysicsHit[] results,
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
                    results[index] = new Eps12vPowerCablePhysicsHit(
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
