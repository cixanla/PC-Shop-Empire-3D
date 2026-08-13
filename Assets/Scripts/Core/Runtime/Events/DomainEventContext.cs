using System;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Stable correlation and direct-causation metadata for a domain event.
    /// </summary>
    public readonly struct DomainEventContext : IEquatable<DomainEventContext>
    {
        private DomainEventContext(
            StableId<DomainCorrelationIdScope> correlationId,
            StableId<DomainEventIdScope> causationId)
        {
            CorrelationId = correlationId;
            CausationId = causationId;
        }

        public StableId<DomainCorrelationIdScope> CorrelationId { get; }

        public StableId<DomainEventIdScope> CausationId { get; }

        public bool IsRoot => CausationId.IsEmpty;

        public static DomainEventContext Root(StableId<DomainCorrelationIdScope> correlationId)
        {
            return FromMetadata(correlationId, default);
        }

        public static DomainEventContext CausedBy(IDomainEventEnvelope parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            return FromMetadata(parent.Context.CorrelationId, parent.Id);
        }

        public static DomainEventContext FromMetadata(
            StableId<DomainCorrelationIdScope> correlationId,
            StableId<DomainEventIdScope> causationId)
        {
            if (correlationId.IsEmpty)
            {
                throw new ArgumentException(
                    "A domain event context requires a stable correlation ID.",
                    nameof(correlationId));
            }

            return new DomainEventContext(correlationId, causationId);
        }

        public bool Equals(DomainEventContext other)
        {
            return CorrelationId.Equals(other.CorrelationId) && CausationId.Equals(other.CausationId);
        }

        public override bool Equals(object obj)
        {
            return obj is DomainEventContext other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (CorrelationId.GetHashCode() * 397) ^ CausationId.GetHashCode();
            }
        }

        public static bool operator ==(DomainEventContext left, DomainEventContext right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DomainEventContext left, DomainEventContext right)
        {
            return !left.Equals(right);
        }
    }
}
