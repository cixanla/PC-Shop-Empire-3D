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
            PcComponentKind componentKind)
        {
            BuildOrderId = buildOrderId;
            ComponentKind = componentKind;
        }

        internal StableId<CustomPcBuildOrderIdScope> BuildOrderId { get; }

        internal PcComponentKind ComponentKind { get; }

        public bool Equals(CustomPcBuildKitOrderComponentKey other)
        {
            return BuildOrderId == other.BuildOrderId &&
                   ComponentKind == other.ComponentKind;
        }

        public override bool Equals(object obj)
        {
            return obj is CustomPcBuildKitOrderComponentKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BuildOrderId, (int)ComponentKind);
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
            InventorySerializedTransferAccess processorBuildKitAccess)
        {
            _workOrders = workOrders;
            _inventory = workOrders.Inventory;
            _sourceContainerId = sourceContainerId;
            _handsContainerId = handsContainerId;
            _motherboardBuildKitContainerId = motherboardBuildKitContainerId;
            _motherboardBuildKitAccess = motherboardBuildKitAccess;
            _processorBuildKitContainerId = processorBuildKitContainerId;
            _processorBuildKitAccess = processorBuildKitAccess;
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
                    access.Value.Second));
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
                requiresStagedMotherboard: false);
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
                requiresStagedMotherboard: true);
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

        private OperationResult<CustomPcBuildKitReceipt> PickupCanonicalComponent(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> buildKitContainerId,
            InventorySerializedTransferAccess buildKitAccess,
            CustomPcBuildKitStage pickupStage,
            Failure lineFailure,
            bool requiresStagedMotherboard)
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
                    out CustomPcBuildOrderLineSnapshot line))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(lineFailure);
            }

            if (requiresStagedMotherboard &&
                !HasStagedComponent(workOrder, PcComponentKind.Motherboard))
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
                componentKind);
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
            CustomPcBuildKitStage placementStage)
        {
            if (!OwnsReceipt(pickupReceipt) ||
                pickupReceipt.Line.ComponentKind != componentKind ||
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
                        pickup.Line.ComponentKind),
                    out CustomPcBuildKitRegistration byOrder) ||
                !ReferenceEquals(registration, byOperation) ||
                !ReferenceEquals(registration, byOrder) ||
                (pickup.Line.ComponentKind == PcComponentKind.Processor &&
                 !HasStagedComponent(
                     pickup.BuildOrder,
                     PcComponentKind.Motherboard)))
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
            out CustomPcBuildOrderLineSnapshot canonicalLine)
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
                if (line != null && line.ComponentKind == componentKind)
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
                   !canonicalLine.ReservationId.IsEmpty;
        }

        private bool HasStagedComponent(
            CustomPcBuildOrderRecord workOrder,
            PcComponentKind componentKind)
        {
            if (workOrder == null ||
                !_registrationsByOrderAndComponent.TryGetValue(
                    new CustomPcBuildKitOrderComponentKey(
                        workOrder.Id,
                        componentKind),
                    out CustomPcBuildKitRegistration registration) ||
                !OwnsRegistration(registration) ||
                registration.PlacementReceipt == null)
            {
                return false;
            }

            return componentKind == PcComponentKind.Motherboard
                ? registration.PlacementReceipt.Stage ==
                  CustomPcBuildKitStage.MotherboardStaged
                : componentKind == PcComponentKind.Processor &&
                  registration.PlacementReceipt.Stage ==
                  CustomPcBuildKitStage.ProcessorStaged;
        }

        private bool TryGetComponentConfiguration(
            PcComponentKind componentKind,
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

            buildKitContainerId = default;
            buildKitAccess = null;
            pickupStage = default;
            placementStage = default;
            return false;
        }

        private static bool IsPickupStage(CustomPcBuildKitStage stage)
        {
            return stage == CustomPcBuildKitStage.MotherboardInHands ||
                   stage == CustomPcBuildKitStage.ProcessorInHands;
        }

        private static bool IsPlacementStage(CustomPcBuildKitStage stage)
        {
            return stage == CustomPcBuildKitStage.MotherboardStaged ||
                   stage == CustomPcBuildKitStage.ProcessorStaged;
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
