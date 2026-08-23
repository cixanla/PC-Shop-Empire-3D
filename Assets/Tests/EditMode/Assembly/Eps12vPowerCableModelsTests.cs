using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class Eps12vPowerCableModelsTests
    {
        private static readonly StableId<AssemblyPowerCableRouteIdScope> RouteId =
            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                "assembly.route.power-cable-eps12v-models");
        private static readonly StableId<AssemblyPowerCableEndpointIdScope>
            PsuEndpointId = EndpointId("psu-eps12v-models");
        private static readonly StableId<AssemblyPowerCableEndpointIdScope>
            MotherboardEndpointId = EndpointId("motherboard-eps12v-models");
        private static readonly StableId<AssemblyPowerCableWaypointIdScope> Waypoint1 =
            WaypointId("01");
        private static readonly StableId<AssemblyPowerCableWaypointIdScope> Waypoint2 =
            WaypointId("02");
        private static readonly StableId<AssemblyPowerCableWaypointIdScope> Waypoint3 =
            WaypointId("03");
        private static readonly StableId<ProductDefinitionIdScope> ProductId =
            StableId<ProductDefinitionIdScope>.Parse(
                "catalog.cable.eps12v-models");
        private static readonly StableId<ContainerIdScope> RouteContainerId =
            StableId<ContainerIdScope>.Parse(
                "inventory.container.eps12v-models-route");

        [Test]
        public void PersistedEps12vValuesAreExplicitAndAppendOnly()
        {
            Assert.That((int)PcComponentKind.PowerCable, Is.EqualTo(8));
            Assert.That(
                (int)PowerCableType.ModularAtx24SplitPsuToMotherboard,
                Is.EqualTo(1));
            Assert.That(
                (int)PowerCableType.ModularEps12v8PinPsuToMotherboard,
                Is.EqualTo(2));
            Assert.That(
                (int)PowerCableConnectorType.PsuModularEps12v8,
                Is.EqualTo(4));
            Assert.That(
                (int)PowerCableConnectorType.MotherboardEps12v8,
                Is.EqualTo(5));
            Assert.That((int)Eps12vPowerCableState.Unsupported, Is.Zero);
            Assert.That((int)Eps12vPowerCableState.Loose, Is.EqualTo(1));
            Assert.That((int)Eps12vPowerCableState.Routed, Is.EqualTo(2));
            Assert.That((int)Eps12vPowerCableOperationKind.Route, Is.EqualTo(1));
            Assert.That((int)Eps12vPowerCableOperationKind.Unroute, Is.EqualTo(2));

            PowerCableEndpointDefinition psu = PsuEndpoint();
            PowerCableEndpointDefinition motherboard = MotherboardEndpoint();
            Assert.That(psu.PinCount, Is.EqualTo(8));
            Assert.That(motherboard.PinCount, Is.EqualTo(8));
            Assert.That(psu.Capacity, Is.EqualTo(1));
            Assert.That(motherboard.Capacity, Is.EqualTo(1));
        }

        [Test]
        public void TopologyAndDefinitionKeepExactOrderedDeterministicIdentity()
        {
            Eps12vPowerCableTopology first = CreateTopology();
            Eps12vPowerCableTopology valueEqual = CreateTopology();
            Eps12vPowerCableDefinition definition =
                Eps12vPowerCableDefinition.Create(
                    ProductId,
                    RouteContainerId,
                    first).Value;

            Assert.That(first.IsValid, Is.True);
            Assert.That(first.RouteId, Is.EqualTo(RouteId));
            Assert.That(first.PsuEndpoint.EndpointId, Is.EqualTo(PsuEndpointId));
            Assert.That(first.MotherboardEndpoint.EndpointId,
                Is.EqualTo(MotherboardEndpointId));
            Assert.That(first.OrderedWaypoints,
                Is.EqualTo(new[] { Waypoint1, Waypoint2, Waypoint3 }));
            Assert.That(first.Fingerprint, Is.EqualTo(
                "assembly.route.power-cable-eps12v-models|" +
                "assembly.endpoint.psu-eps12v-models|4|" +
                "assembly.waypoint.eps12v-models-01|" +
                "assembly.waypoint.eps12v-models-02|" +
                "assembly.waypoint.eps12v-models-03|" +
                "assembly.endpoint.motherboard-eps12v-models|5"));
            Assert.That(valueEqual.Fingerprint, Is.EqualTo(first.Fingerprint));
            Assert.That(definition.IsValid, Is.True);
            Assert.That(definition.ProductId, Is.EqualTo(ProductId));
            Assert.That(definition.RouteContainerId, Is.EqualTo(RouteContainerId));
            Assert.That(definition.Topology, Is.SameAs(first));
            Assert.That(default(Eps12vPowerCableDefinition).IsValid, Is.False);
        }

        [Test]
        public void TopologyRejectsWrongEndpointRolesAndInvalidWaypointOrder()
        {
            Assert.That(Eps12vPowerCableTopology.Create(
                    default,
                    PsuEndpoint(),
                    MotherboardEndpoint(),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableRoute));
            Assert.That(Eps12vPowerCableTopology.Create(
                    RouteId,
                    Endpoint(
                        PsuEndpointId,
                        PowerCableConnectorType.PsuModularAtx24Primary18),
                    MotherboardEndpoint(),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableEndpointTopology));
            Assert.That(Eps12vPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    Endpoint(
                        MotherboardEndpointId,
                        PowerCableConnectorType.MotherboardAtx24),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableEndpointTopology));
            Assert.That(Eps12vPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    Endpoint(
                        PsuEndpointId,
                        PowerCableConnectorType.MotherboardEps12v8),
                    Waypoint1,
                    Waypoint2,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableEndpointTopology));
            Assert.That(Eps12vPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    MotherboardEndpoint(),
                    Waypoint1,
                    Waypoint1,
                    Waypoint3).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableWaypointTopology));
            Assert.That(Eps12vPowerCableTopology.Create(
                    RouteId,
                    PsuEndpoint(),
                    MotherboardEndpoint(),
                    Waypoint1,
                    Waypoint2,
                    default).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableWaypointTopology));
        }

        [Test]
        public void DefinitionAndPublicFailureCodesFailClosed()
        {
            Eps12vPowerCableTopology topology = CreateTopology();

            Assert.That(Eps12vPowerCableDefinition.Create(
                    default,
                    default,
                    null).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableProduct));
            Assert.That(Eps12vPowerCableDefinition.Create(
                    ProductId,
                    default,
                    null).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableRouteContainer));
            Assert.That(Eps12vPowerCableDefinition.Create(
                    ProductId,
                    RouteContainerId,
                    null).Error,
                Is.EqualTo(AssemblyFailures.InvalidPowerCableTopology));
            Assert.That(Eps12vPowerCableDefinition.Create(
                    ProductId,
                    RouteContainerId,
                    topology).IsSuccess,
                Is.True);
            Assert.That(AssemblyFailures.PowerCableHostProcessorUnretained.Code,
                Is.EqualTo("assembly.power-cable.host-processor-unretained"));
        }

        private static Eps12vPowerCableTopology CreateTopology()
        {
            return Eps12vPowerCableTopology.Create(
                RouteId,
                PsuEndpoint(),
                MotherboardEndpoint(),
                Waypoint1,
                Waypoint2,
                Waypoint3).Value;
        }

        private static PowerCableEndpointDefinition PsuEndpoint()
        {
            return Endpoint(
                PsuEndpointId,
                PowerCableConnectorType.PsuModularEps12v8);
        }

        private static PowerCableEndpointDefinition MotherboardEndpoint()
        {
            return Endpoint(
                MotherboardEndpointId,
                PowerCableConnectorType.MotherboardEps12v8);
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
                $"assembly.waypoint.eps12v-models-{suffix}");
        }
    }
}
