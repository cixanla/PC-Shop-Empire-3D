using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.Quality;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public static class CustomPcPackagingStationFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "fulfillment.custom-pc-packaging-station.configuration-missing");
        public static readonly Failure Paused = Failure.FromCode(
            "fulfillment.custom-pc-packaging-station.paused");
        public static readonly Failure HandsBusy = Failure.FromCode(
            "fulfillment.custom-pc-packaging-station.hands-busy");
        public static readonly Failure QualityReleaseMissing = Failure.FromCode(
            "fulfillment.custom-pc-packaging-station.quality-release-missing");
        public static readonly Failure ReviewNotCurrent = Failure.FromCode(
            "fulfillment.custom-pc-packaging-station.review-not-current");
        public static readonly Failure AlreadySealed = Failure.FromCode(
            "fulfillment.custom-pc-packaging-station.already-sealed");
        public static readonly Failure InputReplay = Failure.FromCode(
            "fulfillment.custom-pc-packaging-station.input-replay");
    }

    /// <summary>
    /// Two-step physical command surface: inspect the exact current quality release,
    /// then seal that same receipt into the one package projection.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(69)]
    public sealed class CustomPcPackagingStationProjection : MonoBehaviour
    {
        public const float DefaultInteractionRange = 2.35f;
        public const float DefaultFocusDegrees = 26f;

        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerCarryController playerCarry;
        [SerializeField] private CustomPcPackagePhysicalBinding packageBinding;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private TextMesh statusText;
        [SerializeField, Min(0.1f)] private float interactionRange =
            DefaultInteractionRange;
        [SerializeField, Range(1f, 80f)] private float focusDegrees =
            DefaultFocusDegrees;

        private bool _isReviewing;
        private CustomPcQualityReleaseReceipt _reviewedQualityRelease;
        private int _lastSuccessfulOperationFrame = -1;

        public GarageStockFlowRuntime StockFlow => stockFlow;
        public PlayerInputAdapter PlayerInput => playerInput;
        public FirstPersonMotor PlayerMotor => playerMotor;
        public Camera PlayerCamera => playerCamera;
        public PlayerCarryController PlayerCarry => playerCarry;
        public CustomPcPackagePhysicalBinding PackageBinding => packageBinding;
        public Collider InteractionCollider => interactionCollider;
        public TextMesh StatusText => statusText;
        public bool IsFocused { get; private set; }
        public bool IsReviewing => _isReviewing;
        public string LastFailureCode { get; private set; } = string.Empty;

        public bool IsSealed
        {
            get
            {
                GarageStockFlowSession session = ResolveSession();
                return session != null &&
                       session.TryGetPrototypeCustomPcPackage(out _);
            }
        }

        public string PromptText
        {
            get
            {
                RefreshFocusState();
                if (!IsFocused || playerMotor == null || playerMotor.IsPaused)
                {
                    return string.Empty;
                }

                if (IsSealed)
                {
                    return "PAKET MÜHÜRLENDİ • FİZİKSEL KOLİYİ SEVK ALANINA TAŞI";
                }

                if (playerCarry != null && playerCarry.IsCarrying)
                {
                    return "PAKETLEME ENGELLİ • ELLERİNİ BOŞALT";
                }

                if (!TryGetCurrentQualityRelease(out _))
                {
                    return "PAKETLEME • KALİTE ONAYI VE GÜVENLİ KAPATMA BEKLENİYOR";
                }

                string interact = playerInput != null
                    ? playerInput.InteractBindingPrompt
                    : "E / A";
                return _isReviewing
                    ? $"KALİTE DOSYASI DOĞRULANDI • {interact}: KOLİYİ MÜHÜRLE"
                    : $"{interact}: EXACT KALİTE DOSYASINI İNCELE";
            }
        }

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            PlayerInputAdapter input,
            FirstPersonMotor motor,
            Camera camera,
            PlayerCarryController carry,
            CustomPcPackagePhysicalBinding physicalPackageBinding,
            Collider focusCollider,
            TextMesh stationStatusText)
        {
            stockFlow = garageStockFlow != null
                ? garageStockFlow
                : throw new ArgumentNullException(nameof(garageStockFlow));
            playerInput = input != null
                ? input
                : throw new ArgumentNullException(nameof(input));
            playerMotor = motor != null
                ? motor
                : throw new ArgumentNullException(nameof(motor));
            playerCamera = camera != null
                ? camera
                : throw new ArgumentNullException(nameof(camera));
            playerCarry = carry != null
                ? carry
                : throw new ArgumentNullException(nameof(carry));
            packageBinding = physicalPackageBinding != null
                ? physicalPackageBinding
                : throw new ArgumentNullException(
                    nameof(physicalPackageBinding));
            interactionCollider = focusCollider != null
                ? focusCollider
                : throw new ArgumentNullException(nameof(focusCollider));
            statusText = stationStatusText != null
                ? stationStatusText
                : throw new ArgumentNullException(nameof(stationStatusText));
            RefreshPresentation();
        }

        public void ProcessInputFrame()
        {
            RefreshFocusState();
            ResetReviewIfContextChanged();
            if (playerInput == null || playerMotor == null ||
                playerMotor.IsPaused || playerInput.PausePressedThisFrame ||
                !IsFocused || !playerInput.TryConsumeInteractPressThisFrame())
            {
                return;
            }

            Remember(TryReviewOrSealAuthorized());
            RefreshPresentation();
        }

        public OperationResult TryReviewOrSealForTests()
        {
            OperationResult result = TryReviewOrSealAuthorized();
            Remember(result);
            RefreshPresentation();
            return result;
        }

        public void RefreshPresentation()
        {
            if (statusText == null)
            {
                return;
            }

            if (IsSealed && ResolveSession().TryGetPrototypeCustomPcPackage(
                    out CustomPcPackageReceipt package))
            {
                statusText.text =
                    "CUSTOM PC • MÜHÜRLÜ\n" +
                    $"PACKAGE REV {package.Revision}\nSEVK ALANINA TAŞI";
                return;
            }

            if (!TryGetCurrentQualityRelease(out var quality))
            {
                statusText.text =
                    "PAKETLEME İSTASYONU\nKALİTE ONAYI BEKLENİYOR";
                return;
            }

            statusText.text = _isReviewing
                ? "KALİTE DOSYASI DOĞRULANDI\nAYNI PC • AYNI İŞ EMRİ\nKOLİYİ MÜHÜRLE"
                : "PAKETLEMEYE HAZIR\n" +
                  $"SCORE {quality.BenchmarkScore} • {quality.QualityTier}\n" +
                  "DOSYAYI İNCELE";
        }

        private void Update()
        {
            ProcessInputFrame();
            RefreshPresentation();
        }

        private OperationResult TryReviewOrSealAuthorized()
        {
            if (_lastSuccessfulOperationFrame == Time.frameCount)
            {
                return OperationResult.Fail(
                    CustomPcPackagingStationFailures.InputReplay);
            }

            OperationResult gate = ValidateInteractionGate(requireFocus: false);
            if (gate.IsFailure)
            {
                return gate;
            }

            if (!TryGetCurrentQualityRelease(
                    out CustomPcQualityReleaseReceipt current))
            {
                ResetReview();
                return OperationResult.Fail(
                    CustomPcPackagingStationFailures.QualityReleaseMissing);
            }

            if (!_isReviewing)
            {
                _isReviewing = true;
                _reviewedQualityRelease = current;
                _lastSuccessfulOperationFrame = Time.frameCount;
                return OperationResult.Success();
            }

            if (!ReferenceEquals(_reviewedQualityRelease, current))
            {
                ResetReview();
                return OperationResult.Fail(
                    CustomPcPackagingStationFailures.ReviewNotCurrent);
            }

            OperationResult visualGate = packageBinding.ValidateSealProjection();
            if (visualGate.IsFailure)
            {
                return visualGate;
            }

            GarageStockFlowSession session = ResolveSession();
            OperationResult<CustomPcPackageAuthority> ensured =
                session.EnsureCustomPcPackageAuthority();
            if (ensured.IsFailure)
            {
                return OperationResult.Fail(ensured.Error);
            }

            CustomPcPackageAuthority authority = ensured.Value;
            OperationResult<CustomPcPackageReceipt> sealedPackage =
                authority.TrySealPackage(
                    session.PrototypeCustomPcPackageId,
                    session.CreatePrototypeCustomPcPackageSealOperationId(),
                    current,
                    authority.Revision);
            if (sealedPackage.IsFailure)
            {
                return OperationResult.Fail(sealedPackage.Error);
            }

            OperationResult projection = packageBinding.ActivateSealedPackage(
                sealedPackage.Value);
            if (projection.IsFailure)
            {
                return projection;
            }

            ResetReview();
            _lastSuccessfulOperationFrame = Time.frameCount;
            return OperationResult.Success();
        }

        private OperationResult ValidateInteractionGate(bool requireFocus)
        {
            if (stockFlow == null || playerInput == null || playerMotor == null ||
                playerCamera == null || playerCarry == null ||
                packageBinding == null || interactionCollider == null)
            {
                return OperationResult.Fail(
                    CustomPcPackagingStationFailures.ConfigurationMissing);
            }

            if (playerMotor.IsPaused)
            {
                return OperationResult.Fail(
                    CustomPcPackagingStationFailures.Paused);
            }

            if (playerCarry.IsCarrying || playerCarry.IsDrivingCart)
            {
                return OperationResult.Fail(
                    CustomPcPackagingStationFailures.HandsBusy);
            }

            if (IsSealed)
            {
                return OperationResult.Fail(
                    CustomPcPackagingStationFailures.AlreadySealed);
            }

            if (!requireFocus)
            {
                return OperationResult.Success();
            }

            return WorldInteractionFocusGate.Evaluate(
                playerCamera,
                interactionCollider,
                interactionRange,
                focusDegrees,
                playerMotor.transform);
        }

        private void RefreshFocusState()
        {
            if (stockFlow == null || playerInput == null || playerMotor == null ||
                playerCamera == null || playerCarry == null ||
                packageBinding == null || interactionCollider == null ||
                playerMotor.IsPaused)
            {
                IsFocused = false;
                return;
            }

            IsFocused = WorldInteractionFocusGate.Evaluate(
                playerCamera,
                interactionCollider,
                interactionRange,
                focusDegrees,
                playerMotor.transform).IsSuccess;
        }

        private bool TryGetCurrentQualityRelease(
            out CustomPcQualityReleaseReceipt quality)
        {
            quality = null;
            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetQualityRelease(
                    out CustomPcQualityReleaseAuthority authority))
            {
                return false;
            }

            OperationResult<CustomPcQualityReleaseReceipt> current =
                authority.EvaluateCurrentRelease();
            if (current.IsFailure)
            {
                return false;
            }

            quality = current.Value;
            return true;
        }

        private GarageStockFlowSession ResolveSession()
        {
            return stockFlow != null ? stockFlow.EnsureInitialized() : null;
        }

        private void ResetReviewIfContextChanged()
        {
            if (!_isReviewing)
            {
                return;
            }

            if (playerMotor == null || playerMotor.IsPaused ||
                playerCarry == null || playerCarry.IsCarrying ||
                !TryGetCurrentQualityRelease(out var current) ||
                !ReferenceEquals(current, _reviewedQualityRelease))
            {
                ResetReview();
            }
        }

        private void ResetReview()
        {
            _isReviewing = false;
            _reviewedQualityRelease = null;
        }

        private OperationResult Remember(OperationResult result)
        {
            LastFailureCode = result.IsSuccess ? string.Empty : result.Error.Code;
            return result;
        }
    }
}
