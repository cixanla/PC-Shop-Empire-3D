using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public IEnumerator KeyboardMouseCompletesStorageAssemblyHandoffCycle()
        {
            yield return RunStorageAssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesStorageAssemblyHandoffCycle()
        {
            yield return RunStorageAssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseRecoverySeatsStorageBuildKitHandoff()
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
            CustomPcBuildOrderLineSnapshot storageLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.StorageDevice);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PrepareIssue95RetainedMemory(marker);
            yield return WaitForIssue93DimmLatches(marker);

            PhysicalItemProjection storage = marker.StorageDevice;
            int physicalIdentity = storage.GetInstanceID();
            string itemIdentity = storage.ItemIdValue;
            AimPlayerAtItem(marker, storage, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, null, useGamepad: false);
            ReleaseIssue89Interact(marker, keyboard, null, useGamepad: false);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.StorageBuildKit.IsReleasedForAssembly, Is.True);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(recovery.IsSuccess, Is.True, recovery.Error.Code);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            Assert.That(GetIssue89Item(session, storageLine.ItemId).ContainerId,
                Is.EqualTo(session.StorageSlotContainerId));
            Assert.That(storage.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(storage.ItemIdValue, Is.EqualTo(itemIdentity));
            AssertIssue95StorageAtSlot(marker, storage, "recovery-seat");
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(4));
            AssertIssue89ReservationStillLive(session, workOrder, storageLine);
            CustomPcBuildKitReceipt[] currentReceipts =
                CaptureIssue89StagingReceipts(session);
            for (int index = 0; index < currentReceipts.Length; index++)
            {
                Assert.That(currentReceipts[index],
                    Is.SameAs(historicalStagingReceipts[index]));
            }

            Assert.That(marker.StorageBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator StorageBuildKitPickupFailsClosedWithoutRecoveryHeadroom()
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
            PrepareIssue95RetainedMemory(marker);
            yield return WaitForIssue93DimmLatches(marker);

            PhysicalItemProjection storage = marker.StorageDevice;
            long originalInventoryRevision = session.Inventory.Revision;
            long originalBuildKitRevision = session.CustomPcBuildKit.Revision;
            long originalAssemblyRevision = session.AssemblyBuild.Revision;
            int originalHandoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;
            SetIssue95Revision(session.Inventory, long.MaxValue - 2L);

            OperationResult pickup = marker.PlayerCarry.TryPickup(storage);

            Assert.That(pickup.Error,
                Is.EqualTo(AssemblyFailures.InventoryRevisionOverflow));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(storage.IsCarried, Is.False);
            Assert.That(marker.StorageBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(marker.StorageBuildKit.IsReleasedForAssembly, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(long.MaxValue - 2L));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(originalBuildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(originalAssemblyRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(originalHandoffCount));

            SetIssue95Revision(session.Inventory, originalInventoryRevision);
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator StorageBuildKitRecoveryFailsClosedWithoutHeadroom()
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
            PrepareIssue95RetainedMemory(marker);
            yield return WaitForIssue93DimmLatches(marker);

            PhysicalItemProjection storage = marker.StorageDevice;
            AssertSuccess(marker.PlayerCarry.TryPickup(storage));
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);
            long originalAssemblyRevision = session.AssemblyBuild.Revision;
            long originalInventoryRevision = session.Inventory.Revision;
            long originalBuildKitRevision = session.CustomPcBuildKit.Revision;
            int originalHandoffCount = session.CustomPcBuildKit.AssemblyHandoffCount;
            SetIssue95Revision(session.AssemblyBuild, long.MaxValue - 1L);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(recovery.Error, Is.EqualTo(AssemblyFailures.RevisionOverflow));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(storage.IsCarried, Is.True);
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(long.MaxValue - 1L));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(originalInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(originalBuildKitRevision));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount,
                Is.EqualTo(originalHandoffCount));

            SetIssue95Revision(session.AssemblyBuild, originalAssemblyRevision);
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunStorageAssemblyHandoffCycle(bool useGamepad)
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
            CustomPcBuildOrderLineSnapshot storageLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.StorageDevice);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule &&
                                   line.ComponentKind != PcComponentKind.StorageDevice)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetIssue89Item(session, line.ItemId).ContainerId);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PrepareIssue95RetainedMemory(marker);
            yield return WaitForIssue93DimmLatches(marker);

            PhysicalItemProjection storage = marker.StorageDevice;
            int storagePhysicalIdentity = storage.GetInstanceID();
            string storageItemIdentity = storage.ItemIdValue;

            marker.StorageBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, storage, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(storage));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("PRIMARY SLOT MONTAJINA AL"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.StorageBuildKit.IsReleasedForAssembly, Is.True);
            Assert.That(marker.StorageBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(marker.StorageBuildKit.ProgressText.text,
                Does.Contain("M.2 MONTAJDA"));
            Assert.That(storage.GetInstanceID(), Is.EqualTo(storagePhysicalIdentity));
            Assert.That(storage.ItemIdValue, Is.EqualTo(storageItemIdentity));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeStorageAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt storageHandoff), Is.True);
            Assert.That(storageHandoff.ComponentKind,
                Is.EqualTo(PcComponentKind.StorageDevice));
            Assert.That(storageHandoff.Line, Is.SameAs(storageLine));
            Assert.That(storageHandoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[3]));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue95M2Slot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsM2StorageSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentM2StorageSlotStatus,
                Is.EqualTo(M2StorageSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("18°"));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue95Rotate(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.CurrentM2StorageSlotStatus,
                Is.EqualTo(M2StorageSlotStatus.OrientationInvalid));
            ReleaseIssue95Rotate(marker, keyboard, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.M2OrientationMismatch.Code));
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            PressIssue95Rotate(marker, keyboard, gamepad, useGamepad);
            ReleaseIssue95Rotate(marker, keyboard, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            Assert.That(GetIssue89Item(session, storageLine.ItemId).ContainerId,
                Is.EqualTo(session.StorageSlotContainerId));
            AssertIssue95StorageAtSlot(marker, storage, "initial-seat");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue95M2Slot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
            Assert.That(marker.StorageBinding.IsSecured, Is.True);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.StorageDeviceSecured.Code));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue95M2Slot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            Assert.That(marker.StorageBinding.IsSecured, Is.False);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(GetIssue89Item(session, storageLine.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue95M2Slot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            MovePlayerToIssue95M2Slot(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
            Assert.That(GetIssue89Item(session, motherboardLine.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(GetIssue89Item(session, processorLine.ItemId).ContainerId,
                Is.EqualTo(session.ProcessorSocketContainerId));
            Assert.That(GetIssue89Item(session, memoryLine.ItemId).ContainerId,
                Is.EqualTo(session.MemorySlotContainerId));
            Assert.That(GetIssue89Item(session, storageLine.ItemId).ContainerId,
                Is.EqualTo(session.StorageSlotContainerId));
            Assert.That(storage.GetInstanceID(), Is.EqualTo(storagePhysicalIdentity));
            Assert.That(storage.ItemIdValue, Is.EqualTo(storageItemIdentity));
            AssertIssue95StorageAtSlot(marker, storage, "final-secured");
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(4));
            AssertIssue89ReservationStillLive(session, workOrder, motherboardLine);
            AssertIssue89ReservationStillLive(session, workOrder, processorLine);
            AssertIssue89ReservationStillLive(session, workOrder, memoryLine);
            AssertIssue89ReservationStillLive(session, workOrder, storageLine);
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
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant()
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
        }

        private static void PrepareIssue95RetainedMemory(GaragePrototypeMarker marker)
        {
            PrepareIssue93RetainedProcessor(marker);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.MemoryModule));
            MovePlayerToIssue93DimmSlot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetDimmSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmDimmSeat());
            MovePlayerToIssue93DimmSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateDimmRetention());
            Assert.That(session.AssemblyBuild.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(marker.DimmBinding.IsRetained, Is.True);
        }

        private static void MovePlayerToIssue95M2Slot(GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.StorageSlot.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void PressIssue95Rotate(
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

        private static void ReleaseIssue95Rotate(
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

        private static void AssertIssue95StorageAtSlot(
            GaragePrototypeMarker marker,
            PhysicalItemProjection storage,
            string stage)
        {
            Assert.That(storage.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World), stage);
            Assert.That(storage.IsStablePlacement, Is.True, stage);
            Assert.That(storage.Body.isKinematic, Is.True, stage);
            Assert.That(storage.Body.useGravity, Is.False, stage);
            Assert.That(Vector3.Distance(
                storage.transform.position,
                marker.StorageSlot.SeatedPose.position), Is.LessThan(0.0005f), stage);
            Assert.That(Quaternion.Angle(
                storage.transform.rotation,
                marker.StorageSlot.SeatedPose.rotation), Is.LessThan(0.05f), stage);
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True, stage);
        }

        private static void SetIssue95Revision(object authority, long revision)
        {
            PropertyInfo property = authority.GetType().GetProperty(
                "Revision",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null);
            property.GetSetMethod(nonPublic: true).Invoke(
                authority,
                new object[] { revision });
        }
    }
}
