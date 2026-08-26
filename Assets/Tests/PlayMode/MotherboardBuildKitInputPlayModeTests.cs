using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;
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
    public sealed partial class MotherboardBuildKitInputPlayModeTests : InputTestFixture
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
            Issue68UnrelatedStateSnapshot unrelatedState =
                CaptureIssue68UnrelatedState(session);

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
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeCustomPcBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardInHands));
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
            Assert.That(Quaternion.Angle(
                motherboard.transform.rotation,
                buildKit.ResolveSnapPose(1).rotation), Is.LessThanOrEqualTo(0.25f));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeCustomPcBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
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
            AssertIssue68UnrelatedStateUnchanged(session, unrelatedState);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
        }

        [UnityTest]
        public IEnumerator PhysicalPickupPreflightFailureLeavesDomainAndWorldProjectionUntouched()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            Issue68UnrelatedStateSnapshot unrelatedState =
                CaptureIssue68UnrelatedState(session);

            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;
            Assert.That(session.Inventory.TryGetSerializedItem(
                session.MotherboardItemId,
                out InventoryItemRecord itemBefore), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int instanceId = motherboard.GetInstanceID();
            Transform parent = motherboard.transform.parent;
            Vector3 originalScale = motherboard.transform.localScale;
            motherboard.transform.localScale = Vector3.one * 1.25f;
            Vector3 worldPosition = motherboard.transform.position;
            Quaternion worldRotation = motherboard.transform.rotation;
            Collider collider = motherboard.GetComponentsInChildren<Collider>(true)
                .First(candidate => candidate != null && candidate.enabled);
            bool colliderEnabled = collider.enabled;
            int layer = motherboard.gameObject.layer;
            bool rendererEnabled = motherboard.GetComponentInChildren<Renderer>().enabled;
            Physics.SyncTransforms();

            var result = marker.PlayerCarry.TryPickup(motherboard);

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error.Code, Is.EqualTo("pickup.invalid-scale"));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.MotherboardBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.False);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            AssertIssue68UnrelatedStateUnchanged(session, unrelatedState);
            Assert.That(session.Inventory.TryGetSerializedItem(
                session.MotherboardItemId,
                out InventoryItemRecord itemAfter), Is.True);
            Assert.That(itemAfter.ContainerId, Is.EqualTo(itemBefore.ContainerId));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(instanceId));
            Assert.That(motherboard.transform.parent, Is.EqualTo(parent));
            Assert.That(motherboard.transform.position, Is.EqualTo(worldPosition));
            Assert.That(motherboard.transform.rotation, Is.EqualTo(worldRotation));
            Assert.That(collider.enabled, Is.EqualTo(colliderEnabled));
            Assert.That(motherboard.gameObject.layer, Is.EqualTo(layer));
            Assert.That(motherboard.GetComponentInChildren<Renderer>().enabled,
                Is.EqualTo(rendererEnabled));
            Assert.That(motherboard.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            motherboard.transform.localScale = originalScale;
            Physics.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator ReservedMotherboardInteractGatesLeaveAuthorityAndWorldUntouched()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);

            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            LooseMotherboardStateSnapshot initial =
                CaptureLooseMotherboardState(marker, session, motherboard);
            Vector3 target = motherboard.InteractionCenter;

            Vector3 outOfRangePosition = target + (Vector3.back * 4f);
            outOfRangePosition.y = 0.05f;
            SetPlayerLook(marker, outOfRangePosition, target);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.Null);
            PressKeyboard(marker, keyboard, Key.E);
            AssertLooseMotherboardStateUnchanged(
                marker,
                session,
                motherboard,
                expectedHeldItem: null,
                initial);
            ReleaseKeyboard(marker, keyboard);

            Vector3 focusMissPosition = target + (Vector3.back * 1.25f);
            focusMissPosition.y = 0.05f;
            SetPlayerLook(
                marker,
                focusMissPosition,
                focusMissPosition + Vector3.back);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.Null);
            PressKeyboard(marker, keyboard, Key.E);
            AssertLooseMotherboardStateUnchanged(
                marker,
                session,
                motherboard,
                expectedHeldItem: null,
                initial);
            ReleaseKeyboard(marker, keyboard);

            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(motherboard));
            GameObject blocker = CreateItemLosBlocker(marker, motherboard);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.Null);
            PressKeyboard(marker, keyboard, Key.E);
            AssertLooseMotherboardStateUnchanged(
                marker,
                session,
                motherboard,
                expectedHeldItem: null,
                initial);
            ReleaseKeyboard(marker, keyboard);
            Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();

            PhysicalItemProjection smallBox = Object
                .FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsInactive.Exclude,
                    FindObjectsSortMode.None)
                .Single(item => item.ItemIdValue == "prototype.garage-box-001");
            Assert.That(marker.PlayerCarry.TryPickup(smallBox).IsSuccess, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(smallBox));
            LooseMotherboardStateSnapshot fullHands =
                CaptureLooseMotherboardState(marker, session, motherboard);

            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            AssertLooseMotherboardStateUnchanged(
                marker,
                session,
                motherboard,
                smallBox,
                fullHands);

            OperationResult directFullHands = marker.PlayerCarry.TryPickup(motherboard);
            Assert.That(directFullHands.Error.Code, Is.EqualTo("pickup.slot-occupied"));
            AssertLooseMotherboardStateUnchanged(
                marker,
                session,
                motherboard,
                smallBox,
                fullHands);
            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator DomainCommitPlacementFailureRecoversExactSameMotherboardAtBuildKitPose()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            Issue68UnrelatedStateSnapshot unrelatedState =
                CaptureIssue68UnrelatedState(session);

            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;
            int physicalIdentity = motherboard.GetInstanceID();
            string itemIdentity = motherboard.ItemIdValue;
            Transform worldParent = motherboard.transform.parent;
            int worldLayer = motherboard.gameObject.layer;
            Collider worldCollider = motherboard.GetComponentsInChildren<Collider>(true)
                .First(candidate => candidate != null && candidate.enabled);
            bool worldColliderEnabled = worldCollider.enabled;
            int worldColliderLayer = worldCollider.gameObject.layer;
            bool worldDetectCollisions = motherboard.Body.detectCollisions;
            CollisionDetectionMode worldCollisionMode =
                motherboard.Body.collisionDetectionMode;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeCustomPcBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardInHands));
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseMouse(marker, mouse);

            Pose expectedPose = buildKit.ResolveSnapPose(0);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            FailNextStablePlacement(motherboard);
            PressKeyboard(marker, keyboard, Key.G);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode, Is.Empty);
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(buildKit.MatchesCommittedPlacement(motherboard), Is.True);
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(motherboard.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(motherboard.transform.parent, Is.EqualTo(worldParent));
            Assert.That(motherboard.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(worldCollider.enabled, Is.EqualTo(worldColliderEnabled));
            Assert.That(worldCollider.gameObject.layer,
                Is.EqualTo(worldColliderLayer));
            Assert.That(motherboard.Body.detectCollisions,
                Is.EqualTo(worldDetectCollisions));
            Assert.That(motherboard.Body.collisionDetectionMode,
                Is.EqualTo(worldCollisionMode));
            Assert.That(motherboard.Body.useGravity, Is.False);
            Assert.That(motherboard.Body.isKinematic, Is.True);
            Assert.That(motherboard.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(motherboard.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                motherboard.transform.position,
                expectedPose.position), Is.LessThan(0.0001f));
            Assert.That(Quaternion.Angle(
                motherboard.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.01f));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeCustomPcBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.MotherboardStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            AssertIssue68UnrelatedStateUnchanged(session, unrelatedState);
            Assert.That(Object.FindObjectsByType<PhysicalItemProjection>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .Count(candidate => candidate.ItemIdValue == itemIdentity),
                Is.EqualTo(1));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
            long replayInventoryRevision = session.Inventory.Revision;
            long replayBuildKitRevision = session.CustomPcBuildKit.Revision;
            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(replayInventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(replayBuildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator SameFrameInteractDropAndPrimaryHaveOneBuildKitConsumer()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            Issue68UnrelatedStateSnapshot unrelatedState =
                CaptureIssue68UnrelatedState(session);
            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;

            Assert.That(marker.PlayerCarry.TryPickup(motherboard).IsSuccess, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(buildKit.HasContextualAttention, Is.True);
            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.False);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int physicalIdentity = motherboard.GetInstanceID();

            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E, Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(motherboard.IsCarried, Is.True);
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));

            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision, Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            AssertIssue68UnrelatedStateUnchanged(session, unrelatedState);
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        [UnityTest]
        public IEnumerator GamepadSouthPickupAndPauseCoEdgeRequireReleaseRepress()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);

            PhysicalItemProjection motherboard = marker.MotherboardBinding.PhysicalItem;
            MotherboardBuildKitProjection buildKit = marker.MotherboardBuildKit;
            int physicalIdentity = motherboard.GetInstanceID();
            AimPlayerAtItem(marker, motherboard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsMotherboardBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.Start) |
                              (1u << (int)GamepadButton.East)
                });
            yield return null;
            yield return null;

            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(buildKit.StagedComponentCount, Is.Zero);

            yield return null;
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(motherboard));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(1));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
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

            var genericPlacement =
                marker.PlayerCarry.TryConfirmPlacement();
            Assert.That(genericPlacement.IsFailure, Is.True);
            Assert.That(genericPlacement.Error.Code,
                Is.EqualTo("placement.profile-unsupported"));
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

            var genericCart =
                marker.PlayerCarry.TryLoadHeldItem(marker.TransportCart);
            Assert.That(genericCart.IsFailure, Is.True);
            Assert.That(genericCart.Error.Code,
                Is.EqualTo("cart.load-profile-unsupported"));
            Assert.That(marker.TransportCart.HasCargo, Is.False);
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
            ReleaseKeyboard(marker, keyboard);
            Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();

            GameObject obstruction = CreateBuildKitObstruction(buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.Obstructed),
                marker.PlayerCarry.LastFailureCode);
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
            ReleaseKeyboard(marker, keyboard);
            Object.DestroyImmediate(obstruction);
            Physics.SyncTransforms();

            Vector3 authoredSnapPosition = buildKit.SnapAnchor.position;
            buildKit.SnapAnchor.position += buildKit.SnapAnchor.right * 0.50f;
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentMotherboardBuildKitStatus,
                Is.EqualTo(MotherboardBuildKitStatus.OutsideSurface),
                marker.PlayerCarry.LastFailureCode);
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
            ReleaseKeyboard(marker, keyboard);
            buildKit.SnapAnchor.position = authoredSnapPosition;
            Physics.SyncTransforms();

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
            Camera playerCamera =
                marker.PlayerMotor.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.transform.localRotation = Quaternion.identity;
            }

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

        private static GameObject CreateBuildKitObstruction(
            MotherboardBuildKitProjection buildKit)
        {
            Pose pose = buildKit.ResolveSnapPose(0);
            Collider support = buildKit.SupportCollider;
            GameObject obstruction = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstruction.name = "MotherboardBuildKitFootprintObstruction";
            obstruction.layer = 0;
            obstruction.transform.position = new Vector3(
                pose.position.x + 0.09f,
                support.bounds.max.y + 0.043f,
                pose.position.z);
            obstruction.transform.localScale = new Vector3(0.04f, 0.06f, 0.04f);
            Physics.SyncTransforms();
            return obstruction;
        }

        private static GameObject CreateItemLosBlocker(
            GaragePrototypeMarker marker,
            PhysicalItemProjection item)
        {
            Transform camera = marker.PlayerMotor.GetComponentInChildren<Camera>()
                .transform;
            Vector3 target = item.InteractionCenter;
            Vector3 direction = target - camera.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "MotherboardPickupLosBlocker";
            blocker.layer = 0;
            blocker.transform.SetPositionAndRotation(
                camera.position + (direction * 0.50f),
                Quaternion.LookRotation(direction.normalized, Vector3.up));
            blocker.transform.localScale = new Vector3(0.45f, 0.45f, 0.12f);
            Physics.SyncTransforms();
            return blocker;
        }

        private static void FailNextStablePlacement(
            PhysicalItemProjection item)
        {
            FieldInfo field = typeof(PhysicalItemProjection).GetField(
                "_failNextStablePlacementForTests",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(item, true);
        }

        private static Issue68UnrelatedStateSnapshot CaptureIssue68UnrelatedState(
            GarageStockFlowSession session)
        {
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(
                out CustomPcWorkTicketRecord workTicket), Is.True);

            ReservedLineState[] otherLines = workOrder.Lines
                .Where(line => line.ComponentKind !=
                               PCShopEmpire3D.Catalog.PcComponentKind.Motherboard)
                .Select(line =>
                {
                    Assert.That(session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item), Is.True);
                    Assert.That(session.Inventory.TryGetReservation(
                        line.ReservationId,
                        out InventoryReservation reservation), Is.True);
                    return new ReservedLineState(line, item, reservation);
                })
                .ToArray();
            Assert.That(otherLines.Length, Is.EqualTo(9));

            return new Issue68UnrelatedStateSnapshot
            {
                QuoteRevision = session.CustomPcQuotes.Revision,
                Quote = quote,
                WorkOrderRevision = session.CustomPcWorkOrders.Revision,
                WorkOrder = workOrder,
                WorkTicket = workTicket,
                AssemblyRevision = session.AssemblyBuild.Revision,
                AssemblyReceiptCount = session.AssemblyBuild.ReceiptCount,
                MotherboardSeatState = session.AssemblyBuild.MotherboardSeatState,
                Atx24State = session.AssemblyBuild.Atx24PowerCableState,
                Atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision,
                Atx24ReceiptCount = session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Eps12vState = session.AssemblyBuild.Eps12vPowerCableState,
                Eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision,
                Eps12vReceiptCount = session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                PcieGpuState = session.AssemblyBuild.PcieGpuPowerCableState,
                PcieGpuRevision = session.AssemblyBuild.PcieGpuPowerCableRevision,
                PcieGpuReceiptCount = session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                OtherLines = otherLines
            };
        }

        private static void AssertIssue68UnrelatedStateUnchanged(
            GarageStockFlowSession session,
            Issue68UnrelatedStateSnapshot expected)
        {
            Assert.That(session.CustomPcQuotes.Revision,
                Is.EqualTo(expected.QuoteRevision));
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(quote, Is.SameAs(expected.Quote));
            Assert.That(quote.TotalPrice, Is.EqualTo(expected.Quote.TotalPrice));

            Assert.That(session.CustomPcWorkOrders.Revision,
                Is.EqualTo(expected.WorkOrderRevision));
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            Assert.That(workOrder, Is.SameAs(expected.WorkOrder));
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(
                out CustomPcWorkTicketRecord workTicket), Is.True);
            Assert.That(workTicket, Is.SameAs(expected.WorkTicket));

            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(expected.AssemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(expected.AssemblyReceiptCount));
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(expected.MotherboardSeatState));
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(expected.Atx24State));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(expected.Atx24Revision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(expected.Atx24ReceiptCount));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(expected.Eps12vState));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(expected.Eps12vRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(expected.Eps12vReceiptCount));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(expected.PcieGpuState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(expected.PcieGpuRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(expected.PcieGpuReceiptCount));

            Assert.That(expected.OtherLines.Length, Is.EqualTo(9));
            foreach (ReservedLineState lineState in expected.OtherLines)
            {
                Assert.That(session.Inventory.TryGetSerializedItem(
                    lineState.Line.ItemId,
                    out InventoryItemRecord item), Is.True);
                Assert.That(item.Id, Is.EqualTo(lineState.Item.Id));
                Assert.That(item.ProductId, Is.EqualTo(lineState.Item.ProductId));
                Assert.That(item.ContainerId, Is.EqualTo(lineState.Item.ContainerId));
                Assert.That(item.Condition, Is.EqualTo(lineState.Item.Condition));
                Assert.That(item.UnitCost, Is.EqualTo(lineState.Item.UnitCost));
                Assert.That(item.StateFlags, Is.EqualTo(lineState.Item.StateFlags));

                Assert.That(session.Inventory.TryGetReservation(
                    lineState.Line.ReservationId,
                    out InventoryReservation reservation), Is.True);
                Assert.That(reservation.Id, Is.EqualTo(lineState.Reservation.Id));
                Assert.That(reservation.ClaimId,
                    Is.EqualTo(lineState.Reservation.ClaimId));
                Assert.That(reservation.TargetKind,
                    Is.EqualTo(lineState.Reservation.TargetKind));
                Assert.That(reservation.ItemId,
                    Is.EqualTo(lineState.Reservation.ItemId));
                Assert.That(reservation.BatchId,
                    Is.EqualTo(lineState.Reservation.BatchId));
                Assert.That(reservation.ContainerId,
                    Is.EqualTo(lineState.Reservation.ContainerId));
                Assert.That(reservation.Quantity,
                    Is.EqualTo(lineState.Reservation.Quantity));
                Assert.That(reservation.ReleasePolicy,
                    Is.EqualTo(lineState.Reservation.ReleasePolicy));
            }
        }

        private static LooseMotherboardStateSnapshot CaptureLooseMotherboardState(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection motherboard)
        {
            Assert.That(marker.MotherboardBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(session.Inventory.TryGetSerializedItem(
                session.MotherboardItemId,
                out InventoryItemRecord item), Is.True);
            Collider collider = motherboard.GetComponentsInChildren<Collider>(true)
                .First(candidate => candidate != null && candidate.enabled);
            Renderer renderer = motherboard.GetComponentInChildren<Renderer>(true);
            Assert.That(renderer, Is.Not.Null);

            return new LooseMotherboardStateSnapshot
            {
                InventoryRevision = session.Inventory.Revision,
                BuildKitRevision = session.CustomPcBuildKit.Revision,
                AssemblyRevision = session.AssemblyBuild.Revision,
                AssemblyReceiptCount = session.AssemblyBuild.ReceiptCount,
                Item = item,
                InstanceId = motherboard.GetInstanceID(),
                Parent = motherboard.transform.parent,
                Position = motherboard.transform.position,
                Rotation = motherboard.transform.rotation,
                Layer = motherboard.gameObject.layer,
                Collider = collider,
                ColliderEnabled = collider.enabled,
                ColliderLayer = collider.gameObject.layer,
                Renderer = renderer,
                RendererEnabled = renderer.enabled,
                Unrelated = CaptureIssue68UnrelatedState(session)
            };
        }

        private static void AssertLooseMotherboardStateUnchanged(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection motherboard,
            PhysicalItemProjection expectedHeldItem,
            LooseMotherboardStateSnapshot expected)
        {
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(expectedHeldItem));
            if (expectedHeldItem != null)
            {
                Assert.That(expectedHeldItem.IsCarried, Is.True);
            }

            Assert.That(marker.MotherboardBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(marker.MotherboardBinding.IsAuthorityInHands, Is.False);
            Assert.That(marker.MotherboardBinding.IsAuthorityInBuildKit, Is.False);
            Assert.That(marker.MotherboardBuildKit.HasPickupReceipt, Is.False);
            Assert.That(marker.MotherboardBuildKit.IsStaged, Is.False);
            Assert.That(marker.MotherboardBuildKit.StagedComponentCount, Is.Zero);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(expected.InventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(expected.BuildKitRevision));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(expected.AssemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(expected.AssemblyReceiptCount));
            AssertIssue68UnrelatedStateUnchanged(session, expected.Unrelated);

            Assert.That(session.Inventory.TryGetSerializedItem(
                session.MotherboardItemId,
                out InventoryItemRecord item), Is.True);
            Assert.That(item.Id, Is.EqualTo(expected.Item.Id));
            Assert.That(item.ProductId, Is.EqualTo(expected.Item.ProductId));
            Assert.That(item.ContainerId, Is.EqualTo(expected.Item.ContainerId));
            Assert.That(item.Condition, Is.EqualTo(expected.Item.Condition));
            Assert.That(item.UnitCost, Is.EqualTo(expected.Item.UnitCost));
            Assert.That(item.StateFlags, Is.EqualTo(expected.Item.StateFlags));
            Assert.That(motherboard.GetInstanceID(), Is.EqualTo(expected.InstanceId));
            Assert.That(motherboard.transform.parent, Is.EqualTo(expected.Parent));
            Assert.That(motherboard.transform.position, Is.EqualTo(expected.Position));
            Assert.That(motherboard.transform.rotation, Is.EqualTo(expected.Rotation));
            Assert.That(motherboard.gameObject.layer, Is.EqualTo(expected.Layer));
            Assert.That(expected.Collider.enabled,
                Is.EqualTo(expected.ColliderEnabled));
            Assert.That(expected.Collider.gameObject.layer,
                Is.EqualTo(expected.ColliderLayer));
            Assert.That(expected.Renderer.enabled,
                Is.EqualTo(expected.RendererEnabled));
            Assert.That(motherboard.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.MotherboardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private sealed class LooseMotherboardStateSnapshot
        {
            public long InventoryRevision;
            public long BuildKitRevision;
            public long AssemblyRevision;
            public int AssemblyReceiptCount;
            public InventoryItemRecord Item;
            public int InstanceId;
            public Transform Parent;
            public Vector3 Position;
            public Quaternion Rotation;
            public int Layer;
            public Collider Collider;
            public bool ColliderEnabled;
            public int ColliderLayer;
            public Renderer Renderer;
            public bool RendererEnabled;
            public Issue68UnrelatedStateSnapshot Unrelated;
        }

        private sealed class Issue68UnrelatedStateSnapshot
        {
            public long QuoteRevision;
            public CustomPcQuoteRecord Quote;
            public long WorkOrderRevision;
            public CustomPcBuildOrderRecord WorkOrder;
            public CustomPcWorkTicketRecord WorkTicket;
            public long AssemblyRevision;
            public int AssemblyReceiptCount;
            public AssemblySeatState MotherboardSeatState;
            public Atx24PowerCableState Atx24State;
            public long Atx24Revision;
            public int Atx24ReceiptCount;
            public Eps12vPowerCableState Eps12vState;
            public long Eps12vRevision;
            public int Eps12vReceiptCount;
            public PcieGpuPowerCableState PcieGpuState;
            public long PcieGpuRevision;
            public int PcieGpuReceiptCount;
            public ReservedLineState[] OtherLines;
        }

        private sealed class ReservedLineState
        {
            public ReservedLineState(
                CustomPcBuildOrderLineSnapshot line,
                InventoryItemRecord item,
                InventoryReservation reservation)
            {
                Line = line;
                Item = item;
                Reservation = reservation;
            }

            public CustomPcBuildOrderLineSnapshot Line { get; }

            public InventoryItemRecord Item { get; }

            public InventoryReservation Reservation { get; }
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
