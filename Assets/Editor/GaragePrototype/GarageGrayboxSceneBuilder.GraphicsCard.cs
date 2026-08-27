using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct GraphicsCardBuildResult
        {
            public GraphicsCardBuildResult(
                GraphicsCardSlotProjection slot,
                GraphicsCardAssemblyItemBinding binding,
                PhysicalItemProjection item)
            {
                Slot = slot;
                Binding = binding;
                Item = item;
            }

            public GraphicsCardSlotProjection Slot { get; }

            public GraphicsCardAssemblyItemBinding Binding { get; }

            public PhysicalItemProjection Item { get; }
        }

        private static GraphicsCardBuildResult BuildGraphicsCardAssembly(
            Transform slice,
            Transform motherboardRoot,
            Material motherboardPcb,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material connectorPolymer,
            Material matteHardware,
            Material labelPaper,
            int interactableLayer,
            Collider[] chassisClearanceBlockers,
            Collider[] coolerClearanceBlockers)
        {
            Transform slotRoot = new GameObject(
                "MotherboardPcieX16GraphicsSlot").transform;
            slotRoot.SetParent(motherboardRoot, false);
            slotRoot.localPosition = new Vector3(0f, -0.074f, 0.012f);

            GameObject connector = CreateBeveledCube(
                "PcieX16Connector",
                slotRoot,
                Vector3.zero,
                new Vector3(0.190f, 0.014f, 0.016f),
                0.002f,
                rubber,
                true);
            BoxCollider supportCollider = connector.GetComponent<BoxCollider>();
            supportCollider.isTrigger = false;

            Transform snapAnchor = new GameObject(
                "GraphicsCardPcieX16SnapAnchor").transform;
            snapAnchor.SetParent(slotRoot, false);
            snapAnchor.localPosition = new Vector3(0f, 0f, 0.008f);

            Transform latchPivot = new GameObject(
                "GraphicsCardPcieLatchPivot").transform;
            latchPivot.SetParent(slotRoot, false);
            latchPivot.localPosition = new Vector3(0.103f, 0f, 0.012f);
            GameObject latch = CreateBeveledCube(
                "GraphicsCardPcieLatch",
                latchPivot,
                new Vector3(0.006f, 0f, 0.004f),
                new Vector3(0.024f, 0.026f, 0.010f),
                0.002f,
                accent,
                false);
            Object.DestroyImmediate(latch.GetComponent<Collider>());
            DisableDecorativeRendererCost(latch.GetComponent<Renderer>());

            Transform rearBracketPivot = new GameObject(
                "GraphicsCardRearBracket").transform;
            rearBracketPivot.SetParent(slotRoot, false);
            rearBracketPivot.localPosition = new Vector3(-0.143f, 0f, 0.042f);
            GameObject bracket = CreateBeveledCube(
                "GraphicsCardRearBracketPlate",
                rearBracketPivot,
                Vector3.zero,
                new Vector3(0.012f, 0.142f, 0.058f),
                0.002f,
                matteHardware,
                false);
            Object.DestroyImmediate(bracket.GetComponent<Collider>());
            DisableDecorativeRendererCost(bracket.GetComponent<Renderer>());

            Transform fastenerPivot = new GameObject(
                "GraphicsCardRearBracketFastenerPivot").transform;
            fastenerPivot.SetParent(rearBracketPivot, false);
            fastenerPivot.localPosition = new Vector3(0f, 0.058f, -0.024f);
            GameObject fastener = CreateCylinder(
                "GraphicsCardRearBracketFastener",
                fastenerPivot,
                Vector3.zero,
                new Vector3(0.006f, 0.003f, 0.006f),
                Quaternion.Euler(90f, 0f, 0f),
                accent);
            Object.DestroyImmediate(fastener.GetComponent<Collider>());
            DisableDecorativeRendererCost(fastener.GetComponent<Renderer>());

            GameObject focusTarget = new GameObject(
                "GraphicsCardSlotFocusTarget");
            focusTarget.transform.SetParent(slotRoot, false);
            focusTarget.transform.localPosition = new Vector3(0f, 0f, 0.064f);
            focusTarget.layer = interactableLayer;
            BoxCollider focusCollider = focusTarget.AddComponent<BoxCollider>();
            focusCollider.size = new Vector3(0.320f, 0.160f, 0.110f);
            focusCollider.isTrigger = true;

            GraphicsCardSlotProjection slot =
                slotRoot.gameObject.AddComponent<GraphicsCardSlotProjection>();
            slot.Configure(
                GarageStockFlowSession.GraphicsCardSlotIdValue,
                GarageStockFlowSession.GraphicsCardLatchIdValue,
                GarageStockFlowSession.GraphicsCardRearBracketIdValue,
                GarageStockFlowSession.GraphicsCardBracketFastenerIdValue,
                snapAnchor,
                focusCollider,
                supportCollider,
                motherboardRoot,
                latchPivot,
                fastenerPivot,
                GraphicsCardPcieInterface.PcieX16,
                2f,
                0.94f);
            slot.ConfigureClearanceBlockers(
                chassisClearanceBlockers,
                coolerClearanceBlockers);
            SetLayerRecursively(slotRoot.gameObject, interactableLayer);
            connector.layer = LayerMask.NameToLayer("Ignore Raycast");

            GameObject cardRoot = new GameObject("PrototypeNorthstarA60GraphicsCard");
            cardRoot.transform.SetParent(slice, false);
            cardRoot.transform.localPosition = new Vector3(-0.45f, 0.992f, 3.93f);
            cardRoot.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            cardRoot.layer = interactableLayer;

            Rigidbody body = cardRoot.AddComponent<Rigidbody>();
            body.mass = 0.82f;
            body.useGravity = false;
            body.isKinematic = true;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode =
                CollisionDetectionMode.ContinuousSpeculative;

            GameObject pcb = CreateBeveledCube(
                "GraphicsCardPcb",
                cardRoot.transform,
                new Vector3(0f, -0.004f, 0.062f),
                new Vector3(0.270f, 0.008f, 0.112f),
                0.003f,
                motherboardPcb,
                false);
            Object.DestroyImmediate(pcb.GetComponent<Collider>());
            DisableDecorativeRendererCost(pcb.GetComponent<Renderer>());

            GameObject shroud = CreateBeveledCube(
                "GraphicsCardDualFanShroud",
                cardRoot.transform,
                new Vector3(0.010f, -0.034f, 0.062f),
                new Vector3(0.252f, 0.052f, 0.104f),
                0.010f,
                metal,
                false);
            Object.DestroyImmediate(shroud.GetComponent<Collider>());
            DisableDecorativeRendererCost(shroud.GetComponent<Renderer>());

            for (int fanIndex = 0; fanIndex < 2; fanIndex++)
            {
                float x = fanIndex == 0 ? -0.058f : 0.072f;
                Transform fanRoot = new GameObject(
                    $"GraphicsCardFan_{fanIndex + 1}").transform;
                fanRoot.SetParent(cardRoot.transform, false);
                fanRoot.localPosition = new Vector3(x, -0.062f, 0.062f);
                GameObject rim = CreateCylinder(
                    $"GraphicsCardFanRim_{fanIndex + 1}",
                    fanRoot,
                    Vector3.zero,
                    new Vector3(0.044f, 0.004f, 0.044f),
                    Quaternion.identity,
                    rubber);
                Object.DestroyImmediate(rim.GetComponent<Collider>());
                DisableDecorativeRendererCost(rim.GetComponent<Renderer>());
                GameObject hub = CreateCylinder(
                    $"GraphicsCardFanHub_{fanIndex + 1}",
                    fanRoot,
                    new Vector3(0f, -0.005f, 0f),
                    new Vector3(0.012f, 0.006f, 0.012f),
                    Quaternion.identity,
                    accent);
                Object.DestroyImmediate(hub.GetComponent<Collider>());
                DisableDecorativeRendererCost(hub.GetComponent<Renderer>());
                for (int bladeIndex = 0; bladeIndex < 7; bladeIndex++)
                {
                    Transform bladePivot = new GameObject(
                        $"GraphicsCardFan{fanIndex + 1}BladePivot_{bladeIndex + 1}")
                        .transform;
                    bladePivot.SetParent(fanRoot, false);
                    bladePivot.localRotation = Quaternion.Euler(
                        0f,
                        bladeIndex * (360f / 7f),
                        0f);
                    GameObject blade = CreateBeveledCube(
                        $"GraphicsCardFan{fanIndex + 1}Blade_{bladeIndex + 1}",
                        bladePivot,
                        new Vector3(0.024f, -0.006f, 0f),
                        new Vector3(0.032f, 0.003f, 0.010f),
                        0.002f,
                        connectorPolymer,
                        false);
                    Object.DestroyImmediate(blade.GetComponent<Collider>());
                    DisableDecorativeRendererCost(blade.GetComponent<Renderer>());
                }
            }

            GameObject rearBracket = CreateBeveledCube(
                "GraphicsCardIoRearBracket",
                cardRoot.transform,
                new Vector3(-0.140f, -0.028f, 0.068f),
                new Vector3(0.012f, 0.056f, 0.136f),
                0.002f,
                matteHardware,
                false);
            Object.DestroyImmediate(rearBracket.GetComponent<Collider>());
            DisableDecorativeRendererCost(rearBracket.GetComponent<Renderer>());

            GameObject label = CreateDetailCube(
                "GraphicsCardNorthstarLabel",
                cardRoot.transform,
                new Vector3(0.010f, -0.061f, 0.102f),
                new Vector3(0.112f, 0.002f, 0.014f),
                labelPaper);
            Object.DestroyImmediate(label.GetComponent<Collider>());
            DisableDecorativeRendererCost(label.GetComponent<Renderer>());

            for (int contactIndex = 0; contactIndex < 12; contactIndex++)
            {
                float x = -0.090f + (contactIndex * 0.016f);
                GameObject contact = CreateDetailCube(
                    $"GraphicsCardPcieContact_{contactIndex + 1}",
                    cardRoot.transform,
                    new Vector3(x, -0.010f, 0.006f),
                    new Vector3(0.010f, 0.002f, 0.010f),
                    accent);
                Object.DestroyImmediate(contact.GetComponent<Collider>());
                DisableDecorativeRendererCost(contact.GetComponent<Renderer>());
            }

            BoxCollider cardCollider = cardRoot.AddComponent<BoxCollider>();
            cardCollider.center = new Vector3(0f, -0.032f, 0.0625f);
            cardCollider.size = new Vector3(0.285f, 0.064f, 0.125f);
            SetLayerRecursively(cardRoot, interactableLayer);

            PhysicalItemProjection item =
                cardRoot.AddComponent<PhysicalItemProjection>();
            item.Configure(
                GarageStockFlowSession.GraphicsCardAssemblyItemInstanceIdValue,
                GarageStockFlowSession.ProductDisplayName,
                body,
                new Vector3(0.1425f, 0.032f, 0.0625f),
                new Vector3(0.02f, -0.085f, 0f),
                new Vector3(0f, 180f, 0f),
                PhysicalCarryProfile.PcComponent,
                new Vector3(0f, -0.032f, 0.0625f));
            GraphicsCardAssemblyItemBinding binding =
                cardRoot.AddComponent<GraphicsCardAssemblyItemBinding>();

            return new GraphicsCardBuildResult(slot, binding, item);
        }
    }
}
