using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class MotherboardFastenerProjection : MonoBehaviour
    {
        [SerializeField] private string fastenerId;
        [SerializeField] private Collider focusCollider;
        [SerializeField] private Renderer fastenerRenderer;
        [SerializeField] private Transform screwHead;
        [SerializeField] private Transform screwdriver;
        [SerializeField] private TextMesh statusText;
        [SerializeField] private Material readyMaterial;
        [SerializeField] private Material validMaterial;
        [SerializeField] private Material invalidMaterial;
        [SerializeField] private Material securedMaterial;
        [SerializeField, Min(0.1f)] private float maximumRange = 2f;
        [SerializeField, Range(0f, 1f)] private float minimumFocusDot = 0.975f;

        [SerializeField] private Quaternion openScrewRotation = Quaternion.identity;
        [SerializeField] private Quaternion readyToolRotation = Quaternion.identity;
        [SerializeField] private Vector3 openScrewPosition;
        private AssemblySeatState _authoritativeState = AssemblySeatState.Empty;

        public string FastenerIdValue => fastenerId;

        public Collider FocusCollider => focusCollider;

        public Transform ScrewHead => screwHead;

        public Transform Screwdriver => screwdriver;

        public TextMesh StatusText => statusText;

        public bool IsShowingSecured { get; private set; }

        public bool IsConfigured =>
            !string.IsNullOrWhiteSpace(fastenerId) &&
            focusCollider != null &&
            focusCollider.gameObject.activeInHierarchy &&
            fastenerRenderer != null &&
            screwHead != null &&
            screwdriver != null &&
            statusText != null;

        public MotherboardFastenerEvaluation LastEvaluation { get; private set; }

        public void Configure(
            string stableFastenerId,
            Collider authoredFocusCollider,
            Renderer authoredFastenerRenderer,
            Transform authoredScrewHead,
            Transform authoredScrewdriver,
            TextMesh authoredStatusText,
            Material idle,
            Material valid,
            Material invalid,
            Material secured,
            float range = 2f,
            float focusDot = 0.975f)
        {
            fastenerId = StableId<AssemblyFastenerIdScope>.Parse(stableFastenerId).Value;
            focusCollider = authoredFocusCollider != null
                ? authoredFocusCollider
                : throw new ArgumentNullException(nameof(authoredFocusCollider));
            fastenerRenderer = authoredFastenerRenderer != null
                ? authoredFastenerRenderer
                : throw new ArgumentNullException(nameof(authoredFastenerRenderer));
            screwHead = authoredScrewHead != null
                ? authoredScrewHead
                : throw new ArgumentNullException(nameof(authoredScrewHead));
            screwdriver = authoredScrewdriver != null
                ? authoredScrewdriver
                : throw new ArgumentNullException(nameof(authoredScrewdriver));
            statusText = authoredStatusText != null
                ? authoredStatusText
                : throw new ArgumentNullException(nameof(authoredStatusText));
            readyMaterial = idle;
            validMaterial = valid;
            invalidMaterial = invalid;
            securedMaterial = secured;
            maximumRange = Mathf.Max(0.1f, range);
            minimumFocusDot = Mathf.Clamp01(focusDot);
            openScrewRotation = screwHead.localRotation;
            openScrewPosition = screwHead.localPosition;
            readyToolRotation = screwdriver.localRotation;
            ApplyAuthoritativeState(AssemblySeatState.Empty);
        }

        public MotherboardFastenerEvaluation Evaluate(
            Transform interactionOrigin,
            Transform playerRoot,
            LayerMask obstructionMask,
            bool paused,
            bool isSeated,
            bool isSecured)
        {
            LastEvaluation = MotherboardFastenerSolver.Evaluate(
                interactionOrigin,
                playerRoot,
                focusCollider,
                obstructionMask,
                maximumRange,
                minimumFocusDot,
                paused,
                isSeated,
                isSecured);
            ApplyEvaluationFeedback(LastEvaluation);
            return LastEvaluation;
        }

        public void ApplyAuthoritativeState(AssemblySeatState state)
        {
            _authoritativeState = state;
            IsShowingSecured = state == AssemblySeatState.SeatedSecured;
            if (focusCollider != null)
            {
                bool shouldEnable = state != AssemblySeatState.Empty;
                if (focusCollider.enabled != shouldEnable)
                {
                    focusCollider.enabled = shouldEnable;
                }
            }

            if (screwHead != null)
            {
                Quaternion targetRotation = openScrewRotation * Quaternion.Euler(
                    0f,
                    0f,
                    IsShowingSecured ? 90f : 0f);
                if (Quaternion.Angle(screwHead.localRotation, targetRotation) > 0.001f)
                {
                    screwHead.localRotation = targetRotation;
                }

                Vector3 targetPosition = openScrewPosition +
                                         (IsShowingSecured
                                             ? Vector3.forward * 0.004f
                                             : Vector3.zero);
                if ((screwHead.localPosition - targetPosition).sqrMagnitude > 0.0000000001f)
                {
                    screwHead.localPosition = targetPosition;
                }
            }

            if (screwdriver != null)
            {
                Quaternion targetRotation = readyToolRotation * Quaternion.Euler(
                    IsShowingSecured ? 0f : 18f,
                    0f,
                    IsShowingSecured ? 90f : 0f);
                if (Quaternion.Angle(screwdriver.localRotation, targetRotation) > 0.001f)
                {
                    screwdriver.localRotation = targetRotation;
                }
            }

            SetStatusText(GetAuthorityStatusText());

            ApplyBaseMaterial();
            LastEvaluation = new MotherboardFastenerEvaluation(
                MotherboardFastenerStatus.Uninitialized,
                IsShowingSecured);
        }

        public void ResetFeedback()
        {
            ApplyAuthoritativeState(_authoritativeState);
        }

        public bool MatchesAuthorityState(AssemblySeatState state)
        {
            bool isKnownState = state == AssemblySeatState.Empty ||
                                state == AssemblySeatState.SeatedUnsecured ||
                                state == AssemblySeatState.SeatedSecured;
            bool shouldShowSecured = state == AssemblySeatState.SeatedSecured;
            Vector3 expectedScrewPosition = openScrewPosition +
                                            (shouldShowSecured
                                                ? Vector3.forward * 0.004f
                                                : Vector3.zero);
            Quaternion expectedScrewRotation = openScrewRotation * Quaternion.Euler(
                0f,
                0f,
                shouldShowSecured ? 90f : 0f);
            Quaternion expectedToolRotation = readyToolRotation * Quaternion.Euler(
                shouldShowSecured ? 0f : 18f,
                0f,
                shouldShowSecured ? 90f : 0f);

            return _authoritativeState == state &&
                   IsShowingSecured == shouldShowSecured &&
                   focusCollider != null &&
                   focusCollider.enabled == (state != AssemblySeatState.Empty) &&
                   screwHead != null &&
                   screwdriver != null &&
                   (screwHead.localPosition - expectedScrewPosition).sqrMagnitude <=
                       0.0000000001f &&
                   Quaternion.Angle(screwHead.localRotation, expectedScrewRotation) <=
                       0.01f &&
                   Quaternion.Angle(screwdriver.localRotation, expectedToolRotation) <=
                       0.01f &&
                   isKnownState;
        }

        private void ApplyEvaluationFeedback(MotherboardFastenerEvaluation evaluation)
        {
            SetStatusText(evaluation.Status switch
            {
                MotherboardFastenerStatus.ValidUnsecured => "[O] SIKMAYA HAZIR",
                MotherboardFastenerStatus.ValidSecured => "[O] GEVŞETMEYE HAZIR",
                MotherboardFastenerStatus.LineOfSightBlocked => "[X] ÖNÜNÜ AÇ",
                MotherboardFastenerStatus.Obstructed => "[X] ÖNÜNÜ AÇ",
                MotherboardFastenerStatus.Paused => "[||] DURAKLATILDI",
                MotherboardFastenerStatus.ContextMissing => "[X] KULLANILAMAZ",
                _ => GetAuthorityStatusText()
            });

            Material target = evaluation.CanOperate
                ? evaluation.IsSecured && securedMaterial != null
                    ? securedMaterial
                    : validMaterial
                : evaluation.Status == MotherboardFastenerStatus.OutOfRange ||
                  evaluation.Status == MotherboardFastenerStatus.NotFocused ||
                  evaluation.Status == MotherboardFastenerStatus.AuthorityBlocked
                    ? IsShowingSecured ? securedMaterial : readyMaterial
                    : invalidMaterial;
            if (fastenerRenderer != null &&
                target != null &&
                fastenerRenderer.sharedMaterial != target)
            {
                fastenerRenderer.sharedMaterial = target;
            }
        }

        private void ApplyBaseMaterial()
        {
            if (fastenerRenderer == null)
            {
                return;
            }

            Material target = IsShowingSecured && securedMaterial != null
                ? securedMaterial
                : readyMaterial;
            if (target != null && fastenerRenderer.sharedMaterial != target)
            {
                fastenerRenderer.sharedMaterial = target;
            }
        }

        private string GetAuthorityStatusText()
        {
            return _authoritativeState switch
            {
                AssemblySeatState.SeatedUnsecured => "[O] VİDA GEVŞEK",
                AssemblySeatState.SeatedSecured => "[OK] VİDA SIKILI",
                _ => "[ ] ANAKARTI OTURT"
            };
        }

        private void SetStatusText(string value)
        {
            if (statusText != null && statusText.text != value)
            {
                statusText.text = value;
            }
        }

        private void OnValidate()
        {
            maximumRange = Mathf.Max(0.1f, maximumRange);
            minimumFocusDot = Mathf.Clamp01(minimumFocusDot);
        }

        private void Awake()
        {
            if (!IsValidRotation(openScrewRotation) && screwHead != null)
            {
                openScrewRotation = screwHead.localRotation;
            }

            if (!IsValidRotation(readyToolRotation) && screwdriver != null)
            {
                readyToolRotation = screwdriver.localRotation;
            }

            if ((!IsFinite(openScrewPosition) ||
                 (openScrewPosition == Vector3.zero &&
                  screwHead != null &&
                  screwHead.localPosition != Vector3.zero)) &&
                screwHead != null)
            {
                openScrewPosition = screwHead.localPosition;
            }

            ApplyAuthoritativeState(_authoritativeState);
        }

        private static bool IsValidRotation(Quaternion rotation)
        {
            float magnitude = Mathf.Sqrt(
                (rotation.x * rotation.x) +
                (rotation.y * rotation.y) +
                (rotation.z * rotation.z) +
                (rotation.w * rotation.w));
            return magnitude > 0.999f && magnitude < 1.001f;
        }

        private static bool IsFinite(Vector3 value)
        {
            return float.IsFinite(value.x) &&
                   float.IsFinite(value.y) &&
                   float.IsFinite(value.z);
        }
    }
}
