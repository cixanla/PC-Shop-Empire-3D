using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Stable metadata wrapped around a domain fact for ordering, replay and duplicate detection.
    /// </summary>
    public sealed class DomainEventEnvelope<TEvent> : IDomainEventEnvelope
        where TEvent : IDomainEvent
    {
        public DomainEventEnvelope(
            StableId<DomainEventIdScope> id,
            StableId<DomainEventTypeScope> type,
            DomainEventSequence sequence,
            SimulationTimestamp occurredAt,
            int schemaVersion,
            DomainEventContext context,
            TEvent payload)
        {
            if (id.IsEmpty)
            {
                throw new ArgumentException("A domain event requires a stable event ID.", nameof(id));
            }

            if (type.IsEmpty)
            {
                throw new ArgumentException("A domain event requires a stable event type.", nameof(type));
            }

            if (!sequence.IsAssigned)
            {
                throw new ArgumentException("A domain event requires an assigned sequence.", nameof(sequence));
            }

            if (schemaVersion <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(schemaVersion),
                    schemaVersion,
                    "Domain event schema version must be greater than zero.");
            }

            if (context.CorrelationId.IsEmpty)
            {
                throw new ArgumentException(
                    "A domain event requires a stable correlation context.",
                    nameof(context));
            }

            if (context.CausationId == id)
            {
                throw new ArgumentException("A domain event cannot cause itself.", nameof(context));
            }

            if (ReferenceEquals(payload, null))
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.GetType() != typeof(TEvent))
            {
                throw new ArgumentException(
                    "A domain event payload must exactly match its generic contract type.",
                    nameof(payload));
            }

            Id = id;
            Type = type;
            Sequence = sequence;
            OccurredAt = occurredAt;
            SchemaVersion = schemaVersion;
            Context = context;
            Payload = payload;
        }

        public StableId<DomainEventIdScope> Id { get; }

        public StableId<DomainEventTypeScope> Type { get; }

        public DomainEventSequence Sequence { get; }

        public SimulationTimestamp OccurredAt { get; }

        public int SchemaVersion { get; }

        public DomainEventContext Context { get; }

        public Type PayloadType => typeof(TEvent);

        public TEvent Payload { get; }

        IDomainEvent IDomainEventEnvelope.Payload => Payload;
    }
}
