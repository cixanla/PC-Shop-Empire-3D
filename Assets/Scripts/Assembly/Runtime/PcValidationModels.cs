using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PcValidationOperationIdScope : IStableIdScope
    {
    }

    public sealed class PcValidationProfileIdScope : IStableIdScope
    {
    }

    public enum PcValidationResult
    {
        PassedForQualityStage = 1
    }

    public enum PcStressResult
    {
        Stable = 1
    }

    public enum PcQualityTier
    {
        Standard = 1,
        Good = 2,
        Excellent = 3
    }

    /// <summary>
    /// Versioned integer-only policy for one fictional validation suite. It deliberately
    /// avoids wall-clock, frame timing, random values and host-hardware probes so identical
    /// source receipts produce identical results on every supported platform.
    /// </summary>
    public sealed class PcValidationProfile
    {
        public const int MaximumStressSteps = 1_000_000;
        public const int MaximumTemperatureCelsius = 200;
        public const int MaximumThermalRiseScale = 10_000;
        public const int MaximumScore = 10_000_000;

        private PcValidationProfile(
            StableId<PcValidationProfileIdScope> id,
            int stressSteps,
            int ambientTemperatureCelsius,
            int thermalRiseScale,
            int maximumProcessorTemperatureCelsius,
            int maximumGraphicsCardTemperatureCelsius,
            int minimumPowerMarginWatts,
            int minimumBenchmarkScore,
            int goodBenchmarkScore,
            int excellentBenchmarkScore)
        {
            Id = id;
            StressSteps = stressSteps;
            AmbientTemperatureCelsius = ambientTemperatureCelsius;
            ThermalRiseScale = thermalRiseScale;
            MaximumProcessorTemperatureCelsius =
                maximumProcessorTemperatureCelsius;
            MaximumGraphicsCardTemperatureCelsius =
                maximumGraphicsCardTemperatureCelsius;
            MinimumPowerMarginWatts = minimumPowerMarginWatts;
            MinimumBenchmarkScore = minimumBenchmarkScore;
            GoodBenchmarkScore = goodBenchmarkScore;
            ExcellentBenchmarkScore = excellentBenchmarkScore;
        }

        public StableId<PcValidationProfileIdScope> Id { get; }

        public int StressSteps { get; }

        public int AmbientTemperatureCelsius { get; }

        public int ThermalRiseScale { get; }

        public int MaximumProcessorTemperatureCelsius { get; }

        public int MaximumGraphicsCardTemperatureCelsius { get; }

        public int MinimumPowerMarginWatts { get; }

        public int MinimumBenchmarkScore { get; }

        public int GoodBenchmarkScore { get; }

        public int ExcellentBenchmarkScore { get; }

        public static OperationResult<PcValidationProfile> Create(
            StableId<PcValidationProfileIdScope> id,
            int stressSteps,
            int ambientTemperatureCelsius,
            int thermalRiseScale,
            int maximumProcessorTemperatureCelsius,
            int maximumGraphicsCardTemperatureCelsius,
            int minimumPowerMarginWatts,
            int minimumBenchmarkScore,
            int goodBenchmarkScore,
            int excellentBenchmarkScore)
        {
            bool invalid = id.IsEmpty ||
                           stressSteps <= 0 ||
                           stressSteps > MaximumStressSteps ||
                           ambientTemperatureCelsius < -50 ||
                           ambientTemperatureCelsius >=
                               MaximumTemperatureCelsius ||
                           thermalRiseScale <= 0 ||
                           thermalRiseScale > MaximumThermalRiseScale ||
                           maximumProcessorTemperatureCelsius <=
                               ambientTemperatureCelsius ||
                           maximumProcessorTemperatureCelsius >
                               MaximumTemperatureCelsius ||
                           maximumGraphicsCardTemperatureCelsius <=
                               ambientTemperatureCelsius ||
                           maximumGraphicsCardTemperatureCelsius >
                               MaximumTemperatureCelsius ||
                           minimumPowerMarginWatts < 0 ||
                           minimumPowerMarginWatts >
                               PcElectricalSpecification.MaximumSupportedWatts ||
                           minimumBenchmarkScore <= 0 ||
                           minimumBenchmarkScore > MaximumScore ||
                           goodBenchmarkScore < minimumBenchmarkScore ||
                           goodBenchmarkScore > MaximumScore ||
                           excellentBenchmarkScore < goodBenchmarkScore ||
                           excellentBenchmarkScore > MaximumScore;
            return invalid
                ? OperationResult<PcValidationProfile>.Fail(
                    PcValidationFailures.ProfileInvalid)
                : OperationResult<PcValidationProfile>.Success(
                    new PcValidationProfile(
                        id,
                        stressSteps,
                        ambientTemperatureCelsius,
                        thermalRiseScale,
                        maximumProcessorTemperatureCelsius,
                        maximumGraphicsCardTemperatureCelsius,
                        minimumPowerMarginWatts,
                        minimumBenchmarkScore,
                        goodBenchmarkScore,
                        excellentBenchmarkScore));
        }

        internal PcQualityTier ResolveQualityTier(int benchmarkScore)
        {
            if (benchmarkScore >= ExcellentBenchmarkScore)
            {
                return PcQualityTier.Excellent;
            }

            return benchmarkScore >= GoodBenchmarkScore
                ? PcQualityTier.Good
                : PcQualityTier.Standard;
        }
    }

    internal sealed class PcValidationComputedMetrics
    {
        internal PcValidationComputedMetrics(
            int benchmarkScore,
            int processorScore,
            int graphicsCardScore,
            int processorPeakTemperatureCelsius,
            int graphicsCardPeakTemperatureCelsius,
            int systemPowerDrawWatts,
            int minimumRecommendedPsuWatts,
            int installedPsuWatts,
            int powerMarginWatts,
            PcQualityTier qualityTier)
        {
            BenchmarkScore = benchmarkScore;
            ProcessorScore = processorScore;
            GraphicsCardScore = graphicsCardScore;
            ProcessorPeakTemperatureCelsius =
                processorPeakTemperatureCelsius;
            GraphicsCardPeakTemperatureCelsius =
                graphicsCardPeakTemperatureCelsius;
            SystemPowerDrawWatts = systemPowerDrawWatts;
            MinimumRecommendedPsuWatts = minimumRecommendedPsuWatts;
            InstalledPsuWatts = installedPsuWatts;
            PowerMarginWatts = powerMarginWatts;
            QualityTier = qualityTier;
        }

        internal int BenchmarkScore { get; }
        internal int ProcessorScore { get; }
        internal int GraphicsCardScore { get; }
        internal int ProcessorPeakTemperatureCelsius { get; }
        internal int GraphicsCardPeakTemperatureCelsius { get; }
        internal int SystemPowerDrawWatts { get; }
        internal int MinimumRecommendedPsuWatts { get; }
        internal int InstalledPsuWatts { get; }
        internal int PowerMarginWatts { get; }
        internal PcQualityTier QualityTier { get; }
    }

    /// <summary>
    /// Immutable evidence for one deterministic fictional benchmark and fixed stress run.
    /// It binds exact driver, OS, power-cycle, assembly/cable and power-budget lineage. It is
    /// not a host benchmark, sensor reading, vendor tool, fan curve or hardware-damage result.
    /// </summary>
    public sealed class PcValidationReceipt
    {
        private readonly PcValidationAuthority _owner;

        internal PcValidationReceipt(
            PcValidationAuthority owner,
            StableId<PcValidationOperationIdScope> operationId,
            PcFictionalDriverInstallationReceipt sourceDriverReceipt,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            ElectricalReadinessSnapshot sourceElectricalReadiness,
            PcPowerBudgetSnapshot sourcePowerBudget,
            StableId<PcPerformanceCatalogIdScope> performanceCatalogId,
            StableId<PcValidationProfileIdScope> profileId,
            PcValidationComputedMetrics metrics,
            int stressSteps,
            long expectedPowerStateRevision,
            long expectedRevision,
            long revision)
        {
            _owner = owner;
            OperationId = operationId;
            SourceDriverReceipt = sourceDriverReceipt;
            SourceFirmwareBaselineReceipt = sourceFirmwareBaselineReceipt;
            SourceElectricalReadiness = sourceElectricalReadiness;
            SourcePowerBudget = sourcePowerBudget;
            PerformanceCatalogId = performanceCatalogId;
            ProfileId = profileId;
            BenchmarkScore = metrics.BenchmarkScore;
            ProcessorScore = metrics.ProcessorScore;
            GraphicsCardScore = metrics.GraphicsCardScore;
            ProcessorPeakTemperatureCelsius =
                metrics.ProcessorPeakTemperatureCelsius;
            GraphicsCardPeakTemperatureCelsius =
                metrics.GraphicsCardPeakTemperatureCelsius;
            SystemPowerDrawWatts = metrics.SystemPowerDrawWatts;
            MinimumRecommendedPsuWatts = metrics.MinimumRecommendedPsuWatts;
            InstalledPsuWatts = metrics.InstalledPsuWatts;
            PowerMarginWatts = metrics.PowerMarginWatts;
            QualityTier = metrics.QualityTier;
            StressSteps = stressSteps;
            ExpectedPowerStateRevision = expectedPowerStateRevision;
            ExpectedRevision = expectedRevision;
            Revision = revision;
        }

        public StableId<PcValidationOperationIdScope> OperationId { get; }

        public PcValidationResult Result =>
            PcValidationResult.PassedForQualityStage;

        public PcStressResult StressResult => PcStressResult.Stable;

        public PcFictionalDriverInstallationReceipt SourceDriverReceipt { get; }

        public PcFictionalOsInstallationReceipt SourceOperatingSystemReceipt =>
            SourceDriverReceipt?.SourceOperatingSystemReceipt;

        public PcFirmwareBaselineReceipt SourceFirmwareBaselineReceipt { get; }

        public PcPostStartupReceipt SourcePostStartupReceipt =>
            SourceFirmwareBaselineReceipt?.SourcePostStartupReceipt;

        public PcPowerStateReceipt SourcePowerOnReceipt =>
            SourceFirmwareBaselineReceipt?.SourcePowerOnReceipt;

        public PowerTestAttemptReceipt PreflightReceipt =>
            SourceFirmwareBaselineReceipt?.PreflightReceipt;

        public ElectricalReadinessSnapshot SourceElectricalReadiness { get; }

        public PcPowerBudgetSnapshot SourcePowerBudget { get; }

        public StableId<PcPerformanceCatalogIdScope> PerformanceCatalogId { get; }

        public StableId<PcValidationProfileIdScope> ProfileId { get; }

        public StableId<ItemInstanceIdScope> StorageItemId =>
            SourceDriverReceipt != null
                ? SourceDriverReceipt.StorageItemId
                : default;

        public StableId<ProductDefinitionIdScope> StorageProductId =>
            SourceDriverReceipt != null
                ? SourceDriverReceipt.StorageProductId
                : default;

        public int BenchmarkScore { get; }

        public int ProcessorScore { get; }

        public int GraphicsCardScore { get; }

        public int StressSteps { get; }

        public int ProcessorPeakTemperatureCelsius { get; }

        public int GraphicsCardPeakTemperatureCelsius { get; }

        public int SystemPowerDrawWatts { get; }

        public int MinimumRecommendedPsuWatts { get; }

        public int InstalledPsuWatts { get; }

        public int PowerMarginWatts { get; }

        public PcQualityTier QualityTier { get; }

        public long ExpectedPowerStateRevision { get; }

        public long PowerStateRevision =>
            SourceFirmwareBaselineReceipt?.PowerStateRevision ?? -1L;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        internal bool IsOwnedBy(PcValidationAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<PcValidationOperationIdScope> operationId,
            PcFictionalDriverInstallationReceipt sourceDriverReceipt,
            PcFirmwareBaselineReceipt sourceFirmwareBaselineReceipt,
            long expectedPowerStateRevision,
            long expectedRevision)
        {
            return OperationId == operationId &&
                   ReferenceEquals(SourceDriverReceipt, sourceDriverReceipt) &&
                   ReferenceEquals(
                       SourceFirmwareBaselineReceipt,
                       sourceFirmwareBaselineReceipt) &&
                   ExpectedPowerStateRevision == expectedPowerStateRevision &&
                   ExpectedRevision == expectedRevision;
        }
    }

    public static class PcValidationFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "assembly.validation.configuration-missing");
        public static readonly Failure AuthorityMismatch = Failure.FromCode(
            "assembly.validation.authority-mismatch");
        public static readonly Failure ProfileInvalid = Failure.FromCode(
            "assembly.validation.profile-invalid");
        public static readonly Failure CatalogMismatch = Failure.FromCode(
            "assembly.validation.catalog-mismatch");
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "assembly.validation.operation-id-invalid");
        public static readonly Failure InvalidDriverReceipt = Failure.FromCode(
            "assembly.validation.driver-receipt-invalid");
        public static readonly Failure InvalidFirmwareBaselineReceipt =
            Failure.FromCode("assembly.validation.firmware-receipt-invalid");
        public static readonly Failure BenchmarkReadinessRejected =
            Failure.FromCode("assembly.validation.benchmark-readiness-rejected");
        public static readonly Failure PowerStateRevisionMismatch =
            Failure.FromCode("assembly.validation.power-state-revision-mismatch");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "assembly.validation.revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "assembly.validation.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "assembly.validation.operation-conflict");
        public static readonly Failure NotCurrent = Failure.FromCode(
            "assembly.validation.not-current");
        public static readonly Failure PerformanceProfileMissing =
            Failure.FromCode("assembly.validation.performance-profile-missing");
        public static readonly Failure PerformanceProfileKindMismatch =
            Failure.FromCode("assembly.validation.performance-profile-kind-mismatch");
        public static readonly Failure ArithmeticOverflow = Failure.FromCode(
            "assembly.validation.arithmetic-overflow");
        public static readonly Failure PowerMarginInsufficient = Failure.FromCode(
            "assembly.validation.power-margin-insufficient");
        public static readonly Failure ProcessorThermalLimitExceeded =
            Failure.FromCode("assembly.validation.processor-thermal-limit");
        public static readonly Failure GraphicsThermalLimitExceeded =
            Failure.FromCode("assembly.validation.graphics-thermal-limit");
        public static readonly Failure ScoreBelowMinimum = Failure.FromCode(
            "assembly.validation.score-below-minimum");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "assembly.validation.receipt-history-invalid");
    }
}
