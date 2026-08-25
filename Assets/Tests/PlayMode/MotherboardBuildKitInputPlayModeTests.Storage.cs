using System.Collections;
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
        public IEnumerator StoragePickupRequiresAllThreeStagedPrerequisitesWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardAndProcessorForMemoryBuildKit(marker);

            PhysicalItemProjection storage = marker.StorageDevice;
            StorageBuildKitProjection buildKit = marker.StorageBuildKit;
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord itemBefore),
                Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int physicalIdentity = storage.GetInstanceID();
            Transform worldParent = storage.transform.parent;
            Vector3 worldPosition = storage.transform.position;
            Quaternion worldRotation = storage.transform.rotation;

            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.HasMemoryModulePrerequisite, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(2));

            AimPlayerAtItem(marker, storage, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(storage));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing.Code));
            Assert.That(marker.StorageBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.False);
            Assert.That(marker.StorageBinding.IsAuthorityInBuildKit, Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.False);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.TryGetStorageItem(out InventoryItemRecord itemAfter),
                Is.True);
            Assert.That(itemAfter.Id, Is.EqualTo(itemBefore.Id));
            Assert.That(itemAfter.ProductId, Is.EqualTo(itemBefore.ProductId));
            Assert.That(itemAfter.ContainerId, Is.EqualTo(itemBefore.ContainerId));
            Assert.That(storage.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(storage.transform.parent, Is.SameAs(worldParent));
            Assert.That(storage.transform.position, Is.EqualTo(worldPosition));
            Assert.That(storage.transform.rotation, Is.EqualTo(worldRotation));
            Assert.That(storage.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseMovesExactReservedStorageFromThreeToFour()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardProcessorAndMemoryForStorageBuildKit(marker);

            Assert.That(marker.HasStorageBuildKitR38Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot storageLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.StorageDevice);
            Assert.That(storageLine.ItemId, Is.EqualTo(session.StorageItemId));
            Assert.That(session.Inventory.TryGetReservation(
                storageLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(storageLine.ItemId));

            PhysicalItemProjection storage = marker.StorageDevice;
            StorageBuildKitProjection buildKit = marker.StorageBuildKit;
            int physicalIdentity = storage.GetInstanceID();
            string itemIdentity = storage.ItemIdValue;
            int worldLayer = storage.gameObject.layer;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            StorageSlotState storageSlotState = session.AssemblyBuild.StorageSlotState;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup = session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.HasMemoryModulePrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(buildKit.ProgressText.text, Does.Contain("3/10"));

            AimPlayerAtItem(marker, storage, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(storage));
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeStorageBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.StorageInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(storageLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsStorageBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsM2StorageSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentStorageBuildKitStatus,
                Is.EqualTo(StorageBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("3/10 → 4/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentStorageBuildKitStatus,
                Is.EqualTo(StorageBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(buildKit.ProgressText.text, Does.Contain("4/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("NVMe HAZIR"));
            Assert.That(marker.StorageBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(storage.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(storage.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(storage.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(storage.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(storage.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(storage), Is.True);
            Assert.That(Quaternion.Angle(
                storage.transform.rotation,
                buildKit.ResolveSnapPose(1).rotation), Is.LessThanOrEqualTo(0.25f));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeStorageBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.StorageStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(storageLine));
            Assert.That(session.TryGetStorageItem(
                out InventoryItemRecord stagedStorage), Is.True);
            Assert.That(stagedStorage.ContainerId,
                Is.EqualTo(session.StorageBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                storageLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId, Is.EqualTo(stagedStorage.Id));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 2));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(storageSlotState));
            Assert.That(marker.MotherboardBuildKit.IsStaged, Is.True);
            Assert.That(marker.ProcessorBuildKit.IsStaged, Is.True);
            Assert.That(marker.MemoryModuleBuildKit.IsStaged, Is.True);
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator StorageBuildKitSameFramePrimaryRotateDropHasOneConsumer()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardProcessorAndMemoryForStorageBuildKit(marker);

            PhysicalItemProjection storage = marker.StorageDevice;
            StorageBuildKitProjection buildKit = marker.StorageBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(storage));
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.R, Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsStorageBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsM2StorageSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.G);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.StorageBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator StoragePlacementFailureRecoversExactSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardProcessorAndMemoryForStorageBuildKit(marker);

            PhysicalItemProjection storage = marker.StorageDevice;
            StorageBuildKitProjection buildKit = marker.StorageBuildKit;
            int physicalIdentity = storage.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(storage));

            long inventoryInHands = session.Inventory.Revision;
            long buildKitInHands = session.CustomPcBuildKit.Revision;
            OperationResult genericDrop = marker.PlayerCarry.TryDrop();
            Assert.That(genericDrop.IsFailure, Is.True);
            Assert.That(genericDrop.Error.Code,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(storage));
            Assert.That(marker.StorageBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands));

            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetStorageBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentStorageBuildKitStatus,
                Is.EqualTo(StorageBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);
            long assemblyRevision = session.AssemblyBuild.Revision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            FailNextStablePlacement(storage);
            OperationResult placement = marker.PlayerCarry.TryConfirmStorageBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.StorageBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(storage.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(storage.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(storage.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                storage.transform.position,
                expectedPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                storage.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.05f));
            Assert.That(buildKit.MatchesCommittedPlacement(storage), Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryInHands + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands + 1));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(session.AssemblyBuild.StorageSlotState,
                Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageMotherboardProcessorAndMemoryForStorageBuildKit(
            GaragePrototypeMarker marker)
        {
            StageMotherboardAndProcessorForMemoryBuildKit(marker);
            MemoryModuleBuildKitProjection memoryBuildKit =
                marker.MemoryModuleBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.MemoryModule));
            MovePlayerToBuildKit(marker, memoryBuildKit);
            AssertSuccess(marker.PlayerCarry.TrySetMemoryModuleBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentMemoryModuleBuildKitStatus,
                Is.EqualTo(MemoryModuleBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmMemoryModuleBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(memoryBuildKit.IsStaged, Is.True);
            Assert.That(memoryBuildKit.StagedComponentCount, Is.EqualTo(3));
            Assert.That(marker.DimmBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(marker.DimmBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
        }

        private static void MovePlayerToBuildKit(
            GaragePrototypeMarker marker,
            StorageBuildKitProjection buildKit)
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
    }
}
