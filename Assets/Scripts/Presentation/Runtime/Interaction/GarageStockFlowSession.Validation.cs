using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PrototypePerformanceCatalogIdValue =
            "catalog.performance.prototype.legacy-v1";
        public const string PrototypeValidationProfileIdValue =
            "assembly.validation-profile.workshop-v1";
        public const int PrototypeMotherboardPerformanceScore = 34;
        public const int PrototypeProcessorPerformanceScore = 117;
        public const int PrototypeMemoryPerformanceScore = 31;
        public const int PrototypeStoragePerformanceScore = 49;
        public const int PrototypeProcessorCoolerPerformanceScore = 25;
        public const int PrototypeGraphicsCardPerformanceScore = 121;
        public const int PrototypePowerSupplyPerformanceScore = 24;
        public const int PrototypeProcessorThermalLoadWatts = 125;
        public const int PrototypeGraphicsCardThermalLoadWatts = 200;
        public const int PrototypeProcessorCoolerCapacityWatts = 140;
        public const int PrototypeGraphicsCardCoolingCapacityWatts = 240;
        public const int PrototypeValidationStressSteps = 300;
        public const int PrototypeValidationAmbientTemperatureCelsius = 22;
        public const int PrototypeValidationThermalRiseScale = 50;
        public const int PrototypeValidationMaximumProcessorTemperatureCelsius = 90;
        public const int PrototypeValidationMaximumGraphicsTemperatureCelsius = 88;
        public const int PrototypeValidationMinimumPowerMarginWatts = 25;
        public const int PrototypeValidationMinimumBenchmarkScore = 300;
        public const int PrototypeValidationGoodBenchmarkScore = 380;
        public const int PrototypeValidationExcellentBenchmarkScore = 500;
        public const int PrototypeExpectedValidationBenchmarkScore = 401;
        public const int PrototypeExpectedProcessorPeakTemperatureCelsius = 67;
        public const int PrototypeExpectedGraphicsPeakTemperatureCelsius = 64;

        private PcValidationAuthority _validation;

        public PcPerformanceCatalog PerformanceCatalog { get; }

        public PcValidationProfile ValidationProfile { get; }

        public PcValidationAuthority Validation
        {
            get
            {
                OperationResult<PcValidationAuthority> ensured =
                    EnsureValidationAuthority();
                return ensured.TryGetValue(out PcValidationAuthority authority)
                    ? authority
                    : null;
            }
        }

        public OperationResult<PcValidationAuthority> EnsureValidationAuthority()
        {
            if (_validation != null)
            {
                return OperationResult<PcValidationAuthority>.Success(_validation);
            }

            if (PowerBudget == null ||
                PerformanceCatalog == null ||
                ValidationProfile == null)
            {
                return OperationResult<PcValidationAuthority>.Fail(
                    PcValidationFailures.ConfigurationMissing);
            }

            OperationResult<PcFictionalDriverInstallationAuthority> driver =
                EnsureFictionalDriverInstallationAuthority();
            if (driver.IsFailure)
            {
                return OperationResult<PcValidationAuthority>.Fail(driver.Error);
            }

            OperationResult<PcValidationAuthority> created =
                PcValidationAuthority.Create(
                    driver.Value,
                    PowerBudget,
                    PerformanceCatalog,
                    ValidationProfile);
            if (created.IsFailure)
            {
                return created;
            }

            _validation = created.Value;
            return OperationResult<PcValidationAuthority>.Success(_validation);
        }

        public StableId<PcValidationOperationIdScope>
            CreatePrototypeValidationOperationId(
                PcFictionalDriverInstallationReceipt sourceDriverReceipt,
                long powerStateRevision,
                long expectedValidationRevision)
        {
            long driverRevision = sourceDriverReceipt?.Revision ?? -1L;
            return StableId<PcValidationOperationIdScope>.Parse(
                "assembly.validation.prototype.driver-" + driverRevision +
                ".power-" + powerStateRevision +
                ".run-" + (expectedValidationRevision + 1L));
        }

        public bool TryGetValidation(out PcValidationAuthority authority)
        {
            authority = _validation;
            return authority != null;
        }

        private static PcPerformanceCatalog CreatePrototypePerformanceCatalog(
            PcComponentCatalog components)
        {
            PcPerformanceSpecification motherboard = Specification(
                components,
                MotherboardProductIdValue,
                PrototypeMotherboardPerformanceScore,
                0,
                0);
            PcPerformanceSpecification processor = Specification(
                components,
                ProcessorProductIdValue,
                PrototypeProcessorPerformanceScore,
                PrototypeProcessorThermalLoadWatts,
                0);
            PcPerformanceSpecification memory = Specification(
                components,
                MemoryProductIdValue,
                PrototypeMemoryPerformanceScore,
                0,
                0);
            PcPerformanceSpecification storage = Specification(
                components,
                StorageProductIdValue,
                PrototypeStoragePerformanceScore,
                0,
                0);
            PcPerformanceSpecification cooler = Specification(
                components,
                ProcessorCoolerProductIdValue,
                PrototypeProcessorCoolerPerformanceScore,
                0,
                PrototypeProcessorCoolerCapacityWatts);
            PcPerformanceSpecification graphics = Specification(
                components,
                ProductIdValue,
                PrototypeGraphicsCardPerformanceScore,
                PrototypeGraphicsCardThermalLoadWatts,
                PrototypeGraphicsCardCoolingCapacityWatts);
            PcPerformanceSpecification powerSupply = Specification(
                components,
                PowerSupplyProductIdValue,
                PrototypePowerSupplyPerformanceScore,
                0,
                0);
            return PcPerformanceCatalog.Create(
                StableId<PcPerformanceCatalogIdScope>.Parse(
                    PrototypePerformanceCatalogIdValue),
                components,
                new[]
                {
                    motherboard,
                    processor,
                    memory,
                    storage,
                    cooler,
                    graphics,
                    powerSupply
                }).Value;
        }

        private static PcValidationProfile CreatePrototypeValidationProfile()
        {
            return PcValidationProfile.Create(
                StableId<PcValidationProfileIdScope>.Parse(
                    PrototypeValidationProfileIdValue),
                PrototypeValidationStressSteps,
                PrototypeValidationAmbientTemperatureCelsius,
                PrototypeValidationThermalRiseScale,
                PrototypeValidationMaximumProcessorTemperatureCelsius,
                PrototypeValidationMaximumGraphicsTemperatureCelsius,
                PrototypeValidationMinimumPowerMarginWatts,
                PrototypeValidationMinimumBenchmarkScore,
                PrototypeValidationGoodBenchmarkScore,
                PrototypeValidationExcellentBenchmarkScore).Value;
        }

        private static PcPerformanceSpecification Specification(
            PcComponentCatalog components,
            string productId,
            int performanceScore,
            int thermalLoadWatts,
            int coolingCapacityWatts)
        {
            return PcPerformanceSpecification.Create(
                components,
                StableId<ProductDefinitionIdScope>.Parse(productId),
                performanceScore,
                thermalLoadWatts,
                coolingCapacityWatts).Value;
        }
    }
}
