using System;

namespace PCShopEmpire3D.Core.Randomness
{
    /// <summary>
    /// Deterministic PCG32 initialization derived from a root seed and stable context.
    /// </summary>
    public readonly struct RandomStreamInitialization : IEquatable<RandomStreamInitialization>
    {
        internal RandomStreamInitialization(ulong initialState, ulong streamSelector)
        {
            if (streamSelector > Pcg32Algorithm.MaximumStreamSelector)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(streamSelector),
                    streamSelector,
                    $"A PCG32 stream selector must be at most {Pcg32Algorithm.MaximumStreamSelector}.");
            }

            InitialState = initialState;
            StreamSelector = streamSelector;
        }

        public ulong InitialState { get; }

        public ulong StreamSelector { get; }

        public DeterministicRandom CreateRandom()
        {
            return DeterministicRandom.Create(InitialState, StreamSelector);
        }

        public bool Equals(RandomStreamInitialization other)
        {
            return InitialState == other.InitialState && StreamSelector == other.StreamSelector;
        }

        public override bool Equals(object obj)
        {
            return obj is RandomStreamInitialization other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (InitialState.GetHashCode() * 397) ^ StreamSelector.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{Pcg32Algorithm.Id}:initial-state={InitialState:x16},stream-selector={StreamSelector:x16}";
        }

        public static bool operator ==(RandomStreamInitialization left, RandomStreamInitialization right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RandomStreamInitialization left, RandomStreamInitialization right)
        {
            return !left.Equals(right);
        }
    }
}
