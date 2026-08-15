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
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static class GarageGrayboxSceneBuilder
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string MaterialRoot = "Assets/Art/Prototype/Materials";
        private const string TextureRoot = "Assets/Art/Prototype/Textures";
        private const string LookdevProfilePath = "Assets/Art/Prototype/GarageLookdevProfile.asset";
        private const string PlayerPrefabPath = "Assets/Prefabs/Prototype/PlayerRig.prefab";
        private const string PlayerLayerName = "Player";
        private const string InteractableLayerName = "Interactable";
        private const string HeldItemLayerName = "HeldItem";
        private const string ViewModelLayerName = "ViewModel";

        private enum SurfacePattern
        {
            Concrete,
            PaintedWall,
            BrushedMetal,
            Cardboard,
            WoodLaminate
        }

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

            Texture2D concreteDetail = GetOrCreateSurfaceTexture(
                "ConcreteDetail",
                SurfacePattern.Concrete);
            Texture2D wallDetail = GetOrCreateSurfaceTexture(
                "PaintedWallDetail",
                SurfacePattern.PaintedWall);
            Texture2D metalDetail = GetOrCreateSurfaceTexture(
                "BrushedMetalDetail",
                SurfacePattern.BrushedMetal);
            Texture2D cardboardDetail = GetOrCreateSurfaceTexture(
                "CardboardFiberDetail",
                SurfacePattern.Cardboard);
            Texture2D woodDetail = GetOrCreateSurfaceTexture(
                "WoodLaminateDetail",
                SurfacePattern.WoodLaminate);

            Material concrete = GetOrCreateMaterial(
                "Concrete",
                new Color(0.34f, 0.35f, 0.36f),
                0f,
                0.16f,
                concreteDetail,
                new Vector2(5f, 6f));
            Material wall = GetOrCreateMaterial(
                "WarmWall",
                new Color(0.72f, 0.68f, 0.61f),
                0f,
                0.24f,
                wallDetail,
                new Vector2(4f, 3f));
            Material metal = GetOrCreateMaterial(
                "DarkMetal",
                new Color(0.25f, 0.28f, 0.31f),
                0.08f,
                0.34f,
                metalDetail,
                new Vector2(2f, 8f));
            Material brushedSteel = GetOrCreateMaterial(
                "BrushedSteel",
                new Color(0.58f, 0.60f, 0.62f),
                0.90f,
                0.54f,
                metalDetail,
                new Vector2(2f, 10f));
            Material accent = GetOrCreateMaterial(
                "SafetyAccent",
                new Color(0.82f, 0.39f, 0.06f),
                0.18f,
                0.34f);
            Material cardboard = GetOrCreateMaterial(
                "Cardboard",
                new Color(0.57f, 0.36f, 0.19f),
                0f,
                0.09f,
                cardboardDetail,
                new Vector2(3f, 2f));
            Material wood = GetOrCreateMaterial(
                "WoodLaminate",
                new Color(0.48f, 0.27f, 0.11f),
                0f,
                0.32f,
                woodDetail,
                new Vector2(3.5f, 1.2f));
            Material rubber = GetOrCreateMaterial(
                "WorkshopRubber",
                new Color(0.035f, 0.045f, 0.05f),
                0f,
                0.24f);
            Material labelPaper = GetOrCreateMaterial(
                "LabelPaper",
                new Color(0.82f, 0.79f, 0.68f),
                0f,
                0.18f);
            Material screenGlass = GetOrCreateEmissiveMaterial(
                "ScreenGlass",
                new Color(0.018f, 0.045f, 0.055f),
                0.08f,
                0.84f,
                new Color(0.04f, 0.42f, 0.56f) * 1.6f);
            Material lightDiffuser = GetOrCreateEmissiveMaterial(
                "WarmLightDiffuser",
                new Color(0.82f, 0.76f, 0.65f),
                0f,
                0.62f,
                new Color(1f, 0.70f, 0.40f) * 3.2f);
            Material hands = GetOrCreateMaterial(
                "PrototypeHands",
                new Color(0.10f, 0.25f, 0.31f),
                0.02f,
                0.29f);
            Material stockPlacement = GetOrCreateMaterial(
                "StockPlacementSurface",
                new Color(0.08f, 0.42f, 0.48f),
                0.05f,
                0.22f,
                enableInstancing: false);
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

            BuildRoom(
                environment,
                concrete,
                wall,
                metal,
                brushedSteel,
                accent,
                cardboard,
                wood,
                rubber,
                labelPaper,
                screenGlass,
                lightDiffuser,
                stockPlacement);
            BuildStarterPickups(environment, cardboard, metal, accent, labelPaper, rubber);
            BuildLighting(lighting, metal, lightDiffuser);
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

            BuildLookdevVolume(lighting);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.39f, 0.43f, 0.48f);
            RenderSettings.ambientEquatorColor = new Color(0.31f, 0.30f, 0.28f);
            RenderSettings.ambientGroundColor = new Color(0.15f, 0.14f, 0.13f);
            RenderSettings.ambientIntensity = 1.18f;
            RenderSettings.reflectionIntensity = 0.90f;
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
            Material brushedSteel,
            Material accent,
            Material cardboard,
            Material wood,
            Material rubber,
            Material labelPaper,
            Material screenGlass,
            Material lightDiffuser,
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

            CreateDetailCube(
                "FloorJoint_Long",
                parent,
                new Vector3(0f, 0.006f, 0.2f),
                new Vector3(0.018f, 0.008f, 9.5f),
                rubber);
            CreateDetailCube(
                "FloorJoint_Cross",
                parent,
                new Vector3(0f, 0.007f, 1.8f),
                new Vector3(7.7f, 0.009f, 0.018f),
                rubber);
            CreateDetailCube(
                "BackBaseboard",
                parent,
                new Vector3(0f, 0.10f, 4.965f),
                new Vector3(7.9f, 0.20f, 0.07f),
                metal);
            CreateDetailCube(
                "LeftBaseboard",
                parent,
                new Vector3(-3.965f, 0.10f, 0f),
                new Vector3(0.07f, 0.20f, 9.8f),
                metal);
            CreateDetailCube(
                "RightBaseboard",
                parent,
                new Vector3(3.965f, 0.10f, 0f),
                new Vector3(0.07f, 0.20f, 9.8f),
                metal);

            for (int panel = 1; panel < 5; panel++)
            {
                CreateDetailCube(
                    $"GarageDoorSeam_{panel}",
                    parent,
                    new Vector3(0f, 0.02f + (panel * 0.49f), -4.932f),
                    new Vector3(6.3f, 0.025f, 0.018f),
                    rubber);
            }

            BuildVisualBenchmarkCorner(
                parent,
                metal,
                brushedSteel,
                accent,
                cardboard,
                wood,
                rubber,
                labelPaper,
                screenGlass,
                lightDiffuser);

            BuildStaticShippingBox(
                "StarterBox_A",
                parent,
                new Vector3(-2.8f, 0.3f, 2.8f),
                new Vector3(0.8f, 0.6f, 0.65f),
                cardboard,
                labelPaper,
                rubber);
            BuildStaticShippingBox(
                "StarterBox_B",
                parent,
                new Vector3(-2.55f, 0.85f, 2.85f),
                new Vector3(0.65f, 0.5f, 0.55f),
                cardboard,
                labelPaper,
                rubber);
            BuildStaticShippingBox(
                "DeliveryBox",
                parent,
                new Vector3(2.4f, 0.4f, -3.8f),
                new Vector3(1.1f, 0.8f, 0.9f),
                cardboard,
                labelPaper,
                rubber);

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

        private static void BuildVisualBenchmarkCorner(
            Transform parent,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material cardboard,
            Material wood,
            Material rubber,
            Material labelPaper,
            Material screenGlass,
            Material lightDiffuser)
        {
            Transform benchmark = new GameObject("VisualBenchmarkCorner").transform;
            benchmark.SetParent(parent, false);

            Transform workshop = new GameObject("WorkshopCorner").transform;
            workshop.SetParent(benchmark, false);

            CreateBeveledCube(
                "WorkbenchTop",
                workshop,
                new Vector3(0f, 0.93f, 4.35f),
                new Vector3(3.5f, 0.12f, 0.95f),
                0.025f,
                wood);
            foreach (float x in new[] { -1.52f, 1.52f })
            {
                foreach (float z in new[] { 4.04f, 4.66f })
                {
                    CreateBeveledCube(
                        $"WorkbenchLeg_{x:0.00}_{z:0.00}",
                        workshop,
                        new Vector3(x, 0.46f, z),
                        new Vector3(0.09f, 0.86f, 0.09f),
                        0.012f,
                        brushedSteel);
                    CreateBeveledCube(
                        $"WorkbenchFoot_{x:0.00}_{z:0.00}",
                        workshop,
                        new Vector3(x, 0.035f, z),
                        new Vector3(0.16f, 0.07f, 0.16f),
                        0.018f,
                        rubber);
                }
            }

            CreateBeveledCube(
                "WorkbenchBackRail",
                workshop,
                new Vector3(0f, 0.48f, 4.66f),
                new Vector3(3.08f, 0.10f, 0.09f),
                0.012f,
                brushedSteel);
            CreateBeveledCube(
                "DrawerCabinet",
                workshop,
                new Vector3(-1.08f, 0.48f, 4.35f),
                new Vector3(0.72f, 0.78f, 0.62f),
                0.035f,
                metal);
            for (int drawer = 0; drawer < 3; drawer++)
            {
                float drawerY = 0.25f + (drawer * 0.24f);
                CreateBeveledCube(
                    $"DrawerFront_{drawer + 1}",
                    workshop,
                    new Vector3(-1.08f, drawerY, 4.025f),
                    new Vector3(0.62f, 0.18f, 0.035f),
                    0.012f,
                    metal,
                    false);
                CreateDetailCube(
                    $"DrawerHandle_{drawer + 1}",
                    workshop,
                    new Vector3(-1.08f, drawerY, 3.997f),
                    new Vector3(0.26f, 0.025f, 0.025f),
                    brushedSteel);
            }

            CreateBeveledCube(
                "Pegboard",
                workshop,
                new Vector3(0f, 1.76f, 4.77f),
                new Vector3(3.5f, 1.48f, 0.08f),
                0.025f,
                rubber);
            CreateDetailCube(
                "PegboardTopFrame",
                workshop,
                new Vector3(0f, 2.49f, 4.72f),
                new Vector3(3.5f, 0.055f, 0.055f),
                brushedSteel);
            CreateDetailCube(
                "PegboardBottomFrame",
                workshop,
                new Vector3(0f, 1.03f, 4.72f),
                new Vector3(3.5f, 0.055f, 0.055f),
                brushedSteel);
            for (int column = 0; column < 13; column++)
            {
                for (int row = 0; row < 6; row++)
                {
                    CreateDetailCube(
                        $"PegHole_{column:00}_{row:00}",
                        workshop,
                        new Vector3(-1.50f + (column * 0.25f), 1.16f + (row * 0.22f), 4.721f),
                        new Vector3(0.027f, 0.027f, 0.012f),
                        metal);
                }
            }

            CreateDetailCube(
                "BenchIdentityPlate",
                workshop,
                new Vector3(-0.23f, 2.25f, 4.708f),
                new Vector3(0.72f, 0.18f, 0.018f),
                labelPaper);
            for (int bar = 0; bar < 4; bar++)
            {
                CreateDetailCube(
                    $"BenchIdentityBar_{bar + 1}",
                    workshop,
                    new Vector3(-0.46f + (bar * 0.15f), 2.25f, 4.696f),
                    new Vector3(0.07f, 0.065f + (bar * 0.012f), 0.008f),
                    rubber);
            }

            CreateBeveledCube(
                "DiagnosticMonitorBody",
                workshop,
                new Vector3(0.66f, 1.36f, 4.15f),
                new Vector3(0.82f, 0.52f, 0.09f),
                0.035f,
                rubber,
                false);
            CreateDetailCube(
                "DiagnosticMonitorScreen",
                workshop,
                new Vector3(0.66f, 1.36f, 4.098f),
                new Vector3(0.72f, 0.42f, 0.018f),
                screenGlass);
            CreateBeveledCube(
                "DiagnosticMonitorStand",
                workshop,
                new Vector3(0.66f, 1.09f, 4.20f),
                new Vector3(0.09f, 0.18f, 0.09f),
                0.015f,
                brushedSteel,
                false);
            CreateBeveledCube(
                "DiagnosticKeyboard",
                workshop,
                new Vector3(0.65f, 1.015f, 3.98f),
                new Vector3(0.72f, 0.045f, 0.23f),
                0.018f,
                rubber,
                false);

            CreateBeveledCube(
                "BenchPcCase",
                workshop,
                new Vector3(1.30f, 1.31f, 4.35f),
                new Vector3(0.42f, 0.62f, 0.55f),
                0.035f,
                metal);
            for (int vent = 0; vent < 5; vent++)
            {
                CreateDetailCube(
                    $"BenchPcVent_{vent + 1}",
                    workshop,
                    new Vector3(1.30f, 1.16f + (vent * 0.075f), 4.064f),
                    new Vector3(0.25f, 0.025f, 0.012f),
                    rubber);
            }

            CreateDetailCube(
                "WorkbenchLightHousing",
                workshop,
                new Vector3(0f, 2.43f, 4.63f),
                new Vector3(1.95f, 0.10f, 0.18f),
                brushedSteel);
            CreateDetailCube(
                "WorkbenchLightDiffuser",
                workshop,
                new Vector3(0f, 2.375f, 4.56f),
                new Vector3(1.72f, 0.025f, 0.08f),
                lightDiffuser);

            Transform shelf = new GameObject("StarterShelf").transform;
            shelf.SetParent(benchmark, false);
            shelf.localPosition = new Vector3(3.15f, 0f, 1.4f);
            foreach (float x in new[] { -0.65f, 0.65f })
            {
                CreateBeveledCube(
                    x < 0f ? "Shelf_LeftPost" : "Shelf_RightPost",
                    shelf,
                    new Vector3(x, 1.1f, 0f),
                    new Vector3(0.08f, 2.2f, 0.65f),
                    0.012f,
                    metal);
                CreateBeveledCube(
                    x < 0f ? "Shelf_LeftFoot" : "Shelf_RightFoot",
                    shelf,
                    new Vector3(x, 0.035f, -0.02f),
                    new Vector3(0.16f, 0.07f, 0.72f),
                    0.015f,
                    rubber);
            }

            for (int index = 0; index < 4; index++)
            {
                CreateBeveledCube(
                    $"Shelf_{index + 1}",
                    shelf,
                    new Vector3(0f, 0.25f + (index * 0.55f), 0f),
                    new Vector3(1.4f, 0.08f, 0.72f),
                    0.012f,
                    metal);
            }

            BuildStaticShippingBox(
                "ShelfPartsBox",
                shelf,
                new Vector3(-0.28f, 0.57f, 0f),
                new Vector3(0.56f, 0.50f, 0.50f),
                cardboard,
                labelPaper,
                rubber);
            CreateBeveledCube(
                "ShelfTechUnit",
                shelf,
                new Vector3(0.20f, 1.12f, 0f),
                new Vector3(0.76f, 0.46f, 0.48f),
                0.03f,
                metal);
            CreateDetailCube(
                "ShelfTechDisplay",
                shelf,
                new Vector3(0.20f, 1.12f, -0.247f),
                new Vector3(0.40f, 0.16f, 0.016f),
                screenGlass);
        }

        private static void BuildStaticShippingBox(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 size,
            Material cardboard,
            Material labelPaper,
            Material tape)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = localPosition;
            CreateBeveledCube("Carton", root, Vector3.zero, size, 0.018f, cardboard);
            CreateDetailCube(
                "PackingTape",
                root,
                new Vector3(0f, (size.y * 0.5f) + 0.006f, 0f),
                new Vector3(size.x * 0.18f, 0.012f, size.z * 0.96f),
                tape);
            CreateDetailCube(
                "ShippingLabel",
                root,
                new Vector3(size.x * 0.20f, 0f, -(size.z * 0.5f) - 0.006f),
                new Vector3(size.x * 0.34f, size.y * 0.34f, 0.012f),
                labelPaper);
            for (int bar = 0; bar < 3; bar++)
            {
                CreateDetailCube(
                    $"LabelBar_{bar + 1}",
                    root,
                    new Vector3(
                        size.x * 0.20f,
                        (size.y * 0.065f) - (bar * size.y * 0.07f),
                        -(size.z * 0.5f) - 0.013f),
                    new Vector3(size.x * (0.22f - (bar * 0.035f)), size.y * 0.025f, 0.006f),
                    tape);
            }
        }

        private static void BuildLighting(
            Transform parent,
            Material metal,
            Material lightDiffuser)
        {
            GameObject sun = new GameObject("Directional Light");
            sun.transform.SetParent(parent, false);
            sun.transform.rotation = Quaternion.Euler(42f, -28f, 0f);
            Light sunlight = sun.AddComponent<Light>();
            sunlight.type = LightType.Directional;
            sunlight.color = new Color(0.84f, 0.91f, 1f);
            sunlight.intensity = 0.92f;
            sunlight.shadows = LightShadows.Soft;
            sunlight.shadowStrength = 0.82f;

            CreateCeilingFixture(
                parent,
                "CeilingFixture_A",
                new Vector3(-2.15f, 2.98f, 0.8f),
                metal,
                lightDiffuser);
            CreateCeilingFixture(
                parent,
                "CeilingFixture_B",
                new Vector3(2.15f, 2.98f, -1.7f),
                metal,
                lightDiffuser);
            CreatePointLight(
                parent,
                "CeilingBounce_A",
                new Vector3(-2.15f, 2.66f, 0.8f),
                new Color(1f, 0.79f, 0.60f),
                1.45f,
                6.5f);
            CreatePointLight(
                parent,
                "CeilingBounce_B",
                new Vector3(2.15f, 2.66f, -1.7f),
                new Color(0.78f, 0.88f, 1f),
                1.25f,
                6.2f);
            CreateSpotLight(
                parent,
                "WorkbenchTaskLight",
                new Vector3(0f, 2.34f, 4.44f),
                new Vector3(0f, 1.05f, 4.12f),
                new Color(1f, 0.77f, 0.55f),
                3.8f,
                3.4f,
                68f);

            GameObject reflectionObject = new GameObject("GarageReflectionProbe");
            reflectionObject.transform.SetParent(parent, false);
            reflectionObject.transform.localPosition = new Vector3(0f, 1.45f, 0.5f);
            ReflectionProbe reflectionProbe = reflectionObject.AddComponent<ReflectionProbe>();
            reflectionProbe.mode = ReflectionProbeMode.Realtime;
            reflectionProbe.refreshMode = ReflectionProbeRefreshMode.OnAwake;
            reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
            reflectionProbe.resolution = 128;
            reflectionProbe.size = new Vector3(7.6f, 2.8f, 9.4f);
            reflectionProbe.boxProjection = true;
            reflectionProbe.intensity = 0.85f;
        }

        private static void BuildLookdevVolume(Transform parent)
        {
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(LookdevProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                profile.name = "GarageLookdevProfile";
                AssetDatabase.CreateAsset(profile, LookdevProfilePath);
            }

            Tonemapping tonemapping = GetOrAddVolumeComponent<Tonemapping>(profile);
            tonemapping.mode.Override(TonemappingMode.ACES);

            ColorAdjustments color = GetOrAddVolumeComponent<ColorAdjustments>(profile);
            color.postExposure.Override(0.42f);
            color.contrast.Override(6f);
            color.saturation.Override(-3f);
            color.colorFilter.Override(new Color(1f, 0.985f, 0.955f));

            WhiteBalance whiteBalance = GetOrAddVolumeComponent<WhiteBalance>(profile);
            whiteBalance.temperature.Override(-2f);
            whiteBalance.tint.Override(1f);

            Bloom bloom = GetOrAddVolumeComponent<Bloom>(profile);
            bloom.intensity.Override(0.14f);
            bloom.threshold.Override(1.05f);
            bloom.scatter.Override(0.52f);

            Vignette vignette = GetOrAddVolumeComponent<Vignette>(profile);
            vignette.intensity.Override(0.08f);
            vignette.smoothness.Override(0.34f);
            vignette.rounded.Override(false);

            EditorUtility.SetDirty(profile);
            GameObject volumeObject = new GameObject("GlobalLookdevVolume");
            volumeObject.transform.SetParent(parent, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;
            volume.sharedProfile = profile;
        }

        private static T GetOrAddVolumeComponent<T>(VolumeProfile profile)
            where T : VolumeComponent
        {
            if (profile.TryGet(out T component))
            {
                return component;
            }

            component = profile.Add<T>(true);
            AssetDatabase.AddObjectToAsset(component, profile);
            return component;
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
            camera.allowHDR = true;
            UniversalAdditionalCameraData cameraData = camera.GetUniversalAdditionalCameraData();
            cameraData.renderPostProcessing = true;
            cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            cameraData.antialiasingQuality = AntialiasingQuality.High;
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
                1 << interactableLayer,
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
            Material accent,
            Material labelPaper,
            Material tape)
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
            itemRoot.transform.localPosition = new Vector3(0f, 1.45f, -0.65f);
            itemRoot.layer = interactableLayer;

            GameObject visual = CreateCube(
                "BoxVisual",
                itemRoot.transform,
                Vector3.zero,
                new Vector3(0.7f, 0.45f, 0.5f),
                cardboard);
            visual.layer = interactableLayer;

            GameObject orientationMarker = CreateCube(
                "SmallBoxOrientationMarker",
                itemRoot.transform,
                new Vector3(0f, 0.231f, 0.12f),
                new Vector3(0.12f, 0.012f, 0.18f),
                accent);
            orientationMarker.layer = interactableLayer;
            Collider markerCollider = orientationMarker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(markerCollider);
            }

            CreateDetailCube(
                "SmallBoxPackingTape",
                itemRoot.transform,
                new Vector3(0f, 0.231f, 0f),
                new Vector3(0.13f, 0.012f, 0.48f),
                tape);
            CreateDetailCube(
                "SmallBoxShippingLabel",
                itemRoot.transform,
                new Vector3(0.17f, 0f, -0.256f),
                new Vector3(0.24f, 0.14f, 0.012f),
                labelPaper);

            Rigidbody body = itemRoot.AddComponent<Rigidbody>();
            body.mass = 2f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            PhysicalItemProjection item = itemRoot.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "prototype.garage-box-001",
                "Parça Kutusu",
                body,
                new Vector3(0.35f, 0.225f, 0.25f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.SmallBox);

            GameObject stackBaseRoot = new GameObject("StarterStackBaseBox");
            stackBaseRoot.transform.SetParent(parent, false);
            stackBaseRoot.transform.localPosition = new Vector3(1.4f, 0.31f, -1.4f);
            stackBaseRoot.layer = interactableLayer;

            GameObject stackBaseVisual = CreateCube(
                "StackBaseVisual",
                stackBaseRoot.transform,
                Vector3.zero,
                new Vector3(0.7f, 0.45f, 0.5f),
                cardboard);
            stackBaseVisual.layer = interactableLayer;

            GameObject stackBaseMarker = CreateCube(
                "StackBaseOrientationMarker",
                stackBaseRoot.transform,
                new Vector3(0f, 0.231f, 0.12f),
                new Vector3(0.12f, 0.012f, 0.18f),
                accent);
            stackBaseMarker.layer = interactableLayer;
            Collider stackBaseMarkerCollider = stackBaseMarker.GetComponent<Collider>();
            if (stackBaseMarkerCollider != null)
            {
                UnityEngine.Object.DestroyImmediate(stackBaseMarkerCollider);
            }

            CreateDetailCube(
                "StackBasePackingTape",
                stackBaseRoot.transform,
                new Vector3(0f, 0.231f, 0f),
                new Vector3(0.13f, 0.012f, 0.48f),
                tape);
            CreateDetailCube(
                "StackBaseShippingLabel",
                stackBaseRoot.transform,
                new Vector3(0.17f, 0f, -0.256f),
                new Vector3(0.24f, 0.14f, 0.012f),
                labelPaper);

            Rigidbody stackBaseBody = stackBaseRoot.AddComponent<Rigidbody>();
            stackBaseBody.mass = 2f;
            stackBaseBody.interpolation = RigidbodyInterpolation.Interpolate;
            stackBaseBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            stackBaseBody.useGravity = false;
            stackBaseBody.isKinematic = true;

            PhysicalItemProjection stackBaseItem =
                stackBaseRoot.AddComponent<PhysicalItemProjection>();
            stackBaseItem.Configure(
                "prototype.garage-box-002",
                "Stok Kutusu",
                stackBaseBody,
                new Vector3(0.35f, 0.225f, 0.25f),
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

            CreateDetailCube(
                "HeavyShipmentPackingTape",
                largeItemRoot.transform,
                new Vector3(0f, 0.406f, 0f),
                new Vector3(0.16f, 0.012f, 0.68f),
                tape);
            CreateDetailCube(
                "HeavyShipmentLabel",
                largeItemRoot.transform,
                new Vector3(0f, 0.08f, -0.356f),
                new Vector3(0.34f, 0.20f, 0.012f),
                labelPaper);

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

        private static void CreateCeilingFixture(
            Transform parent,
            string name,
            Vector3 position,
            Material housingMaterial,
            Material diffuserMaterial)
        {
            Transform fixture = new GameObject(name).transform;
            fixture.SetParent(parent, false);
            fixture.localPosition = position;
            CreateDetailCube(
                "Housing",
                fixture,
                Vector3.zero,
                new Vector3(1.55f, 0.10f, 0.32f),
                housingMaterial);
            CreateDetailCube(
                "Diffuser",
                fixture,
                new Vector3(0f, -0.061f, 0f),
                new Vector3(1.35f, 0.025f, 0.24f),
                diffuserMaterial);
        }

        private static void CreatePointLight(
            Transform parent,
            string name,
            Vector3 position,
            Color color,
            float intensity,
            float range)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }

        private static void CreateSpotLight(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 target,
            Color color,
            float intensity,
            float range,
            float spotAngle)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localPosition = position;
            lightObject.transform.localRotation = Quaternion.LookRotation(target - position);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = color;
            light.range = range;
            light.intensity = intensity;
            light.spotAngle = spotAngle;
            light.innerSpotAngle = spotAngle * 0.62f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.68f;
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

        private static GameObject CreateDetailCube(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject detail = CreateCube(name, parent, localPosition, localScale, material);
            Collider collider = detail.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }

            return detail;
        }

        private static GameObject CreateBeveledCube(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 size,
            float bevel,
            Material material,
            bool addCollider = true)
        {
            ProBuilderMesh mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
            mesh.name = name;
            mesh.transform.SetParent(parent, false);
            mesh.transform.localPosition = localPosition;
            List<Edge> edges = mesh.faces
                .SelectMany(face => face.edges)
                .Distinct()
                .ToList();
            Bevel.BevelEdges(mesh, edges, Mathf.Min(bevel, Mathf.Min(size.x, Mathf.Min(size.y, size.z)) * 0.35f));
            mesh.ToMesh();
            mesh.Refresh();
            GameObject meshObject = mesh.gameObject;
            meshObject.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (addCollider)
            {
                BoxCollider collider = meshObject.AddComponent<BoxCollider>();
                collider.size = size;
            }

            mesh.preserveMeshAssetOnDestroy = true;
            UnityEngine.Object.DestroyImmediate(mesh);
            return meshObject;
        }

        private static Material GetOrCreateMaterial(
            string name,
            Color color,
            float metallic,
            float smoothness,
            Texture2D surfaceTexture = null,
            Vector2? textureScale = null,
            bool enableInstancing = true)
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

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
                if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", enableInstancing ? color : Color.white);
                }
            }
            else
            {
                material.color = color;
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", metallic);
            }

            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (surfaceTexture != null && material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", surfaceTexture);
                material.SetTextureScale("_BaseMap", textureScale ?? Vector2.one);
            }

            material.enableInstancing = enableInstancing;

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material GetOrCreateEmissiveMaterial(
            string name,
            Color color,
            float metallic,
            float smoothness,
            Color emission)
        {
            Material material = GetOrCreateMaterial(name, color, metallic, smoothness);
            if (material.HasProperty("_EmissionColor"))
            {
                material.SetColor("_EmissionColor", emission);
                material.EnableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Texture2D GetOrCreateSurfaceTexture(
            string name,
            SurfacePattern pattern)
        {
            const int size = 64;
            string path = $"{TextureRoot}/{name}.asset";
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
            {
                texture = new Texture2D(size, size, TextureFormat.RGBA32, false, false)
                {
                    name = name
                };
                AssetDatabase.CreateAsset(texture, path);
            }
            else if (texture.width != size || texture.height != size)
            {
                texture.Reinitialize(size, size, TextureFormat.RGBA32, false);
            }

            var pixels = new Color[size * size];
            int seed = 131 + ((int)pattern * 977);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float noise = Hash01(x, y, seed);
                    float value = EvaluateSurfaceValue(pattern, x, y, noise);
                    pixels[(y * size) + x] = new Color(value, value, value, 1f);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            texture.anisoLevel = 2;
            EditorUtility.SetDirty(texture);
            return texture;
        }

        private static float EvaluateSurfaceValue(
            SurfacePattern pattern,
            int x,
            int y,
            float noise)
        {
            switch (pattern)
            {
                case SurfacePattern.Concrete:
                    float aggregate = Hash01(x / 3, y / 3, 431) > 0.91f ? -0.13f : 0f;
                    return Mathf.Clamp01(0.78f + ((noise - 0.5f) * 0.16f) + aggregate);
                case SurfacePattern.PaintedWall:
                    return Mathf.Clamp01(0.90f + ((noise - 0.5f) * 0.055f));
                case SurfacePattern.BrushedMetal:
                    return Mathf.Clamp01(
                        0.72f + (Mathf.Sin((y * 2.1f) + (noise * 2f)) * 0.045f) +
                        ((noise - 0.5f) * 0.035f));
                case SurfacePattern.Cardboard:
                    return Mathf.Clamp01(
                        0.82f + (Mathf.Sin((x * 0.52f) + (noise * 3f)) * 0.025f) +
                        ((noise - 0.5f) * 0.075f));
                case SurfacePattern.WoodLaminate:
                    float grain = Mathf.Sin((y * 0.36f) + (Mathf.Sin(x * 0.11f) * 1.8f));
                    return Mathf.Clamp01(0.68f + (grain * 0.11f) + ((noise - 0.5f) * 0.05f));
                default:
                    return 1f;
            }
        }

        private static float Hash01(int x, int y, int seed)
        {
            unchecked
            {
                uint hash = (uint)(x * 374761393) + (uint)(y * 668265263) + (uint)(seed * 69069);
                hash = (hash ^ (hash >> 13)) * 1274126177u;
                hash ^= hash >> 16;
                return (hash & 0x00FFFFFFu) / 16777215f;
            }
        }

        private static Material GetOrCreateGhostMaterial(string name, Color color)
        {
            Material material = GetOrCreateMaterial(
                name,
                color,
                0f,
                0.15f,
                enableInstancing: false);
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

            if (material.HasProperty("_DstBlendAlpha"))
            {
                material.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.Zero);
            }

            if (material.HasProperty("_ZWrite"))
            {
                material.SetFloat("_ZWrite", 0f);
            }

            material.SetOverrideTag("RenderType", "Transparent");
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.SetShaderPassEnabled("DepthOnly", true);
            material.SetShaderPassEnabled("ShadowCaster", true);
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
                TextureRoot,
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
                PlayerInputContract.RotatePlacement,
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
