using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using PCShopEmpire3D.Core.Events;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Tests.EditMode.Core.Events
{
    public sealed class InMemoryDomainEventDispatcherTests
    {
        [Test]
        public void DispatchesGlobalFifoUsingExactTypeAndRegistrationOrder()
        {
            var execution = new List<string>();
            var dispatcher = new InMemoryDomainEventDispatcher();
            Assert.That(dispatcher.Register(HandlerId("handler.alpha.first"),
                Handler<AlphaEvent>(envelope =>
                {
                    execution.Add($"alpha-first-{envelope.Sequence.Value}");
                    return OperationResult.Success();
                })).IsSuccess, Is.True);
            Assert.That(dispatcher.Register(HandlerId("handler.alpha.second"),
                Handler<AlphaEvent>(envelope =>
                {
                    execution.Add($"alpha-second-{envelope.Sequence.Value}");
                    return OperationResult.Success();
                })).IsSuccess, Is.True);
            Assert.That(dispatcher.Register(HandlerId("handler.beta"),
                Handler<BetaEvent>(envelope =>
                {
                    execution.Add($"beta-{envelope.Sequence.Value}");
                    return OperationResult.Success();
                })).IsSuccess, Is.True);

            Assert.That(dispatcher.Enqueue(Alpha(1)).IsAccepted, Is.True);
            Assert.That(dispatcher.Enqueue(Beta(2)).IsAccepted, Is.True);

            DomainEventDispatchReport report = dispatcher.Drain(10);

            Assert.That(execution, Is.EqualTo(new[]
            {
                "alpha-first-1",
                "alpha-second-1",
                "beta-2"
            }));
            Assert.That(report.ProcessedEventCount, Is.EqualTo(2));
            Assert.That(report.HandlerInvocationCount, Is.EqualTo(3));
            Assert.That(report.Failures, Is.Empty);
            Assert.That(report.RemainingEventCount, Is.Zero);
            Assert.That(report.LastConsumedSequence.Value, Is.EqualTo(2));
        }

        [Test]
        public void NestedEnqueueIsBreadthFirstInsteadOfRecursive()
        {
            var execution = new List<string>();
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Register(HandlerId("handler.publisher"), Handler<AlphaEvent>(envelope =>
            {
                execution.Add($"publisher-{envelope.Sequence.Value}");
                if (envelope.Sequence.Value == 1)
                {
                    DomainEventEnqueueResult nested = dispatcher.Enqueue(Alpha(
                        2,
                        "event.0002",
                        DomainEventContext.CausedBy(envelope)));
                    Assert.That(nested.IsAccepted, Is.True);
                }

                return OperationResult.Success();
            }));
            dispatcher.Register(HandlerId("handler.observer"), Handler<AlphaEvent>(envelope =>
            {
                execution.Add($"observer-{envelope.Sequence.Value}");
                return OperationResult.Success();
            }));
            dispatcher.Enqueue(Alpha(1));

            dispatcher.Drain(10);

            Assert.That(execution, Is.EqualTo(new[]
            {
                "publisher-1",
                "observer-1",
                "publisher-2",
                "observer-2"
            }));
        }

        [Test]
        public void DrainBudgetBoundsADeepUniqueEventChainWithoutStackGrowth()
        {
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Register(HandlerId("handler.chain"), Handler<AlphaEvent>(envelope =>
            {
                long sequence = envelope.Sequence.Value;
                if (sequence < 1000)
                {
                    dispatcher.Enqueue(Alpha(
                        sequence + 1,
                        $"event.chain.{sequence + 1:0000}",
                        DomainEventContext.CausedBy(envelope)));
                }

                return OperationResult.Success();
            }));
            dispatcher.Enqueue(Alpha(1, "event.chain.0001"));

            DomainEventDispatchReport first = dispatcher.Drain(500);
            DomainEventDispatchReport second = dispatcher.Drain(500);

            Assert.That(first.ProcessedEventCount, Is.EqualTo(500));
            Assert.That(first.RemainingEventCount, Is.EqualTo(1));
            Assert.That(first.ReachedBudget, Is.True);
            Assert.That(first.LastConsumedSequence.Value, Is.EqualTo(500));
            Assert.That(second.ProcessedEventCount, Is.EqualTo(500));
            Assert.That(second.RemainingEventCount, Is.Zero);
            Assert.That(second.ReachedBudget, Is.False);
            Assert.That(second.LastConsumedSequence.Value, Is.EqualTo(1000));
        }

        [Test]
        public void RestoreCursorAcceptsOnlyTheImmediateNextSequence()
        {
            var dispatcher = new InMemoryDomainEventDispatcher(41);

            DomainEventEnqueueResult gap = dispatcher.Enqueue(Alpha(43, "event.0043"));
            DomainEventEnqueueResult reverse = dispatcher.Enqueue(Alpha(41, "event.0041"));
            DomainEventEnqueueResult expected = dispatcher.Enqueue(Alpha(42, "event.0042"));

            Assert.That(gap.IsRejected, Is.True);
            Assert.That(gap.Error.Code, Is.EqualTo("events.enqueue.sequence"));
            Assert.That(reverse.IsRejected, Is.True);
            Assert.That(expected.IsAccepted, Is.True);
            Assert.That(dispatcher.LastAcceptedSequence, Is.EqualTo(42));
        }

        [Test]
        public void ExactMetadataDuplicateIsIdempotentAndNeverRunsTwice()
        {
            int calls = 0;
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Register(HandlerId("handler.counter"), Handler<AlphaEvent>(_ =>
            {
                calls++;
                return OperationResult.Success();
            }));
            DomainEventEnvelope<AlphaEvent> first = Alpha(1);
            DomainEventEnvelope<AlphaEvent> replay = Alpha(1);

            Assert.That(dispatcher.Enqueue(first).IsAccepted, Is.True);
            DomainEventEnqueueResult duplicate = dispatcher.Enqueue(replay);
            DomainEventDispatchReport report = dispatcher.Drain(10);

            Assert.That(duplicate.IsDuplicate, Is.True);
            Assert.That(duplicate.Error.IsNone, Is.True);
            Assert.That(calls, Is.EqualTo(1));
            Assert.That(report.ProcessedEventCount, Is.EqualTo(1));
        }

        [Test]
        public void ReusedIdWithDifferentMetadataIsAConflict()
        {
            var dispatcher = new InMemoryDomainEventDispatcher();
            DomainEventEnvelope<AlphaEvent> first = Alpha(1);
            var conflict = new DomainEventEnvelope<AlphaEvent>(
                first.Id,
                first.Type,
                first.Sequence,
                first.OccurredAt,
                2,
                first.Context,
                new AlphaEvent("changed"));

            Assert.That(dispatcher.Enqueue(first).IsAccepted, Is.True);
            DomainEventEnqueueResult result = dispatcher.Enqueue(conflict);

            Assert.That(result.IsRejected, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("events.enqueue.duplicate-conflict"));
            Assert.That(dispatcher.PendingCount, Is.EqualTo(1));
        }

        [Test]
        public void HandlerFailureAndExceptionDoNotStopOtherHandlersOrEvents()
        {
            int successfulCalls = 0;
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Register(HandlerId("handler.failure"), Handler<AlphaEvent>(_ =>
                OperationResult.Fail(Failure.FromCode("inventory.test-failure"))));
            dispatcher.Register(HandlerId("handler.exception"), Handler<AlphaEvent>(_ =>
                throw new InvalidOperationException("secret payload text must not enter report")));
            dispatcher.Register(HandlerId("handler.success"), Handler<AlphaEvent>(_ =>
            {
                successfulCalls++;
                return OperationResult.Success();
            }));
            dispatcher.Enqueue(Alpha(1));
            dispatcher.Enqueue(Alpha(2, "event.0002"));

            DomainEventDispatchReport report = dispatcher.Drain(10);

            Assert.That(successfulCalls, Is.EqualTo(2));
            Assert.That(report.ProcessedEventCount, Is.EqualTo(2));
            Assert.That(report.HandlerInvocationCount, Is.EqualTo(6));
            Assert.That(report.Failures.Count, Is.EqualTo(4));
            Assert.That(report.Failures[0].Failure.Code, Is.EqualTo("inventory.test-failure"));
            Assert.That(report.Failures[0].ExceptionType, Is.Empty);
            Assert.That(report.Failures[1].Failure.Code, Is.EqualTo("events.handler.exception"));
            Assert.That(report.Failures[1].ExceptionType, Is.EqualTo(typeof(InvalidOperationException).FullName));
            Assert.That(report.Failures[1].CorrelationId.Value, Is.EqualTo("operation.test-0001"));
            Assert.That(report.Failures[1].EventId.Value, Is.EqualTo("event.0001"));
            Assert.That(report.Failures[1].EventType.Value, Is.EqualTo("tests.alpha"));
        }

        [Test]
        public void FailedHandlerIsConsumedAndNeverAutomaticallyRetried()
        {
            int calls = 0;
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Register(HandlerId("handler.failure"), Handler<AlphaEvent>(_ =>
            {
                calls++;
                return OperationResult.Fail(Failure.FromCode("tests.expected-failure"));
            }));
            DomainEventEnvelope<AlphaEvent> envelope = Alpha(1);
            dispatcher.Enqueue(envelope);

            dispatcher.Drain(10);
            Assert.That(dispatcher.Enqueue(envelope).IsDuplicate, Is.True);
            dispatcher.Drain(10);

            Assert.That(calls, Is.EqualTo(1));
        }

        [Test]
        public void EventWithoutHandlerStillConsumesItsSequence()
        {
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Enqueue(Beta(1));

            DomainEventDispatchReport report = dispatcher.Drain(1);

            Assert.That(report.ProcessedEventCount, Is.EqualTo(1));
            Assert.That(report.HandlerInvocationCount, Is.Zero);
            Assert.That(report.Failures, Is.Empty);
            Assert.That(dispatcher.LastConsumedSequence.Value, Is.EqualTo(1));
        }

        [Test]
        public void ReentrantDrainIsReportedAndFollowingHandlerStillRuns()
        {
            int followingCalls = 0;
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Register(HandlerId("handler.reentrant"), Handler<AlphaEvent>(_ =>
            {
                dispatcher.Drain(1);
                return OperationResult.Success();
            }));
            dispatcher.Register(HandlerId("handler.following"), Handler<AlphaEvent>(_ =>
            {
                followingCalls++;
                return OperationResult.Success();
            }));
            dispatcher.Enqueue(Alpha(1));

            DomainEventDispatchReport report = dispatcher.Drain(10);

            Assert.That(followingCalls, Is.EqualTo(1));
            Assert.That(report.Failures.Count, Is.EqualTo(1));
            Assert.That(report.Failures[0].Failure.Code, Is.EqualTo("events.handler.exception"));
            Assert.That(report.Failures[0].ExceptionType, Is.EqualTo(typeof(InvalidOperationException).FullName));
        }

        [Test]
        public void RegistrationRejectsDuplicateIdsAndLocksAtFirstDrain()
        {
            var dispatcher = new InMemoryDomainEventDispatcher();
            StableId<DomainEventHandlerIdScope> id = HandlerId("handler.unique");

            Assert.That(dispatcher.Register(id, Handler<AlphaEvent>(_ => OperationResult.Success())).IsSuccess, Is.True);
            OperationResult duplicate = dispatcher.Register(id, Handler<BetaEvent>(_ => OperationResult.Success()));
            dispatcher.Drain(1);
            OperationResult locked = dispatcher.Register(
                HandlerId("handler.late"),
                Handler<AlphaEvent>(_ => OperationResult.Success()));

            Assert.That(duplicate.IsFailure, Is.True);
            Assert.That(duplicate.Error.Code, Is.EqualTo("events.handler.duplicate"));
            Assert.That(locked.IsFailure, Is.True);
            Assert.That(locked.Error.Code, Is.EqualTo("events.registration.locked"));
        }

        [Test]
        public void InvalidArgumentsAndExhaustedSequenceFailExplicitly()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new InMemoryDomainEventDispatcher(-1));
            var dispatcher = new InMemoryDomainEventDispatcher();
            Assert.Throws<ArgumentOutOfRangeException>(() => dispatcher.Drain(0));
            Assert.Throws<ArgumentNullException>(() => dispatcher.Enqueue<AlphaEvent>(null));
            Assert.Throws<ArgumentException>(() => dispatcher.Register<AlphaEvent>(
                default,
                Handler<AlphaEvent>(_ => OperationResult.Success())));
            Assert.Throws<ArgumentNullException>(() => dispatcher.Register<AlphaEvent>(
                HandlerId("handler.null"),
                null));

            var exhausted = new InMemoryDomainEventDispatcher(long.MaxValue);
            DomainEventEnqueueResult result = exhausted.Enqueue(Alpha(long.MaxValue, "event.maximum"));
            Assert.That(result.IsRejected, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("events.enqueue.sequence-exhausted"));
        }

        [Test]
        public void EquivalentRunsProduceEquivalentExecutionAndFailureReports()
        {
            RunSnapshot first = RunDeterministicScenario();
            RunSnapshot second = RunDeterministicScenario();

            Assert.That(second.Execution, Is.EqualTo(first.Execution));
            Assert.That(second.FailureCodes, Is.EqualTo(first.FailureCodes));
            Assert.That(second.ExceptionTypes, Is.EqualTo(first.ExceptionTypes));
            Assert.That(second.LastSequence, Is.EqualTo(first.LastSequence));
        }

        private static RunSnapshot RunDeterministicScenario()
        {
            var execution = new List<string>();
            var dispatcher = new InMemoryDomainEventDispatcher();
            dispatcher.Register(HandlerId("handler.record"), Handler<AlphaEvent>(envelope =>
            {
                execution.Add($"{envelope.Id.Value}:{envelope.Payload.Value}");
                return envelope.Sequence.Value == 2
                    ? OperationResult.Fail(Failure.FromCode("tests.second-event"))
                    : OperationResult.Success();
            }));
            dispatcher.Enqueue(Alpha(1));
            dispatcher.Enqueue(Alpha(2, "event.0002"));
            DomainEventDispatchReport report = dispatcher.Drain(10);
            var failureCodes = new List<string>();
            var exceptionTypes = new List<string>();
            for (int index = 0; index < report.Failures.Count; index++)
            {
                failureCodes.Add(report.Failures[index].Failure.Code);
                exceptionTypes.Add(report.Failures[index].ExceptionType);
            }

            return new RunSnapshot(execution, failureCodes, exceptionTypes, report.LastConsumedSequence.Value);
        }

        private static DomainEventEnvelope<AlphaEvent> Alpha(
            long sequence,
            string id = null,
            DomainEventContext? context = null)
        {
            return new DomainEventEnvelope<AlphaEvent>(
                StableId<DomainEventIdScope>.Parse(id ?? $"event.{sequence:0000}"),
                StableId<DomainEventTypeScope>.Parse("tests.alpha"),
                DomainEventSequence.From(sequence),
                SimulationTimestamp.Origin,
                1,
                context ?? RootContext(),
                new AlphaEvent($"alpha-{sequence.ToString(CultureInfo.InvariantCulture)}"));
        }

        private static DomainEventEnvelope<BetaEvent> Beta(long sequence)
        {
            return new DomainEventEnvelope<BetaEvent>(
                StableId<DomainEventIdScope>.Parse($"event.{sequence:0000}"),
                StableId<DomainEventTypeScope>.Parse("tests.beta"),
                DomainEventSequence.From(sequence),
                SimulationTimestamp.Origin,
                1,
                RootContext(),
                new BetaEvent());
        }

        private static DomainEventContext RootContext()
        {
            return DomainEventContext.Root(
                StableId<DomainCorrelationIdScope>.Parse("operation.test-0001"));
        }

        private static StableId<DomainEventHandlerIdScope> HandlerId(string value)
        {
            return StableId<DomainEventHandlerIdScope>.Parse(value);
        }

        private static IDomainEventHandler<TEvent> Handler<TEvent>(
            Func<DomainEventEnvelope<TEvent>, OperationResult> action)
            where TEvent : IDomainEvent
        {
            return new DelegateHandler<TEvent>(action);
        }

        private sealed class DelegateHandler<TEvent> : IDomainEventHandler<TEvent>
            where TEvent : IDomainEvent
        {
            private readonly Func<DomainEventEnvelope<TEvent>, OperationResult> _action;

            public DelegateHandler(Func<DomainEventEnvelope<TEvent>, OperationResult> action)
            {
                _action = action;
            }

            public OperationResult Handle(DomainEventEnvelope<TEvent> envelope)
            {
                return _action(envelope);
            }
        }

        private sealed class AlphaEvent : IDomainEvent
        {
            public AlphaEvent(string value)
            {
                Value = value;
            }

            public string Value { get; }
        }

        private sealed class BetaEvent : IDomainEvent
        {
        }

        private sealed class RunSnapshot
        {
            public RunSnapshot(
                List<string> execution,
                List<string> failureCodes,
                List<string> exceptionTypes,
                long lastSequence)
            {
                Execution = execution;
                FailureCodes = failureCodes;
                ExceptionTypes = exceptionTypes;
                LastSequence = lastSequence;
            }

            public List<string> Execution { get; }

            public List<string> FailureCodes { get; }

            public List<string> ExceptionTypes { get; }

            public long LastSequence { get; }
        }
    }
}
