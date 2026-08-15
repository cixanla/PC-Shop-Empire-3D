using System;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
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
                Assert.That(physicalItems.Length, Is.EqualTo(6));
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
                Assert.That(physicalItems.Count(
                    item => item.CarryProfile == PhysicalCarryProfile.PcComponent),
                    Is.EqualTo(2));
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
                    Is.EqualTo("garage-cpu-socket-retention-r24-v1"));

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
                Assert.That(openChassis, Is.Not.Null);
                Assert.That(seat, Is.Not.Null);
                Assert.That(fastener, Is.Not.Null);
                Assert.That(binding, Is.Not.Null);
                Assert.That(processorSocket, Is.Not.Null);
                Assert.That(processorBinding, Is.Not.Null);
                Assert.That(processor, Is.Not.Null);
                Assert.That(marker.MotherboardSeat, Is.SameAs(seat));
                Assert.That(marker.MotherboardFastener, Is.SameAs(fastener));
                Assert.That(marker.MotherboardBinding, Is.SameAs(binding));
                Assert.That(marker.ProcessorSocket, Is.SameAs(processorSocket));
                Assert.That(marker.ProcessorBinding, Is.SameAs(processorBinding));
                Assert.That(marker.Processor, Is.SameAs(processor));
                Assert.That(binding.Fastener, Is.SameAs(fastener));
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
                    new Vector3(0.0255f, 0.014f, 0.012f)), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(
                    connectorBounds.size,
                    new Vector3(0.171f, 0.182f, 0.012f)), Is.LessThan(0.0001f));
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
                    Is.EqualTo(21));
                Assert.That(assemblySlice.GetComponentsInChildren<Collider>(true).Length,
                    Is.EqualTo(11));
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
