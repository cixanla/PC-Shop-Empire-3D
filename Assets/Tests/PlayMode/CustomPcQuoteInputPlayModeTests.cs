using System.Collections;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.Retail;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed class CustomPcQuoteInputPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardConsultsAcceptsCustomBuildAndAtomicallyReservesTenParts()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(
                session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            Assert.That(customerFlow, Is.Not.Null);
            yield return WaitForCustomerState(
                customerFlow,
                CustomerVisitState.Browsing);
            MovePlayerToCustomer(marker, customerFlow);
            customerFlow.RefreshPresentation();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.True);
            Assert.That(customerFlow.ContextualPromptText,
                Does.Contain("ihtiyacını sor"));

            PressInteract(keyboard, customerFlow);

            Assert.That(customerFlow.ConsultationCompleted, Is.True);
            Assert.That(customerFlow.CustomPcRequestAccepted, Is.False);
            Assert.That(customerFlow.CanProgressCurrentCustomPc, Is.True);
            Assert.That(customerFlow.ContextualPromptText,
                Does.Contain("özel oyun PC'si talebini kabul et"));
            Assert.That(customerFlow.CurrentOfferDecision, Is.Not.Null);
            ReleaseKeyboard(keyboard);
            yield return new WaitForFixedUpdate();

            PressInteract(keyboard, customerFlow);

            Assert.That(customerFlow.CustomPcRequestAccepted, Is.True);
            Assert.That(customerFlow.CustomPcQuoteReady, Is.False);
            Assert.That(customerFlow.CurrentOfferDecision, Is.Null);
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(1));
            Assert.That(customerFlow.ContextualPromptText,
                Does.Contain("10 parçayı ayır"));
            long visitRevision = session.CustomerVisits.Revision;
            long actionRevision = session.CustomerOfferActions.Revision;
            long basketRevision = session.RetailBaskets.Revision;
            OperationResult suppressedRetailAction =
                marker.StockFlow.ItemBinding.TryApplyCurrentCustomerDecision();
            Assert.That(suppressedRetailAction.Error,
                Is.EqualTo(CustomerOfferDecisionActionFailures.InputInvalid));
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(visitRevision));
            Assert.That(session.CustomerOfferActions.Revision, Is.EqualTo(actionRevision));
            Assert.That(session.RetailBaskets.Revision, Is.EqualTo(basketRevision));
            long inventoryRevision = session.Inventory.Revision;
            int reservationCount = session.Inventory.ReservationCount;
            ReleaseKeyboard(keyboard);
            yield return new WaitForFixedUpdate();

            PressInteract(keyboard, customerFlow);

            Assert.That(customerFlow.CustomPcQuoteReady, Is.True);
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(2));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision + 1));
            Assert.That(session.Inventory.ReservationCount,
                Is.EqualTo(reservationCount +
                    CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(quote.ReservedSerializedItemCount,
                Is.EqualTo(CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(quote.TotalPrice.MinorUnits,
                Is.EqualTo(GarageStockFlowSession.PrototypeCustomPcTotalPriceMinorUnits));
            Assert.That(customerFlow.ContextualPromptText, Is.Empty);
            Assert.That(customerFlow.CustomerSpeechText.text, Does.Contain("10 PARÇA"));
            Assert.That(customerFlow.StateText, Does.Contain("ÖZEL PC TEKLİFİ HAZIR"));
            Assert.That(session.RetailBaskets.Count, Is.Zero);
            Assert.That(session.CustomerOfferActions.Count, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator CustomPcLiveFrameKeyboardGamepadRangeLosPauseReleaseSingleConsumer()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            MovePlayerToCustomerAtDistance(marker, customerFlow, 4.0f);
            customerFlow.RefreshPresentation();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            MovePlayerToCustomer(marker, customerFlow);
            Vector3 focusTarget = customerFlow.CustomerVisualRoot.transform.position +
                                  (Vector3.up * 1.35f);
            Vector3 cameraPosition = customerFlow.PlayerCamera.transform.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "CustomPcConsultationLosBlocker";
            blocker.transform.position = Vector3.Lerp(cameraPosition, focusTarget, 0.5f);
            blocker.transform.localScale = new Vector3(0.55f, 0.75f, 0.18f);
            blocker.transform.rotation = Quaternion.LookRotation(
                focusTarget - cameraPosition,
                Vector3.up);
            Physics.SyncTransforms();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;
            Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();
            Assert.That(customerFlow.CanConsultCurrentCustomer, Is.True);

            marker.PlayerMotor.SetPaused(true);
            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.Escape, Key.E));
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = (1u << (int)GamepadButton.Start) |
                              (1u << (int)GamepadButton.South)
                });
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.True);
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return null;
            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.Start });
            yield return null;
            Assert.That(marker.PlayerMotor.IsPaused, Is.False);
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return null;

            PhysicalItemProjection overlapItem = CreateConsultationOverlapItem(
                marker,
                customerFlow);
            marker.PlayerCarry.ProcessInputFrame();
            Assert.That(marker.PlayerCarry.FocusedItem, Is.SameAs(overlapItem));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            yield return null;
            Assert.That(customerFlow.ConsultationCompleted, Is.True);
            Assert.That(session.CustomerConsultations.Revision, Is.EqualTo(1));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(overlapItem.Ownership, Is.EqualTo(PhysicalItemOwnership.World));
            yield return null;
            yield return null;
            Assert.That(customerFlow.CustomPcRequestAccepted, Is.False);
            Assert.That(session.CustomerConsultations.Revision, Is.EqualTo(1));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            yield return null;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            yield return null;
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(customerFlow.CustomPcRequestAccepted, Is.True);
            Assert.That(customerFlow.CustomPcQuoteReady, Is.False);
            Assert.That(customerFlow.ContextualPromptText,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            Assert.That(customerFlow.CurrentOfferDecision, Is.Null);
            yield return null;
            yield return null;
            Assert.That(customerFlow.CustomPcQuoteReady, Is.False);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            yield return null;

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState { buttons = 1u << (int)GamepadButton.South });
            yield return null;
            Assert.That(customerFlow.CustomPcQuoteReady, Is.True);
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(2));
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(quote.ReservedSerializedItemCount,
                Is.EqualTo(CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            Object.DestroyImmediate(overlapItem.gameObject);
            Physics.SyncTransforms();
        }

        [UnityTest]
        public IEnumerator PauseInteractAtBrowsingDeadlineDefersPatienceAndDrainsSingleEdge()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            MovePlayerToCustomer(marker, customerFlow);
            customerFlow.enabled = false;
            CustomerVisitRecord browsing = customerFlow.CurrentVisit;
            Assert.That(browsing, Is.Not.Null);
            int renderedFrame = Time.frameCount;
            int stepCount = 0;
            while (!customerFlow.CurrentSimulationTime.IsAtOrAfter(
                       browsing.StateDeadline))
            {
                customerFlow.ProcessFixedStep(renderedFrame);
                Assert.That(++stepCount, Is.LessThan(10_000));
            }

            Assert.That(customerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Browsing));
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.None));
            Assert.That(customerFlow.CustomerVisible, Is.True);
            long visitRevision = session.CustomerVisits.Revision;
            long consultationRevision = session.CustomerConsultations.Revision;
            long requestRevision = session.CustomPcQuotes.Revision;
            customerFlow.ProcessFixedStep(renderedFrame);
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(visitRevision));

            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.Escape, Key.E));
            InputSystem.Update();
            marker.PlayerMotor.ProcessInputFrame();
            customerFlow.ProcessInputFrame();
            marker.PlayerCarry.ProcessInputFrame();

            Assert.That(marker.PlayerMotor.IsPaused, Is.True);
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(visitRevision));
            Assert.That(session.CustomerConsultations.Revision,
                Is.EqualTo(consultationRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(requestRevision));
            Assert.That(customerFlow.ConsultationCompleted, Is.False);
            Assert.That(marker.PlayerCarry.HeldItem, Is.Null);
            customerFlow.ProcessFixedStep(renderedFrame + 1);
            Assert.That(customerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Browsing));
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.None));
            Assert.That(customerFlow.CustomerVisible, Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator BrowsingDeadlineWithoutInputExpiresExactlyOnceOnNextRenderedFrame()
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            customerFlow.enabled = false;
            CustomerVisitRecord browsing = customerFlow.CurrentVisit;
            int renderedFrame = Time.frameCount;
            int stepCount = 0;
            while (!customerFlow.CurrentSimulationTime.IsAtOrAfter(
                       browsing.StateDeadline))
            {
                customerFlow.ProcessFixedStep(renderedFrame);
                Assert.That(++stepCount, Is.LessThan(10_000));
            }

            Assert.That(customerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Browsing));
            long visitRevision = session.CustomerVisits.Revision;
            customerFlow.ProcessFixedStep(renderedFrame);
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(visitRevision));

            customerFlow.ProcessFixedStep(renderedFrame + 1);

            Assert.That(customerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Exiting));
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.PatienceExpired));
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(visitRevision + 1));
            Assert.That(session.CustomPcQuotes.Revision, Is.Zero);
            Assert.That(session.RetailBaskets.Count, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        [UnityTest]
        public IEnumerator AcceptedCustomPcRequestSurvivesOriginalDeadlineAndKeepsVisibleQuote()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker =
                Object.FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.AcceptArrivedDelivery().IsSuccess, Is.True);
            Assert.That(session.TransferItem(session.ShelfContainerId).IsSuccess, Is.True);
            Assert.That(session.PublishShelfOffer().IsSuccess, Is.True);
            marker.StockFlow.RefreshPresentation();

            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            yield return WaitForCustomerState(customerFlow, CustomerVisitState.Browsing);
            MovePlayerToCustomer(marker, customerFlow);
            PressInteract(keyboard, customerFlow);
            ReleaseKeyboard(keyboard);
            PressInteract(keyboard, customerFlow);
            Assert.That(customerFlow.CustomPcRequestAccepted, Is.True);
            Assert.That(customerFlow.CustomPcQuoteReady, Is.False);
            CustomerVisitRecord acceptedVisit = customerFlow.CurrentVisit;
            long visitRevision = session.CustomerVisits.Revision;
            long requestRevision = session.CustomPcQuotes.Revision;
            long inventoryRevision = session.Inventory.Revision;
            customerFlow.enabled = false;
            int renderedFrame = Time.frameCount;
            int stepCount = 0;
            while (!customerFlow.CurrentSimulationTime.IsAtOrAfter(
                       SimulationTimestamp.Create(
                           acceptedVisit.StateDeadline.Tick + 1,
                           acceptedVisit.StateDeadline.ElapsedMilliseconds + 20)))
            {
                customerFlow.ProcessFixedStep(renderedFrame++);
                Assert.That(++stepCount, Is.LessThan(10_000));
            }

            Assert.That(customerFlow.CurrentVisit.State,
                Is.EqualTo(CustomerVisitState.Browsing));
            Assert.That(customerFlow.CurrentVisit.ExitReason,
                Is.EqualTo(CustomerVisitExitReason.None));
            Assert.That(customerFlow.CustomerVisible, Is.True);
            Assert.That(customerFlow.CurrentOfferDecision, Is.Null);
            Assert.That(session.CustomerVisits.Revision, Is.EqualTo(visitRevision));
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(requestRevision));
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(customerFlow.ContextualPromptText, Does.Contain("10 parçayı ayır"));

            ReleaseKeyboard(keyboard);
            PressInteract(keyboard, customerFlow);

            Assert.That(customerFlow.CustomPcQuoteReady, Is.True);
            Assert.That(customerFlow.CustomerVisible, Is.True);
            Assert.That(session.CustomPcQuotes.Revision, Is.EqualTo(requestRevision + 1));
            Assert.That(session.TryGetPrototypeCustomPcQuote(
                out CustomPcQuoteRecord quote), Is.True);
            Assert.That(quote.ReservedSerializedItemCount,
                Is.EqualTo(CustomPcQuoteAuthority.GraphicsFirstGamingLineCount));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static void PressInteract(
            Keyboard keyboard,
            GarageCustomerFlowRuntime customerFlow)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            customerFlow.ProcessInputFrame();
        }

        private static void ReleaseKeyboard(Keyboard keyboard)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        private static void MovePlayerToCustomer(
            GaragePrototypeMarker marker,
            GarageCustomerFlowRuntime customerFlow)
        {
            MovePlayerToCustomerAtDistance(marker, customerFlow, 1.55f);
        }

        private static void MovePlayerToCustomerAtDistance(
            GaragePrototypeMarker marker,
            GarageCustomerFlowRuntime customerFlow,
            float distance)
        {
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            Vector3 target = customerFlow.CustomerVisualRoot.transform.position +
                             (Vector3.up * 1.35f);
            Vector3 playerPosition = target - (Vector3.right * distance);
            playerPosition.y = 0.05f;
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(Vector3.right, Vector3.up));
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

        private static PhysicalItemProjection CreateConsultationOverlapItem(
            GaragePrototypeMarker marker,
            GarageCustomerFlowRuntime customerFlow)
        {
            Vector3 target = customerFlow.CustomerVisualRoot.transform.position +
                             (Vector3.up * 1.35f);
            Vector3 cameraPosition = customerFlow.PlayerCamera.transform.position;
            Vector3 direct = (target - cameraPosition).normalized;
            Vector3 itemDirection = Quaternion.AngleAxis(15f, Vector3.up) * direct;
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(itemDirection, Vector3.up);

            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.name = "CustomPcSingleConsumerTestItem";
            itemObject.layer = LayerMask.NameToLayer("Interactable");
            itemObject.transform.position = cameraPosition + (itemDirection * 1.05f);
            itemObject.transform.localScale = Vector3.one * 0.18f;
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection item = itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "physical-item.custom-pc-single-consumer-test",
                "Custom PC Single Consumer Test Item",
                body,
                Vector3.one * 0.09f,
                Vector3.zero,
                Vector3.zero);
            Physics.SyncTransforms();
            return item;
        }

        private static IEnumerator WaitForCustomerState(
            GarageCustomerFlowRuntime customerFlow,
            CustomerVisitState expectedState)
        {
            const int MaximumFixedSteps = 650;
            for (int step = 0; step < MaximumFixedSteps; step++)
            {
                CustomerVisitRecord visit = customerFlow.CurrentVisit;
                if (visit != null && visit.State == expectedState)
                {
                    yield break;
                }

                if (visit != null &&
                    visit.State == CustomerVisitState.Exited &&
                    expectedState != CustomerVisitState.Exited)
                {
                    Assert.Fail(
                        $"Customer exited before {expectedState}: " +
                        $"reason={visit.ExitReason} fallback={visit.RouteFallbackUsed}");
                }

                yield return new WaitForFixedUpdate();
            }

            CustomerVisitRecord finalVisit = customerFlow.CurrentVisit;
            Assert.Fail(
                $"Customer did not reach {expectedState}; " +
                $"actual={finalVisit?.State.ToString() ?? "missing"} " +
                $"reason={finalVisit?.ExitReason.ToString() ?? "missing"}");
        }
    }
}
