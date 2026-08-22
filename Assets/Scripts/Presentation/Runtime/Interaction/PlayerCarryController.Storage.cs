using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 M2StorageSeatPreviewSize =
            new Vector3(0.082f, 0.005f, 0.024f);

        [SerializeField] private M2StorageSlotProjection m2StorageSlot;
        [SerializeField] private M2StorageAssemblyItemBinding m2StorageAssemblyBinding;

        public bool IsM2StorageSeatMode { get; private set; }

        public M2StorageSlotStatus CurrentM2StorageSlotStatus { get; private set; } =
            M2StorageSlotStatus.ContextMissing;

        public bool IsM2StorageSlotFocused { get; private set; }

        public bool HasM2StorageSlotContext { get; private set; }

        public void ConfigureM2StorageSlot(
            M2StorageSlotProjection slotProjection,
            M2StorageAssemblyItemBinding assemblyBinding)
        {
            if (slotProjection == null)
            {
                throw new ArgumentNullException(nameof(slotProjection));
            }

            if (assemblyBinding == null)
            {
                throw new ArgumentNullException(nameof(assemblyBinding));
            }

            if (assemblyBinding.Slot != slotProjection)
            {
                throw new ArgumentException(
                    "The M.2 storage binding must own the configured slot.",
                    nameof(assemblyBinding));
            }

            m2StorageSlot = slotProjection;
            m2StorageAssemblyBinding = assemblyBinding;
            m2StorageAssemblyBinding.SyncProjectionToAuthority();
        }

        public bool MatchesM2StorageConfiguration(
            M2StorageSlotProjection slotProjection,
            M2StorageAssemblyItemBinding assemblyBinding)
        {
            return slotProjection != null &&
                   assemblyBinding != null &&
                   m2StorageSlot == slotProjection &&
                   m2StorageAssemblyBinding == assemblyBinding &&
                   assemblyBinding.Slot == slotProjection;
        }

        public OperationResult TryOperateM2StorageCaptiveScrew()
        {
            if (m2StorageSlot == null || m2StorageAssemblyBinding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.context-missing")));
            }

            return TryOperateM2StorageCaptiveScrew(EvaluateM2StorageSlotInteraction());
        }

        private OperationResult TryOperateM2StorageCaptiveScrew(
            M2StorageSlotEvaluation evaluation)
        {
            ApplyM2StorageSlotEvaluation(evaluation);
            if (!evaluation.CanOperateRetention)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult result = m2StorageAssemblyBinding.TryOperateCaptiveScrew();
            if (result.IsSuccess)
            {
                GarageStockFlowSession session = m2StorageAssemblyBinding.Session;
                ApplyM2StorageSlotEvaluation(
                    m2StorageSlot.ApplyAuthoritativeInteractionFeedback(
                        session.AssemblyBuild.MotherboardSeatState,
                        session.AssemblyBuild.StorageSlotState));
            }

            return Remember(result);
        }

        public OperationResult TrySetM2StorageSeatMode(bool enabled)
        {
            M2StorageAssemblyItemBinding binding = GetM2StorageBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.paused")));
            }

            SetM2StorageSeatMode(enabled);
            if (enabled)
            {
                UpdateM2StorageSeatPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateM2StorageSeatPreviewClockwise()
        {
            M2StorageAssemblyItemBinding binding = GetM2StorageBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.nothing-held")));
            }

            if (!IsM2StorageSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.mode-inactive")));
            }

            _placementRotationQuarterTurns = (_placementRotationQuarterTurns + 2) % 4;
            LastFailureCode = string.Empty;
            UpdateM2StorageSeatPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmM2StorageSeat()
        {
            M2StorageAssemblyItemBinding binding = GetM2StorageBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.nothing-held")));
            }

            if (!IsM2StorageSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.mode-inactive")));
            }

            return TryConfirmM2StorageSeat(binding, EvaluateM2StorageSeat(binding));
        }

        private OperationResult TryConfirmM2StorageSeat(
            M2StorageAssemblyItemBinding binding,
            M2StorageSlotEvaluation evaluation)
        {
            ApplyM2StorageSeatEvaluation(evaluation);
            if (!evaluation.CanSeat)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult attach = binding.TryAttachAt(
                evaluation.GuidedPose,
                evaluation.Orientation);
            if (attach.IsSuccess)
            {
                CompleteHeldItemRelease();
                binding.SyncProjectionToAuthority();
            }
            else
            {
                SetCarryHandsState(blocked: true);
            }

            return Remember(attach);
        }

        private bool ProcessHeldM2StorageInput()
        {
            M2StorageAssemblyItemBinding binding = GetM2StorageBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TrySetM2StorageSeatMode(!IsM2StorageSeatMode);
                return true;
            }

            if (IsM2StorageSeatMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateM2StorageSeatPreviewClockwise();
                return true;
            }

            if (!IsM2StorageSeatMode)
            {
                UpdateM2StorageSeatPreview(binding);
                if (input.TryConsumeDropPressThisFrame())
                {
                    input.TryConsumePrimaryActionPressThisFrame();
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    TryDrop();
                }

                return true;
            }

            M2StorageSlotEvaluation evaluation = EvaluateM2StorageSeat(binding);
            ApplyM2StorageSeatEvaluation(evaluation);
            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                TryConfirmM2StorageSeat(binding, evaluation);
            }

            return true;
        }

        private void SetM2StorageSeatMode(bool enabled)
        {
            IsM2StorageSeatMode = enabled &&
                                  HeldItem != null &&
                                  GetM2StorageBinding(HeldItem) != null;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsPlacementMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentM2StorageSlotStatus = M2StorageSlotStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsM2StorageSeatMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                m2StorageSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdateM2StorageSeatPreview(M2StorageAssemblyItemBinding binding)
        {
            if (!IsM2StorageSeatMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentM2StorageSlotStatus = M2StorageSlotStatus.ContextMissing;
                CurrentStackSupport = null;
                placementPreview?.Hide();
                m2StorageSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyM2StorageSeatEvaluation(EvaluateM2StorageSeat(binding));
        }

        private M2StorageSlotEvaluation EvaluateM2StorageSeat(
            M2StorageAssemblyItemBinding binding)
        {
            M2StorageSlotProjection slotProjection = binding?.Slot ?? m2StorageSlot;
            if (slotProjection == null)
            {
                return new M2StorageSlotEvaluation(
                    M2StorageSlotStatus.ContextMissing,
                    default,
                    default,
                    false,
                    default);
            }

            binding?.SyncProjectionToAuthority();
            bool hostSecured = binding != null &&
                               binding.Session != null &&
                               binding.Session.AssemblyBuild.MotherboardSeatState ==
                                   AssemblySeatState.SeatedSecured;
            return slotProjection.EvaluateSeat(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated &&
                    hostSecured);
        }

        private void ApplyM2StorageSeatEvaluation(M2StorageSlotEvaluation evaluation)
        {
            CurrentM2StorageSlotStatus = evaluation.Status;
            PlacementValid = evaluation.CanSeat;
            CurrentPlacementStatus = evaluation.CanSeat
                ? PlacementStatus.Valid
                : PlacementStatus.Blocked;
            CurrentStackSupport = null;
            LastFailureCode = evaluation.CanSeat
                ? string.Empty
                : evaluation.FailureCode;

            if (evaluation.HasPose && HeldItem != null)
            {
                placementPreview?.Show(
                    HeldItem,
                    new PlacementEvaluation(
                        evaluation.CanSeat
                            ? PlacementStatus.Valid
                            : PlacementStatus.Blocked,
                        evaluation.GuidedPose,
                        true),
                    M2StorageSeatPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.CanSeat);
        }

        private void UpdateM2StorageSlotFocus()
        {
            if (m2StorageSlot == null || m2StorageAssemblyBinding == null)
            {
                ResetM2StorageSlotFocus();
                return;
            }

            m2StorageAssemblyBinding.SyncProjectionToAuthority();
            ApplyM2StorageSlotEvaluation(EvaluateM2StorageSlotInteraction());
        }

        private M2StorageSlotEvaluation EvaluateM2StorageSlotInteraction()
        {
            GarageStockFlowSession session = m2StorageAssemblyBinding != null
                ? m2StorageAssemblyBinding.Session
                : null;
            StorageSlotState state = session != null
                ? session.AssemblyBuild.StorageSlotState
                : StorageSlotState.Unsupported;
            bool retentionCloseAvailable = session != null &&
                                           (state == StorageSlotState.StorageDeviceSecured ||
                                            session.AssemblyBuild.MotherboardSeatState ==
                                                AssemblySeatState.SeatedSecured);
            return m2StorageSlot.EvaluateInteraction(
                resolver != null ? resolver.Origin : null,
                transform,
                m2StorageAssemblyBinding != null
                    ? m2StorageAssemblyBinding.PhysicalItem
                    : null,
                obstructionMask,
                motor == null || motor.IsPaused,
                m2StorageAssemblyBinding != null && m2StorageAssemblyBinding.IsSeated,
                state,
                retentionCloseAvailable);
        }

        private void ApplyM2StorageSlotEvaluation(M2StorageSlotEvaluation evaluation)
        {
            CurrentM2StorageSlotStatus = evaluation.Status;
            IsM2StorageSlotFocused = evaluation.CanOperateRetention || evaluation.CanRemove;
            HasM2StorageSlotContext = evaluation.HasOwnedContext;
            if (!IsM2StorageSlotFocused && HasM2StorageSlotContext)
            {
                LastFailureCode = evaluation.FailureCode;
            }
        }

        private void ResetM2StorageSlotFocus()
        {
            IsM2StorageSlotFocused = false;
            HasM2StorageSlotContext = false;
            CurrentM2StorageSlotStatus = M2StorageSlotStatus.ContextMissing;
            m2StorageSlot?.ResetFeedback();
        }

        private bool ProcessM2StorageSlotInput()
        {
            if (!IsM2StorageSlotFocused && !HasM2StorageSlotContext)
            {
                return false;
            }

            FocusedCart = null;
            FocusedItem = m2StorageAssemblyBinding.PhysicalItem;
            SetHandsState(VisibleHandsState.TargetFocused);
            if (IsM2StorageSlotFocused)
            {
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryOperateM2StorageCaptiveScrew(m2StorageSlot.LastEvaluation);
                    return true;
                }

                if (input.TryConsumeInteractPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryPickup(m2StorageAssemblyBinding.PhysicalItem);
                }

                return true;
            }

            bool primaryPressed = input.TryConsumePrimaryActionPressThisFrame();
            input.TryConsumeRotatePlacementPressThisFrame();
            input.TryConsumeInteractPressThisFrame();
            input.TryConsumeDropPressThisFrame();
            if (primaryPressed)
            {
                TryOperateM2StorageCaptiveScrew(m2StorageSlot.LastEvaluation);
            }

            return true;
        }

        private OperationResult TryPickupM2Storage(
            PhysicalItemProjection item,
            M2StorageAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-storage.paused")));
            }

            if (binding.IsSecured)
            {
                return Remember(OperationResult.Fail(AssemblyFailures.StorageDeviceSecured));
            }

            bool wasSeated = binding.IsSeated;
            OperationResult physicalPickup = item.BeginCarry(carryAnchor, heldItemLayer);
            if (physicalPickup.IsFailure)
            {
                return Remember(physicalPickup);
            }

            OperationResult authority = wasSeated
                ? binding.TryCommitSeatedDetach()
                : binding.TryCommitLoosePickup();
            if (authority.IsFailure)
            {
                OperationResult rollback = item.RecoverToLastSafePose();
                if (rollback.IsFailure)
                {
                    Debug.LogError(
                        $"M2_STORAGE_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
                }

                binding.SyncProjectionToAuthority();
                return Remember(authority);
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor?.ApplyCarryProfile(item.CarryProfile);
            binding.SyncProjectionToAuthority();
            SetCarryHandsState(blocked: false);
            return physicalPickup;
        }

        private string GetHeldM2StoragePrompt(
            M2StorageAssemblyItemBinding binding,
            string primary,
            string drop,
            string rotate)
        {
            if (!IsM2StorageSeatMode)
            {
                return $"{primary}: M.2 yuvasına hizala • {drop}: güvenli bırak • " +
                       "M-KEY NVMe 2280";
            }

            string state = GetM2StorageSlotStatusLabel(CurrentM2StorageSlotStatus);
            return PlacementValid
                ? $"[OK] M-KEY HİZALI • 18° TAKMA AÇISI • {drop}: oturt • " +
                  $"{rotate}: 180° döndür • {primary}: çık"
                : $"[X] {state} • {rotate}: 180° döndür • {primary}: çık";
        }

        private string GetM2StorageSlotPrompt()
        {
            string primary = input != null
                ? input.PrimaryBindingPrompt
                : "Mouse Left / RT";
            string interact = input != null
                ? input.InteractBindingPrompt
                : "E / A";
            if (!IsM2StorageSlotFocused)
            {
                return CurrentM2StorageSlotStatus switch
                {
                    M2StorageSlotStatus.LineOfSightBlocked =>
                        "[X] M.2 YUVASI ENGELLİ • görüş hattını aç",
                    M2StorageSlotStatus.Obstructed =>
                        "[X] M.2 YUVASI ENGELLİ • önünü aç",
                    _ => "[X] M.2 YUVASI KULLANILAMIYOR"
                };
            }

            if (CurrentM2StorageSlotStatus ==
                M2StorageSlotStatus.ValidSeatedUnsecuredRetentionBlocked)
            {
                return $"[GEVŞEK] NVMe OTURDU • ANAKART SABİT DEĞİL • " +
                       $"{interact}: SSD'yi çıkar";
            }

            return m2StorageAssemblyBinding.IsSecured
                ? $"[SABİT] M.2 VİDASI SIKILI • {primary}: gevşet • " +
                  $"{interact}: çıkarma kilitli"
                : $"[GEVŞEK] NVMe OTURDU • {primary}: vidayı sık • " +
                  $"{interact}: SSD'yi çıkar";
        }

        private static string GetM2StorageSlotStatusLabel(M2StorageSlotStatus status)
        {
            return status switch
            {
                M2StorageSlotStatus.ValidSeat => "M-KEY HİZALI",
                M2StorageSlotStatus.OutOfRange => "YAKLAŞ",
                M2StorageSlotStatus.NotFocused => "M.2 YUVASINI HEDEFLE",
                M2StorageSlotStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                M2StorageSlotStatus.OrientationInvalid => "M-KEY YÖNÜ TERS",
                M2StorageSlotStatus.Obstructed => "M.2 YUVASI ENGELLİ",
                M2StorageSlotStatus.Paused => "DURAKLATILDI",
                M2StorageSlotStatus.AuthorityBlocked => "ANAKART SABİT DEĞİL",
                M2StorageSlotStatus.ValidSeatedUnsecuredRetentionBlocked =>
                    "ANAKART SABİT DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private static M2StorageAssemblyItemBinding GetM2StorageBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<M2StorageAssemblyItemBinding>()
                : null;
        }
    }
}
