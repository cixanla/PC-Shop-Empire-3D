using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum PcValidationPresentationState
    {
        Waiting = 0,
        Reviewing = 1,
        Passed = 2,
        Rejected = 3,
        NotCurrent = 4
    }

    public sealed partial class ElectricalReadinessWorkbenchProjection
    {
        private long _observedValidationRevision = -1L;

        public bool HasCurrentValidation { get; private set; }

        public PcValidationPresentationState ValidationState { get; private set; } =
            PcValidationPresentationState.Waiting;

        public string ValidationFailureCode { get; private set; } = string.Empty;

        public int ValidationBenchmarkScore { get; private set; }

        public int ValidationStressSteps { get; private set; }

        public int ValidationProcessorPeakTemperatureCelsius { get; private set; }

        public int ValidationGraphicsPeakTemperatureCelsius { get; private set; }

        public int ValidationSystemPowerDrawWatts { get; private set; }

        public int ValidationMinimumRecommendedPsuWatts { get; private set; }

        public int ValidationInstalledPsuWatts { get; private set; }

        public int ValidationPowerMarginWatts { get; private set; }

        public PcQualityTier ValidationQualityTier { get; private set; }

        public string ValidationQualityLabel =>
            ResolveValidationQualityLabel(ValidationQualityTier);

        public void ObserveValidationWaiting()
        {
            ValidationState = PcValidationPresentationState.Waiting;
            ValidationFailureCode = string.Empty;
            ClearValidationMetrics();
            RefreshPresentation();
        }

        public void ObserveValidationReview()
        {
            ValidationState = PcValidationPresentationState.Reviewing;
            ValidationFailureCode = string.Empty;
            ClearValidationMetrics();
            RefreshPresentation();
        }

        public void ObserveValidationRejected(Failure failure)
        {
            ValidationState = PcValidationPresentationState.Rejected;
            ValidationFailureCode = failure.Code;
            ClearValidationMetrics();
            RefreshPresentation();
        }

        private void ObserveValidationAuthorityState(
            GarageStockFlowSession session)
        {
            ClearValidationMetrics();
            if (session == null ||
                !session.TryGetValidation(out PcValidationAuthority authority))
            {
                if (ValidationState == PcValidationPresentationState.Passed ||
                    ValidationState == PcValidationPresentationState.NotCurrent)
                {
                    ValidationState = PcValidationPresentationState.Waiting;
                    ValidationFailureCode = string.Empty;
                }

                return;
            }

            OperationResult history = authority.ValidateReceiptHistory();
            if (history.IsFailure)
            {
                ValidationState = PcValidationPresentationState.Rejected;
                ValidationFailureCode = history.Error.Code;
                return;
            }

            bool energized = session.TryGetPowerState(
                                  out PcPowerStateAuthority powerState) &&
                              powerState.IsEnergized;
            if (ValidationState == PcValidationPresentationState.Reviewing ||
                (ValidationState == PcValidationPresentationState.Rejected &&
                 energized))
            {
                return;
            }

            OperationResult<PcValidationReceipt> current =
                authority.EvaluateCurrentValidation();
            if (current.IsSuccess)
            {
                CaptureValidation(current.Value);
                return;
            }

            if (authority.ReceiptCount > 0)
            {
                ValidationState = PcValidationPresentationState.NotCurrent;
                ValidationFailureCode = current.Error.Code;
                return;
            }

            if (ValidationState != PcValidationPresentationState.Reviewing &&
                ValidationState != PcValidationPresentationState.Rejected)
            {
                ValidationState = PcValidationPresentationState.Waiting;
                ValidationFailureCode = string.Empty;
            }
        }

        private void CaptureValidation(PcValidationReceipt receipt)
        {
            HasCurrentValidation = true;
            ValidationState = PcValidationPresentationState.Passed;
            ValidationFailureCode = string.Empty;
            ValidationBenchmarkScore = receipt.BenchmarkScore;
            ValidationStressSteps = receipt.StressSteps;
            ValidationProcessorPeakTemperatureCelsius =
                receipt.ProcessorPeakTemperatureCelsius;
            ValidationGraphicsPeakTemperatureCelsius =
                receipt.GraphicsCardPeakTemperatureCelsius;
            ValidationSystemPowerDrawWatts = receipt.SystemPowerDrawWatts;
            ValidationMinimumRecommendedPsuWatts =
                receipt.MinimumRecommendedPsuWatts;
            ValidationInstalledPsuWatts = receipt.InstalledPsuWatts;
            ValidationPowerMarginWatts = receipt.PowerMarginWatts;
            ValidationQualityTier = receipt.QualityTier;
        }

        private void ClearValidationMetrics()
        {
            HasCurrentValidation = false;
            ValidationBenchmarkScore = 0;
            ValidationStressSteps = 0;
            ValidationProcessorPeakTemperatureCelsius = 0;
            ValidationGraphicsPeakTemperatureCelsius = 0;
            ValidationSystemPowerDrawWatts = 0;
            ValidationMinimumRecommendedPsuWatts = 0;
            ValidationInstalledPsuWatts = 0;
            ValidationPowerMarginWatts = 0;
            ValidationQualityTier = default;
        }

        private static long ResolveValidationRevision(
            GarageStockFlowSession session)
        {
            return session != null &&
                   session.TryGetValidation(out PcValidationAuthority authority)
                ? authority.Revision
                : -1L;
        }

        private static string ResolveValidationQualityLabel(PcQualityTier tier)
        {
            switch (tier)
            {
                case PcQualityTier.Excellent:
                    return "MÜKEMMEL";
                case PcQualityTier.Good:
                    return "İYİ";
                case PcQualityTier.Standard:
                    return "STANDART";
                default:
                    return "BEKLENİYOR";
            }
        }
    }
}
