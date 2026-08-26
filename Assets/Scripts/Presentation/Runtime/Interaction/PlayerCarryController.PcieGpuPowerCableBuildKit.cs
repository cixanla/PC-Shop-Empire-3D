using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 PcieGpuPowerCableBuildKitPreviewSize =
            new Vector3(0.135f, 0.125f, 0.060f);

        [SerializeField]
        private PcieGpuPowerCableBuildKitProjection pcieGpuPowerCableBuildKit;

        public PcieGpuPowerCableBuildKitProjection PcieGpuPowerCableBuildKit =>
            pcieGpuPowerCableBuildKit;

        public bool IsPcieGpuPowerCableBuildKitMode { get; private set; }

        public PcieGpuPowerCableBuildKitStatus CurrentPcieGpuPowerCableBuildKitStatus
        {
            get;
            private set;
        } = PcieGpuPowerCableBuildKitStatus.ContextMissing;

        public void ConfigurePcieGpuPowerCableBuildKit(
            PcieGpuPowerCableBuildKitProjection projection,
            PcieGpuPowerCableAssemblyItemBinding assemblyBinding)
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
                    "The PCIe GPU binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            pcieGpuPowerCableBuildKit = projection;
            pcieGpuPowerCableBinding = assemblyBinding;
            pcieGpuPowerCableBuildKit.RefreshPresentation();
        }

        public bool MatchesPcieGpuPowerCableBuildKitConfiguration(
            PcieGpuPowerCableBuildKitProjection projection,
            PcieGpuPowerCableAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   pcieGpuPowerCableBuildKit == projection &&
                   pcieGpuPowerCableBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetPcieGpuPowerCableBuildKitMode(bool enabled)
        {
            PcieGpuPowerCableAssemblyItemBinding binding =
                GetPcieGpuPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null ||
                pcieGpuPowerCableBuildKit == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.paused")));
            }

            if (enabled && !pcieGpuPowerCableBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    pcieGpuPowerCableBuildKit.HasAllPrerequisites
                        ? "custom-pc-pcie-gpu-power-cable-build-kit.authority-blocked"
                        : "custom-pc-pcie-gpu-power-cable-build-kit.prerequisite-missing")));
            }

            SetPcieGpuPowerCableBuildKitMode(enabled);
            if (enabled)
            {
                UpdatePcieGpuPowerCableBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotatePcieGpuPowerCableBuildKitPreviewClockwise()
        {
            PcieGpuPowerCableAssemblyItemBinding binding =
                GetPcieGpuPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.nothing-held")));
            }

            if (!IsPcieGpuPowerCableBuildKitMode)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.mode-inactive")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.paused")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdatePcieGpuPowerCableBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmPcieGpuPowerCableBuildKit()
        {
            PcieGpuPowerCableAssemblyItemBinding binding =
                GetPcieGpuPowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.nothing-held")));
            }

            if (!IsPcieGpuPowerCableBuildKitMode)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(
                    "custom-pc-pcie-gpu-power-cable-build-kit.mode-inactive")));
            }

            PcieGpuPowerCableBuildKitEvaluation evaluation =
                EvaluatePcieGpuPowerCableBuildKit(binding);
            ApplyPcieGpuPowerCableBuildKitEvaluation(evaluation);
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
                pcieGpuPowerCableBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private PcieGpuPowerCableBuildKitEvaluation EvaluatePcieGpuPowerCableBuildKit(
            PcieGpuPowerCableAssemblyItemBinding binding)
        {
            if (pcieGpuPowerCableBuildKit == null)
            {
                return new PcieGpuPowerCableBuildKitEvaluation(
                    PcieGpuPowerCableBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return pcieGpuPowerCableBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsRouted &&
                    pcieGpuPowerCableBuildKit.HasPickupReceipt);
        }

        private void UpdatePcieGpuPowerCableBuildKitPreview(
            PcieGpuPowerCableAssemblyItemBinding binding)
        {
            if (!IsPcieGpuPowerCableBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                pcieGpuPowerCableBuildKit?.ResetFeedback();
                return;
            }

            ApplyPcieGpuPowerCableBuildKitEvaluation(
                EvaluatePcieGpuPowerCableBuildKit(binding));
        }

        private void ApplyPcieGpuPowerCableBuildKitEvaluation(
            PcieGpuPowerCableBuildKitEvaluation evaluation)
        {
            CurrentPcieGpuPowerCableBuildKitStatus = evaluation.Status;
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
                        pcieGpuPowerCableBuildKit != null
                            ? pcieGpuPowerCableBuildKit.Surface
                            : null),
                    PcieGpuPowerCableBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetPcieGpuPowerCableBuildKitMode(bool enabled)
        {
            IsPcieGpuPowerCableBuildKitMode = enabled &&
                                              HeldItem != null &&
                                              GetPcieGpuPowerCableBinding(HeldItem) != null &&
                                              pcieGpuPowerCableBuildKit != null;
            IsPcieGpuPowerCableRouteMode = false;
            pcieGpuPowerCableRoute?.SetRouteModeActive(active: false);
            IsAtx24PowerCableRouteMode = false;
            atx24PowerCableRoute?.SetRouteModeActive(active: false);
            ResetAtx24PowerCableBuildKitState();
            ResetEps12vPowerCableBuildKitState();
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
            CurrentPcieGpuPowerCableBuildKitStatus =
                PcieGpuPowerCableBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            pcieGpuPowerCableRoute?.ResetFeedback();
            if (!IsPcieGpuPowerCableBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                pcieGpuPowerCableBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetPcieGpuPowerCableBuildKitState()
        {
            IsPcieGpuPowerCableBuildKitMode = false;
            CurrentPcieGpuPowerCableBuildKitStatus =
                PcieGpuPowerCableBuildKitStatus.ContextMissing;
            pcieGpuPowerCableBuildKit?.ResetFeedback();
        }

        private string GetHeldPcieGpuPowerCableBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = pcieGpuPowerCableBuildKit?.StagedComponentCount ?? 9;
            if (!IsPcieGpuPowerCableBuildKitMode)
            {
                return $"{placement}: PCIe GPU'yi Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetPcieGpuPowerCableBuildKitStatusLabel(
                CurrentPcieGpuPowerCableBuildKitStatus);
            return PlacementValid
                ? $"[OK] PCIe GPU BUILD KIT HİZALI • {drop}: PCIe GPU'yi yerleştir • " +
                  $"{rotate}: 180° döndür • {placement}: çık • {staged}/10 → 10/10"
                : $"[X] {status} • {rotate}: 180° döndür • {placement}: çık";
        }

        private static string GetPcieGpuPowerCableBuildKitStatusLabel(
            PcieGpuPowerCableBuildKitStatus status)
        {
            return status switch
            {
                PcieGpuPowerCableBuildKitStatus.Valid => "PCIe GPU BUILD KIT HAZIR",
                PcieGpuPowerCableBuildKitStatus.PrerequisiteMissing =>
                    "ÖNCE ANAKART, CPU, RAM, NVMe, SOĞUTUCU, GPU, PSU, ATX24 VE EPS12V'Yİ HAZIRLA",
                PcieGpuPowerCableBuildKitStatus.OutOfRange =>
                    "PCIe GPU BUILD KIT'E YAKLAŞ",
                PcieGpuPowerCableBuildKitStatus.NotFocused =>
                    "PCIe GPU TEPSİSİNİ HEDEFLE",
                PcieGpuPowerCableBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                PcieGpuPowerCableBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                PcieGpuPowerCableBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                PcieGpuPowerCableBuildKitStatus.Obstructed => "PCIe GPU TEPSİSİ DOLU",
                PcieGpuPowerCableBuildKitStatus.AlreadyStaged =>
                    "PCIe GPU ZATEN HAZIR",
                PcieGpuPowerCableBuildKitStatus.Paused => "DURAKLATILDI",
                PcieGpuPowerCableBuildKitStatus.AuthorityBlocked =>
                    "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
