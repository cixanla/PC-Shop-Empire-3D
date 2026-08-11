using System;
using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;

namespace PCShopEmpire3D.Tests.EditMode.Core.Time
{
    public sealed class SimulationClockTests
    {
        [TestCase(-1, 0)]
        [TestCase(0, -1)]
        public void TimestampRejectsNegativeComponents(long tick, long elapsedMilliseconds)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                SimulationTimestamp.Create(tick, elapsedMilliseconds));
        }

        [Test]
        public void DurationRejectsNegativeMilliseconds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => SimulationDuration.FromMilliseconds(-1));
        }

        [Test]
        public void AdvanceUsesOnlyExplicitIntegerStep()
        {
            var clock = new SimulationClock();

            OperationResult<SimulationTimestamp> result =
                clock.Advance(SimulationDuration.FromMilliseconds(250));

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(clock.Current, Is.EqualTo(SimulationTimestamp.Create(1, 250)));
            Assert.That(result.Value, Is.EqualTo(clock.Current));
        }

        [Test]
        public void ZeroStepFailsWithoutChangingTime()
        {
            var clock = new SimulationClock();

            OperationResult<SimulationTimestamp> result = clock.Advance(SimulationDuration.Zero);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("time.non-positive-step"));
            Assert.That(clock.Current, Is.EqualTo(SimulationTimestamp.Origin));
        }

        [Test]
        public void PausedClockDoesNotAdvance()
        {
            var clock = new SimulationClock();
            Assert.That(clock.Pause(), Is.True);

            OperationResult<SimulationTimestamp> result =
                clock.Advance(SimulationDuration.FromMilliseconds(1000));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("time.clock-paused"));
            Assert.That(clock.Current, Is.EqualTo(SimulationTimestamp.Origin));
            Assert.That(clock.Pause(), Is.False);
        }

        [Test]
        public void ResumeAllowsClockToAdvanceAgain()
        {
            var clock = new SimulationClock(SimulationTimestamp.Create(4, 4000), true);

            Assert.That(clock.Resume(), Is.True);
            Assert.That(clock.Resume(), Is.False);
            Assert.That(clock.Advance(SimulationDuration.FromMilliseconds(1000)).IsSuccess, Is.True);
            Assert.That(clock.Current, Is.EqualTo(SimulationTimestamp.Create(5, 5000)));
        }

        [Test]
        public void OverflowFailsWithoutPartialMutation()
        {
            SimulationTimestamp initial = SimulationTimestamp.Create(long.MaxValue, 10);
            var clock = new SimulationClock(initial, false);

            OperationResult<SimulationTimestamp> result =
                clock.Advance(SimulationDuration.FromMilliseconds(1));

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("time.overflow"));
            Assert.That(clock.Current, Is.EqualTo(initial));
        }

        [Test]
        public void NonRegressingTimestampRequiresBothAxesToMoveForward()
        {
            SimulationTimestamp current = SimulationTimestamp.Create(10, 5000);

            Assert.That(current.IsAtOrAfter(SimulationTimestamp.Create(9, 5000)), Is.True);
            Assert.That(current.IsAtOrAfter(SimulationTimestamp.Create(10, 4000)), Is.True);
            Assert.That(current.IsAtOrAfter(SimulationTimestamp.Create(11, 4000)), Is.False);
            Assert.That(current.IsAtOrAfter(SimulationTimestamp.Create(9, 6000)), Is.False);
        }
    }
}
