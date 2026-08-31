using System;
using System.Collections;
using System.Collections.Generic;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string FirmwareBaselineSmokeSuccessMarker =
            "GARAGE_FIRMWARE_BASELINE_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted preflight=current " +
            "power-on=player-triggered post=passed " +
            "firmware=optimized-defaults-saved " +
            "review=player-triggered save-exit=player-triggered " +
            "input=keyboard+mouse+gamepad power-off=player-triggered " +
            "state=off receipt=immutable replay=ok active-clear=ok " +
            "history=preserved benchmark=untouched invariants=ok";

        public bool HasFirmwareBaselineR63Runtime =>
            HasPowerStateInterlockR62Runtime &&
            electricalPowerTestStation != null &&
            electricalReadinessWorkbench != null;

        private IEnumerator RunFirmwareBaselineSmoke()
        {
            return RunFirmwareBaselineSmokeGuarded(
                RunFirmwareBaselineSmokeCore());
        }

        private IEnumerator RunFirmwareBaselineSmokeCore()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedPowerStateInterlockSmokeFailureCode = null;
            _suppressPowerStateInterlockSmokeSuccessMarker = true;
            try
            {
                yield return RunPowerStateInterlockSmoke();
            }
            finally
            {
                _suppressPowerStateInterlockSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedPowerStateInterlockSmokeFailureCode;
            _nestedPowerStateInterlockSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogFirmwareBaselineSmokeFailure(
                    "smoke.power-state-prerequisite-" + suffix);
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            ElectricalPowerTestStationProjection station =
                electricalPowerTestStation;
            if (session == null || playerMotor == null || playerInput == null ||
                playerCarry == null || station == null ||
                electricalReadinessWorkbench == null ||
                pcieGpuPowerCableBinding == null ||
                !HasFirmwareBaselineR63Runtime ||
                !pcieGpuPowerCableBinding.IsRouted ||
                session.PowerState == null ||
                session.PowerState.State != PcPowerState.Off ||
                session.PowerState.Revision != 2 ||
                session.PowerState.PostStartupRevision != 1 ||
                session.PowerState.FirmwareBaselineRevision != 0)
            {
                LogFirmwareBaselineSmokeFailure("smoke.context-mismatch");
                yield break;
            }

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            int atx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            int eps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;

            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            Gamepad smokeGamepad = null;
            PcPostStartupReceipt postStartup = null;
            PcFirmwareBaselineReceipt firmware = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeMouse = InputSystem.AddDevice<Mouse>();
                smokeGamepad = InputSystem.AddDevice<Gamepad>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                InputSystem.Update();

                MovePlayerToPowerTestPreflightStation(1.35f);
                if (station.InspectInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains("GÜCÜ AÇ"))
                {
                    LogFirmwareBaselineSmokeFailure(
                        "smoke.power-on-prompt-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                yield return null;
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                yield return null;

                PcPowerStateAuthority powerState = session.PowerState;
                postStartup = powerState.ActivePostStartupReceipt;
                if (powerState.State != PcPowerState.Energized ||
                    powerState.Revision != 3 ||
                    powerState.PostStartupRevision != 2 ||
                    postStartup == null ||
                    powerState.EvaluateCurrentStartupSelfTest().IsFailure ||
                    powerState.FirmwareBaselineReceiptCount != 0 ||
                    station.InspectFirmwareInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains("UEFI SETUP'I AÇ"))
                {
                    LogFirmwareBaselineSmokeFailure(
                        "smoke.post-or-firmware-gate-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeMouse,
                    new MouseState { buttons = 1 });
                yield return null;
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                yield return null;

                if (!station.IsReviewingFirmwareBaseline ||
                    powerState.FirmwareBaselineReceiptCount != 0 ||
                    !station.PromptText.Contains("OPTIMIZED DEFAULTS") ||
                    !station.PromptText.Contains("KAYDET VE ÇIK") ||
                    !station.PromptText.Contains("GÜCÜ KAPAT"))
                {
                    LogFirmwareBaselineSmokeFailure(
                        "smoke.review-presentation-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState { rightTrigger = 1f });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                firmware = powerState.ActiveFirmwareBaselineReceipt;
                OperationResult<PcFirmwareBaselineReceipt> replay =
                    firmware == null
                        ? OperationResult<PcFirmwareBaselineReceipt>.Fail(
                            PcFirmwareBaselineFailures.ReceiptHistoryInvalid)
                        : powerState.TrySaveFirmwareBaseline(
                            firmware.OperationId,
                            postStartup,
                            firmware.ExpectedPowerStateRevision,
                            firmware.ExpectedRevision);
                OperationResult blocked = playerCarry.TryPickup(
                    pcieGpuPowerCableBinding.PhysicalItem);
                electricalReadinessWorkbench.RefreshPresentation();
                if (!playerInput.UsesGamepadPrompts ||
                    station.IsReviewingFirmwareBaseline ||
                    firmware == null || replay.IsFailure ||
                    !ReferenceEquals(replay.Value, firmware) ||
                    firmware.Profile !=
                        PcFirmwareBaselineProfile.OptimizedDefaults ||
                    firmware.Result != PcFirmwareBaselineResult.SavedAndExited ||
                    !ReferenceEquals(
                        firmware.SourcePostStartupReceipt,
                        postStartup) ||
                    powerState.FirmwareBaselineRevision != 1 ||
                    powerState.FirmwareBaselineReceiptCount != 1 ||
                    powerState.EvaluateCurrentFirmwareBaseline().IsFailure ||
                    blocked.Error !=
                        AssemblyFailures.ElectricalPowerOnMaintenanceBlocked ||
                    !station.PromptText.Contains("GÜCÜ KAPAT") ||
                    !station.PromptText.Contains(
                        "UEFI BASELINE KAYDEDİLDİ") ||
                    !electricalReadinessWorkbench
                        .HasCurrentFirmwareBaseline ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "SONRAKİ AŞAMA: OS") ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "BAKIM KİLİDİ AKTİF"))
                {
                    LogFirmwareBaselineSmokeFailure(
                        "smoke.save-replay-or-presentation-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                yield return null;
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                yield return null;

                electricalReadinessWorkbench.RefreshPresentation();
                OperationResult<PcFirmwareBaselineReceipt> historicalReplay =
                    powerState.TrySaveFirmwareBaseline(
                        firmware.OperationId,
                        postStartup,
                        firmware.ExpectedPowerStateRevision,
                        firmware.ExpectedRevision);
                if (powerState.State != PcPowerState.Off ||
                    powerState.Revision != 4 ||
                    powerState.PostStartupRevision != 2 ||
                    powerState.FirmwareBaselineRevision != 1 ||
                    powerState.FirmwareBaselineReceiptCount != 1 ||
                    powerState.ActivePowerOnReceipt != null ||
                    powerState.ActivePostStartupReceipt != null ||
                    powerState.ActiveFirmwareBaselineReceipt != null ||
                    powerState.EvaluateCurrentFirmwareBaseline().Error !=
                        PcFirmwareBaselineFailures.NotCurrent ||
                    historicalReplay.IsFailure ||
                    !ReferenceEquals(historicalReplay.Value, firmware) ||
                    electricalReadinessWorkbench
                        .HasCurrentFirmwareBaseline ||
                    powerState.ValidateReceiptHistory().IsFailure ||
                    !PowerTestSmokeGameplayStateUnchanged(
                        session,
                        inventoryRevision,
                        buildKitRevision,
                        assemblyRevision,
                        assemblyReceiptCount,
                        atx24Revision,
                        eps12vRevision,
                        pcieRevision,
                        atx24ReceiptCount,
                        eps12vReceiptCount,
                        pcieReceiptCount) ||
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().Error !=
                        AssemblyFailures.BuildIncomplete ||
                    session.ValidateInvariants().IsFailure)
                {
                    LogFirmwareBaselineSmokeFailure(
                        "smoke.power-off-history-or-invariant-mismatch");
                    yield break;
                }
            }
            finally
            {
                if (smokeMouse != null && smokeMouse.added)
                {
                    InputSystem.RemoveDevice(smokeMouse);
                }

                RemoveCustomPcWorkTicketSmokeDevices(
                    smokeKeyboard,
                    smokeGamepad);
            }

            Debug.Log(FirmwareBaselineSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private IEnumerator RunFirmwareBaselineSmokeGuarded(IEnumerator root)
        {
            var routines = new Stack<IEnumerator>();
            routines.Push(root);
            try
            {
                while (routines.Count > 0)
                {
                    IEnumerator active = routines.Peek();
                    bool moved = false;
                    object yielded = null;
                    Exception failure = null;
                    try
                    {
                        moved = active.MoveNext();
                        if (moved)
                        {
                            yielded = active.Current;
                        }
                    }
                    catch (Exception exception)
                    {
                        failure = exception;
                    }

                    if (failure != null)
                    {
                        Debug.LogException(failure);
                        LogFirmwareBaselineSmokeFailure(
                            "smoke.unhandled-exception");
                        yield break;
                    }

                    if (!moved)
                    {
                        routines.Pop();
                        (active as IDisposable)?.Dispose();
                        continue;
                    }

                    if (yielded is IEnumerator nested)
                    {
                        routines.Push(nested);
                        continue;
                    }

                    yield return yielded;
                }
            }
            finally
            {
                while (routines.Count > 0)
                {
                    (routines.Pop() as IDisposable)?.Dispose();
                }
            }
        }

        private static void LogFirmwareBaselineSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_FIRMWARE_BASELINE_RUNTIME_SMOKE " +
                "firmware-flow=failed code=" + code);
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
