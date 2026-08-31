using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed class AssemblyWorkbenchHeroReadabilityPlayModeTests
    {
        [UnityTest]
        public IEnumerator RuntimePreservesHeroReadabilityAndBoundedRenderBudget()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(
                GaragePrototypeMarker.Version,
                Is.EqualTo("garage-driver-bound-validation-r66-v1"));

            Transform heroRoot = Object.FindObjectsByType<Transform>(
                    FindObjectsSortMode.None)
                .Single(transform =>
                    transform.name == "AssemblyWorkbenchHeroReadability");
            Renderer[] heroRenderers = heroRoot.GetComponentsInChildren<Renderer>(
                true);
            Assert.That(heroRenderers.Length, Is.EqualTo(4));
            Assert.That(heroRoot.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(heroRoot.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(
                heroRenderers.All(renderer =>
                    renderer.gameObject.layer ==
                    LayerMask.NameToLayer("Ignore Raycast")),
                Is.True);
            Assert.That(
                heroRenderers.All(renderer =>
                    renderer.shadowCastingMode == ShadowCastingMode.Off &&
                    !renderer.receiveShadows &&
                    renderer.motionVectorGenerationMode ==
                    MotionVectorGenerationMode.ForceNoMotion),
                Is.True);

            AssertMaterial(heroRenderers, "AssemblyWorkbenchEsdMat", "WorkshopRubber");
            AssertMaterial(
                heroRenderers,
                "AssemblyWorkbenchSplashback",
                "Concrete");
            AssertMaterial(
                heroRenderers,
                "AssemblyWorkbenchZoneAccent",
                "SafetyAccent");
            AssertMaterial(
                heroRenderers,
                "AssemblyCableRouteReferenceStrip",
                "SafetyAccent");
            Renderer[] mattePolymerRenderers = Object.FindObjectsByType<Renderer>(
                    FindObjectsSortMode.None)
                .Where(renderer => new[]
                {
                    "PcieGpuPsuGpu8ConnectorHousing",
                    "PcieGpuGraphicsCardGpu8ConnectorSixPinHousing",
                    "PcieGpuGraphicsCardGpu8ConnectorTwoPinHousing",
                    "PcieGpuGraphicsCard8HeaderHousing",
                    "PowerSupplyFilteredFloorIntake"
                }.Contains(renderer.name))
                .ToArray();
            Assert.That(mattePolymerRenderers.Length, Is.EqualTo(5));
            Assert.That(
                mattePolymerRenderers.All(renderer =>
                    renderer.sharedMaterial != null &&
                    renderer.sharedMaterial.name.StartsWith(
                        "CableConnectorPolymer",
                        System.StringComparison.Ordinal) &&
                    renderer.sharedMaterial.shader != null &&
                    renderer.sharedMaterial.shader.name ==
                    "Universal Render Pipeline/Unlit" &&
                    renderer.sharedMaterial.HasProperty("_BaseColor") &&
                    renderer.sharedMaterial.GetColor("_BaseColor")
                        .maxColorComponent <= 0.031f &&
                    !renderer.sharedMaterial.IsKeywordEnabled("_EMISSION")),
                Is.True);
            Renderer[] graphicsCardFanBlades = Object.FindObjectsByType<Renderer>(
                    FindObjectsSortMode.None)
                .Where(renderer =>
                    renderer.name.StartsWith(
                        "GraphicsCardFan",
                        System.StringComparison.Ordinal) &&
                    renderer.name.Contains("Blade_"))
                .ToArray();
            Assert.That(graphicsCardFanBlades.Length, Is.EqualTo(14));
            Assert.That(
                graphicsCardFanBlades.All(renderer =>
                    renderer.sharedMaterial != null &&
                    renderer.sharedMaterial.name.StartsWith(
                        "CableConnectorPolymer",
                        System.StringComparison.Ordinal) &&
                    renderer.sharedMaterial.shader.name ==
                    "Universal Render Pipeline/Unlit"),
                Is.True);
            Renderer[] graphicsCardBrackets = Object.FindObjectsByType<Renderer>(
                    FindObjectsSortMode.None)
                .Where(renderer =>
                    renderer.name == "GraphicsCardRearBracketPlate" ||
                    renderer.name == "GraphicsCardIoRearBracket")
                .ToArray();
            Assert.That(graphicsCardBrackets.Length, Is.EqualTo(2));
            Assert.That(
                graphicsCardBrackets.All(renderer =>
                    renderer.sharedMaterial != null &&
                    renderer.sharedMaterial.name.StartsWith(
                        "WorkshopMatteHardware",
                        System.StringComparison.Ordinal) &&
                    renderer.sharedMaterial.shader.name ==
                    "Universal Render Pipeline/Unlit" &&
                    renderer.sharedMaterial.GetColor("_BaseColor")
                        .maxColorComponent <= 0.201f),
                Is.True);
            Transform retailHeroRoot = Object.FindObjectsByType<Transform>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None)
                .SingleOrDefault(transform =>
                    transform.name == "RetailCheckoutHeroReadability");
            Assert.That(
                Object.FindObjectsByType<MeshRenderer>(
                        FindObjectsSortMode.None)
                    .Count(renderer =>
                        retailHeroRoot == null ||
                        !renderer.transform.IsChildOf(retailHeroRoot)),
                Is.EqualTo(459));
            Assert.That(
                SceneManager.GetActiveScene().GetRootGameObjects()
                    .SelectMany(root =>
                        root.GetComponentsInChildren<MeshRenderer>(true))
                    .Count(renderer =>
                        retailHeroRoot == null ||
                        !renderer.transform.IsChildOf(retailHeroRoot)),
                Is.EqualTo(479));
            Assert.That(
                Object.FindObjectsByType<Light>(
                        FindObjectsSortMode.None)
                    .Count(light => light.name != "RetailCheckoutFillLight"),
                Is.EqualTo(4));
            Assert.That(
                Object.FindObjectsByType<Camera>(
                    FindObjectsSortMode.None).Length,
                Is.EqualTo(1));

            Light taskLight = Object.FindObjectsByType<Light>(
                    FindObjectsSortMode.None)
                .Single(light => light.name == "WorkbenchTaskLight");
            Assert.That(taskLight.type, Is.EqualTo(LightType.Spot));
            Assert.That(taskLight.intensity, Is.EqualTo(0.4f).Within(0.0001f));
            Assert.That(taskLight.range, Is.EqualTo(2.8f).Within(0.0001f));
            Assert.That(taskLight.spotAngle, Is.EqualTo(62f).Within(0.0001f));
            Assert.That(taskLight.innerSpotAngle, Is.EqualTo(38.44f).Within(0.001f));
            Assert.That(taskLight.shadows, Is.EqualTo(LightShadows.Soft));
            Assert.That(
                GaragePrototypeMarker
                    .AssemblyWorkbenchHeroReadabilitySmokeSuccessMarker,
                Does.Contain("connector-glare=bounded")
                    .And.Contain("glare-pixels<=64"));
        }

        private static void AssertMaterial(
            Renderer[] renderers,
            string rendererName,
            string materialPrefix)
        {
            Renderer renderer = renderers.Single(candidate =>
                candidate.name == rendererName);
            Assert.That(renderer.sharedMaterial, Is.Not.Null);
            Assert.That(
                renderer.sharedMaterial.name,
                Does.StartWith(materialPrefix));
        }
    }
}
