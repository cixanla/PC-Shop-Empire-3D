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
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class MotherboardBuildKitInputPlayModeTests
    {
        [UnityTest]
        public IEnumerator KeyboardMouseCompletesProcessorAssemblyHandoffCycle()
        {
            yield return RunProcessorAssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesProcessorAssemblyHandoffCycle()
        {
            yield return RunProcessorAssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseRecoverySeatsProcessorBuildKitHandoff()
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
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot processorLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);

            AssertSuccess(marker.PlayerCarry.TryPickup(
                marker.MotherboardBinding.PhysicalItem));
            MovePlayerToMotherboardSeat(marker);
            AssertSuccess(marker.PlayerCarry.TrySetMotherboardSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmMotherboardSeat());
            MovePlayerToIssue89Fastener(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateMotherboardFastener());
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));

            PhysicalItemProjection processor = marker.Processor;
            int physicalIdentity = processor.GetInstanceID();
            string itemIdentity = processor.ItemIdValue;
            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, null, useGamepad: false);
            ReleaseIssue89Interact(marker, keyboard, null, useGamepad: false);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.ProcessorBuildKit.IsReleasedForAssembly, Is.True);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(recovery.IsSuccess, Is.True, recovery.Error.Code);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            Assert.That(GetIssue89Item(session, processorLine.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            Assert.That(processor.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(processor.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(Vector3.Distance(
                processor.transform.position,
                marker.ProcessorSocket.SnapPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                processor.transform.rotation,
                marker.ProcessorSocket.SnapPose.rotation), Is.LessThan(0.05f));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(2));
            AssertIssue89ReservationStillLive(session, workOrder, processorLine);
            CustomPcBuildKitReceipt[] currentReceipts =
                CaptureIssue89StagingReceipts(session);
            for (int index = 0; index < currentReceipts.Length; index++)
            {
                Assert.That(currentReceipts[index],
                    Is.SameAs(historicalStagingReceipts[index]));
            }

            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunProcessorAssemblyHandoffCycle(bool useGamepad)
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

            CustomPcBuildOrderLineSnapshot motherboardLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
            CustomPcBuildOrderLineSnapshot processorLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Processor);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetIssue89Item(session, line.ItemId).ContainerId);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            PhysicalItemProjection processor = marker.Processor;
            int processorPhysicalIdentity = processor.GetInstanceID();
            string processorItemIdentity = processor.ItemIdValue;
            long initialInventoryRevision = session.Inventory.Revision;
            long initialBuildKitRevision = session.CustomPcBuildKit.Revision;
            long initialAssemblyRevision = session.AssemblyBuild.Revision;
            int initialAssemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToMotherboardSeat(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsMotherboardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue89Fastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            marker.ProcessorBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, processor, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(processor));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("CPU'YU SOKET MONTAJINA AL"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(marker.ProcessorBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.ProcessorBuildKit.IsReleasedForAssembly, Is.True);
            Assert.That(marker.ProcessorBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(marker.ProcessorBuildKit.ProgressText.text,
                Does.Contain("CPU MONTAJDA"));
            Assert.That(processor.GetInstanceID(), Is.EqualTo(processorPhysicalIdentity));
            Assert.That(processor.ItemIdValue, Is.EqualTo(processorItemIdentity));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeProcessorAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt processorHandoff), Is.True);
            Assert.That(processorHandoff.ComponentKind,
                Is.EqualTo(PcComponentKind.Processor));
            Assert.That(processorHandoff.Line, Is.SameAs(processorLine));
            Assert.That(processorHandoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[1]));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue91ProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            Assert.That(GetIssue89Item(session, processorLine.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue91ProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(marker.ProcessorBinding.IsRetained, Is.True);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.ProcessorRetained.Code));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue91ProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            Assert.That(marker.ProcessorBinding.IsRetained, Is.False);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(processor));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(GetIssue89Item(session, processorLine.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue91ProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsProcessorSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentProcessorSocketStatus,
                Is.EqualTo(ProcessorSocketStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            MovePlayerToIssue91ProcessorSocket(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(GetIssue89Item(session, motherboardLine.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(GetIssue89Item(session, processorLine.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            Assert.That(processor.GetInstanceID(), Is.EqualTo(processorPhysicalIdentity));
            Assert.That(processor.ItemIdValue, Is.EqualTo(processorItemIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(initialInventoryRevision + 6));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(initialBuildKitRevision + 2));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(initialAssemblyRevision + 8));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(initialAssemblyReceiptCount + 8));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(2));
            AssertIssue89ReservationStillLive(session, workOrder, motherboardLine);
            AssertIssue89ReservationStillLive(session, workOrder, processorLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                untouchedContainers);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            if (useGamepad)
            {
                Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
                Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
                Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
                Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));
            }
        }

        private static void MovePlayerToIssue91ProcessorSocket(
            GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.ProcessorSocket.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
        }
    }
}
