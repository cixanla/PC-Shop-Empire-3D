using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Orders
{
    internal readonly struct CustomPcBuildKitOrderComponentKey :
        IEquatable<CustomPcBuildKitOrderComponentKey>
    {
        internal CustomPcBuildKitOrderComponentKey(
            StableId<CustomPcBuildOrderIdScope> buildOrderId,
            PcComponentKind componentKind,
            PowerCableType powerCableType = default)
        {
            BuildOrderId = buildOrderId;
            ComponentKind = componentKind;
            PowerCableType = powerCableType;
        }

        internal StableId<CustomPcBuildOrderIdScope> BuildOrderId { get; }

        internal PcComponentKind ComponentKind { get; }

        internal PowerCableType PowerCableType { get; }

        public bool Equals(CustomPcBuildKitOrderComponentKey other)
        {
            return BuildOrderId == other.BuildOrderId &&
                   ComponentKind == other.ComponentKind &&
                   PowerCableType == other.PowerCableType;
        }

        public override bool Equals(object obj)
        {
            return obj is CustomPcBuildKitOrderComponentKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(
                BuildOrderId,
                (int)ComponentKind,
                (int)PowerCableType);
        }
    }

    internal sealed class CustomPcBuildKitRegistration
    {
        internal CustomPcBuildKitRegistration(
            CustomPcBuildKitReceipt pickupReceipt,
            InventorySerializedReservationWorkOrderBuildKitReceipt inventoryPickupReceipt)
        {
            PickupReceipt = pickupReceipt;
            InventoryPickupReceipt = inventoryPickupReceipt;
        }

        internal CustomPcBuildKitReceipt PickupReceipt { get; }

        internal InventorySerializedReservationWorkOrderBuildKitReceipt
            InventoryPickupReceipt { get; }

        internal CustomPcBuildKitReceipt PlacementReceipt { get; private set; }

        internal InventorySerializedReservationWorkOrderBuildKitReceipt
            InventoryPlacementReceipt { get; private set; }

        internal bool TryPublishPlacement(
            CustomPcBuildKitReceipt placementReceipt,
            InventorySerializedReservationWorkOrderBuildKitReceipt inventoryPlacementReceipt)
        {
            if (PlacementReceipt != null ||
                InventoryPlacementReceipt != null ||
                placementReceipt == null ||
                inventoryPlacementReceipt == null)
            {
                return false;
            }

            PlacementReceipt = placementReceipt;
            InventoryPlacementReceipt = inventoryPlacementReceipt;
            return true;
        }
    }

    /// <summary>
    /// Work-order projection for the physical reserved component kit. Inventory remains the
    /// custody authority; this aggregate selects the canonical BOM role and maps exact
    /// Inventory receipts back to the customer job without starting AssemblyBuildAuthority.
    /// </summary>
    public sealed class CustomPcBuildKitAuthority
    {
        private readonly CustomPcWorkOrderAuthority _workOrders;
        private readonly InventoryAuthority _inventory;
        private readonly StableId<ContainerIdScope> _sourceContainerId;
        private readonly StableId<ContainerIdScope> _handsContainerId;
        private readonly StableId<ContainerIdScope> _motherboardBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _motherboardBuildKitAccess;
        private readonly StableId<ContainerIdScope> _processorBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _processorBuildKitAccess;
        private readonly StableId<ContainerIdScope> _memoryModuleBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _memoryModuleBuildKitAccess;
        private readonly StableId<ContainerIdScope> _storageBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _storageBuildKitAccess;
        private readonly StableId<ContainerIdScope> _processorCoolerBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _processorCoolerBuildKitAccess;
        private readonly StableId<ContainerIdScope> _graphicsCardBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _graphicsCardBuildKitAccess;
        private readonly StableId<ContainerIdScope> _powerSupplyBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _powerSupplyBuildKitAccess;
        private readonly StableId<ContainerIdScope> _atx24PowerCableBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _atx24PowerCableBuildKitAccess;
        private readonly StableId<ContainerIdScope> _eps12vPowerCableBuildKitContainerId;
        private readonly InventorySerializedTransferAccess _eps12vPowerCableBuildKitAccess;
        private readonly Dictionary<StableId<CustomPcBuildKitOperationIdScope>,
            CustomPcBuildKitRegistration> _registrationsByOperation =
                new Dictionary<StableId<CustomPcBuildKitOperationIdScope>,
                    CustomPcBuildKitRegistration>();
        private readonly Dictionary<CustomPcBuildKitOrderComponentKey,
            CustomPcBuildKitRegistration> _registrationsByOrderAndComponent =
                new Dictionary<CustomPcBuildKitOrderComponentKey,
                    CustomPcBuildKitRegistration>();

        private CustomPcBuildKitAuthority(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            InventorySerializedTransferAccess motherboardBuildKitAccess,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            InventorySerializedTransferAccess processorBuildKitAccess,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId,
            InventorySerializedTransferAccess memoryModuleBuildKitAccess,
            StableId<ContainerIdScope> storageBuildKitContainerId,
            InventorySerializedTransferAccess storageBuildKitAccess,
            StableId<ContainerIdScope> processorCoolerBuildKitContainerId,
            InventorySerializedTransferAccess processorCoolerBuildKitAccess,
            StableId<ContainerIdScope> graphicsCardBuildKitContainerId,
            InventorySerializedTransferAccess graphicsCardBuildKitAccess,
            StableId<ContainerIdScope> powerSupplyBuildKitContainerId,
            InventorySerializedTransferAccess powerSupplyBuildKitAccess,
            StableId<ContainerIdScope> atx24PowerCableBuildKitContainerId = default,
            InventorySerializedTransferAccess atx24PowerCableBuildKitAccess = null,
            StableId<ContainerIdScope> eps12vPowerCableBuildKitContainerId = default,
            InventorySerializedTransferAccess eps12vPowerCableBuildKitAccess = null)
        {
            _workOrders = workOrders;
            _inventory = workOrders.Inventory;
            _sourceContainerId = sourceContainerId;
            _handsContainerId = handsContainerId;
            _motherboardBuildKitContainerId = motherboardBuildKitContainerId;
            _motherboardBuildKitAccess = motherboardBuildKitAccess;
            _processorBuildKitContainerId = processorBuildKitContainerId;
            _processorBuildKitAccess = processorBuildKitAccess;
            _memoryModuleBuildKitContainerId = memoryModuleBuildKitContainerId;
            _memoryModuleBuildKitAccess = memoryModuleBuildKitAccess;
            _storageBuildKitContainerId = storageBuildKitContainerId;
            _storageBuildKitAccess = storageBuildKitAccess;
            _processorCoolerBuildKitContainerId = processorCoolerBuildKitContainerId;
            _processorCoolerBuildKitAccess = processorCoolerBuildKitAccess;
            _graphicsCardBuildKitContainerId = graphicsCardBuildKitContainerId;
            _graphicsCardBuildKitAccess = graphicsCardBuildKitAccess;
            _powerSupplyBuildKitContainerId = powerSupplyBuildKitContainerId;
            _powerSupplyBuildKitAccess = powerSupplyBuildKitAccess;
            _atx24PowerCableBuildKitContainerId = atx24PowerCableBuildKitContainerId;
            _atx24PowerCableBuildKitAccess = atx24PowerCableBuildKitAccess;
            _eps12vPowerCableBuildKitContainerId = eps12vPowerCableBuildKitContainerId;
            _eps12vPowerCableBuildKitAccess = eps12vPowerCableBuildKitAccess;
        }

        public long Revision { get; private set; }

        public int ActiveKitCount => _registrationsByOperation.Count;

        public int StagedComponentCount
        {
            get
            {
                int count = 0;
                foreach (CustomPcBuildKitRegistration registration in
                         _registrationsByOperation.Values)
                {
                    if (registration.PlacementReceipt != null)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public StableId<ContainerIdScope> HandsContainerId => _handsContainerId;

        public StableId<ContainerIdScope> SourceContainerId => _sourceContainerId;

        public StableId<ContainerIdScope> BuildKitContainerId =>
            _motherboardBuildKitContainerId;

        public StableId<ContainerIdScope> ProcessorBuildKitContainerId =>
            _processorBuildKitContainerId;

        public StableId<ContainerIdScope> MemoryModuleBuildKitContainerId =>
            _memoryModuleBuildKitContainerId;

        public StableId<ContainerIdScope> StorageBuildKitContainerId =>
            _storageBuildKitContainerId;

        public StableId<ContainerIdScope> ProcessorCoolerBuildKitContainerId =>
            _processorCoolerBuildKitContainerId;

        public StableId<ContainerIdScope> GraphicsCardBuildKitContainerId =>
            _graphicsCardBuildKitContainerId;

        public StableId<ContainerIdScope> PowerSupplyBuildKitContainerId =>
            _powerSupplyBuildKitContainerId;

        public StableId<ContainerIdScope> Atx24PowerCableBuildKitContainerId =>
            _atx24PowerCableBuildKitContainerId;

        public StableId<ContainerIdScope> Eps12vPowerCableBuildKitContainerId =>
            _eps12vPowerCableBuildKitContainerId;

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId)
        {
            return CreateSingleComponentAuthority(
                workOrders,
                sourceContainerId,
                handsContainerId,
                buildKitContainerId);
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    processorBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                motherboardBuildKitContainerId == processorBuildKitContainerId)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            OperationResult<InventorySerializedTransferAccessPair> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null));
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    processorBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    memoryModuleBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                motherboardBuildKitContainerId == processorBuildKitContainerId ||
                motherboardBuildKitContainerId == memoryModuleBuildKitContainerId ||
                processorBuildKitContainerId == memoryModuleBuildKitContainerId)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            OperationResult<InventorySerializedTransferAccessTriple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId,
                    memoryModuleBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    memoryModuleBuildKitContainerId,
                    access.Value.Third,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null));
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId,
            StableId<ContainerIdScope> storageBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    processorBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    memoryModuleBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    storageBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                motherboardBuildKitContainerId == processorBuildKitContainerId ||
                motherboardBuildKitContainerId == memoryModuleBuildKitContainerId ||
                motherboardBuildKitContainerId == storageBuildKitContainerId ||
                processorBuildKitContainerId == memoryModuleBuildKitContainerId ||
                processorBuildKitContainerId == storageBuildKitContainerId ||
                memoryModuleBuildKitContainerId == storageBuildKitContainerId)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            OperationResult<InventorySerializedTransferAccessQuadruple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId,
                    memoryModuleBuildKitContainerId,
                    storageBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    memoryModuleBuildKitContainerId,
                    access.Value.Third,
                    storageBuildKitContainerId,
                    access.Value.Fourth,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null));
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId,
            StableId<ContainerIdScope> storageBuildKitContainerId,
            StableId<ContainerIdScope> processorCoolerBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    processorBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    memoryModuleBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    storageBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    processorCoolerBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                motherboardBuildKitContainerId == processorBuildKitContainerId ||
                motherboardBuildKitContainerId == memoryModuleBuildKitContainerId ||
                motherboardBuildKitContainerId == storageBuildKitContainerId ||
                motherboardBuildKitContainerId == processorCoolerBuildKitContainerId ||
                processorBuildKitContainerId == memoryModuleBuildKitContainerId ||
                processorBuildKitContainerId == storageBuildKitContainerId ||
                processorBuildKitContainerId == processorCoolerBuildKitContainerId ||
                memoryModuleBuildKitContainerId == storageBuildKitContainerId ||
                memoryModuleBuildKitContainerId == processorCoolerBuildKitContainerId ||
                storageBuildKitContainerId == processorCoolerBuildKitContainerId)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            OperationResult<InventorySerializedTransferAccessQuintuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId,
                    memoryModuleBuildKitContainerId,
                    storageBuildKitContainerId,
                    processorCoolerBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    memoryModuleBuildKitContainerId,
                    access.Value.Third,
                    storageBuildKitContainerId,
                    access.Value.Fourth,
                    processorCoolerBuildKitContainerId,
                    access.Value.Fifth,
                    default,
                    null,
                    default,
                    null));
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId,
            StableId<ContainerIdScope> storageBuildKitContainerId,
            StableId<ContainerIdScope> processorCoolerBuildKitContainerId,
            StableId<ContainerIdScope> graphicsCardBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    processorBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    memoryModuleBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    storageBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    processorCoolerBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                !HasValidBuildKitContainer(
                    workOrders,
                    inventory,
                    graphicsCardBuildKitContainerId,
                    sourceContainerId,
                    handsContainerId) ||
                motherboardBuildKitContainerId == processorBuildKitContainerId ||
                motherboardBuildKitContainerId == memoryModuleBuildKitContainerId ||
                motherboardBuildKitContainerId == storageBuildKitContainerId ||
                motherboardBuildKitContainerId == processorCoolerBuildKitContainerId ||
                motherboardBuildKitContainerId == graphicsCardBuildKitContainerId ||
                processorBuildKitContainerId == memoryModuleBuildKitContainerId ||
                processorBuildKitContainerId == storageBuildKitContainerId ||
                processorBuildKitContainerId == processorCoolerBuildKitContainerId ||
                processorBuildKitContainerId == graphicsCardBuildKitContainerId ||
                memoryModuleBuildKitContainerId == storageBuildKitContainerId ||
                memoryModuleBuildKitContainerId == processorCoolerBuildKitContainerId ||
                memoryModuleBuildKitContainerId == graphicsCardBuildKitContainerId ||
                storageBuildKitContainerId == processorCoolerBuildKitContainerId ||
                storageBuildKitContainerId == graphicsCardBuildKitContainerId ||
                processorCoolerBuildKitContainerId == graphicsCardBuildKitContainerId)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            OperationResult<InventorySerializedTransferAccessSextuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId,
                    memoryModuleBuildKitContainerId,
                    storageBuildKitContainerId,
                    processorCoolerBuildKitContainerId,
                    graphicsCardBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    memoryModuleBuildKitContainerId,
                    access.Value.Third,
                    storageBuildKitContainerId,
                    access.Value.Fourth,
                    processorCoolerBuildKitContainerId,
                    access.Value.Fifth,
                    graphicsCardBuildKitContainerId,
                    access.Value.Sixth,
                    default,
                    null));
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId,
            StableId<ContainerIdScope> storageBuildKitContainerId,
            StableId<ContainerIdScope> processorCoolerBuildKitContainerId,
            StableId<ContainerIdScope> graphicsCardBuildKitContainerId,
            StableId<ContainerIdScope> powerSupplyBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            StableId<ContainerIdScope>[] buildKitContainerIds =
            {
                motherboardBuildKitContainerId,
                processorBuildKitContainerId,
                memoryModuleBuildKitContainerId,
                storageBuildKitContainerId,
                processorCoolerBuildKitContainerId,
                graphicsCardBuildKitContainerId,
                powerSupplyBuildKitContainerId
            };

            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId))
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            for (int index = 1; index < buildKitContainerIds.Length; index++)
            {
                if (!HasValidBuildKitContainer(
                        workOrders,
                        inventory,
                        buildKitContainerIds[index],
                        sourceContainerId,
                        handsContainerId))
                {
                    return OperationResult<CustomPcBuildKitAuthority>.Fail(
                        CustomPcWorkOrderFailures.BuildKitContainerInvalid);
                }
            }

            for (int left = 0; left < buildKitContainerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < buildKitContainerIds.Length; right++)
                {
                    if (buildKitContainerIds[left] == buildKitContainerIds[right])
                    {
                        return OperationResult<CustomPcBuildKitAuthority>.Fail(
                            CustomPcWorkOrderFailures.BuildKitContainerInvalid);
                    }
                }
            }

            OperationResult<InventorySerializedTransferAccessSeptuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId,
                    memoryModuleBuildKitContainerId,
                    storageBuildKitContainerId,
                    processorCoolerBuildKitContainerId,
                    graphicsCardBuildKitContainerId,
                    powerSupplyBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    memoryModuleBuildKitContainerId,
                    access.Value.Third,
                    storageBuildKitContainerId,
                    access.Value.Fourth,
                    processorCoolerBuildKitContainerId,
                    access.Value.Fifth,
                    graphicsCardBuildKitContainerId,
                    access.Value.Sixth,
                    powerSupplyBuildKitContainerId,
                    access.Value.Seventh));
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId,
            StableId<ContainerIdScope> storageBuildKitContainerId,
            StableId<ContainerIdScope> processorCoolerBuildKitContainerId,
            StableId<ContainerIdScope> graphicsCardBuildKitContainerId,
            StableId<ContainerIdScope> powerSupplyBuildKitContainerId,
            StableId<ContainerIdScope> atx24PowerCableBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            StableId<ContainerIdScope>[] buildKitContainerIds =
            {
                motherboardBuildKitContainerId,
                processorBuildKitContainerId,
                memoryModuleBuildKitContainerId,
                storageBuildKitContainerId,
                processorCoolerBuildKitContainerId,
                graphicsCardBuildKitContainerId,
                powerSupplyBuildKitContainerId,
                atx24PowerCableBuildKitContainerId
            };

            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId))
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            for (int index = 1; index < buildKitContainerIds.Length; index++)
            {
                if (!HasValidBuildKitContainer(
                        workOrders,
                        inventory,
                        buildKitContainerIds[index],
                        sourceContainerId,
                        handsContainerId))
                {
                    return OperationResult<CustomPcBuildKitAuthority>.Fail(
                        CustomPcWorkOrderFailures.BuildKitContainerInvalid);
                }
            }

            for (int left = 0; left < buildKitContainerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < buildKitContainerIds.Length; right++)
                {
                    if (buildKitContainerIds[left] == buildKitContainerIds[right])
                    {
                        return OperationResult<CustomPcBuildKitAuthority>.Fail(
                            CustomPcWorkOrderFailures.BuildKitContainerInvalid);
                    }
                }
            }

            OperationResult<InventorySerializedTransferAccessOctuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId,
                    memoryModuleBuildKitContainerId,
                    storageBuildKitContainerId,
                    processorCoolerBuildKitContainerId,
                    graphicsCardBuildKitContainerId,
                    powerSupplyBuildKitContainerId,
                    atx24PowerCableBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    memoryModuleBuildKitContainerId,
                    access.Value.Third,
                    storageBuildKitContainerId,
                    access.Value.Fourth,
                    processorCoolerBuildKitContainerId,
                    access.Value.Fifth,
                    graphicsCardBuildKitContainerId,
                    access.Value.Sixth,
                    powerSupplyBuildKitContainerId,
                    access.Value.Seventh,
                    atx24PowerCableBuildKitContainerId,
                    access.Value.Eighth));
        }

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> motherboardBuildKitContainerId,
            StableId<ContainerIdScope> processorBuildKitContainerId,
            StableId<ContainerIdScope> memoryModuleBuildKitContainerId,
            StableId<ContainerIdScope> storageBuildKitContainerId,
            StableId<ContainerIdScope> processorCoolerBuildKitContainerId,
            StableId<ContainerIdScope> graphicsCardBuildKitContainerId,
            StableId<ContainerIdScope> powerSupplyBuildKitContainerId,
            StableId<ContainerIdScope> atx24PowerCableBuildKitContainerId,
            StableId<ContainerIdScope> eps12vPowerCableBuildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            StableId<ContainerIdScope>[] buildKitContainerIds =
            {
                motherboardBuildKitContainerId,
                processorBuildKitContainerId,
                memoryModuleBuildKitContainerId,
                storageBuildKitContainerId,
                processorCoolerBuildKitContainerId,
                graphicsCardBuildKitContainerId,
                powerSupplyBuildKitContainerId,
                atx24PowerCableBuildKitContainerId,
                eps12vPowerCableBuildKitContainerId
            };

            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId))
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            for (int index = 1; index < buildKitContainerIds.Length; index++)
            {
                if (!HasValidBuildKitContainer(
                        workOrders,
                        inventory,
                        buildKitContainerIds[index],
                        sourceContainerId,
                        handsContainerId))
                {
                    return OperationResult<CustomPcBuildKitAuthority>.Fail(
                        CustomPcWorkOrderFailures.BuildKitContainerInvalid);
                }
            }

            for (int left = 0; left < buildKitContainerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < buildKitContainerIds.Length; right++)
                {
                    if (buildKitContainerIds[left] == buildKitContainerIds[right])
                    {
                        return OperationResult<CustomPcBuildKitAuthority>.Fail(
                            CustomPcWorkOrderFailures.BuildKitContainerInvalid);
                    }
                }
            }

            OperationResult<InventorySerializedTransferAccessNonuple> access =
                inventory.ClaimManagedSerializedTransferContainers(
                    motherboardBuildKitContainerId,
                    processorBuildKitContainerId,
                    memoryModuleBuildKitContainerId,
                    storageBuildKitContainerId,
                    processorCoolerBuildKitContainerId,
                    graphicsCardBuildKitContainerId,
                    powerSupplyBuildKitContainerId,
                    atx24PowerCableBuildKitContainerId,
                    eps12vPowerCableBuildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    motherboardBuildKitContainerId,
                    access.Value.First,
                    processorBuildKitContainerId,
                    access.Value.Second,
                    memoryModuleBuildKitContainerId,
                    access.Value.Third,
                    storageBuildKitContainerId,
                    access.Value.Fourth,
                    processorCoolerBuildKitContainerId,
                    access.Value.Fifth,
                    graphicsCardBuildKitContainerId,
                    access.Value.Sixth,
                    powerSupplyBuildKitContainerId,
                    access.Value.Seventh,
                    atx24PowerCableBuildKitContainerId,
                    access.Value.Eighth,
                    eps12vPowerCableBuildKitContainerId,
                    access.Value.Ninth));
        }

        private static OperationResult<CustomPcBuildKitAuthority>
            CreateSingleComponentAuthority(
                CustomPcWorkOrderAuthority workOrders,
                StableId<ContainerIdScope> sourceContainerId,
                StableId<ContainerIdScope> handsContainerId,
                StableId<ContainerIdScope> buildKitContainerId)
        {
            if (workOrders == null)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitAuthorityMissing);
            }

            InventoryAuthority inventory = workOrders.Inventory;
            if (!HasValidContainerTopology(
                    workOrders,
                    inventory,
                    sourceContainerId,
                    handsContainerId,
                    buildKitContainerId))
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            OperationResult<InventorySerializedTransferAccess> access =
                inventory.ClaimManagedSerializedTransferContainer(buildKitContainerId);
            if (access.IsFailure)
            {
                return OperationResult<CustomPcBuildKitAuthority>.Fail(
                    access.Error == InventoryFailures.RevisionOverflow
                        ? CustomPcWorkOrderFailures.RevisionOverflow
                        : CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return OperationResult<CustomPcBuildKitAuthority>.Success(
                new CustomPcBuildKitAuthority(
                    workOrders,
                    sourceContainerId,
                    handsContainerId,
                    buildKitContainerId,
                    access.Value,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null,
                    default,
                    null));
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalMotherboard(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.Motherboard,
                _motherboardBuildKitContainerId,
                _motherboardBuildKitAccess,
                CustomPcBuildKitStage.MotherboardInHands,
                CustomPcWorkOrderFailures.BuildKitMotherboardLineInvalid,
                requiresStagedMotherboard: false,
                requiresStagedProcessor: false,
                requiresStagedMemoryModule: false,
                requiresStagedStorage: false,
                requiresStagedProcessorCooler: false,
                requiresStagedGraphicsCard: false);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalProcessor(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_processorBuildKitContainerId.IsEmpty ||
                _processorBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.Processor,
                _processorBuildKitContainerId,
                _processorBuildKitAccess,
                CustomPcBuildKitStage.ProcessorInHands,
                CustomPcWorkOrderFailures.BuildKitProcessorLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: false,
                requiresStagedMemoryModule: false,
                requiresStagedStorage: false,
                requiresStagedProcessorCooler: false,
                requiresStagedGraphicsCard: false);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalMemoryModule(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_memoryModuleBuildKitContainerId.IsEmpty ||
                _memoryModuleBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.MemoryModule,
                _memoryModuleBuildKitContainerId,
                _memoryModuleBuildKitAccess,
                CustomPcBuildKitStage.MemoryModuleInHands,
                CustomPcWorkOrderFailures.BuildKitMemoryModuleLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: true,
                requiresStagedMemoryModule: false,
                requiresStagedStorage: false,
                requiresStagedProcessorCooler: false,
                requiresStagedGraphicsCard: false);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalStorage(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_storageBuildKitContainerId.IsEmpty ||
                _storageBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.StorageDevice,
                _storageBuildKitContainerId,
                _storageBuildKitAccess,
                CustomPcBuildKitStage.StorageInHands,
                CustomPcWorkOrderFailures.BuildKitStorageLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: true,
                requiresStagedMemoryModule: true,
                requiresStagedStorage: false,
                requiresStagedProcessorCooler: false,
                requiresStagedGraphicsCard: false);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalProcessorCooler(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_processorCoolerBuildKitContainerId.IsEmpty ||
                _processorCoolerBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.ProcessorCooler,
                _processorCoolerBuildKitContainerId,
                _processorCoolerBuildKitAccess,
                CustomPcBuildKitStage.ProcessorCoolerInHands,
                CustomPcWorkOrderFailures.BuildKitProcessorCoolerLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: true,
                requiresStagedMemoryModule: true,
                requiresStagedStorage: true,
                requiresStagedProcessorCooler: false,
                requiresStagedGraphicsCard: false);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalGraphicsCard(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_graphicsCardBuildKitContainerId.IsEmpty ||
                _graphicsCardBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.GraphicsCard,
                _graphicsCardBuildKitContainerId,
                _graphicsCardBuildKitAccess,
                CustomPcBuildKitStage.GraphicsCardInHands,
                CustomPcWorkOrderFailures.BuildKitGraphicsCardLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: true,
                requiresStagedMemoryModule: true,
                requiresStagedStorage: true,
                requiresStagedProcessorCooler: true,
                requiresStagedGraphicsCard: false);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalPowerSupply(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_powerSupplyBuildKitContainerId.IsEmpty ||
                _powerSupplyBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.PowerSupply,
                _powerSupplyBuildKitContainerId,
                _powerSupplyBuildKitAccess,
                CustomPcBuildKitStage.PowerSupplyInHands,
                CustomPcWorkOrderFailures.BuildKitPowerSupplyLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: true,
                requiresStagedMemoryModule: true,
                requiresStagedStorage: true,
                requiresStagedProcessorCooler: true,
                requiresStagedGraphicsCard: true);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalAtx24PowerCable(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_atx24PowerCableBuildKitContainerId.IsEmpty ||
                _atx24PowerCableBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.PowerCable,
                _atx24PowerCableBuildKitContainerId,
                _atx24PowerCableBuildKitAccess,
                CustomPcBuildKitStage.Atx24PowerCableInHands,
                CustomPcWorkOrderFailures.BuildKitAtx24PowerCableLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: true,
                requiresStagedMemoryModule: true,
                requiresStagedStorage: true,
                requiresStagedProcessorCooler: true,
                requiresStagedGraphicsCard: true,
                requiresStagedPowerSupply: true,
                expectedPowerCableType:
                    PowerCableType.ModularAtx24SplitPsuToMotherboard);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalEps12vPowerCable(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
        {
            if (_eps12vPowerCableBuildKitContainerId.IsEmpty ||
                _eps12vPowerCableBuildKitAccess == null)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitContainerInvalid);
            }

            return PickupCanonicalComponent(
                operationId,
                workOrder,
                PcComponentKind.PowerCable,
                _eps12vPowerCableBuildKitContainerId,
                _eps12vPowerCableBuildKitAccess,
                CustomPcBuildKitStage.Eps12vPowerCableInHands,
                CustomPcWorkOrderFailures.BuildKitEps12vPowerCableLineInvalid,
                requiresStagedMotherboard: true,
                requiresStagedProcessor: true,
                requiresStagedMemoryModule: true,
                requiresStagedStorage: true,
                requiresStagedProcessorCooler: true,
                requiresStagedGraphicsCard: true,
                requiresStagedPowerSupply: true,
                requiresStagedAtx24PowerCable: true,
                expectedPowerCableType:
                    PowerCableType.ModularEps12v8PinPsuToMotherboard);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalMotherboard(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalMotherboard(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalMotherboard(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.Motherboard,
                CustomPcBuildKitStage.MotherboardInHands,
                CustomPcBuildKitStage.MotherboardStaged);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalProcessor(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalProcessor(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalProcessor(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.Processor,
                CustomPcBuildKitStage.ProcessorInHands,
                CustomPcBuildKitStage.ProcessorStaged);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalMemoryModule(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalMemoryModule(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalMemoryModule(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.MemoryModule,
                CustomPcBuildKitStage.MemoryModuleInHands,
                CustomPcBuildKitStage.MemoryModuleStaged);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalStorage(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalStorage(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalStorage(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.StorageDevice,
                CustomPcBuildKitStage.StorageInHands,
                CustomPcBuildKitStage.StorageStaged);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalProcessorCooler(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalProcessorCooler(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalProcessorCooler(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.ProcessorCooler,
                CustomPcBuildKitStage.ProcessorCoolerInHands,
                CustomPcBuildKitStage.ProcessorCoolerStaged);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalGraphicsCard(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalGraphicsCard(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalGraphicsCard(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.GraphicsCard,
                CustomPcBuildKitStage.GraphicsCardInHands,
                CustomPcBuildKitStage.GraphicsCardStaged);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalPowerSupply(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalPowerSupply(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalPowerSupply(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.PowerSupply,
                CustomPcBuildKitStage.PowerSupplyInHands,
                CustomPcBuildKitStage.PowerSupplyStaged);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalAtx24PowerCable(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalAtx24PowerCable(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalAtx24PowerCable(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.PowerCable,
                CustomPcBuildKitStage.Atx24PowerCableInHands,
                CustomPcBuildKitStage.Atx24PowerCableStaged,
                PowerCableType.ModularAtx24SplitPsuToMotherboard);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalEps12vPowerCable(
            CustomPcBuildKitReceipt pickupReceipt)
        {
            return PlaceCanonicalEps12vPowerCable(
                pickupReceipt,
                Revision,
                _inventory.Revision);
        }

        internal OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalEps12vPowerCable(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision)
        {
            return PlaceCanonicalComponent(
                pickupReceipt,
                expectedBuildKitRevision,
                expectedInventoryRevision,
                PcComponentKind.PowerCable,
                CustomPcBuildKitStage.Eps12vPowerCableInHands,
                CustomPcBuildKitStage.Eps12vPowerCableStaged,
                PowerCableType.ModularEps12v8PinPsuToMotherboard);
        }

        private OperationResult<CustomPcBuildKitReceipt> PickupCanonicalComponent(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> buildKitContainerId,
            InventorySerializedTransferAccess buildKitAccess,
            CustomPcBuildKitStage pickupStage,
            Failure lineFailure,
            bool requiresStagedMotherboard,
            bool requiresStagedProcessor,
            bool requiresStagedMemoryModule,
            bool requiresStagedStorage,
            bool requiresStagedProcessorCooler,
            bool requiresStagedGraphicsCard,
            bool requiresStagedPowerSupply = false,
            bool requiresStagedAtx24PowerCable = false,
            PowerCableType expectedPowerCableType = default)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitOperationInvalid);
            }

            if (!_workOrders.TryGetOwnedInventoryAllocation(
                    workOrder,
                    out InventorySerializedReservationWorkOrderAllocationReceipt allocation))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitWorkOrderInvalid);
            }

            if (!TryGetCanonicalLine(
                    workOrder,
                    componentKind,
                    out CustomPcBuildOrderLineSnapshot line,
                    expectedPowerCableType))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(lineFailure);
            }

            if (requiresStagedMotherboard &&
                !HasStagedComponent(workOrder, PcComponentKind.Motherboard))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (requiresStagedProcessor &&
                !HasStagedComponent(workOrder, PcComponentKind.Processor))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (requiresStagedMemoryModule &&
                !HasStagedComponent(workOrder, PcComponentKind.MemoryModule))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (requiresStagedStorage &&
                !HasStagedComponent(workOrder, PcComponentKind.StorageDevice))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (requiresStagedProcessorCooler &&
                !HasStagedComponent(workOrder, PcComponentKind.ProcessorCooler))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (requiresStagedGraphicsCard &&
                !HasStagedComponent(workOrder, PcComponentKind.GraphicsCard))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (requiresStagedPowerSupply &&
                !HasStagedComponent(workOrder, PcComponentKind.PowerSupply))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (requiresStagedAtx24PowerCable &&
                !HasStagedComponent(
                    workOrder,
                    PcComponentKind.PowerCable,
                    PowerCableType.ModularAtx24SplitPsuToMotherboard))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitPrerequisiteMissing);
            }

            if (_registrationsByOperation.TryGetValue(
                    operationId,
                    out CustomPcBuildKitRegistration existing))
            {
                return MatchesRegistration(
                           existing,
                           operationId,
                           workOrder,
                           line) &&
                       OwnsRegistration(existing)
                    ? OperationResult<CustomPcBuildKitReceipt>.Success(
                        existing.PickupReceipt)
                    : OperationResult<CustomPcBuildKitReceipt>.Fail(
                        CustomPcWorkOrderFailures.BuildKitIdentityConflict);
            }

            var orderComponentKey = new CustomPcBuildKitOrderComponentKey(
                workOrder.Id,
                componentKind,
                line.PowerCableType);
            if (_registrationsByOrderAndComponent.ContainsKey(orderComponentKey))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitIdentityConflict);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            OperationResult<InventorySerializedReservationWorkOrderBuildKitReceipt>
                inventoryPickup = _inventory.PickupReservedWorkOrderLineForBuildKit(
                    allocation,
                    buildKitAccess,
                    ToInventoryOperationId(operationId),
                    ToInventoryLineId(line),
                    line.ProductId,
                    line.ItemId,
                    line.ReservationId,
                    line.ComponentKind,
                    _sourceContainerId,
                    _handsContainerId,
                    buildKitContainerId);
            if (inventoryPickup.IsFailure)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    inventoryPickup.Error);
            }

            var receipt = new CustomPcBuildKitReceipt(
                operationId,
                workOrder,
                line,
                inventoryPickup.Value.SourceContainerId,
                _handsContainerId,
                buildKitContainerId,
                pickupStage,
                inventoryPickup.Value.AppliedRevision);
            var registration = new CustomPcBuildKitRegistration(
                receipt,
                inventoryPickup.Value);
            _registrationsByOperation.Add(operationId, registration);
            _registrationsByOrderAndComponent.Add(orderComponentKey, registration);
            Revision++;
            return OperationResult<CustomPcBuildKitReceipt>.Success(receipt);
        }

        private OperationResult<CustomPcBuildKitReceipt> PlaceCanonicalComponent(
            CustomPcBuildKitReceipt pickupReceipt,
            long expectedBuildKitRevision,
            long expectedInventoryRevision,
            PcComponentKind componentKind,
            CustomPcBuildKitStage pickupStage,
            CustomPcBuildKitStage placementStage,
            PowerCableType expectedPowerCableType = default)
        {
            if (!OwnsReceipt(pickupReceipt) ||
                pickupReceipt.Line.ComponentKind != componentKind ||
                (componentKind == PcComponentKind.PowerCable &&
                 pickupReceipt.Line.PowerCableType != expectedPowerCableType) ||
                pickupReceipt.Stage != pickupStage)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitReceiptInvalid);
            }

            CustomPcBuildKitRegistration registration =
                _registrationsByOperation[pickupReceipt.OperationId];
            if (registration.PlacementReceipt != null)
            {
                return OwnsReceipt(registration.PlacementReceipt)
                    ? OperationResult<CustomPcBuildKitReceipt>.Success(
                        registration.PlacementReceipt)
                    : OperationResult<CustomPcBuildKitReceipt>.Fail(
                        CustomPcWorkOrderFailures.BuildKitIdentityConflict);
            }

            if (Revision != expectedBuildKitRevision ||
                _inventory.Revision != expectedInventoryRevision)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitRevisionStale);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            OperationResult<InventorySerializedReservationWorkOrderBuildKitReceipt>
                inventoryPlacement = _inventory.PlaceReservedWorkOrderLineInBuildKit(
                    registration.InventoryPickupReceipt);
            if (inventoryPlacement.IsFailure)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    inventoryPlacement.Error);
            }

            var placementReceipt = new CustomPcBuildKitReceipt(
                pickupReceipt.OperationId,
                pickupReceipt.BuildOrder,
                pickupReceipt.Line,
                pickupReceipt.SourceContainerId,
                pickupReceipt.HandsContainerId,
                pickupReceipt.BuildKitContainerId,
                placementStage,
                inventoryPlacement.Value.AppliedRevision);
            if (!registration.TryPublishPlacement(
                    placementReceipt,
                    inventoryPlacement.Value))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitIdentityConflict);
            }

            Revision++;
            return OperationResult<CustomPcBuildKitReceipt>.Success(placementReceipt);
        }

        public bool TryGetReceipt(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            out CustomPcBuildKitReceipt receipt)
        {
            if (_registrationsByOperation.TryGetValue(
                    operationId,
                    out CustomPcBuildKitRegistration registration) &&
                OwnsRegistration(registration))
            {
                receipt = registration.PlacementReceipt ?? registration.PickupReceipt;
                return true;
            }

            receipt = null;
            return false;
        }

        public OperationResult ValidateInvariants()
        {
            if (_workOrders == null ||
                _inventory == null ||
                !ReferenceEquals(_inventory, _workOrders.Inventory) ||
                _registrationsByOperation.Count !=
                    _registrationsByOrderAndComponent.Count)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.InvariantViolation);
            }

            foreach (CustomPcBuildKitRegistration registration in
                     _registrationsByOperation.Values)
            {
                if (!OwnsRegistration(registration))
                {
                    return OperationResult.Fail(
                        CustomPcWorkOrderFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private bool OwnsReceipt(CustomPcBuildKitReceipt receipt)
        {
            if (receipt == null ||
                !_registrationsByOperation.TryGetValue(
                    receipt.OperationId,
                    out CustomPcBuildKitRegistration registration) ||
                !OwnsRegistration(registration))
            {
                return false;
            }

            return IsPickupStage(receipt.Stage)
                    ? ReferenceEquals(receipt, registration.PickupReceipt)
                    : IsPlacementStage(receipt.Stage) &&
                      ReferenceEquals(receipt, registration.PlacementReceipt);
        }

        private bool OwnsRegistration(CustomPcBuildKitRegistration registration)
        {
            CustomPcBuildKitReceipt pickup = registration?.PickupReceipt;
            if (pickup == null ||
                pickup.OperationId.IsEmpty ||
                pickup.BuildOrder == null ||
                pickup.Line == null ||
                !TryGetComponentConfiguration(
                    pickup.Line.ComponentKind,
                    pickup.Line.PowerCableType,
                    out StableId<ContainerIdScope> expectedBuildKitContainerId,
                    out InventorySerializedTransferAccess expectedBuildKitAccess,
                    out CustomPcBuildKitStage expectedPickupStage,
                    out CustomPcBuildKitStage expectedPlacementStage) ||
                pickup.HandsContainerId != _handsContainerId ||
                pickup.BuildKitContainerId != expectedBuildKitContainerId ||
                pickup.Stage != expectedPickupStage ||
                pickup.InventoryAppliedRevision !=
                    registration.InventoryPickupReceipt?.AppliedRevision ||
                !_workOrders.TryGetOwnedInventoryAllocation(
                    pickup.BuildOrder,
                    out InventorySerializedReservationWorkOrderAllocationReceipt allocation) ||
                !MatchesInventoryReceiptIdentity(
                    registration.InventoryPickupReceipt,
                    allocation,
                    expectedBuildKitAccess,
                    pickup) ||
                !_inventory.OwnsWorkOrderBuildKitReceipt(
                    registration.InventoryPickupReceipt) ||
                !_registrationsByOperation.TryGetValue(
                    pickup.OperationId,
                    out CustomPcBuildKitRegistration byOperation) ||
                !_registrationsByOrderAndComponent.TryGetValue(
                    new CustomPcBuildKitOrderComponentKey(
                        pickup.BuildOrder.Id,
                        pickup.Line.ComponentKind,
                        pickup.Line.PowerCableType),
                    out CustomPcBuildKitRegistration byOrder) ||
                !ReferenceEquals(registration, byOperation) ||
                !ReferenceEquals(registration, byOrder) ||
                (pickup.Line.ComponentKind == PcComponentKind.Processor &&
                 !HasStagedComponent(
                     pickup.BuildOrder,
                     PcComponentKind.Motherboard)) ||
                (pickup.Line.ComponentKind == PcComponentKind.MemoryModule &&
                 (!HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Motherboard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Processor))) ||
                (pickup.Line.ComponentKind == PcComponentKind.StorageDevice &&
                 (!HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Motherboard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Processor) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.MemoryModule))) ||
                (pickup.Line.ComponentKind == PcComponentKind.ProcessorCooler &&
                 (!HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Motherboard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Processor) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.MemoryModule) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.StorageDevice))) ||
                (pickup.Line.ComponentKind == PcComponentKind.GraphicsCard &&
                 (!HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Motherboard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Processor) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.MemoryModule) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.StorageDevice) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.ProcessorCooler))) ||
                (pickup.Line.ComponentKind == PcComponentKind.PowerSupply &&
                 (!HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Motherboard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Processor) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.MemoryModule) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.StorageDevice) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.ProcessorCooler) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.GraphicsCard))) ||
                (pickup.Line.ComponentKind == PcComponentKind.PowerCable &&
                 pickup.Line.PowerCableType ==
                     PowerCableType.ModularAtx24SplitPsuToMotherboard &&
                 (!HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Motherboard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Processor) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.MemoryModule) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.StorageDevice) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.ProcessorCooler) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.GraphicsCard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.PowerSupply)))
                ||
                (pickup.Line.ComponentKind == PcComponentKind.PowerCable &&
                 pickup.Line.PowerCableType ==
                     PowerCableType.ModularEps12v8PinPsuToMotherboard &&
                 (!HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Motherboard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.Processor) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.MemoryModule) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.StorageDevice) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.ProcessorCooler) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.GraphicsCard) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.PowerSupply) ||
                  !HasStagedComponent(
                      pickup.BuildOrder,
                      PcComponentKind.PowerCable,
                      PowerCableType.ModularAtx24SplitPsuToMotherboard))))
            {
                return false;
            }

            CustomPcBuildKitReceipt placement = registration.PlacementReceipt;
            if (placement == null)
            {
                return registration.InventoryPlacementReceipt == null;
            }

            return registration.InventoryPlacementReceipt != null &&
                   placement.Stage == expectedPlacementStage &&
                   placement.InventoryAppliedRevision ==
                       registration.InventoryPlacementReceipt.AppliedRevision &&
                   MatchesReceiptIdentity(pickup, placement) &&
                   MatchesInventoryReceiptIdentity(
                       registration.InventoryPlacementReceipt,
                       allocation,
                       expectedBuildKitAccess,
                       placement) &&
                   _inventory.OwnsWorkOrderBuildKitReceipt(
                       registration.InventoryPlacementReceipt);
        }

        private static bool TryGetCanonicalLine(
            CustomPcBuildOrderRecord workOrder,
            PcComponentKind componentKind,
            out CustomPcBuildOrderLineSnapshot canonicalLine,
            PowerCableType powerCableType = default)
        {
            canonicalLine = null;
            if (workOrder?.Lines == null)
            {
                return false;
            }

            int matchCount = 0;
            for (int index = 0; index < workOrder.Lines.Count; index++)
            {
                CustomPcBuildOrderLineSnapshot line = workOrder.Lines[index];
                if (line != null &&
                    line.ComponentKind == componentKind &&
                    (componentKind != PcComponentKind.PowerCable ||
                     line.PowerCableType == powerCableType))
                {
                    canonicalLine = line;
                    matchCount++;
                }
            }

            return matchCount == 1 &&
                   canonicalLine != null &&
                   !canonicalLine.LineId.IsEmpty &&
                   !canonicalLine.ProductId.IsEmpty &&
                   !canonicalLine.ItemId.IsEmpty &&
                   !canonicalLine.ReservationId.IsEmpty &&
                   (componentKind != PcComponentKind.PowerCable ||
                    canonicalLine.PowerCableType == powerCableType &&
                    powerCableType != default);
        }

        private bool HasStagedComponent(
            CustomPcBuildOrderRecord workOrder,
            PcComponentKind componentKind,
            PowerCableType powerCableType = default)
        {
            if (workOrder == null ||
                !_registrationsByOrderAndComponent.TryGetValue(
                    new CustomPcBuildKitOrderComponentKey(
                        workOrder.Id,
                        componentKind,
                        powerCableType),
                    out CustomPcBuildKitRegistration registration) ||
                !OwnsRegistration(registration) ||
                registration.PlacementReceipt == null)
            {
                return false;
            }

            return componentKind == PcComponentKind.Motherboard
                ? registration.PlacementReceipt.Stage ==
                  CustomPcBuildKitStage.MotherboardStaged
                : componentKind == PcComponentKind.Processor
                    ? registration.PlacementReceipt.Stage ==
                      CustomPcBuildKitStage.ProcessorStaged
                    : componentKind == PcComponentKind.MemoryModule
                        ? registration.PlacementReceipt.Stage ==
                          CustomPcBuildKitStage.MemoryModuleStaged
                        : componentKind == PcComponentKind.StorageDevice
                            ? registration.PlacementReceipt.Stage ==
                              CustomPcBuildKitStage.StorageStaged
                            : componentKind == PcComponentKind.ProcessorCooler
                                ? registration.PlacementReceipt.Stage ==
                                  CustomPcBuildKitStage.ProcessorCoolerStaged
                                : componentKind == PcComponentKind.GraphicsCard
                                    ? registration.PlacementReceipt.Stage ==
                                      CustomPcBuildKitStage.GraphicsCardStaged
                                    : componentKind == PcComponentKind.PowerSupply
                                        ? registration.PlacementReceipt.Stage ==
                                          CustomPcBuildKitStage.PowerSupplyStaged
                                        : componentKind == PcComponentKind.PowerCable &&
                                          powerCableType ==
                                          PowerCableType
                                              .ModularAtx24SplitPsuToMotherboard &&
                                          registration.PlacementReceipt.Stage ==
                                          CustomPcBuildKitStage
                                              .Atx24PowerCableStaged ||
                                          componentKind == PcComponentKind.PowerCable &&
                                          powerCableType ==
                                          PowerCableType
                                              .ModularEps12v8PinPsuToMotherboard &&
                                          registration.PlacementReceipt.Stage ==
                                          CustomPcBuildKitStage
                                              .Eps12vPowerCableStaged;
        }

        private bool TryGetComponentConfiguration(
            PcComponentKind componentKind,
            PowerCableType powerCableType,
            out StableId<ContainerIdScope> buildKitContainerId,
            out InventorySerializedTransferAccess buildKitAccess,
            out CustomPcBuildKitStage pickupStage,
            out CustomPcBuildKitStage placementStage)
        {
            if (componentKind == PcComponentKind.Motherboard)
            {
                buildKitContainerId = _motherboardBuildKitContainerId;
                buildKitAccess = _motherboardBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.MotherboardInHands;
                placementStage = CustomPcBuildKitStage.MotherboardStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.Processor)
            {
                buildKitContainerId = _processorBuildKitContainerId;
                buildKitAccess = _processorBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.ProcessorInHands;
                placementStage = CustomPcBuildKitStage.ProcessorStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.MemoryModule)
            {
                buildKitContainerId = _memoryModuleBuildKitContainerId;
                buildKitAccess = _memoryModuleBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.MemoryModuleInHands;
                placementStage = CustomPcBuildKitStage.MemoryModuleStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.StorageDevice)
            {
                buildKitContainerId = _storageBuildKitContainerId;
                buildKitAccess = _storageBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.StorageInHands;
                placementStage = CustomPcBuildKitStage.StorageStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.ProcessorCooler)
            {
                buildKitContainerId = _processorCoolerBuildKitContainerId;
                buildKitAccess = _processorCoolerBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.ProcessorCoolerInHands;
                placementStage = CustomPcBuildKitStage.ProcessorCoolerStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.GraphicsCard)
            {
                buildKitContainerId = _graphicsCardBuildKitContainerId;
                buildKitAccess = _graphicsCardBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.GraphicsCardInHands;
                placementStage = CustomPcBuildKitStage.GraphicsCardStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.PowerSupply)
            {
                buildKitContainerId = _powerSupplyBuildKitContainerId;
                buildKitAccess = _powerSupplyBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.PowerSupplyInHands;
                placementStage = CustomPcBuildKitStage.PowerSupplyStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.PowerCable &&
                powerCableType ==
                    PowerCableType.ModularAtx24SplitPsuToMotherboard)
            {
                buildKitContainerId = _atx24PowerCableBuildKitContainerId;
                buildKitAccess = _atx24PowerCableBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.Atx24PowerCableInHands;
                placementStage = CustomPcBuildKitStage.Atx24PowerCableStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            if (componentKind == PcComponentKind.PowerCable &&
                powerCableType ==
                    PowerCableType.ModularEps12v8PinPsuToMotherboard)
            {
                buildKitContainerId = _eps12vPowerCableBuildKitContainerId;
                buildKitAccess = _eps12vPowerCableBuildKitAccess;
                pickupStage = CustomPcBuildKitStage.Eps12vPowerCableInHands;
                placementStage = CustomPcBuildKitStage.Eps12vPowerCableStaged;
                return !buildKitContainerId.IsEmpty && buildKitAccess != null;
            }

            buildKitContainerId = default;
            buildKitAccess = null;
            pickupStage = default;
            placementStage = default;
            return false;
        }

        private static bool IsPickupStage(CustomPcBuildKitStage stage)
        {
            return stage == CustomPcBuildKitStage.MotherboardInHands ||
                   stage == CustomPcBuildKitStage.ProcessorInHands ||
                   stage == CustomPcBuildKitStage.MemoryModuleInHands ||
                   stage == CustomPcBuildKitStage.StorageInHands ||
                   stage == CustomPcBuildKitStage.ProcessorCoolerInHands ||
                   stage == CustomPcBuildKitStage.GraphicsCardInHands ||
                   stage == CustomPcBuildKitStage.PowerSupplyInHands ||
                   stage == CustomPcBuildKitStage.Atx24PowerCableInHands ||
                   stage == CustomPcBuildKitStage.Eps12vPowerCableInHands;
        }

        private static bool IsPlacementStage(CustomPcBuildKitStage stage)
        {
            return stage == CustomPcBuildKitStage.MotherboardStaged ||
                   stage == CustomPcBuildKitStage.ProcessorStaged ||
                   stage == CustomPcBuildKitStage.MemoryModuleStaged ||
                   stage == CustomPcBuildKitStage.StorageStaged ||
                   stage == CustomPcBuildKitStage.ProcessorCoolerStaged ||
                   stage == CustomPcBuildKitStage.GraphicsCardStaged ||
                   stage == CustomPcBuildKitStage.PowerSupplyStaged ||
                   stage == CustomPcBuildKitStage.Atx24PowerCableStaged ||
                   stage == CustomPcBuildKitStage.Eps12vPowerCableStaged;
        }

        private static bool MatchesInventoryReceiptIdentity(
            InventorySerializedReservationWorkOrderBuildKitReceipt inventoryReceipt,
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            InventorySerializedTransferAccess buildKitAccess,
            CustomPcBuildKitReceipt receipt)
        {
            return inventoryReceipt != null &&
                   receipt != null &&
                   ReferenceEquals(inventoryReceipt.Allocation, allocation) &&
                   ReferenceEquals(inventoryReceipt.BuildKitAccess, buildKitAccess) &&
                   inventoryReceipt.OperationId ==
                       ToInventoryOperationId(receipt.OperationId) &&
                   inventoryReceipt.LineId == ToInventoryLineId(receipt.Line) &&
                   inventoryReceipt.ProductId == receipt.Line.ProductId &&
                   inventoryReceipt.ItemId == receipt.Line.ItemId &&
                   inventoryReceipt.ReservationId == receipt.Line.ReservationId &&
                   inventoryReceipt.ComponentKind == receipt.Line.ComponentKind &&
                   inventoryReceipt.SourceContainerId == receipt.SourceContainerId &&
                   inventoryReceipt.HandsContainerId == receipt.HandsContainerId &&
                   inventoryReceipt.BuildKitContainerId == receipt.BuildKitContainerId;
        }

        private static bool HasValidContainerTopology(
            CustomPcWorkOrderAuthority workOrders,
            InventoryAuthority inventory,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId)
        {
            return sourceContainerId.IsEmpty == false &&
                   inventory.TryGetContainer(
                       sourceContainerId,
                       out InventoryContainerDefinition source) &&
                   source.Kind == InventoryContainerKind.WorldFloor &&
                   handsContainerId.IsEmpty == false &&
                   inventory.TryGetContainer(
                       handsContainerId,
                       out InventoryContainerDefinition hands) &&
                   hands.Kind == InventoryContainerKind.ActorHands &&
                   hands.UnitCapacity == 1 &&
                   HasValidBuildKitContainer(
                       workOrders,
                       inventory,
                       buildKitContainerId,
                       sourceContainerId,
                       handsContainerId);
        }

        private static bool HasValidBuildKitContainer(
            CustomPcWorkOrderAuthority workOrders,
            InventoryAuthority inventory,
            StableId<ContainerIdScope> buildKitContainerId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId)
        {
            return buildKitContainerId.IsEmpty == false &&
                   inventory.TryGetContainer(
                       buildKitContainerId,
                       out InventoryContainerDefinition buildKit) &&
                   buildKit.Kind == InventoryContainerKind.BuildKit &&
                   buildKit.UnitCapacity == 1 &&
                   sourceContainerId != handsContainerId &&
                   sourceContainerId != buildKitContainerId &&
                   handsContainerId != buildKitContainerId &&
                   buildKitContainerId != workOrders.WorkbenchContainerId;
        }

        private static bool MatchesRegistration(
            CustomPcBuildKitRegistration registration,
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder,
            CustomPcBuildOrderLineSnapshot canonicalLine)
        {
            CustomPcBuildKitReceipt pickup = registration?.PickupReceipt;
            return pickup != null &&
                   pickup.OperationId == operationId &&
                   ReferenceEquals(pickup.BuildOrder, workOrder) &&
                   ReferenceEquals(pickup.Line, canonicalLine);
        }

        private static bool MatchesReceiptIdentity(
            CustomPcBuildKitReceipt expected,
            CustomPcBuildKitReceipt actual)
        {
            return expected != null &&
                   actual != null &&
                   expected.OperationId == actual.OperationId &&
                   ReferenceEquals(expected.BuildOrder, actual.BuildOrder) &&
                   ReferenceEquals(expected.Line, actual.Line) &&
                   expected.SourceContainerId == actual.SourceContainerId &&
                   expected.HandsContainerId == actual.HandsContainerId &&
                   expected.BuildKitContainerId == actual.BuildKitContainerId;
        }

        private static StableId<
            InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
            ToInventoryOperationId(
                StableId<CustomPcBuildKitOperationIdScope> operationId)
        {
            return StableId<
                InventorySerializedReservationWorkOrderBuildKitOperationIdScope>.Parse(
                operationId.Value);
        }

        private static StableId<InventorySerializedReservationWorkOrderLineIdScope>
            ToInventoryLineId(CustomPcBuildOrderLineSnapshot line)
        {
            return StableId<InventorySerializedReservationWorkOrderLineIdScope>.Parse(
                line.LineId.Value);
        }
    }
}
