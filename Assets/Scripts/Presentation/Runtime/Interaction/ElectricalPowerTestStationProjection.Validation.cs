using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class ElectricalPowerTestStationProjection
    {
        private bool _isReviewingValidation;
        private PcFictionalDriverInstallationReceipt
            _reviewedValidationDriverReceipt;
        private PcFirmwareBaselineReceipt _reviewedValidationFirmwareReceipt;
        private long _reviewedValidationPowerStateRevision = -1L;

        public bool IsReviewingValidation => _isReviewingValidation;

        private OperationResult TryAttemptValidationAuthorized(
            GarageStockFlowSession session,
            PcPowerStateAuthority powerState,
            PcFirmwareBaselineReceipt currentFirmware,
            PcFictionalDriverInstallationReceipt currentDriver)
        {
            if (session == null || powerState == null ||
                currentFirmware == null || currentDriver == null ||
                !powerState.IsEnergized)
            {
                ResetValidationReview();
                return OperationResult.Fail(PcValidationFailures.NotCurrent);
            }

            if (session.TryGetValidation(out PcValidationAuthority existing))
            {
                OperationResult history = existing.ValidateReceiptHistory();
                if (history.IsFailure)
                {
                    ResetValidationReview();
                    return history;
                }
            }

            if (!_isReviewingValidation)
            {
                _isReviewingValidation = true;
                _reviewedValidationDriverReceipt = currentDriver;
                _reviewedValidationFirmwareReceipt = currentFirmware;
                _reviewedValidationPowerStateRevision = powerState.Revision;
                _lastSuccessfulOperationFrame = UnityEngine.Time.frameCount;
                readinessProjection?.ObserveValidationReview();
                return OperationResult.Success();
            }

            if (!ReferenceEquals(
                    _reviewedValidationDriverReceipt,
                    currentDriver) ||
                !ReferenceEquals(
                    _reviewedValidationFirmwareReceipt,
                    currentFirmware) ||
                _reviewedValidationPowerStateRevision != powerState.Revision)
            {
                ResetValidationReview();
                return OperationResult.Fail(PcValidationFailures.NotCurrent);
            }

            OperationResult<PcValidationAuthority> ensured =
                session.EnsureValidationAuthority();
            if (ensured.IsFailure)
            {
                ResetValidationReview();
                return OperationResult.Fail(ensured.Error);
            }

            PcValidationAuthority authority = ensured.Value;
            OperationResult historyCheck = authority.ValidateReceiptHistory();
            if (historyCheck.IsFailure)
            {
                ResetValidationReview();
                return historyCheck;
            }

            OperationResult<PcValidationReceipt> completed =
                authority.TryCompleteValidation(
                    session.CreatePrototypeValidationOperationId(
                        currentDriver,
                        powerState.Revision,
                        authority.Revision),
                    currentDriver,
                    currentFirmware,
                    powerState.Revision,
                    authority.Revision);
            if (completed.IsFailure)
            {
                ResetValidationReview();
                readinessProjection?.ObserveValidationRejected(
                    completed.Error);
                return OperationResult.Fail(completed.Error);
            }

            _lastSuccessfulOperationFrame = UnityEngine.Time.frameCount;
            ResetValidationReviewState();
            readinessProjection?.ObserveValidationWaiting();
            return OperationResult.Success();
        }

        private OperationResult ValidateValidationInteractionContext(
            GarageStockFlowSession session,
            PcPowerStateAuthority powerState,
            PcFirmwareBaselineReceipt currentFirmware,
            PcFictionalDriverInstallationReceipt currentDriver)
        {
            if (session == null || powerState == null ||
                currentFirmware == null || currentDriver == null ||
                !powerState.IsEnergized)
            {
                return OperationResult.Fail(PcValidationFailures.NotCurrent);
            }

            if (session.TryGetValidation(out PcValidationAuthority authority))
            {
                OperationResult history = authority.ValidateReceiptHistory();
                if (history.IsFailure)
                {
                    return history;
                }
            }

            if (_isReviewingValidation &&
                (!ReferenceEquals(
                     _reviewedValidationDriverReceipt,
                     currentDriver) ||
                 !ReferenceEquals(
                     _reviewedValidationFirmwareReceipt,
                     currentFirmware) ||
                 _reviewedValidationPowerStateRevision != powerState.Revision))
            {
                return OperationResult.Fail(PcValidationFailures.NotCurrent);
            }

            return OperationResult.Success();
        }

        private string BuildValidationPrompt(
            GarageStockFlowSession session,
            PcPowerStateAuthority powerState,
            PcFirmwareBaselineReceipt currentFirmware,
            PcFictionalDriverInstallationReceipt currentDriver,
            string interactBinding)
        {
            string primaryBinding = playerInput != null
                ? playerInput.PrimaryBindingPrompt
                : "LMB / RT";
            PcValidationAuthority authority = session.TryGetValidation(
                out PcValidationAuthority existing)
                    ? existing
                    : null;
            if (authority != null)
            {
                OperationResult history = authority.ValidateReceiptHistory();
                if (history.IsFailure)
                {
                    return $"{interactBinding}: GÜCÜ KAPAT • " +
                           "VALIDATION KAYDI ENGELLİ • " +
                           history.Error.Code;
                }
            }

            if (_isReviewingValidation &&
                ReferenceEquals(
                    _reviewedValidationDriverReceipt,
                    currentDriver) &&
                ReferenceEquals(
                    _reviewedValidationFirmwareReceipt,
                    currentFirmware) &&
                _reviewedValidationPowerStateRevision == powerState.Revision)
            {
                return "WORKSHOP VALIDATION SUITE • " +
                       $"{GarageStockFlowSession.PrototypeValidationStressSteps} " +
                       "SABİT STRESS ADIMI • " +
                       $"{primaryBinding}: BENCHMARK + STRESS ÇALIŞTIR • " +
                       $"{interactBinding}: GÜCÜ KAPAT";
            }

            if (authority != null)
            {
                OperationResult<PcValidationReceipt> current =
                    authority.EvaluateCurrentValidation();
                if (current.IsSuccess)
                {
                    PcValidationReceipt receipt = current.Value;
                    return $"{interactBinding}: GÜCÜ KAPAT • " +
                           $"VALIDATION GEÇTİ • SCORE {receipt.BenchmarkScore} • " +
                           $"CPU {receipt.ProcessorPeakTemperatureCelsius}°C / " +
                           $"GPU {receipt.GraphicsCardPeakTemperatureCelsius}°C • " +
                           $"PSU +{receipt.PowerMarginWatts}W • " +
                           $"{ResolveValidationQualityLabel(receipt.QualityTier)} • " +
                           $"{primaryBinding}: YENİDEN İNCELE";
                }
            }

            if (LastFailureCode.StartsWith(
                    "assembly.validation.",
                    System.StringComparison.Ordinal))
            {
                return $"{interactBinding}: GÜCÜ KAPAT • " +
                       "VALIDATION REDDEDİLDİ • " + LastFailureCode + " • " +
                       $"{primaryBinding}: TEKRAR İNCELE";
            }

            return $"{interactBinding}: GÜCÜ KAPAT • " +
                   "WORKSHOP DRIVER BUNDLE KURULDU • " +
                   "SONRAKİ AŞAMA: VALIDATION • " +
                   $"{primaryBinding}: VALIDATION'I İNCELE";
        }

        private void ResetValidationReviewIfContextChanged()
        {
            if (!_isReviewingValidation)
            {
                return;
            }

            if (playerInput == null || playerMotor == null ||
                playerMotor.IsPaused || playerInput.PausePressedThisFrame ||
                PlayerIsBusy() || PlayerHasCompetingWorldInteractOwner() ||
                !_isFocused)
            {
                ResetValidationReview();
                return;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session != null &&
                session.TryGetPowerState(out PcPowerStateAuthority powerState) &&
                powerState.IsEnergized &&
                session.TryGetFictionalDriverInstallation(
                    out PcFictionalDriverInstallationAuthority driverAuthority))
            {
                OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                    powerState.EvaluateCurrentFirmwareBaseline();
                OperationResult<PcFictionalDriverInstallationReceipt>
                    currentDriver = driverAuthority.EvaluateInstalledDrivers();
                if (currentFirmware.IsSuccess && currentDriver.IsSuccess &&
                    ReferenceEquals(
                        currentFirmware.Value,
                        _reviewedValidationFirmwareReceipt) &&
                    ReferenceEquals(
                        currentDriver.Value,
                        _reviewedValidationDriverReceipt) &&
                    powerState.Revision ==
                        _reviewedValidationPowerStateRevision)
                {
                    return;
                }
            }

            ResetValidationReview();
        }

        private void ResetValidationReview()
        {
            ResetValidationReviewState();
            readinessProjection?.ObserveValidationWaiting();
        }

        private void ResetValidationReviewState()
        {
            _isReviewingValidation = false;
            _reviewedValidationDriverReceipt = null;
            _reviewedValidationFirmwareReceipt = null;
            _reviewedValidationPowerStateRevision = -1L;
        }

        private static string ResolveValidationQualityLabel(PcQualityTier tier)
        {
            switch (tier)
            {
                case PcQualityTier.Excellent:
                    return "MÜKEMMEL";
                case PcQualityTier.Good:
                    return "İYİ";
                default:
                    return "STANDART";
            }
        }
    }
}
