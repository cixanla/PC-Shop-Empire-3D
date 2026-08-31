using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        private PcFictionalOsInstallationAuthority _fictionalOsInstallation;

        public PcFictionalOsInstallationAuthority FictionalOsInstallation
        {
            get
            {
                OperationResult<PcFictionalOsInstallationAuthority> ensured =
                    EnsureFictionalOsInstallationAuthority();
                return ensured.TryGetValue(
                        out PcFictionalOsInstallationAuthority authority)
                    ? authority
                    : null;
            }
        }

        public OperationResult<PcFictionalOsInstallationAuthority>
            EnsureFictionalOsInstallationAuthority()
        {
            if (_fictionalOsInstallation != null)
            {
                return OperationResult<PcFictionalOsInstallationAuthority>
                    .Success(_fictionalOsInstallation);
            }

            OperationResult<PcPowerStateAuthority> ensuredPowerState =
                EnsurePowerStateAuthority();
            if (ensuredPowerState.IsFailure)
            {
                return OperationResult<PcFictionalOsInstallationAuthority>.Fail(
                    ensuredPowerState.Error);
            }

            OperationResult<PcFictionalOsInstallationAuthority> created =
                PcFictionalOsInstallationAuthority.Create(
                    ensuredPowerState.Value,
                    AssemblyBuild);
            if (created.IsFailure)
            {
                return OperationResult<PcFictionalOsInstallationAuthority>.Fail(
                    created.Error);
            }

            _fictionalOsInstallation = created.Value;
            return OperationResult<PcFictionalOsInstallationAuthority>.Success(
                _fictionalOsInstallation);
        }

        public StableId<PcFictionalOsInstallationOperationIdScope>
            CreatePrototypeFictionalOsInstallationOperationId(
                PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt)
        {
            long sourceRevision =
                sourceFirmwareBaselineReceipt?.Revision ?? -1L;
            return StableId<PcFictionalOsInstallationOperationIdScope>.Parse(
                "assembly.fictional-os.prototype.firmware-" + sourceRevision);
        }

        public bool TryGetFictionalOsInstallation(
            out PcFictionalOsInstallationAuthority authority)
        {
            authority = _fictionalOsInstallation;
            return authority != null;
        }

        public StableId<ItemInstanceIdScope> CurrentStorageItemId =>
            AssemblyBuild.StorageItemId;
    }
}
