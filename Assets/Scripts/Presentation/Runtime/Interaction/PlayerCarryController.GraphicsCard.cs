using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class PlayerCarryController
    {
        private static readonly Vector3 GraphicsCardSeatPreviewSize =
            new Vector3(0.285f, 0.064f, 0.125f);

        [SerializeField] private GraphicsCardSlotProjection graphicsCardSlot;
        [SerializeField] private GraphicsCardAssemblyItemBinding graphicsCardBinding;

        public bool IsGraphicsCardSeatMode { get; private set; }

        public GraphicsCardSlotStatus CurrentGraphicsCardSlotStatus
        {
            get;
            private set;
        } = GraphicsCardSlotStatus.ContextMissing;

        public bool HasGraphicsCardSlotContext { get; private set; }

        public bool IsGraphicsCardSlotFocused { get; private set; }

        public void ConfigureGraphicsCardSlot(
            GraphicsCardSlotProjection slotProjection,
            GraphicsCardAssemblyItemBinding assemblyBinding)
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
                    "The graphics-card binding must own the configured slot.",
                    nameof(assemblyBinding));
            }

            graphicsCardSlot = slotProjection;
            graphicsCardBinding = assemblyBinding;
            graphicsCardBinding.SyncProjectionToAuthority();
        }

        public bool MatchesGraphicsCardConfiguration(
            GraphicsCardSlotProjection slotProjection,
            GraphicsCardAssemblyItemBinding assemblyBinding)
        {
            return slotProjection != null &&
                   assemblyBinding != null &&
                   graphicsCardSlot == slotProjection &&
                   graphicsCardBinding == assemblyBinding &&
                   assemblyBinding.Slot == slotProjection;
        }

        public OperationResult TryOperateGraphicsCardRetention()
        {
            if (graphicsCardSlot == null || graphicsCardBinding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.context-missing")));
            }

            GraphicsCardSlotEvaluation evaluation =
                EvaluateGraphicsCardSlotInteraction();
            ApplyGraphicsCardSlotEvaluation(evaluation);
            if (!evaluation.CanOperateRetention)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode(evaluation.FailureCode)));
            }

            OperationResult result = graphicsCardBinding.TryOperateRetention();
            if (result.IsSuccess)
            {
                UpdateGraphicsCardSlotFocus();
            }

            return Remember(result);
        }

        public OperationResult TrySetGraphicsCardSeatMode(bool enabled)
        {
            GraphicsCardAssemblyItemBinding binding =
                GetGraphicsCardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.nothing-held")));
            }

            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.paused")));
            }

            SetGraphicsCardSeatMode(enabled);
            if (enabled)
            {
                UpdateGraphicsCardSeatPreview(binding);
            }

            return Remember(OperationResult.Success());
        }

        public OperationResult TryRotateGraphicsCardSeatPreview()
        {
            GraphicsCardAssemblyItemBinding binding =
                GetGraphicsCardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.nothing-held")));
            }

            if (!IsGraphicsCardSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.mode-inactive")));
            }

            _placementRotationQuarterTurns =
                (_placementRotationQuarterTurns + 1) % 2;
            LastFailureCode = string.Empty;
            UpdateGraphicsCardSeatPreview(binding);
            return OperationResult.Success();
        }

        public OperationResult TryConfirmGraphicsCardSeat()
        {
            GraphicsCardAssemblyItemBinding binding =
                GetGraphicsCardBinding(HeldItem);
            if (HeldItem == null || binding == null)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.nothing-held")));
            }

            if (!IsGraphicsCardSeatMode)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.mode-inactive")));
            }

            return TryConfirmGraphicsCardSeat(
                binding,
                EvaluateGraphicsCardSeat(binding));
        }

        private OperationResult TryConfirmGraphicsCardSeat(
            GraphicsCardAssemblyItemBinding binding,
            GraphicsCardSlotEvaluation evaluation)
        {
            ApplyGraphicsCardSeatEvaluation(evaluation);
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

        private bool ProcessHeldGraphicsCardInput()
        {
            GraphicsCardAssemblyItemBinding binding =
                GetGraphicsCardBinding(HeldItem);
            if (binding == null)
            {
                return false;
            }

            GraphicsCardBuildKitEvaluation buildKitEvaluation =
                EvaluateGraphicsCardBuildKit(binding);
            bool buildKitOwnsPrimary =
                IsGraphicsCardBuildKitMode ||
                (graphicsCardBuildKit != null &&
                 graphicsCardBuildKit.HasPickupReceipt);

            if (input.TryConsumePrimaryActionPressThisFrame())
            {
                input.TryConsumeRotatePlacementPressThisFrame();
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                if (buildKitOwnsPrimary)
                {
                    TrySetGraphicsCardBuildKitMode(
                        !IsGraphicsCardBuildKitMode);
                }
                else
                {
                    TrySetGraphicsCardSeatMode(!IsGraphicsCardSeatMode);
                }

                return true;
            }

            if (IsGraphicsCardBuildKitMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateGraphicsCardBuildKitPreviewClockwise();
                return true;
            }

            if (IsGraphicsCardSeatMode &&
                input.TryConsumeRotatePlacementPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                input.TryConsumeDropPressThisFrame();
                TryRotateGraphicsCardSeatPreview();
                return true;
            }

            if (IsGraphicsCardBuildKitMode)
            {
                ApplyGraphicsCardBuildKitEvaluation(buildKitEvaluation);
            }
            else
            {
                UpdateGraphicsCardSeatPreview(binding);
            }

            if (input.TryConsumeDropPressThisFrame())
            {
                input.TryConsumeInteractPressThisFrame();
                if (IsGraphicsCardBuildKitMode)
                {
                    TryConfirmGraphicsCardBuildKit();
                }
                else if (IsGraphicsCardSeatMode)
                {
                    GraphicsCardSlotEvaluation evaluation =
                        EvaluateGraphicsCardSeat(binding);
                    ApplyGraphicsCardSeatEvaluation(evaluation);
                    TryConfirmGraphicsCardSeat(binding, evaluation);
                }
                else
                {
                    TryDrop();
                }
            }

            return true;
        }

        private void SetGraphicsCardSeatMode(bool enabled)
        {
            ResetGraphicsCardBuildKitState();
            ResetPowerSupplyBuildKitState();
            IsGraphicsCardSeatMode = enabled &&
                                        HeldItem != null &&
                                        GetGraphicsCardBinding(HeldItem) != null;
            ResetPcieGpuPowerCableState();
            IsPlacementMode = false;
            IsMotherboardSeatMode = false;
            IsProcessorSeatMode = false;
            IsDimmSeatMode = false;
            IsM2StorageSeatMode = false;
            IsProcessorCoolerSeatMode = false;
            IsPowerSupplySeatMode = false;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            CurrentGraphicsCardSlotStatus =
                GraphicsCardSlotStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsGraphicsCardSeatMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                graphicsCardSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdateGraphicsCardSeatPreview(
            GraphicsCardAssemblyItemBinding binding)
        {
            if (!IsGraphicsCardSeatMode || HeldItem == null)
            {
                PlacementValid = false;
                CurrentPlacementStatus = PlacementStatus.ContextMissing;
                CurrentGraphicsCardSlotStatus =
                    GraphicsCardSlotStatus.ContextMissing;
                CurrentStackSupport = null;
                placementPreview?.Hide();
                graphicsCardSlot?.ResetFeedback();
                SetCarryHandsState(blocked: false);
                return;
            }

            ApplyGraphicsCardSeatEvaluation(
                EvaluateGraphicsCardSeat(binding));
        }

        private GraphicsCardSlotEvaluation EvaluateGraphicsCardSeat(
            GraphicsCardAssemblyItemBinding binding)
        {
            GraphicsCardSlotProjection slotProjection =
                binding?.Slot ?? graphicsCardSlot;
            if (slotProjection == null)
            {
                return new GraphicsCardSlotEvaluation(
                    GraphicsCardSlotStatus.ContextMissing,
                    default,
                    false,
                    default);
            }

            binding?.SyncProjectionToAuthority();
            return slotProjection.EvaluateSeat(
                IsGraphicsCardSeatMode,
                resolver != null ? resolver.Origin : null,
                transform,
                HeldItem,
                obstructionMask,
                _placementRotationQuarterTurns,
                motor == null || motor.IsPaused,
                binding != null &&
                    binding.IsAuthorityInHands &&
                    binding.IsHostReady &&
                    !binding.IsSeated,
                binding != null
                    ? binding.CardInterface
                    : GraphicsCardPcieInterface.Unknown,
                binding != null && binding.HasChassisClearance,
                binding != null && binding.HasCoolerClearance);
        }

        private void ApplyGraphicsCardSeatEvaluation(
            GraphicsCardSlotEvaluation evaluation)
        {
            CurrentGraphicsCardSlotStatus = evaluation.Status;
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
                    GraphicsCardSeatPreviewSize);
            }
            else
            {
                placementPreview?.Hide();
            }

            SetCarryHandsState(blocked: !evaluation.CanSeat);
        }

        private void UpdateGraphicsCardSlotFocus()
        {
            if (graphicsCardSlot == null || graphicsCardBinding == null)
            {
                ResetGraphicsCardSlotFocus();
                return;
            }

            graphicsCardBinding.SyncProjectionToAuthority();
            ApplyGraphicsCardSlotEvaluation(
                EvaluateGraphicsCardSlotInteraction());
        }

        private GraphicsCardSlotEvaluation
            EvaluateGraphicsCardSlotInteraction()
        {
            return graphicsCardSlot.EvaluateInteraction(
                true,
                resolver != null ? resolver.Origin : null,
                transform,
                graphicsCardBinding != null
                    ? graphicsCardBinding.PhysicalItem.transform
                    : null,
                obstructionMask,
                motor == null || motor.IsPaused,
                graphicsCardBinding != null &&
                    graphicsCardBinding.IsSeated,
                graphicsCardBinding != null &&
                    (graphicsCardBinding.IsRetained ||
                     graphicsCardBinding.IsHostReady));
        }

        private void ApplyGraphicsCardSlotEvaluation(
            GraphicsCardSlotEvaluation evaluation)
        {
            CurrentGraphicsCardSlotStatus = evaluation.Status;
            IsGraphicsCardSlotFocused =
                evaluation.CanOperateRetention || evaluation.CanRemove;
            HasGraphicsCardSlotContext = evaluation.HasOwnedContext;
            if (!IsGraphicsCardSlotFocused && HasGraphicsCardSlotContext)
            {
                LastFailureCode = evaluation.FailureCode;
            }
        }

        private void ResetGraphicsCardSlotFocus()
        {
            IsGraphicsCardSlotFocused = false;
            HasGraphicsCardSlotContext = false;
            CurrentGraphicsCardSlotStatus =
                GraphicsCardSlotStatus.ContextMissing;
            graphicsCardSlot?.ResetFeedback();
        }

        private bool ProcessGraphicsCardSlotInput()
        {
            if (!IsGraphicsCardSlotFocused &&
                !HasGraphicsCardSlotContext)
            {
                return false;
            }

            FocusedCart = null;
            FocusedItem = graphicsCardBinding.PhysicalItem;
            SetHandsState(VisibleHandsState.TargetFocused);
            if (IsGraphicsCardSlotFocused)
            {
                if (input.TryConsumePrimaryActionPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeInteractPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryOperateGraphicsCardRetention();
                    return true;
                }

                if (input.TryConsumeInteractPressThisFrame())
                {
                    input.TryConsumeRotatePlacementPressThisFrame();
                    input.TryConsumeDropPressThisFrame();
                    TryPickup(graphicsCardBinding.PhysicalItem);
                }

                return true;
            }

            bool primaryPressed = input.TryConsumePrimaryActionPressThisFrame();
            input.TryConsumeRotatePlacementPressThisFrame();
            input.TryConsumeInteractPressThisFrame();
            input.TryConsumeDropPressThisFrame();
            if (primaryPressed)
            {
                TryOperateGraphicsCardRetention();
            }

            return true;
        }

        private bool ProcessLooseGraphicsCardPickupInput()
        {
            if (resolver == null ||
                graphicsCardBinding == null ||
                graphicsCardBinding.IsSeated ||
                !graphicsCardBinding.IsAuthorityLooseWorld)
            {
                return false;
            }

            OperationResult<PhysicalItemProjection> target = resolver.Resolve();
            if (target.IsFailure ||
                target.Value != graphicsCardBinding.PhysicalItem)
            {
                return false;
            }

            ResetGraphicsCardSlotFocus();
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

        private OperationResult TryPickupGraphicsCard(
            PhysicalItemProjection item,
            GraphicsCardAssemblyItemBinding binding)
        {
            if (motor != null && motor.IsPaused)
            {
                return Remember(OperationResult.Fail(
                    Failure.FromCode("assembly-graphics-card.paused")));
            }

            if (binding.IsRetained)
            {
                return Remember(OperationResult.Fail(
                    AssemblyFailures.GraphicsCardRetained));
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
                : binding.IsAuthorityInBuildKit
                    ? binding.TryCommitBuildKitAssemblyPickup()
                    : binding.TryCommitLoosePickup();
            if (authority.IsFailure)
            {
                OperationResult rollback = item.RecoverToLastSafePose();
                if (rollback.IsFailure)
                {
                    Debug.LogError(
                        $"GRAPHICS_CARD_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
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

        private string GetHeldGraphicsCardPrompt(
            GraphicsCardAssemblyItemBinding binding,
            string primary,
            string drop,
            string rotate)
        {
            if (!IsGraphicsCardSeatMode)
            {
                return $"{primary}: ekran kartını hizala • " +
                       $"{drop}: güvenli bırak • PCIe x16";
            }

            string state = GetGraphicsCardStatusLabel(
                CurrentGraphicsCardSlotStatus);
            string orientation = _placementRotationQuarterTurns == 0
                ? "0°"
                : "180°";
            return PlacementValid
                ? $"[OK] PCIe x16 ANAHTARI HİZALI • YÖN {orientation} • " +
                  $"{drop}: oturt • {rotate}: 180° döndür • {primary}: çık"
                : $"[X] {state} • {rotate}: 180° döndür • {primary}: çık";
        }

        private string GetGraphicsCardSlotPrompt()
        {
            string primary = input != null
                ? input.PrimaryBindingPrompt
                : "Mouse Left / RT";
            string interact = input != null
                ? input.InteractBindingPrompt
                : "E / A";
            if (!IsGraphicsCardSlotFocused)
            {
                return CurrentGraphicsCardSlotStatus switch
                {
                    GraphicsCardSlotStatus.LineOfSightBlocked =>
                        "[X] EKRAN KARTI ENGELLİ • görüş hattını aç",
                    GraphicsCardSlotStatus.ChassisClearanceBlocked =>
                        "[X] KASA AÇIKLIĞI YETERSİZ",
                    GraphicsCardSlotStatus.CoolerClearanceBlocked =>
                        "[X] SOĞUTUCU EKRAN KARTI ALANINI ENGELLİYOR",
                    GraphicsCardSlotStatus.Obstructed =>
                        "[X] EKRAN KARTI ALANI ENGELLİ • önünü aç",
                    _ => "[X] EKRAN KARTI BAĞLANTISI KULLANILAMIYOR"
                };
            }

            if (CurrentGraphicsCardSlotStatus ==
                GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked)
            {
                return $"[GEVŞEK] EKRAN KARTI OTURDU • HOST HAZIR DEĞİL • " +
                       $"{interact}: ekran kartını çıkar";
            }

            return graphicsCardBinding.IsRetained
                ? $"[SABİT] PCIe MANDALI + ARKA BRAKET KİLİTLİ • {primary}: " +
                  $"gevşet • {interact}: çıkarma kilitli"
                : $"[GEVŞEK] EKRAN KARTI OTURDU • {primary}: " +
                  $"mandal + braketi sabitle • {interact}: ekran kartını çıkar";
        }

        private static string GetGraphicsCardStatusLabel(
            GraphicsCardSlotStatus status)
        {
            return status switch
            {
                GraphicsCardSlotStatus.ValidSeat => "PCIe x16 HİZALI",
                GraphicsCardSlotStatus.OutOfRange => "YAKLAŞ",
                GraphicsCardSlotStatus.NotFocused => "PCIe x16 SLOTUNU HEDEFLE",
                GraphicsCardSlotStatus.LineOfSightBlocked => "ÖNÜNÜ AÇ",
                GraphicsCardSlotStatus.InterfaceInvalid => "PCIe TİPİ UYUMSUZ",
                GraphicsCardSlotStatus.OrientationInvalid => "YÖN TERS",
                GraphicsCardSlotStatus.Unsupported => "SLOT DESTEĞİ YOK",
                GraphicsCardSlotStatus.ChassisClearanceBlocked =>
                    "KASA AÇIKLIĞI YETERSİZ",
                GraphicsCardSlotStatus.CoolerClearanceBlocked =>
                    "SOĞUTUCU AÇIKLIĞI YETERSİZ",
                GraphicsCardSlotStatus.Obstructed => "MONTAJ ALANI ENGELLİ",
                GraphicsCardSlotStatus.Paused => "DURAKLATILDI",
                GraphicsCardSlotStatus.AuthorityBlocked => "HOST HAZIR DEĞİL",
                GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked =>
                    "HOST HAZIR DEĞİL",
                _ => "BAĞLANTI YOK"
            };
        }

        private static GraphicsCardAssemblyItemBinding GetGraphicsCardBinding(
            PhysicalItemProjection item)
        {
            return item != null
                ? item.GetComponent<GraphicsCardAssemblyItemBinding>()
                : null;
        }
    }
}
