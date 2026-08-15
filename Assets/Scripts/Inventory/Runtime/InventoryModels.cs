using System;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Inventory
{
    /// <summary>
    /// Immutable acquisition provenance for one inventory unit. Currency metadata and
    /// non-two-decimal currencies remain outside this bounded contract.
    /// </summary>
    public readonly struct InventoryUnitCost : IEquatable<InventoryUnitCost>
    {
        public const long MaximumMinorUnits = 999_999_999L;

        private readonly string _currencyCode;

        private InventoryUnitCost(string currencyCode, long minorUnits)
        {
            _currencyCode = currencyCode;
            MinorUnits = minorUnits;
        }

        public string CurrencyCode => _currencyCode ?? string.Empty;

        public long MinorUnits { get; }

        public bool IsValid =>
            IsValidCurrencyCode(CurrencyCode) &&
            MinorUnits > 0 &&
            MinorUnits <= MaximumMinorUnits;

        public static OperationResult<InventoryUnitCost> Create(
            string currencyCode,
            long minorUnits)
        {
            if (!IsValidCurrencyCode(currencyCode))
            {
                return OperationResult<InventoryUnitCost>.Fail(
                    InventoryFailures.InvalidUnitCostCurrency);
            }

            if (minorUnits <= 0)
            {
                return OperationResult<InventoryUnitCost>.Fail(
                    InventoryFailures.InvalidUnitCostAmount);
            }

            if (minorUnits > MaximumMinorUnits)
            {
                return OperationResult<InventoryUnitCost>.Fail(
                    InventoryFailures.UnitCostLimitExceeded);
            }

            return OperationResult<InventoryUnitCost>.Success(
                new InventoryUnitCost(currencyCode, minorUnits));
        }

        public bool Equals(InventoryUnitCost other)
        {
            return string.Equals(_currencyCode, other._currencyCode, StringComparison.Ordinal) &&
                   MinorUnits == other.MinorUnits;
        }

        public override bool Equals(object obj)
        {
            return obj is InventoryUnitCost other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StringComparer.Ordinal.GetHashCode(CurrencyCode) * 397) ^
                       MinorUnits.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{CurrencyCode} {MinorUnits}";
        }

        public static bool operator ==(InventoryUnitCost left, InventoryUnitCost right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InventoryUnitCost left, InventoryUnitCost right)
        {
            return !left.Equals(right);
        }

        private static bool IsValidCurrencyCode(string value)
        {
            if (value == null || value.Length != 3)
            {
                return false;
            }

            for (int index = 0; index < value.Length; index++)
            {
                if (value[index] < 'A' || value[index] > 'Z')
                {
                    return false;
                }
            }

            return true;
        }
    }

    public enum InventoryCondition
    {
        New = 1,
        OpenBox = 2,
        Used = 3,
        Damaged = 4,
        Defective = 5
    }

    public enum InventoryContainerKind
    {
        Receiving = 1,
        Storage = 2,
        Shelf = 3,
        ActorHands = 4,
        TransportCart = 5,
        Workbench = 6,
        CustomerBasket = 7,
        Quarantine = 8,
        WorldFloor = 9
    }

    public enum InventoryReservationTargetKind
    {
        SerializedItem = 1,
        BatchPosition = 2
    }

    public enum InventoryReservationReleasePolicy
    {
        Releasable = 1,
        ConsumeOnly = 2
    }

    public sealed class InventoryContainerDefinition
    {
        private InventoryContainerDefinition(
            StableId<ContainerIdScope> id,
            InventoryContainerKind kind,
            int unitCapacity)
        {
            Id = id;
            Kind = kind;
            UnitCapacity = unitCapacity;
        }

        public StableId<ContainerIdScope> Id { get; }

        public InventoryContainerKind Kind { get; }

        public int UnitCapacity { get; }

        public static OperationResult<InventoryContainerDefinition> Create(
            StableId<ContainerIdScope> id,
            InventoryContainerKind kind,
            int unitCapacity)
        {
            if (id.IsEmpty)
            {
                return OperationResult<InventoryContainerDefinition>.Fail(InventoryFailures.InvalidContainerId);
            }

            if (!InventoryValidation.IsValidContainerKind(kind))
            {
                return OperationResult<InventoryContainerDefinition>.Fail(InventoryFailures.InvalidContainerKind);
            }

            if (unitCapacity <= 0)
            {
                return OperationResult<InventoryContainerDefinition>.Fail(InventoryFailures.InvalidContainerCapacity);
            }

            return OperationResult<InventoryContainerDefinition>.Success(
                new InventoryContainerDefinition(id, kind, unitCapacity));
        }
    }

    public sealed class InventoryItemRecord
    {
        internal InventoryItemRecord(
            StableId<ItemInstanceIdScope> id,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> containerId,
            InventoryCondition condition,
            InventoryUnitCost unitCost)
        {
            Id = id;
            ProductId = productId;
            ContainerId = containerId;
            Condition = condition;
            UnitCost = unitCost;
        }

        public StableId<ItemInstanceIdScope> Id { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public InventoryCondition Condition { get; }

        public InventoryUnitCost UnitCost { get; }
    }

    public sealed class InventoryBatchRecord
    {
        internal InventoryBatchRecord(
            StableId<BatchIdScope> id,
            StableId<ProductDefinitionIdScope> productId,
            InventoryCondition condition,
            InventoryUnitCost unitCost)
        {
            Id = id;
            ProductId = productId;
            Condition = condition;
            UnitCost = unitCost;
        }

        public StableId<BatchIdScope> Id { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public InventoryCondition Condition { get; }

        public InventoryUnitCost UnitCost { get; }
    }

    public readonly struct InventoryBatchPosition
    {
        public InventoryBatchPosition(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId,
            int quantity)
        {
            BatchId = batchId;
            ContainerId = containerId;
            Quantity = quantity;
        }

        public StableId<BatchIdScope> BatchId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public int Quantity { get; }
    }

    public sealed class InventoryReservation
    {
        internal InventoryReservation(
            StableId<ReservationIdScope> id,
            StableId<InventoryClaimIdScope> claimId,
            InventoryReservationTargetKind targetKind,
            StableId<ItemInstanceIdScope> itemId,
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId,
            int quantity,
            InventoryReservationReleasePolicy releasePolicy)
        {
            Id = id;
            ClaimId = claimId;
            TargetKind = targetKind;
            ItemId = itemId;
            BatchId = batchId;
            ContainerId = containerId;
            Quantity = quantity;
            ReleasePolicy = releasePolicy;
        }

        public StableId<ReservationIdScope> Id { get; }

        public StableId<InventoryClaimIdScope> ClaimId { get; }

        public InventoryReservationTargetKind TargetKind { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<BatchIdScope> BatchId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public int Quantity { get; }

        public InventoryReservationReleasePolicy ReleasePolicy { get; }
    }

    internal readonly struct BatchPositionKey : System.IEquatable<BatchPositionKey>
    {
        public BatchPositionKey(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId)
        {
            BatchId = batchId;
            ContainerId = containerId;
        }

        public StableId<BatchIdScope> BatchId { get; }

        public StableId<ContainerIdScope> ContainerId { get; }

        public bool Equals(BatchPositionKey other)
        {
            return BatchId == other.BatchId && ContainerId == other.ContainerId;
        }

        public override bool Equals(object obj)
        {
            return obj is BatchPositionKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (BatchId.GetHashCode() * 397) ^ ContainerId.GetHashCode();
            }
        }
    }

    internal static class InventoryValidation
    {
        public static bool IsValidCondition(InventoryCondition condition)
        {
            return condition == InventoryCondition.New ||
                   condition == InventoryCondition.OpenBox ||
                   condition == InventoryCondition.Used ||
                   condition == InventoryCondition.Damaged ||
                   condition == InventoryCondition.Defective;
        }

        public static bool IsValidContainerKind(InventoryContainerKind kind)
        {
            return kind == InventoryContainerKind.Receiving ||
                   kind == InventoryContainerKind.Storage ||
                   kind == InventoryContainerKind.Shelf ||
                   kind == InventoryContainerKind.ActorHands ||
                   kind == InventoryContainerKind.TransportCart ||
                   kind == InventoryContainerKind.Workbench ||
                   kind == InventoryContainerKind.CustomerBasket ||
                   kind == InventoryContainerKind.Quarantine ||
                   kind == InventoryContainerKind.WorldFloor;
        }
    }

    public static class InventoryFailures
    {
        public static readonly Failure MissingCatalog = Failure.FromCode("inventory.catalog.missing");
        public static readonly Failure InvalidContainerId = Failure.FromCode("inventory.container-id.invalid");
        public static readonly Failure InvalidContainerKind = Failure.FromCode("inventory.container-kind.invalid");
        public static readonly Failure InvalidContainerCapacity = Failure.FromCode("inventory.container-capacity.invalid");
        public static readonly Failure DuplicateContainer = Failure.FromCode("inventory.container.duplicate");
        public static readonly Failure UnknownContainer = Failure.FromCode("inventory.container.unknown");
        public static readonly Failure ContainerCapacityExceeded = Failure.FromCode("inventory.container.capacity");
        public static readonly Failure InvalidItemId = Failure.FromCode("inventory.item-id.invalid");
        public static readonly Failure DuplicateItem = Failure.FromCode("inventory.item.duplicate");
        public static readonly Failure UnknownItem = Failure.FromCode("inventory.item.unknown");
        public static readonly Failure InvalidBatchId = Failure.FromCode("inventory.batch-id.invalid");
        public static readonly Failure DuplicateBatch = Failure.FromCode("inventory.batch.duplicate");
        public static readonly Failure UnknownBatch = Failure.FromCode("inventory.batch.unknown");
        public static readonly Failure UnknownBatchPosition = Failure.FromCode("inventory.batch-position.unknown");
        public static readonly Failure InvalidProductId = Failure.FromCode("inventory.product-id.invalid");
        public static readonly Failure UnknownProduct = Failure.FromCode("inventory.product.unknown");
        public static readonly Failure TrackingMismatch = Failure.FromCode("inventory.product.tracking-mismatch");
        public static readonly Failure InvalidCondition = Failure.FromCode("inventory.condition.invalid");
        public static readonly Failure InvalidUnitCost = Failure.FromCode("inventory.unit-cost.invalid");
        public static readonly Failure InvalidUnitCostCurrency =
            Failure.FromCode("inventory.unit-cost.currency.invalid");
        public static readonly Failure InvalidUnitCostAmount =
            Failure.FromCode("inventory.unit-cost.amount.invalid");
        public static readonly Failure UnitCostLimitExceeded =
            Failure.FromCode("inventory.unit-cost.limit");
        public static readonly Failure InvalidQuantity = Failure.FromCode("inventory.quantity.invalid");
        public static readonly Failure QuantityOverflow = Failure.FromCode("inventory.quantity.overflow");
        public static readonly Failure SameContainer = Failure.FromCode("inventory.transfer.same-container");
        public static readonly Failure ReservedQuantity = Failure.FromCode("inventory.transfer.reserved");
        public static readonly Failure SerializedTransferPlanInvalid =
            Failure.FromCode("inventory.serialized-transfer-plan.invalid");
        public static readonly Failure SerializedTransferPlanStale =
            Failure.FromCode("inventory.serialized-transfer-plan.stale");
        public static readonly Failure SerializedTransferAccessInvalid =
            Failure.FromCode("inventory.serialized-transfer-access.invalid");
        public static readonly Failure SerializedTransferContainerManaged =
            Failure.FromCode("inventory.serialized-transfer-container.managed");
        public static readonly Failure SerializedTransferContainerOccupied =
            Failure.FromCode("inventory.serialized-transfer-container.occupied");
        public static readonly Failure InvalidReservationId = Failure.FromCode("inventory.reservation-id.invalid");
        public static readonly Failure InvalidClaimId = Failure.FromCode("inventory.claim-id.invalid");
        public static readonly Failure DuplicateReservation = Failure.FromCode("inventory.reservation.duplicate");
        public static readonly Failure UnknownReservation = Failure.FromCode("inventory.reservation.unknown");
        public static readonly Failure MissingReservationSet = Failure.FromCode("inventory.reservation-set.missing");
        public static readonly Failure EmptyReservationSet = Failure.FromCode("inventory.reservation-set.empty");
        public static readonly Failure DuplicateReservationInSet = Failure.FromCode("inventory.reservation-set.duplicate");
        public static readonly Failure ItemAlreadyReserved = Failure.FromCode("inventory.reservation.item-reserved");
        public static readonly Failure InsufficientAvailable = Failure.FromCode("inventory.reservation.insufficient-available");
        public static readonly Failure RevisionOverflow = Failure.FromCode("inventory.revision-overflow");
        public static readonly Failure ReservationPlanInvalid =
            Failure.FromCode("inventory.reservation-plan-invalid");
        public static readonly Failure ReservationPlanStale =
            Failure.FromCode("inventory.reservation-plan-stale");
        public static readonly Failure CheckoutConsumptionPlanInvalid =
            Failure.FromCode("inventory.checkout-consumption-plan-invalid");
        public static readonly Failure CheckoutConsumptionPlanStale =
            Failure.FromCode("inventory.checkout-consumption-plan-stale");
        public static readonly Failure ReservationReleaseRestricted =
            Failure.FromCode("inventory.reservation.release-restricted");
        public static readonly Failure ReservationConsumptionRestricted =
            Failure.FromCode("inventory.reservation.consume-restricted");
        public static readonly Failure InvariantViolation = Failure.FromCode("inventory.invariant.failed");
        public static readonly Failure MissingIntake = Failure.FromCode("inventory.intake.missing");
        public static readonly Failure EmptyIntake = Failure.FromCode("inventory.intake.empty");
        public static readonly Failure NullIntakeEntry = Failure.FromCode("inventory.intake.entry-null");
        public static readonly Failure DuplicateIntakeItem = Failure.FromCode("inventory.intake.item-duplicate");
        public static readonly Failure DuplicateIntakeBatch = Failure.FromCode("inventory.intake.batch-duplicate");
    }
}
