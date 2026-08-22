using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class ProcessorCoolerSlotProjection : MonoBehaviour
    {
        [SerializeField] private string slotId =
            GarageStockFlowSession.ProcessorCoolerSlotIdValue;
        [SerializeField] private string bracketId =
            GarageStockFlowSession.ProcessorCoolerBracketIdValue;
        [SerializeField] private string[] retentionPointIds = new string[4];
        [SerializeField] private Transform snapAnchor;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Transform assemblyRoot;
        [SerializeField] private Transform bracketPivot;
        [SerializeField] private Transform[] retentionPoints = new Transform[4];
        [SerializeField] private Collider[] clearanceBlockers = Array.Empty<Collider>();
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;

        private readonly Vector3[] _openPointPositions = new Vector3[4];
        private readonly Quaternion[] _openPointRotations = new Quaternion[4];
        private AssemblySeatState _motherboardState = AssemblySeatState.Empty;
        private ProcessorSocketState _processorState = ProcessorSocketState.EmptyOpen;
        private ProcessorCoolerSlotState _coolerState =
            ProcessorCoolerSlotState.EmptyOpen;

        public string SlotIdValue => slotId;

        public string BracketIdValue => bracketId;

        public string[] RetentionPointIdValues => retentionPointIds;

        public Transform SnapAnchor => snapAnchor;

        public Collider FocusCollider => focusCollider;

        public Transform AssemblyRoot => assemblyRoot;

        public Transform BracketPivot => bracketPivot;

        public Transform[] RetentionPoints => retentionPoints;

        public Collider[] ClearanceBlockers => clearanceBlockers;

        public Pose SnapPose => snapAnchor != null
            ? new Pose(snapAnchor.position, snapAnchor.rotation)
            : default;

        public OperationResult<Pose> ResolveSeatPose(
            ProcessorCoolerMountOrientation orientation)
        {
            if (snapAnchor == null)
            {
                return OperationResult<Pose>.Fail(
                    Failure.FromCode("assembly-cooler.context-missing"));
            }

            if (orientation != ProcessorCoolerMountOrientation.Primary &&
                orientation != ProcessorCoolerMountOrientation.Rotated180)
            {
                return OperationResult<Pose>.Fail(
                    Failure.FromCode("assembly-cooler.orientation-mismatch"));
            }

            return OperationResult<Pose>.Success(
                ProcessorCoolerSlotSolver.ResolveSeatPose(snapAnchor, orientation));
        }

        public ProcessorCoolerSlotEvaluation LastEvaluation { get; private set; }

        public bool IsConfigured =>
            slotId == GarageStockFlowSession.ProcessorCoolerSlotIdValue &&
            bracketId == GarageStockFlowSession.ProcessorCoolerBracketIdValue &&
            retentionPointIds != null &&
            retentionPointIds.Length == 4 &&
            HasCanonicalRetentionPointIds() &&
            retentionPoints != null &&
            retentionPoints.Length == 4 &&
            Array.TrueForAll(retentionPoints, point => point != null) &&
            AreDistinct(retentionPoints) &&
            clearanceBlockers != null &&
            Array.TrueForAll(clearanceBlockers, blocker => blocker != null) &&
            AreDistinct(clearanceBlockers) &&
            snapAnchor != null &&
            focusCollider != null &&
            assemblyRoot != null &&
            bracketPivot != null &&
            snapAnchor.IsChildOf(assemblyRoot) &&
            focusCollider.transform.IsChildOf(assemblyRoot) &&
            bracketPivot.IsChildOf(assemblyRoot) &&
            Array.TrueForAll(retentionPoints, point => point.IsChildOf(assemblyRoot));

        public void Configure(
            string stableSlotId,
            string stableBracketId,
            string[] stableRetentionPointIds,
            Transform authoredSnapAnchor,
            Collider authoredFocus,
            Transform authoredAssemblyRoot,
            Transform authoredBracket,
            Transform[] authoredRetentionPoints,
            float range = 2f,
            float focusDot = 0.94f)
        {
            if (stableRetentionPointIds == null ||
                stableRetentionPointIds.Length != 4)
            {
                throw new ArgumentException(
                    "Top-down cooler requires exactly four stable retention identities.",
                    nameof(stableRetentionPointIds));
            }

            if (authoredRetentionPoints == null || authoredRetentionPoints.Length != 4)
            {
                throw new ArgumentException(
                    "Top-down cooler requires exactly four retention projections.",
                    nameof(authoredRetentionPoints));
            }

            slotId = StableId<AssemblySlotIdScope>.Parse(stableSlotId).Value;
            bracketId = StableId<AssemblyProcessorCoolerBracketIdScope>.Parse(
                stableBracketId).Value;
            if (slotId != GarageStockFlowSession.ProcessorCoolerSlotIdValue ||
                bracketId != GarageStockFlowSession.ProcessorCoolerBracketIdValue)
            {
                throw new ArgumentException(
                    "The prototype cooler slot and bracket must use canonical identities.");
            }

            retentionPointIds = new string[4];
            retentionPoints = new Transform[4];
            for (int index = 0; index < 4; index++)
            {
                retentionPointIds[index] =
                    StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(
                        stableRetentionPointIds[index]).Value;
                retentionPoints[index] = authoredRetentionPoints[index] != null
                    ? authoredRetentionPoints[index]
                    : throw new ArgumentException(
                        "Retention projections cannot contain null entries.",
                        nameof(authoredRetentionPoints));
                _openPointPositions[index] = retentionPoints[index].localPosition;
                _openPointRotations[index] = retentionPoints[index].localRotation;
            }

            if (!HasCanonicalRetentionPointIds() || !AreDistinct(retentionPoints))
            {
                throw new ArgumentException(
                    "The prototype cooler requires the canonical four distinct retention points.",
                    nameof(authoredRetentionPoints));
            }

            snapAnchor = authoredSnapAnchor != null
                ? authoredSnapAnchor
                : throw new ArgumentNullException(nameof(authoredSnapAnchor));
            focusCollider = authoredFocus != null
                ? authoredFocus
                : throw new ArgumentNullException(nameof(authoredFocus));
            assemblyRoot = authoredAssemblyRoot != null
                ? authoredAssemblyRoot
                : throw new ArgumentNullException(nameof(authoredAssemblyRoot));
            bracketPivot = authoredBracket != null
                ? authoredBracket
                : throw new ArgumentNullException(nameof(authoredBracket));
            if (!snapAnchor.IsChildOf(assemblyRoot) ||
                !focusCollider.transform.IsChildOf(assemblyRoot) ||
                !bracketPivot.IsChildOf(assemblyRoot) ||
                !Array.TrueForAll(
                    retentionPoints,
                    point => point.IsChildOf(assemblyRoot)))
            {
                throw new ArgumentException(
                    "All cooler slot projections must belong to the assembly root.");
            }

            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            ApplyAuthoritativeState(
                AssemblySeatState.Empty,
                ProcessorSocketState.EmptyOpen,
                ProcessorCoolerSlotState.EmptyOpen);
        }

        public void ConfigureClearanceBlockers(Collider[] authoredClearanceBlockers)
        {
            if (authoredClearanceBlockers == null)
            {
                clearanceBlockers = Array.Empty<Collider>();
                return;
            }

            clearanceBlockers = new Collider[authoredClearanceBlockers.Length];
            for (int index = 0; index < authoredClearanceBlockers.Length; index++)
            {
                clearanceBlockers[index] = authoredClearanceBlockers[index] != null
                    ? authoredClearanceBlockers[index]
                    : throw new ArgumentException(
                        "Cooler clearance blockers cannot contain null entries.",
                        nameof(authoredClearanceBlockers));
            }
        }

        public ProcessorCoolerSlotEvaluation EvaluateSeat(
            Transform origin,
            Transform player,
            PhysicalItemProjection cooler,
            LayerMask mask,
            int halfTurns,
            bool paused,
            bool authorityAvailable)
        {
            LastEvaluation = ProcessorCoolerSlotSolver.EvaluateSeat(
                origin,
                player,
                cooler,
                snapAnchor,
                focusCollider,
                assemblyRoot,
                mask,
                maximumRange,
                minimumFocusDot,
                halfTurns,
                paused,
                authorityAvailable,
                clearanceBlockers);
            return LastEvaluation;
        }

        public ProcessorCoolerSlotEvaluation EvaluateInteraction(
            Transform origin,
            Transform player,
            Transform seatedCooler,
            LayerMask mask,
            bool paused,
            bool authorityAvailable,
            bool retentionAvailable)
        {
            LastEvaluation = ProcessorCoolerSlotSolver.EvaluateInteraction(
                origin,
                player,
                seatedCooler,
                focusCollider,
                assemblyRoot,
                mask,
                maximumRange,
                minimumFocusDot,
                paused,
                _coolerState,
                authorityAvailable,
                retentionAvailable);
            return LastEvaluation;
        }

        public void ApplyAuthoritativeState(
            AssemblySeatState motherboardState,
            ProcessorSocketState processorState,
            ProcessorCoolerSlotState coolerState)
        {
            _motherboardState = motherboardState;
            _processorState = processorState;
            _coolerState = coolerState;
            if (focusCollider != null)
            {
                focusCollider.enabled = coolerState !=
                    ProcessorCoolerSlotState.Unsupported &&
                    motherboardState != AssemblySeatState.Empty;
            }

            bool retained = coolerState == ProcessorCoolerSlotState.CoolerRetained;
            if (bracketPivot != null)
            {
                bracketPivot.localRotation = retained
                    ? Quaternion.Euler(0f, 0f, 90f)
                    : Quaternion.identity;
            }

            if (retentionPoints != null && retentionPoints.Length == 4)
            {
                for (int index = 0; index < retentionPoints.Length; index++)
                {
                    Transform point = retentionPoints[index];
                    if (point == null)
                    {
                        continue;
                    }

                    point.localPosition = _openPointPositions[index] +
                        (retained ? Vector3.down * 0.003f : Vector3.zero);
                    point.localRotation = retained
                        ? _openPointRotations[index] *
                          Quaternion.AngleAxis(90f, Vector3.up)
                        : _openPointRotations[index];
                }
            }

            ResetFeedback();
        }

        public bool MatchesLogicalAuthorityState(
            AssemblySeatState motherboardState,
            ProcessorSocketState processorState,
            ProcessorCoolerSlotState coolerState)
        {
            return _motherboardState == motherboardState &&
                   _processorState == processorState &&
                   _coolerState == coolerState;
        }

        public void ResetFeedback()
        {
            LastEvaluation = new ProcessorCoolerSlotEvaluation(
                ProcessorCoolerSlotStatus.Uninitialized,
                default,
                false,
                default);
        }

        private bool HasCanonicalRetentionPointIds()
        {
            return retentionPointIds != null &&
                   retentionPointIds.Length == 4 &&
                   retentionPointIds[0] ==
                       GarageStockFlowSession.ProcessorCoolerRetentionPoint1IdValue &&
                   retentionPointIds[1] ==
                       GarageStockFlowSession.ProcessorCoolerRetentionPoint2IdValue &&
                   retentionPointIds[2] ==
                       GarageStockFlowSession.ProcessorCoolerRetentionPoint3IdValue &&
                   retentionPointIds[3] ==
                       GarageStockFlowSession.ProcessorCoolerRetentionPoint4IdValue;
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
