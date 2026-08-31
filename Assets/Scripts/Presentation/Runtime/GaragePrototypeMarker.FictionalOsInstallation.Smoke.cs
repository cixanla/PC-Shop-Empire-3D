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
        public const string FictionalOsInstallationSmokeSuccessMarker =
            "GARAGE_FICTIONAL_OS_INSTALLATION_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted preflight=current " +
            "power-on=player-triggered post=passed " +
            "firmware=optimized-defaults-saved " +
            "os=workshop-standard-installed storage=identity-bound " +
            "review=player-triggered install=player-triggered " +
            "input=keyboard+mouse+gamepad power-off=player-triggered " +
            "state=off persistence=power-off-preserved receipt=immutable " +
            "replay=ok benchmark=untouched invariants=ok";

        public bool HasFictionalOsInstallationR64Runtime =>
            HasFirmwareBaselineR63Runtime &&
            electricalPowerTestStation != null &&
            electricalReadinessWorkbench != null;

        private IEnumerator RunFictionalOsInstallationSmoke()
        {
            return RunFictionalOsInstallationSmokeGuarded(
                RunFictionalOsInstallationSmokeCore());
        }

        private IEnumerator RunFictionalOsInstallationSmokeCore()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedFirmwareBaselineSmokeFailureCode = null;
            _suppressFirmwareBaselineSmokeSuccessMarker = true;
            try
            {
                yield return RunFirmwareBaselineSmoke();
            }
            finally
            {
                _suppressFirmwareBaselineSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedFirmwareBaselineSmokeFailureCode;
            _nestedFirmwareBaselineSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogFictionalOsInstallationSmokeFailure(
                    "smoke.firmware-prerequisite-" + suffix);
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
                !HasFictionalOsInstallationR64Runtime ||
                !pcieGpuPowerCableBinding.IsRouted ||
                session.PowerState == null ||
                session.PowerState.State != PcPowerState.Off ||
                session.PowerState.Revision != 4 ||
                session.PowerState.PostStartupRevision != 2 ||
                session.PowerState.FirmwareBaselineRevision != 1 ||
                session.TryGetFictionalOsInstallation(out _))
            {
                LogFictionalOsInstallationSmokeFailure(
                    "smoke.context-mismatch");
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
            StableId<ItemInstanceIdScope> storageItemId =
                session.AssemblyBuild.StorageItemId;

            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            Gamepad smokeGamepad = null;
            PcFirmwareBaselineReceipt firmware = null;
            PcFictionalOsInstallationReceipt osReceipt = null;
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
                    LogFictionalOsInstallationSmokeFailure(
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
                    powerState.Revision != 5 ||
                    powerState.PostStartupRevision != 3 ||
                    powerState.FirmwareBaselineRevision != 1 ||
                    station.InspectFirmwareInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains("UEFI SETUP'I AÇ"))
                {
                    LogFictionalOsInstallationSmokeFailure(
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

                firmware = powerState.ActiveFirmwareBaselineReceipt;
                if (firmware == null ||
                    powerState.FirmwareBaselineRevision != 2 ||
                    powerState.EvaluateCurrentFirmwareBaseline().IsFailure ||
                    station.InspectFictionalOsInteractionGateForTests()
                        .IsFailure ||
                    !station.PromptText.Contains(
                        "KURGUSAL OS KURULUMUNU AÇ") ||
                    session.TryGetFictionalOsInstallation(out _))
                {
                    LogFictionalOsInstallationSmokeFailure(
                        "smoke.firmware-or-os-gate-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeMouse,
                    new MouseState { buttons = 1 });
                yield return null;
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                yield return null;
                if (!station.IsReviewingFictionalOsInstallation ||
                    !station.PromptText.Contains("WORKSHOP STANDARD") ||
                    !station.PromptText.Contains(
                        "KURULUMU BAŞLAT VE TAMAMLA") ||
                    session.TryGetFictionalOsInstallation(out _))
                {
                    LogFictionalOsInstallationSmokeFailure(
                        "smoke.os-review-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState { rightTrigger = 1f });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                if (!session.TryGetFictionalOsInstallation(
                        out PcFictionalOsInstallationAuthority authority))
                {
                    LogFictionalOsInstallationSmokeFailure(
                        "smoke.os-authority-missing");
                    yield break;
                }

                OperationResult<PcFictionalOsInstallationReceipt> installed =
                    authority.EvaluateInstalledOperatingSystem();
                osReceipt = installed.TryGetValue(
                    out PcFictionalOsInstallationReceipt value)
                        ? value
                        : null;
                OperationResult<PcFictionalOsInstallationReceipt> replay =
                    osReceipt == null
                        ? OperationResult<PcFictionalOsInstallationReceipt>.Fail(
                            PcFictionalOsInstallationFailures
                                .ReceiptHistoryInvalid)
                        : authority.TryCompleteInstallation(
                            osReceipt.OperationId,
                            firmware,
                            storageItemId,
                            osReceipt.ExpectedPowerStateRevision,
                            osReceipt.ExpectedRevision);
                electricalReadinessWorkbench.RefreshPresentation();
                if (!playerInput.UsesGamepadPrompts ||
                    station.IsReviewingFictionalOsInstallation ||
                    osReceipt == null || replay.IsFailure ||
                    !ReferenceEquals(replay.Value, osReceipt) ||
                    osReceipt.Profile !=
                        PcFictionalOsProfile.WorkshopStandard ||
                    osReceipt.Result !=
                        PcFictionalOsInstallationResult
                            .InstalledForDriverStage ||
                    !ReferenceEquals(
                        osReceipt.SourceFirmwareBaselineReceipt,
                        firmware) ||
                    osReceipt.StorageItemId != storageItemId ||
                    authority.Revision != 1 || authority.ReceiptCount != 1 ||
                    !station.PromptText.Contains("KURGUSAL OS KURULDU") ||
                    !station.PromptText.Contains("SONRAKİ AŞAMA: DRIVER") ||
                    !electricalReadinessWorkbench.HasInstalledFictionalOs ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "WORKSHOP STANDARD") ||
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().Error !=
                        AssemblyFailures.BuildIncomplete)
                {
                    LogFictionalOsInstallationSmokeFailure(
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
                OperationResult<PcFictionalOsInstallationReceipt>
                    historicalReplay = authority.TryCompleteInstallation(
                        osReceipt.OperationId,
                        firmware,
                        storageItemId,
                        osReceipt.ExpectedPowerStateRevision,
                        osReceipt.ExpectedRevision);
                if (powerState.State != PcPowerState.Off ||
                    powerState.Revision != 6 ||
                    powerState.ActivePowerOnReceipt != null ||
                    powerState.ActivePostStartupReceipt != null ||
                    powerState.ActiveFirmwareBaselineReceipt != null ||
                    historicalReplay.IsFailure ||
                    !ReferenceEquals(historicalReplay.Value, osReceipt) ||
                    authority.EvaluateInstalledOperatingSystem().Value !=
                        osReceipt ||
                    !electricalReadinessWorkbench.HasInstalledFictionalOs ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "DEPOLAMADA KALICI") ||
                    authority.ValidateReceiptHistory().IsFailure ||
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
                    LogFictionalOsInstallationSmokeFailure(
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

            Debug.Log(FictionalOsInstallationSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private IEnumerator RunFictionalOsInstallationSmokeGuarded(
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
                        LogFictionalOsInstallationSmokeFailure(
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

        private static void LogFictionalOsInstallationSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_FICTIONAL_OS_INSTALLATION_RUNTIME_SMOKE " +
                "os-flow=failed code=" + code);
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
