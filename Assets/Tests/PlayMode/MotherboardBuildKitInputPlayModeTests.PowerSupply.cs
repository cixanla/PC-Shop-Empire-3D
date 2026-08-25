using System.Collections;
using System.Linq;
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
        public IEnumerator KeyboardMouseMovesExactReservedPowerSupplyFromSixToSeven()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Mouse mouse = InputSystem.AddDevice<Mouse>();
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            Assert.That(marker, Is.Not.Null);

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

            AimPlayerAtItem(marker, powerSupply, -Vector3.forward);
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

            MovePlayerToBuildKit(marker, buildKit);
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
