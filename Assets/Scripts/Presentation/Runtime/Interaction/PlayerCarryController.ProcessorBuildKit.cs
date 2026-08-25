using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 ProcessorBuildKitPreviewSize =
            new Vector3(0.055f, 0.0475f, 0.012f);

        [SerializeField] private ProcessorBuildKitProjection processorBuildKit;

        public ProcessorBuildKitProjection ProcessorBuildKit => processorBuildKit;

        public bool IsProcessorBuildKitMode { get; private set; }

        public ProcessorBuildKitStatus CurrentProcessorBuildKitStatus
        {
            get;
            private set;
        } = ProcessorBuildKitStatus.ContextMissing;

        public void ConfigureProcessorBuildKit(
            ProcessorBuildKitProjection projection,
            ProcessorAssemblyItemBinding assemblyBinding)
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
                    "The processor binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            processorBuildKit = projection;
            processorAssemblyBinding = assemblyBinding;
            processorBuildKit.RefreshPresentation();
        }

        public bool MatchesProcessorBuildKitConfiguration(
            ProcessorBuildKitProjection projection,
            ProcessorAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   processorBuildKit == projection &&
                   processorAssemblyBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetProcessorBuildKitMode(bool enabled)
        {
            ProcessorAssemblyItemBinding binding = GetProcessorBinding(HeldItem);
            if (HeldItem == null || binding == null || processorBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-processor-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-processor-build-kit.paused")));
            }

            if (enabled && !processorBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        processorBuildKit.HasMotherboardPrerequisite
                            ? "custom-pc-processor-build-kit.authority-blocked"
                            : "custom-pc-processor-build-kit.prerequisite-missing")));
            }

            SetProcessorBuildKitMode(enabled);
            if (enabled)
            {
                UpdateProcessorBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateProcessorBuildKitPreviewClockwise()
        {
            ProcessorAssemblyItemBinding binding = GetProcessorBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-processor-build-kit.nothing-held")));
            }

            if (!IsProcessorBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-processor-build-kit.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 4;
            LastFailureCode = string.Empty;
            UpdateProcessorBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmProcessorBuildKit()
        {
            ProcessorAssemblyItemBinding binding = GetProcessorBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-processor-build-kit.nothing-held")));
            }

            if (!IsProcessorBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-processor-build-kit.mode-inactive")));
            }

            ProcessorBuildKitEvaluation evaluation =
                EvaluateProcessorBuildKit(binding);
            ApplyProcessorBuildKitEvaluation(evaluation);
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
                processorBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        /// <summary>
        /// Owns all held-processor gameplay presses. This single arbiter prevents the
        /// Build Kit and the legacy processor socket from consuming the same frame.
        /// </summary>
        private bool ProcessHeldProcessorInput()
        {
            ProcessorAssemblyItemBinding binding = GetProcessorBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            ProcessorBuildKitEvaluation buildKitEvaluation =
                EvaluateProcessorBuildKit(binding);
            bool buildKitOwnsPrimary =
                IsProcessorBuildKitMode ||
                (processorBuildKit != null &&
                 processorBuildKit.HasPickupReceipt);

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (buildKitOwnsPrimary)
                {
                    TrySetProcessorBuildKitMode(!IsProcessorBuildKitMode);
                }
                else
                {
                    TrySetProcessorSeatMode(!IsProcessorSeatMode);
                }

                return true;
            }

            if (IsProcessorBuildKitMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateProcessorBuildKitPreviewClockwise();
                return true;
            }

            if (IsProcessorSeatMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateProcessorSeatPreviewClockwise();
                return true;
            }

            if (IsProcessorBuildKitMode)
            {
                ApplyProcessorBuildKitEvaluation(buildKitEvaluation);
            }
            else
            {
                UpdateProcessorSeatPreview(binding);
            }

            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                if (IsProcessorBuildKitMode)
                {
                    TryConfirmProcessorBuildKit();
                }
                else if (IsProcessorSeatMode)
                {
                    ProcessorSocketEvaluation seatEvaluation =
                        EvaluateProcessorSeat(binding);
                    ApplyProcessorSeatEvaluation(seatEvaluation);
                    TryConfirmProcessorSeat(binding, seatEvaluation);
                }
                else
                {
                    TryDrop();
                }
            }

            return true;
        }

        private ProcessorBuildKitEvaluation EvaluateProcessorBuildKit(
            ProcessorAssemblyItemBinding binding)
        {
            if (processorBuildKit == null)
            {
                return new ProcessorBuildKitEvaluation(
                    ProcessorBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return processorBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    processorBuildKit.HasPickupReceipt);
        }

        private void UpdateProcessorBuildKitPreview(
            ProcessorAssemblyItemBinding binding)
        {
            if (!IsProcessorBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                processorBuildKit?.ResetFeedback();
                return;
            }

            ApplyProcessorBuildKitEvaluation(
                EvaluateProcessorBuildKit(binding));
        }

        private void ApplyProcessorBuildKitEvaluation(
            ProcessorBuildKitEvaluation evaluation)
        {
            CurrentProcessorBuildKitStatus = evaluation.Status;
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
                        processorBuildKit != null
                            ? processorBuildKit.Surface
                            : null),
                    ProcessorBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetProcessorBuildKitMode(bool enabled)
        {
            IsProcessorBuildKitMode = enabled &&
                                      HeldItem != null &&
                                      GetProcessorBinding(HeldItem) != null &&
                                      processorBuildKit != null;
            IsProcessorSeatMode = false;
            IsMotherboardSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            ResetMotherboardBuildKitState();
            ResetMemoryModuleBuildKitState();
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentProcessorBuildKitStatus =
                ProcessorBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            processorSocket?.ResetFeedback();
            if (!IsProcessorBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                processorBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetProcessorBuildKitState()
        {
            IsProcessorBuildKitMode = false;
            CurrentProcessorBuildKitStatus =
                ProcessorBuildKitStatus.ContextMissing;
            processorBuildKit?.ResetFeedback();
        }

        private string GetHeldProcessorBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = processorBuildKit?.StagedComponentCount ?? 1;
            if (!IsProcessorBuildKitMode)
            {
                return $"{placement}: CPU Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetProcessorBuildKitStatusLabel(
                CurrentProcessorBuildKitStatus);
            return PlacementValid
                ? $"[OK] CPU BUILD KIT HİZALI • {drop}: işlemciyi yerleştir • " +
                  $"{rotate}: 90° döndür • {placement}: çık • {staged}/10 → 2/10"
                : $"[X] {status} • {rotate}: 90° döndür • {placement}: çık";
        }

        private static string GetProcessorBuildKitStatusLabel(
            ProcessorBuildKitStatus status)
        {
            return status switch
            {
                ProcessorBuildKitStatus.Valid => "CPU BUILD KIT HAZIR",
                ProcessorBuildKitStatus.PrerequisiteMissing => "ÖNCE ANAKARTI HAZIRLA",
                ProcessorBuildKitStatus.OutOfRange => "CPU BUILD KIT'E YAKLAŞ",
                ProcessorBuildKitStatus.NotFocused => "CPU TEPSİSİNİ HEDEFLE",
                ProcessorBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                ProcessorBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                ProcessorBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                ProcessorBuildKitStatus.Obstructed => "CPU TEPSİSİ DOLU",
                ProcessorBuildKitStatus.AlreadyStaged => "İŞLEMCİ ZATEN HAZIR",
                ProcessorBuildKitStatus.Paused => "DURAKLATILDI",
                ProcessorBuildKitStatus.AuthorityBlocked => "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
