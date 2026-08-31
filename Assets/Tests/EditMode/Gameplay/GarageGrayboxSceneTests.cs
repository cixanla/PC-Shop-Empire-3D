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
                CustomPcWorkTicketStationProjection[] workTicketStations = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        CustomPcWorkTicketStationProjection>(true))
                    .ToArray();
                MotherboardBuildKitProjection[] motherboardBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        MotherboardBuildKitProjection>(true))
                    .ToArray();
                ProcessorBuildKitProjection[] processorBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        ProcessorBuildKitProjection>(true))
                    .ToArray();
                MemoryModuleBuildKitProjection[] memoryModuleBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        MemoryModuleBuildKitProjection>(true))
                    .ToArray();
                StorageBuildKitProjection[] storageBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        StorageBuildKitProjection>(true))
                    .ToArray();
                ProcessorCoolerBuildKitProjection[] processorCoolerBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        ProcessorCoolerBuildKitProjection>(true))
                    .ToArray();
                GraphicsCardBuildKitProjection[] graphicsCardBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        GraphicsCardBuildKitProjection>(true))
                    .ToArray();
                PowerSupplyBuildKitProjection[] powerSupplyBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PowerSupplyBuildKitProjection>(true))
                    .ToArray();
                Atx24PowerCableBuildKitProjection[] atx24PowerCableBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        Atx24PowerCableBuildKitProjection>(true))
                    .ToArray();
                Eps12vPowerCableBuildKitProjection[] eps12vPowerCableBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        Eps12vPowerCableBuildKitProjection>(true))
                    .ToArray();
                PcieGpuPowerCableBuildKitProjection[] pcieGpuPowerCableBuildKits = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PcieGpuPowerCableBuildKitProjection>(true))
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
                Assert.That(physicalItems.Length, Is.EqualTo(15));
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
                    item => item.ItemIdValue == "prototype.garage-large-box-001");
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
                PhysicalItemProjection processorCooler = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue);
                PhysicalItemProjection graphicsCard = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession
                                .GraphicsCardAssemblyItemInstanceIdValue);
                PhysicalItemProjection powerSupply = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.PowerSupplyItemInstanceIdValue);
                PhysicalItemProjection atx24PowerCable = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.Atx24PowerCableItemInstanceIdValue);
                PhysicalItemProjection eps12vPowerCable = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession.Eps12vPowerCableItemInstanceIdValue);
                PhysicalItemProjection pcieGpuPowerCable = physicalItems.Single(
                    item => item.ItemIdValue ==
                            GarageStockFlowSession
                                .PcieGpuPowerCableItemInstanceIdValue);
                Assert.That(physicalItems.Count(
                    item => item.CarryProfile == PhysicalCarryProfile.PcComponent),
                    Is.EqualTo(10));
                Assert.That(powerSupply.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.PowerSupplyDisplayName));
                Assert.That(powerSupply.SupportsPlacement, Is.False);
                Assert.That(atx24PowerCable.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.Atx24PowerCableDisplayName));
                Assert.That(atx24PowerCable.SupportsPlacement, Is.False);
                Assert.That(eps12vPowerCable.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.Eps12vPowerCableDisplayName));
                Assert.That(eps12vPowerCable.SupportsPlacement, Is.False);
                Assert.That(pcieGpuPowerCable.DisplayName,
                    Is.EqualTo(
                        GarageStockFlowSession.PcieGpuPowerCableDisplayName));
                Assert.That(pcieGpuPowerCable.SupportsPlacement, Is.False);
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
                ProcessorCoolerAssemblyItemBinding coolerBinding =
                    processorCooler.GetComponent<ProcessorCoolerAssemblyItemBinding>();
                ProcessorCoolerRuntimeGeometry coolerGeometry =
                    processorCooler.GetComponent<ProcessorCoolerRuntimeGeometry>();
                ProcessorCoolerRuntimeSmokeMarker coolerSmoke =
                    processorCooler.GetComponent<ProcessorCoolerRuntimeSmokeMarker>();
                Assert.That(coolerBinding, Is.Not.Null);
                Assert.That(coolerGeometry, Is.Not.Null);
                Assert.That(coolerSmoke, Is.Not.Null);
                Assert.That(processorCooler.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.ProcessorCoolerDisplayName));
                Assert.That(processorCooler.SupportsPlacement, Is.False);
                Assert.That(processorCooler.Body.mass, Is.EqualTo(0.52f).Within(0.001f));
                Assert.That(marker.ProcessorCooler, Is.SameAs(processorCooler));
                Assert.That(marker.ProcessorCoolerBinding, Is.SameAs(coolerBinding));
                Assert.That(marker.ProcessorCoolerGeometry, Is.SameAs(coolerGeometry));
                Assert.That(marker.ProcessorCoolerSlot, Is.Not.Null);
                Assert.That(marker.ProcessorCoolerSlot.IsConfigured, Is.True);
                Assert.That(marker.ProcessorCoolerSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorCoolerSlotIdValue));
                Assert.That(marker.ProcessorCoolerSlot.BracketIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorCoolerBracketIdValue));
                Assert.That(marker.ProcessorCoolerSlot.RetentionPointIdValues,
                    Is.EqualTo(new[]
                    {
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint1IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint2IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint3IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint4IdValue
                    }));
                Assert.That(marker.ProcessorCoolerSlot.RetentionPoints.Length,
                    Is.EqualTo(4));
                Assert.That(marker.ProcessorCoolerSlot.RetentionPoints.Distinct().Count(),
                    Is.EqualTo(4));
                Assert.That(marker.ProcessorCoolerSlot.ClearanceBlockers,
                    Is.EqualTo(new[] { memoryModule.GetComponent<Collider>() }));
                Assert.That(marker.ProcessorCoolerSlot.FocusCollider.enabled, Is.False);
                Assert.That(coolerBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(coolerBinding.PhysicalItem, Is.SameAs(processorCooler));
                Assert.That(coolerBinding.Slot, Is.SameAs(marker.ProcessorCoolerSlot));
                Assert.That(coolerBinding.InventoryItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue));
                Assert.That(marker.PlayerCarry.MatchesProcessorCoolerConfiguration(
                    marker.ProcessorCoolerSlot,
                    coolerBinding), Is.True);
                Assert.That(coolerGeometry.IsCanonical, Is.True);
                Assert.That(coolerGeometry.RetentionPoints.Length, Is.EqualTo(4));
                Assert.That(coolerGeometry.RetentionPoints.Distinct().Count(),
                    Is.EqualTo(4));
                Assert.That(coolerSmoke.IsReady, Is.True);
                Assert.That(marker.HasProcessorCoolerR27Runtime, Is.True);
                Assert.That(marker.StockFlow.Session.TryGetProcessorCoolerItem(
                    out InventoryItemRecord coolerItem), Is.True);
                Assert.That(coolerItem.Id,
                    Is.EqualTo(marker.StockFlow.Session.ProcessorCoolerItemId));
                Assert.That(coolerItem.ProductId,
                    Is.EqualTo(marker.StockFlow.Session.ProcessorCoolerProductId));
                Assert.That(coolerItem.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(coolerItem.StateFlags,
                    Is.EqualTo(InventorySerializedItemStateFlags.None));
                Assert.That(marker.StockFlow.Session.AssemblyBuild
                        .ProcessorCoolerSlotState,
                    Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));
                OperationResult coolerProjectionInvariant =
                    coolerBinding.ValidateProjectionInvariant();
                Assert.That(coolerProjectionInvariant.IsSuccess,
                    Is.True,
                    coolerProjectionInvariant.IsFailure
                        ? coolerProjectionInvariant.Error.Code
                        : string.Empty);
                GraphicsCardAssemblyItemBinding graphicsCardBinding =
                    graphicsCard.GetComponent<GraphicsCardAssemblyItemBinding>();
                Assert.That(graphicsCardBinding, Is.Not.Null);
                Assert.That(graphicsCard.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.ProductDisplayName));
                Assert.That(graphicsCard.SupportsPlacement, Is.False);
                Assert.That(graphicsCard.Body.mass, Is.EqualTo(0.82f).Within(0.001f));
                Assert.That(Vector3.Distance(
                    graphicsCard.DropHalfExtents,
                    new Vector3(0.1425f, 0.032f, 0.0625f)),
                    Is.LessThan(0.0001f));
                Assert.That(marker.GraphicsCard, Is.SameAs(graphicsCard));
                Assert.That(marker.GraphicsCardBinding,
                    Is.SameAs(graphicsCardBinding));
                Assert.That(marker.GraphicsCardSlot, Is.Not.Null);
                Assert.That(marker.GraphicsCardSlot.IsConfigured, Is.True);
                Assert.That(marker.GraphicsCardSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.GraphicsCardSlotIdValue));
                Assert.That(marker.GraphicsCardSlot.LatchIdValue,
                    Is.EqualTo(GarageStockFlowSession.GraphicsCardLatchIdValue));
                Assert.That(marker.GraphicsCardSlot.RearBracketIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.GraphicsCardRearBracketIdValue));
                Assert.That(marker.GraphicsCardSlot.RearBracketFastenerIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession
                            .GraphicsCardBracketFastenerIdValue));
                Assert.That(marker.GraphicsCardSlot.FocusCollider.enabled,
                    Is.False);
                Assert.That(marker.GraphicsCardSlot.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(graphicsCardBinding.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(graphicsCardBinding.PhysicalItem,
                    Is.SameAs(graphicsCard));
                Assert.That(graphicsCardBinding.Slot,
                    Is.SameAs(marker.GraphicsCardSlot));
                Assert.That(graphicsCardBinding.InventoryItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession
                            .GraphicsCardAssemblyItemInstanceIdValue));
                Assert.That(marker.PlayerCarry.MatchesGraphicsCardConfiguration(
                    marker.GraphicsCardSlot,
                    graphicsCardBinding), Is.True);
                Assert.That(marker.HasGraphicsCardR28Runtime, Is.True);
                Assert.That(marker.StockFlow.Session.TryGetGraphicsCardAssemblyItem(
                    out InventoryItemRecord graphicsCardItem), Is.True);
                Assert.That(graphicsCardItem.Id,
                    Is.EqualTo(marker.StockFlow.Session.GraphicsCardAssemblyItemId));
                Assert.That(graphicsCardItem.ProductId,
                    Is.EqualTo(marker.StockFlow.Session.ProductId));
                Assert.That(graphicsCardItem.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.AssemblyBuild
                        .GraphicsCardSlotState,
                    Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
                OperationResult graphicsCardProjectionInvariant =
                    graphicsCardBinding.ValidateProjectionInvariant();
                Assert.That(graphicsCardProjectionInvariant.IsSuccess,
                    Is.True,
                    graphicsCardProjectionInvariant.IsFailure
                        ? graphicsCardProjectionInvariant.Error.Code
                        : string.Empty);
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
                Assert.That(workTicketStations.Length, Is.EqualTo(1));
                CustomPcWorkTicketStationProjection workTicketStation =
                    workTicketStations[0];
                Assert.That(marker.CustomPcWorkTicketStation,
                    Is.SameAs(workTicketStation));
                Assert.That(hud.CustomPcWorkTicketStation,
                    Is.SameAs(workTicketStation));
                Assert.That(workTicketStation.StationIdValue,
                    Is.EqualTo(
                        CustomPcWorkTicketStationProjection.PrototypeStationIdValue));
                Assert.That(workTicketStation.StockFlow, Is.SameAs(marker.StockFlow));
                Assert.That(workTicketStation.PlayerInput,
                    Is.SameAs(marker.PlayerInput));
                Assert.That(workTicketStation.PlayerMotor,
                    Is.SameAs(marker.PlayerMotor));
                Assert.That(workTicketStation.PlayerCarry,
                    Is.SameAs(marker.PlayerCarry));
                Assert.That(workTicketStation.PlayerCamera, Is.SameAs(camera));
                Assert.That(workTicketStation.isActiveAndEnabled, Is.True);
                Assert.That(camera.isActiveAndEnabled, Is.True);
                Assert.That(workTicketStation.InteractionCollider.isTrigger, Is.True);
                Assert.That(workTicketStation.InteractionCollider.gameObject.name,
                    Is.EqualTo("CustomPcWorkTicketFocusTarget"));
                Assert.That(workTicketStation.InteractionCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(workTicketStation.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(1));
                Assert.That(workTicketStation.StationStatusText.text,
                    Does.Contain("TEKLİF BEKLENİYOR"));
                Renderer workTicketTextRenderer =
                    workTicketStation.StationStatusText.GetComponent<Renderer>();
                Assert.That(workTicketTextRenderer, Is.Not.Null);
                Assert.That(workTicketTextRenderer.enabled, Is.True);
                Assert.That(workTicketTextRenderer.gameObject.activeInHierarchy, Is.True);
                Assert.That(
                    camera.cullingMask &
                    (1 << workTicketTextRenderer.gameObject.layer),
                    Is.Not.Zero);
                Assert.That(workTicketStation.InteractionRange,
                    Is.EqualTo(
                        CustomPcWorkTicketStationProjection.DefaultInteractionRange)
                        .Within(0.001f));
                Assert.That(workTicketStation.FocusDegrees,
                    Is.EqualTo(CustomPcWorkTicketStationProjection.DefaultFocusDegrees)
                        .Within(0.001f));
                Assert.That(motherboardBuildKits.Length, Is.EqualTo(1));
                MotherboardBuildKitProjection motherboardBuildKit =
                    motherboardBuildKits[0];
                Assert.That(marker.MotherboardBuildKit,
                    Is.SameAs(motherboardBuildKit));
                Assert.That(marker.HasMotherboardBuildKitR35Runtime, Is.True);
                Assert.That(motherboardBuildKit.IsCanonical, Is.True);
                Assert.That(motherboardBuildKit.IsConfigured, Is.True);
                Assert.That(motherboardBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        MotherboardBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(motherboardBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(motherboardBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        MotherboardBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(motherboardBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(motherboardBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(90f).Within(0.001f));
                Assert.That(motherboardBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(motherboardBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Collider workbenchTop = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Collider>(true))
                    .Single(collider => collider.name == "WorkbenchTop");
                Assert.That(motherboardBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f),
                    "Build Kit support must sit on, not overlap, the workbench top.");
                Assert.That(motherboardBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(motherboardBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(motherboardBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(motherboardBuildKit.GetComponentsInChildren<Collider>(true)
                    .Length, Is.EqualTo(1));
                Assert.That(motherboardBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(1.35f).Within(0.001f));
                Assert.That(motherboardBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.026f).Within(0.001f));
                Assert.That(motherboardBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(marker.MotherboardBinding.BuildKit,
                    Is.SameAs(motherboardBuildKit));
                Assert.That(marker.PlayerCarry.MatchesMotherboardBuildKitConfiguration(
                    motherboardBuildKit,
                    marker.MotherboardBinding), Is.True);
                Assert.That(processorBuildKits.Length, Is.EqualTo(1));
                ProcessorBuildKitProjection processorBuildKit =
                    processorBuildKits[0];
                Assert.That(marker.ProcessorBuildKit,
                    Is.SameAs(processorBuildKit));
                Assert.That(marker.HasProcessorBuildKitR36Runtime, Is.True);
                Assert.That(processorBuildKit.IsCanonical, Is.True);
                Assert.That(processorBuildKit.IsConfigured, Is.True);
                Assert.That(processorBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        ProcessorBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(processorBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(processorBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        ProcessorBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(processorBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(processorBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(90f).Within(0.001f));
                Assert.That(processorBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(processorBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(processorBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f),
                    "CPU Build Kit support must sit on, not overlap, the workbench top.");
                Assert.That(processorBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(processorBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(processorBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(processorBuildKit.GetComponentsInChildren<Collider>(true)
                    .Length, Is.EqualTo(1));
                Assert.That(processorBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(1.66f).Within(0.001f));
                Assert.That(processorBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(processorBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(marker.ProcessorBinding.BuildKit,
                    Is.SameAs(processorBuildKit));
                Assert.That(marker.PlayerCarry.MatchesProcessorBuildKitConfiguration(
                    processorBuildKit,
                    marker.ProcessorBinding), Is.True);
                Assert.That(memoryModuleBuildKits.Length, Is.EqualTo(1));
                MemoryModuleBuildKitProjection memoryModuleBuildKit =
                    memoryModuleBuildKits[0];
                Assert.That(marker.MemoryModuleBuildKit,
                    Is.SameAs(memoryModuleBuildKit));
                Assert.That(marker.HasMemoryModuleBuildKitR37Runtime, Is.True);
                Assert.That(memoryModuleBuildKit.IsCanonical, Is.True);
                Assert.That(memoryModuleBuildKit.IsConfigured, Is.True);
                Assert.That(memoryModuleBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        MemoryModuleBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(memoryModuleBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(memoryModuleBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        MemoryModuleBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(memoryModuleBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(memoryModuleBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(180f).Within(0.001f));
                Assert.That(memoryModuleBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(memoryModuleBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(memoryModuleBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f),
                    "DDR5 Build Kit support must sit on, not overlap, the workbench top.");
                Assert.That(memoryModuleBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(memoryModuleBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(memoryModuleBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(memoryModuleBuildKit.GetComponentsInChildren<Collider>(true)
                    .Length, Is.EqualTo(1));
                Assert.That(memoryModuleBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(1.90f).Within(0.001f));
                Assert.That(memoryModuleBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(memoryModuleBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(marker.DimmBinding.BuildKit,
                    Is.SameAs(memoryModuleBuildKit));
                Assert.That(marker.PlayerCarry.MatchesMemoryModuleBuildKitConfiguration(
                    memoryModuleBuildKit,
                    marker.DimmBinding), Is.True);
                Assert.That(storageBuildKits.Length, Is.EqualTo(1));
                StorageBuildKitProjection storageBuildKit = storageBuildKits[0];
                Assert.That(marker.StorageBuildKit, Is.SameAs(storageBuildKit));
                Assert.That(marker.HasStorageBuildKitR38Runtime, Is.True);
                Assert.That(storageBuildKit.IsCanonical, Is.True);
                Assert.That(storageBuildKit.IsConfigured, Is.True);
                Assert.That(storageBuildKit.ProjectionIdValue,
                    Is.EqualTo(StorageBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(storageBuildKit.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(storageBuildKit.Surface.SurfaceId,
                    Is.EqualTo(StorageBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(storageBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(storageBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(180f).Within(0.001f));
                Assert.That(storageBuildKit.SupportCollider.isTrigger, Is.False);
                Assert.That(storageBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(storageBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f));
                Assert.That(storageBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(storageBuildKit.GetComponentsInChildren<Collider>(true)
                    .Length, Is.EqualTo(1));
                Assert.That(storageBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(2.18f).Within(0.001f));
                Assert.That(storageBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(storageBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(marker.StorageBinding.BuildKit,
                    Is.SameAs(storageBuildKit));
                Assert.That(marker.PlayerCarry.MatchesStorageBuildKitConfiguration(
                    storageBuildKit,
                    marker.StorageBinding), Is.True);
                Assert.That(processorCoolerBuildKits.Length, Is.EqualTo(1));
                ProcessorCoolerBuildKitProjection processorCoolerBuildKit =
                    processorCoolerBuildKits[0];
                Assert.That(marker.ProcessorCoolerBuildKit,
                    Is.SameAs(processorCoolerBuildKit));
                Assert.That(marker.HasProcessorCoolerBuildKitR39Runtime, Is.True);
                Assert.That(marker.HasProcessorCoolerAssemblyHandoffR49Runtime,
                    Is.True);
                Assert.That(marker.HasGraphicsCardAssemblyHandoffR50Runtime,
                    Is.True);
                Assert.That(processorCoolerBuildKit.IsCanonical, Is.True);
                Assert.That(processorCoolerBuildKit.IsConfigured, Is.True);
                Assert.That(processorCoolerBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        ProcessorCoolerBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(processorCoolerBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(processorCoolerBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        ProcessorCoolerBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(processorCoolerBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(processorCoolerBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(90f).Within(0.001f));
                Assert.That(processorCoolerBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(
                    processorCoolerBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(processorCoolerBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f));
                Assert.That(processorCoolerBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(processorCoolerBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(processorCoolerBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(
                    processorCoolerBuildKit.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(1));
                Assert.That(processorCoolerBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(2.47f).Within(0.001f));
                Assert.That(processorCoolerBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(processorCoolerBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(marker.ProcessorCoolerBinding.BuildKit,
                    Is.SameAs(processorCoolerBuildKit));
                Assert.That(
                    marker.PlayerCarry
                        .MatchesProcessorCoolerBuildKitConfiguration(
                            processorCoolerBuildKit,
                            marker.ProcessorCoolerBinding),
                    Is.True);
                Assert.That(graphicsCardBuildKits.Length, Is.EqualTo(1));
                GraphicsCardBuildKitProjection graphicsCardBuildKit =
                    graphicsCardBuildKits[0];
                Assert.That(marker.GraphicsCardBuildKit,
                    Is.SameAs(graphicsCardBuildKit));
                Assert.That(marker.HasGraphicsCardBuildKitR40Runtime, Is.True);
                Assert.That(graphicsCardBuildKit.IsCanonical, Is.True);
                Assert.That(graphicsCardBuildKit.IsConfigured, Is.True);
                Assert.That(graphicsCardBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        GraphicsCardBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(graphicsCardBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(graphicsCardBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        GraphicsCardBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(graphicsCardBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(graphicsCardBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(180f).Within(0.001f));
                Assert.That(graphicsCardBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(graphicsCardBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(graphicsCardBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f));
                Assert.That(graphicsCardBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(graphicsCardBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(graphicsCardBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(
                    graphicsCardBuildKit.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(1));
                Assert.That(graphicsCardBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(2.85f).Within(0.001f));
                Assert.That(graphicsCardBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(graphicsCardBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(marker.GraphicsCardBinding.BuildKit,
                    Is.SameAs(graphicsCardBuildKit));
                Assert.That(
                    marker.PlayerCarry.MatchesGraphicsCardBuildKitConfiguration(
                        graphicsCardBuildKit,
                        marker.GraphicsCardBinding),
                    Is.True);
                Assert.That(powerSupplyBuildKits.Length, Is.EqualTo(1));
                PowerSupplyBuildKitProjection powerSupplyBuildKit =
                    powerSupplyBuildKits[0];
                Assert.That(marker.PowerSupplyBuildKit,
                    Is.SameAs(powerSupplyBuildKit));
                Assert.That(marker.HasPowerSupplyBuildKitR41Runtime, Is.True);
                Assert.That(powerSupplyBuildKit.IsCanonical, Is.True);
                Assert.That(powerSupplyBuildKit.IsConfigured, Is.True);
                Assert.That(powerSupplyBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        PowerSupplyBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(powerSupplyBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(powerSupplyBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        PowerSupplyBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(powerSupplyBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(powerSupplyBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(180f).Within(0.001f));
                Assert.That(powerSupplyBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(powerSupplyBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(powerSupplyBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f));
                Assert.That(powerSupplyBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(powerSupplyBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(powerSupplyBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(
                    powerSupplyBuildKit.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(1));
                Assert.That(powerSupplyBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(3.17f).Within(0.001f));
                Assert.That(powerSupplyBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(powerSupplyBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(
                    powerSupplyBuildKit.SupportCollider.bounds.Intersects(
                        graphicsCardBuildKit.SupportCollider.bounds),
                    Is.False);
                Assert.That(marker.PowerSupplyBinding.BuildKit,
                    Is.SameAs(powerSupplyBuildKit));
                Assert.That(
                    marker.PlayerCarry.MatchesPowerSupplyBuildKitConfiguration(
                        powerSupplyBuildKit,
                        marker.PowerSupplyBinding),
                    Is.True);
                Assert.That(atx24PowerCableBuildKits.Length, Is.EqualTo(1));
                Atx24PowerCableBuildKitProjection atx24PowerCableBuildKit =
                    atx24PowerCableBuildKits[0];
                Assert.That(marker.Atx24PowerCableBuildKit,
                    Is.SameAs(atx24PowerCableBuildKit));
                Assert.That(marker.HasAtx24PowerCableBuildKitR42Runtime, Is.True);
                Assert.That(atx24PowerCableBuildKit.IsCanonical, Is.True);
                Assert.That(atx24PowerCableBuildKit.IsConfigured, Is.True);
                Assert.That(atx24PowerCableBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        Atx24PowerCableBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(atx24PowerCableBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(atx24PowerCableBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        Atx24PowerCableBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(atx24PowerCableBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(atx24PowerCableBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(180f).Within(0.001f));
                Assert.That(atx24PowerCableBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(
                    atx24PowerCableBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(atx24PowerCableBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f));
                Assert.That(atx24PowerCableBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(atx24PowerCableBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(atx24PowerCableBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(
                    atx24PowerCableBuildKit.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(1));
                Assert.That(atx24PowerCableBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(3.43f).Within(0.001f));
                Assert.That(atx24PowerCableBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(atx24PowerCableBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(
                    atx24PowerCableBuildKit.SupportCollider.bounds.Intersects(
                        powerSupplyBuildKit.SupportCollider.bounds),
                    Is.False);
                Assert.That(marker.Atx24PowerCableBinding.BuildKit,
                    Is.SameAs(atx24PowerCableBuildKit));
                Assert.That(
                    marker.PlayerCarry
                        .MatchesAtx24PowerCableBuildKitConfiguration(
                            atx24PowerCableBuildKit,
                            marker.Atx24PowerCableBinding),
                    Is.True);
                Assert.That(eps12vPowerCableBuildKits.Length, Is.EqualTo(1));
                Eps12vPowerCableBuildKitProjection eps12vPowerCableBuildKit =
                    eps12vPowerCableBuildKits[0];
                Assert.That(marker.Eps12vPowerCableBuildKit,
                    Is.SameAs(eps12vPowerCableBuildKit));
                Assert.That(marker.HasEps12vPowerCableBuildKitR43Runtime, Is.True);
                Assert.That(eps12vPowerCableBuildKit.IsCanonical, Is.True);
                Assert.That(eps12vPowerCableBuildKit.IsConfigured, Is.True);
                Assert.That(eps12vPowerCableBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        Eps12vPowerCableBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(eps12vPowerCableBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(eps12vPowerCableBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        Eps12vPowerCableBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(eps12vPowerCableBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(eps12vPowerCableBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(180f).Within(0.001f));
                Assert.That(eps12vPowerCableBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(
                    eps12vPowerCableBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(eps12vPowerCableBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f));
                Assert.That(eps12vPowerCableBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(eps12vPowerCableBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(eps12vPowerCableBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(
                    eps12vPowerCableBuildKit.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(1));
                Assert.That(eps12vPowerCableBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(3.69f).Within(0.001f));
                Assert.That(eps12vPowerCableBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(eps12vPowerCableBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(
                    eps12vPowerCableBuildKit.SupportCollider.bounds.Intersects(
                        atx24PowerCableBuildKit.SupportCollider.bounds),
                    Is.False);
                Assert.That(marker.Eps12vPowerCableBinding.BuildKit,
                    Is.SameAs(eps12vPowerCableBuildKit));
                Assert.That(
                    marker.PlayerCarry
                        .MatchesEps12vPowerCableBuildKitConfiguration(
                            eps12vPowerCableBuildKit,
                            marker.Eps12vPowerCableBinding),
                    Is.True);
                Assert.That(pcieGpuPowerCableBuildKits.Length, Is.EqualTo(1));
                PcieGpuPowerCableBuildKitProjection pcieGpuPowerCableBuildKit =
                    pcieGpuPowerCableBuildKits[0];
                Assert.That(marker.PcieGpuPowerCableBuildKit,
                    Is.SameAs(pcieGpuPowerCableBuildKit));
                Assert.That(marker.HasPcieGpuPowerCableBuildKitR44Runtime, Is.True);
                Assert.That(pcieGpuPowerCableBuildKit.IsCanonical, Is.True);
                Assert.That(pcieGpuPowerCableBuildKit.IsConfigured, Is.True);
                Assert.That(pcieGpuPowerCableBuildKit.ProjectionIdValue,
                    Is.EqualTo(
                        PcieGpuPowerCableBuildKitProjection.PrototypeProjectionIdValue));
                Assert.That(pcieGpuPowerCableBuildKit.Runtime,
                    Is.SameAs(marker.StockFlow));
                Assert.That(pcieGpuPowerCableBuildKit.Surface.SurfaceId,
                    Is.EqualTo(
                        PcieGpuPowerCableBuildKitProjection.PrototypeSurfaceIdValue));
                Assert.That(pcieGpuPowerCableBuildKit.Surface.GridSize,
                    Is.EqualTo(0.01f).Within(0.001f));
                Assert.That(pcieGpuPowerCableBuildKit.Surface.YawStepDegrees,
                    Is.EqualTo(180f).Within(0.001f));
                Assert.That(pcieGpuPowerCableBuildKit.SupportCollider.isTrigger,
                    Is.False);
                Assert.That(
                    pcieGpuPowerCableBuildKit.SupportCollider.gameObject.layer,
                    Is.EqualTo(0));
                Assert.That(pcieGpuPowerCableBuildKit.SupportCollider.bounds.min.y,
                    Is.EqualTo(workbenchTop.bounds.max.y).Within(0.001f));
                Assert.That(pcieGpuPowerCableBuildKit.ProgressText.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(pcieGpuPowerCableBuildKit.ProgressText.text,
                    Does.Contain("BUILD KIT"));
                Assert.That(pcieGpuPowerCableBuildKit.ProgressText.text,
                    Does.Contain("İŞ EMRİ BEKLİYOR"));
                Assert.That(pcieGpuPowerCableBuildKit.StagedComponentCount,
                    Is.EqualTo(0));
                Assert.That(
                    pcieGpuPowerCableBuildKit.GetComponentsInChildren<Collider>(true)
                        .Length,
                    Is.EqualTo(1));
                Assert.That(pcieGpuPowerCableBuildKit.SnapAnchor.position.x,
                    Is.EqualTo(3.90f).Within(0.001f));
                Assert.That(pcieGpuPowerCableBuildKit.SnapAnchor.position.y,
                    Is.EqualTo(1.032f).Within(0.001f));
                Assert.That(pcieGpuPowerCableBuildKit.SnapAnchor.position.z,
                    Is.EqualTo(4.14f).Within(0.001f));
                Assert.That(
                    pcieGpuPowerCableBuildKit.SupportCollider.bounds.Intersects(
                        eps12vPowerCableBuildKit.SupportCollider.bounds),
                    Is.False);
                Assert.That(marker.PcieGpuPowerCableBinding.BuildKit,
                    Is.SameAs(pcieGpuPowerCableBuildKit));
                Assert.That(
                    marker.PlayerCarry
                        .MatchesPcieGpuPowerCableBuildKitConfiguration(
                            pcieGpuPowerCableBuildKit,
                            marker.PcieGpuPowerCableBinding),
                    Is.True);
                Assert.That(marker.StockFlow.ShelfOfferText, Is.Not.Null);
                Assert.That(marker.StockFlow.ShelfOfferText.text,
                    Is.EqualTo("RAF A • FİYAT YOK\nSEPET: BOŞ\nKASA: BEKLİYOR"));
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
                Assert.That(placementSurfaces.Length, Is.EqualTo(12));
                PlacementSurface floorSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId == "prototype.stock-floor-small-box-a");
                PlacementSurface shelfSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId == "prototype.retail-shelf-a");
                PlacementSurface buildKitSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId ==
                               MotherboardBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface processorBuildKitSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId ==
                               ProcessorBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface memoryModuleBuildKitSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId ==
                               MemoryModuleBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface storageBuildKitSurface = placementSurfaces.Single(
                    surface => surface.SurfaceId ==
                               StorageBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface processorCoolerBuildKitSurface =
                    placementSurfaces.Single(
                        surface => surface.SurfaceId ==
                            ProcessorCoolerBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface graphicsCardBuildKitSurface =
                    placementSurfaces.Single(
                        surface => surface.SurfaceId ==
                            GraphicsCardBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface powerSupplyBuildKitSurface =
                    placementSurfaces.Single(
                        surface => surface.SurfaceId ==
                            PowerSupplyBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface atx24PowerCableBuildKitSurface =
                    placementSurfaces.Single(
                        surface => surface.SurfaceId ==
                            Atx24PowerCableBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface eps12vPowerCableBuildKitSurface =
                    placementSurfaces.Single(
                        surface => surface.SurfaceId ==
                            Eps12vPowerCableBuildKitProjection.PrototypeSurfaceIdValue);
                PlacementSurface pcieGpuPowerCableBuildKitSurface =
                    placementSurfaces.Single(
                        surface => surface.SurfaceId ==
                            PcieGpuPowerCableBuildKitProjection.PrototypeSurfaceIdValue);
                Assert.That(floorSurface.GridSize, Is.EqualTo(0.25f).Within(0.001f));
                Assert.That(floorSurface.YawStepDegrees, Is.EqualTo(90f).Within(0.001f));
                Assert.That(shelfSurface.GridSize, Is.EqualTo(0.25f).Within(0.001f));
                Assert.That(buildKitSurface,
                    Is.SameAs(motherboardBuildKit.Surface));
                Assert.That(processorBuildKitSurface,
                    Is.SameAs(processorBuildKit.Surface));
                Assert.That(memoryModuleBuildKitSurface,
                    Is.SameAs(memoryModuleBuildKit.Surface));
                Assert.That(storageBuildKitSurface,
                    Is.SameAs(storageBuildKit.Surface));
                Assert.That(processorCoolerBuildKitSurface,
                    Is.SameAs(processorCoolerBuildKit.Surface));
                Assert.That(graphicsCardBuildKitSurface,
                    Is.SameAs(graphicsCardBuildKit.Surface));
                Assert.That(powerSupplyBuildKitSurface,
                    Is.SameAs(powerSupplyBuildKit.Surface));
                Assert.That(atx24PowerCableBuildKitSurface,
                    Is.SameAs(atx24PowerCableBuildKit.Surface));
                Assert.That(eps12vPowerCableBuildKitSurface,
                    Is.SameAs(eps12vPowerCableBuildKit.Surface));
                Assert.That(pcieGpuPowerCableBuildKitSurface,
                    Is.SameAs(pcieGpuPowerCableBuildKit.Surface));
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
        public void GarageSceneContainsPowerSupplyR29PhysicalMountContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                GaragePrototypeMarker marker = FindInScene<GaragePrototypeMarker>(scene);
                Assert.That(marker, Is.Not.Null);

                PowerSupplyBayProjection bay = marker.PowerSupplyBay;
                PowerSupplyAssemblyItemBinding binding = marker.PowerSupplyBinding;
                PhysicalItemProjection powerSupply = marker.PowerSupply;
                PowerSupplyRuntimeGeometry geometry = marker.PowerSupplyGeometry;

                Assert.That(GaragePrototypeMarker.Version,
                    Is.EqualTo("garage-quality-bound-physical-packaging-r68-v1"));
                Assert.That(marker.HasPowerSupplyR29Runtime, Is.True);
                Assert.That(marker.HasPowerSupplyBuildKitR41Runtime,
                    Is.True, "power-supply BuildKit runtime");
                Assert.That(binding.BuildKit, Is.SameAs(marker.PowerSupplyBuildKit),
                    "power-supply binding BuildKit wiring");
                Assert.That(Vector3.Dot(
                        bay.SnapAnchor.up,
                        bay.AssemblyRoot.up),
                    Is.GreaterThan(0.999f), "upright ATX support axis");
                Assert.That(Vector3.Dot(
                        bay.SnapAnchor.forward,
                        bay.AssemblyRoot.forward),
                    Is.GreaterThan(0.999f), "rear-mount insertion axis");
                Assert.That(bay, Is.Not.Null);
                Assert.That(bay.IsConfigured, Is.True);
                Assert.That(bay.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.PowerSupplyBaySlotIdValue));
                Assert.That(bay.RearMountIdValue,
                    Is.EqualTo(GarageStockFlowSession.PowerSupplyRearMountIdValue));
                Assert.That(bay.TopLeftFastenerIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.PowerSupplyTopLeftFastenerIdValue));
                Assert.That(bay.TopRightFastenerIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.PowerSupplyTopRightFastenerIdValue));
                Assert.That(bay.BottomLeftFastenerIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.PowerSupplyBottomLeftFastenerIdValue));
                Assert.That(bay.BottomRightFastenerIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.PowerSupplyBottomRightFastenerIdValue));
                Assert.That(bay.BayFormFactor,
                    Is.EqualTo(PowerSupplyFormFactor.AtxPs2));
                Assert.That(bay.SnapAnchor.name, Is.EqualTo("PowerSupplyBaySnapAnchor"));
                Assert.That(bay.FocusCollider.name,
                    Is.EqualTo("PowerSupplyBayFocusTarget"));
                Assert.That(bay.FocusCollider.isTrigger, Is.True);
                Assert.That(bay.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(bay.SupportCollider.name,
                    Is.EqualTo("PowerSupplyFilteredFloorIntake"));
                Assert.That(bay.SupportCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(bay.ChassisClearanceBlockers.Length, Is.EqualTo(4));
                Assert.That(
                    bay.ChassisClearanceBlockers.Select(collider => collider.name),
                    Is.EqualTo(new[]
                    {
                        "ChassisBack",
                        "ChassisLeftRail",
                        "ChassisRightRail",
                        "MotherboardTray"
                    }));
                Assert.That(
                    bay.ChassisClearanceBlockers.Distinct().Count(),
                    Is.EqualTo(4));
                Assert.That(
                    bay.ChassisClearanceBlockers,
                    Has.None.SameAs(bay.SupportCollider));
                Assert.That(bay.CableClearanceBlockers, Is.Empty);
                Assert.That(bay.FastenerPivots.Length, Is.EqualTo(4));
                Assert.That(bay.FastenerPivots.Distinct().Count(), Is.EqualTo(4));
                Assert.That(
                    bay.FastenerPivots.Select(pivot => pivot.name),
                    Is.EqualTo(new[]
                    {
                        "PowerSupplyRearFastenerPivot_1",
                        "PowerSupplyRearFastenerPivot_2",
                        "PowerSupplyRearFastenerPivot_3",
                        "PowerSupplyRearFastenerPivot_4"
                    }));

                Assert.That(binding, Is.Not.Null);
                Assert.That(binding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(binding.PhysicalItem, Is.SameAs(powerSupply));
                Assert.That(binding.Slot, Is.SameAs(bay));
                Assert.That(binding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.PowerSupplyItemInstanceIdValue));
                Assert.That(binding.IsAuthorityLooseWorld, Is.True);
                Assert.That(binding.ValidateProjectionInvariant().IsSuccess, Is.True);
                Assert.That(marker.PlayerCarry.MatchesPowerSupplyConfiguration(
                    bay,
                    binding), Is.True);
                PowerSupplyAssemblyItemBinding[] allPowerSupplyBindings = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PowerSupplyAssemblyItemBinding>(true))
                    .ToArray();
                Assert.That(allPowerSupplyBindings.Length, Is.EqualTo(1));
                Assert.That(allPowerSupplyBindings[0], Is.SameAs(binding));
                Assert.That(binding.gameObject, Is.SameAs(powerSupply.gameObject));

                Assert.That(powerSupply, Is.Not.Null);
                Assert.That(powerSupply.ItemIdValue,
                    Is.EqualTo(GarageStockFlowSession.PowerSupplyItemInstanceIdValue));
                Assert.That(powerSupply.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.PowerSupplyDisplayName));
                Assert.That(powerSupply.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(powerSupply.SupportsPlacement, Is.False);
                Assert.That(powerSupply.Body.mass, Is.EqualTo(2.35f).Within(0.001f));
                Assert.That(powerSupply.Body.isKinematic, Is.True);
                Assert.That(powerSupply.Body.useGravity, Is.False);
                Assert.That(Vector3.Distance(
                    powerSupply.DropHalfExtents,
                    new Vector3(0.075f, 0.043f, 0.070f)),
                    Is.LessThan(0.0001f));

                Assert.That(geometry, Is.Not.Null);
                Assert.That(geometry.IsCanonical, Is.True);
                Assert.That(geometry.Housing.name, Is.EqualTo("PowerSupplySteelHousing"));
                Assert.That(geometry.FanAndGrille.name,
                    Is.EqualTo("PowerSupplyIntakeFan"));
                Assert.That(geometry.FilteredFloorIntake,
                    Is.SameAs(bay.SupportCollider.transform));
                Assert.That(geometry.AcInlet.name, Is.EqualTo("PowerSupplyAcInlet"));
                Assert.That(geometry.RockerSwitch.name,
                    Is.EqualTo("PowerSupplyRockerSwitch"));
                Assert.That(geometry.ModularSocketPanel.name,
                    Is.EqualTo("PowerSupplyDisconnectedModularSocketPanel"));
                Assert.That(geometry.RearMountPlate.name,
                    Is.EqualTo("PowerSupplyRearMountPlate"));
                Assert.That(geometry.FastenerPivots,
                    Is.EqualTo(bay.FastenerPivots));

                Assert.That(marker.StockFlow.Session.TryGetPowerSupplyItem(
                    out InventoryItemRecord item), Is.True);
                Assert.That(item.Id,
                    Is.EqualTo(marker.StockFlow.Session.PowerSupplyItemId));
                Assert.That(item.ProductId,
                    Is.EqualTo(marker.StockFlow.Session.PowerSupplyProductId));
                Assert.That(item.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<PhysicalItemProjection>(true))
                    .Count(itemProjection => itemProjection.ItemIdValue ==
                        GarageStockFlowSession.PowerSupplyItemInstanceIdValue),
                    Is.EqualTo(1));
                Assert.That(marker.StockFlow.Session.ValidateInvariants().IsSuccess, Is.True);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsAtx24PowerCableR30PhysicalRouteContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                GaragePrototypeMarker marker = FindInScene<GaragePrototypeMarker>(scene);
                Assert.That(marker, Is.Not.Null);
                Assert.That(GaragePrototypeMarker.Version,
                    Is.EqualTo("garage-quality-bound-physical-packaging-r68-v1"));
                Assert.That(marker.HasAtx24PowerCableR30Runtime, Is.True);

                Atx24PowerCableRouteProjection route = marker.Atx24PowerCableRoute;
                Atx24PowerCableAssemblyItemBinding binding =
                    marker.Atx24PowerCableBinding;
                PhysicalItemProjection cable = marker.Atx24PowerCable;
                Atx24PowerCableRuntimeGeometry geometry =
                    marker.Atx24PowerCableGeometry;

                Assert.That(route, Is.Not.Null);
                Assert.That(route.IsConfigured, Is.True);
                Assert.That(route.RouteIdValue,
                    Is.EqualTo(GarageStockFlowSession.Atx24PowerCableRouteIdValue));
                Assert.That(route.PsuPrimaryEndpointIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.Atx24PowerCablePsuPrimaryEndpointIdValue));
                Assert.That(route.PsuSenseEndpointIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.Atx24PowerCablePsuSenseEndpointIdValue));
                Assert.That(route.MotherboardEndpointIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.Atx24PowerCableMotherboardEndpointIdValue));
                Assert.That(route.WaypointIdValues, Is.EqualTo(new[]
                {
                    GarageStockFlowSession.Atx24PowerCableWaypoint1IdValue,
                    GarageStockFlowSession.Atx24PowerCableWaypoint2IdValue,
                    GarageStockFlowSession.Atx24PowerCableWaypoint3IdValue
                }));
                Assert.That(route.Waypoints.Select(waypoint => waypoint.name),
                    Is.EqualTo(new[]
                    {
                        "Atx24WaypointPsuExit",
                        "Atx24WaypointRearChannel",
                        "Atx24WaypointBoardEntry"
                    }));
                Assert.That(route.Waypoints.Distinct().Count(), Is.EqualTo(3));
                Assert.That(route.PsuPrimaryEndpoint.name,
                    Is.EqualTo("Atx24PsuPrimary18Anchor"));
                Assert.That(route.PsuSenseEndpoint.name,
                    Is.EqualTo("Atx24PsuSense10Anchor"));
                Assert.That(route.MotherboardEndpoint.name,
                    Is.EqualTo("MotherboardAtx24PowerHeader"));
                Assert.That(route.PsuPrimaryEndpoint.parent.name,
                    Is.EqualTo("PowerSupplyDisconnectedModularSocketPanel"));
                Assert.That(route.PsuSenseEndpoint.parent,
                    Is.SameAs(route.PsuPrimaryEndpoint.parent));
                Assert.That(route.MotherboardEndpoint.IsChildOf(
                    marker.MotherboardBinding.PhysicalItem.transform), Is.True);
                Assert.That(route.FocusCollider.isTrigger, Is.True);
                Assert.That(route.FocusCollider.enabled, Is.False);
                Assert.That(route.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(route.PreviewLines.Length, Is.EqualTo(3));
                Assert.That(route.PreviewLines.All(line => !line.enabled), Is.True);
                Assert.That(route.PreviewLines.All(line =>
                    line.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")),
                    Is.True);

                Assert.That(binding, Is.Not.Null);
                Assert.That(binding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(binding.PhysicalItem, Is.SameAs(cable));
                Assert.That(binding.Route, Is.SameAs(route));
                Assert.That(binding.Geometry, Is.SameAs(geometry));
                Assert.That(binding.InventoryItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.Atx24PowerCableItemInstanceIdValue));
                Assert.That(binding.IsAuthorityLooseWorld, Is.True);
                Assert.That(binding.IsRouted, Is.False);
                Assert.That(binding.ValidateProjectionInvariant().IsSuccess, Is.True);
                Assert.That(marker.PlayerCarry.MatchesAtx24PowerCableConfiguration(
                    route,
                    binding), Is.True);

                Assert.That(cable.ItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.Atx24PowerCableItemInstanceIdValue));
                Assert.That(cable.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.Atx24PowerCableDisplayName));
                Assert.That(cable.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(cable.SupportsPlacement, Is.False);
                Assert.That(cable.Body.mass, Is.EqualTo(0.32f).Within(0.001f));
                Assert.That(cable.Body.isKinematic, Is.True);
                Assert.That(cable.Body.useGravity, Is.False);
                Assert.That(cable.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(1));
                Assert.That(cable.GetComponentsInChildren<Joint>(true), Is.Empty);
                Assert.That(cable.GetComponentsInChildren<Rigidbody>(true).Length,
                    Is.EqualTo(1));

                Assert.That(geometry, Is.Not.Null);
                Assert.That(geometry.IsCanonical, Is.True);
                Assert.That(geometry.IsRouted, Is.False);
                Assert.That(geometry.PsuPrimary18Connector.name,
                    Is.EqualTo("Atx24PsuPrimary18Connector"));
                Assert.That(geometry.PsuSense10Connector.name,
                    Is.EqualTo("Atx24PsuSense10Connector"));
                Assert.That(geometry.Motherboard24Connector.name,
                    Is.EqualTo("Atx24Motherboard24Connector"));
                Assert.That(geometry.LooseCoil.name,
                    Is.EqualTo("Atx24LooseBraidedCoil"));
                Assert.That(geometry.LooseCoil.enabled, Is.True);
                Assert.That(new[]
                    {
                        geometry.LooseCoil,
                        geometry.PsuPrimaryBranch,
                        geometry.PsuSenseBranch,
                        geometry.RoutedTrunk
                    }.All(line =>
                        line.gameObject.layer == LayerMask.NameToLayer("Ignore Raycast")),
                    Is.True);

                Atx24PowerCableAssemblyItemBinding[] bindings = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        Atx24PowerCableAssemblyItemBinding>(true))
                    .ToArray();
                Assert.That(bindings.Length, Is.EqualTo(1));
                Assert.That(bindings[0], Is.SameAs(binding));
                Assert.That(scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PhysicalItemProjection>(true))
                    .Count(item => item.ItemIdValue ==
                        GarageStockFlowSession.Atx24PowerCableItemInstanceIdValue),
                    Is.EqualTo(1));

                Assert.That(marker.StockFlow.Session.TryGetAtx24PowerCableItem(
                    out InventoryItemRecord item), Is.True);
                Assert.That(item.Id,
                    Is.EqualTo(marker.StockFlow.Session.Atx24PowerCableItemId));
                Assert.That(item.ProductId,
                    Is.EqualTo(marker.StockFlow.Session.Atx24PowerCableProductId));
                Assert.That(item.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.ValidateInvariants().IsSuccess,
                    Is.True);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsEps12vPowerCableR31PhysicalRouteContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                GaragePrototypeMarker marker =
                    FindInScene<GaragePrototypeMarker>(scene);
                Assert.That(marker, Is.Not.Null);
                Assert.That(GaragePrototypeMarker.Version,
                    Is.EqualTo("garage-quality-bound-physical-packaging-r68-v1"));
                Assert.That(marker.HasEps12vPowerCableR31Runtime, Is.True);

                Eps12vPowerCableRouteProjection route =
                    marker.Eps12vPowerCableRoute;
                Eps12vPowerCableAssemblyItemBinding binding =
                    marker.Eps12vPowerCableBinding;
                PhysicalItemProjection cable = marker.Eps12vPowerCable;
                Eps12vPowerCableRuntimeGeometry geometry =
                    marker.Eps12vPowerCableGeometry;

                Assert.That(route, Is.Not.Null);
                Assert.That(route.IsConfigured, Is.True);
                Assert.That(route.RouteIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.Eps12vPowerCableRouteIdValue));
                Assert.That(route.PsuEndpointIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .Eps12vPowerCablePsuEndpointIdValue));
                Assert.That(route.MotherboardEndpointIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .Eps12vPowerCableMotherboardEndpointIdValue));
                Assert.That(route.WaypointIdValues, Is.EqualTo(new[]
                {
                    GarageStockFlowSession.Eps12vPowerCableWaypoint1IdValue,
                    GarageStockFlowSession.Eps12vPowerCableWaypoint2IdValue,
                    GarageStockFlowSession.Eps12vPowerCableWaypoint3IdValue
                }));
                Assert.That(route.Waypoints.Select(waypoint => waypoint.name),
                    Is.EqualTo(new[]
                    {
                        "Eps12vWaypointPsuExit",
                        "Eps12vWaypointRearChannel",
                        "Eps12vWaypointBoardEntry"
                    }));
                Assert.That(route.Waypoints.Distinct().Count(), Is.EqualTo(3));
                Assert.That(route.PsuEndpoint.name,
                    Is.EqualTo("Eps12vPsuCpu8Anchor"));
                Assert.That(route.PsuEndpoint.parent.name,
                    Is.EqualTo("PowerSupplyDisconnectedModularSocketPanel"));
                Assert.That(route.MotherboardEndpoint.name,
                    Is.EqualTo("MotherboardEps12vCpuPowerHeader"));
                Assert.That(route.MotherboardEndpoint.IsChildOf(
                    marker.MotherboardBinding.PhysicalItem.transform), Is.True);
                Assert.That(route.FocusCollider.name,
                    Is.EqualTo("MotherboardEps12vRouteFocusTarget"));
                Assert.That(route.FocusCollider.isTrigger, Is.True);
                Assert.That(route.FocusCollider.enabled, Is.False);
                Assert.That(route.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(route.PreviewLine.enabled, Is.False);
                Assert.That(route.PreviewLine.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(route.AllowedRouteColliders.Length, Is.EqualTo(2));
                Assert.That(route.AllowedRouteColliders,
                    Does.Contain(marker.PowerSupply.GetComponent<Collider>()));
                Assert.That(route.AllowedRouteColliders,
                    Does.Contain(marker.MotherboardBinding.PhysicalItem.transform
                        .Find("MotherboardPcb").GetComponent<Collider>()));

                Assert.That(binding, Is.Not.Null);
                Assert.That(binding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(binding.PhysicalItem, Is.SameAs(cable));
                Assert.That(binding.Route, Is.SameAs(route));
                Assert.That(binding.Geometry, Is.SameAs(geometry));
                Assert.That(binding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .Eps12vPowerCableItemInstanceIdValue));
                Assert.That(binding.IsAuthorityLooseWorld, Is.True);
                Assert.That(binding.IsRouted, Is.False);
                Assert.That(binding.ValidateProjectionInvariant().IsSuccess,
                    Is.True);
                Assert.That(marker.PlayerCarry
                    .MatchesEps12vPowerCableConfiguration(route, binding),
                    Is.True);

                Assert.That(cable.ItemIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .Eps12vPowerCableItemInstanceIdValue));
                Assert.That(cable.DisplayName,
                    Is.EqualTo(
                        GarageStockFlowSession.Eps12vPowerCableDisplayName));
                Assert.That(cable.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(cable.SupportsPlacement, Is.False);
                Assert.That(cable.Body.mass, Is.EqualTo(0.24f).Within(0.001f));
                Assert.That(cable.Body.isKinematic, Is.True);
                Assert.That(cable.Body.useGravity, Is.False);
                Assert.That(cable.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(1));
                Assert.That(cable.GetComponentsInChildren<Joint>(true), Is.Empty);
                Assert.That(cable.GetComponentsInChildren<Rigidbody>(true).Length,
                    Is.EqualTo(1));

                Assert.That(geometry, Is.Not.Null);
                Assert.That(geometry.IsCanonical, Is.True);
                Assert.That(geometry.IsRouted, Is.False);
                Assert.That(geometry.Psu8Connector.name,
                    Is.EqualTo("Eps12vPsuCpu8Connector"));
                Assert.That(geometry.Motherboard8Connector.name,
                    Is.EqualTo("Eps12vMotherboardCpu8Connector"));
                Assert.That(geometry.LooseCoil.name,
                    Is.EqualTo("Eps12vLooseBraidedCoil"));
                Assert.That(geometry.LooseCoil.enabled, Is.True);
                Assert.That(geometry.RoutedTrunk.enabled, Is.False);
                Assert.That(new[]
                    {
                        geometry.LooseCoil,
                        geometry.RoutedTrunk
                    }.All(line => line.gameObject.layer ==
                        LayerMask.NameToLayer("Ignore Raycast")),
                    Is.True);

                Eps12vPowerCableAssemblyItemBinding[] bindings = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        Eps12vPowerCableAssemblyItemBinding>(true))
                    .ToArray();
                Assert.That(bindings.Length, Is.EqualTo(1));
                Assert.That(bindings[0], Is.SameAs(binding));
                Assert.That(scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PhysicalItemProjection>(true))
                    .Count(item => item.ItemIdValue ==
                        GarageStockFlowSession
                            .Eps12vPowerCableItemInstanceIdValue),
                    Is.EqualTo(1));

                Assert.That(marker.Atx24PowerCableRoute.IsConfigured, Is.True);
                Assert.That(marker.ProcessorSocket.IsConfigured, Is.True);
                Assert.That(marker.ProcessorCoolerSlot.IsConfigured, Is.True);
                Assert.That(marker.StockFlow.Session.TryGetEps12vPowerCableItem(
                    out InventoryItemRecord item), Is.True);
                Assert.That(item.Id,
                    Is.EqualTo(marker.StockFlow.Session.Eps12vPowerCableItemId));
                Assert.That(item.ProductId,
                    Is.EqualTo(
                        marker.StockFlow.Session.Eps12vPowerCableProductId));
                Assert.That(item.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.ValidateInvariants().IsSuccess,
                    Is.True);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsPcieGpuPowerCableR32PhysicalRouteContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                GaragePrototypeMarker marker =
                    FindInScene<GaragePrototypeMarker>(scene);
                Assert.That(marker, Is.Not.Null);
                Assert.That(GaragePrototypeMarker.Version,
                    Is.EqualTo("garage-quality-bound-physical-packaging-r68-v1"));
                Assert.That(marker.HasPcieGpuPowerCableR32Runtime, Is.True);

                PcieGpuPowerCableRouteProjection route =
                    marker.PcieGpuPowerCableRoute;
                PcieGpuPowerCableAssemblyItemBinding binding =
                    marker.PcieGpuPowerCableBinding;
                PhysicalItemProjection cable = marker.PcieGpuPowerCable;
                PcieGpuPowerCableRuntimeGeometry geometry =
                    marker.PcieGpuPowerCableGeometry;

                Assert.That(route, Is.Not.Null);
                Assert.That(route.IsConfigured, Is.True);
                Assert.That(route.RouteIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.PcieGpuPowerCableRouteIdValue));
                Assert.That(route.PsuEndpointIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .PcieGpuPowerCablePsuEndpointIdValue));
                Assert.That(route.GraphicsCardEndpointIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .PcieGpuPowerCableGraphicsCardEndpointIdValue));
                Assert.That(route.WaypointIdValues, Is.EqualTo(new[]
                {
                    GarageStockFlowSession.PcieGpuPowerCableWaypoint1IdValue,
                    GarageStockFlowSession.PcieGpuPowerCableWaypoint2IdValue,
                    GarageStockFlowSession.PcieGpuPowerCableWaypoint3IdValue
                }));
                Assert.That(route.Waypoints.Select(waypoint => waypoint.name),
                    Is.EqualTo(new[]
                    {
                        "PcieGpuWaypointPsuExit",
                        "PcieGpuWaypointRearChannel",
                        "PcieGpuWaypointGpuEntry"
                    }));
                Assert.That(route.Waypoints.Distinct().Count(), Is.EqualTo(3));
                Assert.That(route.PsuEndpoint.name,
                    Is.EqualTo("PcieGpuPsuGpu8Anchor"));
                Assert.That(route.PsuEndpoint.parent.name,
                    Is.EqualTo("PowerSupplyDisconnectedModularSocketPanel"));
                Assert.That(route.GraphicsCardEndpoint.name,
                    Is.EqualTo("PcieGpuGraphicsCard8Anchor"));
                Assert.That(route.GraphicsCardEndpoint.IsChildOf(
                    marker.GraphicsCard.transform), Is.True);
                Assert.That(route.PowerSupplyHostRoot,
                    Is.SameAs(marker.PowerSupply.transform));
                Assert.That(route.GraphicsCardHostRoot,
                    Is.SameAs(marker.GraphicsCard.transform));
                Assert.That(route.FocusCollider.name,
                    Is.EqualTo("PcieGpuGraphicsCardRouteFocusTarget"));
                Assert.That(route.FocusCollider.isTrigger, Is.True);
                Assert.That(route.FocusCollider.enabled, Is.False);
                Assert.That(route.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(route.PreviewLine.enabled, Is.False);
                Assert.That(route.PreviewLine.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(route.AllowedRouteColliders.Length, Is.EqualTo(2));
                Assert.That(route.AllowedRouteColliders,
                    Does.Contain(marker.PowerSupply.GetComponent<Collider>()));
                Assert.That(route.AllowedRouteColliders,
                    Does.Contain(marker.GraphicsCard.GetComponent<Collider>()));

                Assert.That(binding, Is.Not.Null);
                Assert.That(binding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(binding.PhysicalItem, Is.SameAs(cable));
                Assert.That(binding.Route, Is.SameAs(route));
                Assert.That(binding.Geometry, Is.SameAs(geometry));
                Assert.That(binding.InventoryItemIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .PcieGpuPowerCableItemInstanceIdValue));
                Assert.That(binding.IsAuthorityLooseWorld, Is.True);
                Assert.That(binding.IsRouted, Is.False);
                Assert.That(binding.ValidateProjectionInvariant().IsSuccess,
                    Is.True);
                Assert.That(marker.PlayerCarry
                    .MatchesPcieGpuPowerCableConfiguration(route, binding),
                    Is.True);

                Assert.That(cable.ItemIdValue,
                    Is.EqualTo(GarageStockFlowSession
                        .PcieGpuPowerCableItemInstanceIdValue));
                Assert.That(cable.DisplayName,
                    Is.EqualTo(
                        GarageStockFlowSession.PcieGpuPowerCableDisplayName));
                Assert.That(cable.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(cable.SupportsPlacement, Is.False);
                Assert.That(cable.Body.mass, Is.EqualTo(0.24f).Within(0.001f));
                Assert.That(cable.Body.isKinematic, Is.True);
                Assert.That(cable.Body.useGravity, Is.False);
                Assert.That(cable.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(1));
                Assert.That(cable.GetComponentsInChildren<Joint>(true), Is.Empty);
                Assert.That(cable.GetComponentsInChildren<Rigidbody>(true).Length,
                    Is.EqualTo(1));

                Assert.That(geometry, Is.Not.Null);
                Assert.That(geometry.IsCanonical, Is.True);
                Assert.That(geometry.IsRouted, Is.False);
                Assert.That(geometry.Psu8Connector.name,
                    Is.EqualTo("PcieGpuPsuGpu8Connector"));
                Assert.That(geometry.GraphicsCard8Connector.name,
                    Is.EqualTo("PcieGpuGraphicsCardGpu8Connector"));
                Assert.That(geometry.Psu8Connector.Find(
                    "PcieGpuPsuGpu8ConnectorHousing"), Is.Not.Null);
                Assert.That(geometry.Psu8Connector.Find(
                    "PcieGpuPsuGpu8ConnectorPinCount_8"), Is.Not.Null);

                Transform gpuSixPinHousing =
                    geometry.GraphicsCard8Connector.Find(
                        "PcieGpuGraphicsCardGpu8ConnectorSixPinHousing");
                Transform gpuTwoPinHousing =
                    geometry.GraphicsCard8Connector.Find(
                        "PcieGpuGraphicsCardGpu8ConnectorTwoPinHousing");
                Assert.That(gpuSixPinHousing, Is.Not.Null);
                Assert.That(gpuTwoPinHousing, Is.Not.Null);
                Assert.That(
                    gpuSixPinHousing.GetComponent<Renderer>().bounds.size.x,
                    Is.GreaterThan(gpuTwoPinHousing
                        .GetComponent<Renderer>().bounds.size.x));
                Assert.That(gpuSixPinHousing.localPosition.x,
                    Is.LessThan(gpuTwoPinHousing.localPosition.x));
                Assert.That(geometry.GraphicsCard8Connector.Find(
                    "PcieGpuGraphicsCardGpu8ConnectorSixPinKeyedLatch"),
                    Is.Not.Null);
                Assert.That(geometry.GraphicsCard8Connector.Find(
                    "PcieGpuGraphicsCardGpu8ConnectorTwoPinRetentionClip"),
                    Is.Not.Null);
                TextMesh gpuSixPinLabel = geometry.GraphicsCard8Connector.Find(
                        "PcieGpuGraphicsCardGpu8ConnectorPinCount_6")
                    .GetComponent<TextMesh>();
                TextMesh gpuTwoPinLabel = geometry.GraphicsCard8Connector.Find(
                        "PcieGpuGraphicsCardGpu8ConnectorPinCount_2")
                    .GetComponent<TextMesh>();
                Assert.That(gpuSixPinLabel.text, Is.EqualTo("6"));
                Assert.That(gpuTwoPinLabel.text, Is.EqualTo("2"));
                Assert.That(geometry.GraphicsCard8Connector
                    .GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(geometry.LooseCoil.name,
                    Is.EqualTo("PcieGpuLooseBraidedCoil"));
                Assert.That(geometry.LooseCoil.enabled, Is.True);
                Assert.That(geometry.RoutedTrunk.enabled, Is.False);
                Assert.That(new[]
                    {
                        geometry.LooseCoil,
                        geometry.RoutedTrunk
                    }.All(line => line.gameObject.layer ==
                        LayerMask.NameToLayer("Ignore Raycast")),
                    Is.True);

                PcieGpuPowerCableAssemblyItemBinding[] bindings = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PcieGpuPowerCableAssemblyItemBinding>(true))
                    .ToArray();
                Assert.That(bindings.Length, Is.EqualTo(1));
                Assert.That(bindings[0], Is.SameAs(binding));
                Assert.That(scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        PhysicalItemProjection>(true))
                    .Count(item => item.ItemIdValue ==
                        GarageStockFlowSession
                            .PcieGpuPowerCableItemInstanceIdValue),
                    Is.EqualTo(1));

                Assert.That(marker.Atx24PowerCableRoute.IsConfigured, Is.True);
                Assert.That(marker.Eps12vPowerCableRoute.IsConfigured, Is.True);
                Assert.That(marker.StockFlow.Session.TryGetPcieGpuPowerCableItem(
                    out InventoryItemRecord item), Is.True);
                Assert.That(item.Id,
                    Is.EqualTo(marker.StockFlow.Session.PcieGpuPowerCableItemId));
                Assert.That(item.ProductId,
                    Is.EqualTo(
                        marker.StockFlow.Session.PcieGpuPowerCableProductId));
                Assert.That(item.ContainerId,
                    Is.EqualTo(marker.StockFlow.Session.WorldFloorContainerId));
                Assert.That(marker.StockFlow.Session.Inventory.SerializedItemCount,
                    Is.EqualTo(10));
                Assert.That(marker.StockFlow.Session.ValidateInvariants().IsSuccess,
                    Is.True);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsAssemblyWorkbenchHeroReadabilityContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                Transform[] transforms = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .ToArray();
                Transform heroRoot = transforms.Single(transform =>
                    transform.name == "AssemblyWorkbenchHeroReadability");
                Renderer[] heroRenderers = heroRoot.GetComponentsInChildren<Renderer>(true);

                Assert.That(
                    GaragePrototypeMarker.Version,
                    Is.EqualTo("garage-quality-bound-physical-packaging-r68-v1"));
                Assert.That(heroRenderers.Length, Is.EqualTo(4));
                Assert.That(heroRoot.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(heroRoot.GetComponentsInChildren<Light>(true), Is.Empty);
                Assert.That(
                    heroRenderers.Select(renderer => renderer.name),
                    Is.EquivalentTo(new[]
                    {
                        "AssemblyWorkbenchEsdMat",
                        "AssemblyWorkbenchSplashback",
                        "AssemblyWorkbenchZoneAccent",
                        "AssemblyCableRouteReferenceStrip"
                    }));
                Assert.That(
                    heroRenderers.All(renderer =>
                        renderer.gameObject.layer ==
                        LayerMask.NameToLayer("Ignore Raycast")),
                    Is.True);
                Assert.That(
                    heroRenderers.All(renderer =>
                        renderer.shadowCastingMode == ShadowCastingMode.Off &&
                        !renderer.receiveShadows &&
                        renderer.motionVectorGenerationMode ==
                        MotionVectorGenerationMode.ForceNoMotion),
                    Is.True);

                Renderer esdMat = heroRenderers.Single(renderer =>
                    renderer.name == "AssemblyWorkbenchEsdMat");
                Renderer splashback = heroRenderers.Single(renderer =>
                    renderer.name == "AssemblyWorkbenchSplashback");
                Renderer zoneAccent = heroRenderers.Single(renderer =>
                    renderer.name == "AssemblyWorkbenchZoneAccent");
                Renderer routeReference = heroRenderers.Single(renderer =>
                    renderer.name == "AssemblyCableRouteReferenceStrip");
                Assert.That(esdMat.sharedMaterial.name,
                    Does.StartWith("WorkshopRubber"));
                Assert.That(splashback.sharedMaterial.name,
                    Does.StartWith("Concrete"));
                Assert.That(zoneAccent.sharedMaterial.name,
                    Does.StartWith("SafetyAccent"));
                Assert.That(routeReference.sharedMaterial.name,
                    Does.StartWith("SafetyAccent"));
                Assert.That(Vector3.Distance(
                    esdMat.transform.localPosition,
                    new Vector3(-0.50f, 0.993f, 4.28f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    splashback.transform.localPosition,
                    new Vector3(-0.68f, 1.36f, 4.718f)), Is.LessThan(0.0001f));

                Renderer workbenchTop = transforms
                    .Single(transform => transform.name == "WorkbenchTop")
                    .GetComponent<Renderer>();
                Renderer chassisBase = transforms
                    .Single(transform => transform.name == "ChassisBase")
                    .GetComponent<Renderer>();
                Renderer chassisTopRail = transforms
                    .Single(transform => transform.name == "ChassisTopRail")
                    .GetComponent<Renderer>();
                Renderer motherboardTray = transforms
                    .Single(transform => transform.name == "MotherboardTray")
                    .GetComponent<Renderer>();
                Renderer motherboardPcb = transforms
                    .Single(transform => transform.name == "MotherboardPcb")
                    .GetComponent<Renderer>();
                Renderer pegboard = transforms
                    .Single(transform => transform.name == "Pegboard")
                    .GetComponent<Renderer>();
                Renderer[] mattePolymerRenderers = transforms
                    .Where(transform => new[]
                    {
                        "PcieGpuPsuGpu8ConnectorHousing",
                        "PcieGpuGraphicsCardGpu8ConnectorSixPinHousing",
                        "PcieGpuGraphicsCardGpu8ConnectorTwoPinHousing",
                        "PcieGpuGraphicsCard8HeaderHousing",
                        "PowerSupplyFilteredFloorIntake"
                    }.Contains(transform.name))
                    .Select(transform => transform.GetComponent<Renderer>())
                    .ToArray();
                Transform diagnosticMonitor = transforms.Single(transform =>
                    transform.name == "DiagnosticMonitorBody");
                Transform workTicketStation = transforms.Single(transform =>
                    transform.name == "CustomPcWorkTicketStation");
                Assert.That(workbenchTop.sharedMaterial.name,
                    Does.StartWith("WoodLaminate"));
                Assert.That(chassisBase.sharedMaterial.name,
                    Does.StartWith("DarkMetal"));
                Assert.That(chassisTopRail.sharedMaterial.name,
                    Does.StartWith("BrushedSteel"));
                Assert.That(motherboardTray.sharedMaterial.name,
                    Does.StartWith("DarkMetal"));
                Assert.That(motherboardPcb.sharedMaterial.name,
                    Does.StartWith("MotherboardPcb"));
                Assert.That(pegboard.sharedMaterial.name,
                    Does.StartWith("WarmWall"));
                Assert.That(mattePolymerRenderers.Length, Is.EqualTo(5));
                Assert.That(
                    mattePolymerRenderers.All(renderer =>
                        renderer != null &&
                        renderer.sharedMaterial.name.StartsWith(
                            "CableConnectorPolymer",
                            StringComparison.Ordinal) &&
                        renderer.sharedMaterial.shader != null &&
                        renderer.sharedMaterial.shader.name ==
                        "Universal Render Pipeline/Unlit"),
                    Is.True);
                Renderer[] graphicsCardFanBlades = transforms
                    .Where(transform =>
                        transform.name.StartsWith(
                            "GraphicsCardFan",
                            StringComparison.Ordinal) &&
                        transform.name.Contains("Blade_"))
                    .Select(transform => transform.GetComponent<Renderer>())
                    .ToArray();
                Assert.That(graphicsCardFanBlades.Length, Is.EqualTo(14));
                Assert.That(
                    graphicsCardFanBlades.All(renderer =>
                        renderer != null &&
                        renderer.sharedMaterial.name.StartsWith(
                            "CableConnectorPolymer",
                            StringComparison.Ordinal) &&
                        renderer.sharedMaterial.shader.name ==
                        "Universal Render Pipeline/Unlit"),
                    Is.True);
                Renderer[] graphicsCardBrackets = transforms
                    .Where(transform =>
                        transform.name == "GraphicsCardRearBracketPlate" ||
                        transform.name == "GraphicsCardIoRearBracket")
                    .Select(transform => transform.GetComponent<Renderer>())
                    .ToArray();
                Assert.That(graphicsCardBrackets.Length, Is.EqualTo(2));
                Assert.That(
                    graphicsCardBrackets.All(renderer =>
                        renderer != null &&
                        renderer.sharedMaterial.name.StartsWith(
                            "WorkshopMatteHardware",
                            StringComparison.Ordinal) &&
                        renderer.sharedMaterial.shader.name ==
                        "Universal Render Pipeline/Unlit"),
                    Is.True);
                Assert.That(diagnosticMonitor.localPosition.x,
                    Is.EqualTo(1.35f).Within(0.0001f));
                Assert.That(Vector3.Distance(
                    workTicketStation.localPosition,
                    new Vector3(-3.35f, 0f, 4.78f)), Is.LessThan(0.0001f));

                Material wood = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/WoodLaminate.mat");
                Material rubber = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/WorkshopRubber.mat");
                Material pcb = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/MotherboardPcb.mat");
                Material connectorPolymer = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/CableConnectorPolymer.mat");
                Material matteHardware = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/Art/Prototype/Materials/WorkshopMatteHardware.mat");
                Assert.That(wood, Is.Not.Null);
                Assert.That(rubber, Is.Not.Null);
                Assert.That(pcb, Is.Not.Null);
                Assert.That(connectorPolymer, Is.Not.Null);
                Assert.That(matteHardware, Is.Not.Null);
                Assert.That(wood.GetColor("_BaseColor").r,
                    Is.EqualTo(0.55f).Within(0.001f));
                Assert.That(rubber.GetColor("_BaseColor").b,
                    Is.EqualTo(0.085f).Within(0.001f));
                Assert.That(pcb.GetColor("_BaseColor").g,
                    Is.EqualTo(0.22f).Within(0.001f));
                Assert.That(connectorPolymer.GetColor("_BaseColor").b,
                    Is.EqualTo(0.030f).Within(0.001f));
                Assert.That(connectorPolymer.shader, Is.Not.Null);
                Assert.That(connectorPolymer.shader.name,
                    Is.EqualTo("Universal Render Pipeline/Unlit"));
                Assert.That(connectorPolymer.IsKeywordEnabled("_EMISSION"),
                    Is.False);
                Assert.That(matteHardware.shader, Is.Not.Null);
                Assert.That(matteHardware.shader.name,
                    Is.EqualTo("Universal Render Pipeline/Unlit"));
                Assert.That(matteHardware.GetColor("_BaseColor").b,
                    Is.EqualTo(0.20f).Within(0.001f));
                Assert.That(matteHardware.IsKeywordEnabled("_EMISSION"),
                    Is.False);

                MeshRenderer[] sceneRenderers = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<MeshRenderer>(true))
                    .ToArray();
                Light[] sceneLights = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Light>(true))
                    .ToArray();
                Camera[] sceneCameras = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Camera>(true))
                    .ToArray();
                Transform retailHeroRoot = transforms.SingleOrDefault(transform =>
                    transform.name == "RetailCheckoutHeroReadability");
                Assert.That(
                    sceneRenderers.Count(renderer =>
                        retailHeroRoot == null ||
                        !renderer.transform.IsChildOf(retailHeroRoot)),
                    Is.EqualTo(493));
                Assert.That(
                    sceneLights.Count(light =>
                        light.name != "RetailCheckoutFillLight"),
                    Is.EqualTo(4));
                Assert.That(sceneCameras.Length, Is.EqualTo(1));

                Light taskLight = sceneLights.Single(light =>
                    light.name == "WorkbenchTaskLight");
                Assert.That(taskLight.type, Is.EqualTo(LightType.Spot));
                Assert.That(taskLight.intensity, Is.EqualTo(0.4f).Within(0.0001f));
                Assert.That(taskLight.range, Is.EqualTo(2.8f).Within(0.0001f));
                Assert.That(taskLight.spotAngle, Is.EqualTo(62f).Within(0.0001f));
                Assert.That(taskLight.innerSpotAngle,
                    Is.EqualTo(38.44f).Within(0.001f));
                Assert.That(taskLight.shadows, Is.EqualTo(LightShadows.Soft));
                Assert.That(taskLight.shadowStrength,
                    Is.EqualTo(0.68f).Within(0.0001f));
                Assert.That(Vector3.Distance(
                    taskLight.transform.localPosition,
                    new Vector3(0.35f, 2.34f, 4.44f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    taskLight.transform.localRotation,
                    Quaternion.LookRotation(
                        new Vector3(-0.68f, 1.30f, 4.28f) -
                        new Vector3(0.35f, 2.34f, 4.44f))), Is.LessThan(0.01f));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsElectricalReadinessWorkbenchContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                GaragePrototypeMarker marker = FindInScene<GaragePrototypeMarker>(scene);
                ElectricalReadinessWorkbenchProjection[] projections = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        ElectricalReadinessWorkbenchProjection>(true))
                    .ToArray();
                ElectricalPowerTestStationProjection[] powerTestStations = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        ElectricalPowerTestStationProjection>(true))
                    .ToArray();

                Assert.That(marker, Is.Not.Null);
                Assert.That(projections.Length, Is.EqualTo(1));
                Assert.That(powerTestStations.Length, Is.EqualTo(1));
                ElectricalReadinessWorkbenchProjection projection = projections[0];
                ElectricalPowerTestStationProjection powerTestStation =
                    powerTestStations[0];
                Assert.That(marker.ElectricalReadinessWorkbench,
                    Is.SameAs(projection));
                Assert.That(marker.ElectricalPowerTestStation,
                    Is.SameAs(powerTestStation));
                Assert.That(marker.HasPowerBudgetWorkbenchR59Runtime,
                    Is.True);
                Assert.That(marker.HasPowerTestPreflightR60Runtime,
                    Is.True);
                Assert.That(marker.HasPowerStateInterlockR62Runtime,
                    Is.True);
                Assert.That(marker.HasFirmwareBaselineR63Runtime,
                    Is.True);
                Assert.That(marker.HasFictionalOsInstallationR64Runtime,
                    Is.True);
                Assert.That(marker.HasFictionalDriverInstallationR65Runtime,
                    Is.True);
                Assert.That(marker.HasValidationR66Runtime, Is.True);
                Assert.That(marker.HasQualityReleaseR67Runtime, Is.True);
                Assert.That(projection.ProjectionIdValue,
                    Is.EqualTo(ElectricalReadinessWorkbenchProjection
                        .PrototypeProjectionIdValue));
                Assert.That(projection.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(projection.IsConfigured, Is.True);
                Assert.That(projection.StatusText, Is.Not.Null);
                Assert.That(projection.StatusIndicator, Is.Not.Null);
                Assert.That(projection.GetComponentsInChildren<Collider>(true),
                    Is.Empty);
                Assert.That(projection.GetComponentsInChildren<Light>(true),
                    Is.Empty);
                Assert.That(projection.GetComponentsInChildren<Renderer>(true).Length,
                    Is.EqualTo(2));
                Assert.That(projection.GetComponentsInChildren<Renderer>(true).All(
                    renderer => renderer.gameObject.layer ==
                                LayerMask.NameToLayer("Ignore Raycast")), Is.True);
                Assert.That(Vector3.Distance(
                    projection.StatusText.transform.localPosition,
                    new Vector3(1.35f, 1.36f, 4.066f)),
                    Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    projection.StatusIndicator.transform.localPosition,
                    new Vector3(1.66f, 1.55f, 4.068f)),
                    Is.LessThan(0.0001f));
                Assert.That(projection.ReadyMaterial.name,
                    Does.StartWith("DeliveryStatusShelved"));
                Assert.That(projection.BlockedMaterial.name,
                    Does.StartWith("DeliveryStatusArrived"));
                Assert.That(powerTestStation.IsConfigured, Is.True);
                Assert.That(powerTestStation.StockFlow, Is.SameAs(marker.StockFlow));
                Assert.That(powerTestStation.PlayerInput,
                    Is.SameAs(marker.PlayerInput));
                Assert.That(powerTestStation.PlayerMotor,
                    Is.SameAs(marker.PlayerMotor));
                Assert.That(powerTestStation.PlayerCarry,
                    Is.SameAs(marker.PlayerCarry));
                Assert.That(powerTestStation.ReadinessProjection,
                    Is.SameAs(projection));
                Assert.That(powerTestStation.FocusAnchor,
                    Is.SameAs(projection.StatusText.transform));
                Assert.That(powerTestStation.GetComponentsInChildren<Collider>(true),
                    Is.Empty);
                Assert.That(powerTestStation.InteractionRange,
                    Is.EqualTo(ElectricalPowerTestStationProjection
                        .DefaultInteractionRange).Within(0.001f));
                Assert.That(powerTestStation.FocusDegrees,
                    Is.EqualTo(ElectricalPowerTestStationProjection
                        .DefaultFocusDegrees).Within(0.001f));
                GaragePrototypeHud hud = FindInScene<GaragePrototypeHud>(scene);
                Assert.That(hud.ElectricalPowerTestStation,
                    Is.SameAs(powerTestStation));

                GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
                long assemblyRevision = session.AssemblyBuild.Revision;
                long inventoryRevision = session.Inventory.Revision;
                OperationResult refresh = projection.RefreshPresentation();
                Assert.That(refresh.Error,
                    Is.EqualTo(AssemblyFailures.MotherboardMissing));
                Assert.That(projection.IsReady, Is.False);
                Assert.That(projection.PowerState, Is.EqualTo(PcPowerState.Off));
                Assert.That(projection.CurrentFailureCode,
                    Is.EqualTo(AssemblyFailures.MotherboardMissing.Code));
                Assert.That(projection.StatusText.text,
                    Does.Contain("ANAKART EKSİK")
                        .And.Contain("GÜÇ HAZIR DEĞİL"));
                Assert.That(session.AssemblyBuild.Revision,
                    Is.EqualTo(assemblyRevision));
                Assert.That(session.Inventory.Revision,
                    Is.EqualTo(inventoryRevision));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsQualityBoundPhysicalPackagingAndDispatchContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                GaragePrototypeMarker marker = FindInScene<GaragePrototypeMarker>(
                    scene);
                CustomPcPackagingStationProjection[] packagingStations = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        CustomPcPackagingStationProjection>(true))
                    .ToArray();
                CustomPcPackageDispatchProjection[] dispatchStations = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        CustomPcPackageDispatchProjection>(true))
                    .ToArray();
                CustomPcPackagePhysicalBinding[] packageBindings = scene
                    .GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<
                        CustomPcPackagePhysicalBinding>(true))
                    .ToArray();

                Assert.That(marker, Is.Not.Null);
                Assert.That(GaragePrototypeMarker.Version,
                    Is.EqualTo(
                        "garage-quality-bound-physical-packaging-r68-v1"));
                Assert.That(marker.HasCustomPcPackagingR68Runtime, Is.True);
                Assert.That(packagingStations.Length, Is.EqualTo(1));
                Assert.That(dispatchStations.Length, Is.EqualTo(1));
                Assert.That(packageBindings.Length, Is.EqualTo(1));

                CustomPcPackagingStationProjection packaging =
                    packagingStations[0];
                CustomPcPackageDispatchProjection dispatch =
                    dispatchStations[0];
                CustomPcPackagePhysicalBinding binding = packageBindings[0];
                PhysicalItemProjection package = marker.CustomPcPackage;
                Assert.That(marker.CustomPcPackagingStation,
                    Is.SameAs(packaging));
                Assert.That(marker.CustomPcPackageDispatch,
                    Is.SameAs(dispatch));
                Assert.That(marker.CustomPcPackageBinding, Is.SameAs(binding));
                Assert.That(binding.ValidateContract().IsSuccess, Is.True);
                Assert.That(binding.PackageItem, Is.SameAs(package));
                Assert.That(binding.SourceProjections.Count,
                    Is.EqualTo(
                        CustomPcPackagePhysicalBinding
                            .RequiredSourceProjectionCount));
                Assert.That(binding.SourceProjections.Distinct().Count(),
                    Is.EqualTo(binding.SourceProjections.Count));
                Assert.That(binding.SourceProjections.All(source =>
                    source != null &&
                    source.gameObject.activeSelf &&
                    source.CarryProfile == PhysicalCarryProfile.PcComponent),
                    Is.True);
                Assert.That(binding.PackagingAnchor.name,
                    Is.EqualTo("CustomPcPackageWorkbenchAnchor"));
                Assert.That(binding.DispatchAnchor.name,
                    Is.EqualTo("CustomPcDispatchPackageAnchor"));
                Assert.That(binding.PackageLabel.text,
                    Does.Contain("CUSTOM PC")
                        .And.Contain("KALİTE MÜHRÜ BEKLENİYOR"));

                Assert.That(package, Is.Not.Null);
                Assert.That(package.ItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession
                            .PrototypeCustomPcPackageIdValue));
                Assert.That(package.DisplayName,
                    Is.EqualTo("Mühürlü Custom PC Paketi"));
                Assert.That(package.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.LargeBox));
                Assert.That(package.Ownership,
                    Is.EqualTo(PhysicalItemOwnership.World));
                Assert.That(package.gameObject.activeSelf, Is.False);
                Assert.That(package.Body, Is.Not.Null);
                Assert.That(package.Body.mass, Is.EqualTo(12f).Within(0.001f));
                Assert.That(package.Body.isKinematic, Is.True);
                Assert.That(package.Body.useGravity, Is.False);

                Assert.That(packaging.PackageBinding, Is.SameAs(binding));
                Assert.That(packaging.StockFlow, Is.SameAs(marker.StockFlow));
                Assert.That(packaging.PlayerInput, Is.SameAs(marker.PlayerInput));
                Assert.That(packaging.PlayerMotor, Is.SameAs(marker.PlayerMotor));
                Assert.That(packaging.PlayerCarry, Is.SameAs(marker.PlayerCarry));
                Assert.That(Vector3.Distance(
                    packaging.transform.localPosition,
                    new Vector3(-3.28f, 0f, 0.50f)), Is.LessThan(0.0001f));
                Assert.That(packaging.InteractionCollider.isTrigger, Is.True);
                Assert.That(packaging.InteractionCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(packaging.StatusText.text,
                    Does.Contain("PAKETLEME İSTASYONU")
                        .And.Contain("KALİTE ONAYI BEKLENİYOR"));

                Assert.That(dispatch.PackageBinding, Is.SameAs(binding));
                Assert.That(dispatch.PlayerInput, Is.SameAs(marker.PlayerInput));
                Assert.That(dispatch.PlayerMotor, Is.SameAs(marker.PlayerMotor));
                Assert.That(dispatch.PlayerCarry, Is.SameAs(marker.PlayerCarry));
                Assert.That(dispatch.InteractionCollider.isTrigger, Is.True);
                Assert.That(dispatch.InteractionCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(dispatch.StatusText.text,
                    Does.Contain("SEVK SAHNESİ")
                        .And.Contain("MÜHÜRLÜ CUSTOM PC BEKLENİYOR"));

                GaragePrototypeHud hud = FindInScene<GaragePrototypeHud>(scene);
                Assert.That(hud.CustomPcPackagingStation,
                    Is.SameAs(packaging));
                Assert.That(hud.CustomPcPackageDispatch, Is.SameAs(dispatch));

                GarageStockFlowSession session = marker.StockFlow
                    .EnsureInitialized();
                Assert.That(session.TryGetCustomPcPackageAuthority(out _),
                    Is.False);
                Assert.That(session.TryGetPrototypeCustomPcPackage(out _),
                    Is.False);
                Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void GarageSceneContainsRetailCheckoutHeroReadabilityContract()
        {
            Scene scene = EditorSceneManager.OpenScene(
                GaragePrototypeMarker.ScenePath,
                OpenSceneMode.Additive);
            try
            {
                Transform[] transforms = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .ToArray();
                Transform heroRoot = transforms.Single(transform =>
                    transform.name == "RetailCheckoutHeroReadability");
                Renderer[] heroRenderers = heroRoot.GetComponentsInChildren<Renderer>(true);

                Assert.That(
                    GaragePrototypeMarker.Version,
                    Is.EqualTo("garage-quality-bound-physical-packaging-r68-v1"));
                Assert.That(heroRenderers.Length, Is.EqualTo(9));
                Assert.That(heroRoot.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(heroRoot.GetComponentsInChildren<Light>(true), Is.Empty);
                Assert.That(
                    heroRenderers.Select(renderer => renderer.name),
                    Is.EquivalentTo(new[]
                    {
                        "RetailCheckoutHeroDarkMetalDetails",
                        "RetailCheckoutHeroBrushedSteelDetails",
                        "RetailCheckoutHeroSafetyAccentDetails",
                        "RetailCheckoutHeroRubberDetails",
                        "RetailCheckoutLightDiffuser",
                        "RetailShelfOfferStateVisual",
                        "RetailBasketReservedStateVisual",
                        "CheckoutCashStateVisual",
                        "CheckoutReceiptStateVisual"
                    }));
                Assert.That(
                    heroRenderers.All(renderer =>
                        renderer.gameObject.layer ==
                        LayerMask.NameToLayer("Ignore Raycast")),
                    Is.True);
                Assert.That(
                    heroRenderers.All(renderer =>
                        renderer.shadowCastingMode == ShadowCastingMode.Off &&
                        !renderer.receiveShadows &&
                        renderer.motionVectorGenerationMode ==
                        MotionVectorGenerationMode.ForceNoMotion),
                    Is.True);

                AssertRendererMaterial(
                    heroRenderers,
                    "RetailCheckoutHeroDarkMetalDetails",
                    "DarkMetal");
                AssertRendererMaterial(
                    heroRenderers,
                    "RetailCheckoutHeroBrushedSteelDetails",
                    "BrushedSteel");
                AssertRendererMaterial(
                    heroRenderers,
                    "RetailCheckoutHeroSafetyAccentDetails",
                    "SafetyAccent");
                AssertRendererMaterial(
                    heroRenderers,
                    "RetailCheckoutHeroRubberDetails",
                    "WorkshopRubber");
                AssertRendererMaterial(
                    heroRenderers,
                    "RetailCheckoutLightDiffuser",
                    "LabelPaper");
                AssertRendererMaterial(
                    heroRenderers,
                    "RetailShelfOfferStateVisual",
                    "LabelPaper");
                AssertRendererMaterial(
                    heroRenderers,
                    "RetailBasketReservedStateVisual",
                    "SafetyAccent");
                AssertRendererMaterial(
                    heroRenderers,
                    "CheckoutCashStateVisual",
                    "LabelPaper");
                AssertRendererMaterial(
                    heroRenderers,
                    "CheckoutReceiptStateVisual",
                    "LabelPaper");

                RetailCheckoutHeroProjection heroProjection =
                    heroRoot.GetComponent<RetailCheckoutHeroProjection>();
                Assert.That(heroProjection, Is.Not.Null);
                Assert.That(heroProjection.StockFlow, Is.Not.Null);
                Assert.That(heroProjection.ShelfOfferVisual.name,
                    Is.EqualTo("RetailShelfOfferStateVisual"));
                Assert.That(heroProjection.BasketReservedVisual.name,
                    Is.EqualTo("RetailBasketReservedStateVisual"));
                Assert.That(heroProjection.CashCheckoutVisual.name,
                    Is.EqualTo("CheckoutCashStateVisual"));
                Assert.That(heroProjection.ReceiptVisual.name,
                    Is.EqualTo("CheckoutReceiptStateVisual"));
                Assert.That(heroProjection.ShelfOfferVisual.activeSelf, Is.False);
                Assert.That(heroProjection.BasketReservedVisual.activeSelf, Is.False);
                Assert.That(heroProjection.CashCheckoutVisual.activeSelf, Is.False);
                Assert.That(heroProjection.ReceiptVisual.activeSelf, Is.False);
                Assert.That(
                    transforms.Any(transform =>
                        transform.name == "RetailShelfProductDisplay" ||
                        transform.name == "CheckoutPaymentPadBody" ||
                        transform.name == "CheckoutPaymentPadScreen"),
                    Is.False,
                    "Lookdev must not introduce a fake product or payment terminal.");

                AssertTransformPosition(
                    transforms,
                    "RetailCustomerApproachAnchor",
                    new Vector3(1.05f, 0f, -2.10f));
                AssertTransformPosition(
                    transforms,
                    "RetailShelfOfferDisplayAnchor",
                    new Vector3(3.42f, 1.10f, 0.48f));
                AssertTransformPosition(
                    transforms,
                    "RetailBasketPresentationAnchor",
                    new Vector3(1.15f, 1.05f, 3.05f));
                AssertTransformPosition(
                    transforms,
                    "RetailCheckoutPaymentAnchor",
                    new Vector3(0.65f, 1.34f, 2.68f));
                AssertTransformPosition(
                    transforms,
                    "RetailCheckoutReceiptAnchor",
                    new Vector3(-0.02f, 1.18f, 3.02f));

                Assert.That(
                    transforms.Where(transform =>
                        transform.name == "StarterShelf" ||
                        transform.name == "ShelfPartsBox" ||
                        transform.name == "ShelfTechUnit" ||
                        transform.name == "ShelfTechDisplay"),
                    Is.Empty,
                    "The legacy decorative shelf must not compete with retail authority.");

                Transform shelfRoot = transforms.Single(transform =>
                    transform.name == "AuthoritativeRetailShelfA");
                Assert.That(
                    shelfRoot.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(5));
                Transform placementSurface = transforms.Single(transform =>
                    transform.name == "AuthoritativeShelfPlacementSurface");
                Assert.That(placementSurface.parent, Is.SameAs(shelfRoot));
                Assert.That(Vector3.Distance(
                    placementSurface.localPosition,
                    new Vector3(3.47f, 0.805f, 0.55f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    placementSurface.localScale,
                    new Vector3(0.72f, 0.05f, 1.48f)), Is.LessThan(0.0001f));
                Assert.That(placementSurface.GetComponent<BoxCollider>(), Is.Not.Null);
                PlacementSurface retailSurface =
                    placementSurface.GetComponent<PlacementSurface>();
                InventoryPlacementZone retailZone =
                    placementSurface.GetComponent<InventoryPlacementZone>();
                Assert.That(retailSurface, Is.Not.Null);
                Assert.That(retailSurface.SurfaceId,
                    Is.EqualTo("prototype.retail-shelf-a"));
                Assert.That(retailZone, Is.Not.Null);
                Assert.That(retailZone.PlacementSurface, Is.SameAs(retailSurface));
                Assert.That(retailZone.ContainerKind,
                    Is.EqualTo(InventoryContainerKind.Shelf));
                Assert.That(retailZone.ContainerId.Value,
                    Is.EqualTo(GarageStockFlowSession.ShelfContainerIdValue));
                Assert.That(
                    transforms.Select(transform =>
                            transform.GetComponent<InventoryPlacementZone>())
                        .Count(zone =>
                            zone != null &&
                            zone.ContainerId.Value ==
                                GarageStockFlowSession.ShelfContainerIdValue),
                    Is.EqualTo(1));

                TextMesh shelfLabel = transforms.Single(transform =>
                        transform.name == "RetailShelfLabel")
                    .GetComponent<TextMesh>();
                Assert.That(shelfLabel, Is.Not.Null);
                Assert.That(shelfLabel.transform.parent, Is.SameAs(shelfRoot));
                Assert.That(shelfLabel.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(Vector3.Distance(
                    shelfLabel.transform.localPosition,
                    new Vector3(2.998f, 1.60f, 0.48f)), Is.LessThan(0.0001f));
                Assert.That(shelfLabel.characterSize,
                    Is.EqualTo(0.014f).Within(0.0001f));
                Assert.That(shelfLabel.fontSize, Is.EqualTo(64));
                Assert.That(shelfLabel.lineSpacing,
                    Is.EqualTo(1.10f).Within(0.0001f));

                Transform checkoutTerminal = transforms.Single(transform =>
                    transform.name == "CheckoutPlayerTerminal");
                Assert.That(Vector3.Distance(
                    checkoutTerminal.localPosition,
                    new Vector3(0.65f, 1.34f, 2.68f)), Is.LessThan(0.0001f));
                Assert.That(checkoutTerminal.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(checkoutTerminal.GetComponent<BoxCollider>(), Is.Not.Null);

                Transform checkoutStation = transforms.Single(transform =>
                    transform.name == "CustomerCheckoutStation");
                TextMesh checkoutStatus = transforms.Single(transform =>
                        transform.name == "CheckoutStationStatusText")
                    .GetComponent<TextMesh>();
                Assert.That(checkoutStatus.transform.parent,
                    Is.SameAs(checkoutStation));
                Assert.That(Vector3.Distance(
                    checkoutStatus.transform.localPosition,
                    new Vector3(0.65f, 1.34f, 2.646f)), Is.LessThan(0.0001f));
                Assert.That(checkoutStatus.characterSize,
                    Is.EqualTo(0.015f).Within(0.0001f));
                Assert.That(checkoutStatus.lineSpacing,
                    Is.EqualTo(0.88f).Within(0.0001f));

                Transform flowBoard = transforms.Single(transform =>
                    transform.name == "CustomerFlowStatusBoard");
                TextMesh flowText = transforms.Single(transform =>
                        transform.name == "CustomerFlowStatusText")
                    .GetComponent<TextMesh>();
                Assert.That(Vector3.Distance(
                    flowBoard.localPosition,
                    new Vector3(-0.10f, 1.76f, 3.38f)), Is.LessThan(0.0001f));
                MeshFilter flowBoardMesh = flowBoard.GetComponent<MeshFilter>();
                Assert.That(flowBoardMesh, Is.Not.Null);
                Assert.That(Vector3.Distance(
                    flowBoardMesh.sharedMesh.bounds.size,
                    new Vector3(0.90f, 0.34f, 0.04f)), Is.LessThan(0.0001f));
                Assert.That(flowText.transform.parent, Is.SameAs(checkoutStation));
                Assert.That(Vector3.Distance(
                    flowText.transform.localPosition,
                    new Vector3(-0.10f, 1.76f, 3.354f)), Is.LessThan(0.0001f));
                Assert.That(flowText.characterSize,
                    Is.EqualTo(0.015f).Within(0.0001f));
                Assert.That(flowText.lineSpacing,
                    Is.EqualTo(0.82f).Within(0.0001f));

                AssertTransformPosition(
                    transforms,
                    "CustomerEntranceWaypoint",
                    new Vector3(-0.15f, 0f, -4.25f));
                AssertTransformPosition(
                    transforms,
                    "CustomerBrowseWaypoint",
                    new Vector3(2.35f, 0f, 0.55f));
                AssertTransformPosition(
                    transforms,
                    "CustomerCheckoutWaypoint",
                    new Vector3(1.85f, 0f, 2.45f));
                AssertTransformPosition(
                    transforms,
                    "CustomerExitWaypoint",
                    new Vector3(0.20f, 0f, -4.20f));

                Light retailLight = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Light>(true))
                    .Single(light => light.name == "RetailCheckoutFillLight");
                Assert.That(retailLight.type, Is.EqualTo(LightType.Spot));
                Assert.That(retailLight.intensity,
                    Is.EqualTo(0.42f).Within(0.0001f));
                Assert.That(retailLight.range,
                    Is.EqualTo(4.40f).Within(0.0001f));
                Assert.That(retailLight.spotAngle,
                    Is.EqualTo(110f).Within(0.0001f));
                Assert.That(retailLight.innerSpotAngle,
                    Is.EqualTo(68.2f).Within(0.001f));
                Assert.That(retailLight.shadows, Is.EqualTo(LightShadows.None));
                Assert.That(Vector3.Distance(
                    retailLight.transform.localPosition,
                    new Vector3(2.10f, 2.78f, 2.10f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    retailLight.transform.localRotation,
                    Quaternion.LookRotation(
                        new Vector3(1.94f, 0.92f, 1.77f) -
                        new Vector3(2.10f, 2.78f, 2.10f))), Is.LessThan(0.01f));
            }
            finally
            {
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
                    Is.EqualTo("garage-quality-bound-physical-packaging-r68-v1"));

                Transform benchmark = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Single(transform => transform.name == "VisualBenchmarkCorner");
                Assert.That(benchmark.GetComponentsInChildren<Renderer>(true).Length, Is.GreaterThan(75));

                string[] rendererNames = benchmark.GetComponentsInChildren<Renderer>(true)
                    .Select(renderer => renderer.name)
                    .ToArray();
                Assert.That(rendererNames, Does.Contain("WorkbenchTop"));
                Assert.That(rendererNames, Does.Contain("DiagnosticMonitorScreen"));
                Assert.That(
                    benchmark.GetComponentsInChildren<Transform>(true)
                        .Select(transform => transform.name),
                    Does.Not.Contain("StarterShelf")
                        .And.Not.Contain("ShelfPartsBox")
                        .And.Not.Contain("ShelfTechUnit")
                        .And.Not.Contain("ShelfTechDisplay"));

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
                Transform coolerSlotRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerMountingBracket");
                Transform coolerSnapAnchor = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerSnapAnchor");
                Transform coolerBracketPivot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerBracketPivot");
                Transform coolerFocus = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerSlotFocusTarget");
                Transform coolerRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "PrototypeProcessorCooler");
                Transform coolerColdPlate = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerColdPlate");
                Transform coolerTim = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerPreAppliedTim");
                Transform coolerFinStack = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerFinStack");
                Transform coolerFan = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "ProcessorCoolerFan");
                Transform coolerMountingFrame = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "ProcessorCoolerMountingFrame");
                Transform graphicsCardSlotRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "MotherboardPcieX16GraphicsSlot");
                Transform graphicsCardSnapAnchor = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "GraphicsCardPcieX16SnapAnchor");
                Transform graphicsCardLatchPivot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "GraphicsCardPcieLatchPivot");
                Transform graphicsCardRearBracket = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "GraphicsCardRearBracket");
                Transform graphicsCardFastenerPivot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "GraphicsCardRearBracketFastenerPivot");
                Transform graphicsCardFocus = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "GraphicsCardSlotFocusTarget");
                Transform graphicsCardRoot = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "PrototypeNorthstarA60GraphicsCard");
                Transform graphicsCardPcb = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name == "GraphicsCardPcb");
                Transform graphicsCardShroud = assemblySlice
                    .GetComponentsInChildren<Transform>(true)
                    .Single(transform => transform.name ==
                                         "GraphicsCardDualFanShroud");
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
                ProcessorCoolerSlotProjection coolerSlot =
                    coolerSlotRoot.GetComponent<ProcessorCoolerSlotProjection>();
                ProcessorCoolerAssemblyItemBinding coolerBinding =
                    coolerRoot.GetComponent<ProcessorCoolerAssemblyItemBinding>();
                PhysicalItemProjection processorCooler =
                    coolerRoot.GetComponent<PhysicalItemProjection>();
                ProcessorCoolerRuntimeGeometry coolerGeometry =
                    coolerRoot.GetComponent<ProcessorCoolerRuntimeGeometry>();
                ProcessorCoolerRuntimeSmokeMarker coolerSmoke =
                    coolerRoot.GetComponent<ProcessorCoolerRuntimeSmokeMarker>();
                GraphicsCardSlotProjection graphicsCardSlot =
                    graphicsCardSlotRoot.GetComponent<GraphicsCardSlotProjection>();
                GraphicsCardAssemblyItemBinding graphicsCardBinding =
                    graphicsCardRoot.GetComponent<GraphicsCardAssemblyItemBinding>();
                PhysicalItemProjection graphicsCard =
                    graphicsCardRoot.GetComponent<PhysicalItemProjection>();
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
                Assert.That(coolerSlot, Is.Not.Null);
                Assert.That(coolerBinding, Is.Not.Null);
                Assert.That(processorCooler, Is.Not.Null);
                Assert.That(coolerGeometry, Is.Not.Null);
                Assert.That(coolerSmoke, Is.Not.Null);
                Assert.That(graphicsCardSlot, Is.Not.Null);
                Assert.That(graphicsCardBinding, Is.Not.Null);
                Assert.That(graphicsCard, Is.Not.Null);
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
                Assert.That(marker.ProcessorCoolerSlot, Is.SameAs(coolerSlot));
                Assert.That(marker.ProcessorCoolerBinding, Is.SameAs(coolerBinding));
                Assert.That(marker.ProcessorCooler, Is.SameAs(processorCooler));
                Assert.That(marker.ProcessorCoolerGeometry, Is.SameAs(coolerGeometry));
                Assert.That(marker.GraphicsCardSlot, Is.SameAs(graphicsCardSlot));
                Assert.That(marker.GraphicsCardBinding,
                    Is.SameAs(graphicsCardBinding));
                Assert.That(marker.GraphicsCard, Is.SameAs(graphicsCard));
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
                Assert.That(coolerSlot.IsConfigured, Is.True);
                Assert.That(coolerSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorCoolerSlotIdValue));
                Assert.That(coolerSlot.BracketIdValue,
                    Is.EqualTo(GarageStockFlowSession.ProcessorCoolerBracketIdValue));
                Assert.That(coolerSlot.SnapAnchor, Is.SameAs(coolerSnapAnchor));
                Assert.That(coolerSlot.FocusCollider,
                    Is.SameAs(coolerFocus.GetComponent<BoxCollider>()));
                Assert.That(coolerSlot.FocusCollider.enabled, Is.False);
                Assert.That(coolerSlot.AssemblyRoot,
                    Is.SameAs(binding.PhysicalItem.transform));
                Assert.That(coolerSlot.BracketPivot,
                    Is.SameAs(coolerBracketPivot));
                Assert.That(coolerSlot.RetentionPointIdValues,
                    Is.EqualTo(new[]
                    {
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint1IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint2IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint3IdValue,
                        GarageStockFlowSession.ProcessorCoolerRetentionPoint4IdValue
                    }));
                Assert.That(coolerSlot.RetentionPoints.Length, Is.EqualTo(4));
                Assert.That(coolerSlot.RetentionPoints.Distinct().Count(), Is.EqualTo(4));
                Assert.That(coolerSlot.ClearanceBlockers,
                    Is.EqualTo(new[] { memoryRoot.GetComponent<BoxCollider>() }));
                Assert.That(storageSlot.transform.localPosition.y,
                    Is.EqualTo(0.100f).Within(0.0001f));
                Assert.That(Vector3.Distance(
                    coolerSnapAnchor.localPosition,
                    new Vector3(0f, 0f, 0.011f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    coolerFocus.localPosition,
                    new Vector3(0f, 0f, 0.055f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    coolerFocus.GetComponent<BoxCollider>().size,
                    new Vector3(0.145f, 0.145f, 0.10f)), Is.LessThan(0.0001f));
                Assert.That(coolerBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(coolerBinding.PhysicalItem, Is.SameAs(processorCooler));
                Assert.That(coolerBinding.Slot, Is.SameAs(coolerSlot));
                Assert.That(coolerBinding.InventoryItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue));
                Assert.That(marker.PlayerCarry.MatchesProcessorCoolerConfiguration(
                    coolerSlot,
                    coolerBinding), Is.True);
                Assert.That(processorCooler.ItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue));
                Assert.That(processorCooler.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.ProcessorCoolerDisplayName));
                Assert.That(processorCooler.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(processorCooler.SupportsPlacement, Is.False);
                Assert.That(processorCooler.Body.mass,
                    Is.EqualTo(0.52f).Within(0.001f));
                Assert.That(processorCooler.Body.isKinematic, Is.True);
                Assert.That(processorCooler.Body.useGravity, Is.False);
                Assert.That(Vector3.Distance(
                    coolerRoot.localPosition,
                    new Vector3(-0.72f, 0.992f, 3.93f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    coolerRoot.localRotation,
                    Quaternion.Euler(-90f, 0f, 0f)), Is.LessThan(0.1f));
                Assert.That(coolerGeometry.IsCanonical, Is.True);
                Assert.That(coolerGeometry.ColdPlate, Is.SameAs(coolerColdPlate));
                Assert.That(coolerGeometry.PreAppliedTim, Is.SameAs(coolerTim));
                Assert.That(coolerGeometry.FinStack, Is.SameAs(coolerFinStack));
                Assert.That(coolerGeometry.Fan, Is.SameAs(coolerFan));
                Assert.That(coolerGeometry.Bracket, Is.SameAs(coolerMountingFrame));
                Assert.That(coolerGeometry.RetentionPoints.Length, Is.EqualTo(4));
                Assert.That(coolerGeometry.RetentionPoints.Distinct().Count(),
                    Is.EqualTo(4));
                Assert.That(coolerSmoke.IsReady, Is.True);
                Assert.That(marker.HasProcessorCoolerR27Runtime, Is.True);
                OperationResult coolerProjectionInvariant =
                    coolerBinding.ValidateProjectionInvariant();
                Assert.That(coolerProjectionInvariant.IsSuccess,
                    Is.True,
                    coolerProjectionInvariant.IsFailure
                        ? coolerProjectionInvariant.Error.Code
                        : string.Empty);
                Assert.That(graphicsCardSlot.IsConfigured, Is.True);
                Assert.That(graphicsCardSlot.SlotIdValue,
                    Is.EqualTo(GarageStockFlowSession.GraphicsCardSlotIdValue));
                Assert.That(graphicsCardSlot.LatchIdValue,
                    Is.EqualTo(GarageStockFlowSession.GraphicsCardLatchIdValue));
                Assert.That(graphicsCardSlot.RearBracketIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession.GraphicsCardRearBracketIdValue));
                Assert.That(graphicsCardSlot.RearBracketFastenerIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession
                            .GraphicsCardBracketFastenerIdValue));
                Assert.That(graphicsCardSlot.SnapAnchor,
                    Is.SameAs(graphicsCardSnapAnchor));
                Assert.That(Vector3.Distance(
                    graphicsCardSlotRoot.localPosition,
                    new Vector3(0f, -0.074f, 0.012f)),
                    Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    graphicsCardSnapAnchor.localPosition,
                    new Vector3(0f, 0f, 0.008f)),
                    Is.LessThan(0.0001f));
                Assert.That(graphicsCardSlot.AssemblyRoot,
                    Is.SameAs(binding.PhysicalItem.transform));
                Assert.That(graphicsCardSlot.LatchPivot,
                    Is.SameAs(graphicsCardLatchPivot));
                Assert.That(graphicsCardSlot.RearBracketFastenerPivot,
                    Is.SameAs(graphicsCardFastenerPivot));
                Assert.That(graphicsCardSlot.FocusCollider,
                    Is.SameAs(graphicsCardFocus.GetComponent<BoxCollider>()));
                Assert.That(graphicsCardSlot.FocusCollider.enabled, Is.False);
                Assert.That(graphicsCardSlot.FocusCollider.isTrigger, Is.True);
                Assert.That(graphicsCardSlot.FocusCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Interactable")));
                Assert.That(graphicsCardSlot.SupportCollider,
                    Is.SameAs(graphicsCardSlotRoot
                        .Find("PcieX16Connector").GetComponent<BoxCollider>()));
                Assert.That(graphicsCardSlot.SupportCollider.isTrigger, Is.False);
                Assert.That(graphicsCardSlot.SupportCollider.gameObject.layer,
                    Is.EqualTo(LayerMask.NameToLayer("Ignore Raycast")));
                Assert.That(graphicsCardSlot.ChassisClearanceBlockers.Length,
                    Is.EqualTo(5));
                Assert.That(graphicsCardSlot.CoolerClearanceBlockers,
                    Is.EqualTo(new[]
                    {
                        processorCooler.GetComponent<BoxCollider>()
                    }));
                Assert.That(graphicsCardBinding.Runtime, Is.SameAs(marker.StockFlow));
                Assert.That(graphicsCardBinding.PhysicalItem,
                    Is.SameAs(graphicsCard));
                Assert.That(graphicsCardBinding.Slot,
                    Is.SameAs(graphicsCardSlot));
                Assert.That(graphicsCardBinding.InventoryItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession
                            .GraphicsCardAssemblyItemInstanceIdValue));
                Assert.That(marker.PlayerCarry.MatchesGraphicsCardConfiguration(
                    graphicsCardSlot,
                    graphicsCardBinding), Is.True);
                Assert.That(graphicsCard.ItemIdValue,
                    Is.EqualTo(
                        GarageStockFlowSession
                            .GraphicsCardAssemblyItemInstanceIdValue));
                Assert.That(graphicsCard.DisplayName,
                    Is.EqualTo(GarageStockFlowSession.ProductDisplayName));
                Assert.That(graphicsCard.CarryProfile,
                    Is.EqualTo(PhysicalCarryProfile.PcComponent));
                Assert.That(graphicsCard.SupportsPlacement, Is.False);
                Assert.That(graphicsCard.Body.mass,
                    Is.EqualTo(0.82f).Within(0.001f));
                Assert.That(graphicsCard.Body.isKinematic, Is.True);
                Assert.That(graphicsCard.Body.useGravity, Is.False);
                BoxCollider graphicsCardCollider =
                    graphicsCardRoot.GetComponent<BoxCollider>();
                Assert.That(graphicsCardCollider, Is.Not.Null);
                Assert.That(Vector3.Distance(
                    graphicsCardCollider.center,
                    new Vector3(0f, -0.032f, 0.0625f)),
                    Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    graphicsCardCollider.size,
                    new Vector3(0.285f, 0.064f, 0.125f)),
                    Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    graphicsCardPcb.GetComponent<MeshFilter>()
                        .sharedMesh.bounds.size,
                    new Vector3(0.270f, 0.008f, 0.112f)),
                    Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    graphicsCard.DropHalfExtents,
                    new Vector3(0.1425f, 0.032f, 0.0625f)),
                    Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    graphicsCardRoot.localPosition,
                    new Vector3(-0.45f, 0.992f, 3.93f)), Is.LessThan(0.0001f));
                Assert.That(Quaternion.Angle(
                    graphicsCardRoot.localRotation,
                    Quaternion.Euler(-90f, 0f, 0f)), Is.LessThan(0.1f));
                Assert.That(graphicsCardPcb.GetComponent<Renderer>()
                        .sharedMaterial.name,
                    Does.StartWith("MotherboardPcb"));
                Assert.That(graphicsCardShroud.GetComponent<Renderer>()
                        .sharedMaterial.name,
                    Does.StartWith("DarkMetal"));
                Assert.That(graphicsCardRoot.GetComponentsInChildren<Transform>(true)
                        .Count(transform => transform.name.StartsWith(
                            "GraphicsCardFan_",
                            StringComparison.Ordinal)),
                    Is.EqualTo(2));
                Assert.That(graphicsCardRoot.GetComponentsInChildren<Transform>(true)
                        .Count(transform => transform.name.StartsWith(
                            "GraphicsCardPcieContact_",
                            StringComparison.Ordinal)),
                    Is.EqualTo(12));
                Assert.That(graphicsCardRearBracket, Is.Not.Null);
                Assert.That(marker.HasGraphicsCardR28Runtime, Is.True);
                OperationResult graphicsCardProjectionInvariant =
                    graphicsCardBinding.ValidateProjectionInvariant();
                Assert.That(graphicsCardProjectionInvariant.IsSuccess,
                    Is.True,
                    graphicsCardProjectionInvariant.IsFailure
                        ? graphicsCardProjectionInvariant.Error.Code
                        : string.Empty);
                Assert.That(seat.SnapAnchor, Is.SameAs(snapAnchor));
                Assert.That(seat.SnapPose.position,
                    Is.EqualTo(new Vector3(-0.75f, 1.30f, 4.35f)));
                Assert.That(Quaternion.Angle(
                    seat.SnapPose.rotation,
                    Quaternion.Euler(0f, 180f, 0f)), Is.LessThan(0.1f));
                Assert.That(Vector3.Distance(
                    tray.localPosition,
                    new Vector3(-0.75f, 1.361f, 4.387f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    tray.localScale,
                    new Vector3(1f, 0.79f, 1f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    tray.GetComponent<BoxCollider>().size,
                    new Vector3(0.454f, 0.534f, 0.050f)), Is.LessThan(0.0001f));
                Assert.That(tray.GetComponent<Renderer>().sharedMaterial.name,
                    Does.StartWith("DarkMetal"));
                Assert.That(Vector3.Distance(
                    statusPlate.localPosition,
                    new Vector3(-0.75f, 1.155f, 4.353f)), Is.LessThan(0.0001f));
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
                Assert.That(fastener.FocusCollider.isTrigger, Is.True);
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
                    Is.EqualTo(191));
                Assert.That(assemblySlice.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(29));
                Assert.That(assemblySlice.GetComponentsInChildren<Light>(true), Is.Empty);
                Assert.That(assemblySlice.GetComponentsInChildren<TextMesh>(true).Length,
                    Is.EqualTo(9));
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

        private static void AssertRendererMaterial(
            Renderer[] renderers,
            string rendererName,
            string materialPrefix)
        {
            Renderer renderer = renderers.Single(candidate =>
                candidate.name == rendererName);
            Assert.That(renderer.sharedMaterial, Is.Not.Null);
            Assert.That(renderer.sharedMaterial.name, Does.StartWith(materialPrefix));
        }

        private static void AssertTransformPosition(
            Transform[] transforms,
            string transformName,
            Vector3 expectedPosition)
        {
            Transform transform = transforms.Single(candidate =>
                candidate.name == transformName);
            Assert.That(Vector3.Distance(
                transform.localPosition,
                expectedPosition), Is.LessThan(0.0001f));
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
