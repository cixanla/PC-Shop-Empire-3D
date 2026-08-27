using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

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

            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalAtx24PowerCable(
                        PrototypeAtx24PowerCableBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            return Inventory.TransferSerializedItem(
                Atx24PowerCableItemId,
                HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldAtx24PowerCableInCustomPcBuildKit()
        {
            return PlaceHeldAtx24PowerCableInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldAtx24PowerCableInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeAtx24PowerCableBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.Atx24PowerCableInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalAtx24PowerCable(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedAtx24PowerCableForAssembly()
        {
            return PickupStagedAtx24PowerCableForAssembly(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision,
                AssemblyBuild.Revision,
                AssemblyBuild.Atx24PowerCableRevision);
        }

        public OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>
            PickupStagedAtx24PowerCableForAssembly(
                long expectedBuildKitRevision,
                long expectedInventoryRevision,
                long expectedAssemblyRevision,
                long expectedCableRevision)
        {
            if (CustomPcBuildKit == null ||
                !TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder))
            {
                return OperationResult<CustomPcBuildKitAssemblyHandoffReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAssemblyStageInvalid);
            }

            if (CustomPcBuildKit.TryGetAssemblyHandoff(
                    PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                    out _))
            {
                return CustomPcBuildKit.ReleaseCanonicalAtx24PowerCableForAssembly(
                    PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                    workOrder,
                    Atx24PowerCableRouteContainerId,
                    expectedBuildKitRevision,
                    expectedInventoryRevision);
            }

            AssemblyBuildSnapshot snapshot = AssemblyBuild.GetSnapshot();
            Atx24PowerCableDefinition definition =
                AssemblyBuild.Atx24PowerCableDefinition;
            Atx24PowerCableTopology topology = definition.Topology;
            if (snapshot.Revision != expectedAssemblyRevision ||
                AssemblyBuild.Atx24PowerCableRevision != expectedCableRevision ||
                AssemblyBuild.Atx24PowerCableState != Atx24PowerCableState.Loose ||
                AssemblyBuild.Atx24PowerCableReceiptCount != 0 ||
                !definition.IsValid ||
                definition.ProductId != Atx24PowerCableProductId ||
                definition.RouteContainerId != Atx24PowerCableRouteContainerId ||
                topology == null ||
                !topology.IsValid ||
                topology.RouteId != Atx24PowerCableRouteId ||
                topology.PsuPrimaryEndpoint.EndpointId !=
                    Atx24PowerCablePsuPrimaryEndpointId ||
                topology.PsuPrimaryEndpoint.ConnectorType !=
                    PowerCableConnectorType.PsuModularAtx24Primary18 ||
                topology.PsuSenseEndpoint.EndpointId !=
                    Atx24PowerCablePsuSenseEndpointId ||
                topology.PsuSenseEndpoint.ConnectorType !=
                    PowerCableConnectorType.PsuModularAtx24Sense10 ||
                topology.MotherboardEndpoint.EndpointId !=
                    Atx24PowerCableMotherboardEndpointId ||
                topology.MotherboardEndpoint.ConnectorType !=
                    PowerCableConnectorType.MotherboardAtx24 ||
                topology.FirstWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    Atx24PowerCableWaypoint1IdValue) ||
                topology.SecondWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    Atx24PowerCableWaypoint2IdValue) ||
                topology.ThirdWaypointId != StableId<
                    AssemblyPowerCableWaypointIdScope>.Parse(
                    Atx24PowerCableWaypoint3IdValue) ||
                !Inventory.TryGetContainer(
                    Atx24PowerCableRouteContainerId,
                    out InventoryContainerDefinition routeContainer) ||
                routeContainer.Kind != InventoryContainerKind.Workbench ||
                routeContainer.UnitCapacity != 1 ||
                Inventory.GetContainerQuantity(
                    Atx24PowerCableRouteContainerId).Value != 0 ||
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
                !TryGetAtx24PowerCableItem(out InventoryItemRecord cable) ||
                cable.Id != Atx24PowerCableItemId ||
                cable.ProductId != Atx24PowerCableProductId ||
                (cable.ContainerId != Atx24PowerCableBuildKitContainerId &&
                 cable.ContainerId != HandsContainerId) ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypeAtx24PowerCableBuildKitOperationId,
                    out CustomPcBuildKitReceipt staging) ||
                staging.Stage != CustomPcBuildKitStage.Atx24PowerCableStaged ||
                staging.Line.ComponentKind != PcComponentKind.PowerCable ||
                staging.Line.PowerCableType !=
                    PowerCableType.ModularAtx24SplitPsuToMotherboard ||
                staging.Line.ProductId != cable.ProductId ||
                staging.Line.ItemId != cable.Id ||
                staging.BuildKitContainerId !=
                    Atx24PowerCableBuildKitContainerId ||
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

            return CustomPcBuildKit.ReleaseCanonicalAtx24PowerCableForAssembly(
                PrototypeAtx24PowerCableAssemblyHandoffOperationId,
                workOrder,
                Atx24PowerCableRouteContainerId,
                expectedBuildKitRevision,
                expectedInventoryRevision);
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

        private bool HasLiveRetainedPowerSupplyAssemblyPrerequisite(
            AssemblyBuildSnapshot snapshot,
            CustomPcBuildOrderRecord workOrder)
        {
            if (workOrder == null ||
                snapshot.BuildId != AssemblyBuild.BuildId ||
                snapshot.PowerSupplyBayState !=
                    PowerSupplyBayState.PowerSupplyRetained ||
                snapshot.PowerSupplyItemId != PowerSupplyItemId ||
                snapshot.PowerSupplyProductId != PowerSupplyProductId ||
                snapshot.PowerSupplySeatedByOperationId.IsEmpty ||
                snapshot.PowerSupplyRetainedByOperationId.IsEmpty ||
                !CustomPcBuildKit.TryGetAssemblyHandoff(
                    PrototypePowerSupplyAssemblyHandoffOperationId,
                    out CustomPcBuildKitAssemblyHandoffReceipt handoff) ||
                !ReferenceEquals(handoff.BuildOrder, workOrder) ||
                handoff.ComponentKind != PcComponentKind.PowerSupply ||
                handoff.Line.ItemId != snapshot.PowerSupplyItemId ||
                handoff.Line.ProductId != snapshot.PowerSupplyProductId ||
                handoff.WorkbenchContainerId != PowerSupplyBayContainerId ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.PowerSupplySeatedByOperationId,
                    out AssemblyOperationReceipt seat) ||
                !AssemblyBuild.TryGetReceipt(
                    snapshot.PowerSupplyRetainedByOperationId,
                    out AssemblyOperationReceipt retain) ||
                !TryGetPowerSupplyItem(out InventoryItemRecord item))
            {
                return false;
            }

            PowerSupplyBayDefinition bay = snapshot.PowerSupplyBayDefinition;
            PowerSupplyRetentionTopology retention = bay.RetentionTopology;
            return bay.IsValid &&
                   retention != null &&
                   retention.IsValid &&
                   item.Id == snapshot.PowerSupplyItemId &&
                   item.ProductId == snapshot.PowerSupplyProductId &&
                   item.ContainerId == PowerSupplyBayContainerId &&
                   seat.OperationId == snapshot.PowerSupplySeatedByOperationId &&
                   seat.OperationKind == AssemblyOperationKind.SeatPowerSupply &&
                   seat.BuildId == snapshot.BuildId &&
                   seat.ChassisId == snapshot.ChassisId &&
                   seat.SlotId == bay.SlotId &&
                   seat.ItemId == snapshot.PowerSupplyItemId &&
                   seat.ProductId == snapshot.PowerSupplyProductId &&
                   seat.SourceContainerId == HandsContainerId &&
                   seat.TargetContainerId == PowerSupplyBayContainerId &&
                   seat.PreviousPowerSupplyBayState == PowerSupplyBayState.EmptyOpen &&
                   seat.ResultingPowerSupplyBayState ==
                       PowerSupplyBayState.PowerSupplySeatedUnsecured &&
                   seat.PowerSupplyMountOrientation ==
                       snapshot.PowerSupplyMountOrientation &&
                   seat.PowerSupplyBayDefinition.SlotId == bay.SlotId &&
                   seat.PowerSupplyBayDefinition.ContainerId == bay.ContainerId &&
                   seat.PowerSupplyBayDefinition.SupportedPowerSupplyType ==
                       bay.SupportedPowerSupplyType &&
                   retain.OperationId == snapshot.PowerSupplyRetainedByOperationId &&
                   retain.OperationKind == AssemblyOperationKind.RetainPowerSupply &&
                   retain.BuildId == snapshot.BuildId &&
                   retain.ChassisId == snapshot.ChassisId &&
                   retain.SlotId == bay.SlotId &&
                   retain.ItemId == snapshot.PowerSupplyItemId &&
                   retain.ProductId == snapshot.PowerSupplyProductId &&
                   retain.SourcePowerSupplySeatOperationId == seat.OperationId &&
                   retain.PreviousPowerSupplyBayState ==
                       PowerSupplyBayState.PowerSupplySeatedUnsecured &&
                   retain.ResultingPowerSupplyBayState ==
                       PowerSupplyBayState.PowerSupplyRetained &&
                   retain.PowerSupplyMountOrientation ==
                       snapshot.PowerSupplyMountOrientation &&
                   retain.AssemblyRevision == snapshot.Revision;
        }
    }
}
