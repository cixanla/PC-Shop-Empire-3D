using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class ProcessorCoolerAssemblyBuildAuthorityTests
    {
        [Test]
        public void FactoryClaimsFiveContainersAtomicallyAndExposesStableTopology()
        {
            CoolerFixture fixture = CoolerFixture.CreateUnclaimed();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> created = fixture.TryCreateAuthority();

            Assert.That(created.IsSuccess, Is.True);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            AssemblyBuildAuthority authority = created.Value;
            AssemblyBuildSnapshot snapshot = authority.GetSnapshot();
            Assert.That(snapshot.HasProcessorCoolerSlot, Is.True);
            Assert.That(snapshot.ProcessorCoolerSlotId, Is.EqualTo(fixture.CoolerSlotId));
            Assert.That(snapshot.ProcessorCoolerBracketId, Is.EqualTo(fixture.CoolerBracketId));
            Assert.That(snapshot.ProcessorCoolerSlotContainerId,
                Is.EqualTo(fixture.CoolerSlotContainerId));
            Assert.That(snapshot.SupportedProcessorCoolerType,
                Is.EqualTo(ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim));
            Assert.That(snapshot.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));
            Assert.That(snapshot.ProcessorCoolerRetentionTopology.CrossRetentionOrder,
                Is.EqualTo(new[]
                {
                    fixture.CoolerPoint1Id,
                    fixture.CoolerPoint3Id,
                    fixture.CoolerPoint2Id,
                    fixture.CoolerPoint4Id
                }));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.CoolerItemId,
                    fixture.CoolerSlotContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FullCycleIsReplaySafePreservesCustodyAndConsumesTimOnce()
        {
            CoolerFixture fixture = CoolerFixture.Create();
            fixture.PrepareProcessorHost();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.cooler-seat");
            StableId<AssemblyOperationIdScope> retainId =
                OperationId("operation.cooler-retain");
            StableId<AssemblyOperationIdScope> unretainId =
                OperationId("operation.cooler-unretain");
            StableId<AssemblyOperationIdScope> removeId =
                OperationId("operation.cooler-remove");
            long inventoryBeforeSeat = fixture.Inventory.Revision;
            long seatRevision = fixture.Authority.Revision;

            AssemblyOperationReceipt seat = fixture.Authority.SeatProcessorCooler(
                seatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Rotated180,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                seatRevision).Value;
            Assert.That(fixture.Authority.SeatProcessorCooler(
                seatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Rotated180,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                seatRevision).Value, Is.SameAs(seat));
            Assert.That(seat.OperationKind,
                Is.EqualTo(AssemblyOperationKind.SeatProcessorCooler));
            Assert.That(seat.PreviousProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.PreAppliedUnused));
            Assert.That(seat.ResultingProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.AppliedConsumed));
            Assert.That(seat.ProcessorCoolerMountOrientation,
                Is.EqualTo(ProcessorCoolerMountOrientation.Rotated180));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 1));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);

            long inventoryBeforeRetention = fixture.Inventory.Revision;
            AssemblyOperationReceipt retained = fixture.Authority.RetainProcessorCooler(
                retainId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                seatId,
                fixture.Authority.Revision).Value;
            Assert.That(retained.ResultingProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeRetention));
            Assert.That(fixture.Authority.RemoveProcessorCooler(
                    OperationId("operation.cooler-retained-remove"),
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    seatId,
                    fixture.Authority.Revision).Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerRetained));

            AssemblyOperationReceipt unretained =
                fixture.Authority.UnretainProcessorCooler(
                    unretainId,
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    fixture.CoolerBracketId,
                    seatId,
                    retainId,
                    fixture.Authority.Revision).Value;
            Assert.That(unretained.ResultingProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerSeatedUnsecured));
            AssemblyOperationReceipt removed = fixture.Authority.RemoveProcessorCooler(
                removeId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                seatId,
                fixture.Authority.Revision).Value;
            Assert.That(removed.ResultingProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));
            Assert.That(fixture.Authority.RemoveProcessorCooler(
                removeId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                seatId,
                7).Value, Is.SameAs(removed));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.CoolerItemId, out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(
                returned.StateFlags &
                InventorySerializedItemStateFlags.PreAppliedConsumableConsumed,
                Is.EqualTo(
                    InventorySerializedItemStateFlags.PreAppliedConsumableConsumed));
            Assert.That(fixture.Authority.SeatProcessorCooler(
                    OperationId("operation.cooler-reseat-consumed-tim"),
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    ProcessorCoolerMountOrientation.Primary,
                    fixture.AttachId,
                    fixture.SecureId,
                    fixture.ProcessorSeatId,
                    fixture.ProcessorRetainId,
                    fixture.Authority.Revision).Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerTimConsumed));
            Assert.That(fixture.Authority.SeatProcessorCooler(
                    seatId,
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    ProcessorCoolerMountOrientation.Primary,
                    fixture.AttachId,
                    fixture.SecureId,
                    fixture.ProcessorSeatId,
                    fixture.ProcessorRetainId,
                    seatRevision).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(8));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(8));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SeatingFailsClosedAcrossMotherboardAndProcessorHostGates()
        {
            CoolerFixture fixture = CoolerFixture.Create();

            AssertSeatFailure(fixture, "missing-board", AssemblyFailures.MotherboardMissing);
            Assert.That(fixture.Authority.AttachMotherboard(
                fixture.AttachId,
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);
            AssertSeatFailure(
                fixture,
                "unsecured-board",
                AssemblyFailures.MotherboardUnsecured,
                fixture.AttachId);
            Assert.That(fixture.Authority.SecureMotherboardFastener(
                fixture.SecureId,
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId,
                fixture.MotherboardFastenerId,
                fixture.AttachId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            AssertSeatFailure(
                fixture,
                "missing-cpu",
                AssemblyFailures.ProcessorMissing,
                fixture.AttachId,
                fixture.SecureId);
            Assert.That(fixture.Authority.SeatProcessor(
                fixture.ProcessorSeatId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.AttachId,
                fixture.SecureId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            AssertSeatFailure(
                fixture,
                "unretained-cpu",
                AssemblyFailures.ProcessorUnretained,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId);
            Assert.That(fixture.Authority.Revision, Is.EqualTo(3));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(3));
            Assert.That(fixture.Authority.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BenchmarkRequiresRetainedCoolerAfterCompleteCanonicalHost()
        {
            CoolerFixture fixture = CoolerFixture.Create();
            fixture.PrepareProcessorHost();
            fixture.SeatAndRetainMemory();
            fixture.SeatAndSecureStorage();

            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerMissing));
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.cooler-benchmark-seat");
            Assert.That(fixture.Authority.SeatProcessorCooler(
                seatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerUnretained));
            Assert.That(fixture.Authority.RetainProcessorCooler(
                OperationId("operation.cooler-benchmark-retain"),
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                seatId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().IsSuccess, Is.True);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void NonCoolerReceiptsCarryCurrentCoolerStateAndTimSnapshot()
        {
            CoolerFixture fixture = CoolerFixture.Create();
            fixture.PrepareProcessorHost();
            StableId<AssemblyOperationIdScope> coolerSeatId =
                OperationId("operation.cooler-snapshot-seat");
            Assert.That(fixture.Authority.SeatProcessorCooler(
                coolerSeatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.RetainProcessorCooler(
                OperationId("operation.cooler-snapshot-retain"),
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                coolerSeatId,
                fixture.Authority.Revision).IsSuccess, Is.True);

            AssemblyOperationReceipt memoryReceipt = fixture.Authority.SeatMemoryModule(
                OperationId("operation.cooler-snapshot-memory-seat"),
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                fixture.AttachId,
                fixture.SecureId,
                fixture.Authority.Revision).Value;

            Assert.That(memoryReceipt.PreviousProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(memoryReceipt.ResultingProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.CoolerRetained));
            Assert.That(memoryReceipt.PreviousProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.AppliedConsumed));
            Assert.That(memoryReceipt.ResultingProcessorCoolerTimState,
                Is.EqualTo(ProcessorCoolerTimState.AppliedConsumed));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void RetainAndUnretainExactReplayAndConflictsNeverMutate()
        {
            CoolerFixture fixture = CoolerFixture.Create();
            fixture.PrepareProcessorHost();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.cooler-replay-seat");
            StableId<AssemblyOperationIdScope> retainId =
                OperationId("operation.cooler-replay-retain");
            StableId<AssemblyOperationIdScope> unretainId =
                OperationId("operation.cooler-replay-unretain");
            AssemblyOperationReceipt seat = fixture.Authority.SeatProcessorCooler(
                seatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                fixture.Authority.Revision).Value;
            long retainRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt retained = fixture.Authority.RetainProcessorCooler(
                retainId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                seat.OperationId,
                retainRevision).Value;

            Assert.That(fixture.Authority.RetainProcessorCooler(
                retainId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                seat.OperationId,
                retainRevision).Value, Is.SameAs(retained));
            Assert.That(fixture.Authority.RetainProcessorCooler(
                    retainId,
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    StableId<AssemblyProcessorCoolerBracketIdScope>.Parse(
                        "bracket.cooler-foreign"),
                    seat.OperationId,
                    retainRevision).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));

            long unretainRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt unretained =
                fixture.Authority.UnretainProcessorCooler(
                    unretainId,
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    fixture.CoolerBracketId,
                    seat.OperationId,
                    retained.OperationId,
                    unretainRevision).Value;
            long finalAssemblyRevision = fixture.Authority.Revision;
            long finalInventoryRevision = fixture.Inventory.Revision;
            int finalReceiptCount = fixture.Authority.ReceiptCount;

            Assert.That(fixture.Authority.UnretainProcessorCooler(
                unretainId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                seat.OperationId,
                retained.OperationId,
                unretainRevision).Value, Is.SameAs(unretained));
            Assert.That(fixture.Authority.RemoveProcessorCooler(
                    retainId,
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    seat.OperationId,
                    finalAssemblyRevision).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.Revision,
                Is.EqualTo(finalAssemblyRevision));
            Assert.That(fixture.Inventory.Revision,
                Is.EqualTo(finalInventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount,
                Is.EqualTo(finalReceiptCount));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InstalledCoolerBlocksProcessorHostOpeningUntilRemoved()
        {
            CoolerFixture fixture = CoolerFixture.Create();
            fixture.PrepareProcessorHost();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.cooler-host-gate-seat");
            StableId<AssemblyOperationIdScope> retainId =
                OperationId("operation.cooler-host-gate-retain");
            Assert.That(fixture.Authority.SeatProcessorCooler(
                seatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.RetainProcessorCooler(
                retainId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                seatId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            long blockedAssemblyRevision = fixture.Authority.Revision;
            long blockedInventoryRevision = fixture.Inventory.Revision;
            int blockedReceiptCount = fixture.Authority.ReceiptCount;

            Assert.That(fixture.Authority.OpenProcessorRetention(
                    OperationId("operation.cooler-host-gate-cpu-open-blocked"),
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    fixture.ProcessorRetentionId,
                    fixture.ProcessorSeatId,
                    fixture.ProcessorRetainId,
                    blockedAssemblyRevision).Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerInstalled));
            Assert.That(fixture.Authority.Revision,
                Is.EqualTo(blockedAssemblyRevision));
            Assert.That(fixture.Inventory.Revision,
                Is.EqualTo(blockedInventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount,
                Is.EqualTo(blockedReceiptCount));

            Assert.That(fixture.Authority.UnretainProcessorCooler(
                OperationId("operation.cooler-host-gate-unretain"),
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                fixture.CoolerBracketId,
                seatId,
                retainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.RemoveProcessorCooler(
                OperationId("operation.cooler-host-gate-remove"),
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                seatId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.OpenProcessorRetention(
                OperationId("operation.cooler-host-gate-cpu-open"),
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.ProcessorRetentionId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReceiptHistoryRejectsSecondTimConsumptionForSameCoolerIdentity()
        {
            CoolerFixture fixture = CoolerFixture.Create();
            fixture.PrepareProcessorHost();
            StableId<AssemblyOperationIdScope> firstSeatId =
                OperationId("operation.cooler-history-first-seat");
            Assert.That(fixture.Authority.SeatProcessorCooler(
                firstSeatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.RemoveProcessorCooler(
                OperationId("operation.cooler-history-first-remove"),
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                firstSeatId,
                fixture.Authority.Revision).IsSuccess, Is.True);

            StableId<ItemInstanceIdScope> secondCoolerItemId =
                StableId<ItemInstanceIdScope>.Parse(
                    "item.cooler-fixture-cooler-second");
            Assert.That(fixture.Inventory.ReceiveSerializedItem(
                secondCoolerItemId,
                StableId<ProductDefinitionIdScope>.Parse(
                    "component.cooler-fixture-cooler"),
                fixture.HandsId,
                InventoryCondition.New,
                InventoryUnitCost.Create("EUR", 6_900).Value).IsSuccess, Is.True);
            StableId<AssemblyOperationIdScope> secondSeatId =
                OperationId("operation.cooler-history-second-seat");
            AssemblyOperationReceipt secondSeat =
                fixture.Authority.SeatProcessorCooler(
                    secondSeatId,
                    secondCoolerItemId,
                    fixture.CoolerSlotId,
                    ProcessorCoolerMountOrientation.Rotated180,
                    fixture.AttachId,
                    fixture.SecureId,
                    fixture.ProcessorSeatId,
                    fixture.ProcessorRetainId,
                    fixture.Authority.Revision).Value;
            AssemblyOperationReceipt secondRemove =
                fixture.Authority.RemoveProcessorCooler(
                    OperationId("operation.cooler-history-second-remove"),
                    secondCoolerItemId,
                    fixture.CoolerSlotId,
                    secondSeatId,
                    fixture.Authority.Revision).Value;
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);

            FieldInfo itemIdField = typeof(AssemblyOperationReceipt).GetField(
                "<ItemId>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(itemIdField, Is.Not.Null);
            itemIdField.SetValue(secondSeat, fixture.CoolerItemId);
            itemIdField.SetValue(secondRemove, fixture.CoolerItemId);

            Assert.That(fixture.Authority.ValidateInvariants().Error,
                Is.EqualTo(AssemblyFailures.InvariantViolation));
        }

        [Test]
        public void ConsumedTimStateSurvivesAcrossTwoBuildAuthorities()
        {
            CoolerFixture fixture = CoolerFixture.Create();
            fixture.PrepareProcessorHost();
            StableId<AssemblyOperationIdScope> firstSeatId =
                OperationId("operation.cooler-cross-build-seat");
            Assert.That(fixture.Authority.SeatProcessorCooler(
                firstSeatId,
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                ProcessorCoolerMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                fixture.ProcessorSeatId,
                fixture.ProcessorRetainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.RemoveProcessorCooler(
                OperationId("operation.cooler-cross-build-remove"),
                fixture.CoolerItemId,
                fixture.CoolerSlotId,
                firstSeatId,
                fixture.Authority.Revision).IsSuccess, Is.True);

            StableId<ContainerIdScope> workbench = ContainerId("container.second-workbench");
            StableId<ContainerIdScope> processorContainer =
                ContainerId("container.second-processor");
            StableId<ContainerIdScope> memoryContainer =
                ContainerId("container.second-memory");
            StableId<ContainerIdScope> storageContainer =
                ContainerId("container.second-storage");
            StableId<ContainerIdScope> coolerContainer =
                ContainerId("container.second-cooler");
            Register(fixture.Inventory, workbench);
            Register(fixture.Inventory, processorContainer);
            Register(fixture.Inventory, memoryContainer);
            Register(fixture.Inventory, storageContainer);
            Register(fixture.Inventory, coolerContainer);

            StableId<ItemInstanceIdScope> motherboardItem =
                StableId<ItemInstanceIdScope>.Parse("item.second-motherboard");
            StableId<ItemInstanceIdScope> processorItem =
                StableId<ItemInstanceIdScope>.Parse("item.second-processor");
            Assert.That(fixture.Inventory.ReceiveSerializedItem(
                motherboardItem,
                StableId<ProductDefinitionIdScope>.Parse(
                    "component.cooler-fixture-motherboard"),
                fixture.HandsId,
                InventoryCondition.New,
                InventoryUnitCost.Create("EUR", 14_900).Value).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ReceiveSerializedItem(
                processorItem,
                StableId<ProductDefinitionIdScope>.Parse(
                    "component.cooler-fixture-processor"),
                fixture.HandsId,
                InventoryCondition.New,
                InventoryUnitCost.Create("EUR", 24_900).Value).IsSuccess, Is.True);

            StableId<AssemblySlotIdScope> motherboardSlot =
                StableId<AssemblySlotIdScope>.Parse("slot.second-motherboard");
            StableId<AssemblyFastenerIdScope> motherboardFastener =
                StableId<AssemblyFastenerIdScope>.Parse("fastener.second-motherboard");
            StableId<AssemblySlotIdScope> processorSlot =
                StableId<AssemblySlotIdScope>.Parse("slot.second-processor");
            StableId<AssemblyRetentionIdScope> processorRetention =
                StableId<AssemblyRetentionIdScope>.Parse("retention.second-processor");
            StableId<AssemblySlotIdScope> memorySlotId =
                StableId<AssemblySlotIdScope>.Parse("slot.second-memory");
            StableId<AssemblySlotIdScope> storageSlotId =
                StableId<AssemblySlotIdScope>.Parse("slot.second-storage");
            StableId<AssemblySlotIdScope> coolerSlotId =
                StableId<AssemblySlotIdScope>.Parse("slot.second-cooler");
            StableId<AssemblyProcessorCoolerBracketIdScope> coolerBracketId =
                StableId<AssemblyProcessorCoolerBracketIdScope>.Parse(
                    "bracket.second-cooler");
            DimmSlotDefinition memorySlot = DimmSlotDefinition.Create(
                memorySlotId,
                StableId<AssemblyRetentionIdScope>.Parse("retention.second-memory"),
                memoryContainer,
                StableId<AssemblyMemoryChannelIdScope>.Parse("memory-channel.second"),
                StableId<AssemblyMemoryBankIdScope>.Parse("memory-bank.second"),
                1,
                DimmType.Ddr5Udimm).Value;
            M2SlotDefinition storageSlot = M2SlotDefinition.Create(
                storageSlotId,
                StableId<AssemblyStorageStandoffIdScope>.Parse(
                    "standoff.second-storage"),
                StableId<AssemblyRetentionIdScope>.Parse("retention.second-storage"),
                storageContainer,
                M2StorageType.NvmePcie4X4_2280).Value;
            ProcessorCoolerRetentionTopology topology =
                ProcessorCoolerRetentionTopology.Create(
                    CoolerPoint("retention.second-cooler.1"),
                    CoolerPoint("retention.second-cooler.2"),
                    CoolerPoint("retention.second-cooler.3"),
                    CoolerPoint("retention.second-cooler.4")).Value;
            ProcessorCoolerSlotDefinition coolerSlot =
                ProcessorCoolerSlotDefinition.Create(
                    coolerSlotId,
                    coolerBracketId,
                    coolerContainer,
                    topology,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700).Value;
            AssemblyBuildAuthority second =
                AssemblyBuildAuthority
                    .CreateWithProcessorSocketMemoryStorageAndCoolerSlots(
                        fixture.Components,
                        fixture.Inventory,
                        StableId<PcBuildIdScope>.Parse("build.second"),
                        StableId<ChassisIdScope>.Parse("chassis.second"),
                        motherboardSlot,
                        motherboardFastener,
                        processorSlot,
                        processorRetention,
                        memorySlot,
                        storageSlot,
                        coolerSlot,
                        fixture.HandsId,
                        workbench,
                        processorContainer,
                        MotherboardFormFactor.MicroAtx,
                        CpuSocketFamily.Lga1700).Value;
            StableId<AssemblyOperationIdScope> attach =
                OperationId("operation.second-attach");
            StableId<AssemblyOperationIdScope> secure =
                OperationId("operation.second-secure");
            StableId<AssemblyOperationIdScope> cpuSeat =
                OperationId("operation.second-cpu-seat");
            StableId<AssemblyOperationIdScope> cpuRetain =
                OperationId("operation.second-cpu-retain");
            Assert.That(second.AttachMotherboard(
                attach, motherboardItem, motherboardSlot).IsSuccess, Is.True);
            Assert.That(second.SecureMotherboardFastener(
                secure,
                motherboardItem,
                motherboardSlot,
                motherboardFastener,
                attach,
                second.Revision).IsSuccess, Is.True);
            Assert.That(second.SeatProcessor(
                cpuSeat,
                processorItem,
                processorSlot,
                attach,
                secure,
                second.Revision).IsSuccess, Is.True);
            Assert.That(second.CloseProcessorRetention(
                cpuRetain,
                processorItem,
                processorSlot,
                processorRetention,
                cpuSeat,
                second.Revision).IsSuccess, Is.True);
            long firstRevision = fixture.Authority.Revision;
            long secondRevision = second.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int firstReceiptCount = fixture.Authority.ReceiptCount;
            int secondReceiptCount = second.ReceiptCount;

            Assert.That(second.SeatProcessorCooler(
                    OperationId("operation.second-cooler-seat"),
                    fixture.CoolerItemId,
                    coolerSlotId,
                    ProcessorCoolerMountOrientation.Primary,
                    attach,
                    secure,
                    cpuSeat,
                    cpuRetain,
                    secondRevision).Error,
                Is.EqualTo(AssemblyFailures.ProcessorCoolerTimConsumed));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(firstRevision));
            Assert.That(second.Revision, Is.EqualTo(secondRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(firstReceiptCount));
            Assert.That(second.ReceiptCount, Is.EqualTo(secondReceiptCount));
            Assert.That(second.ProcessorCoolerSlotState,
                Is.EqualTo(ProcessorCoolerSlotState.EmptyOpen));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(second.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertSeatFailure(
            CoolerFixture fixture,
            string suffix,
            Failure expected,
            StableId<AssemblyOperationIdScope> attachId = default,
            StableId<AssemblyOperationIdScope> secureId = default,
            StableId<AssemblyOperationIdScope> processorSeatId = default,
            StableId<AssemblyOperationIdScope> processorRetainId = default)
        {
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.SeatProcessorCooler(
                    OperationId("operation.cooler-host-" + suffix),
                    fixture.CoolerItemId,
                    fixture.CoolerSlotId,
                    ProcessorCoolerMountOrientation.Primary,
                    attachId,
                    secureId,
                    processorSeatId,
                    processorRetainId,
                    fixture.Authority.Revision).Error,
                Is.EqualTo(expected));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
        }

        private static StableId<AssemblyOperationIdScope> OperationId(string value)
        {
            return StableId<AssemblyOperationIdScope>.Parse(value);
        }

        private static StableId<ContainerIdScope> ContainerId(string value)
        {
            return StableId<ContainerIdScope>.Parse(value);
        }

        private static StableId<AssemblyProcessorCoolerRetentionPointIdScope>
            CoolerPoint(string value)
        {
            return StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(value);
        }

        private static void Register(
            InventoryAuthority inventory,
            StableId<ContainerIdScope> containerId)
        {
            Assert.That(inventory.RegisterContainer(
                InventoryContainerDefinition.Create(
                    containerId,
                    InventoryContainerKind.Workbench,
                    1).Value).IsSuccess, Is.True);
        }

        private sealed class CoolerFixture
        {
            private CoolerFixture()
            {
            }

            public ProductCatalog Products { get; private set; }
            public PcComponentCatalog Components { get; private set; }
            public InventoryAuthority Inventory { get; private set; }
            public AssemblyBuildAuthority Authority { get; private set; }
            public StableId<PcBuildIdScope> BuildId { get; private set; }
            public StableId<ChassisIdScope> ChassisId { get; private set; }
            public StableId<AssemblySlotIdScope> MotherboardSlotId { get; private set; }
            public StableId<AssemblyFastenerIdScope> MotherboardFastenerId { get; private set; }
            public StableId<AssemblySlotIdScope> ProcessorSlotId { get; private set; }
            public StableId<AssemblyRetentionIdScope> ProcessorRetentionId { get; private set; }
            public StableId<AssemblySlotIdScope> MemorySlotId { get; private set; }
            public StableId<AssemblyRetentionIdScope> MemoryRetentionId { get; private set; }
            public StableId<AssemblySlotIdScope> StorageSlotId { get; private set; }
            public StableId<AssemblyRetentionIdScope> StorageRetentionId { get; private set; }
            public StableId<AssemblySlotIdScope> CoolerSlotId { get; private set; }
            public StableId<AssemblyProcessorCoolerBracketIdScope> CoolerBracketId { get; private set; }
            public StableId<AssemblyProcessorCoolerRetentionPointIdScope> CoolerPoint1Id { get; private set; }
            public StableId<AssemblyProcessorCoolerRetentionPointIdScope> CoolerPoint2Id { get; private set; }
            public StableId<AssemblyProcessorCoolerRetentionPointIdScope> CoolerPoint3Id { get; private set; }
            public StableId<AssemblyProcessorCoolerRetentionPointIdScope> CoolerPoint4Id { get; private set; }
            public StableId<ContainerIdScope> HandsId { get; private set; }
            public StableId<ContainerIdScope> WorkbenchId { get; private set; }
            public StableId<ContainerIdScope> ProcessorSocketContainerId { get; private set; }
            public StableId<ContainerIdScope> MemorySlotContainerId { get; private set; }
            public StableId<ContainerIdScope> StorageSlotContainerId { get; private set; }
            public StableId<ContainerIdScope> CoolerSlotContainerId { get; private set; }
            public StableId<ItemInstanceIdScope> MotherboardItemId { get; private set; }
            public StableId<ItemInstanceIdScope> ProcessorItemId { get; private set; }
            public StableId<ItemInstanceIdScope> MemoryItemId { get; private set; }
            public StableId<ItemInstanceIdScope> StorageItemId { get; private set; }
            public StableId<ItemInstanceIdScope> CoolerItemId { get; private set; }
            public StableId<AssemblyOperationIdScope> AttachId { get; private set; }
            public StableId<AssemblyOperationIdScope> SecureId { get; private set; }
            public StableId<AssemblyOperationIdScope> ProcessorSeatId { get; private set; }
            public StableId<AssemblyOperationIdScope> ProcessorRetainId { get; private set; }
            private DimmSlotDefinition MemorySlot { get; set; }
            private M2SlotDefinition StorageSlot { get; set; }
            private ProcessorCoolerSlotDefinition CoolerSlot { get; set; }

            public static CoolerFixture Create()
            {
                CoolerFixture fixture = CreateUnclaimed();
                fixture.Authority = fixture.TryCreateAuthority().Value;
                return fixture;
            }

            public static CoolerFixture CreateUnclaimed()
            {
                var fixture = new CoolerFixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.cooler-prototype"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.cooler-prototype"),
                    MotherboardSlotId = StableId<AssemblySlotIdScope>.Parse("slot.motherboard-main"),
                    MotherboardFastenerId = StableId<AssemblyFastenerIdScope>.Parse("fastener.motherboard-main-01"),
                    ProcessorSlotId = StableId<AssemblySlotIdScope>.Parse("slot.processor-main"),
                    ProcessorRetentionId = StableId<AssemblyRetentionIdScope>.Parse("retention.processor-main"),
                    MemorySlotId = StableId<AssemblySlotIdScope>.Parse("slot.memory-a2"),
                    MemoryRetentionId = StableId<AssemblyRetentionIdScope>.Parse("retention.memory-a2"),
                    StorageSlotId = StableId<AssemblySlotIdScope>.Parse("slot.m2-primary"),
                    StorageRetentionId = StableId<AssemblyRetentionIdScope>.Parse("retention.m2-primary"),
                    CoolerSlotId = StableId<AssemblySlotIdScope>.Parse("slot.cooler-cpu-main"),
                    CoolerBracketId = StableId<AssemblyProcessorCoolerBracketIdScope>.Parse("bracket.cooler-lga1700"),
                    CoolerPoint1Id = StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse("retention.cooler.point-1"),
                    CoolerPoint2Id = StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse("retention.cooler.point-2"),
                    CoolerPoint3Id = StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse("retention.cooler.point-3"),
                    CoolerPoint4Id = StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse("retention.cooler.point-4"),
                    HandsId = StableId<ContainerIdScope>.Parse("container.actor-hands"),
                    WorkbenchId = StableId<ContainerIdScope>.Parse("container.assembly-workbench"),
                    ProcessorSocketContainerId = StableId<ContainerIdScope>.Parse("container.processor-socket"),
                    MemorySlotContainerId = StableId<ContainerIdScope>.Parse("container.memory-a2"),
                    StorageSlotContainerId = StableId<ContainerIdScope>.Parse("container.m2-primary"),
                    CoolerSlotContainerId = StableId<ContainerIdScope>.Parse("container.cooler-cpu-main"),
                    MotherboardItemId = StableId<ItemInstanceIdScope>.Parse("item.cooler-fixture-motherboard"),
                    ProcessorItemId = StableId<ItemInstanceIdScope>.Parse("item.cooler-fixture-processor"),
                    MemoryItemId = StableId<ItemInstanceIdScope>.Parse("item.cooler-fixture-memory"),
                    StorageItemId = StableId<ItemInstanceIdScope>.Parse("item.cooler-fixture-storage"),
                    CoolerItemId = StableId<ItemInstanceIdScope>.Parse("item.cooler-fixture-cooler"),
                    AttachId = OperationId("operation.cooler-fixture-attach"),
                    SecureId = OperationId("operation.cooler-fixture-secure"),
                    ProcessorSeatId = OperationId("operation.cooler-fixture-cpu-seat"),
                    ProcessorRetainId = OperationId("operation.cooler-fixture-cpu-retain")
                };

                StableId<ProductDefinitionIdScope> motherboardProductId =
                    StableId<ProductDefinitionIdScope>.Parse("component.cooler-fixture-motherboard");
                StableId<ProductDefinitionIdScope> processorProductId =
                    StableId<ProductDefinitionIdScope>.Parse("component.cooler-fixture-processor");
                StableId<ProductDefinitionIdScope> memoryProductId =
                    StableId<ProductDefinitionIdScope>.Parse("component.cooler-fixture-memory");
                StableId<ProductDefinitionIdScope> storageProductId =
                    StableId<ProductDefinitionIdScope>.Parse("component.cooler-fixture-storage");
                StableId<ProductDefinitionIdScope> coolerProductId =
                    StableId<ProductDefinitionIdScope>.Parse("component.cooler-fixture-cooler");
                ProductDefinition motherboard = Definition(motherboardProductId, "Cooler Fixture Motherboard");
                ProductDefinition processor = Definition(processorProductId, "Cooler Fixture Processor");
                ProductDefinition memory = Definition(memoryProductId, "Cooler Fixture Memory");
                ProductDefinition storage = Definition(storageProductId, "Cooler Fixture Storage");
                ProductDefinition cooler = Definition(coolerProductId, "Cooler Fixture Air Cooler");
                fixture.Products = ProductCatalog.Create(new[]
                {
                    motherboard,
                    processor,
                    memory,
                    storage,
                    cooler
                }).Value;
                fixture.Components = PcComponentCatalog.Create(
                    fixture.Products,
                    new[]
                    {
                        PcComponentSpecification.CreateMotherboard(
                            fixture.Products,
                            motherboard.Id,
                            MotherboardFormFactor.MicroAtx,
                            CpuSocketFamily.Lga1700,
                            DimmType.Ddr5Udimm,
                            M2StorageType.NvmePcie4X4_2280).Value,
                        PcComponentSpecification.CreateProcessor(
                            fixture.Products,
                            processor.Id,
                            CpuSocketFamily.Lga1700).Value,
                        PcComponentSpecification.CreateMemoryModule(
                            fixture.Products,
                            memory.Id,
                            DimmType.Ddr5Udimm).Value,
                        PcComponentSpecification.CreateStorageDevice(
                            fixture.Products,
                            storage.Id,
                            M2StorageType.NvmePcie4X4_2280).Value,
                        PcComponentSpecification.CreateProcessorCooler(
                            fixture.Products,
                            cooler.Id,
                            ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                            CpuSocketFamily.Lga1700).Value
                    }).Value;
                fixture.MemorySlot = DimmSlotDefinition.Create(
                    fixture.MemorySlotId,
                    fixture.MemoryRetentionId,
                    fixture.MemorySlotContainerId,
                    StableId<AssemblyMemoryChannelIdScope>.Parse("memory-channel.a"),
                    StableId<AssemblyMemoryBankIdScope>.Parse("memory-bank.2"),
                    1,
                    DimmType.Ddr5Udimm).Value;
                fixture.StorageSlot = M2SlotDefinition.Create(
                    fixture.StorageSlotId,
                    StableId<AssemblyStorageStandoffIdScope>.Parse("standoff.m2-2280"),
                    fixture.StorageRetentionId,
                    fixture.StorageSlotContainerId,
                    M2StorageType.NvmePcie4X4_2280).Value;
                ProcessorCoolerRetentionTopology topology =
                    ProcessorCoolerRetentionTopology.Create(
                        fixture.CoolerPoint1Id,
                        fixture.CoolerPoint2Id,
                        fixture.CoolerPoint3Id,
                        fixture.CoolerPoint4Id).Value;
                fixture.CoolerSlot = ProcessorCoolerSlotDefinition.Create(
                    fixture.CoolerSlotId,
                    fixture.CoolerBracketId,
                    fixture.CoolerSlotContainerId,
                    topology,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700).Value;

                fixture.Inventory = InventoryAuthority.Create(fixture.Products).Value;
                fixture.RegisterContainer(fixture.HandsId, InventoryContainerKind.ActorHands, 5);
                fixture.RegisterContainer(fixture.WorkbenchId, InventoryContainerKind.Workbench, 1);
                fixture.RegisterContainer(fixture.ProcessorSocketContainerId, InventoryContainerKind.Workbench, 1);
                fixture.RegisterContainer(fixture.MemorySlotContainerId, InventoryContainerKind.Workbench, 1);
                fixture.RegisterContainer(fixture.StorageSlotContainerId, InventoryContainerKind.Workbench, 1);
                fixture.RegisterContainer(fixture.CoolerSlotContainerId, InventoryContainerKind.Workbench, 1);
                fixture.Receive(fixture.MotherboardItemId, motherboardProductId, 14_900);
                fixture.Receive(fixture.ProcessorItemId, processorProductId, 24_900);
                fixture.Receive(fixture.MemoryItemId, memoryProductId, 8_900);
                fixture.Receive(fixture.StorageItemId, storageProductId, 9_900);
                fixture.Receive(fixture.CoolerItemId, coolerProductId, 6_900);
                return fixture;
            }

            public OperationResult<AssemblyBuildAuthority> TryCreateAuthority()
            {
                return AssemblyBuildAuthority.CreateWithProcessorSocketMemoryStorageAndCoolerSlots(
                    Components,
                    Inventory,
                    BuildId,
                    ChassisId,
                    MotherboardSlotId,
                    MotherboardFastenerId,
                    ProcessorSlotId,
                    ProcessorRetentionId,
                    MemorySlot,
                    StorageSlot,
                    CoolerSlot,
                    HandsId,
                    WorkbenchId,
                    ProcessorSocketContainerId,
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700);
            }

            public void PrepareProcessorHost()
            {
                Assert.That(Authority.AttachMotherboard(
                    AttachId,
                    MotherboardItemId,
                    MotherboardSlotId).IsSuccess, Is.True);
                Assert.That(Authority.SecureMotherboardFastener(
                    SecureId,
                    MotherboardItemId,
                    MotherboardSlotId,
                    MotherboardFastenerId,
                    AttachId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.SeatProcessor(
                    ProcessorSeatId,
                    ProcessorItemId,
                    ProcessorSlotId,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseProcessorRetention(
                    ProcessorRetainId,
                    ProcessorItemId,
                    ProcessorSlotId,
                    ProcessorRetentionId,
                    ProcessorSeatId,
                    Authority.Revision).IsSuccess, Is.True);
            }

            public void SeatAndRetainMemory()
            {
                StableId<AssemblyOperationIdScope> seatId =
                    OperationId("operation.cooler-fixture-memory-seat");
                Assert.That(Authority.SeatMemoryModule(
                    seatId,
                    MemoryItemId,
                    MemorySlotId,
                    DimmKeyOrientation.NotchAligned,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseMemoryRetention(
                    OperationId("operation.cooler-fixture-memory-retain"),
                    MemoryItemId,
                    MemorySlotId,
                    MemoryRetentionId,
                    seatId,
                    Authority.Revision).IsSuccess, Is.True);
            }

            public void SeatAndSecureStorage()
            {
                StableId<AssemblyOperationIdScope> seatId =
                    OperationId("operation.cooler-fixture-storage-seat");
                Assert.That(Authority.SeatStorageDevice(
                    seatId,
                    StorageItemId,
                    StorageSlotId,
                    M2KeyOrientation.KeyAligned,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.SecureStorageDevice(
                    OperationId("operation.cooler-fixture-storage-secure"),
                    StorageItemId,
                    StorageSlotId,
                    StorageRetentionId,
                    seatId,
                    Authority.Revision).IsSuccess, Is.True);
            }

            private void RegisterContainer(
                StableId<ContainerIdScope> id,
                InventoryContainerKind kind,
                int capacity)
            {
                Assert.That(Inventory.RegisterContainer(
                    InventoryContainerDefinition.Create(id, kind, capacity).Value).IsSuccess,
                    Is.True);
            }

            private void Receive(
                StableId<ItemInstanceIdScope> itemId,
                StableId<ProductDefinitionIdScope> productId,
                long cents)
            {
                Assert.That(Inventory.ReceiveSerializedItem(
                    itemId,
                    productId,
                    HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", cents).Value).IsSuccess,
                    Is.True);
            }

            private static ProductDefinition Definition(
                StableId<ProductDefinitionIdScope> id,
                string displayName)
            {
                return ProductDefinition.Create(
                    id,
                    StableId<ProductCategoryIdScope>.Parse("pc-components"),
                    displayName,
                    ProductTrackingPolicy.SerializedInstance,
                    730).Value;
            }
        }
    }
}
