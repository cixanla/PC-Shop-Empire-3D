using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct Atx24PowerCableBuildKitBuildResult
        {
            public Atx24PowerCableBuildKitBuildResult(
                Atx24PowerCableBuildKitProjection projection,
                PlacementSurface surface,
                Transform snapAnchor,
                TextMesh progressText)
            {
                Projection = projection;
                Surface = surface;
                SnapAnchor = snapAnchor;
                ProgressText = progressText;
            }

            public Atx24PowerCableBuildKitProjection Projection { get; }

            public PlacementSurface Surface { get; }

            public Transform SnapAnchor { get; }

            public TextMesh ProgressText { get; }
        }

        private static Atx24PowerCableBuildKitBuildResult
            BuildAtx24PowerCableBuildKit(
                Transform environment,
                Material brushedSteel,
                Material accent,
                Material rubber,
                Material labelPaper)
        {
            Transform workshop = environment.Find(
                "VisualBenchmarkCorner/WorkshopCorner");
            Require(
                workshop != null,
                "WorkshopCorner is missing for the ATX24 power-cable Build Kit.");

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            Require(ignoreRaycast >= 0, "Ignore Raycast layer is missing.");

            Transform root = new GameObject(
                "Atx24PowerCableBuildKitStation").transform;
            root.SetParent(workshop, false);

            const float centerX = 3.43f;
            GameObject support = CreateBeveledCube(
                "Atx24PowerCableBuildKitPlacementTray",
                root,
                new Vector3(centerX, 1.009f, 4.14f),
                new Vector3(0.20f, 0.038f, 0.19f),
                0.008f,
                rubber);
            support.layer = 0;
            BoxCollider supportCollider = support.GetComponent<BoxCollider>();
            Require(
                supportCollider != null,
                "ATX24 power-cable Build Kit tray requires support.");
            DisableDecorativeRendererCost(support.GetComponent<Renderer>());

            PlacementSurface surface = support.AddComponent<PlacementSurface>();
            surface.Configure(
                Atx24PowerCableBuildKitProjection.PrototypeSurfaceIdValue,
                supportCollider,
                0.01f,
                180f);

            GameObject rearStop = CreateDetailCube(
                "Atx24PowerCableBuildKitRearStop",
                root,
                new Vector3(centerX, 1.036f, 4.248f),
                new Vector3(0.22f, 0.035f, 0.014f),
                accent);
            GameObject leftGuide = CreateDetailCube(
                "Atx24PowerCableBuildKitLeftGuide",
                root,
                new Vector3(centerX - 0.113f, 1.036f, 4.14f),
                new Vector3(0.012f, 0.035f, 0.21f),
                brushedSteel);
            GameObject rightGuide = CreateDetailCube(
                "Atx24PowerCableBuildKitRightGuide",
                root,
                new Vector3(centerX + 0.113f, 1.036f, 4.14f),
                new Vector3(0.012f, 0.035f, 0.21f),
                brushedSteel);
            GameObject identity = CreateBeveledCube(
                "Atx24PowerCableBuildKitIdentityCard",
                root,
                new Vector3(centerX, 1.085f, 3.86f),
                new Vector3(0.21f, 0.115f, 0.016f),
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
                "Atx24PowerCableBuildKitSnapAnchor").transform;
            snapAnchor.SetParent(root, false);
            snapAnchor.localPosition = new Vector3(centerX, 1.032f, 4.14f);
            snapAnchor.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            TextMesh progressText = new GameObject(
                "Atx24PowerCableBuildKitProgressText").AddComponent<TextMesh>();
            progressText.transform.SetParent(root, false);
            progressText.transform.localPosition =
                new Vector3(centerX, 1.085f, 3.849f);
            progressText.anchor = TextAnchor.MiddleCenter;
            progressText.alignment = TextAlignment.Center;
            progressText.characterSize = 0.0105f;
            progressText.fontSize = 40;
            progressText.color = new Color(0.12f, 0.10f, 0.075f);
            progressText.text = "BUILD KIT • 7/10\nATX24 BEKLİYOR";
            progressText.gameObject.layer = ignoreRaycast;
            DisableDecorativeRendererCost(progressText.GetComponent<Renderer>());

            Atx24PowerCableBuildKitProjection projection =
                root.gameObject.AddComponent<Atx24PowerCableBuildKitProjection>();
            return new Atx24PowerCableBuildKitBuildResult(
                projection,
                surface,
                snapAnchor,
                progressText);
        }
    }
}
