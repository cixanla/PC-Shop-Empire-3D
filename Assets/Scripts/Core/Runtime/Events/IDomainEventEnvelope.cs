using System;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Read-only, non-generic view used by journals, diagnostics and dispatch infrastructure.
    /// </summary>
    public interface IDomainEventEnvelope
    {
        StableId<DomainEventIdScope> Id { get; }

        StableId<DomainEventTypeScope> Type { get; }

        DomainEventSequence Sequence { get; }

        SimulationTimestamp OccurredAt { get; }

        int SchemaVersion { get; }

        DomainEventContext Context { get; }

        DomainEventPayloadFingerprint PayloadFingerprint { get; }

        System.Type PayloadType { get; }

        IDomainEvent Payload { get; }
    }
}
