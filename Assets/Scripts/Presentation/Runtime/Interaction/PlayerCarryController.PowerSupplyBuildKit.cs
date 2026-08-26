using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 PowerSupplyBuildKitPreviewSize =
            new Vector3(0.15f, 0.086f, 0.14f);

        [SerializeField]
        private PowerSupplyBuildKitProjection powerSupplyBuildKit;

        public PowerSupplyBuildKitProjection PowerSupplyBuildKit =>
            powerSupplyBuildKit;

        public bool IsPowerSupplyBuildKitMode { get; private set; }

        public PowerSupplyBuildKitStatus CurrentPowerSupplyBuildKitStatus
        {
            get;
            private set;
        } = PowerSupplyBuildKitStatus.ContextMissing;

        public void ConfigurePowerSupplyBuildKit(
            PowerSupplyBuildKitProjection projection,
            PowerSupplyAssemblyItemBinding assemblyBinding)
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
                    "The power-supply binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            powerSupplyBuildKit = projection;
            powerSupplyBinding = assemblyBinding;
            powerSupplyBuildKit.RefreshPresentation();
        }

        public bool MatchesPowerSupplyBuildKitConfiguration(
            PowerSupplyBuildKitProjection projection,
            PowerSupplyAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   powerSupplyBuildKit == projection &&
                   powerSupplyBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetPowerSupplyBuildKitMode(bool enabled)
        {
            PowerSupplyAssemblyItemBinding binding =
                GetPowerSupplyBinding(HeldItem);
            if (HeldItem == null || binding == null ||
                powerSupplyBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.paused")));
            }

            if (enabled && !powerSupplyBuildKit.HasPickupReceipt)
            {
                bool prerequisitesReady =
                    powerSupplyBuildKit.HasMotherboardPrerequisite &&
                    powerSupplyBuildKit.HasProcessorPrerequisite &&
                    powerSupplyBuildKit.HasMemoryModulePrerequisite &&
                    powerSupplyBuildKit.HasStoragePrerequisite &&
                    powerSupplyBuildKit.HasProcessorCoolerPrerequisite &&
                    powerSupplyBuildKit.HasGraphicsCardPrerequisite;
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        prerequisitesReady
                            ? "custom-pc-power-supply-build-kit.authority-blocked"
                            : "custom-pc-power-supply-build-kit.prerequisite-missing")));
            }

            SetPowerSupplyBuildKitMode(enabled);
            if (enabled)
            {
                UpdatePowerSupplyBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotatePowerSupplyBuildKitPreviewClockwise()
        {
            PowerSupplyAssemblyItemBinding binding =
                GetPowerSupplyBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.nothing-held")));
            }

            if (!IsPowerSupplyBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.mode-inactive")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.paused")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdatePowerSupplyBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmPowerSupplyBuildKit()
        {
            PowerSupplyAssemblyItemBinding binding =
                GetPowerSupplyBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.nothing-held")));
            }

            if (!IsPowerSupplyBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-power-supply-build-kit.mode-inactive")));
            }

            PowerSupplyBuildKitEvaluation evaluation =
                EvaluatePowerSupplyBuildKit(binding);
            ApplyPowerSupplyBuildKitEvaluation(evaluation);
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
                powerSupplyBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private PowerSupplyBuildKitEvaluation EvaluatePowerSupplyBuildKit(
            PowerSupplyAssemblyItemBinding binding)
        {
            if (powerSupplyBuildKit == null)
            {
                return new PowerSupplyBuildKitEvaluation(
                    PowerSupplyBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return powerSupplyBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    powerSupplyBuildKit.HasPickupReceipt);
        }

        private void UpdatePowerSupplyBuildKitPreview(
            PowerSupplyAssemblyItemBinding binding)
        {
            if (!IsPowerSupplyBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                powerSupplyBuildKit?.ResetFeedback();
                return;
            }

            ApplyPowerSupplyBuildKitEvaluation(
                EvaluatePowerSupplyBuildKit(binding));
        }

        private void ApplyPowerSupplyBuildKitEvaluation(
            PowerSupplyBuildKitEvaluation evaluation)
        {
            CurrentPowerSupplyBuildKitStatus = evaluation.Status;
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
                        powerSupplyBuildKit != null
                            ? powerSupplyBuildKit.Surface
                            : null),
                    PowerSupplyBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetPowerSupplyBuildKitMode(bool enabled)
        {
            IsPowerSupplyBuildKitMode = enabled &&
                                        HeldItem != null &&
                                        GetPowerSupplyBinding(HeldItem) != null &&
                                        powerSupplyBuildKit != null;
            IsM2StorageSeatMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            ResetPcieGpuPowerCableState();
            ResetMotherboardBuildKitState();
            ResetProcessorBuildKitState();
            ResetMemoryModuleBuildKitState();
            ResetStorageBuildKitState();
            ResetProcessorCoolerBuildKitState();
            ResetGraphicsCardBuildKitState();
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentPowerSupplyBuildKitStatus =
                PowerSupplyBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            powerSupplyBay?.ResetFeedback();
            if (!IsPowerSupplyBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                powerSupplyBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetPowerSupplyBuildKitState()
        {
            IsPowerSupplyBuildKitMode = false;
            CurrentPowerSupplyBuildKitStatus =
                PowerSupplyBuildKitStatus.ContextMissing;
            powerSupplyBuildKit?.ResetFeedback();
        }

        private string GetHeldPowerSupplyBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = powerSupplyBuildKit?.StagedComponentCount ?? 6;
            if (!IsPowerSupplyBuildKitMode)
            {
                return $"{placement}: PSU'yu Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetPowerSupplyBuildKitStatusLabel(
                CurrentPowerSupplyBuildKitStatus);
            return PlacementValid
                ? $"[OK] PSU BUILD KIT HİZALI • {drop}: PSU'yu yerleştir • " +
                  $"{rotate}: 180° döndür • {placement}: çık • {staged}/10 → 7/10"
                : $"[X] {status} • {rotate}: 180° döndür • {placement}: çık";
        }

        private static string GetPowerSupplyBuildKitStatusLabel(
            PowerSupplyBuildKitStatus status)
        {
            return status switch
            {
                PowerSupplyBuildKitStatus.Valid => "PSU BUILD KIT HAZIR",
                PowerSupplyBuildKitStatus.PrerequisiteMissing =>
                    "ÖNCE ANAKART, İŞLEMCİ, BELLEK, NVMe, SOĞUTUCU VE GPU'YU HAZIRLA",
                PowerSupplyBuildKitStatus.OutOfRange =>
                    "PSU BUILD KIT'E YAKLAŞ",
                PowerSupplyBuildKitStatus.NotFocused =>
                    "PSU TEPSİSİNİ HEDEFLE",
                PowerSupplyBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                PowerSupplyBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                PowerSupplyBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                PowerSupplyBuildKitStatus.Obstructed => "PSU TEPSİSİ DOLU",
                PowerSupplyBuildKitStatus.AlreadyStaged =>
                    "PSU ZATEN HAZIR",
                PowerSupplyBuildKitStatus.Paused => "DURAKLATILDI",
                PowerSupplyBuildKitStatus.AuthorityBlocked =>
                    "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
