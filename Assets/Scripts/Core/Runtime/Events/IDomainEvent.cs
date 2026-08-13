namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Immutable fact emitted by a domain system. Implementations write every payload field in a
    /// stable order so the envelope can compute and bind a canonical SHA-256 fingerprint.
    /// </summary>
    public interface IDomainEvent
    {
        void WriteCanonicalPayload(DomainEventPayloadWriter writer);
    }
}
