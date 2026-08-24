using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    /// <summary>
    /// Real Input System coverage for the first physical custom-PC Build Kit handoff.
    /// </summary>
    public sealed class MotherboardBuildKitInputPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardMouseMovesExactReservedMotherboardIntoBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);

            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(out _), Is.True);
            CustomPcBuildOrderLineSnapshot motherboardLine = workOrder.Lines.Single(
                line => line.ComponentKind == PCShopEmpire3D.Catalog.PcComponentKind.Motherboard);
            Assert.That(motherboardLine.ItemId, Is.EqualTo(session.MotherboardItemId));
            Assert.That(session.Inventory.TryGetReservation(
                motherboardLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(motherboardLine.ItemId));

            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;
            int physicalIdentity = motherboard.GetInstanceID();
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.ProgressText.text, Does.Contain("0/10"));
            Assert.That(buildKit.StagedComponentCount, Is.Zero);
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.False);

            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem,
                Is.SameAs(motherboard),
                marker.PlayerCarry.LastFailureCode);
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("0/10 → 1/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(buildKit.ProgressText.text, Does.Contain("1/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("ANAKART HAZIR"));
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(motherboard.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(motherboard.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(motherboard), Is.True);
            Assert.That(session.Inventory.TryGetSerializedItem(
                motherboardLine.ItemId,
                out InventoryItemRecord stagedItem), Is.True);
            Assert.That(stagedItem.ContainerId,
                Is.EqualTo(session.CustomPcBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                motherboardLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId, Is.EqualTo(stagedItem.Id));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(2));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            long committedInventoryRevision = session.Inventory.Revision;
            long committedBuildKitRevision = session.CustomPcBuildKit.Revision;
            ReleaseKeyboard(marker, keyboard);
            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(committedInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(committedBuildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
        }

        [UnityTest]
        public IEnumerator WrongTargetGenericDropAndLosBlockerRemainFailClosed()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);

            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;
            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            ReleaseKeyboard(marker, keyboard);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Vector3 heldLocalPosition = motherboard.transform.localPosition;
            Quaternion heldLocalRotation = motherboard.transform.localRotation;

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(
                marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(
                    InventoryFailures
                        .SerializedReservationWorkOrderBuildKitConflict.Code));
            AssertHeldBuildKitStateUnchanged(
                marker,
                session,
                motherboard,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount,
                heldLocalPosition,
                heldLocalRotation);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToMotherboardSeat(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsMotherboardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentMotherboardSeatStatus,
                Is.EqualTo(MotherboardSeatStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.InventoryTransferRejected.Code));
            AssertHeldBuildKitStateUnchanged(
                marker,
                session,
                motherboard,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount,
                heldLocalPosition,
                heldLocalRotation);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            ReleaseMouse(marker, mouse);

            GameObject blocker = CreateBuildKitLosBlocker(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.LineOfSightBlocked),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);

            PressKeyboard(marker, keyboard, Key.G);
            AssertHeldBuildKitStateUnchanged(
                marker,
                session,
                motherboard,
                inventoryRevision,
                buildKitRevision,
                assemblyRevision,
                assemblyReceiptCount,
                heldLocalPosition,
                heldLocalRotation);
            Object.DestroyImmediate(blocker);

            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static IEnumerator LoadGarage(
            System.Action<GaragePrototypeMarker> assign)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            assign(marker);
        }

        private static IEnumerator PrepareQuote(
            GaragePrototypeMarker marker,
            Keyboard keyboard)
        {
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            yield return WaitForCustomerState(
                customerFlow,
                PCShopEmpire3D.Actors.CustomerVisitState.Browsing);
            MovePlayerToCustomer(marker, customerFlow);

            PressCustomerInteract(keyboard, customerFlow);
            ReleaseKeyboard(keyboard);
            yield return new WaitForFixedUpdate();
            PressCustomerInteract(keyboard, customerFlow);
            ReleaseKeyboard(keyboard);
            yield return new WaitForFixedUpdate();
            PressCustomerInteract(keyboard, customerFlow);
            ReleaseKeyboard(keyboard);
            yield return null;

            Assert.That(customerFlow.CustomPcQuoteReady, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcQuote(out _), Is.True);
        }

        private static IEnumerator IssuePhysicalWorkTicket(
            GaragePrototypeMarker marker,
            Keyboard keyboard)
        {
            MovePlayerToStation(marker, 1.35f);
            marker.CustomPcWorkTicketStation.RefreshPresentation();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(out _), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(out _), Is.True);
        }

        private static void PressKeyboard(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseKeyboard(
            GaragePrototypeMarker marker,
            Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseKeyboard(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        private static void PressMouse(
            GaragePrototypeMarker marker,
            Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseMouse(
            GaragePrototypeMarker marker,
            Mouse mouse)
        {
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void PressCustomerInteract(
            Keyboard keyboard,
            GarageCustomerFlowRuntime customerFlow)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            customerFlow.ProcessInputFrame();
        }

        private static void MovePlayerToCustomer(
            GaragePrototypeMarker marker,
            GarageCustomerFlowRuntime customerFlow)
        {
            Vector3 target = customerFlow.CustomerVisualRoot.transform.position +
                             (Vector3.up * 1.35f);
            Vector3 playerPosition = target - (Vector3.right * 1.55f);
            playerPosition.y = 0.05f;
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void MovePlayerToStation(
            GaragePrototypeMarker marker,
            float distance)
        {
            Collider targetCollider = marker.CustomPcWorkTicketStation
                .InteractionCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 approach = -targetCollider.transform.forward;
            approach.y = 0f;
            approach.Normalize();
            Vector3 playerPosition = target + (approach * distance);
            playerPosition.y = 0.05f;
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void MovePlayerToBuildKit(
            GaragePrototypeMarker marker,
            MotherboardBuildKitProjection buildKit)
        {
            Collider support = buildKit.SupportCollider;
            Vector3 target = new Vector3(
                support.bounds.center.x,
                support.bounds.max.y,
                support.bounds.center.z);
            Vector3 playerPosition = target + (Vector3.back * 0.95f);
            playerPosition.y = 0.05f;
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void MovePlayerToMotherboardSeat(
            GaragePrototypeMarker marker)
        {
            Collider targetCollider = marker.MotherboardSeat.FocusCollider;
            Vector3 target = targetCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.95f, 0.05f, 3.15f);
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void AimPlayerAtItem(
            GaragePrototypeMarker marker,
            PhysicalItemProjection item,
            Vector3 approachDirection)
        {
            Vector3 target = item.InteractionCenter;
            Vector3 playerPosition = target +
                                     (approachDirection.normalized * 1.25f);
            playerPosition.y = 0.05f;
            SetPlayerLook(marker, playerPosition, target);
        }

        private static void SetPlayerLook(
            GaragePrototypeMarker marker,
            Vector3 playerPosition,
            Vector3 target)
        {
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static GameObject CreateBuildKitLosBlocker(
            GaragePrototypeMarker marker,
            MotherboardBuildKitProjection buildKit)
        {
            Transform camera = marker.PlayerMotor.GetComponentInChildren<Camera>()
                .transform;
            Collider support = buildKit.SupportCollider;
            Vector3 target = new Vector3(
                support.bounds.center.x,
                support.bounds.max.y,
                support.bounds.center.z);
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "MotherboardBuildKitLosBlocker";
            blocker.layer = 0;
            blocker.transform.position = Vector3.Lerp(
                camera.position,
                target,
                0.55f);
            blocker.transform.localScale = new Vector3(0.42f, 0.42f, 0.12f);
            Physics.SyncTransforms();
            return blocker;
        }

        private static void AssertHeldBuildKitStateUnchanged(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection motherboard,
            long inventoryRevision,
            long buildKitRevision,
            long assemblyRevision,
            int assemblyReceiptCount,
            Vector3 heldLocalPosition,
            Quaternion heldLocalRotation)
        {
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(motherboard.IsCarried, Is.True);
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.MotherboardBuildKit.StagedComponentCount, Is.Zero);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(Vector3.Distance(
                motherboard.transform.localPosition,
                heldLocalPosition), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(
                motherboard.transform.localRotation,
                heldLocalRotation), Is.LessThan(0.001f));
        }

        private static IEnumerator WaitForCustomerState(
            GarageCustomerFlowRuntime customerFlow,
            PCShopEmpire3D.Actors.CustomerVisitState expectedState)
        {
            const int MaximumFixedSteps = 650;
            for (int step = 0; step < MaximumFixedSteps; step++)
            {
                if (customerFlow.CurrentVisit?.State == expectedState)
                {
                    yield break;
                }

                yield return new WaitForFixedUpdate();
            }

            Assert.Fail($"Customer did not reach {expectedState}.");
        }
    }
}
