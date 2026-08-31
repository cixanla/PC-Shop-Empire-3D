using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    public sealed partial class AssemblyBuildAuthority
    {
        private PcPowerStateAuthority _electricalPowerStateOwner;
        private bool _isElectricallyEnergized;

        public bool IsElectricallyEnergized => _isElectricallyEnergized;

        internal OperationResult BindElectricalPowerState(
            PcPowerStateAuthority owner)
        {
            if (owner == null || !ReferenceEquals(owner.AssemblyBuild, this))
            {
                return OperationResult.Fail(
                    PcPowerStateFailures.AuthorityMismatch);
            }

            if (_electricalPowerStateOwner != null)
            {
                return OperationResult.Fail(PcPowerStateFailures.AlreadyBound);
            }

            _electricalPowerStateOwner = owner;
            return OperationResult.Success();
        }

        internal OperationResult SetElectricalPowerState(
            PcPowerStateAuthority owner,
            bool energized)
        {
            if (owner == null ||
                !ReferenceEquals(_electricalPowerStateOwner, owner) ||
                !ReferenceEquals(owner.AssemblyBuild, this))
            {
                return OperationResult.Fail(
                    PcPowerStateFailures.AuthorityMismatch);
            }

            if (_isElectricallyEnergized == energized)
            {
                return OperationResult.Fail(
                    PcPowerStateFailures.InterlockRejected);
            }

            _isElectricallyEnergized = energized;
            return OperationResult.Success();
        }

        internal bool IsElectricalPowerStateBoundTo(
            PcPowerStateAuthority owner)
        {
            return owner != null &&
                   ReferenceEquals(_electricalPowerStateOwner, owner) &&
                   ReferenceEquals(owner.AssemblyBuild, this);
        }

        private Failure ValidateElectricalMaintenanceInterlock()
        {
            return _isElectricallyEnergized
                ? AssemblyFailures.ElectricalPowerOnMaintenanceBlocked
                : Failure.None;
        }

        private bool ValidateElectricalMaintenanceInterlockInvariant()
        {
            return _electricalPowerStateOwner == null
                ? !_isElectricallyEnergized
                : ReferenceEquals(_electricalPowerStateOwner.AssemblyBuild, this);
        }
    }
}
