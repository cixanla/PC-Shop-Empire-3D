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
    /// Real Input System coverage for the r29 ATX PS/2 four-screw flow.
    /// </summary>
    public sealed class PowerSupplyInputPromptPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardMouseCompletesOrientedFourScrewFlowAndRecovery()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection powerSupply = marker.PowerSupply;
            Pose initialPose = new Pose(
                powerSupply.transform.position,
                powerSupply.transform.rotation);
            int physicalIdentity = powerSupply.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);

            AimPlayerAtItem(marker, powerSupply, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem,
                Is.SameAs(powerSupply),
                marker.PlayerCarry.LastFailureCode);
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            AssertKeyboardPrompts(marker);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] ATX PS/2 HİZALI • FAN FİLTREYE • G: oturt • R: 180° döndür • LMB: çık"));
            ReleaseMouse(marker, mouse);

            long rotationAssemblyRevision = session.AssemblyBuild.Revision;
            long rotationInventoryRevision = session.Inventory.Revision;
            int rotationReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-power-supply.orientation-mismatch"));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(rotationAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(rotationInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(rotationReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-power-supply.orientation-mismatch"));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(rotationAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(rotationInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(rotationReceiptCount));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            Assert.That(marker.PowerSupplyBinding.IsSeated, Is.True);
            Assert.That(marker.PowerSupplyBinding.IsRetained, Is.False);
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            Assert.That(marker.PowerSupplyBinding.IsRetained, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("PSU ARKA PLAKA + 4 VİDA KİLİTLİ"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.PowerSupplyRetained.Code));
            ReleaseKeyboard(marker, keyboard);

            PressMouse(marker, mouse);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            ReleaseMouse(marker, mouse);
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecoveredPowerSupply(
                marker,
                session,
                powerSupply,
                initialPose,
                physicalIdentity,
                recovery);
        }

        [UnityTest]
        public IEnumerator GamepadCompletesOrientedFourScrewFlowAndRecovery()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection powerSupply = marker.PowerSupply;
            Pose initialPose = new Pose(
                powerSupply.transform.position,
                powerSupply.transform.rotation);
            int physicalIdentity = powerSupply.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);

            AimPlayerAtItem(marker, powerSupply, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            AssertGamepadPrompts(marker);
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadTrigger(marker, gamepad);
            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] ATX PS/2 HİZALI • FAN FİLTREYE • B: oturt • RB: 180° döndür • RT: çık"));
            ReleaseGamepad(marker, gamepad);

            long rotationAssemblyRevision = session.AssemblyBuild.Revision;
            long rotationInventoryRevision = session.Inventory.Revision;
            int rotationReceiptCount = session.AssemblyBuild.ReceiptCount;
            PressGamepadButton(marker, gamepad, GamepadButton.RightShoulder);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.East);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-power-supply.orientation-mismatch"));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(rotationAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(rotationInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(rotationReceiptCount));
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.RightShoulder);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.East);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadTrigger(marker, gamepad);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo(AssemblyFailures.PowerSupplyRetained.Code));
            ReleaseGamepad(marker, gamepad);

            PressGamepadTrigger(marker, gamepad);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecoveredPowerSupply(
                marker,
                session,
                powerSupply,
                initialPose,
                physicalIdentity,
                recovery);
        }

        [UnityTest]
        public IEnumerator KeyboardClearanceAndGenericBypassesFailWithoutMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection powerSupply = marker.PowerSupply;
            Pose initialPose = new Pose(
                powerSupply.transform.position,
                powerSupply.transform.rotation);
            int physicalIdentity = powerSupply.GetInstanceID();
            marker.PlayerMotor.SetPaused(false);

            AimPlayerAtItem(marker, powerSupply, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            ReleaseKeyboard(marker, keyboard);

            long blockedAssemblyRevision = session.AssemblyBuild.Revision;
            long blockedInventoryRevision = session.Inventory.Revision;
            int blockedReceiptCount = session.AssemblyBuild.ReceiptCount;
            OperationResult genericPlacement =
                marker.PlayerCarry.TryConfirmPlacement();
            Assert.That(genericPlacement.Error.Code,
                Is.EqualTo("placement.profile-unsupported"));
            OperationResult genericCart =
                marker.PlayerCarry.TryLoadHeldItem(marker.TransportCart);
            Assert.That(genericCart.Error.Code,
                Is.EqualTo("cart.load-profile-unsupported"));
            Assert.That(marker.TransportCart.HasCargo, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(blockedAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(blockedInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(blockedReceiptCount));

            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseMouse(marker, mouse);

            Collider[] authoredBlockers =
                marker.PowerSupplyBay.ChassisClearanceBlockers;
            Assert.That(authoredBlockers.Length, Is.EqualTo(4));
            BoxCollider blocker = authoredBlockers[0] as BoxCollider;
            Assert.That(blocker, Is.Not.Null);
            Transform blockerTransform = blocker.transform;
            Vector3 originalBlockerPosition = blockerTransform.position;
            Quaternion originalBlockerRotation = blockerTransform.rotation;
            Vector3 originalBlockerCenter = blocker.center;
            Vector3 originalBlockerSize = blocker.size;
            Pose seatPose = marker.PowerSupplyBay.ResolveSeatPose(0).Value;
            blockerTransform.SetPositionAndRotation(
                seatPose.position +
                seatPose.rotation * new Vector3(0.055f, 0.022f, 0f),
                seatPose.rotation);
            blocker.center = Vector3.zero;
            blocker.size = new Vector3(0.026f, 0.026f, 0.026f);
            Physics.SyncTransforms();

            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ChassisClearanceBlocked),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-power-supply.chassis-clearance-blocked"));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(blockedAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(blockedInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(blockedReceiptCount));
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            ReleaseKeyboard(marker, keyboard);

            blockerTransform.SetPositionAndRotation(
                originalBlockerPosition,
                originalBlockerRotation);
            blocker.center = originalBlockerCenter;
            blocker.size = originalBlockerSize;
            Physics.SyncTransforms();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecoveredPowerSupply(
                marker,
                session,
                powerSupply,
                initialPose,
                physicalIdentity,
                recovery);
        }

        [UnityTest]
        public IEnumerator GamepadCoEdgesAndPauseDrainRequireFreshRetentionPress()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            yield return LoadGarage();
            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection powerSupply = marker.PowerSupply;
            marker.PlayerMotor.SetPaused(false);

            AimPlayerAtItem(marker, powerSupply, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            long assemblyRevision = session.AssemblyBuild.Revision;
            long inventoryRevision = session.Inventory.Revision;
            int receiptCount = session.AssemblyBuild.ReceiptCount;
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons = (1u << (int)GamepadButton.RightShoulder) |
                              (1u << (int)GamepadButton.East)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.True);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCount));
            ReleaseGamepad(marker, gamepad);

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.RightShoulder) |
                              (1u << (int)GamepadButton.East)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(assemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(receiptCount));
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.RightShoulder);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBayStatus,
                Is.EqualTo(PowerSupplyBayStatus.ValidSeat),
                marker.PlayerCarry.LastFailureCode);
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.East);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            ReleaseGamepad(marker, gamepad);

            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            long seatedAssemblyRevision = session.AssemblyBuild.Revision;
            long seatedInventoryRevision = session.Inventory.Revision;
            int seatedReceiptCount = session.AssemblyBuild.ReceiptCount;
            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    rightTrigger = 1f,
                    buttons = 1u << (int)GamepadButton.Start
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(seatedAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(seatedInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount));

            marker.PlayerMotor.SetPaused(false);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(seatedAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(seatedInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount));

            ReleaseGamepad(marker, gamepad);
            MovePlayerToFocus(marker, marker.PowerSupplyBay.FocusCollider);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadTrigger(marker, gamepad);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(seatedAssemblyRevision + 1));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(seatedInventoryRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(seatedReceiptCount + 1));
            Assert.That(marker.PowerSupplyBinding.ValidateProjectionInvariant()
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

        private static void AssertRecoveredPowerSupply(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection powerSupply,
            Pose initialPose,
            int physicalIdentity,
            OperationResult recovery)
        {
            AssertSuccess(recovery);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(Vector3.Distance(
                powerSupply.transform.position,
                initialPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                powerSupply.transform.rotation,
                initialPose.rotation), Is.LessThan(0.05f));
            Assert.That(powerSupply.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(powerSupply.IsStablePlacement, Is.True);
            Assert.That(marker.PowerSupplyBinding.IsAuthorityLooseWorld, Is.True);
            Assert.That(session.AssemblyBuild.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(session.TryGetPowerSupplyItem(
                out InventoryItemRecord recoveredItem), Is.True);
            Assert.That(recoveredItem.Id, Is.EqualTo(session.PowerSupplyItemId));
            Assert.That(recoveredItem.ProductId,
                Is.EqualTo(session.PowerSupplyProductId));
            Assert.That(recoveredItem.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));
            Assert.That(marker.PowerSupplyBinding.ValidateProjectionInvariant()
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
