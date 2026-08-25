using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 MotherboardBuildKitPreviewSize =
            new Vector3(0.254f, 0.254f, 0.060f);

        [SerializeField] private MotherboardBuildKitProjection motherboardBuildKit;

        public MotherboardBuildKitProjection MotherboardBuildKit => motherboardBuildKit;

        public bool IsMotherboardBuildKitMode { get; private set; }

        public MotherboardBuildKitStatus CurrentMotherboardBuildKitStatus
        {
            get;
            private set;
        } = MotherboardBuildKitStatus.ContextMissing;

        public void ConfigureMotherboardBuildKit(
            MotherboardBuildKitProjection projection,
            MotherboardAssemblyItemBinding assemblyBinding)
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
                    "The motherboard binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            motherboardBuildKit = projection;
            motherboardAssemblyBinding = assemblyBinding;
            motherboardBuildKit.RefreshPresentation();
        }

        public bool MatchesMotherboardBuildKitConfiguration(
            MotherboardBuildKitProjection projection,
            MotherboardAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   motherboardBuildKit == projection &&
                   motherboardAssemblyBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetMotherboardBuildKitMode(bool enabled)
        {
            MotherboardAssemblyItemBinding binding = GetMotherboardBinding(HeldItem);
            if (HeldItem == null || binding == null || motherboardBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-build-kit.paused")));
            }

            if (enabled && !motherboardBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-build-kit.authority-blocked")));
            }

            SetMotherboardBuildKitMode(enabled);
            if (enabled)
            {
                UpdateMotherboardBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateMotherboardBuildKitPreviewClockwise()
        {
            MotherboardAssemblyItemBinding binding = GetMotherboardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-build-kit.nothing-held")));
            }

            if (!IsMotherboardBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-build-kit.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 4;
            LastFailureCode = string.Empty;
            UpdateMotherboardBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmMotherboardBuildKit()
        {
            MotherboardAssemblyItemBinding binding = GetMotherboardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-build-kit.nothing-held")));
            }

            if (!IsMotherboardBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-build-kit.mode-inactive")));
            }

            MotherboardBuildKitEvaluation evaluation =
                EvaluateMotherboardBuildKit(binding);
            ApplyMotherboardBuildKitEvaluation(evaluation);
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
                motherboardBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private bool ProcessHeldMotherboardInput()
        {
            MotherboardAssemblyItemBinding binding = GetMotherboardBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            MotherboardBuildKitEvaluation buildKitEvaluation =
                EvaluateMotherboardBuildKit(binding);
            bool buildKitOwnsPrimary = motherboardBuildKit != null &&
                                       motherboardBuildKit.HasContextualAttention &&
                                       motherboardBuildKit.HasPickupReceipt;

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (buildKitOwnsPrimary)
                {
                    TrySetMotherboardBuildKitMode(!IsMotherboardBuildKitMode);
                }
                else
                {
                    TrySetMotherboardSeatMode(!IsMotherboardSeatMode);
                }

                return true;
            }

            if (IsMotherboardBuildKitMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateMotherboardBuildKitPreviewClockwise();
                return true;
            }

            if (IsMotherboardSeatMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                _placementRotationQuarterTurns =
                    (_placementRotationQuarterTurns + 1) % 4;
                LastFailureCode = string.Empty;
                UpdateMotherboardSeatPreview(binding);
                return true;
            }

            if (IsMotherboardBuildKitMode)
            {
                ApplyMotherboardBuildKitEvaluation(buildKitEvaluation);
            }
            else
            {
                UpdateMotherboardSeatPreview(binding);
            }

            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                if (IsMotherboardBuildKitMode)
                {
                    TryConfirmMotherboardBuildKit();
                }
                else if (IsMotherboardSeatMode)
                {
                    TryConfirmMotherboardSeat();
                }
                else
                {
                    TryDrop();
                }
            }

            return true;
        }

        private MotherboardBuildKitEvaluation EvaluateMotherboardBuildKit(
            MotherboardAssemblyItemBinding binding)
        {
            if (motherboardBuildKit == null)
            {
                return new MotherboardBuildKitEvaluation(
                    MotherboardBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return motherboardBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    motherboardBuildKit.HasPickupReceipt);
        }

        private void UpdateMotherboardBuildKitPreview(
            MotherboardAssemblyItemBinding binding)
        {
            if (!IsMotherboardBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                motherboardBuildKit?.ResetFeedback();
                return;
            }

            ApplyMotherboardBuildKitEvaluation(
                EvaluateMotherboardBuildKit(binding));
        }

        private void ApplyMotherboardBuildKitEvaluation(
            MotherboardBuildKitEvaluation evaluation)
        {
            CurrentMotherboardBuildKitStatus = evaluation.Status;
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
                        motherboardBuildKit != null
                            ? motherboardBuildKit.Surface
                            : null),
                    MotherboardBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetMotherboardBuildKitMode(bool enabled)
        {
            IsMotherboardBuildKitMode = enabled &&
                                        HeldItem != null &&
                                        GetMotherboardBinding(HeldItem) != null &&
                                        motherboardBuildKit != null;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            ResetProcessorBuildKitState();
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsM2StorageSeatMode = false;
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentMotherboardBuildKitStatus =
                MotherboardBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            motherboardSeat?.ResetFeedback();
            if (!IsMotherboardBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                motherboardBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetMotherboardBuildKitState()
        {
            IsMotherboardBuildKitMode = false;
            CurrentMotherboardBuildKitStatus =
                MotherboardBuildKitStatus.ContextMissing;
            motherboardBuildKit?.ResetFeedback();
        }

        private string GetHeldMotherboardBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            if (!IsMotherboardBuildKitMode)
            {
                return $"{placement}: Build Kit matına hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ ANAKARTI 0/10";
            }

            string status = GetMotherboardBuildKitStatusLabel(
                CurrentMotherboardBuildKitStatus);
            return PlacementValid
                ? $"[OK] BUILD KIT HİZALI • {drop}: anakartı yerleştir • " +
                  $"{rotate}: 90° döndür • {placement}: çık • 0/10 → 1/10"
                : $"[X] {status} • {rotate}: 90° döndür • {placement}: çık";
        }

        private static string GetMotherboardBuildKitStatusLabel(
            MotherboardBuildKitStatus status)
        {
            return status switch
            {
                MotherboardBuildKitStatus.Valid => "BUILD KIT HAZIR",
                MotherboardBuildKitStatus.OutOfRange => "BUILD KIT'E YAKLAŞ",
                MotherboardBuildKitStatus.NotFocused => "BUILD KIT MATINI HEDEFLE",
                MotherboardBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                MotherboardBuildKitStatus.Unsupported => "MAT DESTEĞİ YOK",
                MotherboardBuildKitStatus.OutsideSurface => "MAT DIŞI",
                MotherboardBuildKitStatus.Obstructed => "BUILD KIT DOLU",
                MotherboardBuildKitStatus.AlreadyStaged => "ANAKART ZATEN HAZIR",
                MotherboardBuildKitStatus.Paused => "DURAKLATILDI",
                MotherboardBuildKitStatus.AuthorityBlocked => "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
