using System;
using System.Collections.Generic;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PSE.Retail")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PSE.Assembly")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PSE.Orders")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PCShopEmpire3D.EditModeTests")]

namespace PCShopEmpire3D.Inventory
{
    internal sealed class InventorySerializedTransferAccess
    {
        internal InventorySerializedTransferAccess(
            InventoryAuthority owner,
            StableId<ContainerIdScope> managedContainerId)
        {
            Owner = owner;
            ManagedContainerId = managedContainerId;
        }

        internal InventoryAuthority Owner { get; }

        internal StableId<ContainerIdScope> ManagedContainerId { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns two distinct managed
    /// serialized-item containers. The pair is internal so Inventory remains the only
    /// public stock authority.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessPair
    {
        internal InventorySerializedTransferAccessPair(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second)
        {
            First = first;
            Second = second;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns three distinct managed
    /// serialized-item containers. All validation completes before any capability is
    /// published so a failed claim can never leave partial managed custody behind.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessTriple
    {
        internal InventorySerializedTransferAccessTriple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third)
        {
            First = first;
            Second = second;
            Third = third;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns four distinct managed
    /// serialized-item containers. Validation is all-or-none and advances Inventory by
    /// exactly one revision only after all four capabilities can be published together.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessQuadruple
    {
        internal InventorySerializedTransferAccessQuadruple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third,
            InventorySerializedTransferAccess fourth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }

        internal InventorySerializedTransferAccess Fourth { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns five distinct managed
    /// serialized-item containers. Validation is all-or-none and advances Inventory by
    /// exactly one revision only after all five capabilities can be published together.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessQuintuple
    {
        internal InventorySerializedTransferAccessQuintuple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third,
            InventorySerializedTransferAccess fourth,
            InventorySerializedTransferAccess fifth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }

        internal InventorySerializedTransferAccess Fourth { get; }

        internal InventorySerializedTransferAccess Fifth { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns six distinct managed
    /// serialized-item containers. Validation is all-or-none and advances Inventory by
    /// exactly one revision only after all six capabilities can be published together.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessSextuple
    {
        internal InventorySerializedTransferAccessSextuple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third,
            InventorySerializedTransferAccess fourth,
            InventorySerializedTransferAccess fifth,
            InventorySerializedTransferAccess sixth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }

        internal InventorySerializedTransferAccess Fourth { get; }

        internal InventorySerializedTransferAccess Fifth { get; }

        internal InventorySerializedTransferAccess Sixth { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns seven distinct managed
    /// serialized-item containers. Validation is all-or-none and advances Inventory by
    /// exactly one revision only after every capability can be published together.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessSeptuple
    {
        internal InventorySerializedTransferAccessSeptuple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third,
            InventorySerializedTransferAccess fourth,
            InventorySerializedTransferAccess fifth,
            InventorySerializedTransferAccess sixth,
            InventorySerializedTransferAccess seventh)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }

        internal InventorySerializedTransferAccess Fourth { get; }

        internal InventorySerializedTransferAccess Fifth { get; }

        internal InventorySerializedTransferAccess Sixth { get; }

        internal InventorySerializedTransferAccess Seventh { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns eight distinct managed
    /// serialized-item containers. Validation is all-or-none and advances Inventory by
    /// exactly one revision only after every capability can be published together.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessOctuple
    {
        internal InventorySerializedTransferAccessOctuple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third,
            InventorySerializedTransferAccess fourth,
            InventorySerializedTransferAccess fifth,
            InventorySerializedTransferAccess sixth,
            InventorySerializedTransferAccess seventh,
            InventorySerializedTransferAccess eighth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
            Eighth = eighth;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }

        internal InventorySerializedTransferAccess Fourth { get; }

        internal InventorySerializedTransferAccess Fifth { get; }

        internal InventorySerializedTransferAccess Sixth { get; }

        internal InventorySerializedTransferAccess Seventh { get; }

        internal InventorySerializedTransferAccess Eighth { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns nine distinct managed
    /// serialized-item containers. Validation is all-or-none and advances Inventory by
    /// exactly one revision only after every capability can be published together.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessNonuple
    {
        internal InventorySerializedTransferAccessNonuple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third,
            InventorySerializedTransferAccess fourth,
            InventorySerializedTransferAccess fifth,
            InventorySerializedTransferAccess sixth,
            InventorySerializedTransferAccess seventh,
            InventorySerializedTransferAccess eighth,
            InventorySerializedTransferAccess ninth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
            Eighth = eighth;
            Ninth = ninth;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }

        internal InventorySerializedTransferAccess Fourth { get; }

        internal InventorySerializedTransferAccess Fifth { get; }

        internal InventorySerializedTransferAccess Sixth { get; }

        internal InventorySerializedTransferAccess Seventh { get; }

        internal InventorySerializedTransferAccess Eighth { get; }

        internal InventorySerializedTransferAccess Ninth { get; }
    }

    /// <summary>
    /// Atomically issued capabilities for an aggregate that owns ten distinct managed
    /// serialized-item containers. Validation is all-or-none and advances Inventory by
    /// exactly one revision only after every capability can be published together.
    /// </summary>
    internal sealed class InventorySerializedTransferAccessDecuple
    {
        internal InventorySerializedTransferAccessDecuple(
            InventorySerializedTransferAccess first,
            InventorySerializedTransferAccess second,
            InventorySerializedTransferAccess third,
            InventorySerializedTransferAccess fourth,
            InventorySerializedTransferAccess fifth,
            InventorySerializedTransferAccess sixth,
            InventorySerializedTransferAccess seventh,
            InventorySerializedTransferAccess eighth,
            InventorySerializedTransferAccess ninth,
            InventorySerializedTransferAccess tenth)
        {
            First = first;
            Second = second;
            Third = third;
            Fourth = fourth;
            Fifth = fifth;
            Sixth = sixth;
            Seventh = seventh;
            Eighth = eighth;
            Ninth = ninth;
            Tenth = tenth;
        }

        internal InventorySerializedTransferAccess First { get; }

        internal InventorySerializedTransferAccess Second { get; }

        internal InventorySerializedTransferAccess Third { get; }

        internal InventorySerializedTransferAccess Fourth { get; }

        internal InventorySerializedTransferAccess Fifth { get; }

        internal InventorySerializedTransferAccess Sixth { get; }

        internal InventorySerializedTransferAccess Seventh { get; }

        internal InventorySerializedTransferAccess Eighth { get; }

        internal InventorySerializedTransferAccess Ninth { get; }

        internal InventorySerializedTransferAccess Tenth { get; }
    }

    /// <summary>
    /// Immutable, revision-bound permission to move one exact serialized item between
    /// two logical containers. Only the authority that prepared the plan may commit it.
    /// </summary>
    public sealed class InventorySerializedTransferPlan
    {
        internal InventorySerializedTransferPlan(
            InventoryAuthority owner,
            long expectedRevision,
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            InventorySerializedTransferAccess access = null,
            InventorySerializedItemStateFlags requiredAbsentStateFlags =
                InventorySerializedItemStateFlags.None,
            InventorySerializedItemStateFlags stateFlagsToAdd =
                InventorySerializedItemStateFlags.None)
        {
            Owner = owner;
            ExpectedRevision = expectedRevision;
            ItemId = itemId;
            SourceContainerId = sourceContainerId;
            TargetContainerId = targetContainerId;
            Access = access;
            RequiredAbsentStateFlags = requiredAbsentStateFlags;
            StateFlagsToAdd = stateFlagsToAdd;
        }

        internal InventoryAuthority Owner { get; }

        internal InventorySerializedTransferAccess Access { get; }

        internal InventorySerializedItemStateFlags RequiredAbsentStateFlags { get; }

        internal InventorySerializedItemStateFlags StateFlagsToAdd { get; }

        public long ExpectedRevision { get; }

        public StableId<ItemInstanceIdScope> ItemId { get; }

        public StableId<ContainerIdScope> SourceContainerId { get; }

        public StableId<ContainerIdScope> TargetContainerId { get; }
    }

    /// <summary>
    /// Immutable, revision-bound permission to reserve one exact serialized item. The plan
    /// contains no mutable state and can only be committed by the authority that prepared it.
    /// </summary>
    public sealed class InventorySerializedReservationPlan
    {
        internal InventorySerializedReservationPlan(
            InventoryAuthority owner,
            long expectedRevision,
            InventoryReservation reservation)
        {
            Owner = owner;
            ExpectedRevision = expectedRevision;
            Reservation = reservation;
        }

        internal InventoryAuthority Owner { get; }

        internal InventoryReservation Reservation { get; }

        public long ExpectedRevision { get; }

        public StableId<ReservationIdScope> ReservationId => Reservation.Id;

        public StableId<InventoryClaimIdScope> ClaimId => Reservation.ClaimId;

        public StableId<ItemInstanceIdScope> ItemId => Reservation.ItemId;
    }

    /// <summary>
    /// Revision-bound permission to publish one complete set of exact serialized
    /// reservations. Retail is the only friend consumer; public callers use the atomic
    /// ReserveSerializedItems command.
    /// </summary>
    internal sealed class InventorySerializedReservationSetPlan
    {
        private readonly IReadOnlyList<InventorySerializedReservationRequest> _requests;
        private readonly IReadOnlyList<InventoryReservation> _reservations;

        internal InventorySerializedReservationSetPlan(
            InventoryAuthority owner,
            long expectedRevision,
            IReadOnlyList<InventorySerializedReservationRequest> requests,
            IReadOnlyList<InventoryReservation> reservations,
            bool isReplay)
        {
            Owner = owner;
            ExpectedRevision = expectedRevision;
            _requests = requests;
            _reservations = reservations;
            IsReplay = isReplay;
        }

        internal InventoryAuthority Owner { get; }

        internal IReadOnlyList<InventorySerializedReservationRequest> Requests => _requests;

        internal IReadOnlyList<InventoryReservation> Reservations => _reservations;

        internal bool IsReplay { get; }

        public long ExpectedRevision { get; }
    }

    /// <summary>
    /// Inventory-issued proof that one aggregate exclusively owns an exact serialized
    /// reservation set. Public reservation commands cannot extend, release or consume a
    /// managed claim; the owning aggregate must present this capability to validate it.
    /// </summary>
    internal sealed class InventorySerializedReservationSetAccess
    {
        private readonly IReadOnlyList<InventorySerializedReservationRequest> _requests;

        internal InventorySerializedReservationSetAccess(
            InventoryAuthority owner,
            StableId<InventorySerializedReservationSetOperationIdScope> operationId,
            StableId<InventoryClaimIdScope> managedClaimId,
            IReadOnlyList<InventorySerializedReservationRequest> requests,
            long appliedRevision)
        {
            Owner = owner;
            OperationId = operationId;
            ManagedClaimId = managedClaimId;
            _requests = requests;
            AppliedRevision = appliedRevision;
        }

        internal InventoryAuthority Owner { get; }

        internal StableId<InventorySerializedReservationSetOperationIdScope> OperationId { get; }

        internal StableId<InventoryClaimIdScope> ManagedClaimId { get; }

        internal IReadOnlyList<InventorySerializedReservationRequest> Requests => _requests;

        internal long AppliedRevision { get; }
    }

    /// <summary>
    /// Immutable, revision-bound permission to consume one exact reservation set for checkout.
    /// Only the authority that prepared the plan may commit it.
    /// </summary>
    internal sealed class InventoryCheckoutConsumptionPlan
    {
        private readonly IReadOnlyList<StableId<ReservationIdScope>> _reservationIds;

        internal InventoryCheckoutConsumptionPlan(
            InventoryAuthority owner,
            long expectedRevision,
            IReadOnlyList<StableId<ReservationIdScope>> reservationIds)
        {
            Owner = owner;
            ExpectedRevision = expectedRevision;

            if (reservationIds == null)
            {
                _reservationIds = null;
                return;
            }

            var exactIds = new StableId<ReservationIdScope>[reservationIds.Count];
            for (int index = 0; index < reservationIds.Count; index++)
            {
                exactIds[index] = reservationIds[index];
            }

            _reservationIds = Array.AsReadOnly(exactIds);
        }

        internal InventoryAuthority Owner { get; }

        internal IReadOnlyList<StableId<ReservationIdScope>> ReservationIds => _reservationIds;

        public long ExpectedRevision { get; }
    }

    /// <summary>
    /// The single authoritative owner of logical stock. Unity world objects are projections and never mutate
    /// quantities directly. Every failed command leaves state and Revision unchanged.
    /// </summary>
    public sealed partial class InventoryAuthority
    {
        private sealed class ReservationConsumptionSelection
        {
            public ReservationConsumptionSelection(
                IReadOnlyList<InventoryReservation> reservations,
                IReadOnlyCollection<StableId<ItemInstanceIdScope>> serializedItems,
                IReadOnlyDictionary<BatchPositionKey, long> batchConsumption)
            {
                Reservations = reservations;
                SerializedItems = serializedItems;
                BatchConsumption = batchConsumption;
            }

            public IReadOnlyList<InventoryReservation> Reservations { get; }

            public IReadOnlyCollection<StableId<ItemInstanceIdScope>> SerializedItems { get; }

            public IReadOnlyDictionary<BatchPositionKey, long> BatchConsumption { get; }
        }

        private sealed class ManagedSerializedReservationSetRegistration
        {
            private readonly IReadOnlyList<InventorySerializedReservationRequest> _requests;

            public ManagedSerializedReservationSetRegistration(
                StableId<InventorySerializedReservationSetOperationIdScope> operationId,
                StableId<InventoryClaimIdScope> claimId,
                long appliedRevision,
                IReadOnlyList<InventorySerializedReservationRequest> requests,
                InventorySerializedReservationSetAccess access)
            {
                OperationId = operationId;
                ClaimId = claimId;
                AppliedRevision = appliedRevision;
                Access = access;

                var exactRequests = new InventorySerializedReservationRequest[
                    requests.Count];
                for (int index = 0; index < requests.Count; index++)
                {
                    exactRequests[index] = requests[index];
                }

                _requests = Array.AsReadOnly(exactRequests);
            }

            public StableId<InventorySerializedReservationSetOperationIdScope>
                OperationId { get; }

            public StableId<InventoryClaimIdScope> ClaimId { get; }

            public long AppliedRevision { get; }

            public IReadOnlyList<InventorySerializedReservationRequest> Requests =>
                _requests;

            public InventorySerializedReservationSetAccess Access { get; }
        }

        private readonly ProductCatalog _catalog;
        private readonly Dictionary<StableId<ContainerIdScope>, InventoryContainerDefinition> _containers =
            new Dictionary<StableId<ContainerIdScope>, InventoryContainerDefinition>();
        private readonly Dictionary<StableId<ItemInstanceIdScope>, InventoryItemRecord> _items =
            new Dictionary<StableId<ItemInstanceIdScope>, InventoryItemRecord>();
        private readonly Dictionary<StableId<BatchIdScope>, InventoryBatchRecord> _batches =
            new Dictionary<StableId<BatchIdScope>, InventoryBatchRecord>();
        private readonly Dictionary<BatchPositionKey, int> _batchQuantities =
            new Dictionary<BatchPositionKey, int>();
        private readonly Dictionary<StableId<ReservationIdScope>, InventoryReservation> _reservations =
            new Dictionary<StableId<ReservationIdScope>, InventoryReservation>();
        private readonly Dictionary<StableId<ContainerIdScope>, InventorySerializedTransferAccess>
            _managedSerializedTransferContainers =
                new Dictionary<StableId<ContainerIdScope>, InventorySerializedTransferAccess>();
        private readonly Dictionary<StableId<InventoryClaimIdScope>,
            ManagedSerializedReservationSetRegistration> _managedSerializedReservationSets =
                new Dictionary<StableId<InventoryClaimIdScope>,
                    ManagedSerializedReservationSetRegistration>();
        private readonly Dictionary<
            StableId<InventorySerializedReservationSetOperationIdScope>,
            ManagedSerializedReservationSetRegistration>
            _managedSerializedReservationSetOperations =
                new Dictionary<
                    StableId<InventorySerializedReservationSetOperationIdScope>,
                    ManagedSerializedReservationSetRegistration>();

        private InventoryAuthority(ProductCatalog catalog)
        {
            _catalog = catalog;
        }

        public long Revision { get; private set; }

        public int ContainerCount => _containers.Count;

        public int SerializedItemCount => _items.Count;

        public int BatchCount => _batches.Count;

        public int ReservationCount => _reservations.Count;

        public static OperationResult<InventoryAuthority> Create(ProductCatalog catalog)
        {
            return catalog == null
                ? OperationResult<InventoryAuthority>.Fail(InventoryFailures.MissingCatalog)
                : OperationResult<InventoryAuthority>.Success(new InventoryAuthority(catalog));
        }

        internal bool UsesCatalog(ProductCatalog catalog)
        {
            return ReferenceEquals(_catalog, catalog);
        }

        public OperationResult RegisterContainer(InventoryContainerDefinition definition)
        {
            if (definition == null || definition.Id.IsEmpty)
            {
                return OperationResult.Fail(InventoryFailures.InvalidContainerId);
            }

            if (!InventoryValidation.IsValidContainerKind(definition.Kind))
            {
                return OperationResult.Fail(InventoryFailures.InvalidContainerKind);
            }

            if (definition.UnitCapacity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidContainerCapacity);
            }

            if (_containers.ContainsKey(definition.Id))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateContainer);
            }

            _containers.Add(definition.Id, definition);
            AdvanceRevision();
            return OperationResult.Success();
        }

        internal OperationResult<InventorySerializedTransferAccess>
            ClaimManagedSerializedTransferContainer(StableId<ContainerIdScope> containerId)
        {
            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult<InventorySerializedTransferAccess>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(containerId))
            {
                return OperationResult<InventorySerializedTransferAccess>.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (GetContainerLoadUnsafe(containerId) != 0 ||
                HasReservationTargetingContainerUnsafe(containerId))
            {
                return OperationResult<InventorySerializedTransferAccess>.Fail(
                    InventoryFailures.SerializedTransferContainerOccupied);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccess>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var access = new InventorySerializedTransferAccess(this, containerId);
            _managedSerializedTransferContainers.Add(containerId, access);
            Revision++;
            return OperationResult<InventorySerializedTransferAccess>.Success(access);
        }

        internal OperationResult<InventorySerializedTransferAccessPair>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId)
        {
            if (!_containers.ContainsKey(firstContainerId) ||
                !_containers.ContainsKey(secondContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessPair>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            if (firstContainerId == secondContainerId)
            {
                return OperationResult<InventorySerializedTransferAccessPair>.Fail(
                    InventoryFailures.SameContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(firstContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(secondContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessPair>.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (GetContainerLoadUnsafe(firstContainerId) != 0 ||
                GetContainerLoadUnsafe(secondContainerId) != 0 ||
                HasReservationTargetingContainerUnsafe(firstContainerId) ||
                HasReservationTargetingContainerUnsafe(secondContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessPair>.Fail(
                    InventoryFailures.SerializedTransferContainerOccupied);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessPair>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var first = new InventorySerializedTransferAccess(this, firstContainerId);
            var second = new InventorySerializedTransferAccess(this, secondContainerId);
            _managedSerializedTransferContainers.Add(firstContainerId, first);
            _managedSerializedTransferContainers.Add(secondContainerId, second);
            Revision++;
            return OperationResult<InventorySerializedTransferAccessPair>.Success(
                new InventorySerializedTransferAccessPair(first, second));
        }

        internal OperationResult<InventorySerializedTransferAccessTriple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId)
        {
            if (!_containers.ContainsKey(firstContainerId) ||
                !_containers.ContainsKey(secondContainerId) ||
                !_containers.ContainsKey(thirdContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessTriple>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            if (firstContainerId == secondContainerId ||
                firstContainerId == thirdContainerId ||
                secondContainerId == thirdContainerId)
            {
                return OperationResult<InventorySerializedTransferAccessTriple>.Fail(
                    InventoryFailures.SameContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(firstContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(secondContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(thirdContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessTriple>.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (GetContainerLoadUnsafe(firstContainerId) != 0 ||
                GetContainerLoadUnsafe(secondContainerId) != 0 ||
                GetContainerLoadUnsafe(thirdContainerId) != 0 ||
                HasReservationTargetingContainerUnsafe(firstContainerId) ||
                HasReservationTargetingContainerUnsafe(secondContainerId) ||
                HasReservationTargetingContainerUnsafe(thirdContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessTriple>.Fail(
                    InventoryFailures.SerializedTransferContainerOccupied);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessTriple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var first = new InventorySerializedTransferAccess(this, firstContainerId);
            var second = new InventorySerializedTransferAccess(this, secondContainerId);
            var third = new InventorySerializedTransferAccess(this, thirdContainerId);
            _managedSerializedTransferContainers.Add(firstContainerId, first);
            _managedSerializedTransferContainers.Add(secondContainerId, second);
            _managedSerializedTransferContainers.Add(thirdContainerId, third);
            Revision++;
            return OperationResult<InventorySerializedTransferAccessTriple>.Success(
                new InventorySerializedTransferAccessTriple(first, second, third));
        }

        internal OperationResult<InventorySerializedTransferAccessQuadruple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId,
                StableId<ContainerIdScope> fourthContainerId)
        {
            if (!_containers.ContainsKey(firstContainerId) ||
                !_containers.ContainsKey(secondContainerId) ||
                !_containers.ContainsKey(thirdContainerId) ||
                !_containers.ContainsKey(fourthContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessQuadruple>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            if (firstContainerId == secondContainerId ||
                firstContainerId == thirdContainerId ||
                firstContainerId == fourthContainerId ||
                secondContainerId == thirdContainerId ||
                secondContainerId == fourthContainerId ||
                thirdContainerId == fourthContainerId)
            {
                return OperationResult<InventorySerializedTransferAccessQuadruple>.Fail(
                    InventoryFailures.SameContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(firstContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(secondContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(thirdContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(fourthContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessQuadruple>.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (GetContainerLoadUnsafe(firstContainerId) != 0 ||
                GetContainerLoadUnsafe(secondContainerId) != 0 ||
                GetContainerLoadUnsafe(thirdContainerId) != 0 ||
                GetContainerLoadUnsafe(fourthContainerId) != 0 ||
                HasReservationTargetingContainerUnsafe(firstContainerId) ||
                HasReservationTargetingContainerUnsafe(secondContainerId) ||
                HasReservationTargetingContainerUnsafe(thirdContainerId) ||
                HasReservationTargetingContainerUnsafe(fourthContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessQuadruple>.Fail(
                    InventoryFailures.SerializedTransferContainerOccupied);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessQuadruple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var first = new InventorySerializedTransferAccess(this, firstContainerId);
            var second = new InventorySerializedTransferAccess(this, secondContainerId);
            var third = new InventorySerializedTransferAccess(this, thirdContainerId);
            var fourth = new InventorySerializedTransferAccess(this, fourthContainerId);
            _managedSerializedTransferContainers.Add(firstContainerId, first);
            _managedSerializedTransferContainers.Add(secondContainerId, second);
            _managedSerializedTransferContainers.Add(thirdContainerId, third);
            _managedSerializedTransferContainers.Add(fourthContainerId, fourth);
            Revision++;
            return OperationResult<InventorySerializedTransferAccessQuadruple>.Success(
                new InventorySerializedTransferAccessQuadruple(
                    first,
                    second,
                    third,
                    fourth));
        }

        internal OperationResult<InventorySerializedTransferAccessQuintuple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId,
                StableId<ContainerIdScope> fourthContainerId,
                StableId<ContainerIdScope> fifthContainerId)
        {
            if (!_containers.ContainsKey(firstContainerId) ||
                !_containers.ContainsKey(secondContainerId) ||
                !_containers.ContainsKey(thirdContainerId) ||
                !_containers.ContainsKey(fourthContainerId) ||
                !_containers.ContainsKey(fifthContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessQuintuple>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            if (firstContainerId == secondContainerId ||
                firstContainerId == thirdContainerId ||
                firstContainerId == fourthContainerId ||
                firstContainerId == fifthContainerId ||
                secondContainerId == thirdContainerId ||
                secondContainerId == fourthContainerId ||
                secondContainerId == fifthContainerId ||
                thirdContainerId == fourthContainerId ||
                thirdContainerId == fifthContainerId ||
                fourthContainerId == fifthContainerId)
            {
                return OperationResult<InventorySerializedTransferAccessQuintuple>.Fail(
                    InventoryFailures.SameContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(firstContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(secondContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(thirdContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(fourthContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(fifthContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessQuintuple>.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (GetContainerLoadUnsafe(firstContainerId) != 0 ||
                GetContainerLoadUnsafe(secondContainerId) != 0 ||
                GetContainerLoadUnsafe(thirdContainerId) != 0 ||
                GetContainerLoadUnsafe(fourthContainerId) != 0 ||
                GetContainerLoadUnsafe(fifthContainerId) != 0 ||
                HasReservationTargetingContainerUnsafe(firstContainerId) ||
                HasReservationTargetingContainerUnsafe(secondContainerId) ||
                HasReservationTargetingContainerUnsafe(thirdContainerId) ||
                HasReservationTargetingContainerUnsafe(fourthContainerId) ||
                HasReservationTargetingContainerUnsafe(fifthContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessQuintuple>.Fail(
                    InventoryFailures.SerializedTransferContainerOccupied);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessQuintuple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var first = new InventorySerializedTransferAccess(this, firstContainerId);
            var second = new InventorySerializedTransferAccess(this, secondContainerId);
            var third = new InventorySerializedTransferAccess(this, thirdContainerId);
            var fourth = new InventorySerializedTransferAccess(this, fourthContainerId);
            var fifth = new InventorySerializedTransferAccess(this, fifthContainerId);
            _managedSerializedTransferContainers.Add(firstContainerId, first);
            _managedSerializedTransferContainers.Add(secondContainerId, second);
            _managedSerializedTransferContainers.Add(thirdContainerId, third);
            _managedSerializedTransferContainers.Add(fourthContainerId, fourth);
            _managedSerializedTransferContainers.Add(fifthContainerId, fifth);
            Revision++;
            return OperationResult<InventorySerializedTransferAccessQuintuple>.Success(
                new InventorySerializedTransferAccessQuintuple(
                    first,
                    second,
                    third,
                    fourth,
                    fifth));
        }

        internal OperationResult<InventorySerializedTransferAccessSextuple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId,
                StableId<ContainerIdScope> fourthContainerId,
                StableId<ContainerIdScope> fifthContainerId,
                StableId<ContainerIdScope> sixthContainerId)
        {
            if (!_containers.ContainsKey(firstContainerId) ||
                !_containers.ContainsKey(secondContainerId) ||
                !_containers.ContainsKey(thirdContainerId) ||
                !_containers.ContainsKey(fourthContainerId) ||
                !_containers.ContainsKey(fifthContainerId) ||
                !_containers.ContainsKey(sixthContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessSextuple>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            StableId<ContainerIdScope>[] containerIds =
            {
                firstContainerId,
                secondContainerId,
                thirdContainerId,
                fourthContainerId,
                fifthContainerId,
                sixthContainerId
            };
            for (int left = 0; left < containerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < containerIds.Length; right++)
                {
                    if (containerIds[left] == containerIds[right])
                    {
                        return OperationResult<InventorySerializedTransferAccessSextuple>.Fail(
                            InventoryFailures.SameContainer);
                    }
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                StableId<ContainerIdScope> containerId = containerIds[index];
                if (_managedSerializedTransferContainers.ContainsKey(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessSextuple>.Fail(
                        InventoryFailures.SerializedTransferContainerManaged);
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                StableId<ContainerIdScope> containerId = containerIds[index];
                if (GetContainerLoadUnsafe(containerId) != 0 ||
                    HasReservationTargetingContainerUnsafe(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessSextuple>.Fail(
                        InventoryFailures.SerializedTransferContainerOccupied);
                }
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessSextuple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var first = new InventorySerializedTransferAccess(this, firstContainerId);
            var second = new InventorySerializedTransferAccess(this, secondContainerId);
            var third = new InventorySerializedTransferAccess(this, thirdContainerId);
            var fourth = new InventorySerializedTransferAccess(this, fourthContainerId);
            var fifth = new InventorySerializedTransferAccess(this, fifthContainerId);
            var sixth = new InventorySerializedTransferAccess(this, sixthContainerId);
            _managedSerializedTransferContainers.Add(firstContainerId, first);
            _managedSerializedTransferContainers.Add(secondContainerId, second);
            _managedSerializedTransferContainers.Add(thirdContainerId, third);
            _managedSerializedTransferContainers.Add(fourthContainerId, fourth);
            _managedSerializedTransferContainers.Add(fifthContainerId, fifth);
            _managedSerializedTransferContainers.Add(sixthContainerId, sixth);
            Revision++;
            return OperationResult<InventorySerializedTransferAccessSextuple>.Success(
                new InventorySerializedTransferAccessSextuple(
                    first,
                    second,
                    third,
                    fourth,
                    fifth,
                    sixth));
        }

        internal OperationResult<InventorySerializedTransferAccessSeptuple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId,
                StableId<ContainerIdScope> fourthContainerId,
                StableId<ContainerIdScope> fifthContainerId,
                StableId<ContainerIdScope> sixthContainerId,
                StableId<ContainerIdScope> seventhContainerId)
        {
            if (!_containers.ContainsKey(firstContainerId) ||
                !_containers.ContainsKey(secondContainerId) ||
                !_containers.ContainsKey(thirdContainerId) ||
                !_containers.ContainsKey(fourthContainerId) ||
                !_containers.ContainsKey(fifthContainerId) ||
                !_containers.ContainsKey(sixthContainerId) ||
                !_containers.ContainsKey(seventhContainerId))
            {
                return OperationResult<InventorySerializedTransferAccessSeptuple>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            StableId<ContainerIdScope>[] containerIds =
            {
                firstContainerId,
                secondContainerId,
                thirdContainerId,
                fourthContainerId,
                fifthContainerId,
                sixthContainerId,
                seventhContainerId
            };
            for (int left = 0; left < containerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < containerIds.Length; right++)
                {
                    if (containerIds[left] == containerIds[right])
                    {
                        return OperationResult<InventorySerializedTransferAccessSeptuple>.Fail(
                            InventoryFailures.SameContainer);
                    }
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                StableId<ContainerIdScope> containerId = containerIds[index];
                if (_managedSerializedTransferContainers.ContainsKey(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessSeptuple>.Fail(
                        InventoryFailures.SerializedTransferContainerManaged);
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                StableId<ContainerIdScope> containerId = containerIds[index];
                if (GetContainerLoadUnsafe(containerId) != 0 ||
                    HasReservationTargetingContainerUnsafe(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessSeptuple>.Fail(
                        InventoryFailures.SerializedTransferContainerOccupied);
                }
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessSeptuple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var first = new InventorySerializedTransferAccess(this, firstContainerId);
            var second = new InventorySerializedTransferAccess(this, secondContainerId);
            var third = new InventorySerializedTransferAccess(this, thirdContainerId);
            var fourth = new InventorySerializedTransferAccess(this, fourthContainerId);
            var fifth = new InventorySerializedTransferAccess(this, fifthContainerId);
            var sixth = new InventorySerializedTransferAccess(this, sixthContainerId);
            var seventh = new InventorySerializedTransferAccess(this, seventhContainerId);
            _managedSerializedTransferContainers.Add(firstContainerId, first);
            _managedSerializedTransferContainers.Add(secondContainerId, second);
            _managedSerializedTransferContainers.Add(thirdContainerId, third);
            _managedSerializedTransferContainers.Add(fourthContainerId, fourth);
            _managedSerializedTransferContainers.Add(fifthContainerId, fifth);
            _managedSerializedTransferContainers.Add(sixthContainerId, sixth);
            _managedSerializedTransferContainers.Add(seventhContainerId, seventh);
            Revision++;
            return OperationResult<InventorySerializedTransferAccessSeptuple>.Success(
                new InventorySerializedTransferAccessSeptuple(
                    first,
                    second,
                    third,
                    fourth,
                    fifth,
                    sixth,
                    seventh));
        }

        internal OperationResult<InventorySerializedTransferAccessOctuple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId,
                StableId<ContainerIdScope> fourthContainerId,
                StableId<ContainerIdScope> fifthContainerId,
                StableId<ContainerIdScope> sixthContainerId,
                StableId<ContainerIdScope> seventhContainerId,
                StableId<ContainerIdScope> eighthContainerId)
        {
            StableId<ContainerIdScope>[] containerIds =
            {
                firstContainerId,
                secondContainerId,
                thirdContainerId,
                fourthContainerId,
                fifthContainerId,
                sixthContainerId,
                seventhContainerId,
                eighthContainerId
            };

            for (int index = 0; index < containerIds.Length; index++)
            {
                if (!_containers.ContainsKey(containerIds[index]))
                {
                    return OperationResult<InventorySerializedTransferAccessOctuple>.Fail(
                        InventoryFailures.UnknownContainer);
                }
            }

            for (int left = 0; left < containerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < containerIds.Length; right++)
                {
                    if (containerIds[left] == containerIds[right])
                    {
                        return OperationResult<InventorySerializedTransferAccessOctuple>.Fail(
                            InventoryFailures.SameContainer);
                    }
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                StableId<ContainerIdScope> containerId = containerIds[index];
                if (_managedSerializedTransferContainers.ContainsKey(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessOctuple>.Fail(
                        InventoryFailures.SerializedTransferContainerManaged);
                }

                if (GetContainerLoadUnsafe(containerId) != 0 ||
                    HasReservationTargetingContainerUnsafe(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessOctuple>.Fail(
                        InventoryFailures.SerializedTransferContainerOccupied);
                }
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessOctuple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var accesses = new InventorySerializedTransferAccess[containerIds.Length];
            for (int index = 0; index < containerIds.Length; index++)
            {
                accesses[index] = new InventorySerializedTransferAccess(
                    this,
                    containerIds[index]);
                _managedSerializedTransferContainers.Add(
                    containerIds[index],
                    accesses[index]);
            }

            Revision++;
            return OperationResult<InventorySerializedTransferAccessOctuple>.Success(
                new InventorySerializedTransferAccessOctuple(
                    accesses[0],
                    accesses[1],
                    accesses[2],
                    accesses[3],
                    accesses[4],
                    accesses[5],
                    accesses[6],
                    accesses[7]));
        }

        internal OperationResult<InventorySerializedTransferAccessNonuple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId,
                StableId<ContainerIdScope> fourthContainerId,
                StableId<ContainerIdScope> fifthContainerId,
                StableId<ContainerIdScope> sixthContainerId,
                StableId<ContainerIdScope> seventhContainerId,
                StableId<ContainerIdScope> eighthContainerId,
                StableId<ContainerIdScope> ninthContainerId)
        {
            StableId<ContainerIdScope>[] containerIds =
            {
                firstContainerId,
                secondContainerId,
                thirdContainerId,
                fourthContainerId,
                fifthContainerId,
                sixthContainerId,
                seventhContainerId,
                eighthContainerId,
                ninthContainerId
            };

            for (int index = 0; index < containerIds.Length; index++)
            {
                if (!_containers.ContainsKey(containerIds[index]))
                {
                    return OperationResult<InventorySerializedTransferAccessNonuple>.Fail(
                        InventoryFailures.UnknownContainer);
                }
            }

            for (int left = 0; left < containerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < containerIds.Length; right++)
                {
                    if (containerIds[left] == containerIds[right])
                    {
                        return OperationResult<InventorySerializedTransferAccessNonuple>.Fail(
                            InventoryFailures.SameContainer);
                    }
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                if (_managedSerializedTransferContainers.ContainsKey(containerIds[index]))
                {
                    return OperationResult<InventorySerializedTransferAccessNonuple>.Fail(
                        InventoryFailures.SerializedTransferContainerManaged);
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                StableId<ContainerIdScope> containerId = containerIds[index];
                if (GetContainerLoadUnsafe(containerId) != 0 ||
                    HasReservationTargetingContainerUnsafe(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessNonuple>.Fail(
                        InventoryFailures.SerializedTransferContainerOccupied);
                }
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessNonuple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var accesses = new InventorySerializedTransferAccess[containerIds.Length];
            for (int index = 0; index < containerIds.Length; index++)
            {
                accesses[index] = new InventorySerializedTransferAccess(
                    this,
                    containerIds[index]);
                _managedSerializedTransferContainers.Add(
                    containerIds[index],
                    accesses[index]);
            }

            Revision++;
            return OperationResult<InventorySerializedTransferAccessNonuple>.Success(
                new InventorySerializedTransferAccessNonuple(
                    accesses[0],
                    accesses[1],
                    accesses[2],
                    accesses[3],
                    accesses[4],
                    accesses[5],
                    accesses[6],
                    accesses[7],
                    accesses[8]));
        }

        internal OperationResult<InventorySerializedTransferAccessDecuple>
            ClaimManagedSerializedTransferContainers(
                StableId<ContainerIdScope> firstContainerId,
                StableId<ContainerIdScope> secondContainerId,
                StableId<ContainerIdScope> thirdContainerId,
                StableId<ContainerIdScope> fourthContainerId,
                StableId<ContainerIdScope> fifthContainerId,
                StableId<ContainerIdScope> sixthContainerId,
                StableId<ContainerIdScope> seventhContainerId,
                StableId<ContainerIdScope> eighthContainerId,
                StableId<ContainerIdScope> ninthContainerId,
                StableId<ContainerIdScope> tenthContainerId)
        {
            StableId<ContainerIdScope>[] containerIds =
            {
                firstContainerId,
                secondContainerId,
                thirdContainerId,
                fourthContainerId,
                fifthContainerId,
                sixthContainerId,
                seventhContainerId,
                eighthContainerId,
                ninthContainerId,
                tenthContainerId
            };

            for (int index = 0; index < containerIds.Length; index++)
            {
                if (!_containers.ContainsKey(containerIds[index]))
                {
                    return OperationResult<InventorySerializedTransferAccessDecuple>.Fail(
                        InventoryFailures.UnknownContainer);
                }
            }

            for (int left = 0; left < containerIds.Length - 1; left++)
            {
                for (int right = left + 1; right < containerIds.Length; right++)
                {
                    if (containerIds[left] == containerIds[right])
                    {
                        return OperationResult<InventorySerializedTransferAccessDecuple>.Fail(
                            InventoryFailures.SameContainer);
                    }
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                if (_managedSerializedTransferContainers.ContainsKey(containerIds[index]))
                {
                    return OperationResult<InventorySerializedTransferAccessDecuple>.Fail(
                        InventoryFailures.SerializedTransferContainerManaged);
                }
            }

            for (int index = 0; index < containerIds.Length; index++)
            {
                StableId<ContainerIdScope> containerId = containerIds[index];
                if (GetContainerLoadUnsafe(containerId) != 0 ||
                    HasReservationTargetingContainerUnsafe(containerId))
                {
                    return OperationResult<InventorySerializedTransferAccessDecuple>.Fail(
                        InventoryFailures.SerializedTransferContainerOccupied);
                }
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferAccessDecuple>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var accesses = new InventorySerializedTransferAccess[containerIds.Length];
            for (int index = 0; index < containerIds.Length; index++)
            {
                accesses[index] = new InventorySerializedTransferAccess(
                    this,
                    containerIds[index]);
                _managedSerializedTransferContainers.Add(
                    containerIds[index],
                    accesses[index]);
            }

            Revision++;
            return OperationResult<InventorySerializedTransferAccessDecuple>.Success(
                new InventorySerializedTransferAccessDecuple(
                    accesses[0],
                    accesses[1],
                    accesses[2],
                    accesses[3],
                    accesses[4],
                    accesses[5],
                    accesses[6],
                    accesses[7],
                    accesses[8],
                    accesses[9]));
        }

        public OperationResult ReceiveSerializedItem(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> containerId,
            InventoryCondition condition,
            InventoryUnitCost unitCost)
        {
            if (itemId.IsEmpty)
            {
                return OperationResult.Fail(InventoryFailures.InvalidItemId);
            }

            Failure productFailure = ValidateProduct(productId, ProductTrackingPolicy.SerializedInstance);
            if (!productFailure.IsNone)
            {
                return OperationResult.Fail(productFailure);
            }

            if (!InventoryValidation.IsValidCondition(condition))
            {
                return OperationResult.Fail(InventoryFailures.InvalidCondition);
            }

            if (!unitCost.IsValid)
            {
                return OperationResult.Fail(InventoryFailures.InvalidUnitCost);
            }

            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(containerId))
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (_items.ContainsKey(itemId))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateItem);
            }

            Failure capacityFailure = ValidateCapacity(containerId, 1);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            _items.Add(
                itemId,
                new InventoryItemRecord(itemId, productId, containerId, condition, unitCost));
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReceiveBatch(
            StableId<BatchIdScope> batchId,
            StableId<ProductDefinitionIdScope> productId,
            StableId<ContainerIdScope> containerId,
            InventoryCondition condition,
            int quantity,
            InventoryUnitCost unitCost)
        {
            if (batchId.IsEmpty)
            {
                return OperationResult.Fail(InventoryFailures.InvalidBatchId);
            }

            if (quantity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidQuantity);
            }

            Failure productFailure = ValidateProduct(productId, ProductTrackingPolicy.BatchQuantity);
            if (!productFailure.IsNone)
            {
                return OperationResult.Fail(productFailure);
            }

            if (!InventoryValidation.IsValidCondition(condition))
            {
                return OperationResult.Fail(InventoryFailures.InvalidCondition);
            }

            if (!unitCost.IsValid)
            {
                return OperationResult.Fail(InventoryFailures.InvalidUnitCost);
            }

            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(containerId))
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (_batches.ContainsKey(batchId))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateBatch);
            }

            Failure capacityFailure = ValidateCapacity(containerId, quantity);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            var position = new BatchPositionKey(batchId, containerId);
            _batches.Add(
                batchId,
                new InventoryBatchRecord(batchId, productId, condition, unitCost));
            _batchQuantities.Add(position, quantity);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReceiveIntake(
            StableId<ContainerIdScope> containerId,
            InventoryIntake intake)
        {
            if (intake == null)
            {
                return OperationResult.Fail(InventoryFailures.MissingIntake);
            }

            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(containerId))
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            long addedQuantity = 0;
            var pendingItemIds = new HashSet<StableId<ItemInstanceIdScope>>();
            var pendingBatchIds = new HashSet<StableId<BatchIdScope>>();

            for (int index = 0; index < intake.SerializedItems.Count; index++)
            {
                InventorySerializedIntake item = intake.SerializedItems[index];
                if (item == null || item.ItemId.IsEmpty || !pendingItemIds.Add(item.ItemId))
                {
                    return OperationResult.Fail(InventoryFailures.DuplicateIntakeItem);
                }

                if (_items.ContainsKey(item.ItemId))
                {
                    return OperationResult.Fail(InventoryFailures.DuplicateItem);
                }

                Failure productFailure = ValidateProduct(
                    item.ProductId,
                    ProductTrackingPolicy.SerializedInstance);
                if (!productFailure.IsNone)
                {
                    return OperationResult.Fail(productFailure);
                }

                if (!InventoryValidation.IsValidCondition(item.Condition))
                {
                    return OperationResult.Fail(InventoryFailures.InvalidCondition);
                }

                if (!item.UnitCost.IsValid)
                {
                    return OperationResult.Fail(InventoryFailures.InvalidUnitCost);
                }

                addedQuantity++;
            }

            for (int index = 0; index < intake.Batches.Count; index++)
            {
                InventoryBatchIntake batch = intake.Batches[index];
                if (batch == null || batch.BatchId.IsEmpty || !pendingBatchIds.Add(batch.BatchId))
                {
                    return OperationResult.Fail(InventoryFailures.DuplicateIntakeBatch);
                }

                if (_batches.ContainsKey(batch.BatchId))
                {
                    return OperationResult.Fail(InventoryFailures.DuplicateBatch);
                }

                if (batch.Quantity <= 0)
                {
                    return OperationResult.Fail(InventoryFailures.InvalidQuantity);
                }

                Failure productFailure = ValidateProduct(
                    batch.ProductId,
                    ProductTrackingPolicy.BatchQuantity);
                if (!productFailure.IsNone)
                {
                    return OperationResult.Fail(productFailure);
                }

                if (!InventoryValidation.IsValidCondition(batch.Condition))
                {
                    return OperationResult.Fail(InventoryFailures.InvalidCondition);
                }

                if (!batch.UnitCost.IsValid)
                {
                    return OperationResult.Fail(InventoryFailures.InvalidUnitCost);
                }

                if (long.MaxValue - addedQuantity < batch.Quantity)
                {
                    return OperationResult.Fail(InventoryFailures.QuantityOverflow);
                }

                addedQuantity += batch.Quantity;
            }

            if (addedQuantity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.EmptyIntake);
            }

            Failure capacityFailure = ValidateCapacity(containerId, addedQuantity);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            for (int index = 0; index < intake.SerializedItems.Count; index++)
            {
                InventorySerializedIntake item = intake.SerializedItems[index];
                _items.Add(
                    item.ItemId,
                    new InventoryItemRecord(
                        item.ItemId,
                        item.ProductId,
                        containerId,
                        item.Condition,
                        item.UnitCost));
            }

            for (int index = 0; index < intake.Batches.Count; index++)
            {
                InventoryBatchIntake batch = intake.Batches[index];
                _batches.Add(
                    batch.BatchId,
                    new InventoryBatchRecord(
                        batch.BatchId,
                        batch.ProductId,
                        batch.Condition,
                        batch.UnitCost));
                _batchQuantities.Add(
                    new BatchPositionKey(batch.BatchId, containerId),
                    batch.Quantity);
            }

            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult TransferSerializedItem(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> targetContainerId)
        {
            OperationResult<InventorySerializedTransferPlan> prepared =
                PrepareSerializedItemTransfer(itemId, targetContainerId);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedSerializedItemTransfer(prepared.Value);
        }

        internal OperationResult<InventorySerializedTransferPlan> PrepareSerializedItemTransfer(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> targetContainerId)
        {
            return PrepareSerializedItemTransfer(itemId, targetContainerId, null);
        }

        internal OperationResult<InventorySerializedTransferPlan> PrepareSerializedItemTransfer(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> targetContainerId,
            InventorySerializedTransferAccess access)
        {
            return PrepareSerializedItemTransfer(
                itemId,
                targetContainerId,
                access,
                InventorySerializedItemStateFlags.None,
                InventorySerializedItemStateFlags.None);
        }

        internal OperationResult<InventorySerializedTransferPlan>
            PrepareSerializedItemTransferAndConsumePreAppliedState(
                StableId<ItemInstanceIdScope> itemId,
                StableId<ContainerIdScope> targetContainerId,
                InventorySerializedTransferAccess access)
        {
            const InventorySerializedItemStateFlags consumed =
                InventorySerializedItemStateFlags.PreAppliedConsumableConsumed;
            return PrepareSerializedItemTransfer(
                itemId,
                targetContainerId,
                access,
                consumed,
                consumed);
        }

        private OperationResult<InventorySerializedTransferPlan> PrepareSerializedItemTransfer(
            StableId<ItemInstanceIdScope> itemId,
            StableId<ContainerIdScope> targetContainerId,
            InventorySerializedTransferAccess access,
            InventorySerializedItemStateFlags requiredAbsentStateFlags,
            InventorySerializedItemStateFlags stateFlagsToAdd)
        {
            if (!_items.TryGetValue(itemId, out InventoryItemRecord item))
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.UnknownItem);
            }

            if (!_containers.ContainsKey(targetContainerId))
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.UnknownContainer);
            }

            if (item.ContainerId == targetContainerId)
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.SameContainer);
            }

            if (!AreValidSerializedItemStateFlags(requiredAbsentStateFlags) ||
                !AreValidSerializedItemStateFlags(stateFlagsToAdd) ||
                requiredAbsentStateFlags != stateFlagsToAdd)
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.SerializedItemStateInvalid);
            }

            if ((item.StateFlags & requiredAbsentStateFlags) != 0)
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.SerializedItemStateConflict);
            }

            if (_serializedReservationWorkOrderBuildKitsByItem.ContainsKey(itemId))
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderBuildKitConflict);
            }

            Failure accessFailure = ValidateSerializedTransferAccess(
                item.ContainerId,
                targetContainerId,
                access);
            if (!accessFailure.IsNone)
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(accessFailure);
            }

            if (access != null && IsSerializedItemReserved(itemId))
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.ReservedQuantity);
            }

            Failure capacityFailure = ValidateCapacity(targetContainerId, 1);
            if (!capacityFailure.IsNone)
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(capacityFailure);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedTransferPlan>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            return OperationResult<InventorySerializedTransferPlan>.Success(
                new InventorySerializedTransferPlan(
                    this,
                    Revision,
                    itemId,
                    item.ContainerId,
                    targetContainerId,
                    access,
                    requiredAbsentStateFlags,
                    stateFlagsToAdd));
        }

        internal OperationResult CommitPreparedSerializedItemTransfer(
            InventorySerializedTransferPlan plan)
        {
            if (plan == null ||
                !ReferenceEquals(plan.Owner, this) ||
                plan.ItemId.IsEmpty ||
                plan.SourceContainerId.IsEmpty ||
                plan.TargetContainerId.IsEmpty ||
                plan.SourceContainerId == plan.TargetContainerId ||
                !AreValidSerializedItemStateFlags(plan.RequiredAbsentStateFlags) ||
                !AreValidSerializedItemStateFlags(plan.StateFlagsToAdd) ||
                plan.RequiredAbsentStateFlags != plan.StateFlagsToAdd)
            {
                return OperationResult.Fail(InventoryFailures.SerializedTransferPlanInvalid);
            }

            if (Revision != plan.ExpectedRevision)
            {
                return OperationResult.Fail(InventoryFailures.SerializedTransferPlanStale);
            }

            if (!ValidateSerializedTransferAccess(
                    plan.SourceContainerId,
                    plan.TargetContainerId,
                    plan.Access).IsNone)
            {
                return OperationResult.Fail(InventoryFailures.SerializedTransferPlanInvalid);
            }

            OperationResult<InventorySerializedTransferPlan> current =
                PrepareSerializedItemTransfer(
                    plan.ItemId,
                    plan.TargetContainerId,
                    plan.Access,
                    plan.RequiredAbsentStateFlags,
                    plan.StateFlagsToAdd);
            if (current.IsFailure)
            {
                return current.Error == InventoryFailures.RevisionOverflow
                    ? OperationResult.Fail(current.Error)
                    : OperationResult.Fail(InventoryFailures.SerializedTransferPlanStale);
            }

            if (current.Value.SourceContainerId != plan.SourceContainerId)
            {
                return OperationResult.Fail(InventoryFailures.SerializedTransferPlanStale);
            }

            InventoryItemRecord item = _items[plan.ItemId];
            _items[plan.ItemId] = new InventoryItemRecord(
                item.Id,
                item.ProductId,
                plan.TargetContainerId,
                item.Condition,
                item.UnitCost,
                item.StateFlags | plan.StateFlagsToAdd);
            Revision++;
            return OperationResult.Success();
        }

        public OperationResult TransferBatch(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            int quantity)
        {
            if (quantity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidQuantity);
            }

            if (!_batches.ContainsKey(batchId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatch);
            }

            if (!_containers.ContainsKey(sourceContainerId) || !_containers.ContainsKey(targetContainerId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownContainer);
            }

            if (_managedSerializedTransferContainers.ContainsKey(sourceContainerId) ||
                _managedSerializedTransferContainers.ContainsKey(targetContainerId))
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            if (sourceContainerId == targetContainerId)
            {
                return OperationResult.Fail(InventoryFailures.SameContainer);
            }

            var source = new BatchPositionKey(batchId, sourceContainerId);
            if (!_batchQuantities.TryGetValue(source, out int sourceQuantity))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatchPosition);
            }

            if (sourceQuantity < quantity)
            {
                return OperationResult.Fail(InventoryFailures.InsufficientAvailable);
            }

            int reservedAtSource = GetReservedBatchQuantityUnsafe(batchId, sourceContainerId);
            if ((long)sourceQuantity - reservedAtSource < quantity)
            {
                return OperationResult.Fail(InventoryFailures.ReservedQuantity);
            }

            Failure capacityFailure = ValidateCapacity(targetContainerId, quantity);
            if (!capacityFailure.IsNone)
            {
                return OperationResult.Fail(capacityFailure);
            }

            var target = new BatchPositionKey(batchId, targetContainerId);
            _batchQuantities.TryGetValue(target, out int targetQuantity);
            if ((long)targetQuantity + quantity > int.MaxValue)
            {
                return OperationResult.Fail(InventoryFailures.QuantityOverflow);
            }

            int remaining = sourceQuantity - quantity;
            if (remaining == 0)
            {
                _batchQuantities.Remove(source);
            }
            else
            {
                _batchQuantities[source] = remaining;
            }

            _batchQuantities[target] = targetQuantity + quantity;
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReserveSerializedItem(
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId,
            StableId<ItemInstanceIdScope> itemId)
        {
            OperationResult<InventorySerializedReservationPlan> prepared =
                PrepareSerializedItemReservation(reservationId, claimId, itemId);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedSerializedItemReservation(prepared.Value);
        }

        /// <summary>
        /// Atomically reserves a complete exact serialized-item set. All identities,
        /// ownership conflicts and managed-container restrictions are preflighted before
        /// one successful set advances Inventory Revision exactly once.
        /// </summary>
        public OperationResult ReserveSerializedItems(
            IReadOnlyList<InventorySerializedReservationRequest> requests)
        {
            OperationResult<InventorySerializedReservationSetPlan> prepared =
                PrepareSerializedItemReservationSet(requests);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedSerializedItemReservationSet(prepared.Value);
        }

        /// <summary>
        /// Atomically reserves and exclusively manages one exact serialized-item set for a
        /// friend aggregate. The claim must be unused, every member must share that claim,
        /// and Inventory publishes both reservations and their ownership capability in one
        /// revision.
        /// </summary>
        internal OperationResult<InventorySerializedReservationSetAccess>
            ReserveManagedSerializedItems(
                StableId<InventorySerializedReservationSetOperationIdScope> operationId,
                IReadOnlyList<InventorySerializedReservationRequest> requests)
        {
            if (operationId.IsEmpty)
            {
                return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                    InventoryFailures.InvalidSerializedReservationSetOperationId);
            }

            if (requests == null)
            {
                return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                    InventoryFailures.MissingSerializedReservationSet);
            }

            if (requests.Count == 0)
            {
                return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                    InventoryFailures.EmptySerializedReservationSet);
            }

            StableId<InventoryClaimIdScope> claimId = default;
            for (int index = 0; index < requests.Count; index++)
            {
                InventorySerializedReservationRequest request = requests[index];
                if (request == null)
                {
                    return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                        InventoryFailures.NullSerializedReservationRequest);
                }

                if (index == 0)
                {
                    claimId = request.ClaimId;
                }
                else if (request.ClaimId != claimId)
                {
                    return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                        InventoryFailures.SerializedReservationSetClaimMismatch);
                }
            }

            if (claimId.IsEmpty)
            {
                return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                    InventoryFailures.InvalidClaimId);
            }

            if (_managedSerializedReservationSetOperations.TryGetValue(
                    operationId,
                    out ManagedSerializedReservationSetRegistration
                        existingForOperation))
            {
                if (existingForOperation.ClaimId != claimId ||
                    !MatchesManagedSerializedReservationRequests(
                        existingForOperation.Requests,
                        requests))
                {
                    return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                        InventoryFailures.SerializedReservationSetOperationConflict);
                }

                return OwnsManagedSerializedReservationSet(
                        existingForOperation.Access)
                    ? OperationResult<InventorySerializedReservationSetAccess>.Success(
                        existingForOperation.Access)
                    : OperationResult<InventorySerializedReservationSetAccess>.Fail(
                        InventoryFailures.SerializedReservationSetAccessInvalid);
            }

            if (_managedSerializedReservationSets.ContainsKey(claimId))
            {
                return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                    InventoryFailures.SerializedReservationSetOperationConflict);
            }

            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.ClaimId == claimId)
                {
                    return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                        InventoryFailures.SerializedReservationClaimOccupied);
                }
            }

            OperationResult<InventorySerializedReservationSetPlan> prepared =
                PrepareSerializedItemReservationSet(requests);
            if (prepared.IsFailure)
            {
                return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                    prepared.Error);
            }

            InventorySerializedReservationSetPlan plan = prepared.Value;
            if (plan.IsReplay || plan.ExpectedRevision != Revision)
            {
                return OperationResult<InventorySerializedReservationSetAccess>.Fail(
                    InventoryFailures.SerializedReservationClaimOccupied);
            }

            var exactRequests = new InventorySerializedReservationRequest[
                plan.Requests.Count];
            for (int index = 0; index < plan.Requests.Count; index++)
            {
                exactRequests[index] = plan.Requests[index];
            }

            var access = new InventorySerializedReservationSetAccess(
                this,
                operationId,
                claimId,
                Array.AsReadOnly(exactRequests),
                Revision + 1);
            var registration =
                new ManagedSerializedReservationSetRegistration(
                    operationId,
                    claimId,
                    Revision + 1,
                    exactRequests,
                    access);
            for (int index = 0; index < plan.Reservations.Count; index++)
            {
                InventoryReservation reservation = plan.Reservations[index];
                _reservations.Add(reservation.Id, reservation);
            }

            _managedSerializedReservationSets.Add(claimId, registration);
            _managedSerializedReservationSetOperations.Add(
                operationId,
                registration);
            Revision++;
            return OperationResult<InventorySerializedReservationSetAccess>.Success(access);
        }

        internal OperationResult<InventorySerializedReservationSetPlan>
            PrepareSerializedItemReservationSet(
                IReadOnlyList<InventorySerializedReservationRequest> requests)
        {
            if (requests == null)
            {
                return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                    InventoryFailures.MissingSerializedReservationSet);
            }

            if (requests.Count == 0)
            {
                return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                    InventoryFailures.EmptySerializedReservationSet);
            }

            var requestIds = new HashSet<StableId<ReservationIdScope>>();
            var itemIds = new HashSet<StableId<ItemInstanceIdScope>>();
            var exactRequests = new List<InventorySerializedReservationRequest>(
                requests.Count);
            var reservations = new List<InventoryReservation>(requests.Count);
            int replayCount = 0;

            for (int index = 0; index < requests.Count; index++)
            {
                InventorySerializedReservationRequest request = requests[index];
                if (request == null)
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        InventoryFailures.NullSerializedReservationRequest);
                }

                Failure identityFailure = ValidateReservationIdentity(
                    request.ReservationId,
                    request.ClaimId);
                if (!identityFailure.IsNone)
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        identityFailure);
                }

                if (_managedSerializedReservationSets.ContainsKey(request.ClaimId))
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        InventoryFailures.SerializedReservationClaimManaged);
                }

                if (request.ItemId.IsEmpty)
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        InventoryFailures.InvalidItemId);
                }

                if (!requestIds.Add(request.ReservationId))
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        InventoryFailures.DuplicateSerializedReservationRequest);
                }

                if (!itemIds.Add(request.ItemId))
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        InventoryFailures.DuplicateSerializedReservationItem);
                }

                if (!_items.TryGetValue(request.ItemId, out InventoryItemRecord item))
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        InventoryFailures.UnknownItem);
                }

                if (_managedSerializedTransferContainers.ContainsKey(item.ContainerId))
                {
                    return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                        InventoryFailures.SerializedTransferContainerManaged);
                }

                InventoryReservation exactExisting = null;
                if (_reservations.TryGetValue(
                        request.ReservationId,
                        out InventoryReservation byId))
                {
                    if (!MatchesSerializedReservation(byId, request))
                    {
                        return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                            InventoryFailures.DuplicateReservation);
                    }

                    exactExisting = byId;
                    replayCount++;
                }

                foreach (InventoryReservation existing in _reservations.Values)
                {
                    if (existing.TargetKind ==
                            InventoryReservationTargetKind.SerializedItem &&
                        existing.ItemId == request.ItemId &&
                        !ReferenceEquals(existing, exactExisting))
                    {
                        return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                            InventoryFailures.ItemAlreadyReserved);
                    }
                }

                exactRequests.Add(request);
                reservations.Add(exactExisting ?? new InventoryReservation(
                    request.ReservationId,
                    request.ClaimId,
                    InventoryReservationTargetKind.SerializedItem,
                    request.ItemId,
                    default,
                    default,
                    1,
                    InventoryReservationReleasePolicy.Releasable));
            }

            if (replayCount > 0 && replayCount != requests.Count)
            {
                return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                    InventoryFailures.PartialSerializedReservationReplay);
            }

            if (replayCount == 0 && Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedReservationSetPlan>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            exactRequests.Sort((left, right) => string.Compare(
                left.ReservationId.Value,
                right.ReservationId.Value,
                StringComparison.Ordinal));
            reservations.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return OperationResult<InventorySerializedReservationSetPlan>.Success(
                new InventorySerializedReservationSetPlan(
                    this,
                    Revision,
                    Array.AsReadOnly(exactRequests.ToArray()),
                    Array.AsReadOnly(reservations.ToArray()),
                    replayCount == requests.Count));
        }

        internal OperationResult CommitPreparedSerializedItemReservationSet(
            InventorySerializedReservationSetPlan plan)
        {
            if (plan == null ||
                !ReferenceEquals(plan.Owner, this) ||
                plan.Requests == null ||
                plan.Reservations == null ||
                plan.Requests.Count == 0 ||
                plan.Requests.Count != plan.Reservations.Count)
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedReservationSetPlanInvalid);
            }

