using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        private PcFictionalDriverInstallationAuthority
            _fictionalDriverInstallation;

        public PcFictionalDriverInstallationAuthority
            FictionalDriverInstallation
        {
            get
            {
                OperationResult<PcFictionalDriverInstallationAuthority>
                    ensured = EnsureFictionalDriverInstallationAuthority();
                return ensured.TryGetValue(
                        out PcFictionalDriverInstallationAuthority authority)
                    ? authority
                    : null;
            }
        }

        public OperationResult<PcFictionalDriverInstallationAuthority>
            EnsureFictionalDriverInstallationAuthority()
        {
            if (_fictionalDriverInstallation != null)
            {
                return OperationResult<
                    PcFictionalDriverInstallationAuthority>.Success(
                    _fictionalDriverInstallation);
            }

            OperationResult<PcFictionalOsInstallationAuthority> ensuredOs =
                EnsureFictionalOsInstallationAuthority();
            if (ensuredOs.IsFailure)
            {
                return OperationResult<
                    PcFictionalDriverInstallationAuthority>.Fail(
                    ensuredOs.Error);
            }

            OperationResult<PcFictionalDriverInstallationAuthority> created =
                PcFictionalDriverInstallationAuthority.Create(ensuredOs.Value);
            if (created.IsFailure)
            {
                return OperationResult<
                    PcFictionalDriverInstallationAuthority>.Fail(
                    created.Error);
            }

            _fictionalDriverInstallation = created.Value;
            return OperationResult<
                PcFictionalDriverInstallationAuthority>.Success(
                _fictionalDriverInstallation);
        }

        public StableId<PcFictionalDriverInstallationOperationIdScope>
            CreatePrototypeFictionalDriverInstallationOperationId(
                PcFictionalOsInstallationReceipt sourceOperatingSystemReceipt)
        {
            long sourceRevision =
                sourceOperatingSystemReceipt?.Revision ?? -1L;
            return StableId<
                PcFictionalDriverInstallationOperationIdScope>.Parse(
                "assembly.fictional-driver.prototype.os-" + sourceRevision);
        }

        public bool TryGetFictionalDriverInstallation(
            out PcFictionalDriverInstallationAuthority authority)
        {
            authority = _fictionalDriverInstallation;
            return authority != null;
        }
    }
}
