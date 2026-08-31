using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PcPowerBudgetPolicyIdScope : IStableIdScope
    {
    }

    /// <summary>
    /// Versioned integer-only PSU sizing policy. The first policy preserves the legacy
    /// Dashboard's 30 percent headroom and 50 W upward-rounding rule.
    /// </summary>
    public sealed class PcPowerBudgetPolicy
    {
        public const int MaximumSystemPowerDrawWatts = 1_000_000;

        private PcPowerBudgetPolicy(
            StableId<PcPowerBudgetPolicyIdScope> id,
            int platformBaseLoadWatts,
            int chassisLoadWatts,
            int headroomNumerator,
            int headroomDenominator,
            int capacityQuantumWatts)
        {
            Id = id;
            PlatformBaseLoadWatts = platformBaseLoadWatts;
            ChassisLoadWatts = chassisLoadWatts;
            HeadroomNumerator = headroomNumerator;
            HeadroomDenominator = headroomDenominator;
            CapacityQuantumWatts = capacityQuantumWatts;
        }

        public StableId<PcPowerBudgetPolicyIdScope> Id { get; }

        public int PlatformBaseLoadWatts { get; }

        public int ChassisLoadWatts { get; }

        public int HeadroomNumerator { get; }

        public int HeadroomDenominator { get; }

        public int CapacityQuantumWatts { get; }

        public static OperationResult<PcPowerBudgetPolicy> Create(
            StableId<PcPowerBudgetPolicyIdScope> id,
            int platformBaseLoadWatts,
            int chassisLoadWatts,
            int headroomNumerator,
            int headroomDenominator,
            int capacityQuantumWatts)
        {
            if (id.IsEmpty ||
                platformBaseLoadWatts < 0 ||
                chassisLoadWatts < 0 ||
                platformBaseLoadWatts > MaximumSystemPowerDrawWatts ||
                chassisLoadWatts > MaximumSystemPowerDrawWatts ||
                headroomNumerator <= 0 ||
                headroomDenominator <= 0 ||
                headroomNumerator < headroomDenominator ||
                capacityQuantumWatts <= 0 ||
                capacityQuantumWatts > PcElectricalSpecification.MaximumSupportedWatts)
            {
                return OperationResult<PcPowerBudgetPolicy>.Fail(
                    PcPowerBudgetFailures.PolicyInvalid);
            }

            return OperationResult<PcPowerBudgetPolicy>.Success(
                new PcPowerBudgetPolicy(
                    id,
                    platformBaseLoadWatts,
                    chassisLoadWatts,
                    headroomNumerator,
                    headroomDenominator,
                    capacityQuantumWatts));
        }

        public OperationResult<int> CalculateMinimumRecommendedPsuWatts(
            int systemPowerDrawWatts)
        {
            if (systemPowerDrawWatts <= 0 ||
                systemPowerDrawWatts > MaximumSystemPowerDrawWatts)
            {
                return OperationResult<int>.Fail(
                    PcPowerBudgetFailures.SystemPowerDrawInvalid);
            }

            long scaledDraw = (long)systemPowerDrawWatts * HeadroomNumerator;
            long quantumDivisor = (long)HeadroomDenominator * CapacityQuantumWatts;
            long requiredQuanta =
                (scaledDraw + quantumDivisor - 1L) / quantumDivisor;
            long requiredCapacity = requiredQuanta * CapacityQuantumWatts;
            if (requiredCapacity <= 0 || requiredCapacity > int.MaxValue)
            {
                return OperationResult<int>.Fail(
                    PcPowerBudgetFailures.ArithmeticOverflow);
            }

            return OperationResult<int>.Success((int)requiredCapacity);
        }
    }

    /// <summary>
    /// Read-only power-budget assessment bound to one exact electrical-readiness lineage.
    /// It is not a power-on, electrical-fault, POST or benchmark receipt.
    /// </summary>
    public sealed class PcPowerBudgetSnapshot
    {
        internal PcPowerBudgetSnapshot(
            ElectricalReadinessSnapshot electricalReadiness,
            StableId<PcPowerBudgetPolicyIdScope> policyId,
            StableId<ProductDefinitionIdScope> motherboardProductId,
            StableId<ProductDefinitionIdScope> processorProductId,
            StableId<ProductDefinitionIdScope> memoryProductId,
            StableId<ProductDefinitionIdScope> storageProductId,
            StableId<ProductDefinitionIdScope> processorCoolerProductId,
            StableId<ProductDefinitionIdScope> graphicsCardProductId,
            StableId<ProductDefinitionIdScope> powerSupplyProductId,
            int platformBaseLoadWatts,
            int chassisLoadWatts,
            int processorLoadWatts,
            int memoryLoadWatts,
            int storageLoadWatts,
            int processorCoolerLoadWatts,
            int graphicsCardLoadWatts,
            int systemPowerDrawWatts,
            int minimumRecommendedPsuWatts,
            int installedPsuWatts,
            int capacityMarginWatts,
            Failure blocker)
        {
            ElectricalReadiness = electricalReadiness;
            PolicyId = policyId;
            MotherboardProductId = motherboardProductId;
            ProcessorProductId = processorProductId;
            MemoryProductId = memoryProductId;
            StorageProductId = storageProductId;
            ProcessorCoolerProductId = processorCoolerProductId;
            GraphicsCardProductId = graphicsCardProductId;
            PowerSupplyProductId = powerSupplyProductId;
            PlatformBaseLoadWatts = platformBaseLoadWatts;
            ChassisLoadWatts = chassisLoadWatts;
            ProcessorLoadWatts = processorLoadWatts;
            MemoryLoadWatts = memoryLoadWatts;
            StorageLoadWatts = storageLoadWatts;
            ProcessorCoolerLoadWatts = processorCoolerLoadWatts;
            GraphicsCardLoadWatts = graphicsCardLoadWatts;
            SystemPowerDrawWatts = systemPowerDrawWatts;
            MinimumRecommendedPsuWatts = minimumRecommendedPsuWatts;
            InstalledPsuWatts = installedPsuWatts;
            CapacityMarginWatts = capacityMarginWatts;
            Blocker = blocker;
        }

        public ElectricalReadinessSnapshot ElectricalReadiness { get; }

        public StableId<PcPowerBudgetPolicyIdScope> PolicyId { get; }

        public StableId<ProductDefinitionIdScope> MotherboardProductId { get; }

        public StableId<ProductDefinitionIdScope> ProcessorProductId { get; }

        public StableId<ProductDefinitionIdScope> MemoryProductId { get; }

        public StableId<ProductDefinitionIdScope> StorageProductId { get; }

        public StableId<ProductDefinitionIdScope> ProcessorCoolerProductId { get; }

        public StableId<ProductDefinitionIdScope> GraphicsCardProductId { get; }

        public StableId<ProductDefinitionIdScope> PowerSupplyProductId { get; }

        public int PlatformBaseLoadWatts { get; }

        public int ChassisLoadWatts { get; }

        public int ProcessorLoadWatts { get; }

        public int MemoryLoadWatts { get; }

        public int StorageLoadWatts { get; }

        public int ProcessorCoolerLoadWatts { get; }

        public int GraphicsCardLoadWatts { get; }

        public int SystemPowerDrawWatts { get; }

        public int MinimumRecommendedPsuWatts { get; }

        public int InstalledPsuWatts { get; }

        public int CapacityMarginWatts { get; }

        public Failure Blocker { get; }

        public bool IsSufficient => Blocker.IsNone;
    }

    public static class PcPowerBudgetFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "assembly.power-budget.configuration-missing");
        public static readonly Failure CatalogMismatch = Failure.FromCode(
            "assembly.power-budget.catalog-mismatch");
        public static readonly Failure PolicyInvalid = Failure.FromCode(
            "assembly.power-budget.policy-invalid");
        public static readonly Failure SystemPowerDrawInvalid = Failure.FromCode(
            "assembly.power-budget.system-draw-invalid");
        public static readonly Failure ArithmeticOverflow = Failure.FromCode(
            "assembly.power-budget.arithmetic-overflow");
        public static readonly Failure ElectricalProfileMissing = Failure.FromCode(
            "assembly.power-budget.electrical-profile-missing");
        public static readonly Failure ElectricalProfileKindMismatch = Failure.FromCode(
            "assembly.power-budget.electrical-profile-kind-mismatch");
        public static readonly Failure PowerSupplyInsufficient = Failure.FromCode(
            "assembly.power-budget.power-supply-insufficient");
    }
}
