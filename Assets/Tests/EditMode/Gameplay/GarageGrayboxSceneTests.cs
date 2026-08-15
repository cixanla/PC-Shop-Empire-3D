using System;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
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
                Assert.That(physicalItems.Length, Is.EqualTo(4));
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
                Assert.That(marker.StockFlow, Is.Not.Null);
                Assert.That(marker.StockFlow.ItemBinding, Is.SameAs(deliveryBinding));
                Assert.That(marker.StockFlow.Parcel, Is.SameAs(deliveryParcel));
                Assert.That(marker.StockFlow.EnsureInitialized().Order.Status,
                    Is.EqualTo(PCShopEmpire3D.Orders.PurchaseOrderStatus.Arrived));
                Assert.That(marker.StockFlow.Session.TryGetItem(out _), Is.False);
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
                Assert.That(marker.CustomerFlow, Is.Not.Null);
                Assert.That(marker.CustomerFlow.StockFlow, Is.SameAs(marker.StockFlow));
                Assert.That(marker.CustomerFlow.NavigationSurface, Is.Not.Null);
                Assert.That(marker.CustomerFlow.NavigationSurface.collectObjects,
                    Is.EqualTo(CollectObjects.Volume));
                Assert.That(marker.CustomerFlow.NavigationSurface.useGeometry,
                    Is.EqualTo(NavMeshCollectGeometry.PhysicsColliders));
                Assert.That(marker.CustomerFlow.CustomerAgent, Is.Not.Null);
                Assert.That(marker.CustomerFlow.CustomerAgent.speed, Is.EqualTo(2.2f).Within(0.001f));
                Assert.That(marker.CustomerFlow.CustomerAgent.radius, Is.EqualTo(0.28f).Within(0.001f));
                Assert.That(marker.CustomerFlow.CustomerVisualRoot.activeSelf, Is.False);
                Assert.That(marker.CustomerFlow.CustomerStatusText.text,
                    Does.Contain("MÜŞTERİ AKIŞI: TEKLİF BEKLİYOR"));
                Assert.That(marker.CustomerFlow.EntranceWaypoint, Is.Not.Null);
                Assert.That(marker.CustomerFlow.BrowseWaypoint, Is.Not.Null);
                Assert.That(marker.CustomerFlow.CheckoutWaypoint, Is.Not.Null);
                Assert.That(marker.CustomerFlow.ExitWaypoint, Is.Not.Null);
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
                    Is.EqualTo("garage-cash-settlement-r19-v1"));

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
