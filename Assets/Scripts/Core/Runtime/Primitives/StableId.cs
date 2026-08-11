using System;

namespace PCShopEmpire3D.Core.Primitives
{
    /// <summary>
    /// A type-scoped, culture-independent identifier suitable for saves and cross-system references.
    /// </summary>
    /// <typeparam name="TScope">The domain identity scope.</typeparam>
    public readonly struct StableId<TScope> : IEquatable<StableId<TScope>>
        where TScope : IStableIdScope
    {
        public const int MaximumLength = 128;

        private readonly string _value;

        private StableId(string value)
        {
            _value = value;
        }

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        public static StableId<TScope> Parse(string value)
        {
            if (!TryParse(value, out StableId<TScope> id))
            {
                throw new ArgumentException(
                    "A stable ID must be 1-128 characters, start and end with a lowercase ASCII letter or digit, " +
                    "and contain only lowercase ASCII letters, digits, '.', '_' or '-'.",
                    nameof(value));
            }

            return id;
        }

        public static bool TryParse(string value, out StableId<TScope> id)
        {
            if (!IsValid(value))
            {
                id = default;
                return false;
            }

            id = new StableId<TScope>(value);
            return true;
        }

        public bool Equals(StableId<TScope> other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is StableId<TScope> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(StableId<TScope> left, StableId<TScope> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StableId<TScope> left, StableId<TScope> right)
        {
            return !left.Equals(right);
        }

        private static bool IsValid(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > MaximumLength)
            {
                return false;
            }

            if (!IsLowercaseLetterOrDigit(value[0]) || !IsLowercaseLetterOrDigit(value[value.Length - 1]))
            {
                return false;
            }

            for (int index = 1; index < value.Length - 1; index++)
            {
                char character = value[index];
                if (!IsLowercaseLetterOrDigit(character) &&
                    character != '.' &&
                    character != '_' &&
                    character != '-')
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsLowercaseLetterOrDigit(char character)
        {
            return (character >= 'a' && character <= 'z') ||
                   (character >= '0' && character <= '9');
        }
    }
}
