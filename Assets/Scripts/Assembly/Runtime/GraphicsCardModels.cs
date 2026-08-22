using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Stable identity scope for the motherboard PCIe slot retention latch.
    /// Kept with the bounded graphics-card model so no shared identity file edit is
    /// required by this patch.
    /// </summary>
    public sealed class AssemblyGraphicsCardLatchIdScope : IStableIdScope
    {
    }

    /// <summary>
    /// Persisted bounded state of the canonical, capacity-one PCIe x16 graphics-card
    /// slot. Unsupported is the fail-closed default for authorities created before
    /// graphics-card topology existed.
    /// </summary>
    public enum GraphicsCardSlotState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        GraphicsCardSeatedUnsecured = 2,
        GraphicsCardRetained = 3
    }

    /// <summary>
    /// The two deterministic 180-degree choices exposed by placement input. The
    /// compatibility evaluator accepts only the keyed orientation for the canonical
    /// full-height PCIe card.
    /// </summary>
    public enum GraphicsCardMountOrientation
    {
        Primary = 1,
        Rotated180 = 2
    }

    /// <summary>
    /// Immutable PCIe latch and chassis rear-bracket retention identities. Retain and
    /// unretain apply the latch plus the single rear-bracket fastener atomically, so
    /// no half-latched or half-fastened persisted state can exist.
    /// </summary>
    public sealed class GraphicsCardRetentionTopology
    {
        private GraphicsCardRetentionTopology(
            StableId<AssemblyGraphicsCardLatchIdScope> latchId,
            StableId<AssemblyFastenerIdScope> bracketFastenerId)
        {
            LatchId = latchId;
            BracketFastenerId = bracketFastenerId;
        }

        public StableId<AssemblyGraphicsCardLatchIdScope> LatchId { get; }

        public StableId<AssemblyFastenerIdScope> BracketFastenerId { get; }

        public bool IsValid =>
            !LatchId.IsEmpty &&
            !BracketFastenerId.IsEmpty;

        public static OperationResult<GraphicsCardRetentionTopology> Create(
            StableId<AssemblyGraphicsCardLatchIdScope> latchId,
            StableId<AssemblyFastenerIdScope> bracketFastenerId)
        {
            if (latchId.IsEmpty)
            {
                return OperationResult<GraphicsCardRetentionTopology>.Fail(
                    AssemblyFailures.InvalidGraphicsCardSlotLatch);
            }

            if (bracketFastenerId.IsEmpty)
            {
                return OperationResult<GraphicsCardRetentionTopology>.Fail(
                    AssemblyFailures.InvalidGraphicsCardBracketFastener);
            }

            return OperationResult<GraphicsCardRetentionTopology>.Success(
                new GraphicsCardRetentionTopology(
                    latchId,
                    bracketFastenerId));
        }

        internal bool HasExactIdentity(GraphicsCardRetentionTopology other)
        {
            return other != null &&
                   LatchId == other.LatchId &&
                   BracketFastenerId == other.BracketFastenerId;
        }
    }

    /// <summary>
    /// Immutable topology and typed fitment of the single canonical PCIe x16 slot.
    /// The graphics-card type is a persisted interface/height/thickness profile; world
    /// clearance remains a fail-closed presentation query before this domain operation.
    /// </summary>
    public readonly struct GraphicsCardSlotDefinition
    {
        private GraphicsCardSlotDefinition(
            StableId<AssemblySlotIdScope> slotId,
            StableId<ContainerIdScope> containerId,
            GraphicsCardRetentionTopology retentionTopology,
            GraphicsCardType supportedGraphicsCardType)
        {
            SlotId = slotId;
            ContainerId = containerId;
            RetentionTopology = retentionTopology;
            SupportedGraphicsCardType = supportedGraphicsCardType;
        }

        public StableId<AssemblySlotIdScope> SlotId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public GraphicsCardRetentionTopology RetentionTopology { get; }

        public GraphicsCardType SupportedGraphicsCardType { get; }

        public bool IsValid =>
            !SlotId.IsEmpty &&
            !ContainerId.IsEmpty &&
            RetentionTopology != null &&
            RetentionTopology.IsValid &&
            PcComponentSpecification.IsValidGraphicsCardType(
                SupportedGraphicsCardType);

        public static OperationResult<GraphicsCardSlotDefinition> Create(
            StableId<AssemblySlotIdScope> slotId,
            StableId<ContainerIdScope> containerId,
            GraphicsCardRetentionTopology retentionTopology,
            GraphicsCardType supportedGraphicsCardType)
        {
            if (slotId.IsEmpty)
            {
                return OperationResult<GraphicsCardSlotDefinition>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (containerId.IsEmpty)
            {
                return OperationResult<GraphicsCardSlotDefinition>.Fail(
                    AssemblyFailures.InvalidGraphicsCardSlotContainer);
            }

            if (retentionTopology == null || !retentionTopology.IsValid)
            {
                return OperationResult<GraphicsCardSlotDefinition>.Fail(
                    retentionTopology == null || retentionTopology.LatchId.IsEmpty
                        ? AssemblyFailures.InvalidGraphicsCardSlotLatch
                        : AssemblyFailures.InvalidGraphicsCardBracketFastener);
            }

            if (!PcComponentSpecification.IsValidGraphicsCardType(
                    supportedGraphicsCardType))
            {
                return OperationResult<GraphicsCardSlotDefinition>.Fail(
                    AssemblyFailures.InvalidGraphicsCardType);
            }

            return OperationResult<GraphicsCardSlotDefinition>.Success(
                new GraphicsCardSlotDefinition(
                    slotId,
                    containerId,
                    retentionTopology,
                    supportedGraphicsCardType));
        }

        internal bool HasExactIdentity(GraphicsCardSlotDefinition other)
        {
            return SlotId == other.SlotId &&
                   ContainerId == other.ContainerId &&
                   SupportedGraphicsCardType == other.SupportedGraphicsCardType &&
                   RetentionTopology != null &&
                   RetentionTopology.HasExactIdentity(other.RetentionTopology);
        }
    }
}
