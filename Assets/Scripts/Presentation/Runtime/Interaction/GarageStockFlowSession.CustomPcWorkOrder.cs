using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Retail;

namespace PCShopEmpire3D.Presentation.Interaction
{
    public sealed partial class GarageStockFlowSession
    {
        public const string PrototypeCustomPcBuildOrderIdValue =
            "orders.custom-pc-build-order.demo-gaming-001";
        public const string PrototypeCustomPcWorkTicketIdValue =
            "orders.custom-pc-work-ticket.demo-gaming-001";
        public const string PrototypeCustomPcWorkOrderOperationIdValue =
            "orders.custom-pc-build-order.demo-gaming-001.issue";

        public StableId<CustomPcBuildOrderIdScope> PrototypeCustomPcBuildOrderId =>
            StableId<CustomPcBuildOrderIdScope>.Parse(
                PrototypeCustomPcBuildOrderIdValue);

        public StableId<CustomPcWorkTicketIdScope> PrototypeCustomPcWorkTicketId =>
            StableId<CustomPcWorkTicketIdScope>.Parse(
                PrototypeCustomPcWorkTicketIdValue);

        public StableId<CustomPcWorkOrderOperationIdScope>
            PrototypeCustomPcWorkOrderOperationId =>
                StableId<CustomPcWorkOrderOperationIdScope>.Parse(
                    PrototypeCustomPcWorkOrderOperationIdValue);

        internal OperationResult RegisterCanonicalCustomPcWorkTicketStation(
            CustomPcWorkTicketStationProjection station)
        {
            if (station == null ||
                !string.Equals(
                    station.StationIdValue,
                    CustomPcWorkTicketStationProjection.PrototypeStationIdValue,
                    System.StringComparison.Ordinal))
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures.CanonicalStationMismatch);
            }

            if (_canonicalCustomPcWorkTicketStation != null &&
                !ReferenceEquals(_canonicalCustomPcWorkTicketStation, station))
            {
                return OperationResult.Fail(
                    CustomPcWorkTicketStationFailures
                        .CanonicalStationRegistrationConflict);
            }

            _canonicalCustomPcWorkTicketStation = station;
            return OperationResult.Success();
        }

        internal OperationResult<CustomPcWorkOrderIssueResult>
            IssueFromPhysicalWorkTicket(
                CustomPcWorkTicketStationProjection station,
                SimulationTimestamp issuedAt)
        {
            if (station == null ||
                !ReferenceEquals(station, _canonicalCustomPcWorkTicketStation) ||
                !string.Equals(
                    station.StationIdValue,
                    CustomPcWorkTicketStationProjection.PrototypeStationIdValue,
                    System.StringComparison.Ordinal))
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                    CustomPcWorkTicketStationFailures.CanonicalStationMismatch);
            }

            if (
                !station.TryConsumePhysicalIssueAuthorization(
                    this,
                    UnityEngine.Time.frameCount))
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                    CustomPcWorkOrderFailures.IssueAccessInvalid);
            }

            if (!TryGetPrototypeCustomPcQuote(out CustomPcQuoteRecord quote))
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                    CustomPcWorkOrderFailures.QuoteNotOwned);
            }

            return CustomPcWorkOrders.Issue(
                _customPcWorkOrderIssueAccess,
                PrototypeCustomPcBuildOrderId,
                PrototypeCustomPcWorkTicketId,
                PrototypeCustomPcWorkOrderOperationId,
                quote,
                issuedAt);
        }

        internal OperationResult<CustomPcWorkOrderIssueResult>
            ReplayIssuedPrototypeCustomPcWorkOrderForVerification(
                SimulationTimestamp issuedAt)
        {
            if (!TryGetPrototypeCustomPcBuildOrder(out _) ||
                !TryGetPrototypeCustomPcWorkTicket(out _) ||
                !TryGetPrototypeCustomPcQuote(out CustomPcQuoteRecord quote))
            {
                return OperationResult<CustomPcWorkOrderIssueResult>.Fail(
                    CustomPcWorkOrderFailures.QuoteNotOwned);
            }

            return CustomPcWorkOrders.Issue(
                _customPcWorkOrderIssueAccess,
                PrototypeCustomPcBuildOrderId,
                PrototypeCustomPcWorkTicketId,
                PrototypeCustomPcWorkOrderOperationId,
                quote,
                issuedAt);
        }

        public bool TryGetPrototypeCustomPcBuildOrder(
            out CustomPcBuildOrderRecord buildOrder)
        {
            return CustomPcWorkOrders.TryGetWorkOrder(
                PrototypeCustomPcBuildOrderId,
                out buildOrder);
        }

        public bool TryGetPrototypeCustomPcWorkTicket(
            out CustomPcWorkTicketRecord workTicket)
        {
            return CustomPcWorkOrders.TryGetWorkTicket(
                PrototypeCustomPcWorkTicketId,
                out workTicket);
        }
    }
}
