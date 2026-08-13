using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static class GarageGrayboxSceneBuilder
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string MaterialRoot = "Assets/Art/Prototype/Materials";
        private const string PlayerPrefabPath = "Assets/Prefabs/Prototype/PlayerRig.prefab";
        private const string PlayerLayerName = "Player";
        private const string InteractableLayerName = "Interactable";
        private const string HeldItemLayerName = "HeldItem";
        private const string ViewModelLayerName = "ViewModel";

        [MenuItem("PC Shop Empire/Prototype/Rebuild Garage Graybox")]
        public static void Build()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                Debug.Log("GARAGE_GRAYBOX_BUILD_CANCELLED unsaved-scene-work-preserved");
                return;
            }

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            try
            {
                BuildInternal();
            }
            finally
            {
                if (!Application.isBatchMode && previousSetup.Length > 0)
                {
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
                }
            }
        }

        private static void BuildInternal()
        {
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                PlayerInputContract.AssetPath);
            Require(inputActions != null, $"Input asset is missing: {PlayerInputContract.AssetPath}");
            ValidateInputContract(inputActions);

            EnsureProjectDirectories();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "GarageGraybox";

            Material concrete = GetOrCreateMaterial(
                "Concrete",
                new Color(0.23f, 0.25f, 0.27f),
                0f,
                0.25f);
            Material wall = GetOrCreateMaterial(
                "WarmWall",
                new Color(0.50f, 0.47f, 0.42f),
                0f,
                0.18f);
            Material metal = GetOrCreateMaterial(
                "DarkMetal",
                new Color(0.11f, 0.14f, 0.17f),
                0.65f,
                0.45f);
            Material accent = GetOrCreateMaterial(
                "SafetyAccent",
                new Color(0.95f, 0.53f, 0.08f),
                0.1f,
                0.3f);
            Material cardboard = GetOrCreateMaterial(
                "Cardboard",
                new Color(0.52f, 0.32f, 0.16f),
                0f,
                0.12f);
            Material hands = GetOrCreateMaterial(
                "PrototypeHands",
                new Color(0.15f, 0.36f, 0.55f),
                0.05f,
                0.35f);
            Material stockPlacement = GetOrCreateMaterial(
                "StockPlacementSurface",
                new Color(0.08f, 0.42f, 0.48f),
                0.05f,
                0.22f);
            Material placementValid = GetOrCreateGhostMaterial(
                "PlacementGhostValid",
                new Color(0.12f, 0.95f, 0.35f, 0.42f));
            Material placementInvalid = GetOrCreateGhostMaterial(
                "PlacementGhostInvalid",
                new Color(1f, 0.16f, 0.10f, 0.48f));

            Transform systems = CreateRoot("__Systems").transform;
            Transform environment = CreateRoot("Environment").transform;
            Transform gameplay = CreateRoot("Gameplay").transform;
            Transform spawn = CreateRoot("PlayerSpawn").transform;
            Transform lighting = CreateRoot("Lighting").transform;
            Transform debug = CreateRoot("Debug").transform;
            spawn.position = new Vector3(0f, 0.05f, -2.5f);

            BuildRoom(environment, concrete, wall, metal, accent, cardboard, stockPlacement);
            BuildStarterPickups(environment, cardboard, metal, accent);
            BuildLighting(lighting);
            FirstPersonMotor prefabSource = BuildPlayer(
                gameplay,
                inputActions,
                hands,
                placementValid,
                placementInvalid);
            GameObject playerPrefab = PrefabUtility.SaveAsPrefabAsset(
                prefabSource.gameObject,
                PlayerPrefabPath);
            Require(playerPrefab != null, $"Player prefab could not be saved: {PlayerPrefabPath}");
            UnityEngine.Object.DestroyImmediate(prefabSource.gameObject);

            GameObject playerInstance = PrefabUtility.InstantiatePrefab(playerPrefab, scene) as GameObject;
            Require(playerInstance != null, $"Player prefab could not be instantiated: {PlayerPrefabPath}");
            playerInstance.transform.SetParent(gameplay, false);
            playerInstance.transform.SetPositionAndRotation(spawn.position, spawn.rotation);
            FirstPersonMotor motor = playerInstance.GetComponent<FirstPersonMotor>();
            Require(motor != null, "The PlayerRig prefab is missing FirstPersonMotor.");
            PlayerInputAdapter input = motor.GetComponent<PlayerInputAdapter>();
            PlayerCarryController carry = motor.GetComponent<PlayerCarryController>();
            Require(carry != null, "The PlayerRig prefab is missing PlayerCarryController.");

            GaragePrototypeMarker marker = systems.gameObject.AddComponent<GaragePrototypeMarker>();
            marker.Configure(motor, input, carry);
            GaragePrototypeHud hud = systems.gameObject.AddComponent<GaragePrototypeHud>();
            hud.Configure(motor, carry);

            GameObject debugMarker = CreateCube(
                "InteractionTestMarker",
                debug,
                new Vector3(0f, 0.01f, 1.25f),
                new Vector3(2.4f, 0.02f, 1.6f),
                accent);
            Collider debugCollider = debugMarker.GetComponent<Collider>();
            if (debugCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(debugCollider);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.30f, 0.32f, 0.35f);
            RenderSettings.fog = false;

            EditorSceneManager.SaveScene(scene, GaragePrototypeMarker.ScenePath);
            SetBuildScenes();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"GARAGE_GRAYBOX_BUILD_OK scene={GaragePrototypeMarker.ScenePath} " +
                $"version={GaragePrototypeMarker.Version}");
        }

        public static void BuildFromCommandLine()
        {
            Build();
        }

        private static void BuildRoom(
            Transform parent,
            Material concrete,
            Material wall,
            Material metal,
            Material accent,
            Material cardboard,
            Material stockPlacement)
        {
            CreateCube("Floor", parent, new Vector3(0f, -0.1f, 0f), new Vector3(8f, 0.2f, 10f), concrete);
            CreateCube("Ceiling", parent, new Vector3(0f, 3.1f, 0f), new Vector3(8.4f, 0.15f, 10.2f), wall);
            CreateCube("Wall_Left", parent, new Vector3(-4.1f, 1.5f, 0f), new Vector3(0.2f, 3.2f, 10.2f), wall);
            CreateCube("Wall_Right", parent, new Vector3(4.1f, 1.5f, 0f), new Vector3(0.2f, 3.2f, 10.2f), wall);
            CreateCube("Wall_Back", parent, new Vector3(0f, 1.5f, 5.1f), new Vector3(8.4f, 3.2f, 0.2f), wall);
            CreateCube("Front_Header", parent, new Vector3(0f, 2.85f, -5.1f), new Vector3(8.4f, 0.5f, 0.2f), wall);
            CreateCube("Front_Left", parent, new Vector3(-3.75f, 1.3f, -5.1f), new Vector3(0.9f, 2.6f, 0.2f), wall);
            CreateCube("Front_Right", parent, new Vector3(3.75f, 1.3f, -5.1f), new Vector3(0.9f, 2.6f, 0.2f), wall);
            CreateCube("GarageDoor", parent, new Vector3(0f, 1.25f, -5f), new Vector3(6.6f, 2.5f, 0.12f), metal);

            Transform workshop = new GameObject("WorkshopCorner").transform;
            workshop.SetParent(parent, false);
            CreateCube("WorkbenchBase", workshop, new Vector3(0f, 0.45f, 4.35f), new Vector3(3.2f, 0.8f, 0.75f), metal);
            CreateCube("WorkbenchTop", workshop, new Vector3(0f, 0.9f, 4.35f), new Vector3(3.5f, 0.12f, 0.95f), accent);
            CreateCube("Backboard", workshop, new Vector3(0f, 1.75f, 4.72f), new Vector3(3.5f, 1.55f, 0.08f), metal);

            Transform shelf = new GameObject("StarterShelf").transform;
            shelf.SetParent(parent, false);
            shelf.position = new Vector3(3.15f, 0f, 1.4f);
            CreateCube("Shelf_LeftPost", shelf, new Vector3(-0.65f, 1.1f, 0f), new Vector3(0.08f, 2.2f, 0.65f), metal);
            CreateCube("Shelf_RightPost", shelf, new Vector3(0.65f, 1.1f, 0f), new Vector3(0.08f, 2.2f, 0.65f), metal);
            for (int index = 0; index < 4; index++)
            {
                CreateCube(
                    $"Shelf_{index + 1}",
                    shelf,
                    new Vector3(0f, 0.25f + (index * 0.55f), 0f),
                    new Vector3(1.4f, 0.08f, 0.72f),
                    metal);
            }

            CreateCube("StarterBox_A", parent, new Vector3(-2.8f, 0.3f, 2.8f), new Vector3(0.8f, 0.6f, 0.65f), cardboard);
            CreateCube("StarterBox_B", parent, new Vector3(-2.55f, 0.85f, 2.85f), new Vector3(0.65f, 0.5f, 0.55f), cardboard);
            CreateCube("DeliveryBox", parent, new Vector3(2.4f, 0.4f, -3.8f), new Vector3(1.1f, 0.8f, 0.9f), cardboard);

            for (int stripe = -3; stripe <= 3; stripe++)
            {
                GameObject stripeObject = CreateCube(
                    $"SafetyStripe_{stripe + 4}",
                    parent,
                    new Vector3(stripe * 0.75f, 0.015f, -4.25f),
                    new Vector3(0.42f, 0.025f, 0.9f),
                    accent);
                stripeObject.transform.rotation = Quaternion.Euler(0f, 25f, 0f);
                Collider collider = stripeObject.GetComponent<Collider>();
                if (collider != null)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                }
            }

            GameObject stockSurfaceObject = CreateCube(
                "SmallBoxStockPlacementSurface",
                parent,
                new Vector3(2.15f, 0.03f, -1.1f),
                new Vector3(2.3f, 0.06f, 1.7f),
                stockPlacement);
            BoxCollider stockSurfaceCollider = stockSurfaceObject.GetComponent<BoxCollider>();
            PlacementSurface stockSurface = stockSurfaceObject.AddComponent<PlacementSurface>();
            stockSurface.Configure(
                "prototype.stock-floor-small-box-a",
                stockSurfaceCollider,
                0.25f,
                90f);

            GameObject placementBlocker = CreateCube(
                "StockPlacementBlocker",
                parent,
                new Vector3(2.92f, 0.24f, -1.1f),
                new Vector3(0.35f, 0.42f, 0.65f),
                metal);
            placementBlocker.layer = RequireLayer(InteractableLayerName);
        }

        private static void BuildLighting(Transform parent)
        {
            GameObject sun = new GameObject("Directional Light");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(50f, -32f, 0f);
            Light sunlight = sun.AddComponent<Light>();
            sunlight.type = LightType.Directional;
            sunlight.color = new Color(1f, 0.93f, 0.82f);
            sunlight.intensity = 1.1f;
            sunlight.shadows = LightShadows.Soft;

            CreatePointLight(parent, "CeilingLight_A", new Vector3(-2.2f, 2.72f, 0.8f));
            CreatePointLight(parent, "CeilingLight_B", new Vector3(2.2f, 2.72f, -1.7f));
        }

        private static FirstPersonMotor BuildPlayer(
            Transform gameplay,
            InputActionAsset inputActions,
            Material handsMaterial,
            Material placementValidMaterial,
            Material placementInvalidMaterial)
        {
            GameObject rig = new GameObject("PlayerRig");
            rig.transform.SetParent(gameplay, false);
            rig.layer = RequireLayer(PlayerLayerName);

            CharacterController controller = rig.AddComponent<CharacterController>();
            controller.height = 1.75f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.875f, 0f);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 45f;
            controller.skinWidth = 0.04f;

            PlayerInputAdapter input = rig.AddComponent<PlayerInputAdapter>();
            input.Configure(inputActions);

            GameObject pivotObject = new GameObject("CameraPivot");
            pivotObject.transform.SetParent(rig.transform, false);
            pivotObject.transform.localPosition = new Vector3(0f, 1.62f, 0f);

            GameObject cameraObject = new GameObject("MainCamera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(pivotObject.transform, false);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.nearClipPlane = 0.06f;
            camera.farClipPlane = 150f;
            camera.fieldOfView = 72f;
            camera.clearFlags = CameraClearFlags.Skybox;
            cameraObject.AddComponent<AudioListener>();

            Transform interactionOrigin = new GameObject("InteractionOrigin").transform;
            interactionOrigin.SetParent(cameraObject.transform, false);
            interactionOrigin.localPosition = new Vector3(0f, 0f, 0.1f);

            Transform carryAnchor = new GameObject("CarryAnchor").transform;
            carryAnchor.SetParent(cameraObject.transform, false);
            carryAnchor.localPosition = new Vector3(0f, -0.15f, 1.35f);

            Transform viewModelHands = new GameObject("ViewModelHands").transform;
            viewModelHands.SetParent(cameraObject.transform, false);
            Transform leftHand = CreateViewModelHand(
                "LeftHand",
                viewModelHands,
                new Vector3(-0.23f, -0.30f, 0.55f),
                -8f,
                handsMaterial);
            Transform rightHand = CreateViewModelHand(
                "RightHand",
                viewModelHands,
                new Vector3(0.23f, -0.30f, 0.55f),
                8f,
                handsMaterial);
            SetLayerRecursively(viewModelHands.gameObject, RequireLayer(ViewModelLayerName));

            VisibleHandsPresenter handsPresenter = viewModelHands.gameObject.AddComponent<VisibleHandsPresenter>();
            handsPresenter.Configure(leftHand, rightHand);

            GameObject placementPreviewObject = CreateCube(
                "PlacementPreview",
                rig.transform,
                Vector3.zero,
                Vector3.one,
                placementValidMaterial);
            Collider placementPreviewCollider = placementPreviewObject.GetComponent<Collider>();
            if (placementPreviewCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(placementPreviewCollider);
            }

            PlacementPreview placementPreview = placementPreviewObject.AddComponent<PlacementPreview>();
            placementPreview.Configure(placementValidMaterial, placementInvalidMaterial);

            FirstPersonMotor motor = rig.AddComponent<FirstPersonMotor>();
            motor.Configure(controller, input, pivotObject.transform, camera);

            PhysicalInteractionResolver resolver = rig.AddComponent<PhysicalInteractionResolver>();
            int interactableLayer = RequireLayer(InteractableLayerName);
            int playerLayer = RequireLayer(PlayerLayerName);
            resolver.Configure(
                interactionOrigin,
                rig.transform,
                2f,
                0.08f,
                (1 << 0) | (1 << interactableLayer));

            PlayerCarryController carryController = rig.AddComponent<PlayerCarryController>();
            carryController.Configure(
                input,
                motor,
                resolver,
                carryAnchor,
                handsPresenter,
                placementPreview,
                1 << 0,
                (1 << 0) | (1 << interactableLayer) | (1 << playerLayer),
                RequireLayer(HeldItemLayerName));
            return motor;
        }

        private static Transform CreateViewModelHand(
            string name,
            Transform parent,
            Vector3 position,
            float yaw,
            Material material)
        {
            GameObject hand = CreateCube(name, parent, position, new Vector3(0.13f, 0.11f, 0.38f), material);
            hand.transform.localRotation = Quaternion.Euler(12f, yaw, 0f);
            Collider collider = hand.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return hand.transform;
        }

        private static void BuildStarterPickups(
            Transform parent,
            Material cardboard,
            Material metal,
            Material accent)
        {
            int interactableLayer = RequireLayer(InteractableLayerName);
            CreateCube(
                "SmallBoxPickupPedestal",
                parent,
                new Vector3(0f, 0.6f, -0.65f),
                new Vector3(0.75f, 1.2f, 0.75f),
                metal);
            GameObject itemRoot = new GameObject("StarterPickupBox");
            itemRoot.transform.SetParent(parent, false);
            itemRoot.transform.localPosition = new Vector3(0f, 1.5f, -0.65f);
            itemRoot.layer = interactableLayer;

            GameObject visual = CreateCube(
                "BoxVisual",
                itemRoot.transform,
                Vector3.zero,
                new Vector3(0.55f, 0.55f, 0.55f),
                cardboard);
            visual.layer = interactableLayer;

            Rigidbody body = itemRoot.AddComponent<Rigidbody>();
            body.mass = 2f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            PhysicalItemProjection item = itemRoot.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "prototype.garage-box-001",
                "Parça Kutusu",
                body,
                new Vector3(0.275f, 0.275f, 0.275f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.SmallBox);

            CreateCube(
                "LargeBoxPickupPedestal",
                parent,
                new Vector3(-1.5f, 0.55f, -0.65f),
                new Vector3(1.25f, 1.1f, 0.9f),
                metal);
            GameObject largeItemRoot = new GameObject("HeavyShipmentBox");
            largeItemRoot.transform.SetParent(parent, false);
            largeItemRoot.transform.localPosition = new Vector3(-1.5f, 1.5f, -0.65f);
            largeItemRoot.layer = interactableLayer;

            GameObject largeVisual = CreateCube(
                "LargeBoxVisual",
                largeItemRoot.transform,
                Vector3.zero,
                new Vector3(1.1f, 0.8f, 0.7f),
                cardboard);
            largeVisual.layer = interactableLayer;

            for (int band = -1; band <= 1; band += 2)
            {
                GameObject warningBand = CreateCube(
                    $"HeavyLoadBand_{(band < 0 ? "Left" : "Right")}",
                    largeItemRoot.transform,
                    new Vector3(band * 0.28f, 0f, -0.356f),
                    new Vector3(0.12f, 0.72f, 0.02f),
                    accent);
                warningBand.layer = interactableLayer;
                Collider bandCollider = warningBand.GetComponent<Collider>();
                if (bandCollider != null)
                {
                    UnityEngine.Object.DestroyImmediate(bandCollider);
                }
            }

            Rigidbody largeBody = largeItemRoot.AddComponent<Rigidbody>();
            largeBody.mass = 9f;
            largeBody.interpolation = RigidbodyInterpolation.Interpolate;
            largeBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            PhysicalItemProjection largeItem = largeItemRoot.AddComponent<PhysicalItemProjection>();
            largeItem.Configure(
                "prototype.garage-large-box-001",
                "Büyük Kargo Kutusu",
                largeBody,
                new Vector3(0.55f, 0.4f, 0.35f),
                new Vector3(0f, -0.30f, -0.32f),
                Vector3.zero,
                PhysicalCarryProfile.LargeBox);
        }

        private static void CreatePointLight(Transform parent, string name, Vector3 position)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.86f, 0.70f);
            light.range = 8f;
            light.intensity = 1.6f;
            light.shadows = LightShadows.None;
        }

        private static GameObject CreateRoot(string name)
        {
            return new GameObject(name);
        }

        private static int RequireLayer(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            Require(layer >= 0, $"Required layer is missing: {layerName}");
            return layer;
        }

        private static void SetLayerRecursively(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private static GameObject CreateCube(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent, false);
            cube.transform.localPosition = localPosition;
            cube.transform.localScale = localScale;
            cube.GetComponent<Renderer>().sharedMaterial = material;
            return cube;
        }

        private static Material GetOrCreateMaterial(
            string name,
            Color color,
            float metallic,
            float smoothness)
        {
            string path = $"{MaterialRoot}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                Require(shader != null, "A supported lit shader is unavailable.");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }

            material.color = color;
            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateGhostMaterial(string name, Color color)
        {
            Material material = GetOrCreateMaterial(name, color, 0f, 0.15f);
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
            }

            if (material.HasProperty("_SrcBlend"))
            {
                material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            }

            if (material.HasProperty("_DstBlend"))
            {
                material.SetFloat(
                    "_DstBlend",
                    (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void SetBuildScenes()
        {
            EditorBuildSettings.scenes = ComposeBuildScenes(EditorBuildSettings.scenes);
        }

        public static EditorBuildSettingsScene[] ComposeBuildScenes(
            IEnumerable<EditorBuildSettingsScene> existingScenes)
        {
            Require(existingScenes != null, "Existing build scenes are required.");
            var scenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(GaragePrototypeMarker.ScenePath, true)
            };

            scenes.AddRange(existingScenes.Where(scene => !string.Equals(
                scene.path,
                GaragePrototypeMarker.ScenePath,
                StringComparison.Ordinal)));
            if (!scenes.Any(scene => string.Equals(
                    scene.path,
                    SampleScenePath,
                    StringComparison.Ordinal)))
            {
                scenes.Add(new EditorBuildSettingsScene(SampleScenePath, true));
            }

            return scenes.ToArray();
        }

        private static void EnsureProjectDirectories()
        {
            string[] relativePaths =
            {
                "Assets/Scenes/Prototypes",
                MaterialRoot,
                "Assets/Prefabs/Prototype"
            };

            foreach (string path in relativePaths)
            {
                string absolute = Path.Combine(
                    Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty,
                    path);
                Directory.CreateDirectory(absolute);
            }

            AssetDatabase.Refresh();
        }

        private static void ValidateInputContract(InputActionAsset actions)
        {
            InputActionMap player = actions.FindActionMap(PlayerInputContract.PlayerMap, true);
            string[] requiredActions =
            {
                PlayerInputContract.Move,
                PlayerInputContract.Look,
                PlayerInputContract.PrimaryAction,
                PlayerInputContract.Interact,
                PlayerInputContract.Sprint,
                PlayerInputContract.Drop,
                PlayerInputContract.Pause
            };
            foreach (string actionName in requiredActions)
            {
                player.FindAction(actionName, true);
            }

            Require(actions.controlSchemes.Count == 2, "Only Keyboard&Mouse and Gamepad are supported here.");
            Require(actions.controlSchemes.Any(scheme => scheme.name == PlayerInputContract.KeyboardAndMouseScheme),
                "Keyboard&Mouse control scheme is missing.");
            Require(actions.controlSchemes.Any(scheme => scheme.name == PlayerInputContract.GamepadScheme),
                "Gamepad control scheme is missing.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new InvalidOperationException(message);
            }
        }
    }
}
