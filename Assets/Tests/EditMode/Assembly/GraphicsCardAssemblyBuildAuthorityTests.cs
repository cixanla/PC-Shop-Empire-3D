using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class GraphicsCardAssemblyBuildAuthorityTests
    {
        [Test]
        public void FactoryClaimsSixContainersAtomicallyAndExposesTopology()
        {
            GraphicsCardFixture fixture = GraphicsCardFixture.CreateUnclaimed();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> created = fixture.TryCreateAuthority();

            Assert.That(created.IsSuccess, Is.True);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(created.Value.HasGraphicsCardSlot, Is.True);
            Assert.That(created.Value.GraphicsCardSlotId,
                Is.EqualTo(fixture.GraphicsCardSlotId));
            Assert.That(created.Value.GraphicsCardSlotContainerId,
                Is.EqualTo(fixture.GraphicsCardSlotContainerId));
            Assert.That(created.Value.GraphicsCardRetentionTopology.LatchId,
                Is.EqualTo(fixture.GraphicsCardLatchId));
            Assert.That(created.Value.GraphicsCardRetentionTopology.BracketFastenerId,
                Is.EqualTo(fixture.GraphicsCardBracketFastenerId));
            Assert.That(created.Value.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.MotherboardItemId,
                    fixture.WorkbenchId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.GraphicsCardItemId,
                    fixture.GraphicsCardSlotContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(created.Value.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SixthContainerClaimConflictLeavesEarlierFiveUnmanaged()
        {
            GraphicsCardFixture fixture = GraphicsCardFixture.CreateUnclaimed();
            Assert.That(fixture.Inventory.ClaimManagedSerializedTransferContainer(
                fixture.GraphicsCardSlotContainerId).IsSuccess, Is.True);

            OperationResult<AssemblyBuildAuthority> created = fixture.TryCreateAuthority();

            Assert.That(created.Error, Is.EqualTo(AssemblyFailures.PlanForeign));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.MotherboardItemId,
                    fixture.WorkbenchId).IsSuccess,
                Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.ProcessorItemId,
                    fixture.ProcessorSocketContainerId).IsSuccess,
                Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.MemoryItemId,
                    fixture.MemorySlotContainerId).IsSuccess,
                Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.StorageItemId,
                    fixture.StorageSlotContainerId).IsSuccess,
                Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.CoolerItemId,
                    fixture.CoolerSlotContainerId).IsSuccess,
                Is.True);
        }

        [Test]
        public void ExactFullCycleIsReplaySafeAndPreservesReceiptLineage()
        {
            GraphicsCardFixture fixture = GraphicsCardFixture.Create();
            fixture.PrepareMotherboardHost();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.graphics-card-seat");
            StableId<AssemblyOperationIdScope> retainId =
                OperationId("operation.graphics-card-retain");
            StableId<AssemblyOperationIdScope> unretainId =
                OperationId("operation.graphics-card-unretain");
            StableId<AssemblyOperationIdScope> removeId =
                OperationId("operation.graphics-card-remove");
            long inventoryBeforeSeat = fixture.Inventory.Revision;
            long seatRevision = fixture.Authority.Revision;

            AssemblyOperationReceipt seat = fixture.Authority.SeatGraphicsCard(
                seatId,
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                GraphicsCardMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                seatRevision).Value;
            Assert.That(fixture.Authority.SeatGraphicsCard(
                seatId,
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                GraphicsCardMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                seatRevision).Value, Is.SameAs(seat));
            Assert.That(seat.SourceAttachOperationId, Is.EqualTo(fixture.AttachId));
            Assert.That(seat.SourceSecureOperationId, Is.EqualTo(fixture.SecureId));
            Assert.That(seat.ResultingGraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));

            long retainRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt retained = fixture.Authority.RetainGraphicsCard(
                retainId,
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                fixture.GraphicsCardLatchId,
                fixture.GraphicsCardBracketFastenerId,
                seatId,
                retainRevision).Value;
            Assert.That(fixture.Authority.RetainGraphicsCard(
                retainId,
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                fixture.GraphicsCardLatchId,
                fixture.GraphicsCardBracketFastenerId,
                seatId,
                retainRevision).Value, Is.SameAs(retained));
            Assert.That(retained.SourceGraphicsCardSeatOperationId,
                Is.EqualTo(seatId));
            Assert.That(retained.ResultingGraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardRetained));
            Assert.That(fixture.Authority.RemoveGraphicsCard(
                    OperationId("operation.graphics-card-remove-retained"),
                    fixture.GraphicsCardItemId,
                    fixture.GraphicsCardSlotId,
                    seatId,
                    fixture.Authority.Revision).Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardRetained));

            long unretainRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt unretained =
                fixture.Authority.UnretainGraphicsCard(
                    unretainId,
                    fixture.GraphicsCardItemId,
                    fixture.GraphicsCardSlotId,
                    fixture.GraphicsCardLatchId,
                    fixture.GraphicsCardBracketFastenerId,
                    seatId,
                    retainId,
                    unretainRevision).Value;
            Assert.That(unretained.SourceGraphicsCardSeatOperationId,
                Is.EqualTo(seatId));
            Assert.That(unretained.SourceGraphicsCardRetentionOperationId,
                Is.EqualTo(retainId));

            long removeRevision = fixture.Authority.Revision;
            AssemblyOperationReceipt removed = fixture.Authority.RemoveGraphicsCard(
                removeId,
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                seatId,
                removeRevision).Value;
            Assert.That(fixture.Authority.RemoveGraphicsCard(
                removeId,
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                seatId,
                removeRevision).Value, Is.SameAs(removed));
            Assert.That(removed.SourceGraphicsCardSeatOperationId,
                Is.EqualTo(seatId));
            Assert.That(removed.ResultingGraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.EmptyOpen));
            Assert.That(fixture.Inventory.Revision,
                Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.GraphicsCardItemId, out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(6));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(6));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void WrongOrientationCrossKindStaleOccupiedAndConflictNeverMutate()
        {
            GraphicsCardFixture fixture = GraphicsCardFixture.Create();
            AssertSeatFailure(
                fixture,
                "missing-board",
                fixture.GraphicsCardItemId,
                GraphicsCardMountOrientation.Primary,
                AssemblyFailures.MotherboardMissing);
            fixture.PrepareMotherboardHost();
            AssertSeatFailure(
                fixture,
                "cross-kind",
                fixture.ProcessorItemId,
                GraphicsCardMountOrientation.Primary,
                AssemblyFailures.UnsupportedComponentKind,
                fixture.AttachId,
                fixture.SecureId);
            AssertSeatFailure(
                fixture,
                "wrong-orientation",
                fixture.GraphicsCardItemId,
                GraphicsCardMountOrientation.Rotated180,
                AssemblyFailures.GraphicsCardOrientationMismatch,
                fixture.AttachId,
                fixture.SecureId);

            long revision = fixture.Authority.Revision;
            Assert.That(fixture.Authority.SeatGraphicsCard(
                    OperationId("operation.graphics-card-stale"),
                    fixture.GraphicsCardItemId,
                    fixture.GraphicsCardSlotId,
                    GraphicsCardMountOrientation.Primary,
                    fixture.AttachId,
                    fixture.SecureId,
                    revision - 1).Error,
                Is.EqualTo(AssemblyFailures.PlanStale));

            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.graphics-card-exact-seat");
            Assert.That(fixture.Authority.SeatGraphicsCard(
                seatId,
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                GraphicsCardMountOrientation.Primary,
                fixture.AttachId,
                fixture.SecureId,
                revision).IsSuccess, Is.True);
            long finalAssemblyRevision = fixture.Authority.Revision;
            long finalInventoryRevision = fixture.Inventory.Revision;
            int finalReceiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.SeatGraphicsCard(
                    OperationId("operation.graphics-card-occupied"),
                    fixture.ProcessorItemId,
                    fixture.GraphicsCardSlotId,
                    GraphicsCardMountOrientation.Primary,
                    fixture.AttachId,
                    fixture.SecureId,
                    finalAssemblyRevision).Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardSlotOccupied));
            Assert.That(fixture.Authority.SeatGraphicsCard(
                    seatId,
                    fixture.GraphicsCardItemId,
                    fixture.GraphicsCardSlotId,
                    GraphicsCardMountOrientation.Rotated180,
                    fixture.AttachId,
                    fixture.SecureId,
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
        public void FullHandsRemovalFailsWithoutMutation()
        {
            GraphicsCardFixture fixture = GraphicsCardFixture.Create();
            fixture.PrepareMotherboardHost();
            StableId<AssemblyOperationIdScope> seatId = fixture.SeatGraphicsCard();
            StableId<AssemblyOperationIdScope> retainId =
                fixture.RetainGraphicsCard(seatId);
            Assert.That(fixture.Authority.UnretainGraphicsCard(
                OperationId("operation.graphics-card-full-hands-unretain"),
                fixture.GraphicsCardItemId,
                fixture.GraphicsCardSlotId,
                fixture.GraphicsCardLatchId,
                fixture.GraphicsCardBracketFastenerId,
                seatId,
                retainId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            fixture.FillHandsToCapacity();
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            Assert.That(fixture.Authority.RemoveGraphicsCard(
                    OperationId("operation.graphics-card-full-hands-remove"),
                    fixture.GraphicsCardItemId,
                    fixture.GraphicsCardSlotId,
                    seatId,
                    assemblyRevision).Error,
                Is.EqualTo(AssemblyFailures.HandsCapacityExceeded));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.GraphicsCardSlotState,
                Is.EqualTo(GraphicsCardSlotState.GraphicsCardSeatedUnsecured));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InstalledGraphicsCardWinsMotherboardDetachGatePrecedence()
        {
            GraphicsCardFixture fixture = GraphicsCardFixture.Create();
            fixture.PrepareMotherboardHost();
            StableId<AssemblyOperationIdScope> seatId = fixture.SeatGraphicsCard();

            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.graphics-card-host-detach-seated"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardInstalled));
            fixture.RetainGraphicsCard(seatId);
            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.graphics-card-host-detach-retained"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardInstalled));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BenchmarkRequiresRetainedGraphicsCardThenRemainsIncomplete()
        {
            GraphicsCardFixture fixture = GraphicsCardFixture.Create();
            fixture.PrepareCanonicalPreGraphicsCardBuild();

            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardMissing));
            StableId<AssemblyOperationIdScope> seatId = fixture.SeatGraphicsCard();
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.GraphicsCardUnretained));
            fixture.RetainGraphicsCard(seatId);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void AssertSeatFailure(
            GraphicsCardFixture fixture,
            string suffix,
            StableId<ItemInstanceIdScope> itemId,
            GraphicsCardMountOrientation orientation,
            Failure expected,
            StableId<AssemblyOperationIdScope> attachId = default,
            StableId<AssemblyOperationIdScope> secureId = default)
        {
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.SeatGraphicsCard(
                    OperationId("operation.graphics-card-" + suffix),
                    itemId,
                    fixture.GraphicsCardSlotId,
                    orientation,
                    attachId,
                    secureId,
                    assemblyRevision).Error,
                Is.EqualTo(expected));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
        }

        private static StableId<AssemblyOperationIdScope> OperationId(string value)
        {
            return StableId<AssemblyOperationIdScope>.Parse(value);
        }

        private sealed class GraphicsCardFixture
        {
            private GraphicsCardFixture()
            {
            }

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
            public StableId<AssemblySlotIdScope> GraphicsCardSlotId { get; private set; }
            public StableId<AssemblyGraphicsCardLatchIdScope> GraphicsCardLatchId { get; private set; }
            public StableId<AssemblyFastenerIdScope> GraphicsCardBracketFastenerId { get; private set; }
            public StableId<ContainerIdScope> HandsId { get; private set; }
            public StableId<ContainerIdScope> WorkbenchId { get; private set; }
            public StableId<ContainerIdScope> ProcessorSocketContainerId { get; private set; }
            public StableId<ContainerIdScope> MemorySlotContainerId { get; private set; }
            public StableId<ContainerIdScope> StorageSlotContainerId { get; private set; }
            public StableId<ContainerIdScope> CoolerSlotContainerId { get; private set; }
            public StableId<ContainerIdScope> GraphicsCardSlotContainerId { get; private set; }
            public StableId<ItemInstanceIdScope> MotherboardItemId { get; private set; }
            public StableId<ItemInstanceIdScope> ProcessorItemId { get; private set; }
            public StableId<ItemInstanceIdScope> MemoryItemId { get; private set; }
            public StableId<ItemInstanceIdScope> StorageItemId { get; private set; }
            public StableId<ItemInstanceIdScope> CoolerItemId { get; private set; }
            public StableId<ItemInstanceIdScope> GraphicsCardItemId { get; private set; }
            public StableId<AssemblyOperationIdScope> AttachId { get; private set; }
            public StableId<AssemblyOperationIdScope> SecureId { get; private set; }
            private StableId<ProductDefinitionIdScope> GraphicsCardProductId { get; set; }
            private DimmSlotDefinition MemorySlot { get; set; }
            private M2SlotDefinition StorageSlot { get; set; }
            private ProcessorCoolerSlotDefinition CoolerSlot { get; set; }
            private GraphicsCardSlotDefinition GraphicsCardSlot { get; set; }

            public static GraphicsCardFixture Create()
            {
                GraphicsCardFixture fixture = CreateUnclaimed();
                fixture.Authority = fixture.TryCreateAuthority().Value;
                return fixture;
            }

            public static GraphicsCardFixture CreateUnclaimed()
            {
                var fixture = new GraphicsCardFixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.graphics-card"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.graphics-card"),
                    MotherboardSlotId = Slot("slot.motherboard-main"),
                    MotherboardFastenerId = Fastener("fastener.motherboard-main"),
                    ProcessorSlotId = Slot("slot.processor-main"),
                    ProcessorRetentionId = Retention("retention.processor-main"),
                    MemorySlotId = Slot("slot.memory-a2"),
                    MemoryRetentionId = Retention("retention.memory-a2"),
                    StorageSlotId = Slot("slot.m2-primary"),
                    StorageRetentionId = Retention("retention.m2-primary"),
                    CoolerSlotId = Slot("slot.cooler-main"),
                    GraphicsCardSlotId = Slot("slot.graphics-card-x16"),
                    GraphicsCardLatchId =
                        StableId<AssemblyGraphicsCardLatchIdScope>.Parse(
                            "latch.graphics-card-x16"),
                    GraphicsCardBracketFastenerId =
                        Fastener("fastener.graphics-card-bracket-01"),
                    HandsId = Container("container.actor-hands"),
                    WorkbenchId = Container("container.assembly-workbench"),
                    ProcessorSocketContainerId = Container("container.processor"),
                    MemorySlotContainerId = Container("container.memory-a2"),
                    StorageSlotContainerId = Container("container.m2-primary"),
                    CoolerSlotContainerId = Container("container.cooler-main"),
                    GraphicsCardSlotContainerId = Container("container.graphics-card"),
                    MotherboardItemId = Item("item.graphics-card-motherboard"),
                    ProcessorItemId = Item("item.graphics-card-processor"),
                    MemoryItemId = Item("item.graphics-card-memory"),
                    StorageItemId = Item("item.graphics-card-storage"),
                    CoolerItemId = Item("item.graphics-card-cooler"),
                    GraphicsCardItemId = Item("item.graphics-card-card"),
                    AttachId = OperationId("operation.graphics-card-attach"),
                    SecureId = OperationId("operation.graphics-card-secure")
                };

                StableId<ProductDefinitionIdScope> motherboardProduct =
                    Product("component.graphics-card-motherboard");
                StableId<ProductDefinitionIdScope> processorProduct =
                    Product("component.graphics-card-processor");
                StableId<ProductDefinitionIdScope> memoryProduct =
                    Product("component.graphics-card-memory");
                StableId<ProductDefinitionIdScope> storageProduct =
                    Product("component.graphics-card-storage");
                StableId<ProductDefinitionIdScope> coolerProduct =
                    Product("component.graphics-card-cooler");
                fixture.GraphicsCardProductId =
                    Product("component.graphics-card-card");
                ProductCatalog products = ProductCatalog.Create(new[]
                {
                    Definition(motherboardProduct),
                    Definition(processorProduct),
                    Definition(memoryProduct),
                    Definition(storageProduct),
                    Definition(coolerProduct),
                    Definition(fixture.GraphicsCardProductId)
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
                        fixture.GraphicsCardProductId,
                        GraphicsCardType.Pcie4X16FullHeightDualSlot).Value
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
                fixture.CoolerSlot = ProcessorCoolerSlotDefinition.Create(
                    fixture.CoolerSlotId,
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
                    fixture.GraphicsCardSlotId,
                    fixture.GraphicsCardSlotContainerId,
                    GraphicsCardRetentionTopology.Create(
                        fixture.GraphicsCardLatchId,
                        fixture.GraphicsCardBracketFastenerId).Value,
                    GraphicsCardType.Pcie4X16FullHeightDualSlot).Value;

                fixture.Inventory = InventoryAuthority.Create(products).Value;
                fixture.Register(fixture.HandsId, InventoryContainerKind.ActorHands, 8);
                fixture.Register(fixture.WorkbenchId, InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.ProcessorSocketContainerId, InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.MemorySlotContainerId, InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.StorageSlotContainerId, InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.CoolerSlotContainerId, InventoryContainerKind.Workbench, 1);
                fixture.Register(fixture.GraphicsCardSlotContainerId, InventoryContainerKind.Workbench, 1);
                fixture.Receive(fixture.MotherboardItemId, motherboardProduct);
                fixture.Receive(fixture.ProcessorItemId, processorProduct);
                fixture.Receive(fixture.MemoryItemId, memoryProduct);
                fixture.Receive(fixture.StorageItemId, storageProduct);
                fixture.Receive(fixture.CoolerItemId, coolerProduct);
                fixture.Receive(fixture.GraphicsCardItemId,
                    fixture.GraphicsCardProductId);
                return fixture;
            }

            public OperationResult<AssemblyBuildAuthority> TryCreateAuthority()
            {
                return AssemblyBuildAuthority
                    .CreateWithProcessorSocketMemoryStorageCoolerAndGraphicsCardSlots(
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
                        HandsId,
                        WorkbenchId,
                        ProcessorSocketContainerId,
                        MotherboardFormFactor.MicroAtx,
                        CpuSocketFamily.Lga1700);
            }

            public void PrepareMotherboardHost()
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
            }

            public void PrepareCanonicalPreGraphicsCardBuild()
            {
                PrepareMotherboardHost();
                StableId<AssemblyOperationIdScope> processorSeat =
                    OperationId("operation.graphics-card-processor-seat");
                Assert.That(Authority.SeatProcessor(
                    processorSeat,
                    ProcessorItemId,
                    ProcessorSlotId,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseProcessorRetention(
                    OperationId("operation.graphics-card-processor-retain"),
                    ProcessorItemId,
                    ProcessorSlotId,
                    ProcessorRetentionId,
                    processorSeat,
                    Authority.Revision).IsSuccess, Is.True);
                StableId<AssemblyOperationIdScope> memorySeat =
                    OperationId("operation.graphics-card-memory-seat");
                Assert.That(Authority.SeatMemoryModule(
                    memorySeat,
                    MemoryItemId,
                    MemorySlotId,
                    DimmKeyOrientation.NotchAligned,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseMemoryRetention(
                    OperationId("operation.graphics-card-memory-retain"),
                    MemoryItemId,
                    MemorySlotId,
                    MemoryRetentionId,
                    memorySeat,
                    Authority.Revision).IsSuccess, Is.True);
                StableId<AssemblyOperationIdScope> storageSeat =
                    OperationId("operation.graphics-card-storage-seat");
                Assert.That(Authority.SeatStorageDevice(
                    storageSeat,
                    StorageItemId,
                    StorageSlotId,
                    M2KeyOrientation.KeyAligned,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.SecureStorageDevice(
                    OperationId("operation.graphics-card-storage-retain"),
                    StorageItemId,
                    StorageSlotId,
                    StorageRetentionId,
                    storageSeat,
                    Authority.Revision).IsSuccess, Is.True);
                StableId<AssemblyOperationIdScope> coolerSeat =
                    OperationId("operation.graphics-card-cooler-seat");
                Assert.That(Authority.SeatProcessorCooler(
                    coolerSeat,
                    CoolerItemId,
                    CoolerSlotId,
                    ProcessorCoolerMountOrientation.Primary,
                    AttachId,
                    SecureId,
                    processorSeat,
                    OperationId("operation.graphics-card-processor-retain"),
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.RetainProcessorCooler(
                    OperationId("operation.graphics-card-cooler-retain"),
                    CoolerItemId,
                    CoolerSlotId,
                    CoolerSlot.BracketId,
                    coolerSeat,
                    Authority.Revision).IsSuccess, Is.True);
            }

            public StableId<AssemblyOperationIdScope> SeatGraphicsCard()
            {
                StableId<AssemblyOperationIdScope> operationId =
                    OperationId("operation.graphics-card-fixture-seat");
                Assert.That(Authority.SeatGraphicsCard(
                    operationId,
                    GraphicsCardItemId,
                    GraphicsCardSlotId,
                    GraphicsCardMountOrientation.Primary,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                return operationId;
            }

            public StableId<AssemblyOperationIdScope> RetainGraphicsCard(
                StableId<AssemblyOperationIdScope> seatId)
            {
                StableId<AssemblyOperationIdScope> operationId =
                    OperationId("operation.graphics-card-fixture-retain");
                Assert.That(Authority.RetainGraphicsCard(
                    operationId,
                    GraphicsCardItemId,
                    GraphicsCardSlotId,
                    GraphicsCardLatchId,
                    GraphicsCardBracketFastenerId,
                    seatId,
                    Authority.Revision).IsSuccess, Is.True);
                return operationId;
            }

            public void FillHandsToCapacity()
            {
                for (int index = 1; index <= 4; index++)
                {
                    Receive(
                        Item("item.graphics-card-filler-" + index),
                        GraphicsCardProductId);
                }

                Assert.That(Inventory.GetContainerQuantity(HandsId).Value,
                    Is.EqualTo(8));
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

            private static StableId<AssemblySlotIdScope> Slot(string value)
            {
                return StableId<AssemblySlotIdScope>.Parse(value);
            }

            private static StableId<AssemblyFastenerIdScope> Fastener(string value)
            {
                return StableId<AssemblyFastenerIdScope>.Parse(value);
            }

            private static StableId<AssemblyRetentionIdScope> Retention(string value)
            {
                return StableId<AssemblyRetentionIdScope>.Parse(value);
            }

            private static StableId<ContainerIdScope> Container(string value)
            {
                return StableId<ContainerIdScope>.Parse(value);
            }

            private static StableId<ItemInstanceIdScope> Item(string value)
            {
                return StableId<ItemInstanceIdScope>.Parse(value);
            }

            private static StableId<ProductDefinitionIdScope> Product(string value)
            {
                return StableId<ProductDefinitionIdScope>.Parse(value);
            }

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
