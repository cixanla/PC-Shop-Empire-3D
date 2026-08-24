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
        public const string PcieGpuPowerCableSmokeSuccessMarker =
            "GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE cable-flow=ok " +
            "preflight=ok psu-retained-gate=ok motherboard-secured-gate=ok " +
            "gpu-retained-gate=ok endpoint-key=ok route-waypoints=ok " +
            "route-clearance=ok generic-bypass-blocked=ok " +
            "duplicate-route-blocked=ok dependent-detach-blocked=ok replay=ok " +
            "authority-isolated=ok identity=stable recovery=ok";

        private IEnumerator RunPcieGpuPowerCableSmoke()
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
                graphicsCardSlot == null ||
                graphicsCardBinding == null ||
                graphicsCard == null ||
                pcieGpuPowerCableRoute == null ||
                pcieGpuPowerCableBinding == null ||
                pcieGpuPowerCable == null ||
                pcieGpuPowerCableGeometry == null)
            {
                LogPcieGpuPowerCableSmokeFailure("smoke.context-missing");
                yield break;
            }

            int physicalIdentity = pcieGpuPowerCable.GetInstanceID();
            string stableItemId = pcieGpuPowerCable.ItemIdValue;
            Transform initialParent = pcieGpuPowerCable.transform.parent;
            Pose initialPose = new Pose(
                pcieGpuPowerCable.transform.position,
                pcieGpuPowerCable.transform.rotation);
            Rigidbody body = pcieGpuPowerCable.Body;
            bool preflight = session.Inventory.SerializedItemCount == 10 &&
                             session.TryGetPcieGpuPowerCableItem(
                                 out InventoryItemRecord looseCable) &&
                             looseCable.Id == session.PcieGpuPowerCableItemId &&
                             looseCable.ProductId ==
                                 session.PcieGpuPowerCableProductId &&
                             looseCable.ContainerId ==
                                 session.WorldFloorContainerId &&
                             session.AssemblyBuild.PcieGpuPowerCableState ==
                                 PcieGpuPowerCableState.Loose &&
                             session.AssemblyBuild.PcieGpuPowerCableRevision == 0 &&
                             pcieGpuPowerCableRoute.Waypoints.Length == 3 &&
                             pcieGpuPowerCableGeometry.IsCanonical &&
                             pcieGpuPowerCableBinding
                                 .ValidateProjectionInvariant().IsSuccess;
            PcieGpuPowerCableTopology topology =
                session.AssemblyBuild.PcieGpuPowerCableTopology;
            bool connectors = topology != null &&
                              topology.PsuEndpoint.PinCount == 8 &&
                              topology.GraphicsCardEndpoint.PinCount == 8 &&
                              topology.OrderedWaypoints.Count == 3;
            if (!preflight || !connectors)
            {
                LogPcieGpuPowerCableSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            StableId<AssemblyOperationIdScope> motherboardAttach =
                PcieGpuPowerCableSmokeOperationId("motherboard-attach");
            StableId<AssemblyOperationIdScope> motherboardSecure =
                PcieGpuPowerCableSmokeOperationId("motherboard-secure");
            StableId<AssemblyOperationIdScope> powerSupplySeat =
                PcieGpuPowerCableSmokeOperationId("power-supply-seat");
            StableId<AssemblyOperationIdScope> powerSupplyRetain =
                PcieGpuPowerCableSmokeOperationId("power-supply-retain");
            StableId<AssemblyOperationIdScope> graphicsCardSeat =
                PcieGpuPowerCableSmokeOperationId("graphics-card-seat");
            StableId<AssemblyOperationIdScope> graphicsCardRetain =
                PcieGpuPowerCableSmokeOperationId("graphics-card-retain");

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
            OperationResult pickupGraphicsCard =
                session.PickupLooseGraphicsCardToHands();
            OperationResult<AssemblyOperationReceipt> seatGraphicsCard =
                pickupGraphicsCard.IsSuccess
                    ? session.SeatGraphicsCard(
                        graphicsCardSeat,
                        GraphicsCardMountOrientation.Primary,
                        motherboardAttach,
                        motherboardSecure,
                        session.AssemblyBuild.Revision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        pickupGraphicsCard.Error);
            OperationResult<AssemblyOperationReceipt> retainGraphicsCard =
                seatGraphicsCard.IsSuccess
                    ? session.RetainGraphicsCard(
                        graphicsCardRetain,
                        graphicsCardSeat,
                        session.AssemblyBuild.Revision)
                    : OperationResult<AssemblyOperationReceipt>.Fail(
                        seatGraphicsCard.Error);

            OperationResult syncMotherboard =
                motherboardBinding.PhysicalItem.SynchronizeStableWorldPose(
                    motherboardSeat.SnapPose);
            motherboardFastener.ApplyAuthoritativeState(
                AssemblySeatState.SeatedSecured);
            OperationResult syncPowerSupply =
                powerSupplyBinding.SyncProjectionToAuthority();
            OperationResult<Pose> resolvedGraphicsCardPose =
                graphicsCardSlot.ResolveSeatPose(0);
            OperationResult syncGraphicsCardPose = resolvedGraphicsCardPose.IsSuccess
                ? graphicsCard.SynchronizeStableWorldPose(
                    resolvedGraphicsCardPose.Value)
                : OperationResult.Fail(resolvedGraphicsCardPose.Error);
            OperationResult syncGraphicsCard =
                graphicsCardBinding.SyncProjectionToAuthority();
            Physics.SyncTransforms();
            bool hostsReady = secureMotherboard.IsSuccess &&
                              retainPowerSupply.IsSuccess &&
                              retainGraphicsCard.IsSuccess &&
                              syncMotherboard.IsSuccess &&
                              syncPowerSupply.IsSuccess &&
                              syncGraphicsCardPose.IsSuccess &&
                              syncGraphicsCard.IsSuccess &&
                              motherboardBinding.IsSecured &&
                              powerSupplyBinding.IsRetained &&
                              graphicsCardBinding.IsRetained &&
                              session.AssemblyBuild.MotherboardSeatState ==
                                  AssemblySeatState.SeatedSecured &&
                              session.AssemblyBuild.PowerSupplyBayState ==
                                  PowerSupplyBayState.PowerSupplyRetained &&
                              session.AssemblyBuild.GraphicsCardSlotState ==
                                  GraphicsCardSlotState.GraphicsCardRetained;
            if (!hostsReady)
            {
                LogPcieGpuPowerCableSmokeFailure("smoke.host-setup-failed");
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
            long eps12vRevisionBefore =
                session.AssemblyBuild.Eps12vPowerCableRevision;
            int eps12vReceiptCountBefore =
                session.AssemblyBuild.Eps12vPowerCableReceiptCount;

            MovePlayerToPcieGpuPowerCableRoute();
            OperationResult pickup = playerCarry.TryPickup(pcieGpuPowerCable);
            long assemblyRevisionBeforeBypass = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeBypass = session.Inventory.Revision;
            long cableRevisionBeforeBypass =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int cableReceiptsBeforeBypass =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
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
                                            pcieGpuPowerCable &&
                                        session.AssemblyBuild.Revision ==
                                            assemblyRevisionBeforeBypass &&
                                        session.Inventory.Revision ==
                                            inventoryRevisionBeforeBypass &&
                                        session.AssemblyBuild
                                                .PcieGpuPowerCableRevision ==
                                            cableRevisionBeforeBypass &&
                                        session.AssemblyBuild
                                                .PcieGpuPowerCableReceiptCount ==
                                            cableReceiptsBeforeBypass;
            OperationResult mode = genericBypassBlocked
                ? playerCarry.TrySetPcieGpuPowerCableRouteMode(true)
                : OperationResult.Fail(
                    Failure.FromCode("smoke.generic-bypass-mismatch"));
            long cableRevisionBeforeWrong =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            long inventoryRevisionBeforeWrong = session.Inventory.Revision;
            OperationResult reverse = mode.IsSuccess
                ? playerCarry.TryRotatePcieGpuPowerCableConnectorPreview()
                : OperationResult.Fail(mode.Error);
            OperationResult wrongConfirm = reverse.IsSuccess
                ? playerCarry.TryConfirmPcieGpuPowerCableRoute()
                : OperationResult.Fail(reverse.Error);
            bool keyedOrientationGate = wrongConfirm.IsFailure &&
                                        playerCarry
                                                .CurrentPcieGpuPowerCableRouteStatus ==
                                            PcieGpuPowerCableRouteStatus
                                                .OrientationInvalid &&
                                        session.AssemblyBuild
                                                .PcieGpuPowerCableRevision ==
                                            cableRevisionBeforeWrong &&
                                        session.Inventory.Revision ==
                                            inventoryRevisionBeforeWrong &&
                                        playerCarry.HeldItem ==
                                            pcieGpuPowerCable;

            OperationResult restoreKey = playerCarry
                .TryRotatePcieGpuPowerCableConnectorPreview();
            bool routeClearance = restoreKey.IsSuccess &&
                                  playerCarry.PlacementValid &&
                                  playerCarry
                                          .CurrentPcieGpuPowerCableRouteStatus ==
                                      PcieGpuPowerCableRouteStatus.ValidRoute;
            OperationResult routeResult = routeClearance
                ? playerCarry.TryConfirmPcieGpuPowerCableRoute()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.route-clearance-mismatch"));
            StableId<AssemblyOperationIdScope> routeOperationId =
                PcieGpuPowerCablePrototypeOperationId("route", 1);
            bool hasRouteReceipt = session.AssemblyBuild
                .TryGetPcieGpuPowerCableReceipt(
                    routeOperationId,
                    out PcieGpuPowerCableOperationReceipt routeReceipt);
            bool hostLineage = hasRouteReceipt &&
                               routeReceipt
                                       .SourceMotherboardSecureOperationId ==
                                   motherboardSecure &&
                               routeReceipt
                                       .SourcePowerSupplyRetentionOperationId ==
                                   powerSupplyRetain &&
                               routeReceipt
                                       .SourceGraphicsCardRetentionOperationId ==
                                   graphicsCardRetain;
            bool routed = routeResult.IsSuccess &&
                          !playerCarry.IsCarrying &&
                          pcieGpuPowerCableBinding.IsRouted &&
                          pcieGpuPowerCableGeometry.IsRouted &&
                          hasRouteReceipt &&
                          routeReceipt.RouteFingerprint == topology.Fingerprint &&
                          session.TryGetPcieGpuPowerCableItem(
                              out InventoryItemRecord routedCable) &&
                          routedCable.ContainerId ==
                              session.PcieGpuPowerCableRouteContainerId;
            if (!keyedOrientationGate ||
                !routeClearance ||
                !hostLineage ||
                !routed)
            {
                LogPcieGpuPowerCableSmokeFailure(
                    routeResult.IsFailure
                        ? routeResult.Error.Code
                        : "smoke.route-mismatch");
                yield break;
            }

            long assemblyRevisionBeforeGates = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeGates = session.Inventory.Revision;
            long cableRevisionBeforeGates =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            int cableReceiptsBeforeGates =
                session.AssemblyBuild.PcieGpuPowerCableReceiptCount;
            OperationResult<PcieGpuPowerCableOperationReceipt> duplicateRoute =
                session.RoutePcieGpuPowerCable(
                    PcieGpuPowerCableSmokeOperationId("duplicate-route"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.PcieGpuPowerCableRevision);
            OperationResult<AssemblyOperationReceipt> blockedUnretain =
                session.UnretainPowerSupply(
                    PcieGpuPowerCableSmokeOperationId("blocked-psu-unretain"),
                    powerSupplySeat,
                    powerSupplyRetain,
                    session.AssemblyBuild.Revision);
            OperationResult<AssemblyOperationReceipt> blockedUnsecure =
                session.UnsecureMotherboardFastener(
                    PcieGpuPowerCableSmokeOperationId(
                        "blocked-board-unsecure"),
                    motherboardAttach,
                    motherboardSecure,
                    session.AssemblyBuild.Revision);
            OperationResult<AssemblyOperationReceipt> blockedGraphicsCardOpen =
                session.UnretainGraphicsCard(
                    PcieGpuPowerCableSmokeOperationId(
                        "blocked-graphics-card-open"),
                    graphicsCardSeat,
                    graphicsCardRetain,
                    session.AssemblyBuild.Revision);
            bool dependentGates = duplicateRoute.Error ==
                                      AssemblyFailures.PowerCableAlreadyRouted &&
                                  blockedUnretain.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  blockedUnsecure.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  blockedGraphicsCardOpen.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  session.AssemblyBuild.Revision ==
                                      assemblyRevisionBeforeGates &&
                                  session.Inventory.Revision ==
                                      inventoryRevisionBeforeGates &&
                                  session.AssemblyBuild
                                      .PcieGpuPowerCableRevision ==
                                      cableRevisionBeforeGates &&
                                  session.AssemblyBuild
                                      .PcieGpuPowerCableReceiptCount ==
                                      cableReceiptsBeforeGates;

            OperationResult<PcieGpuPowerCableOperationReceipt> routeReplay =
                session.RoutePcieGpuPowerCable(
                    routeOperationId,
                    PowerCableKeyOrientation.Keyed,
                    routeReceipt.ExpectedCableRevision);
            long cableRevisionBeforeUnroute =
                session.AssemblyBuild.PcieGpuPowerCableRevision;
            OperationResult unroutePickup =
                playerCarry.TryPickup(pcieGpuPowerCable);
            StableId<AssemblyOperationIdScope> unrouteOperationId =
                PcieGpuPowerCablePrototypeOperationId("unroute", 2);
            bool hasUnrouteReceipt = session.AssemblyBuild
                .TryGetPcieGpuPowerCableReceipt(
                    unrouteOperationId,
                    out PcieGpuPowerCableOperationReceipt unrouteReceipt);
            bool unrouted = unroutePickup.IsSuccess &&
                            playerCarry.HeldItem == pcieGpuPowerCable &&
                            !pcieGpuPowerCableBinding.IsRouted &&
                            !pcieGpuPowerCableGeometry.IsRouted &&
                            hasUnrouteReceipt &&
                            unrouteReceipt.SourceRouteOperationId ==
                                routeOperationId &&
                            session.AssemblyBuild.PcieGpuPowerCableRevision ==
                                cableRevisionBeforeUnroute + 1;
            OperationResult<PcieGpuPowerCableOperationReceipt> unrouteReplay =
                hasUnrouteReceipt
                    ? session.UnroutePcieGpuPowerCable(
                        unrouteOperationId,
                        routeOperationId,
                        unrouteReceipt.ExpectedCableRevision)
                    : OperationResult<PcieGpuPowerCableOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            bool replay = routeReplay.IsSuccess &&
                          unrouteReplay.IsSuccess &&
                          ReferenceEquals(routeReplay.Value, routeReceipt) &&
                          ReferenceEquals(unrouteReplay.Value, unrouteReceipt) &&
                          session.AssemblyBuild
                              .ValidatePcieGpuPowerCableReceiptHistory().IsSuccess;

            OperationResult recovery = unrouted
                ? playerCarry.TryRecoverHeldItem()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.unroute-mismatch"));
            bool identity = physicalIdentity ==
                                pcieGpuPowerCable.GetInstanceID() &&
                            stableItemId == pcieGpuPowerCable.ItemIdValue &&
                            stableItemId ==
                                pcieGpuPowerCableBinding.InventoryItemIdValue &&
                            CountCanonicalPcieGpuPowerCableProjections(
                                stableItemId) == 1;
            bool recovered = recovery.IsSuccess &&
                             !playerCarry.IsCarrying &&
                             pcieGpuPowerCable.transform.parent ==
                                 initialParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     pcieGpuPowerCable.transform.position,
                                     pcieGpuPowerCable.transform.rotation),
                                 initialPose) &&
                             pcieGpuPowerCable.Body == body &&
                             pcieGpuPowerCableBinding.IsAuthorityLooseWorld &&
                             pcieGpuPowerCableBinding
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
                    atx24ReceiptCountBefore &&
                session.AssemblyBuild.Eps12vPowerCableRevision ==
                    eps12vRevisionBefore &&
                session.AssemblyBuild.Eps12vPowerCableReceiptCount ==
                    eps12vReceiptCountBefore;

            if (!genericBypassBlocked ||
                !dependentGates ||
                !replay ||
                !hostLineage ||
                !authorityIsolated ||
                !identity ||
                !recovered)
            {
                LogPcieGpuPowerCableSmokeFailure(
                    "smoke.final-invariant-mismatch");
                yield break;
            }

            Debug.Log(PcieGpuPowerCableSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToPcieGpuPowerCableRoute()
        {
            Vector3 target =
                pcieGpuPowerCableRoute.FocusCollider.bounds.center;
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
            PcieGpuPowerCableSmokeOperationId(string suffix)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.runtime-smoke.pcie-gpu-power-cable-{suffix}");
        }

        private static StableId<AssemblyOperationIdScope>
            PcieGpuPowerCablePrototypeOperationId(
                string action,
                long resultingRevision)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.pcie-gpu-power-cable-{action}." +
                $"r{resultingRevision:000000}");
        }

        private static void LogPcieGpuPowerCableSmokeFailure(string code)
        {
            Debug.LogError(
                "GARAGE_PCIE_GPU_POWER_CABLE_RUNTIME_SMOKE " +
                $"cable-flow=failed code={code}");
        }
    }
}
