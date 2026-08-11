using System;

namespace PCShopEmpire3D.Core.Events
{
    /// <summary>
    /// One-based journal ordering value. Zero/default is intentionally unassigned.
    /// </summary>
    public readonly struct DomainEventSequence : IEquatable<DomainEventSequence>, IComparable<DomainEventSequence>
    {
        private readonly long _value;

        private DomainEventSequence(long value)
        {
            _value = value;
        }

        public long Value => _value;

        public bool IsAssigned => _value > 0;

        public static DomainEventSequence From(long value)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Domain event sequence must be greater than zero.");
            }

            return new DomainEventSequence(value);
        }

        public DomainEventSequence Next()
        {
            if (!IsAssigned)
            {
                throw new InvalidOperationException("An unassigned domain event sequence has no successor.");
            }

            return From(checked(_value + 1));
        }

        public int CompareTo(DomainEventSequence other)
        {
            return _value.CompareTo(other._value);
        }

        public bool Equals(DomainEventSequence other)
        {
            return _value == other._value;
        }

        public override bool Equals(object obj)
        {
            return obj is DomainEventSequence other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public static bool operator ==(DomainEventSequence left, DomainEventSequence right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DomainEventSequence left, DomainEventSequence right)
        {
            return !left.Equals(right);
        }
    }
}
