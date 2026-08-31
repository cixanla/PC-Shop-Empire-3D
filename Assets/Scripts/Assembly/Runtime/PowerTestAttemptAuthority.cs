using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Owns only the historical fact that a player-triggered preflight was accepted for one
    /// exact electrical context. It does not own Assembly or electrical power state.
    /// </summary>
    public sealed class PowerTestAttemptAuthority
    {
        private readonly PcPowerBudgetAuthority _powerBudget;
        private readonly AssemblyBuildAuthority _assemblyBuild;
        private readonly Dictionary<StableId<PowerTestAttemptOperationIdScope>,
            PowerTestAttemptReceipt> _receipts =
                new Dictionary<StableId<PowerTestAttemptOperationIdScope>,
                    PowerTestAttemptReceipt>();

        private PowerTestAttemptAuthority(
            PcPowerBudgetAuthority powerBudget,
            AssemblyBuildAuthority assemblyBuild)
        {
            _powerBudget = powerBudget;
            _assemblyBuild = assemblyBuild;
        }

        public PcPowerBudgetAuthority PowerBudget => _powerBudget;

        public AssemblyBuildAuthority AssemblyBuild => _assemblyBuild;

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public bool HasCompletedPreflight => ReceiptCount == 1;

        public static OperationResult<PowerTestAttemptAuthority> Create(
            PcPowerBudgetAuthority powerBudget,
            AssemblyBuildAuthority assemblyBuild)
        {
            if (powerBudget == null || assemblyBuild == null)
            {
                return OperationResult<PowerTestAttemptAuthority>.Fail(
                    PowerTestAttemptFailures.ConfigurationMissing);
            }

            if (!ReferenceEquals(powerBudget.AssemblyBuild, assemblyBuild))
            {
                return OperationResult<PowerTestAttemptAuthority>.Fail(
                    PowerTestAttemptFailures.AuthorityMismatch);
            }

            return OperationResult<PowerTestAttemptAuthority>.Success(
                new PowerTestAttemptAuthority(powerBudget, assemblyBuild));
        }

        public OperationResult<PowerTestAttemptContext> ObserveCurrentContext()
        {
            OperationResult<PcPowerBudgetSnapshot> assessment =
                _powerBudget.AssessPowerBudget();
            return assessment.IsFailure
                ? OperationResult<PowerTestAttemptContext>.Fail(assessment.Error)
                : PowerTestAttemptContext.Capture(assessment.Value);
        }

        public OperationResult<PowerTestAttemptReceipt> TryAttemptPreflight(
            StableId<PowerTestAttemptOperationIdScope> operationId,
            PowerTestAttemptContext expectedContext,
            long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.InvalidOperationId);
            }

            if (expectedContext == null || !expectedContext.IsValid)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.ContextInvalid);
            }

            if (_receipts.TryGetValue(
                    operationId,
                    out PowerTestAttemptReceipt replay))
            {
                if (!replay.MatchesCommand(
                        operationId,
                        expectedRevision,
                        expectedContext))
                {
                    return OperationResult<PowerTestAttemptReceipt>.Fail(
                        PowerTestAttemptFailures.OperationConflict);
                }

                return OperationResult<PowerTestAttemptReceipt>.Success(replay);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.RevisionMismatch);
            }

            if (ReceiptCount != 0)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.AlreadyCompleted);
            }

            OperationResult<PowerTestAttemptContext> currentContext =
                ObserveCurrentContext();
            if (currentContext.IsFailure)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    currentContext.Error);
            }

            if (!currentContext.Value.IsSufficient)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.PowerSupplyInsufficient);
            }

            if (!expectedContext.Matches(currentContext.Value))
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.ContextStale);
            }

            long nextRevision = Revision + 1;
            var receipt = new PowerTestAttemptReceipt(
                operationId,
                expectedRevision,
                nextRevision,
                currentContext.Value);
            _receipts.Add(operationId, receipt);
            Revision = nextRevision;
            return OperationResult<PowerTestAttemptReceipt>.Success(receipt);
        }

        public OperationResult<PowerTestAttemptReceipt> EvaluateCurrentReceipt()
        {
            if (!TryGetCompletedReceipt(out PowerTestAttemptReceipt receipt))
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.ReceiptMissing);
            }

            OperationResult<PowerTestAttemptContext> currentContext =
                ObserveCurrentContext();
            if (currentContext.IsFailure)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    currentContext.Error);
            }

            if (!currentContext.Value.IsSufficient)
            {
                return OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.PowerSupplyInsufficient);
            }

            return receipt.Context.Matches(currentContext.Value)
                ? OperationResult<PowerTestAttemptReceipt>.Success(receipt)
                : OperationResult<PowerTestAttemptReceipt>.Fail(
                    PowerTestAttemptFailures.ContextStale);
        }

        public bool TryGetReceipt(
            StableId<PowerTestAttemptOperationIdScope> operationId,
            out PowerTestAttemptReceipt receipt)
        {
            return _receipts.TryGetValue(operationId, out receipt);
        }

        public bool TryGetCompletedReceipt(out PowerTestAttemptReceipt receipt)
        {
            receipt = null;
            if (ReceiptCount != 1)
            {
                return false;
            }

            foreach (PowerTestAttemptReceipt candidate in _receipts.Values)
            {
                receipt = candidate;
            }

            return receipt != null;
        }

        public OperationResult ValidateReceiptHistory()
        {
            if (Revision != ReceiptCount || ReceiptCount > 1)
            {
                return OperationResult.Fail(
                    PowerTestAttemptFailures.ReceiptHistoryInvalid);
            }

            if (ReceiptCount == 0)
            {
                return OperationResult.Success();
            }

            foreach (KeyValuePair<StableId<PowerTestAttemptOperationIdScope>,
                         PowerTestAttemptReceipt> entry in _receipts)
            {
                PowerTestAttemptReceipt receipt = entry.Value;
                if (entry.Key.IsEmpty || receipt == null ||
                    receipt.OperationId != entry.Key ||
                    receipt.Kind != PowerTestAttemptKind.PreflightReady ||
                    receipt.ExpectedRevision != 0 || receipt.Revision != 1 ||
                    receipt.Context == null || !receipt.Context.IsValid ||
                    !receipt.Context.IsSufficient)
                {
                    return OperationResult.Fail(
                        PowerTestAttemptFailures.ReceiptHistoryInvalid);
                }
            }

            return OperationResult.Success();
        }
    }
}
