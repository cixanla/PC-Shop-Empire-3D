using NUnit.Framework;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class TransportCartTests
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
        public void LargeBoxTransfersHandsCartHandsAndPreservesWorldSnapshot()
        {
            TransportCartProjection cart = CreateCart();
            PhysicalItemProjection item = CreateItem(
                "tests.cart-large-box-001",
                PhysicalCarryProfile.LargeBox,
                new Vector3(1f, 0.5f, 2f));
            Transform handAnchor = CreateTransform("HandAnchor");
            string identity = item.ItemIdValue;
            Vector3 safePosition = item.LastSafePosition;

            Assert.That(item.BeginCarry(handAnchor, 8).IsSuccess, Is.True);
            Assert.That(cart.TryLoad(item, 8).IsSuccess, Is.True);
            Assert.That(cart.Cargo, Is.SameAs(item));
            Assert.That(item.IsMountedOnTransportCart, Is.True);
            Assert.That(item.transform.parent, Is.EqualTo(_root.transform));
            Assert.That(Vector3.Distance(item.transform.position, cart.CargoAnchor.position), Is.LessThan(0.001f));
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.Body.isKinematic, Is.True);
            Assert.That(item.Body.detectCollisions, Is.False);

            var unload = cart.TryUnload(handAnchor, 8);
            Assert.That(unload.IsSuccess, Is.True);
            Assert.That(unload.Value, Is.SameAs(item));
            Assert.That(cart.HasCargo, Is.False);
            Assert.That(item.IsCarried, Is.True);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));

            Pose dropPose = new Pose(new Vector3(3f, 0.5f, 4f), Quaternion.identity);
            Assert.That(item.ReleaseTo(dropPose).IsSuccess, Is.True);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.Body.isKinematic, Is.False);
            Assert.That(item.Body.useGravity, Is.True);
            Assert.That(item.Body.detectCollisions, Is.True);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.LastSafePosition, Is.Not.EqualTo(safePosition));
        }

        [Test]
        public void CartRejectsSmallBoxAndSecondCargoWithoutChangingOwnership()
        {
            TransportCartProjection cart = CreateCart();
            Transform firstHands = CreateTransform("FirstHands");
            PhysicalItemProjection small = CreateItem(
                "tests.cart-small-box",
                PhysicalCarryProfile.SmallBox,
                Vector3.up);
            Assert.That(small.BeginCarry(firstHands, 8).IsSuccess, Is.True);

            var unsupported = cart.TryLoad(small, 8);
            Assert.That(unsupported.IsFailure, Is.True);
            Assert.That(unsupported.Error.Code, Is.EqualTo("cart.load-profile-unsupported"));
            Assert.That(small.IsCarried, Is.True);
            Assert.That(cart.HasCargo, Is.False);
            Assert.That(small.RecoverToLastSafePose().IsSuccess, Is.True);

            PhysicalItemProjection first = CreateItem(
                "tests.cart-large-box-first",
                PhysicalCarryProfile.LargeBox,
                Vector3.up);
            PhysicalItemProjection second = CreateItem(
                "tests.cart-large-box-second",
                PhysicalCarryProfile.LargeBox,
                Vector3.up * 2f);
            Transform secondHands = CreateTransform("SecondHands");
            Assert.That(first.BeginCarry(firstHands, 8).IsSuccess, Is.True);
            Assert.That(cart.TryLoad(first, 8).IsSuccess, Is.True);
            Assert.That(second.BeginCarry(secondHands, 8).IsSuccess, Is.True);

            var occupied = cart.TryLoad(second, 8);
            Assert.That(occupied.IsFailure, Is.True);
            Assert.That(occupied.Error.Code, Is.EqualTo("cart.load-slot-occupied"));
            Assert.That(cart.Cargo, Is.SameAs(first));
            Assert.That(first.IsMountedOnTransportCart, Is.True);
            Assert.That(second.IsCarried, Is.True);
        }

        [Test]
        public void DrivenCartMovesOnFullSupportAndFailsClosedAtObstruction()
        {
            CreateFloor();
            TransportCartProjection cart = CreateCart();
            cart.gameObject.layer = 7;
            foreach (Collider collider in cart.GetComponentsInChildren<Collider>(true))
            {
                collider.gameObject.layer = 7;
            }

            Transform driver = CreateTransform("Driver");
            driver.position = new Vector3(0f, 0f, -1.2f);
            Assert.That(cart.BeginDrive(driver).IsSuccess, Is.True);
            Vector3 initialPosition = cart.transform.position;

            driver.position += Vector3.forward * 0.25f;
            Physics.SyncTransforms();
            Assert.That(cart.TryFollowDriver(1 << 0, 1 << 7).IsSuccess, Is.True);
            Assert.That(cart.transform.position.z, Is.GreaterThan(initialPosition.z));
            Vector3 safePosition = cart.transform.position;

            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "CartMotionBlocker";
            blocker.transform.SetParent(_root.transform);
            blocker.layer = 7;
            blocker.transform.position = cart.transform.position +
                                         (Vector3.forward * 0.65f) +
                                         (Vector3.up * 0.85f);
            blocker.transform.localScale = new Vector3(1.4f, 1.7f, 0.35f);
            driver.position += Vector3.forward;
            Physics.SyncTransforms();

            var blocked = cart.TryFollowDriver(1 << 0, 1 << 7);
            Assert.That(blocked.IsFailure, Is.True);
            Assert.That(blocked.Error.Code, Is.EqualTo("cart.drive-blocked"));
            Assert.That(Vector3.Distance(cart.transform.position, safePosition), Is.LessThan(0.001f));
            Assert.That(cart.LastSafePosition, Is.EqualTo(safePosition));
        }

        [Test]
        public void DrivenCartRequiresAllFourWheelSupports()
        {
            TransportCartProjection cart = CreateCart();
            Transform driver = CreateTransform("Driver");
            driver.position = new Vector3(0f, 0f, -1.2f);
            Assert.That(cart.BeginDrive(driver).IsSuccess, Is.True);
            driver.position += Vector3.forward * 0.2f;
            Physics.SyncTransforms();

            var result = cart.TryFollowDriver(1 << 0, 1 << 7);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("cart.no-support"));
            Assert.That(cart.transform.position, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void DriverCannotGripFromInsideOrBeyondTheBoundedReach()
        {
            TransportCartProjection cart = CreateCart();
            Transform driver = CreateTransform("Driver");
            driver.position = new Vector3(0f, 0f, -0.2f);

            var tooClose = cart.BeginDrive(driver);
            Assert.That(tooClose.IsFailure, Is.True);
            Assert.That(tooClose.Error.Code, Is.EqualTo("cart.driver-too-close"));
            Assert.That(cart.IsDriven, Is.False);

            driver.position = new Vector3(0f, 0f, -3f);
            var tooFar = cart.BeginDrive(driver);
            Assert.That(tooFar.IsFailure, Is.True);
            Assert.That(tooFar.Error.Code, Is.EqualTo("cart.driver-too-far"));
            Assert.That(cart.IsDriven, Is.False);
        }

        private TransportCartProjection CreateCart()
        {
            _root ??= new GameObject("TransportCartTests");
            GameObject cartObject = new GameObject("Cart");
            cartObject.transform.SetParent(_root.transform);
            cartObject.layer = 7;
            BoxCollider collider = cartObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, 0.4f, 0f);
            collider.size = new Vector3(1f, 0.8f, 1.2f);
            Rigidbody body = cartObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            Transform cargoAnchor = new GameObject("CargoAnchor").transform;
            cargoAnchor.SetParent(cartObject.transform, false);
            cargoAnchor.localPosition = new Vector3(0f, 0.8f, 0f);
            TransportCartProjection cart = cartObject.AddComponent<TransportCartProjection>();
            cart.Configure(
                "tests.transport-cart-001",
                "Test Cart",
                body,
                cargoAnchor,
                new Vector3(0.55f, 0.70f, 0.65f),
                new Vector3(0.60f, 0.85f, 0.70f));
            return cart;
        }

        private PhysicalItemProjection CreateItem(
            string identity,
            PhysicalCarryProfile profile,
            Vector3 position)
        {
            _root ??= new GameObject("TransportCartTests");
            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.name = identity;
            itemObject.transform.SetParent(_root.transform);
            itemObject.transform.position = position;
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                identity,
                "Test Cargo",
                body,
                Vector3.one * 0.5f,
                Vector3.zero,
                Vector3.zero,
                profile);
            return item;
        }

        private Transform CreateTransform(string name)
        {
            _root ??= new GameObject("TransportCartTests");
            Transform result = new GameObject(name).transform;
            result.SetParent(_root.transform);
            return result;
        }

        private void CreateFloor()
        {
            _root ??= new GameObject("TransportCartTests");
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(_root.transform);
            floor.transform.position = new Vector3(0f, -0.1f, 0f);
            floor.transform.localScale = new Vector3(10f, 0.2f, 10f);
        }
    }
}
