using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Owns the safe Off/Energized state and bounded baseline POST receipts for one exact
    /// assembly and preflight authority. It deliberately does not own BIOS, firmware,
    /// operating-system, driver or benchmark outcomes.
    /// </summary>
    public sealed class PcPowerStateAuthority
    {
        private readonly PowerTestAttemptAuthority _powerTestAttempts;
        private readonly AssemblyBuildAuthority _assemblyBuild;
        private readonly Dictionary<StableId<PcPowerStateOperationIdScope>,
            PcPowerStateReceipt> _receipts =
                new Dictionary<StableId<PcPowerStateOperationIdScope>,
                    PcPowerStateReceipt>();
        private readonly List<PcPowerStateReceipt> _receiptsByRevision =
            new List<PcPowerStateReceipt>();
        private readonly Dictionary<StableId<PcPostStartupOperationIdScope>,
            PcPostStartupReceipt> _postStartupReceipts =
                new Dictionary<StableId<PcPostStartupOperationIdScope>,
                    PcPostStartupReceipt>();
        private readonly List<PcPostStartupReceipt> _postStartupReceiptsByRevision =
            new List<PcPostStartupReceipt>();

        private PcPowerStateReceipt _activePowerOnReceipt;
        private PcPostStartupReceipt _activePostStartupReceipt;

        private PcPowerStateAuthority(
            PowerTestAttemptAuthority powerTestAttempts,
            AssemblyBuildAuthority assemblyBuild)
        {
            _powerTestAttempts = powerTestAttempts;
            _assemblyBuild = assemblyBuild;
        }

        public PowerTestAttemptAuthority PowerTestAttempts => _powerTestAttempts;

        public AssemblyBuildAuthority AssemblyBuild => _assemblyBuild;

        public PcPowerState State { get; private set; }

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public bool IsEnergized => State == PcPowerState.Energized;

        public PcPowerStateReceipt ActivePowerOnReceipt => _activePowerOnReceipt;

        public long PostStartupRevision { get; private set; }

        public int PostStartupReceiptCount => _postStartupReceipts.Count;

        public PcPostStartupReceipt ActivePostStartupReceipt =>
            _activePostStartupReceipt;

        public static OperationResult<PcPowerStateAuthority> Create(
            PowerTestAttemptAuthority powerTestAttempts,
            AssemblyBuildAuthority assemblyBuild)
        {
            if (powerTestAttempts == null || assemblyBuild == null)
            {
                return OperationResult<PcPowerStateAuthority>.Fail(
                    PcPowerStateFailures.ConfigurationMissing);
            }

            if (!ReferenceEquals(
                    powerTestAttempts.AssemblyBuild,
                    assemblyBuild))
            {
                return OperationResult<PcPowerStateAuthority>.Fail(
                    PcPowerStateFailures.AuthorityMismatch);
            }

            var authority = new PcPowerStateAuthority(
                powerTestAttempts,
                assemblyBuild);
            OperationResult binding =
                assemblyBuild.BindElectricalPowerState(authority);
            return binding.IsFailure
                ? OperationResult<PcPowerStateAuthority>.Fail(binding.Error)
                : OperationResult<PcPowerStateAuthority>.Success(authority);
        }

        public OperationResult<PcPowerStateReceipt> TryPowerOn(
            StableId<PcPowerStateOperationIdScope> operationId,
            PowerTestAttemptReceipt preflightReceipt,
            long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(
                    operationId,
                    out PcPowerStateReceipt replay))
            {
                return replay.MatchesPowerOnCommand(
                        operationId,
                        preflightReceipt,
                        expectedRevision)
                    ? OperationResult<PcPowerStateReceipt>.Success(replay)
                    : OperationResult<PcPowerStateReceipt>.Fail(
                        PcPowerStateFailures.OperationConflict);
            }

            if (preflightReceipt == null ||
                !_powerTestAttempts.TryGetReceipt(
                    preflightReceipt.OperationId,
                    out PowerTestAttemptReceipt knownPreflight) ||
                !ReferenceEquals(knownPreflight, preflightReceipt))
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.InvalidPreflightReceipt);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.RevisionOverflow);
            }

            if (State != PcPowerState.Off || _activePowerOnReceipt != null)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.AlreadyEnergized);
            }

            if (_activePostStartupReceipt != null)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.ReceiptHistoryInvalid);
            }

            if (!HasLiveInterlockBinding())
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.InterlockRejected);
            }

            OperationResult<PowerTestAttemptReceipt> currentPreflight =
                _powerTestAttempts.EvaluateCurrentReceipt();
            if (currentPreflight.IsFailure ||
                !ReferenceEquals(currentPreflight.Value, preflightReceipt))
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.PreflightStale);
            }

            long nextRevision = Revision + 1;
            var receipt = new PcPowerStateReceipt(
                this,
                operationId,
                PcPowerTransitionKind.PowerOn,
                expectedRevision,
                nextRevision,
                preflightReceipt,
                null);
            OperationResult interlock = _assemblyBuild.SetElectricalPowerState(
                this,
                energized: true);
            if (interlock.IsFailure)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    interlock.Error);
            }

            _receipts.Add(operationId, receipt);
            _receiptsByRevision.Add(receipt);
            _activePowerOnReceipt = receipt;
            State = PcPowerState.Energized;
            Revision = nextRevision;
            return OperationResult<PcPowerStateReceipt>.Success(receipt);
        }

        public OperationResult<PcPowerStateReceipt> TryPowerOff(
            StableId<PcPowerStateOperationIdScope> operationId,
            PcPowerStateReceipt sourcePowerOnReceipt,
            long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(
                    operationId,
                    out PcPowerStateReceipt replay))
            {
                return replay.MatchesPowerOffCommand(
                        operationId,
                        sourcePowerOnReceipt,
                        expectedRevision)
                    ? OperationResult<PcPowerStateReceipt>.Success(replay)
                    : OperationResult<PcPowerStateReceipt>.Fail(
                        PcPowerStateFailures.OperationConflict);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.RevisionOverflow);
            }

            if (State != PcPowerState.Energized ||
                _activePowerOnReceipt == null)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.AlreadyOff);
            }

            if (sourcePowerOnReceipt == null ||
                !sourcePowerOnReceipt.IsOwnedBy(this) ||
                !ReferenceEquals(
                    sourcePowerOnReceipt,
                    _activePowerOnReceipt))
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.ActivePowerOnMismatch);
            }

            if (!HasLiveInterlockBinding())
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.InterlockRejected);
            }

            long nextRevision = Revision + 1;
            var receipt = new PcPowerStateReceipt(
                this,
                operationId,
                PcPowerTransitionKind.PowerOff,
                expectedRevision,
                nextRevision,
                sourcePowerOnReceipt.PreflightReceipt,
                sourcePowerOnReceipt);
            OperationResult interlock = _assemblyBuild.SetElectricalPowerState(
                this,
                energized: false);
            if (interlock.IsFailure)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    interlock.Error);
            }

            _receipts.Add(operationId, receipt);
            _receiptsByRevision.Add(receipt);
            _activePowerOnReceipt = null;
            _activePostStartupReceipt = null;
            State = PcPowerState.Off;
            Revision = nextRevision;
            return OperationResult<PcPowerStateReceipt>.Success(receipt);
        }

        public OperationResult<PcPowerStateReceipt> EvaluateCurrentPowerOn()
        {
            if (State != PcPowerState.Energized ||
                _activePowerOnReceipt == null)
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.NotEnergized);
            }

            if (!HasLiveInterlockBinding())
            {
                return OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.InterlockRejected);
            }

            OperationResult<PowerTestAttemptReceipt> currentPreflight =
                _powerTestAttempts.EvaluateCurrentReceipt();
            return currentPreflight.IsSuccess &&
                   ReferenceEquals(
                       currentPreflight.Value,
                       _activePowerOnReceipt.PreflightReceipt)
                ? OperationResult<PcPowerStateReceipt>.Success(
                    _activePowerOnReceipt)
                : OperationResult<PcPowerStateReceipt>.Fail(
                    PcPowerStateFailures.PreflightStale);
        }

        public OperationResult<PcPostStartupReceipt>
            TryCompleteStartupSelfTest(
                StableId<PcPostStartupOperationIdScope> operationId,
                PcPowerStateReceipt sourcePowerOnReceipt,
                long expectedPowerStateRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.InvalidOperationId);
            }

            if (_postStartupReceipts.TryGetValue(
                    operationId,
                    out PcPostStartupReceipt replay))
            {
                return replay.MatchesCommand(
                        operationId,
                        sourcePowerOnReceipt,
                        expectedPowerStateRevision)
                    ? OperationResult<PcPostStartupReceipt>.Success(replay)
                    : OperationResult<PcPostStartupReceipt>.Fail(
                        PcPostStartupFailures.OperationConflict);
            }

            if (expectedPowerStateRevision != Revision)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.PowerStateRevisionMismatch);
            }

            if (PostStartupRevision == long.MaxValue)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.RevisionOverflow);
            }

            if (State != PcPowerState.Energized ||
                _activePowerOnReceipt == null)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.NotCurrent);
            }

            if (sourcePowerOnReceipt == null ||
                !sourcePowerOnReceipt.IsOwnedBy(this) ||
                sourcePowerOnReceipt.TransitionKind !=
                    PcPowerTransitionKind.PowerOn ||
                !ReferenceEquals(
                    sourcePowerOnReceipt,
                    _activePowerOnReceipt))
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.InvalidPowerOnReceipt);
            }

            if (_activePostStartupReceipt != null)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.AlreadyCompleted);
            }

            OperationResult<PcPowerStateReceipt> currentPowerOn =
                EvaluateCurrentPowerOn();
            if (currentPowerOn.IsFailure)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    currentPowerOn.Error);
            }

            if (!ReferenceEquals(currentPowerOn.Value, sourcePowerOnReceipt))
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.InvalidPowerOnReceipt);
            }

            long nextPostStartupRevision = PostStartupRevision + 1L;
            var receipt = new PcPostStartupReceipt(
                this,
                operationId,
                sourcePowerOnReceipt,
                expectedPowerStateRevision,
                nextPostStartupRevision);
            _postStartupReceipts.Add(operationId, receipt);
            _postStartupReceiptsByRevision.Add(receipt);
            _activePostStartupReceipt = receipt;
            PostStartupRevision = nextPostStartupRevision;
            return OperationResult<PcPostStartupReceipt>.Success(receipt);
        }

        public OperationResult<PcPostStartupReceipt>
            EvaluateCurrentStartupSelfTest()
        {
            if (State != PcPowerState.Energized ||
                _activePowerOnReceipt == null ||
                _activePostStartupReceipt == null)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    PcPostStartupFailures.NotCurrent);
            }

            OperationResult<PcPowerStateReceipt> currentPowerOn =
                EvaluateCurrentPowerOn();
            if (currentPowerOn.IsFailure ||
                !ReferenceEquals(
                    currentPowerOn.Value,
                    _activePostStartupReceipt.SourcePowerOnReceipt) ||
                _activePostStartupReceipt.ExpectedPowerStateRevision != Revision)
            {
                return OperationResult<PcPostStartupReceipt>.Fail(
                    currentPowerOn.IsFailure
                        ? currentPowerOn.Error
                        : PcPostStartupFailures.NotCurrent);
            }

            return OperationResult<PcPostStartupReceipt>.Success(
                _activePostStartupReceipt);
        }

        public bool TryGetStartupSelfTestReceipt(
            StableId<PcPostStartupOperationIdScope> operationId,
            out PcPostStartupReceipt receipt)
        {
            return _postStartupReceipts.TryGetValue(operationId, out receipt);
        }

        public bool TryGetReceipt(
            StableId<PcPowerStateOperationIdScope> operationId,
            out PcPowerStateReceipt receipt)
        {
            return _receipts.TryGetValue(operationId, out receipt);
        }

        public OperationResult ValidateReceiptHistory()
        {
            if (Revision != _receipts.Count ||
                _receipts.Count != _receiptsByRevision.Count ||
                !_assemblyBuild.IsElectricalPowerStateBoundTo(this))
            {
                return OperationResult.Fail(
                    PcPowerStateFailures.ReceiptHistoryInvalid);
            }

            PcPowerState foldedState = PcPowerState.Off;
            PcPowerStateReceipt foldedPowerOn = null;
            for (int index = 0; index < _receiptsByRevision.Count; index++)
            {
                PcPowerStateReceipt receipt = _receiptsByRevision[index];
                long revision = index + 1L;
                if (receipt == null || !receipt.IsOwnedBy(this) ||
                    receipt.OperationId.IsEmpty ||
                    receipt.ExpectedRevision != revision - 1L ||
                    receipt.Revision != revision ||
                    !_receipts.TryGetValue(
                        receipt.OperationId,
                        out PcPowerStateReceipt mapped) ||
                    !ReferenceEquals(mapped, receipt) ||
                    receipt.PreflightReceipt == null ||
                    !_powerTestAttempts.TryGetReceipt(
                        receipt.PreflightReceipt.OperationId,
                        out PowerTestAttemptReceipt knownPreflight) ||
                    !ReferenceEquals(
                        knownPreflight,
                        receipt.PreflightReceipt))
                {
                    return OperationResult.Fail(
                        PcPowerStateFailures.ReceiptHistoryInvalid);
                }

                if (receipt.TransitionKind == PcPowerTransitionKind.PowerOn)
                {
                    if (foldedState != PcPowerState.Off ||
                        foldedPowerOn != null ||
                        receipt.SourcePowerOnReceipt != null ||
                        receipt.ResultingState != PcPowerState.Energized)
                    {
                        return OperationResult.Fail(
                            PcPowerStateFailures.ReceiptHistoryInvalid);
                    }

                    foldedState = PcPowerState.Energized;
                    foldedPowerOn = receipt;
                }
                else if (receipt.TransitionKind ==
                         PcPowerTransitionKind.PowerOff)
                {
                    if (foldedState != PcPowerState.Energized ||
                        foldedPowerOn == null ||
                        !ReferenceEquals(
                            receipt.SourcePowerOnReceipt,
                            foldedPowerOn) ||
                        !ReferenceEquals(
                            receipt.PreflightReceipt,
                            foldedPowerOn.PreflightReceipt) ||
                        receipt.ResultingState != PcPowerState.Off)
                    {
                        return OperationResult.Fail(
                            PcPowerStateFailures.ReceiptHistoryInvalid);
                    }

                    foldedState = PcPowerState.Off;
                    foldedPowerOn = null;
                }
                else
                {
                    return OperationResult.Fail(
                        PcPowerStateFailures.ReceiptHistoryInvalid);
                }
            }

            if (foldedState != State ||
                !ReferenceEquals(foldedPowerOn, _activePowerOnReceipt) ||
                _assemblyBuild.IsElectricallyEnergized != IsEnergized)
            {
                return OperationResult.Fail(
                    PcPowerStateFailures.ReceiptHistoryInvalid);
            }

            return ValidatePostStartupReceiptHistory();
        }

        private OperationResult ValidatePostStartupReceiptHistory()
        {
            if (PostStartupRevision != _postStartupReceipts.Count ||
                _postStartupReceipts.Count !=
                    _postStartupReceiptsByRevision.Count ||
                (State == PcPowerState.Off &&
                 _activePostStartupReceipt != null))
            {
                return OperationResult.Fail(
                    PcPostStartupFailures.ReceiptHistoryInvalid);
            }

            for (int index = 0;
                 index < _postStartupReceiptsByRevision.Count;
                 index++)
            {
                PcPostStartupReceipt receipt =
                    _postStartupReceiptsByRevision[index];
                long postRevision = index + 1L;
                PcPowerStateReceipt source = receipt?.SourcePowerOnReceipt;
                if (receipt == null || !receipt.IsOwnedBy(this) ||
                    receipt.OperationId.IsEmpty ||
                    receipt.Revision != postRevision || source == null ||
                    !source.IsOwnedBy(this) ||
                    source.TransitionKind != PcPowerTransitionKind.PowerOn ||
                    receipt.ExpectedPowerStateRevision != source.Revision ||
                    receipt.PowerStateRevision != source.Revision ||
                    !ReferenceEquals(
                        receipt.PreflightReceipt,
                        source.PreflightReceipt) ||
                    !_postStartupReceipts.TryGetValue(
                        receipt.OperationId,
                        out PcPostStartupReceipt mappedPost) ||
                    !ReferenceEquals(mappedPost, receipt) ||
                    !_receipts.TryGetValue(
                        source.OperationId,
                        out PcPowerStateReceipt mappedPowerOn) ||
                    !ReferenceEquals(mappedPowerOn, source))
                {
                    return OperationResult.Fail(
                        PcPostStartupFailures.ReceiptHistoryInvalid);
                }

                for (int previous = 0; previous < index; previous++)
                {
                    if (ReferenceEquals(
                        _postStartupReceiptsByRevision[previous]
                            .SourcePowerOnReceipt,
                        source))
                    {
                        return OperationResult.Fail(
                            PcPostStartupFailures.ReceiptHistoryInvalid);
                    }
                }
            }

            if (_activePostStartupReceipt == null)
            {
                return OperationResult.Success();
            }

            return State == PcPowerState.Energized &&
                   _activePowerOnReceipt != null &&
                   ReferenceEquals(
                       _activePostStartupReceipt.SourcePowerOnReceipt,
                       _activePowerOnReceipt) &&
                   _postStartupReceipts.TryGetValue(
                       _activePostStartupReceipt.OperationId,
                       out PcPostStartupReceipt activeMapped) &&
                   ReferenceEquals(activeMapped, _activePostStartupReceipt)
                ? OperationResult.Success()
                : OperationResult.Fail(
                    PcPostStartupFailures.ReceiptHistoryInvalid);
        }

        private bool HasLiveInterlockBinding()
        {
            return _assemblyBuild.IsElectricalPowerStateBoundTo(this) &&
                   _assemblyBuild.IsElectricallyEnergized == IsEnergized;
        }
    }
}
