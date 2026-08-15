using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class MotherboardFastenerSolverTests
    {
        [Test]
        public void ValidTargetPreservesAuthoritativeSecuredState()
        {
            using var fixture = new Fixture();

            MotherboardFastenerEvaluation unsecured = fixture.Evaluate(isSecured: false);
            MotherboardFastenerEvaluation secured = fixture.Evaluate(isSecured: true);

            Assert.That(unsecured.Status,
                Is.EqualTo(MotherboardFastenerStatus.ValidUnsecured));
            Assert.That(unsecured.CanOperate, Is.True);
            Assert.That(unsecured.IsSecured, Is.False);
            Assert.That(secured.Status,
                Is.EqualTo(MotherboardFastenerStatus.ValidSecured));
            Assert.That(secured.CanOperate, Is.True);
            Assert.That(secured.IsSecured, Is.True);
        }

        [Test]
        public void PauseAuthorityRangeFocusLosAndObstructionAreFailClosed()
        {
            using var fixture = new Fixture();

            Assert.That(fixture.Evaluate(paused: true).Status,
                Is.EqualTo(MotherboardFastenerStatus.Paused));

            fixture.Target.enabled = false;
            Assert.That(fixture.Evaluate(isSeated: false).Status,
                Is.EqualTo(MotherboardFastenerStatus.AuthorityBlocked));
            Assert.That(fixture.Evaluate(isSeated: true).Status,
                Is.EqualTo(MotherboardFastenerStatus.ContextMissing));
            fixture.Target.enabled = true;

            fixture.Origin.position = new Vector3(0f, 0f, -3f);
            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(MotherboardFastenerStatus.OutOfRange));
            fixture.Origin.position = Vector3.zero;
            fixture.Origin.rotation = Quaternion.LookRotation(Vector3.right, Vector3.up);
            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(MotherboardFastenerStatus.NotFocused));

            fixture.Origin.rotation = Quaternion.identity;
            fixture.Target.isTrigger = true;
            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(MotherboardFastenerStatus.LineOfSightBlocked));
            fixture.Target.isTrigger = false;
            fixture.CreateBlocker();
            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(MotherboardFastenerStatus.Obstructed));
        }

        [Test]
        public void DefaultEvaluationAndPublicFailureCodesAreStable()
        {
            MotherboardFastenerEvaluation empty = default;

            Assert.That(empty.Status, Is.EqualTo(MotherboardFastenerStatus.Uninitialized));
            Assert.That(empty.CanOperate, Is.False);
            Assert.That(Evaluation(MotherboardFastenerStatus.ContextMissing).FailureCode,
                Is.EqualTo("assembly-fastener.context-missing"));
            Assert.That(Evaluation(MotherboardFastenerStatus.Paused).FailureCode,
                Is.EqualTo("assembly-fastener.paused"));
            Assert.That(Evaluation(MotherboardFastenerStatus.AuthorityBlocked).FailureCode,
                Is.EqualTo("assembly-fastener.authority-blocked"));
            Assert.That(Evaluation(MotherboardFastenerStatus.OutOfRange).FailureCode,
                Is.EqualTo("assembly-fastener.out-of-range"));
            Assert.That(Evaluation(MotherboardFastenerStatus.NotFocused).FailureCode,
                Is.EqualTo("assembly-fastener.focus-missing"));
            Assert.That(Evaluation(MotherboardFastenerStatus.LineOfSightBlocked).FailureCode,
                Is.EqualTo("assembly-fastener.line-of-sight-blocked"));
            Assert.That(Evaluation(MotherboardFastenerStatus.Obstructed).FailureCode,
                Is.EqualTo("assembly-fastener.obstructed"));
            Assert.That(Evaluation(MotherboardFastenerStatus.ValidUnsecured).FailureCode,
                Is.Empty);
            Assert.That(Evaluation(MotherboardFastenerStatus.ValidSecured).FailureCode,
                Is.Empty);
        }

        [Test]
        public void CoincidentObstructionWinsDeterministicallyOverTarget()
        {
            using var fixture = new Fixture();
            fixture.CreateCoincidentBlocker();

            Assert.That(fixture.Evaluate().Status,
                Is.EqualTo(MotherboardFastenerStatus.Obstructed));
        }

        [TestCase(-0.00005f, MotherboardFastenerStatus.Obstructed)]
        [TestCase(0f, MotherboardFastenerStatus.Obstructed)]
        [TestCase(0.00005f, MotherboardFastenerStatus.Obstructed)]
        [TestCase(0.00020f, MotherboardFastenerStatus.ValidUnsecured)]
        public void NearHitTieBreakIsStableAcrossRaycastOrdering(
            float blockerOffset,
            MotherboardFastenerStatus expectedStatus)
        {
            using var fixture = new Fixture();
            fixture.CreateOffsetBlocker(blockerOffset);

            Assert.That(fixture.Evaluate().Status, Is.EqualTo(expectedStatus));
        }

        [Test]
        public void ProjectionUsesTextAndShapeStateInsteadOfColorAlone()
        {
            var root = new GameObject("FastenerProjectionTest");
            var tool = new GameObject("Screwdriver").transform;
            tool.SetParent(root.transform, false);
            tool.localRotation = Quaternion.Euler(0f, 0f, -55f);
            Quaternion authoredToolRotation = tool.localRotation;
            GameObject screw = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            screw.name = "ScrewHead";
            screw.transform.SetParent(root.transform, false);
            screw.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            var text = new GameObject("StatusText").AddComponent<TextMesh>();
            text.transform.SetParent(root.transform, false);
            var projection = root.AddComponent<MotherboardFastenerProjection>();
            try
            {
                projection.Configure(
                    "assembly.fastener.motherboard-main-01",
                    screw.GetComponent<Collider>(),
                    screw.GetComponent<Renderer>(),
                    screw.transform,
                    tool,
                    text,
                    null,
                    null,
                    null,
                    null);
                Quaternion openRotation = screw.transform.localRotation;
                Vector3 openPosition = screw.transform.localPosition;

                Assert.That(screw.GetComponent<Collider>().enabled, Is.False);

                projection.ApplyAuthoritativeState(AssemblySeatState.SeatedSecured);

                Assert.That(projection.IsConfigured, Is.True);
                Assert.That(projection.IsShowingSecured, Is.True);
                Assert.That(screw.GetComponent<Collider>().enabled, Is.True);
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured), Is.True);
                Assert.That(text.text, Does.Contain("SIKILI"));
                Assert.That(Quaternion.Angle(
                    openRotation,
                    screw.transform.localRotation), Is.GreaterThan(45f));
                Assert.That(Vector3.Distance(
                    screw.transform.localPosition,
                    openPosition + (Vector3.forward * 0.004f)), Is.LessThan(0.00001f));
                screw.transform.localPosition = openPosition;
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured), Is.False);
                projection.ApplyAuthoritativeState(AssemblySeatState.SeatedSecured);
                tool.localRotation = authoredToolRotation;
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured), Is.False);
                projection.ApplyAuthoritativeState(AssemblySeatState.SeatedSecured);
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedSecured), Is.True);

                projection.ApplyAuthoritativeState(AssemblySeatState.SeatedUnsecured);
                Assert.That(projection.IsShowingSecured, Is.False);
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedUnsecured), Is.True);
                screw.GetComponent<Collider>().enabled = false;
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.SeatedUnsecured), Is.False);
                screw.GetComponent<Collider>().enabled = true;
                Assert.That(text.text, Does.Contain("GEVŞEK"));
                Assert.That(Quaternion.Angle(
                    openRotation,
                    screw.transform.localRotation), Is.LessThan(0.01f));
                Assert.That(Vector3.Distance(
                    screw.transform.localPosition,
                    openPosition), Is.LessThan(0.00001f));
                Assert.That(Quaternion.Angle(
                    authoredToolRotation * Quaternion.Euler(18f, 0f, 0f),
                    tool.localRotation), Is.LessThan(0.01f));

                projection.Evaluate(
                    null,
                    root.transform,
                    1 << 0,
                    false,
                    true,
                    false);
                Assert.That(text.text, Is.EqualTo("[X] KULLANILAMAZ"));
                projection.ResetFeedback();
                Assert.That(text.text, Is.EqualTo("[O] VİDA GEVŞEK"));

                projection.ApplyAuthoritativeState(AssemblySeatState.Empty);
                Assert.That(screw.GetComponent<Collider>().enabled, Is.False);
                Assert.That(projection.IsConfigured, Is.True);
                Assert.That(projection.MatchesAuthorityState(
                    AssemblySeatState.Empty), Is.True);
                Assert.That(text.text, Is.EqualTo("[ ] ANAKARTI OTURT"));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static MotherboardFastenerEvaluation Evaluation(
            MotherboardFastenerStatus status)
        {
            return new MotherboardFastenerEvaluation(status, false);
        }

        private sealed class Fixture : System.IDisposable
        {
            private readonly GameObject _originObject;
            private readonly GameObject _playerObject;
            private readonly GameObject _targetObject;
            private GameObject _blocker;

            public Fixture()
            {
                _originObject = new GameObject("FastenerOrigin");
                _playerObject = new GameObject("FastenerPlayer");
                _targetObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _targetObject.name = "FastenerTarget";
                _targetObject.transform.position = new Vector3(0f, 0f, 1f);
                _targetObject.transform.localScale = Vector3.one * 0.1f;
                Origin.rotation = Quaternion.identity;
                Physics.SyncTransforms();
            }

            public Transform Origin => _originObject.transform;

            public Collider Target => _targetObject.GetComponent<Collider>();

            public MotherboardFastenerEvaluation Evaluate(
                bool paused = false,
                bool isSeated = true,
                bool isSecured = false)
            {
                Physics.SyncTransforms();
                return MotherboardFastenerSolver.Evaluate(
                    Origin,
                    _playerObject.transform,
                    Target,
                    1 << 0,
                    2f,
                    0.975f,
                    paused,
                    isSeated,
                    isSecured);
            }

            public void CreateBlocker()
            {
                _blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _blocker.name = "FastenerBlocker";
                _blocker.transform.position = new Vector3(0f, 0f, 0.5f);
                _blocker.transform.localScale = new Vector3(0.2f, 0.2f, 0.1f);
                Physics.SyncTransforms();
            }

            public void CreateCoincidentBlocker()
            {
                _blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _blocker.name = "CoincidentFastenerBlocker";
                _blocker.transform.position = _targetObject.transform.position;
                _blocker.transform.localScale = _targetObject.transform.localScale;
                Physics.SyncTransforms();
            }

            public void CreateOffsetBlocker(float forwardOffset)
            {
                _blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _blocker.name = "OffsetFastenerBlocker";
                _blocker.transform.position = _targetObject.transform.position +
                                              (Vector3.forward * forwardOffset);
                _blocker.transform.localScale = _targetObject.transform.localScale;
                Physics.SyncTransforms();
            }

            public void Dispose()
            {
                Object.DestroyImmediate(_blocker);
                Object.DestroyImmediate(_targetObject);
                Object.DestroyImmediate(_playerObject);
                Object.DestroyImmediate(_originObject);
            }
        }
    }
}
