using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PSE.Assembly")]

namespace PCShopEmpire3D.Catalog
{
    /// <summary>
    /// Persisted component category used by physical PC assembly rules. Existing numeric
    /// values are save-data contracts and must never be renumbered.
    /// </summary>
    public enum PcComponentKind
    {
        Motherboard = 1,
        Processor = 2
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
    /// Persisted keyed CPU socket compatibility family. This is assembly compatibility
    /// metadata rather than a display string so comparisons remain culture independent.
    /// </summary>
    public enum CpuSocketFamily
    {
        Lga1700 = 1,
        Am5 = 2
    }

    /// <summary>
    /// Immutable assembly-facing extension of one authoritative product definition.
    /// </summary>
    public sealed class PcComponentSpecification
    {
        private PcComponentSpecification(
            ProductCatalog ownerCatalog,
            StableId<ProductDefinitionIdScope> productId,
            PcComponentKind kind,
            MotherboardFormFactor motherboardFormFactor,
            CpuSocketFamily cpuSocketFamily)
        {
            OwnerCatalog = ownerCatalog;
            ProductId = productId;
            Kind = kind;
            MotherboardFormFactor = motherboardFormFactor;
            CpuSocketFamily = cpuSocketFamily;
        }

        internal ProductCatalog OwnerCatalog { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public PcComponentKind Kind { get; }

        public MotherboardFormFactor MotherboardFormFactor { get; }

        public CpuSocketFamily CpuSocketFamily { get; }

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

            if (kind != PcComponentKind.Motherboard)
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.ComponentMetadataMismatch);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    kind,
                    motherboardFormFactor,
                    default));
        }

        public static OperationResult<PcComponentSpecification> CreateMotherboard(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            MotherboardFormFactor motherboardFormFactor,
            CpuSocketFamily cpuSocketFamily)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidMotherboardFormFactor(motherboardFormFactor))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidMotherboardFormFactor);
            }

            if (!IsValidCpuSocketFamily(cpuSocketFamily))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidCpuSocketFamily);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.Motherboard,
                    motherboardFormFactor,
                    cpuSocketFamily));
        }

        public static OperationResult<PcComponentSpecification> CreateProcessor(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            CpuSocketFamily cpuSocketFamily)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidCpuSocketFamily(cpuSocketFamily))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidCpuSocketFamily);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.Processor,
                    default,
                    cpuSocketFamily));
        }

        public static bool IsValidComponentKind(PcComponentKind kind)
        {
            return kind == PcComponentKind.Motherboard ||
                   kind == PcComponentKind.Processor;
        }

        public static bool IsValidMotherboardFormFactor(MotherboardFormFactor formFactor)
        {
            return formFactor == MotherboardFormFactor.MiniItx ||
                   formFactor == MotherboardFormFactor.MicroAtx ||
                   formFactor == MotherboardFormFactor.Atx;
        }

        public static bool IsValidCpuSocketFamily(CpuSocketFamily socketFamily)
        {
            return socketFamily == CpuSocketFamily.Lga1700 ||
                   socketFamily == CpuSocketFamily.Am5;
        }

        private static Failure ValidateSerializedComponentProduct(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId)
        {
            if (productCatalog == null)
            {
                return CatalogFailures.MissingProductCatalog;
            }

            if (productId.IsEmpty)
            {
                return CatalogFailures.InvalidComponentProductId;
            }

            if (!productCatalog.TryGet(productId, out ProductDefinition definition))
            {
                return CatalogFailures.UnknownComponentProduct;
            }

            return definition.TrackingPolicy == ProductTrackingPolicy.SerializedInstance
                ? Failure.None
                : CatalogFailures.ComponentTrackingMismatch;
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
            ProductCatalog ownerCatalog,
            Dictionary<StableId<ProductDefinitionIdScope>, PcComponentSpecification> byProductId,
            IReadOnlyList<PcComponentSpecification> specifications)
        {
            OwnerCatalog = ownerCatalog;
            _byProductId = byProductId;
            _specifications = specifications;
        }

        internal ProductCatalog OwnerCatalog { get; }

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

                if (!ReferenceEquals(specification.OwnerCatalog, productCatalog))
                {
                    return OperationResult<PcComponentCatalog>.Fail(
                        CatalogFailures.ComponentProductCatalogMismatch);
                }

                if (!productCatalog.TryGet(specification.ProductId, out ProductDefinition definition) ||
                    definition.TrackingPolicy != ProductTrackingPolicy.SerializedInstance)
                {
                    return OperationResult<PcComponentCatalog>.Fail(
                        CatalogFailures.UnknownComponentProduct);
                }

                bool metadataIsValid =
                    specification.Kind == PcComponentKind.Motherboard
                        ? PcComponentSpecification.IsValidMotherboardFormFactor(
                              specification.MotherboardFormFactor) &&
                          (specification.CpuSocketFamily == default ||
                           PcComponentSpecification.IsValidCpuSocketFamily(
                               specification.CpuSocketFamily))
                        : specification.Kind == PcComponentKind.Processor &&
                          specification.MotherboardFormFactor == default &&
                          PcComponentSpecification.IsValidCpuSocketFamily(
                              specification.CpuSocketFamily);
                if (!metadataIsValid)
                {
                    return OperationResult<PcComponentCatalog>.Fail(
                        CatalogFailures.ComponentMetadataMismatch);
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
                new PcComponentCatalog(
                    productCatalog,
                    byProductId,
                    Array.AsReadOnly(ordered.ToArray())));
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
