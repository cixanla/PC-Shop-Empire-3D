using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class GraphicsCardSlotProjection : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private string latchId;
        [SerializeField] private string rearBracketId;
        [SerializeField] private string rearBracketFastenerId;
        [SerializeField] private GraphicsCardPcieInterface slotInterface =
            GraphicsCardPcieInterface.PcieX16;
        [SerializeField] private Transform snapAnchor;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Collider supportCollider;
        [SerializeField] private Transform assemblyRoot;
        [SerializeField] private Transform latchPivot;
        [SerializeField] private Transform rearBracketFastenerPivot;
        [SerializeField] private Collider[] chassisClearanceBlockers =
            Array.Empty<Collider>();
        [SerializeField] private Collider[] coolerClearanceBlockers =
            Array.Empty<Collider>();
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;

        [SerializeField, HideInInspector] private Vector3 _openLatchLocalPosition;
        [SerializeField, HideInInspector] private Quaternion _openLatchLocalRotation =
            Quaternion.identity;
        [SerializeField, HideInInspector] private Vector3 _openFastenerLocalPosition;
        [SerializeField, HideInInspector] private Quaternion _openFastenerLocalRotation =
            Quaternion.identity;
        [SerializeField, HideInInspector] private bool _motherboardSecured;
        [SerializeField, HideInInspector]
        private GraphicsCardSlotProjectionState _slotState =
            GraphicsCardSlotProjectionState.EmptyOpen;

        public string SlotIdValue => slotId;

        public string LatchIdValue => latchId;

        public string RearBracketIdValue => rearBracketId;

        public string RearBracketFastenerIdValue => rearBracketFastenerId;

        public GraphicsCardPcieInterface SlotInterface => slotInterface;

        public Transform SnapAnchor => snapAnchor;

        public Collider FocusCollider => focusCollider;

        public Collider SupportCollider => supportCollider;

        public Transform AssemblyRoot => assemblyRoot;

        public Transform LatchPivot => latchPivot;

        public Transform RearBracketFastenerPivot => rearBracketFastenerPivot;

        public Collider[] ChassisClearanceBlockers => chassisClearanceBlockers;

        public Collider[] CoolerClearanceBlockers => coolerClearanceBlockers;

        public GraphicsCardSlotEvaluation LastEvaluation { get; private set; }

        public bool IsConfigured =>
            IsStableId(slotId) &&
            IsStableId(latchId) &&
            IsStableId(rearBracketId) &&
            IsStableId(rearBracketFastenerId) &&
            AreDistinctIds(slotId, latchId, rearBracketId, rearBracketFastenerId) &&
            slotInterface == GraphicsCardPcieInterface.PcieX16 &&
            snapAnchor != null &&
            focusCollider != null &&
            supportCollider != null &&
            assemblyRoot != null &&
            latchPivot != null &&
            rearBracketFastenerPivot != null &&
            snapAnchor.IsChildOf(assemblyRoot) &&
            focusCollider.transform.IsChildOf(assemblyRoot) &&
            supportCollider.transform.IsChildOf(assemblyRoot) &&
            latchPivot.IsChildOf(assemblyRoot) &&
            rearBracketFastenerPivot.IsChildOf(assemblyRoot) &&
            AreValidDistinctColliders(chassisClearanceBlockers) &&
            AreValidDistinctColliders(coolerClearanceBlockers) &&
            !HaveSharedCollider(
                chassisClearanceBlockers,
                coolerClearanceBlockers);

        public void Configure(
            string stableSlotId,
            string stableLatchId,
            string stableRearBracketId,
            string stableRearBracketFastenerId,
            Transform authoredSnapAnchor,
            Collider authoredFocusCollider,
            Collider authoredSupportCollider,
            Transform authoredAssemblyRoot,
            Transform authoredLatchPivot,
            Transform authoredRearBracketFastenerPivot,
            GraphicsCardPcieInterface authoredSlotInterface =
                GraphicsCardPcieInterface.PcieX16,
            float range = 2f,
            float focusDot = 0.94f)
        {
            slotId = RequireStableId(stableSlotId, nameof(stableSlotId));
            latchId = RequireStableId(stableLatchId, nameof(stableLatchId));
            rearBracketId = RequireStableId(
                stableRearBracketId,
                nameof(stableRearBracketId));
            rearBracketFastenerId = RequireStableId(
                stableRearBracketFastenerId,
                nameof(stableRearBracketFastenerId));
            if (!AreDistinctIds(
                    slotId,
                    latchId,
                    rearBracketId,
                    rearBracketFastenerId))
            {
                throw new ArgumentException(
                    "The PCIe slot, latch, bracket and fastener require distinct stable identities.");
            }

            if (authoredSlotInterface != GraphicsCardPcieInterface.PcieX16)
            {
                throw new ArgumentException(
                    "The prototype graphics-card slot must expose a PCIe x16 interface.",
                    nameof(authoredSlotInterface));
            }

            snapAnchor = authoredSnapAnchor != null
                ? authoredSnapAnchor
                : throw new ArgumentNullException(nameof(authoredSnapAnchor));
            focusCollider = authoredFocusCollider != null
                ? authoredFocusCollider
                : throw new ArgumentNullException(nameof(authoredFocusCollider));
            supportCollider = authoredSupportCollider != null
                ? authoredSupportCollider
                : throw new ArgumentNullException(nameof(authoredSupportCollider));
            assemblyRoot = authoredAssemblyRoot != null
                ? authoredAssemblyRoot
                : throw new ArgumentNullException(nameof(authoredAssemblyRoot));
            latchPivot = authoredLatchPivot != null
                ? authoredLatchPivot
                : throw new ArgumentNullException(nameof(authoredLatchPivot));
            rearBracketFastenerPivot = authoredRearBracketFastenerPivot != null
                ? authoredRearBracketFastenerPivot
                : throw new ArgumentNullException(
                    nameof(authoredRearBracketFastenerPivot));
            if (!snapAnchor.IsChildOf(assemblyRoot) ||
                !focusCollider.transform.IsChildOf(assemblyRoot) ||
                !supportCollider.transform.IsChildOf(assemblyRoot) ||
                !latchPivot.IsChildOf(assemblyRoot) ||
                !rearBracketFastenerPivot.IsChildOf(assemblyRoot))
            {
                throw new ArgumentException(
                    "All graphics-card slot projections must belong to the assembly root.");
            }

            slotInterface = authoredSlotInterface;
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            _openLatchLocalPosition = latchPivot.localPosition;
            _openLatchLocalRotation = latchPivot.localRotation;
            _openFastenerLocalPosition = rearBracketFastenerPivot.localPosition;
            _openFastenerLocalRotation = rearBracketFastenerPivot.localRotation;
            ConfigureClearanceBlockers(null, null);
            ApplyAuthoritativeState(
                false,
                GraphicsCardSlotProjectionState.EmptyOpen);
        }

        public void ConfigureClearanceBlockers(
            Collider[] authoredChassisBlockers,
            Collider[] authoredCoolerBlockers)
        {
            Collider[] chassis = CloneAndValidate(
                authoredChassisBlockers,
                nameof(authoredChassisBlockers));
            Collider[] cooler = CloneAndValidate(
                authoredCoolerBlockers,
                nameof(authoredCoolerBlockers));
            if (HaveSharedCollider(chassis, cooler))
            {
                throw new ArgumentException(
                    "A clearance collider cannot belong to both chassis and cooler sets.");
            }

            chassisClearanceBlockers = chassis;
            coolerClearanceBlockers = cooler;
        }

        public OperationResult<Pose> ResolveSeatPose(int halfTurns)
        {
            if (snapAnchor == null)
            {
                return OperationResult<Pose>.Fail(
                    Failure.FromCode("assembly-graphics-card.context-missing"));
            }

            return OperationResult<Pose>.Success(
                GraphicsCardSlotSolver.ResolveSeatPose(snapAnchor, halfTurns));
        }

        public GraphicsCardSlotEvaluation EvaluateSeat(
            bool placementModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection graphicsCard,
            LayerMask obstructionMask,
            int halfTurns,
            bool paused,
            bool authorityAvailable,
            GraphicsCardPcieInterface graphicsCardInterface,
            bool chassisClearanceAvailable,
            bool coolerClearanceAvailable)
        {
            LastEvaluation = GraphicsCardSlotSolver.EvaluateSeat(
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
                coolerClearanceBlockers);
            return LastEvaluation;
        }

        public GraphicsCardSlotEvaluation EvaluateRecoverySeat(
            PhysicalItemProjection graphicsCard,
            LayerMask obstructionMask,
            int halfTurns,
            bool authorityAvailable,
            GraphicsCardPcieInterface graphicsCardInterface,
            bool chassisClearanceAvailable,
            bool coolerClearanceAvailable)
        {
            LastEvaluation = GraphicsCardSlotSolver.EvaluateRecoverySeat(
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
                coolerClearanceBlockers);
            return LastEvaluation;
        }

        public GraphicsCardSlotEvaluation EvaluateInteraction(
            bool interactionModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedGraphicsCard,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable,
            bool retentionAvailable)
        {
            LastEvaluation = GraphicsCardSlotSolver.EvaluateInteraction(
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
                _slotState,
                authorityAvailable,
                retentionAvailable);
            return LastEvaluation;
        }

        public GraphicsCardSlotEvaluation ApplyAuthoritativeInteractionFeedback(
            bool motherboardSecured,
            GraphicsCardSlotProjectionState slotState)
        {
            GraphicsCardSlotStatus status = slotState switch
            {
                GraphicsCardSlotProjectionState.GraphicsCardRetained =>
                    GraphicsCardSlotStatus.ValidRetained,
                GraphicsCardSlotProjectionState.GraphicsCardSeatedUnsecured
                    when motherboardSecured =>
                    GraphicsCardSlotStatus.ValidSeatedUnsecured,
                GraphicsCardSlotProjectionState.GraphicsCardSeatedUnsecured =>
                    GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked,
                _ => GraphicsCardSlotStatus.ContextMissing
            };
            LastEvaluation = new GraphicsCardSlotEvaluation(
                status,
                default,
                false,
                default);
            return LastEvaluation;
        }

        public void ApplyAuthoritativeState(
            bool motherboardSecured,
            GraphicsCardSlotProjectionState slotState)
        {
            _motherboardSecured = motherboardSecured;
            _slotState = slotState;

            bool cardIsSeated =
                slotState ==
                    GraphicsCardSlotProjectionState.GraphicsCardSeatedUnsecured ||
                slotState == GraphicsCardSlotProjectionState.GraphicsCardRetained;
            if (focusCollider != null)
            {
                focusCollider.enabled =
                    slotState != GraphicsCardSlotProjectionState.Unsupported &&
                    (motherboardSecured || cardIsSeated);
            }

            bool retained =
                slotState == GraphicsCardSlotProjectionState.GraphicsCardRetained;
            if (latchPivot != null)
            {
                latchPivot.localPosition = _openLatchLocalPosition;
                latchPivot.localRotation = retained
                    ? _openLatchLocalRotation *
                      Quaternion.AngleAxis(25f, Vector3.up)
                    : _openLatchLocalRotation;
            }

            if (rearBracketFastenerPivot != null)
            {
                rearBracketFastenerPivot.localPosition =
                    _openFastenerLocalPosition +
                    (retained ? Vector3.down * 0.003f : Vector3.zero);
                rearBracketFastenerPivot.localRotation = retained
                    ? _openFastenerLocalRotation *
                      Quaternion.AngleAxis(120f, Vector3.forward)
                    : _openFastenerLocalRotation;
            }

            ResetFeedback();
        }

        public bool MatchesLogicalAuthorityState(
            bool motherboardSecured,
            GraphicsCardSlotProjectionState slotState)
        {
            return _motherboardSecured == motherboardSecured &&
                   _slotState == slotState;
        }

        public void ResetFeedback()
        {
            LastEvaluation = new GraphicsCardSlotEvaluation(
                GraphicsCardSlotStatus.Uninitialized,
                default,
                false,
                default);
        }

        private static Collider[] CloneAndValidate(
            Collider[] colliders,
            string parameterName)
        {
            if (colliders == null || colliders.Length == 0)
            {
                return Array.Empty<Collider>();
            }

            var clone = new Collider[colliders.Length];
            for (int index = 0; index < colliders.Length; index++)
            {
                Collider collider = colliders[index];
                if (collider == null)
                {
                    throw new ArgumentException(
                        "Clearance blocker arrays cannot contain null entries.",
                        parameterName);
                }

                for (int prior = 0; prior < index; prior++)
                {
                    if (clone[prior] == collider)
                    {
                        throw new ArgumentException(
                            "Clearance blocker arrays require distinct colliders.",
                            parameterName);
                    }
                }

                clone[index] = collider;
            }

            return clone;
        }

        private static bool AreValidDistinctColliders(Collider[] colliders)
        {
            if (colliders == null)
            {
                return false;
            }

            for (int index = 0; index < colliders.Length; index++)
            {
                if (colliders[index] == null)
                {
                    return false;
                }

                for (int prior = 0; prior < index; prior++)
                {
                    if (colliders[prior] == colliders[index])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool HaveSharedCollider(Collider[] left, Collider[] right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            for (int leftIndex = 0; leftIndex < left.Length; leftIndex++)
            {
                for (int rightIndex = 0; rightIndex < right.Length; rightIndex++)
                {
                    if (left[leftIndex] != null &&
                        left[leftIndex] == right[rightIndex])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static string RequireStableId(string value, string parameterName)
        {
            if (!IsStableId(value))
            {
                throw new ArgumentException(
                    "A stable, trimmed identity without whitespace is required.",
                    parameterName);
            }

            return value;
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value != value.Trim())
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                if (char.IsWhiteSpace(value[index]) || char.IsControl(value[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool AreDistinctIds(
            string first,
            string second,
            string third,
            string fourth)
        {
            return !string.Equals(first, second, StringComparison.Ordinal) &&
                   !string.Equals(first, third, StringComparison.Ordinal) &&
                   !string.Equals(first, fourth, StringComparison.Ordinal) &&
                   !string.Equals(second, third, StringComparison.Ordinal) &&
                   !string.Equals(second, fourth, StringComparison.Ordinal) &&
                   !string.Equals(third, fourth, StringComparison.Ordinal);
        }
    }
}
