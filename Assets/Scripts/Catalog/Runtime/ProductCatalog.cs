using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Catalog
{
    /// <summary>
    /// Validated, immutable and deterministically ordered product-definition registry.
    /// </summary>
    public sealed class ProductCatalog
    {
        private readonly Dictionary<StableId<ProductDefinitionIdScope>, ProductDefinition> _byId;
        private readonly IReadOnlyList<ProductDefinition> _definitions;

        private ProductCatalog(
            Dictionary<StableId<ProductDefinitionIdScope>, ProductDefinition> byId,
            IReadOnlyList<ProductDefinition> definitions)
        {
            _byId = byId;
            _definitions = definitions;
        }

        public int Count => _definitions.Count;

        public IReadOnlyList<ProductDefinition> Definitions => _definitions;

        public static OperationResult<ProductCatalog> Create(IEnumerable<ProductDefinition> definitions)
        {
            if (definitions == null)
            {
                return OperationResult<ProductCatalog>.Fail(CatalogFailures.EmptyCatalog);
            }

            var byId = new Dictionary<StableId<ProductDefinitionIdScope>, ProductDefinition>();
            var ordered = new List<ProductDefinition>();

            foreach (ProductDefinition definition in definitions)
            {
                if (definition == null)
                {
                    return OperationResult<ProductCatalog>.Fail(CatalogFailures.NullDefinition);
                }

                if (byId.ContainsKey(definition.Id))
                {
                    return OperationResult<ProductCatalog>.Fail(CatalogFailures.DuplicateDefinition);
                }

                byId.Add(definition.Id, definition);
                ordered.Add(definition);
            }

            if (ordered.Count == 0)
            {
                return OperationResult<ProductCatalog>.Fail(CatalogFailures.EmptyCatalog);
            }

            ordered.Sort((left, right) => string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return OperationResult<ProductCatalog>.Success(
                new ProductCatalog(byId, Array.AsReadOnly(ordered.ToArray())));
        }

        public bool TryGet(
            StableId<ProductDefinitionIdScope> productId,
            out ProductDefinition definition)
        {
            return _byId.TryGetValue(productId, out definition);
        }

        public OperationResult<ProductDefinition> Get(StableId<ProductDefinitionIdScope> productId)
        {
            return TryGet(productId, out ProductDefinition definition)
                ? OperationResult<ProductDefinition>.Success(definition)
                : OperationResult<ProductDefinition>.Fail(CatalogFailures.UnknownProduct);
        }
    }
}
