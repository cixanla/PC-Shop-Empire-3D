using System;
using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class DimmSlotSolverTests
    {
        [Test]
        public void KeyedSeatAcceptsOnlyNotchAlignedAndIdentifiesReversedModule()
        {
            using var fixture = new Fixture();

            DimmSlotEvaluation aligned = fixture.EvaluateSeat();
            DimmSlotEvaluation sideways = fixture.EvaluateSeat(quarterTurns: 1);
            DimmSlotEvaluation reversed = fixture.EvaluateSeat(quarterTurns: 2);

            Assert.That(aligned.Status, Is.EqualTo(DimmSlotStatus.ValidSeat));
            Assert.That(aligned.CanSeat, Is.True);
            Assert.That(aligned.Orientation,
                Is.EqualTo(DimmKeyOrientation.NotchAligned));
            Assert.That(aligned.Pose.position, Is.EqualTo(fixture.Snap.position));
            Assert.That(aligned.Pose.rotation, Is.EqualTo(fixture.Snap.rotation));
            Assert.That(sideways.Status, Is.EqualTo(DimmSlotStatus.OrientationInvalid));
            Assert.That(sideways.Orientation, Is.EqualTo(default(DimmKeyOrientation)));
            Assert.That(sideways.FailureCode,
                Is.EqualTo(AssemblyFailures.InvalidDimmOrientation.Code));
            Assert.That(reversed.Status, Is.EqualTo(DimmSlotStatus.OrientationInvalid));
            Assert.That(reversed.Orientation, Is.EqualTo(DimmKeyOrientation.Reversed));
            Assert.That(reversed.FailureCode,
                Is.EqualTo(AssemblyFailures.DimmOrientationMismatch.Code));
            Assert.That(Quaternion.Angle(
                reversed.Pose.rotation,
                fixture.Snap.rotation * Quaternion.AngleAxis(180f, Vector3.forward)),
                Is.LessThan(0.001f));
        }

        [Test]
        public void PauseAuthorityRangeAndFocusFailClosedWithoutLosingCandidatePose()
        {
            using var fixture = new Fixture();

            Assert.That(fixture.EvaluateSeat(paused: true).Status,
                Is.EqualTo(DimmSlotStatus.Paused));
            Assert.That(fixture.EvaluateSeat(authorityAvailable: false).Status,
                Is.EqualTo(DimmSlotStatus.AuthorityBlocked));

            fixture.Origin.position = new Vector3(0f, 0f, -3f);
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(DimmSlotStatus.OutOfRange));

            fixture.Origin.position = Vector3.zero;
            fixture.Origin.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            DimmSlotEvaluation notFocused = fixture.EvaluateSeat();
            Assert.That(notFocused.Status, Is.EqualTo(DimmSlotStatus.NotFocused));
            Assert.That(notFocused.HasPose, Is.True);
        }

        [Test]
        public void SharedPhysicsRejectsLineOfSightAndForeignInsertionObstruction()
        {
            using var fixture = new Fixture();
            fixture.Focus.isTrigger = true;
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(DimmSlotStatus.LineOfSightBlocked));

            fixture.Focus.isTrigger = false;
            fixture.CreateSeatBlocker();
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(DimmSlotStatus.Obstructed));
        }

        [Test]
        public void SlotInteractionUsesAuthorityStateAndRetainedRemovalGate()
        {
            using var fixture = new Fixture();
            fixture.Memory.transform.position = new Vector3(0f, 0f, 0.82f);
            Physics.SyncTransforms();

            DimmSlotEvaluation seated = fixture.EvaluateInteraction(
                MemorySlotState.MemoryModuleSeatedOpen);
            DimmSlotEvaluation retained = fixture.EvaluateInteraction(
                MemorySlotState.MemoryModuleRetained);
            DimmSlotEvaluation unsecuredOpen = fixture.EvaluateInteraction(
                MemorySlotState.MemoryModuleSeatedOpen,
                retentionCloseAvailable: false);
            DimmSlotEvaluation empty = fixture.EvaluateInteraction(
                MemorySlotState.EmptyOpen);

            Assert.That(seated.Status, Is.EqualTo(DimmSlotStatus.ValidSeatedOpen));
            Assert.That(seated.CanOperateRetention, Is.True);
            Assert.That(seated.CanRemove, Is.True);
            Assert.That(retained.Status, Is.EqualTo(DimmSlotStatus.ValidRetained));
            Assert.That(retained.CanOperateRetention, Is.True);
            Assert.That(retained.CanRemove, Is.False);
            Assert.That(unsecuredOpen.Status,
                Is.EqualTo(DimmSlotStatus.ValidSeatedOpenRetentionBlocked));
            Assert.That(unsecuredOpen.CanOperateRetention, Is.False);
            Assert.That(unsecuredOpen.CanRemove, Is.True);
            Assert.That(unsecuredOpen.FailureCode,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured.Code));
            Assert.That(empty.Status, Is.EqualTo(DimmSlotStatus.AuthorityBlocked));
        }

        [Test]
        public void ProjectionOwnsDualLatchShapeButNoPrivateGhostRenderer()
        {
            var root = new GameObject("DimmSlotProjectionTest");
            var snap = new GameObject("Snap").transform;
            snap.SetParent(root.transform, false);
            var focusObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            focusObject.name = "Focus";
            focusObject.transform.SetParent(root.transform, false);
            var leftLatch = new GameObject("LeftLatchPivot").transform;
            leftLatch.SetParent(root.transform, false);
            leftLatch.localRotation = Quaternion.Euler(28f, 0f, 0f);
            var rightLatch = new GameObject("RightLatchPivot").transform;
            rightLatch.SetParent(root.transform, false);
            rightLatch.localRotation = Quaternion.Euler(-28f, 0f, 0f);
            var projection = root.AddComponent<DimmSlotProjection>();
            try
            {
                Quaternion openLeft = leftLatch.localRotation;
                Quaternion openRight = rightLatch.localRotation;
                projection.Configure(
                    GarageStockFlowSession.MemorySlotIdValue,
                    GarageStockFlowSession.MemoryRetentionIdValue,
                    GarageStockFlowSession.MemoryChannelIdValue,
                    GarageStockFlowSession.MemoryBankIdValue,
                    snap,
                    focusObject.GetComponent<Collider>(),
                    root.transform,
                    leftLatch,
                    rightLatch);

                Assert.That(projection.IsConfigured, Is.True);
                Assert.That(typeof(DimmSlotProjection).GetField(
                    "ghostRenderer",
                    BindingFlags.Instance | BindingFlags.NonPublic), Is.Null);
                Assert.That(projection.FocusCollider.enabled, Is.False);
                Assert.That(projection.ChannelIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryChannelIdValue));
                Assert.That(projection.BankIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryBankIdValue));

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleSeatedOpen);
                Assert.That(projection.FocusCollider.enabled, Is.True);
                Assert.That(Quaternion.Angle(leftLatch.localRotation, openLeft),
                    Is.LessThan(0.001f));
                Assert.That(Quaternion.Angle(rightLatch.localRotation, openRight),
                    Is.LessThan(0.001f));

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleRetained);
                Assert.That(projection.LatchVisualPhase,
                    Is.EqualTo(DimmLatchVisualPhase.ClosingLeft));
                Assert.That(projection.MatchesLogicalAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleRetained), Is.True);
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleRetained), Is.False);
                rightLatch.localRotation = Quaternion.identity;
                Assert.That(projection.MatchesLogicalAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleRetained), Is.False);
                rightLatch.localRotation = openRight;
                projection.AdvanceLatchAnimation(0.04f);
                Assert.That(Quaternion.Angle(leftLatch.localRotation, openLeft),
                    Is.GreaterThan(0.001f));
                Assert.That(Quaternion.Angle(rightLatch.localRotation, openRight),
                    Is.LessThan(0.001f));
                projection.AdvanceLatchAnimation(0.10f);
                Assert.That(Quaternion.Angle(leftLatch.localRotation, Quaternion.identity),
                    Is.LessThan(0.001f));
                Assert.That(projection.LatchVisualPhase,
                    Is.EqualTo(DimmLatchVisualPhase.ClosingRight));
                Assert.That(Quaternion.Angle(rightLatch.localRotation, openRight),
                    Is.LessThan(0.001f));
                projection.AdvanceLatchAnimation(0.10f);
                Assert.That(Quaternion.Angle(rightLatch.localRotation, Quaternion.identity),
                    Is.LessThan(0.001f));
                Assert.That(projection.LatchVisualPhase,
                    Is.EqualTo(DimmLatchVisualPhase.Stable));
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleRetained), Is.True);
                leftLatch.localRotation = openLeft;
                Assert.That(projection.MatchesLogicalAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleRetained), Is.False);
                leftLatch.localRotation = Quaternion.identity;

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleSeatedOpen);
                Assert.That(projection.LatchVisualPhase,
                    Is.EqualTo(DimmLatchVisualPhase.OpeningRight));
                Assert.That(projection.MatchesLogicalAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleSeatedOpen), Is.True);
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleSeatedOpen), Is.False);
                projection.AdvanceLatchAnimation(0.10f);
                Assert.That(Quaternion.Angle(rightLatch.localRotation, openRight),
                    Is.LessThan(0.001f));
                Assert.That(Quaternion.Angle(leftLatch.localRotation, Quaternion.identity),
                    Is.LessThan(0.001f));
                Assert.That(projection.LatchVisualPhase,
                    Is.EqualTo(DimmLatchVisualPhase.OpeningLeft));
                projection.AdvanceLatchAnimation(0.10f);
                Assert.That(Quaternion.Angle(leftLatch.localRotation, openLeft),
                    Is.LessThan(0.001f));
                Assert.That(projection.LatchVisualPhase,
                    Is.EqualTo(DimmLatchVisualPhase.Stable));
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    MemorySlotState.MemoryModuleSeatedOpen), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void ConfigureRejectsAliasedLatchPivots()
        {
            var root = new GameObject("DimmAliasedLatchTest");
            var snap = new GameObject("Snap").transform;
            snap.SetParent(root.transform, false);
            var focusObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            focusObject.transform.SetParent(root.transform, false);
            var sharedLatch = new GameObject("SharedLatchPivot").transform;
            sharedLatch.SetParent(root.transform, false);
            var projection = root.AddComponent<DimmSlotProjection>();
            try
            {
                Assert.Throws<ArgumentException>(() => projection.Configure(
                    GarageStockFlowSession.MemorySlotIdValue,
                    GarageStockFlowSession.MemoryRetentionIdValue,
                    GarageStockFlowSession.MemoryChannelIdValue,
                    GarageStockFlowSession.MemoryBankIdValue,
                    snap,
                    focusObject.GetComponent<Collider>(),
                    root.transform,
                    sharedLatch,
                    sharedLatch));
                Assert.That(projection.IsConfigured, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PublicFailureCodesRemainStable()
        {
            Assert.That(Evaluation(DimmSlotStatus.ContextMissing).FailureCode,
                Is.EqualTo("assembly-memory.context-missing"));
            Assert.That(Evaluation(DimmSlotStatus.Paused).FailureCode,
                Is.EqualTo("assembly-memory.paused"));
            Assert.That(Evaluation(DimmSlotStatus.AuthorityBlocked).FailureCode,
                Is.EqualTo("assembly-memory.authority-blocked"));
            Assert.That(Evaluation(
                    DimmSlotStatus.ValidSeatedOpenRetentionBlocked).FailureCode,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured.Code));
            Assert.That(Evaluation(DimmSlotStatus.OutOfRange).FailureCode,
                Is.EqualTo("assembly-memory.out-of-range"));
            Assert.That(Evaluation(DimmSlotStatus.NotFocused).FailureCode,
                Is.EqualTo("assembly-memory.focus-missing"));
            Assert.That(Evaluation(DimmSlotStatus.LineOfSightBlocked).FailureCode,
                Is.EqualTo("assembly-memory.line-of-sight-blocked"));
            Assert.That(Evaluation(DimmSlotStatus.Obstructed).FailureCode,
                Is.EqualTo("assembly-memory.obstructed"));
            Assert.That(Evaluation(DimmSlotStatus.ValidSeat).FailureCode, Is.Empty);
        }

        private static DimmSlotEvaluation Evaluation(DimmSlotStatus status)
        {
            return new DimmSlotEvaluation(status, default, false, default);
        }

        private sealed class Fixture : IDisposable
        {
            private readonly GameObject _player;
            private readonly GameObject _origin;
            private readonly GameObject _assembly;
            private readonly GameObject _snap;
            private readonly GameObject _focus;
            private readonly GameObject _memory;
            private GameObject _blocker;

            public Fixture()
            {
                _player = new GameObject("DimmPlayer");
                _origin = new GameObject("DimmOrigin");
                _origin.transform.SetParent(_player.transform, false);
                _origin.transform.rotation = Quaternion.identity;
                _assembly = new GameObject("DimmAssembly");
                _snap = new GameObject("DimmSnap");
                _snap.transform.SetParent(_assembly.transform, false);
                _snap.transform.position = new Vector3(0f, 0f, 1f);
                _snap.transform.rotation = Quaternion.identity;
                _focus = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _focus.name = "DimmFocus";
                _focus.transform.SetParent(_assembly.transform, false);
                _focus.transform.position = new Vector3(0f, 0f, 1f);
                _focus.transform.localScale = new Vector3(0.14f, 0.04f, 0.02f);
                _memory = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _memory.name = "DimmItem";
                _memory.transform.position = new Vector3(3f, 0f, 0f);
                _memory.transform.localScale = Vector3.one;
                _memory.GetComponent<BoxCollider>().size =
                    new Vector3(0.13f, 0.03f, 0.008f);
                Rigidbody body = _memory.AddComponent<Rigidbody>();
                body.useGravity = false;
                body.isKinematic = true;
                Memory = _memory.AddComponent<PhysicalItemProjection>();
                Memory.Configure(
                    GarageStockFlowSession.MemoryItemInstanceIdValue,
                    GarageStockFlowSession.MemoryDisplayName,
                    body,
                    new Vector3(0.065f, 0.015f, 0.004f),
                    Vector3.zero,
                    Vector3.zero,
                    PhysicalCarryProfile.PcComponent);
                Physics.SyncTransforms();
            }

            public Transform Origin => _origin.transform;

            public Transform Snap => _snap.transform;

            public Collider Focus => _focus.GetComponent<Collider>();

            public PhysicalItemProjection Memory { get; }

            public DimmSlotEvaluation EvaluateSeat(
                int quarterTurns = 0,
                bool paused = false,
                bool authorityAvailable = true)
            {
                Physics.SyncTransforms();
                return DimmSlotSolver.EvaluateSeat(
                    Origin,
                    _player.transform,
                    Memory,
                    Snap,
                    Focus,
                    _assembly.transform,
                    1 << 0,
                    2f,
                    0.94f,
                    quarterTurns,
                    paused,
                    authorityAvailable);
            }

            public DimmSlotEvaluation EvaluateInteraction(
                MemorySlotState state,
                bool retentionCloseAvailable = true)
            {
                Physics.SyncTransforms();
                return DimmSlotSolver.EvaluateInteraction(
                    Origin,
                    _player.transform,
                    Memory.transform,
                    Focus,
                    _assembly.transform,
                    1 << 0,
                    2f,
                    0.94f,
                    false,
                    state,
                    true,
                    retentionCloseAvailable);
            }

            public void CreateSeatBlocker()
            {
                _blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _blocker.name = "DimmSeatBlocker";
                _blocker.transform.position = Snap.position + new Vector3(0.075f, 0f, 0f);
                _blocker.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                Physics.SyncTransforms();
            }

            public void Dispose()
            {
                if (_blocker != null)
                {
                    UnityEngine.Object.DestroyImmediate(_blocker);
                }

                UnityEngine.Object.DestroyImmediate(_memory);
                UnityEngine.Object.DestroyImmediate(_assembly);
                UnityEngine.Object.DestroyImmediate(_player);
            }
        }
    }
}
