using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct StorageBuildKitBuildResult
        {
            public StorageBuildKitBuildResult(
                StorageBuildKitProjection projection,
                PlacementSurface surface,
                Transform snapAnchor,
                TextMesh progressText)
            {
                Projection = projection;
                Surface = surface;
                SnapAnchor = snapAnchor;
                ProgressText = progressText;
            }

            public StorageBuildKitProjection Projection { get; }

            public PlacementSurface Surface { get; }

            public Transform SnapAnchor { get; }

            public TextMesh ProgressText { get; }
        }

        private static StorageBuildKitBuildResult BuildStorageBuildKit(
            Transform environment,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material labelPaper)
        {
            Transform workshop = environment.Find(
                "VisualBenchmarkCorner/WorkshopCorner");
            Require(workshop != null, "WorkshopCorner is missing for the M.2 NVMe Build Kit.");

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            Require(ignoreRaycast >= 0, "Ignore Raycast layer is missing.");

            Transform root = new GameObject("StorageBuildKitStation").transform;
            root.SetParent(workshop, false);

            GameObject support = CreateBeveledCube(
                "StorageBuildKitPlacementTray",
                root,
                new Vector3(2.18f, 1.009f, 4.14f),
                new Vector3(0.14f, 0.038f, 0.13f),
                0.008f,
                rubber);
            support.layer = 0;
            BoxCollider supportCollider = support.GetComponent<BoxCollider>();
            Require(supportCollider != null, "M.2 NVMe Build Kit tray requires support.");
            DisableDecorativeRendererCost(support.GetComponent<Renderer>());

            PlacementSurface surface = support.AddComponent<PlacementSurface>();
            surface.Configure(
                StorageBuildKitProjection.PrototypeSurfaceIdValue,
                supportCollider,
                0.01f,
                180f);

            GameObject rearStop = CreateDetailCube(
                "StorageBuildKitRearStop",
                root,
                new Vector3(2.18f, 1.036f, 4.213f),
                new Vector3(0.16f, 0.035f, 0.014f),
                accent);
            GameObject leftGuide = CreateDetailCube(
                "StorageBuildKitLeftGuide",
                root,
                new Vector3(2.096f, 1.036f, 4.14f),
                new Vector3(0.012f, 0.035f, 0.16f),
                brushedSteel);
            GameObject rightGuide = CreateDetailCube(
                "StorageBuildKitRightGuide",
                root,
                new Vector3(2.264f, 1.036f, 4.14f),
                new Vector3(0.012f, 0.035f, 0.16f),
                brushedSteel);
            GameObject identity = CreateBeveledCube(
                "StorageBuildKitIdentityCard",
                root,
                new Vector3(2.18f, 1.085f, 3.86f),
                new Vector3(0.18f, 0.115f, 0.016f),
                0.006f,
                labelPaper,
                false);
            SetLayerRecursively(rearStop, ignoreRaycast);
            SetLayerRecursively(leftGuide, ignoreRaycast);
            SetLayerRecursively(rightGuide, ignoreRaycast);
            SetLayerRecursively(identity, ignoreRaycast);
            DisableDecorativeRendererCost(rearStop.GetComponent<Renderer>());
            DisableDecorativeRendererCost(leftGuide.GetComponent<Renderer>());
            DisableDecorativeRendererCost(rightGuide.GetComponent<Renderer>());
            DisableDecorativeRendererCost(identity.GetComponent<Renderer>());

            Transform snapAnchor = new GameObject(
                "StorageBuildKitSnapAnchor").transform;
            snapAnchor.SetParent(root, false);
            snapAnchor.localPosition = new Vector3(2.18f, 1.032f, 4.14f);
            snapAnchor.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            TextMesh progressText = new GameObject(
                "StorageBuildKitProgressText").AddComponent<TextMesh>();
            progressText.transform.SetParent(root, false);
            progressText.transform.localPosition = new Vector3(2.18f, 1.085f, 3.849f);
            progressText.anchor = TextAnchor.MiddleCenter;
            progressText.alignment = TextAlignment.Center;
            progressText.characterSize = 0.0105f;
            progressText.fontSize = 40;
            progressText.color = new Color(0.12f, 0.10f, 0.075f);
            progressText.text = "BUILD KIT • 3/10\nNVMe BEKLİYOR";
            progressText.gameObject.layer = ignoreRaycast;
            DisableDecorativeRendererCost(progressText.GetComponent<Renderer>());

            StorageBuildKitProjection projection =
                root.gameObject.AddComponent<StorageBuildKitProjection>();
            return new StorageBuildKitBuildResult(
                projection,
                surface,
                snapAnchor,
                progressText);
        }
    }
}
