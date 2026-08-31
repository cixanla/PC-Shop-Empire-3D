using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Fulfillment;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public static class CustomPcPackageDispatchFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "fulfillment.custom-pc-package-dispatch.configuration-missing");
        public static readonly Failure Paused = Failure.FromCode(
            "fulfillment.custom-pc-package-dispatch.paused");
        public static readonly Failure PackageNotHeld = Failure.FromCode(
            "fulfillment.custom-pc-package-dispatch.package-not-held");
        public static readonly Failure InputReplay = Failure.FromCode(
            "fulfillment.custom-pc-package-dispatch.input-replay");
    }

    /// <summary>
    /// Explicit physical staging surface. Delivery/courier completion is intentionally
    /// outside this slice; staging only records ActorHands -> DispatchStaging custody.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(70)]
    public sealed class CustomPcPackageDispatchProjection : MonoBehaviour
    {
        public const float DefaultInteractionRange = 2.45f;
        public const float DefaultFocusDegrees = 28f;

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

        private int _lastSuccessfulOperationFrame = -1;

        public PlayerInputAdapter PlayerInput => playerInput;
        public FirstPersonMotor PlayerMotor => playerMotor;
        public Camera PlayerCamera => playerCamera;
        public PlayerCarryController PlayerCarry => playerCarry;
        public CustomPcPackagePhysicalBinding PackageBinding => packageBinding;
        public Collider InteractionCollider => interactionCollider;
        public TextMesh StatusText => statusText;
        public bool IsFocused { get; private set; }
        public string LastFailureCode { get; private set; } = string.Empty;

        public bool IsStaged => packageBinding != null &&
                                packageBinding.TryGetCurrentCustody(
                                    out CustomPcPackageCustody custody) &&
                                custody ==
                                    CustomPcPackageCustody.DispatchStaging;

        public string PromptText
        {
            get
            {
                RefreshFocusState();
                if (!IsFocused || playerMotor == null || playerMotor.IsPaused)
                {
                    return string.Empty;
                }

                if (IsStaged)
                {
                    return "SEVK ALANI • CUSTOM PC GÜVENLİ BİÇİMDE SAHNELENDİ";
                }

                if (!IsPackageHeld())
                {
                    return "SEVK ALANI • MÜHÜRLÜ CUSTOM PC PAKETİNİ GETİR";
                }

                string interact = playerInput != null
                    ? playerInput.InteractBindingPrompt
                    : "E / A";
                return $"{interact}: MÜHÜRLÜ CUSTOM PC'Yİ SEVK ALANINA BIRAK";
            }
        }

        public void Configure(
            PlayerInputAdapter input,
            FirstPersonMotor motor,
            Camera camera,
            PlayerCarryController carry,
            CustomPcPackagePhysicalBinding physicalPackageBinding,
            Collider focusCollider,
            TextMesh stationStatusText)
        {
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
            if (playerInput == null || playerMotor == null ||
                playerMotor.IsPaused || playerInput.PausePressedThisFrame ||
                !IsFocused || !IsPackageHeld() ||
                !playerInput.TryConsumeInteractPressThisFrame())
            {
                return;
            }

            Remember(TryStageAuthorized());
            RefreshPresentation();
        }

        public OperationResult TryStageForTests()
        {
            OperationResult result = TryStageAuthorized();
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

            statusText.text = IsStaged
                ? "SEVK SAHNESİ\nCUSTOM PC • MÜHÜRLÜ\nCUSTODY: DISPATCH STAGING"
                : "SEVK SAHNESİ\nMÜHÜRLÜ CUSTOM PC BEKLENİYOR";
        }

        private void Update()
        {
            ProcessInputFrame();
            RefreshPresentation();
        }

        private OperationResult TryStageAuthorized()
        {
            if (_lastSuccessfulOperationFrame == Time.frameCount)
            {
                return OperationResult.Fail(
                    CustomPcPackageDispatchFailures.InputReplay);
            }

            OperationResult gate = ValidateInteractionGate(requireFocus: false);
            if (gate.IsFailure)
            {
                return gate;
            }

            if (!IsPackageHeld())
            {
                return OperationResult.Fail(
                    CustomPcPackageDispatchFailures.PackageNotHeld);
            }

            OperationResult staged = playerCarry.TryStageHeldCustomPcPackage(
                packageBinding,
                packageBinding.DispatchPose);
            if (staged.IsSuccess)
            {
                _lastSuccessfulOperationFrame = Time.frameCount;
            }

            return staged;
        }

        private OperationResult ValidateInteractionGate(bool requireFocus)
        {
            if (playerInput == null || playerMotor == null ||
                playerCamera == null || playerCarry == null ||
                packageBinding == null || interactionCollider == null)
            {
                return OperationResult.Fail(
                    CustomPcPackageDispatchFailures.ConfigurationMissing);
            }

            if (playerMotor.IsPaused)
            {
                return OperationResult.Fail(
                    CustomPcPackageDispatchFailures.Paused);
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
            IsFocused = ValidateInteractionGate(requireFocus: true).IsSuccess;
        }

        private bool IsPackageHeld()
        {
            return playerCarry != null && packageBinding != null &&
                   playerCarry.HeldItem == packageBinding.PackageItem;
        }

        private OperationResult Remember(OperationResult result)
        {
            LastFailureCode = result.IsSuccess ? string.Empty : result.Error.Code;
            return result;
        }
    }
}
