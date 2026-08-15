using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class GarageStockFlowTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            _root = null;
        }

        [Test]
        public void ArrivedOrderHasVisibleManifestIdentityButNoAuthoritativeStock()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();

            Assert.That(session.Order.Status, Is.EqualTo(PurchaseOrderStatus.Arrived));
            Assert.That(session.Order.Manifest.Intake.SerializedItems.Count, Is.EqualTo(1));
            Assert.That(
                session.Order.Manifest.Intake.SerializedItems[0].ItemId,
                Is.EqualTo(session.ItemId));
            Assert.That(session.TryGetItem(out _), Is.False);
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void AcceptanceAndContainerTransfersPreserveOneStableSerializedItem()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();

            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            AssertLocation(session, session.ReceivingContainerId);
            Assert.That(session.Order.Status, Is.EqualTo(PurchaseOrderStatus.Accepted));
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.EqualTo(1));

            Assert.That(session.TransferItem(session.HandsContainerId).IsSuccess, Is.True);
            AssertLocation(session, session.HandsContainerId);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            AssertLocation(session, session.ShelfContainerId);
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BindingMovesAuthorityBeforeProjectionAndRecoveryReturnsBothToSafeSource()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.RequiresAcceptance, Is.True);
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryPreparePickupTransfer().Error,
                Is.EqualTo(StockProjectionFailures.ParcelSealed));
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            Vector3 safePosition = fixture.Item.LastSafePosition;

            Assert.That(fixture.Binding.TryPreparePickupTransfer().IsSuccess, Is.True);
            AssertLocation(fixture.Session, fixture.Session.HandsContainerId);
            Assert.That(fixture.Item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            OperationResult beginCarry = fixture.Item.BeginCarry(fixture.Anchor, 8);
            Assert.That(beginCarry.IsSuccess, Is.True, beginCarry.IsFailure ? beginCarry.Error.Code : string.Empty);
            Assert.That(fixture.Binding.CommitPreparedTransfer(targetIsWorld: false).IsSuccess, Is.True);

            Assert.That(fixture.Binding.TryPrepareRecoveryTransfer().IsSuccess, Is.True);
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
            Assert.That(fixture.Item.RecoverToLastSafePose().IsSuccess, Is.True);
            Assert.That(fixture.Binding.CommitPreparedTransfer(targetIsWorld: true).IsSuccess, Is.True);

            Assert.That(fixture.Item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(Vector3.Distance(fixture.Item.transform.position, safePosition), Is.LessThan(0.001f));
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
        }

        [Test]
        public void FullShelfRejectsPreparedPlacementWithoutMovingHeldProjectionOrInventory()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            FillShelf(fixture.Session);
            Assert.That(fixture.Binding.TryPreparePickupTransfer().IsSuccess, Is.True);
            OperationResult beginCarry = fixture.Item.BeginCarry(fixture.Anchor, 8);
            Assert.That(beginCarry.IsSuccess, Is.True, beginCarry.IsFailure ? beginCarry.Error.Code : string.Empty);
            Assert.That(fixture.Binding.CommitPreparedTransfer(targetIsWorld: false).IsSuccess, Is.True);
            Vector3 heldPosition = fixture.Item.transform.position;

            GameObject surfaceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surfaceObject.name = "FullShelfSurface";
            surfaceObject.transform.SetParent(_root.transform);
            PlacementSurface surface = surfaceObject.AddComponent<PlacementSurface>();
            surface.Configure("tests.full-shelf", surfaceObject.GetComponent<Collider>(), 0.25f, 90f);
            InventoryPlacementZone zone = surfaceObject.AddComponent<InventoryPlacementZone>();
            zone.Configure(
                GarageStockFlowSession.ShelfContainerIdValue,
                InventoryContainerKind.Shelf,
                "Full Shelf",
                surface);

            OperationResult result = fixture.Binding.TryPreparePlacementTransfer(surface);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(fixture.Binding.HasPreparedTransfer, Is.False);
            Assert.That(fixture.Item.IsCarried, Is.True);
            Assert.That(Vector3.Distance(fixture.Item.transform.position, heldPosition), Is.LessThan(0.001f));
            AssertLocation(fixture.Session, fixture.Session.HandsContainerId);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void AcceptedParcelOpensExactlyOnceWithoutMutatingOrderOrInventory()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Binding.RequiresUnpacking, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);

            Assert.That(fixture.Parcel.IsOpened, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.EqualTo(1));
            Assert.That(fixture.Parcel.SealedVisualRoot.activeSelf, Is.False);
            Assert.That(fixture.Parcel.ProductVisualRoot.activeSelf, Is.True);
            Assert.That(fixture.Parcel.OpenedShellVisualRoot.activeSelf, Is.True);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Session.Inventory.GetTotalQuantity(fixture.Session.ProductId).Value,
                Is.EqualTo(1));
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
        }

        [Test]
        public void ParcelCannotOpenBeforeAcceptanceAndLeavesEveryStateUntouched()
        {
            Fixture fixture = CreateBindingFixture();
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            OperationResult result = fixture.Binding.TryOpenParcel();

            Assert.That(result.Error, Is.EqualTo(StockProjectionFailures.ParcelNotAccepted));
            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.Zero);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Session.TryGetItem(out _), Is.False);
            Assert.That(fixture.Session.Inventory.GetTotalQuantity(fixture.Session.ProductId).Value,
                Is.Zero);
        }

        [Test]
        public void IdentityMismatchKeepsAcceptedParcelSealedAndInventoryInReceiving()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;
            fixture.Binding.Configure(
                fixture.Binding.Runtime,
                fixture.Item,
                "inventory.item.wrong-projection-001");

            OperationResult result = fixture.Binding.TryOpenParcel();

            Assert.That(result.Error, Is.EqualTo(StockProjectionFailures.IdentityMismatch));
            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.Zero);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
        }

        [Test]
        public void AcceptedItemOutsideReceivingCannotRevealSealedParcel()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Session.TransferItem(fixture.Session.HandsContainerId).IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            OperationResult result = fixture.Binding.TryOpenParcel();

            Assert.That(result.Error, Is.EqualTo(StockProjectionFailures.ParcelLocationMismatch));
            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.Zero);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            AssertLocation(fixture.Session, fixture.Session.HandsContainerId);
        }

        private Fixture CreateBindingFixture()
        {
            _root = new GameObject("StockFlowTestRoot");
            Transform world = new GameObject("World").transform;
            world.SetParent(_root.transform);
            Transform anchor = new GameObject("CarryAnchor").transform;
            anchor.SetParent(_root.transform);

            GameObject itemObject = new GameObject("BoundDeliveryItem");
            itemObject.name = "BoundDeliveryItem";
            itemObject.transform.SetParent(world);
            itemObject.transform.position = Vector3.up;
            GameObject sealedVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sealedVisual.name = "SealedParcelVisual";
            sealedVisual.transform.SetParent(itemObject.transform, false);
            sealedVisual.transform.localScale = Vector3.one;
            GameObject productVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            productVisual.name = "ProductVisual";
            productVisual.transform.SetParent(itemObject.transform, false);
            productVisual.transform.localScale = Vector3.one * 0.8f;
            GameObject openedShell = new GameObject("OpenedParcelShell");
            openedShell.transform.SetParent(world, false);
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                GarageStockFlowSession.ItemInstanceIdValue,
                GarageStockFlowSession.ProductDisplayName,
                body,
                Vector3.one * 0.5f,
                Vector3.zero,
                Vector3.zero);
            DeliveryParcelProjection parcel = itemObject.AddComponent<DeliveryParcelProjection>();
            parcel.Configure(item, sealedVisual, productVisual, openedShell);
            InventoryItemWorldBinding binding = itemObject.AddComponent<InventoryItemWorldBinding>();
            GarageStockFlowRuntime runtime = _root.AddComponent<GarageStockFlowRuntime>();
            runtime.Configure(binding, null, null, null, null, null);
            GarageStockFlowSession session = runtime.EnsureInitialized();
            return new Fixture(session, binding, item, parcel, anchor);
        }

        private static void FillShelf(GarageStockFlowSession session)
        {
            for (int index = 0; index < 8; index++)
            {
                Assert.That(session.Inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse($"inventory.item.shelf-filler-{index:00}"),
                    session.ProductId,
                    session.ShelfContainerId,
                    InventoryCondition.New).IsSuccess,
                    Is.True);
            }
        }

        private static void AssertLocation(
            GarageStockFlowSession session,
            StableId<ContainerIdScope> expectedContainer)
        {
            Assert.That(session.TryGetItem(out InventoryItemRecord item), Is.True);
            Assert.That(item.Id, Is.EqualTo(session.ItemId));
            Assert.That(item.ContainerId, Is.EqualTo(expectedContainer));
        }

        private readonly struct Fixture
        {
            public Fixture(
                GarageStockFlowSession session,
                InventoryItemWorldBinding binding,
                PhysicalItemProjection item,
                DeliveryParcelProjection parcel,
                Transform anchor)
            {
                Session = session;
                Binding = binding;
                Item = item;
                Parcel = parcel;
                Anchor = anchor;
            }

            public GarageStockFlowSession Session { get; }

            public InventoryItemWorldBinding Binding { get; }

            public PhysicalItemProjection Item { get; }

            public DeliveryParcelProjection Parcel { get; }

            public Transform Anchor { get; }
        }
    }
}
