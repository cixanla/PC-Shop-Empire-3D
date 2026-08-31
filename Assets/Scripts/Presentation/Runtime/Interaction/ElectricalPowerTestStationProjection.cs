using System;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Player;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public static class ElectricalPowerTestStationFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "electrical-power-test-station.configuration-missing");
        public static readonly Failure RuntimeNotReady = Failure.FromCode(
            "electrical-power-test-station.runtime-not-ready");
        public static readonly Failure Paused = Failure.FromCode(
            "electrical-power-test-station.paused");
        public static readonly Failure HandsBusy = Failure.FromCode(
            "electrical-power-test-station.hands-busy");
        public static readonly Failure CompetingInteractOwner = Failure.FromCode(
            "electrical-power-test-station.competing-interact-owner");
        public static readonly Failure OutOfRange = Failure.FromCode(
            "electrical-power-test-station.out-of-range");
        public static readonly Failure FocusMissing = Failure.FromCode(
            "electrical-power-test-station.focus-missing");
        public static readonly Failure LineOfSightBlocked = Failure.FromCode(
            "electrical-power-test-station.line-of-sight-blocked");
        public static readonly Failure InputReplay = Failure.FromCode(
            "electrical-power-test-station.input-replay");
    }

    /// <summary>
    /// Focused player command surface for one exact power-test preflight. The existing
    /// readiness display remains the presentation observer; this component owns only the
    /// interaction gate and delegates the immutable receipt to PowerTestAttemptAuthority.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(68)]
    public sealed class ElectricalPowerTestStationProjection : MonoBehaviour
    {
        public const float DefaultInteractionRange = 2.25f;
        public const float DefaultFocusDegrees = 24f;
        private const float FeedbackRangePadding = 0.75f;
        private const float FeedbackFocusPadding = 8f;
        private const float LineOfSightEndpointPadding = 0.025f;
        private const int LineOfSightHitCapacity = 16;

        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private PlayerInputAdapter playerInput;
        [SerializeField] private FirstPersonMotor playerMotor;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private PlayerCarryController playerCarry;
        [SerializeField]
        private ElectricalReadinessWorkbenchProjection readinessProjection;
        [SerializeField] private Transform focusAnchor;
        [SerializeField, Min(0.1f)] private float interactionRange =
            DefaultInteractionRange;
        [SerializeField, Range(1f, 80f)] private float focusDegrees =
            DefaultFocusDegrees;

        private bool _isFocused;
        private bool _hasContextualAttention;
        private Failure _focusFailure =
            ElectricalPowerTestStationFailures.FocusMissing;
        private int _lastSuccessfulOperationFrame = -1;
        private readonly RaycastHit[] _lineOfSightHits =
            new RaycastHit[LineOfSightHitCapacity];
        private int _promptCacheFrame = -1;
        private string _promptCache = string.Empty;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public PlayerInputAdapter PlayerInput => playerInput;

        public FirstPersonMotor PlayerMotor => playerMotor;

        public Camera PlayerCamera => playerCamera;

        public PlayerCarryController PlayerCarry => playerCarry;

        public ElectricalReadinessWorkbenchProjection ReadinessProjection =>
            readinessProjection;

        public Transform FocusAnchor => focusAnchor;

        public float InteractionRange => interactionRange;

        public float FocusDegrees => focusDegrees;

        public bool IsFocused => _isFocused;

        public bool HasContextualAttention => _hasContextualAttention;

        public string LastFailureCode { get; private set; } = string.Empty;

        public bool IsConfigured => stockFlow != null && playerInput != null &&
                                    playerMotor != null && playerCamera != null &&
                                    playerCarry != null &&
                                    readinessProjection != null &&
                                    focusAnchor != null;

        public string PromptText
        {
            get
            {
                if (_promptCacheFrame != Time.frameCount)
                {
                    _promptCache = BuildPromptText();
                    _promptCacheFrame = Time.frameCount;
                }

                return _promptCache;
            }
        }

        public void Configure(
            GarageStockFlowRuntime garageStockFlow,
            PlayerInputAdapter input,
            FirstPersonMotor motor,
            Camera camera,
            PlayerCarryController carry,
            ElectricalReadinessWorkbenchProjection workbenchProjection,
            Transform logicalFocusAnchor)
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
            readinessProjection = workbenchProjection != null
                ? workbenchProjection
                : throw new ArgumentNullException(nameof(workbenchProjection));
            focusAnchor = logicalFocusAnchor != null
                ? logicalFocusAnchor
                : throw new ArgumentNullException(nameof(logicalFocusAnchor));
            InvalidatePromptCache();
            RefreshFocusState();
        }

        public void ProcessInputFrame()
        {
            InvalidatePromptCache();
            RefreshFocusState();
            if (playerInput == null || playerMotor == null || playerMotor.IsPaused ||
                playerInput.PausePressedThisFrame ||
                !playerInput.InteractPressedThisFrame)
            {
                return;
            }

            OperationResult gate = ValidateInteractionGate();
            if (gate.IsFailure)
            {
                Remember(gate);
                readinessProjection?.RefreshPresentation();
                return;
            }

            if (!playerInput.TryConsumeInteractPressThisFrame())
            {
                return;
            }

            Remember(TryAttemptAuthorized());
            readinessProjection.RefreshPresentation();
        }

        internal OperationResult InspectInteractionGateForTests()
        {
            return ValidateInteractionGate();
        }

        internal OperationResult TryAttemptAuthorizedForTests()
        {
            return Remember(TryAttemptAuthorized());
        }

        private void Update()
        {
            ProcessInputFrame();
        }

        private GarageStockFlowSession ResolveSession()
        {
            return stockFlow != null ? stockFlow.EnsureInitialized() : null;
        }

        private OperationResult TryAttemptAuthorized()
        {
            if (_lastSuccessfulOperationFrame == Time.frameCount)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.InputReplay);
            }

            GarageStockFlowSession session = ResolveSession();
            PowerTestAttemptAuthority attempts = session?.PowerTestAttempts;
            if (attempts == null)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.RuntimeNotReady);
            }

            OperationResult<PowerTestAttemptContext> observed =
                attempts.ObserveCurrentContext();
            if (observed.IsFailure)
            {
                return OperationResult.Fail(observed.Error);
            }

            OperationResult<PowerTestAttemptReceipt> attempted =
                attempts.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    observed.Value,
                    attempts.Revision);
            if (attempted.IsFailure)
            {
                return OperationResult.Fail(attempted.Error);
            }

            _lastSuccessfulOperationFrame = Time.frameCount;
            return OperationResult.Success();
        }

        private OperationResult ValidateInteractionGate()
        {
            if (!IsConfigured)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.ConfigurationMissing);
            }

            if (playerMotor.IsPaused)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.Paused);
            }

            if (PlayerIsBusy())
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.HandsBusy);
            }

            if (PlayerHasCompetingWorldInteractOwner())
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.CompetingInteractOwner);
            }

            RefreshFocusState();
            if (!_isFocused)
            {
                return OperationResult.Fail(_focusFailure);
            }

            GarageStockFlowSession session = ResolveSession();
            PowerTestAttemptAuthority attempts = session?.PowerTestAttempts;
            if (attempts == null)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.RuntimeNotReady);
            }

            if (attempts.HasCompletedPreflight)
            {
                OperationResult<PowerTestAttemptReceipt> current =
                    attempts.EvaluateCurrentReceipt();
                return current.IsSuccess
                    ? OperationResult.Fail(PowerTestAttemptFailures.AlreadyCompleted)
                    : OperationResult.Fail(current.Error);
            }

            OperationResult<PowerTestAttemptContext> context =
                attempts.ObserveCurrentContext();
            if (context.IsFailure)
            {
                return OperationResult.Fail(context.Error);
            }

            return context.Value.IsSufficient
                ? OperationResult.Success()
                : OperationResult.Fail(
                    PowerTestAttemptFailures.PowerSupplyInsufficient);
        }

        private bool PlayerIsBusy()
        {
            return playerCarry != null &&
                   (playerCarry.IsCarrying ||
                    playerCarry.IsDrivingCart ||
                    playerCarry.HasAssemblyPromptOwnership);
        }

        private bool PlayerHasCompetingWorldInteractOwner()
        {
            return playerCarry != null &&
                   playerCarry.HasCompetingWorldInteractOwner;
        }

        private string BuildPromptText()
        {
            RefreshFocusState();
            if (playerMotor == null || playerMotor.IsPaused)
            {
                return string.Empty;
            }

            if (!_isFocused)
            {
                return _hasContextualAttention
                    ? $"GÜÇ TESTİ ENGELLİ • {_focusFailure.Code}"
                    : string.Empty;
            }

            if (PlayerIsBusy())
            {
                return "GÜÇ TESTİ ENGELLİ • ELLERİNİ BOŞALT";
            }

            if (PlayerHasCompetingWorldInteractOwner())
            {
                return "GÜÇ TESTİ ENGELLİ • ODAKTAKİ NESNE ÖNCELİKLİ";
            }

            GarageStockFlowSession session = ResolveSession();
            PowerTestAttemptAuthority attempts = session?.PowerTestAttempts;
            if (attempts == null)
            {
                return "GÜÇ TESTİ ENGELLİ • AUTHORITY HAZIR DEĞİL";
            }

            if (attempts.HasCompletedPreflight)
            {
                OperationResult<PowerTestAttemptReceipt> current =
                    attempts.EvaluateCurrentReceipt();
                return current.IsSuccess
                    ? "ÖN KONTROL GEÇTİ • POWER-ON BEKLİYOR"
                    : $"ÖN KONTROL GEÇERSİZ • {current.Error.Code}";
            }

            OperationResult<PowerTestAttemptContext> context =
                attempts.ObserveCurrentContext();
            if (context.IsFailure)
            {
                return $"GÜÇ TESTİ ENGELLİ • {context.Error.Code}";
            }

            if (!context.Value.IsSufficient)
            {
                return "GÜÇ TESTİ ENGELLİ • PSU YETERSİZ";
            }

            string interact = playerInput != null
                ? playerInput.InteractBindingPrompt
                : "E / A";
            return $"{interact}: GÜÇ TESTİ ÖN KONTROLÜNÜ ÇALIŞTIR";
        }

        private void InvalidatePromptCache()
        {
            _promptCacheFrame = -1;
            _promptCache = string.Empty;
        }

        private void RefreshFocusState()
        {
            _isFocused = false;
            _hasContextualAttention = false;
            _focusFailure = ElectricalPowerTestStationFailures.FocusMissing;
            if (playerCamera == null || focusAnchor == null ||
                !focusAnchor.gameObject.activeInHierarchy)
            {
                _focusFailure =
                    ElectricalPowerTestStationFailures.ConfigurationMissing;
                return;
            }

            Vector3 origin = playerCamera.transform.position;
            Vector3 toTarget = focusAnchor.position - origin;
            float distance = toTarget.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                _focusFailure = ElectricalPowerTestStationFailures.OutOfRange;
                return;
            }

            Vector3 direction = toTarget / distance;
            float angle = Vector3.Angle(
                playerCamera.transform.forward,
                direction);
            _hasContextualAttention =
                distance <= interactionRange + FeedbackRangePadding &&
                angle <= focusDegrees + FeedbackFocusPadding;
            if (distance > interactionRange)
            {
                _focusFailure = ElectricalPowerTestStationFailures.OutOfRange;
                return;
            }

            if (angle > focusDegrees)
            {
                _focusFailure = ElectricalPowerTestStationFailures.FocusMissing;
                return;
            }

            float rayDistance = Mathf.Max(
                0f,
                distance - LineOfSightEndpointPadding);
            if (rayDistance > Mathf.Epsilon &&
                HasLineOfSightObstruction(origin, direction, rayDistance))
            {
                _focusFailure =
                    ElectricalPowerTestStationFailures.LineOfSightBlocked;
                return;
            }

            _isFocused = true;
        }

        private bool HasLineOfSightObstruction(
            Vector3 origin,
            Vector3 direction,
            float distance)
        {
            int hitCount = Physics.RaycastNonAlloc(
                origin,
                direction,
                _lineOfSightHits,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            if (hitCount >= LineOfSightHitCapacity)
            {
                return true;
            }

            Transform playerRoot = playerMotor != null
                ? playerMotor.transform
                : null;
            for (int index = 0; index < hitCount; index++)
            {
                Collider collider = _lineOfSightHits[index].collider;
                if (collider == null)
                {
                    continue;
                }

                Transform candidate = collider.transform;
                if (playerRoot != null &&
                    (candidate == playerRoot || candidate.IsChildOf(playerRoot)))
                {
                    continue;
                }

                return true;
            }

            return false;
        }

        private OperationResult Remember(OperationResult result)
        {
            InvalidatePromptCache();
            LastFailureCode = result.IsFailure ? result.Error.Code : string.Empty;
            return result;
        }
    }
}
