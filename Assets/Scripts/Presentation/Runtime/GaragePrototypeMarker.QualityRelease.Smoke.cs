using System;
using System.Collections;
using System.Collections.Generic;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Quality;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string QualityReleaseSmokeSuccessMarker =
            "GARAGE_QUALITY_RELEASE_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted validation=passed stress=stable " +
            "power-off=player-triggered safe-shutdown=exact " +
            "quality-review=player-triggered release=player-triggered " +
            "input=keyboard+mouse+gamepad job=exact line-lineage=10 " +
            "result=ready-for-packaging score=401 quality=good " +
            "receipt=immutable replay=ok history=preserved " +
            "upstream=unchanged invariants=ok";

        public bool HasQualityReleaseR67Runtime =>
            HasValidationR66Runtime &&
            stockFlow != null &&
            electricalPowerTestStation != null &&
            electricalReadinessWorkbench != null;

        private IEnumerator RunQualityReleaseSmoke()
        {
            return RunQualityReleaseSmokeGuarded(
                RunQualityReleaseSmokeCore());
        }

        private IEnumerator RunQualityReleaseSmokeCore()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedValidationSmokeFailureCode = null;
            _suppressValidationSmokeSuccessMarker = true;
            try
            {
                yield return RunValidationSmoke();
            }
            finally
            {
                _suppressValidationSmokeSuccessMarker = false;
            }

            string prerequisiteFailure = _nestedValidationSmokeFailureCode;
            _nestedValidationSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogQualityReleaseSmokeFailure(
                    "smoke.validation-prerequisite-" + suffix);
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
                !HasQualityReleaseR67Runtime ||
                session.PowerState == null ||
                session.PowerState.State != PcPowerState.Off ||
                session.PowerState.IsEnergized ||
                session.TryGetQualityRelease(out _) ||
                !session.TryGetQualityReleaseCandidate(
                    out CustomPcBuildOrderRecord workOrder,
                    out CustomPcWorkTicketRecord workTicket,
                    out PcValidationReceipt validationReceipt,
                    out PcPowerStateReceipt powerOffReceipt) ||
                workOrder == null || workTicket == null ||
                validationReceipt == null || powerOffReceipt == null ||
                workOrder.Lines == null || workOrder.Lines.Count != 10 ||
                validationReceipt.Result !=
                    PcValidationResult.PassedForQualityStage ||
                validationReceipt.StressResult != PcStressResult.Stable ||
                powerOffReceipt.TransitionKind !=
                    PcPowerTransitionKind.PowerOff ||
                powerOffReceipt.ResultingState != PcPowerState.Off ||
                !ReferenceEquals(
                    powerOffReceipt.SourcePowerOnReceipt,
                    validationReceipt.SourcePowerOnReceipt) ||
                session.PowerState.Revision != powerOffReceipt.Revision ||
                electricalReadinessWorkbench.QualityReleaseState !=
                    CustomPcQualityReleasePresentationState.ReadyForReview)
            {
                LogQualityReleaseSmokeFailure("smoke.context-mismatch");
                yield break;
            }

            long inventoryRevision = session.Inventory.Revision;
            long workOrderRevision = session.CustomPcWorkOrders.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long atx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int atx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            int eps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            long powerRevision = session.PowerState.Revision;
            int powerReceiptCount = session.PowerState.ReceiptCount;
            long validationRevision = session.Validation.Revision;
            int validationReceiptCount = session.Validation.ReceiptCount;

            Mouse smokeMouse = null;
            Gamepad smokeGamepad = null;
            try
            {
                smokeMouse = InputSystem.AddDevice<Mouse>();
                smokeGamepad = InputSystem.AddDevice<Gamepad>();
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.QueueStateEvent(smokeGamepad, new GamepadState());
                InputSystem.Update();

                MovePlayerToPowerTestPreflightStation(1.35f);
                if (station.InspectQualityReleaseInteractionGateForTests()
                        .IsFailure ||
                    !station.PromptText.Contains("VALIDATION GEÇTİ") ||
                    !station.PromptText.Contains("GÜVENLİ KAPATILDI") ||
                    !station.PromptText.Contains("KALİTE DOSYASINI İNCELE"))
                {
                    LogQualityReleaseSmokeFailure(
                        "smoke.review-prompt-or-gate-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeMouse,
                    new MouseState { buttons = 1 });
                yield return null;
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                yield return null;

                if (!station.IsReviewingQualityRelease ||
                    session.TryGetQualityRelease(out _) ||
                    !station.PromptText.Contains("KALİTE DOSYASI") ||
                    !station.PromptText.Contains(
                        "EXACT İŞ EMRİ + SAFE SHUTDOWN") ||
                    !station.PromptText.Contains("PAKETLEME SERBEST BIRAK") ||
                    electricalReadinessWorkbench.QualityReleaseState !=
                        CustomPcQualityReleasePresentationState.Reviewing ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "KALİTE DOSYASI İNCELENİYOR"))
                {
                    LogQualityReleaseSmokeFailure(
                        "smoke.review-state-mismatch");
                    yield break;
                }

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState { rightTrigger = 1f });
                yield return null;
                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState());
                yield return null;

                if (!session.TryGetQualityRelease(
                        out CustomPcQualityReleaseAuthority authority))
                {
                    LogQualityReleaseSmokeFailure(
                        "smoke.quality-authority-missing");
                    yield break;
                }

                OperationResult<CustomPcQualityReleaseReceipt> current =
                    authority.EvaluateCurrentRelease();
                if (current.IsFailure)
                {
                    LogQualityReleaseSmokeFailure(
                        "smoke.current-release-missing");
                    yield break;
                }

                CustomPcQualityReleaseReceipt receipt = current.Value;
                OperationResult<CustomPcQualityReleaseReceipt> replay =
                    authority.TryReleaseForPackaging(
                        receipt.OperationId,
                        workOrder,
                        workTicket,
                        validationReceipt,
                        powerOffReceipt,
                        receipt.ExpectedRevision);
                bool receiptOwned = authority.TryGetReceipt(
                    receipt.OperationId,
                    out CustomPcQualityReleaseReceipt historical);
                if (!playerInput.UsesGamepadPrompts ||
                    station.IsReviewingQualityRelease ||
                    authority.Revision != 1L || authority.ReceiptCount != 1 ||
                    receipt.Revision != 1L || receipt.ExpectedRevision != 0L ||
                    receipt.Result !=
                        CustomPcQualityReleaseResult.ReadyForPackaging ||
                    !ReferenceEquals(receipt.WorkOrder, workOrder) ||
                    !ReferenceEquals(receipt.WorkTicket, workTicket) ||
                    receipt.WorkOrderId != workOrder.Id ||
                    receipt.WorkTicketId != workTicket.Id ||
                    !ReferenceEquals(
                        receipt.SourceValidationReceipt,
                        validationReceipt) ||
                    !ReferenceEquals(
                        receipt.SourcePowerOffReceipt,
                        powerOffReceipt) ||
                    !ReferenceEquals(
                        receipt.SourcePowerOnReceipt,
                        validationReceipt.SourcePowerOnReceipt) ||
                    !ReferenceEquals(
                        receipt.SourceElectricalReadiness,
                        validationReceipt.SourceElectricalReadiness) ||
                    receipt.BenchmarkScore != 401 ||
                    receipt.StressSteps != 300 ||
                    receipt.QualityTier != PcQualityTier.Good ||
                    replay.IsFailure ||
                    !ReferenceEquals(replay.Value, receipt) ||
                    !receiptOwned || !ReferenceEquals(historical, receipt) ||
                    electricalReadinessWorkbench.QualityReleaseState !=
                        CustomPcQualityReleasePresentationState
                            .ReadyForPackaging ||
                    !electricalReadinessWorkbench.IsReadyForPackaging ||
                    electricalReadinessWorkbench.QualityWorkOrderId !=
                        workOrder.Id ||
                    electricalReadinessWorkbench.QualityWorkTicketId !=
                        workTicket.Id ||
                    electricalReadinessWorkbench
                        .QualityReleaseBenchmarkScore != 401 ||
                    electricalReadinessWorkbench.QualityReleaseTier !=
                        PcQualityTier.Good ||
                    !electricalReadinessWorkbench.StatusText.text.Contains(
                        "PAKETLEMEYE HAZIR") ||
                    !station.PromptText.Contains("PAKETLEMEYE HAZIR") ||
                    !station.PromptText.Contains("SCORE 401") ||
                    session.Inventory.Revision != inventoryRevision ||
                    session.CustomPcWorkOrders.Revision != workOrderRevision ||
                    session.PowerState.Revision != powerRevision ||
                    session.PowerState.ReceiptCount != powerReceiptCount ||
                    session.Validation.Revision != validationRevision ||
                    session.Validation.ReceiptCount != validationReceiptCount ||
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
                    authority.ValidateReceiptHistory().IsFailure ||
                    session.ValidateInvariants().IsFailure)
                {
                    LogQualityReleaseSmokeFailure(
                        "smoke.receipt-replay-presentation-or-mutation-mismatch");
                    yield break;
                }
            }
            finally
            {
                if (smokeMouse != null && smokeMouse.added)
                {
                    InputSystem.RemoveDevice(smokeMouse);
                }

                if (smokeGamepad != null && smokeGamepad.added)
                {
                    InputSystem.RemoveDevice(smokeGamepad);
                }
            }

            Debug.Log(QualityReleaseSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private IEnumerator RunQualityReleaseSmokeGuarded(IEnumerator root)
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
                        LogQualityReleaseSmokeFailure(
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

        private static void LogQualityReleaseSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_QUALITY_RELEASE_RUNTIME_SMOKE " +
                "quality-release-flow=failed code=" + code);
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
