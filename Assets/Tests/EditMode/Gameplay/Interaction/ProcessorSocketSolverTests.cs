using System;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class ProcessorSocketSolverTests
    {
        [Test]
        public void KeyedSeatAcceptsOnlyCanonicalRotationAndPreservesExactPose()
        {
            using var fixture = new Fixture();

            ProcessorSocketEvaluation valid = fixture.EvaluateSeat();
            ProcessorSocketEvaluation rotated = fixture.EvaluateSeat(quarterTurns: 1);

            Assert.That(valid.Status, Is.EqualTo(ProcessorSocketStatus.ValidSeat));
            Assert.That(valid.CanSeat, Is.True);
            Assert.That(valid.Pose.position, Is.EqualTo(fixture.Snap.position));
            Assert.That(valid.Pose.rotation, Is.EqualTo(fixture.Snap.rotation));
            Assert.That(rotated.Status,
                Is.EqualTo(ProcessorSocketStatus.OrientationInvalid));
            Assert.That(rotated.CanSeat, Is.False);
            Assert.That(rotated.HasPose, Is.True);
            Assert.That(Quaternion.Angle(
                rotated.Pose.rotation,
                fixture.Snap.rotation * Quaternion.AngleAxis(90f, Vector3.forward)),
                Is.LessThan(0.001f));
        }

        [Test]
        public void PauseAuthorityRangeAndFocusFailClosedWithoutLosingCandidatePose()
        {
            using var fixture = new Fixture();

            Assert.That(fixture.EvaluateSeat(paused: true).Status,
                Is.EqualTo(ProcessorSocketStatus.Paused));
            Assert.That(fixture.EvaluateSeat(authorityAvailable: false).Status,
                Is.EqualTo(ProcessorSocketStatus.AuthorityBlocked));

            fixture.Origin.position = new Vector3(0f, 0f, -3f);
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(ProcessorSocketStatus.OutOfRange));

            fixture.Origin.position = Vector3.zero;
            fixture.Origin.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            ProcessorSocketEvaluation notFocused = fixture.EvaluateSeat();
            Assert.That(notFocused.Status,
                Is.EqualTo(ProcessorSocketStatus.NotFocused));
            Assert.That(notFocused.HasPose, Is.True);
        }

        [Test]
        public void LineOfSightAndForeignSeatObstructionAreDeterministic()
        {
            using var fixture = new Fixture();
            fixture.Focus.isTrigger = true;
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(ProcessorSocketStatus.LineOfSightBlocked));

            fixture.Focus.isTrigger = false;
            fixture.CreateSeatBlocker();
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(ProcessorSocketStatus.Obstructed));
        }

        [Test]
        public void SocketInteractionUsesAuthorityStateAndIgnoresSeatedProcessorCollider()
        {
            using var fixture = new Fixture();
            fixture.Processor.transform.position = new Vector3(0f, 0f, 0.82f);
            Physics.SyncTransforms();

            ProcessorSocketEvaluation seated = fixture.EvaluateInteraction(
                ProcessorSocketState.ProcessorSeatedOpen);
            ProcessorSocketEvaluation retained = fixture.EvaluateInteraction(
                ProcessorSocketState.ProcessorRetained);
            ProcessorSocketEvaluation unsecuredOpen = fixture.EvaluateInteraction(
                ProcessorSocketState.ProcessorSeatedOpen,
                retentionCloseAvailable: false);
            ProcessorSocketEvaluation empty = fixture.EvaluateInteraction(
                ProcessorSocketState.EmptyOpen);

            Assert.That(seated.Status,
                Is.EqualTo(ProcessorSocketStatus.ValidSeatedOpen));
            Assert.That(seated.CanOperateRetention, Is.True);
            Assert.That(seated.CanRemove, Is.True);
            Assert.That(retained.Status,
                Is.EqualTo(ProcessorSocketStatus.ValidRetained));
            Assert.That(retained.CanOperateRetention, Is.True);
            Assert.That(retained.CanRemove, Is.False);
            Assert.That(unsecuredOpen.Status,
                Is.EqualTo(ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked));
            Assert.That(unsecuredOpen.CanOperateRetention, Is.False);
            Assert.That(unsecuredOpen.CanRemove, Is.True);
            Assert.That(unsecuredOpen.FailureCode,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured.Code));
            Assert.That(empty.Status,
                Is.EqualTo(ProcessorSocketStatus.AuthorityBlocked));
        }

        [Test]
        public void ProjectionSupportsSharedPreviewAndMatchesOpenClosedShapeState()
        {
            var root = new GameObject("ProcessorSocketProjectionTest");
            var snap = new GameObject("Snap").transform;
            snap.SetParent(root.transform, false);
            var focusObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            focusObject.name = "Focus";
            focusObject.transform.SetParent(root.transform, false);
            var loadPlate = new GameObject("LoadPlatePivot").transform;
            loadPlate.SetParent(root.transform, false);
            loadPlate.localRotation = Quaternion.Euler(-72f, 0f, 0f);
            var lever = new GameObject("RetentionLeverPivot").transform;
            lever.SetParent(root.transform, false);
            lever.localRotation = Quaternion.Euler(-35f, 0f, 0f);
            var projection = root.AddComponent<ProcessorSocketProjection>();
            try
            {
                Quaternion openPlate = loadPlate.localRotation;
                Quaternion openLever = lever.localRotation;
                projection.Configure(
                    GarageStockFlowSession.ProcessorSlotIdValue,
                    GarageStockFlowSession.ProcessorRetentionIdValue,
                    snap,
                    focusObject.GetComponent<Collider>(),
                    root.transform,
                    loadPlate,
                    lever,
                    null,
                    null,
                    null);

                Assert.That(projection.IsConfigured, Is.True);
                Assert.That(projection.GhostRenderer, Is.Null);
                Assert.That(projection.FocusCollider.enabled, Is.False);
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.Empty,
                    ProcessorSocketState.EmptyOpen), Is.True);

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedUnsecured,
                    ProcessorSocketState.EmptyOpen);
                Assert.That(projection.FocusCollider.enabled, Is.True);

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedUnsecured,
                    ProcessorSocketState.ProcessorSeatedOpen);
                Assert.That(projection.FocusCollider.enabled, Is.True);

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    ProcessorSocketState.ProcessorSeatedOpen);
                Assert.That(projection.FocusCollider.enabled, Is.True);
                Assert.That(Quaternion.Angle(loadPlate.localRotation, openPlate),
                    Is.LessThan(0.001f));
                Assert.That(Quaternion.Angle(lever.localRotation, openLever),
                    Is.LessThan(0.001f));

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    ProcessorSocketState.ProcessorRetained);
                Assert.That(Quaternion.Angle(loadPlate.localRotation, Quaternion.identity),
                    Is.LessThan(0.001f));
                Assert.That(Quaternion.Angle(lever.localRotation, Quaternion.identity),
                    Is.LessThan(0.001f));
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    ProcessorSocketState.ProcessorRetained), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PublicFailureCodesRemainStable()
        {
            Assert.That(Evaluation(ProcessorSocketStatus.ContextMissing).FailureCode,
                Is.EqualTo("assembly-processor.context-missing"));
            Assert.That(Evaluation(ProcessorSocketStatus.Paused).FailureCode,
                Is.EqualTo("assembly-processor.paused"));
            Assert.That(Evaluation(ProcessorSocketStatus.AuthorityBlocked).FailureCode,
                Is.EqualTo("assembly-processor.authority-blocked"));
            Assert.That(Evaluation(
                    ProcessorSocketStatus.ValidSeatedOpenRetentionBlocked).FailureCode,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured.Code));
            Assert.That(Evaluation(ProcessorSocketStatus.OutOfRange).FailureCode,
                Is.EqualTo("assembly-processor.out-of-range"));
            Assert.That(Evaluation(ProcessorSocketStatus.NotFocused).FailureCode,
                Is.EqualTo("assembly-processor.focus-missing"));
            Assert.That(Evaluation(ProcessorSocketStatus.LineOfSightBlocked).FailureCode,
                Is.EqualTo("assembly-processor.line-of-sight-blocked"));
            Assert.That(Evaluation(ProcessorSocketStatus.OrientationInvalid).FailureCode,
                Is.EqualTo("assembly-processor.orientation-invalid"));
            Assert.That(Evaluation(ProcessorSocketStatus.Obstructed).FailureCode,
                Is.EqualTo("assembly-processor.obstructed"));
            Assert.That(Evaluation(ProcessorSocketStatus.ValidSeat).FailureCode, Is.Empty);
        }

        private static ProcessorSocketEvaluation Evaluation(ProcessorSocketStatus status)
        {
            return new ProcessorSocketEvaluation(status, default, false);
        }

        private sealed class Fixture : IDisposable
        {
            private readonly GameObject _player;
            private readonly GameObject _origin;
            private readonly GameObject _assembly;
            private readonly GameObject _snap;
            private readonly GameObject _focus;
            private readonly GameObject _processor;
            private GameObject _blocker;

            public Fixture()
            {
                _player = new GameObject("ProcessorPlayer");
                _origin = new GameObject("ProcessorOrigin");
                _origin.transform.SetParent(_player.transform, false);
                _origin.transform.rotation = Quaternion.identity;
                _assembly = new GameObject("ProcessorAssembly");
                _snap = new GameObject("ProcessorSnap");
                _snap.transform.SetParent(_assembly.transform, false);
                _snap.transform.position = new Vector3(0f, 0f, 1f);
                _snap.transform.rotation = Quaternion.identity;
                _focus = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _focus.name = "ProcessorFocus";
                _focus.transform.SetParent(_assembly.transform, false);
                _focus.transform.position = new Vector3(0f, 0f, 1f);
                _focus.transform.localScale = new Vector3(0.10f, 0.10f, 0.02f);
                _processor = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _processor.name = "ProcessorItem";
                _processor.transform.position = new Vector3(3f, 0f, 0f);
                _processor.transform.localScale = Vector3.one;
                _processor.GetComponent<BoxCollider>().size =
                    new Vector3(0.05f, 0.05f, 0.008f);
                Rigidbody body = _processor.AddComponent<Rigidbody>();
                body.useGravity = false;
                body.isKinematic = true;
                Processor = _processor.AddComponent<PhysicalItemProjection>();
                Processor.Configure(
                    "tests.physical-item.processor-001",
                    "Test Processor",
                    body,
                    new Vector3(0.025f, 0.025f, 0.004f),
                    Vector3.zero,
                    Vector3.zero,
                    PhysicalCarryProfile.PcComponent);
                Physics.SyncTransforms();
            }

            public Transform Origin => _origin.transform;

            public Transform Snap => _snap.transform;

            public Collider Focus => _focus.GetComponent<Collider>();

            public PhysicalItemProjection Processor { get; }

            public ProcessorSocketEvaluation EvaluateSeat(
                int quarterTurns = 0,
                bool paused = false,
                bool authorityAvailable = true)
            {
                Physics.SyncTransforms();
                return ProcessorSocketSolver.EvaluateSeat(
                    Origin,
                    _player.transform,
                    Processor,
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

            public ProcessorSocketEvaluation EvaluateInteraction(
                ProcessorSocketState state,
                bool retentionCloseAvailable = true)
            {
                Physics.SyncTransforms();
                return ProcessorSocketSolver.EvaluateInteraction(
                    Origin,
                    _player.transform,
                    Processor.transform,
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
                _blocker.name = "ProcessorSeatBlocker";
                _blocker.transform.position =
                    Snap.position + new Vector3(0.030f, 0f, 0f);
                _blocker.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                Physics.SyncTransforms();
            }

            public void Dispose()
            {
                if (_blocker != null)
                {
                    UnityEngine.Object.DestroyImmediate(_blocker);
                }

                UnityEngine.Object.DestroyImmediate(_processor);
                UnityEngine.Object.DestroyImmediate(_assembly);
                UnityEngine.Object.DestroyImmediate(_player);
            }
        }
    }
}
