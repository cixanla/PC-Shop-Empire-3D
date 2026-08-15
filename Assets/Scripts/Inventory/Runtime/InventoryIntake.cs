using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Inventory
{
    public sealed class InventorySerializedIntake
    {
        private InventorySerializedIntake(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            InventoryCondition condition,
            InventoryUnitCost unitCost)
        {
            ItemId = itemId;
            ProductId = productId;
            Condition = condition;
            UnitCost = unitCost;
        }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public InventoryCondition Condition { get; }

        public InventoryUnitCost UnitCost { get; }

        public static OperationResult<InventorySerializedIntake> Create(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            InventoryCondition condition,
            InventoryUnitCost unitCost)
        {
            if (itemId.IsEmpty)
            {
                return OperationResult<InventorySerializedIntake>.Fail(InventoryFailures.InvalidItemId);
            }

            if (productId.IsEmpty)
            {
                return OperationResult<InventorySerializedIntake>.Fail(InventoryFailures.InvalidProductId);
            }

            if (!InventoryValidation.IsValidCondition(condition))
            {
                return OperationResult<InventorySerializedIntake>.Fail(InventoryFailures.InvalidCondition);
            }

            if (!unitCost.IsValid)
            {
                return OperationResult<InventorySerializedIntake>.Fail(InventoryFailures.InvalidUnitCost);
            }

            return OperationResult<InventorySerializedIntake>.Success(
                new InventorySerializedIntake(itemId, productId, condition, unitCost));
        }
    }

    public sealed class InventoryBatchIntake
    {
        private InventoryBatchIntake(
            StableId<BatchIdScope> batchId,
            StableId<ProductDefinitionIdScope> productId,
            InventoryCondition condition,
            int quantity,
            InventoryUnitCost unitCost)
        {
            BatchId = batchId;
            ProductId = productId;
            Condition = condition;
            Quantity = quantity;
            UnitCost = unitCost;
        }

        public StableId<BatchIdScope> BatchId { get; }

        public StableId<ProductDefinitionIdScope> ProductId { get; }

        public InventoryCondition Condition { get; }

        public int Quantity { get; }

        public InventoryUnitCost UnitCost { get; }

        public static OperationResult<InventoryBatchIntake> Create(
            StableId<BatchIdScope> batchId,
            StableId<ProductDefinitionIdScope> productId,
            InventoryCondition condition,
            int quantity,
            InventoryUnitCost unitCost)
        {
            if (batchId.IsEmpty)
            {
                return OperationResult<InventoryBatchIntake>.Fail(InventoryFailures.InvalidBatchId);
            }

            if (productId.IsEmpty)
            {
                return OperationResult<InventoryBatchIntake>.Fail(InventoryFailures.InvalidProductId);
            }

            if (!InventoryValidation.IsValidCondition(condition))
            {
                return OperationResult<InventoryBatchIntake>.Fail(InventoryFailures.InvalidCondition);
            }

            if (quantity <= 0)
            {
                return OperationResult<InventoryBatchIntake>.Fail(InventoryFailures.InvalidQuantity);
            }

            if (!unitCost.IsValid)
            {
                return OperationResult<InventoryBatchIntake>.Fail(InventoryFailures.InvalidUnitCost);
            }

            return OperationResult<InventoryBatchIntake>.Success(
                new InventoryBatchIntake(batchId, productId, condition, quantity, unitCost));
        }
    }

    /// <summary>
    /// Immutable, deterministic stock-intake request. The authority preflights every entry before mutating state.
    /// </summary>
    public sealed class InventoryIntake
    {
        private readonly IReadOnlyList<InventorySerializedIntake> _serializedItems;
        private readonly IReadOnlyList<InventoryBatchIntake> _batches;

        private InventoryIntake(
            IReadOnlyList<InventorySerializedIntake> serializedItems,
            IReadOnlyList<InventoryBatchIntake> batches)
        {
            _serializedItems = serializedItems;
            _batches = batches;
        }

        public IReadOnlyList<InventorySerializedIntake> SerializedItems => _serializedItems;

        public IReadOnlyList<InventoryBatchIntake> Batches => _batches;

        public long UnitQuantity
        {
            get
            {
                long total = _serializedItems.Count;
                for (int index = 0; index < _batches.Count; index++)
                {
                    total += _batches[index].Quantity;
                }

                return total;
            }
        }

        public static OperationResult<InventoryIntake> Create(
            IEnumerable<InventorySerializedIntake> serializedItems,
            IEnumerable<InventoryBatchIntake> batches)
        {
            var orderedItems = new List<InventorySerializedIntake>();
            var orderedBatches = new List<InventoryBatchIntake>();
            var itemIds = new HashSet<StableId<ItemInstanceIdScope>>();
            var batchIds = new HashSet<StableId<BatchIdScope>>();

            if (serializedItems != null)
            {
                foreach (InventorySerializedIntake item in serializedItems)
                {
                    if (item == null)
                    {
                        return OperationResult<InventoryIntake>.Fail(InventoryFailures.NullIntakeEntry);
                    }

                    if (!itemIds.Add(item.ItemId))
                    {
                        return OperationResult<InventoryIntake>.Fail(InventoryFailures.DuplicateIntakeItem);
                    }

                    orderedItems.Add(item);
                }
            }

            if (batches != null)
            {
                foreach (InventoryBatchIntake batch in batches)
                {
                    if (batch == null)
                    {
                        return OperationResult<InventoryIntake>.Fail(InventoryFailures.NullIntakeEntry);
                    }

                    if (!batchIds.Add(batch.BatchId))
                    {
                        return OperationResult<InventoryIntake>.Fail(InventoryFailures.DuplicateIntakeBatch);
                    }

                    orderedBatches.Add(batch);
                }
            }

            if (orderedItems.Count == 0 && orderedBatches.Count == 0)
            {
                return OperationResult<InventoryIntake>.Fail(InventoryFailures.EmptyIntake);
            }

            orderedItems.Sort((left, right) =>
                string.Compare(left.ItemId.Value, right.ItemId.Value, StringComparison.Ordinal));
            orderedBatches.Sort((left, right) =>
                string.Compare(left.BatchId.Value, right.BatchId.Value, StringComparison.Ordinal));

            return OperationResult<InventoryIntake>.Success(
                new InventoryIntake(
                    Array.AsReadOnly(orderedItems.ToArray()),
                    Array.AsReadOnly(orderedBatches.ToArray())));
        }
    }
}
