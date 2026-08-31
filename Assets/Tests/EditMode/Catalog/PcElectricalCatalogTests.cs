using System;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Tests.EditMode.Catalog
{
    public sealed class PcElectricalCatalogTests
    {
        [Test]
        public void ElectricalCatalogKeepsExactProductKindAndPositiveWattContract()
        {
            Fixture fixture = CreateFixture("primary");
            PcElectricalSpecification processor =
                PcElectricalSpecification.CreateLoad(
                    fixture.Components,
                    fixture.ProcessorId,
                    125).Value;
            PcElectricalSpecification powerSupply =
                PcElectricalSpecification.CreatePowerSupply(
                    fixture.Components,
                    fixture.PowerSupplyId,
                    550).Value;

            OperationResult<PcElectricalCatalog> result = PcElectricalCatalog.Create(
                fixture.Components,
                new[] { powerSupply, processor });

            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            Assert.That(result.Value.Count, Is.EqualTo(2));
            Assert.That(result.Value.Specifications[0].ProductId.Value,
                Is.LessThan(result.Value.Specifications[1].ProductId.Value));
            Assert.That(result.Value.Get(fixture.ProcessorId).Value,
                Is.SameAs(processor));
            Assert.That(processor.ComponentKind, Is.EqualTo(PcComponentKind.Processor));
            Assert.That(processor.LoadWatts, Is.EqualTo(125));
            Assert.That(processor.RatedOutputWatts, Is.Zero);
            Assert.That(processor.IsLoadProfile, Is.True);
            Assert.That(powerSupply.ComponentKind,
                Is.EqualTo(PcComponentKind.PowerSupply));
            Assert.That(powerSupply.LoadWatts, Is.Zero);
            Assert.That(powerSupply.RatedOutputWatts, Is.EqualTo(550));
            Assert.That(powerSupply.IsPowerSupplyProfile, Is.True);
        }

        [Test]
        public void ElectricalSpecificationRejectsUnknownWrongKindAndUnsafeWattValues()
        {
            Fixture fixture = CreateFixture("validation");

            Assert.That(PcElectricalSpecification.CreateLoad(
                    fixture.Components,
                    default,
                    125).Error,
                Is.EqualTo(PcElectricalCatalogFailures.InvalidProductId));
            Assert.That(PcElectricalSpecification.CreateLoad(
                    fixture.Components,
                    ProductId("catalog.unknown"),
                    125).Error,
                Is.EqualTo(PcElectricalCatalogFailures.UnknownComponentProduct));
            Assert.That(PcElectricalSpecification.CreateLoad(
                    fixture.Components,
                    fixture.MotherboardId,
                    35).Error,
                Is.EqualTo(PcElectricalCatalogFailures.UnsupportedLoadKind));
            Assert.That(PcElectricalSpecification.CreatePowerSupply(
                    fixture.Components,
                    fixture.ProcessorId,
                    550).Error,
                Is.EqualTo(PcElectricalCatalogFailures.PowerSupplyKindMismatch));
            Assert.That(PcElectricalSpecification.CreateLoad(
                    fixture.Components,
                    fixture.ProcessorId,
                    0).Error,
                Is.EqualTo(PcElectricalCatalogFailures.InvalidLoadWatts));
            Assert.That(PcElectricalSpecification.CreateLoad(
                    fixture.Components,
                    fixture.ProcessorId,
                    int.MaxValue).Error,
                Is.EqualTo(PcElectricalCatalogFailures.InvalidLoadWatts));
            Assert.That(PcElectricalSpecification.CreatePowerSupply(
                    fixture.Components,
                    fixture.PowerSupplyId,
                    -1).Error,
                Is.EqualTo(PcElectricalCatalogFailures.InvalidRatedOutputWatts));
        }

        [Test]
        public void ElectricalCatalogRejectsNullEmptyDuplicateAndForeignSpecifications()
        {
            Fixture first = CreateFixture("first");
            Fixture second = CreateFixture("second");
            PcElectricalSpecification firstProcessor =
                PcElectricalSpecification.CreateLoad(
                    first.Components,
                    first.ProcessorId,
                    125).Value;

            Assert.That(PcElectricalCatalog.Create(
                    first.Components,
                    null).Error,
                Is.EqualTo(PcElectricalCatalogFailures.EmptyCatalog));
            Assert.That(PcElectricalCatalog.Create(
                    first.Components,
                    Array.Empty<PcElectricalSpecification>()).Error,
                Is.EqualTo(PcElectricalCatalogFailures.EmptyCatalog));
            Assert.That(PcElectricalCatalog.Create(
                    first.Components,
                    new PcElectricalSpecification[] { null }).Error,
                Is.EqualTo(PcElectricalCatalogFailures.NullSpecification));
            Assert.That(PcElectricalCatalog.Create(
                    first.Components,
                    new[] { firstProcessor, firstProcessor }).Error,
                Is.EqualTo(PcElectricalCatalogFailures.DuplicateSpecification));
            Assert.That(PcElectricalCatalog.Create(
                    second.Components,
                    new[] { firstProcessor }).Error,
                Is.EqualTo(PcElectricalCatalogFailures.ComponentCatalogMismatch));
        }

        [Test]
        public void ElectricalCatalogUnknownLookupFailsClosed()
        {
            Fixture fixture = CreateFixture("lookup");
            PcElectricalCatalog catalog = PcElectricalCatalog.Create(
                fixture.Components,
                new[]
                {
                    PcElectricalSpecification.CreateLoad(
                        fixture.Components,
                        fixture.ProcessorId,
                        125).Value
                }).Value;

            Assert.That(catalog.Get(ProductId("catalog.lookup.unknown")).Error,
                Is.EqualTo(PcElectricalCatalogFailures.UnknownSpecification));
        }

        private static Fixture CreateFixture(string suffix)
        {
            StableId<ProductDefinitionIdScope> motherboardId =
                ProductId($"catalog.{suffix}.motherboard");
            StableId<ProductDefinitionIdScope> processorId =
                ProductId($"catalog.{suffix}.processor");
            StableId<ProductDefinitionIdScope> powerSupplyId =
                ProductId($"catalog.{suffix}.power-supply");
            ProductCatalog products = ProductCatalog.Create(new[]
            {
                Product(motherboardId, $"Motherboard {suffix}"),
                Product(processorId, $"Processor {suffix}"),
                Product(powerSupplyId, $"Power Supply {suffix}")
            }).Value;
            PcComponentCatalog components = PcComponentCatalog.Create(
                products,
                new[]
                {
                    PcComponentSpecification.CreateMotherboard(
                        products,
                        motherboardId,
                        MotherboardFormFactor.MicroAtx,
                        CpuSocketFamily.Lga1700).Value,
                    PcComponentSpecification.CreateProcessor(
                        products,
                        processorId,
                        CpuSocketFamily.Lga1700).Value,
                    PcComponentSpecification.CreatePowerSupply(
                        products,
                        powerSupplyId,
                        PowerSupplyType.AtxPs2).Value
                }).Value;
            return new Fixture(
                components,
                motherboardId,
                processorId,
                powerSupplyId);
        }

        private static ProductDefinition Product(
            StableId<ProductDefinitionIdScope> id,
            string name)
        {
            return ProductDefinition.Create(
                id,
                StableId<ProductCategoryIdScope>.Parse("catalog.category.test"),
                name,
                ProductTrackingPolicy.SerializedInstance,
                365).Value;
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
                StableId<ProductDefinitionIdScope> powerSupplyId)
            {
                Components = components;
                MotherboardId = motherboardId;
                ProcessorId = processorId;
                PowerSupplyId = powerSupplyId;
            }

            public PcComponentCatalog Components { get; }

            public StableId<ProductDefinitionIdScope> MotherboardId { get; }

            public StableId<ProductDefinitionIdScope> ProcessorId { get; }

            public StableId<ProductDefinitionIdScope> PowerSupplyId { get; }
        }
    }
}
