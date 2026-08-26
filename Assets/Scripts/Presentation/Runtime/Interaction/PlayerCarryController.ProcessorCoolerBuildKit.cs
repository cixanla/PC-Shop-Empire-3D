using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 ProcessorCoolerBuildKitPreviewSize =
            new Vector3(0.118f, 0.080f, 0.118f);

        [SerializeField]
        private ProcessorCoolerBuildKitProjection processorCoolerBuildKit;

        public ProcessorCoolerBuildKitProjection ProcessorCoolerBuildKit =>
            processorCoolerBuildKit;

        public bool IsProcessorCoolerBuildKitMode { get; private set; }

        public ProcessorCoolerBuildKitStatus CurrentProcessorCoolerBuildKitStatus
        {
            get;
            private set;
        } = ProcessorCoolerBuildKitStatus.ContextMissing;

        public void ConfigureProcessorCoolerBuildKit(
            ProcessorCoolerBuildKitProjection projection,
            ProcessorCoolerAssemblyItemBinding assemblyBinding)
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
                    "The processor-cooler binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            processorCoolerBuildKit = projection;
            processorCoolerBinding = assemblyBinding;
            processorCoolerBuildKit.RefreshPresentation();
        }

        public bool MatchesProcessorCoolerBuildKitConfiguration(
            ProcessorCoolerBuildKitProjection projection,
            ProcessorCoolerAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   processorCoolerBuildKit == projection &&
                   processorCoolerBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetProcessorCoolerBuildKitMode(bool enabled)
        {
            ProcessorCoolerAssemblyItemBinding binding =
                GetProcessorCoolerBinding(HeldItem);
            if (HeldItem == null || binding == null ||
                processorCoolerBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.paused")));
            }

            if (enabled && !processorCoolerBuildKit.HasPickupReceipt)
            {
                bool prerequisitesReady =
                    processorCoolerBuildKit.HasMotherboardPrerequisite &&
                    processorCoolerBuildKit.HasProcessorPrerequisite &&
                    processorCoolerBuildKit.HasMemoryModulePrerequisite &&
                    processorCoolerBuildKit.HasStoragePrerequisite;
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        prerequisitesReady
                            ? "custom-pc-processor-cooler-build-kit.authority-blocked"
                            : "custom-pc-processor-cooler-build-kit.prerequisite-missing")));
            }

            SetProcessorCoolerBuildKitMode(enabled);
            if (enabled)
            {
                UpdateProcessorCoolerBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateProcessorCoolerBuildKitPreviewClockwise()
        {
            ProcessorCoolerAssemblyItemBinding binding =
                GetProcessorCoolerBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.nothing-held")));
            }

            if (!IsProcessorCoolerBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.mode-inactive")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.paused")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 4;
            LastFailureCode = string.Empty;
            UpdateProcessorCoolerBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmProcessorCoolerBuildKit()
        {
            ProcessorCoolerAssemblyItemBinding binding =
                GetProcessorCoolerBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.nothing-held")));
            }

            if (!IsProcessorCoolerBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-processor-cooler-build-kit.mode-inactive")));
            }

            ProcessorCoolerBuildKitEvaluation evaluation =
                EvaluateProcessorCoolerBuildKit(binding);
            ApplyProcessorCoolerBuildKitEvaluation(evaluation);
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
                processorCoolerBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private ProcessorCoolerBuildKitEvaluation EvaluateProcessorCoolerBuildKit(
            ProcessorCoolerAssemblyItemBinding binding)
        {
            if (processorCoolerBuildKit == null)
            {
                return new ProcessorCoolerBuildKitEvaluation(
                    ProcessorCoolerBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return processorCoolerBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    processorCoolerBuildKit.HasPickupReceipt);
        }

        private void UpdateProcessorCoolerBuildKitPreview(
            ProcessorCoolerAssemblyItemBinding binding)
        {
            if (!IsProcessorCoolerBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                processorCoolerBuildKit?.ResetFeedback();
                return;
            }

            ApplyProcessorCoolerBuildKitEvaluation(
                EvaluateProcessorCoolerBuildKit(binding));
        }

        private void ApplyProcessorCoolerBuildKitEvaluation(
            ProcessorCoolerBuildKitEvaluation evaluation)
        {
            CurrentProcessorCoolerBuildKitStatus = evaluation.Status;
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
                        processorCoolerBuildKit != null
                            ? processorCoolerBuildKit.Surface
                            : null),
                    ProcessorCoolerBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetProcessorCoolerBuildKitMode(bool enabled)
        {
            IsProcessorCoolerBuildKitMode = enabled &&
                                                HeldItem != null &&
                                                GetProcessorCoolerBinding(HeldItem) != null &&
                                                processorCoolerBuildKit != null;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            ResetMotherboardBuildKitState();
            ResetProcessorBuildKitState();
            ResetMemoryModuleBuildKitState();
            ResetStorageBuildKitState();
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentProcessorCoolerBuildKitStatus =
                ProcessorCoolerBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            processorCoolerSlot?.ResetFeedback();
            if (!IsProcessorCoolerBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                processorCoolerBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetProcessorCoolerBuildKitState()
        {
            IsProcessorCoolerBuildKitMode = false;
            CurrentProcessorCoolerBuildKitStatus =
                ProcessorCoolerBuildKitStatus.ContextMissing;
            processorCoolerBuildKit?.ResetFeedback();
        }

        private string GetHeldProcessorCoolerBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = processorCoolerBuildKit?.StagedComponentCount ?? 4;
            if (!IsProcessorCoolerBuildKitMode)
            {
                return $"{placement}: CPU soğutucusunu Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetProcessorCoolerBuildKitStatusLabel(
                CurrentProcessorCoolerBuildKitStatus);
            return PlacementValid
                ? $"[OK] SOĞUTUCU BUILD KIT HİZALI • {drop}: soğutucuyu yerleştir • " +
                  $"{rotate}: 90° döndür • {placement}: çık • {staged}/10 → 5/10"
                : $"[X] {status} • {rotate}: 90° döndür • {placement}: çık";
        }

        private static string GetProcessorCoolerBuildKitStatusLabel(
            ProcessorCoolerBuildKitStatus status)
        {
            return status switch
            {
                ProcessorCoolerBuildKitStatus.Valid => "SOĞUTUCU BUILD KIT HAZIR",
                ProcessorCoolerBuildKitStatus.PrerequisiteMissing =>
                    "ÖNCE ANAKART, İŞLEMCİ, BELLEK VE NVMe'Yİ HAZIRLA",
                ProcessorCoolerBuildKitStatus.OutOfRange =>
                    "SOĞUTUCU BUILD KIT'E YAKLAŞ",
                ProcessorCoolerBuildKitStatus.NotFocused =>
                    "SOĞUTUCU TEPSİSİNİ HEDEFLE",
                ProcessorCoolerBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                ProcessorCoolerBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                ProcessorCoolerBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                ProcessorCoolerBuildKitStatus.Obstructed => "SOĞUTUCU TEPSİSİ DOLU",
                ProcessorCoolerBuildKitStatus.AlreadyStaged =>
                    "SOĞUTUCU ZATEN HAZIR",
                ProcessorCoolerBuildKitStatus.Paused => "DURAKLATILDI",
                ProcessorCoolerBuildKitStatus.AuthorityBlocked =>
                    "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
