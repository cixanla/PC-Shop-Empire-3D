using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PcPostStartupOperationIdScope : IStableIdScope
    {
    }

    public enum PcPostStartupResult
    {
        Passed = 1
    }

    /// <summary>
    /// Immutable evidence that one exact active power-on cycle completed the bounded
    /// baseline POST self-test. It is not firmware, OS, driver or benchmark evidence.
    /// </summary>
    public sealed class PcPostStartupReceipt
    {
        private readonly PcPowerStateAuthority _owner;

        internal PcPostStartupReceipt(
            PcPowerStateAuthority owner,
            StableId<PcPostStartupOperationIdScope> operationId,
            PcPowerStateReceipt sourcePowerOnReceipt,
            long expectedPowerStateRevision,
            long revision)
        {
            _owner = owner;
            OperationId = operationId;
            SourcePowerOnReceipt = sourcePowerOnReceipt;
            ExpectedPowerStateRevision = expectedPowerStateRevision;
            Revision = revision;
        }

        public StableId<PcPostStartupOperationIdScope> OperationId { get; }

        public PcPostStartupResult Result => PcPostStartupResult.Passed;

        public PcPowerStateReceipt SourcePowerOnReceipt { get; }

        public PowerTestAttemptReceipt PreflightReceipt =>
            SourcePowerOnReceipt?.PreflightReceipt;

        public long ExpectedPowerStateRevision { get; }

        public long PowerStateRevision => SourcePowerOnReceipt?.Revision ?? -1L;

        public long Revision { get; }

        internal bool IsOwnedBy(PcPowerStateAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<PcPostStartupOperationIdScope> operationId,
            PcPowerStateReceipt sourcePowerOnReceipt,
            long expectedPowerStateRevision)
        {
            return OperationId == operationId &&
                   ReferenceEquals(SourcePowerOnReceipt, sourcePowerOnReceipt) &&
                   ExpectedPowerStateRevision == expectedPowerStateRevision;
        }
    }

    public static class PcPostStartupFailures
    {
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "assembly.post-startup.operation-id-invalid");
        public static readonly Failure InvalidPowerOnReceipt = Failure.FromCode(
            "assembly.post-startup.power-on-receipt-invalid");
        public static readonly Failure PowerStateRevisionMismatch = Failure.FromCode(
            "assembly.post-startup.power-state-revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "assembly.post-startup.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "assembly.post-startup.operation-conflict");
        public static readonly Failure AlreadyCompleted = Failure.FromCode(
            "assembly.post-startup.already-completed");
        public static readonly Failure NotCurrent = Failure.FromCode(
            "assembly.post-startup.not-current");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "assembly.post-startup.receipt-history-invalid");
    }
}
