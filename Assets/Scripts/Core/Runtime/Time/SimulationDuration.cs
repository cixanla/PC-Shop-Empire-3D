using System;

namespace PCShopEmpire3D.Core.Time
{
    /// <summary>
    /// Non-negative elapsed game time represented with integer milliseconds.
    /// </summary>
    public readonly struct SimulationDuration : IEquatable<SimulationDuration>
    {
        private readonly long _milliseconds;

        private SimulationDuration(long milliseconds)
        {
            _milliseconds = milliseconds;
        }

        public static SimulationDuration Zero => default;

        public long Milliseconds => _milliseconds;

        public bool IsZero => _milliseconds == 0;

        public static SimulationDuration FromMilliseconds(long milliseconds)
        {
            if (milliseconds < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(milliseconds),
                    milliseconds,
                    "Simulation duration cannot be negative.");
            }

            return new SimulationDuration(milliseconds);
        }

        public bool Equals(SimulationDuration other)
        {
            return _milliseconds == other._milliseconds;
        }

        public override bool Equals(object obj)
        {
            return obj is SimulationDuration other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _milliseconds.GetHashCode();
        }

        public static bool operator ==(SimulationDuration left, SimulationDuration right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SimulationDuration left, SimulationDuration right)
        {
            return !left.Equals(right);
        }
    }
}
