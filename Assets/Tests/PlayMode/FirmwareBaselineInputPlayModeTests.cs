using System;
using System.Collections;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class PowerTestPreflightInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator FirmwareBaselineKeyboardMouseReviewsSavesAndPowersOff()
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

            PcPowerStateAuthority powerState = session.PowerState;
            PcPostStartupReceipt postStartup =
                powerState.ActivePostStartupReceipt;
            Assert.That(postStartup, Is.Not.Null);
            Assert.That(station.PromptText,
                Does.Contain("GÜCÜ KAPAT")
                    .And.Contain("POST GEÇTİ")
                    .And.Contain("UEFI SETUP'I AÇ"));
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("UEFI SETUP BEKLİYOR")
                    .And.Contain("BAKIM KİLİDİ AKTİF"));

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            Assert.That(station.IsReviewingFirmwareBaseline, Is.True);
            Assert.That(powerState.FirmwareBaselineReceiptCount, Is.Zero);
            Assert.That(station.PromptText,
                Does.Contain("UEFI SETUP")
                    .And.Contain("OPTIMIZED DEFAULTS")
                    .And.Contain("KAYDET VE ÇIK")
                    .And.Contain("GÜCÜ KAPAT"));

            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;

            PcFirmwareBaselineReceipt firmware =
                powerState.ActiveFirmwareBaselineReceipt;
            Assert.That(firmware, Is.Not.Null);
            Assert.That(firmware.SourcePostStartupReceipt, Is.SameAs(postStartup));
            Assert.That(firmware.Profile,
                Is.EqualTo(PcFirmwareBaselineProfile.OptimizedDefaults));
            Assert.That(station.IsReviewingFirmwareBaseline, Is.False);
            Assert.That(powerState.FirmwareBaselineRevision, Is.EqualTo(1));
            Assert.That(powerState.FirmwareBaselineReceiptCount, Is.EqualTo(1));
            Assert.That(station.PromptText,
                Does.Contain("GÜCÜ KAPAT")
                    .And.Contain("UEFI BASELINE KAYDEDİLDİ")
                    .And.Contain("SONRAKİ AŞAMA: OS"));
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasCurrentFirmwareBaseline, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("UEFI BASELINE KAYDEDİLDİ")
                    .And.Contain("SONRAKİ AŞAMA: OS")
                    .And.Contain("BAKIM KİLİDİ AKTİF"));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
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
            Assert.That(powerState.ActiveFirmwareBaselineReceipt, Is.Null);
            Assert.That(powerState.FirmwareBaselineRevision, Is.EqualTo(1));
            Assert.That(powerState.FirmwareBaselineReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.TryGetFirmwareBaselineReceipt(
                    firmware.OperationId,
                    out PcFirmwareBaselineReceipt historical),
                Is.True);
            Assert.That(historical, Is.SameAs(firmware));
            Assert.That(powerState.TrySaveFirmwareBaseline(
                    firmware.OperationId,
                    postStartup,
                    firmware.ExpectedPowerStateRevision,
                    firmware.ExpectedRevision).Value,
                Is.SameAs(firmware));
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasCurrentFirmwareBaseline, Is.False);
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FirmwareBaselineGamepadReviewsSavesAndPowersOff()
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
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(station.IsReviewingFirmwareBaseline, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("RT").And.Contain("KAYDET VE ÇIK")
                    .And.Contain("A").And.Contain("GÜCÜ KAPAT"));

            PressGamepadPrimary(gamepad, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount,
                Is.EqualTo(1));
            Assert.That(session.PowerState.ActiveFirmwareBaselineReceipt,
                Is.Not.Null);
            Assert.That(station.PromptText,
                Does.Contain("A").And.Contain("GÜCÜ KAPAT")
                    .And.Contain("UEFI BASELINE KAYDEDİLDİ"));

            PressGamepadInteractForFirmware(gamepad, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.PowerState.ActiveFirmwareBaselineReceipt, Is.Null);
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount,
                Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FirmwareBaselineConcurrentPowerOffWinsPrimaryEdge()
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

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            station.ProcessInputFrame();

            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(session.PowerState.Revision, Is.EqualTo(2));
            Assert.That(session.PowerState.ReceiptCount, Is.EqualTo(2));
            Assert.That(session.PowerState.FirmwareBaselineRevision, Is.Zero);
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount, Is.Zero);
            Assert.That(station.IsReviewingFirmwareBaseline, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator FirmwareBaselinePauseAndCompetingOwnerDoNotConsume()
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

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.PrimaryActionPressedThisFrame, Is.True);
            Assert.That(station.IsReviewingFirmwareBaseline, Is.False);
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount, Is.Zero);
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
            Assert.That(station.IsReviewingFirmwareBaseline, Is.False);
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount, Is.Zero);
            UnityEngine.Object.DestroyImmediate(competingItem.gameObject);
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            yield return null;

            PressPrimary(mouse, station.ProcessInputFrame);
            yield return null;
            Assert.That(station.IsReviewingFirmwareBaseline, Is.True);
            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.PowerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(station.IsReviewingFirmwareBaseline, Is.False);
            Assert.That(session.PowerState.FirmwareBaselineReceiptCount, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator FirmwareBaselineMalformedHistoryNeverBlocksPowerOff()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
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

            PcPowerStateAuthority powerState = session.PowerState;
            System.Reflection.FieldInfo revisionField =
                typeof(PcPowerStateAuthority).GetField(
                    "<FirmwareBaselineRevision>k__BackingField",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic);
            Assert.That(revisionField, Is.Not.Null);
            revisionField.SetValue(powerState, 1L);

            Assert.That(powerState.ValidateReceiptHistory().Error,
                Is.EqualTo(PcFirmwareBaselineFailures.ReceiptHistoryInvalid));
            Assert.That(station.PromptText, Does.Contain("GÜCÜ KAPAT"));

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.Revision, Is.EqualTo(2));
            Assert.That(powerState.ActivePowerOnReceipt, Is.Null);
            Assert.That(powerState.ActivePostStartupReceipt, Is.Null);
            Assert.That(powerState.ActiveFirmwareBaselineReceipt, Is.Null);

            revisionField.SetValue(powerState, 0L);
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PressPrimary(Mouse mouse, Action processInput)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            processInput();
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        private static void PressGamepadPrimary(
            Gamepad gamepad,
            Action processInput)
        {
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            processInput();
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        private static void PressGamepadInteractForFirmware(
            Gamepad gamepad,
            Action processInput)
        {
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
            InputSystem.Update();
            processInput();
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }
    }
}
