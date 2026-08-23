using System;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Canonical r31 visual contract for one EPS12V/CPU 8-pin cable. The same
    /// physical item is shown as a loose coil or as five authored route points;
    /// no rope simulation, joint, duplicate cable or hidden inventory authority
    /// is created here.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Eps12vPowerCableRuntimeGeometry : MonoBehaviour
    {
        public const string RuntimeMarker = "r31.eps12v-power-cable.geometry";

        [SerializeField] private Transform psu8Connector;
        [SerializeField] private Transform motherboard8Connector;
        [SerializeField] private LineRenderer looseCoil;
        [SerializeField] private LineRenderer routedTrunk;
        [SerializeField] private Transform psuAnchor;
        [SerializeField] private Transform motherboardAnchor;
        [SerializeField] private Transform[] waypoints = new Transform[3];

        private Vector3 _psuLoosePosition;
        private Quaternion _psuLooseRotation;
        private Vector3 _motherboardLoosePosition;
        private Quaternion _motherboardLooseRotation;
        private readonly Vector3[] _looseCoilPoints = new Vector3[15];
        private bool _configurationValidated;

        public Transform Psu8Connector => psu8Connector;

        public Transform Motherboard8Connector => motherboard8Connector;

        public LineRenderer LooseCoil => looseCoil;

        public LineRenderer RoutedTrunk => routedTrunk;

        public bool IsRouted { get; private set; }

        public bool IsCanonical => ValidateCanonicalConfiguration();

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
            Transform authoredPsu8Connector,
            Transform authoredMotherboard8Connector,
            LineRenderer authoredLooseCoil,
            LineRenderer authoredRoutedTrunk,
            Transform authoredPsuAnchor,
            Transform authoredMotherboardAnchor,
            Transform[] authoredWaypoints)
        {
            if (authoredWaypoints == null || authoredWaypoints.Length != 3)
            {
                throw new ArgumentException(
                    "EPS12V geometry requires exactly three authored waypoints.",
                    nameof(authoredWaypoints));
            }

            psu8Connector = authoredPsu8Connector ??
                throw new ArgumentNullException(nameof(authoredPsu8Connector));
            motherboard8Connector = authoredMotherboard8Connector ??
                throw new ArgumentNullException(nameof(authoredMotherboard8Connector));
            looseCoil = authoredLooseCoil ??
                throw new ArgumentNullException(nameof(authoredLooseCoil));
            routedTrunk = authoredRoutedTrunk ??
                throw new ArgumentNullException(nameof(authoredRoutedTrunk));
            psuAnchor = authoredPsuAnchor ??
                throw new ArgumentNullException(nameof(authoredPsuAnchor));
            motherboardAnchor = authoredMotherboardAnchor ??
                throw new ArgumentNullException(nameof(authoredMotherboardAnchor));
            waypoints = (Transform[])authoredWaypoints.Clone();

            CaptureLooseConnectorPoses();
            looseCoil.useWorldSpace = true;
            routedTrunk.useWorldSpace = true;

            if (!ValidateCanonicalConfiguration())
            {
                throw new ArgumentException(
                    "EPS12V geometry must preserve one physical item, two connectors and zero joints.");
            }

            _configurationValidated = true;
            SetRouted(routed: false);
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
                psu8Connector.SetPositionAndRotation(
                    psuAnchor.position,
                    psuAnchor.rotation);
                motherboard8Connector.SetPositionAndRotation(
                    motherboardAnchor.position,
                    motherboardAnchor.rotation);
                routedTrunk.positionCount = 5;
                routedTrunk.SetPosition(0, psuAnchor.position);
                routedTrunk.SetPosition(1, waypoints[0].position);
                routedTrunk.SetPosition(2, waypoints[1].position);
                routedTrunk.SetPosition(3, waypoints[2].position);
                routedTrunk.SetPosition(4, motherboardAnchor.position);
                return;
            }

            for (int index = 0; index < _looseCoilPoints.Length; index++)
            {
                float angle = (Mathf.PI * 2f * index) /
                              (_looseCoilPoints.Length - 1);
                Vector3 local = new Vector3(
                    Mathf.Cos(angle) * 0.044f,
                    0.003f + Mathf.Sin(angle * 2f) * 0.003f,
                    Mathf.Sin(angle) * 0.030f);
                _looseCoilPoints[index] = transform.TransformPoint(local);
            }

            looseCoil.positionCount = _looseCoilPoints.Length;
            looseCoil.SetPositions(_looseCoilPoints);
        }

        private void CaptureLooseConnectorPoses()
        {
            _psuLoosePosition = psu8Connector.localPosition;
            _psuLooseRotation = psu8Connector.localRotation;
            _motherboardLoosePosition = motherboard8Connector.localPosition;
            _motherboardLooseRotation = motherboard8Connector.localRotation;
        }

        private void RestoreLooseConnectorPoses()
        {
            if (psu8Connector != null)
            {
                psu8Connector.localPosition = _psuLoosePosition;
                psu8Connector.localRotation = _psuLooseRotation;
            }

            if (motherboard8Connector != null)
            {
                motherboard8Connector.localPosition = _motherboardLoosePosition;
                motherboard8Connector.localRotation =
                    _motherboardLooseRotation;
            }
        }

        private bool ValidateCanonicalConfiguration()
        {
            return IsOwnedChild(psu8Connector) &&
                   IsOwnedChild(motherboard8Connector) &&
                   psu8Connector != motherboard8Connector &&
                   looseCoil != null &&
                   routedTrunk != null &&
                   looseCoil != routedTrunk &&
                   psuAnchor != null &&
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

        private static bool AreDistinct<T>(T[] values)
            where T : UnityEngine.Object
        {
            for (int left = 0; left < values.Length; left++)
            {
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
