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
        public IEnumerator KeyboardMouseCompletesMotherboardAssemblyHandoffCycle()
        {
            yield return RunMotherboardAssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesMotherboardAssemblyHandoffCycle()
        {
            yield return RunMotherboardAssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseRecoverySeatsBuildKitHandoffWithoutLosingCustody()
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
            CustomPcBuildOrderLineSnapshot motherboardLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.Motherboard);
            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            int physicalIdentity = motherboard.GetInstanceID();
            string itemIdentity = motherboard.ItemIdValue;

            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("MONTAJA AL"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            PressIssue89Interact(marker, keyboard, null, useGamepad: false);
            ReleaseIssue89Interact(marker, keyboard, null, useGamepad: false);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(recovery.IsSuccess, Is.True, recovery.Error.Code);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(GetIssue89Item(session, motherboardLine.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(motherboard.ItemIdValue, Is.EqualTo(itemIdentity));
            AssertIssue89ReservationStillLive(session, workOrder, motherboardLine);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunMotherboardAssemblyHandoffCycle(bool useGamepad)
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
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetIssue89Item(session, line.ItemId).ContainerId);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);

            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;
            int physicalIdentity = motherboard.GetInstanceID();
            string itemIdentity = motherboard.ItemIdValue;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(buildKit.ProgressText.text, Does.Contain("10/10"));
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.True);

            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(motherboard));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("MONTAJA AL"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.IsReleasedForAssembly, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(buildKit.ProgressText.text, Does.Contain("ANAKART MONTAJDA"));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(motherboard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeMotherboardAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt handoff), Is.True);
            Assert.That(handoff.Line, Is.SameAs(motherboardLine));
            Assert.That(handoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[0]));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToMotherboardSeat(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsMotherboardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(GetIssue89Item(session, motherboardLine.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue89Fastener(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardFastenerStatus,
                Is.EqualTo(MotherboardFastenerStatus.ValidUnsecured),
                marker.PlayerCarry.LastFailureCode);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(marker.MotherboardBinding.IsSecured, Is.True);
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(marker.MotherboardBinding.IsSecured, Is.False);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            marker.PlayerCarry.ProcessInputFrame();
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(GetIssue89Item(session, motherboardLine.ItemId).ContainerId,
                Is.EqualTo(session.HandsContainerId));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
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

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(GetIssue89Item(session, motherboardLine.ItemId).ContainerId,
                Is.EqualTo(session.WorkbenchContainerId));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(motherboard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 4));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision + 5));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount + 5));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(1));
            AssertIssue89ReservationStillLive(session, workOrder, motherboardLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                untouchedContainers);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            if (useGamepad)
            {
                Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
                Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
                Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
                Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));
            }

            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
        }

        private static void StageCompleteIssue89BuildKit(
            GaragePrototypeMarker marker)
        {
            StageFirstNineForPcieGpuPowerCableBuildKit(marker);
            PcieGpuPowerCableBuildKitProjection buildKit =
                marker.PcieGpuPowerCableBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(
                marker.PcieGpuPowerCableBinding.PhysicalItem));
            MovePlayerToPcieGpuPowerCableBuildKit(marker, buildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetPcieGpuPowerCableBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentPcieGpuPowerCableBuildKitStatus,
                Is.EqualTo(PcieGpuPowerCableBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmPcieGpuPowerCableBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(marker.PcieGpuPowerCableBinding.IsAuthorityInBuildKit,
                Is.True);
        }

        private static CustomPcBuildKitReceipt[] CaptureIssue89StagingReceipts(
            GarageStockFlowSession session)
        {
            StableId<CustomPcBuildKitOperationIdScope>[] operationIds =
            {
                session.PrototypeCustomPcBuildKitOperationId,
                session.PrototypeProcessorBuildKitOperationId,
                session.PrototypeMemoryModuleBuildKitOperationId,
                session.PrototypeStorageBuildKitOperationId,
                session.PrototypeProcessorCoolerBuildKitOperationId,
                session.PrototypeGraphicsCardBuildKitOperationId,
                session.PrototypePowerSupplyBuildKitOperationId,
                session.PrototypeAtx24PowerCableBuildKitOperationId,
                session.PrototypeEps12vPowerCableBuildKitOperationId,
                session.PrototypePcieGpuPowerCableBuildKitOperationId
            };
            var receipts = new CustomPcBuildKitReceipt[operationIds.Length];
            for (int index = 0; index < operationIds.Length; index++)
            {
                Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                    operationIds[index],
                    out receipts[index]), Is.True);
                Assert.That(receipts[index], Is.Not.Null);
            }

            return receipts;
        }

        private static void AssertIssue89HistoricalKitPreserved(
            GarageStockFlowSession session,
            IReadOnlyList<CustomPcBuildKitReceipt> expectedReceipts,
            IReadOnlyDictionary<StableId<ItemInstanceIdScope>,
                StableId<ContainerIdScope>> expectedContainers)
        {
            CustomPcBuildKitReceipt[] currentReceipts =
                CaptureIssue89StagingReceipts(session);
            Assert.That(currentReceipts.Length, Is.EqualTo(expectedReceipts.Count));
            for (int index = 0; index < currentReceipts.Length; index++)
            {
                Assert.That(currentReceipts[index], Is.SameAs(expectedReceipts[index]));
            }

            foreach (KeyValuePair<StableId<ItemInstanceIdScope>,
                         StableId<ContainerIdScope>> entry in expectedContainers)
            {
                Assert.That(GetIssue89Item(session, entry.Key).ContainerId,
                    Is.EqualTo(entry.Value));
            }
        }

        private static InventoryItemRecord GetIssue89Item(
            GarageStockFlowSession session,
            StableId<ItemInstanceIdScope> itemId)
        {
            Assert.That(session.Inventory.TryGetSerializedItem(
                itemId,
                out InventoryItemRecord item), Is.True);
            return item;
        }

        private static void AssertIssue89ReservationStillLive(
            GarageStockFlowSession session,
            CustomPcBuildOrderRecord workOrder,
            CustomPcBuildOrderLineSnapshot line)
        {
            Assert.That(session.Inventory.TryGetReservation(
                line.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(line.ItemId));
            Assert.That(reservation.ClaimId, Is.EqualTo(workOrder.InventoryClaimId));
        }

        private static void MovePlayerToIssue89Fastener(
            GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.MotherboardFastener.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void PressIssue89Interact(
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
                        buttons = 1u << (int)GamepadButton.South
                    });
            }
            else
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            }

            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseIssue89Interact(
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

        private static void PressIssue89Primary(
            GaragePrototypeMarker marker,
            Mouse mouse,
            Gamepad gamepad,
            bool useGamepad)
        {
            if (useGamepad)
            {
                InputSystem.QueueStateEvent(
                    gamepad,
                    new GamepadState { rightTrigger = 1f });
            }
            else
            {
                InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            }

            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseIssue89Primary(
            GaragePrototypeMarker marker,
            Mouse mouse,
            Gamepad gamepad,
            bool useGamepad)
        {
            if (useGamepad)
            {
                InputSystem.QueueStateEvent(gamepad, new GamepadState());
            }
            else
            {
                InputSystem.QueueStateEvent(mouse, new MouseState());
            }

            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void PressIssue89Drop(
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
                        buttons = 1u << (int)GamepadButton.East
                    });
            }
            else
            {
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            }

            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseIssue89Drop(
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
    }
}
