using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PowerSupplyBayProjection : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private string rearMountId;
        [SerializeField] private string topLeftFastenerId;
        [SerializeField] private string topRightFastenerId;
        [SerializeField] private string bottomLeftFastenerId;
        [SerializeField] private string bottomRightFastenerId;
        [SerializeField] private PowerSupplyFormFactor bayFormFactor =
            PowerSupplyFormFactor.AtxPs2;
        [SerializeField] private Transform snapAnchor;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Collider supportCollider;
        [SerializeField] private Transform assemblyRoot;
        [SerializeField] private Transform topLeftFastenerPivot;
        [SerializeField] private Transform topRightFastenerPivot;
        [SerializeField] private Transform bottomLeftFastenerPivot;
        [SerializeField] private Transform bottomRightFastenerPivot;
        [SerializeField] private Collider[] chassisClearanceBlockers =
            Array.Empty<Collider>();
        [SerializeField] private Collider[] cableClearanceBlockers =
            Array.Empty<Collider>();
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;

        [SerializeField, HideInInspector] private Vector3[] _openFastenerPositions =
            Array.Empty<Vector3>();
        [SerializeField, HideInInspector] private Quaternion[] _openFastenerRotations =
            Array.Empty<Quaternion>();
        [SerializeField, HideInInspector]
        private PowerSupplyBayProjectionState _bayState =
            PowerSupplyBayProjectionState.EmptyOpen;

        public string SlotIdValue => slotId;

        public string RearMountIdValue => rearMountId;

        public string TopLeftFastenerIdValue => topLeftFastenerId;

        public string TopRightFastenerIdValue => topRightFastenerId;

        public string BottomLeftFastenerIdValue => bottomLeftFastenerId;

        public string BottomRightFastenerIdValue => bottomRightFastenerId;

        public PowerSupplyFormFactor BayFormFactor => bayFormFactor;

        public Transform SnapAnchor => snapAnchor;

        public Collider FocusCollider => focusCollider;

        public Collider SupportCollider => supportCollider;

        public Transform AssemblyRoot => assemblyRoot;

        public Transform[] FastenerPivots => new[]
        {
            topLeftFastenerPivot,
            topRightFastenerPivot,
            bottomLeftFastenerPivot,
            bottomRightFastenerPivot
        };

        public Collider[] ChassisClearanceBlockers => chassisClearanceBlockers;

        public Collider[] CableClearanceBlockers => cableClearanceBlockers;

        public PowerSupplyBayEvaluation LastEvaluation { get; private set; }

        public bool IsConfigured =>
            AreStableDistinctIds(
                slotId,
                rearMountId,
                topLeftFastenerId,
                topRightFastenerId,
                bottomLeftFastenerId,
                bottomRightFastenerId) &&
            bayFormFactor == PowerSupplyFormFactor.AtxPs2 &&
            snapAnchor != null &&
            focusCollider != null &&
            supportCollider != null &&
            assemblyRoot != null &&
            AreValidFastenerPivots() &&
            snapAnchor.IsChildOf(assemblyRoot) &&
            focusCollider.transform.IsChildOf(assemblyRoot) &&
            supportCollider.transform.IsChildOf(assemblyRoot) &&
            AreValidDistinctColliders(chassisClearanceBlockers) &&
            AreValidDistinctColliders(cableClearanceBlockers) &&
            !HaveSharedCollider(chassisClearanceBlockers, cableClearanceBlockers);

        public void Configure(
            string stableSlotId,
            string stableRearMountId,
            string stableTopLeftFastenerId,
            string stableTopRightFastenerId,
            string stableBottomLeftFastenerId,
            string stableBottomRightFastenerId,
            Transform authoredSnapAnchor,
            Collider authoredFocusCollider,
            Collider authoredSupportCollider,
            Transform authoredAssemblyRoot,
            Transform authoredTopLeftFastenerPivot,
            Transform authoredTopRightFastenerPivot,
            Transform authoredBottomLeftFastenerPivot,
            Transform authoredBottomRightFastenerPivot,
            PowerSupplyFormFactor authoredFormFactor = PowerSupplyFormFactor.AtxPs2,
            float range = 2f,
            float focusDot = 0.94f)
        {
            string[] ids =
            {
                RequireStableId(stableSlotId, nameof(stableSlotId)),
                RequireStableId(stableRearMountId, nameof(stableRearMountId)),
                RequireStableId(
                    stableTopLeftFastenerId,
                    nameof(stableTopLeftFastenerId)),
                RequireStableId(
                    stableTopRightFastenerId,
                    nameof(stableTopRightFastenerId)),
                RequireStableId(
                    stableBottomLeftFastenerId,
                    nameof(stableBottomLeftFastenerId)),
                RequireStableId(
                    stableBottomRightFastenerId,
                    nameof(stableBottomRightFastenerId))
            };
            if (!AreStableDistinctIds(ids))
            {
                throw new ArgumentException(
                    "The PSU bay, rear mount and four fasteners require distinct stable identities.");
            }

            if (authoredFormFactor != PowerSupplyFormFactor.AtxPs2)
            {
                throw new ArgumentException(
                    "The prototype PSU bay must expose an ATX PS/2 form factor.",
                    nameof(authoredFormFactor));
            }

            slotId = ids[0];
            rearMountId = ids[1];
            topLeftFastenerId = ids[2];
            topRightFastenerId = ids[3];
            bottomLeftFastenerId = ids[4];
            bottomRightFastenerId = ids[5];
            snapAnchor = RequireChild(
                authoredSnapAnchor,
                authoredAssemblyRoot,
                nameof(authoredSnapAnchor));
            focusCollider = RequireColliderChild(
                authoredFocusCollider,
                authoredAssemblyRoot,
                nameof(authoredFocusCollider));
            supportCollider = RequireColliderChild(
                authoredSupportCollider,
                authoredAssemblyRoot,
                nameof(authoredSupportCollider));
            assemblyRoot = authoredAssemblyRoot != null
                ? authoredAssemblyRoot
                : throw new ArgumentNullException(nameof(authoredAssemblyRoot));
            topLeftFastenerPivot = RequireChild(
                authoredTopLeftFastenerPivot,
                assemblyRoot,
                nameof(authoredTopLeftFastenerPivot));
            topRightFastenerPivot = RequireChild(
                authoredTopRightFastenerPivot,
                assemblyRoot,
                nameof(authoredTopRightFastenerPivot));
            bottomLeftFastenerPivot = RequireChild(
                authoredBottomLeftFastenerPivot,
                assemblyRoot,
                nameof(authoredBottomLeftFastenerPivot));
            bottomRightFastenerPivot = RequireChild(
                authoredBottomRightFastenerPivot,
                assemblyRoot,
                nameof(authoredBottomRightFastenerPivot));
            if (!AreDistinctTransforms(FastenerPivots))
            {
                throw new ArgumentException(
                    "The four PSU fasteners require distinct projection pivots.");
            }

            bayFormFactor = authoredFormFactor;
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            CaptureOpenFastenerPoses();
            ConfigureClearanceBlockers(null, null);
            ApplyAuthoritativeState(PowerSupplyBayProjectionState.EmptyOpen);
        }

        public void ConfigureClearanceBlockers(
            Collider[] authoredChassisBlockers,
            Collider[] authoredCableBlockers)
        {
            Collider[] chassis = CloneAndValidate(
                authoredChassisBlockers,
                nameof(authoredChassisBlockers));
            Collider[] cables = CloneAndValidate(
                authoredCableBlockers,
                nameof(authoredCableBlockers));
            if (HaveSharedCollider(chassis, cables))
            {
                throw new ArgumentException(
                    "A clearance collider cannot belong to both chassis and cable sets.");
            }

            chassisClearanceBlockers = chassis;
            cableClearanceBlockers = cables;
        }

        public OperationResult<Pose> ResolveSeatPose(int halfTurns)
        {
            return snapAnchor == null
                ? OperationResult<Pose>.Fail(
                    Failure.FromCode("assembly-power-supply.context-missing"))
                : OperationResult<Pose>.Success(
                    PowerSupplyBaySolver.ResolveSeatPose(snapAnchor, halfTurns));
        }

        public PowerSupplyBayEvaluation EvaluateSeat(
            bool placementModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection powerSupply,
            LayerMask obstructionMask,
            int halfTurns,
            bool paused,
            bool authorityAvailable,
            PowerSupplyFormFactor powerSupplyFormFactor,
            bool chassisClearanceAvailable,
            bool cableClearanceAvailable)
        {
            LastEvaluation = PowerSupplyBaySolver.EvaluateSeat(
                placementModeEnabled,
                interactionOrigin,
                playerRoot,
                powerSupply,
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
                powerSupplyFormFactor,
                bayFormFactor,
                chassisClearanceAvailable,
                cableClearanceAvailable,
                chassisClearanceBlockers,
                cableClearanceBlockers);
            return LastEvaluation;
        }

        public PowerSupplyBayEvaluation EvaluateRecoverySeat(
            PhysicalItemProjection powerSupply,
            LayerMask obstructionMask,
            int halfTurns,
            bool authorityAvailable,
            PowerSupplyFormFactor powerSupplyFormFactor,
            bool chassisClearanceAvailable,
            bool cableClearanceAvailable)
        {
            LastEvaluation = PowerSupplyBaySolver.EvaluateRecoverySeat(
                powerSupply,
                snapAnchor,
                focusCollider,
                supportCollider,
                assemblyRoot,
                obstructionMask,
                halfTurns,
                authorityAvailable,
                powerSupplyFormFactor,
                bayFormFactor,
                chassisClearanceAvailable,
                cableClearanceAvailable,
                chassisClearanceBlockers,
                cableClearanceBlockers);
            return LastEvaluation;
        }

        public PowerSupplyBayEvaluation EvaluateInteraction(
            bool interactionModeEnabled,
            Transform interactionOrigin,
            Transform playerRoot,
            Transform seatedPowerSupply,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable)
        {
            LastEvaluation = PowerSupplyBaySolver.EvaluateInteraction(
                interactionModeEnabled,
                interactionOrigin,
                playerRoot,
                seatedPowerSupply,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                _bayState,
                authorityAvailable,
                true);
            return LastEvaluation;
        }

        public PowerSupplyBayEvaluation ApplyAuthoritativeInteractionFeedback(
            PowerSupplyBayProjectionState bayState)
        {
            PowerSupplyBayStatus status = bayState switch
            {
                PowerSupplyBayProjectionState.PowerSupplyRetained =>
                    PowerSupplyBayStatus.ValidRetained,
                PowerSupplyBayProjectionState.PowerSupplySeatedUnsecured =>
                    PowerSupplyBayStatus.ValidSeatedUnsecured,
                _ => PowerSupplyBayStatus.ContextMissing
            };
            LastEvaluation = new PowerSupplyBayEvaluation(
                status,
                default,
                false,
                default);
            return LastEvaluation;
        }

        public void ApplyAuthoritativeState(PowerSupplyBayProjectionState bayState)
        {
            _bayState = bayState;
            if (focusCollider != null)
            {
                focusCollider.enabled =
                    bayState != PowerSupplyBayProjectionState.Unsupported;
            }

            bool retained = bayState == PowerSupplyBayProjectionState.PowerSupplyRetained;
            Transform[] pivots = FastenerPivots;
            if (_openFastenerPositions.Length == pivots.Length &&
                _openFastenerRotations.Length == pivots.Length)
            {
                for (int index = 0; index < pivots.Length; index++)
                {
                    Transform pivot = pivots[index];
                    if (pivot == null)
                    {
                        continue;
                    }

                    pivot.localPosition = _openFastenerPositions[index] +
                                          (retained
                                              ? Vector3.forward * 0.003f
                                              : Vector3.zero);
                    pivot.localRotation = retained
                        ? _openFastenerRotations[index] *
                          Quaternion.AngleAxis(120f, Vector3.forward)
                        : _openFastenerRotations[index];
                }
            }

            ResetFeedback();
        }

        public bool MatchesLogicalAuthorityState(PowerSupplyBayProjectionState bayState)
        {
            return _bayState == bayState;
        }

        public void ResetFeedback()
        {
            LastEvaluation = new PowerSupplyBayEvaluation(
                PowerSupplyBayStatus.Uninitialized,
                default,
                false,
                default);
        }

        private void CaptureOpenFastenerPoses()
        {
            Transform[] pivots = FastenerPivots;
            _openFastenerPositions = new Vector3[pivots.Length];
            _openFastenerRotations = new Quaternion[pivots.Length];
            for (int index = 0; index < pivots.Length; index++)
            {
                _openFastenerPositions[index] = pivots[index].localPosition;
                _openFastenerRotations[index] = pivots[index].localRotation;
            }
        }

        private bool AreValidFastenerPivots()
        {
            Transform[] pivots = FastenerPivots;
            return AreDistinctTransforms(pivots) &&
                   Array.TrueForAll(
                       pivots,
                       pivot => pivot != null && pivot.IsChildOf(assemblyRoot));
        }

        private static Transform RequireChild(
            Transform value,
            Transform root,
            string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (!value.IsChildOf(root))
            {
                throw new ArgumentException(
                    "The projection transform must belong to the assembly root.",
                    parameterName);
            }

            return value;
        }

        private static Collider RequireColliderChild(
            Collider value,
            Transform root,
            string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            RequireChild(value.transform, root, parameterName);
            return value;
        }

        private static Collider[] CloneAndValidate(
            Collider[] colliders,
            string parameterName)
        {
            if (colliders == null || colliders.Length == 0)
            {
                return Array.Empty<Collider>();
            }

            var clone = (Collider[])colliders.Clone();
            if (!AreValidDistinctColliders(clone))
            {
                throw new ArgumentException(
                    "Clearance blockers require distinct non-null colliders.",
                    parameterName);
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

            foreach (Collider leftCollider in left)
            {
                foreach (Collider rightCollider in right)
                {
                    if (leftCollider != null && leftCollider == rightCollider)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool AreDistinctTransforms(Transform[] transforms)
        {
            if (transforms == null)
            {
                return false;
            }

            for (int index = 0; index < transforms.Length; index++)
            {
                if (transforms[index] == null)
                {
                    return false;
                }

                for (int prior = 0; prior < index; prior++)
                {
                    if (transforms[prior] == transforms[index])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool AreStableDistinctIds(params string[] values)
        {
            if (values == null)
            {
                return false;
            }

            for (int index = 0; index < values.Length; index++)
            {
                if (!IsStableId(values[index]))
                {
                    return false;
                }

                for (int prior = 0; prior < index; prior++)
                {
                    if (string.Equals(
                            values[prior],
                            values[index],
                            StringComparison.Ordinal))
                    {
                        return false;
                    }
                }
            }

            return true;
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
    }
}
