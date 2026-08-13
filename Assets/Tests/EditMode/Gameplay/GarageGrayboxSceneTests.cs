using System;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using PCShopEmpire3D.Editor.GaragePrototype;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
                Assert.That(physicalItems.Length, Is.EqualTo(2));
                Assert.That(
                    physicalItems.Select(item => item.ItemIdValue).Distinct(StringComparer.Ordinal).Count(),
                    Is.EqualTo(physicalItems.Length));
                PhysicalItemProjection smallBox = physicalItems.Single(
                    item => item.CarryProfile == PhysicalCarryProfile.SmallBox);
                PhysicalItemProjection largeBox = physicalItems.Single(
                    item => item.CarryProfile == PhysicalCarryProfile.LargeBox);
                Assert.That(smallBox.ItemIdValue, Is.EqualTo("prototype.garage-box-001"));
                Assert.That(smallBox.SupportsPlacement, Is.True);
                Assert.That(smallBox.DropHalfExtents, Is.EqualTo(new Vector3(0.35f, 0.225f, 0.25f)));
                Assert.That(largeBox.ItemIdValue, Is.EqualTo("prototype.garage-large-box-001"));
                Assert.That(largeBox.DisplayName, Is.EqualTo("Büyük Kargo Kutusu"));
                Assert.That(largeBox.SupportsPlacement, Is.False);
                Assert.That(largeBox.Body.mass, Is.EqualTo(9f).Within(0.001f));
                Assert.That(largeBox.DropHalfExtents, Is.EqualTo(new Vector3(0.55f, 0.4f, 0.35f)));
                Assert.That(physicalItems.All(item => item.Body != null), Is.True);
                Assert.That(
                    physicalItems.All(item =>
                        item.GetComponentsInChildren<Collider>().Length >= 1),
                    Is.True);
                Assert.That(placementSurfaces.Length, Is.EqualTo(1));
                Assert.That(placementSurfaces[0].SurfaceId, Is.EqualTo("prototype.stock-floor-small-box-a"));
                Assert.That(placementSurfaces[0].GridSize, Is.EqualTo(0.25f).Within(0.001f));
                Assert.That(placementSurfaces[0].YawStepDegrees, Is.EqualTo(90f).Within(0.001f));
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
                Assert.That(GaragePrototypeMarker.Version, Is.EqualTo("garage-readable-lookdev-g6-v1"));

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
                Assert.That(concrete, Is.Not.Null);
                Assert.That(metal, Is.Not.Null);
                Assert.That(steel, Is.Not.Null);
                Assert.That(wood, Is.Not.Null);
                Assert.That(cardboard, Is.Not.Null);
                Assert.That(screen, Is.Not.Null);
                Assert.That(concrete.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(metal.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(steel.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(wood.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(cardboard.GetTexture("_BaseMap"), Is.Not.Null);
                Assert.That(metal.GetFloat("_Metallic"), Is.LessThan(0.2f));
                Assert.That(steel.GetFloat("_Metallic"), Is.GreaterThan(0.8f));
                Assert.That(screen.GetFloat("_Smoothness"), Is.GreaterThan(0.75f));
                Assert.That(screen.IsKeywordEnabled("_EMISSION"), Is.True);

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
