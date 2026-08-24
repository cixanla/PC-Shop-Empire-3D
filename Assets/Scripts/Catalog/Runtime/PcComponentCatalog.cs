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
        Processor = 2,
        MemoryModule = 3,
        StorageDevice = 4,
        ProcessorCooler = 5,
        GraphicsCard = 6,
        PowerSupply = 7,
        PowerCable = 8
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
    /// Persisted keyed DIMM compatibility type. Form factor and DDR generation are one
    /// typed key so a visually similar but electrically incompatible module fails closed.
    /// </summary>
    public enum DimmType
    {
        Ddr5Udimm = 1
    }

    /// <summary>
    /// Persisted keyed M.2 storage compatibility type. Interface, protocol and physical
    /// length are one typed key so similarly shaped but incompatible modules fail closed.
    /// </summary>
    public enum M2StorageType
    {
        NvmePcie4X4_2280 = 1
    }

    /// <summary>
    /// Persisted keyed processor cooler fitment and thermal-interface configuration.
    /// This is assembly compatibility metadata rather than a display string.
    /// </summary>
    public enum ProcessorCoolerType
    {
        Lga1700TopDownAirPreAppliedTim = 1
    }

    /// <summary>
    /// Persisted keyed graphics-card electrical interface and physical envelope.
    /// The first bounded prototype supports one full-height dual-slot PCIe 4 x16 card.
    /// </summary>
    public enum GraphicsCardType
    {
        Pcie4X16FullHeightDualSlot = 1
    }

    /// <summary>
    /// Persisted keyed power-supply mechanical envelope. Electrical capacity, rails,
    /// efficiency and cabling remain separate later contracts.
    /// </summary>
    public enum PowerSupplyType
    {
        AtxPs2 = 1
    }

    /// <summary>
    /// Persisted physical cable-family contract. One serialized cable can expose more
    /// than one PSU-side connector while remaining a single inventory item.
    /// </summary>
    public enum PowerCableType
    {
        ModularAtx24SplitPsuToMotherboard = 1,
        ModularEps12v8PinPsuToMotherboard = 2,
        ModularPcie8PinPsuToGraphicsCard = 3
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
            CpuSocketFamily cpuSocketFamily,
            DimmType dimmType,
            M2StorageType m2StorageType,
            ProcessorCoolerType processorCoolerType,
            GraphicsCardType graphicsCardType,
            PowerSupplyType powerSupplyType,
            PowerCableType powerCableType = default)
        {
            OwnerCatalog = ownerCatalog;
            ProductId = productId;
            Kind = kind;
            MotherboardFormFactor = motherboardFormFactor;
            CpuSocketFamily = cpuSocketFamily;
            DimmType = dimmType;
            M2StorageType = m2StorageType;
            ProcessorCoolerType = processorCoolerType;
            GraphicsCardType = graphicsCardType;
            PowerSupplyType = powerSupplyType;
            PowerCableType = powerCableType;
        }

        internal ProductCatalog OwnerCatalog { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public PcComponentKind Kind { get; }

        public MotherboardFormFactor MotherboardFormFactor { get; }

        public CpuSocketFamily CpuSocketFamily { get; }

        public DimmType DimmType { get; }

        public M2StorageType M2StorageType { get; }

        /// <summary>
        /// Typed cooler fitment metadata. It is populated only for processor coolers.
        /// </summary>
        public ProcessorCoolerType ProcessorCoolerType { get; }

        /// <summary>
        /// Typed PCIe interface and physical profile. Motherboards may advertise the
        /// supported profile; graphics cards must carry one exact non-default value.
        /// </summary>
        public GraphicsCardType GraphicsCardType { get; }

        /// <summary>
        /// Typed ATX PSU mechanical envelope. It is populated only for power supplies.
        /// </summary>
        public PowerSupplyType PowerSupplyType { get; }

        /// <summary>
        /// Typed modular-cable topology family. It is populated only for power cables.
        /// </summary>
        public PowerCableType PowerCableType { get; }

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
                    default,
                    default,
                    default,
                    default,
                    default,
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
                    cpuSocketFamily,
                    default,
                    default,
                    default,
                    default,
                    default));
        }

        public static OperationResult<PcComponentSpecification> CreateMotherboard(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            MotherboardFormFactor motherboardFormFactor,
            CpuSocketFamily cpuSocketFamily,
            DimmType supportedDimmType)
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

            if (!IsValidDimmType(supportedDimmType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidDimmType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.Motherboard,
                    motherboardFormFactor,
                    cpuSocketFamily,
                    supportedDimmType,
                    default,
                    default,
                    default,
                    default));
        }

        public static OperationResult<PcComponentSpecification> CreateMotherboard(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            MotherboardFormFactor motherboardFormFactor,
            CpuSocketFamily cpuSocketFamily,
            DimmType supportedDimmType,
            M2StorageType supportedM2StorageType)
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

            if (!IsValidDimmType(supportedDimmType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidDimmType);
            }

            if (!IsValidM2StorageType(supportedM2StorageType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidM2StorageType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.Motherboard,
                    motherboardFormFactor,
                    cpuSocketFamily,
                    supportedDimmType,
                    supportedM2StorageType,
                    default,
                    default,
                    default));
        }

        public static OperationResult<PcComponentSpecification> CreateMotherboard(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            MotherboardFormFactor motherboardFormFactor,
            CpuSocketFamily cpuSocketFamily,
            DimmType supportedDimmType,
            M2StorageType supportedM2StorageType,
            GraphicsCardType supportedGraphicsCardType)
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

            if (!IsValidDimmType(supportedDimmType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidDimmType);
            }

            if (!IsValidM2StorageType(supportedM2StorageType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidM2StorageType);
            }

            if (!IsValidGraphicsCardType(supportedGraphicsCardType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidGraphicsCardType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.Motherboard,
                    motherboardFormFactor,
                    cpuSocketFamily,
                    supportedDimmType,
                    supportedM2StorageType,
                    default,
                    supportedGraphicsCardType,
                    default));
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
                    cpuSocketFamily,
                    default,
                    default,
                    default,
                    default,
                    default));
        }

        public static OperationResult<PcComponentSpecification> CreateMemoryModule(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            DimmType dimmType)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidDimmType(dimmType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidDimmType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.MemoryModule,
                    default,
                    default,
                    dimmType,
                    default,
                    default,
                    default,
                    default));
        }

        public static OperationResult<PcComponentSpecification> CreateStorageDevice(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            M2StorageType m2StorageType)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidM2StorageType(m2StorageType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidM2StorageType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.StorageDevice,
                    default,
                    default,
                    default,
                    m2StorageType,
                    default,
                    default,
                    default));
        }

        /// <summary>
        /// Creates immutable assembly metadata for a serialized processor cooler.
        /// Socket compatibility remains a separate typed key so future cooler variants
        /// do not rely on parsing their persisted cooler type or display name.
        /// </summary>
        public static OperationResult<PcComponentSpecification> CreateProcessorCooler(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            ProcessorCoolerType processorCoolerType,
            CpuSocketFamily cpuSocketFamily)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidProcessorCoolerType(processorCoolerType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.ComponentMetadataMismatch);
            }

            if (!IsValidCpuSocketFamily(cpuSocketFamily))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidCpuSocketFamily);
            }

            if (!IsProcessorCoolerCompatibleWithSocket(
                    processorCoolerType,
                    cpuSocketFamily))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.ComponentMetadataMismatch);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.ProcessorCooler,
                    default,
                    cpuSocketFamily,
                    default,
                    default,
                    processorCoolerType,
                    default,
                    default));
        }

        /// <summary>
        /// Creates immutable assembly metadata for one serialized graphics card while
        /// reusing the authoritative retail product definition.
        /// </summary>
        public static OperationResult<PcComponentSpecification> CreateGraphicsCard(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            GraphicsCardType graphicsCardType)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidGraphicsCardType(graphicsCardType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidGraphicsCardType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.GraphicsCard,
                    default,
                    default,
                    default,
                    default,
                    default,
                    graphicsCardType,
                    default));
        }

        /// <summary>
        /// Creates immutable assembly metadata for one serialized ATX PS/2 power supply.
        /// Mechanical fitment is intentionally independent from future electrical and
        /// cabling compatibility contracts.
        /// </summary>
        public static OperationResult<PcComponentSpecification> CreatePowerSupply(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            PowerSupplyType powerSupplyType)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidPowerSupplyType(powerSupplyType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidPowerSupplyType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.PowerSupply,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    powerSupplyType));
        }

        /// <summary>
        /// Creates immutable assembly metadata for one serialized modular power cable.
        /// Connector identities and route geometry remain separate assembly contracts.
        /// </summary>
        public static OperationResult<PcComponentSpecification> CreatePowerCable(
            ProductCatalog productCatalog,
            StableId<ProductDefinitionIdScope> productId,
            PowerCableType powerCableType)
        {
            Failure productFailure = ValidateSerializedComponentProduct(
                productCatalog,
                productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<PcComponentSpecification>.Fail(productFailure);
            }

            if (!IsValidPowerCableType(powerCableType))
            {
                return OperationResult<PcComponentSpecification>.Fail(
                    CatalogFailures.InvalidPowerCableType);
            }

            return OperationResult<PcComponentSpecification>.Success(
                new PcComponentSpecification(
                    productCatalog,
                    productId,
                    PcComponentKind.PowerCable,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    default,
                    powerCableType));
        }

        public static bool IsValidComponentKind(PcComponentKind kind)
        {
            return kind == PcComponentKind.Motherboard ||
                   kind == PcComponentKind.Processor ||
                   kind == PcComponentKind.MemoryModule ||
                   kind == PcComponentKind.StorageDevice ||
                   kind == PcComponentKind.ProcessorCooler ||
                   kind == PcComponentKind.GraphicsCard ||
                   kind == PcComponentKind.PowerSupply ||
                   kind == PcComponentKind.PowerCable;
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

        public static bool IsValidDimmType(DimmType dimmType)
        {
            return dimmType == DimmType.Ddr5Udimm;
        }

        public static bool IsValidM2StorageType(M2StorageType m2StorageType)
        {
            return m2StorageType == M2StorageType.NvmePcie4X4_2280;
        }

        public static bool IsValidProcessorCoolerType(
            ProcessorCoolerType processorCoolerType)
        {
            return processorCoolerType ==
                   ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim;
        }

        public static bool IsProcessorCoolerCompatibleWithSocket(
            ProcessorCoolerType processorCoolerType,
            CpuSocketFamily cpuSocketFamily)
        {
            return processorCoolerType ==
                       ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim &&
                   cpuSocketFamily == CpuSocketFamily.Lga1700;
        }

        public static bool IsValidGraphicsCardType(GraphicsCardType graphicsCardType)
        {
            return graphicsCardType ==
                   GraphicsCardType.Pcie4X16FullHeightDualSlot;
        }

        public static bool IsValidPowerSupplyType(PowerSupplyType powerSupplyType)
        {
            return powerSupplyType == PowerSupplyType.AtxPs2;
        }

        public static bool IsValidPowerCableType(PowerCableType powerCableType)
        {
            return powerCableType ==
                       PowerCableType.ModularAtx24SplitPsuToMotherboard ||
                   powerCableType ==
                       PowerCableType.ModularEps12v8PinPsuToMotherboard ||
                   powerCableType ==
                       PowerCableType.ModularPcie8PinPsuToGraphicsCard;
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
                               specification.CpuSocketFamily)) &&
                          (specification.DimmType == default ||
                           PcComponentSpecification.IsValidDimmType(
                               specification.DimmType)) &&
                          (specification.M2StorageType == default ||
                           PcComponentSpecification.IsValidM2StorageType(
                               specification.M2StorageType)) &&
                          specification.ProcessorCoolerType == default &&
                          (specification.GraphicsCardType == default ||
                           PcComponentSpecification.IsValidGraphicsCardType(
                               specification.GraphicsCardType)) &&
                          specification.PowerSupplyType == default &&
                          specification.PowerCableType == default
                        : (specification.Kind == PcComponentKind.Processor &&
                           specification.MotherboardFormFactor == default &&
                           PcComponentSpecification.IsValidCpuSocketFamily(
                               specification.CpuSocketFamily) &&
                           specification.DimmType == default &&
                           specification.M2StorageType == default &&
                           specification.ProcessorCoolerType == default &&
                           specification.GraphicsCardType == default &&
                           specification.PowerSupplyType == default &&
                           specification.PowerCableType == default) ||
                          (specification.Kind == PcComponentKind.MemoryModule &&
                           specification.MotherboardFormFactor == default &&
                           specification.CpuSocketFamily == default &&
                           PcComponentSpecification.IsValidDimmType(
                               specification.DimmType) &&
                           specification.M2StorageType == default &&
                           specification.ProcessorCoolerType == default &&
                           specification.GraphicsCardType == default &&
                           specification.PowerSupplyType == default &&
                           specification.PowerCableType == default) ||
                          (specification.Kind == PcComponentKind.StorageDevice &&
                           specification.MotherboardFormFactor == default &&
                           specification.CpuSocketFamily == default &&
                           specification.DimmType == default &&
                           PcComponentSpecification.IsValidM2StorageType(
                               specification.M2StorageType) &&
                           specification.ProcessorCoolerType == default &&
                           specification.GraphicsCardType == default &&
                           specification.PowerSupplyType == default &&
                           specification.PowerCableType == default) ||
                          (specification.Kind == PcComponentKind.ProcessorCooler &&
                           specification.MotherboardFormFactor == default &&
                           PcComponentSpecification.IsValidCpuSocketFamily(
                               specification.CpuSocketFamily) &&
                           specification.DimmType == default &&
                           specification.M2StorageType == default &&
                           PcComponentSpecification.IsValidProcessorCoolerType(
                               specification.ProcessorCoolerType) &&
                           PcComponentSpecification.IsProcessorCoolerCompatibleWithSocket(
                               specification.ProcessorCoolerType,
                               specification.CpuSocketFamily) &&
                           specification.GraphicsCardType == default &&
                           specification.PowerSupplyType == default &&
                           specification.PowerCableType == default) ||
                          (specification.Kind == PcComponentKind.GraphicsCard &&
                           specification.MotherboardFormFactor == default &&
                           specification.CpuSocketFamily == default &&
                           specification.DimmType == default &&
                           specification.M2StorageType == default &&
                           specification.ProcessorCoolerType == default &&
                           PcComponentSpecification.IsValidGraphicsCardType(
                               specification.GraphicsCardType) &&
                           specification.PowerSupplyType == default &&
                           specification.PowerCableType == default) ||
                          (specification.Kind == PcComponentKind.PowerSupply &&
                           specification.MotherboardFormFactor == default &&
                           specification.CpuSocketFamily == default &&
                           specification.DimmType == default &&
                           specification.M2StorageType == default &&
                           specification.ProcessorCoolerType == default &&
                           specification.GraphicsCardType == default &&
                           PcComponentSpecification.IsValidPowerSupplyType(
                               specification.PowerSupplyType) &&
                           specification.PowerCableType == default) ||
                          (specification.Kind == PcComponentKind.PowerCable &&
                           specification.MotherboardFormFactor == default &&
                           specification.CpuSocketFamily == default &&
                           specification.DimmType == default &&
                           specification.M2StorageType == default &&
                           specification.ProcessorCoolerType == default &&
                           specification.GraphicsCardType == default &&
                           specification.PowerSupplyType == default &&
                           PcComponentSpecification.IsValidPowerCableType(
                               specification.PowerCableType));
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
