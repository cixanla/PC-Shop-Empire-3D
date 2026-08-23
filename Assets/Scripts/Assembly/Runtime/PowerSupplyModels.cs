using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Stable chassis-owned identity for the PSU rear mounting plane.
    /// </summary>
    public sealed class AssemblyPowerSupplyRearMountIdScope : IStableIdScope
    {
    }

    /// <summary>
    /// Persisted bounded state of one capacity-one PSU bay. Unsupported remains the
    /// fail-closed default for assembly authorities created before this topology existed.
    /// </summary>
    public enum PowerSupplyBayState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        PowerSupplySeatedUnsecured = 2,
        PowerSupplyRetained = 3
    }

    /// <summary>
    /// The two deterministic 180-degree orientations exposed by placement input.
    /// The canonical garage chassis accepts only its filtered floor-vent orientation.
    /// </summary>
    public enum PowerSupplyMountOrientation
    {
        FanToFilteredVent = 1,
        FanAwayFromFilteredVent = 2
    }

    /// <summary>
    /// Immutable rear mount and four visible fastener identities. Retain/unretain are
    /// atomic domain operations; the stable cross order is a deterministic presentation
    /// contract and never permits a half-fastened persisted state.
    /// </summary>
    public sealed class PowerSupplyRetentionTopology
    {
        private readonly IReadOnlyList<StableId<AssemblyFastenerIdScope>> _physicalOrder;
        private readonly IReadOnlyList<StableId<AssemblyFastenerIdScope>>
            _deterministicRetentionOrder;
        private readonly IReadOnlyList<StableId<AssemblyFastenerIdScope>>
            _reverseRetentionOrder;

        private PowerSupplyRetentionTopology(
            StableId<AssemblyPowerSupplyRearMountIdScope> rearMountId,
            StableId<AssemblyFastenerIdScope> topLeftFastenerId,
            StableId<AssemblyFastenerIdScope> topRightFastenerId,
            StableId<AssemblyFastenerIdScope> bottomLeftFastenerId,
            StableId<AssemblyFastenerIdScope> bottomRightFastenerId)
        {
            RearMountId = rearMountId;
            TopLeftFastenerId = topLeftFastenerId;
            TopRightFastenerId = topRightFastenerId;
            BottomLeftFastenerId = bottomLeftFastenerId;
            BottomRightFastenerId = bottomRightFastenerId;
            _physicalOrder = Array.AsReadOnly(new[]
            {
                topLeftFastenerId,
                topRightFastenerId,
                bottomLeftFastenerId,
                bottomRightFastenerId
            });
            _deterministicRetentionOrder = Array.AsReadOnly(new[]
            {
                topLeftFastenerId,
                bottomRightFastenerId,
                topRightFastenerId,
                bottomLeftFastenerId
            });
            _reverseRetentionOrder = Array.AsReadOnly(new[]
            {
                bottomLeftFastenerId,
                topRightFastenerId,
                bottomRightFastenerId,
                topLeftFastenerId
            });
        }

        public StableId<AssemblyPowerSupplyRearMountIdScope> RearMountId { get; }

        public StableId<AssemblyFastenerIdScope> TopLeftFastenerId { get; }

        public StableId<AssemblyFastenerIdScope> TopRightFastenerId { get; }

        public StableId<AssemblyFastenerIdScope> BottomLeftFastenerId { get; }

        public StableId<AssemblyFastenerIdScope> BottomRightFastenerId { get; }

        public IReadOnlyList<StableId<AssemblyFastenerIdScope>> PhysicalOrder =>
            _physicalOrder;

        public IReadOnlyList<StableId<AssemblyFastenerIdScope>>
            DeterministicRetentionOrder => _deterministicRetentionOrder;

        public IReadOnlyList<StableId<AssemblyFastenerIdScope>> ReverseRetentionOrder =>
            _reverseRetentionOrder;

        public bool IsValid =>
            !RearMountId.IsEmpty &&
            AreFourFastenersDistinctAndNonEmpty(
                TopLeftFastenerId,
                TopRightFastenerId,
                BottomLeftFastenerId,
                BottomRightFastenerId);

        public static OperationResult<PowerSupplyRetentionTopology> Create(
            StableId<AssemblyPowerSupplyRearMountIdScope> rearMountId,
            StableId<AssemblyFastenerIdScope> topLeftFastenerId,
            StableId<AssemblyFastenerIdScope> topRightFastenerId,
            StableId<AssemblyFastenerIdScope> bottomLeftFastenerId,
            StableId<AssemblyFastenerIdScope> bottomRightFastenerId)
        {
            if (rearMountId.IsEmpty)
            {
                return OperationResult<PowerSupplyRetentionTopology>.Fail(
                    AssemblyFailures.InvalidPowerSupplyRearMount);
            }

            if (!AreFourFastenersDistinctAndNonEmpty(
                    topLeftFastenerId,
                    topRightFastenerId,
                    bottomLeftFastenerId,
                    bottomRightFastenerId))
            {
                return OperationResult<PowerSupplyRetentionTopology>.Fail(
                    AssemblyFailures.InvalidPowerSupplyFastenerTopology);
            }

            return OperationResult<PowerSupplyRetentionTopology>.Success(
                new PowerSupplyRetentionTopology(
                    rearMountId,
                    topLeftFastenerId,
                    topRightFastenerId,
                    bottomLeftFastenerId,
                    bottomRightFastenerId));
        }

        internal bool HasExactIdentity(PowerSupplyRetentionTopology other)
        {
            return other != null &&
                   RearMountId == other.RearMountId &&
                   TopLeftFastenerId == other.TopLeftFastenerId &&
                   TopRightFastenerId == other.TopRightFastenerId &&
                   BottomLeftFastenerId == other.BottomLeftFastenerId &&
                   BottomRightFastenerId == other.BottomRightFastenerId;
        }

        private static bool AreFourFastenersDistinctAndNonEmpty(
            StableId<AssemblyFastenerIdScope> first,
            StableId<AssemblyFastenerIdScope> second,
            StableId<AssemblyFastenerIdScope> third,
            StableId<AssemblyFastenerIdScope> fourth)
        {
            return !first.IsEmpty &&
                   !second.IsEmpty &&
                   !third.IsEmpty &&
                   !fourth.IsEmpty &&
                   first != second &&
                   first != third &&
                   first != fourth &&
                   second != third &&
                   second != fourth &&
                   third != fourth;
        }
    }

    /// <summary>
    /// Immutable chassis-owned PSU bay topology and mechanical fitment contract.
    /// </summary>
    public readonly struct PowerSupplyBayDefinition
    {
        private PowerSupplyBayDefinition(
            StableId<AssemblySlotIdScope> slotId,
            StableId<ContainerIdScope> containerId,
            PowerSupplyRetentionTopology retentionTopology,
            PowerSupplyType supportedPowerSupplyType)
        {
            SlotId = slotId;
            ContainerId = containerId;
            RetentionTopology = retentionTopology;
            SupportedPowerSupplyType = supportedPowerSupplyType;
        }

        public StableId<AssemblySlotIdScope> SlotId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public PowerSupplyRetentionTopology RetentionTopology { get; }

        public PowerSupplyType SupportedPowerSupplyType { get; }

        public bool IsValid =>
            !SlotId.IsEmpty &&
            !ContainerId.IsEmpty &&
            RetentionTopology != null &&
            RetentionTopology.IsValid &&
            PcComponentSpecification.IsValidPowerSupplyType(SupportedPowerSupplyType);

        public static OperationResult<PowerSupplyBayDefinition> Create(
            StableId<AssemblySlotIdScope> slotId,
            StableId<ContainerIdScope> containerId,
            PowerSupplyRetentionTopology retentionTopology,
            PowerSupplyType supportedPowerSupplyType)
        {
            if (slotId.IsEmpty)
            {
                return OperationResult<PowerSupplyBayDefinition>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (containerId.IsEmpty)
            {
                return OperationResult<PowerSupplyBayDefinition>.Fail(
                    AssemblyFailures.InvalidPowerSupplyBayContainer);
            }

            if (retentionTopology == null || !retentionTopology.IsValid)
            {
                return OperationResult<PowerSupplyBayDefinition>.Fail(
                    retentionTopology == null || retentionTopology.RearMountId.IsEmpty
                        ? AssemblyFailures.InvalidPowerSupplyRearMount
                        : AssemblyFailures.InvalidPowerSupplyFastenerTopology);
            }

            if (!PcComponentSpecification.IsValidPowerSupplyType(
                    supportedPowerSupplyType))
            {
                return OperationResult<PowerSupplyBayDefinition>.Fail(
                    AssemblyFailures.InvalidPowerSupplyType);
            }

            return OperationResult<PowerSupplyBayDefinition>.Success(
                new PowerSupplyBayDefinition(
                    slotId,
                    containerId,
                    retentionTopology,
                    supportedPowerSupplyType));
        }

        internal bool HasExactIdentity(PowerSupplyBayDefinition other)
        {
            return SlotId == other.SlotId &&
                   ContainerId == other.ContainerId &&
                   SupportedPowerSupplyType == other.SupportedPowerSupplyType &&
                   RetentionTopology != null &&
                   RetentionTopology.HasExactIdentity(other.RetentionTopology);
        }
    }
}
