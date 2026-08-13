using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Randomness
{
    public sealed class RandomStreamDomainScope : IStableIdScope
    {
        private RandomStreamDomainScope()
        {
        }
    }

    public sealed class RandomStreamContextScope : IStableIdScope
    {
        private RandomStreamContextScope()
        {
        }
    }
}
