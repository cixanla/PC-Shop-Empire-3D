using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PcieGpuPowerCableRouteProjection : MonoBehaviour
    {
        [SerializeField] private string routeId =
            GarageStockFlowSession.PcieGpuPowerCableRouteIdValue;
        [SerializeField] private string psuEndpointId =
            GarageStockFlowSession.PcieGpuPowerCablePsuEndpointIdValue;
        [SerializeField] private string graphicsCardEndpointId =
            GarageStockFlowSession.PcieGpuPowerCableGraphicsCardEndpointIdValue;
        [SerializeField] private string[] waypointIds = new string[3];
        [SerializeField] private Transform psuEndpoint;
        [SerializeField] private Transform graphicsCardEndpoint;
        [SerializeField] private Transform[] waypoints = new Transform[3];
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Transform routeRoot;
        [SerializeField] private Transform powerSupplyHostRoot;
        [SerializeField] private Transform graphicsCardHostRoot;
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

        public string RouteIdValue => routeId;

        public string PsuEndpointIdValue => psuEndpointId;

        public string GraphicsCardEndpointIdValue => graphicsCardEndpointId;

        public string[] WaypointIdValues => waypointIds;

        public Transform PsuEndpoint => psuEndpoint;

        public Transform GraphicsCardEndpoint => graphicsCardEndpoint;

        public Transform[] Waypoints => waypoints;

        public Collider FocusCollider => focusCollider;

        public Transform RouteRoot => routeRoot;

        public Transform PowerSupplyHostRoot => powerSupplyHostRoot;

        public Transform GraphicsCardHostRoot => graphicsCardHostRoot;

        public Collider[] AllowedRouteColliders => allowedRouteColliders;

        public LineRenderer PreviewLine => previewLine;

        public bool IsRouteModeActive => _routeModeActive;

        public bool IsAuthoritativeRouted => _authoritativeRouted;

        public PcieGpuPowerCableRouteEvaluation LastEvaluation { get; private set; }

        public bool IsConfigured =>
            HasCanonicalIdentities() &&
            psuEndpoint != null &&
            graphicsCardEndpoint != null &&
            psuEndpoint != graphicsCardEndpoint &&
            waypoints != null &&
            waypoints.Length == 3 &&
            Array.TrueForAll(waypoints, waypoint => waypoint != null) &&
            AreDistinct(waypoints) &&
            focusCollider != null &&
            routeRoot != null &&
            powerSupplyHostRoot != null &&
            graphicsCardHostRoot != null &&
            psuEndpoint.IsChildOf(powerSupplyHostRoot) &&
            graphicsCardEndpoint.IsChildOf(graphicsCardHostRoot) &&
            focusCollider.transform.IsChildOf(graphicsCardHostRoot) &&
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
            string stableGraphicsCardEndpointId,
            string[] stableWaypointIds,
            Transform authoredPsuEndpoint,
            Transform authoredGraphicsCardEndpoint,
            Transform[] authoredWaypoints,
            Collider authoredFocusCollider,
            Transform authoredRouteRoot,
            Transform authoredPowerSupplyHostRoot,
            Transform authoredGraphicsCardHostRoot,
            Collider[] authoredAllowedRouteColliders,
            LineRenderer authoredPreviewLine,
            float range = 2f,
            float focusDot = 0.94f,
            float cableRadius = 0.0065f)
        {
            if (stableWaypointIds == null || stableWaypointIds.Length != 3)
            {
                throw new ArgumentException(
                    "Exactly three stable PCIe GPU waypoint identities are required.",
                    nameof(stableWaypointIds));
            }

            if (authoredWaypoints == null || authoredWaypoints.Length != 3)
            {
                throw new ArgumentException(
                    "Exactly three authored PCIe GPU waypoints are required.",
                    nameof(authoredWaypoints));
            }

            if (authoredAllowedRouteColliders == null ||
                authoredAllowedRouteColliders.Length == 0)
            {
                throw new ArgumentException(
                    "PCIe GPU requires an explicit non-empty collider allowlist.",
                    nameof(authoredAllowedRouteColliders));
            }

            routeId = StableId<AssemblyPowerCableRouteIdScope>.Parse(
                stableRouteId).Value;
            psuEndpointId = StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                stablePsuEndpointId).Value;
            graphicsCardEndpointId =
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    stableGraphicsCardEndpointId).Value;
            waypointIds = new string[3];
            waypoints = new Transform[3];
            for (int index = 0; index < 3; index++)
            {
                waypointIds[index] =
                    StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                        stableWaypointIds[index]).Value;
                waypoints[index] = authoredWaypoints[index] ??
                    throw new ArgumentException(
                        "PCIe GPU waypoints cannot contain null entries.",
                        nameof(authoredWaypoints));
            }

            allowedRouteColliders =
                (Collider[])authoredAllowedRouteColliders.Clone();
            psuEndpoint = authoredPsuEndpoint ??
                throw new ArgumentNullException(nameof(authoredPsuEndpoint));
            graphicsCardEndpoint = authoredGraphicsCardEndpoint ??
                throw new ArgumentNullException(nameof(authoredGraphicsCardEndpoint));
            focusCollider = authoredFocusCollider ??
                throw new ArgumentNullException(nameof(authoredFocusCollider));
            routeRoot = authoredRouteRoot ??
                throw new ArgumentNullException(nameof(authoredRouteRoot));
            powerSupplyHostRoot = authoredPowerSupplyHostRoot ??
                throw new ArgumentNullException(nameof(authoredPowerSupplyHostRoot));
            graphicsCardHostRoot = authoredGraphicsCardHostRoot ??
                throw new ArgumentNullException(nameof(authoredGraphicsCardHostRoot));
            previewLine = authoredPreviewLine ??
                throw new ArgumentNullException(nameof(authoredPreviewLine));
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            routeRadius = Mathf.Max(0.001f, cableRadius);

            if (!IsConfigured)
            {
                throw new ArgumentException(
                    "PCIe GPU route projection must own canonical endpoints, waypoint order and explicit collision allowances.");
            }

            ApplyAuthoritativeState(routed: false);
            SetRouteModeActive(active: false);
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

        public PcieGpuPowerCableRouteEvaluation EvaluateRoute(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable,
            bool motherboardSecured,
            bool powerSupplyRetained,
            bool graphicsCardRetained,
            PowerCableKeyOrientation orientation)
        {
            LastEvaluation = PcieGpuPowerCableRouteSolver.Evaluate(
                _routeModeActive,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                psuEndpoint,
                graphicsCardEndpoint,
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
                graphicsCardRetained,
                orientation);
            ApplyPreview(LastEvaluation);
            return LastEvaluation;
        }

        public OperationResult<Pose> ResolveRoutedItemPose()
        {
            return graphicsCardEndpoint == null
                ? OperationResult<Pose>.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.context-missing"))
                : OperationResult<Pose>.Success(
                    new Pose(
                        graphicsCardEndpoint.position,
                        graphicsCardEndpoint.rotation));
        }

        public PcieGpuPowerCableRouteStatus EvaluateRoutedFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            LayerMask obstructionMask,
            bool paused)
        {
            return PcieGpuPowerCableRouteSolver.EvaluateRoutedFocus(
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
            LastEvaluation = new PcieGpuPowerCableRouteEvaluation(
                PcieGpuPowerCableRouteStatus.Uninitialized,
                default,
                false,
                default);
        }

        private void ApplyPreview(PcieGpuPowerCableRouteEvaluation evaluation)
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
            previewLine.SetPosition(4, graphicsCardEndpoint.position);
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
            return routeId == GarageStockFlowSession.PcieGpuPowerCableRouteIdValue &&
                   psuEndpointId ==
                       GarageStockFlowSession.PcieGpuPowerCablePsuEndpointIdValue &&
                   graphicsCardEndpointId ==
                       GarageStockFlowSession
                           .PcieGpuPowerCableGraphicsCardEndpointIdValue &&
                   waypointIds != null &&
                   waypointIds.Length == 3 &&
                   waypointIds[0] ==
                       GarageStockFlowSession.PcieGpuPowerCableWaypoint1IdValue &&
                   waypointIds[1] ==
                       GarageStockFlowSession.PcieGpuPowerCableWaypoint2IdValue &&
                   waypointIds[2] ==
                       GarageStockFlowSession.PcieGpuPowerCableWaypoint3IdValue;
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
