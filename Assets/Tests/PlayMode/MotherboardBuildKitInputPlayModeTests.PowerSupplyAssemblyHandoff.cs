using System;
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
        public IEnumerator KeyboardMouseCompletesPowerSupplyBuildKitBayRetentionCycle()
        {
            yield return RunIssue102PowerSupplyAssemblyHandoffCycle(
                useGamepad: false);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesPowerSupplyBuildKitBayRetentionCycle()
        {
            yield return RunIssue102PowerSupplyAssemblyHandoffCycle(
                useGamepad: true);
        }

        [UnityTest]
        public IEnumerator PowerSupplyBuildKitRecoverySeatsSameInstanceExactlyOnce()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.HasPowerSupplyAssemblyHandoffR51Runtime, Is.True);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue102RetainedGraphicsCard(marker);

            PhysicalItemProjection powerSupply = marker.PowerSupply;
            int physicalIdentity = powerSupply.GetInstanceID();
            string itemIdentity = powerSupply.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(powerSupply));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PowerSupplyBuildKit.IsReleasedForAssembly, Is.True);

            long assemblyRevisionBefore = session.AssemblyBuild.Revision;
            long inventoryRevisionBefore = session.Inventory.Revision;
            int receiptCountBefore = session.AssemblyBuild.ReceiptCount;
            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(recovery.IsSuccess, Is.True,
                $"{recovery.Error.Code} {DescribeIssue102PowerSupplySeatVolume(marker)}");
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(powerSupply.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            Assert.That(session.AssemblyBuild.PowerSupplyMountOrientation,
                Is.EqualTo(PowerSupplyMountOrientation.FanToFilteredVent));
            AssertIssue102PowerSupplyAtBay(marker, powerSupply, "recovery-seat");
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(7));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionBefore + 1));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBefore + 1));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountBefore + 1));
            Assert.That(UnityEngine.Object.FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsSortMode.None).Count(
                    item => item.ItemIdValue == itemIdentity),
                Is.EqualTo(1));

            long assemblyRevisionAfter = session.AssemblyBuild.Revision;
            long inventoryRevisionAfter = session.Inventory.Revision;
            int receiptCountAfter = session.AssemblyBuild.ReceiptCount;
            Assert.That(marker.PlayerCarry.TryRecoverHeldItem().Error,
                Is.EqualTo(Failure.FromCode("carry.nothing-held")));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevisionAfter));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionAfter));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCountAfter));

            MovePlayerToIssue102PowerSupplyBay(marker);
            AssertSuccess(marker.PlayerCarry.TryOperatePowerSupplyRetention());
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            Assert.That(marker.PowerSupplyBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator PowerSupplyBuildKitRecoveryFailsClosedWhenBayIsObstructed()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.HasPowerSupplyAssemblyHandoffR51Runtime, Is.True);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            PrepareIssue102RetainedGraphicsCard(marker);

            PhysicalItemProjection powerSupply = marker.PowerSupply;
            int physicalIdentity = powerSupply.GetInstanceID();
            string itemIdentity = powerSupply.ItemIdValue;
            AssertSuccess(marker.PlayerCarry.TryPickup(powerSupply));

            Pose seatPose = marker.PowerSupplyBay.ResolveSeatPose(0).Value;
            GameObject obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "Issue102RecoverySeatObstruction";
            obstruction.layer = 0;
            obstruction.transform.SetPositionAndRotation(
                seatPose.position,
                seatPose.rotation);
            obstruction.transform.localScale = Vector3.one * 0.25f;
            Physics.SyncTransforms();

            long assemblyRevision = session.AssemblyBuild.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int receiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult blocked = marker.PlayerCarry.TryRecoverHeldItem();

            Assert.That(blocked.Error,
                Is.EqualTo(Failure.FromCode("assembly-power-supply.obstructed")));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(powerSupply.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(powerSupply.IsCarried, Is.True);
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptCount));

            UnityEngine.Object.Destroy(obstruction);
            yield return null;
            Physics.SyncTransforms();

            OperationResult clearedRecovery = marker.PlayerCarry.TryRecoverHeldItem();
            Assert.That(clearedRecovery.IsSuccess, Is.True,
                $"{clearedRecovery.Error.Code} " +
                DescribeIssue102PowerSupplySeatVolume(marker));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(powerSupply.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            AssertIssue102PowerSupplyAtBay(marker, powerSupply, "recovery-after-clear");
            Assert.That(marker.PowerSupplyBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private IEnumerator RunIssue102PowerSupplyAssemblyHandoffCycle(
            bool useGamepad)
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = useGamepad ? null : InputSystem.AddDevice<Mouse>();
            Gamepad gamepad = useGamepad ? InputSystem.AddDevice<Gamepad>() : null;
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.HasPowerSupplyAssemblyHandoffR51Runtime, Is.True);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageCompleteIssue89BuildKit(marker);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot powerSupplyLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.PowerSupply);
            Dictionary<StableId<ItemInstanceIdScope>, StableId<ContainerIdScope>>
                cableContainers = workOrder.Lines
                    .Where(line => line.ComponentKind == PcComponentKind.PowerCable)
                    .ToDictionary(
                        line => line.ItemId,
                        line => GetIssue89Item(session, line.ItemId).ContainerId);
            CustomPcBuildKitReceipt[] historicalStagingReceipts =
                CaptureIssue89StagingReceipts(session);
            PrepareIssue102RetainedGraphicsCard(marker);

            PhysicalItemProjection powerSupply = marker.PowerSupply;
            int physicalIdentity = powerSupply.GetInstanceID();
            string itemIdentity = powerSupply.ItemIdValue;
            marker.PowerSupplyBuildKit.RefreshPresentation();
            AimPlayerAtItem(marker, powerSupply, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(powerSupply));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("10/10"));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("PSU'YU BAY MONTAJINA AL"));

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);

            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PowerSupplyBuildKit.IsReleasedForAssembly, Is.True);
            Assert.That(marker.PowerSupplyBuildKit.ProgressText.text,
                Does.Contain("PSU MONTAJDA"));
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(powerSupply.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.CustomPcBuildKit.TryGetAssemblyHandoff(
                session.PrototypePowerSupplyAssemblyHandoffOperationId,
                out CustomPcBuildKitAssemblyHandoffReceipt handoff), Is.True);
            Assert.That(handoff.ComponentKind, Is.EqualTo(PcComponentKind.PowerSupply));
            Assert.That(handoff.Line, Is.SameAs(powerSupplyLine));
            Assert.That(handoff.StagingReceipt,
                Is.SameAs(historicalStagingReceipts[6]));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            long blockedInventoryRevision = session.Inventory.Revision;
            long blockedBuildKitRevision = session.CustomPcBuildKit.Revision;
            long blockedAssemblyRevision = session.AssemblyBuild.Revision;
            int blockedReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.LastFailureCode, Is.EqualTo(
                InventoryFailures.SerializedReservationWorkOrderBuildKitConflict.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.Inventory.Revision, Is.EqualTo(blockedInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(blockedBuildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(blockedAssemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(blockedReceiptCount));
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue102PowerSupplyBay(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeIssue102PowerSupplySeatVolume(marker));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            AssertIssue102PowerSupplyAtBay(marker, powerSupply, "initial-seat");
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue102PowerSupplyBay(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.PowerSupplyRetained.Code));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue102PowerSupplyBay(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Interact(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            ReleaseIssue89Interact(marker, keyboard, gamepad, useGamepad);

            MovePlayerToIssue102PowerSupplyBay(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);
            PressIssue89Drop(marker, keyboard, gamepad, useGamepad);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            ReleaseIssue89Drop(marker, keyboard, gamepad, useGamepad);
            MovePlayerToIssue102PowerSupplyBay(marker);
            PressIssue89Primary(marker, mouse, gamepad, useGamepad);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            ReleaseIssue89Primary(marker, mouse, gamepad, useGamepad);

            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(powerSupply.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(session.CustomPcBuildKit.StagedComponentCount, Is.EqualTo(10));
            Assert.That(session.CustomPcBuildKit.AssemblyHandoffCount, Is.EqualTo(7));
            AssertIssue89ReservationStillLive(session, workOrder, powerSupplyLine);
            AssertIssue89HistoricalKitPreserved(
                session,
                historicalStagingReceipts,
                cableContainers);
            Assert.That(marker.PowerSupplyBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            if (useGamepad)
            {
                Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
                Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
                Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
                Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));
            }
            else
            {
                Assert.That(mouse, Is.Not.Null);
            }
        }

        private static void PrepareIssue102RetainedGraphicsCard(
            GaragePrototypeMarker marker)
        {
            PrepareIssue99RetainedCooler(marker);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.GraphicsCard));
            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmGraphicsCardSeat());
            MovePlayerToIssue99GraphicsCardSlot(marker);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
        }

        private static void MovePlayerToIssue102PowerSupplyBay(
            GaragePrototypeMarker marker)
        {
            Vector3 target = marker.PowerSupplyBay.FocusCollider.bounds.center;
            SetPlayerLook(
                marker,
                new Vector3(-0.95f, 0.05f, 3.15f),
                target);
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void AssertIssue102PowerSupplyAtBay(
            GaragePrototypeMarker marker,
            PhysicalItemProjection powerSupply,
            string context)
        {
            OperationResult<Pose> seatPose = marker.PowerSupplyBay.ResolveSeatPose(0);
            Assert.That(seatPose.IsSuccess, Is.True, context);
            Assert.That(Vector3.Distance(
                    powerSupply.transform.position,
                    seatPose.Value.position),
                Is.LessThanOrEqualTo(0.0005f), context);
            Assert.That(Quaternion.Angle(
                    powerSupply.transform.rotation,
                    seatPose.Value.rotation),
                Is.LessThanOrEqualTo(0.05f), context);
        }

        private static string DescribeIssue102PowerSupplySeatVolume(
            GaragePrototypeMarker marker)
        {
            Pose seatPose = marker.PowerSupplyBay.ResolveSeatPose(0).Value;
            PhysicalItemProjection powerSupply = marker.PowerSupply;
            Vector3 center = powerSupply.ResolveDropCenter(seatPose);
            Vector3 normal = marker.PowerSupplyBay.AssemblyRoot.forward.sqrMagnitude >
                             Mathf.Epsilon
                ? -marker.PowerSupplyBay.AssemblyRoot.forward.normalized
                : marker.PowerSupplyBay.SnapAnchor.forward.normalized;
            Collider[] overlaps = Physics.OverlapBox(
                center,
                powerSupply.DropHalfExtents,
                seatPose.rotation,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide);
            RaycastHit[] insertionHits = Physics.BoxCastAll(
                center + normal * 0.18f,
                powerSupply.DropHalfExtents,
                -normal,
                seatPose.rotation,
                0.18f,
                Physics.AllLayers,
                QueryTriggerInteraction.Collide);
            string overlapNames = string.Join(",", overlaps.Select(
                collider => collider != null
                    ? $"{collider.name}@{collider.bounds.center}/{collider.bounds.size}"
                    : "null"));
            string insertionNames = string.Join(",", insertionHits.Select(
                hit => hit.collider != null ? hit.collider.name : "null"));
            return $"seat={seatPose.position} insertion={normal} " +
                   $"support={marker.PowerSupplyBay.AssemblyRoot.up} " +
                   $"slotLocal={marker.GraphicsCardSlot.transform.localPosition} " +
                   $"gpu={marker.GraphicsCard.GetComponent<Collider>().bounds.center}/" +
                   $"{marker.GraphicsCard.GetComponent<Collider>().bounds.size} " +
                   $"overlaps=[{overlapNames}] casts=[{insertionNames}]";
        }
    }
}
