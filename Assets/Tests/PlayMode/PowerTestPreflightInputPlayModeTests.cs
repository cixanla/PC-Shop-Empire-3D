using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using PCShopEmpire3D.Actors;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Core.Time;
using PCShopEmpire3D.Presentation;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace PCShopEmpire3D.Tests.PlayMode
{
    public sealed partial class PowerTestPreflightInputPlayModeTests : InputTestFixture
    {
        [UnityTest]
        public IEnumerator KeyboardInteractPublishesOneReceiptWithoutGameplayMutation()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            MovePlayerToPowerTestStation(marker, 1.35f);
            Assert.That(session.TryGetPowerTestAttempts(out _), Is.False);
            yield return null;
            Assert.That(station.InspectInteractionGateForTests().IsSuccess, Is.True);
            string initialPrompt = station.PromptText;
            Assert.That(initialPrompt,
                Does.Contain(marker.PlayerInput.InteractBindingPrompt));
            Assert.That(initialPrompt, Does.Contain("GÜÇ TESTİ ÖN KONTROLÜ"));
            Assert.That(station.PromptText, Is.SameAs(initialPrompt),
                "Repeated same-frame HUD reads must reuse the cached prompt.");
            Assert.That(session.TryGetPowerTestAttempts(out _), Is.False,
                "Prompt and gate observation must not create gameplay authority.");

            long inventoryRevision = session.Inventory.Revision;
            long buildKitRevision = session.CustomPcBuildKit.Revision;
            long assemblyRevision = session.AssemblyBuild.Revision;
            long atx24Revision = session.AssemblyBuild.Atx24PowerCableRevision;
            long eps12vRevision = session.AssemblyBuild.Eps12vPowerCableRevision;
            long pcieRevision = session.AssemblyBuild.PcieGpuPowerCableRevision;
            int assemblyReceiptCount = session.AssemblyBuild.ReceiptCount;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True);
            station.ProcessInputFrame();

            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(station.PromptText,
                Does.Contain("ÖN KONTROL GEÇTİ")
                    .And.Contain("GÜCÜ AÇ"));
            Assert.That(session.PowerTestAttempts.Revision, Is.EqualTo(1));
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.EqualTo(1));
            Assert.That(session.PowerTestAttempts.HasCompletedPreflight, Is.True);
            Assert.That(session.Inventory.Revision, Is.EqualTo(inventoryRevision));
            Assert.That(session.CustomPcBuildKit.Revision,
                Is.EqualTo(buildKitRevision));
            Assert.That(session.AssemblyBuild.Revision, Is.EqualTo(assemblyRevision));
            Assert.That(session.AssemblyBuild.Atx24PowerCableRevision,
                Is.EqualTo(atx24Revision));
            Assert.That(session.AssemblyBuild.Eps12vPowerCableRevision,
                Is.EqualTo(eps12vRevision));
            Assert.That(session.AssemblyBuild.PcieGpuPowerCableRevision,
                Is.EqualTo(pcieRevision));
            Assert.That(session.AssemblyBuild.ReceiptCount,
                Is.EqualTo(assemblyReceiptCount));
            Assert.That(station.TryAttemptAuthorizedForTests().Error,
                Is.EqualTo(ElectricalPowerTestStationFailures.InputReplay));
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.EqualTo(1));
            Assert.That(marker.ElectricalReadinessWorkbench.HasAcceptedPreflight,
                Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasCurrentAcceptedPreflight, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("ÖN KONTROL GEÇTİ")
                    .And.Contain("380W")
                    .And.Contain("500W")
                    .And.Contain("550W")
                    .And.Contain("POWER-ON BEKLİYOR"));
            Assert.That(session.AssemblyBuild.EvaluateBenchmarkReadiness().Error,
                Is.EqualTo(AssemblyFailures.BuildIncomplete));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);

            PowerTestAttemptReceipt historical =
                session.PowerTestAttempts.EvaluateCurrentReceipt().Value;
            Require(session.UnroutePcieGpuPowerCable(
                OperationId("unroute-pcie-gpu-after-preflight"),
                OperationId("route-pcie-gpu"),
                session.AssemblyBuild.PcieGpuPowerCableRevision));
            Require(session.RoutePcieGpuPowerCable(
                OperationId("reroute-pcie-gpu-after-preflight"),
                PowerCableKeyOrientation.Keyed,
                session.AssemblyBuild.PcieGpuPowerCableRevision));
            OperationResult stalePresentation =
                marker.ElectricalReadinessWorkbench.RefreshPresentation();
            Assert.That(stalePresentation.Error,
                Is.EqualTo(PowerTestAttemptFailures.ContextStale));
            Assert.That(marker.ElectricalReadinessWorkbench.HasAcceptedPreflight,
                Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench
                .HasCurrentAcceptedPreflight, Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("ÖN KONTROL GEÇERSİZ")
                    .And.Contain("YAPILANDIRMA DEĞİŞTİ")
                    .And.Contain("POWER-ON BEKLİYOR"));
            Assert.That(session.PowerTestAttempts.TryGetReceipt(
                session.PrototypePowerTestAttemptOperationId,
                out PowerTestAttemptReceipt retained), Is.True);
            Assert.That(retained, Is.SameAs(historical));
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.EqualTo(1));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator GamepadInteractUsesSameCommandAndReceipt()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;
            MovePlayerToPowerTestStation(marker, 1.35f);
            Assert.That(station.InspectInteractionGateForTests().IsSuccess, Is.True);

            InputSystem.QueueStateEvent(
                gamepad,
                new GamepadState
                {
                    buttons = 1u << (int)GamepadButton.South
                });
            InputSystem.Update();
            Assert.That(marker.PlayerInput.UsesGamepadPrompts, Is.True);
            Assert.That(station.PromptText, Does.Contain("A"));
            station.ProcessInputFrame();

            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.False);
            Assert.That(session.PowerTestAttempts.Revision, Is.EqualTo(1));
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.EqualTo(1));
            Assert.That(session.PowerTestAttempts.ValidateReceiptHistory().IsSuccess,
                Is.True);
            Assert.That(marker.GetComponent<GaragePrototypeHud>()
                    .ElectricalPowerTestStation,
                Is.SameAs(station));
            Assert.That(marker.GetComponent<GaragePrototypeHud>()
                    .EffectivePromptText,
                Does.Contain("ÖN KONTROL GEÇTİ")
                    .And.Contain("GÜCÜ AÇ"));
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
            InputSystem.QueueStateEvent(gamepad, new GamepadState());
            InputSystem.Update();
        }

        [UnityTest]
        public IEnumerator FocusRangeOcclusionPauseAndBusyGatesFailClosed()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            GaragePrototypeMarker marker = null;
            yield return LoadPowerReadyGarage(keyboard, value => marker = value);

            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            ElectricalPowerTestStationProjection station =
                marker.ElectricalPowerTestStation;

            MovePlayerToPowerTestStation(
                marker,
                station.InteractionRange + 0.65f);
            Assert.That(station.InspectInteractionGateForTests().Error,
                Is.EqualTo(ElectricalPowerTestStationFailures.OutOfRange));

            MovePlayerToPowerTestStation(marker, 1.35f);
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            Quaternion centeredLook = cameraPivot.rotation;
            cameraPivot.rotation =
                Quaternion.AngleAxis(30f, Vector3.up) * centeredLook;
            Physics.SyncTransforms();
            Assert.That(station.InspectInteractionGateForTests().Error,
                Is.EqualTo(ElectricalPowerTestStationFailures.FocusMissing));
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            station.ProcessInputFrame();
            Assert.That(marker.PlayerInput.InteractPressedThisFrame, Is.True,
                "Off-target power test must not steal another world interaction.");
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.Zero);
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();

            MovePlayerToPowerTestStation(marker, 1.35f);
            Vector3 origin = station.PlayerCamera.transform.position;
            Vector3 target = station.FocusAnchor.position;
            GameObject blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "PowerTestPreflightLosBlocker";
            blocker.transform.position = Vector3.Lerp(origin, target, 0.5f);
            blocker.transform.localScale = new Vector3(0.45f, 0.55f, 0.12f);
            Physics.SyncTransforms();
            Assert.That(station.InspectInteractionGateForTests().Error,
                Is.EqualTo(ElectricalPowerTestStationFailures.LineOfSightBlocked));
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.Zero);
            UnityEngine.Object.DestroyImmediate(blocker);
            Physics.SyncTransforms();

            marker.PlayerMotor.SetPaused(true);
            Assert.That(station.InspectInteractionGateForTests().Error,
                Is.EqualTo(ElectricalPowerTestStationFailures.Paused));
            marker.PlayerMotor.SetPaused(false);

            InputSystem.QueueStateEvent(
                keyboard,
                new KeyboardState(Key.Escape, Key.E));
            InputSystem.Update();
            marker.PlayerMotor.ProcessInputFrame(0f, 0f);
            station.ProcessInputFrame();
            Assert.That(marker.PlayerMotor.IsPaused, Is.True);
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.Zero,
                "Pause + Interact co-edge must not run a preflight.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            marker.PlayerMotor.SetPaused(false);

            MovePlayerToPowerTestStation(marker, 1.35f);
            PhysicalItemProjection largeBox = UnityEngine.Object
                .FindObjectsByType<PhysicalItemProjection>(FindObjectsSortMode.None)
                .Single(item => item.CarryProfile == PhysicalCarryProfile.LargeBox);
            PhysicalItemProjection competingItem =
                CreateCompetingInteractItem(station);
            Assert.That(marker.PlayerCarry.HasCompetingWorldInteractOwner, Is.True,
                "The test item must be the resolver's current Interact owner.");
            Assert.That(station.InspectInteractionGateForTests().Error,
                Is.EqualTo(
                    ElectricalPowerTestStationFailures.CompetingInteractOwner));
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.Zero);
            UnityEngine.Object.DestroyImmediate(competingItem.gameObject);
            Physics.SyncTransforms();

            Require(marker.PlayerCarry.TryPickup(largeBox));
            Assert.That(station.InspectInteractionGateForTests().Error,
                Is.EqualTo(ElectricalPowerTestStationFailures.HandsBusy));
            Assert.That(session.PowerTestAttempts.Revision, Is.Zero);
            Assert.That(session.PowerTestAttempts.ReceiptCount, Is.Zero);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static IEnumerator LoadPowerReadyGarage(
            Keyboard keyboard,
            Action<GaragePrototypeMarker> assign)
        {
            GaragePrototypeMarker marker = null;
            yield return LoadGarage(value => marker = value);
            PreparePhysicalWorkOrder(marker, keyboard);
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(
                out _), Is.True);
            StageAllComponents(session);
            AssemblePowerReadyConfiguration(session);
            marker.ElectricalReadinessWorkbench.RefreshPresentation();
            Assert.That(session.PowerBudget.AssessPowerBudget().IsSuccess, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.IsReady, Is.True);
            Assert.That(marker.ElectricalReadinessWorkbench.HasAcceptedPreflight,
                Is.False);
            Assert.That(marker.ElectricalReadinessWorkbench.StatusText.text,
                Does.Contain("GÜÇ TESTİ BEKLİYOR"));
            assign(marker);
        }

        private static IEnumerator LoadGarage(Action<GaragePrototypeMarker> assign)
        {
            AsyncOperation load = SceneManager.LoadSceneAsync(
                "GarageGraybox",
                LoadSceneMode.Single);
            Assert.That(load, Is.Not.Null);
            yield return load;
            yield return new WaitForFixedUpdate();
            yield return null;

            GaragePrototypeMarker marker = UnityEngine.Object
                .FindFirstObjectByType<GaragePrototypeMarker>();
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.ElectricalPowerTestStation, Is.Not.Null);
            marker.PlayerMotor.SetPaused(false);
            assign(marker);
        }

        private static void PreparePhysicalWorkOrder(
            GaragePrototypeMarker marker,
            Keyboard keyboard)
        {
            GarageStockFlowSession session = marker.StockFlow.EnsureInitialized();
            GarageCustomerFlowRuntime customerFlow = marker.CustomerFlow;
            customerFlow.enabled = false;
            SimulationTimestamp now = customerFlow.CurrentSimulationTime;
            if (!session.TryGetPrototypeCustomerVisit(out CustomerVisitRecord visit))
            {
                Require(session.StartPrototypeCustomerVisit(After(now, 1)));
                Assert.That(session.TryGetPrototypeCustomerVisit(out visit), Is.True);
            }

            if (visit.State == CustomerVisitState.Entering)
            {
                Require(session.MarkPrototypeCustomerBrowseArrival(After(now, 2)));
                Assert.That(session.TryGetPrototypeCustomerVisit(out visit), Is.True);
            }

            Assert.That(visit.State, Is.EqualTo(CustomerVisitState.Browsing));
            Require(session.ConsultPrototypeCustomer(After(now, 3)));
            Require(session.AcceptPrototypeCustomPcRequest(After(now, 4)));
            Require(session.CreatePrototypeCustomPcQuote(After(now, 5)));
            Assert.That(session.TryGetPrototypeCustomPcQuote(out _), Is.True);

            CustomPcWorkTicketStationProjection workTicket =
                marker.CustomPcWorkTicketStation;
            MovePlayerToWorkTicket(marker, workTicket, 1.35f);
            workTicket.RefreshPresentation();
            Assert.That(workTicket.IsFocused, Is.True);
            PressInteract(keyboard, workTicket.ProcessInputFrame);
            Assert.That(session.TryGetPrototypeCustomPcBuildOrder(out _), Is.True);
        }

        private static SimulationTimestamp After(
            SimulationTimestamp timestamp,
            long tickOffset)
        {
            return SimulationTimestamp.Create(
                timestamp.Tick + tickOffset,
                timestamp.ElapsedMilliseconds + (tickOffset * 20L));
        }

        private static void StageAllComponents(GarageStockFlowSession session)
        {
            Require(session.PickupLooseMotherboardToHands());
            Require(session.PlaceHeldMotherboardInCustomPcBuildKit());
            Require(session.PickupLooseProcessorToHands());
            Require(session.PlaceHeldProcessorInCustomPcBuildKit());
            Require(session.PickupLooseMemoryToHands());
            Require(session.PlaceHeldMemoryModuleInCustomPcBuildKit());
            Require(session.PickupLooseStorageToHands());
            Require(session.PlaceHeldStorageInCustomPcBuildKit());
            Require(session.PickupLooseProcessorCoolerToHands());
            Require(session.PlaceHeldProcessorCoolerInCustomPcBuildKit());
            Require(session.PickupLooseGraphicsCardToHands());
            Require(session.PlaceHeldGraphicsCardInCustomPcBuildKit());
            Require(session.PickupLoosePowerSupplyToHands());
            Require(session.PlaceHeldPowerSupplyInCustomPcBuildKit());
            Require(session.PickupLooseAtx24PowerCableToHands());
            Require(session.PlaceHeldAtx24PowerCableInCustomPcBuildKit());
            Require(session.PickupLooseEps12vPowerCableToHands());
            Require(session.PlaceHeldEps12vPowerCableInCustomPcBuildKit());
            Require(session.PickupLoosePcieGpuPowerCableToHands());
            Require(session.PlaceHeldPcieGpuPowerCableInCustomPcBuildKit());
            Assert.That(session.CustomPcBuildKit.StagedComponentCount,
                Is.EqualTo(10));
        }

        private static void AssemblePowerReadyConfiguration(
            GarageStockFlowSession session)
        {
            Require(session.PickupStagedMotherboardForAssembly());
            StableId<AssemblyOperationIdScope> motherboardAttach =
                OperationId("attach-motherboard");
            Require(session.AttachMotherboard(motherboardAttach));
            StableId<AssemblyOperationIdScope> motherboardSecure =
                OperationId("secure-motherboard");
            Require(session.SecureMotherboardFastener(
                motherboardSecure,
                motherboardAttach,
                session.AssemblyBuild.Revision));

            Require(session.PickupStagedProcessorForAssembly());
            StableId<AssemblyOperationIdScope> processorSeat =
                OperationId("seat-processor");
            Require(session.SeatProcessor(
                processorSeat,
                motherboardAttach,
                motherboardSecure,
                session.AssemblyBuild.Revision));
            StableId<AssemblyOperationIdScope> processorRetain =
                OperationId("retain-processor");
            Require(session.CloseProcessorRetention(
                processorRetain,
                processorSeat,
                session.AssemblyBuild.Revision));

            Require(session.PickupStagedMemoryModuleForAssembly());
            StableId<AssemblyOperationIdScope> memorySeat =
                OperationId("seat-memory");
            Require(session.SeatMemoryModule(
                memorySeat,
                DimmKeyOrientation.NotchAligned,
                motherboardAttach,
                motherboardSecure,
                session.AssemblyBuild.Revision));
            StableId<AssemblyOperationIdScope> memoryRetain =
                OperationId("retain-memory");
            Require(session.CloseMemoryRetention(
                memoryRetain,
                memorySeat,
                session.AssemblyBuild.Revision));

            Require(session.PickupStagedStorageForAssembly());
            StableId<AssemblyOperationIdScope> storageSeat =
                OperationId("seat-storage");
            Require(session.SeatStorageDevice(
                storageSeat,
                M2KeyOrientation.KeyAligned,
                motherboardAttach,
                motherboardSecure,
                session.AssemblyBuild.Revision));
            StableId<AssemblyOperationIdScope> storageSecure =
                OperationId("secure-storage");
            Require(session.SecureStorageDevice(
                storageSecure,
                storageSeat,
                session.AssemblyBuild.Revision));

            Require(session.PickupStagedProcessorCoolerForAssembly());
            StableId<AssemblyOperationIdScope> coolerSeat =
                OperationId("seat-cooler");
            Require(session.SeatProcessorCooler(
                coolerSeat,
                ProcessorCoolerMountOrientation.Primary,
                motherboardAttach,
                motherboardSecure,
                processorSeat,
                processorRetain,
                session.AssemblyBuild.Revision));
            StableId<AssemblyOperationIdScope> coolerRetain =
                OperationId("retain-cooler");
            Require(session.RetainProcessorCooler(
                coolerRetain,
                coolerSeat,
                session.AssemblyBuild.Revision));

            Require(session.PickupStagedGraphicsCardForAssembly());
            StableId<AssemblyOperationIdScope> graphicsSeat =
                OperationId("seat-graphics-card");
            Require(session.SeatGraphicsCard(
                graphicsSeat,
                GraphicsCardMountOrientation.Primary,
                motherboardAttach,
                motherboardSecure,
                session.AssemblyBuild.Revision));
            StableId<AssemblyOperationIdScope> graphicsRetain =
                OperationId("retain-graphics-card");
            Require(session.RetainGraphicsCard(
                graphicsRetain,
                graphicsSeat,
                session.AssemblyBuild.Revision));

            Require(session.PickupStagedPowerSupplyForAssembly());
            StableId<AssemblyOperationIdScope> powerSupplySeat =
                OperationId("seat-power-supply");
            Require(session.SeatPowerSupply(
                powerSupplySeat,
                PowerSupplyMountOrientation.FanToFilteredVent,
                session.AssemblyBuild.Revision));
            StableId<AssemblyOperationIdScope> powerSupplyRetain =
                OperationId("retain-power-supply");
            Require(session.RetainPowerSupply(
                powerSupplyRetain,
                powerSupplySeat,
                session.AssemblyBuild.Revision));

            Require(session.PickupStagedAtx24PowerCableForAssembly());
            Require(session.RouteAtx24PowerCable(
                OperationId("route-atx24"),
                PowerCableKeyOrientation.Keyed,
                session.AssemblyBuild.Atx24PowerCableRevision));
            Require(session.PickupStagedEps12vPowerCableForAssembly());
            Require(session.RouteEps12vPowerCable(
                OperationId("route-eps12v"),
                PowerCableKeyOrientation.Keyed,
                session.AssemblyBuild.Eps12vPowerCableRevision));
            Require(session.PickupStagedPcieGpuPowerCableForAssembly());
            Require(session.RoutePcieGpuPowerCable(
                OperationId("route-pcie-gpu"),
                PowerCableKeyOrientation.Keyed,
                session.AssemblyBuild.PcieGpuPowerCableRevision));

            Assert.That(session.AssemblyBuild.EvaluateElectricalReadiness().IsSuccess,
                Is.True);
            Assert.That(session.ValidateInvariants().IsSuccess, Is.True);
        }

        private static StableId<AssemblyOperationIdScope> OperationId(string suffix)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.issue123.playmode.{suffix}");
        }

        private static T Require<T>(OperationResult<T> result)
        {
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
            return result.Value;
        }

        private static void Require(OperationResult result)
        {
            Assert.That(result.IsSuccess, Is.True, result.Error.Code);
        }

        private static void PressInteract(Keyboard keyboard, Action processInput)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.E));
            InputSystem.Update();
            processInput();
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
        }

        private static void MovePlayerToWorkTicket(
            GaragePrototypeMarker marker,
            CustomPcWorkTicketStationProjection station,
            float distance)
        {
            Vector3 target = station.InteractionCollider.bounds.center;
            Vector3 approach = -station.InteractionCollider.transform.forward;
            approach.y = 0f;
            approach.Normalize();
            MovePlayerAndAim(marker, target + (approach * distance), target);
        }

        private static void MovePlayerToPowerTestStation(
            GaragePrototypeMarker marker,
            float distance)
        {
            Vector3 target = marker.ElectricalPowerTestStation.FocusAnchor.position;
            MovePlayerAndAim(marker, target + (Vector3.back * distance), target);
            marker.PlayerCarry.ProcessInputFrame();
            marker.ElectricalPowerTestStation.ProcessInputFrame();
        }

        private static PhysicalItemProjection CreateCompetingInteractItem(
            ElectricalPowerTestStationProjection station)
        {
            Vector3 origin = station.PlayerCamera.transform.position;
            Vector3 direction = (station.FocusAnchor.position - origin).normalized;
            GameObject itemObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            itemObject.name = "PowerTestPreflightCompetingInteractItem";
            itemObject.SetActive(false);
            itemObject.layer = LayerMask.NameToLayer("Interactable");
            itemObject.transform.position = origin + (direction * 0.75f);
            itemObject.transform.localScale = Vector3.one * 0.18f;
            Rigidbody body = itemObject.AddComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true;
            PhysicalItemProjection item =
                itemObject.AddComponent<PhysicalItemProjection>();
            item.Configure(
                "physical-item.issue123-competing-owner",
                "Issue 123 Competing Interact Item",
                body,
                Vector3.one * 0.09f,
                Vector3.zero,
                Vector3.zero);
            itemObject.SetActive(true);
            Physics.SyncTransforms();
            return item;
        }

        private static void MovePlayerAndAim(
            GaragePrototypeMarker marker,
            Vector3 playerPosition,
            Vector3 target)
        {
            playerPosition.y = 0.05f;
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            CharacterController controller =
                marker.PlayerMotor.GetComponent<CharacterController>();
            controller.enabled = false;
            marker.PlayerMotor.transform.SetPositionAndRotation(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));
            Transform cameraPivot = marker.PlayerMotor.transform.Find("CameraPivot");
            cameraPivot.rotation = Quaternion.LookRotation(
                target - cameraPivot.position,
                Vector3.up);
            controller.enabled = true;
            Physics.SyncTransforms();
        }

    }
}
