using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class ProcessorCoolerModelsTests
    {
        private static readonly StableId<AssemblySlotIdScope> SlotId =
            StableId<AssemblySlotIdScope>.Parse("assembly.slot.processor-cooler-main");
        private static readonly StableId<AssemblyProcessorCoolerBracketIdScope> BracketId =
            StableId<AssemblyProcessorCoolerBracketIdScope>.Parse(
                "assembly.bracket.processor-cooler-lga1700");
        private static readonly StableId<ContainerIdScope> ContainerId =
            StableId<ContainerIdScope>.Parse(
                "inventory.container.processor-cooler-slot-main");

        [Test]
        public void PersistedCoolerValuesAreExplicitAndAppendOnly()
        {
            Assert.That((int)PcComponentKind.Motherboard, Is.EqualTo(1));
            Assert.That((int)PcComponentKind.Processor, Is.EqualTo(2));
            Assert.That((int)PcComponentKind.MemoryModule, Is.EqualTo(3));
            Assert.That((int)PcComponentKind.StorageDevice, Is.EqualTo(4));
            Assert.That((int)PcComponentKind.ProcessorCooler, Is.EqualTo(5));
            Assert.That((int)ProcessorCoolerSlotState.Unsupported, Is.Zero);
            Assert.That((int)ProcessorCoolerSlotState.EmptyOpen, Is.EqualTo(1));
            Assert.That((int)ProcessorCoolerSlotState.CoolerSeatedUnsecured,
                Is.EqualTo(2));
            Assert.That((int)ProcessorCoolerSlotState.CoolerRetained, Is.EqualTo(3));
            Assert.That((int)ProcessorCoolerMountOrientation.Primary, Is.EqualTo(1));
            Assert.That((int)ProcessorCoolerMountOrientation.Rotated180,
                Is.EqualTo(2));
            Assert.That((int)ProcessorCoolerTimState.Unsupported, Is.Zero);
            Assert.That((int)ProcessorCoolerTimState.PreAppliedUnused, Is.EqualTo(1));
            Assert.That((int)ProcessorCoolerTimState.AppliedConsumed, Is.EqualTo(2));
        }

        [Test]
        public void FourPointTopologyKeepsExactPhysicalCrossAndReverseOrder()
        {
            ProcessorCoolerRetentionTopology topology = CreateTopology();

            Assert.That(topology.IsValid, Is.True);
            Assert.That(topology.PhysicalOrder, Is.EqualTo(new[]
            {
                Point(1), Point(2), Point(3), Point(4)
            }));
            Assert.That(topology.CrossRetentionOrder, Is.EqualTo(new[]
            {
                Point(1), Point(3), Point(2), Point(4)
            }));
            Assert.That(topology.ReverseCrossRetentionOrder, Is.EqualTo(new[]
            {
                Point(4), Point(2), Point(3), Point(1)
            }));
            Assert.That(topology.CrossRetentionOrder,
                Is.InstanceOf<IReadOnlyList<StableId<
                    AssemblyProcessorCoolerRetentionPointIdScope>>>());
        }

        [Test]
        public void TopologyAndSlotRejectInvalidIdentityInDeterministicOrder()
        {
            Assert.That(ProcessorCoolerRetentionTopology.Create(
                    default, default, default, default).Error,
                Is.EqualTo(AssemblyFailures.InvalidProcessorCoolerRetentionTopology));
            Assert.That(ProcessorCoolerRetentionTopology.Create(
                    Point(1), Point(1), Point(3), Point(4)).Error,
                Is.EqualTo(AssemblyFailures.InvalidProcessorCoolerRetentionTopology));

            ProcessorCoolerRetentionTopology topology = CreateTopology();
            Assert.That(ProcessorCoolerSlotDefinition.Create(
                    default,
                    default,
                    default,
                    topology,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidSlotId));
            Assert.That(ProcessorCoolerSlotDefinition.Create(
                    SlotId,
                    default,
                    default,
                    topology,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidProcessorCoolerBracket));
            Assert.That(ProcessorCoolerSlotDefinition.Create(
                    SlotId,
                    BracketId,
                    default,
                    topology,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidProcessorCoolerSlotContainer));
            Assert.That(ProcessorCoolerSlotDefinition.Create(
                    SlotId,
                    BracketId,
                    ContainerId,
                    topology,
                    default,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidProcessorCoolerType));
            Assert.That(ProcessorCoolerSlotDefinition.Create(
                    SlotId,
                    BracketId,
                    ContainerId,
                    topology,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidCpuSocketFamily));
            Assert.That(ProcessorCoolerSlotDefinition.Create(
                    SlotId,
                    BracketId,
                    ContainerId,
                    topology,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Am5).Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerSocketMismatch));
        }

        [Test]
        public void SlotDefinitionKeepsExactTypedLga1700Topology()
        {
            ProcessorCoolerSlotDefinition definition =
                ProcessorCoolerSlotDefinition.Create(
                    SlotId,
                    BracketId,
                    ContainerId,
                    CreateTopology(),
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700).Value;

            Assert.That(definition.IsValid, Is.True);
            Assert.That(definition.SlotId, Is.EqualTo(SlotId));
            Assert.That(definition.BracketId, Is.EqualTo(BracketId));
            Assert.That(definition.ContainerId, Is.EqualTo(ContainerId));
            Assert.That(definition.SupportedCoolerType,
                Is.EqualTo(ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim));
            Assert.That(definition.SupportedSocketFamily,
                Is.EqualTo(CpuSocketFamily.Lga1700));
            Assert.That(definition.RetentionTopology.CrossRetentionOrder,
                Is.EqualTo(new[] { Point(1), Point(3), Point(2), Point(4) }));
            Assert.That(default(ProcessorCoolerSlotDefinition).IsValid, Is.False);
        }

        [Test]
        public void CompatibilityRequiresExactKindsSocketTypeAndOneOfTwoOrientations()
        {
            ProductCatalog products = ProductCatalog.Create(new[]
            {
                Definition("component.motherboard"),
                Definition("component.processor"),
                Definition("component.cooler")
            }).Value;
            PcComponentSpecification motherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    ProductId("component.motherboard"),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    DimmType.Ddr5Udimm,
                    M2StorageType.NvmePcie4X4_2280).Value;
            PcComponentSpecification processor =
                PcComponentSpecification.CreateProcessor(
                    products,
                    ProductId("component.processor"),
                    CpuSocketFamily.Lga1700).Value;
            PcComponentSpecification cooler =
                PcComponentSpecification.CreateProcessorCooler(
                    products,
                    ProductId("component.cooler"),
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700).Value;

            foreach (ProcessorCoolerMountOrientation orientation in new[]
                     {
                         ProcessorCoolerMountOrientation.Primary,
                         ProcessorCoolerMountOrientation.Rotated180
                     })
            {
                Assert.That(AssemblyCompatibilityEvaluator.EvaluateProcessorCoolerSeat(
                        cooler,
                        motherboard,
                        processor,
                        ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                        CpuSocketFamily.Lga1700,
                        orientation).IsCompatible,
                    Is.True);
            }

            Assert.That(AssemblyCompatibilityEvaluator.EvaluateProcessorCoolerSeat(
                    cooler,
                    motherboard,
                    processor,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700,
                    default).Reason,
                Is.EqualTo(AssemblyFailures.InvalidProcessorCoolerOrientation));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateProcessorCoolerSeat(
                    processor,
                    motherboard,
                    processor,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700,
                    ProcessorCoolerMountOrientation.Primary).Reason,
                Is.EqualTo(AssemblyFailures.UnsupportedComponentKind));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateProcessorCoolerSeat(
                    cooler,
                    motherboard,
                    processor,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Am5,
                    ProcessorCoolerMountOrientation.Primary).Reason,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerSocketMismatch));
        }

        private static ProcessorCoolerRetentionTopology CreateTopology()
        {
            return ProcessorCoolerRetentionTopology.Create(
                Point(1), Point(2), Point(3), Point(4)).Value;
        }

        private static StableId<AssemblyProcessorCoolerRetentionPointIdScope> Point(
            int index)
        {
            return StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(
                $"assembly.retention-point.processor-cooler-{index}");
        }

        private static ProductDefinition Definition(string id)
        {
            return ProductDefinition.Create(
                ProductId(id),
                StableId<ProductCategoryIdScope>.Parse("pc-components"),
                id,
                ProductTrackingPolicy.SerializedInstance,
                730).Value;
        }

        private static StableId<ProductDefinitionIdScope> ProductId(string value)
        {
            return StableId<ProductDefinitionIdScope>.Parse(value);
        }
    }
}
