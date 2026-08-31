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
        public IEnumerator FictionalOsKeyboardMouseReviewsInstallsAndPersists()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
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

            PcPowerStateAuthority powerState = session.PowerState;
            PcFirmwareBaselineReceipt firmware =
                powerState.ActiveFirmwareBaselineReceipt;
            Assert.That(firmware, Is.Not.Null);
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False,
                "Prompt and Workbench observers must not create the OS authority.");
            Assert.That(station.PromptText,
                Does.Contain("UEFI BASELINE KAYDEDİLDİ")
                    .And.Contain("KURGUSAL OS KURULUMUNU AÇ")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("SONRAKİ AŞAMA: OS KURULUMU")
                    .And.Contain("BAKIM KİLİDİ AKTİF"));

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            Assert.That(station.PromptText,
                Does.Contain("KURGUSAL OS KURULUMU")
                    .And.Contain("WORKSHOP STANDARD")
                    .And.Contain("KURULUMU BAŞLAT VE TAMAMLA")
                    .And.Contain("GÜCÜ KAPAT"));

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetFictionalOsInstallation(
                out PcFictionalOsInstallationAuthority authority), Is.True);
            OperationResult<PcFictionalOsInstallationReceipt> installed =
                authority.EvaluateInstalledOperatingSystem();
            Assert.That(installed.IsSuccess, Is.True, installed.Error.Code);
            Assert.That(installed.Value.SourceFirmwareBaselineReceipt,
                Is.SameAs(firmware));
            Assert.That(installed.Value.StorageItemId,
                Is.EqualTo(session.AssemblyBuild.StorageItemId));
            Assert.That(installed.Value.Profile,
                Is.EqualTo(PcFictionalOsProfile.WorkshopStandard));
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False);
            Assert.That(station.PromptText,
                Does.Contain("KURGUSAL OS KURULDU")
                    .And.Contain("SONRAKİ AŞAMA: DRIVER")
                    .And.Contain("GÜCÜ KAPAT"));
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasInstalledFictionalOs, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("KURGUSAL OS KURULDU")
                    .And.Contain("WORKSHOP STANDARD")
                    .And.Contain("SONRAKİ AŞAMA: DRIVER")
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
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess,
                Is.True);

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(authority.EvaluateInstalledOperatingSystem().Value,
                Is.SameAs(installed.Value));
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasInstalledFictionalOs, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("DEPOLAMADA KALICI")
                    .And.Contain("POWER-ON BEKLİYOR")
                    .And.Contain("SONRAKİ AŞAMA: DRIVER"));
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalOsGamepadUsesSameInstallAndPowerOffPath()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;
            PressGamepadInteractForFirmware(gamepad, station.ProcessInputFrame);
            yield return null;
            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;
            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;
            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("RT")
                    .And.Contain("KURULUMU BAŞLAT VE TAMAMLA")
                    .And.Contain("A")
                    .And.Contain("GÜCÜ KAPAT"));

            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetFictionalOsInstallation(
                out PcFictionalOsInstallationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateInstalledOperatingSystem().IsSuccess,
                Is.True);
            Assert.That(station.PromptText,
                Does.Contain("A").And.Contain("GÜCÜ KAPAT")
                    .And.Contain("KURGUSAL OS KURULDU"));

            PressGamepadInteractForFirmware(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(authority.EvaluateInstalledOperatingSystem().IsSuccess,
                Is.True);
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalOsConcurrentPowerOffWinsPrimaryEdge()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
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

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            station.ProcessInputFrame();

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator FictionalOsPauseAndCompetingOwnerDoNotConsume()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
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

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False);
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            yield return null;

            var competingItem = CreateCompetingInteractItem(station);
            Assert.That(marker.PlayerCarry.HasCompetingWorldInteractOwner, Is.True);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False);
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            UnityEngine.Object.DestroyImmediate(competingItem.gameObject);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False,
                "Pause must invalidate an open OS review without consuming input.");
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerMotor.IsPaused, Is.False,
                "The pause edge itself is tested before the motor consumes it.");
            Assert.That(marker.PlayerInput.PausePressedThisFrame, Is.True);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False,
                "A pause edge must invalidate review without consuming primary.");
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);
            competingItem = CreateCompetingInteractItem(station);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False,
                "A competing owner must invalidate an open OS review.");
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            UnityEngine.Object.DestroyImmediate(competingItem.gameObject);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True,
                "Returning to the station must require a fresh review.");
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetFictionalOsInstallation(
                out PcFictionalOsInstallationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.False);
            Assert.That(authority.EvaluateInstalledOperatingSystem().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalOsReviewResetsAfterSpatialAndHandsContextLoss()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
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
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);

            Vector3 target = station.FocusAnchor.position;
            MovePlayerAndAim(
                marker,
                target + (Vector3.back *
                    (station.InteractionRange + 0.65f)),
                target);
            AssertFictionalOsPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "Range loss");
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);

            Transform cameraPivot = marker.PlayerMotor.transform.Find(
                "CameraPivot");
            cameraPivot.rotation = Quaternion.AngleAxis(30f, Vector3.up) *
                                   cameraPivot.rotation;
            Physics.SyncTransforms();
            AssertFictionalOsPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "Focus loss");
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);

            Vector3 origin = station.PlayerCamera.transform.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "FictionalOsReviewLosBlocker";
            blocker.transform.position = Vector3.Lerp(origin, target, 0.5f);
            blocker.transform.localScale = new Vector3(0.45f, 0.55f, 0.12f);
            Physics.SyncTransforms();
            AssertFictionalOsPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "LOS loss");
            UnityEngine.Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();
            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True);
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);

            PhysicalItemProjection largeBox = UnityEngine.Object
                .FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsSortMode.None)
                .Single(item =>
                    item.CarryProfile == PhysicalCarryProfile.LargeBox);
            Assert.That(marker.PlayerCarry.TryPickup(largeBox).IsSuccess,
                Is.True);
            AssertFictionalOsPrimaryInputSurvivesContextLoss(
                marker,
                station,
                mouse,
                "Busy hands");
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            Assert.That(marker.PlayerCarry.TryRecoverHeldItem().IsSuccess,
                Is.True);
            yield return null;

            MovePlayerToPowerTestStation(marker, 1.35f);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFictionalOsInstallation, Is.True,
                "Returning after context loss must reopen review first.");
            Assert.That(session.TryGetFictionalOsInstallation(out _), Is.False);
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.TryGetFictionalOsInstallation(
                out PcFictionalOsInstallationAuthority authority), Is.True);
            Assert.That(authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(authority.EvaluateInstalledOperatingSystem().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FictionalOsMalformedHistoryNeverBlocksPowerOff()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
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

            Assert.That(session.TryGetFictionalOsInstallation(
                out PcFictionalOsInstallationAuthority authority), Is.True);
            System.Reflection.FieldInfo revisionField =
                typeof(PcFictionalOsInstallationAuthority).GetField(
                    "<Revision>k__BackingField",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(revisionField, Is.Not.Null);
            revisionField.SetValue(authority, 2L);

            Assert.That(authority.ValidateReceiptHistory().Error,
                Is.EqualTo(
                    PcFictionalOsInstallationFailures.ReceiptHistoryInvalid));
            Assert.That(station.PromptText, Does.Contain("GÜCÜ KAPAT"));

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.PowerState.ActivePowerOnReceipt, Is.Null);
            Assert.That(session.PowerState.ActivePostStartupReceipt, Is.Null);
            Assert.That(session.PowerState.ActiveFirmwareBaselineReceipt, Is.Null);

            revisionField.SetValue(authority, 1L);
            marker.ElectricalReadinessWorkbench.RefreshPresentation();
            Assert.That(authority.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(authority.EvaluateInstalledOperatingSystem().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertFictionalOsPrimaryInputSurvivesContextLoss(
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
            Assert.That(station.IsReviewingFictionalOsInstallation,
                Is.False,
                context + " must invalidate an open OS review.");
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }
    }
}
