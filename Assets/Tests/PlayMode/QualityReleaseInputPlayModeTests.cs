using System.Collections;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Quality;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class PowerTestPreflightInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator QualityReleaseKeyboardMouseReviewsAndReleasesExactJob()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareSafeShutdownForQuality(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            Assert.That(session.TryGetQualityRelease(out _), Is.False,
                "Prompt and workbench observers must not create quality authority.");
            Assert.That(station.PromptText,
                Does.Contain("VALIDATION GEÇTİ")
                    .And.Contain("GÜVENLİ KAPATILDI")
                    .And.Contain("KALİTE DOSYASINI İNCELE")
                    .And.Contain("GÜCÜ AÇ"));
            Assert.That(marker.ElectricalReadinessWorkbench.QualityReleaseState,
                Is.EqualTo(
                    CustomPcQualityReleasePresentationState.ReadyForReview));

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewingQualityRelease, Is.True);
            Assert.That(session.TryGetQualityRelease(out _), Is.False);
            Assert.That(station.PromptText,
                Does.Contain("KALİTE DOSYASI")
                    .And.Contain("EXACT İŞ EMRİ + SAFE SHUTDOWN")
                    .And.Contain("PAKETLEME SERBEST BIRAK"));
            Assert.That(marker.ElectricalReadinessWorkbench.QualityReleaseState,
                Is.EqualTo(
                    CustomPcQualityReleasePresentationState.Reviewing));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("KALİTE DOSYASI İNCELENİYOR")
                    .And.Contain("ONAYI BEKLİYOR"));

            long inventoryRevision = session.Inventory.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long powerRevision = session.PowerState.Revision;
            long validationRevision = session.Validation.Revision;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetQualityRelease(
                out CustomPcQualityReleaseAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            OperationResult<CustomPcQualityReleaseReceipt> current =
                authority.EvaluateCurrentRelease();
            Assert.That(current.IsSuccess, Is.True, current.Error.Code);
            Assert.That(current.Value.Result,
                Is.EqualTo(CustomPcQualityReleaseResult.ReadyForPackaging));
            Assert.That(current.Value.WorkOrderId,
                Is.EqualTo(session.PrototypeCustomPcBuildOrderId));
            Assert.That(current.Value.WorkTicketId,
                Is.EqualTo(session.PrototypeCustomPcWorkTicketId));
            Assert.That(current.Value.SourceValidationReceipt.BenchmarkScore,
                Is.EqualTo(401));
            Assert.That(current.Value.SourcePowerOffReceipt.ResultingState,
                Is.EqualTo(PcPowerState.Off));
            Assert.That(station.IsReviewingQualityRelease, Is.False);
            Assert.That(station.PromptText,
                Does.Contain("PAKETLEMEYE HAZIR")
                    .And.Contain("SCORE 401")
                    .And.Contain("KALİTEYİ YENİDEN İNCELE"));
            Assert.That(marker.ElectricalReadinessWorkbench.QualityReleaseState,
                Is.EqualTo(
                    CustomPcQualityReleasePresentationState.ReadyForPackaging));
            Assert.That(marker.ElectricalReadinessWorkbench.IsReadyForPackaging,
                Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("KALİTE ONAYLANDI")
                    .And.Contain("PAKETLEMEYE HAZIR")
                    .And.Contain("EXACT İŞ EMRİ + SAFE SHUTDOWN"));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(workOrderRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.Validation.Revision,
                Is.EqualTo(validationRevision));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator QualityReleaseGamepadUsesSameReviewAndReleasePath()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareSafeShutdownForQuality(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(station.IsReviewingQualityRelease, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("RT")
                    .And.Contain("PAKETLEME SERBEST BIRAK")
                    .And.Contain("A")
                    .And.Contain("GÜCÜ AÇ"));

            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetQualityRelease(
                out CustomPcQualityReleaseAuthority authority), Is.True);
            Assert.That(authority.EvaluateCurrentRelease().IsSuccess, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.IsReadyForPackaging,
                Is.True);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator QualityReleaseConcurrentPowerOffWinsPrimaryEdge()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareValidationPassedForQuality(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            station.ProcessInputFrame();

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.TryGetQualityRelease(out _), Is.False);
            Assert.That(station.IsReviewingQualityRelease, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame,
                Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.QualityReleaseState,
                Is.EqualTo(
                    CustomPcQualityReleasePresentationState.ReadyForReview));

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewingQualityRelease, Is.True);
            Assert.That(session.TryGetQualityRelease(out _), Is.False,
                "Power-off co-edge must not skip directly to release.");
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator QualityReleaseContextLossDoesNotConsumeAndTamperCannotBlockPowerOn()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareSafeShutdownForQuality(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingQualityRelease, Is.True);

            Vector3 target = station.FocusAnchor.position;
            MovePlayerAndAim(
                marker,
                target + (Vector3.back * (station.InteractionRange + 0.65f)),
                target);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame,
                Is.True,
                "Range loss must not consume the quality primary edge.");
            Assert.That(station.IsReviewingQualityRelease, Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetQualityRelease(
                out CustomPcQualityReleaseAuthority authority), Is.True);

            System.Reflection.FieldInfo revisionField = authority.GetType()
                .GetField(
                    "<Revision>k__BackingField",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(revisionField, Is.Not.Null);
            revisionField.SetValue(authority, 2L);
            Assert.That(authority.ValidateReceiptHistory().Error,
                Is.EqualTo(
                    CustomPcQualityReleaseFailures.ReceiptHistoryInvalid));
            Assert.That(station.PromptText,
                Does.Contain("GÜCÜ AÇ")
                    .And.Contain("KALİTE KAYDI ENGELLİ"));

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State,
                Is.EqualTo(PcPowerState.Energized));
            revisionField.SetValue(authority, 1L);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(authority.EvaluateCurrentRelease().Error,
                Is.EqualTo(CustomPcQualityReleaseFailures.NotCurrent));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static IEnumerator PrepareSafeShutdownForQuality(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse)
        {
            yield return PrepareValidationPassedForQuality(
                marker,
                keyboard,
                mouse);
            PressInteract(
                keyboard,
                marker.ElectricalPowerTestStation.ProcessInputFrame);
            yield return null;

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.TryGetQualityReleaseCandidate(
                out _,
                out _,
                out PcValidationReceipt validation,
                out PcPowerStateReceipt powerOff), Is.True);
            Assert.That(validation, Is.Not.Null);
            Assert.That(powerOff, Is.Not.Null);
            Assert.That(powerOff.SourcePowerOnReceipt,
                Is.SameAs(validation.SourcePowerOnReceipt));
        }

        private static IEnumerator PrepareValidationPassedForQuality(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse)
        {
            yield return PrepareFictionalDriverInstalledForValidation(
                marker,
                keyboard,
                mouse);
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.Validation.EvaluateCurrentValidation().IsSuccess,
                Is.True);
            Assert.That(session.TryGetQualityReleaseCandidate(
                out _,
                out _,
                out PcValidationReceipt validation,
                out PcPowerStateReceipt powerOff), Is.True);
            Assert.That(validation, Is.Not.Null);
            Assert.That(powerOff, Is.Null);
        }
    }
}
