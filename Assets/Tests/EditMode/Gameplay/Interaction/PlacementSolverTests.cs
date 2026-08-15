using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class PlacementSolverTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _objects.Count - 1; index >= 0; index--)
            {
                Object.DestroyImmediate(_objects[index]);
            }

            _objects.Clear();
        }

        [Test]
        public void MarkedSurfaceProducesGridAndNinetyDegreeSnappedPose()
        {
            Transform origin = CreateOrigin(46f);
            PlacementSurface surface = CreateSurface();
            PhysicalItemProjection item = CreateCarriedItem();
            Physics.SyncTransforms();

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                origin,
                item,
                1 << surface.gameObject.layer,
                0);

            Assert.That(evaluation.IsValid, Is.True);
            Assert.That(evaluation.Status, Is.EqualTo(PlacementStatus.Valid));
            Assert.That(evaluation.Pose.rotation.eulerAngles.y, Is.EqualTo(90f).Within(0.01f));
            Vector3 localPoint = surface.transform.InverseTransformPoint(evaluation.Pose.position);
            Assert.That(localPoint.x / surface.GridSize, Is.EqualTo(Mathf.Round(localPoint.x / surface.GridSize)).Within(0.001f));
            Assert.That(localPoint.z / surface.GridSize, Is.EqualTo(Mathf.Round(localPoint.z / surface.GridSize)).Within(0.001f));
        }

        [Test]
        public void ClockwiseQuarterTurnsAreDeterministicAndNormalizeAfterFourSteps()
        {
            Transform origin = CreateOrigin(0f);
            PlacementSurface surface = CreateSurface();
            PhysicalItemProjection item = CreateCarriedItem();
            Physics.SyncTransforms();

            PlacementEvaluation initial = PlacementSolver.Evaluate(
                origin,
                item,
                1 << surface.gameObject.layer,
                0,
                0);
            PlacementEvaluation clockwise = PlacementSolver.Evaluate(
                origin,
                item,
                1 << surface.gameObject.layer,
                0,
                1);
            PlacementEvaluation wrapped = PlacementSolver.Evaluate(
                origin,
                item,
                1 << surface.gameObject.layer,
                0,
                5);

            Assert.That(initial.IsValid, Is.True);
            Assert.That(clockwise.IsValid, Is.True);
            Assert.That(wrapped.IsValid, Is.True);
            Assert.That(
                Mathf.DeltaAngle(initial.Pose.rotation.eulerAngles.y, clockwise.Pose.rotation.eulerAngles.y),
                Is.EqualTo(90f).Within(0.01f));
            Assert.That(
                Quaternion.Angle(clockwise.Pose.rotation, wrapped.Pose.rotation),
                Is.LessThan(0.01f));
            Assert.That(Vector3.Distance(clockwise.Pose.position, wrapped.Pose.position), Is.LessThan(0.001f));
        }

        [Test]
        public void UnmarkedFloorIsRejectedWithVisibleCandidatePose()
        {
            Transform origin = CreateOrigin(0f);
            GameObject floor = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            floor.name = "UnmarkedFloor";
            floor.transform.SetPositionAndRotation(new Vector3(0f, 0f, 1f), Quaternion.identity);
            floor.transform.localScale = new Vector3(4f, 0.1f, 4f);
            PhysicalItemProjection item = CreateCarriedItem();
            Physics.SyncTransforms();

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                origin,
                item,
                1 << floor.layer,
                0);

            Assert.That(evaluation.IsValid, Is.False);
            Assert.That(evaluation.HasPose, Is.True);
            Assert.That(evaluation.Status, Is.EqualTo(PlacementStatus.SurfaceNotAllowed));
            Assert.That(evaluation.FailureCode, Is.EqualTo("placement.surface-not-allowed"));
        }

        [Test]
        public void ObstructionLayerRejectsEveryCandidateWithoutReleasingItem()
        {
            Transform origin = CreateOrigin(0f);
            PlacementSurface surface = CreateSurface();
            PhysicalItemProjection item = CreateCarriedItem();
            GameObject blocker = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            blocker.name = "PlacementBlocker";
            blocker.layer = 7;
            blocker.transform.SetPositionAndRotation(new Vector3(0f, 0.4f, 1f), Quaternion.identity);
            blocker.transform.localScale = new Vector3(3f, 0.8f, 3f);
            Physics.SyncTransforms();

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                origin,
                item,
                1 << surface.gameObject.layer,
                1 << blocker.layer);

            Assert.That(evaluation.IsValid, Is.False);
            Assert.That(evaluation.Status, Is.EqualTo(PlacementStatus.Blocked));
            Assert.That(item.IsCarried, Is.True);
        }

        [Test]
        public void StableSmallBoxProducesCenteredStackPoseAndLocksItsSupport()
        {
            Transform origin = CreateOrigin(0f);
            PhysicalItemProjection support = CreateStackSupport(stable: true);
            PhysicalItemProjection item = CreateCarriedItem();
            int stackLayerMask = 1 << support.gameObject.layer;
            Physics.SyncTransforms();

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                origin,
                item,
                0,
                stackLayerMask,
                0,
                stackLayerMask);

            Assert.That(evaluation.IsValid, Is.True);
            Assert.That(evaluation.StackSupport, Is.SameAs(support));
            Assert.That(evaluation.Pose.position.x, Is.EqualTo(support.transform.position.x).Within(0.001f));
            Assert.That(evaluation.Pose.position.z, Is.EqualTo(support.transform.position.z).Within(0.001f));
            Assert.That(item.PlaceAt(evaluation.Pose, evaluation.StackSupport).IsSuccess, Is.True);
            Assert.That(item.StackSupport, Is.SameAs(support));
            Assert.That(support.StackedItem, Is.SameAs(item));
            Assert.That(item.Body.isKinematic, Is.True);

            GameObject baseCarryAnchor = Track(new GameObject("BaseCarryAnchor"));
            var blockedPickup = support.BeginCarry(baseCarryAnchor.transform, 8);
            Assert.That(blockedPickup.IsFailure, Is.True);
            Assert.That(blockedPickup.Error.Code, Is.EqualTo("pickup.stack-occupied"));

            GameObject topCarryAnchor = Track(new GameObject("TopCarryAnchor"));
            Assert.That(item.BeginCarry(topCarryAnchor.transform, 8).IsSuccess, Is.True);
            Assert.That(item.StackSupport, Is.Null);
            Assert.That(support.StackedItem, Is.Null);
        }

        [Test]
        public void RotatedRectangularBoxWithoutFullFootprintSupportFailsClosed()
        {
            Transform origin = CreateOrigin(0f);
            PhysicalItemProjection support = CreateStackSupport(stable: true);
            PhysicalItemProjection item = CreateCarriedItem();
            int stackLayerMask = 1 << support.gameObject.layer;
            Physics.SyncTransforms();

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                origin,
                item,
                0,
                stackLayerMask,
                1,
                stackLayerMask);

            Assert.That(evaluation.IsValid, Is.False);
            Assert.That(evaluation.Status, Is.EqualTo(PlacementStatus.OutsideSurface));
            Assert.That(evaluation.StackSupport, Is.Null);
            Assert.That(item.IsCarried, Is.True);
        }

        [Test]
        public void DynamicSmallBoxCannotBecomeStackSupport()
        {
            Transform origin = CreateOrigin(0f);
            PhysicalItemProjection support = CreateStackSupport(stable: false);
            PhysicalItemProjection item = CreateCarriedItem();
            int stackLayerMask = 1 << support.gameObject.layer;
            Physics.SyncTransforms();

            PlacementEvaluation evaluation = PlacementSolver.Evaluate(
                origin,
                item,
                0,
                stackLayerMask,
                0,
                stackLayerMask);

            Assert.That(evaluation.IsValid, Is.False);
            Assert.That(evaluation.Status, Is.EqualTo(PlacementStatus.StackSupportUnavailable));
            Assert.That(evaluation.FailureCode, Is.EqualTo("placement.stack-support-unavailable"));
            Assert.That(item.IsCarried, Is.True);
        }

        private Transform CreateOrigin(float yaw)
        {
            GameObject origin = Track(new GameObject("PlacementOrigin"));
            origin.transform.SetPositionAndRotation(
                new Vector3(0f, 0.05f, 0f),
                Quaternion.Euler(0f, yaw, 0f));
            return origin.transform;
        }

        private PlacementSurface CreateSurface()
        {
            GameObject surfaceObject = Track(GameObject.CreatePrimitive(PrimitiveType.Cube));
            surfaceObject.name = "PlacementSurface";
            surfaceObject.transform.SetPositionAndRotation(new Vector3(0f, 0f, 1f), Quaternion.identity);
            surfaceObject.transform.localScale = new Vector3(4f, 0.1f, 4f);
            PlacementSurface surface = surfaceObject.AddComponent<PlacementSurface>();
            surface.Configure(
                "tests.stock-surface",
                surfaceObject.GetComponent<Collider>(),
                0.25f,
                90f);
            return surface;
        }

        private PhysicalItemProjection CreateCarriedItem()
        {
            GameObject itemObject = Track(new GameObject("PlacementItem"));
            itemObject.AddComponent<BoxCollider>();
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "tests.placement-item",
                "Placement Box",
                body,
                new Vector3(0.35f, 0.225f, 0.25f),
                Vector3.zero,
                Vector3.zero);
            GameObject anchor = Track(new GameObject("CarryAnchor"));
            Assert.That(item.BeginCarry(anchor.transform, 8).IsSuccess, Is.True);
            return item;
        }

        private PhysicalItemProjection CreateStackSupport(bool stable)
        {
            GameObject supportObject = Track(new GameObject("StackSupport"));
            supportObject.layer = 7;
            supportObject.transform.position = new Vector3(0f, 0.275f, 1.15f);
            BoxCollider collider = supportObject.AddComponent<BoxCollider>();
            collider.size = new Vector3(0.7f, 0.45f, 0.5f);
            Rigidbody body = supportObject.AddComponent<Rigidbody>();
            body.useGravity = !stable;
            body.isKinematic = stable;
            PhysicalItemProjection support = supportObject.AddComponent<PhysicalItemProjection>();
            support.Configure(
                stable ? "tests.stack-support-stable" : "tests.stack-support-dynamic",
                "Stack Support",
                body,
                new Vector3(0.35f, 0.225f, 0.25f),
                Vector3.zero,
                Vector3.zero);
            return support;
        }

        private GameObject Track(GameObject gameObject)
        {
            _objects.Add(gameObject);
            return gameObject;
        }
    }
}
