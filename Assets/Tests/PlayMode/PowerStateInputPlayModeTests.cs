using System;
using System.Collections;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
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
        public IEnumerator KeyboardInteractRunsPreflightPowerOnAndPowerOff()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            MovePlayerToPowerTestStation(marker, 1.35f);
            Assert.That(session.TryGetPowerState(out _), Is.False);

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;
            Assert.That(session.PowerTestAttempts.HasCompletedPreflight, Is.True);
            Assert.That(session.TryGetPowerState(out _), Is.False,
                "The presentation observer must not create the power authority.");
            Assert.That(station.PromptText,
                Does.Contain("ÖN KONTROL GEÇTİ").And.Contain("GÜCÜ AÇ"));

            long inventoryRevision = session.Inventory.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(session.TryGetPowerState(
                out PcPowerStateAuthority powerState), Is.True);
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Energized));
            Assert.That(powerState.Revision, Is.EqualTo(1));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(1));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.EvaluateCurrentStartupSelfTest().Value.Result,
                Is.EqualTo(PcPostStartupResult.Passed));
            Assert.That(session.AssemblyBuild.IsElectricallyEnergized, Is.True);
            Assert.That(station.PromptText,
                Does.Contain("GÜCÜ KAPAT").And.Contain("POST GEÇTİ"));
            Assert.That(marker.ElectricalReadinessWorkbench.IsEnergized, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasCurrentPostStartupPass, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("GÜÇ AÇIK")
                    .And.Contain("POST GEÇTİ")
                    .And.Contain("UEFI SETUP BEKLİYOR")
                    .And.Contain("BAKIM KİLİDİ AKTİF"));
            Assert.That(session.UnroutePcieGpuPowerCable(
                    AssemblyOperationId("blocked-unroute-powered"),
                    session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId,
                    session.AssemblyBuild.PcieGpuPowerCableRevision).Error,
                Is.EqualTo(AssemblyFailures.ElectricalPowerOnMaintenanceBlocked));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieRevision));

            PressInteract(keyboard, station.ProcessInputFrame);
            yield return null;

            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.Revision, Is.EqualTo(2));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(2));
            Assert.That(powerState.ActivePowerOnReceipt, Is.Null);
            Assert.That(powerState.ActivePostStartupReceipt, Is.Null);
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(1));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.EvaluateCurrentStartupSelfTest().Error,
                Is.EqualTo(PcPostStartupFailures.NotCurrent));
            Assert.That(session.AssemblyBuild.IsElectricallyEnergized, Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.IsEnergized, Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasCurrentPostStartupPass, Is.False);
            Assert.That(station.PromptText,
                Does.Contain("ÖN KONTROL GEÇTİ").And.Contain("GÜCÜ AÇ"));
            Assert.That(session.UnroutePcieGpuPowerCable(
                    AssemblyOperationId("allowed-unroute-after-power-off"),
                    session.AssemblyBuild.PcieGpuPowerCableRoutedByOperationId,
                    session.AssemblyBuild.PcieGpuPowerCableRevision).IsSuccess,
                Is.True);
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.PowerCableMissing));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadUsesSamePowerOnAndPowerOffTransitions()
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
            PcPowerStateAuthority powerState = session.PowerState;

            PressGamepadInteract(gamepad, station.ProcessInputFrame);
            yield return null;
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Energized));
            Assert.That(station.PromptText,
                Does.Contain("A").And.Contain("GÜCÜ KAPAT")
                    .And.Contain("POST GEÇTİ"));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));

            PressGamepadInteract(gamepad, station.ProcessInputFrame);
            yield return null;
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.Revision, Is.EqualTo(2));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(2));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(station.PromptText,
                Does.Contain("A").And.Contain("GÜCÜ AÇ"));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator ConcurrentKeyboardGamepadPressRunsOneTransitionAndPauseBlocksPowerOff()
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

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
            InputSystem.Update();
            station.ProcessInputFrame();
            station.ProcessInputFrame();

            Assert.That(session.TryGetPowerState(
                out PcPowerStateAuthority powerState), Is.True);
            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Energized));
            Assert.That(powerState.Revision, Is.EqualTo(1));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.PostStartupRevision, Is.EqualTo(1));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            yield return null;

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            station.ProcessInputFrame();

            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Energized));
            Assert.That(powerState.Revision, Is.EqualTo(1));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True,
                "A paused station must not consume or execute the power-off press.");

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);
            yield return null;

            PressGamepadInteract(gamepad, station.ProcessInputFrame);
            yield return null;

            Assert.That(powerState.State, Is.EqualTo(PcPowerState.Off));
            Assert.That(powerState.Revision, Is.EqualTo(2));
            Assert.That(powerState.ReceiptCount, Is.EqualTo(2));
            Assert.That(powerState.PostStartupReceiptCount, Is.EqualTo(1));
            Assert.That(powerState.ValidateReceiptHistory().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static StableId<AssemblyOperationIdScope> AssemblyOperationId(
            string suffix)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                "assembly.operation.issue125.playmode." + suffix);
        }

        private static void PressGamepadInteract(
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
