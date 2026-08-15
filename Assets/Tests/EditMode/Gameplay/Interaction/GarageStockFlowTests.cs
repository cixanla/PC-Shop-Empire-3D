using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Economy;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Orders;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Tests.EditMode.Gameplay.Interaction
{
    public sealed class GarageStockFlowTests
    {
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null)
            {
                Object.DestroyImmediate(_root);
            }

            _root = null;
        }

        [Test]
        public void ArrivedOrderHasVisibleManifestIdentityButNoAuthoritativeStock()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();

            Assert.That(session.Order.Status, Is.EqualTo(PurchaseOrderStatus.Arrived));
            Assert.That(session.Order.Manifest.Intake.SerializedItems.Count, Is.EqualTo(1));
            Assert.That(
                session.Order.Manifest.Intake.SerializedItems[0].ItemId,
                Is.EqualTo(session.ItemId));
            Assert.That(session.TryGetItem(out _), Is.False);
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void AcceptanceAndContainerTransfersPreserveOneStableSerializedItem()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();

            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            AssertLocation(session, session.ReceivingContainerId);
            Assert.That(session.Order.Status, Is.EqualTo(PurchaseOrderStatus.Accepted));
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.EqualTo(1));

            Assert.That(session.TransferItem(session.HandsContainerId).IsSuccess, Is.True);
            AssertLocation(session, session.HandsContainerId);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            AssertLocation(session, session.ShelfContainerId);
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void CustomerVisitLifecycleCannotMutateStockOrderOrRetailAuthorities()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();
            long inventoryRevision = session.Inventory.Revision;
            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;

            Assert.That(session.StartPrototypeCustomerVisit(Timestamp(10)).IsSuccess, Is.True);
            Assert.That(session.MarkPrototypeCustomerBrowseArrival(Timestamp(11)).IsSuccess, Is.True);
            Assert.That(
                session.BeginPrototypeCustomerCheckoutNavigation(Timestamp(12)).IsSuccess,
                Is.True);
            Assert.That(session.MarkPrototypeCustomerCheckoutArrival(Timestamp(13)).IsSuccess, Is.True);
            Assert.That(session.BeginPrototypeCustomerExit(
                CustomerVisitExitReason.Fulfilled,
                Timestamp(14)).IsSuccess, Is.True);
            Assert.That(session.MarkPrototypeCustomerExitArrival(Timestamp(15)).IsSuccess, Is.True);

            Assert.That(session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit), Is.True);
            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Exited));
            Assert.That(visit.ExitReason, Is.EqualTo(CustomerVisitExitReason.Fulfilled));
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(6));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(session.RetailCheckouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(session.Order.Status, Is.EqualTo(PurchaseOrderStatus.Arrived));
            Assert.That(session.TryGetItem(out _), Is.False);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PrototypeConsultationGatesDecisionAndOnlyAdvancesConsultationAuthority()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            Assert.That(session.StartPrototypeCustomerVisit(Timestamp(10)).IsSuccess, Is.True);
            Assert.That(session.MarkPrototypeCustomerBrowseArrival(Timestamp(11)).IsSuccess,
                Is.True);

            long visitRevision = session.CustomerVisits.Revision;
            long inventoryRevision = session.Inventory.Revision;
            long orderRevision = session.Orders.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long checkoutRevision = session.RetailCheckouts.Revision;
            long economyRevision = session.CheckoutSettlements.Revision;

            OperationResult<CustomerOfferDecision> gated =
                session.EvaluatePrototypeCustomerOffer();
            Assert.That(gated.Error,
                Is.EqualTo(CustomerOfferDecisionFailures.ConsultationRequired));
            Assert.That(session.CustomerConsultations.Revision, Is.Zero);

            OperationResult consultation = session.ConsultPrototypeCustomer(Timestamp(12));
            OperationResult replay = session.ConsultPrototypeCustomer(Timestamp(12));
            Assert.That(consultation.IsSuccess, Is.True);
            Assert.That(replay.IsSuccess, Is.True);
            Assert.That(session.CustomerConsultations.Revision, Is.EqualTo(1));
            Assert.That(session.TryGetPrototypeCustomerConsultation(
                out CustomerConsultationRecord record), Is.True);
            Assert.That(record.Id, Is.EqualTo(session.PrototypeCustomerConsultationId));
            Assert.That(record.VisitId, Is.EqualTo(session.PrototypeCustomerVisitId));
            Assert.That(record.Need, Is.EqualTo(CustomerNeedKind.GraphicsUpgrade));
            Assert.That(record.ProductId, Is.EqualTo(session.ProductId));

            OperationResult<CustomerOfferDecision> decision =
                session.EvaluatePrototypeCustomerOffer();
            Assert.That(decision.IsSuccess, Is.True);
            Assert.That(decision.Value.DecisionKind,
                Is.EqualTo(CustomerOfferDecisionKind.Buy));
            Assert.That(decision.Value.Consultation, Is.EqualTo(record));
            Assert.That(session.ConsultPrototypeCustomer(Timestamp(13)).Error,
                Is.EqualTo(CustomerConsultationFailures.IdentityConflict));

            Assert.That(session.CustomerConsultations.Revision, Is.EqualTo(1));
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(visitRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(session.RetailCheckouts.Revision, Is.EqualTo(checkoutRevision));
            Assert.That(session.CheckoutSettlements.Revision, Is.EqualTo(economyRevision));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BindingMovesAuthorityBeforeProjectionAndRecoveryReturnsBothToSafeSource()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.RequiresAcceptance, Is.True);
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryPreparePickupTransfer().Error,
                Is.EqualTo(StockProjectionFailures.ParcelSealed));
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            Vector3 safePosition = fixture.Item.LastSafePosition;

            Assert.That(fixture.Binding.TryPreparePickupTransfer().IsSuccess, Is.True);
            AssertLocation(fixture.Session, fixture.Session.HandsContainerId);
            Assert.That(fixture.Item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            OperationResult beginCarry = fixture.Item.BeginCarry(fixture.Anchor, 8);
            Assert.That(beginCarry.IsSuccess, Is.True, beginCarry.IsFailure ? beginCarry.Error.Code : string.Empty);
            Assert.That(fixture.Binding.CommitPreparedTransfer(targetIsWorld: false).IsSuccess, Is.True);

            Assert.That(fixture.Binding.TryPrepareRecoveryTransfer().IsSuccess, Is.True);
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
            Assert.That(fixture.Item.RecoverToLastSafePose().IsSuccess, Is.True);
            Assert.That(fixture.Binding.CommitPreparedTransfer(targetIsWorld: true).IsSuccess, Is.True);

            Assert.That(fixture.Item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            Assert.That(Vector3.Distance(fixture.Item.transform.position, safePosition), Is.LessThan(0.001f));
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
        }

        [Test]
        public void FullShelfRejectsPreparedPlacementWithoutMovingHeldProjectionOrInventory()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            FillShelf(fixture.Session);
            Assert.That(fixture.Binding.TryPreparePickupTransfer().IsSuccess, Is.True);
            OperationResult beginCarry = fixture.Item.BeginCarry(fixture.Anchor, 8);
            Assert.That(beginCarry.IsSuccess, Is.True, beginCarry.IsFailure ? beginCarry.Error.Code : string.Empty);
            Assert.That(fixture.Binding.CommitPreparedTransfer(targetIsWorld: false).IsSuccess, Is.True);
            Vector3 heldPosition = fixture.Item.transform.position;

            GameObject surfaceObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surfaceObject.name = "FullShelfSurface";
            surfaceObject.transform.SetParent(_root.transform);
            PlacementSurface surface = surfaceObject.AddComponent<PlacementSurface>();
            surface.Configure("tests.full-shelf", surfaceObject.GetComponent<Collider>(), 0.25f, 90f);
            InventoryPlacementZone zone = surfaceObject.AddComponent<InventoryPlacementZone>();
            zone.Configure(
                GarageStockFlowSession.ShelfContainerIdValue,
                InventoryContainerKind.Shelf,
                "Full Shelf",
                surface);

            OperationResult result = fixture.Binding.TryPreparePlacementTransfer(surface);

            Assert.That(result.Error, Is.EqualTo(InventoryFailures.ContainerCapacityExceeded));
            Assert.That(fixture.Binding.HasPreparedTransfer, Is.False);
            Assert.That(fixture.Item.IsCarried, Is.True);
            Assert.That(Vector3.Distance(fixture.Item.transform.position, heldPosition), Is.LessThan(0.001f));
            AssertLocation(fixture.Session, fixture.Session.HandsContainerId);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void AcceptedParcelOpensExactlyOnceWithoutMutatingOrderOrInventory()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Binding.RequiresUnpacking, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);

            Assert.That(fixture.Parcel.IsOpened, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.EqualTo(1));
            Assert.That(fixture.Parcel.SealedVisualRoot.activeSelf, Is.False);
            Assert.That(fixture.Parcel.ProductVisualRoot.activeSelf, Is.True);
            Assert.That(fixture.Parcel.OpenedShellVisualRoot.activeSelf, Is.True);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Session.Inventory.GetTotalQuantity(fixture.Session.ProductId).Value,
                Is.EqualTo(1));
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
        }

        [Test]
        public void ParcelCannotOpenBeforeAcceptanceAndLeavesEveryStateUntouched()
        {
            Fixture fixture = CreateBindingFixture();
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            OperationResult result = fixture.Binding.TryOpenParcel();

            Assert.That(result.Error, Is.EqualTo(StockProjectionFailures.ParcelNotAccepted));
            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.Zero);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Session.TryGetItem(out _), Is.False);
            Assert.That(fixture.Session.Inventory.GetTotalQuantity(fixture.Session.ProductId).Value,
                Is.Zero);
        }

        [Test]
        public void IdentityMismatchKeepsAcceptedParcelSealedAndInventoryInReceiving()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;
            fixture.Binding.Configure(
                fixture.Binding.Runtime,
                fixture.Item,
                "inventory.item.wrong-projection-001");

            OperationResult result = fixture.Binding.TryOpenParcel();

            Assert.That(result.Error, Is.EqualTo(StockProjectionFailures.IdentityMismatch));
            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.Zero);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            AssertLocation(fixture.Session, fixture.Session.ReceivingContainerId);
        }

        [Test]
        public void AcceptedItemOutsideReceivingCannotRevealSealedParcel()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Session.TransferItem(fixture.Session.HandsContainerId).IsSuccess, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            OperationResult result = fixture.Binding.TryOpenParcel();

            Assert.That(result.Error, Is.EqualTo(StockProjectionFailures.ParcelLocationMismatch));
            Assert.That(fixture.Parcel.IsSealed, Is.True);
            Assert.That(fixture.Parcel.OpenTransitionCount, Is.Zero);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            AssertLocation(fixture.Session, fixture.Session.HandsContainerId);
        }

        [Test]
        public void ShelfOfferPublishesOnceOnlyForExactShelvedItemWithoutStockOrOrderMutation()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);

            long inventoryBeforeRejected = fixture.Session.Inventory.Revision;
            long ordersBeforeRejected = fixture.Session.Orders.Revision;
            OperationResult rejected = fixture.Binding.TryPublishShelfOffer();
            Assert.That(rejected.Error,
                Is.EqualTo(StockProjectionFailures.ShelfOfferLocationMismatch));
            Assert.That(fixture.Session.RetailOffers.Revision, Is.Zero);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryBeforeRejected));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(ordersBeforeRejected));

            Assert.That(fixture.Session.TransferItem(fixture.Session.ShelfContainerId).IsSuccess,
                Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            Assert.That(fixture.Binding.RequiresShelfOffer, Is.True);
            Assert.That(fixture.Binding.TryPublishShelfOffer().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryPublishShelfOffer().IsSuccess, Is.True);

            Assert.That(fixture.Binding.RequiresShelfOffer, Is.False);
            Assert.That(fixture.Session.TryGetShelfOffer(out var offer), Is.True);
            Assert.That(offer.Id, Is.EqualTo(fixture.Session.ShelfOfferId));
            Assert.That(offer.Price.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(fixture.Session.RetailOffers.Revision, Is.EqualTo(1));
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PrototypeCustomerReservationIsIdempotentAndReleaseRestoresAvailability()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);

            long inventoryBeforeMissingOffer = session.Inventory.Revision;
            OperationResult missingOffer = session.ReservePrototypeCustomerBasket();
            Assert.That(missingOffer.Error,
                Is.EqualTo(PCShopEmpire3D.Retail.RetailBasketFailures.UnknownOffer));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryBeforeMissingOffer));
            Assert.That(session.RetailBaskets.Revision, Is.Zero);

            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long orderRevision = session.Orders.Revision;

            Assert.That(session.ReservePrototypeCustomerBasket().IsSuccess, Is.True);
            Assert.That(session.ReservePrototypeCustomerBasket().IsSuccess, Is.True);
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision + 1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(session.Inventory.GetAvailableQuantity(session.ProductId).Value, Is.Zero);
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value, Is.EqualTo(1));
            Assert.That(session.TryGetPrototypeBasketLine(out var line), Is.True);
            Assert.That(line.CustomerId, Is.EqualTo(session.PrototypeCustomerId));
            Assert.That(line.BasketId, Is.EqualTo(session.PrototypeBasketId));
            Assert.That(line.ItemId, Is.EqualTo(session.ItemId));

            inventoryRevision = session.Inventory.Revision;
            basketRevision = session.RetailBaskets.Revision;
            Assert.That(session.ReleasePrototypeCustomerBasket().IsSuccess, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision + 1));
            Assert.That(session.Inventory.GetAvailableQuantity(session.ProductId).Value,
                Is.EqualTo(1));
            Assert.That(session.Inventory.GetTotalQuantity(session.ProductId).Value,
                Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void ReservedShelfItemCannotMoveToHandsUntilBindingReleasesReservation()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);

            OperationResult wrongLocation = fixture.Binding.TryReserveForCustomer();
            Assert.That(wrongLocation.Error,
                Is.EqualTo(StockProjectionFailures.CustomerReservationLocationMismatch));
            Assert.That(fixture.Session.RetailBaskets.Count, Is.Zero);

            Assert.That(fixture.Session.TransferItem(fixture.Session.ShelfContainerId).IsSuccess,
                Is.True);
            OperationResult missingOffer = fixture.Binding.TryReserveForCustomer();
            Assert.That(missingOffer.Error, Is.EqualTo(StockProjectionFailures.ShelfOfferRequired));
            Assert.That(fixture.Binding.TryPublishShelfOffer().IsSuccess, Is.True);
            Assert.That(fixture.Session.ReservePrototypeCustomerBasket().IsSuccess, Is.True);
            Assert.That(fixture.Binding.IsCustomerReserved, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long basketRevision = fixture.Session.RetailBaskets.Revision;

            OperationResult blockedPickup = fixture.Binding.TryPreparePickupTransfer();

            Assert.That(blockedPickup.Error, Is.EqualTo(StockProjectionFailures.CustomerReserved));
            Assert.That(fixture.Binding.HasPreparedTransfer, Is.False);
            Assert.That(fixture.Item.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            AssertLocation(fixture.Session, fixture.Session.ShelfContainerId);
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));

            Assert.That(fixture.Binding.TryReleaseCustomerReservation().IsSuccess, Is.True);
            Assert.That(fixture.Binding.IsCustomerReserved, Is.False);
            Assert.That(fixture.Binding.TryPreparePickupTransfer().IsSuccess, Is.True);
            AssertLocation(fixture.Session, fixture.Session.HandsContainerId);
            Assert.That(fixture.Binding.RollbackPreparedTransfer().IsSuccess, Is.True);
            AssertLocation(fixture.Session, fixture.Session.ShelfContainerId);
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void PrototypeCheckoutFreezesPriceWithoutMutatingStockBasketOfferOrOrder()
        {
            GarageStockFlowSession session = GarageStockFlowSession.CreateArrived();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            Assert.That(session.ReservePrototypeCustomerBasket().IsSuccess, Is.True);
            long inventoryRevision = session.Inventory.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            long offerRevision = session.RetailOffers.Revision;
            long orderRevision = session.Orders.Revision;

            Assert.That(session.BeginPrototypeCheckout().IsSuccess, Is.True);
            Assert.That(session.BeginPrototypeCheckout().IsSuccess, Is.True);

            Assert.That(session.RetailCheckouts.Revision, Is.EqualTo(1));
            Assert.That(session.RetailCheckouts.Count, Is.EqualTo(1));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(session.TryGetPrototypeCheckout(out RetailCheckoutRecord checkout), Is.True);
            Assert.That(checkout.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(checkout.Lines.Count, Is.EqualTo(1));
            Assert.That(checkout.Lines[0].ItemId, Is.EqualTo(session.ItemId));
            Assert.That(checkout.Lines[0].UnitPrice.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(checkout.Lines[0].UnitCost.CurrencyCode,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(checkout.Lines[0].UnitCost.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(checkout.Lines[0].SourceOfferRevision, Is.EqualTo(1));

            Assert.That(session.RetailOffers.SetOffer(
                session.ShelfOfferId,
                session.ProductId,
                session.ShelfContainerId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                59_999).IsSuccess, Is.True);
            Assert.That(session.BeginPrototypeCheckout().IsSuccess, Is.True);
            Assert.That(session.TryGetPrototypeCheckout(out checkout), Is.True);
            Assert.That(checkout.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(checkout.Lines[0].UnitPrice.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(session.RetailCheckouts.Revision, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BindingStartsCheckoutOnceAndLocksReservationAndPickup()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            Assert.That(fixture.Session.TransferItem(fixture.Session.ShelfContainerId).IsSuccess,
                Is.True);
            Assert.That(fixture.Binding.TryPublishShelfOffer().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryBeginCheckout().Error,
                Is.EqualTo(StockProjectionFailures.CustomerReservationMissing));
            Assert.That(fixture.Session.ReservePrototypeCustomerBasket().IsSuccess, Is.True);
            Assert.That(fixture.Binding.RequiresCheckoutStart, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long basketRevision = fixture.Session.RetailBaskets.Revision;
            long offerRevision = fixture.Session.RetailOffers.Revision;
            long orderRevision = fixture.Session.Orders.Revision;

            Assert.That(fixture.Binding.TryBeginCheckout().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryBeginCheckout().IsSuccess, Is.True);

            Assert.That(fixture.Binding.IsCheckoutStarted, Is.True);
            Assert.That(fixture.Binding.RequiresCheckoutStart, Is.False);
            Assert.That(fixture.Session.RetailCheckouts.Revision, Is.EqualTo(1));
            Assert.That(fixture.Binding.TryReleaseCustomerReservation().Error,
                Is.EqualTo(StockProjectionFailures.CheckoutActive));
            Assert.That(fixture.Session.ReleasePrototypeCustomerBasket().Error,
                Is.EqualTo(StockProjectionFailures.CheckoutActive));
            Assert.That(fixture.Binding.TryPreparePickupTransfer().Error,
                Is.EqualTo(StockProjectionFailures.CustomerReserved));
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(fixture.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        [Test]
        public void BindingSettlesExactCashPostsBalancedLedgerAndReplaysWithoutMutation()
        {
            Fixture fixture = CreateBindingFixture();
            Assert.That(fixture.Binding.TryAcceptDelivery().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryOpenParcel().IsSuccess, Is.True);
            Assert.That(fixture.Session.TransferItem(
                fixture.Session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryPublishShelfOffer().IsSuccess, Is.True);
            Assert.That(fixture.Session.ReservePrototypeCustomerBasket().IsSuccess, Is.True);
            Assert.That(fixture.Binding.TryBeginCheckout().IsSuccess, Is.True);
            Assert.That(fixture.Binding.RequiresCheckoutCompletion, Is.True);
            long inventoryRevision = fixture.Session.Inventory.Revision;
            long basketRevision = fixture.Session.RetailBaskets.Revision;
            long checkoutRevision = fixture.Session.RetailCheckouts.Revision;
            long offerRevision = fixture.Session.RetailOffers.Revision;
            long orderRevision = fixture.Session.Orders.Revision;
            long economyRevision = fixture.Session.CheckoutSettlements.Revision;

            OperationResult result = fixture.Binding.TrySettleCashCheckout();

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(fixture.Binding.IsCheckoutCompleted, Is.True);
            Assert.That(fixture.Binding.IsCheckoutSettled, Is.True);
            Assert.That(fixture.Binding.IsCustomerReserved, Is.False);
            Assert.That(fixture.Binding.RequiresCheckoutCompletion, Is.False);
            Assert.That(fixture.Item.gameObject.activeSelf, Is.False);
            Assert.That(fixture.Session.TryGetItem(out _), Is.False);
            Assert.That(fixture.Session.Inventory.SerializedItemCount, Is.Zero);
            Assert.That(fixture.Session.Inventory.ReservationCount, Is.Zero);
            Assert.That(fixture.Session.RetailBaskets.Count, Is.Zero);
            Assert.That(fixture.Session.Inventory.GetTotalQuantity(
                fixture.Session.ProductId).Value, Is.Zero);
            Assert.That(fixture.Session.Inventory.GetAvailableQuantity(
                fixture.Session.ProductId).Value, Is.Zero);
            Assert.That(fixture.Session.RetailCheckouts.CompletionCount, Is.EqualTo(1));
            Assert.That(fixture.Session.TryGetPrototypeCheckoutCompletion(
                out RetailCheckoutCompletionRecord completion), Is.True);
            Assert.That(completion.Id,
                Is.EqualTo(fixture.Session.PrototypeCheckoutCompletionId));
            Assert.That(completion.TotalMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(fixture.Session.TryGetPrototypeCheckoutSettlement(
                out CheckoutSettlementReceipt receipt), Is.True);
            Assert.That(receipt.Id,
                Is.EqualTo(fixture.Session.PrototypeCheckoutSettlementId));
            Assert.That(receipt.TransactionId,
                Is.EqualTo(fixture.Session.PrototypeLedgerTransactionId));
            Assert.That(receipt.CompletionId, Is.EqualTo(completion.Id));
            Assert.That(receipt.CheckoutId, Is.EqualTo(completion.CheckoutId));
            Assert.That(receipt.CustomerId, Is.EqualTo(completion.CustomerId));
            Assert.That(receipt.PaymentMethod, Is.EqualTo(CheckoutPaymentMethod.Cash));
            Assert.That(receipt.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(receipt.GrossMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(receipt.CostOfGoodsSoldMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(receipt.GrossMarginMinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits -
                           GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(fixture.Session.TryGetPrototypeLedgerTransaction(
                out EconomyLedgerTransactionRecord transaction), Is.True);
            Assert.That(transaction.Id,
                Is.EqualTo(fixture.Session.PrototypeLedgerTransactionId));
            Assert.That(transaction.SettlementId, Is.EqualTo(receipt.Id));
            Assert.That(transaction.Entries.Count, Is.EqualTo(4));
            AssertLedgerEntry(
                transaction.Entries[0],
                EconomyAccountKind.Cash,
                EconomyEntryDirection.Debit,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertLedgerEntry(
                transaction.Entries[1],
                EconomyAccountKind.SalesRevenue,
                EconomyEntryDirection.Credit,
                GarageStockFlowSession.PrototypePriceMinorUnits);
            AssertLedgerEntry(
                transaction.Entries[2],
                EconomyAccountKind.CostOfGoodsSold,
                EconomyEntryDirection.Debit,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits);
            AssertLedgerEntry(
                transaction.Entries[3],
                EconomyAccountKind.InventoryAsset,
                EconomyEntryDirection.Credit,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits);
            Assert.That(
                transaction.Entries[0].MinorUnits + transaction.Entries[2].MinorUnits,
                Is.EqualTo(
                    transaction.Entries[1].MinorUnits + transaction.Entries[3].MinorUnits));
            CurrencyCode currency = CurrencyCode.Create(
                GarageStockFlowSession.PrototypeCurrencyCode).Value;
            Assert.That(fixture.Session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.Cash, currency).Value,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(fixture.Session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.SalesRevenue, currency).Value,
                Is.EqualTo(GarageStockFlowSession.PrototypePriceMinorUnits));
            Assert.That(fixture.Session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.CostOfGoodsSold, currency).Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(fixture.Session.CheckoutSettlements.GetAccountDelta(
                EconomyAccountKind.InventoryAsset, currency).Value,
                Is.EqualTo(-GarageStockFlowSession.PrototypeUnitCostMinorUnits));
            Assert.That(fixture.Session.Inventory.Revision,
                Is.EqualTo(inventoryRevision + 1));
            Assert.That(fixture.Session.RetailBaskets.Revision,
                Is.EqualTo(basketRevision + 1));
            Assert.That(fixture.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevision + 1));
            Assert.That(fixture.Session.CheckoutSettlements.Revision,
                Is.EqualTo(economyRevision + 1));
            Assert.That(fixture.Session.CheckoutSettlements.SettlementCount, Is.EqualTo(1));
            Assert.That(fixture.Session.CheckoutSettlements.TransactionCount, Is.EqualTo(1));
            Assert.That(fixture.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Binding.LocationLabel,
                Is.EqualTo("MÜŞTERİYE TESLİM EDİLDİ • STOK 0"));
            Assert.That(fixture.Binding.Runtime.CustomerBasketStatusText,
                Is.EqualTo("TESLİM EDİLDİ"));
            Assert.That(fixture.Binding.Runtime.CheckoutStatusText,
                Is.EqualTo($"{GarageStockFlowRuntime.PrototypePriceText} • NAKİT ALINDI"));
            Assert.That(fixture.Binding.Runtime.ShelfOfferLabelText,
                Does.Contain("NAKİT ALINDI"));
            Assert.That(fixture.Binding.Runtime.EconomyStatusText,
                Does.Contain("NAKİT +"));
            Assert.That(fixture.Binding.Runtime.EconomyStatusText,
                Does.Contain("GELİR +"));
            Assert.That(fixture.Binding.Runtime.EconomyStatusText,
                Does.Contain("COGS"));
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);

            inventoryRevision = fixture.Session.Inventory.Revision;
            basketRevision = fixture.Session.RetailBaskets.Revision;
            checkoutRevision = fixture.Session.RetailCheckouts.Revision;
            offerRevision = fixture.Session.RetailOffers.Revision;
            orderRevision = fixture.Session.Orders.Revision;
            economyRevision = fixture.Session.CheckoutSettlements.Revision;
            Assert.That(fixture.Binding.TrySettleCashCheckout().IsSuccess, Is.True);
            OperationResult conflict = fixture.Session.CheckoutSettlements.SettleCashCheckout(
                fixture.Session.PrototypeCheckoutSettlementId,
                StableId<EconomyLedgerTransactionIdScope>.Parse(
                    "economy.ledger-transaction.editmode-conflict"),
                fixture.Session.PrototypeCheckoutCompletionId,
                fixture.Session.PrototypeCheckoutId,
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypePriceMinorUnits,
                receipt.PaidAt);
            Assert.That(conflict.Error,
                Is.EqualTo(CheckoutSettlementFailures.SettlementIdentityConflict));
            Assert.That(fixture.Session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(fixture.Session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            Assert.That(fixture.Session.RetailCheckouts.Revision,
                Is.EqualTo(checkoutRevision));
            Assert.That(fixture.Session.RetailOffers.Revision, Is.EqualTo(offerRevision));
            Assert.That(fixture.Session.Orders.Revision, Is.EqualTo(orderRevision));
            Assert.That(fixture.Session.CheckoutSettlements.Revision, Is.EqualTo(economyRevision));
            Assert.That(fixture.Session.RetailCheckouts.CompletionCount, Is.EqualTo(1));
            Assert.That(fixture.Session.CheckoutSettlements.SettlementCount, Is.EqualTo(1));
            Assert.That(fixture.Session.CheckoutSettlements.TransactionCount, Is.EqualTo(1));
            Assert.That(fixture.Session.ValidateInvariants().IsSuccess, Is.True);
        }

        private Fixture CreateBindingFixture()
        {
            _root = new GameObject("StockFlowTestRoot");
            Transform world = new GameObject("World").transform;
            world.SetParent(_root.transform);
            Transform anchor = new GameObject("CarryAnchor").transform;
            anchor.SetParent(_root.transform);

            GameObject itemObject = new GameObject("BoundDeliveryItem");
            itemObject.name = "BoundDeliveryItem";
            itemObject.transform.SetParent(world);
            itemObject.transform.position = Vector3.up;
            GameObject sealedVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sealedVisual.name = "SealedParcelVisual";
            sealedVisual.transform.SetParent(itemObject.transform, false);
            sealedVisual.transform.localScale = Vector3.one;
            GameObject productVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            productVisual.name = "ProductVisual";
            productVisual.transform.SetParent(itemObject.transform, false);
            productVisual.transform.localScale = Vector3.one * 0.8f;
            GameObject openedShell = new GameObject("OpenedParcelShell");
            openedShell.transform.SetParent(world, false);
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                GarageStockFlowSession.ItemInstanceIdValue,
                GarageStockFlowSession.ProductDisplayName,
                body,
                Vector3.one * 0.5f,
                Vector3.zero,
                Vector3.zero);
            DeliveryParcelProjection parcel = itemObject.AddComponent<DeliveryParcelProjection>();
            parcel.Configure(item, sealedVisual, productVisual, openedShell);
            InventoryItemWorldBinding binding = itemObject.AddComponent<InventoryItemWorldBinding>();
            GarageStockFlowRuntime runtime = _root.AddComponent<GarageStockFlowRuntime>();
            runtime.Configure(binding, null, null, null, null, null, null);
            GarageStockFlowSession session = runtime.EnsureInitialized();
            return new Fixture(session, binding, item, parcel, anchor);
        }

        private static void FillShelf(GarageStockFlowSession session)
        {
            InventoryUnitCost unitCost = InventoryUnitCost.Create(
                GarageStockFlowSession.PrototypeCurrencyCode,
                GarageStockFlowSession.PrototypeUnitCostMinorUnits).Value;
            for (int index = 0; index < 8; index++)
            {
                Assert.That(session.Inventory.ReceiveSerializedItem(
                    StableId<ItemInstanceIdScope>.Parse($"inventory.item.shelf-filler-{index:00}"),
                    session.ProductId,
                    session.ShelfContainerId,
                    InventoryCondition.New,
                    unitCost).IsSuccess,
                    Is.True);
            }
        }

        private static void AssertLedgerEntry(
            EconomyLedgerEntryRecord entry,
            EconomyAccountKind account,
            EconomyEntryDirection direction,
            long minorUnits)
        {
            Assert.That(entry.Account, Is.EqualTo(account));
            Assert.That(entry.Direction, Is.EqualTo(direction));
            Assert.That(entry.Currency.Value,
                Is.EqualTo(GarageStockFlowSession.PrototypeCurrencyCode));
            Assert.That(entry.MinorUnits, Is.EqualTo(minorUnits));
        }

        private static void AssertLocation(
            GarageStockFlowSession session,
            StableId<ContainerIdScope> expectedContainer)
        {
            Assert.That(session.TryGetItem(out InventoryItemRecord item), Is.True);
            Assert.That(item.Id, Is.EqualTo(session.ItemId));
            Assert.That(item.ContainerId, Is.EqualTo(expectedContainer));
        }

        private static SimulationTimestamp Timestamp(long tick)
        {
            return SimulationTimestamp.Create(tick, tick * 1000L);
        }

        private readonly struct Fixture
        {
            public Fixture(
                GarageStockFlowSession session,
                InventoryItemWorldBinding binding,
                PhysicalItemProjection item,
                DeliveryParcelProjection parcel,
                Transform anchor)
            {
                Session = session;
                Binding = binding;
                Item = item;
                Parcel = parcel;
                Anchor = anchor;
            }

            public GarageStockFlowSession Session { get; }

            public InventoryItemWorldBinding Binding { get; }

            public PhysicalItemProjection Item { get; }

            public DeliveryParcelProjection Parcel { get; }

            public Transform Anchor { get; }
        }
    }
}
