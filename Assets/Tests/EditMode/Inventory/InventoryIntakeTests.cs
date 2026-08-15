using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Inventory
{
    public sealed class InventoryIntakeTests
    {
        private static readonly StableId<ProductDefinitionIdScope> SerializedProduct = ProductId("intake.serialized");
        private static readonly StableId<ProductDefinitionIdScope> BatchProduct = ProductId("intake.batch");
        private static readonly StableId<ContainerIdScope> Receiving = ContainerId("intake.receiving");

        [Test]
        public void IntakeOrdersEntriesByStableStockIdentity()
        {
            InventoryIntake intake = InventoryIntake.Create(
                new[]
                {
                    Serialized("item.zeta", SerializedProduct),
                    Serialized("item.alpha", SerializedProduct)
                },
                new[]
                {
                    Batch("batch.zeta", BatchProduct, 3),
                    Batch("batch.alpha", BatchProduct, 2)
                }).Value;

            Assert.That(intake.SerializedItems[0].ItemId.Value, Is.EqualTo("item.alpha"));
            Assert.That(intake.SerializedItems[1].ItemId.Value, Is.EqualTo("item.zeta"));
            Assert.That(intake.Batches[0].BatchId.Value, Is.EqualTo("batch.alpha"));
            Assert.That(intake.Batches[1].BatchId.Value, Is.EqualTo("batch.zeta"));
            Assert.That(intake.UnitQuantity, Is.EqualTo(7));
        }

        [Test]
        public void IntakeRejectsEmptyNullAndDuplicateEntries()
        {
            InventorySerializedIntake duplicate = Serialized("item.duplicate", SerializedProduct);
            InventoryBatchIntake duplicateBatch = Batch("batch.duplicate", BatchProduct, 1);

            Assert.That(InventoryIntake.Create(null, null).Error, Is.EqualTo(InventoryFailures.EmptyIntake));
            Assert.That(InventoryIntake.Create(
                new InventorySerializedIntake[] { null }, null).Error,
                Is.EqualTo(InventoryFailures.NullIntakeEntry));
            Assert.That(InventoryIntake.Create(
                new[] { duplicate, duplicate }, null).Error,
                Is.EqualTo(InventoryFailures.DuplicateIntakeItem));
            Assert.That(InventoryIntake.Create(
                null, new[] { duplicateBatch, duplicateBatch }).Error,
                Is.EqualTo(InventoryFailures.DuplicateIntakeBatch));
        }

        [Test]
        public void MixedIntakeCommitsAllStockWithOneRevision()
        {
            InventoryAuthority authority = CreateAuthority(20);
            InventoryIntake intake = InventoryIntake.Create(
                new[]
                {
                    Serialized("item.one", SerializedProduct),
                    Serialized("item.two", SerializedProduct)
                },
                new[] { Batch("batch.one", BatchProduct, 5) }).Value;
            long revision = authority.Revision;

            Assert.That(authority.ReceiveIntake(Receiving, intake).IsSuccess, Is.True);

            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.GetTotalQuantity(SerializedProduct).Value, Is.EqualTo(2));
            Assert.That(authority.GetTotalQuantity(BatchProduct).Value, Is.EqualTo(5));
            Assert.That(authority.GetContainerQuantity(Receiving).Value, Is.EqualTo(7));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void IntakeCapacityFailureCommitsNothing()
        {
            InventoryAuthority authority = CreateAuthority(3);
            InventoryIntake intake = InventoryIntake.Create(
                new[] { Serialized("item.capacity", SerializedProduct) },
                new[] { Batch("batch.capacity", BatchProduct, 3) }).Value;
            long revision = authority.Revision;

            OperationResult result = authority.ReceiveIntake(Receiving, intake);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.SerializedItemCount, Is.Zero);
            Assert.That(authority.BatchCount, Is.Zero);
            Assert.That(authority.GetContainerQuantity(Receiving).Value, Is.Zero);
        }

        [Test]
        public void ExistingIdentityOnLaterEntryPreventsEveryIntakeMutation()
        {
            InventoryAuthority authority = CreateAuthority(20);
            authority.ReceiveSerializedItem(
                ItemId("item.existing"), SerializedProduct, Receiving, InventoryCondition.New);
            InventoryIntake intake = InventoryIntake.Create(
                new[]
                {
                    Serialized("item.fresh", SerializedProduct),
                    Serialized("item.existing", SerializedProduct)
                },
                null).Value;
            long revision = authority.Revision;

            OperationResult result = authority.ReceiveIntake(Receiving, intake);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.DuplicateItem));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TryGetSerializedItem(ItemId("item.fresh"), out _), Is.False);
            Assert.That(authority.SerializedItemCount, Is.EqualTo(1));
        }

        [Test]
        public void TrackingMismatchPreventsEveryIntakeMutation()
        {
            InventoryAuthority authority = CreateAuthority(20);
            InventoryIntake intake = InventoryIntake.Create(
                new[] { Serialized("item.wrong-policy", BatchProduct) },
                null).Value;
            long revision = authority.Revision;

            OperationResult result = authority.ReceiveIntake(Receiving, intake);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.TrackingMismatch));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.SerializedItemCount, Is.Zero);
        }

        private static InventoryAuthority CreateAuthority(int capacity)
        {
            ProductCatalog catalog = CreateCatalog();
            InventoryAuthority authority = InventoryAuthority.Create(catalog).Value;
            authority.RegisterContainer(InventoryContainerDefinition.Create(
                Receiving, InventoryContainerKind.Receiving, capacity).Value);
            return authority;
        }

        private static ProductCatalog CreateCatalog()
        {
            ProductDefinition serialized = ProductDefinition.Create(
                SerializedProduct,
                CategoryId("intake-category"),
                "Intake Serialized",
                ProductTrackingPolicy.SerializedInstance,
                365).Value;
            ProductDefinition batch = ProductDefinition.Create(
                BatchProduct,
                CategoryId("intake-category"),
                "Intake Batch",
                ProductTrackingPolicy.BatchQuantity,
                0).Value;
            return ProductCatalog.Create(new[] { serialized, batch }).Value;
        }

        private static InventorySerializedIntake Serialized(
            string itemId,
            StableId<ProductDefinitionIdScope> productId)
        {
            return InventorySerializedIntake.Create(
                ItemId(itemId), productId, InventoryCondition.New).Value;
        }

        private static InventoryBatchIntake Batch(
            string batchId,
            StableId<ProductDefinitionIdScope> productId,
            int quantity)
        {
            return InventoryBatchIntake.Create(
                BatchId(batchId), productId, InventoryCondition.New, quantity).Value;
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
    }
}
