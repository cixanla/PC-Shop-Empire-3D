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
        private static readonly StableId<ContainerIdScope> StorageSlot =
            StableId<ContainerIdScope>.Parse("container.storage-slot-m2-primary");
        private static readonly StableId<ContainerIdScope> BenchmarkSlot =
            StableId<ContainerIdScope>.Parse("container.benchmark-slot-primary");
        private static readonly StableId<ContainerIdScope> GraphicsCardSlot =
            StableId<ContainerIdScope>.Parse("container.graphics-card-slot-primary");
        private static readonly StableId<ContainerIdScope> PowerSupplyBay =
            StableId<ContainerIdScope>.Parse("container.power-supply-bay-primary");
        private static readonly StableId<ContainerIdScope> PowerCableRoute =
            StableId<ContainerIdScope>.Parse("container.power-cable-route-primary");
        private static readonly StableId<ContainerIdScope> CpuPowerCableRoute =
            StableId<ContainerIdScope>.Parse("container.cpu-power-cable-route-primary");
        private static readonly StableId<ContainerIdScope> GpuPowerCableRoute =
            StableId<ContainerIdScope>.Parse("container.gpu-power-cable-route-primary");
        private static readonly StableId<ItemInstanceIdScope> Item =
            StableId<ItemInstanceIdScope>.Parse("item.motherboard-001");
        private static readonly StableId<ItemInstanceIdScope> MemorySlotItem =
            StableId<ItemInstanceIdScope>.Parse("item.memory-slot-occupied");
        private static readonly StableId<ItemInstanceIdScope> StorageSlotItem =
            StableId<ItemInstanceIdScope>.Parse("item.storage-slot-occupied");
        private static readonly StableId<ItemInstanceIdScope> BenchmarkSlotItem =
            StableId<ItemInstanceIdScope>.Parse("item.benchmark-slot-occupied");
        private static readonly StableId<ItemInstanceIdScope> GraphicsCardSlotItem =
            StableId<ItemInstanceIdScope>.Parse("item.graphics-card-slot-occupied");
        private static readonly StableId<ItemInstanceIdScope> PowerSupplyBayItem =
            StableId<ItemInstanceIdScope>.Parse("item.power-supply-bay-occupied");
        private static readonly StableId<ItemInstanceIdScope> PowerCableRouteItem =
            StableId<ItemInstanceIdScope>.Parse("item.power-cable-route-occupied");
        private static readonly StableId<ItemInstanceIdScope> CpuPowerCableRouteItem =
            StableId<ItemInstanceIdScope>.Parse("item.cpu-power-cable-route-occupied");
        private static readonly StableId<ItemInstanceIdScope> GpuPowerCableRouteItem =
            StableId<ItemInstanceIdScope>.Parse("item.gpu-power-cable-route-occupied");

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
        public void PreAppliedConsumableStateCommitsAtomicallyAndSurvivesLaterTransfers()
        {
            InventoryAuthority authority = CreateAuthority();
            long revision = authority.Revision;
            InventorySerializedTransferPlan consumePlan = authority
                .PrepareSerializedItemTransferAndConsumePreAppliedState(
                    Item,
                    Workbench,
                    null).Value;

            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord beforeCommit), Is.True);
            Assert.That(beforeCommit.StateFlags,
                Is.EqualTo(InventorySerializedItemStateFlags.None));
            Assert.That(authority.Revision, Is.EqualTo(revision));
            Assert.That(authority.CommitPreparedSerializedItemTransfer(consumePlan).IsSuccess,
                Is.True);
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord consumed), Is.True);
            Assert.That(consumed.ContainerId, Is.EqualTo(Workbench));
            Assert.That(consumed.StateFlags,
                Is.EqualTo(
                    InventorySerializedItemStateFlags.PreAppliedConsumableConsumed));

            Assert.That(authority.TransferSerializedItem(Item, Hands).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord returned), Is.True);
            Assert.That(returned.StateFlags,
                Is.EqualTo(
                    InventorySerializedItemStateFlags.PreAppliedConsumableConsumed));
            long conflictRevision = authority.Revision;
            Assert.That(authority.PrepareSerializedItemTransferAndConsumePreAppliedState(
                    Item,
                    Workbench,
                    null).Error,
                Is.EqualTo(InventoryFailures.SerializedItemStateConflict));
            Assert.That(authority.Revision, Is.EqualTo(conflictRevision));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SerializedItemStateFlagValuesAreExplicitAndAppendOnly()
        {
            Assert.That((int)InventorySerializedItemStateFlags.None, Is.EqualTo(0));
            Assert.That(
                (int)InventorySerializedItemStateFlags
                    .PreAppliedConsumableConsumed,
                Is.EqualTo(1));
        }

        [Test]
        public void ConsumablePlanReplayAndCompetingPlanAreStaleWithoutMutation()
        {
            InventoryAuthority authority = CreateAuthority();
            InventorySerializedTransferPlan first = authority
                .PrepareSerializedItemTransferAndConsumePreAppliedState(
                    Item,
                    Workbench,
                    null).Value;
            InventorySerializedTransferPlan competing = authority
                .PrepareSerializedItemTransferAndConsumePreAppliedState(
                    Item,
                    Workbench,
                    null).Value;

            Assert.That(authority.CommitPreparedSerializedItemTransfer(first).IsSuccess,
                Is.True);
            long committedRevision = authority.Revision;
            Assert.That(authority.CommitPreparedSerializedItemTransfer(first).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanStale));
            Assert.That(authority.CommitPreparedSerializedItemTransfer(competing).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanStale));
            Assert.That(authority.Revision, Is.EqualTo(committedRevision));
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord consumed), Is.True);
            Assert.That(consumed.ContainerId, Is.EqualTo(Workbench));
            Assert.That(consumed.StateFlags,
                Is.EqualTo(
                    InventorySerializedItemStateFlags
                        .PreAppliedConsumableConsumed));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ConsumablePlanStaleAndCapacityFailuresNeverApplyState()
        {
            InventoryAuthority stale = CreateAuthority();
            InventorySerializedTransferPlan plan = stale
                .PrepareSerializedItemTransferAndConsumePreAppliedState(
                    Item,
                    Workbench,
                    null).Value;
            Assert.That(stale.RegisterContainer(
                InventoryContainerDefinition.Create(
                    StableId<ContainerIdScope>.Parse("container.consume-stale"),
                    InventoryContainerKind.Quarantine,
                    1).Value).IsSuccess, Is.True);
            long staleRevision = stale.Revision;

            Assert.That(stale.CommitPreparedSerializedItemTransfer(plan).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferPlanStale));
            Assert.That(stale.Revision, Is.EqualTo(staleRevision));
            Assert.That(stale.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(unchanged.StateFlags,
                Is.EqualTo(InventorySerializedItemStateFlags.None));

            InventoryAuthority full = CreateAuthority(
                workbenchCapacity: 1,
                fillWorkbench: true);
            long fullRevision = full.Revision;
            Assert.That(full.PrepareSerializedItemTransferAndConsumePreAppliedState(
                    Item,
                    Workbench,
                    null).Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(full.Revision, Is.EqualTo(fullRevision));
            Assert.That(full.TryGetSerializedItem(
                Item, out InventoryItemRecord capacityUnchanged), Is.True);
            Assert.That(capacityUnchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(capacityUnchanged.StateFlags,
                Is.EqualTo(InventorySerializedItemStateFlags.None));
            Assert.That(stale.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(full.ValidateInvariants().IsSuccess, Is.True);
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

        [Test]
        public void ManagedContainerQuadrupleClaimSucceedsInOneRevisionAndBlocksAllPublicPaths()
        {
            InventoryAuthority authority = CreateQuadrupleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessQuadruple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(claim.Value.Fourth.ManagedContainerId, Is.EqualTo(StorageSlot));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, ProcessorSocket).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, MemorySlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, StorageSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerQuadrupleConflictOrOccupiedFourthLeavesNoPartialClaim()
        {
            InventoryAuthority managedConflict = CreateQuadrupleAuthority();
            Assert.That(managedConflict.ClaimManagedSerializedTransferContainer(
                StorageSlot).IsSuccess, Is.True);
            long managedRevision = managedConflict.Revision;

            Assert.That(managedConflict.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedConflict.Revision, Is.EqualTo(managedRevision));
            Assert.That(managedConflict.PrepareSerializedItemTransfer(Item, Workbench).IsSuccess,
                Is.True);
            Assert.That(managedConflict.PrepareSerializedItemTransfer(
                Item, ProcessorSocket).IsSuccess, Is.True);
            Assert.That(managedConflict.PrepareSerializedItemTransfer(Item, MemorySlot).IsSuccess,
                Is.True);

            InventoryAuthority occupiedConflict = CreateQuadrupleAuthority(fillStorageSlot: true);
            long occupiedRevision = occupiedConflict.Revision;
            Assert.That(occupiedConflict.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedConflict.Revision, Is.EqualTo(occupiedRevision));
            Assert.That(occupiedConflict.PrepareSerializedItemTransfer(Item, Workbench).IsSuccess,
                Is.True);
            Assert.That(occupiedConflict.PrepareSerializedItemTransfer(
                Item, ProcessorSocket).IsSuccess, Is.True);
            Assert.That(occupiedConflict.PrepareSerializedItemTransfer(Item, MemorySlot).IsSuccess,
                Is.True);
            Assert.That(occupiedConflict.TryGetSerializedItem(
                StorageSlotItem, out InventoryItemRecord occupied), Is.True);
            Assert.That(occupied.ContainerId, Is.EqualTo(StorageSlot));
            Assert.That(occupiedConflict.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerQuadrupleUsesDeterministicValidationPrecedence()
        {
            InventoryAuthority unknownBeforeSame = CreateQuadrupleAuthority();
            Assert.That(unknownBeforeSame.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StableId<ContainerIdScope>.Parse("container.unknown")).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));

            InventoryAuthority sameBeforeManaged = CreateQuadrupleAuthority();
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainer(
                StorageSlot).IsSuccess, Is.True);
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot).Error,
                Is.EqualTo(InventoryFailures.SameContainer));

            InventoryAuthority managedBeforeOccupied =
                CreateQuadrupleAuthority(fillStorageSlot: true);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
        }

        [Test]
        public void ManagedContainerQuadrupleAtRevisionBoundaryIsAtomic()
        {
            InventoryAuthority overflow = CreateQuadrupleAuthority();
            SetRevision(overflow, long.MaxValue);

            Assert.That(overflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(overflow.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));

            InventoryAuthority finalRevision = CreateQuadrupleAuthority();
            SetRevision(finalRevision, long.MaxValue - 1);
            OperationResult<InventorySerializedTransferAccessQuadruple> claim =
                finalRevision.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot);
            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(finalRevision.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(finalRevision.PrepareSerializedItemTransfer(Item, StorageSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(finalRevision.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerQuintupleClaimSucceedsInOneRevisionAndEachAccessOnlyMovesItsOwnContainer()
        {
            InventoryAuthority authority = CreateQuintupleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessQuintuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(claim.Value.Fourth.ManagedContainerId, Is.EqualTo(StorageSlot));
            Assert.That(claim.Value.Fifth.ManagedContainerId, Is.EqualTo(BenchmarkSlot));

            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, Workbench, claim.Value.First, claim.Value.Second);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, ProcessorSocket, claim.Value.Second, claim.Value.Third);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, MemorySlot, claim.Value.Third, claim.Value.Fourth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, StorageSlot, claim.Value.Fourth, claim.Value.Fifth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, BenchmarkSlot, claim.Value.Fifth, claim.Value.First);

            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerQuintupleManagedOccupiedAndReservationFailuresLeaveNoPartialClaim()
        {
            InventoryAuthority managedConflict = CreateQuintupleAuthority();
            Assert.That(managedConflict.ClaimManagedSerializedTransferContainer(
                BenchmarkSlot).IsSuccess, Is.True);
            long managedRevision = managedConflict.Revision;

            Assert.That(managedConflict.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedConflict.Revision, Is.EqualTo(managedRevision));
            Assert.That(managedConflict.PrepareSerializedItemTransfer(Item, Workbench).IsSuccess,
                Is.True);
            Assert.That(managedConflict.PrepareSerializedItemTransfer(
                Item, ProcessorSocket).IsSuccess, Is.True);
            Assert.That(managedConflict.PrepareSerializedItemTransfer(Item, MemorySlot).IsSuccess,
                Is.True);
            Assert.That(managedConflict.PrepareSerializedItemTransfer(Item, StorageSlot).IsSuccess,
                Is.True);

            InventoryAuthority reservationConflict = CreateQuintupleAuthority(
                fillBenchmarkSlot: true);
            StableId<ReservationIdScope> reservationId =
                StableId<ReservationIdScope>.Parse("reservation.benchmark-slot");
            StableId<InventoryClaimIdScope> claimId =
                StableId<InventoryClaimIdScope>.Parse("claim.benchmark-slot");
            Assert.That(reservationConflict.ReserveSerializedItem(
                reservationId, claimId, BenchmarkSlotItem).IsSuccess, Is.True);
            long reservationRevision = reservationConflict.Revision;
            int reservationCount = reservationConflict.ReservationCount;

            Assert.That(reservationConflict.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(reservationConflict.Revision, Is.EqualTo(reservationRevision));
            Assert.That(reservationConflict.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(reservationConflict.PrepareSerializedItemTransfer(Item, Workbench).IsSuccess,
                Is.True);
            Assert.That(reservationConflict.PrepareSerializedItemTransfer(
                Item, ProcessorSocket).IsSuccess, Is.True);
            Assert.That(reservationConflict.PrepareSerializedItemTransfer(Item, MemorySlot).IsSuccess,
                Is.True);
            Assert.That(reservationConflict.PrepareSerializedItemTransfer(Item, StorageSlot).IsSuccess,
                Is.True);
            Assert.That(reservationConflict.TryGetSerializedItem(
                BenchmarkSlotItem, out InventoryItemRecord reserved), Is.True);
            Assert.That(reserved.ContainerId, Is.EqualTo(BenchmarkSlot));
            Assert.That(reservationConflict.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerQuintupleUsesDeterministicValidationPrecedence()
        {
            InventoryAuthority unknownBeforeSame = CreateQuintupleAuthority();
            Assert.That(unknownBeforeSame.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    StableId<ContainerIdScope>.Parse("container.unknown")).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));

            InventoryAuthority sameBeforeManaged = CreateQuintupleAuthority();
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainer(
                BenchmarkSlot).IsSuccess, Is.True);
            long sameRevision = sameBeforeManaged.Revision;
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.SameContainer));
            Assert.That(sameBeforeManaged.Revision, Is.EqualTo(sameRevision));

            InventoryAuthority managedBeforeOccupied = CreateQuintupleAuthority(
                fillBenchmarkSlot: true);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            SetRevision(managedBeforeOccupied, long.MaxValue);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedBeforeOccupied.Revision, Is.EqualTo(long.MaxValue));

            InventoryAuthority occupiedBeforeOverflow = CreateQuintupleAuthority(
                fillBenchmarkSlot: true);
            SetRevision(occupiedBeforeOverflow, long.MaxValue);
            Assert.That(occupiedBeforeOverflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedBeforeOverflow.Revision, Is.EqualTo(long.MaxValue));

            InventoryAuthority overflow = CreateQuintupleAuthority();
            SetRevision(overflow, long.MaxValue);
            Assert.That(overflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
        }

        [Test]
        public void ManagedContainerQuintupleAtRevisionBoundaryIsAtomic()
        {
            InventoryAuthority overflow = CreateQuintupleAuthority();
            SetRevision(overflow, long.MaxValue);

            Assert.That(overflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(overflow.PrepareSerializedItemTransfer(Item, BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));

            InventoryAuthority finalRevision = CreateQuintupleAuthority();
            SetRevision(finalRevision, long.MaxValue - 1);
            OperationResult<InventorySerializedTransferAccessQuintuple> claim =
                finalRevision.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(finalRevision.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(finalRevision.PrepareSerializedItemTransfer(Item, BenchmarkSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(finalRevision.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(finalRevision.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerSextupleClaimSucceedsInOneRevisionAndEachAccessOnlyMovesItsOwnContainer()
        {
            InventoryAuthority authority = CreateSextupleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessSextuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(claim.Value.Fourth.ManagedContainerId, Is.EqualTo(StorageSlot));
            Assert.That(claim.Value.Fifth.ManagedContainerId, Is.EqualTo(BenchmarkSlot));
            Assert.That(claim.Value.Sixth.ManagedContainerId, Is.EqualTo(GraphicsCardSlot));

            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, Workbench, claim.Value.First, claim.Value.Sixth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, ProcessorSocket, claim.Value.Second, claim.Value.First);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, MemorySlot, claim.Value.Third, claim.Value.Second);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, StorageSlot, claim.Value.Fourth, claim.Value.Third);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, BenchmarkSlot, claim.Value.Fifth, claim.Value.Fourth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, GraphicsCardSlot, claim.Value.Sixth, claim.Value.Fifth);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerSextupleValidationPrecedenceLeavesNoPartialClaim()
        {
            InventoryAuthority unknownBeforeSame = CreateSextupleAuthority();
            Assert.That(unknownBeforeSame.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    StableId<ContainerIdScope>.Parse("container.unknown")).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));

            InventoryAuthority sameBeforeManaged = CreateSextupleAuthority();
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainer(
                GraphicsCardSlot).IsSuccess, Is.True);
            long sameRevision = sameBeforeManaged.Revision;
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot).Error,
                Is.EqualTo(InventoryFailures.SameContainer));
            Assert.That(sameBeforeManaged.Revision, Is.EqualTo(sameRevision));

            InventoryAuthority managedBeforeOccupied = CreateSextupleAuthority(
                fillGraphicsCardSlot: true);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            SetRevision(managedBeforeOccupied, long.MaxValue);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedBeforeOccupied.Revision, Is.EqualTo(long.MaxValue));

            InventoryAuthority occupiedBeforeOverflow = CreateSextupleAuthority(
                fillGraphicsCardSlot: true);
            SetRevision(occupiedBeforeOverflow, long.MaxValue);
            Assert.That(occupiedBeforeOverflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedBeforeOverflow.Revision, Is.EqualTo(long.MaxValue));
            SetRevision(occupiedBeforeOverflow, long.MaxValue - 1L);
            Assert.That(occupiedBeforeOverflow.ClaimManagedSerializedTransferContainer(
                Workbench).IsSuccess, Is.True);

            InventoryAuthority overflow = CreateSextupleAuthority();
            SetRevision(overflow, long.MaxValue);
            Assert.That(overflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(overflow.PrepareSerializedItemTransfer(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
        }

        [Test]
        public void ManagedContainerSextupleAtRevisionBoundaryIsAtomic()
        {
            InventoryAuthority authority = CreateSextupleAuthority();
            SetRevision(authority, long.MaxValue - 1);

            OperationResult<InventorySerializedTransferAccessSextuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, GraphicsCardSlot).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerSeptupleClaimSucceedsInOneRevisionAndEachAccessOnlyMovesItsOwnContainer()
        {
            InventoryAuthority authority = CreateSeptupleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessSeptuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(claim.Value.Fourth.ManagedContainerId, Is.EqualTo(StorageSlot));
            Assert.That(claim.Value.Fifth.ManagedContainerId, Is.EqualTo(BenchmarkSlot));
            Assert.That(claim.Value.Sixth.ManagedContainerId, Is.EqualTo(GraphicsCardSlot));
            Assert.That(claim.Value.Seventh.ManagedContainerId, Is.EqualTo(PowerSupplyBay));

            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, Workbench, claim.Value.First, claim.Value.Seventh);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, ProcessorSocket, claim.Value.Second, claim.Value.First);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, MemorySlot, claim.Value.Third, claim.Value.Second);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, StorageSlot, claim.Value.Fourth, claim.Value.Third);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, BenchmarkSlot, claim.Value.Fifth, claim.Value.Fourth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, GraphicsCardSlot, claim.Value.Sixth, claim.Value.Fifth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, PowerSupplyBay, claim.Value.Seventh, claim.Value.Sixth);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerSeptupleValidationPrecedenceLeavesNoPartialClaim()
        {
            InventoryAuthority unknownBeforeSame = CreateSeptupleAuthority();
            Assert.That(unknownBeforeSame.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    StableId<ContainerIdScope>.Parse("container.unknown")).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));

            InventoryAuthority sameBeforeManaged = CreateSeptupleAuthority();
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainer(
                PowerSupplyBay).IsSuccess, Is.True);
            long sameRevision = sameBeforeManaged.Revision;
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay).Error,
                Is.EqualTo(InventoryFailures.SameContainer));
            Assert.That(sameBeforeManaged.Revision, Is.EqualTo(sameRevision));

            InventoryAuthority managedBeforeOccupied = CreateSeptupleAuthority(
                fillPowerSupplyBay: true);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            SetRevision(managedBeforeOccupied, long.MaxValue);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedBeforeOccupied.Revision, Is.EqualTo(long.MaxValue));

            InventoryAuthority occupiedBeforeOverflow = CreateSeptupleAuthority(
                fillPowerSupplyBay: true);
            SetRevision(occupiedBeforeOverflow, long.MaxValue);
            Assert.That(occupiedBeforeOverflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedBeforeOverflow.Revision, Is.EqualTo(long.MaxValue));

            InventoryAuthority overflow = CreateSeptupleAuthority();
            SetRevision(overflow, long.MaxValue);
            Assert.That(overflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(overflow.PrepareSerializedItemTransfer(Item, Workbench).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
        }

        [Test]
        public void ManagedContainerSeptupleAtRevisionBoundaryIsAtomic()
        {
            InventoryAuthority authority = CreateSeptupleAuthority();
            SetRevision(authority, long.MaxValue - 1);

            OperationResult<InventorySerializedTransferAccessSeptuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(authority.PrepareSerializedItemTransfer(Item, PowerSupplyBay).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(Item, out InventoryItemRecord unchanged),
                Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerOctupleClaimSucceedsInOneRevisionAndEachAccessOnlyMovesItsOwnContainer()
        {
            InventoryAuthority authority = CreateOctupleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessOctuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(claim.Value.Fourth.ManagedContainerId, Is.EqualTo(StorageSlot));
            Assert.That(claim.Value.Fifth.ManagedContainerId, Is.EqualTo(BenchmarkSlot));
            Assert.That(claim.Value.Sixth.ManagedContainerId, Is.EqualTo(GraphicsCardSlot));
            Assert.That(claim.Value.Seventh.ManagedContainerId, Is.EqualTo(PowerSupplyBay));
            Assert.That(claim.Value.Eighth.ManagedContainerId, Is.EqualTo(PowerCableRoute));

            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, Workbench, claim.Value.First, claim.Value.Eighth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, ProcessorSocket, claim.Value.Second, claim.Value.First);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, MemorySlot, claim.Value.Third, claim.Value.Second);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, StorageSlot, claim.Value.Fourth, claim.Value.Third);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, BenchmarkSlot, claim.Value.Fifth, claim.Value.Fourth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, GraphicsCardSlot, claim.Value.Sixth, claim.Value.Fifth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, PowerSupplyBay, claim.Value.Seventh, claim.Value.Sixth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, PowerCableRoute, claim.Value.Eighth, claim.Value.Seventh);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerOctupleValidationPrecedenceLeavesNoPartialClaim()
        {
            StableId<ContainerIdScope> unknown =
                StableId<ContainerIdScope>.Parse("container.unknown");
            InventoryAuthority unknownBeforeSame = CreateOctupleAuthority();
            long unknownRevision = unknownBeforeSame.Revision;
            Assert.That(unknownBeforeSame.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    unknown).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));
            AssertOctupleClaimFailureLeftPublicTransferAvailable(
                unknownBeforeSame,
                unknownRevision);

            InventoryAuthority sameBeforeManaged = CreateOctupleAuthority();
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainer(
                PowerCableRoute).IsSuccess, Is.True);
            long sameRevision = sameBeforeManaged.Revision;
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SameContainer));
            Assert.That(sameBeforeManaged.Revision, Is.EqualTo(sameRevision));
            Assert.That(sameBeforeManaged.PrepareSerializedItemTransfer(
                Item, Workbench).IsSuccess, Is.True);

            InventoryAuthority managedBeforeOccupied = CreateOctupleAuthority(
                fillPowerCableRoute: true);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            SetRevision(managedBeforeOccupied, long.MaxValue);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedBeforeOccupied.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(managedBeforeOccupied.PrepareSerializedItemTransfer(
                Item, Workbench).Error, Is.EqualTo(InventoryFailures.RevisionOverflow));

            InventoryAuthority occupiedBeforeOverflow = CreateOctupleAuthority(
                fillPowerCableRoute: true);
            SetRevision(occupiedBeforeOverflow, long.MaxValue);
            Assert.That(occupiedBeforeOverflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedBeforeOverflow.Revision, Is.EqualTo(long.MaxValue));

            InventoryAuthority overflow = CreateOctupleAuthority();
            SetRevision(overflow, long.MaxValue);
            Assert.That(overflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(overflow.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(overflow.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerOctupleAtRevisionBoundaryIsAtomic()
        {
            InventoryAuthority authority = CreateOctupleAuthority();
            SetRevision(authority, long.MaxValue - 1);

            OperationResult<InventorySerializedTransferAccessOctuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, PowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerNonupleClaimSucceedsInOneRevisionAndEachAccessOnlyMovesItsOwnContainer()
        {
            InventoryAuthority authority = CreateNonupleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessNonuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(claim.Value.Fourth.ManagedContainerId, Is.EqualTo(StorageSlot));
            Assert.That(claim.Value.Fifth.ManagedContainerId, Is.EqualTo(BenchmarkSlot));
            Assert.That(claim.Value.Sixth.ManagedContainerId, Is.EqualTo(GraphicsCardSlot));
            Assert.That(claim.Value.Seventh.ManagedContainerId, Is.EqualTo(PowerSupplyBay));
            Assert.That(claim.Value.Eighth.ManagedContainerId, Is.EqualTo(PowerCableRoute));
            Assert.That(claim.Value.Ninth.ManagedContainerId, Is.EqualTo(CpuPowerCableRoute));

            StableId<ContainerIdScope>[] managedContainerIds =
            {
                Workbench,
                ProcessorSocket,
                MemorySlot,
                StorageSlot,
                BenchmarkSlot,
                GraphicsCardSlot,
                PowerSupplyBay,
                PowerCableRoute,
                CpuPowerCableRoute
            };
            InventorySerializedTransferAccess[] accesses =
            {
                claim.Value.First,
                claim.Value.Second,
                claim.Value.Third,
                claim.Value.Fourth,
                claim.Value.Fifth,
                claim.Value.Sixth,
                claim.Value.Seventh,
                claim.Value.Eighth,
                claim.Value.Ninth
            };
            for (int targetIndex = 0; targetIndex < managedContainerIds.Length; targetIndex++)
            {
                for (int accessIndex = 0; accessIndex < accesses.Length; accessIndex++)
                {
                    OperationResult<InventorySerializedTransferPlan> prepared =
                        authority.PrepareSerializedItemTransfer(
                            Item,
                            managedContainerIds[targetIndex],
                            accesses[accessIndex]);
                    if (targetIndex == accessIndex)
                    {
                        Assert.That(prepared.IsSuccess, Is.True);
                    }
                    else
                    {
                        Assert.That(prepared.Error,
                            Is.EqualTo(InventoryFailures.SerializedTransferAccessInvalid));
                    }
                }
            }

            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, Workbench, claim.Value.First, claim.Value.Ninth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, ProcessorSocket, claim.Value.Second, claim.Value.First);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, MemorySlot, claim.Value.Third, claim.Value.Second);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, StorageSlot, claim.Value.Fourth, claim.Value.Third);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, BenchmarkSlot, claim.Value.Fifth, claim.Value.Fourth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, GraphicsCardSlot, claim.Value.Sixth, claim.Value.Fifth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, PowerSupplyBay, claim.Value.Seventh, claim.Value.Sixth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, PowerCableRoute, claim.Value.Eighth, claim.Value.Seventh);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, CpuPowerCableRoute, claim.Value.Ninth, claim.Value.Eighth);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerNonupleValidationPrecedenceLeavesNoPartialClaim()
        {
            StableId<ContainerIdScope> unknown =
                StableId<ContainerIdScope>.Parse("container.unknown");
            InventoryAuthority unknownBeforeSame = CreateNonupleAuthority();
            long unknownRevision = unknownBeforeSame.Revision;
            Assert.That(unknownBeforeSame.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    unknown).Error,
                Is.EqualTo(InventoryFailures.UnknownContainer));
            AssertNonupleClaimFailureLeftPublicTransferAvailable(
                unknownBeforeSame,
                unknownRevision);

            InventoryAuthority sameBeforeManaged = CreateNonupleAuthority();
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainer(
                CpuPowerCableRoute).IsSuccess, Is.True);
            long sameRevision = sameBeforeManaged.Revision;
            Assert.That(sameBeforeManaged.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    Workbench,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SameContainer));
            Assert.That(sameBeforeManaged.Revision, Is.EqualTo(sameRevision));
            Assert.That(sameBeforeManaged.PrepareSerializedItemTransfer(
                    Item, CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            AssertPublicTransferAvailable(
                sameBeforeManaged,
                Workbench,
                MemorySlot,
                StorageSlot,
                BenchmarkSlot,
                GraphicsCardSlot,
                PowerSupplyBay,
                PowerCableRoute);
            Assert.That(sameBeforeManaged.TryGetSerializedItem(
                Item, out InventoryItemRecord sameUnchanged), Is.True);
            Assert.That(sameUnchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(sameBeforeManaged.ValidateInvariants().IsSuccess, Is.True);

            InventoryAuthority managedBeforeOccupied = CreateNonupleAuthority(
                fillCpuPowerCableRoute: true);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainer(
                ProcessorSocket).IsSuccess, Is.True);
            long managedRevision = managedBeforeOccupied.Revision;
            SetRevision(managedBeforeOccupied, long.MaxValue);
            Assert.That(managedBeforeOccupied.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedBeforeOccupied.Revision, Is.EqualTo(long.MaxValue));
            SetRevision(managedBeforeOccupied, managedRevision);
            AssertPublicTransferAvailable(
                managedBeforeOccupied,
                Workbench,
                MemorySlot,
                StorageSlot,
                BenchmarkSlot,
                GraphicsCardSlot,
                PowerSupplyBay,
                PowerCableRoute);
            Assert.That(managedBeforeOccupied.PrepareSerializedItemTransfer(
                    Item, ProcessorSocket).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(managedBeforeOccupied.PrepareSerializedItemTransfer(
                    Item, CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(managedBeforeOccupied.TryGetSerializedItem(
                Item, out InventoryItemRecord managedUnchanged), Is.True);
            Assert.That(managedUnchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(managedBeforeOccupied.TryGetSerializedItem(
                CpuPowerCableRouteItem, out InventoryItemRecord managedOccupied), Is.True);
            Assert.That(managedOccupied.ContainerId, Is.EqualTo(CpuPowerCableRoute));
            Assert.That(managedBeforeOccupied.ValidateInvariants().IsSuccess, Is.True);

            InventoryAuthority occupiedOnly = CreateNonupleAuthority(
                fillCpuPowerCableRoute: true);
            long occupiedOnlyRevision = occupiedOnly.Revision;
            Assert.That(occupiedOnly.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedOnly.Revision, Is.EqualTo(occupiedOnlyRevision));
            AssertPublicTransferAvailable(
                occupiedOnly,
                Workbench,
                ProcessorSocket,
                MemorySlot,
                StorageSlot,
                BenchmarkSlot,
                GraphicsCardSlot,
                PowerSupplyBay,
                PowerCableRoute);
            Assert.That(occupiedOnly.PrepareSerializedItemTransfer(
                    Item, CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(occupiedOnly.TryGetSerializedItem(
                CpuPowerCableRouteItem, out InventoryItemRecord occupiedOnlyItem), Is.True);
            Assert.That(occupiedOnlyItem.ContainerId, Is.EqualTo(CpuPowerCableRoute));
            Assert.That(occupiedOnly.ValidateInvariants().IsSuccess, Is.True);

            InventoryAuthority occupiedBeforeOverflow = CreateNonupleAuthority(
                fillCpuPowerCableRoute: true);
            StableId<ReservationIdScope> reservationId =
                StableId<ReservationIdScope>.Parse("reservation.cpu-power-cable-route");
            StableId<InventoryClaimIdScope> claimId =
                StableId<InventoryClaimIdScope>.Parse("claim.cpu-power-cable-route");
            Assert.That(occupiedBeforeOverflow.ReserveSerializedItem(
                reservationId, claimId, CpuPowerCableRouteItem).IsSuccess, Is.True);
            long occupiedRevision = occupiedBeforeOverflow.Revision;
            int reservationCount = occupiedBeforeOverflow.ReservationCount;
            SetRevision(occupiedBeforeOverflow, long.MaxValue);
            Assert.That(occupiedBeforeOverflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupiedBeforeOverflow.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(occupiedBeforeOverflow.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(occupiedBeforeOverflow.TryGetSerializedItem(
                CpuPowerCableRouteItem, out InventoryItemRecord reserved), Is.True);
            Assert.That(reserved.ContainerId, Is.EqualTo(CpuPowerCableRoute));
            SetRevision(occupiedBeforeOverflow, occupiedRevision);
            AssertPublicTransferAvailable(
                occupiedBeforeOverflow,
                Workbench,
                ProcessorSocket,
                MemorySlot,
                StorageSlot,
                BenchmarkSlot,
                GraphicsCardSlot,
                PowerSupplyBay,
                PowerCableRoute);
            Assert.That(occupiedBeforeOverflow.PrepareSerializedItemTransfer(
                    Item, CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(occupiedBeforeOverflow.TryGetSerializedItem(
                Item, out InventoryItemRecord reservedUnchanged), Is.True);
            Assert.That(reservedUnchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(occupiedBeforeOverflow.ValidateInvariants().IsSuccess, Is.True);

            InventoryAuthority overflow = CreateNonupleAuthority();
            long overflowRevision = overflow.Revision;
            SetRevision(overflow, long.MaxValue);
            Assert.That(overflow.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(overflow.Revision, Is.EqualTo(long.MaxValue));
            SetRevision(overflow, overflowRevision);
            AssertNonupleClaimFailureLeftPublicTransferAvailable(
                overflow,
                overflowRevision);
        }

        [Test]
        public void ManagedContainerNonupleAtRevisionBoundaryIsAtomic()
        {
            InventoryAuthority authority = CreateNonupleAuthority();
            SetRevision(authority, long.MaxValue - 1);

            OperationResult<InventorySerializedTransferAccessNonuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, CpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerDecupleClaimSucceedsInOneRevisionAndKeepsExactAccessIdentity()
        {
            InventoryAuthority authority = CreateDecupleAuthority();
            long revision = authority.Revision;

            OperationResult<InventorySerializedTransferAccessDecuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute,
                    GpuPowerCableRoute);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(revision + 1));
            Assert.That(claim.Value.First.ManagedContainerId, Is.EqualTo(Workbench));
            Assert.That(claim.Value.Second.ManagedContainerId, Is.EqualTo(ProcessorSocket));
            Assert.That(claim.Value.Third.ManagedContainerId, Is.EqualTo(MemorySlot));
            Assert.That(claim.Value.Fourth.ManagedContainerId, Is.EqualTo(StorageSlot));
            Assert.That(claim.Value.Fifth.ManagedContainerId, Is.EqualTo(BenchmarkSlot));
            Assert.That(claim.Value.Sixth.ManagedContainerId, Is.EqualTo(GraphicsCardSlot));
            Assert.That(claim.Value.Seventh.ManagedContainerId, Is.EqualTo(PowerSupplyBay));
            Assert.That(claim.Value.Eighth.ManagedContainerId, Is.EqualTo(PowerCableRoute));
            Assert.That(claim.Value.Ninth.ManagedContainerId, Is.EqualTo(CpuPowerCableRoute));
            Assert.That(claim.Value.Tenth.ManagedContainerId, Is.EqualTo(GpuPowerCableRoute));

            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, Workbench, claim.Value.First, claim.Value.Tenth);
            AssertManagedAccessMovesOnlyOwnContainer(
                authority, Item, GpuPowerCableRoute, claim.Value.Tenth, claim.Value.First);
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerDecupleValidationIsAllOrNoneAndOccupiedPrecedesOverflow()
        {
            InventoryAuthority occupied = CreateDecupleAuthority(
                fillGpuPowerCableRoute: true);
            long revision = occupied.Revision;
            SetRevision(occupied, long.MaxValue);

            Assert.That(occupied.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute,
                    GpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerOccupied));
            Assert.That(occupied.Revision, Is.EqualTo(long.MaxValue));

            SetRevision(occupied, revision);
            Assert.That(occupied.PrepareSerializedItemTransfer(
                    Item, Workbench).IsSuccess,
                Is.True);
            Assert.That(occupied.PrepareSerializedItemTransfer(
                    Item, CpuPowerCableRoute).IsSuccess,
                Is.True);
            Assert.That(occupied.PrepareSerializedItemTransfer(
                    Item, GpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(occupied.TryGetSerializedItem(
                GpuPowerCableRouteItem, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(GpuPowerCableRoute));
            Assert.That(occupied.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedContainerDecupleAtRevisionBoundaryIsAtomic()
        {
            InventoryAuthority authority = CreateDecupleAuthority();
            SetRevision(authority, long.MaxValue - 1);

            OperationResult<InventorySerializedTransferAccessDecuple> claim =
                authority.ClaimManagedSerializedTransferContainers(
                    Workbench,
                    ProcessorSocket,
                    MemorySlot,
                    StorageSlot,
                    BenchmarkSlot,
                    GraphicsCardSlot,
                    PowerSupplyBay,
                    PowerCableRoute,
                    CpuPowerCableRoute,
                    GpuPowerCableRoute);

            Assert.That(claim.IsSuccess, Is.True);
            Assert.That(authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, GpuPowerCableRoute).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertPublicTransferAvailable(
            InventoryAuthority authority,
            params StableId<ContainerIdScope>[] containerIds)
        {
            for (int index = 0; index < containerIds.Length; index++)
            {
                Assert.That(authority.PrepareSerializedItemTransfer(
                    Item, containerIds[index]).IsSuccess, Is.True);
            }
        }

        private static void AssertNonupleClaimFailureLeftPublicTransferAvailable(
            InventoryAuthority authority,
            long expectedRevision)
        {
            Assert.That(authority.Revision, Is.EqualTo(expectedRevision));
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, Workbench).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, ProcessorSocket).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, MemorySlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, StorageSlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, BenchmarkSlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, GraphicsCardSlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, PowerSupplyBay).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, PowerCableRoute).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, CpuPowerCableRoute).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertOctupleClaimFailureLeftPublicTransferAvailable(
            InventoryAuthority authority,
            long expectedRevision)
        {
            Assert.That(authority.Revision, Is.EqualTo(expectedRevision));
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, Workbench).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, ProcessorSocket).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, MemorySlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, StorageSlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, BenchmarkSlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, GraphicsCardSlot).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, PowerSupplyBay).IsSuccess, Is.True);
            Assert.That(authority.PrepareSerializedItemTransfer(
                Item, PowerCableRoute).IsSuccess, Is.True);
            Assert.That(authority.TryGetSerializedItem(
                Item, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(Hands));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertManagedAccessMovesOnlyOwnContainer(
            InventoryAuthority authority,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> managedContainerId,
            InventorySerializedTransferAccess ownAccess,
            InventorySerializedTransferAccess foreignAccess)
        {
            Assert.That(authority.PrepareSerializedItemTransfer(
                    itemId, managedContainerId, foreignAccess).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferAccessInvalid));

            OperationResult<InventorySerializedTransferPlan> outbound =
                authority.PrepareSerializedItemTransfer(itemId, managedContainerId, ownAccess);
            Assert.That(outbound.IsSuccess, Is.True);
            Assert.That(authority.CommitPreparedSerializedItemTransfer(outbound.Value).IsSuccess,
                Is.True);
            Assert.That(authority.TryGetSerializedItem(itemId, out InventoryItemRecord seated), Is.True);
            Assert.That(seated.ContainerId, Is.EqualTo(managedContainerId));

            OperationResult<InventorySerializedTransferPlan> returnPlan =
                authority.PrepareSerializedItemTransfer(itemId, Hands, ownAccess);
            Assert.That(returnPlan.IsSuccess, Is.True);
            Assert.That(authority.CommitPreparedSerializedItemTransfer(returnPlan.Value).IsSuccess,
                Is.True);
            Assert.That(authority.TryGetSerializedItem(itemId, out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(Hands));
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

        private static InventoryAuthority CreateQuadrupleAuthority(bool fillStorageSlot = false)
        {
            InventoryAuthority authority = CreateTripleAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                StorageSlot,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillStorageSlot)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    StorageSlotItem,
                    ProductId,
                    StorageSlot,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }

        private static InventoryAuthority CreateQuintupleAuthority(bool fillBenchmarkSlot = false)
        {
            InventoryAuthority authority = CreateQuadrupleAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                BenchmarkSlot,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillBenchmarkSlot)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    BenchmarkSlotItem,
                    ProductId,
                    BenchmarkSlot,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }

        private static InventoryAuthority CreateSextupleAuthority(
            bool fillGraphicsCardSlot = false)
        {
            InventoryAuthority authority = CreateQuintupleAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                GraphicsCardSlot,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillGraphicsCardSlot)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    GraphicsCardSlotItem,
                    ProductId,
                    GraphicsCardSlot,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }

        private static InventoryAuthority CreateSeptupleAuthority(
            bool fillPowerSupplyBay = false)
        {
            InventoryAuthority authority = CreateSextupleAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                PowerSupplyBay,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillPowerSupplyBay)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    PowerSupplyBayItem,
                    ProductId,
                    PowerSupplyBay,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }

        private static InventoryAuthority CreateOctupleAuthority(
            bool fillPowerCableRoute = false)
        {
            InventoryAuthority authority = CreateSeptupleAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                PowerCableRoute,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillPowerCableRoute)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    PowerCableRouteItem,
                    ProductId,
                    PowerCableRoute,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }

        private static InventoryAuthority CreateNonupleAuthority(
            bool fillCpuPowerCableRoute = false)
        {
            InventoryAuthority authority = CreateOctupleAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                CpuPowerCableRoute,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillCpuPowerCableRoute)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    CpuPowerCableRouteItem,
                    ProductId,
                    CpuPowerCableRoute,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }

        private static InventoryAuthority CreateDecupleAuthority(
            bool fillGpuPowerCableRoute = false)
        {
            InventoryAuthority authority = CreateNonupleAuthority();
            Assert.That(authority.RegisterContainer(InventoryContainerDefinition.Create(
                GpuPowerCableRoute,
                InventoryContainerKind.Workbench,
                1).Value).IsSuccess, Is.True);

            if (fillGpuPowerCableRoute)
            {
                Assert.That(authority.ReceiveSerializedItem(
                    GpuPowerCableRouteItem,
                    ProductId,
                    GpuPowerCableRoute,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess, Is.True);
            }

            return authority;
        }
    }
}
