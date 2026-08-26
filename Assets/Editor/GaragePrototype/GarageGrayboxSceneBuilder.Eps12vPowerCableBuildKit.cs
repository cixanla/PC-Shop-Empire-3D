using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct Eps12vPowerCableBuildKitBuildResult
        {
            public Eps12vPowerCableBuildKitBuildResult(
                Eps12vPowerCableBuildKitProjection projection,
                PlacementSurface surface,
                Transform snapAnchor,
                TextMesh progressText)
            {
                Projection = projection;
                Surface = surface;
                SnapAnchor = snapAnchor;
                ProgressText = progressText;
            }

            public Eps12vPowerCableBuildKitProjection Projection { get; }

            public PlacementSurface Surface { get; }

            public Transform SnapAnchor { get; }

            public TextMesh ProgressText { get; }
        }

        private static Eps12vPowerCableBuildKitBuildResult
            BuildEps12vPowerCableBuildKit(
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
                "WorkshopCorner is missing for the EPS12V Build Kit.");

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            Require(ignoreRaycast >= 0, "Ignore Raycast layer is missing.");

            Transform root = new GameObject(
                "Eps12vPowerCableBuildKitStation").transform;
            root.SetParent(workshop, false);

            const float centerX = 3.69f;
            GameObject support = CreateBeveledCube(
                "Eps12vPowerCableBuildKitPlacementTray",
                root,
                new Vector3(centerX, 1.009f, 4.14f),
                new Vector3(0.20f, 0.038f, 0.19f),
                0.008f,
                rubber);
            support.layer = 0;
            BoxCollider supportCollider = support.GetComponent<BoxCollider>();
            Require(
                supportCollider != null,
                "EPS12V Build Kit tray requires support.");
            DisableDecorativeRendererCost(support.GetComponent<Renderer>());

            PlacementSurface surface = support.AddComponent<PlacementSurface>();
            surface.Configure(
                Eps12vPowerCableBuildKitProjection.PrototypeSurfaceIdValue,
                supportCollider,
                0.01f,
                180f);

            GameObject rearStop = CreateDetailCube(
                "Eps12vPowerCableBuildKitRearStop",
                root,
                new Vector3(centerX, 1.036f, 4.248f),
                new Vector3(0.22f, 0.035f, 0.014f),
                accent);
            GameObject leftGuide = CreateDetailCube(
                "Eps12vPowerCableBuildKitLeftGuide",
                root,
                new Vector3(centerX - 0.113f, 1.036f, 4.14f),
                new Vector3(0.012f, 0.035f, 0.21f),
                brushedSteel);
            GameObject rightGuide = CreateDetailCube(
                "Eps12vPowerCableBuildKitRightGuide",
                root,
                new Vector3(centerX + 0.113f, 1.036f, 4.14f),
                new Vector3(0.012f, 0.035f, 0.21f),
                brushedSteel);
            GameObject identity = CreateBeveledCube(
                "Eps12vPowerCableBuildKitIdentityCard",
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
                "Eps12vPowerCableBuildKitSnapAnchor").transform;
            snapAnchor.SetParent(root, false);
            snapAnchor.localPosition = new Vector3(centerX, 1.032f, 4.14f);
            snapAnchor.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            TextMesh progressText = new GameObject(
                "Eps12vPowerCableBuildKitProgressText").AddComponent<TextMesh>();
            progressText.transform.SetParent(root, false);
            progressText.transform.localPosition =
                new Vector3(centerX, 1.085f, 3.849f);
            progressText.anchor = TextAnchor.MiddleCenter;
            progressText.alignment = TextAlignment.Center;
            progressText.characterSize = 0.0105f;
            progressText.fontSize = 40;
            progressText.color = new Color(0.12f, 0.10f, 0.075f);
            progressText.text = "BUILD KIT • 8/10\nEPS12V BEKLİYOR";
            progressText.gameObject.layer = ignoreRaycast;
            DisableDecorativeRendererCost(progressText.GetComponent<Renderer>());

            Eps12vPowerCableBuildKitProjection projection =
                root.gameObject.AddComponent<Eps12vPowerCableBuildKitProjection>();
            return new Eps12vPowerCableBuildKitBuildResult(
                projection,
                surface,
                snapAnchor,
                progressText);
        }
    }
}
