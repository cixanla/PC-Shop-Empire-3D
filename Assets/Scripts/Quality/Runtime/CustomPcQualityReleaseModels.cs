using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Quality
{
    public sealed class CustomPcQualityReleaseOperationIdScope : IStableIdScope
    {
    }

    public enum CustomPcQualityReleaseResult
    {
        ReadyForPackaging = 1
    }

    /// <summary>
    /// Immutable proof that one exact customer work order passed deterministic validation,
    /// was safely powered off from that same validation cycle and remained mechanically
    /// unchanged when a player explicitly released it to the future packaging stage.
    /// </summary>
    public sealed class CustomPcQualityReleaseReceipt
    {
        private readonly CustomPcQualityReleaseAuthority _owner;

        internal CustomPcQualityReleaseReceipt(
            CustomPcQualityReleaseAuthority owner,
            StableId<CustomPcQualityReleaseOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder,
            CustomPcWorkTicketRecord workTicket,
            PcValidationReceipt sourceValidationReceipt,
            PcPowerStateReceipt sourcePowerOffReceipt,
            ElectricalReadinessSnapshot sourceElectricalReadiness,
            long expectedRevision,
            long revision)
        {
            _owner = owner;
            OperationId = operationId;
            WorkOrder = workOrder;
            WorkTicket = workTicket;
            SourceValidationReceipt = sourceValidationReceipt;
            SourcePowerOffReceipt = sourcePowerOffReceipt;
            SourceElectricalReadiness = sourceElectricalReadiness;
            ExpectedRevision = expectedRevision;
            Revision = revision;
        }

        public StableId<CustomPcQualityReleaseOperationIdScope> OperationId { get; }

        public CustomPcQualityReleaseResult Result =>
            CustomPcQualityReleaseResult.ReadyForPackaging;

        public CustomPcBuildOrderRecord WorkOrder { get; }

        public CustomPcWorkTicketRecord WorkTicket { get; }

        public StableId<CustomPcBuildOrderIdScope> WorkOrderId => WorkOrder.Id;

        public StableId<CustomPcWorkTicketIdScope> WorkTicketId => WorkTicket.Id;

        public StableId<CustomPcQuoteIdScope> SourceQuoteId => WorkOrder.SourceQuoteId;

        public StableId<CustomPcRequestIdScope> SourceRequestId =>
            WorkOrder.SourceRequestId;

        public StableId<CustomerRetailIdentityBindingIdScope> CustomerBindingId =>
            WorkOrder.CustomerBindingId;

        public StableId<InventoryClaimIdScope> InventoryClaimId =>
            WorkOrder.InventoryClaimId;

        public StableId<ContainerIdScope> WorkbenchContainerId =>
            WorkOrder.WorkbenchContainerId;

        public PcValidationReceipt SourceValidationReceipt { get; }

        public PcPowerStateReceipt SourcePowerOffReceipt { get; }

        public PcPowerStateReceipt SourcePowerOnReceipt =>
            SourceValidationReceipt?.SourcePowerOnReceipt;

        public ElectricalReadinessSnapshot SourceElectricalReadiness { get; }

        public StableId<PcBuildIdScope> BuildId =>
            SourceElectricalReadiness.BuildId;

        public StableId<ChassisIdScope> ChassisId =>
            SourceElectricalReadiness.ChassisId;

        public StableId<PcPerformanceCatalogIdScope> PerformanceCatalogId =>
            SourceValidationReceipt.PerformanceCatalogId;

        public StableId<PcValidationProfileIdScope> ValidationProfileId =>
            SourceValidationReceipt.ProfileId;

        public PcQualityTier QualityTier => SourceValidationReceipt.QualityTier;

        public int BenchmarkScore => SourceValidationReceipt.BenchmarkScore;

        public int StressSteps => SourceValidationReceipt.StressSteps;

        public int ProcessorPeakTemperatureCelsius =>
            SourceValidationReceipt.ProcessorPeakTemperatureCelsius;

        public int GraphicsCardPeakTemperatureCelsius =>
            SourceValidationReceipt.GraphicsCardPeakTemperatureCelsius;

        public int SystemPowerDrawWatts =>
            SourceValidationReceipt.SystemPowerDrawWatts;

        public int PowerMarginWatts => SourceValidationReceipt.PowerMarginWatts;

        public long PowerOffRevision => SourcePowerOffReceipt.Revision;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        internal bool IsOwnedBy(CustomPcQualityReleaseAuthority owner)
        {
            return owner != null && ReferenceEquals(_owner, owner);
        }

        internal bool MatchesCommand(
            StableId<CustomPcQualityReleaseOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder,
            CustomPcWorkTicketRecord workTicket,
            PcValidationReceipt sourceValidationReceipt,
            PcPowerStateReceipt sourcePowerOffReceipt,
            long expectedRevision)
        {
            return OperationId == operationId &&
                   ReferenceEquals(WorkOrder, workOrder) &&
                   ReferenceEquals(WorkTicket, workTicket) &&
                   ReferenceEquals(SourceValidationReceipt, sourceValidationReceipt) &&
                   ReferenceEquals(SourcePowerOffReceipt, sourcePowerOffReceipt) &&
                   ExpectedRevision == expectedRevision;
        }
    }

    public static class CustomPcQualityReleaseFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "quality.custom-pc-release.configuration-missing");
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "quality.custom-pc-release.operation-id-invalid");
        public static readonly Failure InvalidWorkOrder = Failure.FromCode(
            "quality.custom-pc-release.work-order-invalid");
        public static readonly Failure InvalidWorkTicket = Failure.FromCode(
            "quality.custom-pc-release.work-ticket-invalid");
        public static readonly Failure WorkOrderLineageMismatch = Failure.FromCode(
            "quality.custom-pc-release.work-order-lineage-mismatch");
        public static readonly Failure ValidationReceiptInvalid = Failure.FromCode(
            "quality.custom-pc-release.validation-receipt-invalid");
        public static readonly Failure ValidationNotPassed = Failure.FromCode(
            "quality.custom-pc-release.validation-not-passed");
        public static readonly Failure SafePowerOffMissing = Failure.FromCode(
            "quality.custom-pc-release.safe-power-off-missing");
        public static readonly Failure AssemblyDrift = Failure.FromCode(
            "quality.custom-pc-release.assembly-drift");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "quality.custom-pc-release.revision-mismatch");
        public static readonly Failure RevisionOverflow = Failure.FromCode(
            "quality.custom-pc-release.revision-overflow");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "quality.custom-pc-release.operation-conflict");
        public static readonly Failure NotCurrent = Failure.FromCode(
            "quality.custom-pc-release.not-current");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "quality.custom-pc-release.receipt-history-invalid");
    }
}
