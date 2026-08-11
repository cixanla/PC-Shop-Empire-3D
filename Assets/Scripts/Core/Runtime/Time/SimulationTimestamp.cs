using System;

namespace PCShopEmpire3D.Core.Time
{
    /// <summary>
    /// Monotonic simulation position. It deliberately contains no wall-clock or time-zone value.
    /// </summary>
    public readonly struct SimulationTimestamp : IEquatable<SimulationTimestamp>
    {
        private SimulationTimestamp(long tick, long elapsedMilliseconds)
        {
            Tick = tick;
            ElapsedMilliseconds = elapsedMilliseconds;
        }

        public static SimulationTimestamp Origin => default;

        public long Tick { get; }

        public long ElapsedMilliseconds { get; }

        public static SimulationTimestamp Create(long tick, long elapsedMilliseconds)
        {
            if (tick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(tick), tick, "Simulation tick cannot be negative.");
            }

            if (elapsedMilliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(elapsedMilliseconds),
                    elapsedMilliseconds,
                    "Elapsed simulation time cannot be negative.");
            }

            return new SimulationTimestamp(tick, elapsedMilliseconds);
        }

        public bool IsAtOrAfter(SimulationTimestamp other)
        {
            return Tick >= other.Tick && ElapsedMilliseconds >= other.ElapsedMilliseconds;
        }

        public bool Equals(SimulationTimestamp other)
        {
            return Tick == other.Tick && ElapsedMilliseconds == other.ElapsedMilliseconds;
        }

        public override bool Equals(object obj)
        {
            return obj is SimulationTimestamp other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Tick.GetHashCode() * 397) ^ ElapsedMilliseconds.GetHashCode();
            }
        }

        public static bool operator ==(SimulationTimestamp left, SimulationTimestamp right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SimulationTimestamp left, SimulationTimestamp right)
        {
            return !left.Equals(right);
        }
    }
}
