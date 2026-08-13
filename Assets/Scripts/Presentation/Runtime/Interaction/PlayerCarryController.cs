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
        [SerializeField] private LayerMask supportMask;
        [SerializeField] private LayerMask obstructionMask;
        [SerializeField] private int heldItemLayer;

        private bool _applicationQuitting;
        private string _heldItemId = string.Empty;

        public PhysicalItemProjection FocusedItem { get; private set; }

        public PhysicalItemProjection HeldItem { get; private set; }

        public string LastFailureCode { get; private set; } = string.Empty;

        public bool IsCarrying => HeldItem != null;

        public string PromptText
        {
            get
            {
                if (HeldItem != null)
                {
                    return $"{(input != null ? input.DropBindingPrompt : "G / B")}: {HeldItem.DisplayName} bırak";
                }

                return FocusedItem != null
                    ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: {FocusedItem.DisplayName} al"
                    : string.Empty;
            }
        }

        public void Configure(
            PlayerInputAdapter inputAdapter,
            FirstPersonMotor playerMotor,
            PhysicalInteractionResolver interactionResolver,
            Transform itemCarryAnchor,
            VisibleHandsPresenter handsPresenter,
            LayerMask groundLayers,
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
            supportMask = groundLayers;
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
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.CarryingSmallItem);
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
                LastFailureCode = pose.Error.Code;
                SetHandsState(VisibleHandsState.DropBlocked);
                return OperationResult.Fail(pose.Error);
            }

            PhysicalItemProjection releasedItem = HeldItem;
            OperationResult result = releasedItem.ReleaseTo(pose.Value);
            if (result.IsFailure)
            {
                return Remember(result);
            }

            Physics.SyncTransforms();
            HeldItem = null;
            _heldItemId = string.Empty;
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.Empty);
            return result;
        }

        public void ProcessInputFrame()
        {
            if (HeldItem == null && !string.IsNullOrEmpty(_heldItemId))
            {
                LastFailureCode = "carry.projection-missing";
                _heldItemId = string.Empty;
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
                return;
            }

            bool wasCarryingAtFrameStart = HeldItem != null;
            if (wasCarryingAtFrameStart)
            {
                if (input.DropPressedThisFrame)
                {
                    TryDrop();
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

        private void LateUpdate()
        {
            ProcessInputFrame();
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
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.Empty);
            return result;
        }

        private void OnDisable()
        {
            if (Application.isPlaying && !_applicationQuitting && HeldItem != null)
            {
                TryRecoverHeldItem();
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
    }
}
