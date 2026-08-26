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
        public void Eps12vPowerCableBuildKitRequiresFirstEightAndSelectsExactFamily()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstSevenBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot eps12v =
                CanonicalEps12vPowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot atx24 =
                CanonicalAtx24PowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot pcie = workOrder.Lines.Single(
                line => line.PowerCableType ==
                    PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;

            OperationResult<CustomPcBuildKitReceipt> prerequisiteFailure =
                session.CustomPcBuildKit.PickupCanonicalEps12vPowerCable(
                    session.PrototypeEps12vPowerCableBuildKitOperationId,
                    workOrder);

            Assert.That(prerequisiteFailure.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.TryGetSerializedItem(
                eps12v.ItemId,
                out InventoryItemRecord untouchedEps12v), Is.True);
            Assert.That(untouchedEps12v.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));

            CustomPcBuildKitReceipt atx24Pickup = session.CustomPcBuildKit
                .PickupCanonicalAtx24PowerCable(
                    session.PrototypeAtx24PowerCableBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalAtx24PowerCable(atx24Pickup).IsSuccess, Is.True);
            Assert.That(session.CustomPcBuildKit.StagedComponentCount,
                Is.EqualTo(8));

            CustomPcBuildKitReceipt pickup = session.CustomPcBuildKit
                .PickupCanonicalEps12vPowerCable(
                    session.PrototypeEps12vPowerCableBuildKitOperationId,
                    workOrder).Value;

            Assert.That(pickup.Stage,
                Is.EqualTo(CustomPcBuildKitStage.Eps12vPowerCableInHands));
            Assert.That(pickup.Line, Is.SameAs(eps12v));
            Assert.That(pickup.Line.LineId, Is.EqualTo(eps12v.LineId));
            Assert.That(pickup.Line.ProductId, Is.EqualTo(eps12v.ProductId));
            Assert.That(pickup.Line.ItemId, Is.EqualTo(eps12v.ItemId));
            Assert.That(pickup.Line.ReservationId, Is.EqualTo(eps12v.ReservationId));
            Assert.That(pickup.Line.PowerCableType,
                Is.EqualTo(PowerCableType.ModularEps12v8PinPsuToMotherboard));
            Assert.That(session.Inventory.TryGetSerializedItem(
                eps12v.ItemId,
                out InventoryItemRecord heldEps12v), Is.True);
            Assert.That(heldEps12v.ContainerId,
                Is.EqualTo(session.HandsContainerId));

            CustomPcBuildKitReceipt placement = session.CustomPcBuildKit
                .PlaceCanonicalEps12vPowerCable(pickup).Value;

            Assert.That(placement.Stage,
                Is.EqualTo(CustomPcBuildKitStage.Eps12vPowerCableStaged));
            Assert.That(placement.Line, Is.SameAs(eps12v));
            Assert.That(session.Inventory.TryGetSerializedItem(
                eps12v.ItemId,
                out InventoryItemRecord stagedEps12v), Is.True);
            Assert.That(stagedEps12v.ContainerId,
                Is.EqualTo(session.Eps12vPowerCableBuildKitContainerId));
            Assert.That(session.CustomPcBuildKit.Eps12vPowerCableBuildKitContainerId,
                Is.EqualTo(session.Eps12vPowerCableBuildKitContainerId));
            Assert.That(session.CustomPcBuildKit.ActiveKitCount, Is.EqualTo(9));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(9));
            Assert.That(session.Inventory.TryGetSerializedItem(
                atx24.ItemId,
                out InventoryItemRecord stagedAtx24), Is.True);
            Assert.That(stagedAtx24.ContainerId,
                Is.EqualTo(session.Atx24PowerCableBuildKitContainerId));
            Assert.That(session.Inventory.TryGetSerializedItem(
                pcie.ItemId,
                out InventoryItemRecord untouchedPcie), Is.True);
            Assert.That(untouchedPcie.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));

            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Eps12vWrongFamilyForgeryStaleAndReplayAreMutationFree()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstEightBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot eps12v =
                CanonicalEps12vPowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot atx24 =
                CanonicalAtx24PowerCable(workOrder);
            CustomPcBuildOrderLineSnapshot pcie = workOrder.Lines.Single(
                line => line.PowerCableType ==
                    PowerCableType.ModularPcie8PinPsuToGraphicsCard);
            CustomPcBuildKitAuthority buildKit = session.CustomPcBuildKit;
            CustomPcBuildKitReceipt pickup = buildKit
                .PickupCanonicalEps12vPowerCable(
                    session.PrototypeEps12vPowerCableBuildKitOperationId,
                    workOrder).Value;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = buildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            OperationResult<CustomPcBuildKitReceipt> pickupReplay = buildKit
                .PickupCanonicalEps12vPowerCable(
                    session.PrototypeEps12vPowerCableBuildKitOperationId,
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
                        eps12v,
                        eps12v.LineId,
                        eps12v.ProductId,
                        eps12v.ItemId,
                        eps12v.ReservationId,
                        eps12v.ComponentKind,
                        PowerCableType.ModularAtx24SplitPsuToMotherboard)),
                CloneReceipt(
                    pickup,
                    CloneLine(
                        eps12v,
                        eps12v.LineId,
                        eps12v.ProductId,
                        eps12v.ItemId,
                        eps12v.ReservationId,
                        eps12v.ComponentKind,
                        PowerCableType.ModularPcie8PinPsuToGraphicsCard)),
                CloneReceipt(pickup, atx24),
                CloneReceipt(pickup, pcie)
            };

            foreach (CustomPcBuildKitReceipt forgery in wrongFamilyReceipts)
            {
                OperationResult<CustomPcBuildKitReceipt> result =
                    buildKit.PlaceCanonicalEps12vPowerCable(forgery);
                Assert.That(result.Error,
                    Is.EqualTo(CustomPcWorkOrderFailures.BuildKitReceiptInvalid));
                Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));
                Assert.That(session.Inventory.TryGetSerializedItem(
                    eps12v.ItemId,
                    out InventoryItemRecord stillHeld), Is.True);
                Assert.That(stillHeld.ContainerId,
                    Is.EqualTo(session.HandsContainerId));
            }

            Assert.That(buildKit.PlaceCanonicalEps12vPowerCable(
                pickup,
                buildKitRevision - 1,
                inventoryRevision).Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(buildKit.PlaceCanonicalEps12vPowerCable(
                pickup,
                buildKitRevision,
                inventoryRevision - 1).Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitRevisionStale));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(buildKit.Revision, Is.EqualTo(buildKitRevision));

            CustomPcBuildKitReceipt placed = buildKit
                .PlaceCanonicalEps12vPowerCable(pickup).Value;
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = buildKit.Revision;
            OperationResult<CustomPcBuildKitReceipt> placementReplay = buildKit
                .PlaceCanonicalEps12vPowerCable(
                    pickup,
                    buildKitRevision - 1,
                    inventoryRevision - 1);
            pickupReplay = buildKit.PickupCanonicalEps12vPowerCable(
                session.PrototypeEps12vPowerCableBuildKitOperationId,
                workOrder);
            OperationResult<CustomPcBuildKitReceipt> secondOperation = buildKit
                .PickupCanonicalEps12vPowerCable(
                    StableId<CustomPcBuildKitOperationIdScope>.Parse(
                        "orders.custom-pc-build-kit-operation.second-eps12v"),
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
        public void SessionEps12vBuildKitCustodyBlocksGenericTransferAndRouteBypass()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StageFirstEightBuildKitComponents(session, workOrder);
            CustomPcBuildOrderLineSnapshot eps12v =
                CanonicalEps12vPowerCable(workOrder);

            OperationResult pickup = session.PickupLooseEps12vPowerCableToHands();

            Assert.That(pickup.IsSuccess, Is.True);
            Assert.That(session.Inventory.TryGetSerializedItem(
                eps12v.ItemId,
                out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(session.HandsContainerId));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;

            OperationResult wrapperDrop = session.DropHeldEps12vPowerCableToWorld();
            Assert.That(wrapperDrop.Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));

            StableId<ContainerIdScope>[] bypassTargets =
            {
                session.ReceivingContainerId,
                session.ShelfContainerId,
                session.WorkbenchContainerId,
                session.Eps12vPowerCableRouteContainerId
            };
            foreach (StableId<ContainerIdScope> target in bypassTargets)
            {
                OperationResult result = session.Inventory.TransferSerializedItem(
                    eps12v.ItemId,
                    target);
                Assert.That(result.Error,
                    Is.EqualTo(
                        InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
                Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(session.CustomPcBuildKit.Revision,
                    Is.EqualTo(buildKitRevision));
            }

            OperationResult<Eps12vPowerCableOperationReceipt> route =
                session.RouteEps12vPowerCable(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.eps12v-build-kit-route-bypass"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.Eps12vPowerCableRevision);
            Assert.That(route.IsFailure, Is.True);
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            OperationResult<CustomPcBuildKitReceipt> placement =
                session.PlaceHeldEps12vPowerCableInCustomPcBuildKit();
            Assert.That(placement.IsSuccess, Is.True);
            inventoryRevision = session.Inventory.Revision;
            buildKitRevision = session.CustomPcBuildKit.Revision;
            OperationResult stagedBypass = session.Inventory.TransferSerializedItem(
                eps12v.ItemId,
                session.WorldFloorContainerId);
            Assert.That(stagedBypass.Error,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.Inventory.TryGetSerializedItem(
                eps12v.ItemId,
                out InventoryItemRecord staged), Is.True);
            Assert.That(staged.ContainerId,
                Is.EqualTo(session.Eps12vPowerCableBuildKitContainerId));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void Eps12vNonupleClaimRejectsOccupiedNinthWithoutPartialClaim()
        {
            GarageStockFlowSession session = CreateIssuedSession(
                out CustomPcBuildOrderRecord workOrder);
            StableId<ContainerIdScope>[] containerIds = Enumerable.Range(1, 9)
                .Select(index => StableId<ContainerIdScope>.Parse(
                    $"inventory.container.eps12v-build-kit-occupied-{index}"))
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
                    "inventory.item.eps12v-build-kit-occupied-peer");
            Assert.That(session.Inventory.ReceiveSerializedItem(
                foreignItemId,
                processor.ProductId,
                containerIds[8],
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
                    containerIds[8]);

            Assert.That(occupied.Error,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitContainerInvalid));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Inventory.TransferSerializedItem(
                foreignItemId,
                session.ShelfContainerId).IsSuccess, Is.True,
                "A rejected nonuple claim must leave every peer unclaimed.");
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
                    containerIds[8]);

            Assert.That(valid.IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(beforeValidClaimRevision + 1));
            Assert.That(valid.Value.Eps12vPowerCableBuildKitContainerId,
                Is.EqualTo(containerIds[8]));
            Assert.That(valid.Value.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageFirstEightBuildKitComponents(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder)
        {
            StageFirstSevenBuildKitComponents(session, workOrder);
            CustomPcBuildKitReceipt atx24Pickup = session.CustomPcBuildKit
                .PickupCanonicalAtx24PowerCable(
                    session.PrototypeAtx24PowerCableBuildKitOperationId,
                    workOrder).Value;
            Assert.That(session.CustomPcBuildKit
                .PlaceCanonicalAtx24PowerCable(atx24Pickup).IsSuccess, Is.True);
        }

        private static CustomPcBuildOrderLineSnapshot CanonicalEps12vPowerCable(
            CustomPcBuildOrderRecord workOrder)
        {
            return workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerCable &&
                        line.PowerCableType ==
                            PowerCableType.ModularEps12v8PinPsuToMotherboard);
        }
    }
}
