using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum PcieGpuPowerCableRouteStatus
    {
        Uninitialized = 0,
        ModeDisabled = 1,
        ValidRoute = 2,
        ContextMissing = 3,
        Paused = 4,
        AuthorityBlocked = 5,
        HostMotherboardUnsecured = 6,
        HostPowerSupplyUnretained = 7,
        HostGraphicsCardUnretained = 8,
        OutOfRange = 9,
        NotFocused = 10,
        LineOfSightBlocked = 11,
        OrientationInvalid = 12,
        RouteObstructed = 13,
        QuerySaturated = 14
    }

    public readonly struct PcieGpuPowerCableRouteEvaluation
    {
        public PcieGpuPowerCableRouteEvaluation(
            PcieGpuPowerCableRouteStatus status,
            Pose pose,
            bool hasPose,
            PowerCableKeyOrientation orientation)
        {
            Status = status;
            Pose = pose;
            HasPose = hasPose;
            Orientation = orientation;
        }

        public PcieGpuPowerCableRouteStatus Status { get; }

        public Pose Pose { get; }

        public bool HasPose { get; }

        public PowerCableKeyOrientation Orientation { get; }

        public bool CanRoute =>
            Status == PcieGpuPowerCableRouteStatus.ValidRoute &&
            HasPose &&
            Orientation == PowerCableKeyOrientation.Keyed;

        public bool HasPreview =>
            HasPose &&
            Status != PcieGpuPowerCableRouteStatus.ModeDisabled &&
            Status != PcieGpuPowerCableRouteStatus.ContextMissing;

        public string FailureCode => Status switch
        {
            PcieGpuPowerCableRouteStatus.ModeDisabled =>
                "assembly-pcie-gpu-cable.mode-disabled",
            PcieGpuPowerCableRouteStatus.ContextMissing =>
                "assembly-pcie-gpu-cable.context-missing",
            PcieGpuPowerCableRouteStatus.Paused =>
                "assembly-pcie-gpu-cable.paused",
            PcieGpuPowerCableRouteStatus.AuthorityBlocked =>
                "assembly-pcie-gpu-cable.authority-blocked",
            PcieGpuPowerCableRouteStatus.HostMotherboardUnsecured =>
                "assembly-pcie-gpu-cable.host-motherboard-unsecured",
            PcieGpuPowerCableRouteStatus.HostPowerSupplyUnretained =>
                "assembly-pcie-gpu-cable.host-power-supply-unretained",
            PcieGpuPowerCableRouteStatus.HostGraphicsCardUnretained =>
                "assembly-pcie-gpu-cable.host-graphics-card-unretained",
            PcieGpuPowerCableRouteStatus.OutOfRange =>
                "assembly-pcie-gpu-cable.out-of-range",
            PcieGpuPowerCableRouteStatus.NotFocused =>
                "assembly-pcie-gpu-cable.focus-missing",
            PcieGpuPowerCableRouteStatus.LineOfSightBlocked =>
                "assembly-pcie-gpu-cable.line-of-sight-blocked",
            PcieGpuPowerCableRouteStatus.OrientationInvalid =>
                "assembly-pcie-gpu-cable.orientation-mismatch",
            PcieGpuPowerCableRouteStatus.RouteObstructed =>
                "assembly-pcie-gpu-cable.route-obstructed",
            PcieGpuPowerCableRouteStatus.QuerySaturated =>
                "assembly-pcie-gpu-cable.query-saturated",
            _ => string.Empty
        };
    }

    internal readonly struct PcieGpuPowerCablePhysicsHit
    {
        internal PcieGpuPowerCablePhysicsHit(Collider collider, float distance)
        {
            Collider = collider;
            Distance = distance;
        }

        internal Collider Collider { get; }

        internal float Distance { get; }
    }

    internal interface IPcieGpuPowerCableRoutePhysics
    {
        int RaycastNonAlloc(
            Vector3 origin,
            Vector3 direction,
            PcieGpuPowerCablePhysicsHit[] results,
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
    /// Deterministic fail-closed gate for the authored PCIe GPU route. Only an
    /// explicit collider allowlist may intersect the cable path; whole PSU or
    /// motherboard hierarchies are never ignored.
    /// </summary>
    public static class PcieGpuPowerCableRouteSolver
    {
        internal const int HitCapacity = 32;
        internal const float DistanceTieEpsilon = 0.0001f;

        private static readonly PcieGpuPowerCablePhysicsHit[] LineHits =
            new PcieGpuPowerCablePhysicsHit[HitCapacity];
        private static readonly Collider[] RouteOverlaps =
            new Collider[HitCapacity];

        public static PcieGpuPowerCableRouteStatus EvaluateRoutedFocus(
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
                UnityPcieGpuPowerCableRoutePhysics.Instance);
        }

        internal static PcieGpuPowerCableRouteStatus EvaluateRoutedFocus(
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
            IPcieGpuPowerCableRoutePhysics physics)
        {
            if (!authoritativeRouted)
            {
                return PcieGpuPowerCableRouteStatus.ModeDisabled;
            }

            if (!HasFocusContext(
                    interactionOrigin,
                    cable,
                    focusCollider,
                    routeRoot,
                    allowedRouteColliders,
                    physics))
            {
                return PcieGpuPowerCableRouteStatus.ContextMissing;
            }

            if (paused)
            {
                return PcieGpuPowerCableRouteStatus.Paused;
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

        public static PcieGpuPowerCableRouteEvaluation Evaluate(
            bool routeModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform psuEndpoint,
            Transform graphicsCardEndpoint,
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
            bool graphicsCardRetained,
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
                graphicsCardEndpoint,
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
                graphicsCardRetained,
                orientation,
                UnityPcieGpuPowerCableRoutePhysics.Instance);
        }

        internal static PcieGpuPowerCableRouteEvaluation Evaluate(
            bool routeModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            Collider focusCollider,
            Transform routeRoot,
            Transform psuEndpoint,
            Transform graphicsCardEndpoint,
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
            bool graphicsCardRetained,
            PowerCableKeyOrientation orientation,
            IPcieGpuPowerCableRoutePhysics physics)
        {
            if (!routeModeEnabled)
            {
                return Invalid(PcieGpuPowerCableRouteStatus.ModeDisabled);
            }

            if (!HasFocusContext(
                    interactionOrigin,
                    cable,
                    focusCollider,
                    routeRoot,
                    allowedRouteColliders,
                    physics) ||
                psuEndpoint == null ||
                graphicsCardEndpoint == null ||
                firstWaypoint == null ||
                secondWaypoint == null ||
                thirdWaypoint == null)
            {
                return Invalid(PcieGpuPowerCableRouteStatus.ContextMissing);
            }

            Pose routedPose = new Pose(
                graphicsCardEndpoint.position,
                graphicsCardEndpoint.rotation);
            if (paused)
            {
                return Invalid(
                    PcieGpuPowerCableRouteStatus.Paused,
                    routedPose,
                    orientation);
            }

            if (!authorityAvailable)
            {
                return Invalid(
                    PcieGpuPowerCableRouteStatus.AuthorityBlocked,
                    routedPose,
                    orientation);
            }

            if (!motherboardSecured)
            {
                return Invalid(
                    PcieGpuPowerCableRouteStatus.HostMotherboardUnsecured,
                    routedPose,
                    orientation);
            }

            if (!powerSupplyRetained)
            {
                return Invalid(
                    PcieGpuPowerCableRouteStatus.HostPowerSupplyUnretained,
                    routedPose,
                    orientation);
            }

            if (!graphicsCardRetained)
            {
                return Invalid(
                    PcieGpuPowerCableRouteStatus.HostGraphicsCardUnretained,
                    routedPose,
                    orientation);
            }

            if (orientation != PowerCableKeyOrientation.Keyed)
            {
                return Invalid(
                    PcieGpuPowerCableRouteStatus.OrientationInvalid,
                    routedPose,
                    orientation);
            }

            PcieGpuPowerCableRouteStatus focusStatus = EvaluateFocus(
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
            if (focusStatus != PcieGpuPowerCableRouteStatus.ValidRoute)
            {
                return Invalid(focusStatus, routedPose, orientation);
            }

            float radius = Mathf.Max(0.001f, routeRadius);
            PcieGpuPowerCableRouteStatus routeStatus = EvaluateRouteSegment(
                psuEndpoint.position,
                firstWaypoint.position,
                radius,
                focusCollider,
                cable.transform,
                routeRoot,
                allowedRouteColliders,
                obstructionMask,
                physics);
            if (routeStatus == PcieGpuPowerCableRouteStatus.ValidRoute)
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

            if (routeStatus == PcieGpuPowerCableRouteStatus.ValidRoute)
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

            if (routeStatus == PcieGpuPowerCableRouteStatus.ValidRoute)
            {
                routeStatus = EvaluateRouteSegment(
                    thirdWaypoint.position,
                    graphicsCardEndpoint.position,
                    radius,
                    focusCollider,
                    cable.transform,
                    routeRoot,
                    allowedRouteColliders,
                    obstructionMask,
                    physics);
            }

            if (routeStatus != PcieGpuPowerCableRouteStatus.ValidRoute)
            {
                return Invalid(routeStatus, routedPose, orientation);
            }

            return new PcieGpuPowerCableRouteEvaluation(
                PcieGpuPowerCableRouteStatus.ValidRoute,
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
            IPcieGpuPowerCableRoutePhysics physics)
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

        private static PcieGpuPowerCableRouteStatus EvaluateRouteSegment(
            Vector3 start,
            Vector3 end,
            float radius,
            Collider focusCollider,
            Transform cableRoot,
            Transform routeRoot,
            Collider[] allowedRouteColliders,
            LayerMask obstructionMask,
            IPcieGpuPowerCableRoutePhysics physics)
        {
            int overlapCount = physics.OverlapCapsuleNonAlloc(
                start,
                end,
                radius,
                RouteOverlaps,
                obstructionMask);
            if (overlapCount >= HitCapacity)
            {
                return PcieGpuPowerCableRouteStatus.QuerySaturated;
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
                    return PcieGpuPowerCableRouteStatus.RouteObstructed;
                }
            }

            return PcieGpuPowerCableRouteStatus.ValidRoute;
        }

        private static PcieGpuPowerCableRouteStatus EvaluateFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            Transform cableRoot,
            Collider focusCollider,
            Transform routeRoot,
            Collider[] allowedRouteColliders,
            LayerMask obstructionMask,
            float maximumRange,
            float minimumFocusDot,
            IPcieGpuPowerCableRoutePhysics physics)
        {
            Vector3 toFocus = focusCollider.bounds.center - interactionOrigin.position;
            float distance = toFocus.magnitude;
            if (distance <= Mathf.Epsilon ||
                distance > Mathf.Max(0.1f, maximumRange))
            {
                return PcieGpuPowerCableRouteStatus.OutOfRange;
            }

            Vector3 direction = toFocus / distance;
            if (Vector3.Dot(interactionOrigin.forward, direction) <
                Mathf.Clamp01(minimumFocusDot))
            {
                return PcieGpuPowerCableRouteStatus.NotFocused;
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
                return PcieGpuPowerCableRouteStatus.LineOfSightBlocked;
            }

            if (hitCount >= HitCapacity)
            {
                return PcieGpuPowerCableRouteStatus.QuerySaturated;
            }

            float targetDistance = float.PositiveInfinity;
            float obstructionDistance = float.PositiveInfinity;
            for (int index = 0; index < hitCount; index++)
            {
                PcieGpuPowerCablePhysicsHit hit = LineHits[index];
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
                return PcieGpuPowerCableRouteStatus.LineOfSightBlocked;
            }

            return obstructionDistance <= targetDistance + DistanceTieEpsilon
                ? PcieGpuPowerCableRouteStatus.LineOfSightBlocked
                : PcieGpuPowerCableRouteStatus.ValidRoute;
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

        private static PcieGpuPowerCableRouteEvaluation Invalid(
            PcieGpuPowerCableRouteStatus status,
            Pose pose = default,
            PowerCableKeyOrientation orientation = default)
        {
            bool hasPose = status != PcieGpuPowerCableRouteStatus.Uninitialized &&
                           status != PcieGpuPowerCableRouteStatus.ModeDisabled &&
                           status != PcieGpuPowerCableRouteStatus.ContextMissing;
            return new PcieGpuPowerCableRouteEvaluation(
                status,
                pose,
                hasPose,
                orientation);
        }

        private sealed class UnityPcieGpuPowerCableRoutePhysics :
            IPcieGpuPowerCableRoutePhysics
        {
            internal static readonly UnityPcieGpuPowerCableRoutePhysics Instance =
                new();

            private readonly RaycastHit[] _raycastHits =
                new RaycastHit[HitCapacity];

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                PcieGpuPowerCablePhysicsHit[] results,
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
                    results[index] = new PcieGpuPowerCablePhysicsHit(
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
