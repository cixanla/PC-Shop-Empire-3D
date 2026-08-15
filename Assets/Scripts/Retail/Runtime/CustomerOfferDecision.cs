using System;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Catalog;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;

namespace PCShopEmpire3D.Retail
{
    public enum CustomerOfferDecisionKind
    {
        Buy = 1,
        Leave = 2
    }

    public static class CustomerOfferDecisionReasonCodes
    {
        public const string BuyExactProductWithinLimit =
            "retail.offer-decision.buy.exact-product-within-limit";
        public const string LeaveProductMismatch =
            "retail.offer-decision.leave.product-mismatch";
        public const string LeavePriceAboveLimit =
            "retail.offer-decision.leave.price-above-limit";
    }

    public static class CustomerOfferDecisionFailures
    {
        public static readonly Failure InputInvalid =
            Failure.FromCode("retail.offer-decision.input-invalid");
        public static readonly Failure VisitNotBrowsing =
            Failure.FromCode("retail.offer-decision.visit-not-browsing");
        public static readonly Failure NeedUnsupported =
            Failure.FromCode("retail.offer-decision.need-unsupported");
        public static readonly Failure CurrencyMismatch =
            Failure.FromCode("retail.offer-decision.currency-mismatch");
    }

    /// <summary>
    /// Immutable provenance for one pure customer/offer comparison. It is descriptive only:
    /// consumers must not treat this result as an inventory, basket or checkout command.
    /// </summary>
    public sealed class CustomerOfferDecision : IEquatable<CustomerOfferDecision>
    {
        internal CustomerOfferDecision(
            StableId<CustomerIdScope> customerId,
            StableId<CustomerVisitIdScope> visitId,
            StableId<CustomerIntentIdScope> intentId,
            CustomerVisitState visitState,
            SimulationTimestamp visitLastUpdatedAt,
            CustomerNeedKind need,
            StableId<ProductDefinitionIdScope> intentProductId,
            StableId<ShelfOfferIdScope> offerId,
            long offerRevision,
            StableId<ContainerIdScope> shelfContainerId,
            StableId<ProductDefinitionIdScope> offerProductId,
            ShelfPrice offerPrice,
            ShelfPrice maximumAcceptedPrice,
            CustomerOfferDecisionKind decisionKind,
            string reasonCode)
        {
            CustomerId = customerId;
            VisitId = visitId;
            IntentId = intentId;
            VisitState = visitState;
            VisitLastUpdatedAt = visitLastUpdatedAt;
            Need = need;
            IntentProductId = intentProductId;
            OfferId = offerId;
            OfferRevision = offerRevision;
            ShelfContainerId = shelfContainerId;
            OfferProductId = offerProductId;
            OfferPrice = offerPrice;
            MaximumAcceptedPrice = maximumAcceptedPrice;
            DecisionKind = decisionKind;
            ReasonCode = reasonCode;
        }

        public StableId<CustomerIdScope> CustomerId { get; }

        public StableId<CustomerVisitIdScope> VisitId { get; }

        public StableId<CustomerIntentIdScope> IntentId { get; }

        public CustomerVisitState VisitState { get; }

        public SimulationTimestamp VisitLastUpdatedAt { get; }

        public CustomerNeedKind Need { get; }

        public StableId<ProductDefinitionIdScope> IntentProductId { get; }

        public StableId<ShelfOfferIdScope> OfferId { get; }

        public long OfferRevision { get; }

        public StableId<ContainerIdScope> ShelfContainerId { get; }

        public StableId<ProductDefinitionIdScope> OfferProductId { get; }

        public ShelfPrice OfferPrice { get; }

        public ShelfPrice MaximumAcceptedPrice { get; }

        public CustomerOfferDecisionKind DecisionKind { get; }

        public string ReasonCode { get; }

