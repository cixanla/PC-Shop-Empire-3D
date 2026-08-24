using System;
using System.Collections.Generic;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Retail;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PCShopEmpire3D.EditModeTests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("PSE.Presentation")]

namespace PCShopEmpire3D.Orders
{
    /// <summary>
    /// Opaque capability required to publish a custom-PC work order. The creating
    /// authority owns the only valid instance; callers cannot manufacture one.
    /// </summary>
    internal sealed class CustomPcWorkOrderIssueAccess
    {
        internal CustomPcWorkOrderIssueAccess(CustomPcWorkOrderAuthority owner)
        {
            Owner = owner;
        }

        internal CustomPcWorkOrderAuthority Owner { get; }
    }

    internal sealed class CustomPcWorkOrderAuthorityCreation
    {
        internal CustomPcWorkOrderAuthorityCreation(
            CustomPcWorkOrderAuthority authority,
            CustomPcWorkOrderIssueAccess issueAccess)
        {
            Authority = authority;
            IssueAccess = issueAccess;
        }

        internal CustomPcWorkOrderAuthority Authority { get; }

        internal CustomPcWorkOrderIssueAccess IssueAccess { get; }
    }

    /// <summary>
    /// Publishes immutable custom-PC build orders and physical work tickets from exact owned
    /// quote reservations. Inventory remains the stock authority; this aggregate records the
    /// customer-job projection and supports operation-keyed recovery after interrupted
    /// publication.
    /// </summary>
    public sealed class CustomPcWorkOrderAuthority
    {
        private readonly CustomPcQuoteAuthority _quotes;
        private readonly InventoryAuthority _inventory;
        private readonly StableId<ContainerIdScope> _workbenchContainerId;
        private readonly CustomPcWorkOrderIssueAccess _issueAccess;
        private readonly Dictionary<StableId<CustomPcBuildOrderIdScope>,
            CustomPcWorkOrderIssueResult> _issuesByOrder =
                new Dictionary<StableId<CustomPcBuildOrderIdScope>,
                    CustomPcWorkOrderIssueResult>();
        private readonly Dictionary<StableId<CustomPcWorkTicketIdScope>,
            CustomPcWorkOrderIssueResult> _issuesByTicket =
                new Dictionary<StableId<CustomPcWorkTicketIdScope>,
                    CustomPcWorkOrderIssueResult>();
        private readonly Dictionary<StableId<CustomPcQuoteIdScope>,
            CustomPcWorkOrderIssueResult> _issuesByQuote =
                new Dictionary<StableId<CustomPcQuoteIdScope>,
                    CustomPcWorkOrderIssueResult>();
        private readonly Dictionary<StableId<InventoryClaimIdScope>,
            CustomPcWorkOrderIssueResult> _issuesByClaim =
                new Dictionary<StableId<InventoryClaimIdScope>,
                    CustomPcWorkOrderIssueResult>();
        private readonly Dictionary<StableId<CustomPcWorkOrderOperationIdScope>,
            CustomPcWorkOrderIssueResult> _issuesByOperation =
                new Dictionary<StableId<CustomPcWorkOrderOperationIdScope>,
                    CustomPcWorkOrderIssueResult>();
        private readonly Dictionary<StableId<ContainerIdScope>,
            CustomPcWorkOrderIssueResult> _issuesByWorkbench =
                new Dictionary<StableId<ContainerIdScope>,
                    CustomPcWorkOrderIssueResult>();
        private readonly Dictionary<StableId<CustomPcBuildOrderIdScope>,
            InventorySerializedReservationWorkOrderAllocationReceipt> _allocationsByOrder =
                new Dictionary<StableId<CustomPcBuildOrderIdScope>,
                    InventorySerializedReservationWorkOrderAllocationReceipt>();

        private CustomPcWorkOrderAuthority(
            CustomPcQuoteAuthority quotes,
            StableId<ContainerIdScope> workbenchContainerId)
        {
            _quotes = quotes;
            _inventory = quotes.Inventory;
            _workbenchContainerId = workbenchContainerId;
            _issueAccess = new CustomPcWorkOrderIssueAccess(this);
        }

