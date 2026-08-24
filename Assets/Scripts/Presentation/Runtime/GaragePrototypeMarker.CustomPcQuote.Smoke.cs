using System.Collections;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string CustomPcQuoteSmokeSuccessMarker =
            "GARAGE_CUSTOM_PC_QUOTE_RUNTIME_SMOKE custom-pc-flow=ok " +
            "browse-route=ok focus-gate=ok consultation=ok request=accepted " +
            "bom-lines=10 compatibility=ok budget=ok reservation-set=atomic " +
            "conflict=fail-closed replay=ok authority-isolated=ok " +
            "presentation=ok invariants=ok";

        private IEnumerator RunCustomPcQuoteSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return new WaitForFixedUpdate();

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (session == null ||
                playerMotor == null ||
                customerFlow == null ||
                customerFlow.CustomerAgent == null ||
                customerFlow.CustomerVisualRoot == null ||
                customerFlow.StockFlow != stockFlow ||
                session.CustomPcQuotes == null)
            {
                LogCustomPcQuoteSmokeFailure("smoke.context-missing");
                yield break;
            }

            OperationResult acceptDelivery = session.AcceptArrivedDelivery();
            OperationResult shelfTransfer = session.TransferItem(
                session.ShelfContainerId);
            OperationResult publishOffer = session.PublishShelfOffer();
            stockFlow.RefreshPresentation();
            if (acceptDelivery.IsFailure ||
                shelfTransfer.IsFailure ||
                publishOffer.IsFailure ||
                !session.TryGetShelfOffer(out _))
            {
                string failureCode = acceptDelivery.IsFailure
                    ? acceptDelivery.Error.Code
                    : shelfTransfer.IsFailure
                        ? shelfTransfer.Error.Code
                        : publishOffer.IsFailure
                            ? publishOffer.Error.Code
                            : "smoke.storefront-prerequisite-mismatch";
                LogCustomPcQuoteSmokeFailure(failureCode);
                yield break;
            }

            const int MaximumBrowseSteps = 650;
            int browseSteps = 0;
            while (browseSteps < MaximumBrowseSteps)
            {
                CustomerVisitRecord candidate = customerFlow.CurrentVisit;
                if (candidate != null &&
                    candidate.State == CustomerVisitState.Browsing)
                {
                    break;
                }

                if (candidate != null &&
                    candidate.State == CustomerVisitState.Exited)
                {
                    LogCustomPcQuoteSmokeFailure(
                        "smoke.customer-exited-before-browse");
                    yield break;
                }

                browseSteps++;
                playerMotor.SetPaused(false);
                yield return new WaitForFixedUpdate();
            }

            CustomerVisitRecord browsingVisit = customerFlow.CurrentVisit;
            if (browsingVisit == null ||
                browsingVisit.State != CustomerVisitState.Browsing ||
                browsingVisit.TotalRouteFailureCount != 0)
            {
                LogCustomPcQuoteSmokeFailure("smoke.browse-route-mismatch");
                yield break;
            }

            MovePlayerToCustomPcCustomer();
            playerMotor.SetPaused(false);
            customerFlow.RefreshPresentation();
            if (!customerFlow.CanConsultCurrentCustomer ||
                !customerFlow.ContextualPromptText.Contains("ihtiyacını sor"))
            {
                LogCustomPcQuoteSmokeFailure("smoke.focus-gate-mismatch");
                yield break;
            }

            long inventoryRevisionBefore = session.Inventory.Revision;
            int reservationCountBefore = session.Inventory.ReservationCount;
            long orderRevisionBefore = session.Orders.Revision;
            long offerRevisionBefore = session.RetailOffers.Revision;
            long basketRevisionBefore = session.RetailBaskets.Revision;
            long checkoutRevisionBefore = session.RetailCheckouts.Revision;
            long settlementRevisionBefore =
                session.CheckoutSettlements.Revision;
            long visitRevisionBefore = session.CustomerVisits.Revision;
            long consultationRevisionBefore =
                session.CustomerConsultations.Revision;
            long customPcRevisionBefore = session.CustomPcQuotes.Revision;
            long actionRevisionBefore = session.CustomerOfferActions.Revision;
            long assemblyRevisionBefore = session.AssemblyBuild.Revision;

            OperationResult consultation =
                customerFlow.TryConsultCurrentCustomer();
            if (consultation.IsFailure ||
                !customerFlow.ConsultationCompleted ||
                customerFlow.CustomPcRequestAccepted ||
                !customerFlow.CanProgressCurrentCustomPc)
            {
                LogCustomPcQuoteSmokeFailure(
                    consultation.IsFailure
                        ? consultation.Error.Code
                        : "smoke.consultation-mismatch");
                yield break;
            }

            OperationResult acceptRequest =
                customerFlow.TryProgressCurrentCustomPc();
            if (acceptRequest.IsFailure ||
                !session.TryGetPrototypeCustomPcRequest(
                    out CustomPcRequestRecord request) ||
                !customerFlow.CustomPcRequestAccepted ||
                customerFlow.CustomPcQuoteReady ||
                customerFlow.CurrentOfferDecision != null ||
                request.Profile != CustomPcBuildProfile.GraphicsFirstGaming ||
                request.MaximumBudget.MinorUnits !=
                    GarageStockFlowSession
                        .PrototypeCustomPcMaximumBudgetMinorUnits ||
                !customerFlow.ContextualPromptText.Contains("10 parçayı ayır"))
            {
                LogCustomPcQuoteSmokeFailure(
                    acceptRequest.IsFailure
                        ? acceptRequest.Error.Code
                        : "smoke.request-mismatch");
                yield break;
            }

            OperationResult createQuote =
                customerFlow.TryProgressCurrentCustomPc();
            if (createQuote.IsFailure ||
                !session.TryGetPrototypeCustomPcQuote(
                    out CustomPcQuoteRecord quote))
            {
                LogCustomPcQuoteSmokeFailure(
                    createQuote.IsFailure
                        ? createQuote.Error.Code
                        : "smoke.quote-missing");
                yield break;
            }

            bool exactReservations =
                quote.Request == request &&
                quote.InventoryClaimId == session.PrototypeCustomPcClaimId &&
                quote.ReservedSerializedItemCount ==
                    CustomPcQuoteAuthority.GraphicsFirstGamingLineCount &&
                quote.TotalPrice.MinorUnits ==
                    GarageStockFlowSession.PrototypeCustomPcTotalPriceMinorUnits &&
                quote.TotalPrice.MinorUnits <= request.MaximumBudget.MinorUnits;
            foreach (CustomPcQuoteLineSnapshot line in quote.Lines)
            {
                exactReservations = exactReservations &&
                    session.Inventory.TryGetSerializedItem(
                        line.ItemId,
                        out InventoryItemRecord item) &&
                    item.ProductId == line.ProductId &&
                    item.UnitCost == line.UnitCost &&
                    session.Inventory.TryGetReservation(
                        line.ReservationId,
                        out InventoryReservation reservation) &&
                    reservation.ClaimId == quote.InventoryClaimId &&
                    reservation.ItemId == line.ItemId &&
                    reservation.TargetKind ==
                        InventoryReservationTargetKind.SerializedItem &&
                    reservation.Quantity == 1;
            }

            long inventoryRevisionCommitted = session.Inventory.Revision;
            long customPcRevisionCommitted = session.CustomPcQuotes.Revision;
            int reservationCountCommitted = session.Inventory.ReservationCount;
            OperationResult requestReplay =
                session.AcceptPrototypeCustomPcRequest(request.AcceptedAt);
            OperationResult quoteReplay =
                session.CreatePrototypeCustomPcQuote(quote.QuotedAt);
            bool replay = requestReplay.IsSuccess &&
                          quoteReplay.IsSuccess &&
                          session.Inventory.Revision ==
                              inventoryRevisionCommitted &&
                          session.CustomPcQuotes.Revision ==
                              customPcRevisionCommitted &&
                          session.Inventory.ReservationCount ==
                              reservationCountCommitted;

            bool authorityIsolated =
                session.Inventory.Revision == inventoryRevisionBefore + 1 &&
                session.Inventory.ReservationCount ==
                    reservationCountBefore +
                    CustomPcQuoteAuthority.GraphicsFirstGamingLineCount &&
                session.CustomerConsultations.Revision ==
                    consultationRevisionBefore + 1 &&
                session.CustomPcQuotes.Revision == customPcRevisionBefore + 2 &&
                session.Orders.Revision == orderRevisionBefore &&
                session.RetailOffers.Revision == offerRevisionBefore &&
                session.RetailBaskets.Revision == basketRevisionBefore &&
                session.RetailCheckouts.Revision == checkoutRevisionBefore &&
                session.CheckoutSettlements.Revision ==
                    settlementRevisionBefore &&
                session.CustomerVisits.Revision == visitRevisionBefore &&
                session.CustomerOfferActions.Revision ==
                    actionRevisionBefore &&
                session.AssemblyBuild.Revision == assemblyRevisionBefore &&
                session.RetailBaskets.Count == 0 &&
                session.CustomerOfferActions.Count == 0;
            bool presentation = customerFlow.CustomPcQuoteReady &&
                                customerFlow.CurrentCustomPcQuote == quote &&
                                customerFlow.ContextualPromptText.Length == 0 &&
                                customerFlow.CustomerSpeechTextValue
                                    .Contains("10 PARÇA") &&
                                customerFlow.StateText
                                    .Contains("ÖZEL PC TEKLİFİ HAZIR");
            bool conflictFailClosed =
                ValidateCustomPcAtomicConflictProbe();
            bool invariants = session.CustomPcQuotes
                                  .ValidateInvariants().IsSuccess &&
                              session.ValidateInvariants().IsSuccess;

            if (!exactReservations ||
                !replay ||
                !authorityIsolated ||
                !presentation ||
                !conflictFailClosed ||
                !invariants)
            {
                LogCustomPcQuoteSmokeFailure(
                    "smoke.final-invariant-mismatch");
                yield break;
            }

            Debug.Log(CustomPcQuoteSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToCustomPcCustomer()
        {
            CharacterController controller =
                playerMotor.GetComponent<CharacterController>();
            Vector3 target =
                customerFlow.CustomerVisualRoot.transform.position +
                (Vector3.up * 1.35f);
            Vector3 playerPosition = target - (Vector3.right * 1.55f);
            playerPosition.y = 0.05f;
            controller.enabled = false;
            playerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(Vector3.right, Vector3.up));
            Transform cameraPivot = playerMotor.transform.Find("CameraPivot");
            if (cameraPivot != null)
            {
                cameraPivot.rotation = Quaternion.LookRotation(
                    target - cameraPivot.position,
                    Vector3.up);
            }

            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static bool ValidateCustomPcAtomicConflictProbe()
        {
            GarageStockFlowSession probe =
                GarageStockFlowSession.CreateArrived(true);
            SimulationTimestamp visitAt = SimulationTimestamp.Create(10, 10_000);
            SimulationTimestamp browseAt = SimulationTimestamp.Create(11, 11_000);
            SimulationTimestamp consultAt = SimulationTimestamp.Create(12, 12_000);
            SimulationTimestamp acceptAt = SimulationTimestamp.Create(13, 13_000);
            SimulationTimestamp quoteAt = SimulationTimestamp.Create(14, 14_000);
            if (probe.StartPrototypeCustomerVisit(visitAt).IsFailure ||
                probe.MarkPrototypeCustomerBrowseArrival(browseAt).IsFailure ||
                probe.ConsultPrototypeCustomer(consultAt).IsFailure ||
                probe.AcceptPrototypeCustomPcRequest(acceptAt).IsFailure)
            {
                return false;
            }

            OperationResult externalReservation =
                probe.Inventory.ReserveSerializedItem(
                    StableId<ReservationIdScope>.Parse(
                        "inventory.reservation.runtime-smoke.custom-pc.external"),
                    StableId<InventoryClaimIdScope>.Parse(
                        "inventory.claim.runtime-smoke.custom-pc.external"),
                    probe.MotherboardItemId);
            long inventoryRevision = probe.Inventory.Revision;
            int reservationCount = probe.Inventory.ReservationCount;
            long customPcRevision = probe.CustomPcQuotes.Revision;
            OperationResult conflict =
                probe.CreatePrototypeCustomPcQuote(quoteAt);
            bool noPartialReservation = true;
            foreach (CustomPcQuoteLineDraft line in
                     probe.CreatePrototypeCustomPcQuoteLines())
            {
                noPartialReservation = noPartialReservation &&
                    !probe.Inventory.TryGetReservation(
                        line.ReservationId,
                        out _);
            }

            return externalReservation.IsSuccess &&
                   conflict.Error == InventoryFailures.ItemAlreadyReserved &&
                   probe.Inventory.Revision == inventoryRevision &&
                   probe.Inventory.ReservationCount == reservationCount &&
                   probe.CustomPcQuotes.Revision == customPcRevision &&
                   probe.CustomPcQuotes.QuoteCount == 0 &&
                   noPartialReservation &&
                   probe.ValidateInvariants().IsSuccess;
        }

        private static void LogCustomPcQuoteSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_CUSTOM_PC_QUOTE_RUNTIME_SMOKE " +
                $"custom-pc-flow=failed code={code}");
        }
    }
}
