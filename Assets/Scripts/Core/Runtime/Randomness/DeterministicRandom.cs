using System;

namespace PCShopEmpire3D.Core.Randomness
{
    /// <summary>
    /// Mutable, deterministic simulation random stream. It is not thread-safe or cryptographically secure.
    /// </summary>
    public sealed class DeterministicRandom
    {
        private ulong _state;
        private readonly ulong _increment;

        private DeterministicRandom(ulong state, ulong increment)
        {
            _state = state;
            _increment = increment;
        }

        public static DeterministicRandom Create(ulong initialState, ulong streamSelector)
        {
            if (streamSelector > Pcg32Algorithm.MaximumStreamSelector)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(streamSelector),
                    streamSelector,
                    $"A PCG32 stream selector must be at most {Pcg32Algorithm.MaximumStreamSelector}.");
            }

            ulong increment = unchecked((streamSelector << 1) | 1UL);
            var random = new DeterministicRandom(0UL, increment);
            random.AdvanceState();
            random._state = unchecked(random._state + initialState);
            random.AdvanceState();
            return random;
        }

        public static DeterministicRandom FromState(Pcg32State snapshot)
        {
            if (!snapshot.IsValid)
            {
                throw new ArgumentException("The PCG32 continuation state must contain an odd increment.", nameof(snapshot));
            }

            return new DeterministicRandom(snapshot.State, snapshot.Increment);
        }

        public uint NextUInt32()
        {
            ulong previousState = _state;
            AdvanceState();

            uint xorshifted = unchecked((uint)(((previousState >> 18) ^ previousState) >> 27));
            int rotation = (int)(previousState >> 59);
            return (xorshifted >> rotation) | (xorshifted << ((-rotation) & 31));
        }

        public int NextInt32(int exclusiveMax)
        {
            if (exclusiveMax <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(exclusiveMax),
                    exclusiveMax,
                    "The exclusive upper bound must be positive.");
            }

            uint bound = (uint)exclusiveMax;
            uint threshold = unchecked(0U - bound) % bound;

            while (true)
            {
                uint value = NextUInt32();
                if (value >= threshold)
                {
                    return (int)(value % bound);
                }
            }
        }

        public Pcg32State CaptureState()
        {
            return Pcg32State.Create(_state, _increment);
        }

        private void AdvanceState()
        {
            _state = unchecked((_state * Pcg32Algorithm.Multiplier) + _increment);
        }
    }
}
