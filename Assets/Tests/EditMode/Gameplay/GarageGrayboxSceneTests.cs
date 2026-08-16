using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using PCShopEmpire3D.Editor.GaragePrototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay
{
    public sealed class GarageGrayboxSceneTests
    {
        [Test]
        public void GarageSceneIsFirstBuildSceneAndSampleReferenceIsPreserved()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;

            Assert.That(scenes.Length, Is.GreaterThanOrEqualTo(2));
            Assert.That(scenes[0].enabled, Is.True);
            Assert.That(scenes[0].path, Is.EqualTo(GaragePrototypeMarker.ScenePath));
            Assert.That(
                scenes.Any(scene => scene.path == "Assets/Scenes/SampleScene.unity"),
                Is.True);
        }

        [Test]
        public void GarageSceneContainsPlayableRigComfortDefaultsAndPrototypeHands()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                string[] rootNames = scene.GetRootGameObjects().Select(root => root.name).ToArray();
                Assert.That(rootNames, Does.Contain("__Systems"));
                Assert.That(rootNames, Does.Contain("Environment"));
                Assert.That(rootNames, Does.Contain("Gameplay"));
                Assert.That(rootNames, Does.Contain("PlayerSpawn"));
                Assert.That(rootNames, Does.Contain("Lighting"));
                Assert.That(rootNames, Does.Contain("Debug"));

                GaragePrototypeMarker marker = FindInScene<GaragePrototypeMarker>(scene);
                Assert.That(marker, Is.Not.Null);
                Assert.That(marker.PlayerMotor, Is.Not.Null);
                Assert.That(marker.PlayerInput, Is.Not.Null);
                Assert.That(marker.PlayerInput.Actions, Is.Not.Null);
                Assert.That(marker.PlayerCarry, Is.Not.Null);
                Assert.That(marker.PlayerInput.Actions.name, Is.EqualTo("InputSystem_Actions"));

                FirstPersonMotor motor = marker.PlayerMotor;
                CharacterController controller = motor.GetComponent<CharacterController>();
                Camera camera = motor.GetComponentInChildren<Camera>(true);
                Transform hands = motor.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ViewModelHands");
                VisibleHandsPresenter handsPresenter = hands.GetComponent<VisibleHandsPresenter>();
                PhysicalItemProjection[] physicalItems = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PhysicalItemProjection>(true))
                    .ToArray();
                TransportCartProjection[] transportCarts = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<TransportCartProjection>(true))
                    .ToArray();
                PlacementSurface[] placementSurfaces = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PlacementSurface>(true))
                    .ToArray();
                CheckoutStationProjection[] checkoutStations = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<CheckoutStationProjection>(true))
                    .ToArray();
                PlacementPreview placementPreview = motor.GetComponentInChildren<PlacementPreview>(true);

                Assert.That(controller.height, Is.EqualTo(1.75f).Within(0.001f));
                Assert.That(controller.radius, Is.EqualTo(0.3f).Within(0.001f));
                Assert.That(motor.WalkSpeed, Is.EqualTo(3.5f).Within(0.001f));
                Assert.That(motor.SprintSpeed, Is.EqualTo(5.2f).Within(0.001f));
                Assert.That(camera.fieldOfView, Is.EqualTo(72f).Within(0.001f));
                Assert.That(camera.nearClipPlane, Is.EqualTo(0.06f).Within(0.001f));
                Assert.That(camera.farClipPlane, Is.EqualTo(150f).Within(0.001f));
                Assert.That(motor.ViewSettings.MotionReduced, Is.True);
                Assert.That(hands.childCount, Is.EqualTo(2));
                Assert.That(handsPresenter, Is.Not.Null);
                Assert.That(physicalItems.Length, Is.EqualTo(8));
                Assert.That(
                    physicalItems.Select(item => item.ItemIdValue).Distinct(StringComparer.Ordinal).Count(),
                    Is.EqualTo(physicalItems.Length));
                PhysicalItemProjection[] smallBoxes = physicalItems.Where(
                    item => item.CarryProfile == PhysicalCarryProfile.SmallBox).ToArray();
                Assert.That(smallBoxes.Length, Is.EqualTo(3));
                PhysicalItemProjection smallBox = smallBoxes.Single(
                    item => item.ItemIdValue == "prototype.garage-box-001");
                PhysicalItemProjection stackBase = smallBoxes.Single(
                    item => item.ItemIdValue == "prototype.garage-box-002");
                PhysicalItemProjection largeBox = physicalItems.Single(
                    item => item.CarryProfile == PhysicalCarryProfile.LargeBox);
                PhysicalItemProjection deliveryItem = physicalItems.Single(
                    item => item.ItemIdValue == GarageStockFlowSession.ItemInstanceIdValue);
                PhysicalItemProjection motherboard = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.MotherboardItemInstanceIdValue);
                PhysicalItemProjection processor = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.ProcessorItemInstanceIdValue);
                PhysicalItemProjection memoryModule = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.MemoryItemInstanceIdValue);
                PhysicalItemProjection storageDevice = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.StorageItemInstanceIdValue);
                Assert.That(physicalItems.Count(
                    item => item.CarryProfile == PhysicalCarryProfile.PcComponent),
                    Is.EqualTo(4));
                Assert.That(smallBox.ItemIdValue, Is.EqualTo("prototype.garage-box-001"));
                Assert.That(smallBox.SupportsPlacement, Is.True);
                Assert.That(smallBox.DropHalfExtents, Is.EqualTo(new Vector3(0.35f, 0.225f, 0.25f)));
                Assert.That(stackBase.DisplayName, Is.EqualTo("Stok Kutusu"));
                Assert.That(stackBase.IsStablePlacement, Is.True);
                Assert.That(stackBase.IsStacked, Is.False);
                Assert.That(stackBase.HasStackedItem, Is.False);
                Assert.That(largeBox.ItemIdValue, Is.EqualTo("prototype.garage-large-box-001"));
                Assert.That(largeBox.DisplayName, Is.EqualTo("Büyük Kargo Kutusu"));
                Assert.That(largeBox.SupportsPlacement, Is.False);
                Assert.That(largeBox.Body.mass, Is.EqualTo(9f).Within(0.001f));
                Assert.That(largeBox.DropHalfExtents, Is.EqualTo(new Vector3(0.55f, 0.4f, 0.35f)));
                Assert.That(deliveryItem.DisplayName, Is.EqualTo(GarageStockFlowSession.ProductDisplayName));
                Assert.That(deliveryItem.IsStablePlacement, Is.True);
                InventoryItemWorldBinding deliveryBinding =
                    deliveryItem.GetComponent<InventoryItemWorldBinding>();
                DeliveryParcelProjection deliveryParcel =
                    deliveryItem.GetComponent<DeliveryParcelProjection>();
                Assert.That(deliveryBinding, Is.Not.Null);
                Assert.That(deliveryParcel, Is.Not.Null);
                Assert.That(deliveryParcel.State, Is.EqualTo(DeliveryParcelState.Sealed));
                Assert.That(deliveryParcel.SealedVisualRoot.activeSelf, Is.True);
                Assert.That(deliveryParcel.ProductVisualRoot.activeSelf, Is.False);
                Assert.That(deliveryParcel.OpenedShellVisualRoot.activeSelf, Is.False);
                Assert.That(deliveryParcel.OpenedShellVisualRoot.transform.parent,
                    Is.SameAs(deliveryItem.transform.parent));
                Assert.That(deliveryBinding.InventoryItemId.Value, Is.EqualTo(deliveryItem.ItemIdValue));
                MotherboardAssemblyItemBinding motherboardBinding =
                    motherboard.GetComponent<MotherboardAssemblyItemBinding>();
                Assert.That(motherboardBinding, Is.Not.Null);
                Assert.That(motherboard.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.MotherboardDisplayName));
                Assert.That(motherboard.SupportsPlacement, Is.False);
                Assert.That(motherboard.Body.mass, Is.EqualTo(0.9f).Within(0.001f));
                Assert.That(Vector3.Distance(
                    motherboard.DropHalfExtents,
                    new Vector3(0.127f, 0.127f, 0.045f)), Is.LessThan(0.0001f));
                Assert.That(marker.MotherboardSeat, Is.Not.Null);
                Assert.That(marker.MotherboardSeat.IsConfigured, Is.True);
                Assert.That(marker.MotherboardFastener, Is.Not.Null);
                Assert.That(marker.MotherboardFastener.IsConfigured, Is.True);
                Assert.That(marker.MotherboardBinding, Is.SameAs(motherboardBinding));
                Assert.That(motherboardBinding.PhysicalItem, Is.SameAs(motherboard));
                Assert.That(motherboardBinding.Seat, Is.SameAs(marker.MotherboardSeat));
                Assert.That(motherboardBinding.Fastener,
                    Is.SameAs(marker.MotherboardFastener));
                Assert.That(marker.MotherboardFastener.FastenerIdValue,
                    Is.EqualTo(GarageStockFlowSession.MotherboardFastenerIdValue));
                Assert.That(marker.MotherboardFastener.FocusCollider.enabled, Is.False);
                Assert.That(marker.MotherboardFastener.MatchesAuthorityState(
                    AssemblySeatState.Empty), Is.True);
                Assert.That(motherboardBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(motherboardBinding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.MotherboardItemInstanceIdValue));
                Assert.That(motherboard.ItemIdValue,
                    Is.EqualTo(motherboardBinding.InventoryItemIdValue));
                Assert.That(marker.StockFlow, Is.Not.Null);
                Assert.That(marker.StockFlow.ItemBinding, Is.SameAs(deliveryBinding));
                Assert.That(marker.StockFlow.Parcel, Is.SameAs(deliveryParcel));
                Assert.That(marker.StockFlow.EnsureInitialized().Order.Status,
                    Is.EqualTo(PCShopEmpire3D.Orders.PurchaseOrderStatus.Arrived));
                Assert.That(marker.StockFlow.Session.TryGetItem(out _), Is.False);
                Assert.That(marker.StockFlow.Session.TryGetMotherboardItem(
                    out InventoryItemRecord motherboardItem), Is.True);
                Assert.That(motherboardItem.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.AssemblyBuild.MotherboardSeatState,
                    Is.EqualTo(AssemblySeatState.Empty));
                Assert.That(motherboardBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
                ProcessorAssemblyItemBinding processorBinding =
                    processor.GetComponent<ProcessorAssemblyItemBinding>();
                Assert.That(processorBinding, Is.Not.Null);
                Assert.That(processor.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.ProcessorDisplayName));
                Assert.That(processor.SupportsPlacement, Is.False);
                Assert.That(processor.Body.mass, Is.EqualTo(0.08f).Within(0.001f));
                Assert.That(Vector3.Distance(
                    processor.DropHalfExtents,
                    new Vector3(0.0225f, 0.01875f, 0.010f)), Is.LessThan(0.0001f));
                Assert.That(marker.Processor, Is.SameAs(processor));
                Assert.That(marker.ProcessorBinding, Is.SameAs(processorBinding));
                Assert.That(marker.ProcessorSocket, Is.Not.Null);
                Assert.That(marker.ProcessorSocket.IsConfigured, Is.True);
                Assert.That(marker.ProcessorSocket.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorSlotIdValue));
                Assert.That(marker.ProcessorSocket.RetentionIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorRetentionIdValue));
                Assert.That(marker.ProcessorSocket.FocusCollider.enabled, Is.False);
                Assert.That(marker.ProcessorSocket.MatchesAuthorityState(
                    AssemblySeatState.Empty,
                    ProcessorSocketState.EmptyOpen), Is.True);
                Assert.That(processorBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(processorBinding.PhysicalItem, Is.SameAs(processor));
                Assert.That(processorBinding.Socket, Is.SameAs(marker.ProcessorSocket));
                Assert.That(processorBinding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorItemInstanceIdValue));
                Assert.That(marker.StockFlow.Session.TryGetProcessorItem(
                    out InventoryItemRecord processorItem), Is.True);
                Assert.That(processorItem.Id,
                    Is.EqualTo(marker.StockFlow.Session.ProcessorItemId));
                Assert.That(processorItem.ProductId,
                    Is.EqualTo(marker.StockFlow.Session.ProcessorProductId));
                Assert.That(processorItem.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.AssemblyBuild.HasProcessorSocket,
                    Is.True);
                Assert.That(marker.StockFlow.Session.AssemblyBuild.ProcessorSocketState,
                    Is.EqualTo(ProcessorSocketState.EmptyOpen));
                Assert.That(processorBinding.ValidateProjectionInvariant().IsSuccess,
                    Is.True);

                DimmAssemblyItemBinding dimmBinding =
                    memoryModule.GetComponent<DimmAssemblyItemBinding>();
                Assert.That(dimmBinding, Is.Not.Null);
                Assert.That(memoryModule.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.MemoryDisplayName));
                Assert.That(memoryModule.SupportsPlacement, Is.False);
                Assert.That(memoryModule.Body.mass, Is.EqualTo(0.045f).Within(0.001f));
                Assert.That(Vector3.Distance(
                    memoryModule.DropHalfExtents,
                    new Vector3(0.068f, 0.018f, 0.010f)), Is.LessThan(0.0001f));
                Assert.That(marker.MemoryModule, Is.SameAs(memoryModule));
                Assert.That(marker.DimmBinding, Is.SameAs(dimmBinding));
                Assert.That(marker.DimmSlot, Is.Not.Null);
                Assert.That(marker.DimmSlot.IsConfigured, Is.True);
                Assert.That(marker.DimmSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemorySlotIdValue));
                Assert.That(marker.DimmSlot.RetentionIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryRetentionIdValue));
                Assert.That(marker.DimmSlot.ChannelIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryChannelIdValue));
                Assert.That(marker.DimmSlot.BankIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryBankIdValue));
                Assert.That(marker.DimmSlot.FocusCollider.enabled, Is.False);
                Assert.That(marker.DimmSlot.MatchesAuthorityState(
                    AssemblySeatState.Empty,
                    MemorySlotState.EmptyOpen), Is.True);
                Assert.That(dimmBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(dimmBinding.PhysicalItem, Is.SameAs(memoryModule));
                Assert.That(dimmBinding.Slot, Is.SameAs(marker.DimmSlot));
                Assert.That(dimmBinding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryItemInstanceIdValue));
                Assert.That(marker.StockFlow.Session.TryGetMemoryItem(
                    out InventoryItemRecord memoryItem), Is.True);
                Assert.That(memoryItem.Id,
                    Is.EqualTo(marker.StockFlow.Session.MemoryItemId));
                Assert.That(memoryItem.ProductId,
                    Is.EqualTo(marker.StockFlow.Session.MemoryProductId));
                Assert.That(memoryItem.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.AssemblyBuild.HasMemorySlot, Is.True);
                Assert.That(marker.StockFlow.Session.AssemblyBuild.MemorySlotState,
                    Is.EqualTo(MemorySlotState.EmptyOpen));
                Assert.That(dimmBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
                M2StorageAssemblyItemBinding storageBinding =
                    storageDevice.GetComponent<M2StorageAssemblyItemBinding>();
                Assert.That(storageBinding, Is.Not.Null);
                Assert.That(storageDevice.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.StorageDisplayName));
                Assert.That(storageDevice.SupportsPlacement, Is.False);
                Assert.That(storageDevice.Body.mass, Is.EqualTo(0.010f).Within(0.001f));
                Assert.That(marker.StorageDevice, Is.SameAs(storageDevice));
                Assert.That(marker.StorageBinding, Is.SameAs(storageBinding));
                Assert.That(marker.StorageSlot, Is.Not.Null);
                Assert.That(marker.StorageSlot.IsConfigured, Is.True);
                Assert.That(marker.StorageSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageSlotIdValue));
                Assert.That(marker.StorageSlot.StandoffIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageStandoffIdValue));
                Assert.That(marker.StorageSlot.CaptiveScrewIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageCaptiveScrewIdValue));
                Assert.That(marker.StorageSlot.FocusCollider.enabled, Is.False);
                Assert.That(storageBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(storageBinding.PhysicalItem, Is.SameAs(storageDevice));
                Assert.That(storageBinding.Slot, Is.SameAs(marker.StorageSlot));
                Assert.That(marker.PlayerCarry.MatchesM2StorageConfiguration(
                    marker.StorageSlot,
                    storageBinding), Is.True);
                Assert.That(marker.StockFlow.Session.TryGetStorageItem(
                    out InventoryItemRecord storageItem), Is.True);
                Assert.That(storageItem.Id,
                    Is.EqualTo(marker.StockFlow.Session.StorageItemId));
                Assert.That(storageItem.ProductId,
                    Is.EqualTo(marker.StockFlow.Session.StorageProductId));
                Assert.That(storageItem.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.AssemblyBuild.StorageSlotState,
                    Is.EqualTo(StorageSlotState.EmptyOpen));
                Assert.That(storageBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
                Assert.That(marker.StockFlow.Session.RetailOffers.Count, Is.Zero);
                Assert.That(marker.StockFlow.Session.RetailBaskets.Count, Is.Zero);
                Assert.That(marker.StockFlow.Session.RetailCheckouts.Count, Is.Zero);
                Assert.That(marker.StockFlow.Session.RetailCheckouts.CompletionCount, Is.Zero);
                Assert.That(marker.StockFlow.Session.CheckoutSettlements, Is.Not.Null);
                Assert.That(marker.StockFlow.Session.CheckoutSettlements.Revision, Is.Zero);
                Assert.That(marker.StockFlow.Session.CheckoutSettlements.SettlementCount, Is.Zero);
                Assert.That(marker.StockFlow.Session.CheckoutSettlements.TransactionCount, Is.Zero);
                Assert.That(marker.StockFlow.EconomyStatusText, Is.EqualTo("HAREKET YOK"));
                Assert.That(marker.StockFlow.StatusText, Does.Contain("MUHASEBE: HAREKET YOK"));
                Assert.That(marker.StockFlow.Session.CustomerVisits.Count, Is.Zero);
                Assert.That(marker.StockFlow.Session.CustomerConsultations, Is.Not.Null);
                Assert.That(marker.StockFlow.Session.CustomerConsultations.Revision, Is.Zero);
                Assert.That(marker.StockFlow.Session.TryGetPrototypeCustomerConsultation(out _),
                    Is.False);
                Assert.That(marker.CustomerFlow, Is.Not.Null);
                Assert.That(marker.CustomerFlow.StockFlow, Is.SameAs(marker.StockFlow));
                Assert.That(marker.CustomerFlow.PlayerInput, Is.SameAs(marker.PlayerInput));
                Assert.That(marker.CustomerFlow.PlayerCamera, Is.Not.Null);
                Assert.That(marker.CustomerFlow.NavigationSurface, Is.Not.Null);
                Assert.That(marker.CustomerFlow.NavigationSurface.collectObjects,
                    Is.EqualTo(CollectObjects.Volume));
                Assert.That(marker.CustomerFlow.NavigationSurface.useGeometry,
                    Is.EqualTo(NavMeshCollectGeometry.PhysicsColliders));
                Assert.That(marker.CustomerFlow.CustomerAgent, Is.Not.Null);
                Assert.That(marker.CustomerFlow.CustomerAgent.speed, Is.EqualTo(2.2f).Within(0.001f));
                Assert.That(marker.CustomerFlow.CustomerAgent.radius, Is.EqualTo(0.28f).Within(0.001f));
                Assert.That(marker.CustomerFlow.CustomerVisualRoot.activeSelf, Is.False);
                Assert.That(marker.CustomerFlow.CustomerVisualRoot.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                CapsuleCollider customerFocusCollider =
                    marker.CustomerFlow.CustomerVisualRoot.GetComponent<CapsuleCollider>();
                Assert.That(customerFocusCollider, Is.Not.Null);
                Assert.That(customerFocusCollider.isTrigger, Is.True);
                Assert.That(marker.CustomerFlow.CustomerStatusText.text,
                    Does.Contain("MÜŞTERİ AKIŞI: TEKLİF BEKLİYOR"));
                Assert.That(marker.CustomerFlow.CustomerSpeechText, Is.Not.Null);
                Assert.That(marker.CustomerFlow.CustomerSpeechText.text,
                    Does.Contain("MÜŞTERİ 001"));
                Assert.That(marker.CustomerFlow.EntranceWaypoint, Is.Not.Null);
                Assert.That(marker.CustomerFlow.BrowseWaypoint, Is.Not.Null);
                Assert.That(marker.CustomerFlow.CheckoutWaypoint, Is.Not.Null);
                Assert.That(marker.CustomerFlow.ExitWaypoint, Is.Not.Null);
                Assert.That(checkoutStations.Length, Is.EqualTo(1));
                CheckoutStationProjection checkoutStation = checkoutStations[0];
                Assert.That(marker.CheckoutStation, Is.SameAs(checkoutStation));
                Assert.That(checkoutStation.StationIdValue,
                    Is.EqualTo(CheckoutStationProjection.PrototypeStationIdValue));
                Assert.That(checkoutStation.StationId.Value,
                    Is.EqualTo(CheckoutStationProjection.PrototypeStationIdValue));
                Assert.That(checkoutStation.StockFlow, Is.SameAs(marker.StockFlow));
                Assert.That(checkoutStation.CustomerFlow, Is.SameAs(marker.CustomerFlow));
                Assert.That(checkoutStation.PlayerInput, Is.SameAs(marker.PlayerInput));
                Assert.That(checkoutStation.PlayerMotor, Is.SameAs(marker.PlayerMotor));
                Assert.That(checkoutStation.PlayerCamera, Is.SameAs(camera));
                Assert.That(checkoutStation.InteractionCollider, Is.Not.Null);
                Assert.That(checkoutStation.InteractionCollider.gameObject.name,
                    Is.EqualTo("CheckoutPlayerTerminal"));
                Assert.That(checkoutStation.InteractionCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(checkoutStation.StationStatusText, Is.Not.Null);
                Assert.That(checkoutStation.StationStatusText.text,
                    Does.Contain("KASA İSTASYONU"));
                Assert.That(checkoutStation.InteractionRange,
                    Is.EqualTo(CheckoutStationProjection.DefaultInteractionRange).Within(0.001f));
                Assert.That(checkoutStation.FocusDegrees,
                    Is.EqualTo(CheckoutStationProjection.DefaultFocusDegrees).Within(0.001f));
                GaragePrototypeHud hud = FindInScene<GaragePrototypeHud>(scene);
                Assert.That(hud, Is.Not.Null);
                Assert.That(hud.CheckoutStation, Is.SameAs(checkoutStation));
                Assert.That(marker.StockFlow.ShelfOfferText, Is.Not.Null);
                Assert.That(marker.StockFlow.ShelfOfferText.text,
                    Is.EqualTo("RAF A\nFİYAT YOK\nMÜŞTERİ: BOŞ\nKASA: BEKLİYOR"));
                Assert.That(physicalItems.All(item => item.Body != null), Is.True);
                Assert.That(
                    physicalItems.All(item =>
                        item.GetComponentsInChildren<Collider>().Length >= 1),
                    Is.True);
                Assert.That(transportCarts.Length, Is.EqualTo(1));
                TransportCartProjection cart = transportCarts[0];
                Assert.That(cart.CartIdValue, Is.EqualTo("prototype.garage-transport-cart-001"));
                Assert.That(cart.DisplayName, Is.EqualTo("Platform Arabası"));
                Assert.That(cart.Body, Is.Not.Null);
                Assert.That(cart.Body.isKinematic, Is.True);
                Assert.That(cart.Body.useGravity, Is.False);
                Assert.That(cart.CargoAnchor, Is.Not.Null);
                Assert.That(cart.HasCargo, Is.False);
                Assert.That(marker.TransportCart, Is.SameAs(cart));
                Assert.That(
                    cart.GetComponentsInChildren<Collider>(true).Length,
                    Is.GreaterThanOrEqualTo(3));
                Assert.That(placementSurfaces.Length, Is.EqualTo(2));
                PlacementSurface floorSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId == "prototype.stock-floor-small-box-a");
                PlacementSurface shelfSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId == "prototype.retail-shelf-a");
                Assert.That(floorSurface.GridSize, Is.EqualTo(0.25f).Within(0.001f));
                Assert.That(floorSurface.YawStepDegrees, Is.EqualTo(90f).Within(0.001f));
                Assert.That(shelfSurface.GridSize, Is.EqualTo(0.25f).Within(0.001f));
                InventoryPlacementZone shelfZone = shelfSurface.GetComponent<InventoryPlacementZone>();
                Assert.That(shelfZone, Is.Not.Null);
                Assert.That(shelfZone.ContainerId.Value,
                    Is.EqualTo(GarageStockFlowSession.ShelfContainerIdValue));
                Assert.That(shelfZone.ContainerKind, Is.EqualTo(InventoryContainerKind.Shelf));
                Assert.That(placementPreview, Is.Not.Null);
                Assert.That(placementPreview.IsVisible, Is.False);
                Assert.That(marker.PlayerCarry.PlacementPreview, Is.SameAs(placementPreview));
                Assert.That(
                    PrefabUtility.GetPrefabInstanceStatus(motor.gameObject),
                    Is.EqualTo(PrefabInstanceStatus.Connected));
                Assert.That(
                    PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(motor.gameObject),
                    Is.EqualTo("Assets/Prefabs/Prototype/PlayerRig.prefab"));

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    "Assets/Prefabs/Prototype/PlayerRig.prefab");
                Assert.That(prefab, Is.Not.Null);
                Assert.That(prefab.transform.localPosition, Is.EqualTo(Vector3.zero));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void BuildSceneCompositionPreservesExistingEnabledFlags()
        {
            var disabledSample = new EditorBuildSettingsScene(
                "Assets/Scenes/SampleScene.unity",
                false);
            var disabledFutureScene = new EditorBuildSettingsScene(
                "Assets/Scenes/FutureDisabled.unity",
                false);

            EditorBuildSettingsScene[] composed = GarageGrayboxSceneBuilder.ComposeBuildScenes(
                new[] { disabledSample, disabledFutureScene });

            Assert.That(composed[0].path, Is.EqualTo(GaragePrototypeMarker.ScenePath));
            Assert.That(composed[0].enabled, Is.True);
            Assert.That(composed.Single(scene => scene.path == disabledSample.path).enabled, Is.False);
            Assert.That(composed.Single(scene => scene.path == disabledFutureScene.path).enabled, Is.False);
        }

        [Test]
        public void DimmBindingRejectsCompensationUnsafeRevisionsBeforeMutation()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            var carryAnchor = new GameObject("DimmHeadroomCarryAnchor");
            try
            {
                GaragePrototypeMarker marker = FindInScene<GaragePrototypeMarker>(scene);
                GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
                DimmAssemblyItemBinding binding = marker.DimmBinding;
                PhysicalItemProjection memoryModule = marker.MemoryModule;
                StableId<AssemblyOperationIdScope> attachId =
                    StableId<AssemblyOperationIdScope>.Parse(
                        "operation.scene-dimm-headroom-attach");
                StableId<AssemblyOperationIdScope> secureId =
                    StableId<AssemblyOperationIdScope>.Parse(
                        "operation.scene-dimm-headroom-secure");

                Assert.That(session.PickupLooseMotherboardToHands().IsSuccess, Is.True);
                Assert.That(session.AttachMotherboard(attachId).IsSuccess, Is.True);
                Assert.That(session.SecureMotherboardFastener(
                    secureId,
                    attachId,
                    1).IsSuccess, Is.True);
                Assert.That(session.PickupLooseMemoryToHands().IsSuccess, Is.True);
                Assert.That(memoryModule.BeginCarry(
                    carryAnchor.transform,
                    LayerMask.NameToLayer("HeldItem")).IsSuccess, Is.True);

                long assemblyRevision = session.AssemblyBuild.Revision;
                long inventoryRevision = session.Inventory.Revision;
                int receiptCount = session.AssemblyBuild.ReceiptCount;
                SetRevision(session.AssemblyBuild, long.MaxValue - 1L);
                Assert.That(binding.TryAttachAt(
                        marker.DimmSlot.SnapPose,
                        DimmKeyOrientation.NotchAligned).Error,
                    Is.EqualTo(AssemblyFailures.RevisionOverflow));
                Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptCount));
                Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(session.AssemblyBuild.MemorySlotState,
                    Is.EqualTo(MemorySlotState.EmptyOpen));
                Assert.That(memoryModule.IsCarried, Is.True);
                SetRevision(session.AssemblyBuild, assemblyRevision);

                SetRevision(session.Inventory, long.MaxValue - 1L);
                Assert.That(binding.TryDropToWorld(new Pose(
                        memoryModule.transform.position,
                        memoryModule.transform.rotation)).Error,
                    Is.EqualTo(AssemblyFailures.InventoryRevisionOverflow));
                Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
                Assert.That(session.AssemblyBuild.ReceiptCount, Is.EqualTo(receiptCount));
                Assert.That(session.AssemblyBuild.MemorySlotState,
                    Is.EqualTo(MemorySlotState.EmptyOpen));
                Assert.That(memoryModule.IsCarried, Is.True);
                Assert.That(session.TryGetMemoryItem(out InventoryItemRecord memoryItem), Is.True);
                Assert.That(memoryItem.ContainerId, Is.EqualTo(session.HandsContainerId));
                SetRevision(session.Inventory, inventoryRevision);

                Assert.That(binding.SyncProjectionToAuthority().IsSuccess, Is.True);
                Assert.That(binding.ValidateProjectionInvariant().IsSuccess, Is.True);
                Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(carryAnchor);
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsReadableSemiRealisticBenchmarkContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                GaragePrototypeMarker marker = FindInScene<GaragePrototypeMarker>(scene);
                Assert.That(marker, Is.Not.Null);
                Assert.That(
                    GaragePrototypeMarker.Version,
                    Is.EqualTo("garage-m2-nvme-captive-screw-r26-v1"));

                Transform benchmark = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Single(transform => transform.name == "VisualBenchmarkCorner");
                Assert.That(benchmark.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(90));

                string[] rendererNames = benchmark.GetComponentsInChildren<Renderer>(true)
                    .Select(renderer => renderer.name)
                    .ToArray();
                Assert.That(rendererNames, Does.Contain("WorkbenchTop"));
                Assert.That(rendererNames, Does.Contain("DiagnosticMonitorScreen"));
                Assert.That(rendererNames, Does.Contain("ShelfTechUnit"));
                Assert.That(
                    benchmark.GetComponentsInChildren<Transform>(true)
                        .Select(transform => transform.name),
                    Does.Contain("ShelfPartsBox"));

                Transform assemblySlice = benchmark.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "PrototypeMotherboardAssemblySlice");
                Transform openChassis = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "PrototypeOpenChassis");
                Transform snapAnchor = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardSnapAnchor");
                Transform tray = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardTray");
                Transform standoffMarks = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "StandoffMarkArray");
                Transform statusPlate = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardSeatStatusPlate");
                Transform ioKey = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardIoKey");
                Transform cpuSocket = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardCpuSocket");
                Transform processorSocketBase = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorSocketBase");
                Transform processorSnapAnchor = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorSnapAnchor");
                Transform processorLoadPlatePivot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorLoadPlatePivot");
                Transform processorLoadPlate = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorLoadPlate");
                Transform processorLeverPivot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorRetentionLeverPivot");
                Transform processorLever = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorRetentionLever");
                Transform processorFocus = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorSocketFocusTarget");
                Transform processorRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "PrototypeProcessor");
                Transform processorPackage = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "PrototypeProcessorPackage");
                Transform dimmSlotRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardDimmSlotA2");
                Transform dimmSlotBase = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "DimmSlotBase");
                Transform dimmSnapAnchor = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MemoryModuleSnapAnchor");
                Transform leftLatchPivot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "DimmLeftLatchPivot");
                Transform leftLatch = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "DimmLeftLatch");
                Transform rightLatchPivot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "DimmRightLatchPivot");
                Transform rightLatch = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "DimmRightLatch");
                Transform dimmFocus = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "DimmSlotFocusTarget");
                Transform memoryRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "PrototypeMemoryModule");
                Transform memoryPackage = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "PrototypeMemoryModulePackage");
                Transform storageSlotRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardM2SlotPrimary");
                Transform storageSeatAnchor = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2StorageSeatedAnchor");
                Transform storageStandoff = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2Storage2280Standoff");
                Transform storageScrew = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2CaptiveScrew");
                Transform storageFocus = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2StorageSlotFocusTarget");
                Transform storageRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "PrototypeM2Nvme2280");
                Transform storagePcb = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2NvmePcb");
                Transform storageController = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2NvmeController");
                Transform storageNand = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2NvmeNandA");
                Transform storageLabel = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "M2NvmeLabel");
                Transform connectorMarks = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardConnectorMarks");
                Transform fastenerStation = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardFastenerStation");
                Transform fastenerHead = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardCaptiveFastener");
                Transform fastenerFocus = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "MotherboardFastenerFocusTarget");
                Transform screwdriver = assemblySlice.GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "CaptiveFastenerScrewdriver");
                MotherboardSeatProjection seat =
                    assemblySlice.GetComponentInChildren<MotherboardSeatProjection>(true);
                MotherboardFastenerProjection fastener =
                    assemblySlice.GetComponentInChildren<MotherboardFastenerProjection>(true);
                MotherboardAssemblyItemBinding binding =
                    assemblySlice.GetComponentInChildren<MotherboardAssemblyItemBinding>(true);
                ProcessorSocketProjection processorSocket =
                    assemblySlice.GetComponentInChildren<ProcessorSocketProjection>(true);
                ProcessorAssemblyItemBinding processorBinding =
                    assemblySlice.GetComponentInChildren<ProcessorAssemblyItemBinding>(true);
                PhysicalItemProjection processor =
                    processorRoot.GetComponent<PhysicalItemProjection>();
                DimmSlotProjection dimmSlot =
                    dimmSlotRoot.GetComponent<DimmSlotProjection>();
                DimmAssemblyItemBinding dimmBinding =
                    memoryRoot.GetComponent<DimmAssemblyItemBinding>();
                PhysicalItemProjection memoryModule =
                    memoryRoot.GetComponent<PhysicalItemProjection>();
                M2StorageSlotProjection storageSlot =
                    storageSlotRoot.GetComponent<M2StorageSlotProjection>();
                M2StorageAssemblyItemBinding storageBinding =
                    storageRoot.GetComponent<M2StorageAssemblyItemBinding>();
                PhysicalItemProjection storageDevice =
                    storageRoot.GetComponent<PhysicalItemProjection>();
                Assert.That(openChassis, Is.Not.Null);
                Assert.That(seat, Is.Not.Null);
                Assert.That(fastener, Is.Not.Null);
                Assert.That(binding, Is.Not.Null);
                Assert.That(processorSocket, Is.Not.Null);
                Assert.That(processorBinding, Is.Not.Null);
                Assert.That(processor, Is.Not.Null);
                Assert.That(dimmSlot, Is.Not.Null);
                Assert.That(dimmBinding, Is.Not.Null);
                Assert.That(memoryModule, Is.Not.Null);
                Assert.That(storageSlot, Is.Not.Null);
                Assert.That(storageBinding, Is.Not.Null);
                Assert.That(storageDevice, Is.Not.Null);
                Assert.That(marker.MotherboardSeat, Is.SameAs(seat));
                Assert.That(marker.MotherboardFastener, Is.SameAs(fastener));
                Assert.That(marker.MotherboardBinding, Is.SameAs(binding));
                Assert.That(marker.ProcessorSocket, Is.SameAs(processorSocket));
                Assert.That(marker.ProcessorBinding, Is.SameAs(processorBinding));
                Assert.That(marker.Processor, Is.SameAs(processor));
                Assert.That(marker.DimmSlot, Is.SameAs(dimmSlot));
                Assert.That(marker.DimmBinding, Is.SameAs(dimmBinding));
                Assert.That(marker.MemoryModule, Is.SameAs(memoryModule));
                Assert.That(marker.StorageSlot, Is.SameAs(storageSlot));
                Assert.That(marker.StorageBinding, Is.SameAs(storageBinding));
                Assert.That(marker.StorageDevice, Is.SameAs(storageDevice));
                Assert.That(binding.Fastener, Is.SameAs(fastener));
                Assert.That(storageSlot.IsConfigured, Is.True);
                Assert.That(storageSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageSlotIdValue));
                Assert.That(storageSlot.StandoffIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageStandoffIdValue));
                Assert.That(storageSlot.CaptiveScrewIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageCaptiveScrewIdValue));
                Assert.That(storageSlot.SeatedAnchor, Is.SameAs(storageSeatAnchor));
                Assert.That(storageSlot.FocusCollider,
                    Is.SameAs(storageFocus.GetComponent<BoxCollider>()));
                Assert.That(storageSlot.CaptiveScrewPivot,
                    Is.SameAs(storageScrew.parent));
                Assert.That(storageStandoff, Is.Not.Null);
                Assert.That(storagePcb.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("MotherboardPcb"));
                Assert.That(storageController.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("WorkshopRubber"));
                Assert.That(storageNand.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("WorkshopRubber"));
                Assert.That(storageLabel.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("LabelPaper"));
                Assert.That(storageDevice.ItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.StorageItemInstanceIdValue));
                Assert.That(storageDevice.Body.mass, Is.EqualTo(0.010f).Within(0.001f));
                Assert.That(storageBinding.ValidateProjectionInvariant().IsSuccess, Is.True);
                Assert.That(seat.SnapAnchor, Is.SameAs(snapAnchor));
                Assert.That(seat.SnapPose.position,
                    Is.EqualTo(new Vector3(-0.75f, 1.30f, 4.35f)));
                Assert.That(Quaternion.Angle(
                    seat.SnapPose.rotation,
                    Quaternion.Euler(0f, 180f, 0f)), Is.LessThan(0.1f));
                Assert.That(Vector3.Distance(
                    tray.localPosition,
                    new Vector3(-0.75f, 1.305f, 4.387f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    tray.GetComponent<BoxCollider>().size,
                    new Vector3(0.454f, 0.534f, 0.050f)), Is.LessThan(0.0001f));
                Assert.That(tray.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("DarkMetal"));
                Assert.That(Vector3.Distance(
                    statusPlate.localPosition,
                    new Vector3(-0.75f, 1.105f, 4.353f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    ioKey.localPosition,
                    new Vector3(-0.085f, 0.070f, 0.022f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    cpuSocket.localPosition,
                    new Vector3(0.015f, 0.025f, 0.012f)), Is.LessThan(0.0001f));
                Assert.That(processorSocket.IsConfigured, Is.True);
                Assert.That(processorSocket.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorSlotIdValue));
                Assert.That(processorSocket.RetentionIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorRetentionIdValue));
                Assert.That(processorSocket.SnapAnchor,
                    Is.SameAs(processorSnapAnchor));
                Assert.That(processorSocket.AssemblyRoot,
                    Is.SameAs(binding.PhysicalItem.transform));
                Assert.That(processorSocket.LoadPlatePivot,
                    Is.SameAs(processorLoadPlatePivot));
                Assert.That(processorSocket.RetentionLeverPivot,
                    Is.SameAs(processorLeverPivot));
                Assert.That(processorSocket.GhostRenderer, Is.Null);
                Assert.That(processorSocket.FocusCollider,
                    Is.SameAs(processorFocus.GetComponent<BoxCollider>()));
                Assert.That(processorSocket.FocusCollider.enabled, Is.False);
                Assert.That(processorSocket.FocusCollider.isTrigger, Is.False);
                Assert.That(processorSocket.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(Vector3.Distance(
                    processorFocus.localPosition,
                    new Vector3(0f, 0f, 0.010f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    processorFocus.GetComponent<BoxCollider>().size,
                    new Vector3(0.092f, 0.084f, 0.022f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    processorSocketBase.localPosition,
                    Vector3.zero), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    processorSnapAnchor.localPosition,
                    new Vector3(0f, 0f, 0.0035f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    processorLoadPlatePivot.localPosition,
                    new Vector3(0f, 0.026f, 0.007f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    processorLoadPlatePivot.localRotation,
                    Quaternion.Euler(-68f, 0f, 0f)), Is.LessThan(0.1f));
                Assert.That(Vector3.Distance(
                    processorLeverPivot.localPosition,
                    new Vector3(0.03025f, 0.026f, 0.007f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    processorLeverPivot.localRotation,
                    Quaternion.Euler(-55f, 0f, 0f)), Is.LessThan(0.1f));
                Assert.That(processorLoadPlate.GetComponent<Collider>(), Is.Null);
                Assert.That(processorLever.GetComponent<Collider>(), Is.Null);
                Assert.That(processorSocket.MatchesAuthorityState(
                    AssemblySeatState.Empty,
                    ProcessorSocketState.EmptyOpen), Is.True);
                Assert.That(processorBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(processorBinding.PhysicalItem, Is.SameAs(processor));
                Assert.That(processorBinding.Socket, Is.SameAs(processorSocket));
                Assert.That(processorBinding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorItemInstanceIdValue));
                Assert.That(processor.ItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorItemInstanceIdValue));
                Assert.That(processor.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.ProcessorDisplayName));
                Assert.That(processor.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(processor.SupportsPlacement, Is.False);
                Assert.That(processor.Body.mass, Is.EqualTo(0.08f).Within(0.001f));
                Assert.That(processor.Body.isKinematic, Is.True);
                Assert.That(processor.Body.useGravity, Is.False);
                Assert.That(Vector3.Distance(
                    processor.DropHalfExtents,
                    new Vector3(0.0225f, 0.01875f, 0.010f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    processorRoot.localPosition,
                    new Vector3(-1.17f, 0.992f, 3.93f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    processorRoot.localRotation,
                    Quaternion.Euler(-90f, 0f, 0f)), Is.LessThan(0.1f));
                Mesh processorMesh = processorPackage.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(processorMesh, Is.Not.Null);
                Assert.That(processorMesh.subMeshCount, Is.EqualTo(2));
                Assert.That(processorMesh.vertexCount, Is.EqualTo(54));
                Assert.That(processorMesh.uv.Length, Is.EqualTo(54));
                Assert.That(Vector3.Distance(processorMesh.bounds.center, Vector3.zero),
                    Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    processorMesh.bounds.size,
                    new Vector3(0.045f, 0.0375f, 0.004f)), Is.LessThan(0.0001f));
                Material[] processorMaterials =
                    processorPackage.GetComponent<Renderer>().sharedMaterials;
                Assert.That(processorMaterials.Length, Is.EqualTo(2));
                Assert.That(processorMaterials[0].name, Does.StartWith("MotherboardPcb"));
                Assert.That(processorMaterials[1].name, Does.StartWith("BrushedSteel"));
                Assert.That(processorRoot.GetComponent<BoxCollider>().center,
                    Is.EqualTo(Vector3.zero));
                Assert.That(Vector3.Distance(
                    processorRoot.GetComponent<BoxCollider>().size,
                    new Vector3(0.045f, 0.0375f, 0.004f)), Is.LessThan(0.0001f));
                Assert.That(processorSocketBase.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("WorkshopRubber"));
                Bounds socketBaseBounds =
                    processorSocketBase.GetComponent<MeshFilter>().sharedMesh.bounds;
                Mesh socketBaseMesh =
                    processorSocketBase.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(socketBaseMesh.vertexCount, Is.EqualTo(138));
                Assert.That(socketBaseMesh.uv.Length, Is.EqualTo(138));
                Assert.That(socketBaseBounds.size.x, Is.EqualTo(0.060f).Within(0.0001f));
                Assert.That(socketBaseBounds.size.y, Is.EqualTo(0.052f).Within(0.0001f));
                Assert.That(socketBaseMesh.vertices.Any(vertex => Vector3.Distance(
                    vertex,
                    new Vector3(0.01925f, -0.01850f, 0f)) < 0.00001f), Is.True);
                Assert.That(socketBaseMesh.vertices.Any(vertex => Vector3.Distance(
                    vertex,
                    new Vector3(0.02225f, -0.01550f, 0.0035f)) < 0.00001f), Is.True);
                Bounds loadPlateBounds =
                    processorLoadPlate.GetComponent<MeshFilter>().sharedMesh.bounds;
                Mesh loadPlateMesh =
                    processorLoadPlate.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(loadPlateMesh.vertexCount, Is.EqualTo(96));
                Assert.That(loadPlateMesh.uv.Length, Is.EqualTo(96));
                Assert.That(Vector3.Distance(
                    loadPlateBounds.size,
                    new Vector3(0.058f, 0.050f, 0.0015f)), Is.LessThan(0.0001f));
                Assert.That(loadPlateBounds.center.y, Is.EqualTo(-0.026f).Within(0.0001f));
                Assert.That(loadPlateMesh.vertices.Any(vertex =>
                    Mathf.Abs(Mathf.Abs(vertex.x) - 0.0175f) < 0.00001f &&
                    Mathf.Abs(vertex.y + 0.0105f) < 0.00001f), Is.True);
                Assert.That(loadPlateMesh.vertices.Any(vertex =>
                    Mathf.Abs(Mathf.Abs(vertex.x) - 0.0175f) < 0.00001f &&
                    Mathf.Abs(vertex.y + 0.0415f) < 0.00001f), Is.True);
                Assert.That(
                    processorSnapAnchor.localPosition.z + processorMesh.bounds.max.z,
                    Is.LessThan(
                        processorLoadPlatePivot.localPosition.z -
                        loadPlateBounds.extents.z),
                    "Closed load-plate frame must clear the seated CPU IHS.");
                Renderer workbenchTop = benchmark.GetComponentsInChildren<Renderer>(true)
                    .Single(renderer => renderer.name == "WorkbenchTop");
                Assert.That(Mathf.Abs(
                    processorPackage.GetComponent<Renderer>().bounds.min.y -
                    workbenchTop.bounds.max.y), Is.LessThan(0.0001f));
                Assert.That(processorBinding.ValidateProjectionInvariant().IsSuccess,
                    Is.True);

                Assert.That(Vector3.Distance(
                    dimmSlotRoot.localPosition,
                    new Vector3(0.105f, 0.045f, 0.012f)), Is.LessThan(0.0001f));
                Assert.That(dimmSlot.IsConfigured, Is.True);
                Assert.That(dimmSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemorySlotIdValue));
                Assert.That(dimmSlot.RetentionIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryRetentionIdValue));
                Assert.That(dimmSlot.ChannelIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryChannelIdValue));
                Assert.That(dimmSlot.BankIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryBankIdValue));
                Assert.That(dimmSlot.SnapAnchor, Is.SameAs(dimmSnapAnchor));
                Assert.That(dimmSlot.AssemblyRoot, Is.SameAs(binding.PhysicalItem.transform));
                Assert.That(dimmSlot.LeftLatchPivot, Is.SameAs(leftLatchPivot));
                Assert.That(dimmSlot.RightLatchPivot, Is.SameAs(rightLatchPivot));
                Assert.That(leftLatchPivot, Is.Not.SameAs(rightLatchPivot));
                Assert.That(Vector3.Distance(
                    dimmSnapAnchor.localPosition,
                    new Vector3(0f, 0f, 0.024f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    dimmSnapAnchor.localRotation,
                    Quaternion.LookRotation(
                        Vector3.right,
                        Vector3.forward)), Is.LessThan(0.1f));
                Assert.That(Vector3.Dot(dimmSnapAnchor.right, dimmSlotRoot.up),
                    Is.GreaterThan(0.999f));
                Assert.That(Vector3.Dot(dimmSnapAnchor.up, dimmSlotRoot.forward),
                    Is.GreaterThan(0.999f));
                Assert.That(dimmSlot.FocusCollider,
                    Is.SameAs(dimmFocus.GetComponent<BoxCollider>()));
                Assert.That(dimmSlot.FocusCollider.enabled, Is.False);
                Assert.That(dimmSlot.FocusCollider.isTrigger, Is.False);
                Assert.That(dimmSlot.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(Vector3.Distance(
                    dimmFocus.localPosition,
                    new Vector3(0f, 0f, 0.042f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    dimmFocus.GetComponent<BoxCollider>().size,
                    new Vector3(0.052f, 0.150f, 0.080f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    leftLatchPivot.localPosition,
                    new Vector3(0f, -0.064f, 0.006f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    rightLatchPivot.localPosition,
                    new Vector3(0f, 0.064f, 0.006f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    leftLatchPivot.localRotation,
                    Quaternion.Euler(-28f, 0f, 0f)), Is.LessThan(0.1f));
                Assert.That(Quaternion.Angle(
                    rightLatchPivot.localRotation,
                    Quaternion.Euler(28f, 0f, 0f)), Is.LessThan(0.1f));
                Assert.That(leftLatch.GetComponent<Collider>(), Is.Null);
                Assert.That(rightLatch.GetComponent<Collider>(), Is.Null);
                Assert.That(dimmSlotBase.GetComponent<Collider>(), Is.Null);
                Assert.That(dimmSlot.MatchesAuthorityState(
                    AssemblySeatState.Empty,
                    MemorySlotState.EmptyOpen), Is.True);
                Assert.That(dimmSlot.LatchVisualPhase,
                    Is.EqualTo(DimmLatchVisualPhase.Stable));

                Assert.That(dimmBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(dimmBinding.PhysicalItem, Is.SameAs(memoryModule));
                Assert.That(dimmBinding.Slot, Is.SameAs(dimmSlot));
                Assert.That(marker.PlayerCarry.MatchesDimmConfiguration(
                    dimmSlot,
                    dimmBinding), Is.True);
                Assert.That(marker.PlayerCarry.MatchesDimmConfiguration(
                    null,
                    null), Is.False);
                Assert.That(dimmBinding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryItemInstanceIdValue));
                Assert.That(memoryModule.ItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.MemoryItemInstanceIdValue));
                Assert.That(memoryModule.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.MemoryDisplayName));
                Assert.That(memoryModule.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(memoryModule.SupportsPlacement, Is.False);
                Assert.That(memoryModule.Body.mass, Is.EqualTo(0.045f).Within(0.001f));
                Assert.That(memoryModule.Body.isKinematic, Is.True);
                Assert.That(memoryModule.Body.useGravity, Is.False);
                Assert.That(Vector3.Distance(
                    memoryModule.DropHalfExtents,
                    new Vector3(0.068f, 0.018f, 0.010f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    memoryRoot.localPosition,
                    new Vector3(-1.05f, 0.992f, 3.93f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    memoryRoot.localRotation,
                    Quaternion.Euler(-90f, 90f, 0f)), Is.LessThan(0.1f));
                Assert.That(memoryRoot.GetComponent<BoxCollider>().center,
                    Is.EqualTo(new Vector3(0f, 0.004f, 0f)));
                Assert.That(Vector3.Distance(
                    memoryRoot.GetComponent<BoxCollider>().size,
                    new Vector3(0.136f, 0.034f, 0.010f)), Is.LessThan(0.0001f));
                Mesh memoryMesh = memoryPackage.GetComponent<MeshFilter>().sharedMesh;
                Assert.That(memoryMesh, Is.Not.Null);
                Assert.That(memoryMesh.subMeshCount, Is.EqualTo(4));
                Assert.That(memoryMesh.vertexCount, Is.GreaterThan(250));
                Assert.That(memoryMesh.uv.Length, Is.EqualTo(memoryMesh.vertexCount));
                Assert.That(memoryPackage.GetComponent<Renderer>().sharedMaterials.Length,
                    Is.EqualTo(4));
                Assert.That(Mathf.Abs(
                    memoryPackage.GetComponent<Renderer>().bounds.min.y -
                    workbenchTop.bounds.max.y), Is.LessThan(0.0001f));
                Assert.That(dimmBinding.ValidateProjectionInvariant().IsSuccess, Is.True);

                Bounds standoffBounds = standoffMarks.GetComponent<MeshFilter>().sharedMesh.bounds;
                Assert.That(Vector3.Distance(
                    standoffBounds.center,
                    new Vector3(-0.75f, 1.30f, 4.359f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    standoffBounds.size,
                    new Vector3(0.192f, 0.192f, 0.006f)), Is.LessThan(0.0001f));

                Bounds connectorBounds = connectorMarks.GetComponent<MeshFilter>().sharedMesh.bounds;
                Assert.That(Vector3.Distance(
                    connectorBounds.center,
                    new Vector3(0.020f, 0.014f, 0.012f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    connectorBounds.size,
                    new Vector3(0.160f, 0.182f, 0.012f)), Is.LessThan(0.0001f));
                Assert.That(fastener.FastenerIdValue,
                    Is.EqualTo(GarageStockFlowSession.MotherboardFastenerIdValue));
                Assert.That(fastener.IsConfigured, Is.True);
                Assert.That(fastener.FocusCollider,
                    Is.SameAs(fastenerFocus.GetComponent<BoxCollider>()));
                Assert.That(fastener.FocusCollider.enabled, Is.False);
                Assert.That(fastener.FocusCollider.isTrigger, Is.False);
                Assert.That(fastener.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(Vector3.Distance(
                    fastenerFocus.localPosition,
                    new Vector3(-0.66f, 1.21f, 4.336f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    fastenerFocus.GetComponent<BoxCollider>().size,
                    new Vector3(0.060f, 0.060f, 0.016f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    fastenerHead.localPosition,
                    new Vector3(-0.66f, 1.21f, 4.335f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    fastenerHead.localScale,
                    new Vector3(0.012f, 0.004f, 0.012f)), Is.LessThan(0.0001f));
                Assert.That(fastenerHead.GetComponent<Collider>(), Is.Null);
                Assert.That(fastener.Screwdriver, Is.SameAs(screwdriver));
                Assert.That(screwdriver.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(fastenerStation.GetComponent<Rigidbody>(), Is.Null);
                Assert.That(fastener.StatusText, Is.Not.Null);
                Assert.That(fastener.StatusText.text, Is.EqualTo("[ ] ANAKARTI OTURT"));
                Assert.That(fastener.StatusText.text, Does.Not.Contain("\n"));
                Assert.That(fastener.StatusText.transform.parent, Is.SameAs(statusPlate));
                Assert.That(Vector3.Distance(
                    fastener.StatusText.transform.localPosition,
                    new Vector3(0f, 0f, -0.010f)), Is.LessThan(0.0001f));
                Assert.That(fastener.StatusText.characterSize, Is.EqualTo(0.011f));
                Assert.That(fastener.StatusText.fontSize, Is.EqualTo(36));
                Renderer fastenerTextRenderer = fastener.StatusText.GetComponent<Renderer>();
                Assert.That(fastenerTextRenderer.shadowCastingMode,
                    Is.EqualTo(ShadowCastingMode.Off));
                Assert.That(fastenerTextRenderer.receiveShadows, Is.False);
                Assert.That(fastenerTextRenderer.motionVectorGenerationMode,
                    Is.EqualTo(MotionVectorGenerationMode.ForceNoMotion));
                Renderer statusPlateRenderer = statusPlate.GetComponent<Renderer>();
                Assert.That(statusPlateRenderer.shadowCastingMode,
                    Is.EqualTo(ShadowCastingMode.Off));
                Assert.That(statusPlateRenderer.receiveShadows, Is.False);
                Assert.That(statusPlateRenderer.motionVectorGenerationMode,
                    Is.EqualTo(MotionVectorGenerationMode.ForceNoMotion));
                Assert.That(fastenerHead.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("BrushedSteel"));
                Assert.That(fastener.MatchesAuthorityState(AssemblySeatState.Empty), Is.True);
                Assert.That(assemblySlice.GetComponentsInChildren<Renderer>(true).Length,
                    Is.EqualTo(40));
                Assert.That(assemblySlice.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(15));
                Assert.That(assemblySlice.GetComponentsInChildren<Light>(true), Is.Empty);
                Assert.That(assemblySlice.GetComponentsInChildren<TextMesh>(true).Length,
                    Is.EqualTo(1));
                Assert.That(assemblySlice.GetComponentsInChildren<NavMeshObstacle>(true), Is.Empty);
                Assert.That(
                    benchmark.GetComponentsInChildren<Transform>(true)
                        .Select(transform => transform.name),
                    Does.Not.Contain("BenchPcCase"));

                Material concrete = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/Concrete.mat");
                Material metal = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/DarkMetal.mat");
                Material steel = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/BrushedSteel.mat");
                Material wood = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/WoodLaminate.mat");
                Material cardboard = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/Cardboard.mat");
                Material screen = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/ScreenGlass.mat");
                Material deliveryArrived = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/DeliveryStatusArrived.mat");
                Material deliveryAccepted = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/DeliveryStatusAccepted.mat");
                Material deliveryShelved = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/DeliveryStatusShelved.mat");
                Assert.That(concrete, Is.Not.Null);
                Assert.That(metal, Is.Not.Null);
                Assert.That(steel, Is.Not.Null);
                Assert.That(wood, Is.Not.Null);
                Assert.That(cardboard, Is.Not.Null);
                Assert.That(screen, Is.Not.Null);
                Assert.That(deliveryArrived, Is.Not.Null);
                Assert.That(deliveryAccepted, Is.Not.Null);
                Assert.That(deliveryShelved, Is.Not.Null);
                Assert.That(concrete.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(metal.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(steel.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(wood.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(cardboard.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(metal.GetFloat("_Metallic"), Is.LessThan(0.2f));
                Assert.That(steel.GetFloat("_Metallic"), Is.GreaterThan(0.8f));
                Assert.That(screen.GetFloat("_Smoothness"), Is.GreaterThan(0.75f));
                Assert.That(screen.IsKeywordEnabled("_EMISSION"), Is.True);
                Assert.That(deliveryArrived.IsKeywordEnabled("_EMISSION"), Is.True);
                Assert.That(deliveryAccepted.IsKeywordEnabled("_EMISSION"), Is.True);
                Assert.That(deliveryShelved.IsKeywordEnabled("_EMISSION"), Is.True);

                Volume volume = FindInScene<Volume>(scene);
                Assert.That(volume, Is.Not.Null);
                Assert.That(volume.isGlobal, Is.True);
                Assert.That(volume.sharedProfile, Is.Not.Null);
                Assert.That(volume.sharedProfile.TryGet(out Tonemapping tonemapping), Is.True);
                Assert.That(tonemapping.mode.value, Is.EqualTo(TonemappingMode.ACES));
                Assert.That(volume.sharedProfile.TryGet(out Bloom bloom), Is.True);
                Assert.That(bloom.intensity.value, Is.InRange(0.05f, 0.25f));

                Camera camera = marker.PlayerMotor.GetComponentInChildren<Camera>(true);
                UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
                Assert.That(camera.allowHDR, Is.True);
                Assert.That(cameraData.renderPostProcessing, Is.True);
                Assert.That(
                    cameraData.antialiasing,
                    Is.EqualTo(AntialiasingMode.SubpixelMorphologicalAntiAliasing));

                Light taskLight = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Light>(true))
                    .Single(light => light.name == "WorkbenchTaskLight");
                Assert.That(taskLight.type, Is.EqualTo(LightType.Spot));
                Assert.That(taskLight.shadows, Is.EqualTo(LightShadows.Soft));

                ReflectionProbe reflectionProbe = FindInScene<ReflectionProbe>(scene);
                Assert.That(reflectionProbe, Is.Not.Null);
                Assert.That(reflectionProbe.mode, Is.EqualTo(ReflectionProbeMode.Realtime));
                Assert.That(reflectionProbe.refreshMode, Is.EqualTo(ReflectionProbeRefreshMode.OnAwake));
                Assert.That(reflectionProbe.resolution, Is.EqualTo(128));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static void SetRevision(object authority, long revision)
        {
            PropertyInfo property = authority.GetType().GetProperty(
                "Revision",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(property, Is.Not.Null);
            property.GetSetMethod(nonPublic: true).Invoke(
                authority,
                new object[] { revision });
        }

        private static T FindInScene<T>(Scene scene)
            where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                {
                    return component;
                }
            }

            return null;
        }
    }
}
