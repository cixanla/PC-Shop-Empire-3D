using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class AssemblyBuildAuthorityTests
    {
        [Test]
        public void CompatibilityEvaluatorAcceptsExactFormFactorAndRejectsMismatchOrMissingData()
        {
            Fixture fixture = Fixture.Create();
            fixture.Components.TryGet(fixture.ItemProductId, out PcComponentSpecification specification);

            AssemblyCompatibilityResult exact =
                AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    specification,
                    MotherboardFormFactor.MicroAtx);
            AssemblyCompatibilityResult mismatch =
                AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    specification,
                    MotherboardFormFactor.Atx);
            AssemblyCompatibilityResult missing =
                AssemblyCompatibilityEvaluator.EvaluateMotherboardSeat(
                    null,
                    MotherboardFormFactor.MicroAtx);

            Assert.That(exact.IsCompatible, Is.True);
            Assert.That(exact.Reason, Is.EqualTo(Failure.None));
            Assert.That(mismatch.IsCompatible, Is.False);
            Assert.That(mismatch.Reason,
                Is.EqualTo(AssemblyFailures.MotherboardFormFactorMismatch));
            Assert.That(missing.Reason,
                Is.EqualTo(AssemblyFailures.UnknownComponentSpecification));
        }

        [Test]
        public void AttachSeatsExactSerializedMotherboardAndMovesInventoryAtomically()
        {
            Fixture fixture = Fixture.Create();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> result =
                fixture.Authority.AttachMotherboard(
                    OperationId("operation.attach-001"),
                    fixture.ItemId,
                    fixture.SlotId);

            Assert.That(result.IsSuccess, Is.True);
            AssemblyOperationReceipt receipt = result.Value;
            Assert.That(receipt.OperationKind,
                Is.EqualTo(AssemblyOperationKind.AttachMotherboard));
            Assert.That(receipt.BuildId, Is.EqualTo(fixture.BuildId));
            Assert.That(receipt.ChassisId, Is.EqualTo(fixture.ChassisId));
            Assert.That(receipt.SlotId, Is.EqualTo(fixture.SlotId));
            Assert.That(receipt.ItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(receipt.ProductId, Is.EqualTo(fixture.ItemProductId));
            Assert.That(receipt.SourceContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(receipt.TargetContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(receipt.ResultingSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(receipt.AssemblyRevision, Is.EqualTo(1));
            Assert.That(receipt.InventoryRevision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(fixture.Authority.MotherboardItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord moved), Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ExactAttachReplayReturnsSameReceiptAndConflictDoesNotMutate()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> operationId = OperationId("operation.attach-replay");
            AssemblyOperationReceipt original = fixture.Authority.AttachMotherboard(
                operationId,
                fixture.ItemId,
                fixture.SlotId).Value;
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> replay =
                fixture.Authority.AttachMotherboard(
                    operationId,
                    fixture.ItemId,
                    fixture.SlotId);
            OperationResult<AssemblyOperationReceipt> conflict =
                fixture.Authority.AttachMotherboard(
                    operationId,
                    StableId<ItemInstanceIdScope>.Parse("item.conflicting"),
                    fixture.SlotId);

            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(replay.Value, Is.SameAs(original));
            Assert.That(conflict.Error, Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(1));
        }

        [Test]
        public void DetachReturnsSameSerializedMotherboardToHandsAndReplaysExactly()
        {
            Fixture fixture = Fixture.Create();
            fixture.Authority.AttachMotherboard(
                OperationId("operation.attach-before-detach"),
                fixture.ItemId,
                fixture.SlotId);
            StableId<AssemblyOperationIdScope> detachId = OperationId("operation.detach-001");

            AssemblyOperationReceipt detached = fixture.Authority.DetachMotherboard(
                detachId,
                fixture.ItemId,
                fixture.SlotId).Value;
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            AssemblyOperationReceipt replay = fixture.Authority.DetachMotherboard(
                detachId,
                fixture.ItemId,
                fixture.SlotId).Value;

            Assert.That(detached.OperationKind,
                Is.EqualTo(AssemblyOperationKind.DetachMotherboard));
            Assert.That(detached.ItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(detached.ProductId, Is.EqualTo(fixture.ItemProductId));
            Assert.That(detached.SourceContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(detached.TargetContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(detached.SourceAttachOperationId,
                Is.EqualTo(OperationId("operation.attach-before-detach")));
            Assert.That(detached.ResultingSeatState, Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(replay, Is.SameAs(detached));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(fixture.Authority.MotherboardItemId.IsEmpty, Is.True);
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord moved), Is.True);
            Assert.That(moved.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void WrongSlotFormFactorAndUnknownSpecificationFailWithoutMutation()
        {
            Fixture wrongSlot = Fixture.Create();
            AssertFailureWithoutMutation(
                wrongSlot,
                wrongSlot.Authority.AttachMotherboard(
                    OperationId("operation.wrong-slot"),
                    wrongSlot.ItemId,
                    StableId<AssemblySlotIdScope>.Parse("slot.wrong")),
                AssemblyFailures.UnknownSlot);

            Fixture mismatch = Fixture.Create(
                itemProductId: "component.motherboard-atx");
            AssertFailureWithoutMutation(
                mismatch,
                mismatch.Authority.AttachMotherboard(
                    OperationId("operation.form-factor-mismatch"),
                    mismatch.ItemId,
                    mismatch.SlotId),
                AssemblyFailures.MotherboardFormFactorMismatch);

            Fixture unknownSpecification = Fixture.Create(
                itemProductId: "component.motherboard-unclassified");
            AssertFailureWithoutMutation(
                unknownSpecification,
                unknownSpecification.Authority.AttachMotherboard(
                    OperationId("operation.unknown-specification"),
                    unknownSpecification.ItemId,
                    unknownSpecification.SlotId),
                AssemblyFailures.UnknownComponentSpecification);
        }

        [Test]
        public void OccupiedWorkbenchAndSeatFailuresAreFailClosed()
        {
            Fixture fullWorkbench = Fixture.Create(fillWorkbench: true);
            AssertFailureWithoutMutation(
                fullWorkbench,
                fullWorkbench.Authority.AttachMotherboard(
                    OperationId("operation.full-workbench"),
                    fullWorkbench.ItemId,
                    fullWorkbench.SlotId),
                AssemblyFailures.WorkbenchCapacityExceeded);

            Fixture occupiedSeat = Fixture.Create();
            occupiedSeat.Authority.AttachMotherboard(
                OperationId("operation.first-seat"),
                occupiedSeat.ItemId,
                occupiedSeat.SlotId);
            long assemblyRevision = occupiedSeat.Authority.Revision;
            long inventoryRevision = occupiedSeat.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> second =
                occupiedSeat.Authority.AttachMotherboard(
                    OperationId("operation.second-seat"),
                    occupiedSeat.ItemId,
                    occupiedSeat.SlotId);
            Assert.That(second.Error, Is.EqualTo(AssemblyFailures.SlotOccupied));
            Assert.That(occupiedSeat.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(occupiedSeat.Inventory.Revision, Is.EqualTo(inventoryRevision));

            Fixture emptySeat = Fixture.Create();
            long emptyInventoryRevision = emptySeat.Inventory.Revision;
            Assert.That(emptySeat.Authority.DetachMotherboard(
                    OperationId("operation.detach-empty"),
                    emptySeat.ItemId,
                    emptySeat.SlotId).Error,
                Is.EqualTo(AssemblyFailures.SlotEmpty));
            Assert.That(emptySeat.Authority.Revision, Is.Zero);
            Assert.That(emptySeat.Inventory.Revision, Is.EqualTo(emptyInventoryRevision));
        }

        [Test]
        public void BenchmarkRemainsBlockedBeforeAndAfterUnsecuredMotherboardSeat()
        {
            Fixture fixture = Fixture.Create();

            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.MotherboardMissing));
            fixture.Authority.AttachMotherboard(
                OperationId("operation.benchmark-gate"),
                fixture.ItemId,
                fixture.SlotId);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured));
        }

        [Test]
        public void ExternalInventoryDriftIsDetectedAndCannotDetachSilently()
        {
            Fixture fixture = Fixture.Create();
            fixture.Authority.AttachMotherboard(
                OperationId("operation.attach-before-drift"),
                fixture.ItemId,
                fixture.SlotId);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.ItemId,
                fixture.StorageId).IsSuccess, Is.True);
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;

            Assert.That(fixture.Authority.ValidateInvariants().Error,
                Is.EqualTo(AssemblyFailures.InvariantViolation));
            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-after-drift"),
                    fixture.ItemId,
                    fixture.SlotId).Error,
                Is.EqualTo(AssemblyFailures.ItemNotOnWorkbench));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
        }

        [Test]
        public void SnapshotAndReceiptLookupExposeStableReadOnlyProjection()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> operationId = OperationId("operation.snapshot");
            AssemblyOperationReceipt receipt = fixture.Authority.AttachMotherboard(
                operationId,
                fixture.ItemId,
                fixture.SlotId).Value;

            AssemblyBuildSnapshot snapshot = fixture.Authority.GetSnapshot();

            Assert.That(snapshot.BuildId, Is.EqualTo(fixture.BuildId));
            Assert.That(snapshot.ChassisId, Is.EqualTo(fixture.ChassisId));
            Assert.That(snapshot.MotherboardSlotId, Is.EqualTo(fixture.SlotId));
            Assert.That(snapshot.HandsContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(snapshot.WorkbenchContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(snapshot.SupportedMotherboardFormFactor,
                Is.EqualTo(MotherboardFormFactor.MicroAtx));
            Assert.That(snapshot.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(snapshot.MotherboardItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(snapshot.MotherboardProductId, Is.EqualTo(fixture.ItemProductId));
            Assert.That(snapshot.InstalledByOperationId, Is.EqualTo(operationId));
            Assert.That(snapshot.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.TryGetReceipt(operationId, out AssemblyOperationReceipt found),
                Is.True);
            Assert.That(found, Is.SameAs(receipt));
            Assert.That(fixture.Authority.GetReceipts(), Is.EqualTo(new[] { receipt }));
        }

        private static void AssertFailureWithoutMutation(
            Fixture fixture,
            OperationResult<AssemblyOperationReceipt> result,
            Failure expectedFailure)
        {
            Assert.That(result.Error, Is.EqualTo(expectedFailure));
            Assert.That(fixture.Authority.Revision, Is.Zero);
            Assert.That(fixture.Authority.ReceiptCount, Is.Zero);
            Assert.That(fixture.Authority.MotherboardSeatState, Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static StableId<AssemblyOperationIdScope> OperationId(string value)
        {
            return StableId<AssemblyOperationIdScope>.Parse(value);
        }

        private sealed class Fixture
        {
            private Fixture()
            {
            }

            public ProductCatalog Products { get; private set; }

            public PcComponentCatalog Components { get; private set; }

            public InventoryAuthority Inventory { get; private set; }

            public AssemblyBuildAuthority Authority { get; private set; }

            public StableId<PcBuildIdScope> BuildId { get; private set; }

            public StableId<ChassisIdScope> ChassisId { get; private set; }

            public StableId<AssemblySlotIdScope> SlotId { get; private set; }

            public StableId<ContainerIdScope> HandsId { get; private set; }

            public StableId<ContainerIdScope> WorkbenchId { get; private set; }

            public StableId<ContainerIdScope> StorageId { get; private set; }

            public StableId<ItemInstanceIdScope> ItemId { get; private set; }

            public StableId<ProductDefinitionIdScope> ItemProductId { get; private set; }

            public static Fixture Create(
                string itemProductId = "component.motherboard-matx",
                bool fillWorkbench = false)
            {
                var fixture = new Fixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.prototype-001"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.prototype-001"),
                    SlotId = StableId<AssemblySlotIdScope>.Parse("slot.motherboard-main"),
                    HandsId = StableId<ContainerIdScope>.Parse("container.actor-hands"),
                    WorkbenchId = StableId<ContainerIdScope>.Parse("container.assembly-workbench"),
                    StorageId = StableId<ContainerIdScope>.Parse("container.storage"),
                    ItemId = StableId<ItemInstanceIdScope>.Parse("item.motherboard-001"),
                    ItemProductId = StableId<ProductDefinitionIdScope>.Parse(itemProductId)
                };

                ProductDefinition microAtx = Definition(
                    "component.motherboard-matx",
                    "Micro ATX Motherboard");
                ProductDefinition atx = Definition(
                    "component.motherboard-atx",
                    "ATX Motherboard");
                ProductDefinition unclassified = Definition(
                    "component.motherboard-unclassified",
                    "Unclassified Motherboard");
                fixture.Products = ProductCatalog.Create(
                    new[] { microAtx, atx, unclassified }).Value;
                fixture.Components = PcComponentCatalog.Create(
                    fixture.Products,
                    new[]
                    {
                        PcComponentSpecification.Create(
                            fixture.Products,
                            microAtx.Id,
                            PcComponentKind.Motherboard,
                            MotherboardFormFactor.MicroAtx).Value,
                        PcComponentSpecification.Create(
                            fixture.Products,
                            atx.Id,
                            PcComponentKind.Motherboard,
                            MotherboardFormFactor.Atx).Value
                    }).Value;

                fixture.Inventory = InventoryAuthority.Create(fixture.Products).Value;
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.HandsId,
                    InventoryContainerKind.ActorHands,
                    2).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.WorkbenchId,
                    InventoryContainerKind.Workbench,
                    1).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.StorageId,
                    InventoryContainerKind.Storage,
                    4).Value);
                fixture.Inventory.ReceiveSerializedItem(
                    fixture.ItemId,
                    fixture.ItemProductId,
                    fixture.HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", 12_900).Value);
                if (fillWorkbench)
                {
                    fixture.Inventory.ReceiveSerializedItem(
                        StableId<ItemInstanceIdScope>.Parse("item.workbench-occupied"),
                        microAtx.Id,
                        fixture.WorkbenchId,
                        InventoryCondition.OpenBox,
                        InventoryUnitCost.Create("EUR", 9_900).Value);
                }

                fixture.Authority = AssemblyBuildAuthority.Create(
                    fixture.Components,
                    fixture.Inventory,
                    fixture.BuildId,
                    fixture.ChassisId,
                    fixture.SlotId,
                    fixture.HandsId,
                    fixture.WorkbenchId,
                    MotherboardFormFactor.MicroAtx).Value;
                return fixture;
            }

            private static ProductDefinition Definition(string id, string displayName)
            {
                return ProductDefinition.Create(
                    StableId<ProductDefinitionIdScope>.Parse(id),
                    StableId<ProductCategoryIdScope>.Parse("pc-components"),
                    displayName,
                    ProductTrackingPolicy.SerializedInstance,
                    730).Value;
            }
        }
    }
}
