using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Events
{
    public interface IDomainEventHandler<TEvent>
        where TEvent : IDomainEvent
    {
        OperationResult Handle(DomainEventEnvelope<TEvent> envelope);
    }
}
