using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;

namespace PCShopEmpire3D.Inventory
{
    /// <summary>
    /// Inventory-owned proof that one exact managed reservation set has been allocated to a
    /// single work order and its physical ticket. Allocation preserves every serialized item
    /// and reservation; it never moves or consumes stock.
    /// </summary>
    internal sealed class InventorySerializedReservationWorkOrderAllocationReceipt
    {
        private readonly IReadOnlyList<InventorySerializedReservationRequest> _requests;

        internal InventorySerializedReservationWorkOrderAllocationReceipt(
            InventoryAuthority owner,
            InventorySerializedReservationSetAccess reservationSetAccess,
            StableId<InventorySerializedReservationWorkOrderOperationIdScope> operationId,
            StableId<InventorySerializedReservationWorkOrderIdScope> workOrderId,
            StableId<InventorySerializedReservationWorkTicketIdScope> workTicketId,
            StableId<ContainerIdScope> workbenchContainerId,
            long appliedRevision)
        {
            Owner = owner;
            ReservationSetAccess = reservationSetAccess;
            OperationId = operationId;
            WorkOrderId = workOrderId;
            WorkTicketId = workTicketId;
            WorkbenchContainerId = workbenchContainerId;
            AppliedRevision = appliedRevision;

            var exactRequests = new InventorySerializedReservationRequest[
                reservationSetAccess.Requests.Count];
            for (int index = 0; index < reservationSetAccess.Requests.Count; index++)
            {
                exactRequests[index] = reservationSetAccess.Requests[index];
            }

            _requests = Array.AsReadOnly(exactRequests);
        }

        internal InventoryAuthority Owner { get; }

        internal InventorySerializedReservationSetAccess ReservationSetAccess { get; }

        internal StableId<InventorySerializedReservationWorkOrderOperationIdScope>
            OperationId { get; }

        internal StableId<InventorySerializedReservationWorkOrderIdScope> WorkOrderId { get; }

        internal StableId<InventorySerializedReservationWorkTicketIdScope> WorkTicketId { get; }

        internal StableId<ContainerIdScope> WorkbenchContainerId { get; }

        internal StableId<InventoryClaimIdScope> ClaimId =>
            ReservationSetAccess.ManagedClaimId;

        internal IReadOnlyList<InventorySerializedReservationRequest> Requests => _requests;

        internal long AppliedRevision { get; }
    }

    public sealed partial class InventoryAuthority
    {
        private readonly Dictionary<StableId<InventoryClaimIdScope>,
            InventorySerializedReservationWorkOrderAllocationReceipt>
            _serializedReservationWorkOrderAllocationsByClaim =
                new Dictionary<StableId<InventoryClaimIdScope>,
                    InventorySerializedReservationWorkOrderAllocationReceipt>();
        private readonly Dictionary<
            StableId<InventorySerializedReservationWorkOrderOperationIdScope>,
            InventorySerializedReservationWorkOrderAllocationReceipt>
            _serializedReservationWorkOrderAllocationsByOperation =
                new Dictionary<
                    StableId<InventorySerializedReservationWorkOrderOperationIdScope>,
                    InventorySerializedReservationWorkOrderAllocationReceipt>();
        private readonly Dictionary<StableId<InventorySerializedReservationWorkOrderIdScope>,
            InventorySerializedReservationWorkOrderAllocationReceipt>
            _serializedReservationWorkOrderAllocationsByOrder =
                new Dictionary<StableId<InventorySerializedReservationWorkOrderIdScope>,
                    InventorySerializedReservationWorkOrderAllocationReceipt>();
        private readonly Dictionary<StableId<InventorySerializedReservationWorkTicketIdScope>,
            InventorySerializedReservationWorkOrderAllocationReceipt>
            _serializedReservationWorkOrderAllocationsByTicket =
                new Dictionary<StableId<InventorySerializedReservationWorkTicketIdScope>,
                    InventorySerializedReservationWorkOrderAllocationReceipt>();
        private readonly Dictionary<StableId<ContainerIdScope>,
            InventorySerializedReservationWorkOrderAllocationReceipt>
            _serializedReservationWorkOrderAllocationsByWorkbench =
                new Dictionary<StableId<ContainerIdScope>,
                    InventorySerializedReservationWorkOrderAllocationReceipt>();

