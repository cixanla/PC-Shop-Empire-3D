using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Catalog
{
    /// <summary>
    /// Persisted component category used by physical PC assembly rules. The first bounded
    /// assembly slice intentionally supports motherboards only.
    /// </summary>
    public enum PcComponentKind
    {
        Motherboard = 1
    }

    /// <summary>
    /// Persisted motherboard/chassis compatibility key.
    /// </summary>
    public enum MotherboardFormFactor
    {
        MiniItx = 1,
        MicroAtx = 2,
        Atx = 3
    }

    /// <summary>
    /// Immutable assembly-facing extension of one authoritative product definition.
    /// </summary>
    public sealed class PcComponentSpecification
    {
        private PcComponentSpecification(
            StableId<ProductDefinitionIdScope> productId,
            PcComponentKind kind,
            MotherboardFormFactor motherboardFormFactor)
        {
            ProductId = productId;
            Kind = kind;
            MotherboardFormFactor = motherboardFormFactor;
        }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public PcComponentKind Kind { get; }

        public MotherboardFormFactor MotherboardFormFactor { get; }

        public static OperationResult<PcComponentSpecification> Create(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            PcComponentKind kind,
            MotherboardFormFactor motherboardFormFactor)
        {
            if (productCatalog == null)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.MissingProductCatalog);
            }

            if (productId.IsEmpty)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidComponentProductId);
            }

            if (!productCatalog.TryGet(productId, out ProductDefinition definition))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.UnknownComponentProduct);
            }

            if (definition.TrackingPolicy != ProductTrackingPolicy.SerializedInstance)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.ComponentTrackingMismatch);
            }

            if (!IsValidComponentKind(kind))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidComponentKind);
            }

            if (!IsValidMotherboardFormFactor(motherboardFormFactor))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidMotherboardFormFactor);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(productId, kind, motherboardFormFactor));
        }

        public static bool IsValidComponentKind(PcComponentKind kind)
        {
            return kind == PcComponentKind.Motherboard;
        }

        public static bool IsValidMotherboardFormFactor(MotherboardFormFactor formFactor)
        {
            return formFactor == MotherboardFormFactor.MiniItx ||
                   formFactor == MotherboardFormFactor.MicroAtx ||
                   formFactor == MotherboardFormFactor.Atx;
        }
    }

    /// <summary>
    /// Validated immutable registry that adds assembly metadata without duplicating product
    /// identity, display or tracking authority.
    /// </summary>
    public sealed class PcComponentCatalog
    {
        private readonly Dictionary<StableId<ProductDefinitionIdScope>, PcComponentSpecification> _byProductId;
        private readonly IReadOnlyList<PcComponentSpecification> _specifications;

        private PcComponentCatalog(
            Dictionary<StableId<ProductDefinitionIdScope>, PcComponentSpecification> byProductId,
            IReadOnlyList<PcComponentSpecification> specifications)
        {
            _byProductId = byProductId;
            _specifications = specifications;
        }

        public int Count => _specifications.Count;

        public IReadOnlyList<PcComponentSpecification> Specifications => _specifications;

        public static OperationResult<PcComponentCatalog> Create(
            ProductCatalog productCatalog,
            IEnumerable<PcComponentSpecification> specifications)
        {
            if (productCatalog == null)
            {
                return OperationResult<PcComponentCatalog>.Fail(
                    CatalogFailures.MissingProductCatalog);
            }

            if (specifications == null)
            {
                return OperationResult<PcComponentCatalog>.Fail(
                    CatalogFailures.EmptyComponentCatalog);
            }

            var byProductId =
                new Dictionary<StableId<ProductDefinitionIdScope>, PcComponentSpecification>();
            var ordered = new List<PcComponentSpecification>();
            foreach (PcComponentSpecification specification in specifications)
            {
                if (specification == null)
                {
                    return OperationResult<PcComponentCatalog>.Fail(
                        CatalogFailures.NullComponentSpecification);
                }

                if (!productCatalog.TryGet(specification.ProductId, out ProductDefinition definition) ||
                    definition.TrackingPolicy != ProductTrackingPolicy.SerializedInstance)
                {
                    return OperationResult<PcComponentCatalog>.Fail(
                        CatalogFailures.UnknownComponentProduct);
                }

                if (byProductId.ContainsKey(specification.ProductId))
                {
                    return OperationResult<PcComponentCatalog>.Fail(
                        CatalogFailures.DuplicateComponentSpecification);
                }

                byProductId.Add(specification.ProductId, specification);
                ordered.Add(specification);
            }

            if (ordered.Count == 0)
            {
                return OperationResult<PcComponentCatalog>.Fail(
                    CatalogFailures.EmptyComponentCatalog);
            }

            ordered.Sort((left, right) => string.Compare(
                left.ProductId.Value,
                right.ProductId.Value,
                StringComparison.Ordinal));
            return OperationResult<PcComponentCatalog>.Success(
                new PcComponentCatalog(byProductId, Array.AsReadOnly(ordered.ToArray())));
        }

        public bool TryGet(
            StableId<ProductDefinitionIdScope> productId,
            out PcComponentSpecification specification)
        {
            return _byProductId.TryGetValue(productId, out specification);
        }

        public OperationResult<PcComponentSpecification> Get(
            StableId<ProductDefinitionIdScope> productId)
        {
            return TryGet(productId, out PcComponentSpecification specification)
                ? OperationResult<PcComponentSpecification>.Success(specification)
                : OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.UnknownComponentSpecification);
        }
    }
}
