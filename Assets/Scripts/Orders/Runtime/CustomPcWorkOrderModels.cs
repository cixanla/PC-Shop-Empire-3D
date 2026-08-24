using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Orders
{
    /// <summary>
    /// Append-only persisted state. ReservationSetAllocated means the exact reserved kit is
    /// allocated to the job, not that any component has moved or been installed.
    /// </summary>
    public enum CustomPcBuildOrderStatus
    {
        ReservationSetAllocated = 1
    }

    public enum CustomPcWorkTicketStatus
    {
        PostedAtWorkbenchStation = 1
    }

    public sealed class CustomPcBuildOrderLineSnapshot
    {
        internal CustomPcBuildOrderLineSnapshot(CustomPcQuoteLineSnapshot source)
        {
            LineId = source.LineId;
            ProductId = source.ProductId;
            ItemId = source.ItemId;
            ReservationId = source.ReservationId;
            ComponentKind = source.ComponentKind;
            PowerCableType = source.PowerCableType;
            UnitCost = source.UnitCost;
            UnitPrice = source.UnitPrice;
        }

        public StableId<CustomPcBomLineIdScope> LineId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ReservationIdScope> ReservationId { get; }

        public PcComponentKind ComponentKind { get; }

        public PowerCableType PowerCableType { get; }

        public InventoryUnitCost UnitCost { get; }

        public ShelfPrice UnitPrice { get; }

        internal bool Matches(CustomPcQuoteLineSnapshot source)
        {
            return source != null &&
                   LineId == source.LineId &&
                   ProductId == source.ProductId &&
                   ItemId == source.ItemId &&
                   ReservationId == source.ReservationId &&
                   ComponentKind == source.ComponentKind &&
                   PowerCableType == source.PowerCableType &&
                   UnitCost == source.UnitCost &&
                   UnitPrice == source.UnitPrice;
        }
    }

    public sealed class CustomPcBuildOrderRecord
    {
        internal CustomPcBuildOrderRecord(
            StableId<CustomPcBuildOrderIdScope> id,
            StableId<CustomPcWorkTicketIdScope> workTicketId,
            StableId<CustomPcWorkOrderOperationIdScope> operationId,
            CustomPcQuoteRecord sourceQuote,
            StableId<ContainerIdScope> workbenchContainerId,
            SimulationTimestamp issuedAt,
            IReadOnlyList<CustomPcBuildOrderLineSnapshot> lines,
            long inventoryAllocationRevision)
        {
            Id = id;
            WorkTicketId = workTicketId;
            OperationId = operationId;
            SourceQuote = sourceQuote;
            WorkbenchContainerId = workbenchContainerId;
            IssuedAt = issuedAt;
            Lines = lines;
            InventoryAllocationRevision = inventoryAllocationRevision;
            Status = CustomPcBuildOrderStatus.ReservationSetAllocated;
        }

        public StableId<CustomPcBuildOrderIdScope> Id { get; }

        public StableId<CustomPcWorkTicketIdScope> WorkTicketId { get; }

        public StableId<CustomPcWorkOrderOperationIdScope> OperationId { get; }

        public CustomPcQuoteRecord SourceQuote { get; }

        public StableId<CustomPcQuoteIdScope> SourceQuoteId => SourceQuote.Id;

        public StableId<CustomPcRequestIdScope> SourceRequestId => SourceQuote.Request.Id;

        public StableId<CustomerRetailIdentityBindingIdScope> CustomerBindingId =>
            SourceQuote.Request.CustomerBinding.Id;

        public StableId<InventoryClaimIdScope> InventoryClaimId =>
            SourceQuote.InventoryClaimId;

        public StableId<ContainerIdScope> WorkbenchContainerId { get; }

        public CustomPcBuildOrderStatus Status { get; }

        public SimulationTimestamp IssuedAt { get; }

        public IReadOnlyList<CustomPcBuildOrderLineSnapshot> Lines { get; }

        public long InventoryAllocationRevision { get; }

        public int ReservedSerializedItemCount => Lines?.Count ?? 0;
    }

    public sealed class CustomPcWorkTicketRecord
    {
        internal CustomPcWorkTicketRecord(
            StableId<CustomPcWorkTicketIdScope> id,
            CustomPcBuildOrderRecord buildOrder)
        {
            Id = id;
            BuildOrder = buildOrder;
            Status = CustomPcWorkTicketStatus.PostedAtWorkbenchStation;
        }

        public StableId<CustomPcWorkTicketIdScope> Id { get; }

        public CustomPcBuildOrderRecord BuildOrder { get; }

        public StableId<CustomPcBuildOrderIdScope> BuildOrderId => BuildOrder.Id;

        public StableId<CustomPcQuoteIdScope> SourceQuoteId => BuildOrder.SourceQuoteId;

        public StableId<InventoryClaimIdScope> InventoryClaimId =>
            BuildOrder.InventoryClaimId;

        public StableId<ContainerIdScope> WorkbenchContainerId =>
            BuildOrder.WorkbenchContainerId;

        public CustomPcWorkTicketStatus Status { get; }

        public SimulationTimestamp IssuedAt => BuildOrder.IssuedAt;

        public int ReservedSerializedItemCount => BuildOrder.ReservedSerializedItemCount;
    }

    public sealed class CustomPcWorkOrderIssueResult
    {
        internal CustomPcWorkOrderIssueResult(
            CustomPcBuildOrderRecord buildOrder,
            CustomPcWorkTicketRecord workTicket)
        {
            BuildOrder = buildOrder;
            WorkTicket = workTicket;
        }

        public CustomPcBuildOrderRecord BuildOrder { get; }

        public CustomPcWorkTicketRecord WorkTicket { get; }
    }

    public static class CustomPcWorkOrderFailures
    {
        public static readonly Failure MissingAuthority =
            Failure.FromCode("orders.custom-pc-work-order.authority-missing");
        public static readonly Failure AuthorityMismatch =
            Failure.FromCode("orders.custom-pc-work-order.authority-mismatch");
        public static readonly Failure IssueAccessInvalid =
            Failure.FromCode("orders.custom-pc-work-order.issue-access-invalid");
        public static readonly Failure InputInvalid =
            Failure.FromCode("orders.custom-pc-work-order.input-invalid");
        public static readonly Failure QuoteNotOwned =
            Failure.FromCode("orders.custom-pc-work-order.quote-not-owned");
        public static readonly Failure QuoteReservationDrift =
            Failure.FromCode("orders.custom-pc-work-order.quote-reservation-drift");
        public static readonly Failure WorkbenchInvalid =
            Failure.FromCode("orders.custom-pc-work-order.workbench-invalid");
        public static readonly Failure PublisherAlreadyRegistered =
            Failure.FromCode("orders.custom-pc-work-order.publisher-already-registered");
        public static readonly Failure IdentityConflict =
            Failure.FromCode("orders.custom-pc-work-order.identity-conflict");
        public static readonly Failure TimestampInvalid =
            Failure.FromCode("orders.custom-pc-work-order.timestamp-invalid");
        public static readonly Failure RevisionOverflow =
            Failure.FromCode("orders.custom-pc-work-order.revision-overflow");
        public static readonly Failure InvariantViolation =
            Failure.FromCode("orders.custom-pc-work-order.invariant");
    }
}
