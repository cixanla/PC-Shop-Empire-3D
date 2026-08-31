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
        public const string ValidationSmokeSuccessMarker =
            "GARAGE_VALIDATION_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted driver=current firmware=current " +
            "review=player-triggered validation=player-triggered " +
            "input=keyboard+mouse+gamepad result=passed score=401 " +
            "stress-steps=300 stress=stable cpu-peak=67C gpu-peak=64C " +
            "power-draw=380W minimum-psu=500W installed-psu=550W " +
            "power-margin=50W quality=good receipt=immutable replay=ok " +
            "power-off=player-triggered current-after-power-off=false " +
            "history=preserved upstream=unchanged invariants=ok";

        public bool HasValidationR66Runtime =>
            HasFictionalDriverInstallationR65Runtime &&
            electricalPowerTestStation != null &&
            electricalReadinessWorkbench != null;

        private IEnumerator RunValidationSmoke()
        {
            return RunValidationSmokeGuarded(RunValidationSmokeCore());
        }

        private IEnumerator RunValidationSmokeCore()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedFictionalDriverInstallationSmokeFailureCode = null;
            _suppressFictionalDriverInstallationSmokeSuccessMarker = true;
            try
            {
                yield return RunFictionalDriverInstallationSmoke();
            }
            finally
            {
                _suppressFictionalDriverInstallationSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedFictionalDriverInstallationSmokeFailureCode;
            _nestedFictionalDriverInstallationSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogValidationSmokeFailure(
                    "smoke.driver-prerequisite-" + suffix);
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
                !HasValidationR66Runtime ||
                !pcieGpuPowerCableBinding.IsRouted ||
                session.PowerState == null ||
                session.PowerState.State != PcPowerState.Off ||
                session.PowerState.Revision != 8 ||
                session.PowerState.PostStartupRevision != 4 ||
                session.PowerState.FirmwareBaselineRevision != 3 ||
                !session.TryGetFictionalDriverInstallation(
                    out PcFictionalDriverInstallationAuthority driverAuthority) ||
                driverAuthority.Revision != 1 ||
                driverAuthority.ReceiptCount != 1 ||
                driverAuthority.EvaluateInstalledDrivers().IsFailure ||
                session.TryGetValidation(out _))
            {
                LogValidationSmokeFailure("smoke.context-mismatch");
                yield break;
            }

            PcFictionalDriverInstallationReceipt driverReceipt =
                driverAuthority.EvaluateInstalledDrivers().Value;
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
            PcFirmwareBaselineReceipt completionFirmware = null;
            PcValidationReceipt validationReceipt = null;
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
                    LogValidationSmokeFailure(
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
                    powerState.Revision != 9 ||
                    powerState.PostStartupRevision != 5 ||
                    powerState.FirmwareBaselineRevision != 3 ||
                    station.InspectFirmwareInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains("UEFI SETUP'I AÇ"))
                {
                    LogValidationSmokeFailure(
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
                OperationResult<PcFictionalDriverInstallationReceipt>
                    currentDriver = driverAuthority.EvaluateInstalledDrivers();
                if (completionFirmware == null ||
                    powerState.FirmwareBaselineRevision != 4 ||
                    powerState.EvaluateCurrentFirmwareBaseline().IsFailure ||
                    currentDriver.IsFailure ||
                    !ReferenceEquals(currentDriver.Value, driverReceipt) ||
                    session.TryGetValidation(out _) ||
                    !station.PromptText.Contains(
                        "SONRAKİ AŞAMA: VALIDATION") ||
                    !station.PromptText.Contains("VALIDATION'I İNCELE") ||
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().IsFailure)
                {
                    LogValidationSmokeFailure(
                        "smoke.current-lineage-or-validation-gate-mismatch");
                    yield break;
                }

                long powerRevisionBeforeValidation = powerState.Revision;
                int powerReceiptCountBeforeValidation = powerState.ReceiptCount;
                long postRevisionBeforeValidation =
                    powerState.PostStartupRevision;
                int postReceiptCountBeforeValidation =
                    powerState.PostStartupReceiptCount;
                long firmwareRevisionBeforeValidation =
                    powerState.FirmwareBaselineRevision;
                int firmwareReceiptCountBeforeValidation =
                    powerState.FirmwareBaselineReceiptCount;
                long osRevisionBeforeValidation =
                    session.FictionalOsInstallation.Revision;
                int osReceiptCountBeforeValidation =
                    session.FictionalOsInstallation.ReceiptCount;
                long driverRevisionBeforeValidation = driverAuthority.Revision;
                int driverReceiptCountBeforeValidation =
                    driverAuthority.ReceiptCount;

                InputSystem.QueueStateEvent(
                    smokeMouse,
                    new MouseState { buttons = 1 });
                yield return null;
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                yield return null;
                if (!station.IsReviewingValidation ||
                    session.TryGetValidation(out _) ||
                    !station.PromptText.Contains("WORKSHOP VALIDATION SUITE") ||
                    !station.PromptText.Contains("300 SABİT STRESS ADIMI") ||
                    electricalReadinessWorkbench.ValidationState !=
                        PcValidationPresentationState.Reviewing)
                {
                    LogValidationSmokeFailure(
                        "smoke.validation-review-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState { rightTrigger = 1f });
                yield return null;
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                yield return null;

                if (!session.TryGetValidation(
                        out PcValidationAuthority validationAuthority))
                {
                    LogValidationSmokeFailure(
                        "smoke.validation-authority-missing");
                    yield break;
                }

                OperationResult<PcValidationReceipt> currentValidation =
                    validationAuthority.EvaluateCurrentValidation();
                validationReceipt = currentValidation.TryGetValue(
                    out PcValidationReceipt value)
                        ? value
                        : null;
                OperationResult<PcValidationReceipt> replay =
                    validationReceipt == null
                        ? OperationResult<PcValidationReceipt>.Fail(
                            PcValidationFailures.ReceiptHistoryInvalid)
                        : validationAuthority.TryCompleteValidation(
                            validationReceipt.OperationId,
                            driverReceipt,
                            completionFirmware,
                            validationReceipt.ExpectedPowerStateRevision,
                            validationReceipt.ExpectedRevision);
                electricalReadinessWorkbench.RefreshPresentation();
                if (!playerInput.UsesGamepadPrompts ||
                    station.IsReviewingValidation ||
                    validationReceipt == null ||
                    replay.IsFailure ||
                    !ReferenceEquals(replay.Value, validationReceipt) ||
                    validationReceipt.Result !=
                        PcValidationResult.PassedForQualityStage ||
                    validationReceipt.StressResult != PcStressResult.Stable ||
                    !ReferenceEquals(
                        validationReceipt.SourceDriverReceipt,
                        driverReceipt) ||
                    !ReferenceEquals(
                        validationReceipt.SourceFirmwareBaselineReceipt,
                        completionFirmware) ||
                    validationReceipt.BenchmarkScore != 401 ||
                    validationReceipt.StressSteps != 300 ||
                    validationReceipt.ProcessorPeakTemperatureCelsius != 67 ||
                    validationReceipt.GraphicsCardPeakTemperatureCelsius != 64 ||
                    validationReceipt.SystemPowerDrawWatts != 380 ||
                    validationReceipt.MinimumRecommendedPsuWatts != 500 ||
                    validationReceipt.InstalledPsuWatts != 550 ||
                    validationReceipt.PowerMarginWatts != 50 ||
                    validationReceipt.QualityTier != PcQualityTier.Good ||
                    validationAuthority.Revision != 1 ||
                    validationAuthority.ReceiptCount != 1 ||
                    !station.PromptText.Contains("VALIDATION GEÇTİ") ||
                    !station.PromptText.Contains("SCORE 401") ||
                    electricalReadinessWorkbench.ValidationState !=
                        PcValidationPresentationState.Passed ||
                    !electricalReadinessWorkbench.HasCurrentValidation ||
                    electricalReadinessWorkbench.ValidationBenchmarkScore != 401 ||
                    electricalReadinessWorkbench.ValidationStressSteps != 300 ||
                    electricalReadinessWorkbench
                        .ValidationProcessorPeakTemperatureCelsius != 67 ||
                    electricalReadinessWorkbench
                        .ValidationGraphicsPeakTemperatureCelsius != 64 ||
                    electricalReadinessWorkbench.ValidationSystemPowerDrawWatts !=
                        380 ||
                    electricalReadinessWorkbench
                        .ValidationMinimumRecommendedPsuWatts != 500 ||
                    electricalReadinessWorkbench.ValidationInstalledPsuWatts !=
                        550 ||
                    electricalReadinessWorkbench.ValidationPowerMarginWatts != 50 ||
                    electricalReadinessWorkbench.ValidationQualityTier !=
                        PcQualityTier.Good ||
                    powerState.Revision != powerRevisionBeforeValidation ||
                    powerState.ReceiptCount != powerReceiptCountBeforeValidation ||
                    powerState.PostStartupRevision !=
                        postRevisionBeforeValidation ||
                    powerState.PostStartupReceiptCount !=
                        postReceiptCountBeforeValidation ||
                    powerState.FirmwareBaselineRevision !=
                        firmwareRevisionBeforeValidation ||
                    powerState.FirmwareBaselineReceiptCount !=
                        firmwareReceiptCountBeforeValidation ||
                    session.FictionalOsInstallation.Revision !=
                        osRevisionBeforeValidation ||
                    session.FictionalOsInstallation.ReceiptCount !=
                        osReceiptCountBeforeValidation ||
                    driverAuthority.Revision != driverRevisionBeforeValidation ||
                    driverAuthority.ReceiptCount !=
                        driverReceiptCountBeforeValidation ||
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
                        pcieReceiptCount))
                {
                    LogValidationSmokeFailure(
                        "smoke.result-replay-presentation-or-mutation-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.E));
                yield return null;
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                yield return null;

                electricalReadinessWorkbench.RefreshPresentation();
                OperationResult<PcValidationReceipt> historicalReplay =
                    validationAuthority.TryCompleteValidation(
                        validationReceipt.OperationId,
                        driverReceipt,
                        completionFirmware,
                        validationReceipt.ExpectedPowerStateRevision,
                        validationReceipt.ExpectedRevision);
                if (powerState.State != PcPowerState.Off ||
                    powerState.Revision != 10 ||
                    powerState.ActivePowerOnReceipt != null ||
                    powerState.ActivePostStartupReceipt != null ||
                    powerState.ActiveFirmwareBaselineReceipt != null ||
                    validationAuthority.EvaluateCurrentValidation().Error !=
                        PcValidationFailures.NotCurrent ||
                    historicalReplay.IsFailure ||
                    !ReferenceEquals(
                        historicalReplay.Value,
                        validationReceipt) ||
                    !validationAuthority.TryGetReceipt(
                        validationReceipt.OperationId,
                        out PcValidationReceipt historical) ||
                    !ReferenceEquals(historical, validationReceipt) ||
                    electricalReadinessWorkbench.ValidationState !=
                        PcValidationPresentationState.NotCurrent ||
                    electricalReadinessWorkbench.HasCurrentValidation ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "VALIDATION TARİHÇEDE KORUNDU") ||
                    validationAuthority.ValidateReceiptHistory().IsFailure ||
                    driverAuthority.ValidateReceiptHistory().IsFailure ||
                    session.FictionalOsInstallation.ValidateReceiptHistory()
                        .IsFailure ||
                    powerState.ValidateReceiptHistory().IsFailure ||
                    session.AssemblyBuild.EvaluateBenchmarkReadiness().IsFailure ||
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
                    LogValidationSmokeFailure(
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

            Debug.Log(ValidationSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private IEnumerator RunValidationSmokeGuarded(IEnumerator root)
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
                        LogValidationSmokeFailure("smoke.unhandled-exception");
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

        private static void LogValidationSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_VALIDATION_RUNTIME_SMOKE " +
                "validation-flow=failed code=" + code);
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
