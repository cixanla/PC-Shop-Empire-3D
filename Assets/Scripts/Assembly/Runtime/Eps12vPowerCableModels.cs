using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public enum Eps12vPowerCableState
    {
        Unsupported = 0,
        Loose = 1,
        Routed = 2
    }

    public enum Eps12vPowerCableOperationKind
    {
        Route = 1,
        Unroute = 2
    }

    /// <summary>
    /// Immutable two-endpoint EPS12V contract. The authored waypoint order is identity;
    /// no rope particles, joints or transient physics state participate in persistence.
    /// </summary>
    public sealed class Eps12vPowerCableTopology
    {
        private readonly IReadOnlyList<StableId<AssemblyPowerCableWaypointIdScope>>
            _orderedWaypoints;

        private Eps12vPowerCableTopology(
            StableId<AssemblyPowerCableRouteIdScope> routeId,
            PowerCableEndpointDefinition psuEndpoint,
            PowerCableEndpointDefinition motherboardEndpoint,
            StableId<AssemblyPowerCableWaypointIdScope> firstWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> secondWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> thirdWaypointId)
        {
            RouteId = routeId;
            PsuEndpoint = psuEndpoint;
            MotherboardEndpoint = motherboardEndpoint;
            FirstWaypointId = firstWaypointId;
            SecondWaypointId = secondWaypointId;
            ThirdWaypointId = thirdWaypointId;
            _orderedWaypoints = Array.AsReadOnly(new[]
            {
                firstWaypointId,
                secondWaypointId,
                thirdWaypointId
            });
            Fingerprint = string.Join(
                "|",
                RouteId.Value,
                PsuEndpoint.EndpointId.Value,
                ((int)PsuEndpoint.ConnectorType).ToString(),
                FirstWaypointId.Value,
                SecondWaypointId.Value,
                ThirdWaypointId.Value,
                MotherboardEndpoint.EndpointId.Value,
                ((int)MotherboardEndpoint.ConnectorType).ToString());
        }

        public StableId<AssemblyPowerCableRouteIdScope> RouteId { get; }

        public PowerCableEndpointDefinition PsuEndpoint { get; }

        public PowerCableEndpointDefinition MotherboardEndpoint { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> FirstWaypointId { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> SecondWaypointId { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> ThirdWaypointId { get; }

        public IReadOnlyList<StableId<AssemblyPowerCableWaypointIdScope>> OrderedWaypoints =>
            _orderedWaypoints;

        public string Fingerprint { get; }

        public bool IsValid =>
            !RouteId.IsEmpty &&
            PsuEndpoint.IsValid &&
            PsuEndpoint.ConnectorType == PowerCableConnectorType.PsuModularEps12v8 &&
            MotherboardEndpoint.IsValid &&
            MotherboardEndpoint.ConnectorType ==
                PowerCableConnectorType.MotherboardEps12v8 &&
            PsuEndpoint.EndpointId != MotherboardEndpoint.EndpointId &&
            AreDistinctNonEmptyWaypoints(
                FirstWaypointId,
                SecondWaypointId,
                ThirdWaypointId) &&
            !string.IsNullOrEmpty(Fingerprint);

        public static OperationResult<Eps12vPowerCableTopology> Create(
            StableId<AssemblyPowerCableRouteIdScope> routeId,
            PowerCableEndpointDefinition psuEndpoint,
            PowerCableEndpointDefinition motherboardEndpoint,
            StableId<AssemblyPowerCableWaypointIdScope> firstWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> secondWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> thirdWaypointId)
        {
            if (routeId.IsEmpty)
            {
                return OperationResult<Eps12vPowerCableTopology>.Fail(
                    AssemblyFailures.InvalidPowerCableRoute);
            }

            var topology = new Eps12vPowerCableTopology(
                routeId,
                psuEndpoint,
                motherboardEndpoint,
                firstWaypointId,
                secondWaypointId,
                thirdWaypointId);
            if (topology.IsValid)
            {
                return OperationResult<Eps12vPowerCableTopology>.Success(topology);
            }

            bool endpointInvalid =
                !psuEndpoint.IsValid ||
                psuEndpoint.ConnectorType !=
                    PowerCableConnectorType.PsuModularEps12v8 ||
                !motherboardEndpoint.IsValid ||
                motherboardEndpoint.ConnectorType !=
                    PowerCableConnectorType.MotherboardEps12v8 ||
                psuEndpoint.EndpointId == motherboardEndpoint.EndpointId;
            return OperationResult<Eps12vPowerCableTopology>.Fail(
                endpointInvalid
                    ? AssemblyFailures.InvalidPowerCableEndpointTopology
                    : AssemblyFailures.InvalidPowerCableWaypointTopology);
        }

        internal bool HasExactIdentity(Eps12vPowerCableTopology other)
        {
            return other != null &&
                   string.Equals(Fingerprint, other.Fingerprint, StringComparison.Ordinal);
        }

        private static bool AreDistinctNonEmptyWaypoints(
            StableId<AssemblyPowerCableWaypointIdScope> first,
            StableId<AssemblyPowerCableWaypointIdScope> second,
            StableId<AssemblyPowerCableWaypointIdScope> third)
        {
            return !first.IsEmpty &&
                   !second.IsEmpty &&
                   !third.IsEmpty &&
                   first != second &&
                   first != third &&
                   second != third;
        }
    }

    public readonly struct Eps12vPowerCableDefinition
    {
        private Eps12vPowerCableDefinition(
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> routeContainerId,
            Eps12vPowerCableTopology topology)
        {
            ProductId = productId;
            RouteContainerId = routeContainerId;
            Topology = topology;
        }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> RouteContainerId { get; }

        public Eps12vPowerCableTopology Topology { get; }

        public bool IsValid =>
            !ProductId.IsEmpty &&
            !RouteContainerId.IsEmpty &&
            Topology != null &&
            Topology.IsValid;

        internal bool HasAnyValue =>
            !ProductId.IsEmpty ||
            !RouteContainerId.IsEmpty ||
            Topology != null;

        public static OperationResult<Eps12vPowerCableDefinition> Create(
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> routeContainerId,
            Eps12vPowerCableTopology topology)
        {
            if (productId.IsEmpty)
            {
                return OperationResult<Eps12vPowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableProduct);
            }

            if (routeContainerId.IsEmpty)
            {
                return OperationResult<Eps12vPowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableRouteContainer);
            }

            if (topology == null || !topology.IsValid)
            {
                return OperationResult<Eps12vPowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableTopology);
            }

            return OperationResult<Eps12vPowerCableDefinition>.Success(
                new Eps12vPowerCableDefinition(productId, routeContainerId, topology));
        }

        internal bool HasExactIdentity(Eps12vPowerCableDefinition other)
        {
            return ProductId == other.ProductId &&
                   RouteContainerId == other.RouteContainerId &&
                   Topology != null &&
                   Topology.HasExactIdentity(other.Topology);
        }
    }

    public sealed class Eps12vPowerCableOperationReceipt
    {
        internal Eps12vPowerCableOperationReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            Eps12vPowerCableOperationKind operationKind,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            Eps12vPowerCableDefinition definition,
            PowerCableKeyOrientation orientation,
            Eps12vPowerCableState previousState,
            Eps12vPowerCableState resultingState,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision,
            long cableRevision,
            long inventoryRevision)
        {
            OperationId = operationId;
            OperationKind = operationKind;
            BuildId = buildId;
            ChassisId = chassisId;
            ItemId = itemId;
            ProductId = productId;
            SourceContainerId = sourceContainerId;
            TargetContainerId = targetContainerId;
            Definition = definition;
            Orientation = orientation;
            PreviousState = previousState;
            ResultingState = resultingState;
            SourceMotherboardSecureOperationId = sourceMotherboardSecureOperationId;
            SourcePowerSupplyRetentionOperationId =
                sourcePowerSupplyRetentionOperationId;
            SourceProcessorRetentionOperationId = sourceProcessorRetentionOperationId;
            SourceRouteOperationId = sourceRouteOperationId;
            ExpectedCableRevision = expectedCableRevision;
            CableRevision = cableRevision;
            InventoryRevision = inventoryRevision;
        }

        public StableId<AssemblyOperationIdScope> OperationId { get; }

        public Eps12vPowerCableOperationKind OperationKind { get; }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> SourceContainerId { get; }

        public StableId<ContainerIdScope> TargetContainerId { get; }

        public Eps12vPowerCableDefinition Definition { get; }

        public string RouteFingerprint => Definition.Topology?.Fingerprint ?? string.Empty;

        public PowerCableKeyOrientation Orientation { get; }

        public Eps12vPowerCableState PreviousState { get; }

        public Eps12vPowerCableState ResultingState { get; }

        public StableId<AssemblyOperationIdScope> SourceMotherboardSecureOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> SourcePowerSupplyRetentionOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> SourceProcessorRetentionOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> SourceRouteOperationId { get; }

        public long ExpectedCableRevision { get; }

        public long CableRevision { get; }

        public long InventoryRevision { get; }

        internal bool MatchesRoute(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            Eps12vPowerCableDefinition definition,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            long expectedCableRevision)
        {
            return OperationKind == Eps12vPowerCableOperationKind.Route &&
                   OperationId == operationId &&
                   BuildId == buildId &&
                   ChassisId == chassisId &&
                   ItemId == itemId &&
                   ProductId == productId &&
                   SourceContainerId == sourceContainerId &&
                   TargetContainerId == targetContainerId &&
                   Definition.HasExactIdentity(definition) &&
                   Orientation == orientation &&
                   PreviousState == Eps12vPowerCableState.Loose &&
                   ResultingState == Eps12vPowerCableState.Routed &&
                   SourceMotherboardSecureOperationId ==
                       sourceMotherboardSecureOperationId &&
                   SourcePowerSupplyRetentionOperationId ==
                       sourcePowerSupplyRetentionOperationId &&
                   SourceProcessorRetentionOperationId ==
                       sourceProcessorRetentionOperationId &&
                   SourceRouteOperationId.IsEmpty &&
                   ExpectedCableRevision == expectedCableRevision;
        }

        internal bool MatchesUnroute(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            Eps12vPowerCableDefinition definition,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceProcessorRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            return OperationKind == Eps12vPowerCableOperationKind.Unroute &&
                   OperationId == operationId &&
                   BuildId == buildId &&
                   ChassisId == chassisId &&
                   ItemId == itemId &&
                   ProductId == productId &&
                   SourceContainerId == sourceContainerId &&
                   TargetContainerId == targetContainerId &&
                   Definition.HasExactIdentity(definition) &&
                   Orientation == PowerCableKeyOrientation.Keyed &&
                   PreviousState == Eps12vPowerCableState.Routed &&
                   ResultingState == Eps12vPowerCableState.Loose &&
                   SourceMotherboardSecureOperationId ==
                       sourceMotherboardSecureOperationId &&
                   SourcePowerSupplyRetentionOperationId ==
                       sourcePowerSupplyRetentionOperationId &&
                   SourceProcessorRetentionOperationId ==
                       sourceProcessorRetentionOperationId &&
                   SourceRouteOperationId == sourceRouteOperationId &&
                   ExpectedCableRevision == expectedCableRevision;
        }
    }
}
