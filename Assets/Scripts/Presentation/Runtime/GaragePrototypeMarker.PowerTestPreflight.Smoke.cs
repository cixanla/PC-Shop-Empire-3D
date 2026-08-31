using System;
using System.Collections;
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
        public const string PowerTestPreflightSmokeSuccessMarker =
            "GARAGE_POWER_TEST_PREFLIGHT_RUNTIME_SMOKE " +
            "prerequisite-setup=assisted electrical-readiness=ready " +
            "power-budget=380/500/550 station=existing-focus-surface " +
            "input=keyboard+gamepad single-consumer=ok " +
            "range=ok focus=ok los=ok pause=ok co-edge=ok " +
            "receipt=immutable replay=ok stale=detected " +
            "attempt-mutation=zero benchmark=untouched " +
            "presentation=ok power-on=not-started invariants=ok";

        private IEnumerator RunPowerTestPreflightSmoke()
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
                LogPowerTestPreflightSmokeFailure(
                    $"smoke.pcie-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            ElectricalPowerTestStationProjection station =
                electricalPowerTestStation;
            PhysicalItemProjection physicalCable =
                pcieGpuPowerCableBinding != null
                    ? pcieGpuPowerCableBinding.PhysicalItem
                    : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                station == null ||
                physicalCable == null ||
                electricalReadinessWorkbench == null ||
                !HasPowerTestPreflightR60Runtime ||
                station.StockFlow != stockFlow ||
                station.PlayerInput != playerInput ||
                station.PlayerMotor != playerMotor ||
                station.PlayerCarry != playerCarry ||
                station.ReadinessProjection != electricalReadinessWorkbench ||
                station.FocusAnchor != electricalReadinessWorkbench.StatusText.transform ||
                playerCarry.HeldItem != physicalCable ||
                !pcieGpuPowerCableBinding.IsAuthorityInHands ||
                session.PowerTestAttempts == null ||
                session.PowerTestAttempts.ReceiptCount != 0)
            {
                LogPowerTestPreflightSmokeFailure("smoke.context-mismatch");
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
            OperationResult<PcPowerBudgetSnapshot> budget =
                session.PowerBudget.AssessPowerBudget();
            if (route.IsFailure ||
                readiness.IsFailure ||
                budget.IsFailure ||
                !budget.Value.IsSufficient ||
                budget.Value.SystemPowerDrawWatts != 380 ||
                budget.Value.MinimumRecommendedPsuWatts != 500 ||
                budget.Value.InstalledPsuWatts != 550 ||
                playerCarry.HeldItem != null ||
                !pcieGpuPowerCableBinding.IsRouted ||
                !electricalReadinessWorkbench.IsReady ||
                electricalReadinessWorkbench.HasAcceptedPreflight ||
                !electricalReadinessWorkbench.StatusText.text.Contains(
                    "GÜÇ TESTİ BEKLİYOR") ||
                session.AssemblyBuild.EvaluateBenchmarkReadiness().IsFailure)
            {
                LogPowerTestPreflightSmokeFailure(
                    route.IsFailure
                        ? $"smoke.ready-route-{route.Error.Code}"
                        : "smoke.ready-context-mismatch");
                yield break;
            }

            PowerTestAttemptAuthority attempts = session.PowerTestAttempts;
            long inventoryRevision = session.Inventory.Revision;
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

            MovePlayerToPowerTestPreflightStation(
                station.InteractionRange + 0.65f);
            OperationResult outOfRange =
                station.InspectInteractionGateForTests();
            if (outOfRange.Error !=
                    ElectricalPowerTestStationFailures.OutOfRange ||
                !PowerTestSmokeAttemptStateIsZero(attempts) ||
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
                LogPowerTestPreflightSmokeFailure("smoke.range-gate-mismatch");
                yield break;
            }

            MovePlayerToPowerTestPreflightStation(1.35f);
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot == null)
            {
                LogPowerTestPreflightSmokeFailure("smoke.camera-pivot-missing");
                yield break;
            }

            Quaternion centeredLook = cameraPivot.rotation;
            cameraPivot.rotation =
                Quaternion.AngleAxis(30f, Vector3.up) * centeredLook;
            Physics.SyncTransforms();
            OperationResult offTarget =
                station.InspectInteractionGateForTests();
            if (offTarget.Error !=
                    ElectricalPowerTestStationFailures.FocusMissing ||
                !PowerTestSmokeAttemptStateIsZero(attempts))
            {
                LogPowerTestPreflightSmokeFailure("smoke.focus-gate-mismatch");
                yield break;
            }

            MovePlayerToPowerTestPreflightStation(1.35f);
            Vector3 cameraPosition = station.PlayerCamera.transform.position;
            Vector3 focusTarget = station.FocusAnchor.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "PowerTestPreflightNativeSmokeLosBlocker";
            blocker.transform.position =
                Vector3.Lerp(cameraPosition, focusTarget, 0.5f);
            blocker.transform.localScale = new Vector3(0.45f, 0.55f, 0.12f);
            Physics.SyncTransforms();
            OperationResult blocked =
                station.InspectInteractionGateForTests();
            bool losSafe = blocked.Error ==
                           ElectricalPowerTestStationFailures.LineOfSightBlocked &&
                           PowerTestSmokeAttemptStateIsZero(attempts);
            Destroy(blocker);
            yield return null;
            Physics.SyncTransforms();
            if (!losSafe)
            {
                LogPowerTestPreflightSmokeFailure("smoke.los-gate-mismatch");
                yield break;
            }

            playerMotor.SetPaused(true);
            OperationResult paused =
                station.InspectInteractionGateForTests();
            playerMotor.SetPaused(false);
            if (paused.Error != ElectricalPowerTestStationFailures.Paused ||
                !PowerTestSmokeAttemptStateIsZero(attempts))
            {
                LogPowerTestPreflightSmokeFailure("smoke.pause-gate-mismatch");
                yield break;
            }

            Keyboard smokeKeyboard = null;
            Gamepad smokeGamepad = null;
            bool keyboardPauseCoedgeSafe = false;
            bool gamepadPauseCoedgeSafe = false;
            bool gamepadUnpausedWithoutAttempt = false;
            bool freshGamepadPressAccepted = false;
            bool freshGamepadUsesPrompts = false;
            bool freshGamepadInteractAvailable = false;
            bool freshGamepadPauseEdge = false;
            bool freshGamepadStationFocused = false;
            bool freshGamepadHandsBusy = false;
            string freshGamepadGateCode = string.Empty;
            string freshGamepadLineOfSight = string.Empty;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeGamepad = InputSystem.AddDevice<Gamepad>();
                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState());
                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState());
                InputSystem.Update();

                MovePlayerToPowerTestPreflightStation(1.35f);
                if (station.InspectInteractionGateForTests().IsFailure ||
                    !station.PromptText.Contains("GÜÇ TESTİ ÖN KONTROLÜ"))
                {
                    LogPowerTestPreflightSmokeFailure(
                        "smoke.focused-prompt-mismatch");
                    yield break;
                }

                playerMotor.SetPaused(true);
                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState(Key.Escape, Key.E));
                yield return null;
                keyboardPauseCoedgeSafe =
                    !playerMotor.IsPaused &&
                    !playerInput.InteractPressedThisFrame &&
                    PowerTestSmokeAttemptStateIsZero(attempts);
                InputSystem.QueueStateEvent(
                    smokeKeyboard,
                    new KeyboardState());
                yield return null;

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState
                    {
                        buttons = (1u << (int)GamepadButton.Start) |
                                  (1u << (int)GamepadButton.South)
                    });
                yield return null;
                gamepadPauseCoedgeSafe =
                    playerMotor.IsPaused &&
                    !playerInput.InteractPressedThisFrame &&
                    PowerTestSmokeAttemptStateIsZero(attempts);
                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState());
                yield return null;

                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.Start
                    });
                yield return null;
                gamepadUnpausedWithoutAttempt =
                    !playerMotor.IsPaused &&
                    PowerTestSmokeAttemptStateIsZero(attempts);
                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState());
                yield return null;

                MovePlayerToPowerTestPreflightStation(1.35f);
                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.South
                    });
                yield return null;
                OperationResult freshGamepadGate =
                    station.InspectInteractionGateForTests();
                freshGamepadUsesPrompts = playerInput.UsesGamepadPrompts;
                freshGamepadInteractAvailable =
                    playerInput.InteractPressedThisFrame;
                freshGamepadPauseEdge = playerInput.PausePressedThisFrame;
                freshGamepadStationFocused = station.IsFocused;
                freshGamepadHandsBusy = playerCarry.IsCarrying ||
                                        playerCarry.IsDrivingCart ||
                                        playerCarry.HasAssemblyPromptOwnership;
                freshGamepadGateCode = freshGamepadGate.IsFailure
                    ? freshGamepadGate.Error.Code
                    : "success";
                freshGamepadLineOfSight =
                    DescribePowerTestPreflightLineOfSight(station);
                freshGamepadPressAccepted =
                    playerInput.UsesGamepadPrompts &&
                    !playerInput.InteractPressedThisFrame &&
                    attempts.Revision == 1 &&
                    attempts.ReceiptCount == 1 &&
                    attempts.HasCompletedPreflight;
                InputSystem.QueueStateEvent(
                    smokeGamepad,
                    new GamepadState());
                yield return null;
            }
            finally
            {
                RemoveCustomPcWorkTicketSmokeDevices(
                    smokeKeyboard,
                    smokeGamepad);
            }

            if (!keyboardPauseCoedgeSafe ||
                !gamepadPauseCoedgeSafe ||
                !gamepadUnpausedWithoutAttempt ||
                !freshGamepadPressAccepted)
            {
                LogPowerTestPreflightSmokeFailure(
                    "smoke.input-lifecycle-mismatch " +
                    $"keyboard-coedge={keyboardPauseCoedgeSafe} " +
                    $"gamepad-coedge={gamepadPauseCoedgeSafe} " +
                    $"gamepad-unpause={gamepadUnpausedWithoutAttempt} " +
                    $"fresh-gamepad={freshGamepadPressAccepted} " +
                    $"paused={playerMotor.IsPaused} " +
                    $"attempt-revision={attempts.Revision} " +
                    $"attempt-receipts={attempts.ReceiptCount} " +
                    $"uses-gamepad={freshGamepadUsesPrompts} " +
                    $"interact-available={freshGamepadInteractAvailable} " +
                    $"pause-edge={freshGamepadPauseEdge} " +
                    $"station-focused={freshGamepadStationFocused} " +
                    $"hands-busy={freshGamepadHandsBusy} " +
                    $"gate={freshGamepadGateCode} " +
                    $"los={freshGamepadLineOfSight} " +
                    $"station-last={station.LastFailureCode}");
                yield break;
            }

            OperationResult presentation =
                electricalReadinessWorkbench.RefreshPresentation();
            if (presentation.IsFailure ||
                !attempts.TryGetCompletedReceipt(
                    out PowerTestAttemptReceipt receipt) ||
                receipt.OperationId != session.PrototypePowerTestAttemptOperationId ||
                receipt.ExpectedRevision != 0 ||
                receipt.Revision != 1 ||
                receipt.Context == null ||
                receipt.Context.SystemPowerDrawWatts != 380 ||
                receipt.Context.MinimumRecommendedPsuWatts != 500 ||
                receipt.Context.InstalledPsuWatts != 550 ||
                !electricalReadinessWorkbench.HasAcceptedPreflight ||
                !electricalReadinessWorkbench.HasCurrentAcceptedPreflight ||
                !electricalReadinessWorkbench.StatusText.text.Contains(
                    "ÖN KONTROL GEÇTİ") ||
                !electricalReadinessWorkbench.StatusText.text.Contains(
                    "POWER-ON BEKLİYOR") ||
                !station.PromptText.Contains("ÖN KONTROL GEÇTİ") ||
                !station.PromptText.Contains("GÜCÜ AÇ") ||
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
                LogPowerTestPreflightSmokeFailure(
                    "smoke.receipt-presentation-or-isolation-mismatch");
                yield break;
            }

            OperationResult<PowerTestAttemptReceipt> replay =
                attempts.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    receipt.Context,
                    receipt.ExpectedRevision);
            OperationResult<PowerTestAttemptReceipt> duplicate =
                attempts.TryAttemptPreflight(
                    StableId<PowerTestAttemptOperationIdScope>.Parse(
                        "assembly.power-test-attempt.runtime-smoke-duplicate"),
                    receipt.Context,
                    attempts.Revision);
            OperationResult completedGate =
                station.InspectInteractionGateForTests();
            if (replay.IsFailure ||
                !ReferenceEquals(replay.Value, receipt) ||
                duplicate.Error != PowerTestAttemptFailures.AlreadyCompleted ||
                completedGate.IsFailure ||
                attempts.Revision != 1 ||
                attempts.ReceiptCount != 1 ||
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
                    pcieReceiptCount))
            {
                LogPowerTestPreflightSmokeFailure(
                    "smoke.replay-or-duplicate-mismatch");
                yield break;
            }

            OperationResult pickup = playerCarry.TryPickup(physicalCable);
            MovePlayerToPcieGpuPowerCableRoute();
            OperationResult rerouteMode = pickup.IsSuccess
                ? playerCarry.TrySetPcieGpuPowerCableRouteMode(true)
                : pickup;
            OperationResult reroute = rerouteMode.IsSuccess
                ? playerCarry.TryConfirmPcieGpuPowerCableRoute()
                : rerouteMode;
            yield return null;
            OperationResult stalePresentation =
                electricalReadinessWorkbench.RefreshPresentation();
            OperationResult<PowerTestAttemptReceipt> stale =
                attempts.EvaluateCurrentReceipt();
            OperationResult<PowerTestAttemptReceipt> historicalReplay =
                attempts.TryAttemptPreflight(
                    session.PrototypePowerTestAttemptOperationId,
                    receipt.Context,
                    receipt.ExpectedRevision);
            bool receiptRetained = attempts.TryGetReceipt(
                session.PrototypePowerTestAttemptOperationId,
                out PowerTestAttemptReceipt retained) &&
                ReferenceEquals(retained, receipt);
            if (pickup.IsFailure ||
                reroute.IsFailure ||
                playerCarry.HeldItem != null ||
                !pcieGpuPowerCableBinding.IsRouted ||
                stalePresentation.Error != PowerTestAttemptFailures.ContextStale ||
                stale.Error != PowerTestAttemptFailures.ContextStale ||
                historicalReplay.IsFailure ||
                !ReferenceEquals(historicalReplay.Value, receipt) ||
                !receiptRetained ||
                attempts.Revision != 1 ||
                attempts.ReceiptCount != 1 ||
                attempts.ValidateReceiptHistory().IsFailure ||
                !electricalReadinessWorkbench.HasAcceptedPreflight ||
                electricalReadinessWorkbench.HasCurrentAcceptedPreflight ||
                !electricalReadinessWorkbench.StatusText.text.Contains(
                    "ÖN KONTROL GEÇERSİZ") ||
                !electricalReadinessWorkbench.StatusText.text.Contains(
                    "YAPILANDIRMA DEĞİŞTİ") ||
                !electricalReadinessWorkbench.StatusText.text.Contains(
                    "POWER-ON BEKLİYOR") ||
                session.AssemblyBuild.EvaluateBenchmarkReadiness().Error !=
                    AssemblyFailures.PowerCableMissing ||
                pcieGpuPowerCableBinding.ValidateProjectionInvariant().IsFailure ||
                session.ValidateInvariants().IsFailure)
            {
                LogPowerTestPreflightSmokeFailure(
                    pickup.IsFailure
                        ? $"smoke.stale-pickup-{pickup.Error.Code}"
                        : reroute.IsFailure
                            ? $"smoke.stale-reroute-{reroute.Error.Code}"
                            : "smoke.stale-receipt-or-invariant-mismatch");
                yield break;
            }

            Debug.Log(PowerTestPreflightSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
            if (!Application.isEditor)
            {
                Application.Quit(0);
            }
        }

        private void MovePlayerToPowerTestPreflightStation(float distance)
        {
            Vector3 target = electricalPowerTestStation.FocusAnchor.position;
            Vector3 playerPosition = target + (Vector3.back * distance);
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private static bool PowerTestSmokeAttemptStateIsZero(
            PowerTestAttemptAuthority attempts)
        {
            return attempts != null &&
                   attempts.Revision == 0 &&
                   attempts.ReceiptCount == 0 &&
                   !attempts.HasCompletedPreflight;
        }

        private static string DescribePowerTestPreflightLineOfSight(
            ElectricalPowerTestStationProjection station)
        {
            if (station?.PlayerCamera == null || station.FocusAnchor == null)
            {
                return "context-missing";
            }

            Vector3 origin = station.PlayerCamera.transform.position;
            Vector3 offset = station.FocusAnchor.position - origin;
            float distance = offset.magnitude;
            if (distance <= Mathf.Epsilon)
            {
                return "distance-zero";
            }

            float rayDistance = Mathf.Max(0f, distance - 0.025f);
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                offset / distance,
                rayDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);
            Collider nearestCollider = null;
            float nearestDistance = float.PositiveInfinity;
            Transform playerRoot = station.PlayerMotor != null
                ? station.PlayerMotor.transform
                : null;
            for (int index = 0; index < hits.Length; index++)
            {
                Collider collider = hits[index].collider;
                if (collider == null)
                {
                    continue;
                }

                Transform candidate = collider.transform;
                if (playerRoot != null &&
                    (candidate == playerRoot || candidate.IsChildOf(playerRoot)))
                {
                    continue;
                }

                if (hits[index].distance < nearestDistance)
                {
                    nearestCollider = collider;
                    nearestDistance = hits[index].distance;
                }
            }

            return nearestCollider == null
                ? "clear"
                : $"{nearestCollider.name}@{nearestDistance:0.000}" +
                  $"/layer={nearestCollider.gameObject.layer}";
        }

        private static bool PowerTestSmokeGameplayStateUnchanged(
            GarageStockFlowSession session,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount,
            long atx24Revision,
            long eps12vRevision,
            long pcieRevision,
            int atx24ReceiptCount,
            int eps12vReceiptCount,
            int pcieReceiptCount)
        {
            return session != null &&
                   session.Inventory.Revision == inventoryRevision &&
                   session.CustomPcBuildKit.Revision == buildKitRevision &&
                   session.AssemblyBuild.Revision == assemblyRevision &&
                   session.AssemblyBuild.ReceiptCount == assemblyReceiptCount &&
                   session.AssemblyBuild.Atx24PowerCableRevision == atx24Revision &&
                   session.AssemblyBuild.Eps12vPowerCableRevision == eps12vRevision &&
                   session.AssemblyBuild.PcieGpuPowerCableRevision == pcieRevision &&
                   session.AssemblyBuild.Atx24PowerCableReceiptCount ==
                       atx24ReceiptCount &&
                   session.AssemblyBuild.Eps12vPowerCableReceiptCount ==
                       eps12vReceiptCount &&
                   session.AssemblyBuild.PcieGpuPowerCableReceiptCount ==
                       pcieReceiptCount &&
                   session.AssemblyBuild.EvaluateBenchmarkReadiness().IsSuccess;
        }

        private static void LogPowerTestPreflightSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_POWER_TEST_PREFLIGHT_RUNTIME_SMOKE " +
                $"preflight-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
