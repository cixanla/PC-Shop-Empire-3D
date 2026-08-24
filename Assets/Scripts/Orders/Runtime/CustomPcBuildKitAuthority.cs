using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Orders
{
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
        private readonly StableId<ContainerIdScope> _buildKitContainerId;
        private readonly InventorySerializedTransferAccess _buildKitAccess;
        private readonly Dictionary<StableId<CustomPcBuildKitOperationIdScope>,
            CustomPcBuildKitRegistration> _registrationsByOperation =
                new Dictionary<StableId<CustomPcBuildKitOperationIdScope>,
                    CustomPcBuildKitRegistration>();
        private readonly Dictionary<StableId<CustomPcBuildOrderIdScope>,
            CustomPcBuildKitRegistration> _registrationsByOrder =
                new Dictionary<StableId<CustomPcBuildOrderIdScope>,
                    CustomPcBuildKitRegistration>();

        private CustomPcBuildKitAuthority(
            CustomPcWorkOrderAuthority workOrders,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId,
            InventorySerializedTransferAccess buildKitAccess)
        {
            _workOrders = workOrders;
            _inventory = workOrders.Inventory;
            _sourceContainerId = sourceContainerId;
            _handsContainerId = handsContainerId;
            _buildKitContainerId = buildKitContainerId;
            _buildKitAccess = buildKitAccess;
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

        public StableId<ContainerIdScope> BuildKitContainerId => _buildKitContainerId;

        internal static OperationResult<CustomPcBuildKitAuthority> Create(
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
            if (sourceContainerId.IsEmpty ||
                !inventory.TryGetContainer(
                    sourceContainerId,
                    out InventoryContainerDefinition source) ||
                source.Kind != InventoryContainerKind.WorldFloor ||
                handsContainerId.IsEmpty ||
                !inventory.TryGetContainer(
                    handsContainerId,
                    out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands ||
                hands.UnitCapacity != 1 ||
                buildKitContainerId.IsEmpty ||
                !inventory.TryGetContainer(
                    buildKitContainerId,
                    out InventoryContainerDefinition buildKit) ||
                buildKit.Kind != InventoryContainerKind.BuildKit ||
                buildKit.UnitCapacity != 1 ||
                sourceContainerId == handsContainerId ||
                sourceContainerId == buildKitContainerId ||
                handsContainerId == buildKitContainerId ||
                buildKitContainerId == workOrders.WorkbenchContainerId)
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
                    access.Value));
        }

        internal OperationResult<CustomPcBuildKitReceipt> PickupCanonicalMotherboard(
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder)
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

            if (!TryGetCanonicalMotherboardLine(
                    workOrder,
                    out CustomPcBuildOrderLineSnapshot motherboardLine))
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    CustomPcWorkOrderFailures.BuildKitMotherboardLineInvalid);
            }

            if (_registrationsByOperation.TryGetValue(
                    operationId,
                    out CustomPcBuildKitRegistration existing))
            {
                return MatchesRegistration(
                           existing,
                           operationId,
                           workOrder,
                           motherboardLine) &&
                       OwnsRegistration(existing)
                    ? OperationResult<CustomPcBuildKitReceipt>.Success(
                        existing.PickupReceipt)
                    : OperationResult<CustomPcBuildKitReceipt>.Fail(
                        CustomPcWorkOrderFailures.BuildKitIdentityConflict);
            }

            if (_registrationsByOrder.ContainsKey(workOrder.Id))
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
                    _buildKitAccess,
                    ToInventoryOperationId(operationId),
                    ToInventoryLineId(motherboardLine),
                    motherboardLine.ProductId,
                    motherboardLine.ItemId,
                    motherboardLine.ReservationId,
                    motherboardLine.ComponentKind,
                    _sourceContainerId,
                    _handsContainerId,
                    _buildKitContainerId);
            if (inventoryPickup.IsFailure)
            {
                return OperationResult<CustomPcBuildKitReceipt>.Fail(
                    inventoryPickup.Error);
            }

            var receipt = new CustomPcBuildKitReceipt(
                operationId,
                workOrder,
                motherboardLine,
                inventoryPickup.Value.SourceContainerId,
                _handsContainerId,
                _buildKitContainerId,
                CustomPcBuildKitStage.MotherboardInHands,
                inventoryPickup.Value.AppliedRevision);
            var registration = new CustomPcBuildKitRegistration(
                receipt,
                inventoryPickup.Value);
            _registrationsByOperation.Add(operationId, registration);
            _registrationsByOrder.Add(workOrder.Id, registration);
            Revision++;
            return OperationResult<CustomPcBuildKitReceipt>.Success(receipt);
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
            if (!OwnsReceipt(pickupReceipt) ||
                pickupReceipt.Stage != CustomPcBuildKitStage.MotherboardInHands)
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
                CustomPcBuildKitStage.MotherboardStaged,
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
                _registrationsByOperation.Count != _registrationsByOrder.Count)
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

            return receipt.Stage == CustomPcBuildKitStage.MotherboardInHands
                    ? ReferenceEquals(receipt, registration.PickupReceipt)
                    : receipt.Stage == CustomPcBuildKitStage.MotherboardStaged &&
                      ReferenceEquals(receipt, registration.PlacementReceipt);
        }

        private bool OwnsRegistration(CustomPcBuildKitRegistration registration)
        {
            CustomPcBuildKitReceipt pickup = registration?.PickupReceipt;
            if (pickup == null ||
                pickup.OperationId.IsEmpty ||
                pickup.BuildOrder == null ||
                pickup.Line == null ||
                pickup.Line.ComponentKind != PcComponentKind.Motherboard ||
                pickup.HandsContainerId != _handsContainerId ||
                pickup.BuildKitContainerId != _buildKitContainerId ||
                pickup.Stage != CustomPcBuildKitStage.MotherboardInHands ||
                pickup.InventoryAppliedRevision !=
                    registration.InventoryPickupReceipt?.AppliedRevision ||
                !_workOrders.TryGetOwnedInventoryAllocation(
                    pickup.BuildOrder,
                    out _) ||
                !_inventory.OwnsWorkOrderBuildKitReceipt(
                    registration.InventoryPickupReceipt) ||
                !_registrationsByOperation.TryGetValue(
                    pickup.OperationId,
                    out CustomPcBuildKitRegistration byOperation) ||
                !_registrationsByOrder.TryGetValue(
                    pickup.BuildOrder.Id,
                    out CustomPcBuildKitRegistration byOrder) ||
                !ReferenceEquals(registration, byOperation) ||
                !ReferenceEquals(registration, byOrder))
            {
                return false;
            }

            CustomPcBuildKitReceipt placement = registration.PlacementReceipt;
            if (placement == null)
            {
                return registration.InventoryPlacementReceipt == null;
            }

            return registration.InventoryPlacementReceipt != null &&
                   placement.Stage == CustomPcBuildKitStage.MotherboardStaged &&
                   placement.InventoryAppliedRevision ==
                       registration.InventoryPlacementReceipt.AppliedRevision &&
                   MatchesReceiptIdentity(pickup, placement) &&
                   _inventory.OwnsWorkOrderBuildKitReceipt(
                       registration.InventoryPlacementReceipt);
        }

        private static bool TryGetCanonicalMotherboardLine(
            CustomPcBuildOrderRecord workOrder,
            out CustomPcBuildOrderLineSnapshot motherboardLine)
        {
            motherboardLine = null;
            if (workOrder?.Lines == null)
            {
                return false;
            }

            int matchCount = 0;
            for (int index = 0; index < workOrder.Lines.Count; index++)
            {
                CustomPcBuildOrderLineSnapshot line = workOrder.Lines[index];
                if (line != null && line.ComponentKind == PcComponentKind.Motherboard)
                {
                    motherboardLine = line;
                    matchCount++;
                }
            }

            return matchCount == 1 &&
                   motherboardLine != null &&
                   !motherboardLine.LineId.IsEmpty &&
                   !motherboardLine.ProductId.IsEmpty &&
                   !motherboardLine.ItemId.IsEmpty &&
                   !motherboardLine.ReservationId.IsEmpty;
        }

        private static bool MatchesRegistration(
            CustomPcBuildKitRegistration registration,
            StableId<CustomPcBuildKitOperationIdScope> operationId,
            CustomPcBuildOrderRecord workOrder,
            CustomPcBuildOrderLineSnapshot motherboardLine)
        {
            CustomPcBuildKitReceipt pickup = registration?.PickupReceipt;
            return pickup != null &&
                   pickup.OperationId == operationId &&
                   ReferenceEquals(pickup.BuildOrder, workOrder) &&
                   ReferenceEquals(pickup.Line, motherboardLine);
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
