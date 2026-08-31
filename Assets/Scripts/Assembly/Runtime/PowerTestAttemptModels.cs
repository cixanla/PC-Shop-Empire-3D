using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    public sealed class PowerTestAttemptOperationIdScope : IStableIdScope
    {
    }

    public enum PowerTestAttemptKind
    {
        PreflightReady = 1
    }

    /// <summary>
    /// Immutable copy boundary for one exact electrical-readiness and PSU-budget result.
    /// It proves no energization, electrical fault detection, POST or benchmark outcome.
    /// </summary>
    public sealed class PowerTestAttemptContext
    {
        private PowerTestAttemptContext(PcPowerBudgetSnapshot powerBudget)
        {
            PowerBudget = powerBudget;
        }

        public PcPowerBudgetSnapshot PowerBudget { get; }

        public ElectricalReadinessSnapshot ElectricalReadiness =>
            PowerBudget.ElectricalReadiness;

        public StableId<PcBuildIdScope> BuildId => ElectricalReadiness.BuildId;

        public StableId<ChassisIdScope> ChassisId => ElectricalReadiness.ChassisId;

        public StableId<PcPowerBudgetPolicyIdScope> PolicyId => PowerBudget.PolicyId;

        public int SystemPowerDrawWatts => PowerBudget.SystemPowerDrawWatts;

        public int MinimumRecommendedPsuWatts =>
            PowerBudget.MinimumRecommendedPsuWatts;

        public int InstalledPsuWatts => PowerBudget.InstalledPsuWatts;

        public int CapacityMarginWatts => PowerBudget.CapacityMarginWatts;

        public bool IsSufficient => PowerBudget.IsSufficient;

        public static OperationResult<PowerTestAttemptContext> Capture(
            PcPowerBudgetSnapshot powerBudget)
        {
            var context = powerBudget == null
                ? null
                : new PowerTestAttemptContext(powerBudget);
            return context != null && context.IsValid
                ? OperationResult<PowerTestAttemptContext>.Success(context)
                : OperationResult<PowerTestAttemptContext>.Fail(
                    PowerTestAttemptFailures.ContextInvalid);
        }

        public bool Matches(PowerTestAttemptContext other)
        {
            return other != null && Matches(other.PowerBudget);
        }

        public bool Matches(PcPowerBudgetSnapshot other)
        {
            if (other == null || other.ElectricalReadiness == null)
            {
                return false;
            }

            ElectricalReadinessSnapshot first = ElectricalReadiness;
            ElectricalReadinessSnapshot second = other.ElectricalReadiness;
            return first.BuildId == second.BuildId &&
                   first.ChassisId == second.ChassisId &&
                   first.MotherboardItemId == second.MotherboardItemId &&
                   first.ProcessorItemId == second.ProcessorItemId &&
                   first.MemoryItemId == second.MemoryItemId &&
                   first.StorageItemId == second.StorageItemId &&
                   first.ProcessorCoolerItemId == second.ProcessorCoolerItemId &&
                   first.GraphicsCardItemId == second.GraphicsCardItemId &&
                   first.PowerSupplyItemId == second.PowerSupplyItemId &&
                   first.Atx24PowerCableItemId == second.Atx24PowerCableItemId &&
                   first.Eps12vPowerCableItemId == second.Eps12vPowerCableItemId &&
                   first.PcieGpuPowerCableItemId == second.PcieGpuPowerCableItemId &&
                   first.MotherboardSecureOperationId ==
                       second.MotherboardSecureOperationId &&
                   first.ProcessorRetainOperationId ==
                       second.ProcessorRetainOperationId &&
                   first.MemoryRetainOperationId == second.MemoryRetainOperationId &&
                   first.StorageSecureOperationId == second.StorageSecureOperationId &&
                   first.ProcessorCoolerRetainOperationId ==
                       second.ProcessorCoolerRetainOperationId &&
                   first.GraphicsCardRetainOperationId ==
                       second.GraphicsCardRetainOperationId &&
                   first.PowerSupplyRetainOperationId ==
                       second.PowerSupplyRetainOperationId &&
                   first.Atx24RouteOperationId == second.Atx24RouteOperationId &&
                   first.Eps12vRouteOperationId == second.Eps12vRouteOperationId &&
                   first.PcieGpuRouteOperationId == second.PcieGpuRouteOperationId &&
                   first.AssemblyRevision == second.AssemblyRevision &&
                   first.Atx24PowerCableRevision == second.Atx24PowerCableRevision &&
                   first.Eps12vPowerCableRevision == second.Eps12vPowerCableRevision &&
                   first.PcieGpuPowerCableRevision == second.PcieGpuPowerCableRevision &&
                   PowerBudget.PolicyId == other.PolicyId &&
                   PowerBudget.MotherboardProductId == other.MotherboardProductId &&
                   PowerBudget.ProcessorProductId == other.ProcessorProductId &&
                   PowerBudget.MemoryProductId == other.MemoryProductId &&
                   PowerBudget.StorageProductId == other.StorageProductId &&
                   PowerBudget.ProcessorCoolerProductId ==
                       other.ProcessorCoolerProductId &&
                   PowerBudget.GraphicsCardProductId == other.GraphicsCardProductId &&
                   PowerBudget.PowerSupplyProductId == other.PowerSupplyProductId &&
                   PowerBudget.PlatformBaseLoadWatts == other.PlatformBaseLoadWatts &&
                   PowerBudget.ChassisLoadWatts == other.ChassisLoadWatts &&
                   PowerBudget.ProcessorLoadWatts == other.ProcessorLoadWatts &&
                   PowerBudget.MemoryLoadWatts == other.MemoryLoadWatts &&
                   PowerBudget.StorageLoadWatts == other.StorageLoadWatts &&
                   PowerBudget.ProcessorCoolerLoadWatts ==
                       other.ProcessorCoolerLoadWatts &&
                   PowerBudget.GraphicsCardLoadWatts ==
                       other.GraphicsCardLoadWatts &&
                   PowerBudget.SystemPowerDrawWatts == other.SystemPowerDrawWatts &&
                   PowerBudget.MinimumRecommendedPsuWatts ==
                       other.MinimumRecommendedPsuWatts &&
                   PowerBudget.InstalledPsuWatts == other.InstalledPsuWatts &&
                   PowerBudget.CapacityMarginWatts == other.CapacityMarginWatts &&
                   PowerBudget.Blocker == other.Blocker;
        }

        internal bool IsValid
        {
            get
            {
                ElectricalReadinessSnapshot readiness =
                    PowerBudget?.ElectricalReadiness;
                if (readiness == null || readiness.BuildId.IsEmpty ||
                    readiness.ChassisId.IsEmpty ||
                    readiness.MotherboardItemId.IsEmpty ||
                    readiness.ProcessorItemId.IsEmpty ||
                    readiness.MemoryItemId.IsEmpty ||
                    readiness.StorageItemId.IsEmpty ||
                    readiness.ProcessorCoolerItemId.IsEmpty ||
                    readiness.GraphicsCardItemId.IsEmpty ||
                    readiness.PowerSupplyItemId.IsEmpty ||
                    readiness.Atx24PowerCableItemId.IsEmpty ||
                    readiness.Eps12vPowerCableItemId.IsEmpty ||
                    readiness.PcieGpuPowerCableItemId.IsEmpty ||
                    readiness.MotherboardSecureOperationId.IsEmpty ||
                    readiness.ProcessorRetainOperationId.IsEmpty ||
                    readiness.MemoryRetainOperationId.IsEmpty ||
                    readiness.StorageSecureOperationId.IsEmpty ||
                    readiness.ProcessorCoolerRetainOperationId.IsEmpty ||
                    readiness.GraphicsCardRetainOperationId.IsEmpty ||
                    readiness.PowerSupplyRetainOperationId.IsEmpty ||
                    readiness.Atx24RouteOperationId.IsEmpty ||
                    readiness.Eps12vRouteOperationId.IsEmpty ||
                    readiness.PcieGpuRouteOperationId.IsEmpty ||
                    readiness.AssemblyRevision <= 0 ||
                    readiness.Atx24PowerCableRevision <= 0 ||
                    readiness.Eps12vPowerCableRevision <= 0 ||
                    readiness.PcieGpuPowerCableRevision <= 0 ||
                    PowerBudget.PolicyId.IsEmpty ||
                    PowerBudget.MotherboardProductId.IsEmpty ||
                    PowerBudget.ProcessorProductId.IsEmpty ||
                    PowerBudget.MemoryProductId.IsEmpty ||
                    PowerBudget.StorageProductId.IsEmpty ||
                    PowerBudget.ProcessorCoolerProductId.IsEmpty ||
                    PowerBudget.GraphicsCardProductId.IsEmpty ||
                    PowerBudget.PowerSupplyProductId.IsEmpty ||
                    PowerBudget.PlatformBaseLoadWatts < 0 ||
                    PowerBudget.ChassisLoadWatts < 0 ||
                    PowerBudget.ProcessorLoadWatts <= 0 ||
                    PowerBudget.MemoryLoadWatts <= 0 ||
                    PowerBudget.StorageLoadWatts <= 0 ||
                    PowerBudget.ProcessorCoolerLoadWatts <= 0 ||
                    PowerBudget.GraphicsCardLoadWatts <= 0 ||
                    PowerBudget.SystemPowerDrawWatts <= 0 ||
                    PowerBudget.MinimumRecommendedPsuWatts <= 0 ||
                    PowerBudget.InstalledPsuWatts <= 0)
                {
                    return false;
                }

                long draw = (long)PowerBudget.PlatformBaseLoadWatts +
                            PowerBudget.ChassisLoadWatts +
                            PowerBudget.ProcessorLoadWatts +
                            PowerBudget.MemoryLoadWatts +
                            PowerBudget.StorageLoadWatts +
                            PowerBudget.ProcessorCoolerLoadWatts +
                            PowerBudget.GraphicsCardLoadWatts;
                long margin = (long)PowerBudget.InstalledPsuWatts -
                              PowerBudget.MinimumRecommendedPsuWatts;
                bool blockerMatches = margin >= 0
                    ? PowerBudget.Blocker.IsNone
                    : PowerBudget.Blocker ==
                      PcPowerBudgetFailures.PowerSupplyInsufficient;
                return draw == PowerBudget.SystemPowerDrawWatts &&
                       margin == PowerBudget.CapacityMarginWatts &&
                       blockerMatches;
            }
        }
    }

    public sealed class PowerTestAttemptReceipt
    {
        internal PowerTestAttemptReceipt(
            StableId<PowerTestAttemptOperationIdScope> operationId,
            long expectedRevision,
            long revision,
            PowerTestAttemptContext context)
        {
            OperationId = operationId;
            ExpectedRevision = expectedRevision;
            Revision = revision;
            Context = context;
        }

        public StableId<PowerTestAttemptOperationIdScope> OperationId { get; }

        public PowerTestAttemptKind Kind => PowerTestAttemptKind.PreflightReady;

        public long ExpectedRevision { get; }

        public long Revision { get; }

        public PowerTestAttemptContext Context { get; }

        internal bool MatchesCommand(
            StableId<PowerTestAttemptOperationIdScope> operationId,
            long expectedRevision,
            PowerTestAttemptContext expectedContext)
        {
            return OperationId == operationId &&
                   ExpectedRevision == expectedRevision &&
                   Context != null &&
                   Context.Matches(expectedContext);
        }
    }

    public static class PowerTestAttemptFailures
    {
        public static readonly Failure ConfigurationMissing = Failure.FromCode(
            "assembly.power-test-attempt.configuration-missing");
        public static readonly Failure AuthorityMismatch = Failure.FromCode(
            "assembly.power-test-attempt.authority-mismatch");
        public static readonly Failure InvalidOperationId = Failure.FromCode(
            "assembly.power-test-attempt.operation-id-invalid");
        public static readonly Failure ContextInvalid = Failure.FromCode(
            "assembly.power-test-attempt.context-invalid");
        public static readonly Failure ContextStale = Failure.FromCode(
            "assembly.power-test-attempt.context-stale");
        public static readonly Failure RevisionMismatch = Failure.FromCode(
            "assembly.power-test-attempt.revision-mismatch");
        public static readonly Failure OperationConflict = Failure.FromCode(
            "assembly.power-test-attempt.operation-conflict");
        public static readonly Failure AlreadyCompleted = Failure.FromCode(
            "assembly.power-test-attempt.already-completed");
        public static readonly Failure PowerSupplyInsufficient = Failure.FromCode(
            "assembly.power-test-attempt.power-supply-insufficient");
        public static readonly Failure ReceiptMissing = Failure.FromCode(
            "assembly.power-test-attempt.receipt-missing");
        public static readonly Failure ReceiptHistoryInvalid = Failure.FromCode(
            "assembly.power-test-attempt.receipt-history-invalid");
    }
}
