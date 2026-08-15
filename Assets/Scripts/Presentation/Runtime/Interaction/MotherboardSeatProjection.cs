using System;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class MotherboardSeatProjection : MonoBehaviour
    {
        [SerializeField] private Transform snapAnchor;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Collider supportCollider;
        [SerializeField] private Transform chassisRoot;
        [SerializeField] private Renderer statusRenderer;
        [SerializeField] private Material readyMaterial;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;

        public Transform SnapAnchor => snapAnchor;

        public Collider FocusCollider => focusCollider;

        public Collider SupportCollider => supportCollider;

        public Transform ChassisRoot => chassisRoot;

        public Pose SnapPose => snapAnchor != null
            ? new Pose(snapAnchor.position, snapAnchor.rotation)
            : default;

        public bool IsConfigured => snapAnchor != null &&
                                    focusCollider != null &&
                                    focusCollider.enabled &&
                                    focusCollider.gameObject.activeInHierarchy &&
                                    supportCollider != null &&
                                    supportCollider.enabled &&
                                    supportCollider.gameObject.activeInHierarchy &&
                                    chassisRoot != null &&
                                    chassisRoot.gameObject.activeInHierarchy;

        public MotherboardSeatEvaluation LastEvaluation { get; private set; }

        public void Configure(
            Transform authoredSnapAnchor,
            Collider authoredFocusCollider,
            Collider authoredSupportCollider,
            Transform authoredChassisRoot,
            Renderer feedbackRenderer,
            Material idle,
            Material valid,
            Material invalid,
            float range = 2f,
            float focusDot = 0.94f)
        {
            snapAnchor = authoredSnapAnchor != null
                ? authoredSnapAnchor
                : throw new ArgumentNullException(nameof(authoredSnapAnchor));
            focusCollider = authoredFocusCollider != null
                ? authoredFocusCollider
                : throw new ArgumentNullException(nameof(authoredFocusCollider));
            supportCollider = authoredSupportCollider != null
                ? authoredSupportCollider
                : throw new ArgumentNullException(nameof(authoredSupportCollider));
            chassisRoot = authoredChassisRoot != null
                ? authoredChassisRoot
                : throw new ArgumentNullException(nameof(authoredChassisRoot));
            statusRenderer = feedbackRenderer;
            readyMaterial = idle;
            validMaterial = valid;
            invalidMaterial = invalid;
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            ResetFeedback();
        }

        public MotherboardSeatEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection motherboard,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns,
            bool paused,
            bool authorityAvailable)
        {
            LastEvaluation = MotherboardSeatSolver.Evaluate(
                interactionOrigin,
                playerRoot,
                motherboard,
                snapAnchor,
                focusCollider,
                supportCollider,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                clockwiseQuarterTurns,
                paused,
                authorityAvailable);
            ApplyFeedback(LastEvaluation);
            return LastEvaluation;
        }

        public void ResetFeedback()
        {
            LastEvaluation = new MotherboardSeatEvaluation(
                MotherboardSeatStatus.Uninitialized,
                default,
                false);
            if (statusRenderer != null && readyMaterial != null)
            {
                statusRenderer.sharedMaterial = readyMaterial;
            }
        }

        private void ApplyFeedback(MotherboardSeatEvaluation evaluation)
        {
            if (statusRenderer == null)
            {
                return;
            }

            Material target = evaluation.IsValid ? validMaterial : invalidMaterial;
            if (target != null)
            {
                statusRenderer.sharedMaterial = target;
            }
        }

        private void OnValidate()
        {
            maximumRange = Mathf.Max(0.1f, maximumRange);
            minimumFocusDot = Mathf.Clamp01(minimumFocusDot);
        }
    }
}
