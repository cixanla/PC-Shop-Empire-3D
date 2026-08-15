using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using UnityEngine;

namespace PCShopEmpire3D.Presentation.Interaction
{
    [DisallowMultipleComponent]
    public sealed class GarageStockFlowRuntime : MonoBehaviour
    {
        [SerializeField] private InventoryItemWorldBinding itemBinding;
        [SerializeField] private TextMesh worldStatusText;
        [SerializeField] private Renderer statusIndicator;
        [SerializeField] private Material arrivedMaterial;
        [SerializeField] private Material acceptedMaterial;
        [SerializeField] private Material shelvedMaterial;

        public GarageStockFlowSession Session { get; private set; }

        public InventoryItemWorldBinding ItemBinding => itemBinding;

        public string StatusText
        {
            get
            {
                GarageStockFlowSession session = EnsureInitialized();
                string order = session.Order.Status == PurchaseOrderStatus.Arrived
                    ? "GELDİ • KABUL BEKLİYOR"
                    : "KABUL EDİLDİ";
                return $"SİPARİŞ: {order}\nÜRÜN: {itemBinding?.LocationLabel ?? "PROJECTION EKSİK"}";
            }
        }

        public void Configure(
            InventoryItemWorldBinding binding,
            TextMesh statusTextMesh,
            Renderer indicator,
            Material waitingMaterial,
            Material receivingMaterial,
            Material shelfMaterial)
        {
            itemBinding = binding != null
                ? binding
                : throw new System.ArgumentNullException(nameof(binding));
            worldStatusText = statusTextMesh;
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
    }
}
