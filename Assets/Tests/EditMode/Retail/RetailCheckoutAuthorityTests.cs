using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Retail
{
    public sealed class RetailCheckoutAuthorityTests
    {
        private static readonly StableId<ProductDefinitionIdScope> ProductA =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.checkout-a60");
        private static readonly StableId<ProductDefinitionIdScope> ProductB =
            StableId<ProductDefinitionIdScope>.Parse("catalog.gpu.checkout-b70");
        private static readonly StableId<ContainerIdScope> Shelf =
            StableId<ContainerIdScope>.Parse("inventory.container.checkout-shelf");
        private static readonly StableId<ContainerIdScope> Receiving =
            StableId<ContainerIdScope>.Parse("inventory.container.checkout-receiving");
        private static readonly StableId<ItemInstanceIdScope> ItemA =
            StableId<ItemInstanceIdScope>.Parse("inventory.item.checkout-a60-001");
        private static readonly StableId<ItemInstanceIdScope> ItemB =
            StableId<ItemInstanceIdScope>.Parse("inventory.item.checkout-b70-001");
        private static readonly StableId<ShelfOfferIdScope> OfferA =
            StableId<ShelfOfferIdScope>.Parse("retail.offer.checkout-a60");
        private static readonly StableId<ShelfOfferIdScope> OfferB =
            StableId<ShelfOfferIdScope>.Parse("retail.offer.checkout-b70");
        private static readonly StableId<RetailBasketLineIdScope> LineA =
            StableId<RetailBasketLineIdScope>.Parse("retail.basket-line.checkout-a60");
        private static readonly StableId<RetailBasketLineIdScope> LineB =
            StableId<RetailBasketLineIdScope>.Parse("retail.basket-line.checkout-b70");
        private static readonly StableId<RetailBasketIdScope> Basket =
            StableId<RetailBasketIdScope>.Parse("retail.basket.checkout-customer");
        private static readonly StableId<RetailCustomerIdScope> Customer =
            StableId<RetailCustomerIdScope>.Parse("retail.customer.checkout-walk-in");
        private static readonly StableId<ReservationIdScope> ReservationA =
            StableId<ReservationIdScope>.Parse("inventory.reservation.checkout-a60");
        private static readonly StableId<ReservationIdScope> ReservationB =
            StableId<ReservationIdScope>.Parse("inventory.reservation.checkout-b70");
        private static readonly StableId<InventoryClaimIdScope> ClaimA =
            StableId<InventoryClaimIdScope>.Parse("inventory.claim.checkout-a60");
        private static readonly StableId<InventoryClaimIdScope> ClaimB =
            StableId<InventoryClaimIdScope>.Parse("inventory.claim.checkout-b70");
        private static readonly StableId<RetailCheckoutIdScope> Checkout =
            StableId<RetailCheckoutIdScope>.Parse("retail.checkout.customer-001");
        private static readonly StableId<RetailCheckoutCompletionIdScope> Completion =
            StableId<RetailCheckoutCompletionIdScope>.Parse(
                "retail.checkout-completion.customer-001");
        private static readonly SimulationTimestamp StartedAt =
            SimulationTimestamp.Create(10, 10_000);
        private static readonly SimulationTimestamp CompletedAt =
            SimulationTimestamp.Create(11, 11_000);

        [Test]
        public void CreateRequiresAllAuthorities()
        {
            Fixture fixture = CreateFixture();

            Assert.That(RetailCheckoutAuthority.Create(
                null,
                fixture.Baskets,
                fixture.Inventory).Error,
                Is.EqualTo(RetailCheckoutFailures.MissingOfferAuthority));
            Assert.That(RetailCheckoutAuthority.Create(
                fixture.Offers,
                null,
                fixture.Inventory).Error,
                Is.EqualTo(RetailCheckoutFailures.MissingBasketAuthority));
            Assert.That(RetailCheckoutAuthority.Create(
                fixture.Offers,
                fixture.Baskets,
                null).Error,
                Is.EqualTo(RetailCheckoutFailures.MissingInventory));
        }

        [Test]
        public void BeginFreezesExactLineAndMutatesOnlyCheckoutOnce()
        {
            Fixture fixture = CreateFixture();
            AuthorityState before = Capture(fixture);

            OperationResult result = Begin(fixture);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(1));
            Assert.That(fixture.Checkouts.Count, Is.EqualTo(1));
            AssertOtherAuthoritiesUnchanged(fixture, before);
            Assert.That(fixture.Checkouts.TryGetCheckout(
                Checkout,
                out RetailCheckoutRecord checkout), Is.True);
            Assert.That(checkout.BasketId, Is.EqualTo(Basket));
            Assert.That(checkout.CustomerId, Is.EqualTo(Customer));
            Assert.That(checkout.StartedAt, Is.EqualTo(StartedAt));
            Assert.That(checkout.Currency.Value, Is.EqualTo("EUR"));
            Assert.That(checkout.TotalMinorUnits, Is.EqualTo(54_999));
            Assert.That(checkout.Lines.Count, Is.EqualTo(1));
            Assert.That(checkout.Lines[0].BasketLineId, Is.EqualTo(LineA));
            Assert.That(checkout.Lines[0].OfferId, Is.EqualTo(OfferA));
            Assert.That(checkout.Lines[0].ItemId, Is.EqualTo(ItemA));
            Assert.That(checkout.Lines[0].InventoryReservationId,
                Is.EqualTo(ReservationA));
            Assert.That(checkout.Lines[0].UnitCost.CurrencyCode, Is.EqualTo("EUR"));
            Assert.That(checkout.Lines[0].UnitCost.MinorUnits, Is.EqualTo(42_000));
            Assert.That(checkout.Lines[0].UnitPrice.MinorUnits, Is.EqualTo(54_999));
            Assert.That(checkout.Lines[0].SourceOfferRevision, Is.EqualTo(1));
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ExactBeginRepeatIsIdempotentAcrossEveryAuthority()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            AuthorityState before = Capture(fixture);
            long checkoutRevision = fixture.Checkouts.Revision;

            OperationResult repeated = Begin(fixture);

            Assert.That(repeated.IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(fixture.Checkouts.Count, Is.EqualTo(1));
            AssertOtherAuthoritiesUnchanged(fixture, before);
        }

        [Test]
        public void LaterOfferUpdateCannotRewriteFrozenPrice()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            long checkoutRevision = fixture.Checkouts.Revision;
            Assert.That(fixture.Offers.SetOffer(
                OfferA,
                ProductA,
                Shelf,
                "EUR",
                59_999).IsSuccess, Is.True);
            long offerRevision = fixture.Offers.Revision;

            Assert.That(Begin(fixture).IsSuccess, Is.True);

            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(fixture.Offers.Revision, Is.EqualTo(offerRevision));
            Assert.That(fixture.Checkouts.TryGetCheckout(Checkout, out var checkout), Is.True);
            Assert.That(checkout.TotalMinorUnits, Is.EqualTo(54_999));
            Assert.That(checkout.Lines[0].UnitCost.MinorUnits, Is.EqualTo(42_000));
            Assert.That(checkout.Lines[0].UnitPrice.MinorUnits, Is.EqualTo(54_999));
            Assert.That(checkout.Lines[0].SourceOfferRevision, Is.EqualTo(1));
            Assert.That(fixture.Offers.TryGetOffer(OfferA, out var currentOffer), Is.True);
            Assert.That(currentOffer.Price.MinorUnits, Is.EqualTo(59_999));
            Assert.That(currentOffer.OfferRevision, Is.EqualTo(2));
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MultipleLinesAreSortedAndTotaledInOneCurrency()
        {
            Fixture fixture = CreateFixture(includeSecondLine: true);
            AuthorityState before = Capture(fixture);

            Assert.That(Begin(fixture).IsSuccess, Is.True);

            AssertOtherAuthoritiesUnchanged(fixture, before);
            Assert.That(fixture.Checkouts.TryGetCheckout(Checkout, out var checkout), Is.True);
            Assert.That(checkout.Currency.Value, Is.EqualTo("EUR"));
            Assert.That(checkout.TotalMinorUnits, Is.EqualTo(119_998));
            Assert.That(checkout.Lines.Count, Is.EqualTo(2));
            Assert.That(checkout.Lines[0].BasketLineId, Is.EqualTo(LineA));
            Assert.That(checkout.Lines[1].BasketLineId, Is.EqualTo(LineB));
            Assert.That(checkout.Lines[0].UnitCost.MinorUnits, Is.EqualTo(42_000));
            Assert.That(checkout.Lines[1].UnitCost.MinorUnits, Is.EqualTo(48_000));
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MixedCurrencyBasketFailsWithoutMutation()
        {
            Fixture fixture = CreateFixture(includeSecondLine: true, secondCurrency: "USD");
            AuthorityState before = Capture(fixture);

            OperationResult result = Begin(fixture);

            Assert.That(result.Error, Is.EqualTo(RetailCheckoutFailures.MixedCurrency));
            Assert.That(fixture.Checkouts.Revision, Is.Zero);
            Assert.That(fixture.Checkouts.Count, Is.Zero);
            AssertOtherAuthoritiesUnchanged(fixture, before);
        }

        [Test]
        public void UnitCostCurrencyMismatchFailsBeforeCheckoutMutation()
        {
            Fixture fixture = CreateFixture(firstCostCurrency: "USD");
            AuthorityState before = Capture(fixture);

            OperationResult result = Begin(fixture);

            Assert.That(result.Error,
                Is.EqualTo(RetailCheckoutFailures.CostCurrencyMismatch));
            Assert.That(fixture.Checkouts.Revision, Is.Zero);
            Assert.That(fixture.Checkouts.Count, Is.Zero);
            AssertOtherAuthoritiesUnchanged(fixture, before);
        }

        [Test]
        public void UnknownBasketAndWrongCustomerFailWithoutMutation()
        {
            Fixture fixture = CreateFixture();
            AuthorityState before = Capture(fixture);

            OperationResult unknown = fixture.Checkouts.BeginCheckout(
                Checkout,
                StableId<RetailBasketIdScope>.Parse("retail.basket.unknown"),
                Customer,
                StartedAt);
            OperationResult wrongCustomer = fixture.Checkouts.BeginCheckout(
                Checkout,
                Basket,
                StableId<RetailCustomerIdScope>.Parse("retail.customer.wrong"),
                StartedAt);

            Assert.That(unknown.Error,
                Is.EqualTo(RetailCheckoutFailures.UnknownOrEmptyBasket));
            Assert.That(wrongCustomer.Error,
                Is.EqualTo(RetailCheckoutFailures.CustomerMismatch));
            Assert.That(fixture.Checkouts.Revision, Is.Zero);
            Assert.That(fixture.Checkouts.Count, Is.Zero);
            AssertOtherAuthoritiesUnchanged(fixture, before);
        }

        [Test]
        public void ReservationOrShelfDriftFailsBeforeCheckoutMutation()
        {
            Fixture reservationFixture = CreateFixture();
            Assert.That(reservationFixture.Inventory.ReleaseReservation(ReservationA).IsSuccess,
                Is.True);
            AuthorityState reservationBefore = Capture(reservationFixture);

            OperationResult reservationDrift = Begin(reservationFixture);

            Assert.That(reservationDrift.Error,
                Is.EqualTo(RetailCheckoutFailures.InventoryReservationDrift));
            Assert.That(reservationFixture.Checkouts.Count, Is.Zero);
            AssertOtherAuthoritiesUnchanged(reservationFixture, reservationBefore);

            Fixture shelfFixture = CreateFixture();
            Assert.That(shelfFixture.Inventory.TransferSerializedItem(ItemA, Receiving).IsSuccess,
                Is.True);
            AuthorityState shelfBefore = Capture(shelfFixture);

            OperationResult shelfDrift = Begin(shelfFixture);

            Assert.That(shelfDrift.Error,
                Is.EqualTo(RetailCheckoutFailures.ItemNotOnOfferShelf));
            Assert.That(shelfFixture.Checkouts.Count, Is.Zero);
            AssertOtherAuthoritiesUnchanged(shelfFixture, shelfBefore);
        }

        [Test]
        public void TransactionIdentityAndSecondCheckoutForBasketAreRejected()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            AuthorityState before = Capture(fixture);
            long checkoutRevision = fixture.Checkouts.Revision;

            OperationResult identityConflict = fixture.Checkouts.BeginCheckout(
                Checkout,
                Basket,
                Customer,
                SimulationTimestamp.Create(11, 11_000));
            OperationResult duplicateBasket = fixture.Checkouts.BeginCheckout(
                StableId<RetailCheckoutIdScope>.Parse("retail.checkout.customer-duplicate"),
                Basket,
                Customer,
                StartedAt);

            Assert.That(identityConflict.Error,
                Is.EqualTo(RetailCheckoutFailures.CheckoutIdentityConflict));
            Assert.That(duplicateBasket.Error,
                Is.EqualTo(RetailCheckoutFailures.BasketAlreadyInCheckout));
            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(fixture.Checkouts.Count, Is.EqualTo(1));
            AssertOtherAuthoritiesUnchanged(fixture, before);
        }

        [Test]
        public void BasketChangeAfterBeginIsDetectedWithoutRewritingSnapshot()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            Assert.That(ReserveSecondLine(fixture, Basket, Customer).IsSuccess, Is.True);
            long checkoutRevision = fixture.Checkouts.Revision;
            AuthorityState before = Capture(fixture);

            OperationResult repeated = Begin(fixture);

            Assert.That(repeated.Error,
                Is.EqualTo(RetailCheckoutFailures.CheckoutBasketDrift));
            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(fixture.Checkouts.TryGetCheckout(Checkout, out var checkout), Is.True);
            Assert.That(checkout.Lines.Count, Is.EqualTo(1));
            AssertOtherAuthoritiesUnchanged(fixture, before);
            Assert.That(fixture.Checkouts.ValidateInvariants().Error,
                Is.EqualTo(RetailCheckoutFailures.InvariantViolation));
        }

        [Test]
        public void QueryOrderIsDeterministicAcrossBaskets()
        {
            Fixture fixture = CreateFixture();
            StableId<RetailBasketIdScope> secondBasket =
                StableId<RetailBasketIdScope>.Parse("retail.basket.checkout-second");
            StableId<RetailCustomerIdScope> secondCustomer =
                StableId<RetailCustomerIdScope>.Parse("retail.customer.checkout-second");
            Assert.That(ReserveSecondLine(fixture, secondBasket, secondCustomer).IsSuccess, Is.True);
            StableId<RetailCheckoutIdScope> laterId =
                StableId<RetailCheckoutIdScope>.Parse("retail.checkout.z-later");
            StableId<RetailCheckoutIdScope> earlierId =
                StableId<RetailCheckoutIdScope>.Parse("retail.checkout.a-earlier");

            Assert.That(fixture.Checkouts.BeginCheckout(
                laterId,
                Basket,
                Customer,
                StartedAt).IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.BeginCheckout(
                earlierId,
                secondBasket,
                secondCustomer,
                StartedAt).IsSuccess, Is.True);

            Assert.That(fixture.Checkouts.GetCheckouts()[0].Id, Is.EqualTo(earlierId));
            Assert.That(fixture.Checkouts.GetCheckouts()[1].Id, Is.EqualTo(laterId));
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CompletionConsumesEveryLineAtomicallyAndCreatesImmutableResult()
        {
            Fixture fixture = CreateFixture(includeSecondLine: true);
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.TryGetCheckout(
                Checkout, out RetailCheckoutRecord checkout), Is.True);
            long inventoryRevision = fixture.Inventory.Revision;
            long basketRevision = fixture.Baskets.Revision;
            long checkoutRevision = fixture.Checkouts.Revision;
            long offerRevision = fixture.Offers.Revision;

            OperationResult result = Complete(fixture);

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(basketRevision + 1));
            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(checkoutRevision + 1));
            Assert.That(fixture.Offers.Revision, Is.EqualTo(offerRevision));
            Assert.That(fixture.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(fixture.Inventory.ReservationCount, Is.Zero);
            Assert.That(fixture.Baskets.Count, Is.Zero);
            Assert.That(fixture.Checkouts.Count, Is.EqualTo(1));
            Assert.That(fixture.Checkouts.CompletionCount, Is.EqualTo(1));
            Assert.That(fixture.Checkouts.TryGetCompletion(
                Completion, out RetailCheckoutCompletionRecord completion), Is.True);
            Assert.That(completion.CheckoutId, Is.EqualTo(Checkout));
            Assert.That(completion.BasketId, Is.EqualTo(Basket));
            Assert.That(completion.CustomerId, Is.EqualTo(Customer));
            Assert.That(completion.CompletedAt, Is.EqualTo(CompletedAt));
            Assert.That(completion.Currency, Is.EqualTo(checkout.Currency));
            Assert.That(completion.TotalMinorUnits, Is.EqualTo(119_998));
            Assert.That(completion.Lines.Count, Is.EqualTo(2));
            Assert.That(completion.Lines[0].ItemId, Is.EqualTo(ItemA));
            Assert.That(completion.Lines[1].ItemId, Is.EqualTo(ItemB));
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);

            Assert.That(fixture.Offers.SetOffer(
                OfferA, ProductA, Shelf, "EUR", 59_999).IsSuccess, Is.True);
            Assert.That(checkout.TotalMinorUnits, Is.EqualTo(119_998));
            Assert.That(completion.TotalMinorUnits, Is.EqualTo(119_998));
            Assert.That(completion.Lines[0].UnitPrice.MinorUnits, Is.EqualTo(54_999));
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PrepareCheckoutCompletionDoesNotMutateAnyAuthority()
        {
            Fixture fixture = CreateFixture(includeSecondLine: true);
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            AuthorityState before = Capture(fixture);

            OperationResult<RetailCheckoutCompletionPlan> prepared =
                fixture.Checkouts.PrepareCheckoutCompletion(
                    Completion,
                    Checkout,
                    CompletedAt);

            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(prepared.Value.IsReplay, Is.False);
            Assert.That(prepared.Value.ExpectedRevision,
                Is.EqualTo(before.CheckoutRevision));
            Assert.That(prepared.Value.Completion.Id, Is.EqualTo(Completion));
            AssertAllAuthoritiesUnchanged(fixture, before);
        }

        [Test]
        public void ForeignCompletionPlanIsRejectedWithoutMutatingEitherAuthorityGraph()
        {
            Fixture ownerFixture = CreateFixture();
            Fixture foreignFixture = CreateFixture();
            Assert.That(Begin(ownerFixture).IsSuccess, Is.True);
            Assert.That(Begin(foreignFixture).IsSuccess, Is.True);
            OperationResult<RetailCheckoutCompletionPlan> prepared =
                ownerFixture.Checkouts.PrepareCheckoutCompletion(
                    Completion,
                    Checkout,
                    CompletedAt);
            Assert.That(prepared.IsSuccess, Is.True);
            AuthorityState ownerBefore = Capture(ownerFixture);
            AuthorityState foreignBefore = Capture(foreignFixture);

            OperationResult result = foreignFixture.Checkouts
                .CommitPreparedCheckoutCompletion(prepared.Value);

            Assert.That(result.Error,
                Is.EqualTo(RetailCheckoutFailures.CompletionPlanInvalid));
            AssertAllAuthoritiesUnchanged(ownerFixture, ownerBefore);
            AssertAllAuthoritiesUnchanged(foreignFixture, foreignBefore);
        }

        [Test]
        public void CheckoutRevisionDriftRejectsPreparedCompletionWithoutFurtherMutation()
        {
            Fixture fixture = CreateFixture();
            StableId<RetailBasketIdScope> secondBasket =
                StableId<RetailBasketIdScope>.Parse("retail.basket.checkout-stale");
            StableId<RetailCustomerIdScope> secondCustomer =
                StableId<RetailCustomerIdScope>.Parse("retail.customer.checkout-stale");
            StableId<RetailCheckoutIdScope> secondCheckout =
                StableId<RetailCheckoutIdScope>.Parse("retail.checkout.stale-trigger");
            Assert.That(ReserveSecondLine(
                fixture,
                secondBasket,
                secondCustomer).IsSuccess, Is.True);
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            OperationResult<RetailCheckoutCompletionPlan> prepared =
                fixture.Checkouts.PrepareCheckoutCompletion(
                    Completion,
                    Checkout,
                    CompletedAt);
            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.BeginCheckout(
                secondCheckout,
                secondBasket,
                secondCustomer,
                StartedAt).IsSuccess, Is.True);
            AuthorityState beforeCommit = Capture(fixture);

            OperationResult result = fixture.Checkouts
                .CommitPreparedCheckoutCompletion(prepared.Value);

            Assert.That(result.Error,
                Is.EqualTo(RetailCheckoutFailures.CompletionPlanStale));
            AssertAllAuthoritiesUnchanged(fixture, beforeCommit);
        }

        [Test]
        public void BasketRevisionDriftRejectsPreparedCompletionBeforeInventoryMutation()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            OperationResult<RetailCheckoutCompletionPlan> prepared =
                fixture.Checkouts.PrepareCheckoutCompletion(
                    Completion,
                    Checkout,
                    CompletedAt);
            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(ReserveSecondLine(
                fixture,
                StableId<RetailBasketIdScope>.Parse("retail.basket.basket-stale"),
                StableId<RetailCustomerIdScope>.Parse(
                    "retail.customer.basket-stale")).IsSuccess, Is.True);
            AuthorityState beforeCommit = Capture(fixture);

            OperationResult result = fixture.Checkouts
                .CommitPreparedCheckoutCompletion(prepared.Value);

            Assert.That(result.Error,
                Is.EqualTo(RetailBasketFailures.CheckoutPlanStale));
            AssertAllAuthoritiesUnchanged(fixture, beforeCommit);
        }

        [Test]
        public void InventoryRevisionDriftRejectsPreparedCompletionBeforeBasketOrCheckoutMutation()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            OperationResult<RetailCheckoutCompletionPlan> prepared =
                fixture.Checkouts.PrepareCheckoutCompletion(
                    Completion,
                    Checkout,
                    CompletedAt);
            Assert.That(prepared.IsSuccess, Is.True);
            RegisterContainer(
                fixture.Inventory,
                StableId<ContainerIdScope>.Parse(
                    "inventory.container.checkout-stale-trigger"),
                InventoryContainerKind.Storage);
            AuthorityState beforeCommit = Capture(fixture);

            OperationResult result = fixture.Checkouts
                .CommitPreparedCheckoutCompletion(prepared.Value);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.CheckoutConsumptionPlanStale));
            AssertAllAuthoritiesUnchanged(fixture, beforeCommit);
        }

        [Test]
        public void PreparedCompletionCommitsEachAuthorityOnceAndExactReplayIsIdempotent()
        {
            Fixture fixture = CreateFixture(includeSecondLine: true);
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            OperationResult<RetailCheckoutCompletionPlan> prepared =
                fixture.Checkouts.PrepareCheckoutCompletion(
                    Completion,
                    Checkout,
                    CompletedAt);
            Assert.That(prepared.IsSuccess, Is.True);
            AuthorityState beforeCommit = Capture(fixture);

            OperationResult committed = fixture.Checkouts
                .CommitPreparedCheckoutCompletion(prepared.Value);

            Assert.That(committed.IsSuccess, Is.True);
            Assert.That(fixture.Inventory.Revision,
                Is.EqualTo(beforeCommit.InventoryRevision + 1));
            Assert.That(fixture.Baskets.Revision,
                Is.EqualTo(beforeCommit.BasketRevision + 1));
            Assert.That(fixture.Checkouts.Revision,
                Is.EqualTo(beforeCommit.CheckoutRevision + 1));
            Assert.That(fixture.Offers.Revision,
                Is.EqualTo(beforeCommit.OfferRevision));
            Assert.That(fixture.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(fixture.Inventory.ReservationCount, Is.Zero);
            Assert.That(fixture.Baskets.Count, Is.Zero);
            Assert.That(fixture.Checkouts.CompletionCount, Is.EqualTo(1));
            AuthorityState committedState = Capture(fixture);

            OperationResult<RetailCheckoutCompletionPlan> replay =
                fixture.Checkouts.PrepareCheckoutCompletion(
                    Completion,
                    Checkout,
                    CompletedAt);
            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value.IsReplay, Is.True);
            AssertAllAuthoritiesUnchanged(fixture, committedState);

            OperationResult replayCommit = fixture.Checkouts
                .CommitPreparedCheckoutCompletion(replay.Value);

            Assert.That(replayCommit.IsSuccess, Is.True);
            AssertAllAuthoritiesUnchanged(fixture, committedState);

            OperationResult originalPlanAgain = fixture.Checkouts
                .CommitPreparedCheckoutCompletion(prepared.Value);
            Assert.That(originalPlanAgain.Error,
                Is.EqualTo(RetailCheckoutFailures.CompletionPlanStale));
            AssertAllAuthoritiesUnchanged(fixture, committedState);
        }

        [Test]
        public void ExactCompletionAndBeginRepeatsAreIdempotentAfterFulfillment()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            Assert.That(Complete(fixture).IsSuccess, Is.True);
            AuthorityState before = Capture(fixture);
            long checkoutRevision = fixture.Checkouts.Revision;
            int serializedItemCount = fixture.Inventory.SerializedItemCount;

            OperationResult repeatedCompletion = Complete(fixture);
            OperationResult repeatedBegin = Begin(fixture);

            Assert.That(repeatedCompletion.IsSuccess, Is.True);
            Assert.That(repeatedBegin.IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(fixture.Checkouts.CompletionCount, Is.EqualTo(1));
            Assert.That(fixture.Inventory.SerializedItemCount, Is.EqualTo(serializedItemCount));
            AssertOtherAuthoritiesUnchanged(fixture, before);
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CompletionIdentityConflictsFailWithoutMutation()
        {
            Fixture fixture = CreateFixture();
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            Assert.That(Complete(fixture).IsSuccess, Is.True);
            AuthorityState before = Capture(fixture);
            long checkoutRevision = fixture.Checkouts.Revision;

            OperationResult reusedCompletionId = fixture.Checkouts.CompleteCheckout(
                Completion,
                StableId<RetailCheckoutIdScope>.Parse("retail.checkout.other"),
                CompletedAt);
            OperationResult secondCompletion = fixture.Checkouts.CompleteCheckout(
                StableId<RetailCheckoutCompletionIdScope>.Parse(
                    "retail.checkout-completion.second"),
                Checkout,
                CompletedAt);

            Assert.That(reusedCompletionId.Error,
                Is.EqualTo(RetailCheckoutFailures.CompletionIdentityConflict));
            Assert.That(secondCompletion.Error,
                Is.EqualTo(RetailCheckoutFailures.CheckoutAlreadyCompleted));
            Assert.That(fixture.Checkouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(fixture.Checkouts.CompletionCount, Is.EqualTo(1));
            AssertOtherAuthoritiesUnchanged(fixture, before);
        }

        [Test]
        public void CompletionTimeAndReservationDriftFailWithoutCrossAuthorityMutation()
        {
            Fixture timeFixture = CreateFixture();
            Assert.That(Begin(timeFixture).IsSuccess, Is.True);
            AuthorityState timeBefore = Capture(timeFixture);
            OperationResult early = timeFixture.Checkouts.CompleteCheckout(
                Completion,
                Checkout,
                SimulationTimestamp.Create(9, 9_000));
            Assert.That(early.Error,
                Is.EqualTo(RetailCheckoutFailures.CompletionBeforeCheckout));
            Assert.That(timeFixture.Checkouts.CompletionCount, Is.Zero);
            AssertOtherAuthoritiesUnchanged(timeFixture, timeBefore);

            Fixture driftFixture = CreateFixture();
            Assert.That(Begin(driftFixture).IsSuccess, Is.True);
            Assert.That(driftFixture.Inventory.ReleaseReservation(ReservationA).IsSuccess,
                Is.True);
            AuthorityState driftBefore = Capture(driftFixture);
            long checkoutRevision = driftFixture.Checkouts.Revision;

            OperationResult drifted = Complete(driftFixture);

            Assert.That(drifted.Error,
                Is.EqualTo(RetailCheckoutFailures.InventoryReservationDrift));
            Assert.That(driftFixture.Checkouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(driftFixture.Checkouts.CompletionCount, Is.Zero);
            AssertOtherAuthoritiesUnchanged(driftFixture, driftBefore);
        }

        [Test]
        public void CompletionQueriesUseDeterministicStableIdOrder()
        {
            Fixture fixture = CreateFixture();
            StableId<RetailBasketIdScope> secondBasket =
                StableId<RetailBasketIdScope>.Parse("retail.basket.checkout-second");
            StableId<RetailCustomerIdScope> secondCustomer =
                StableId<RetailCustomerIdScope>.Parse("retail.customer.checkout-second");
            StableId<RetailCheckoutIdScope> secondCheckout =
                StableId<RetailCheckoutIdScope>.Parse("retail.checkout.second");
            StableId<RetailCheckoutCompletionIdScope> laterCompletion =
                StableId<RetailCheckoutCompletionIdScope>.Parse(
                    "retail.checkout-completion.z-later");
            StableId<RetailCheckoutCompletionIdScope> earlierCompletion =
                StableId<RetailCheckoutCompletionIdScope>.Parse(
                    "retail.checkout-completion.a-earlier");
            Assert.That(ReserveSecondLine(fixture, secondBasket, secondCustomer).IsSuccess,
                Is.True);
            Assert.That(Begin(fixture).IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.BeginCheckout(
                secondCheckout, secondBasket, secondCustomer, StartedAt).IsSuccess, Is.True);

            Assert.That(fixture.Checkouts.CompleteCheckout(
                laterCompletion, Checkout, CompletedAt).IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.CompleteCheckout(
                earlierCompletion, secondCheckout, CompletedAt).IsSuccess, Is.True);

            Assert.That(fixture.Checkouts.GetCompletions()[0].Id,
                Is.EqualTo(earlierCompletion));
            Assert.That(fixture.Checkouts.GetCompletions()[1].Id,
                Is.EqualTo(laterCompletion));
            Assert.That(fixture.Checkouts.TryGetCompletionForCheckout(
                secondCheckout, out RetailCheckoutCompletionRecord completion), Is.True);
            Assert.That(completion.Id, Is.EqualTo(earlierCompletion));
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        private static OperationResult Begin(Fixture fixture)
        {
            return fixture.Checkouts.BeginCheckout(Checkout, Basket, Customer, StartedAt);
        }

        private static OperationResult Complete(Fixture fixture)
        {
            return fixture.Checkouts.CompleteCheckout(
                Completion,
                Checkout,
                CompletedAt);
        }

        private static OperationResult ReserveSecondLine(
            Fixture fixture,
            StableId<RetailBasketIdScope> basketId,
            StableId<RetailCustomerIdScope> customerId)
        {
            return fixture.Baskets.ReserveSerializedOffer(
                LineB,
                basketId,
                customerId,
                OfferB,
                ItemB,
                ReservationB,
                ClaimB);
        }

        private static AuthorityState Capture(Fixture fixture)
        {
            return new AuthorityState(
                fixture.Inventory.Revision,
                fixture.Inventory.ContainerCount,
                fixture.Inventory.SerializedItemCount,
                fixture.Inventory.BatchCount,
                fixture.Inventory.ReservationCount,
                fixture.Inventory.GetContainers(),
                fixture.Inventory.GetSerializedItems(),
                fixture.Inventory.GetReservations(),
                fixture.Baskets.Revision,
                fixture.Baskets.Count,
                fixture.Baskets.GetLines(),
                fixture.Offers.Revision,
                fixture.Offers.Count,
                fixture.Offers.GetOffers(),
                fixture.Checkouts.Revision,
                fixture.Checkouts.Count,
                fixture.Checkouts.CompletionCount,
                fixture.Checkouts.GetCheckouts(),
                fixture.Checkouts.GetCompletions());
        }

        private static void AssertOtherAuthoritiesUnchanged(
            Fixture fixture,
            AuthorityState expected)
        {
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(expected.InventoryRevision));
            Assert.That(fixture.Inventory.ReservationCount,
                Is.EqualTo(expected.InventoryReservationCount));
            Assert.That(fixture.Baskets.Revision, Is.EqualTo(expected.BasketRevision));
            Assert.That(fixture.Baskets.Count, Is.EqualTo(expected.BasketCount));
            Assert.That(fixture.Offers.Revision, Is.EqualTo(expected.OfferRevision));
            Assert.That(fixture.Offers.Count, Is.EqualTo(expected.OfferCount));
        }

        private static void AssertAllAuthoritiesUnchanged(
            Fixture fixture,
            AuthorityState expected)
        {
            AssertOtherAuthoritiesUnchanged(fixture, expected);
            Assert.That(fixture.Inventory.ContainerCount,
                Is.EqualTo(expected.InventoryContainerCount));
            Assert.That(fixture.Inventory.SerializedItemCount,
                Is.EqualTo(expected.InventorySerializedItemCount));
            Assert.That(fixture.Inventory.BatchCount,
                Is.EqualTo(expected.InventoryBatchCount));
            AssertSameReferences(
                fixture.Inventory.GetContainers(),
                expected.InventoryContainers);
            AssertSameReferences(
                fixture.Inventory.GetSerializedItems(),
                expected.InventoryItems);
            AssertSameReferences(
                fixture.Inventory.GetReservations(),
                expected.InventoryReservations);
            AssertSameReferences(fixture.Baskets.GetLines(), expected.BasketLines);
            AssertSameReferences(fixture.Offers.GetOffers(), expected.OfferRecords);
            Assert.That(fixture.Checkouts.Revision,
                Is.EqualTo(expected.CheckoutRevision));
            Assert.That(fixture.Checkouts.Count,
                Is.EqualTo(expected.CheckoutCount));
            Assert.That(fixture.Checkouts.CompletionCount,
                Is.EqualTo(expected.CheckoutCompletionCount));
            AssertSameReferences(
                fixture.Checkouts.GetCheckouts(),
                expected.CheckoutRecords);
            AssertSameReferences(
                fixture.Checkouts.GetCompletions(),
                expected.CheckoutCompletions);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Baskets.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Offers.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Checkouts.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertSameReferences<T>(
            IReadOnlyList<T> actual,
            IReadOnlyList<T> expected)
            where T : class
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (int index = 0; index < actual.Count; index++)
            {
                Assert.That(object.ReferenceEquals(actual[index], expected[index]), Is.True);
            }
        }

        private static Fixture CreateFixture(
            bool includeSecondLine = false,
            string secondCurrency = "EUR",
            string firstCostCurrency = "EUR")
        {
            ProductDefinition productA = CreateProduct(ProductA, "Checkout A60");
            ProductDefinition productB = CreateProduct(ProductB, "Checkout B70");
            ProductCatalog catalog = ProductCatalog.Create(new[] { productA, productB }).Value;
            InventoryAuthority inventory = InventoryAuthority.Create(catalog).Value;
            RegisterContainer(inventory, Shelf, InventoryContainerKind.Shelf);
            RegisterContainer(inventory, Receiving, InventoryContainerKind.Receiving);
            Assert.That(inventory.ReceiveSerializedItem(
                ItemA,
                ProductA,
                Shelf,
                InventoryCondition.New,
                InventoryUnitCost.Create(firstCostCurrency, 42_000).Value).IsSuccess, Is.True);
            Assert.That(inventory.ReceiveSerializedItem(
                ItemB,
                ProductB,
                Shelf,
                InventoryCondition.New,
                InventoryUnitCost.Create(secondCurrency, 48_000).Value).IsSuccess, Is.True);

            ShelfOfferAuthority offers = ShelfOfferAuthority.Create(catalog, inventory).Value;
            Assert.That(offers.SetOffer(OfferA, ProductA, Shelf, "EUR", 54_999).IsSuccess,
                Is.True);
            Assert.That(offers.SetOffer(OfferB, ProductB, Shelf, secondCurrency, 64_999).IsSuccess,
                Is.True);
            RetailBasketAuthority baskets = RetailBasketAuthority.Create(offers, inventory).Value;
            Assert.That(baskets.ReserveSerializedOffer(
                LineA,
                Basket,
                Customer,
                OfferA,
                ItemA,
                ReservationA,
                ClaimA).IsSuccess, Is.True);
            var fixture = new Fixture(
                inventory,
                offers,
                baskets,
                RetailCheckoutAuthority.Create(offers, baskets, inventory).Value);
            if (includeSecondLine)
            {
                Assert.That(ReserveSecondLine(fixture, Basket, Customer).IsSuccess, Is.True);
            }

            return fixture;
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
            StableId<ContainerIdScope> id,
            InventoryContainerKind kind)
        {
            Assert.That(inventory.RegisterContainer(
                InventoryContainerDefinition.Create(id, kind, 8).Value).IsSuccess, Is.True);
        }

        private sealed class AuthorityState
        {
            public AuthorityState(
                long inventoryRevision,
                int inventoryContainerCount,
                int inventorySerializedItemCount,
                int inventoryBatchCount,
                int inventoryReservationCount,
                IReadOnlyList<InventoryContainerDefinition> inventoryContainers,
                IReadOnlyList<InventoryItemRecord> inventoryItems,
                IReadOnlyList<InventoryReservation> inventoryReservations,
                long basketRevision,
                int basketCount,
                IReadOnlyList<RetailBasketLineRecord> basketLines,
                long offerRevision,
                int offerCount,
                IReadOnlyList<ShelfOfferRecord> offerRecords,
                long checkoutRevision,
                int checkoutCount,
                int checkoutCompletionCount,
                IReadOnlyList<RetailCheckoutRecord> checkoutRecords,
                IReadOnlyList<RetailCheckoutCompletionRecord> checkoutCompletions)
            {
                InventoryRevision = inventoryRevision;
                InventoryContainerCount = inventoryContainerCount;
                InventorySerializedItemCount = inventorySerializedItemCount;
                InventoryBatchCount = inventoryBatchCount;
                InventoryReservationCount = inventoryReservationCount;
                InventoryContainers = inventoryContainers;
                InventoryItems = inventoryItems;
                InventoryReservations = inventoryReservations;
                BasketRevision = basketRevision;
                BasketCount = basketCount;
                BasketLines = basketLines;
                OfferRevision = offerRevision;
                OfferCount = offerCount;
                OfferRecords = offerRecords;
                CheckoutRevision = checkoutRevision;
                CheckoutCount = checkoutCount;
                CheckoutCompletionCount = checkoutCompletionCount;
                CheckoutRecords = checkoutRecords;
                CheckoutCompletions = checkoutCompletions;
            }

            public long InventoryRevision { get; }

            public int InventoryContainerCount { get; }

            public int InventorySerializedItemCount { get; }

            public int InventoryBatchCount { get; }

            public int InventoryReservationCount { get; }

            public IReadOnlyList<InventoryContainerDefinition> InventoryContainers { get; }

            public IReadOnlyList<InventoryItemRecord> InventoryItems { get; }

            public IReadOnlyList<InventoryReservation> InventoryReservations { get; }

            public long BasketRevision { get; }

            public int BasketCount { get; }

            public IReadOnlyList<RetailBasketLineRecord> BasketLines { get; }

            public long OfferRevision { get; }

            public int OfferCount { get; }

            public IReadOnlyList<ShelfOfferRecord> OfferRecords { get; }

            public long CheckoutRevision { get; }

            public int CheckoutCount { get; }

            public int CheckoutCompletionCount { get; }

            public IReadOnlyList<RetailCheckoutRecord> CheckoutRecords { get; }

            public IReadOnlyList<RetailCheckoutCompletionRecord> CheckoutCompletions { get; }
        }

        private readonly struct Fixture
        {
            public Fixture(
                InventoryAuthority inventory,
                ShelfOfferAuthority offers,
                RetailBasketAuthority baskets,
                RetailCheckoutAuthority checkouts)
            {
                Inventory = inventory;
                Offers = offers;
                Baskets = baskets;
                Checkouts = checkouts;
            }

            public InventoryAuthority Inventory { get; }

            public ShelfOfferAuthority Offers { get; }

            public RetailBasketAuthority Baskets { get; }

            public RetailCheckoutAuthority Checkouts { get; }
        }
    }
}
