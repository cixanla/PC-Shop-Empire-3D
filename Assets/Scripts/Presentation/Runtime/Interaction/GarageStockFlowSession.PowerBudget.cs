using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PrototypePowerBudgetPolicyIdValue =
            "assembly.power-budget-policy.legacy-v1";
        public const int PrototypePlatformBaseLoadWatts = 35;
        public const int PrototypeChassisLoadWatts = 4;
        public const int PrototypeProcessorLoadWatts = 125;
        public const int PrototypeGraphicsCardLoadWatts = 200;
        public const int PrototypeMemoryLoadWatts = 6;
        public const int PrototypeStorageLoadWatts = 5;
        public const int PrototypeProcessorCoolerLoadWatts = 5;
        public const int PrototypePowerSupplyRatedOutputWatts = 550;
        public const int PrototypePowerBudgetHeadroomNumerator = 130;
        public const int PrototypePowerBudgetHeadroomDenominator = 100;
        public const int PrototypePowerBudgetCapacityQuantumWatts = 50;
        public const int PrototypeExpectedSystemPowerDrawWatts = 380;
        public const int PrototypeExpectedMinimumRecommendedPsuWatts = 500;

        public PcPowerBudgetAuthority PowerBudget { get; }

        private static PcPowerBudgetAuthority CreatePrototypePowerBudget(
            PcComponentCatalog components,
            AssemblyBuildAuthority assemblyBuild)
        {
            PcElectricalSpecification processor =
                PcElectricalSpecification.CreateLoad(
                    components,
                    StableId<ProductDefinitionIdScope>.Parse(ProcessorProductIdValue),
                    PrototypeProcessorLoadWatts).Value;
            PcElectricalSpecification graphicsCard =
                PcElectricalSpecification.CreateLoad(
                    components,
                    StableId<ProductDefinitionIdScope>.Parse(ProductIdValue),
                    PrototypeGraphicsCardLoadWatts).Value;
            PcElectricalSpecification memory =
                PcElectricalSpecification.CreateLoad(
                    components,
                    StableId<ProductDefinitionIdScope>.Parse(MemoryProductIdValue),
                    PrototypeMemoryLoadWatts).Value;
            PcElectricalSpecification storage =
                PcElectricalSpecification.CreateLoad(
                    components,
                    StableId<ProductDefinitionIdScope>.Parse(StorageProductIdValue),
                    PrototypeStorageLoadWatts).Value;
            PcElectricalSpecification processorCooler =
                PcElectricalSpecification.CreateLoad(
                    components,
                    StableId<ProductDefinitionIdScope>.Parse(
                        ProcessorCoolerProductIdValue),
                    PrototypeProcessorCoolerLoadWatts).Value;
            PcElectricalSpecification powerSupply =
                PcElectricalSpecification.CreatePowerSupply(
                    components,
                    StableId<ProductDefinitionIdScope>.Parse(
                        PowerSupplyProductIdValue),
                    PrototypePowerSupplyRatedOutputWatts).Value;
            PcElectricalCatalog electricalCatalog = PcElectricalCatalog.Create(
                components,
                new[]
                {
                    processor,
                    graphicsCard,
                    memory,
                    storage,
                    processorCooler,
                    powerSupply
                }).Value;
            PcPowerBudgetPolicy policy = PcPowerBudgetPolicy.Create(
                StableId<PcPowerBudgetPolicyIdScope>.Parse(
                    PrototypePowerBudgetPolicyIdValue),
                PrototypePlatformBaseLoadWatts,
                PrototypeChassisLoadWatts,
                PrototypePowerBudgetHeadroomNumerator,
                PrototypePowerBudgetHeadroomDenominator,
                PrototypePowerBudgetCapacityQuantumWatts).Value;
            return PcPowerBudgetAuthority.Create(
                electricalCatalog,
                assemblyBuild,
                policy).Value;
        }
    }
}
