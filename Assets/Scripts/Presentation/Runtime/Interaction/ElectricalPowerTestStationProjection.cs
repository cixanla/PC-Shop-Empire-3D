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
    /// Focused player command surface for one exact power-test preflight, its safe
    /// Off/Energized transition, bounded post-POST UEFI baseline review/save and the
    /// exact-storage fictional OS and driver installation commands. The existing
    /// readiness display remains the presentation observer; this component owns only
    /// interaction gates and delegates immutable receipts.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(68)]
    public sealed partial class ElectricalPowerTestStationProjection : MonoBehaviour
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
        private bool _isReviewingFirmwareBaseline;
        private PcPostStartupReceipt _reviewedPostStartupReceipt;
        private bool _isReviewingFictionalOsInstallation;
        private PcFirmwareBaselineReceipt _reviewedFirmwareBaselineReceipt;
        private bool _isReviewingFictionalDriverInstallation;
        private PcFictionalOsInstallationReceipt
            _reviewedOperatingSystemReceipt;
        private PcFirmwareBaselineReceipt
            _reviewedDriverFirmwareBaselineReceipt;
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

        public bool IsReviewingFirmwareBaseline =>
            _isReviewingFirmwareBaseline;

        public bool IsReviewingFictionalOsInstallation =>
            _isReviewingFictionalOsInstallation;

        public bool IsReviewingFictionalDriverInstallation =>
            _isReviewingFictionalDriverInstallation;

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
            ResetFirmwareReviewIfContextChanged();
            ResetFictionalOsReviewIfContextChanged();
            ResetFictionalDriverReviewIfContextChanged();
            ResetValidationReviewIfContextChanged();
            ResetQualityReleaseReviewIfContextChanged();
            if (playerInput == null || playerMotor == null || playerMotor.IsPaused ||
                playerInput.PausePressedThisFrame)
            {
                return;
            }

            bool interactPressed = playerInput.InteractPressedThisFrame;
            bool primaryPressed = playerInput.PrimaryActionPressedThisFrame;
            if (!interactPressed && !primaryPressed)
            {
                return;
            }

            if (interactPressed)
            {
                OperationResult powerGate = ValidateInteractionGate();
                if (powerGate.IsFailure)
                {
                    Remember(powerGate);
                    readinessProjection?.RefreshPresentation();
                    return;
                }

                if (!playerInput.TryConsumeInteractPressThisFrame())
                {
                    return;
                }

                if (primaryPressed)
                {
                    playerInput.TryConsumePrimaryActionPressThisFrame();
                }

                OperationResult powerResult = Remember(TryAttemptAuthorized());
                if (powerResult.IsSuccess)
                {
                    ResetPrimaryReviews();
                }

                readinessProjection.RefreshPresentation();
                return;
            }

            bool qualityReleasePrimary = HasQualityReleasePrimaryContext();
            OperationResult primaryGate = qualityReleasePrimary
                ? ValidateQualityReleaseInteractionGate()
                : ValidateFirmwareInteractionGate();
            if (primaryGate.IsFailure)
            {
                Remember(primaryGate);
                readinessProjection?.RefreshPresentation();
                return;
            }

            if (!playerInput.TryConsumePrimaryActionPressThisFrame())
            {
                return;
            }

            Remember(qualityReleasePrimary
                ? TryAttemptQualityReleaseAuthorized()
                : TryAttemptFirmwareAuthorized());
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

        internal OperationResult InspectFirmwareInteractionGateForTests()
        {
            return ValidateFirmwareInteractionGate();
        }

        internal OperationResult TryAttemptFirmwareAuthorizedForTests()
        {
            return Remember(TryAttemptFirmwareAuthorized());
        }

        internal OperationResult InspectFictionalOsInteractionGateForTests()
        {
            return ValidateFirmwareInteractionGate();
        }

        internal OperationResult TryAttemptFictionalOsAuthorizedForTests()
        {
            return Remember(TryAttemptFirmwareAuthorized());
        }

        internal OperationResult InspectFictionalDriverInteractionGateForTests()
        {
            return ValidateFirmwareInteractionGate();
        }

        internal OperationResult TryAttemptFictionalDriverAuthorizedForTests()
        {
            return Remember(TryAttemptFirmwareAuthorized());
        }

        internal OperationResult InspectValidationInteractionGateForTests()
        {
            return ValidateFirmwareInteractionGate();
        }

        internal OperationResult TryAttemptValidationAuthorizedForTests()
        {
            return Remember(TryAttemptFirmwareAuthorized());
        }

        internal OperationResult InspectQualityReleaseInteractionGateForTests()
        {
            return ValidateQualityReleaseInteractionGate();
        }

        internal OperationResult TryAttemptQualityReleaseAuthorizedForTests()
        {
            return Remember(TryAttemptQualityReleaseAuthorized());
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
            if (session == null)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.RuntimeNotReady);
            }

            OperationResult<PowerTestAttemptAuthority> ensuredAttempts =
                session.EnsurePowerTestAttemptsAuthority();
            if (ensuredAttempts.IsFailure)
            {
                return OperationResult.Fail(ensuredAttempts.Error);
            }

            PowerTestAttemptAuthority attempts = ensuredAttempts.Value;

            if (attempts.HasCompletedPreflight)
            {
                OperationResult<PcPowerStateAuthority> ensuredPowerState =
                    session.EnsurePowerStateAuthority();
                if (ensuredPowerState.IsFailure)
                {
                    return OperationResult.Fail(ensuredPowerState.Error);
                }

                PcPowerStateAuthority powerState = ensuredPowerState.Value;

                OperationResult<PcPowerStateReceipt> transition;
                if (powerState.IsEnergized)
                {
                    transition = powerState.TryPowerOff(
                        session.CreatePrototypePowerStateOperationId(
                            PcPowerTransitionKind.PowerOff,
                            powerState.Revision + 1L),
                        powerState.ActivePowerOnReceipt,
                        powerState.Revision);
                }
                else
                {
                    OperationResult<PowerTestAttemptReceipt> current =
                        attempts.EvaluateCurrentReceipt();
                    if (current.IsFailure)
                    {
                        return OperationResult.Fail(current.Error);
                    }

                    transition = powerState.TryPowerOn(
                        session.CreatePrototypePowerStateOperationId(
                            PcPowerTransitionKind.PowerOn,
                            powerState.Revision + 1L),
                        current.Value,
                        powerState.Revision);
                }

                if (transition.IsFailure)
                {
                    return OperationResult.Fail(transition.Error);
                }

                _lastSuccessfulOperationFrame = Time.frameCount;
                if (transition.Value.TransitionKind ==
                    PcPowerTransitionKind.PowerOn)
                {
                    session.ObservePowerOnForQuality();
                    OperationResult<PcPostStartupReceipt> postStartup =
                        powerState.TryCompleteStartupSelfTest(
                            session.CreatePrototypePostStartupOperationId(
                                transition.Value),
                            transition.Value,
                            powerState.Revision);
                    return postStartup.IsSuccess
                        ? OperationResult.Success()
                        : OperationResult.Fail(postStartup.Error);
                }

                session.ObservePowerOffCandidateForQuality(transition.Value);

                return OperationResult.Success();
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

        private OperationResult TryAttemptFirmwareAuthorized()
        {
            if (_lastSuccessfulOperationFrame == Time.frameCount)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.InputReplay);
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetPowerState(
                    out PcPowerStateAuthority powerState))
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.RuntimeNotReady);
            }

            OperationResult<PcPostStartupReceipt> currentPostStartup =
                powerState.EvaluateCurrentStartupSelfTest();
            if (currentPostStartup.IsFailure)
            {
                return OperationResult.Fail(currentPostStartup.Error);
            }

            OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                powerState.EvaluateCurrentFirmwareBaseline();
            if (currentFirmware.IsSuccess)
            {
                if (session.TryGetFictionalOsInstallation(
                        out PcFictionalOsInstallationAuthority osAuthority) &&
                    osAuthority.ReceiptCount > 0)
                {
                    OperationResult osHistory =
                        osAuthority.ValidateReceiptHistory();
                    if (osHistory.IsFailure)
                    {
                        return osHistory;
                    }

                    OperationResult<PcFictionalOsInstallationReceipt>
                        installedOs =
                            osAuthority.EvaluateInstalledOperatingSystem();
                    if (installedOs.IsFailure)
                    {
                        return OperationResult.Fail(installedOs.Error);
                    }

                    if (session.TryGetFictionalDriverInstallation(
                            out PcFictionalDriverInstallationAuthority
                                driverAuthority))
                    {
                        OperationResult driverHistory =
                            driverAuthority.ValidateReceiptHistory();
                        if (driverHistory.IsFailure)
                        {
                            return driverHistory;
                        }

                        OperationResult<PcFictionalDriverInstallationReceipt>
                            installedDriver =
                                driverAuthority.EvaluateInstalledDrivers();
                        if (installedDriver.IsSuccess)
                        {
                            return TryAttemptValidationAuthorized(
                                session,
                                powerState,
                                currentFirmware.Value,
                                installedDriver.Value);
                        }

                        if (driverAuthority.ReceiptCount > 0)
                        {
                            return OperationResult.Fail(installedDriver.Error);
                        }
                    }

                    return TryAttemptFictionalDriverAuthorized(
                        session,
                        powerState,
                        currentFirmware.Value,
                        installedOs.Value);
                }

                return TryAttemptFictionalOsAuthorized(
                    session,
                    powerState,
                    currentFirmware.Value);
            }

            if (!_isReviewingFirmwareBaseline)
            {
                _isReviewingFirmwareBaseline = true;
                _reviewedPostStartupReceipt = currentPostStartup.Value;
                _lastSuccessfulOperationFrame = Time.frameCount;
                return OperationResult.Success();
            }

            if (!ReferenceEquals(
                    _reviewedPostStartupReceipt,
                    currentPostStartup.Value))
            {
                ResetFirmwareReview();
                return OperationResult.Fail(
                    PcFirmwareBaselineFailures.NotCurrent);
            }

            OperationResult<PcFirmwareBaselineReceipt> saved =
                powerState.TrySaveFirmwareBaseline(
                    session.CreatePrototypeFirmwareBaselineOperationId(
                        currentPostStartup.Value),
                    currentPostStartup.Value,
                    powerState.Revision,
                    powerState.FirmwareBaselineRevision);
            if (saved.IsFailure)
            {
                return OperationResult.Fail(saved.Error);
            }

            _lastSuccessfulOperationFrame = Time.frameCount;
            ResetFirmwareReview();
            return OperationResult.Success();
        }

        private OperationResult TryAttemptFictionalOsAuthorized(
            GarageStockFlowSession session,
            PcPowerStateAuthority powerState,
            PcFirmwareBaselineReceipt currentFirmware)
        {
            if (!_isReviewingFictionalOsInstallation)
            {
                _isReviewingFictionalOsInstallation = true;
                _reviewedFirmwareBaselineReceipt = currentFirmware;
                _lastSuccessfulOperationFrame = Time.frameCount;
                return OperationResult.Success();
            }

            if (!ReferenceEquals(
                    _reviewedFirmwareBaselineReceipt,
                    currentFirmware))
            {
                ResetFictionalOsReview();
                return OperationResult.Fail(
                    PcFictionalOsInstallationFailures.NotCurrent);
            }

            OperationResult<PcFictionalOsInstallationAuthority> ensured =
                session.EnsureFictionalOsInstallationAuthority();
            if (ensured.IsFailure)
            {
                return OperationResult.Fail(ensured.Error);
            }

            PcFictionalOsInstallationAuthority authority = ensured.Value;
            OperationResult<PcFictionalOsInstallationReceipt> installed =
                authority.TryCompleteInstallation(
                    session.CreatePrototypeFictionalOsInstallationOperationId(
                        currentFirmware),
                    currentFirmware,
                    session.AssemblyBuild.StorageItemId,
                    powerState.Revision,
                    authority.Revision);
            if (installed.IsFailure)
            {
                return OperationResult.Fail(installed.Error);
            }

            _lastSuccessfulOperationFrame = Time.frameCount;
            ResetFictionalOsReview();
            return OperationResult.Success();
        }

        private OperationResult TryAttemptFictionalDriverAuthorized(
            GarageStockFlowSession session,
            PcPowerStateAuthority powerState,
            PcFirmwareBaselineReceipt currentFirmware,
            PcFictionalOsInstallationReceipt currentOperatingSystem)
        {
            if (!_isReviewingFictionalDriverInstallation)
            {
                _isReviewingFictionalDriverInstallation = true;
                _reviewedOperatingSystemReceipt = currentOperatingSystem;
                _reviewedDriverFirmwareBaselineReceipt = currentFirmware;
                _lastSuccessfulOperationFrame = Time.frameCount;
                readinessProjection?.ObserveFictionalDriverReview();
                return OperationResult.Success();
            }

            if (!ReferenceEquals(
                    _reviewedOperatingSystemReceipt,
                    currentOperatingSystem) ||
                !ReferenceEquals(
                    _reviewedDriverFirmwareBaselineReceipt,
                    currentFirmware))
            {
                ResetFictionalDriverReview();
                return OperationResult.Fail(
                    PcFictionalDriverInstallationFailures.NotCurrent);
            }

            OperationResult<PcFictionalDriverInstallationAuthority> ensured =
                session.EnsureFictionalDriverInstallationAuthority();
            if (ensured.IsFailure)
            {
                ResetFictionalDriverReview();
                return OperationResult.Fail(ensured.Error);
            }

            PcFictionalDriverInstallationAuthority authority = ensured.Value;
            OperationResult<PcFictionalDriverInstallationReceipt> installed =
                authority.TryCompleteInstallation(
                    session
                        .CreatePrototypeFictionalDriverInstallationOperationId(
                            currentOperatingSystem),
                    currentOperatingSystem,
                    currentFirmware,
                    session.AssemblyBuild.StorageItemId,
                    powerState.Revision,
                    authority.Revision);
            if (installed.IsFailure)
            {
                ResetFictionalDriverReview();
                return OperationResult.Fail(installed.Error);
            }

            _lastSuccessfulOperationFrame = Time.frameCount;
            ResetFictionalDriverReview();
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
            if (session == null || session.PowerBudget == null)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.RuntimeNotReady);
            }

            if (!session.TryGetPowerTestAttempts(
                    out PowerTestAttemptAuthority attempts))
            {
                OperationResult<PcPowerBudgetSnapshot> assessment =
                    session.PowerBudget.AssessPowerBudget();
                if (assessment.IsFailure)
                {
                    return OperationResult.Fail(assessment.Error);
                }

                return assessment.Value.IsSufficient
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        PowerTestAttemptFailures.PowerSupplyInsufficient);
            }

            if (attempts.HasCompletedPreflight)
            {
                PcPowerStateAuthority powerState =
                    session.TryGetPowerState(out PcPowerStateAuthority existing)
                        ? existing
                        : null;
                if (powerState?.IsEnergized == true)
                {
                    return powerState.ActivePowerOnReceipt != null
                        ? OperationResult.Success()
                        : OperationResult.Fail(
                            PcPowerStateFailures.ReceiptHistoryInvalid);
                }

                if (powerState != null &&
                    powerState.ValidateReceiptHistory().IsFailure)
                {
                    return OperationResult.Fail(
                        ElectricalPowerTestStationFailures.RuntimeNotReady);
                }

                OperationResult<PowerTestAttemptReceipt> current =
                    attempts.EvaluateCurrentReceipt();
                return current.IsSuccess
                    ? OperationResult.Success()
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

        private OperationResult ValidateFirmwareInteractionGate()
        {
            OperationResult powerGate = ValidateInteractionGate();
            if (powerGate.IsFailure)
            {
                return powerGate;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetPowerState(
                    out PcPowerStateAuthority powerState) ||
                !powerState.IsEnergized)
            {
                return OperationResult.Fail(
                    PcFirmwareBaselineFailures.NotCurrent);
            }

            OperationResult history = powerState.ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult.Fail(history.Error);
            }

            OperationResult<PcPostStartupReceipt> currentPostStartup =
                powerState.EvaluateCurrentStartupSelfTest();
            if (currentPostStartup.IsFailure)
            {
                return OperationResult.Fail(currentPostStartup.Error);
            }

            OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                powerState.EvaluateCurrentFirmwareBaseline();
            if (currentFirmware.IsSuccess)
            {
                if (session.TryGetFictionalOsInstallation(
                        out PcFictionalOsInstallationAuthority osAuthority))
                {
                    OperationResult osHistory =
                        osAuthority.ValidateReceiptHistory();
                    if (osHistory.IsFailure)
                    {
                        return osHistory;
                    }

                    OperationResult<PcFictionalOsInstallationReceipt>
                        installedOs =
                            osAuthority.EvaluateInstalledOperatingSystem();
                    if (installedOs.IsSuccess)
                    {
                        if (session.TryGetFictionalDriverInstallation(
                                out PcFictionalDriverInstallationAuthority
                                    driverAuthority))
                        {
                            OperationResult driverHistory =
                                driverAuthority.ValidateReceiptHistory();
                            if (driverHistory.IsFailure)
                            {
                                return driverHistory;
                            }

                            if (driverAuthority
                                .EvaluateInstalledDrivers()
                                .TryGetValue(
                                    out PcFictionalDriverInstallationReceipt
                                        installedDriver))
                            {
                                return ValidateValidationInteractionContext(
                                    session,
                                    powerState,
                                    currentFirmware.Value,
                                    installedDriver);
                            }

                            if (driverAuthority.ReceiptCount > 0)
                            {
                                return OperationResult.Fail(
                                    driverAuthority.EvaluateInstalledDrivers()
                                        .Error);
                            }
                        }

                        if (_isReviewingFictionalDriverInstallation &&
                            (!ReferenceEquals(
                                _reviewedOperatingSystemReceipt,
                                installedOs.Value) ||
                             !ReferenceEquals(
                                _reviewedDriverFirmwareBaselineReceipt,
                                currentFirmware.Value)))
                        {
                            return OperationResult.Fail(
                                PcFictionalDriverInstallationFailures
                                    .NotCurrent);
                        }

                        return OperationResult.Success();
                    }

                    if (osAuthority.ReceiptCount > 0)
                    {
                        return OperationResult.Fail(installedOs.Error);
                    }
                }

                if (_isReviewingFictionalOsInstallation &&
                    !ReferenceEquals(
                        _reviewedFirmwareBaselineReceipt,
                        currentFirmware.Value))
                {
                    return OperationResult.Fail(
                        PcFictionalOsInstallationFailures.NotCurrent);
                }

                return OperationResult.Success();
            }

            if (_isReviewingFirmwareBaseline &&
                !ReferenceEquals(
                    _reviewedPostStartupReceipt,
                    currentPostStartup.Value))
            {
                return OperationResult.Fail(
                    PcFirmwareBaselineFailures.NotCurrent);
            }

            return OperationResult.Success();
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
            ResetFirmwareReviewIfContextChanged();
            ResetFictionalOsReviewIfContextChanged();
            ResetFictionalDriverReviewIfContextChanged();
            ResetValidationReviewIfContextChanged();
            ResetQualityReleaseReviewIfContextChanged();
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
            if (session == null || session.PowerBudget == null)
            {
                return "GÜÇ TESTİ ENGELLİ • AUTHORITY HAZIR DEĞİL";
            }

            if (!session.TryGetPowerTestAttempts(
                    out PowerTestAttemptAuthority attempts))
            {
                OperationResult<PcPowerBudgetSnapshot> assessment =
                    session.PowerBudget.AssessPowerBudget();
                if (assessment.IsFailure)
                {
                    return $"GÜÇ TESTİ ENGELLİ • {assessment.Error.Code}";
                }

                if (!assessment.Value.IsSufficient)
                {
                    return "GÜÇ TESTİ ENGELLİ • PSU YETERSİZ";
                }

                string freshInteract = playerInput != null
                    ? playerInput.InteractBindingPrompt
                    : "E / A";
                return $"{freshInteract}: GÜÇ TESTİ ÖN KONTROLÜNÜ ÇALIŞTIR";
            }

            if (attempts.HasCompletedPreflight)
            {
                string bindingPrompt = playerInput != null
                    ? playerInput.InteractBindingPrompt
                    : "E / A";
                PcPowerStateAuthority powerState =
                    session.TryGetPowerState(out PcPowerStateAuthority existing)
                        ? existing
                        : null;
                if (powerState?.IsEnergized == true)
                {
                    OperationResult<PcPostStartupReceipt> postStartup =
                        powerState.EvaluateCurrentStartupSelfTest();
                    if (postStartup.IsSuccess)
                    {
                        OperationResult<PcFirmwareBaselineReceipt> firmware =
                            powerState.EvaluateCurrentFirmwareBaseline();
                        if (firmware.IsSuccess)
                        {
                            if (session.TryGetFictionalOsInstallation(
                                    out PcFictionalOsInstallationAuthority
                                        osAuthority))
                            {
                                OperationResult osHistory =
                                    osAuthority.ValidateReceiptHistory();
                                if (osHistory.IsFailure)
                                {
                                    return $"{bindingPrompt}: GÜCÜ KAPAT • " +
                                           "OS KAYDI ENGELLİ • " +
                                           osHistory.Error.Code;
                                }

                                OperationResult<
                                    PcFictionalOsInstallationReceipt>
                                    installedOs = osAuthority
                                        .EvaluateInstalledOperatingSystem();
                                if (installedOs.IsSuccess)
                                {
                                    if (session
                                        .TryGetFictionalDriverInstallation(
                                            out
                                            PcFictionalDriverInstallationAuthority
                                                driverAuthority))
                                    {
                                        OperationResult driverHistory =
                                            driverAuthority
                                                .ValidateReceiptHistory();
                                        if (driverHistory.IsFailure)
                                        {
                                            return $"{bindingPrompt}: " +
                                                   "GÜCÜ KAPAT • " +
                                                   "DRIVER KAYDI ENGELLİ • " +
                                                   driverHistory.Error.Code;
                                        }

                                        OperationResult<
                                            PcFictionalDriverInstallationReceipt>
                                            installedDriver = driverAuthority
                                                .EvaluateInstalledDrivers();
                                        if (installedDriver.IsSuccess)
                                        {
                                            return BuildValidationPrompt(
                                                session,
                                                powerState,
                                                firmware.Value,
                                                installedDriver.Value,
                                                bindingPrompt);
                                        }
                                    }

                                    string driverPrimaryBinding =
                                        playerInput != null
                                            ? playerInput.PrimaryBindingPrompt
                                            : "LMB / RT";
                                    if (_isReviewingFictionalDriverInstallation)
                                    {
                                        return "KURGUSAL DRIVER KURULUMU • " +
                                               "WORKSHOP DRIVER BUNDLE • " +
                                               $"{driverPrimaryBinding}: " +
                                               "KURULUMU BAŞLAT VE TAMAMLA • " +
                                               $"{bindingPrompt}: GÜCÜ KAPAT";
                                    }

                                    if (LastFailureCode.StartsWith(
                                            "assembly.fictional-driver-" +
                                            "installation.",
                                            StringComparison.Ordinal))
                                    {
                                        return $"{bindingPrompt}: GÜCÜ KAPAT • " +
                                               "DRIVER KURULUMU ENGELLİ • " +
                                               LastFailureCode;
                                    }

                                    return $"{bindingPrompt}: GÜCÜ KAPAT • " +
                                           "KURGUSAL OS KURULDU • " +
                                           "SONRAKİ AŞAMA: DRIVER • " +
                                           $"{driverPrimaryBinding}: " +
                                           "DRIVER KURULUMUNU AÇ";
                                }
                            }

                            string osPrimaryBinding = playerInput != null
                                ? playerInput.PrimaryBindingPrompt
                                : "LMB / RT";
                            if (_isReviewingFictionalOsInstallation)
                            {
                                return "KURGUSAL OS KURULUMU • " +
                                       "WORKSHOP STANDARD • " +
                                       $"{osPrimaryBinding}: KURULUMU BAŞLAT " +
                                       "VE TAMAMLA • " +
                                       $"{bindingPrompt}: GÜCÜ KAPAT";
                            }

                            if (LastFailureCode.StartsWith(
                                    "assembly.fictional-os-installation.",
                                    StringComparison.Ordinal))
                            {
                                return $"{bindingPrompt}: GÜCÜ KAPAT • " +
                                       "OS KURULUMU ENGELLİ • " +
                                       LastFailureCode;
                            }

                            return $"{bindingPrompt}: GÜCÜ KAPAT • " +
                                   "UEFI BASELINE KAYDEDİLDİ • " +
                                   "SONRAKİ AŞAMA: OS • " +
                                   $"{osPrimaryBinding}: KURGUSAL OS " +
                                   "KURULUMUNU AÇ";
                        }

                        string primaryBinding = playerInput != null
                            ? playerInput.PrimaryBindingPrompt
                            : "LMB / RT";
                        if (_isReviewingFirmwareBaseline)
                        {
                            return "UEFI SETUP • OPTIMIZED DEFAULTS • " +
                                   $"{primaryBinding}: KAYDET VE ÇIK • " +
                                   $"{bindingPrompt}: GÜCÜ KAPAT";
                        }

                        if (LastFailureCode.StartsWith(
                                "assembly.firmware-baseline.",
                                StringComparison.Ordinal))
                        {
                            return $"{bindingPrompt}: GÜCÜ KAPAT • " +
                                   "UEFI KAYDI ENGELLİ • " +
                                   LastFailureCode;
                        }

                        return $"{bindingPrompt}: GÜCÜ KAPAT • POST GEÇTİ • " +
                               $"{primaryBinding}: UEFI SETUP'I AÇ";
                    }

                    return string.IsNullOrEmpty(LastFailureCode)
                        ? $"{bindingPrompt}: GÜCÜ KAPAT • POST BEKLİYOR"
                        : $"{bindingPrompt}: GÜCÜ KAPAT • POST ENGELLİ • " +
                          LastFailureCode;
                }

                if (powerState != null &&
                    powerState.ValidateReceiptHistory().IsFailure)
                {
                    return "GÜÇ TESTİ ENGELLİ • POWER AUTHORITY GEÇERSİZ";
                }

                if (powerState != null && !powerState.IsEnergized)
                {
                    string qualityReleasePrompt = BuildQualityReleasePrompt(
                        session,
                        bindingPrompt);
                    if (!string.IsNullOrEmpty(qualityReleasePrompt))
                    {
                        return qualityReleasePrompt;
                    }
                }

                OperationResult<PowerTestAttemptReceipt> current =
                    attempts.EvaluateCurrentReceipt();
                return current.IsSuccess
                    ? $"ÖN KONTROL GEÇTİ • {bindingPrompt}: GÜCÜ AÇ"
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

        private void ResetFirmwareReviewIfContextChanged()
        {
            if (!_isReviewingFirmwareBaseline)
            {
                return;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session != null &&
                session.TryGetPowerState(
                    out PcPowerStateAuthority powerState) &&
                powerState.IsEnergized)
            {
                OperationResult<PcPostStartupReceipt> currentPostStartup =
                    powerState.EvaluateCurrentStartupSelfTest();
                if (currentPostStartup.IsSuccess &&
                    ReferenceEquals(
                        currentPostStartup.Value,
                        _reviewedPostStartupReceipt))
                {
                    return;
                }
            }

            ResetFirmwareReview();
        }

        private void ResetFirmwareReview()
        {
            _isReviewingFirmwareBaseline = false;
            _reviewedPostStartupReceipt = null;
        }

        private void ResetFictionalOsReviewIfContextChanged()
        {
            if (!_isReviewingFictionalOsInstallation)
            {
                return;
            }

            if (playerInput == null || playerMotor == null ||
                playerMotor.IsPaused ||
                playerInput.PausePressedThisFrame || PlayerIsBusy() ||
                PlayerHasCompetingWorldInteractOwner() || !_isFocused)
            {
                ResetFictionalOsReview();
                return;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session != null &&
                session.TryGetPowerState(
                    out PcPowerStateAuthority powerState) &&
                powerState.IsEnergized)
            {
                OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                    powerState.EvaluateCurrentFirmwareBaseline();
                bool alreadyInstalled =
                    session.TryGetFictionalOsInstallation(
                        out PcFictionalOsInstallationAuthority authority) &&
                    authority.EvaluateInstalledOperatingSystem().IsSuccess;
                if (currentFirmware.IsSuccess && !alreadyInstalled &&
                    ReferenceEquals(
                        currentFirmware.Value,
                        _reviewedFirmwareBaselineReceipt))
                {
                    return;
                }
            }

            ResetFictionalOsReview();
        }

        private void ResetFictionalOsReview()
        {
            _isReviewingFictionalOsInstallation = false;
            _reviewedFirmwareBaselineReceipt = null;
        }

        private void ResetFictionalDriverReviewIfContextChanged()
        {
            if (!_isReviewingFictionalDriverInstallation)
            {
                return;
            }

            if (playerInput == null || playerMotor == null ||
                playerMotor.IsPaused ||
                playerInput.PausePressedThisFrame || PlayerIsBusy() ||
                PlayerHasCompetingWorldInteractOwner() || !_isFocused)
            {
                ResetFictionalDriverReview();
                return;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session != null &&
                session.TryGetPowerState(
                    out PcPowerStateAuthority powerState) &&
                powerState.IsEnergized &&
                session.TryGetFictionalOsInstallation(
                    out PcFictionalOsInstallationAuthority osAuthority))
            {
                OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                    powerState.EvaluateCurrentFirmwareBaseline();
                OperationResult<PcFictionalOsInstallationReceipt> currentOs =
                    osAuthority.EvaluateInstalledOperatingSystem();
                bool alreadyInstalled =
                    session.TryGetFictionalDriverInstallation(
                        out PcFictionalDriverInstallationAuthority authority) &&
                    authority.EvaluateInstalledDrivers().IsSuccess;
                if (currentFirmware.IsSuccess && currentOs.IsSuccess &&
                    !alreadyInstalled &&
                    ReferenceEquals(
                        currentOs.Value,
                        _reviewedOperatingSystemReceipt) &&
                    ReferenceEquals(
                        currentFirmware.Value,
                        _reviewedDriverFirmwareBaselineReceipt))
                {
                    return;
                }
            }

            ResetFictionalDriverReview();
        }

        private void ResetFictionalDriverReview()
        {
            _isReviewingFictionalDriverInstallation = false;
            _reviewedOperatingSystemReceipt = null;
            _reviewedDriverFirmwareBaselineReceipt = null;
            readinessProjection?.ObserveFictionalDriverWaiting();
        }

        private void ResetPrimaryReviews()
        {
            ResetFirmwareReview();
            ResetFictionalOsReview();
            ResetFictionalDriverReview();
            ResetValidationReview();
            ResetQualityReleaseReview();
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
            if (result.IsFailure && result.Error.Code.StartsWith(
                    "assembly.fictional-driver-installation.",
                    StringComparison.Ordinal))
            {
                readinessProjection?.ObserveFictionalDriverRejected(
                    result.Error);
            }

            if (result.IsFailure && result.Error.Code.StartsWith(
                    "assembly.validation.",
                    StringComparison.Ordinal))
            {
                readinessProjection?.ObserveValidationRejected(result.Error);
            }

            return result;
        }
    }
}
