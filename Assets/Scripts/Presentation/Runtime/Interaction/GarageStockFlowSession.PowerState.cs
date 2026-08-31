using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PrototypePowerOnOperationIdValue =
            "assembly.power-state.prototype.on-1";
        public const string PrototypePowerOffOperationIdValue =
            "assembly.power-state.prototype.off-2";

        private PcPowerStateAuthority _powerState;

        public PcPowerStateAuthority PowerState
        {
            get
            {
                OperationResult<PcPowerStateAuthority> ensured =
                    EnsurePowerStateAuthority();
                return ensured.TryGetValue(out PcPowerStateAuthority powerState)
                    ? powerState
                    : null;
            }
        }

        public OperationResult<PcPowerStateAuthority>
            EnsurePowerStateAuthority()
        {
            if (_powerState != null)
            {
                return OperationResult<PcPowerStateAuthority>.Success(_powerState);
            }

            OperationResult<PowerTestAttemptAuthority> ensuredAttempts =
                EnsurePowerTestAttemptsAuthority();
            if (ensuredAttempts.IsFailure)
            {
                return OperationResult<PcPowerStateAuthority>.Fail(
                    ensuredAttempts.Error);
            }

            OperationResult<PcPowerStateAuthority> created =
                PcPowerStateAuthority.Create(ensuredAttempts.Value, AssemblyBuild);
            if (created.IsFailure)
            {
                return OperationResult<PcPowerStateAuthority>.Fail(created.Error);
            }

            _powerState = created.Value;
            return OperationResult<PcPowerStateAuthority>.Success(_powerState);
        }

        public StableId<PcPowerStateOperationIdScope>
            PrototypePowerOnOperationId =>
                StableId<PcPowerStateOperationIdScope>.Parse(
                    PrototypePowerOnOperationIdValue);

        public StableId<PcPowerStateOperationIdScope>
            PrototypePowerOffOperationId =>
                StableId<PcPowerStateOperationIdScope>.Parse(
                    PrototypePowerOffOperationIdValue);

        public StableId<PcPowerStateOperationIdScope>
            CreatePrototypePowerStateOperationId(
                PcPowerTransitionKind transitionKind,
                long resultingRevision)
        {
            string transition = transitionKind == PcPowerTransitionKind.PowerOn
                ? "on"
                : "off";
            return StableId<PcPowerStateOperationIdScope>.Parse(
                "assembly.power-state.prototype." + transition + "-" +
                resultingRevision);
        }

        public StableId<PcPostStartupOperationIdScope>
            CreatePrototypePostStartupOperationId(
                PcPowerStateReceipt sourcePowerOnReceipt)
        {
            long sourceRevision = sourcePowerOnReceipt?.Revision ?? -1L;
            return StableId<PcPostStartupOperationIdScope>.Parse(
                "assembly.post-startup.prototype.power-on-" + sourceRevision);
        }

        public StableId<PcFirmwareBaselineOperationIdScope>
            CreatePrototypeFirmwareBaselineOperationId(
                PcPostStartupReceipt sourcePostStartupReceipt)
        {
            long sourceRevision = sourcePostStartupReceipt?.Revision ?? -1L;
            return StableId<PcFirmwareBaselineOperationIdScope>.Parse(
                "assembly.firmware-baseline.prototype.post-" + sourceRevision);
        }

        public bool TryGetPowerState(out PcPowerStateAuthority powerState)
        {
            powerState = _powerState;
            return powerState != null;
        }
    }
}
