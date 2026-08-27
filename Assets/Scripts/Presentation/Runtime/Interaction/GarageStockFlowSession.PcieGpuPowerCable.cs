using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

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

            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalPcieGpuPowerCable(
                        PrototypePcieGpuPowerCableBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            return Inventory.TransferSerializedItem(
                PcieGpuPowerCableItemId,
                HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldPcieGpuPowerCableInCustomPcBuildKit()
        {
            return PlaceHeldPcieGpuPowerCableInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldPcieGpuPowerCableInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypePcieGpuPowerCableBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.PcieGpuPowerCableInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalPcieGpuPowerCable(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedPcieGpuPowerCableForAssembly()
        {
            return PickupStagedPcieGpuPowerCableForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision,
                AssemblyBuild.PcieGpuPowerCableRevision,
                AssemblyBuild.Atx24PowerCableRevision,
                AssemblyBuild.Eps12vPowerCableRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedPcieGpuPowerCableForAssembly(
                long expectedBuildKitRevision,
                long expectedInventoryRevision,
                long expectedAssemblyRevision,
                long expectedCableRevision,
                long expectedAtx24CableRevision,
                long expectedEps12vCableRevision)
        {
            if (CustomPcBuildKit == null ||
                !TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            if (CustomPcBuildKit.TryGetAssemblyHandoff(
                    PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                    out _))
            {
                return CustomPcBuildKit.ReleaseCanonicalPcieGpuPowerCableForAssembly(
                    PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                    workOrder,
                    PcieGpuPowerCableRouteContainerId,
                    expectedBuildKitRevision,
                    expectedInventoryRevision);
            }

            AssemblyBuildSnapshot snapshot = AssemblyBuild.GetSnapshot();
            PcieGpuPowerCableDefinition definition =
                AssemblyBuild.PcieGpuPowerCableDefinition;
            PcieGpuPowerCableTopology topology = definition.Topology;
            if (snapshot.Revision != expectedAssemblyRevision ||
                AssemblyBuild.PcieGpuPowerCableRevision != expectedCableRevision ||
                AssemblyBuild.PcieGpuPowerCableState !=
                    PcieGpuPowerCableState.Loose ||
                AssemblyBuild.PcieGpuPowerCableReceiptCount != 0 ||
                AssemblyBuild.Atx24PowerCableRevision !=
                    expectedAtx24CableRevision ||
                AssemblyBuild.Eps12vPowerCableRevision !=
                    expectedEps12vCableRevision ||
                !definition.IsValid ||
                definition.ProductId != PcieGpuPowerCableProductId ||
                definition.RouteContainerId != PcieGpuPowerCableRouteContainerId ||
                topology == null ||
                !topology.IsValid ||
                topology.RouteId != PcieGpuPowerCableRouteId ||
                topology.PsuEndpoint.EndpointId != PcieGpuPowerCablePsuEndpointId ||
                topology.PsuEndpoint.ConnectorType !=
                    PowerCableConnectorType.PsuModularPcie8 ||
                topology.GraphicsCardEndpoint.EndpointId !=
                    PcieGpuPowerCableGraphicsCardEndpointId ||
                topology.GraphicsCardEndpoint.ConnectorType !=
                    PowerCableConnectorType.GraphicsCardPcie8 ||
                topology.FirstWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    PcieGpuPowerCableWaypoint1IdValue) ||
                topology.SecondWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    PcieGpuPowerCableWaypoint2IdValue) ||
                topology.ThirdWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    PcieGpuPowerCableWaypoint3IdValue) ||
                !Inventory.TryGetContainer(
                    PcieGpuPowerCableRouteContainerId,
                    out InventoryContainerDefinition routeContainer) ||
                routeContainer.Kind != InventoryContainerKind.Workbench ||
                routeContainer.UnitCapacity != 1 ||
                Inventory.GetContainerQuantity(
                    PcieGpuPowerCableRouteContainerId).Value != 0 ||
                snapshot.MotherboardSeatState != AssemblySeatState.SeatedSecured ||
                snapshot.ProcessorSocketState !=
                    ProcessorSocketState.ProcessorRetained ||
                snapshot.MemorySlotState !=
                    MemorySlotState.MemoryModuleRetained ||
                snapshot.StorageSlotState !=
                    StorageSlotState.StorageDeviceSecured ||
                snapshot.ProcessorCoolerSlotState !=
                    ProcessorCoolerSlotState.CoolerRetained ||
                snapshot.ProcessorCoolerTimState !=
                    ProcessorCoolerTimState.AppliedConsumed ||
                snapshot.GraphicsCardSlotState !=
                    GraphicsCardSlotState.GraphicsCardRetained ||
                snapshot.PowerSupplyBayState !=
                    PowerSupplyBayState.PowerSupplyRetained ||
                !HasExactRoutedAtx24AssemblyPrerequisite(workOrder) ||
                !HasExactRoutedEps12vAssemblyPrerequisite(workOrder) ||
                !TryGetPcieGpuPowerCableItem(out InventoryItemRecord cable) ||
                cable.Id != PcieGpuPowerCableItemId ||
                cable.ProductId != PcieGpuPowerCableProductId ||
                (cable.ContainerId != PcieGpuPowerCableBuildKitContainerId &&
                 cable.ContainerId != HandsContainerId) ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypePcieGpuPowerCableBuildKitOperationId,
                    out CustomPcBuildKitReceipt staging) ||
                staging.Stage != CustomPcBuildKitStage.PcieGpuPowerCableStaged ||
                staging.Line.ComponentKind != PcComponentKind.PowerCable ||
                staging.Line.PowerCableType !=
                    PowerCableType.ModularPcie8PinPsuToGraphicsCard ||
                staging.Line.ProductId != cable.ProductId ||
                staging.Line.ItemId != cable.Id ||
                staging.BuildKitContainerId !=
                    PcieGpuPowerCableBuildKitContainerId ||
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

            return CustomPcBuildKit.ReleaseCanonicalPcieGpuPowerCableForAssembly(
                PrototypePcieGpuPowerCableAssemblyHandoffOperationId,
                workOrder,
                PcieGpuPowerCableRouteContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
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

        private bool HasExactRoutedEps12vAssemblyPrerequisite(
            CustomPcBuildOrderRecord workOrder)
        {
            if (workOrder == null ||
                !AssemblyBuild.IsEps12vPowerCableRouted ||
                AssemblyBuild.Eps12vPowerCableRoutedByOperationId.IsEmpty ||
                AssemblyBuild.ValidateEps12vPowerCableReceiptHistory().IsFailure ||
                !AssemblyBuild.TryGetEps12vPowerCableReceipt(
                    AssemblyBuild.Eps12vPowerCableRoutedByOperationId,
                    out Eps12vPowerCableOperationReceipt routeReceipt) ||
                routeReceipt.OperationKind != Eps12vPowerCableOperationKind.Route ||
                routeReceipt.BuildId != AssemblyBuild.BuildId ||
                routeReceipt.ChassisId != AssemblyBuild.ChassisId ||
                routeReceipt.ItemId != Eps12vPowerCableItemId ||
                routeReceipt.ProductId != Eps12vPowerCableProductId ||
                routeReceipt.SourceContainerId != HandsContainerId ||
                routeReceipt.TargetContainerId != Eps12vPowerCableRouteContainerId ||
                routeReceipt.ResultingState != Eps12vPowerCableState.Routed ||
                routeReceipt.CableRevision !=
                    AssemblyBuild.Eps12vPowerCableRevision ||
                !TryGetEps12vPowerCableItem(out InventoryItemRecord eps12v) ||
                eps12v.ContainerId != Eps12vPowerCableRouteContainerId ||
                !CustomPcBuildKit.TryGetAssemblyHandoff(
                    PrototypeEps12vPowerCableAssemblyHandoffOperationId,
                    out CustomPcBuildKitAssemblyHandoffReceipt handoff) ||
                !ReferenceEquals(handoff.BuildOrder, workOrder) ||
                handoff.ComponentKind != PcComponentKind.PowerCable ||
                handoff.Line.PowerCableType !=
                    PowerCableType.ModularEps12v8PinPsuToMotherboard ||
                handoff.Line.ItemId != eps12v.Id ||
                handoff.Line.ProductId != eps12v.ProductId ||
                handoff.WorkbenchContainerId != Eps12vPowerCableRouteContainerId)
            {
                return false;
            }

            return routeReceipt.Definition.IsValid &&
                   routeReceipt.Definition.ProductId == eps12v.ProductId &&
                   routeReceipt.Definition.RouteContainerId ==
                       Eps12vPowerCableRouteContainerId;
        }
    }
}
