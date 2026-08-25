using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Inventory
{
    internal enum InventorySerializedReservationWorkOrderBuildKitStage
    {
        ActorHands = 1,
        BuildKit = 2
    }

    /// <summary>
    /// Immutable Inventory proof for one exact reserved work-order line moving through the
    /// physical build-kit handoff. The reservation and parent allocation remain live.
    /// </summary>
    internal sealed class InventorySerializedReservationWorkOrderBuildKitReceipt
    {
        internal InventorySerializedReservationWorkOrderBuildKitReceipt(
            InventoryAuthority owner,
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            InventorySerializedTransferAccess buildKitAccess,
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId,
            InventorySerializedReservationWorkOrderBuildKitStage stage,
            long appliedRevision)
        {
            Owner = owner;
            Allocation = allocation;
            BuildKitAccess = buildKitAccess;
            OperationId = operationId;
            LineId = lineId;
            ProductId = productId;
            ItemId = itemId;
            ReservationId = reservationId;
            ComponentKind = componentKind;
            SourceContainerId = sourceContainerId;
            HandsContainerId = handsContainerId;
            BuildKitContainerId = buildKitContainerId;
            Stage = stage;
            AppliedRevision = appliedRevision;
        }

        internal InventoryAuthority Owner { get; }

        internal InventorySerializedReservationWorkOrderAllocationReceipt Allocation { get; }

        internal InventorySerializedTransferAccess BuildKitAccess { get; }

        internal StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
            OperationId { get; }

        internal StableId<InventorySerializedReservationWorkOrderLineIdScope> LineId { get; }

        internal StableId<ProductDefinitionIdScope> ProductId { get; }

        internal StableId<ItemInstanceIdScope> ItemId { get; }

        internal StableId<ReservationIdScope> ReservationId { get; }

        internal PcComponentKind ComponentKind { get; }

        internal StableId<ContainerIdScope> SourceContainerId { get; }

        internal StableId<ContainerIdScope> HandsContainerId { get; }

        internal StableId<ContainerIdScope> BuildKitContainerId { get; }

        internal InventorySerializedReservationWorkOrderBuildKitStage Stage { get; }

        internal long AppliedRevision { get; }
    }

    internal sealed class InventorySerializedReservationWorkOrderBuildKitRegistration
    {
        internal InventorySerializedReservationWorkOrderBuildKitRegistration(
            InventorySerializedReservationWorkOrderBuildKitReceipt pickupReceipt)
        {
            PickupReceipt = pickupReceipt;
        }

        internal InventorySerializedReservationWorkOrderBuildKitReceipt PickupReceipt { get; }

        internal InventorySerializedReservationWorkOrderBuildKitReceipt PlacementReceipt
        {
            get;
            private set;
        }

        internal InventorySerializedReservationWorkOrderBuildKitStage CurrentStage =>
            PlacementReceipt == null
                ? InventorySerializedReservationWorkOrderBuildKitStage.ActorHands
                : InventorySerializedReservationWorkOrderBuildKitStage.BuildKit;

        internal bool TryPublishPlacement(
            InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt)
        {
            if (PlacementReceipt != null ||
                placementReceipt == null ||
                placementReceipt.Stage !=
                InventorySerializedReservationWorkOrderBuildKitStage.BuildKit)
            {
                return false;
            }

            PlacementReceipt = placementReceipt;
            return true;
        }
    }

    public sealed partial class InventoryAuthority
    {
        private readonly Dictionary<
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>,
            InventorySerializedReservationWorkOrderBuildKitRegistration>
            _serializedReservationWorkOrderBuildKitsByOperation =
                new Dictionary<
                    StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>,
                    InventorySerializedReservationWorkOrderBuildKitRegistration>();
        private readonly Dictionary<StableId<ItemInstanceIdScope>,
            InventorySerializedReservationWorkOrderBuildKitRegistration>
            _serializedReservationWorkOrderBuildKitsByItem =
                new Dictionary<StableId<ItemInstanceIdScope>,
                    InventorySerializedReservationWorkOrderBuildKitRegistration>();
        private readonly Dictionary<StableId<ContainerIdScope>,
            InventorySerializedReservationWorkOrderBuildKitRegistration>
            _serializedReservationWorkOrderBuildKitsByContainer =
                new Dictionary<StableId<ContainerIdScope>,
                    InventorySerializedReservationWorkOrderBuildKitRegistration>();

        internal int SerializedReservationWorkOrderBuildKitCount =>
            _serializedReservationWorkOrderBuildKitsByOperation.Count;

        /// <summary>
        /// Moves one exact supported reserved PC component from its current un-managed stock/world
        /// container to ActorHands. Exact replay returns the original receipt without advancing
        /// Inventory Revision. The dedicated BuildKit capability and parent work-order
        /// allocation are bound before the first custody mutation.
        /// </summary>
        internal OperationResult<InventorySerializedReservationWorkOrderBuildKitReceipt>
            PickupReservedWorkOrderLineForBuildKit(
                InventorySerializedReservationWorkOrderAllocationReceipt allocation,
                InventorySerializedTransferAccess buildKitAccess,
                StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                    operationId,
                StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
                StableId<ProductDefinitionIdScope> productId,
                StableId<ItemInstanceIdScope> itemId,
                StableId<ReservationIdScope> reservationId,
                PcComponentKind componentKind,
                StableId<ContainerIdScope> sourceContainerId,
                StableId<ContainerIdScope> handsContainerId,
                StableId<ContainerIdScope> buildKitContainerId)
        {
            Failure inputFailure = ValidateWorkOrderBuildKitInput(
                allocation,
                buildKitAccess,
                operationId,
                lineId,
                productId,
                itemId,
                reservationId,
                componentKind,
                sourceContainerId,
                handsContainerId,
                buildKitContainerId);
            if (!inputFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(inputFailure);
            }

            if (_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    operationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration existing))
            {
                return MatchesWorkOrderBuildKitRegistration(
                           existing,
                           allocation,
                           buildKitAccess,
                           operationId,
                           lineId,
                           productId,
                           itemId,
                           reservationId,
                           componentKind,
                           sourceContainerId,
                           handsContainerId,
                           buildKitContainerId) &&
                       OwnsWorkOrderBuildKitRegistration(existing)
                    ? OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(
                        existing.PickupReceipt)
                    : OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                        InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            if (_serializedReservationWorkOrderBuildKitsByItem.ContainsKey(itemId) ||
                _serializedReservationWorkOrderBuildKitsByContainer.ContainsKey(
                    buildKitContainerId))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            InventoryItemRecord item = _items[itemId];
            if (item.ContainerId != sourceContainerId ||
                _managedSerializedTransferContainers.ContainsKey(item.ContainerId))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitStageInvalid);
            }

            Failure handsCapacityFailure = ValidateCapacity(handsContainerId, 1);
            if (!handsCapacityFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    handsCapacityFailure);
            }

            Failure buildKitCapacityFailure = ValidateCapacity(buildKitContainerId, 1);
            if (!buildKitCapacityFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    buildKitCapacityFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var receipt = new InventorySerializedReservationWorkOrderBuildKitReceipt(
                this,
                allocation,
                buildKitAccess,
                operationId,
                lineId,
                productId,
                itemId,
                reservationId,
                componentKind,
                item.ContainerId,
                handsContainerId,
                buildKitContainerId,
                InventorySerializedReservationWorkOrderBuildKitStage.ActorHands,
                Revision + 1);
            var registration =
                new InventorySerializedReservationWorkOrderBuildKitRegistration(receipt);

            _items[itemId] = MoveSerializedItem(item, handsContainerId);
            _serializedReservationWorkOrderBuildKitsByOperation.Add(operationId, registration);
            _serializedReservationWorkOrderBuildKitsByItem.Add(itemId, registration);
            _serializedReservationWorkOrderBuildKitsByContainer.Add(
                buildKitContainerId,
                registration);
            Revision++;
            return OperationResult<
                InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(receipt);
        }

        /// <summary>
        /// Commits the second custody leg from ActorHands to the exact managed capacity-one
        /// BuildKit container. The live reservation is preserved under the registration-bound
        /// invariant exception; no generic transfer or reservation rule is relaxed.
        /// </summary>
        internal OperationResult<InventorySerializedReservationWorkOrderBuildKitReceipt>
            PlaceReservedWorkOrderLineInBuildKit(
                InventorySerializedReservationWorkOrderBuildKitReceipt pickupReceipt)
        {
            if (!OwnsWorkOrderBuildKitReceipt(pickupReceipt) ||
                pickupReceipt.Stage !=
                InventorySerializedReservationWorkOrderBuildKitStage.ActorHands)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitReceiptInvalid);
            }

            InventorySerializedReservationWorkOrderBuildKitRegistration registration =
                _serializedReservationWorkOrderBuildKitsByOperation[
                    pickupReceipt.OperationId];
            if (registration.PlacementReceipt != null)
            {
                return OwnsWorkOrderBuildKitReceipt(registration.PlacementReceipt)
                    ? OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(
                        registration.PlacementReceipt)
                    : OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                        InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            if (registration.CurrentStage !=
                    InventorySerializedReservationWorkOrderBuildKitStage.ActorHands ||
                !_items.TryGetValue(
                    pickupReceipt.ItemId,
                    out InventoryItemRecord item) ||
                item.ContainerId != pickupReceipt.HandsContainerId)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitStageInvalid);
            }

            Failure capacityFailure = ValidateCapacity(
                pickupReceipt.BuildKitContainerId,
                1);
            if (!capacityFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    capacityFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var placementReceipt =
                new InventorySerializedReservationWorkOrderBuildKitReceipt(
                    this,
                    pickupReceipt.Allocation,
                    pickupReceipt.BuildKitAccess,
                    pickupReceipt.OperationId,
                    pickupReceipt.LineId,
                    pickupReceipt.ProductId,
                    pickupReceipt.ItemId,
                    pickupReceipt.ReservationId,
                    pickupReceipt.ComponentKind,
                    pickupReceipt.SourceContainerId,
                    pickupReceipt.HandsContainerId,
                    pickupReceipt.BuildKitContainerId,
                    InventorySerializedReservationWorkOrderBuildKitStage.BuildKit,
                    Revision + 1);
            if (!registration.TryPublishPlacement(placementReceipt))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            _items[item.Id] = MoveSerializedItem(item, pickupReceipt.BuildKitContainerId);
            Revision++;
            return OperationResult<
                InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(
                placementReceipt);
        }

        internal bool OwnsWorkOrderBuildKitReceipt(
            InventorySerializedReservationWorkOrderBuildKitReceipt receipt)
        {
            if (receipt == null ||
                !ReferenceEquals(receipt.Owner, this) ||
                receipt.AppliedRevision <= 0 ||
                receipt.AppliedRevision > Revision ||
                !_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    receipt.OperationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration
                        registration) ||
                !OwnsWorkOrderBuildKitRegistration(registration))
            {
                return false;
            }

            return receipt.Stage ==
                       InventorySerializedReservationWorkOrderBuildKitStage.ActorHands
                    ? ReferenceEquals(receipt, registration.PickupReceipt)
                    : receipt.Stage ==
                          InventorySerializedReservationWorkOrderBuildKitStage.BuildKit &&
                      ReferenceEquals(receipt, registration.PlacementReceipt);
        }

        internal bool TryGetWorkOrderBuildKitReceipt(
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            out InventorySerializedReservationWorkOrderBuildKitReceipt receipt)
        {
            if (_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    operationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration registration) &&
                OwnsWorkOrderBuildKitRegistration(registration))
            {
                receipt = registration.PlacementReceipt ?? registration.PickupReceipt;
                return true;
            }

            receipt = null;
            return false;
        }

        private Failure ValidateWorkOrderBuildKitInput(
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            InventorySerializedTransferAccess buildKitAccess,
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId)
        {
            if (!OwnsSerializedReservationWorkOrderAllocation(allocation))
            {
                return InventoryFailures.SerializedReservationWorkOrderReceiptInvalid;
            }

            if (operationId.IsEmpty)
            {
                return InventoryFailures
                    .InvalidSerializedReservationWorkOrderBuildKitOperationId;
            }

            if (lineId.IsEmpty)
            {
                return InventoryFailures.InvalidSerializedReservationWorkOrderLineId;
            }

            if (productId.IsEmpty || itemId.IsEmpty || reservationId.IsEmpty ||
                !IsSupportedWorkOrderBuildKitComponent(componentKind))
            {
                return InventoryFailures.SerializedReservationWorkOrderBuildKitLineInvalid;
            }

            if (sourceContainerId.IsEmpty ||
                !_containers.TryGetValue(
                    sourceContainerId,
                    out InventoryContainerDefinition source) ||
                source.Kind != InventoryContainerKind.WorldFloor ||
                handsContainerId.IsEmpty ||
                !_containers.TryGetValue(
                    handsContainerId,
                    out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands ||
                hands.UnitCapacity != 1 ||
                buildKitContainerId.IsEmpty ||
                !_containers.TryGetValue(
                    buildKitContainerId,
                    out InventoryContainerDefinition buildKit) ||
                buildKit.Kind != InventoryContainerKind.BuildKit ||
                buildKit.UnitCapacity != 1 ||
                sourceContainerId == handsContainerId ||
                sourceContainerId == buildKitContainerId ||
                handsContainerId == buildKitContainerId)
            {
                return InventoryFailures.SerializedReservationWorkOrderBuildKitContainerInvalid;
            }

            Failure accessFailure = ValidateSerializedTransferAccess(
                handsContainerId,
                buildKitContainerId,
                buildKitAccess);
            if (!accessFailure.IsNone)
            {
                return accessFailure;
            }

            if (!_items.TryGetValue(itemId, out InventoryItemRecord item) ||
                item.ProductId != productId ||
                !_reservations.TryGetValue(
                    reservationId,
                    out InventoryReservation reservation) ||
                reservation.TargetKind != InventoryReservationTargetKind.SerializedItem ||
                reservation.ItemId != itemId ||
                reservation.ClaimId != allocation.ClaimId ||
                reservation.Quantity != 1 ||
                !ContainsExactWorkOrderAllocationRequest(
                    allocation,
                    reservationId,
                    itemId))
            {
                return InventoryFailures.SerializedReservationWorkOrderBuildKitLineInvalid;
            }

            return Failure.None;
        }

        private bool OwnsWorkOrderBuildKitRegistration(
            InventorySerializedReservationWorkOrderBuildKitRegistration registration)
        {
            InventorySerializedReservationWorkOrderBuildKitReceipt pickup =
                registration?.PickupReceipt;
            if (pickup == null ||
                pickup.Stage !=
                    InventorySerializedReservationWorkOrderBuildKitStage.ActorHands ||
                pickup.AppliedRevision <= 0 ||
                pickup.AppliedRevision > Revision ||
                pickup.OperationId.IsEmpty ||
                pickup.LineId.IsEmpty ||
                pickup.ProductId.IsEmpty ||
                pickup.ItemId.IsEmpty ||
                pickup.ReservationId.IsEmpty ||
                !IsSupportedWorkOrderBuildKitComponent(pickup.ComponentKind) ||
                pickup.SourceContainerId.IsEmpty ||
                pickup.HandsContainerId.IsEmpty ||
                pickup.BuildKitContainerId.IsEmpty ||
                pickup.SourceContainerId == pickup.HandsContainerId ||
                pickup.SourceContainerId == pickup.BuildKitContainerId ||
                pickup.HandsContainerId == pickup.BuildKitContainerId ||
                !OwnsSerializedReservationWorkOrderAllocation(pickup.Allocation) ||
                !_containers.TryGetValue(
                    pickup.SourceContainerId,
                    out InventoryContainerDefinition source) ||
                source.Kind != InventoryContainerKind.WorldFloor ||
                _managedSerializedTransferContainers.ContainsKey(
                    pickup.SourceContainerId) ||
                !_containers.TryGetValue(
                    pickup.HandsContainerId,
                    out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands ||
                hands.UnitCapacity != 1 ||
                !_containers.TryGetValue(
                    pickup.BuildKitContainerId,
                    out InventoryContainerDefinition buildKit) ||
                buildKit.Kind != InventoryContainerKind.BuildKit ||
                buildKit.UnitCapacity != 1 ||
                !ValidateSerializedTransferAccess(
                        pickup.HandsContainerId,
                        pickup.BuildKitContainerId,
                        pickup.BuildKitAccess).IsNone ||
                !_items.TryGetValue(
                    pickup.ItemId,
                    out InventoryItemRecord item) ||
                item.ProductId != pickup.ProductId ||
                !_reservations.TryGetValue(
                    pickup.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.TargetKind != InventoryReservationTargetKind.SerializedItem ||
                reservation.ItemId != pickup.ItemId ||
                reservation.ClaimId != pickup.Allocation.ClaimId ||
                reservation.Quantity != 1 ||
                !ContainsExactWorkOrderAllocationRequest(
                    pickup.Allocation,
                    pickup.ReservationId,
                    pickup.ItemId) ||
                !_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    pickup.OperationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration byOperation) ||
                !_serializedReservationWorkOrderBuildKitsByItem.TryGetValue(
                    pickup.ItemId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration byItem) ||
                !_serializedReservationWorkOrderBuildKitsByContainer.TryGetValue(
                    pickup.BuildKitContainerId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration byContainer) ||
                !ReferenceEquals(registration, byOperation) ||
                !ReferenceEquals(registration, byItem) ||
                !ReferenceEquals(registration, byContainer))
            {
                return false;
            }

            InventorySerializedReservationWorkOrderBuildKitReceipt placement =
                registration.PlacementReceipt;
            if (placement == null)
            {
                return registration.CurrentStage ==
                           InventorySerializedReservationWorkOrderBuildKitStage.ActorHands &&
                       item.ContainerId == pickup.HandsContainerId &&
                       GetContainerLoadUnsafe(pickup.BuildKitContainerId) == 0;
            }

            return placement.Stage ==
                       InventorySerializedReservationWorkOrderBuildKitStage.BuildKit &&
                   placement.AppliedRevision > pickup.AppliedRevision &&
                   placement.AppliedRevision <= Revision &&
                   MatchesWorkOrderBuildKitReceiptIdentity(pickup, placement) &&
                   registration.CurrentStage ==
                       InventorySerializedReservationWorkOrderBuildKitStage.BuildKit &&
                   item.ContainerId == pickup.BuildKitContainerId &&
                   GetContainerLoadUnsafe(pickup.BuildKitContainerId) == 1;
        }

        private bool HasValidSerializedReservationWorkOrderBuildKits()
        {
            int count = _serializedReservationWorkOrderBuildKitsByOperation.Count;
            if (_serializedReservationWorkOrderBuildKitsByItem.Count != count ||
                _serializedReservationWorkOrderBuildKitsByContainer.Count != count)
            {
                return false;
            }

            foreach (InventorySerializedReservationWorkOrderBuildKitRegistration registration in
                     _serializedReservationWorkOrderBuildKitsByOperation.Values)
            {
                if (!OwnsWorkOrderBuildKitRegistration(registration))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsValidReservedSerializedWorkOrderBuildKitCustody(
            InventoryReservation reservation,
            InventoryItemRecord item)
        {
            return reservation != null &&
                   item != null &&
                   _serializedReservationWorkOrderBuildKitsByItem.TryGetValue(
                       item.Id,
                       out InventorySerializedReservationWorkOrderBuildKitRegistration
                           registration) &&
                   registration.PlacementReceipt != null &&
                   registration.PlacementReceipt.ReservationId == reservation.Id &&
                   registration.PlacementReceipt.BuildKitContainerId == item.ContainerId &&
                   OwnsWorkOrderBuildKitRegistration(registration);
        }

        private static bool ContainsExactWorkOrderAllocationRequest(
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            StableId<ReservationIdScope> reservationId,
            StableId<ItemInstanceIdScope> itemId)
        {
            if (allocation?.Requests == null)
            {
                return false;
            }

            int matchCount = 0;
            for (int index = 0; index < allocation.Requests.Count; index++)
            {
                InventorySerializedReservationRequest request = allocation.Requests[index];
                if (request != null &&
                    request.ReservationId == reservationId &&
                    request.ClaimId == allocation.ClaimId &&
                    request.ItemId == itemId)
                {
                    matchCount++;
                }
            }

            return matchCount == 1;
        }

        private static bool IsSupportedWorkOrderBuildKitComponent(
            PcComponentKind componentKind)
        {
            return componentKind == PcComponentKind.Motherboard ||
                   componentKind == PcComponentKind.Processor ||
                   componentKind == PcComponentKind.MemoryModule;
        }

        private static InventoryItemRecord MoveSerializedItem(
            InventoryItemRecord item,
            StableId<ContainerIdScope> targetContainerId)
        {
            return new InventoryItemRecord(
                item.Id,
                item.ProductId,
                targetContainerId,
                item.Condition,
                item.UnitCost,
                item.StateFlags);
        }

        private static bool MatchesWorkOrderBuildKitRegistration(
            InventorySerializedReservationWorkOrderBuildKitRegistration registration,
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            InventorySerializedTransferAccess buildKitAccess,
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId)
        {
            InventorySerializedReservationWorkOrderBuildKitReceipt pickup =
                registration?.PickupReceipt;
            return pickup != null &&
                   ReferenceEquals(pickup.Allocation, allocation) &&
                   ReferenceEquals(pickup.BuildKitAccess, buildKitAccess) &&
                   pickup.OperationId == operationId &&
                   pickup.LineId == lineId &&
                   pickup.ProductId == productId &&
                   pickup.ItemId == itemId &&
                   pickup.ReservationId == reservationId &&
                   pickup.ComponentKind == componentKind &&
                   pickup.SourceContainerId == sourceContainerId &&
                   pickup.HandsContainerId == handsContainerId &&
                   pickup.BuildKitContainerId == buildKitContainerId;
        }

        private static bool MatchesWorkOrderBuildKitReceiptIdentity(
            InventorySerializedReservationWorkOrderBuildKitReceipt expected,
            InventorySerializedReservationWorkOrderBuildKitReceipt actual)
        {
            return expected != null &&
                   actual != null &&
                   ReferenceEquals(expected.Owner, actual.Owner) &&
                   ReferenceEquals(expected.Allocation, actual.Allocation) &&
                   ReferenceEquals(expected.BuildKitAccess, actual.BuildKitAccess) &&
                   expected.OperationId == actual.OperationId &&
                   expected.LineId == actual.LineId &&
                   expected.ProductId == actual.ProductId &&
                   expected.ItemId == actual.ItemId &&
                   expected.ReservationId == actual.ReservationId &&
                   expected.ComponentKind == actual.ComponentKind &&
                   expected.SourceContainerId == actual.SourceContainerId &&
                   expected.HandsContainerId == actual.HandsContainerId &&
                   expected.BuildKitContainerId == actual.BuildKitContainerId;
        }
    }
}
