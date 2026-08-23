using System;
using System.Collections.Generic;
using NUnit.Framework;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class PowerSupplyBaySolverTests
    {
        private const string SlotId = "assembly.slot.power-supply-bottom-rear";
        private const string RearMountId = "assembly.mount.power-supply-rear";
        private const string TopLeftFastenerId =
            "assembly.fastener.power-supply-rear-01";
        private const string TopRightFastenerId =
            "assembly.fastener.power-supply-rear-02";
        private const string BottomLeftFastenerId =
            "assembly.fastener.power-supply-rear-03";
        private const string BottomRightFastenerId =
            "assembly.fastener.power-supply-rear-04";

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
            var query = new FakePowerSupplySlotPhysics();

            PowerSupplyBayEvaluation seat = PowerSupplyBaySolver.EvaluateSeat(
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
                PowerSupplyFormFactor.AtxPs2,
                PowerSupplyFormFactor.AtxPs2,
                true,
                true,
                null,
                null,
                query);
            PowerSupplyBayEvaluation interaction =
                PowerSupplyBaySolver.EvaluateInteraction(
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
                    PowerSupplyBayProjectionState.PowerSupplyRetained,
                    true,
                    true,
                    query);

            Assert.That(seat.Status, Is.EqualTo(PowerSupplyBayStatus.ModeDisabled));
            Assert.That(seat.HasPose, Is.False);
            Assert.That(interaction.Status,
                Is.EqualTo(PowerSupplyBayStatus.ModeDisabled));
            Assert.That(query.TotalQueryCount, Is.Zero);
        }

        [Test]
        public void ValidFanToFilteredVentReturnsExactAuthoredPoseForPreviewAndCommit()
        {
            Fixture fixture = CreateFixture();

            PowerSupplyBayEvaluation result = fixture.Evaluate();
            Pose expected = PowerSupplyBaySolver.ResolveSeatPose(fixture.Snap, 0);

            Assert.That(result.Status, Is.EqualTo(PowerSupplyBayStatus.ValidSeat));
            Assert.That(result.CanSeat, Is.True);
            Assert.That(result.HasPose, Is.True);
            Assert.That(result.Orientation,
                Is.EqualTo(PowerSupplySeatOrientation.FanToFilteredVent));
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

            PowerSupplyBayEvaluation primary = fixture.Evaluate();
            PowerSupplyBayEvaluation rotated = fixture.Evaluate(halfTurns: 1);

            Assert.That(rotated.Status,
                Is.EqualTo(PowerSupplyBayStatus.OrientationInvalid));
            Assert.That(rotated.CanSeat, Is.False);
            Assert.That(rotated.HasPose, Is.True);
            Assert.That(rotated.Orientation,
                Is.EqualTo(PowerSupplySeatOrientation.FanAwayFromFilteredVent));
            Assert.That(rotated.Pose.position, Is.EqualTo(primary.Pose.position));
            Assert.That(Quaternion.Angle(
                    primary.Pose.rotation,
                    rotated.Pose.rotation),
                Is.EqualTo(180f).Within(0.001f));
            Assert.That(rotated.FailureCode,
                Is.EqualTo("assembly-power-supply.orientation-mismatch"));
        }

        [Test]
        public void RangeFocusAndMissingLineOfSightFailClosedBeforeVolumeQueries()
        {
            Fixture fixture = CreateFixture();
            fixture.Origin.position = new Vector3(0f, 0f, -3f);

            PowerSupplyBayEvaluation outOfRange = fixture.Evaluate();

            fixture.Origin.position = Vector3.zero;
            fixture.Origin.rotation = Quaternion.LookRotation(
                Vector3.back,
                Vector3.up);
            PowerSupplyBayEvaluation notFocused = fixture.Evaluate();

            fixture.Origin.rotation = Quaternion.LookRotation(
                fixture.Focus.bounds.center - fixture.Origin.position,
                Vector3.up);
            fixture.Query.SetLineHits();
            PowerSupplyBayEvaluation noLineOfSight = fixture.Evaluate();

            Assert.That(outOfRange.Status,
                Is.EqualTo(PowerSupplyBayStatus.OutOfRange));
            Assert.That(notFocused.Status,
                Is.EqualTo(PowerSupplyBayStatus.NotFocused));
            Assert.That(noLineOfSight.Status,
                Is.EqualTo(PowerSupplyBayStatus.LineOfSightBlocked));
            Assert.That(fixture.Query.SupportQueryCount, Is.Zero);
            Assert.That(fixture.Query.OverlapQueryCount, Is.Zero);
            Assert.That(fixture.Query.BoxCastQueryCount, Is.Zero);
        }

        [Test]
        public void ExactDistanceLineOfSightTieFailsClosedInEitherResultOrder()
        {
            Fixture fixture = CreateFixture();
            Collider blocker = CreateCube(
                "PowerSupplyLosTieBlocker",
                new Vector3(0f, 0f, 0.5f),
                Vector3.one * 0.05f).GetComponent<Collider>();

            fixture.Query.SetLineHits(
                new PowerSupplyPhysicsHit(fixture.Focus, 1f),
                new PowerSupplyPhysicsHit(blocker, 1f));
            PowerSupplyBayEvaluation targetFirst = fixture.Evaluate();

            fixture.Query.SetLineHits(
                new PowerSupplyPhysicsHit(blocker, 1f),
                new PowerSupplyPhysicsHit(fixture.Focus, 1f));
            PowerSupplyBayEvaluation blockerFirst = fixture.Evaluate();

            Assert.That(targetFirst.Status,
                Is.EqualTo(PowerSupplyBayStatus.LineOfSightBlocked));
            Assert.That(blockerFirst.Status, Is.EqualTo(targetFirst.Status));
            Assert.That(targetFirst.FailureCode,
                Is.EqualTo("assembly-power-supply.line-of-sight-blocked"));
        }

        [Test]
        public void MissingFilteredFloorSupportFailsBeforeAnySeatVolumeQuery()
        {
            Fixture fixture = CreateFixture();
            fixture.Query.SupportHit = false;

            PowerSupplyBayEvaluation result = fixture.Evaluate();

            Assert.That(result.Status,
                Is.EqualTo(PowerSupplyBayStatus.Unsupported));
            Assert.That(result.FailureCode,
                Is.EqualTo("assembly-power-supply.support-missing"));
            Assert.That(fixture.Query.RaycastQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Query.SupportQueryCount, Is.EqualTo(1));
            Assert.That(fixture.Query.OverlapQueryCount, Is.Zero);
            Assert.That(fixture.Query.BoxCastQueryCount, Is.Zero);
        }

        [Test]
        public void ChassisCableAndGenericObstructionsUseStablePriority()
        {
            Fixture fixture = CreateFixture();
            Collider chassis = CreateAssemblyBlocker(
                fixture.Assembly,
                "PowerSupplyChassisClearance");
            Collider cable = CreateAssemblyBlocker(
                fixture.Assembly,
                "PowerSupplyCableClearance");
            Collider generic = CreateCube(
                "PowerSupplyForeignObstruction",
                fixture.Snap.position,
                Vector3.one * 0.05f).GetComponent<Collider>();
            var chassisBlockers = new[] { chassis };
            var cableBlockers = new[] { cable };

            fixture.Query.SetOverlaps(generic, cable, chassis);
            PowerSupplyBayEvaluation allKinds = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                cableBlockers: cableBlockers);

            fixture.Query.SetOverlaps(generic, cable);
            PowerSupplyBayEvaluation cableAndGeneric = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                cableBlockers: cableBlockers);

            fixture.Query.SetOverlaps(generic);
            PowerSupplyBayEvaluation genericOnly = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                cableBlockers: cableBlockers);

            Assert.That(allKinds.Status,
                Is.EqualTo(PowerSupplyBayStatus.ChassisClearanceBlocked));
            Assert.That(cableAndGeneric.Status,
                Is.EqualTo(PowerSupplyBayStatus.CableClearanceBlocked));
            Assert.That(genericOnly.Status,
                Is.EqualTo(PowerSupplyBayStatus.Obstructed));
        }

        [Test]
        public void StaticClearanceFailurePrecedesSupportAndVolumeQueries()
        {
            Fixture fixture = CreateFixture();

            PowerSupplyBayEvaluation chassis = fixture.Evaluate(
                chassisClearanceAvailable: false,
                cableClearanceAvailable: false);
            PowerSupplyBayEvaluation cable = fixture.Evaluate(
                chassisClearanceAvailable: true,
                cableClearanceAvailable: false);

            Assert.That(chassis.Status,
                Is.EqualTo(PowerSupplyBayStatus.ChassisClearanceBlocked));
            Assert.That(cable.Status,
                Is.EqualTo(PowerSupplyBayStatus.CableClearanceBlocked));
            Assert.That(fixture.Query.SupportQueryCount, Is.Zero);
            Assert.That(fixture.Query.OverlapQueryCount, Is.Zero);
            Assert.That(fixture.Query.BoxCastQueryCount, Is.Zero);
        }

        [Test]
        public void OverlapAndBoxCastSaturationFailClosed()
        {
            Fixture fixture = CreateFixture();
            fixture.Query.OverlapCountOverride = PowerSupplyBaySolver.HitCapacity;

            PowerSupplyBayEvaluation overlapSaturated = fixture.Evaluate();

            fixture.Query.OverlapCountOverride = null;
            fixture.Query.BoxCastCountOverride = PowerSupplyBaySolver.HitCapacity;
            PowerSupplyBayEvaluation boxCastSaturated = fixture.Evaluate();

            Assert.That(overlapSaturated.Status,
                Is.EqualTo(PowerSupplyBayStatus.Obstructed));
            Assert.That(boxCastSaturated.Status,
                Is.EqualTo(PowerSupplyBayStatus.Obstructed));
            Assert.That(overlapSaturated.CanSeat, Is.False);
            Assert.That(boxCastSaturated.CanSeat, Is.False);
        }

        [Test]
        public void ObstructionDecisionIsIndependentOfBufferAndResultOrder()
        {
            Fixture fixture = CreateFixture();
            Collider chassis = CreateAssemblyBlocker(
                fixture.Assembly,
                "PowerSupplyOrderedChassisClearance");
            Collider cable = CreateAssemblyBlocker(
                fixture.Assembly,
                "PowerSupplyOrderedCableClearance");
            Collider generic = CreateCube(
                "PowerSupplyOrderedForeignObstruction",
                fixture.Snap.position,
                Vector3.one * 0.05f).GetComponent<Collider>();
            var chassisBlockers = new[] { chassis };
            var cableBlockers = new[] { cable };

            fixture.Query.SetOverlaps(generic, cable);
            fixture.Query.SetBoxCastHits(new PowerSupplyPhysicsHit(chassis, 0.05f));
            PowerSupplyBayEvaluation overlapFirst = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                cableBlockers: cableBlockers);

            fixture.Query.SetOverlaps(chassis);
            fixture.Query.SetBoxCastHits(
                new PowerSupplyPhysicsHit(cable, 0.02f),
                new PowerSupplyPhysicsHit(generic, 0.01f));
            PowerSupplyBayEvaluation castFirst = fixture.Evaluate(
                chassisBlockers: chassisBlockers,
                cableBlockers: cableBlockers);

            Assert.That(overlapFirst.Status,
                Is.EqualTo(PowerSupplyBayStatus.ChassisClearanceBlocked));
            Assert.That(castFirst.Status, Is.EqualTo(overlapFirst.Status));
            Assert.That(castFirst.FailureCode, Is.EqualTo(overlapFirst.FailureCode));
        }

        [Test]
        public void ProjectionOwnsDistinctIdentityExactPoseAndVisualState()
        {
            Fixture fixture = CreateFixture();
            Transform[] fasteners =
            {
                CreateChild(fixture.Assembly, "PowerSupplyFastenerTopLeft"),
                CreateChild(fixture.Assembly, "PowerSupplyFastenerTopRight"),
                CreateChild(fixture.Assembly, "PowerSupplyFastenerBottomLeft"),
                CreateChild(fixture.Assembly, "PowerSupplyFastenerBottomRight")
            };
            var openPositions = new Vector3[fasteners.Length];
            var openRotations = new Quaternion[fasteners.Length];
            for (int index = 0; index < fasteners.Length; index++)
            {
                fasteners[index].localPosition = new Vector3(
                    index * 0.01f,
                    0.01f,
                    0f);
                fasteners[index].localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    index * 10f);
                openPositions[index] = fasteners[index].localPosition;
                openRotations[index] = fasteners[index].localRotation;
            }

            PowerSupplyBayProjection projection =
                fixture.Assembly.gameObject.AddComponent<PowerSupplyBayProjection>();

            projection.Configure(
                SlotId,
                RearMountId,
                TopLeftFastenerId,
                TopRightFastenerId,
                BottomLeftFastenerId,
                BottomRightFastenerId,
                fixture.Snap,
                fixture.Focus,
                fixture.Support,
                fixture.Assembly,
                fasteners[0],
                fasteners[1],
                fasteners[2],
                fasteners[3]);

            Assert.That(projection.IsConfigured, Is.True);
            Assert.That(projection.SlotIdValue, Is.EqualTo(SlotId));
            Assert.That(projection.RearMountIdValue, Is.EqualTo(RearMountId));
            Assert.That(projection.TopLeftFastenerIdValue,
                Is.EqualTo(TopLeftFastenerId));
            Assert.That(projection.TopRightFastenerIdValue,
                Is.EqualTo(TopRightFastenerId));
            Assert.That(projection.BottomLeftFastenerIdValue,
                Is.EqualTo(BottomLeftFastenerId));
            Assert.That(projection.BottomRightFastenerIdValue,
                Is.EqualTo(BottomRightFastenerId));
            Assert.That(projection.BayFormFactor,
                Is.EqualTo(PowerSupplyFormFactor.AtxPs2));
            Assert.That(projection.FocusCollider.enabled, Is.True);

            var projectionPose = projection.ResolveSeatPose(0);
            Pose solverPose = PowerSupplyBaySolver.ResolveSeatPose(
                fixture.Snap,
                0);
            Assert.That(projectionPose.IsSuccess, Is.True);
            Assert.That(projectionPose.Value.position, Is.EqualTo(solverPose.position));
            Assert.That(Quaternion.Angle(
                    projectionPose.Value.rotation,
                    solverPose.rotation),
                Is.LessThan(0.001f));

            projection.ApplyAuthoritativeState(
                PowerSupplyBayProjectionState.PowerSupplySeatedUnsecured);
            Assert.That(projection.FocusCollider.enabled, Is.True);
            for (int index = 0; index < fasteners.Length; index++)
            {
                Assert.That(fasteners[index].localPosition,
                    Is.EqualTo(openPositions[index]));
                Assert.That(Quaternion.Angle(
                        fasteners[index].localRotation,
                        openRotations[index]),
                    Is.LessThan(0.001f));
            }

            projection.ApplyAuthoritativeState(
                PowerSupplyBayProjectionState.PowerSupplyRetained);
            Assert.That(projection.MatchesLogicalAuthorityState(
                PowerSupplyBayProjectionState.PowerSupplyRetained), Is.True);
            for (int index = 0; index < fasteners.Length; index++)
            {
                Assert.That(fasteners[index].localPosition.z,
                    Is.EqualTo(openPositions[index].z + 0.003f).Within(0.00001f));
                Assert.That(Quaternion.Angle(
                        fasteners[index].localRotation,
                        openRotations[index] *
                        Quaternion.AngleAxis(120f, Vector3.forward)),
                    Is.LessThan(0.001f));
            }

            PowerSupplyBayEvaluation blockedFeedback =
                projection.ApplyAuthoritativeInteractionFeedback(
                    PowerSupplyBayProjectionState.PowerSupplySeatedUnsecured);
            Assert.That(blockedFeedback.Status,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeatedUnsecured));

            Assert.Throws<ArgumentException>(() => projection.Configure(
                SlotId,
                SlotId,
                TopLeftFastenerId,
                TopRightFastenerId,
                BottomLeftFastenerId,
                BottomRightFastenerId,
                fixture.Snap,
                fixture.Focus,
                fixture.Support,
                fixture.Assembly,
                fasteners[0],
                fasteners[1],
                fasteners[2],
                fasteners[3]));
        }

        private Fixture CreateFixture()
        {
            GameObject player = CreateObject("PowerSupplyPlayer");
            Transform origin = CreateChild(player.transform, "PowerSupplyOrigin");
            GameObject assemblyObject = CreateObject("PowerSupplyAssembly");
            Transform snap = CreateChild(
                assemblyObject.transform,
                "PowerSupplySnapAnchor");
            snap.position = new Vector3(0f, 0f, 1f);
            snap.rotation = Quaternion.Euler(7f, 23f, 11f);

            GameObject focusObject = CreateCube(
                "PowerSupplySlotFocus",
                new Vector3(0f, 0f, 1f),
                new Vector3(0.30f, 0.12f, 0.04f));
            focusObject.transform.SetParent(assemblyObject.transform, true);
            GameObject supportObject = CreateCube(
                "PowerSupplyFilteredFloorSupport",
                new Vector3(0f, 0f, 1.04f),
                new Vector3(0.24f, 0.08f, 0.02f));
            supportObject.transform.SetParent(assemblyObject.transform, true);

            GameObject powerSupplyObject = CreateCube(
                "PowerSupplyPhysicalItem",
                new Vector3(3f, 0f, 0f),
                new Vector3(0.15f, 0.086f, 0.14f));
            Rigidbody body = powerSupplyObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection powerSupply =
                powerSupplyObject.AddComponent<PhysicalItemProjection>();
            powerSupply.Configure(
                "tests.power-supply.instance-001",
                "Test ATX PS/2 Power Supply",
                body,
                new Vector3(0.075f, 0.043f, 0.07f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.PcComponent);

            origin.position = Vector3.zero;
            Physics.SyncTransforms();
            Collider focus = focusObject.GetComponent<Collider>();
            origin.rotation = Quaternion.LookRotation(
                focus.bounds.center - origin.position,
                Vector3.up);
            var query = new FakePowerSupplySlotPhysics();
            query.SetLineHits(new PowerSupplyPhysicsHit(focus, 1f));
            return new Fixture(
                origin,
                player.transform,
                powerSupply,
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
                PhysicalItemProjection powerSupply,
                Transform snap,
                Collider focus,
                Collider support,
                Transform assembly,
                FakePowerSupplySlotPhysics query)
            {
                Origin = origin;
                Player = player;
                PowerSupply = powerSupply;
                Snap = snap;
                Focus = focus;
                Support = support;
                Assembly = assembly;
                Query = query;
            }

            public Transform Origin { get; }

            public Transform Player { get; }

            public PhysicalItemProjection PowerSupply { get; }

            public Transform Snap { get; }

            public Collider Focus { get; }

            public Collider Support { get; }

            public Transform Assembly { get; }

            public FakePowerSupplySlotPhysics Query { get; }

            public PowerSupplyBayEvaluation Evaluate(
                int halfTurns = 0,
                bool placementModeEnabled = true,
                bool paused = false,
                bool authorityAvailable = true,
                PowerSupplyFormFactor powerSupplyInterface =
                    PowerSupplyFormFactor.AtxPs2,
                PowerSupplyFormFactor slotInterface =
                    PowerSupplyFormFactor.AtxPs2,
                bool chassisClearanceAvailable = true,
                bool cableClearanceAvailable = true,
                IReadOnlyList<Collider> chassisBlockers = null,
                IReadOnlyList<Collider> cableBlockers = null)
            {
                return PowerSupplyBaySolver.EvaluateSeat(
                    placementModeEnabled,
                    Origin,
                    Player,
                    PowerSupply,
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
                    powerSupplyInterface,
                    slotInterface,
                    chassisClearanceAvailable,
                    cableClearanceAvailable,
                    chassisBlockers,
                    cableBlockers,
                    Query);
            }
        }

        private sealed class FakePowerSupplySlotPhysics :
            IPowerSupplyBayPhysics
        {
            private PowerSupplyPhysicsHit[] _lineHits =
                Array.Empty<PowerSupplyPhysicsHit>();
            private Collider[] _overlaps = Array.Empty<Collider>();
            private PowerSupplyPhysicsHit[] _boxCastHits =
                Array.Empty<PowerSupplyPhysicsHit>();

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

            public void SetLineHits(params PowerSupplyPhysicsHit[] hits)
            {
                _lineHits = hits ?? Array.Empty<PowerSupplyPhysicsHit>();
            }

            public void SetOverlaps(params Collider[] colliders)
            {
                _overlaps = colliders ?? Array.Empty<Collider>();
            }

            public void SetBoxCastHits(params PowerSupplyPhysicsHit[] hits)
            {
                _boxCastHits = hits ?? Array.Empty<PowerSupplyPhysicsHit>();
            }

            public int RaycastNonAlloc(
                Vector3 origin,
                Vector3 direction,
                PowerSupplyPhysicsHit[] results,
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
                PowerSupplyPhysicsHit[] results,
                Quaternion orientation,
                float maximumDistance,
                int layerMask)
            {
                BoxCastQueryCount++;
                Copy(_boxCastHits, results);
                return BoxCastCountOverride ?? _boxCastHits.Length;
            }

            private static void Copy(
                PowerSupplyPhysicsHit[] source,
                PowerSupplyPhysicsHit[] destination)
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
