using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;

namespace PCShopEmpire3D.Tests.EditMode.Inventory
{
    public sealed class InventorySerializedReservationSetTests
    {
        [Test]
        public void ExactSetReservesAtomicallyInOneRevisionAndReplaysWithoutMutation()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            InventorySerializedReservationRequest[] requests =
            {
                Request("atomic-motherboard", session.MotherboardItemId),
                Request("atomic-processor", session.ProcessorItemId)
            };
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            Assert.That(inventory.ReserveSerializedItems(requests).IsSuccess, Is.True);
            Assert.That(inventory.Revision, Is.EqualTo(revision + 1));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount + 2));
            AssertExactReservation(inventory, requests[0]);
            AssertExactReservation(inventory, requests[1]);

            long committedRevision = inventory.Revision;
            Assert.That(inventory.ReserveSerializedItems(requests).IsSuccess, Is.True);
            Assert.That(inventory.Revision, Is.EqualTo(committedRevision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount + 2));
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ConflictingMemberRejectsWholeSetWithoutPartialReservation()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            Assert.That(inventory.ReserveSerializedItem(
                ReservationId("inventory.reservation.set-conflict-existing"),
                ClaimId("inventory.claim.set-conflict-existing"),
                session.ProcessorItemId).IsSuccess, Is.True);
            InventorySerializedReservationRequest first =
                Request("conflict-motherboard", session.MotherboardItemId);
            InventorySerializedReservationRequest second =
                Request("conflict-processor", session.ProcessorItemId);
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            OperationResult result = inventory.ReserveSerializedItems(
                new[] { first, second });

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ItemAlreadyReserved));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(inventory.TryGetReservation(first.ReservationId, out _), Is.False);
            Assert.That(inventory.TryGetReservation(second.ReservationId, out _), Is.False);
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PartialReplayFailsClosedWithoutPublishingNewMember()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            InventorySerializedReservationRequest existing =
                Request("partial-existing", session.MotherboardItemId);
            InventorySerializedReservationRequest added =
                Request("partial-added", session.MemoryItemId);
            Assert.That(inventory.ReserveSerializedItems(
                new[] { existing }).IsSuccess, Is.True);
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            OperationResult result = inventory.ReserveSerializedItems(
                new[] { existing, added });

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.PartialSerializedReservationReplay));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(inventory.TryGetReservation(added.ReservationId, out _), Is.False);
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PreparedSetBecomesStaleAfterUnrelatedInventoryMutation()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            InventorySerializedReservationRequest[] requests =
            {
                Request("stale-motherboard", session.MotherboardItemId),
                Request("stale-processor", session.ProcessorItemId)
            };
            OperationResult<InventorySerializedReservationSetPlan> prepared =
                inventory.PrepareSerializedItemReservationSet(requests);
            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(inventory.ReserveSerializedItem(
                ReservationId("inventory.reservation.set-stale-unrelated"),
                ClaimId("inventory.claim.set-stale-unrelated"),
                session.MemoryItemId).IsSuccess, Is.True);
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            OperationResult result =
                inventory.CommitPreparedSerializedItemReservationSet(prepared.Value);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationSetPlanStale));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(inventory.TryGetReservation(requests[0].ReservationId, out _), Is.False);
            Assert.That(inventory.TryGetReservation(requests[1].ReservationId, out _), Is.False);
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void DuplicateReservationOrItemIdentityFailsWithoutMutation()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            InventorySerializedReservationRequest first =
                Request("duplicate", session.MotherboardItemId);
            InventorySerializedReservationRequest sameReservation =
                Request("duplicate", session.ProcessorItemId);
            InventorySerializedReservationRequest sameItem =
                Request("duplicate-item-second-id", session.MotherboardItemId);
            long revision = inventory.Revision;

            Assert.That(inventory.ReserveSerializedItems(
                    new[] { first, sameReservation }).Error,
                Is.EqualTo(InventoryFailures.DuplicateSerializedReservationRequest));
            Assert.That(inventory.ReserveSerializedItems(
                    new[] { first, sameItem }).Error,
                Is.EqualTo(InventoryFailures.DuplicateSerializedReservationItem));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.Zero);
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedSerializedReservationSetExactReplayReturnsSameAccessWithoutMutation()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            StableId<InventoryClaimIdScope> claimId =
                ClaimId("inventory.claim.set-test.managed-replay");
            StableId<InventorySerializedReservationSetOperationIdScope> operationId =
                OperationId("inventory.operation.set-test.managed-replay");
            InventorySerializedReservationRequest first = ManagedRequest(
                "managed-replay-motherboard",
                claimId,
                session.MotherboardItemId);
            InventorySerializedReservationRequest second = ManagedRequest(
                "managed-replay-processor",
                claimId,
                session.ProcessorItemId);
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            OperationResult<InventorySerializedReservationSetAccess> committed =
                inventory.ReserveManagedSerializedItems(
                    operationId,
                    new[] { first, second });

            Assert.That(committed.IsSuccess, Is.True);
            Assert.That(committed.Value.OperationId, Is.EqualTo(operationId));
            Assert.That(committed.Value.AppliedRevision, Is.EqualTo(revision + 1));
            Assert.That(inventory.Revision, Is.EqualTo(revision + 1));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount + 2));
            long committedRevision = inventory.Revision;

            OperationResult<InventorySerializedReservationSetAccess> replay =
                inventory.ReserveManagedSerializedItems(
                    operationId,
                    new[] { second, first });

            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(committed.Value));
            Assert.That(inventory.Revision, Is.EqualTo(committedRevision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount + 2));
            Assert.That(inventory.OwnsManagedSerializedReservationSet(replay.Value), Is.True);
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedSerializedReservationSetStoredOperationDriftFailsClosed()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            StableId<InventoryClaimIdScope> claimId =
                ClaimId("inventory.claim.set-test.operation-drift");
            StableId<InventorySerializedReservationSetOperationIdScope> operationId =
                OperationId("inventory.operation.set-test.operation-drift");
            InventorySerializedReservationRequest[] requests =
            {
                ManagedRequest(
                    "operation-drift-motherboard",
                    claimId,
                    session.MotherboardItemId),
                ManagedRequest(
                    "operation-drift-processor",
                    claimId,
                    session.ProcessorItemId)
            };
            OperationResult<InventorySerializedReservationSetAccess> committed =
                inventory.ReserveManagedSerializedItems(operationId, requests);
            Assert.That(committed.IsSuccess, Is.True);
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            SetAccessBackingField(
                committed.Value,
                "<OperationId>k__BackingField",
                OperationId("inventory.operation.set-test.operation-drift.forged"));

            Assert.That(inventory.OwnsManagedSerializedReservationSet(
                committed.Value), Is.False);
            Assert.That(inventory.ValidateInvariants().Error,
                Is.EqualTo(InventoryFailures.InvariantViolation));
            OperationResult<InventorySerializedReservationSetAccess> replay =
                inventory.ReserveManagedSerializedItems(operationId, requests);
            Assert.That(replay.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationSetAccessInvalid));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount));
        }

        [Test]
        public void ManagedSerializedReservationSetStoredAppliedRevisionDriftFailsClosed()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            StableId<InventoryClaimIdScope> claimId =
                ClaimId("inventory.claim.set-test.revision-drift");
            StableId<InventorySerializedReservationSetOperationIdScope> operationId =
                OperationId("inventory.operation.set-test.revision-drift");
            InventorySerializedReservationRequest[] requests =
            {
                ManagedRequest(
                    "revision-drift-motherboard",
                    claimId,
                    session.MotherboardItemId),
                ManagedRequest(
                    "revision-drift-processor",
                    claimId,
                    session.ProcessorItemId)
            };
            OperationResult<InventorySerializedReservationSetAccess> committed =
                inventory.ReserveManagedSerializedItems(operationId, requests);
            Assert.That(committed.IsSuccess, Is.True);
            Assert.That(committed.Value.AppliedRevision, Is.GreaterThan(1));
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            SetAccessBackingField(
                committed.Value,
                "<AppliedRevision>k__BackingField",
                committed.Value.AppliedRevision - 1);

            Assert.That(inventory.OwnsManagedSerializedReservationSet(
                committed.Value), Is.False);
            Assert.That(inventory.ValidateInvariants().Error,
                Is.EqualTo(InventoryFailures.InvariantViolation));
            OperationResult<InventorySerializedReservationSetAccess> replay =
                inventory.ReserveManagedSerializedItems(operationId, requests);
            Assert.That(replay.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationSetAccessInvalid));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount));
        }

        [Test]
        public void ManagedSerializedReservationSetSameClaimAndPayloadWithDifferentOperationIdCannotAdoptAccess()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            StableId<InventoryClaimIdScope> claimId =
                ClaimId("inventory.claim.set-test.managed-adoption");
            InventorySerializedReservationRequest[] requests =
            {
                ManagedRequest(
                    "managed-adoption-motherboard",
                    claimId,
                    session.MotherboardItemId),
                ManagedRequest(
                    "managed-adoption-processor",
                    claimId,
                    session.ProcessorItemId)
            };
            Assert.That(inventory.ReserveManagedSerializedItems(
                OperationId("inventory.operation.set-test.managed-owner"),
                requests).IsSuccess, Is.True);
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            OperationResult<InventorySerializedReservationSetAccess> result =
                inventory.ReserveManagedSerializedItems(
                    OperationId("inventory.operation.set-test.managed-foreign"),
                    requests);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationSetOperationConflict));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedSerializedReservationSetSameOperationWithDifferentPayloadFailsClosed()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived(true);
            InventoryAuthority inventory = session.Inventory;
            StableId<InventoryClaimIdScope> claimId =
                ClaimId("inventory.claim.set-test.managed-payload");
            StableId<InventorySerializedReservationSetOperationIdScope> operationId =
                OperationId("inventory.operation.set-test.managed-payload");
            InventorySerializedReservationRequest first = ManagedRequest(
                "managed-payload-motherboard",
                claimId,
                session.MotherboardItemId);
            InventorySerializedReservationRequest second = ManagedRequest(
                "managed-payload-processor",
                claimId,
                session.ProcessorItemId);
            Assert.That(inventory.ReserveManagedSerializedItems(
                operationId,
                new[] { first, second }).IsSuccess, Is.True);
            InventorySerializedReservationRequest changed =
                InventorySerializedReservationRequest.Create(
                    second.ReservationId,
                    claimId,
                    session.MemoryItemId).Value;
            long revision = inventory.Revision;
            int reservationCount = inventory.ReservationCount;

            OperationResult<InventorySerializedReservationSetAccess> result =
                inventory.ReserveManagedSerializedItems(
                    operationId,
                    new[] { first, changed });

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.SerializedReservationSetOperationConflict));
            Assert.That(inventory.Revision, Is.EqualTo(revision));
            Assert.That(inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        private static InventorySerializedReservationRequest Request(
            string suffix,
            StableId<ItemInstanceIdScope> itemId)
        {
            return InventorySerializedReservationRequest.Create(
                ReservationId($"inventory.reservation.set-test.{suffix}"),
                ClaimId($"inventory.claim.set-test.{suffix}"),
                itemId).Value;
        }

        private static InventorySerializedReservationRequest ManagedRequest(
            string suffix,
            StableId<InventoryClaimIdScope> claimId,
            StableId<ItemInstanceIdScope> itemId)
        {
            return InventorySerializedReservationRequest.Create(
                ReservationId($"inventory.reservation.set-test.{suffix}"),
                claimId,
                itemId).Value;
        }

        private static void AssertExactReservation(
            InventoryAuthority inventory,
            InventorySerializedReservationRequest request)
        {
            Assert.That(inventory.TryGetReservation(
                request.ReservationId,
                out InventoryReservation reservation), Is.True);
            Assert.That(reservation.ClaimId, Is.EqualTo(request.ClaimId));
            Assert.That(reservation.ItemId, Is.EqualTo(request.ItemId));
            Assert.That(reservation.TargetKind,
                Is.EqualTo(InventoryReservationTargetKind.SerializedItem));
            Assert.That(reservation.Quantity, Is.EqualTo(1));
            Assert.That(reservation.ReleasePolicy,
                Is.EqualTo(InventoryReservationReleasePolicy.Releasable));
        }

        private static void SetAccessBackingField(
            InventorySerializedReservationSetAccess access,
            string fieldName,
            object value)
        {
            FieldInfo field = typeof(InventorySerializedReservationSetAccess)
                .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(access, value);
        }

        private static StableId<ReservationIdScope> ReservationId(string value) =>
            StableId<ReservationIdScope>.Parse(value);

        private static StableId<InventoryClaimIdScope> ClaimId(string value) =>
            StableId<InventoryClaimIdScope>.Parse(value);

        private static StableId<InventorySerializedReservationSetOperationIdScope>
            OperationId(string value) =>
                StableId<InventorySerializedReservationSetOperationIdScope>.Parse(value);
    }
}
