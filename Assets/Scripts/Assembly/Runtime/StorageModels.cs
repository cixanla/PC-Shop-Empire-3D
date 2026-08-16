using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Persisted bounded state of the single M.2 storage slot. Unsupported is the
    /// fail-closed default for authorities created before storage topology existed.
    /// </summary>
    public enum StorageSlotState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        StorageDeviceSeatedUnsecured = 2,
        StorageDeviceSecured = 3
    }

    /// <summary>
    /// The two physically reachable M-key orientations. Only KeyAligned may seat.
    /// </summary>
    public enum M2KeyOrientation
    {
        KeyAligned = 1,
        Reversed = 2
    }

    /// <summary>
    /// Immutable topology of the bounded primary M.2 slot. The slot, 2280 standoff and
    /// motherboard-owned captive screw have distinct typed identities.
    /// </summary>
    public readonly struct M2SlotDefinition
    {
        private M2SlotDefinition(
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyStorageStandoffIdScope> standoffId,
            StableId<AssemblyRetentionIdScope> captiveScrewId,
            StableId<ContainerIdScope> containerId,
            M2StorageType supportedStorageType)
        {
            SlotId = slotId;
            StandoffId = standoffId;
            CaptiveScrewId = captiveScrewId;
            ContainerId = containerId;
            SupportedStorageType = supportedStorageType;
        }

        public StableId<AssemblySlotIdScope> SlotId { get; }

        public StableId<AssemblyStorageStandoffIdScope> StandoffId { get; }

        public StableId<AssemblyRetentionIdScope> CaptiveScrewId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public M2StorageType SupportedStorageType { get; }

        public bool IsValid =>
            !SlotId.IsEmpty &&
            !StandoffId.IsEmpty &&
            !CaptiveScrewId.IsEmpty &&
            !ContainerId.IsEmpty &&
            PcComponentSpecification.IsValidM2StorageType(SupportedStorageType);

        public static OperationResult<M2SlotDefinition> Create(
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyStorageStandoffIdScope> standoffId,
            StableId<AssemblyRetentionIdScope> captiveScrewId,
            StableId<ContainerIdScope> containerId,
            M2StorageType supportedStorageType)
        {
            if (slotId.IsEmpty)
            {
                return OperationResult<M2SlotDefinition>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (standoffId.IsEmpty)
            {
                return OperationResult<M2SlotDefinition>.Fail(
                    AssemblyFailures.InvalidStorageStandoff);
            }

            if (captiveScrewId.IsEmpty)
            {
                return OperationResult<M2SlotDefinition>.Fail(
                    AssemblyFailures.InvalidRetention);
            }

            if (containerId.IsEmpty)
            {
                return OperationResult<M2SlotDefinition>.Fail(
                    AssemblyFailures.InvalidStorageSlotContainer);
            }

            if (!PcComponentSpecification.IsValidM2StorageType(supportedStorageType))
            {
                return OperationResult<M2SlotDefinition>.Fail(
                    AssemblyFailures.InvalidM2StorageType);
            }

            return OperationResult<M2SlotDefinition>.Success(
                new M2SlotDefinition(
                    slotId,
                    standoffId,
                    captiveScrewId,
                    containerId,
                    supportedStorageType));
        }
    }
}