        public bool Equals(CustomerOfferDecision other)
        {
            return !ReferenceEquals(other, null) &&
                   CustomerId == other.CustomerId &&
                   VisitId == other.VisitId &&
                   IntentId == other.IntentId &&
                   VisitState == other.VisitState &&
                   VisitLastUpdatedAt == other.VisitLastUpdatedAt &&
                   Need == other.Need &&
                   IntentProductId == other.IntentProductId &&
                   OfferId == other.OfferId &&
                   OfferRevision == other.OfferRevision &&
                   ShelfContainerId == other.ShelfContainerId &&
                   OfferProductId == other.OfferProductId &&
                   OfferPrice == other.OfferPrice &&
                   MaximumAcceptedPrice == other.MaximumAcceptedPrice &&
                   DecisionKind == other.DecisionKind &&
                   string.Equals(ReasonCode, other.ReasonCode, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CustomerOfferDecision);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = CustomerId.GetHashCode();
                hash = (hash * 397) ^ VisitId.GetHashCode();
                hash = (hash * 397) ^ IntentId.GetHashCode();
                hash = (hash * 397) ^ (int)VisitState;
                hash = (hash * 397) ^ VisitLastUpdatedAt.GetHashCode();
                hash = (hash * 397) ^ (int)Need;
                hash = (hash * 397) ^ IntentProductId.GetHashCode();
                hash = (hash * 397) ^ OfferId.GetHashCode();
                hash = (hash * 397) ^ OfferRevision.GetHashCode();
                hash = (hash * 397) ^ ShelfContainerId.GetHashCode();
                hash = (hash * 397) ^ OfferProductId.GetHashCode();
                hash = (hash * 397) ^ OfferPrice.GetHashCode();
                hash = (hash * 397) ^ MaximumAcceptedPrice.GetHashCode();
                hash = (hash * 397) ^ (int)DecisionKind;
                return (hash * 397) ^ StringComparer.Ordinal.GetHashCode(ReasonCode ?? string.Empty);
            }
        }
    }

    /// <summary>
    /// Stateless single-offer decision rule. A valid Leave is a successful customer outcome;
    /// failures are reserved for invalid or incomparable snapshots.
    /// </summary>
    public static class CustomerOfferDecisionEvaluator
    {
        public static OperationResult<CustomerOfferDecision> Evaluate(
            CustomerVisitRecord visit,
            ShelfOfferRecord offer,
            ShelfPrice maximumAcceptedPrice)
        {
            if (!HasValidStructure(visit, offer, maximumAcceptedPrice))
            {
                return OperationResult<CustomerOfferDecision>.Fail(
                    CustomerOfferDecisionFailures.InputInvalid);
            }

            if (visit.State != CustomerVisitState.Browsing)
            {
                return OperationResult<CustomerOfferDecision>.Fail(
                    CustomerOfferDecisionFailures.VisitNotBrowsing);
            }

            if (visit.Intent.Need != CustomerNeedKind.GraphicsUpgrade)
            {
                return OperationResult<CustomerOfferDecision>.Fail(
                    CustomerOfferDecisionFailures.NeedUnsupported);
            }

            if (offer.Price.Currency != maximumAcceptedPrice.Currency)
            {
                return OperationResult<CustomerOfferDecision>.Fail(
                    CustomerOfferDecisionFailures.CurrencyMismatch);
            }

            CustomerOfferDecisionKind kind;
            string reasonCode;
            if (visit.Intent.ProductId != offer.ProductId)
            {
                kind = CustomerOfferDecisionKind.Leave;
                reasonCode = CustomerOfferDecisionReasonCodes.LeaveProductMismatch;
            }
            else if (offer.Price.MinorUnits > maximumAcceptedPrice.MinorUnits)
            {
                kind = CustomerOfferDecisionKind.Leave;
                reasonCode = CustomerOfferDecisionReasonCodes.LeavePriceAboveLimit;
            }
            else
            {
                kind = CustomerOfferDecisionKind.Buy;
                reasonCode = CustomerOfferDecisionReasonCodes.BuyExactProductWithinLimit;
            }

            return OperationResult<CustomerOfferDecision>.Success(
                new CustomerOfferDecision(
                    visit.Intent.CustomerId,
                    visit.Id,
                    visit.Intent.Id,
                    visit.State,
                    visit.LastUpdatedAt,
                    visit.Intent.Need,
                    visit.Intent.ProductId,
                    offer.Id,
                    offer.OfferRevision,
                    offer.ShelfContainerId,
                    offer.ProductId,
                    offer.Price,
                    maximumAcceptedPrice,
                    kind,
                    reasonCode));
        }

        private static bool HasValidStructure(
            CustomerVisitRecord visit,
            ShelfOfferRecord offer,
            ShelfPrice maximumAcceptedPrice)
        {
            if (visit == null ||
                visit.Intent == null ||
                visit.Id.IsEmpty ||
                visit.Intent.Id.IsEmpty ||
                visit.Intent.CustomerId.IsEmpty ||
                visit.Intent.ProductId.IsEmpty ||
                offer == null ||
                offer.Id.IsEmpty ||
                offer.ProductId.IsEmpty ||
                offer.ShelfContainerId.IsEmpty ||
                offer.OfferRevision <= 0)
            {
                return false;
            }

            return ShelfPrice.Create(
                       offer.Price.Currency.Value,
                       offer.Price.MinorUnits).IsSuccess &&
                   ShelfPrice.Create(
                       maximumAcceptedPrice.Currency.Value,
                       maximumAcceptedPrice.MinorUnits).IsSuccess;
        }
    }
}
