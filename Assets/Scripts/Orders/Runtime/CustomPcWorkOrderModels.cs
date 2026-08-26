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

    /// <summary>
    /// Append-only physical custody progress for the reserved custom-PC component kit.
    /// These states do not mean that a component has been installed in the chassis.
    /// </summary>
    public enum CustomPcBuildKitStage
    {
        MotherboardInHands = 1,
        MotherboardStaged = 2,
        ProcessorInHands = 3,
        ProcessorStaged = 4,
        MemoryModuleInHands = 5,
        MemoryModuleStaged = 6,
        StorageInHands = 7,
        StorageStaged = 8,
        ProcessorCoolerInHands = 9,
        ProcessorCoolerStaged = 10,
        GraphicsCardInHands = 11,
        GraphicsCardStaged = 12,
        PowerSupplyInHands = 13,
        PowerSupplyStaged = 14,
        Atx24PowerCableInHands = 15,
        Atx24PowerCableStaged = 16,
        Eps12vPowerCableInHands = 17,
        Eps12vPowerCableStaged = 18,
        PcieGpuPowerCableInHands = 19,
        PcieGpuPowerCableStaged = 20
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

    public sealed class CustomPcBuildKitReceipt
    {
        internal CustomPcBuildKitReceipt(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord buildOrder,
            CustomPcBuildOrderLineSnapshot line,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId,
            CustomPcBuildKitStage stage,
            long inventoryAppliedRevision)
        {
            OperationId = operationId;
            BuildOrder = buildOrder;
            Line = line;
            SourceContainerId = sourceContainerId;
            HandsContainerId = handsContainerId;
            BuildKitContainerId = buildKitContainerId;
            Stage = stage;
            InventoryAppliedRevision = inventoryAppliedRevision;
        }

        public StableId<CustomPcBuildKitOperationIdScope> OperationId { get; }

        public CustomPcBuildOrderRecord BuildOrder { get; }

        public CustomPcBuildOrderLineSnapshot Line { get; }

        public StableId<ContainerIdScope> SourceContainerId { get; }

        public StableId<ContainerIdScope> HandsContainerId { get; }

        public StableId<ContainerIdScope> BuildKitContainerId { get; }

        public CustomPcBuildKitStage Stage { get; }

        public long InventoryAppliedRevision { get; }

        public int StagedComponentCount =>
            Stage == CustomPcBuildKitStage.MotherboardStaged ||
            Stage == CustomPcBuildKitStage.ProcessorStaged ||
            Stage == CustomPcBuildKitStage.MemoryModuleStaged ||
            Stage == CustomPcBuildKitStage.StorageStaged ||
            Stage == CustomPcBuildKitStage.ProcessorCoolerStaged ||
            Stage == CustomPcBuildKitStage.GraphicsCardStaged ||
            Stage == CustomPcBuildKitStage.PowerSupplyStaged ||
            Stage == CustomPcBuildKitStage.Atx24PowerCableStaged ||
            Stage == CustomPcBuildKitStage.Eps12vPowerCableStaged ||
            Stage == CustomPcBuildKitStage.PcieGpuPowerCableStaged
                ? 1
                : 0;
    }

    /// <summary>
    /// Immutable proof that one canonical, fully staged component was released from the
    /// reserved BuildKit into its exact existing Assembly-owned target. The original ten
    /// staging receipts remain append-only history and the work-order reservation stays live.
    /// </summary>
    public sealed class CustomPcBuildKitAssemblyHandoffReceipt
    {
        internal CustomPcBuildKitAssemblyHandoffReceipt(
            StableId<CustomPcBuildKitAssemblyOperationIdScope> operationId,
            CustomPcBuildOrderRecord buildOrder,
            CustomPcBuildOrderLineSnapshot line,
            CustomPcBuildKitReceipt stagingReceipt,
            StableId<ContainerIdScope> buildKitContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> workbenchContainerId,
            long inventoryAppliedRevision)
        {
            OperationId = operationId;
            BuildOrder = buildOrder;
            Line = line;
            StagingReceipt = stagingReceipt;
            BuildKitContainerId = buildKitContainerId;
            HandsContainerId = handsContainerId;
            WorkbenchContainerId = workbenchContainerId;
            InventoryAppliedRevision = inventoryAppliedRevision;
        }

        public StableId<CustomPcBuildKitAssemblyOperationIdScope> OperationId { get; }

        public CustomPcBuildOrderRecord BuildOrder { get; }

        public CustomPcBuildOrderLineSnapshot Line { get; }

        public PcComponentKind ComponentKind => Line?.ComponentKind ?? default;

        public CustomPcBuildKitReceipt StagingReceipt { get; }

        public StableId<ContainerIdScope> BuildKitContainerId { get; }

        public StableId<ContainerIdScope> HandsContainerId { get; }

        public StableId<ContainerIdScope> WorkbenchContainerId { get; }

        public long InventoryAppliedRevision { get; }
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
        public static readonly Failure BuildKitAuthorityMissing =
            Failure.FromCode("orders.custom-pc-build-kit.authority-missing");
        public static readonly Failure BuildKitContainerInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.container-invalid");
        public static readonly Failure BuildKitOperationInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.operation-invalid");
        public static readonly Failure BuildKitWorkOrderInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.work-order-invalid");
        public static readonly Failure BuildKitMotherboardLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.motherboard-line-invalid");
        public static readonly Failure BuildKitProcessorLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.processor-line-invalid");
        public static readonly Failure BuildKitMemoryModuleLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.memory-module-line-invalid");
        public static readonly Failure BuildKitStorageLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.storage-line-invalid");
        public static readonly Failure BuildKitProcessorCoolerLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.processor-cooler-line-invalid");
        public static readonly Failure BuildKitGraphicsCardLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.graphics-card-line-invalid");
        public static readonly Failure BuildKitPowerSupplyLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.power-supply-line-invalid");
        public static readonly Failure BuildKitAtx24PowerCableLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.atx24-power-cable-line-invalid");
        public static readonly Failure BuildKitEps12vPowerCableLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.eps12v-power-cable-line-invalid");
        public static readonly Failure BuildKitPcieGpuPowerCableLineInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.pcie-gpu-power-cable-line-invalid");
        public static readonly Failure BuildKitPrerequisiteMissing =
            Failure.FromCode("orders.custom-pc-build-kit.prerequisite-missing");
        public static readonly Failure BuildKitIdentityConflict =
            Failure.FromCode("orders.custom-pc-build-kit.identity-conflict");
        public static readonly Failure BuildKitStageInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.stage-invalid");
        public static readonly Failure BuildKitReceiptInvalid =
            Failure.FromCode("orders.custom-pc-build-kit.receipt-invalid");
        public static readonly Failure BuildKitRevisionStale =
            Failure.FromCode("orders.custom-pc-build-kit.revision-stale");
        public static readonly Failure BuildKitAssemblyOperationInvalid =
            Failure.FromCode("orders.custom-pc-build-kit-assembly.operation-invalid");
        public static readonly Failure BuildKitAssemblyIdentityConflict =
            Failure.FromCode("orders.custom-pc-build-kit-assembly.identity-conflict");
        public static readonly Failure BuildKitAssemblyStageInvalid =
            Failure.FromCode("orders.custom-pc-build-kit-assembly.stage-invalid");
        public static readonly Failure BuildKitAssemblyWorkbenchInvalid =
            Failure.FromCode("orders.custom-pc-build-kit-assembly.workbench-invalid");
    }
}
