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
                Assert.That(physicalItems.Length, Is.EqualTo(1));
                Assert.That(
                    physicalItems.Select(item => item.ItemIdValue).Distinct(StringComparer.Ordinal).Count(),
                    Is.EqualTo(physicalItems.Length));
                Assert.That(physicalItems[0].ItemIdValue, Is.EqualTo("prototype.garage-box-001"));
                Assert.That(physicalItems[0].Body, Is.Not.Null);
                Assert.That(physicalItems[0].GetComponentsInChildren<Collider>().Length, Is.GreaterThanOrEqualTo(1));
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
