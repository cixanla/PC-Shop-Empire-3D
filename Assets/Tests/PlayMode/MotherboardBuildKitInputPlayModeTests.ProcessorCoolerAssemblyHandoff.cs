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
        public IEnumerator KeyboardMouseCompletesCoolerBuildKitFourPointTimCycle()
        {
            yield return RunIssue97CoolerAssemblyHandoffCycle(useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesCoolerBuildKitFourPointTimCycle()
        {
            yield return RunIssue97CoolerAssemblyHandoffCycle(useGamepad: true);
        }

        [UnityTest]
        public IEnumerator CoolerBuildKitRecoverySeatsSameInstanceExactlyOnce()
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
            PrepareIssue97SecuredStorage(marker);
            Assert.That(marker.HasProcessorCoolerAssemblyHandoffR49Runtime,
                Is.True,
                "The installed M.2 and both cooler seat orientations must be physically disjoint.");

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            int physicalIdentity = cooler.GetInstanceID();
            string itemIdentity = cooler.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.ProcessorCoolerBuildKit.IsReleasedForAssembly, Is.True);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            AssertSuccess(recovery);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cooler.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            Assert.That(session.AssemblyBuild.ProcessorCoolerMountOrientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Primary));
            AssertIssue97ConsumedTim(session);
            AssertIssue97CoolerAtSlot(marker, cooler, "recovery-seat");
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(5));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));

            MovePlayerToIssue97CoolerSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorCoolerRetention());
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(marker.ProcessorCoolerBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunIssue97CoolerAssemblyHandoffCycle(bool useGamepad)
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
            CustomPcBuildOrderLineSnapshot coolerLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.ProcessorCooler);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                untouchedContainers = workOrder.Lines
                    .Where(line => line.ComponentKind != PcComponentKind.Motherboard &&
                                   line.ComponentKind != PcComponentKind.Processor &&
                                   line.ComponentKind != PcComponentKind.MemoryModule &&
                                   line.ComponentKind != PcComponentKind.StorageDevice &&
                                   line.ComponentKind != PcComponentKind.ProcessorCooler)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetIssue89Item(session, line.ItemId).ContainerId);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PrepareIssue97SecuredStorage(marker);
            Assert.That(marker.HasProcessorCoolerAssemblyHandoffR49Runtime,
                Is.True,
                "The installed M.2 and both cooler seat orientations must be physically disjoint.");

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            int physicalIdentity = cooler.GetInstanceID();
            string itemIdentity = cooler.ItemIdValue;
            marker.ProcessorCoolerBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, cooler, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cooler));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("4 NOKTALI MONTAJA AL"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.ProcessorCoolerBuildKit.IsReleasedForAssembly, Is.True);
            Assert.That(marker.ProcessorCoolerBuildKit.ProgressText.text,
                Does.Contain("SOĞUTUCU MONTAJDA"));
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cooler.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypeProcessorCoolerAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt handoff), Is.True);
            Assert.That(handoff.ComponentKind,
                Is.EqualTo(PcComponentKind.ProcessorCooler));
            Assert.That(handoff.Line, Is.SameAs(coolerLine));
            Assert.That(handoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[4]));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            OperationResult blockedWorldDrop = marker.PlayerCarry.TryDrop();
            Assert.That(blockedWorldDrop.Error, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);

            MovePlayerToIssue97CoolerSlot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerSeatMode(true));
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerSlotStatus,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat),
                $"{marker.PlayerCarry.LastFailureCode}\n" +
                DescribeIssue97CoolerSeatOverlaps(marker));
            AssertSuccess(marker.PlayerCarry.TryRotateProcessorCoolerSeatPreview());
            Assert.That(marker.ProcessorCoolerSlot.LastEvaluation.Orientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Rotated180));
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorCoolerSeat());

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            Assert.That(session.AssemblyBuild.ProcessorCoolerMountOrientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Rotated180));
            AssertIssue97ConsumedTim(session);
            AssertIssue97CoolerAtSlot(marker, cooler, "initial-seat");

            MovePlayerToIssue97CoolerSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorCoolerRetention());
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(marker.PlayerCarry.TryPickup(cooler).Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerRetained));

            MovePlayerToIssue97CoolerSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorCoolerRetention());
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            AssertSuccess(marker.PlayerCarry.TryPickup(cooler));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));

            MovePlayerToIssue97CoolerSlot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerSeatMode(true));
            OperationResult consumedTimReseat =
                marker.PlayerCarry.TryConfirmProcessorCoolerSeat();
            Assert.That(consumedTimReseat.Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerTimConsumed));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));
            AssertIssue97ConsumedInventoryFlag(session);
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cooler.ItemIdValue, Is.EqualTo(itemIdentity));

            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(5));
            AssertIssue89ReservationStillLive(session, workOrder, coolerLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                untouchedContainers);
            Assert.That(marker.ProcessorCoolerBinding.ValidateProjectionInvariant()
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

        private static void PrepareIssue97SecuredStorage(GaragePrototypeMarker marker)
        {
            PrepareIssue95RetainedMemory(marker);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.StorageDevice));
            MovePlayerToIssue95M2Slot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetM2StorageSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmM2StorageSeat());
            MovePlayerToIssue95M2Slot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateM2StorageCaptiveScrew());
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
            Assert.That(marker.StorageBinding.IsSecured, Is.True);
        }

        private static void MovePlayerToIssue97CoolerSlot(
            GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.ProcessorCoolerSlot.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void AssertIssue97ConsumedTim(GarageStockFlowSession session)
        {
            Assert.That(session.AssemblyBuild.ProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.AppliedConsumed));
            AssertIssue97ConsumedInventoryFlag(session);
        }

        private static void AssertIssue97ConsumedInventoryFlag(
            GarageStockFlowSession session)
        {
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord item), Is.True);
            Assert.That(
                item.StateFlags &
                InventorySerializedItemStateFlags.PreAppliedConsumableConsumed,
                Is.EqualTo(
                    InventorySerializedItemStateFlags.PreAppliedConsumableConsumed));
        }

        private static void AssertIssue97CoolerAtSlot(
            GaragePrototypeMarker marker,
            PhysicalItemProjection cooler,
            string stage)
        {
            Assert.That(cooler.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World), stage);
            Assert.That(cooler.IsStablePlacement, Is.True, stage);
            Assert.That(cooler.Body.isKinematic, Is.True, stage);
            Assert.That(cooler.Body.useGravity, Is.False, stage);
            Pose expected = marker.ProcessorCoolerSlot.ResolveSeatPose(
                marker.StockFlow.Session.AssemblyBuild
                    .ProcessorCoolerMountOrientation).Value;
            Assert.That(Vector3.Distance(
                cooler.transform.position,
                expected.position), Is.LessThan(0.0005f), stage);
            Assert.That(Quaternion.Angle(
                cooler.transform.rotation,
                expected.rotation), Is.LessThan(0.05f), stage);
            Assert.That(marker.ProcessorCoolerBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True, stage);
        }

        private static string DescribeIssue97CoolerSeatOverlaps(
            GaragePrototypeMarker marker)
        {
            Pose pose = marker.ProcessorCoolerSlot.ResolveSeatPose(
                ProcessorCoolerMountOrientation.Primary).Value;
            PhysicalItemProjection cooler = marker.ProcessorCooler;
            Physics.SyncTransforms();
            Collider[] overlaps = Physics.OverlapBox(
                cooler.ResolveDropCenter(pose),
                cooler.DropHalfExtents,
                pose.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Ignore);
            return "Issue97 seat overlaps: " + string.Join(
                " | ",
                overlaps.Select(collider =>
                    $"{ResolveIssue97TransformPath(collider.transform)} " +
                    $"layer={LayerMask.LayerToName(collider.gameObject.layer)} " +
                    $"center={collider.bounds.center} size={collider.bounds.size}"));
        }

        private static string ResolveIssue97TransformPath(Transform transform)
        {
            string path = transform != null ? transform.name : "<null>";
            Transform cursor = transform != null ? transform.parent : null;
            while (cursor != null)
            {
                path = $"{cursor.name}/{path}";
                cursor = cursor.parent;
            }

            return path;
        }
    }
}
