using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed class AssemblyPowerCableRouteIdScope : IStableIdScope
    {
    }

    public sealed class AssemblyPowerCableEndpointIdScope : IStableIdScope
    {
    }

    public sealed class AssemblyPowerCableWaypointIdScope : IStableIdScope
    {
    }

    public enum Atx24PowerCableState
    {
        Unsupported = 0,
        Loose = 1,
        Routed = 2
    }

    public enum PowerCableConnectorType
    {
        PsuModularAtx24Primary18 = 1,
        PsuModularAtx24Sense10 = 2,
        MotherboardAtx24 = 3
    }

    public enum PowerCableKeyOrientation
    {
        Keyed = 1,
        Reversed = 2
    }

    public enum Atx24PowerCableOperationKind
    {
        Route = 1,
        Unroute = 2
    }

    public readonly struct PowerCableEndpointDefinition
    {
        private PowerCableEndpointDefinition(
            StableId<AssemblyPowerCableEndpointIdScope> endpointId,
            PowerCableConnectorType connectorType)
        {
            EndpointId = endpointId;
            ConnectorType = connectorType;
        }

        public StableId<AssemblyPowerCableEndpointIdScope> EndpointId { get; }

        public PowerCableConnectorType ConnectorType { get; }

        public int Capacity => 1;

        public int PinCount
        {
            get
            {
                switch (ConnectorType)
                {
                    case PowerCableConnectorType.PsuModularAtx24Primary18:
                        return 18;
                    case PowerCableConnectorType.PsuModularAtx24Sense10:
                        return 10;
                    case PowerCableConnectorType.MotherboardAtx24:
                        return 24;
                    default:
                        return 0;
                }
            }
        }

        public bool IsValid =>
            !EndpointId.IsEmpty &&
            (ConnectorType == PowerCableConnectorType.PsuModularAtx24Primary18 ||
             ConnectorType == PowerCableConnectorType.PsuModularAtx24Sense10 ||
             ConnectorType == PowerCableConnectorType.MotherboardAtx24);

        public static OperationResult<PowerCableEndpointDefinition> Create(
            StableId<AssemblyPowerCableEndpointIdScope> endpointId,
            PowerCableConnectorType connectorType)
        {
            if (endpointId.IsEmpty)
            {
                return OperationResult<PowerCableEndpointDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableEndpoint);
            }

            if (connectorType != PowerCableConnectorType.PsuModularAtx24Primary18 &&
                connectorType != PowerCableConnectorType.PsuModularAtx24Sense10 &&
                connectorType != PowerCableConnectorType.MotherboardAtx24)
            {
                return OperationResult<PowerCableEndpointDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableConnectorType);
            }

            return OperationResult<PowerCableEndpointDefinition>.Success(
                new PowerCableEndpointDefinition(endpointId, connectorType));
        }
    }

    /// <summary>
    /// Immutable physical contract for one keyed modular ATX 24-pin cable. The three
    /// waypoints are identifiers, not free rope particles, and therefore persist a stable
    /// authored route without joints, spring state or physics-driven drift.
    /// </summary>
    public sealed class Atx24PowerCableTopology
    {
        private readonly IReadOnlyList<StableId<AssemblyPowerCableWaypointIdScope>>
            _orderedWaypoints;

        private Atx24PowerCableTopology(
            StableId<AssemblyPowerCableRouteIdScope> routeId,
            PowerCableEndpointDefinition psuPrimaryEndpoint,
            PowerCableEndpointDefinition psuSenseEndpoint,
            PowerCableEndpointDefinition motherboardEndpoint,
            StableId<AssemblyPowerCableWaypointIdScope> firstWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> secondWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> thirdWaypointId)
        {
            RouteId = routeId;
            PsuPrimaryEndpoint = psuPrimaryEndpoint;
            PsuSenseEndpoint = psuSenseEndpoint;
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
                PsuPrimaryEndpoint.EndpointId.Value,
                ((int)PsuPrimaryEndpoint.ConnectorType).ToString(),
                PsuSenseEndpoint.EndpointId.Value,
                ((int)PsuSenseEndpoint.ConnectorType).ToString(),
                FirstWaypointId.Value,
                SecondWaypointId.Value,
                ThirdWaypointId.Value,
                MotherboardEndpoint.EndpointId.Value,
                ((int)MotherboardEndpoint.ConnectorType).ToString());
        }

        public StableId<AssemblyPowerCableRouteIdScope> RouteId { get; }

        public PowerCableEndpointDefinition PsuPrimaryEndpoint { get; }

        public PowerCableEndpointDefinition PsuSenseEndpoint { get; }

        public PowerCableEndpointDefinition MotherboardEndpoint { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> FirstWaypointId { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> SecondWaypointId { get; }

        public StableId<AssemblyPowerCableWaypointIdScope> ThirdWaypointId { get; }

        public IReadOnlyList<StableId<AssemblyPowerCableWaypointIdScope>> OrderedWaypoints =>
            _orderedWaypoints;

        public string Fingerprint { get; }

        public bool IsValid =>
            !RouteId.IsEmpty &&
            PsuPrimaryEndpoint.IsValid &&
            PsuPrimaryEndpoint.ConnectorType ==
                PowerCableConnectorType.PsuModularAtx24Primary18 &&
            PsuSenseEndpoint.IsValid &&
            PsuSenseEndpoint.ConnectorType ==
                PowerCableConnectorType.PsuModularAtx24Sense10 &&
            MotherboardEndpoint.IsValid &&
            MotherboardEndpoint.ConnectorType ==
                PowerCableConnectorType.MotherboardAtx24 &&
            AreDistinctEndpointIds(
                PsuPrimaryEndpoint.EndpointId,
                PsuSenseEndpoint.EndpointId,
                MotherboardEndpoint.EndpointId) &&
            AreDistinctNonEmptyWaypoints(
                FirstWaypointId,
                SecondWaypointId,
                ThirdWaypointId) &&
            !string.IsNullOrEmpty(Fingerprint);

        public static OperationResult<Atx24PowerCableTopology> Create(
            StableId<AssemblyPowerCableRouteIdScope> routeId,
            PowerCableEndpointDefinition psuPrimaryEndpoint,
            PowerCableEndpointDefinition psuSenseEndpoint,
            PowerCableEndpointDefinition motherboardEndpoint,
            StableId<AssemblyPowerCableWaypointIdScope> firstWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> secondWaypointId,
            StableId<AssemblyPowerCableWaypointIdScope> thirdWaypointId)
        {
            if (routeId.IsEmpty)
            {
                return OperationResult<Atx24PowerCableTopology>.Fail(
                    AssemblyFailures.InvalidPowerCableRoute);
            }

            var topology = new Atx24PowerCableTopology(
                routeId,
                psuPrimaryEndpoint,
                psuSenseEndpoint,
                motherboardEndpoint,
                firstWaypointId,
                secondWaypointId,
                thirdWaypointId);
            return topology.IsValid
                ? OperationResult<Atx24PowerCableTopology>.Success(topology)
                : OperationResult<Atx24PowerCableTopology>.Fail(
                    !psuPrimaryEndpoint.IsValid ||
                    !psuSenseEndpoint.IsValid ||
                    !motherboardEndpoint.IsValid ||
                    !AreDistinctEndpointIds(
                        psuPrimaryEndpoint.EndpointId,
                        psuSenseEndpoint.EndpointId,
                        motherboardEndpoint.EndpointId)
                        ? AssemblyFailures.InvalidPowerCableEndpointTopology
                        : AssemblyFailures.InvalidPowerCableWaypointTopology);
        }

        internal bool HasExactIdentity(Atx24PowerCableTopology other)
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

        private static bool AreDistinctEndpointIds(
            StableId<AssemblyPowerCableEndpointIdScope> first,
            StableId<AssemblyPowerCableEndpointIdScope> second,
            StableId<AssemblyPowerCableEndpointIdScope> third)
        {
            return !first.IsEmpty &&
                   !second.IsEmpty &&
                   !third.IsEmpty &&
                   first != second &&
                   first != third &&
                   second != third;
        }
    }

    public readonly struct Atx24PowerCableDefinition
    {
        private Atx24PowerCableDefinition(
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> routeContainerId,
            Atx24PowerCableTopology topology)
        {
            ProductId = productId;
            RouteContainerId = routeContainerId;
            Topology = topology;
        }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> RouteContainerId { get; }

        public Atx24PowerCableTopology Topology { get; }

        public bool IsValid =>
            !ProductId.IsEmpty &&
            !RouteContainerId.IsEmpty &&
            Topology != null &&
            Topology.IsValid;

        internal bool HasAnyValue =>
            !ProductId.IsEmpty ||
            !RouteContainerId.IsEmpty ||
            Topology != null;

        public static OperationResult<Atx24PowerCableDefinition> Create(
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> routeContainerId,
            Atx24PowerCableTopology topology)
        {
            if (productId.IsEmpty)
            {
                return OperationResult<Atx24PowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableProduct);
            }

            if (routeContainerId.IsEmpty)
            {
                return OperationResult<Atx24PowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableRouteContainer);
            }

            if (topology == null || !topology.IsValid)
            {
                return OperationResult<Atx24PowerCableDefinition>.Fail(
                    AssemblyFailures.InvalidPowerCableTopology);
            }

            return OperationResult<Atx24PowerCableDefinition>.Success(
                new Atx24PowerCableDefinition(productId, routeContainerId, topology));
        }

        internal bool HasExactIdentity(Atx24PowerCableDefinition other)
        {
            return ProductId == other.ProductId &&
                   RouteContainerId == other.RouteContainerId &&
                   Topology != null &&
                   Topology.HasExactIdentity(other.Topology);
        }
    }

    public sealed class Atx24PowerCableOperationReceipt
    {
        internal Atx24PowerCableOperationReceipt(
            StableId<AssemblyOperationIdScope> operationId,
            Atx24PowerCableOperationKind operationKind,
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            Atx24PowerCableDefinition definition,
            PowerCableKeyOrientation orientation,
            Atx24PowerCableState previousState,
            Atx24PowerCableState resultingState,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
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
            SourcePowerSupplyRetentionOperationId = sourcePowerSupplyRetentionOperationId;
            SourceRouteOperationId = sourceRouteOperationId;
            ExpectedCableRevision = expectedCableRevision;
            CableRevision = cableRevision;
            InventoryRevision = inventoryRevision;
        }

        public StableId<AssemblyOperationIdScope> OperationId { get; }

        public Atx24PowerCableOperationKind OperationKind { get; }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> SourceContainerId { get; }

        public StableId<ContainerIdScope> TargetContainerId { get; }

        public Atx24PowerCableDefinition Definition { get; }

        public string RouteFingerprint => Definition.Topology?.Fingerprint ?? string.Empty;

        public PowerCableKeyOrientation Orientation { get; }

        public Atx24PowerCableState PreviousState { get; }

        public Atx24PowerCableState ResultingState { get; }

        public StableId<AssemblyOperationIdScope> SourceMotherboardSecureOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> SourcePowerSupplyRetentionOperationId
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
            Atx24PowerCableDefinition definition,
            PowerCableKeyOrientation orientation,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            long expectedCableRevision)
        {
            return OperationKind == Atx24PowerCableOperationKind.Route &&
                   OperationId == operationId &&
                   BuildId == buildId &&
                   ChassisId == chassisId &&
                   ItemId == itemId &&
                   ProductId == productId &&
                   SourceContainerId == sourceContainerId &&
                   TargetContainerId == targetContainerId &&
                   Definition.HasExactIdentity(definition) &&
                   Orientation == orientation &&
                   PreviousState == Atx24PowerCableState.Loose &&
                   ResultingState == Atx24PowerCableState.Routed &&
                   SourceMotherboardSecureOperationId ==
                       sourceMotherboardSecureOperationId &&
                   SourcePowerSupplyRetentionOperationId ==
                       sourcePowerSupplyRetentionOperationId &&
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
            Atx24PowerCableDefinition definition,
            StableId<AssemblyOperationIdScope> sourceMotherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            return OperationKind == Atx24PowerCableOperationKind.Unroute &&
                   OperationId == operationId &&
                   BuildId == buildId &&
                   ChassisId == chassisId &&
                   ItemId == itemId &&
                   ProductId == productId &&
                   SourceContainerId == sourceContainerId &&
                   TargetContainerId == targetContainerId &&
                   Definition.HasExactIdentity(definition) &&
                   Orientation == PowerCableKeyOrientation.Keyed &&
                   PreviousState == Atx24PowerCableState.Routed &&
                   ResultingState == Atx24PowerCableState.Loose &&
                   SourceMotherboardSecureOperationId ==
                       sourceMotherboardSecureOperationId &&
                   SourcePowerSupplyRetentionOperationId ==
                       sourcePowerSupplyRetentionOperationId &&
                   SourceRouteOperationId == sourceRouteOperationId &&
                   ExpectedCableRevision == expectedCableRevision;
        }
    }
}
