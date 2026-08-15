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
        private string _activeCartId = string.Empty;
        private int _placementRotationQuarterTurns;

        public PhysicalItemProjection FocusedItem { get; private set; }

        public PhysicalItemProjection HeldItem { get; private set; }

        public TransportCartProjection FocusedCart { get; private set; }

        public TransportCartProjection ActiveCart { get; private set; }

        public string LastFailureCode { get; private set; } = string.Empty;

        public bool IsCarrying => HeldItem != null;

        public bool IsDrivingCart => ActiveCart != null && ActiveCart.IsDriven;

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
                    string stockState = GetStockStateSuffix(HeldItem);
                    string placement = input != null
                        ? input.PrimaryBindingPrompt
                        : "Mouse Left / RT";
                    string drop = input != null ? input.DropBindingPrompt : "G / B";
                    string rotate = input != null
                        ? input.RotatePlacementBindingPrompt
                        : "R / Right Shoulder";
                    if (HeldItem.CarryProfile == PhysicalCarryProfile.LargeBox)
                    {
                        string load = FocusedCart != null && FocusedCart.CanLoad(HeldItem)
                            ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                              $"{FocusedCart.DisplayName} üzerine yükle   |   "
                            : string.Empty;
                        string blocked = LastFailureCode.StartsWith(
                            "drop.",
                            StringComparison.Ordinal)
                            ? "   |   BIRAKMA ENGELLİ"
                            : string.Empty;
                        return load +
                               $"{drop}: {HeldItem.DisplayName} güvenli bırak   |   " +
                               $"AĞIR YÜK — sprint kapalı{blocked}{stockState}";
                    }

                    return IsPlacementMode
                        ? $"{drop}: yerleştir   |   {rotate}: 90° döndür " +
                          $"[{PlacementRotationDegrees:0}°]   |   {placement}: iptal   |   " +
                          (PlacementValid
                              ? (CurrentStackSupport != null ? "İSTİF GEÇERLİ" : "GEÇERLİ")
                              : "ENGELLİ") + stockState
                        : $"{placement}: yerleştirme önizlemesi   |   " +
                          $"{drop}: güvenli bırak{stockState}";
                }

                if (ActiveCart != null)
                {
                    string cargo = ActiveCart.HasCargo
                        ? $"YÜKLÜ: {ActiveCart.Cargo.DisplayName}"
                        : "BOŞ";
                    return $"{(input != null ? input.PrimaryBindingPrompt : "Mouse Left / RT")}: " +
                           $"arabayı bırak   |   {cargo}   |   sprint kapalı";
                }

                if (FocusedCart != null)
                {
                    string unload = FocusedCart.HasCargo
                        ? $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                          $"{FocusedCart.Cargo.DisplayName} yükünü al   |   "
                        : string.Empty;
                    string blocked = LastFailureCode.StartsWith(
                        "cart.",
                        StringComparison.Ordinal)
                        ? "   |   ARABA ENGELLİ"
                        : string.Empty;
                    return unload +
                           $"{(input != null ? input.PrimaryBindingPrompt : "Mouse Left / RT")}: " +
                           $"{FocusedCart.DisplayName} tut{blocked}";
                }

                if (FocusedItem == null)
                {
                    return string.Empty;
                }

                InventoryItemWorldBinding binding = GetInventoryBinding(FocusedItem);
                if (binding != null && binding.RequiresAcceptance)
                {
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"{FocusedItem.DisplayName} teslimatını kabul et   |   " +
                           binding.LocationLabel;
                }

                if (binding != null && binding.RequiresUnpacking)
                {
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"{FocusedItem.DisplayName} kolisini aç   |   " +
                           binding.LocationLabel;
                }

                if (binding != null && binding.RequiresShelfOffer)
                {
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"RAF A fiyatını yayınla • " +
                           $"{GarageStockFlowRuntime.PrototypePriceText}   |   " +
                           binding.LocationLabel;
                }

                if (binding != null && binding.IsCustomerReserved)
                {
                    if (binding.RequiresCheckoutCompletion)
                    {
                        return $"{(input != null ? input.PrimaryBindingPrompt : "Mouse Left / RT")}: " +
                               "satışı tamamla   |   " +
                               $"KASA: {binding.Runtime.CheckoutStatusText}   |   " +
                               "REZERVASYON KİLİTLİ";
                    }

                    return $"{(input != null ? input.PrimaryBindingPrompt : "Mouse Left / RT")}: " +
                           "kasayı başlat   |   " +
                           $"{(input != null ? input.DropBindingPrompt : "G / B")}: " +
                           "müşteri rezervasyonunu kaldır   |   MÜŞTERİ İÇİN AYRILDI";
                }

                if (binding != null && binding.RequiresCustomerReservation)
                {
                    return $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                           $"{FocusedItem.DisplayName} al   |   " +
                           $"{(input != null ? input.DropBindingPrompt : "G / B")}: " +
                           "demo müşteri için ayır   |   " +
                           binding.LocationLabel;
                }

                return FocusedItem.HasStackedItem
                    ? $"{FocusedItem.DisplayName}: önce üst kutuyu al"
                    : $"{(input != null ? input.InteractBindingPrompt : "E / A")}: " +
                      $"{FocusedItem.DisplayName} al{GetStockStateSuffix(FocusedItem)}";
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

            InventoryItemWorldBinding binding = GetInventoryBinding(item);
            if (binding != null && binding.RequiresAcceptance)
            {
                OperationResult acceptance = binding.TryAcceptDelivery();
                if (acceptance.IsSuccess)
                {
                    LastFailureCode = string.Empty;
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return Remember(acceptance);
            }

            if (binding != null && binding.RequiresUnpacking)
            {
                OperationResult unpack = binding.TryOpenParcel();
                if (unpack.IsSuccess)
                {
                    LastFailureCode = string.Empty;
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return Remember(unpack);
            }

            if (binding != null && binding.RequiresShelfOffer)
            {
                OperationResult publish = binding.TryPublishShelfOffer();
                if (publish.IsSuccess)
                {
                    LastFailureCode = string.Empty;
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return Remember(publish);
            }

            OperationResult authorityTransfer = binding != null
                ? binding.TryPreparePickupTransfer()
                : OperationResult.Success();
            if (authorityTransfer.IsFailure)
            {
                SetHandsState(VisibleHandsState.TargetFocused);
                return Remember(authorityTransfer);
            }

            OperationResult result = item.BeginCarry(carryAnchor, heldItemLayer);
            if (result.IsFailure)
            {
                RollbackAuthorityTransfer(binding);
                return Remember(result);
            }

            if (binding != null)
            {
                OperationResult commit = binding.CommitPreparedTransfer(targetIsWorld: false);
                if (commit.IsFailure)
                {
                    item.RecoverToLastSafePose();
                    RollbackAuthorityTransfer(binding);
                    return Remember(commit);
                }
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

        public OperationResult TryLoadHeldItem(TransportCartProjection cart)
        {
            if (HeldItem == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.load-nothing-held")));
            }

            if (cart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.load-no-target")));
            }

            PhysicalItemProjection item = HeldItem;
            OperationResult result = cart.TryLoad(item, heldItemLayer);
            if (result.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(result);
            }

            HeldItem = null;
            _heldItemId = string.Empty;
            FocusedItem = null;
            FocusedCart = cart;
            ResetPlacementState();
            motor?.ClearCarryProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.TargetFocused);
            return result;
        }

        public OperationResult TryUnloadCart(TransportCartProjection cart)
        {
            if (HeldItem != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("pickup.slot-occupied")));
            }

            if (cart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.unload-no-target")));
            }

            OperationResult<PhysicalItemProjection> result = cart.TryUnload(
                carryAnchor,
                heldItemLayer);
            if (result.IsFailure)
            {
                return Remember(OperationResult.Fail(result.Error));
            }

            HeldItem = result.Value;
            _heldItemId = HeldItem.ItemIdValue;
            FocusedItem = null;
            FocusedCart = null;
            ResetPlacementState();
            motor?.ApplyCarryProfile(HeldItem.CarryProfile);
            LastFailureCode = string.Empty;
            SetCarryHandsState(blocked: false);
            return OperationResult.Success();
        }

        public OperationResult TryBeginCartDrive(TransportCartProjection cart)
        {
            if (HeldItem != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-hands-occupied")));
            }

            if (ActiveCart != null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-slot-occupied")));
            }

            if (cart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-no-target")));
            }

            OperationResult result = cart.BeginDrive(transform);
            if (result.IsFailure)
            {
                return Remember(result);
            }

            ActiveCart = cart;
            _activeCartId = cart.CartIdValue;
            FocusedCart = cart;
            FocusedItem = null;
            motor?.ApplyTransportCartDriveProfile(cart.MovementSpeedMultiplier);
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.DrivingTransportCart);
            return result;
        }

        public OperationResult TryEndCartDrive()
        {
            if (ActiveCart == null)
            {
                return Remember(OperationResult.Fail(Failure.FromCode("cart.driver-inactive")));
            }

            TransportCartProjection cart = ActiveCart;
            OperationResult result = cart.EndDrive();
            if (result.IsFailure)
            {
                return Remember(result);
            }

            ActiveCart = null;
            _activeCartId = string.Empty;
            FocusedCart = cart;
            motor?.ClearTransportCartDriveProfile();
            LastFailureCode = string.Empty;
            SetHandsState(VisibleHandsState.TargetFocused);
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

            return ReleaseHeldItem(pose.Value, stabilizePlacement: false, placementSurface: null);
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
                evaluation.StackSupport,
                evaluation.Surface);
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

            if (ActiveCart == null && !string.IsNullOrEmpty(_activeCartId))
            {
                LastFailureCode = "cart.projection-missing";
                _activeCartId = string.Empty;
                motor?.ClearTransportCartDriveProfile();
                SetHandsState(VisibleHandsState.Recovering);
                Debug.LogError("CART_RECOVERY_FAILED code=cart.projection-missing");
                return;
            }

            if (ActiveCart != null && (!ActiveCart.isActiveAndEnabled || !ActiveCart.IsDriven))
            {
                ActiveCart = null;
                _activeCartId = string.Empty;
                FocusedCart = null;
                motor?.ClearTransportCartDriveProfile();
                LastFailureCode = "cart.driver-interrupted";
                SetHandsState(VisibleHandsState.Recovering);
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
                FocusedCart = null;
                if (HeldItem.CarryProfile == PhysicalCarryProfile.LargeBox)
                {
                    OperationResult<TransportCartProjection> cartTarget =
                        resolver.ResolveTransportCart();
                    FocusedCart = cartTarget.IsSuccess ? cartTarget.Value : null;
                    if (input.InteractPressedThisFrame && FocusedCart != null)
                    {
                        TryLoadHeldItem(FocusedCart);
                        return;
                    }
                }

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

            if (ActiveCart != null)
            {
                FocusedItem = null;
                FocusedCart = ActiveCart;
                SetHandsState(VisibleHandsState.DrivingTransportCart);
                if (input.PrimaryActionPressedThisFrame)
                {
                    TryEndCartDrive();
                    return;
                }

                OperationResult motion = ActiveCart.TryFollowDriver(supportMask, obstructionMask);
                if (motion.IsFailure)
                {
                    string failureCode = motion.Error.Code;
                    Debug.LogWarning($"TRANSPORT_CART_DRIVE_STOPPED code={failureCode}");
                    TransportCartProjection blockedCart = ActiveCart;
                    if (blockedCart.IsDriven)
                    {
                        blockedCart.EndDrive();
                    }

                    ActiveCart = null;
                    _activeCartId = string.Empty;
                    FocusedCart = blockedCart;
                    motor.ClearTransportCartDriveProfile();
                    Remember(OperationResult.Fail(Failure.FromCode(failureCode)));
                    SetHandsState(VisibleHandsState.TargetFocused);
                }

                return;
            }

            OperationResult<TransportCartProjection> resolvedCart = resolver.ResolveTransportCart();
            FocusedCart = resolvedCart.IsSuccess ? resolvedCart.Value : null;
            if (FocusedCart != null)
            {
                FocusedItem = null;
                SetHandsState(VisibleHandsState.TargetFocused);
                if (input.PrimaryActionPressedThisFrame)
                {
                    TryBeginCartDrive(FocusedCart);
                }
                else if (input.InteractPressedThisFrame && FocusedCart.HasCargo)
                {
                    TryUnloadCart(FocusedCart);
                }

                return;
            }

            OperationResult<PhysicalItemProjection> target = resolver.Resolve();
            FocusedItem = target.IsSuccess ? target.Value : null;
            SetHandsState(FocusedItem != null
                ? VisibleHandsState.TargetFocused
                : VisibleHandsState.Empty);

            InventoryItemWorldBinding focusedBinding = GetInventoryBinding(FocusedItem);
            if (focusedBinding != null &&
                focusedBinding.RequiresCheckoutCompletion &&
                input.PrimaryActionPressedThisFrame)
            {
                Remember(focusedBinding.TryCompleteCheckout());
                return;
            }

            if (focusedBinding != null &&
                focusedBinding.RequiresCheckoutStart &&
                input.PrimaryActionPressedThisFrame)
            {
                Remember(focusedBinding.TryBeginCheckout());
                return;
            }

            if (focusedBinding != null && input.DropPressedThisFrame)
            {
                if (focusedBinding.IsCustomerReserved)
                {
                    Remember(focusedBinding.TryReleaseCustomerReservation());
                    return;
                }

                if (focusedBinding.RequiresCustomerReservation)
                {
                    Remember(focusedBinding.TryReserveForCustomer());
                    return;
                }
            }

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

            InventoryItemWorldBinding binding = GetInventoryBinding(item);
            OperationResult authorityTransfer = binding != null
                ? binding.TryPrepareRecoveryTransfer()
                : OperationResult.Success();
            if (authorityTransfer.IsFailure)
            {
                return Remember(authorityTransfer);
            }

            OperationResult result = item.RecoverToLastSafePose();
            if (result.IsFailure)
            {
                RollbackAuthorityTransfer(binding);
                return Remember(result);
            }

            if (binding != null)
            {
                OperationResult commit = binding.CommitPreparedTransfer(targetIsWorld: true);
                if (commit.IsFailure)
                {
                    RollbackAuthorityTransfer(binding);
                    return Remember(commit);
                }
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
            PhysicalItemProjection stackSupport = null,
            PlacementSurface placementSurface = null)
        {
            PhysicalItemProjection releasedItem = HeldItem;
            InventoryItemWorldBinding binding = GetInventoryBinding(releasedItem);
            OperationResult authorityTransfer = binding == null
                ? OperationResult.Success()
                : stabilizePlacement
                    ? binding.TryPreparePlacementTransfer(placementSurface)
                    : binding.TryPrepareDropTransfer();
            if (authorityTransfer.IsFailure)
            {
                SetCarryHandsState(blocked: true);
                return Remember(authorityTransfer);
            }

            OperationResult result = stabilizePlacement
                ? releasedItem.PlaceAt(pose, stackSupport)
                : releasedItem.ReleaseTo(pose);
            if (result.IsFailure)
            {
                RollbackAuthorityTransfer(binding);
                return Remember(result);
            }

            if (binding != null)
            {
                OperationResult commit = binding.CommitPreparedTransfer(targetIsWorld: true);
                if (commit.IsFailure)
                {
                    RollbackAuthorityTransfer(binding);
                    return Remember(commit);
                }
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
            if (Application.isPlaying && !_applicationQuitting && ActiveCart != null)
            {
                if (ActiveCart.IsDriven)
                {
                    ActiveCart.EndDrive();
                }

                ActiveCart = null;
                _activeCartId = string.Empty;
                FocusedCart = null;
                motor?.ClearTransportCartDriveProfile();
            }

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

        private static InventoryItemWorldBinding GetInventoryBinding(PhysicalItemProjection item)
        {
            return item != null ? item.GetComponent<InventoryItemWorldBinding>() : null;
        }

        private static string GetStockStateSuffix(PhysicalItemProjection item)
        {
            InventoryItemWorldBinding binding = GetInventoryBinding(item);
            return binding != null ? $"   |   {binding.LocationLabel}" : string.Empty;
        }

        private static void RollbackAuthorityTransfer(InventoryItemWorldBinding binding)
        {
            if (binding == null || !binding.HasPreparedTransfer)
            {
                return;
            }

            OperationResult rollback = binding.RollbackPreparedTransfer();
            if (rollback.IsFailure)
            {
                Debug.LogError($"STOCK_PROJECTION_ROLLBACK_FAILED code={rollback.Error.Code}");
            }
        }
    }
}
