using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class ProcessorCoolerSlotSolverTests
    {
        [Test]
        public void DeterministicTopDownGateAllowsBothCanonicalHalfTurns()
        {
            using var fixture = new Fixture();
            ProcessorCoolerSlotEvaluation aligned = fixture.Evaluate(0);
            ProcessorCoolerSlotEvaluation reversed = fixture.Evaluate(1);
            Assert.That(aligned.Status, Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat));
            Assert.That(aligned.CanSeat, Is.True);
            Assert.That(aligned.Orientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Primary));
            Assert.That(reversed.Status,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat));
            Assert.That(reversed.CanSeat, Is.True);
            Assert.That(reversed.Orientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Rotated180));
            Assert.That(Vector3.Angle(aligned.Pose.forward, reversed.Pose.forward),
                Is.LessThan(0.01f));
            Assert.That(Vector3.Angle(aligned.Pose.up, -reversed.Pose.up),
                Is.LessThan(0.01f));
        }
        [Test]
        public void GateIsFailClosedForPauseAuthorityFocusAndForeignObstruction()
        {
            using var fixture = new Fixture();
            Assert.That(fixture.Evaluate(paused: true).Status, Is.EqualTo(ProcessorCoolerSlotStatus.Paused));
            Assert.That(fixture.Evaluate(authority: false).Status, Is.EqualTo(ProcessorCoolerSlotStatus.AuthorityBlocked));
            fixture.Origin.rotation = Quaternion.LookRotation(Vector3.back);
            Assert.That(fixture.Evaluate().Status, Is.EqualTo(ProcessorCoolerSlotStatus.NotFocused));
            fixture.Origin.rotation = Quaternion.identity; fixture.Block();
            Assert.That(fixture.Evaluate().Status, Is.EqualTo(ProcessorCoolerSlotStatus.Obstructed));
        }
        [Test]
        public void ExplicitRamClearanceBlockerInsideAssemblyIsNeverIgnored()
        {
            using var fixture = new Fixture();
            fixture.Block(parentToAssembly: true);
            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat));
            Assert.That(fixture.Evaluate(useExplicitBlocker: true).Status,
                Is.EqualTo(ProcessorCoolerSlotStatus.Obstructed));
        }
        [Test]
        public void ProjectionRequiresFourPointTopologyAndMirrorsState()
        {
            var root = new GameObject("CoolerSlot"); var anchor = new GameObject("Anchor").transform; anchor.SetParent(root.transform);
            var focus = GameObject.CreatePrimitive(PrimitiveType.Cube); focus.transform.SetParent(root.transform);
            var bracket = new GameObject("Bracket").transform; bracket.SetParent(root.transform);
            var points = new Transform[4]; for (int i = 0; i < points.Length; i++) { points[i] = new GameObject("Point" + i).transform; points[i].SetParent(root.transform); }
            var projection = root.AddComponent<ProcessorCoolerSlotProjection>();
            try
            {
                projection.Configure(
                    GarageStockFlowSession.ProcessorCoolerSlotIdValue,
                    GarageStockFlowSession.ProcessorCoolerBracketIdValue,
                    new[]
                    {
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint1IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint2IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint3IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint4IdValue
                    },
                    anchor,
                    focus.GetComponent<Collider>(),
                    root.transform,
                    bracket,
                    points);
                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    ProcessorSocketState.ProcessorRetained,
                    ProcessorCoolerSlotState.CoolerRetained);
                Assert.That(projection.IsConfigured, Is.True);
                Assert.That(projection.MatchesLogicalAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    ProcessorSocketState.ProcessorRetained,
                    ProcessorCoolerSlotState.CoolerRetained), Is.True);
                OperationResult<Pose> primaryPose = projection.ResolveSeatPose(
                    ProcessorCoolerMountOrientation.Primary);
                OperationResult<Pose> rotatedPose = projection.ResolveSeatPose(
                    ProcessorCoolerMountOrientation.Rotated180);
                Assert.That(primaryPose.IsSuccess, Is.True);
                Assert.That(rotatedPose.IsSuccess, Is.True);
                Assert.That(Vector3.Angle(
                    primaryPose.Value.forward,
                    rotatedPose.Value.forward), Is.LessThan(0.01f));
                Assert.That(Quaternion.Angle(
                    primaryPose.Value.rotation,
                    rotatedPose.Value.rotation), Is.GreaterThan(179.9f));
                Assert.That(Quaternion.Angle(
                    bracket.localRotation,
                    Quaternion.Euler(0f, 0f, 90f)), Is.LessThan(.01f));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void ProjectionRejectsDuplicateRetentionTransforms()
        {
            var root = new GameObject("CoolerSlotDuplicate");
            Transform anchor = new GameObject("Anchor").transform;
            anchor.SetParent(root.transform);
            GameObject focus = GameObject.CreatePrimitive(PrimitiveType.Cube);
            focus.transform.SetParent(root.transform);
            Transform bracket = new GameObject("Bracket").transform;
            bracket.SetParent(root.transform);
            Transform sharedPoint = new GameObject("SharedPoint").transform;
            sharedPoint.SetParent(root.transform);
            var projection = root.AddComponent<ProcessorCoolerSlotProjection>();
            try
            {
                Assert.Throws<System.ArgumentException>(() => projection.Configure(
                    GarageStockFlowSession.ProcessorCoolerSlotIdValue,
                    GarageStockFlowSession.ProcessorCoolerBracketIdValue,
                    new[]
                    {
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint1IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint2IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint3IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint4IdValue
                    },
                    anchor,
                    focus.GetComponent<Collider>(),
                    root.transform,
                    bracket,
                    new[] { sharedPoint, sharedPoint, sharedPoint, sharedPoint }));
                Assert.That(projection.IsConfigured, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void RuntimeGeometryRequiresFourDistinctChildrenOwnedByCoolerRoot()
        {
            var root = new GameObject("CoolerGeometryRoot");
            var geometry = root.AddComponent<ProcessorCoolerRuntimeGeometry>();
            Transform plate = Child(root, "Plate");
            Transform tim = Child(root, "Tim");
            Transform fins = Child(root, "Fins");
            Transform fan = Child(root, "Fan");
            Transform bracket = Child(root, "Bracket");
            Transform[] points =
            {
                Child(root, "Point1"),
                Child(root, "Point2"),
                Child(root, "Point3"),
                Child(root, "Point4")
            };
            try
            {
                geometry.Configure(plate, tim, fins, fan, bracket, points);
                Assert.That(geometry.IsCanonical, Is.True);
                Assert.That(geometry.RetentionPoints, Is.Not.SameAs(points));

                var duplicateRoot = new GameObject("DuplicateCoolerGeometryRoot");
                var duplicateGeometry = duplicateRoot.AddComponent<
                    ProcessorCoolerRuntimeGeometry>();
                Transform duplicatePlate = Child(duplicateRoot, "Plate");
                Transform duplicateTim = Child(duplicateRoot, "Tim");
                Transform duplicateFins = Child(duplicateRoot, "Fins");
                Transform duplicateFan = Child(duplicateRoot, "Fan");
                Transform duplicateBracket = Child(duplicateRoot, "Bracket");
                Transform shared = Child(duplicateRoot, "SharedPoint");
                Assert.Throws<System.ArgumentException>(() =>
                    duplicateGeometry.Configure(
                        duplicatePlate,
                        duplicateTim,
                        duplicateFins,
                        duplicateFan,
                        duplicateBracket,
                        new[] { shared, shared, shared, shared }));
                Object.DestroyImmediate(duplicateRoot);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Transform Child(GameObject parent, string name)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent.transform);
            return child;
        }
        private sealed class Fixture : System.IDisposable
        {
            private readonly GameObject _player = new GameObject("Player"); private readonly GameObject _assembly = new GameObject("Assembly"); private readonly GameObject _cooler = GameObject.CreatePrimitive(PrimitiveType.Cube); private readonly GameObject _focus = GameObject.CreatePrimitive(PrimitiveType.Cube); private GameObject _blocker;
            public Fixture() { Origin = new GameObject("Origin").transform; Origin.SetParent(_player.transform); Seat = new GameObject("Seat").transform; Seat.SetParent(_assembly.transform); Seat.position = new Vector3(0,0,1); _focus.transform.SetParent(_assembly.transform); _focus.transform.position = Seat.position; _focus.transform.localScale = new Vector3(.14f,.03f,.14f); _cooler.transform.position = new Vector3(3,0,0); var body = _cooler.AddComponent<Rigidbody>(); body.isKinematic = true; Cooler = _cooler.AddComponent<PhysicalItemProjection>(); Cooler.Configure("prototype.garage-cooler-001", "LGA1700 Air Cooler", body, new Vector3(.07f,.04f,.07f), Vector3.zero, Vector3.zero, PhysicalCarryProfile.PcComponent); Physics.SyncTransforms(); }
            public Transform Origin { get; } public Transform Seat { get; } public PhysicalItemProjection Cooler { get; }
            public ProcessorCoolerSlotEvaluation Evaluate(int turns = 0, bool paused = false, bool authority = true, bool useExplicitBlocker = false) { Physics.SyncTransforms(); return ProcessorCoolerSlotSolver.EvaluateSeat(Origin, _player.transform, Cooler, Seat, _focus.GetComponent<Collider>(), _assembly.transform, 1 << 0, 2f, .94f, turns, paused, authority, useExplicitBlocker && _blocker != null ? new[] { _blocker.GetComponent<Collider>() } : null); }
            public void Block(bool parentToAssembly = false) { _blocker = GameObject.CreatePrimitive(PrimitiveType.Cube); if (parentToAssembly) _blocker.transform.SetParent(_assembly.transform); _blocker.transform.position = Seat.position + Vector3.right * .05f; _blocker.transform.localScale = Vector3.one * .03f; Physics.SyncTransforms(); }
            public void Dispose() { if (_blocker != null) Object.DestroyImmediate(_blocker); Object.DestroyImmediate(_cooler); Object.DestroyImmediate(_focus); Object.DestroyImmediate(_assembly); Object.DestroyImmediate(_player); }
        }
    }
}
