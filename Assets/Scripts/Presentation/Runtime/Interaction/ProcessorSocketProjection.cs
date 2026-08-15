using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class ProcessorSocketProjection : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private string retentionId;
        [SerializeField] private Transform snapAnchor;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Transform assemblyRoot;
        [SerializeField] private Transform loadPlatePivot;
        [SerializeField] private Transform retentionLeverPivot;
        [SerializeField] private Renderer ghostRenderer;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;
        [SerializeField] private Quaternion openLoadPlateRotation = Quaternion.identity;
        [SerializeField] private Quaternion openLeverRotation = Quaternion.identity;
        [SerializeField] private Quaternion closedLoadPlateRotation = Quaternion.identity;
        [SerializeField] private Quaternion closedLeverRotation = Quaternion.identity;

        private AssemblySeatState _motherboardState = AssemblySeatState.Empty;
        private ProcessorSocketState _processorState = ProcessorSocketState.EmptyOpen;

        public string SlotIdValue => slotId;

        public string RetentionIdValue => retentionId;

        public Transform SnapAnchor => snapAnchor;

        public Collider FocusCollider => focusCollider;

        public Transform AssemblyRoot => assemblyRoot;

        public Transform LoadPlatePivot => loadPlatePivot;

        public Transform RetentionLeverPivot => retentionLeverPivot;

        public Renderer GhostRenderer => ghostRenderer;

        public Pose SnapPose => snapAnchor != null
            ? new Pose(snapAnchor.position, snapAnchor.rotation)
            : default;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(slotId) &&
            !string.IsNullOrWhiteSpace(retentionId) &&
            snapAnchor != null &&
            focusCollider != null &&
            assemblyRoot != null &&
            loadPlatePivot != null &&
            retentionLeverPivot != null;

        public ProcessorSocketEvaluation LastEvaluation { get; private set; }

        public void Configure(
            string stableSlotId,
            string stableRetentionId,
            Transform authoredSnapAnchor,
            Collider authoredFocusCollider,
            Transform authoredAssemblyRoot,
            Transform authoredLoadPlatePivot,
            Transform authoredRetentionLeverPivot,
            Renderer authoredGhostRenderer,
            Material valid,
            Material invalid,
            float range = 2f,
            float focusDot = 0.94f)
        {
            slotId = StableId<AssemblySlotIdScope>.Parse(stableSlotId).Value;
            retentionId = StableId<AssemblyRetentionIdScope>.Parse(
                stableRetentionId).Value;
            snapAnchor = authoredSnapAnchor != null
                ? authoredSnapAnchor
                : throw new ArgumentNullException(nameof(authoredSnapAnchor));
            focusCollider = authoredFocusCollider != null
                ? authoredFocusCollider
                : throw new ArgumentNullException(nameof(authoredFocusCollider));
            assemblyRoot = authoredAssemblyRoot != null
                ? authoredAssemblyRoot
                : throw new ArgumentNullException(nameof(authoredAssemblyRoot));
            loadPlatePivot = authoredLoadPlatePivot != null
                ? authoredLoadPlatePivot
                : throw new ArgumentNullException(nameof(authoredLoadPlatePivot));
            retentionLeverPivot = authoredRetentionLeverPivot != null
                ? authoredRetentionLeverPivot
                : throw new ArgumentNullException(nameof(authoredRetentionLeverPivot));
            ghostRenderer = authoredGhostRenderer;
            validMaterial = valid;
            invalidMaterial = invalid;
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            openLoadPlateRotation = loadPlatePivot.localRotation;
            openLeverRotation = retentionLeverPivot.localRotation;
            closedLoadPlateRotation = Quaternion.identity;
            closedLeverRotation = Quaternion.identity;
            ApplyAuthoritativeState(AssemblySeatState.Empty, ProcessorSocketState.EmptyOpen);
        }

        public ProcessorSocketEvaluation EvaluateSeat(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection processor,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns,
            bool paused,
            bool authorityAvailable)
        {
            LastEvaluation = ProcessorSocketSolver.EvaluateSeat(
                interactionOrigin,
                playerRoot,
                processor,
                snapAnchor,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                clockwiseQuarterTurns,
                paused,
                authorityAvailable);
            ApplySeatFeedback(LastEvaluation);
            return LastEvaluation;
        }

        public ProcessorSocketEvaluation EvaluateInteraction(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection processor,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable,
            ProcessorSocketState state,
            bool retentionCloseAvailable)
        {
            HideGhost();
            LastEvaluation = ProcessorSocketSolver.EvaluateInteraction(
                interactionOrigin,
                playerRoot,
                processor != null ? processor.transform : null,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                state,
                authorityAvailable,
                retentionCloseAvailable);
            return LastEvaluation;
        }

        public ProcessorSocketEvaluation ApplyAuthoritativeInteractionFeedback(
            AssemblySeatState motherboardState,
            ProcessorSocketState processorState)
        {
            HideGhost();
            ProcessorSocketStatus status = processorState switch
            {
                ProcessorSocketState.ProcessorRetained =>
                    ProcessorSocketStatus.ValidRetained,
                ProcessorSocketState.ProcessorSeatedOpen
                    when motherboardState == AssemblySeatState.SeatedSecured =>
                    ProcessorSocketStatus.ValidSeatedOpen,
                ProcessorSocketState.ProcessorSeatedOpen =>
                    ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked,
                _ => ProcessorSocketStatus.ContextMissing
            };
            LastEvaluation = new ProcessorSocketEvaluation(
                status,
                default,
                false);
            return LastEvaluation;
        }

        public void ApplyAuthoritativeState(
            AssemblySeatState motherboardState,
            ProcessorSocketState processorState)
        {
            _motherboardState = motherboardState;
            _processorState = processorState;
            bool socketAvailable = processorState != ProcessorSocketState.Unsupported &&
                                   motherboardState != AssemblySeatState.Empty;
            if (focusCollider != null && focusCollider.enabled != socketAvailable)
            {
                focusCollider.enabled = socketAvailable;
            }

            bool isOpen = processorState == ProcessorSocketState.EmptyOpen ||
                          processorState == ProcessorSocketState.ProcessorSeatedOpen;
            ApplyRotation(
                loadPlatePivot,
                isOpen ? openLoadPlateRotation : closedLoadPlateRotation);
            ApplyRotation(
                retentionLeverPivot,
                isOpen ? openLeverRotation : closedLeverRotation);
            HideGhost();
            LastEvaluation = new ProcessorSocketEvaluation(
                ProcessorSocketStatus.Uninitialized,
                default,
                false);
        }

        public bool MatchesAuthorityState(
            AssemblySeatState motherboardState,
            ProcessorSocketState processorState)
        {
            bool isOpen = processorState == ProcessorSocketState.EmptyOpen ||
                          processorState == ProcessorSocketState.ProcessorSeatedOpen;
            bool shouldEnable = processorState != ProcessorSocketState.Unsupported &&
                                motherboardState != AssemblySeatState.Empty;
            return _motherboardState == motherboardState &&
                   _processorState == processorState &&
                   focusCollider != null &&
                   focusCollider.enabled == shouldEnable &&
                   loadPlatePivot != null &&
                   retentionLeverPivot != null &&
                   Quaternion.Angle(
                       loadPlatePivot.localRotation,
                       isOpen ? openLoadPlateRotation : closedLoadPlateRotation) <= 0.01f &&
                   Quaternion.Angle(
                       retentionLeverPivot.localRotation,
                       isOpen ? openLeverRotation : closedLeverRotation) <= 0.01f;
        }

        public void ResetFeedback()
        {
            HideGhost();
            LastEvaluation = new ProcessorSocketEvaluation(
                ProcessorSocketStatus.Uninitialized,
                default,
                false);
        }

        private void ApplySeatFeedback(ProcessorSocketEvaluation evaluation)
        {
            if (ghostRenderer == null)
            {
                return;
            }

            bool visible = evaluation.HasPose;
            if (ghostRenderer.enabled != visible)
            {
                ghostRenderer.enabled = visible;
            }

            if (!visible)
            {
                return;
            }

            ghostRenderer.transform.SetPositionAndRotation(
                evaluation.Pose.position,
                evaluation.Pose.rotation);
            Material target = evaluation.CanSeat ? validMaterial : invalidMaterial;
            if (target != null && ghostRenderer.sharedMaterial != target)
            {
                ghostRenderer.sharedMaterial = target;
            }
        }

        private void HideGhost()
        {
            if (ghostRenderer != null && ghostRenderer.enabled)
            {
                ghostRenderer.enabled = false;
            }
        }

        private static void ApplyRotation(Transform target, Quaternion rotation)
        {
            if (target != null && Quaternion.Angle(target.localRotation, rotation) > 0.001f)
            {
                target.localRotation = rotation;
            }
        }

        private void OnValidate()
        {
            maximumRange = Mathf.Max(0.1f, maximumRange);
            minimumFocusDot = Mathf.Clamp01(minimumFocusDot);
        }
    }
}
