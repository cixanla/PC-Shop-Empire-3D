using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PcFirmwareBaselineOperationIdScope : IStableIdScope
    {
    }

    public enum PcFirmwareBaselineProfile
    {
        OptimizedDefaults = 1
    }

    public enum PcFirmwareBaselineResult
    {
        SavedAndExited = 1
    }

    /// <summary>
    /// Immutable evidence that one exact current POST cycle saved the bounded fictional
    /// UEFI safe-default profile. It is not firmware flashing, OS or benchmark evidence.
    /// </summary>
    public sealed class PcFirmwareBaselineReceipt
    {
        private readonly PcPowerStateAuthority _owner;

        internal PcFirmwareBaselineReceipt(
            PcPowerStateAuthority owner,
            StableId<PcFirmwareBaselineOperationIdScope> operationId,
            PcPostStartupReceipt sourcePostStartupReceipt,
            long expectedPowerStateRevision,
            long expectedRevision,
            long revision)
        {
            _owner = owner;
            OperationId = operationId;
            SourcePostStartupReceipt = sourcePostStartupReceipt;
            ExpectedPowerStateRevision = expectedPowerStateRevision;
            ExpectedRevision = expectedRevision;
            Revision = revision;
        }

        public StableId<PcFirmwareBaselineOperationIdScope> OperationId { get; }

        public PcFirmwareBaselineProfile Profile =>
            PcFirmwareBaselineProfile.OptimizedDefaults;

        public PcFirmwareBaselineResult Result =>
            PcFirmwareBaselineResult.SavedAndExited;

        public PcPostStartupReceipt SourcePostStartupReceipt { get; }

        public PcPowerStateReceipt SourcePowerOnReceipt =>
            SourcePostStartupReceipt?.SourcePowerOnReceipt;

        public PowerTestAttemptReceipt PreflightReceipt =>
            SourcePostStartupReceipt?.PreflightReceipt;

        public long SourcePostStartupRevision =>
            SourcePostStartupReceipt?.Revision ?? -1L;

        public long ExpectedPowerStateRevision { get; }

        public long PowerStateRevision =>
            SourcePostStartupReceipt?.PowerStateRevision ?? -1L;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        internal bool IsOwnedBy(PcPowerStateAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<PcFirmwareBaselineOperationIdScope> operationId,
            PcPostStartupReceipt sourcePostStartupReceipt,
            long expectedPowerStateRevision,
            long expectedRevision)
        {
            return OperationId == operationId &&
                   ReferenceEquals(
                       SourcePostStartupReceipt,
                       sourcePostStartupReceipt) &&
                   ExpectedPowerStateRevision == expectedPowerStateRevision &&
                   ExpectedRevision == expectedRevision;
        }
    }

    public static class PcFirmwareBaselineFailures
    {
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "assembly.firmware-baseline.operation-id-invalid");
        public static readonly Failure InvalidPostStartupReceipt = Failure.FromCode(
            "assembly.firmware-baseline.post-startup-receipt-invalid");
        public static readonly Failure PowerStateRevisionMismatch = Failure.FromCode(
            "assembly.firmware-baseline.power-state-revision-mismatch");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "assembly.firmware-baseline.revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "assembly.firmware-baseline.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "assembly.firmware-baseline.operation-conflict");
        public static readonly Failure AlreadyCompleted = Failure.FromCode(
            "assembly.firmware-baseline.already-completed");
        public static readonly Failure NotCurrent = Failure.FromCode(
            "assembly.firmware-baseline.not-current");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "assembly.firmware-baseline.receipt-history-invalid");
    }
}
