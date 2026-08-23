using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 ProcessorCoolerSeatPreviewSize =
            new Vector3(0.118f, 0.074f, 0.118f);

        [SerializeField] private ProcessorCoolerSlotProjection processorCoolerSlot;
        [SerializeField] private ProcessorCoolerAssemblyItemBinding processorCoolerBinding;

        public bool IsProcessorCoolerSeatMode { get; private set; }

        public ProcessorCoolerSlotStatus CurrentProcessorCoolerSlotStatus
        {
            get;
            private set;
        } = ProcessorCoolerSlotStatus.ContextMissing;

        public bool HasProcessorCoolerSlotContext { get; private set; }

        public bool IsProcessorCoolerSlotFocused { get; private set; }

        public void ConfigureProcessorCoolerSlot(
            ProcessorCoolerSlotProjection slotProjection,
            ProcessorCoolerAssemblyItemBinding assemblyBinding)
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
                    "The cooler binding must own the configured slot.",
                    nameof(assemblyBinding));
            }

            processorCoolerSlot = slotProjection;
            processorCoolerBinding = assemblyBinding;
            processorCoolerBinding.SyncProjectionToAuthority();
        }

        public bool MatchesProcessorCoolerConfiguration(
            ProcessorCoolerSlotProjection slotProjection,
            ProcessorCoolerAssemblyItemBinding assemblyBinding)
        {
            return slotProjection != null &&
                   assemblyBinding != null &&
                   processorCoolerSlot == slotProjection &&
                   processorCoolerBinding == assemblyBinding &&
                   assemblyBinding.Slot == slotProjection;
        }

        public OperationResult TryOperateProcessorCoolerRetention()
        {
            if (processorCoolerSlot == null || processorCoolerBinding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.context-missing")));
            }

            ProcessorCoolerSlotEvaluation evaluation =
                EvaluateProcessorCoolerSlotInteraction();
            ApplyProcessorCoolerSlotEvaluation(evaluation);
            if (!evaluation.CanOperateRetention)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult result = processorCoolerBinding.TryOperateRetention();
            if (result.IsSuccess)
            {
                UpdateProcessorCoolerSlotFocus();
            }

            return Remember(result);
        }

        public OperationResult TrySetProcessorCoolerSeatMode(bool enabled)
        {
            ProcessorCoolerAssemblyItemBinding binding =
                GetProcessorCoolerBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.paused")));
            }

            SetProcessorCoolerSeatMode(enabled);
            if (enabled)
            {
                UpdateProcessorCoolerSeatPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateProcessorCoolerSeatPreview()
        {
            ProcessorCoolerAssemblyItemBinding binding =
                GetProcessorCoolerBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.nothing-held")));
            }

            if (!IsProcessorCoolerSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateProcessorCoolerSeatPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmProcessorCoolerSeat()
        {
            ProcessorCoolerAssemblyItemBinding binding =
                GetProcessorCoolerBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.nothing-held")));
            }

            if (!IsProcessorCoolerSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.mode-inactive")));
            }

            return TryConfirmProcessorCoolerSeat(
                binding,
                EvaluateProcessorCoolerSeat(binding));
        }

        private OperationResult TryConfirmProcessorCoolerSeat(
            ProcessorCoolerAssemblyItemBinding binding,
            ProcessorCoolerSlotEvaluation evaluation)
        {
            ApplyProcessorCoolerSeatEvaluation(evaluation);
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

        private bool ProcessHeldProcessorCoolerInput()
        {
            ProcessorCoolerAssemblyItemBinding binding =
                GetProcessorCoolerBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TrySetProcessorCoolerSeatMode(!IsProcessorCoolerSeatMode);
                return true;
            }

            if (IsProcessorCoolerSeatMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateProcessorCoolerSeatPreview();
                return true;
            }

            if (!IsProcessorCoolerSeatMode)
            {
                UpdateProcessorCoolerSeatPreview(binding);
                if (input.TryConsumeDropPressThisFrame())
                {
                    input.TryConsumePrimaryActionPressThisFrame();
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    TryDrop();
                }

                return true;
            }

            ProcessorCoolerSlotEvaluation evaluation =
                EvaluateProcessorCoolerSeat(binding);
            ApplyProcessorCoolerSeatEvaluation(evaluation);
            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumePrimaryActionPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                TryConfirmProcessorCoolerSeat(binding, evaluation);
            }

            return true;
        }

        private void SetProcessorCoolerSeatMode(bool enabled)
        {
            IsProcessorCoolerSeatMode = enabled &&
                                        HeldItem != null &&
                                        GetProcessorCoolerBinding(HeldItem) != null;
            IsPlacementMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsM2StorageSeatMode = false;
            IsGraphicsCardSeatMode = false;
            IsPowerSupplySeatMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentProcessorCoolerSlotStatus =
                ProcessorCoolerSlotStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsProcessorCoolerSeatMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                processorCoolerSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdateProcessorCoolerSeatPreview(
            ProcessorCoolerAssemblyItemBinding binding)
        {
            if (!IsProcessorCoolerSeatMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentProcessorCoolerSlotStatus =
                    ProcessorCoolerSlotStatus.ContextMissing;
                CurrentStackSupport = null;
                placementPreview?.Hide();
                processorCoolerSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyProcessorCoolerSeatEvaluation(
                EvaluateProcessorCoolerSeat(binding));
        }

        private ProcessorCoolerSlotEvaluation EvaluateProcessorCoolerSeat(
            ProcessorCoolerAssemblyItemBinding binding)
        {
            ProcessorCoolerSlotProjection slotProjection =
                binding?.Slot ?? processorCoolerSlot;
            if (slotProjection == null)
            {
                return new ProcessorCoolerSlotEvaluation(
                    ProcessorCoolerSlotStatus.ContextMissing,
                    default,
                    false,
                    default);
            }

            binding?.SyncProjectionToAuthority();
            return slotProjection.EvaluateSeat(
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    binding.IsHostReady &&
                    !binding.IsSeated);
        }

        private void ApplyProcessorCoolerSeatEvaluation(
            ProcessorCoolerSlotEvaluation evaluation)
        {
            CurrentProcessorCoolerSlotStatus = evaluation.Status;
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
                    ProcessorCoolerSeatPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.CanSeat);
        }

        private void UpdateProcessorCoolerSlotFocus()
        {
            if (processorCoolerSlot == null || processorCoolerBinding == null)
            {
                ResetProcessorCoolerSlotFocus();
                return;
            }

            processorCoolerBinding.SyncProjectionToAuthority();
            ApplyProcessorCoolerSlotEvaluation(
                EvaluateProcessorCoolerSlotInteraction());
        }

        private ProcessorCoolerSlotEvaluation
            EvaluateProcessorCoolerSlotInteraction()
        {
            return processorCoolerSlot.EvaluateInteraction(
                resolver != null ? resolver.Origin : null,
                transform,
                processorCoolerBinding != null
                    ? processorCoolerBinding.PhysicalItem.transform
                    : null,
                obstructionMask,
                motor == null || motor.IsPaused,
                processorCoolerBinding != null &&
                    processorCoolerBinding.IsSeated,
                processorCoolerBinding != null &&
                    (processorCoolerBinding.IsRetained ||
                     processorCoolerBinding.IsHostReady));
        }

        private void ApplyProcessorCoolerSlotEvaluation(
            ProcessorCoolerSlotEvaluation evaluation)
        {
            CurrentProcessorCoolerSlotStatus = evaluation.Status;
            IsProcessorCoolerSlotFocused =
                evaluation.CanOperateRetention || evaluation.CanRemove;
            HasProcessorCoolerSlotContext = evaluation.HasOwnedContext;
            if (!IsProcessorCoolerSlotFocused && HasProcessorCoolerSlotContext)
            {
                LastFailureCode = evaluation.FailureCode;
            }
        }

        private void ResetProcessorCoolerSlotFocus()
        {
            IsProcessorCoolerSlotFocused = false;
            HasProcessorCoolerSlotContext = false;
            CurrentProcessorCoolerSlotStatus =
                ProcessorCoolerSlotStatus.ContextMissing;
            processorCoolerSlot?.ResetFeedback();
        }

        private bool ProcessProcessorCoolerSlotInput()
        {
            if (!IsProcessorCoolerSlotFocused &&
                !HasProcessorCoolerSlotContext)
            {
                return false;
            }

            FocusedCart = null;
            FocusedItem = processorCoolerBinding.PhysicalItem;
            SetHandsState(VisibleHandsState.TargetFocused);
            if (IsProcessorCoolerSlotFocused)
            {
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryOperateProcessorCoolerRetention();
                    return true;
                }

                if (input.TryConsumeInteractPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryPickup(processorCoolerBinding.PhysicalItem);
                }

                return true;
            }

            bool primaryPressed = input.TryConsumePrimaryActionPressThisFrame();
            input.TryConsumeRotatePlacementPressThisFrame();
            input.TryConsumeInteractPressThisFrame();
            input.TryConsumeDropPressThisFrame();
            if (primaryPressed)
            {
                TryOperateProcessorCoolerRetention();
            }

            return true;
        }

        private bool ProcessLooseProcessorCoolerPickupInput()
        {
            if (resolver == null ||
                processorCoolerBinding == null ||
                processorCoolerBinding.IsSeated ||
                !processorCoolerBinding.IsAuthorityLooseWorld)
            {
                return false;
            }

            OperationResult<PhysicalItemProjection> target = resolver.Resolve();
            if (target.IsFailure ||
                target.Value != processorCoolerBinding.PhysicalItem)
            {
                return false;
            }

            ResetProcessorCoolerSlotFocus();
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

        private OperationResult TryPickupProcessorCooler(
            PhysicalItemProjection item,
            ProcessorCoolerAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-cooler.paused")));
            }

            if (binding.IsRetained)
            {
                return Remember(OperationResult.Fail(
                    AssemblyFailures.ProcessorCoolerRetained));
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
                        $"PROCESSOR_COOLER_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
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

        private string GetHeldProcessorCoolerPrompt(
            ProcessorCoolerAssemblyItemBinding binding,
            string primary,
            string drop,
            string rotate)
        {
            if (!IsProcessorCoolerSeatMode)
            {
                return $"{primary}: CPU soğutucusunu hizala • " +
                       $"{drop}: güvenli bırak • ÖN UYGULAMALI TIM";
            }

            string state = GetProcessorCoolerStatusLabel(
                CurrentProcessorCoolerSlotStatus);
            string orientation = _placementRotationQuarterTurns == 0
                ? "0°"
                : "180°";
            return PlacementValid
                ? $"[OK] 4 NOKTA HİZALI • YÖN {orientation} • " +
                  $"{drop}: oturt • {rotate}: 180° döndür • {primary}: çık"
                : $"[X] {state} • {rotate}: 180° döndür • {primary}: çık";
        }

        private string GetProcessorCoolerSlotPrompt()
        {
            string primary = input != null
                ? input.PrimaryBindingPrompt
                : "Mouse Left / RT";
            string interact = input != null
                ? input.InteractBindingPrompt
                : "E / A";
            if (!IsProcessorCoolerSlotFocused)
            {
                return CurrentProcessorCoolerSlotStatus switch
                {
                    ProcessorCoolerSlotStatus.LineOfSightBlocked =>
                        "[X] SOĞUTUCU ENGELLİ • görüş hattını aç",
                    ProcessorCoolerSlotStatus.Obstructed =>
                        "[X] SOĞUTUCU ALANI ENGELLİ • önünü aç",
                    _ => "[X] SOĞUTUCU BAĞLANTISI KULLANILAMIYOR"
                };
            }

            if (CurrentProcessorCoolerSlotStatus ==
                ProcessorCoolerSlotStatus.ValidSeatedUnsecuredRetentionBlocked)
            {
                return $"[GEVŞEK] SOĞUTUCU OTURDU • HOST HAZIR DEĞİL • " +
                       $"{interact}: soğutucuyu çıkar";
            }

            return processorCoolerBinding.IsRetained
                ? $"[SABİT] 4 NOKTA 1→3→2→4 SIKILI • {primary}: " +
                  $"4→2→3→1 gevşet • {interact}: çıkarma kilitli"
                : $"[GEVŞEK] SOĞUTUCU OTURDU • {primary}: " +
                  $"1→3→2→4 sık • {interact}: soğutucuyu çıkar";
        }

        private static string GetProcessorCoolerStatusLabel(
            ProcessorCoolerSlotStatus status)
        {
            return status switch
            {
                ProcessorCoolerSlotStatus.ValidSeat => "4 NOKTA HİZALI",
                ProcessorCoolerSlotStatus.OutOfRange => "YAKLAŞ",
                ProcessorCoolerSlotStatus.NotFocused => "CPU SOKETİNİ HEDEFLE",
                ProcessorCoolerSlotStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                ProcessorCoolerSlotStatus.Obstructed => "MONTAJ ALANI ENGELLİ",
                ProcessorCoolerSlotStatus.Paused => "DURAKLATILDI",
                ProcessorCoolerSlotStatus.AuthorityBlocked => "HOST HAZIR DEĞİL",
                ProcessorCoolerSlotStatus.ValidSeatedUnsecuredRetentionBlocked =>
                    "HOST HAZIR DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private static ProcessorCoolerAssemblyItemBinding GetProcessorCoolerBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<ProcessorCoolerAssemblyItemBinding>()
                : null;
        }
    }
}
