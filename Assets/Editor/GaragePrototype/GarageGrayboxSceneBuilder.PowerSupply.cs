using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct PowerSupplyBuildResult
        {
            public PowerSupplyBuildResult(
                PowerSupplyBayProjection bay,
                PowerSupplyAssemblyItemBinding binding,
                PhysicalItemProjection item,
                PowerSupplyRuntimeGeometry geometry)
            {
                Bay = bay;
                Binding = binding;
                Item = item;
                Geometry = geometry;
            }

            public PowerSupplyBayProjection Bay { get; }

            public PowerSupplyAssemblyItemBinding Binding { get; }

            public PhysicalItemProjection Item { get; }

            public PowerSupplyRuntimeGeometry Geometry { get; }
        }

        private static PowerSupplyBuildResult BuildPowerSupplyAssembly(
            Transform slice,
            Transform chassisRoot,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material labelPaper,
            int interactableLayer)
        {
            Transform bayRoot = new GameObject("PowerSupplyBottomRearBay").transform;
            bayRoot.SetParent(chassisRoot, false);

            GameObject floorFilter = CreateBeveledCube(
                "PowerSupplyFilteredFloorIntake",
                bayRoot,
                new Vector3(-0.75f, 1.045f, 4.25f),
                new Vector3(0.19f, 0.008f, 0.18f),
                0.003f,
                rubber,
                true);
            BoxCollider supportCollider = floorFilter.GetComponent<BoxCollider>();
            supportCollider.isTrigger = false;
            for (int bar = 0; bar < 7; bar++)
            {
                GameObject filterBar = CreateDetailCube(
                    $"PowerSupplyFloorFilterBar_{bar + 1}",
                    bayRoot,
                    new Vector3(-0.75f, 1.051f, 4.18f + (bar * 0.023f)),
                    new Vector3(0.17f, 0.003f, 0.005f),
                    brushedSteel);
                Object.DestroyImmediate(filterBar.GetComponent<Collider>());
                DisableDecorativeRendererCost(filterBar.GetComponent<Renderer>());
            }

            Transform rearPlate = new GameObject("PowerSupplyRearMountPlate").transform;
            rearPlate.SetParent(bayRoot, false);
            rearPlate.localPosition = new Vector3(-0.75f, 1.13f, 4.407f);
            GameObject rearPlateVisual = CreateBeveledCube(
                "PowerSupplyRearMountPlateVisual",
                rearPlate,
                Vector3.zero,
                new Vector3(0.19f, 0.18f, 0.008f),
                0.003f,
                brushedSteel,
                false);
            Object.DestroyImmediate(rearPlateVisual.GetComponent<Collider>());
            DisableDecorativeRendererCost(rearPlateVisual.GetComponent<Renderer>());

            Transform snapAnchor = new GameObject("PowerSupplyBaySnapAnchor").transform;
            snapAnchor.SetParent(bayRoot, false);
            snapAnchor.localPosition = new Vector3(-0.75f, 1.105f, 4.25f);
            snapAnchor.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            Vector3[] screwPositions =
            {
                new Vector3(-0.075f, 0.068f, -0.008f),
                new Vector3(0.075f, 0.068f, -0.008f),
                new Vector3(-0.075f, -0.068f, -0.008f),
                new Vector3(0.075f, -0.068f, -0.008f)
            };
            var fastenerPivots = new Transform[4];
            for (int index = 0; index < fastenerPivots.Length; index++)
            {
                Transform pivot = new GameObject(
                    $"PowerSupplyRearFastenerPivot_{index + 1}").transform;
                pivot.SetParent(rearPlate, false);
                pivot.localPosition = screwPositions[index];
                GameObject screw = CreateCylinder(
                    $"PowerSupplyRearFastener_{index + 1}",
                    pivot,
                    Vector3.zero,
                    new Vector3(0.007f, 0.004f, 0.007f),
                    Quaternion.Euler(90f, 0f, 0f),
                    accent);
                Object.DestroyImmediate(screw.GetComponent<Collider>());
                DisableDecorativeRendererCost(screw.GetComponent<Renderer>());
                GameObject slot = CreateDetailCube(
                    $"PowerSupplyRearFastenerSlot_{index + 1}",
                    pivot,
                    new Vector3(0f, 0f, -0.005f),
                    new Vector3(0.009f, 0.002f, 0.002f),
                    rubber);
                Object.DestroyImmediate(slot.GetComponent<Collider>());
                DisableDecorativeRendererCost(slot.GetComponent<Renderer>());
                fastenerPivots[index] = pivot;
            }

            GameObject focusTarget = new GameObject("PowerSupplyBayFocusTarget");
            focusTarget.transform.SetParent(bayRoot, false);
            focusTarget.transform.localPosition = new Vector3(-0.75f, 1.12f, 4.29f);
            focusTarget.layer = interactableLayer;
            BoxCollider focusCollider = focusTarget.AddComponent<BoxCollider>();
            focusCollider.size = new Vector3(0.25f, 0.23f, 0.27f);
            focusCollider.isTrigger = true;

            PowerSupplyBayProjection bay =
                bayRoot.gameObject.AddComponent<PowerSupplyBayProjection>();
            bay.Configure(
                GarageStockFlowSession.PowerSupplyBaySlotIdValue,
                GarageStockFlowSession.PowerSupplyRearMountIdValue,
                GarageStockFlowSession.PowerSupplyTopLeftFastenerIdValue,
                GarageStockFlowSession.PowerSupplyTopRightFastenerIdValue,
                GarageStockFlowSession.PowerSupplyBottomLeftFastenerIdValue,
                GarageStockFlowSession.PowerSupplyBottomRightFastenerIdValue,
                snapAnchor,
                focusCollider,
                supportCollider,
                chassisRoot,
                fastenerPivots[0],
                fastenerPivots[1],
                fastenerPivots[2],
                fastenerPivots[3],
                PowerSupplyFormFactor.AtxPs2,
                2f,
                0.94f);
            bay.ConfigureClearanceBlockers(null, null);
            SetLayerRecursively(bayRoot.gameObject, interactableLayer);
            floorFilter.layer = LayerMask.NameToLayer("Ignore Raycast");

            GameObject itemRoot = new GameObject("PrototypeNorthstarP01PowerSupply");
            itemRoot.transform.SetParent(slice, false);
            itemRoot.transform.localPosition = new Vector3(-0.17f, 1.033f, 3.93f);
            itemRoot.transform.localRotation = Quaternion.identity;
            itemRoot.layer = interactableLayer;

            Rigidbody body = itemRoot.AddComponent<Rigidbody>();
            body.mass = 2.35f;
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            Transform housing = CreateBeveledCube(
                "PowerSupplySteelHousing",
                itemRoot.transform,
                Vector3.zero,
                new Vector3(0.15f, 0.086f, 0.14f),
                0.008f,
                metal,
                false).transform;
            Object.DestroyImmediate(housing.GetComponent<Collider>());
            DisableDecorativeRendererCost(housing.GetComponent<Renderer>());

            Transform fanRoot = new GameObject("PowerSupplyIntakeFan").transform;
            fanRoot.SetParent(itemRoot.transform, false);
            fanRoot.localPosition = new Vector3(0f, -0.045f, 0f);
            GameObject fanRim = CreateCylinder(
                "PowerSupplyFanGrilleRim",
                fanRoot,
                Vector3.zero,
                new Vector3(0.054f, 0.003f, 0.054f),
                Quaternion.identity,
                rubber);
            Object.DestroyImmediate(fanRim.GetComponent<Collider>());
            DisableDecorativeRendererCost(fanRim.GetComponent<Renderer>());
            for (int spoke = 0; spoke < 8; spoke++)
            {
                Transform spokePivot = new GameObject(
                    $"PowerSupplyFanGrilleSpokePivot_{spoke + 1}").transform;
                spokePivot.SetParent(fanRoot, false);
                spokePivot.localRotation = Quaternion.Euler(0f, spoke * 45f, 0f);
                GameObject spokeVisual = CreateDetailCube(
                    $"PowerSupplyFanGrilleSpoke_{spoke + 1}",
                    spokePivot,
                    new Vector3(0.027f, -0.004f, 0f),
                    new Vector3(0.052f, 0.002f, 0.003f),
                    brushedSteel);
                Object.DestroyImmediate(spokeVisual.GetComponent<Collider>());
                DisableDecorativeRendererCost(spokeVisual.GetComponent<Renderer>());
            }

            Transform rearPanel = CreateDetailCube(
                "PowerSupplyRearPanel",
                itemRoot.transform,
                new Vector3(0f, 0f, 0.071f),
                new Vector3(0.142f, 0.078f, 0.004f),
                brushedSteel).transform;
            Object.DestroyImmediate(rearPanel.GetComponent<Collider>());
            DisableDecorativeRendererCost(rearPanel.GetComponent<Renderer>());
            Transform acInlet = CreateBeveledCube(
                "PowerSupplyAcInlet",
                itemRoot.transform,
                new Vector3(-0.042f, 0.012f, 0.075f),
                new Vector3(0.036f, 0.028f, 0.007f),
                0.002f,
                rubber,
                false).transform;
            Object.DestroyImmediate(acInlet.GetComponent<Collider>());
            DisableDecorativeRendererCost(acInlet.GetComponent<Renderer>());
            Transform rockerSwitch = CreateBeveledCube(
                "PowerSupplyRockerSwitch",
                itemRoot.transform,
                new Vector3(0.045f, 0.012f, 0.075f),
                new Vector3(0.024f, 0.020f, 0.008f),
                0.002f,
                accent,
                false).transform;
            Object.DestroyImmediate(rockerSwitch.GetComponent<Collider>());
            DisableDecorativeRendererCost(rockerSwitch.GetComponent<Renderer>());

            Transform modularPanel = CreateDetailCube(
                "PowerSupplyDisconnectedModularSocketPanel",
                itemRoot.transform,
                new Vector3(0f, 0f, -0.071f),
                new Vector3(0.128f, 0.064f, 0.004f),
                rubber).transform;
            Object.DestroyImmediate(modularPanel.GetComponent<Collider>());
            DisableDecorativeRendererCost(modularPanel.GetComponent<Renderer>());
            for (int socket = 0; socket < 4; socket++)
            {
                GameObject socketVisual = CreateDetailCube(
                    $"PowerSupplyModularSocket_{socket + 1}",
                    modularPanel,
                    new Vector3(-0.045f + (socket * 0.030f), 0f, -0.004f),
                    new Vector3(0.020f, 0.024f, 0.004f),
                    metal);
                Object.DestroyImmediate(socketVisual.GetComponent<Collider>());
                DisableDecorativeRendererCost(socketVisual.GetComponent<Renderer>());
            }

            Transform label = CreateDetailCube(
                "PowerSupplyNorthstarP01Label",
                itemRoot.transform,
                new Vector3(0f, 0.044f, 0f),
                new Vector3(0.095f, 0.002f, 0.052f),
                labelPaper).transform;
            Object.DestroyImmediate(label.GetComponent<Collider>());
            DisableDecorativeRendererCost(label.GetComponent<Renderer>());

            BoxCollider itemCollider = itemRoot.AddComponent<BoxCollider>();
            itemCollider.size = new Vector3(0.15f, 0.086f, 0.14f);
            SetLayerRecursively(itemRoot, interactableLayer);

            PhysicalItemProjection item =
                itemRoot.AddComponent<PhysicalItemProjection>();
            item.Configure(
                GarageStockFlowSession.PowerSupplyItemInstanceIdValue,
                GarageStockFlowSession.PowerSupplyDisplayName,
                body,
                new Vector3(0.075f, 0.043f, 0.070f),
                new Vector3(0f, -0.085f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent);
            PowerSupplyAssemblyItemBinding binding =
                itemRoot.AddComponent<PowerSupplyAssemblyItemBinding>();
            PowerSupplyRuntimeGeometry geometry =
                itemRoot.AddComponent<PowerSupplyRuntimeGeometry>();
            geometry.Configure(
                housing,
                fanRoot,
                floorFilter.transform,
                acInlet,
                rockerSwitch,
                modularPanel,
                rearPlate,
                fastenerPivots);

            return new PowerSupplyBuildResult(bay, binding, item, geometry);
        }
    }
}
