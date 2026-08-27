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

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedEps12vPowerCableForAssembly()
        {
            return PickupStagedEps12vPowerCableForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision,
                AssemblyBuild.Eps12vPowerCableRevision,
                AssemblyBuild.Atx24PowerCableRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedEps12vPowerCableForAssembly(
                long expectedBuildKitRevision,
                long expectedInventoryRevision,
                long expectedAssemblyRevision,
                long expectedCableRevision,
                long expectedAtx24CableRevision)
        {
            if (CustomPcBuildKit == null ||
                !TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            if (CustomPcBuildKit.TryGetAssemblyHandoff(
                    PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                    out _))
            {
                return CustomPcBuildKit.ReleaseCanonicalEps12vPowerCableForAssembly(
                    PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                    workOrder,
                    Eps12vPowerCableRouteContainerId,
                    expectedBuildKitRevision,
                    expectedInventoryRevision);
            }

            AssemblyBuildSnapshot snapshot = AssemblyBuild.GetSnapshot();
            Eps12vPowerCableDefinition definition =
                AssemblyBuild.Eps12vPowerCableDefinition;
            Eps12vPowerCableTopology topology = definition.Topology;
            if (snapshot.Revision != expectedAssemblyRevision ||
                AssemblyBuild.Eps12vPowerCableRevision != expectedCableRevision ||
                AssemblyBuild.Eps12vPowerCableState != Eps12vPowerCableState.Loose ||
                AssemblyBuild.Eps12vPowerCableReceiptCount != 0 ||
                AssemblyBuild.Atx24PowerCableRevision != expectedAtx24CableRevision ||
                !definition.IsValid ||
                definition.ProductId != Eps12vPowerCableProductId ||
                definition.RouteContainerId != Eps12vPowerCableRouteContainerId ||
                topology == null ||
                !topology.IsValid ||
                topology.RouteId != Eps12vPowerCableRouteId ||
                topology.PsuEndpoint.EndpointId != Eps12vPowerCablePsuEndpointId ||
                topology.PsuEndpoint.ConnectorType !=
                    PowerCableConnectorType.PsuModularEps12v8 ||
                topology.MotherboardEndpoint.EndpointId !=
                    Eps12vPowerCableMotherboardEndpointId ||
                topology.MotherboardEndpoint.ConnectorType !=
                    PowerCableConnectorType.MotherboardEps12v8 ||
                topology.FirstWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    Eps12vPowerCableWaypoint1IdValue) ||
                topology.SecondWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    Eps12vPowerCableWaypoint2IdValue) ||
                topology.ThirdWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    Eps12vPowerCableWaypoint3IdValue) ||
                !Inventory.TryGetContainer(
                    Eps12vPowerCableRouteContainerId,
                    out InventoryContainerDefinition routeContainer) ||
                routeContainer.Kind != InventoryContainerKind.Workbench ||
                routeContainer.UnitCapacity != 1 ||
                Inventory.GetContainerQuantity(
                    Eps12vPowerCableRouteContainerId).Value != 0 ||
                snapshot.MotherboardSeatState != AssemblySeatState.SeatedSecured ||
                snapshot.ProcessorSocketState != ProcessorSocketState.ProcessorRetained ||
                snapshot.MemorySlotState != MemorySlotState.MemoryModuleRetained ||
                snapshot.StorageSlotState != StorageSlotState.StorageDeviceSecured ||
                snapshot.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.CoolerRetained ||
                snapshot.ProcessorCoolerTimState !=
                    ProcessorCoolerTimState.AppliedConsumed ||
                snapshot.GraphicsCardSlotState !=
                    GraphicsCardSlotState.GraphicsCardRetained ||
                snapshot.PowerSupplyBayState !=
                    PowerSupplyBayState.PowerSupplyRetained ||
                !HasExactRoutedAtx24AssemblyPrerequisite(workOrder) ||
                !TryGetEps12vPowerCableItem(out InventoryItemRecord cable) ||
                cable.Id != Eps12vPowerCableItemId ||
                cable.ProductId != Eps12vPowerCableProductId ||
                (cable.ContainerId != Eps12vPowerCableBuildKitContainerId &&
                 cable.ContainerId != HandsContainerId) ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeEps12vPowerCableBuildKitOperationId,
                    out CustomPcBuildKitReceipt staging) ||
                staging.Stage != CustomPcBuildKitStage.Eps12vPowerCableStaged ||
                staging.Line.ComponentKind != PcComponentKind.PowerCable ||
                staging.Line.PowerCableType !=
                    PowerCableType.ModularEps12v8PinPsuToMotherboard ||
                staging.Line.ProductId != cable.ProductId ||
                staging.Line.ItemId != cable.Id ||
                staging.BuildKitContainerId !=
                    Eps12vPowerCableBuildKitContainerId ||
                !ReferenceEquals(staging.BuildOrder, workOrder) ||
                !HasLiveSecuredMotherboardAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedProcessorAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedMemoryModuleAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveSecuredStorageAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedProcessorCoolerAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedGraphicsCardAssemblyPrerequisite(
                    snapshot,
                    workOrder,
                    requireCurrentRevision: false) ||
                !HasLiveRetainedPowerSupplyAssemblyPrerequisite(
                    snapshot,
                    workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            return CustomPcBuildKit.ReleaseCanonicalEps12vPowerCableForAssembly(
                PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                workOrder,
                Eps12vPowerCableRouteContainerId,
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

        private bool HasExactRoutedAtx24AssemblyPrerequisite(
            CustomPcBuildOrderRecord workOrder)
        {
            if (workOrder == null ||
                !AssemblyBuild.IsAtx24PowerCableRouted ||
                AssemblyBuild.Atx24PowerCableRoutedByOperationId.IsEmpty ||
                AssemblyBuild.ValidateAtx24PowerCableReceiptHistory().IsFailure ||
                !AssemblyBuild.TryGetAtx24PowerCableReceipt(
                    AssemblyBuild.Atx24PowerCableRoutedByOperationId,
                    out Atx24PowerCableOperationReceipt routeReceipt) ||
                routeReceipt.OperationKind != Atx24PowerCableOperationKind.Route ||
                routeReceipt.BuildId != AssemblyBuild.BuildId ||
                routeReceipt.ChassisId != AssemblyBuild.ChassisId ||
                routeReceipt.ItemId != Atx24PowerCableItemId ||
                routeReceipt.ProductId != Atx24PowerCableProductId ||
                routeReceipt.SourceContainerId != HandsContainerId ||
                routeReceipt.TargetContainerId != Atx24PowerCableRouteContainerId ||
                routeReceipt.ResultingState != Atx24PowerCableState.Routed ||
                routeReceipt.CableRevision != AssemblyBuild.Atx24PowerCableRevision ||
                !TryGetAtx24PowerCableItem(out InventoryItemRecord atx24) ||
                atx24.ContainerId != Atx24PowerCableRouteContainerId ||
                !CustomPcBuildKit.TryGetAssemblyHandoff(
                    PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                    out CustomPcBuildKitAssemblyHandoffReceipt handoff) ||
                !ReferenceEquals(handoff.BuildOrder, workOrder) ||
                handoff.ComponentKind != PcComponentKind.PowerCable ||
                handoff.Line.PowerCableType !=
                    PowerCableType.ModularAtx24SplitPsuToMotherboard ||
                handoff.Line.ItemId != atx24.Id ||
                handoff.Line.ProductId != atx24.ProductId ||
                handoff.WorkbenchContainerId != Atx24PowerCableRouteContainerId)
            {
                return false;
            }

            return routeReceipt.Definition.IsValid &&
                   routeReceipt.Definition.ProductId == atx24.ProductId &&
                   routeReceipt.Definition.RouteContainerId ==
                       Atx24PowerCableRouteContainerId;
        }
    }
}
