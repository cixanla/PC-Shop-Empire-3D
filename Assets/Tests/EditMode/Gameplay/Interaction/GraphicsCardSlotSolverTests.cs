using System;
using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class GraphicsCardSlotSolverTests
    {
        private const string SlotId = "assembly.slot.graphics-card.pcie-x16-01";
        private const string LatchId = "assembly.latch.graphics-card.pcie-x16-01";
        private const string BracketId = "assembly.bracket.graphics-card.rear-01";
        private const string FastenerId =
            "assembly.fastener.graphics-card.rear-01";

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
            var query = new FakeGraphicsCardSlotPhysics();

            GraphicsCardSlotEvaluation seat = GraphicsCardSlotSolver.EvaluateSeat(
                false,
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
                0,
                false,
                true,
                GraphicsCardPcieInterface.PcieX16,
                GraphicsCardPcieInterface.PcieX16,
                true,
                true,
                null,
                null,
                query);
            GraphicsCardSlotEvaluation interaction =
                GraphicsCardSlotSolver.EvaluateInteraction(
                    false,
                    null,
                    null,
                    null,
                    null,
                    null,
                    default,
                    2f,
                    0.94f,
                    false,
                    GraphicsCardSlotProjectionState.GraphicsCardRetained,
                    true,
                    true,
                    query);

            Assert.That(seat.Status, Is.EqualTo(GraphicsCardSlotStatus.ModeDisabled));
            Assert.That(seat.HasPose, Is.False);
            Assert.That(interaction.Status,
                Is.EqualTo(GraphicsCardSlotStatus.ModeDisabled));
            Assert.That(query.TotalQueryCount, Is.Zero);
        }

        [Test]
        public void ValidPrimaryReturnsExactAuthoredPoseForPreviewAndCommit()
        {
            Fixture fixture = CreateFixture();

            GraphicsCardSlotEvaluation result = fixture.Evaluate();
            Pose expected = GraphicsCardSlotSolver.ResolveSeatPose(fixture.Snap, 0);

            Assert.That(result.Status, Is.EqualTo(GraphicsCardSlotStatus.ValidSeat));
            Assert.That(result.CanSeat, Is.True);
            Assert.That(result.HasPose, Is.True);
            Assert.That(result.Orientation,
                Is.EqualTo(GraphicsCardSeatOrientation.Primary));
            Assert.That(result.Pose.position, Is.EqualTo(expected.position));
            Assert.That(Quaternion.Angle(result.Pose.rotation, expected.rotation),
                Is.LessThan(0.001f));
            Assert.That(fixture.Query.RaycastQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Query.SupportQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Query.OverlapQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Query.BoxCastQueryCount, Is.EqualTo(1));
        }

        [Test]
        public void RotatedHalfTurnKeepsCandidatePoseButRejectsKeyMismatch()
        {
            Fixture fixture = CreateFixture();

            GraphicsCardSlotEvaluation primary = fixture.Evaluate();
            GraphicsCardSlotEvaluation rotated = fixture.Evaluate(halfTurns: 1);

            Assert.That(rotated.Status,
                Is.EqualTo(GraphicsCardSlotStatus.OrientationInvalid));
            Assert.That(rotated.CanSeat, Is.False);
            Assert.That(rotated.HasPose, Is.True);
            Assert.That(rotated.Orientation,
                Is.EqualTo(GraphicsCardSeatOrientation.Rotated180));
            Assert.That(rotated.Pose.position, Is.EqualTo(primary.Pose.position));
            Assert.That(Quaternion.Angle(
                    primary.Pose.rotation,
                    rotated.Pose.rotation),
                Is.EqualTo(180f).Within(0.001f));
            Assert.That(rotated.FailureCode,
                Is.EqualTo("assembly-graphics-card.orientation-mismatch"));
        }

        [Test]
        public void RangeFocusAndMissingLineOfSightFailClosedBeforeVolumeQueries()
        {
            Fixture fixture = CreateFixture();
            fixture.Origin.position = new Vector3(0f, 0f, -3f);

            GraphicsCardSlotEvaluation outOfRange = fixture.Evaluate();

            fixture.Origin.position = Vector3.zero;
            fixture.Origin.rotation = Quaternion.LookRotation(
                Vector3.back,
                Vector3.up);
            GraphicsCardSlotEvaluation notFocused = fixture.Evaluate();

            fixture.Origin.rotation = Quaternion.LookRotation(
                fixture.Focus.bounds.center - fixture.Origin.position,
                Vector3.up);
            fixture.Query.SetLineHits();
            GraphicsCardSlotEvaluation noLineOfSight = fixture.Evaluate();

            Assert.That(outOfRange.Status,
                Is.EqualTo(GraphicsCardSlotStatus.OutOfRange));
            Assert.That(notFocused.Status,
                Is.EqualTo(GraphicsCardSlotStatus.NotFocused));
            Assert.That(noLineOfSight.Status,
                Is.EqualTo(GraphicsCardSlotStatus.LineOfSightBlocked));
            Assert.That(fixture.Query.SupportQueryCount, Is.Zero);
            Assert.That(fixture.Query.OverlapQueryCount, Is.Zero);
            Assert.That(fixture.Query.BoxCastQueryCount, Is.Zero);
        }

        [Test]
        public void ExactDistanceLineOfSightTieFailsClosedInEitherResultOrder()
        {
            Fixture fixture = CreateFixture();
            Collider blocker = CreateCube(
                "GraphicsCardLosTieBlocker",
                new Vector3(0f, 0f, 0.5f),
                Vector3.one * 0.05f).GetComponent<Collider>();

            fixture.Query.SetLineHits(
                new GraphicsCardPhysicsHit(fixture.Focus, 1f),
                new GraphicsCardPhysicsHit(blocker, 1f));
            GraphicsCardSlotEvaluation targetFirst = fixture.Evaluate();

            fixture.Query.SetLineHits(
                new GraphicsCardPhysicsHit(blocker, 1f),
                new GraphicsCardPhysicsHit(fixture.Focus, 1f));
            GraphicsCardSlotEvaluation blockerFirst = fixture.Evaluate();

            Assert.That(targetFirst.Status,
                Is.EqualTo(GraphicsCardSlotStatus.LineOfSightBlocked));
            Assert.That(blockerFirst.Status, Is.EqualTo(targetFirst.Status));
            Assert.That(targetFirst.FailureCode,
                Is.EqualTo("assembly-graphics-card.line-of-sight-blocked"));
        }

        [Test]
        public void MissingPcieSupportFailsBeforeAnySeatVolumeQuery()
        {
            Fixture fixture = CreateFixture();
            fixture.Query.SupportHit = false;

            GraphicsCardSlotEvaluation result = fixture.Evaluate();

            Assert.That(result.Status,
                Is.EqualTo(GraphicsCardSlotStatus.Unsupported));
            Assert.That(result.FailureCode,
                Is.EqualTo("assembly-graphics-card.support-missing"));
            Assert.That(fixture.Query.RaycastQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Query.SupportQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Query.OverlapQueryCount, Is.Zero);
            Assert.That(fixture.Query.BoxCastQueryCount, Is.Zero);
        }

        [Test]
        public void ChassisCoolerAndGenericObstructionsUseStablePriority()
        {
            Fixture fixture = CreateFixture();
            Collider chassis = CreateAssemblyBlocker(
                fixture.Assembly,
                "GraphicsCardChassisClearance");
            Collider cooler = CreateAssemblyBlocker(
                fixture.Assembly,
                "GraphicsCardCoolerClearance");
            Collider generic = CreateCube(
                "GraphicsCardForeignObstruction",
                fixture.Snap.position,
                Vector3.one * 0.05f).GetComponent<Collider>();
            var chassisBlockers = new[] { chassis };
            var coolerBlockers = new[] { cooler };

            fixture.Query.SetOverlaps(generic, cooler, chassis);
            GraphicsCardSlotEvaluation allKinds = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                coolerBlockers: coolerBlockers);

            fixture.Query.SetOverlaps(generic, cooler);
            GraphicsCardSlotEvaluation coolerAndGeneric = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                coolerBlockers: coolerBlockers);

            fixture.Query.SetOverlaps(generic);
            GraphicsCardSlotEvaluation genericOnly = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                coolerBlockers: coolerBlockers);

            Assert.That(allKinds.Status,
                Is.EqualTo(GraphicsCardSlotStatus.ChassisClearanceBlocked));
            Assert.That(coolerAndGeneric.Status,
                Is.EqualTo(GraphicsCardSlotStatus.CoolerClearanceBlocked));
            Assert.That(genericOnly.Status,
                Is.EqualTo(GraphicsCardSlotStatus.Obstructed));
        }

        [Test]
        public void StaticClearanceFailurePrecedesSupportAndVolumeQueries()
        {
            Fixture fixture = CreateFixture();

            GraphicsCardSlotEvaluation chassis = fixture.Evaluate(
                chassisClearanceAvailable: false,
                coolerClearanceAvailable: false);
            GraphicsCardSlotEvaluation cooler = fixture.Evaluate(
                chassisClearanceAvailable: true,
                coolerClearanceAvailable: false);

            Assert.That(chassis.Status,
                Is.EqualTo(GraphicsCardSlotStatus.ChassisClearanceBlocked));
            Assert.That(cooler.Status,
                Is.EqualTo(GraphicsCardSlotStatus.CoolerClearanceBlocked));
            Assert.That(fixture.Query.SupportQueryCount, Is.Zero);
            Assert.That(fixture.Query.OverlapQueryCount, Is.Zero);
            Assert.That(fixture.Query.BoxCastQueryCount, Is.Zero);
        }

        [Test]
        public void OverlapAndBoxCastSaturationFailClosed()
        {
            Fixture fixture = CreateFixture();
            fixture.Query.OverlapCountOverride = GraphicsCardSlotSolver.HitCapacity;

            GraphicsCardSlotEvaluation overlapSaturated = fixture.Evaluate();

            fixture.Query.OverlapCountOverride = null;
            fixture.Query.BoxCastCountOverride = GraphicsCardSlotSolver.HitCapacity;
            GraphicsCardSlotEvaluation boxCastSaturated = fixture.Evaluate();

            Assert.That(overlapSaturated.Status,
                Is.EqualTo(GraphicsCardSlotStatus.Obstructed));
            Assert.That(boxCastSaturated.Status,
                Is.EqualTo(GraphicsCardSlotStatus.Obstructed));
            Assert.That(overlapSaturated.CanSeat, Is.False);
            Assert.That(boxCastSaturated.CanSeat, Is.False);
        }

        [Test]
        public void ObstructionDecisionIsIndependentOfBufferAndResultOrder()
        {
            Fixture fixture = CreateFixture();
            Collider chassis = CreateAssemblyBlocker(
                fixture.Assembly,
                "GraphicsCardOrderedChassisClearance");
            Collider cooler = CreateAssemblyBlocker(
                fixture.Assembly,
                "GraphicsCardOrderedCoolerClearance");
            Collider generic = CreateCube(
                "GraphicsCardOrderedForeignObstruction",
                fixture.Snap.position,
                Vector3.one * 0.05f).GetComponent<Collider>();
            var chassisBlockers = new[] { chassis };
            var coolerBlockers = new[] { cooler };

            fixture.Query.SetOverlaps(generic, cooler);
            fixture.Query.SetBoxCastHits(new GraphicsCardPhysicsHit(chassis, 0.05f));
            GraphicsCardSlotEvaluation overlapFirst = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                coolerBlockers: coolerBlockers);

            fixture.Query.SetOverlaps(chassis);
            fixture.Query.SetBoxCastHits(
                new GraphicsCardPhysicsHit(cooler, 0.02f),
                new GraphicsCardPhysicsHit(generic, 0.01f));
            GraphicsCardSlotEvaluation castFirst = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                coolerBlockers: coolerBlockers);

            Assert.That(overlapFirst.Status,
                Is.EqualTo(GraphicsCardSlotStatus.ChassisClearanceBlocked));
            Assert.That(castFirst.Status, Is.EqualTo(overlapFirst.Status));
            Assert.That(castFirst.FailureCode, Is.EqualTo(overlapFirst.FailureCode));
        }

        [Test]
        public void ProjectionOwnsDistinctIdentityExactPoseAndVisualState()
        {
            Fixture fixture = CreateFixture();
            Transform latch = CreateChild(fixture.Assembly, "GraphicsCardLatchPivot");
            Transform fastener = CreateChild(
                fixture.Assembly,
                "GraphicsCardRearBracketFastenerPivot");
            latch.localRotation = Quaternion.Euler(0f, 8f, 0f);
            fastener.localPosition = new Vector3(0f, 0.01f, 0f);
            fastener.localRotation = Quaternion.Euler(0f, 0f, 12f);
            Quaternion openLatchRotation = latch.localRotation;
            Vector3 openFastenerPosition = fastener.localPosition;
            Quaternion openFastenerRotation = fastener.localRotation;
            GraphicsCardSlotProjection projection =
                fixture.Assembly.gameObject.AddComponent<GraphicsCardSlotProjection>();

            projection.Configure(
                SlotId,
                LatchId,
                BracketId,
                FastenerId,
                fixture.Snap,
                fixture.Focus,
                fixture.Support,
                fixture.Assembly,
                latch,
                fastener);

            Assert.That(projection.IsConfigured, Is.True);
            Assert.That(projection.SlotIdValue, Is.EqualTo(SlotId));
            Assert.That(projection.LatchIdValue, Is.EqualTo(LatchId));
            Assert.That(projection.RearBracketIdValue, Is.EqualTo(BracketId));
            Assert.That(projection.RearBracketFastenerIdValue,
                Is.EqualTo(FastenerId));
            Assert.That(projection.SlotInterface,
                Is.EqualTo(GraphicsCardPcieInterface.PcieX16));
            Assert.That(projection.FocusCollider.enabled, Is.False);

            var projectionPose = projection.ResolveSeatPose(0);
            Pose solverPose = GraphicsCardSlotSolver.ResolveSeatPose(
                fixture.Snap,
                0);
            Assert.That(projectionPose.IsSuccess, Is.True);
            Assert.That(projectionPose.Value.position, Is.EqualTo(solverPose.position));
            Assert.That(Quaternion.Angle(
                    projectionPose.Value.rotation,
                    solverPose.rotation),
                Is.LessThan(0.001f));

            projection.ApplyAuthoritativeState(
                true,
                GraphicsCardSlotProjectionState.GraphicsCardSeatedUnsecured);
            Assert.That(projection.FocusCollider.enabled, Is.True);
            Assert.That(Quaternion.Angle(latch.localRotation, openLatchRotation),
                Is.LessThan(0.001f));
            Assert.That(fastener.localPosition, Is.EqualTo(openFastenerPosition));

            projection.ApplyAuthoritativeState(
                true,
                GraphicsCardSlotProjectionState.GraphicsCardRetained);
            Assert.That(projection.MatchesLogicalAuthorityState(
                true,
                GraphicsCardSlotProjectionState.GraphicsCardRetained), Is.True);
            Assert.That(Quaternion.Angle(
                    latch.localRotation,
                    openLatchRotation * Quaternion.AngleAxis(25f, Vector3.up)),
                Is.LessThan(0.001f));
            Assert.That(fastener.localPosition.y,
                Is.EqualTo(openFastenerPosition.y - 0.003f).Within(0.00001f));
            Assert.That(Quaternion.Angle(
                    fastener.localRotation,
                    openFastenerRotation *
                    Quaternion.AngleAxis(120f, Vector3.forward)),
                Is.LessThan(0.001f));

            GraphicsCardSlotEvaluation blockedFeedback =
                projection.ApplyAuthoritativeInteractionFeedback(
                    false,
                    GraphicsCardSlotProjectionState.GraphicsCardSeatedUnsecured);
            Assert.That(blockedFeedback.Status,
                Is.EqualTo(
                    GraphicsCardSlotStatus.ValidSeatedUnsecuredRetentionBlocked));

            Assert.Throws<ArgumentException>(() => projection.Configure(
                SlotId,
                SlotId,
                BracketId,
                FastenerId,
                fixture.Snap,
                fixture.Focus,
                fixture.Support,
                fixture.Assembly,
                latch,
                fastener));
        }

        private Fixture CreateFixture()
        {
            GameObject player = CreateObject("GraphicsCardPlayer");
            Transform origin = CreateChild(player.transform, "GraphicsCardOrigin");
            GameObject assemblyObject = CreateObject("GraphicsCardAssembly");
            Transform snap = CreateChild(
                assemblyObject.transform,
                "GraphicsCardSnapAnchor");
            snap.position = new Vector3(0f, 0f, 1f);
            snap.rotation = Quaternion.Euler(7f, 23f, 11f);

            GameObject focusObject = CreateCube(
                "GraphicsCardSlotFocus",
                new Vector3(0f, 0f, 1f),
                new Vector3(0.30f, 0.12f, 0.04f));
            focusObject.transform.SetParent(assemblyObject.transform, true);
            GameObject supportObject = CreateCube(
                "GraphicsCardPcieSupport",
                new Vector3(0f, 0f, 1.04f),
                new Vector3(0.24f, 0.08f, 0.02f));
            supportObject.transform.SetParent(assemblyObject.transform, true);

            GameObject cardObject = CreateCube(
                "GraphicsCardPhysicalItem",
                new Vector3(3f, 0f, 0f),
                new Vector3(0.30f, 0.12f, 0.06f));
            Rigidbody body = cardObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection card =
                cardObject.AddComponent<PhysicalItemProjection>();
            card.Configure(
                "tests.graphics-card.instance-001",
                "Test PCIe x16 Graphics Card",
                body,
                new Vector3(0.15f, 0.06f, 0.03f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.PcComponent);

            origin.position = Vector3.zero;
            Physics.SyncTransforms();
            Collider focus = focusObject.GetComponent<Collider>();
            origin.rotation = Quaternion.LookRotation(
                focus.bounds.center - origin.position,
                Vector3.up);
            var query = new FakeGraphicsCardSlotPhysics();
            query.SetLineHits(new GraphicsCardPhysicsHit(focus, 1f));
            return new Fixture(
                origin,
                player.transform,
                card,
                snap,
                focus,
                supportObject.GetComponent<Collider>(),
                assemblyObject.transform,
                query);
        }

        private Collider CreateAssemblyBlocker(Transform assemblyRoot, string name)
        {
            GameObject blocker = CreateCube(
                name,
                assemblyRoot.position,
                Vector3.one * 0.05f);
            blocker.transform.SetParent(assemblyRoot, true);
            return blocker.GetComponent<Collider>();
        }

        private Transform CreateChild(Transform parent, string name)
        {
            GameObject child = CreateObject(name);
            child.transform.SetParent(parent, false);
            return child.transform;
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
                PhysicalItemProjection graphicsCard,
                Transform snap,
                Collider focus,
                Collider support,
                Transform assembly,
                FakeGraphicsCardSlotPhysics query)
            {
                Origin = origin;
                Player = player;
                GraphicsCard = graphicsCard;
                Snap = snap;
                Focus = focus;
                Support = support;
                Assembly = assembly;
                Query = query;
            }

            public Transform Origin { get; }

            public Transform Player { get; }

            public PhysicalItemProjection GraphicsCard { get; }

            public Transform Snap { get; }

            public Collider Focus { get; }

            public Collider Support { get; }

            public Transform Assembly { get; }

            public FakeGraphicsCardSlotPhysics Query { get; }

            public GraphicsCardSlotEvaluation Evaluate(
                int halfTurns = 0,
                bool placementModeEnabled = true,
                bool paused = false,
                bool authorityAvailable = true,
                GraphicsCardPcieInterface graphicsCardInterface =
                    GraphicsCardPcieInterface.PcieX16,
                GraphicsCardPcieInterface slotInterface =
                    GraphicsCardPcieInterface.PcieX16,
                bool chassisClearanceAvailable = true,
                bool coolerClearanceAvailable = true,
                IReadOnlyList<Collider> chassisBlockers = null,
                IReadOnlyList<Collider> coolerBlockers = null)
            {
                return GraphicsCardSlotSolver.EvaluateSeat(
                    placementModeEnabled,
                    Origin,
                    Player,
                    GraphicsCard,
                    Snap,
                    Focus,
                    Support,
                    Assembly,
                    1 << 0,
                    2f,
                    0.94f,
                    halfTurns,
                    paused,
                    authorityAvailable,
                    graphicsCardInterface,
                    slotInterface,
                    chassisClearanceAvailable,
                    coolerClearanceAvailable,
                    chassisBlockers,
                    coolerBlockers,
                    Query);
            }
        }

        private sealed class FakeGraphicsCardSlotPhysics :
            IGraphicsCardSlotPhysics
        {
            private GraphicsCardPhysicsHit[] _lineHits =
                Array.Empty<GraphicsCardPhysicsHit>();
            private Collider[] _overlaps = Array.Empty<Collider>();
            private GraphicsCardPhysicsHit[] _boxCastHits =
                Array.Empty<GraphicsCardPhysicsHit>();

            public bool SupportHit { get; set; } = true;

            public int? RaycastCountOverride { get; set; }

            public int? OverlapCountOverride { get; set; }

            public int? BoxCastCountOverride { get; set; }

            public int RaycastQueryCount { get; private set; }

            public int SupportQueryCount { get; private set; }

            public int OverlapQueryCount { get; private set; }

            public int BoxCastQueryCount { get; private set; }

            public int TotalQueryCount =>
                RaycastQueryCount +
                SupportQueryCount +
                OverlapQueryCount +
                BoxCastQueryCount;

            public void SetLineHits(params GraphicsCardPhysicsHit[] hits)
            {
                _lineHits = hits ?? Array.Empty<GraphicsCardPhysicsHit>();
            }

            public void SetOverlaps(params Collider[] colliders)
            {
                _overlaps = colliders ?? Array.Empty<Collider>();
            }

            public void SetBoxCastHits(params GraphicsCardPhysicsHit[] hits)
            {
                _boxCastHits = hits ?? Array.Empty<GraphicsCardPhysicsHit>();
            }

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                GraphicsCardPhysicsHit[] results,
                float maximumDistance,
                int layerMask)
            {
                RaycastQueryCount++;
                Copy(_lineHits, results);
                return RaycastCountOverride ?? _lineHits.Length;
            }

            public bool RaycastCollider(
                Collider collider,
                Ray ray,
                float maximumDistance)
            {
                SupportQueryCount++;
                return SupportHit;
            }

            public int OverlapBoxNonAlloc(
                Vector3 center,
                Vector3 halfExtents,
                Collider[] results,
                Quaternion orientation,
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

            public int BoxCastNonAlloc(
                Vector3 center,
                Vector3 halfExtents,
                Vector3 direction,
                GraphicsCardPhysicsHit[] results,
                Quaternion orientation,
                float maximumDistance,
                int layerMask)
            {
                BoxCastQueryCount++;
                Copy(_boxCastHits, results);
                return BoxCastCountOverride ?? _boxCastHits.Length;
            }

            private static void Copy(
                GraphicsCardPhysicsHit[] source,
                GraphicsCardPhysicsHit[] destination)
            {
                int count = Math.Min(source.Length, destination.Length);
                for (int index = 0; index < count; index++)
                {
                    destination[index] = source[index];
                }
            }
        }
    }
}
