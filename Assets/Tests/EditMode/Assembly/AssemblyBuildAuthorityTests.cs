using System.Reflection;
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
        public void OccupiedSeatAndEmptyDetachFailuresAreFailClosed()
        {
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
        public void CreateRejectsOccupiedWorkbenchWithoutClaimingInventory()
        {
            Fixture fixture = Fixture.CreateUnclaimed(fillWorkbench: true);
            StableId<ItemInstanceIdScope> occupiedItem =
                StableId<ItemInstanceIdScope>.Parse("item.workbench-occupied");
            long revision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> result = fixture.TryCreateAuthority();

            Assert.That(result.Error, Is.EqualTo(AssemblyFailures.SlotOccupied));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                occupiedItem, out InventoryItemRecord occupied), Is.True);
            Assert.That(occupied.ContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId, out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);

            Assert.That(fixture.Inventory.TransferSerializedItem(
                occupiedItem, fixture.StorageId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                occupiedItem, out InventoryItemRecord recovered), Is.True);
            Assert.That(recovered.ContainerId, Is.EqualTo(fixture.StorageId));
        }

        [Test]
        public void SecondAuthorityClaimingSameWorkbenchReturnsPlanForeignWithoutMutation()
        {
            Fixture fixture = Fixture.CreateUnclaimed();
            OperationResult<AssemblyBuildAuthority> first = fixture.TryCreateAuthority();
            Assert.That(first.IsSuccess, Is.True);
            long revision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> second = fixture.TryCreateAuthority();

            Assert.That(second.Error, Is.EqualTo(AssemblyFailures.PlanForeign));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId, out InventoryItemRecord held), Is.True);
            Assert.That(held.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(first.Value.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FullHandsDetachReturnsStableCapacityFailureWithoutMutation()
        {
            Fixture fixture = Fixture.Create();
            Assert.That(fixture.Authority.AttachMotherboard(
                OperationId("operation.attach-before-full-hands"),
                fixture.ItemId,
                fixture.SlotId).IsSuccess, Is.True);
            for (int index = 0; index < 2; index++)
            {
                Assert.That(fixture.Inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse($"item.hands-blocker-{index}"),
                    fixture.ItemProductId,
                    fixture.HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", 1).Value).IsSuccess, Is.True);
            }

            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            OperationResult<AssemblyOperationReceipt> result =
                fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-to-full-hands"),
                    fixture.ItemId,
                    fixture.SlotId);

            Assert.That(result.Error, Is.EqualTo(AssemblyFailures.HandsCapacityExceeded));
            Assert.That(result.Error.Code, Is.EqualTo("assembly.hands.capacity"));
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(fixture.Authority.MotherboardItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId, out InventoryItemRecord seated), Is.True);
            Assert.That(seated.ContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InventoryRevisionOverflowMapsToCanonicalRevisionOverflowWithoutMutation()
        {
            Fixture fixture = Fixture.Create();
            PropertyInfo revisionProperty = typeof(InventoryAuthority).GetProperty(
                nameof(InventoryAuthority.Revision));
            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                fixture.Inventory,
                new object[] { long.MaxValue });

            OperationResult<AssemblyOperationReceipt> result =
                fixture.Authority.AttachMotherboard(
                    OperationId("operation.inventory-revision-overflow"),
                    fixture.ItemId,
                    fixture.SlotId);

            Assert.That(result.Error, Is.EqualTo(AssemblyFailures.RevisionOverflow));
            Assert.That(result.Error.Code, Is.EqualTo("assembly.revision-overflow"));
            Assert.That(fixture.Authority.Revision, Is.Zero);
            Assert.That(fixture.Authority.ReceiptCount, Is.Zero);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
        }

        [Test]
        public void CreateAtInventoryRevisionMaxReturnsCanonicalOverflowAndDoesNotClaimWorkbench()
        {
            Fixture fixture = Fixture.CreateUnclaimed();
            PropertyInfo revisionProperty = typeof(InventoryAuthority).GetProperty(
                nameof(InventoryAuthority.Revision));
            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                fixture.Inventory,
                new object[] { long.MaxValue });

            OperationResult<AssemblyBuildAuthority> first = fixture.TryCreateAuthority();
            OperationResult<AssemblyBuildAuthority> retry = fixture.TryCreateAuthority();

            Assert.That(first.Error, Is.EqualTo(AssemblyFailures.RevisionOverflow));
            Assert.That(retry.Error, Is.EqualTo(AssemblyFailures.RevisionOverflow));
            Assert.That(first.Error.Code, Is.EqualTo("assembly.revision-overflow"));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void WrongDetachItemIdentityReturnsIdentityConflictWithoutMutation()
        {
            Fixture fixture = Fixture.Create();
            fixture.Authority.AttachMotherboard(
                OperationId("operation.attach-before-wrong-detach"),
                fixture.ItemId,
                fixture.SlotId);
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            OperationResult<AssemblyOperationReceipt> result =
                fixture.Authority.DetachMotherboard(
                    StableId<AssemblyOperationIdScope>.Parse(
                        "operation.wrong-detach-identity"),
                    StableId<ItemInstanceIdScope>.Parse("item.motherboard-foreign"),
                    fixture.SlotId);

            Assert.That(result.Error, Is.EqualTo(AssemblyFailures.IdentityConflict));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.MotherboardItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId, out InventoryItemRecord seated), Is.True);
            Assert.That(seated.ContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
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
        public void SecureAndUnsecurePreserveExactMotherboardAndInventory()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId =
                OperationId("operation.fastener-attach");
            fixture.Authority.AttachMotherboard(
                attachId,
                fixture.ItemId,
                fixture.SlotId);
            long inventoryRevision = fixture.Inventory.Revision;
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord before), Is.True);

            StableId<AssemblyOperationIdScope> secureId =
                OperationId("operation.fastener-secure");
            OperationResult<AssemblyOperationReceipt> secure =
                fixture.Authority.SecureMotherboardFastener(
                    secureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    1);
            OperationResult<AssemblyOperationReceipt> secureReplay =
                fixture.Authority.SecureMotherboardFastener(
                    secureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    1);

            Assert.That(secure.IsSuccess, Is.True);
            Assert.That(secureReplay.Value, Is.SameAs(secure.Value));
            Assert.That(secure.Value.OperationKind,
                Is.EqualTo(AssemblyOperationKind.SecureMotherboardFastener));
            Assert.That(secure.Value.FastenerId, Is.EqualTo(fixture.FastenerId));
            Assert.That(secure.Value.SequenceIndex, Is.Zero);
            Assert.That(secure.Value.ExpectedAssemblyRevision, Is.EqualTo(1));
            Assert.That(secure.Value.PreviousSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(secure.Value.ResultingSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(secure.Value.SourceContainerId.IsEmpty, Is.True);
            Assert.That(secure.Value.TargetContainerId.IsEmpty, Is.True);
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(fixture.Authority.SecuredByOperationId, Is.EqualTo(secureId));
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));

            StableId<AssemblyOperationIdScope> unsecureId =
                OperationId("operation.fastener-unsecure");
            OperationResult<AssemblyOperationReceipt> unsecure =
                fixture.Authority.UnsecureMotherboardFastener(
                    unsecureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    secureId,
                    2);
            OperationResult<AssemblyOperationReceipt> unsecureReplay =
                fixture.Authority.UnsecureMotherboardFastener(
                    unsecureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    secureId,
                    2);

            Assert.That(unsecure.IsSuccess, Is.True);
            Assert.That(unsecureReplay.Value, Is.SameAs(unsecure.Value));
            Assert.That(unsecure.Value.SourceAttachOperationId, Is.EqualTo(attachId));
            Assert.That(unsecure.Value.SourceSecureOperationId, Is.EqualTo(secureId));
            Assert.That(unsecure.Value.PreviousSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(unsecure.Value.ResultingSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(fixture.Authority.SecuredByOperationId.IsEmpty, Is.True);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord after), Is.True);
            Assert.That(after.Id, Is.EqualTo(before.Id));
            Assert.That(after.ProductId, Is.EqualTo(before.ProductId));
            Assert.That(after.ContainerId, Is.EqualTo(before.ContainerId));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(3));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(3));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void WrongFastenerIdentityAndStaleRevisionAreFailClosed()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId =
                OperationId("operation.fastener-failure-attach");
            fixture.Authority.AttachMotherboard(
                attachId,
                fixture.ItemId,
                fixture.SlotId);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> wrongFastener =
                fixture.Authority.SecureMotherboardFastener(
                    OperationId("operation.fastener-wrong-id"),
                    fixture.ItemId,
                    fixture.SlotId,
                    StableId<AssemblyFastenerIdScope>.Parse("fastener.foreign"),
                    attachId,
                    1);
            OperationResult<AssemblyOperationReceipt> stale =
                fixture.Authority.SecureMotherboardFastener(
                    OperationId("operation.fastener-stale"),
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    0);
            OperationResult<AssemblyOperationReceipt> wrongItem =
                fixture.Authority.SecureMotherboardFastener(
                    OperationId("operation.fastener-wrong-item"),
                    StableId<ItemInstanceIdScope>.Parse("item.foreign"),
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    1);

            Assert.That(wrongFastener.Error, Is.EqualTo(AssemblyFailures.InvalidFastener));
            Assert.That(stale.Error, Is.EqualTo(AssemblyFailures.PlanStale));
            Assert.That(wrongItem.Error, Is.EqualTo(AssemblyFailures.IdentityConflict));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void DelayedFastenerReplayReturnsHistoricalReceiptWithoutRollingStateBack()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId = OperationId("operation.delayed-attach");
            StableId<AssemblyOperationIdScope> secureId = OperationId("operation.delayed-secure");
            StableId<AssemblyOperationIdScope> unsecureId =
                OperationId("operation.delayed-unsecure");
            fixture.Authority.AttachMotherboard(attachId, fixture.ItemId, fixture.SlotId);
            AssemblyOperationReceipt secure = fixture.Authority.SecureMotherboardFastener(
                secureId,
                fixture.ItemId,
                fixture.SlotId,
                fixture.FastenerId,
                attachId,
                1).Value;
            fixture.Authority.UnsecureMotherboardFastener(
                unsecureId,
                fixture.ItemId,
                fixture.SlotId,
                fixture.FastenerId,
                attachId,
                secureId,
                2);
            fixture.Authority.DetachMotherboard(
                OperationId("operation.delayed-detach"),
                fixture.ItemId,
                fixture.SlotId);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> replay =
                fixture.Authority.SecureMotherboardFastener(
                    secureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    1);
            OperationResult<AssemblyOperationReceipt> crossKindConflict =
                fixture.Authority.UnsecureMotherboardFastener(
                    secureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    secureId,
                    2);

            Assert.That(replay.Value, Is.SameAs(secure));
            Assert.That(crossKindConflict.Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(4));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(4));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FastenerOrderLineageSlotAndSameKindConflictsAreFailClosed()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId =
                OperationId("operation.fastener-order-attach");
            StableId<AssemblyOperationIdScope> secureId =
                OperationId("operation.fastener-order-secure");
            StableId<AssemblyOperationIdScope> foreignSecureId =
                OperationId("operation.fastener-order-foreign-secure");
            fixture.Authority.AttachMotherboard(
                attachId,
                fixture.ItemId,
                fixture.SlotId);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> unsecureBeforeSecure =
                fixture.Authority.UnsecureMotherboardFastener(
                    OperationId("operation.fastener-order-early-unsecure"),
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    foreignSecureId,
                    1);
            OperationResult<AssemblyOperationReceipt> secure =
                fixture.Authority.SecureMotherboardFastener(
                    secureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    1);
            OperationResult<AssemblyOperationReceipt> secondSecure =
                fixture.Authority.SecureMotherboardFastener(
                    OperationId("operation.fastener-order-second-secure"),
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    2);
            OperationResult<AssemblyOperationReceipt> wrongLineage =
                fixture.Authority.UnsecureMotherboardFastener(
                    OperationId("operation.fastener-order-wrong-lineage"),
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    foreignSecureId,
                    2);
            OperationResult<AssemblyOperationReceipt> wrongSlot =
                fixture.Authority.UnsecureMotherboardFastener(
                    OperationId("operation.fastener-order-wrong-slot"),
                    fixture.ItemId,
                    StableId<AssemblySlotIdScope>.Parse("slot.foreign"),
                    fixture.FastenerId,
                    attachId,
                    secureId,
                    2);
            OperationResult<AssemblyOperationReceipt> sameKindConflict =
                fixture.Authority.SecureMotherboardFastener(
                    secureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    2);

            Assert.That(unsecureBeforeSecure.Error,
                Is.EqualTo(AssemblyFailures.FastenerOutOfOrder));
            Assert.That(secure.IsSuccess, Is.True);
            Assert.That(secondSecure.Error,
                Is.EqualTo(AssemblyFailures.FastenerOutOfOrder));
            Assert.That(wrongLineage.Error,
                Is.EqualTo(AssemblyFailures.FastenerOutOfOrder));
            Assert.That(wrongSlot.Error, Is.EqualTo(AssemblyFailures.UnknownSlot));
            Assert.That(sameKindConflict.Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedSecured));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(2));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(2));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReceiptHistoryFoldRejectsHistoricalSecureLineageCorruption()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId =
                OperationId("operation.history-fold-attach");
            StableId<AssemblyOperationIdScope> secureOneId =
                OperationId("operation.history-fold-secure-one");
            StableId<AssemblyOperationIdScope> secureTwoId =
                OperationId("operation.history-fold-secure-two");
            fixture.Authority.AttachMotherboard(attachId, fixture.ItemId, fixture.SlotId);
            fixture.Authority.SecureMotherboardFastener(
                secureOneId,
                fixture.ItemId,
                fixture.SlotId,
                fixture.FastenerId,
                attachId,
                1);
            fixture.Authority.UnsecureMotherboardFastener(
                OperationId("operation.history-fold-unsecure-one"),
                fixture.ItemId,
                fixture.SlotId,
                fixture.FastenerId,
                attachId,
                secureOneId,
                2);
            fixture.Authority.SecureMotherboardFastener(
                secureTwoId,
                fixture.ItemId,
                fixture.SlotId,
                fixture.FastenerId,
                attachId,
                3);
            AssemblyOperationReceipt unsecureTwo = fixture.Authority
                .UnsecureMotherboardFastener(
                    OperationId("operation.history-fold-unsecure-two"),
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    secureTwoId,
                    4).Value;
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);

            FieldInfo sourceSecureField = typeof(AssemblyOperationReceipt).GetField(
                "<SourceSecureOperationId>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(sourceSecureField, Is.Not.Null);
            sourceSecureField.SetValue(unsecureTwo, secureOneId);

            Assert.That(fixture.Authority.ValidateInvariants().Error,
                Is.EqualTo(AssemblyFailures.InvariantViolation));
        }

        [Test]
        public void ReceiptHistoryFoldRejectsInventoryRevisionRegressionAndTransferWithoutAdvance()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId =
                OperationId("operation.history-inventory-attach");
            StableId<AssemblyOperationIdScope> secureId =
                OperationId("operation.history-inventory-secure");
            AssemblyOperationReceipt attach = fixture.Authority.AttachMotherboard(
                attachId,
                fixture.ItemId,
                fixture.SlotId).Value;
            AssemblyOperationReceipt secure = fixture.Authority
                .SecureMotherboardFastener(
                    secureId,
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    1).Value;
            FieldInfo inventoryRevisionField = typeof(AssemblyOperationReceipt).GetField(
                "<InventoryRevision>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(inventoryRevisionField, Is.Not.Null);
            Assert.That(attach.InventoryRevision, Is.GreaterThan(1));

            long secureInventoryRevision = secure.InventoryRevision;
            inventoryRevisionField.SetValue(secure, attach.InventoryRevision - 1L);
            Assert.That(fixture.Authority.ValidateInvariants().Error,
                Is.EqualTo(AssemblyFailures.InvariantViolation));

            inventoryRevisionField.SetValue(secure, secureInventoryRevision);
            AssemblyOperationReceipt unsecure = fixture.Authority
                .UnsecureMotherboardFastener(
                    OperationId("operation.history-inventory-unsecure"),
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    secureId,
                    2).Value;
            AssemblyOperationReceipt detach = fixture.Authority.DetachMotherboard(
                OperationId("operation.history-inventory-detach"),
                fixture.ItemId,
                fixture.SlotId).Value;
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);

            inventoryRevisionField.SetValue(detach, unsecure.InventoryRevision);
            Assert.That(fixture.Authority.ValidateInvariants().Error,
                Is.EqualTo(AssemblyFailures.InvariantViolation));
        }

        [Test]
        public void SecuredMotherboardCannotDetachUntilExactUnsecureSucceeds()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId = OperationId("operation.detach-gate-attach");
            StableId<AssemblyOperationIdScope> secureId = OperationId("operation.detach-gate-secure");
            fixture.Authority.AttachMotherboard(attachId, fixture.ItemId, fixture.SlotId);
            fixture.Authority.SecureMotherboardFastener(
                secureId,
                fixture.ItemId,
                fixture.SlotId,
                fixture.FastenerId,
                attachId,
                1);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> blocked =
                fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-while-secured"),
                    fixture.ItemId,
                    fixture.SlotId);

            Assert.That(blocked.Error, Is.EqualTo(AssemblyFailures.ComponentSecured));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(2));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(2));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));

            Assert.That(fixture.Authority.UnsecureMotherboardFastener(
                OperationId("operation.detach-gate-unsecure"),
                fixture.ItemId,
                fixture.SlotId,
                fixture.FastenerId,
                attachId,
                secureId,
                2).IsSuccess, Is.True);
            Assert.That(fixture.Authority.DetachMotherboard(
                OperationId("operation.detach-after-unsecure"),
                fixture.ItemId,
                fixture.SlotId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.Empty));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FastenerRevisionOverflowLeavesAuthoritiesUnchanged()
        {
            Fixture fixture = Fixture.Create();
            StableId<AssemblyOperationIdScope> attachId =
                OperationId("operation.fastener-overflow-attach");
            AssemblyOperationReceipt attach = fixture.Authority.AttachMotherboard(
                attachId,
                fixture.ItemId,
                fixture.SlotId).Value;
            PropertyInfo revisionProperty = typeof(AssemblyBuildAuthority).GetProperty(
                nameof(AssemblyBuildAuthority.Revision));
            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                fixture.Authority,
                new object[] { long.MaxValue });
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> result =
                fixture.Authority.SecureMotherboardFastener(
                    OperationId("operation.fastener-overflow"),
                    fixture.ItemId,
                    fixture.SlotId,
                    fixture.FastenerId,
                    attachId,
                    long.MaxValue);

            Assert.That(result.Error, Is.EqualTo(AssemblyFailures.RevisionOverflow));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(fixture.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(fixture.Authority.MotherboardItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(fixture.Authority.MotherboardProductId,
                Is.EqualTo(fixture.ItemProductId));
            Assert.That(fixture.Authority.InstalledByOperationId, Is.EqualTo(attachId));
            Assert.That(fixture.Authority.SecuredByOperationId.IsEmpty, Is.True);
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(1));
            Assert.That(fixture.Authority.GetReceipts(), Is.EqualTo(new[] { attach }));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
        }

        [Test]
        public void ManagedWorkbenchRejectsRawTransferAndExactDetachStillSucceeds()
        {
            Fixture fixture = Fixture.Create();
            fixture.Authority.AttachMotherboard(
                OperationId("operation.attach-before-custody-check"),
                fixture.ItemId,
                fixture.SlotId);
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            OperationResult rawTransfer = fixture.Inventory.TransferSerializedItem(
                fixture.ItemId,
                fixture.StorageId);

            Assert.That(rawTransfer.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord seated), Is.True);
            Assert.That(seated.ContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));

            OperationResult<AssemblyOperationReceipt> detach =
                fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-after-custody-check"),
                    fixture.ItemId,
                    fixture.SlotId);

            Assert.That(detach.IsSuccess, Is.True);
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision + 1));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount + 1));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord detached), Is.True);
            Assert.That(detached.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ManagedWorkbenchRejectsRawTransferBeforeAttachWithoutMutation()
        {
            Fixture fixture = Fixture.Create();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult result = fixture.Inventory.TransferSerializedItem(
                fixture.ItemId,
                fixture.WorkbenchId);

            Assert.That(result.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.Revision, Is.Zero);
            Assert.That(fixture.Authority.ReceiptCount, Is.Zero);
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReservedMotherboardCannotAttachUntilReservationIsReleased()
        {
            Fixture fixture = Fixture.Create();
            StableId<ReservationIdScope> reservationId =
                StableId<ReservationIdScope>.Parse("reservation.motherboard-seat");
            StableId<InventoryClaimIdScope> claimId =
                StableId<InventoryClaimIdScope>.Parse("claim.motherboard-seat");
            Assert.That(fixture.Inventory.ReserveSerializedItem(
                reservationId,
                claimId,
                fixture.ItemId).IsSuccess, Is.True);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> blocked =
                fixture.Authority.AttachMotherboard(
                    OperationId("operation.attach-reserved"),
                    fixture.ItemId,
                    fixture.SlotId);

            Assert.That(blocked.Error, Is.EqualTo(AssemblyFailures.ItemNotInActorHands));
            Assert.That(fixture.Authority.Revision, Is.Zero);
            Assert.That(fixture.Authority.ReceiptCount, Is.Zero);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Inventory.ReservationCount, Is.EqualTo(1));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ItemId,
                out InventoryItemRecord reserved), Is.True);
            Assert.That(reserved.ContainerId, Is.EqualTo(fixture.HandsId));

            Assert.That(fixture.Inventory.ReleaseReservation(reservationId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.AttachMotherboard(
                OperationId("operation.attach-after-release"),
                fixture.ItemId,
                fixture.SlotId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void SeatedMotherboardCannotBeReservedWithoutMutation()
        {
            Fixture fixture = Fixture.Create();
            fixture.Authority.AttachMotherboard(
                OperationId("operation.attach-before-reservation"),
                fixture.ItemId,
                fixture.SlotId);
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            OperationResult reservation = fixture.Inventory.ReserveSerializedItem(
                StableId<ReservationIdScope>.Parse("reservation.seated-motherboard"),
                StableId<InventoryClaimIdScope>.Parse("claim.seated-motherboard"),
                fixture.ItemId);

            Assert.That(reservation.Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Inventory.ReservationCount, Is.Zero);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CreateRejectsValueEqualForeignCatalogAuthority()
        {
            Fixture fixture = Fixture.Create();
            ProductCatalog valueEqualForeign = ProductCatalog.Create(
                fixture.Products.Definitions).Value;
            InventoryAuthority foreignInventory = InventoryAuthority.Create(
                valueEqualForeign).Value;
            foreignInventory.RegisterContainer(InventoryContainerDefinition.Create(
                fixture.HandsId,
                InventoryContainerKind.ActorHands,
                2).Value);
            foreignInventory.RegisterContainer(InventoryContainerDefinition.Create(
                fixture.WorkbenchId,
                InventoryContainerKind.Workbench,
                1).Value);

            OperationResult<AssemblyBuildAuthority> result = AssemblyBuildAuthority.Create(
                fixture.Components,
                foreignInventory,
                StableId<PcBuildIdScope>.Parse("build.foreign-catalog"),
                StableId<ChassisIdScope>.Parse("chassis.foreign-catalog"),
                fixture.SlotId,
                fixture.FastenerId,
                fixture.HandsId,
                fixture.WorkbenchId,
                MotherboardFormFactor.MicroAtx);

            Assert.That(result.Error, Is.EqualTo(AssemblyFailures.CatalogAuthorityMismatch));
            Assert.That(foreignInventory.Revision, Is.EqualTo(2));
        }

        [Test]
        public void PublicFailureCodesMatchIssue53AssemblyContract()
        {
            Assert.That(AssemblyFailures.InvalidBuildId.Code, Is.EqualTo("assembly.invalid-build"));
            Assert.That(AssemblyFailures.InvalidChassisId.Code, Is.EqualTo("assembly.invalid-chassis"));
            Assert.That(AssemblyFailures.InvalidSlotId.Code, Is.EqualTo("assembly.invalid-slot"));
            Assert.That(AssemblyFailures.InvalidFastener.Code,
                Is.EqualTo("assembly.invalid-fastener"));
            Assert.That(AssemblyFailures.InvalidComponent.Code, Is.EqualTo("assembly.invalid-component"));
            Assert.That(AssemblyFailures.ComponentKindMismatch.Code,
                Is.EqualTo("assembly.component-kind-mismatch"));
            Assert.That(AssemblyFailures.FormFactorMismatch.Code,
                Is.EqualTo("assembly.form-factor-mismatch"));
            Assert.That(AssemblyFailures.SlotOccupied.Code, Is.EqualTo("assembly.slot-occupied"));
            Assert.That(AssemblyFailures.ComponentNotInActorHands.Code,
                Is.EqualTo("assembly.component-not-in-hands"));
            Assert.That(AssemblyFailures.ComponentNotSeated.Code,
                Is.EqualTo("assembly.component-not-seated"));
            Assert.That(AssemblyFailures.ComponentSecured.Code,
                Is.EqualTo("assembly.component-secured"));
            Assert.That(AssemblyFailures.FastenerOutOfOrder.Code,
                Is.EqualTo("assembly.fastener-out-of-order"));
            Assert.That(AssemblyFailures.IdentityConflict.Code,
                Is.EqualTo("assembly.identity-conflict"));
            Assert.That(AssemblyFailures.PlanForeign.Code, Is.EqualTo("assembly.plan-foreign"));
            Assert.That(AssemblyFailures.PlanStale.Code, Is.EqualTo("assembly.plan-stale"));
            Assert.That(AssemblyFailures.RevisionOverflow.Code,
                Is.EqualTo("assembly.revision-overflow"));
            Assert.That(AssemblyFailures.HandsCapacityExceeded.Code,
                Is.EqualTo("assembly.hands.capacity"));
            Assert.That(AssemblyFailures.InventoryRevisionOverflow,
                Is.EqualTo(AssemblyFailures.RevisionOverflow));
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
            Assert.That(snapshot.MotherboardFastenerId, Is.EqualTo(fixture.FastenerId));
            Assert.That(snapshot.HandsContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(snapshot.WorkbenchContainerId, Is.EqualTo(fixture.WorkbenchId));
            Assert.That(snapshot.SupportedMotherboardFormFactor,
                Is.EqualTo(MotherboardFormFactor.MicroAtx));
            Assert.That(snapshot.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(snapshot.MotherboardItemId, Is.EqualTo(fixture.ItemId));
            Assert.That(snapshot.MotherboardProductId, Is.EqualTo(fixture.ItemProductId));
            Assert.That(snapshot.InstalledByOperationId, Is.EqualTo(operationId));
            Assert.That(snapshot.SecuredByOperationId.IsEmpty, Is.True);
            Assert.That(snapshot.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.TryGetReceipt(operationId, out AssemblyOperationReceipt found),
                Is.True);
            Assert.That(found, Is.SameAs(receipt));
            Assert.That(fixture.Authority.GetReceipts(), Is.EqualTo(new[] { receipt }));
        }

        [Test]
        public void ProcessorSeatRetentionAndRemovalPreserveExactCustodyAndReplay()
        {
            ProcessorFixture fixture = ProcessorFixture.Create();
            fixture.AttachAndSecureMotherboard();
            long inventoryBeforeSeat = fixture.Inventory.Revision;
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.processor-seat");

            OperationResult<AssemblyOperationReceipt> seated =
                fixture.Authority.SeatProcessor(
                    seatId,
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    fixture.AttachId,
                    fixture.SecureId,
                    2);
            OperationResult<AssemblyOperationReceipt> seatReplay =
                fixture.Authority.SeatProcessor(
                    seatId,
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    fixture.AttachId,
                    fixture.SecureId,
                    2);

            Assert.That(seated.IsSuccess, Is.True);
            Assert.That(seatReplay.Value, Is.SameAs(seated.Value));
            Assert.That(seated.Value.OperationKind,
                Is.EqualTo(AssemblyOperationKind.SeatProcessor));
            Assert.That(seated.Value.PreviousProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(seated.Value.ResultingProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            Assert.That(seated.Value.SourceContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(seated.Value.TargetContainerId,
                Is.EqualTo(fixture.ProcessorSocketContainerId));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 1));
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.ProcessorUnretained));

            long inventoryBeforeRetention = fixture.Inventory.Revision;
            StableId<AssemblyOperationIdScope> retainId =
                OperationId("operation.processor-retain");
            AssemblyOperationReceipt retained =
                fixture.Authority.CloseProcessorRetention(
                    retainId,
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    fixture.RetentionId,
                    seatId,
                    3).Value;
            AssemblyOperationReceipt retainedReplay =
                fixture.Authority.CloseProcessorRetention(
                    retainId,
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    fixture.RetentionId,
                    seatId,
                    3).Value;

            Assert.That(retainedReplay, Is.SameAs(retained));
            Assert.That(retained.ResultingProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorRetained));
            Assert.That(retained.RetentionId, Is.EqualTo(fixture.RetentionId));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeRetention));
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));

            StableId<AssemblyOperationIdScope> openId =
                OperationId("operation.processor-open");
            Assert.That(fixture.Authority.OpenProcessorRetention(
                openId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                retainId,
                4).IsSuccess, Is.True);
            long inventoryBeforeRemove = fixture.Inventory.Revision;
            StableId<AssemblyOperationIdScope> removeId =
                OperationId("operation.processor-remove");
            AssemblyOperationReceipt removed = fixture.Authority.RemoveProcessor(
                removeId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                seatId,
                5).Value;
            AssemblyOperationReceipt removeReplay = fixture.Authority.RemoveProcessor(
                removeId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                seatId,
                5).Value;

            Assert.That(removeReplay, Is.SameAs(removed));
            Assert.That(removed.ResultingProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(6));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(6));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeRemove + 1));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ProcessorItemId,
                out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorMismatchAndStaleLineageFailWithoutMutation()
        {
            ProcessorFixture mismatch = ProcessorFixture.Create(
                "component.processor-am5");
            mismatch.AttachAndSecureMotherboard();
            long mismatchInventoryRevision = mismatch.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> incompatible =
                mismatch.Authority.SeatProcessor(
                    OperationId("operation.processor-mismatch"),
                    mismatch.ProcessorItemId,
                    mismatch.ProcessorSlotId,
                    mismatch.AttachId,
                    mismatch.SecureId,
                    2);

            Assert.That(incompatible.Error,
                Is.EqualTo(AssemblyFailures.CpuSocketFamilyMismatch));
            Assert.That(mismatch.Authority.Revision, Is.EqualTo(2));
            Assert.That(mismatch.Authority.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(mismatch.Inventory.Revision,
                Is.EqualTo(mismatchInventoryRevision));

            ProcessorFixture stale = ProcessorFixture.Create();
            stale.AttachAndSecureMotherboard();
            long staleInventoryRevision = stale.Inventory.Revision;
            OperationResult<AssemblyOperationReceipt> staleResult =
                stale.Authority.SeatProcessor(
                    OperationId("operation.processor-stale"),
                    stale.ProcessorItemId,
                    stale.ProcessorSlotId,
                    stale.AttachId,
                    stale.SecureId,
                    1);

            Assert.That(staleResult.Error, Is.EqualTo(AssemblyFailures.PlanStale));
            Assert.That(stale.Authority.Revision, Is.EqualTo(2));
            Assert.That(stale.Inventory.Revision, Is.EqualTo(staleInventoryRevision));
            Assert.That(stale.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InstalledProcessorBlocksMotherboardDetachUntilOpenedAndRemoved()
        {
            ProcessorFixture fixture = ProcessorFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.detach-gate-seat");
            StableId<AssemblyOperationIdScope> retainId =
                OperationId("operation.detach-gate-retain");
            fixture.Authority.SeatProcessor(
                seatId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.AttachId,
                fixture.SecureId,
                2);
            fixture.Authority.CloseProcessorRetention(
                retainId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                3);
            long securedAssemblyRevision = fixture.Authority.Revision;
            long securedInventoryRevision = fixture.Inventory.Revision;
            int securedReceiptCount = fixture.Authority.ReceiptCount;

            OperationResult<AssemblyOperationReceipt> securedBlocked =
                fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-gate-secured-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId);

            Assert.That(securedBlocked.Error,
                Is.EqualTo(AssemblyFailures.ProcessorInstalled));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(securedAssemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(securedInventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(securedReceiptCount));
            fixture.Authority.UnsecureMotherboardFastener(
                OperationId("operation.detach-gate-unsecure"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId,
                fixture.FastenerId,
                fixture.AttachId,
                fixture.SecureId,
                4);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> blocked =
                fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-gate-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId);

            Assert.That(blocked.Error, Is.EqualTo(AssemblyFailures.ProcessorInstalled));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.OpenProcessorRetention(
                OperationId("operation.detach-gate-open"),
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                retainId,
                5).IsSuccess, Is.True);
            Assert.That(fixture.Authority.RemoveProcessor(
                OperationId("operation.detach-gate-remove"),
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                seatId,
                6).IsSuccess, Is.True);
            Assert.That(fixture.Authority.DetachMotherboard(
                OperationId("operation.detach-gate-detach"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorSeatReportsHostStateBeforeLineageWithoutMutation()
        {
            ProcessorFixture fixture = ProcessorFixture.Create();
            long initialInventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> missing =
                fixture.Authority.SeatProcessor(
                    OperationId("operation.processor-host-missing"),
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    default,
                    default,
                    0);

            Assert.That(missing.Error, Is.EqualTo(AssemblyFailures.MotherboardMissing));
            Assert.That(fixture.Authority.Revision, Is.Zero);
            Assert.That(fixture.Authority.ReceiptCount, Is.Zero);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(initialInventoryRevision));

            Assert.That(fixture.Authority.AttachMotherboard(
                fixture.AttachId,
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);
            long attachedInventoryRevision = fixture.Inventory.Revision;
            int attachedReceiptCount = fixture.Authority.ReceiptCount;

            OperationResult<AssemblyOperationReceipt> unsecured =
                fixture.Authority.SeatProcessor(
                    OperationId("operation.processor-host-unsecured"),
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    fixture.AttachId,
                    default,
                    1);

            Assert.That(unsecured.Error,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(attachedReceiptCount));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(attachedInventoryRevision));
            Assert.That(fixture.Authority.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorSnapshotExposesStableSocketRetentionAndBenchmarkGates()
        {
            ProcessorFixture fixture = ProcessorFixture.Create();
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.MotherboardMissing));
            fixture.AttachAndSecureMotherboard();
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.ProcessorMissing));

            AssemblyBuildSnapshot snapshot = fixture.Authority.GetSnapshot();

            Assert.That(snapshot.HasProcessorSocket, Is.True);
            Assert.That(snapshot.ProcessorSlotId, Is.EqualTo(fixture.ProcessorSlotId));
            Assert.That(snapshot.ProcessorRetentionId, Is.EqualTo(fixture.RetentionId));
            Assert.That(snapshot.ProcessorSocketContainerId,
                Is.EqualTo(fixture.ProcessorSocketContainerId));
            Assert.That(snapshot.SupportedCpuSocketFamily,
                Is.EqualTo(CpuSocketFamily.Lga1700));
            Assert.That(snapshot.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void OccupiedProcessorSocketFailsAtomicPairClaimWithoutManagingWorkbench()
        {
            ProcessorFixture fixture = ProcessorFixture.CreateUnclaimed(
                fillProcessorSocket: true);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> result =
                fixture.TryCreateAuthority();

            Assert.That(result.Error,
                Is.EqualTo(AssemblyFailures.ProcessorSocketOccupied));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.MotherboardItemId,
                fixture.WorkbenchId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorSocketCapacityMustBeExactlyOneAndPairClaimFailureIsAtomic()
        {
            ProcessorFixture fixture = ProcessorFixture.CreateUnclaimed(
                processorSocketCapacity: 2);
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> result =
                fixture.TryCreateAuthority();

            Assert.That(result.Error,
                Is.EqualTo(AssemblyFailures.InvalidProcessorSocketContainer));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.MotherboardItemId,
                fixture.WorkbenchId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.ProcessorItemId,
                fixture.ProcessorSocketContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.GetContainerQuantity(
                fixture.ProcessorSocketContainerId).Value, Is.EqualTo(1));
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorFullCycleDelayedReplayAndConflictsNeverRewriteFinalCustody()
        {
            ProcessorFixture fixture = ProcessorFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.processor-delayed-seat");
            StableId<AssemblyOperationIdScope> closeId =
                OperationId("operation.processor-delayed-close");
            StableId<AssemblyOperationIdScope> openId =
                OperationId("operation.processor-delayed-open");
            StableId<AssemblyOperationIdScope> removeId =
                OperationId("operation.processor-delayed-remove");

            AssemblyOperationReceipt seat = fixture.Authority.SeatProcessor(
                seatId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.AttachId,
                fixture.SecureId,
                2).Value;
            AssemblyOperationReceipt close = fixture.Authority.CloseProcessorRetention(
                closeId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                3).Value;
            AssemblyOperationReceipt open = fixture.Authority.OpenProcessorRetention(
                openId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                closeId,
                4).Value;
            AssemblyOperationReceipt remove = fixture.Authority.RemoveProcessor(
                removeId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                seatId,
                5).Value;
            long finalAssemblyRevision = fixture.Authority.Revision;
            long finalInventoryRevision = fixture.Inventory.Revision;
            int finalReceiptCount = fixture.Authority.ReceiptCount;

            Assert.That(fixture.Authority.SeatProcessor(
                seatId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.AttachId,
                fixture.SecureId,
                2).Value, Is.SameAs(seat));
            Assert.That(fixture.Authority.CloseProcessorRetention(
                closeId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                3).Value, Is.SameAs(close));
            Assert.That(fixture.Authority.OpenProcessorRetention(
                openId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                closeId,
                4).Value, Is.SameAs(open));
            Assert.That(fixture.Authority.RemoveProcessor(
                removeId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                seatId,
                5).Value, Is.SameAs(remove));

            Assert.That(fixture.Authority.SeatProcessor(
                seatId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.AttachId,
                default,
                2).Error, Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.CloseProcessorRetention(
                seatId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.RetentionId,
                seatId,
                3).Error, Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(finalAssemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(finalInventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(finalReceiptCount));
            Assert.That(fixture.Authority.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.EmptyOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ProcessorItemId,
                out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorRemoveIntoFullHandsFailsWithoutMutation()
        {
            ProcessorFixture fixture = ProcessorFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.processor-full-hands-seat");
            Assert.That(fixture.Authority.SeatProcessor(
                seatId,
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.AttachId,
                fixture.SecureId,
                2).IsSuccess, Is.True);
            for (int index = 0; index < 3; index++)
            {
                Assert.That(fixture.Inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse($"item.hands-blocker-{index}"),
                    fixture.ProcessorProductId,
                    fixture.HandsId,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 10_000 + index).Value).IsSuccess,
                    Is.True);
            }

            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;
            OperationResult<AssemblyOperationReceipt> blocked =
                fixture.Authority.RemoveProcessor(
                    OperationId("operation.processor-full-hands-remove"),
                    fixture.ProcessorItemId,
                    fixture.ProcessorSlotId,
                    seatId,
                    3);

            Assert.That(blocked.Error, Is.EqualTo(AssemblyFailures.HandsCapacityExceeded));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.ProcessorSocketState,
                Is.EqualTo(ProcessorSocketState.ProcessorSeatedOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.ProcessorItemId,
                out InventoryItemRecord seated), Is.True);
            Assert.That(seated.ContainerId,
                Is.EqualTo(fixture.ProcessorSocketContainerId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
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

            public StableId<AssemblyFastenerIdScope> FastenerId { get; private set; }

            public StableId<ContainerIdScope> HandsId { get; private set; }

            public StableId<ContainerIdScope> WorkbenchId { get; private set; }

            public StableId<ContainerIdScope> StorageId { get; private set; }

            public StableId<ItemInstanceIdScope> ItemId { get; private set; }

            public StableId<ProductDefinitionIdScope> ItemProductId { get; private set; }

            public static Fixture Create(string itemProductId = "component.motherboard-matx")
            {
                Fixture fixture = CreateUnclaimed(itemProductId);
                fixture.Authority = fixture.TryCreateAuthority().Value;
                return fixture;
            }

            public static Fixture CreateUnclaimed(
                string itemProductId = "component.motherboard-matx",
                bool fillWorkbench = false)
            {
                var fixture = new Fixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.prototype-001"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.prototype-001"),
                    SlotId = StableId<AssemblySlotIdScope>.Parse("slot.motherboard-main"),
                    FastenerId = StableId<AssemblyFastenerIdScope>.Parse(
                        "fastener.motherboard-main-01"),
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

                return fixture;
            }

            public OperationResult<AssemblyBuildAuthority> TryCreateAuthority()
            {
                return AssemblyBuildAuthority.Create(
                    Components,
                    Inventory,
                    BuildId,
                    ChassisId,
                    SlotId,
                    FastenerId,
                    HandsId,
                    WorkbenchId,
                    MotherboardFormFactor.MicroAtx);
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

        private sealed class ProcessorFixture
        {
            private ProcessorFixture()
            {
            }

            public ProductCatalog Products { get; private set; }

            public PcComponentCatalog Components { get; private set; }

            public InventoryAuthority Inventory { get; private set; }

            public AssemblyBuildAuthority Authority { get; private set; }

            public StableId<PcBuildIdScope> BuildId { get; private set; }

            public StableId<ChassisIdScope> ChassisId { get; private set; }

            public StableId<AssemblySlotIdScope> MotherboardSlotId { get; private set; }

            public StableId<AssemblySlotIdScope> ProcessorSlotId { get; private set; }

            public StableId<AssemblyFastenerIdScope> FastenerId { get; private set; }

            public StableId<AssemblyRetentionIdScope> RetentionId { get; private set; }

            public StableId<ContainerIdScope> HandsId { get; private set; }

            public StableId<ContainerIdScope> WorkbenchId { get; private set; }

            public StableId<ContainerIdScope> ProcessorSocketContainerId { get; private set; }

            public StableId<ContainerIdScope> StorageId { get; private set; }

            public StableId<ItemInstanceIdScope> MotherboardItemId { get; private set; }

            public StableId<ItemInstanceIdScope> ProcessorItemId { get; private set; }

            public StableId<ProductDefinitionIdScope> MotherboardProductId { get; private set; }

            public StableId<ProductDefinitionIdScope> ProcessorProductId { get; private set; }

            public StableId<AssemblyOperationIdScope> AttachId { get; private set; }

            public StableId<AssemblyOperationIdScope> SecureId { get; private set; }

            public static ProcessorFixture Create(
                string processorProductId = "component.processor-lga1700")
            {
                ProcessorFixture fixture = CreateUnclaimed(processorProductId);
                fixture.Authority = fixture.TryCreateAuthority().Value;
                return fixture;
            }

            public static ProcessorFixture CreateUnclaimed(
                string processorProductId = "component.processor-lga1700",
                bool fillProcessorSocket = false,
                int processorSocketCapacity = 1)
            {
                var fixture = new ProcessorFixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.processor-prototype"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.processor-prototype"),
                    MotherboardSlotId = StableId<AssemblySlotIdScope>.Parse(
                        "slot.motherboard-main"),
                    ProcessorSlotId = StableId<AssemblySlotIdScope>.Parse(
                        "slot.processor-main"),
                    FastenerId = StableId<AssemblyFastenerIdScope>.Parse(
                        "fastener.motherboard-main-01"),
                    RetentionId = StableId<AssemblyRetentionIdScope>.Parse(
                        "retention.processor-main"),
                    HandsId = StableId<ContainerIdScope>.Parse("container.actor-hands"),
                    WorkbenchId = StableId<ContainerIdScope>.Parse(
                        "container.assembly-workbench"),
                    ProcessorSocketContainerId = StableId<ContainerIdScope>.Parse(
                        "container.processor-socket"),
                    StorageId = StableId<ContainerIdScope>.Parse("container.storage"),
                    MotherboardItemId = StableId<ItemInstanceIdScope>.Parse(
                        "item.motherboard-processor-fixture"),
                    ProcessorItemId = StableId<ItemInstanceIdScope>.Parse(
                        "item.processor-fixture"),
                    MotherboardProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "component.motherboard-lga1700"),
                    ProcessorProductId = StableId<ProductDefinitionIdScope>.Parse(
                        processorProductId),
                    AttachId = OperationId("operation.processor-fixture-attach"),
                    SecureId = OperationId("operation.processor-fixture-secure")
                };

                ProductDefinition motherboard = Definition(
                    "component.motherboard-lga1700",
                    "LGA1700 Motherboard");
                ProductDefinition processor = Definition(
                    "component.processor-lga1700",
                    "LGA1700 Processor");
                ProductDefinition mismatchedProcessor = Definition(
                    "component.processor-am5",
                    "AM5 Processor");
                fixture.Products = ProductCatalog.Create(new[]
                {
                    motherboard,
                    processor,
                    mismatchedProcessor
                }).Value;
                fixture.Components = PcComponentCatalog.Create(
                    fixture.Products,
                    new[]
                    {
                        PcComponentSpecification.CreateMotherboard(
                            fixture.Products,
                            motherboard.Id,
                            MotherboardFormFactor.MicroAtx,
                            CpuSocketFamily.Lga1700).Value,
                        PcComponentSpecification.CreateProcessor(
                            fixture.Products,
                            processor.Id,
                            CpuSocketFamily.Lga1700).Value,
                        PcComponentSpecification.CreateProcessor(
                            fixture.Products,
                            mismatchedProcessor.Id,
                            CpuSocketFamily.Am5).Value
                    }).Value;

                fixture.Inventory = InventoryAuthority.Create(fixture.Products).Value;
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.HandsId,
                    InventoryContainerKind.ActorHands,
                    3).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.WorkbenchId,
                    InventoryContainerKind.Workbench,
                    1).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.ProcessorSocketContainerId,
                    InventoryContainerKind.Workbench,
                    processorSocketCapacity).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.StorageId,
                    InventoryContainerKind.Storage,
                    4).Value);
                fixture.Inventory.ReceiveSerializedItem(
                    fixture.MotherboardItemId,
                    fixture.MotherboardProductId,
                    fixture.HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", 14_900).Value);
                fixture.Inventory.ReceiveSerializedItem(
                    fixture.ProcessorItemId,
                    fixture.ProcessorProductId,
                    fixture.HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", 24_900).Value);
                if (fillProcessorSocket)
                {
                    fixture.Inventory.ReceiveSerializedItem(
                        StableId<ItemInstanceIdScope>.Parse(
                            "item.processor-socket-occupied"),
                        processor.Id,
                        fixture.ProcessorSocketContainerId,
                        InventoryCondition.OpenBox,
                        InventoryUnitCost.Create("EUR", 20_000).Value);
                }

                return fixture;
            }

            public OperationResult<AssemblyBuildAuthority> TryCreateAuthority()
            {
                return AssemblyBuildAuthority.CreateWithProcessorSocket(
                    Components,
                    Inventory,
                    BuildId,
                    ChassisId,
                    MotherboardSlotId,
                    FastenerId,
                    ProcessorSlotId,
                    RetentionId,
                    HandsId,
                    WorkbenchId,
                    ProcessorSocketContainerId,
                    MotherboardFormFactor.MicroAtx,
                    CpuSocketFamily.Lga1700);
            }

            public void AttachAndSecureMotherboard()
            {
                Assert.That(Authority.AttachMotherboard(
                    AttachId,
                    MotherboardItemId,
                    MotherboardSlotId).IsSuccess, Is.True);
                Assert.That(Authority.SecureMotherboardFastener(
                    SecureId,
                    MotherboardItemId,
                    MotherboardSlotId,
                    FastenerId,
                    AttachId,
                    1).IsSuccess, Is.True);
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
