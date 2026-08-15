using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Economy;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Tests.EditMode.Economy
{
    public sealed class CheckoutSettlementAuthorityTests
    {
        private static readonly SimulationTimestamp PaidAt =
            SimulationTimestamp.Create(7, 7_000);

        [Test]
        public void ExactCashSettlementAtomicallyConsumesStockAndPostsBalancedSale()
        {
            GarageStockFlowSession session = CreateReadyCheckout();
            AuthorityState before = AuthorityState.Capture(session);

            OperationResult result = session.SettlePrototypeCashCheckout();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(before.InventoryRevision + 1));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(before.BasketRevision + 1));
            Assert.That(session.RetailCheckouts.Revision,
                Is.EqualTo(before.CheckoutRevision + 1));
            Assert.That(session.CheckoutSettlements.Revision,
                Is.EqualTo(before.EconomyRevision + 1));
            Assert.That(session.RetailOffers.Revision, Is.EqualTo(before.OfferRevision));
            Assert.That(session.Orders.Revision, Is.EqualTo(before.OrderRevision));

            Assert.That(session.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(session.Inventory.ReservationCount, Is.Zero);
            Assert.That(session.RetailBaskets.Count, Is.Zero);
            Assert.That(session.RetailCheckouts.CompletionCount, Is.EqualTo(1));
            Assert.That(session.CheckoutSettlements.SettlementCount, Is.EqualTo(1));
            Assert.That(session.CheckoutSettlements.TransactionCount, Is.EqualTo(1));
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.Zero);
            Assert.That(session.Inventory.GetAvailableQuantity(session.ProductId).Value, Is.Zero);

            Assert.That(session.TryGetPrototypeCheckoutCompletion(
                out RetailCheckoutCompletionRecord completion), Is.True);
            Assert.That(completion.Id, Is.EqualTo(session.PrototypeCheckoutCompletionId));
            Assert.That(completion.Lines.Count, Is.EqualTo(1));
            Assert.That(completion.Lines[0].UnitCost.CurrencyCode,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(completion.Lines[0].UnitCost.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeUnitCostMinorUnits));

            Assert.That(session.TryGetPrototypeCheckoutSettlement(
                out CheckoutSettlementReceipt receipt), Is.True);
            Assert.That(receipt.Id, Is.EqualTo(session.PrototypeCheckoutSettlementId));
            Assert.That(receipt.TransactionId, Is.EqualTo(session.PrototypeLedgerTransactionId));
            Assert.That(receipt.CompletionId, Is.EqualTo(session.PrototypeCheckoutCompletionId));
            Assert.That(receipt.CheckoutId, Is.EqualTo(session.PrototypeCheckoutId));
            Assert.That(receipt.CustomerId, Is.EqualTo(session.PrototypeCustomerId));
            Assert.That(receipt.PaymentMethod, Is.EqualTo(CheckoutPaymentMethod.Cash));
            Assert.That(receipt.PaidAt, Is.EqualTo(PaidAt));
            Assert.That(receipt.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(receipt.GrossMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(receipt.CostOfGoodsSoldMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(receipt.GrossMarginMinorUnits, Is.EqualTo(12_999));
            Assert.That(session.CheckoutSettlements.TryGetSettlementForCheckout(
                session.PrototypeCheckoutId,
                out CheckoutSettlementReceipt checkoutReceipt), Is.True);
            Assert.That(checkoutReceipt, Is.SameAs(receipt));

            Assert.That(session.TryGetPrototypeLedgerTransaction(
                out EconomyLedgerTransactionRecord transaction), Is.True);
            Assert.That(transaction.Id, Is.EqualTo(session.PrototypeLedgerTransactionId));
            Assert.That(transaction.SettlementId,
                Is.EqualTo(session.PrototypeCheckoutSettlementId));
            Assert.That(transaction.PostedAt, Is.EqualTo(PaidAt));
            Assert.That(transaction.Entries.Count, Is.EqualTo(4));
            AssertEntry(
                transaction.Entries[0],
                EconomyAccountKind.Cash,
                EconomyEntryDirection.Debit,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertEntry(
                transaction.Entries[1],
                EconomyAccountKind.SalesRevenue,
                EconomyEntryDirection.Credit,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertEntry(
                transaction.Entries[2],
                EconomyAccountKind.CostOfGoodsSold,
                EconomyEntryDirection.Debit,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits);
            AssertEntry(
                transaction.Entries[3],
                EconomyAccountKind.InventoryAsset,
                EconomyEntryDirection.Credit,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits);

            long debitMinorUnits = 0;
            long creditMinorUnits = 0;
            for (int index = 0; index < transaction.Entries.Count; index++)
            {
                EconomyLedgerEntryRecord entry = transaction.Entries[index];
                if (entry.Direction == EconomyEntryDirection.Debit)
                {
                    debitMinorUnits += entry.MinorUnits;
                }
                else
                {
                    creditMinorUnits += entry.MinorUnits;
                }
            }

            Assert.That(debitMinorUnits, Is.EqualTo(96_999));
            Assert.That(creditMinorUnits, Is.EqualTo(debitMinorUnits));
            AssertAccountDelta(
                session,
                EconomyAccountKind.Cash,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertAccountDelta(
                session,
                EconomyAccountKind.SalesRevenue,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertAccountDelta(
                session,
                EconomyAccountKind.CostOfGoodsSold,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits);
            AssertAccountDelta(
                session,
                EconomyAccountKind.InventoryAsset,
                -GarageStockFlowSession.PrototypeUnitCostMinorUnits);
            AssertInvariants(session);
        }

        [TestCase("EUR", 54_998L)]
        [TestCase("EUR", 55_000L)]
        [TestCase("USD", GarageStockFlowSession.PrototypePriceMinorUnits)]
        public void WrongAmountOrCurrencyRejectsWithoutAnyMutation(
            string tenderCurrencyCode,
            long tenderedMinorUnits)
        {
            GarageStockFlowSession session = CreateReadyCheckout();
            AuthorityState before = AuthorityState.Capture(session);

            OperationResult result = session.CheckoutSettlements.SettleCashCheckout(
                session.PrototypeCheckoutSettlementId,
                session.PrototypeLedgerTransactionId,
                session.PrototypeCheckoutCompletionId,
                session.PrototypeCheckoutId,
                tenderCurrencyCode,
                tenderedMinorUnits,
                PaidAt);

            Assert.That(result.Error, Is.EqualTo(CheckoutSettlementFailures.PaymentMismatch));
            before.AssertUnchanged(session);
            Assert.That(session.TryGetPrototypeCheckoutCompletion(out _), Is.False);
            Assert.That(session.TryGetPrototypeCheckoutSettlement(out _), Is.False);
            Assert.That(session.TryGetPrototypeLedgerTransaction(out _), Is.False);
            Assert.That(session.Inventory.SerializedItemCount, Is.EqualTo(1));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(1));
            Assert.That(session.RetailBaskets.Count, Is.EqualTo(1));
            AssertInvariants(session);
        }

        [Test]
        public void UnsupportedPaymentMethodRejectsValidPayloadWithoutAnyMutation()
        {
            GarageStockFlowSession session = CreateReadyCheckout();
            AuthorityState before = AuthorityState.Capture(session);

            OperationResult result = session.CheckoutSettlements.SettleCheckout(
                session.PrototypeCheckoutSettlementId,
                session.PrototypeLedgerTransactionId,
                session.PrototypeCheckoutCompletionId,
                session.PrototypeCheckoutId,
                (CheckoutPaymentMethod)999,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits,
                PaidAt);

            Assert.That(result.Error,
                Is.EqualTo(CheckoutSettlementFailures.PaymentMethodUnsupported));
            before.AssertUnchanged(session);
            Assert.That(session.TryGetPrototypeCheckoutCompletion(out _), Is.False);
            Assert.That(session.TryGetPrototypeCheckoutSettlement(out _), Is.False);
            Assert.That(session.TryGetPrototypeLedgerTransaction(out _), Is.False);
            Assert.That(session.Inventory.SerializedItemCount, Is.EqualTo(1));
            Assert.That(session.Inventory.ReservationCount, Is.EqualTo(1));
            Assert.That(session.RetailBaskets.Count, Is.EqualTo(1));
            AssertInvariants(session);
        }

        [Test]
        public void ExactReplayReturnsSuccessWithoutAnySecondMutation()
        {
            GarageStockFlowSession session = CreateReadyCheckout();
            Assert.That(session.SettlePrototypeCashCheckout().IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCheckoutSettlement(
                out CheckoutSettlementReceipt originalReceipt), Is.True);
            Assert.That(session.TryGetPrototypeLedgerTransaction(
                out EconomyLedgerTransactionRecord originalTransaction), Is.True);
            AuthorityState settled = AuthorityState.Capture(session);

            OperationResult replay = session.SettlePrototypeCashCheckout();

            Assert.That(replay.IsSuccess, Is.True);
            settled.AssertUnchanged(session);
            Assert.That(session.TryGetPrototypeCheckoutSettlement(
                out CheckoutSettlementReceipt replayedReceipt), Is.True);
            Assert.That(session.TryGetPrototypeLedgerTransaction(
                out EconomyLedgerTransactionRecord replayedTransaction), Is.True);
            Assert.That(replayedReceipt, Is.SameAs(originalReceipt));
            Assert.That(replayedTransaction, Is.SameAs(originalTransaction));
            AssertInvariants(session);
        }

        [Test]
        public void ReusedSettlementIdentityFailsWithoutMutation()
        {
            GarageStockFlowSession session = CreateSettledCheckout();
            AuthorityState settled = AuthorityState.Capture(session);

            OperationResult conflict = session.CheckoutSettlements.SettleCashCheckout(
                session.PrototypeCheckoutSettlementId,
                TransactionId("economy.ledger-transaction.settlement-conflict"),
                session.PrototypeCheckoutCompletionId,
                session.PrototypeCheckoutId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits,
                PaidAt);

            Assert.That(conflict.Error,
                Is.EqualTo(CheckoutSettlementFailures.SettlementIdentityConflict));
            settled.AssertUnchanged(session);
            AssertInvariants(session);
        }

        [Test]
        public void ReusedTransactionIdentityFailsWithoutMutation()
        {
            GarageStockFlowSession session = CreateSettledCheckout();
            AuthorityState settled = AuthorityState.Capture(session);

            OperationResult conflict = session.CheckoutSettlements.SettleCashCheckout(
                SettlementId("economy.checkout-settlement.transaction-conflict"),
                session.PrototypeLedgerTransactionId,
                session.PrototypeCheckoutCompletionId,
                session.PrototypeCheckoutId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits,
                PaidAt);

            Assert.That(conflict.Error,
                Is.EqualTo(CheckoutSettlementFailures.TransactionIdentityConflict));
            settled.AssertUnchanged(session);
            AssertInvariants(session);
        }

        [Test]
        public void SecondPaymentForSettledCheckoutFailsWithoutMutation()
        {
            GarageStockFlowSession session = CreateSettledCheckout();
            AuthorityState settled = AuthorityState.Capture(session);

            OperationResult secondPayment = session.CheckoutSettlements.SettleCashCheckout(
                SettlementId("economy.checkout-settlement.second-payment"),
                TransactionId("economy.ledger-transaction.second-payment"),
                StableId<RetailCheckoutCompletionIdScope>.Parse(
                    "retail.checkout-completion.second-payment"),
                session.PrototypeCheckoutId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits,
                PaidAt);

            Assert.That(secondPayment.Error,
                Is.EqualTo(CheckoutSettlementFailures.CheckoutAlreadySettled));
            settled.AssertUnchanged(session);
            AssertInvariants(session);
        }

        private static GarageStockFlowSession CreateReadyCheckout()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            Assert.That(session.ReservePrototypeCustomerBasket().IsSuccess, Is.True);
            Assert.That(session.BeginPrototypeCheckout().IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCheckout(out _), Is.True);
            Assert.That(session.TryGetPrototypeCheckoutCompletion(out _), Is.False);
            Assert.That(session.TryGetPrototypeCheckoutSettlement(out _), Is.False);
            Assert.That(session.TryGetPrototypeLedgerTransaction(out _), Is.False);
            AssertInvariants(session);
            return session;
        }

        private static GarageStockFlowSession CreateSettledCheckout()
        {
            GarageStockFlowSession session = CreateReadyCheckout();
            Assert.That(session.SettlePrototypeCashCheckout().IsSuccess, Is.True);
            return session;
        }

        private static StableId<EconomyCheckoutSettlementIdScope> SettlementId(string value)
        {
            return StableId<EconomyCheckoutSettlementIdScope>.Parse(value);
        }

        private static StableId<EconomyLedgerTransactionIdScope> TransactionId(string value)
        {
            return StableId<EconomyLedgerTransactionIdScope>.Parse(value);
        }

        private static void AssertEntry(
            EconomyLedgerEntryRecord entry,
            EconomyAccountKind account,
            EconomyEntryDirection direction,
            long minorUnits)
        {
            Assert.That(entry.Account, Is.EqualTo(account));
            Assert.That(entry.Direction, Is.EqualTo(direction));
            Assert.That(entry.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(entry.MinorUnits, Is.EqualTo(minorUnits));
        }

        private static void AssertAccountDelta(
            GarageStockFlowSession session,
            EconomyAccountKind account,
            long expectedMinorUnits)
        {
            CurrencyCode currency = CurrencyCode.Create(
                GarageStockFlowSession.PrototypeCurrencyCode).Value;
            OperationResult<long> delta =
                session.CheckoutSettlements.GetAccountDelta(account, currency);
            Assert.That(delta.IsSuccess, Is.True);
            Assert.That(delta.Value, Is.EqualTo(expectedMinorUnits));
        }

        private static void AssertInvariants(GarageStockFlowSession session)
        {
            Assert.That(session.Inventory.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.RetailBaskets.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.RetailCheckouts.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.CheckoutSettlements.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private readonly struct AuthorityState
        {
            private AuthorityState(GarageStockFlowSession session)
            {
                InventoryRevision = session.Inventory.Revision;
                InventorySerializedItemCount = session.Inventory.SerializedItemCount;
                InventoryReservationCount = session.Inventory.ReservationCount;
                InventoryTotalQuantity =
                    session.Inventory.GetTotalQuantity(session.ProductId).Value;
                InventoryAvailableQuantity =
                    session.Inventory.GetAvailableQuantity(session.ProductId).Value;
                BasketRevision = session.RetailBaskets.Revision;
                BasketCount = session.RetailBaskets.Count;
                CheckoutRevision = session.RetailCheckouts.Revision;
                CheckoutCount = session.RetailCheckouts.Count;
                CompletionCount = session.RetailCheckouts.CompletionCount;
                EconomyRevision = session.CheckoutSettlements.Revision;
                SettlementCount = session.CheckoutSettlements.SettlementCount;
                TransactionCount = session.CheckoutSettlements.TransactionCount;
                OfferRevision = session.RetailOffers.Revision;
                OrderRevision = session.Orders.Revision;
            }

            public long InventoryRevision { get; }

            private int InventorySerializedItemCount { get; }

            private int InventoryReservationCount { get; }

            private long InventoryTotalQuantity { get; }

            private long InventoryAvailableQuantity { get; }

            public long BasketRevision { get; }

            private int BasketCount { get; }

            public long CheckoutRevision { get; }

            private int CheckoutCount { get; }

            private int CompletionCount { get; }

            public long EconomyRevision { get; }

            private int SettlementCount { get; }

            private int TransactionCount { get; }

            public long OfferRevision { get; }

            public long OrderRevision { get; }

            public static AuthorityState Capture(GarageStockFlowSession session)
            {
                return new AuthorityState(session);
            }

            public void AssertUnchanged(GarageStockFlowSession session)
            {
                Assert.That(session.Inventory.Revision, Is.EqualTo(InventoryRevision));
                Assert.That(session.Inventory.SerializedItemCount,
                    Is.EqualTo(InventorySerializedItemCount));
                Assert.That(session.Inventory.ReservationCount,
                    Is.EqualTo(InventoryReservationCount));
                Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value,
                    Is.EqualTo(InventoryTotalQuantity));
                Assert.That(session.Inventory.GetAvailableQuantity(session.ProductId).Value,
                    Is.EqualTo(InventoryAvailableQuantity));
                Assert.That(session.RetailBaskets.Revision, Is.EqualTo(BasketRevision));
                Assert.That(session.RetailBaskets.Count, Is.EqualTo(BasketCount));
                Assert.That(session.RetailCheckouts.Revision, Is.EqualTo(CheckoutRevision));
                Assert.That(session.RetailCheckouts.Count, Is.EqualTo(CheckoutCount));
                Assert.That(session.RetailCheckouts.CompletionCount,
                    Is.EqualTo(CompletionCount));
                Assert.That(session.CheckoutSettlements.Revision, Is.EqualTo(EconomyRevision));
                Assert.That(session.CheckoutSettlements.SettlementCount,
                    Is.EqualTo(SettlementCount));
                Assert.That(session.CheckoutSettlements.TransactionCount,
                    Is.EqualTo(TransactionCount));
                Assert.That(session.RetailOffers.Revision, Is.EqualTo(OfferRevision));
                Assert.That(session.Orders.Revision, Is.EqualTo(OrderRevision));
            }
        }
    }
}
