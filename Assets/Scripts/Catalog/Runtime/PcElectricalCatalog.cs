using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Catalog
{
    /// <summary>
    /// Immutable electrical metadata for one authoritative serialized PC-component product.
    /// Loads and PSU rated output are deliberately separate from mechanical fitment metadata.
    /// </summary>
    public sealed class PcElectricalSpecification
    {
        public const int MaximumSupportedWatts = 100_000;

        private PcElectricalSpecification(
            PcComponentCatalog ownerComponentCatalog,
            StableId<ProductDefinitionIdScope> productId,
            PcComponentKind componentKind,
            int loadWatts,
            int ratedOutputWatts)
        {
            OwnerComponentCatalog = ownerComponentCatalog;
            ProductId = productId;
            ComponentKind = componentKind;
            LoadWatts = loadWatts;
            RatedOutputWatts = ratedOutputWatts;
        }

        internal PcComponentCatalog OwnerComponentCatalog { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public PcComponentKind ComponentKind { get; }

        public int LoadWatts { get; }

        public int RatedOutputWatts { get; }

        public bool IsLoadProfile => LoadWatts > 0 && RatedOutputWatts == 0;

        public bool IsPowerSupplyProfile =>
            ComponentKind == PcComponentKind.PowerSupply &&
            LoadWatts == 0 &&
            RatedOutputWatts > 0;

        public static OperationResult<PcElectricalSpecification> CreateLoad(
            PcComponentCatalog componentCatalog,
            StableId<ProductDefinitionIdScope> productId,
            int loadWatts)
        {
            OperationResult<PcComponentSpecification> component =
                ResolveComponent(componentCatalog, productId);
            if (component.IsFailure)
            {
                return OperationResult<PcElectricalSpecification>.Fail(component.Error);
            }

            if (!IsSupportedLoadKind(component.Value.Kind))
            {
                return OperationResult<PcElectricalSpecification>.Fail(
                    PcElectricalCatalogFailures.UnsupportedLoadKind);
            }

            if (!IsValidWatts(loadWatts))
            {
                return OperationResult<PcElectricalSpecification>.Fail(
                    PcElectricalCatalogFailures.InvalidLoadWatts);
            }

            return OperationResult<PcElectricalSpecification>.Success(
                new PcElectricalSpecification(
                    componentCatalog,
                    productId,
                    component.Value.Kind,
                    loadWatts,
                    0));
        }

        public static OperationResult<PcElectricalSpecification> CreatePowerSupply(
            PcComponentCatalog componentCatalog,
            StableId<ProductDefinitionIdScope> productId,
            int ratedOutputWatts)
        {
            OperationResult<PcComponentSpecification> component =
                ResolveComponent(componentCatalog, productId);
            if (component.IsFailure)
            {
                return OperationResult<PcElectricalSpecification>.Fail(component.Error);
            }

            if (component.Value.Kind != PcComponentKind.PowerSupply)
            {
                return OperationResult<PcElectricalSpecification>.Fail(
                    PcElectricalCatalogFailures.PowerSupplyKindMismatch);
            }

            if (!IsValidWatts(ratedOutputWatts))
            {
                return OperationResult<PcElectricalSpecification>.Fail(
                    PcElectricalCatalogFailures.InvalidRatedOutputWatts);
            }

            return OperationResult<PcElectricalSpecification>.Success(
                new PcElectricalSpecification(
                    componentCatalog,
                    productId,
                    component.Value.Kind,
                    0,
                    ratedOutputWatts));
        }

        internal static bool IsSupportedLoadKind(PcComponentKind kind)
        {
            return kind == PcComponentKind.Processor ||
                   kind == PcComponentKind.MemoryModule ||
                   kind == PcComponentKind.StorageDevice ||
                   kind == PcComponentKind.ProcessorCooler ||
                   kind == PcComponentKind.GraphicsCard;
        }

        private static bool IsValidWatts(int watts)
        {
            return watts > 0 && watts <= MaximumSupportedWatts;
        }

        private static OperationResult<PcComponentSpecification> ResolveComponent(
            PcComponentCatalog componentCatalog,
            StableId<ProductDefinitionIdScope> productId)
        {
            if (componentCatalog == null)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    PcElectricalCatalogFailures.MissingComponentCatalog);
            }

            if (productId.IsEmpty)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    PcElectricalCatalogFailures.InvalidProductId);
            }

            return componentCatalog.TryGet(productId, out PcComponentSpecification component)
                ? OperationResult<PcComponentSpecification>.Success(component)
                : OperationResult<PcComponentSpecification>.Fail(
                    PcElectricalCatalogFailures.UnknownComponentProduct);
        }
    }

    /// <summary>
    /// Validated immutable registry for component load and PSU rated-output metadata.
    /// Product identity remains owned by ProductCatalog and component kind by PcComponentCatalog.
    /// </summary>
    public sealed class PcElectricalCatalog
    {
        private readonly Dictionary<StableId<ProductDefinitionIdScope>, PcElectricalSpecification>
            _byProductId;
        private readonly IReadOnlyList<PcElectricalSpecification> _specifications;

        private PcElectricalCatalog(
            PcComponentCatalog ownerComponentCatalog,
            Dictionary<StableId<ProductDefinitionIdScope>, PcElectricalSpecification> byProductId,
            IReadOnlyList<PcElectricalSpecification> specifications)
        {
            OwnerComponentCatalog = ownerComponentCatalog;
            _byProductId = byProductId;
            _specifications = specifications;
        }

        internal PcComponentCatalog OwnerComponentCatalog { get; }

        public int Count => _specifications.Count;

        public IReadOnlyList<PcElectricalSpecification> Specifications => _specifications;

        public static OperationResult<PcElectricalCatalog> Create(
            PcComponentCatalog componentCatalog,
            IEnumerable<PcElectricalSpecification> specifications)
        {
            if (componentCatalog == null)
            {
                return OperationResult<PcElectricalCatalog>.Fail(
                    PcElectricalCatalogFailures.MissingComponentCatalog);
            }

            if (specifications == null)
            {
                return OperationResult<PcElectricalCatalog>.Fail(
                    PcElectricalCatalogFailures.EmptyCatalog);
            }

            var byProductId = new Dictionary<
                StableId<ProductDefinitionIdScope>, PcElectricalSpecification>();
            var ordered = new List<PcElectricalSpecification>();
            foreach (PcElectricalSpecification specification in specifications)
            {
                if (specification == null)
                {
                    return OperationResult<PcElectricalCatalog>.Fail(
                        PcElectricalCatalogFailures.NullSpecification);
                }

                if (!ReferenceEquals(
                        specification.OwnerComponentCatalog,
                        componentCatalog))
                {
                    return OperationResult<PcElectricalCatalog>.Fail(
                        PcElectricalCatalogFailures.ComponentCatalogMismatch);
                }

                if (!componentCatalog.TryGet(
                        specification.ProductId,
                        out PcComponentSpecification component) ||
                    component.Kind != specification.ComponentKind)
                {
                    return OperationResult<PcElectricalCatalog>.Fail(
                        PcElectricalCatalogFailures.MetadataMismatch);
                }

                bool metadataValid = specification.IsPowerSupplyProfile ||
                                     (specification.IsLoadProfile &&
                                      PcElectricalSpecification.IsSupportedLoadKind(
                                          specification.ComponentKind));
                if (!metadataValid)
                {
                    return OperationResult<PcElectricalCatalog>.Fail(
                        PcElectricalCatalogFailures.MetadataMismatch);
                }

                if (byProductId.ContainsKey(specification.ProductId))
                {
                    return OperationResult<PcElectricalCatalog>.Fail(
                        PcElectricalCatalogFailures.DuplicateSpecification);
                }

                byProductId.Add(specification.ProductId, specification);
                ordered.Add(specification);
            }

            if (ordered.Count == 0)
            {
                return OperationResult<PcElectricalCatalog>.Fail(
                    PcElectricalCatalogFailures.EmptyCatalog);
            }

            ordered.Sort((left, right) => string.Compare(
                left.ProductId.Value,
                right.ProductId.Value,
                StringComparison.Ordinal));
            return OperationResult<PcElectricalCatalog>.Success(
                new PcElectricalCatalog(
                    componentCatalog,
                    byProductId,
                    Array.AsReadOnly(ordered.ToArray())));
        }

        public bool TryGet(
            StableId<ProductDefinitionIdScope> productId,
            out PcElectricalSpecification specification)
        {
            return _byProductId.TryGetValue(productId, out specification);
        }

        public OperationResult<PcElectricalSpecification> Get(
            StableId<ProductDefinitionIdScope> productId)
        {
            return TryGet(productId, out PcElectricalSpecification specification)
                ? OperationResult<PcElectricalSpecification>.Success(specification)
                : OperationResult<PcElectricalSpecification>.Fail(
                    PcElectricalCatalogFailures.UnknownSpecification);
        }
    }

    public static class PcElectricalCatalogFailures
    {
        public static readonly Failure MissingComponentCatalog = Failure.FromCode(
            "catalog.electrical.component-catalog.missing");
        public static readonly Failure InvalidProductId = Failure.FromCode(
            "catalog.electrical.product-id.invalid");
        public static readonly Failure UnknownComponentProduct = Failure.FromCode(
            "catalog.electrical.component-product.unknown");
        public static readonly Failure UnsupportedLoadKind = Failure.FromCode(
            "catalog.electrical.load-kind.unsupported");
        public static readonly Failure PowerSupplyKindMismatch = Failure.FromCode(
            "catalog.electrical.power-supply-kind.mismatch");
        public static readonly Failure InvalidLoadWatts = Failure.FromCode(
            "catalog.electrical.load-watts.invalid");
        public static readonly Failure InvalidRatedOutputWatts = Failure.FromCode(
            "catalog.electrical.rated-output-watts.invalid");
        public static readonly Failure EmptyCatalog = Failure.FromCode(
            "catalog.electrical.empty");
        public static readonly Failure NullSpecification = Failure.FromCode(
            "catalog.electrical.specification.null");
        public static readonly Failure ComponentCatalogMismatch = Failure.FromCode(
            "catalog.electrical.component-catalog.mismatch");
        public static readonly Failure MetadataMismatch = Failure.FromCode(
            "catalog.electrical.metadata.mismatch");
        public static readonly Failure DuplicateSpecification = Failure.FromCode(
            "catalog.electrical.specification.duplicate");
        public static readonly Failure UnknownSpecification = Failure.FromCode(
            "catalog.electrical.specification.unknown");
    }
}
