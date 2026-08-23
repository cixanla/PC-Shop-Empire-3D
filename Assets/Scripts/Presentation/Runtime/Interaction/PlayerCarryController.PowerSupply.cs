using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 PowerSupplySeatPreviewSize =
            new Vector3(0.15f, 0.086f, 0.14f);

        [SerializeField] private PowerSupplyBayProjection powerSupplyBay;
        [SerializeField] private PowerSupplyAssemblyItemBinding powerSupplyBinding;

        public bool IsPowerSupplySeatMode { get; private set; }

        public PowerSupplyBayStatus CurrentPowerSupplyBayStatus
        {
            get;
            private set;
        } = PowerSupplyBayStatus.ContextMissing;

        public bool HasPowerSupplyBayContext { get; private set; }

        public bool IsPowerSupplyBayFocused { get; private set; }

        public void ConfigurePowerSupplyBay(
            PowerSupplyBayProjection slotProjection,
            PowerSupplyAssemblyItemBinding assemblyBinding)
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
                    "The power-supply binding must own the configured slot.",
                    nameof(assemblyBinding));
            }

            powerSupplyBay = slotProjection;
            powerSupplyBinding = assemblyBinding;
            powerSupplyBinding.SyncProjectionToAuthority();
        }

        public bool MatchesPowerSupplyConfiguration(
            PowerSupplyBayProjection slotProjection,
            PowerSupplyAssemblyItemBinding assemblyBinding)
        {
            return slotProjection != null &&
                   assemblyBinding != null &&
                   powerSupplyBay == slotProjection &&
                   powerSupplyBinding == assemblyBinding &&
                   assemblyBinding.Slot == slotProjection;
        }

        public OperationResult TryOperatePowerSupplyRetention()
        {
            if (powerSupplyBay == null || powerSupplyBinding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.context-missing")));
            }

            PowerSupplyBayEvaluation evaluation =
                EvaluatePowerSupplyBayInteraction();
            ApplyPowerSupplyBayEvaluation(evaluation);
            if (!evaluation.CanOperateRetention)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult result = powerSupplyBinding.TryOperateRetention();
            if (result.IsSuccess)
            {
                UpdatePowerSupplyBayFocus();
            }

            return Remember(result);
        }

        public OperationResult TrySetPowerSupplySeatMode(bool enabled)
        {
            PowerSupplyAssemblyItemBinding binding =
                GetPowerSupplyBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.paused")));
            }

            SetPowerSupplySeatMode(enabled);
            if (enabled)
            {
                UpdatePowerSupplySeatPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotatePowerSupplySeatPreview()
        {
            PowerSupplyAssemblyItemBinding binding =
                GetPowerSupplyBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.nothing-held")));
            }

            if (!IsPowerSupplySeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdatePowerSupplySeatPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmPowerSupplySeat()
        {
            PowerSupplyAssemblyItemBinding binding =
                GetPowerSupplyBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.nothing-held")));
            }

            if (!IsPowerSupplySeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.mode-inactive")));
            }

            return TryConfirmPowerSupplySeat(
                binding,
                EvaluatePowerSupplySeat(binding));
        }

        private OperationResult TryConfirmPowerSupplySeat(
            PowerSupplyAssemblyItemBinding binding,
            PowerSupplyBayEvaluation evaluation)
        {
            ApplyPowerSupplySeatEvaluation(evaluation);
            if (!evaluation.CanSeat)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult attach = binding.TryAttachAt(
                evaluation.Pose,
                evaluation.Orientation,
                carryAnchor,
                heldItemLayer);
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

        private bool ProcessHeldPowerSupplyInput()
        {
            PowerSupplyAssemblyItemBinding binding =
                GetPowerSupplyBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TrySetPowerSupplySeatMode(!IsPowerSupplySeatMode);
                return true;
            }

            if (IsPowerSupplySeatMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotatePowerSupplySeatPreview();
                return true;
            }

            if (!IsPowerSupplySeatMode)
            {
                UpdatePowerSupplySeatPreview(binding);
                if (input.TryConsumeDropPressThisFrame())
                {
                    input.TryConsumePrimaryActionPressThisFrame();
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    TryDrop();
                }

                return true;
            }

            PowerSupplyBayEvaluation evaluation =
                EvaluatePowerSupplySeat(binding);
            ApplyPowerSupplySeatEvaluation(evaluation);
            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                TryConfirmPowerSupplySeat(binding, evaluation);
            }

            return true;
        }

        private void SetPowerSupplySeatMode(bool enabled)
        {
            IsPowerSupplySeatMode = enabled &&
                                        HeldItem != null &&
                                        GetPowerSupplyBinding(HeldItem) != null;
            IsPlacementMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsM2StorageSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsGraphicsCardSeatMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentPowerSupplyBayStatus =
                PowerSupplyBayStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsPowerSupplySeatMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                powerSupplyBay?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdatePowerSupplySeatPreview(
            PowerSupplyAssemblyItemBinding binding)
        {
            if (!IsPowerSupplySeatMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentPowerSupplyBayStatus =
                    PowerSupplyBayStatus.ContextMissing;
                CurrentStackSupport = null;
                placementPreview?.Hide();
                powerSupplyBay?.ResetFeedback();
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyPowerSupplySeatEvaluation(
                EvaluatePowerSupplySeat(binding));
        }

        private PowerSupplyBayEvaluation EvaluatePowerSupplySeat(
            PowerSupplyAssemblyItemBinding binding)
        {
            PowerSupplyBayProjection slotProjection =
                binding?.Slot ?? powerSupplyBay;
            if (slotProjection == null)
            {
                return new PowerSupplyBayEvaluation(
                    PowerSupplyBayStatus.ContextMissing,
                    default,
                    false,
                    default);
            }

            binding?.SyncProjectionToAuthority();
            return slotProjection.EvaluateSeat(
                IsPowerSupplySeatMode,
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    !binding.IsSeated,
                binding != null
                    ? binding.FormFactor
                    : PowerSupplyFormFactor.Unknown,
                binding != null && binding.HasChassisClearance,
                binding != null && binding.HasCableClearance);
        }

        private void ApplyPowerSupplySeatEvaluation(
            PowerSupplyBayEvaluation evaluation)
        {
            CurrentPowerSupplyBayStatus = evaluation.Status;
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
                        evaluation.Pose,
                        true),
                    PowerSupplySeatPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.CanSeat);
        }

        private void UpdatePowerSupplyBayFocus()
        {
            if (powerSupplyBay == null || powerSupplyBinding == null)
            {
                ResetPowerSupplyBayFocus();
                return;
            }

            powerSupplyBinding.SyncProjectionToAuthority();
            ApplyPowerSupplyBayEvaluation(
                EvaluatePowerSupplyBayInteraction());
        }

        private PowerSupplyBayEvaluation
            EvaluatePowerSupplyBayInteraction()
        {
            return powerSupplyBay.EvaluateInteraction(
                true,
                resolver != null ? resolver.Origin : null,
                transform,
                powerSupplyBinding != null
                    ? powerSupplyBinding.PhysicalItem.transform
                    : null,
                obstructionMask,
                motor == null || motor.IsPaused,
                powerSupplyBinding != null &&
                    powerSupplyBinding.IsSeated);
        }

        private void ApplyPowerSupplyBayEvaluation(
            PowerSupplyBayEvaluation evaluation)
        {
            CurrentPowerSupplyBayStatus = evaluation.Status;
            IsPowerSupplyBayFocused =
                evaluation.CanOperateRetention || evaluation.CanRemove;
            HasPowerSupplyBayContext = evaluation.HasOwnedContext;
            if (!IsPowerSupplyBayFocused && HasPowerSupplyBayContext)
            {
                LastFailureCode = evaluation.FailureCode;
            }
        }

        private void ResetPowerSupplyBayFocus()
        {
            IsPowerSupplyBayFocused = false;
            HasPowerSupplyBayContext = false;
            CurrentPowerSupplyBayStatus =
                PowerSupplyBayStatus.ContextMissing;
            powerSupplyBay?.ResetFeedback();
        }

        private bool ProcessPowerSupplyBayInput()
        {
            if (!IsPowerSupplyBayFocused &&
                !HasPowerSupplyBayContext)
            {
                return false;
            }

            FocusedCart = null;
            FocusedItem = powerSupplyBinding.PhysicalItem;
            SetHandsState(VisibleHandsState.TargetFocused);
            if (IsPowerSupplyBayFocused)
            {
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryOperatePowerSupplyRetention();
                    return true;
                }

                if (input.TryConsumeInteractPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryPickup(powerSupplyBinding.PhysicalItem);
                }

                return true;
            }

            bool primaryPressed = input.TryConsumePrimaryActionPressThisFrame();
            input.TryConsumeRotatePlacementPressThisFrame();
            input.TryConsumeInteractPressThisFrame();
            input.TryConsumeDropPressThisFrame();
            if (primaryPressed)
            {
                TryOperatePowerSupplyRetention();
            }

            return true;
        }

        private bool ProcessLoosePowerSupplyPickupInput()
        {
            if (resolver == null ||
                powerSupplyBinding == null ||
                powerSupplyBinding.IsSeated ||
                !powerSupplyBinding.IsAuthorityLooseWorld)
            {
                return false;
            }

            OperationResult<PhysicalItemProjection> target = resolver.Resolve();
            if (target.IsFailure ||
                target.Value != powerSupplyBinding.PhysicalItem)
            {
                return false;
            }

            ResetPowerSupplyBayFocus();
            FocusedCart = null;
            FocusedItem = target.Value;
            SetHandsState(VisibleHandsState.TargetFocused);
            if (input.TryConsumeInteractPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryPickup(FocusedItem);
            }

            return true;
        }

        private OperationResult TryPickupPowerSupply(
            PhysicalItemProjection item,
            PowerSupplyAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.paused")));
            }

            if (binding.IsRetained)
            {
                return Remember(OperationResult.Fail(
                    AssemblyFailures.PowerSupplyRetained));
            }

            bool wasSeated = binding.IsSeated;
            OperationResult physicalPickup = item.BeginCarry(
                carryAnchor,
                heldItemLayer);
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
                        $"POWER_SUPPLY_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
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

        private string GetHeldPowerSupplyPrompt(
            PowerSupplyAssemblyItemBinding binding,
            string primary,
            string drop,
            string rotate)
        {
            if (!IsPowerSupplySeatMode)
            {
                return $"{primary}: PSU yuvasına hizala • " +
                       $"{drop}: güvenli bırak • ATX PS/2";
            }

            string state = GetPowerSupplyStatusLabel(
                CurrentPowerSupplyBayStatus);
            string orientation = _placementRotationQuarterTurns == 0
                ? "FAN FİLTREYE"
                : "180°";
            return PlacementValid
                ? $"[OK] ATX PS/2 HİZALI • {orientation} • " +
                  $"{drop}: oturt • {rotate}: 180° döndür • {primary}: çık"
                : $"[X] {state} • {rotate}: 180° döndür • {primary}: çık";
        }

        private string GetPowerSupplyBayPrompt()
        {
            string primary = input != null
                ? input.PrimaryBindingPrompt
                : "Mouse Left / RT";
            string interact = input != null
                ? input.InteractBindingPrompt
                : "E / A";
            if (!IsPowerSupplyBayFocused)
            {
                return CurrentPowerSupplyBayStatus switch
                {
                    PowerSupplyBayStatus.LineOfSightBlocked =>
                        "[X] PSU YUVASI ENGELLİ • görüş hattını aç",
                    PowerSupplyBayStatus.ChassisClearanceBlocked =>
                        "[X] KASA AÇIKLIĞI YETERSİZ",
                    PowerSupplyBayStatus.CableClearanceBlocked =>
                        "[X] KABLO ALANI PSU YUVASINI ENGELLİYOR",
                    PowerSupplyBayStatus.Obstructed =>
                        "[X] PSU MONTAJ ALANI ENGELLİ • önünü aç",
                    _ => "[X] PSU YUVASI KULLANILAMIYOR"
                };
            }

            return powerSupplyBinding.IsRetained
                ? $"[SABİT] PSU ARKA PLAKA + 4 VİDA KİLİTLİ • {primary}: " +
                  $"gevşet • {interact}: çıkarma kilitli"
                : $"[GEVŞEK] PSU OTURDU • {primary}: " +
                  $"4 vidayı çapraz sık • {interact}: PSU'yu çıkar";
        }

        private static string GetPowerSupplyStatusLabel(
            PowerSupplyBayStatus status)
        {
            return status switch
            {
                PowerSupplyBayStatus.ValidSeat => "ATX PS/2 HİZALI",
                PowerSupplyBayStatus.OutOfRange => "YAKLAŞ",
                PowerSupplyBayStatus.NotFocused => "PSU YUVASINI HEDEFLE",
                PowerSupplyBayStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                PowerSupplyBayStatus.FormFactorInvalid => "PSU FORMATI UYUMSUZ",
                PowerSupplyBayStatus.OrientationInvalid => "FAN YÖNÜ TERS",
                PowerSupplyBayStatus.Unsupported => "FİLTRELİ TABAN DESTEĞİ YOK",
                PowerSupplyBayStatus.ChassisClearanceBlocked =>
                    "KASA AÇIKLIĞI YETERSİZ",
                PowerSupplyBayStatus.CableClearanceBlocked =>
                    "KABLO AÇIKLIĞI YETERSİZ",
                PowerSupplyBayStatus.Obstructed => "MONTAJ ALANI ENGELLİ",
                PowerSupplyBayStatus.Paused => "DURAKLATILDI",
                PowerSupplyBayStatus.AuthorityBlocked => "AUTHORITY ENGELLİ",
                _ => "BAĞLANTI YOK"
            };
        }

        private static PowerSupplyAssemblyItemBinding GetPowerSupplyBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<PowerSupplyAssemblyItemBinding>()
                : null;
        }
    }
}
