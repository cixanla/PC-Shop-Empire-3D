using System;
using System.Collections.Generic;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    /// <summary>
    /// Persisted request profile. Numeric values are explicit because future save data must
    /// never reinterpret an existing accepted customer request.
    /// </summary>
    public enum CustomPcBuildProfile
    {
        GraphicsFirstGaming = 1
    }

    /// <summary>
    /// Immutable accepted request derived from one owned consultation receipt.
    /// </summary>
    public sealed class CustomPcRequestRecord
    {
        internal CustomPcRequestRecord(
            StableId<CustomPcRequestIdScope> id,
            CustomerRetailIdentityBinding customerBinding,
            CustomerConsultationRecord consultation,
            CustomPcBuildProfile profile,
            ShelfPrice maximumBudget,
            SimulationTimestamp acceptedAt)
        {
            Id = id;
            CustomerBinding = customerBinding;
            Consultation = consultation;
            Profile = profile;
            MaximumBudget = maximumBudget;
            AcceptedAt = acceptedAt;
        }

        public StableId<CustomPcRequestIdScope> Id { get; }

        public CustomerRetailIdentityBinding CustomerBinding { get; }

        public CustomerConsultationRecord Consultation { get; }

        public CustomPcBuildProfile Profile { get; }

        public ShelfPrice MaximumBudget { get; }

        public SimulationTimestamp AcceptedAt { get; }

        internal bool Matches(
            CustomerRetailIdentityBinding customerBinding,
            CustomerConsultationRecord consultation,
            CustomPcBuildProfile profile,
            ShelfPrice maximumBudget,
            SimulationTimestamp acceptedAt)
        {
            return CustomerBinding != null &&
                   CustomerBinding.Equals(customerBinding) &&
                   ReferenceEquals(Consultation, consultation) &&
                   Profile == profile &&
                   MaximumBudget == maximumBudget &&
                   AcceptedAt == acceptedAt;
        }
    }

    /// <summary>
    /// Validated quote input. Quantity is deliberately fixed at one because every line must
    /// bind one exact serialized Inventory instance.
    /// </summary>
    public sealed class CustomPcQuoteLineDraft
    {
        private CustomPcQuoteLineDraft(
            StableId<CustomPcBomLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            ShelfPrice unitPrice)
        {
            LineId = lineId;
            ProductId = productId;
            ItemId = itemId;
            ReservationId = reservationId;
            UnitPrice = unitPrice;
        }

        public StableId<CustomPcBomLineIdScope> LineId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ReservationIdScope> ReservationId { get; }

        public ShelfPrice UnitPrice { get; }

        public static OperationResult<CustomPcQuoteLineDraft> Create(
            StableId<CustomPcBomLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            ShelfPrice unitPrice)
        {
            return lineId.IsEmpty ||
                   productId.IsEmpty ||
                   itemId.IsEmpty ||
                   reservationId.IsEmpty ||
                   !CurrencyCode.IsValid(unitPrice.Currency.Value) ||
                   unitPrice.MinorUnits <= 0 ||
                   unitPrice.MinorUnits > ShelfPrice.MaximumMinorUnits
                ? OperationResult<CustomPcQuoteLineDraft>.Fail(
                    CustomPcQuoteFailures.InvalidLine)
                : OperationResult<CustomPcQuoteLineDraft>.Success(
                    new CustomPcQuoteLineDraft(
                        lineId,
                        productId,
                        itemId,
                        reservationId,
                        unitPrice));
        }
    }

    /// <summary>
    /// One immutable BOM line with exact commercial and acquisition provenance.
    /// </summary>
    public sealed class CustomPcQuoteLineSnapshot
    {
        internal CustomPcQuoteLineSnapshot(
            StableId<CustomPcBomLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind,
            PowerCableType powerCableType,
            InventoryUnitCost unitCost,
            ShelfPrice unitPrice)
        {
            LineId = lineId;
            ProductId = productId;
            ItemId = itemId;
            ReservationId = reservationId;
            ComponentKind = componentKind;
            PowerCableType = powerCableType;
            UnitCost = unitCost;
            UnitPrice = unitPrice;
        }

        public StableId<CustomPcBomLineIdScope> LineId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ReservationIdScope> ReservationId { get; }

        public PcComponentKind ComponentKind { get; }

        public PowerCableType PowerCableType { get; }

        public InventoryUnitCost UnitCost { get; }

        public ShelfPrice UnitPrice { get; }

        internal bool HasExactIdentity(CustomPcQuoteLineSnapshot other)
        {
            return other != null &&
                   LineId == other.LineId &&
                   ProductId == other.ProductId &&
                   ItemId == other.ItemId &&
                   ReservationId == other.ReservationId &&
                   ComponentKind == other.ComponentKind &&
                   PowerCableType == other.PowerCableType &&
                   UnitCost == other.UnitCost &&
                   UnitPrice == other.UnitPrice;
        }
    }

    /// <summary>
    /// Immutable accepted quote and BOM. A live record always owns the complete exact
    /// Inventory reservation set identified by InventoryClaimId.
    /// </summary>
    public sealed class CustomPcQuoteRecord
    {
        internal CustomPcQuoteRecord(
            StableId<CustomPcQuoteIdScope> id,
            CustomPcRequestRecord request,
            StableId<InventoryClaimIdScope> inventoryClaimId,
            SimulationTimestamp quotedAt,
            ShelfPrice totalPrice,
            IReadOnlyList<CustomPcQuoteLineSnapshot> lines)
        {
            Id = id;
            Request = request;
            InventoryClaimId = inventoryClaimId;
            QuotedAt = quotedAt;
            TotalPrice = totalPrice;
            Lines = lines;
        }

        public StableId<CustomPcQuoteIdScope> Id { get; }

        public CustomPcRequestRecord Request { get; }

        public StableId<InventoryClaimIdScope> InventoryClaimId { get; }

        public SimulationTimestamp QuotedAt { get; }

        public ShelfPrice TotalPrice { get; }

        public IReadOnlyList<CustomPcQuoteLineSnapshot> Lines { get; }

        public int ReservedSerializedItemCount => Lines?.Count ?? 0;

        internal bool HasExactIdentity(CustomPcQuoteRecord other)
        {
            if (other == null ||
                Id != other.Id ||
                !ReferenceEquals(Request, other.Request) ||
                InventoryClaimId != other.InventoryClaimId ||
                QuotedAt != other.QuotedAt ||
                TotalPrice != other.TotalPrice ||
                Lines == null ||
                other.Lines == null ||
                Lines.Count != other.Lines.Count)
            {
                return false;
            }

            for (int index = 0; index < Lines.Count; index++)
            {
                if (!Lines[index].HasExactIdentity(other.Lines[index]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
