using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Quality;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public enum CustomPcQualityReleasePresentationState
    {
        WaitingForValidation = 0,
        AwaitingSafeShutdown = 1,
        ReadyForReview = 2,
        Reviewing = 3,
        ReadyForPackaging = 4,
        Rejected = 5,
        NotCurrent = 6
    }

    public sealed partial class ElectricalReadinessWorkbenchProjection
    {
        private long _observedQualityReleaseRevision = -1L;

        public CustomPcQualityReleasePresentationState QualityReleaseState
        {
            get;
            private set;
        } = CustomPcQualityReleasePresentationState.WaitingForValidation;

        public string QualityReleaseFailureCode { get; private set; } =
            string.Empty;

        public bool IsReadyForPackaging { get; private set; }

        public StableId<CustomPcBuildOrderIdScope> QualityWorkOrderId
        {
            get;
            private set;
        }

        public StableId<CustomPcWorkTicketIdScope> QualityWorkTicketId
        {
            get;
            private set;
        }

        public int QualityReleaseBenchmarkScore { get; private set; }

        public PcQualityTier QualityReleaseTier { get; private set; }

        public void ObserveQualityReleaseWaiting()
        {
            QualityReleaseState =
                CustomPcQualityReleasePresentationState.WaitingForValidation;
            QualityReleaseFailureCode = string.Empty;
            ClearQualityReleaseMetrics();
            RefreshPresentation();
        }

        public void ObserveQualityReleaseReview()
        {
            QualityReleaseState =
                CustomPcQualityReleasePresentationState.Reviewing;
            QualityReleaseFailureCode = string.Empty;
            ClearQualityReleaseMetrics();
            RefreshPresentation();
        }

        public void ObserveQualityReleaseRejected(Failure failure)
        {
            QualityReleaseState =
                CustomPcQualityReleasePresentationState.Rejected;
            QualityReleaseFailureCode = failure.Code;
            ClearQualityReleaseMetrics();
            RefreshPresentation();
        }

        private void ObserveQualityReleaseAuthorityState(
            GarageStockFlowSession session)
        {
            ClearQualityReleaseMetrics();
            if (session == null)
            {
                QualityReleaseState = CustomPcQualityReleasePresentationState
                    .WaitingForValidation;
                QualityReleaseFailureCode = string.Empty;
                return;
            }

            if (session.TryGetQualityRelease(
                    out CustomPcQualityReleaseAuthority authority))
            {
                OperationResult history = authority.ValidateReceiptHistory();
                if (history.IsFailure)
                {
                    QualityReleaseState =
                        CustomPcQualityReleasePresentationState.Rejected;
                    QualityReleaseFailureCode = history.Error.Code;
                    return;
                }

                if (QualityReleaseState ==
                        CustomPcQualityReleasePresentationState.Reviewing ||
                    QualityReleaseState ==
                        CustomPcQualityReleasePresentationState.Rejected)
                {
                    return;
                }

                OperationResult<CustomPcQualityReleaseReceipt> current =
                    authority.EvaluateCurrentRelease();
                if (current.IsSuccess)
                {
                    CaptureQualityRelease(current.Value);
                    return;
                }

                if (authority.ReceiptCount > 0)
                {
                    QualityReleaseState =
                        CustomPcQualityReleasePresentationState.NotCurrent;
                    QualityReleaseFailureCode = current.Error.Code;
                    return;
                }
            }

            if (!session.TryGetQualityReleaseCandidate(
                    out _,
                    out _,
                    out PcValidationReceipt validationReceipt,
                    out PcPowerStateReceipt powerOffReceipt) ||
                validationReceipt == null)
            {
                QualityReleaseState = CustomPcQualityReleasePresentationState
                    .WaitingForValidation;
                QualityReleaseFailureCode = string.Empty;
                return;
            }

            if (QualityReleaseState ==
                    CustomPcQualityReleasePresentationState.Reviewing ||
                QualityReleaseState ==
                    CustomPcQualityReleasePresentationState.Rejected)
            {
                return;
            }

            QualityReleaseState = powerOffReceipt == null
                ? CustomPcQualityReleasePresentationState.AwaitingSafeShutdown
                : CustomPcQualityReleasePresentationState.ReadyForReview;
            QualityReleaseFailureCode = string.Empty;
        }

        private void CaptureQualityRelease(
            CustomPcQualityReleaseReceipt receipt)
        {
            IsReadyForPackaging = true;
            QualityReleaseState =
                CustomPcQualityReleasePresentationState.ReadyForPackaging;
            QualityReleaseFailureCode = string.Empty;
            QualityWorkOrderId = receipt.WorkOrderId;
            QualityWorkTicketId = receipt.WorkTicketId;
            QualityReleaseBenchmarkScore = receipt.BenchmarkScore;
            QualityReleaseTier = receipt.QualityTier;
        }

        private void ClearQualityReleaseMetrics()
        {
            IsReadyForPackaging = false;
            QualityWorkOrderId = default;
            QualityWorkTicketId = default;
            QualityReleaseBenchmarkScore = 0;
            QualityReleaseTier = default;
        }

        private static long ResolveQualityReleaseRevision(
            GarageStockFlowSession session)
        {
            return session != null &&
                   session.TryGetQualityRelease(
                       out CustomPcQualityReleaseAuthority authority)
                ? authority.Revision
                : -1L;
        }
    }
}
