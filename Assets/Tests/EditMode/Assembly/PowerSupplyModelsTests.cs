using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class PowerSupplyModelsTests
    {
        private static readonly StableId<AssemblySlotIdScope> SlotId =
            StableId<AssemblySlotIdScope>.Parse("assembly.slot.power-supply-bottom-rear");
        private static readonly StableId<AssemblyPowerSupplyRearMountIdScope> RearMountId =
            StableId<AssemblyPowerSupplyRearMountIdScope>.Parse(
                "assembly.mount.power-supply-rear");
        private static readonly StableId<AssemblyFastenerIdScope> TopLeft = Fastener("01");
        private static readonly StableId<AssemblyFastenerIdScope> TopRight = Fastener("02");
        private static readonly StableId<AssemblyFastenerIdScope> BottomLeft = Fastener("03");
        private static readonly StableId<AssemblyFastenerIdScope> BottomRight = Fastener("04");
        private static readonly StableId<ContainerIdScope> ContainerId =
            StableId<ContainerIdScope>.Parse(
                "inventory.container.assembly-power-supply-bay");

        [Test]
        public void PersistedPowerSupplyValuesAreExplicitAndAppendOnly()
        {
            Assert.That((int)PcComponentKind.GraphicsCard, Is.EqualTo(6));
            Assert.That((int)PcComponentKind.PowerSupply, Is.EqualTo(7));
            Assert.That((int)PowerSupplyType.AtxPs2, Is.EqualTo(1));
            Assert.That((int)PowerSupplyBayState.Unsupported, Is.Zero);
            Assert.That((int)PowerSupplyBayState.EmptyOpen, Is.EqualTo(1));
            Assert.That((int)PowerSupplyBayState.PowerSupplySeatedUnsecured,
                Is.EqualTo(2));
            Assert.That((int)PowerSupplyBayState.PowerSupplyRetained, Is.EqualTo(3));
            Assert.That((int)PowerSupplyMountOrientation.FanToFilteredVent,
                Is.EqualTo(1));
            Assert.That((int)PowerSupplyMountOrientation.FanAwayFromFilteredVent,
                Is.EqualTo(2));
            Assert.That((int)AssemblyOperationKind.SeatPowerSupply, Is.EqualTo(25));
            Assert.That((int)AssemblyOperationKind.RemovePowerSupply, Is.EqualTo(26));
            Assert.That((int)AssemblyOperationKind.RetainPowerSupply, Is.EqualTo(27));
            Assert.That((int)AssemblyOperationKind.UnretainPowerSupply, Is.EqualTo(28));
        }

        [Test]
        public void TopologyAndBayKeepExactStableIdentityAndDeterministicOrders()
        {
            PowerSupplyRetentionTopology topology = CreateTopology();
            PowerSupplyBayDefinition definition = PowerSupplyBayDefinition.Create(
                SlotId,
                ContainerId,
                topology,
                PowerSupplyType.AtxPs2).Value;

            Assert.That(topology.IsValid, Is.True);
            Assert.That(topology.RearMountId, Is.EqualTo(RearMountId));
            Assert.That(topology.PhysicalOrder,
                Is.EqualTo(new[] { TopLeft, TopRight, BottomLeft, BottomRight }));
            Assert.That(topology.DeterministicRetentionOrder,
                Is.EqualTo(new[] { TopLeft, BottomRight, TopRight, BottomLeft }));
            Assert.That(topology.ReverseRetentionOrder,
                Is.EqualTo(new[] { BottomLeft, TopRight, BottomRight, TopLeft }));
            Assert.That(definition.IsValid, Is.True);
            Assert.That(definition.SlotId, Is.EqualTo(SlotId));
            Assert.That(definition.ContainerId, Is.EqualTo(ContainerId));
            Assert.That(definition.RetentionTopology, Is.SameAs(topology));
            Assert.That(definition.SupportedPowerSupplyType,
                Is.EqualTo(PowerSupplyType.AtxPs2));
            Assert.That(default(PowerSupplyBayDefinition).IsValid, Is.False);
        }

        [Test]
        public void TopologyAndBayRejectInvalidInputsInDeterministicOrder()
        {
            Assert.That(PowerSupplyRetentionTopology.Create(
                    default,
                    TopLeft,
                    TopRight,
                    BottomLeft,
                    BottomRight).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerSupplyRearMount));
            Assert.That(PowerSupplyRetentionTopology.Create(
                    RearMountId,
                    TopLeft,
                    TopLeft,
                    BottomLeft,
                    BottomRight).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerSupplyFastenerTopology));
            Assert.That(PowerSupplyRetentionTopology.Create(
                    RearMountId,
                    TopLeft,
                    TopRight,
                    BottomLeft,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerSupplyFastenerTopology));

            PowerSupplyRetentionTopology topology = CreateTopology();
            Assert.That(PowerSupplyBayDefinition.Create(
                    default,
                    default,
                    null,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidSlotId));
            Assert.That(PowerSupplyBayDefinition.Create(
                    SlotId,
                    default,
                    null,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerSupplyBayContainer));
            Assert.That(PowerSupplyBayDefinition.Create(
                    SlotId,
                    ContainerId,
                    null,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerSupplyRearMount));
            Assert.That(PowerSupplyBayDefinition.Create(
                    SlotId,
                    ContainerId,
                    topology,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerSupplyType));
        }

        [Test]
        public void CompatibilityRequiresPowerSupplyKindAtxPs2AndFilteredVentOrientation()
        {
            ProductCatalog products = ProductCatalog.Create(new[]
            {
                Definition("component.power-supply-models-psu"),
                Definition("component.power-supply-models-motherboard")
            }).Value;
            PcComponentSpecification powerSupply =
                PcComponentSpecification.CreatePowerSupply(
                    products,
                    ProductId("component.power-supply-models-psu"),
                    PowerSupplyType.AtxPs2).Value;
            PcComponentSpecification motherboard =
                PcComponentSpecification.Create(
                    products,
                    ProductId("component.power-supply-models-motherboard"),
                    PcComponentKind.Motherboard,
                    MotherboardFormFactor.MicroAtx).Value;

            Assert.That(AssemblyCompatibilityEvaluator.EvaluatePowerSupplySeat(
                    powerSupply,
                    PowerSupplyType.AtxPs2,
                    PowerSupplyMountOrientation.FanToFilteredVent).IsCompatible,
                Is.True);
            Assert.That(AssemblyCompatibilityEvaluator.EvaluatePowerSupplySeat(
                    powerSupply,
                    PowerSupplyType.AtxPs2,
                    default).Reason,
                Is.EqualTo(AssemblyFailures.InvalidPowerSupplyOrientation));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluatePowerSupplySeat(
                    powerSupply,
                    PowerSupplyType.AtxPs2,
                    PowerSupplyMountOrientation.FanAwayFromFilteredVent).Reason,
                Is.EqualTo(AssemblyFailures.PowerSupplyOrientationMismatch));
            Assert.That(AssemblyCompatibilityEvaluator.EvaluatePowerSupplySeat(
                    motherboard,
                    PowerSupplyType.AtxPs2,
                    PowerSupplyMountOrientation.FanToFilteredVent).Reason,
                Is.EqualTo(AssemblyFailures.UnsupportedComponentKind));
        }

        [Test]
        public void PublicFailureCodesMatchIssue60Contract()
        {
            Assert.That(AssemblyFailures.InvalidPowerSupplyBayContainer.Code,
                Is.EqualTo("assembly.power-supply-bay-container.invalid"));
            Assert.That(AssemblyFailures.InvalidPowerSupplyBayDefinition.Code,
                Is.EqualTo("assembly.power-supply-bay-definition.invalid"));
            Assert.That(AssemblyFailures.InvalidPowerSupplyRearMount.Code,
                Is.EqualTo("assembly.power-supply-rear-mount.invalid"));
            Assert.That(AssemblyFailures.InvalidPowerSupplyFastenerTopology.Code,
                Is.EqualTo("assembly.power-supply-fastener-topology.invalid"));
            Assert.That(AssemblyFailures.InvalidPowerSupplyType.Code,
                Is.EqualTo("assembly.power-supply-type.invalid"));
            Assert.That(AssemblyFailures.InvalidPowerSupplyOrientation.Code,
                Is.EqualTo("assembly.power-supply-orientation.invalid"));
            Assert.That(AssemblyFailures.PowerSupplyOrientationMismatch.Code,
                Is.EqualTo("assembly.power-supply-orientation.mismatch"));
            Assert.That(AssemblyFailures.PowerSupplyBayOccupied.Code,
                Is.EqualTo("assembly.power-supply-bay.occupied"));
            Assert.That(AssemblyFailures.PowerSupplyRetentionOutOfOrder.Code,
                Is.EqualTo("assembly.power-supply-retention.out-of-order"));
            Assert.That(AssemblyFailures.PowerSupplyRetained.Code,
                Is.EqualTo("assembly.power-supply.retained"));
            Assert.That(AssemblyFailures.PowerSupplyTypeMismatch.Code,
                Is.EqualTo("assembly.power-supply-type.mismatch"));
            Assert.That(AssemblyFailures.PowerSupplyBayCapacityExceeded.Code,
                Is.EqualTo("assembly.power-supply-bay.capacity"));
            Assert.That(AssemblyFailures.PowerSupplyMissing.Code,
                Is.EqualTo("assembly.benchmark.power-supply-missing"));
            Assert.That(AssemblyFailures.PowerSupplyUnretained.Code,
                Is.EqualTo("assembly.benchmark.power-supply-unretained"));
        }

        private static PowerSupplyRetentionTopology CreateTopology()
        {
            return PowerSupplyRetentionTopology.Create(
                RearMountId,
                TopLeft,
                TopRight,
                BottomLeft,
                BottomRight).Value;
        }

        private static StableId<AssemblyFastenerIdScope> Fastener(string suffix)
        {
            return StableId<AssemblyFastenerIdScope>.Parse(
                $"assembly.fastener.power-supply-rear-{suffix}");
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
