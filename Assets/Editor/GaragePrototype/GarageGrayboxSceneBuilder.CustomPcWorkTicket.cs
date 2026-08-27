using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct CustomPcWorkTicketBuildResult
        {
            public CustomPcWorkTicketBuildResult(
                CustomPcWorkTicketStationProjection projection,
                Collider interactionCollider,
                TextMesh statusText)
            {
                Projection = projection;
                InteractionCollider = interactionCollider;
                StatusText = statusText;
            }

            public CustomPcWorkTicketStationProjection Projection { get; }

            public Collider InteractionCollider { get; }

            public TextMesh StatusText { get; }
        }

        private static CustomPcWorkTicketBuildResult
            BuildCustomPcWorkTicketStation(
                Transform parent,
                Material metal,
                Material brushedSteel,
                Material accent,
                Material labelPaper)
        {
            Transform station = new GameObject("CustomPcWorkTicketStation").transform;
            station.SetParent(parent, false);
            station.localPosition = new Vector3(-3.35f, 0f, 4.78f);

            GameObject backing = CreateBeveledCube(
                "CustomPcWorkTicketPegboard",
                station,
                new Vector3(0f, 1.52f, 0f),
                new Vector3(0.82f, 0.92f, 0.055f),
                0.016f,
                metal,
                false);
            GameObject paper = CreateBeveledCube(
                "CustomPcWorkTicketPaper",
                station,
                new Vector3(0f, 1.48f, -0.037f),
                new Vector3(0.68f, 0.72f, 0.018f),
                0.008f,
                labelPaper,
                false);
            GameObject clip = CreateBeveledCube(
                "CustomPcWorkTicketClip",
                station,
                new Vector3(0f, 1.88f, -0.065f),
                new Vector3(0.24f, 0.10f, 0.035f),
                0.010f,
                brushedSteel,
                false);
            CreateDetailCube(
                "CustomPcWorkTicketAccent",
                station,
                new Vector3(0f, 1.15f, -0.060f),
                new Vector3(0.64f, 0.045f, 0.025f),
                accent);

            int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
            SetLayerRecursively(backing, ignoreRaycast);
            SetLayerRecursively(paper, ignoreRaycast);
            SetLayerRecursively(clip, ignoreRaycast);

            GameObject focusTarget = new GameObject("CustomPcWorkTicketFocusTarget");
            focusTarget.transform.SetParent(station, false);
            focusTarget.transform.localPosition = new Vector3(0f, 1.52f, -0.09f);
            focusTarget.layer = RequireLayer(InteractableLayerName);
            BoxCollider interactionCollider = focusTarget.AddComponent<BoxCollider>();
            interactionCollider.size = new Vector3(0.74f, 0.84f, 0.08f);
            interactionCollider.isTrigger = true;

            TextMesh statusText = new GameObject("CustomPcWorkTicketStatusText")
                .AddComponent<TextMesh>();
            statusText.transform.SetParent(station, false);
            statusText.transform.localPosition = new Vector3(0f, 1.49f, -0.055f);
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center;
            statusText.characterSize = 0.025f;
            statusText.fontSize = 40;
            statusText.color = new Color(0.12f, 0.10f, 0.075f);
            statusText.text = "ÖZEL PC İŞ EMRİ\nTEKLİF BEKLENİYOR";
            statusText.gameObject.layer = ignoreRaycast;

            CustomPcWorkTicketStationProjection projection =
                station.gameObject.AddComponent<CustomPcWorkTicketStationProjection>();
            return new CustomPcWorkTicketBuildResult(
                projection,
                interactionCollider,
                statusText);
        }
    }
}
