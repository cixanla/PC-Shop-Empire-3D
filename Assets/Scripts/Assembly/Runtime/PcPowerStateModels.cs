using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PcPowerStateOperationIdScope : IStableIdScope
    {
    }

    public enum PcPowerState
    {
        Off = 0,
        Energized = 1
    }

    public enum PcPowerTransitionKind
    {
        PowerOn = 1,
        PowerOff = 2
    }

    /// <summary>
    /// Immutable state-transition evidence. A power-on receipt binds the exact accepted
    /// preflight; a power-off receipt binds the exact active power-on receipt.
    /// </summary>
    public sealed class PcPowerStateReceipt
    {
        private readonly PcPowerStateAuthority _owner;

        internal PcPowerStateReceipt(
            PcPowerStateAuthority owner,
            StableId<PcPowerStateOperationIdScope> operationId,
            PcPowerTransitionKind transitionKind,
            long expectedRevision,
            long revision,
            PowerTestAttemptReceipt preflightReceipt,
            PcPowerStateReceipt sourcePowerOnReceipt)
        {
            _owner = owner;
            OperationId = operationId;
            TransitionKind = transitionKind;
            ExpectedRevision = expectedRevision;
            Revision = revision;
            PreflightReceipt = preflightReceipt;
            SourcePowerOnReceipt = sourcePowerOnReceipt;
        }

        public StableId<PcPowerStateOperationIdScope> OperationId { get; }

        public PcPowerTransitionKind TransitionKind { get; }

        public PcPowerState ResultingState =>
            TransitionKind == PcPowerTransitionKind.PowerOn
                ? PcPowerState.Energized
                : PcPowerState.Off;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        public PowerTestAttemptReceipt PreflightReceipt { get; }

        public PcPowerStateReceipt SourcePowerOnReceipt { get; }

        internal bool IsOwnedBy(PcPowerStateAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesPowerOnCommand(
            StableId<PcPowerStateOperationIdScope> operationId,
            PowerTestAttemptReceipt preflightReceipt,
            long expectedRevision)
        {
            return TransitionKind == PcPowerTransitionKind.PowerOn &&
                   OperationId == operationId &&
                   ExpectedRevision == expectedRevision &&
                   ReferenceEquals(PreflightReceipt, preflightReceipt) &&
                   SourcePowerOnReceipt == null;
        }

        internal bool MatchesPowerOffCommand(
            StableId<PcPowerStateOperationIdScope> operationId,
            PcPowerStateReceipt sourcePowerOnReceipt,
            long expectedRevision)
        {
            return TransitionKind == PcPowerTransitionKind.PowerOff &&
                   OperationId == operationId &&
                   ExpectedRevision == expectedRevision &&
                   ReferenceEquals(SourcePowerOnReceipt, sourcePowerOnReceipt) &&
                   ReferenceEquals(
                       PreflightReceipt,
                       sourcePowerOnReceipt?.PreflightReceipt);
        }
    }

    public static class PcPowerStateFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "assembly.power-state.configuration-missing");
        public static readonly Failure AuthorityMismatch = Failure.FromCode(
            "assembly.power-state.authority-mismatch");
        public static readonly Failure AlreadyBound = Failure.FromCode(
            "assembly.power-state.already-bound");
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "assembly.power-state.operation-id-invalid");
        public static readonly Failure InvalidPreflightReceipt = Failure.FromCode(
            "assembly.power-state.preflight-invalid");
        public static readonly Failure PreflightStale = Failure.FromCode(
            "assembly.power-state.preflight-stale");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "assembly.power-state.revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "assembly.power-state.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "assembly.power-state.operation-conflict");
        public static readonly Failure AlreadyEnergized = Failure.FromCode(
            "assembly.power-state.already-energized");
        public static readonly Failure AlreadyOff = Failure.FromCode(
            "assembly.power-state.already-off");
        public static readonly Failure ActivePowerOnMismatch = Failure.FromCode(
            "assembly.power-state.active-power-on-mismatch");
        public static readonly Failure NotEnergized = Failure.FromCode(
            "assembly.power-state.not-energized");
        public static readonly Failure InterlockRejected = Failure.FromCode(
            "assembly.power-state.interlock-rejected");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "assembly.power-state.receipt-history-invalid");
    }
}
