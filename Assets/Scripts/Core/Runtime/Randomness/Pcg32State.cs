using System;

namespace PCShopEmpire3D.Core.Randomness
{
    /// <summary>
    /// Serializable continuation state for a versioned PCG32 stream.
    /// </summary>
    public readonly struct Pcg32State : IEquatable<Pcg32State>
    {
        private Pcg32State(ulong state, ulong increment)
        {
            State = state;
            Increment = increment;
        }

        public ulong State { get; }

        public ulong Increment { get; }

        public bool IsValid => (Increment & 1UL) == 1UL;

        public static Pcg32State Create(ulong state, ulong increment)
        {
            if ((increment & 1UL) == 0UL)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(increment),
                    increment,
                    "A PCG32 set-sequence increment must be odd.");
            }

            return new Pcg32State(state, increment);
        }

        public bool Equals(Pcg32State other)
        {
            return State == other.State && Increment == other.Increment;
        }

        public override bool Equals(object obj)
        {
            return obj is Pcg32State other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (State.GetHashCode() * 397) ^ Increment.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{Pcg32Algorithm.Id}:state={State:x16},increment={Increment:x16}";
        }

        public static bool operator ==(Pcg32State left, Pcg32State right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Pcg32State left, Pcg32State right)
        {
            return !left.Equals(right);
        }
    }
}
