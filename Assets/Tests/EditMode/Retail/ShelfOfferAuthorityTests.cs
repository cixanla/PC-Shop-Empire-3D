using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Retail
{
    public sealed class ShelfOfferAuthorityTests
    {
        private static readonly StableId<ProductDefinitionIdScope> ProductId =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.tests-a60");
        private static readonly StableId<ProductDefinitionIdScope> SecondProductId =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.tests-b70");
        private static readonly StableId<ContainerIdScope> ShelfId =
            StableId<ContainerIdScope>.Parse("inventory.container.tests-shelf-a");
        private static readonly StableId<ContainerIdScope> SecondShelfId =
            StableId<ContainerIdScope>.Parse("inventory.container.tests-shelf-b");
        private static readonly StableId<ContainerIdScope> ReceivingId =
            StableId<ContainerIdScope>.Parse("inventory.container.tests-receiving");
        private static readonly StableId<ShelfOfferIdScope> OfferId =
            StableId<ShelfOfferIdScope>.Parse("retail.offer.tests-a60-shelf-a");

        [Test]
        public void CreateRequiresCatalogAndInventory()
        {
            Fixture fixture = CreateFixture();

            Assert.That(ShelfOfferAuthority.Create(null, fixture.Inventory).Error,
                Is.EqualTo(RetailFailures.MissingCatalog));
            Assert.That(ShelfOfferAuthority.Create(fixture.Catalog, null).Error,
                Is.EqualTo(RetailFailures.MissingInventory));
        }

        [Test]
        public void ValidOfferPublishesStableIdentityAndIntegerMinorUnits()
        {
            Fixture fixture = CreateFixture();

            OperationResult result = fixture.Offers.SetOffer(
                OfferId,
                ProductId,
                ShelfId,
                "EUR",
                54_999);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(fixture.Offers.Revision, Is.EqualTo(1));
            Assert.That(fixture.Offers.Count, Is.EqualTo(1));
            Assert.That(fixture.Offers.TryGetOffer(OfferId, out ShelfOfferRecord offer), Is.True);
            Assert.That(offer.ProductId, Is.EqualTo(ProductId));
            Assert.That(offer.ShelfContainerId, Is.EqualTo(ShelfId));
            Assert.That(offer.Price.Currency.Value, Is.EqualTo("EUR"));
            Assert.That(offer.Price.MinorUnits, Is.EqualTo(54_999));
            Assert.That(offer.OfferRevision, Is.EqualTo(1));
            Assert.That(fixture.Offers.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RepeatingExactCommandIsIdempotent()
        {
            Fixture fixture = CreateFixture();
            Assert.That(PublishDefault(fixture).IsSuccess, Is.True);
            long revision = fixture.Offers.Revision;

            OperationResult repeated = PublishDefault(fixture);

            Assert.That(repeated.IsSuccess, Is.True);
            Assert.That(fixture.Offers.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Offers.Count, Is.EqualTo(1));
            Assert.That(fixture.Offers.TryGetOffer(OfferId, out ShelfOfferRecord offer), Is.True);
            Assert.That(offer.OfferRevision, Is.EqualTo(1));
        }

        [Test]
        public void ManualPriceUpdateChangesOnlyOneOfferAndOneRevision()
        {
            Fixture fixture = CreateFixture();
            Assert.That(PublishDefault(fixture).IsSuccess, Is.True);

            OperationResult update = fixture.Offers.SetOffer(
                OfferId,
                ProductId,
                ShelfId,
                "EUR",
                52_499);

            Assert.That(update.IsSuccess, Is.True);
            Assert.That(fixture.Offers.Revision, Is.EqualTo(2));
            Assert.That(fixture.Offers.Count, Is.EqualTo(1));
            Assert.That(fixture.Offers.TryGetOffer(OfferId, out ShelfOfferRecord offer), Is.True);
            Assert.That(offer.Price.MinorUnits, Is.EqualTo(52_499));
            Assert.That(offer.OfferRevision, Is.EqualTo(2));
        }

        [TestCase(null, 54999, "retail.currency.invalid")]
        [TestCase("eur", 54999, "retail.currency.invalid")]
        [TestCase("EURO", 54999, "retail.currency.invalid")]
        [TestCase("EUR", 0, "retail.price.invalid")]
        [TestCase("EUR", -1, "retail.price.invalid")]
        [TestCase("EUR", ShelfPrice.MaximumMinorUnits + 1, "retail.price.limit")]
        public void InvalidMoneyNeverMutatesAuthority(
            string currency,
            long minorUnits,
            string expectedFailure)
        {
            Fixture fixture = CreateFixture();

            OperationResult result = fixture.Offers.SetOffer(
                OfferId,
                ProductId,
                ShelfId,
                currency,
                minorUnits);

            Assert.That(result.Error.Code, Is.EqualTo(expectedFailure));
            Assert.That(fixture.Offers.Revision, Is.Zero);
            Assert.That(fixture.Offers.Count, Is.Zero);
        }

        [Test]
        public void UnknownOrNonShelfDependenciesNeverMutateAuthority()
        {
            Fixture fixture = CreateFixture();
            StableId<ProductDefinitionIdScope> unknownProduct =
                StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.unknown");
            StableId<ContainerIdScope> unknownShelf =
                StableId<ContainerIdScope>.Parse("inventory.container.unknown-shelf");

            AssertNoMutation(
                fixture,
                fixture.Offers.SetOffer(OfferId, unknownProduct, ShelfId, "EUR", 54_999),
                RetailFailures.UnknownProduct);
            AssertNoMutation(
                fixture,
                fixture.Offers.SetOffer(OfferId, ProductId, unknownShelf, "EUR", 54_999),
                RetailFailures.UnknownShelfContainer);
            AssertNoMutation(
                fixture,
                fixture.Offers.SetOffer(OfferId, ProductId, ReceivingId, "EUR", 54_999),
                RetailFailures.ContainerIsNotShelf);
        }

        [Test]
        public void OfferIdentityConflictAndDuplicateShelfProductNeverMutatePublishedState()
        {
            Fixture fixture = CreateFixture();
            Assert.That(PublishDefault(fixture).IsSuccess, Is.True);
            long revision = fixture.Offers.Revision;
            StableId<ShelfOfferIdScope> duplicateId =
                StableId<ShelfOfferIdScope>.Parse("retail.offer.tests-duplicate");

            OperationResult identityConflict = fixture.Offers.SetOffer(
                OfferId,
                SecondProductId,
                SecondShelfId,
                "EUR",
                44_999);
            OperationResult duplicatePosition = fixture.Offers.SetOffer(
                duplicateId,
                ProductId,
                ShelfId,
                "EUR",
                44_999);

            Assert.That(identityConflict.Error, Is.EqualTo(RetailFailures.OfferIdentityConflict));
            Assert.That(duplicatePosition.Error, Is.EqualTo(RetailFailures.DuplicateShelfProduct));
            Assert.That(fixture.Offers.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Offers.Count, Is.EqualTo(1));
            Assert.That(fixture.Offers.TryGetOffer(OfferId, out ShelfOfferRecord offer), Is.True);
            Assert.That(offer.Price.MinorUnits, Is.EqualTo(54_999));
        }

        [Test]
        public void QueriesAreDeterministicAndResolveShelfProductIdentity()
        {
            Fixture fixture = CreateFixture();
            StableId<ShelfOfferIdScope> laterId =
                StableId<ShelfOfferIdScope>.Parse("retail.offer.z-second");
            Assert.That(fixture.Offers.SetOffer(
                laterId,
                SecondProductId,
                SecondShelfId,
                "EUR",
                42_999).IsSuccess, Is.True);
            Assert.That(PublishDefault(fixture).IsSuccess, Is.True);

            Assert.That(fixture.Offers.GetOffers()[0].Id, Is.EqualTo(OfferId));
            Assert.That(fixture.Offers.GetOffers()[1].Id, Is.EqualTo(laterId));
            Assert.That(fixture.Offers.TryGetOfferForShelfProduct(
                ShelfId,
                ProductId,
                out ShelfOfferRecord resolved), Is.True);
            Assert.That(resolved.Id, Is.EqualTo(OfferId));
        }

        private static OperationResult PublishDefault(Fixture fixture)
        {
            return fixture.Offers.SetOffer(OfferId, ProductId, ShelfId, "EUR", 54_999);
        }

        private static void AssertNoMutation(
            Fixture fixture,
            OperationResult result,
            Failure expectedFailure)
        {
            Assert.That(result.Error, Is.EqualTo(expectedFailure));
            Assert.That(fixture.Offers.Revision, Is.Zero);
            Assert.That(fixture.Offers.Count, Is.Zero);
        }

        private static Fixture CreateFixture()
        {
            ProductDefinition first = CreateProduct(ProductId, "Test A60");
            ProductDefinition second = CreateProduct(SecondProductId, "Test B70");
            ProductCatalog catalog = ProductCatalog.Create(new[] { first, second }).Value;
            InventoryAuthority inventory = InventoryAuthority.Create(catalog).Value;
            RegisterContainer(inventory, ShelfId, InventoryContainerKind.Shelf);
            RegisterContainer(inventory, SecondShelfId, InventoryContainerKind.Shelf);
            RegisterContainer(inventory, ReceivingId, InventoryContainerKind.Receiving);
            ShelfOfferAuthority offers = ShelfOfferAuthority.Create(catalog, inventory).Value;
            return new Fixture(catalog, inventory, offers);
        }

        private static ProductDefinition CreateProduct(
            StableId<ProductDefinitionIdScope> productId,
            string name)
        {
            return ProductDefinition.Create(
                productId,
                StableId<ProductCategoryIdScope>.Parse("catalog.category.graphics-cards"),
                name,
                ProductTrackingPolicy.SerializedInstance,
                1095).Value;
        }

        private static void RegisterContainer(
            InventoryAuthority inventory,
            StableId<ContainerIdScope> containerId,
            InventoryContainerKind kind)
        {
            Assert.That(inventory.RegisterContainer(
                InventoryContainerDefinition.Create(containerId, kind, 8).Value).IsSuccess,
                Is.True);
        }

        private readonly struct Fixture
        {
            public Fixture(
                ProductCatalog catalog,
                InventoryAuthority inventory,
                ShelfOfferAuthority offers)
            {
                Catalog = catalog;
                Inventory = inventory;
                Offers = offers;
            }

            public ProductCatalog Catalog { get; }

            public InventoryAuthority Inventory { get; }

            public ShelfOfferAuthority Offers { get; }
        }
    }
}
