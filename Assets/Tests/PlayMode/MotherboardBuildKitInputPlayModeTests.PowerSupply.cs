using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
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
        public IEnumerator StorageSmokeRunsPhysicalWorkTicketFrameLifecycle()
        {
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            MethodInfo smokeMethod = typeof(GaragePrototypeMarker).GetMethod(
                "RunStorageBuildKitSmoke",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(smokeMethod, Is.Not.Null);
            IEnumerator smoke = smokeMethod.Invoke(marker, null) as IEnumerator;
            Assert.That(smoke, Is.Not.Null);

            LogAssert.Expect(
                LogType.Log,
                GaragePrototypeMarker.StorageBuildKitSmokeSuccessMarker);
            yield return marker.StartCoroutine(smoke);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.TryGetPrototypeCustomPcWorkTicket(out _), Is.True);
            Assert.That(marker.StorageBuildKit.IsStaged, Is.True);
            Assert.That(marker.StorageBuildKit.StagedComponentCount,
                Is.EqualTo(4));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator KeyboardMouseWalksAndMovesExactReservedPowerSupplyFromSixToSeven()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);
            Assert.That(
                GaragePrototypeMarker.PowerSupplyBuildKitSmokeSuccessMarker,
                Does.Contain("prerequisite-positioning=teleport-assisted"));
            Assert.That(
                GaragePrototypeMarker.PowerSupplyBuildKitSmokeSuccessMarker,
                Does.Contain("post-prerequisite-input=keyboard+mouse"));
            Assert.That(
                GaragePrototypeMarker.PowerSupplyBuildKitSmokeSuccessMarker,
                Does.Contain("post-prerequisite-return=authored-spawn"));
            Assert.That(
                GaragePrototypeMarker.PowerSupplyBuildKitSmokeSuccessMarker,
                Does.Contain(
                    "post-prerequisite-route=authored-spawn>power-supply>power-supply-build-kit"));
            Assert.That(
                GaragePrototypeMarker.PowerSupplyBuildKitSmokeSuccessMarker,
                Does.Contain("route-horizontal-step-envelope=bounded"));
            Assert.That(
                GaragePrototypeMarker.PowerSupplyBuildKitSmokeSuccessMarker,
                Does.Not.Contain("human-route=ok"));
            Assert.That(
                GaragePrototypeMarker.PowerSupplyBuildKitSmokeSuccessMarker,
                Does.Not.Contain("no-teleport=ok"));

            Transform player = marker.PlayerMotor.transform;
            Transform authoredPlayerParent = player.parent;
            Vector3 authoredPlayerSpawn = player.position;
            Assert.That(
                Vector3.ProjectOnPlane(
                    authoredPlayerSpawn - new Vector3(0f, 0.05f, -2.5f),
                    Vector3.up).magnitude,
                Is.LessThanOrEqualTo(0.10f),
                "The power-supply route must capture the authored PlayerSpawn.");

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSixForPowerSupplyBuildKit(marker);

            Assert.That(marker.HasPowerSupplyBuildKitR41Runtime, Is.True);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out CustomPcBuildOrderRecord workOrder), Is.True);
            CustomPcBuildOrderLineSnapshot powerSupplyLine =
                workOrder.Lines.Single(
                    line => line.ComponentKind == PcComponentKind.PowerSupply);
            Assert.That(powerSupplyLine.ItemId,
                Is.EqualTo(session.PowerSupplyItemId));
            Assert.That(session.Inventory.TryGetReservation(
                powerSupplyLine.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ItemId, Is.EqualTo(powerSupplyLine.ItemId));
            Assert.That(reservation.ClaimId,
                Is.EqualTo(workOrder.InventoryClaimId));

            PhysicalItemProjection powerSupply =
                marker.PowerSupplyBinding.PhysicalItem;
            PowerSupplyBuildKitProjection buildKit =
                marker.PowerSupplyBuildKit;
            int physicalIdentity = powerSupply.GetInstanceID();
            string itemIdentity = powerSupply.ItemIdValue;
            int worldLayer = powerSupply.gameObject.layer;
            AssemblyBuildSnapshot assemblyBefore =
                session.AssemblyBuild.GetSnapshot();
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;
            Atx24PowerCableState atx24State =
                session.AssemblyBuild.Atx24PowerCableState;
            long atx24Revision =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int atx24ReceiptCount =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
            Eps12vPowerCableState eps12vState =
                session.AssemblyBuild.Eps12vPowerCableState;
            long eps12vRevision =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int eps12vReceiptCount =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            PcieGpuPowerCableState pcieState =
                session.AssemblyBuild.PcieGpuPowerCableState;
            long pcieRevision =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int pcieReceiptCount =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            long inventoryRevisionBeforePickup = session.Inventory.Revision;
            long buildKitRevisionBeforePickup =
                session.CustomPcBuildKit.Revision;

            buildKit.RefreshPresentation();
            Assert.That(buildKit.HasMotherboardPrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorPrerequisite, Is.True);
            Assert.That(buildKit.HasMemoryModulePrerequisite, Is.True);
            Assert.That(buildKit.HasStoragePrerequisite, Is.True);
            Assert.That(buildKit.HasProcessorCoolerPrerequisite, Is.True);
            Assert.That(buildKit.HasGraphicsCardPrerequisite, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(buildKit.ProgressText.text, Does.Contain("6/10"));

            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(2.0f, 0.05f, 2.15f),
                90,
                authoredPlayerParent,
                "return-from-graphics-kit");
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(2.0f, 0.05f, -0.25f),
                90,
                authoredPlayerParent,
                "return-through-east-aisle");
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(2.0f, 0.05f, -2.5f),
                90,
                authoredPlayerParent,
                "return-to-south-aisle");
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                authoredPlayerSpawn,
                65,
                authoredPlayerParent,
                "return-to-authored-spawn");
            Assert.That(
                Vector3.ProjectOnPlane(
                    player.position - authoredPlayerSpawn,
                    Vector3.up).magnitude,
                Is.LessThanOrEqualTo(0.18f),
                "The staged prerequisites must return through real input, not a transform snap.");
            RunPowerSupplyCardinalCalibration(
                marker,
                keyboard,
                mouse,
                authoredPlayerParent);

            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(2.0f, 0.05f, -2.5f),
                65,
                authoredPlayerParent,
                "spawn-to-east-aisle");
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(2.0f, 0.05f, -0.25f),
                90,
                authoredPlayerParent,
                "east-aisle-northbound");
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(1.15f, 0.05f, -0.25f),
                45,
                authoredPlayerParent,
                "east-aisle-crossing");
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(1.15f, 0.05f, 2.15f),
                90,
                authoredPlayerParent,
                "workbench-front-approach");
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                new Vector3(-0.17f, 0.05f, 2.15f),
                55,
                authoredPlayerParent,
                "power-supply-front-crossing");

            Vector3 powerSupplyFocus = powerSupply.InteractionCenter;
            Vector3 powerSupplyApproach =
                powerSupplyFocus - (Vector3.forward * 1.60f);
            powerSupplyApproach.y = marker.PlayerMotor.transform.position.y;
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                powerSupplyApproach,
                90,
                authoredPlayerParent,
                "power-supply-pickup-approach");
            AimPowerSupplyRouteAt(
                marker,
                keyboard,
                mouse,
                powerSupplyFocus,
                authoredPlayerParent,
                "power-supply-pickup-look");
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem,
                Is.SameAs(powerSupply));
            PressKeyboard(marker, keyboard, Key.E);

            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(powerSupply));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            Assert.That(buildKit.HasPickupReceipt, Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypePowerSupplyBuildKitOperationId,
                out CustomPcBuildKitReceipt pickupReceipt), Is.True);
            Assert.That(pickupReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.PowerSupplyInHands));
            Assert.That(pickupReceipt.Line, Is.SameAs(powerSupplyLine));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 1));
            ReleaseKeyboard(marker, keyboard);

            long inventoryRevisionInHands = session.Inventory.Revision;
            long buildKitRevisionInHands = session.CustomPcBuildKit.Revision;
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(powerSupply));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionInHands));

            Collider support = buildKit.SupportCollider;
            Vector3 buildKitFocus = new Vector3(
                support.bounds.center.x,
                support.bounds.max.y,
                support.bounds.center.z);
            Vector3 buildKitApproach =
                buildKitFocus + (Vector3.back * 0.95f);
            buildKitApproach.y = marker.PlayerMotor.transform.position.y;
            DrivePowerSupplyRoutePoint(
                marker,
                keyboard,
                mouse,
                buildKitApproach,
                125,
                authoredPlayerParent,
                "power-supply-build-kit-approach");
            AimPowerSupplyRouteAt(
                marker,
                keyboard,
                mouse,
                buildKitFocus,
                authoredPlayerParent,
                "power-supply-build-kit-look");
            marker.PlayerCarry.ProcessInputFrame();
            PressMouse(marker, mouse);
            Assert.That(marker.PlayerCarry.IsPowerSupplyBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.False);
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBuildKitStatus,
                Is.EqualTo(PowerSupplyBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(marker.PlayerCarry.PlacementValid, Is.True);
            Assert.That(marker.PlayerCarry.PromptText,
                Does.Contain("6/10 → 7/10"));
            ReleaseMouse(marker, mouse);

            PressKeyboard(marker, keyboard, Key.R);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBuildKitStatus,
                Is.EqualTo(PowerSupplyBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Assert.That(Quaternion.Angle(
                marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                buildKit.ResolveSnapPose(1).rotation),
                Is.LessThanOrEqualTo(0.25f));
            ReleaseKeyboard(marker, keyboard);

            PressKeyboard(marker, keyboard, Key.G);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(7));
            Assert.That(buildKit.ProgressText.text, Does.Contain("7/10"));
            Assert.That(buildKit.ProgressText.text, Does.Contain("PSU HAZIR"));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(powerSupply.ItemIdValue, Is.EqualTo(itemIdentity));
            Assert.That(powerSupply.gameObject.layer, Is.EqualTo(worldLayer));
            Assert.That(powerSupply.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(powerSupply.IsStablePlacement, Is.True);
            Assert.That(buildKit.MatchesCommittedPlacement(powerSupply), Is.True);
            Assert.That(session.CustomPcBuildKit.TryGetReceipt(
                session.PrototypePowerSupplyBuildKitOperationId,
                out CustomPcBuildKitReceipt placementReceipt), Is.True);
            Assert.That(placementReceipt.Stage,
                Is.EqualTo(CustomPcBuildKitStage.PowerSupplyStaged));
            Assert.That(placementReceipt, Is.Not.SameAs(pickupReceipt));
            Assert.That(placementReceipt.Line, Is.SameAs(powerSupplyLine));
            Assert.That(session.TryGetPowerSupplyItem(
                out InventoryItemRecord stagedPowerSupply), Is.True);
            Assert.That(stagedPowerSupply.Id,
                Is.EqualTo(powerSupplyLine.ItemId));
            Assert.That(stagedPowerSupply.ProductId,
                Is.EqualTo(powerSupplyLine.ProductId));
            Assert.That(stagedPowerSupply.ContainerId,
                Is.EqualTo(session.PowerSupplyBuildKitContainerId));
            Assert.That(session.Inventory.TryGetReservation(
                powerSupplyLine.ReservationId,
                out InventoryReservation stagedReservation), Is.True);
            Assert.That(stagedReservation.ItemId,
                Is.EqualTo(stagedPowerSupply.Id));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevisionBeforePickup + 2));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevisionBeforePickup + 2));
            AssertPowerSupplyAndCablesUnchanged(
                session,
                assemblyBefore,
                assemblyReceiptCount,
                atx24State,
                atx24Revision,
                atx24ReceiptCount,
                eps12vState,
                eps12vRevision,
                eps12vReceiptCount,
                pcieState,
                pcieRevision,
                pcieReceiptCount);
            Assert.That(marker.PowerSupplyBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            ReleaseKeyboard(marker, keyboard);
        }

        [UnityTest]
        public IEnumerator PowerSupplyBuildKitRotationCyclesTwoPosesAndResets()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSixForPowerSupplyBuildKit(marker);

            PhysicalItemProjection powerSupply =
                marker.PowerSupplyBinding.PhysicalItem;
            PowerSupplyBuildKitProjection buildKit =
                marker.PowerSupplyBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(powerSupply));
            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetPowerSupplyBuildKitMode(true));
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;

            for (int expectedTurn = 1; expectedTurn <= 2; expectedTurn++)
            {
                PressKeyboard(marker, keyboard, Key.R);
                int normalizedTurn = expectedTurn % 2;
                Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns,
                    Is.EqualTo(normalizedTurn));
                Assert.That(marker.PlayerCarry.CurrentPowerSupplyBuildKitStatus,
                    Is.EqualTo(PowerSupplyBuildKitStatus.Valid),
                    marker.PlayerCarry.LastFailureCode);
                Assert.That(Quaternion.Angle(
                    marker.PlayerCarry.PlacementPreview.CurrentPose.rotation,
                    buildKit.ResolveSnapPose(normalizedTurn).rotation),
                    Is.LessThanOrEqualTo(0.25f));
                ReleaseKeyboard(marker, keyboard);
            }

            AssertSuccess(marker.PlayerCarry.TrySetPowerSupplyBuildKitMode(false));
            Assert.That(marker.PlayerCarry.IsPowerSupplyBuildKitMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.False);
            AssertSuccess(marker.PlayerCarry.TrySetPowerSupplyBuildKitMode(true));
            Assert.That(marker.PlayerCarry.IsPowerSupplyBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.PlacementPreview.IsVisible, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(marker.PowerSupplyBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator GamepadPowerSupplyCoEdgesPauseAndReleaseRepressAreDeterministic()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSixForPowerSupplyBuildKit(marker);

            PhysicalItemProjection powerSupply =
                marker.PowerSupplyBinding.PhysicalItem;
            PowerSupplyBuildKitProjection buildKit =
                marker.PowerSupplyBuildKit;
            int physicalIdentity = powerSupply.GetInstanceID();
            AimPlayerAtItem(marker, powerSupply, -Vector3.forward);
            marker.PlayerCarry.ProcessInputFrame();

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.HeldItem,
                Is.SameAs(powerSupply));
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            MovePlayerToBuildKit(marker, buildKit);
            marker.PlayerCarry.ProcessInputFrame();
            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;

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
            Assert.That(marker.PlayerCarry.IsPowerSupplyBuildKitMode, Is.True);
            Assert.That(marker.PlayerCarry.IsPowerSupplySeatMode, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons =
                        (1u << (int)GamepadButton.Start) |
                        (1u << (int)GamepadButton.RightShoulder) |
                        (1u << (int)GamepadButton.East)
                });
            yield return null;
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(marker.PlayerCarry.PlacementRotationQuarterTurns, Is.Zero);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(6));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

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
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.East
                });
            InputSystem.Update();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(7));
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision + 1));
            Assert.That(marker.PowerSupplyBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator PowerSupplyPlacementFailureRecoversSameInstanceAtBuildKit()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            yield return PrepareQuote(marker, keyboard);
            yield return IssuePhysicalWorkTicket(marker, keyboard);
            StageFirstSixForPowerSupplyBuildKit(marker);

            PhysicalItemProjection powerSupply =
                marker.PowerSupplyBinding.PhysicalItem;
            PowerSupplyBuildKitProjection buildKit =
                marker.PowerSupplyBuildKit;
            int physicalIdentity = powerSupply.GetInstanceID();
            AssertSuccess(marker.PlayerCarry.TryPickup(powerSupply));

            long inventoryInHands = session.Inventory.Revision;
            long buildKitInHands = session.CustomPcBuildKit.Revision;
            Assert.That(marker.PlayerCarry.TryDrop().IsFailure, Is.True);
            Assert.That(marker.PlayerCarry.HeldItem, Is.SameAs(powerSupply));
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInHands, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryInHands));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands));

            MovePlayerToBuildKit(marker, buildKit);
            AssertSuccess(marker.PlayerCarry.TrySetPowerSupplyBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentPowerSupplyBuildKitStatus,
                Is.EqualTo(PowerSupplyBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            Pose expectedPose = buildKit.ResolveSnapPose(0);

            FailNextStablePlacement(powerSupply);
            var placement = marker.PlayerCarry.TryConfirmPowerSupplyBuildKit();

            Assert.That(placement.IsSuccess, Is.True,
                placement.IsFailure ? placement.Error.Code : string.Empty);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(marker.PowerSupplyBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(buildKit.IsStaged, Is.True);
            Assert.That(buildKit.StagedComponentCount, Is.EqualTo(7));
            Assert.That(powerSupply.GetInstanceID(), Is.EqualTo(physicalIdentity));
            Assert.That(powerSupply.Ownership,
                Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(powerSupply.IsStablePlacement, Is.True);
            Assert.That(Vector3.Distance(
                powerSupply.transform.position,
                expectedPose.position), Is.LessThan(0.0005f));
            Assert.That(Quaternion.Angle(
                powerSupply.transform.rotation,
                expectedPose.rotation), Is.LessThan(0.05f));
            Assert.That(buildKit.MatchesCommittedPlacement(powerSupply), Is.True);
            Assert.That(session.Inventory.Revision,
                Is.EqualTo(inventoryInHands + 1));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitInHands + 1));
            Assert.That(marker.PowerSupplyBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void StageFirstSixForPowerSupplyBuildKit(
            GaragePrototypeMarker marker)
        {
            StageFirstFiveForGraphicsCardBuildKit(marker);
            GraphicsCardBuildKitProjection graphicsCardBuildKit =
                marker.GraphicsCardBuildKit;
            AssertSuccess(marker.PlayerCarry.TryPickup(marker.GraphicsCard));
            MovePlayerToBuildKit(marker, graphicsCardBuildKit);
            AssertSuccess(
                marker.PlayerCarry.TrySetGraphicsCardBuildKitMode(true));
            Assert.That(marker.PlayerCarry.CurrentGraphicsCardBuildKitStatus,
                Is.EqualTo(GraphicsCardBuildKitStatus.Valid),
                marker.PlayerCarry.LastFailureCode);
            AssertSuccess(
                marker.PlayerCarry.TryConfirmGraphicsCardBuildKit());
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(graphicsCardBuildKit.IsStaged, Is.True);
            Assert.That(graphicsCardBuildKit.StagedComponentCount,
                Is.EqualTo(6));
            Assert.That(marker.GraphicsCardBinding.IsAuthorityInBuildKit,
                Is.True);
            Assert.That(marker.GraphicsCardBinding
                .ValidateProjectionInvariant().IsSuccess, Is.True);
        }

        private static void MovePlayerToBuildKit(
            GaragePrototypeMarker marker,
            PowerSupplyBuildKitProjection buildKit)
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

        private static void RunPowerSupplyCardinalCalibration(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse,
            Transform expectedParent)
        {
            const int FramesPerDirection = 3;
            Transform player = marker.PlayerMotor.transform;
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null, "cardinal-calibration");
            Assert.That(controller.enabled, Is.True, "cardinal-calibration");
            Vector3 forward = player.forward;
            Vector3 right = player.right;

            Vector3 start = player.position;
            StepPowerSupplyRouteMovement(
                marker,
                keyboard,
                mouse,
                Vector2.up,
                FramesPerDirection,
                expectedParent,
                "W-cardinal-calibration");
            Assert.That(
                Vector3.Dot(player.position - start, forward),
                Is.GreaterThan(0.05f),
                "W must move the CharacterController forward.");

            start = player.position;
            StepPowerSupplyRouteMovement(
                marker,
                keyboard,
                mouse,
                Vector2.down,
                FramesPerDirection,
                expectedParent,
                "S-cardinal-calibration");
            Assert.That(
                Vector3.Dot(player.position - start, forward),
                Is.LessThan(-0.05f),
                "S must move the CharacterController backward.");

            start = player.position;
            StepPowerSupplyRouteMovement(
                marker,
                keyboard,
                mouse,
                Vector2.left,
                FramesPerDirection,
                expectedParent,
                "A-cardinal-calibration");
            Assert.That(
                Vector3.Dot(player.position - start, right),
                Is.LessThan(-0.05f),
                "A must move the CharacterController left.");

            start = player.position;
            StepPowerSupplyRouteMovement(
                marker,
                keyboard,
                mouse,
                Vector2.right,
                FramesPerDirection,
                expectedParent,
                "D-cardinal-calibration");
            Assert.That(
                Vector3.Dot(player.position - start, right),
                Is.GreaterThan(0.05f),
                "D must move the CharacterController right.");

            ReleasePowerSupplyRouteInput(keyboard, mouse);
        }

        private static void StepPowerSupplyRouteMovement(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse,
            Vector2 movement,
            int frameCount,
            Transform expectedParent,
            string label)
        {
            for (int frame = 0; frame < frameCount; frame++)
            {
                StepPowerSupplyRouteFrame(
                    marker,
                    keyboard,
                    mouse,
                    movement,
                    Vector2.zero,
                    expectedParent,
                    label,
                    frame);
            }
        }

        private static void DrivePowerSupplyRoutePoint(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse,
            Vector3 worldTarget,
            int maximumFrames,
            Transform expectedParent,
            string routeLabel)
        {
            const float ArrivalTolerance = 0.18f;
            const float MinimumProgress = 0.003f;
            const int MaximumStagnantFrames = 30;
            Transform player = marker.PlayerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            Assert.That(cameraPivot, Is.Not.Null, routeLabel);
            Assert.That(controller, Is.Not.Null, routeLabel);
            Assert.That(controller.enabled, Is.True, routeLabel);
            marker.PlayerMotor.SetPaused(false);

            int stagnantFrames = 0;
            float previousDistance = PowerSupplyPlanarDistance(
                player.position,
                worldTarget);
            for (int frame = 0; frame < maximumFrames; frame++)
            {
                float distance = PowerSupplyPlanarDistance(
                    player.position,
                    worldTarget);
                if (distance <= ArrivalTolerance)
                {
                    ReleasePowerSupplyRouteInput(keyboard, mouse);
                    return;
                }

                Vector3 direction = Vector3.ProjectOnPlane(
                    worldTarget - player.position,
                    Vector3.up).normalized;
                float desiredYaw = Mathf.Atan2(
                    direction.x,
                    direction.z) * Mathf.Rad2Deg;
                float yawError = Mathf.DeltaAngle(
                    player.eulerAngles.y,
                    desiredYaw);
                float pitchError = -NormalizePowerSupplyAngle(
                    cameraPivot.localEulerAngles.x);
                bool aligned = Mathf.Abs(yawError) <= 2f &&
                               Mathf.Abs(pitchError) <= 2f;
                StepPowerSupplyRouteFrame(
                    marker,
                    keyboard,
                    mouse,
                    aligned ? Vector2.up : Vector2.zero,
                    ResolvePowerSupplyRouteLook(
                        marker,
                        yawError,
                        pitchError),
                    expectedParent,
                    routeLabel,
                    frame);

                float currentDistance = PowerSupplyPlanarDistance(
                    player.position,
                    worldTarget);
                stagnantFrames = aligned &&
                                 previousDistance - currentDistance <
                                 MinimumProgress
                    ? stagnantFrames + 1
                    : 0;
                Assert.That(
                    stagnantFrames,
                    Is.LessThan(MaximumStagnantFrames),
                    $"{routeLabel} became blocked while approaching {worldTarget}.");
                previousDistance = currentDistance;
            }

            ReleasePowerSupplyRouteInput(keyboard, mouse);
            Assert.Fail(
                $"{routeLabel} did not reach {worldTarget}; remaining " +
                $"{PowerSupplyPlanarDistance(player.position, worldTarget):0.000} m " +
                $"after {maximumFrames} frames.");
        }

        private static void AimPowerSupplyRouteAt(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse,
            Vector3 worldTarget,
            Transform expectedParent,
            string routeLabel)
        {
            const int MaximumFrames = 90;
            Transform player = marker.PlayerMotor.transform;
            Transform cameraPivot = player.Find("CameraPivot");
            Camera camera = marker.PlayerMotor.GetComponentInChildren<Camera>();
            Assert.That(cameraPivot, Is.Not.Null, routeLabel);
            Assert.That(camera, Is.Not.Null, routeLabel);

            for (int frame = 0; frame < MaximumFrames; frame++)
            {
                Vector3 direction =
                    (worldTarget - camera.transform.position).normalized;
                Vector3 planarDirection = Vector3.ProjectOnPlane(
                    direction,
                    Vector3.up);
                float desiredYaw = Mathf.Atan2(
                    planarDirection.x,
                    planarDirection.z) * Mathf.Rad2Deg;
                float desiredPitch = -Mathf.Asin(direction.y) * Mathf.Rad2Deg;
                float yawError = Mathf.DeltaAngle(
                    player.eulerAngles.y,
                    desiredYaw);
                float pitchError = Mathf.DeltaAngle(
                    NormalizePowerSupplyAngle(cameraPivot.localEulerAngles.x),
                    desiredPitch);
                if (Mathf.Abs(yawError) <= 0.75f &&
                    Mathf.Abs(pitchError) <= 0.75f)
                {
                    ReleasePowerSupplyRouteInput(keyboard, mouse);
                    Vector3 expectedDirection =
                        (worldTarget - camera.transform.position).normalized;
                    Assert.That(
                        Vector3.Dot(camera.transform.forward, expectedDirection),
                        Is.GreaterThan(0.999f),
                        $"{routeLabel} did not acquire {worldTarget}.");
                    return;
                }

                StepPowerSupplyRouteFrame(
                    marker,
                    keyboard,
                    mouse,
                    Vector2.zero,
                    ResolvePowerSupplyRouteLook(
                        marker,
                        yawError,
                        pitchError),
                    expectedParent,
                    routeLabel,
                    frame);
            }

            ReleasePowerSupplyRouteInput(keyboard, mouse);
            Assert.Fail($"{routeLabel} did not acquire {worldTarget}.");
        }

        private static Vector2 ResolvePowerSupplyRouteLook(
            GaragePrototypeMarker marker,
            float yawError,
            float pitchError)
        {
            float sensitivity = marker.PlayerMotor.ViewSettings.MouseSensitivity;
            float vertical = marker.PlayerMotor.ViewSettings.InvertY
                ? pitchError / sensitivity
                : -pitchError / sensitivity;
            return new Vector2(
                Mathf.Clamp(yawError / sensitivity, -80f, 80f),
                Mathf.Clamp(vertical, -80f, 80f));
        }

        private static void StepPowerSupplyRouteFrame(
            GaragePrototypeMarker marker,
            Keyboard keyboard,
            Mouse mouse,
            Vector2 movement,
            Vector2 look,
            Transform expectedParent,
            string routeLabel,
            int frame)
        {
            const float SimulatedFrameDeltaTime = 1f / 60f;
            Transform player = marker.PlayerMotor.transform;
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            Assert.That(controller, Is.Not.Null, routeLabel);
            Assert.That(controller.enabled, Is.True, routeLabel);
            Assert.That(player.parent, Is.SameAs(expectedParent), routeLabel);

            InputSystem.QueueStateEvent(
                keyboard,
                PowerSupplyKeyboardStateForMove(movement));
            InputSystem.QueueStateEvent(
                mouse,
                new MouseState { delta = look });
            InputSystem.Update();

            Vector3 before = player.position;
            float maximumHorizontalStep =
                marker.PlayerMotor.ResolveHorizontalSpeed(false) *
                SimulatedFrameDeltaTime + 0.02f;
            marker.PlayerMotor.ProcessInputFrame(
                SimulatedFrameDeltaTime,
                SimulatedFrameDeltaTime);
            Physics.SyncTransforms();

            float horizontalStep = Vector3.ProjectOnPlane(
                player.position - before,
                Vector3.up).magnitude;
            Assert.That(
                horizontalStep,
                Is.LessThanOrEqualTo(maximumHorizontalStep),
                $"{routeLabel} exceeded the physical movement envelope on frame {frame}.");
            Assert.That(controller.enabled, Is.True, routeLabel);
            Assert.That(player.parent, Is.SameAs(expectedParent), routeLabel);
        }

        private static KeyboardState PowerSupplyKeyboardStateForMove(
            Vector2 movement)
        {
            if (movement.y > 0.5f)
            {
                return new KeyboardState(Key.W);
            }

            if (movement.y < -0.5f)
            {
                return new KeyboardState(Key.S);
            }

            if (movement.x < -0.5f)
            {
                return new KeyboardState(Key.A);
            }

            return movement.x > 0.5f
                ? new KeyboardState(Key.D)
                : new KeyboardState();
        }

        private static void ReleasePowerSupplyRouteInput(
            Keyboard keyboard,
            Mouse mouse)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.QueueStateEvent(mouse, new MouseState());
            InputSystem.Update();
        }

        private static float PowerSupplyPlanarDistance(
            Vector3 first,
            Vector3 second)
        {
            return Vector3.ProjectOnPlane(
                first - second,
                Vector3.up).magnitude;
        }

        private static float NormalizePowerSupplyAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }

        private static void AssertPowerSupplyAndCablesUnchanged(
            GarageStockFlowSession session,
            AssemblyBuildSnapshot expected,
            int expectedAssemblyReceiptCount,
            Atx24PowerCableState expectedAtx24State,
            long expectedAtx24Revision,
            int expectedAtx24ReceiptCount,
            Eps12vPowerCableState expectedEps12vState,
            long expectedEps12vRevision,
            int expectedEps12vReceiptCount,
            PcieGpuPowerCableState expectedPcieState,
            long expectedPcieRevision,
            int expectedPcieReceiptCount)
        {
            AssemblyBuildSnapshot actual = session.AssemblyBuild.GetSnapshot();
            Assert.That(actual.Revision, Is.EqualTo(expected.Revision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(expectedAssemblyReceiptCount));
            Assert.That(actual.PowerSupplyBayState,
                Is.EqualTo(expected.PowerSupplyBayState));
            Assert.That(actual.PowerSupplyItemId,
                Is.EqualTo(expected.PowerSupplyItemId));
            Assert.That(actual.PowerSupplyProductId,
                Is.EqualTo(expected.PowerSupplyProductId));
            Assert.That(actual.PowerSupplySeatedByOperationId,
                Is.EqualTo(expected.PowerSupplySeatedByOperationId));
            Assert.That(actual.PowerSupplyRetainedByOperationId,
                Is.EqualTo(expected.PowerSupplyRetainedByOperationId));
            Assert.That(actual.PowerSupplyMountOrientation,
                Is.EqualTo(expected.PowerSupplyMountOrientation));
            Assert.That(session.AssemblyBuild.Atx24PowerCableState,
                Is.EqualTo(expectedAtx24State));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(expectedAtx24Revision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableReceiptCount,
                Is.EqualTo(expectedAtx24ReceiptCount));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableState,
                Is.EqualTo(expectedEps12vState));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(expectedEps12vRevision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableReceiptCount,
                Is.EqualTo(expectedEps12vReceiptCount));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableState,
                Is.EqualTo(expectedPcieState));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(expectedPcieRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableReceiptCount,
                Is.EqualTo(expectedPcieReceiptCount));
        }
    }
}
