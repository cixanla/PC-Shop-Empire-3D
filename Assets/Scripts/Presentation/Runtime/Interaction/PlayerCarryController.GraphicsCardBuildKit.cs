using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 GraphicsCardBuildKitPreviewSize =
            new Vector3(0.295f, 0.075f, 0.135f);

        [SerializeField]
        private GraphicsCardBuildKitProjection graphicsCardBuildKit;

        public GraphicsCardBuildKitProjection GraphicsCardBuildKit =>
            graphicsCardBuildKit;

        public bool IsGraphicsCardBuildKitMode { get; private set; }

        public GraphicsCardBuildKitStatus CurrentGraphicsCardBuildKitStatus
        {
            get;
            private set;
        } = GraphicsCardBuildKitStatus.ContextMissing;

        public void ConfigureGraphicsCardBuildKit(
            GraphicsCardBuildKitProjection projection,
            GraphicsCardAssemblyItemBinding assemblyBinding)
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
                    "The graphics-card binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            graphicsCardBuildKit = projection;
            graphicsCardBinding = assemblyBinding;
            graphicsCardBuildKit.RefreshPresentation();
        }

        public bool MatchesGraphicsCardBuildKitConfiguration(
            GraphicsCardBuildKitProjection projection,
            GraphicsCardAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   graphicsCardBuildKit == projection &&
                   graphicsCardBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetGraphicsCardBuildKitMode(bool enabled)
        {
            GraphicsCardAssemblyItemBinding binding =
                GetGraphicsCardBinding(HeldItem);
            if (HeldItem == null || binding == null ||
                graphicsCardBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-graphics-card-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-graphics-card-build-kit.paused")));
            }

            if (enabled && !graphicsCardBuildKit.HasPickupReceipt)
            {
                bool prerequisitesReady =
                    graphicsCardBuildKit.HasMotherboardPrerequisite &&
                    graphicsCardBuildKit.HasProcessorPrerequisite &&
                    graphicsCardBuildKit.HasMemoryModulePrerequisite &&
                    graphicsCardBuildKit.HasStoragePrerequisite &&
                    graphicsCardBuildKit.HasProcessorCoolerPrerequisite;
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        prerequisitesReady
                            ? "custom-pc-graphics-card-build-kit.authority-blocked"
                            : "custom-pc-graphics-card-build-kit.prerequisite-missing")));
            }

            SetGraphicsCardBuildKitMode(enabled);
            if (enabled)
            {
                UpdateGraphicsCardBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateGraphicsCardBuildKitPreviewClockwise()
        {
            GraphicsCardAssemblyItemBinding binding =
                GetGraphicsCardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-graphics-card-build-kit.nothing-held")));
            }

            if (!IsGraphicsCardBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-graphics-card-build-kit.mode-inactive")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-graphics-card-build-kit.paused")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateGraphicsCardBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmGraphicsCardBuildKit()
        {
            GraphicsCardAssemblyItemBinding binding =
                GetGraphicsCardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-graphics-card-build-kit.nothing-held")));
            }

            if (!IsGraphicsCardBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        "custom-pc-graphics-card-build-kit.mode-inactive")));
            }

            GraphicsCardBuildKitEvaluation evaluation =
                EvaluateGraphicsCardBuildKit(binding);
            ApplyGraphicsCardBuildKitEvaluation(evaluation);
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
                graphicsCardBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private GraphicsCardBuildKitEvaluation EvaluateGraphicsCardBuildKit(
            GraphicsCardAssemblyItemBinding binding)
        {
            if (graphicsCardBuildKit == null)
            {
                return new GraphicsCardBuildKitEvaluation(
                    GraphicsCardBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return graphicsCardBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    graphicsCardBuildKit.HasPickupReceipt);
        }

        private void UpdateGraphicsCardBuildKitPreview(
            GraphicsCardAssemblyItemBinding binding)
        {
            if (!IsGraphicsCardBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                graphicsCardBuildKit?.ResetFeedback();
                return;
            }

            ApplyGraphicsCardBuildKitEvaluation(
                EvaluateGraphicsCardBuildKit(binding));
        }

        private void ApplyGraphicsCardBuildKitEvaluation(
            GraphicsCardBuildKitEvaluation evaluation)
        {
            CurrentGraphicsCardBuildKitStatus = evaluation.Status;
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
                        graphicsCardBuildKit != null
                            ? graphicsCardBuildKit.Surface
                            : null),
                    GraphicsCardBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetGraphicsCardBuildKitMode(bool enabled)
        {
            IsGraphicsCardBuildKitMode = enabled &&
                                                HeldItem != null &&
                                                GetGraphicsCardBinding(HeldItem) != null &&
                                                graphicsCardBuildKit != null;
            IsGraphicsCardSeatMode = false;
            IsM2StorageSeatMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            ResetPcieGpuPowerCableState();
            ResetMotherboardBuildKitState();
            ResetProcessorBuildKitState();
            ResetMemoryModuleBuildKitState();
            ResetStorageBuildKitState();
            ResetProcessorCoolerBuildKitState();
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentGraphicsCardBuildKitStatus =
                GraphicsCardBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            graphicsCardSlot?.ResetFeedback();
            if (!IsGraphicsCardBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                graphicsCardBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetGraphicsCardBuildKitState()
        {
            IsGraphicsCardBuildKitMode = false;
            CurrentGraphicsCardBuildKitStatus =
                GraphicsCardBuildKitStatus.ContextMissing;
            graphicsCardBuildKit?.ResetFeedback();
        }

        private string GetHeldGraphicsCardBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = graphicsCardBuildKit?.StagedComponentCount ?? 5;
            if (!IsGraphicsCardBuildKitMode)
            {
                return $"{placement}: ekran kartını Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetGraphicsCardBuildKitStatusLabel(
                CurrentGraphicsCardBuildKitStatus);
            return PlacementValid
                ? $"[OK] GPU BUILD KIT HİZALI • {drop}: ekran kartını yerleştir • " +
                  $"{rotate}: 180° döndür • {placement}: çık • {staged}/10 → 6/10"
                : $"[X] {status} • {rotate}: 180° döndür • {placement}: çık";
        }

        private static string GetGraphicsCardBuildKitStatusLabel(
            GraphicsCardBuildKitStatus status)
        {
            return status switch
            {
                GraphicsCardBuildKitStatus.Valid => "GPU BUILD KIT HAZIR",
                GraphicsCardBuildKitStatus.PrerequisiteMissing =>
                    "ÖNCE ANAKART, İŞLEMCİ, BELLEK, NVMe VE SOĞUTUCUYU HAZIRLA",
                GraphicsCardBuildKitStatus.OutOfRange =>
                    "GPU BUILD KIT'E YAKLAŞ",
                GraphicsCardBuildKitStatus.NotFocused =>
                    "GPU TEPSİSİNİ HEDEFLE",
                GraphicsCardBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                GraphicsCardBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                GraphicsCardBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                GraphicsCardBuildKitStatus.Obstructed => "GPU TEPSİSİ DOLU",
                GraphicsCardBuildKitStatus.AlreadyStaged =>
                    "GPU ZATEN HAZIR",
                GraphicsCardBuildKitStatus.Paused => "DURAKLATILDI",
                GraphicsCardBuildKitStatus.AuthorityBlocked =>
                    "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
