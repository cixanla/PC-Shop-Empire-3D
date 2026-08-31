using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Owns bounded fictional driver receipts for exact installed fictional OS and
    /// storage identities. Completion needs a current energized POST/UEFI cycle;
    /// the completed result survives power-off and follows the same OS/storage item.
    /// </summary>
    public sealed class PcFictionalDriverInstallationAuthority
    {
        private readonly PcFictionalOsInstallationAuthority
            _fictionalOsInstallation;
        private readonly PcPowerStateAuthority _powerState;
        private readonly AssemblyBuildAuthority _assemblyBuild;
        private readonly Dictionary<
            StableId<PcFictionalDriverInstallationOperationIdScope>,
            PcFictionalDriverInstallationReceipt> _receipts =
                new Dictionary<
                    StableId<PcFictionalDriverInstallationOperationIdScope>,
                    PcFictionalDriverInstallationReceipt>();
        private readonly List<PcFictionalDriverInstallationReceipt>
            _receiptsByRevision =
                new List<PcFictionalDriverInstallationReceipt>();
        private readonly Dictionary<StableId<ItemInstanceIdScope>,
            PcFictionalDriverInstallationReceipt> _receiptsByStorageItem =
                new Dictionary<StableId<ItemInstanceIdScope>,
                    PcFictionalDriverInstallationReceipt>();

        private PcFictionalDriverInstallationAuthority(
            PcFictionalOsInstallationAuthority fictionalOsInstallation)
        {
            _fictionalOsInstallation = fictionalOsInstallation;
            _powerState = fictionalOsInstallation.PowerState;
            _assemblyBuild = fictionalOsInstallation.AssemblyBuild;
        }

        public PcFictionalOsInstallationAuthority FictionalOsInstallation =>
            _fictionalOsInstallation;

        public PcPowerStateAuthority PowerState => _powerState;

        public AssemblyBuildAuthority AssemblyBuild => _assemblyBuild;

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public static OperationResult<PcFictionalDriverInstallationAuthority>
            Create(PcFictionalOsInstallationAuthority fictionalOsInstallation)
        {
            if (fictionalOsInstallation == null ||
                fictionalOsInstallation.PowerState == null ||
                fictionalOsInstallation.AssemblyBuild == null)
            {
                return OperationResult<
                    PcFictionalDriverInstallationAuthority>.Fail(
                    PcFictionalDriverInstallationFailures.ConfigurationMissing);
            }

            if (!ReferenceEquals(
                    fictionalOsInstallation.PowerState.AssemblyBuild,
                    fictionalOsInstallation.AssemblyBuild))
            {
                return OperationResult<
                    PcFictionalDriverInstallationAuthority>.Fail(
                    PcFictionalDriverInstallationFailures.AuthorityMismatch);
            }

            return OperationResult<
                PcFictionalDriverInstallationAuthority>.Success(
                new PcFictionalDriverInstallationAuthority(
                    fictionalOsInstallation));
        }

        public OperationResult<PcFictionalDriverInstallationReceipt>
            TryCompleteInstallation(
                StableId<PcFictionalDriverInstallationOperationIdScope>
                    operationId,
                PcFictionalOsInstallationReceipt sourceOperatingSystemReceipt,
                PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
                StableId<ItemInstanceIdScope> expectedStorageItemId,
                long expectedPowerStateRevision,
                long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.InvalidOperationId);
            }

            if (_receipts.TryGetValue(
                    operationId,
                    out PcFictionalDriverInstallationReceipt replay))
            {
                return replay.MatchesCommand(
                        operationId,
                        sourceOperatingSystemReceipt,
                        sourceFirmwareBaselineReceipt,
                        expectedStorageItemId,
                        expectedPowerStateRevision,
                        expectedRevision)
                    ? OperationResult<
                        PcFictionalDriverInstallationReceipt>.Success(replay)
                    : OperationResult<
                        PcFictionalDriverInstallationReceipt>.Fail(
                        PcFictionalDriverInstallationFailures
                            .OperationConflict);
            }

            if (expectedStorageItemId.IsEmpty)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.InvalidStorageItem);
            }

            if (!_powerState.IsEnergized ||
                expectedPowerStateRevision != _powerState.Revision)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures
                        .PowerStateRevisionMismatch);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.RevisionOverflow);
            }

            OperationResult osHistory =
                _fictionalOsInstallation.ValidateReceiptHistory();
            if (osHistory.IsFailure)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures
                        .InvalidOperatingSystemReceipt);
            }

            if (sourceOperatingSystemReceipt == null ||
                !sourceOperatingSystemReceipt.IsOwnedBy(
                    _fictionalOsInstallation) ||
                !_fictionalOsInstallation.TryGetReceipt(
                    sourceOperatingSystemReceipt.OperationId,
                    out PcFictionalOsInstallationReceipt knownOs) ||
                !ReferenceEquals(knownOs, sourceOperatingSystemReceipt))
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures
                        .InvalidOperatingSystemReceipt);
            }

            OperationResult<PcFictionalOsInstallationReceipt> currentOs =
                _fictionalOsInstallation.EvaluateInstalledOperatingSystem();
            if (currentOs.IsFailure ||
                !ReferenceEquals(currentOs.Value, sourceOperatingSystemReceipt) ||
                sourceOperatingSystemReceipt.StorageItemId !=
                    expectedStorageItemId)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.NotCurrent);
            }

            if (sourceFirmwareBaselineReceipt == null ||
                !sourceFirmwareBaselineReceipt.IsOwnedBy(_powerState) ||
                !_powerState.TryGetFirmwareBaselineReceipt(
                    sourceFirmwareBaselineReceipt.OperationId,
                    out PcFirmwareBaselineReceipt knownFirmware) ||
                !ReferenceEquals(knownFirmware, sourceFirmwareBaselineReceipt))
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures
                        .InvalidFirmwareBaselineReceipt);
            }

            OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                _powerState.EvaluateCurrentFirmwareBaseline();
            if (currentFirmware.IsFailure ||
                !ReferenceEquals(
                    currentFirmware.Value,
                    sourceFirmwareBaselineReceipt) ||
                sourceFirmwareBaselineReceipt.PowerStateRevision !=
                    expectedPowerStateRevision)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.NotCurrent);
            }

            if (!MatchesImmutableSourceLineage(
                    sourceOperatingSystemReceipt,
                    sourceFirmwareBaselineReceipt) ||
                !MatchesCurrentHardwareLineage(
                    sourceOperatingSystemReceipt,
                    out ElectricalReadinessSnapshot currentReadiness) ||
                !MatchesCompletionHardwareLineage(
                    sourceFirmwareBaselineReceipt,
                    currentReadiness))
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures
                        .HardwareLineageNotCurrent);
            }

            if (_receiptsByStorageItem.ContainsKey(expectedStorageItemId))
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.AlreadyCompleted);
            }

            for (int index = 0; index < _receiptsByRevision.Count; index++)
            {
                if (ReferenceEquals(
                        _receiptsByRevision[index]
                            .SourceOperatingSystemReceipt,
                        sourceOperatingSystemReceipt))
                {
                    return OperationResult<
                        PcFictionalDriverInstallationReceipt>.Fail(
                        PcFictionalDriverInstallationFailures
                            .AlreadyCompleted);
                }
            }

            long nextRevision = Revision + 1L;
            var receipt = new PcFictionalDriverInstallationReceipt(
                this,
                operationId,
                sourceOperatingSystemReceipt,
                sourceFirmwareBaselineReceipt,
                currentReadiness.StorageSecureOperationId,
                currentReadiness.AssemblyRevision,
                expectedPowerStateRevision,
                expectedRevision,
                nextRevision);
            _receipts.Add(operationId, receipt);
            _receiptsByRevision.Add(receipt);
            _receiptsByStorageItem.Add(expectedStorageItemId, receipt);
            Revision = nextRevision;
            return OperationResult<
                PcFictionalDriverInstallationReceipt>.Success(receipt);
        }

        public OperationResult<PcFictionalDriverInstallationReceipt>
            EvaluateInstalledDrivers()
        {
            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(history.Error);
            }

            OperationResult<PcFictionalOsInstallationReceipt> currentOs =
                _fictionalOsInstallation.EvaluateInstalledOperatingSystem();
            if (currentOs.IsFailure)
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.NotCurrent);
            }

            if (!_receiptsByStorageItem.TryGetValue(
                    currentOs.Value.StorageItemId,
                    out PcFictionalDriverInstallationReceipt receipt))
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.NotInstalled);
            }

            if (!ReferenceEquals(
                    receipt.SourceOperatingSystemReceipt,
                    currentOs.Value))
            {
                return OperationResult<
                    PcFictionalDriverInstallationReceipt>.Fail(
                    PcFictionalDriverInstallationFailures.NotCurrent);
            }

            return OperationResult<
                PcFictionalDriverInstallationReceipt>.Success(receipt);
        }

        public bool TryGetReceipt(
            StableId<PcFictionalDriverInstallationOperationIdScope> operationId,
            out PcFictionalDriverInstallationReceipt receipt)
        {
            return _receipts.TryGetValue(operationId, out receipt);
        }

        public bool TryGetInstalledReceipt(
            StableId<ItemInstanceIdScope> storageItemId,
            out PcFictionalDriverInstallationReceipt receipt)
        {
            return _receiptsByStorageItem.TryGetValue(storageItemId, out receipt);
        }

        public OperationResult ValidateReceiptHistory()
        {
            if (Revision != _receipts.Count ||
                _receipts.Count != _receiptsByRevision.Count ||
                _receipts.Count != _receiptsByStorageItem.Count ||
                !ReferenceEquals(
                    _fictionalOsInstallation.PowerState,
                    _powerState) ||
                !ReferenceEquals(
                    _fictionalOsInstallation.AssemblyBuild,
                    _assemblyBuild) ||
                _fictionalOsInstallation.ValidateReceiptHistory().IsFailure)
            {
                return OperationResult.Fail(
                    PcFictionalDriverInstallationFailures
                        .ReceiptHistoryInvalid);
            }

            for (int index = 0; index < _receiptsByRevision.Count; index++)
            {
                PcFictionalDriverInstallationReceipt receipt =
                    _receiptsByRevision[index];
                long revision = index + 1L;
                PcFictionalOsInstallationReceipt sourceOs =
                    receipt?.SourceOperatingSystemReceipt;
                PcFirmwareBaselineReceipt sourceFirmware =
                    receipt?.SourceFirmwareBaselineReceipt;
                if (receipt == null || !receipt.IsOwnedBy(this) ||
                    receipt.OperationId.IsEmpty ||
                    receipt.StorageItemId.IsEmpty ||
                    receipt.StorageProductId.IsEmpty ||
                    receipt.InstallationStorageSecureOperationId.IsEmpty ||
                    receipt.InstallationAssemblyRevision <= 0 ||
                    receipt.ExpectedRevision != revision - 1L ||
                    receipt.Revision != revision || sourceOs == null ||
                    sourceFirmware == null ||
                    !sourceOs.IsOwnedBy(_fictionalOsInstallation) ||
                    !sourceFirmware.IsOwnedBy(_powerState) ||
                    receipt.SourceOperatingSystemRevision != sourceOs.Revision ||
                    receipt.SourceFirmwareBaselineRevision !=
                        sourceFirmware.Revision ||
                    receipt.ExpectedPowerStateRevision !=
                        sourceFirmware.PowerStateRevision ||
                    receipt.PowerStateRevision !=
                        sourceFirmware.PowerStateRevision ||
                    !ReferenceEquals(
                        receipt.SourcePostStartupReceipt,
                        sourceFirmware.SourcePostStartupReceipt) ||
                    !ReferenceEquals(
                        receipt.SourcePowerOnReceipt,
                        sourceFirmware.SourcePowerOnReceipt) ||
                    !ReferenceEquals(
                        receipt.PreflightReceipt,
                        sourceFirmware.PreflightReceipt) ||
                    !ReferenceEquals(
                        receipt.OperatingSystemPreflightReceipt,
                        sourceOs.PreflightReceipt) ||
                    !MatchesImmutableSourceLineage(sourceOs, sourceFirmware) ||
                    !MatchesCapturedCompletionLineage(
                        receipt,
                        sourceFirmware) ||
                    !MatchesHistoricalStorageSecureLineage(receipt) ||
                    !_receipts.TryGetValue(
                        receipt.OperationId,
                        out PcFictionalDriverInstallationReceipt mapped) ||
                    !ReferenceEquals(mapped, receipt) ||
                    !_receiptsByStorageItem.TryGetValue(
                        receipt.StorageItemId,
                        out PcFictionalDriverInstallationReceipt
                            mappedStorage) ||
                    !ReferenceEquals(mappedStorage, receipt) ||
                    !_fictionalOsInstallation.TryGetReceipt(
                        sourceOs.OperationId,
                        out PcFictionalOsInstallationReceipt mappedOs) ||
                    !ReferenceEquals(mappedOs, sourceOs) ||
                    !_powerState.TryGetFirmwareBaselineReceipt(
                        sourceFirmware.OperationId,
                        out PcFirmwareBaselineReceipt mappedFirmware) ||
                    !ReferenceEquals(mappedFirmware, sourceFirmware))
                {
                    return OperationResult.Fail(
                        PcFictionalDriverInstallationFailures
                            .ReceiptHistoryInvalid);
                }

                for (int previous = 0; previous < index; previous++)
                {
                    PcFictionalDriverInstallationReceipt candidate =
                        _receiptsByRevision[previous];
                    if (candidate.StorageItemId == receipt.StorageItemId ||
                        ReferenceEquals(
                            candidate.SourceOperatingSystemReceipt,
                            sourceOs))
                    {
                        return OperationResult.Fail(
                            PcFictionalDriverInstallationFailures
                                .ReceiptHistoryInvalid);
                    }
                }
            }

            return OperationResult.Success();
        }

        private bool MatchesImmutableSourceLineage(
            PcFictionalOsInstallationReceipt sourceOperatingSystemReceipt,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt)
        {
            PowerTestAttemptContext osContext =
                sourceOperatingSystemReceipt?.PreflightReceipt?.Context;
            PowerTestAttemptContext firmwareContext =
                sourceFirmwareBaselineReceipt?.PreflightReceipt?.Context;
            ElectricalReadinessSnapshot readiness =
                osContext?.ElectricalReadiness;
            PcPowerBudgetSnapshot budget = osContext?.PowerBudget;
            return osContext != null && firmwareContext != null &&
                   readiness != null && budget != null &&
                   osContext.Matches(firmwareContext) &&
                   sourceOperatingSystemReceipt.StorageItemId ==
                       readiness.StorageItemId &&
                   sourceOperatingSystemReceipt.StorageProductId ==
                       budget.StorageProductId &&
                   sourceOperatingSystemReceipt
                       .SourceStorageSecureOperationId ==
                       readiness.StorageSecureOperationId &&
                   sourceOperatingSystemReceipt.SourceAssemblyRevision ==
                       readiness.AssemblyRevision;
        }

        private bool MatchesCurrentHardwareLineage(
            PcFictionalOsInstallationReceipt sourceOperatingSystemReceipt,
            out ElectricalReadinessSnapshot current)
        {
            current = null;
            ElectricalReadinessSnapshot source =
                sourceOperatingSystemReceipt?.PreflightReceipt?.Context
                    ?.ElectricalReadiness;
            PcPowerBudgetSnapshot sourceBudget =
                sourceOperatingSystemReceipt?.PreflightReceipt?.Context
                    ?.PowerBudget;
            OperationResult<ElectricalReadinessSnapshot> observed =
                _assemblyBuild.EvaluateElectricalReadiness();
            if (source == null || sourceBudget == null || observed.IsFailure)
            {
                return false;
            }

            current = observed.Value;
            return current.BuildId == source.BuildId &&
                   current.ChassisId == source.ChassisId &&
                   current.MotherboardItemId == source.MotherboardItemId &&
                   current.ProcessorItemId == source.ProcessorItemId &&
                   current.MemoryItemId == source.MemoryItemId &&
                   current.StorageItemId == source.StorageItemId &&
                   current.ProcessorCoolerItemId ==
                       source.ProcessorCoolerItemId &&
                   current.GraphicsCardItemId == source.GraphicsCardItemId &&
                   current.PowerSupplyItemId == source.PowerSupplyItemId &&
                   current.Atx24PowerCableItemId ==
                       source.Atx24PowerCableItemId &&
                   current.Eps12vPowerCableItemId ==
                       source.Eps12vPowerCableItemId &&
                   current.PcieGpuPowerCableItemId ==
                       source.PcieGpuPowerCableItemId &&
                   current.MotherboardSecureOperationId ==
                       source.MotherboardSecureOperationId &&
                   current.ProcessorRetainOperationId ==
                       source.ProcessorRetainOperationId &&
                   current.MemoryRetainOperationId ==
                       source.MemoryRetainOperationId &&
                   current.ProcessorCoolerRetainOperationId ==
                       source.ProcessorCoolerRetainOperationId &&
                   current.GraphicsCardRetainOperationId ==
                       source.GraphicsCardRetainOperationId &&
                   current.PowerSupplyRetainOperationId ==
                       source.PowerSupplyRetainOperationId &&
                   current.Atx24RouteOperationId ==
                       source.Atx24RouteOperationId &&
                   current.Eps12vRouteOperationId ==
                       source.Eps12vRouteOperationId &&
                   current.PcieGpuRouteOperationId ==
                       source.PcieGpuRouteOperationId &&
                   current.Atx24PowerCableRevision ==
                       source.Atx24PowerCableRevision &&
                   current.Eps12vPowerCableRevision ==
                       source.Eps12vPowerCableRevision &&
                   current.PcieGpuPowerCableRevision ==
                       source.PcieGpuPowerCableRevision &&
                   _assemblyBuild.MotherboardProductId ==
                       sourceBudget.MotherboardProductId &&
                   _assemblyBuild.ProcessorProductId ==
                       sourceBudget.ProcessorProductId &&
                   _assemblyBuild.MemoryProductId ==
                       sourceBudget.MemoryProductId &&
                   _assemblyBuild.StorageProductId ==
                       sourceBudget.StorageProductId &&
                   _assemblyBuild.ProcessorCoolerProductId ==
                       sourceBudget.ProcessorCoolerProductId &&
                   _assemblyBuild.GraphicsCardProductId ==
                       sourceBudget.GraphicsCardProductId &&
                   _assemblyBuild.PowerSupplyProductId ==
                       sourceBudget.PowerSupplyProductId &&
                   HasValidStorageSecureLineage(
                       current.StorageSecureOperationId,
                       current.StorageItemId,
                       _assemblyBuild.StorageProductId,
                       current.AssemblyRevision);
        }

        private static bool MatchesCompletionHardwareLineage(
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            ElectricalReadinessSnapshot currentReadiness)
        {
            ElectricalReadinessSnapshot sourceReadiness =
                sourceFirmwareBaselineReceipt?.PreflightReceipt?.Context
                    ?.ElectricalReadiness;
            return sourceReadiness != null && currentReadiness != null &&
                   currentReadiness.StorageSecureOperationId ==
                       sourceReadiness.StorageSecureOperationId &&
                   currentReadiness.AssemblyRevision ==
                       sourceReadiness.AssemblyRevision;
        }

        private static bool MatchesCapturedCompletionLineage(
            PcFictionalDriverInstallationReceipt receipt,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt)
        {
            ElectricalReadinessSnapshot sourceReadiness =
                sourceFirmwareBaselineReceipt?.PreflightReceipt?.Context
                    ?.ElectricalReadiness;
            return receipt != null && sourceReadiness != null &&
                   receipt.InstallationStorageSecureOperationId ==
                       sourceReadiness.StorageSecureOperationId &&
                   receipt.InstallationAssemblyRevision ==
                       sourceReadiness.AssemblyRevision;
        }

        private bool MatchesHistoricalStorageSecureLineage(
            PcFictionalDriverInstallationReceipt receipt)
        {
            return receipt != null && HasValidStorageSecureLineage(
                receipt.InstallationStorageSecureOperationId,
                receipt.StorageItemId,
                receipt.StorageProductId,
                receipt.InstallationAssemblyRevision);
        }

        private bool HasValidStorageSecureLineage(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            long assemblyRevision)
        {
            return !operationId.IsEmpty && !itemId.IsEmpty &&
                   !productId.IsEmpty && assemblyRevision > 0 &&
                   _assemblyBuild.TryGetReceipt(
                       operationId,
                       out AssemblyOperationReceipt receipt) &&
                   receipt.OperationId == operationId &&
                   receipt.OperationKind ==
                       AssemblyOperationKind.SecureStorageDevice &&
                   receipt.ItemId == itemId &&
                   receipt.ProductId == productId &&
                   receipt.AssemblyRevision > 0 &&
                   receipt.AssemblyRevision <= assemblyRevision;
        }
    }
}
