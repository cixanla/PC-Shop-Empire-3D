using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 MemoryModuleBuildKitPreviewSize =
            new Vector3(0.132f, 0.032f, 0.008f);

        [SerializeField] private MemoryModuleBuildKitProjection memoryModuleBuildKit;

        public MemoryModuleBuildKitProjection MemoryModuleBuildKit => memoryModuleBuildKit;

        public bool IsMemoryModuleBuildKitMode { get; private set; }

        public MemoryModuleBuildKitStatus CurrentMemoryModuleBuildKitStatus
        {
            get;
            private set;
        } = MemoryModuleBuildKitStatus.ContextMissing;

        public void ConfigureMemoryModuleBuildKit(
            MemoryModuleBuildKitProjection projection,
            DimmAssemblyItemBinding assemblyBinding)
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
                    "The memory-module binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            memoryModuleBuildKit = projection;
            dimmAssemblyBinding = assemblyBinding;
            memoryModuleBuildKit.RefreshPresentation();
        }

        public bool MatchesMemoryModuleBuildKitConfiguration(
            MemoryModuleBuildKitProjection projection,
            DimmAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   memoryModuleBuildKit == projection &&
                   dimmAssemblyBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetMemoryModuleBuildKitMode(bool enabled)
        {
            DimmAssemblyItemBinding binding = GetDimmBinding(HeldItem);
            if (HeldItem == null || binding == null || memoryModuleBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-memory-module-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-memory-module-build-kit.paused")));
            }

            if (enabled && !memoryModuleBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        memoryModuleBuildKit.HasMotherboardPrerequisite &&
                        memoryModuleBuildKit.HasProcessorPrerequisite
                            ? "custom-pc-memory-module-build-kit.authority-blocked"
                            : "custom-pc-memory-module-build-kit.prerequisite-missing")));
            }

            SetMemoryModuleBuildKitMode(enabled);
            if (enabled)
            {
                UpdateMemoryModuleBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateMemoryModuleBuildKitPreviewClockwise()
        {
            DimmAssemblyItemBinding binding = GetDimmBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-memory-module-build-kit.nothing-held")));
            }

            if (!IsMemoryModuleBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-memory-module-build-kit.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateMemoryModuleBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmMemoryModuleBuildKit()
        {
            DimmAssemblyItemBinding binding = GetDimmBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-memory-module-build-kit.nothing-held")));
            }

            if (!IsMemoryModuleBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-memory-module-build-kit.mode-inactive")));
            }

            MemoryModuleBuildKitEvaluation evaluation =
                EvaluateMemoryModuleBuildKit(binding);
            ApplyMemoryModuleBuildKitEvaluation(evaluation);
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
                memoryModuleBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        /// <summary>
        /// Owns all held-memory-module gameplay presses. This single arbiter prevents the
        /// Build Kit and the legacy DIMM socket from consuming the same frame.
        /// </summary>
        private bool ProcessHeldMemoryModuleInput()
        {
            DimmAssemblyItemBinding binding = GetDimmBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            MemoryModuleBuildKitEvaluation buildKitEvaluation =
                EvaluateMemoryModuleBuildKit(binding);
            bool buildKitOwnsPrimary =
                IsMemoryModuleBuildKitMode ||
                (memoryModuleBuildKit != null &&
                 memoryModuleBuildKit.HasPickupReceipt);

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (buildKitOwnsPrimary)
                {
                    TrySetMemoryModuleBuildKitMode(!IsMemoryModuleBuildKitMode);
                }
                else
                {
                    TrySetDimmSeatMode(!IsDimmSeatMode);
                }

                return true;
            }

            if (IsMemoryModuleBuildKitMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateMemoryModuleBuildKitPreviewClockwise();
                return true;
            }

            if (IsDimmSeatMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateDimmSeatPreviewClockwise();
                return true;
            }

            if (IsMemoryModuleBuildKitMode)
            {
                ApplyMemoryModuleBuildKitEvaluation(buildKitEvaluation);
            }
            else
            {
                UpdateDimmSeatPreview(binding);
            }

            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                if (IsMemoryModuleBuildKitMode)
                {
                    TryConfirmMemoryModuleBuildKit();
                }
                else if (IsDimmSeatMode)
                {
                    DimmSlotEvaluation seatEvaluation =
                        EvaluateDimmSeat(binding);
                    ApplyDimmSeatEvaluation(seatEvaluation);
                    TryConfirmDimmSeat(binding, seatEvaluation);
                }
                else
                {
                    TryDrop();
                }
            }

            return true;
        }

        private MemoryModuleBuildKitEvaluation EvaluateMemoryModuleBuildKit(
            DimmAssemblyItemBinding binding)
        {
            if (memoryModuleBuildKit == null)
            {
                return new MemoryModuleBuildKitEvaluation(
                    MemoryModuleBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return memoryModuleBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    memoryModuleBuildKit.HasPickupReceipt);
        }

        private void UpdateMemoryModuleBuildKitPreview(
            DimmAssemblyItemBinding binding)
        {
            if (!IsMemoryModuleBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                memoryModuleBuildKit?.ResetFeedback();
                return;
            }

            ApplyMemoryModuleBuildKitEvaluation(
                EvaluateMemoryModuleBuildKit(binding));
        }

        private void ApplyMemoryModuleBuildKitEvaluation(
            MemoryModuleBuildKitEvaluation evaluation)
        {
            CurrentMemoryModuleBuildKitStatus = evaluation.Status;
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
                        memoryModuleBuildKit != null
                            ? memoryModuleBuildKit.Surface
                            : null),
                    MemoryModuleBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetMemoryModuleBuildKitMode(bool enabled)
        {
            IsMemoryModuleBuildKitMode = enabled &&
                                      HeldItem != null &&
                                      GetDimmBinding(HeldItem) != null &&
                                      memoryModuleBuildKit != null;
            IsDimmSeatMode = false;
            IsMotherboardSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            ResetMotherboardBuildKitState();
            ResetProcessorBuildKitState();
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentMemoryModuleBuildKitStatus =
                MemoryModuleBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            dimmSlot?.ResetFeedback();
            if (!IsMemoryModuleBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                memoryModuleBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetMemoryModuleBuildKitState()
        {
            IsMemoryModuleBuildKitMode = false;
            CurrentMemoryModuleBuildKitStatus =
                MemoryModuleBuildKitStatus.ContextMissing;
            memoryModuleBuildKit?.ResetFeedback();
        }

        private string GetHeldMemoryModuleBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = memoryModuleBuildKit?.StagedComponentCount ?? 2;
            if (!IsMemoryModuleBuildKitMode)
            {
                return $"{placement}: DDR5 Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetMemoryModuleBuildKitStatusLabel(
                CurrentMemoryModuleBuildKitStatus);
            return PlacementValid
                ? $"[OK] DDR5 BUILD KIT HİZALI • {drop}: DDR5 modülünü yerleştir • " +
                  $"{rotate}: 180° döndür • {placement}: çık • {staged}/10 → 3/10"
                : $"[X] {status} • {rotate}: 180° döndür • {placement}: çık";
        }

        private static string GetMemoryModuleBuildKitStatusLabel(
            MemoryModuleBuildKitStatus status)
        {
            return status switch
            {
                MemoryModuleBuildKitStatus.Valid => "DDR5 BUILD KIT HAZIR",
                MemoryModuleBuildKitStatus.PrerequisiteMissing => "ÖNCE ANAKART VE İŞLEMCİYİ HAZIRLA",
                MemoryModuleBuildKitStatus.OutOfRange => "DDR5 BUILD KIT'E YAKLAŞ",
                MemoryModuleBuildKitStatus.NotFocused => "DDR5 TEPSİSİNİ HEDEFLE",
                MemoryModuleBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                MemoryModuleBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                MemoryModuleBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                MemoryModuleBuildKitStatus.Obstructed => "DDR5 TEPSİSİ DOLU",
                MemoryModuleBuildKitStatus.AlreadyStaged => "BELLEK ZATEN HAZIR",
                MemoryModuleBuildKitStatus.Paused => "DURAKLATILDI",
                MemoryModuleBuildKitStatus.AuthorityBlocked => "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
