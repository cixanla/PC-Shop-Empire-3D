using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Orders
{
    public sealed partial class CustomPcBuildKitAuthorityTests
    {
        [Test]
        public void PcieGpuPowerCableBuildKitRequiresFirstNineAndSelectsExactFamily()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstEightBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot pcieGpu =
                CanonicalPcieGpuPowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot atx24 =
                CanonicalAtx24PowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot eps12v = workOrder.Lines.Single(
                line => line.PowerCableType ==
                    PowerCableType.ModularEps12v8PinPsuToMotherboard);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long pcieGpuRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;

            OperationResult<CustomPcBuildKitReceipt> prerequisiteFailure =
                session.CustomPcBuildKit.PickupCanonicalPcieGpuPowerCable(
                    session.PrototypePcieGpuPowerCableBuildKitOperationId,
                    workOrder);

            Assert.That(prerequisiteFailure.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.TryGetSerializedItem(
                pcieGpu.ItemId,
                out InventoryItemRecord untouchedPcieGpu), Is.True);
            Assert.That(untouchedPcieGpu.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));

            CustomPcBuildKitReceipt eps12vPickup = session.CustomPcBuildKit
                .PickupCanonicalEps12vPowerCable(
                    session.PrototypeEps12vPowerCableBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalEps12vPowerCable(eps12vPickup).IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.StagedComponentCount,
                Is.EqualTo(9));

            CustomPcBuildKitReceipt pickup = session.CustomPcBuildKit
                .PickupCanonicalPcieGpuPowerCable(
                    session.PrototypePcieGpuPowerCableBuildKitOperationId,
                    workOrder).Value;

            Assert.That(pickup.Stage,
                Is.EqualTo(CustomPcBuildKitStage.PcieGpuPowerCableInHands));
            Assert.That(pickup.Line, Is.SameAs(pcieGpu));
            Assert.That(pickup.Line.LineId, Is.EqualTo(pcieGpu.LineId));
            Assert.That(pickup.Line.ProductId, Is.EqualTo(pcieGpu.ProductId));
            Assert.That(pickup.Line.ItemId, Is.EqualTo(pcieGpu.ItemId));
            Assert.That(pickup.Line.ReservationId, Is.EqualTo(pcieGpu.ReservationId));
            Assert.That(pickup.Line.PowerCableType,
                Is.EqualTo(PowerCableType.ModularPcie8PinPsuToGraphicsCard));
            Assert.That(session.Inventory.TryGetSerializedItem(
                pcieGpu.ItemId,
                out InventoryItemRecord heldPcieGpu), Is.True);
            Assert.That(heldPcieGpu.ContainerId,
                Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt placement = session.CustomPcBuildKit
                .PlaceCanonicalPcieGpuPowerCable(pickup).Value;

            Assert.That(placement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.PcieGpuPowerCableStaged));
            Assert.That(placement.Line, Is.SameAs(pcieGpu));
            Assert.That(session.Inventory.TryGetSerializedItem(
                pcieGpu.ItemId,
                out InventoryItemRecord stagedPcieGpu), Is.True);
            Assert.That(stagedPcieGpu.ContainerId,
                Is.EqualTo(session.PcieGpuPowerCableBuildKitContainerId));
            Assert.That(session.CustomPcBuildKit.PcieGpuPowerCableBuildKitContainerId,
                Is.EqualTo(session.PcieGpuPowerCableBuildKitContainerId));
            Assert.That(session.CustomPcBuildKit.ActiveKitCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.Inventory.TryGetSerializedItem(
                atx24.ItemId,
                out InventoryItemRecord stagedAtx24), Is.True);
            Assert.That(stagedAtx24.ContainerId,
                Is.EqualTo(session.Atx24PowerCableBuildKitContainerId));
            Assert.That(session.Inventory.TryGetSerializedItem(
                eps12v.ItemId,
                out InventoryItemRecord stagedEps12v), Is.True);
            Assert.That(stagedEps12v.ContainerId,
                Is.EqualTo(session.Eps12vPowerCableBuildKitContainerId));

            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieGpuRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PcieGpuWrongFamilyForgeryStaleAndReplayAreMutationFree()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstNineBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot pcieGpu =
                CanonicalPcieGpuPowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot atx24 =
                CanonicalAtx24PowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot eps12v = workOrder.Lines.Single(
                line => line.PowerCableType ==
                    PowerCableType.ModularEps12v8PinPsuToMotherboard);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildKitReceipt pickup = buildKit
                .PickupCanonicalPcieGpuPowerCable(
                    session.PrototypePcieGpuPowerCableBuildKitOperationId,
                    workOrder).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            OperationResult<CustomPcBuildKitReceipt> pickupReplay = buildKit
                .PickupCanonicalPcieGpuPowerCable(
                    session.PrototypePcieGpuPowerCableBuildKitOperationId,
                    workOrder);
            Assert.That(pickupReplay.IsSuccess, Is.True);
            Assert.That(pickupReplay.Value, Is.SameAs(pickup));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));

            CustomPcBuildKitReceipt[] wrongFamilyReceipts =
            {
                CloneReceipt(
                    pickup,
                    CloneLine(
                        pcieGpu,
                        pcieGpu.LineId,
                        pcieGpu.ProductId,
                        pcieGpu.ItemId,
                        pcieGpu.ReservationId,
                        pcieGpu.ComponentKind,
                        PowerCableType.ModularAtx24SplitPsuToMotherboard)),
                CloneReceipt(
                    pickup,
                    CloneLine(
                        pcieGpu,
                        pcieGpu.LineId,
                        pcieGpu.ProductId,
                        pcieGpu.ItemId,
                        pcieGpu.ReservationId,
                        pcieGpu.ComponentKind,
                        PowerCableType.ModularEps12v8PinPsuToMotherboard)),
                CloneReceipt(pickup, atx24),
                CloneReceipt(pickup, eps12v)
            };

            foreach (CustomPcBuildKitReceipt forgery in wrongFamilyReceipts)
            {
                OperationResult<CustomPcBuildKitReceipt> result =
                    buildKit.PlaceCanonicalPcieGpuPowerCable(forgery);
                Assert.That(result.Error,
                    Is.EqualTo(CustomPcWorkOrderFailures.BuildKitReceiptInvalid));
                Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
                Assert.That(session.Inventory.TryGetSerializedItem(
                    pcieGpu.ItemId,
                    out InventoryItemRecord stillHeld), Is.True);
                Assert.That(stillHeld.ContainerId,
                    Is.EqualTo(session.HandsContainerId));
            }

            Assert.That(buildKit.PlaceCanonicalPcieGpuPowerCable(
                pickup,
                buildKitRevision - 1,
                inventoryRevision).Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(buildKit.PlaceCanonicalPcieGpuPowerCable(
                pickup,
                buildKitRevision,
                inventoryRevision - 1).Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));

            CustomPcBuildKitReceipt placed = buildKit
                .PlaceCanonicalPcieGpuPowerCable(pickup).Value;
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = buildKit.Revision;
            OperationResult<CustomPcBuildKitReceipt> placementReplay = buildKit
                .PlaceCanonicalPcieGpuPowerCable(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision - 1);
            pickupReplay = buildKit.PickupCanonicalPcieGpuPowerCable(
                session.PrototypePcieGpuPowerCableBuildKitOperationId,
                workOrder);
            OperationResult<CustomPcBuildKitReceipt> secondOperation = buildKit
                .PickupCanonicalPcieGpuPowerCable(
                    StableId<CustomPcBuildKitOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-operation.second-pcie-gpu"),
                    workOrder);

            Assert.That(placementReplay.IsSuccess, Is.True);
            Assert.That(placementReplay.Value, Is.SameAs(placed));
            Assert.That(pickupReplay.IsSuccess, Is.True);
            Assert.That(pickupReplay.Value, Is.SameAs(pickup));
            Assert.That(secondOperation.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitIdentityConflict));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SessionPcieGpuBuildKitCustodyBlocksGenericTransferAndRouteBypass()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstNineBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot pcieGpu =
                CanonicalPcieGpuPowerCable(workOrder);

            OperationResult pickup = session.PickupLoosePcieGpuPowerCableToHands();

            Assert.That(pickup.IsSuccess, Is.True);
            Assert.That(session.Inventory.TryGetSerializedItem(
                pcieGpu.ItemId,
                out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult wrapperDrop = session.DropHeldPcieGpuPowerCableToWorld();
            Assert.That(wrapperDrop.Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<ContainerIdScope>[] bypassTargets =
            {
                session.ReceivingContainerId,
                session.ShelfContainerId,
                session.WorkbenchContainerId,
                session.PcieGpuPowerCableRouteContainerId
            };
            foreach (StableId<ContainerIdScope> target in bypassTargets)
            {
                OperationResult result = session.Inventory.TransferSerializedItem(
                    pcieGpu.ItemId,
                    target);
                Assert.That(result.Error,
                    Is.EqualTo(
                        InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
                Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(session.CustomPcBuildKit.Revision,
                    Is.EqualTo(buildKitRevision));
            }

            OperationResult<PcieGpuPowerCableOperationReceipt> route =
                session.RoutePcieGpuPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.pcie-gpu-build-kit-route-bypass"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.PcieGpuPowerCableRevision);
            Assert.That(route.IsFailure, Is.True);
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            OperationResult<CustomPcBuildKitReceipt> placement =
                session.PlaceHeldPcieGpuPowerCableInCustomPcBuildKit();
            Assert.That(placement.IsSuccess, Is.True);
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = session.CustomPcBuildKit.Revision;
            OperationResult stagedBypass = session.Inventory.TransferSerializedItem(
                pcieGpu.ItemId,
                session.WorldFloorContainerId);
            Assert.That(stagedBypass.Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.TryGetSerializedItem(
                pcieGpu.ItemId,
                out InventoryItemRecord staged), Is.True);
            Assert.That(staged.ContainerId,
                Is.EqualTo(session.PcieGpuPowerCableBuildKitContainerId));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PcieGpuDecupleClaimRejectsOccupiedTenthWithoutPartialClaim()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StableId<ContainerIdScope>[] containerIds = Enumerable.Range(1, 10)
                .Select(index => StableId<ContainerIdScope>.Parse(
                    $"inventory.container.pcie-gpu-build-kit-occupied-{index}"))
                .ToArray();
            foreach (StableId<ContainerIdScope> containerId in containerIds)
            {
                RegisterBuildKitContainer(session, containerId);
            }

            CustomPcBuildOrderLineSnapshot processor = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            Assert.That(session.Inventory.TryGetSerializedItem(
                processor.ItemId,
                out InventoryItemRecord processorItem), Is.True);
            StableId<ItemInstanceIdScope> foreignItemId =
                StableId<ItemInstanceIdScope>.Parse(
                    "inventory.item.pcie-gpu-build-kit-occupied-peer");
            Assert.That(session.Inventory.ReceiveSerializedItem(
                foreignItemId,
                processor.ProductId,
                containerIds[9],
                InventoryCondition.New,
                processorItem.UnitCost).IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAuthority> occupied =
                CustomPcBuildKitAuthority.Create(
                    session.CustomPcWorkOrders,
                    session.WorldFloorContainerId,
                    session.HandsContainerId,
                    containerIds[0],
                    containerIds[1],
                    containerIds[2],
                    containerIds[3],
                    containerIds[4],
                    containerIds[5],
                    containerIds[6],
                    containerIds[7],
                    containerIds[8],
                    containerIds[9]);

            Assert.That(occupied.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitContainerInvalid));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.TransferSerializedItem(
                foreignItemId,
                session.ShelfContainerId).IsSuccess, Is.True,
                "A rejected decuple claim must leave every peer unclaimed.");
            long beforeValidClaimRevision = session.Inventory.Revision;

            OperationResult<CustomPcBuildKitAuthority> valid =
                CustomPcBuildKitAuthority.Create(
                    session.CustomPcWorkOrders,
                    session.WorldFloorContainerId,
                    session.HandsContainerId,
                    containerIds[0],
                    containerIds[1],
                    containerIds[2],
                    containerIds[3],
                    containerIds[4],
                    containerIds[5],
                    containerIds[6],
                    containerIds[7],
                    containerIds[8],
                    containerIds[9]);

            Assert.That(valid.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(beforeValidClaimRevision + 1));
            Assert.That(valid.Value.PcieGpuPowerCableBuildKitContainerId,
                Is.EqualTo(containerIds[9]));
            Assert.That(valid.Value.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageFirstNineBuildKitComponents(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            StageFirstEightBuildKitComponents(session, workOrder);
            CustomPcBuildKitReceipt eps12vPickup = session.CustomPcBuildKit
                .PickupCanonicalEps12vPowerCable(
                    session.PrototypeEps12vPowerCableBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalEps12vPowerCable(eps12vPickup).IsSuccess, Is.True);
        }

        private static CustomPcBuildOrderLineSnapshot CanonicalPcieGpuPowerCable(
            CustomPcBuildOrderRecord workOrder)
        {
            return workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularPcie8PinPsuToGraphicsCard);
        }
    }
}
