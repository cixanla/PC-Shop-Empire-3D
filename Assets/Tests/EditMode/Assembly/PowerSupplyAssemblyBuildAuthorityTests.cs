using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class PowerSupplyAssemblyBuildAuthorityTests
    {
        [Test]
        public void FactoryClaimsSevenContainersAtomicallyAndExposesChassisTopology()
        {
            PowerSupplyFixture fixture = PowerSupplyFixture.CreateUnclaimed();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> created = fixture.TryCreateAuthority();

            Assert.That(created.IsSuccess, Is.True);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(created.Value.HasPowerSupplyBay, Is.True);
            Assert.That(created.Value.PowerSupplyBaySlotId,
                Is.EqualTo(fixture.PowerSupplySlotId));
            Assert.That(created.Value.PowerSupplyBayContainerId,
                Is.EqualTo(fixture.PowerSupplyBayContainerId));
            Assert.That(created.Value.PowerSupplyRetentionTopology.RearMountId,
                Is.EqualTo(fixture.PowerSupplyRearMountId));
            Assert.That(created.Value.PowerSupplyRetentionTopology.PhysicalOrder,
                Is.EqualTo(fixture.PowerSupplyTopology.PhysicalOrder));
            Assert.That(created.Value.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.MotherboardItemId,
                    fixture.WorkbenchId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplyBayContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(created.Value.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SeventhContainerClaimConflictLeavesEarlierSixUnmanaged()
        {
            PowerSupplyFixture fixture = PowerSupplyFixture.CreateUnclaimed();
            Assert.That(fixture.Inventory.ClaimManagedSerializedTransferContainer(
                fixture.PowerSupplyBayContainerId).IsSuccess, Is.True);

            OperationResult<AssemblyBuildAuthority> created = fixture.TryCreateAuthority();

            Assert.That(created.Error, Is.EqualTo(AssemblyFailures.PlanForeign));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.MotherboardItemId, fixture.WorkbenchId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.ProcessorItemId,
                fixture.ProcessorSocketContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.MemoryItemId, fixture.MemorySlotContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.StorageItemId, fixture.StorageSlotContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.CoolerItemId, fixture.CoolerSlotContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotContainerId).IsSuccess, Is.True);
        }

        [Test]
        public void ExactFullCycleIsReplaySafeAndRetentionDoesNotChangeInventoryRevision()
        {
            PowerSupplyFixture fixture = PowerSupplyFixture.Create();
            StableId<AssemblyOperationIdScope> seatId = Operation("psu-seat");
            StableId<AssemblyOperationIdScope> retainId = Operation("psu-retain");
            StableId<AssemblyOperationIdScope> unretainId = Operation("psu-unretain");
            StableId<AssemblyOperationIdScope> removeId = Operation("psu-remove");
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.PowerSupplyItemId,
                out InventoryItemRecord initialPowerSupply), Is.True);
            StableId<ProductDefinitionIdScope> powerSupplyProductId =
                initialPowerSupply.ProductId;
            long inventoryBeforeSeat = fixture.Inventory.Revision;
            long seatRevision = fixture.Authority.Revision;

            AssemblyOperationReceipt seat = fixture.Authority.SeatPowerSupply(
                seatId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                PowerSupplyMountOrientation.FanToFilteredVent,
                seatRevision).Value;
            Assert.That(fixture.Authority.SeatPowerSupply(
                seatId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                PowerSupplyMountOrientation.FanToFilteredVent,
                seatRevision).Value, Is.SameAs(seat));
            long inventoryAfterSeat = fixture.Inventory.Revision;
            AssertExactPowerSupplyReceipt(
                fixture,
                seat,
                seatId,
                AssemblyOperationKind.SeatPowerSupply,
                powerSupplyProductId,
                fixture.HandsId,
                fixture.PowerSupplyBayContainerId,
                default,
                default,
                seatRevision,
                seatRevision + 1,
                inventoryAfterSeat,
                PowerSupplyBayState.EmptyOpen,
                PowerSupplyBayState.PowerSupplySeatedUnsecured,
                -1);

            long retainRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt retained = fixture.RetainPowerSupply(
                retainId,
                seatId,
                retainRevision);
            Assert.That(fixture.RetainPowerSupply(
                retainId,
                seatId,
                retainRevision), Is.SameAs(retained));
            AssertExactPowerSupplyReceipt(
                fixture,
                retained,
                retainId,
                AssemblyOperationKind.RetainPowerSupply,
                powerSupplyProductId,
                default,
                default,
                seatId,
                default,
                retainRevision,
                retainRevision + 1,
                inventoryAfterSeat,
                PowerSupplyBayState.PowerSupplySeatedUnsecured,
                PowerSupplyBayState.PowerSupplyRetained,
                0);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryAfterSeat));
            Assert.That(fixture.Authority.RemovePowerSupply(
                    Operation("psu-remove-retained"),
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    seatId,
                    fixture.Authority.Revision).Error,
                Is.EqualTo(AssemblyFailures.PowerSupplyRetained));

            long unretainRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt unretained = fixture.Authority.UnretainPowerSupply(
                unretainId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                fixture.PowerSupplyRearMountId,
                fixture.PowerSupplyTopology.TopLeftFastenerId,
                fixture.PowerSupplyTopology.TopRightFastenerId,
                fixture.PowerSupplyTopology.BottomLeftFastenerId,
                fixture.PowerSupplyTopology.BottomRightFastenerId,
                seatId,
                retainId,
                unretainRevision).Value;
            Assert.That(fixture.Authority.UnretainPowerSupply(
                unretainId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                fixture.PowerSupplyRearMountId,
                fixture.PowerSupplyTopology.TopLeftFastenerId,
                fixture.PowerSupplyTopology.TopRightFastenerId,
                fixture.PowerSupplyTopology.BottomLeftFastenerId,
                fixture.PowerSupplyTopology.BottomRightFastenerId,
                seatId,
                retainId,
                unretainRevision).Value, Is.SameAs(unretained));
            AssertExactPowerSupplyReceipt(
                fixture,
                unretained,
                unretainId,
                AssemblyOperationKind.UnretainPowerSupply,
                powerSupplyProductId,
                default,
                default,
                seatId,
                retainId,
                unretainRevision,
                unretainRevision + 1,
                inventoryAfterSeat,
                PowerSupplyBayState.PowerSupplyRetained,
                PowerSupplyBayState.PowerSupplySeatedUnsecured,
                0);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryAfterSeat));

            long removeRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt removed = fixture.Authority.RemovePowerSupply(
                removeId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                seatId,
                removeRevision).Value;
            Assert.That(fixture.Authority.RemovePowerSupply(
                removeId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                seatId,
                removeRevision).Value, Is.SameAs(removed));
            Assert.That(fixture.Inventory.Revision,
                Is.EqualTo(inventoryBeforeSeat + 2));
            AssertExactPowerSupplyReceipt(
                fixture,
                removed,
                removeId,
                AssemblyOperationKind.RemovePowerSupply,
                powerSupplyProductId,
                fixture.PowerSupplyBayContainerId,
                fixture.HandsId,
                seatId,
                default,
                removeRevision,
                removeRevision + 1,
                inventoryBeforeSeat + 2,
                PowerSupplyBayState.PowerSupplySeatedUnsecured,
                PowerSupplyBayState.EmptyOpen,
                -1);
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.PowerSupplyItemId, out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(4));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(4));

            long finalAssemblyRevision = fixture.Authority.Revision;
            long finalInventoryRevision = fixture.Inventory.Revision;
            int finalReceiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.SeatPowerSupply(
                seatId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                PowerSupplyMountOrientation.FanToFilteredVent,
                seatRevision).Value, Is.SameAs(seat));
            Assert.That(fixture.RetainPowerSupply(
                retainId,
                seatId,
                retainRevision), Is.SameAs(retained));
            Assert.That(fixture.Authority.UnretainPowerSupply(
                unretainId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                fixture.PowerSupplyRearMountId,
                fixture.PowerSupplyTopology.TopLeftFastenerId,
                fixture.PowerSupplyTopology.TopRightFastenerId,
                fixture.PowerSupplyTopology.BottomLeftFastenerId,
                fixture.PowerSupplyTopology.BottomRightFastenerId,
                seatId,
                retainId,
                unretainRevision).Value, Is.SameAs(unretained));
            Assert.That(fixture.Authority.RemovePowerSupply(
                removeId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                seatId,
                removeRevision).Value, Is.SameAs(removed));

            Assert.That(fixture.Authority.SeatPowerSupply(
                    seatId,
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    PowerSupplyMountOrientation.FanAwayFromFilteredVent,
                    seatRevision).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.RetainPowerSupply(
                    retainId,
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    fixture.PowerSupplyRearMountId,
                    fixture.PowerSupplyTopology.TopRightFastenerId,
                    fixture.PowerSupplyTopology.TopLeftFastenerId,
                    fixture.PowerSupplyTopology.BottomLeftFastenerId,
                    fixture.PowerSupplyTopology.BottomRightFastenerId,
                    seatId,
                    retainRevision).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.UnretainPowerSupply(
                    unretainId,
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    fixture.PowerSupplyRearMountId,
                    fixture.PowerSupplyTopology.TopLeftFastenerId,
                    fixture.PowerSupplyTopology.TopRightFastenerId,
                    fixture.PowerSupplyTopology.BottomLeftFastenerId,
                    fixture.PowerSupplyTopology.BottomRightFastenerId,
                    seatId,
                    Operation("psu-conflicting-retention-source"),
                    unretainRevision).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.RemovePowerSupply(
                    removeId,
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    Operation("psu-conflicting-seat-source"),
                    removeRevision).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.Revision,
                Is.EqualTo(finalAssemblyRevision));
            Assert.That(fixture.Inventory.Revision,
                Is.EqualTo(finalInventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount,
                Is.EqualTo(finalReceiptCount));
            Assert.That(fixture.Authority.GetSnapshot().PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.EmptyOpen));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void WrongOrientationCrossKindStaleOccupiedAndConflictNeverMutate()
        {
            PowerSupplyFixture fixture = PowerSupplyFixture.Create();
            AssertSeatFailure(
                fixture,
                "cross-kind",
                fixture.ProcessorItemId,
                PowerSupplyMountOrientation.FanToFilteredVent,
                AssemblyFailures.UnsupportedComponentKind);
            AssertSeatFailure(
                fixture,
                "wrong-orientation",
                fixture.PowerSupplyItemId,
                PowerSupplyMountOrientation.FanAwayFromFilteredVent,
                AssemblyFailures.PowerSupplyOrientationMismatch);

            long revision = fixture.Authority.Revision;
            Assert.That(fixture.Authority.SeatPowerSupply(
                    Operation("psu-stale"),
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    PowerSupplyMountOrientation.FanToFilteredVent,
                    revision + 1).Error,
                Is.EqualTo(AssemblyFailures.PlanStale));

            StableId<AssemblyOperationIdScope> seatId = Operation("psu-exact-seat");
            Assert.That(fixture.Authority.SeatPowerSupply(
                seatId,
                fixture.PowerSupplyItemId,
                fixture.PowerSupplySlotId,
                PowerSupplyMountOrientation.FanToFilteredVent,
                revision).IsSuccess, Is.True);
            long finalAssemblyRevision = fixture.Authority.Revision;
            long finalInventoryRevision = fixture.Inventory.Revision;
            int finalReceiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.SeatPowerSupply(
                    Operation("psu-occupied"),
                    fixture.ProcessorItemId,
                    fixture.PowerSupplySlotId,
                    PowerSupplyMountOrientation.FanToFilteredVent,
                    finalAssemblyRevision).Error,
                Is.EqualTo(AssemblyFailures.PowerSupplyBayOccupied));
            Assert.That(fixture.Authority.SeatPowerSupply(
                    seatId,
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    PowerSupplyMountOrientation.FanAwayFromFilteredVent,
                    revision).Error,
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
        public void PowerSupplyIsChassisOwnedAndDoesNotForceMotherboardOrderOrDetachGate()
        {
            PowerSupplyFixture fixture = PowerSupplyFixture.Create();
            StableId<AssemblyOperationIdScope> seatId = fixture.SeatPowerSupply();
            fixture.RetainPowerSupply(Operation("psu-order-retain"), seatId);

            Assert.That(fixture.Authority.AttachMotherboard(
                fixture.AttachId,
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.SecureMotherboardFastener(
                fixture.SecureId,
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId,
                fixture.MotherboardFastenerId,
                fixture.AttachId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.UnsecureMotherboardFastener(
                Operation("psu-order-board-unsecure"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId,
                fixture.MotherboardFastenerId,
                fixture.AttachId,
                fixture.SecureId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.DetachMotherboard(
                Operation("psu-order-board-detach"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);

            Assert.That(fixture.Authority.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplyRetained));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BenchmarkRequiresRetainedPowerSupplyThenRemainsIncompleteForCabling()
        {
            PowerSupplyFixture fixture = PowerSupplyFixture.Create();
            fixture.PrepareCanonicalPrePowerSupplyBuild();

            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.PowerSupplyMissing));
            StableId<AssemblyOperationIdScope> seatId = fixture.SeatPowerSupply();
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.PowerSupplyUnretained));
            fixture.RetainPowerSupply(Operation("psu-benchmark-retain"), seatId);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FullHandsRemovalFailsWithoutMutation()
        {
            PowerSupplyFixture fixture = PowerSupplyFixture.Create();
            StableId<AssemblyOperationIdScope> seatId = fixture.SeatPowerSupply();
            fixture.FillHandsToCapacity();
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            Assert.That(fixture.Authority.RemovePowerSupply(
                    Operation("psu-full-hands-remove"),
                    fixture.PowerSupplyItemId,
                    fixture.PowerSupplySlotId,
                    seatId,
                    assemblyRevision).Error,
                Is.EqualTo(AssemblyFailures.HandsCapacityExceeded));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.PowerSupplyBayState,
                Is.EqualTo(PowerSupplyBayState.PowerSupplySeatedUnsecured));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertExactPowerSupplyReceipt(
            PowerSupplyFixture fixture,
            AssemblyOperationReceipt receipt,
            StableId<AssemblyOperationIdScope> operationId,
            AssemblyOperationKind operationKind,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            StableId<AssemblyOperationIdScope> sourceSeatOperationId,
            StableId<AssemblyOperationIdScope> sourceRetentionOperationId,
            long expectedAssemblyRevision,
            long assemblyRevision,
            long inventoryRevision,
            PowerSupplyBayState previousState,
            PowerSupplyBayState resultingState,
            int sequenceIndex)
        {
            Assert.That(receipt, Is.Not.Null);
            Assert.That(receipt.OperationId, Is.EqualTo(operationId));
            Assert.That(receipt.OperationKind, Is.EqualTo(operationKind));
            Assert.That(receipt.BuildId, Is.EqualTo(fixture.BuildId));
            Assert.That(receipt.ChassisId, Is.EqualTo(fixture.ChassisId));
            Assert.That(receipt.SlotId, Is.EqualTo(fixture.PowerSupplySlotId));
            Assert.That(receipt.ItemId, Is.EqualTo(fixture.PowerSupplyItemId));
            Assert.That(receipt.ProductId, Is.EqualTo(productId));
            Assert.That(receipt.SourceContainerId,
                Is.EqualTo(sourceContainerId));
            Assert.That(receipt.TargetContainerId,
                Is.EqualTo(targetContainerId));
            Assert.That(receipt.SourcePowerSupplySeatOperationId,
                Is.EqualTo(sourceSeatOperationId));
            Assert.That(receipt.SourcePowerSupplyRetentionOperationId,
                Is.EqualTo(sourceRetentionOperationId));
            Assert.That(receipt.ExpectedAssemblyRevision,
                Is.EqualTo(expectedAssemblyRevision));
            Assert.That(receipt.AssemblyRevision,
                Is.EqualTo(assemblyRevision));
            Assert.That(receipt.InventoryRevision,
                Is.EqualTo(inventoryRevision));
            Assert.That(receipt.PreviousPowerSupplyBayState,
                Is.EqualTo(previousState));
            Assert.That(receipt.ResultingPowerSupplyBayState,
                Is.EqualTo(resultingState));
            Assert.That(receipt.PowerSupplyMountOrientation,
                Is.EqualTo(PowerSupplyMountOrientation.FanToFilteredVent));
            Assert.That(receipt.SequenceIndex, Is.EqualTo(sequenceIndex));

            PowerSupplyBayDefinition bay = receipt.PowerSupplyBayDefinition;
            Assert.That(bay.IsValid, Is.True);
            Assert.That(bay.SlotId, Is.EqualTo(fixture.PowerSupplySlotId));
            Assert.That(bay.ContainerId,
                Is.EqualTo(fixture.PowerSupplyBayContainerId));
            Assert.That(bay.SupportedPowerSupplyType,
                Is.EqualTo(PowerSupplyType.AtxPs2));
            Assert.That(bay.RetentionTopology.RearMountId,
                Is.EqualTo(fixture.PowerSupplyRearMountId));
            Assert.That(bay.RetentionTopology.TopLeftFastenerId,
                Is.EqualTo(fixture.PowerSupplyTopology.TopLeftFastenerId));
            Assert.That(bay.RetentionTopology.TopRightFastenerId,
                Is.EqualTo(fixture.PowerSupplyTopology.TopRightFastenerId));
            Assert.That(bay.RetentionTopology.BottomLeftFastenerId,
                Is.EqualTo(fixture.PowerSupplyTopology.BottomLeftFastenerId));
            Assert.That(bay.RetentionTopology.BottomRightFastenerId,
                Is.EqualTo(fixture.PowerSupplyTopology.BottomRightFastenerId));
            Assert.That(bay.RetentionTopology.PhysicalOrder,
                Is.EqualTo(fixture.PowerSupplyTopology.PhysicalOrder));
            Assert.That(bay.RetentionTopology.DeterministicRetentionOrder,
                Is.EqualTo(fixture.PowerSupplyTopology.DeterministicRetentionOrder));
            Assert.That(bay.RetentionTopology.ReverseRetentionOrder,
                Is.EqualTo(fixture.PowerSupplyTopology.ReverseRetentionOrder));
        }

        private static void AssertSeatFailure(
            PowerSupplyFixture fixture,
            string suffix,
            StableId<ItemInstanceIdScope> itemId,
            PowerSupplyMountOrientation orientation,
            Failure expected)
        {
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.SeatPowerSupply(
                    Operation("operation." + suffix),
                    itemId,
                    fixture.PowerSupplySlotId,
                    orientation,
                    assemblyRevision).Error,
                Is.EqualTo(expected));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
        }

        private static StableId<AssemblyOperationIdScope> Operation(string value)
        {
            return StableId<AssemblyOperationIdScope>.Parse("operation.power-supply." + value);
        }

        private sealed class PowerSupplyFixture
        {
            public PcComponentCatalog Components { get; private set; }
            public InventoryAuthority Inventory { get; private set; }
            public AssemblyBuildAuthority Authority { get; private set; }
            public StableId<PcBuildIdScope> BuildId { get; private set; }
            public StableId<ChassisIdScope> ChassisId { get; private set; }
            public StableId<AssemblySlotIdScope> MotherboardSlotId { get; private set; }
            public StableId<AssemblyFastenerIdScope> MotherboardFastenerId { get; private set; }
            public StableId<AssemblySlotIdScope> ProcessorSlotId { get; private set; }
            public StableId<AssemblyRetentionIdScope> ProcessorRetentionId { get; private set; }
            public DimmSlotDefinition MemorySlot { get; private set; }
            public M2SlotDefinition StorageSlot { get; private set; }
            public ProcessorCoolerSlotDefinition CoolerSlot { get; private set; }
            public GraphicsCardSlotDefinition GraphicsCardSlot { get; private set; }
            public PowerSupplyBayDefinition PowerSupplyBay { get; private set; }
            public PowerSupplyRetentionTopology PowerSupplyTopology { get; private set; }
            public StableId<AssemblySlotIdScope> PowerSupplySlotId { get; private set; }
            public StableId<AssemblyPowerSupplyRearMountIdScope> PowerSupplyRearMountId
            {
                get;
                private set;
            }
            public StableId<ContainerIdScope> HandsId { get; private set; }
            public StableId<ContainerIdScope> WorkbenchId { get; private set; }
            public StableId<ContainerIdScope> ProcessorSocketContainerId { get; private set; }
            public StableId<ContainerIdScope> MemorySlotContainerId { get; private set; }
            public StableId<ContainerIdScope> StorageSlotContainerId { get; private set; }
            public StableId<ContainerIdScope> CoolerSlotContainerId { get; private set; }
            public StableId<ContainerIdScope> GraphicsCardSlotContainerId { get; private set; }
            public StableId<ContainerIdScope> PowerSupplyBayContainerId { get; private set; }
            public StableId<ItemInstanceIdScope> MotherboardItemId { get; private set; }
            public StableId<ItemInstanceIdScope> ProcessorItemId { get; private set; }
            public StableId<ItemInstanceIdScope> MemoryItemId { get; private set; }
            public StableId<ItemInstanceIdScope> StorageItemId { get; private set; }
            public StableId<ItemInstanceIdScope> CoolerItemId { get; private set; }
            public StableId<ItemInstanceIdScope> GraphicsCardItemId { get; private set; }
            public StableId<ItemInstanceIdScope> PowerSupplyItemId { get; private set; }
            public StableId<AssemblyOperationIdScope> AttachId { get; private set; }
            public StableId<AssemblyOperationIdScope> SecureId { get; private set; }

            public static PowerSupplyFixture Create()
            {
                PowerSupplyFixture fixture = CreateUnclaimed();
                fixture.Authority = fixture.TryCreateAuthority().Value;
                return fixture;
            }

            public static PowerSupplyFixture CreateUnclaimed()
            {
                var fixture = new PowerSupplyFixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.power-supply"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.power-supply"),
                    MotherboardSlotId = Slot("slot.motherboard-main"),
                    MotherboardFastenerId = Fastener("fastener.motherboard-main"),
                    ProcessorSlotId = Slot("slot.processor-main"),
                    ProcessorRetentionId = Retention("retention.processor-main"),
                    HandsId = Container("container.actor-hands"),
                    WorkbenchId = Container("container.assembly-workbench"),
                    ProcessorSocketContainerId = Container("container.processor"),
                    MemorySlotContainerId = Container("container.memory-a2"),
                    StorageSlotContainerId = Container("container.m2-primary"),
                    CoolerSlotContainerId = Container("container.cooler-main"),
                    GraphicsCardSlotContainerId = Container("container.graphics-card"),
                    PowerSupplyBayContainerId = Container("container.power-supply-bay"),
                    MotherboardItemId = Item("item.power-supply-motherboard"),
                    ProcessorItemId = Item("item.power-supply-processor"),
                    MemoryItemId = Item("item.power-supply-memory"),
                    StorageItemId = Item("item.power-supply-storage"),
                    CoolerItemId = Item("item.power-supply-cooler"),
                    GraphicsCardItemId = Item("item.power-supply-graphics-card"),
                    PowerSupplyItemId = Item("item.power-supply-psu"),
                    PowerSupplySlotId = Slot("slot.power-supply-bottom-rear"),
                    PowerSupplyRearMountId =
                        StableId<AssemblyPowerSupplyRearMountIdScope>.Parse(
                            "mount.power-supply-rear"),
                    AttachId = Operation("fixture-board-attach"),
                    SecureId = Operation("fixture-board-secure")
                };

                StableId<ProductDefinitionIdScope> motherboardProduct =
                    Product("component.power-supply-motherboard");
                StableId<ProductDefinitionIdScope> processorProduct =
                    Product("component.power-supply-processor");
                StableId<ProductDefinitionIdScope> memoryProduct =
                    Product("component.power-supply-memory");
                StableId<ProductDefinitionIdScope> storageProduct =
                    Product("component.power-supply-storage");
                StableId<ProductDefinitionIdScope> coolerProduct =
                    Product("component.power-supply-cooler");
                StableId<ProductDefinitionIdScope> graphicsCardProduct =
                    Product("component.power-supply-graphics-card");
                StableId<ProductDefinitionIdScope> powerSupplyProduct =
                    Product("component.power-supply-psu");
                ProductCatalog products = ProductCatalog.Create(new[]
                {
                    Definition(motherboardProduct),
                    Definition(processorProduct),
                    Definition(memoryProduct),
                    Definition(storageProduct),
                    Definition(coolerProduct),
                    Definition(graphicsCardProduct),
                    Definition(powerSupplyProduct)
                }).Value;
                fixture.Components = PcComponentCatalog.Create(products, new[]
                {
                    PcComponentSpecification.CreateMotherboard(
                        products,
                        motherboardProduct,
                        MotherboardFormFactor.MicroAtx,
                        CpuSocketFamily.Lga1700,
                        DimmType.Ddr5Udimm,
                        M2StorageType.NvmePcie4X4_2280,
                        GraphicsCardType.Pcie4X16FullHeightDualSlot).Value,
                    PcComponentSpecification.CreateProcessor(
                        products,
                        processorProduct,
                        CpuSocketFamily.Lga1700).Value,
                    PcComponentSpecification.CreateMemoryModule(
                        products,
                        memoryProduct,
                        DimmType.Ddr5Udimm).Value,
                    PcComponentSpecification.CreateStorageDevice(
                        products,
                        storageProduct,
                        M2StorageType.NvmePcie4X4_2280).Value,
                    PcComponentSpecification.CreateProcessorCooler(
                        products,
                        coolerProduct,
                        ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                        CpuSocketFamily.Lga1700).Value,
                    PcComponentSpecification.CreateGraphicsCard(
                        products,
                        graphicsCardProduct,
                        GraphicsCardType.Pcie4X16FullHeightDualSlot).Value,
                    PcComponentSpecification.CreatePowerSupply(
                        products,
                        powerSupplyProduct,
                        PowerSupplyType.AtxPs2).Value
                }).Value;

                fixture.MemorySlot = DimmSlotDefinition.Create(
                    Slot("slot.memory-a2"),
                    Retention("retention.memory-a2"),
                    fixture.MemorySlotContainerId,
                    StableId<AssemblyMemoryChannelIdScope>.Parse("memory-channel.a"),
                    StableId<AssemblyMemoryBankIdScope>.Parse("memory-bank.2"),
                    1,
                    DimmType.Ddr5Udimm).Value;
                fixture.StorageSlot = M2SlotDefinition.Create(
                    Slot("slot.m2-primary"),
                    StableId<AssemblyStorageStandoffIdScope>.Parse("standoff.m2-2280"),
                    Retention("retention.m2-primary"),
                    fixture.StorageSlotContainerId,
                    M2StorageType.NvmePcie4X4_2280).Value;
                fixture.CoolerSlot = ProcessorCoolerSlotDefinition.Create(
                    Slot("slot.cooler-main"),
                    StableId<AssemblyProcessorCoolerBracketIdScope>.Parse(
                        "bracket.cooler-lga1700"),
                    fixture.CoolerSlotContainerId,
                    ProcessorCoolerRetentionTopology.Create(
                        CoolerPoint(1),
                        CoolerPoint(2),
                        CoolerPoint(3),
                        CoolerPoint(4)).Value,
                    ProcessorCoolerType.Lga1700TopDownAirPreAppliedTim,
                    CpuSocketFamily.Lga1700).Value;
                fixture.GraphicsCardSlot = GraphicsCardSlotDefinition.Create(
                    Slot("slot.graphics-card-x16"),
                    fixture.GraphicsCardSlotContainerId,
                    GraphicsCardRetentionTopology.Create(
                        StableId<AssemblyGraphicsCardLatchIdScope>.Parse(
                            "latch.graphics-card-x16"),
                        Fastener("fastener.graphics-card-bracket-01")).Value,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot).Value;
                fixture.PowerSupplyTopology = PowerSupplyRetentionTopology.Create(
                    fixture.PowerSupplyRearMountId,
                    Fastener("fastener.power-supply-01"),
                    Fastener("fastener.power-supply-02"),
                    Fastener("fastener.power-supply-03"),
                    Fastener("fastener.power-supply-04")).Value;
                fixture.PowerSupplyBay = PowerSupplyBayDefinition.Create(
                    fixture.PowerSupplySlotId,
                    fixture.PowerSupplyBayContainerId,
                    fixture.PowerSupplyTopology,
                    PowerSupplyType.AtxPs2).Value;

                fixture.Inventory = InventoryAuthority.Create(products).Value;
                fixture.Register(fixture.HandsId, InventoryContainerKind.ActorHands, 8);
                fixture.Register(fixture.WorkbenchId, InventoryContainerKind.Workbench, 1);
                fixture.Register(
                    fixture.ProcessorSocketContainerId,
                    InventoryContainerKind.Workbench,
                    1);
                fixture.Register(fixture.MemorySlotContainerId,
                    InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.StorageSlotContainerId,
                    InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.CoolerSlotContainerId,
                    InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.GraphicsCardSlotContainerId,
                    InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.PowerSupplyBayContainerId,
                    InventoryContainerKind.Workbench, 1);
                fixture.Receive(fixture.MotherboardItemId, motherboardProduct);
                fixture.Receive(fixture.ProcessorItemId, processorProduct);
                fixture.Receive(fixture.MemoryItemId, memoryProduct);
                fixture.Receive(fixture.StorageItemId, storageProduct);
                fixture.Receive(fixture.CoolerItemId, coolerProduct);
                fixture.Receive(fixture.GraphicsCardItemId, graphicsCardProduct);
                fixture.Receive(fixture.PowerSupplyItemId, powerSupplyProduct);
                return fixture;
            }

            public OperationResult<AssemblyBuildAuthority> TryCreateAuthority()
            {
                return AssemblyBuildAuthority
                    .CreateWithProcessorSocketMemoryStorageCoolerGraphicsCardAndPowerSupplySlots(
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
                        GraphicsCardSlot,
                        PowerSupplyBay,
                        HandsId,
                        WorkbenchId,
                        ProcessorSocketContainerId,
                        MotherboardFormFactor.MicroAtx,
                        CpuSocketFamily.Lga1700);
            }

            public StableId<AssemblyOperationIdScope> SeatPowerSupply()
            {
                StableId<AssemblyOperationIdScope> operationId =
                    Operation("fixture-psu-seat");
                Assert.That(Authority.SeatPowerSupply(
                    operationId,
                    PowerSupplyItemId,
                    PowerSupplySlotId,
                    PowerSupplyMountOrientation.FanToFilteredVent,
                    Authority.Revision).IsSuccess, Is.True);
                return operationId;
            }

            public AssemblyOperationReceipt RetainPowerSupply(
                StableId<AssemblyOperationIdScope> operationId,
                StableId<AssemblyOperationIdScope> seatId,
                long? expectedRevision = null)
            {
                OperationResult<AssemblyOperationReceipt> result = Authority.RetainPowerSupply(
                    operationId,
                    PowerSupplyItemId,
                    PowerSupplySlotId,
                    PowerSupplyRearMountId,
                    PowerSupplyTopology.TopLeftFastenerId,
                    PowerSupplyTopology.TopRightFastenerId,
                    PowerSupplyTopology.BottomLeftFastenerId,
                    PowerSupplyTopology.BottomRightFastenerId,
                    seatId,
                    expectedRevision ?? Authority.Revision);
                Assert.That(result.IsSuccess, Is.True);
                return result.Value;
            }

            public void PrepareCanonicalPrePowerSupplyBuild()
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

                StableId<AssemblyOperationIdScope> processorSeat =
                    Operation("fixture-processor-seat");
                StableId<AssemblyOperationIdScope> processorRetain =
                    Operation("fixture-processor-retain");
                Assert.That(Authority.SeatProcessor(
                    processorSeat,
                    ProcessorItemId,
                    ProcessorSlotId,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseProcessorRetention(
                    processorRetain,
                    ProcessorItemId,
                    ProcessorSlotId,
                    ProcessorRetentionId,
                    processorSeat,
                    Authority.Revision).IsSuccess, Is.True);

                StableId<AssemblyOperationIdScope> memorySeat =
                    Operation("fixture-memory-seat");
                Assert.That(Authority.SeatMemoryModule(
                    memorySeat,
                    MemoryItemId,
                    MemorySlot.SlotId,
                    DimmKeyOrientation.NotchAligned,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseMemoryRetention(
                    Operation("fixture-memory-retain"),
                    MemoryItemId,
                    MemorySlot.SlotId,
                    MemorySlot.RetentionId,
                    memorySeat,
                    Authority.Revision).IsSuccess, Is.True);

                StableId<AssemblyOperationIdScope> storageSeat =
                    Operation("fixture-storage-seat");
                Assert.That(Authority.SeatStorageDevice(
                    storageSeat,
                    StorageItemId,
                    StorageSlot.SlotId,
                    M2KeyOrientation.KeyAligned,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.SecureStorageDevice(
                    Operation("fixture-storage-retain"),
                    StorageItemId,
                    StorageSlot.SlotId,
                    StorageSlot.CaptiveScrewId,
                    storageSeat,
                    Authority.Revision).IsSuccess, Is.True);

                StableId<AssemblyOperationIdScope> coolerSeat =
                    Operation("fixture-cooler-seat");
                Assert.That(Authority.SeatProcessorCooler(
                    coolerSeat,
                    CoolerItemId,
                    CoolerSlot.SlotId,
                    ProcessorCoolerMountOrientation.Primary,
                    AttachId,
                    SecureId,
                    processorSeat,
                    processorRetain,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.RetainProcessorCooler(
                    Operation("fixture-cooler-retain"),
                    CoolerItemId,
                    CoolerSlot.SlotId,
                    CoolerSlot.BracketId,
                    coolerSeat,
                    Authority.Revision).IsSuccess, Is.True);

                StableId<AssemblyOperationIdScope> graphicsSeat =
                    Operation("fixture-graphics-seat");
                Assert.That(Authority.SeatGraphicsCard(
                    graphicsSeat,
                    GraphicsCardItemId,
                    GraphicsCardSlot.SlotId,
                    GraphicsCardMountOrientation.Primary,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.RetainGraphicsCard(
                    Operation("fixture-graphics-retain"),
                    GraphicsCardItemId,
                    GraphicsCardSlot.SlotId,
                    GraphicsCardSlot.RetentionTopology.LatchId,
                    GraphicsCardSlot.RetentionTopology.BracketFastenerId,
                    graphicsSeat,
                    Authority.Revision).IsSuccess, Is.True);
            }

            public void FillHandsToCapacity()
            {
                Receive(Item("item.power-supply-filler-a"), PowerSupplyItemProductId());
                Receive(Item("item.power-supply-filler-b"), PowerSupplyItemProductId());
                Assert.That(Inventory.GetContainerQuantity(HandsId).Value, Is.EqualTo(8));
            }

            private StableId<ProductDefinitionIdScope> PowerSupplyItemProductId()
            {
                Assert.That(Inventory.TryGetSerializedItem(
                    PowerSupplyItemId,
                    out InventoryItemRecord item), Is.True);
                return item.ProductId;
            }

            private void Register(
                StableId<ContainerIdScope> id,
                InventoryContainerKind kind,
                int capacity)
            {
                Assert.That(Inventory.RegisterContainer(
                    InventoryContainerDefinition.Create(id, kind, capacity).Value)
                    .IsSuccess, Is.True);
            }

            private void Receive(
                StableId<ItemInstanceIdScope> itemId,
                StableId<ProductDefinitionIdScope> productId)
            {
                Assert.That(Inventory.ReceiveSerializedItem(
                    itemId,
                    productId,
                    HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", 9_900).Value).IsSuccess,
                    Is.True);
            }

            private static StableId<AssemblySlotIdScope> Slot(string value) =>
                StableId<AssemblySlotIdScope>.Parse(value);

            private static StableId<AssemblyFastenerIdScope> Fastener(string value) =>
                StableId<AssemblyFastenerIdScope>.Parse(value);

            private static StableId<AssemblyRetentionIdScope> Retention(string value) =>
                StableId<AssemblyRetentionIdScope>.Parse(value);

            private static StableId<ContainerIdScope> Container(string value) =>
                StableId<ContainerIdScope>.Parse(value);

            private static StableId<ItemInstanceIdScope> Item(string value) =>
                StableId<ItemInstanceIdScope>.Parse(value);

            private static StableId<ProductDefinitionIdScope> Product(string value) =>
                StableId<ProductDefinitionIdScope>.Parse(value);

            private static StableId<AssemblyProcessorCoolerRetentionPointIdScope>
                CoolerPoint(int index)
            {
                return StableId<AssemblyProcessorCoolerRetentionPointIdScope>.Parse(
                    "retention.cooler.point-" + index);
            }

            private static ProductDefinition Definition(
                StableId<ProductDefinitionIdScope> id)
            {
                return ProductDefinition.Create(
                    id,
                    StableId<ProductCategoryIdScope>.Parse("pc-components"),
                    id.Value,
                    ProductTrackingPolicy.SerializedInstance,
                    730).Value;
            }
        }
    }
}
