using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct Atx24PowerCableBuildResult
        {
            public Atx24PowerCableBuildResult(
                Atx24PowerCableRouteProjection route,
                Atx24PowerCableAssemblyItemBinding binding,
                PhysicalItemProjection item,
                Atx24PowerCableRuntimeGeometry geometry)
            {
                Route = route;
                Binding = binding;
                Item = item;
                Geometry = geometry;
            }

            public Atx24PowerCableRouteProjection Route { get; }

            public Atx24PowerCableAssemblyItemBinding Binding { get; }

            public PhysicalItemProjection Item { get; }

            public Atx24PowerCableRuntimeGeometry Geometry { get; }
        }

        private static Atx24PowerCableBuildResult BuildAtx24PowerCableAssembly(
            Transform slice,
            Transform chassisRoot,
            Transform motherboardRoot,
            PhysicalItemProjection powerSupply,
            PowerSupplyRuntimeGeometry powerSupplyGeometry,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material labelPaper,
            Material validMaterial,
            int interactableLayer)
        {
            Transform routeRoot = new GameObject(
                "Atx24PowerCableAuthoredRoute").transform;
            routeRoot.SetParent(chassisRoot, false);

            Transform psuPrimaryAnchor = new GameObject(
                "Atx24PsuPrimary18Anchor").transform;
            psuPrimaryAnchor.SetParent(
                powerSupplyGeometry.ModularSocketPanel,
                false);
            psuPrimaryAnchor.localPosition = new Vector3(-0.030f, 0f, -0.006f);
            Transform psuPrimarySocket = CreateBeveledCube(
                "Atx24PsuPrimary18Socket",
                psuPrimaryAnchor,
                Vector3.zero,
                new Vector3(0.048f, 0.026f, 0.008f),
                0.002f,
                metal,
                false).transform;
            Object.DestroyImmediate(psuPrimarySocket.GetComponent<Collider>());
            DisableDecorativeRendererCost(psuPrimarySocket.GetComponent<Renderer>());

            Transform psuSenseAnchor = new GameObject(
                "Atx24PsuSense10Anchor").transform;
            psuSenseAnchor.SetParent(
                powerSupplyGeometry.ModularSocketPanel,
                false);
            psuSenseAnchor.localPosition = new Vector3(0.030f, 0f, -0.006f);
            Transform psuSenseSocket = CreateBeveledCube(
                "Atx24PsuSense10Socket",
                psuSenseAnchor,
                Vector3.zero,
                new Vector3(0.030f, 0.026f, 0.008f),
                0.002f,
                metal,
                false).transform;
            Object.DestroyImmediate(psuSenseSocket.GetComponent<Collider>());
            DisableDecorativeRendererCost(psuSenseSocket.GetComponent<Renderer>());

            Transform motherboardAnchor = new GameObject(
                "MotherboardAtx24PowerHeader").transform;
            motherboardAnchor.SetParent(motherboardRoot, false);
            motherboardAnchor.localPosition = new Vector3(0.085f, -0.070f, 0.018f);
            Transform motherboardHeader = CreateBeveledCube(
                "MotherboardAtx24PowerHeaderHousing",
                motherboardAnchor,
                Vector3.zero,
                new Vector3(0.052f, 0.018f, 0.020f),
                0.002f,
                rubber,
                false).transform;
            Object.DestroyImmediate(motherboardHeader.GetComponent<Collider>());
            DisableDecorativeRendererCost(motherboardHeader.GetComponent<Renderer>());

            GameObject focusTarget = new GameObject(
                "MotherboardAtx24RouteFocusTarget");
            focusTarget.transform.SetParent(motherboardAnchor, false);
            focusTarget.transform.localPosition = new Vector3(0f, 0f, 0.018f);
            focusTarget.layer = interactableLayer;
            BoxCollider focusCollider = focusTarget.AddComponent<BoxCollider>();
            focusCollider.size = new Vector3(0.072f, 0.052f, 0.030f);
            focusCollider.isTrigger = true;

            Vector3[] waypointPositions =
            {
                new Vector3(-0.170f, 1.500f, 3.950f),
                new Vector3(-0.620f, 1.500f, 4.050f),
                new Vector3(-0.800f, 1.500f, 4.180f)
            };
            string[] waypointNames =
            {
                "Atx24WaypointPsuExit",
                "Atx24WaypointRearChannel",
                "Atx24WaypointBoardEntry"
            };
            Transform[] waypoints = new Transform[3];
            for (int index = 0; index < waypoints.Length; index++)
            {
                Transform waypoint = new GameObject(waypointNames[index]).transform;
                waypoint.SetParent(routeRoot, false);
                waypoint.localPosition = waypointPositions[index];
                waypoints[index] = waypoint;
            }

            LineRenderer[] previewLines =
            {
                CreateAtx24CableLine(
                    "Atx24PreviewPrimary18Branch",
                    routeRoot,
                    validMaterial,
                    0.009f),
                CreateAtx24CableLine(
                    "Atx24PreviewSense10Branch",
                    routeRoot,
                    validMaterial,
                    0.008f),
                CreateAtx24CableLine(
                    "Atx24PreviewMainTrunk",
                    routeRoot,
                    validMaterial,
                    0.012f)
            };

            Atx24PowerCableRouteProjection route =
                routeRoot.gameObject.AddComponent<Atx24PowerCableRouteProjection>();
            route.Configure(
                GarageStockFlowSession.Atx24PowerCableRouteIdValue,
                GarageStockFlowSession.Atx24PowerCablePsuPrimaryEndpointIdValue,
                GarageStockFlowSession.Atx24PowerCablePsuSenseEndpointIdValue,
                GarageStockFlowSession.Atx24PowerCableMotherboardEndpointIdValue,
                new[]
                {
                    GarageStockFlowSession.Atx24PowerCableWaypoint1IdValue,
                    GarageStockFlowSession.Atx24PowerCableWaypoint2IdValue,
                    GarageStockFlowSession.Atx24PowerCableWaypoint3IdValue
                },
                psuPrimaryAnchor,
                psuSenseAnchor,
                motherboardAnchor,
                waypoints,
                focusCollider,
                routeRoot,
                powerSupply.transform,
                motherboardRoot,
                previewLines,
                2f,
                0.94f,
                0.0075f);

            GameObject itemRoot = new GameObject("PrototypeAtx24PowerCable");
            itemRoot.transform.SetParent(slice, false);
            itemRoot.transform.localPosition = new Vector3(-0.05f, 1.08f, 3.35f);
            itemRoot.transform.localRotation = Quaternion.identity;
            itemRoot.layer = interactableLayer;

            Rigidbody body = itemRoot.AddComponent<Rigidbody>();
            body.mass = 0.32f;
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            Transform primaryConnector = CreateAtx24Connector(
                "Atx24PsuPrimary18Connector",
                itemRoot.transform,
                new Vector3(-0.047f, 0.004f, 0f),
                new Vector3(0.050f, 0.028f, 0.024f),
                metal,
                accent,
                "18");
            Transform senseConnector = CreateAtx24Connector(
                "Atx24PsuSense10Connector",
                itemRoot.transform,
                new Vector3(0.020f, 0.004f, 0f),
                new Vector3(0.034f, 0.028f, 0.024f),
                metal,
                accent,
                "10");
            Transform motherboardConnector = CreateAtx24Connector(
                "Atx24Motherboard24Connector",
                itemRoot.transform,
                new Vector3(0.005f, 0.052f, 0f),
                new Vector3(0.056f, 0.030f, 0.026f),
                rubber,
                accent,
                "24");

            Transform identityPlate = CreateDetailCube(
                "Atx24CableIdentityPlate",
                itemRoot.transform,
                new Vector3(0f, -0.026f, 0f),
                new Vector3(0.090f, 0.012f, 0.045f),
                labelPaper).transform;
            Object.DestroyImmediate(identityPlate.GetComponent<Collider>());
            DisableDecorativeRendererCost(identityPlate.GetComponent<Renderer>());

            LineRenderer looseCoil = CreateAtx24CableLine(
                "Atx24LooseBraidedCoil",
                itemRoot.transform,
                rubber,
                0.011f);
            LineRenderer routedPrimary = CreateAtx24CableLine(
                "Atx24RoutedPrimary18Branch",
                itemRoot.transform,
                rubber,
                0.010f);
            LineRenderer routedSense = CreateAtx24CableLine(
                "Atx24RoutedSense10Branch",
                itemRoot.transform,
                rubber,
                0.009f);
            LineRenderer routedTrunk = CreateAtx24CableLine(
                "Atx24RoutedMainTrunk",
                itemRoot.transform,
                rubber,
                0.013f);

            BoxCollider itemCollider = itemRoot.AddComponent<BoxCollider>();
            itemCollider.center = new Vector3(0f, 0.014f, 0f);
            itemCollider.size = new Vector3(0.125f, 0.105f, 0.060f);
            SetLayerRecursively(itemRoot, interactableLayer);
            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            looseCoil.gameObject.layer = ignoreRaycastLayer;
            routedPrimary.gameObject.layer = ignoreRaycastLayer;
            routedSense.gameObject.layer = ignoreRaycastLayer;
            routedTrunk.gameObject.layer = ignoreRaycastLayer;

            PhysicalItemProjection item =
                itemRoot.AddComponent<PhysicalItemProjection>();
            item.Configure(
                GarageStockFlowSession.Atx24PowerCableItemInstanceIdValue,
                GarageStockFlowSession.Atx24PowerCableDisplayName,
                body,
                new Vector3(0.0625f, 0.0525f, 0.030f),
                new Vector3(0f, -0.075f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent,
                new Vector3(0f, 0.014f, 0f));

            Atx24PowerCableRuntimeGeometry geometry =
                itemRoot.AddComponent<Atx24PowerCableRuntimeGeometry>();
            geometry.Configure(
                primaryConnector,
                senseConnector,
                motherboardConnector,
                looseCoil,
                routedPrimary,
                routedSense,
                routedTrunk,
                psuPrimaryAnchor,
                psuSenseAnchor,
                motherboardAnchor,
                waypoints);

            Atx24PowerCableAssemblyItemBinding binding =
                itemRoot.AddComponent<Atx24PowerCableAssemblyItemBinding>();
            return new Atx24PowerCableBuildResult(
                route,
                binding,
                item,
                geometry);
        }

        private static Transform CreateAtx24Connector(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector3 size,
            Material housingMaterial,
            Material keyMaterial,
            string pinLabel)
        {
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = localPosition;
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

            TextMesh label = new GameObject($"{name}PinCount_{pinLabel}")
                .AddComponent<TextMesh>();
            label.transform.SetParent(root, false);
            label.transform.localPosition = new Vector3(
                0f,
                0f,
                -(size.z * 0.5f) - 0.002f);
            label.transform.localRotation = Quaternion.identity;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.005f;
            label.fontSize = 32;
            label.text = pinLabel;
            label.color = Color.white;
            DisableDecorativeRendererCost(label.GetComponent<Renderer>());
            return root;
        }

        private static LineRenderer CreateAtx24CableLine(
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
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.positionCount = 0;
            line.enabled = false;
            return line;
        }
    }
}
