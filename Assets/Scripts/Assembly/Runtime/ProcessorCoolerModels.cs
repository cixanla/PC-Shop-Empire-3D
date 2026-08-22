using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Assembly
{
    /// <summary>
    /// Persisted bounded state of the single processor-cooler slot. Unsupported is the
    /// fail-closed default for authorities created before cooler topology existed.
    /// </summary>
    public enum ProcessorCoolerSlotState
    {
        Unsupported = 0,
        EmptyOpen = 1,
        CoolerSeatedUnsecured = 2,
        CoolerRetained = 3
    }

    /// <summary>
    /// The only two orientations exposed by the canonical 180-degree mounting flow.
    /// </summary>
    public enum ProcessorCoolerMountOrientation
    {
        Primary = 1,
        Rotated180 = 2
    }

    /// <summary>
    /// Receipt-level state of the factory-applied, single-use thermal interface.
    /// </summary>
    public enum ProcessorCoolerTimState
    {
        Unsupported = 0,
        PreAppliedUnused = 1,
        AppliedConsumed = 2
    }

    /// <summary>
    /// Immutable four-point retention topology. Physical point order is 1,2,3,4;
    /// retention is the stable cross order 1,3,2,4 and release is its exact reverse.
    /// </summary>
    public sealed class ProcessorCoolerRetentionTopology
    {
        private readonly IReadOnlyList<StableId<AssemblyProcessorCoolerRetentionPointIdScope>>
            _physicalOrder;
        private readonly IReadOnlyList<StableId<AssemblyProcessorCoolerRetentionPointIdScope>>
            _crossRetentionOrder;
        private readonly IReadOnlyList<StableId<AssemblyProcessorCoolerRetentionPointIdScope>>
            _reverseCrossRetentionOrder;

        private ProcessorCoolerRetentionTopology(
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point1Id,
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point2Id,
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point3Id,
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point4Id)
        {
            Point1Id = point1Id;
            Point2Id = point2Id;
            Point3Id = point3Id;
            Point4Id = point4Id;
            _physicalOrder = Array.AsReadOnly(new[]
            {
                point1Id,
                point2Id,
                point3Id,
                point4Id
            });
            _crossRetentionOrder = Array.AsReadOnly(new[]
            {
                point1Id,
                point3Id,
                point2Id,
                point4Id
            });
            _reverseCrossRetentionOrder = Array.AsReadOnly(new[]
            {
                point4Id,
                point2Id,
                point3Id,
                point1Id
            });
        }

        public StableId<AssemblyProcessorCoolerRetentionPointIdScope> Point1Id { get; }

        public StableId<AssemblyProcessorCoolerRetentionPointIdScope> Point2Id { get; }

        public StableId<AssemblyProcessorCoolerRetentionPointIdScope> Point3Id { get; }

        public StableId<AssemblyProcessorCoolerRetentionPointIdScope> Point4Id { get; }

        public IReadOnlyList<StableId<AssemblyProcessorCoolerRetentionPointIdScope>>
            PhysicalOrder => _physicalOrder;

        public IReadOnlyList<StableId<AssemblyProcessorCoolerRetentionPointIdScope>>
            CrossRetentionOrder => _crossRetentionOrder;

        public IReadOnlyList<StableId<AssemblyProcessorCoolerRetentionPointIdScope>>
            ReverseCrossRetentionOrder => _reverseCrossRetentionOrder;

        public bool IsValid =>
            !Point1Id.IsEmpty &&
            !Point2Id.IsEmpty &&
            !Point3Id.IsEmpty &&
            !Point4Id.IsEmpty &&
            Point1Id != Point2Id &&
            Point1Id != Point3Id &&
            Point1Id != Point4Id &&
            Point2Id != Point3Id &&
            Point2Id != Point4Id &&
            Point3Id != Point4Id;

        public static OperationResult<ProcessorCoolerRetentionTopology> Create(
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point1Id,
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point2Id,
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point3Id,
            StableId<AssemblyProcessorCoolerRetentionPointIdScope> point4Id)
        {
            if (point1Id.IsEmpty || point2Id.IsEmpty ||
                point3Id.IsEmpty || point4Id.IsEmpty ||
                point1Id == point2Id || point1Id == point3Id || point1Id == point4Id ||
                point2Id == point3Id || point2Id == point4Id || point3Id == point4Id)
            {
                return OperationResult<ProcessorCoolerRetentionTopology>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerRetentionTopology);
            }

            return OperationResult<ProcessorCoolerRetentionTopology>.Success(
                new ProcessorCoolerRetentionTopology(
                    point1Id,
                    point2Id,
                    point3Id,
                    point4Id));
        }

        internal bool HasExactIdentity(ProcessorCoolerRetentionTopology other)
        {
            return other != null &&
                   Point1Id == other.Point1Id &&
                   Point2Id == other.Point2Id &&
                   Point3Id == other.Point3Id &&
                   Point4Id == other.Point4Id;
        }
    }

    /// <summary>
    /// Immutable topology and typed fitment of the single canonical cooler slot.
    /// </summary>
    public readonly struct ProcessorCoolerSlotDefinition
    {
        private ProcessorCoolerSlotDefinition(
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyProcessorCoolerBracketIdScope> bracketId,
            StableId<ContainerIdScope> containerId,
            ProcessorCoolerRetentionTopology retentionTopology,
            ProcessorCoolerType supportedCoolerType,
            CpuSocketFamily supportedSocketFamily)
        {
            SlotId = slotId;
            BracketId = bracketId;
            ContainerId = containerId;
            RetentionTopology = retentionTopology;
            SupportedCoolerType = supportedCoolerType;
            SupportedSocketFamily = supportedSocketFamily;
        }

        public StableId<AssemblySlotIdScope> SlotId { get; }

        public StableId<AssemblyProcessorCoolerBracketIdScope> BracketId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public ProcessorCoolerRetentionTopology RetentionTopology { get; }

        public ProcessorCoolerType SupportedCoolerType { get; }

        public CpuSocketFamily SupportedSocketFamily { get; }

        public bool IsValid =>
            !SlotId.IsEmpty &&
            !BracketId.IsEmpty &&
            !ContainerId.IsEmpty &&
            RetentionTopology != null &&
            RetentionTopology.IsValid &&
            PcComponentSpecification.IsValidProcessorCoolerType(SupportedCoolerType) &&
            PcComponentSpecification.IsValidCpuSocketFamily(SupportedSocketFamily) &&
            PcComponentSpecification.IsProcessorCoolerCompatibleWithSocket(
                SupportedCoolerType,
                SupportedSocketFamily);

        public static OperationResult<ProcessorCoolerSlotDefinition> Create(
            StableId<AssemblySlotIdScope> slotId,
            StableId<AssemblyProcessorCoolerBracketIdScope> bracketId,
            StableId<ContainerIdScope> containerId,
            ProcessorCoolerRetentionTopology retentionTopology,
            ProcessorCoolerType supportedCoolerType,
            CpuSocketFamily supportedSocketFamily)
        {
            if (slotId.IsEmpty)
            {
                return OperationResult<ProcessorCoolerSlotDefinition>.Fail(
                    AssemblyFailures.InvalidSlotId);
            }

            if (bracketId.IsEmpty)
            {
                return OperationResult<ProcessorCoolerSlotDefinition>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerBracket);
            }

            if (containerId.IsEmpty)
            {
                return OperationResult<ProcessorCoolerSlotDefinition>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerSlotContainer);
            }

            if (retentionTopology == null || !retentionTopology.IsValid)
            {
                return OperationResult<ProcessorCoolerSlotDefinition>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerRetentionTopology);
            }

            if (!PcComponentSpecification.IsValidProcessorCoolerType(supportedCoolerType))
            {
                return OperationResult<ProcessorCoolerSlotDefinition>.Fail(
                    AssemblyFailures.InvalidProcessorCoolerType);
            }

            if (!PcComponentSpecification.IsValidCpuSocketFamily(supportedSocketFamily))
            {
                return OperationResult<ProcessorCoolerSlotDefinition>.Fail(
                    AssemblyFailures.InvalidCpuSocketFamily);
            }

            if (!PcComponentSpecification.IsProcessorCoolerCompatibleWithSocket(
                    supportedCoolerType,
                    supportedSocketFamily))
            {
                return OperationResult<ProcessorCoolerSlotDefinition>.Fail(
                    AssemblyFailures.ProcessorCoolerSocketMismatch);
            }

            return OperationResult<ProcessorCoolerSlotDefinition>.Success(
                new ProcessorCoolerSlotDefinition(
                    slotId,
                    bracketId,
                    containerId,
                    retentionTopology,
                    supportedCoolerType,
                    supportedSocketFamily));
        }

        internal bool HasExactIdentity(ProcessorCoolerSlotDefinition other)
        {
            return SlotId == other.SlotId &&
                   BracketId == other.BracketId &&
                   ContainerId == other.ContainerId &&
                   SupportedCoolerType == other.SupportedCoolerType &&
                   SupportedSocketFamily == other.SupportedSocketFamily &&
                   RetentionTopology != null &&
                   RetentionTopology.HasExactIdentity(other.RetentionTopology);
        }
    }
}
