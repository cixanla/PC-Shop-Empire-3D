using System;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Core.Time
{
    /// <summary>
    /// Explicitly advanced fixed-step clock. It never reads the operating-system clock or Unity frame time.
    /// </summary>
    public sealed class SimulationClock : ISimulationClock
    {
        private static readonly Failure ClockPausedFailure = Failure.FromCode("time.clock-paused");
        private static readonly Failure NonPositiveStepFailure = Failure.FromCode("time.non-positive-step");
        private static readonly Failure OverflowFailure = Failure.FromCode("time.overflow");

        public SimulationClock()
            : this(SimulationTimestamp.Origin, false)
        {
        }

        public SimulationClock(SimulationTimestamp initial, bool isPaused)
        {
            Current = initial;
            IsPaused = isPaused;
        }

        public SimulationTimestamp Current { get; private set; }

        public bool IsPaused { get; private set; }

        public bool Pause()
        {
            if (IsPaused)
            {
                return false;
            }

            IsPaused = true;
            return true;
        }

        public bool Resume()
        {
            if (!IsPaused)
            {
                return false;
            }

            IsPaused = false;
            return true;
        }

        public OperationResult<SimulationTimestamp> Advance(SimulationDuration step)
        {
            if (IsPaused)
            {
                return OperationResult<SimulationTimestamp>.Fail(ClockPausedFailure);
            }

            if (step.IsZero)
            {
                return OperationResult<SimulationTimestamp>.Fail(NonPositiveStepFailure);
            }

            long nextTick;
            long nextElapsedMilliseconds;

            try
            {
                nextTick = checked(Current.Tick + 1);
                nextElapsedMilliseconds = checked(Current.ElapsedMilliseconds + step.Milliseconds);
            }
            catch (OverflowException)
            {
                return OperationResult<SimulationTimestamp>.Fail(OverflowFailure);
            }

            Current = SimulationTimestamp.Create(nextTick, nextElapsedMilliseconds);
            return OperationResult<SimulationTimestamp>.Success(Current);
        }
    }
}
