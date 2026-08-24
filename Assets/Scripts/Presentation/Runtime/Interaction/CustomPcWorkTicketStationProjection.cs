using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.Retail;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed class CustomPcWorkTicketStationIdScope : IStableIdScope
    {
    }

    public static class CustomPcWorkTicketStationFailures
    {
        public static readonly Failure ConfigurationMissing =
            Failure.FromCode("custom-pc-work-ticket-station.configuration-missing");
        public static readonly Failure Paused =
            Failure.FromCode("custom-pc-work-ticket-station.paused");
        public static readonly Failure QuoteMissing =
            Failure.FromCode("custom-pc-work-ticket-station.quote-missing");
        public static readonly Failure AlreadyIssued =
            Failure.FromCode("custom-pc-work-ticket-station.already-issued");
        public static readonly Failure HandsBusy =
            Failure.FromCode("custom-pc-work-ticket-station.hands-busy");
        public static readonly Failure OutOfRange =
            Failure.FromCode("custom-pc-work-ticket-station.out-of-range");
        public static readonly Failure FocusMissing =
            Failure.FromCode("custom-pc-work-ticket-station.focus-missing");
        public static readonly Failure LineOfSightBlocked =
            Failure.FromCode("custom-pc-work-ticket-station.line-of-sight-blocked");
        public static readonly Failure InputReplay =
            Failure.FromCode("custom-pc-work-ticket-station.input-replay");
        public static readonly Failure CanonicalStationMismatch =
            Failure.FromCode("custom-pc-work-ticket-station.canonical-station-mismatch");
        public static readonly Failure CanonicalStationRegistrationConflict =
            Failure.FromCode(
                "custom-pc-work-ticket-station.canonical-station-registration-conflict");
    }

    /// <summary>
    /// Fixed physical projection for the first custom-PC work ticket. It owns no order,
    /// reservation, component, or Assembly state; it only exposes the authoritative one-shot
    /// work-order allocation through a focused world interaction.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(60)]
    public sealed class CustomPcWorkTicketStationProjection : MonoBehaviour
    {
        public const string PrototypeStationIdValue =
            "world.custom-pc-work-ticket-station.garage-001";
        public const float DefaultInteractionRange = 2.25f;
        public const float DefaultFocusDegrees = 24f;
        private const float FeedbackRangePadding = 0.75f;
        private const float FeedbackFocusPadding = 8f;

        [SerializeField] private string stationId = PrototypeStationIdValue;
        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerCarryController playerCarry;
        [SerializeField] private Collider interactionCollider;
        [SerializeField] private TextMesh stationStatusText;
        [SerializeField, Min(0.1f)] private float interactionRange =
            DefaultInteractionRange;
        [SerializeField, Range(1f, 80f)] private float focusDegrees =
            DefaultFocusDegrees;

        private bool _isFocused;
        private bool _hasContextualAttention;
        private Failure _focusFailure = CustomPcWorkTicketStationFailures.FocusMissing;
        private int _lastSuccessfulOperationFrame = -1;
        private GarageStockFlowSession _pendingIssueSession;
        private int _pendingIssueFrame = -1;
        private bool _pendingIssueArmed;

        public StableId<CustomPcWorkTicketStationIdScope> StationId =>
            StableId<CustomPcWorkTicketStationIdScope>.Parse(stationId);

        public string StationIdValue => stationId;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public PlayerInputAdapter PlayerInput => playerInput;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public Camera PlayerCamera => playerCamera;

        public PlayerCarryController PlayerCarry => playerCarry;

        public Collider InteractionCollider => interactionCollider;

        public TextMesh StationStatusText => stationStatusText;

        public float InteractionRange => interactionRange;

        public float FocusDegrees => focusDegrees;

        public bool IsFocused => _isFocused;

        public bool HasContextualAttention => _hasContextualAttention;

        public string LastFailureCode { get; private set; } = string.Empty;

        public bool HasQuote
        {
            get
            {
                GarageStockFlowSession session = ResolveSession();
                return session != null &&
                       session.TryGetPrototypeCustomPcQuote(out _);
            }
        }

        public bool IsIssued
        {
            get
            {
                GarageStockFlowSession session = ResolveSession();
                return session != null &&
                       session.TryGetPrototypeCustomPcWorkTicket(out _);
            }
        }

        public bool HasPendingAction => HasQuote && !IsIssued;

        public bool CanIssue => ValidateInteractionGate().IsSuccess;

        public string PromptText
        {
            get
            {
                RefreshFocusState();
                if (playerMotor == null || playerMotor.IsPaused)
                {
                    return string.Empty;
                }

                if (IsIssued)
                {
                    return _isFocused
                        ? "İŞ EMRİ • 10/10 PARÇA AYRILDI • MONTAJA HAZIR • HENÜZ BAŞLAMADI"
                        : string.Empty;
                }

                if (!HasQuote)
                {
                    return _isFocused
                        ? "İŞ EMRİ PANOSU • ÖZEL PC TEKLİFİ BEKLENİYOR"
                        : string.Empty;
                }

                if (!_isFocused)
                {
                    return _hasContextualAttention
                        ? $"İŞ EMRİ ENGELLİ • {_focusFailure.Code}"
                        : string.Empty;
                }

                if (PlayerIsBusy())
                {
                    return "İŞ EMRİ ENGELLİ • ELLERİNİ BOŞALT";
                }

                string interact = playerInput != null
                    ? playerInput.InteractBindingPrompt
                    : "E / A";
                return $"{interact}: 10/10 PARÇA İÇİN İŞ EMRİNİ ÇIKAR";
            }
        }

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            PlayerInputAdapter input,
            FirstPersonMotor motor,
            Camera camera,
            PlayerCarryController carry,
            Collider targetCollider,
            TextMesh statusText,
            string stableStationId = PrototypeStationIdValue)
        {
            stockFlow = garageStockFlow != null
                ? garageStockFlow
                : throw new ArgumentNullException(nameof(garageStockFlow));
            playerInput = input != null
                ? input
                : throw new ArgumentNullException(nameof(input));
            playerMotor = motor != null
                ? motor
                : throw new ArgumentNullException(nameof(motor));
            playerCamera = camera != null
                ? camera
                : throw new ArgumentNullException(nameof(camera));
            playerCarry = carry != null
                ? carry
                : throw new ArgumentNullException(nameof(carry));
            interactionCollider = targetCollider != null
                ? targetCollider
                : throw new ArgumentNullException(nameof(targetCollider));
            stationStatusText = statusText != null
                ? statusText
                : throw new ArgumentNullException(nameof(statusText));
            stationId = StableId<CustomPcWorkTicketStationIdScope>.Parse(
                stableStationId).Value;
            RefreshPresentation();
        }

        public void ProcessInputFrame()
        {
            RefreshFocusState();
            if (playerInput == null || playerMotor == null || playerMotor.IsPaused ||
                playerInput.PausePressedThisFrame || !HasPendingAction ||
                !_isFocused || PlayerIsBusy() ||
                !playerInput.TryConsumeInteractPressThisFrame())
            {
                return;
            }

            OperationResult gate = ValidateInteractionGate();
            Remember(gate.IsFailure ? gate : TryIssueAuthorized());
            RefreshPresentation();
        }

        internal OperationResult InspectInteractionGateForTests()
        {
            return ValidateInteractionGate();
        }

        internal bool TryConsumePhysicalIssueAuthorization(
            GarageStockFlowSession session,
            int frame)
        {
            bool valid = _pendingIssueArmed &&
                         ReferenceEquals(_pendingIssueSession, session) &&
                         _pendingIssueFrame == frame;
            ClearPhysicalIssueAuthorization();
            return valid;
        }

        public void RefreshPresentation()
        {
            RefreshFocusState();
            if (stationStatusText == null)
            {
                return;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null)
            {
                stationStatusText.text = "ÖZEL PC İŞ EMRİ\nAUTHORITY EKSİK";
                return;
            }

            if (session.TryGetPrototypeCustomPcWorkTicket(
                    out CustomPcWorkTicketRecord ticket))
            {
                stationStatusText.text =
                    "İŞ EMRİ • DEMO-GAMING-001\n" +
                    $"{ticket.ReservedSerializedItemCount}/10 PARÇA AYRILDI\n" +
                    "MONTAJA HAZIR • HENÜZ BAŞLAMADI";
                return;
            }

            if (session.TryGetPrototypeCustomPcQuote(out CustomPcQuoteRecord quote))
            {
                stationStatusText.text =
                    $"{quote.ReservedSerializedItemCount}/10 PARÇA AYRILDI\n" +
                    "İŞ EMRİNİ ÇIKAR";
                return;
            }

            stationStatusText.text = "ÖZEL PC İŞ EMRİ\nTEKLİF BEKLENİYOR";
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

        private GarageStockFlowSession ResolveSession()
        {
            return stockFlow != null ? stockFlow.EnsureInitialized() : null;
        }

        private OperationResult TryIssueAuthorized()
        {
            if (_lastSuccessfulOperationFrame == Time.frameCount)
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures.InputReplay);
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetPrototypeCustomPcQuote(out CustomPcQuoteRecord quote))
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures.QuoteMissing);
            }

            OperationResult<CustomPcWorkOrderIssueResult> issued;
            ArmPhysicalIssueAuthorization(session);
            try
            {
                issued = session.IssueFromPhysicalWorkTicket(this, quote.QuotedAt);
            }
            finally
            {
                ClearPhysicalIssueAuthorization();
            }
            if (issued.IsFailure)
            {
                return OperationResult.Fail(issued.Error);
            }

            _lastSuccessfulOperationFrame = Time.frameCount;
            return OperationResult.Success();
        }

        private void ArmPhysicalIssueAuthorization(GarageStockFlowSession session)
        {
            _pendingIssueSession = session;
            _pendingIssueFrame = Time.frameCount;
            _pendingIssueArmed = true;
        }

        private void ClearPhysicalIssueAuthorization()
        {
            _pendingIssueSession = null;
            _pendingIssueFrame = -1;
            _pendingIssueArmed = false;
        }

        private OperationResult ValidateInteractionGate()
        {
            if (stockFlow == null || playerInput == null || playerMotor == null ||
                playerCamera == null || playerCarry == null ||
                interactionCollider == null)
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures.ConfigurationMissing);
            }

            if (playerMotor.IsPaused)
            {
                return OperationResult.Fail(CustomPcWorkTicketStationFailures.Paused);
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null || !session.TryGetPrototypeCustomPcQuote(out _))
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures.QuoteMissing);
            }

            if (session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures.AlreadyIssued);
            }

            if (PlayerIsBusy())
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures.HandsBusy);
            }

            RefreshFocusState();
            return _isFocused
                ? OperationResult.Success()
                : OperationResult.Fail(_focusFailure);
        }

        private bool PlayerIsBusy()
        {
            return playerCarry != null &&
                   (playerCarry.IsCarrying ||
                    playerCarry.IsDrivingCart ||
                    playerCarry.HasAssemblyPromptOwnership);
        }

        private void RefreshFocusState()
        {
            _isFocused = false;
            _hasContextualAttention = false;
            _focusFailure = CustomPcWorkTicketStationFailures.FocusMissing;
            if (playerCamera == null || interactionCollider == null ||
                !interactionCollider.enabled ||
                !interactionCollider.gameObject.activeInHierarchy)
            {
                _focusFailure =
                    CustomPcWorkTicketStationFailures.ConfigurationMissing;
                return;
            }

            Vector3 origin = playerCamera.transform.position;
            Vector3 target = interactionCollider.bounds.center;
            Vector3 toTarget = target - origin;
            float distance = toTarget.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                _focusFailure = CustomPcWorkTicketStationFailures.OutOfRange;
                return;
            }

            Vector3 direction = toTarget / distance;
            Vector3 cameraForward = playerCamera.transform.forward;
            float angle = Vector3.Angle(cameraForward, direction);
            _hasContextualAttention =
                distance <= interactionRange + FeedbackRangePadding &&
                angle <= focusDegrees + FeedbackFocusPadding;
            if (distance > interactionRange)
            {
                _focusFailure = CustomPcWorkTicketStationFailures.OutOfRange;
                return;
            }

            if (angle > focusDegrees)
            {
                _focusFailure = CustomPcWorkTicketStationFailures.FocusMissing;
                return;
            }

            if (!Physics.Raycast(
                    origin,
                    cameraForward,
                    out RaycastHit hit,
                    interactionRange,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Collide) ||
                hit.collider != interactionCollider)
            {
                _focusFailure =
                    CustomPcWorkTicketStationFailures.LineOfSightBlocked;
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
