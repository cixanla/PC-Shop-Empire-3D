using System;
using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class Atx24PowerCableRouteSolverTests
    {
        private readonly List<GameObject> _objects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int index = _objects.Count - 1; index >= 0; index--)
            {
                if (_objects[index] != null)
                {
                    UnityEngine.Object.DestroyImmediate(_objects[index]);
                }
            }

            _objects.Clear();
        }

        [Test]
        public void ModeOffReturnsBeforeContextAndPerformsZeroQueries()
        {
            var physics = new FakeRoutePhysics();

            Atx24PowerCableRouteEvaluation result =
                Atx24PowerCableRouteSolver.Evaluate(
                    false,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    default,
                    2f,
                    0.94f,
                    0.01f,
                    false,
                    false,
                    false,
                    false,
                    PowerCableKeyOrientation.Reversed,
                    physics);

            Assert.That(result.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.ModeDisabled));
            Assert.That(result.HasPose, Is.False);
            Assert.That(physics.TotalQueryCount, Is.Zero);
        }

        [Test]
        public void ValidKeyedRouteReturnsExactMotherboardPoseAndFiveAuthoredSegments()
        {
            Fixture fixture = CreateFixture();

            Atx24PowerCableRouteEvaluation result = fixture.Evaluate();

            Assert.That(result.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.ValidRoute));
            Assert.That(result.CanRoute, Is.True);
            Assert.That(result.Pose.position,
                Is.EqualTo(fixture.MotherboardEndpoint.position));
            Assert.That(Quaternion.Angle(
                    result.Pose.rotation,
                    fixture.MotherboardEndpoint.rotation),
                Is.LessThan(0.001f));
            Assert.That(fixture.Physics.RaycastQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Physics.OverlapQueryCount, Is.EqualTo(5));
        }

        [Test]
        public void RoutedFocusUsesTheAuthoredConnectorWithoutRouteOverlapQueries()
        {
            Fixture fixture = CreateFixture();

            Atx24PowerCableRouteStatus disabled =
                Atx24PowerCableRouteSolver.EvaluateRoutedFocus(
                    false,
                    fixture.Origin,
                    fixture.Player,
                    fixture.Cable,
                    fixture.Focus,
                    fixture.RouteRoot,
                    fixture.PowerSupplyHost,
                    fixture.MotherboardHost,
                    1 << 0,
                    2f,
                    0.94f,
                    false,
                    fixture.Physics);
            Atx24PowerCableRouteStatus focused =
                Atx24PowerCableRouteSolver.EvaluateRoutedFocus(
                    true,
                    fixture.Origin,
                    fixture.Player,
                    fixture.Cable,
                    fixture.Focus,
                    fixture.RouteRoot,
                    fixture.PowerSupplyHost,
                    fixture.MotherboardHost,
                    1 << 0,
                    2f,
                    0.94f,
                    false,
                    fixture.Physics);

            Assert.That(disabled,
                Is.EqualTo(Atx24PowerCableRouteStatus.ModeDisabled));
            Assert.That(focused,
                Is.EqualTo(Atx24PowerCableRouteStatus.ValidRoute));
            Assert.That(fixture.Physics.RaycastQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Physics.OverlapQueryCount, Is.Zero);
        }

        [Test]
        public void HostAndOrientationGatesFailBeforeAnyPhysicsQuery()
        {
            Fixture fixture = CreateFixture();

            Atx24PowerCableRouteEvaluation boardMissing = fixture.Evaluate(
                motherboardSecured: false);
            Atx24PowerCableRouteEvaluation psuMissing = fixture.Evaluate(
                powerSupplyRetained: false);
            Atx24PowerCableRouteEvaluation reversed = fixture.Evaluate(
                orientation: PowerCableKeyOrientation.Reversed);

            Assert.That(boardMissing.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.HostMotherboardUnsecured));
            Assert.That(psuMissing.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.HostPowerSupplyUnretained));
            Assert.That(reversed.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.OrientationInvalid));
            Assert.That(fixture.Physics.TotalQueryCount, Is.Zero);
        }

        [Test]
        public void ExactDistanceLineOfSightTieFailsClosedInEitherResultOrder()
        {
            Fixture fixture = CreateFixture();
            Collider blocker = CreateCube(
                "Atx24LosTieBlocker",
                new Vector3(0f, 0f, 0.5f),
                Vector3.one * 0.05f).GetComponent<Collider>();

            fixture.Physics.SetLineHits(
                new Atx24PowerCablePhysicsHit(fixture.Focus, 1f),
                new Atx24PowerCablePhysicsHit(blocker, 1f));
            Atx24PowerCableRouteEvaluation targetFirst = fixture.Evaluate();

            fixture.Physics.SetLineHits(
                new Atx24PowerCablePhysicsHit(blocker, 1f),
                new Atx24PowerCablePhysicsHit(fixture.Focus, 1f));
            Atx24PowerCableRouteEvaluation blockerFirst = fixture.Evaluate();

            Assert.That(targetFirst.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.LineOfSightBlocked));
            Assert.That(blockerFirst.Status, Is.EqualTo(targetFirst.Status));
            Assert.That(targetFirst.FailureCode,
                Is.EqualTo("assembly-power-cable.line-of-sight-blocked"));
        }

        [Test]
        public void RouteObstructionAndSaturatedQueriesFailClosed()
        {
            Fixture fixture = CreateFixture();
            Collider blocker = CreateCube(
                "Atx24RouteBlocker",
                fixture.FirstWaypoint.position,
                Vector3.one * 0.05f).GetComponent<Collider>();
            fixture.Physics.SetOverlaps(blocker);

            Atx24PowerCableRouteEvaluation obstructed = fixture.Evaluate();

            fixture.Physics.SetOverlaps();
            fixture.Physics.OverlapCountOverride =
                Atx24PowerCableRouteSolver.HitCapacity;
            Atx24PowerCableRouteEvaluation saturated = fixture.Evaluate();

            Assert.That(obstructed.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.RouteObstructed));
            Assert.That(saturated.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.QuerySaturated));
            Assert.That(obstructed.CanRoute, Is.False);
            Assert.That(saturated.CanRoute, Is.False);
        }

        [Test]
        public void CableRouteAndHostOwnedCollidersAreIgnoredButPlayerIsNot()
        {
            Fixture fixture = CreateFixture();
            Collider cableCollider = fixture.Cable.gameObject.AddComponent<BoxCollider>();
            Collider routeCollider = fixture.RouteRoot.gameObject.AddComponent<BoxCollider>();
            Collider psuCollider = fixture.PowerSupplyHost.gameObject
                .AddComponent<BoxCollider>();
            Collider boardCollider = fixture.MotherboardHost.gameObject
                .AddComponent<BoxCollider>();
            fixture.Physics.SetOverlaps(
                cableCollider,
                routeCollider,
                psuCollider,
                boardCollider,
                fixture.Focus);

            Atx24PowerCableRouteEvaluation ignored = fixture.Evaluate();

            Collider playerCollider = fixture.Player.gameObject
                .AddComponent<BoxCollider>();
            fixture.Physics.SetOverlaps(playerCollider);
            Atx24PowerCableRouteEvaluation playerBlocked = fixture.Evaluate();

            Assert.That(ignored.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.ValidRoute));
            Assert.That(playerBlocked.Status,
                Is.EqualTo(Atx24PowerCableRouteStatus.RouteObstructed));
        }

        private Fixture CreateFixture()
        {
            Transform player = CreateObject("Player").transform;
            Transform origin = CreateObject("Origin").transform;
            origin.SetParent(player, false);
            origin.position = Vector3.zero;

            Transform cableRoot = CreateObject("Cable").transform;
            Rigidbody body = cableRoot.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            PhysicalItemProjection cable =
                cableRoot.gameObject.AddComponent<PhysicalItemProjection>();
            cable.Configure(
                "prototype.garage-atx24-power-cable-001",
                "ATX 24-pin Cable",
                body,
                new Vector3(0.06f, 0.05f, 0.03f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.PcComponent);

            Transform routeRoot = CreateObject("Route").transform;
            Transform powerSupplyHost = CreateObject("PowerSupplyHost").transform;
            Transform motherboardHost = CreateObject("MotherboardHost").transform;
            Transform psuPrimary = CreateChild(
                powerSupplyHost,
                "PsuPrimary",
                new Vector3(-0.2f, 0f, 0.3f));
            Transform psuSense = CreateChild(
                powerSupplyHost,
                "PsuSense",
                new Vector3(0.2f, 0f, 0.3f));
            Transform first = CreateChild(
                routeRoot,
                "Waypoint1",
                new Vector3(-0.2f, 0.1f, 0.5f));
            Transform second = CreateChild(
                routeRoot,
                "Waypoint2",
                new Vector3(0f, 0.1f, 0.7f));
            Transform third = CreateChild(
                routeRoot,
                "Waypoint3",
                new Vector3(0.2f, 0.1f, 0.9f));
            Transform motherboardEndpoint = CreateChild(
                motherboardHost,
                "MotherboardEndpoint",
                new Vector3(0f, 0f, 1f));
            motherboardEndpoint.rotation = Quaternion.Euler(0f, 90f, 0f);
            GameObject focusObject = CreateCube(
                "Focus",
                motherboardEndpoint.position,
                Vector3.one * 0.05f);
            focusObject.transform.SetParent(motherboardHost, true);
            Collider focus = focusObject.GetComponent<Collider>();
            focus.isTrigger = true;
            Physics.SyncTransforms();
            origin.rotation = Quaternion.LookRotation(
                focus.bounds.center - origin.position,
                Vector3.up);

            var physics = new FakeRoutePhysics();
            physics.SetLineHits(new Atx24PowerCablePhysicsHit(focus, 1f));
            return new Fixture(
                origin,
                player,
                cable,
                focus,
                routeRoot,
                psuPrimary,
                psuSense,
                motherboardEndpoint,
                first,
                second,
                third,
                powerSupplyHost,
                motherboardHost,
                physics);
        }

        private Transform CreateChild(
            Transform parent,
            string name,
            Vector3 worldPosition)
        {
            Transform child = CreateObject(name).transform;
            child.SetParent(parent, false);
            child.position = worldPosition;
            return child;
        }

        private GameObject CreateCube(string name, Vector3 position, Vector3 scale)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.position = position;
            cube.transform.localScale = scale;
            _objects.Add(cube);
            return cube;
        }

        private GameObject CreateObject(string name)
        {
            var instance = new GameObject(name);
            _objects.Add(instance);
            return instance;
        }

        private readonly struct Fixture
        {
            public Fixture(
                Transform origin,
                Transform player,
                PhysicalItemProjection cable,
                Collider focus,
                Transform routeRoot,
                Transform psuPrimary,
                Transform psuSense,
                Transform motherboardEndpoint,
                Transform firstWaypoint,
                Transform secondWaypoint,
                Transform thirdWaypoint,
                Transform powerSupplyHost,
                Transform motherboardHost,
                FakeRoutePhysics physics)
            {
                Origin = origin;
                Player = player;
                Cable = cable;
                Focus = focus;
                RouteRoot = routeRoot;
                PsuPrimary = psuPrimary;
                PsuSense = psuSense;
                MotherboardEndpoint = motherboardEndpoint;
                FirstWaypoint = firstWaypoint;
                SecondWaypoint = secondWaypoint;
                ThirdWaypoint = thirdWaypoint;
                PowerSupplyHost = powerSupplyHost;
                MotherboardHost = motherboardHost;
                Physics = physics;
            }

            public Transform Origin { get; }
            public Transform Player { get; }
            public PhysicalItemProjection Cable { get; }
            public Collider Focus { get; }
            public Transform RouteRoot { get; }
            public Transform PsuPrimary { get; }
            public Transform PsuSense { get; }
            public Transform MotherboardEndpoint { get; }
            public Transform FirstWaypoint { get; }
            public Transform SecondWaypoint { get; }
            public Transform ThirdWaypoint { get; }
            public Transform PowerSupplyHost { get; }
            public Transform MotherboardHost { get; }
            public FakeRoutePhysics Physics { get; }

            public Atx24PowerCableRouteEvaluation Evaluate(
                bool motherboardSecured = true,
                bool powerSupplyRetained = true,
                PowerCableKeyOrientation orientation =
                    PowerCableKeyOrientation.Keyed)
            {
                return Atx24PowerCableRouteSolver.Evaluate(
                    true,
                    Origin,
                    Player,
                    Cable,
                    Focus,
                    RouteRoot,
                    PsuPrimary,
                    PsuSense,
                    MotherboardEndpoint,
                    FirstWaypoint,
                    SecondWaypoint,
                    ThirdWaypoint,
                    PowerSupplyHost,
                    MotherboardHost,
                    1 << 0,
                    2f,
                    0.94f,
                    0.01f,
                    false,
                    true,
                    motherboardSecured,
                    powerSupplyRetained,
                    orientation,
                    Physics);
            }
        }

        private sealed class FakeRoutePhysics : IAtx24PowerCableRoutePhysics
        {
            private Atx24PowerCablePhysicsHit[] _lineHits =
                Array.Empty<Atx24PowerCablePhysicsHit>();
            private Collider[] _overlaps = Array.Empty<Collider>();

            public int? RaycastCountOverride { get; set; }
            public int? OverlapCountOverride { get; set; }
            public int RaycastQueryCount { get; private set; }
            public int OverlapQueryCount { get; private set; }
            public int TotalQueryCount => RaycastQueryCount + OverlapQueryCount;

            public void SetLineHits(params Atx24PowerCablePhysicsHit[] hits)
            {
                _lineHits = hits ?? Array.Empty<Atx24PowerCablePhysicsHit>();
            }

            public void SetOverlaps(params Collider[] colliders)
            {
                _overlaps = colliders ?? Array.Empty<Collider>();
            }

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                Atx24PowerCablePhysicsHit[] results,
                float maximumDistance,
                int layerMask)
            {
                RaycastQueryCount++;
                int count = Math.Min(_lineHits.Length, results.Length);
                for (int index = 0; index < count; index++)
                {
                    results[index] = _lineHits[index];
                }

                return RaycastCountOverride ?? _lineHits.Length;
            }

            public int OverlapCapsuleNonAlloc(
                Vector3 point0,
                Vector3 point1,
                float radius,
                Collider[] results,
                int layerMask)
            {
                OverlapQueryCount++;
                int count = Math.Min(_overlaps.Length, results.Length);
                for (int index = 0; index < count; index++)
                {
                    results[index] = _overlaps[index];
                }

                return OverlapCountOverride ?? _overlaps.Length;
            }
        }
    }
}
