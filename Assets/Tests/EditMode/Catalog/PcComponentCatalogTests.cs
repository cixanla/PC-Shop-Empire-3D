using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Tests.EditMode.Catalog
{
    public sealed class PcComponentCatalogTests
    {
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
