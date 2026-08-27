using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class Atx24PowerCableRouteProjection : MonoBehaviour
    {
        [SerializeField] private string routeId =
            GarageStockFlowSession.Atx24PowerCableRouteIdValue;
        [SerializeField] private string psuPrimaryEndpointId =
            GarageStockFlowSession.Atx24PowerCablePsuPrimaryEndpointIdValue;
        [SerializeField] private string psuSenseEndpointId =
            GarageStockFlowSession.Atx24PowerCablePsuSenseEndpointIdValue;
        [SerializeField] private string motherboardEndpointId =
            GarageStockFlowSession.Atx24PowerCableMotherboardEndpointIdValue;
        [SerializeField] private string[] waypointIds = new string[3];
        [SerializeField] private Transform psuPrimaryEndpoint;
        [SerializeField] private Transform psuSenseEndpoint;
        [SerializeField] private Transform motherboardEndpoint;
        [SerializeField] private Transform[] waypoints = new Transform[3];
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Transform routeRoot;
        [SerializeField] private Transform powerSupplyHostRoot;
        [SerializeField] private Transform motherboardHostRoot;
        private Transform[] _installedAssemblyHostRoots = Array.Empty<Transform>();
        [SerializeField] private LineRenderer[] previewLines = new LineRenderer[3];
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;
        [SerializeField, Min(0.001f)] private float routeRadius = 0.0075f;
        [SerializeField] private Color validColor = new Color(0.20f, 0.90f, 0.46f, 0.80f);
        [SerializeField] private Color invalidColor = new Color(0.95f, 0.20f, 0.20f, 0.80f);

        private bool _routeModeActive;
        private bool _authoritativeRouted;

        public string RouteIdValue => routeId;

        public string PsuPrimaryEndpointIdValue => psuPrimaryEndpointId;

        public string PsuSenseEndpointIdValue => psuSenseEndpointId;

        public string MotherboardEndpointIdValue => motherboardEndpointId;

        public string[] WaypointIdValues => waypointIds;

        public Transform PsuPrimaryEndpoint => psuPrimaryEndpoint;

        public Transform PsuSenseEndpoint => psuSenseEndpoint;

        public Transform MotherboardEndpoint => motherboardEndpoint;

        public Transform[] Waypoints => waypoints;

        public Collider FocusCollider => focusCollider;

        public Transform RouteRoot => routeRoot;

        public Transform PowerSupplyHostRoot => powerSupplyHostRoot;

        public Transform MotherboardHostRoot => motherboardHostRoot;

        public bool MatchesInstalledAssemblyHostRoots(
            Transform processorCoolerRoot,
            Transform graphicsCardRoot,
            Transform chassisCablePassThroughRoot)
        {
            return processorCoolerRoot != null &&
                   graphicsCardRoot != null &&
                   chassisCablePassThroughRoot != null &&
                   _installedAssemblyHostRoots.Length == 3 &&
                   _installedAssemblyHostRoots[0] == processorCoolerRoot &&
                   _installedAssemblyHostRoots[1] == graphicsCardRoot &&
                   _installedAssemblyHostRoots[2] == chassisCablePassThroughRoot;
        }

        public LineRenderer[] PreviewLines => previewLines;

        public bool IsRouteModeActive => _routeModeActive;

        public bool IsAuthoritativeRouted => _authoritativeRouted;

        public Atx24PowerCableRouteEvaluation LastEvaluation { get; private set; }

        public bool IsConfigured =>
            HasCanonicalIdentities() &&
            psuPrimaryEndpoint != null &&
            psuSenseEndpoint != null &&
            motherboardEndpoint != null &&
            waypoints != null &&
            waypoints.Length == 3 &&
            Array.TrueForAll(waypoints, waypoint => waypoint != null) &&
            AreDistinct(waypoints) &&
            focusCollider != null &&
            routeRoot != null &&
            powerSupplyHostRoot != null &&
            motherboardHostRoot != null &&
            psuPrimaryEndpoint.IsChildOf(powerSupplyHostRoot) &&
            psuSenseEndpoint.IsChildOf(powerSupplyHostRoot) &&
            motherboardEndpoint.IsChildOf(motherboardHostRoot) &&
            focusCollider.transform.IsChildOf(motherboardHostRoot) &&
            Array.TrueForAll(waypoints, waypoint => waypoint.IsChildOf(routeRoot)) &&
            previewLines != null &&
            previewLines.Length == 3 &&
            Array.TrueForAll(previewLines, line => line != null) &&
            AreDistinct(previewLines);

        public void ConfigureInstalledAssemblyHostRoots(
            Transform processorCoolerRoot,
            Transform graphicsCardRoot,
            Transform chassisCablePassThroughRoot)
        {
            if (processorCoolerRoot == null)
            {
                throw new ArgumentNullException(nameof(processorCoolerRoot));
            }

            if (graphicsCardRoot == null)
            {
                throw new ArgumentNullException(nameof(graphicsCardRoot));
            }

            if (chassisCablePassThroughRoot == null)
            {
                throw new ArgumentNullException(
                    nameof(chassisCablePassThroughRoot));
            }

            if (processorCoolerRoot == graphicsCardRoot ||
                processorCoolerRoot == chassisCablePassThroughRoot ||
                graphicsCardRoot == chassisCablePassThroughRoot ||
                processorCoolerRoot.IsChildOf(graphicsCardRoot) ||
                graphicsCardRoot.IsChildOf(processorCoolerRoot) ||
                processorCoolerRoot.IsChildOf(chassisCablePassThroughRoot) ||
                graphicsCardRoot.IsChildOf(chassisCablePassThroughRoot) ||
                chassisCablePassThroughRoot.IsChildOf(processorCoolerRoot) ||
                chassisCablePassThroughRoot.IsChildOf(graphicsCardRoot))
            {
                throw new ArgumentException(
                    "ATX24 installed assembly host roots must be distinct.");
            }

            _installedAssemblyHostRoots =
                new[]
                {
                    processorCoolerRoot,
                    graphicsCardRoot,
                    chassisCablePassThroughRoot
                };
        }

        public void Configure(
            string stableRouteId,
            string stablePsuPrimaryEndpointId,
            string stablePsuSenseEndpointId,
            string stableMotherboardEndpointId,
            string[] stableWaypointIds,
            Transform authoredPsuPrimaryEndpoint,
            Transform authoredPsuSenseEndpoint,
            Transform authoredMotherboardEndpoint,
            Transform[] authoredWaypoints,
            Collider authoredFocusCollider,
            Transform authoredRouteRoot,
            Transform authoredPowerSupplyHostRoot,
            Transform authoredMotherboardHostRoot,
            LineRenderer[] authoredPreviewLines,
            float range = 2f,
            float focusDot = 0.94f,
            float cableRadius = 0.0075f)
        {
            if (stableWaypointIds == null || stableWaypointIds.Length != 3)
            {
                throw new ArgumentException(
                    "Exactly three stable ATX24 waypoint identities are required.",
                    nameof(stableWaypointIds));
            }

            if (authoredWaypoints == null || authoredWaypoints.Length != 3)
            {
                throw new ArgumentException(
                    "Exactly three authored ATX24 waypoints are required.",
                    nameof(authoredWaypoints));
            }

            if (authoredPreviewLines == null || authoredPreviewLines.Length != 3)
            {
                throw new ArgumentException(
                    "ATX24 preview requires two PSU branches and one trunk.",
                    nameof(authoredPreviewLines));
            }

            routeId = StableId<AssemblyPowerCableRouteIdScope>.Parse(
                stableRouteId).Value;
            psuPrimaryEndpointId = StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                stablePsuPrimaryEndpointId).Value;
            psuSenseEndpointId = StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                stablePsuSenseEndpointId).Value;
            motherboardEndpointId = StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                stableMotherboardEndpointId).Value;
            waypointIds = new string[3];
            waypoints = new Transform[3];
            previewLines = new LineRenderer[3];
            for (int index = 0; index < 3; index++)
            {
                waypointIds[index] = StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                    stableWaypointIds[index]).Value;
                waypoints[index] = authoredWaypoints[index] != null
                    ? authoredWaypoints[index]
                    : throw new ArgumentException(
                        "ATX24 waypoints cannot contain null entries.",
                        nameof(authoredWaypoints));
                previewLines[index] = authoredPreviewLines[index] != null
                    ? authoredPreviewLines[index]
                    : throw new ArgumentException(
                        "ATX24 preview lines cannot contain null entries.",
                        nameof(authoredPreviewLines));
            }

            psuPrimaryEndpoint = authoredPsuPrimaryEndpoint ??
                throw new ArgumentNullException(nameof(authoredPsuPrimaryEndpoint));
            psuSenseEndpoint = authoredPsuSenseEndpoint ??
                throw new ArgumentNullException(nameof(authoredPsuSenseEndpoint));
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
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            routeRadius = Mathf.Max(0.001f, cableRadius);

            if (!IsConfigured)
            {
                throw new ArgumentException(
                    "ATX24 route projection must own the canonical endpoints and waypoint order.");
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

        public Atx24PowerCableRouteEvaluation EvaluateRoute(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable,
            bool motherboardSecured,
            bool powerSupplyRetained,
            PowerCableKeyOrientation orientation)
        {
            LastEvaluation = Atx24PowerCableRouteSolver.Evaluate(
                _routeModeActive,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                psuPrimaryEndpoint,
                psuSenseEndpoint,
                motherboardEndpoint,
                waypoints != null && waypoints.Length > 0 ? waypoints[0] : null,
                waypoints != null && waypoints.Length > 1 ? waypoints[1] : null,
                waypoints != null && waypoints.Length > 2 ? waypoints[2] : null,
                powerSupplyHostRoot,
                motherboardHostRoot,
                _installedAssemblyHostRoots,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                routeRadius,
                paused,
                authorityAvailable,
                motherboardSecured,
                powerSupplyRetained,
                orientation);
            ApplyPreview(LastEvaluation);
            return LastEvaluation;
        }

        public OperationResult<Pose> ResolveRoutedItemPose()
        {
            return motherboardEndpoint == null
                ? OperationResult<Pose>.Fail(
                    Failure.FromCode("assembly-power-cable.context-missing"))
                : OperationResult<Pose>.Success(
                    new Pose(
                        motherboardEndpoint.position,
                        motherboardEndpoint.rotation));
        }

        public Atx24PowerCableRouteStatus EvaluateRoutedFocus(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection cable,
            LayerMask obstructionMask,
            bool paused)
        {
            return Atx24PowerCableRouteSolver.EvaluateRoutedFocus(
                _authoritativeRouted,
                interactionOrigin,
                playerRoot,
                cable,
                focusCollider,
                routeRoot,
                powerSupplyHostRoot,
                motherboardHostRoot,
                _installedAssemblyHostRoots,
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
            LastEvaluation = new Atx24PowerCableRouteEvaluation(
                Atx24PowerCableRouteStatus.Uninitialized,
                default,
                false,
                default);
        }

        private void ApplyPreview(Atx24PowerCableRouteEvaluation evaluation)
        {
            if (!_routeModeActive || !evaluation.HasPreview || !IsConfigured)
            {
                HidePreview();
                return;
            }

            Color color = evaluation.CanRoute ? validColor : invalidColor;
            ConfigurePreviewLine(
                previewLines[0],
                color,
                psuPrimaryEndpoint.position,
                waypoints[0].position);
            ConfigurePreviewLine(
                previewLines[1],
                color,
                psuSenseEndpoint.position,
                waypoints[0].position);
            ConfigurePreviewLine(
                previewLines[2],
                color,
                waypoints[0].position,
                waypoints[1].position,
                waypoints[2].position,
                motherboardEndpoint.position);
        }

        private static void ConfigurePreviewLine(
            LineRenderer line,
            Color color,
            Vector3 first,
            Vector3 second)
        {
            line.startColor = color;
            line.endColor = color;
            line.positionCount = 2;
            line.SetPosition(0, first);
            line.SetPosition(1, second);
            line.enabled = true;
        }

        private static void ConfigurePreviewLine(
            LineRenderer line,
            Color color,
            Vector3 first,
            Vector3 second,
            Vector3 third,
            Vector3 fourth)
        {
            line.startColor = color;
            line.endColor = color;
            line.positionCount = 4;
            line.SetPosition(0, first);
            line.SetPosition(1, second);
            line.SetPosition(2, third);
            line.SetPosition(3, fourth);
            line.enabled = true;
        }

        private void HidePreview()
        {
            if (previewLines == null)
            {
                return;
            }

            foreach (LineRenderer line in previewLines)
            {
                if (line != null)
                {
                    line.enabled = false;
                }
            }
        }

        private bool HasCanonicalIdentities()
        {
            return routeId == GarageStockFlowSession.Atx24PowerCableRouteIdValue &&
                   psuPrimaryEndpointId ==
                       GarageStockFlowSession.Atx24PowerCablePsuPrimaryEndpointIdValue &&
                   psuSenseEndpointId ==
                       GarageStockFlowSession.Atx24PowerCablePsuSenseEndpointIdValue &&
                   motherboardEndpointId ==
                       GarageStockFlowSession.Atx24PowerCableMotherboardEndpointIdValue &&
                   waypointIds != null &&
                   waypointIds.Length == 3 &&
                   waypointIds[0] ==
                       GarageStockFlowSession.Atx24PowerCableWaypoint1IdValue &&
                   waypointIds[1] ==
                       GarageStockFlowSession.Atx24PowerCableWaypoint2IdValue &&
                   waypointIds[2] ==
                       GarageStockFlowSession.Atx24PowerCableWaypoint3IdValue;
        }

        private static bool AreDistinct<T>(T[] values)
            where T : UnityEngine.Object
        {
            if (values == null)
            {
                return false;
            }

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
