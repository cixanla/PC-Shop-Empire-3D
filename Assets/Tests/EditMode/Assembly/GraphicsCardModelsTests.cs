using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class GraphicsCardModelsTests
    {
        private static readonly StableId<AssemblySlotIdScope> SlotId =
            StableId<AssemblySlotIdScope>.Parse("assembly.slot.graphics-card-x16");
        private static readonly StableId<AssemblyGraphicsCardLatchIdScope> LatchId =
            StableId<AssemblyGraphicsCardLatchIdScope>.Parse(
                "assembly.latch.graphics-card-x16");
        private static readonly StableId<AssemblyFastenerIdScope> BracketFastenerId =
            StableId<AssemblyFastenerIdScope>.Parse(
                "assembly.fastener.graphics-card-bracket-01");
        private static readonly StableId<ContainerIdScope> ContainerId =
            StableId<ContainerIdScope>.Parse(
                "inventory.container.graphics-card-x16");

        [Test]
        public void PersistedGraphicsCardValuesAreExplicitAndAppendOnly()
        {
            Assert.That((int)PcComponentKind.ProcessorCooler, Is.EqualTo(5));
            Assert.That((int)PcComponentKind.GraphicsCard, Is.EqualTo(6));
            Assert.That((int)GraphicsCardType.Pcie4X16FullHeightDualSlot,
                Is.EqualTo(1));
            Assert.That((int)GraphicsCardSlotState.Unsupported, Is.Zero);
            Assert.That((int)GraphicsCardSlotState.EmptyOpen, Is.EqualTo(1));
            Assert.That((int)GraphicsCardSlotState.GraphicsCardSeatedUnsecured,
                Is.EqualTo(2));
            Assert.That((int)GraphicsCardSlotState.GraphicsCardRetained,
                Is.EqualTo(3));
            Assert.That((int)GraphicsCardMountOrientation.Primary, Is.EqualTo(1));
            Assert.That((int)GraphicsCardMountOrientation.Rotated180,
                Is.EqualTo(2));
        }

        [Test]
        public void TopologyAndSlotKeepExactStableIdentity()
        {
            GraphicsCardRetentionTopology topology = CreateTopology();
            GraphicsCardSlotDefinition definition =
                GraphicsCardSlotDefinition.Create(
                    SlotId,
                    ContainerId,
                    topology,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot).Value;

            Assert.That(topology.IsValid, Is.True);
            Assert.That(topology.LatchId, Is.EqualTo(LatchId));
            Assert.That(topology.BracketFastenerId,
                Is.EqualTo(BracketFastenerId));
            Assert.That(definition.IsValid, Is.True);
            Assert.That(definition.SlotId, Is.EqualTo(SlotId));
            Assert.That(definition.ContainerId, Is.EqualTo(ContainerId));
            Assert.That(definition.RetentionTopology, Is.SameAs(topology));
            Assert.That(definition.SupportedGraphicsCardType,
                Is.EqualTo(GraphicsCardType.Pcie4X16FullHeightDualSlot));
            Assert.That(default(GraphicsCardSlotDefinition).IsValid, Is.False);
        }

        [Test]
        public void TopologyAndSlotRejectInvalidInputsInDeterministicOrder()
        {
            Assert.That(GraphicsCardRetentionTopology.Create(
                    default,
                    BracketFastenerId).Error,
                Is.EqualTo(AssemblyFailures.InvalidGraphicsCardSlotLatch));
            Assert.That(GraphicsCardRetentionTopology.Create(
                    LatchId,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidGraphicsCardBracketFastener));

            GraphicsCardRetentionTopology topology = CreateTopology();
            Assert.That(GraphicsCardSlotDefinition.Create(
                    default,
                    default,
                    null,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidSlotId));
            Assert.That(GraphicsCardSlotDefinition.Create(
                    SlotId,
                    default,
                    null,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidGraphicsCardSlotContainer));
            Assert.That(GraphicsCardSlotDefinition.Create(
                    SlotId,
                    ContainerId,
                    null,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidGraphicsCardSlotLatch));
            Assert.That(GraphicsCardSlotDefinition.Create(
                    SlotId,
                    ContainerId,
                    topology,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidGraphicsCardType));
        }

        [Test]
        public void CompatibilityRequiresGraphicsCardKindTypeAndPrimaryOrientation()
        {
            ProductCatalog products = ProductCatalog.Create(new[]
            {
                Definition("component.graphics-card-models-motherboard"),
                Definition("component.graphics-card-models-card")
            }).Value;
            PcComponentSpecification motherboard =
                PcComponentSpecification.CreateMotherboard(
                    products,
                    ProductId("component.graphics-card-models-motherboard"),
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700,
                    DimmType.Ddr5Udimm,
                    M2StorageType.NvmePcie4X4_2280,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot).Value;
            PcComponentSpecification graphicsCard =
                PcComponentSpecification.CreateGraphicsCard(
                    products,
                    ProductId("component.graphics-card-models-card"),
                    GraphicsCardType.Pcie4X16FullHeightDualSlot).Value;

            Assert.That(AssemblyCompatibilityEvaluator.EvaluateGraphicsCardSeat(
                    graphicsCard,
                    motherboard,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot,
                    GraphicsCardMountOrientation.Primary).IsCompatible,
                Is.True);
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateGraphicsCardSeat(
                    graphicsCard,
                    motherboard,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot,
                    default).Reason,
                Is.EqualTo(AssemblyFailures.InvalidGraphicsCardOrientation));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateGraphicsCardSeat(
                    graphicsCard,
                    motherboard,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot,
                    GraphicsCardMountOrientation.Rotated180).Reason,
                Is.EqualTo(AssemblyFailures.GraphicsCardOrientationMismatch));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluateGraphicsCardSeat(
                    motherboard,
                    motherboard,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot,
                    GraphicsCardMountOrientation.Primary).Reason,
                Is.EqualTo(AssemblyFailures.UnsupportedComponentKind));
        }

        [Test]
        public void PublicFailureCodesMatchIssue59Contract()
        {
            Assert.That(AssemblyFailures.InvalidGraphicsCardSlotContainer.Code,
                Is.EqualTo("assembly.graphics-card-slot-container.invalid"));
            Assert.That(AssemblyFailures.InvalidGraphicsCardSlotDefinition.Code,
                Is.EqualTo("assembly.graphics-card-slot-definition.invalid"));
            Assert.That(AssemblyFailures.InvalidGraphicsCardSlotLatch.Code,
                Is.EqualTo("assembly.graphics-card-slot-latch.invalid"));
            Assert.That(AssemblyFailures.InvalidGraphicsCardBracketFastener.Code,
                Is.EqualTo("assembly.graphics-card-bracket-fastener.invalid"));
            Assert.That(AssemblyFailures.InvalidGraphicsCardType.Code,
                Is.EqualTo("assembly.graphics-card-type.invalid"));
            Assert.That(AssemblyFailures.InvalidGraphicsCardOrientation.Code,
                Is.EqualTo("assembly.graphics-card-orientation.invalid"));
            Assert.That(AssemblyFailures.GraphicsCardOrientationMismatch.Code,
                Is.EqualTo("assembly.graphics-card-orientation.mismatch"));
            Assert.That(AssemblyFailures.GraphicsCardSlotOccupied.Code,
                Is.EqualTo("assembly.graphics-card-slot.occupied"));
            Assert.That(AssemblyFailures.GraphicsCardInstalled.Code,
                Is.EqualTo("assembly.motherboard.graphics-card-installed"));
            Assert.That(AssemblyFailures.GraphicsCardRetentionOutOfOrder.Code,
                Is.EqualTo("assembly.graphics-card-retention.out-of-order"));
            Assert.That(AssemblyFailures.GraphicsCardRetained.Code,
                Is.EqualTo("assembly.graphics-card.retained"));
            Assert.That(AssemblyFailures.GraphicsCardTypeMismatch.Code,
                Is.EqualTo("assembly.graphics-card-type.mismatch"));
            Assert.That(AssemblyFailures.GraphicsCardSlotCapacityExceeded.Code,
                Is.EqualTo("assembly.graphics-card-slot.capacity"));
            Assert.That(AssemblyFailures.GraphicsCardMissing.Code,
                Is.EqualTo("assembly.benchmark.graphics-card-missing"));
            Assert.That(AssemblyFailures.GraphicsCardUnretained.Code,
                Is.EqualTo("assembly.benchmark.graphics-card-unretained"));
        }

        private static GraphicsCardRetentionTopology CreateTopology()
        {
            return GraphicsCardRetentionTopology.Create(
                LatchId,
                BracketFastenerId).Value;
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

        private static StableId<ProductDefinitionIdScope> ProductId(string id)
        {
            return StableId<ProductDefinitionIdScope>.Parse(id);
        }
    }
}
