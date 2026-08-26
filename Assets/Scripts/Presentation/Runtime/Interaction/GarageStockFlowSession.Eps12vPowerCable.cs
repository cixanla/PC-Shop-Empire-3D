using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string Eps12vPowerCableProductIdValue =
            "catalog.cable.northstar-eps12v-cpu-8pin";
        public const string Eps12vPowerCableCategoryIdValue =
            "catalog.category.power-cables";
        public const string Eps12vPowerCableItemInstanceIdValue =
            "inventory.item.northstar-eps12v-cpu-8pin-001";
        public const string Eps12vPowerCableRouteContainerIdValue =
            "inventory.container.assembly-eps12v-cpu-route";
        public const string Eps12vPowerCableRouteIdValue =
            "assembly.route.power-cable-eps12v-cpu-main";
        public const string Eps12vPowerCablePsuEndpointIdValue =
            "assembly.endpoint.psu-modular-eps12v-cpu-8";
        public const string Eps12vPowerCableMotherboardEndpointIdValue =
            "assembly.endpoint.motherboard-eps12v-cpu-8";
        public const string Eps12vPowerCableWaypoint1IdValue =
            "assembly.waypoint.eps12v-cpu-psu-exit";
        public const string Eps12vPowerCableWaypoint2IdValue =
            "assembly.waypoint.eps12v-cpu-rear-channel";
        public const string Eps12vPowerCableWaypoint3IdValue =
            "assembly.waypoint.eps12v-cpu-board-entry";
        public const string Eps12vPowerCableDisplayName =
            "Northstar EPS12V CPU 8-pin Güç Kablosu";
        public const long Eps12vPowerCableUnitCostMinorUnits = 1_500;

        public StableId<ProductDefinitionIdScope> Eps12vPowerCableProductId =>
            StableId<ProductDefinitionIdScope>.Parse(Eps12vPowerCableProductIdValue);

        public StableId<ItemInstanceIdScope> Eps12vPowerCableItemId =>
            StableId<ItemInstanceIdScope>.Parse(
                Eps12vPowerCableItemInstanceIdValue);

        public StableId<ContainerIdScope> Eps12vPowerCableRouteContainerId =>
            StableId<ContainerIdScope>.Parse(Eps12vPowerCableRouteContainerIdValue);

        public StableId<AssemblyPowerCableRouteIdScope> Eps12vPowerCableRouteId =>
            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                Eps12vPowerCableRouteIdValue);

        public StableId<AssemblyPowerCableEndpointIdScope>
            Eps12vPowerCablePsuEndpointId =>
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    Eps12vPowerCablePsuEndpointIdValue);

        public StableId<AssemblyPowerCableEndpointIdScope>
            Eps12vPowerCableMotherboardEndpointId =>
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    Eps12vPowerCableMotherboardEndpointIdValue);

        public StableId<AssemblyPowerCableWaypointIdScope>
            Eps12vPowerCableWaypoint1Id =>
                StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                    Eps12vPowerCableWaypoint1IdValue);

        public StableId<AssemblyPowerCableWaypointIdScope>
            Eps12vPowerCableWaypoint2Id =>
                StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                    Eps12vPowerCableWaypoint2IdValue);

        public StableId<AssemblyPowerCableWaypointIdScope>
            Eps12vPowerCableWaypoint3Id =>
                StableId<AssemblyPowerCableWaypointIdScope>.Parse(
                    Eps12vPowerCableWaypoint3IdValue);

        public bool TryGetEps12vPowerCableItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(Eps12vPowerCableItemId, out item);
        }

        public OperationResult PickupLooseEps12vPowerCableToHands()
        {
            if (!AssemblyBuild.HasEps12vPowerCableRoute ||
                AssemblyBuild.IsEps12vPowerCableRouted ||
                !TryGetEps12vPowerCableItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.loose-pickup-invalid"));
            }

            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalEps12vPowerCable(
                        PrototypeEps12vPowerCableBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            return Inventory.TransferSerializedItem(
                Eps12vPowerCableItemId,
                HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldEps12vPowerCableInCustomPcBuildKit()
        {
            return PlaceHeldEps12vPowerCableInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldEps12vPowerCableInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeEps12vPowerCableBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.Eps12vPowerCableInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalEps12vPowerCable(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult DropHeldEps12vPowerCableToWorld()
        {
            if (!AssemblyBuild.HasEps12vPowerCableRoute ||
                AssemblyBuild.IsEps12vPowerCableRouted ||
                !TryGetEps12vPowerCableItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-eps12v-cable.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                Eps12vPowerCableItemId,
                WorldFloorContainerId);
        }

        public OperationResult<Eps12vPowerCableOperationReceipt> RouteEps12vPowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            PowerCableKeyOrientation orientation,
            long expectedCableRevision)
        {
            return AssemblyBuild.RouteEps12vPowerCable(
                operationId,
                Eps12vPowerCableItemId,
                orientation,
                AssemblyBuild.SecuredByOperationId,
                AssemblyBuild.PowerSupplyRetainedByOperationId,
                AssemblyBuild.ProcessorRetainedByOperationId,
                expectedCableRevision);
        }

        public OperationResult<Eps12vPowerCableOperationReceipt> UnrouteEps12vPowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            return AssemblyBuild.UnrouteEps12vPowerCable(
                operationId,
                Eps12vPowerCableItemId,
                sourceRouteOperationId,
                expectedCableRevision);
        }
    }
}
