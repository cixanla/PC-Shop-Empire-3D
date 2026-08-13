using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Redacted deterministic handler failure metadata. Raw payload and stack trace are excluded.
    /// </summary>
    public readonly struct DomainEventDispatchFailure
    {
        internal DomainEventDispatchFailure(
            StableId<DomainEventHandlerIdScope> handlerId,
            IDomainEventEnvelope envelope,
            Failure failure,
            string exceptionType)
        {
            HandlerId = handlerId;
            EventId = envelope.Id;
            EventType = envelope.Type;
            Sequence = envelope.Sequence;
            CorrelationId = envelope.Context.CorrelationId;
            CausationId = envelope.Context.CausationId;
            Failure = failure;
            ExceptionType = exceptionType ?? string.Empty;
        }

        public StableId<DomainEventHandlerIdScope> HandlerId { get; }

        public StableId<DomainEventIdScope> EventId { get; }

        public StableId<DomainEventTypeScope> EventType { get; }

        public DomainEventSequence Sequence { get; }

        public StableId<DomainCorrelationIdScope> CorrelationId { get; }

        public StableId<DomainEventIdScope> CausationId { get; }

        public Failure Failure { get; }

        public string ExceptionType { get; }
    }
}