            if (HasExactSerializedReservationSet(plan.Requests))
            {
                return OperationResult.Success();
            }

            if (Revision != plan.ExpectedRevision)
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedReservationSetPlanStale);
            }

            OperationResult<InventorySerializedReservationSetPlan> current =
                PrepareSerializedItemReservationSet(plan.Requests);
            if (current.IsFailure || current.Value.IsReplay)
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedReservationSetPlanStale);
            }

            for (int index = 0; index < plan.Reservations.Count; index++)
            {
                InventoryReservation reservation = plan.Reservations[index];
                _reservations.Add(reservation.Id, reservation);
            }

            Revision++;
            return OperationResult.Success();
        }

        public OperationResult<InventorySerializedReservationPlan>
            PrepareSerializedItemReservation(
                StableId<ReservationIdScope> reservationId,
                StableId<InventoryClaimIdScope> claimId,
                StableId<ItemInstanceIdScope> itemId)
        {
            return PrepareSerializedItemReservation(
                reservationId,
                claimId,
                itemId,
                InventoryReservationReleasePolicy.Releasable);
        }

        internal OperationResult<InventorySerializedReservationPlan>
            PrepareSerializedItemReservationForConsumption(
                StableId<ReservationIdScope> reservationId,
                StableId<InventoryClaimIdScope> claimId,
                StableId<ItemInstanceIdScope> itemId)
        {
            return PrepareSerializedItemReservation(
                reservationId,
                claimId,
                itemId,
                InventoryReservationReleasePolicy.ConsumeOnly);
        }

        private OperationResult<InventorySerializedReservationPlan>
            PrepareSerializedItemReservation(
                StableId<ReservationIdScope> reservationId,
                StableId<InventoryClaimIdScope> claimId,
                StableId<ItemInstanceIdScope> itemId,
                InventoryReservationReleasePolicy releasePolicy)
        {
            Failure identityFailure = ValidateReservationIdentity(reservationId, claimId);
            if (!identityFailure.IsNone)
            {
                return OperationResult<InventorySerializedReservationPlan>.Fail(identityFailure);
            }

            if (_managedSerializedReservationSets.ContainsKey(claimId))
            {
                return OperationResult<InventorySerializedReservationPlan>.Fail(
                    InventoryFailures.SerializedReservationClaimManaged);
            }

            if (_reservations.ContainsKey(reservationId))
            {
                return OperationResult<InventorySerializedReservationPlan>.Fail(
                    InventoryFailures.DuplicateReservation);
            }

            if (!_items.ContainsKey(itemId))
            {
                return OperationResult<InventorySerializedReservationPlan>.Fail(
                    InventoryFailures.UnknownItem);
            }

            if (_managedSerializedTransferContainers.ContainsKey(_items[itemId].ContainerId))
            {
                return OperationResult<InventorySerializedReservationPlan>.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            foreach (InventoryReservation existing in _reservations.Values)
            {
                if (existing.TargetKind == InventoryReservationTargetKind.SerializedItem &&
                    existing.ItemId == itemId)
                {
                    return OperationResult<InventorySerializedReservationPlan>.Fail(
                        InventoryFailures.ItemAlreadyReserved);
                }
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<InventorySerializedReservationPlan>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var reservation = new InventoryReservation(
                reservationId,
                claimId,
                InventoryReservationTargetKind.SerializedItem,
                itemId,
                default,
                default,
                1,
                releasePolicy);
            return OperationResult<InventorySerializedReservationPlan>.Success(
                new InventorySerializedReservationPlan(this, Revision, reservation));
        }

        private bool HasExactSerializedReservationSet(
            IReadOnlyList<InventorySerializedReservationRequest> requests)
        {
            if (requests == null || requests.Count == 0)
            {
                return false;
            }

            for (int index = 0; index < requests.Count; index++)
            {
                InventorySerializedReservationRequest request = requests[index];
                if (request == null ||
                    !_reservations.TryGetValue(
                        request.ReservationId,
                        out InventoryReservation reservation) ||
                    !MatchesSerializedReservation(reservation, request))
                {
                    return false;
                }
            }

            return true;
        }

        internal bool OwnsManagedSerializedReservationSet(
            InventorySerializedReservationSetAccess access)
        {
            if (access == null ||
                !ReferenceEquals(access.Owner, this) ||
                access.OperationId.IsEmpty ||
                access.ManagedClaimId.IsEmpty ||
                access.AppliedRevision <= 0 ||
                access.AppliedRevision > Revision ||
                !_managedSerializedReservationSets.TryGetValue(
                    access.ManagedClaimId,
                    out ManagedSerializedReservationSetRegistration byClaim) ||
                !_managedSerializedReservationSetOperations.TryGetValue(
                    access.OperationId,
                    out ManagedSerializedReservationSetRegistration byOperation))
            {
                return false;
            }

            return ReferenceEquals(byClaim, byOperation) &&
                   ReferenceEquals(access, byClaim.Access) &&
                   byClaim.OperationId == access.OperationId &&
                   byClaim.ClaimId == access.ManagedClaimId &&
                   byClaim.AppliedRevision == access.AppliedRevision &&
                   MatchesManagedSerializedReservationRequests(
                       byClaim.Requests,
                       access.Requests) &&
                   HasExactManagedSerializedReservationSet(access);
        }

        private bool HasExactManagedSerializedReservationSet(
            InventorySerializedReservationSetAccess access)
        {
            if (access == null ||
                !ReferenceEquals(access.Owner, this) ||
                access.OperationId.IsEmpty ||
                access.ManagedClaimId.IsEmpty ||
                access.AppliedRevision <= 0 ||
                access.AppliedRevision > Revision ||
                access.Requests == null ||
                access.Requests.Count == 0)
            {
                return false;
            }

            int claimReservationCount = 0;
            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.ClaimId == access.ManagedClaimId)
                {
                    claimReservationCount++;
                }
            }

            if (claimReservationCount != access.Requests.Count)
            {
                return false;
            }

            var reservationIds = new HashSet<StableId<ReservationIdScope>>();
            var itemIds = new HashSet<StableId<ItemInstanceIdScope>>();
            string previousReservationId = null;
            for (int index = 0; index < access.Requests.Count; index++)
            {
                InventorySerializedReservationRequest request = access.Requests[index];
                if (request == null ||
                    request.ClaimId != access.ManagedClaimId ||
                    !reservationIds.Add(request.ReservationId) ||
                    !itemIds.Add(request.ItemId) ||
                    (previousReservationId != null &&
                     string.Compare(
                         previousReservationId,
                         request.ReservationId.Value,
                         StringComparison.Ordinal) >= 0) ||
                    !_reservations.TryGetValue(
                        request.ReservationId,
                        out InventoryReservation reservation) ||
                    !MatchesSerializedReservation(reservation, request))
                {
                    return false;
                }

                previousReservationId = request.ReservationId.Value;
            }

            return true;
        }

        private static bool MatchesManagedSerializedReservationRequests(
            IReadOnlyList<InventorySerializedReservationRequest> expected,
            IReadOnlyList<InventorySerializedReservationRequest> actual)
        {
            if (expected == null ||
                actual == null ||
                expected.Count == 0 ||
                expected.Count != actual.Count)
            {
                return false;
            }

            var matched = new bool[expected.Count];
            for (int actualIndex = 0; actualIndex < actual.Count; actualIndex++)
            {
                InventorySerializedReservationRequest candidate = actual[actualIndex];
                if (candidate == null)
                {
                    return false;
                }

                bool found = false;
                for (int expectedIndex = 0; expectedIndex < expected.Count; expectedIndex++)
                {
                    InventorySerializedReservationRequest owned = expected[expectedIndex];
                    if (matched[expectedIndex] ||
                        owned == null ||
                        owned.ReservationId != candidate.ReservationId ||
                        owned.ClaimId != candidate.ClaimId ||
                        owned.ItemId != candidate.ItemId)
                    {
                        continue;
                    }

                    matched[expectedIndex] = true;
                    found = true;
                    break;
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesSerializedReservation(
            InventoryReservation reservation,
            InventorySerializedReservationRequest request)
        {
            return reservation != null &&
                   request != null &&
                   reservation.Id == request.ReservationId &&
                   reservation.ClaimId == request.ClaimId &&
                   reservation.TargetKind ==
                       InventoryReservationTargetKind.SerializedItem &&
                   reservation.ItemId == request.ItemId &&
                   reservation.BatchId.IsEmpty &&
                   reservation.ContainerId.IsEmpty &&
                   reservation.Quantity == 1 &&
                   reservation.ReleasePolicy ==
                       InventoryReservationReleasePolicy.Releasable;
        }

        public OperationResult CommitPreparedSerializedItemReservation(
            InventorySerializedReservationPlan plan)
        {
            if (plan == null || !ReferenceEquals(plan.Owner, this))
            {
                return OperationResult.Fail(InventoryFailures.ReservationPlanInvalid);
            }

            if (_reservations.TryGetValue(
                    plan.ReservationId,
                    out InventoryReservation existing) &&
                ReferenceEquals(existing, plan.Reservation))
            {
                return OperationResult.Success();
            }

            if (Revision != plan.ExpectedRevision)
            {
                return OperationResult.Fail(InventoryFailures.ReservationPlanStale);
            }

            OperationResult<InventorySerializedReservationPlan> current =
                PrepareSerializedItemReservation(
                    plan.ReservationId,
                    plan.ClaimId,
                    plan.ItemId);
            if (current.IsFailure)
            {
                return current.Error == InventoryFailures.RevisionOverflow
                    ? OperationResult.Fail(current.Error)
                    : OperationResult.Fail(InventoryFailures.ReservationPlanStale);
            }

            _reservations.Add(plan.ReservationId, plan.Reservation);
            Revision++;
            return OperationResult.Success();
        }

        public OperationResult ReserveBatch(
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId,
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId,
            int quantity)
        {
            Failure identityFailure = ValidateReservationIdentity(reservationId, claimId);
            if (!identityFailure.IsNone)
            {
                return OperationResult.Fail(identityFailure);
            }

            if (_managedSerializedReservationSets.ContainsKey(claimId))
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedReservationClaimManaged);
            }

            if (quantity <= 0)
            {
                return OperationResult.Fail(InventoryFailures.InvalidQuantity);
            }

            if (_reservations.ContainsKey(reservationId))
            {
                return OperationResult.Fail(InventoryFailures.DuplicateReservation);
            }

            if (!_batches.ContainsKey(batchId))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatch);
            }

            if (_managedSerializedTransferContainers.ContainsKey(containerId))
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedTransferContainerManaged);
            }

            var position = new BatchPositionKey(batchId, containerId);
            if (!_batchQuantities.TryGetValue(position, out int storedQuantity))
            {
                return OperationResult.Fail(InventoryFailures.UnknownBatchPosition);
            }

            int reservedQuantity = GetReservedBatchQuantityUnsafe(batchId, containerId);
            if ((long)storedQuantity - reservedQuantity < quantity)
            {
                return OperationResult.Fail(InventoryFailures.InsufficientAvailable);
            }

            _reservations.Add(
                reservationId,
                new InventoryReservation(
                    reservationId,
                    claimId,
                    InventoryReservationTargetKind.BatchPosition,
                    default,
                    batchId,
                    containerId,
                    quantity,
                    InventoryReservationReleasePolicy.Releasable));
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ReleaseReservation(StableId<ReservationIdScope> reservationId)
        {
            if (!_reservations.TryGetValue(
                    reservationId,
                    out InventoryReservation reservation))
            {
                return OperationResult.Fail(InventoryFailures.UnknownReservation);
            }

            if (_managedSerializedReservationSets.ContainsKey(reservation.ClaimId))
            {
                return OperationResult.Fail(
                    InventoryFailures.SerializedReservationClaimManaged);
            }

            if (reservation.ReleasePolicy == InventoryReservationReleasePolicy.ConsumeOnly)
            {
                return OperationResult.Fail(InventoryFailures.ReservationReleaseRestricted);
            }

            _reservations.Remove(reservationId);
            AdvanceRevision();
            return OperationResult.Success();
        }

        public OperationResult ConsumeReservation(StableId<ReservationIdScope> reservationId)
        {
            return ConsumeReservations(new[] { reservationId }, false);
        }

        /// <summary>
        /// Atomically consumes an exact set of reservations. Every target is preflighted before
        /// any item, batch position or reservation is removed, and one successful set advances
        /// Inventory Revision exactly once.
        /// </summary>
        public OperationResult ConsumeReservations(
            IReadOnlyList<StableId<ReservationIdScope>> reservationIds)
        {
            return ConsumeReservations(reservationIds, false);
        }

        internal OperationResult ConsumeCheckoutReservations(
            IReadOnlyList<StableId<ReservationIdScope>> reservationIds)
        {
            OperationResult<InventoryCheckoutConsumptionPlan> prepared =
                PrepareCheckoutReservationConsumption(reservationIds);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitPreparedCheckoutReservationConsumption(prepared.Value);
        }

        internal OperationResult<InventoryCheckoutConsumptionPlan>
            PrepareCheckoutReservationConsumption(
                IReadOnlyList<StableId<ReservationIdScope>> reservationIds)
        {
            OperationResult<ReservationConsumptionSelection> prepared =
                PrepareReservationConsumption(reservationIds, true);
            if (prepared.IsFailure)
            {
                return OperationResult<InventoryCheckoutConsumptionPlan>.Fail(prepared.Error);
            }

            return OperationResult<InventoryCheckoutConsumptionPlan>.Success(
                new InventoryCheckoutConsumptionPlan(this, Revision, reservationIds));
        }

        internal OperationResult CommitPreparedCheckoutReservationConsumption(
            InventoryCheckoutConsumptionPlan plan)
        {
            if (plan == null ||
                !ReferenceEquals(plan.Owner, this) ||
                plan.ReservationIds == null ||
                plan.ReservationIds.Count == 0)
            {
                return OperationResult.Fail(
                    InventoryFailures.CheckoutConsumptionPlanInvalid);
            }

            if (plan.ExpectedRevision != Revision)
            {
                return OperationResult.Fail(
                    InventoryFailures.CheckoutConsumptionPlanStale);
            }

            return ConsumeReservations(plan.ReservationIds, true);
        }

        private OperationResult ConsumeReservations(
            IReadOnlyList<StableId<ReservationIdScope>> reservationIds,
            bool allowConsumeOnly)
        {
            OperationResult<ReservationConsumptionSelection> prepared =
                PrepareReservationConsumption(reservationIds, allowConsumeOnly);
            return prepared.IsFailure
                ? OperationResult.Fail(prepared.Error)
                : CommitReservationConsumption(prepared.Value);
        }

        private OperationResult<ReservationConsumptionSelection> PrepareReservationConsumption(
            IReadOnlyList<StableId<ReservationIdScope>> reservationIds,
            bool allowConsumeOnly)
        {
            if (reservationIds == null)
            {
                return OperationResult<ReservationConsumptionSelection>.Fail(
                    InventoryFailures.MissingReservationSet);
            }

            if (reservationIds.Count == 0)
            {
                return OperationResult<ReservationConsumptionSelection>.Fail(
                    InventoryFailures.EmptyReservationSet);
            }

            var selectedIds = new HashSet<StableId<ReservationIdScope>>();
            var selected = new List<InventoryReservation>(reservationIds.Count);
            var serializedItems = new HashSet<StableId<ItemInstanceIdScope>>();
            var batchConsumption = new Dictionary<BatchPositionKey, long>();
            for (int index = 0; index < reservationIds.Count; index++)
            {
                StableId<ReservationIdScope> reservationId = reservationIds[index];
                if (reservationId.IsEmpty)
                {
                    return OperationResult<ReservationConsumptionSelection>.Fail(
                        InventoryFailures.InvalidReservationId);
                }

                if (!selectedIds.Add(reservationId))
                {
                    return OperationResult<ReservationConsumptionSelection>.Fail(
                        InventoryFailures.DuplicateReservationInSet);
                }

                if (!_reservations.TryGetValue(reservationId, out InventoryReservation reservation))
                {
                    return OperationResult<ReservationConsumptionSelection>.Fail(
                        InventoryFailures.UnknownReservation);
                }

                if (reservation == null ||
                    reservation.Id != reservationId ||
                    reservation.ClaimId.IsEmpty)
                {
                    return OperationResult<ReservationConsumptionSelection>.Fail(
                        InventoryFailures.InvariantViolation);
                }

                if (_managedSerializedReservationSets.ContainsKey(reservation.ClaimId))
                {
                    return OperationResult<ReservationConsumptionSelection>.Fail(
                        InventoryFailures.SerializedReservationClaimManaged);
                }

                if (!allowConsumeOnly &&
                    reservation.ReleasePolicy == InventoryReservationReleasePolicy.ConsumeOnly)
                {
                    return OperationResult<ReservationConsumptionSelection>.Fail(
                        InventoryFailures.ReservationConsumptionRestricted);
                }

                if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem)
                {
                    if (reservation.Quantity != 1 ||
                        !_items.TryGetValue(reservation.ItemId, out InventoryItemRecord item) ||
                        !serializedItems.Add(reservation.ItemId))
                    {
                        return OperationResult<ReservationConsumptionSelection>.Fail(
                            InventoryFailures.InvariantViolation);
                    }

                    if (_managedSerializedTransferContainers.ContainsKey(item.ContainerId))
                    {
                        return OperationResult<ReservationConsumptionSelection>.Fail(
                            InventoryFailures.SerializedTransferContainerManaged);
                    }
                }
                else if (reservation.TargetKind == InventoryReservationTargetKind.BatchPosition)
                {
                    var position = new BatchPositionKey(
                        reservation.BatchId,
                        reservation.ContainerId);
                    if (reservation.Quantity <= 0 ||
                        !_batches.ContainsKey(reservation.BatchId) ||
                        !_batchQuantities.TryGetValue(position, out int storedQuantity))
                    {
                        return OperationResult<ReservationConsumptionSelection>.Fail(
                            InventoryFailures.InvariantViolation);
                    }

                    if (_managedSerializedTransferContainers.ContainsKey(reservation.ContainerId))
                    {
                        return OperationResult<ReservationConsumptionSelection>.Fail(
                            InventoryFailures.SerializedTransferContainerManaged);
                    }

                    batchConsumption.TryGetValue(position, out long selectedQuantity);
                    if (selectedQuantity > storedQuantity - (long)reservation.Quantity)
                    {
                        return OperationResult<ReservationConsumptionSelection>.Fail(
                            InventoryFailures.InvariantViolation);
                    }

                    batchConsumption[position] = selectedQuantity + reservation.Quantity;
                }
                else
                {
                    return OperationResult<ReservationConsumptionSelection>.Fail(
                        InventoryFailures.InvariantViolation);
                }

                selected.Add(reservation);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<ReservationConsumptionSelection>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            return OperationResult<ReservationConsumptionSelection>.Success(
                new ReservationConsumptionSelection(
                    Array.AsReadOnly(selected.ToArray()),
                    serializedItems,
                    batchConsumption));
        }

        private OperationResult CommitReservationConsumption(
            ReservationConsumptionSelection selection)
        {
            foreach (StableId<ItemInstanceIdScope> itemId in selection.SerializedItems)
            {
                _items.Remove(itemId);
            }

            foreach (KeyValuePair<BatchPositionKey, long> consumption in selection.BatchConsumption)
            {
                int remaining = _batchQuantities[consumption.Key] - (int)consumption.Value;
                if (remaining == 0)
                {
                    _batchQuantities.Remove(consumption.Key);
                }
                else
                {
                    _batchQuantities[consumption.Key] = remaining;
                }
            }

            var affectedBatches = new HashSet<StableId<BatchIdScope>>();
            for (int index = 0; index < selection.Reservations.Count; index++)
            {
                InventoryReservation reservation = selection.Reservations[index];
                _reservations.Remove(reservation.Id);
                if (reservation.TargetKind == InventoryReservationTargetKind.BatchPosition)
                {
                    affectedBatches.Add(reservation.BatchId);
                }
            }

            foreach (StableId<BatchIdScope> batchId in affectedBatches)
            {
                if (!HasBatchPositionsUnsafe(batchId))
                {
                    _batches.Remove(batchId);
                }
            }

            Revision++;
            return OperationResult.Success();
        }

        public bool TryGetSerializedItem(
            StableId<ItemInstanceIdScope> itemId,
            out InventoryItemRecord item)
        {
            return _items.TryGetValue(itemId, out item);
        }

        public bool TryGetBatch(
            StableId<BatchIdScope> batchId,
            out InventoryBatchRecord batch)
        {
            return _batches.TryGetValue(batchId, out batch);
        }

        public bool TryGetContainer(
            StableId<ContainerIdScope> containerId,
            out InventoryContainerDefinition container)
        {
            return _containers.TryGetValue(containerId, out container);
        }

        public bool TryGetReservation(
            StableId<ReservationIdScope> reservationId,
            out InventoryReservation reservation)
        {
            return _reservations.TryGetValue(reservationId, out reservation);
        }

        public OperationResult<int> GetBatchQuantity(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId)
        {
            if (!_batches.ContainsKey(batchId))
            {
                return OperationResult<int>.Fail(InventoryFailures.UnknownBatch);
            }

            var key = new BatchPositionKey(batchId, containerId);
            return _batchQuantities.TryGetValue(key, out int quantity)
                ? OperationResult<int>.Success(quantity)
                : OperationResult<int>.Fail(InventoryFailures.UnknownBatchPosition);
        }

        public OperationResult<long> GetTotalQuantity(StableId<ProductDefinitionIdScope> productId)
        {
            Failure productFailure = ValidateKnownProduct(productId);
            if (!productFailure.IsNone)
            {
                return OperationResult<long>.Fail(productFailure);
            }

            long total = 0;
            foreach (InventoryItemRecord item in _items.Values)
            {
                if (item.ProductId == productId)
                {
                    total++;
                }
            }

            foreach (KeyValuePair<BatchPositionKey, int> position in _batchQuantities)
            {
                if (_batches[position.Key.BatchId].ProductId == productId)
                {
                    total += position.Value;
                }
            }

            return OperationResult<long>.Success(total);
        }

        public OperationResult<long> GetAvailableQuantity(StableId<ProductDefinitionIdScope> productId)
        {
            OperationResult<long> totalResult = GetTotalQuantity(productId);
            if (totalResult.IsFailure)
            {
                return totalResult;
            }

            long reserved = 0;
            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem)
                {
                    if (_items.TryGetValue(reservation.ItemId, out InventoryItemRecord item) &&
                        item.ProductId == productId)
                    {
                        reserved++;
                    }
                }
                else if (_batches.TryGetValue(reservation.BatchId, out InventoryBatchRecord batch) &&
                         batch.ProductId == productId)
                {
                    reserved += reservation.Quantity;
                }
            }

            return OperationResult<long>.Success(totalResult.Value - reserved);
        }

        public OperationResult<long> GetContainerQuantity(StableId<ContainerIdScope> containerId)
        {
            if (!_containers.ContainsKey(containerId))
            {
                return OperationResult<long>.Fail(InventoryFailures.UnknownContainer);
            }

            return OperationResult<long>.Success(GetContainerLoadUnsafe(containerId));
        }

        public IReadOnlyList<InventoryContainerDefinition> GetContainers()
        {
            var values = new List<InventoryContainerDefinition>(_containers.Values);
            values.Sort((left, right) => string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public IReadOnlyList<InventoryItemRecord> GetSerializedItems()
        {
            var values = new List<InventoryItemRecord>(_items.Values);
            values.Sort((left, right) => string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public IReadOnlyList<InventoryBatchPosition> GetBatchPositions()
        {
            var values = new List<InventoryBatchPosition>();
            foreach (KeyValuePair<BatchPositionKey, int> position in _batchQuantities)
            {
                values.Add(new InventoryBatchPosition(
                    position.Key.BatchId,
                    position.Key.ContainerId,
                    position.Value));
            }

            values.Sort(CompareBatchPositions);
            return Array.AsReadOnly(values.ToArray());
        }

        public IReadOnlyList<InventoryReservation> GetReservations()
        {
            var values = new List<InventoryReservation>(_reservations.Values);
            values.Sort((left, right) => string.Compare(left.Id.Value, right.Id.Value, StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            var containerLoads = new Dictionary<StableId<ContainerIdScope>, long>();
            foreach (KeyValuePair<StableId<ContainerIdScope>, InventoryContainerDefinition> entry in _containers)
            {
                InventoryContainerDefinition definition = entry.Value;
                if (definition == null ||
                    entry.Key != definition.Id ||
                    definition.Id.IsEmpty ||
                    !InventoryValidation.IsValidContainerKind(definition.Kind) ||
                    definition.UnitCapacity <= 0)
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                containerLoads.Add(entry.Key, 0);
            }

            foreach (KeyValuePair<StableId<ContainerIdScope>, InventorySerializedTransferAccess> entry
                     in _managedSerializedTransferContainers)
            {
                InventorySerializedTransferAccess access = entry.Value;
                if (!_containers.ContainsKey(entry.Key) ||
                    access == null ||
                    !ReferenceEquals(access.Owner, this) ||
                    access.ManagedContainerId != entry.Key)
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            var reservedItems = new HashSet<StableId<ItemInstanceIdScope>>();
            var reservedBatches = new Dictionary<BatchPositionKey, long>();

            foreach (KeyValuePair<StableId<ItemInstanceIdScope>, InventoryItemRecord> entry in _items)
            {
                InventoryItemRecord item = entry.Value;
                if (item == null ||
                    entry.Key != item.Id ||
                    item.Id.IsEmpty ||
                    !_containers.ContainsKey(item.ContainerId) ||
                    !InventoryValidation.IsValidCondition(item.Condition) ||
                    !item.UnitCost.IsValid ||
                    !AreValidSerializedItemStateFlags(item.StateFlags) ||
                    !ProductHasTrackingPolicy(item.ProductId, ProductTrackingPolicy.SerializedInstance))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                containerLoads[item.ContainerId]++;
            }

            foreach (KeyValuePair<StableId<BatchIdScope>, InventoryBatchRecord> entry in _batches)
            {
                InventoryBatchRecord batch = entry.Value;
                if (batch == null ||
                    entry.Key != batch.Id ||
                    batch.Id.IsEmpty ||
                    !InventoryValidation.IsValidCondition(batch.Condition) ||
                    !batch.UnitCost.IsValid ||
                    !ProductHasTrackingPolicy(batch.ProductId, ProductTrackingPolicy.BatchQuantity) ||
                    !HasBatchPositionsUnsafe(batch.Id))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<BatchPositionKey, int> entry in _batchQuantities)
            {
                if (entry.Key.BatchId.IsEmpty ||
                    entry.Key.ContainerId.IsEmpty ||
                    entry.Value <= 0 ||
                    !_batches.ContainsKey(entry.Key.BatchId) ||
                    !containerLoads.ContainsKey(entry.Key.ContainerId) ||
                    _managedSerializedTransferContainers.ContainsKey(entry.Key.ContainerId))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                containerLoads[entry.Key.ContainerId] += entry.Value;
            }

            foreach (KeyValuePair<StableId<ReservationIdScope>, InventoryReservation> entry in _reservations)
            {
                InventoryReservation reservation = entry.Value;
                if (reservation == null ||
                    entry.Key != reservation.Id ||
                    reservation.Id.IsEmpty ||
                    reservation.ClaimId.IsEmpty ||
                    (reservation.ReleasePolicy != InventoryReservationReleasePolicy.Releasable &&
                     reservation.ReleasePolicy != InventoryReservationReleasePolicy.ConsumeOnly))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }

                if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem)
                {
                    if (reservation.Quantity != 1 ||
                        !_items.TryGetValue(reservation.ItemId, out InventoryItemRecord item) ||
                        (_managedSerializedTransferContainers.ContainsKey(item.ContainerId) &&
                         !IsValidReservedSerializedWorkOrderBuildKitCustody(
                             reservation,
                             item)) ||
                        !reservedItems.Add(reservation.ItemId))
                    {
                        return OperationResult.Fail(InventoryFailures.InvariantViolation);
                    }
                }
                else if (reservation.TargetKind == InventoryReservationTargetKind.BatchPosition)
                {
                    var key = new BatchPositionKey(reservation.BatchId, reservation.ContainerId);
                    if (reservation.Quantity <= 0 ||
                        _managedSerializedTransferContainers.ContainsKey(reservation.ContainerId) ||
                        !_batchQuantities.ContainsKey(key))
                    {
                        return OperationResult.Fail(InventoryFailures.InvariantViolation);
                    }

                    reservedBatches.TryGetValue(key, out long quantity);
                    reservedBatches[key] = quantity + reservation.Quantity;
                }
                else
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            if (_managedSerializedReservationSets.Count !=
                _managedSerializedReservationSetOperations.Count)
            {
                return OperationResult.Fail(InventoryFailures.InvariantViolation);
            }

            foreach (KeyValuePair<StableId<InventoryClaimIdScope>,
                ManagedSerializedReservationSetRegistration> entry in
                _managedSerializedReservationSets)
            {
                ManagedSerializedReservationSetRegistration registration = entry.Value;
                if (entry.Key.IsEmpty ||
                    registration == null ||
                    entry.Key != registration.ClaimId ||
                    !OwnsManagedSerializedReservationSet(registration.Access))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<
                StableId<InventorySerializedReservationSetOperationIdScope>,
                ManagedSerializedReservationSetRegistration> entry in
                _managedSerializedReservationSetOperations)
            {
                ManagedSerializedReservationSetRegistration registration = entry.Value;
                if (entry.Key.IsEmpty ||
                    registration == null ||
                    entry.Key != registration.OperationId ||
                    !_managedSerializedReservationSets.TryGetValue(
                        registration.ClaimId,
                        out ManagedSerializedReservationSetRegistration byClaim) ||
                    !ReferenceEquals(registration, byClaim) ||
                    !OwnsManagedSerializedReservationSet(registration.Access))
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            if (!HasValidSerializedReservationWorkOrderAllocations())
            {
                return OperationResult.Fail(InventoryFailures.InvariantViolation);
            }

            if (!HasValidSerializedReservationWorkOrderBuildKits())
            {
                return OperationResult.Fail(InventoryFailures.InvariantViolation);
            }

            foreach (KeyValuePair<BatchPositionKey, long> reserved in reservedBatches)
            {
                if (reserved.Value > _batchQuantities[reserved.Key])
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            foreach (KeyValuePair<StableId<ContainerIdScope>, long> load in containerLoads)
            {
                if (load.Value < 0 || load.Value > _containers[load.Key].UnitCapacity)
                {
                    return OperationResult.Fail(InventoryFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private Failure ValidateProduct(
            StableId<ProductDefinitionIdScope> productId,
            ProductTrackingPolicy expectedTrackingPolicy)
        {
            Failure knownFailure = ValidateKnownProduct(productId);
            if (!knownFailure.IsNone)
            {
                return knownFailure;
            }

            _catalog.TryGet(productId, out ProductDefinition definition);
            return definition.TrackingPolicy == expectedTrackingPolicy
                ? Failure.None
                : InventoryFailures.TrackingMismatch;
        }

        private Failure ValidateKnownProduct(StableId<ProductDefinitionIdScope> productId)
        {
            if (productId.IsEmpty)
            {
                return InventoryFailures.InvalidProductId;
            }

            return _catalog.TryGet(productId, out _)
                ? Failure.None
                : InventoryFailures.UnknownProduct;
        }

        private bool ProductHasTrackingPolicy(
            StableId<ProductDefinitionIdScope> productId,
            ProductTrackingPolicy policy)
        {
            return _catalog.TryGet(productId, out ProductDefinition definition) &&
                   definition.TrackingPolicy == policy;
        }

        private Failure ValidateCapacity(StableId<ContainerIdScope> containerId, long addedQuantity)
        {
            if (!_containers.TryGetValue(containerId, out InventoryContainerDefinition container))
            {
                return InventoryFailures.UnknownContainer;
            }

            long load = GetContainerLoadUnsafe(containerId);
            return load > container.UnitCapacity - (long)addedQuantity
                ? InventoryFailures.ContainerCapacityExceeded
                : Failure.None;
        }

        private Failure ValidateSerializedTransferAccess(
            StableId<ContainerIdScope> sourceContainerId,
            StableId<ContainerIdScope> targetContainerId,
            InventorySerializedTransferAccess access)
        {
            _managedSerializedTransferContainers.TryGetValue(
                sourceContainerId,
                out InventorySerializedTransferAccess sourceAccess);
            _managedSerializedTransferContainers.TryGetValue(
                targetContainerId,
                out InventorySerializedTransferAccess targetAccess);

            if (sourceAccess != null &&
                targetAccess != null &&
                !ReferenceEquals(sourceAccess, targetAccess))
            {
                return InventoryFailures.SerializedTransferAccessInvalid;
            }

            InventorySerializedTransferAccess requiredAccess = sourceAccess ?? targetAccess;

            if (requiredAccess == null)
            {
                return access == null
                    ? Failure.None
                    : InventoryFailures.SerializedTransferAccessInvalid;
            }

            if (access == null)
            {
                return InventoryFailures.SerializedTransferContainerManaged;
            }

            return ReferenceEquals(access.Owner, this) &&
                   ReferenceEquals(access, requiredAccess) &&
                   access.ManagedContainerId == requiredAccess.ManagedContainerId
                ? Failure.None
                : InventoryFailures.SerializedTransferAccessInvalid;
        }

        private bool IsSerializedItemReserved(StableId<ItemInstanceIdScope> itemId)
        {
            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem &&
                    reservation.ItemId == itemId)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasReservationTargetingContainerUnsafe(
            StableId<ContainerIdScope> containerId)
        {
            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.TargetKind == InventoryReservationTargetKind.SerializedItem)
                {
                    if (_items.TryGetValue(
                            reservation.ItemId,
                            out InventoryItemRecord item) &&
                        item.ContainerId == containerId)
                    {
                        return true;
                    }
                }
                else if (reservation.TargetKind == InventoryReservationTargetKind.BatchPosition &&
                         reservation.ContainerId == containerId)
                {
                    return true;
                }
            }

            return false;
        }

        private Failure ValidateReservationIdentity(
            StableId<ReservationIdScope> reservationId,
            StableId<InventoryClaimIdScope> claimId)
        {
            if (reservationId.IsEmpty)
            {
                return InventoryFailures.InvalidReservationId;
            }

            return claimId.IsEmpty
                ? InventoryFailures.InvalidClaimId
                : Failure.None;
        }

        private long GetContainerLoadUnsafe(StableId<ContainerIdScope> containerId)
        {
            long total = 0;
            foreach (InventoryItemRecord item in _items.Values)
            {
                if (item.ContainerId == containerId)
                {
                    total++;
                }
            }

            foreach (KeyValuePair<BatchPositionKey, int> position in _batchQuantities)
            {
                if (position.Key.ContainerId == containerId)
                {
                    total += position.Value;
                }
            }

            return total;
        }

        private int GetReservedBatchQuantityUnsafe(
            StableId<BatchIdScope> batchId,
            StableId<ContainerIdScope> containerId)
        {
            long total = 0;
            foreach (InventoryReservation reservation in _reservations.Values)
            {
                if (reservation.TargetKind == InventoryReservationTargetKind.BatchPosition &&
                    reservation.BatchId == batchId &&
                    reservation.ContainerId == containerId)
                {
                    total += reservation.Quantity;
                }
            }

            return total > int.MaxValue ? int.MaxValue : (int)total;
        }

        private bool HasBatchPositionsUnsafe(StableId<BatchIdScope> batchId)
        {
            foreach (BatchPositionKey position in _batchQuantities.Keys)
            {
                if (position.BatchId == batchId)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool AreValidSerializedItemStateFlags(
            InventorySerializedItemStateFlags stateFlags)
        {
            const InventorySerializedItemStateFlags allKnownFlags =
                InventorySerializedItemStateFlags.PreAppliedConsumableConsumed;
            return (stateFlags & ~allKnownFlags) == 0;
        }

        private void AdvanceRevision()
        {
            if (Revision < long.MaxValue)
            {
                Revision++;
            }
        }

        private static int CompareBatchPositions(InventoryBatchPosition left, InventoryBatchPosition right)
        {
            int batchComparison = string.Compare(left.BatchId.Value, right.BatchId.Value, StringComparison.Ordinal);
            return batchComparison != 0
                ? batchComparison
                : string.Compare(left.ContainerId.Value, right.ContainerId.Value, StringComparison.Ordinal);
        }
    }
}
