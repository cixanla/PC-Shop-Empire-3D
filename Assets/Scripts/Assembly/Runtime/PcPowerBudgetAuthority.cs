using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Read-only calculator that joins exact retained assembly identity with immutable
    /// electrical metadata. It owns no power state, input, inventory or receipts.
    /// </summary>
    public sealed class PcPowerBudgetAuthority
    {
        private readonly PcElectricalCatalog _electricalCatalog;
        private readonly AssemblyBuildAuthority _assemblyBuild;
        private readonly PcPowerBudgetPolicy _policy;

        private PcPowerBudgetAuthority(
            PcElectricalCatalog electricalCatalog,
            AssemblyBuildAuthority assemblyBuild,
            PcPowerBudgetPolicy policy)
        {
            _electricalCatalog = electricalCatalog;
            _assemblyBuild = assemblyBuild;
            _policy = policy;
        }

        public PcElectricalCatalog ElectricalCatalog => _electricalCatalog;

        public AssemblyBuildAuthority AssemblyBuild => _assemblyBuild;

        public PcPowerBudgetPolicy Policy => _policy;

        public static OperationResult<PcPowerBudgetAuthority> Create(
            PcElectricalCatalog electricalCatalog,
            AssemblyBuildAuthority assemblyBuild,
            PcPowerBudgetPolicy policy)
        {
            if (electricalCatalog == null || assemblyBuild == null || policy == null)
            {
                return OperationResult<PcPowerBudgetAuthority>.Fail(
                    PcPowerBudgetFailures.ConfigurationMissing);
            }

            if (!ReferenceEquals(
                    electricalCatalog.OwnerComponentCatalog,
                    assemblyBuild.ComponentCatalog))
            {
                return OperationResult<PcPowerBudgetAuthority>.Fail(
                    PcPowerBudgetFailures.CatalogMismatch);
            }

            return OperationResult<PcPowerBudgetAuthority>.Success(
                new PcPowerBudgetAuthority(
                    electricalCatalog,
                    assemblyBuild,
                    policy));
        }

        public OperationResult<PcPowerBudgetSnapshot> AssessPowerBudget()
        {
            OperationResult<ElectricalReadinessSnapshot> readiness =
                _assemblyBuild.EvaluateElectricalReadiness();
            if (readiness.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(readiness.Error);
            }

            OperationResult<PcElectricalSpecification> processor = ResolveLoad(
                _assemblyBuild.ProcessorProductId,
                PcComponentKind.Processor);
            if (processor.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(processor.Error);
            }

            OperationResult<PcElectricalSpecification> memory = ResolveLoad(
                _assemblyBuild.MemoryProductId,
                PcComponentKind.MemoryModule);
            if (memory.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(memory.Error);
            }

            OperationResult<PcElectricalSpecification> storage = ResolveLoad(
                _assemblyBuild.StorageProductId,
                PcComponentKind.StorageDevice);
            if (storage.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(storage.Error);
            }

            OperationResult<PcElectricalSpecification> processorCooler = ResolveLoad(
                _assemblyBuild.ProcessorCoolerProductId,
                PcComponentKind.ProcessorCooler);
            if (processorCooler.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(processorCooler.Error);
            }

            OperationResult<PcElectricalSpecification> graphicsCard = ResolveLoad(
                _assemblyBuild.GraphicsCardProductId,
                PcComponentKind.GraphicsCard);
            if (graphicsCard.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(graphicsCard.Error);
            }

            OperationResult<PcElectricalSpecification> powerSupply = ResolvePowerSupply(
                _assemblyBuild.PowerSupplyProductId);
            if (powerSupply.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(powerSupply.Error);
            }

            long draw = (long)_policy.PlatformBaseLoadWatts +
                        _policy.ChassisLoadWatts +
                        processor.Value.LoadWatts +
                        memory.Value.LoadWatts +
                        storage.Value.LoadWatts +
                        processorCooler.Value.LoadWatts +
                        graphicsCard.Value.LoadWatts;
            if (draw <= 0 || draw > PcPowerBudgetPolicy.MaximumSystemPowerDrawWatts)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(
                    PcPowerBudgetFailures.ArithmeticOverflow);
            }

            OperationResult<int> recommended =
                _policy.CalculateMinimumRecommendedPsuWatts((int)draw);
            if (recommended.IsFailure)
            {
                return OperationResult<PcPowerBudgetSnapshot>.Fail(recommended.Error);
            }

            int margin = powerSupply.Value.RatedOutputWatts - recommended.Value;
            Failure blocker = margin >= 0
                ? Failure.None
                : PcPowerBudgetFailures.PowerSupplyInsufficient;
            return OperationResult<PcPowerBudgetSnapshot>.Success(
                new PcPowerBudgetSnapshot(
                    readiness.Value,
                    _policy.Id,
                    _assemblyBuild.MotherboardProductId,
                    _assemblyBuild.ProcessorProductId,
                    _assemblyBuild.MemoryProductId,
                    _assemblyBuild.StorageProductId,
                    _assemblyBuild.ProcessorCoolerProductId,
                    _assemblyBuild.GraphicsCardProductId,
                    _assemblyBuild.PowerSupplyProductId,
                    _policy.PlatformBaseLoadWatts,
                    _policy.ChassisLoadWatts,
                    processor.Value.LoadWatts,
                    memory.Value.LoadWatts,
                    storage.Value.LoadWatts,
                    processorCooler.Value.LoadWatts,
                    graphicsCard.Value.LoadWatts,
                    (int)draw,
                    recommended.Value,
                    powerSupply.Value.RatedOutputWatts,
                    margin,
                    blocker));
        }

        private OperationResult<PcElectricalSpecification> ResolveLoad(
            StableId<ProductDefinitionIdScope> productId,
            PcComponentKind expectedKind)
        {
            if (!_electricalCatalog.TryGet(
                    productId,
                    out PcElectricalSpecification specification))
            {
                return OperationResult<PcElectricalSpecification>.Fail(
                    PcPowerBudgetFailures.ElectricalProfileMissing);
            }

            return specification.ComponentKind == expectedKind &&
                   specification.IsLoadProfile
                ? OperationResult<PcElectricalSpecification>.Success(specification)
                : OperationResult<PcElectricalSpecification>.Fail(
                    PcPowerBudgetFailures.ElectricalProfileKindMismatch);
        }

        private OperationResult<PcElectricalSpecification> ResolvePowerSupply(
            StableId<ProductDefinitionIdScope> productId)
        {
            if (!_electricalCatalog.TryGet(
                    productId,
                    out PcElectricalSpecification specification))
            {
                return OperationResult<PcElectricalSpecification>.Fail(
                    PcPowerBudgetFailures.ElectricalProfileMissing);
            }

            return specification.ComponentKind == PcComponentKind.PowerSupply &&
                   specification.IsPowerSupplyProfile
                ? OperationResult<PcElectricalSpecification>.Success(specification)
                : OperationResult<PcElectricalSpecification>.Fail(
                    PcPowerBudgetFailures.ElectricalProfileKindMismatch);
        }
    }
}