        internal int SerializedReservationWorkOrderAllocationCount =>
            _serializedReservationWorkOrderAllocationsByClaim.Count;

        /// <summary>
        /// Allocates one live managed reservation set to one work order. Exact replay returns
        /// the original receipt without advancing Revision; every conflicting reuse fails
        /// before mutation. The physical ticket is a separate projection and does not consume
        /// the workbench container's capacity-one Inventory slot.
        /// </summary>
        internal OperationResult<InventorySerializedReservationWorkOrderAllocationReceipt>
            AllocateManagedSerializedReservationSetToWorkOrder(
                InventorySerializedReservationSetAccess reservationSetAccess,
                StableId<InventorySerializedReservationWorkOrderOperationIdScope> operationId,
                StableId<InventorySerializedReservationWorkOrderIdScope> workOrderId,
                StableId<InventorySerializedReservationWorkTicketIdScope> workTicketId,
                StableId<ContainerIdScope> workbenchContainerId)
        {
            if (reservationSetAccess == null ||
                !OwnsManagedSerializedReservationSet(reservationSetAccess))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    InventoryFailures.SerializedReservationSetAccessInvalid);
            }

            if (operationId.IsEmpty)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    InventoryFailures.InvalidSerializedReservationWorkOrderOperationId);
            }

            if (workOrderId.IsEmpty)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    InventoryFailures.InvalidSerializedReservationWorkOrderId);
            }

            if (workTicketId.IsEmpty)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    InventoryFailures.InvalidSerializedReservationWorkTicketId);
            }

            if (workbenchContainerId.IsEmpty ||
                !_containers.TryGetValue(
                    workbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderWorkbenchInvalid);
            }

            if (_serializedReservationWorkOrderAllocationsByOperation.TryGetValue(
                    operationId,
                    out InventorySerializedReservationWorkOrderAllocationReceipt existing))
            {
                return MatchesSerializedReservationWorkOrderAllocation(
                           existing,
                           reservationSetAccess,
                           operationId,
                           workOrderId,
                           workTicketId,
                           workbenchContainerId) &&
                       OwnsSerializedReservationWorkOrderAllocation(existing)
                    ? OperationResult<
                        InventorySerializedReservationWorkOrderAllocationReceipt>.Success(
                        existing)
                    : OperationResult<
                        InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                        InventoryFailures.SerializedReservationWorkOrderConflict);
            }

            if (_serializedReservationWorkOrderAllocationsByClaim.ContainsKey(
                    reservationSetAccess.ManagedClaimId) ||
                _serializedReservationWorkOrderAllocationsByOrder.ContainsKey(workOrderId) ||
                _serializedReservationWorkOrderAllocationsByTicket.ContainsKey(workTicketId) ||
                _serializedReservationWorkOrderAllocationsByWorkbench.ContainsKey(
                    workbenchContainerId))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    InventoryFailures.SerializedReservationWorkOrderConflict);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    InventoryFailures.RevisionOverflow);
            }

            var receipt = new InventorySerializedReservationWorkOrderAllocationReceipt(
                this,
                reservationSetAccess,
                operationId,
                workOrderId,
                workTicketId,
                workbenchContainerId,
                Revision + 1);
            _serializedReservationWorkOrderAllocationsByClaim.Add(receipt.ClaimId, receipt);
            _serializedReservationWorkOrderAllocationsByOperation.Add(operationId, receipt);
            _serializedReservationWorkOrderAllocationsByOrder.Add(workOrderId, receipt);
            _serializedReservationWorkOrderAllocationsByTicket.Add(workTicketId, receipt);
            _serializedReservationWorkOrderAllocationsByWorkbench.Add(
                workbenchContainerId,
                receipt);
            Revision++;
            return OperationResult<
                InventorySerializedReservationWorkOrderAllocationReceipt>.Success(receipt);
        }

        internal bool OwnsSerializedReservationWorkOrderAllocation(
            InventorySerializedReservationWorkOrderAllocationReceipt receipt)
        {
            if (receipt == null ||
                !ReferenceEquals(receipt.Owner, this) ||
                receipt.OperationId.IsEmpty ||
                receipt.WorkOrderId.IsEmpty ||
                receipt.WorkTicketId.IsEmpty ||
                receipt.WorkbenchContainerId.IsEmpty ||
                receipt.ClaimId.IsEmpty ||
                receipt.AppliedRevision <= 0 ||
                receipt.AppliedRevision > Revision ||
                receipt.Requests == null ||
                receipt.Requests.Count == 0 ||
                !_containers.TryGetValue(
                    receipt.WorkbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench ||
                !OwnsManagedSerializedReservationSet(receipt.ReservationSetAccess) ||
                !_serializedReservationWorkOrderAllocationsByClaim.TryGetValue(
                    receipt.ClaimId,
                    out InventorySerializedReservationWorkOrderAllocationReceipt byClaim) ||
                !_serializedReservationWorkOrderAllocationsByOperation.TryGetValue(
                    receipt.OperationId,
                    out InventorySerializedReservationWorkOrderAllocationReceipt byOperation) ||
                !_serializedReservationWorkOrderAllocationsByOrder.TryGetValue(
                    receipt.WorkOrderId,
                    out InventorySerializedReservationWorkOrderAllocationReceipt byOrder) ||
                !_serializedReservationWorkOrderAllocationsByTicket.TryGetValue(
                    receipt.WorkTicketId,
                    out InventorySerializedReservationWorkOrderAllocationReceipt byTicket) ||
                !_serializedReservationWorkOrderAllocationsByWorkbench.TryGetValue(
                    receipt.WorkbenchContainerId,
                    out InventorySerializedReservationWorkOrderAllocationReceipt byWorkbench))
            {
                return false;
            }

            return ReferenceEquals(receipt, byClaim) &&
                   ReferenceEquals(receipt, byOperation) &&
                   ReferenceEquals(receipt, byOrder) &&
                   ReferenceEquals(receipt, byTicket) &&
                   ReferenceEquals(receipt, byWorkbench) &&
                   receipt.ClaimId == receipt.ReservationSetAccess.ManagedClaimId &&
                   MatchesManagedSerializedReservationRequests(
                       receipt.ReservationSetAccess.Requests,
                       receipt.Requests);
        }

        private bool HasValidSerializedReservationWorkOrderAllocations()
        {
            int count = _serializedReservationWorkOrderAllocationsByClaim.Count;
            if (_serializedReservationWorkOrderAllocationsByOperation.Count != count ||
                _serializedReservationWorkOrderAllocationsByOrder.Count != count ||
                _serializedReservationWorkOrderAllocationsByTicket.Count != count ||
                _serializedReservationWorkOrderAllocationsByWorkbench.Count != count)
            {
                return false;
            }

            foreach (InventorySerializedReservationWorkOrderAllocationReceipt receipt in
                     _serializedReservationWorkOrderAllocationsByClaim.Values)
            {
                if (!OwnsSerializedReservationWorkOrderAllocation(receipt))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesSerializedReservationWorkOrderAllocation(
            InventorySerializedReservationWorkOrderAllocationReceipt receipt,
            InventorySerializedReservationSetAccess reservationSetAccess,
            StableId<InventorySerializedReservationWorkOrderOperationIdScope> operationId,
            StableId<InventorySerializedReservationWorkOrderIdScope> workOrderId,
            StableId<InventorySerializedReservationWorkTicketIdScope> workTicketId,
            StableId<ContainerIdScope> workbenchContainerId)
        {
            return receipt != null &&
                   ReferenceEquals(receipt.ReservationSetAccess, reservationSetAccess) &&
                   receipt.OperationId == operationId &&
                   receipt.WorkOrderId == workOrderId &&
                   receipt.WorkTicketId == workTicketId &&
                   receipt.WorkbenchContainerId == workbenchContainerId;
        }
    }
}
