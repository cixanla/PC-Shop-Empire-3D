using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class PlayerCarryController : MonoBehaviour
    {
        [SerializeField] private PlayerInputAdapter input;
        [SerializeField] private FirstPersonMotor motor;
        [SerializeField] private PhysicalInteractionResolver resolver;
        [SerializeField] private Transform carryAnchor;
        [SerializeField] private VisibleHandsPresenter hands;
        [SerializeField] private PlacementPreview placementPreview;
        [SerializeField] private LayerMask supportMask;
        [SerializeField] private LayerMask stackSupportMask;
        [SerializeField] private LayerMask obstructionMask;
        [SerializeField] private int heldItemLayer;

        private bool _applicationQuitting;
        private string _heldItemId = string.Empty;
        private int _placementRotationQuarterTurns;

        public PhysicalItemProjection FocusedItem { get; private set; }

        public PhysicalItemProjection HeldItem { get; private set; }

        public string LastFailureCode { get; private set; } = string.Empty;

        public bool IsCarrying => HeldItem != null;

        public bool IsPlacementMode { get; private set; }

        public bool PlacementValid { get; private set; }

        public PlacementStatus CurrentPlacementStatus { get; private set; } = PlacementStatus.ContextMissing;

        public PlacementPreview PlacementPreview => placementPreview;

        public PhysicalItemProjection CurrentStackSupport { get; private set; }

        public int PlacementRotationQuarterTurns => _placementRotationQuarterTurns;

        public float PlacementRotationDegrees => _placementRotationQuarterTurns * 90f;

        public string PromptText
        {
            get
            {
                if (HeldItem != null)
                {
                    string placement = input != null
                        ? input.PrimaryBindingPrompt
                        : "Mouse Left / RT";
                    string drop = input != null ? input.DropBindingPrompt : "G / B";
                    string rotate = input != null
                        ? input.RotatePlacementBindingPrompt
                        : "R / Right Shoulder";
                    if (HeldItem.CarryProfile == PhysicalCarryProfile.LargeBox)
                    {
                        string blocked = LastFailureCode.StartsWith(
                            "drop.",
                            StringComparison.Ordinal)
                            ? "   |   BIRAKMA ENGELLİ"
                            : string.Empty;
                        return $"{drop}: {HeldItem.DisplayName} güvenli bırak   |   " +
                               $"AĞIR YÜK — sprint kapalı{blocked}";
                    }

                    return IsPlacementMode
                        ? $"{drop}: yerleştir   |   {rotate}: 90° döndür " +
                          $"[{PlacementRotationDegrees:0}°]   |   {placement}: iptal   |   " +
                          (PlacementValid
                              ? (CurrentStackSupport != null ? "İSTİF GEÇERLİ" : "GEÇERLİ")
                              : "ENGELLİ")
                        : $"{placement}: yerleştirme önizlemesi   |   {drop}: güvenli bırak";
                }

                return FocusedItem != null
                    ? (FocusedItem.HasStackedItem
                        ? $"{FocusedItem.DisplayName}: önce üst kutuyu al"
                        : $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                          $"{FocusedItem.DisplayName} al")
                    : string.Empty;
            }
        }

        public void Configure(
            PlayerInputAdapter inputAdapter,
            FirstPersonMotor playerMotor,
            PhysicalInteractionResolver interactionResolver,
            Transform itemCarryAnchor,
            VisibleHandsPresenter handsPresenter,
            PlacementPreview preview,
            LayerMask groundLayers,
            LayerMask stackingLayers,
            LayerMask blockingLayers,
            int heldLayer)
        {
            input = inputAdapter != null ? inputAdapter : throw new ArgumentNullException(nameof(inputAdapter));
            motor = playerMotor != null ? playerMotor : throw new ArgumentNullException(nameof(playerMotor));
            resolver = interactionResolver != null
                ? interactionResolver
                : throw new ArgumentNullException(nameof(interactionResolver));
            carryAnchor = itemCarryAnchor != null
                ? itemCarryAnchor
                : throw new ArgumentNullException(nameof(itemCarryAnchor));
            hands = handsPresenter != null
                ? handsPresenter
                : throw new ArgumentNullException(nameof(handsPresenter));
            placementPreview = preview != null ? preview : throw new ArgumentNullException(nameof(preview));
            supportMask = groundLayers;
            stackSupportMask = stackingLayers;
            obstructionMask = blockingLayers;
            heldItemLayer = heldLayer;
        }

        public OperationResult TryPickup(PhysicalItemProjection item)
        {
            if (item == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("pickup.no-target")));
            }

            if (HeldItem != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("pickup.slot-occupied")));
            }

            OperationResult result = item.BeginCarry(carryAnchor, heldItemLayer);
            if (result.IsFailure)
            {
                return Remember(result);
            }

            HeldItem = item;
            _heldItemId = item.ItemIdValue;
            FocusedItem = null;
            ResetPlacementState();
            LastFailureCode = string.Empty;
            motor.ApplyCarryProfile(item.CarryProfile);
            SetCarryHandsState(blocked: false);
            return result;
        }

        public OperationResult TryDrop()
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("drop.nothing-held")));
            }

            OperationResult<Pose> pose = SafeDropSolver.FindPose(
                transform,
                HeldItem,
                supportMask,
                obstructionMask);
            if (pose.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(OperationResult.Fail(pose.Error));
            }

            return ReleaseHeldItem(pose.Value, stabilizePlacement: false);
        }

        public OperationResult TryConfirmPlacement()
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("placement.nothing-held")));
            }

            if (!HeldItem.SupportsPlacement)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("placement.profile-unsupported")));
            }

            if (!IsPlacementMode)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("placement.mode-inactive")));
            }

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                transform,
                HeldItem,
                supportMask,
                obstructionMask,
                _placementRotationQuarterTurns,
                stackSupportMask);
            ApplyPlacementEvaluation(evaluation);
            if (!evaluation.IsValid)
            {
                return Remember(OperationResult.Fail(Failure.FromCode(evaluation.FailureCode)));
            }

            return ReleaseHeldItem(
                evaluation.Pose,
                stabilizePlacement: true,
                evaluation.StackSupport);
        }

        public void ProcessInputFrame()
        {
            if (HeldItem == null && !string.IsNullOrEmpty(_heldItemId))
            {
                LastFailureCode = "carry.projection-missing";
                _heldItemId = string.Empty;
                ResetPlacementState();
                motor?.ClearCarryProfile();
                SetHandsState(VisibleHandsState.Recovering);
                Debug.LogError("CARRY_RECOVERY_FAILED code=carry.projection-missing");
                return;
            }

            if (HeldItem != null && (!HeldItem.isActiveAndEnabled || !HeldItem.IsCarried))
            {
                TryRecoverHeldItem();
                return;
            }

            if (input == null || motor == null || resolver == null || motor.IsPaused)
            {
                placementPreview?.Hide();
                return;
            }

            if (HeldItem != null)
            {
                if (HeldItem.SupportsPlacement && input.PrimaryActionPressedThisFrame)
                {
                    SetPlacementMode(!IsPlacementMode);
                }

                if (IsPlacementMode && input.RotatePlacementPressedThisFrame)
                {
                    _placementRotationQuarterTurns = (_placementRotationQuarterTurns + 1) % 4;
                    LastFailureCode = string.Empty;
                }

                UpdatePlacementPreview();
                if (input.DropPressedThisFrame)
                {
                    if (IsPlacementMode)
                    {
                        TryConfirmPlacement();
                    }
                    else
                    {
                        TryDrop();
                    }
                }

                return;
            }

            OperationResult<PhysicalItemProjection> target = resolver.Resolve();
            FocusedItem = target.IsSuccess ? target.Value : null;
            SetHandsState(FocusedItem != null
                ? VisibleHandsState.TargetFocused
                : VisibleHandsState.Empty);

            if (input.InteractPressedThisFrame)
            {
                TryPickup(FocusedItem);
            }
        }

        public OperationResult TryRecoverHeldItem()
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("carry.nothing-held")));
            }

            PhysicalItemProjection item = HeldItem;
            SetHandsState(VisibleHandsState.Recovering);
            if (!item.gameObject.activeSelf)
            {
                item.gameObject.SetActive(true);
            }

            if (!item.enabled)
            {
                item.enabled = true;
            }

            OperationResult result = item.RecoverToLastSafePose();
            if (result.IsFailure)
            {
                return Remember(result);
            }

            HeldItem = null;
            _heldItemId = string.Empty;
            ResetPlacementState();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.Empty);
            return result;
        }

        private void LateUpdate()
        {
            ProcessInputFrame();
        }

        private OperationResult ReleaseHeldItem(
            Pose pose,
            bool stabilizePlacement,
            PhysicalItemProjection stackSupport = null)
        {
            PhysicalItemProjection releasedItem = HeldItem;
            OperationResult result = stabilizePlacement
                ? releasedItem.PlaceAt(pose, stackSupport)
                : releasedItem.ReleaseTo(pose);
            if (result.IsFailure)
            {
                return Remember(result);
            }

            Physics.SyncTransforms();
            HeldItem = null;
            _heldItemId = string.Empty;
            ResetPlacementState();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.Empty);
            return result;
        }

        private void SetPlacementMode(bool enabled)
        {
            IsPlacementMode = enabled && HeldItem != null && HeldItem.SupportsPlacement;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            LastFailureCode = string.Empty;
            if (!IsPlacementMode)
            {
                _placementRotationQuarterTurns = 0;
                placementPreview?.Hide();
                SetCarryHandsState(blocked: false);
            }
        }

        private void UpdatePlacementPreview()
        {
            if (!IsPlacementMode || HeldItem == null)
            {
                PlacementValid = false;
                placementPreview?.Hide();
                return;
            }

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                transform,
                HeldItem,
                supportMask,
                obstructionMask,
                _placementRotationQuarterTurns,
                stackSupportMask);
            ApplyPlacementEvaluation(evaluation);
        }

        private void ApplyPlacementEvaluation(PlacementEvaluation evaluation)
        {
            CurrentPlacementStatus = evaluation.Status;
            PlacementValid = evaluation.IsValid;
            CurrentStackSupport = evaluation.StackSupport;
            LastFailureCode = evaluation.IsValid ? string.Empty : evaluation.FailureCode;
            placementPreview?.Show(HeldItem, evaluation);
            SetCarryHandsState(blocked: !evaluation.IsValid);
        }

        private void ResetPlacementState()
        {
            IsPlacementMode = false;
            _placementRotationQuarterTurns = 0;
            PlacementValid = false;
            CurrentStackSupport = null;
            CurrentPlacementStatus = PlacementStatus.ContextMissing;
            placementPreview?.Hide();
        }

        private void OnDisable()
        {
            placementPreview?.Hide();
            if (Application.isPlaying && !_applicationQuitting && HeldItem != null)
            {
                TryRecoverHeldItem();
            }
            else if (Application.isPlaying && !_applicationQuitting)
            {
                motor?.ClearCarryProfile();
            }
        }

        private void OnApplicationQuit()
        {
            _applicationQuitting = true;
        }

        private OperationResult Remember(OperationResult result)
        {
            LastFailureCode = result.IsFailure ? result.Error.Code : string.Empty;
            return result;
        }

        private void SetHandsState(VisibleHandsState state)
        {
            if (hands != null)
            {
                hands.SetState(state);
            }
        }

        private void SetCarryHandsState(bool blocked)
        {
            bool carryingLarge = HeldItem != null &&
                                 HeldItem.CarryProfile == PhysicalCarryProfile.LargeBox;
            SetHandsState(carryingLarge
                ? (blocked ? VisibleHandsState.LargeDropBlocked : VisibleHandsState.CarryingLargeItem)
                : (blocked ? VisibleHandsState.DropBlocked : VisibleHandsState.CarryingSmallItem));
        }
    }
}
