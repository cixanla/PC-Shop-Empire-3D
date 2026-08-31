using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Owns deterministic fictional validation receipts for an exact current driver,
    /// powered firmware cycle and serialized electrical build lineage. All upstream
    /// authorities are read-only sources; this authority mutates only its own ledger.
    /// </summary>
    public sealed class PcValidationAuthority
    {
        private readonly PcFictionalDriverInstallationAuthority _driverInstallation;
        private readonly PcPowerStateAuthority _powerState;
        private readonly AssemblyBuildAuthority _assemblyBuild;
        private readonly PcPowerBudgetAuthority _powerBudget;
        private readonly PcPerformanceCatalog _performanceCatalog;
        private readonly PcValidationProfile _profile;
        private readonly Dictionary<StableId<PcValidationOperationIdScope>,
            PcValidationReceipt> _receipts =
                new Dictionary<StableId<PcValidationOperationIdScope>,
                    PcValidationReceipt>();
        private readonly List<PcValidationReceipt> _receiptsByRevision =
            new List<PcValidationReceipt>();

        private PcValidationAuthority(
            PcFictionalDriverInstallationAuthority driverInstallation,
            PcPowerBudgetAuthority powerBudget,
            PcPerformanceCatalog performanceCatalog,
            PcValidationProfile profile)
        {
            _driverInstallation = driverInstallation;
            _powerState = driverInstallation.PowerState;
            _assemblyBuild = driverInstallation.AssemblyBuild;
            _powerBudget = powerBudget;
            _performanceCatalog = performanceCatalog;
            _profile = profile;
        }

        public PcFictionalDriverInstallationAuthority DriverInstallation =>
            _driverInstallation;

        public PcPowerStateAuthority PowerState => _powerState;

        public AssemblyBuildAuthority AssemblyBuild => _assemblyBuild;

        public PcPowerBudgetAuthority PowerBudget => _powerBudget;

        public PcPerformanceCatalog PerformanceCatalog => _performanceCatalog;

        public PcValidationProfile Profile => _profile;

        public long Revision { get; private set; }

        public int ReceiptCount => _receipts.Count;

        public static OperationResult<PcValidationAuthority> Create(
            PcFictionalDriverInstallationAuthority driverInstallation,
            PcPowerBudgetAuthority powerBudget,
            PcPerformanceCatalog performanceCatalog,
            PcValidationProfile profile)
        {
            if (driverInstallation == null ||
                driverInstallation.PowerState == null ||
                driverInstallation.AssemblyBuild == null ||
                powerBudget == null ||
                performanceCatalog == null ||
                profile == null)
            {
                return OperationResult<PcValidationAuthority>.Fail(
                    PcValidationFailures.ConfigurationMissing);
            }

            if (!ReferenceEquals(
                    driverInstallation.PowerState.AssemblyBuild,
                    driverInstallation.AssemblyBuild) ||
                !ReferenceEquals(
                    powerBudget.AssemblyBuild,
                    driverInstallation.AssemblyBuild))
            {
                return OperationResult<PcValidationAuthority>.Fail(
                    PcValidationFailures.AuthorityMismatch);
            }

            if (!ReferenceEquals(
                    performanceCatalog.OwnerComponentCatalog,
                    driverInstallation.AssemblyBuild.ComponentCatalog))
            {
                return OperationResult<PcValidationAuthority>.Fail(
                    PcValidationFailures.CatalogMismatch);
            }

            return OperationResult<PcValidationAuthority>.Success(
                new PcValidationAuthority(
                    driverInstallation,
                    powerBudget,
                    performanceCatalog,
                    profile));
        }

        public OperationResult<PcValidationReceipt> TryCompleteValidation(
            StableId<PcValidationOperationIdScope> operationId,
            PcFictionalDriverInstallationReceipt sourceDriverReceipt,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            long expectedPowerStateRevision,
            long expectedRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.InvalidOperationId);
            }

            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    history.Error);
            }

            if (_receipts.TryGetValue(
                    operationId,
                    out PcValidationReceipt replay))
            {
                return replay.MatchesCommand(
                        operationId,
                        sourceDriverReceipt,
                        sourceFirmwareBaselineReceipt,
                        expectedPowerStateRevision,
                        expectedRevision)
                    ? OperationResult<PcValidationReceipt>.Success(replay)
                    : OperationResult<PcValidationReceipt>.Fail(
                        PcValidationFailures.OperationConflict);
            }

            if (expectedRevision != Revision)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.RevisionMismatch);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.RevisionOverflow);
            }

            OperationResult<PcFictionalDriverInstallationReceipt> currentDriver =
                _driverInstallation.EvaluateInstalledDrivers();
            if (sourceDriverReceipt == null ||
                !sourceDriverReceipt.IsOwnedBy(_driverInstallation) ||
                !_driverInstallation.TryGetReceipt(
                    sourceDriverReceipt.OperationId,
                    out PcFictionalDriverInstallationReceipt knownDriver) ||
                !ReferenceEquals(knownDriver, sourceDriverReceipt) ||
                currentDriver.IsFailure ||
                !ReferenceEquals(currentDriver.Value, sourceDriverReceipt))
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.InvalidDriverReceipt);
            }

            if (!_powerState.IsEnergized ||
                expectedPowerStateRevision != _powerState.Revision)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.PowerStateRevisionMismatch);
            }

            OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                _powerState.EvaluateCurrentFirmwareBaseline();
            if (sourceFirmwareBaselineReceipt == null ||
                !sourceFirmwareBaselineReceipt.IsOwnedBy(_powerState) ||
                !_powerState.TryGetFirmwareBaselineReceipt(
                    sourceFirmwareBaselineReceipt.OperationId,
                    out PcFirmwareBaselineReceipt knownFirmware) ||
                !ReferenceEquals(knownFirmware, sourceFirmwareBaselineReceipt) ||
                currentFirmware.IsFailure ||
                !ReferenceEquals(
                    currentFirmware.Value,
                    sourceFirmwareBaselineReceipt))
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.InvalidFirmwareBaselineReceipt);
            }

            OperationResult benchmarkReadiness =
                _assemblyBuild.EvaluateBenchmarkReadiness();
            if (benchmarkReadiness.IsFailure)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.BenchmarkReadinessRejected);
            }

            OperationResult<ElectricalReadinessSnapshot> currentElectrical =
                _assemblyBuild.EvaluateElectricalReadiness();
            OperationResult<PcPowerBudgetSnapshot> currentBudget =
                _powerBudget.AssessPowerBudget();
            PowerTestAttemptContext context =
                sourceFirmwareBaselineReceipt.PreflightReceipt?.Context;
            if (currentElectrical.IsFailure || currentBudget.IsFailure ||
                context?.ElectricalReadiness == null ||
                context.PowerBudget == null ||
                !MatchesDriverAndFirmwareLineage(
                    sourceDriverReceipt,
                    sourceFirmwareBaselineReceipt) ||
                !MatchesElectricalReadiness(
                    currentElectrical.Value,
                    context.ElectricalReadiness) ||
                !MatchesPowerBudget(currentBudget.Value, context.PowerBudget))
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.NotCurrent);
            }

            OperationResult<PcValidationComputedMetrics> calculated =
                CalculateMetrics(context.PowerBudget);
            if (calculated.IsFailure)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    calculated.Error);
            }

            long nextRevision = Revision + 1L;
            var receipt = new PcValidationReceipt(
                this,
                operationId,
                sourceDriverReceipt,
                sourceFirmwareBaselineReceipt,
                context.ElectricalReadiness,
                context.PowerBudget,
                _performanceCatalog.CatalogId,
                _profile.Id,
                calculated.Value,
                _profile.StressSteps,
                expectedPowerStateRevision,
                expectedRevision,
                nextRevision);
            _receipts.Add(operationId, receipt);
            _receiptsByRevision.Add(receipt);
            Revision = nextRevision;
            return OperationResult<PcValidationReceipt>.Success(receipt);
        }

        public OperationResult<PcValidationReceipt> EvaluateCurrentValidation()
        {
            OperationResult history = ValidateReceiptHistory();
            if (history.IsFailure)
            {
                return OperationResult<PcValidationReceipt>.Fail(history.Error);
            }

            if (_receiptsByRevision.Count == 0)
            {
                return OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.NotCurrent);
            }

            PcValidationReceipt receipt =
                _receiptsByRevision[_receiptsByRevision.Count - 1];
            return MatchesCurrentContext(receipt)
                ? OperationResult<PcValidationReceipt>.Success(receipt)
                : OperationResult<PcValidationReceipt>.Fail(
                    PcValidationFailures.NotCurrent);
        }

        public bool TryGetReceipt(
            StableId<PcValidationOperationIdScope> operationId,
            out PcValidationReceipt receipt)
        {
            return _receipts.TryGetValue(operationId, out receipt);
        }

        public OperationResult ValidateReceiptHistory()
        {
            OperationResult upstream = ValidateUpstreamHistory();
            if (upstream.IsFailure ||
                Revision != _receipts.Count ||
                _receipts.Count != _receiptsByRevision.Count)
            {
                return OperationResult.Fail(
                    PcValidationFailures.ReceiptHistoryInvalid);
            }

            for (int index = 0; index < _receiptsByRevision.Count; index++)
            {
                PcValidationReceipt receipt = _receiptsByRevision[index];
                long revision = index + 1L;
                PcFictionalDriverInstallationReceipt sourceDriver =
                    receipt?.SourceDriverReceipt;
                PcFirmwareBaselineReceipt sourceFirmware =
                    receipt?.SourceFirmwareBaselineReceipt;
                PowerTestAttemptContext sourceContext =
                    sourceFirmware?.PreflightReceipt?.Context;
                if (receipt == null ||
                    !receipt.IsOwnedBy(this) ||
                    receipt.OperationId.IsEmpty ||
                    receipt.PerformanceCatalogId !=
                        _performanceCatalog.CatalogId ||
                    receipt.ProfileId != _profile.Id ||
                    receipt.ExpectedRevision != revision - 1L ||
                    receipt.Revision != revision ||
                    receipt.ExpectedPowerStateRevision <= 0 ||
                    sourceDriver == null ||
                    sourceFirmware == null ||
                    sourceContext?.ElectricalReadiness == null ||
                    sourceContext.PowerBudget == null ||
                    !sourceDriver.IsOwnedBy(_driverInstallation) ||
                    !sourceFirmware.IsOwnedBy(_powerState) ||
                    !_driverInstallation.TryGetReceipt(
                        sourceDriver.OperationId,
                        out PcFictionalDriverInstallationReceipt mappedDriver) ||
                    !ReferenceEquals(mappedDriver, sourceDriver) ||
                    !_powerState.TryGetFirmwareBaselineReceipt(
                        sourceFirmware.OperationId,
                        out PcFirmwareBaselineReceipt mappedFirmware) ||
                    !ReferenceEquals(mappedFirmware, sourceFirmware) ||
                    !ReferenceEquals(
                        receipt.SourceElectricalReadiness,
                        sourceContext.ElectricalReadiness) ||
                    !ReferenceEquals(
                        receipt.SourcePowerBudget,
                        sourceContext.PowerBudget) ||
                    !MatchesDriverAndFirmwareLineage(
                        sourceDriver,
                        sourceFirmware) ||
                    receipt.ExpectedPowerStateRevision !=
                        sourceFirmware.PowerStateRevision ||
                    !_receipts.TryGetValue(
                        receipt.OperationId,
                        out PcValidationReceipt mapped) ||
                    !ReferenceEquals(mapped, receipt))
                {
                    return OperationResult.Fail(
                        PcValidationFailures.ReceiptHistoryInvalid);
                }

                OperationResult<PcValidationComputedMetrics> calculated =
                    CalculateMetrics(receipt.SourcePowerBudget);
                if (calculated.IsFailure ||
                    !MatchesMetrics(receipt, calculated.Value))
                {
                    return OperationResult.Fail(
                        PcValidationFailures.ReceiptHistoryInvalid);
                }
            }

            return OperationResult.Success();
        }

        private OperationResult ValidateUpstreamHistory()
        {
            if (!ReferenceEquals(
                    _driverInstallation.PowerState,
                    _powerState) ||
                !ReferenceEquals(
                    _driverInstallation.AssemblyBuild,
                    _assemblyBuild) ||
                !ReferenceEquals(_powerBudget.AssemblyBuild, _assemblyBuild) ||
                !ReferenceEquals(
                    _performanceCatalog.OwnerComponentCatalog,
                    _assemblyBuild.ComponentCatalog))
            {
                return OperationResult.Fail(
                    PcValidationFailures.AuthorityMismatch);
            }

            if (_driverInstallation.ValidateReceiptHistory().IsFailure ||
                _powerState.ValidateReceiptHistory().IsFailure)
            {
                return OperationResult.Fail(
                    PcValidationFailures.ReceiptHistoryInvalid);
            }

            return OperationResult.Success();
        }

        private bool MatchesCurrentContext(PcValidationReceipt receipt)
        {
            if (receipt == null ||
                !_powerState.IsEnergized ||
                receipt.ExpectedPowerStateRevision != _powerState.Revision ||
                _assemblyBuild.EvaluateBenchmarkReadiness().IsFailure)
            {
                return false;
            }

            OperationResult<PcFictionalDriverInstallationReceipt> currentDriver =
                _driverInstallation.EvaluateInstalledDrivers();
            OperationResult<PcFirmwareBaselineReceipt> currentFirmware =
                _powerState.EvaluateCurrentFirmwareBaseline();
            OperationResult<ElectricalReadinessSnapshot> currentElectrical =
                _assemblyBuild.EvaluateElectricalReadiness();
            OperationResult<PcPowerBudgetSnapshot> currentBudget =
                _powerBudget.AssessPowerBudget();
            return currentDriver.IsSuccess &&
                   currentFirmware.IsSuccess &&
                   currentElectrical.IsSuccess &&
                   currentBudget.IsSuccess &&
                   ReferenceEquals(
                       currentDriver.Value,
                       receipt.SourceDriverReceipt) &&
                   ReferenceEquals(
                       currentFirmware.Value,
                       receipt.SourceFirmwareBaselineReceipt) &&
                   MatchesElectricalReadiness(
                       currentElectrical.Value,
                       receipt.SourceElectricalReadiness) &&
                   MatchesPowerBudget(
                       currentBudget.Value,
                       receipt.SourcePowerBudget);
        }

        private OperationResult<PcValidationComputedMetrics> CalculateMetrics(
            PcPowerBudgetSnapshot budget)
        {
            if (budget == null || !budget.IsSufficient ||
                budget.CapacityMarginWatts < _profile.MinimumPowerMarginWatts)
            {
                return OperationResult<PcValidationComputedMetrics>.Fail(
                    PcValidationFailures.PowerMarginInsufficient);
            }

            OperationResult<PcPerformanceSpecification> motherboard = Resolve(
                budget.MotherboardProductId,
                PcComponentKind.Motherboard);
            OperationResult<PcPerformanceSpecification> processor = Resolve(
                budget.ProcessorProductId,
                PcComponentKind.Processor);
            OperationResult<PcPerformanceSpecification> memory = Resolve(
                budget.MemoryProductId,
                PcComponentKind.MemoryModule);
            OperationResult<PcPerformanceSpecification> storage = Resolve(
                budget.StorageProductId,
                PcComponentKind.StorageDevice);
            OperationResult<PcPerformanceSpecification> cooler = Resolve(
                budget.ProcessorCoolerProductId,
                PcComponentKind.ProcessorCooler);
            OperationResult<PcPerformanceSpecification> graphics = Resolve(
                budget.GraphicsCardProductId,
                PcComponentKind.GraphicsCard);
            OperationResult<PcPerformanceSpecification> powerSupply = Resolve(
                budget.PowerSupplyProductId,
                PcComponentKind.PowerSupply);
            if (motherboard.IsFailure || processor.IsFailure || memory.IsFailure ||
                storage.IsFailure || cooler.IsFailure || graphics.IsFailure ||
                powerSupply.IsFailure)
            {
                return OperationResult<PcValidationComputedMetrics>.Fail(
                    PcValidationFailures.PerformanceProfileMissing);
            }

            long score = (long)motherboard.Value.PerformanceScore +
                         processor.Value.PerformanceScore +
                         memory.Value.PerformanceScore +
                         storage.Value.PerformanceScore +
                         cooler.Value.PerformanceScore +
                         graphics.Value.PerformanceScore +
                         powerSupply.Value.PerformanceScore;
            if (score <= 0 || score > PcValidationProfile.MaximumScore)
            {
                return OperationResult<PcValidationComputedMetrics>.Fail(
                    PcValidationFailures.ArithmeticOverflow);
            }

            if (score < _profile.MinimumBenchmarkScore)
            {
                return OperationResult<PcValidationComputedMetrics>.Fail(
                    PcValidationFailures.ScoreBelowMinimum);
            }

            OperationResult<int> processorPeak = CalculatePeakTemperature(
                processor.Value.ThermalLoadWatts,
                cooler.Value.CoolingCapacityWatts);
            OperationResult<int> graphicsPeak = CalculatePeakTemperature(
                graphics.Value.ThermalLoadWatts,
                graphics.Value.CoolingCapacityWatts);
            if (processorPeak.IsFailure || graphicsPeak.IsFailure)
            {
                return OperationResult<PcValidationComputedMetrics>.Fail(
                    PcValidationFailures.ArithmeticOverflow);
            }

            if (processorPeak.Value >
                _profile.MaximumProcessorTemperatureCelsius)
            {
                return OperationResult<PcValidationComputedMetrics>.Fail(
                    PcValidationFailures.ProcessorThermalLimitExceeded);
            }

            if (graphicsPeak.Value >
                _profile.MaximumGraphicsCardTemperatureCelsius)
            {
                return OperationResult<PcValidationComputedMetrics>.Fail(
                    PcValidationFailures.GraphicsThermalLimitExceeded);
            }

            return OperationResult<PcValidationComputedMetrics>.Success(
                new PcValidationComputedMetrics(
                    (int)score,
                    processor.Value.PerformanceScore,
                    graphics.Value.PerformanceScore,
                    processorPeak.Value,
                    graphicsPeak.Value,
                    budget.SystemPowerDrawWatts,
                    budget.MinimumRecommendedPsuWatts,
                    budget.InstalledPsuWatts,
                    budget.CapacityMarginWatts,
                    _profile.ResolveQualityTier((int)score)));
        }

        private OperationResult<PcPerformanceSpecification> Resolve(
            StableId<ProductDefinitionIdScope> productId,
            PcComponentKind expectedKind)
        {
            if (!_performanceCatalog.TryGet(
                    productId,
                    out PcPerformanceSpecification specification))
            {
                return OperationResult<PcPerformanceSpecification>.Fail(
                    PcValidationFailures.PerformanceProfileMissing);
            }

            return specification.ComponentKind == expectedKind
                ? OperationResult<PcPerformanceSpecification>.Success(specification)
                : OperationResult<PcPerformanceSpecification>.Fail(
                    PcValidationFailures.PerformanceProfileKindMismatch);
        }

        private OperationResult<int> CalculatePeakTemperature(
            int thermalLoadWatts,
            int coolingCapacityWatts)
        {
            if (thermalLoadWatts <= 0 || coolingCapacityWatts <= 0)
            {
                return OperationResult<int>.Fail(
                    PcValidationFailures.ArithmeticOverflow);
            }

            long numerator =
                (long)thermalLoadWatts * _profile.ThermalRiseScale;
            long rise = (numerator + coolingCapacityWatts - 1L) /
                        coolingCapacityWatts;
            long peak = _profile.AmbientTemperatureCelsius + rise;
            return peak > _profile.AmbientTemperatureCelsius &&
                   peak <= PcValidationProfile.MaximumTemperatureCelsius
                ? OperationResult<int>.Success((int)peak)
                : OperationResult<int>.Fail(
                    PcValidationFailures.ArithmeticOverflow);
        }

        private bool MatchesMetrics(
            PcValidationReceipt receipt,
            PcValidationComputedMetrics metrics)
        {
            return receipt.Result == PcValidationResult.PassedForQualityStage &&
                   receipt.StressResult == PcStressResult.Stable &&
                   receipt.StressSteps == _profile.StressSteps &&
                   receipt.BenchmarkScore == metrics.BenchmarkScore &&
                   receipt.ProcessorScore == metrics.ProcessorScore &&
                   receipt.GraphicsCardScore == metrics.GraphicsCardScore &&
                   receipt.ProcessorPeakTemperatureCelsius ==
                       metrics.ProcessorPeakTemperatureCelsius &&
                   receipt.GraphicsCardPeakTemperatureCelsius ==
                       metrics.GraphicsCardPeakTemperatureCelsius &&
                   receipt.SystemPowerDrawWatts == metrics.SystemPowerDrawWatts &&
                   receipt.MinimumRecommendedPsuWatts ==
                       metrics.MinimumRecommendedPsuWatts &&
                   receipt.InstalledPsuWatts == metrics.InstalledPsuWatts &&
                   receipt.PowerMarginWatts == metrics.PowerMarginWatts &&
                   receipt.QualityTier == metrics.QualityTier;
        }

        private static bool MatchesDriverAndFirmwareLineage(
            PcFictionalDriverInstallationReceipt driver,
            PcFirmwareBaselineReceipt firmware)
        {
            PowerTestAttemptContext driverContext =
                driver?.SourceOperatingSystemReceipt?.PreflightReceipt?.Context;
            PowerTestAttemptContext firmwareContext =
                firmware?.PreflightReceipt?.Context;
            return driverContext?.ElectricalReadiness != null &&
                   driverContext.PowerBudget != null &&
                   firmwareContext?.ElectricalReadiness != null &&
                   firmwareContext.PowerBudget != null &&
                   driver.StorageItemId ==
                       firmwareContext.ElectricalReadiness.StorageItemId &&
                   driver.StorageProductId ==
                       firmwareContext.PowerBudget.StorageProductId &&
                   MatchesElectricalReadiness(
                       driverContext.ElectricalReadiness,
                       firmwareContext.ElectricalReadiness) &&
                   MatchesPowerBudget(
                       driverContext.PowerBudget,
                       firmwareContext.PowerBudget);
        }

        private static bool MatchesPowerBudget(
            PcPowerBudgetSnapshot left,
            PcPowerBudgetSnapshot right)
        {
            return left != null && right != null &&
                   MatchesElectricalReadiness(
                       left.ElectricalReadiness,
                       right.ElectricalReadiness) &&
                   left.PolicyId == right.PolicyId &&
                   left.MotherboardProductId == right.MotherboardProductId &&
                   left.ProcessorProductId == right.ProcessorProductId &&
                   left.MemoryProductId == right.MemoryProductId &&
                   left.StorageProductId == right.StorageProductId &&
                   left.ProcessorCoolerProductId ==
                       right.ProcessorCoolerProductId &&
                   left.GraphicsCardProductId == right.GraphicsCardProductId &&
                   left.PowerSupplyProductId == right.PowerSupplyProductId &&
                   left.PlatformBaseLoadWatts == right.PlatformBaseLoadWatts &&
                   left.ChassisLoadWatts == right.ChassisLoadWatts &&
                   left.ProcessorLoadWatts == right.ProcessorLoadWatts &&
                   left.MemoryLoadWatts == right.MemoryLoadWatts &&
                   left.StorageLoadWatts == right.StorageLoadWatts &&
                   left.ProcessorCoolerLoadWatts ==
                       right.ProcessorCoolerLoadWatts &&
                   left.GraphicsCardLoadWatts == right.GraphicsCardLoadWatts &&
                   left.SystemPowerDrawWatts == right.SystemPowerDrawWatts &&
                   left.MinimumRecommendedPsuWatts ==
                       right.MinimumRecommendedPsuWatts &&
                   left.InstalledPsuWatts == right.InstalledPsuWatts &&
                   left.CapacityMarginWatts == right.CapacityMarginWatts &&
                   left.Blocker == right.Blocker;
        }

        private static bool MatchesElectricalReadiness(
            ElectricalReadinessSnapshot left,
            ElectricalReadinessSnapshot right)
        {
            return left != null && right != null &&
                   left.BuildId == right.BuildId &&
                   left.ChassisId == right.ChassisId &&
                   left.MotherboardItemId == right.MotherboardItemId &&
                   left.ProcessorItemId == right.ProcessorItemId &&
                   left.MemoryItemId == right.MemoryItemId &&
                   left.StorageItemId == right.StorageItemId &&
                   left.ProcessorCoolerItemId == right.ProcessorCoolerItemId &&
                   left.GraphicsCardItemId == right.GraphicsCardItemId &&
                   left.PowerSupplyItemId == right.PowerSupplyItemId &&
                   left.Atx24PowerCableItemId == right.Atx24PowerCableItemId &&
                   left.Eps12vPowerCableItemId == right.Eps12vPowerCableItemId &&
                   left.PcieGpuPowerCableItemId ==
                       right.PcieGpuPowerCableItemId &&
                   left.MotherboardSecureOperationId ==
                       right.MotherboardSecureOperationId &&
                   left.ProcessorRetainOperationId ==
                       right.ProcessorRetainOperationId &&
                   left.MemoryRetainOperationId ==
                       right.MemoryRetainOperationId &&
                   left.StorageSecureOperationId ==
                       right.StorageSecureOperationId &&
                   left.ProcessorCoolerRetainOperationId ==
                       right.ProcessorCoolerRetainOperationId &&
                   left.GraphicsCardRetainOperationId ==
                       right.GraphicsCardRetainOperationId &&
                   left.PowerSupplyRetainOperationId ==
                       right.PowerSupplyRetainOperationId &&
                   left.Atx24RouteOperationId == right.Atx24RouteOperationId &&
                   left.Eps12vRouteOperationId == right.Eps12vRouteOperationId &&
                   left.PcieGpuRouteOperationId ==
                       right.PcieGpuRouteOperationId &&
                   left.AssemblyRevision == right.AssemblyRevision &&
                   left.Atx24PowerCableRevision ==
                       right.Atx24PowerCableRevision &&
                   left.Eps12vPowerCableRevision ==
                       right.Eps12vPowerCableRevision &&
                   left.PcieGpuPowerCableRevision ==
                       right.PcieGpuPowerCableRevision;
        }
    }
}
