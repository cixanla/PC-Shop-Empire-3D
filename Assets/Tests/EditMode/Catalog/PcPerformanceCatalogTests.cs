using System;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Tests.EditMode.Catalog
{
    public sealed class PcPerformanceCatalogTests
    {
        [Test]
        public void PerformanceCatalogKeepsStableIdentityExactMetadataAndOrdinalOrder()
        {
            Fixture fixture = CreateFixture("happy");
            PcPerformanceSpecification processor = Specification(
                fixture,
                fixture.ProcessorId,
                17000,
                125,
                0);
            PcPerformanceSpecification cooler = Specification(
                fixture,
                fixture.CoolerId,
                300,
                0,
                180);
            PcPerformanceSpecification graphicsCard = Specification(
                fixture,
                fixture.GraphicsCardId,
                24000,
                250,
                275);

            OperationResult<PcPerformanceCatalog> result = PcPerformanceCatalog.Create(
                CatalogId("performance.happy.v1"),
                fixture.Components,
                new[] { processor, graphicsCard, cooler });

            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            Assert.That(result.Value.CatalogId.Value, Is.EqualTo("performance.happy.v1"));
            Assert.That(result.Value.Count, Is.EqualTo(3));
            Assert.That(result.Value.Specifications[0].ProductId.Value,
                Is.LessThan(result.Value.Specifications[1].ProductId.Value));
            Assert.That(result.Value.Specifications[1].ProductId.Value,
                Is.LessThan(result.Value.Specifications[2].ProductId.Value));
            Assert.That(result.Value.Get(fixture.ProcessorId).Value, Is.SameAs(processor));
            Assert.That(result.Value.TryGet(fixture.CoolerId, out PcPerformanceSpecification found),
                Is.True);
            Assert.That(found, Is.SameAs(cooler));
            Assert.That(graphicsCard.ComponentKind, Is.EqualTo(PcComponentKind.GraphicsCard));
            Assert.That(graphicsCard.PerformanceScore, Is.EqualTo(24000));
            Assert.That(graphicsCard.ThermalLoadWatts, Is.EqualTo(250));
            Assert.That(graphicsCard.CoolingCapacityWatts, Is.EqualTo(275));
        }

        [Test]
        public void PerformanceSpecificationEnforcesSupportedKindsAndExactThermalShapes()
        {
            Fixture fixture = CreateFixture("shape");

            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.MotherboardId, 500, 0, 0).IsSuccess,
                Is.True);
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.MemoryId, 800, 0, 0).IsSuccess,
                Is.True);
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.StorageId, 900, 0, 0).IsSuccess,
                Is.True);
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.PowerSupplyId, 700, 0, 0).IsSuccess,
                Is.True);
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.ProcessorId, 17000, 125, 5).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidThermalShape));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.GraphicsCardId, 24000, 250, 249).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidThermalShape));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.CoolerId, 300, 1, 180).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidThermalShape));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.MotherboardId, 500, 1, 0).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidThermalShape));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.PowerCableId, 1, 0, 0).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.UnsupportedComponentKind));
        }

        [Test]
        public void PerformanceSpecificationRejectsInvalidIdentityScoreBoundsAndUnknownProduct()
        {
            Fixture fixture = CreateFixture("validation");

            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, default, 17000, 125, 0).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidProductId));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, ProductId("performance.unknown"), 17000, 125, 0).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.UnknownComponentProduct));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.ProcessorId, 0, 125, 0).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidPerformanceScore));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components,
                    fixture.ProcessorId,
                    PcPerformanceSpecification.MaximumSupportedScore + 1,
                    125,
                    0).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidPerformanceScore));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components, fixture.ProcessorId, 17000, -1, 0).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidThermalWatts));
            Assert.That(PcPerformanceSpecification.Create(
                    fixture.Components,
                    fixture.CoolerId,
                    300,
                    0,
                    PcPerformanceSpecification.MaximumSupportedWatts + 1).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidThermalWatts));
        }

        [Test]
        public void PerformanceCatalogRejectsInvalidIdentityEmptyNullDuplicateAndForeignSpecifications()
        {
            Fixture first = CreateFixture("first");
            Fixture second = CreateFixture("second");
            PcPerformanceSpecification processor = Specification(
                first,
                first.ProcessorId,
                17000,
                125,
                0);

            Assert.That(PcPerformanceCatalog.Create(
                    default, first.Components, new[] { processor }).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.InvalidCatalogId));
            Assert.That(PcPerformanceCatalog.Create(
                    CatalogId("performance.first.v1"), first.Components, null).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.EmptyCatalog));
            Assert.That(PcPerformanceCatalog.Create(
                    CatalogId("performance.first.v1"),
                    first.Components,
                    Array.Empty<PcPerformanceSpecification>()).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.EmptyCatalog));
            Assert.That(PcPerformanceCatalog.Create(
                    CatalogId("performance.first.v1"),
                    first.Components,
                    new PcPerformanceSpecification[] { null }).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.NullSpecification));
            Assert.That(PcPerformanceCatalog.Create(
                    CatalogId("performance.first.v1"),
                    first.Components,
                    new[] { processor, processor }).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.DuplicateSpecification));
            Assert.That(PcPerformanceCatalog.Create(
                    CatalogId("performance.second.v1"), second.Components, new[] { processor }).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.ComponentCatalogMismatch));
        }

        [Test]
        public void PerformanceCatalogUnknownLookupFailsClosed()
        {
            Fixture fixture = CreateFixture("lookup");
            PcPerformanceCatalog catalog = PcPerformanceCatalog.Create(
                CatalogId("performance.lookup.v1"),
                fixture.Components,
                new[] { Specification(fixture, fixture.ProcessorId, 17000, 125, 0) }).Value;

            Assert.That(catalog.Get(ProductId("performance.lookup.unknown")).Error,
                Is.EqualTo(PcPerformanceCatalogFailures.UnknownSpecification));
        }

        private static PcPerformanceSpecification Specification(
            Fixture fixture,
            StableId<ProductDefinitionIdScope> productId,
            int performanceScore,
            int thermalLoadWatts,
            int coolingCapacityWatts)
        {
            OperationResult<PcPerformanceSpecification> result =
                PcPerformanceSpecification.Create(
                    fixture.Components,
                    productId,
                    performanceScore,
                    thermalLoadWatts,
                    coolingCapacityWatts);
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static Fixture CreateFixture(string suffix)
        {
            StableId<ProductDefinitionIdScope> motherboardId = ProductId($"performance.{suffix}.motherboard");
            StableId<ProductDefinitionIdScope> processorId = ProductId($"performance.{suffix}.processor");
            StableId<ProductDefinitionIdScope> memoryId = ProductId($"performance.{suffix}.memory");
            StableId<ProductDefinitionIdScope> storageId = ProductId($"performance.{suffix}.storage");
            StableId<ProductDefinitionIdScope> coolerId = ProductId($"performance.{suffix}.cooler");
            StableId<ProductDefinitionIdScope> graphicsCardId = ProductId($"performance.{suffix}.graphics");
            StableId<ProductDefinitionIdScope> powerSupplyId = ProductId($"performance.{suffix}.power-supply");
            StableId<ProductDefinitionIdScope> powerCableId = ProductId($"performance.{suffix}.power-cable");
            ProductCatalog products = ProductCatalog.Create(new[]
            {
                Product(motherboardId, $"Motherboard {suffix}"),
                Product(processorId, $"Processor {suffix}"),
                Product(memoryId, $"Memory {suffix}"),
                Product(storageId, $"Storage {suffix}"),
                Product(coolerId, $"Cooler {suffix}"),
                Product(graphicsCardId, $"Graphics {suffix}"),
                Product(powerSupplyId, $"Power Supply {suffix}"),
                Product(powerCableId, $"Power Cable {suffix}")
            }).Value;
            PcComponentCatalog components = PcComponentCatalog.Create(
                products,
                new[]
                {
                    PcComponentSpecification.CreateMotherboard(
                        products, motherboardId, MotherboardFormFactor.MicroAtx, CpuSocketFamily.Lga1700).Value,
                    PcComponentSpecification.CreateProcessor(
                        products, processorId, CpuSocketFamily.Lga1700).Value,
                    PcComponentSpecification.CreateMemoryModule(
                        products, memoryId, DimmType.Ddr5Udimm).Value,
                    PcComponentSpecification.CreateStorageDevice(
                        products,
                        storageId,
                        M2StorageType.NvmePcie4X4_2280).Value,
                    PcComponentSpecification.CreateProcessorCooler(
                        products,
                        coolerId,
                        ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                        CpuSocketFamily.Lga1700).Value,
                    PcComponentSpecification.CreateGraphicsCard(
                        products,
                        graphicsCardId,
                        GraphicsCardType.Pcie4X16FullHeightDualSlot).Value,
                    PcComponentSpecification.CreatePowerSupply(
                        products, powerSupplyId, PowerSupplyType.AtxPs2).Value,
                    PcComponentSpecification.CreatePowerCable(
                        products, powerCableId, PowerCableType.ModularAtx24SplitPsuToMotherboard).Value
                }).Value;
            return new Fixture(
                components,
                motherboardId,
                processorId,
                memoryId,
                storageId,
                coolerId,
                graphicsCardId,
                powerSupplyId,
                powerCableId);
        }

        private static ProductDefinition Product(
            StableId<ProductDefinitionIdScope> id,
            string name)
        {
            return ProductDefinition.Create(
                id,
                StableId<ProductCategoryIdScope>.Parse("catalog.category.performance"),
                name,
                ProductTrackingPolicy.SerializedInstance,
                365).Value;
        }

        private static StableId<PcPerformanceCatalogIdScope> CatalogId(string value)
        {
            return StableId<PcPerformanceCatalogIdScope>.Parse(value);
        }

        private static StableId<ProductDefinitionIdScope> ProductId(string value)
        {
            return StableId<ProductDefinitionIdScope>.Parse(value);
        }

        private sealed class Fixture
        {
            public Fixture(
                PcComponentCatalog components,
                StableId<ProductDefinitionIdScope> motherboardId,
                StableId<ProductDefinitionIdScope> processorId,
                StableId<ProductDefinitionIdScope> memoryId,
                StableId<ProductDefinitionIdScope> storageId,
                StableId<ProductDefinitionIdScope> coolerId,
                StableId<ProductDefinitionIdScope> graphicsCardId,
                StableId<ProductDefinitionIdScope> powerSupplyId,
                StableId<ProductDefinitionIdScope> powerCableId)
            {
                Components = components;
                MotherboardId = motherboardId;
                ProcessorId = processorId;
                MemoryId = memoryId;
                StorageId = storageId;
                CoolerId = coolerId;
                GraphicsCardId = graphicsCardId;
                PowerSupplyId = powerSupplyId;
                PowerCableId = powerCableId;
            }

            public PcComponentCatalog Components { get; }
            public StableId<ProductDefinitionIdScope> MotherboardId { get; }
            public StableId<ProductDefinitionIdScope> ProcessorId { get; }
            public StableId<ProductDefinitionIdScope> MemoryId { get; }
            public StableId<ProductDefinitionIdScope> StorageId { get; }
            public StableId<ProductDefinitionIdScope> CoolerId { get; }
            public StableId<ProductDefinitionIdScope> GraphicsCardId { get; }
            public StableId<ProductDefinitionIdScope> PowerSupplyId { get; }
            public StableId<ProductDefinitionIdScope> PowerCableId { get; }
        }
    }
}
