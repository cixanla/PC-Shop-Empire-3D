using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Events
{
    public sealed class DomainEventIdScope : IStableIdScope
    {
        private DomainEventIdScope()
        {
        }
    }

    public sealed class DomainEventTypeScope : IStableIdScope
    {
        private DomainEventTypeScope()
        {
        }
    }

    public sealed class DomainCorrelationIdScope : IStableIdScope
    {
        private DomainCorrelationIdScope()
        {
        }
    }
}
