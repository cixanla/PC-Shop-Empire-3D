using System;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed class CheckoutStationIdScope : IStableIdScope
    {
    }

    public static class CheckoutStationFailures
    {
        public static readonly Failure ConfigurationMissing =
            Failure.FromCode("checkout-station.configuration-missing");
        public static readonly Failure CustomerNotAwaitingCheckout =
            Failure.FromCode("checkout-station.customer-not-awaiting-checkout");
        public static readonly Failure Paused =
            Failure.FromCode("checkout-station.paused");
        public static readonly Failure OutOfRange =
            Failure.FromCode("checkout-station.out-of-range");
        public static readonly Failure FocusMissing =
            Failure.FromCode("checkout-station.focus-missing");
        public static readonly Failure LineOfSightBlocked =
            Failure.FromCode("checkout-station.line-of-sight-blocked");
        public static readonly Failure CheckoutNotReady =
            Failure.FromCode("checkout-station.checkout-not-ready");
        public static readonly Failure InputReplay =
            Failure.FromCode("checkout-station.input-replay");
    }

    /// <summary>
    /// Physical-world gate for the Garage prototype's existing checkout and exact-cash
    /// settlement authorities. It owns no commerce state and never invents customer provenance.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(150)]
    public sealed class CheckoutStationProjection : MonoBehaviour
    {
        public const string PrototypeStationIdValue =
            "world.checkout-station.garage-001";
        public const float DefaultInteractionRange = 2.75f;
        public const float DefaultFocusDegrees = 24f;
        private const float FeedbackRangePadding = 1.25f;
        private const float FeedbackFocusPadding = 8f;

        [SerializeField] private string stationId = PrototypeStationIdValue;
        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private GarageCustomerFlowRuntime customerFlow;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private TextMesh stationStatusText;
        [SerializeField, Min(0.1f)] private float interactionRange = DefaultInteractionRange;
        [SerializeField, Range(1f, 80f)] private float focusDegrees = DefaultFocusDegrees;

        private bool _isFocused;
        private bool _hasContextualAttention;
        private Failure _focusFailure = CheckoutStationFailures.FocusMissing;
        private int _lastSuccessfulOperationFrame = -1;

        public StableId<CheckoutStationIdScope> StationId =>
            StableId<CheckoutStationIdScope>.Parse(stationId);

        public string StationIdValue => stationId;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public GarageCustomerFlowRuntime CustomerFlow => customerFlow;

        public PlayerInputAdapter PlayerInput => playerInput;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public Camera PlayerCamera => playerCamera;

        public Collider InteractionCollider => interactionCollider;

        public TextMesh StationStatusText => stationStatusText;

        public float InteractionRange => interactionRange;

        public float FocusDegrees => focusDegrees;

        public bool IsFocused => _isFocused;

        public bool HasContextualAttention => _hasContextualAttention;

        public string LastFailureCode { get; private set; } = string.Empty;

        public bool HasPendingAction
        {
            get
            {
                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                if (session == null || session.TryGetPrototypeCheckoutSettlement(out _))
                {
                    return false;
                }

                InventoryItemWorldBinding binding = stockFlow.ItemBinding;
                return binding != null &&
                       (binding.RequiresCheckoutStart || binding.RequiresCheckoutCompletion);
            }
        }

        public bool CanOperate
        {
            get
            {
                return ValidateInteractionGate().IsSuccess &&
                       HasPendingAction;
            }
        }

        public string PromptText
        {
            get
            {
                RefreshFocusState();
                if (playerMotor == null || playerMotor.IsPaused)
                {
                    return string.Empty;
                }

                if (!_isFocused)
                {
                    return _hasContextualAttention && HasPendingAction
                        ? $"KASA ENGELLİ • {_focusFailure.Code}"
                        : string.Empty;
                }

                GarageStockFlowSession session = stockFlow != null
                    ? stockFlow.EnsureInitialized()
                    : null;
                if (session == null)
                {
                    return $"KASA ENGELLİ • {CheckoutStationFailures.ConfigurationMissing.Code}";
                }

                if (session.TryGetPrototypeCheckoutSettlement(out _))
                {
                    return "KASA İSTASYONU • NAKİT ALINDI";
                }

                OperationResult customerGate = ValidateCustomerGate();
                if (customerGate.IsFailure)
                {
                    return $"KASA ENGELLİ • {customerGate.Error.Code}";
                }

                InventoryItemWorldBinding binding = stockFlow.ItemBinding;
                string primary = playerInput != null
                    ? playerInput.PrimaryBindingPrompt
                    : "Mouse Left / RT";
                if (binding != null && binding.RequiresCheckoutStart)
                {
                    return $"{primary}: KASAYI BAŞLAT • FİZİKSEL KASA";
                }

                if (binding != null && binding.RequiresCheckoutCompletion)
                {
                    return $"{primary}: NAKİT ÖDEMEYİ AL • " +
                           stockFlow.CheckoutStatusText;
                }

                return $"KASA ENGELLİ • {CheckoutStationFailures.CheckoutNotReady.Code}";
            }
        }

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            GarageCustomerFlowRuntime garageCustomerFlow,
            PlayerInputAdapter input,
            FirstPersonMotor motor,
            Camera camera,
            Collider targetCollider,
            TextMesh statusText,
            string stableStationId = PrototypeStationIdValue)
        {
            stockFlow = garageStockFlow != null
                ? garageStockFlow
                : throw new ArgumentNullException(nameof(garageStockFlow));
            customerFlow = garageCustomerFlow != null
                ? garageCustomerFlow
                : throw new ArgumentNullException(nameof(garageCustomerFlow));
            playerInput = input != null
                ? input
                : throw new ArgumentNullException(nameof(input));
            playerMotor = motor != null
                ? motor
                : throw new ArgumentNullException(nameof(motor));
            playerCamera = camera != null
                ? camera
                : throw new ArgumentNullException(nameof(camera));
            interactionCollider = targetCollider != null
                ? targetCollider
                : throw new ArgumentNullException(nameof(targetCollider));
            stationStatusText = statusText != null
                ? statusText
                : throw new ArgumentNullException(nameof(statusText));
            stationId = StableId<CheckoutStationIdScope>.Parse(stableStationId).Value;
            RefreshPresentation();
        }

        public void ProcessInputFrame()
        {
            RefreshFocusState();
            if (playerInput == null || playerMotor == null || playerMotor.IsPaused ||
                (!_isFocused && !_hasContextualAttention) || !HasPendingAction ||
                !playerInput.TryConsumePrimaryActionPressThisFrame())
            {
                return;
            }

            OperationResult interactionGate = ValidateInteractionGate();
            Remember(interactionGate.IsFailure
                ? interactionGate
                : TryOperateAuthorized());
            RefreshPresentation();
        }

        internal OperationResult TryOperate()
        {
            OperationResult interactionGate = ValidateInteractionGate();
            if (interactionGate.IsFailure)
            {
                return Remember(interactionGate);
            }

            OperationResult result = TryOperateAuthorized();
            RefreshPresentation();
            return Remember(result);
        }

        public void RefreshPresentation()
        {
            RefreshFocusState();
            if (stationStatusText == null)
            {
                return;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (session == null)
            {
                stationStatusText.text = "KASA İSTASYONU\nAUTHORITY EKSİK";
                return;
            }

            if (session.TryGetPrototypeCheckoutSettlement(out _))
            {
                stationStatusText.text = "KASA İSTASYONU\nNAKİT ALINDI";
                return;
            }

            OperationResult customerGate = ValidateCustomerGate();
            if (customerGate.IsFailure)
            {
                stationStatusText.text = "KASA İSTASYONU\nMÜŞTERİYİ BEKLİYOR";
                return;
            }

            InventoryItemWorldBinding binding = stockFlow.ItemBinding;
            stationStatusText.text = binding != null && binding.RequiresCheckoutCompletion
                ? "KASA İSTASYONU\nNAKİT ÖDEMEYİ AL"
                : binding != null && binding.RequiresCheckoutStart
                    ? "KASA İSTASYONU\nKASAYI BAŞLAT"
                    : "KASA İSTASYONU\nİŞLEM HAZIR DEĞİL";
        }

        private void Awake()
        {
            RefreshPresentation();
        }

        private void Update()
        {
            ProcessInputFrame();
            RefreshPresentation();
        }

        private OperationResult TryOperateFocused()
        {
            if (stockFlow == null || stockFlow.ItemBinding == null)
            {
                return OperationResult.Fail(CheckoutStationFailures.ConfigurationMissing);
            }

            GarageStockFlowSession session = stockFlow.EnsureInitialized();
            OperationResult customerGate = ValidateCustomerGate();
            if (customerGate.IsFailure)
            {
                return customerGate;
            }

            if (session.TryGetPrototypeCheckoutSettlement(out _))
            {
                return OperationResult.Success();
            }

            InventoryItemWorldBinding binding = stockFlow.ItemBinding;
            if (binding.RequiresCheckoutStart)
            {
                return binding.TryBeginCheckout();
            }

            if (binding.RequiresCheckoutCompletion)
            {
                return binding.TrySettleCashCheckout();
            }

            return OperationResult.Fail(CheckoutStationFailures.CheckoutNotReady);
        }

        private OperationResult TryOperateAuthorized()
        {
            if (_lastSuccessfulOperationFrame == Time.frameCount)
            {
                return OperationResult.Fail(CheckoutStationFailures.InputReplay);
            }

            OperationResult result = TryOperateFocused();
            if (result.IsSuccess)
            {
                _lastSuccessfulOperationFrame = Time.frameCount;
            }

            return result;
        }

        private OperationResult ValidateCustomerGate()
        {
            if (stockFlow == null || customerFlow == null)
            {
                return OperationResult.Fail(CheckoutStationFailures.ConfigurationMissing);
            }

            GarageStockFlowSession session = stockFlow.EnsureInitialized();
            CustomerVisitRecord projectedVisit = customerFlow.CurrentVisit;
            if (!session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit) ||
                projectedVisit == null ||
                visit.Id != session.PrototypeCustomerVisitId ||
                visit.Intent.CustomerId != session.PrototypeActorCustomerId ||
                visit.Intent.Id != session.PrototypeCustomerIntentId ||
                projectedVisit.Id != visit.Id ||
                projectedVisit.Intent.CustomerId != visit.Intent.CustomerId ||
                projectedVisit.Intent.Id != visit.Intent.Id ||
                projectedVisit.State != visit.State ||
                visit.State != CustomerVisitState.AwaitingCheckout)
            {
                return OperationResult.Fail(
                    CheckoutStationFailures.CustomerNotAwaitingCheckout);
            }

            return OperationResult.Success();
        }

        private OperationResult ValidateInteractionGate()
        {
            if (stockFlow == null || customerFlow == null || playerInput == null ||
                playerMotor == null || playerCamera == null || interactionCollider == null)
            {
                return OperationResult.Fail(CheckoutStationFailures.ConfigurationMissing);
            }

            if (playerMotor.IsPaused)
            {
                return OperationResult.Fail(CheckoutStationFailures.Paused);
            }

            RefreshFocusState();
            return _isFocused
                ? ValidateCustomerGate()
                : OperationResult.Fail(_focusFailure);
        }

        private void RefreshFocusState()
        {
            _isFocused = false;
            _hasContextualAttention = false;
            _focusFailure = CheckoutStationFailures.FocusMissing;
            if (playerCamera == null || interactionCollider == null ||
                !interactionCollider.enabled || !interactionCollider.gameObject.activeInHierarchy)
            {
                _focusFailure = CheckoutStationFailures.ConfigurationMissing;
                return;
            }

            Vector3 origin = playerCamera.transform.position;
            Vector3 target = interactionCollider.bounds.center;
            Vector3 toTarget = target - origin;
            float distance = toTarget.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                _focusFailure = CheckoutStationFailures.OutOfRange;
                return;
            }

            Vector3 direction = toTarget / distance;
            float angle = Vector3.Angle(playerCamera.transform.forward, direction);
            _hasContextualAttention =
                distance <= interactionRange + FeedbackRangePadding &&
                angle <= focusDegrees + FeedbackFocusPadding;
            if (distance > interactionRange)
            {
                _focusFailure = CheckoutStationFailures.OutOfRange;
                return;
            }

            if (angle > focusDegrees)
            {
                _focusFailure = CheckoutStationFailures.FocusMissing;
                return;
            }

            if (!Physics.Raycast(
                    origin,
                    direction,
                    out RaycastHit hit,
                    interactionRange,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore) ||
                hit.collider != interactionCollider)
            {
                _focusFailure = CheckoutStationFailures.LineOfSightBlocked;
                return;
            }

            _isFocused = true;
        }

        private OperationResult Remember(OperationResult result)
        {
            LastFailureCode = result.IsFailure ? result.Error.Code : string.Empty;
            return result;
        }
    }
}
