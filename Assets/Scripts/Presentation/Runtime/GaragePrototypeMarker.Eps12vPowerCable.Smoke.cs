using System.Collections;
using PCShopEmpire3D.Assembly;
using PCShopEmpire3D.Core.Primitives;
using PCShopEmpire3D.Inventory;
using PCShopEmpire3D.Presentation.Interaction;
using PCShopEmpire3D.World.Interaction;
using UnityEngine;

namespace PCShopEmpire3D.Presentation
{
    public sealed partial class GaragePrototypeMarker
    {
        public const string Eps12vPowerCableSmokeSuccessMarker =
            "GARAGE_EPS12V_POWER_CABLE_RUNTIME_SMOKE cable-flow=ok " +
            "preflight=ok psu-retained-gate=ok motherboard-secured-gate=ok " +
            "cpu-retained-gate=ok endpoint-key=ok route-waypoints=ok " +
            "route-clearance=ok generic-bypass-blocked=ok " +
            "duplicate-route-blocked=ok dependent-detach-blocked=ok replay=ok " +
            "authority-isolated=ok identity=stable recovery=ok";

        private IEnumerator RunEps12vPowerCableSmoke()
        {
            yield return null;
            playerMotor?.SetPaused(false);
            yield return null;

            GarageStockFlowSession session = stockFlow != null
                ? stockFlow.EnsureInitialized()
                : null;
            if (session == null ||
                playerMotor == null ||
                playerCarry == null ||
                transportCart == null ||
                motherboardBinding == null ||
                motherboardSeat == null ||
                motherboardFastener == null ||
                powerSupplyBinding == null ||
                processorSocket == null ||
                processorBinding == null ||
                processor == null ||
                eps12vPowerCableRoute == null ||
                eps12vPowerCableBinding == null ||
                eps12vPowerCable == null ||
                eps12vPowerCableGeometry == null)
            {
                LogEps12vPowerCableSmokeFailure("smoke.context-missing");
                yield break;
            }

            int physicalIdentity = eps12vPowerCable.GetInstanceID();
            string stableItemId = eps12vPowerCable.ItemIdValue;
            Transform initialParent = eps12vPowerCable.transform.parent;
            Pose initialPose = new Pose(
                eps12vPowerCable.transform.position,
                eps12vPowerCable.transform.rotation);
            Rigidbody body = eps12vPowerCable.Body;
            bool preflight = session.Inventory.SerializedItemCount == 9 &&
                             session.TryGetEps12vPowerCableItem(
                                 out InventoryItemRecord looseCable) &&
                             looseCable.Id == session.Eps12vPowerCableItemId &&
                             looseCable.ProductId ==
                                 session.Eps12vPowerCableProductId &&
                             looseCable.ContainerId ==
                                 session.WorldFloorContainerId &&
                             session.AssemblyBuild.Eps12vPowerCableState ==
                                 Eps12vPowerCableState.Loose &&
                             session.AssemblyBuild.Eps12vPowerCableRevision == 0 &&
                             eps12vPowerCableRoute.Waypoints.Length == 3 &&
                             eps12vPowerCableGeometry.IsCanonical &&
                             eps12vPowerCableBinding
                                 .ValidateProjectionInvariant().IsSuccess;
            Eps12vPowerCableTopology topology =
                session.AssemblyBuild.Eps12vPowerCableTopology;
            bool connectors = topology != null &&
                              topology.PsuEndpoint.PinCount == 8 &&
                              topology.MotherboardEndpoint.PinCount == 8 &&
                              topology.OrderedWaypoints.Count == 3;
            if (!preflight || !connectors)
            {
                LogEps12vPowerCableSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            StableId<AssemblyOperationIdScope> motherboardAttach =
                Eps12vPowerCableSmokeOperationId("motherboard-attach");
            StableId<AssemblyOperationIdScope> motherboardSecure =
                Eps12vPowerCableSmokeOperationId("motherboard-secure");
            StableId<AssemblyOperationIdScope> powerSupplySeat =
                Eps12vPowerCableSmokeOperationId("power-supply-seat");
            StableId<AssemblyOperationIdScope> powerSupplyRetain =
                Eps12vPowerCableSmokeOperationId("power-supply-retain");
            StableId<AssemblyOperationIdScope> processorSeat =
                Eps12vPowerCableSmokeOperationId("processor-seat");
            StableId<AssemblyOperationIdScope> processorRetain =
                Eps12vPowerCableSmokeOperationId("processor-retain");

            OperationResult pickupMotherboard =
                session.PickupLooseMotherboardToHands();
            OperationResult<AssemblyOperationReceipt> attachMotherboard =
                pickupMotherboard.IsSuccess
                    ? session.AttachMotherboard(motherboardAttach)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        pickupMotherboard.Error);
            OperationResult<AssemblyOperationReceipt> secureMotherboard =
                attachMotherboard.IsSuccess
                    ? session.SecureMotherboardFastener(
                        motherboardSecure,
                        motherboardAttach,
                        session.AssemblyBuild.Revision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        attachMotherboard.Error);
            OperationResult pickupPowerSupply =
                session.PickupLoosePowerSupplyToHands();
            OperationResult<AssemblyOperationReceipt> seatPowerSupply =
                pickupPowerSupply.IsSuccess
                    ? session.SeatPowerSupply(
                        powerSupplySeat,
                        PowerSupplyMountOrientation.FanToFilteredVent,
                        session.AssemblyBuild.Revision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        pickupPowerSupply.Error);
            OperationResult<AssemblyOperationReceipt> retainPowerSupply =
                seatPowerSupply.IsSuccess
                    ? session.RetainPowerSupply(
                        powerSupplyRetain,
                        powerSupplySeat,
                        session.AssemblyBuild.Revision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        seatPowerSupply.Error);
            OperationResult pickupProcessor =
                session.PickupLooseProcessorToHands();
            OperationResult<AssemblyOperationReceipt> seatProcessor =
                pickupProcessor.IsSuccess
                    ? session.SeatProcessor(
                        processorSeat,
                        motherboardAttach,
                        motherboardSecure,
                        session.AssemblyBuild.Revision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        pickupProcessor.Error);
            OperationResult<AssemblyOperationReceipt> retainProcessor =
                seatProcessor.IsSuccess
                    ? session.CloseProcessorRetention(
                        processorRetain,
                        processorSeat,
                        session.AssemblyBuild.Revision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        seatProcessor.Error);

            OperationResult syncMotherboard =
                motherboardBinding.PhysicalItem.SynchronizeStableWorldPose(
                    motherboardSeat.SnapPose);
            motherboardFastener.ApplyAuthoritativeState(
                AssemblySeatState.SeatedSecured);
            OperationResult syncPowerSupply =
                powerSupplyBinding.SyncProjectionToAuthority();
            OperationResult syncProcessorPose =
                processor.SynchronizeStableWorldPose(processorSocket.SnapPose);
            OperationResult syncProcessor =
                processorBinding.SyncProjectionToAuthority();
            Physics.SyncTransforms();
            bool hostsReady = secureMotherboard.IsSuccess &&
                              retainPowerSupply.IsSuccess &&
                              retainProcessor.IsSuccess &&
                              syncMotherboard.IsSuccess &&
                              syncPowerSupply.IsSuccess &&
                              syncProcessorPose.IsSuccess &&
                              syncProcessor.IsSuccess &&
                              motherboardBinding.IsSecured &&
                              powerSupplyBinding.IsRetained &&
                              processorBinding.IsRetained &&
                              session.AssemblyBuild.MotherboardSeatState ==
                                  AssemblySeatState.SeatedSecured &&
                              session.AssemblyBuild.PowerSupplyBayState ==
                                  PowerSupplyBayState.PowerSupplyRetained &&
                              session.AssemblyBuild.ProcessorSocketState ==
                                  ProcessorSocketState.ProcessorRetained;
            if (!hostsReady)
            {
                LogEps12vPowerCableSmokeFailure("smoke.host-setup-failed");
                yield break;
            }

            long ordersRevisionBefore = session.Orders.Revision;
            long offersRevisionBefore = session.RetailOffers.Revision;
            long basketsRevisionBefore = session.RetailBaskets.Revision;
            long checkoutsRevisionBefore = session.RetailCheckouts.Revision;
            long settlementsRevisionBefore =
                session.CheckoutSettlements.Revision;
            long visitsRevisionBefore = session.CustomerVisits.Revision;
            long consultationsRevisionBefore =
                session.CustomerConsultations.Revision;
            long offerActionsRevisionBefore =
                session.CustomerOfferActions.Revision;
            long atx24RevisionBefore =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int atx24ReceiptCountBefore =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;

            MovePlayerToEps12vPowerCableRoute();
            OperationResult pickup = playerCarry.TryPickup(eps12vPowerCable);
            long assemblyRevisionBeforeBypass = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeBypass = session.Inventory.Revision;
            long cableRevisionBeforeBypass =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int cableReceiptsBeforeBypass =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            OperationResult genericPlacement = pickup.IsSuccess
                ? playerCarry.TryConfirmPlacement()
                : OperationResult.Fail(pickup.Error);
            OperationResult genericCart = pickup.IsSuccess
                ? playerCarry.TryLoadHeldItem(transportCart)
                : OperationResult.Fail(pickup.Error);
            bool genericBypassBlocked = pickup.IsSuccess &&
                                        genericPlacement.Error.Code ==
                                            "placement.profile-unsupported" &&
                                        genericCart.Error.Code ==
                                            "cart.load-profile-unsupported" &&
                                        !transportCart.HasCargo &&
                                        playerCarry.HeldItem ==
                                            eps12vPowerCable &&
                                        session.AssemblyBuild.Revision ==
                                            assemblyRevisionBeforeBypass &&
                                        session.Inventory.Revision ==
                                            inventoryRevisionBeforeBypass &&
                                        session.AssemblyBuild
                                                .Eps12vPowerCableRevision ==
                                            cableRevisionBeforeBypass &&
                                        session.AssemblyBuild
                                                .Eps12vPowerCableReceiptCount ==
                                            cableReceiptsBeforeBypass;
            OperationResult mode = genericBypassBlocked
                ? playerCarry.TrySetEps12vPowerCableRouteMode(true)
                : OperationResult.Fail(
                    Failure.FromCode("smoke.generic-bypass-mismatch"));
            long cableRevisionBeforeWrong =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            long inventoryRevisionBeforeWrong = session.Inventory.Revision;
            OperationResult reverse = mode.IsSuccess
                ? playerCarry.TryRotateEps12vPowerCableConnectorPreview()
                : OperationResult.Fail(mode.Error);
            OperationResult wrongConfirm = reverse.IsSuccess
                ? playerCarry.TryConfirmEps12vPowerCableRoute()
                : OperationResult.Fail(reverse.Error);
            bool keyedOrientationGate = wrongConfirm.IsFailure &&
                                        playerCarry
                                                .CurrentEps12vPowerCableRouteStatus ==
                                            Eps12vPowerCableRouteStatus
                                                .OrientationInvalid &&
                                        session.AssemblyBuild
                                                .Eps12vPowerCableRevision ==
                                            cableRevisionBeforeWrong &&
                                        session.Inventory.Revision ==
                                            inventoryRevisionBeforeWrong &&
                                        playerCarry.HeldItem ==
                                            eps12vPowerCable;

            OperationResult restoreKey = playerCarry
                .TryRotateEps12vPowerCableConnectorPreview();
            bool routeClearance = restoreKey.IsSuccess &&
                                  playerCarry.PlacementValid &&
                                  playerCarry
                                          .CurrentEps12vPowerCableRouteStatus ==
                                      Eps12vPowerCableRouteStatus.ValidRoute;
            OperationResult routeResult = routeClearance
                ? playerCarry.TryConfirmEps12vPowerCableRoute()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.route-clearance-mismatch"));
            StableId<AssemblyOperationIdScope> routeOperationId =
                Eps12vPowerCablePrototypeOperationId("route", 1);
            bool hasRouteReceipt = session.AssemblyBuild
                .TryGetEps12vPowerCableReceipt(
                    routeOperationId,
                    out Eps12vPowerCableOperationReceipt routeReceipt);
            bool hostLineage = hasRouteReceipt &&
                               routeReceipt
                                       .SourceMotherboardSecureOperationId ==
                                   motherboardSecure &&
                               routeReceipt
                                       .SourcePowerSupplyRetentionOperationId ==
                                   powerSupplyRetain &&
                               routeReceipt
                                       .SourceProcessorRetentionOperationId ==
                                   processorRetain;
            bool routed = routeResult.IsSuccess &&
                          !playerCarry.IsCarrying &&
                          eps12vPowerCableBinding.IsRouted &&
                          eps12vPowerCableGeometry.IsRouted &&
                          hasRouteReceipt &&
                          routeReceipt.RouteFingerprint == topology.Fingerprint &&
                          session.TryGetEps12vPowerCableItem(
                              out InventoryItemRecord routedCable) &&
                          routedCable.ContainerId ==
                              session.Eps12vPowerCableRouteContainerId;
            if (!keyedOrientationGate ||
                !routeClearance ||
                !hostLineage ||
                !routed)
            {
                LogEps12vPowerCableSmokeFailure(
                    routeResult.IsFailure
                        ? routeResult.Error.Code
                        : "smoke.route-mismatch");
                yield break;
            }

            long assemblyRevisionBeforeGates = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeGates = session.Inventory.Revision;
            long cableRevisionBeforeGates =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int cableReceiptsBeforeGates =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;
            OperationResult<Eps12vPowerCableOperationReceipt> duplicateRoute =
                session.RouteEps12vPowerCable(
                    Eps12vPowerCableSmokeOperationId("duplicate-route"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.Eps12vPowerCableRevision);
            OperationResult<AssemblyOperationReceipt> blockedUnretain =
                session.UnretainPowerSupply(
                    Eps12vPowerCableSmokeOperationId("blocked-psu-unretain"),
                    powerSupplySeat,
                    powerSupplyRetain,
                    session.AssemblyBuild.Revision);
            OperationResult<AssemblyOperationReceipt> blockedUnsecure =
                session.UnsecureMotherboardFastener(
                    Eps12vPowerCableSmokeOperationId(
                        "blocked-board-unsecure"),
                    motherboardAttach,
                    motherboardSecure,
                    session.AssemblyBuild.Revision);
            OperationResult<AssemblyOperationReceipt> blockedProcessorOpen =
                session.OpenProcessorRetention(
                    Eps12vPowerCableSmokeOperationId(
                        "blocked-processor-open"),
                    processorSeat,
                    processorRetain,
                    session.AssemblyBuild.Revision);
            bool dependentGates = duplicateRoute.Error ==
                                      AssemblyFailures.PowerCableAlreadyRouted &&
                                  blockedUnretain.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  blockedUnsecure.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  blockedProcessorOpen.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  session.AssemblyBuild.Revision ==
                                      assemblyRevisionBeforeGates &&
                                  session.Inventory.Revision ==
                                      inventoryRevisionBeforeGates &&
                                  session.AssemblyBuild
                                      .Eps12vPowerCableRevision ==
                                      cableRevisionBeforeGates &&
                                  session.AssemblyBuild
                                      .Eps12vPowerCableReceiptCount ==
                                      cableReceiptsBeforeGates;

            OperationResult<Eps12vPowerCableOperationReceipt> routeReplay =
                session.RouteEps12vPowerCable(
                    routeOperationId,
                    PowerCableKeyOrientation.Keyed,
                    routeReceipt.ExpectedCableRevision);
            long cableRevisionBeforeUnroute =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            OperationResult unroutePickup =
                playerCarry.TryPickup(eps12vPowerCable);
            StableId<AssemblyOperationIdScope> unrouteOperationId =
                Eps12vPowerCablePrototypeOperationId("unroute", 2);
            bool hasUnrouteReceipt = session.AssemblyBuild
                .TryGetEps12vPowerCableReceipt(
                    unrouteOperationId,
                    out Eps12vPowerCableOperationReceipt unrouteReceipt);
            bool unrouted = unroutePickup.IsSuccess &&
                            playerCarry.HeldItem == eps12vPowerCable &&
                            !eps12vPowerCableBinding.IsRouted &&
                            !eps12vPowerCableGeometry.IsRouted &&
                            hasUnrouteReceipt &&
                            unrouteReceipt.SourceRouteOperationId ==
                                routeOperationId &&
                            session.AssemblyBuild.Eps12vPowerCableRevision ==
                                cableRevisionBeforeUnroute + 1;
            OperationResult<Eps12vPowerCableOperationReceipt> unrouteReplay =
                hasUnrouteReceipt
                    ? session.UnrouteEps12vPowerCable(
                        unrouteOperationId,
                        routeOperationId,
                        unrouteReceipt.ExpectedCableRevision)
                    : OperationResult<Eps12vPowerCableOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            bool replay = routeReplay.IsSuccess &&
                          unrouteReplay.IsSuccess &&
                          ReferenceEquals(routeReplay.Value, routeReceipt) &&
                          ReferenceEquals(unrouteReplay.Value, unrouteReceipt) &&
                          session.AssemblyBuild
                              .ValidateEps12vPowerCableReceiptHistory().IsSuccess;

            OperationResult recovery = unrouted
                ? playerCarry.TryRecoverHeldItem()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.unroute-mismatch"));
            bool identity = physicalIdentity ==
                                eps12vPowerCable.GetInstanceID() &&
                            stableItemId == eps12vPowerCable.ItemIdValue &&
                            stableItemId ==
                                eps12vPowerCableBinding.InventoryItemIdValue &&
                            CountCanonicalEps12vPowerCableProjections(
                                stableItemId) == 1;
            bool recovered = recovery.IsSuccess &&
                             !playerCarry.IsCarrying &&
                             eps12vPowerCable.transform.parent ==
                                 initialParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     eps12vPowerCable.transform.position,
                                     eps12vPowerCable.transform.rotation),
                                 initialPose) &&
                             eps12vPowerCable.Body == body &&
                             eps12vPowerCableBinding.IsAuthorityLooseWorld &&
                             eps12vPowerCableBinding
                                 .ValidateProjectionInvariant().IsSuccess &&
                             session.ValidateInvariants().IsSuccess;
            bool authorityIsolated =
                session.Orders.Revision == ordersRevisionBefore &&
                session.RetailOffers.Revision == offersRevisionBefore &&
                session.RetailBaskets.Revision == basketsRevisionBefore &&
                session.RetailCheckouts.Revision == checkoutsRevisionBefore &&
                session.CheckoutSettlements.Revision ==
                    settlementsRevisionBefore &&
                session.CustomerVisits.Revision == visitsRevisionBefore &&
                session.CustomerConsultations.Revision ==
                    consultationsRevisionBefore &&
                session.CustomerOfferActions.Revision ==
                    offerActionsRevisionBefore &&
                session.AssemblyBuild.Atx24PowerCableRevision ==
                    atx24RevisionBefore &&
                session.AssemblyBuild.Atx24PowerCableReceiptCount ==
                    atx24ReceiptCountBefore;

            if (!genericBypassBlocked ||
                !dependentGates ||
                !replay ||
                !hostLineage ||
                !authorityIsolated ||
                !identity ||
                !recovered)
            {
                LogEps12vPowerCableSmokeFailure(
                    "smoke.final-invariant-mismatch");
                yield break;
            }

            Debug.Log(Eps12vPowerCableSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToEps12vPowerCableRoute()
        {
            Vector3 target =
                eps12vPowerCableRoute.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.72f, 0.05f, 3.25f);
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));

            Camera playerCamera =
                playerMotor.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.transform.rotation = Quaternion.LookRotation(
                    target - playerCamera.transform.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private static StableId<AssemblyOperationIdScope>
            Eps12vPowerCableSmokeOperationId(string suffix)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.runtime-smoke.eps12v-power-cable-{suffix}");
        }

        private static StableId<AssemblyOperationIdScope>
            Eps12vPowerCablePrototypeOperationId(
                string action,
                long resultingRevision)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.eps12v-power-cable-{action}." +
                $"r{resultingRevision:000000}");
        }

        private static void LogEps12vPowerCableSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_EPS12V_POWER_CABLE_RUNTIME_SMOKE " +
                $"cable-flow=failed code={code}");
        }
    }
}
