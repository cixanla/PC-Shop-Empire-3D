using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Input;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Presentation.Player;
using PCShopEmpire3D.World.Interaction;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
        private const string MaterialRoot = "Assets/Art/Prototype/Materials";
        private const string TextureRoot = "Assets/Art/Prototype/Textures";
        private const string MeshRoot = "Assets/Art/Prototype/Meshes";
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

        private readonly struct StockFlowBuildResult
        {
            public StockFlowBuildResult(
                InventoryItemWorldBinding binding,
                TextMesh statusText,
                TextMesh shelfOfferText,
                Renderer statusIndicator)
            {
                Binding = binding;
                StatusText = statusText;
                ShelfOfferText = shelfOfferText;
                StatusIndicator = statusIndicator;
            }

            public InventoryItemWorldBinding Binding { get; }

            public TextMesh StatusText { get; }

            public TextMesh ShelfOfferText { get; }

            public Renderer StatusIndicator { get; }
        }

        private readonly struct CustomerFlowBuildResult
        {
            public CustomerFlowBuildResult(
                NavMeshSurface navigationSurface,
                NavMeshAgent customerAgent,
                GameObject customerVisualRoot,
                TextMesh customerStatusText,
                TextMesh customerSpeechText,
                CheckoutStationProjection checkoutStation,
                Collider checkoutInteractionCollider,
                TextMesh checkoutStatusText,
                Transform entranceWaypoint,
                Transform browseWaypoint,
                Transform checkoutWaypoint,
                Transform exitWaypoint)
            {
                NavigationSurface = navigationSurface;
                CustomerAgent = customerAgent;
                CustomerVisualRoot = customerVisualRoot;
                CustomerStatusText = customerStatusText;
                CustomerSpeechText = customerSpeechText;
                CheckoutStation = checkoutStation;
                CheckoutInteractionCollider = checkoutInteractionCollider;
                CheckoutStatusText = checkoutStatusText;
                EntranceWaypoint = entranceWaypoint;
                BrowseWaypoint = browseWaypoint;
                CheckoutWaypoint = checkoutWaypoint;
                ExitWaypoint = exitWaypoint;
            }

            public NavMeshSurface NavigationSurface { get; }

            public NavMeshAgent CustomerAgent { get; }

            public GameObject CustomerVisualRoot { get; }

            public TextMesh CustomerStatusText { get; }

            public TextMesh CustomerSpeechText { get; }

            public CheckoutStationProjection CheckoutStation { get; }

            public Collider CheckoutInteractionCollider { get; }

            public TextMesh CheckoutStatusText { get; }

            public Transform EntranceWaypoint { get; }

            public Transform BrowseWaypoint { get; }

            public Transform CheckoutWaypoint { get; }

            public Transform ExitWaypoint { get; }
        }

        private readonly struct AssemblyBuildResult
        {
            public AssemblyBuildResult(
                MotherboardSeatProjection seat,
                MotherboardFastenerProjection fastener,
                MotherboardAssemblyItemBinding binding,
                PhysicalItemProjection motherboard,
                ProcessorSocketProjection processorSocket,
                ProcessorAssemblyItemBinding processorBinding,
                PhysicalItemProjection processor,
                DimmSlotProjection dimmSlot,
                DimmAssemblyItemBinding dimmBinding,
                PhysicalItemProjection memoryModule,
                M2StorageSlotProjection storageSlot,
                M2StorageAssemblyItemBinding storageBinding,
                PhysicalItemProjection storageDevice,
                ProcessorCoolerSlotProjection processorCoolerSlot,
                ProcessorCoolerAssemblyItemBinding processorCoolerBinding,
                PhysicalItemProjection processorCooler,
                ProcessorCoolerRuntimeGeometry processorCoolerGeometry,
                GraphicsCardSlotProjection graphicsCardSlot,
                GraphicsCardAssemblyItemBinding graphicsCardBinding,
                PhysicalItemProjection graphicsCard,
                PowerSupplyBayProjection powerSupplyBay,
                PowerSupplyAssemblyItemBinding powerSupplyBinding,
                PhysicalItemProjection powerSupply,
                PowerSupplyRuntimeGeometry powerSupplyGeometry,
                Atx24PowerCableRouteProjection atx24PowerCableRoute,
                Atx24PowerCableAssemblyItemBinding atx24PowerCableBinding,
                PhysicalItemProjection atx24PowerCable,
                Atx24PowerCableRuntimeGeometry atx24PowerCableGeometry,
                Eps12vPowerCableRouteProjection eps12vPowerCableRoute,
                Eps12vPowerCableAssemblyItemBinding eps12vPowerCableBinding,
                PhysicalItemProjection eps12vPowerCable,
                Eps12vPowerCableRuntimeGeometry eps12vPowerCableGeometry,
                PcieGpuPowerCableRouteProjection pcieGpuPowerCableRoute,
                PcieGpuPowerCableAssemblyItemBinding pcieGpuPowerCableBinding,
                PhysicalItemProjection pcieGpuPowerCable,
                PcieGpuPowerCableRuntimeGeometry pcieGpuPowerCableGeometry)
            {
                Seat = seat;
                Fastener = fastener;
                Binding = binding;
                Motherboard = motherboard;
                ProcessorSocket = processorSocket;
                ProcessorBinding = processorBinding;
                Processor = processor;
                DimmSlot = dimmSlot;
                DimmBinding = dimmBinding;
                MemoryModule = memoryModule;
                StorageSlot = storageSlot;
                StorageBinding = storageBinding;
                StorageDevice = storageDevice;
                ProcessorCoolerSlot = processorCoolerSlot;
                ProcessorCoolerBinding = processorCoolerBinding;
                ProcessorCooler = processorCooler;
                ProcessorCoolerGeometry = processorCoolerGeometry;
                GraphicsCardSlot = graphicsCardSlot;
                GraphicsCardBinding = graphicsCardBinding;
                GraphicsCard = graphicsCard;
                PowerSupplyBay = powerSupplyBay;
                PowerSupplyBinding = powerSupplyBinding;
                PowerSupply = powerSupply;
                PowerSupplyGeometry = powerSupplyGeometry;
                Atx24PowerCableRoute = atx24PowerCableRoute;
                Atx24PowerCableBinding = atx24PowerCableBinding;
                Atx24PowerCable = atx24PowerCable;
                Atx24PowerCableGeometry = atx24PowerCableGeometry;
                Eps12vPowerCableRoute = eps12vPowerCableRoute;
                Eps12vPowerCableBinding = eps12vPowerCableBinding;
                Eps12vPowerCable = eps12vPowerCable;
                Eps12vPowerCableGeometry = eps12vPowerCableGeometry;
                PcieGpuPowerCableRoute = pcieGpuPowerCableRoute;
                PcieGpuPowerCableBinding = pcieGpuPowerCableBinding;
                PcieGpuPowerCable = pcieGpuPowerCable;
                PcieGpuPowerCableGeometry = pcieGpuPowerCableGeometry;
            }

            public MotherboardSeatProjection Seat { get; }

            public MotherboardFastenerProjection Fastener { get; }

            public MotherboardAssemblyItemBinding Binding { get; }

            public PhysicalItemProjection Motherboard { get; }

            public ProcessorSocketProjection ProcessorSocket { get; }

            public ProcessorAssemblyItemBinding ProcessorBinding { get; }

            public PhysicalItemProjection Processor { get; }

            public DimmSlotProjection DimmSlot { get; }

            public DimmAssemblyItemBinding DimmBinding { get; }

            public PhysicalItemProjection MemoryModule { get; }

            public M2StorageSlotProjection StorageSlot { get; }

            public M2StorageAssemblyItemBinding StorageBinding { get; }

            public PhysicalItemProjection StorageDevice { get; }

            public ProcessorCoolerSlotProjection ProcessorCoolerSlot { get; }

            public ProcessorCoolerAssemblyItemBinding ProcessorCoolerBinding { get; }

            public PhysicalItemProjection ProcessorCooler { get; }

            public ProcessorCoolerRuntimeGeometry ProcessorCoolerGeometry { get; }

            public GraphicsCardSlotProjection GraphicsCardSlot { get; }

            public GraphicsCardAssemblyItemBinding GraphicsCardBinding { get; }

            public PhysicalItemProjection GraphicsCard { get; }

            public PowerSupplyBayProjection PowerSupplyBay { get; }

            public PowerSupplyAssemblyItemBinding PowerSupplyBinding { get; }

            public PhysicalItemProjection PowerSupply { get; }

            public PowerSupplyRuntimeGeometry PowerSupplyGeometry { get; }

            public Atx24PowerCableRouteProjection Atx24PowerCableRoute { get; }

            public Atx24PowerCableAssemblyItemBinding Atx24PowerCableBinding { get; }

            public PhysicalItemProjection Atx24PowerCable { get; }

            public Atx24PowerCableRuntimeGeometry Atx24PowerCableGeometry { get; }

            public Eps12vPowerCableRouteProjection Eps12vPowerCableRoute { get; }

            public Eps12vPowerCableAssemblyItemBinding Eps12vPowerCableBinding { get; }

            public PhysicalItemProjection Eps12vPowerCable { get; }

            public Eps12vPowerCableRuntimeGeometry Eps12vPowerCableGeometry { get; }

            public PcieGpuPowerCableRouteProjection PcieGpuPowerCableRoute { get; }

            public PcieGpuPowerCableAssemblyItemBinding PcieGpuPowerCableBinding { get; }

            public PhysicalItemProjection PcieGpuPowerCable { get; }

            public PcieGpuPowerCableRuntimeGeometry PcieGpuPowerCableGeometry { get; }
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
            Material motherboardPcb = GetOrCreateMaterial(
                "MotherboardPcb",
                new Color(0.035f, 0.16f, 0.105f),
                0.06f,
                0.28f);
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
            Material customerJacket = GetOrCreateMaterial(
                "CustomerJacket",
                new Color(0.055f, 0.12f, 0.19f),
                0.03f,
                0.31f);
            Material customerSkin = GetOrCreateMaterial(
                "CustomerSkin",
                new Color(0.54f, 0.32f, 0.21f),
                0f,
                0.24f);
            Material customerDenim = GetOrCreateMaterial(
                "CustomerDenim",
                new Color(0.075f, 0.14f, 0.21f),
                0f,
                0.20f);
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
            Material deliveryArrived = GetOrCreateEmissiveMaterial(
                "DeliveryStatusArrived",
                new Color(0.82f, 0.39f, 0.06f),
                0.1f,
                0.35f,
                new Color(1f, 0.26f, 0.02f) * 2.4f);
            Material deliveryAccepted = GetOrCreateEmissiveMaterial(
                "DeliveryStatusAccepted",
                new Color(0.06f, 0.42f, 0.55f),
                0.1f,
                0.42f,
                new Color(0.02f, 0.55f, 0.78f) * 2.2f);
            Material deliveryShelved = GetOrCreateEmissiveMaterial(
                "DeliveryStatusShelved",
                new Color(0.08f, 0.55f, 0.22f),
                0.1f,
                0.38f,
                new Color(0.03f, 0.78f, 0.20f) * 2.2f);

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
            AssemblyBuildResult assemblyBuild = BuildMotherboardAssembly(
                environment,
                metal,
                brushedSteel,
                accent,
                rubber,
                motherboardPcb,
                labelPaper,
                screenGlass,
                placementValid,
                placementInvalid);
            BuildStarterPickups(environment, cardboard, metal, accent, labelPaper, rubber);
            StockFlowBuildResult stockFlowBuild = BuildAuthoritativeStockFlow(
                environment,
                cardboard,
                metal,
                brushedSteel,
                accent,
                labelPaper,
                rubber,
                stockPlacement,
                deliveryArrived);
            CustomerFlowBuildResult customerFlowBuild = BuildCustomerFlow(
                gameplay,
                customerJacket,
                customerSkin,
                customerDenim,
                rubber,
                wood,
                metal,
                screenGlass);
            CustomPcWorkTicketBuildResult customPcWorkTicketBuild =
                BuildCustomPcWorkTicketStation(
                    environment,
                    metal,
                    brushedSteel,
                    accent,
                    labelPaper);
            TransportCartProjection transportCart = BuildTransportCart(
                environment,
                metal,
                brushedSteel,
                accent,
                rubber,
                labelPaper);
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
            Camera playerCamera = motor.GetComponentInChildren<Camera>(true);
            Require(input != null, "The PlayerRig prefab is missing PlayerInputAdapter.");
            Require(playerCamera != null, "The PlayerRig prefab is missing its player Camera.");
            PlayerCarryController carry = motor.GetComponent<PlayerCarryController>();
            Require(carry != null, "The PlayerRig prefab is missing PlayerCarryController.");

            GarageStockFlowRuntime stockFlow = systems.gameObject.AddComponent<GarageStockFlowRuntime>();
            stockFlow.Configure(
                stockFlowBuild.Binding,
                stockFlowBuild.StatusText,
                stockFlowBuild.ShelfOfferText,
                stockFlowBuild.StatusIndicator,
                deliveryArrived,
                deliveryAccepted,
                deliveryShelved,
                seedAssemblyPrototype: true);
            assemblyBuild.Binding.Configure(
                stockFlow,
                assemblyBuild.Motherboard,
                assemblyBuild.Seat,
                assemblyBuild.Fastener,
                GarageStockFlowSession.MotherboardItemInstanceIdValue);
            assemblyBuild.ProcessorBinding.Configure(
                stockFlow,
                assemblyBuild.Processor,
                assemblyBuild.ProcessorSocket,
                GarageStockFlowSession.ProcessorItemInstanceIdValue);
            assemblyBuild.DimmBinding.Configure(
                stockFlow,
                assemblyBuild.MemoryModule,
                assemblyBuild.DimmSlot,
                GarageStockFlowSession.MemoryItemInstanceIdValue);
            assemblyBuild.StorageBinding.Configure(
                stockFlow,
                assemblyBuild.StorageDevice,
                assemblyBuild.StorageSlot,
                GarageStockFlowSession.StorageItemInstanceIdValue);
            assemblyBuild.ProcessorCoolerBinding.Configure(
                stockFlow,
                assemblyBuild.ProcessorCooler,
                assemblyBuild.ProcessorCoolerSlot,
                GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue);
            assemblyBuild.GraphicsCardBinding.Configure(
                stockFlow,
                assemblyBuild.GraphicsCard,
                assemblyBuild.GraphicsCardSlot,
                GarageStockFlowSession.GraphicsCardAssemblyItemInstanceIdValue);
            assemblyBuild.PowerSupplyBinding.Configure(
                stockFlow,
                assemblyBuild.PowerSupply,
                assemblyBuild.PowerSupplyBay,
                GarageStockFlowSession.PowerSupplyItemInstanceIdValue);
            assemblyBuild.Atx24PowerCableBinding.Configure(
                stockFlow,
                assemblyBuild.Atx24PowerCable,
                assemblyBuild.Atx24PowerCableRoute,
                assemblyBuild.Atx24PowerCableGeometry,
                GarageStockFlowSession.Atx24PowerCableItemInstanceIdValue);
            assemblyBuild.Eps12vPowerCableBinding.Configure(
                stockFlow,
                assemblyBuild.Eps12vPowerCable,
                assemblyBuild.Eps12vPowerCableRoute,
                assemblyBuild.Eps12vPowerCableGeometry,
                GarageStockFlowSession.Eps12vPowerCableItemInstanceIdValue);
            assemblyBuild.PcieGpuPowerCableBinding.Configure(
                stockFlow,
                assemblyBuild.PcieGpuPowerCable,
                assemblyBuild.PcieGpuPowerCableRoute,
                assemblyBuild.PcieGpuPowerCableGeometry,
                GarageStockFlowSession.PcieGpuPowerCableItemInstanceIdValue);
            carry.ConfigureMotherboardSeat(assemblyBuild.Seat);
            carry.ConfigureMotherboardFastener(
                assemblyBuild.Fastener,
                assemblyBuild.Binding);
            carry.ConfigureProcessorSocket(
                assemblyBuild.ProcessorSocket,
                assemblyBuild.ProcessorBinding);
            carry.ConfigureDimmSlot(
                assemblyBuild.DimmSlot,
                assemblyBuild.DimmBinding);
            carry.ConfigureM2StorageSlot(
                assemblyBuild.StorageSlot,
                assemblyBuild.StorageBinding);
            carry.ConfigureProcessorCoolerSlot(
                assemblyBuild.ProcessorCoolerSlot,
                assemblyBuild.ProcessorCoolerBinding);
            carry.ConfigureGraphicsCardSlot(
                assemblyBuild.GraphicsCardSlot,
                assemblyBuild.GraphicsCardBinding);
            carry.ConfigurePowerSupplyBay(
                assemblyBuild.PowerSupplyBay,
                assemblyBuild.PowerSupplyBinding);
            carry.ConfigureAtx24PowerCableRoute(
                assemblyBuild.Atx24PowerCableRoute,
                assemblyBuild.Atx24PowerCableBinding);
            carry.ConfigureEps12vPowerCableRoute(
                assemblyBuild.Eps12vPowerCableRoute,
                assemblyBuild.Eps12vPowerCableBinding);
            carry.ConfigurePcieGpuPowerCableRoute(
                assemblyBuild.PcieGpuPowerCableRoute,
                assemblyBuild.PcieGpuPowerCableBinding);
            GarageCustomerFlowRuntime customerFlow =
                systems.gameObject.AddComponent<GarageCustomerFlowRuntime>();
            customerFlow.Configure(
                stockFlow,
                motor,
                input,
                playerCamera,
                customerFlowBuild.NavigationSurface,
                customerFlowBuild.CustomerAgent,
                customerFlowBuild.CustomerVisualRoot,
                customerFlowBuild.CustomerStatusText,
                customerFlowBuild.CustomerSpeechText,
                customerFlowBuild.EntranceWaypoint,
                customerFlowBuild.BrowseWaypoint,
                customerFlowBuild.CheckoutWaypoint,
                customerFlowBuild.ExitWaypoint);
            customerFlowBuild.CheckoutStation.Configure(
                stockFlow,
                customerFlow,
                input,
                motor,
                playerCamera,
                customerFlowBuild.CheckoutInteractionCollider,
                customerFlowBuild.CheckoutStatusText);
            customPcWorkTicketBuild.Projection.Configure(
                stockFlow,
                input,
                motor,
                playerCamera,
                carry,
                customPcWorkTicketBuild.InteractionCollider,
                customPcWorkTicketBuild.StatusText);
            GaragePrototypeMarker marker = systems.gameObject.AddComponent<GaragePrototypeMarker>();
            marker.Configure(
                motor,
                input,
                carry,
                transportCart,
                stockFlow,
                customerFlow,
                customerFlowBuild.CheckoutStation,
                customPcWorkTicketBuild.Projection,
                assemblyBuild.Seat,
                assemblyBuild.Fastener,
                assemblyBuild.Binding,
                assemblyBuild.ProcessorSocket,
                assemblyBuild.ProcessorBinding,
                assemblyBuild.Processor,
                assemblyBuild.DimmSlot,
                assemblyBuild.DimmBinding,
                assemblyBuild.MemoryModule,
                assemblyBuild.StorageSlot,
                assemblyBuild.StorageBinding,
                assemblyBuild.StorageDevice,
                assemblyBuild.ProcessorCoolerSlot,
                assemblyBuild.ProcessorCoolerBinding,
                assemblyBuild.ProcessorCooler,
                assemblyBuild.ProcessorCoolerGeometry,
                assemblyBuild.GraphicsCardSlot,
                assemblyBuild.GraphicsCardBinding,
                assemblyBuild.GraphicsCard,
                assemblyBuild.PowerSupplyBay,
                assemblyBuild.PowerSupplyBinding,
                assemblyBuild.PowerSupply,
                assemblyBuild.PowerSupplyGeometry,
                assemblyBuild.Atx24PowerCableRoute,
                assemblyBuild.Atx24PowerCableBinding,
                assemblyBuild.Atx24PowerCable,
                assemblyBuild.Atx24PowerCableGeometry,
                assemblyBuild.Eps12vPowerCableRoute,
                assemblyBuild.Eps12vPowerCableBinding,
                assemblyBuild.Eps12vPowerCable,
                assemblyBuild.Eps12vPowerCableGeometry,
                assemblyBuild.PcieGpuPowerCableRoute,
                assemblyBuild.PcieGpuPowerCableBinding,
                assemblyBuild.PcieGpuPowerCable,
                assemblyBuild.PcieGpuPowerCableGeometry);
            GaragePrototypeHud hud = systems.gameObject.AddComponent<GaragePrototypeHud>();
            hud.Configure(
                motor,
                carry,
                stockFlow,
                customerFlow,
                customerFlowBuild.CheckoutStation,
                customPcWorkTicketBuild.Projection);

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

        private static AssemblyBuildResult BuildMotherboardAssembly(
            Transform environment,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material motherboardPcb,
            Material labelPaper,
            Material readyMaterial,
            Material validMaterial,
            Material invalidMaterial)
        {
            Transform workshop = environment.Find(
                "VisualBenchmarkCorner/WorkshopCorner");
            Require(workshop != null, "WorkshopCorner is missing for motherboard assembly.");

            Transform slice = new GameObject(
                "PrototypeMotherboardAssemblySlice").transform;
            slice.SetParent(workshop, false);

            Transform chassis = new GameObject("PrototypeOpenChassis").transform;
            chassis.SetParent(slice, false);

            int interactableLayer = RequireLayer(InteractableLayerName);
            GameObject chassisBase = CreateBeveledCube(
                "ChassisBase",
                chassis,
                new Vector3(-0.75f, 1.015f, 4.25f),
                new Vector3(0.55f, 0.05f, 0.42f),
                0.012f,
                metal);
            GameObject chassisBack = CreateBeveledCube(
                "ChassisBack",
                chassis,
                new Vector3(-0.75f, 1.31f, 4.435f),
                new Vector3(0.55f, 0.62f, 0.05f),
                0.012f,
                metal);
            GameObject chassisLeft = CreateBeveledCube(
                "ChassisLeftRail",
                chassis,
                new Vector3(-1f, 1.31f, 4.25f),
                new Vector3(0.05f, 0.62f, 0.32f),
                0.012f,
                metal);
            GameObject chassisRight = CreateBeveledCube(
                "ChassisRightRail",
                chassis,
                new Vector3(-0.50f, 1.31f, 4.25f),
                new Vector3(0.05f, 0.62f, 0.32f),
                0.012f,
                metal);
            GameObject chassisTop = CreateBeveledCube(
                "ChassisTopRail",
                chassis,
                new Vector3(-0.75f, 1.595f, 4.25f),
                new Vector3(0.45f, 0.05f, 0.32f),
                0.012f,
                metal,
                false);
            GameObject tray = CreateBeveledCube(
                "MotherboardTray",
                chassis,
                new Vector3(-0.75f, 1.305f, 4.387f),
                new Vector3(0.454f, 0.534f, 0.050f),
                0.006f,
                metal);

            SetLayerRecursively(chassisBase, interactableLayer);
            SetLayerRecursively(chassisBack, interactableLayer);
            SetLayerRecursively(chassisLeft, interactableLayer);
            SetLayerRecursively(chassisRight, interactableLayer);
            SetLayerRecursively(chassisTop, interactableLayer);
            SetLayerRecursively(tray, interactableLayer);

            CreateCombinedBoxDetails(
                "StandoffMarkArray",
                chassis,
                new[]
                {
                    new Vector3(-0.84f, 1.21f, 4.359f),
                    new Vector3(-0.66f, 1.21f, 4.359f),
                    new Vector3(-0.84f, 1.30f, 4.359f),
                    new Vector3(-0.66f, 1.30f, 4.359f),
                    new Vector3(-0.84f, 1.39f, 4.359f),
                    new Vector3(-0.66f, 1.39f, 4.359f)
                },
                new[]
                {
                    new Vector3(0.012f, 0.012f, 0.006f),
                    new Vector3(0.012f, 0.012f, 0.006f),
                    new Vector3(0.012f, 0.012f, 0.006f),
                    new Vector3(0.012f, 0.012f, 0.006f),
                    new Vector3(0.012f, 0.012f, 0.006f),
                    new Vector3(0.012f, 0.012f, 0.006f)
                },
                brushedSteel);

            Transform seatRoot = new GameObject("MotherboardSeat").transform;
            seatRoot.SetParent(chassis, false);
            Transform snapAnchor = new GameObject("MotherboardSnapAnchor").transform;
            snapAnchor.SetParent(seatRoot, false);
            snapAnchor.localPosition = new Vector3(-0.75f, 1.30f, 4.350f);
            snapAnchor.localRotation = Quaternion.Euler(0f, 180f, 0f);

            GameObject statusPlate = CreateBeveledCube(
                "MotherboardSeatStatusPlate",
                seatRoot,
                new Vector3(-0.75f, 1.105f, 4.353f),
                new Vector3(0.24f, 0.035f, 0.018f),
                0.005f,
                metal);
            SetLayerRecursively(statusPlate, interactableLayer);
            Renderer statusRenderer = statusPlate.GetComponent<Renderer>();
            DisableDecorativeRendererCost(statusRenderer);

            MotherboardSeatProjection seat = seatRoot.gameObject.AddComponent<
                MotherboardSeatProjection>();
            seat.Configure(
                snapAnchor,
                statusPlate.GetComponent<Collider>(),
                tray.GetComponent<Collider>(),
                chassis,
                statusRenderer,
                readyMaterial,
                validMaterial,
                invalidMaterial,
                2f,
                0.94f);

            Transform fastenerRoot = new GameObject("MotherboardFastenerStation").transform;
            fastenerRoot.SetParent(chassis, false);
            GameObject screwHead = CreateCylinder(
                "MotherboardCaptiveFastener",
                fastenerRoot,
                new Vector3(-0.66f, 1.21f, 4.335f),
                new Vector3(0.012f, 0.004f, 0.012f),
                Quaternion.Euler(90f, 0f, 0f),
                brushedSteel);
            SetLayerRecursively(screwHead, interactableLayer);
            Renderer fastenerRenderer = screwHead.GetComponent<Renderer>();
            DisableDecorativeRendererCost(fastenerRenderer);
            UnityEngine.Object.DestroyImmediate(screwHead.GetComponent<Collider>());
            GameObject focusTarget = new GameObject("MotherboardFastenerFocusTarget");
            focusTarget.transform.SetParent(fastenerRoot, false);
            focusTarget.transform.localPosition = new Vector3(-0.66f, 1.21f, 4.336f);
            focusTarget.layer = interactableLayer;
            BoxCollider focusCollider = focusTarget.AddComponent<BoxCollider>();
            focusCollider.size = new Vector3(0.060f, 0.060f, 0.016f);
            focusCollider.isTrigger = true;
            GameObject recessHorizontal = CreateDetailCube(
                "FastenerCrossRecessHorizontal",
                fastenerRoot,
                new Vector3(-0.66f, 1.21f, 4.3305f),
                new Vector3(0.010f, 0.002f, 0.001f),
                rubber);
            GameObject recessVertical = CreateDetailCube(
                "FastenerCrossRecessVertical",
                fastenerRoot,
                new Vector3(-0.66f, 1.21f, 4.3305f),
                new Vector3(0.002f, 0.010f, 0.001f),
                rubber);
            DisableDecorativeRendererCost(recessHorizontal.GetComponent<Renderer>());
            DisableDecorativeRendererCost(recessVertical.GetComponent<Renderer>());

            Transform screwdriver = new GameObject("CaptiveFastenerScrewdriver").transform;
            screwdriver.SetParent(fastenerRoot, false);
            screwdriver.localPosition = new Vector3(-0.37f, 1.12f, 4.305f);
            screwdriver.localRotation = Quaternion.Euler(0f, 0f, -55f);
            GameObject screwdriverHandle = CreateCylinder(
                "ScrewdriverHandle",
                screwdriver,
                new Vector3(0f, -0.065f, 0f),
                new Vector3(0.024f, 0.055f, 0.024f),
                Quaternion.identity,
                accent);
            GameObject screwdriverShaft = CreateCylinder(
                "ScrewdriverShaft",
                screwdriver,
                new Vector3(0f, 0.065f, 0f),
                new Vector3(0.004f, 0.075f, 0.004f),
                Quaternion.identity,
                brushedSteel);
            UnityEngine.Object.DestroyImmediate(screwdriverHandle.GetComponent<Collider>());
            UnityEngine.Object.DestroyImmediate(screwdriverShaft.GetComponent<Collider>());
            DisableDecorativeRendererCost(screwdriverHandle.GetComponent<Renderer>());
            DisableDecorativeRendererCost(screwdriverShaft.GetComponent<Renderer>());

            TextMesh fastenerStatusText = new GameObject("MotherboardFastenerStatusText")
                .AddComponent<TextMesh>();
            fastenerStatusText.transform.SetParent(statusPlate.transform, false);
            fastenerStatusText.transform.localPosition = new Vector3(0f, 0f, -0.010f);
            fastenerStatusText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            fastenerStatusText.anchor = TextAnchor.MiddleCenter;
            fastenerStatusText.alignment = TextAlignment.Center;
            fastenerStatusText.characterSize = 0.011f;
            fastenerStatusText.fontSize = 36;
            fastenerStatusText.color = new Color(0.88f, 0.92f, 0.90f);
            DisableDecorativeRendererCost(fastenerStatusText.GetComponent<Renderer>());

            MotherboardFastenerProjection fastener = fastenerRoot.gameObject.AddComponent<
                MotherboardFastenerProjection>();
            fastener.Configure(
                GarageStockFlowSession.MotherboardFastenerIdValue,
                focusCollider,
                fastenerRenderer,
                screwHead.transform,
                screwdriver,
                fastenerStatusText,
                brushedSteel,
                validMaterial,
                invalidMaterial,
                brushedSteel,
                2f,
                0.975f);

            GameObject motherboardRoot = new GameObject("PrototypeMotherboard");
            motherboardRoot.transform.SetParent(slice, false);
            motherboardRoot.transform.localPosition = new Vector3(-1.35f, 0.996f, 4.20f);
            motherboardRoot.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            motherboardRoot.layer = interactableLayer;

            Rigidbody body = motherboardRoot.AddComponent<Rigidbody>();
            body.mass = 0.9f;
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            GameObject pcb = CreateBeveledCube(
                "MotherboardPcb",
                motherboardRoot.transform,
                Vector3.zero,
                new Vector3(0.244f, 0.244f, 0.012f),
                0.004f,
                motherboardPcb);
            GameObject ioKey = CreateBeveledCube(
                "MotherboardIoKey",
                motherboardRoot.transform,
                new Vector3(-0.085f, 0.070f, 0.022f),
                new Vector3(0.07f, 0.10f, 0.035f),
                0.004f,
                brushedSteel);
            Transform processorSocketRoot = new GameObject(
                "MotherboardCpuSocket").transform;
            processorSocketRoot.SetParent(motherboardRoot.transform, false);
            processorSocketRoot.localPosition = new Vector3(0.015f, 0.025f, 0.012f);

            GameObject processorSocketBase = CreateProcessorSocketBase(
                "ProcessorSocketBase",
                processorSocketRoot,
                rubber);
            DisableDecorativeRendererCost(processorSocketBase.GetComponent<Renderer>());

            Transform processorSnapAnchor = new GameObject(
                "ProcessorSnapAnchor").transform;
            processorSnapAnchor.SetParent(processorSocketRoot, false);
            processorSnapAnchor.localPosition = new Vector3(0f, 0f, 0.0035f);

            Transform loadPlatePivot = new GameObject("ProcessorLoadPlatePivot").transform;
            loadPlatePivot.SetParent(processorSocketRoot, false);
            loadPlatePivot.localPosition = new Vector3(0f, 0.026f, 0.007f);
            loadPlatePivot.localRotation = Quaternion.Euler(-68f, 0f, 0f);
            GameObject loadPlate = CreateHardSurfaceBoxDetails(
                "ProcessorLoadPlate",
                loadPlatePivot,
                new[]
                {
                    new Vector3(-0.02325f, -0.026f, 0f),
                    new Vector3(0.02325f, -0.026f, 0f),
                    new Vector3(0f, -0.00575f, 0f),
                    new Vector3(0f, -0.04625f, 0f)
                },
                new[]
                {
                    new Vector3(0.0115f, 0.050f, 0.0015f),
                    new Vector3(0.0115f, 0.050f, 0.0015f),
                    new Vector3(0.035f, 0.0095f, 0.0015f),
                    new Vector3(0.035f, 0.0095f, 0.0015f)
                },
                brushedSteel);
            DisableDecorativeRendererCost(loadPlate.GetComponent<Renderer>());

            Transform retentionLeverPivot = new GameObject(
                "ProcessorRetentionLeverPivot").transform;
            retentionLeverPivot.SetParent(processorSocketRoot, false);
            retentionLeverPivot.localPosition = new Vector3(0.03025f, 0.026f, 0.007f);
            retentionLeverPivot.localRotation = Quaternion.Euler(-55f, 0f, 0f);
            GameObject retentionLever = CreateCylinder(
                "ProcessorRetentionLever",
                retentionLeverPivot,
                new Vector3(0f, -0.026f, 0f),
                new Vector3(0.0025f, 0.026f, 0.0025f),
                Quaternion.identity,
                brushedSteel);
            UnityEngine.Object.DestroyImmediate(retentionLever.GetComponent<Collider>());
            DisableDecorativeRendererCost(retentionLever.GetComponent<Renderer>());

            GameObject processorFocusTarget = new GameObject(
                "ProcessorSocketFocusTarget");
            processorFocusTarget.transform.SetParent(processorSocketRoot, false);
            processorFocusTarget.transform.localPosition = new Vector3(0f, 0f, 0.010f);
            processorFocusTarget.layer = interactableLayer;
            BoxCollider processorFocusCollider =
                processorFocusTarget.AddComponent<BoxCollider>();
            processorFocusCollider.size = new Vector3(0.092f, 0.084f, 0.022f);
            processorFocusCollider.isTrigger = false;

            ProcessorSocketProjection processorSocket =
                processorSocketRoot.gameObject.AddComponent<ProcessorSocketProjection>();
            processorSocket.Configure(
                GarageStockFlowSession.ProcessorSlotIdValue,
                GarageStockFlowSession.ProcessorRetentionIdValue,
                processorSnapAnchor,
                processorFocusCollider,
                motherboardRoot.transform,
                loadPlatePivot,
                retentionLeverPivot,
                null,
                validMaterial,
                invalidMaterial,
                2f,
                0.94f);

            Transform processorCoolerSlotRoot = new GameObject(
                "ProcessorCoolerMountingBracket").transform;
            processorCoolerSlotRoot.SetParent(processorSocketRoot, false);
            processorCoolerSlotRoot.localPosition = Vector3.zero;

            Transform processorCoolerSnapAnchor = new GameObject(
                "ProcessorCoolerSnapAnchor").transform;
            processorCoolerSnapAnchor.SetParent(processorCoolerSlotRoot, false);
            processorCoolerSnapAnchor.localPosition = new Vector3(0f, 0f, 0.011f);

            Transform processorCoolerBracketPivot = new GameObject(
                "ProcessorCoolerBracketPivot").transform;
            processorCoolerBracketPivot.SetParent(processorCoolerSlotRoot, false);
            processorCoolerBracketPivot.localPosition = new Vector3(0f, 0f, 0.010f);
            GameObject bracketHorizontal = CreateDetailCube(
                "ProcessorCoolerBracketHorizontal",
                processorCoolerBracketPivot,
                Vector3.zero,
                new Vector3(0.112f, 0.012f, 0.005f),
                brushedSteel);
            GameObject bracketVertical = CreateDetailCube(
                "ProcessorCoolerBracketVertical",
                processorCoolerBracketPivot,
                Vector3.zero,
                new Vector3(0.012f, 0.112f, 0.005f),
                brushedSteel);
            UnityEngine.Object.DestroyImmediate(bracketHorizontal.GetComponent<Collider>());
            UnityEngine.Object.DestroyImmediate(bracketVertical.GetComponent<Collider>());
            DisableDecorativeRendererCost(bracketHorizontal.GetComponent<Renderer>());
            DisableDecorativeRendererCost(bracketVertical.GetComponent<Renderer>());

            Vector3[] coolerPointPositions =
            {
                new Vector3(-0.047f, 0.047f, 0.014f),
                new Vector3(0.047f, 0.047f, 0.014f),
                new Vector3(0.047f, -0.047f, 0.014f),
                new Vector3(-0.047f, -0.047f, 0.014f)
            };
            Transform[] coolerRetentionPoints = new Transform[4];
            for (int pointIndex = 0; pointIndex < coolerRetentionPoints.Length;
                 pointIndex++)
            {
                Transform pointPivot = new GameObject(
                    $"ProcessorCoolerRetentionPoint_{pointIndex + 1}").transform;
                pointPivot.SetParent(processorCoolerSlotRoot, false);
                pointPivot.localPosition = coolerPointPositions[pointIndex];
                GameObject pointHead = CreateCylinder(
                    $"ProcessorCoolerRetentionHead_{pointIndex + 1}",
                    pointPivot,
                    Vector3.zero,
                    new Vector3(0.006f, 0.003f, 0.006f),
                    Quaternion.Euler(90f, 0f, 0f),
                    brushedSteel);
                UnityEngine.Object.DestroyImmediate(pointHead.GetComponent<Collider>());
                DisableDecorativeRendererCost(pointHead.GetComponent<Renderer>());
                coolerRetentionPoints[pointIndex] = pointPivot;
            }

            GameObject processorCoolerFocusTarget = new GameObject(
                "ProcessorCoolerSlotFocusTarget");
            processorCoolerFocusTarget.transform.SetParent(
                processorCoolerSlotRoot,
                false);
            processorCoolerFocusTarget.transform.localPosition =
                new Vector3(0f, 0f, 0.055f);
            processorCoolerFocusTarget.layer = interactableLayer;
            BoxCollider processorCoolerFocusCollider =
                processorCoolerFocusTarget.AddComponent<BoxCollider>();
            processorCoolerFocusCollider.size = new Vector3(0.145f, 0.145f, 0.10f);
            processorCoolerFocusCollider.isTrigger = false;

            ProcessorCoolerSlotProjection processorCoolerSlot =
                processorCoolerSlotRoot.gameObject.AddComponent<
                    ProcessorCoolerSlotProjection>();
            processorCoolerSlot.Configure(
                GarageStockFlowSession.ProcessorCoolerSlotIdValue,
                GarageStockFlowSession.ProcessorCoolerBracketIdValue,
                new[]
                {
                    GarageStockFlowSession.ProcessorCoolerRetentionPoint1IdValue,
                    GarageStockFlowSession.ProcessorCoolerRetentionPoint2IdValue,
                    GarageStockFlowSession.ProcessorCoolerRetentionPoint3IdValue,
                    GarageStockFlowSession.ProcessorCoolerRetentionPoint4IdValue
                },
                processorCoolerSnapAnchor,
                processorCoolerFocusCollider,
                motherboardRoot.transform,
                processorCoolerBracketPivot,
                coolerRetentionPoints,
                2f,
                0.94f);

            Transform dimmSlotRoot = new GameObject("MotherboardDimmSlotA2").transform;
            dimmSlotRoot.SetParent(motherboardRoot.transform, false);
            dimmSlotRoot.localPosition = new Vector3(0.105f, 0.045f, 0.012f);
            GameObject dimmSlotBase = CreateHardSurfaceBoxDetails(
                "DimmSlotBase",
                dimmSlotRoot,
                new[]
                {
                    new Vector3(0f, 0f, 0f),
                    new Vector3(-0.008f, 0f, 0.004f),
                    new Vector3(0.008f, 0f, 0.004f),
                    new Vector3(0f, -0.009f, 0.006f)
                },
                new[]
                {
                    new Vector3(0.012f, 0.122f, 0.008f),
                    new Vector3(0.004f, 0.116f, 0.012f),
                    new Vector3(0.004f, 0.116f, 0.012f),
                    new Vector3(0.012f, 0.004f, 0.012f)
                },
                rubber);
            DisableDecorativeRendererCost(dimmSlotBase.GetComponent<Renderer>());

            Transform dimmSnapAnchor = new GameObject("MemoryModuleSnapAnchor").transform;
            dimmSnapAnchor.SetParent(dimmSlotRoot, false);
            dimmSnapAnchor.localPosition = new Vector3(0f, 0f, 0.024f);
            dimmSnapAnchor.localRotation = Quaternion.LookRotation(
                Vector3.right,
                Vector3.forward);

            Transform leftLatchPivot = new GameObject("DimmLeftLatchPivot").transform;
            leftLatchPivot.SetParent(dimmSlotRoot, false);
            leftLatchPivot.localPosition = new Vector3(0f, -0.064f, 0.006f);
            leftLatchPivot.localRotation = Quaternion.Euler(-28f, 0f, 0f);
            GameObject leftLatch = CreateBeveledCube(
                "DimmLeftLatch",
                leftLatchPivot,
                new Vector3(0f, -0.006f, 0.004f),
                new Vector3(0.026f, 0.014f, 0.012f),
                0.002f,
                accent,
                false);
            DisableDecorativeRendererCost(leftLatch.GetComponent<Renderer>());

            Transform rightLatchPivot = new GameObject("DimmRightLatchPivot").transform;
            rightLatchPivot.SetParent(dimmSlotRoot, false);
            rightLatchPivot.localPosition = new Vector3(0f, 0.064f, 0.006f);
            rightLatchPivot.localRotation = Quaternion.Euler(28f, 0f, 0f);
            GameObject rightLatch = CreateBeveledCube(
                "DimmRightLatch",
                rightLatchPivot,
                new Vector3(0f, 0.006f, 0.004f),
                new Vector3(0.026f, 0.014f, 0.012f),
                0.002f,
                accent,
                false);
            DisableDecorativeRendererCost(rightLatch.GetComponent<Renderer>());

            GameObject dimmFocusTarget = new GameObject("DimmSlotFocusTarget");
            dimmFocusTarget.transform.SetParent(dimmSlotRoot, false);
            dimmFocusTarget.transform.localPosition = new Vector3(0f, 0f, 0.042f);
            dimmFocusTarget.layer = interactableLayer;
            BoxCollider dimmFocusCollider = dimmFocusTarget.AddComponent<BoxCollider>();
            dimmFocusCollider.size = new Vector3(0.052f, 0.150f, 0.080f);
            dimmFocusCollider.isTrigger = false;

            DimmSlotProjection dimmSlot =
                dimmSlotRoot.gameObject.AddComponent<DimmSlotProjection>();
            dimmSlot.Configure(
                GarageStockFlowSession.MemorySlotIdValue,
                GarageStockFlowSession.MemoryRetentionIdValue,
                GarageStockFlowSession.MemoryChannelIdValue,
                GarageStockFlowSession.MemoryBankIdValue,
                dimmSnapAnchor,
                dimmFocusCollider,
                motherboardRoot.transform,
                leftLatchPivot,
                rightLatchPivot,
                2f,
                0.94f);

            Transform storageSlotRoot = new GameObject(
                "MotherboardM2SlotPrimary").transform;
            storageSlotRoot.SetParent(motherboardRoot.transform, false);
            storageSlotRoot.localPosition = new Vector3(0.020f, 0.085f, 0.012f);
            GameObject storageConnector = CreateBeveledCube(
                "M2MKeyConnector",
                storageSlotRoot,
                new Vector3(-0.046f, 0f, 0.006f),
                new Vector3(0.014f, 0.030f, 0.012f),
                0.002f,
                rubber,
                false);
            UnityEngine.Object.DestroyImmediate(storageConnector.GetComponent<Collider>());
            DisableDecorativeRendererCost(storageConnector.GetComponent<Renderer>());

            Transform storageSeatAnchor = new GameObject(
                "M2StorageSeatedAnchor").transform;
            storageSeatAnchor.SetParent(storageSlotRoot, false);
            storageSeatAnchor.localPosition = new Vector3(0f, 0f, 0.012f);

            GameObject storageStandoff = CreateCylinder(
                "M2Storage2280Standoff",
                storageSlotRoot,
                new Vector3(0.040f, 0f, 0.007f),
                new Vector3(0.006f, 0.003f, 0.006f),
                Quaternion.Euler(90f, 0f, 0f),
                brushedSteel);
            UnityEngine.Object.DestroyImmediate(storageStandoff.GetComponent<Collider>());
            DisableDecorativeRendererCost(storageStandoff.GetComponent<Renderer>());

            Transform captiveScrewPivot = new GameObject(
                "M2CaptiveScrewPivot").transform;
            captiveScrewPivot.SetParent(storageSlotRoot, false);
            captiveScrewPivot.localPosition = new Vector3(0.040f, 0f, 0.014f);
            GameObject captiveScrew = CreateCylinder(
                "M2CaptiveScrew",
                captiveScrewPivot,
                Vector3.zero,
                new Vector3(0.005f, 0.002f, 0.005f),
                Quaternion.Euler(90f, 0f, 0f),
                brushedSteel);
            UnityEngine.Object.DestroyImmediate(captiveScrew.GetComponent<Collider>());
            DisableDecorativeRendererCost(captiveScrew.GetComponent<Renderer>());
            GameObject screwSlot = CreateDetailCube(
                "M2CaptiveScrewSlot",
                captiveScrewPivot,
                new Vector3(0f, 0f, -0.0022f),
                new Vector3(0.006f, 0.0014f, 0.001f),
                rubber);
            UnityEngine.Object.DestroyImmediate(screwSlot.GetComponent<Collider>());
            DisableDecorativeRendererCost(screwSlot.GetComponent<Renderer>());

            GameObject storageFocusTarget = new GameObject("M2StorageSlotFocusTarget");
            storageFocusTarget.transform.SetParent(storageSlotRoot, false);
            storageFocusTarget.transform.localPosition = new Vector3(0f, 0f, 0.028f);
            storageFocusTarget.layer = interactableLayer;
            BoxCollider storageFocusCollider =
                storageFocusTarget.AddComponent<BoxCollider>();
            storageFocusCollider.size = new Vector3(0.115f, 0.060f, 0.070f);
            storageFocusCollider.isTrigger = false;

            M2StorageSlotProjection storageSlot =
                storageSlotRoot.gameObject.AddComponent<M2StorageSlotProjection>();
            storageSlot.Configure(
                GarageStockFlowSession.StorageSlotIdValue,
                GarageStockFlowSession.StorageStandoffIdValue,
                GarageStockFlowSession.StorageCaptiveScrewIdValue,
                storageSeatAnchor,
                storageFocusCollider,
                motherboardRoot.transform,
                captiveScrewPivot,
                2f,
                0.94f);
            CreateCombinedBoxDetails(
                "MotherboardConnectorMarks",
                motherboardRoot.transform,
                new[]
                {
                    new Vector3(0.085f, 0.045f, 0.012f),
                    new Vector3(0.02f, -0.07f, 0.012f)
                },
                new[]
                {
                    new Vector3(0.012f, 0.12f, 0.012f),
                    new Vector3(0.16f, 0.014f, 0.012f)
                },
                rubber);
            SetLayerRecursively(pcb, interactableLayer);
            SetLayerRecursively(ioKey, interactableLayer);
            SetLayerRecursively(motherboardRoot, interactableLayer);

            PhysicalItemProjection motherboard = motherboardRoot.AddComponent<
                PhysicalItemProjection>();
            motherboard.Configure(
                GarageStockFlowSession.MotherboardItemInstanceIdValue,
                GarageStockFlowSession.MotherboardDisplayName,
                body,
                new Vector3(0.127f, 0.127f, 0.045f),
                new Vector3(0f, -0.05f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent);
            MotherboardAssemblyItemBinding binding = motherboardRoot.AddComponent<
                MotherboardAssemblyItemBinding>();

            GameObject processorRoot = new GameObject("PrototypeProcessor");
            processorRoot.transform.SetParent(slice, false);
            processorRoot.transform.localPosition = new Vector3(-1.17f, 0.992f, 3.93f);
            processorRoot.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            processorRoot.layer = interactableLayer;

            Rigidbody processorBody = processorRoot.AddComponent<Rigidbody>();
            processorBody.mass = 0.08f;
            processorBody.useGravity = false;
            processorBody.isKinematic = true;
            processorBody.interpolation = RigidbodyInterpolation.Interpolate;
            processorBody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            GameObject processorPackage = CreateProcessorPackage(
                "PrototypeProcessorPackage",
                processorRoot.transform,
                motherboardPcb,
                brushedSteel);
            DisableDecorativeRendererCost(processorPackage.GetComponent<Renderer>());
            BoxCollider processorCollider = processorRoot.AddComponent<BoxCollider>();
            processorCollider.center = Vector3.zero;
            processorCollider.size = new Vector3(0.045f, 0.0375f, 0.004f);
            SetLayerRecursively(processorRoot, interactableLayer);

            PhysicalItemProjection processor = processorRoot.AddComponent<
                PhysicalItemProjection>();
            processor.Configure(
                GarageStockFlowSession.ProcessorItemInstanceIdValue,
                GarageStockFlowSession.ProcessorDisplayName,
                processorBody,
                new Vector3(0.0225f, 0.01875f, 0.010f),
                new Vector3(0f, -0.04f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent);
            ProcessorAssemblyItemBinding processorBinding =
                processorRoot.AddComponent<ProcessorAssemblyItemBinding>();

            GameObject memoryRoot = new GameObject("PrototypeMemoryModule");
            memoryRoot.transform.SetParent(slice, false);
            memoryRoot.transform.localPosition = new Vector3(-1.05f, 0.992f, 3.93f);
            memoryRoot.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
            memoryRoot.layer = interactableLayer;

            Rigidbody memoryBody = memoryRoot.AddComponent<Rigidbody>();
            memoryBody.mass = 0.045f;
            memoryBody.useGravity = false;
            memoryBody.isKinematic = true;
            memoryBody.interpolation = RigidbodyInterpolation.Interpolate;
            memoryBody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            GameObject memoryPackage = CreateMemoryModulePackage(
                "PrototypeMemoryModulePackage",
                memoryRoot.transform,
                motherboardPcb,
                rubber,
                brushedSteel,
                accent);
            DisableDecorativeRendererCost(memoryPackage.GetComponent<Renderer>());
            BoxCollider memoryCollider = memoryRoot.AddComponent<BoxCollider>();
            memoryCollider.center = new Vector3(0f, 0.004f, 0f);
            memoryCollider.size = new Vector3(0.136f, 0.034f, 0.010f);
            processorCoolerSlot.ConfigureClearanceBlockers(
                new Collider[] { memoryCollider });
            SetLayerRecursively(memoryRoot, interactableLayer);

            PhysicalItemProjection memoryModule = memoryRoot.AddComponent<
                PhysicalItemProjection>();
            memoryModule.Configure(
                GarageStockFlowSession.MemoryItemInstanceIdValue,
                GarageStockFlowSession.MemoryDisplayName,
                memoryBody,
                new Vector3(0.068f, 0.018f, 0.010f),
                new Vector3(0f, -0.055f, 0f),
                new Vector3(0f, 180f, 90f),
                PhysicalCarryProfile.PcComponent);
            DimmAssemblyItemBinding dimmBinding =
                memoryRoot.AddComponent<DimmAssemblyItemBinding>();

            GameObject storageRoot = new GameObject("PrototypeM2Nvme2280");
            storageRoot.transform.SetParent(slice, false);
            storageRoot.transform.localPosition = new Vector3(-0.91f, 0.992f, 3.93f);
            storageRoot.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            storageRoot.layer = interactableLayer;

            Rigidbody storageBody = storageRoot.AddComponent<Rigidbody>();
            storageBody.mass = 0.010f;
            storageBody.useGravity = false;
            storageBody.isKinematic = true;
            storageBody.interpolation = RigidbodyInterpolation.Interpolate;
            storageBody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            GameObject storagePcb = CreateBeveledCube(
                "M2NvmePcb",
                storageRoot.transform,
                Vector3.zero,
                new Vector3(0.080f, 0.022f, 0.003f),
                0.001f,
                motherboardPcb);
            UnityEngine.Object.DestroyImmediate(storagePcb.GetComponent<Collider>());
            DisableDecorativeRendererCost(storagePcb.GetComponent<Renderer>());
            GameObject storageController = CreateDetailCube(
                "M2NvmeController",
                storageRoot.transform,
                new Vector3(-0.012f, 0f, 0.003f),
                new Vector3(0.013f, 0.013f, 0.003f),
                rubber);
            GameObject storageNandA = CreateDetailCube(
                "M2NvmeNandA",
                storageRoot.transform,
                new Vector3(0.008f, 0f, 0.003f),
                new Vector3(0.014f, 0.014f, 0.003f),
                rubber);
            GameObject storageNandB = CreateDetailCube(
                "M2NvmeNandB",
                storageRoot.transform,
                new Vector3(0.027f, 0f, 0.003f),
                new Vector3(0.014f, 0.014f, 0.003f),
                rubber);
            GameObject storageLabel = CreateDetailCube(
                "M2NvmeLabel",
                storageRoot.transform,
                new Vector3(0.012f, 0f, 0.005f),
                new Vector3(0.048f, 0.018f, 0.001f),
                labelPaper);
            DisableDecorativeRendererCost(storageController.GetComponent<Renderer>());
            DisableDecorativeRendererCost(storageNandA.GetComponent<Renderer>());
            DisableDecorativeRendererCost(storageNandB.GetComponent<Renderer>());
            DisableDecorativeRendererCost(storageLabel.GetComponent<Renderer>());

            for (int contact = 0; contact < 6; contact++)
            {
                float y = -0.009f + (contact * 0.0036f);
                if (contact == 4)
                {
                    continue;
                }

                GameObject finger = CreateDetailCube(
                    $"M2MKeyContact_{contact + 1}",
                    storageRoot.transform,
                    new Vector3(-0.0385f, y, 0.0025f),
                    new Vector3(0.008f, 0.0022f, 0.001f),
                    accent);
                DisableDecorativeRendererCost(finger.GetComponent<Renderer>());
            }

            GameObject keyNotch = CreateDetailCube(
                "M2MKeyNotch",
                storageRoot.transform,
                new Vector3(-0.0395f, 0.0053f, 0.0015f),
                new Vector3(0.004f, 0.003f, 0.0045f),
                rubber);
            DisableDecorativeRendererCost(keyNotch.GetComponent<Renderer>());
            BoxCollider storageCollider = storageRoot.AddComponent<BoxCollider>();
            storageCollider.center = Vector3.zero;
            storageCollider.size = new Vector3(0.082f, 0.024f, 0.009f);
            SetLayerRecursively(storageRoot, interactableLayer);

            PhysicalItemProjection storageDevice = storageRoot.AddComponent<
                PhysicalItemProjection>();
            storageDevice.Configure(
                GarageStockFlowSession.StorageItemInstanceIdValue,
                GarageStockFlowSession.StorageDisplayName,
                storageBody,
                new Vector3(0.041f, 0.012f, 0.006f),
                new Vector3(0f, -0.045f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent);
            M2StorageAssemblyItemBinding storageBinding =
                storageRoot.AddComponent<M2StorageAssemblyItemBinding>();

            GameObject processorCoolerRoot = new GameObject(
                "PrototypeProcessorCooler");
            processorCoolerRoot.transform.SetParent(slice, false);
            processorCoolerRoot.transform.localPosition =
                new Vector3(-0.72f, 0.992f, 3.93f);
            processorCoolerRoot.transform.localRotation =
                Quaternion.Euler(-90f, 0f, 0f);
            processorCoolerRoot.layer = interactableLayer;

            Rigidbody processorCoolerBody =
                processorCoolerRoot.AddComponent<Rigidbody>();
            processorCoolerBody.mass = 0.52f;
            processorCoolerBody.useGravity = false;
            processorCoolerBody.isKinematic = true;
            processorCoolerBody.interpolation = RigidbodyInterpolation.Interpolate;
            processorCoolerBody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            GameObject coolerColdPlate = CreateBeveledCube(
                "ProcessorCoolerColdPlate",
                processorCoolerRoot.transform,
                new Vector3(0f, 0f, 0.006f),
                new Vector3(0.052f, 0.052f, 0.010f),
                0.002f,
                brushedSteel);
            UnityEngine.Object.DestroyImmediate(coolerColdPlate.GetComponent<Collider>());
            DisableDecorativeRendererCost(coolerColdPlate.GetComponent<Renderer>());

            GameObject coolerTim = CreateDetailCube(
                "ProcessorCoolerPreAppliedTim",
                processorCoolerRoot.transform,
                new Vector3(0f, 0f, 0.0005f),
                new Vector3(0.038f, 0.038f, 0.001f),
                rubber);
            UnityEngine.Object.DestroyImmediate(coolerTim.GetComponent<Collider>());
            DisableDecorativeRendererCost(coolerTim.GetComponent<Renderer>());

            Transform coolerBracket = new GameObject(
                "ProcessorCoolerMountingFrame").transform;
            coolerBracket.SetParent(processorCoolerRoot.transform, false);
            coolerBracket.localPosition = new Vector3(0f, 0f, 0.013f);
            GameObject coolerBracketX = CreateDetailCube(
                "ProcessorCoolerFrameX",
                coolerBracket,
                Vector3.zero,
                new Vector3(0.116f, 0.012f, 0.006f),
                brushedSteel);
            GameObject coolerBracketY = CreateDetailCube(
                "ProcessorCoolerFrameY",
                coolerBracket,
                Vector3.zero,
                new Vector3(0.012f, 0.116f, 0.006f),
                brushedSteel);
            UnityEngine.Object.DestroyImmediate(coolerBracketX.GetComponent<Collider>());
            UnityEngine.Object.DestroyImmediate(coolerBracketY.GetComponent<Collider>());
            DisableDecorativeRendererCost(coolerBracketX.GetComponent<Renderer>());
            DisableDecorativeRendererCost(coolerBracketY.GetComponent<Renderer>());

            GameObject coolerFinStack = CreateBeveledCube(
                "ProcessorCoolerFinStack",
                processorCoolerRoot.transform,
                new Vector3(0f, 0f, 0.043f),
                new Vector3(0.098f, 0.098f, 0.058f),
                0.004f,
                metal);
            UnityEngine.Object.DestroyImmediate(coolerFinStack.GetComponent<Collider>());
            DisableDecorativeRendererCost(coolerFinStack.GetComponent<Renderer>());
            for (int finIndex = 0; finIndex < 7; finIndex++)
            {
                GameObject fin = CreateDetailCube(
                    $"ProcessorCoolerFin_{finIndex + 1}",
                    processorCoolerRoot.transform,
                    new Vector3(0f, 0f, 0.020f + (finIndex * 0.008f)),
                    new Vector3(0.106f, 0.106f, 0.0015f),
                    brushedSteel);
                UnityEngine.Object.DestroyImmediate(fin.GetComponent<Collider>());
                DisableDecorativeRendererCost(fin.GetComponent<Renderer>());
            }

            Transform coolerFan = new GameObject("ProcessorCoolerFan").transform;
            coolerFan.SetParent(processorCoolerRoot.transform, false);
            coolerFan.localPosition = new Vector3(0f, 0f, 0.078f);
            GameObject fanHub = CreateCylinder(
                "ProcessorCoolerFanHub",
                coolerFan,
                Vector3.zero,
                new Vector3(0.014f, 0.006f, 0.014f),
                Quaternion.Euler(90f, 0f, 0f),
                rubber);
            UnityEngine.Object.DestroyImmediate(fanHub.GetComponent<Collider>());
            DisableDecorativeRendererCost(fanHub.GetComponent<Renderer>());
            for (int bladeIndex = 0; bladeIndex < 7; bladeIndex++)
            {
                Transform bladePivot = new GameObject(
                    $"ProcessorCoolerFanBladePivot_{bladeIndex + 1}").transform;
                bladePivot.SetParent(coolerFan, false);
                bladePivot.localRotation = Quaternion.Euler(
                    0f,
                    0f,
                    bladeIndex * (360f / 7f));
                GameObject blade = CreateBeveledCube(
                    $"ProcessorCoolerFanBlade_{bladeIndex + 1}",
                    bladePivot,
                    new Vector3(0.026f, 0f, 0f),
                    new Vector3(0.042f, 0.012f, 0.004f),
                    0.002f,
                    rubber,
                    false);
                UnityEngine.Object.DestroyImmediate(blade.GetComponent<Collider>());
                DisableDecorativeRendererCost(blade.GetComponent<Renderer>());
            }

            Transform[] coolerGeometryPoints = new Transform[4];
            for (int pointIndex = 0; pointIndex < coolerGeometryPoints.Length;
                 pointIndex++)
            {
                Transform point = new GameObject(
                    $"ProcessorCoolerFastener_{pointIndex + 1}").transform;
                point.SetParent(processorCoolerRoot.transform, false);
                point.localPosition = coolerPointPositions[pointIndex];
                GameObject pointHead = CreateCylinder(
                    $"ProcessorCoolerFastenerHead_{pointIndex + 1}",
                    point,
                    Vector3.zero,
                    new Vector3(0.006f, 0.004f, 0.006f),
                    Quaternion.Euler(90f, 0f, 0f),
                    accent);
                UnityEngine.Object.DestroyImmediate(pointHead.GetComponent<Collider>());
                DisableDecorativeRendererCost(pointHead.GetComponent<Renderer>());
                coolerGeometryPoints[pointIndex] = point;
            }

            BoxCollider processorCoolerCollider =
                processorCoolerRoot.AddComponent<BoxCollider>();
            processorCoolerCollider.center = new Vector3(0f, 0f, 0.040f);
            processorCoolerCollider.size = new Vector3(0.120f, 0.120f, 0.082f);
            SetLayerRecursively(processorCoolerRoot, interactableLayer);

            PhysicalItemProjection processorCooler =
                processorCoolerRoot.AddComponent<PhysicalItemProjection>();
            processorCooler.Configure(
                GarageStockFlowSession.ProcessorCoolerItemInstanceIdValue,
                GarageStockFlowSession.ProcessorCoolerDisplayName,
                processorCoolerBody,
                new Vector3(0.060f, 0.060f, 0.043f),
                new Vector3(0f, -0.075f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent,
                new Vector3(0f, 0f, 0.040f));
            ProcessorCoolerAssemblyItemBinding processorCoolerBinding =
                processorCoolerRoot.AddComponent<
                    ProcessorCoolerAssemblyItemBinding>();
            ProcessorCoolerRuntimeGeometry processorCoolerGeometry =
                processorCoolerRoot.AddComponent<ProcessorCoolerRuntimeGeometry>();
            processorCoolerGeometry.Configure(
                coolerColdPlate.transform,
                coolerTim.transform,
                coolerFinStack.transform,
                coolerFan,
                coolerBracket,
                coolerGeometryPoints);
            ProcessorCoolerRuntimeSmokeMarker processorCoolerSmoke =
                processorCoolerRoot.AddComponent<ProcessorCoolerRuntimeSmokeMarker>();
            processorCoolerSmoke.Configure(
                processorCoolerGeometry,
                processorCoolerSlot,
                processorCoolerBinding);

            GraphicsCardBuildResult graphicsCardBuild = BuildGraphicsCardAssembly(
                slice,
                motherboardRoot.transform,
                motherboardPcb,
                metal,
                brushedSteel,
                accent,
                rubber,
                labelPaper,
                interactableLayer,
                new[]
                {
                    chassisBase.GetComponent<Collider>(),
                    chassisBack.GetComponent<Collider>(),
                    chassisLeft.GetComponent<Collider>(),
                    chassisRight.GetComponent<Collider>(),
                    tray.GetComponent<Collider>()
                },
                new Collider[] { processorCoolerCollider });
            PowerSupplyBuildResult powerSupplyBuild = BuildPowerSupplyAssembly(
                slice,
                chassis,
                metal,
                brushedSteel,
                accent,
                rubber,
                labelPaper,
                interactableLayer,
                new[]
                {
                    chassisBack.GetComponent<Collider>(),
                    chassisLeft.GetComponent<Collider>(),
                    chassisRight.GetComponent<Collider>(),
                    tray.GetComponent<Collider>()
                });
            Atx24PowerCableBuildResult atx24PowerCableBuild =
                BuildAtx24PowerCableAssembly(
                    slice,
                    chassis,
                    motherboardRoot.transform,
                    powerSupplyBuild.Item,
                    powerSupplyBuild.Geometry,
                    metal,
                    brushedSteel,
                    accent,
                    rubber,
                    labelPaper,
                    validMaterial,
                    interactableLayer);
            Eps12vPowerCableBuildResult eps12vPowerCableBuild =
                BuildEps12vPowerCableAssembly(
                    slice,
                    chassis,
                    motherboardRoot.transform,
                    motherboard,
                    powerSupplyBuild.Item,
                    powerSupplyBuild.Geometry,
                    metal,
                    accent,
                    rubber,
                    labelPaper,
                    validMaterial,
                    interactableLayer);
            PcieGpuPowerCableBuildResult pcieGpuPowerCableBuild =
                BuildPcieGpuPowerCableAssembly(
                    slice,
                    chassis,
                    graphicsCardBuild.Item.transform,
                    graphicsCardBuild.Item,
                    powerSupplyBuild.Item,
                    powerSupplyBuild.Geometry,
                    metal,
                    accent,
                    rubber,
                    labelPaper,
                    validMaterial,
                    interactableLayer);

            return new AssemblyBuildResult(
                seat,
                fastener,
                binding,
                motherboard,
                processorSocket,
                processorBinding,
                processor,
                dimmSlot,
                dimmBinding,
                memoryModule,
                storageSlot,
                storageBinding,
                storageDevice,
                processorCoolerSlot,
                processorCoolerBinding,
                processorCooler,
                processorCoolerGeometry,
                graphicsCardBuild.Slot,
                graphicsCardBuild.Binding,
                graphicsCardBuild.Item,
                powerSupplyBuild.Bay,
                powerSupplyBuild.Binding,
                powerSupplyBuild.Item,
                powerSupplyBuild.Geometry,
                atx24PowerCableBuild.Route,
                atx24PowerCableBuild.Binding,
                atx24PowerCableBuild.Item,
                atx24PowerCableBuild.Geometry,
                eps12vPowerCableBuild.Route,
                eps12vPowerCableBuild.Binding,
                eps12vPowerCableBuild.Item,
                eps12vPowerCableBuild.Geometry,
                pcieGpuPowerCableBuild.Route,
                pcieGpuPowerCableBuild.Binding,
                pcieGpuPowerCableBuild.Item,
                pcieGpuPowerCableBuild.Geometry);
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
                new Vector3(-0.55f, 1.12f, 4.28f),
                new Color(1f, 0.77f, 0.55f),
                3.8f,
                3.4f,
                74f);

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

        private static StockFlowBuildResult BuildAuthoritativeStockFlow(
            Transform parent,
            Material cardboard,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material labelPaper,
            Material rubber,
            Material shelfSurfaceMaterial,
            Material arrivedStatusMaterial)
        {
            int interactableLayer = RequireLayer(InteractableLayerName);
            Transform receiving = new GameObject("AuthoritativeReceivingBay").transform;
            receiving.SetParent(parent, false);

            CreateBeveledCube(
                "ReceivingPallet",
                receiving,
                new Vector3(2.55f, 0.58f, -3.55f),
                new Vector3(1.15f, 1.16f, 0.92f),
                0.025f,
                brushedSteel);
            CreateDetailCube(
                "ReceivingPalletMat",
                receiving,
                new Vector3(2.55f, 1.172f, -3.55f),
                new Vector3(1.02f, 0.024f, 0.79f),
                rubber);

            GameObject itemRoot = new GameObject("ArrivedNorthstarA60Delivery");
            itemRoot.transform.SetParent(receiving, false);
            itemRoot.transform.localPosition = new Vector3(2.55f, 1.43f, -3.55f);
            itemRoot.layer = interactableLayer;

            GameObject sealedParcel = new GameObject("SealedDeliveryParcelVisual");
            sealedParcel.transform.SetParent(itemRoot.transform, false);
            GameObject parcelCarton = CreateCube(
                "OuterDeliveryCarton",
                sealedParcel.transform,
                Vector3.zero,
                new Vector3(0.90f, 0.58f, 0.70f),
                cardboard);
            parcelCarton.layer = interactableLayer;
            CreateDetailCube(
                "OuterParcelTape",
                sealedParcel.transform,
                new Vector3(0f, 0.296f, 0f),
                new Vector3(0.15f, 0.012f, 0.67f),
                rubber);
            CreateDetailCube(
                "OuterParcelManifestLabel",
                sealedParcel.transform,
                new Vector3(0.19f, 0.02f, -0.356f),
                new Vector3(0.36f, 0.22f, 0.012f),
                labelPaper);
            CreateDetailCube(
                "OuterParcelSealBand",
                sealedParcel.transform,
                new Vector3(0f, -0.10f, -0.358f),
                new Vector3(0.84f, 0.08f, 0.014f),
                accent);
            SetLayerRecursively(sealedParcel, interactableLayer);

            GameObject productVisual = new GameObject("RevealedNorthstarA60Product");
            productVisual.transform.SetParent(itemRoot.transform, false);
            GameObject productCarton = CreateCube(
                "NorthstarA60RetailCarton",
                productVisual.transform,
                Vector3.zero,
                new Vector3(0.72f, 0.46f, 0.52f),
                cardboard);
            productCarton.layer = interactableLayer;
            CreateDetailCube(
                "NorthstarA60RetailBand",
                productVisual.transform,
                new Vector3(0f, -0.07f, -0.268f),
                new Vector3(0.68f, 0.07f, 0.014f),
                accent);
            CreateDetailCube(
                "NorthstarA60RetailLabel",
                productVisual.transform,
                new Vector3(0.16f, 0.01f, -0.266f),
                new Vector3(0.30f, 0.18f, 0.012f),
                labelPaper);
            SetLayerRecursively(productVisual, interactableLayer);

            GameObject openedParcelShell = new GameObject("OpenedDeliveryParcelShell");
            openedParcelShell.transform.SetParent(receiving, false);
            CreateDetailCube(
                "OpenedParcelBase",
                openedParcelShell.transform,
                new Vector3(2.55f, 1.18f, -3.55f),
                new Vector3(0.94f, 0.06f, 0.74f),
                cardboard);
            CreateDetailCube(
                "OpenedParcelFlapFront",
                openedParcelShell.transform,
                new Vector3(2.55f, 1.24f, -3.98f),
                new Vector3(0.90f, 0.05f, 0.34f),
                cardboard);
            CreateDetailCube(
                "OpenedParcelFlapBack",
                openedParcelShell.transform,
                new Vector3(2.55f, 1.24f, -3.12f),
                new Vector3(0.90f, 0.05f, 0.34f),
                cardboard);
            CreateDetailCube(
                "OpenedParcelFlapLeft",
                openedParcelShell.transform,
                new Vector3(2.03f, 1.24f, -3.55f),
                new Vector3(0.30f, 0.05f, 0.68f),
                cardboard);
            CreateDetailCube(
                "OpenedParcelFlapRight",
                openedParcelShell.transform,
                new Vector3(3.07f, 1.24f, -3.55f),
                new Vector3(0.30f, 0.05f, 0.68f),
                cardboard);
            productVisual.SetActive(false);
            openedParcelShell.SetActive(false);

            Rigidbody body = itemRoot.AddComponent<Rigidbody>();
            body.mass = 2.4f;
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            PhysicalItemProjection projection = itemRoot.AddComponent<PhysicalItemProjection>();
            projection.Configure(
                GarageStockFlowSession.ItemInstanceIdValue,
                GarageStockFlowSession.ProductDisplayName,
                body,
                new Vector3(0.36f, 0.23f, 0.26f),
                Vector3.zero,
                Vector3.zero,
                PhysicalCarryProfile.SmallBox);
            DeliveryParcelProjection parcel = itemRoot.AddComponent<DeliveryParcelProjection>();
            parcel.Configure(projection, sealedParcel, productVisual, openedParcelShell);
            InventoryItemWorldBinding binding = itemRoot.AddComponent<InventoryItemWorldBinding>();

            GameObject statusBoard = CreateBeveledCube(
                "ReceivingStatusBoard",
                receiving,
                new Vector3(2.55f, 2.20f, -4.73f),
                new Vector3(2.35f, 0.92f, 0.06f),
                0.018f,
                metal,
                false);
            TextMesh statusText = new GameObject("ReceivingStatusText").AddComponent<TextMesh>();
            statusText.transform.SetParent(statusBoard.transform, false);
            statusText.transform.localPosition = new Vector3(0f, 0f, -0.038f);
            statusText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center;
            statusText.characterSize = 0.047f;
            statusText.fontSize = 46;
            statusText.color = new Color(0.90f, 0.94f, 0.96f);
            statusText.text = "SİPARİŞ: GELDİ • KABUL BEKLİYOR\nKOLİ: KAPALI\n" +
                              "ÜRÜN: KABUL BEKLİYOR • STOK 0\nFİYAT: FİYAT YOK\n" +
                              "SEPET: BOŞ\nKASA: BEKLİYOR";

            GameObject indicator = CreateDetailCube(
                "ReceivingStatusIndicator",
                receiving,
                new Vector3(3.56f, 2.20f, -4.68f),
                new Vector3(0.16f, 0.50f, 0.08f),
                arrivedStatusMaterial);

            Transform shelf = new GameObject("AuthoritativeRetailShelfA").transform;
            shelf.SetParent(parent, false);
            CreateBeveledCube(
                "RetailShelfDeck",
                shelf,
                new Vector3(3.48f, 0.73f, 0.55f),
                new Vector3(0.82f, 0.10f, 1.62f),
                0.018f,
                brushedSteel);
            CreateBeveledCube(
                "RetailShelfBack",
                shelf,
                new Vector3(3.90f, 1.22f, 0.55f),
                new Vector3(0.07f, 1.05f, 1.68f),
                0.012f,
                metal);
            foreach (float z in new[] { -0.18f, 1.28f })
            {
                CreateBeveledCube(
                    z < 0f ? "RetailShelfLegFront" : "RetailShelfLegBack",
                    shelf,
                    new Vector3(3.72f, 0.35f, z),
                    new Vector3(0.10f, 0.70f, 0.10f),
                    0.014f,
                    metal);
            }

            GameObject shelfSurfaceObject = CreateCube(
                "AuthoritativeShelfPlacementSurface",
                shelf,
                new Vector3(3.47f, 0.805f, 0.55f),
                new Vector3(0.72f, 0.05f, 1.48f),
                shelfSurfaceMaterial);
            BoxCollider shelfCollider = shelfSurfaceObject.GetComponent<BoxCollider>();
            PlacementSurface shelfSurface = shelfSurfaceObject.AddComponent<PlacementSurface>();
            shelfSurface.Configure("prototype.retail-shelf-a", shelfCollider, 0.25f, 90f);
            InventoryPlacementZone shelfZone = shelfSurfaceObject.AddComponent<InventoryPlacementZone>();
            shelfZone.Configure(
                GarageStockFlowSession.ShelfContainerIdValue,
                InventoryContainerKind.Shelf,
                "RAF A",
                shelfSurface);

            GameObject shelfLabelBoard = CreateDetailCube(
                "RetailShelfLabelBoard",
                shelf,
                new Vector3(3.03f, 1.58f, 0.55f),
                new Vector3(0.05f, 0.32f, 1.30f),
                accent);
            TextMesh shelfLabel = new GameObject("RetailShelfLabel").AddComponent<TextMesh>();
            shelfLabel.transform.SetParent(shelfLabelBoard.transform, false);
            shelfLabel.transform.localPosition = new Vector3(-0.56f, 0f, 0f);
            shelfLabel.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
            shelfLabel.anchor = TextAnchor.MiddleCenter;
            shelfLabel.alignment = TextAlignment.Center;
            shelfLabel.characterSize = 0.06f;
            shelfLabel.fontSize = 48;
            shelfLabel.color = Color.white;
            shelfLabel.text = "RAF A\nFİYAT YOK\nMÜŞTERİ: BOŞ\nKASA: BEKLİYOR";

            return new StockFlowBuildResult(
                binding,
                statusText,
                shelfLabel,
                indicator.GetComponent<Renderer>());
        }

        private static CustomerFlowBuildResult BuildCustomerFlow(
            Transform parent,
            Material jacket,
            Material skin,
            Material denim,
            Material shoes,
            Material counterTop,
            Material counterBody,
            Material statusScreen)
        {
            Transform navigation = new GameObject("CustomerNavigation").transform;
            navigation.SetParent(parent, false);

            NavMeshSurface surface = navigation.gameObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Volume;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.layerMask = 1 << LayerMask.NameToLayer("Default");
            surface.center = new Vector3(0f, 1.45f, 0f);
            surface.size = new Vector3(7.8f, 3f, 9.6f);

            Transform waypoints = new GameObject("CustomerRouteWaypoints").transform;
            waypoints.SetParent(navigation, false);
            Transform entrance = CreateWaypoint(
                "CustomerEntranceWaypoint",
                waypoints,
                new Vector3(-0.15f, 0f, -4.25f));
            Transform browse = CreateWaypoint(
                "CustomerBrowseWaypoint",
                waypoints,
                new Vector3(2.35f, 0f, 0.55f));
            Transform checkout = CreateWaypoint(
                "CustomerCheckoutWaypoint",
                waypoints,
                new Vector3(1.85f, 0f, 2.45f));
            Transform exit = CreateWaypoint(
                "CustomerExitWaypoint",
                waypoints,
                new Vector3(0.20f, 0f, -4.20f));

            Transform checkoutStation = new GameObject("CustomerCheckoutStation").transform;
            checkoutStation.SetParent(parent, false);
            CreateBeveledCube(
                "CheckoutCounterBody",
                checkoutStation,
                new Vector3(0.65f, 0.48f, 3.05f),
                new Vector3(1.65f, 0.96f, 0.62f),
                0.035f,
                counterBody);
            CreateBeveledCube(
                "CheckoutCounterTop",
                checkoutStation,
                new Vector3(0.65f, 1.00f, 3.05f),
                new Vector3(1.82f, 0.10f, 0.72f),
                0.025f,
                counterTop);
            CreateBeveledCube(
                "CheckoutDisplay",
                checkoutStation,
                new Vector3(0.65f, 1.32f, 3.25f),
                new Vector3(0.54f, 0.42f, 0.08f),
                0.018f,
                statusScreen,
                false);
            CreateDetailCube(
                "CheckoutDisplayStand",
                checkoutStation,
                new Vector3(0.65f, 1.10f, 3.24f),
                new Vector3(0.07f, 0.22f, 0.07f),
                counterBody);

            GameObject checkoutTerminal = CreateBeveledCube(
                "CheckoutPlayerTerminal",
                checkoutStation,
                new Vector3(0.65f, 1.34f, 2.68f),
                new Vector3(0.68f, 0.48f, 0.055f),
                0.014f,
                statusScreen);
            checkoutTerminal.layer = LayerMask.NameToLayer(InteractableLayerName);
            Collider checkoutInteractionCollider = checkoutTerminal.GetComponent<Collider>();
            Require(
                checkoutInteractionCollider != null,
                "Checkout player terminal is missing its interaction collider.");
            TextMesh checkoutStatusText = new GameObject("CheckoutStationStatusText")
                .AddComponent<TextMesh>();
            checkoutStatusText.transform.SetParent(checkoutTerminal.transform, false);
            checkoutStatusText.transform.localPosition = new Vector3(0f, 0f, -0.040f);
            checkoutStatusText.anchor = TextAnchor.MiddleCenter;
            checkoutStatusText.alignment = TextAlignment.Center;
            checkoutStatusText.characterSize = 0.026f;
            checkoutStatusText.fontSize = 42;
            checkoutStatusText.color = new Color(0.88f, 0.96f, 0.98f);
            checkoutStatusText.text = "KASA İSTASYONU\nMÜŞTERİYİ BEKLİYOR";
            CheckoutStationProjection checkoutProjection =
                checkoutStation.gameObject.AddComponent<CheckoutStationProjection>();

            GameObject flowBoard = CreateBeveledCube(
                "CustomerFlowStatusBoard",
                checkoutStation,
                new Vector3(-0.55f, 1.72f, 3.38f),
                new Vector3(1.52f, 0.62f, 0.06f),
                0.018f,
                counterBody,
                false);
            TextMesh flowText = new GameObject("CustomerFlowStatusText").AddComponent<TextMesh>();
            flowText.transform.SetParent(flowBoard.transform, false);
            flowText.transform.localPosition = new Vector3(0f, 0f, -0.038f);
            flowText.anchor = TextAnchor.MiddleCenter;
            flowText.alignment = TextAlignment.Center;
            flowText.characterSize = 0.044f;
            flowText.fontSize = 44;
            flowText.color = new Color(0.82f, 0.93f, 0.96f);
            flowText.text = "MÜŞTERİ AKIŞI: TEKLİF BEKLİYOR\nROTA BEKLEMEDE";

            GameObject customer = new GameObject("PrototypeCustomer");
            customer.transform.SetParent(parent, false);
            customer.transform.position = entrance.position;
            customer.transform.rotation = Quaternion.identity;
            customer.layer = LayerMask.NameToLayer(InteractableLayerName);
            CapsuleCollider customerFocusCollider = customer.AddComponent<CapsuleCollider>();
            customerFocusCollider.center = new Vector3(0f, 0.90f, 0f);
            customerFocusCollider.radius = 0.34f;
            customerFocusCollider.height = 1.80f;
            customerFocusCollider.isTrigger = true;

            CreateBeveledCube(
                "CustomerTorso",
                customer.transform,
                new Vector3(0f, 1.16f, 0f),
                new Vector3(0.56f, 0.76f, 0.32f),
                0.055f,
                jacket,
                false);
            CreateBeveledCube(
                "CustomerWaist",
                customer.transform,
                new Vector3(0f, 0.76f, 0f),
                new Vector3(0.50f, 0.18f, 0.30f),
                0.035f,
                denim,
                false);
            CreateVisualCylinder(
                "CustomerLeg_Left",
                customer.transform,
                new Vector3(-0.15f, 0.39f, 0f),
                new Vector3(0.12f, 0.37f, 0.12f),
                Quaternion.identity,
                denim);
            CreateVisualCylinder(
                "CustomerLeg_Right",
                customer.transform,
                new Vector3(0.15f, 0.39f, 0f),
                new Vector3(0.12f, 0.37f, 0.12f),
                Quaternion.identity,
                denim);
            CreateBeveledCube(
                "CustomerShoe_Left",
                customer.transform,
                new Vector3(-0.15f, 0.09f, 0.07f),
                new Vector3(0.24f, 0.16f, 0.38f),
                0.035f,
                shoes,
                false);
            CreateBeveledCube(
                "CustomerShoe_Right",
                customer.transform,
                new Vector3(0.15f, 0.09f, 0.07f),
                new Vector3(0.24f, 0.16f, 0.38f),
                0.035f,
                shoes,
                false);
            CreateVisualCylinder(
                "CustomerArm_Left",
                customer.transform,
                new Vector3(-0.38f, 1.12f, 0f),
                new Vector3(0.10f, 0.37f, 0.10f),
                Quaternion.Euler(0f, 0f, -7f),
                jacket);
            CreateVisualCylinder(
                "CustomerArm_Right",
                customer.transform,
                new Vector3(0.38f, 1.12f, 0f),
                new Vector3(0.10f, 0.37f, 0.10f),
                Quaternion.Euler(0f, 0f, 7f),
                jacket);
            CreateVisualSphere(
                "CustomerHead",
                customer.transform,
                new Vector3(0f, 1.73f, 0f),
                new Vector3(0.34f, 0.40f, 0.34f),
                skin);
            CreateVisualSphere(
                "CustomerHair",
                customer.transform,
                new Vector3(0f, 1.89f, -0.015f),
                new Vector3(0.35f, 0.17f, 0.35f),
                shoes);

            TextMesh identityText = new GameObject("CustomerIdentityText").AddComponent<TextMesh>();
            identityText.transform.SetParent(customer.transform, false);
            identityText.transform.localPosition = new Vector3(0f, 2.18f, 0f);
            identityText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            identityText.anchor = TextAnchor.MiddleCenter;
            identityText.alignment = TextAlignment.Center;
            identityText.characterSize = 0.032f;
            identityText.fontSize = 40;
            identityText.color = new Color(0.90f, 0.94f, 0.96f);
            identityText.text = "MÜŞTERİ 001\nYARDIM BEKLİYOR";

            NavMeshAgent agent = customer.AddComponent<NavMeshAgent>();
            agent.radius = 0.28f;
            agent.height = 1.80f;
            agent.baseOffset = 0f;
            agent.speed = 2.20f;
            agent.angularSpeed = 540f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.12f;
            agent.autoBraking = true;
            agent.autoRepath = false;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            customer.SetActive(false);

            return new CustomerFlowBuildResult(
                surface,
                agent,
                customer,
                flowText,
                identityText,
                checkoutProjection,
                checkoutInteractionCollider,
                checkoutStatusText,
                entrance,
                browse,
                checkout,
                exit);
        }

        private static Transform CreateWaypoint(
            string name,
            Transform parent,
            Vector3 localPosition)
        {
            Transform waypoint = new GameObject(name).transform;
            waypoint.SetParent(parent, false);
            waypoint.localPosition = localPosition;
            return waypoint;
        }

        private static void CreateVisualCylinder(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            GameObject visual = CreateCylinder(
                name,
                parent,
                localPosition,
                localScale,
                localRotation,
                material);
            Collider collider = visual.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static void CreateVisualSphere(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = name;
            sphere.transform.SetParent(parent, false);
            sphere.transform.localPosition = localPosition;
            sphere.transform.localScale = localScale;
            sphere.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = sphere.GetComponent<Collider>();
            if (collider != null)
            {
                UnityEngine.Object.DestroyImmediate(collider);
            }
        }

        private static TransportCartProjection BuildTransportCart(
            Transform parent,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material labelPaper)
        {
            int interactableLayer = RequireLayer(InteractableLayerName);
            Transform cartRoot = new GameObject("TransportCart").transform;
            cartRoot.SetParent(parent, false);
            cartRoot.localPosition = new Vector3(-2.90f, 0f, -2.10f);
            cartRoot.localRotation = Quaternion.identity;

            CreateBeveledCube(
                "CartPlatform",
                cartRoot,
                new Vector3(0f, 0.30f, 0f),
                new Vector3(1.16f, 0.16f, 1.34f),
                0.025f,
                metal);
            CreateBeveledCube(
                "CartDeckMat",
                cartRoot,
                new Vector3(0f, 0.392f, 0f),
                new Vector3(1.02f, 0.025f, 1.18f),
                0.008f,
                rubber,
                false);

            foreach (float x in new[] { -0.49f, 0.49f })
            {
                CreateBeveledCube(
                    x < 0f ? "CartHandlePost_Left" : "CartHandlePost_Right",
                    cartRoot,
                    new Vector3(x, 0.95f, -0.60f),
                    new Vector3(0.075f, 1.20f, 0.075f),
                    0.015f,
                    brushedSteel);
            }

            CreateBeveledCube(
                "CartHandleGrip",
                cartRoot,
                new Vector3(0f, 1.55f, -0.60f),
                new Vector3(1.05f, 0.11f, 0.11f),
                0.025f,
                rubber);
            CreateDetailCube(
                "CartSafetyPlate",
                cartRoot,
                new Vector3(0f, 0.78f, -0.645f),
                new Vector3(0.44f, 0.22f, 0.018f),
                accent);
            CreateDetailCube(
                "CartIdentityLabel",
                cartRoot,
                new Vector3(0f, 0.78f, -0.657f),
                new Vector3(0.28f, 0.10f, 0.008f),
                labelPaper);

            foreach (float x in new[] { -0.53f, 0.53f })
            {
                foreach (float z in new[] { -0.49f, 0.49f })
                {
                    GameObject wheel = CreateCylinder(
                        $"CartWheel_{(x < 0f ? "L" : "R")}_{(z < 0f ? "Rear" : "Front")}",
                        cartRoot,
                        new Vector3(x, 0.15f, z),
                        new Vector3(0.28f, 0.06f, 0.28f),
                        Quaternion.Euler(0f, 0f, 90f),
                        rubber);
                    Collider wheelCollider = wheel.GetComponent<Collider>();
                    if (wheelCollider != null)
                    {
                        UnityEngine.Object.DestroyImmediate(wheelCollider);
                    }

                    CreateDetailCube(
                        $"CartWheelHub_{(x < 0f ? "L" : "R")}_{(z < 0f ? "Rear" : "Front")}",
                        cartRoot,
                        new Vector3(x, 0.15f, z),
                        new Vector3(0.10f, 0.10f, 0.10f),
                        brushedSteel).transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
                }
            }

            Transform cargoAnchor = new GameObject("CartCargoAnchor").transform;
            cargoAnchor.SetParent(cartRoot, false);
            cargoAnchor.localPosition = new Vector3(0f, 0.80f, 0.02f);

            SetLayerRecursively(cartRoot.gameObject, interactableLayer);
            Rigidbody cartBody = cartRoot.gameObject.AddComponent<Rigidbody>();
            cartBody.mass = 18f;
            cartBody.useGravity = false;
            cartBody.isKinematic = true;
            cartBody.interpolation = RigidbodyInterpolation.Interpolate;
            cartBody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            TransportCartProjection cart = cartRoot.gameObject.AddComponent<TransportCartProjection>();
            cart.Configure(
                "prototype.garage-transport-cart-001",
                "Platform Arabası",
                cartBody,
                cargoAnchor,
                new Vector3(0.61f, 0.74f, 0.72f),
                new Vector3(0.61f, 0.84f, 0.72f));
            return cart;
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

        private static GameObject CreateCombinedBoxDetails(
            string name,
            Transform parent,
            IReadOnlyList<Vector3> centers,
            IReadOnlyList<Vector3> sizes,
            Material material)
        {
            Require(centers != null && sizes != null && centers.Count == sizes.Count,
                $"Combined detail geometry is invalid: {name}");
            Require(centers.Count > 0, $"Combined detail geometry is empty: {name}");

            string meshPath = $"{MeshRoot}/{name}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            bool createAsset = mesh == null;
            mesh ??= new Mesh { name = name };
            mesh.Clear();

            var vertices = new List<Vector3>(centers.Count * 8);
            var triangles = new List<int>(centers.Count * 36);
            int[] boxTriangles =
            {
                0, 2, 1, 0, 3, 2,
                4, 5, 6, 4, 6, 7,
                0, 1, 5, 0, 5, 4,
                2, 3, 7, 2, 7, 6,
                1, 2, 6, 1, 6, 5,
                3, 0, 4, 3, 4, 7
            };
            for (int boxIndex = 0; boxIndex < centers.Count; boxIndex++)
            {
                Vector3 center = centers[boxIndex];
                Vector3 half = sizes[boxIndex] * 0.5f;
                int vertexOffset = vertices.Count;
                vertices.Add(center + new Vector3(-half.x, -half.y, -half.z));
                vertices.Add(center + new Vector3(half.x, -half.y, -half.z));
                vertices.Add(center + new Vector3(half.x, half.y, -half.z));
                vertices.Add(center + new Vector3(-half.x, half.y, -half.z));
                vertices.Add(center + new Vector3(-half.x, -half.y, half.z));
                vertices.Add(center + new Vector3(half.x, -half.y, half.z));
                vertices.Add(center + new Vector3(half.x, half.y, half.z));
                vertices.Add(center + new Vector3(-half.x, half.y, half.z));
                foreach (int triangleIndex in boxTriangles)
                {
                    triangles.Add(vertexOffset + triangleIndex);
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (createAsset)
            {
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            else
            {
                EditorUtility.SetDirty(mesh);
            }

            GameObject detail = new GameObject(name);
            detail.transform.SetParent(parent, false);
            detail.AddComponent<MeshFilter>().sharedMesh = mesh;
            detail.AddComponent<MeshRenderer>().sharedMaterial = material;
            return detail;
        }

        private static GameObject CreateProcessorSocketBase(
            string name,
            Transform parent,
            Material material)
        {
            return CreateHardSurfaceDetail(
                name,
                parent,
                material,
                (vertices, uvs, triangles) =>
                {
                    Vector3[] centers =
                    {
                        new Vector3(0f, 0f, -0.0025f),
                        new Vector3(-0.026f, 0f, 0.001f),
                        new Vector3(0.026f, 0f, 0.001f),
                        new Vector3(0f, -0.0235f, 0.001f),
                        new Vector3(0f, 0.0235f, 0.001f)
                    };
                    Vector3[] sizes =
                    {
                        new Vector3(0.060f, 0.052f, 0.005f),
                        new Vector3(0.004f, 0.047f, 0.002f),
                        new Vector3(0.004f, 0.047f, 0.002f),
                        new Vector3(0.052f, 0.005f, 0.002f),
                        new Vector3(0.052f, 0.005f, 0.002f)
                    };
                    for (int index = 0; index < centers.Length; index++)
                    {
                        AppendHardSurfaceBoxGeometry(
                            vertices,
                            uvs,
                            triangles,
                            centers[index],
                            sizes[index]);
                    }

                    AppendHardSurfaceTriangularPrismGeometry(
                        vertices,
                        uvs,
                        triangles,
                        new Vector2(0.01925f, -0.01850f),
                        new Vector2(0.02225f, -0.01850f),
                        new Vector2(0.02225f, -0.01550f),
                        0f,
                        0.0035f);
                });
        }

        private static GameObject CreateHardSurfaceBoxDetails(
            string name,
            Transform parent,
            IReadOnlyList<Vector3> centers,
            IReadOnlyList<Vector3> sizes,
            Material material)
        {
            Require(centers != null && sizes != null && centers.Count == sizes.Count,
                $"Hard-surface detail geometry is invalid: {name}");
            Require(centers.Count > 0,
                $"Hard-surface detail geometry is empty: {name}");
            return CreateHardSurfaceDetail(
                name,
                parent,
                material,
                (vertices, uvs, triangles) =>
                {
                    for (int index = 0; index < centers.Count; index++)
                    {
                        AppendHardSurfaceBoxGeometry(
                            vertices,
                            uvs,
                            triangles,
                            centers[index],
                            sizes[index]);
                    }
                });
        }

        private static GameObject CreateHardSurfaceDetail(
            string name,
            Transform parent,
            Material material,
            Action<List<Vector3>, List<Vector2>, List<int>> appendGeometry)
        {
            string meshPath = $"{MeshRoot}/{name}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            bool createAsset = mesh == null;
            mesh ??= new Mesh { name = name };
            mesh.Clear();

            var vertices = new List<Vector3>();
            var uvs = new List<Vector2>();
            var triangles = new List<int>();
            appendGeometry(vertices, uvs, triangles);
            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (createAsset)
            {
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            else
            {
                EditorUtility.SetDirty(mesh);
            }

            GameObject detail = new GameObject(name);
            detail.transform.SetParent(parent, false);
            detail.AddComponent<MeshFilter>().sharedMesh = mesh;
            detail.AddComponent<MeshRenderer>().sharedMaterial = material;
            return detail;
        }

        private static GameObject CreateProcessorPackage(
            string name,
            Transform parent,
            Material substrateMaterial,
            Material heatSpreaderMaterial)
        {
            string meshPath = $"{MeshRoot}/{name}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            bool createAsset = mesh == null;
            mesh ??= new Mesh { name = name };
            mesh.Clear();

            var vertices = new List<Vector3>(54);
            var uvs = new List<Vector2>(54);
            var substrateTriangles = new List<int>(48);
            var heatSpreaderTriangles = new List<int>(36);
            Vector2[] outline =
            {
                new Vector2(-0.0225f, -0.01875f),
                new Vector2(0.0185f, -0.01875f),
                new Vector2(0.0225f, -0.01475f),
                new Vector2(0.0225f, 0.01875f),
                new Vector2(-0.0225f, 0.01875f)
            };
            const float substrateBottom = -0.002f;
            const float substrateTop = -0.0004f;
            AppendHardSurfacePolygonPrismGeometry(
                vertices,
                uvs,
                substrateTriangles,
                outline,
                substrateBottom,
                substrateTop);

            AppendHardSurfaceBoxGeometry(
                vertices,
                uvs,
                heatSpreaderTriangles,
                new Vector3(0f, 0f, 0.0008f),
                new Vector3(0.031f, 0.027f, 0.0024f));

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 2;
            mesh.SetTriangles(substrateTriangles, 0);
            mesh.SetTriangles(heatSpreaderTriangles, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (createAsset)
            {
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            else
            {
                EditorUtility.SetDirty(mesh);
            }

            GameObject package = new GameObject(name);
            package.transform.SetParent(parent, false);
            package.AddComponent<MeshFilter>().sharedMesh = mesh;
            package.AddComponent<MeshRenderer>().sharedMaterials =
                new[] { substrateMaterial, heatSpreaderMaterial };
            return package;
        }

        private static GameObject CreateMemoryModulePackage(
            string name,
            Transform parent,
            Material pcbMaterial,
            Material chipMaterial,
            Material heatSpreaderMaterial,
            Material contactMaterial)
        {
            string meshPath = $"{MeshRoot}/{name}.asset";
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            bool createAsset = mesh == null;
            mesh ??= new Mesh { name = name };
            mesh.Clear();

            var vertices = new List<Vector3>(420);
            var uvs = new List<Vector2>(420);
            var pcbTriangles = new List<int>(108);
            var chipTriangles = new List<int>(288);
            var heatSpreaderTriangles = new List<int>(108);
            var contactTriangles = new List<int>(432);

            AppendHardSurfaceBoxGeometry(
                vertices,
                uvs,
                pcbTriangles,
                new Vector3(0f, 0.008f, 0f),
                new Vector3(0.134f, 0.024f, 0.004f));
            AppendHardSurfaceBoxGeometry(
                vertices,
                uvs,
                pcbTriangles,
                new Vector3(-0.0385f, -0.008f, 0f),
                new Vector3(0.057f, 0.008f, 0.004f));
            AppendHardSurfaceBoxGeometry(
                vertices,
                uvs,
                pcbTriangles,
                new Vector3(0.0355f, -0.008f, 0f),
                new Vector3(0.063f, 0.008f, 0.004f));

            for (int index = 0; index < 8; index++)
            {
                AppendHardSurfaceBoxGeometry(
                    vertices,
                    uvs,
                    chipTriangles,
                    new Vector3(-0.0525f + (index * 0.015f), 0.005f, 0.003f),
                    new Vector3(0.011f, 0.012f, 0.002f));
            }

            AppendHardSurfaceBoxGeometry(
                vertices,
                uvs,
                heatSpreaderTriangles,
                new Vector3(0f, 0.016f, 0.0042f),
                new Vector3(0.122f, 0.006f, 0.0024f));
            AppendHardSurfaceBoxGeometry(
                vertices,
                uvs,
                heatSpreaderTriangles,
                new Vector3(-0.040f, -0.001f, 0.0042f),
                new Vector3(0.047f, 0.004f, 0.0024f));
            AppendHardSurfaceBoxGeometry(
                vertices,
                uvs,
                heatSpreaderTriangles,
                new Vector3(0.038f, -0.001f, 0.0042f),
                new Vector3(0.051f, 0.004f, 0.0024f));

            for (int index = 0; index < 13; index++)
            {
                float x = -0.060f + (index * 0.010f);
                if (x > -0.012f && x < 0.006f)
                {
                    continue;
                }

                AppendHardSurfaceBoxGeometry(
                    vertices,
                    uvs,
                    contactTriangles,
                    new Vector3(x, -0.012f, 0.003f),
                    new Vector3(0.006f, 0.004f, 0.0015f));
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, uvs);
            mesh.subMeshCount = 4;
            mesh.SetTriangles(pcbTriangles, 0);
            mesh.SetTriangles(chipTriangles, 1);
            mesh.SetTriangles(heatSpreaderTriangles, 2);
            mesh.SetTriangles(contactTriangles, 3);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (createAsset)
            {
                AssetDatabase.CreateAsset(mesh, meshPath);
            }
            else
            {
                EditorUtility.SetDirty(mesh);
            }

            GameObject package = new GameObject(name);
            package.transform.SetParent(parent, false);
            package.AddComponent<MeshFilter>().sharedMesh = mesh;
            package.AddComponent<MeshRenderer>().sharedMaterials = new[]
            {
                pcbMaterial,
                chipMaterial,
                heatSpreaderMaterial,
                contactMaterial
            };
            return package;
        }

        private static void AppendHardSurfaceBoxGeometry(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 center,
            Vector3 size)
        {
            Vector3 half = size * 0.5f;
            Vector3[] boxVertices =
            {
                center + new Vector3(-half.x, -half.y, -half.z),
                center + new Vector3(half.x, -half.y, -half.z),
                center + new Vector3(half.x, half.y, -half.z),
                center + new Vector3(-half.x, half.y, -half.z),
                center + new Vector3(-half.x, -half.y, half.z),
                center + new Vector3(half.x, -half.y, half.z),
                center + new Vector3(half.x, half.y, half.z),
                center + new Vector3(-half.x, half.y, half.z)
            };
            AppendHardSurfaceQuad(
                vertices, uvs, triangles,
                boxVertices[0], boxVertices[3], boxVertices[2], boxVertices[1]);
            AppendHardSurfaceQuad(
                vertices, uvs, triangles,
                boxVertices[4], boxVertices[5], boxVertices[6], boxVertices[7]);
            AppendHardSurfaceQuad(
                vertices, uvs, triangles,
                boxVertices[0], boxVertices[1], boxVertices[5], boxVertices[4]);
            AppendHardSurfaceQuad(
                vertices, uvs, triangles,
                boxVertices[2], boxVertices[3], boxVertices[7], boxVertices[6]);
            AppendHardSurfaceQuad(
                vertices, uvs, triangles,
                boxVertices[1], boxVertices[2], boxVertices[6], boxVertices[5]);
            AppendHardSurfaceQuad(
                vertices, uvs, triangles,
                boxVertices[3], boxVertices[0], boxVertices[4], boxVertices[7]);
        }

        private static void AppendHardSurfaceTriangularPrismGeometry(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector2 first,
            Vector2 second,
            Vector2 third,
            float bottom,
            float top)
        {
            AppendHardSurfacePolygonPrismGeometry(
                vertices,
                uvs,
                triangles,
                new[] { first, second, third },
                bottom,
                top);
        }

        private static void AppendHardSurfacePolygonPrismGeometry(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            IReadOnlyList<Vector2> outline,
            float bottom,
            float top)
        {
            Require(outline != null && outline.Count >= 3,
                "Hard-surface polygon prism requires at least three points.");

            int bottomOffset = vertices.Count;
            foreach (Vector2 point in outline)
            {
                vertices.Add(new Vector3(point.x, point.y, bottom));
                uvs.Add(new Vector2(
                    point.x / 0.060f + 0.5f,
                    point.y / 0.052f + 0.5f));
            }

            for (int index = 1; index < outline.Count - 1; index++)
            {
                triangles.Add(bottomOffset);
                triangles.Add(bottomOffset + index + 1);
                triangles.Add(bottomOffset + index);
            }

            int topOffset = vertices.Count;
            foreach (Vector2 point in outline)
            {
                vertices.Add(new Vector3(point.x, point.y, top));
                uvs.Add(new Vector2(
                    point.x / 0.060f + 0.5f,
                    point.y / 0.052f + 0.5f));
            }

            for (int index = 1; index < outline.Count - 1; index++)
            {
                triangles.Add(topOffset);
                triangles.Add(topOffset + index);
                triangles.Add(topOffset + index + 1);
            }

            for (int index = 0; index < outline.Count; index++)
            {
                int next = (index + 1) % outline.Count;
                Vector2 currentPoint = outline[index];
                Vector2 nextPoint = outline[next];
                AppendHardSurfaceQuad(
                    vertices,
                    uvs,
                    triangles,
                    new Vector3(currentPoint.x, currentPoint.y, bottom),
                    new Vector3(nextPoint.x, nextPoint.y, bottom),
                    new Vector3(nextPoint.x, nextPoint.y, top),
                    new Vector3(currentPoint.x, currentPoint.y, top));
            }
        }

        private static void AppendHardSurfaceQuad(
            List<Vector3> vertices,
            List<Vector2> uvs,
            List<int> triangles,
            Vector3 first,
            Vector3 second,
            Vector3 third,
            Vector3 fourth)
        {
            int offset = vertices.Count;
            vertices.Add(first);
            vertices.Add(second);
            vertices.Add(third);
            vertices.Add(fourth);
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 1f));
            triangles.Add(offset);
            triangles.Add(offset + 1);
            triangles.Add(offset + 2);
            triangles.Add(offset);
            triangles.Add(offset + 2);
            triangles.Add(offset + 3);
        }

        private static GameObject CreateCylinder(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 localScale,
            Quaternion localRotation,
            Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinder.name = name;
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localPosition = localPosition;
            cylinder.transform.localRotation = localRotation;
            cylinder.transform.localScale = localScale;
            cylinder.GetComponent<Renderer>().sharedMaterial = material;
            return cylinder;
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

        private static void DisableDecorativeRendererCost(Renderer renderer)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode =
                MotionVectorGenerationMode.ForceNoMotion;
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
                MeshRoot,
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
