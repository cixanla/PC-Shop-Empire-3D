using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Single-threaded deterministic dispatcher with FIFO, breadth-first reentrancy and process-local deduplication.
    /// </summary>
    public sealed class InMemoryDomainEventDispatcher
    {
        private static readonly Failure DuplicateHandlerFailure = Failure.FromCode("events.handler.duplicate");
        private static readonly Failure RegistrationLockedFailure = Failure.FromCode("events.registration.locked");
        private static readonly Failure SequenceFailure = Failure.FromCode("events.enqueue.sequence");
        private static readonly Failure SequenceExhaustedFailure = Failure.FromCode("events.enqueue.sequence-exhausted");
        private static readonly Failure DuplicateConflictFailure = Failure.FromCode("events.enqueue.duplicate-conflict");
        private static readonly Failure HandlerExceptionFailure = Failure.FromCode("events.handler.exception");

        private readonly Dictionary<Type, List<IHandlerAdapter>> _handlersByPayloadType =
            new Dictionary<Type, List<IHandlerAdapter>>();
        private readonly HashSet<string> _handlerIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, EventReceipt> _receipts =
            new Dictionary<string, EventReceipt>(StringComparer.Ordinal);
        private readonly Queue<IDomainEventEnvelope> _queue = new Queue<IDomainEventEnvelope>();

        private long _lastAcceptedSequence;
        private DomainEventSequence _lastConsumedSequence;
        private bool _registrationLocked;
        private bool _isDraining;

        public InMemoryDomainEventDispatcher(long lastCommittedSequence = 0)
        {
            if (lastCommittedSequence < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lastCommittedSequence),
                    lastCommittedSequence,
                    "The last committed event sequence cannot be negative.");
            }

            _lastAcceptedSequence = lastCommittedSequence;
            _lastConsumedSequence = lastCommittedSequence == 0
                ? default
                : DomainEventSequence.From(lastCommittedSequence);
        }

        public int PendingCount => _queue.Count;

        public long LastAcceptedSequence => _lastAcceptedSequence;

        public DomainEventSequence LastConsumedSequence => _lastConsumedSequence;

        public OperationResult Register<TEvent>(
            StableId<DomainEventHandlerIdScope> handlerId,
            IDomainEventHandler<TEvent> handler)
            where TEvent : IDomainEvent
        {
            if (handlerId.IsEmpty)
            {
                throw new ArgumentException("A domain event handler requires a stable ID.", nameof(handlerId));
            }

            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (_registrationLocked)
            {
                return OperationResult.Fail(RegistrationLockedFailure);
            }

            if (!_handlerIds.Add(handlerId.Value))
            {
                return OperationResult.Fail(DuplicateHandlerFailure);
            }

            Type payloadType = typeof(TEvent);
            if (!_handlersByPayloadType.TryGetValue(payloadType, out List<IHandlerAdapter> handlers))
            {
                handlers = new List<IHandlerAdapter>();
                _handlersByPayloadType.Add(payloadType, handlers);
            }

            handlers.Add(new HandlerAdapter<TEvent>(handlerId, handler));
            return OperationResult.Success();
        }

        public DomainEventEnqueueResult Enqueue<TEvent>(DomainEventEnvelope<TEvent> envelope)
            where TEvent : IDomainEvent
        {
            if (envelope == null)
            {
                throw new ArgumentNullException(nameof(envelope));
            }

            string eventId = envelope.Id.Value;
            var receipt = new EventReceipt(envelope);
            if (_receipts.TryGetValue(eventId, out EventReceipt existingReceipt))
            {
                return existingReceipt.Equals(receipt)
                    ? DomainEventEnqueueResult.Duplicate()
                    : DomainEventEnqueueResult.Rejected(DuplicateConflictFailure);
            }

            if (_lastAcceptedSequence == long.MaxValue)
            {
                return DomainEventEnqueueResult.Rejected(SequenceExhaustedFailure);
            }

            long expectedSequence = _lastAcceptedSequence + 1;
            if (envelope.Sequence.Value != expectedSequence)
            {
                return DomainEventEnqueueResult.Rejected(SequenceFailure);
            }

            _receipts.Add(eventId, receipt);
            _lastAcceptedSequence = envelope.Sequence.Value;
            _queue.Enqueue(envelope);
            return DomainEventEnqueueResult.Accepted();
        }

        public DomainEventDispatchReport Drain(int maxEvents)
        {
            if (maxEvents <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxEvents),
                    maxEvents,
                    "A domain event drain budget must be positive.");
            }

            if (_isDraining)
            {
                throw new InvalidOperationException("A domain event dispatcher cannot drain recursively.");
            }

            _registrationLocked = true;
            _isDraining = true;
            int processedEventCount = 0;
            int handlerInvocationCount = 0;
            var failures = new List<DomainEventDispatchFailure>();

            try
            {
                while (processedEventCount < maxEvents && _queue.Count > 0)
                {
                    IDomainEventEnvelope envelope = _queue.Dequeue();
                    processedEventCount++;

                    if (_handlersByPayloadType.TryGetValue(
                        envelope.PayloadType,
                        out List<IHandlerAdapter> handlers))
                    {
                        for (int index = 0; index < handlers.Count; index++)
                        {
                            IHandlerAdapter handler = handlers[index];
                            handlerInvocationCount++;
                            try
                            {
                                OperationResult result = handler.Invoke(envelope);
                                if (result.IsFailure)
                                {
                                    failures.Add(new DomainEventDispatchFailure(
                                        handler.Id,
                                        envelope,
                                        result.Error,
                                        string.Empty));
                                }
                            }
                            catch (Exception exception) when (!IsFatal(exception))
                            {
                                failures.Add(new DomainEventDispatchFailure(
                                    handler.Id,
                                    envelope,
                                    HandlerExceptionFailure,
                                    exception.GetType().FullName));
                            }
                        }
                    }

                    _lastConsumedSequence = envelope.Sequence;
                }
            }
            finally
            {
                _isDraining = false;
            }

            return new DomainEventDispatchReport(
                processedEventCount,
                handlerInvocationCount,
                _queue.Count,
                _lastConsumedSequence,
                failures);
        }

        private static bool IsFatal(Exception exception)
        {
            return exception is OutOfMemoryException ||
                   exception is StackOverflowException ||
                   exception is AccessViolationException ||
                   exception is AppDomainUnloadedException ||
                   exception is BadImageFormatException ||
                   exception is CannotUnloadAppDomainException ||
                   exception is InvalidProgramException;
        }

        private interface IHandlerAdapter
        {
            StableId<DomainEventHandlerIdScope> Id { get; }

            OperationResult Invoke(IDomainEventEnvelope envelope);
        }

        private sealed class HandlerAdapter<TEvent> : IHandlerAdapter
            where TEvent : IDomainEvent
        {
            private readonly IDomainEventHandler<TEvent> _handler;

            public HandlerAdapter(
                StableId<DomainEventHandlerIdScope> id,
                IDomainEventHandler<TEvent> handler)
            {
                Id = id;
                _handler = handler;
            }

            public StableId<DomainEventHandlerIdScope> Id { get; }

            public OperationResult Invoke(IDomainEventEnvelope envelope)
            {
                return _handler.Handle((DomainEventEnvelope<TEvent>)envelope);
            }
        }

        private readonly struct EventReceipt : IEquatable<EventReceipt>
        {
            public EventReceipt(IDomainEventEnvelope envelope)
            {
                Type = envelope.Type;
                Sequence = envelope.Sequence;
                OccurredAt = envelope.OccurredAt;
                SchemaVersion = envelope.SchemaVersion;
                Context = envelope.Context;
                PayloadType = envelope.PayloadType;
            }

            private StableId<DomainEventTypeScope> Type { get; }

            private DomainEventSequence Sequence { get; }

            private Time.SimulationTimestamp OccurredAt { get; }

            private int SchemaVersion { get; }

            private DomainEventContext Context { get; }

            private Type PayloadType { get; }

            public bool Equals(EventReceipt other)
            {
                return Type.Equals(other.Type) &&
                       Sequence.Equals(other.Sequence) &&
                       OccurredAt.Equals(other.OccurredAt) &&
                       SchemaVersion == other.SchemaVersion &&
                       Context.Equals(other.Context) &&
                       PayloadType == other.PayloadType;
            }

            public override bool Equals(object obj)
            {
                return obj is EventReceipt other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = Type.GetHashCode();
                    hashCode = (hashCode * 397) ^ Sequence.GetHashCode();
                    hashCode = (hashCode * 397) ^ OccurredAt.GetHashCode();
                    hashCode = (hashCode * 397) ^ SchemaVersion;
                    hashCode = (hashCode * 397) ^ Context.GetHashCode();
                    hashCode = (hashCode * 397) ^ PayloadType.GetHashCode();
                    return hashCode;
                }
            }
        }
    }
}
