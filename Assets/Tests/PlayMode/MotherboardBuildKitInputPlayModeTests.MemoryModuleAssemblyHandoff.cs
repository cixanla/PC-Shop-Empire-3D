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
        public IEnumerator KeyboardMouseCompletesMemoryAssemblyHandoffCycle()
        {
            yield return RunMemoryAssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesMemoryAssemblyHandoffCycle()
        {
            yield return RunMemoryAssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseRecoverySeatsMemoryBuildKitHandoff()
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
            CustomPcBuildOrderLineSnapshot memoryLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.MemoryModule);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PrepareIssue93RetainedProcessor(marker);

            PhysicalItemProjection memory = marker.MemoryModule;
            int physicalIdentity = memory.GetInstanceID();
            string itemIdentity = memory.ItemIdValue;
            AimPlayerAtItem(marker, memory, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, null, useGamepad: false);
            ReleaseIssue89Interact(marker, keyboard, null, useGamepad: false);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.MemoryModuleBuildKit.IsReleasedForAssembly, Is.True);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(recovery.IsSuccess, Is.True, recovery.Error.Code);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(GetIssue89Item(session, memoryLine.ItemId).ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(memory.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(Vector3.Distance(
                memory.transform.position,
                marker.DimmSlot.SnapPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                memory.transform.rotation,
                marker.DimmSlot.SnapPose.rotation), Is.LessThan(0.05f));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(3));
            AssertIssue89ReservationStillLive(session, workOrder, memoryLine);
            CustomPcBuildKitReceipt[] currentReceipts =
                CaptureIssue89StagingReceipts(session);
            for (int index = 0; index < currentReceipts.Length; index++)
            {
                Assert.That(currentReceipts[index],
                    Is.SameAs(historicalStagingReceipts[index]));
            }

            Assert.That(marker.DimmBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunMemoryAssemblyHandoffCycle(bool useGamepad)
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
            CustomPcBuildOrderLineSnapshot memoryLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.MemoryModule);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetIssue89Item(session, line.ItemId).ContainerId);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PrepareIssue93RetainedProcessor(marker);

            PhysicalItemProjection memory = marker.MemoryModule;
            int memoryPhysicalIdentity = memory.GetInstanceID();
            string memoryItemIdentity = memory.ItemIdValue;

            marker.MemoryModuleBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, memory, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(memory));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("DDR5'İ A2 MONTAJINA AL"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(marker.DimmBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.MemoryModuleBuildKit.IsReleasedForAssembly, Is.True);
            Assert.That(marker.MemoryModuleBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(marker.MemoryModuleBuildKit.ProgressText.text,
                Does.Contain("DDR5 MONTAJDA"));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(memoryPhysicalIdentity));
            Assert.That(memory.ItemIdValue, Is.EqualTo(memoryItemIdentity));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeMemoryModuleAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt memoryHandoff), Is.True);
            Assert.That(memoryHandoff.ComponentKind,
                Is.EqualTo(PcComponentKind.MemoryModule));
            Assert.That(memoryHandoff.Line, Is.SameAs(memoryLine));
            Assert.That(memoryHandoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[2]));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue93DimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsDimmSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(GetIssue89Item(session, memoryLine.ItemId).ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue93DimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(marker.DimmBinding.IsRetained, Is.True);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            yield return WaitForIssue93DimmLatches(marker);

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.MemoryModuleRetained.Code));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue93DimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(marker.DimmBinding.IsRetained, Is.False);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            yield return WaitForIssue93DimmLatches(marker);

            MovePlayerToIssue93DimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(memory));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(GetIssue89Item(session, memoryLine.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue93DimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsDimmSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentDimmSlotStatus,
                Is.EqualTo(DimmSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            MovePlayerToIssue93DimmSlot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            yield return WaitForIssue93DimmLatches(marker);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(GetIssue89Item(session, motherboardLine.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(GetIssue89Item(session, processorLine.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            Assert.That(GetIssue89Item(session, memoryLine.ItemId).ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            Assert.That(memory.GetInstanceID(), Is.EqualTo(memoryPhysicalIdentity));
            Assert.That(memory.ItemIdValue, Is.EqualTo(memoryItemIdentity));
            Assert.That(Vector3.Distance(
                memory.transform.position,
                marker.DimmSlot.SnapPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                memory.transform.rotation,
                marker.DimmSlot.SnapPose.rotation), Is.LessThan(0.05f));
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.Stable));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(3));
            AssertIssue89ReservationStillLive(session, workOrder, motherboardLine);
            AssertIssue89ReservationStillLive(session, workOrder, processorLine);
            AssertIssue89ReservationStillLive(session, workOrder, memoryLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                untouchedContainers);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(marker.ProcessorBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant()
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

        private static void PrepareIssue93RetainedProcessor(
            GaragePrototypeMarker marker)
        {
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(
                marker.MotherboardBinding.PhysicalItem));
            MovePlayerToMotherboardSeat(marker);
            AssertSuccess(marker.PlayerCarry.TrySetMotherboardSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmMotherboardSeat());
            MovePlayerToIssue89Fastener(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateMotherboardFastener());
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.Processor));
            MovePlayerToIssue91ProcessorSocket(marker);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorSeat());
            MovePlayerToIssue91ProcessorSocket(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorRetention());
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
        }

        private static IEnumerator WaitForIssue93DimmLatches(
            GaragePrototypeMarker marker)
        {
            int frames = 0;
            while (marker.DimmSlot.IsLatchAnimating && frames < 120)
            {
                frames++;
                yield return null;
            }

            Assert.That(marker.DimmSlot.IsLatchAnimating, Is.False);
            Assert.That(marker.DimmSlot.LatchVisualPhase,
                Is.EqualTo(DimmLatchVisualPhase.Stable));
        }

        private static void MovePlayerToIssue93DimmSlot(
            GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.DimmSlot.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
        }
    }
}
