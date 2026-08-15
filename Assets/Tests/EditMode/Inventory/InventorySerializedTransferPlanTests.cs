using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Inventory
{
    public sealed class InventorySerializedTransferPlanTests
    {
        private static readonly StableId<ProductDefinitionIdScope> ProductId =
            StableId<ProductDefinitionIdScope>.Parse("component.motherboard-matx");
        private static readonly StableId<ContainerIdScope> Hands =
            StableId<ContainerIdScope>.Parse("container.actor-hands");
        private static readonly StableId<ContainerIdScope> Workbench =
            StableId<ContainerIdScope>.Parse("container.assembly-workbench");
        private static readonly StableId<ItemInstanceIdScope> Item =
            StableId<ItemInstanceIdScope>.Parse("item.motherboard-001");

        [Test]
        public void PrepareIsSideEffectFreeAndBindsExactSourceTargetAndRevision()
        {
            InventoryAuthority authority = CreateAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferPlan> prepared =
                authority.PrepareSerializedItemTransfer(Item, Workbench);

            Assert.That(prepared.IsSuccess, Is.True);
            Assert.That(prepared.Value.Owner, Is.SameAs(authority));
            Assert.That(prepared.Value.ExpectedRevision, Is.EqualTo(revision));
            Assert.That(prepared.Value.ItemId, Is.EqualTo(Item));
            Assert.That(prepared.Value.SourceContainerId, Is.EqualTo(Hands));
            Assert.That(prepared.Value.TargetContainerId, Is.EqualTo(Workbench));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
        }

        [Test]
        public void CommitMovesExactItemOnceAndReplayIsStaleWithoutMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            InventorySerializedTransferPlan plan =
                authority.PrepareSerializedItemTransfer(Item, Workbench).Value;
            long revision = authority.Revision;

            Assert.That(authority.CommitPreparedSerializedItemTransfer(plan).IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord moved), Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(Workbench));
            long committedRevision = authority.Revision;

            Assert.That(authority.CommitPreparedSerializedItemTransfer(plan).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanStale));
            Assert.That(authority.Revision, Is.EqualTo(committedRevision));
            Assert.That(authority.TryGetSerializedItem(Item, out moved), Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(Workbench));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CommitRejectsForeignMalformedAndStalePlansWithoutMutation()
        {
            InventoryAuthority owner = CreateAuthority();
            InventoryAuthority foreign = CreateAuthority();
            InventorySerializedTransferPlan plan =
                owner.PrepareSerializedItemTransfer(Item, Workbench).Value;
            var malformed = new InventorySerializedTransferPlan(
                owner,
                owner.Revision,
                Item,
                default,
                Workbench);
            long ownerRevision = owner.Revision;
            long foreignRevision = foreign.Revision;

            Assert.That(owner.CommitPreparedSerializedItemTransfer(null).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanInvalid));
            Assert.That(owner.CommitPreparedSerializedItemTransfer(malformed).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanInvalid));
            Assert.That(foreign.CommitPreparedSerializedItemTransfer(plan).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanInvalid));
            Assert.That(owner.Revision, Is.EqualTo(ownerRevision));
            Assert.That(foreign.Revision, Is.EqualTo(foreignRevision));

            InventoryContainerDefinition quarantine = InventoryContainerDefinition.Create(
                StableId<ContainerIdScope>.Parse("container.quarantine"),
                InventoryContainerKind.Quarantine,
                4).Value;
            Assert.That(owner.RegisterContainer(quarantine).IsSuccess, Is.True);
            long staleRevision = owner.Revision;
            Assert.That(owner.CommitPreparedSerializedItemTransfer(plan).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanStale));
            Assert.That(owner.Revision, Is.EqualTo(staleRevision));
            Assert.That(owner.TryGetSerializedItem(Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
        }

        [Test]
        public void PrepareFailuresAndRevisionOverflowLeaveItemUntouched()
        {
            InventoryAuthority authority = CreateAuthority(workbenchCapacity: 1, fillWorkbench: true);
            long revision = authority.Revision;

            Assert.That(authority.PrepareSerializedItemTransfer(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord item), Is.True);
            Assert.That(item.ContainerId, Is.EqualTo(Hands));

            InventoryAuthority overflow = CreateAuthority();
            PropertyInfo revisionProperty = typeof(InventoryAuthority).GetProperty(
                nameof(InventoryAuthority.Revision),
                BindingFlags.Instance | BindingFlags.Public);
            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                overflow,
                new object[] { long.MaxValue });
            Assert.That(overflow.PrepareSerializedItemTransfer(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.TryGetSerializedItem(Item, out item), Is.True);
            Assert.That(item.ContainerId, Is.EqualTo(Hands));
        }

        [Test]
        public void ManagedContainerClaimAdvancesRevisionStalesPreparedPlanAndDuplicateIsNoOp()
        {
            InventoryAuthority authority = CreateAuthority();
            InventorySerializedTransferPlan prepared =
                authority.PrepareSerializedItemTransfer(Item, Workbench).Value;
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccess> claim =
                authority.ClaimManagedSerializedTransferContainer(Workbench);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.CommitPreparedSerializedItemTransfer(prepared).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanStale));
            Assert.That(authority.TransferSerializedItem(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            long claimedRevision = authority.Revision;

            Assert.That(authority.ClaimManagedSerializedTransferContainer(Workbench).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.Revision, Is.EqualTo(claimedRevision));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerClaimRejectsOccupiedContainerWithoutGhostCustody()
        {
            InventoryAuthority authority = CreateAuthority(fillWorkbench: true);
            StableId<ItemInstanceIdScope> occupied =
                StableId<ItemInstanceIdScope>.Parse("item.motherboard-occupied");
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccess> claim =
                authority.ClaimManagedSerializedTransferContainer(Workbench);

            Assert.That(claim.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TransferSerializedItem(occupied, Hands).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(
                occupied, out InventoryItemRecord moved), Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static InventoryAuthority CreateAuthority(
            int workbenchCapacity = 2,
            bool fillWorkbench = false)
        {
            ProductDefinition product = ProductDefinition.Create(
                ProductId,
                StableId<ProductCategoryIdScope>.Parse("pc-components"),
                "Micro ATX Motherboard",
                ProductTrackingPolicy.SerializedInstance,
                730).Value;
            ProductCatalog catalog = ProductCatalog.Create(new[] { product }).Value;
            InventoryAuthority authority = InventoryAuthority.Create(catalog).Value;
            authority.RegisterContainer(InventoryContainerDefinition.Create(
                Hands,
                InventoryContainerKind.ActorHands,
                2).Value);
            authority.RegisterContainer(InventoryContainerDefinition.Create(
                Workbench,
                InventoryContainerKind.Workbench,
                workbenchCapacity).Value);
            authority.ReceiveSerializedItem(
                Item,
                ProductId,
                Hands,
                InventoryCondition.New,
                InventoryUnitCost.Create("EUR", 12_900).Value);

            if (fillWorkbench)
            {
                authority.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse("item.motherboard-occupied"),
                    ProductId,
                    Workbench,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value);
            }

            return authority;
        }
    }
}
