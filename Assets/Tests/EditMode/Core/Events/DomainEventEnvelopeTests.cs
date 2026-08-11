using System;
using NUnit.Framework;
using PCShopEmpire3D.Core.Events;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Tests.EditMode.Core.Events
{
    public sealed class DomainEventEnvelopeTests
    {
        [TestCase(0)]
        [TestCase(-1)]
        public void SequenceRejectsUnassignedValue(long value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DomainEventSequence.From(value));
        }

        [Test]
        public void SequenceHasCheckedSuccessor()
        {
            DomainEventSequence sequence = DomainEventSequence.From(41);

            Assert.That(sequence.Next().Value, Is.EqualTo(42));
            Assert.Throws<OverflowException>(() => DomainEventSequence.From(long.MaxValue).Next());
            Assert.Throws<InvalidOperationException>(() => default(DomainEventSequence).Next());
        }

        [Test]
        public void EnvelopePreservesStableReplayMetadata()
        {
            var payload = new StockReservedEvent(3);
            var envelope = new DomainEventEnvelope<StockReservedEvent>(
                StableId<DomainEventIdScope>.Parse("event.0001"),
                StableId<DomainEventTypeScope>.Parse("inventory.stock-reserved"),
                DomainEventSequence.From(7),
                SimulationTimestamp.Create(12, 5000),
                1,
                payload);

            Assert.That(envelope.Id.Value, Is.EqualTo("event.0001"));
            Assert.That(envelope.Type.Value, Is.EqualTo("inventory.stock-reserved"));
            Assert.That(envelope.Sequence.Value, Is.EqualTo(7));
            Assert.That(envelope.OccurredAt, Is.EqualTo(SimulationTimestamp.Create(12, 5000)));
            Assert.That(envelope.SchemaVersion, Is.EqualTo(1));
            Assert.That(envelope.Payload, Is.SameAs(payload));
        }

        [Test]
        public void EnvelopeRejectsEmptyEventId()
        {
            Assert.Throws<ArgumentException>(() => new DomainEventEnvelope<StockReservedEvent>(
                default,
                EventType(),
                DomainEventSequence.From(1),
                SimulationTimestamp.Origin,
                1,
                new StockReservedEvent(1)));
        }

        [Test]
        public void EnvelopeRejectsEmptyEventType()
        {
            Assert.Throws<ArgumentException>(() => new DomainEventEnvelope<StockReservedEvent>(
                EventId(),
                default,
                DomainEventSequence.From(1),
                SimulationTimestamp.Origin,
                1,
                new StockReservedEvent(1)));
        }

        [Test]
        public void EnvelopeRejectsUnassignedSequence()
        {
            Assert.Throws<ArgumentException>(() => new DomainEventEnvelope<StockReservedEvent>(
                EventId(),
                EventType(),
                default,
                SimulationTimestamp.Origin,
                1,
                new StockReservedEvent(1)));
        }

        [Test]
        public void EnvelopeRejectsInvalidSchemaVersion()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new DomainEventEnvelope<StockReservedEvent>(
                EventId(),
                EventType(),
                DomainEventSequence.From(1),
                SimulationTimestamp.Origin,
                0,
                new StockReservedEvent(1)));
        }

        [Test]
        public void EnvelopeRejectsNullPayload()
        {
            Assert.Throws<ArgumentNullException>(() => new DomainEventEnvelope<StockReservedEvent>(
                EventId(),
                EventType(),
                DomainEventSequence.From(1),
                SimulationTimestamp.Origin,
                1,
                null));
        }

        private static StableId<DomainEventIdScope> EventId()
        {
            return StableId<DomainEventIdScope>.Parse("event.0001");
        }

        private static StableId<DomainEventTypeScope> EventType()
        {
            return StableId<DomainEventTypeScope>.Parse("inventory.stock-reserved");
        }

        private sealed class StockReservedEvent : IDomainEvent
        {
            public StockReservedEvent(int quantity)
            {
                Quantity = quantity;
            }

            public int Quantity { get; }
        }
    }
}
