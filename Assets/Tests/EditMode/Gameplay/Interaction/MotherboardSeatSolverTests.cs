using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class MotherboardSeatSolverTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _objects.Count - 1; index >= 0; index--)
            {
                if (_objects[index] != null)
                {
                    Object.DestroyImmediate(_objects[index]);
                }
            }

            _objects.Clear();
        }

        [Test]
        public void ExactAuthoredPoseIsSharedByValidPreviewAndCommit()
        {
            Fixture fixture = CreateFixture();

            MotherboardSeatEvaluation result = fixture.Evaluate();

            Assert.That(result.Status, Is.EqualTo(MotherboardSeatStatus.Valid));
            Assert.That(result.IsValid, Is.True);
            Assert.That(result.HasPose, Is.True);
            Assert.That(result.Pose.position, Is.EqualTo(fixture.Snap.position));
            Assert.That(Quaternion.Angle(result.Pose.rotation, fixture.Snap.rotation),
                Is.LessThan(0.001f));
        }

        [Test]
        public void PublicFailureCodesMatchIssue53SeatContract()
        {
            Assert.That(Evaluation(MotherboardSeatStatus.ContextMissing).FailureCode,
                Is.EqualTo("assembly-seat.context-missing"));
            Assert.That(Evaluation(MotherboardSeatStatus.AuthorityBlocked).FailureCode,
                Is.EqualTo("assembly-seat.authority-blocked"));
            Assert.That(Evaluation(MotherboardSeatStatus.OutOfRange).FailureCode,
                Is.EqualTo("assembly-seat.out-of-range"));
            Assert.That(Evaluation(MotherboardSeatStatus.NotFocused).FailureCode,
                Is.EqualTo("assembly-seat.focus-missing"));
            Assert.That(Evaluation(MotherboardSeatStatus.LineOfSightBlocked).FailureCode,
                Is.EqualTo("assembly-seat.line-of-sight-blocked"));
            Assert.That(Evaluation(MotherboardSeatStatus.Obstructed).FailureCode,
                Is.EqualTo("assembly-seat.obstructed"));
            Assert.That(Evaluation(MotherboardSeatStatus.Paused).FailureCode,
                Is.EqualTo("assembly-seat.paused"));
            Assert.That(Evaluation(MotherboardSeatStatus.OrientationInvalid).FailureCode,
                Is.EqualTo("assembly-seat.orientation-invalid"));
            Assert.That(Evaluation(MotherboardSeatStatus.Unsupported).FailureCode,
                Is.EqualTo("assembly-seat.unsupported"));
            Assert.That(Evaluation(MotherboardSeatStatus.Uninitialized).FailureCode, Is.Empty);
            Assert.That(Evaluation(MotherboardSeatStatus.Valid).FailureCode, Is.Empty);
        }

        [Test]
        public void DefaultResetAndDisabledSeatConfigurationFailClosed()
        {
            MotherboardSeatEvaluation empty = default;
            Assert.That(empty.Status, Is.EqualTo(MotherboardSeatStatus.Uninitialized));
            Assert.That(empty.HasPose, Is.False);
            Assert.That(empty.IsValid, Is.False);

            Fixture fixture = CreateFixture();
            GameObject projectionObject = CreateObject("SeatProjection");
            MotherboardSeatProjection projection =
                projectionObject.AddComponent<MotherboardSeatProjection>();
            projection.Configure(
                fixture.Snap,
                fixture.Focus,
                fixture.Support,
                fixture.Player,
                null,
                null,
                null,
                null);

            Assert.That(projection.IsConfigured, Is.True);
            Assert.That(projection.LastEvaluation.Status,
                Is.EqualTo(MotherboardSeatStatus.Uninitialized));
            Assert.That(projection.LastEvaluation.IsValid, Is.False);

            fixture.Support.enabled = false;
            Assert.That(projection.IsConfigured, Is.False);
        }

        [Test]
        public void GeometricRangeAndFocusGatesRejectBeforeValidPose()
        {
            Fixture fixture = CreateFixture();
            fixture.Origin.position = new Vector3(0f, 1f, -3f);
            Physics.SyncTransforms();

            MotherboardSeatEvaluation outOfRange = fixture.Evaluate();
            Assert.That(outOfRange.Status, Is.EqualTo(MotherboardSeatStatus.OutOfRange));
            Assert.That(outOfRange.IsValid, Is.False);

            fixture.Origin.position = new Vector3(0f, 1f, 0f);
            fixture.Origin.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            Physics.SyncTransforms();

            MotherboardSeatEvaluation notFocused = fixture.Evaluate();
            Assert.That(notFocused.Status, Is.EqualTo(MotherboardSeatStatus.NotFocused));
            Assert.That(notFocused.IsValid, Is.False);
        }

        [Test]
        public void PauseAuthorityAndKeyedRotationFailClosedWithoutChangingSnapPose()
        {
            Fixture fixture = CreateFixture();

            MotherboardSeatEvaluation paused = fixture.Evaluate(paused: true);
            MotherboardSeatEvaluation authority = fixture.Evaluate(authorityAvailable: false);
            MotherboardSeatEvaluation rotated = fixture.Evaluate(clockwiseQuarterTurns: 1);

            Assert.That(paused.Status, Is.EqualTo(MotherboardSeatStatus.Paused));
            Assert.That(authority.Status, Is.EqualTo(MotherboardSeatStatus.AuthorityBlocked));
            Assert.That(rotated.Status,
                Is.EqualTo(MotherboardSeatStatus.OrientationInvalid));
            Assert.That(paused.Pose.position, Is.EqualTo(fixture.Snap.position));
            Assert.That(authority.Pose.position, Is.EqualTo(fixture.Snap.position));
            Assert.That(rotated.Pose.position, Is.EqualTo(fixture.Snap.position));
        }

        [Test]
        public void MissingTrayAndBlockedLineOfSightAreRejectedDeterministically()
        {
            Fixture fixture = CreateFixture();
            fixture.Support.enabled = false;
            Physics.SyncTransforms();
            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(MotherboardSeatStatus.Unsupported));

            fixture.Support.enabled = true;
            GameObject blocker = CreateCube(
                "LineOfSightBlocker",
                Vector3.Lerp(
                    fixture.Origin.position,
                    fixture.Focus.bounds.center,
                    0.55f),
                new Vector3(0.12f, 0.12f, 0.04f));
            Physics.SyncTransforms();

            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(MotherboardSeatStatus.LineOfSightBlocked));
            Assert.That(blocker, Is.Not.Null);
        }

        [Test]
        public void FinalSeatAndInsertionVolumeRejectForeignCollider()
        {
            Fixture fixture = CreateFixture();
            CreateCube(
                "SeatObstruction",
                fixture.Snap.position + new Vector3(0.085f, 0.07f, 0f),
                new Vector3(0.035f, 0.035f, 0.035f));
            Physics.SyncTransforms();

            MotherboardSeatEvaluation result = fixture.Evaluate();

            Assert.That(result.Status, Is.EqualTo(MotherboardSeatStatus.Obstructed));
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.HasPose, Is.True);
        }

        [Test]
        public void FinalSeatRejectsForeignColliderAtFullAuthoredEdge()
        {
            Fixture fixture = CreateFixture();
            CreateCube(
                "SeatEdgeObstruction",
                fixture.Snap.position + new Vector3(0.105f, 0f, 0f),
                new Vector3(0.02f, 0.02f, 0.02f));
            Physics.SyncTransforms();

            MotherboardSeatEvaluation result = fixture.Evaluate();

            Assert.That(result.Status, Is.EqualTo(MotherboardSeatStatus.Obstructed));
            Assert.That(result.IsValid, Is.False);
            Assert.That(result.HasPose, Is.True);
        }

        private Fixture CreateFixture()
        {
            GameObject player = CreateObject("PlayerRoot");
            GameObject originObject = CreateObject("InteractionOrigin");
            originObject.transform.SetParent(player.transform, false);
            originObject.transform.position = new Vector3(0f, 1f, 0f);

            GameObject snapObject = CreateObject("SnapAnchor");
            snapObject.transform.SetPositionAndRotation(
                new Vector3(0f, 1f, 1f),
                Quaternion.Euler(0f, 180f, 0f));
            GameObject focusObject = CreateCube(
                "Focus",
                new Vector3(0f, 0.82f, 0.95f),
                new Vector3(0.22f, 0.10f, 0.02f));
            GameObject supportObject = CreateCube(
                "Support",
                new Vector3(0f, 1f, 1.035f),
                new Vector3(0.28f, 0.28f, 0.02f));

            GameObject motherboardObject = CreateCube(
                "Motherboard",
                new Vector3(3f, 1f, 0f),
                new Vector3(0.20f, 0.20f, 0.04f));
            Rigidbody body = motherboardObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection motherboard =
                motherboardObject.AddComponent<PhysicalItemProjection>();
            motherboard.Configure(
                "tests.physical-item.motherboard-001",
                "Test Motherboard",
                body,
                new Vector3(0.50f, 0.50f, 0.50f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.PcComponent);

            originObject.transform.rotation = Quaternion.LookRotation(
                focusObject.GetComponent<Collider>().bounds.center -
                originObject.transform.position,
                Vector3.up);
            Physics.SyncTransforms();
            return new Fixture(
                originObject.transform,
                player.transform,
                motherboard,
                snapObject.transform,
                focusObject.GetComponent<Collider>(),
                supportObject.GetComponent<Collider>());
        }

        private static MotherboardSeatEvaluation Evaluation(MotherboardSeatStatus status)
        {
            return new MotherboardSeatEvaluation(status, default, false);
        }

        private GameObject CreateObject(string name)
        {
            var gameObject = new GameObject(name);
            _objects.Add(gameObject);
            return gameObject;
        }

        private GameObject CreateCube(string name, Vector3 position, Vector3 scale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetPositionAndRotation(position, Quaternion.identity);
            cube.transform.localScale = scale;
            _objects.Add(cube);
            return cube;
        }

        private readonly struct Fixture
        {
            public Fixture(
                Transform origin,
                Transform player,
                PhysicalItemProjection motherboard,
                Transform snap,
                Collider focus,
                Collider support)
            {
                Origin = origin;
                Player = player;
                Motherboard = motherboard;
                Snap = snap;
                Focus = focus;
                Support = support;
            }

            public Transform Origin { get; }

            public Transform Player { get; }

            public PhysicalItemProjection Motherboard { get; }

            public Transform Snap { get; }

            public Collider Focus { get; }

            public Collider Support { get; }

            public MotherboardSeatEvaluation Evaluate(
                int clockwiseQuarterTurns = 0,
                bool paused = false,
                bool authorityAvailable = true)
            {
                return MotherboardSeatSolver.Evaluate(
                    Origin,
                    Player,
                    Motherboard,
                    Snap,
                    Focus,
                    Support,
                    1 << 0,
                    2f,
                    0f,
                    clockwiseQuarterTurns,
                    paused,
                    authorityAvailable);
            }
        }
    }
}
