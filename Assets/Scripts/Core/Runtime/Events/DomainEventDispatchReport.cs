using System;
using System.Collections.Generic;

namespace PCShopEmpire3D.Core.Events
{
    public sealed class DomainEventDispatchReport
    {
        internal DomainEventDispatchReport(
            int processedEventCount,
            int handlerInvocationCount,
            int remainingEventCount,
            DomainEventSequence lastConsumedSequence,
            List<DomainEventDispatchFailure> failures)
        {
            ProcessedEventCount = processedEventCount;
            HandlerInvocationCount = handlerInvocationCount;
            RemainingEventCount = remainingEventCount;
            LastConsumedSequence = lastConsumedSequence;
            Failures = Array.AsReadOnly(failures.ToArray());
        }

        public int ProcessedEventCount { get; }

        public int HandlerInvocationCount { get; }

        public int RemainingEventCount { get; }

        public DomainEventSequence LastConsumedSequence { get; }

        public IReadOnlyList<DomainEventDispatchFailure> Failures { get; }

        public bool ReachedBudget => RemainingEventCount > 0;
    }
}
