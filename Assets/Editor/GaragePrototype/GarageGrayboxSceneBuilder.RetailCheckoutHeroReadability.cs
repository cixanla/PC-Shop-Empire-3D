using PCShopEmpire3D.Presentation.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Editor.GaragePrototype
{
    public static partial class GarageGrayboxSceneBuilder
    {
        private readonly struct RetailCheckoutHeroBuildResult
        {
            public RetailCheckoutHeroBuildResult(
                RetailCheckoutHeroProjection projection,
                GameObject shelfOfferVisual,
                GameObject basketReservedVisual,
                GameObject cashCheckoutVisual,
                GameObject receiptVisual)
            {
                Projection = projection;
                ShelfOfferVisual = shelfOfferVisual;
                BasketReservedVisual = basketReservedVisual;
                CashCheckoutVisual = cashCheckoutVisual;
                ReceiptVisual = receiptVisual;
            }

            public RetailCheckoutHeroProjection Projection { get; }

            public GameObject ShelfOfferVisual { get; }

            public GameObject BasketReservedVisual { get; }

            public GameObject CashCheckoutVisual { get; }

            public GameObject ReceiptVisual { get; }
        }

        private static RetailCheckoutHeroBuildResult
            BuildRetailCheckoutHeroReadability(
                Transform environment,
                Transform lighting,
                Material darkMetal,
                Material brushedSteel,
                Material accent,
                Material labelPaper,
                Material rubber)
        {
            Transform heroRoot = new GameObject(
                "RetailCheckoutHeroReadability").transform;
            heroRoot.SetParent(environment, false);

            CreateCombinedBoxDetails(
                "RetailCheckoutHeroDarkMetalDetails",
                heroRoot,
                new[]
                {
                    new Vector3(3.90f, 1.94f, 0.48f),
                    new Vector3(0.65f, 0.52f, 2.728f),
                    new Vector3(-0.02f, 1.12f, 3.08f),
                    new Vector3(2.10f, 2.94f, 2.10f)
                },
                new[]
                {
                    new Vector3(0.12f, 0.16f, 1.36f),
                    new Vector3(1.38f, 0.72f, 0.018f),
                    new Vector3(0.30f, 0.18f, 0.26f),
                    new Vector3(1.35f, 0.08f, 0.32f)
                },
                darkMetal);

            CreateCombinedBoxDetails(
                "RetailCheckoutHeroBrushedSteelDetails",
                heroRoot,
                new[]
                {
                    new Vector3(3.04f, 0.96f, 0.48f),
                    new Vector3(0.89f, 1.11f, 3.05f),
                    new Vector3(1.41f, 1.11f, 3.05f),
                    new Vector3(1.15f, 1.11f, 2.81f),
                    new Vector3(1.15f, 1.11f, 3.29f)
                },
                new[]
                {
                    new Vector3(0.025f, 0.18f, 0.72f),
                    new Vector3(0.025f, 0.12f, 0.48f),
                    new Vector3(0.025f, 0.12f, 0.48f),
                    new Vector3(0.50f, 0.12f, 0.025f),
                    new Vector3(0.50f, 0.12f, 0.025f)
                },
                brushedSteel);

            CreateCombinedBoxDetails(
                "RetailCheckoutHeroSafetyAccentDetails",
                heroRoot,
                new[]
                {
                    new Vector3(3.80f, 1.47f, 0.48f),
                    new Vector3(0.65f, 0.88f, 2.716f),
                    new Vector3(-0.15f, 0.014f, -3.60f),
                    new Vector3(0.45f, 0.014f, -2.85f),
                    new Vector3(1.05f, 0.014f, -2.10f),
                    new Vector3(1.65f, 0.014f, -1.30f),
                    new Vector3(2.10f, 0.014f, -0.55f),
                    new Vector3(2.35f, 0.014f, 0.55f),
                    new Vector3(2.35f, 0.014f, 1.35f),
                    new Vector3(2.10f, 0.014f, 1.85f),
                    new Vector3(1.85f, 0.014f, 2.30f),
                    new Vector3(1.85f, 0.014f, 2.45f)
                },
                new[]
                {
                    new Vector3(0.015f, 0.045f, 0.52f),
                    new Vector3(1.28f, 0.035f, 0.010f),
                    new Vector3(0.26f, 0.008f, 0.09f),
                    new Vector3(0.26f, 0.008f, 0.09f),
                    new Vector3(0.26f, 0.008f, 0.09f),
                    new Vector3(0.26f, 0.008f, 0.09f),
                    new Vector3(0.26f, 0.008f, 0.09f),
                    new Vector3(0.42f, 0.008f, 0.10f),
                    new Vector3(0.22f, 0.008f, 0.08f),
                    new Vector3(0.22f, 0.008f, 0.08f),
                    new Vector3(0.22f, 0.008f, 0.08f),
                    new Vector3(0.42f, 0.008f, 0.08f)
                },
                accent);

            CreateCombinedBoxDetails(
                "RetailCheckoutHeroRubberDetails",
                heroRoot,
                new[]
                {
                    new Vector3(3.84f, 1.20f, 0.48f),
                    new Vector3(1.15f, 1.057f, 3.05f),
                    new Vector3(-0.02f, 1.055f, 2.92f)
                },
                new[]
                {
                    new Vector3(0.018f, 0.58f, 0.52f),
                    new Vector3(0.52f, 0.014f, 0.48f),
                    new Vector3(0.34f, 0.020f, 0.22f)
                },
                rubber);

            CreateCombinedBoxDetails(
                "RetailCheckoutLightDiffuser",
                heroRoot,
                new[] { new Vector3(2.10f, 2.885f, 2.10f) },
                new[] { new Vector3(1.12f, 0.020f, 0.22f) },
                labelPaper);

            GameObject shelfOfferVisual = CreateCombinedBoxDetails(
                "RetailShelfOfferStateVisual",
                heroRoot,
                new[] { new Vector3(3.010f, 0.96f, 0.48f) },
                new[] { new Vector3(0.008f, 0.14f, 0.62f) },
                labelPaper);
            GameObject basketReservedVisual = CreateCombinedBoxDetails(
                "RetailBasketReservedStateVisual",
                heroRoot,
                new[] { new Vector3(1.15f, 1.071f, 3.05f) },
                new[] { new Vector3(0.36f, 0.012f, 0.32f) },
                accent);
            GameObject cashCheckoutVisual = CreateCombinedBoxDetails(
                "CheckoutCashStateVisual",
                heroRoot,
                new[] { new Vector3(0.65f, 1.07f, 2.700f) },
                new[] { new Vector3(0.36f, 0.08f, 0.010f) },
                labelPaper);
            GameObject receiptVisual = CreateCombinedBoxDetails(
                "CheckoutReceiptStateVisual",
                heroRoot,
                new[] { new Vector3(-0.02f, 1.22f, 2.98f) },
                new[] { new Vector3(0.20f, 0.008f, 0.16f) },
                labelPaper);

            CreateReadabilityAnchor(
                "RetailCustomerApproachAnchor",
                heroRoot,
                new Vector3(1.05f, 0f, -2.10f));
            CreateReadabilityAnchor(
                "RetailShelfOfferDisplayAnchor",
                heroRoot,
                new Vector3(3.42f, 1.10f, 0.48f));
            CreateReadabilityAnchor(
                "RetailBasketPresentationAnchor",
                heroRoot,
                new Vector3(1.15f, 1.05f, 3.05f));
            CreateReadabilityAnchor(
                "RetailCheckoutPaymentAnchor",
                heroRoot,
                new Vector3(0.65f, 1.34f, 2.68f));
            CreateReadabilityAnchor(
                "RetailCheckoutReceiptAnchor",
                heroRoot,
                new Vector3(-0.02f, 1.18f, 3.02f));

            int ignoreRaycastLayer = RequireLayer("Ignore Raycast");
            foreach (Renderer renderer in heroRoot.GetComponentsInChildren<Renderer>(true))
            {
                renderer.gameObject.layer = ignoreRaycastLayer;
                DisableDecorativeRendererCost(renderer);
            }

            RetailCheckoutHeroProjection projection =
                heroRoot.gameObject.AddComponent<RetailCheckoutHeroProjection>();
            CreateRetailCheckoutFillLight(lighting);
            return new RetailCheckoutHeroBuildResult(
                projection,
                shelfOfferVisual,
                basketReservedVisual,
                cashCheckoutVisual,
                receiptVisual);
        }

        private static void CreateReadabilityAnchor(
            string name,
            Transform parent,
            Vector3 position)
        {
            Transform anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = position;
        }

        private static void CreateRetailCheckoutFillLight(Transform lighting)
        {
            Vector3 position = new Vector3(2.10f, 2.78f, 2.10f);
            Vector3 target = new Vector3(1.94f, 0.92f, 1.77f);
            GameObject lightObject = new GameObject("RetailCheckoutFillLight");
            lightObject.transform.SetParent(lighting, false);
            lightObject.transform.localPosition = position;
            lightObject.transform.localRotation = Quaternion.LookRotation(target - position);

            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Spot;
            light.color = new Color(1f, 0.90f, 0.82f);
            light.intensity = 0.42f;
            light.range = 4.40f;
            light.spotAngle = 110f;
            light.innerSpotAngle = 68.2f;
            light.shadows = LightShadows.None;
        }
    }
}
