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
        public const string Atx24PowerCableR30Marker =
            Atx24PowerCableRuntimeGeometry.RuntimeMarker;

        public const string Atx24PowerCableSmokeSuccessMarker =
            "GARAGE_POWER_CABLE_RUNTIME_SMOKE cable-flow=ok preflight=ok " +
            "psu-retained-gate=ok motherboard-secured-gate=ok endpoint-key=ok " +
            "route-waypoints=ok route-clearance=ok generic-bypass-blocked=ok " +
            "duplicate-route-blocked=ok dependent-detach-blocked=ok replay=ok " +
            "authority-isolated=ok identity=stable recovery=ok";

        [SerializeField] private Atx24PowerCableRouteProjection atx24PowerCableRoute;
        [SerializeField] private Atx24PowerCableAssemblyItemBinding atx24PowerCableBinding;
        [SerializeField] private PhysicalItemProjection atx24PowerCable;
        [SerializeField] private Atx24PowerCableRuntimeGeometry atx24PowerCableGeometry;

        public Atx24PowerCableRouteProjection Atx24PowerCableRoute =>
            atx24PowerCableRoute;

        public Atx24PowerCableAssemblyItemBinding Atx24PowerCableBinding =>
            atx24PowerCableBinding;

        public PhysicalItemProjection Atx24PowerCable => atx24PowerCable;

        public Atx24PowerCableRuntimeGeometry Atx24PowerCableGeometry =>
            atx24PowerCableGeometry;

        public bool HasAtx24PowerCableR30Runtime =>
            atx24PowerCableGeometry != null &&
            atx24PowerCableGeometry.IsCanonical &&
            atx24PowerCableRoute != null &&
            atx24PowerCableRoute.IsConfigured &&
            atx24PowerCableBinding != null &&
            atx24PowerCableBinding.Route == atx24PowerCableRoute &&
            atx24PowerCableBinding.PhysicalItem == atx24PowerCable &&
            atx24PowerCableBinding.Geometry == atx24PowerCableGeometry &&
            atx24PowerCable != null &&
            atx24PowerCableRoute.FocusCollider.isTrigger &&
            atx24PowerCableRoute.FocusCollider.gameObject.layer ==
                LayerMask.NameToLayer("Interactable") &&
            atx24PowerCableRoute.PreviewLines.Length == 3 &&
            CountCanonicalAtx24PowerCableProjections(
                GarageStockFlowSession.Atx24PowerCableItemInstanceIdValue) == 1 &&
            FindObjectsByType<Atx24PowerCableRuntimeGeometry>(
                FindObjectsSortMode.None).Length == 1;

        private void ConfigureAtx24PowerCable(
            Atx24PowerCableRouteProjection physicalRoute,
            Atx24PowerCableAssemblyItemBinding physicalBinding,
            PhysicalItemProjection physicalCable,
            Atx24PowerCableRuntimeGeometry physicalGeometry)
        {
            atx24PowerCableRoute = physicalRoute;
            atx24PowerCableBinding = physicalBinding;
            atx24PowerCable = physicalCable;
            atx24PowerCableGeometry = physicalGeometry;
        }

        private IEnumerator RunAtx24PowerCableSmoke()
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
                powerSupplyBinding == null ||
                atx24PowerCableRoute == null ||
                atx24PowerCableBinding == null ||
                atx24PowerCable == null ||
                atx24PowerCableGeometry == null)
            {
                LogAtx24PowerCableSmokeFailure("smoke.context-missing");
                yield break;
            }

            string stableItemId = atx24PowerCable.ItemIdValue;
            Transform initialParent = atx24PowerCable.transform.parent;
            Pose initialPose = new Pose(
                atx24PowerCable.transform.position,
                atx24PowerCable.transform.rotation);
            Rigidbody body = atx24PowerCable.Body;
            bool preflight = session.Inventory.SerializedItemCount == 8 &&
                             session.TryGetAtx24PowerCableItem(
                                 out InventoryItemRecord looseCable) &&
                             looseCable.Id == session.Atx24PowerCableItemId &&
                             looseCable.ProductId ==
                                 session.Atx24PowerCableProductId &&
                             looseCable.ContainerId ==
                                 session.WorldFloorContainerId &&
                             session.AssemblyBuild.Atx24PowerCableState ==
                                 Atx24PowerCableState.Loose &&
                             session.AssemblyBuild.Atx24PowerCableRevision == 0 &&
                             atx24PowerCableRoute.Waypoints.Length == 3 &&
                             atx24PowerCableGeometry.IsCanonical &&
                             atx24PowerCableBinding
                                 .ValidateProjectionInvariant().IsSuccess;
            Atx24PowerCableTopology topology =
                session.AssemblyBuild.Atx24PowerCableTopology;
            bool connectors = topology != null &&
                              topology.PsuPrimaryEndpoint.PinCount == 18 &&
                              topology.PsuSenseEndpoint.PinCount == 10 &&
                              topology.MotherboardEndpoint.PinCount == 24 &&
                              topology.OrderedWaypoints.Count == 3;
            if (!preflight || !connectors)
            {
                LogAtx24PowerCableSmokeFailure("smoke.preflight-mismatch");
                yield break;
            }

            StableId<AssemblyOperationIdScope> motherboardAttach =
                Atx24PowerCableSmokeOperationId("motherboard-attach");
            StableId<AssemblyOperationIdScope> motherboardSecure =
                Atx24PowerCableSmokeOperationId("motherboard-secure");
            StableId<AssemblyOperationIdScope> powerSupplySeat =
                Atx24PowerCableSmokeOperationId("power-supply-seat");
            StableId<AssemblyOperationIdScope> powerSupplyRetain =
                Atx24PowerCableSmokeOperationId("power-supply-retain");

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
            motherboardBinding.PhysicalItem.SynchronizeStableWorldPose(
                motherboardSeat.SnapPose);
            motherboardFastener.ApplyAuthoritativeState(
                AssemblySeatState.SeatedSecured);
            powerSupplyBinding.SyncProjectionToAuthority();
            Physics.SyncTransforms();
            bool hostsReady = secureMotherboard.IsSuccess &&
                              retainPowerSupply.IsSuccess &&
                              motherboardBinding.IsSecured &&
                              powerSupplyBinding.IsRetained &&
                              session.AssemblyBuild.MotherboardSeatState ==
                                  AssemblySeatState.SeatedSecured &&
                              session.AssemblyBuild.PowerSupplyBayState ==
                                  PowerSupplyBayState.PowerSupplyRetained;
            if (!hostsReady)
            {
                LogAtx24PowerCableSmokeFailure("smoke.host-setup-failed");
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

            MovePlayerToAtx24PowerCableRoute();
            OperationResult pickup = playerCarry.TryPickup(atx24PowerCable);
            long assemblyRevisionBeforeBypass = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeBypass = session.Inventory.Revision;
            long cableRevisionBeforeBypass =
                session.AssemblyBuild.Atx24PowerCableRevision;
            int cableReceiptsBeforeBypass =
                session.AssemblyBuild.Atx24PowerCableReceiptCount;
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
                                            atx24PowerCable &&
                                        session.AssemblyBuild.Revision ==
                                            assemblyRevisionBeforeBypass &&
                                        session.Inventory.Revision ==
                                            inventoryRevisionBeforeBypass &&
                                        session.AssemblyBuild
                                                .Atx24PowerCableRevision ==
                                            cableRevisionBeforeBypass &&
                                        session.AssemblyBuild
                                                .Atx24PowerCableReceiptCount ==
                                            cableReceiptsBeforeBypass;
            OperationResult mode = genericBypassBlocked
                ? playerCarry.TrySetAtx24PowerCableRouteMode(true)
                : OperationResult.Fail(
                    Failure.FromCode("smoke.generic-bypass-mismatch"));
            long cableRevisionBeforeWrong =
                session.AssemblyBuild.Atx24PowerCableRevision;
            long inventoryRevisionBeforeWrong = session.Inventory.Revision;
            OperationResult reverse = mode.IsSuccess
                ? playerCarry.TryRotateAtx24PowerCableConnectorPreview()
                : OperationResult.Fail(mode.Error);
            OperationResult wrongConfirm = reverse.IsSuccess
                ? playerCarry.TryConfirmAtx24PowerCableRoute()
                : OperationResult.Fail(reverse.Error);
            bool keyedOrientationGate = wrongConfirm.IsFailure &&
                                        playerCarry
                                                .CurrentAtx24PowerCableRouteStatus ==
                                            Atx24PowerCableRouteStatus
                                                .OrientationInvalid &&
                                        session.AssemblyBuild
                                                .Atx24PowerCableRevision ==
                                            cableRevisionBeforeWrong &&
                                        session.Inventory.Revision ==
                                            inventoryRevisionBeforeWrong &&
                                        playerCarry.HeldItem == atx24PowerCable;

            OperationResult restoreKey = playerCarry
                .TryRotateAtx24PowerCableConnectorPreview();
            OperationResult routeResult = restoreKey.IsSuccess
                ? playerCarry.TryConfirmAtx24PowerCableRoute()
                : OperationResult.Fail(restoreKey.Error);
            StableId<AssemblyOperationIdScope> routeOperationId =
                Atx24PowerCablePrototypeOperationId("route", 1);
            bool hasRouteReceipt = session.AssemblyBuild
                .TryGetAtx24PowerCableReceipt(
                    routeOperationId,
                    out Atx24PowerCableOperationReceipt routeReceipt);
            bool routed = routeResult.IsSuccess &&
                          !playerCarry.IsCarrying &&
                          atx24PowerCableBinding.IsRouted &&
                          atx24PowerCableGeometry.IsRouted &&
                          hasRouteReceipt &&
                          routeReceipt.RouteFingerprint == topology.Fingerprint &&
                          session.TryGetAtx24PowerCableItem(
                              out InventoryItemRecord routedCable) &&
                          routedCable.ContainerId ==
                              session.Atx24PowerCableRouteContainerId;
            if (!keyedOrientationGate || !routed)
            {
                LogAtx24PowerCableSmokeFailure(
                    routeResult.IsFailure
                        ? routeResult.Error.Code
                        : "smoke.route-mismatch");
                yield break;
            }

            long assemblyRevisionBeforeGates = session.AssemblyBuild.Revision;
            long inventoryRevisionBeforeGates = session.Inventory.Revision;
            OperationResult<Atx24PowerCableOperationReceipt> duplicateRoute =
                session.RouteAtx24PowerCable(
                    Atx24PowerCableSmokeOperationId("duplicate-route"),
                    PowerCableKeyOrientation.Keyed,
                    session.AssemblyBuild.Atx24PowerCableRevision);
            OperationResult<AssemblyOperationReceipt> blockedUnretain =
                session.UnretainPowerSupply(
                    Atx24PowerCableSmokeOperationId("blocked-psu-unretain"),
                    powerSupplySeat,
                    powerSupplyRetain,
                    session.AssemblyBuild.Revision);
            OperationResult<AssemblyOperationReceipt> blockedUnsecure =
                session.UnsecureMotherboardFastener(
                    Atx24PowerCableSmokeOperationId("blocked-board-unsecure"),
                    motherboardAttach,
                    motherboardSecure,
                    session.AssemblyBuild.Revision);
            bool dependentGates = duplicateRoute.Error ==
                                      AssemblyFailures.PowerCableAlreadyRouted &&
                                  blockedUnretain.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  blockedUnsecure.Error ==
                                      AssemblyFailures
                                          .PowerCableDependentComponentLocked &&
                                  session.AssemblyBuild.Revision ==
                                      assemblyRevisionBeforeGates &&
                                  session.Inventory.Revision ==
                                      inventoryRevisionBeforeGates;

            OperationResult<Atx24PowerCableOperationReceipt> routeReplay =
                session.RouteAtx24PowerCable(
                    routeOperationId,
                    PowerCableKeyOrientation.Keyed,
                    routeReceipt.ExpectedCableRevision);
            long cableRevisionBeforeUnroute =
                session.AssemblyBuild.Atx24PowerCableRevision;
            OperationResult unroutePickup = playerCarry.TryPickup(atx24PowerCable);
            StableId<AssemblyOperationIdScope> unrouteOperationId =
                Atx24PowerCablePrototypeOperationId("unroute", 2);
            bool hasUnrouteReceipt = session.AssemblyBuild
                .TryGetAtx24PowerCableReceipt(
                    unrouteOperationId,
                    out Atx24PowerCableOperationReceipt unrouteReceipt);
            bool unrouted = unroutePickup.IsSuccess &&
                            playerCarry.HeldItem == atx24PowerCable &&
                            !atx24PowerCableBinding.IsRouted &&
                            !atx24PowerCableGeometry.IsRouted &&
                            hasUnrouteReceipt &&
                            unrouteReceipt.SourceRouteOperationId ==
                                routeOperationId &&
                            session.AssemblyBuild.Atx24PowerCableRevision ==
                                cableRevisionBeforeUnroute + 1;
            OperationResult<Atx24PowerCableOperationReceipt> unrouteReplay =
                hasUnrouteReceipt
                    ? session.UnrouteAtx24PowerCable(
                        unrouteOperationId,
                        routeOperationId,
                        unrouteReceipt.ExpectedCableRevision)
                    : OperationResult<Atx24PowerCableOperationReceipt>.Fail(
                        Failure.FromCode("smoke.receipt-missing"));
            bool replay = routeReplay.IsSuccess &&
                          unrouteReplay.IsSuccess &&
                          ReferenceEquals(routeReplay.Value, routeReceipt) &&
                          ReferenceEquals(unrouteReplay.Value, unrouteReceipt) &&
                          session.AssemblyBuild
                              .ValidateAtx24PowerCableReceiptHistory().IsSuccess;

            OperationResult recovery = unrouted
                ? playerCarry.TryRecoverHeldItem()
                : OperationResult.Fail(
                    Failure.FromCode("smoke.unroute-mismatch"));
            bool identity = stableItemId == atx24PowerCable.ItemIdValue &&
                            stableItemId ==
                                atx24PowerCableBinding.InventoryItemIdValue &&
                            CountCanonicalAtx24PowerCableProjections(stableItemId) == 1;
            bool recovered = recovery.IsSuccess &&
                             !playerCarry.IsCarrying &&
                             atx24PowerCable.transform.parent == initialParent &&
                             ApproximatelySamePose(
                                 new Pose(
                                     atx24PowerCable.transform.position,
                                     atx24PowerCable.transform.rotation),
                                 initialPose) &&
                             atx24PowerCable.Body == body &&
                             atx24PowerCableBinding.IsAuthorityLooseWorld &&
                             atx24PowerCableBinding
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
                    offerActionsRevisionBefore;

            if (!genericBypassBlocked ||
                !dependentGates ||
                !replay ||
                !authorityIsolated ||
                !identity ||
                !recovered)
            {
                LogAtx24PowerCableSmokeFailure(
                    "smoke.final-invariant-mismatch");
                yield break;
            }

            Debug.Log(Atx24PowerCableSmokeSuccessMarker);
            yield return new WaitForEndOfFrame();
        }

        private void MovePlayerToAtx24PowerCableRoute()
        {
            Vector3 target = atx24PowerCableRoute.FocusCollider.bounds.center;
            Vector3 playerPosition = new Vector3(-0.72f, 0.05f, 3.25f);
            Vector3 horizontalLook = target - playerPosition;
            horizontalLook.y = 0f;
            SetPlayerPose(
                playerPosition,
                Quaternion.LookRotation(horizontalLook.normalized, Vector3.up));

            Camera playerCamera = playerMotor.GetComponentInChildren<Camera>(true);
            if (playerCamera != null)
            {
                playerCamera.transform.rotation = Quaternion.LookRotation(
                    target - playerCamera.transform.position,
                    Vector3.up);
            }

            Physics.SyncTransforms();
        }

        private static int CountCanonicalAtx24PowerCableProjections(
            string canonicalItemId)
        {
            int count = 0;
            foreach (PhysicalItemProjection item in
                     FindObjectsByType<PhysicalItemProjection>(
                         FindObjectsInactive.Include,
                         FindObjectsSortMode.None))
            {
                if (item != null && item.ItemIdValue == canonicalItemId)
                {
                    count++;
                }
            }

            return count;
        }

        private static StableId<AssemblyOperationIdScope>
            Atx24PowerCableSmokeOperationId(string suffix)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.runtime-smoke.atx24-power-cable-{suffix}");
        }

        private static StableId<AssemblyOperationIdScope>
            Atx24PowerCablePrototypeOperationId(
                string action,
                long resultingRevision)
        {
            return StableId<AssemblyOperationIdScope>.Parse(
                $"assembly.operation.prototype-001.atx24-power-cable-{action}." +
                $"r{resultingRevision:000000}");
        }

        private static void LogAtx24PowerCableSmokeFailure(string code)
        {
            Debug.LogError(
                $"GARAGE_POWER_CABLE_RUNTIME_SMOKE cable-flow=failed code={code}");
        }
    }
}
