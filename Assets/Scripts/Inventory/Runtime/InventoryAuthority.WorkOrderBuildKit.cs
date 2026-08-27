using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Inventory
{
    internal enum InventorySerializedReservationWorkOrderBuildKitStage
    {
        ActorHands = 1,
        BuildKit = 2,
        AssemblyHands = 3,
        AssemblyWorkbench = 4
    }

    /// <summary>
    /// Immutable Inventory proof for one exact reserved work-order line moving through the
    /// physical build-kit handoff. The reservation and parent allocation remain live.
    /// </summary>
    internal sealed class InventorySerializedReservationWorkOrderBuildKitReceipt
    {
        internal InventorySerializedReservationWorkOrderBuildKitReceipt(
            InventoryAuthority owner,
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            InventorySerializedTransferAccess buildKitAccess,
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId,
            InventorySerializedReservationWorkOrderBuildKitStage stage,
            long appliedRevision)
        {
            Owner = owner;
            Allocation = allocation;
            BuildKitAccess = buildKitAccess;
            OperationId = operationId;
            LineId = lineId;
            ProductId = productId;
            ItemId = itemId;
            ReservationId = reservationId;
            ComponentKind = componentKind;
            SourceContainerId = sourceContainerId;
            HandsContainerId = handsContainerId;
            BuildKitContainerId = buildKitContainerId;
            Stage = stage;
            AppliedRevision = appliedRevision;
        }

        internal InventoryAuthority Owner { get; }

        internal InventorySerializedReservationWorkOrderAllocationReceipt Allocation { get; }

        internal InventorySerializedTransferAccess BuildKitAccess { get; }

        internal StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
            OperationId { get; }

        internal StableId<InventorySerializedReservationWorkOrderLineIdScope> LineId { get; }

        internal StableId<ProductDefinitionIdScope> ProductId { get; }

        internal StableId<ItemInstanceIdScope> ItemId { get; }

        internal StableId<ReservationIdScope> ReservationId { get; }

        internal PcComponentKind ComponentKind { get; }

        internal StableId<ContainerIdScope> SourceContainerId { get; }

        internal StableId<ContainerIdScope> HandsContainerId { get; }

        internal StableId<ContainerIdScope> BuildKitContainerId { get; }

        internal InventorySerializedReservationWorkOrderBuildKitStage Stage { get; }

        internal long AppliedRevision { get; }
    }

    /// <summary>
    /// Immutable authorization proof for releasing the fully staged canonical motherboard
    /// into one exact managed Assembly workbench while its reservation remains live.
    /// </summary>
    internal sealed class
        InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt
    {
        internal InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt(
            InventoryAuthority owner,
            InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
            InventorySerializedTransferAccess assemblyAccess,
            StableId<
                InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                operationId,
            StableId<ContainerIdScope> workbenchContainerId,
            long appliedRevision)
        {
            Owner = owner;
            PlacementReceipt = placementReceipt;
            AssemblyAccess = assemblyAccess;
            OperationId = operationId;
            WorkbenchContainerId = workbenchContainerId;
            AppliedRevision = appliedRevision;
        }

        internal InventoryAuthority Owner { get; }

        internal InventorySerializedReservationWorkOrderBuildKitReceipt PlacementReceipt
        {
            get;
        }

        internal InventorySerializedTransferAccess AssemblyAccess { get; }

        internal StableId<
            InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
            OperationId { get; }

        internal StableId<ContainerIdScope> WorkbenchContainerId { get; }

        internal long AppliedRevision { get; }
    }

    /// <summary>
    /// Append-only custody evidence for each exact Assembly hands/workbench transition after
    /// the BuildKit release. Assembly owns installation state; Inventory still owns custody.
    /// </summary>
    internal sealed class
        InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt
    {
        internal InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt(
            InventoryAuthority owner,
            InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt handoff,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            InventorySerializedReservationWorkOrderBuildKitStage stage,
            long appliedRevision)
        {
            Owner = owner;
            Handoff = handoff;
            SourceContainerId = sourceContainerId;
            TargetContainerId = targetContainerId;
            Stage = stage;
            AppliedRevision = appliedRevision;
        }

        internal InventoryAuthority Owner { get; }

        internal InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt
            Handoff { get; }

        internal StableId<ContainerIdScope> SourceContainerId { get; }

        internal StableId<ContainerIdScope> TargetContainerId { get; }

        internal InventorySerializedReservationWorkOrderBuildKitStage Stage { get; }

        internal long AppliedRevision { get; }
    }

    internal sealed class InventorySerializedReservationWorkOrderBuildKitRegistration
    {
        private readonly List<
            InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt>
            _assemblyTransferReceipts =
                new List<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt>();

        internal InventorySerializedReservationWorkOrderBuildKitRegistration(
            InventorySerializedReservationWorkOrderBuildKitReceipt pickupReceipt)
        {
            PickupReceipt = pickupReceipt;
        }

        internal InventorySerializedReservationWorkOrderBuildKitReceipt PickupReceipt { get; }

        internal InventorySerializedReservationWorkOrderBuildKitReceipt PlacementReceipt
        {
            get;
            private set;
        }

        internal InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt
            AssemblyHandoffReceipt { get; private set; }

        internal IReadOnlyList<
            InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt>
            AssemblyTransferReceipts => _assemblyTransferReceipts;

        internal InventorySerializedReservationWorkOrderBuildKitStage CurrentStage =>
            PlacementReceipt == null
                ? InventorySerializedReservationWorkOrderBuildKitStage.ActorHands
                : AssemblyHandoffReceipt == null
                    ? InventorySerializedReservationWorkOrderBuildKitStage.BuildKit
                    : _assemblyTransferReceipts.Count == 0
                        ? InventorySerializedReservationWorkOrderBuildKitStage.AssemblyHands
                        : _assemblyTransferReceipts[_assemblyTransferReceipts.Count - 1].Stage;

        internal bool TryPublishPlacement(
            InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt)
        {
            if (PlacementReceipt != null ||
                placementReceipt == null ||
                placementReceipt.Stage !=
                InventorySerializedReservationWorkOrderBuildKitStage.BuildKit)
            {
                return false;
            }

            PlacementReceipt = placementReceipt;
            return true;
        }

        internal bool TryPublishAssemblyHandoff(
            InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt handoff)
        {
            if (PlacementReceipt == null ||
                AssemblyHandoffReceipt != null ||
                handoff == null ||
                !ReferenceEquals(handoff.PlacementReceipt, PlacementReceipt))
            {
                return false;
            }

            AssemblyHandoffReceipt = handoff;
            return true;
        }

        internal bool TryPublishAssemblyTransfer(
            InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt receipt)
        {
            if (AssemblyHandoffReceipt == null ||
                receipt == null ||
                !ReferenceEquals(receipt.Handoff, AssemblyHandoffReceipt))
            {
                return false;
            }

            bool fromHands = CurrentStage ==
                             InventorySerializedReservationWorkOrderBuildKitStage.AssemblyHands;
            StableId<ContainerIdScope> expectedSource = fromHands
                ? PlacementReceipt.HandsContainerId
                : AssemblyHandoffReceipt.WorkbenchContainerId;
            StableId<ContainerIdScope> expectedTarget = fromHands
                ? AssemblyHandoffReceipt.WorkbenchContainerId
                : PlacementReceipt.HandsContainerId;
            InventorySerializedReservationWorkOrderBuildKitStage expectedStage = fromHands
                ? InventorySerializedReservationWorkOrderBuildKitStage.AssemblyWorkbench
                : InventorySerializedReservationWorkOrderBuildKitStage.AssemblyHands;
            if (receipt.SourceContainerId != expectedSource ||
                receipt.TargetContainerId != expectedTarget ||
                receipt.Stage != expectedStage)
            {
                return false;
            }

            _assemblyTransferReceipts.Add(receipt);
            return true;
        }
    }

    public sealed partial class InventoryAuthority
    {
        private readonly Dictionary<
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>,
            InventorySerializedReservationWorkOrderBuildKitRegistration>
            _serializedReservationWorkOrderBuildKitsByOperation =
                new Dictionary<
                    StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>,
                    InventorySerializedReservationWorkOrderBuildKitRegistration>();
        private readonly Dictionary<StableId<ItemInstanceIdScope>,
            InventorySerializedReservationWorkOrderBuildKitRegistration>
            _serializedReservationWorkOrderBuildKitsByItem =
                new Dictionary<StableId<ItemInstanceIdScope>,
                    InventorySerializedReservationWorkOrderBuildKitRegistration>();
        private readonly Dictionary<StableId<ContainerIdScope>,
            InventorySerializedReservationWorkOrderBuildKitRegistration>
            _serializedReservationWorkOrderBuildKitsByContainer =
                new Dictionary<StableId<ContainerIdScope>,
                    InventorySerializedReservationWorkOrderBuildKitRegistration>();
        private readonly Dictionary<
            StableId<
                InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>,
            InventorySerializedReservationWorkOrderBuildKitRegistration>
            _serializedReservationWorkOrderBuildKitAssemblyHandoffsByOperation =
                new Dictionary<
                    StableId<
                        InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>,
                    InventorySerializedReservationWorkOrderBuildKitRegistration>();

        internal int SerializedReservationWorkOrderBuildKitCount =>
            _serializedReservationWorkOrderBuildKitsByOperation.Count;

        internal int SerializedReservationWorkOrderBuildKitAssemblyHandoffCount =>
            _serializedReservationWorkOrderBuildKitAssemblyHandoffsByOperation.Count;

        /// <summary>
        /// Moves one exact supported reserved PC component from its current un-managed stock/world
        /// container to ActorHands. Exact replay returns the original receipt without advancing
        /// Inventory Revision. The dedicated BuildKit capability and parent work-order
        /// allocation are bound before the first custody mutation.
        /// </summary>
        internal OperationResult<InventorySerializedReservationWorkOrderBuildKitReceipt>
            PickupReservedWorkOrderLineForBuildKit(
                InventorySerializedReservationWorkOrderAllocationReceipt allocation,
                InventorySerializedTransferAccess buildKitAccess,
                StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                    operationId,
                StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
                StableId<ProductDefinitionIdScope> productId,
                StableId<ItemInstanceIdScope> itemId,
                StableId<ReservationIdScope> reservationId,
                PcComponentKind componentKind,
                StableId<ContainerIdScope> sourceContainerId,
                StableId<ContainerIdScope> handsContainerId,
                StableId<ContainerIdScope> buildKitContainerId)
        {
            Failure inputFailure = ValidateWorkOrderBuildKitInput(
                allocation,
                buildKitAccess,
                operationId,
                lineId,
                productId,
                itemId,
                reservationId,
                componentKind,
                sourceContainerId,
                handsContainerId,
                buildKitContainerId);
            if (!inputFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(inputFailure);
            }

            if (_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    operationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration existing))
            {
                return MatchesWorkOrderBuildKitRegistration(
                           existing,
                           allocation,
                           buildKitAccess,
                           operationId,
                           lineId,
                           productId,
                           itemId,
                           reservationId,
                           componentKind,
                           sourceContainerId,
                           handsContainerId,
                           buildKitContainerId) &&
                       OwnsWorkOrderBuildKitRegistration(existing)
                    ? OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(
                        existing.PickupReceipt)
                    : OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                        InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            if (_serializedReservationWorkOrderBuildKitsByItem.ContainsKey(itemId) ||
                _serializedReservationWorkOrderBuildKitsByContainer.ContainsKey(
                    buildKitContainerId))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            InventoryItemRecord item = _items[itemId];
            if (item.ContainerId != sourceContainerId ||
                _managedSerializedTransferContainers.ContainsKey(item.ContainerId))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitStageInvalid);
            }

            Failure handsCapacityFailure = ValidateCapacity(handsContainerId, 1);
            if (!handsCapacityFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    handsCapacityFailure);
            }

            Failure buildKitCapacityFailure = ValidateCapacity(buildKitContainerId, 1);
            if (!buildKitCapacityFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    buildKitCapacityFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var receipt = new InventorySerializedReservationWorkOrderBuildKitReceipt(
                this,
                allocation,
                buildKitAccess,
                operationId,
                lineId,
                productId,
                itemId,
                reservationId,
                componentKind,
                item.ContainerId,
                handsContainerId,
                buildKitContainerId,
                InventorySerializedReservationWorkOrderBuildKitStage.ActorHands,
                Revision + 1);
            var registration =
                new InventorySerializedReservationWorkOrderBuildKitRegistration(receipt);

            _items[itemId] = MoveSerializedItem(item, handsContainerId);
            _serializedReservationWorkOrderBuildKitsByOperation.Add(operationId, registration);
            _serializedReservationWorkOrderBuildKitsByItem.Add(itemId, registration);
            _serializedReservationWorkOrderBuildKitsByContainer.Add(
                buildKitContainerId,
                registration);
            Revision++;
            return OperationResult<
                InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(receipt);
        }

        /// <summary>
        /// Commits the second custody leg from ActorHands to the exact managed capacity-one
        /// BuildKit container. The live reservation is preserved under the registration-bound
        /// invariant exception; no generic transfer or reservation rule is relaxed.
        /// </summary>
        internal OperationResult<InventorySerializedReservationWorkOrderBuildKitReceipt>
            PlaceReservedWorkOrderLineInBuildKit(
                InventorySerializedReservationWorkOrderBuildKitReceipt pickupReceipt)
        {
            if (!OwnsWorkOrderBuildKitReceipt(pickupReceipt) ||
                pickupReceipt.Stage !=
                InventorySerializedReservationWorkOrderBuildKitStage.ActorHands)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitReceiptInvalid);
            }

            InventorySerializedReservationWorkOrderBuildKitRegistration registration =
                _serializedReservationWorkOrderBuildKitsByOperation[
                    pickupReceipt.OperationId];
            if (registration.PlacementReceipt != null)
            {
                return OwnsWorkOrderBuildKitReceipt(registration.PlacementReceipt)
                    ? OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(
                        registration.PlacementReceipt)
                    : OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                        InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            if (registration.CurrentStage !=
                    InventorySerializedReservationWorkOrderBuildKitStage.ActorHands ||
                !_items.TryGetValue(
                    pickupReceipt.ItemId,
                    out InventoryItemRecord item) ||
                item.ContainerId != pickupReceipt.HandsContainerId)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitStageInvalid);
            }

            Failure capacityFailure = ValidateCapacity(
                pickupReceipt.BuildKitContainerId,
                1);
            if (!capacityFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    capacityFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var placementReceipt =
                new InventorySerializedReservationWorkOrderBuildKitReceipt(
                    this,
                    pickupReceipt.Allocation,
                    pickupReceipt.BuildKitAccess,
                    pickupReceipt.OperationId,
                    pickupReceipt.LineId,
                    pickupReceipt.ProductId,
                    pickupReceipt.ItemId,
                    pickupReceipt.ReservationId,
                    pickupReceipt.ComponentKind,
                    pickupReceipt.SourceContainerId,
                    pickupReceipt.HandsContainerId,
                    pickupReceipt.BuildKitContainerId,
                    InventorySerializedReservationWorkOrderBuildKitStage.BuildKit,
                    Revision + 1);
            if (!registration.TryPublishPlacement(placementReceipt))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            _items[item.Id] = MoveSerializedItem(item, pickupReceipt.BuildKitContainerId);
            Revision++;
            return OperationResult<
                InventorySerializedReservationWorkOrderBuildKitReceipt>.Success(
                placementReceipt);
        }

        /// <summary>
        /// Releases the exact staged motherboard from BuildKit to ActorHands and binds all
        /// subsequent reserved custody to the one managed workbench already owned by Assembly.
        /// Exact replay returns the original proof without advancing Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedMotherboardForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> workbenchContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.Motherboard,
                operationId,
                workbenchContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged processor from BuildKit to ActorHands and binds all
        /// subsequent reserved custody to the capacity-one managed processor socket already
        /// owned by Assembly. Exact replay returns the original proof without advancing
        /// Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedProcessorForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> processorSocketContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.Processor,
                operationId,
                processorSocketContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged DDR memory module from BuildKit to ActorHands and binds
        /// all subsequent reserved custody to the capacity-one managed memory slot already
        /// owned by Assembly. Exact replay returns the original proof without advancing
        /// Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedMemoryModuleForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> memorySlotContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.MemoryModule,
                operationId,
                memorySlotContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged M.2 storage device from BuildKit to ActorHands and
        /// binds all subsequent reserved custody to the capacity-one managed primary M.2
        /// slot already owned by Assembly. Exact replay returns the original proof without
        /// advancing Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedStorageForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> storageSlotContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.StorageDevice,
                operationId,
                storageSlotContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged processor cooler from BuildKit to ActorHands and binds
        /// all subsequent reserved custody to the capacity-one managed cooler slot already
        /// owned by Assembly. Exact replay returns the original proof without advancing
        /// Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedProcessorCoolerForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> processorCoolerSlotContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.ProcessorCooler,
                operationId,
                processorCoolerSlotContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged graphics card from BuildKit to ActorHands and binds
        /// all subsequent reserved custody to the capacity-one managed PCIe x16 slot
        /// already owned by Assembly. Exact replay returns the original proof without
        /// advancing Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedGraphicsCardForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> graphicsCardSlotContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.GraphicsCard,
                operationId,
                graphicsCardSlotContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged ATX power supply from BuildKit to ActorHands and binds
        /// all subsequent reserved custody to the capacity-one managed PSU bay already
        /// owned by Assembly. Exact replay returns the original proof without advancing
        /// Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedPowerSupplyForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> powerSupplyBayContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.PowerSupply,
                operationId,
                powerSupplyBayContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged ATX24 power cable from BuildKit to ActorHands and binds
        /// all subsequent reserved custody to the capacity-one managed ATX24 route container
        /// already owned by Assembly. The Custom-PC authority validates the exact cable family;
        /// Inventory preserves the exact line/product/item/reservation tuple carried by the
        /// staging receipt. Exact replay returns the original proof without advancing Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedAtx24PowerCableForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> routeContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.PowerCable,
                operationId,
                routeContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged EPS12V power cable from BuildKit to ActorHands and binds
        /// all subsequent reserved custody to the capacity-one managed CPU-power route
        /// container already owned by Assembly. The Custom-PC authority validates the exact
        /// cable family; Inventory preserves the exact line/product/item/reservation tuple
        /// carried by the staging receipt. Exact replay returns the original proof without
        /// advancing Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedEps12vPowerCableForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> routeContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.PowerCable,
                operationId,
                routeContainerId,
                expectedInventoryRevision);
        }

        /// <summary>
        /// Releases the exact staged PCIe/GPU 6+2 power cable from BuildKit to ActorHands and
        /// binds subsequent reserved custody to the capacity-one managed GPU-power route
        /// container already owned by Assembly. The exact cable family remains validated by
        /// Custom-PC authority and replay does not advance Inventory Revision.
        /// </summary>
        internal OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedPcieGpuPowerCableForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> routeContainerId,
                long expectedInventoryRevision)
        {
            return ReleaseReservedComponentForAssembly(
                placementReceipt,
                PcComponentKind.PowerCable,
                operationId,
                routeContainerId,
                expectedInventoryRevision);
        }

        private OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
            ReleaseReservedComponentForAssembly(
                InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
                PcComponentKind expectedComponentKind,
                StableId<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                    operationId,
                StableId<ContainerIdScope> workbenchContainerId,
                long expectedInventoryRevision)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures
                        .InvalidSerializedReservationWorkOrderBuildKitAssemblyOperationId);
            }

            if (_serializedReservationWorkOrderBuildKitAssemblyHandoffsByOperation.TryGetValue(
                    operationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration replay))
            {
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt receipt =
                    replay.AssemblyHandoffReceipt;
                return MatchesWorkOrderBuildKitAssemblyHandoff(
                           replay,
                           placementReceipt,
                           expectedComponentKind,
                           operationId,
                           workbenchContainerId) &&
                       OwnsWorkOrderBuildKitRegistration(replay)
                    ? OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                        .Success(receipt)
                    : OperationResult<
                        InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>
                        .Fail(
                            InventoryFailures
                                .SerializedReservationWorkOrderBuildKitAssemblyConflict);
            }

            if (expectedInventoryRevision != Revision)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures.SerializedTransferPlanStale);
            }

            if (!OwnsWorkOrderBuildKitReceipt(placementReceipt) ||
                placementReceipt.Stage !=
                    InventorySerializedReservationWorkOrderBuildKitStage.BuildKit ||
                placementReceipt.ComponentKind != expectedComponentKind ||
                !_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    placementReceipt.OperationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration registration) ||
                !ReferenceEquals(registration.PlacementReceipt, placementReceipt))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures
                        .SerializedReservationWorkOrderBuildKitAssemblyStageInvalid);
            }

            if (registration.AssemblyHandoffReceipt != null)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures
                        .SerializedReservationWorkOrderBuildKitAssemblyConflict);
            }

            if (workbenchContainerId.IsEmpty ||
                workbenchContainerId == placementReceipt.HandsContainerId ||
                workbenchContainerId == placementReceipt.BuildKitContainerId ||
                !_containers.TryGetValue(
                    workbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench ||
                !_managedSerializedTransferContainers.TryGetValue(
                    workbenchContainerId,
                    out InventorySerializedTransferAccess assemblyAccess) ||
                !ValidateSerializedTransferAccess(
                        placementReceipt.HandsContainerId,
                        workbenchContainerId,
                        assemblyAccess).IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures
                        .SerializedReservationWorkOrderBuildKitAssemblyWorkbenchInvalid);
            }

            if (registration.CurrentStage !=
                    InventorySerializedReservationWorkOrderBuildKitStage.BuildKit ||
                !_items.TryGetValue(
                    placementReceipt.ItemId,
                    out InventoryItemRecord item) ||
                item.ContainerId != placementReceipt.BuildKitContainerId ||
                GetContainerLoadUnsafe(placementReceipt.BuildKitContainerId) != 1)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures
                        .SerializedReservationWorkOrderBuildKitAssemblyStageInvalid);
            }

            Failure handsCapacityFailure = ValidateCapacity(
                placementReceipt.HandsContainerId,
                1);
            if (!handsCapacityFailure.IsNone)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    handsCapacityFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var handoff =
                new InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt(
                    this,
                    placementReceipt,
                    assemblyAccess,
                    operationId,
                    workbenchContainerId,
                    Revision + 1);
            if (!registration.TryPublishAssemblyHandoff(handoff))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Fail(
                    InventoryFailures
                        .SerializedReservationWorkOrderBuildKitAssemblyConflict);
            }

            _serializedReservationWorkOrderBuildKitAssemblyHandoffsByOperation.Add(
                operationId,
                registration);
            _items[item.Id] = MoveSerializedItem(item, placementReceipt.HandsContainerId);
            Revision++;
            return OperationResult<
                InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt>.Success(
                handoff);
        }

        internal bool OwnsWorkOrderBuildKitReceipt(
            InventorySerializedReservationWorkOrderBuildKitReceipt receipt)
        {
            if (receipt == null ||
                !ReferenceEquals(receipt.Owner, this) ||
                receipt.AppliedRevision <= 0 ||
                receipt.AppliedRevision > Revision ||
                !_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    receipt.OperationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration
                        registration) ||
                !OwnsWorkOrderBuildKitRegistration(registration))
            {
                return false;
            }

            return receipt.Stage ==
                       InventorySerializedReservationWorkOrderBuildKitStage.ActorHands
                    ? ReferenceEquals(receipt, registration.PickupReceipt)
                    : receipt.Stage ==
                          InventorySerializedReservationWorkOrderBuildKitStage.BuildKit &&
                      ReferenceEquals(receipt, registration.PlacementReceipt);
        }

        internal bool OwnsWorkOrderBuildKitAssemblyHandoffReceipt(
            InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt receipt)
        {
            return receipt != null &&
                   ReferenceEquals(receipt.Owner, this) &&
                   receipt.AppliedRevision > 0 &&
                   receipt.AppliedRevision <= Revision &&
                   _serializedReservationWorkOrderBuildKitAssemblyHandoffsByOperation.TryGetValue(
                       receipt.OperationId,
                       out InventorySerializedReservationWorkOrderBuildKitRegistration
                           registration) &&
                   ReferenceEquals(receipt, registration.AssemblyHandoffReceipt) &&
                   OwnsWorkOrderBuildKitRegistration(registration);
        }

        internal bool TryGetWorkOrderBuildKitReceipt(
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            out InventorySerializedReservationWorkOrderBuildKitReceipt receipt)
        {
            if (_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    operationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration registration) &&
                OwnsWorkOrderBuildKitRegistration(registration))
            {
                receipt = registration.PlacementReceipt ?? registration.PickupReceipt;
                return true;
            }

            receipt = null;
            return false;
        }

        private Failure ValidateWorkOrderBuildKitInput(
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            InventorySerializedTransferAccess buildKitAccess,
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId)
        {
            if (!OwnsSerializedReservationWorkOrderAllocation(allocation))
            {
                return InventoryFailures.SerializedReservationWorkOrderReceiptInvalid;
            }

            if (operationId.IsEmpty)
            {
                return InventoryFailures
                    .InvalidSerializedReservationWorkOrderBuildKitOperationId;
            }

            if (lineId.IsEmpty)
            {
                return InventoryFailures.InvalidSerializedReservationWorkOrderLineId;
            }

            if (productId.IsEmpty || itemId.IsEmpty || reservationId.IsEmpty ||
                !IsSupportedWorkOrderBuildKitComponent(componentKind))
            {
                return InventoryFailures.SerializedReservationWorkOrderBuildKitLineInvalid;
            }

            if (sourceContainerId.IsEmpty ||
                !_containers.TryGetValue(
                    sourceContainerId,
                    out InventoryContainerDefinition source) ||
                source.Kind != InventoryContainerKind.WorldFloor ||
                handsContainerId.IsEmpty ||
                !_containers.TryGetValue(
                    handsContainerId,
                    out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands ||
                hands.UnitCapacity != 1 ||
                buildKitContainerId.IsEmpty ||
                !_containers.TryGetValue(
                    buildKitContainerId,
                    out InventoryContainerDefinition buildKit) ||
                buildKit.Kind != InventoryContainerKind.BuildKit ||
                buildKit.UnitCapacity != 1 ||
                sourceContainerId == handsContainerId ||
                sourceContainerId == buildKitContainerId ||
                handsContainerId == buildKitContainerId)
            {
                return InventoryFailures.SerializedReservationWorkOrderBuildKitContainerInvalid;
            }

            Failure accessFailure = ValidateSerializedTransferAccess(
                handsContainerId,
                buildKitContainerId,
                buildKitAccess);
            if (!accessFailure.IsNone)
            {
                return accessFailure;
            }

            if (!_items.TryGetValue(itemId, out InventoryItemRecord item) ||
                item.ProductId != productId ||
                !_reservations.TryGetValue(
                    reservationId,
                    out InventoryReservation reservation) ||
                reservation.TargetKind != InventoryReservationTargetKind.SerializedItem ||
                reservation.ItemId != itemId ||
                reservation.ClaimId != allocation.ClaimId ||
                reservation.Quantity != 1 ||
                !ContainsExactWorkOrderAllocationRequest(
                    allocation,
                    reservationId,
                    itemId))
            {
                return InventoryFailures.SerializedReservationWorkOrderBuildKitLineInvalid;
            }

            return Failure.None;
        }

        private bool OwnsWorkOrderBuildKitRegistration(
            InventorySerializedReservationWorkOrderBuildKitRegistration registration)
        {
            InventorySerializedReservationWorkOrderBuildKitReceipt pickup =
                registration?.PickupReceipt;
            if (pickup == null ||
                pickup.Stage !=
                    InventorySerializedReservationWorkOrderBuildKitStage.ActorHands ||
                pickup.AppliedRevision <= 0 ||
                pickup.AppliedRevision > Revision ||
                pickup.OperationId.IsEmpty ||
                pickup.LineId.IsEmpty ||
                pickup.ProductId.IsEmpty ||
                pickup.ItemId.IsEmpty ||
                pickup.ReservationId.IsEmpty ||
                !IsSupportedWorkOrderBuildKitComponent(pickup.ComponentKind) ||
                pickup.SourceContainerId.IsEmpty ||
                pickup.HandsContainerId.IsEmpty ||
                pickup.BuildKitContainerId.IsEmpty ||
                pickup.SourceContainerId == pickup.HandsContainerId ||
                pickup.SourceContainerId == pickup.BuildKitContainerId ||
                pickup.HandsContainerId == pickup.BuildKitContainerId ||
                !OwnsSerializedReservationWorkOrderAllocation(pickup.Allocation) ||
                !_containers.TryGetValue(
                    pickup.SourceContainerId,
                    out InventoryContainerDefinition source) ||
                source.Kind != InventoryContainerKind.WorldFloor ||
                _managedSerializedTransferContainers.ContainsKey(
                    pickup.SourceContainerId) ||
                !_containers.TryGetValue(
                    pickup.HandsContainerId,
                    out InventoryContainerDefinition hands) ||
                hands.Kind != InventoryContainerKind.ActorHands ||
                hands.UnitCapacity != 1 ||
                !_containers.TryGetValue(
                    pickup.BuildKitContainerId,
                    out InventoryContainerDefinition buildKit) ||
                buildKit.Kind != InventoryContainerKind.BuildKit ||
                buildKit.UnitCapacity != 1 ||
                !ValidateSerializedTransferAccess(
                        pickup.HandsContainerId,
                        pickup.BuildKitContainerId,
                        pickup.BuildKitAccess).IsNone ||
                !_items.TryGetValue(
                    pickup.ItemId,
                    out InventoryItemRecord item) ||
                item.ProductId != pickup.ProductId ||
                !_reservations.TryGetValue(
                    pickup.ReservationId,
                    out InventoryReservation reservation) ||
                reservation.TargetKind != InventoryReservationTargetKind.SerializedItem ||
                reservation.ItemId != pickup.ItemId ||
                reservation.ClaimId != pickup.Allocation.ClaimId ||
                reservation.Quantity != 1 ||
                !ContainsExactWorkOrderAllocationRequest(
                    pickup.Allocation,
                    pickup.ReservationId,
                    pickup.ItemId) ||
                !_serializedReservationWorkOrderBuildKitsByOperation.TryGetValue(
                    pickup.OperationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration byOperation) ||
                !_serializedReservationWorkOrderBuildKitsByItem.TryGetValue(
                    pickup.ItemId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration byItem) ||
                !_serializedReservationWorkOrderBuildKitsByContainer.TryGetValue(
                    pickup.BuildKitContainerId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration byContainer) ||
                !ReferenceEquals(registration, byOperation) ||
                !ReferenceEquals(registration, byItem) ||
                !ReferenceEquals(registration, byContainer))
            {
                return false;
            }

            InventorySerializedReservationWorkOrderBuildKitReceipt placement =
                registration.PlacementReceipt;
            if (placement == null)
            {
                return registration.CurrentStage ==
                           InventorySerializedReservationWorkOrderBuildKitStage.ActorHands &&
                       item.ContainerId == pickup.HandsContainerId &&
                       GetContainerLoadUnsafe(pickup.BuildKitContainerId) == 0;
            }

            if (placement.Stage !=
                    InventorySerializedReservationWorkOrderBuildKitStage.BuildKit ||
                placement.AppliedRevision <= pickup.AppliedRevision ||
                placement.AppliedRevision > Revision ||
                !MatchesWorkOrderBuildKitReceiptIdentity(pickup, placement))
            {
                return false;
            }

            InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt handoff =
                registration.AssemblyHandoffReceipt;
            if (handoff == null)
            {
                return registration.CurrentStage ==
                           InventorySerializedReservationWorkOrderBuildKitStage.BuildKit &&
                       item.ContainerId == pickup.BuildKitContainerId &&
                       GetContainerLoadUnsafe(pickup.BuildKitContainerId) == 1 &&
                       registration.AssemblyTransferReceipts.Count == 0;
            }

            if ((pickup.ComponentKind != PcComponentKind.Motherboard &&
                 pickup.ComponentKind != PcComponentKind.Processor &&
                 pickup.ComponentKind != PcComponentKind.MemoryModule &&
                 pickup.ComponentKind != PcComponentKind.StorageDevice &&
                 pickup.ComponentKind != PcComponentKind.ProcessorCooler &&
                 pickup.ComponentKind != PcComponentKind.GraphicsCard &&
                 pickup.ComponentKind != PcComponentKind.PowerSupply &&
                 pickup.ComponentKind != PcComponentKind.PowerCable) ||
                !ReferenceEquals(handoff.Owner, this) ||
                !ReferenceEquals(handoff.PlacementReceipt, placement) ||
                handoff.OperationId.IsEmpty ||
                handoff.WorkbenchContainerId.IsEmpty ||
                handoff.WorkbenchContainerId == pickup.HandsContainerId ||
                handoff.WorkbenchContainerId == pickup.BuildKitContainerId ||
                handoff.AppliedRevision <= placement.AppliedRevision ||
                handoff.AppliedRevision > Revision ||
                !_containers.TryGetValue(
                    handoff.WorkbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench ||
                !_managedSerializedTransferContainers.TryGetValue(
                    handoff.WorkbenchContainerId,
                    out InventorySerializedTransferAccess workbenchAccess) ||
                !ReferenceEquals(workbenchAccess, handoff.AssemblyAccess) ||
                !ValidateSerializedTransferAccess(
                        pickup.HandsContainerId,
                        handoff.WorkbenchContainerId,
                        handoff.AssemblyAccess).IsNone ||
                !_serializedReservationWorkOrderBuildKitAssemblyHandoffsByOperation.TryGetValue(
                    handoff.OperationId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration
                        byAssemblyOperation) ||
                !ReferenceEquals(registration, byAssemblyOperation) ||
                GetContainerLoadUnsafe(pickup.BuildKitContainerId) != 0)
            {
                return false;
            }

            StableId<ContainerIdScope> expectedContainerId = pickup.HandsContainerId;
            long priorAppliedRevision = handoff.AppliedRevision;
            IReadOnlyList<
                InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt>
                transitions = registration.AssemblyTransferReceipts;
            for (int index = 0; index < transitions.Count; index++)
            {
                InventorySerializedReservationWorkOrderBuildKitAssemblyTransferReceipt
                    transition = transitions[index];
                bool fromHands = expectedContainerId == pickup.HandsContainerId;
                StableId<ContainerIdScope> expectedTargetId = fromHands
                    ? handoff.WorkbenchContainerId
                    : pickup.HandsContainerId;
                InventorySerializedReservationWorkOrderBuildKitStage expectedStage = fromHands
                    ? InventorySerializedReservationWorkOrderBuildKitStage.AssemblyWorkbench
                    : InventorySerializedReservationWorkOrderBuildKitStage.AssemblyHands;
                if (transition == null ||
                    !ReferenceEquals(transition.Owner, this) ||
                    !ReferenceEquals(transition.Handoff, handoff) ||
                    transition.SourceContainerId != expectedContainerId ||
                    transition.TargetContainerId != expectedTargetId ||
                    transition.Stage != expectedStage ||
                    transition.AppliedRevision <= priorAppliedRevision ||
                    transition.AppliedRevision > Revision)
                {
                    return false;
                }

                expectedContainerId = transition.TargetContainerId;
                priorAppliedRevision = transition.AppliedRevision;
            }

            InventorySerializedReservationWorkOrderBuildKitStage expectedCurrentStage =
                expectedContainerId == pickup.HandsContainerId
                    ? InventorySerializedReservationWorkOrderBuildKitStage.AssemblyHands
                    : InventorySerializedReservationWorkOrderBuildKitStage.AssemblyWorkbench;
            return registration.CurrentStage == expectedCurrentStage &&
                   item.ContainerId == expectedContainerId;
        }

        private bool HasValidSerializedReservationWorkOrderBuildKits()
        {
            int count = _serializedReservationWorkOrderBuildKitsByOperation.Count;
            if (_serializedReservationWorkOrderBuildKitsByItem.Count != count ||
                _serializedReservationWorkOrderBuildKitsByContainer.Count != count)
            {
                return false;
            }

            int assemblyHandoffCount = 0;

            foreach (InventorySerializedReservationWorkOrderBuildKitRegistration registration in
                     _serializedReservationWorkOrderBuildKitsByOperation.Values)
            {
                if (!OwnsWorkOrderBuildKitRegistration(registration))
                {
                    return false;
                }

                if (registration.AssemblyHandoffReceipt != null)
                {
                    assemblyHandoffCount++;
                }
            }

            return assemblyHandoffCount ==
                   _serializedReservationWorkOrderBuildKitAssemblyHandoffsByOperation.Count;
        }

        private bool IsValidReservedSerializedWorkOrderBuildKitCustody(
            InventoryReservation reservation,
            InventoryItemRecord item)
        {
            return reservation != null &&
                   item != null &&
                   _serializedReservationWorkOrderBuildKitsByItem.TryGetValue(
                       item.Id,
                       out InventorySerializedReservationWorkOrderBuildKitRegistration
                           registration) &&
                   registration.PlacementReceipt != null &&
                   registration.PlacementReceipt.ReservationId == reservation.Id &&
                   (registration.AssemblyHandoffReceipt == null
                       ? registration.PlacementReceipt.BuildKitContainerId == item.ContainerId
                       : (registration.CurrentStage ==
                              InventorySerializedReservationWorkOrderBuildKitStage.AssemblyHands &&
                          registration.PlacementReceipt.HandsContainerId == item.ContainerId) ||
                         (registration.CurrentStage ==
                              InventorySerializedReservationWorkOrderBuildKitStage
                                  .AssemblyWorkbench &&
                          registration.AssemblyHandoffReceipt.WorkbenchContainerId ==
                              item.ContainerId)) &&
                   OwnsWorkOrderBuildKitRegistration(registration);
        }

        private bool TryGetAuthorizedWorkOrderBuildKitAssemblyTransfer(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> targetContainerId,
            InventorySerializedTransferAccess access,
            out InventorySerializedReservationWorkOrderBuildKitRegistration registration)
        {
            registration = null;
            if (!_serializedReservationWorkOrderBuildKitsByItem.TryGetValue(
                    itemId,
                    out InventorySerializedReservationWorkOrderBuildKitRegistration candidate) ||
                candidate.AssemblyHandoffReceipt == null ||
                !OwnsWorkOrderBuildKitRegistration(candidate) ||
                !_items.TryGetValue(itemId, out InventoryItemRecord item))
            {
                return false;
            }

            InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt handoff =
                candidate.AssemblyHandoffReceipt;
            bool handsToWorkbench =
                candidate.CurrentStage ==
                    InventorySerializedReservationWorkOrderBuildKitStage.AssemblyHands &&
                item.ContainerId == candidate.PlacementReceipt.HandsContainerId &&
                targetContainerId == handoff.WorkbenchContainerId;
            bool workbenchToHands =
                candidate.CurrentStage ==
                    InventorySerializedReservationWorkOrderBuildKitStage.AssemblyWorkbench &&
                item.ContainerId == handoff.WorkbenchContainerId &&
                targetContainerId == candidate.PlacementReceipt.HandsContainerId;
            if ((!handsToWorkbench && !workbenchToHands) ||
                !ReferenceEquals(access, handoff.AssemblyAccess))
            {
                return false;
            }

            registration = candidate;
            return true;
        }

        private static bool ContainsExactWorkOrderAllocationRequest(
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            StableId<ReservationIdScope> reservationId,
            StableId<ItemInstanceIdScope> itemId)
        {
            if (allocation?.Requests == null)
            {
                return false;
            }

            int matchCount = 0;
            for (int index = 0; index < allocation.Requests.Count; index++)
            {
                InventorySerializedReservationRequest request = allocation.Requests[index];
                if (request != null &&
                    request.ReservationId == reservationId &&
                    request.ClaimId == allocation.ClaimId &&
                    request.ItemId == itemId)
                {
                    matchCount++;
                }
            }

            return matchCount == 1;
        }

        private static bool IsSupportedWorkOrderBuildKitComponent(
            PcComponentKind componentKind)
        {
            return componentKind == PcComponentKind.Motherboard ||
                   componentKind == PcComponentKind.Processor ||
                   componentKind == PcComponentKind.MemoryModule ||
                   componentKind == PcComponentKind.StorageDevice ||
                   componentKind == PcComponentKind.ProcessorCooler ||
                   componentKind == PcComponentKind.GraphicsCard ||
                   componentKind == PcComponentKind.PowerSupply ||
                   componentKind == PcComponentKind.PowerCable;
        }

        private static InventoryItemRecord MoveSerializedItem(
            InventoryItemRecord item,
            StableId<ContainerIdScope> targetContainerId)
        {
            return new InventoryItemRecord(
                item.Id,
                item.ProductId,
                targetContainerId,
                item.Condition,
                item.UnitCost,
                item.StateFlags);
        }

        private static bool MatchesWorkOrderBuildKitRegistration(
            InventorySerializedReservationWorkOrderBuildKitRegistration registration,
            InventorySerializedReservationWorkOrderAllocationReceipt allocation,
            InventorySerializedTransferAccess buildKitAccess,
            StableId<InventorySerializedReservationWorkOrderBuildKitOperationIdScope>
                operationId,
            StableId<InventorySerializedReservationWorkOrderLineIdScope> lineId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ReservationIdScope> reservationId,
            PcComponentKind componentKind,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> handsContainerId,
            StableId<ContainerIdScope> buildKitContainerId)
        {
            InventorySerializedReservationWorkOrderBuildKitReceipt pickup =
                registration?.PickupReceipt;
            return pickup != null &&
                   ReferenceEquals(pickup.Allocation, allocation) &&
                   ReferenceEquals(pickup.BuildKitAccess, buildKitAccess) &&
                   pickup.OperationId == operationId &&
                   pickup.LineId == lineId &&
                   pickup.ProductId == productId &&
                   pickup.ItemId == itemId &&
                   pickup.ReservationId == reservationId &&
                   pickup.ComponentKind == componentKind &&
                   pickup.SourceContainerId == sourceContainerId &&
                   pickup.HandsContainerId == handsContainerId &&
                   pickup.BuildKitContainerId == buildKitContainerId;
        }

        private static bool MatchesWorkOrderBuildKitReceiptIdentity(
            InventorySerializedReservationWorkOrderBuildKitReceipt expected,
            InventorySerializedReservationWorkOrderBuildKitReceipt actual)
        {
            return expected != null &&
                   actual != null &&
                   ReferenceEquals(expected.Owner, actual.Owner) &&
                   ReferenceEquals(expected.Allocation, actual.Allocation) &&
                   ReferenceEquals(expected.BuildKitAccess, actual.BuildKitAccess) &&
                   expected.OperationId == actual.OperationId &&
                   expected.LineId == actual.LineId &&
                   expected.ProductId == actual.ProductId &&
                   expected.ItemId == actual.ItemId &&
                   expected.ReservationId == actual.ReservationId &&
                   expected.ComponentKind == actual.ComponentKind &&
                   expected.SourceContainerId == actual.SourceContainerId &&
                   expected.HandsContainerId == actual.HandsContainerId &&
                   expected.BuildKitContainerId == actual.BuildKitContainerId;
        }

        private static bool MatchesWorkOrderBuildKitAssemblyHandoff(
            InventorySerializedReservationWorkOrderBuildKitRegistration registration,
            InventorySerializedReservationWorkOrderBuildKitReceipt placementReceipt,
            PcComponentKind expectedComponentKind,
            StableId<
                InventorySerializedReservationWorkOrderBuildKitAssemblyOperationIdScope>
                operationId,
            StableId<ContainerIdScope> workbenchContainerId)
        {
            InventorySerializedReservationWorkOrderBuildKitAssemblyHandoffReceipt handoff =
                registration?.AssemblyHandoffReceipt;
            return handoff != null &&
                   handoff.PlacementReceipt.ComponentKind == expectedComponentKind &&
                   ReferenceEquals(handoff.PlacementReceipt, placementReceipt) &&
                   handoff.OperationId == operationId &&
                   handoff.WorkbenchContainerId == workbenchContainerId;
        }
    }
}
