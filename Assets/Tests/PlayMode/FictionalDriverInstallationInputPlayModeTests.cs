using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
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
        public IEnumerator FictionalDriverKeyboardMouseReviewsInstallsAndPersists()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalOsInstalledForDriver(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PcFictionalOsInstallationReceipt operatingSystem = session
                .FictionalOsInstallation.EvaluateInstalledOperatingSystem().Value;
            PcFirmwareBaselineReceipt firmware =
                session.PowerState.ActiveFirmwareBaselineReceipt;

            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False,
                "Prompt and Workbench observers must not create driver authority.");
            Assert.That(station.PromptText,
                Does.Contain("KURGUSAL OS KURULDU")
                    .And.Contain("DRIVER KURULUMUNU AÇ")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("SONRAKİ AŞAMA: DRIVER")
                    .And.Contain("BAKIM KİLİDİ AKTİF"));

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False);
            Assert.That(station.PromptText,
                Does.Contain("KURGUSAL DRIVER KURULUMU")
                    .And.Contain("WORKSHOP DRIVER BUNDLE")
                    .And.Contain("KURULUMU BAŞLAT VE TAMAMLA")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench
                    .FictionalDriverState,
                Is.EqualTo(FictionalDriverPresentationState.Reviewing));
            Assert.That(marker.ElectricalReadinessWorkbench
                    .FictionalDriverFailureCode,
                Is.Empty);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("KURGUSAL DRIVER KURULUMU İNCELENİYOR")
                    .And.Contain("ONAY BEKLİYOR"));

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            long powerRevision = session.PowerState.Revision;
            long postRevision = session.PowerState.PostStartupRevision;
            long firmwareRevision =
                session.PowerState.FirmwareBaselineRevision;
            long osRevision = session.FictionalOsInstallation.Revision;

            System.Reflection.FieldInfo reviewedFirmwareField = station
                .GetType().GetField(
                    "_reviewedDriverFirmwareBaselineReceipt",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(reviewedFirmwareField, Is.Not.Null);
            reviewedFirmwareField.SetValue(station, null);

            OperationResult rejectedReview =
                station.TryAttemptFictionalDriverAuthorizedForTests();

            Assert.That(rejectedReview.Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures.NotCurrent));
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False);
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False,
                "A rejected review must not create driver authority.");
            Assert.That(station.LastFailureCode,
                Is.EqualTo(PcFictionalDriverInstallationFailures.NotCurrent.Code));
            Assert.That(marker.ElectricalReadinessWorkbench
                    .FictionalDriverState,
                Is.EqualTo(FictionalDriverPresentationState.Rejected));
            Assert.That(marker.ElectricalReadinessWorkbench
                    .FictionalDriverFailureCode,
                Is.EqualTo(PcFictionalDriverInstallationFailures.NotCurrent.Code));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("DRIVER KURULUMU REDDEDİLDİ")
                    .And.Contain(
                        PcFictionalDriverInstallationFailures.NotCurrent.Code));

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench
                    .FictionalDriverState,
                Is.EqualTo(FictionalDriverPresentationState.Reviewing));

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetFictionalDriverInstallation(
                out PcFictionalDriverInstallationAuthority authority), Is.True);
            OperationResult<PcFictionalDriverInstallationReceipt> installed =
                authority.EvaluateInstalledDrivers();
            Assert.That(installed.IsSuccess, Is.True, installed.Error.Code);
            Assert.That(installed.Value.SourceOperatingSystemReceipt,
                Is.SameAs(operatingSystem));
            Assert.That(installed.Value.SourceFirmwareBaselineReceipt,
                Is.SameAs(firmware));
            Assert.That(installed.Value.StorageItemId,
                Is.EqualTo(session.AssemblyBuild.StorageItemId));
            Assert.That(installed.Value.Profile,
                Is.EqualTo(PcFictionalDriverProfile.WorkshopDriverBundle));
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False);
            Assert.That(station.PromptText,
                Does.Contain("WORKSHOP DRIVER BUNDLE KURULDU")
                    .And.Contain("SONRAKİ AŞAMA: VALIDATION")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasInstalledFictionalDrivers, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("WORKSHOP DRIVER BUNDLE KURULDU")
                    .And.Contain("SONRAKİ AŞAMA: VALIDATION")
                    .And.Contain("BAKIM KİLİDİ AKTİF"));
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
            Assert.That(session.PowerState.Revision, Is.EqualTo(powerRevision));
            Assert.That(session.PowerState.PostStartupRevision,
                Is.EqualTo(postRevision));
            Assert.That(session.PowerState.FirmwareBaselineRevision,
                Is.EqualTo(firmwareRevision));
            Assert.That(session.FictionalOsInstallation.Revision,
                Is.EqualTo(osRevision));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess,
                Is.True);

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(authority.EvaluateInstalledDrivers().Value,
                Is.SameAs(installed.Value));
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasInstalledFictionalDrivers, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("DEPOLAMADA KALICI")
                    .And.Contain("POWER-ON BEKLİYOR")
                    .And.Contain("SONRAKİ AŞAMA: VALIDATION"));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalDriverGamepadUsesSameInstallAndPowerOffPath()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalOsInstalledForDriver(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);
            Assert.That(station.PromptText,
                Does.Contain("RT")
                    .And.Contain("KURULUMU BAŞLAT VE TAMAMLA")
                    .And.Contain("A")
                    .And.Contain("GÜCÜ KAPAT"));

            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetFictionalDriverInstallation(
                out PcFictionalDriverInstallationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateInstalledDrivers().IsSuccess, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("A").And.Contain("GÜCÜ KAPAT")
                    .And.Contain("WORKSHOP DRIVER BUNDLE KURULDU"));

            PressGamepadInteractForFirmware(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(authority.EvaluateInstalledDrivers().IsSuccess, Is.True);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalDriverConcurrentPowerOffWinsPrimaryEdge()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalOsInstalledForDriver(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            station.ProcessInputFrame();

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False);
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame,
                Is.False);
            Assert.That(session.FictionalOsInstallation
                .EvaluateInstalledOperatingSystem().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator FictionalDriverPauseAndCompetingOwnerDoNotConsume()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalOsInstalledForDriver(
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
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False);
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            yield return null;

            var competingItem = CreateCompetingInteractItem(station);
            Assert.That(marker.PlayerCarry.HasCompetingWorldInteractOwner,
                Is.True);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False);
            UnityEngine.Object.DestroyImmediate(competingItem.gameObject);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False,
                "Motor pause must reset an open driver review.");
            Assert.That(marker.ElectricalReadinessWorkbench
                    .FictionalDriverState,
                Is.EqualTo(FictionalDriverPresentationState.Waiting));
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);

            competingItem = CreateCompetingInteractItem(station);
            Assert.That(marker.PlayerCarry.HasCompetingWorldInteractOwner,
                Is.True);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False,
                "A competing owner must reset an open driver review.");
            Assert.That(marker.ElectricalReadinessWorkbench
                    .FictionalDriverState,
                Is.EqualTo(FictionalDriverPresentationState.Waiting));
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False);
            UnityEngine.Object.DestroyImmediate(competingItem.gameObject);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerMotor.IsPaused, Is.False,
                "The raw pause edge is evaluated before motor consumption.");
            Assert.That(marker.PlayerInput.PausePressedThisFrame, Is.True);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False,
                "Raw pause must reset driver review without consuming primary.");
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetFictionalDriverInstallation(
                out PcFictionalDriverInstallationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateInstalledDrivers().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalDriverReviewResetsAfterContextLoss()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalOsInstalledForDriver(
                marker,
                keyboard,
                mouse);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True);

            Vector3 target = station.FocusAnchor.position;
            MovePlayerAndAim(
                marker,
                target + (Vector3.back *
                    (station.InteractionRange + 0.65f)),
                target);
            AssertFictionalDriverPrimaryInputSurvivesContextLoss(
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
            AssertFictionalDriverPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "Focus loss");
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Vector3 origin = station.PlayerCamera.transform.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "FictionalDriverReviewLosBlocker";
            blocker.transform.position = Vector3.Lerp(origin, target, 0.5f);
            blocker.transform.localScale = new Vector3(0.45f, 0.55f, 0.12f);
            Physics.SyncTransforms();
            AssertFictionalDriverPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "LOS loss");
            UnityEngine.Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            PhysicalItemProjection largeBox = UnityEngine.Object
                .FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsSortMode.None)
                .Single(item =>
                    item.CarryProfile == PhysicalCarryProfile.LargeBox);
            Assert.That(marker.PlayerCarry.TryPickup(largeBox).IsSuccess,
                Is.True);
            AssertFictionalDriverPrimaryInputSurvivesContextLoss(
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
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.True,
                "Returning after context loss must reopen driver review first.");
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetFictionalDriverInstallation(
                out PcFictionalDriverInstallationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateInstalledDrivers().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalDriverMalformedHistoryNeverBlocksPowerOff()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);
            yield return PrepareFictionalOsInstalledForDriver(
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

            Assert.That(session.TryGetFictionalDriverInstallation(
                out PcFictionalDriverInstallationAuthority authority), Is.True);
            System.Reflection.FieldInfo revisionField =
                typeof(PcFictionalDriverInstallationAuthority).GetField(
                    "<Revision>k__BackingField",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(revisionField, Is.Not.Null);
            revisionField.SetValue(authority, 2L);

            Assert.That(authority.ValidateReceiptHistory().Error,
                Is.EqualTo(PcFictionalDriverInstallationFailures
                    .ReceiptHistoryInvalid));
            Assert.That(station.PromptText, Does.Contain("GÜCÜ KAPAT"));

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
            Assert.That(authority.EvaluateInstalledDrivers().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static IEnumerator PrepareFictionalOsInstalledForDriver(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse)
        {
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;
            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.TryGetFictionalOsInstallation(
                out PcFictionalOsInstallationAuthority authority), Is.True);
            Assert.That(authority.EvaluateInstalledOperatingSystem().IsSuccess,
                Is.True);
            Assert.That(session.PowerState.IsEnergized, Is.True);
            Assert.That(session.PowerState
                .EvaluateCurrentFirmwareBaseline().IsSuccess, Is.True);
            Assert.That(session.TryGetFictionalDriverInstallation(out _),
                Is.False);
        }

        private static void
            AssertFictionalDriverPrimaryInputSurvivesContextLoss(
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
            Assert.That(station.IsReviewingFictionalDriverInstallation,
                Is.False,
                context + " must invalidate an open driver review.");
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }
    }
}
