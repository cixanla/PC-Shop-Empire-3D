using System;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// Lowercase SHA-256 fingerprint of a domain event's versioned canonical payload bytes.
    /// </summary>
    public readonly struct DomainEventPayloadFingerprint : IEquatable<DomainEventPayloadFingerprint>
    {
        public const int CanonicalLength = 64;
        private static readonly char[] LowercaseHex = "0123456789abcdef".ToCharArray();

        private readonly string _value;

        private DomainEventPayloadFingerprint(string value)
        {
            _value = value;
        }

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        public static DomainEventPayloadFingerprint Parse(string value)
        {
            if (!TryParse(value, out DomainEventPayloadFingerprint fingerprint))
            {
                throw new FormatException(
                    "A domain event payload fingerprint must be exactly 64 lowercase hexadecimal characters.");
            }

            return fingerprint;
        }

        public static bool TryParse(string value, out DomainEventPayloadFingerprint fingerprint)
        {
            if (value == null || value.Length != CanonicalLength)
            {
                fingerprint = default;
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                char character = value[index];
                bool isDigit = character >= '0' && character <= '9';
                bool isLowercaseHex = character >= 'a' && character <= 'f';
                if (!isDigit && !isLowercaseHex)
                {
                    fingerprint = default;
                    return false;
                }
            }

            fingerprint = new DomainEventPayloadFingerprint(value);
            return true;
        }

        public bool Equals(DomainEventPayloadFingerprint other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is DomainEventPayloadFingerprint other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(
            DomainEventPayloadFingerprint left,
            DomainEventPayloadFingerprint right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            DomainEventPayloadFingerprint left,
            DomainEventPayloadFingerprint right)
        {
            return !left.Equals(right);
        }

        internal static DomainEventPayloadFingerprint FromSha256Digest(byte[] digest)
        {
            if (digest == null || digest.Length != 32)
            {
                throw new ArgumentException("A SHA-256 digest must contain exactly 32 bytes.", nameof(digest));
            }

            var characters = new char[CanonicalLength];
            for (int index = 0; index < digest.Length; index++)
            {
                byte current = digest[index];
                characters[index * 2] = LowercaseHex[current >> 4];
                characters[(index * 2) + 1] = LowercaseHex[current & 0x0F];
            }

            return new DomainEventPayloadFingerprint(new string(characters));
        }

        internal static DomainEventPayloadFingerprint Compute(IDomainEvent payload)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var writer = new DomainEventPayloadWriter();
            payload.WriteCanonicalPayload(writer);
            return writer.ComputeFingerprint();
        }
    }
}
