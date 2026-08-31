using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Quality;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class ElectricalPowerTestStationProjection
    {
        private bool _isReviewingQualityRelease;
        private CustomPcBuildOrderRecord _reviewedQualityWorkOrder;
        private CustomPcWorkTicketRecord _reviewedQualityWorkTicket;
        private PcValidationReceipt _reviewedQualityValidationReceipt;
        private PcPowerStateReceipt _reviewedQualityPowerOffReceipt;

        public bool IsReviewingQualityRelease => _isReviewingQualityRelease;

        private bool HasQualityReleasePrimaryContext()
        {
            GarageStockFlowSession session = ResolveSession();
            return session != null &&
                   session.TryGetPowerState(out PcPowerStateAuthority powerState) &&
                   !powerState.IsEnergized &&
                   session.TryGetQualityReleaseCandidate(
                       out _,
                       out _,
                       out _,
                       out PcPowerStateReceipt powerOffReceipt) &&
                   powerOffReceipt != null &&
                   powerState.Revision == powerOffReceipt.Revision;
        }

        private OperationResult ValidateQualityReleaseInteractionGate()
        {
            OperationResult interaction = ValidateInteractionGate();
            if (interaction.IsFailure)
            {
                return interaction;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetPowerState(out PcPowerStateAuthority powerState) ||
                powerState.IsEnergized ||
                !session.TryGetQualityReleaseCandidate(
                    out CustomPcBuildOrderRecord workOrder,
                    out CustomPcWorkTicketRecord workTicket,
                    out PcValidationReceipt validationReceipt,
                    out PcPowerStateReceipt powerOffReceipt) ||
                powerOffReceipt == null ||
                powerState.Revision != powerOffReceipt.Revision)
            {
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.NotCurrent);
            }

            if (session.TryGetQualityRelease(
                    out CustomPcQualityReleaseAuthority authority))
            {
                OperationResult history = authority.ValidateReceiptHistory();
                if (history.IsFailure)
                {
                    return history;
                }
            }

            if (_isReviewingQualityRelease &&
                (!ReferenceEquals(_reviewedQualityWorkOrder, workOrder) ||
                 !ReferenceEquals(_reviewedQualityWorkTicket, workTicket) ||
                 !ReferenceEquals(
                     _reviewedQualityValidationReceipt,
                     validationReceipt) ||
                 !ReferenceEquals(
                     _reviewedQualityPowerOffReceipt,
                     powerOffReceipt)))
            {
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.NotCurrent);
            }

            return OperationResult.Success();
        }

        private OperationResult TryAttemptQualityReleaseAuthorized()
        {
            if (_lastSuccessfulOperationFrame == UnityEngine.Time.frameCount)
            {
                return OperationResult.Fail(
                    ElectricalPowerTestStationFailures.InputReplay);
            }

            GarageStockFlowSession session = ResolveSession();
            if (session == null ||
                !session.TryGetQualityReleaseCandidate(
                    out CustomPcBuildOrderRecord workOrder,
                    out CustomPcWorkTicketRecord workTicket,
                    out PcValidationReceipt validationReceipt,
                    out PcPowerStateReceipt powerOffReceipt) ||
                powerOffReceipt == null)
            {
                ResetQualityReleaseReview();
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.NotCurrent);
            }

            if (!_isReviewingQualityRelease)
            {
                _isReviewingQualityRelease = true;
                _reviewedQualityWorkOrder = workOrder;
                _reviewedQualityWorkTicket = workTicket;
                _reviewedQualityValidationReceipt = validationReceipt;
                _reviewedQualityPowerOffReceipt = powerOffReceipt;
                _lastSuccessfulOperationFrame = UnityEngine.Time.frameCount;
                readinessProjection?.ObserveQualityReleaseReview();
                return OperationResult.Success();
            }

            if (!ReferenceEquals(_reviewedQualityWorkOrder, workOrder) ||
                !ReferenceEquals(_reviewedQualityWorkTicket, workTicket) ||
                !ReferenceEquals(
                    _reviewedQualityValidationReceipt,
                    validationReceipt) ||
                !ReferenceEquals(
                    _reviewedQualityPowerOffReceipt,
                    powerOffReceipt))
            {
                ResetQualityReleaseReview();
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.NotCurrent);
            }

            OperationResult<CustomPcQualityReleaseAuthority> ensured =
                session.EnsureQualityReleaseAuthority();
            if (ensured.IsFailure)
            {
                ResetQualityReleaseReview();
                return OperationResult.Fail(ensured.Error);
            }

            CustomPcQualityReleaseAuthority authority = ensured.Value;
            OperationResult<CustomPcQualityReleaseReceipt> released =
                authority.TryReleaseForPackaging(
                    session.CreatePrototypeQualityReleaseOperationId(
                        validationReceipt,
                        powerOffReceipt,
                        authority.Revision),
                    workOrder,
                    workTicket,
                    validationReceipt,
                    powerOffReceipt,
                    authority.Revision);
            if (released.IsFailure)
            {
                ResetQualityReleaseReviewState();
                readinessProjection?.ObserveQualityReleaseRejected(
                    released.Error);
                return OperationResult.Fail(released.Error);
            }

            _lastSuccessfulOperationFrame = UnityEngine.Time.frameCount;
            ResetQualityReleaseReviewState();
            readinessProjection?.ObserveQualityReleaseWaiting();
            return OperationResult.Success();
        }

        private string BuildQualityReleasePrompt(
            GarageStockFlowSession session,
            string interactBinding)
        {
            if (session == null ||
                !session.TryGetQualityReleaseCandidate(
                    out CustomPcBuildOrderRecord workOrder,
                    out CustomPcWorkTicketRecord workTicket,
                    out PcValidationReceipt validationReceipt,
                    out PcPowerStateReceipt powerOffReceipt) ||
                powerOffReceipt == null)
            {
                return string.Empty;
            }

            string primaryBinding = playerInput != null
                ? playerInput.PrimaryBindingPrompt
                : "LMB / RT";
            if (session.TryGetQualityRelease(
                    out CustomPcQualityReleaseAuthority authority))
            {
                OperationResult history = authority.ValidateReceiptHistory();
                if (history.IsFailure)
                {
                    return $"{interactBinding}: GÜCÜ AÇ • " +
                           "KALİTE KAYDI ENGELLİ • " + history.Error.Code;
                }

                OperationResult<CustomPcQualityReleaseReceipt> current =
                    authority.EvaluateCurrentRelease();
                if (current.IsSuccess)
                {
                    return "PAKETLEMEYE HAZIR • " +
                           $"{ResolveValidationQualityLabel(current.Value.QualityTier)} " +
                           $"• SCORE {current.Value.BenchmarkScore} • " +
                           $"{primaryBinding}: KALİTEYİ YENİDEN İNCELE • " +
                           $"{interactBinding}: YENİ CYCLE İÇİN GÜCÜ AÇ";
                }
            }

            if (_isReviewingQualityRelease &&
                ReferenceEquals(_reviewedQualityWorkOrder, workOrder) &&
                ReferenceEquals(_reviewedQualityWorkTicket, workTicket) &&
                ReferenceEquals(
                    _reviewedQualityValidationReceipt,
                    validationReceipt) &&
                ReferenceEquals(
                    _reviewedQualityPowerOffReceipt,
                    powerOffReceipt))
            {
                return "KALİTE DOSYASI • EXACT İŞ EMRİ + SAFE SHUTDOWN • " +
                       $"{primaryBinding}: PAKETLEME SERBEST BIRAK • " +
                       $"{interactBinding}: GÜCÜ AÇ";
            }

            if (LastFailureCode.StartsWith(
                    "quality.custom-pc-release.",
                    System.StringComparison.Ordinal))
            {
                return "KALİTE ONAYI REDDEDİLDİ • " + LastFailureCode + " • " +
                       $"{primaryBinding}: TEKRAR İNCELE • " +
                       $"{interactBinding}: GÜCÜ AÇ";
            }

            return "VALIDATION GEÇTİ • GÜVENLİ KAPATILDI • " +
                   $"{primaryBinding}: KALİTE DOSYASINI İNCELE • " +
                   $"{interactBinding}: GÜCÜ AÇ";
        }

        private void ResetQualityReleaseReviewIfContextChanged()
        {
            if (!_isReviewingQualityRelease)
            {
                return;
            }

            if (playerInput == null || playerMotor == null ||
                playerMotor.IsPaused || playerInput.PausePressedThisFrame ||
                PlayerIsBusy() || PlayerHasCompetingWorldInteractOwner() ||
                !_isFocused)
            {
                ResetQualityReleaseReview();
                return;
            }

            GarageStockFlowSession session = ResolveSession();
            if (session != null &&
                session.TryGetPowerState(out PcPowerStateAuthority powerState) &&
                !powerState.IsEnergized &&
                session.TryGetQualityReleaseCandidate(
                    out CustomPcBuildOrderRecord workOrder,
                    out CustomPcWorkTicketRecord workTicket,
                    out PcValidationReceipt validationReceipt,
                    out PcPowerStateReceipt powerOffReceipt) &&
                powerOffReceipt != null &&
                powerState.Revision == powerOffReceipt.Revision &&
                ReferenceEquals(_reviewedQualityWorkOrder, workOrder) &&
                ReferenceEquals(_reviewedQualityWorkTicket, workTicket) &&
                ReferenceEquals(
                    _reviewedQualityValidationReceipt,
                    validationReceipt) &&
                ReferenceEquals(
                    _reviewedQualityPowerOffReceipt,
                    powerOffReceipt))
            {
                return;
            }

            ResetQualityReleaseReview();
        }

        private void ResetQualityReleaseReview()
        {
            ResetQualityReleaseReviewState();
            readinessProjection?.ObserveQualityReleaseWaiting();
        }

        private void ResetQualityReleaseReviewState()
        {
            _isReviewingQualityRelease = false;
            _reviewedQualityWorkOrder = null;
            _reviewedQualityWorkTicket = null;
            _reviewedQualityValidationReceipt = null;
            _reviewedQualityPowerOffReceipt = null;
        }
    }
}
