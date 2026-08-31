using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Quality;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        private CustomPcQualityReleaseAuthority _qualityRelease;
        private PcValidationReceipt _qualityCandidateValidation;
        private PcPowerStateReceipt _qualityCandidatePowerOff;

        public CustomPcQualityReleaseAuthority QualityRelease
        {
            get
            {
                OperationResult<CustomPcQualityReleaseAuthority> ensured =
                    EnsureQualityReleaseAuthority();
                return ensured.TryGetValue(
                    out CustomPcQualityReleaseAuthority authority)
                        ? authority
                        : null;
            }
        }

        public OperationResult<CustomPcQualityReleaseAuthority>
            EnsureQualityReleaseAuthority()
        {
            if (_qualityRelease != null)
            {
                return OperationResult<CustomPcQualityReleaseAuthority>.Success(
                    _qualityRelease);
            }

            if (_validation == null)
            {
                return OperationResult<CustomPcQualityReleaseAuthority>.Fail(
                    CustomPcQualityReleaseFailures.ConfigurationMissing);
            }

            OperationResult<CustomPcQualityReleaseAuthority> created =
                CustomPcQualityReleaseAuthority.Create(
                    CustomPcWorkOrders,
                    _validation);
            if (created.IsFailure)
            {
                return created;
            }

            _qualityRelease = created.Value;
            return OperationResult<CustomPcQualityReleaseAuthority>.Success(
                _qualityRelease);
        }

        public StableId<CustomPcQualityReleaseOperationIdScope>
            CreatePrototypeQualityReleaseOperationId(
                PcValidationReceipt validationReceipt,
                PcPowerStateReceipt powerOffReceipt,
                long expectedRevision)
        {
            long validationRevision = validationReceipt?.Revision ?? -1L;
            long powerOffRevision = powerOffReceipt?.Revision ?? -1L;
            return StableId<CustomPcQualityReleaseOperationIdScope>.Parse(
                "quality.custom-pc-release.prototype.validation-" +
                validationRevision + ".power-off-" + powerOffRevision +
                ".run-" + (expectedRevision + 1L));
        }

        internal OperationResult ObserveValidationCandidateForQuality(
            PcValidationReceipt receipt)
        {
            if (_validation == null || receipt == null ||
                !_validation.TryGetReceipt(
                    receipt.OperationId,
                    out PcValidationReceipt owned) ||
                !ReferenceEquals(owned, receipt))
            {
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.ValidationReceiptInvalid);
            }

            _qualityCandidateValidation = receipt;
            _qualityCandidatePowerOff = null;
            return OperationResult.Success();
        }

        internal OperationResult ObservePowerOffCandidateForQuality(
            PcPowerStateReceipt receipt)
        {
            if (_qualityCandidateValidation == null || receipt == null ||
                receipt.TransitionKind != PcPowerTransitionKind.PowerOff ||
                !ReferenceEquals(
                    receipt.SourcePowerOnReceipt,
                    _qualityCandidateValidation.SourcePowerOnReceipt) ||
                _powerState == null ||
                !_powerState.TryGetReceipt(
                    receipt.OperationId,
                    out PcPowerStateReceipt owned) ||
                !ReferenceEquals(owned, receipt))
            {
                return OperationResult.Fail(
                    CustomPcQualityReleaseFailures.SafePowerOffMissing);
            }

            _qualityCandidatePowerOff = receipt;
            return OperationResult.Success();
        }

        internal void ObservePowerOnForQuality()
        {
            _qualityCandidatePowerOff = null;
        }

        public bool TryGetQualityReleaseCandidate(
            out CustomPcBuildOrderRecord workOrder,
            out CustomPcWorkTicketRecord workTicket,
            out PcValidationReceipt validationReceipt,
            out PcPowerStateReceipt powerOffReceipt)
        {
            validationReceipt = _qualityCandidateValidation;
            powerOffReceipt = _qualityCandidatePowerOff;
            if (validationReceipt != null &&
                TryGetPrototypeCustomPcBuildOrder(out workOrder) &&
                TryGetPrototypeCustomPcWorkTicket(out workTicket))
            {
                return true;
            }

            workOrder = null;
            workTicket = null;
            return false;
        }

        public bool TryGetQualityRelease(
            out CustomPcQualityReleaseAuthority authority)
        {
            authority = _qualityRelease;
            return authority != null;
        }
    }
}
