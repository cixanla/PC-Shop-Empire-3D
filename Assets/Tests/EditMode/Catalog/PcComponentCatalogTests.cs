using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Tests.EditMode.Catalog
{
    public sealed class PcComponentCatalogTests
    {
        [Test]
        public void PersistedComponentDimmAndStorageEnumValuesAreAppendOnly()
        {
            Assert.That((int)PcComponentKind.Motherboard, Is.EqualTo(1));
            Assert.That((int)PcComponentKind.Processor, Is.EqualTo(2));
            Assert.That((int)PcComponentKind.MemoryModule, Is.EqualTo(3));
            Assert.That((int)PcComponentKind.StorageDevice, Is.EqualTo(4));
            Assert.That((int)PcComponentKind.ProcessorCooler, Is.EqualTo(5));
            Assert.That((int)DimmType.Ddr5Udimm, Is.EqualTo(1));
            Assert.That((int)M2StorageType.NvmePcie4X4_2280, Is.EqualTo(1));
            Assert.That(
                (int)ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                Is.EqualTo(1));
        }

        [Test]
        public void MotherboardSpecificationKeepsAuthoritativeProductIdentityAndFormFactor()
        {
            ProductCatalog products = CreateProducts();
            StableId<ProductDefinitionIdScope> productId = ProductId("component.motherboard-matx");

            OperationResult<PcComponentSpecification> result = PcComponentSpecification.Create(
                products,
                productId,
                PcComponentKind.Motherboard,
                MotherboardFormFactor.MicroAtx);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.ProductId, Is.EqualTo(productId));
            Assert.That(result.Value.Kind, Is.EqualTo(PcComponentKind.Motherboard));
            Assert.That(result.Value.MotherboardFormFactor,
                Is.EqualTo(MotherboardFormFactor.MicroAtx));
        }

        [Test]
        public void ProcessorAndMotherboardSpecificationsKeepTypedSocketCompatibility()
        {
            ProductCatalog products = CreateProducts();
            StableId<ProductDefinitionIdScope> motherboardId =
                ProductId("component.motherboard-matx");
            StableId<ProductDefinitionIdScope> processorId =
                ProductId("component.processor-lga1700");

            OperationResult<PcComponentSpecification> motherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    motherboardId,
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700);
            OperationResult<PcComponentSpecification> processor =
                PcComponentSpecification.CreateProcessor(
                    products,
                    processorId,
                    CpuSocketFamily.Lga1700);

            Assert.That(motherboard.IsSuccess, Is.True);
            Assert.That(motherboard.Value.Kind, Is.EqualTo(PcComponentKind.Motherboard));
            Assert.That(motherboard.Value.CpuSocketFamily,
                Is.EqualTo(CpuSocketFamily.Lga1700));
            Assert.That(processor.IsSuccess, Is.True);
            Assert.That(processor.Value.ProductId, Is.EqualTo(processorId));
            Assert.That(processor.Value.Kind, Is.EqualTo(PcComponentKind.Processor));
            Assert.That(processor.Value.MotherboardFormFactor,
                Is.EqualTo(default(MotherboardFormFactor)));
            Assert.That(processor.Value.CpuSocketFamily,
                Is.EqualTo(CpuSocketFamily.Lga1700));
        }

        [Test]
        public void MotherboardAndMemoryModuleKeepTypedDdr5UdimmCompatibility()
        {
            ProductCatalog products = CreateProducts();
            StableId<ProductDefinitionIdScope> motherboardId =
                ProductId("component.motherboard-matx");
            StableId<ProductDefinitionIdScope> memoryId =
                ProductId("component.memory-ddr5-udimm");

            OperationResult<PcComponentSpecification> motherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    motherboardId,
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    DimmType.Ddr5Udimm);
            OperationResult<PcComponentSpecification> memory =
                PcComponentSpecification.CreateMemoryModule(
                    products,
                    memoryId,
                    DimmType.Ddr5Udimm);

            Assert.That(motherboard.IsSuccess, Is.True);
            Assert.That(motherboard.Value.DimmType, Is.EqualTo(DimmType.Ddr5Udimm));
            Assert.That(memory.IsSuccess, Is.True);
            Assert.That(memory.Value.Kind, Is.EqualTo(PcComponentKind.MemoryModule));
            Assert.That(memory.Value.ProductId, Is.EqualTo(memoryId));
            Assert.That(memory.Value.DimmType, Is.EqualTo(DimmType.Ddr5Udimm));
            Assert.That(memory.Value.MotherboardFormFactor,
                Is.EqualTo(default(MotherboardFormFactor)));
            Assert.That(memory.Value.CpuSocketFamily,
                Is.EqualTo(default(CpuSocketFamily)));

            PcComponentSpecification legacyMotherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    motherboardId,
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700).Value;
            Assert.That(legacyMotherboard.DimmType, Is.EqualTo(default(DimmType)));
            Assert.That(legacyMotherboard.M2StorageType,
                Is.EqualTo(default(M2StorageType)));
        }

        [Test]
        public void MotherboardAndStorageDeviceKeepTypedM2Nvme2280Compatibility()
        {
            ProductCatalog products = CreateProducts();
            StableId<ProductDefinitionIdScope> motherboardId =
                ProductId("component.motherboard-matx");
            StableId<ProductDefinitionIdScope> storageId =
                ProductId("component.storage-nvme-2280");

            OperationResult<PcComponentSpecification> motherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    motherboardId,
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    DimmType.Ddr5Udimm,
                    M2StorageType.NvmePcie4X4_2280);
            OperationResult<PcComponentSpecification> storage =
                PcComponentSpecification.CreateStorageDevice(
                    products,
                    storageId,
                    M2StorageType.NvmePcie4X4_2280);

            Assert.That(motherboard.IsSuccess, Is.True);
            Assert.That(motherboard.Value.M2StorageType,
                Is.EqualTo(M2StorageType.NvmePcie4X4_2280));
            Assert.That(storage.IsSuccess, Is.True);
            Assert.That(storage.Value.Kind, Is.EqualTo(PcComponentKind.StorageDevice));
            Assert.That(storage.Value.ProductId, Is.EqualTo(storageId));
            Assert.That(storage.Value.M2StorageType,
                Is.EqualTo(M2StorageType.NvmePcie4X4_2280));
            Assert.That(storage.Value.MotherboardFormFactor,
                Is.EqualTo(default(MotherboardFormFactor)));
            Assert.That(storage.Value.CpuSocketFamily,
                Is.EqualTo(default(CpuSocketFamily)));
            Assert.That(storage.Value.DimmType, Is.EqualTo(default(DimmType)));
        }

        [Test]
        public void ProcessorCoolerKeepsTypedCoolerAndSocketCompatibility()
        {
            ProductCatalog products = CreateProducts();
            StableId<ProductDefinitionIdScope> coolerId =
                ProductId("component.cooler-lga1700-top-down-air");

            OperationResult<PcComponentSpecification> result =
                PcComponentSpecification.CreateProcessorCooler(
                    products,
                    coolerId,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.ProductId, Is.EqualTo(coolerId));
            Assert.That(result.Value.Kind, Is.EqualTo(PcComponentKind.ProcessorCooler));
            Assert.That(result.Value.ProcessorCoolerType,
                Is.EqualTo(ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim));
            Assert.That(result.Value.CpuSocketFamily,
                Is.EqualTo(CpuSocketFamily.Lga1700));
            Assert.That(result.Value.MotherboardFormFactor,
                Is.EqualTo(default(MotherboardFormFactor)));
            Assert.That(result.Value.DimmType, Is.EqualTo(default(DimmType)));
            Assert.That(result.Value.M2StorageType,
                Is.EqualTo(default(M2StorageType)));
        }

        [Test]
        public void ComponentCatalogRegistersMotherboardProcessorMemoryStorageAndCoolerMetadataTogether()
        {
            ProductCatalog products = CreateProducts();
            PcComponentSpecification motherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    ProductId("component.motherboard-matx"),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    DimmType.Ddr5Udimm,
                    M2StorageType.NvmePcie4X4_2280).Value;
            PcComponentSpecification processor =
                PcComponentSpecification.CreateProcessor(
                    products,
                    ProductId("component.processor-lga1700"),
                    CpuSocketFamily.Lga1700).Value;
            PcComponentSpecification memory =
                PcComponentSpecification.CreateMemoryModule(
                    products,
                    ProductId("component.memory-ddr5-udimm"),
                    DimmType.Ddr5Udimm).Value;
            PcComponentSpecification storage =
                PcComponentSpecification.CreateStorageDevice(
                    products,
                    ProductId("component.storage-nvme-2280"),
                    M2StorageType.NvmePcie4X4_2280).Value;
            PcComponentSpecification cooler =
                PcComponentSpecification.CreateProcessorCooler(
                    products,
                    ProductId("component.cooler-lga1700-top-down-air"),
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700).Value;

            OperationResult<PcComponentCatalog> result = PcComponentCatalog.Create(
                products,
                new[] { motherboard, processor, memory, storage, cooler });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Count, Is.EqualTo(5));
            Assert.That(result.Value.Get(cooler.ProductId).Value, Is.SameAs(cooler));
            Assert.That(result.Value.Get(memory.ProductId).Value, Is.SameAs(memory));
            Assert.That(result.Value.Specifications[0], Is.SameAs(cooler));
            Assert.That(result.Value.Specifications[1], Is.SameAs(memory));
            Assert.That(result.Value.Specifications[2], Is.SameAs(motherboard));
            Assert.That(result.Value.Specifications[3], Is.SameAs(processor));
            Assert.That(result.Value.Specifications[4], Is.SameAs(storage));
            Assert.That(PcComponentSpecification.Create(
                    products,
                    memory.ProductId,
                    PcComponentKind.MemoryModule,
                    MotherboardFormFactor.MicroAtx).Error,
                Is.EqualTo(CatalogFailures.ComponentMetadataMismatch));
        }

        [Test]
        public void ProcessorCoolerRejectsInvalidDefaultKindAndNonSerializedProductData()
        {
            ProductCatalog products = CreateProducts();
            StableId<ProductDefinitionIdScope> coolerId =
                ProductId("component.cooler-lga1700-top-down-air");

            Assert.That(PcComponentSpecification.CreateProcessorCooler(
                    products,
                    coolerId,
                    default,
                    CpuSocketFamily.Lga1700).Error,
                Is.EqualTo(CatalogFailures.ComponentMetadataMismatch));
            Assert.That(PcComponentSpecification.CreateProcessorCooler(
                    products,
                    coolerId,
                    (ProcessorCoolerType)99,
                    CpuSocketFamily.Lga1700).Error,
                Is.EqualTo(CatalogFailures.ComponentMetadataMismatch));
            Assert.That(PcComponentSpecification.CreateProcessorCooler(
                    products,
                    coolerId,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    default).Error,
                Is.EqualTo(CatalogFailures.InvalidCpuSocketFamily));
            Assert.That(PcComponentSpecification.CreateProcessorCooler(
                    products,
                    coolerId,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Am5).Error,
                Is.EqualTo(CatalogFailures.ComponentMetadataMismatch));
            Assert.That(PcComponentSpecification.Create(
                    products,
                    coolerId,
                    PcComponentKind.ProcessorCooler,
                    MotherboardFormFactor.MicroAtx).Error,
                Is.EqualTo(CatalogFailures.ComponentMetadataMismatch));
            Assert.That(PcComponentSpecification.CreateProcessorCooler(
                    products,
                    ProductId("consumable.screw"),
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700).Error,
                Is.EqualTo(CatalogFailures.ComponentTrackingMismatch));
        }

        [Test]
        public void SpecificationRejectsUnknownBatchAndInvalidCompatibilityData()
        {
            ProductCatalog products = CreateProducts();

            Assert.That(PcComponentSpecification.Create(
                    null,
                    ProductId("component.motherboard-matx"),
                    PcComponentKind.Motherboard,
                    MotherboardFormFactor.MicroAtx).Error,
                Is.EqualTo(CatalogFailures.MissingProductCatalog));
            Assert.That(PcComponentSpecification.Create(
                    products,
                    ProductId("component.unknown"),
                    PcComponentKind.Motherboard,
                    MotherboardFormFactor.MicroAtx).Error,
                Is.EqualTo(CatalogFailures.UnknownComponentProduct));
            Assert.That(PcComponentSpecification.Create(
                    products,
                    ProductId("consumable.screw"),
                    PcComponentKind.Motherboard,
                    MotherboardFormFactor.MicroAtx).Error,
                Is.EqualTo(CatalogFailures.ComponentTrackingMismatch));
            Assert.That(PcComponentSpecification.Create(
                    products,
                    ProductId("component.motherboard-matx"),
                    (PcComponentKind)99,
                    MotherboardFormFactor.MicroAtx).Error,
                Is.EqualTo(CatalogFailures.InvalidComponentKind));
            Assert.That(PcComponentSpecification.Create(
                    products,
                    ProductId("component.motherboard-matx"),
                    PcComponentKind.Motherboard,
                    (MotherboardFormFactor)99).Error,
                Is.EqualTo(CatalogFailures.InvalidMotherboardFormFactor));
            Assert.That(PcComponentSpecification.Create(
                    products,
                    ProductId("component.processor-lga1700"),
                    PcComponentKind.Processor,
                    MotherboardFormFactor.MicroAtx).Error,
                Is.EqualTo(CatalogFailures.ComponentMetadataMismatch));
            Assert.That(PcComponentSpecification.CreateProcessor(
                    products,
                    ProductId("component.processor-lga1700"),
                    (CpuSocketFamily)99).Error,
                Is.EqualTo(CatalogFailures.InvalidCpuSocketFamily));
            Assert.That(PcComponentSpecification.CreateMemoryModule(
                    products,
                    ProductId("component.memory-ddr5-udimm"),
                    default).Error,
                Is.EqualTo(CatalogFailures.InvalidDimmType));
            Assert.That(PcComponentSpecification.CreateMotherboard(
                    products,
                    ProductId("component.motherboard-matx"),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    (DimmType)99).Error,
                Is.EqualTo(CatalogFailures.InvalidDimmType));
            Assert.That(PcComponentSpecification.CreateStorageDevice(
                    products,
                    ProductId("component.storage-nvme-2280"),
                    default).Error,
                Is.EqualTo(CatalogFailures.InvalidM2StorageType));
            Assert.That(PcComponentSpecification.CreateMotherboard(
                    products,
                    ProductId("component.motherboard-matx"),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    DimmType.Ddr5Udimm,
                    (M2StorageType)99).Error,
                Is.EqualTo(CatalogFailures.InvalidM2StorageType));
        }

        [Test]
        public void ComponentCatalogIsImmutableOrderedAndRejectsDuplicateOrEmptyInput()
        {
            ProductCatalog products = CreateProducts();
            PcComponentSpecification microAtx = Specification(
                products,
                "component.motherboard-matx",
                MotherboardFormFactor.MicroAtx);
            PcComponentSpecification atx = Specification(
                products,
                "component.motherboard-atx",
                MotherboardFormFactor.Atx);

            PcComponentCatalog catalog = PcComponentCatalog.Create(
                products,
                new[] { microAtx, atx }).Value;

            Assert.That(catalog.Count, Is.EqualTo(2));
            Assert.That(catalog.Specifications[0], Is.SameAs(atx));
            Assert.That(catalog.Specifications[1], Is.SameAs(microAtx));
            Assert.That(catalog.Get(microAtx.ProductId).Value, Is.SameAs(microAtx));
            Assert.That(catalog.Get(ProductId("component.unknown")).Error,
                Is.EqualTo(CatalogFailures.UnknownComponentSpecification));
            Assert.That(PcComponentCatalog.Create(products, null).Error,
                Is.EqualTo(CatalogFailures.EmptyComponentCatalog));
            Assert.That(PcComponentCatalog.Create(
                    products,
                    System.Array.Empty<PcComponentSpecification>()).Error,
                Is.EqualTo(CatalogFailures.EmptyComponentCatalog));
            Assert.That(PcComponentCatalog.Create(
                    products,
                    new PcComponentSpecification[] { microAtx, null }).Error,
                Is.EqualTo(CatalogFailures.NullComponentSpecification));
            Assert.That(PcComponentCatalog.Create(
                    products,
                    new[] { microAtx, microAtx }).Error,
                Is.EqualTo(CatalogFailures.DuplicateComponentSpecification));
        }

        [Test]
        public void ComponentCatalogRejectsSpecificationFromValueEqualForeignProductCatalog()
        {
            ProductCatalog first = CreateProducts();
            ProductCatalog valueEqualForeign = CreateProducts();
            PcComponentSpecification specification = Specification(
                first,
                "component.motherboard-matx",
                MotherboardFormFactor.MicroAtx);

            OperationResult<PcComponentCatalog> result = PcComponentCatalog.Create(
                valueEqualForeign,
                new[] { specification });

            Assert.That(result.Error,
                Is.EqualTo(CatalogFailures.ComponentProductCatalogMismatch));
        }

        private static ProductCatalog CreateProducts()
        {
            return ProductCatalog.Create(new[]
            {
                Definition("component.motherboard-matx", ProductTrackingPolicy.SerializedInstance),
                Definition("component.motherboard-atx", ProductTrackingPolicy.SerializedInstance),
                Definition("component.processor-lga1700", ProductTrackingPolicy.SerializedInstance),
                Definition("component.memory-ddr5-udimm", ProductTrackingPolicy.SerializedInstance),
                Definition("component.storage-nvme-2280", ProductTrackingPolicy.SerializedInstance),
                Definition("component.cooler-lga1700-top-down-air", ProductTrackingPolicy.SerializedInstance),
                Definition("consumable.screw", ProductTrackingPolicy.BatchQuantity)
            }).Value;
        }

        private static PcComponentSpecification Specification(
            ProductCatalog products,
            string productId,
            MotherboardFormFactor formFactor)
        {
            return PcComponentSpecification.Create(
                products,
                ProductId(productId),
                PcComponentKind.Motherboard,
                formFactor).Value;
        }

        private static ProductDefinition Definition(string id, ProductTrackingPolicy trackingPolicy)
        {
            return ProductDefinition.Create(
                ProductId(id),
                StableId<ProductCategoryIdScope>.Parse("pc-components"),
                id,
                trackingPolicy,
                730).Value;
        }

        private static StableId<ProductDefinitionIdScope> ProductId(string value)
        {
            return StableId<ProductDefinitionIdScope>.Parse(value);
        }
    }
}
