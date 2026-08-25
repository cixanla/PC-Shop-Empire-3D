using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PowerSupplyProductIdValue =
            "catalog.psu.northstar-p01-atx";
        public const string PowerSupplyCategoryIdValue =
            "catalog.category.power-supplies";
        public const string PowerSupplyItemInstanceIdValue =
            "inventory.item.northstar-p01-atx-001";
        public const string PowerSupplyBayContainerIdValue =
            "inventory.container.assembly-power-supply-bay";
        public const string PowerSupplyBaySlotIdValue =
            "assembly.slot.power-supply-bottom-rear";
        public const string PowerSupplyRearMountIdValue =
            "assembly.mount.power-supply-rear";
        public const string PowerSupplyTopLeftFastenerIdValue =
            "assembly.fastener.power-supply-rear-01";
        public const string PowerSupplyTopRightFastenerIdValue =
            "assembly.fastener.power-supply-rear-02";
        public const string PowerSupplyBottomLeftFastenerIdValue =
            "assembly.fastener.power-supply-rear-03";
        public const string PowerSupplyBottomRightFastenerIdValue =
            "assembly.fastener.power-supply-rear-04";
        public const string PowerSupplyDisplayName =
            "Northstar P-01 ATX Güç Kaynağı";
        public const long PowerSupplyUnitCostMinorUnits = 7_900;

        public StableId<ProductDefinitionIdScope> PowerSupplyProductId =>
            StableId<ProductDefinitionIdScope>.Parse(PowerSupplyProductIdValue);

        public StableId<ItemInstanceIdScope> PowerSupplyItemId =>
            StableId<ItemInstanceIdScope>.Parse(PowerSupplyItemInstanceIdValue);

        public StableId<ContainerIdScope> PowerSupplyBayContainerId =>
            StableId<ContainerIdScope>.Parse(PowerSupplyBayContainerIdValue);

        public StableId<AssemblySlotIdScope> PowerSupplyBaySlotId =>
            StableId<AssemblySlotIdScope>.Parse(PowerSupplyBaySlotIdValue);

        public StableId<AssemblyPowerSupplyRearMountIdScope> PowerSupplyRearMountId =>
            StableId<AssemblyPowerSupplyRearMountIdScope>.Parse(
                PowerSupplyRearMountIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyTopLeftFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyTopLeftFastenerIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyTopRightFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyTopRightFastenerIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyBottomLeftFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyBottomLeftFastenerIdValue);

        public StableId<AssemblyFastenerIdScope> PowerSupplyBottomRightFastenerId =>
            StableId<AssemblyFastenerIdScope>.Parse(
                PowerSupplyBottomRightFastenerIdValue);

        public bool TryGetPowerSupplyItem(out InventoryItemRecord item)
        {
            return Inventory.TryGetSerializedItem(PowerSupplyItemId, out item);
        }

        public OperationResult PickupLoosePowerSupplyToHands()
        {
            if (CustomPcBuildKit != null &&
                TryGetPrototypeCustomPcBuildOrder(out CustomPcBuildOrderRecord workOrder))
            {
                OperationResult<CustomPcBuildKitReceipt> pickup =
                    CustomPcBuildKit.PickupCanonicalPowerSupply(
                        PrototypePowerSupplyBuildKitOperationId,
                        workOrder);
                return pickup.IsSuccess
                    ? OperationResult.Success()
                    : OperationResult.Fail(pickup.Error);
            }

            if (!AssemblyBuild.HasPowerSupplyBay ||
                AssemblyBuild.PowerSupplyBayState != PowerSupplyBayState.EmptyOpen ||
                !TryGetPowerSupplyItem(out InventoryItemRecord item) ||
                item.ContainerId != WorldFloorContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.loose-pickup-invalid"));
            }

            return Inventory.TransferSerializedItem(
                PowerSupplyItemId,
                HandsContainerId);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldPowerSupplyInCustomPcBuildKit()
        {
            return PlaceHeldPowerSupplyInCustomPcBuildKit(
                CustomPcBuildKit?.Revision ?? -1L,
                Inventory.Revision);
        }

        public OperationResult<CustomPcBuildKitReceipt>
            PlaceHeldPowerSupplyInCustomPcBuildKit(
                long expectedBuildKitRevision,
                long expectedInventoryRevision)
        {
            if (CustomPcBuildKit == null ||
                !CustomPcBuildKit.TryGetReceipt(
                    PrototypePowerSupplyBuildKitOperationId,
                    out CustomPcBuildKitReceipt pickup) ||
                pickup.Stage != CustomPcBuildKitStage.PowerSupplyInHands)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            return CustomPcBuildKit.PlaceCanonicalPowerSupply(
                pickup,
                expectedBuildKitRevision,
                expectedInventoryRevision);
        }

        public OperationResult DropHeldPowerSupplyToWorld()
        {
            if (CustomPcBuildKit != null &&
                CustomPcBuildKit.TryGetReceipt(
                    PrototypePowerSupplyBuildKitOperationId,
                    out CustomPcBuildKitReceipt buildKitReceipt) &&
                buildKitReceipt.Stage == CustomPcBuildKitStage.PowerSupplyInHands)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.BuildKitStageInvalid);
            }

            if (!AssemblyBuild.HasPowerSupplyBay ||
                AssemblyBuild.PowerSupplyBayState != PowerSupplyBayState.EmptyOpen ||
                !TryGetPowerSupplyItem(out InventoryItemRecord item) ||
                item.ContainerId != HandsContainerId)
            {
                return OperationResult.Fail(
                    Failure.FromCode("assembly-power-supply.world-drop-invalid"));
            }

            return Inventory.TransferSerializedItem(
                PowerSupplyItemId,
                WorldFloorContainerId);
        }

        public OperationResult<AssemblyOperationReceipt> SeatPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            PowerSupplyMountOrientation orientation,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.SeatPowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                orientation,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RetainPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RetainPowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                PowerSupplyRearMountId,
                PowerSupplyTopLeftFastenerId,
                PowerSupplyTopRightFastenerId,
                PowerSupplyBottomLeftFastenerId,
                PowerSupplyBottomRightFastenerId,
                sourcePowerSupplySeatOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> UnretainPowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplyRetentionOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.UnretainPowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                PowerSupplyRearMountId,
                PowerSupplyTopLeftFastenerId,
                PowerSupplyTopRightFastenerId,
                PowerSupplyBottomLeftFastenerId,
                PowerSupplyBottomRightFastenerId,
                sourcePowerSupplySeatOperationId,
                sourcePowerSupplyRetentionOperationId,
                expectedAssemblyRevision);
        }

        public OperationResult<AssemblyOperationReceipt> RemovePowerSupply(
            StableId<AssemblyOperationIdScope> operationId,
            StableId<AssemblyOperationIdScope> sourcePowerSupplySeatOperationId,
            long expectedAssemblyRevision)
        {
            return AssemblyBuild.RemovePowerSupply(
                operationId,
                PowerSupplyItemId,
                PowerSupplyBaySlotId,
                sourcePowerSupplySeatOperationId,
                expectedAssemblyRevision);
        }
    }
}
