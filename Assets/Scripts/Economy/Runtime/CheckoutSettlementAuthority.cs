using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Economy
{
    /// <summary>
    /// The sole production coordinator for checkout fulfillment and the first exact cash ledger
    /// settlement. Every participating authority is preflighted before stock or money mutates.
    /// </summary>
    public sealed class CheckoutSettlementAuthority
    {
        private readonly RetailCheckoutAuthority _checkouts;
        private readonly Dictionary<StableId<EconomyCheckoutSettlementIdScope>, CheckoutSettlementReceipt>
            _settlements =
                new Dictionary<StableId<EconomyCheckoutSettlementIdScope>, CheckoutSettlementReceipt>();
        private readonly Dictionary<StableId<EconomyLedgerTransactionIdScope>, EconomyLedgerTransactionRecord>
            _transactions =
                new Dictionary<StableId<EconomyLedgerTransactionIdScope>, EconomyLedgerTransactionRecord>();
        private readonly Dictionary<StableId<RetailCheckoutIdScope>, StableId<EconomyCheckoutSettlementIdScope>>
            _settlementByCheckout =
                new Dictionary<StableId<RetailCheckoutIdScope>, StableId<EconomyCheckoutSettlementIdScope>>();

        private CheckoutSettlementAuthority(RetailCheckoutAuthority checkouts)
        {
            _checkouts = checkouts;
        }

        public long Revision { get; private set; }

        public int SettlementCount => _settlements.Count;

        public int TransactionCount => _transactions.Count;

        public static OperationResult<CheckoutSettlementAuthority> Create(
            RetailCheckoutAuthority checkouts)
        {
            return checkouts == null
                ? OperationResult<CheckoutSettlementAuthority>.Fail(
                    CheckoutSettlementFailures.MissingCheckoutAuthority)
                : OperationResult<CheckoutSettlementAuthority>.Success(
                    new CheckoutSettlementAuthority(checkouts));
        }

        public OperationResult SettleCashCheckout(
            StableId<EconomyCheckoutSettlementIdScope> settlementId,
            StableId<EconomyLedgerTransactionIdScope> transactionId,
            StableId<RetailCheckoutCompletionIdScope> completionId,
            StableId<RetailCheckoutIdScope> checkoutId,
            string tenderCurrencyCode,
            long tenderedMinorUnits,
            SimulationTimestamp paidAt)
        {
            return SettleCheckout(
                settlementId,
                transactionId,
                completionId,
                checkoutId,
                CheckoutPaymentMethod.Cash,
                tenderCurrencyCode,
                tenderedMinorUnits,
                paidAt);
        }

        public OperationResult SettleCheckout(
            StableId<EconomyCheckoutSettlementIdScope> settlementId,
            StableId<EconomyLedgerTransactionIdScope> transactionId,
            StableId<RetailCheckoutCompletionIdScope> completionId,
            StableId<RetailCheckoutIdScope> checkoutId,
            CheckoutPaymentMethod paymentMethod,
            string tenderCurrencyCode,
            long tenderedMinorUnits,
            SimulationTimestamp paidAt)
        {
            if (settlementId.IsEmpty ||
                transactionId.IsEmpty ||
                completionId.IsEmpty ||
                checkoutId.IsEmpty)
            {
                return OperationResult.Fail(CheckoutSettlementFailures.InvalidInput);
            }

            if (paymentMethod != CheckoutPaymentMethod.Cash)
            {
                return OperationResult.Fail(
                    CheckoutSettlementFailures.PaymentMethodUnsupported);
            }

            OperationResult<CurrencyCode> tenderCurrency =
                CurrencyCode.Create(tenderCurrencyCode);
            if (tenderCurrency.IsFailure || tenderedMinorUnits <= 0)
            {
                return OperationResult.Fail(CheckoutSettlementFailures.PaymentMismatch);
            }

            if (_settlements.TryGetValue(settlementId, out CheckoutSettlementReceipt existing))
            {
                return Matches(
                           existing,
                           transactionId,
                           completionId,
                           checkoutId,
                           tenderCurrency.Value,
                           tenderedMinorUnits,
                           paidAt) &&
                       ValidateReceipt(existing).IsNone
                    ? OperationResult.Success()
                    : OperationResult.Fail(
                        CheckoutSettlementFailures.SettlementIdentityConflict);
            }

            if (_transactions.ContainsKey(transactionId))
            {
                return OperationResult.Fail(
                    CheckoutSettlementFailures.TransactionIdentityConflict);
            }

            if (_settlementByCheckout.ContainsKey(checkoutId))
            {
                return OperationResult.Fail(
                    CheckoutSettlementFailures.CheckoutAlreadySettled);
            }

            if (!_checkouts.TryGetCheckout(checkoutId, out RetailCheckoutRecord checkout))
            {
                return OperationResult.Fail(CheckoutSettlementFailures.UnknownCheckout);
            }

            if (_checkouts.TryGetCompletionForCheckout(checkoutId, out _))
            {
                return OperationResult.Fail(
                    CheckoutSettlementFailures.CheckoutAlreadyFulfilled);
            }

            if (!paidAt.IsAtOrAfter(checkout.StartedAt))
            {
                return OperationResult.Fail(CheckoutSettlementFailures.BeforeCheckout);
            }

            if (checkout.Currency != tenderCurrency.Value ||
                checkout.TotalMinorUnits != tenderedMinorUnits)
            {
                return OperationResult.Fail(CheckoutSettlementFailures.PaymentMismatch);
            }

            Failure costFailure = TryCalculateCostOfGoodsSold(
                checkout,
                out long costOfGoodsSoldMinorUnits);
            if (!costFailure.IsNone)
            {
                return OperationResult.Fail(costFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult.Fail(CheckoutSettlementFailures.RevisionOverflow);
            }

            OperationResult<RetailCheckoutCompletionPlan> checkoutPlan =
                _checkouts.PrepareCheckoutCompletion(
                    completionId,
                    checkoutId,
                    paidAt);
            if (checkoutPlan.IsFailure)
            {
                return OperationResult.Fail(checkoutPlan.Error);
            }

            if (checkoutPlan.Value.IsReplay)
            {
                return OperationResult.Fail(
                    CheckoutSettlementFailures.CheckoutAlreadyFulfilled);
            }

            IReadOnlyList<EconomyLedgerEntryRecord> entries = CreateEntries(
                checkout.Currency,
                checkout.TotalMinorUnits,
                costOfGoodsSoldMinorUnits);
            var transaction = new EconomyLedgerTransactionRecord(
                transactionId,
                settlementId,
                paidAt,
                entries);
            var receipt = new CheckoutSettlementReceipt(
                settlementId,
                transactionId,
                completionId,
                checkoutId,
                checkout.CustomerId,
                paymentMethod,
                paidAt,
                checkout.Currency,
                checkout.TotalMinorUnits,
                costOfGoodsSoldMinorUnits);

            Failure preparedFailure = ValidatePrepared(transaction, receipt);
            if (!preparedFailure.IsNone)
            {
                return OperationResult.Fail(preparedFailure);
            }

            OperationResult completion =
                _checkouts.CommitPreparedCheckoutCompletion(checkoutPlan.Value);
            if (completion.IsFailure)
            {
                return completion;
            }

            _transactions.Add(transaction.Id, transaction);
            _settlements.Add(receipt.Id, receipt);
            _settlementByCheckout.Add(receipt.CheckoutId, receipt.Id);
            Revision++;
            return OperationResult.Success();
        }

        public bool TryGetSettlement(
            StableId<EconomyCheckoutSettlementIdScope> settlementId,
            out CheckoutSettlementReceipt receipt)
        {
            return _settlements.TryGetValue(settlementId, out receipt);
        }

        public bool TryGetSettlementForCheckout(
            StableId<RetailCheckoutIdScope> checkoutId,
            out CheckoutSettlementReceipt receipt)
        {
            if (_settlementByCheckout.TryGetValue(
                    checkoutId,
                    out StableId<EconomyCheckoutSettlementIdScope> settlementId))
            {
                return _settlements.TryGetValue(settlementId, out receipt);
            }

            receipt = null;
            return false;
        }

        public bool TryGetTransaction(
            StableId<EconomyLedgerTransactionIdScope> transactionId,
            out EconomyLedgerTransactionRecord transaction)
        {
            return _transactions.TryGetValue(transactionId, out transaction);
        }

        public IReadOnlyList<CheckoutSettlementReceipt> GetSettlements()
        {
            var ordered = new List<CheckoutSettlementReceipt>(_settlements.Values);
            ordered.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(ordered.ToArray());
        }

        public IReadOnlyList<EconomyLedgerTransactionRecord> GetTransactions()
        {
            var ordered = new List<EconomyLedgerTransactionRecord>(_transactions.Values);
            ordered.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(ordered.ToArray());
        }

        public OperationResult<long> GetAccountDelta(
            EconomyAccountKind account,
            CurrencyCode currency)
        {
            if (!IsValidAccount(account) ||
                CurrencyCode.Create(currency.Value).IsFailure)
            {
                return OperationResult<long>.Fail(
                    CheckoutSettlementFailures.InvalidInput);
            }

            long total = 0;
            foreach (EconomyLedgerTransactionRecord transaction in _transactions.Values)
            {
                for (int index = 0; index < transaction.Entries.Count; index++)
                {
                    EconomyLedgerEntryRecord entry = transaction.Entries[index];
                    if (entry.Account != account || entry.Currency != currency)
                    {
                        continue;
                    }

                    long signed = IsDebitNormal(account)
                        ? (entry.Direction == EconomyEntryDirection.Debit
                            ? entry.MinorUnits
                            : -entry.MinorUnits)
                        : (entry.Direction == EconomyEntryDirection.Credit
                            ? entry.MinorUnits
                            : -entry.MinorUnits);
                    if ((signed > 0 && total > long.MaxValue - signed) ||
                        (signed < 0 && total < long.MinValue - signed))
                    {
                        return OperationResult<long>.Fail(
                            CheckoutSettlementFailures.BalanceOverflow);
                    }

                    total += signed;
                }
            }

            return OperationResult<long>.Success(total);
        }

        public OperationResult ValidateInvariants()
        {
            if (Revision != _settlements.Count ||
                _settlements.Count != _transactions.Count ||
                _settlements.Count != _settlementByCheckout.Count)
            {
                return OperationResult.Fail(CheckoutSettlementFailures.InvariantViolation);
            }

            var transactionIds = new HashSet<StableId<EconomyLedgerTransactionIdScope>>();
            var checkoutIds = new HashSet<StableId<RetailCheckoutIdScope>>();
            foreach (KeyValuePair<StableId<EconomyCheckoutSettlementIdScope>, CheckoutSettlementReceipt>
                     entry in _settlements)
            {
                CheckoutSettlementReceipt receipt = entry.Value;
                if (receipt == null ||
                    entry.Key != receipt.Id ||
                    !transactionIds.Add(receipt.TransactionId) ||
                    !checkoutIds.Add(receipt.CheckoutId) ||
                    !_settlementByCheckout.TryGetValue(
                        receipt.CheckoutId,
                        out StableId<EconomyCheckoutSettlementIdScope> owner) ||
                    owner != receipt.Id ||
                    !ValidateReceipt(receipt).IsNone)
                {
                    return OperationResult.Fail(
                        CheckoutSettlementFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<StableId<EconomyLedgerTransactionIdScope>, EconomyLedgerTransactionRecord>
                     entry in _transactions)
            {
                if (entry.Value == null ||
                    entry.Key != entry.Value.Id ||
                    !_settlements.ContainsKey(entry.Value.SettlementId) ||
                    !ValidateTransaction(entry.Value).IsNone)
                {
                    return OperationResult.Fail(
                        CheckoutSettlementFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private Failure TryCalculateCostOfGoodsSold(
            RetailCheckoutRecord checkout,
            out long costOfGoodsSoldMinorUnits)
        {
            costOfGoodsSoldMinorUnits = 0;
            if (checkout.Lines == null || checkout.Lines.Count == 0)
            {
                return CheckoutSettlementFailures.CostBasisInvalid;
            }

            for (int index = 0; index < checkout.Lines.Count; index++)
            {
                RetailCheckoutLineSnapshot line = checkout.Lines[index];
                if (line == null ||
                    InventoryUnitCost.Create(
                        line.UnitCost.CurrencyCode,
                        line.UnitCost.MinorUnits).IsFailure)
                {
                    return CheckoutSettlementFailures.CostBasisInvalid;
                }

                if (!string.Equals(
                        line.UnitCost.CurrencyCode,
                        checkout.Currency.Value,
                        StringComparison.Ordinal))
                {
                    return CheckoutSettlementFailures.CostCurrencyMismatch;
                }

                if (costOfGoodsSoldMinorUnits > long.MaxValue - line.UnitCost.MinorUnits)
                {
                    return CheckoutSettlementFailures.CostOverflow;
                }

                costOfGoodsSoldMinorUnits += line.UnitCost.MinorUnits;
            }

            return costOfGoodsSoldMinorUnits > 0
                ? Failure.None
                : CheckoutSettlementFailures.CostBasisInvalid;
        }

        private static IReadOnlyList<EconomyLedgerEntryRecord> CreateEntries(
            CurrencyCode currency,
            long grossMinorUnits,
            long costOfGoodsSoldMinorUnits)
        {
            return Array.AsReadOnly(new[]
            {
                new EconomyLedgerEntryRecord(
                    EconomyAccountKind.Cash,
                    EconomyEntryDirection.Debit,
                    currency,
                    grossMinorUnits),
                new EconomyLedgerEntryRecord(
                    EconomyAccountKind.SalesRevenue,
                    EconomyEntryDirection.Credit,
                    currency,
                    grossMinorUnits),
                new EconomyLedgerEntryRecord(
                    EconomyAccountKind.CostOfGoodsSold,
                    EconomyEntryDirection.Debit,
                    currency,
                    costOfGoodsSoldMinorUnits),
                new EconomyLedgerEntryRecord(
                    EconomyAccountKind.InventoryAsset,
                    EconomyEntryDirection.Credit,
                    currency,
                    costOfGoodsSoldMinorUnits)
            });
        }

        private Failure ValidateReceipt(CheckoutSettlementReceipt receipt)
        {
            if (receipt.Id.IsEmpty ||
                receipt.TransactionId.IsEmpty ||
                receipt.CompletionId.IsEmpty ||
                receipt.CheckoutId.IsEmpty ||
                receipt.CustomerId.IsEmpty ||
                receipt.PaymentMethod != CheckoutPaymentMethod.Cash ||
                CurrencyCode.Create(receipt.Currency.Value).IsFailure ||
                receipt.GrossMinorUnits <= 0 ||
                receipt.CostOfGoodsSoldMinorUnits <= 0 ||
                !_transactions.TryGetValue(
                    receipt.TransactionId,
                    out EconomyLedgerTransactionRecord transaction) ||
                transaction.SettlementId != receipt.Id ||
                !_checkouts.TryGetCompletion(
                    receipt.CompletionId,
                    out RetailCheckoutCompletionRecord completion) ||
                completion.CheckoutId != receipt.CheckoutId ||
                completion.CustomerId != receipt.CustomerId ||
                completion.Currency != receipt.Currency ||
                completion.TotalMinorUnits != receipt.GrossMinorUnits ||
                completion.CompletedAt != receipt.PaidAt)
            {
                return CheckoutSettlementFailures.InvariantViolation;
            }

            Failure transactionFailure = ValidateTransaction(transaction);
            if (!transactionFailure.IsNone ||
                transaction.PostedAt != receipt.PaidAt ||
                transaction.Entries[0].MinorUnits != receipt.GrossMinorUnits ||
                transaction.Entries[1].MinorUnits != receipt.GrossMinorUnits ||
                transaction.Entries[2].MinorUnits != receipt.CostOfGoodsSoldMinorUnits ||
                transaction.Entries[3].MinorUnits != receipt.CostOfGoodsSoldMinorUnits)
            {
                return CheckoutSettlementFailures.InvariantViolation;
            }

            return Failure.None;
        }

        private static Failure ValidatePrepared(
            EconomyLedgerTransactionRecord transaction,
            CheckoutSettlementReceipt receipt)
        {
            return transaction == null ||
                   receipt == null ||
                   transaction.Id != receipt.TransactionId ||
                   transaction.SettlementId != receipt.Id ||
                   transaction.PostedAt != receipt.PaidAt ||
                   transaction.Entries == null ||
                   transaction.Entries.Count != 4 ||
                   !ValidateTransaction(transaction).IsNone
                ? CheckoutSettlementFailures.PlanInvalid
                : Failure.None;
        }

        private static Failure ValidateTransaction(EconomyLedgerTransactionRecord transaction)
        {
            if (transaction == null ||
                transaction.Id.IsEmpty ||
                transaction.SettlementId.IsEmpty ||
                transaction.Entries == null ||
                transaction.Entries.Count != 4)
            {
                return CheckoutSettlementFailures.InvariantViolation;
            }

            EconomyLedgerEntryRecord cash = transaction.Entries[0];
            EconomyLedgerEntryRecord revenue = transaction.Entries[1];
            EconomyLedgerEntryRecord cogs = transaction.Entries[2];
            EconomyLedgerEntryRecord inventory = transaction.Entries[3];
            if (!MatchesEntry(cash, EconomyAccountKind.Cash, EconomyEntryDirection.Debit) ||
                !MatchesEntry(revenue, EconomyAccountKind.SalesRevenue, EconomyEntryDirection.Credit) ||
                !MatchesEntry(cogs, EconomyAccountKind.CostOfGoodsSold, EconomyEntryDirection.Debit) ||
                !MatchesEntry(inventory, EconomyAccountKind.InventoryAsset, EconomyEntryDirection.Credit) ||
                cash.Currency != revenue.Currency ||
                cash.Currency != cogs.Currency ||
                cash.Currency != inventory.Currency ||
                cash.MinorUnits != revenue.MinorUnits ||
                cogs.MinorUnits != inventory.MinorUnits)
            {
                return CheckoutSettlementFailures.InvariantViolation;
            }

            if (cash.MinorUnits > long.MaxValue - cogs.MinorUnits)
            {
                return CheckoutSettlementFailures.InvariantViolation;
            }

            long debitTotal = cash.MinorUnits + cogs.MinorUnits;
            long creditTotal = revenue.MinorUnits + inventory.MinorUnits;
            return debitTotal == creditTotal
                ? Failure.None
                : CheckoutSettlementFailures.InvariantViolation;
        }

        private static bool MatchesEntry(
            EconomyLedgerEntryRecord entry,
            EconomyAccountKind account,
            EconomyEntryDirection direction)
        {
            return entry != null &&
                   entry.Account == account &&
                   entry.Direction == direction &&
                   CurrencyCode.Create(entry.Currency.Value).IsSuccess &&
                   entry.MinorUnits > 0;
        }

        private static bool Matches(
            CheckoutSettlementReceipt receipt,
            StableId<EconomyLedgerTransactionIdScope> transactionId,
            StableId<RetailCheckoutCompletionIdScope> completionId,
            StableId<RetailCheckoutIdScope> checkoutId,
            CurrencyCode currency,
            long tenderedMinorUnits,
            SimulationTimestamp paidAt)
        {
            return receipt.TransactionId == transactionId &&
                   receipt.CompletionId == completionId &&
                   receipt.CheckoutId == checkoutId &&
                   receipt.PaymentMethod == CheckoutPaymentMethod.Cash &&
                   receipt.Currency == currency &&
                   receipt.GrossMinorUnits == tenderedMinorUnits &&
                   receipt.PaidAt == paidAt;
        }

        private static bool IsValidAccount(EconomyAccountKind account)
        {
            return account == EconomyAccountKind.Cash ||
                   account == EconomyAccountKind.SalesRevenue ||
                   account == EconomyAccountKind.CostOfGoodsSold ||
                   account == EconomyAccountKind.InventoryAsset;
        }

        private static bool IsDebitNormal(EconomyAccountKind account)
        {
            return account == EconomyAccountKind.Cash ||
                   account == EconomyAccountKind.CostOfGoodsSold ||
                   account == EconomyAccountKind.InventoryAsset;
        }
    }

    public static class CheckoutSettlementFailures
    {
        public static readonly Failure MissingCheckoutAuthority =
            Failure.FromCode("economy.checkout-settlement.checkout-missing");
        public static readonly Failure InvalidInput =
            Failure.FromCode("economy.checkout-settlement.input-invalid");
        public static readonly Failure PaymentMethodUnsupported =
            Failure.FromCode("economy.checkout-settlement.payment-method-unsupported");
        public static readonly Failure PaymentMismatch =
            Failure.FromCode("economy.checkout-settlement.payment-mismatch");
        public static readonly Failure CostBasisInvalid =
            Failure.FromCode("economy.checkout-settlement.cost-basis-invalid");
        public static readonly Failure CostCurrencyMismatch =
            Failure.FromCode("economy.checkout-settlement.cost-currency-mismatch");
        public static readonly Failure CostOverflow =
            Failure.FromCode("economy.checkout-settlement.cost-overflow");
        public static readonly Failure SettlementIdentityConflict =
            Failure.FromCode("economy.checkout-settlement.identity-conflict");
        public static readonly Failure TransactionIdentityConflict =
            Failure.FromCode("economy.checkout-settlement.transaction-identity-conflict");
        public static readonly Failure CheckoutAlreadySettled =
            Failure.FromCode("economy.checkout-settlement.checkout-already-settled");
        public static readonly Failure CheckoutAlreadyFulfilled =
            Failure.FromCode("economy.checkout-settlement.checkout-already-fulfilled");
        public static readonly Failure UnknownCheckout =
            Failure.FromCode("economy.checkout-settlement.checkout-unknown");
        public static readonly Failure BeforeCheckout =
            Failure.FromCode("economy.checkout-settlement.before-checkout");
        public static readonly Failure PlanInvalid =
            Failure.FromCode("economy.checkout-settlement.plan-invalid");
        public static readonly Failure PlanStale =
            Failure.FromCode("economy.checkout-settlement.plan-stale");
        public static readonly Failure RevisionOverflow =
            Failure.FromCode("economy.checkout-settlement.revision-overflow");
        public static readonly Failure BalanceOverflow =
            Failure.FromCode("economy.checkout-settlement.balance-overflow");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("economy.checkout-settlement.invariant");
    }
}
