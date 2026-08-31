using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Owns fictional OS installation receipts for exact physical storage items.
    /// Installation requires a current UEFI baseline; the installed result persists
    /// independently of the active power cycle and follows the same storage identity.
    /// </summary>
    public sealed class PcFictionalOsInstallationAuthority
    {
        private readonly PcPowerStateAuthority _powerState;
        private readonly AssemblyBuildAuthority _assemblyBuild;
        private readonly Dictionary<
            StableId<PcFictionalOsInstallationOperationIdScope>,
            PcFictionalOsInstallationReceipt> _receipts =
                new Dictionary<
                    StableId<PcFictionalOsInstallationOperationIdScope>,
                    PcFictionalOsInstallationReceipt>();
        private readonly List<PcFictionalOsInstallationReceipt>
            _receiptsByRevision =
                new List<PcFictionalOsInstallationReceipt>();
        private readonly Dictionary<StableId<ItemInstanceIdScope>,
            PcFictionalOsInstallationReceipt> _receiptsByStorageItem =
                new Dictionary<StableId<ItemInstanceIdScope>,
                    PcFictionalOsInstallationReceipt>();

        private PcFictionalOsInstallationAuthority(
            PcPowerStateAuthority powerState,
            AssemblyBuildAuthority assemblyBuild)
        {
            _powerState = powerState;
            _assemblyBuild = assemblyBuild;
        }

        public PcPowerStateAuthority PowerState => _powerState;

        public AssemblyBuildAuthority AssemblyBuild => _assemblyBuild;

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public static OperationResult<PcFictionalOsInstallationAuthority> Create(
            PcPowerStateAuthority powerState,
            AssemblyBuildAuthority assemblyBuild)
        {
            if (powerState == null || assemblyBuild == null)
            {
                return OperationResult<PcFictionalOsInstallationAuthority>.Fail(
                    PcFictionalOsInstallationFailures.ConfigurationMissing);
            }

            if (!ReferenceEquals(powerState.AssemblyBuild, assemblyBuild))
            {
                return OperationResult<PcFictionalOsInstallationAuthority>.Fail(
                    PcFictionalOsInstallationFailures.AuthorityMismatch);
            }

            return OperationResult<PcFictionalOsInstallationAuthority>.Success(
                new PcFictionalOsInstallationAuthority(
                    powerState,
                    assemblyBuild));
        }

        public OperationResult<PcFictionalOsInstallationReceipt>
            TryCompleteInstallation(
                StableId<PcFictionalOsInstallationOperationIdScope> operationId,
                PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
                StableId<ItemInstanceIdScope> expectedStorageItemId,
                long expectedPowerStateRevision,
                long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(
                    operationId,
                    out PcFictionalOsInstallationReceipt replay))
            {
                return replay.MatchesCommand(
                        operationId,
                        sourceFirmwareBaselineReceipt,
                        expectedStorageItemId,
                        expectedPowerStateRevision,
                        expectedRevision)
                    ? OperationResult<PcFictionalOsInstallationReceipt>.Success(
                        replay)
                    : OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                        PcFictionalOsInstallationFailures.OperationConflict);
            }

            if (expectedStorageItemId.IsEmpty)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.InvalidStorageItem);
            }

            if (expectedPowerStateRevision != _powerState.Revision)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures
                        .PowerStateRevisionMismatch);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.RevisionOverflow);
            }

            if (sourceFirmwareBaselineReceipt == null ||
                !sourceFirmwareBaselineReceipt.IsOwnedBy(_powerState) ||
                !_powerState.TryGetFirmwareBaselineReceipt(
                    sourceFirmwareBaselineReceipt.OperationId,
                    out PcFirmwareBaselineReceipt knownFirmware) ||
                !ReferenceEquals(
                    knownFirmware,
                    sourceFirmwareBaselineReceipt))
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures
                        .InvalidFirmwareBaselineReceipt);
            }

            OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                _powerState.EvaluateCurrentFirmwareBaseline();
            if (currentFirmware.IsFailure)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.NotCurrent);
            }

            if (!ReferenceEquals(
                    currentFirmware.Value,
                    sourceFirmwareBaselineReceipt))
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures
                        .InvalidFirmwareBaselineReceipt);
            }

            if (sourceFirmwareBaselineReceipt.PowerStateRevision !=
                    expectedPowerStateRevision ||
                _assemblyBuild.StorageSlotState !=
                    StorageSlotState.StorageDeviceSecured ||
                _assemblyBuild.StorageItemId != expectedStorageItemId ||
                _assemblyBuild.StorageProductId.IsEmpty ||
                _assemblyBuild.StorageSecuredByOperationId.IsEmpty ||
                _assemblyBuild.Revision <= 0 ||
                !MatchesSourceStorageLineage(
                    sourceFirmwareBaselineReceipt,
                    expectedStorageItemId,
                    _assemblyBuild.StorageProductId,
                    _assemblyBuild.StorageSecuredByOperationId,
                    _assemblyBuild.Revision))
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.StorageNotReady);
            }

            if (_receiptsByStorageItem.ContainsKey(expectedStorageItemId))
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.AlreadyCompleted);
            }

            for (int index = 0; index < _receiptsByRevision.Count; index++)
            {
                if (ReferenceEquals(
                        _receiptsByRevision[index]
                            .SourceFirmwareBaselineReceipt,
                        sourceFirmwareBaselineReceipt))
                {
                    return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                        PcFictionalOsInstallationFailures.AlreadyCompleted);
                }
            }

            long nextRevision = Revision + 1L;
            var receipt = new PcFictionalOsInstallationReceipt(
                this,
                operationId,
                sourceFirmwareBaselineReceipt,
                expectedStorageItemId,
                _assemblyBuild.StorageProductId,
                _assemblyBuild.StorageSecuredByOperationId,
                _assemblyBuild.Revision,
                expectedPowerStateRevision,
                expectedRevision,
                nextRevision);
            _receipts.Add(operationId, receipt);
            _receiptsByRevision.Add(receipt);
            _receiptsByStorageItem.Add(expectedStorageItemId, receipt);
            Revision = nextRevision;
            return OperationResult<PcFictionalOsInstallationReceipt>.Success(
                receipt);
        }

        public OperationResult<PcFictionalOsInstallationReceipt>
            EvaluateInstalledOperatingSystem()
        {
            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    history.Error);
            }

            if (_assemblyBuild.StorageSlotState !=
                    StorageSlotState.StorageDeviceSecured ||
                _assemblyBuild.StorageItemId.IsEmpty ||
                _assemblyBuild.StorageProductId.IsEmpty)
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.NotCurrent);
            }

            if (!_receiptsByStorageItem.TryGetValue(
                    _assemblyBuild.StorageItemId,
                    out PcFictionalOsInstallationReceipt receipt))
            {
                return OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.NotInstalled);
            }

            return receipt.StorageProductId == _assemblyBuild.StorageProductId
                ? OperationResult<PcFictionalOsInstallationReceipt>.Success(
                    receipt)
                : OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                    PcFictionalOsInstallationFailures.NotCurrent);
        }

        public bool TryGetReceipt(
            StableId<PcFictionalOsInstallationOperationIdScope> operationId,
            out PcFictionalOsInstallationReceipt receipt)
        {
            return _receipts.TryGetValue(operationId, out receipt);
        }

        public bool TryGetInstalledReceipt(
            StableId<ItemInstanceIdScope> storageItemId,
            out PcFictionalOsInstallationReceipt receipt)
        {
            return _receiptsByStorageItem.TryGetValue(
                storageItemId,
                out receipt);
        }

        public OperationResult ValidateReceiptHistory()
        {
            if (Revision != _receipts.Count ||
                _receipts.Count != _receiptsByRevision.Count ||
                _receipts.Count != _receiptsByStorageItem.Count ||
                !ReferenceEquals(_powerState.AssemblyBuild, _assemblyBuild) ||
                _powerState.ValidateReceiptHistory().IsFailure)
            {
                return OperationResult.Fail(
                    PcFictionalOsInstallationFailures.ReceiptHistoryInvalid);
            }

            for (int index = 0; index < _receiptsByRevision.Count; index++)
            {
                PcFictionalOsInstallationReceipt receipt =
                    _receiptsByRevision[index];
                long revision = index + 1L;
                PcFirmwareBaselineReceipt source =
                    receipt?.SourceFirmwareBaselineReceipt;
                if (receipt == null || !receipt.IsOwnedBy(this) ||
                    receipt.OperationId.IsEmpty ||
                    receipt.StorageItemId.IsEmpty ||
                    receipt.StorageProductId.IsEmpty ||
                    receipt.SourceStorageSecureOperationId.IsEmpty ||
                    receipt.SourceAssemblyRevision <= 0 ||
                    receipt.ExpectedRevision != revision - 1L ||
                    receipt.Revision != revision || source == null ||
                    !source.IsOwnedBy(_powerState) ||
                    receipt.SourceFirmwareBaselineRevision != source.Revision ||
                    receipt.ExpectedPowerStateRevision !=
                        source.PowerStateRevision ||
                    receipt.PowerStateRevision != source.PowerStateRevision ||
                    !ReferenceEquals(
                        receipt.SourcePostStartupReceipt,
                        source.SourcePostStartupReceipt) ||
                    !ReferenceEquals(
                        receipt.SourcePowerOnReceipt,
                        source.SourcePowerOnReceipt) ||
                    !ReferenceEquals(
                        receipt.PreflightReceipt,
                        source.PreflightReceipt) ||
                    !MatchesSourceStorageLineage(
                        source,
                        receipt.StorageItemId,
                        receipt.StorageProductId,
                        receipt.SourceStorageSecureOperationId,
                        receipt.SourceAssemblyRevision) ||
                    !_receipts.TryGetValue(
                        receipt.OperationId,
                        out PcFictionalOsInstallationReceipt mapped) ||
                    !ReferenceEquals(mapped, receipt) ||
                    !_receiptsByStorageItem.TryGetValue(
                        receipt.StorageItemId,
                        out PcFictionalOsInstallationReceipt mappedStorage) ||
                    !ReferenceEquals(mappedStorage, receipt) ||
                    !_powerState.TryGetFirmwareBaselineReceipt(
                        source.OperationId,
                        out PcFirmwareBaselineReceipt mappedFirmware) ||
                    !ReferenceEquals(mappedFirmware, source))
                {
                    return OperationResult.Fail(
                        PcFictionalOsInstallationFailures.ReceiptHistoryInvalid);
                }

                for (int previous = 0; previous < index; previous++)
                {
                    PcFictionalOsInstallationReceipt candidate =
                        _receiptsByRevision[previous];
                    if (candidate.StorageItemId == receipt.StorageItemId ||
                        ReferenceEquals(
                            candidate.SourceFirmwareBaselineReceipt,
                            source))
                    {
                        return OperationResult.Fail(
                            PcFictionalOsInstallationFailures
                                .ReceiptHistoryInvalid);
                    }
                }
            }

            return OperationResult.Success();
        }

        private bool MatchesSourceStorageLineage(
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            StableId<ItemInstanceIdScope> storageItemId,
            StableId<ProductDefinitionIdScope> storageProductId,
            StableId<AssemblyOperationIdScope> storageSecureOperationId,
            long assemblyRevision)
        {
            PowerTestAttemptContext context =
                sourceFirmwareBaselineReceipt?.PreflightReceipt?.Context;
            ElectricalReadinessSnapshot readiness =
                context?.ElectricalReadiness;
            PcPowerBudgetSnapshot budget = context?.PowerBudget;
            if (readiness == null || budget == null ||
                storageItemId.IsEmpty || storageProductId.IsEmpty ||
                storageSecureOperationId.IsEmpty || assemblyRevision <= 0 ||
                readiness.StorageItemId != storageItemId ||
                budget.StorageProductId != storageProductId ||
                readiness.StorageSecureOperationId !=
                    storageSecureOperationId ||
                readiness.AssemblyRevision != assemblyRevision)
            {
                return false;
            }

            return _assemblyBuild.TryGetReceipt(
                       storageSecureOperationId,
                       out AssemblyOperationReceipt secureReceipt) &&
                   secureReceipt.OperationId == storageSecureOperationId &&
                   secureReceipt.OperationKind ==
                       AssemblyOperationKind.SecureStorageDevice &&
                   secureReceipt.ItemId == storageItemId &&
                   secureReceipt.ProductId == storageProductId &&
                   secureReceipt.AssemblyRevision > 0 &&
                   secureReceipt.AssemblyRevision <= assemblyRevision;
        }
    }
}
