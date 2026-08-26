using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 Eps12vPowerCableBuildKitPreviewSize =
            new Vector3(0.135f, 0.125f, 0.060f);

        [SerializeField]
        private Eps12vPowerCableBuildKitProjection eps12vPowerCableBuildKit;

        public Eps12vPowerCableBuildKitProjection Eps12vPowerCableBuildKit =>
            eps12vPowerCableBuildKit;

        public bool IsEps12vPowerCableBuildKitMode { get; private set; }

        public Eps12vPowerCableBuildKitStatus CurrentEps12vPowerCableBuildKitStatus
        {
            get;
            private set;
        } = Eps12vPowerCableBuildKitStatus.ContextMissing;

        public void ConfigureEps12vPowerCableBuildKit(
            Eps12vPowerCableBuildKitProjection projection,
            Eps12vPowerCableAssemblyItemBinding assemblyBinding)
        {
            if (projection == null)
            {
                throw new ArgumentNullException(nameof(projection));
            }

            if (assemblyBinding == null)
            {
                throw new ArgumentNullException(nameof(assemblyBinding));
            }

            if (!assemblyBinding.MatchesBuildKitConfiguration(projection))
            {
                throw new ArgumentException(
                    "The EPS12V binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            eps12vPowerCableBuildKit = projection;
            eps12vPowerCableBinding = assemblyBinding;
            eps12vPowerCableBuildKit.RefreshPresentation();
        }

        public bool MatchesEps12vPowerCableBuildKitConfiguration(
            Eps12vPowerCableBuildKitProjection projection,
            Eps12vPowerCableAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   eps12vPowerCableBuildKit == projection &&
                   eps12vPowerCableBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetEps12vPowerCableBuildKitMode(bool enabled)
        {
            Eps12vPowerCableAssemblyItemBinding binding =
                GetEps12vPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null ||
                eps12vPowerCableBuildKit == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-eps12v-power-cable-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-eps12v-power-cable-build-kit.paused")));
            }

            if (enabled && !eps12vPowerCableBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    eps12vPowerCableBuildKit.HasAllPrerequisites
                        ? "custom-pc-eps12v-power-cable-build-kit.authority-blocked"
                        : "custom-pc-eps12v-power-cable-build-kit.prerequisite-missing")));
            }

            SetEps12vPowerCableBuildKitMode(enabled);
            if (enabled)
            {
                UpdateEps12vPowerCableBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateEps12vPowerCableBuildKitPreviewClockwise()
        {
            Eps12vPowerCableAssemblyItemBinding binding =
                GetEps12vPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-eps12v-power-cable-build-kit.nothing-held")));
            }

            if (!IsEps12vPowerCableBuildKitMode)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-eps12v-power-cable-build-kit.mode-inactive")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-eps12v-power-cable-build-kit.paused")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateEps12vPowerCableBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmEps12vPowerCableBuildKit()
        {
            Eps12vPowerCableAssemblyItemBinding binding =
                GetEps12vPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-eps12v-power-cable-build-kit.nothing-held")));
            }

            if (!IsEps12vPowerCableBuildKitMode)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-eps12v-power-cable-build-kit.mode-inactive")));
            }

            Eps12vPowerCableBuildKitEvaluation evaluation =
                EvaluateEps12vPowerCableBuildKit(binding);
            ApplyEps12vPowerCableBuildKitEvaluation(evaluation);
            if (!evaluation.IsValid)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult placement = binding.TryPlaceInBuildKit(
                resolver != null ? resolver.Origin : null,
                transform,
                carryAnchor,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused);
            if (placement.IsSuccess)
            {
                CompleteHeldItemRelease();
                eps12vPowerCableBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private Eps12vPowerCableBuildKitEvaluation EvaluateEps12vPowerCableBuildKit(
            Eps12vPowerCableAssemblyItemBinding binding)
        {
            if (eps12vPowerCableBuildKit == null)
            {
                return new Eps12vPowerCableBuildKitEvaluation(
                    Eps12vPowerCableBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return eps12vPowerCableBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsRouted &&
                    eps12vPowerCableBuildKit.HasPickupReceipt);
        }

        private void UpdateEps12vPowerCableBuildKitPreview(
            Eps12vPowerCableAssemblyItemBinding binding)
        {
            if (!IsEps12vPowerCableBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                eps12vPowerCableBuildKit?.ResetFeedback();
                return;
            }

            ApplyEps12vPowerCableBuildKitEvaluation(
                EvaluateEps12vPowerCableBuildKit(binding));
        }

        private void ApplyEps12vPowerCableBuildKitEvaluation(
            Eps12vPowerCableBuildKitEvaluation evaluation)
        {
            CurrentEps12vPowerCableBuildKitStatus = evaluation.Status;
            PlacementValid = evaluation.IsValid;
            CurrentPlacementStatus = evaluation.IsValid
                ? PlacementStatus.Valid
                : PlacementStatus.Blocked;
            CurrentStackSupport = null;
            LastFailureCode = evaluation.IsValid
                ? string.Empty
                : evaluation.FailureCode;

            if (evaluation.HasPose && HeldItem != null)
            {
                placementPreview?.Show(
                    HeldItem,
                    new PlacementEvaluation(
                        evaluation.IsValid
                            ? PlacementStatus.Valid
                            : PlacementStatus.Blocked,
                        evaluation.Pose,
                        true,
                        null,
                        eps12vPowerCableBuildKit != null
                            ? eps12vPowerCableBuildKit.Surface
                            : null),
                    Eps12vPowerCableBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetEps12vPowerCableBuildKitMode(bool enabled)
        {
            IsEps12vPowerCableBuildKitMode = enabled &&
                                              HeldItem != null &&
                                              GetEps12vPowerCableBinding(HeldItem) != null &&
                                              eps12vPowerCableBuildKit != null;
            IsEps12vPowerCableRouteMode = false;
            eps12vPowerCableRoute?.SetRouteModeActive(active: false);
            IsAtx24PowerCableRouteMode = false;
            atx24PowerCableRoute?.SetRouteModeActive(active: false);
            ResetAtx24PowerCableBuildKitState();
            ResetPcieGpuPowerCableState();
            ResetMotherboardBuildKitState();
            ResetProcessorBuildKitState();
            ResetMemoryModuleBuildKitState();
            ResetStorageBuildKitState();
            ResetProcessorCoolerBuildKitState();
            ResetGraphicsCardBuildKitState();
            ResetPowerSupplyBuildKitState();
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentEps12vPowerCableBuildKitStatus =
                Eps12vPowerCableBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            eps12vPowerCableRoute?.ResetFeedback();
            if (!IsEps12vPowerCableBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                eps12vPowerCableBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetEps12vPowerCableBuildKitState()
        {
            IsEps12vPowerCableBuildKitMode = false;
            CurrentEps12vPowerCableBuildKitStatus =
                Eps12vPowerCableBuildKitStatus.ContextMissing;
            eps12vPowerCableBuildKit?.ResetFeedback();
        }

        private string GetHeldEps12vPowerCableBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = eps12vPowerCableBuildKit?.StagedComponentCount ?? 8;
            if (!IsEps12vPowerCableBuildKitMode)
            {
                return $"{placement}: EPS12V'yi Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetEps12vPowerCableBuildKitStatusLabel(
                CurrentEps12vPowerCableBuildKitStatus);
            return PlacementValid
                ? $"[OK] EPS12V BUILD KIT HİZALI • {drop}: EPS12V'yi yerleştir • " +
                  $"{rotate}: 180° döndür • {placement}: çık • {staged}/10 → 9/10"
                : $"[X] {status} • {rotate}: 180° döndür • {placement}: çık";
        }

        private static string GetEps12vPowerCableBuildKitStatusLabel(
            Eps12vPowerCableBuildKitStatus status)
        {
            return status switch
            {
                Eps12vPowerCableBuildKitStatus.Valid => "EPS12V BUILD KIT HAZIR",
                Eps12vPowerCableBuildKitStatus.PrerequisiteMissing =>
                    "ÖNCE ANAKART, CPU, RAM, NVMe, SOĞUTUCU, GPU, PSU VE ATX24'Ü HAZIRLA",
                Eps12vPowerCableBuildKitStatus.OutOfRange =>
                    "EPS12V BUILD KIT'E YAKLAŞ",
                Eps12vPowerCableBuildKitStatus.NotFocused =>
                    "EPS12V TEPSİSİNİ HEDEFLE",
                Eps12vPowerCableBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                Eps12vPowerCableBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                Eps12vPowerCableBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                Eps12vPowerCableBuildKitStatus.Obstructed => "EPS12V TEPSİSİ DOLU",
                Eps12vPowerCableBuildKitStatus.AlreadyStaged =>
                    "EPS12V ZATEN HAZIR",
                Eps12vPowerCableBuildKitStatus.Paused => "DURAKLATILDI",
                Eps12vPowerCableBuildKitStatus.AuthorityBlocked =>
                    "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
