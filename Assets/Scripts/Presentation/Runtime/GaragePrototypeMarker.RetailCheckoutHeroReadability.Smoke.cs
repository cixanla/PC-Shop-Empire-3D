using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Economy;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string RetailCheckoutHeroReadabilitySmokeSuccessMarker =
            "GARAGE_RETAIL_CHECKOUT_HERO_READABILITY_RUNTIME_SMOKE " +
            "states=customer-approach+shelf-offer-basket+" +
            "checkout-payment-receipt hero=ready " +
            "materials=dark-metal+brushed-steel+rubber+safety-accent+" +
            "label-paper light=focused total-renderers=502 lights=5 cameras=1 " +
            "screenshots=3 glare=bounded glare-pixels<=256 " +
            "contrast=bounded contrast-ratio>=1.25 " +
            "ui=hud-suppressed world-text=preserved human=false";

        private const string RetailCheckoutCaptureDirectoryArgument =
            "-pse-retail-checkout-capture-directory=";
        private const int RetailCheckoutMaximumGlarePixels = 256;
        private const byte RetailCheckoutGlareChannelThreshold = 250;
        private const float RetailCheckoutMinimumContrastRatio = 1.25f;
        private const int RetailCheckoutRouteStepLimit = 900;
        private bool retailCheckoutUnexpectedExceptionObserved;

        private IEnumerator RunRetailCheckoutHeroReadabilitySmoke()
        {
            yield return null;
            playerMotor?.SetPaused(true);
            yield return new WaitForFixedUpdate();

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            InventoryItemWorldBinding binding = stockFlow != null
                ? stockFlow.ItemBinding
                : null;
            RetailCheckoutHeroProjection heroProjection =
                FindFirstObjectByType<RetailCheckoutHeroProjection>();
            float originalAgentSpeed = 0f;
            bool hasOriginalAgentSpeed = false;
            retailCheckoutUnexpectedExceptionObserved = false;
            Application.logMessageReceived +=
                HandleRetailCheckoutHeroReadabilityRuntimeLog;

            try
            {
                if (playerMotor == null ||
                    playerCarry == null ||
                    stockFlow == null ||
                    session == null ||
                    binding == null ||
                    binding.Projection == null ||
                    binding.Parcel == null ||
                    customerFlow == null ||
                    customerFlow.CustomerAgent == null ||
                    checkoutStation == null ||
                    checkoutStation.InteractionCollider == null ||
                    heroProjection == null)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        "smoke.context-missing");
                    yield break;
                }

                originalAgentSpeed = customerFlow.CustomerAgent.speed;
                hasOriginalAgentSpeed = true;
                if (!customerFlow.NavigationReady &&
                    !customerFlow.EnsureNavigationBuilt())
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        "smoke.navigation-unavailable");
                    yield break;
                }

                if (!ValidateRetailCheckoutHeroRuntimeContract(
                        heroProjection,
                        out int initialActiveRendererCount,
                        out string heroFailure))
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(heroFailure);
                    yield break;
                }

                string captureDirectory;
                try
                {
                    captureDirectory = ResolveRetailCheckoutCaptureDirectory();
                    Directory.CreateDirectory(captureDirectory);
                }
                catch (Exception exception)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        $"smoke.capture-directory-failed-" +
                        exception.GetType().Name);
                    yield break;
                }

                OperationResult accept = session.AcceptArrivedDelivery();
                OperationResult openParcel = accept.IsSuccess
                    ? binding.TryOpenParcel()
                    : OperationResult.Fail(accept.Error);
                OperationResult shelfTransfer = openParcel.IsSuccess
                    ? session.TransferItem(session.ShelfContainerId)
                    : OperationResult.Fail(openParcel.Error);
                OperationResult publishOffer = shelfTransfer.IsSuccess
                    ? session.PublishShelfOffer()
                    : OperationResult.Fail(shelfTransfer.Error);
                stockFlow.RefreshPresentation();
                heroProjection.RefreshPresentation();

                Transform shelfAnchor = FindSceneTransform(
                    "RetailShelfOfferDisplayAnchor");
                if (accept.IsFailure ||
                    openParcel.IsFailure ||
                    shelfTransfer.IsFailure ||
                    publishOffer.IsFailure ||
                    shelfAnchor == null ||
                    !session.TryGetItem(out InventoryItemRecord shelfItem) ||
                    shelfItem.Id != session.ItemId ||
                    shelfItem.ProductId != session.ProductId ||
                    shelfItem.ContainerId != session.ShelfContainerId ||
                    !session.TryGetShelfOffer(out ShelfOfferRecord shelfOffer) ||
                    shelfOffer.Id != session.ShelfOfferId ||
                    shelfOffer.ProductId != session.ProductId ||
                    shelfOffer.ShelfContainerId != session.ShelfContainerId ||
                    shelfOffer.Price.MinorUnits !=
                        GarageStockFlowSession.PrototypePriceMinorUnits ||
                    shelfOffer.Price.Currency.Value !=
                        GarageStockFlowSession.PrototypeCurrencyCode ||
                    !heroProjection.ShelfOfferVisual.activeSelf ||
                    heroProjection.BasketReservedVisual.activeSelf ||
                    heroProjection.CashCheckoutVisual.activeSelf ||
                    heroProjection.ReceiptVisual.activeSelf)
                {
                    string code = accept.IsFailure
                        ? accept.Error.Code
                        : openParcel.IsFailure
                            ? openParcel.Error.Code
                            : shelfTransfer.IsFailure
                                ? shelfTransfer.Error.Code
                                : publishOffer.IsFailure
                                    ? publishOffer.Error.Code
                                    : "smoke.stock-offer-setup-mismatch";
                    LogRetailCheckoutHeroReadabilitySmokeFailure(code);
                    yield break;
                }

                binding.Projection.transform.SetPositionAndRotation(
                    shelfAnchor.position,
                    Quaternion.Euler(0f, 90f, 0f));
                binding.Projection.RecordSafePose();
                Physics.SyncTransforms();

                long stableOrderRevision = session.Orders.Revision;
                long stableOfferRevision = session.RetailOffers.Revision;
                long routeInventoryRevision = session.Inventory.Revision;
                long routeBasketRevision = session.RetailBaskets.Revision;
                long routeCheckoutRevision = session.RetailCheckouts.Revision;
                long routeEconomyRevision =
                    session.CheckoutSettlements.Revision;

                customerFlow.CustomerAgent.speed =
                    Mathf.Min(originalAgentSpeed, 0.10f);
                playerMotor.SetPaused(false);
                int waitSteps = 0;
                while (!customerFlow.VisitStarted && waitSteps < 100)
                {
                    waitSteps++;
                    yield return new WaitForFixedUpdate();
                }

                waitSteps = 0;
                while (!customerFlow.HasAssignedRoute && waitSteps < 100)
                {
                    waitSteps++;
                    yield return new WaitForFixedUpdate();
                }

                playerMotor.SetPaused(true);
                yield return new WaitForFixedUpdate();
                CustomerVisitRecord approachVisit = customerFlow.CurrentVisit;
                if (!customerFlow.VisitStarted ||
                    !customerFlow.CustomerVisible ||
                    !customerFlow.HasAssignedRoute ||
                    approachVisit == null ||
                    approachVisit.State != CustomerVisitState.Entering ||
                    approachVisit.TotalRouteFailureCount != 0 ||
                    session.Inventory.Revision != routeInventoryRevision ||
                    session.RetailBaskets.Revision != routeBasketRevision ||
                    session.RetailCheckouts.Revision != routeCheckoutRevision ||
                    session.CheckoutSettlements.Revision != routeEconomyRevision)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        "smoke.customer-approach-state-mismatch");
                    yield break;
                }

                SuppressRetailCaptureUi();
                SetRetailCaptureWorldTextVisibility("CustomerIdentityText");
                stockFlow.RefreshPresentation();
                customerFlow.RefreshPresentation();
                heroProjection.RefreshPresentation();
                SetRetailCapturePose(
                    new Vector3(1.20f, 0.35f, -1.40f),
                    new Vector3(-0.10f, 1.05f, -3.75f),
                    58f);
                LogRetailCheckoutCaptureComposition("customer-approach");
                yield return CaptureLookdevFrame(
                    captureDirectory,
                    "retail-customer-approach-r56.png");
                RestoreRetailCaptureControl(paused: true);

                customerFlow.CustomerAgent.speed = originalAgentSpeed;
                playerMotor.SetPaused(false);
                waitSteps = 0;
                while (waitSteps < RetailCheckoutRouteStepLimit)
                {
                    CustomerVisitRecord candidate = customerFlow.CurrentVisit;
                    if (candidate != null &&
                        candidate.State == CustomerVisitState.Browsing)
                    {
                        break;
                    }

                    waitSteps++;
                    yield return new WaitForFixedUpdate();
                }

                CustomerVisitRecord browsingVisit = customerFlow.CurrentVisit;
                if (browsingVisit == null ||
                    browsingVisit.State != CustomerVisitState.Browsing ||
                    browsingVisit.TotalRouteFailureCount != 0)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        "smoke.customer-browse-state-mismatch");
                    yield break;
                }

                playerMotor.SetPaused(true);
                yield return new WaitForFixedUpdate();
                long visitRevisionBeforeBuy = session.CustomerVisits.Revision;
                long inventoryRevisionBeforeBuy = session.Inventory.Revision;
                long basketRevisionBeforeBuy = session.RetailBaskets.Revision;
                long checkoutRevisionBeforeBuy = session.RetailCheckouts.Revision;
                long economyRevisionBeforeBuy =
                    session.CheckoutSettlements.Revision;
                long consultationRevisionBefore =
                    session.CustomerConsultations.Revision;
                long actionRevisionBefore =
                    session.CustomerOfferActions.Revision;

                OperationResult consultation =
                    session.ConsultPrototypeCustomer(
                        customerFlow.CurrentConsultationTime);
                OperationResult<CustomerOfferDecision> evaluated =
                    session.EvaluatePrototypeCustomerOffer();
                CustomerOfferDecision displayed =
                    customerFlow.CurrentOfferDecision;
                if (consultation.IsFailure ||
                    !evaluated.TryGetValue(
                        out CustomerOfferDecision evaluatedDecision) ||
                    displayed == null ||
                    !displayed.Equals(evaluatedDecision) ||
                    displayed.DecisionKind != CustomerOfferDecisionKind.Buy ||
                    displayed.ReasonCode !=
                        CustomerOfferDecisionReasonCodes
                            .BuyExactProductWithinLimit)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        consultation.IsFailure
                            ? consultation.Error.Code
                            : "smoke.consultation-or-decision-mismatch");
                    yield break;
                }

                OperationResult buy = session.ApplyPrototypeCustomerBuy(
                    displayed,
                    customerFlow.CurrentOfferActionTime);
                customerFlow.RecordOfferActionResult(buy);
                stockFlow.RefreshPresentation();
                customerFlow.RefreshPresentation();
                heroProjection.RefreshPresentation();
                if (buy.IsFailure ||
                    !session.TryGetPrototypeCustomerVisit(
                        out CustomerVisitRecord navigatingVisit) ||
                    navigatingVisit.State !=
                        CustomerVisitState.NavigatingToCheckout ||
                    !session.TryGetPrototypeCustomerBuyAction(out _) ||
                    !session.TryGetPrototypeBasketLine(
                        out RetailBasketLineRecord basketLine) ||
                    !basketLine.IsActionOwned ||
                    basketLine.ItemId != session.ItemId ||
                    session.CustomerConsultations.Revision !=
                        consultationRevisionBefore + 1 ||
                    session.CustomerOfferActions.Revision !=
                        actionRevisionBefore + 1 ||
                    session.CustomerVisits.Revision !=
                        visitRevisionBeforeBuy + 1 ||
                    session.Inventory.Revision !=
                        inventoryRevisionBeforeBuy + 1 ||
                    session.RetailBaskets.Revision !=
                        basketRevisionBeforeBuy + 1 ||
                    session.RetailCheckouts.Revision !=
                        checkoutRevisionBeforeBuy ||
                    session.CheckoutSettlements.Revision !=
                        economyRevisionBeforeBuy ||
                    session.Orders.Revision != stableOrderRevision ||
                    session.RetailOffers.Revision != stableOfferRevision ||
                    !heroProjection.ShelfOfferVisual.activeSelf ||
                    !heroProjection.BasketReservedVisual.activeSelf ||
                    heroProjection.CashCheckoutVisual.activeSelf ||
                    heroProjection.ReceiptVisual.activeSelf)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        buy.IsFailure
                            ? buy.Error.Code
                            : "smoke.buy-basket-projection-mismatch");
                    yield break;
                }

                customerFlow.CustomerAgent.speed = originalAgentSpeed;
                playerMotor.SetPaused(false);
                waitSteps = 0;
                while (waitSteps < RetailCheckoutRouteStepLimit)
                {
                    CustomerVisitRecord candidate = customerFlow.CurrentVisit;
                    if (candidate != null &&
                        candidate.State == CustomerVisitState.AwaitingCheckout)
                    {
                        break;
                    }

                    waitSteps++;
                    yield return new WaitForFixedUpdate();
                }

                CustomerVisitRecord awaitingVisit = customerFlow.CurrentVisit;
                if (awaitingVisit == null ||
                    awaitingVisit.State != CustomerVisitState.AwaitingCheckout ||
                    awaitingVisit.TotalRouteFailureCount != 0 ||
                    session.ValidatePrototypeCustomerCheckoutProvenance()
                        .IsFailure)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        "smoke.checkout-arrival-or-provenance-mismatch");
                    yield break;
                }

                playerMotor.SetPaused(true);
                yield return new WaitForFixedUpdate();
                SetRetailCaptureWorldTextVisibility("RetailShelfLabel");
                SetRetailCapturePose(
                    new Vector3(1.05f, 0.35f, -0.25f),
                    new Vector3(3.24f, 1.35f, 0.55f),
                    58f);
                LogRetailCheckoutCaptureComposition("shelf-offer-basket");
                yield return CaptureLookdevFrame(
                    captureDirectory,
                    "retail-shelf-offer-basket-r56.png");
                RestoreRetailCaptureControl(paused: true);

                playerMotor.SetPaused(true);
                yield return null;
                MovePlayerToCheckoutStation(1.45f);
                yield return null;
                playerMotor.SetPaused(false);
                yield return null;
                checkoutStation.RefreshPresentation();
                if (!checkoutStation.IsFocused ||
                    !checkoutStation.PromptText.Contains("KASAYI BAŞLAT"))
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        string.IsNullOrEmpty(checkoutStation.LastFailureCode)
                            ? "smoke.checkout-focus-missing"
                            : checkoutStation.LastFailureCode);
                    yield break;
                }

                long checkoutRevisionBeforeStart =
                    session.RetailCheckouts.Revision;
                long economyRevisionBeforeStart =
                    session.CheckoutSettlements.Revision;
                OperationResult beginCheckout = checkoutStation.TryOperate();
                heroProjection.RefreshPresentation();
                if (beginCheckout.IsFailure ||
                    !binding.RequiresCheckoutCompletion ||
                    session.RetailCheckouts.Revision !=
                        checkoutRevisionBeforeStart + 1 ||
                    session.CheckoutSettlements.Revision !=
                        economyRevisionBeforeStart ||
                    !session.TryGetPrototypeCheckout(
                        out RetailCheckoutRecord checkout) ||
                    checkout.TotalMinorUnits !=
                        GarageStockFlowSession.PrototypePriceMinorUnits ||
                    !heroProjection.BasketReservedVisual.activeSelf ||
                    !heroProjection.CashCheckoutVisual.activeSelf ||
                    heroProjection.ReceiptVisual.activeSelf)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        beginCheckout.IsFailure
                            ? beginCheckout.Error.Code
                            : "smoke.checkout-start-projection-mismatch");
                    yield break;
                }

                yield return null;
                MovePlayerToCheckoutStation(1.45f);
                checkoutStation.RefreshPresentation();
                long inventoryRevisionBeforeSettlement =
                    session.Inventory.Revision;
                long basketRevisionBeforeSettlement =
                    session.RetailBaskets.Revision;
                long checkoutRevisionBeforeSettlement =
                    session.RetailCheckouts.Revision;
                long economyRevisionBeforeSettlement =
                    session.CheckoutSettlements.Revision;
                OperationResult settleCash = checkoutStation.TryOperate();
                playerMotor.SetPaused(true);
                yield return new WaitForFixedUpdate();

                stockFlow.RefreshPresentation();
                customerFlow.RefreshPresentation();
                checkoutStation.RefreshPresentation();
                heroProjection.RefreshPresentation();
                if (settleCash.IsFailure ||
                    !binding.IsCheckoutSettled ||
                    binding.RequiresCheckoutCompletion ||
                    session.TryGetItem(out _) ||
                    binding.Projection.gameObject.activeSelf ||
                    !session.TryGetPrototypeCheckoutSettlement(
                        out CheckoutSettlementReceipt receipt) ||
                    receipt.PaymentMethod != CheckoutPaymentMethod.Cash ||
                    receipt.GrossMinorUnits !=
                        GarageStockFlowSession.PrototypePriceMinorUnits ||
                    receipt.CostOfGoodsSoldMinorUnits !=
                        GarageStockFlowSession.PrototypeUnitCostMinorUnits ||
                    !session.TryGetPrototypeLedgerTransaction(
                        out EconomyLedgerTransactionRecord transaction) ||
                    transaction.Entries.Count != 4 ||
                    session.Inventory.Revision !=
                        inventoryRevisionBeforeSettlement + 1 ||
                    session.RetailBaskets.Revision !=
                        basketRevisionBeforeSettlement + 1 ||
                    session.RetailCheckouts.Revision !=
                        checkoutRevisionBeforeSettlement + 1 ||
                    session.CheckoutSettlements.Revision !=
                        economyRevisionBeforeSettlement + 1 ||
                    session.Orders.Revision != stableOrderRevision ||
                    session.RetailOffers.Revision != stableOfferRevision ||
                    session.ValidateInvariants().IsFailure ||
                    !stockFlow.CheckoutStatusText.Contains("NAKİT ALINDI") ||
                    !checkoutStation.StationStatusText.text.Contains(
                        "NAKİT ALINDI") ||
                    heroProjection.BasketReservedVisual.activeSelf ||
                    heroProjection.CashCheckoutVisual.activeSelf ||
                    !heroProjection.ReceiptVisual.activeSelf)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        settleCash.IsFailure
                            ? settleCash.Error.Code
                            : "smoke.cash-settlement-receipt-mismatch");
                    yield break;
                }

                SetRetailCaptureWorldTextVisibility(
                    "CheckoutStationStatusText",
                    "CustomerFlowStatusText");
                SetRetailCapturePose(
                    new Vector3(0.95f, 0.35f, 0.85f),
                    new Vector3(0.315f, 1.26f, 2.85f),
                    58f);
                LogRetailCheckoutCaptureComposition(
                    "checkout-payment-receipt");
                yield return CaptureLookdevFrame(
                    captureDirectory,
                    "retail-checkout-payment-receipt-r56.png");
                RestoreRetailCaptureControl(paused: true);

                string[] expectedScreenshots =
                {
                    "retail-customer-approach-r56.png",
                    "retail-shelf-offer-basket-r56.png",
                    "retail-checkout-payment-receipt-r56.png"
                };
                int maximumGlarePixels = 0;
                float minimumContrastRatio = float.PositiveInfinity;
                foreach (string screenshot in expectedScreenshots)
                {
                    string path = Path.Combine(captureDirectory, screenshot);
                    if (!File.Exists(path) || new FileInfo(path).Length <= 0)
                    {
                        LogRetailCheckoutHeroReadabilitySmokeFailure(
                            $"smoke.capture-missing-{screenshot}");
                        yield break;
                    }

                    if (!TryMeasureRetailCheckoutFrame(
                            path,
                            out int glarePixels,
                            out float contrastRatio,
                            out string measureFailure))
                    {
                        LogRetailCheckoutHeroReadabilitySmokeFailure(
                            $"smoke.capture-measure-failed-{screenshot}-" +
                            measureFailure);
                        yield break;
                    }

                    maximumGlarePixels = Mathf.Max(
                        maximumGlarePixels,
                        glarePixels);
                    minimumContrastRatio = Mathf.Min(
                        minimumContrastRatio,
                        contrastRatio);
                    Debug.Log(
                        "GARAGE_RETAIL_CHECKOUT_HERO_CAPTURE_METRICS " +
                        $"file={screenshot} glare-pixels={glarePixels} " +
                        $"contrast-ratio={contrastRatio:F3}");
                    if (glarePixels > RetailCheckoutMaximumGlarePixels ||
                        contrastRatio < RetailCheckoutMinimumContrastRatio)
                    {
                        LogRetailCheckoutHeroReadabilitySmokeFailure(
                            $"smoke.capture-budget-exceeded-{screenshot}-" +
                            $"glare-{glarePixels}-contrast-" +
                            $"{contrastRatio:F3}");
                        yield break;
                    }
                }

                playerMotor.SetPaused(false);
                waitSteps = 0;
                while (waitSteps < RetailCheckoutRouteStepLimit)
                {
                    CustomerVisitRecord candidate = customerFlow.CurrentVisit;
                    if (candidate != null &&
                        candidate.State == CustomerVisitState.Exited)
                    {
                        break;
                    }

                    waitSteps++;
                    yield return new WaitForFixedUpdate();
                }

                yield return null;
                customerFlow.RefreshPresentation();
                CustomerVisitRecord exitedVisit = customerFlow.CurrentVisit;
                if (exitedVisit == null ||
                    exitedVisit.State != CustomerVisitState.Exited ||
                    exitedVisit.ExitReason != CustomerVisitExitReason.Fulfilled ||
                    exitedVisit.TotalRouteFailureCount != 0 ||
                    exitedVisit.RouteFallbackUsed ||
                    customerFlow.CustomerVisible)
                {
                    LogRetailCheckoutHeroReadabilitySmokeFailure(
                        "smoke.fulfilled-exit-mismatch-" +
                        $"state-{exitedVisit?.State.ToString() ?? "missing"}-" +
                        $"reason-{exitedVisit?.ExitReason.ToString() ?? "missing"}-" +
                        $"route-failures-" +
                        $"{exitedVisit?.TotalRouteFailureCount.ToString() ?? "missing"}-" +
                        $"fallback-" +
                        $"{(exitedVisit?.RouteFallbackUsed == true ? "yes" : "no")}-" +
                        $"visible-{(customerFlow.CustomerVisible ? "yes" : "no")}-" +
                        $"steps-{waitSteps}");
                    yield break;
                }

                yield return new WaitForEndOfFrame();
                if (retailCheckoutUnexpectedExceptionObserved)
                {
                    yield break;
                }

                Debug.Log(
                    $"{RetailCheckoutHeroReadabilitySmokeSuccessMarker} " +
                    $"active-renderers={initialActiveRendererCount} " +
                    $"max-glare-pixels={maximumGlarePixels} " +
                    $"min-contrast-ratio={minimumContrastRatio:F3} " +
                    $"capture-directory={captureDirectory}");
                if (!Application.isEditor)
                {
                    Application.Quit(0);
                }
            }
            finally
            {
                Application.logMessageReceived -=
                    HandleRetailCheckoutHeroReadabilityRuntimeLog;
                if (playerMotor != null)
                {
                    playerMotor.enabled = true;
                    playerMotor.SetPaused(false);
                }

                if (hasOriginalAgentSpeed &&
                    customerFlow != null &&
                    customerFlow.CustomerAgent != null)
                {
                    customerFlow.CustomerAgent.speed = originalAgentSpeed;
                }
            }
        }

        private static bool ValidateRetailCheckoutHeroRuntimeContract(
            RetailCheckoutHeroProjection heroProjection,
            out int activeRendererCount,
            out string failure)
        {
            activeRendererCount = FindObjectsByType<MeshRenderer>(
                FindObjectsSortMode.None).Length;
            int totalRendererCount = heroProjection.gameObject.scene
                .GetRootGameObjects()
                .SelectMany(root =>
                    root.GetComponentsInChildren<MeshRenderer>(true))
                .Count();
            int lightCount = FindObjectsByType<Light>(
                FindObjectsSortMode.None).Length;
            int cameraCount = FindObjectsByType<Camera>(
                FindObjectsSortMode.None).Length;
            Transform heroRoot = heroProjection.transform;
            Renderer[] heroRenderers = heroRoot.GetComponentsInChildren<Renderer>(true);
            Light retailLight = FindSceneLight("RetailCheckoutFillLight");

            Renderer FindHeroRenderer(string name)
            {
                return heroRenderers.FirstOrDefault(renderer =>
                    renderer.name == name);
            }

            var mismatches = new List<string>();
            if (heroRenderers.Length != 9)
            {
                mismatches.Add($"hero-renderers-{heroRenderers.Length}");
            }

            if (heroRoot.GetComponentsInChildren<Collider>(true).Length != 0)
            {
                mismatches.Add("hero-colliders");
            }

            if (heroRoot.GetComponentsInChildren<Light>(true).Length != 0)
            {
                mismatches.Add("hero-child-lights");
            }

            if (!heroRenderers.All(renderer =>
                    renderer.gameObject.layer ==
                    LayerMask.NameToLayer("Ignore Raycast") &&
                    renderer.shadowCastingMode ==
                    UnityEngine.Rendering.ShadowCastingMode.Off &&
                    !renderer.receiveShadows &&
                    renderer.motionVectorGenerationMode ==
                    MotionVectorGenerationMode.ForceNoMotion))
            {
                mismatches.Add("renderer-cost-contract");
            }

            bool materialsMatch =
                MaterialNameStartsWith(
                    FindHeroRenderer("RetailCheckoutHeroDarkMetalDetails"),
                    "DarkMetal") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("RetailCheckoutHeroBrushedSteelDetails"),
                    "BrushedSteel") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("RetailCheckoutHeroSafetyAccentDetails"),
                    "SafetyAccent") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("RetailCheckoutHeroRubberDetails"),
                    "WorkshopRubber") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("RetailCheckoutLightDiffuser"),
                    "LabelPaper") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("RetailShelfOfferStateVisual"),
                    "LabelPaper") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("RetailBasketReservedStateVisual"),
                    "SafetyAccent") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("CheckoutCashStateVisual"),
                    "LabelPaper") &&
                MaterialNameStartsWith(
                    FindHeroRenderer("CheckoutReceiptStateVisual"),
                    "LabelPaper");
            if (!materialsMatch)
            {
                mismatches.Add("materials");
            }

            if (FindSceneTransform("RetailShelfProductDisplay") != null ||
                FindSceneTransform("CheckoutPaymentPadBody") != null ||
                FindSceneTransform("CheckoutPaymentPadScreen") != null)
            {
                mismatches.Add("duplicate-authority-decoy");
            }

            if (heroProjection.StockFlow == null ||
                heroProjection.ShelfOfferVisual == null ||
                heroProjection.BasketReservedVisual == null ||
                heroProjection.CashCheckoutVisual == null ||
                heroProjection.ReceiptVisual == null)
            {
                mismatches.Add("projection-references");
            }
            else if (heroProjection.ShelfOfferVisual.activeSelf ||
                     heroProjection.BasketReservedVisual.activeSelf ||
                     heroProjection.CashCheckoutVisual.activeSelf ||
                     heroProjection.ReceiptVisual.activeSelf)
            {
                mismatches.Add("initial-visual-state");
            }

            if (retailLight == null ||
                retailLight.type != LightType.Spot ||
                !Mathf.Approximately(retailLight.intensity, 0.42f) ||
                !Mathf.Approximately(retailLight.range, 4.40f) ||
                !Mathf.Approximately(retailLight.spotAngle, 110f) ||
                !Mathf.Approximately(retailLight.innerSpotAngle, 68.2f) ||
                retailLight.shadows != LightShadows.None)
            {
                mismatches.Add("fill-light");
            }

            if (activeRendererCount != 478 ||
                totalRendererCount != 502 ||
                lightCount != 5 ||
                cameraCount != 1)
            {
                mismatches.Add(
                    $"budget-active-{activeRendererCount}-total-" +
                    $"{totalRendererCount}-lights-{lightCount}-" +
                    $"cameras-{cameraCount}");
            }

            failure = mismatches.Count == 0
                ? null
                : "smoke.hero-contract-mismatch-" +
                  string.Join("+", mismatches);
            return mismatches.Count == 0;
        }

        private void SetRetailCapturePose(
            Vector3 playerPosition,
            Vector3 target,
            float fieldOfView)
        {
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));

            Camera camera = playerMotor.GetComponentInChildren<Camera>(true);
            if (camera != null)
            {
                camera.fieldOfView = fieldOfView;
                camera.transform.localRotation = Quaternion.identity;
                Transform cameraPivot =
                    playerMotor.transform.Find("CameraPivot");
                Transform lookTransform = cameraPivot != null
                    ? cameraPivot
                    : camera.transform;
                lookTransform.rotation = Quaternion.LookRotation(
                    target - camera.transform.position,
                    Vector3.up);
            }

            playerMotor.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Physics.SyncTransforms();
        }

        private void RestoreRetailCaptureControl(bool paused)
        {
            playerMotor.enabled = true;
            playerMotor.SetPaused(paused);
            Physics.SyncTransforms();
        }

        private static void SuppressRetailCaptureUi()
        {
            GaragePrototypeHud hud = FindFirstObjectByType<GaragePrototypeHud>();
            if (hud != null)
            {
                hud.enabled = false;
            }

            var preservedWorldText = new HashSet<string>(StringComparer.Ordinal)
            {
                "RetailShelfLabel",
                "CheckoutStationStatusText",
                "CustomerFlowStatusText",
                "CustomerIdentityText"
            };
            foreach (TextMesh text in FindObjectsByType<TextMesh>(
                         FindObjectsSortMode.None))
            {
                Renderer renderer = text.GetComponent<Renderer>();
                if (renderer != null &&
                    !preservedWorldText.Contains(text.name))
                {
                    renderer.enabled = false;
                }
            }

            foreach (Renderer renderer in FindObjectsByType<Renderer>(
                         FindObjectsSortMode.None))
            {
                if (renderer.name == "LeftHand" ||
                    renderer.name == "RightHand")
                {
                    renderer.enabled = false;
                }
            }
        }

        private static void SetRetailCaptureWorldTextVisibility(
            params string[] visibleNames)
        {
            var visible = new HashSet<string>(
                visibleNames ?? Array.Empty<string>(),
                StringComparer.Ordinal);
            var managedWorldText = new HashSet<string>(
                StringComparer.Ordinal)
            {
                "RetailShelfLabel",
                "CheckoutStationStatusText",
                "CustomerFlowStatusText",
                "CustomerIdentityText"
            };
            foreach (TextMesh text in FindObjectsByType<TextMesh>(
                         FindObjectsSortMode.None))
            {
                if (!managedWorldText.Contains(text.name))
                {
                    continue;
                }

                Renderer renderer = text.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.enabled = visible.Contains(text.name);
                }
            }
        }

        private void LogRetailCheckoutCaptureComposition(string state)
        {
            Camera camera = playerMotor != null
                ? playerMotor.GetComponentInChildren<Camera>(true)
                : null;
            if (camera == null)
            {
                return;
            }

            string composition = string.Join(
                " | ",
                FindObjectsByType<Renderer>(FindObjectsSortMode.None)
                    .Where(renderer => renderer.enabled)
                    .Select(renderer => new
                    {
                        Renderer = renderer,
                        Area = ProjectedViewportArea(camera, renderer.bounds)
                    })
                    .Where(entry => entry.Area > 0.01f)
                    .OrderByDescending(entry => entry.Area)
                    .Take(12)
                    .Select(entry =>
                        $"{entry.Renderer.name}:" +
                        $"{entry.Renderer.sharedMaterial?.name ?? "none"}:" +
                        $"{entry.Area:F3}"));
            Debug.Log(
                "GARAGE_RETAIL_CHECKOUT_HERO_CAPTURE_COMPOSITION " +
                $"state={state} {composition}");
        }

        private static bool TryMeasureRetailCheckoutFrame(
            string path,
            out int glarePixels,
            out float contrastRatio,
            out string failure)
        {
            glarePixels = 0;
            contrastRatio = 0f;
            failure = null;
            Texture2D texture = null;
            try
            {
                texture = new Texture2D(
                    2,
                    2,
                    TextureFormat.RGBA32,
                    false,
                    false);
                if (!ImageConversion.LoadImage(
                        texture,
                        File.ReadAllBytes(path),
                        false))
                {
                    failure = "decode-failed";
                    return false;
                }

                Color32[] pixels = texture.GetPixels32();
                int xMinimum = Mathf.Clamp(
                    Mathf.FloorToInt(texture.width * 0.15f),
                    0,
                    texture.width - 1);
                int xMaximum = Mathf.Clamp(
                    Mathf.CeilToInt(texture.width * 0.90f),
                    xMinimum + 1,
                    texture.width);
                int yMinimum = Mathf.Clamp(
                    Mathf.FloorToInt(texture.height * 0.15f),
                    0,
                    texture.height - 1);
                int yMaximum = Mathf.Clamp(
                    Mathf.CeilToInt(texture.height * 0.80f),
                    yMinimum + 1,
                    texture.height);
                int sampleCount =
                    (xMaximum - xMinimum) * (yMaximum - yMinimum);
                if (sampleCount <= 0)
                {
                    failure = "empty-region";
                    return false;
                }

                var luminanceSamples = new float[sampleCount];
                int sampleIndex = 0;
                for (int y = yMinimum; y < yMaximum; y++)
                {
                    int rowOffset = y * texture.width;
                    for (int x = xMinimum; x < xMaximum; x++)
                    {
                        Color32 pixel = pixels[rowOffset + x];
                        if (pixel.r > RetailCheckoutGlareChannelThreshold &&
                            pixel.g > RetailCheckoutGlareChannelThreshold &&
                            pixel.b > RetailCheckoutGlareChannelThreshold)
                        {
                            glarePixels++;
                        }

                        float red = SrgbByteToLinear(pixel.r);
                        float green = SrgbByteToLinear(pixel.g);
                        float blue = SrgbByteToLinear(pixel.b);
                        luminanceSamples[sampleIndex++] =
                            (0.2126f * red) +
                            (0.7152f * green) +
                            (0.0722f * blue);
                    }
                }

                Array.Sort(luminanceSamples);
                float low = luminanceSamples[Mathf.Clamp(
                    Mathf.FloorToInt((sampleCount - 1) * 0.10f),
                    0,
                    sampleCount - 1)];
                float high = luminanceSamples[Mathf.Clamp(
                    Mathf.FloorToInt((sampleCount - 1) * 0.90f),
                    0,
                    sampleCount - 1)];
                contrastRatio = (high + 0.05f) / (low + 0.05f);
                return !float.IsNaN(contrastRatio) &&
                       !float.IsInfinity(contrastRatio);
            }
            catch (Exception exception)
            {
                failure = exception.GetType().Name;
                return false;
            }
            finally
            {
                if (texture != null)
                {
                    UnityEngine.Object.Destroy(texture);
                }
            }
        }

        private static float SrgbByteToLinear(byte value)
        {
            float channel = value / 255f;
            return channel <= 0.04045f
                ? channel / 12.92f
                : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
        }

        private static string ResolveRetailCheckoutCaptureDirectory()
        {
            foreach (string argument in Environment.GetCommandLineArgs())
            {
                if (argument.StartsWith(
                        RetailCheckoutCaptureDirectoryArgument,
                        StringComparison.Ordinal))
                {
                    string requested = argument.Substring(
                        RetailCheckoutCaptureDirectoryArgument.Length);
                    if (string.IsNullOrWhiteSpace(requested))
                    {
                        throw new InvalidOperationException(
                            "The retail checkout capture directory is empty.");
                    }

                    return Path.GetFullPath(requested);
                }
            }

            return Path.Combine(
                Application.persistentDataPath,
                "RetailCheckoutHeroEvidence");
        }

        private void HandleRetailCheckoutHeroReadabilityRuntimeLog(
            string condition,
            string stackTrace,
            LogType type)
        {
            if (type != LogType.Exception)
            {
                return;
            }

            retailCheckoutUnexpectedExceptionObserved = true;
            Application.logMessageReceived -=
                HandleRetailCheckoutHeroReadabilityRuntimeLog;
            Debug.LogError(
                "GARAGE_RETAIL_CHECKOUT_HERO_READABILITY_RUNTIME_SMOKE " +
                "hero-flow=failed code=smoke.unexpected-exception");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }

        private void LogRetailCheckoutHeroReadabilitySmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_RETAIL_CHECKOUT_HERO_READABILITY_RUNTIME_SMOKE " +
                $"hero-flow=failed code={code}");
            if (!Application.isEditor)
            {
                Application.Quit(1);
            }
        }
    }
}
