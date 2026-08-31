using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void PowerBudgetBindsExactLegacyPolicyAndReadinessWithoutMutation()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;

            OperationResult<PcPowerBudgetSnapshot> result =
                session.PowerBudget.AssessPowerBudget();
            OperationResult<PcPowerBudgetSnapshot> replay =
                session.PowerBudget.AssessPowerBudget();

            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            Assert.That(replay.IsSuccess, Is.True, replay.Error.Code);
            Assert.That(result.Value.IsSufficient, Is.True);
            Assert.That(result.Value.Blocker, Is.EqualTo(Failure.None));
            Assert.That(result.Value.PolicyId.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypePowerBudgetPolicyIdValue));
            Assert.That(result.Value.ElectricalReadiness.BuildId,
                Is.EqualTo(session.AssemblyBuild.BuildId));
            Assert.That(result.Value.ElectricalReadiness.ChassisId,
                Is.EqualTo(session.AssemblyBuild.ChassisId));
            Assert.That(result.Value.ElectricalReadiness.AssemblyRevision,
                Is.EqualTo(assemblyRevision));
            Assert.That(result.Value.MotherboardProductId,
                Is.EqualTo(session.MotherboardProductId));
            Assert.That(result.Value.ProcessorProductId,
                Is.EqualTo(session.ProcessorProductId));
            Assert.That(result.Value.MemoryProductId,
                Is.EqualTo(session.MemoryProductId));
            Assert.That(result.Value.StorageProductId,
                Is.EqualTo(session.StorageProductId));
            Assert.That(result.Value.ProcessorCoolerProductId,
                Is.EqualTo(session.ProcessorCoolerProductId));
            Assert.That(result.Value.GraphicsCardProductId,
                Is.EqualTo(session.ProductId));
            Assert.That(result.Value.PowerSupplyProductId,
                Is.EqualTo(session.PowerSupplyProductId));
            Assert.That(result.Value.PlatformBaseLoadWatts, Is.EqualTo(35));
            Assert.That(result.Value.ChassisLoadWatts, Is.EqualTo(4));
            Assert.That(result.Value.ProcessorLoadWatts, Is.EqualTo(125));
            Assert.That(result.Value.GraphicsCardLoadWatts, Is.EqualTo(200));
            Assert.That(result.Value.MemoryLoadWatts, Is.EqualTo(6));
            Assert.That(result.Value.StorageLoadWatts, Is.EqualTo(5));
            Assert.That(result.Value.ProcessorCoolerLoadWatts, Is.EqualTo(5));
            Assert.That(result.Value.SystemPowerDrawWatts, Is.EqualTo(380));
            Assert.That(result.Value.MinimumRecommendedPsuWatts, Is.EqualTo(500));
            Assert.That(result.Value.InstalledPsuWatts, Is.EqualTo(550));
            Assert.That(result.Value.CapacityMarginWatts, Is.EqualTo(50));
            Assert.That(replay.Value.SystemPowerDrawWatts,
                Is.EqualTo(result.Value.SystemPowerDrawWatts));
            Assert.That(replay.Value.MinimumRecommendedPsuWatts,
                Is.EqualTo(result.Value.MinimumRecommendedPsuWatts));
            AssertPowerBudgetDidNotMutate(
                session,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                atx24Revision,
                eps12vRevision,
                pcieRevision);
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PowerBudgetRequiresExactElectricalReadinessBeforeCatalogEvaluation()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24AndEps12vForPcieGpuAssembly(
                session,
                out _,
                out _,
                out _,
                out _);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<PcPowerBudgetSnapshot> result =
                session.PowerBudget.AssessPowerBudget();

            Assert.That(result.Error,
                Is.EqualTo(ElectricalReadinessFailures.PcieGpuPowerCableMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
        }

        [Test]
        public void PowerBudgetReportsInsufficientPsuAsValidBlockedAssessment()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcPowerBudgetAuthority insufficient = CreateTestPowerBudget(
                session,
                450,
                includeGraphicsCardProfile: true);
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<PcPowerBudgetSnapshot> result =
                insufficient.AssessPowerBudget();

            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            Assert.That(result.Value.IsSufficient, Is.False);
            Assert.That(result.Value.Blocker,
                Is.EqualTo(PcPowerBudgetFailures.PowerSupplyInsufficient));
            Assert.That(result.Value.SystemPowerDrawWatts, Is.EqualTo(380));
            Assert.That(result.Value.MinimumRecommendedPsuWatts, Is.EqualTo(500));
            Assert.That(result.Value.InstalledPsuWatts, Is.EqualTo(450));
            Assert.That(result.Value.CapacityMarginWatts, Is.EqualTo(-50));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
        }

        [Test]
        public void PowerBudgetFailsClosedWhenExactProductProfileIsMissing()
        {
            GarageStockFlowSession session = PreparePowerBudgetReadySession();
            PcPowerBudgetAuthority missingGpu = CreateTestPowerBudget(
                session,
                550,
                includeGraphicsCardProfile: false);
            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult<PcPowerBudgetSnapshot> result =
                missingGpu.AssessPowerBudget();

            Assert.That(result.Error,
                Is.EqualTo(PcPowerBudgetFailures.ElectricalProfileMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
        }

        [Test]
        public void LegacyPowerBudgetPolicyUsesThirtyPercentAndFiftyWattCeiling()
        {
            PcPowerBudgetPolicy policy = CreateTestPowerBudgetPolicy();

            Assert.That(policy.CalculateMinimumRecommendedPsuWatts(380).Value,
                Is.EqualTo(500));
            Assert.That(policy.CalculateMinimumRecommendedPsuWatts(384).Value,
                Is.EqualTo(500));
            Assert.That(policy.CalculateMinimumRecommendedPsuWatts(385).Value,
                Is.EqualTo(550));
            Assert.That(policy.CalculateMinimumRecommendedPsuWatts(0).Error,
                Is.EqualTo(PcPowerBudgetFailures.SystemPowerDrawInvalid));
            Assert.That(PcPowerBudgetPolicy.Create(
                    StableId<PcPowerBudgetPolicyIdScope>.Parse(
                        "assembly.power-budget-policy.invalid"),
                    35,
                    4,
                    90,
                    100,
                    50).Error,
                Is.EqualTo(PcPowerBudgetFailures.PolicyInvalid));
        }

        [Test]
        public void PowerBudgetAuthorityRejectsForeignAssemblyCatalog()
        {
            GarageStockFlowSession first =
                GarageStockFlowSession.CreateArrived(includeAssemblyPrototype: true);
            GarageStockFlowSession second =
                GarageStockFlowSession.CreateArrived(includeAssemblyPrototype: true);

            OperationResult<PcPowerBudgetAuthority> result =
                PcPowerBudgetAuthority.Create(
                    first.PowerBudget.ElectricalCatalog,
                    second.AssemblyBuild,
                    first.PowerBudget.Policy);

            Assert.That(result.Error,
                Is.EqualTo(PcPowerBudgetFailures.CatalogMismatch));
        }

        private static GarageStockFlowSession PreparePowerBudgetReadySession()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageAllTenBuildKitComponents(session, workOrder);
            PrepareRoutedAtx24AndEps12vForPcieGpuAssembly(
                session,
                out _,
                out _,
                out _,
                out _);
            Assert.That(session.PickupStagedPcieGpuPowerCableForAssembly().IsSuccess,
                Is.True);
            OperationResult<PcieGpuPowerCableOperationReceipt> route =
                session.RoutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.issue121.route-pcie-gpu"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.PcieGpuPowerCableRevision);
            Assert.That(route.IsSuccess, Is.True, route.Error.Code);
            Assert.That(session.AssemblyBuild.EvaluateElectricalReadiness().IsSuccess,
                Is.True);
            return session;
        }

        private static PcPowerBudgetAuthority CreateTestPowerBudget(
            GarageStockFlowSession session,
            int powerSupplyWatts,
            bool includeGraphicsCardProfile)
        {
            var specifications = new List<PcElectricalSpecification>
            {
                PcElectricalSpecification.CreateLoad(
                    session.Components,
                    session.ProcessorProductId,
                    GarageStockFlowSession.PrototypeProcessorLoadWatts).Value,
                PcElectricalSpecification.CreateLoad(
                    session.Components,
                    session.MemoryProductId,
                    GarageStockFlowSession.PrototypeMemoryLoadWatts).Value,
                PcElectricalSpecification.CreateLoad(
                    session.Components,
                    session.StorageProductId,
                    GarageStockFlowSession.PrototypeStorageLoadWatts).Value,
                PcElectricalSpecification.CreateLoad(
                    session.Components,
                    session.ProcessorCoolerProductId,
                    GarageStockFlowSession.PrototypeProcessorCoolerLoadWatts).Value,
                PcElectricalSpecification.CreatePowerSupply(
                    session.Components,
                    session.PowerSupplyProductId,
                    powerSupplyWatts).Value
            };
            if (includeGraphicsCardProfile)
            {
                specifications.Add(PcElectricalSpecification.CreateLoad(
                    session.Components,
                    session.ProductId,
                    GarageStockFlowSession.PrototypeGraphicsCardLoadWatts).Value);
            }

            PcElectricalCatalog electricalCatalog = PcElectricalCatalog.Create(
                session.Components,
                specifications).Value;
            return PcPowerBudgetAuthority.Create(
                electricalCatalog,
                session.AssemblyBuild,
                CreateTestPowerBudgetPolicy()).Value;
        }

        private static PcPowerBudgetPolicy CreateTestPowerBudgetPolicy()
        {
            return PcPowerBudgetPolicy.Create(
                StableId<PcPowerBudgetPolicyIdScope>.Parse(
                    GarageStockFlowSession.PrototypePowerBudgetPolicyIdValue),
                GarageStockFlowSession.PrototypePlatformBaseLoadWatts,
                GarageStockFlowSession.PrototypeChassisLoadWatts,
                GarageStockFlowSession.PrototypePowerBudgetHeadroomNumerator,
                GarageStockFlowSession.PrototypePowerBudgetHeadroomDenominator,
                GarageStockFlowSession.PrototypePowerBudgetCapacityQuantumWatts).Value;
        }

        private static void AssertPowerBudgetDidNotMutate(
            GarageStockFlowSession session,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            long atx24Revision,
            long eps12vRevision,
            long pcieRevision)
        {
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieRevision));
        }
    }
}
