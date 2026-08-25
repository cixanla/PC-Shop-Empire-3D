using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct ProcessorBuildKitBuildResult
        {
            public ProcessorBuildKitBuildResult(
                ProcessorBuildKitProjection projection,
                PlacementSurface surface,
                Transform snapAnchor,
                TextMesh progressText)
            {
                Projection = projection;
                Surface = surface;
                SnapAnchor = snapAnchor;
                ProgressText = progressText;
            }

            public ProcessorBuildKitProjection Projection { get; }

            public PlacementSurface Surface { get; }

            public Transform SnapAnchor { get; }

            public TextMesh ProgressText { get; }
        }

        private static ProcessorBuildKitBuildResult BuildProcessorBuildKit(
            Transform environment,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material labelPaper)
        {
            Transform workshop = environment.Find(
                "VisualBenchmarkCorner/WorkshopCorner");
            Require(workshop != null, "WorkshopCorner is missing for the CPU Build Kit.");

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            Require(ignoreRaycast >= 0, "Ignore Raycast layer is missing.");

            Transform root = new GameObject("ProcessorBuildKitStation").transform;
            root.SetParent(workshop, false);

            GameObject support = CreateBeveledCube(
                "ProcessorBuildKitPlacementTray",
                root,
                new Vector3(1.66f, 1.009f, 4.14f),
                new Vector3(0.14f, 0.038f, 0.13f),
                0.008f,
                rubber);
            support.layer = 0;
            BoxCollider supportCollider = support.GetComponent<BoxCollider>();
            Require(supportCollider != null, "CPU Build Kit tray requires support.");
            DisableDecorativeRendererCost(support.GetComponent<Renderer>());

            PlacementSurface surface = support.AddComponent<PlacementSurface>();
            surface.Configure(
                ProcessorBuildKitProjection.PrototypeSurfaceIdValue,
                supportCollider,
                0.01f,
                90f);

            GameObject rearStop = CreateDetailCube(
                "ProcessorBuildKitRearStop",
                root,
                new Vector3(1.66f, 1.036f, 4.213f),
                new Vector3(0.16f, 0.035f, 0.014f),
                accent);
            GameObject leftGuide = CreateDetailCube(
                "ProcessorBuildKitLeftGuide",
                root,
                new Vector3(1.576f, 1.036f, 4.14f),
                new Vector3(0.012f, 0.035f, 0.16f),
                brushedSteel);
            GameObject identity = CreateBeveledCube(
                "ProcessorBuildKitIdentityCard",
                root,
                new Vector3(1.66f, 1.085f, 3.86f),
                new Vector3(0.18f, 0.115f, 0.016f),
                0.006f,
                labelPaper,
                false);
            SetLayerRecursively(rearStop, ignoreRaycast);
            SetLayerRecursively(leftGuide, ignoreRaycast);
            SetLayerRecursively(identity, ignoreRaycast);
            DisableDecorativeRendererCost(rearStop.GetComponent<Renderer>());
            DisableDecorativeRendererCost(leftGuide.GetComponent<Renderer>());
            DisableDecorativeRendererCost(identity.GetComponent<Renderer>());

            Transform snapAnchor = new GameObject(
                "ProcessorBuildKitSnapAnchor").transform;
            snapAnchor.SetParent(root, false);
            snapAnchor.localPosition = new Vector3(1.66f, 1.032f, 4.14f);
            snapAnchor.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            TextMesh progressText = new GameObject(
                "ProcessorBuildKitProgressText").AddComponent<TextMesh>();
            progressText.transform.SetParent(root, false);
            progressText.transform.localPosition = new Vector3(1.66f, 1.085f, 3.849f);
            progressText.anchor = TextAnchor.MiddleCenter;
            progressText.alignment = TextAlignment.Center;
            progressText.characterSize = 0.0105f;
            progressText.fontSize = 40;
            progressText.color = new Color(0.12f, 0.10f, 0.075f);
            progressText.text = "BUILD KIT • 1/10\nİŞLEMCİ BEKLİYOR";
            progressText.gameObject.layer = ignoreRaycast;
            DisableDecorativeRendererCost(progressText.GetComponent<Renderer>());

            ProcessorBuildKitProjection projection =
                root.gameObject.AddComponent<ProcessorBuildKitProjection>();
            return new ProcessorBuildKitBuildResult(
                projection,
                surface,
                snapAnchor,
                progressText);
        }
    }
}
