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
    /// Real Input System coverage for the r31 EPS12V/CPU 8-pin route.
    /// </summary>
    public sealed class Eps12vPowerCableInputPromptPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardMouseCompletesKeyedRouteUnrouteAndRecovery()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            yield return LoadGarage();

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GaragePrototypeHud hud =
                Object.FindFirstObjectByType<GaragePrototypeHud>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection cable = marker.Eps12vPowerCable;
            Pose initialPose = new Pose(
                cable.transform.position,
                cable.transform.rotation);
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            marker.PlayerMotor.SetPaused(false);
            PrepareRetainedHosts(marker, session);

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("2 ANAHTARLI 8-PIN"));
            Assert.That(hud.UsesCompactAssemblyUi, Is.True);
            Assert.That(hud.EffectivePromptText,
                Is.EqualTo(marker.PlayerCarry.PromptText));
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "LMB: EPS12V rota önizlemesi • G: güvenli bırak • " +
                    "PSU CPU 8 → ANAKART CPU 8"));
            ReleaseKeyboard(marker, keyboard);

            long bypassAssemblyRevision = session.AssemblyBuild.Revision;
            long bypassInventoryRevision = session.Inventory.Revision;
            long bypassCableRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int bypassCableReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            OperationResult genericPlacement =
                marker.PlayerCarry.TryConfirmPlacement();
            OperationResult genericCart =
                marker.PlayerCarry.TryLoadHeldItem(marker.TransportCart);
            Assert.That(genericPlacement.Error.Code,
                Is.EqualTo("placement.profile-unsupported"));
            Assert.That(genericCart.Error.Code,
                Is.EqualTo("cart.load-profile-unsupported"));
            Assert.That(marker.TransportCart.HasCargo, Is.False);
            AssertCableAuthorityUnchanged(
                session,
                bypassAssemblyRevision,
                bypassInventoryRevision,
                bypassCableRevision,
                bypassCableReceiptCount);

            MovePlayerToRouteFocus(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeRouteOverlaps(marker) + " " +
                DescribeLineOfSight(marker));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            AssertKeyboardPrompts(marker);
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] EPS12V ROTA AÇIK • ANAHTAR HİZALI • " +
                    "G: yönlendir • R: konektörü çevir • LMB: çık"));
            ReleaseMouse(marker, mouse);

            long wrongAssemblyRevision = session.AssemblyBuild.Revision;
            long wrongInventoryRevision = session.Inventory.Revision;
            long wrongCableRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int wrongCableReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.PlacementValid, Is.False);
            ReleaseKeyboard(marker, keyboard);
            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.LastFailureCode,
                Is.EqualTo("assembly-eps12v-cable.orientation-mismatch"));
            AssertCableAuthorityUnchanged(
                session,
                wrongAssemblyRevision,
                wrongInventoryRevision,
                wrongCableRevision,
                wrongCableReceiptCount);
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                $"{marker.PlayerCarry.LastFailureCode} " +
                DescribeLineOfSight(marker));
            ReleaseKeyboard(marker, keyboard);
            PressKeyboard(marker, keyboard, Key.G);
            AssertRouted(marker, session, cable, physicalIdentity, stableItemId);
            ReleaseKeyboard(marker, keyboard);

            MovePlayerToRouteFocus(marker);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.PromptText, Does.Contain("çöz"));
            Assert.That(hud.UsesCompactAssemblyUi, Is.True);
            Assert.That(hud.EffectivePromptText,
                Is.EqualTo(marker.PlayerCarry.PromptText));
            PressKeyboard(marker, keyboard, Key.E);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Loose));

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecovered(
                marker,
                session,
                cable,
                initialPose,
                physicalIdentity,
                stableItemId,
                recovery);
        }

        [UnityTest]
        public IEnumerator GamepadCoEdgesAndPauseDrainRequireFreshRoutePress()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            yield return LoadGarage();

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            PhysicalItemProjection cable = marker.Eps12vPowerCable;
            Pose initialPose = new Pose(
                cable.transform.position,
                cable.transform.rotation);
            int physicalIdentity = cable.GetInstanceID();
            string stableItemId = cable.ItemIdValue;
            marker.PlayerMotor.SetPaused(false);
            PrepareRetainedHosts(marker, session);

            AimPlayerAtItem(marker, cable, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            AssertGamepadPrompts(marker);
            ReleaseGamepad(marker, gamepad);

            MovePlayerToRouteFocus(marker);
            marker.PlayerCarry.ProcessInputFrame();
            long initialAssemblyRevision = session.AssemblyBuild.Revision;
            long initialInventoryRevision = session.Inventory.Revision;
            long initialCableRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int initialCableReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
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

            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.PromptText,
                Is.EqualTo(
                    "[OK] EPS12V ROTA AÇIK • ANAHTAR HİZALI • " +
                    "B: yönlendir • RB: konektörü çevir • RT: çık"));
            AssertCableAuthorityUnchanged(
                session,
                initialAssemblyRevision,
                initialInventoryRevision,
                initialCableRevision,
                initialCableReceiptCount);
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
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.OrientationInvalid));
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            AssertCableAuthorityUnchanged(
                session,
                initialAssemblyRevision,
                initialInventoryRevision,
                initialCableRevision,
                initialCableReceiptCount);
            ReleaseGamepad(marker, gamepad);

            PressGamepadButton(marker, gamepad, GamepadButton.RightShoulder);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                marker.PlayerCarry.LastFailureCode);
            ReleaseGamepad(marker, gamepad);

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.East) |
                              (1u << (int)GamepadButton.Start)
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            AssertCableAuthorityUnchanged(
                session,
                initialAssemblyRevision,
                initialInventoryRevision,
                initialCableRevision,
                initialCableReceiptCount);

            marker.PlayerMotor.SetPaused(false);
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.False);
            AssertCableAuthorityUnchanged(
                session,
                initialAssemblyRevision,
                initialInventoryRevision,
                initialCableRevision,
                initialCableReceiptCount);
            ReleaseGamepad(marker, gamepad);

            MovePlayerToRouteFocus(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadPrimary(marker, gamepad);
            Assert.That(marker.PlayerCarry.IsEps12vPowerCableRouteMode, Is.True);
            Assert.That(marker.PlayerCarry.CurrentEps12vPowerCableRouteStatus,
                Is.EqualTo(Eps12vPowerCableRouteStatus.ValidRoute),
                marker.PlayerCarry.LastFailureCode);
            ReleaseGamepad(marker, gamepad);
            PressGamepadButton(marker, gamepad, GamepadButton.East);
            AssertRouted(marker, session, cable, physicalIdentity, stableItemId);
            ReleaseGamepad(marker, gamepad);

            MovePlayerToRouteFocus(marker);
            marker.PlayerCarry.ProcessInputFrame();
            PressGamepadButton(marker, gamepad, GamepadButton.South);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(cable));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityInHands, Is.True);

            OperationResult recovery = marker.PlayerCarry.TryRecoverHeldItem();
            AssertRecovered(
                marker,
                session,
                cable,
                initialPose,
                physicalIdentity,
                stableItemId,
                recovery);
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

        private static void PrepareRetainedHosts(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session)
        {
            StableId<AssemblyOperationIdScope> motherboardAttach =
                OperationId("motherboard-attach");
            StableId<AssemblyOperationIdScope> motherboardSecure =
                OperationId("motherboard-secure");
            StableId<AssemblyOperationIdScope> powerSupplySeat =
                OperationId("power-supply-seat");
            StableId<AssemblyOperationIdScope> powerSupplyRetain =
                OperationId("power-supply-retain");

            AssertSuccess(session.PickupLooseMotherboardToHands());
            Assert.That(session.AttachMotherboard(motherboardAttach).IsSuccess,
                Is.True);
            Assert.That(session.SecureMotherboardFastener(
                    motherboardSecure,
                    motherboardAttach,
                    session.AssemblyBuild.Revision).IsSuccess,
                Is.True);
            AssertSuccess(session.PickupLoosePowerSupplyToHands());
            OperationResult<AssemblyOperationReceipt> seat =
                session.SeatPowerSupply(
                    powerSupplySeat,
                    PowerSupplyMountOrientation.FanToFilteredVent,
                    session.AssemblyBuild.Revision);
            Assert.That(seat.IsSuccess, Is.True,
                seat.IsFailure ? seat.Error.Code : string.Empty);
            OperationResult<AssemblyOperationReceipt> retain =
                session.RetainPowerSupply(
                    powerSupplyRetain,
                    powerSupplySeat,
                    session.AssemblyBuild.Revision);
            Assert.That(retain.IsSuccess, Is.True,
                retain.IsFailure ? retain.Error.Code : string.Empty);

            AssertSuccess(marker.MotherboardBinding.PhysicalItem
                .SynchronizeStableWorldPose(marker.MotherboardSeat.SnapPose));
            marker.MotherboardFastener.ApplyAuthoritativeState(
                AssemblySeatState.SeatedSecured);
            AssertSuccess(marker.PowerSupplyBinding.SyncProjectionToAuthority());
            Physics.SyncTransforms();
            Assert.That(marker.MotherboardBinding.IsSecured, Is.True);
            Assert.That(marker.PowerSupplyBinding.IsRetained, Is.True);

            AssertSuccess(marker.PlayerCarry.TryPickup(marker.Processor));
            MovePlayerToFocus(marker, marker.ProcessorSocket.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TrySetProcessorSeatMode(true));
            AssertSuccess(marker.PlayerCarry.TryConfirmProcessorSeat());
            MovePlayerToFocus(marker, marker.ProcessorSocket.FocusCollider);
            AssertSuccess(marker.PlayerCarry.TryOperateProcessorRetention());
            Assert.That(session.AssemblyBuild.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(marker.ProcessorBinding.IsRetained, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertRouted(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection cable,
            int physicalIdentity,
            string stableItemId)
        {
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.True);
            Assert.That(marker.Eps12vPowerCableGeometry.IsRouted, Is.True);
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(Eps12vPowerCableState.Routed));
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(session.TryGetEps12vPowerCableItem(
                out InventoryItemRecord routed), Is.True);
            Assert.That(routed.Id, Is.EqualTo(session.Eps12vPowerCableItemId));
            Assert.That(routed.ContainerId,
                Is.EqualTo(session.Eps12vPowerCableRouteContainerId));
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
        }

        private static void AssertRecovered(
            GaragePrototypeMarker marker,
            GarageStockFlowSession session,
            PhysicalItemProjection cable,
            Pose initialPose,
            int physicalIdentity,
            string stableItemId,
            OperationResult recovery)
        {
            AssertSuccess(recovery);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(cable.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(cable.ItemIdValue, Is.EqualTo(stableItemId));
            Assert.That(Vector3.Distance(
                cable.transform.position,
                initialPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                cable.transform.rotation,
                initialPose.rotation), Is.LessThan(0.05f));
            Assert.That(cable.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(marker.Eps12vPowerCableBinding.IsAuthorityLooseWorld,
                Is.True);
            Assert.That(marker.Eps12vPowerCableBinding.IsRouted, Is.False);
            Assert.That(session.TryGetEps12vPowerCableItem(
                out InventoryItemRecord recovered), Is.True);
            Assert.That(recovered.Id, Is.EqualTo(session.Eps12vPowerCableItemId));
            Assert.That(recovered.ProductId,
                Is.EqualTo(session.Eps12vPowerCableProductId));
            Assert.That(recovered.ContainerId,
                Is.EqualTo(session.WorldFloorContainerId));
            Assert.That(marker.Eps12vPowerCableBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertCableAuthorityUnchanged(
            GarageStockFlowSession session,
            long expectedAssemblyRevision,
            long expectedInventoryRevision,
            long expectedCableRevision,
            int expectedCableReceiptCount)
        {
            Assert.That(session.AssemblyBuild.Revision,
                Is.EqualTo(expectedAssemblyRevision));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(expectedInventoryRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(expectedCableRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(expectedCableReceiptCount));
        }

        private static StableId<AssemblyOperationIdScope> OperationId(string suffix)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.playmode.eps12v-power-cable-{suffix}");
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

        private static void PressGamepadPrimary(
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

        private static void MovePlayerToRouteFocus(GaragePrototypeMarker marker)
        {
            Vector3 target = marker.Eps12vPowerCableRoute.FocusCollider.bounds.center;
            SetPlayerLook(marker, new Vector3(-0.72f, 0.05f, 3.25f), target);
        }

        private static void MovePlayerToFocus(
            GaragePrototypeMarker marker,
            Collider focus)
        {
            Vector3 target = focus.bounds.center;
            SetPlayerLook(marker, new Vector3(-0.95f, 0.05f, 3.15f), target);
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

        private static string DescribeRouteOverlaps(GaragePrototypeMarker marker)
        {
            Eps12vPowerCableRouteProjection route = marker.Eps12vPowerCableRoute;
            Vector3[] points =
            {
                route.PsuEndpoint.position,
                route.Waypoints[0].position,
                route.Waypoints[1].position,
                route.Waypoints[2].position,
                route.MotherboardEndpoint.position
            };
            int[] pairs = { 0, 1, 1, 2, 2, 3, 3, 4 };
            var descriptions = new System.Collections.Generic.List<string>();
            for (int index = 0; index < points.Length; index++)
            {
                descriptions.Add($"p{index}={points[index]:F3}");
            }

            for (int pair = 0; pair < pairs.Length; pair += 2)
            {
                foreach (Collider collider in Physics.OverlapCapsule(
                             points[pairs[pair]],
                             points[pairs[pair + 1]],
                             0.0075f,
                             ~0,
                             QueryTriggerInteraction.Ignore))
                {
                    descriptions.Add(
                        $"s{pair / 2}:{collider.name}@{collider.transform.position:F3}");
                }
            }

            return string.Join(",", descriptions);
        }

        private static string DescribeLineOfSight(GaragePrototypeMarker marker)
        {
            PhysicalInteractionResolver resolver =
                Object.FindFirstObjectByType<PhysicalInteractionResolver>();
            if (resolver == null || resolver.Origin == null)
            {
                return "los=resolver-missing";
            }

            Vector3 origin = resolver.Origin.position;
            Vector3 target = marker.Eps12vPowerCableRoute.FocusCollider.bounds.center;
            Vector3 offset = target - origin;
            float distance = offset.magnitude;
            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                offset.normalized,
                distance + 0.03f,
                ~0,
                QueryTriggerInteraction.Collide);
            var descriptions = new System.Collections.Generic.List<string>
            {
                $"los-origin={origin:F3}",
                $"los-target={target:F3}"
            };
            foreach (RaycastHit hit in hits)
            {
                Collider collider = hit.collider;
                descriptions.Add(
                    $"{collider.name}@{hit.distance:F3}" +
                    $"/layer={collider.gameObject.layer}" +
                    $"/trigger={collider.isTrigger}");
            }

            return string.Join(",", descriptions);
        }

    }
}
