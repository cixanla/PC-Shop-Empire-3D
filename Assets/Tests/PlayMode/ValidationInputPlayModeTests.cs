using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Quality;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class PowerTestPreflightInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator ValidationKeyboardMouseReviewsRunsAndPreservesUpstream()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalDriverInstalledForValidation(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PcFictionalDriverInstallationReceipt driver = session
                .FictionalDriverInstallation.EvaluateInstalledDrivers().Value;
            PcFirmwareBaselineReceipt firmware =
                session.PowerState.ActiveFirmwareBaselineReceipt;

            Assert.That(session.TryGetValidation(out _), Is.False,
                "Prompt and Workbench observers must not create validation authority.");
            Assert.That(station.PromptText,
                Does.Contain("SONRAKİ AŞAMA: VALIDATION")
                    .And.Contain("VALIDATION'I İNCELE")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.Waiting));

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewingValidation, Is.True);
            Assert.That(session.TryGetValidation(out _), Is.False);
            Assert.That(station.PromptText,
                Does.Contain("WORKSHOP VALIDATION SUITE")
                    .And.Contain("300 SABİT STRESS ADIMI")
                    .And.Contain("BENCHMARK + STRESS ÇALIŞTIR")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.Reviewing));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("VALIDATION İNCELENİYOR")
                    .And.Contain("ONAY BEKLİYOR"));

            System.Reflection.FieldInfo reviewedFirmwareField = station
                .GetType().GetField(
                    "_reviewedValidationFirmwareReceipt",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(reviewedFirmwareField, Is.Not.Null);
            reviewedFirmwareField.SetValue(station, null);
            OperationResult rejected =
                station.TryAttemptValidationAuthorizedForTests();

            Assert.That(rejected.Error,
                Is.EqualTo(PcValidationFailures.NotCurrent));
            Assert.That(station.IsReviewingValidation, Is.False);
            Assert.That(session.TryGetValidation(out _), Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.Rejected));
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationFailureCode,
                Is.EqualTo(PcValidationFailures.NotCurrent.Code));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("VALIDATION REDDEDİLDİ")
                    .And.Contain(PcValidationFailures.NotCurrent.Code));
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingValidation, Is.True);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            long preflightRevision = session.PowerTestAttempts.Revision;
            int preflightReceiptCount = session.PowerTestAttempts.ReceiptCount;
            long powerRevision = session.PowerState.Revision;
            int powerReceiptCount = session.PowerState.ReceiptCount;
            long postRevision = session.PowerState.PostStartupRevision;
            int postReceiptCount = session.PowerState.PostStartupReceiptCount;
            long firmwareRevision = session.PowerState.FirmwareBaselineRevision;
            int firmwareReceiptCount =
                session.PowerState.FirmwareBaselineReceiptCount;
            long osRevision = session.FictionalOsInstallation.Revision;
            int osReceiptCount = session.FictionalOsInstallation.ReceiptCount;
            long driverRevision = session.FictionalDriverInstallation.Revision;
            int driverReceiptCount =
                session.FictionalDriverInstallation.ReceiptCount;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetValidation(
                out PcValidationAuthority authority), Is.True);
            OperationResult<PcValidationReceipt> current =
                authority.EvaluateCurrentValidation();
            Assert.That(current.IsSuccess, Is.True, current.Error.Code);
            Assert.That(current.Value.SourceDriverReceipt, Is.SameAs(driver));
            Assert.That(current.Value.SourceFirmwareBaselineReceipt,
                Is.SameAs(firmware));
            Assert.That(current.Value.BenchmarkScore, Is.EqualTo(401));
            Assert.That(current.Value.StressSteps, Is.EqualTo(300));
            Assert.That(current.Value.ProcessorPeakTemperatureCelsius,
                Is.EqualTo(67));
            Assert.That(current.Value.GraphicsCardPeakTemperatureCelsius,
                Is.EqualTo(64));
            Assert.That(current.Value.PowerMarginWatts, Is.EqualTo(50));
            Assert.That(current.Value.QualityTier,
                Is.EqualTo(PcQualityTier.Good));
            Assert.That(station.IsReviewingValidation, Is.False);
            Assert.That(station.PromptText,
                Does.Contain("VALIDATION GEÇTİ")
                    .And.Contain("SCORE 401")
                    .And.Contain("CPU 67°C / GPU 64°C")
                    .And.Contain("PSU +50W")
                    .And.Contain("YENİDEN İNCELE")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.Passed));
            Assert.That(marker.ElectricalReadinessWorkbench.HasCurrentValidation,
                Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("VALIDATION GEÇTİ • İYİ")
                    .And.Contain("SCORE 401")
                    .And.Contain("STRESS 300 ADIM STABLE")
                    .And.Contain("CPU 67°C")
                    .And.Contain("GPU 64°C")
                    .And.Contain("PSU +50W"));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieRevision));
            Assert.That(session.PowerTestAttempts.Revision,
                Is.EqualTo(preflightRevision));
            Assert.That(session.PowerTestAttempts.ReceiptCount,
                Is.EqualTo(preflightReceiptCount));
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.PowerState.ReceiptCount,
                Is.EqualTo(powerReceiptCount));
            Assert.That(session.PowerState.PostStartupRevision,
                Is.EqualTo(postRevision));
            Assert.That(session.PowerState.PostStartupReceiptCount,
                Is.EqualTo(postReceiptCount));
            Assert.That(session.PowerState.FirmwareBaselineRevision,
                Is.EqualTo(firmwareRevision));
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount,
                Is.EqualTo(firmwareReceiptCount));
            Assert.That(session.FictionalOsInstallation.Revision,
                Is.EqualTo(osRevision));
            Assert.That(session.FictionalOsInstallation.ReceiptCount,
                Is.EqualTo(osReceiptCount));
            Assert.That(session.FictionalDriverInstallation.Revision,
                Is.EqualTo(driverRevision));
            Assert.That(session.FictionalDriverInstallation.ReceiptCount,
                Is.EqualTo(driverReceiptCount));

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(authority.EvaluateCurrentValidation().Error,
                Is.EqualTo(PcValidationFailures.NotCurrent));
            Assert.That(authority.TryGetReceipt(
                    current.Value.OperationId,
                    out PcValidationReceipt historical),
                Is.True);
            Assert.That(historical, Is.SameAs(current.Value));
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.NotCurrent));
            Assert.That(marker.ElectricalReadinessWorkbench.QualityReleaseState,
                Is.EqualTo(
                    CustomPcQualityReleasePresentationState.ReadyForReview));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("VALIDATION GEÇTİ")
                    .And.Contain("GÜVENLİ KAPATILDI")
                    .And.Contain("KALİTE DOSYASI"));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator ValidationGamepadUsesSameRunAndPowerOffPath()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalDriverInstalledForValidation(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(station.IsReviewingValidation, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("RT")
                    .And.Contain("BENCHMARK + STRESS ÇALIŞTIR")
                    .And.Contain("A")
                    .And.Contain("GÜCÜ KAPAT"));

            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetValidation(
                out PcValidationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateCurrentValidation().IsSuccess, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("A")
                    .And.Contain("GÜCÜ KAPAT")
                    .And.Contain("VALIDATION GEÇTİ"));

            PressGamepadInteractForFirmware(
                gamepad,
                station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(authority.EvaluateCurrentValidation().Error,
                Is.EqualTo(PcValidationFailures.NotCurrent));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator ValidationConcurrentPowerOffWinsPrimaryEdge()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalDriverInstalledForValidation(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingValidation, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            station.ProcessInputFrame();

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.TryGetValidation(out _), Is.False);
            Assert.That(station.IsReviewingValidation, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame,
                Is.False);
            Assert.That(session.FictionalDriverInstallation
                .EvaluateInstalledDrivers().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator ValidationPauseRawPauseAndCompetingOwnerDoNotConsume()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalDriverInstalledForValidation(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingValidation, Is.False);
            Assert.That(session.TryGetValidation(out _), Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            yield return null;

            PhysicalItemProjection competingItem =
                CreateCompetingInteractItem(station);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingValidation, Is.False);
            Object.DestroyImmediate(competingItem.gameObject);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingValidation, Is.True);
            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingValidation, Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.Waiting));
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            competingItem = CreateCompetingInteractItem(station);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingValidation, Is.False);
            Object.DestroyImmediate(competingItem.gameObject);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PausePressedThisFrame, Is.True);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingValidation, Is.False);
            Assert.That(session.TryGetValidation(out _), Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetValidation(
                out PcValidationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateCurrentValidation().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator ValidationReviewResetsAfterContextLoss()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalDriverInstalledForValidation(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingValidation, Is.True);

            Vector3 target = station.FocusAnchor.position;
            MovePlayerAndAim(
                marker,
                target + (Vector3.back * (station.InteractionRange + 0.65f)),
                target);
            AssertValidationPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "Range loss");
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Transform cameraPivot = marker.PlayerMotor.transform.Find(
                "CameraPivot");
            cameraPivot.rotation = Quaternion.AngleAxis(30f, Vector3.up) *
                                   cameraPivot.rotation;
            Physics.SyncTransforms();
            AssertValidationPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "Focus loss");
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Vector3 origin = station.PlayerCamera.transform.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "ValidationReviewLosBlocker";
            blocker.transform.position = Vector3.Lerp(origin, target, 0.5f);
            blocker.transform.localScale = new Vector3(0.45f, 0.55f, 0.12f);
            Physics.SyncTransforms();
            AssertValidationPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "LOS loss");
            Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            PhysicalItemProjection largeBox = Object
                .FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsSortMode.None)
                .Single(item =>
                    item.CarryProfile == PhysicalCarryProfile.LargeBox);
            Assert.That(marker.PlayerCarry.TryPickup(largeBox).IsSuccess, Is.True);
            AssertValidationPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "Busy hands");
            Assert.That(marker.PlayerCarry.TryRecoverHeldItem().IsSuccess,
                Is.True);
            yield return null;

            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingValidation, Is.True);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetValidation(
                out PcValidationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateCurrentValidation().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator ValidationMalformedHistoryNeverBlocksPowerOff()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalDriverInstalledForValidation(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetValidation(
                out PcValidationAuthority authority), Is.True);

            System.Reflection.FieldInfo revisionField =
                typeof(PcValidationAuthority).GetField(
                    "<Revision>k__BackingField",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(revisionField, Is.Not.Null);
            revisionField.SetValue(authority, 2L);

            Assert.That(authority.ValidateReceiptHistory().Error,
                Is.EqualTo(PcValidationFailures.ReceiptHistoryInvalid));
            Assert.That(station.PromptText,
                Does.Contain("GÜCÜ KAPAT")
                    .And.Contain("VALIDATION KAYDI ENGELLİ")
                    .And.Contain(PcValidationFailures.ReceiptHistoryInvalid.Code));
            marker.ElectricalReadinessWorkbench.RefreshPresentation();
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.Rejected));

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.PowerState.ActivePowerOnReceipt, Is.Null);
            Assert.That(session.PowerState.ActivePostStartupReceipt, Is.Null);
            Assert.That(session.PowerState.ActiveFirmwareBaselineReceipt,
                Is.Null);

            revisionField.SetValue(authority, 1L);
            marker.ElectricalReadinessWorkbench.RefreshPresentation();
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(authority.EvaluateCurrentValidation().Error,
                Is.EqualTo(PcValidationFailures.NotCurrent));
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.NotCurrent));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static IEnumerator PrepareFictionalDriverInstalledForValidation(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse)
        {
            yield return PrepareFictionalOsInstalledForDriver(
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
            Assert.That(session.TryGetFictionalDriverInstallation(
                out PcFictionalDriverInstallationAuthority authority), Is.True);
            Assert.That(authority.EvaluateInstalledDrivers().IsSuccess, Is.True);
            Assert.That(session.PowerState.IsEnergized, Is.True);
            Assert.That(session.PowerState
                .EvaluateCurrentFirmwareBaseline().IsSuccess, Is.True);
            Assert.That(session.TryGetValidation(out _), Is.False);
        }

        private static void AssertValidationPrimaryInputSurvivesContextLoss(
            GaragePrototypeMarker marker,
            ElectricalPowerTestStationProjection station,
            Mouse mouse,
            string context)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame,
                Is.True,
                context + " must not consume the primary edge.");
            Assert.That(station.IsReviewingValidation,
                Is.False,
                context + " must invalidate an open validation review.");
            Assert.That(marker.ElectricalReadinessWorkbench.ValidationState,
                Is.EqualTo(PcValidationPresentationState.Waiting),
                context);
            Assert.That(marker.StockFlow.EnsureInitialized()
                .TryGetValidation(out _), Is.False, context);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }
    }
}
