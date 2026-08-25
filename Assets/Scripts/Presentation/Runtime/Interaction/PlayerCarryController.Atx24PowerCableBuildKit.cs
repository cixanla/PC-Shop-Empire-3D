using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 Atx24PowerCableBuildKitPreviewSize =
            new Vector3(0.135f, 0.125f, 0.060f);

        [SerializeField]
        private Atx24PowerCableBuildKitProjection atx24PowerCableBuildKit;

        public Atx24PowerCableBuildKitProjection Atx24PowerCableBuildKit =>
            atx24PowerCableBuildKit;

        public bool IsAtx24PowerCableBuildKitMode { get; private set; }

        public Atx24PowerCableBuildKitStatus CurrentAtx24PowerCableBuildKitStatus
        {
            get;
            private set;
        } = Atx24PowerCableBuildKitStatus.ContextMissing;

        public void ConfigureAtx24PowerCableBuildKit(
            Atx24PowerCableBuildKitProjection projection,
            Atx24PowerCableAssemblyItemBinding assemblyBinding)
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
                    "The atx24-power-cable binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            atx24PowerCableBuildKit = projection;
            atx24PowerCableBinding = assemblyBinding;
            atx24PowerCableBuildKit.RefreshPresentation();
        }

        public bool MatchesAtx24PowerCableBuildKitConfiguration(
            Atx24PowerCableBuildKitProjection projection,
            Atx24PowerCableAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   atx24PowerCableBuildKit == projection &&
                   atx24PowerCableBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetAtx24PowerCableBuildKitMode(bool enabled)
        {
            Atx24PowerCableAssemblyItemBinding binding =
                GetAtx24PowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null ||
                atx24PowerCableBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-atx24-power-cable-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-atx24-power-cable-build-kit.paused")));
            }

            if (enabled && !atx24PowerCableBuildKit.HasPickupReceipt)
            {
                bool prerequisitesReady =
                    atx24PowerCableBuildKit.HasMotherboardPrerequisite &&
                    atx24PowerCableBuildKit.HasProcessorPrerequisite &&
                    atx24PowerCableBuildKit.HasMemoryModulePrerequisite &&
                    atx24PowerCableBuildKit.HasStoragePrerequisite &&
                    atx24PowerCableBuildKit.HasProcessorCoolerPrerequisite &&
                    atx24PowerCableBuildKit.HasGraphicsCardPrerequisite &&
                    atx24PowerCableBuildKit.HasPowerSupplyPrerequisite;
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        prerequisitesReady
                            ? "custom-pc-atx24-power-cable-build-kit.authority-blocked"
                            : "custom-pc-atx24-power-cable-build-kit.prerequisite-missing")));
            }

            SetAtx24PowerCableBuildKitMode(enabled);
            if (enabled)
            {
                UpdateAtx24PowerCableBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateAtx24PowerCableBuildKitPreviewClockwise()
        {
            Atx24PowerCableAssemblyItemBinding binding =
                GetAtx24PowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-atx24-power-cable-build-kit.nothing-held")));
            }

            if (!IsAtx24PowerCableBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-atx24-power-cable-build-kit.mode-inactive")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-atx24-power-cable-build-kit.paused")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateAtx24PowerCableBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmAtx24PowerCableBuildKit()
        {
            Atx24PowerCableAssemblyItemBinding binding =
                GetAtx24PowerCableBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-atx24-power-cable-build-kit.nothing-held")));
            }

            if (!IsAtx24PowerCableBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-atx24-power-cable-build-kit.mode-inactive")));
            }

            Atx24PowerCableBuildKitEvaluation evaluation =
                EvaluateAtx24PowerCableBuildKit(binding);
            ApplyAtx24PowerCableBuildKitEvaluation(evaluation);
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
                atx24PowerCableBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private Atx24PowerCableBuildKitEvaluation EvaluateAtx24PowerCableBuildKit(
            Atx24PowerCableAssemblyItemBinding binding)
        {
            if (atx24PowerCableBuildKit == null)
            {
                return new Atx24PowerCableBuildKitEvaluation(
                    Atx24PowerCableBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return atx24PowerCableBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsRouted &&
                    atx24PowerCableBuildKit.HasPickupReceipt);
        }

        private void UpdateAtx24PowerCableBuildKitPreview(
            Atx24PowerCableAssemblyItemBinding binding)
        {
            if (!IsAtx24PowerCableBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                atx24PowerCableBuildKit?.ResetFeedback();
                return;
            }

            ApplyAtx24PowerCableBuildKitEvaluation(
                EvaluateAtx24PowerCableBuildKit(binding));
        }

        private void ApplyAtx24PowerCableBuildKitEvaluation(
            Atx24PowerCableBuildKitEvaluation evaluation)
        {
            CurrentAtx24PowerCableBuildKitStatus = evaluation.Status;
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
                        atx24PowerCableBuildKit != null
                            ? atx24PowerCableBuildKit.Surface
                            : null),
                    Atx24PowerCableBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetAtx24PowerCableBuildKitMode(bool enabled)
        {
            IsAtx24PowerCableBuildKitMode = enabled &&
                                        HeldItem != null &&
                                        GetAtx24PowerCableBinding(HeldItem) != null &&
                                        atx24PowerCableBuildKit != null;
            IsM2StorageSeatMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsAtx24PowerCableRouteMode = false;
            atx24PowerCableRoute?.SetRouteModeActive(active: false);
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            ResetEps12vPowerCableState();
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
            CurrentAtx24PowerCableBuildKitStatus =
                Atx24PowerCableBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            atx24PowerCableRoute?.ResetFeedback();
            if (!IsAtx24PowerCableBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                atx24PowerCableBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetAtx24PowerCableBuildKitState()
        {
            IsAtx24PowerCableBuildKitMode = false;
            CurrentAtx24PowerCableBuildKitStatus =
                Atx24PowerCableBuildKitStatus.ContextMissing;
            atx24PowerCableBuildKit?.ResetFeedback();
        }

        private string GetHeldAtx24PowerCableBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = atx24PowerCableBuildKit?.StagedComponentCount ?? 7;
            if (!IsAtx24PowerCableBuildKitMode)
            {
                return $"{placement}: ATX24'ü Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetAtx24PowerCableBuildKitStatusLabel(
                CurrentAtx24PowerCableBuildKitStatus);
            return PlacementValid
                ? $"[OK] ATX24 BUILD KIT HİZALI • {drop}: ATX24'ü yerleştir • " +
                  $"{rotate}: 180° döndür • {placement}: çık • {staged}/10 → 8/10"
                : $"[X] {status} • {rotate}: 180° döndür • {placement}: çık";
        }

        private static string GetAtx24PowerCableBuildKitStatusLabel(
            Atx24PowerCableBuildKitStatus status)
        {
            return status switch
            {
                Atx24PowerCableBuildKitStatus.Valid => "ATX24 BUILD KIT HAZIR",
                Atx24PowerCableBuildKitStatus.PrerequisiteMissing =>
                    "ÖNCE ANAKART, İŞLEMCİ, BELLEK, NVMe, SOĞUTUCU, GPU VE PSU'YU HAZIRLA",
                Atx24PowerCableBuildKitStatus.OutOfRange =>
                    "ATX24 BUILD KIT'E YAKLAŞ",
                Atx24PowerCableBuildKitStatus.NotFocused =>
                    "ATX24 TEPSİSİNİ HEDEFLE",
                Atx24PowerCableBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                Atx24PowerCableBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                Atx24PowerCableBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                Atx24PowerCableBuildKitStatus.Obstructed => "ATX24 TEPSİSİ DOLU",
                Atx24PowerCableBuildKitStatus.AlreadyStaged =>
                    "ATX24 ZATEN HAZIR",
                Atx24PowerCableBuildKitStatus.Paused => "DURAKLATILDI",
                Atx24PowerCableBuildKitStatus.AuthorityBlocked =>
                    "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
