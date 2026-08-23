using System;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Canonical r30 visual contract for one split-PSU ATX24 cable. Routed geometry is
    /// authored with line segments; it never creates joints, rope particles or a second
    /// physical cable instance.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Atx24PowerCableRuntimeGeometry : MonoBehaviour
    {
        public const string RuntimeMarker = "r30.atx24-power-cable.geometry";

        [SerializeField] private Transform psuPrimary18Connector;
        [SerializeField] private Transform psuSense10Connector;
        [SerializeField] private Transform motherboard24Connector;
        [SerializeField] private LineRenderer looseCoil;
        [SerializeField] private LineRenderer psuPrimaryBranch;
        [SerializeField] private LineRenderer psuSenseBranch;
        [SerializeField] private LineRenderer routedTrunk;
        [SerializeField] private Transform psuPrimaryAnchor;
        [SerializeField] private Transform psuSenseAnchor;
        [SerializeField] private Transform motherboardAnchor;
        [SerializeField] private Transform[] waypoints = new Transform[3];

        private Vector3 _primaryLoosePosition;
        private Quaternion _primaryLooseRotation;
        private Vector3 _senseLoosePosition;
        private Quaternion _senseLooseRotation;
        private Vector3 _motherboardLoosePosition;
        private Quaternion _motherboardLooseRotation;
        private readonly Vector3[] _looseCoilPoints = new Vector3[17];
        private bool _configurationValidated;

        public Transform PsuPrimary18Connector => psuPrimary18Connector;

        public Transform PsuSense10Connector => psuSense10Connector;

        public Transform Motherboard24Connector => motherboard24Connector;

        public LineRenderer LooseCoil => looseCoil;

        public LineRenderer PsuPrimaryBranch => psuPrimaryBranch;

        public LineRenderer PsuSenseBranch => psuSenseBranch;

        public LineRenderer RoutedTrunk => routedTrunk;

        public bool IsRouted { get; private set; }

        public bool IsCanonical =>
            IsOwnedChild(psuPrimary18Connector) &&
            IsOwnedChild(psuSense10Connector) &&
            IsOwnedChild(motherboard24Connector) &&
            AreDistinct(
                psuPrimary18Connector,
                psuSense10Connector,
                motherboard24Connector) &&
            looseCoil != null &&
            psuPrimaryBranch != null &&
            psuSenseBranch != null &&
            routedTrunk != null &&
            AreDistinct(
                looseCoil,
                psuPrimaryBranch,
                psuSenseBranch,
                routedTrunk) &&
            psuPrimaryAnchor != null &&
            psuSenseAnchor != null &&
            motherboardAnchor != null &&
            waypoints != null &&
            waypoints.Length == 3 &&
            Array.TrueForAll(waypoints, waypoint => waypoint != null) &&
            AreDistinct(waypoints) &&
            GetComponentsInChildren<Joint>(true).Length == 0 &&
            GetComponentsInChildren<Rigidbody>(true).Length == 1;

        private void Awake()
        {
            _configurationValidated = ValidateCanonicalConfiguration();
            if (!_configurationValidated)
            {
                return;
            }

            CaptureLooseConnectorPoses();
            SetRouted(routed: false);
        }

        public void Configure(
            Transform primary18Connector,
            Transform sense10Connector,
            Transform board24Connector,
            LineRenderer authoredLooseCoil,
            LineRenderer authoredPrimaryBranch,
            LineRenderer authoredSenseBranch,
            LineRenderer authoredRoutedTrunk,
            Transform authoredPsuPrimaryAnchor,
            Transform authoredPsuSenseAnchor,
            Transform authoredMotherboardAnchor,
            Transform[] authoredWaypoints)
        {
            if (authoredWaypoints == null || authoredWaypoints.Length != 3)
            {
                throw new ArgumentException(
                    "ATX24 geometry requires exactly three authored waypoints.",
                    nameof(authoredWaypoints));
            }

            psuPrimary18Connector = primary18Connector ??
                throw new ArgumentNullException(nameof(primary18Connector));
            psuSense10Connector = sense10Connector ??
                throw new ArgumentNullException(nameof(sense10Connector));
            motherboard24Connector = board24Connector ??
                throw new ArgumentNullException(nameof(board24Connector));
            looseCoil = authoredLooseCoil ??
                throw new ArgumentNullException(nameof(authoredLooseCoil));
            psuPrimaryBranch = authoredPrimaryBranch ??
                throw new ArgumentNullException(nameof(authoredPrimaryBranch));
            psuSenseBranch = authoredSenseBranch ??
                throw new ArgumentNullException(nameof(authoredSenseBranch));
            routedTrunk = authoredRoutedTrunk ??
                throw new ArgumentNullException(nameof(authoredRoutedTrunk));
            psuPrimaryAnchor = authoredPsuPrimaryAnchor ??
                throw new ArgumentNullException(nameof(authoredPsuPrimaryAnchor));
            psuSenseAnchor = authoredPsuSenseAnchor ??
                throw new ArgumentNullException(nameof(authoredPsuSenseAnchor));
            motherboardAnchor = authoredMotherboardAnchor ??
                throw new ArgumentNullException(nameof(authoredMotherboardAnchor));
            waypoints = (Transform[])authoredWaypoints.Clone();

            CaptureLooseConnectorPoses();

            looseCoil.useWorldSpace = true;
            psuPrimaryBranch.useWorldSpace = true;
            psuSenseBranch.useWorldSpace = true;
            routedTrunk.useWorldSpace = true;

            if (!ValidateCanonicalConfiguration())
            {
                throw new ArgumentException(
                    "ATX24 geometry must preserve one physical item, three connectors and zero joints.");
            }

            _configurationValidated = true;
            SetRouted(routed: false);
        }

        private void CaptureLooseConnectorPoses()
        {
            _primaryLoosePosition = psuPrimary18Connector.localPosition;
            _primaryLooseRotation = psuPrimary18Connector.localRotation;
            _senseLoosePosition = psuSense10Connector.localPosition;
            _senseLooseRotation = psuSense10Connector.localRotation;
            _motherboardLoosePosition = motherboard24Connector.localPosition;
            _motherboardLooseRotation = motherboard24Connector.localRotation;
        }

        public void SetRouted(bool routed)
        {
            IsRouted = routed;
            if (!routed)
            {
                RestoreLooseConnectorPoses();
            }

            if (looseCoil != null)
            {
                looseCoil.enabled = !routed;
            }

            if (psuPrimaryBranch != null)
            {
                psuPrimaryBranch.enabled = routed;
            }

            if (psuSenseBranch != null)
            {
                psuSenseBranch.enabled = routed;
            }

            if (routedTrunk != null)
            {
                routedTrunk.enabled = routed;
            }

            RefreshGeometry();
        }

        private void LateUpdate()
        {
            RefreshGeometry();
        }

        private void RefreshGeometry()
        {
            if (!_configurationValidated)
            {
                return;
            }

            if (IsRouted)
            {
                psuPrimary18Connector.SetPositionAndRotation(
                    psuPrimaryAnchor.position,
                    psuPrimaryAnchor.rotation);
                psuSense10Connector.SetPositionAndRotation(
                    psuSenseAnchor.position,
                    psuSenseAnchor.rotation);
                motherboard24Connector.SetPositionAndRotation(
                    motherboardAnchor.position,
                    motherboardAnchor.rotation);
                SetLine(
                    psuPrimaryBranch,
                    psuPrimaryAnchor.position,
                    waypoints[0].position);
                SetLine(
                    psuSenseBranch,
                    psuSenseAnchor.position,
                    waypoints[0].position);
                SetLine(
                    routedTrunk,
                    waypoints[0].position,
                    waypoints[1].position,
                    waypoints[2].position,
                    motherboardAnchor.position);
                return;
            }

            for (int index = 0; index < _looseCoilPoints.Length; index++)
            {
                float angle = (Mathf.PI * 2f * index) /
                              (_looseCoilPoints.Length - 1);
                Vector3 local = new Vector3(
                    Mathf.Cos(angle) * 0.052f,
                    0.004f + Mathf.Sin(angle * 2f) * 0.004f,
                    Mathf.Sin(angle) * 0.036f);
                _looseCoilPoints[index] = transform.TransformPoint(local);
            }

            looseCoil.positionCount = _looseCoilPoints.Length;
            looseCoil.SetPositions(_looseCoilPoints);
        }

        private void RestoreLooseConnectorPoses()
        {
            if (psuPrimary18Connector != null)
            {
                psuPrimary18Connector.localPosition = _primaryLoosePosition;
                psuPrimary18Connector.localRotation = _primaryLooseRotation;
            }

            if (psuSense10Connector != null)
            {
                psuSense10Connector.localPosition = _senseLoosePosition;
                psuSense10Connector.localRotation = _senseLooseRotation;
            }

            if (motherboard24Connector != null)
            {
                motherboard24Connector.localPosition = _motherboardLoosePosition;
                motherboard24Connector.localRotation = _motherboardLooseRotation;
            }
        }

        private static void SetLine(
            LineRenderer line,
            Vector3 first,
            Vector3 second)
        {
            line.positionCount = 2;
            line.SetPosition(0, first);
            line.SetPosition(1, second);
        }

        private static void SetLine(
            LineRenderer line,
            Vector3 first,
            Vector3 second,
            Vector3 third,
            Vector3 fourth)
        {
            line.positionCount = 4;
            line.SetPosition(0, first);
            line.SetPosition(1, second);
            line.SetPosition(2, third);
            line.SetPosition(3, fourth);
        }

        private bool ValidateCanonicalConfiguration()
        {
            return IsOwnedChild(psuPrimary18Connector) &&
                   IsOwnedChild(psuSense10Connector) &&
                   IsOwnedChild(motherboard24Connector) &&
                   AreDistinct(
                       psuPrimary18Connector,
                       psuSense10Connector,
                       motherboard24Connector) &&
                   looseCoil != null &&
                   psuPrimaryBranch != null &&
                   psuSenseBranch != null &&
                   routedTrunk != null &&
                   AreDistinct(
                       looseCoil,
                       psuPrimaryBranch,
                       psuSenseBranch,
                       routedTrunk) &&
                   psuPrimaryAnchor != null &&
                   psuSenseAnchor != null &&
                   motherboardAnchor != null &&
                   waypoints != null &&
                   waypoints.Length == 3 &&
                   Array.TrueForAll(waypoints, waypoint => waypoint != null) &&
                   AreDistinct(waypoints) &&
                   GetComponentsInChildren<Joint>(true).Length == 0 &&
                   GetComponentsInChildren<Rigidbody>(true).Length == 1;
        }

        private bool IsOwnedChild(Transform candidate)
        {
            return candidate != null &&
                   candidate != transform &&
                   candidate.IsChildOf(transform);
        }

        private static bool AreDistinct<T>(params T[] values)
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
