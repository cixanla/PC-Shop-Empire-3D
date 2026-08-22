using NUnit.Framework;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class PhysicalCarryTests
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
        public void PickupAndDropPreserveTheSameItemAndRestorePhysics()
        {
            _root = new GameObject("TestRoot");
            Transform worldParent = new GameObject("WorldParent").transform;
            worldParent.SetParent(_root.transform);
            Transform anchor = new GameObject("CarryAnchor").transform;
            anchor.SetParent(_root.transform);

            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.transform.SetParent(worldParent);
            itemObject.layer = 7;
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            body.useGravity = true;
            body.isKinematic = false;
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "tests.item-001",
                "Test Package",
                body,
                Vector3.one * 0.5f,
                Vector3.zero,
                Vector3.zero);
            string identityBefore = item.ItemIdValue;

            Assert.That(item.BeginCarry(anchor, 8).IsSuccess, Is.True);
            Assert.That(item.transform.parent, Is.EqualTo(anchor));
            Assert.That(body.isKinematic, Is.True);
            Assert.That(body.useGravity, Is.False);
            Assert.That(body.detectCollisions, Is.False);
            Assert.That(item.GetComponent<Collider>().enabled, Is.False);

            Pose dropPose = new Pose(new Vector3(2f, 0.5f, 3f), Quaternion.Euler(0f, 45f, 0f));
            Assert.That(item.ReleaseTo(dropPose).IsSuccess, Is.True);
            Assert.That(item.transform.parent, Is.EqualTo(worldParent));
            Assert.That(item.transform.position, Is.EqualTo(dropPose.position));
            Assert.That(body.isKinematic, Is.False);
            Assert.That(body.useGravity, Is.True);
            Assert.That(body.detectCollisions, Is.True);
            Assert.That(item.GetComponent<Collider>().enabled, Is.True);
            Assert.That(item.gameObject.layer, Is.EqualTo(7));
            Assert.That(item.ItemIdValue, Is.EqualTo(identityBefore));
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
        }

        [Test]
        public void ACarriedItemCannotBePickedUpTwice()
        {
            _root = new GameObject("TestRoot");
            Transform anchor = new GameObject("CarryAnchor").transform;
            anchor.SetParent(_root.transform);
            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.transform.SetParent(_root.transform);
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "tests.item-002",
                "Test Package",
                body,
                Vector3.one * 0.5f,
                Vector3.zero,
                Vector3.zero);

            Assert.That(item.BeginCarry(anchor, 8).IsSuccess, Is.True);
            Assert.That(item.BeginCarry(anchor, 8).Error.Code, Is.EqualTo("pickup.target-unavailable"));
        }

        [Test]
        public void CarryProfilesKeepSmallBoxUnrestrictedAndBoundLargeBoxCost()
        {
            PhysicalCarryProfileDefinition small = PhysicalCarryProfileRules.Resolve(
                PhysicalCarryProfile.SmallBox);
            PhysicalCarryProfileDefinition large = PhysicalCarryProfileRules.Resolve(
                PhysicalCarryProfile.LargeBox);

            Assert.That(small.MovementSpeedMultiplier, Is.EqualTo(1f));
            Assert.That(small.FieldOfViewPenalty, Is.Zero);
            Assert.That(small.AllowsSprint, Is.True);
            Assert.That(small.SupportsPlacement, Is.True);

            Assert.That(
                large.MovementSpeedMultiplier,
                Is.EqualTo(PhysicalCarryProfileRules.LargeBoxMovementSpeedMultiplier));
            Assert.That(
                large.MovementSpeedMultiplier,
                Is.InRange(
                    PhysicalCarryProfileRules.MinimumMovementSpeedMultiplier,
                    PhysicalCarryProfileRules.MaximumMovementSpeedMultiplier));
            Assert.That(
                large.FieldOfViewPenalty,
                Is.EqualTo(PhysicalCarryProfileRules.LargeBoxFieldOfViewPenalty));
            Assert.That(
                large.FieldOfViewPenalty,
                Is.InRange(0f, PhysicalCarryProfileRules.MaximumFieldOfViewPenalty));
            Assert.That(large.AllowsSprint, Is.False);
            Assert.That(large.SupportsPlacement, Is.False);
        }

        [Test]
        public void LargeBoxProfileAndIdentitySurviveCarryAndSafeRelease()
        {
            PhysicalItemProjection item = CreateItem(
                "tests.large-item",
                new Vector3(0f, 1f, 0f),
                PhysicalCarryProfile.LargeBox);
            Transform anchor = new GameObject("Anchor").transform;
            anchor.SetParent(_root.transform);
            string identity = item.ItemIdValue;

            Assert.That(item.BeginCarry(anchor, 8).IsSuccess, Is.True);
            Assert.That(item.CarryProfile, Is.EqualTo(PhysicalCarryProfile.LargeBox));
            Assert.That(item.SupportsPlacement, Is.False);
            Assert.That(item.ReleaseTo(new Pose(Vector3.up, Quaternion.identity)).IsSuccess, Is.True);
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.CarryProfile, Is.EqualTo(PhysicalCarryProfile.LargeBox));
        }

        [Test]
        public void NonUnitScaleIsRejectedBeforePhysicsStateChanges()
        {
            PhysicalItemProjection item = CreateItem("tests.item-scaled", Vector3.zero);
            item.transform.localScale = Vector3.one * 1.5f;
            Rigidbody body = item.Body;
            Transform anchor = new GameObject("Anchor").transform;
            anchor.SetParent(_root.transform);

            var result = item.BeginCarry(anchor, 8);

            Assert.That(result.Error.Code, Is.EqualTo("pickup.invalid-scale"));
            Assert.That(body.isKinematic, Is.False);
            Assert.That(body.detectCollisions, Is.True);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
        }

        [Test]
        public void RecoveryReturnsTheSameCarriedItemToItsLastSafePose()
        {
            PhysicalItemProjection item = CreateItem("tests.item-recovery", new Vector3(1f, 2f, 3f));
            Transform anchor = new GameObject("Anchor").transform;
            anchor.SetParent(_root.transform);
            Vector3 safePosition = item.LastSafePosition;
            string identity = item.ItemIdValue;

            Assert.That(item.BeginCarry(anchor, 8).IsSuccess, Is.True);
            anchor.position = new Vector3(20f, 20f, 20f);
            Assert.That(item.RecoverToLastSafePose().IsSuccess, Is.True);

            Assert.That(item.transform.position, Is.EqualTo(safePosition));
            Assert.That(item.ItemIdValue, Is.EqualTo(identity));
            Assert.That(item.IsCarried, Is.False);
            Assert.That(item.Body.isKinematic, Is.False);
        }

        [Test]
        public void StableWorldPoseSynchronizationNeverPretendsAWorldItemIsHeld()
        {
            PhysicalItemProjection item = CreateItem(
                "tests.item-stable-world-sync",
                new Vector3(1f, 2f, 3f),
                PhysicalCarryProfile.PcComponent);
            Transform anchor = new GameObject("Anchor").transform;
            anchor.SetParent(_root.transform);
            Pose seatedPose = new Pose(
                new Vector3(2f, 1f, 4f),
                Quaternion.Euler(0f, 180f, 0f));
            Pose correctedPose = new Pose(
                new Vector3(2.01f, 1.02f, 4.03f),
                Quaternion.Euler(0f, 0f, 180f));

            Assert.That(item.BeginCarry(anchor, 8).IsSuccess, Is.True);
            Assert.That(item.PlaceAt(seatedPose).IsSuccess, Is.True);
            Assert.That(item.SynchronizeStableWorldPose(correctedPose).IsSuccess,
                Is.True);
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(item.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                item.transform.position,
                correctedPose.position), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(
                item.transform.rotation,
                correctedPose.rotation), Is.LessThan(0.01f));
            Assert.That(Vector3.Distance(
                item.LastSafePosition,
                correctedPose.position), Is.LessThan(0.0001f));

            item.Body.isKinematic = false;
            item.Body.useGravity = true;
            Assert.That(item.BeginCarry(anchor, 8).IsSuccess, Is.True);
            Assert.That(item.ReleaseTo(seatedPose).IsSuccess, Is.True);
            Pose beforeRejectedSync = new Pose(
                item.transform.position,
                item.transform.rotation);
            Assert.That(item.SynchronizeStableWorldPose(correctedPose).Error.Code,
                Is.EqualTo("recovery.stable-world-pose-unavailable"));
            Assert.That(item.transform.position,
                Is.EqualTo(beforeRejectedSync.position));
            Assert.That(item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
        }

        [Test]
        public void SafeDropReportsNoSupportAndBlockedWithoutReleasingTheItem()
        {
            PhysicalItemProjection item = CreateItem("tests.item-drop", new Vector3(0f, 1f, 0f));
            Transform origin = new GameObject("Origin").transform;
            origin.SetParent(_root.transform);
            origin.position = new Vector3(0f, 1.6f, 0f);
            Transform anchor = new GameObject("Anchor").transform;
            anchor.SetParent(_root.transform);
            Assert.That(item.BeginCarry(anchor, 8).IsSuccess, Is.True);

            var noSupport = SafeDropSolver.FindPose(origin, item, 0, 1 << 0);
            Assert.That(noSupport.Error.Code, Is.EqualTo("drop.no-support"));
            Assert.That(item.IsCarried, Is.True);

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(_root.transform);
            floor.transform.SetPositionAndRotation(new Vector3(0f, -0.1f, 1f), Quaternion.identity);
            floor.transform.localScale = new Vector3(4f, 0.2f, 4f);
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "DropBlocker";
            blocker.transform.SetParent(_root.transform);
            blocker.transform.SetPositionAndRotation(new Vector3(0f, 0.5f, 0.9f), Quaternion.identity);
            blocker.transform.localScale = new Vector3(1.4f, 1f, 1.6f);
            blocker.layer = 7;
            Physics.SyncTransforms();

            var blocked = SafeDropSolver.FindPose(origin, item, 1 << 0, (1 << 0) | (1 << 7));
            Assert.That(blocked.Error.Code, Is.EqualTo("drop.blocked"));
            Assert.That(item.IsCarried, Is.True);
        }

        [Test]
        public void ResolverFindsVisibleItemAndRejectsOccludedItem()
        {
            _root = new GameObject("TestRoot");
            Transform playerRoot = new GameObject("PlayerRoot").transform;
            playerRoot.SetParent(_root.transform);
            Transform origin = new GameObject("Origin").transform;
            origin.SetParent(playerRoot);
            PhysicalInteractionResolver resolver = _root.AddComponent<PhysicalInteractionResolver>();
            resolver.Configure(origin, playerRoot, 2f, 0f, 1 << 7);

            PhysicalItemProjection item = CreateItem("tests.item-target", new Vector3(0f, 0f, 1.4f));
            item.gameObject.layer = 7;
            foreach (Collider collider in item.GetComponentsInChildren<Collider>())
            {
                collider.gameObject.layer = 7;
            }

            Physics.SyncTransforms();
            Assert.That(resolver.Resolve().Value, Is.SameAs(item));

            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.transform.SetParent(_root.transform);
            blocker.transform.position = new Vector3(0f, 0f, 0.65f);
            blocker.transform.localScale = new Vector3(0.8f, 0.8f, 0.2f);
            blocker.layer = 7;
            Physics.SyncTransforms();
            Assert.That(resolver.Resolve().Error.Code, Is.EqualTo("interaction.no-target"));
        }

        private PhysicalItemProjection CreateItem(
            string identity,
            Vector3 position,
            PhysicalCarryProfile carryProfile = PhysicalCarryProfile.SmallBox)
        {
            _root ??= new GameObject("TestRoot");
            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.transform.SetParent(_root.transform);
            itemObject.transform.position = position;
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                identity,
                "Test Package",
                body,
                Vector3.one * 0.5f,
                Vector3.zero,
                Vector3.zero,
                carryProfile);
            return item;
        }
    }
}
