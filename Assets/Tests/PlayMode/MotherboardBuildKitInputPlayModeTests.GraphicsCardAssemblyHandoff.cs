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

            OperationResult blockedWorldDrop = marker.PlayerCarry.TryDrop();
            Assert.That(blockedWorldDrop.Error, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);

            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardSeatMode(true));
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            long invalidAssemblyRevision = session.AssemblyBuild.Revision;
            long invalidInventoryRevision = session.Inventory.Revision;
            int invalidReceiptCount = session.AssemblyBuild.ReceiptCount;
            AssertSuccess(marker.PlayerCarry.TryRotateGraphicsCardSeatPreview());
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.OrientationInvalid));
            OperationResult invalidSeat =
                marker.PlayerCarry.TryConfirmGraphicsCardSeat();
            Assert.That(invalidSeat.Error,
                Is.EqualTo(Failure.FromCode(
                    "assembly-graphics-card.orientation-mismatch")));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(invalidAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(invalidInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(invalidReceiptCount));

            AssertSuccess(marker.PlayerCarry.TryRotateGraphicsCardSeatPreview());
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat));
            AssertSuccess(marker.PlayerCarry.TryConfirmGraphicsCardSeat());

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            AssertIssue99GraphicsCardAtSlot(marker, graphicsCard, "initial-seat");

            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(marker.PlayerCarry.TryPickup(graphicsCard).Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardRetained));

            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            AssertSuccess(marker.PlayerCarry.TryPickup(graphicsCard));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));

            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmGraphicsCardSeat());
            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
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
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
            marker.PlayerCarry.ProcessInputFrame();
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
