using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PcieGpuPowerCableProductIdValue =
            "catalog.cable.northstar-pcie-gpu-8pin";
        public const string PcieGpuPowerCableCategoryIdValue =
            "catalog.category.power-cables";
        public const string PcieGpuPowerCableItemInstanceIdValue =
            "inventory.item.northstar-pcie-gpu-8pin-001";
        public const string PcieGpuPowerCableRouteContainerIdValue =
            "inventory.container.assembly-pcie-gpu-route";
        public const string PcieGpuPowerCableRouteIdValue =
            "assembly.route.power-cable-pcie-gpu-main";
        public const string PcieGpuPowerCablePsuEndpointIdValue =
            "assembly.endpoint.psu-modular-pcie-gpu-8";
        public const string PcieGpuPowerCableGraphicsCardEndpointIdValue =
            "assembly.endpoint.graphics-card-pcie-gpu-8";
        public const string PcieGpuPowerCableWaypoint1IdValue =
            "assembly.waypoint.pcie-gpu-psu-exit";
        public const string PcieGpuPowerCableWaypoint2IdValue =
            "assembly.waypoint.pcie-gpu-rear-channel";
        public const string PcieGpuPowerCableWaypoint3IdValue =
            "assembly.waypoint.pcie-gpu-card-entry";
        public const string PcieGpuPowerCableDisplayName =
            "Northstar PCIe GPU 8-pin 6+2 Güç Kablosu";
        public const long PcieGpuPowerCableUnitCostMinorUnits = 1_500;

        public StableId<ProductDefinitionIdScope> PcieGpuPowerCableProductId =>
            StableId<ProductDefinitionIdScope>.Parse(PcieGpuPowerCableProductIdValue);

        public StableId<ItemInstanceIdScope> PcieGpuPowerCableItemId =>
            StableId<ItemInstanceIdScope>.Parse(
                PcieGpuPowerCableItemInstanceIdValue);

        public StableId<ContainerIdScope> PcieGpuPowerCableRouteContainerId =>
            StableId<ContainerIdScope>.Parse(PcieGpuPowerCableRouteContainerIdValue);

        public StableId<AssemblyPowerCableRouteIdScope> PcieGpuPowerCableRouteId =>
            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                PcieGpuPowerCableRouteIdValue);

        public StableId<AssemblyPowerCableEndpointIdScope>
            PcieGpuPowerCablePsuEndpointId =>
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    PcieGpuPowerCablePsuEndpointIdValue);

        public StableId<AssemblyPowerCableEndpointIdScope>
            PcieGpuPowerCableGraphicsCardEndpointId =>
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    PcieGpuPowerCableGraphicsCardEndpointIdValue);

        public StableId<AssemblyPowerCableWaypointIdScope>
            PcieGpuPowerCableWaypoint1Id =>
                StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                    PcieGpuPowerCableWaypoint1IdValue);

        public StableId<AssemblyPowerCableWaypointIdScope>
            PcieGpuPowerCableWaypoint2Id =>
                StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                    PcieGpuPowerCableWaypoint2IdValue);

        public StableId<AssemblyPowerCableWaypointIdScope>
            PcieGpuPowerCableWaypoint3Id =>
                StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                    PcieGpuPowerCableWaypoint3IdValue);

        public bool TryGetPcieGpuPowerCableItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(PcieGpuPowerCableItemId, out item);
        }

        public OperationResult PickupLoosePcieGpuPowerCableToHands()
        {
            if (!AssemblyBuild.HasPcieGpuPowerCableRoute ||
                AssemblyBuild.IsPcieGpuPowerCableRouted ||
                !TryGetPcieGpuPowerCableItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(
                PcieGpuPowerCableItemId,
                HandsContainerId);
        }

        public OperationResult DropHeldPcieGpuPowerCableToWorld()
        {
            if (!AssemblyBuild.HasPcieGpuPowerCableRoute ||
                AssemblyBuild.IsPcieGpuPowerCableRouted ||
                !TryGetPcieGpuPowerCableItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-pcie-gpu-cable.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                PcieGpuPowerCableItemId,
                WorldFloorContainerId);
        }

        public OperationResult<PcieGpuPowerCableOperationReceipt>
            RoutePcieGpuPowerCable(
                StableId<AssemblyOperationIdScope> operationId,
                PowerCableKeyOrientation orientation,
                long expectedCableRevision)
        {
            return AssemblyBuild.RoutePcieGpuPowerCable(
                operationId,
                PcieGpuPowerCableItemId,
                orientation,
                AssemblyBuild.SecuredByOperationId,
                AssemblyBuild.PowerSupplyRetainedByOperationId,
                AssemblyBuild.GraphicsCardRetainedByOperationId,
                expectedCableRevision);
        }

        public OperationResult<PcieGpuPowerCableOperationReceipt>
            UnroutePcieGpuPowerCable(
                StableId<AssemblyOperationIdScope> operationId,
                StableId<AssemblyOperationIdScope> sourceRouteOperationId,
                long expectedCableRevision)
        {
            return AssemblyBuild.UnroutePcieGpuPowerCable(
                operationId,
                PcieGpuPowerCableItemId,
                sourceRouteOperationId,
                expectedCableRevision);
        }
    }
}
