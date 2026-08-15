using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// The sole authority for shelf/product offers. Inventory validates shelf identity but
    /// never owns price state. Failed commands leave both records and Revision unchanged.
    /// </summary>
    public sealed class ShelfOfferAuthority
    {
        private readonly ProductCatalog _catalog;
        private readonly InventoryAuthority _inventory;
        private readonly Dictionary<StableId<ShelfOfferIdScope>, ShelfOfferRecord> _offers =
            new Dictionary<StableId<ShelfOfferIdScope>, ShelfOfferRecord>();

        private ShelfOfferAuthority(ProductCatalog catalog, InventoryAuthority inventory)
        {
            _catalog = catalog;
            _inventory = inventory;
        }

        public long Revision { get; private set; }

        public int Count => _offers.Count;

        public static OperationResult<ShelfOfferAuthority> Create(
            ProductCatalog catalog,
            InventoryAuthority inventory)
        {
            if (catalog == null)
            {
                return OperationResult<ShelfOfferAuthority>.Fail(RetailFailures.MissingCatalog);
            }

            return inventory == null
                ? OperationResult<ShelfOfferAuthority>.Fail(RetailFailures.MissingInventory)
                : OperationResult<ShelfOfferAuthority>.Success(
                    new ShelfOfferAuthority(catalog, inventory));
        }

        public OperationResult SetOffer(
            StableId<ShelfOfferIdScope> offerId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> shelfContainerId,
            string currencyCode,
            long priceMinorUnits)
        {
            Failure validation = ValidateCommand(
                offerId,
                productId,
                shelfContainerId,
                currencyCode,
                priceMinorUnits,
                out ShelfPrice price);
            if (!validation.IsNone)
            {
                return OperationResult.Fail(validation);
            }

            if (_offers.TryGetValue(offerId, out ShelfOfferRecord existing))
            {
                if (existing.ProductId != productId ||
                    existing.ShelfContainerId != shelfContainerId)
                {
                    return OperationResult.Fail(RetailFailures.OfferIdentityConflict);
                }

                if (existing.Price == price)
                {
                    return OperationResult.Success();
                }

                if (existing.OfferRevision == long.MaxValue || Revision == long.MaxValue)
                {
                    return OperationResult.Fail(RetailFailures.RevisionOverflow);
                }

                _offers[offerId] = new ShelfOfferRecord(
                    offerId,
                    productId,
                    shelfContainerId,
                    price,
                    existing.OfferRevision + 1);
                Revision++;
                return OperationResult.Success();
            }

            foreach (ShelfOfferRecord offer in _offers.Values)
            {
                if (offer.ProductId == productId &&
                    offer.ShelfContainerId == shelfContainerId)
                {
                    return OperationResult.Fail(RetailFailures.DuplicateShelfProduct);
                }
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(RetailFailures.RevisionOverflow);
            }

            _offers.Add(
                offerId,
                new ShelfOfferRecord(offerId, productId, shelfContainerId, price, 1));
            Revision++;
            return OperationResult.Success();
        }

        public bool TryGetOffer(
            StableId<ShelfOfferIdScope> offerId,
            out ShelfOfferRecord offer)
        {
            return _offers.TryGetValue(offerId, out offer);
        }

        public bool TryGetOfferForShelfProduct(
            StableId<ContainerIdScope> shelfContainerId,
            StableId<ProductDefinitionIdScope> productId,
            out ShelfOfferRecord offer)
        {
            foreach (ShelfOfferRecord candidate in _offers.Values)
            {
                if (candidate.ShelfContainerId == shelfContainerId &&
                    candidate.ProductId == productId)
                {
                    offer = candidate;
                    return true;
                }
            }

            offer = null;
            return false;
        }

        public IReadOnlyList<ShelfOfferRecord> GetOffers()
        {
            var ordered = new List<ShelfOfferRecord>(_offers.Values);
            ordered.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(ordered.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            var positions = new HashSet<ShelfProductKey>();
            foreach (KeyValuePair<StableId<ShelfOfferIdScope>, ShelfOfferRecord> entry in _offers)
            {
                ShelfOfferRecord offer = entry.Value;
                if (offer == null ||
                    entry.Key.IsEmpty ||
                    entry.Key != offer.Id ||
                    offer.ProductId.IsEmpty ||
                    offer.ShelfContainerId.IsEmpty ||
                    offer.OfferRevision <= 0 ||
                    ShelfPrice.Create(offer.Price.Currency.Value, offer.Price.MinorUnits).IsFailure ||
                    !_catalog.TryGet(offer.ProductId, out _) ||
                    !_inventory.TryGetContainer(
                        offer.ShelfContainerId,
                        out InventoryContainerDefinition container) ||
                    container.Kind != InventoryContainerKind.Shelf ||
                    !positions.Add(new ShelfProductKey(
                        offer.ShelfContainerId,
                        offer.ProductId)))
                {
                    return OperationResult.Fail(RetailFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private Failure ValidateCommand(
            StableId<ShelfOfferIdScope> offerId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> shelfContainerId,
            string currencyCode,
            long priceMinorUnits,
            out ShelfPrice price)
        {
            price = default;
            if (offerId.IsEmpty)
            {
                return RetailFailures.InvalidOfferId;
            }

            if (productId.IsEmpty)
            {
                return RetailFailures.InvalidProductId;
            }

            if (shelfContainerId.IsEmpty)
            {
                return RetailFailures.InvalidShelfContainerId;
            }

            OperationResult<ShelfPrice> priceResult = ShelfPrice.Create(
                currencyCode,
                priceMinorUnits);
            if (priceResult.IsFailure)
            {
                return priceResult.Error;
            }

            if (!_catalog.TryGet(productId, out _))
            {
                return RetailFailures.UnknownProduct;
            }

            if (!_inventory.TryGetContainer(
                    shelfContainerId,
                    out InventoryContainerDefinition container))
            {
                return RetailFailures.UnknownShelfContainer;
            }

            if (container.Kind != InventoryContainerKind.Shelf)
            {
                return RetailFailures.ContainerIsNotShelf;
            }

            price = priceResult.Value;
            return Failure.None;
        }

        private readonly struct ShelfProductKey : IEquatable<ShelfProductKey>
        {
            public ShelfProductKey(
                StableId<ContainerIdScope> shelfContainerId,
                StableId<ProductDefinitionIdScope> productId)
            {
                ShelfContainerId = shelfContainerId;
                ProductId = productId;
            }

            private StableId<ContainerIdScope> ShelfContainerId { get; }

            private StableId<ProductDefinitionIdScope> ProductId { get; }

            public bool Equals(ShelfProductKey other)
            {
                return ShelfContainerId == other.ShelfContainerId &&
                       ProductId == other.ProductId;
            }

            public override bool Equals(object obj)
            {
                return obj is ShelfProductKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (ShelfContainerId.GetHashCode() * 397) ^ ProductId.GetHashCode();
                }
            }
        }
    }

    public static class RetailFailures
    {
        public static readonly Failure MissingCatalog = Failure.FromCode("retail.catalog.missing");
        public static readonly Failure MissingInventory = Failure.FromCode("retail.inventory.missing");
        public static readonly Failure InvalidOfferId = Failure.FromCode("retail.offer-id.invalid");
        public static readonly Failure InvalidProductId = Failure.FromCode("retail.product-id.invalid");
        public static readonly Failure InvalidShelfContainerId = Failure.FromCode("retail.shelf-id.invalid");
        public static readonly Failure InvalidCurrencyCode = Failure.FromCode("retail.currency.invalid");
        public static readonly Failure InvalidPrice = Failure.FromCode("retail.price.invalid");
        public static readonly Failure PriceLimitExceeded = Failure.FromCode("retail.price.limit");
        public static readonly Failure UnknownProduct = Failure.FromCode("retail.product.unknown");
        public static readonly Failure UnknownShelfContainer = Failure.FromCode("retail.shelf.unknown");
        public static readonly Failure ContainerIsNotShelf = Failure.FromCode("retail.shelf.kind");
        public static readonly Failure DuplicateShelfProduct = Failure.FromCode("retail.shelf-product.duplicate");
        public static readonly Failure OfferIdentityConflict = Failure.FromCode("retail.offer.identity-conflict");
        public static readonly Failure RevisionOverflow = Failure.FromCode("retail.revision.overflow");
        public static readonly Failure InvariantViolation = Failure.FromCode("retail.invariant");
    }
}
