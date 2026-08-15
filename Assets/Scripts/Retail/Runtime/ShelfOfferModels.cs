using System;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// A validated three-letter currency identity. Currency metadata and non-two-decimal
    /// currencies remain outside this first shelf-offer slice.
    /// </summary>
    public readonly struct CurrencyCode : IEquatable<CurrencyCode>
    {
        private readonly string _value;

        private CurrencyCode(string value)
        {
            _value = value;
        }

        public string Value => _value ?? string.Empty;

        public static OperationResult<CurrencyCode> Create(string value)
        {
            if (!IsValid(value))
            {
                return OperationResult<CurrencyCode>.Fail(RetailFailures.InvalidCurrencyCode);
            }

            return OperationResult<CurrencyCode>.Success(new CurrencyCode(value));
        }

        public bool Equals(CurrencyCode other)
        {
            return string.Equals(_value, other._value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is CurrencyCode other && Equals(other);
        }

        public override int GetHashCode()
        {
            return StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value;
        }

        public static bool operator ==(CurrencyCode left, CurrencyCode right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CurrencyCode left, CurrencyCode right)
        {
            return !left.Equals(right);
        }

        internal static bool IsValid(string value)
        {
            if (value == null || value.Length != 3)
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] < 'A' || value[index] > 'Z')
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Positive shelf price represented exclusively in integer minor units. This bounded
    /// first contract uses two decimal places and never stores a float or double.
    /// </summary>
    public readonly struct ShelfPrice : IEquatable<ShelfPrice>
    {
        public const long MaximumMinorUnits = 999_999_999L;

        private ShelfPrice(CurrencyCode currency, long minorUnits)
        {
            Currency = currency;
            MinorUnits = minorUnits;
        }

        public CurrencyCode Currency { get; }

        public long MinorUnits { get; }

        public static OperationResult<ShelfPrice> Create(string currencyCode, long minorUnits)
        {
            OperationResult<CurrencyCode> currency = CurrencyCode.Create(currencyCode);
            if (currency.IsFailure)
            {
                return OperationResult<ShelfPrice>.Fail(currency.Error);
            }

            if (minorUnits <= 0)
            {
                return OperationResult<ShelfPrice>.Fail(RetailFailures.InvalidPrice);
            }

            if (minorUnits > MaximumMinorUnits)
            {
                return OperationResult<ShelfPrice>.Fail(RetailFailures.PriceLimitExceeded);
            }

            return OperationResult<ShelfPrice>.Success(
                new ShelfPrice(currency.Value, minorUnits));
        }

        public bool Equals(ShelfPrice other)
        {
            return Currency == other.Currency && MinorUnits == other.MinorUnits;
        }

        public override bool Equals(object obj)
        {
            return obj is ShelfPrice other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Currency.GetHashCode() * 397) ^ MinorUnits.GetHashCode();
            }
        }

        public static bool operator ==(ShelfPrice left, ShelfPrice right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ShelfPrice left, ShelfPrice right)
        {
            return !left.Equals(right);
        }
    }

    public sealed class ShelfOfferRecord
    {
        internal ShelfOfferRecord(
            StableId<ShelfOfferIdScope> id,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> shelfContainerId,
            ShelfPrice price,
            long offerRevision)
        {
            Id = id;
            ProductId = productId;
            ShelfContainerId = shelfContainerId;
            Price = price;
            OfferRevision = offerRevision;
        }

        public StableId<ShelfOfferIdScope> Id { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> ShelfContainerId { get; }

        public ShelfPrice Price { get; }

        public long OfferRevision { get; }
    }
}
