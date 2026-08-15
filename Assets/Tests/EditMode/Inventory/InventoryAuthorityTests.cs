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
        private static readonly InventoryUnitCost SerializedCost = UnitCost("EUR", 42_000);
        private static readonly InventoryUnitCost BatchCost = UnitCost("EUR", 25);

        [Test]
        public void UnitCostValidatesCurrencyAmountBoundAndValueEquality()
        {
            Assert.That(InventoryUnitCost.Create(null, 1).Error,
                Is.EqualTo(InventoryFailures.InvalidUnitCostCurrency));
            Assert.That(InventoryUnitCost.Create("eur", 1).Error,
                Is.EqualTo(InventoryFailures.InvalidUnitCostCurrency));
            Assert.That(InventoryUnitCost.Create("EURO", 1).Error,
                Is.EqualTo(InventoryFailures.InvalidUnitCostCurrency));
            Assert.That(InventoryUnitCost.Create("EUR", 0).Error,
                Is.EqualTo(InventoryFailures.InvalidUnitCostAmount));
            Assert.That(InventoryUnitCost.Create(
                    "EUR", InventoryUnitCost.MaximumMinorUnits + 1).Error,
                Is.EqualTo(InventoryFailures.UnitCostLimitExceeded));

            InventoryUnitCost first = UnitCost("EUR", InventoryUnitCost.MaximumMinorUnits);
            InventoryUnitCost equal = UnitCost("EUR", InventoryUnitCost.MaximumMinorUnits);
            InventoryUnitCost differentCurrency = UnitCost("USD", InventoryUnitCost.MaximumMinorUnits);
            InventoryUnitCost differentAmount = UnitCost("EUR", InventoryUnitCost.MaximumMinorUnits - 1);

            Assert.That(first.IsValid, Is.True);
            Assert.That(first.CurrencyCode, Is.EqualTo("EUR"));
            Assert.That(first.MinorUnits, Is.EqualTo(InventoryUnitCost.MaximumMinorUnits));
            Assert.That(first, Is.EqualTo(equal));
            Assert.That(first == equal, Is.True);
            Assert.That(first != differentCurrency, Is.True);
            Assert.That(first != differentAmount, Is.True);
            Assert.That(first.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(default(InventoryUnitCost).IsValid, Is.False);
        }

        [Test]
        public void ReceivesSerializedAndBatchStockIntoOneAuthority()
        {
            InventoryAuthority authority = CreateAuthority();

            Assert.That(authority.ReceiveSerializedItem(
                ItemId("item.gpu-001"), SerializedProduct, Receiving, InventoryCondition.New,
                SerializedCost).IsSuccess, Is.True);
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.ties-001"), BatchProduct, Receiving, InventoryCondition.New, 12,
                BatchCost).IsSuccess, Is.True);

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
                ItemId("item.wrong"), BatchProduct, Receiving, InventoryCondition.New,
                SerializedCost);
            OperationResult wrongBatch = authority.ReceiveBatch(
                BatchId("batch.wrong"), SerializedProduct, Receiving, InventoryCondition.New, 2,
                BatchCost);

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
                item, SerializedProduct, Receiving, InventoryCondition.New, SerializedCost).IsSuccess, Is.True);
            long revision = authority.Revision;

            Assert.That(authority.ReceiveSerializedItem(
                item, SerializedProduct, Shelf, InventoryCondition.New, SerializedCost).Error,
                Is.EqualTo(InventoryFailures.DuplicateItem));
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.zero"), BatchProduct, Receiving, InventoryCondition.New, 0,
                BatchCost).Error,
                Is.EqualTo(InventoryFailures.InvalidQuantity));
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.condition"), BatchProduct, Receiving, (InventoryCondition)99, 1,
                BatchCost).Error,
                Is.EqualTo(InventoryFailures.InvalidCondition));

            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.GetContainerQuantity(Receiving).Value, Is.EqualTo(1));
        }

        [Test]
        public void MixedStockCannotOverflowLogicalContainerCapacity()
        {
            InventoryAuthority authority = CreateAuthority(receivingCapacity: 3);
            Assert.That(authority.ReceiveSerializedItem(
                ItemId("item.capacity"), SerializedProduct, Receiving, InventoryCondition.New,
                SerializedCost).IsSuccess, Is.True);
            Assert.That(authority.ReceiveBatch(
                BatchId("batch.capacity"), BatchProduct, Receiving, InventoryCondition.New, 2,
                BatchCost).IsSuccess, Is.True);
            long revision = authority.Revision;

            OperationResult result = authority.ReceiveSerializedItem(
                ItemId("item.overflow"), SerializedProduct, Receiving, InventoryCondition.New,
                SerializedCost);

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
                item, SerializedProduct, Receiving, InventoryCondition.OpenBox,
                SerializedCost).IsSuccess, Is.True);
            Assert.That(authority.ReserveSerializedItem(
                reservation, ClaimId("claim.customer-1"), item).IsSuccess, Is.True);

            Assert.That(authority.ReserveSerializedItem(
                ReservationId("reservation.sale-2"), ClaimId("claim.customer-2"), item).Error,
                Is.EqualTo(InventoryFailures.ItemAlreadyReserved));
            Assert.That(authority.GetAvailableQuantity(SerializedProduct).Value, Is.Zero);
            Assert.That(authority.TransferSerializedItem(item, Shelf).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(item, out InventoryItemRecord moved), Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(Shelf));
            Assert.That(moved.UnitCost, Is.EqualTo(SerializedCost));

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
            authority.ReceiveSerializedItem(
                item, SerializedProduct, Receiving, InventoryCondition.New, SerializedCost);
            authority.ReserveSerializedItem(reservation, ClaimId("claim.release"), item);

            Assert.That(authority.ReleaseReservation(reservation).IsSuccess, Is.True);
            Assert.That(authority.GetAvailableQuantity(SerializedProduct).Value, Is.EqualTo(1));
            Assert.That(authority.ReservationCount, Is.Zero);
        }

        [Test]
        public void PreparedSerializedReservationCommitIsExactReplayWithoutMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.prepared-replay");
            StableId<ReservationIdScope> reservation =
                ReservationId("reservation.prepared-replay");
            Assert.That(authority.ReceiveSerializedItem(
                item,
                SerializedProduct,
                Receiving,
                InventoryCondition.New,
                SerializedCost).IsSuccess, Is.True);
            OperationResult<InventorySerializedReservationPlan> prepared =
                authority.PrepareSerializedItemReservation(
                    reservation,
                    ClaimId("claim.prepared-replay"),
                    item);
            Assert.That(prepared.IsSuccess, Is.True);
            long preparedRevision = authority.Revision;

            Assert.That(authority.CommitPreparedSerializedItemReservation(
                prepared.Value).IsSuccess, Is.True);
            long committedRevision = authority.Revision;
            Assert.That(committedRevision, Is.EqualTo(preparedRevision + 1));
            Assert.That(authority.CommitPreparedSerializedItemReservation(
                prepared.Value).IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(committedRevision));
            Assert.That(authority.ReservationCount, Is.EqualTo(1));

            Assert.That(authority.ReleaseReservation(reservation).IsSuccess, Is.True);
            long releasedRevision = authority.Revision;
            Assert.That(authority.CommitPreparedSerializedItemReservation(prepared.Value).Error,
                Is.EqualTo(InventoryFailures.ReservationPlanStale));
            Assert.That(authority.Revision, Is.EqualTo(releasedRevision));
            Assert.That(authority.ReservationCount, Is.Zero);
        }

        [Test]
        public void CheckoutConsumptionPrepareIsSideEffectFreeAndCopiesExactReservationIds()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> firstItem = ItemId("item.checkout-prepare-a");
            StableId<ItemInstanceIdScope> secondItem = ItemId("item.checkout-prepare-b");
            StableId<ReservationIdScope> firstReservation =
                ReservationId("reservation.checkout-prepare-a");
            StableId<ReservationIdScope> secondReservation =
                ReservationId("reservation.checkout-prepare-b");
            AddCheckoutReservation(authority, firstItem, firstReservation);
            AddCheckoutReservation(authority, secondItem, secondReservation);
            var requestedIds = new[] { secondReservation, firstReservation };
            long revision = authority.Revision;

            OperationResult<InventoryCheckoutConsumptionPlan> prepared =
                authority.PrepareCheckoutReservationConsumption(requestedIds);

            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(prepared.Value.Owner, Is.SameAs(authority));
            Assert.That(prepared.Value.ExpectedRevision, Is.EqualTo(revision));
            Assert.That(prepared.Value.ReservationIds,
                Is.EqualTo(new[] { secondReservation, firstReservation }));
            requestedIds[0] = ReservationId("reservation.checkout-mutated-input");
            Assert.That(prepared.Value.ReservationIds,
                Is.EqualTo(new[] { secondReservation, firstReservation }));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.SerializedItemCount, Is.EqualTo(2));
            Assert.That(authority.ReservationCount, Is.EqualTo(2));
            Assert.That(authority.TryGetSerializedItem(firstItem, out _), Is.True);
            Assert.That(authority.TryGetSerializedItem(secondItem, out _), Is.True);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CheckoutConsumptionPrepareFailuresLeaveInventoryUntouched()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.checkout-prepare-failure");
            StableId<ReservationIdScope> reservation =
                ReservationId("reservation.checkout-prepare-failure");
            AddCheckoutReservation(authority, item, reservation);
            long revision = authority.Revision;

            Assert.That(authority.PrepareCheckoutReservationConsumption(null).Error,
                Is.EqualTo(InventoryFailures.MissingReservationSet));
            Assert.That(authority.PrepareCheckoutReservationConsumption(
                    System.Array.Empty<StableId<ReservationIdScope>>()).Error,
                Is.EqualTo(InventoryFailures.EmptyReservationSet));
            Assert.That(authority.PrepareCheckoutReservationConsumption(
                    new[] { default(StableId<ReservationIdScope>) }).Error,
                Is.EqualTo(InventoryFailures.InvalidReservationId));
            Assert.That(authority.PrepareCheckoutReservationConsumption(
                    new[] { reservation, reservation }).Error,
                Is.EqualTo(InventoryFailures.DuplicateReservationInSet));
            Assert.That(authority.PrepareCheckoutReservationConsumption(
                    new[] { reservation, ReservationId("reservation.checkout-unknown") }).Error,
                Is.EqualTo(InventoryFailures.UnknownReservation));
            Assert.That(authority.ConsumeReservations(new[] { reservation }).Error,
                Is.EqualTo(InventoryFailures.ReservationConsumptionRestricted));

            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.SerializedItemCount, Is.EqualTo(1));
            Assert.That(authority.ReservationCount, Is.EqualTo(1));
            Assert.That(authority.TryGetSerializedItem(item, out _), Is.True);
            Assert.That(authority.TryGetReservation(reservation, out _), Is.True);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CheckoutConsumptionCommitRejectsNullMalformedAndForeignPlansWithoutMutation()
        {
            InventoryAuthority owner = CreateAuthority();
            InventoryAuthority foreign = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.checkout-foreign");
            StableId<ReservationIdScope> reservation =
                ReservationId("reservation.checkout-foreign");
            AddCheckoutReservation(owner, item, reservation);
            InventoryCheckoutConsumptionPlan plan = owner
                .PrepareCheckoutReservationConsumption(new[] { reservation }).Value;
            var malformed = new InventoryCheckoutConsumptionPlan(
                owner,
                owner.Revision,
                null);
            long ownerRevision = owner.Revision;
            long foreignRevision = foreign.Revision;

            Assert.That(owner.CommitPreparedCheckoutReservationConsumption(null).Error,
                Is.EqualTo(InventoryFailures.CheckoutConsumptionPlanInvalid));
            Assert.That(owner.CommitPreparedCheckoutReservationConsumption(malformed).Error,
                Is.EqualTo(InventoryFailures.CheckoutConsumptionPlanInvalid));
            Assert.That(foreign.CommitPreparedCheckoutReservationConsumption(plan).Error,
                Is.EqualTo(InventoryFailures.CheckoutConsumptionPlanInvalid));

            Assert.That(owner.Revision, Is.EqualTo(ownerRevision));
            Assert.That(foreign.Revision, Is.EqualTo(foreignRevision));
            Assert.That(owner.TryGetSerializedItem(item, out _), Is.True);
            Assert.That(owner.TryGetReservation(reservation, out _), Is.True);
            Assert.That(owner.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(foreign.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CheckoutConsumptionCommitRejectsStalePlanWithoutMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.checkout-stale");
            StableId<ReservationIdScope> reservation =
                ReservationId("reservation.checkout-stale");
            AddCheckoutReservation(authority, item, reservation);
            InventoryCheckoutConsumptionPlan plan = authority
                .PrepareCheckoutReservationConsumption(new[] { reservation }).Value;
            Assert.That(authority.TransferSerializedItem(item, Shelf).IsSuccess, Is.True);
            long staleRevision = authority.Revision;

            OperationResult result =
                authority.CommitPreparedCheckoutReservationConsumption(plan);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.CheckoutConsumptionPlanStale));
            Assert.That(authority.Revision, Is.EqualTo(staleRevision));
            Assert.That(authority.TryGetSerializedItem(item, out InventoryItemRecord record), Is.True);
            Assert.That(record.ContainerId, Is.EqualTo(Shelf));
            Assert.That(authority.TryGetReservation(reservation, out _), Is.True);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CheckoutConsumptionCommitConsumesExactSetOnceAndReplayIsStale()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> firstItem = ItemId("item.checkout-success-a");
            StableId<ItemInstanceIdScope> secondItem = ItemId("item.checkout-success-b");
            StableId<ReservationIdScope> firstReservation =
                ReservationId("reservation.checkout-success-a");
            StableId<ReservationIdScope> secondReservation =
                ReservationId("reservation.checkout-success-b");
            AddCheckoutReservation(authority, firstItem, firstReservation);
            AddCheckoutReservation(authority, secondItem, secondReservation);
            InventoryCheckoutConsumptionPlan plan = authority
                .PrepareCheckoutReservationConsumption(
                    new[] { secondReservation, firstReservation }).Value;
            long revision = authority.Revision;

            Assert.That(authority.CommitPreparedCheckoutReservationConsumption(plan).IsSuccess,
                Is.True);

            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.SerializedItemCount, Is.Zero);
            Assert.That(authority.ReservationCount, Is.Zero);
            Assert.That(authority.TryGetSerializedItem(firstItem, out _), Is.False);
            Assert.That(authority.TryGetSerializedItem(secondItem, out _), Is.False);
            long committedRevision = authority.Revision;

            OperationResult replay =
                authority.CommitPreparedCheckoutReservationConsumption(plan);
            Assert.That(replay.Error,
                Is.EqualTo(InventoryFailures.CheckoutConsumptionPlanStale));
            Assert.That(authority.Revision, Is.EqualTo(committedRevision));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CheckoutConsumptionPlanBecomesStaleWhenReservationWasAlreadyRemoved()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.checkout-removed");
            StableId<ReservationIdScope> reservation =
                ReservationId("reservation.checkout-removed");
            AddCheckoutReservation(authority, item, reservation);
            InventoryCheckoutConsumptionPlan plan = authority
                .PrepareCheckoutReservationConsumption(new[] { reservation }).Value;

            Assert.That(authority.ConsumeCheckoutReservations(new[] { reservation }).IsSuccess,
                Is.True);
            long consumedRevision = authority.Revision;
            Assert.That(authority.TryGetReservation(reservation, out _), Is.False);

            OperationResult result =
                authority.CommitPreparedCheckoutReservationConsumption(plan);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.CheckoutConsumptionPlanStale));
            Assert.That(authority.Revision, Is.EqualTo(consumedRevision));
            Assert.That(authority.SerializedItemCount, Is.Zero);
            Assert.That(authority.ReservationCount, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BatchTransferSplitsPositionButPreservesBatchIdentityAndTotal()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.split");
            authority.ReceiveBatch(
                batch, BatchProduct, Receiving, InventoryCondition.New, 10, BatchCost);

            Assert.That(authority.TransferBatch(batch, Receiving, Shelf, 4).IsSuccess, Is.True);

            Assert.That(authority.GetBatchQuantity(batch, Receiving).Value, Is.EqualTo(6));
            Assert.That(authority.GetBatchQuantity(batch, Shelf).Value, Is.EqualTo(4));
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.EqualTo(10));
            Assert.That(authority.BatchCount, Is.EqualTo(1));
            Assert.That(authority.TryGetBatch(batch, out InventoryBatchRecord record), Is.True);
            Assert.That(record.UnitCost, Is.EqualTo(BatchCost));
            Assert.That(authority.GetBatchPositions().Select(position => position.BatchId).Distinct().Single(),
                Is.EqualTo(batch));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReservedBatchQuantityCannotBeMovedFromItsPosition()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.transfer-lock");
            authority.ReceiveBatch(
                batch, BatchProduct, Receiving, InventoryCondition.New, 10, BatchCost);
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
            authority.ReceiveBatch(
                batch, BatchProduct, Receiving, InventoryCondition.New, 3, BatchCost);
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
            authority.ReceiveBatch(
                batch, BatchProduct, Receiving, InventoryCondition.New, 8, BatchCost);
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
            authority.ReceiveBatch(
                batch, BatchProduct, Receiving, InventoryCondition.New, 2, BatchCost);
            authority.ReserveBatch(reservation, ClaimId("claim.empty"), batch, Receiving, 2);

            Assert.That(authority.ConsumeReservation(reservation).IsSuccess, Is.True);
            Assert.That(authority.BatchCount, Is.Zero);
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BulkConsumeRemovesMultipleSerializedReservationsInOneRevision()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> firstItem = ItemId("item.bulk-a");
            StableId<ItemInstanceIdScope> secondItem = ItemId("item.bulk-b");
            StableId<ReservationIdScope> firstReservation =
                ReservationId("reservation.bulk-a");
            StableId<ReservationIdScope> secondReservation =
                ReservationId("reservation.bulk-b");
            authority.ReceiveSerializedItem(
                firstItem, SerializedProduct, Shelf, InventoryCondition.New, SerializedCost);
            authority.ReceiveSerializedItem(
                secondItem, SerializedProduct, Shelf, InventoryCondition.New, SerializedCost);
            authority.ReserveSerializedItem(
                firstReservation, ClaimId("claim.bulk-a"), firstItem);
            authority.ReserveSerializedItem(
                secondReservation, ClaimId("claim.bulk-b"), secondItem);
            long revision = authority.Revision;

            OperationResult result = authority.ConsumeReservations(
                new[] { secondReservation, firstReservation });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.SerializedItemCount, Is.Zero);
            Assert.That(authority.ReservationCount, Is.Zero);
            Assert.That(authority.GetTotalQuantity(SerializedProduct).Value, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BulkConsumeAggregatesBatchReservationsBeforeOneMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<BatchIdScope> batch = BatchId("batch.bulk");
            StableId<ReservationIdScope> first = ReservationId("reservation.batch-bulk-a");
            StableId<ReservationIdScope> second = ReservationId("reservation.batch-bulk-b");
            authority.ReceiveBatch(
                batch, BatchProduct, Receiving, InventoryCondition.New, 10, BatchCost);
            authority.ReserveBatch(first, ClaimId("claim.batch-bulk-a"), batch, Receiving, 3);
            authority.ReserveBatch(second, ClaimId("claim.batch-bulk-b"), batch, Receiving, 4);
            long revision = authority.Revision;

            OperationResult result = authority.ConsumeReservations(new[] { first, second });

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.GetBatchQuantity(batch, Receiving).Value, Is.EqualTo(3));
            Assert.That(authority.ReservationCount, Is.Zero);
            Assert.That(authority.GetAvailableQuantity(BatchProduct).Value, Is.EqualTo(3));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InvalidBulkReservationSetsFailWithoutPartialConsumption()
        {
            InventoryAuthority authority = CreateAuthority();
            StableId<ItemInstanceIdScope> item = ItemId("item.bulk-failure");
            StableId<ReservationIdScope> reservation =
                ReservationId("reservation.bulk-failure");
            authority.ReceiveSerializedItem(
                item, SerializedProduct, Shelf, InventoryCondition.New, SerializedCost);
            authority.ReserveSerializedItem(
                reservation, ClaimId("claim.bulk-failure"), item);
            long revision = authority.Revision;

            Assert.That(authority.ConsumeReservations(null).Error,
                Is.EqualTo(InventoryFailures.MissingReservationSet));
            Assert.That(authority.ConsumeReservations(
                    System.Array.Empty<StableId<ReservationIdScope>>()).Error,
                Is.EqualTo(InventoryFailures.EmptyReservationSet));
            Assert.That(authority.ConsumeReservations(
                    new[] { reservation, reservation }).Error,
                Is.EqualTo(InventoryFailures.DuplicateReservationInSet));
            Assert.That(authority.ConsumeReservations(
                    new[] { reservation, ReservationId("reservation.unknown") }).Error,
                Is.EqualTo(InventoryFailures.UnknownReservation));

            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TryGetSerializedItem(item, out _), Is.True);
            Assert.That(authority.TryGetReservation(reservation, out _), Is.True);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void QueriesReturnDeterministicStableIdOrder()
        {
            InventoryAuthority authority = CreateAuthority();
            authority.ReceiveSerializedItem(
                ItemId("item.zeta"), SerializedProduct, Receiving, InventoryCondition.New,
                SerializedCost);
            authority.ReceiveSerializedItem(
                ItemId("item.alpha"), SerializedProduct, Receiving, InventoryCondition.New,
                SerializedCost);
            authority.ReceiveBatch(
                BatchId("batch.zeta"), BatchProduct, Receiving, InventoryCondition.New, 1,
                BatchCost);
            authority.ReceiveBatch(
                BatchId("batch.alpha"), BatchProduct, Shelf, InventoryCondition.New, 1,
                BatchCost);

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

        [Test]
        public void InvalidUnitCostReceiptsFailWithoutAnyMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            long revision = authority.Revision;

            OperationResult itemResult = authority.ReceiveSerializedItem(
                ItemId("item.invalid-cost"),
                SerializedProduct,
                Receiving,
                InventoryCondition.New,
                default);
            OperationResult batchResult = authority.ReceiveBatch(
                BatchId("batch.invalid-cost"),
                BatchProduct,
                Receiving,
                InventoryCondition.New,
                3,
                default);

            Assert.That(itemResult.Error, Is.EqualTo(InventoryFailures.InvalidUnitCost));
            Assert.That(batchResult.Error, Is.EqualTo(InventoryFailures.InvalidUnitCost));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.SerializedItemCount, Is.Zero);
            Assert.That(authority.BatchCount, Is.Zero);
            Assert.That(authority.GetContainerQuantity(Receiving).Value, Is.Zero);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
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

        private static void AddCheckoutReservation(
            InventoryAuthority authority,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId)
        {
            Assert.That(authority.ReceiveSerializedItem(
                itemId,
                SerializedProduct,
                Receiving,
                InventoryCondition.New,
                SerializedCost).IsSuccess, Is.True);
            OperationResult<InventorySerializedReservationPlan> prepared =
                authority.PrepareSerializedItemReservationForConsumption(
                    reservationId,
                    ClaimId($"claim.{reservationId.Value}"),
                    itemId);
            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(authority.CommitPreparedSerializedItemReservation(prepared.Value).IsSuccess,
                Is.True);
            Assert.That(authority.TryGetReservation(
                reservationId, out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ReleasePolicy,
                Is.EqualTo(InventoryReservationReleasePolicy.ConsumeOnly));
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

        private static InventoryUnitCost UnitCost(string currencyCode, long minorUnits) =>
            InventoryUnitCost.Create(currencyCode, minorUnits).Value;
    }
}
