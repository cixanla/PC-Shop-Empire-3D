using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string ProcessorAssemblyHandoffSmokeSuccessMarker =
            "GARAGE_PROCESSOR_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
            "work-ticket=ok prerequisites=10/10 motherboard=secured " +
            "pickup=exact custody=build-kit-to-hands-to-socket " +
            "reservation=alive physical-identity=stable input=keyboard+mouse " +
            "seat=ok retain=ok retained-block=ok open=ok detach=ok reseat=ok " +
            "history=10/10-preserved other-eight=untouched " +
            "receipts=ok revisions=ok no-duplicate-loss=ok invariants=ok";

        private bool _suppressProcessorAssemblyHandoffSmokeSuccessMarker;
        private string _nestedProcessorAssemblyHandoffSmokeFailureCode;

        public bool HasProcessorAssemblyHandoffR46Runtime =>
            HasMotherboardAssemblyHandoffR45Runtime &&
            processorBinding != null &&
            processorBuildKit != null &&
            processorSocket != null &&
            processorBinding.BuildKit == processorBuildKit &&
            processorBinding.Socket == processorSocket &&
            stockFlow != null &&
            stockFlow.Session != null &&
            stockFlow.Session.PrototypeProcessorAssemblyHandoffOperationId.Value !=
                stockFlow.Session.PrototypeProcessorBuildKitOperationId.Value &&
            stockFlow.Session.ProcessorSocketContainerId !=
                stockFlow.Session.ProcessorBuildKitContainerId;

        private IEnumerator RunProcessorAssemblyHandoffSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            _nestedPcieGpuPowerCableBuildKitSmokeFailureCode = null;
            _suppressPcieGpuPowerCableBuildKitSmokeSuccessMarker = true;
            try
            {
                yield return RunPcieGpuPowerCableBuildKitSmoke();
            }
            finally
            {
                _suppressPcieGpuPowerCableBuildKitSmokeSuccessMarker = false;
            }

            string prerequisiteFailure =
                _nestedPcieGpuPowerCableBuildKitSmokeFailureCode;
            _nestedPcieGpuPowerCableBuildKitSmokeFailureCode = null;
            if (!string.IsNullOrEmpty(prerequisiteFailure))
            {
                const string SmokePrefix = "smoke.";
                string suffix = prerequisiteFailure.StartsWith(
                    SmokePrefix,
                    StringComparison.Ordinal)
                        ? prerequisiteFailure.Substring(SmokePrefix.Length)
                        : prerequisiteFailure;
                LogProcessorAssemblyHandoffSmokeFailure(
                    $"smoke.build-kit-prerequisite-{suffix}");
                yield break;
            }

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            PhysicalItemProjection motherboard = motherboardBinding != null
                ? motherboardBinding.PhysicalItem
                : null;
            PhysicalItemProjection processor = processorBinding != null
                ? processorBinding.PhysicalItem
                : null;
            if (session == null ||
                playerMotor == null ||
                playerInput == null ||
                playerCarry == null ||
                motherboard == null ||
                processor == null ||
                !HasProcessorAssemblyHandoffR46Runtime ||
                !motherboardBuildKit.IsStaged ||
                !processorBuildKit.IsStaged ||
                !motherboardBinding.IsAuthorityInBuildKit ||
                !processorBinding.IsAuthorityInBuildKit ||
                session.CustomPcBuildKit.StagedComponentCount != 10 ||
                session.AssemblyBuild.MotherboardSeatState != AssemblySeatState.Empty ||
                session.AssemblyBuild.ProcessorSocketState !=
                    ProcessorSocketState.EmptyOpen)
            {
                LogProcessorAssemblyHandoffSmokeFailure(
                    "smoke.prerequisite-context-mismatch");
                yield break;
            }

            if (!session.TryGetPrototypeCustomPcBuildOrder(
                    out CustomPcBuildOrderRecord workOrder) ||
                !session.TryGetPrototypeCustomPcWorkTicket(out _))
            {
                LogProcessorAssemblyHandoffSmokeFailure("smoke.work-ticket-missing");
                yield break;
            }

            CustomPcBuildOrderLineSnapshot motherboardLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.Motherboard);
            CustomPcBuildOrderLineSnapshot processorLine = workOrder.Lines
                .SingleOrDefault(line =>
                    line.ComponentKind == PcComponentKind.Processor);
            if (motherboardLine == null ||
                processorLine == null ||
                !TryCaptureMotherboardAssemblyHandoffStagingReceipts(
                    session,
                    out CustomPcBuildKitReceipt[] historicalReceipts) ||
                !TryCaptureProcessorAssemblyHandoffOtherContainers(
                    session,
                    workOrder,
                    out Dictionary<StableId<ItemInstanceIdScope>,
                        StableId<ContainerIdScope>> otherContainers) ||
                !MotherboardAssemblyHandoffSmokeReservationIsLive(
                    session,
                    workOrder,
                    motherboardLine) ||
                !MotherboardAssemblyHandoffSmokeReservationIsLive(
                    session,
                    workOrder,
                    processorLine))
            {
                LogProcessorAssemblyHandoffSmokeFailure(
                    "smoke.reservation-or-history-mismatch");
                yield break;
            }

            int processorPhysicalIdentity = processor.GetInstanceID();
            string processorItemIdentity = processor.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Keyboard smokeKeyboard = null;
            Mouse smokeMouse = null;
            try
            {
                smokeKeyboard = InputSystem.AddDevice<Keyboard>();
                smokeMouse = InputSystem.AddDevice<Mouse>();
                InputSystem.QueueStateEvent(smokeKeyboard, new KeyboardState());
                InputSystem.QueueStateEvent(smokeMouse, new MouseState());
                InputSystem.Update();

                AimMotherboardBuildKitSmokeAtItem(motherboard, -Vector3.forward);
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool motherboardPickedUp =
                    playerCarry.HeldItem == motherboard &&
                    motherboardBinding.IsAuthorityInHands &&
                    motherboardBuildKit.IsReleasedForAssembly;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!motherboardPickedUp)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.motherboard-pickup-mismatch");
                    yield break;
                }

                MoveMotherboardBuildKitSmokePlayerToSeat();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool motherboardSeatReady =
                    playerCarry.IsMotherboardSeatMode &&
                    playerCarry.CurrentMotherboardSeatStatus ==
                        MotherboardSeatStatus.Valid;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!motherboardSeatReady)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.motherboard-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                MovePlayerToMotherboardFastener();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool motherboardSecured =
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedSecured &&
                    motherboardBinding.IsSecured;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!motherboardSecured)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.motherboard-secure-mismatch");
                    yield break;
                }

                processorBuildKit.RefreshPresentation();
                AimMotherboardBuildKitSmokeAtItem(processor, -Vector3.forward);
                playerCarry.ProcessInputFrame();
                if (playerCarry.FocusedItem != processor)
                {
                    PhysicalItemProjection focused = playerCarry.FocusedItem;
                    PhysicalInteractionResolver interaction =
                        playerMotor.GetComponentInChildren<PhysicalInteractionResolver>();
                    Vector3 interactionOriginPosition = interaction != null &&
                        interaction.Origin != null
                            ? interaction.Origin.position
                            : Vector3.zero;
                    Vector3 interactionOriginForward = interaction != null &&
                        interaction.Origin != null
                            ? interaction.Origin.forward
                            : Vector3.zero;
                    string rayHits = interaction != null && interaction.Origin != null
                        ? string.Join(
                            ",",
                            Physics.RaycastAll(
                                    interaction.Origin.position,
                                    interaction.Origin.forward,
                                    interaction.MaximumRange,
                                    interaction.QueryMask,
                                    QueryTriggerInteraction.Ignore)
                                .OrderBy(hit => hit.distance)
                                .Select(hit => hit.collider != null
                                    ? $"{hit.collider.name}@{hit.distance:F4}:" +
                                      hit.point.ToString("F4")
                                    : "none"))
                        : "resolver-missing";
                    string processorColliders = string.Join(
                        ",",
                        processor.GetComponentsInChildren<Collider>(true)
                            .Select(itemCollider =>
                                $"{itemCollider.name}:enabled={itemCollider.enabled}:" +
                                $"layer={itemCollider.gameObject.layer}:" +
                                $"center={itemCollider.bounds.center.ToString("F4")}:" +
                                $"size={itemCollider.bounds.size.ToString("F4")}"));
                    Debug.LogError(
                        "GARAGE_PROCESSOR_ASSEMBLY_HANDOFF_RUNTIME_SMOKE_DIAGNOSTIC " +
                        $"focused={(focused != null ? focused.name : "none")} " +
                        $"focused-item={(focused != null ? focused.ItemIdValue : "none")} " +
                        $"processor={processor.name} processor-item={processor.ItemIdValue} " +
                        $"held={(playerCarry.HeldItem != null ? playerCarry.HeldItem.name : "none")} " +
                        $"prompt={playerCarry.PromptText.Replace('\n', '|')} " +
                        $"origin={interactionOriginPosition.ToString("F4")} " +
                        $"forward={interactionOriginForward.ToString("F4")} " +
                        $"target={processor.InteractionCenter.ToString("F4")} " +
                        $"transform={processor.transform.position.ToString("F4")} " +
                        $"processor-colliders={processorColliders} ray-hits={rayHits}");
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-focus-mismatch");
                    yield break;
                }

                if (!playerCarry.PromptText.Contains("10/10"))
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-history-prompt-mismatch");
                    yield break;
                }

                if (!playerCarry.PromptText.Contains(
                        "CPU'YU SOKET MONTAJINA AL"))
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-action-prompt-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool processorPickedUp =
                    playerCarry.HeldItem == processor &&
                    processorBinding.IsAuthorityInHands &&
                    processorBuildKit.IsReleasedForAssembly &&
                    processorBuildKit.ProgressText.text.Contains("CPU MONTAJDA") &&
                    session.CustomPcBuildKit.TryGetAssemblyHandoff(
                        session.PrototypeProcessorAssemblyHandoffOperationId,
                        out CustomPcBuildKitAssemblyHandoffReceipt handoff) &&
                    handoff.ComponentKind == PcComponentKind.Processor &&
                    ReferenceEquals(handoff.Line, processorLine) &&
                    ReferenceEquals(handoff.StagingReceipt, historicalReceipts[1]) &&
                    processor.GetInstanceID() == processorPhysicalIdentity &&
                    processor.ItemIdValue == processorItemIdentity;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!processorPickedUp)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-pickup-mismatch");
                    yield break;
                }

                MovePlayerToProcessorSocket();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool processorSeatReady =
                    playerCarry.IsProcessorSeatMode &&
                    playerCarry.CurrentProcessorSocketStatus ==
                        ProcessorSocketStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!processorSeatReady)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-seat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                bool seated =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorSeatedOpen &&
                    processorBinding.IsSeated;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!seated)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-seat-mismatch");
                    yield break;
                }

                MovePlayerToProcessorSocket();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool retained =
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained &&
                    processorBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!retained)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-retain-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool retainedBlocked =
                    playerCarry.HeldItem == null &&
                    playerCarry.LastFailureCode ==
                        AssemblyFailures.ProcessorRetained.Code &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!retainedBlocked)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.retained-detach-not-blocked");
                    yield break;
                }

                MovePlayerToProcessorSocket();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool opened =
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorSeatedOpen &&
                    !processorBinding.IsRetained;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!opened)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-open-mismatch");
                    yield break;
                }

                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.E);
                bool detached =
                    playerCarry.HeldItem == processor &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.EmptyOpen &&
                    processorBinding.IsAuthorityInHands;
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                if (!detached)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-detach-mismatch");
                    yield break;
                }

                MovePlayerToProcessorSocket();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                bool reseatReady =
                    playerCarry.IsProcessorSeatMode &&
                    playerCarry.CurrentProcessorSocketStatus ==
                        ProcessorSocketStatus.ValidSeat;
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);
                if (!reseatReady)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.processor-reseat-preflight-mismatch");
                    yield break;
                }

                PressMotherboardBuildKitSmokeKey(smokeKeyboard, Key.G);
                ReleaseMotherboardBuildKitSmokeKeyboard(smokeKeyboard);
                MovePlayerToProcessorSocket();
                playerCarry.ProcessInputFrame();
                PressMotherboardBuildKitSmokeMouse(smokeMouse);
                ReleaseMotherboardBuildKitSmokeMouse(smokeMouse);

                bool finalState =
                    playerCarry.HeldItem == null &&
                    session.AssemblyBuild.MotherboardSeatState ==
                        AssemblySeatState.SeatedSecured &&
                    session.AssemblyBuild.ProcessorSocketState ==
                        ProcessorSocketState.ProcessorRetained &&
                    session.Inventory.Revision == inventoryRevision + 6 &&
                    session.CustomPcBuildKit.Revision == buildKitRevision + 2 &&
                    session.AssemblyBuild.Revision == assemblyRevision + 8 &&
                    session.AssemblyBuild.ReceiptCount == assemblyReceiptCount + 8 &&
                    session.CustomPcBuildKit.StagedComponentCount == 10 &&
                    session.CustomPcBuildKit.AssemblyHandoffCount == 2 &&
                    processor.GetInstanceID() == processorPhysicalIdentity &&
                    processor.ItemIdValue == processorItemIdentity &&
                    MotherboardAssemblyHandoffSmokeReservationIsLive(
                        session,
                        workOrder,
                        motherboardLine) &&
                    MotherboardAssemblyHandoffSmokeReservationIsLive(
                        session,
                        workOrder,
                        processorLine) &&
                    MotherboardAssemblyHandoffSmokeHistoryIsPreserved(
                        session,
                        historicalReceipts,
                        otherContainers) &&
                    motherboardBinding.ValidateProjectionInvariant().IsSuccess &&
                    processorBinding.ValidateProjectionInvariant().IsSuccess &&
                    session.ValidateInvariants().IsSuccess;
                if (!finalState)
                {
                    LogProcessorAssemblyHandoffSmokeFailure(
                        "smoke.final-state-or-invariant-mismatch");
                    yield break;
                }

                if (!_suppressProcessorAssemblyHandoffSmokeSuccessMarker)
                {
                    Debug.Log(ProcessorAssemblyHandoffSmokeSuccessMarker);
                }

                yield return new WaitForEndOfFrame();
                if (!Application.isEditor &&
                    !_suppressProcessorAssemblyHandoffSmokeSuccessMarker)
                {
                    Application.Quit(0);
                }
            }
            finally
            {
                RemoveMotherboardBuildKitSmokeDevices(smokeKeyboard, smokeMouse);
            }
        }

        private static bool TryCaptureProcessorAssemblyHandoffOtherContainers(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            out Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>> containers)
        {
            containers = new Dictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>>();
            foreach (CustomPcBuildOrderLineSnapshot line in workOrder.Lines)
            {
                if (line.ComponentKind == PcComponentKind.Motherboard ||
                    line.ComponentKind == PcComponentKind.Processor)
                {
                    continue;
                }

                if (!session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item))
                {
                    containers = null;
                    return false;
                }

                containers.Add(line.ItemId, item.ContainerId);
            }

            return containers.Count == 8;
        }

        private void LogProcessorAssemblyHandoffSmokeFailure(string code)
        {
            if (_suppressProcessorAssemblyHandoffSmokeSuccessMarker)
            {
                _nestedProcessorAssemblyHandoffSmokeFailureCode = code;
                return;
            }

            Debug.LogError(
                "GARAGE_PROCESSOR_ASSEMBLY_HANDOFF_RUNTIME_SMOKE " +
                $"assembly-handoff-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
