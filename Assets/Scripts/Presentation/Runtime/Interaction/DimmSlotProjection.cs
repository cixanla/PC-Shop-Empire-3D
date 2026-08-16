using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum DimmLatchVisualPhase
    {
        Stable = 0,
        ClosingLeft = 1,
        ClosingRight = 2,
        OpeningRight = 3,
        OpeningLeft = 4
    }

    [DisallowMultipleComponent]
    public sealed class DimmSlotProjection : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField] private string retentionId;
        [SerializeField] private string channelId;
        [SerializeField] private string bankId;
        [SerializeField] private Transform snapAnchor;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Transform assemblyRoot;
        [SerializeField] private Transform leftLatchPivot;
        [SerializeField] private Transform rightLatchPivot;
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.94f;
        [SerializeField] private Quaternion openLeftLatchRotation = Quaternion.identity;
        [SerializeField] private Quaternion openRightLatchRotation = Quaternion.identity;
        [SerializeField] private Quaternion closedLeftLatchRotation = Quaternion.identity;
        [SerializeField] private Quaternion closedRightLatchRotation = Quaternion.identity;
        [SerializeField, Min(1f)] private float latchDegreesPerSecond = 420f;

        private AssemblySeatState _motherboardState = AssemblySeatState.Empty;
        private MemorySlotState _memoryState = MemorySlotState.EmptyOpen;

        public string SlotIdValue => slotId;

        public string RetentionIdValue => retentionId;

        public string ChannelIdValue => channelId;

        public string BankIdValue => bankId;

        public Transform SnapAnchor => snapAnchor;

        public Collider FocusCollider => focusCollider;

        public Transform AssemblyRoot => assemblyRoot;

        public Transform LeftLatchPivot => leftLatchPivot;

        public Transform RightLatchPivot => rightLatchPivot;

        public Pose SnapPose => snapAnchor != null
            ? new Pose(snapAnchor.position, snapAnchor.rotation)
            : default;

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(slotId) &&
            !string.IsNullOrWhiteSpace(retentionId) &&
            !string.IsNullOrWhiteSpace(channelId) &&
            !string.IsNullOrWhiteSpace(bankId) &&
            snapAnchor != null &&
            focusCollider != null &&
            assemblyRoot != null &&
            leftLatchPivot != null &&
            rightLatchPivot != null &&
            leftLatchPivot != rightLatchPivot;

        public DimmSlotEvaluation LastEvaluation { get; private set; }

        public DimmLatchVisualPhase LatchVisualPhase { get; private set; }

        public bool IsLatchAnimating => LatchVisualPhase != DimmLatchVisualPhase.Stable;

        public void Configure(
            string stableSlotId,
            string stableRetentionId,
            string stableChannelId,
            string stableBankId,
            Transform authoredSnapAnchor,
            Collider authoredFocusCollider,
            Transform authoredAssemblyRoot,
            Transform authoredLeftLatchPivot,
            Transform authoredRightLatchPivot,
            float range = 2f,
            float focusDot = 0.94f)
        {
            if (authoredLeftLatchPivot != null &&
                authoredRightLatchPivot != null &&
                authoredLeftLatchPivot == authoredRightLatchPivot)
            {
                throw new ArgumentException(
                    "The DIMM slot requires two distinct visual latch pivots.",
                    nameof(authoredRightLatchPivot));
            }

            slotId = StableId<AssemblySlotIdScope>.Parse(stableSlotId).Value;
            retentionId = StableId<AssemblyRetentionIdScope>.Parse(stableRetentionId).Value;
            channelId = StableId<AssemblyMemoryChannelIdScope>.Parse(stableChannelId).Value;
            bankId = StableId<AssemblyMemoryBankIdScope>.Parse(stableBankId).Value;
            snapAnchor = authoredSnapAnchor != null
                ? authoredSnapAnchor
                : throw new ArgumentNullException(nameof(authoredSnapAnchor));
            focusCollider = authoredFocusCollider != null
                ? authoredFocusCollider
                : throw new ArgumentNullException(nameof(authoredFocusCollider));
            assemblyRoot = authoredAssemblyRoot != null
                ? authoredAssemblyRoot
                : throw new ArgumentNullException(nameof(authoredAssemblyRoot));
            leftLatchPivot = authoredLeftLatchPivot != null
                ? authoredLeftLatchPivot
                : throw new ArgumentNullException(nameof(authoredLeftLatchPivot));
            rightLatchPivot = authoredRightLatchPivot != null
                ? authoredRightLatchPivot
                : throw new ArgumentNullException(nameof(authoredRightLatchPivot));
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            openLeftLatchRotation = leftLatchPivot.localRotation;
            openRightLatchRotation = rightLatchPivot.localRotation;
            closedLeftLatchRotation = Quaternion.identity;
            closedRightLatchRotation = Quaternion.identity;
            ApplyAuthoritativeState(AssemblySeatState.Empty, MemorySlotState.EmptyOpen);
        }

        public DimmSlotEvaluation EvaluateSeat(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection memoryModule,
            LayerMask obstructionMask,
            int clockwiseQuarterTurns,
            bool paused,
            bool authorityAvailable)
        {
            LastEvaluation = DimmSlotSolver.EvaluateSeat(
                interactionOrigin,
                playerRoot,
                memoryModule,
                snapAnchor,
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

        public DimmSlotEvaluation EvaluateInteraction(
            Transform interactionOrigin,
            Transform playerRoot,
            PhysicalItemProjection memoryModule,
            LayerMask obstructionMask,
            bool paused,
            bool authorityAvailable,
            MemorySlotState state,
            bool retentionCloseAvailable)
        {
            LastEvaluation = DimmSlotSolver.EvaluateInteraction(
                interactionOrigin,
                playerRoot,
                memoryModule != null ? memoryModule.transform : null,
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

        public DimmSlotEvaluation ApplyAuthoritativeInteractionFeedback(
            AssemblySeatState motherboardState,
            MemorySlotState memoryState)
        {
            DimmSlotStatus status = memoryState switch
            {
                MemorySlotState.MemoryModuleRetained => DimmSlotStatus.ValidRetained,
                MemorySlotState.MemoryModuleSeatedOpen
                    when motherboardState == AssemblySeatState.SeatedSecured =>
                    DimmSlotStatus.ValidSeatedOpen,
                MemorySlotState.MemoryModuleSeatedOpen =>
                    DimmSlotStatus.ValidSeatedOpenRetentionBlocked,
                _ => DimmSlotStatus.ContextMissing
            };
            LastEvaluation = new DimmSlotEvaluation(status, default, false, default);
            return LastEvaluation;
        }

        public void ApplyAuthoritativeState(
            AssemblySeatState motherboardState,
            MemorySlotState memoryState)
        {
            AssemblySeatState previousMotherboardState = _motherboardState;
            MemorySlotState previousMemoryState = _memoryState;
            _motherboardState = motherboardState;
            _memoryState = memoryState;
            bool slotAvailable = memoryState != MemorySlotState.Unsupported &&
                                 motherboardState != AssemblySeatState.Empty;
            if (focusCollider != null && focusCollider.enabled != slotAvailable)
            {
                focusCollider.enabled = slotAvailable;
            }

            if (previousMotherboardState == motherboardState &&
                previousMemoryState == memoryState)
            {
                return;
            }

            bool wasOpen = previousMemoryState == MemorySlotState.EmptyOpen ||
                           previousMemoryState == MemorySlotState.MemoryModuleSeatedOpen;
            bool isOpen = memoryState == MemorySlotState.EmptyOpen ||
                          memoryState == MemorySlotState.MemoryModuleSeatedOpen;
            if (wasOpen && !isOpen)
            {
                ApplyRotation(leftLatchPivot, openLeftLatchRotation);
                ApplyRotation(rightLatchPivot, openRightLatchRotation);
                LatchVisualPhase = DimmLatchVisualPhase.ClosingLeft;
            }
            else if (!wasOpen && isOpen)
            {
                ApplyRotation(leftLatchPivot, closedLeftLatchRotation);
                ApplyRotation(rightLatchPivot, closedRightLatchRotation);
                LatchVisualPhase = DimmLatchVisualPhase.OpeningRight;
            }
            else
            {
                ApplyRotation(
                    leftLatchPivot,
                    isOpen ? openLeftLatchRotation : closedLeftLatchRotation);
                ApplyRotation(
                    rightLatchPivot,
                    isOpen ? openRightLatchRotation : closedRightLatchRotation);
                LatchVisualPhase = DimmLatchVisualPhase.Stable;
            }

            LastEvaluation = new DimmSlotEvaluation(
                DimmSlotStatus.Uninitialized,
                default,
                false,
                default);
        }

        public bool MatchesAuthorityState(
            AssemblySeatState motherboardState,
            MemorySlotState memoryState)
        {
            bool isOpen = memoryState == MemorySlotState.EmptyOpen ||
                          memoryState == MemorySlotState.MemoryModuleSeatedOpen;
            return MatchesLogicalAuthorityState(motherboardState, memoryState) &&
                   !IsLatchAnimating &&
                   Quaternion.Angle(
                       leftLatchPivot.localRotation,
                       isOpen ? openLeftLatchRotation : closedLeftLatchRotation) <= 0.01f &&
                   Quaternion.Angle(
                       rightLatchPivot.localRotation,
                       isOpen ? openRightLatchRotation : closedRightLatchRotation) <= 0.01f;
        }

        public bool MatchesLogicalAuthorityState(
            AssemblySeatState motherboardState,
            MemorySlotState memoryState)
        {
            bool targetOpen = memoryState == MemorySlotState.EmptyOpen ||
                              memoryState == MemorySlotState.MemoryModuleSeatedOpen;
            bool shouldEnable = memoryState != MemorySlotState.Unsupported &&
                                motherboardState != AssemblySeatState.Empty;
            if (_motherboardState != motherboardState ||
                _memoryState != memoryState ||
                !IsConfigured ||
                focusCollider.enabled != shouldEnable)
            {
                return false;
            }

            return LatchVisualPhase switch
            {
                DimmLatchVisualPhase.Stable => BothLatchesMatchTarget(targetOpen),
                DimmLatchVisualPhase.ClosingLeft =>
                    !targetOpen && MatchesRotation(
                        rightLatchPivot,
                        openRightLatchRotation),
                DimmLatchVisualPhase.ClosingRight =>
                    !targetOpen && MatchesRotation(
                        leftLatchPivot,
                        closedLeftLatchRotation),
                DimmLatchVisualPhase.OpeningRight =>
                    targetOpen && MatchesRotation(
                        leftLatchPivot,
                        closedLeftLatchRotation),
                DimmLatchVisualPhase.OpeningLeft =>
                    targetOpen && MatchesRotation(
                        rightLatchPivot,
                        openRightLatchRotation),
                _ => false
            };
        }

        public void ResetFeedback()
        {
            LastEvaluation = new DimmSlotEvaluation(
                DimmSlotStatus.Uninitialized,
                default,
                false,
                default);
        }

        public void AdvanceLatchAnimation(float unscaledDeltaTime)
        {
            if (!IsLatchAnimating || unscaledDeltaTime <= 0f)
            {
                return;
            }

            float step = Mathf.Max(1f, latchDegreesPerSecond) * unscaledDeltaTime;
            switch (LatchVisualPhase)
            {
                case DimmLatchVisualPhase.ClosingLeft:
                    if (RotateTowards(leftLatchPivot, closedLeftLatchRotation, step))
                    {
                        LatchVisualPhase = DimmLatchVisualPhase.ClosingRight;
                    }

                    break;
                case DimmLatchVisualPhase.ClosingRight:
                    if (RotateTowards(rightLatchPivot, closedRightLatchRotation, step))
                    {
                        LatchVisualPhase = DimmLatchVisualPhase.Stable;
                    }

                    break;
                case DimmLatchVisualPhase.OpeningRight:
                    if (RotateTowards(rightLatchPivot, openRightLatchRotation, step))
                    {
                        LatchVisualPhase = DimmLatchVisualPhase.OpeningLeft;
                    }

                    break;
                case DimmLatchVisualPhase.OpeningLeft:
                    if (RotateTowards(leftLatchPivot, openLeftLatchRotation, step))
                    {
                        LatchVisualPhase = DimmLatchVisualPhase.Stable;
                    }

                    break;
                default:
                    LatchVisualPhase = DimmLatchVisualPhase.Stable;
                    break;
            }
        }

        private void Update()
        {
            AdvanceLatchAnimation(Time.unscaledDeltaTime);
        }

        private static void ApplyRotation(Transform target, Quaternion rotation)
        {
            if (target != null && Quaternion.Angle(target.localRotation, rotation) > 0.001f)
            {
                target.localRotation = rotation;
            }
        }

        private static bool RotateTowards(
            Transform target,
            Quaternion rotation,
            float maximumDegreesDelta)
        {
            if (target == null)
            {
                return true;
            }

            target.localRotation = Quaternion.RotateTowards(
                target.localRotation,
                rotation,
                maximumDegreesDelta);
            return Quaternion.Angle(target.localRotation, rotation) <= 0.01f;
        }

        private bool BothLatchesMatchTarget(bool targetOpen)
        {
            return MatchesRotation(
                       leftLatchPivot,
                       targetOpen ? openLeftLatchRotation : closedLeftLatchRotation) &&
                   MatchesRotation(
                       rightLatchPivot,
                       targetOpen ? openRightLatchRotation : closedRightLatchRotation);
        }

        private static bool MatchesRotation(Transform target, Quaternion rotation)
        {
            return target != null &&
                   Quaternion.Angle(target.localRotation, rotation) <= 0.01f;
        }

        private void OnValidate()
        {
            maximumRange = Mathf.Max(0.1f, maximumRange);
            minimumFocusDot = Mathf.Clamp01(minimumFocusDot);
            latchDegreesPerSecond = Mathf.Max(1f, latchDegreesPerSecond);
        }
    }
}
