using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 StorageBuildKitPreviewSize =
            new Vector3(0.082f, 0.009f, 0.024f);

        [SerializeField] private StorageBuildKitProjection storageBuildKit;

        public StorageBuildKitProjection StorageBuildKit => storageBuildKit;

        public bool IsStorageBuildKitMode { get; private set; }

        public StorageBuildKitStatus CurrentStorageBuildKitStatus
        {
            get;
            private set;
        } = StorageBuildKitStatus.ContextMissing;

        public void ConfigureStorageBuildKit(
            StorageBuildKitProjection projection,
            M2StorageAssemblyItemBinding assemblyBinding)
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
                    "The M.2 storage binding must own the configured Build Kit.",
                    nameof(assemblyBinding));
            }

            storageBuildKit = projection;
            m2StorageAssemblyBinding = assemblyBinding;
            storageBuildKit.RefreshPresentation();
        }

        public bool MatchesStorageBuildKitConfiguration(
            StorageBuildKitProjection projection,
            M2StorageAssemblyItemBinding assemblyBinding)
        {
            return projection != null &&
                   assemblyBinding != null &&
                   storageBuildKit == projection &&
                   m2StorageAssemblyBinding == assemblyBinding &&
                   assemblyBinding.MatchesBuildKitConfiguration(projection);
        }

        public OperationResult TrySetStorageBuildKitMode(bool enabled)
        {
            M2StorageAssemblyItemBinding binding = GetM2StorageBinding(HeldItem);
            if (HeldItem == null || binding == null || storageBuildKit == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-storage-build-kit.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-storage-build-kit.paused")));
            }

            if (enabled && !storageBuildKit.HasPickupReceipt)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(
                        storageBuildKit.HasMotherboardPrerequisite &&
                        storageBuildKit.HasProcessorPrerequisite &&
                        storageBuildKit.HasMemoryModulePrerequisite
                            ? "custom-pc-storage-build-kit.authority-blocked"
                            : "custom-pc-storage-build-kit.prerequisite-missing")));
            }

            SetStorageBuildKitMode(enabled);
            if (enabled)
            {
                UpdateStorageBuildKitPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateStorageBuildKitPreviewClockwise()
        {
            M2StorageAssemblyItemBinding binding = GetM2StorageBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-storage-build-kit.nothing-held")));
            }

            if (!IsStorageBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-storage-build-kit.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateStorageBuildKitPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmStorageBuildKit()
        {
            M2StorageAssemblyItemBinding binding = GetM2StorageBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-storage-build-kit.nothing-held")));
            }

            if (!IsStorageBuildKitMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("custom-pc-storage-build-kit.mode-inactive")));
            }

            StorageBuildKitEvaluation evaluation =
                EvaluateStorageBuildKit(binding);
            ApplyStorageBuildKitEvaluation(evaluation);
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
                storageBuildKit.RefreshPresentation();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(placement);
        }

        private StorageBuildKitEvaluation EvaluateStorageBuildKit(
            M2StorageAssemblyItemBinding binding)
        {
            if (storageBuildKit == null)
            {
                return new StorageBuildKitEvaluation(
                    StorageBuildKitStatus.ContextMissing,
                    default,
                    false);
            }

            return storageBuildKit.Evaluate(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    storageBuildKit.HasPickupReceipt);
        }

        private void UpdateStorageBuildKitPreview(
            M2StorageAssemblyItemBinding binding)
        {
            if (!IsStorageBuildKitMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                storageBuildKit?.ResetFeedback();
                return;
            }

            ApplyStorageBuildKitEvaluation(
                EvaluateStorageBuildKit(binding));
        }

        private void ApplyStorageBuildKitEvaluation(
            StorageBuildKitEvaluation evaluation)
        {
            CurrentStorageBuildKitStatus = evaluation.Status;
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
                        storageBuildKit != null
                            ? storageBuildKit.Surface
                            : null),
                    StorageBuildKitPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void SetStorageBuildKitMode(bool enabled)
        {
            IsStorageBuildKitMode = enabled &&
                                      HeldItem != null &&
                                      GetM2StorageBinding(HeldItem) != null &&
                                      storageBuildKit != null;
            IsM2StorageSeatMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsPowerSupplySeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            ResetMotherboardBuildKitState();
            ResetProcessorBuildKitState();
            ResetMemoryModuleBuildKitState();
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentStorageBuildKitStatus =
                StorageBuildKitStatus.ContextMissing;
            LastFailureCode = string.Empty;
            m2StorageSlot?.ResetFeedback();
            if (!IsStorageBuildKitMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                storageBuildKit?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void ResetStorageBuildKitState()
        {
            IsStorageBuildKitMode = false;
            CurrentStorageBuildKitStatus =
                StorageBuildKitStatus.ContextMissing;
            storageBuildKit?.ResetFeedback();
        }

        private string GetHeldStorageBuildKitPrompt(
            string placement,
            string drop,
            string rotate)
        {
            int staged = storageBuildKit?.StagedComponentCount ?? 3;
            if (!IsStorageBuildKitMode)
            {
                return $"{placement}: M.2 NVMe Build Kit tepsisine hizala • " +
                       $"{drop}: bırakma kilitli • İŞ EMRİ {staged}/10";
            }

            string status = GetStorageBuildKitStatusLabel(
                CurrentStorageBuildKitStatus);
            return PlacementValid
                ? $"[OK] M.2 NVMe BUILD KIT HİZALI • {drop}: M.2 NVMe'yi yerleştir • " +
                  $"{rotate}: 180° döndür • {placement}: çık • {staged}/10 → 4/10"
                : $"[X] {status} • {rotate}: 180° döndür • {placement}: çık";
        }

        private static string GetStorageBuildKitStatusLabel(
            StorageBuildKitStatus status)
        {
            return status switch
            {
                StorageBuildKitStatus.Valid => "M.2 NVMe BUILD KIT HAZIR",
                StorageBuildKitStatus.PrerequisiteMissing => "ÖNCE ANAKART, İŞLEMCİ VE BELLEĞİ HAZIRLA",
                StorageBuildKitStatus.OutOfRange => "M.2 NVMe BUILD KIT'E YAKLAŞ",
                StorageBuildKitStatus.NotFocused => "M.2 NVMe TEPSİSİNİ HEDEFLE",
                StorageBuildKitStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                StorageBuildKitStatus.Unsupported => "TEPSİ DESTEĞİ YOK",
                StorageBuildKitStatus.OutsideSurface => "TEPSİ DIŞI",
                StorageBuildKitStatus.Obstructed => "M.2 NVMe TEPSİSİ DOLU",
                StorageBuildKitStatus.AlreadyStaged => "NVMe ZATEN HAZIR",
                StorageBuildKitStatus.Paused => "DURAKLATILDI",
                StorageBuildKitStatus.AuthorityBlocked => "İŞ EMRİ YETKİSİ YOK",
                _ => "BAĞLANTI YOK"
            };
        }
    }
}
