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
                if (_powerTestAttempts == null && PowerBudget != null &&
                    AssemblyBuild != null)
                {
                    _powerTestAttempts = PowerTestAttemptAuthority.Create(
                        PowerBudget,
                        AssemblyBuild).Value;
                }

                return _powerTestAttempts;
            }
        }

        public StableId<PowerTestAttemptOperationIdScope>
            PrototypePowerTestAttemptOperationId =>
                StableId<PowerTestAttemptOperationIdScope>.Parse(
                    PrototypePowerTestAttemptOperationIdValue);
    }
}
