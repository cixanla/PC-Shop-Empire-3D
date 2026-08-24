using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class PcieGpuPowerCableModelsTests
    {
        private static readonly StableId<AssemblyPowerCableRouteIdScope> RouteId =
            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                "assembly.route.power-cable-pcie-gpu-models");
        private static readonly StableId<AssemblyPowerCableEndpointIdScope>
            PsuEndpointId = EndpointId("psu-pcie-gpu-models");
        private static readonly StableId<AssemblyPowerCableEndpointIdScope>
            GraphicsCardEndpointId = EndpointId("graphics-card-pcie-gpu-models");
        private static readonly StableId<AssemblyPowerCableWaypointIdScope> Waypoint1 =
            WaypointId("01");
        private static readonly StableId<AssemblyPowerCableWaypointIdScope> Waypoint2 =
            WaypointId("02");
        private static readonly StableId<AssemblyPowerCableWaypointIdScope> Waypoint3 =
            WaypointId("03");
        private static readonly StableId<ProductDefinitionIdScope> ProductId =
            StableId<ProductDefinitionIdScope>.Parse(
                "catalog.cable.pcie-gpu-models");
        private static readonly StableId<ContainerIdScope> RouteContainerId =
            StableId<ContainerIdScope>.Parse(
                "inventory.container.pcie-gpu-models-route");

        [Test]
        public void PersistedPcieGpuValuesAreExplicitAndAppendOnly()
        {
            Assert.That((int)PcComponentKind.PowerCable, Is.EqualTo(8));
            Assert.That(
                (int)PowerCableType.ModularAtx24SplitPsuToMotherboard,
                Is.EqualTo(1));
            Assert.That(
                (int)PowerCableType.ModularPcie8PinPsuToGraphicsCard,
                Is.EqualTo(3));
            Assert.That(
                (int)PowerCableConnectorType.PsuModularPcie8,
                Is.EqualTo(6));
            Assert.That(
                (int)PowerCableConnectorType.GraphicsCardPcie8,
                Is.EqualTo(7));
            Assert.That((int)PcieGpuPowerCableState.Unsupported, Is.Zero);
            Assert.That((int)PcieGpuPowerCableState.Loose, Is.EqualTo(1));
            Assert.That((int)PcieGpuPowerCableState.Routed, Is.EqualTo(2));
            Assert.That((int)PcieGpuPowerCableOperationKind.Route, Is.EqualTo(1));
            Assert.That((int)PcieGpuPowerCableOperationKind.Unroute, Is.EqualTo(2));

            PowerCableEndpointDefinition psu = PsuEndpoint();
            PowerCableEndpointDefinition graphicsCard = GraphicsCardEndpoint();
            Assert.That(psu.PinCount, Is.EqualTo(8));
            Assert.That(graphicsCard.PinCount, Is.EqualTo(8));
            Assert.That(psu.Capacity, Is.EqualTo(1));
            Assert.That(graphicsCard.Capacity, Is.EqualTo(1));
        }

        [Test]
        public void TopologyAndDefinitionKeepExactOrderedDeterministicIdentity()
        {
            PcieGpuPowerCableTopology first = CreateTopology();
            PcieGpuPowerCableTopology valueEqual = CreateTopology();
            PcieGpuPowerCableDefinition definition =
                PcieGpuPowerCableDefinition.Create(
                    ProductId,
                    RouteContainerId,
                    first).Value;

            Assert.That(first.IsValid, Is.True);
            Assert.That(first.RouteId, Is.EqualTo(RouteId));
            Assert.That(first.PsuEndpoint.EndpointId, Is.EqualTo(PsuEndpointId));
            Assert.That(first.GraphicsCardEndpoint.EndpointId,
                Is.EqualTo(GraphicsCardEndpointId));
            Assert.That(first.OrderedWaypoints,
                Is.EqualTo(new[] { Waypoint1, Waypoint2, Waypoint3 }));
            Assert.That(first.Fingerprint, Is.EqualTo(
                "assembly.route.power-cable-pcie-gpu-models|" +
                "assembly.endpoint.psu-pcie-gpu-models|6|" +
                "assembly.waypoint.pcie-gpu-models-01|" +
                "assembly.waypoint.pcie-gpu-models-02|" +
                "assembly.waypoint.pcie-gpu-models-03|" +
                "assembly.endpoint.graphics-card-pcie-gpu-models|7"));
            Assert.That(valueEqual.Fingerprint, Is.EqualTo(first.Fingerprint));
            Assert.That(definition.IsValid, Is.True);
            Assert.That(definition.ProductId, Is.EqualTo(ProductId));
            Assert.That(definition.RouteContainerId, Is.EqualTo(RouteContainerId));
            Assert.That(definition.Topology, Is.SameAs(first));
            Assert.That(default(PcieGpuPowerCableDefinition).IsValid, Is.False);
        }

        [Test]
        public void TopologyRejectsWrongEndpointRolesAndInvalidWaypointOrder()
        {
            Assert.That(PcieGpuPowerCableTopology.Create(
                    default,
                    PsuEndpoint(),
                    GraphicsCardEndpoint(),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableRoute));
            Assert.That(PcieGpuPowerCableTopology.Create(
                    RouteId,
                    Endpoint(
                        PsuEndpointId,
                        PowerCableConnectorType.PsuModularAtx24Primary18),
                    GraphicsCardEndpoint(),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableEndpointTopology));
            Assert.That(PcieGpuPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    Endpoint(
                        GraphicsCardEndpointId,
                        PowerCableConnectorType.MotherboardAtx24),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableEndpointTopology));
            Assert.That(PcieGpuPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    Endpoint(
                        PsuEndpointId,
                        PowerCableConnectorType.GraphicsCardPcie8),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableEndpointTopology));
            Assert.That(PcieGpuPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    GraphicsCardEndpoint(),
                    Waypoint1,
                    Waypoint1,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableWaypointTopology));
            Assert.That(PcieGpuPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    GraphicsCardEndpoint(),
                    Waypoint1,
                    Waypoint2,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableWaypointTopology));
        }

        [Test]
        public void DefinitionAndPublicFailureCodesFailClosed()
        {
            PcieGpuPowerCableTopology topology = CreateTopology();

            Assert.That(PcieGpuPowerCableDefinition.Create(
                    default,
                    default,
                    null).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableProduct));
            Assert.That(PcieGpuPowerCableDefinition.Create(
                    ProductId,
                    default,
                    null).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableRouteContainer));
            Assert.That(PcieGpuPowerCableDefinition.Create(
                    ProductId,
                    RouteContainerId,
                    null).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableTopology));
            Assert.That(PcieGpuPowerCableDefinition.Create(
                    ProductId,
                    RouteContainerId,
                    topology).IsSuccess,
                Is.True);
            Assert.That(
                AssemblyFailures.PowerCableHostGraphicsCardUnretained.Code,
                Is.EqualTo(
                    "assembly.power-cable.host-graphics-card-unretained"));
        }

        private static PcieGpuPowerCableTopology CreateTopology()
        {
            return PcieGpuPowerCableTopology.Create(
                RouteId,
                PsuEndpoint(),
                GraphicsCardEndpoint(),
                Waypoint1,
                Waypoint2,
                Waypoint3).Value;
        }

        private static PowerCableEndpointDefinition PsuEndpoint()
        {
            return Endpoint(
                PsuEndpointId,
                PowerCableConnectorType.PsuModularPcie8);
        }

        private static PowerCableEndpointDefinition GraphicsCardEndpoint()
        {
            return Endpoint(
                GraphicsCardEndpointId,
                PowerCableConnectorType.GraphicsCardPcie8);
        }

        private static PowerCableEndpointDefinition Endpoint(
            StableId<AssemblyPowerCableEndpointIdScope> endpointId,
            PowerCableConnectorType connectorType)
        {
            return PowerCableEndpointDefinition.Create(endpointId, connectorType).Value;
        }

        private static StableId<AssemblyPowerCableEndpointIdScope> EndpointId(
            string suffix)
        {
            return StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                $"assembly.endpoint.{suffix}");
        }

        private static StableId<AssemblyPowerCableWaypointIdScope> WaypointId(
            string suffix)
        {
            return StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                $"assembly.waypoint.pcie-gpu-models-{suffix}");
        }
    }
}
