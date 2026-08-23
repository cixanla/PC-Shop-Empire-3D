using System;
using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class Eps12vPowerCableRouteSolverTests
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

            Eps12vPowerCableRouteEvaluation result =
                Eps12vPowerCableRouteSolver.Evaluate(
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
                    default,
                    2f,
                    0.94f,
                    0.01f,
                    false,
                    false,
                    false,
                    false,
                    false,
                    PowerCableKeyOrientation.Reversed,
                    physics);

            Assert.That(result.Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ModeDisabled));
            Assert.That(result.HasPose, Is.False);
            Assert.That(physics.TotalQueryCount, Is.Zero);
        }

        [Test]
        public void ValidKeyedRouteReturnsExactPoseAndFourOrderedSegments()
        {
            Fixture fixture = CreateFixture();

            Eps12vPowerCableRouteEvaluation result = fixture.Evaluate();

            Assert.That(result.Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute));
            Assert.That(result.CanRoute, Is.True);
            Assert.That(result.Pose.position,
                Is.EqualTo(fixture.MotherboardEndpoint.position));
            Assert.That(Quaternion.Angle(
                    result.Pose.rotation,
                    fixture.MotherboardEndpoint.rotation),
                Is.LessThan(0.001f));
            Assert.That(fixture.Physics.RaycastQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Physics.OverlapQueryCount, Is.EqualTo(4));
        }

        [Test]
        public void MotherboardPsuProcessorAndOrientationGatesRunBeforePhysics()
        {
            Fixture fixture = CreateFixture();

            Assert.That(
                fixture.Evaluate(motherboardSecured: false).Status,
                Is.EqualTo(
                    Eps12vPowerCableRouteStatus.HostMotherboardUnsecured));
            Assert.That(
                fixture.Evaluate(powerSupplyRetained: false).Status,
                Is.EqualTo(
                    Eps12vPowerCableRouteStatus.HostPowerSupplyUnretained));
            Assert.That(
                fixture.Evaluate(processorRetained: false).Status,
                Is.EqualTo(
                    Eps12vPowerCableRouteStatus.HostProcessorUnretained));
            Assert.That(
                fixture.Evaluate(
                    orientation: PowerCableKeyOrientation.Reversed).Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.OrientationInvalid));
            Assert.That(fixture.Physics.TotalQueryCount, Is.Zero);
        }

        [Test]
        public void ExactDistanceLineOfSightTieFailsClosedInEitherOrder()
        {
            Fixture fixture = CreateFixture();
            Collider blocker = CreateCube(
                "Eps12vLosTieBlocker",
                new Vector3(0f, 0f, 0.5f),
                Vector3.one * 0.05f).GetComponent<Collider>();

            fixture.Physics.SetLineHits(
                new Eps12vPowerCablePhysicsHit(fixture.Focus, 1f),
                new Eps12vPowerCablePhysicsHit(blocker, 1f));
            Eps12vPowerCableRouteEvaluation targetFirst = fixture.Evaluate();

            fixture.Physics.SetLineHits(
                new Eps12vPowerCablePhysicsHit(blocker, 1f),
                new Eps12vPowerCablePhysicsHit(fixture.Focus, 1f));
            Eps12vPowerCableRouteEvaluation blockerFirst = fixture.Evaluate();

            Assert.That(targetFirst.Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.LineOfSightBlocked));
            Assert.That(blockerFirst.Status, Is.EqualTo(targetFirst.Status));
            Assert.That(targetFirst.FailureCode,
                Is.EqualTo("assembly-eps12v-cable.line-of-sight-blocked"));
        }

        [Test]
        public void ObstructionAndSaturatedOverlapQueriesFailClosed()
        {
            Fixture fixture = CreateFixture();
            Collider blocker = CreateCube(
                "Eps12vRouteBlocker",
                fixture.FirstWaypoint.position,
                Vector3.one * 0.05f).GetComponent<Collider>();
            fixture.Physics.SetOverlaps(blocker);

            Eps12vPowerCableRouteEvaluation obstructed = fixture.Evaluate();

            fixture.Physics.SetOverlaps();
            fixture.Physics.OverlapCountOverride =
                Eps12vPowerCableRouteSolver.HitCapacity;
            Eps12vPowerCableRouteEvaluation saturated = fixture.Evaluate();

            Assert.That(obstructed.Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.RouteObstructed));
            Assert.That(saturated.Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.QuerySaturated));
        }

        [Test]
        public void ExplicitAllowlistDoesNotIgnoreWholeHostHierarchy()
        {
            Fixture fixture = CreateFixture();
            fixture.Physics.SetOverlaps(
                fixture.AllowedPsuCollider,
                fixture.AllowedMotherboardCollider,
                fixture.Focus);

            Eps12vPowerCableRouteEvaluation allowed = fixture.Evaluate();

            GameObject unlistedHostChild = CreateCube(
                "UnlistedMotherboardObstacle",
                fixture.SecondWaypoint.position,
                Vector3.one * 0.05f);
            unlistedHostChild.transform.SetParent(
                fixture.MotherboardHost,
                true);
            fixture.Physics.SetOverlaps(
                unlistedHostChild.GetComponent<Collider>());
            Eps12vPowerCableRouteEvaluation blocked = fixture.Evaluate();

            Assert.That(allowed.Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute));
            Assert.That(blocked.Status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.RouteObstructed));
        }

        [Test]
        public void RoutedFocusUsesBoardEndpointWithoutOverlapQueries()
        {
            Fixture fixture = CreateFixture();

            Eps12vPowerCableRouteStatus status =
                Eps12vPowerCableRouteSolver.EvaluateRoutedFocus(
                    true,
                    fixture.Origin,
                    fixture.Player,
                    fixture.Cable,
                    fixture.Focus,
                    fixture.RouteRoot,
                    fixture.AllowedColliders,
                    1 << 0,
                    2f,
                    0.94f,
                    false,
                    fixture.Physics);

            Assert.That(status,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute));
            Assert.That(fixture.Physics.RaycastQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Physics.OverlapQueryCount, Is.Zero);
        }

        private Fixture CreateFixture()
        {
            Transform player = CreateObject("Player").transform;
            Transform origin = CreateObject("Origin").transform;
            origin.SetParent(player, false);

            Transform cableRoot = CreateObject("Eps12vCable").transform;
            Rigidbody body = cableRoot.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;
            PhysicalItemProjection cable =
                cableRoot.gameObject.AddComponent<PhysicalItemProjection>();
            cable.Configure(
                GarageStockFlowSession.Eps12vPowerCableItemInstanceIdValue,
                GarageStockFlowSession.Eps12vPowerCableDisplayName,
                body,
                new Vector3(0.045f, 0.039f, 0.026f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.PcComponent);

            Transform routeRoot = CreateObject("Eps12vRoute").transform;
            Transform powerSupplyHost = CreateObject("PowerSupplyHost").transform;
            Transform motherboardHost = CreateObject("MotherboardHost").transform;
            Collider allowedPsu = powerSupplyHost.gameObject.AddComponent<BoxCollider>();
            Collider allowedMotherboard =
                motherboardHost.gameObject.AddComponent<BoxCollider>();
            Transform psuEndpoint = CreateChild(
                powerSupplyHost,
                "PsuCpu8",
                new Vector3(-0.2f, 0f, 0.3f));
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
                "MotherboardCpu8",
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
            physics.SetLineHits(new Eps12vPowerCablePhysicsHit(focus, 1f));
            return new Fixture(
                origin,
                player,
                cable,
                focus,
                routeRoot,
                psuEndpoint,
                motherboardEndpoint,
                first,
                second,
                third,
                powerSupplyHost,
                motherboardHost,
                allowedPsu,
                allowedMotherboard,
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
                Transform psuEndpoint,
                Transform motherboardEndpoint,
                Transform firstWaypoint,
                Transform secondWaypoint,
                Transform thirdWaypoint,
                Transform powerSupplyHost,
                Transform motherboardHost,
                Collider allowedPsuCollider,
                Collider allowedMotherboardCollider,
                FakeRoutePhysics physics)
            {
                Origin = origin;
                Player = player;
                Cable = cable;
                Focus = focus;
                RouteRoot = routeRoot;
                PsuEndpoint = psuEndpoint;
                MotherboardEndpoint = motherboardEndpoint;
                FirstWaypoint = firstWaypoint;
                SecondWaypoint = secondWaypoint;
                ThirdWaypoint = thirdWaypoint;
                PowerSupplyHost = powerSupplyHost;
                MotherboardHost = motherboardHost;
                AllowedPsuCollider = allowedPsuCollider;
                AllowedMotherboardCollider = allowedMotherboardCollider;
                AllowedColliders = new[]
                {
                    allowedPsuCollider,
                    allowedMotherboardCollider
                };
                Physics = physics;
            }

            public Transform Origin { get; }
            public Transform Player { get; }
            public PhysicalItemProjection Cable { get; }
            public Collider Focus { get; }
            public Transform RouteRoot { get; }
            public Transform PsuEndpoint { get; }
            public Transform MotherboardEndpoint { get; }
            public Transform FirstWaypoint { get; }
            public Transform SecondWaypoint { get; }
            public Transform ThirdWaypoint { get; }
            public Transform PowerSupplyHost { get; }
            public Transform MotherboardHost { get; }
            public Collider AllowedPsuCollider { get; }
            public Collider AllowedMotherboardCollider { get; }
            public Collider[] AllowedColliders { get; }
            public FakeRoutePhysics Physics { get; }

            public Eps12vPowerCableRouteEvaluation Evaluate(
                bool motherboardSecured = true,
                bool powerSupplyRetained = true,
                bool processorRetained = true,
                PowerCableKeyOrientation orientation =
                    PowerCableKeyOrientation.Keyed)
            {
                return Eps12vPowerCableRouteSolver.Evaluate(
                    true,
                    Origin,
                    Player,
                    Cable,
                    Focus,
                    RouteRoot,
                    PsuEndpoint,
                    MotherboardEndpoint,
                    FirstWaypoint,
                    SecondWaypoint,
                    ThirdWaypoint,
                    AllowedColliders,
                    1 << 0,
                    2f,
                    0.94f,
                    0.01f,
                    false,
                    true,
                    motherboardSecured,
                    powerSupplyRetained,
                    processorRetained,
                    orientation,
                    Physics);
            }
        }

        private sealed class FakeRoutePhysics : IEps12vPowerCableRoutePhysics
        {
            private Eps12vPowerCablePhysicsHit[] _lineHits =
                Array.Empty<Eps12vPowerCablePhysicsHit>();
            private Collider[] _overlaps = Array.Empty<Collider>();

            public int? RaycastCountOverride { get; set; }
            public int? OverlapCountOverride { get; set; }
            public int RaycastQueryCount { get; private set; }
            public int OverlapQueryCount { get; private set; }
            public int TotalQueryCount => RaycastQueryCount + OverlapQueryCount;

            public void SetLineHits(params Eps12vPowerCablePhysicsHit[] hits)
            {
                _lineHits = hits ?? Array.Empty<Eps12vPowerCablePhysicsHit>();
            }

            public void SetOverlaps(params Collider[] colliders)
            {
                _overlaps = colliders ?? Array.Empty<Collider>();
            }

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                Eps12vPowerCablePhysicsHit[] results,
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
