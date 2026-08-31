using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Catalog
{
    /// <summary>
    /// Stable identity scope for one immutable performance-metadata catalog revision.
    /// </summary>
    public sealed class PcPerformanceCatalogIdScope : IStableIdScope
    {
    }

    /// <summary>
    /// Immutable deterministic performance and thermal metadata for one exact PC component
    /// product. This is catalog data only; it does not represent a benchmark result.
    /// </summary>
    public sealed class PcPerformanceSpecification
    {
        public const int MaximumSupportedScore = 1_000_000;
        public const int MaximumSupportedWatts = 100_000;

        private PcPerformanceSpecification(
            PcComponentCatalog ownerComponentCatalog,
            StableId<ProductDefinitionIdScope> productId,
            PcComponentKind componentKind,
            int performanceScore,
            int thermalLoadWatts,
            int coolingCapacityWatts)
        {
            OwnerComponentCatalog = ownerComponentCatalog;
            ProductId = productId;
            ComponentKind = componentKind;
            PerformanceScore = performanceScore;
            ThermalLoadWatts = thermalLoadWatts;
            CoolingCapacityWatts = coolingCapacityWatts;
        }

        internal PcComponentCatalog OwnerComponentCatalog { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public PcComponentKind ComponentKind { get; }

        public int PerformanceScore { get; }

        public int ThermalLoadWatts { get; }

        public int CoolingCapacityWatts { get; }

        public static OperationResult<PcPerformanceSpecification> Create(
            PcComponentCatalog componentCatalog,
            StableId<ProductDefinitionIdScope> productId,
            int performanceScore,
            int thermalLoadWatts,
            int coolingCapacityWatts)
        {
            OperationResult<PcComponentSpecification> component = ResolveComponent(
                componentCatalog,
                productId);
            if (component.IsFailure)
            {
                return OperationResult<PcPerformanceSpecification>.Fail(component.Error);
            }

            if (!IsSupportedKind(component.Value.Kind))
            {
                return OperationResult<PcPerformanceSpecification>.Fail(
                    PcPerformanceCatalogFailures.UnsupportedComponentKind);
            }

            if (!IsValidScore(performanceScore))
            {
                return OperationResult<PcPerformanceSpecification>.Fail(
                    PcPerformanceCatalogFailures.InvalidPerformanceScore);
            }

            if (!IsWithinBounds(thermalLoadWatts) ||
                !IsWithinBounds(coolingCapacityWatts))
            {
                return OperationResult<PcPerformanceSpecification>.Fail(
                    PcPerformanceCatalogFailures.InvalidThermalWatts);
            }

            if (!HasValidKindShape(
                    component.Value.Kind,
                    thermalLoadWatts,
                    coolingCapacityWatts))
            {
                return OperationResult<PcPerformanceSpecification>.Fail(
                    PcPerformanceCatalogFailures.InvalidThermalShape);
            }

            return OperationResult<PcPerformanceSpecification>.Success(
                new PcPerformanceSpecification(
                    componentCatalog,
                    productId,
                    component.Value.Kind,
                    performanceScore,
                    thermalLoadWatts,
                    coolingCapacityWatts));
        }

        internal static bool IsSupportedKind(PcComponentKind kind)
        {
            return kind == PcComponentKind.Motherboard ||
                   kind == PcComponentKind.Processor ||
                   kind == PcComponentKind.MemoryModule ||
                   kind == PcComponentKind.StorageDevice ||
                   kind == PcComponentKind.ProcessorCooler ||
                   kind == PcComponentKind.GraphicsCard ||
                   kind == PcComponentKind.PowerSupply;
        }

        internal static bool HasValidKindShape(
            PcComponentKind kind,
            int thermalLoadWatts,
            int coolingCapacityWatts)
        {
            switch (kind)
            {
                case PcComponentKind.Processor:
                    return thermalLoadWatts > 0 && coolingCapacityWatts == 0;
                case PcComponentKind.GraphicsCard:
                    return thermalLoadWatts > 0 &&
                           coolingCapacityWatts >= thermalLoadWatts;
                case PcComponentKind.ProcessorCooler:
                    return thermalLoadWatts == 0 && coolingCapacityWatts > 0;
                case PcComponentKind.Motherboard:
                case PcComponentKind.MemoryModule:
                case PcComponentKind.StorageDevice:
                case PcComponentKind.PowerSupply:
                    return thermalLoadWatts == 0 && coolingCapacityWatts == 0;
                default:
                    return false;
            }
        }

        internal static bool IsValidScore(int performanceScore)
        {
            return performanceScore > 0 && performanceScore <= MaximumSupportedScore;
        }

        internal static bool IsWithinBounds(int watts)
        {
            return watts >= 0 && watts <= MaximumSupportedWatts;
        }

        private static OperationResult<PcComponentSpecification> ResolveComponent(
            PcComponentCatalog componentCatalog,
            StableId<ProductDefinitionIdScope> productId)
        {
            if (componentCatalog == null)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    PcPerformanceCatalogFailures.MissingComponentCatalog);
            }

            if (productId.IsEmpty)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    PcPerformanceCatalogFailures.InvalidProductId);
            }

            return componentCatalog.TryGet(productId, out PcComponentSpecification component)
                ? OperationResult<PcComponentSpecification>.Success(component)
                : OperationResult<PcComponentSpecification>.Fail(
                    PcPerformanceCatalogFailures.UnknownComponentProduct);
        }
    }

    /// <summary>
    /// Validated immutable registry for deterministic component performance and thermal data.
    /// It owns no gameplay state, result, receipt, or live hardware measurement.
    /// </summary>
    public sealed class PcPerformanceCatalog
    {
        private readonly Dictionary<StableId<ProductDefinitionIdScope>, PcPerformanceSpecification>
            _byProductId;
        private readonly IReadOnlyList<PcPerformanceSpecification> _specifications;

        private PcPerformanceCatalog(
            StableId<PcPerformanceCatalogIdScope> catalogId,
            PcComponentCatalog ownerComponentCatalog,
            Dictionary<StableId<ProductDefinitionIdScope>, PcPerformanceSpecification> byProductId,
            IReadOnlyList<PcPerformanceSpecification> specifications)
        {
            CatalogId = catalogId;
            OwnerComponentCatalog = ownerComponentCatalog;
            _byProductId = byProductId;
            _specifications = specifications;
        }

        public StableId<PcPerformanceCatalogIdScope> CatalogId { get; }

        internal PcComponentCatalog OwnerComponentCatalog { get; }

        public int Count => _specifications.Count;

        public IReadOnlyList<PcPerformanceSpecification> Specifications => _specifications;

        public static OperationResult<PcPerformanceCatalog> Create(
            StableId<PcPerformanceCatalogIdScope> catalogId,
            PcComponentCatalog componentCatalog,
            IEnumerable<PcPerformanceSpecification> specifications)
        {
            if (catalogId.IsEmpty)
            {
                return OperationResult<PcPerformanceCatalog>.Fail(
                    PcPerformanceCatalogFailures.InvalidCatalogId);
            }

            if (componentCatalog == null)
            {
                return OperationResult<PcPerformanceCatalog>.Fail(
                    PcPerformanceCatalogFailures.MissingComponentCatalog);
            }

            if (specifications == null)
            {
                return OperationResult<PcPerformanceCatalog>.Fail(
                    PcPerformanceCatalogFailures.EmptyCatalog);
            }

            var byProductId = new Dictionary<
                StableId<ProductDefinitionIdScope>, PcPerformanceSpecification>();
            var ordered = new List<PcPerformanceSpecification>();
            foreach (PcPerformanceSpecification specification in specifications)
            {
                if (specification == null)
                {
                    return OperationResult<PcPerformanceCatalog>.Fail(
                        PcPerformanceCatalogFailures.NullSpecification);
                }

                if (!ReferenceEquals(
                        specification.OwnerComponentCatalog,
                        componentCatalog))
                {
                    return OperationResult<PcPerformanceCatalog>.Fail(
                        PcPerformanceCatalogFailures.ComponentCatalogMismatch);
                }

                if (!componentCatalog.TryGet(
                        specification.ProductId,
                        out PcComponentSpecification component) ||
                    component.Kind != specification.ComponentKind)
                {
                    return OperationResult<PcPerformanceCatalog>.Fail(
                        PcPerformanceCatalogFailures.MetadataMismatch);
                }

                if (!PcPerformanceSpecification.IsSupportedKind(
                        specification.ComponentKind) ||
                    !PcPerformanceSpecification.IsValidScore(
                        specification.PerformanceScore) ||
                    !PcPerformanceSpecification.IsWithinBounds(
                        specification.ThermalLoadWatts) ||
                    !PcPerformanceSpecification.IsWithinBounds(
                        specification.CoolingCapacityWatts) ||
                    !PcPerformanceSpecification.HasValidKindShape(
                        specification.ComponentKind,
                        specification.ThermalLoadWatts,
                        specification.CoolingCapacityWatts))
                {
                    return OperationResult<PcPerformanceCatalog>.Fail(
                        PcPerformanceCatalogFailures.MetadataMismatch);
                }

                if (byProductId.ContainsKey(specification.ProductId))
                {
                    return OperationResult<PcPerformanceCatalog>.Fail(
                        PcPerformanceCatalogFailures.DuplicateSpecification);
                }

                byProductId.Add(specification.ProductId, specification);
                ordered.Add(specification);
            }

            if (ordered.Count == 0)
            {
                return OperationResult<PcPerformanceCatalog>.Fail(
                    PcPerformanceCatalogFailures.EmptyCatalog);
            }

            ordered.Sort((left, right) => string.Compare(
                left.ProductId.Value,
                right.ProductId.Value,
                StringComparison.Ordinal));
            return OperationResult<PcPerformanceCatalog>.Success(
                new PcPerformanceCatalog(
                    catalogId,
                    componentCatalog,
                    byProductId,
                    Array.AsReadOnly(ordered.ToArray())));
        }

        public bool TryGet(
            StableId<ProductDefinitionIdScope> productId,
            out PcPerformanceSpecification specification)
        {
            return _byProductId.TryGetValue(productId, out specification);
        }

        public OperationResult<PcPerformanceSpecification> Get(
            StableId<ProductDefinitionIdScope> productId)
        {
            return TryGet(productId, out PcPerformanceSpecification specification)
                ? OperationResult<PcPerformanceSpecification>.Success(specification)
                : OperationResult<PcPerformanceSpecification>.Fail(
                    PcPerformanceCatalogFailures.UnknownSpecification);
        }
    }

    public static class PcPerformanceCatalogFailures
    {
        public static readonly Failure InvalidCatalogId = Failure.FromCode(
            "catalog.performance.catalog-id.invalid");
        public static readonly Failure MissingComponentCatalog = Failure.FromCode(
            "catalog.performance.component-catalog.missing");
        public static readonly Failure InvalidProductId = Failure.FromCode(
            "catalog.performance.product-id.invalid");
        public static readonly Failure UnknownComponentProduct = Failure.FromCode(
            "catalog.performance.component-product.unknown");
        public static readonly Failure UnsupportedComponentKind = Failure.FromCode(
            "catalog.performance.component-kind.unsupported");
        public static readonly Failure InvalidPerformanceScore = Failure.FromCode(
            "catalog.performance.score.invalid");
        public static readonly Failure InvalidThermalWatts = Failure.FromCode(
            "catalog.performance.thermal-watts.invalid");
        public static readonly Failure InvalidThermalShape = Failure.FromCode(
            "catalog.performance.thermal-shape.invalid");
        public static readonly Failure EmptyCatalog = Failure.FromCode(
            "catalog.performance.empty");
        public static readonly Failure NullSpecification = Failure.FromCode(
            "catalog.performance.specification.null");
        public static readonly Failure ComponentCatalogMismatch = Failure.FromCode(
            "catalog.performance.component-catalog.mismatch");
        public static readonly Failure MetadataMismatch = Failure.FromCode(
            "catalog.performance.metadata.mismatch");
        public static readonly Failure DuplicateSpecification = Failure.FromCode(
            "catalog.performance.specification.duplicate");
        public static readonly Failure UnknownSpecification = Failure.FromCode(
            "catalog.performance.specification.unknown");
    }
}
