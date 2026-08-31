using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct ElectricalReadinessWorkbenchBuildResult
        {
            public ElectricalReadinessWorkbenchBuildResult(
                ElectricalReadinessWorkbenchProjection projection,
                ElectricalPowerTestStationProjection powerTestStation,
                TextMesh statusText,
                Renderer statusIndicator,
                Material readyMaterial,
                Material blockedMaterial)
            {
                Projection = projection;
                PowerTestStation = powerTestStation;
                StatusText = statusText;
                StatusIndicator = statusIndicator;
                ReadyMaterial = readyMaterial;
                BlockedMaterial = blockedMaterial;
            }

            public ElectricalReadinessWorkbenchProjection Projection { get; }

            public ElectricalPowerTestStationProjection PowerTestStation { get; }

            public TextMesh StatusText { get; }

            public Renderer StatusIndicator { get; }

            public Material ReadyMaterial { get; }

            public Material BlockedMaterial { get; }
        }

        private static ElectricalReadinessWorkbenchBuildResult
            BuildElectricalReadinessWorkbenchStatus(
                Transform parent,
                Material readyMaterial,
                Material blockedMaterial)
        {
            Transform root = new GameObject(
                "ElectricalReadinessWorkbenchStatus").transform;
            root.SetParent(parent, false);

            TextMesh statusText = new GameObject(
                "ElectricalReadinessWorkbenchStatusText").AddComponent<TextMesh>();
            statusText.transform.SetParent(root, false);
            statusText.transform.localPosition = new Vector3(1.35f, 1.36f, 4.066f);
            statusText.anchor = TextAnchor.MiddleCenter;
            statusText.alignment = TextAlignment.Center;
            statusText.characterSize = 0.0125f;
            statusText.fontSize = 40;
            statusText.color = new Color(1f, 0.78f, 0.50f);
            statusText.text =
                "ELEKTRİK KONTROLÜ\nANAKART EKSİK\nGÜÇ HAZIR DEĞİL";

            GameObject indicator = CreateDetailCube(
                "ElectricalReadinessWorkbenchIndicator",
                root,
                new Vector3(1.66f, 1.55f, 4.068f),
                new Vector3(0.045f, 0.045f, 0.010f),
                blockedMaterial);
            int ignoreRaycast = RequireLayer("Ignore Raycast");
            statusText.gameObject.layer = ignoreRaycast;
            indicator.layer = ignoreRaycast;
            DisableDecorativeRendererCost(statusText.GetComponent<Renderer>());
            Renderer statusIndicator = indicator.GetComponent<Renderer>();
            DisableDecorativeRendererCost(statusIndicator);

            ElectricalReadinessWorkbenchProjection projection =
                root.gameObject.AddComponent<ElectricalReadinessWorkbenchProjection>();
            ElectricalPowerTestStationProjection powerTestStation =
                root.gameObject.AddComponent<ElectricalPowerTestStationProjection>();
            return new ElectricalReadinessWorkbenchBuildResult(
                projection,
                powerTestStation,
                statusText,
                statusIndicator,
                readyMaterial,
                blockedMaterial);
        }
    }
}
