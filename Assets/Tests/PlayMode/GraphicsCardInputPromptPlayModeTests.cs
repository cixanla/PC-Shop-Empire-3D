using System.Collections;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
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
    /// Real Input System coverage for the r28 PCIe x16 graphics-card flow.
    /// </summary>
    public sealed class GraphicsCardInputPromptPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardMouseCompletesKeyedSeatRetentionRemovalAndRecovery()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection graphicsCard = marker.GraphicsCard;
            Pose initialPose = new Pose(
                graphicsCard.transform.position,
                graphicsCard.transform.rotation);
            int physicalIdentity = graphicsCard.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);
            PrepareSecuredMotherboardHost(marker);

            AimPlayerAtItem(marker, graphicsCard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem,
                Is.SameAs(graphicsCard),
                marker.PlayerCarry.LastFailureCode);
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInHands, Is.True);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            AssertKeyboardPrompts(marker);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] PCIe x16 ANAHTARI HİZALI • YÖN 0° • G: oturt • R: 180° döndür • LMB: çık"));
            ReleaseMouse(marker, mouse);

            long rotationAssemblyRevision = session.AssemblyBuild.Revision;
            long rotationInventoryRevision = session.Inventory.Revision;
            int rotationReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-graphics-card.orientation-mismatch"));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(rotationAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(rotationInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(rotationReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-graphics-card.orientation-mismatch"));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(rotationAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(rotationInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(rotationReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.Zero);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(
                    GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            Assert.That(marker.GraphicsCardBinding.IsSeated, Is.True);
            Assert.That(marker.GraphicsCardBinding.IsRetained, Is.False);
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("PCIe MANDALI + ARKA BRAKET KİLİTLİ"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.GraphicsCardRetained.Code));
            ReleaseKeyboard(marker, keyboard);

            PressMouse(marker, mouse);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(
                    GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            ReleaseMouse(marker, mouse);
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecoveredGraphicsCard(
                marker,
                session,
                graphicsCard,
                initialPose,
                physicalIdentity,
                recovery);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesKeyedSeatRetentionRemovalAndRecovery()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection graphicsCard = marker.GraphicsCard;
            Pose initialPose = new Pose(
                graphicsCard.transform.position,
                graphicsCard.transform.rotation);
            int physicalIdentity = graphicsCard.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);
            PrepareSecuredMotherboardHost(marker);

            AimPlayerAtItem(marker, graphicsCard, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            AssertGamepadPrompts(marker);
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadTrigger(marker, gamepad);
            Assert.That(marker.PlayerCarry.IsGraphicsCardSeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] PCIe x16 ANAHTARI HİZALI • YÖN 0° • B: oturt • RB: 180° döndür • RT: çık"));
            ReleaseGamepad(marker, gamepad);

            long rotationAssemblyRevision = session.AssemblyBuild.Revision;
            long rotationInventoryRevision = session.Inventory.Revision;
            int rotationReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressGamepadButton(marker, gamepad, GamepadButton.RightShoulder);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.East);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-graphics-card.orientation-mismatch"));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(rotationAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(rotationInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(rotationReceiptCount));
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.RightShoulder);
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.East);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(
                    GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadTrigger(marker, gamepad);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.GraphicsCardRetained.Code));
            ReleaseGamepad(marker, gamepad);

            PressGamepadTrigger(marker, gamepad);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(
                    GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(graphicsCard));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecoveredGraphicsCard(
                marker,
                session,
                graphicsCard,
                initialPose,
                physicalIdentity,
                recovery);
        }

        [UnityTest]
        public IEnumerator CoolerThenGraphicsCardRemainPhysicallyCompatible()
        {
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            marker.PlayerMotor.SetPaused(false);
            PrepareRetainedProcessorHost(marker);

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.ProcessorCooler));
            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerSeatMode(true));
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerSlotStatus,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorCoolerSeat());
            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorCoolerRetention());

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.GraphicsCard));
            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardSeatMode(true));
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmGraphicsCardSeat());
            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());

            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(marker.ProcessorCoolerBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GraphicsCardThenCoolerRemainPhysicallyCompatible()
        {
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            marker.PlayerMotor.SetPaused(false);
            PrepareRetainedProcessorHost(marker);

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.GraphicsCard));
            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardSeatMode(true));
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardSlotStatus,
                Is.EqualTo(GraphicsCardSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmGraphicsCardSeat());
            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.ProcessorCooler));
            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorCoolerSeatMode(true));
            Assert.That(marker.PlayerCarry.CurrentProcessorCoolerSlotStatus,
                Is.EqualTo(ProcessorCoolerSlotStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorCoolerSeat());
            MovePlayerToFocus(marker, marker.ProcessorCoolerSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorCoolerRetention());

            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(session.AssemblyBuild.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(marker.ProcessorCoolerBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator RetainedGraphicsCardBlocksMotherboardDetachWithoutMutation()
        {
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            marker.PlayerMotor.SetPaused(false);
            PrepareSecuredMotherboardHost(marker);

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.GraphicsCard));
            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetGraphicsCardSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmGraphicsCardSeat());
            MovePlayerToFocus(marker, marker.GraphicsCardSlot.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateGraphicsCardRetention());

            AssertSuccess(marker.MotherboardBinding.TryOperateFastener());
            AssertSuccess(marker.GraphicsCardBinding.SyncProjectionToAuthority());
            long assemblyRevision = session.AssemblyBuild.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int receiptCount = session.AssemblyBuild.ReceiptCount;

            OperationResult<AssemblyOperationReceipt> hostDetach =
                session.DetachMotherboard(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "assembly.operation.playmode.gpu-host-detach"));

            Assert.That(hostDetach.IsFailure,
                Is.True,
                hostDetach.IsSuccess ? "unexpected-success" : hostDetach.Error.Code);
            Assert.That(hostDetach.Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardInstalled));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCount));
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static IEnumerator LoadGarage()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;
            Assert.That(Object.FindFirstObjectByType<GaragePrototypeMarker>(),
                Is.Not.Null);
        }

        private static void PrepareSecuredMotherboardHost(
            GaragePrototypeMarker marker)
        {
            AssertSuccess(marker.PlayerCarry.TryPickup(
                marker.MotherboardBinding.PhysicalItem));
            MovePlayerToFocus(marker, marker.MotherboardSeat.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetMotherboardSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmMotherboardSeat());
            MovePlayerToFocus(marker, marker.MotherboardFastener.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateMotherboardFastener());
            AssertSuccess(marker.GraphicsCardBinding.SyncProjectionToAuthority());
            Assert.That(marker.StockFlow.Session.AssemblyBuild.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(marker.GraphicsCardSlot.FocusCollider.enabled, Is.True);
        }

        private static void PrepareRetainedProcessorHost(
            GaragePrototypeMarker marker)
        {
            PrepareSecuredMotherboardHost(marker);
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.Processor));
            MovePlayerToFocus(marker, marker.ProcessorSocket.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorSeat());
            MovePlayerToFocus(marker, marker.ProcessorSocket.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorRetention());
            Assert.That(marker.StockFlow.Session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
        }

        private static void AssertRecoveredGraphicsCard(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection graphicsCard,
            Pose initialPose,
            int physicalIdentity,
            OperationResult recovery)
        {
            AssertSuccess(recovery);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(graphicsCard.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(Vector3.Distance(
                graphicsCard.transform.position,
                initialPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                graphicsCard.transform.rotation,
                initialPose.rotation), Is.LessThan(0.05f));
            Assert.That(graphicsCard.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(graphicsCard.IsStablePlacement, Is.True);
            Assert.That(marker.GraphicsCardBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(session.AssemblyBuild.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            Assert.That(session.TryGetGraphicsCardAssemblyItem(
                out InventoryItemRecord recoveredItem), Is.True);
            Assert.That(recoveredItem.Id,
                Is.EqualTo(session.GraphicsCardAssemblyItemId));
            Assert.That(recoveredItem.ProductId, Is.EqualTo(session.ProductId));
            Assert.That(recoveredItem.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));
            Assert.That(marker.GraphicsCardBinding.ValidateProjectionInvariant()
                .IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertSuccess(OperationResult result)
        {
            Assert.That(result.IsSuccess,
                Is.True,
                result.IsFailure ? result.Error.Code : string.Empty);
        }

        private static void AssertKeyboardPrompts(GaragePrototypeMarker marker)
        {
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("E"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("G"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt,
                Is.EqualTo("R"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt,
                Is.EqualTo("LMB"));
        }

        private static void AssertGamepadPrompts(GaragePrototypeMarker marker)
        {
            Assert.That(marker.PlayerInput.InteractBindingPrompt, Is.EqualTo("A"));
            Assert.That(marker.PlayerInput.DropBindingPrompt, Is.EqualTo("B"));
            Assert.That(marker.PlayerInput.RotatePlacementBindingPrompt,
                Is.EqualTo("RB"));
            Assert.That(marker.PlayerInput.PrimaryBindingPrompt,
                Is.EqualTo("RT"));
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

        private static void PressGamepadButton(
            GaragePrototypeMarker marker,
            Gamepad gamepad,
            GamepadButton button)
        {
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)button });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void PressGamepadTrigger(
            GaragePrototypeMarker marker,
            Gamepad gamepad)
        {
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { rightTrigger = 1f });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void ReleaseGamepad(
            GaragePrototypeMarker marker,
            Gamepad gamepad)
        {
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
        }

        private static void MovePlayerToFocus(
            GaragePrototypeMarker marker,
            Collider focus)
        {
            Vector3 target = focus.bounds.center;
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
            Transform cameraPivot =
                marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }
    }
}
