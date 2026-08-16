using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class DimmAssemblyBuildAuthorityTests
    {
        [Test]
        public void FactoryClaimsThreeContainersAtomicallyAndExposesStableTopology()
        {
            DimmFixture fixture = DimmFixture.CreateUnclaimed();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> created = fixture.TryCreateAuthority();

            Assert.That(created.IsSuccess, Is.True);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            AssemblyBuildAuthority authority = created.Value;
            AssemblyBuildSnapshot snapshot = authority.GetSnapshot();
            Assert.That(snapshot.HasMemorySlot, Is.True);
            Assert.That(snapshot.MemorySlotId, Is.EqualTo(fixture.MemorySlotId));
            Assert.That(snapshot.MemoryRetentionId, Is.EqualTo(fixture.MemoryRetentionId));
            Assert.That(snapshot.MemorySlotContainerId,
                Is.EqualTo(fixture.MemorySlotContainerId));
            Assert.That(snapshot.MemoryChannelId, Is.EqualTo(fixture.MemoryChannelId));
            Assert.That(snapshot.MemoryBankId, Is.EqualTo(fixture.MemoryBankId));
            Assert.That(snapshot.MemoryPopulationPriority, Is.EqualTo(1));
            Assert.That(snapshot.SupportedDimmType, Is.EqualTo(DimmType.Ddr5Udimm));
            Assert.That(snapshot.MemorySlotState, Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.MotherboardItemId, fixture.WorkbenchId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.ProcessorItemId, fixture.ProcessorSocketContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.MemoryItemId, fixture.MemorySlotContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MemoryFullCycleIsExactReplayAndPreservesCanonicalCustody()
        {
            DimmFixture fixture = DimmFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId = OperationId("operation.dimm-seat");
            StableId<AssemblyOperationIdScope> closeId = OperationId("operation.dimm-close");
            StableId<AssemblyOperationIdScope> openId = OperationId("operation.dimm-open");
            StableId<AssemblyOperationIdScope> removeId = OperationId("operation.dimm-remove");
            long inventoryBeforeSeat = fixture.Inventory.Revision;

            AssemblyOperationReceipt seat = fixture.Authority.SeatMemoryModule(
                seatId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                fixture.AttachId,
                fixture.SecureId,
                2).Value;
            Assert.That(fixture.Authority.SeatMemoryModule(
                seatId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                fixture.AttachId,
                fixture.SecureId,
                2).Value, Is.SameAs(seat));
            Assert.That(seat.OperationKind, Is.EqualTo(AssemblyOperationKind.SeatMemoryModule));
            Assert.That(seat.DimmKeyOrientation,
                Is.EqualTo(DimmKeyOrientation.NotchAligned));
            Assert.That(seat.PreviousMemorySlotState, Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(seat.ResultingMemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(seat.SourceContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(seat.TargetContainerId, Is.EqualTo(fixture.MemorySlotContainerId));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 1));

            long inventoryBeforeClose = fixture.Inventory.Revision;
            AssemblyOperationReceipt close = fixture.Authority.CloseMemoryRetention(
                closeId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                seatId,
                3).Value;
            Assert.That(fixture.Authority.CloseMemoryRetention(
                closeId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                seatId,
                3).Value, Is.SameAs(close));
            Assert.That(close.ResultingMemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeClose));

            long retainedAssemblyRevision = fixture.Authority.Revision;
            long retainedInventoryRevision = fixture.Inventory.Revision;
            int retainedReceiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.RemoveMemoryModule(
                    OperationId("operation.dimm-retained-remove"),
                    fixture.MemoryItemId,
                    fixture.MemorySlotId,
                    seatId,
                    4).Error,
                Is.EqualTo(AssemblyFailures.MemoryModuleRetained));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(retainedAssemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(retainedInventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(retainedReceiptCount));

            long inventoryBeforeOpen = fixture.Inventory.Revision;
            AssemblyOperationReceipt open = fixture.Authority.OpenMemoryRetention(
                openId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                seatId,
                closeId,
                4).Value;
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeOpen));
            AssemblyOperationReceipt remove = fixture.Authority.RemoveMemoryModule(
                removeId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                seatId,
                5).Value;
            Assert.That(fixture.Authority.OpenMemoryRetention(
                openId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                seatId,
                closeId,
                4).Value, Is.SameAs(open));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Authority.RemoveMemoryModule(
                removeId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                seatId,
                5).Value, Is.SameAs(remove));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Authority.SeatMemoryModule(
                    seatId,
                    fixture.MemoryItemId,
                    fixture.MemorySlotId,
                    DimmKeyOrientation.Reversed,
                    fixture.AttachId,
                    fixture.SecureId,
                    2).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(6));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(6));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Authority.MemorySlotState, Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.MemoryItemId, out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InvalidAndReversedDimmOrientationFailWithoutMutation()
        {
            DimmFixture fixture = DimmFixture.Create();
            fixture.AttachAndSecureMotherboard();
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            Assert.That(fixture.Authority.SeatMemoryModule(
                    OperationId("operation.dimm-invalid-orientation"),
                    fixture.MemoryItemId,
                    fixture.MemorySlotId,
                    default,
                    fixture.AttachId,
                    fixture.SecureId,
                    2).Error,
                Is.EqualTo(AssemblyFailures.InvalidDimmOrientation));
            Assert.That(fixture.Authority.SeatMemoryModule(
                    OperationId("operation.dimm-reversed"),
                    fixture.MemoryItemId,
                    fixture.MemorySlotId,
                    DimmKeyOrientation.Reversed,
                    fixture.AttachId,
                    fixture.SecureId,
                    2).Error,
                Is.EqualTo(AssemblyFailures.DimmOrientationMismatch));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.MemorySlotState, Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.MemoryItemId, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MemorySeatReportsHostStateBeforeLineageWithoutMutation()
        {
            DimmFixture fixture = DimmFixture.Create();
            long initialInventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> missing =
                fixture.Authority.SeatMemoryModule(
                    OperationId("operation.dimm-host-missing"),
                    fixture.MemoryItemId,
                    fixture.MemorySlotId,
                    DimmKeyOrientation.NotchAligned,
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
                fixture.Authority.SeatMemoryModule(
                    OperationId("operation.dimm-host-unsecured"),
                    fixture.MemoryItemId,
                    fixture.MemorySlotId,
                    DimmKeyOrientation.NotchAligned,
                    fixture.AttachId,
                    default,
                    1);

            Assert.That(unsecured.Error, Is.EqualTo(AssemblyFailures.MotherboardUnsecured));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(attachedReceiptCount));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(attachedInventoryRevision));
            Assert.That(fixture.Authority.MemorySlotState, Is.EqualTo(MemorySlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.MemoryItemId, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MemoryRetentionRequiresSecuredHostAndOpenPathAllowsRecovery()
        {
            DimmFixture unsecured = DimmFixture.Create();
            unsecured.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> unsecuredSeatId =
                OperationId("operation.dimm-unsecured-seat");
            Assert.That(unsecured.Authority.SeatMemoryModule(
                unsecuredSeatId,
                unsecured.MemoryItemId,
                unsecured.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                unsecured.AttachId,
                unsecured.SecureId,
                2).IsSuccess, Is.True);
            Assert.That(unsecured.Authority.UnsecureMotherboardFastener(
                OperationId("operation.dimm-unsecure-host"),
                unsecured.MotherboardItemId,
                unsecured.MotherboardSlotId,
                unsecured.FastenerId,
                unsecured.AttachId,
                unsecured.SecureId,
                3).IsSuccess, Is.True);
            long assemblyRevision = unsecured.Authority.Revision;
            long inventoryRevision = unsecured.Inventory.Revision;
            int receiptCount = unsecured.Authority.ReceiptCount;

            Assert.That(unsecured.Authority.CloseMemoryRetention(
                    OperationId("operation.dimm-close-unsecured"),
                    unsecured.MemoryItemId,
                    unsecured.MemorySlotId,
                    unsecured.MemoryRetentionId,
                    unsecuredSeatId,
                    4).Error,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured));
            Assert.That(unsecured.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(unsecured.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(unsecured.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(unsecured.Authority.RemoveMemoryModule(
                OperationId("operation.dimm-unsecured-remove"),
                unsecured.MemoryItemId,
                unsecured.MemorySlotId,
                unsecuredSeatId,
                4).IsSuccess, Is.True);
            Assert.That(unsecured.Authority.ValidateInvariants().IsSuccess, Is.True);

            DimmFixture retained = DimmFixture.Create();
            retained.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.dimm-recovery-seat");
            StableId<AssemblyOperationIdScope> closeId =
                OperationId("operation.dimm-recovery-close");
            Assert.That(retained.Authority.SeatMemoryModule(
                seatId,
                retained.MemoryItemId,
                retained.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                retained.AttachId,
                retained.SecureId,
                2).IsSuccess, Is.True);
            Assert.That(retained.Authority.CloseMemoryRetention(
                closeId,
                retained.MemoryItemId,
                retained.MemorySlotId,
                retained.MemoryRetentionId,
                seatId,
                3).IsSuccess, Is.True);
            Assert.That(retained.Authority.UnsecureMotherboardFastener(
                OperationId("operation.dimm-recovery-unsecure"),
                retained.MotherboardItemId,
                retained.MotherboardSlotId,
                retained.FastenerId,
                retained.AttachId,
                retained.SecureId,
                4).IsSuccess, Is.True);
            Assert.That(retained.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(retained.Authority.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleRetained));
            Assert.That(retained.Authority.OpenMemoryRetention(
                OperationId("operation.dimm-recovery-open"),
                retained.MemoryItemId,
                retained.MemorySlotId,
                retained.MemoryRetentionId,
                seatId,
                closeId,
                5).IsSuccess, Is.True);
            Assert.That(retained.Authority.RemoveMemoryModule(
                OperationId("operation.dimm-recovery-remove"),
                retained.MemoryItemId,
                retained.MemorySlotId,
                seatId,
                6).IsSuccess, Is.True);
            Assert.That(retained.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorDetachGatePrecedesMemoryThenBothCanBeRecovered()
        {
            DimmFixture fixture = DimmFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> processorSeatId =
                OperationId("operation.detach-processor-seat");
            StableId<AssemblyOperationIdScope> processorCloseId =
                OperationId("operation.detach-processor-close");
            StableId<AssemblyOperationIdScope> memorySeatId =
                OperationId("operation.detach-memory-seat");
            StableId<AssemblyOperationIdScope> memoryCloseId =
                OperationId("operation.detach-memory-close");
            fixture.SeatAndRetainProcessor(processorSeatId, processorCloseId);
            fixture.Authority.SeatMemoryModule(
                memorySeatId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                fixture.AttachId,
                fixture.SecureId,
                4);
            fixture.Authority.CloseMemoryRetention(
                memoryCloseId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                memorySeatId,
                5);
            fixture.Authority.UnsecureMotherboardFastener(
                OperationId("operation.detach-unsecure"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId,
                fixture.FastenerId,
                fixture.AttachId,
                fixture.SecureId,
                6);

            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-processor-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.ProcessorInstalled));
            fixture.Authority.OpenProcessorRetention(
                OperationId("operation.detach-processor-open"),
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.ProcessorRetentionId,
                processorSeatId,
                processorCloseId,
                7);
            fixture.Authority.RemoveProcessor(
                OperationId("operation.detach-processor-remove"),
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                processorSeatId,
                8);
            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.detach-memory-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.MemoryModuleInstalled));
            fixture.Authority.OpenMemoryRetention(
                OperationId("operation.detach-memory-open"),
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                memorySeatId,
                memoryCloseId,
                9);
            fixture.Authority.RemoveMemoryModule(
                OperationId("operation.detach-memory-remove"),
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                memorySeatId,
                10);
            Assert.That(fixture.Authority.DetachMotherboard(
                OperationId("operation.detach-complete"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BenchmarkGatesMemoryMissingThenUnretainedThenBuildIncomplete()
        {
            DimmFixture fixture = DimmFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> processorSeatId =
                OperationId("operation.benchmark-processor-seat");
            StableId<AssemblyOperationIdScope> processorCloseId =
                OperationId("operation.benchmark-processor-close");
            fixture.SeatAndRetainProcessor(processorSeatId, processorCloseId);

            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.MemoryMissing));
            StableId<AssemblyOperationIdScope> memorySeatId =
                OperationId("operation.benchmark-memory-seat");
            fixture.Authority.SeatMemoryModule(
                memorySeatId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                fixture.AttachId,
                fixture.SecureId,
                4);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.MemoryUnretained));
            fixture.Authority.CloseMemoryRetention(
                OperationId("operation.benchmark-memory-close"),
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                memorySeatId,
                5);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryFailuresLeaveEveryPeerUnmanagedAndUnchanged()
        {
            DimmFixture occupied = DimmFixture.CreateUnclaimed(fillMemorySlot: true);
            long occupiedRevision = occupied.Inventory.Revision;
            Assert.That(occupied.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.MemorySlotOccupied));
            Assert.That(occupied.Inventory.Revision, Is.EqualTo(occupiedRevision));
            Assert.That(occupied.Inventory.TransferSerializedItem(
                occupied.MotherboardItemId, occupied.WorkbenchId).IsSuccess, Is.True);
            Assert.That(occupied.Inventory.TransferSerializedItem(
                occupied.ProcessorItemId,
                occupied.ProcessorSocketContainerId).IsSuccess, Is.True);
            StableId<ItemInstanceIdScope> blockerId =
                StableId<ItemInstanceIdScope>.Parse("item.memory-slot-occupied");
            Assert.That(occupied.Inventory.TransferSerializedItem(
                blockerId, occupied.StorageId).IsSuccess, Is.True);
            Assert.That(occupied.Inventory.TransferSerializedItem(
                occupied.MemoryItemId, occupied.MemorySlotContainerId).IsSuccess, Is.True);
            Assert.That(occupied.Inventory.ValidateInvariants().IsSuccess, Is.True);

            DimmFixture invalidCapacity = DimmFixture.CreateUnclaimed(memorySlotCapacity: 2);
            long invalidRevision = invalidCapacity.Inventory.Revision;
            Assert.That(invalidCapacity.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.InvalidMemorySlotContainer));
            Assert.That(invalidCapacity.Inventory.Revision, Is.EqualTo(invalidRevision));
            Assert.That(invalidCapacity.Inventory.TransferSerializedItem(
                invalidCapacity.MotherboardItemId,
                invalidCapacity.WorkbenchId).IsSuccess, Is.True);
            Assert.That(invalidCapacity.Inventory.TransferSerializedItem(
                invalidCapacity.ProcessorItemId,
                invalidCapacity.ProcessorSocketContainerId).IsSuccess, Is.True);
            Assert.That(invalidCapacity.Inventory.TransferSerializedItem(
                invalidCapacity.MemoryItemId,
                invalidCapacity.MemorySlotContainerId).IsSuccess, Is.True);
            Assert.That(invalidCapacity.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryManagedConflictLeavesOnlyOriginalCapabilityInControl()
        {
            DimmFixture fixture = DimmFixture.CreateUnclaimed();
            OperationResult<InventorySerializedTransferAccess> originalClaim =
                fixture.Inventory.ClaimManagedSerializedTransferContainer(
                    fixture.MemorySlotContainerId);
            Assert.That(originalClaim.IsSuccess, Is.True);
            long revision = fixture.Inventory.Revision;

            Assert.That(fixture.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.PlanForeign));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.MotherboardItemId, fixture.WorkbenchId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.ProcessorItemId,
                fixture.ProcessorSocketContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.PrepareSerializedItemTransfer(
                    fixture.MemoryItemId,
                    fixture.MemorySlotContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.PrepareSerializedItemTransfer(
                    fixture.MemoryItemId,
                    fixture.MemorySlotContainerId,
                    originalClaim.Value).IsSuccess,
                Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryReservationFailureLeavesEveryContainerPublicAfterRelease()
        {
            DimmFixture fixture = DimmFixture.CreateUnclaimed(fillMemorySlot: true);
            StableId<ItemInstanceIdScope> blockerId =
                StableId<ItemInstanceIdScope>.Parse("item.memory-slot-occupied");
            StableId<ReservationIdScope> reservationId =
                StableId<ReservationIdScope>.Parse("reservation.factory-memory-slot");
            StableId<InventoryClaimIdScope> claimId =
                StableId<InventoryClaimIdScope>.Parse("claim.factory-memory-slot");
            Assert.That(fixture.Inventory.ReserveSerializedItem(
                reservationId, claimId, blockerId).IsSuccess, Is.True);
            long revision = fixture.Inventory.Revision;
            int reservationCount = fixture.Inventory.ReservationCount;

            Assert.That(fixture.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.MemorySlotOccupied));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(revision));
            Assert.That(fixture.Inventory.ReservationCount, Is.EqualTo(reservationCount));
            Assert.That(fixture.Inventory.ReleaseReservation(reservationId).IsSuccess,
                Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                blockerId, fixture.StorageId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.MotherboardItemId, fixture.WorkbenchId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.ProcessorItemId,
                fixture.ProcessorSocketContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.TransferSerializedItem(
                fixture.MemoryItemId, fixture.MemorySlotContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryRevisionOverflowNeverPublishesManagedCapabilities()
        {
            DimmFixture fixture = DimmFixture.CreateUnclaimed();
            SetInventoryRevision(fixture.Inventory, long.MaxValue);

            Assert.That(fixture.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.RevisionOverflow));
            Assert.That(fixture.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.RevisionOverflow));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(long.MaxValue));
            Assert.That(fixture.Inventory.PrepareSerializedItemTransfer(
                    fixture.MotherboardItemId, fixture.WorkbenchId).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(fixture.Inventory.PrepareSerializedItemTransfer(
                    fixture.ProcessorItemId,
                    fixture.ProcessorSocketContainerId).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(fixture.Inventory.PrepareSerializedItemTransfer(
                    fixture.MemoryItemId,
                    fixture.MemorySlotContainerId).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MemoryRemoveFailsClosedWhenAuthoritativeHostIsMissing()
        {
            DimmFixture fixture = DimmFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.dimm-host-defense-seat");
            Assert.That(fixture.Authority.SeatMemoryModule(
                seatId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                fixture.AttachId,
                fixture.SecureId,
                2).IsSuccess, Is.True);
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;
            FieldInfo hostStateField = typeof(AssemblyBuildAuthority).GetField(
                "_motherboardSeatState",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(hostStateField, Is.Not.Null);
            AssemblySeatState originalState = fixture.Authority.MotherboardSeatState;

            try
            {
                hostStateField.SetValue(fixture.Authority, AssemblySeatState.Empty);
                Assert.That(fixture.Authority.RemoveMemoryModule(
                        OperationId("operation.dimm-host-defense-remove"),
                        fixture.MemoryItemId,
                        fixture.MemorySlotId,
                        seatId,
                        assemblyRevision).Error,
                    Is.EqualTo(AssemblyFailures.MotherboardMissing));
                Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
                Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
                Assert.That(fixture.Authority.MemorySlotState,
                    Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
                Assert.That(fixture.Inventory.TryGetSerializedItem(
                    fixture.MemoryItemId, out InventoryItemRecord seated), Is.True);
                Assert.That(seated.ContainerId, Is.EqualTo(fixture.MemorySlotContainerId));
            }
            finally
            {
                hostStateField.SetValue(fixture.Authority, originalState);
            }

            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MemoryRemoveIntoFullHandsFailsWithoutMutation()
        {
            DimmFixture fixture = DimmFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.dimm-full-hands-seat");
            fixture.Authority.SeatMemoryModule(
                seatId,
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                DimmKeyOrientation.NotchAligned,
                fixture.AttachId,
                fixture.SecureId,
                2);
            for (int index = 0; index < 3; index++)
            {
                Assert.That(fixture.Inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse($"item.dimm-hands-blocker-{index}"),
                    fixture.MemoryProductId,
                    fixture.HandsId,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 7_000 + index).Value).IsSuccess, Is.True);
            }

            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.RemoveMemoryModule(
                    OperationId("operation.dimm-full-hands-remove"),
                    fixture.MemoryItemId,
                    fixture.MemorySlotId,
                    seatId,
                    3).Error,
                Is.EqualTo(AssemblyFailures.HandsCapacityExceeded));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.MemorySlotState,
                Is.EqualTo(MemorySlotState.MemoryModuleSeatedOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.MemoryItemId, out InventoryItemRecord seated), Is.True);
            Assert.That(seated.ContainerId, Is.EqualTo(fixture.MemorySlotContainerId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        private static StableId<AssemblyOperationIdScope> OperationId(string value)
        {
            return StableId<AssemblyOperationIdScope>.Parse(value);
        }

        private static void SetInventoryRevision(InventoryAuthority authority, long revision)
        {
            PropertyInfo revisionProperty = typeof(InventoryAuthority).GetProperty(
                nameof(InventoryAuthority.Revision),
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(revisionProperty, Is.Not.Null);
            revisionProperty.GetSetMethod(nonPublic: true).Invoke(
                authority,
                new object[] { revision });
        }

        private sealed class DimmFixture
        {
            private DimmFixture()
            {
            }

            public ProductCatalog Products { get; private set; }

            public PcComponentCatalog Components { get; private set; }

            public InventoryAuthority Inventory { get; private set; }

            public AssemblyBuildAuthority Authority { get; private set; }

            public StableId<PcBuildIdScope> BuildId { get; private set; }

            public StableId<ChassisIdScope> ChassisId { get; private set; }

            public StableId<AssemblySlotIdScope> MotherboardSlotId { get; private set; }

            public StableId<AssemblyFastenerIdScope> FastenerId { get; private set; }

            public StableId<AssemblySlotIdScope> ProcessorSlotId { get; private set; }

            public StableId<AssemblyRetentionIdScope> ProcessorRetentionId { get; private set; }

            public StableId<AssemblySlotIdScope> MemorySlotId { get; private set; }

            public StableId<AssemblyRetentionIdScope> MemoryRetentionId { get; private set; }

            public StableId<AssemblyMemoryChannelIdScope> MemoryChannelId { get; private set; }

            public StableId<AssemblyMemoryBankIdScope> MemoryBankId { get; private set; }

            public StableId<ContainerIdScope> HandsId { get; private set; }

            public StableId<ContainerIdScope> WorkbenchId { get; private set; }

            public StableId<ContainerIdScope> ProcessorSocketContainerId { get; private set; }

            public StableId<ContainerIdScope> MemorySlotContainerId { get; private set; }

            public StableId<ContainerIdScope> StorageId { get; private set; }

            public StableId<ItemInstanceIdScope> MotherboardItemId { get; private set; }

            public StableId<ItemInstanceIdScope> ProcessorItemId { get; private set; }

            public StableId<ItemInstanceIdScope> MemoryItemId { get; private set; }

            public StableId<ProductDefinitionIdScope> MotherboardProductId { get; private set; }

            public StableId<ProductDefinitionIdScope> ProcessorProductId { get; private set; }

            public StableId<ProductDefinitionIdScope> MemoryProductId { get; private set; }

            public StableId<AssemblyOperationIdScope> AttachId { get; private set; }

            public StableId<AssemblyOperationIdScope> SecureId { get; private set; }

            public DimmSlotDefinition MemorySlotDefinition { get; private set; }

            public static DimmFixture Create()
            {
                DimmFixture fixture = CreateUnclaimed();
                fixture.Authority = fixture.TryCreateAuthority().Value;
                return fixture;
            }

            public static DimmFixture CreateUnclaimed(
                bool fillMemorySlot = false,
                int memorySlotCapacity = 1)
            {
                var fixture = new DimmFixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.dimm-prototype"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.dimm-prototype"),
                    MotherboardSlotId = StableId<AssemblySlotIdScope>.Parse(
                        "slot.motherboard-main"),
                    FastenerId = StableId<AssemblyFastenerIdScope>.Parse(
                        "fastener.motherboard-main-01"),
                    ProcessorSlotId = StableId<AssemblySlotIdScope>.Parse(
                        "slot.processor-main"),
                    ProcessorRetentionId = StableId<AssemblyRetentionIdScope>.Parse(
                        "retention.processor-main"),
                    MemorySlotId = StableId<AssemblySlotIdScope>.Parse(
                        "assembly.slot.memory-a2"),
                    MemoryRetentionId = StableId<AssemblyRetentionIdScope>.Parse(
                        "assembly.retention.memory-a2-dual-latch"),
                    MemoryChannelId = StableId<AssemblyMemoryChannelIdScope>.Parse(
                        "assembly.memory-channel.a"),
                    MemoryBankId = StableId<AssemblyMemoryBankIdScope>.Parse(
                        "assembly.memory-bank.2"),
                    HandsId = StableId<ContainerIdScope>.Parse("container.actor-hands"),
                    WorkbenchId = StableId<ContainerIdScope>.Parse(
                        "container.assembly-workbench"),
                    ProcessorSocketContainerId = StableId<ContainerIdScope>.Parse(
                        "container.processor-socket"),
                    MemorySlotContainerId = StableId<ContainerIdScope>.Parse(
                        "inventory.container.assembly-memory-a2"),
                    StorageId = StableId<ContainerIdScope>.Parse("container.storage"),
                    MotherboardItemId = StableId<ItemInstanceIdScope>.Parse(
                        "item.motherboard-dimm-fixture"),
                    ProcessorItemId = StableId<ItemInstanceIdScope>.Parse(
                        "item.processor-dimm-fixture"),
                    MemoryItemId = StableId<ItemInstanceIdScope>.Parse(
                        "inventory.item.northstar-d5-16-udimm-001"),
                    MotherboardProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "component.motherboard-ddr5"),
                    ProcessorProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "component.processor-lga1700"),
                    MemoryProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "catalog.memory.northstar-d5-16-udimm"),
                    AttachId = OperationId("operation.dimm-fixture-attach"),
                    SecureId = OperationId("operation.dimm-fixture-secure")
                };

                ProductDefinition motherboard = Definition(
                    fixture.MotherboardProductId,
                    "DDR5 Motherboard");
                ProductDefinition processor = Definition(
                    fixture.ProcessorProductId,
                    "LGA1700 Processor");
                ProductDefinition memory = Definition(
                    fixture.MemoryProductId,
                    "Northstar D5 16 GB UDIMM");
                fixture.Products = ProductCatalog.Create(new[]
                {
                    motherboard,
                    processor,
                    memory
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
                            DimmType.Ddr5Udimm).Value,
                        PcComponentSpecification.CreateProcessor(
                            fixture.Products,
                            processor.Id,
                            CpuSocketFamily.Lga1700).Value,
                        PcComponentSpecification.CreateMemoryModule(
                            fixture.Products,
                            memory.Id,
                            DimmType.Ddr5Udimm).Value
                    }).Value;
                fixture.MemorySlotDefinition = DimmSlotDefinition.Create(
                    fixture.MemorySlotId,
                    fixture.MemoryRetentionId,
                    fixture.MemorySlotContainerId,
                    fixture.MemoryChannelId,
                    fixture.MemoryBankId,
                    1,
                    DimmType.Ddr5Udimm).Value;

                fixture.Inventory = InventoryAuthority.Create(fixture.Products).Value;
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.HandsId,
                    InventoryContainerKind.ActorHands,
                    4).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.WorkbenchId,
                    InventoryContainerKind.Workbench,
                    1).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.ProcessorSocketContainerId,
                    InventoryContainerKind.Workbench,
                    1).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.MemorySlotContainerId,
                    InventoryContainerKind.Workbench,
                    memorySlotCapacity).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.StorageId,
                    InventoryContainerKind.Storage,
                    8).Value);
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
                fixture.Inventory.ReceiveSerializedItem(
                    fixture.MemoryItemId,
                    fixture.MemoryProductId,
                    fixture.HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", 8_900).Value);
                if (fillMemorySlot)
                {
                    fixture.Inventory.ReceiveSerializedItem(
                        StableId<ItemInstanceIdScope>.Parse("item.memory-slot-occupied"),
                        fixture.MemoryProductId,
                        fixture.MemorySlotContainerId,
                        InventoryCondition.OpenBox,
                        InventoryUnitCost.Create("EUR", 7_900).Value);
                }

                return fixture;
            }

            public OperationResult<AssemblyBuildAuthority> TryCreateAuthority()
            {
                return AssemblyBuildAuthority.CreateWithProcessorSocketAndMemorySlot(
                    Components,
                    Inventory,
                    BuildId,
                    ChassisId,
                    MotherboardSlotId,
                    FastenerId,
                    ProcessorSlotId,
                    ProcessorRetentionId,
                    MemorySlotDefinition,
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

            public void SeatAndRetainProcessor(
                StableId<AssemblyOperationIdScope> seatId,
                StableId<AssemblyOperationIdScope> closeId)
            {
                Assert.That(Authority.SeatProcessor(
                    seatId,
                    ProcessorItemId,
                    ProcessorSlotId,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseProcessorRetention(
                    closeId,
                    ProcessorItemId,
                    ProcessorSlotId,
                    ProcessorRetentionId,
                    seatId,
                    Authority.Revision).IsSuccess, Is.True);
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
