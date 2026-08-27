using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    /// <summary>
    /// Projects canonical retail authority state onto collider-free lookdev visuals. This
    /// component never mutates stock, customer, basket, checkout, payment, or receipt authority.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RetailCheckoutHeroProjection : MonoBehaviour
    {
        [SerializeField] private GarageStockFlowRuntime stockFlow;
        [SerializeField] private GameObject shelfOfferVisual;
        [SerializeField] private GameObject basketReservedVisual;
        [SerializeField] private GameObject cashCheckoutVisual;
        [SerializeField] private GameObject receiptVisual;

        public GarageStockFlowRuntime StockFlow => stockFlow;

        public GameObject ShelfOfferVisual => shelfOfferVisual;

        public GameObject BasketReservedVisual => basketReservedVisual;

        public GameObject CashCheckoutVisual => cashCheckoutVisual;

        public GameObject ReceiptVisual => receiptVisual;

        public void Configure(
            GarageStockFlowRuntime configuredStockFlow,
            GameObject configuredShelfOfferVisual,
            GameObject configuredBasketReservedVisual,
            GameObject configuredCashCheckoutVisual,
            GameObject configuredReceiptVisual)
        {
            stockFlow = configuredStockFlow;
            shelfOfferVisual = configuredShelfOfferVisual;
            basketReservedVisual = configuredBasketReservedVisual;
            cashCheckoutVisual = configuredCashCheckoutVisual;
            receiptVisual = configuredReceiptVisual;
            RefreshPresentation();
        }

        public void RefreshPresentation()
        {
            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.Session
                : null;
            bool hasOffer = session != null && session.TryGetShelfOffer(out _);
            bool hasBasket = session != null &&
                             session.TryGetPrototypeBasketLine(out _);
            bool hasCheckout = session != null &&
                               session.TryGetPrototypeCheckout(out _);
            bool hasReceipt = session != null &&
                              session.TryGetPrototypeCheckoutSettlement(out _);

            SetActive(shelfOfferVisual, hasOffer);
            SetActive(basketReservedVisual, hasBasket && !hasReceipt);
            SetActive(cashCheckoutVisual, hasCheckout && !hasReceipt);
            SetActive(receiptVisual, hasReceipt);
        }

        private void Awake()
        {
            RefreshPresentation();
        }

        private void Update()
        {
            RefreshPresentation();
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
