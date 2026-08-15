using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Tests.EditMode.Catalog
{
    public sealed class ProductCatalogTests
    {
        [Test]
        public void ProductDefinitionAcceptsExplicitValidatedData()
        {
            OperationResult<ProductDefinition> result = ProductDefinition.Create(
                ProductId("gpu.alpha-70"),
                CategoryId("graphics-cards"),
                "Apex 70 Graphics Card",
                ProductTrackingPolicy.SerializedInstance,
                1095);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Id.Value, Is.EqualTo("gpu.alpha-70"));
            Assert.That(result.Value.CategoryId.Value, Is.EqualTo("graphics-cards"));
            Assert.That(result.Value.DisplayName, Is.EqualTo("Apex 70 Graphics Card"));
            Assert.That(result.Value.TrackingPolicy, Is.EqualTo(ProductTrackingPolicy.SerializedInstance));
            Assert.That(result.Value.WarrantyDays, Is.EqualTo(1095));
        }

        [TestCase("", ProductTrackingPolicy.SerializedInstance, 365, "catalog.display-name.invalid")]
        [TestCase(" Padded", ProductTrackingPolicy.SerializedInstance, 365, "catalog.display-name.invalid")]
        [TestCase("Control\nName", ProductTrackingPolicy.SerializedInstance, 365, "catalog.display-name.invalid")]
        [TestCase("Valid", (ProductTrackingPolicy)999, 365, "catalog.tracking-policy.invalid")]
        [TestCase("Valid", ProductTrackingPolicy.BatchQuantity, -1, "catalog.warranty-days.invalid")]
        [TestCase("Valid", ProductTrackingPolicy.BatchQuantity, 3651, "catalog.warranty-days.invalid")]
        public void ProductDefinitionRejectsInvalidPlayerFacingOrPersistedFields(
            string displayName,
            ProductTrackingPolicy policy,
            int warrantyDays,
            string expectedFailure)
        {
            OperationResult<ProductDefinition> result = ProductDefinition.Create(
                ProductId("cable.usb-a"),
                CategoryId("cables"),
                displayName,
                policy,
                warrantyDays);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo(expectedFailure));
        }

        [Test]
        public void CatalogRejectsEmptyNullAndDuplicateDefinitions()
        {
            ProductDefinition definition = Definition("case.zeta", ProductTrackingPolicy.BatchQuantity);

            Assert.That(ProductCatalog.Create(System.Array.Empty<ProductDefinition>()).Error,
                Is.EqualTo(CatalogFailures.EmptyCatalog));
            Assert.That(ProductCatalog.Create(null).Error, Is.EqualTo(CatalogFailures.EmptyCatalog));
            Assert.That(ProductCatalog.Create(new ProductDefinition[] { definition, null }).Error,
                Is.EqualTo(CatalogFailures.NullDefinition));
            Assert.That(ProductCatalog.Create(new[] { definition, definition }).Error,
                Is.EqualTo(CatalogFailures.DuplicateDefinition));
        }

        [Test]
        public void CatalogOrdersDefinitionsByStableIdAndLooksUpExactly()
        {
            ProductDefinition zeta = Definition("zeta.product", ProductTrackingPolicy.BatchQuantity);
            ProductDefinition alpha = Definition("alpha.product", ProductTrackingPolicy.SerializedInstance);

            ProductCatalog catalog = ProductCatalog.Create(new[] { zeta, alpha }).Value;

            Assert.That(catalog.Count, Is.EqualTo(2));
            Assert.That(catalog.Definitions[0].Id, Is.EqualTo(alpha.Id));
            Assert.That(catalog.Definitions[1].Id, Is.EqualTo(zeta.Id));
            Assert.That(catalog.Get(alpha.Id).Value, Is.SameAs(alpha));
            Assert.That(catalog.Get(ProductId("missing.product")).Error, Is.EqualTo(CatalogFailures.UnknownProduct));
        }

        private static ProductDefinition Definition(string id, ProductTrackingPolicy policy)
        {
            return ProductDefinition.Create(
                ProductId(id),
                CategoryId("test-category"),
                id,
                policy,
                365).Value;
        }

        private static StableId<ProductDefinitionIdScope> ProductId(string value)
        {
            return StableId<ProductDefinitionIdScope>.Parse(value);
        }

        private static StableId<ProductCategoryIdScope> CategoryId(string value)
        {
            return StableId<ProductCategoryIdScope>.Parse(value);
        }
    }
}
