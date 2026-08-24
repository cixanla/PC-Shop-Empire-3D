using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct MotherboardBuildKitBuildResult
        {
            public MotherboardBuildKitBuildResult(
                MotherboardBuildKitProjection projection,
                PlacementSurface surface,
                Transform snapAnchor,
                TextMesh progressText)
            {
                Projection = projection;
                Surface = surface;
                SnapAnchor = snapAnchor;
                ProgressText = progressText;
            }

            public MotherboardBuildKitProjection Projection { get; }

            public PlacementSurface Surface { get; }

            public Transform SnapAnchor { get; }

            public TextMesh ProgressText { get; }
        }

        private static MotherboardBuildKitBuildResult BuildMotherboardBuildKit(
            Transform environment,
            Material metal,
            Material brushedSteel,
            Material accent,
            Material rubber,
            Material labelPaper)
        {
            Transform workshop = environment.Find(
                "VisualBenchmarkCorner/WorkshopCorner");
            Require(workshop != null, "WorkshopCorner is missing for the Build Kit.");

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            Require(ignoreRaycast >= 0, "Ignore Raycast layer is missing.");

            Transform root = new GameObject("MotherboardBuildKitStation").transform;
            root.SetParent(workshop, false);

            GameObject support = CreateBeveledCube(
                "MotherboardBuildKitPlacementMat",
                root,
                new Vector3(1.35f, 1.005f, 4.14f),
                new Vector3(0.38f, 0.03f, 0.38f),
                0.010f,
                rubber);
            support.layer = 0;
            BoxCollider supportCollider = support.GetComponent<BoxCollider>();
            Require(supportCollider != null, "Build Kit placement mat requires support.");
            DisableDecorativeRendererCost(support.GetComponent<Renderer>());

            PlacementSurface surface = support.AddComponent<PlacementSurface>();
            surface.Configure(
                MotherboardBuildKitProjection.PrototypeSurfaceIdValue,
                supportCollider,
                0.01f,
                90f);

            GameObject leftRail = CreateDetailCube(
                "MotherboardBuildKitLeftRail",
                root,
                new Vector3(1.145f, 1.032f, 4.14f),
                new Vector3(0.025f, 0.035f, 0.42f),
                accent);
            GameObject rightRail = CreateDetailCube(
                "MotherboardBuildKitRightRail",
                root,
                new Vector3(1.555f, 1.032f, 4.14f),
                new Vector3(0.025f, 0.035f, 0.42f),
                accent);
            GameObject frontIdentity = CreateDetailCube(
                "MotherboardBuildKitFrontIdentity",
                root,
                new Vector3(1.35f, 1.025f, 3.93f),
                new Vector3(0.30f, 0.040f, 0.018f),
                labelPaper);
            SetLayerRecursively(leftRail, ignoreRaycast);
            SetLayerRecursively(rightRail, ignoreRaycast);
            SetLayerRecursively(frontIdentity, ignoreRaycast);

            Transform snapAnchor = new GameObject(
                "MotherboardBuildKitSnapAnchor").transform;
            snapAnchor.SetParent(root, false);
            snapAnchor.localPosition = new Vector3(1.35f, 1.026f, 4.14f);
            snapAnchor.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            GameObject board = CreateBeveledCube(
                "MotherboardBuildKitProgressBoard",
                root,
                new Vector3(1.35f, 1.72f, 4.705f),
                new Vector3(0.70f, 0.40f, 0.035f),
                0.012f,
                metal,
                false);
            GameObject paper = CreateBeveledCube(
                "MotherboardBuildKitProgressPaper",
                root,
                new Vector3(1.35f, 1.72f, 4.682f),
                new Vector3(0.62f, 0.32f, 0.012f),
                0.006f,
                labelPaper,
                false);
            GameObject clip = CreateBeveledCube(
                "MotherboardBuildKitProgressClip",
                root,
                new Vector3(1.35f, 1.93f, 4.665f),
                new Vector3(0.20f, 0.07f, 0.025f),
                0.008f,
                brushedSteel,
                false);
            SetLayerRecursively(board, ignoreRaycast);
            SetLayerRecursively(paper, ignoreRaycast);
            SetLayerRecursively(clip, ignoreRaycast);
            DisableDecorativeRendererCost(board.GetComponent<Renderer>());
            DisableDecorativeRendererCost(paper.GetComponent<Renderer>());
            DisableDecorativeRendererCost(clip.GetComponent<Renderer>());

            TextMesh progressText = new GameObject(
                "MotherboardBuildKitProgressText").AddComponent<TextMesh>();
            progressText.transform.SetParent(root, false);
            progressText.transform.localPosition = new Vector3(1.35f, 1.72f, 4.673f);
            progressText.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            progressText.anchor = TextAnchor.MiddleCenter;
            progressText.alignment = TextAlignment.Center;
            progressText.characterSize = 0.022f;
            progressText.fontSize = 42;
            progressText.color = new Color(0.12f, 0.10f, 0.075f);
            progressText.text = "BUILD KIT • 0/10\nANAKART BEKLİYOR";
            progressText.gameObject.layer = ignoreRaycast;
            DisableDecorativeRendererCost(progressText.GetComponent<Renderer>());

            MotherboardBuildKitProjection projection =
                root.gameObject.AddComponent<MotherboardBuildKitProjection>();
            return new MotherboardBuildKitBuildResult(
                projection,
                surface,
                snapAnchor,
                progressText);
        }
    }
}
