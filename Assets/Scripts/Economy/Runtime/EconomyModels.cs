using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Economy
{
    public enum EconomyAccountKind
    {
        Cash = 1,
        SalesRevenue = 2,
        CostOfGoodsSold = 3,
        InventoryAsset = 4
    }

    public enum EconomyEntryDirection
    {
        Debit = 1,
        Credit = 2
    }

    public enum CheckoutPaymentMethod
    {
        Cash = 1
    }

    public sealed class EconomyLedgerEntryRecord
    {
        internal EconomyLedgerEntryRecord(
            EconomyAccountKind account,
            EconomyEntryDirection direction,
            CurrencyCode currency,
            long minorUnits)
        {
            Account = account;
            Direction = direction;
            Currency = currency;
            MinorUnits = minorUnits;
        }

        public EconomyAccountKind Account { get; }

        public EconomyEntryDirection Direction { get; }

        public CurrencyCode Currency { get; }

        public long MinorUnits { get; }
    }

    public sealed class EconomyLedgerTransactionRecord
    {
        internal EconomyLedgerTransactionRecord(
            StableId<EconomyLedgerTransactionIdScope> id,
            StableId<EconomyCheckoutSettlementIdScope> settlementId,
            SimulationTimestamp postedAt,
            IReadOnlyList<EconomyLedgerEntryRecord> entries)
        {
            Id = id;
            SettlementId = settlementId;
            PostedAt = postedAt;
            Entries = entries;
        }

        public StableId<EconomyLedgerTransactionIdScope> Id { get; }

        public StableId<EconomyCheckoutSettlementIdScope> SettlementId { get; }

        public SimulationTimestamp PostedAt { get; }

        public IReadOnlyList<EconomyLedgerEntryRecord> Entries { get; }
    }

    public sealed class CheckoutSettlementReceipt
    {
        internal CheckoutSettlementReceipt(
            StableId<EconomyCheckoutSettlementIdScope> id,
            StableId<EconomyLedgerTransactionIdScope> transactionId,
            StableId<RetailCheckoutCompletionIdScope> completionId,
            StableId<RetailCheckoutIdScope> checkoutId,
            StableId<RetailCustomerIdScope> customerId,
            CheckoutPaymentMethod paymentMethod,
            SimulationTimestamp paidAt,
            CurrencyCode currency,
            long grossMinorUnits,
            long costOfGoodsSoldMinorUnits)
        {
            Id = id;
            TransactionId = transactionId;
            CompletionId = completionId;
            CheckoutId = checkoutId;
            CustomerId = customerId;
            PaymentMethod = paymentMethod;
            PaidAt = paidAt;
            Currency = currency;
            GrossMinorUnits = grossMinorUnits;
            CostOfGoodsSoldMinorUnits = costOfGoodsSoldMinorUnits;
        }

        public StableId<EconomyCheckoutSettlementIdScope> Id { get; }

        public StableId<EconomyLedgerTransactionIdScope> TransactionId { get; }

        public StableId<RetailCheckoutCompletionIdScope> CompletionId { get; }

        public StableId<RetailCheckoutIdScope> CheckoutId { get; }

        public StableId<RetailCustomerIdScope> CustomerId { get; }

        public CheckoutPaymentMethod PaymentMethod { get; }

        public SimulationTimestamp PaidAt { get; }

        public CurrencyCode Currency { get; }

        public long GrossMinorUnits { get; }

        public long CostOfGoodsSoldMinorUnits { get; }

        public long GrossMarginMinorUnits =>
            GrossMinorUnits - CostOfGoodsSoldMinorUnits;
    }
}
