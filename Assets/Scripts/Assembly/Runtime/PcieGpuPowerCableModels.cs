using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public enum PcieGpuPowerCableState
    {
        Unsupported = 0,
        Loose = 1,
        Routed = 2
    }

    public enum PcieGpuPowerCableOperationKind
    {
        Route = 1,
        Unroute = 2
    }

    /// <summary>
    /// Immutable two-endpoint PCIe/GPU contract. The authored waypoint order is identity;
    /// no rope particles, joints or transient physics state participate in persistence.
    /// </summary>
    public sealed class PcieGpuPowerCableTopology
    {
        private readonly IReadOnlyList<StableId<AssemblyPowerCableWaypointIdScope>>
            _orderedWaypoints;

        private PcieGpuPowerCableTopology(
            StableId<AssemblyPowerCableRouteIdScope> routeId,
            PowerCableEndpointDefinition psuEndpoint,
            PowerCableEndpointDefinition graphicsCardEndpoint,
            StableId<AssemblyPowerCableWaypointIdScope> firstWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> secondWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> thirdWaypointId)
        {
            RouteId = routeId;
            PsuEndpoint = psuEndpoint;
            GraphicsCardEndpoint = graphicsCardEndpoint;
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
                GraphicsCardEndpoint.EndpointId.Value,
                ((int)GraphicsCardEndpoint.ConnectorType).ToString());
        }

        public StableId<AssemblyPowerCableRouteIdScope> RouteId { get; }

        public PowerCableEndpointDefinition PsuEndpoint { get; }

        public PowerCableEndpointDefinition GraphicsCardEndpoint { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> FirstWaypointId { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> SecondWaypointId { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> ThirdWaypointId { get; }

        public IReadOnlyList<StableId<AssemblyPowerCableWaypointIdScope>> OrderedWaypoints =>
            _orderedWaypoints;

        public string Fingerprint { get; }

        public bool IsValid =>
            !RouteId.IsEmpty &&
            PsuEndpoint.IsValid &&
            PsuEndpoint.ConnectorType == PowerCableConnectorType.PsuModularPcie8 &&
            GraphicsCardEndpoint.IsValid &&
            GraphicsCardEndpoint.ConnectorType ==
                PowerCableConnectorType.GraphicsCardPcie8 &&
            PsuEndpoint.EndpointId != GraphicsCardEndpoint.EndpointId &&
            AreDistinctNonEmptyWaypoints(
                FirstWaypointId,
                SecondWaypointId,
                ThirdWaypointId) &&
            !string.IsNullOrEmpty(Fingerprint);

        public static OperationResult<PcieGpuPowerCableTopology> Create(
            StableId<AssemblyPowerCableRouteIdScope> routeId,
            PowerCableEndpointDefinition psuEndpoint,
            PowerCableEndpointDefinition graphicsCardEndpoint,
            StableId<AssemblyPowerCableWaypointIdScope> firstWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> secondWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> thirdWaypointId)
        {
            if (routeId.IsEmpty)
            {
                return OperationResult<PcieGpuPowerCableTopology>.Fail(
                    AssemblyFailures.InvalidPowerCableRoute);
            }

            var topology = new PcieGpuPowerCableTopology(
                routeId,
                psuEndpoint,
                graphicsCardEndpoint,
                firstWaypointId,
                secondWaypointId,
                thirdWaypointId);
            if (topology.IsValid)
            {
                return OperationResult<PcieGpuPowerCableTopology>.Success(topology);
            }

            bool endpointInvalid =
                !psuEndpoint.IsValid ||
                psuEndpoint.ConnectorType !=
                    PowerCableConnectorType.PsuModularPcie8 ||
                !graphicsCardEndpoint.IsValid ||
                graphicsCardEndpoint.ConnectorType !=
                    PowerCableConnectorType.GraphicsCardPcie8 ||
                psuEndpoint.EndpointId == graphicsCardEndpoint.EndpointId;
            return OperationResult<PcieGpuPowerCableTopology>.Fail(
                endpointInvalid
                    ? AssemblyFailures.InvalidPowerCableEndpointTopology
                    : AssemblyFailures.InvalidPowerCableWaypointTopology);
        }

        internal bool HasExactIdentity(PcieGpuPowerCableTopology other)
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

    public readonly struct PcieGpuPowerCableDefinition
    {
        private PcieGpuPowerCableDefinition(
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> routeContainerId,
            PcieGpuPowerCableTopology topology)
        {
            ProductId = productId;
            RouteContainerId = routeContainerId;
            Topology = topology;
        }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> RouteContainerId { get; }

        public PcieGpuPowerCableTopology Topology { get; }

        public bool IsValid =>
            !ProductId.IsEmpty &&
            !RouteContainerId.IsEmpty &&
            Topology != null &&
            Topology.IsValid;

        internal bool HasAnyValue =>
            !ProductId.IsEmpty ||
            !RouteContainerId.IsEmpty ||
            Topology != null;

        public static OperationResult<PcieGpuPowerCableDefinition> Create(
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> routeContainerId,
            PcieGpuPowerCableTopology topology)
        {
            if (productId.IsEmpty)
            {
                return OperationResult<PcieGpuPowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableProduct);
            }

            if (routeContainerId.IsEmpty)
            {
                return OperationResult<PcieGpuPowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableRouteContainer);
            }

            if (topology == null || !topology.IsValid)
            {
                return OperationResult<PcieGpuPowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableTopology);
            }

            return OperationResult<PcieGpuPowerCableDefinition>.Success(
                new PcieGpuPowerCableDefinition(productId, routeContainerId, topology));
        }

        internal bool HasExactIdentity(PcieGpuPowerCableDefinition other)
        {
            return ProductId == other.ProductId &&
                   RouteContainerId == other.RouteContainerId &&
                   Topology != null &&
                   Topology.HasExactIdentity(other.Topology);
        }
    }

    public sealed class PcieGpuPowerCableOperationReceipt
    {
        internal PcieGpuPowerCableOperationReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            PcieGpuPowerCableOperationKind operationKind,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            PcieGpuPowerCableDefinition definition,
            PowerCableKeyOrientation orientation,
            PcieGpuPowerCableState previousState,
            PcieGpuPowerCableState resultingState,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardRetentionOperationId,
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
            SourceGraphicsCardRetentionOperationId = sourceGraphicsCardRetentionOperationId;
            SourceRouteOperationId = sourceRouteOperationId;
            ExpectedCableRevision = expectedCableRevision;
            CableRevision = cableRevision;
            InventoryRevision = inventoryRevision;
        }

        public StableId<AssemblyOperationIdScope> OperationId { get; }

        public PcieGpuPowerCableOperationKind OperationKind { get; }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> SourceContainerId { get; }

        public StableId<ContainerIdScope> TargetContainerId { get; }

        public PcieGpuPowerCableDefinition Definition { get; }

        public string RouteFingerprint => Definition.Topology?.Fingerprint ?? string.Empty;

        public PowerCableKeyOrientation Orientation { get; }

        public PcieGpuPowerCableState PreviousState { get; }

        public PcieGpuPowerCableState ResultingState { get; }

        public StableId<AssemblyOperationIdScope> SourceMotherboardSecureOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> SourcePowerSupplyRetentionOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> SourceGraphicsCardRetentionOperationId
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
            PcieGpuPowerCableDefinition definition,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardRetentionOperationId,
            long expectedCableRevision)
        {
            return OperationKind == PcieGpuPowerCableOperationKind.Route &&
                   OperationId == operationId &&
                   BuildId == buildId &&
                   ChassisId == chassisId &&
                   ItemId == itemId &&
                   ProductId == productId &&
                   SourceContainerId == sourceContainerId &&
                   TargetContainerId == targetContainerId &&
                   Definition.HasExactIdentity(definition) &&
                   Orientation == orientation &&
                   PreviousState == PcieGpuPowerCableState.Loose &&
                   ResultingState == PcieGpuPowerCableState.Routed &&
                   SourceMotherboardSecureOperationId ==
                       sourceMotherboardSecureOperationId &&
                   SourcePowerSupplyRetentionOperationId ==
                       sourcePowerSupplyRetentionOperationId &&
                   SourceGraphicsCardRetentionOperationId ==
                       sourceGraphicsCardRetentionOperationId &&
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
            PcieGpuPowerCableDefinition definition,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceGraphicsCardRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            return OperationKind == PcieGpuPowerCableOperationKind.Unroute &&
                   OperationId == operationId &&
                   BuildId == buildId &&
                   ChassisId == chassisId &&
                   ItemId == itemId &&
                   ProductId == productId &&
                   SourceContainerId == sourceContainerId &&
                   TargetContainerId == targetContainerId &&
                   Definition.HasExactIdentity(definition) &&
                   Orientation == PowerCableKeyOrientation.Keyed &&
                   PreviousState == PcieGpuPowerCableState.Routed &&
                   ResultingState == PcieGpuPowerCableState.Loose &&
                   SourceMotherboardSecureOperationId ==
                       sourceMotherboardSecureOperationId &&
                   SourcePowerSupplyRetentionOperationId ==
                       sourcePowerSupplyRetentionOperationId &&
                   SourceGraphicsCardRetentionOperationId ==
                       sourceGraphicsCardRetentionOperationId &&
                   SourceRouteOperationId == sourceRouteOperationId &&
                   ExpectedCableRevision == expectedCableRevision;
        }
    }
}
