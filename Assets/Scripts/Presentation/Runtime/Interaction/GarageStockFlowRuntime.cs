using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Retail;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class GarageStockFlowRuntime : MonoBehaviour
    {
        [SerializeField] private InventoryItemWorldBinding itemBinding;
        [SerializeField] private TextMesh worldStatusText;
        [SerializeField] private TextMesh shelfOfferText;
        [SerializeField] private Renderer statusIndicator;
        [SerializeField] private Material arrivedMaterial;
        [SerializeField] private Material acceptedMaterial;
        [SerializeField] private Material shelvedMaterial;

        public GarageStockFlowSession Session { get; private set; }

        public InventoryItemWorldBinding ItemBinding => itemBinding;

        public DeliveryParcelProjection Parcel => itemBinding != null ? itemBinding.Parcel : null;

        public TextMesh ShelfOfferText => shelfOfferText;

        public string ShelfOfferPriceText
        {
            get
            {
                GarageStockFlowSession session = EnsureInitialized();
                return session.TryGetShelfOffer(out ShelfOfferRecord offer)
                    ? FormatPrice(offer.Price)
                    : "FİYAT YOK";
            }
        }

        public string ShelfOfferLabelText => $"RAF A\n{ShelfOfferPriceText}";

        public static string PrototypePriceText => FormatPrice(
            ShelfPrice.Create(
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits).Value);

        public string StatusText
        {
            get
            {
                GarageStockFlowSession session = EnsureInitialized();
                string order = session.Order.Status == PurchaseOrderStatus.Arrived
                    ? "GELDİ • KABUL BEKLİYOR"
                    : "KABUL EDİLDİ";
                string parcel = Parcel != null ? Parcel.StateLabel : "PROJECTION EKSİK";
                return $"SİPARİŞ: {order}\nKOLİ: {parcel}\n" +
                       $"ÜRÜN: {itemBinding?.LocationLabel ?? "PROJECTION EKSİK"}\n" +
                       $"FİYAT: {ShelfOfferPriceText}";
            }
        }

        public void Configure(
            InventoryItemWorldBinding binding,
            TextMesh statusTextMesh,
            TextMesh shelfOfferTextMesh,
            Renderer indicator,
            Material waitingMaterial,
            Material receivingMaterial,
            Material shelfMaterial)
        {
            itemBinding = binding != null
                ? binding
                : throw new System.ArgumentNullException(nameof(binding));
            worldStatusText = statusTextMesh;
            shelfOfferText = shelfOfferTextMesh;
            statusIndicator = indicator;
            arrivedMaterial = waitingMaterial;
            acceptedMaterial = receivingMaterial;
            shelvedMaterial = shelfMaterial;
            itemBinding.Configure(
                this,
                itemBinding.GetComponent<PCShopEmpire3D.World.Interaction.PhysicalItemProjection>(),
                GarageStockFlowSession.ItemInstanceIdValue);
        }

        public GarageStockFlowSession EnsureInitialized()
        {
            Session ??= GarageStockFlowSession.CreateArrived();
            return Session;
        }

        public void RefreshPresentation()
        {
            GarageStockFlowSession session = EnsureInitialized();
            if (worldStatusText != null)
            {
                worldStatusText.text = StatusText;
            }

            if (shelfOfferText != null)
            {
                shelfOfferText.text = ShelfOfferLabelText;
            }

            if (statusIndicator == null)
            {
                return;
            }

            Material target = arrivedMaterial;
            if (session.TryGetItem(out InventoryItemRecord item) &&
                session.Inventory.TryGetContainer(
                    item.ContainerId,
                    out InventoryContainerDefinition container))
            {
                target = container.Kind == InventoryContainerKind.Shelf
                    ? shelvedMaterial
                    : acceptedMaterial;
            }

            if (target != null)
            {
                statusIndicator.sharedMaterial = target;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
            RefreshPresentation();
        }

        public static string FormatPrice(ShelfPrice price)
        {
            long majorUnits = price.MinorUnits / 100;
            long minorUnits = price.MinorUnits % 100;
            return $"{majorUnits},{minorUnits:00} {price.Currency.Value}";
        }
    }
}
