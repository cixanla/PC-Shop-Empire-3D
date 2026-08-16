using System;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class M2StorageSlotSolverTests
    {
        [Test]
        public void KeyedSeatExposesRaisedGuidedPoseAndSeparateFlatSeatPose()
        {
            using var fixture = new Fixture();

            M2StorageSlotEvaluation aligned = fixture.EvaluateSeat();
            M2StorageSlotEvaluation sideways = fixture.EvaluateSeat(1);
            M2StorageSlotEvaluation reversed = fixture.EvaluateSeat(2);

            Assert.That(aligned.Status, Is.EqualTo(M2StorageSlotStatus.ValidSeat));
            Assert.That(aligned.CanSeat, Is.True);
            Assert.That(aligned.Orientation, Is.EqualTo(M2KeyOrientation.KeyAligned));
            Assert.That(aligned.SeatedPose.position, Is.EqualTo(fixture.Seat.position));
            Assert.That(aligned.SeatedPose.rotation, Is.EqualTo(fixture.Seat.rotation));
            Assert.That(aligned.GuidedPose.position.y - aligned.SeatedPose.position.y,
                Is.EqualTo(M2StorageSlotSolver.GuidedLiftMetres).Within(0.00001f));
            Assert.That(Quaternion.Angle(
                    aligned.GuidedPose.rotation,
                    aligned.SeatedPose.rotation),
                Is.EqualTo(M2StorageSlotSolver.GuidedInsertionAngleDegrees)
                    .Within(0.001f));
            Assert.That(sideways.Status,
                Is.EqualTo(M2StorageSlotStatus.OrientationInvalid));
            Assert.That(sideways.Orientation, Is.EqualTo(default(M2KeyOrientation)));
            Assert.That(sideways.FailureCode,
                Is.EqualTo(AssemblyFailures.InvalidM2Orientation.Code));
            Assert.That(reversed.Status,
                Is.EqualTo(M2StorageSlotStatus.OrientationInvalid));
            Assert.That(reversed.Orientation, Is.EqualTo(M2KeyOrientation.Reversed));
            Assert.That(reversed.FailureCode,
                Is.EqualTo(AssemblyFailures.M2OrientationMismatch.Code));
        }

        [Test]
        public void PauseAuthorityRangeAndFocusFailClosedWithoutLosingCandidatePoses()
        {
            using var fixture = new Fixture();

            Assert.That(fixture.EvaluateSeat(paused: true).Status,
                Is.EqualTo(M2StorageSlotStatus.Paused));
            Assert.That(fixture.EvaluateSeat(authorityAvailable: false).Status,
                Is.EqualTo(M2StorageSlotStatus.AuthorityBlocked));

            fixture.Origin.position = new Vector3(0f, 0f, -3f);
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(M2StorageSlotStatus.OutOfRange));

            fixture.Origin.position = Vector3.zero;
            fixture.Origin.rotation = Quaternion.LookRotation(Vector3.back, Vector3.up);
            M2StorageSlotEvaluation notFocused = fixture.EvaluateSeat();
            Assert.That(notFocused.Status, Is.EqualTo(M2StorageSlotStatus.NotFocused));
            Assert.That(notFocused.HasPose, Is.True);
        }

        [Test]
        public void SharedPhysicsRejectsLineOfSightAndForeignInsertionObstruction()
        {
            using var fixture = new Fixture();
            fixture.Focus.isTrigger = true;
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(M2StorageSlotStatus.LineOfSightBlocked));

            fixture.Focus.isTrigger = false;
            fixture.CreateSeatBlocker();
            Assert.That(fixture.EvaluateSeat().Status,
                Is.EqualTo(M2StorageSlotStatus.Obstructed));
        }

        [Test]
        public void InteractionUsesAuthorityStateAndSecuredRemovalGate()
        {
            using var fixture = new Fixture();
            fixture.Storage.transform.position = new Vector3(0f, 0f, 0.82f);
            Physics.SyncTransforms();

            M2StorageSlotEvaluation unsecured = fixture.EvaluateInteraction(
                StorageSlotState.StorageDeviceSeatedUnsecured);
            M2StorageSlotEvaluation secured = fixture.EvaluateInteraction(
                StorageSlotState.StorageDeviceSecured);
            M2StorageSlotEvaluation hostBlocked = fixture.EvaluateInteraction(
                StorageSlotState.StorageDeviceSeatedUnsecured,
                retentionCloseAvailable: false);
            M2StorageSlotEvaluation empty = fixture.EvaluateInteraction(
                StorageSlotState.EmptyOpen);

            Assert.That(unsecured.Status,
                Is.EqualTo(M2StorageSlotStatus.ValidSeatedUnsecured));
            Assert.That(unsecured.CanOperateRetention, Is.True);
            Assert.That(unsecured.CanRemove, Is.True);
            Assert.That(secured.Status, Is.EqualTo(M2StorageSlotStatus.ValidSecured));
            Assert.That(secured.CanOperateRetention, Is.True);
            Assert.That(secured.CanRemove, Is.False);
            Assert.That(hostBlocked.Status,
                Is.EqualTo(M2StorageSlotStatus.ValidSeatedUnsecuredRetentionBlocked));
            Assert.That(hostBlocked.CanOperateRetention, Is.False);
            Assert.That(hostBlocked.CanRemove, Is.True);
            Assert.That(hostBlocked.FailureCode,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured.Code));
            Assert.That(empty.Status, Is.EqualTo(M2StorageSlotStatus.AuthorityBlocked));
        }

        [Test]
        public void ProjectionOwnsStableTopologyAndCaptiveScrewVisualState()
        {
            var root = new GameObject("M2StorageSlotProjectionTest");
            var seat = new GameObject("Seat").transform;
            seat.SetParent(root.transform, false);
            var focusObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            focusObject.transform.SetParent(root.transform, false);
            var screw = new GameObject("CaptiveScrewPivot").transform;
            screw.SetParent(root.transform, false);
            screw.localPosition = new Vector3(0f, 0.01f, 0f);
            screw.localRotation = Quaternion.Euler(0f, 15f, 0f);
            var projection = root.AddComponent<M2StorageSlotProjection>();
            try
            {
                Vector3 openPosition = screw.localPosition;
                Quaternion openRotation = screw.localRotation;
                projection.Configure(
                    GarageStockFlowSession.StorageSlotIdValue,
                    GarageStockFlowSession.StorageStandoffIdValue,
                    GarageStockFlowSession.StorageCaptiveScrewIdValue,
                    seat,
                    focusObject.GetComponent<Collider>(),
                    root.transform,
                    screw);

                Assert.That(projection.IsConfigured, Is.True);
                Assert.That(projection.StandoffIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageStandoffIdValue));
                Assert.That(projection.CaptiveScrewIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageCaptiveScrewIdValue));
                Assert.That(projection.FocusCollider.enabled, Is.False);

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    StorageSlotState.StorageDeviceSeatedUnsecured);
                Assert.That(projection.FocusCollider.enabled, Is.True);
                Assert.That(screw.localPosition, Is.EqualTo(openPosition));
                Assert.That(Quaternion.Angle(screw.localRotation, openRotation),
                    Is.LessThan(0.001f));

                projection.ApplyAuthoritativeState(
                    AssemblySeatState.SeatedSecured,
                    StorageSlotState.StorageDeviceSecured);
                Assert.That(screw.localPosition.y,
                    Is.EqualTo(openPosition.y - 0.004f).Within(0.00001f));
                Assert.That(Quaternion.Angle(screw.localRotation, openRotation),
                    Is.EqualTo(120f).Within(0.001f));
                Assert.That(projection.MatchesLogicalAuthorityState(
                    AssemblySeatState.SeatedSecured,
                    StorageSlotState.StorageDeviceSecured), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void PublicFailureCodesRemainStable()
        {
            Assert.That(Evaluation(M2StorageSlotStatus.ContextMissing).FailureCode,
                Is.EqualTo("assembly-storage.context-missing"));
            Assert.That(Evaluation(M2StorageSlotStatus.Paused).FailureCode,
                Is.EqualTo("assembly-storage.paused"));
            Assert.That(Evaluation(M2StorageSlotStatus.AuthorityBlocked).FailureCode,
                Is.EqualTo("assembly-storage.authority-blocked"));
            Assert.That(Evaluation(M2StorageSlotStatus.OutOfRange).FailureCode,
                Is.EqualTo("assembly-storage.out-of-range"));
            Assert.That(Evaluation(M2StorageSlotStatus.NotFocused).FailureCode,
                Is.EqualTo("assembly-storage.focus-missing"));
            Assert.That(Evaluation(M2StorageSlotStatus.LineOfSightBlocked).FailureCode,
                Is.EqualTo("assembly-storage.line-of-sight-blocked"));
            Assert.That(Evaluation(M2StorageSlotStatus.Obstructed).FailureCode,
                Is.EqualTo("assembly-storage.obstructed"));
            Assert.That(Evaluation(M2StorageSlotStatus.ValidSeat).FailureCode, Is.Empty);
        }

        private static M2StorageSlotEvaluation Evaluation(M2StorageSlotStatus status)
        {
            return new M2StorageSlotEvaluation(status, default, default, false, default);
        }

        private sealed class Fixture : IDisposable
        {
            private readonly GameObject _player;
            private readonly GameObject _origin;
            private readonly GameObject _assembly;
            private readonly GameObject _seat;
            private readonly GameObject _focus;
            private readonly GameObject _storage;
            private GameObject _blocker;

            public Fixture()
            {
                _player = new GameObject("M2StoragePlayer");
                _origin = new GameObject("M2StorageOrigin");
                _origin.transform.SetParent(_player.transform, false);
                _assembly = new GameObject("M2StorageAssembly");
                _seat = new GameObject("M2StorageSeat");
                _seat.transform.SetParent(_assembly.transform, false);
                _seat.transform.position = new Vector3(0f, 0f, 1f);
                _focus = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _focus.transform.SetParent(_assembly.transform, false);
                _focus.transform.position = _seat.transform.position;
                _focus.transform.localScale = new Vector3(0.12f, 0.02f, 0.04f);
                _storage = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _storage.transform.position = new Vector3(3f, 0f, 0f);
                BoxCollider storageCollider = _storage.GetComponent<BoxCollider>();
                storageCollider.size = new Vector3(0.08f, 0.004f, 0.022f);
                Rigidbody body = _storage.AddComponent<Rigidbody>();
                body.useGravity = false;
                body.isKinematic = true;
                Storage = _storage.AddComponent<PhysicalItemProjection>();
                Storage.Configure(
                    GarageStockFlowSession.StorageItemInstanceIdValue,
                    GarageStockFlowSession.StorageDisplayName,
                    body,
                    new Vector3(0.04f, 0.002f, 0.011f),
                    Vector3.zero,
                    Vector3.zero,
                    PhysicalCarryProfile.PcComponent);
                Physics.SyncTransforms();
            }

            public Transform Origin => _origin.transform;

            public Transform Seat => _seat.transform;

            public Collider Focus => _focus.GetComponent<Collider>();

            public PhysicalItemProjection Storage { get; }

            public M2StorageSlotEvaluation EvaluateSeat(
                int quarterTurns = 0,
                bool paused = false,
                bool authorityAvailable = true)
            {
                Physics.SyncTransforms();
                return M2StorageSlotSolver.EvaluateSeat(
                    Origin,
                    _player.transform,
                    Storage,
                    Seat,
                    Focus,
                    _assembly.transform,
                    1 << 0,
                    2f,
                    0.94f,
                    quarterTurns,
                    paused,
                    authorityAvailable);
            }

            public M2StorageSlotEvaluation EvaluateInteraction(
                StorageSlotState state,
                bool retentionCloseAvailable = true)
            {
                Physics.SyncTransforms();
                return M2StorageSlotSolver.EvaluateInteraction(
                    Origin,
                    _player.transform,
                    Storage.transform,
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
                _blocker.transform.position = Seat.position + new Vector3(0.05f, 0f, 0f);
                _blocker.transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                Physics.SyncTransforms();
            }

            public void Dispose()
            {
                if (_blocker != null)
                {
                    UnityEngine.Object.DestroyImmediate(_blocker);
                }

                UnityEngine.Object.DestroyImmediate(_storage);
                UnityEngine.Object.DestroyImmediate(_assembly);
                UnityEngine.Object.DestroyImmediate(_player);
            }
        }
    }
}
