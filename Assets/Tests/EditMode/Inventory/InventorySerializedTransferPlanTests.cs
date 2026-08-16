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
        private static readonly StableId<ContainerIdScope> ProcessorSocket =
            StableId<ContainerIdScope>.Parse("container.processor-socket");
        private static readonly StableId<ContainerIdScope> MemorySlot =
            StableId<ContainerIdScope>.Parse("container.memory-slot-a2");
        private static readonly StableId<ItemInstanceIdScope> Item =
            StableId<ItemInstanceIdScope>.Parse("item.motherboard-001");
        private static readonly StableId<ItemInstanceIdScope> MemorySlotItem =
            StableId<ItemInstanceIdScope>.Parse("item.memory-slot-occupied");

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

        [Test]
        public void ManagedContainerPairClaimSucceedsInOneRevisionAndBlocksBothPublicPaths()
        {
            InventoryAuthority authority = CreateAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessPair> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(authority.TransferSerializedItem(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TransferSerializedItem(Item, ProcessorSocket).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerPairConflictLeavesUnclaimedPeerPubliclyAvailable()
        {
            InventoryAuthority authority = CreateAuthority();
            Assert.That(authority.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessPair> conflict =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket);

            Assert.That(conflict.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.TransferSerializedItem(Item, Workbench).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord moved),
                Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(Workbench));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerTripleClaimSucceedsInOneRevisionAndBlocksAllPublicPaths()
        {
            InventoryAuthority authority = CreateTripleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessTriple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, Workbench, claim.Value.First).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, ProcessorSocket, claim.Value.Second).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, MemorySlot, claim.Value.Third).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, MemorySlot, claim.Value.First).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferAccessInvalid));
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, Workbench, claim.Value.Third).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferAccessInvalid));
            Assert.That(authority.TransferSerializedItem(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TransferSerializedItem(Item, ProcessorSocket).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TransferSerializedItem(Item, MemorySlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerTripleConflictAndOccupiedPeerLeaveEveryUnclaimedPeerPublic()
        {
            InventoryAuthority managedConflict = CreateTripleAuthority();
            Assert.That(managedConflict.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            long managedRevision = managedConflict.Revision;

            OperationResult<InventorySerializedTransferAccessTriple> conflict =
                managedConflict.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot);

            Assert.That(conflict.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedConflict.Revision, Is.EqualTo(managedRevision));
            Assert.That(managedConflict.TransferSerializedItem(Item, Workbench).IsSuccess, Is.True);
            Assert.That(managedConflict.TransferSerializedItem(Item, MemorySlot).IsSuccess, Is.True);

            InventoryAuthority occupiedConflict = CreateTripleAuthority(fillMemorySlot: true);
            long occupiedRevision = occupiedConflict.Revision;
            OperationResult<InventorySerializedTransferAccessTriple> occupied =
                occupiedConflict.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot);

            Assert.That(occupied.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedConflict.Revision, Is.EqualTo(occupiedRevision));
            Assert.That(occupiedConflict.TransferSerializedItem(Item, Workbench).IsSuccess, Is.True);
            Assert.That(occupiedConflict.TransferSerializedItem(Item, ProcessorSocket).IsSuccess,
                Is.True);
            Assert.That(occupiedConflict.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerTripleUsesDeterministicValidationPrecedence()
        {
            InventoryAuthority authority = CreateTripleAuthority();
            StableId<ContainerIdScope> unknown =
                StableId<ContainerIdScope>.Parse("container.unknown");

            Assert.That(authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    unknown).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));
            Assert.That(authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot).Error,
                Is.EqualTo(InventoryFailures.SameContainer));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);

            InventoryAuthority sameBeforeManaged = CreateTripleAuthority();
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            long sameRevision = sameBeforeManaged.Revision;
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    ProcessorSocket).Error,
                Is.EqualTo(InventoryFailures.SameContainer));
            Assert.That(sameBeforeManaged.Revision, Is.EqualTo(sameRevision));

            InventoryAuthority managedBeforeOccupiedAndOverflow =
                CreateTripleAuthority(fillMemorySlot: true);
            Assert.That(managedBeforeOccupiedAndOverflow
                .ClaimManagedSerializedTransferContainer(ProcessorSocket).IsSuccess, Is.True);
            SetRevision(managedBeforeOccupiedAndOverflow, long.MaxValue);
            Assert.That(managedBeforeOccupiedAndOverflow
                    .ClaimManagedSerializedTransferContainers(
                        Workbench,
                        ProcessorSocket,
                        MemorySlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedBeforeOccupiedAndOverflow.Revision, Is.EqualTo(long.MaxValue));

            InventoryAuthority occupiedBeforeOverflow =
                CreateTripleAuthority(fillMemorySlot: true);
            SetRevision(occupiedBeforeOverflow, long.MaxValue);
            Assert.That(occupiedBeforeOverflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedBeforeOverflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(sameBeforeManaged.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(managedBeforeOccupiedAndOverflow.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(occupiedBeforeOverflow.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerTripleReservedMemorySlotFailsWithoutMutation()
        {
            InventoryAuthority authority = CreateTripleAuthority(fillMemorySlot: true);
            StableId<ReservationIdScope> reservationId =
                StableId<ReservationIdScope>.Parse("reservation.memory-slot");
            StableId<InventoryClaimIdScope> claimId =
                StableId<InventoryClaimIdScope>.Parse("claim.memory-slot");
            Assert.That(authority.ReserveSerializedItem(
                reservationId,
                claimId,
                MemorySlotItem).IsSuccess, Is.True);
            long revision = authority.Revision;
            int reservationCount = authority.ReservationCount;

            OperationResult<InventorySerializedTransferAccessTriple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot);

            Assert.That(claim.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(authority.GetContainerQuantity(Workbench).Value, Is.Zero);
            Assert.That(authority.GetContainerQuantity(ProcessorSocket).Value, Is.Zero);
            Assert.That(authority.GetContainerQuantity(MemorySlot).Value, Is.EqualTo(1));
            Assert.That(authority.TryGetSerializedItem(
                MemorySlotItem, out InventoryItemRecord reserved), Is.True);
            Assert.That(reserved.ContainerId, Is.EqualTo(MemorySlot));
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, Workbench).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, ProcessorSocket).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, MemorySlot).Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerTripleAtRevisionMaxFailsWithoutGhostCustody()
        {
            InventoryAuthority authority = CreateTripleAuthority();

            PropertyInfo revisionProperty = typeof(InventoryAuthority).GetProperty(
                nameof(InventoryAuthority.Revision),
                BindingFlags.Instance | BindingFlags.Public);
            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                authority,
                new object[] { long.MaxValue });

            Assert.That(authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(authority.GetContainerQuantity(Workbench).Value, Is.Zero);
            Assert.That(authority.GetContainerQuantity(ProcessorSocket).Value, Is.Zero);
            Assert.That(authority.GetContainerQuantity(MemorySlot).Value, Is.Zero);
            Assert.That(authority.PrepareSerializedItemTransfer(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, ProcessorSocket).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, MemorySlot).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerTripleAtRevisionMaxMinusOneClaimsAllThreeAtomically()
        {
            InventoryAuthority authority = CreateTripleAuthority();
            PropertyInfo revisionProperty = typeof(InventoryAuthority).GetProperty(
                nameof(InventoryAuthority.Revision),
                BindingFlags.Instance | BindingFlags.Public);

            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                authority,
                new object[] { long.MaxValue - 1 });

            OperationResult<InventorySerializedTransferAccessTriple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, ProcessorSocket).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, MemorySlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.GetContainerQuantity(Workbench).Value, Is.Zero);
            Assert.That(authority.GetContainerQuantity(ProcessorSocket).Value, Is.Zero);
            Assert.That(authority.GetContainerQuantity(MemorySlot).Value, Is.Zero);
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void SetRevision(InventoryAuthority authority, long revision)
        {
            PropertyInfo revisionProperty = typeof(InventoryAuthority).GetProperty(
                nameof(InventoryAuthority.Revision),
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(revisionProperty, Is.Not.Null);
            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                authority,
                new object[] { revision });
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
            authority.RegisterContainer(InventoryContainerDefinition.Create(
                ProcessorSocket,
                InventoryContainerKind.Workbench,
                1).Value);
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

        private static InventoryAuthority CreateTripleAuthority(bool fillMemorySlot = false)
        {
            InventoryAuthority authority = CreateAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                MemorySlot,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillMemorySlot)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    MemorySlotItem,
                    ProductId,
                    MemorySlot,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }
    }
}
