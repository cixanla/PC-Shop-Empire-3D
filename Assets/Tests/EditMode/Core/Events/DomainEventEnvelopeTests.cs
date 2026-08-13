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
                RootContext(),
                payload);

            Assert.That(envelope.Id.Value, Is.EqualTo("event.0001"));
            Assert.That(envelope.Type.Value, Is.EqualTo("inventory.stock-reserved"));
            Assert.That(envelope.Sequence.Value, Is.EqualTo(7));
            Assert.That(envelope.OccurredAt, Is.EqualTo(SimulationTimestamp.Create(12, 5000)));
            Assert.That(envelope.SchemaVersion, Is.EqualTo(1));
            Assert.That(envelope.Context, Is.EqualTo(RootContext()));
            Assert.That(envelope.Context.IsRoot, Is.True);
            Assert.That(envelope.Payload, Is.SameAs(payload));
            Assert.That(envelope.PayloadType, Is.EqualTo(typeof(StockReservedEvent)));

            IDomainEventEnvelope untyped = envelope;
            Assert.That(untyped.Payload, Is.SameAs(payload));
            Assert.That(untyped.PayloadType, Is.EqualTo(typeof(StockReservedEvent)));
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
                RootContext(),
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
                RootContext(),
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
                RootContext(),
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
                RootContext(),
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
                RootContext(),
                null));
        }

        [Test]
        public void ChildContextInheritsCorrelationAndPointsToDirectParent()
        {
            var parent = new DomainEventEnvelope<StockReservedEvent>(
                EventId(),
                EventType(),
                DomainEventSequence.From(1),
                SimulationTimestamp.Origin,
                1,
                RootContext(),
                new StockReservedEvent(1));
            DomainEventContext childContext = DomainEventContext.CausedBy(parent);
            var child = new DomainEventEnvelope<StockReservedEvent>(
                StableId<DomainEventIdScope>.Parse("event.0002"),
                EventType(),
                DomainEventSequence.From(2),
                SimulationTimestamp.Create(0, 1),
                1,
                childContext,
                new StockReservedEvent(1));

            Assert.That(child.Context.CorrelationId, Is.EqualTo(parent.Context.CorrelationId));
            Assert.That(child.Context.CausationId, Is.EqualTo(parent.Id));
            Assert.That(child.Context.IsRoot, Is.False);
        }

        [Test]
        public void ContextRejectsEmptyCorrelationAndNullParent()
        {
            Assert.Throws<ArgumentException>(() => DomainEventContext.Root(default));
            Assert.Throws<ArgumentException>(() => DomainEventContext.FromMetadata(default, EventId()));
            Assert.Throws<ArgumentNullException>(() => DomainEventContext.CausedBy(null));
        }

        [Test]
        public void EnvelopeRejectsDefaultContextAndSelfCausation()
        {
            Assert.Throws<ArgumentException>(() => new DomainEventEnvelope<StockReservedEvent>(
                EventId(),
                EventType(),
                DomainEventSequence.From(1),
                SimulationTimestamp.Origin,
                1,
                default,
                new StockReservedEvent(1)));

            DomainEventContext selfCausation = DomainEventContext.FromMetadata(
                CorrelationId(),
                EventId());
            Assert.Throws<ArgumentException>(() => new DomainEventEnvelope<StockReservedEvent>(
                EventId(),
                EventType(),
                DomainEventSequence.From(1),
                SimulationTimestamp.Origin,
                1,
                selfCausation,
                new StockReservedEvent(1)));
        }

        private static StableId<DomainEventIdScope> EventId()
        {
            return StableId<DomainEventIdScope>.Parse("event.0001");
        }

        private static StableId<DomainEventTypeScope> EventType()
        {
            return StableId<DomainEventTypeScope>.Parse("inventory.stock-reserved");
        }

        private static StableId<DomainCorrelationIdScope> CorrelationId()
        {
            return StableId<DomainCorrelationIdScope>.Parse("operation.sale-0001");
        }

        private static DomainEventContext RootContext()
        {
            return DomainEventContext.Root(CorrelationId());
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
