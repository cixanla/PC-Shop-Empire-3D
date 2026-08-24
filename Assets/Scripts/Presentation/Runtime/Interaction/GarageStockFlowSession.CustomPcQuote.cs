using System.Collections.Generic;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PrototypeCustomPcRequestIdValue =
            "retail.custom-pc-request.demo-gaming-001";
        public const string PrototypeCustomPcQuoteIdValue =
            "retail.custom-pc-quote.demo-gaming-001";
        public const string PrototypeCustomPcClaimIdValue =
            "inventory.claim.custom-pc.demo-gaming-001";
        public const long PrototypeCustomPcMaximumBudgetMinorUnits = 160_000;
        public const long PrototypeCustomPcTotalPriceMinorUnits = 154_299;

        public StableId<CustomPcRequestIdScope> PrototypeCustomPcRequestId =>
            StableId<CustomPcRequestIdScope>.Parse(
                PrototypeCustomPcRequestIdValue);

        public StableId<CustomPcQuoteIdScope> PrototypeCustomPcQuoteId =>
            StableId<CustomPcQuoteIdScope>.Parse(
                PrototypeCustomPcQuoteIdValue);

        public StableId<InventoryClaimIdScope> PrototypeCustomPcClaimId =>
            StableId<InventoryClaimIdScope>.Parse(
                PrototypeCustomPcClaimIdValue);

        public OperationResult AcceptPrototypeCustomPcRequest(
            SimulationTimestamp acceptedAt)
        {
            if (!TryGetPrototypeCustomerConsultation(
                    out CustomerConsultationRecord consultation))
            {
                return OperationResult.Fail(
                    CustomPcQuoteFailures.ConsultationMismatch);
            }

            return CustomPcQuotes.AcceptRequest(
                PrototypeCustomPcRequestId,
                PrototypeCustomerBinding,
                consultation,
                CustomPcBuildProfile.GraphicsFirstGaming,
                ShelfPrice.Create(
                    PrototypeCurrencyCode,
                    PrototypeCustomPcMaximumBudgetMinorUnits).Value,
                acceptedAt);
        }

        public OperationResult CreatePrototypeCustomPcQuote(
            SimulationTimestamp quotedAt)
        {
            return CustomPcQuotes.CreateQuoteAndReserve(
                PrototypeCustomPcQuoteId,
                PrototypeCustomPcRequestId,
                PrototypeCustomPcClaimId,
                CreatePrototypeCustomPcQuoteLines(),
                quotedAt);
        }

        public bool TryGetPrototypeCustomPcRequest(
            out CustomPcRequestRecord request)
        {
            return CustomPcQuotes.TryGetRequest(
                PrototypeCustomPcRequestId,
                out request);
        }

        public bool TryGetPrototypeCustomPcQuote(out CustomPcQuoteRecord quote)
        {
            return CustomPcQuotes.TryGetQuote(
                PrototypeCustomPcQuoteId,
                out quote);
        }

        public IReadOnlyList<CustomPcQuoteLineDraft>
            CreatePrototypeCustomPcQuoteLines()
        {
            return new[]
            {
                QuoteLine(
                    "motherboard",
                    MotherboardProductId,
                    MotherboardItemId,
                    11_900),
                QuoteLine(
                    "processor",
                    ProcessorProductId,
                    ProcessorItemId,
                    32_900),
                QuoteLine(
                    "memory",
                    MemoryProductId,
                    MemoryItemId,
                    11_900),
                QuoteLine(
                    "storage",
                    StorageProductId,
                    StorageItemId,
                    13_900),
                QuoteLine(
                    "processor-cooler",
                    ProcessorCoolerProductId,
                    ProcessorCoolerItemId,
                    9_900),
                QuoteLine(
                    "graphics-card",
                    ProductId,
                    GraphicsCardAssemblyItemId,
                    54_999),
                QuoteLine(
                    "power-supply",
                    PowerSupplyProductId,
                    PowerSupplyItemId,
                    10_900),
                QuoteLine(
                    "atx24-power-cable",
                    Atx24PowerCableProductId,
                    Atx24PowerCableItemId,
                    2_900),
                QuoteLine(
                    "eps12v-power-cable",
                    Eps12vPowerCableProductId,
                    Eps12vPowerCableItemId,
                    2_500),
                QuoteLine(
                    "pcie-gpu-power-cable",
                    PcieGpuPowerCableProductId,
                    PcieGpuPowerCableItemId,
                    2_500)
            };
        }

        private static CustomPcQuoteLineDraft QuoteLine(
            string suffix,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            long unitPriceMinorUnits)
        {
            return CustomPcQuoteLineDraft.Create(
                StableId<CustomPcBomLineIdScope>.Parse(
                    $"retail.custom-pc-bom-line.demo-gaming-001.{suffix}"),
                productId,
                itemId,
                StableId<ReservationIdScope>.Parse(
                    $"inventory.reservation.custom-pc.demo-gaming-001.{suffix}"),
                ShelfPrice.Create(
                    PrototypeCurrencyCode,
                    unitPriceMinorUnits).Value).Value;
        }
    }
}
