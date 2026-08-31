using System;
using System.Collections;
using System.Collections.Generic;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string PowerStateInterlockSmokeSuccessMarker =
            "GARAGE_POWER_STATE_INTERLOCK_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted preflight=current " +
            "power-on=player-triggered power-off=player-triggered " +
            "input=keyboard+gamepad state=off cycles=1 " +
            "maintenance-while-energized=blocked receipt=immutable " +
            "replay=ok presentation=ok post=not-started " +
            "benchmark=untouched invariants=ok";

        public bool HasPowerStateInterlockR61Runtime
        {
            get
            {
                if (!HasPowerTestPreflightR60Runtime || stockFlow == null ||
                    !stockFlow.TryGetInitializedSession(
                        out GarageStockFlowSession session))
                {
                    return HasPowerTestPreflightR60Runtime;
                }

                if (!session.TryGetPowerState(
                        out PcPowerStateAuthority powerState))
                {
                    return !session.AssemblyBuild.IsElectricallyEnergized;
                }

                return ReferenceEquals(powerState.AssemblyBuild,
                           session.AssemblyBuild) &&
                       powerState.IsEnergized ==
                           session.AssemblyBuild.IsElectricallyEnergized &&
                       powerState.ValidateReceiptHistory().IsSuccess;
            }
        }

        private IEnumerator RunPowerStateInterlockSmoke()
        {
            return RunPowerStateInterlockSmokeGuarded(
                RunPowerStateInterlockSmokeCore());
        }

        private IEnumerator RunPowerStateInterlockSmokeCore()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode = null;
            _suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker = true;
            try
            {
                yield return RunPcieGpuPowerCableAssemblyHandoffSmoke();
            }
            finally
            {
                _suppressPcieGpuPowerCableAssemblyHandoffSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode;
            _nestedPcieGpuPowerCableAssemblyHandoffSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogPowerStateInterlockSmokeFailure(
                    "smoke.pcie-prerequisite-" + suffix);
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
                !HasPowerStateInterlockR61Runtime ||
                playerCarry.HeldItem != pcieGpuPowerCableBinding.PhysicalItem ||
                !pcieGpuPowerCableBinding.IsAuthorityInHands)
            {
                LogPowerStateInterlockSmokeFailure("smoke.context-mismatch");
                yield break;
            }

            MovePlayerToPcieGpuPowerCableRoute();
            OperationResult routeMode =
                playerCarry.TrySetPcieGpuPowerCableRouteMode(true);
            OperationResult route = routeMode.IsSuccess
                ? playerCarry.TryConfirmPcieGpuPowerCableRoute()
                : routeMode;
            OperationResult readiness =
                electricalReadinessWorkbench.RefreshPresentation();
            if (route.IsFailure || readiness.IsFailure ||
                playerCarry.HeldItem != null ||
                !pcieGpuPowerCableBinding.IsRouted ||
                session.PowerTestAttempts == null ||
                session.PowerTestAttempts.ReceiptCount != 0)
            {
                LogPowerStateInterlockSmokeFailure(
                    route.IsFailure
                        ? "smoke.ready-route-" + route.Error.Code
                        : "smoke.ready-context-mismatch");
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
            Gamepad smokeGamepad = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeGamepad = InputSystem.AddDevice<Gamepad>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                InputSystem.Update();

                MovePlayerToPowerTestPreflightStation(1.35f);
                if (station.InspectInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains(
                        "GÜÇ TESTİ ÖN KONTROLÜ"))
                {
                    LogPowerStateInterlockSmokeFailure(
                        "smoke.preflight-prompt-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                yield return null;
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                yield return null;

                PowerTestAttemptAuthority attempts = session.PowerTestAttempts;
                if (attempts.Revision != 1 || attempts.ReceiptCount != 1 ||
                    station.InspectInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains("GÜCÜ AÇ"))
                {
                    LogPowerStateInterlockSmokeFailure(
                        "smoke.preflight-transition-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.South
                    });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                OperationResult<PcPowerStateAuthority> ensuredPowerState =
                    session.EnsurePowerStateAuthority();
                if (ensuredPowerState.IsFailure)
                {
                    LogPowerStateInterlockSmokeFailure(
                        "smoke.power-authority-" +
                        ensuredPowerState.Error.Code);
                    yield break;
                }

                PcPowerStateAuthority powerState = ensuredPowerState.Value;
                PcPowerStateReceipt powerOn = powerState.ActivePowerOnReceipt;
                OperationResult blocked = playerCarry.TryPickup(
                    pcieGpuPowerCableBinding.PhysicalItem);
                OperationResult<PcPowerStateReceipt> powerOnReplay =
                    powerOn == null
                        ? OperationResult<PcPowerStateReceipt>.Fail(
                            PcPowerStateFailures.ReceiptHistoryInvalid)
                        : powerState.TryPowerOn(
                            powerOn.OperationId,
                            powerOn.PreflightReceipt,
                            powerOn.ExpectedRevision);
                electricalReadinessWorkbench.RefreshPresentation();
                if (!playerInput.UsesGamepadPrompts ||
                    powerState.State != PcPowerState.Energized ||
                    powerState.Revision != 1 || powerState.ReceiptCount != 1 ||
                    powerOn == null || powerOnReplay.IsFailure ||
                    !ReferenceEquals(powerOnReplay.Value, powerOn) ||
                    blocked.Error !=
                        AssemblyFailures.ElectricalPowerOnMaintenanceBlocked ||
                    playerCarry.HeldItem != null ||
                    !pcieGpuPowerCableBinding.IsRouted ||
                    pcieGpuPowerCableBinding.PhysicalItem.Ownership !=
                        PhysicalItemOwnership.World ||
                    !session.AssemblyBuild.IsElectricallyEnergized ||
                    !station.PromptText.Contains("GÜCÜ KAPAT") ||
                    !station.PromptText.Contains("POST BEKLİYOR") ||
                    !electricalReadinessWorkbench.IsEnergized ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "BAKIM KİLİDİ AKTİF"))
                {
                    LogPowerStateInterlockSmokeFailure(
                        "smoke.power-on-or-interlock-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.South
                    });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                electricalReadinessWorkbench.RefreshPresentation();
                bool hasPowerOff = powerState.TryGetReceipt(
                    session.PrototypePowerOffOperationId,
                    out PcPowerStateReceipt powerOff);
                OperationResult<PcPowerStateReceipt> powerOffReplay =
                    hasPowerOff
                        ? powerState.TryPowerOff(
                            powerOff.OperationId,
                            powerOn,
                            powerOff.ExpectedRevision)
                        : OperationResult<PcPowerStateReceipt>.Fail(
                            PcPowerStateFailures.ReceiptHistoryInvalid);
                if (powerState.State != PcPowerState.Off ||
                    powerState.Revision != 2 || powerState.ReceiptCount != 2 ||
                    powerState.ActivePowerOnReceipt != null ||
                    !hasPowerOff || powerOffReplay.IsFailure ||
                    !ReferenceEquals(powerOffReplay.Value, powerOff) ||
                    session.AssemblyBuild.IsElectricallyEnergized ||
                    !station.PromptText.Contains("GÜCÜ AÇ") ||
                    electricalReadinessWorkbench.IsEnergized ||
                    powerState.ValidateReceiptHistory().IsFailure ||
                    attempts.ValidateReceiptHistory().IsFailure ||
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
                    session.ValidateInvariants().IsFailure)
                {
                    LogPowerStateInterlockSmokeFailure(
                        "smoke.power-off-replay-or-invariant-mismatch");
                    yield break;
                }
            }
            finally
            {
                RemoveCustomPcWorkTicketSmokeDevices(
                    smokeKeyboard,
                    smokeGamepad);
            }

            Debug.Log(PowerStateInterlockSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private static IEnumerator RunPowerStateInterlockSmokeGuarded(
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
                        LogPowerStateInterlockSmokeFailure(
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
                DisposePowerStateSmokeRoutines(routines);
            }
        }

        private static void DisposePowerStateSmokeRoutines(
            Stack<IEnumerator> routines)
        {
            while (routines.Count > 0)
            {
                (routines.Pop() as IDisposable)?.Dispose();
            }
        }

        private static void LogPowerStateInterlockSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_POWER_STATE_INTERLOCK_RUNTIME_SMOKE " +
                "power-state-flow=failed code=" + code);
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
