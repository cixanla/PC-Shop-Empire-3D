using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct Eps12vPowerCableBuildResult
        {
            public Eps12vPowerCableBuildResult(
                Eps12vPowerCableRouteProjection route,
                Eps12vPowerCableAssemblyItemBinding binding,
                PhysicalItemProjection item,
                Eps12vPowerCableRuntimeGeometry geometry)
            {
                Route = route;
                Binding = binding;
                Item = item;
                Geometry = geometry;
            }

            public Eps12vPowerCableRouteProjection Route { get; }

            public Eps12vPowerCableAssemblyItemBinding Binding { get; }

            public PhysicalItemProjection Item { get; }

            public Eps12vPowerCableRuntimeGeometry Geometry { get; }
        }

        private static Eps12vPowerCableBuildResult BuildEps12vPowerCableAssembly(
            Transform slice,
            Transform chassisRoot,
            Transform motherboardRoot,
            PhysicalItemProjection motherboard,
            PhysicalItemProjection powerSupply,
            PowerSupplyRuntimeGeometry powerSupplyGeometry,
            Material metal,
            Material accent,
            Material rubber,
            Material labelPaper,
            Material validMaterial,
            int interactableLayer)
        {
            Transform routeRoot = new GameObject(
                "Eps12vPowerCableAuthoredRoute").transform;
            routeRoot.SetParent(chassisRoot, false);

            Transform psuAnchor = new GameObject(
                "Eps12vPsuCpu8Anchor").transform;
            psuAnchor.SetParent(powerSupplyGeometry.ModularSocketPanel, false);
            psuAnchor.localPosition = new Vector3(0.0045f, 0f, -0.006f);
            Transform psuSocket = CreateBeveledCube(
                "Eps12vPsuCpu8Socket",
                psuAnchor,
                Vector3.zero,
                new Vector3(0.034f, 0.026f, 0.008f),
                0.002f,
                metal,
                false).transform;
            Object.DestroyImmediate(psuSocket.GetComponent<Collider>());
            DisableDecorativeRendererCost(psuSocket.GetComponent<Renderer>());

            Transform motherboardAnchor = new GameObject(
                "MotherboardEps12vCpuPowerHeader").transform;
            motherboardAnchor.SetParent(motherboardRoot, false);
            motherboardAnchor.localPosition = new Vector3(-0.100f, -0.070f, 0.018f);
            Transform motherboardHeader = CreateBeveledCube(
                "MotherboardEps12vCpuPowerHeaderHousing",
                motherboardAnchor,
                Vector3.zero,
                new Vector3(0.036f, 0.018f, 0.020f),
                0.002f,
                rubber,
                false).transform;
            Object.DestroyImmediate(motherboardHeader.GetComponent<Collider>());
            DisableDecorativeRendererCost(
                motherboardHeader.GetComponent<Renderer>());

            GameObject focusTarget = new GameObject(
                "MotherboardEps12vRouteFocusTarget");
            focusTarget.transform.SetParent(motherboardAnchor, false);
            focusTarget.transform.localPosition = new Vector3(0f, 0f, 0.018f);
            focusTarget.layer = interactableLayer;
            BoxCollider focusCollider = focusTarget.AddComponent<BoxCollider>();
            focusCollider.size = new Vector3(0.046f, 0.038f, 0.028f);
            focusCollider.isTrigger = true;

            Vector3[] waypointPositions =
            {
                new Vector3(-0.250f, 1.300f, 3.860f),
                new Vector3(-0.650f, 1.550f, 4.080f),
                new Vector3(-0.600f, 1.300f, 4.300f)
            };
            string[] waypointNames =
            {
                "Eps12vWaypointPsuExit",
                "Eps12vWaypointRearChannel",
                "Eps12vWaypointBoardEntry"
            };
            Transform[] waypoints = new Transform[3];
            for (int index = 0; index < waypoints.Length; index++)
            {
                Transform waypoint = new GameObject(waypointNames[index]).transform;
                waypoint.SetParent(routeRoot, false);
                waypoint.localPosition = waypointPositions[index];
                waypoints[index] = waypoint;
            }

            LineRenderer previewLine = CreateEps12vCableLine(
                "Eps12vPreviewRoute",
                routeRoot,
                validMaterial,
                0.009f);

            Collider motherboardCollider =
                motherboard != null
                    ? motherboard.transform.Find("MotherboardPcb")
                        ?.GetComponent<Collider>()
                    : null;
            Collider powerSupplyCollider =
                powerSupply != null ? powerSupply.GetComponent<Collider>() : null;
            var allowedRouteColliders = new[]
            {
                motherboardCollider,
                powerSupplyCollider
            };

            Eps12vPowerCableRouteProjection route =
                routeRoot.gameObject.AddComponent<Eps12vPowerCableRouteProjection>();
            route.Configure(
                GarageStockFlowSession.Eps12vPowerCableRouteIdValue,
                GarageStockFlowSession.Eps12vPowerCablePsuEndpointIdValue,
                GarageStockFlowSession.Eps12vPowerCableMotherboardEndpointIdValue,
                new[]
                {
                    GarageStockFlowSession.Eps12vPowerCableWaypoint1IdValue,
                    GarageStockFlowSession.Eps12vPowerCableWaypoint2IdValue,
                    GarageStockFlowSession.Eps12vPowerCableWaypoint3IdValue
                },
                psuAnchor,
                motherboardAnchor,
                waypoints,
                focusCollider,
                routeRoot,
                powerSupply.transform,
                motherboardRoot,
                allowedRouteColliders,
                previewLine,
                2f,
                0.94f,
                0.0065f);

            GameObject itemRoot = new GameObject("PrototypeEps12vPowerCable");
            itemRoot.transform.SetParent(slice, false);
            itemRoot.transform.localPosition = new Vector3(0.150f, 1.080f, 3.350f);
            itemRoot.transform.localRotation = Quaternion.identity;
            itemRoot.layer = interactableLayer;

            Rigidbody body = itemRoot.AddComponent<Rigidbody>();
            body.mass = 0.24f;
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            Transform psuConnector = CreateEps12vConnector(
                "Eps12vPsuCpu8Connector",
                itemRoot.transform,
                new Vector3(-0.030f, 0.006f, 0f),
                metal,
                accent);
            Transform motherboardConnector = CreateEps12vConnector(
                "Eps12vMotherboardCpu8Connector",
                itemRoot.transform,
                new Vector3(0.030f, 0.006f, 0f),
                rubber,
                accent);

            Transform identityPlate = CreateDetailCube(
                "Eps12vCableIdentityPlate",
                itemRoot.transform,
                new Vector3(0f, -0.022f, 0f),
                new Vector3(0.074f, 0.010f, 0.038f),
                labelPaper).transform;
            Object.DestroyImmediate(identityPlate.GetComponent<Collider>());
            DisableDecorativeRendererCost(identityPlate.GetComponent<Renderer>());

            LineRenderer looseCoil = CreateEps12vCableLine(
                "Eps12vLooseBraidedCoil",
                itemRoot.transform,
                rubber,
                0.009f);
            LineRenderer routedTrunk = CreateEps12vCableLine(
                "Eps12vRoutedCpuPowerTrunk",
                itemRoot.transform,
                rubber,
                0.011f);

            BoxCollider itemCollider = itemRoot.AddComponent<BoxCollider>();
            itemCollider.center = new Vector3(0f, 0.012f, 0f);
            itemCollider.size = new Vector3(0.090f, 0.078f, 0.052f);
            SetLayerRecursively(itemRoot, interactableLayer);
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            looseCoil.gameObject.layer = ignoreRaycastLayer;
            routedTrunk.gameObject.layer = ignoreRaycastLayer;

            PhysicalItemProjection item =
                itemRoot.AddComponent<PhysicalItemProjection>();
            item.Configure(
                GarageStockFlowSession.Eps12vPowerCableItemInstanceIdValue,
                GarageStockFlowSession.Eps12vPowerCableDisplayName,
                body,
                new Vector3(0.045f, 0.039f, 0.026f),
                new Vector3(0f, -0.070f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent,
                new Vector3(0f, 0.012f, 0f));

            Eps12vPowerCableRuntimeGeometry geometry =
                itemRoot.AddComponent<Eps12vPowerCableRuntimeGeometry>();
            geometry.Configure(
                psuConnector,
                motherboardConnector,
                looseCoil,
                routedTrunk,
                psuAnchor,
                motherboardAnchor,
                waypoints);

            Eps12vPowerCableAssemblyItemBinding binding =
                itemRoot.AddComponent<Eps12vPowerCableAssemblyItemBinding>();
            return new Eps12vPowerCableBuildResult(
                route,
                binding,
                item,
                geometry);
        }

        private static Transform CreateEps12vConnector(
            string name,
            Transform parent,
            Vector3 localPosition,
            Material housingMaterial,
            Material keyMaterial)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = localPosition;
            Vector3 size = new Vector3(0.034f, 0.028f, 0.024f);
            Transform housing = CreateBeveledCube(
                $"{name}Housing",
                root,
                Vector3.zero,
                size,
                0.003f,
                housingMaterial,
                false).transform;
            Object.DestroyImmediate(housing.GetComponent<Collider>());
            DisableDecorativeRendererCost(housing.GetComponent<Renderer>());

            Transform keyedLatch = CreateDetailCube(
                $"{name}KeyedLatch",
                root,
                new Vector3(0f, (size.y * 0.5f) + 0.004f, 0f),
                new Vector3(size.x * 0.42f, 0.006f, size.z * 0.34f),
                keyMaterial).transform;
            Object.DestroyImmediate(keyedLatch.GetComponent<Collider>());
            DisableDecorativeRendererCost(keyedLatch.GetComponent<Renderer>());

            TextMesh label = new GameObject($"{name}PinCount_8")
                .AddComponent<TextMesh>();
            label.transform.SetParent(root, false);
            label.transform.localPosition = new Vector3(
                0f,
                0f,
                -(size.z * 0.5f) - 0.002f);
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.005f;
            label.fontSize = 32;
            label.text = "8";
            label.color = Color.white;
            DisableDecorativeRendererCost(label.GetComponent<Renderer>());
            return root;
        }

        private static LineRenderer CreateEps12vCableLine(
            string name,
            Transform parent,
            Material material,
            float width)
        {
            GameObject lineObject = new GameObject(name);
            lineObject.transform.SetParent(parent, false);
            lineObject.layer = LayerMask.NameToLayer("Ignore Raycast");
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.widthMultiplier = width;
            line.numCapVertices = 5;
            line.numCornerVertices = 5;
            line.textureMode = LineTextureMode.Tile;
            line.alignment = LineAlignment.View;
            line.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = 0;
            line.enabled = false;
            return line;
        }
    }
}
