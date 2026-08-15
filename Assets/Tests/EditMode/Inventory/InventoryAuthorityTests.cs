using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Inventory
{
    public sealed class InventoryAuthorityTests
    {
        private static readonly StableId<ProductDefinitionIdScope> SerializedProduct = ProductId("pc.graphics-card");
        private static readonly StableId<ProductDefinitionIdScope> BatchProduct = ProductId("shop.cable-tie");
        private static readonly StableId<ContainerIdScope> Receiving = ContainerId("container.receiving");
        private static readonly StableId<ContainerIdScope> Shelf = ContainerId("container.shelf");

        [Test]
        public void ReceivesSerializedAndBatchStockIntoOneAuthority()
        {
            InventoryAuthority authority = CreateAuthority();

            Assert.That(authority.ReceiveSerializedItem(
                ItemId("item.gpu-001"), SerializedProduct, Receiving, InventoryCondition.New).IsSuccess, Is.True);
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.ties-001"), BatchProduct, Receiving, InventoryCondition.New, 12).IsSuccess, Is.True);

            Assert.That(authority.GetTotalQuantity(SerializedProduct).Value, Is.EqualTo(1));
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.EqualTo(12));
            Assert.That(authority.GetAvailableQuantity(SerializedProduct).Value, Is.EqualTo(1));
            Assert.That(authority.GetContainerQuantity(Receiving).Value, Is.EqualTo(13));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void TrackingMismatchFailsWithoutMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            long revision = authority.Revision;

            OperationResult wrongItem = authority.ReceiveSerializedItem(
                ItemId("item.wrong"), BatchProduct, Receiving, InventoryCondition.New);
            OperationResult wrongBatch = authority.ReceiveBatch(
                BatchId("batch.wrong"), SerializedProduct, Receiving, InventoryCondition.New, 2);

            Assert.That(wrongItem.Error, Is.EqualTo(InventoryFailures.TrackingMismatch));
            Assert.That(wrongBatch.Error, Is.EqualTo(InventoryFailures.TrackingMismatch));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.SerializedItemCount, Is.Zero);
            Assert.That(authority.BatchCount, Is.Zero);
        }

        [Test]
        public void DuplicateAndInvalidReceiptsFailWithoutMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.unique");
            Assert.That(authority.ReceiveSerializedItem(
                item, SerializedProduct, Receiving, InventoryCondition.New).IsSuccess, Is.True);
            long revision = authority.Revision;

            Assert.That(authority.ReceiveSerializedItem(
                item, SerializedProduct, Shelf, InventoryCondition.New).Error,
                Is.EqualTo(InventoryFailures.DuplicateItem));
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.zero"), BatchProduct, Receiving, InventoryCondition.New, 0).Error,
                Is.EqualTo(InventoryFailures.InvalidQuantity));
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.condition"), BatchProduct, Receiving, (InventoryCondition)99, 1).Error,
                Is.EqualTo(InventoryFailures.InvalidCondition));

            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.GetContainerQuantity(Receiving).Value, Is.EqualTo(1));
        }

        [Test]
        public void MixedStockCannotOverflowLogicalContainerCapacity()
        {
            InventoryAuthority authority = CreateAuthority(receivingCapacity: 3);
            Assert.That(authority.ReceiveSerializedItem(
                ItemId("item.capacity"), SerializedProduct, Receiving, InventoryCondition.New).IsSuccess, Is.True);
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.capacity"), BatchProduct, Receiving, InventoryCondition.New, 2).IsSuccess, Is.True);
            long revision = authority.Revision;

            OperationResult result = authority.ReceiveSerializedItem(
                ItemId("item.overflow"), SerializedProduct, Receiving, InventoryCondition.New);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.GetContainerQuantity(Receiving).Value, Is.EqualTo(3));
        }

        [Test]
        public void SerializedReservationIsExclusiveFollowsTransferAndConsumesExactlyOnce()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.reserved");
            StableId<ReservationIdScope> reservation = ReservationId("reservation.sale-1");
            Assert.That(authority.ReceiveSerializedItem(
                item, SerializedProduct, Receiving, InventoryCondition.OpenBox).IsSuccess, Is.True);
            Assert.That(authority.ReserveSerializedItem(
                reservation, ClaimId("claim.customer-1"), item).IsSuccess, Is.True);

            Assert.That(authority.ReserveSerializedItem(
                ReservationId("reservation.sale-2"), ClaimId("claim.customer-2"), item).Error,
                Is.EqualTo(InventoryFailures.ItemAlreadyReserved));
            Assert.That(authority.GetAvailableQuantity(SerializedProduct).Value, Is.Zero);
            Assert.That(authority.TransferSerializedItem(item, Shelf).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(item, out InventoryItemRecord moved), Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(Shelf));

            Assert.That(authority.ConsumeReservation(reservation).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(item, out _), Is.False);
            Assert.That(authority.GetTotalQuantity(SerializedProduct).Value, Is.Zero);
            Assert.That(authority.ConsumeReservation(reservation).Error,
                Is.EqualTo(InventoryFailures.UnknownReservation));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReleasedSerializedReservationMakesUnitAvailableAgain()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.release");
            StableId<ReservationIdScope> reservation = ReservationId("reservation.release");
            authority.ReceiveSerializedItem(item, SerializedProduct, Receiving, InventoryCondition.New);
            authority.ReserveSerializedItem(reservation, ClaimId("claim.release"), item);

            Assert.That(authority.ReleaseReservation(reservation).IsSuccess, Is.True);
            Assert.That(authority.GetAvailableQuantity(SerializedProduct).Value, Is.EqualTo(1));
            Assert.That(authority.ReservationCount, Is.Zero);
        }

        [Test]
        public void BatchTransferSplitsPositionButPreservesBatchIdentityAndTotal()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.split");
            authority.ReceiveBatch(batch, BatchProduct, Receiving, InventoryCondition.New, 10);

            Assert.That(authority.TransferBatch(batch, Receiving, Shelf, 4).IsSuccess, Is.True);

            Assert.That(authority.GetBatchQuantity(batch, Receiving).Value, Is.EqualTo(6));
            Assert.That(authority.GetBatchQuantity(batch, Shelf).Value, Is.EqualTo(4));
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.EqualTo(10));
            Assert.That(authority.BatchCount, Is.EqualTo(1));
            Assert.That(authority.GetBatchPositions().Select(position => position.BatchId).Distinct().Single(),
                Is.EqualTo(batch));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReservedBatchQuantityCannotBeMovedFromItsPosition()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.transfer-lock");
            authority.ReceiveBatch(batch, BatchProduct, Receiving, InventoryCondition.New, 10);
            authority.ReserveBatch(
                ReservationId("reservation.batch-lock"),
                ClaimId("claim.batch-lock"),
                batch,
                Receiving,
                7);
            long revision = authority.Revision;

            OperationResult result = authority.TransferBatch(batch, Receiving, Shelf, 4);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ReservedQuantity));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.GetBatchQuantity(batch, Receiving).Value, Is.EqualTo(10));
            Assert.That(authority.GetBatchQuantity(batch, Shelf).Error,
                Is.EqualTo(InventoryFailures.UnknownBatchPosition));
        }

        [Test]
        public void BatchTransferCannotMoveMoreThanStoredAndLeavesStateUntouched()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.over-transfer");
            authority.ReceiveBatch(batch, BatchProduct, Receiving, InventoryCondition.New, 3);
            long revision = authority.Revision;

            OperationResult result = authority.TransferBatch(batch, Receiving, Shelf, 4);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.InsufficientAvailable));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.GetBatchQuantity(batch, Receiving).Value, Is.EqualTo(3));
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.EqualTo(3));
        }

        [Test]
        public void BatchReservationsAreBoundedAndConsumptionReducesStockAtomically()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.consume");
            StableId<ReservationIdScope> first = ReservationId("reservation.batch-1");
            authority.ReceiveBatch(batch, BatchProduct, Receiving, InventoryCondition.New, 8);
            Assert.That(authority.ReserveBatch(
                first, ClaimId("claim.order-1"), batch, Receiving, 5).IsSuccess, Is.True);
            long revision = authority.Revision;

            OperationResult overReserve = authority.ReserveBatch(
                ReservationId("reservation.batch-2"),
                ClaimId("claim.order-2"),
                batch,
                Receiving,
                4);
            Assert.That(overReserve.Error, Is.EqualTo(InventoryFailures.InsufficientAvailable));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.GetAvailableQuantity(BatchProduct).Value, Is.EqualTo(3));

            Assert.That(authority.ConsumeReservation(first).IsSuccess, Is.True);
            Assert.That(authority.GetBatchQuantity(batch, Receiving).Value, Is.EqualTo(3));
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.EqualTo(3));
            Assert.That(authority.GetAvailableQuantity(BatchProduct).Value, Is.EqualTo(3));
        }

        [Test]
        public void ConsumingEntireBatchRemovesEmptyBatchMetadata()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.empty");
            StableId<ReservationIdScope> reservation = ReservationId("reservation.empty");
            authority.ReceiveBatch(batch, BatchProduct, Receiving, InventoryCondition.New, 2);
            authority.ReserveBatch(reservation, ClaimId("claim.empty"), batch, Receiving, 2);

            Assert.That(authority.ConsumeReservation(reservation).IsSuccess, Is.True);
            Assert.That(authority.BatchCount, Is.Zero);
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void QueriesReturnDeterministicStableIdOrder()
        {
            InventoryAuthority authority = CreateAuthority();
            authority.ReceiveSerializedItem(
                ItemId("item.zeta"), SerializedProduct, Receiving, InventoryCondition.New);
            authority.ReceiveSerializedItem(
                ItemId("item.alpha"), SerializedProduct, Receiving, InventoryCondition.New);
            authority.ReceiveBatch(
                BatchId("batch.zeta"), BatchProduct, Receiving, InventoryCondition.New, 1);
            authority.ReceiveBatch(
                BatchId("batch.alpha"), BatchProduct, Shelf, InventoryCondition.New, 1);

            Assert.That(authority.GetContainers().Select(value => value.Id.Value),
                Is.Ordered.Using<string>(System.StringComparer.Ordinal));
            Assert.That(authority.GetSerializedItems().Select(value => value.Id.Value),
                Is.EqualTo(new[] { "item.alpha", "item.zeta" }));
            Assert.That(authority.GetBatchPositions().Select(value => value.BatchId.Value),
                Is.EqualTo(new[] { "batch.alpha", "batch.zeta" }));
        }

        [Test]
        public void UnknownProductAndContainerQueriesFailClosed()
        {
            InventoryAuthority authority = CreateAuthority();

            Assert.That(authority.GetTotalQuantity(ProductId("missing.product")).Error,
                Is.EqualTo(InventoryFailures.UnknownProduct));
            Assert.That(authority.GetContainerQuantity(ContainerId("container.missing")).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));
            Assert.That(authority.Revision, Is.EqualTo(2), "Only the two fixture containers should mutate state.");
        }

        [Test]
        public void ContainerRegistrationRejectsDuplicateWithoutMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            long revision = authority.Revision;
            InventoryContainerDefinition duplicate = InventoryContainerDefinition.Create(
                Receiving, InventoryContainerKind.Receiving, 999).Value;

            Assert.That(authority.RegisterContainer(duplicate).Error,
                Is.EqualTo(InventoryFailures.DuplicateContainer));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.ContainerCount, Is.EqualTo(2));
        }

        private static InventoryAuthority CreateAuthority(int receivingCapacity = 100, int shelfCapacity = 100)
        {
            ProductDefinition serialized = ProductDefinition.Create(
                SerializedProduct,
                CategoryId("graphics-cards"),
                "Serialized Graphics Card",
                ProductTrackingPolicy.SerializedInstance,
                1095).Value;
            ProductDefinition batch = ProductDefinition.Create(
                BatchProduct,
                CategoryId("accessories"),
                "Cable Tie",
                ProductTrackingPolicy.BatchQuantity,
                0).Value;
            ProductCatalog catalog = ProductCatalog.Create(new[] { serialized, batch }).Value;
            InventoryAuthority authority = InventoryAuthority.Create(catalog).Value;
            authority.RegisterContainer(InventoryContainerDefinition.Create(
                Receiving, InventoryContainerKind.Receiving, receivingCapacity).Value);
            authority.RegisterContainer(InventoryContainerDefinition.Create(
                Shelf, InventoryContainerKind.Shelf, shelfCapacity).Value);
            return authority;
        }

        private static StableId<ProductDefinitionIdScope> ProductId(string value) =>
            StableId<ProductDefinitionIdScope>.Parse(value);

        private static StableId<ProductCategoryIdScope> CategoryId(string value) =>
            StableId<ProductCategoryIdScope>.Parse(value);

        private static StableId<ContainerIdScope> ContainerId(string value) =>
            StableId<ContainerIdScope>.Parse(value);

        private static StableId<ItemInstanceIdScope> ItemId(string value) =>
            StableId<ItemInstanceIdScope>.Parse(value);

        private static StableId<BatchIdScope> BatchId(string value) =>
            StableId<BatchIdScope>.Parse(value);

        private static StableId<ReservationIdScope> ReservationId(string value) =>
            StableId<ReservationIdScope>.Parse(value);

        private static StableId<InventoryClaimIdScope> ClaimId(string value) =>
            StableId<InventoryClaimIdScope>.Parse(value);
    }
}