        public long Revision { get; private set; }

        public int WorkOrderCount => _issuesByOrder.Count;

        public int WorkTicketCount => _issuesByTicket.Count;

        public CustomPcQuoteAuthority Quotes => _quotes;

        public InventoryAuthority Inventory => _inventory;

        public StableId<ContainerIdScope> WorkbenchContainerId =>
            _workbenchContainerId;

        internal static OperationResult<CustomPcWorkOrderAuthorityCreation> Create(
            CustomPcQuoteAuthority quotes,
            StableId<ContainerIdScope> workbenchContainerId)
        {
            if (quotes == null)
            {
                return OperationResult<CustomPcWorkOrderAuthorityCreation>.Fail(
                    CustomPcWorkOrderFailures.MissingAuthority);
            }

            if (workbenchContainerId.IsEmpty ||
                !quotes.Inventory.TryGetContainer(
                    workbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench)
            {
                return OperationResult<CustomPcWorkOrderAuthorityCreation>.Fail(
                    CustomPcWorkOrderFailures.WorkbenchInvalid);
            }

            var authority = new CustomPcWorkOrderAuthority(
                quotes,
                workbenchContainerId);
            return quotes.TryRegisterWorkOrderPublisher(
                    workbenchContainerId,
                    authority)
                ? OperationResult<CustomPcWorkOrderAuthorityCreation>.Success(
                    new CustomPcWorkOrderAuthorityCreation(
                        authority,
                        authority._issueAccess))
                : OperationResult<CustomPcWorkOrderAuthorityCreation>.Fail(
                    CustomPcWorkOrderFailures.PublisherAlreadyRegistered);
        }

        internal OperationResult<CustomPcWorkOrderIssueResult> Issue(
            CustomPcWorkOrderIssueAccess issueAccess,
            StableId<CustomPcBuildOrderIdScope> workOrderId,
            StableId<CustomPcWorkTicketIdScope> workTicketId,
            StableId<CustomPcWorkOrderOperationIdScope> operationId,
            CustomPcQuoteRecord quote,
            SimulationTimestamp issuedAt)
        {
            if (!ReferenceEquals(issueAccess, _issueAccess) ||
                !ReferenceEquals(issueAccess?.Owner, this))
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                    CustomPcWorkOrderFailures.IssueAccessInvalid);
            }

            Failure inputFailure = ValidateIssueInput(
                workOrderId,
                workTicketId,
                operationId,
                quote,
                issuedAt);
            if (!inputFailure.IsNone)
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(inputFailure);
            }

