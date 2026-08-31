using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Read-only proof that the canonical ten-part build is mechanically retained and
    /// all three required power-cable families are routed. It is not a power-on, POST,
    /// firmware, operating-system or benchmark-completion receipt.
    /// </summary>
    public sealed class ElectricalReadinessSnapshot
    {
        internal ElectricalReadinessSnapshot(
            StableId<PcBuildIdScope> buildId,
            StableId<ChassisIdScope> chassisId,
            StableId<ItemInstanceIdScope> motherboardItemId,
            StableId<ItemInstanceIdScope> processorItemId,
            StableId<ItemInstanceIdScope> memoryItemId,
            StableId<ItemInstanceIdScope> storageItemId,
            StableId<ItemInstanceIdScope> processorCoolerItemId,
            StableId<ItemInstanceIdScope> graphicsCardItemId,
            StableId<ItemInstanceIdScope> powerSupplyItemId,
            StableId<ItemInstanceIdScope> atx24PowerCableItemId,
            StableId<ItemInstanceIdScope> eps12vPowerCableItemId,
            StableId<ItemInstanceIdScope> pcieGpuPowerCableItemId,
            StableId<AssemblyOperationIdScope> motherboardSecureOperationId,
            StableId<AssemblyOperationIdScope> processorRetainOperationId,
            StableId<AssemblyOperationIdScope> memoryRetainOperationId,
            StableId<AssemblyOperationIdScope> storageSecureOperationId,
            StableId<AssemblyOperationIdScope> processorCoolerRetainOperationId,
            StableId<AssemblyOperationIdScope> graphicsCardRetainOperationId,
            StableId<AssemblyOperationIdScope> powerSupplyRetainOperationId,
            StableId<AssemblyOperationIdScope> atx24RouteOperationId,
            StableId<AssemblyOperationIdScope> eps12vRouteOperationId,
            StableId<AssemblyOperationIdScope> pcieGpuRouteOperationId,
            long assemblyRevision,
            long atx24PowerCableRevision,
            long eps12vPowerCableRevision,
            long pcieGpuPowerCableRevision)
        {
            BuildId = buildId;
            ChassisId = chassisId;
            MotherboardItemId = motherboardItemId;
            ProcessorItemId = processorItemId;
            MemoryItemId = memoryItemId;
            StorageItemId = storageItemId;
            ProcessorCoolerItemId = processorCoolerItemId;
            GraphicsCardItemId = graphicsCardItemId;
            PowerSupplyItemId = powerSupplyItemId;
            Atx24PowerCableItemId = atx24PowerCableItemId;
            Eps12vPowerCableItemId = eps12vPowerCableItemId;
            PcieGpuPowerCableItemId = pcieGpuPowerCableItemId;
            MotherboardSecureOperationId = motherboardSecureOperationId;
            ProcessorRetainOperationId = processorRetainOperationId;
            MemoryRetainOperationId = memoryRetainOperationId;
            StorageSecureOperationId = storageSecureOperationId;
            ProcessorCoolerRetainOperationId = processorCoolerRetainOperationId;
            GraphicsCardRetainOperationId = graphicsCardRetainOperationId;
            PowerSupplyRetainOperationId = powerSupplyRetainOperationId;
            Atx24RouteOperationId = atx24RouteOperationId;
            Eps12vRouteOperationId = eps12vRouteOperationId;
            PcieGpuRouteOperationId = pcieGpuRouteOperationId;
            AssemblyRevision = assemblyRevision;
            Atx24PowerCableRevision = atx24PowerCableRevision;
            Eps12vPowerCableRevision = eps12vPowerCableRevision;
            PcieGpuPowerCableRevision = pcieGpuPowerCableRevision;
        }

        public StableId<PcBuildIdScope> BuildId { get; }

        public StableId<ChassisIdScope> ChassisId { get; }

        public StableId<ItemInstanceIdScope> MotherboardItemId { get; }

        public StableId<ItemInstanceIdScope> ProcessorItemId { get; }

        public StableId<ItemInstanceIdScope> MemoryItemId { get; }

        public StableId<ItemInstanceIdScope> StorageItemId { get; }

        public StableId<ItemInstanceIdScope> ProcessorCoolerItemId { get; }

        public StableId<ItemInstanceIdScope> GraphicsCardItemId { get; }

        public StableId<ItemInstanceIdScope> PowerSupplyItemId { get; }

        public StableId<ItemInstanceIdScope> Atx24PowerCableItemId { get; }

        public StableId<ItemInstanceIdScope> Eps12vPowerCableItemId { get; }

        public StableId<ItemInstanceIdScope> PcieGpuPowerCableItemId { get; }

        public StableId<AssemblyOperationIdScope> MotherboardSecureOperationId { get; }

        public StableId<AssemblyOperationIdScope> ProcessorRetainOperationId { get; }

        public StableId<AssemblyOperationIdScope> MemoryRetainOperationId { get; }

        public StableId<AssemblyOperationIdScope> StorageSecureOperationId { get; }

        public StableId<AssemblyOperationIdScope> ProcessorCoolerRetainOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> GraphicsCardRetainOperationId
        {
            get;
        }

        public StableId<AssemblyOperationIdScope> PowerSupplyRetainOperationId { get; }

        public StableId<AssemblyOperationIdScope> Atx24RouteOperationId { get; }

        public StableId<AssemblyOperationIdScope> Eps12vRouteOperationId { get; }

        public StableId<AssemblyOperationIdScope> PcieGpuRouteOperationId { get; }

        public long AssemblyRevision { get; }

        public long Atx24PowerCableRevision { get; }

        public long Eps12vPowerCableRevision { get; }

        public long PcieGpuPowerCableRevision { get; }
    }

    public static class ElectricalReadinessFailures
    {
        public static readonly Failure ConfigurationUnsupported =
            Failure.FromCode("assembly.electrical.configuration-unsupported");
        public static readonly Failure Atx24PowerCableMissing =
            Failure.FromCode("assembly.electrical.atx24-missing");
        public static readonly Failure Eps12vPowerCableMissing =
            Failure.FromCode("assembly.electrical.eps12v-missing");
        public static readonly Failure PcieGpuPowerCableMissing =
            Failure.FromCode("assembly.electrical.pcie-gpu-missing");
        public static readonly Failure InvariantInvalid =
            Failure.FromCode("assembly.electrical.invariant-invalid");
    }
}
