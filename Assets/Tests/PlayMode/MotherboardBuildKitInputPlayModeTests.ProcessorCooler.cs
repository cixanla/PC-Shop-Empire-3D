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
        public IEnumerator ProcessorCoolerPickupRequiresAllFirstFourStagedWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageMotherboardProcessorAndMemoryForStorageBuildKit(marker);

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            ProcessorCoolerBuildKitProjection buildKit =
                marker.ProcessorCoolerBuildKit;
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord itemBefore), Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            int physicalIdentity = cooler.GetInstanceID();
            Transform worldParent = cooler.transform.parent;
            Vector3 worldPosition = cooler.transform.position;
            Quaternion worldRotation = cooler.transform.rotation;

            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.HasMemoryModulePrerequisite, Is.True);
            Assert.That(buildKit.HasStoragePrerequisite, Is.False);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(3));

            AimPlayerAtItem(marker, cooler, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cooler));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing.Code));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityLooseWorld,
                Is.True);
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands,
                Is.False);
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInBuildKit,
                Is.False);
            Assert.That(buildKit.HasPickupReceipt, Is.False);
            Assert.That(buildKit.IsStaged, Is.False);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord itemAfter), Is.True);
            Assert.That(itemAfter.Id, Is.EqualTo(itemBefore.Id));
            Assert.That(itemAfter.ProductId, Is.EqualTo(itemBefore.ProductId));
            Assert.That(itemAfter.ContainerId, Is.EqualTo(itemBefore.ContainerId));
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cooler.transform.parent, Is.SameAs(worldParent));
            Assert.That(cooler.transform.position, Is.EqualTo(worldPosition));
            Assert.That(cooler.transform.rotation, Is.EqualTo(worldRotation));
            Assert.That(cooler.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.ProcessorCoolerBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseMovesExactReservedProcessorCoolerFromFourToFive()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFourForProcessorCoolerBuildKit(marker);

            Assert.That(marker.HasProcessorCoolerBuildKitR39Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot coolerLine = workOrder.Lines.Single(
                line => line.ComponentKind == PcComponentKind.ProcessorCooler);
            Assert.That(coolerLine.ItemId,
                Is.EqualTo(session.ProcessorCoolerItemId));
            Assert.That(session.Inventory.TryGetReservation(
                coolerLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(coolerLine.ItemId));

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            ProcessorCoolerBuildKitProjection buildKit =
                marker.ProcessorCoolerBuildKit;
            int physicalIdentity = cooler.GetInstanceID();
            string itemIdentity = cooler.ItemIdValue;
            int worldLayer = cooler.gameObject.layer;
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup = session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.HasMemoryModulePrerequisite, Is.True);
            Assert.That(buildKit.HasStoragePrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(buildKit.ProgressText.text, Does.Contain("4/10"));

            AimPlayerAtItem(marker, cooler, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cooler));
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorCoolerBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorCoolerInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(coolerLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsProcessorCoolerBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorCoolerSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerBuildKitStatus,
                Is.EqualTo(ProcessorCoolerBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("4/10 → 5/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerBuildKitStatus,
                Is.EqualTo(ProcessorCoolerBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(buildKit.ProgressText.text, Does.Contain("5/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("SOĞUTUCU HAZIR"));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cooler.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(cooler.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(cooler.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cooler.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(cooler), Is.True);
            Assert.That(Quaternion.Angle(
                cooler.transform.rotation,
                buildKit.ResolveSnapPose(1).rotation), Is.LessThanOrEqualTo(0.25f));
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypeProcessorCoolerBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.ProcessorCoolerStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(coolerLine));
            Assert.That(session.TryGetProcessorCoolerItem(
                out InventoryItemRecord stagedCooler), Is.True);
            Assert.That(stagedCooler.ContainerId,
                Is.EqualTo(session.ProcessorCoolerBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                coolerLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId, Is.EqualTo(stagedCooler.Id));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 2));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);
            Assert.That(marker.MotherboardBuildKit.IsStaged, Is.True);
            Assert.That(marker.ProcessorBuildKit.IsStaged, Is.True);
            Assert.That(marker.MemoryModuleBuildKit.IsStaged, Is.True);
            Assert.That(marker.StorageBuildKit.IsStaged, Is.True);
            Assert.That(marker.ProcessorCoolerBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator ProcessorCoolerBuildKitRotationCyclesFourPosesAndResets()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFourForProcessorCoolerBuildKit(marker);

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            ProcessorCoolerBuildKitProjection buildKit =
                marker.ProcessorCoolerBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(cooler));
            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerBuildKitMode(true));
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;

            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
            Assert.That(Vector3.Distance(
                marker.PlayerCarry.PlacementPreview.CurrentPose.position,
                buildKit.ResolveSnapPose(0).position),
                Is.LessThanOrEqualTo(0.0005f));
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(0).rotation), Is.LessThanOrEqualTo(0.25f));

            for (int expectedTurn = 1; expectedTurn <= 4; expectedTurn++)
            {
                PressKeyboard(marker, keyboard, Key.R);
                int normalizedTurn = expectedTurn % 4;
                Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                    Is.EqualTo(normalizedTurn));
                Assert.That(marker.PlayerCarry.CurrentProcessorCoolerBuildKitStatus,
                    Is.EqualTo(ProcessorCoolerBuildKitStatus.Valid),
                    marker.PlayerCarry.LastFailureCode);
                Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
                Assert.That(Vector3.Distance(
                    marker.PlayerCarry.PlacementPreview.CurrentPose.position,
                    buildKit.ResolveSnapPose(normalizedTurn).position),
                    Is.LessThanOrEqualTo(0.0005f));
                Assert.That(Quaternion.Angle(
                    marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                    buildKit.ResolveSnapPose(normalizedTurn).rotation),
                    Is.LessThanOrEqualTo(0.25f));
                ReleaseKeyboard(marker, keyboard);
            }

            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerBuildKitMode(false));
            Assert.That(marker.PlayerCarry.IsProcessorCoolerBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerBuildKitMode(true));
            Assert.That(marker.PlayerCarry.IsProcessorCoolerBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorCoolerSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
            Assert.That(Vector3.Distance(
                marker.PlayerCarry.PlacementPreview.CurrentPose.position,
                buildKit.ResolveSnapPose(0).position),
                Is.LessThanOrEqualTo(0.0005f));
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(0).rotation), Is.LessThanOrEqualTo(0.25f));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);
            Assert.That(marker.ProcessorCoolerBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadProcessorCoolerPauseCoEdgesRequireFreshRotateAndDrop()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFourForProcessorCoolerBuildKit(marker);

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            ProcessorCoolerBuildKitProjection buildKit =
                marker.ProcessorCoolerBuildKit;
            int physicalIdentity = cooler.GetInstanceID();
            AimPlayerAtItem(marker, cooler, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt, Is.EqualTo("RT"));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons =
                        (1u << (int)GamepadButton.RightShoulder) |
                        (1u << (int)GamepadButton.East)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.IsProcessorCoolerBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorCoolerSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.Start) |
                              (1u << (int)GamepadButton.RightShoulder) |
                              (1u << (int)GamepadButton.East)
                });
            yield return null;
            yield return null;

            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.RightShoulder
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
            Assert.That(Vector3.Distance(
                marker.PlayerCarry.PlacementPreview.CurrentPose.position,
                buildKit.ResolveSnapPose(1).position),
                Is.LessThanOrEqualTo(0.0005f));
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(1).rotation),
                Is.LessThanOrEqualTo(0.25f));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.East });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);
            Assert.That(marker.ProcessorCoolerBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator ProcessorCoolerBuildKitSameFramePrimaryRotateDropHasOneConsumer()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFourForProcessorCoolerBuildKit(marker);

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            ProcessorCoolerBuildKitProjection buildKit =
                marker.ProcessorCoolerBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(cooler));
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            InputSystem.QueueStateEvent(mouse, new MouseState { buttons = 1 });
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.R, Key.G));
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsProcessorCoolerBuildKitMode,
                Is.True);
            Assert.That(marker.PlayerCarry.IsProcessorCoolerSeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);

            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.G);

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);
            Assert.That(marker.ProcessorCoolerBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator ProcessorCoolerPlacementFailureRecoversSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstFourForProcessorCoolerBuildKit(marker);

            PhysicalItemProjection cooler = marker.ProcessorCooler;
            ProcessorCoolerBuildKitProjection buildKit =
                marker.ProcessorCoolerBuildKit;
            int physicalIdentity = cooler.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(cooler));

            long inventoryInHands = session.Inventory.Revision;
            long buildKitInHands = session.CustomPcBuildKit.Revision;
            OperationResult genericDrop = marker.PlayerCarry.TryDrop();
            Assert.That(genericDrop.IsFailure, Is.True);
            Assert.That(genericDrop.Error.Code,
                Is.EqualTo(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict.Code));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cooler));
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInHands,
                Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands));

            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetProcessorCoolerBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerBuildKitStatus,
                Is.EqualTo(ProcessorCoolerBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            FailNextStablePlacement(cooler);
            OperationResult placement =
                marker.PlayerCarry.TryConfirmProcessorCoolerBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.ProcessorCoolerBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(5));
            Assert.That(cooler.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cooler.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(cooler.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                cooler.transform.position,
                expectedPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                cooler.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.05f));
            Assert.That(buildKit.MatchesCommittedPlacement(cooler), Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryInHands + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands + 1));
            AssertProcessorCoolerAssemblyUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount);
            Assert.That(marker.ProcessorCoolerBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageFirstFourForProcessorCoolerBuildKit(
            GaragePrototypeMarker marker)
        {
            StageMotherboardProcessorAndMemoryForStorageBuildKit(marker);
            StorageBuildKitProjection storageBuildKit = marker.StorageBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.StorageDevice));
            MovePlayerToBuildKit(marker, storageBuildKit);
            AssertSuccess(marker.PlayerCarry.TrySetStorageBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentStorageBuildKitStatus,
                Is.EqualTo(StorageBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmStorageBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(storageBuildKit.IsStaged, Is.True);
            Assert.That(storageBuildKit.StagedComponentCount, Is.EqualTo(4));
            Assert.That(marker.StorageBinding.IsAuthorityInBuildKit, Is.True);
            Assert.That(marker.StorageBinding.ValidateProjectionInvariant().IsSuccess,
                Is.True);
        }

        private static void MovePlayerToBuildKit(
            GaragePrototypeMarker marker,
            ProcessorCoolerBuildKitProjection buildKit)
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

        private static void AssertProcessorCoolerAssemblyUnchanged(
            GarageStockFlowSession session,
            AssemblyBuildSnapshot expected,
            int expectedReceiptCount)
        {
            AssemblyBuildSnapshot actual = session.AssemblyBuild.GetSnapshot();
            Assert.That(actual.Revision, Is.EqualTo(expected.Revision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(expectedReceiptCount));
            Assert.That(actual.ProcessorCoolerSlotState,
                Is.EqualTo(expected.ProcessorCoolerSlotState));
            Assert.That(actual.ProcessorCoolerTimState,
                Is.EqualTo(expected.ProcessorCoolerTimState));
            Assert.That(actual.ProcessorCoolerItemId,
                Is.EqualTo(expected.ProcessorCoolerItemId));
            Assert.That(actual.ProcessorCoolerMountOrientation,
                Is.EqualTo(expected.ProcessorCoolerMountOrientation));
            Assert.That(actual.ProcessorCoolerSeatedByOperationId,
                Is.EqualTo(expected.ProcessorCoolerSeatedByOperationId));
            Assert.That(actual.ProcessorCoolerRetainedByOperationId,
                Is.EqualTo(expected.ProcessorCoolerRetainedByOperationId));
        }
    }
}
