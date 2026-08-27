using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct PcieGpuPowerCableBuildResult
        {
            public PcieGpuPowerCableBuildResult(
                PcieGpuPowerCableRouteProjection route,
                PcieGpuPowerCableAssemblyItemBinding binding,
                PhysicalItemProjection item,
                PcieGpuPowerCableRuntimeGeometry geometry)
            {
                Route = route;
                Binding = binding;
                Item = item;
                Geometry = geometry;
            }

            public PcieGpuPowerCableRouteProjection Route { get; }

            public PcieGpuPowerCableAssemblyItemBinding Binding { get; }

            public PhysicalItemProjection Item { get; }

            public PcieGpuPowerCableRuntimeGeometry Geometry { get; }
        }

        private static PcieGpuPowerCableBuildResult BuildPcieGpuPowerCableAssembly(
            Transform slice,
            Transform chassisRoot,
            Transform graphicsCardRoot,
            PhysicalItemProjection graphicsCard,
            PhysicalItemProjection powerSupply,
            PowerSupplyRuntimeGeometry powerSupplyGeometry,
            Material metal,
            Material accent,
            Material rubber,
            Material connectorPolymer,
            Material labelPaper,
            Material validMaterial,
            int interactableLayer)
        {
            Transform routeRoot = new GameObject(
                "PcieGpuPowerCableAuthoredRoute").transform;
            routeRoot.SetParent(chassisRoot, false);

            Transform psuAnchor = new GameObject(
                "PcieGpuPsuGpu8Anchor").transform;
            psuAnchor.SetParent(powerSupplyGeometry.ModularSocketPanel, false);
            psuAnchor.localPosition = new Vector3(0.0045f, 0f, -0.006f);
            Transform psuSocket = CreateBeveledCube(
                "PcieGpuPsuGpu8Socket",
                psuAnchor,
                Vector3.zero,
                new Vector3(0.034f, 0.026f, 0.008f),
                0.002f,
                metal,
                false).transform;
            Object.DestroyImmediate(psuSocket.GetComponent<Collider>());
            DisableDecorativeRendererCost(psuSocket.GetComponent<Renderer>());

            Transform graphicsCardAnchor = new GameObject(
                "PcieGpuGraphicsCard8Anchor").transform;
            graphicsCardAnchor.SetParent(graphicsCardRoot, false);
            graphicsCardAnchor.localPosition = new Vector3(0.098f, -0.004f, 0.112f);
            Transform graphicsCardHeader = CreateBeveledCube(
                "PcieGpuGraphicsCard8HeaderHousing",
                graphicsCardAnchor,
                Vector3.zero,
                new Vector3(0.036f, 0.018f, 0.020f),
                0.002f,
                connectorPolymer,
                false).transform;
            Object.DestroyImmediate(graphicsCardHeader.GetComponent<Collider>());
            DisableDecorativeRendererCost(
                graphicsCardHeader.GetComponent<Renderer>());

            GameObject focusTarget = new GameObject(
                "PcieGpuGraphicsCardRouteFocusTarget");
            focusTarget.transform.SetParent(graphicsCardAnchor, false);
            focusTarget.transform.localPosition = new Vector3(0f, 0f, 0.018f);
            focusTarget.layer = interactableLayer;
            BoxCollider focusCollider = focusTarget.AddComponent<BoxCollider>();
            focusCollider.size = new Vector3(0.046f, 0.038f, 0.028f);
            focusCollider.isTrigger = true;

            Vector3[] waypointPositions =
            {
                new Vector3(-0.250f, 1.300f, 3.860f),
                new Vector3(-0.650f, 1.550f, 4.080f),
                new Vector3(-0.920f, 1.160f, 4.100f)
            };
            string[] waypointNames =
            {
                "PcieGpuWaypointPsuExit",
                "PcieGpuWaypointRearChannel",
                "PcieGpuWaypointGpuEntry"
            };
            Transform[] waypoints = new Transform[3];
            for (int index = 0; index < waypoints.Length; index++)
            {
                Transform waypoint = new GameObject(waypointNames[index]).transform;
                waypoint.SetParent(routeRoot, false);
                waypoint.localPosition = waypointPositions[index];
                waypoints[index] = waypoint;
            }

            LineRenderer previewLine = CreatePcieGpuCableLine(
                "PcieGpuPreviewRoute",
                routeRoot,
                validMaterial,
                0.009f);

            Collider graphicsCardCollider =
                graphicsCard != null
                    ? graphicsCard.GetComponent<Collider>()
                    : null;
            Collider powerSupplyCollider =
                powerSupply != null ? powerSupply.GetComponent<Collider>() : null;
            var allowedRouteColliders = new[]
            {
                graphicsCardCollider,
                powerSupplyCollider
            };

            PcieGpuPowerCableRouteProjection route =
                routeRoot.gameObject.AddComponent<PcieGpuPowerCableRouteProjection>();
            route.Configure(
                GarageStockFlowSession.PcieGpuPowerCableRouteIdValue,
                GarageStockFlowSession.PcieGpuPowerCablePsuEndpointIdValue,
                GarageStockFlowSession.PcieGpuPowerCableGraphicsCardEndpointIdValue,
                new[]
                {
                    GarageStockFlowSession.PcieGpuPowerCableWaypoint1IdValue,
                    GarageStockFlowSession.PcieGpuPowerCableWaypoint2IdValue,
                    GarageStockFlowSession.PcieGpuPowerCableWaypoint3IdValue
                },
                psuAnchor,
                graphicsCardAnchor,
                waypoints,
                focusCollider,
                routeRoot,
                powerSupply.transform,
                graphicsCardRoot,
                allowedRouteColliders,
                previewLine,
                2f,
                0.94f,
                0.0065f);

            GameObject itemRoot = new GameObject("PrototypePcieGpuPowerCable");
            itemRoot.transform.SetParent(slice, false);
            itemRoot.transform.localPosition = new Vector3(0.270f, 1.080f, 3.350f);
            itemRoot.transform.localRotation = Quaternion.identity;
            itemRoot.layer = interactableLayer;

            Rigidbody body = itemRoot.AddComponent<Rigidbody>();
            body.mass = 0.24f;
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            Transform psuConnector = CreatePcieGpuConnector(
                "PcieGpuPsuGpu8Connector",
                itemRoot.transform,
                new Vector3(-0.030f, 0.006f, 0f),
                connectorPolymer,
                accent,
                false);
            Transform graphicsCardConnector = CreatePcieGpuConnector(
                "PcieGpuGraphicsCardGpu8Connector",
                itemRoot.transform,
                new Vector3(0.030f, 0.006f, 0f),
                connectorPolymer,
                accent,
                true);

            Transform identityPlate = CreateDetailCube(
                "PcieGpuCableIdentityPlate",
                itemRoot.transform,
                new Vector3(0f, -0.022f, 0f),
                new Vector3(0.074f, 0.010f, 0.038f),
                labelPaper).transform;
            Object.DestroyImmediate(identityPlate.GetComponent<Collider>());
            DisableDecorativeRendererCost(identityPlate.GetComponent<Renderer>());

            LineRenderer looseCoil = CreatePcieGpuCableLine(
                "PcieGpuLooseBraidedCoil",
                itemRoot.transform,
                rubber,
                0.009f);
            LineRenderer routedTrunk = CreatePcieGpuCableLine(
                "PcieGpuRoutedGpuPowerTrunk",
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
                GarageStockFlowSession.PcieGpuPowerCableItemInstanceIdValue,
                GarageStockFlowSession.PcieGpuPowerCableDisplayName,
                body,
                new Vector3(0.045f, 0.039f, 0.026f),
                new Vector3(0f, -0.070f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent,
                new Vector3(0f, 0.012f, 0f));

            PcieGpuPowerCableRuntimeGeometry geometry =
                itemRoot.AddComponent<PcieGpuPowerCableRuntimeGeometry>();
            geometry.Configure(
                psuConnector,
                graphicsCardConnector,
                looseCoil,
                routedTrunk,
                psuAnchor,
                graphicsCardAnchor,
                waypoints);

            PcieGpuPowerCableAssemblyItemBinding binding =
                itemRoot.AddComponent<PcieGpuPowerCableAssemblyItemBinding>();
            return new PcieGpuPowerCableBuildResult(
                route,
                binding,
                item,
                geometry);
        }

        private static Transform CreatePcieGpuConnector(
            string name,
            Transform parent,
            Vector3 localPosition,
            Material housingMaterial,
            Material keyMaterial,
            bool splitSixPlusTwo)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = localPosition;
            Vector3 size = new Vector3(0.034f, 0.028f, 0.024f);

            if (splitSixPlusTwo)
            {
                const float splitGap = 0.0015f;
                const float sixPinWidth = 0.024f;
                const float twoPinWidth = 0.008f;
                float totalWidth = sixPinWidth + splitGap + twoPinWidth;
                float sixPinCenter = (-totalWidth + sixPinWidth) * 0.5f;
                float twoPinCenter = (totalWidth - twoPinWidth) * 0.5f;

                Transform sixPinHousing = CreateBeveledCube(
                    $"{name}SixPinHousing",
                    root,
                    new Vector3(sixPinCenter, 0f, 0f),
                    new Vector3(sixPinWidth, size.y, size.z),
                    0.0025f,
                    housingMaterial,
                    false).transform;
                Object.DestroyImmediate(sixPinHousing.GetComponent<Collider>());
                DisableDecorativeRendererCost(
                    sixPinHousing.GetComponent<Renderer>());

                Transform twoPinHousing = CreateBeveledCube(
                    $"{name}TwoPinHousing",
                    root,
                    new Vector3(twoPinCenter, 0f, 0f),
                    new Vector3(twoPinWidth, size.y, size.z),
                    0.0015f,
                    housingMaterial,
                    false).transform;
                Object.DestroyImmediate(twoPinHousing.GetComponent<Collider>());
                DisableDecorativeRendererCost(
                    twoPinHousing.GetComponent<Renderer>());

                Transform sixPinLatch = CreateDetailCube(
                    $"{name}SixPinKeyedLatch",
                    root,
                    new Vector3(
                        sixPinCenter,
                        (size.y * 0.5f) + 0.004f,
                        0f),
                    new Vector3(
                        sixPinWidth * 0.48f,
                        0.006f,
                        size.z * 0.34f),
                    keyMaterial).transform;
                Object.DestroyImmediate(sixPinLatch.GetComponent<Collider>());
                DisableDecorativeRendererCost(
                    sixPinLatch.GetComponent<Renderer>());

                Transform twoPinRetentionClip = CreateDetailCube(
                    $"{name}TwoPinRetentionClip",
                    root,
                    new Vector3(
                        twoPinCenter,
                        (size.y * 0.5f) + 0.0025f,
                        0f),
                    new Vector3(
                        twoPinWidth * 0.62f,
                        0.004f,
                        size.z * 0.28f),
                    keyMaterial).transform;
                Object.DestroyImmediate(
                    twoPinRetentionClip.GetComponent<Collider>());
                DisableDecorativeRendererCost(
                    twoPinRetentionClip.GetComponent<Renderer>());

                CreatePcieGpuPinCountLabel(
                    $"{name}PinCount_6",
                    root,
                    new Vector3(
                        sixPinCenter,
                        0f,
                        -(size.z * 0.5f) - 0.002f),
                    "6");
                CreatePcieGpuPinCountLabel(
                    $"{name}PinCount_2",
                    root,
                    new Vector3(
                        twoPinCenter,
                        0f,
                        -(size.z * 0.5f) - 0.002f),
                    "2");
                return root;
            }

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

            CreatePcieGpuPinCountLabel(
                $"{name}PinCount_8",
                root,
                new Vector3(0f, 0f, -(size.z * 0.5f) - 0.002f),
                "8");
            return root;
        }

        private static void CreatePcieGpuPinCountLabel(
            string name,
            Transform parent,
            Vector3 localPosition,
            string text)
        {
            TextMesh label = new GameObject(name).AddComponent<TextMesh>();
            label.transform.SetParent(parent, false);
            label.transform.localPosition = localPosition;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.005f;
            label.fontSize = 32;
            label.text = text;
            label.color = Color.white;
            DisableDecorativeRendererCost(label.GetComponent<Renderer>());
        }

        private static LineRenderer CreatePcieGpuCableLine(
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
