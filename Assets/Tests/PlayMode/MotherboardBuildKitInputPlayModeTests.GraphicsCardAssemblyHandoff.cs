using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class MotherboardBuildKitInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator KeyboardMouseCompletesGraphicsCardBuildKitPcieRetentionCycle()
        {
            yield return RunIssue99GraphicsCardAssemblyHandoffCycle(
                useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesGraphicsCardBuildKitPcieRetentionCycle()
        {
            yield return RunIssue99GraphicsCardAssemblyHandoffCycle(
                useGamepad: true);
        }

        [UnityTest]
        public IEnumerator GraphicsCardBuildKitRecoverySeatsSameInstanceExactlyOnce()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue99RetainedCooler(marker);

            PhysicalItemProjection graphicsCard = marker.GraphicsCard;
            int physicalIdentity = graphicsCard.GetInstanceID();
            string itemIdentity = graphicsCard.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.GraphicsCardBuildKit.IsReleasedForAssembly, Is.True);

            long assemblyRevisionBeforeRecovery =
                session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeRecovery = session.Inventory.Revision;
            int receiptCountBeforeRecovery =
                session.AssemblyBuild.ReceiptCount;
            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            AssertSuccess(recovery);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            Assert.That(session.AssemblyBuild.GraphicsCardMountOrientation,
                Is.EqualTo(GraphicsCardMountOrientation.Primary));
            AssertIssue99GraphicsCardAtSlot(marker, graphicsCard, "recovery-seat");
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(6));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionBeforeRecovery + 1));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforeRecovery + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountBeforeRecovery + 1));
            Assert.That(Object.FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsSortMode.None).Count(
                    item => item.ItemIdValue == itemIdentity),
                Is.EqualTo(1));

            long assemblyRevisionAfterRecovery = session.AssemblyBuild.Revision;
            long inventoryRevisionAfterRecovery = session.Inventory.Revision;
            int receiptCountAfterRecovery = session.AssemblyBuild.ReceiptCount;
            OperationResult duplicateRecovery =
                marker.PlayerCarry.TryRecoverHeldItem();
            Assert.That(duplicateRecovery.Error,
                Is.EqualTo(Failure.FromCode("carry.nothing-held")));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionAfterRecovery));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionAfterRecovery));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountAfterRecovery));
            Assert.That(Object.FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsSortMode.None).Count(
                    item => item.ItemIdValue == itemIdentity),
                Is.EqualTo(1));

            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GraphicsCardBuildKitRecoveryFailsClosedWhenPcieSeatIsObstructed()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue99RetainedCooler(marker);

            PhysicalItemProjection graphicsCard = marker.GraphicsCard;
            int physicalIdentity = graphicsCard.GetInstanceID();
            string itemIdentity = graphicsCard.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);

            Pose seatPose = marker.GraphicsCardSlot.ResolveSeatPose(0).Value;
            GameObject obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Issue99RecoverySeatObstruction";
            obstruction.layer = 0;
            obstruction.transform.SetPositionAndRotation(
                seatPose.position,
                seatPose.rotation);
            obstruction.transform.localScale = Vector3.one * 0.25f;
            Physics.SyncTransforms();

            long assemblyRevision = session.AssemblyBuild.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult blockedRecovery =
                marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(blockedRecovery.Error,
                Is.EqualTo(Failure.FromCode(
                    "assembly-graphics-card.obstructed")));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(graphicsCard.IsCarried, Is.True);
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));

            Object.Destroy(obstruction);
            yield return null;
            Physics.SyncTransforms();

            AssertSuccess(marker.PlayerCarry.TryRecoverHeldItem());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            AssertIssue99GraphicsCardAtSlot(marker, graphicsCard, "recovery-after-clear");
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseGraphicsCardAssemblyInputGatesFailClosed()
        {
            yield return RunIssue99GraphicsCardAssemblyInputGates(
                useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadGraphicsCardAssemblyInputGatesFailClosed()
        {
            yield return RunIssue99GraphicsCardAssemblyInputGates(
                useGamepad: true);
        }

        private IEnumerator RunIssue99GraphicsCardAssemblyInputGates(
            bool useGamepad)
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = useGamepad ? null : InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = useGamepad ? InputSystem.AddDevice<Gamepad>() : null;
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue99RetainedCooler(marker);

            PhysicalItemProjection graphicsCard = marker.GraphicsCard;
            int physicalIdentity = graphicsCard.GetInstanceID();
            string itemIdentity = graphicsCard.ItemIdValue;
            AimPlayerAtItem(marker, graphicsCard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Collider slotFocus = marker.GraphicsCardSlot.FocusCollider;
            Vector3 slotTarget = slotFocus.bounds.center;

            Vector3 slotOutward = ResolveIssue99GraphicsCardSlotOutward(marker);
            Vector3 outOfRangePosition = slotTarget + (slotOutward * 8f);
            outOfRangePosition.y = 0.05f;
            SetPlayerLook(marker, outOfRangePosition, slotTarget);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.OutOfRange));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            AssertIssue99GraphicsCardInputGateNoMutation(
                marker,
                session,
                graphicsCard,
                physicalIdentity,
                itemIdentity,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount,
                "out-of-range");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.False);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            Vector3 nearPosition = ResolveIssue99GraphicsCardPlayerPosition(marker);
            Vector3 awayFromSlot = nearPosition - (slotTarget - nearPosition);
            awayFromSlot.y = slotTarget.y;
            SetPlayerLook(
                marker,
                nearPosition,
                awayFromSlot);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.NotFocused));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            AssertIssue99GraphicsCardInputGateNoMutation(
                marker,
                session,
                graphicsCard,
                physicalIdentity,
                itemIdentity,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount,
                "wrong-target");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.False);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            MovePlayerToIssue99GraphicsCardSlot(marker);
            GameObject lineOfSightBlocker = CreateIssue99GraphicsCardLosBlocker(
                marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.LineOfSightBlocked));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            AssertIssue99GraphicsCardInputGateNoMutation(
                marker,
                session,
                graphicsCard,
                physicalIdentity,
                itemIdentity,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount,
                "line-of-sight");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.False);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            Object.Destroy(lineOfSightBlocker);
            yield return null;
            Physics.SyncTransforms();

            MovePlayerToIssue99GraphicsCardSlot(marker);
            GameObject seatObstruction = CreateIssue99GraphicsCardSeatObstruction(
                marker,
                graphicsCard);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.Obstructed));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            AssertIssue99GraphicsCardInputGateNoMutation(
                marker,
                session,
                graphicsCard,
                physicalIdentity,
                itemIdentity,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount,
                "seat-obstruction");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Object.Destroy(seatObstruction);
            yield return null;
            Physics.SyncTransforms();

            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunIssue99GraphicsCardAssemblyHandoffCycle(
            bool useGamepad)
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = useGamepad ? null : InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = useGamepad ? InputSystem.AddDevice<Gamepad>() : null;
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot graphicsCardLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.GraphicsCard);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule &&
                                   line.ComponentKind != PcComponentKind.StorageDevice &&
                                   line.ComponentKind != PcComponentKind.ProcessorCooler &&
                                   line.ComponentKind != PcComponentKind.GraphicsCard)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetIssue89Item(session, line.ItemId).ContainerId);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PrepareIssue99RetainedCooler(marker);

            PhysicalItemProjection graphicsCard = marker.GraphicsCard;
            int physicalIdentity = graphicsCard.GetInstanceID();
            string itemIdentity = graphicsCard.ItemIdValue;
            marker.GraphicsCardBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, graphicsCard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(graphicsCard));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("PCIe x16 MONTAJINA AL"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.GraphicsCardBuildKit.IsReleasedForAssembly, Is.True);
            Assert.That(marker.GraphicsCardBuildKit.ProgressText.text,
                Does.Contain("GPU MONTAJDA"));
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeGraphicsCardAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt handoff), Is.True);
            Assert.That(handoff.ComponentKind,
                Is.EqualTo(PcComponentKind.GraphicsCard));
            Assert.That(handoff.Line, Is.SameAs(graphicsCardLine));
            Assert.That(handoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[5]));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            long blockedDropInventoryRevision = session.Inventory.Revision;
            long blockedDropBuildKitRevision =
                session.CustomPcBuildKit.Revision;
            long blockedDropAssemblyRevision = session.AssemblyBuild.Revision;
            int blockedDropReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.LastFailureCode, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict.Code));
            AssertIssue99GraphicsCardInputGateNoMutation(
                marker,
                session,
                graphicsCard,
                physicalIdentity,
                itemIdentity,
                blockedDropInventoryRevision,
                blockedDropBuildKitRevision,
                blockedDropAssemblyRevision,
                blockedDropReceiptCount,
                "reserved-world-drop");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue99GraphicsCardSlot(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            long invalidAssemblyRevision = session.AssemblyBuild.Revision;
            long invalidInventoryRevision = session.Inventory.Revision;
            long invalidBuildKitRevision = session.CustomPcBuildKit.Revision;
            int invalidReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressIssue99Rotate(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.OrientationInvalid));
            ReleaseIssue99Rotate(marker, keyboard, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-graphics-card.orientation-mismatch"));
            AssertIssue99GraphicsCardInputGateNoMutation(
                marker,
                session,
                graphicsCard,
                physicalIdentity,
                itemIdentity,
                invalidInventoryRevision,
                invalidBuildKitRevision,
                invalidAssemblyRevision,
                invalidReceiptCount,
                "invalid-orientation");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            PressIssue99Rotate(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat));
            ReleaseIssue99Rotate(marker, keyboard, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            AssertIssue99GraphicsCardAtSlot(marker, graphicsCard, "initial-seat");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue99GraphicsCardSlot(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            long retainedRemoveInventoryRevision = session.Inventory.Revision;
            long retainedRemoveBuildKitRevision =
                session.CustomPcBuildKit.Revision;
            long retainedRemoveAssemblyRevision = session.AssemblyBuild.Revision;
            int retainedRemoveReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.GraphicsCardRetained.Code));
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(marker.GraphicsCardBinding.IsSeated, Is.True);
            Assert.That(marker.GraphicsCardBinding.IsRetained, Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(retainedRemoveInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(retainedRemoveBuildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(retainedRemoveAssemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(retainedRemoveReceiptCount));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue99GraphicsCardSlot(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue99GraphicsCardSlot(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            MovePlayerToIssue99GraphicsCardSlot(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(graphicsCard.ItemIdValue, Is.EqualTo(itemIdentity));

            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(6));
            AssertIssue89ReservationStillLive(
                session,
                workOrder,
                graphicsCardLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                untouchedContainers);
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            if (useGamepad)
            {
                Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
                Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
                Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
                Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));
                Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt,
                    Is.EqualTo("RB"));
            }
            else
            {
                Assert.That(mouse, Is.Not.Null);
            }
        }

        private static void PrepareIssue99RetainedCooler(
            GaragePrototypeMarker marker)
        {
            PrepareIssue97SecuredStorage(marker);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.ProcessorCooler));
            MovePlayerToIssue97CoolerSlot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorCoolerSeat());
            MovePlayerToIssue97CoolerSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorCoolerRetention());
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(marker.ProcessorCoolerBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
        }

        private static void MovePlayerToIssue99GraphicsCardSlot(
            GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.GraphicsCardSlot.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = ResolveIssue99GraphicsCardPlayerPosition(
                marker);
            SetPlayerLook(marker, playerPosition, target);
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static Vector3 ResolveIssue99GraphicsCardPlayerPosition(
            GaragePrototypeMarker marker)
        {
            Vector3 target = marker.GraphicsCardSlot.FocusCollider.bounds.center;
            Vector3 playerPosition = target +
                                     (ResolveIssue99GraphicsCardSlotOutward(marker) *
                                      1.25f);
            playerPosition.y = 0.05f;
            return playerPosition;
        }

        private static Vector3 ResolveIssue99GraphicsCardSlotOutward(
            GaragePrototypeMarker marker)
        {
            Vector3 target = marker.GraphicsCardSlot.FocusCollider.bounds.center;
            Vector3 outward = target -
                              marker.GraphicsCardSlot.AssemblyRoot.position;
            outward.y = 0f;
            if (outward.sqrMagnitude < 0.0001f)
            {
                outward = -marker.GraphicsCardSlot.SnapAnchor.forward;
                outward.y = 0f;
            }

            return outward.sqrMagnitude >= 0.0001f
                ? outward.normalized
                : Vector3.back;
        }

        private static void PressIssue99Rotate(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Gamepad gamepad,
            bool useGamepad)
        {
            if (useGamepad)
            {
                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState
                    {
                        buttons = 1u << (int)GamepadButton.RightShoulder
                    });
            }
            else
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.R));
            }

            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseIssue99Rotate(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Gamepad gamepad,
            bool useGamepad)
        {
            if (useGamepad)
            {
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
            }
            else
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            }

            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static GameObject CreateIssue99GraphicsCardLosBlocker(
            GaragePrototypeMarker marker)
        {
            Transform camera = marker.PlayerMotor.GetComponentInChildren<Camera>()
                .transform;
            Vector3 target = marker.GraphicsCardSlot.FocusCollider.bounds.center;
            Vector3 direction = target - camera.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "Issue99GraphicsCardLosBlocker";
            blocker.layer = 0;
            blocker.transform.SetPositionAndRotation(
                camera.position + (direction * 0.55f),
                Quaternion.LookRotation(direction.normalized, Vector3.up));
            blocker.transform.localScale = new Vector3(0.36f, 0.36f, 0.10f);
            Physics.SyncTransforms();
            return blocker;
        }

        private static GameObject CreateIssue99GraphicsCardSeatObstruction(
            GaragePrototypeMarker marker,
            PhysicalItemProjection graphicsCard)
        {
            Pose seatPose = marker.GraphicsCardSlot.ResolveSeatPose(0).Value;
            Transform camera = marker.PlayerMotor.GetComponentInChildren<Camera>()
                .transform;
            Vector3 target = marker.GraphicsCardSlot.FocusCollider.bounds.center;
            Vector3 viewDirection = (target - camera.position).normalized;
            Vector3 lateral = Vector3.Cross(Vector3.up, viewDirection).normalized;
            if (lateral.sqrMagnitude < 0.0001f)
            {
                lateral = seatPose.rotation * Vector3.right;
            }

            Vector3 halfExtents = graphicsCard.DropHalfExtents;
            Vector3 localRight = seatPose.rotation * Vector3.right;
            Vector3 localUp = seatPose.rotation * Vector3.up;
            Vector3 localForward = seatPose.rotation * Vector3.forward;
            float lateralHalfExtent =
                (Mathf.Abs(Vector3.Dot(lateral, localRight)) * halfExtents.x) +
                (Mathf.Abs(Vector3.Dot(lateral, localUp)) * halfExtents.y) +
                (Mathf.Abs(Vector3.Dot(lateral, localForward)) * halfExtents.z);
            float obstructionSize = Mathf.Clamp(
                lateralHalfExtent * 0.35f,
                0.025f,
                0.08f);
            GameObject obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Issue99GraphicsCardSeatInputObstruction";
            obstruction.layer = 0;
            obstruction.transform.position =
                graphicsCard.ResolveDropCenter(seatPose) +
                (lateral * lateralHalfExtent * 0.72f);
            obstruction.transform.localScale = Vector3.one * obstructionSize;
            Physics.SyncTransforms();
            Collider obstructionCollider = obstruction.GetComponent<Collider>();
            Collider[] overlaps = Physics.OverlapBox(
                graphicsCard.ResolveDropCenter(seatPose),
                halfExtents,
                seatPose.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            Assert.That(overlaps, Does.Contain(obstructionCollider),
                "The authored seat obstruction must overlap the real GPU seat volume.");
            return obstruction;
        }

        private static void AssertIssue99GraphicsCardInputGateNoMutation(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection graphicsCard,
            int physicalIdentity,
            string itemIdentity,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount,
            string stage)
        {
            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(graphicsCard), stage);
            Assert.That(graphicsCard.GetInstanceID(),
                Is.EqualTo(physicalIdentity), stage);
            Assert.That(graphicsCard.ItemIdValue,
                Is.EqualTo(itemIdentity), stage);
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands,
                Is.True, stage);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision), stage);
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision), stage);
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision), stage);
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount), stage);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen), stage);
        }

        private static void AssertIssue99GraphicsCardAtSlot(
            GaragePrototypeMarker marker,
            PhysicalItemProjection graphicsCard,
            string stage)
        {
            Assert.That(graphicsCard.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World), stage);
            Assert.That(graphicsCard.IsStablePlacement, Is.True, stage);
            Assert.That(graphicsCard.Body.isKinematic, Is.True, stage);
            Assert.That(graphicsCard.Body.useGravity, Is.False, stage);
            Pose expected = marker.GraphicsCardSlot.ResolveSeatPose(
                marker.StockFlow.Session.AssemblyBuild.GraphicsCardMountOrientation ==
                    GraphicsCardMountOrientation.Primary
                    ? 0
                    : 1).Value;
            Assert.That(Vector3.Distance(
                graphicsCard.transform.position,
                expected.position), Is.LessThan(0.0005f), stage);
            Assert.That(Quaternion.Angle(
                graphicsCard.transform.rotation,
                expected.rotation), Is.LessThan(0.05f), stage);
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True, stage);
        }
    }
}
