using System;
using System.Collections;
using System.Collections.Generic;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string FictionalDriverInstallationSmokeSuccessMarker =
            "GARAGE_FICTIONAL_DRIVER_INSTALLATION_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted preflight=current " +
            "power-on=player-triggered post=passed " +
            "firmware=optimized-defaults-saved " +
            "os=workshop-standard-installed " +
            "driver=workshop-driver-bundle-installed " +
            "storage=identity-bound review=player-triggered " +
            "install=player-triggered input=keyboard+mouse+gamepad " +
            "power-off=player-triggered state=off " +
            "persistence=power-off-preserved receipt=immutable " +
            "replay=ok benchmark=untouched invariants=ok";

        private string _nestedFictionalDriverInstallationSmokeFailureCode;
        private bool _suppressFictionalDriverInstallationSmokeSuccessMarker;

        public bool HasFictionalDriverInstallationR65Runtime =>
            HasFictionalOsInstallationR64Runtime &&
            electricalPowerTestStation != null &&
            electricalReadinessWorkbench != null;

        private IEnumerator RunFictionalDriverInstallationSmoke()
        {
            return RunFictionalDriverInstallationSmokeGuarded(
                RunFictionalDriverInstallationSmokeCore());
        }

        private IEnumerator RunFictionalDriverInstallationSmokeCore()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedFictionalOsInstallationSmokeFailureCode = null;
            _suppressFictionalOsInstallationSmokeSuccessMarker = true;
            try
            {
                yield return RunFictionalOsInstallationSmoke();
            }
            finally
            {
                _suppressFictionalOsInstallationSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedFictionalOsInstallationSmokeFailureCode;
            _nestedFictionalOsInstallationSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogFictionalDriverInstallationSmokeFailure(
                    "smoke.os-prerequisite-" + suffix);
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
                !HasFictionalDriverInstallationR65Runtime ||
                !pcieGpuPowerCableBinding.IsRouted ||
                session.PowerState == null ||
                session.PowerState.State != PcPowerState.Off ||
                session.PowerState.Revision != 6 ||
                session.PowerState.PostStartupRevision != 3 ||
                session.PowerState.FirmwareBaselineRevision != 2 ||
                !session.TryGetFictionalOsInstallation(
                    out PcFictionalOsInstallationAuthority osAuthority) ||
                osAuthority.Revision != 1 || osAuthority.ReceiptCount != 1 ||
                osAuthority.EvaluateInstalledOperatingSystem().IsFailure ||
                session.TryGetFictionalDriverInstallation(out _))
            {
                LogFictionalDriverInstallationSmokeFailure(
                    "smoke.context-mismatch");
                yield break;
            }

            PcFictionalOsInstallationReceipt osReceipt =
                osAuthority.EvaluateInstalledOperatingSystem().Value;
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
            StableId<ItemInstanceIdScope> storageItemId =
                session.AssemblyBuild.StorageItemId;

            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            Gamepad smokeGamepad = null;
            PcFirmwareBaselineReceipt completionFirmware = null;
            PcFictionalDriverInstallationReceipt driverReceipt = null;
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
                    LogFictionalDriverInstallationSmokeFailure(
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
                if (powerState.State != PcPowerState.Energized ||
                    powerState.Revision != 7 ||
                    powerState.PostStartupRevision != 4 ||
                    powerState.FirmwareBaselineRevision != 2 ||
                    station.InspectFirmwareInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains("UEFI SETUP'I AÇ"))
                {
                    LogFictionalDriverInstallationSmokeFailure(
                        "smoke.post-or-firmware-gate-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeMouse,
                    new MouseState { buttons = 1 });
                yield return null;
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                yield return null;
                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState { rightTrigger = 1f });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                completionFirmware = powerState.ActiveFirmwareBaselineReceipt;
                if (completionFirmware == null ||
                    powerState.FirmwareBaselineRevision != 3 ||
                    powerState.EvaluateCurrentFirmwareBaseline().IsFailure ||
                    ReferenceEquals(
                        completionFirmware,
                        osReceipt.SourceFirmwareBaselineReceipt) ||
                    station.InspectFictionalDriverInteractionGateForTests()
                        .IsFailure ||
                    !station.PromptText.Contains("DRIVER KURULUMUNU AÇ") ||
                    session.TryGetFictionalDriverInstallation(out _))
                {
                    LogFictionalDriverInstallationSmokeFailure(
                        "smoke.firmware-or-driver-gate-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeMouse,
                    new MouseState { buttons = 1 });
                yield return null;
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                yield return null;
                if (!station.IsReviewingFictionalDriverInstallation ||
                    !station.PromptText.Contains("WORKSHOP DRIVER BUNDLE") ||
                    !station.PromptText.Contains(
                        "KURULUMU BAŞLAT VE TAMAMLA") ||
                    session.TryGetFictionalDriverInstallation(out _))
                {
                    LogFictionalDriverInstallationSmokeFailure(
                        "smoke.driver-review-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState { rightTrigger = 1f });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                if (!session.TryGetFictionalDriverInstallation(
                        out PcFictionalDriverInstallationAuthority authority))
                {
                    LogFictionalDriverInstallationSmokeFailure(
                        "smoke.driver-authority-missing");
                    yield break;
                }

                OperationResult<PcFictionalDriverInstallationReceipt>
                    installed = authority.EvaluateInstalledDrivers();
                driverReceipt = installed.TryGetValue(
                    out PcFictionalDriverInstallationReceipt value)
                        ? value
                        : null;
                OperationResult<PcFictionalDriverInstallationReceipt> replay =
                    driverReceipt == null
                        ? OperationResult<
                            PcFictionalDriverInstallationReceipt>.Fail(
                            PcFictionalDriverInstallationFailures
                                .ReceiptHistoryInvalid)
                        : authority.TryCompleteInstallation(
                            driverReceipt.OperationId,
                            osReceipt,
                            completionFirmware,
                            storageItemId,
                            driverReceipt.ExpectedPowerStateRevision,
                            driverReceipt.ExpectedRevision);
                electricalReadinessWorkbench.RefreshPresentation();
                if (!playerInput.UsesGamepadPrompts ||
                    station.IsReviewingFictionalDriverInstallation ||
                    driverReceipt == null || replay.IsFailure ||
                    !ReferenceEquals(replay.Value, driverReceipt) ||
                    driverReceipt.Profile !=
                        PcFictionalDriverProfile.WorkshopDriverBundle ||
                    driverReceipt.Result !=
                        PcFictionalDriverInstallationResult
                            .InstalledForBenchmarkStage ||
                    !ReferenceEquals(
                        driverReceipt.SourceOperatingSystemReceipt,
                        osReceipt) ||
                    !ReferenceEquals(
                        driverReceipt.SourceFirmwareBaselineReceipt,
                        completionFirmware) ||
                    ReferenceEquals(
                        driverReceipt.SourceFirmwareBaselineReceipt,
                        osReceipt.SourceFirmwareBaselineReceipt) ||
                    driverReceipt.StorageItemId != storageItemId ||
                    authority.Revision != 1 || authority.ReceiptCount != 1 ||
                    !station.PromptText.Contains(
                        "WORKSHOP DRIVER BUNDLE KURULDU") ||
                    !station.PromptText.Contains("SONRAKİ AŞAMA: VALIDATION") ||
                    !electricalReadinessWorkbench
                        .HasInstalledFictionalDrivers ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "WORKSHOP DRIVER BUNDLE KURULDU") ||
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().IsFailure)
                {
                    LogFictionalDriverInstallationSmokeFailure(
                        "smoke.install-replay-or-presentation-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                yield return null;
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                yield return null;

                electricalReadinessWorkbench.RefreshPresentation();
                OperationResult<PcFictionalDriverInstallationReceipt>
                    historicalReplay = authority.TryCompleteInstallation(
                        driverReceipt.OperationId,
                        osReceipt,
                        completionFirmware,
                        storageItemId,
                        driverReceipt.ExpectedPowerStateRevision,
                        driverReceipt.ExpectedRevision);
                if (powerState.State != PcPowerState.Off ||
                    powerState.Revision != 8 ||
                    powerState.ActivePowerOnReceipt != null ||
                    powerState.ActivePostStartupReceipt != null ||
                    powerState.ActiveFirmwareBaselineReceipt != null ||
                    historicalReplay.IsFailure ||
                    !ReferenceEquals(historicalReplay.Value, driverReceipt) ||
                    authority.EvaluateInstalledDrivers().Value !=
                        driverReceipt ||
                    !electricalReadinessWorkbench
                        .HasInstalledFictionalDrivers ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "DEPOLAMADA KALICI") ||
                    authority.ValidateReceiptHistory().IsFailure ||
                    osAuthority.ValidateReceiptHistory().IsFailure ||
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
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().IsFailure ||
                    session.ValidateInvariants().IsFailure)
                {
                    LogFictionalDriverInstallationSmokeFailure(
                        "smoke.power-off-persistence-or-invariant-mismatch");
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

            if (!_suppressFictionalDriverInstallationSmokeSuccessMarker)
            {
                Debug.Log(FictionalDriverInstallationSmokeSuccessMarker);
                yield return new WaitForEndOfFrame();
                if (!Application.isEditor)
                {
                    Application.Quit(0);
                }
            }
        }

        private IEnumerator RunFictionalDriverInstallationSmokeGuarded(
            IEnumerator root)
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
                        LogFictionalDriverInstallationSmokeFailure(
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

        private void LogFictionalDriverInstallationSmokeFailure(string code)
        {
            if (_suppressFictionalDriverInstallationSmokeSuccessMarker)
            {
                _nestedFictionalDriverInstallationSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_FICTIONAL_DRIVER_INSTALLATION_RUNTIME_SMOKE " +
                "driver-flow=failed code=" + code);
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
