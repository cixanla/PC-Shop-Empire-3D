using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class M2StorageSlotProjection : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private string standoffId;
        [SerializeField] private string captiveScrewId;
        [SerializeField] private Transform seatedAnchor;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Transform assemblyRoot;
        [SerializeField] private Transform captiveScrewPivot;
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;

        private Vector3 _openScrewLocalPosition;
        private Quaternion _openScrewLocalRotation = Quaternion.identity;
        private AssemblySeatState _motherboardState = AssemblySeatState.Empty;
        private StorageSlotState _storageState = StorageSlotState.EmptyOpen;

        public string SlotIdValue => slotId;

        public string StandoffIdValue => standoffId;

        public string CaptiveScrewIdValue => captiveScrewId;

        public Transform SeatedAnchor => seatedAnchor;

        public Collider FocusCollider => focusCollider;

        public Transform AssemblyRoot => assemblyRoot;

        public Transform CaptiveScrewPivot => captiveScrewPivot;

        public Pose SeatedPose => seatedAnchor != null
            ? new Pose(seatedAnchor.position, seatedAnchor.rotation)
            : default;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(slotId) &&
            !string.IsNullOrWhiteSpace(standoffId) &&
            !string.IsNullOrWhiteSpace(captiveScrewId) &&
            seatedAnchor != null &&
            focusCollider != null &&
            assemblyRoot != null &&
            captiveScrewPivot != null;

        public M2StorageSlotEvaluation LastEvaluation { get; private set; }

        public void Configure(
            string stableSlotId,
            string stableStandoffId,
            string stableCaptiveScrewId,
            Transform authoredSeatedAnchor,
            Collider authoredFocusCollider,
            Transform authoredAssemblyRoot,
            Transform authoredCaptiveScrewPivot,
            float range = 2f,
            float focusDot = 0.94f)
        {
            slotId = StableId<AssemblySlotIdScope>.Parse(stableSlotId).Value;
            standoffId = StableId<AssemblyStorageStandoffIdScope>.Parse(
                stableStandoffId).Value;
            captiveScrewId = StableId<AssemblyRetentionIdScope>.Parse(
                stableCaptiveScrewId).Value;
            seatedAnchor = authoredSeatedAnchor != null
                ? authoredSeatedAnchor
                : throw new ArgumentNullException(nameof(authoredSeatedAnchor));
            focusCollider = authoredFocusCollider != null
                ? authoredFocusCollider
                : throw new ArgumentNullException(nameof(authoredFocusCollider));
            assemblyRoot = authoredAssemblyRoot != null
                ? authoredAssemblyRoot
                : throw new ArgumentNullException(nameof(authoredAssemblyRoot));
            captiveScrewPivot = authoredCaptiveScrewPivot != null
                ? authoredCaptiveScrewPivot
                : throw new ArgumentNullException(nameof(authoredCaptiveScrewPivot));
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            _openScrewLocalPosition = captiveScrewPivot.localPosition;
            _openScrewLocalRotation = captiveScrewPivot.localRotation;
            ApplyAuthoritativeState(AssemblySeatState.Empty, StorageSlotState.EmptyOpen);
        }

        public M2StorageSlotEvaluation EvaluateSeat(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection storageDevice,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns,
            bool paused,
            bool authorityAvailable)
        {
            LastEvaluation = M2StorageSlotSolver.EvaluateSeat(
                interactionOrigin,
                playerRoot,
                storageDevice,
                seatedAnchor,
                focusCollider,
                assemblyRoot,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                clockwiseQuarterTurns,
                paused,
                authorityAvailable);
            return LastEvaluation;
        }

        public M2StorageSlotEvaluation EvaluateInteraction(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection storageDevice,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable,
            StorageSlotState state,
            bool retentionCloseAvailable)
        {
            LastEvaluation = M2StorageSlotSolver.EvaluateInteraction(
                interactionOrigin,
                playerRoot,
                storageDevice != null ? storageDevice.transform : null,
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

        public M2StorageSlotEvaluation ApplyAuthoritativeInteractionFeedback(
            AssemblySeatState motherboardState,
            StorageSlotState storageState)
        {
            M2StorageSlotStatus status = storageState switch
            {
                StorageSlotState.StorageDeviceSecured =>
                    M2StorageSlotStatus.ValidSecured,
                StorageSlotState.StorageDeviceSeatedUnsecured
                    when motherboardState == AssemblySeatState.SeatedSecured =>
                    M2StorageSlotStatus.ValidSeatedUnsecured,
                StorageSlotState.StorageDeviceSeatedUnsecured =>
                    M2StorageSlotStatus.ValidSeatedUnsecuredRetentionBlocked,
                _ => M2StorageSlotStatus.ContextMissing
            };
            LastEvaluation = new M2StorageSlotEvaluation(
                status,
                default,
                default,
                false,
                default);
            return LastEvaluation;
        }

        public void ApplyAuthoritativeState(
            AssemblySeatState motherboardState,
            StorageSlotState storageState)
        {
            _motherboardState = motherboardState;
            _storageState = storageState;
            bool slotAvailable = storageState != StorageSlotState.Unsupported &&
                                 motherboardState != AssemblySeatState.Empty;
            if (focusCollider != null)
            {
                focusCollider.enabled = slotAvailable;
            }

            if (captiveScrewPivot != null)
            {
                bool secured = storageState == StorageSlotState.StorageDeviceSecured;
                captiveScrewPivot.localPosition = _openScrewLocalPosition +
                    (secured ? Vector3.down * 0.004f : Vector3.zero);
                captiveScrewPivot.localRotation = secured
                    ? _openScrewLocalRotation * Quaternion.AngleAxis(120f, Vector3.up)
                    : _openScrewLocalRotation;
            }

            ResetFeedback();
        }

        public bool MatchesLogicalAuthorityState(
            AssemblySeatState motherboardState,
            StorageSlotState storageState)
        {
            return _motherboardState == motherboardState &&
                   _storageState == storageState;
        }

        public void ResetFeedback()
        {
            LastEvaluation = new M2StorageSlotEvaluation(
                M2StorageSlotStatus.Uninitialized,
                default,
                default,
                false,
                default);
        }
    }
}
