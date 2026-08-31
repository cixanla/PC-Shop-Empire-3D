using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PrototypePowerTestAttemptOperationIdValue =
            "assembly.power-test-attempt.prototype-001";

        private PowerTestAttemptAuthority _powerTestAttempts;

        public PowerTestAttemptAuthority PowerTestAttempts
        {
            get
            {
                OperationResult<PowerTestAttemptAuthority> ensured =
                    EnsurePowerTestAttemptsAuthority();
                return ensured.TryGetValue(
                    out PowerTestAttemptAuthority powerTestAttempts)
                        ? powerTestAttempts
                        : null;
            }
        }

        public OperationResult<PowerTestAttemptAuthority>
            EnsurePowerTestAttemptsAuthority()
        {
            if (_powerTestAttempts != null)
            {
                return OperationResult<PowerTestAttemptAuthority>.Success(
                    _powerTestAttempts);
            }

            OperationResult<PowerTestAttemptAuthority> created =
                PowerTestAttemptAuthority.Create(PowerBudget, AssemblyBuild);
            if (created.IsFailure)
            {
                return OperationResult<PowerTestAttemptAuthority>.Fail(
                    created.Error);
            }

            _powerTestAttempts = created.Value;
            return OperationResult<PowerTestAttemptAuthority>.Success(
                _powerTestAttempts);
        }

        public StableId<PowerTestAttemptOperationIdScope>
            PrototypePowerTestAttemptOperationId =>
                StableId<PowerTestAttemptOperationIdScope>.Parse(
                    PrototypePowerTestAttemptOperationIdValue);

        public bool TryGetPowerTestAttempts(
            out PowerTestAttemptAuthority powerTestAttempts)
        {
            powerTestAttempts = _powerTestAttempts;
            return powerTestAttempts != null;
        }
    }
}
