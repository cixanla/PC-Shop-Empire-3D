using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Retail
{
    public sealed class RetailBasketAuthorityTests
    {
        private static readonly StableId<ProductDefinitionIdScope> ProductId =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.basket-a60");
        private static readonly StableId<ProductDefinitionIdScope> OtherProductId =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.basket-b70");
        private static readonly StableId<ContainerIdScope> ShelfId =
            StableId<ContainerIdScope>.Parse("inventory.container.basket-shelf-a");
        private static readonly StableId<ContainerIdScope> ReceivingId =
            StableId<ContainerIdScope>.Parse("inventory.container.basket-receiving");
        private static readonly StableId<ItemInstanceIdScope> ItemId =
            StableId<ItemInstanceIdScope>.Parse("inventory.item.basket-a60-001");
        private static readonly StableId<ShelfOfferIdScope> OfferId =
            StableId<ShelfOfferIdScope>.Parse("retail.offer.basket-a60");
        private static readonly StableId<RetailBasketLineIdScope> LineId =
            StableId<RetailBasketLineIdScope>.Parse("retail.basket-line.a60-001");
        private static readonly StableId<RetailBasketIdScope> BasketId =
            StableId<RetailBasketIdScope>.Parse("retail.basket.customer-001");
        private static readonly StableId<RetailCustomerIdScope> CustomerId =
            StableId<RetailCustomerIdScope>.Parse("retail.customer.walk-in-001");
        private static readonly StableId<ReservationIdScope> ReservationId =
            StableId<ReservationIdScope>.Parse("inventory.reservation.basket-a60-001");
        private static readonly StableId<InventoryClaimIdScope> ClaimId =
            StableId<InventoryClaimIdScope>.Parse("inventory.claim.basket-customer-001");

        [Test]
        public void CreateRequiresOfferAndInventoryAuthorities()
        {
            Fixture fixture = CreateFixture();

            Assert.That(RetailBasketAuthority.Create(null, fixture.Inventory).Error,
                Is.EqualTo(RetailBasketFailures.MissingOfferAuthority));
            Assert.That(RetailBasketAuthority.Create(fixture.Offers, null).Error,
                Is.EqualTo(RetailBasketFailures.MissingInventory));
        }

        [Test]
        public void ReservingExactShelfItemMutatesBothAuthoritiesExactlyOnce()
        {
            Fixture fixture = CreateFixture();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = ReserveDefault(fixture);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(1));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Baskets.Count, Is.EqualTo(1));
            Assert.That(fixture.Inventory.GetAvailableQuantity(ProductId).Value, Is.Zero);
            Assert.That(fixture.Inventory.GetTotalQuantity(ProductId).Value, Is.EqualTo(1));
            Assert.That(fixture.Baskets.TryGetLine(LineId, out RetailBasketLineRecord line), Is.True);
            Assert.That(line.BasketId, Is.EqualTo(BasketId));
            Assert.That(line.CustomerId, Is.EqualTo(CustomerId));
            Assert.That(line.OfferId, Is.EqualTo(OfferId));
            Assert.That(line.ItemId, Is.EqualTo(ItemId));
            Assert.That(fixture.Inventory.TryGetReservation(
                ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(ItemId));
            Assert.That(reservation.ClaimId, Is.EqualTo(ClaimId));
            Assert.That(fixture.Baskets.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RepeatingExactSelectionIsIdempotentAcrossBothAuthorities()
        {
            Fixture fixture = CreateFixture();
            Assert.That(ReserveDefault(fixture).IsSuccess, Is.True);
            long basketRevision = fixture.Baskets.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult repeated = ReserveDefault(fixture);

            Assert.That(repeated.IsSuccess, Is.True);
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Baskets.Count, Is.EqualTo(1));
            Assert.That(fixture.Inventory.ReservationCount, Is.EqualTo(1));
        }

        [Test]
        public void SecondBasketCannotReserveSameSerializedItem()
        {
            Fixture fixture = CreateFixture();
            Assert.That(ReserveDefault(fixture).IsSuccess, Is.True);
            long basketRevision = fixture.Baskets.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult duplicate = fixture.Baskets.ReserveSerializedOffer(
                StableId<RetailBasketLineIdScope>.Parse("retail.basket-line.a60-duplicate"),
                StableId<RetailBasketIdScope>.Parse("retail.basket.customer-002"),
                StableId<RetailCustomerIdScope>.Parse("retail.customer.walk-in-002"),
                OfferId,
                ItemId,
                StableId<ReservationIdScope>.Parse("inventory.reservation.basket-a60-002"),
                StableId<InventoryClaimIdScope>.Parse("inventory.claim.basket-customer-002"));

            Assert.That(duplicate.Error, Is.EqualTo(RetailBasketFailures.ItemAlreadyInBasket));
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Baskets.Count, Is.EqualTo(1));
            Assert.That(fixture.Inventory.ReservationCount, Is.EqualTo(1));
        }

        [Test]
        public void UnknownOfferOrItemLeavesBothAuthoritiesUntouched()
        {
            Fixture fixture = CreateFixture();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult unknownOffer = fixture.Baskets.ReserveSerializedOffer(
                LineId,
                BasketId,
                CustomerId,
                StableId<ShelfOfferIdScope>.Parse("retail.offer.unknown"),
                ItemId,
                ReservationId,
                ClaimId);
            OperationResult unknownItem = fixture.Baskets.ReserveSerializedOffer(
                LineId,
                BasketId,
                CustomerId,
                OfferId,
                StableId<ItemInstanceIdScope>.Parse("inventory.item.unknown"),
                ReservationId,
                ClaimId);

            Assert.That(unknownOffer.Error, Is.EqualTo(RetailBasketFailures.UnknownOffer));
            Assert.That(unknownItem.Error, Is.EqualTo(RetailBasketFailures.UnknownItem));
            AssertNoMutation(fixture, inventoryRevision);
        }

        [Test]
        public void ItemMustMatchOfferProductAndShelf()
        {
            Fixture fixture = CreateFixture(includeOtherProductItem: true);
            long inventoryRevision = fixture.Inventory.Revision;
            StableId<ItemInstanceIdScope> otherItemId =
                StableId<ItemInstanceIdScope>.Parse("inventory.item.basket-b70-001");

            OperationResult productMismatch = fixture.Baskets.ReserveSerializedOffer(
                LineId,
                BasketId,
                CustomerId,
                OfferId,
                otherItemId,
                ReservationId,
                ClaimId);
            Assert.That(productMismatch.Error,
                Is.EqualTo(RetailBasketFailures.OfferProductMismatch));
            AssertNoMutation(fixture, inventoryRevision);

            Assert.That(fixture.Inventory.TransferSerializedItem(ItemId, ReceivingId).IsSuccess,
                Is.True);
            inventoryRevision = fixture.Inventory.Revision;
            OperationResult shelfMismatch = ReserveDefault(fixture);
            Assert.That(shelfMismatch.Error,
                Is.EqualTo(RetailBasketFailures.ItemNotOnOfferShelf));
            AssertNoMutation(fixture, inventoryRevision);
        }

        [Test]
        public void ExistingInventoryReservationFailureDoesNotCreateBasketLine()
        {
            Fixture fixture = CreateFixture();
            StableId<ReservationIdScope> externalReservation =
                StableId<ReservationIdScope>.Parse("inventory.reservation.external-a60");
            Assert.That(fixture.Inventory.ReserveSerializedItem(
                externalReservation,
                StableId<InventoryClaimIdScope>.Parse("inventory.claim.external-a60"),
                ItemId).IsSuccess, Is.True);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = ReserveDefault(fixture);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ItemAlreadyReserved));
            Assert.That(fixture.Baskets.Revision, Is.Zero);
            Assert.That(fixture.Baskets.Count, Is.Zero);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Inventory.ReservationCount, Is.EqualTo(1));
        }

        [Test]
        public void ReleaseRestoresAvailabilityAndAdvancesBothAuthoritiesOnce()
        {
            Fixture fixture = CreateFixture();
            Assert.That(ReserveDefault(fixture).IsSuccess, Is.True);
            long basketRevision = fixture.Baskets.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult release = fixture.Baskets.ReleaseLine(LineId);

            Assert.That(release.IsSuccess, Is.True);
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(basketRevision + 1));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Baskets.Count, Is.Zero);
            Assert.That(fixture.Inventory.ReservationCount, Is.Zero);
            Assert.That(fixture.Inventory.GetAvailableQuantity(ProductId).Value, Is.EqualTo(1));
            Assert.That(fixture.Inventory.GetTotalQuantity(ProductId).Value, Is.EqualTo(1));
            Assert.That(fixture.Baskets.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReservationDriftMakesReleaseFailWithoutFurtherMutation()
        {
            Fixture fixture = CreateFixture();
            Assert.That(ReserveDefault(fixture).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ReleaseReservation(ReservationId).IsSuccess, Is.True);
            long basketRevision = fixture.Baskets.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult release = fixture.Baskets.ReleaseLine(LineId);

            Assert.That(release.Error,
                Is.EqualTo(RetailBasketFailures.InventoryReservationDrift));
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Baskets.Count, Is.EqualTo(1));
            Assert.That(fixture.Baskets.ValidateInvariants().Error,
                Is.EqualTo(RetailBasketFailures.InvariantViolation));
        }

        [Test]
        public void IdempotentRepeatDetectsCrossAuthorityDrift()
        {
            Fixture fixture = CreateFixture();
            Assert.That(ReserveDefault(fixture).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ReleaseReservation(ReservationId).IsSuccess, Is.True);
            long basketRevision = fixture.Baskets.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult repeated = ReserveDefault(fixture);

            Assert.That(repeated.Error,
                Is.EqualTo(RetailBasketFailures.InventoryReservationDrift));
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Baskets.Count, Is.EqualTo(1));
        }

        [Test]
        public void OneBasketIdentityCannotBelongToTwoCustomers()
        {
            Fixture fixture = CreateFixture(includeOtherProductItem: true);
            Assert.That(ReserveDefault(fixture).IsSuccess, Is.True);
            long basketRevision = fixture.Baskets.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = fixture.Baskets.ReserveSerializedOffer(
                StableId<RetailBasketLineIdScope>.Parse("retail.basket-line.b70-001"),
                BasketId,
                StableId<RetailCustomerIdScope>.Parse("retail.customer.walk-in-002"),
                StableId<ShelfOfferIdScope>.Parse("retail.offer.basket-b70"),
                StableId<ItemInstanceIdScope>.Parse("inventory.item.basket-b70-001"),
                StableId<ReservationIdScope>.Parse("inventory.reservation.basket-b70-001"),
                StableId<InventoryClaimIdScope>.Parse("inventory.claim.basket-customer-002"));

            Assert.That(result.Error,
                Is.EqualTo(RetailBasketFailures.BasketCustomerConflict));
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Baskets.Count, Is.EqualTo(1));
            Assert.That(fixture.Inventory.ReservationCount, Is.EqualTo(1));
        }

        private static OperationResult ReserveDefault(Fixture fixture)
        {
            return fixture.Baskets.ReserveSerializedOffer(
                LineId,
                BasketId,
                CustomerId,
                OfferId,
                ItemId,
                ReservationId,
                ClaimId);
        }

        private static void AssertNoMutation(Fixture fixture, long expectedInventoryRevision)
        {
            Assert.That(fixture.Baskets.Revision, Is.Zero);
            Assert.That(fixture.Baskets.Count, Is.Zero);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(expectedInventoryRevision));
            Assert.That(fixture.Inventory.ReservationCount, Is.Zero);
        }

        private static Fixture CreateFixture(bool includeOtherProductItem = false)
        {
            ProductDefinition product = CreateProduct(ProductId, "Basket A60");
            ProductDefinition otherProduct = CreateProduct(OtherProductId, "Basket B70");
            ProductCatalog catalog = ProductCatalog.Create(new[] { product, otherProduct }).Value;
            InventoryAuthority inventory = InventoryAuthority.Create(catalog).Value;
            RegisterContainer(inventory, ShelfId, InventoryContainerKind.Shelf);
            RegisterContainer(inventory, ReceivingId, InventoryContainerKind.Receiving);
            Assert.That(inventory.ReceiveSerializedItem(
                ItemId,
                ProductId,
                ShelfId,
                InventoryCondition.New).IsSuccess, Is.True);
            if (includeOtherProductItem)
            {
                Assert.That(inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse("inventory.item.basket-b70-001"),
                    OtherProductId,
                    ShelfId,
                    InventoryCondition.New).IsSuccess, Is.True);
            }

            ShelfOfferAuthority offers = ShelfOfferAuthority.Create(catalog, inventory).Value;
            Assert.That(offers.SetOffer(OfferId, ProductId, ShelfId, "EUR", 54_999).IsSuccess,
                Is.True);
            if (includeOtherProductItem)
            {
                Assert.That(offers.SetOffer(
                    StableId<ShelfOfferIdScope>.Parse("retail.offer.basket-b70"),
                    OtherProductId,
                    ShelfId,
                    "EUR",
                    64_999).IsSuccess,
                    Is.True);
            }
            RetailBasketAuthority baskets = RetailBasketAuthority.Create(offers, inventory).Value;
            return new Fixture(inventory, offers, baskets);
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
                InventoryAuthority inventory,
                ShelfOfferAuthority offers,
                RetailBasketAuthority baskets)
            {
                Inventory = inventory;
                Offers = offers;
                Baskets = baskets;
            }

            public InventoryAuthority Inventory { get; }

            public ShelfOfferAuthority Offers { get; }

            public RetailBasketAuthority Baskets { get; }
        }
    }
}