            if (_issuesByOperation.TryGetValue(
                    operationId,
                    out CustomPcWorkOrderIssueResult existing))
            {
                return MatchesIssue(
                           existing,
                           workOrderId,
                           workTicketId,
                           operationId,
                           quote,
                           issuedAt) &&
                       OwnsIssue(existing)
                    ? OperationResult<CustomPcWorkOrderIssueResult>.Success(existing)
                    : OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                        CustomPcWorkOrderFailures.IdentityConflict);
            }

            if (_issuesByOrder.ContainsKey(workOrderId) ||
                _issuesByTicket.ContainsKey(workTicketId) ||
                _issuesByQuote.ContainsKey(quote.Id) ||
                _issuesByClaim.ContainsKey(quote.InventoryClaimId) ||
                _issuesByWorkbench.ContainsKey(_workbenchContainerId))
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                    CustomPcWorkOrderFailures.IdentityConflict);
            }

            if (Revision == long.MaxValue)
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                    CustomPcWorkOrderFailures.RevisionOverflow);
            }

            OperationResult<InventorySerializedReservationWorkOrderAllocationReceipt>
                allocation = PrepareInventoryAllocationForRecovery(
                    issueAccess,
                    workOrderId,
                    workTicketId,
                    operationId,
                    quote);
            if (allocation.IsFailure)
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(allocation.Error);
            }

            IReadOnlyList<CustomPcBuildOrderLineSnapshot> exactLines =
                CopyQuoteLines(quote.Lines);
            var buildOrder = new CustomPcBuildOrderRecord(
                workOrderId,
                workTicketId,
                operationId,
                quote,
                _workbenchContainerId,
                issuedAt,
                exactLines,
                allocation.Value.AppliedRevision);
            var workTicket = new CustomPcWorkTicketRecord(workTicketId, buildOrder);
            var issue = new CustomPcWorkOrderIssueResult(buildOrder, workTicket);

            _issuesByOrder.Add(workOrderId, issue);
            _issuesByTicket.Add(workTicketId, issue);
            _issuesByQuote.Add(quote.Id, issue);
            _issuesByClaim.Add(quote.InventoryClaimId, issue);
            _issuesByOperation.Add(operationId, issue);
            _issuesByWorkbench.Add(_workbenchContainerId, issue);
            _allocationsByOrder.Add(workOrderId, allocation.Value);
            Revision++;
            return OperationResult<CustomPcWorkOrderIssueResult>.Success(issue);
        }

        /// <summary>
        /// Internal two-phase seam used by save/recovery and tests. A successful call may be
        /// followed by an interrupted publication; an exact Issue retry reuses the Inventory
        /// receipt without a second mutation.
        /// </summary>
        internal OperationResult<InventorySerializedReservationWorkOrderAllocationReceipt>
            PrepareInventoryAllocationForRecovery(
                CustomPcWorkOrderIssueAccess issueAccess,
                StableId<CustomPcBuildOrderIdScope> workOrderId,
                StableId<CustomPcWorkTicketIdScope> workTicketId,
                StableId<CustomPcWorkOrderOperationIdScope> operationId,
                CustomPcQuoteRecord quote)
        {
            if (!ReferenceEquals(issueAccess, _issueAccess) ||
                !ReferenceEquals(issueAccess?.Owner, this))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    CustomPcWorkOrderFailures.IssueAccessInvalid);
            }

            if (workOrderId.IsEmpty ||
                workTicketId.IsEmpty ||
                operationId.IsEmpty ||
                quote == null)
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    CustomPcWorkOrderFailures.InputInvalid);
            }

            if (!_quotes.TryGetOwnedReservationSet(
                    quote,
                    out InventorySerializedReservationSetAccess reservationSetAccess))
            {
                return OperationResult<
                    InventorySerializedReservationWorkOrderAllocationReceipt>.Fail(
                    CustomPcWorkOrderFailures.QuoteReservationDrift);
            }

            return _inventory.AllocateManagedSerializedReservationSetToWorkOrder(
                reservationSetAccess,
                ToInventoryOperationId(operationId),
                ToInventoryWorkOrderId(workOrderId),
                ToInventoryWorkTicketId(workTicketId),
                _workbenchContainerId);
        }

        public bool TryGetWorkOrder(
            StableId<CustomPcBuildOrderIdScope> workOrderId,
            out CustomPcBuildOrderRecord workOrder)
        {
            if (_issuesByOrder.TryGetValue(
                    workOrderId,
                    out CustomPcWorkOrderIssueResult issue))
            {
                workOrder = issue.BuildOrder;
                return true;
            }

            workOrder = null;
            return false;
        }

        public bool TryGetWorkTicket(
            StableId<CustomPcWorkTicketIdScope> workTicketId,
            out CustomPcWorkTicketRecord workTicket)
        {
            if (_issuesByTicket.TryGetValue(
                    workTicketId,
                    out CustomPcWorkOrderIssueResult issue))
            {
                workTicket = issue.WorkTicket;
                return true;
            }

            workTicket = null;
            return false;
        }

        public bool TryGetWorkOrderForQuote(
            StableId<CustomPcQuoteIdScope> quoteId,
            out CustomPcBuildOrderRecord workOrder)
        {
            if (_issuesByQuote.TryGetValue(
                    quoteId,
                    out CustomPcWorkOrderIssueResult issue))
            {
                workOrder = issue.BuildOrder;
                return true;
            }

            workOrder = null;
            return false;
        }

        public IReadOnlyList<CustomPcBuildOrderRecord> GetWorkOrders()
        {
            var values = new List<CustomPcBuildOrderRecord>(_issuesByOrder.Count);
            foreach (CustomPcWorkOrderIssueResult issue in _issuesByOrder.Values)
            {
                values.Add(issue.BuildOrder);
            }

            values.Sort((left, right) => string.Compare(
                left.Id.Value,
                right.Id.Value,
                StringComparison.Ordinal));
            return Array.AsReadOnly(values.ToArray());
        }

        public OperationResult ValidateInvariants()
        {
            int count = _issuesByOrder.Count;
            if (_quotes == null ||
                _inventory == null ||
                !ReferenceEquals(_inventory, _quotes.Inventory) ||
                _quotes.ValidateInvariants().IsFailure ||
                _inventory.ValidateInvariants().IsFailure ||
                !_inventory.TryGetContainer(
                    _workbenchContainerId,
                    out InventoryContainerDefinition workbench) ||
                workbench.Kind != InventoryContainerKind.Workbench ||
                !_quotes.OwnsWorkOrderPublisher(_workbenchContainerId, this) ||
                Revision < 0 ||
                _issuesByTicket.Count != count ||
                _issuesByQuote.Count != count ||
                _issuesByClaim.Count != count ||
                _issuesByOperation.Count != count ||
                _issuesByWorkbench.Count != count ||
                _allocationsByOrder.Count != count)
            {
                return OperationResult.Fail(
                    CustomPcWorkOrderFailures.InvariantViolation);
            }

            foreach (CustomPcWorkOrderIssueResult issue in _issuesByOrder.Values)
            {
                if (!OwnsIssue(issue))
                {
                    return OperationResult.Fail(
                        CustomPcWorkOrderFailures.InvariantViolation);
                }
            }

            return OperationResult.Success();
        }

        private Failure ValidateIssueInput(
            StableId<CustomPcBuildOrderIdScope> workOrderId,
            StableId<CustomPcWorkTicketIdScope> workTicketId,
            StableId<CustomPcWorkOrderOperationIdScope> operationId,
            CustomPcQuoteRecord quote,
            SimulationTimestamp issuedAt)
        {
            if (workOrderId.IsEmpty || workTicketId.IsEmpty || operationId.IsEmpty || quote == null)
            {
                return CustomPcWorkOrderFailures.InputInvalid;
            }

            if (!_quotes.TryGetOwnedReservationSet(quote, out _))
            {
                return CustomPcWorkOrderFailures.QuoteReservationDrift;
            }

            return issuedAt.IsAtOrAfter(quote.QuotedAt)
                ? Failure.None
                : CustomPcWorkOrderFailures.TimestampInvalid;
        }

        private bool OwnsIssue(CustomPcWorkOrderIssueResult issue)
        {
            if (issue?.BuildOrder == null ||
                issue.WorkTicket == null ||
                issue.BuildOrder.SourceQuote == null ||
                issue.BuildOrder.Status !=
                    CustomPcBuildOrderStatus.ReservationSetAllocated ||
                issue.WorkTicket.Status !=
                    CustomPcWorkTicketStatus.PostedAtWorkbenchStation ||
                !ReferenceEquals(issue.WorkTicket.BuildOrder, issue.BuildOrder) ||
                issue.WorkTicket.Id != issue.BuildOrder.WorkTicketId ||
                issue.BuildOrder.WorkbenchContainerId != _workbenchContainerId ||
                !issue.BuildOrder.IssuedAt.IsAtOrAfter(
                    issue.BuildOrder.SourceQuote.QuotedAt) ||
                !_quotes.TryGetOwnedReservationSet(
                    issue.BuildOrder.SourceQuote,
                    out _) ||
                !HasExactQuoteLines(issue.BuildOrder) ||
                !_issuesByOrder.TryGetValue(
                    issue.BuildOrder.Id,
                    out CustomPcWorkOrderIssueResult byOrder) ||
                !_issuesByTicket.TryGetValue(
                    issue.WorkTicket.Id,
                    out CustomPcWorkOrderIssueResult byTicket) ||
                !_issuesByQuote.TryGetValue(
                    issue.BuildOrder.SourceQuoteId,
                    out CustomPcWorkOrderIssueResult byQuote) ||
                !_issuesByClaim.TryGetValue(
                    issue.BuildOrder.InventoryClaimId,
                    out CustomPcWorkOrderIssueResult byClaim) ||
                !_issuesByOperation.TryGetValue(
                    issue.BuildOrder.OperationId,
                    out CustomPcWorkOrderIssueResult byOperation) ||
                !_issuesByWorkbench.TryGetValue(
                    issue.BuildOrder.WorkbenchContainerId,
                    out CustomPcWorkOrderIssueResult byWorkbench) ||
                !_allocationsByOrder.TryGetValue(
                    issue.BuildOrder.Id,
                    out InventorySerializedReservationWorkOrderAllocationReceipt allocation) ||
                !_inventory.OwnsSerializedReservationWorkOrderAllocation(allocation) ||
                allocation.AppliedRevision !=
                    issue.BuildOrder.InventoryAllocationRevision)
            {
                return false;
            }

            return ReferenceEquals(issue, byOrder) &&
                   ReferenceEquals(issue, byTicket) &&
                   ReferenceEquals(issue, byQuote) &&
                   ReferenceEquals(issue, byClaim) &&
                   ReferenceEquals(issue, byOperation) &&
                   ReferenceEquals(issue, byWorkbench);
        }

        private static IReadOnlyList<CustomPcBuildOrderLineSnapshot> CopyQuoteLines(
            IReadOnlyList<CustomPcQuoteLineSnapshot> quoteLines)
        {
            var exactLines = new CustomPcBuildOrderLineSnapshot[quoteLines.Count];
            for (int index = 0; index < quoteLines.Count; index++)
            {
                exactLines[index] = new CustomPcBuildOrderLineSnapshot(quoteLines[index]);
            }

            return Array.AsReadOnly(exactLines);
        }

        private static bool HasExactQuoteLines(CustomPcBuildOrderRecord order)
        {
            if (order.Lines == null ||
                order.SourceQuote.Lines == null ||
                order.Lines.Count != order.SourceQuote.Lines.Count ||
                order.Lines.Count != CustomPcQuoteAuthority.GraphicsFirstGamingLineCount)
            {
                return false;
            }

            for (int index = 0; index < order.Lines.Count; index++)
            {
                if (order.Lines[index] == null ||
                    !order.Lines[index].Matches(order.SourceQuote.Lines[index]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool MatchesIssue(
            CustomPcWorkOrderIssueResult issue,
            StableId<CustomPcBuildOrderIdScope> workOrderId,
            StableId<CustomPcWorkTicketIdScope> workTicketId,
            StableId<CustomPcWorkOrderOperationIdScope> operationId,
            CustomPcQuoteRecord quote,
            SimulationTimestamp issuedAt)
        {
            return issue?.BuildOrder != null &&
                   issue.WorkTicket != null &&
                   issue.BuildOrder.Id == workOrderId &&
                   issue.WorkTicket.Id == workTicketId &&
                   issue.BuildOrder.OperationId == operationId &&
                   ReferenceEquals(issue.BuildOrder.SourceQuote, quote) &&
                   issue.BuildOrder.IssuedAt == issuedAt;
        }

        private static StableId<InventorySerializedReservationWorkOrderOperationIdScope>
            ToInventoryOperationId(
                StableId<CustomPcWorkOrderOperationIdScope> operationId)
        {
            return StableId<InventorySerializedReservationWorkOrderOperationIdScope>.Parse(
                operationId.Value);
        }

        private static StableId<InventorySerializedReservationWorkOrderIdScope>
            ToInventoryWorkOrderId(StableId<CustomPcBuildOrderIdScope> workOrderId)
        {
            return StableId<InventorySerializedReservationWorkOrderIdScope>.Parse(
                workOrderId.Value);
        }

        private static StableId<InventorySerializedReservationWorkTicketIdScope>
            ToInventoryWorkTicketId(StableId<CustomPcWorkTicketIdScope> workTicketId)
        {
            return StableId<InventorySerializedReservationWorkTicketIdScope>.Parse(
                workTicketId.Value);
        }
    }
}
