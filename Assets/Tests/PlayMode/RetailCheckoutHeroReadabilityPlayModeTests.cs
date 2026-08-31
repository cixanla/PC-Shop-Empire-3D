using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed class RetailCheckoutHeroReadabilityPlayModeTests
    {
        [UnityTest]
        public IEnumerator RuntimePreservesRetailCheckoutHeroAndAuthorityBudget()
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
                Is.EqualTo("garage-safe-power-state-r61-v1"));

            Transform[] transforms = SceneManager.GetActiveScene()
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<Transform>(true))
                .ToArray();
            Transform heroRoot = transforms.Single(transform =>
                transform.name == "RetailCheckoutHeroReadability");
            Renderer[] heroRenderers = heroRoot.GetComponentsInChildren<Renderer>(true);
            Assert.That(heroRenderers.Length, Is.EqualTo(9));
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

            AssertMaterial(
                heroRenderers,
                "RetailCheckoutHeroDarkMetalDetails",
                "DarkMetal");
            AssertMaterial(
                heroRenderers,
                "RetailCheckoutHeroBrushedSteelDetails",
                "BrushedSteel");
            AssertMaterial(
                heroRenderers,
                "RetailCheckoutHeroSafetyAccentDetails",
                "SafetyAccent");
            AssertMaterial(
                heroRenderers,
                "RetailCheckoutHeroRubberDetails",
                "WorkshopRubber");
            AssertMaterial(
                heroRenderers,
                "RetailCheckoutLightDiffuser",
                "LabelPaper");
            AssertMaterial(
                heroRenderers,
                "RetailShelfOfferStateVisual",
                "LabelPaper");
            AssertMaterial(
                heroRenderers,
                "RetailBasketReservedStateVisual",
                "SafetyAccent");
            AssertMaterial(
                heroRenderers,
                "CheckoutCashStateVisual",
                "LabelPaper");
            AssertMaterial(
                heroRenderers,
                "CheckoutReceiptStateVisual",
                "LabelPaper");

            RetailCheckoutHeroProjection heroProjection =
                heroRoot.GetComponent<RetailCheckoutHeroProjection>();
            Assert.That(heroProjection, Is.Not.Null);
            Assert.That(heroProjection.StockFlow, Is.SameAs(marker.StockFlow));
            Assert.That(heroProjection.ShelfOfferVisual.activeSelf, Is.False);
            Assert.That(heroProjection.BasketReservedVisual.activeSelf, Is.False);
            Assert.That(heroProjection.CashCheckoutVisual.activeSelf, Is.False);
            Assert.That(heroProjection.ReceiptVisual.activeSelf, Is.False);

            Assert.That(
                transforms.Count(transform => transform.name == "StarterShelf"),
                Is.Zero);
            Assert.That(
                transforms.Count(transform =>
                    transform.name == "ShelfPartsBox" ||
                    transform.name == "ShelfTechUnit" ||
                    transform.name == "ShelfTechDisplay"),
                Is.Zero);
            Assert.That(
                transforms.Count(transform =>
                    transform.name == "AuthoritativeRetailShelfA"),
                Is.EqualTo(1));
            Transform authoritativeShelf = transforms.Single(transform =>
                transform.name == "AuthoritativeRetailShelfA");
            Assert.That(
                authoritativeShelf.GetComponentsInChildren<Collider>(true).Length,
                Is.EqualTo(5));
            PlacementSurface[] shelfSurfaces = authoritativeShelf
                .GetComponentsInChildren<PlacementSurface>(true);
            InventoryPlacementZone[] shelfZones = authoritativeShelf
                .GetComponentsInChildren<InventoryPlacementZone>(true);
            Assert.That(shelfSurfaces.Length, Is.EqualTo(1));
            Assert.That(shelfZones.Length, Is.EqualTo(1));
            Assert.That(
                shelfSurfaces[0].SurfaceId,
                Is.EqualTo("prototype.retail-shelf-a"));
            Assert.That(shelfZones[0].PlacementSurface, Is.SameAs(shelfSurfaces[0]));
            Assert.That(
                shelfZones[0].ContainerId.Value,
                Is.EqualTo(GarageStockFlowSession.ShelfContainerIdValue));
            Assert.That(
                transforms.Select(transform =>
                        transform.GetComponent<InventoryPlacementZone>())
                    .Count(zone =>
                        zone != null &&
                        zone.ContainerId.Value ==
                            GarageStockFlowSession.ShelfContainerIdValue),
                Is.EqualTo(1));
            Assert.That(
                transforms.Count(transform =>
                    transform.name == "CheckoutPlayerTerminal"),
                Is.EqualTo(1));

            TextMesh shelfLabel = transforms.Single(transform =>
                    transform.name == "RetailShelfLabel")
                .GetComponent<TextMesh>();
            TextMesh checkoutText = transforms.Single(transform =>
                    transform.name == "CheckoutStationStatusText")
                .GetComponent<TextMesh>();
            TextMesh customerFlowText = transforms.Single(transform =>
                    transform.name == "CustomerFlowStatusText")
                .GetComponent<TextMesh>();
            TextMesh customerIdentityText = transforms.Single(transform =>
                    transform.name == "CustomerIdentityText")
                .GetComponent<TextMesh>();
            Assert.That(shelfLabel.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(shelfLabel.characterSize,
                Is.EqualTo(0.014f).Within(0.0001f));
            Assert.That(checkoutText.characterSize,
                Is.EqualTo(0.015f).Within(0.0001f));
            Assert.That(customerFlowText.characterSize,
                Is.EqualTo(0.015f).Within(0.0001f));
            Assert.That(customerIdentityText.characterSize,
                Is.EqualTo(0.018f).Within(0.0001f));

            Assert.That(marker.StockFlow, Is.Not.Null);
            Assert.That(marker.StockFlow.Session, Is.Not.Null);
            Assert.That(marker.CustomerFlow, Is.Not.Null);
            Assert.That(marker.CheckoutStation, Is.Not.Null);
            Assert.That(marker.StockFlow.Session.RetailOffers.Count, Is.EqualTo(0));
            Assert.That(marker.StockFlow.Session.RetailBaskets.Count, Is.EqualTo(0));
            Assert.That(marker.StockFlow.Session.RetailCheckouts.Count, Is.EqualTo(0));
            Assert.That(
                marker.StockFlow.Session.CheckoutSettlements.SettlementCount,
                Is.EqualTo(0));
            Assert.That(
                marker.StockFlow.Session.CheckoutSettlements.TransactionCount,
                Is.EqualTo(0));
            Assert.That(marker.StockFlow.Session.ValidateInvariants().IsSuccess, Is.True);

            Assert.That(
                Object.FindObjectsByType<MeshRenderer>(
                    FindObjectsSortMode.None).Length,
                Is.EqualTo(464));
            Assert.That(
                SceneManager.GetActiveScene().GetRootGameObjects()
                    .SelectMany(root =>
                        root.GetComponentsInChildren<MeshRenderer>(true))
                    .Count(),
                Is.EqualTo(488));
            Assert.That(
                Object.FindObjectsByType<Light>(
                    FindObjectsSortMode.None).Length,
                Is.EqualTo(5));
            Assert.That(
                Object.FindObjectsByType<Camera>(
                    FindObjectsSortMode.None).Length,
                Is.EqualTo(1));

            Light retailLight = Object.FindObjectsByType<Light>(
                    FindObjectsSortMode.None)
                .Single(light => light.name == "RetailCheckoutFillLight");
            Assert.That(retailLight.type, Is.EqualTo(LightType.Spot));
            Assert.That(retailLight.intensity,
                Is.EqualTo(0.42f).Within(0.0001f));
            Assert.That(retailLight.shadows, Is.EqualTo(LightShadows.None));
            Assert.That(
                GaragePrototypeMarker
                    .RetailCheckoutHeroReadabilitySmokeSuccessMarker,
                Does.Contain("states=customer-approach+shelf-offer-basket+")
                    .And.Contain("checkout-payment-receipt")
                    .And.Contain("shelf-authority=single")
                    .And.Contain("legacy-starter-shelf=absent")
                    .And.Contain("screenshots=3")
                    .And.Contain("world-text=preserved")
                    .And.Contain("human=false"));
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
                renderer.sharedMaterial.name.StartsWith(
                    materialPrefix,
                    StringComparison.Ordinal),
                Is.True);
        }
    }
}
