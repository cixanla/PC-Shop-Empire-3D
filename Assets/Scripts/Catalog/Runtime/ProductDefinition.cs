using System;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Catalog
{
    public sealed class ProductDefinitionIdScope : IStableIdScope
    {
    }

    public sealed class ProductCategoryIdScope : IStableIdScope
    {
    }

    /// <summary>
    /// Defines whether physical stock is tracked as unique units or fungible batch quantities.
    /// Values are explicit because this contract will be persisted later.
    /// </summary>
    public enum ProductTrackingPolicy
    {
        SerializedInstance = 1,
        BatchQuantity = 2
    }

    /// <summary>
    /// Immutable gameplay definition shared by catalog and inventory. Commercial pricing is intentionally separate.
    /// </summary>
    public sealed class ProductDefinition
    {
        public const int MaximumDisplayNameLength = 128;
        public const int MaximumWarrantyDays = 3650;

        private ProductDefinition(
            StableId<ProductDefinitionIdScope> id,
            StableId<ProductCategoryIdScope> categoryId,
            string displayName,
            ProductTrackingPolicy trackingPolicy,
            int warrantyDays)
        {
            Id = id;
            CategoryId = categoryId;
            DisplayName = displayName;
            TrackingPolicy = trackingPolicy;
            WarrantyDays = warrantyDays;
        }

        public StableId<ProductDefinitionIdScope> Id { get; }

        public StableId<ProductCategoryIdScope> CategoryId { get; }

        public string DisplayName { get; }

        public ProductTrackingPolicy TrackingPolicy { get; }

        public int WarrantyDays { get; }

        public static OperationResult<ProductDefinition> Create(
            StableId<ProductDefinitionIdScope> id,
            StableId<ProductCategoryIdScope> categoryId,
            string displayName,
            ProductTrackingPolicy trackingPolicy,
            int warrantyDays)
        {
            if (id.IsEmpty)
            {
                return OperationResult<ProductDefinition>.Fail(CatalogFailures.InvalidProductId);
            }

            if (categoryId.IsEmpty)
            {
                return OperationResult<ProductDefinition>.Fail(CatalogFailures.InvalidCategoryId);
            }

            if (!IsValidDisplayName(displayName))
            {
                return OperationResult<ProductDefinition>.Fail(CatalogFailures.InvalidDisplayName);
            }

            if (!IsValidTrackingPolicy(trackingPolicy))
            {
                return OperationResult<ProductDefinition>.Fail(CatalogFailures.InvalidTrackingPolicy);
            }

            if (warrantyDays < 0 || warrantyDays > MaximumWarrantyDays)
            {
                return OperationResult<ProductDefinition>.Fail(CatalogFailures.InvalidWarrantyDays);
            }

            return OperationResult<ProductDefinition>.Success(
                new ProductDefinition(id, categoryId, displayName, trackingPolicy, warrantyDays));
        }

        public static bool IsValidTrackingPolicy(ProductTrackingPolicy trackingPolicy)
        {
            return trackingPolicy == ProductTrackingPolicy.SerializedInstance ||
                   trackingPolicy == ProductTrackingPolicy.BatchQuantity;
        }

        private static bool IsValidDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName) ||
                displayName.Length > MaximumDisplayNameLength ||
                !string.Equals(displayName, displayName.Trim(), StringComparison.Ordinal))
            {
                return false;
            }

            for (int index = 0; index < displayName.Length; index++)
            {
                if (char.IsControl(displayName[index]))
                {
                    return false;
                }
            }

            return true;
        }
    }

    public static class CatalogFailures
    {
        public static readonly Failure InvalidProductId = Failure.FromCode("catalog.product-id.invalid");
        public static readonly Failure InvalidCategoryId = Failure.FromCode("catalog.category-id.invalid");
        public static readonly Failure InvalidDisplayName = Failure.FromCode("catalog.display-name.invalid");
        public static readonly Failure InvalidTrackingPolicy = Failure.FromCode("catalog.tracking-policy.invalid");
        public static readonly Failure InvalidWarrantyDays = Failure.FromCode("catalog.warranty-days.invalid");
        public static readonly Failure EmptyCatalog = Failure.FromCode("catalog.empty");
        public static readonly Failure NullDefinition = Failure.FromCode("catalog.definition.null");
        public static readonly Failure DuplicateDefinition = Failure.FromCode("catalog.definition.duplicate");
        public static readonly Failure UnknownProduct = Failure.FromCode("catalog.product.unknown");
        public static readonly Failure MissingProductCatalog =
            Failure.FromCode("catalog.component.product-catalog.missing");
        public static readonly Failure InvalidComponentProductId =
            Failure.FromCode("catalog.component.product-id.invalid");
        public static readonly Failure UnknownComponentProduct =
            Failure.FromCode("catalog.component.product.unknown");
        public static readonly Failure ComponentProductCatalogMismatch =
            Failure.FromCode("catalog.component.product-catalog.mismatch");
        public static readonly Failure ComponentTrackingMismatch =
            Failure.FromCode("catalog.component.tracking-mismatch");
        public static readonly Failure InvalidComponentKind =
            Failure.FromCode("catalog.component.kind.invalid");
        public static readonly Failure InvalidMotherboardFormFactor =
            Failure.FromCode("catalog.component.motherboard-form-factor.invalid");
        public static readonly Failure InvalidCpuSocketFamily =
            Failure.FromCode("catalog.component.cpu-socket-family.invalid");
        public static readonly Failure ComponentMetadataMismatch =
            Failure.FromCode("catalog.component.metadata-mismatch");
        public static readonly Failure EmptyComponentCatalog =
            Failure.FromCode("catalog.component-catalog.empty");
        public static readonly Failure NullComponentSpecification =
            Failure.FromCode("catalog.component-specification.null");
        public static readonly Failure DuplicateComponentSpecification =
            Failure.FromCode("catalog.component-specification.duplicate");
        public static readonly Failure UnknownComponentSpecification =
            Failure.FromCode("catalog.component-specification.unknown");
    }
}
