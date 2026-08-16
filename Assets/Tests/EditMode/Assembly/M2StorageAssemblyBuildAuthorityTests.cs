using System.Reflection;
using NUnit.Framework;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Tests.EditMode.Assembly
{
    public sealed class M2StorageAssemblyBuildAuthorityTests
    {
        [Test]
        public void FactoryClaimsFourContainersAtomicallyAndExposesStableTopology()
        {
            StorageFixture fixture = StorageFixture.CreateUnclaimed();
            long inventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyBuildAuthority> created = fixture.TryCreateAuthority();

            Assert.That(created.IsSuccess, Is.True);
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            AssemblyBuildAuthority authority = created.Value;
            AssemblyBuildSnapshot snapshot = authority.GetSnapshot();
            Assert.That(snapshot.HasStorageSlot, Is.True);
            Assert.That(snapshot.StorageSlotId, Is.EqualTo(fixture.StorageSlotId));
            Assert.That(snapshot.StorageCaptiveScrewId, Is.EqualTo(fixture.StorageRetentionId));
            Assert.That(snapshot.StorageSlotContainerId,
                Is.EqualTo(fixture.StorageSlotContainerId));
            Assert.That(snapshot.SupportedM2StorageType,
                Is.EqualTo(M2StorageType.NvmePcie4X4_2280));
            Assert.That(snapshot.StorageSlotState, Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.MotherboardItemId, fixture.WorkbenchId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.ProcessorItemId, fixture.ProcessorSocketContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.TransferSerializedItem(
                    fixture.StorageItemId, fixture.StorageSlotContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StorageFullCycleIsExactReplayAndPreservesCanonicalCustody()
        {
            StorageFixture fixture = StorageFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId = OperationId("operation.m2-seat");
            StableId<AssemblyOperationIdScope> closeId = OperationId("operation.m2-close");
            StableId<AssemblyOperationIdScope> openId = OperationId("operation.m2-open");
            StableId<AssemblyOperationIdScope> removeId = OperationId("operation.m2-remove");
            long inventoryBeforeSeat = fixture.Inventory.Revision;

            AssemblyOperationReceipt seat = fixture.Authority.SeatStorageDevice(
                seatId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                fixture.AttachId,
                fixture.SecureId,
                2).Value;
            Assert.That(fixture.Authority.SeatStorageDevice(
                seatId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                fixture.AttachId,
                fixture.SecureId,
                2).Value, Is.SameAs(seat));
            Assert.That(seat.OperationKind, Is.EqualTo(AssemblyOperationKind.SeatStorageDevice));
            Assert.That(seat.M2KeyOrientation,
                Is.EqualTo(M2KeyOrientation.KeyAligned));
            Assert.That(seat.PreviousStorageSlotState, Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(seat.ResultingStorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            Assert.That(seat.SourceContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(seat.TargetContainerId, Is.EqualTo(fixture.StorageSlotContainerId));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 1));

            long inventoryBeforeClose = fixture.Inventory.Revision;
            AssemblyOperationReceipt close = fixture.Authority.SecureStorageDevice(
                closeId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                seatId,
                3).Value;
            Assert.That(fixture.Authority.SecureStorageDevice(
                closeId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                seatId,
                3).Value, Is.SameAs(close));
            Assert.That(close.ResultingStorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeClose));

            long retainedAssemblyRevision = fixture.Authority.Revision;
            long retainedInventoryRevision = fixture.Inventory.Revision;
            int retainedReceiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.RemoveStorageDevice(
                    OperationId("operation.m2-retained-remove"),
                    fixture.StorageItemId,
                    fixture.StorageSlotId,
                    seatId,
                    4).Error,
                Is.EqualTo(AssemblyFailures.StorageDeviceSecured));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(retainedAssemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(retainedInventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(retainedReceiptCount));

            long inventoryBeforeOpen = fixture.Inventory.Revision;
            AssemblyOperationReceipt open = fixture.Authority.UnsecureStorageDevice(
                openId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                seatId,
                closeId,
                4).Value;
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeOpen));
            AssemblyOperationReceipt remove = fixture.Authority.RemoveStorageDevice(
                removeId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                seatId,
                5).Value;
            Assert.That(fixture.Authority.UnsecureStorageDevice(
                openId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                seatId,
                closeId,
                4).Value, Is.SameAs(open));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Authority.RemoveStorageDevice(
                removeId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                seatId,
                5).Value, Is.SameAs(remove));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Authority.SeatStorageDevice(
                    seatId,
                    fixture.StorageItemId,
                    fixture.StorageSlotId,
                    M2KeyOrientation.Reversed,
                    fixture.AttachId,
                    fixture.SecureId,
                    2).Error,
                Is.EqualTo(AssemblyFailures.OperationConflict));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(6));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(6));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryBeforeSeat + 2));
            Assert.That(fixture.Authority.StorageSlotState, Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.StorageItemId, out InventoryItemRecord returned), Is.True);
            Assert.That(returned.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void InvalidAndReversedM2OrientationFailWithoutMutation()
        {
            StorageFixture fixture = StorageFixture.Create();
            fixture.AttachAndSecureMotherboard();
            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;

            Assert.That(fixture.Authority.SeatStorageDevice(
                    OperationId("operation.m2-invalid-orientation"),
                    fixture.StorageItemId,
                    fixture.StorageSlotId,
                    default,
                    fixture.AttachId,
                    fixture.SecureId,
                    2).Error,
                Is.EqualTo(AssemblyFailures.InvalidM2Orientation));
            Assert.That(fixture.Authority.SeatStorageDevice(
                    OperationId("operation.m2-reversed"),
                    fixture.StorageItemId,
                    fixture.StorageSlotId,
                    M2KeyOrientation.Reversed,
                    fixture.AttachId,
                    fixture.SecureId,
                    2).Error,
                Is.EqualTo(AssemblyFailures.M2OrientationMismatch));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.StorageSlotState, Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.StorageItemId, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StorageSeatReportsHostStateBeforeLineageWithoutMutation()
        {
            StorageFixture fixture = StorageFixture.Create();
            long initialInventoryRevision = fixture.Inventory.Revision;

            OperationResult<AssemblyOperationReceipt> missing =
                fixture.Authority.SeatStorageDevice(
                    OperationId("operation.m2-host-missing"),
                    fixture.StorageItemId,
                    fixture.StorageSlotId,
                    M2KeyOrientation.KeyAligned,
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
                fixture.Authority.SeatStorageDevice(
                    OperationId("operation.m2-host-unsecured"),
                    fixture.StorageItemId,
                    fixture.StorageSlotId,
                    M2KeyOrientation.KeyAligned,
                    fixture.AttachId,
                    default,
                    1);

            Assert.That(unsecured.Error, Is.EqualTo(AssemblyFailures.MotherboardUnsecured));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(1));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(attachedReceiptCount));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(attachedInventoryRevision));
            Assert.That(fixture.Authority.StorageSlotState, Is.EqualTo(StorageSlotState.EmptyOpen));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.StorageItemId, out InventoryItemRecord unchanged), Is.True);
            Assert.That(unchanged.ContainerId, Is.EqualTo(fixture.HandsId));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StorageRetentionRequiresSecuredHostAndOpenPathAllowsRecovery()
        {
            StorageFixture unsecured = StorageFixture.Create();
            unsecured.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> unsecuredSeatId =
                OperationId("operation.m2-unsecured-seat");
            Assert.That(unsecured.Authority.SeatStorageDevice(
                unsecuredSeatId,
                unsecured.StorageItemId,
                unsecured.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                unsecured.AttachId,
                unsecured.SecureId,
                2).IsSuccess, Is.True);
            Assert.That(unsecured.Authority.UnsecureMotherboardFastener(
                OperationId("operation.m2-unsecure-host"),
                unsecured.MotherboardItemId,
                unsecured.MotherboardSlotId,
                unsecured.FastenerId,
                unsecured.AttachId,
                unsecured.SecureId,
                3).IsSuccess, Is.True);
            long assemblyRevision = unsecured.Authority.Revision;
            long inventoryRevision = unsecured.Inventory.Revision;
            int receiptCount = unsecured.Authority.ReceiptCount;

            Assert.That(unsecured.Authority.SecureStorageDevice(
                    OperationId("operation.m2-close-unsecured"),
                    unsecured.StorageItemId,
                    unsecured.StorageSlotId,
                    unsecured.StorageRetentionId,
                    unsecuredSeatId,
                    4).Error,
                Is.EqualTo(AssemblyFailures.MotherboardUnsecured));
            Assert.That(unsecured.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(unsecured.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(unsecured.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(unsecured.Authority.RemoveStorageDevice(
                OperationId("operation.m2-unsecured-remove"),
                unsecured.StorageItemId,
                unsecured.StorageSlotId,
                unsecuredSeatId,
                4).IsSuccess, Is.True);
            Assert.That(unsecured.Authority.ValidateInvariants().IsSuccess, Is.True);

            StorageFixture retained = StorageFixture.Create();
            retained.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.m2-recovery-seat");
            StableId<AssemblyOperationIdScope> closeId =
                OperationId("operation.m2-recovery-close");
            Assert.That(retained.Authority.SeatStorageDevice(
                seatId,
                retained.StorageItemId,
                retained.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                retained.AttachId,
                retained.SecureId,
                2).IsSuccess, Is.True);
            Assert.That(retained.Authority.SecureStorageDevice(
                closeId,
                retained.StorageItemId,
                retained.StorageSlotId,
                retained.StorageRetentionId,
                seatId,
                3).IsSuccess, Is.True);
            Assert.That(retained.Authority.UnsecureMotherboardFastener(
                OperationId("operation.m2-recovery-unsecure"),
                retained.MotherboardItemId,
                retained.MotherboardSlotId,
                retained.FastenerId,
                retained.AttachId,
                retained.SecureId,
                4).IsSuccess, Is.True);
            Assert.That(retained.Authority.MotherboardSeatState,
                Is.EqualTo(AssemblySeatState.SeatedUnsecured));
            Assert.That(retained.Authority.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSecured));
            Assert.That(retained.Authority.UnsecureStorageDevice(
                OperationId("operation.m2-recovery-open"),
                retained.StorageItemId,
                retained.StorageSlotId,
                retained.StorageRetentionId,
                seatId,
                closeId,
                5).IsSuccess, Is.True);
            Assert.That(retained.Authority.RemoveStorageDevice(
                OperationId("operation.m2-recovery-remove"),
                retained.StorageItemId,
                retained.StorageSlotId,
                seatId,
                6).IsSuccess, Is.True);
            Assert.That(retained.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ProcessorDetachGatePrecedesStorageThenBothCanBeRecovered()
        {
            StorageFixture fixture = StorageFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> processorSeatId =
                OperationId("operation.detach-processor-seat");
            StableId<AssemblyOperationIdScope> processorCloseId =
                OperationId("operation.detach-processor-close");
            StableId<AssemblyOperationIdScope> storageSeatId =
                OperationId("operation.detach-storage-seat");
            StableId<AssemblyOperationIdScope> storageCloseId =
                OperationId("operation.detach-storage-close");
            fixture.SeatAndRetainProcessor(processorSeatId, processorCloseId);
            fixture.Authority.SeatStorageDevice(
                storageSeatId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                fixture.AttachId,
                fixture.SecureId,
                4);
            fixture.Authority.SecureStorageDevice(
                storageCloseId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                storageSeatId,
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
                    OperationId("operation.detach-storage-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.StorageDeviceInstalled));
            fixture.Authority.UnsecureStorageDevice(
                OperationId("operation.detach-storage-open"),
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                storageSeatId,
                storageCloseId,
                9);
            fixture.Authority.RemoveStorageDevice(
                OperationId("operation.detach-storage-remove"),
                fixture.StorageItemId,
                fixture.StorageSlotId,
                storageSeatId,
                10);
            Assert.That(fixture.Authority.DetachMotherboard(
                OperationId("operation.detach-complete"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void MotherboardDetachPreservesProcessorThenMemoryThenStoragePrecedence()
        {
            StorageFixture fixture = StorageFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> processorSeat =
                OperationId("operation.precedence-processor-seat");
            StableId<AssemblyOperationIdScope> processorClose =
                OperationId("operation.precedence-processor-close");
            StableId<AssemblyOperationIdScope> memorySeat =
                OperationId("operation.precedence-memory-seat");
            StableId<AssemblyOperationIdScope> memoryClose =
                OperationId("operation.precedence-memory-close");
            StableId<AssemblyOperationIdScope> storageSeat =
                OperationId("operation.precedence-storage-seat");
            StableId<AssemblyOperationIdScope> storageSecure =
                OperationId("operation.precedence-storage-secure");
            fixture.SeatAndRetainProcessor(processorSeat, processorClose);
            fixture.SeatAndRetainMemory(memorySeat, memoryClose);
            Assert.That(fixture.Authority.SeatStorageDevice(
                storageSeat,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                fixture.AttachId,
                fixture.SecureId,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.SecureStorageDevice(
                storageSecure,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                storageSeat,
                fixture.Authority.Revision).IsSuccess, Is.True);
            Assert.That(fixture.Authority.UnsecureMotherboardFastener(
                OperationId("operation.precedence-host-unsecure"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId,
                fixture.FastenerId,
                fixture.AttachId,
                fixture.SecureId,
                fixture.Authority.Revision).IsSuccess, Is.True);

            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.precedence-processor-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.ProcessorInstalled));
            fixture.Authority.OpenProcessorRetention(
                OperationId("operation.precedence-processor-open"),
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                fixture.ProcessorRetentionId,
                processorSeat,
                processorClose,
                fixture.Authority.Revision);
            fixture.Authority.RemoveProcessor(
                OperationId("operation.precedence-processor-remove"),
                fixture.ProcessorItemId,
                fixture.ProcessorSlotId,
                processorSeat,
                fixture.Authority.Revision);

            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.precedence-memory-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.MemoryModuleInstalled));
            fixture.Authority.OpenMemoryRetention(
                OperationId("operation.precedence-memory-open"),
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                fixture.MemoryRetentionId,
                memorySeat,
                memoryClose,
                fixture.Authority.Revision);
            fixture.Authority.RemoveMemoryModule(
                OperationId("operation.precedence-memory-remove"),
                fixture.MemoryItemId,
                fixture.MemorySlotId,
                memorySeat,
                fixture.Authority.Revision);

            Assert.That(fixture.Authority.DetachMotherboard(
                    OperationId("operation.precedence-storage-blocked"),
                    fixture.MotherboardItemId,
                    fixture.MotherboardSlotId).Error,
                Is.EqualTo(AssemblyFailures.StorageDeviceInstalled));
            fixture.Authority.UnsecureStorageDevice(
                OperationId("operation.precedence-storage-unsecure"),
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                storageSeat,
                storageSecure,
                fixture.Authority.Revision);
            fixture.Authority.RemoveStorageDevice(
                OperationId("operation.precedence-storage-remove"),
                fixture.StorageItemId,
                fixture.StorageSlotId,
                storageSeat,
                fixture.Authority.Revision);
            Assert.That(fixture.Authority.DetachMotherboard(
                OperationId("operation.precedence-detach"),
                fixture.MotherboardItemId,
                fixture.MotherboardSlotId).IsSuccess, Is.True);
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BenchmarkGatesStorageMissingThenUnretainedThenBuildIncomplete()
        {
            StorageFixture fixture = StorageFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> processorSeatId =
                OperationId("operation.benchmark-processor-seat");
            StableId<AssemblyOperationIdScope> processorCloseId =
                OperationId("operation.benchmark-processor-close");
            fixture.SeatAndRetainProcessor(processorSeatId, processorCloseId);
            fixture.SeatAndRetainMemory(
                OperationId("operation.benchmark-memory-seat"),
                OperationId("operation.benchmark-memory-close"));

            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.StorageMissing));
            StableId<AssemblyOperationIdScope> storageSeatId =
                OperationId("operation.benchmark-storage-seat");
            fixture.Authority.SeatStorageDevice(
                storageSeatId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                fixture.AttachId,
                fixture.SecureId,
                6);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.StorageUnsecured));
            fixture.Authority.SecureStorageDevice(
                OperationId("operation.benchmark-storage-close"),
                fixture.StorageItemId,
                fixture.StorageSlotId,
                fixture.StorageRetentionId,
                storageSeatId,
                7);
            Assert.That(fixture.Authority.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryFailuresLeaveEveryPeerUnmanagedAndUnchanged()
        {
            StorageFixture occupied = StorageFixture.CreateUnclaimed(fillStorageSlot: true);
            long occupiedRevision = occupied.Inventory.Revision;
            Assert.That(occupied.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.StorageSlotOccupied));
            Assert.That(occupied.Inventory.Revision, Is.EqualTo(occupiedRevision));
            Assert.That(occupied.Inventory.TransferSerializedItem(
                occupied.MotherboardItemId, occupied.WorkbenchId).IsSuccess, Is.True);
            Assert.That(occupied.Inventory.TransferSerializedItem(
                occupied.ProcessorItemId,
                occupied.ProcessorSocketContainerId).IsSuccess, Is.True);
            StableId<ItemInstanceIdScope> blockerId =
                StableId<ItemInstanceIdScope>.Parse("item.storage-slot-occupied");
            Assert.That(occupied.Inventory.TransferSerializedItem(
                blockerId, occupied.StorageId).IsSuccess, Is.True);
            Assert.That(occupied.Inventory.TransferSerializedItem(
                occupied.StorageItemId, occupied.StorageSlotContainerId).IsSuccess, Is.True);
            Assert.That(occupied.Inventory.ValidateInvariants().IsSuccess, Is.True);

            StorageFixture invalidCapacity = StorageFixture.CreateUnclaimed(storageSlotCapacity: 2);
            long invalidRevision = invalidCapacity.Inventory.Revision;
            Assert.That(invalidCapacity.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.InvalidStorageSlotContainer));
            Assert.That(invalidCapacity.Inventory.Revision, Is.EqualTo(invalidRevision));
            Assert.That(invalidCapacity.Inventory.TransferSerializedItem(
                invalidCapacity.MotherboardItemId,
                invalidCapacity.WorkbenchId).IsSuccess, Is.True);
            Assert.That(invalidCapacity.Inventory.TransferSerializedItem(
                invalidCapacity.ProcessorItemId,
                invalidCapacity.ProcessorSocketContainerId).IsSuccess, Is.True);
            Assert.That(invalidCapacity.Inventory.TransferSerializedItem(
                invalidCapacity.StorageItemId,
                invalidCapacity.StorageSlotContainerId).IsSuccess, Is.True);
            Assert.That(invalidCapacity.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryManagedConflictLeavesOnlyOriginalCapabilityInControl()
        {
            StorageFixture fixture = StorageFixture.CreateUnclaimed();
            OperationResult<InventorySerializedTransferAccess> originalClaim =
                fixture.Inventory.ClaimManagedSerializedTransferContainer(
                    fixture.StorageSlotContainerId);
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
                    fixture.StorageItemId,
                    fixture.StorageSlotContainerId).Error,
                Is.EqualTo(InventoryFailures.SerializedTransferContainerManaged));
            Assert.That(fixture.Inventory.PrepareSerializedItemTransfer(
                    fixture.StorageItemId,
                    fixture.StorageSlotContainerId,
                    originalClaim.Value).IsSuccess,
                Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryReservationFailureLeavesEveryContainerPublicAfterRelease()
        {
            StorageFixture fixture = StorageFixture.CreateUnclaimed(fillStorageSlot: true);
            StableId<ItemInstanceIdScope> blockerId =
                StableId<ItemInstanceIdScope>.Parse("item.storage-slot-occupied");
            StableId<ReservationIdScope> reservationId =
                StableId<ReservationIdScope>.Parse("reservation.factory-storage-slot");
            StableId<InventoryClaimIdScope> claimId =
                StableId<InventoryClaimIdScope>.Parse("claim.factory-storage-slot");
            Assert.That(fixture.Inventory.ReserveSerializedItem(
                reservationId, claimId, blockerId).IsSuccess, Is.True);
            long revision = fixture.Inventory.Revision;
            int reservationCount = fixture.Inventory.ReservationCount;

            Assert.That(fixture.TryCreateAuthority().Error,
                Is.EqualTo(AssemblyFailures.StorageSlotOccupied));
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
                fixture.StorageItemId, fixture.StorageSlotContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void FactoryRevisionOverflowNeverPublishesManagedCapabilities()
        {
            StorageFixture fixture = StorageFixture.CreateUnclaimed();
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
                    fixture.StorageItemId,
                    fixture.StorageSlotContainerId).Error,
                Is.EqualTo(InventoryFailures.RevisionOverflow));
            Assert.That(fixture.Inventory.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StorageRemoveFailsClosedWhenAuthoritativeHostIsMissing()
        {
            StorageFixture fixture = StorageFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.m2-host-defense-seat");
            Assert.That(fixture.Authority.SeatStorageDevice(
                seatId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                M2KeyOrientation.KeyAligned,
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
                Assert.That(fixture.Authority.RemoveStorageDevice(
                        OperationId("operation.m2-host-defense-remove"),
                        fixture.StorageItemId,
                        fixture.StorageSlotId,
                        seatId,
                        assemblyRevision).Error,
                    Is.EqualTo(AssemblyFailures.MotherboardMissing));
                Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
                Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
                Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
                Assert.That(fixture.Authority.StorageSlotState,
                    Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
                Assert.That(fixture.Inventory.TryGetSerializedItem(
                    fixture.StorageItemId, out InventoryItemRecord seated), Is.True);
                Assert.That(seated.ContainerId, Is.EqualTo(fixture.StorageSlotContainerId));
            }
            finally
            {
                hostStateField.SetValue(fixture.Authority, originalState);
            }

            Assert.That(fixture.Authority.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void StorageRemoveIntoFullHandsFailsWithoutMutation()
        {
            StorageFixture fixture = StorageFixture.Create();
            fixture.AttachAndSecureMotherboard();
            StableId<AssemblyOperationIdScope> seatId =
                OperationId("operation.m2-full-hands-seat");
            fixture.Authority.SeatStorageDevice(
                seatId,
                fixture.StorageItemId,
                fixture.StorageSlotId,
                M2KeyOrientation.KeyAligned,
                fixture.AttachId,
                fixture.SecureId,
                2);
            for (int index = 0; index < 3; index++)
            {
                Assert.That(fixture.Inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse($"item.m2-hands-blocker-{index}"),
                    fixture.StorageProductId,
                    fixture.HandsId,
                    InventoryCondition.OpenBox,
                    InventoryUnitCost.Create("EUR", 7_000 + index).Value).IsSuccess, Is.True);
            }

            long assemblyRevision = fixture.Authority.Revision;
            long inventoryRevision = fixture.Inventory.Revision;
            int receiptCount = fixture.Authority.ReceiptCount;
            Assert.That(fixture.Authority.RemoveStorageDevice(
                    OperationId("operation.m2-full-hands-remove"),
                    fixture.StorageItemId,
                    fixture.StorageSlotId,
                    seatId,
                    3).Error,
                Is.EqualTo(AssemblyFailures.HandsCapacityExceeded));
            Assert.That(fixture.Authority.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(fixture.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Authority.ReceiptCount, Is.EqualTo(receiptCount));
            Assert.That(fixture.Authority.StorageSlotState,
                Is.EqualTo(StorageSlotState.StorageDeviceSeatedUnsecured));
            Assert.That(fixture.Inventory.TryGetSerializedItem(
                fixture.StorageItemId, out InventoryItemRecord seated), Is.True);
            Assert.That(seated.ContainerId, Is.EqualTo(fixture.StorageSlotContainerId));
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

        private sealed class StorageFixture
        {
            private StorageFixture()
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
            public StableId<AssemblySlotIdScope> StorageSlotId { get; private set; }
            public StableId<AssemblyStorageStandoffIdScope> StorageStandoffId { get; private set; }
            public StableId<AssemblyRetentionIdScope> StorageRetentionId { get; private set; }
            public StableId<AssemblyRetentionIdScope> StorageCaptiveScrewId =>
                StorageRetentionId;
            public StableId<ContainerIdScope> HandsId { get; private set; }
            public StableId<ContainerIdScope> WorkbenchId { get; private set; }
            public StableId<ContainerIdScope> ProcessorSocketContainerId { get; private set; }
            public StableId<ContainerIdScope> StorageSlotContainerId { get; private set; }
            public StableId<ContainerIdScope> StorageId { get; private set; }
            public StableId<ItemInstanceIdScope> MotherboardItemId { get; private set; }
            public StableId<ItemInstanceIdScope> ProcessorItemId { get; private set; }
            public StableId<ItemInstanceIdScope> StorageItemId { get; private set; }
            public StableId<ProductDefinitionIdScope> MotherboardProductId { get; private set; }
            public StableId<ProductDefinitionIdScope> ProcessorProductId { get; private set; }
            public StableId<ProductDefinitionIdScope> StorageProductId { get; private set; }
            public StableId<AssemblyOperationIdScope> AttachId { get; private set; }
            public StableId<AssemblyOperationIdScope> SecureId { get; private set; }
            public M2SlotDefinition StorageSlotDefinition { get; private set; }

            public StableId<AssemblySlotIdScope> MemorySlotId { get; private set; }
            public StableId<AssemblyRetentionIdScope> MemoryRetentionId { get; private set; }
            private StableId<AssemblyMemoryChannelIdScope> MemoryChannelId { get; set; }
            private StableId<AssemblyMemoryBankIdScope> MemoryBankId { get; set; }
            private StableId<ContainerIdScope> MemorySlotContainerId { get; set; }
            public StableId<ItemInstanceIdScope> MemoryItemId { get; private set; }
            private StableId<ProductDefinitionIdScope> MemoryProductId { get; set; }
            private DimmSlotDefinition MemorySlotDefinition { get; set; }

            public static StorageFixture Create()
            {
                StorageFixture fixture = CreateUnclaimed();
                fixture.Authority = fixture.TryCreateAuthority().Value;
                return fixture;
            }

            public static StorageFixture CreateUnclaimed(
                bool fillStorageSlot = false,
                int storageSlotCapacity = 1)
            {
                var fixture = new StorageFixture
                {
                    BuildId = StableId<PcBuildIdScope>.Parse("build.m2-storage-prototype"),
                    ChassisId = StableId<ChassisIdScope>.Parse("chassis.m2-storage-prototype"),
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
                    StorageSlotId = StableId<AssemblySlotIdScope>.Parse(
                        "assembly.slot.m2-primary"),
                    StorageStandoffId = StableId<AssemblyStorageStandoffIdScope>.Parse(
                        "assembly.standoff.m2-2280"),
                    StorageRetentionId = StableId<AssemblyRetentionIdScope>.Parse(
                        "assembly.retention.m2-captive-screw"),
                    HandsId = StableId<ContainerIdScope>.Parse("container.actor-hands"),
                    WorkbenchId = StableId<ContainerIdScope>.Parse(
                        "container.assembly-workbench"),
                    ProcessorSocketContainerId = StableId<ContainerIdScope>.Parse(
                        "container.processor-socket"),
                    MemorySlotContainerId = StableId<ContainerIdScope>.Parse(
                        "inventory.container.assembly-memory-a2"),
                    StorageSlotContainerId = StableId<ContainerIdScope>.Parse(
                        "inventory.container.assembly-m2-primary"),
                    StorageId = StableId<ContainerIdScope>.Parse("container.storage"),
                    MotherboardItemId = StableId<ItemInstanceIdScope>.Parse(
                        "item.motherboard-m2-fixture"),
                    ProcessorItemId = StableId<ItemInstanceIdScope>.Parse(
                        "item.processor-m2-fixture"),
                    MemoryItemId = StableId<ItemInstanceIdScope>.Parse(
                        "item.memory-m2-fixture"),
                    StorageItemId = StableId<ItemInstanceIdScope>.Parse(
                        "inventory.item.northstar-nvme-2280-001"),
                    MotherboardProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "component.motherboard-m2"),
                    ProcessorProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "component.processor-lga1700"),
                    MemoryProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "component.memory-ddr5"),
                    StorageProductId = StableId<ProductDefinitionIdScope>.Parse(
                        "catalog.storage.northstar-nvme-2280"),
                    AttachId = OperationId("operation.m2-fixture-attach"),
                    SecureId = OperationId("operation.m2-fixture-secure")
                };

                ProductDefinition motherboard = Definition(
                    fixture.MotherboardProductId,
                    "M.2 Motherboard");
                ProductDefinition processor = Definition(
                    fixture.ProcessorProductId,
                    "LGA1700 Processor");
                ProductDefinition memory = Definition(
                    fixture.MemoryProductId,
                    "DDR5 Memory");
                ProductDefinition storage = Definition(
                    fixture.StorageProductId,
                    "Northstar NVMe 2280");
                fixture.Products = ProductCatalog.Create(new[]
                {
                    motherboard,
                    processor,
                    memory,
                    storage
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
                            M2StorageType.NvmePcie4X4_2280).Value
                    }).Value;
                fixture.MemorySlotDefinition = DimmSlotDefinition.Create(
                    fixture.MemorySlotId,
                    fixture.MemoryRetentionId,
                    fixture.MemorySlotContainerId,
                    fixture.MemoryChannelId,
                    fixture.MemoryBankId,
                    1,
                    DimmType.Ddr5Udimm).Value;
                fixture.StorageSlotDefinition = M2SlotDefinition.Create(
                    fixture.StorageSlotId,
                    fixture.StorageStandoffId,
                    fixture.StorageRetentionId,
                    fixture.StorageSlotContainerId,
                    M2StorageType.NvmePcie4X4_2280).Value;

                fixture.Inventory = InventoryAuthority.Create(fixture.Products).Value;
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.HandsId,
                    InventoryContainerKind.ActorHands,
                    5).Value);
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
                    1).Value);
                fixture.Inventory.RegisterContainer(InventoryContainerDefinition.Create(
                    fixture.StorageSlotContainerId,
                    InventoryContainerKind.Workbench,
                    storageSlotCapacity).Value);
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
                fixture.Inventory.ReceiveSerializedItem(
                    fixture.StorageItemId,
                    fixture.StorageProductId,
                    fixture.HandsId,
                    InventoryCondition.New,
                    InventoryUnitCost.Create("EUR", 9_900).Value);
                if (fillStorageSlot)
                {
                    fixture.Inventory.ReceiveSerializedItem(
                        StableId<ItemInstanceIdScope>.Parse("item.storage-slot-occupied"),
                        fixture.StorageProductId,
                        fixture.StorageSlotContainerId,
                        InventoryCondition.OpenBox,
                        InventoryUnitCost.Create("EUR", 7_900).Value);
                }

                return fixture;
            }

            public OperationResult<AssemblyBuildAuthority> TryCreateAuthority()
            {
                return AssemblyBuildAuthority.CreateWithProcessorSocketMemorySlotAndStorageSlot(
                    Components,
                    Inventory,
                    BuildId,
                    ChassisId,
                    MotherboardSlotId,
                    FastenerId,
                    ProcessorSlotId,
                    ProcessorRetentionId,
                    MemorySlotDefinition,
                    StorageSlotDefinition,
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

            public void SeatAndRetainMemory(
                StableId<AssemblyOperationIdScope> seatId,
                StableId<AssemblyOperationIdScope> closeId)
            {
                Assert.That(Authority.SeatMemoryModule(
                    seatId,
                    MemoryItemId,
                    MemorySlotId,
                    DimmKeyOrientation.NotchAligned,
                    AttachId,
                    SecureId,
                    Authority.Revision).IsSuccess, Is.True);
                Assert.That(Authority.CloseMemoryRetention(
                    closeId,
                    MemoryItemId,
                    MemorySlotId,
                    MemoryRetentionId,
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
