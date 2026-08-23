using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string Atx24PowerCableProductIdValue =
            "catalog.cable.northstar-atx24-modular";
        public const string Atx24PowerCableCategoryIdValue =
            "catalog.category.power-cables";
        public const string Atx24PowerCableItemInstanceIdValue =
            "inventory.item.northstar-atx24-modular-001";
        public const string Atx24PowerCableRouteContainerIdValue =
            "inventory.container.assembly-atx24-route";
        public const string Atx24PowerCableRouteIdValue =
            "assembly.route.power-cable-atx24-main";
        public const string Atx24PowerCablePsuPrimaryEndpointIdValue =
            "assembly.endpoint.psu-modular-atx24-primary-18";
        public const string Atx24PowerCablePsuSenseEndpointIdValue =
            "assembly.endpoint.psu-modular-atx24-sense-10";
        public const string Atx24PowerCableMotherboardEndpointIdValue =
            "assembly.endpoint.motherboard-atx24";
        public const string Atx24PowerCableWaypoint1IdValue =
            "assembly.waypoint.atx24-psu-exit";
        public const string Atx24PowerCableWaypoint2IdValue =
            "assembly.waypoint.atx24-rear-channel";
        public const string Atx24PowerCableWaypoint3IdValue =
            "assembly.waypoint.atx24-board-entry";
        public const string Atx24PowerCableDisplayName =
            "Northstar 24-pin ATX Modüler Güç Kablosu";
        public const long Atx24PowerCableUnitCostMinorUnits = 1_900;

        public StableId<ProductDefinitionIdScope> Atx24PowerCableProductId =>
            StableId<ProductDefinitionIdScope>.Parse(Atx24PowerCableProductIdValue);

        public StableId<ItemInstanceIdScope> Atx24PowerCableItemId =>
            StableId<ItemInstanceIdScope>.Parse(Atx24PowerCableItemInstanceIdValue);

        public StableId<ContainerIdScope> Atx24PowerCableRouteContainerId =>
            StableId<ContainerIdScope>.Parse(Atx24PowerCableRouteContainerIdValue);

        public StableId<AssemblyPowerCableRouteIdScope> Atx24PowerCableRouteId =>
            StableId<AssemblyPowerCableRouteIdScope>.Parse(
                Atx24PowerCableRouteIdValue);

        public StableId<AssemblyPowerCableEndpointIdScope>
            Atx24PowerCablePsuPrimaryEndpointId =>
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    Atx24PowerCablePsuPrimaryEndpointIdValue);

        public StableId<AssemblyPowerCableEndpointIdScope>
            Atx24PowerCablePsuSenseEndpointId =>
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    Atx24PowerCablePsuSenseEndpointIdValue);

        public StableId<AssemblyPowerCableEndpointIdScope>
            Atx24PowerCableMotherboardEndpointId =>
                StableId<AssemblyPowerCableEndpointIdScope>.Parse(
                    Atx24PowerCableMotherboardEndpointIdValue);

        public bool TryGetAtx24PowerCableItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(Atx24PowerCableItemId, out item);
        }

        public OperationResult PickupLooseAtx24PowerCableToHands()
        {
            if (!AssemblyBuild.HasAtx24PowerCableRoute ||
                AssemblyBuild.IsAtx24PowerCableRouted ||
                !TryGetAtx24PowerCableItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(
                Atx24PowerCableItemId,
                HandsContainerId);
        }

        public OperationResult DropHeldAtx24PowerCableToWorld()
        {
            if (!AssemblyBuild.HasAtx24PowerCableRoute ||
                AssemblyBuild.IsAtx24PowerCableRouted ||
                !TryGetAtx24PowerCableItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-cable.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                Atx24PowerCableItemId,
                WorldFloorContainerId);
        }

        public OperationResult<Atx24PowerCableOperationReceipt> RouteAtx24PowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            PowerCableKeyOrientation orientation,
            long expectedCableRevision)
        {
            return AssemblyBuild.RouteAtx24PowerCable(
                operationId,
                Atx24PowerCableItemId,
                orientation,
                AssemblyBuild.SecuredByOperationId,
                AssemblyBuild.PowerSupplyRetainedByOperationId,
                expectedCableRevision);
        }

        public OperationResult<Atx24PowerCableOperationReceipt> UnrouteAtx24PowerCable(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourceRouteOperationId,
            long expectedCableRevision)
        {
            return AssemblyBuild.UnrouteAtx24PowerCable(
                operationId,
                Atx24PowerCableItemId,
                sourceRouteOperationId,
                expectedCableRevision);
        }
    }
}
