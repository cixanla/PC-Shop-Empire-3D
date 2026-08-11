using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Stable metadata wrapped around a domain fact for ordering, replay and duplicate detection.
    /// </summary>
    public sealed class DomainEventEnvelope<TEvent>
        where TEvent : IDomainEvent
    {
        public DomainEventEnvelope(
            StableId<DomainEventIdScope> id,
            StableId<DomainEventTypeScope> type,
            DomainEventSequence sequence,
            SimulationTimestamp occurredAt,
            int schemaVersion,
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

            if (ReferenceEquals(payload, null))
            {
                throw new ArgumentNullException(nameof(payload));
            }

            Id = id;
            Type = type;
            Sequence = sequence;
            OccurredAt = occurredAt;
            SchemaVersion = schemaVersion;
            Payload = payload;
        }

        public StableId<DomainEventIdScope> Id { get; }

        public StableId<DomainEventTypeScope> Type { get; }

        public DomainEventSequence Sequence { get; }

        public SimulationTimestamp OccurredAt { get; }

        public int SchemaVersion { get; }

        public TEvent Payload { get; }
    }
}
