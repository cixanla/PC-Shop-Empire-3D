using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class Eps12vPowerCableRouteProjection : MonoBehaviour
    {
        [SerializeField] private string routeId =
            GarageStockFlowSession.Eps12vPowerCableRouteIdValue;
        [SerializeField] private string psuEndpointId =
            GarageStockFlowSession.Eps12vPowerCablePsuEndpointIdValue;
        [SerializeField] private string motherboardEndpointId =
            GarageStockFlowSession.Eps12vPowerCableMotherboardEndpointIdValue;
        [SerializeField] private string[] waypointIds = new string[3];
        [SerializeField] private Transform psuEndpoint;
        [SerializeField] private Transform motherboardEndpoint;
        [SerializeField] private Transform[] waypoints = new Transform[3];
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Transform routeRoot;
        [SerializeField] private Transform powerSupplyHostRoot;
        [SerializeField] private Transform motherboardHostRoot;
        [SerializeField] private Collider[] allowedRouteColliders =
            Array.Empty<Collider>();
        [SerializeField] private LineRenderer previewLine;
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;
        [SerializeField, Min(0.001f)] private float routeRadius = 0.0065f;
        [SerializeField] private Color validColor =
            new Color(0.20f, 0.90f, 0.46f, 0.80f);
        [SerializeField] private Color invalidColor =
            new Color(0.95f, 0.20f, 0.20f, 0.80f);

        private bool _routeModeActive;
        private bool _authoritativeRouted;
        private Collider[] _installedAssemblyColliders = Array.Empty<Collider>();

        public string RouteIdValue => routeId;

        public string PsuEndpointIdValue => psuEndpointId;

        public string MotherboardEndpointIdValue => motherboardEndpointId;

        public string[] WaypointIdValues => waypointIds;

        public Transform PsuEndpoint => psuEndpoint;

        public Transform MotherboardEndpoint => motherboardEndpoint;

        public Transform[] Waypoints => waypoints;

        public Collider FocusCollider => focusCollider;

        public Transform RouteRoot => routeRoot;

        public Transform PowerSupplyHostRoot => powerSupplyHostRoot;

        public Transform MotherboardHostRoot => motherboardHostRoot;

        public Collider[] AllowedRouteColliders => allowedRouteColliders;

        public bool MatchesInstalledAssemblyColliders(
            Collider chassisCablePassThroughCollider,
            Collider graphicsCardCollider,
            Collider graphicsCardSlotConnectorCollider)
        {
            return chassisCablePassThroughCollider != null &&
                   graphicsCardCollider != null &&
                   graphicsCardSlotConnectorCollider != null &&
                   _installedAssemblyColliders.Length == 3 &&
                   _installedAssemblyColliders[0] ==
                       chassisCablePassThroughCollider &&
                   _installedAssemblyColliders[1] == graphicsCardCollider &&
                   _installedAssemblyColliders[2] ==
                       graphicsCardSlotConnectorCollider;
        }

        public LineRenderer PreviewLine => previewLine;

        public bool IsRouteModeActive => _routeModeActive;

        public bool IsAuthoritativeRouted => _authoritativeRouted;

        public Eps12vPowerCableRouteEvaluation LastEvaluation { get; private set; }

        public bool IsConfigured =>
            HasCanonicalIdentities() &&
            psuEndpoint != null &&
            motherboardEndpoint != null &&
            psuEndpoint != motherboardEndpoint &&
            waypoints != null &&
            waypoints.Length == 3 &&
            Array.TrueForAll(waypoints, waypoint => waypoint != null) &&
            AreDistinct(waypoints) &&
            focusCollider != null &&
            routeRoot != null &&
            powerSupplyHostRoot != null &&
            motherboardHostRoot != null &&
            psuEndpoint.IsChildOf(powerSupplyHostRoot) &&
            motherboardEndpoint.IsChildOf(motherboardHostRoot) &&
            focusCollider.transform.IsChildOf(motherboardHostRoot) &&
            Array.TrueForAll(waypoints, waypoint => waypoint.IsChildOf(routeRoot)) &&
            allowedRouteColliders != null &&
            allowedRouteColliders.Length > 0 &&
            Array.TrueForAll(
                allowedRouteColliders,
                collider => collider != null) &&
            AreDistinct(allowedRouteColliders) &&
            previewLine != null;

        public void Configure(
            string stableRouteId,
            string stablePsuEndpointId,
            string stableMotherboardEndpointId,
            string[] stableWaypointIds,
            Transform authoredPsuEndpoint,
            Transform authoredMotherboardEndpoint,
            Transform[] authoredWaypoints,
            Collider authoredFocusCollider,
            Transform authoredRouteRoot,
            Transform authoredPowerSupplyHostRoot,
            Transform authoredMotherboardHostRoot,
            Collider[] authoredAllowedRouteColliders,
            LineRenderer authoredPreviewLine,
            float range = 2f,
            float focusDot = 0.94f,
            float cableRadius = 0.0065f)
        {
            if (stableWaypointIds == null || stableWaypointIds.Length != 3)
            {
                throw new ArgumentException(
                    "Exactly three stable EPS12V waypoint identities are required.",
                    nameof(stableWaypointIds));
            }

            if (authoredWaypoints == null || authoredWaypoints.Length != 3)
            {
                throw new ArgumentException(
                    "Exactly three authored EPS12V waypoints are required.",
                    nameof(authoredWaypoints));
            }

            if (authoredAllowedRouteColliders == null ||
                authoredAllowedRouteColliders.Length == 0)
            {
                throw new ArgumentException(
                    "EPS12V requires an explicit non-empty collider allowlist.",
                    nameof(authoredAllowedRouteColliders));
            }

            routeId = StableId<AssemblyPowerCableRouteIdScope>.Parse(
                stableRouteId).Value;
            psuEndpointId = StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                stablePsuEndpointId).Value;
            motherboardEndpointId =
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    stableMotherboardEndpointId).Value;
            waypointIds = new string[3];
            waypoints = new Transform[3];
            for (int index = 0; index < 3; index++)
            {
                waypointIds[index] =
                    StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                        stableWaypointIds[index]).Value;
                waypoints[index] = authoredWaypoints[index] ??
                    throw new ArgumentException(
                        "EPS12V waypoints cannot contain null entries.",
                        nameof(authoredWaypoints));
            }

            allowedRouteColliders =
                (Collider[])authoredAllowedRouteColliders.Clone();
            _installedAssemblyColliders = Array.Empty<Collider>();
            psuEndpoint = authoredPsuEndpoint ??
                throw new ArgumentNullException(nameof(authoredPsuEndpoint));
            motherboardEndpoint = authoredMotherboardEndpoint ??
                throw new ArgumentNullException(nameof(authoredMotherboardEndpoint));
            focusCollider = authoredFocusCollider ??
                throw new ArgumentNullException(nameof(authoredFocusCollider));
            routeRoot = authoredRouteRoot ??
                throw new ArgumentNullException(nameof(authoredRouteRoot));
            powerSupplyHostRoot = authoredPowerSupplyHostRoot ??
                throw new ArgumentNullException(nameof(authoredPowerSupplyHostRoot));
            motherboardHostRoot = authoredMotherboardHostRoot ??
                throw new ArgumentNullException(nameof(authoredMotherboardHostRoot));
            previewLine = authoredPreviewLine ??
                throw new ArgumentNullException(nameof(authoredPreviewLine));
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            routeRadius = Mathf.Max(0.001f, cableRadius);

            if (!IsConfigured)
            {
                throw new ArgumentException(
                    "EPS12V route projection must own canonical endpoints, waypoint order and explicit collision allowances.");
            }

            ApplyAuthoritativeState(routed: false);
            SetRouteModeActive(active: false);
        }

        public void ConfigureInstalledAssemblyColliders(
            Collider chassisCablePassThroughCollider,
            Collider graphicsCardCollider,
            Collider graphicsCardSlotConnectorCollider)
        {
            Collider[] additions =
            {
                chassisCablePassThroughCollider,
                graphicsCardCollider,
                graphicsCardSlotConnectorCollider
            };
            if (Array.Exists(additions, collider => collider == null) ||
                !AreDistinct(additions))
            {
                throw new ArgumentException(
                    "EPS12V installed assembly colliders must be non-null and distinct.");
            }

            if (_installedAssemblyColliders.Length != 0)
            {
                if (MatchesInstalledAssemblyColliders(
                        chassisCablePassThroughCollider,
                        graphicsCardCollider,
                        graphicsCardSlotConnectorCollider))
                {
                    return;
                }

                throw new InvalidOperationException(
                    "EPS12V installed assembly colliders are already configured.");
            }

            for (int additionIndex = 0;
                 additionIndex < additions.Length;
                 additionIndex++)
            {
                if (Array.Exists(
                        allowedRouteColliders,
                        collider => collider == additions[additionIndex]))
                {
                    throw new ArgumentException(
                        "EPS12V installed assembly colliders must be distinct from the authored allowlist.");
                }
            }

            var expanded = new Collider[
                allowedRouteColliders.Length + additions.Length];
            Array.Copy(
                allowedRouteColliders,
                expanded,
                allowedRouteColliders.Length);
            Array.Copy(
                additions,
                0,
                expanded,
                allowedRouteColliders.Length,
                additions.Length);
            allowedRouteColliders = expanded;
            _installedAssemblyColliders = (Collider[])additions.Clone();
        }

        public void SetRouteModeActive(bool active)
        {
            _routeModeActive = active && !_authoritativeRouted && IsConfigured;
            if (focusCollider != null)
            {
                focusCollider.enabled =
                    _routeModeActive || _authoritativeRouted;
            }

            if (!_routeModeActive)
            {
                HidePreview();
                ResetFeedback();
            }
        }

        public Eps12vPowerCableRouteEvaluation EvaluateRoute(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable,
            bool motherboardSecured,
            bool powerSupplyRetained,
            bool processorRetained,
            PowerCableKeyOrientation orientation)
        {
            LastEvaluation = Eps12vPowerCableRouteSolver.Evaluate(
                _routeModeActive,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                psuEndpoint,
                motherboardEndpoint,
                waypoints != null && waypoints.Length > 0 ? waypoints[0] : null,
                waypoints != null && waypoints.Length > 1 ? waypoints[1] : null,
                waypoints != null && waypoints.Length > 2 ? waypoints[2] : null,
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
                orientation);
            ApplyPreview(LastEvaluation);
            return LastEvaluation;
        }

        public OperationResult<Pose> ResolveRoutedItemPose()
        {
            return motherboardEndpoint == null
                ? OperationResult<Pose>.Fail(
                    Failure.FromCode("assembly-eps12v-cable.context-missing"))
                : OperationResult<Pose>.Success(
                    new Pose(
                        motherboardEndpoint.position,
                        motherboardEndpoint.rotation));
        }

        public Eps12vPowerCableRouteStatus EvaluateRoutedFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            LayerMask obstructionMask,
            bool paused)
        {
            return Eps12vPowerCableRouteSolver.EvaluateRoutedFocus(
                _authoritativeRouted,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                allowedRouteColliders,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused);
        }

        public void ApplyAuthoritativeState(bool routed)
        {
            _authoritativeRouted = routed;
            SetRouteModeActive(_routeModeActive);
        }

        public void ResetFeedback()
        {
            LastEvaluation = new Eps12vPowerCableRouteEvaluation(
                Eps12vPowerCableRouteStatus.Uninitialized,
                default,
                false,
                default);
        }

        private void ApplyPreview(Eps12vPowerCableRouteEvaluation evaluation)
        {
            if (!_routeModeActive || !evaluation.HasPreview || !IsConfigured)
            {
                HidePreview();
                return;
            }

            Color color = evaluation.CanRoute ? validColor : invalidColor;
            previewLine.startColor = color;
            previewLine.endColor = color;
            previewLine.positionCount = 5;
            previewLine.SetPosition(0, psuEndpoint.position);
            previewLine.SetPosition(1, waypoints[0].position);
            previewLine.SetPosition(2, waypoints[1].position);
            previewLine.SetPosition(3, waypoints[2].position);
            previewLine.SetPosition(4, motherboardEndpoint.position);
            previewLine.enabled = true;
        }

        private void HidePreview()
        {
            if (previewLine != null)
            {
                previewLine.enabled = false;
            }
        }

        private bool HasCanonicalIdentities()
        {
            return routeId == GarageStockFlowSession.Eps12vPowerCableRouteIdValue &&
                   psuEndpointId ==
                       GarageStockFlowSession.Eps12vPowerCablePsuEndpointIdValue &&
                   motherboardEndpointId ==
                       GarageStockFlowSession
                           .Eps12vPowerCableMotherboardEndpointIdValue &&
                   waypointIds != null &&
                   waypointIds.Length == 3 &&
                   waypointIds[0] ==
                       GarageStockFlowSession.Eps12vPowerCableWaypoint1IdValue &&
                   waypointIds[1] ==
                       GarageStockFlowSession.Eps12vPowerCableWaypoint2IdValue &&
                   waypointIds[2] ==
                       GarageStockFlowSession.Eps12vPowerCableWaypoint3IdValue;
        }

        private static bool AreDistinct<T>(T[] values)
            where T : UnityEngine.Object
        {
            for (int left = 0; left < values.Length; left++)
            {
                if (values[left] == null)
                {
                    return false;
                }

                for (int right = left + 1; right < values.Length; right++)
                {
                    if (values[left] == values[right])
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
