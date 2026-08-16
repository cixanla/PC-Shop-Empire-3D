using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Persisted bounded state of one DIMM slot. Unsupported is the fail-closed default for
    /// older authorities that were created before memory topology existed.
    /// </summary>
    public enum MemorySlotState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        MemoryModuleSeatedOpen = 2,
        MemoryModuleRetained = 3
    }

    /// <summary>
    /// The two physically reachable keyed orientations of the bounded UDIMM presentation.
    /// Only NotchAligned is compatible with a seat transaction.
    /// </summary>
    public enum DimmKeyOrientation
    {
        NotchAligned = 1,
        Reversed = 2
    }

    /// <summary>
    /// Immutable topology for one canonical memory slot. Channel and bank use distinct
    /// stable-id scopes so their positional identities cannot be accidentally swapped.
    /// </summary>
    public readonly struct DimmSlotDefinition
    {
        private DimmSlotDefinition(
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<ContainerIdScope> containerId,
            StableId<AssemblyMemoryChannelIdScope> channelId,
            StableId<AssemblyMemoryBankIdScope> bankId,
            int populationPriority,
            DimmType supportedDimmType)
        {
            SlotId = slotId;
            RetentionId = retentionId;
            ContainerId = containerId;
            ChannelId = channelId;
            BankId = bankId;
            PopulationPriority = populationPriority;
            SupportedDimmType = supportedDimmType;
        }

        public StableId<AssemblySlotIdScope> SlotId { get; }

        public StableId<AssemblyRetentionIdScope> RetentionId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public StableId<AssemblyMemoryChannelIdScope> ChannelId { get; }

        public StableId<AssemblyMemoryBankIdScope> BankId { get; }

        public int PopulationPriority { get; }

        public DimmType SupportedDimmType { get; }

        public bool IsValid =>
            !SlotId.IsEmpty &&
            !RetentionId.IsEmpty &&
            !ContainerId.IsEmpty &&
            !ChannelId.IsEmpty &&
            !BankId.IsEmpty &&
            PopulationPriority == 1 &&
            PcComponentSpecification.IsValidDimmType(SupportedDimmType);

        public static OperationResult<DimmSlotDefinition> Create(
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyRetentionIdScope> retentionId,
            StableId<ContainerIdScope> containerId,
            StableId<AssemblyMemoryChannelIdScope> channelId,
            StableId<AssemblyMemoryBankIdScope> bankId,
            int populationPriority,
            DimmType supportedDimmType)
        {
            if (slotId.IsEmpty)
            {
                return OperationResult<DimmSlotDefinition>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (retentionId.IsEmpty)
            {
                return OperationResult<DimmSlotDefinition>.Fail(
                    AssemblyFailures.InvalidRetention);
            }

            if (containerId.IsEmpty)
            {
                return OperationResult<DimmSlotDefinition>.Fail(
                    AssemblyFailures.InvalidMemorySlotContainer);
            }

            if (channelId.IsEmpty)
            {
                return OperationResult<DimmSlotDefinition>.Fail(
                    AssemblyFailures.InvalidMemoryChannel);
            }

            if (bankId.IsEmpty)
            {
                return OperationResult<DimmSlotDefinition>.Fail(
                    AssemblyFailures.InvalidMemoryBank);
            }

            if (populationPriority != 1)
            {
                return OperationResult<DimmSlotDefinition>.Fail(
                    AssemblyFailures.InvalidMemoryPopulationPriority);
            }

            if (!PcComponentSpecification.IsValidDimmType(supportedDimmType))
            {
                return OperationResult<DimmSlotDefinition>.Fail(
                    AssemblyFailures.InvalidDimmType);
            }

            return OperationResult<DimmSlotDefinition>.Success(
                new DimmSlotDefinition(
                    slotId,
                    retentionId,
                    containerId,
                    channelId,
                    bankId,
                    populationPriority,
                    supportedDimmType));
        }
    }
}
