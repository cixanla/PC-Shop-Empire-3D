using System;
using System.Globalization;

namespace PCShopEmpire3D.Core.Randomness
{
    /// <summary>
    /// Save-safe root seed for all deterministic random streams of one company.
    /// </summary>
    public readonly struct RandomRootSeed : IEquatable<RandomRootSeed>
    {
        public const int CanonicalLength = 16;

        public RandomRootSeed(ulong value)
        {
            Value = value;
        }

        public ulong Value { get; }

        public static RandomRootSeed ParseCanonical(string value)
        {
            if (!TryParseCanonical(value, out RandomRootSeed seed))
            {
                throw new FormatException(
                    "A random root seed must be exactly 16 lowercase hexadecimal characters without a prefix.");
            }

            return seed;
        }

        public static bool TryParseCanonical(string value, out RandomRootSeed seed)
        {
            if (value == null || value.Length != CanonicalLength)
            {
                seed = default;
                return false;
            }

            ulong parsed = 0UL;
            for (int index = 0; index < value.Length; index++)
            {
                int nibble = ParseLowercaseHexNibble(value[index]);
                if (nibble < 0)
                {
                    seed = default;
                    return false;
                }

                parsed = unchecked((parsed << 4) | (uint)nibble);
            }

            seed = new RandomRootSeed(parsed);
            return true;
        }

        public string ToCanonicalString()
        {
            return Value.ToString("x16", CultureInfo.InvariantCulture);
        }

        public bool Equals(RandomRootSeed other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is RandomRootSeed other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return ToCanonicalString();
        }

        public static bool operator ==(RandomRootSeed left, RandomRootSeed right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(RandomRootSeed left, RandomRootSeed right)
        {
            return !left.Equals(right);
        }

        private static int ParseLowercaseHexNibble(char character)
        {
            if (character >= '0' && character <= '9')
            {
                return character - '0';
            }

            if (character >= 'a' && character <= 'f')
            {
                return character - 'a' + 10;
            }

            return -1;
        }
    }
}
